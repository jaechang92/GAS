using UnityEngine;
using System.Threading;

namespace Enemy
{
    /// <summary>
    /// Enemy Hit 상태
    /// 피격 시 경직 및 넉백 효과
    /// </summary>
    public class EnemyHitState : EnemyBaseState
    {
        private float hitStunTime = 0f;

        public EnemyHitState() : base(EnemyStateType.Hit) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("Hit 상태 진입");
            hitStunTime = 0f;

            // 이동 정지 (넉백은 DamageSystem에서 처리됨)
            StopMovement();

            // 피격 애니메이션 트리거 (애니메이터 있으면)
            // TODO: 애니메이션 추가 시 구현

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("Hit 상태 종료");
            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override void UpdateState(float deltaTime)
        {
            if (enemy == null || enemy.Data == null) return;

            hitStunTime += deltaTime;

            // 경직 시간 종료 후 다음 상태로 전환
            if (hitStunTime >= enemy.Data.hitStunDuration)
            {
                TransitionToNextState();
            }
        }

        /// <summary>
        /// 다음 상태로 전환
        /// </summary>
        private void TransitionToNextState()
        {
            if (enemy == null || enemy.Target == null)
            {
                enemy.ChangeState(EnemyStateType.Idle);
                return;
            }

            float distanceToTarget = enemy.DistanceToTarget;

            // 타겟이 추적 범위 내에 있으면 Chase
            if (distanceToTarget <= enemy.Data.chaseRange)
            {
                enemy.ChangeState(EnemyStateType.Chase);
            }
            else
            {
                // 범위 밖이면 Idle
                enemy.ChangeState(EnemyStateType.Idle);
            }
        }
    }
}
