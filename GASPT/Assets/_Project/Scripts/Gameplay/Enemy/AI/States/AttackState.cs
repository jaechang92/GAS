using UnityEngine;

namespace GASPT.Gameplay.Enemies.AI.States
{
    /// <summary>
    /// 공격 상태
    /// 플레이어에게 데미지를 주고 쿨다운 대기
    /// 공격 범위를 벗어나면 추격 상태로 전환
    /// </summary>
    public class AttackState : EnemyAIStateBase
    {
        // ====== IEnemyAIState 구현 ======

        public override string StateName => "Attack";

        public override void Enter(EnemyAIContext context)
        {
            base.Enter(context);
            Stop(context);

            // 플레이어를 바라보기
            FacePlayer(context);
        }

        public override void Update(EnemyAIContext context)
        {
            context.StateTimer += Time.deltaTime;

            // 공격 실행
            if (context.CanAttack())
            {
                PerformAttack(context);
            }
        }

        public override IEnemyAIState CheckTransitions(EnemyAIContext context)
        {
            // 사망 체크
            if (context.IsDead())
            {
                return new DeadState();
            }

            // 플레이어가 공격 범위를 벗어나면 추격
            if (!context.IsPlayerInAttackRange())
            {
                return new ChaseState();
            }

            return null;
        }


        // ====== 공격 ======

        /// <summary>
        /// 공격 실행
        /// </summary>
        private void PerformAttack(EnemyAIContext context)
        {
            if (context.PlayerStats == null) return;
            if (context.PlayerStats.IsDead) return;

            // 데미지 적용
            int damage = context.Data?.attack ?? 5;
            context.PlayerStats.TakeDamage(damage);

            // 쿨다운 시작
            context.LastAttackTime = Time.time;

            if (context.ShowDebugLogs)
            {
                Debug.Log($"[AI] {context.Data?.enemyName} 플레이어 공격! 데미지: {damage}");
            }
        }
    }
}
