using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 벽 잡기 상태
    /// 플레이어가 벽에 매달려 있을 때의 상태
    /// </summary>
    public class PlayerWallGrabState : PlayerBaseState
    {
        private float wallSlideSpeed = 2f;
        private float wallGrabDuration = 1f;
        private float wallGrabTime = 0f;

        public PlayerWallGrabState() : base(PlayerStateType.WallGrab) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("벽 잡기 상태 진입");

            // 벽 잡기 초기화
            wallGrabTime = 0f;

            // TODO: PlayerStats에서 벽 슬라이드 속도 가져오기
            wallSlideSpeed = 2f;
            wallGrabDuration = 1f;

            // 벽 잡기 시 수평 속도 제거
            if (playerController != null)
            {
                Vector2 velocity = playerController.Velocity;
                velocity.x = 0;
                playerController.SetVelocity(velocity);
            }

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("벽 잡기 상태 종료");
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            wallGrabTime += deltaTime;

            // 벽 슬라이딩 처리
            HandleWallSliding();

            // 상태 전환 체크
            CheckForStateTransitions();
        }

        private void HandleWallSliding()
        {
            if (playerController == null || playerController == null) return;

            Vector2 input = playerController.GetInputVector();

            // 벽 방향으로 입력이 있을 때만 벽에 붙어있기
            bool holdingTowardsWall = (playerController.FacingDirection > 0 && input.x > 0) ||
                                     (playerController.FacingDirection < 0 && input.x < 0);

            if (holdingTowardsWall)
            {
                // 벽에 붙어서 천천히 슬라이딩
                Vector2 velocity = playerController.Velocity;
                velocity.x = 0;

                if (velocity.y < 0)
                {
                    velocity.y = Mathf.Max(velocity.y, -wallSlideSpeed);
                }

                playerController.SetVelocity(velocity);
            }
            else
            {
                // 벽에서 떨어짐
                StateMachine?.ForceTransitionTo(PlayerStateType.Fall.ToString());
            }
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null) return;

            // 벽에서 떨어졌으면 Fall 상태로
            if (!playerController.IsTouchingWall)
            {
                StateMachine?.ForceTransitionTo(PlayerStateType.Fall.ToString());
                return;
            }

            // 땅에 닿으면 상태 전환
            if (playerController.IsGrounded)
            {
                Vector2 input = playerController.GetInputVector();
                if (Mathf.Abs(input.x) > 0.1f)
                {
                    StateMachine?.ForceTransitionTo(PlayerStateType.Move.ToString());
                }
                else
                {
                    StateMachine?.ForceTransitionTo(PlayerStateType.Idle.ToString());
                }
                return;
            }

            // 점프 입력이 있으면 벽 점프
            if (playerController.IsJumpPressed())
            {
                StateMachine?.ForceTransitionTo(PlayerStateType.WallJump.ToString());
                return;
            }

            // 대시 입력이 있으면 Dash 상태로 전환
            if (playerController.IsDashPressed() && playerController.CanDash)
            {
                StateMachine?.ForceTransitionTo(PlayerStateType.Dash.ToString());
                return;
            }

            // 일정 시간 후 자동으로 떨어짐
            if (wallGrabTime > wallGrabDuration)
            {
                StateMachine?.ForceTransitionTo(PlayerStateType.Fall.ToString());
                return;
            }
        }
    }
}
