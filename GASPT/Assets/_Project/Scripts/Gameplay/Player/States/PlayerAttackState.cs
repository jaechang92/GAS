using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 공격 상태 (리팩토링됨)
    /// </summary>
    public class PlayerAttackState : PlayerBaseState
    {
        private float attackDuration = 0.3f;
        private float attackTime = 0f;

        public PlayerAttackState(PlayerController controller) : base(PlayerStateType.Attack)
        {
            playerController = controller;
        }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("공격 상태 진입");
            attackTime = 0f;

            // GAS 시스템을 통한 공격 어빌리티 활성화
            playerController.ActivateAbility("BasicAttack");

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("공격 상태 종료");

            // 공격 어빌리티 비활성화
            playerController.DeactivateAbility("BasicAttack");

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            attackTime += deltaTime;
            CheckForStateTransitions();
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null) return;

            // 공격 시간이 끝나면 이전 상태로 복귀
            if (attackTime >= attackDuration)
            {
                if (playerController.IsGrounded)
                {
                    if (Mathf.Abs(playerController.Velocity.x) > 0.1f)
                        playerController.ChangeState(PlayerStateType.Move);
                    else
                        playerController.ChangeState(PlayerStateType.Idle);
                }
                else
                {
                    playerController.ChangeState(PlayerStateType.Jump);
                }
            }
        }
    }
}