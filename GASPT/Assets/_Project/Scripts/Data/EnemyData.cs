using UnityEngine;
using Core.Enums;
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
