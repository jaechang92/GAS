using UnityEngine;

namespace Player
{
    public class PlayerWallGrabState : PlayerBaseState
    {
        public PlayerWallGrabState(PlayerController controller) : base(PlayerStateType.WallGrab)
        {
            playerController = controller;
        }

        protected override void EnterStateSync()
        {
            LogStateDebug("벽잡기 상태 진입(동기)");
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("벽잡기 상태 종료(동기)");
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