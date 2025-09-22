using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 대시 상태
    /// 플레이어가 대시할 때의 상태
    /// </summary>
    public class PlayerDashState : PlayerBaseState
    {
        private float dashSpeed = 20f;
        private float dashDuration = 0.2f;
        private float dashTime = 0f;
        private int dashDirection = 1;

        public PlayerDashState() : base(PlayerStateType.Dash) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("대시 상태 진입");

            // 대시 초기화
            dashTime = 0f;
            dashDirection = playerController.FacingDirection;

            // TODO: PlayerStats에서 대시 속도와 지속시간 가져오기
            dashSpeed = 20f;
            dashDuration = 0.2f;

            // 대시 시작
            if (rb != null)
            {
                // 중력 무시하고 수평으로 빠르게 이동
                rb.gravityScale = 0f;
                ApplyDash(dashSpeed, dashDirection);
            }

            // 대시 쿨다운 시작
            playerController.StartDash();

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("대시 상태 종료");

            // 중력 복구
            if (rb != null)
            {
                rb.gravityScale = 3f; // 기본 중력으로 복구
            }

            // 대시 입력 리셋
            playerController.ResetDash();

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            dashTime += deltaTime;

            // 대시 지속시간 확인
            if (dashTime >= dashDuration)
            {
                // 대시 완료
                CompleteDash();
                return;
            }

            // 대시 중에는 이동 입력 무시하고 직진
            MaintainDashMovement();

            // 조기 종료 조건 체크
            CheckForEarlyExit();
        }

        private void MaintainDashMovement()
        {
            if (rb == null) return;

            // 대시 속도를 일정하게 유지
            Vector2 velocity = rb.linearVelocity;
            velocity.x = dashSpeed * dashDirection;
            velocity.y = 0; // 수평 대시이므로 Y축 속도는 0
            rb.linearVelocity = velocity;
        }

        private void CheckForEarlyExit()
        {
            if (playerController == null) return;

            // 벽에 부딪히면 대시 종료
            if (playerController.IsTouchingWall)
            {
                CompleteDash();
                return;
            }
        }

        private void CompleteDash()
        {
            // 대시 완료 후 상태 결정
            if (playerController.IsGrounded)
            {
                // 땅에 있으면 이동 입력에 따라 상태 결정
                Vector2 input = playerController.GetInputVector();
                if (Mathf.Abs(input.x) > 0.1f)
                {
                    StateMachine?.ForceTransitionTo(PlayerStateType.Move.ToString());
                }
                else
                {
                    StateMachine?.ForceTransitionTo(PlayerStateType.Idle.ToString());
                }
            }
            else
            {
                // 공중에 있으면 낙하 상태로
                StateMachine?.ForceTransitionTo(PlayerStateType.Fall.ToString());
            }
        }
    }
}