using UnityEngine;

namespace Player
{
    public class PlayerDeadState : PlayerBaseState
    {
        public PlayerDeadState(PlayerController controller) : base(PlayerStateType.Dead)
        {
            playerController = controller;
        }

        protected override void EnterStateSync()
        {
            LogStateDebug("사망 상태 진입(동기)");
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("사망 상태 종료(동기)");
        }

        protected override void UpdateState(float deltaTime)
        {
            // 사망 상태에서는 아무것도 하지 않음
        }
    }
}