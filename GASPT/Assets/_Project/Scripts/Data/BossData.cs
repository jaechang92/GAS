using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Loot;
using GASPT.Gameplay.Form;

namespace GASPT.Data
{
    /// <summary>
    /// 보스 데이터 ScriptableObject
    /// 보스의 기본 정보, 스탯, 페이즈, 패턴, 보상을 정의
    /// </summary>
    [CreateAssetMenu(fileName = "BossData", menuName = "GASPT/Bosses/Boss Data")]
    public class BossData : ScriptableObject
    {
        // ====== 기본 정보 ======

        [Header("기본 정보")]
        [Tooltip("보스 고유 ID (B001, B002 등)")]
        public string bossId = "B001";

        [Tooltip("보스 이름 (UI 표시용)")]
        public string bossName = "보스";

        [Tooltip("보스 설명 (도감용)")]
        [TextArea(2, 4)]
        public string description = "";

        [Tooltip("보스 등급")]
        public BossGrade bossGrade = BossGrade.MiniBoss;

        [Tooltip("보스 속성")]
        public ElementType elementType = ElementType.None;

        [Tooltip("보스 아이콘 (UI 표시용)")]
        public Sprite icon;


        // ====== 스탯 ======

        [Header("스탯")]
        [Tooltip("최대 체력")]
        [Range(100, 10000)]
        public int maxHealth = 500;

        [Tooltip("기본 공격력")]
        [Range(5, 200)]
        public int baseAttack = 15;

        [Tooltip("방어력")]
        [Range(0, 100)]
        public int defense = 5;

        [Tooltip("이동 속도")]
        [Range(1f, 10f)]
        public float moveSpeed = 3f;


        // ====== 페이즈 설정 ======

        [Header("페이즈 설정")]
        [Tooltip("페이즈 목록 (페이즈 수만큼 설정)")]
        public BossPhaseData[] phases;


        // ====== 전투 설정 ======

        [Header("전투 설정")]
        [Tooltip("플레이어 감지 거리")]
        [Range(5f, 30f)]
        public float detectionRange = 15f;

        [Tooltip("공격 범위")]
        [Range(1f, 10f)]
        public float attackRange = 3f;

        [Tooltip("기본 공격 쿨다운")]
        [Range(1f, 5f)]
        public float attackCooldown = 2f;

        [Tooltip("제한 시간 (0 = 무제한)")]
        [Range(0, 600)]
        public float timeLimit = 120f;


        // ====== 보상 ======

        [Header("기본 보상")]
        [Tooltip("골드 보상")]
        [Range(0, 5000)]
        public int goldReward = 100;

        [Tooltip("경험치 보상")]
        [Range(0, 2000)]
        public int expReward = 50;

        [Tooltip("아이템 드롭 테이블")]
        public LootTable lootTable;


        // ====== 메타 재화 보상 ======

        [Header("메타 재화 보상")]
        [Tooltip("Bone 드롭량")]
        [Range(0, 100)]
        public int boneDrop = 10;

        [Tooltip("Soul 드롭량")]
        [Range(0, 50)]
        public int soulDrop = 1;


        // ====== 첫 클리어 보상 ======

        [Header("첫 클리어 보상")]
        [Tooltip("첫 클리어 시 보상 폼 (null 가능)")]
        public FormData firstClearRewardForm;

        [Tooltip("첫 클리어 추가 골드")]
        [Range(0, 10000)]
        public int firstClearBonusGold = 500;


        // ====== 등급별 기본값 ======

        /// <summary>
        /// 등급에 맞는 기본값 적용 (에디터 버튼용)
        /// </summary>
        [ContextMenu("등급별 기본값 적용")]
        private void ApplyGradeDefaults()
        {
            switch (bossGrade)
            {
                case BossGrade.MiniBoss:
                    maxHealth = 500;
                    baseAttack = 15;
                    goldReward = 100;
                    expReward = 50;
                    boneDrop = 5;
                    soulDrop = 0;
                    timeLimit = 120f;
                    phases = BossPhaseData.CreateMiniBossPhases();
                    break;

                case BossGrade.MidBoss:
                    maxHealth = 1200;
                    baseAttack = 25;
                    goldReward = 300;
                    expReward = 150;
                    boneDrop = 15;
                    soulDrop = 1;
                    timeLimit = 180f;
                    phases = BossPhaseData.CreateMidBossPhases();
                    break;

                case BossGrade.FinalBoss:
                    maxHealth = 2500;
                    baseAttack = 40;
                    goldReward = 1000;
                    expReward = 500;
                    boneDrop = 30;
                    soulDrop = 5;
                    timeLimit = 300f;
                    phases = BossPhaseData.CreateFinalBossPhases();
                    break;
            }

            Debug.Log($"[BossData] {bossName}: {bossGrade} 기본값 적용됨");
        }


        // ====== 유효성 검증 ======

        private void OnValidate()
        {
            // 페이즈가 없으면 경고
            if (phases == null || phases.Length == 0)
            {
                Debug.LogWarning($"[BossData] {bossName}: 페이즈가 설정되지 않았습니다.");
            }

            // ID 형식 검증
            if (string.IsNullOrEmpty(bossId) || !bossId.StartsWith("B"))
            {
                Debug.LogWarning($"[BossData] {bossName}: bossId는 'B'로 시작해야 합니다. (예: B001)");
            }
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 특정 체력 비율에 해당하는 페이즈 인덱스 반환
        /// </summary>
        public int GetPhaseIndexForHealthRatio(float healthRatio)
        {
            if (phases == null || phases.Length == 0)
                return 0;

            for (int i = phases.Length - 1; i >= 0; i--)
            {
                if (healthRatio <= phases[i].healthThreshold)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// 페이즈 수 반환
        /// </summary>
        public int PhaseCount => phases != null ? phases.Length : 1;

        /// <summary>
        /// 예상 전투 시간 (초) 반환
        /// </summary>
        public float GetExpectedCombatTime()
        {
            return bossGrade switch
            {
                BossGrade.MiniBoss => 45f,
                BossGrade.MidBoss => 90f,
                BossGrade.FinalBoss => 180f,
                _ => 60f
            };
        }

        /// <summary>
        /// 디버그 정보 문자열 반환
        /// </summary>
        public override string ToString()
        {
            return $"[BossData] {bossId}: {bossName} ({bossGrade}) - HP={maxHealth}, ATK={baseAttack}, Phases={PhaseCount}";
        }
    }
}
