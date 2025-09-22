using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 점프 상태
    /// 플레이어가 점프할 때의 상태
    /// </summary>
    public class PlayerJumpState : PlayerBaseState
    {
        private float jumpForce = 15f;
        private float jumpTime = 0f;
        private float maxJumpTime = 0.3f;
        private bool jumpReleased = false;

        public PlayerJumpState() : base(PlayerStateType.Jump) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("점프 상태 진입");

            // 점프 초기화
            jumpTime = 0f;
            jumpReleased = false;

            // 점프 실행
            if (rb != null)
            {
                // TODO: PlayerStats에서 점프력 가져오기
                jumpForce = 15f;

                // 벽 점프인지 일반 점프인지 확인
                if (playerController.IsTouchingWall && !playerController.IsGrounded)
                {
                    // 벽 점프
                    Vector2 velocity = rb.linearVelocity;
                    velocity.y = jumpForce * 0.8f; // 벽 점프는 좀 더 약하게
                    velocity.x = -playerController.FacingDirection * 8f; // 벽에서 밀어내기
                    rb.linearVelocity = velocity;
                }
                else
                {
                    // 일반 점프
                    ApplyJump(jumpForce);
                }

                // 수평 이동 유지
                ApplyMovement(8f);
            }

            // 점프 입력 리셋
            playerController.ResetJump();

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

            // 점프 중 수평 이동 처리
            HandleAirMovement();

            // 가변 점프 높이 처리
            HandleVariableJumpHeight();

            // 상태 전환 체크
            CheckForStateTransitions();
        }

        private void HandleAirMovement()
        {
            if (playerController == null || rb == null) return;

            // 공중에서의 이동 (지상보다 약간 느림)
            Vector2 input = playerController.GetInputVector();
            Vector2 velocity = rb.linearVelocity;

            float airMoveSpeed = 6f; // 공중 이동 속도
            float airAcceleration = 30f; // 공중 가속도

            float targetVelocityX = input.x * airMoveSpeed;
            velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, airAcceleration * Time.fixedDeltaTime);

            rb.linearVelocity = velocity;
        }

        private void HandleVariableJumpHeight()
        {
            if (rb == null || playerController == null) return;

            // 점프 버튼을 놓으면 상승 속도 감소
            if (!playerController.IsJumpPressed() && !jumpReleased)
            {
                jumpReleased = true;

                if (rb.linearVelocity.y > 0)
                {
                    Vector2 velocity = rb.linearVelocity;
                    velocity.y *= 0.5f; // 점프 높이 감소
                    rb.linearVelocity = velocity;
                }
            }

            // 최대 점프 시간 초과 시 강제로 하강
            if (jumpTime > maxJumpTime)
            {
                jumpReleased = true;
            }

            // 중력 적용 (상승 중일 때는 약하게, 하강 중일 때는 강하게)
            if (rb.linearVelocity.y > 0 && !jumpReleased)
            {
                ApplyGravity(1f); // 일반 중력
            }
            else
            {
                ApplyGravity(2f); // 강화된 중력
            }
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null || rb == null) return;

            // 땅에 착지하면 상태 전환
            if (playerController.IsGrounded)
            {
                // TouchGround 이벤트에 의해 자동으로 Idle 또는 Move 상태로 전환됨
                return;
            }

            // 하강 중이면 Fall 상태로 전환
            if (rb.linearVelocity.y < -0.5f)
            {
                // 자동으로 Fall 상태로 전환
                StateMachine?.ForceTransitionTo(PlayerStateType.Fall.ToString());
                return;
            }

            // 벽에 닿으면 Wall Grab 상태로 전환 (조건 확인)
            if (playerController.IsTouchingWall && rb.linearVelocity.y < 0)
            {
                Vector2 input = playerController.GetInputVector();
                // 벽 방향으로 입력이 있을 때만 벽 잡기
                if ((playerController.FacingDirection > 0 && input.x > 0) ||
                    (playerController.FacingDirection < 0 && input.x < 0))
                {
                    StateMachine?.ForceTransitionTo(PlayerStateType.WallGrab.ToString());
                    return;
                }
            }

            // 대시 입력이 있으면 Dash 상태로 전환
            if (playerController.IsDashPressed() && playerController.CanDash)
            {
                // Dash 상태로의 전환은 PlayerEventType.DashPressed 이벤트에 의해 자동 처리됨
                return;
            }

            // 공격 입력이 있으면 Attack 상태로 전환
            if (playerController.IsAttackPressed())
            {
                // Attack 상태로의 전환은 PlayerEventType.AttackPressed 이벤트에 의해 자동 처리됨
                return;
            }
        }
    }
}