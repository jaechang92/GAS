using System.Threading;
using UnityEngine;

namespace Player
{
    public class PlayerDeadState : PlayerBaseState
    {
        public PlayerDeadState(PlayerController controller) : base(PlayerStateType.Dead)
        {
            playerController = controller;
        }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("사망 상태 진입");
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("사망 상태 종료");
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 사망 상태에서는 아무것도 하지 않음
        }
    }
}