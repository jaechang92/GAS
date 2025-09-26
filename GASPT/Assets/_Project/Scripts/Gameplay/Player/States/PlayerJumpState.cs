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
        private float maxJumpTime = 0.5f; // 0.3f에서 0.5f로 증가 (더 긴 점프 허용)
        private bool jumpReleased = false;

        public PlayerJumpState() : base(PlayerStateType.Jump) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("점프 상태 진입");

            // 점프 초기화
            jumpTime = 0f;
            jumpReleased = false;

            // 점프 실행 (커스텀 물리 사용)
            jumpForce = playerController.JumpForce; // PlayerController에서 점프력 가져오기

            // 벽 점프인지 일반 점프인지 확인
            if (playerController.IsTouchingWall && !playerController.IsGrounded)
            {
                // 벽 점프
                Vector3 velocity = playerController.Velocity;
                velocity.y = jumpForce * 0.8f; // 벽 점프는 좀 더 약하게
                velocity.x = -playerController.FacingDirection * 8f; // 벽에서 밀어내기
                playerController.SetVelocity(velocity);
            }
            else
            {
                // 일반 점프
                ApplyJump(jumpForce);
            }

            // 점프 입력 리셋
            playerController.ResetJump();

            // 점프 시작 시 강제로 접지 상태 해제 (TouchGround 이벤트 발생 보장)
            playerController.ForceSetGrounded(false);

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

            // 중력
            ApplyGravity();

            // 상태 전환 체크
            CheckForStateTransitions();
        }

        private void HandleAirMovement()
        {
            if (playerController == null) return;

            // 공중에서의 이동 (커스텀 물리 사용)
            Vector2 input = playerController.GetInputVector();
            float airMoveSpeed = playerController.AirMoveSpeed;

            // 커스텀 물리를 통한 공중 이동
            playerController.SetHorizontalMovement(input.x, airMoveSpeed);
        }

        private void HandleVariableJumpHeight()
        {
            if (playerController == null) return;

            // 점프 버튼을 놓으면 상승 속도 감소
            if (!playerController.IsJumpPressed() && !jumpReleased)
            {
                jumpReleased = true;

                Vector3 velocity = playerController.Velocity;
                if (velocity.y > 0)
                {
                    velocity.y *= 0.5f; // 점프 높이 감소
                    playerController.SetVelocity(velocity);
                }
            }

            // 최대 점프 시간 초과 시 강제로 하강
            if (jumpTime > maxJumpTime)
            {
                jumpReleased = true;
            }

            // Jump 상태에서는 접지 상태와 관계없이 중력 적용 (강제 적용)
            if (playerController.Velocity.y > 0 && !jumpReleased)
            {
                // 상승 중일 때는 약하게
                playerController.ForceApplyGravity(1f);
            }
            else
            {
                // 하강 중일 때는 강하게
                playerController.ForceApplyGravity(2f);
            }
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null || playerController == null) return;

            // 점프 최소 시간 보장 (0.15초 이상 경과 후에만 상태 전환 허용)
            if (jumpTime < 0.15f) return;

            // 땅에 착지하면 상태 전환 (단, 하강 중이고 충분히 떨어진 후에만)
            if (playerController.IsGrounded && playerController.Velocity.y <= -0.1f && jumpTime > 0.2f)
            {
                // TouchGround 이벤트에 의해 자동으로 Idle 또는 Move 상태로 전환됨
                return;
            }

            // 하강 중이면 Fall 상태로 전환 (더 엄격한 조건)
            if (playerController.Velocity.y < -1.5f && jumpTime > 0.2f)
            {
                // 자동으로 Fall 상태로 전환
                StateMachine?.ForceTransitionTo(PlayerStateType.Fall.ToString());
                return;
            }

            // 벽에 닿으면 Wall Grab 상태로 전환 (조건 확인)
            if (playerController.IsTouchingWall && playerController.Velocity.y < 0)
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
