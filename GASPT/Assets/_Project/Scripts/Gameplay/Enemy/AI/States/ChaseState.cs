using UnityEngine;

namespace GASPT.Gameplay.Enemies.AI.States
{
    /// <summary>
    /// 추격 상태
    /// 플레이어를 향해 이동
    /// 공격 범위 진입 시 공격 상태로 전환
    /// </summary>
    public class ChaseState : EnemyAIStateBase
    {
        // ====== IEnemyAIState 구현 ======

        public override string StateName => "Chase";

        public override void Enter(EnemyAIContext context)
        {
            base.Enter(context);

            if (context.ShowDebugLogs)
            {
                Debug.Log($"[AI] {context.Data?.enemyName} 플레이어 추격 시작!");
            }
        }

        public override void Update(EnemyAIContext context)
        {
            context.StateTimer += Time.deltaTime;
        }

        public override void PhysicsUpdate(EnemyAIContext context)
        {
            if (context.Data == null) return;

            // 플레이어 방향으로 이동
            Vector2 direction = context.GetDirectionToPlayer();
            Move(context, direction, context.Data.chaseSpeed);
        }

        public override IEnemyAIState CheckTransitions(EnemyAIContext context)
        {
            // 사망 체크
            if (context.IsDead())
            {
                return new DeadState();
            }

            // 공격 범위 진입 시 공격
            if (context.IsPlayerInAttackRange())
            {
                return new AttackState();
            }

            // 플레이어가 감지 범위를 벗어나면 순찰로 복귀
            if (!context.IsPlayerInDetectionRange())
            {
                return new PatrolState();
            }

            return null;
        }
    }
}
