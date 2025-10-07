using UnityEngine;
using System.Threading;

namespace Enemy
{
    /// <summary>
    /// Enemy Idle 상태
    /// 대기 상태에서 플레이어 감지 및 Patrol/Chase 전환
    /// </summary>
    public class EnemyIdleState : EnemyBaseState
    {
        private float idleTime = 0f;
        private const float MaxIdleTime = 2f; // 2초 후 Patrol로 전환

        public EnemyIdleState() : base(EnemyStateType.Idle) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("Idle 상태 진입");
            idleTime = 0f;

            // 이동 정지
            StopMovement();

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("Idle 상태 종료");
            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override void UpdateState(float deltaTime)
        {
            if (enemy == null || enemy.Data == null) return;

            idleTime += deltaTime;

            // 플레이어 감지 확인
            if (enemy.Target != null && enemy.DistanceToTarget <= enemy.Data.detectionRange)
            {
                LogStateDebug($"플레이어 감지! 거리: {enemy.DistanceToTarget:F2}");
                enemy.ChangeState(EnemyStateType.Chase);
                return;
            }

            // 일정 시간 후 Patrol로 전환 (Patrol이 활성화된 경우)
            if (enemy.Data.enablePatrol && idleTime >= MaxIdleTime)
            {
                LogStateDebug("Patrol 상태로 전환");
                enemy.ChangeState(EnemyStateType.Patrol);
            }
        }
    }
}
