using UnityEngine;
using System.Threading;

namespace Enemy
{
    /// <summary>
    /// Enemy Patrol 상태
    /// 좌우로 정찰하며 플레이어 감지
    /// </summary>
    public class EnemyPatrolState : EnemyBaseState
    {
        private float patrolWaitTime = 0f;
        private bool isWaiting = false;

        public EnemyPatrolState() : base(EnemyStateType.Patrol) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("Patrol 상태 진입");
            patrolWaitTime = 0f;
            isWaiting = false;

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("Patrol 상태 종료");
            StopMovement();
            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override void UpdateState(float deltaTime)
        {
            if (enemy == null || enemy.Data == null) return;

            // 플레이어 감지 확인
            if (enemy.Target != null && enemy.DistanceToTarget <= enemy.Data.detectionRange)
            {
                LogStateDebug($"플레이어 감지! Chase로 전환");
                enemy.ChangeState(EnemyStateType.Chase);
                return;
            }

            // 대기 중인 경우
            if (isWaiting)
            {
                patrolWaitTime += deltaTime;

                if (patrolWaitTime >= enemy.Data.patrolWaitTime)
                {
                    // 방향 전환
                    enemy.PatrolMovingRight = !enemy.PatrolMovingRight;
                    isWaiting = false;
                    patrolWaitTime = 0f;
                    LogStateDebug($"정찰 방향 전환: {(enemy.PatrolMovingRight ? "오른쪽" : "왼쪽")}");
                }
                return;
            }

            // 정찰 이동
            float currentDistance = Vector3.Distance(enemy.transform.position, enemy.PatrolStartPosition);

            // 정찰 거리 초과 시 대기 및 방향 전환
            if (currentDistance >= enemy.Data.patrolDistance)
            {
                StopMovement();
                isWaiting = true;
                patrolWaitTime = 0f;
                LogStateDebug("정찰 거리 도달 - 대기 시작");
                return;
            }

            // 이동
            int direction = enemy.PatrolMovingRight ? 1 : -1;
            enemy.FacingDirection = direction;
            enemy.UpdateSpriteDirection();

            Vector2 velocity = enemy.Rigidbody.linearVelocity;
            velocity.x = direction * enemy.Data.moveSpeed * 0.5f; // 정찰 속도는 이동 속도의 50%
            enemy.Rigidbody.linearVelocity = velocity;
        }
    }
}
