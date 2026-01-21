using UnityEngine;
using GASPT.Data;
using GASPT.Stats;

namespace GASPT.Gameplay.Enemies.AI
{
    /// <summary>
    /// AI 상태 간 공유 데이터 컨텍스트
    /// 모든 상태가 접근할 수 있는 공용 데이터
    /// </summary>
    public class EnemyAIContext
    {
        // ====== 컴포넌트 참조 ======

        /// <summary>
        /// Enemy 컴포넌트 (PlatformerEnemy 또는 하위 클래스)
        /// </summary>
        public Enemy Enemy { get; set; }

        /// <summary>
        /// Rigidbody2D 컴포넌트
        /// </summary>
        public Rigidbody2D Rigidbody { get; set; }

        /// <summary>
        /// Transform 컴포넌트
        /// </summary>
        public Transform Transform { get; set; }

        /// <summary>
        /// SpriteRenderer 컴포넌트
        /// </summary>
        public SpriteRenderer SpriteRenderer { get; set; }

        /// <summary>
        /// Collider2D 컴포넌트
        /// </summary>
        public Collider2D Collider { get; set; }


        // ====== 플레이어 참조 ======

        /// <summary>
        /// 플레이어 Transform
        /// </summary>
        public Transform PlayerTransform { get; set; }

        /// <summary>
        /// 플레이어 스탯
        /// </summary>
        public PlayerStats PlayerStats { get; set; }


        // ====== 설정 (EnemyData에서) ======

        /// <summary>
        /// 적 데이터 ScriptableObject
        /// </summary>
        public EnemyData Data { get; set; }


        // ====== 상태 공유 변수 ======

        /// <summary>
        /// 시작 위치 (순찰 기준점)
        /// </summary>
        public Vector3 StartPosition { get; set; }

        /// <summary>
        /// 마지막 공격 시간
        /// </summary>
        public float LastAttackTime { get; set; }

        /// <summary>
        /// 오른쪽을 바라보고 있는지
        /// </summary>
        public bool IsFacingRight { get; set; } = true;


        // ====== 순찰 관련 ======

        /// <summary>
        /// 순찰 왼쪽 경계
        /// </summary>
        public float PatrolLeftBound { get; set; }

        /// <summary>
        /// 순찰 오른쪽 경계
        /// </summary>
        public float PatrolRightBound { get; set; }

        /// <summary>
        /// 순찰 방향 (true = 오른쪽)
        /// </summary>
        public bool PatrolMovingRight { get; set; } = true;


        // ====== 타이머 ======

        /// <summary>
        /// 현재 상태 진입 후 경과 시간
        /// </summary>
        public float StateTimer { get; set; }


        // ====== 디버그 ======

        /// <summary>
        /// 디버그 로그 출력 여부
        /// </summary>
        public bool ShowDebugLogs { get; set; }

        /// <summary>
        /// 기즈모 표시 여부
        /// </summary>
        public bool ShowGizmos { get; set; } = true;


        // ====== 유틸리티 메서드 ======

        /// <summary>
        /// 플레이어까지의 거리 계산
        /// </summary>
        public float GetDistanceToPlayer()
        {
            if (PlayerTransform == null) return float.MaxValue;
            return Vector2.Distance(Transform.position, PlayerTransform.position);
        }

        /// <summary>
        /// 플레이어가 감지 범위 안에 있는지
        /// </summary>
        public bool IsPlayerInDetectionRange()
        {
            if (Data == null) return false;
            return GetDistanceToPlayer() <= Data.detectionRange;
        }

        /// <summary>
        /// 플레이어가 공격 범위 안에 있는지
        /// </summary>
        public bool IsPlayerInAttackRange()
        {
            if (Data == null) return false;
            return GetDistanceToPlayer() <= Data.attackRange;
        }

        /// <summary>
        /// 플레이어 방향 벡터
        /// </summary>
        public Vector2 GetDirectionToPlayer()
        {
            if (PlayerTransform == null) return Vector2.right;
            return (PlayerTransform.position - Transform.position).normalized;
        }

        /// <summary>
        /// 공격 쿨다운이 완료되었는지
        /// </summary>
        public bool CanAttack()
        {
            if (Data == null) return false;
            return Time.time - LastAttackTime >= Data.attackCooldown;
        }

        /// <summary>
        /// 적이 사망했는지
        /// </summary>
        public bool IsDead()
        {
            return Enemy != null && Enemy.IsDead;
        }
    }
}
