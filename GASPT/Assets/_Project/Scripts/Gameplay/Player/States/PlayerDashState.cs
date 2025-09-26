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

            // PlayerController에서 대시 속도와 지속시간 가져오기
            dashSpeed = 20f;  // 기본값 사용
            dashDuration = 0.2f; // 기본값 사용

            // 대시 디버그 로그
            Debug.Log($"[Dash] 방향: {(dashDirection > 0 ? "오른쪽" : "왼쪽")} ({dashDirection}), 속도: {dashSpeed}, 지속시간: {dashDuration}");
            Debug.Log($"[Dash] 대시 전 속도: {playerController.Velocity}");

            // 대시 시작 (커스텀 물리 사용)
            ApplyDash(dashSpeed, dashDirection);
            Debug.Log($"[Dash] 대시 후 속도: {playerController.Velocity}");

            // 대시 쿨다운 시작
            playerController.StartDash();

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("대시 상태 종료");

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
            if (playerController == null) return;

            // 대시 속도를 일정하게 유지 (커스텀 물리 사용)
            Vector3 velocity = playerController.Velocity;
            float targetVelocityX = dashSpeed * dashDirection;
            velocity.x = targetVelocityX;
            velocity.y = 0; // 수평 대시이므로 Y축 속도는 0
            playerController.SetVelocity(velocity);

            // 주기적으로 속도 확인 (0.05초마다)
            if (Time.fixedTime % 0.05f < Time.fixedDeltaTime)
            {
                Debug.Log($"[Dash] 유지 중 - 목표속도: {targetVelocityX}, 현재속도: {playerController.Velocity.x}, 시간: {dashTime:F2}/{dashDuration:F2}");
            }
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