using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 점프 상태 (리팩토링됨)
    /// 플레이어가 점프할 때의 상태
    /// </summary>
    public class PlayerJumpState : PlayerBaseState
    {
        private float jumpTime = 0f;
        private float minJumpTime = 0.1f; // 최소 점프 시간

        public PlayerJumpState(PlayerController controller) : base(PlayerStateType.Jump)
        {
            playerController = controller;
        }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("점프 상태 진입");

            jumpTime = 0f;

            // PhysicsEngine이 점프를 자동으로 처리함
            // State에서는 진입 시 특별한 로직만 수행

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("점프 상태 종료");
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            jumpTime += deltaTime;

            // PhysicsEngine이 공중 이동, 중력, 점프 높이 조절을 모두 처리
            // State에서는 상태 전환 로직만 관리

            CheckForStateTransitions();
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null) return;

            // 최소 점프 시간이 지나야 상태 전환 허용
            if (jumpTime < minJumpTime) return;

            // 접지 상태가 되면 적절한 상태로 전환 (더 안정적인 조건)
            if (playerController.IsGrounded && playerController.Velocity.y <= 0.5f)
            {
                // 추가 안정성 체크: 일정 시간 이상 접지 상태여야 전환
                if (jumpTime > minJumpTime + 0.1f) // 최소 점프 시간 + 0.1초 더 대기
                {
                    // 이동 입력이 있으면 Move, 없으면 Idle
                    if (Mathf.Abs(playerController.Velocity.x) > 0.1f)
                        playerController.ChangeState(PlayerStateType.Move);
                    else
                        playerController.ChangeState(PlayerStateType.Idle);
                }
            }

            // 벽에 닿으면 WallGrab 상태로 전환 (벽점프 가능한 경우)
            if (playerController.IsTouchingWall && playerController.Velocity.y < -1f) // 더 확실한 하강 상태일 때만
            {
                playerController.ChangeState(PlayerStateType.WallGrab);
            }

            // 확실히 낙하 중일 때 Fall 상태로 전환
            if (playerController.Velocity.y < -5f && !playerController.IsGrounded)
            {
                playerController.ChangeState(PlayerStateType.Fall);
            }
        }
    }
}