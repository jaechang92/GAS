using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 대시 상태 (리팩토링됨)
    /// 플레이어가 대시할 때의 상태
    /// </summary>
    public class PlayerDashState : PlayerBaseState
    {
        public PlayerDashState(PlayerController controller) : base(PlayerStateType.Dash)
        {
            playerController = controller;
        }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("대시 상태 진입");

            // PhysicsEngine이 대시를 자동으로 처리
            // 대시 방향은 InputHandler에서 자동으로 결정됨

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("대시 상태 종료");
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // PhysicsEngine이 대시 지속시간, 속도 등을 모두 관리
            CheckForStateTransitions();
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null) return;

            // 대시가 끝나면 적절한 상태로 전환
            if (!playerController.IsDashing)
            {
                if (playerController.IsGrounded)
                {
                    // 땅에 있으면 이동 입력에 따라 상태 결정
                    if (Mathf.Abs(playerController.Velocity.x) > 0.1f)
                        playerController.ChangeState(PlayerStateType.Move);
                    else
                        playerController.ChangeState(PlayerStateType.Idle);
                }
                else
                {
                    // 공중에 있으면 Jump 상태
                    playerController.ChangeState(PlayerStateType.Jump);
                }
            }
        }
    }
}