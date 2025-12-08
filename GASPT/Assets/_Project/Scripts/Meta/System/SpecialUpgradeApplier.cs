using System;
using UnityEngine;

namespace GASPT.Meta
{
    /// <summary>
    /// 특수 업그레이드 런타임 적용 시스템
    /// ExtraDash, Revive 등 게임플레이에 직접 영향을 주는 업그레이드 관리
    /// </summary>
    public class SpecialUpgradeApplier : MonoBehaviour
    {
        // ====== 싱글톤 ======

        private static SpecialUpgradeApplier instance;
        public static SpecialUpgradeApplier Instance => instance;


        // ====== 런타임 상태 ======

        private int remainingRevives;
        private int baseDashCount = 1;
        private int currentExtraDash;


        // ====== 이벤트 ======

        /// <summary>부활 사용 이벤트 (남은 횟수)</summary>
        public event Action<int> OnReviveUsed;

        /// <summary>부활 불가 이벤트 (0회 남음)</summary>
        public event Action OnNoRevivesLeft;


        // ====== 프로퍼티 ======

        /// <summary>남은 부활 횟수</summary>
        public int RemainingRevives => remainingRevives;

        /// <summary>부활 가능 여부</summary>
        public bool CanRevive => remainingRevives > 0;

        /// <summary>총 대시 횟수 (기본 + 추가)</summary>
        public int TotalDashCount => baseDashCount + currentExtraDash;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        private void Start()
        {
            var metaManager = MetaProgressionManager.Instance;
            if (metaManager != null)
            {
                metaManager.OnRunStarted += OnRunStarted;
            }
        }

        private void OnDestroy()
        {
            var metaManager = MetaProgressionManager.Instance;
            if (metaManager != null)
            {
                metaManager.OnRunStarted -= OnRunStarted;
            }

            if (instance == this)
            {
                instance = null;
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// 런 시작 시 특수 업그레이드 적용
        /// </summary>
        private void OnRunStarted()
        {
            ApplySpecialUpgrades();
        }

        /// <summary>
        /// 특수 업그레이드 값 적용
        /// </summary>
        public void ApplySpecialUpgrades()
        {
            var metaManager = MetaProgressionManager.Instance;
            if (metaManager == null) return;

            // 부활 횟수 설정
            remainingRevives = metaManager.GetReviveCount();

            // 추가 대시 횟수 설정
            currentExtraDash = metaManager.GetExtraDash();

            Debug.Log($"[SpecialUpgradeApplier] 특수 업그레이드 적용 - 부활: {remainingRevives}회, 대시: {TotalDashCount}회");
        }


        // ====== 부활 시스템 ======

        /// <summary>
        /// 부활 시도
        /// </summary>
        /// <returns>부활 성공 여부</returns>
        public bool TryRevive()
        {
            if (remainingRevives <= 0)
            {
                Debug.Log("[SpecialUpgradeApplier] 부활 불가 - 남은 횟수 없음");
                OnNoRevivesLeft?.Invoke();
                return false;
            }

            remainingRevives--;

            Debug.Log($"[SpecialUpgradeApplier] 부활 사용! 남은 횟수: {remainingRevives}");
            OnReviveUsed?.Invoke(remainingRevives);

            return true;
        }

        /// <summary>
        /// 부활 시 체력 회복 비율 가져오기
        /// </summary>
        /// <returns>회복 비율 (0.0 ~ 1.0)</returns>
        public float GetReviveHealthPercent()
        {
            // 기본 30% 체력으로 부활
            return 0.3f;
        }


        // ====== 대시 시스템 ======

        /// <summary>
        /// 기본 대시 횟수 설정 (캐릭터나 폼에서 호출)
        /// </summary>
        public void SetBaseDashCount(int count)
        {
            baseDashCount = Mathf.Max(1, count);
            Debug.Log($"[SpecialUpgradeApplier] 기본 대시 설정: {baseDashCount}, 총: {TotalDashCount}");
        }

        /// <summary>
        /// 대시 횟수 가져오기 (캐릭터 컨트롤러에서 사용)
        /// </summary>
        public int GetDashCount()
        {
            return TotalDashCount;
        }


        // ====== 스탯 적용 헬퍼 ======

        /// <summary>
        /// 모든 영구 업그레이드 효과를 스탯에 적용
        /// </summary>
        /// <param name="baseStats">기본 스탯</param>
        /// <returns>업그레이드가 적용된 스탯</returns>
        public static AppliedStats ApplyUpgradesToStats(BaseStats baseStats)
        {
            var metaManager = MetaProgressionManager.Instance;
            if (metaManager == null)
            {
                return new AppliedStats(baseStats);
            }

            return new AppliedStats
            {
                maxHP = baseStats.maxHP + metaManager.GetMaxHPBonus(),
                attack = baseStats.attack * (1f + metaManager.GetAttackBonus()),
                defense = metaManager.GetDefenseBonus(),
                moveSpeed = baseStats.moveSpeed * (1f + metaManager.GetMoveSpeedBonus()),
                goldBonus = metaManager.GetGoldBonus(),
                expBonus = metaManager.GetExpBonus(),
                startGold = metaManager.GetStartGold()
            };
        }


        // ====== 디버그 ======

        [ContextMenu("Debug: Print Status")]
        private void DebugPrintStatus()
        {
            Debug.Log("========== SpecialUpgradeApplier ==========");
            Debug.Log($"남은 부활: {remainingRevives}회");
            Debug.Log($"기본 대시: {baseDashCount}, 추가 대시: {currentExtraDash}, 총: {TotalDashCount}");
            Debug.Log("============================================");
        }

        [ContextMenu("Debug: Use Revive")]
        private void DebugUseRevive()
        {
            TryRevive();
        }
    }


    /// <summary>
    /// 기본 스탯 구조체 (업그레이드 적용 전)
    /// </summary>
    [Serializable]
    public struct BaseStats
    {
        public int maxHP;
        public float attack;
        public float moveSpeed;

        public BaseStats(int hp, float atk, float speed)
        {
            maxHP = hp;
            attack = atk;
            moveSpeed = speed;
        }
    }


    /// <summary>
    /// 업그레이드가 적용된 스탯 구조체
    /// </summary>
    [Serializable]
    public struct AppliedStats
    {
        public int maxHP;
        public float attack;
        public float defense;
        public float moveSpeed;
        public float goldBonus;
        public float expBonus;
        public int startGold;

        public AppliedStats(BaseStats baseStats)
        {
            maxHP = baseStats.maxHP;
            attack = baseStats.attack;
            defense = 0f;
            moveSpeed = baseStats.moveSpeed;
            goldBonus = 0f;
            expBonus = 0f;
            startGold = 0;
        }

        public override string ToString()
        {
            return $"HP:{maxHP}, ATK:{attack:F1}, DEF:{defense:P0}, SPD:{moveSpeed:F1}, Gold+{goldBonus:P0}, Exp+{expBonus:P0}, StartGold:{startGold}";
        }
    }
}
