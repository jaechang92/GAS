using UnityEngine;

namespace Enemy.Data
{
    /// <summary>
    /// 적 데이터 ScriptableObject
    /// 적의 기본 스탯, 행동 설정 등을 정의
    /// </summary>
    [CreateAssetMenu(fileName = "New Enemy Data", menuName = "GASPT/Enemy/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("적 이름")]
        public string enemyName = "Enemy";

        [Tooltip("적 타입")]
        public EnemyType enemyType = EnemyType.Melee;

        [Header("기본 스탯")]
        [Tooltip("최대 체력")]
        public float maxHealth = 50f;

        [Tooltip("이동 속도")]
        public float moveSpeed = 3f;

        [Tooltip("공격 데미지")]
        public float attackDamage = 10f;

        [Tooltip("공격 쿨다운 (초)")]
        public float attackCooldown = 1.5f;

        [Header("감지 설정")]
        [Tooltip("플레이어 감지 주기 (초) - 성능 최적화")]
        public float detectionInterval = 0.2f;

        [Tooltip("플레이어 감지 범위 (감지 시작)")]
        public float detectionRange = 10f;

        [Tooltip("추적 범위 (추적 중 유지 거리)")]
        public float chaseRange = 12f;

        [Tooltip("공격 범위 (공격 가능 거리)")]
        public float attackRange = 1.5f;

        [Header("정찰 설정")]
        [Tooltip("정찰 활성화")]
        public bool enablePatrol = true;

        [Tooltip("정찰 이동 거리")]
        public float patrolDistance = 5f;

        [Tooltip("정찰 전환 대기 시간 (초)")]
        public float patrolWaitTime = 2f;

        [Header("히트박스 설정")]
        [Tooltip("히트박스 크기")]
        public Vector2 hitboxSize = new Vector2(1.5f, 1.0f);

        [Tooltip("히트박스 오프셋 (앞쪽 방향 기준)")]
        public Vector2 hitboxOffset = new Vector2(0.7f, 0f);

        [Tooltip("히트박스 지속 시간")]
        public float hitboxDuration = 0.2f;

        [Header("넉백 설정")]
        [Tooltip("넉백 힘")]
        public float knockbackForce = 5f;

        [Tooltip("피격 경직 시간 (초)")]
        public float hitStunDuration = 0.3f;

        [Header("시각화")]
        [Tooltip("적 색상 (디버그용)")]
        public Color enemyColor = Color.red;
    }
}
