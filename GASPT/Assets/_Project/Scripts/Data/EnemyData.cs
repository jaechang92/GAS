using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Loot;

namespace GASPT.Data
{
    /// <summary>
    /// 적 데이터 ScriptableObject
    /// 적의 타입, 스탯, 보상, UI 설정을 정의
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "GASPT/Enemies/Enemy")]
    public class EnemyData : ScriptableObject
    {
        // ====== 타입 ======

        [Header("타입")]
        [Tooltip("적 타입 (Normal, Named, Boss)")]
        public EnemyType enemyType;

        [Tooltip("적 클래스 (어떤 Enemy 컴포넌트를 사용할지 결정)")]
        public EnemyClass enemyClass = EnemyClass.BasicMelee;


        // ====== 기본 정보 ======

        [Header("기본 정보")]
        [Tooltip("적 이름")]
        public string enemyName;

        [Tooltip("적 아이콘 (UI 표시용)")]
        public Sprite icon;


        // ====== 스탯 ======

        [Header("스탯")]
        [Tooltip("최대 HP")]
        [Range(1, 1000)]
        public int maxHp = 30;

        [Tooltip("공격력")]
        [Range(1, 100)]
        public int attack = 5;


        // ====== 보상 ======

        [Header("보상")]
        [Tooltip("최소 골드 드랍")]
        [Range(0, 1000)]
        public int minGoldDrop = 15;

        [Tooltip("최대 골드 드랍")]
        [Range(0, 1000)]
        public int maxGoldDrop = 25;

        [Tooltip("경험치 보상")]
        [Range(0, 1000)]
        public int expReward = 10;

        [Tooltip("아이템 드롭 테이블 (선택사항)")]
        public LootTable lootTable;


        // ====== 메타 재화 보상 ======

        [Header("메타 재화 보상")]
        [Tooltip("최소 Bone 드롭")]
        [Range(0, 100)]
        public int minBoneDrop = 0;

        [Tooltip("최대 Bone 드롭")]
        [Range(0, 100)]
        public int maxBoneDrop = 0;

        [Tooltip("Soul 드롭량 (보스 전용)")]
        [Range(0, 100)]
        public int soulDrop = 0;


        // ====== 플랫포머 설정 ======

        [Header("플랫포머 설정")]
        [Tooltip("기본 이동 속도")]
        [Range(0.5f, 10f)]
        public float moveSpeed = 2f;

        [Tooltip("플레이어 감지 거리")]
        [Range(1f, 20f)]
        public float detectionRange = 5f;

        [Tooltip("공격 범위 (근접 공격)")]
        [Range(0.5f, 5f)]
        public float attackRange = 1.5f;

        [Tooltip("순찰 거리 (좌우)")]
        [Range(1f, 10f)]
        public float patrolDistance = 3f;

        [Tooltip("추격 속도 (감지 후)")]
        [Range(1f, 15f)]
        public float chaseSpeed = 3f;

        [Tooltip("공격 쿨다운 (초)")]
        [Range(0.5f, 5f)]
        public float attackCooldown = 1.5f;


        // ====== 원거리 적 설정 (RangedEnemy용) ======

        [Header("원거리 적 설정")]
        [Tooltip("최적 공격 거리 (RangedEnemy용)")]
        [Range(5f, 20f)]
        public float optimalAttackDistance = 10f;

        [Tooltip("최소 안전 거리 (RangedEnemy용)")]
        [Range(2f, 10f)]
        public float minDistance = 5f;


        // ====== 비행 적 설정 (FlyingEnemy용) ======

        [Header("비행 적 설정")]
        [Tooltip("비행 높이 (FlyingEnemy용)")]
        [Range(3f, 10f)]
        public float flyHeight = 6f;

        [Tooltip("급강하 속도 (FlyingEnemy용)")]
        [Range(5f, 20f)]
        public float diveSpeed = 10f;

        [Tooltip("일반 비행 속도 (FlyingEnemy용)")]
        [Range(1f, 8f)]
        public float flySpeed = 3f;


        // ====== 정예 적 스킬 설정 (EliteEnemy용) ======

        [Header("정예 적 스킬 설정")]
        [Tooltip("돌진 공격 쿨다운 (초)")]
        [Range(3f, 10f)]
        public float chargeCooldown = 5f;

        [Tooltip("범위 공격 쿨다운 (초)")]
        [Range(5f, 15f)]
        public float areaCooldown = 7f;

        [Tooltip("범위 공격 반경")]
        [Range(2f, 6f)]
        public float areaAttackRadius = 3f;

        [Tooltip("돌진 속도")]
        [Range(5f, 15f)]
        public float chargeSpeed = 8f;

        [Tooltip("돌진 거리")]
        [Range(3f, 10f)]
        public float chargeDistance = 5f;


        // ====== 보스 전용 설정 (BossEnemy용) ======

        [Header("보스 전용 설정")]
        [Tooltip("Phase 1 원거리 공격 쿨다운 (초)")]
        [Range(2f, 10f)]
        public float bossRangedCooldown = 3f;

        [Tooltip("Phase 2 돌진 공격 쿨다운 (초)")]
        [Range(3f, 15f)]
        public float bossChargeCooldown = 5f;

        [Tooltip("Phase 2 소환 쿨다운 (초)")]
        [Range(5f, 20f)]
        public float bossSummonCooldown = 10f;

        [Tooltip("Phase 3 범위 공격 쿨다운 (초)")]
        [Range(5f, 15f)]
        public float bossAreaCooldown = 7f;

        [Tooltip("Phase 3 범위 공격 반경")]
        [Range(3f, 10f)]
        public float bossAreaRadius = 5f;

        [Tooltip("투사체 속도 (보스 원거리 공격)")]
        [Range(5f, 20f)]
        public float bossProjectileSpeed = 8f;

        [Tooltip("투사체 데미지 (보스 원거리 공격)")]
        [Range(10, 50)]
        public int bossProjectileDamage = 15;

        [Tooltip("범위 공격 데미지")]
        [Range(20, 100)]
        public int bossAreaDamage = 30;

        [Tooltip("돌진 속도 (보스)")]
        [Range(5f, 20f)]
        public float bossChargeSpeed = 10f;

        [Tooltip("돌진 거리 (보스)")]
        [Range(5f, 15f)]
        public float bossChargeDistance = 8f;


        // ====== UI ======

        [Header("UI")]
        [Tooltip("이름표 표시 여부 (Named 전용)")]
        public bool showNameTag = false;

        [Tooltip("보스 체력바 표시 여부 (Boss 전용)")]
        public bool showBossHealthBar = false;


        // ====== 유효성 검증 ======

        /// <summary>
        /// 데이터 유효성 검증 (에디터에서 호출)
        /// </summary>
        private void OnValidate()
        {
            // 최소 골드 드랍이 최대 골드 드랍보다 크면 조정
            if (minGoldDrop > maxGoldDrop)
            {
                minGoldDrop = maxGoldDrop;
                Debug.LogWarning($"[EnemyData] {enemyName}: minGoldDrop이 maxGoldDrop보다 큽니다. minGoldDrop을 {maxGoldDrop}(으)로 조정했습니다.");
            }

            // 최소 Bone 드랍이 최대 Bone 드랍보다 크면 조정
            if (minBoneDrop > maxBoneDrop)
            {
                minBoneDrop = maxBoneDrop;
                Debug.LogWarning($"[EnemyData] {enemyName}: minBoneDrop이 maxBoneDrop보다 큽니다. minBoneDrop을 {maxBoneDrop}(으)로 조정했습니다.");
            }

            // 타입별 기본값 제안
            switch (enemyType)
            {
                case EnemyType.Normal:
                    if (maxHp > 50)
                    {
                        Debug.LogWarning($"[EnemyData] {enemyName}: Normal 타입은 일반적으로 HP가 30-50 정도입니다.");
                    }
                    break;

                case EnemyType.Named:
                    if (!showNameTag)
                    {
                        Debug.LogWarning($"[EnemyData] {enemyName}: Named 타입은 showNameTag를 true로 설정하는 것이 권장됩니다.");
                    }
                    break;

                case EnemyType.Boss:
                    if (!showBossHealthBar)
                    {
                        Debug.LogWarning($"[EnemyData] {enemyName}: Boss 타입은 showBossHealthBar를 true로 설정하는 것이 권장됩니다.");
                    }
                    break;
            }
        }


        // ====== 골드 드랍 계산 ======

        /// <summary>
        /// 랜덤 골드 드랍 계산
        /// </summary>
        /// <returns>minGoldDrop ~ maxGoldDrop 사이의 랜덤 골드</returns>
        public int GetRandomGoldDrop()
        {
            return Random.Range(minGoldDrop, maxGoldDrop + 1);
        }

        /// <summary>
        /// 랜덤 Bone 드랍 계산
        /// </summary>
        /// <returns>minBoneDrop ~ maxBoneDrop 사이의 랜덤 Bone</returns>
        public int GetRandomBoneDrop()
        {
            return Random.Range(minBoneDrop, maxBoneDrop + 1);
        }

        /// <summary>
        /// Soul 드랍량 반환 (보스 전용)
        /// </summary>
        public int GetSoulDrop()
        {
            return soulDrop;
        }


        // ====== 디버그 정보 ======

        /// <summary>
        /// 적 정보 문자열 반환
        /// </summary>
        public override string ToString()
        {
            return $"[EnemyData] {enemyName} ({enemyType}): HP={maxHp}, Attack={attack}, Gold={minGoldDrop}-{maxGoldDrop}";
        }
    }
}
