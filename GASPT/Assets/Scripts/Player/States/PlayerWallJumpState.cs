using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 벽 점프 상태
    /// 플레이어가 벽에서 점프할 때의 상태
    /// </summary>
    public class PlayerWallJumpState : PlayerBaseState
    {
        private float wallJumpForce = 12f;
        private float wallJumpHorizontalForce = 8f;
        private float wallJumpTime = 0f;
        private float wallJumpDuration = 0.3f;
        private bool inputLocked = true; // 초기에는 입력 제한

        public PlayerWallJumpState() : base(PlayerStateType.WallJump) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("벽 점프 상태 진입");

            // 벽 점프 초기화
            wallJumpTime = 0f;
            inputLocked = true;

            // TODO: PlayerStats에서 벽 점프 설정 가져오기
            wallJumpForce = 12f;
            wallJumpHorizontalForce = 8f;
            wallJumpDuration = 0.3f;

            // 벽 점프 실행
            if (rb != null)
            {
                Vector2 velocity = rb.linearVelocity;

                // 벽의 반대 방향으로 점프
                int jumpDirection = -playerController.FacingDirection;

                velocity.y = wallJumpForce;
                velocity.x = wallJumpHorizontalForce * jumpDirection;

                rb.linearVelocity = velocity;
            }

            // 점프 입력 리셋
            playerController.ResetJump();

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("벽 점프 상태 종료");
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            wallJumpTime += deltaTime;

            // 일정 시간 후 입력 잠금 해제
            if (wallJumpTime > 0.1f && inputLocked)
            {
                inputLocked = false;
                LogStateDebug("벽 점프 입력 잠금 해제");
            }

            // 공중 이동 처리 (입력 잠금 해제 후)
            if (!inputLocked)
            {
                HandleAirMovement();
            }

            // 중력 적용
            ApplyGravity(1.5f);

            // 상태 전환 체크
            CheckForStateTransitions();
        }

        private void HandleAirMovement()
        {
            if (playerController == null || rb == null) return;

            // 벽 점프 후 공중 제어 (약간 제한적)
            Vector2 input = playerController.GetInputVector();
            Vector2 velocity = rb.linearVelocity;

            float airMoveSpeed = 5f; // 벽 점프 후 공중 이동 속도
            float airAcceleration = 20f; // 공중 가속도

            float targetVelocityX = input.x * airMoveSpeed;

            // 기존 수평 속도를 완전히 무시하지 않고 부드럽게 변경
            velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, airAcceleration * Time.fixedDeltaTime);

            rb.linearVelocity = velocity;
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null || rb == null) return;

            // 벽 점프 지속시간이 끝나면 Fall 상태로 전환
            if (wallJumpTime > wallJumpDuration)
            {
                if (rb.linearVelocity.y <= 0)
                {
                    StateMachine?.ForceTransitionTo(PlayerStateType.Fall.ToString());
                    return;
                }
            }

            // 땅에 착지하면 상태 전환
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

            // 하강 중이면 Fall 상태로 전환
            if (rb.linearVelocity.y < -0.5f)
            {
                StateMachine?.ForceTransitionTo(PlayerStateType.Fall.ToString());
                return;
            }

            // 다른 벽에 닿으면 다시 Wall Grab 상태로 (입력 잠금 해제 후)
            if (!inputLocked && playerController.IsTouchingWall && rb.linearVelocity.y < 0)
            {
                Vector2 input = playerController.GetInputVector();
                // 새로운 벽 방향으로 입력이 있을 때만
                if ((playerController.FacingDirection > 0 && input.x > 0) ||
                    (playerController.FacingDirection < 0 && input.x < 0))
                {
                    StateMachine?.ForceTransitionTo(PlayerStateType.WallGrab.ToString());
                    return;
                }
            }

            // 대시 입력이 있으면 Dash 상태로 전환 (입력 잠금 해제 후)
            if (!inputLocked && playerController.IsDashPressed() && playerController.CanDash)
            {
                StateMachine?.ForceTransitionTo(PlayerStateType.Dash.ToString());
                return;
            }

            // 공격 입력이 있으면 Attack 상태로 전환 (입력 잠금 해제 후)
            if (!inputLocked && playerController.IsAttackPressed())
            {
                StateMachine?.ForceTransitionTo(PlayerStateType.Attack.ToString());
                return;
            }
        }
    }
}