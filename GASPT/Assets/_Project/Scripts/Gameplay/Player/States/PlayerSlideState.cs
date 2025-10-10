using UnityEngine;

namespace Player
{
    public class PlayerSlideState : PlayerBaseState
    {
        private float slideDuration = 0.8f;
        private float slideTime = 0f;

        public PlayerSlideState(PlayerController controller) : base(PlayerStateType.Slide)
        {
            playerController = controller;
        }

        protected override void EnterStateSync()
        {
            LogStateDebug("슬라이드 상태 진입(동기)");
            slideTime = 0f;
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("슬라이드 상태 종료(동기)");
        }

        protected override void UpdateState(float deltaTime)
        {
            slideTime += deltaTime;
            if (slideTime >= slideDuration || !playerController.IsGrounded)
            {
                playerController.ChangeState(PlayerStateType.Idle);
            }
        }
    }
}