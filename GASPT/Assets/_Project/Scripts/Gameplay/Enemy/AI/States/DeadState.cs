using UnityEngine;

namespace GASPT.Gameplay.Enemies.AI.States
{
    /// <summary>
    /// 사망 상태
    /// 이동 정지, 콜라이더 비활성화
    /// 다른 상태로 전환되지 않음
    /// </summary>
    public class DeadState : EnemyAIStateBase
    {
        // ====== IEnemyAIState 구현 ======

        public override string StateName => "Dead";

        public override void Enter(EnemyAIContext context)
        {
            base.Enter(context);

            // 이동 정지
            Stop(context);

            // 콜라이더 비활성화
            if (context.Collider != null)
            {
                context.Collider.enabled = false;
            }

            if (context.ShowDebugLogs)
            {
                Debug.Log($"[AI] {context.Data?.enemyName} 사망!");
            }
        }

        public override void Update(EnemyAIContext context)
        {
            // 사망 상태에서는 아무것도 하지 않음
        }

        public override IEnemyAIState CheckTransitions(EnemyAIContext context)
        {
            // 사망 상태에서는 전환하지 않음
            return null;
        }
    }
}
