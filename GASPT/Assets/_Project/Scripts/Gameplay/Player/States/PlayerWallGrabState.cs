using System.Threading;
using UnityEngine;

namespace Player
{
    public class PlayerWallGrabState : PlayerBaseState
    {
        public PlayerWallGrabState(PlayerController controller) : base(PlayerStateType.WallGrab)
        {
            playerController = controller;
        }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("벽잡기 상태 진입");
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("벽잡기 상태 종료");
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            if (!playerController.IsTouchingWall || playerController.IsGrounded)
            {
                if (playerController.IsGrounded)
                    playerController.ChangeState(PlayerStateType.Idle);
                else
                    playerController.ChangeState(PlayerStateType.Jump);
            }
        }
    }
}