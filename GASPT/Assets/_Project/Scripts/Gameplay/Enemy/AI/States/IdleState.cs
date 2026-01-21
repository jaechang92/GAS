using UnityEngine;

namespace GASPT.Gameplay.Enemies.AI.States
{
    /// <summary>
    /// 대기 상태
    /// 일정 시간 후 순찰 상태로 전환
    /// </summary>
    public class IdleState : EnemyAIStateBase
    {
        // ====== 설정 ======

        private readonly float idleDuration;


        // ====== 생성자 ======

        /// <summary>
        /// 기본 생성자 (ForceChangeState용)
        /// </summary>
        public IdleState() : this(0.5f)
        {
        }

        /// <summary>
        /// IdleState 생성자
        /// </summary>
        /// <param name="duration">대기 시간</param>
        public IdleState(float duration)
        {
            idleDuration = duration;
        }


        // ====== IEnemyAIState 구현 ======

        public override string StateName => "Idle";

        public override void Enter(EnemyAIContext context)
        {
            base.Enter(context);
            Stop(context);
        }

        public override void Update(EnemyAIContext context)
        {
            context.StateTimer += Time.deltaTime;
        }

        public override IEnemyAIState CheckTransitions(EnemyAIContext context)
        {
            // 사망 체크
            if (context.IsDead())
            {
                return new DeadState();
            }

            // 플레이어 감지 시 즉시 추격
            if (context.IsPlayerInDetectionRange())
            {
                return new ChaseState();
            }

            // 대기 시간 완료 후 순찰
            if (context.StateTimer >= idleDuration)
            {
                return new PatrolState();
            }

            return null;
        }
    }
}
