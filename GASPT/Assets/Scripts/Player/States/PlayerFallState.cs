using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 낙하 상태
    /// 플레이어가 공중에서 떨어질 때의 상태
    /// </summary>
    public class PlayerFallState : PlayerBaseState
    {
        private float coyoteTime = 0.1f; // 땅에서 떨어진 후 점프 가능한 시간
        private float coyoteTimeCounter = 0f;
        private bool wasGrounded = false;

        public PlayerFallState() : base(PlayerStateType.Fall) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("낙하 상태 진입");

            // 코요테 타임 초기화
            coyoteTimeCounter = coyoteTime;
            wasGrounded = true; // Fall 상태로 들어온 것은 이전에 땅에 있었다는 뜻

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("낙하 상태 종료");
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 코요테 타임 감소
            if (coyoteTimeCounter > 0)
            {
                coyoteTimeCounter -= deltaTime;
            }

            // 공중 이동 처리
            HandleAirMovement();

            // 강화된 중력 적용 (더 빠른 낙하)
            ApplyGravity(2.5f);

            // 최대 낙하 속도 제한
            LimitFallSpeed();

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
            float airAcceleration = 25f; // 공중 가속도 (Jump보다 약간 느림)

            float targetVelocityX = input.x * airMoveSpeed;
            velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, airAcceleration * Time.fixedDeltaTime);

            rb.linearVelocity = velocity;
        }

        private void LimitFallSpeed()
        {
            if (rb == null) return;

            float maxFallSpeed = 20f; // 최대 낙하 속도

            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                Vector2 velocity = rb.linearVelocity;
                velocity.y = -maxFallSpeed;
                rb.linearVelocity = velocity;
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

            // 코요테 타임 내에 점프 입력이 있으면 점프 허용
            if (playerController.IsJumpPressed() && coyoteTimeCounter > 0)
            {
                StateMachine?.ForceTransitionTo(PlayerStateType.Jump.ToString());
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