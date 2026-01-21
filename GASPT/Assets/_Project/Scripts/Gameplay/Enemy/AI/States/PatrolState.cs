using UnityEngine;

namespace GASPT.Gameplay.Enemies.AI.States
{
    /// <summary>
    /// 순찰 상태
    /// 설정된 범위 내에서 좌우로 이동
    /// 플레이어 감지 시 추격 상태로 전환
    /// </summary>
    public class PatrolState : EnemyAIStateBase
    {
        // ====== 낭떠러지 체크 설정 ======

        private readonly LayerMask groundLayer;
        private readonly float edgeCheckDistance;
        private readonly Vector2 edgeCheckOffset;


        // ====== 생성자 ======

        /// <summary>
        /// 기본 생성자 (ForceChangeState용)
        /// </summary>
        public PatrolState() : this(null, 0.5f, null)
        {
        }

        /// <summary>
        /// PatrolState 생성자
        /// </summary>
        /// <param name="groundLayer">바닥 레이어 (기본값: 1)</param>
        /// <param name="edgeCheckDistance">낭떠러지 체크 거리 (기본값: 0.5f)</param>
        /// <param name="edgeCheckOffset">낭떠러지 체크 오프셋 (기본값: (0.5f, -0.5f))</param>
        public PatrolState(
            LayerMask? groundLayer,
            float edgeCheckDistance,
            Vector2? edgeCheckOffset)
        {
            this.groundLayer = groundLayer ?? 1;
            this.edgeCheckDistance = edgeCheckDistance;
            this.edgeCheckOffset = edgeCheckOffset ?? new Vector2(0.5f, -0.5f);
        }


        // ====== IEnemyAIState 구현 ======

        public override string StateName => "Patrol";

        public override void Enter(EnemyAIContext context)
        {
            base.Enter(context);

            // 시작 위치 기준으로 순찰 방향 설정
            if (context.Transform.position.x < context.StartPosition.x)
            {
                context.PatrolMovingRight = true;
            }
            else if (context.Transform.position.x > context.StartPosition.x)
            {
                context.PatrolMovingRight = false;
            }
        }

        public override void Update(EnemyAIContext context)
        {
            context.StateTimer += Time.deltaTime;
        }

        public override void PhysicsUpdate(EnemyAIContext context)
        {
            if (context.Data == null) return;

            // 순찰 방향으로 이동
            Vector2 direction = context.PatrolMovingRight ? Vector2.right : Vector2.left;
            Move(context, direction, context.Data.moveSpeed);

            // 순찰 범위 체크
            if (context.PatrolMovingRight &&
                context.Transform.position.x >= context.PatrolRightBound)
            {
                context.PatrolMovingRight = false;
            }
            else if (!context.PatrolMovingRight &&
                     context.Transform.position.x <= context.PatrolLeftBound)
            {
                context.PatrolMovingRight = true;
            }

            // 낭떠러지 체크
            if (IsEdgeAhead(context))
            {
                context.PatrolMovingRight = !context.PatrolMovingRight;

                if (context.ShowDebugLogs)
                {
                    Debug.Log($"[AI] {context.Data?.enemyName} 낭떠러지 감지 - 방향 반전!");
                }
            }
        }

        public override IEnemyAIState CheckTransitions(EnemyAIContext context)
        {
            // 사망 체크
            if (context.IsDead())
            {
                return new DeadState();
            }

            // 플레이어 감지 시 추격
            if (context.IsPlayerInDetectionRange())
            {
                return new ChaseState();
            }

            return null;
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 앞쪽에 낭떠러지가 있는지 체크
        /// </summary>
        private bool IsEdgeAhead(EnemyAIContext context)
        {
            if (context.Collider == null) return false;

            // 체크 위치 계산 (적의 앞쪽 바닥)
            Vector2 checkDirection = context.IsFacingRight ? Vector2.right : Vector2.left;
            Vector2 checkOrigin = (Vector2)context.Transform.position +
                                 new Vector2(checkDirection.x * edgeCheckOffset.x, edgeCheckOffset.y);

            // Raycast로 바닥 체크
            RaycastHit2D hit = Physics2D.Raycast(checkOrigin, Vector2.down, edgeCheckDistance, groundLayer);

            // 디버그 라인
            if (context.ShowGizmos)
            {
                Debug.DrawRay(checkOrigin, Vector2.down * edgeCheckDistance,
                             hit.collider != null ? Color.green : Color.red);
            }

            // 바닥이 없으면 낭떠러지
            return hit.collider == null;
        }
    }
}
