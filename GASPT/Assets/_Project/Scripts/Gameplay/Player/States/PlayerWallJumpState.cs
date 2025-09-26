using System.Threading;
using UnityEngine;

namespace Player
{
    public class PlayerWallJumpState : PlayerBaseState
    {
        private float wallJumpTime = 0f;
        private float wallJumpDuration = 0.2f;

        public PlayerWallJumpState(PlayerController controller) : base(PlayerStateType.WallJump)
        {
            playerController = controller;
        }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("벽점프 상태 진입");
            wallJumpTime = 0f;
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("벽점프 상태 종료");
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            wallJumpTime += deltaTime;
            if (wallJumpTime >= wallJumpDuration)
            {
                playerController.ChangeState(PlayerStateType.Jump);
            }
        }
    }
}