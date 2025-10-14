using UnityEngine;
using System.Threading;

namespace Enemy
{
    /// <summary>
    /// Enemy Patrol 상태
    /// 좌우로 정찰 전용 - 감지는 EnemyController가 처리
    /// </summary>
    public class EnemyPatrolState : EnemyBaseState
    {
        private float patrolWaitTime = 0f;
        private bool isWaiting = false;

        public EnemyPatrolState() : base(EnemyStateType.Patrol) { }

        protected override void EnterStateSync()
        {
            LogStateDebug("Patrol 상태 진입(동기) - 정찰 시작");
            patrolWaitTime = 0f;
            isWaiting = false;
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("Patrol 상태 종료(동기)");
            StopMovement();
        }

        protected override void UpdateState(float deltaTime)
        {
            if (enemy == null || enemy.Data == null) return;

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
            if (currentDistance >= enemy.Data.patrolRange)
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
