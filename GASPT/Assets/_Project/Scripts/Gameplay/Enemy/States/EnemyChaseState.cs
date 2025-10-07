using UnityEngine;
using System.Threading;

namespace Enemy
{
    /// <summary>
    /// Enemy Chase 상태
    /// 플레이어를 추적하며 공격 범위 내 진입 시 Attack 전환
    /// </summary>
    public class EnemyChaseState : EnemyBaseState
    {
        public EnemyChaseState() : base(EnemyStateType.Chase) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("Chase 상태 진입");
            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("Chase 상태 종료");
            StopMovement();
            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override void UpdateState(float deltaTime)
        {
            if (enemy == null || enemy.Data == null || enemy.Target == null) return;

            float distanceToTarget = enemy.DistanceToTarget;

            // 추적 범위를 벗어나면 Idle로 전환
            if (distanceToTarget > enemy.Data.chaseRange)
            {
                LogStateDebug($"추적 범위 초과 - Idle로 전환 (거리: {distanceToTarget:F2})");
                enemy.ChangeState(EnemyStateType.Idle);
                return;
            }

            // 공격 범위 내 진입 시 Attack 전환
            if (distanceToTarget <= enemy.Data.attackRange)
            {
                LogStateDebug($"공격 범위 진입! (거리: {distanceToTarget:F2})");
                enemy.ChangeState(EnemyStateType.Attack);
                return;
            }

            // 타겟 추적
            MoveToTarget(enemy.Data.moveSpeed);
            enemy.UpdateSpriteDirection();
        }
    }
}
