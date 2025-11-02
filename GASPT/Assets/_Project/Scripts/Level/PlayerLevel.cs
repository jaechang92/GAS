using System;
using UnityEngine;
using GASPT.Stats;

namespace GASPT.Level
{
    /// <summary>
    /// 플레이어 레벨 및 경험치 시스템 싱글톤
    /// 레벨업 시 스탯 증가 처리
    /// </summary>
    public class PlayerLevel : SingletonManager<PlayerLevel>
    {
        // ====== 레벨 설정 ======

        [Header("Level Settings")]
        [SerializeField] [Tooltip("시작 레벨")]
        private int startingLevel = 1;

        [SerializeField] [Tooltip("최대 레벨")]
        private int maxLevel = 99;

        [SerializeField] [Tooltip("레벨당 필요 EXP 계수 (Level × baseExpPerLevel)")]
        private int baseExpPerLevel = 100;


        // ====== 레벨업 보상 ======

        [Header("Level Up Rewards")]
        [SerializeField] [Tooltip("레벨업 시 HP 증가량")]
        private int hpPerLevel = 10;

        [SerializeField] [Tooltip("레벨업 시 공격력 증가량")]
        private int attackPerLevel = 2;

        [SerializeField] [Tooltip("레벨업 시 방어력 증가량")]
        private int defensePerLevel = 1;


        // ====== 현재 상태 ======

        private int currentLevel;
        private int currentExp;


        // ====== PlayerStats 참조 ======

        private PlayerStats playerStats;


        // ====== 이벤트 ======

        /// <summary>
        /// 레벨업 시 발생하는 이벤트
        /// 매개변수: (새 레벨)
        /// </summary>
        public event Action<int> OnLevelUp;

        /// <summary>
        /// 경험치 획득 시 발생하는 이벤트
        /// 매개변수: (획득 EXP, 현재 EXP, 필요 EXP)
        /// </summary>
        public event Action<int, int, int> OnExpGained;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 레벨
        /// </summary>
        public int Level => currentLevel;

        /// <summary>
        /// 현재 경험치
        /// </summary>
        public int CurrentExp => currentExp;

        /// <summary>
        /// 다음 레벨까지 필요한 경험치
        /// </summary>
        public int RequiredExp => CalculateRequiredExp(currentLevel);

        /// <summary>
        /// 최대 레벨 여부
        /// </summary>
        public bool IsMaxLevel => currentLevel >= maxLevel;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            currentLevel = startingLevel;
            currentExp = 0;

            // PlayerStats 찾기
            playerStats = FindAnyObjectByType<PlayerStats>();

            if (playerStats == null)
            {
                Debug.LogWarning("[PlayerLevel] PlayerStats를 찾을 수 없습니다. 레벨업 스탯 증가가 작동하지 않습니다.");
            }

            Debug.Log($"[PlayerLevel] 초기화 완료 - Level: {currentLevel}, EXP: {currentExp}/{RequiredExp}");
        }


        // ====== 경험치 관리 ======

        /// <summary>
        /// 경험치 획득
        /// </summary>
        /// <param name="exp">획득할 경험치</param>
        public void AddExp(int exp)
        {
            if (exp <= 0)
            {
                Debug.LogWarning($"[PlayerLevel] AddExp(): exp가 0 이하입니다: {exp}");
                return;
            }

            if (IsMaxLevel)
            {
                Debug.Log($"[PlayerLevel] 최대 레벨에 도달했습니다. EXP를 획득할 수 없습니다.");
                return;
            }

            currentExp += exp;

            Debug.Log($"[PlayerLevel] EXP 획득: +{exp} (현재: {currentExp}/{RequiredExp})");

            // 이벤트 발생
            OnExpGained?.Invoke(exp, currentExp, RequiredExp);

            // 레벨업 체크
            CheckLevelUp();
        }

        /// <summary>
        /// 레벨업 가능 여부 확인 및 자동 레벨업
        /// </summary>
        private void CheckLevelUp()
        {
            while (currentExp >= RequiredExp && !IsMaxLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// 레벨업 처리
        /// </summary>
        private void LevelUp()
        {
            // EXP 차감
            currentExp -= RequiredExp;

            // 레벨 증가
            int oldLevel = currentLevel;
            currentLevel++;

            Debug.Log($"[PlayerLevel] 레벨 업! {oldLevel} → {currentLevel}");

            // 스탯 증가 적용
            ApplyLevelUpStats();

            // 이벤트 발생
            OnLevelUp?.Invoke(currentLevel);

            // 최대 레벨 도달 체크
            if (IsMaxLevel)
            {
                currentExp = 0; // 최대 레벨에서는 EXP 0으로 설정
                Debug.Log($"[PlayerLevel] 최대 레벨 {maxLevel}에 도달했습니다!");
            }
        }

        /// <summary>
        /// 레벨업 스탯 증가 적용
        /// </summary>
        private void ApplyLevelUpStats()
        {
            if (playerStats == null)
            {
                Debug.LogWarning("[PlayerLevel] PlayerStats가 없어서 스탯을 증가시킬 수 없습니다.");
                return;
            }

            // PlayerStats의 기본 스탯 증가
            // Reflection으로 private 필드 접근
            var baseHPField = typeof(PlayerStats).GetField("baseHP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseAttackField = typeof(PlayerStats).GetField("baseAttack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseDefenseField = typeof(PlayerStats).GetField("baseDefense", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (baseHPField != null)
            {
                int currentHP = (int)baseHPField.GetValue(playerStats);
                baseHPField.SetValue(playerStats, currentHP + hpPerLevel);
            }

            if (baseAttackField != null)
            {
                int currentAttack = (int)baseAttackField.GetValue(playerStats);
                baseAttackField.SetValue(playerStats, currentAttack + attackPerLevel);
            }

            if (baseDefenseField != null)
            {
                int currentDefense = (int)baseDefenseField.GetValue(playerStats);
                baseDefenseField.SetValue(playerStats, currentDefense + defensePerLevel);
            }

            // 스탯 재계산 강제 실행 (Dirty Flag 설정)
            var isDirtyField = typeof(PlayerStats).GetField("isDirty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (isDirtyField != null)
            {
                isDirtyField.SetValue(playerStats, true);
            }

            // 현재 HP를 MaxHP로 회복 (레벨업 보너스)
            playerStats.Revive();

            Debug.Log($"[PlayerLevel] 스탯 증가 적용: HP +{hpPerLevel}, Attack +{attackPerLevel}, Defense +{defensePerLevel}");
        }


        // ====== EXP 계산 ======

        /// <summary>
        /// 특정 레벨에서 다음 레벨까지 필요한 EXP 계산
        /// 공식: Level × baseExpPerLevel
        /// </summary>
        private int CalculateRequiredExp(int level)
        {
            if (level >= maxLevel)
            {
                return 0; // 최대 레벨에서는 필요 EXP 없음
            }

            return level * baseExpPerLevel;
        }


        // ====== 디버그 ======

        /// <summary>
        /// 현재 레벨 정보 출력
        /// </summary>
        [ContextMenu("Print Level Info")]
        public void DebugPrintLevelInfo()
        {
            Debug.Log("========== PlayerLevel Info ==========");
            Debug.Log($"Level: {currentLevel} / {maxLevel}");
            Debug.Log($"EXP: {currentExp} / {RequiredExp}");
            Debug.Log($"Is Max Level: {IsMaxLevel}");
            Debug.Log($"Level Up Rewards: HP +{hpPerLevel}, Attack +{attackPerLevel}, Defense +{defensePerLevel}");
            Debug.Log("======================================");
        }

        /// <summary>
        /// 테스트용 EXP 추가 (Context Menu)
        /// </summary>
        [ContextMenu("Add 50 EXP (Test)")]
        private void DebugAddExp()
        {
            AddExp(50);
        }

        /// <summary>
        /// 테스트용 레벨업 (Context Menu)
        /// </summary>
        [ContextMenu("Level Up (Test)")]
        private void DebugLevelUp()
        {
            int expNeeded = RequiredExp - currentExp;
            AddExp(expNeeded);
        }
    }
}
