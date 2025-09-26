using System.Threading;
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

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("슬라이드 상태 진입");
            slideTime = 0f;
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("슬라이드 상태 종료");
            await Awaitable.NextFrameAsync();
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