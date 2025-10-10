using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 낙하 상태 (리팩토링됨)
    /// 플레이어가 공중에서 떨어질 때의 상태
    /// </summary>
    public class PlayerFallState : PlayerBaseState
    {
        public PlayerFallState(PlayerController controller) : base(PlayerStateType.Fall)
        {
            playerController = controller;
        }

        protected override void EnterStateSync()
        {
            LogStateDebug("낙하 상태 진입(동기)");
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("낙하 상태 종료(동기)");
        }

        protected override void UpdateState(float deltaTime)
        {
            // PhysicsEngine이 낙하 물리를 자동으로 처리
            CheckForStateTransitions();
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null) return;

            // 땅에 착지하면 상태 전환
            if (playerController.IsGrounded)
            {
                if (Mathf.Abs(playerController.Velocity.x) > 0.1f)
                    playerController.ChangeState(PlayerStateType.Move);
                else
                    playerController.ChangeState(PlayerStateType.Idle);
            }

            // 벽에 닿으면 WallGrab 상태로 전환
            if (playerController.IsTouchingWall && playerController.Velocity.y < 0)
            {
                playerController.ChangeState(PlayerStateType.WallGrab);
            }
        }
    }
}