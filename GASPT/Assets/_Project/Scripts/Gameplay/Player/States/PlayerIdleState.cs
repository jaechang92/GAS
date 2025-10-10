using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 대기 상태 (리팩토링됨)
    /// 플레이어가 움직이지 않고 대기할 때의 상태
    /// </summary>
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerController controller) : base(PlayerStateType.Idle)
        {
            playerController = controller;
        }

        protected override void EnterStateSync()
        {
            LogStateDebug("대기 상태 진입(동기)");
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("대기 상태 종료(동기)");
        }

        protected override void UpdateState(float deltaTime)
        {
            // PhysicsEngine이 InputHandler로부터 입력을 받아 자동으로 처리
            // State에서는 상태 전환 로직만 관리

            CheckForStateTransitions();
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null) return;

            // 모든 상태 전환은 PlayerController의 이벤트 시스템에 의해 자동 처리
            // - StartMove 이벤트 → Move 상태
            // - JumpPressed 이벤트 → Jump 상태
            // - DashPressed 이벤트 → Dash 상태
            // - AttackPressed 이벤트 → Attack 상태

            // 땅에서 떨어지면 공중 상태로 전환 체크
            if (!playerController.IsGrounded && playerController.Velocity.y < -0.1f)
            {
                // Fall 이벤트가 있다면 발생, 없다면 Jump 상태로 전환
                playerController.ChangeState(PlayerStateType.Jump);
            }
        }
    }
}