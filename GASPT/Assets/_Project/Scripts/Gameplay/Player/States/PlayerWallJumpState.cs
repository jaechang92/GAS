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

        protected override void EnterStateSync()
        {
            LogStateDebug("벽점프 상태 진입(동기)");
            wallJumpTime = 0f;
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("벽점프 상태 종료(동기)");
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