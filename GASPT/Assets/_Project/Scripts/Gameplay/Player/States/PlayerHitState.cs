using System.Threading;
using UnityEngine;

namespace Player
{
    public class PlayerHitState : PlayerBaseState
    {
        private float hitDuration = 0.5f;
        private float hitTime = 0f;

        public PlayerHitState(PlayerController controller) : base(PlayerStateType.Hit)
        {
            playerController = controller;
        }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("피격 상태 진입");
            hitTime = 0f;
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("피격 상태 종료");
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            hitTime += deltaTime;
            if (hitTime >= hitDuration)
            {
                playerController.ChangeState(PlayerStateType.Idle);
            }
        }
    }
}