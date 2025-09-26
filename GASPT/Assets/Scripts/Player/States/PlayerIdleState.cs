using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 대기 상태
    /// 플레이어가 움직이지 않을 때의 기본 상태
    /// </summary>
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState() : base(PlayerStateType.Idle) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("대기 상태 진입");

            // 점프에서 착지했을 때만 운동량 보존 체크
            if (playerController != null)
            {
                var prevState = playerController.GetType().GetField("currentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                // 움직이지 않으면 대기 상태에서는 수평 이동 즉시 정지
                Vector2 input = playerController.GetInputVector();
                if (Mathf.Abs(input.x) < 0.1f)
                {
                    StopHorizontalMovement();
                }
            }

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("대기 상태 종료");
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // GroundChecker가 FixedUpdate에서 자동으로 지면 체크 수행

            // Idle 상태에서는 중력 적용하지 않음 (접지 상태이므로)
            // ApplyGravity(); // 제거됨

            // 입력 체크
            CheckForStateTransitions();
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null) return;

            // 이동 입력이 있으면 Move 상태로 전환
            Vector2 input = playerController.GetInputVector();
            if (Mathf.Abs(input.x) > 0.1f)
            {
                // Move 상태로의 전환은 PlayerEventType.StartMove 이벤트에 의해 자동 처리됨
                return;
            }

            // 점프 입력이 있고 땅에 있으면 Jump 상태로 전환
            if (playerController.IsJumpPressed() && playerController.IsGrounded)
            {
                // Jump 상태로의 전환은 PlayerEventType.JumpPressed 이벤트에 의해 자동 처리됨
                return;
            }

            // 땅에서 떨어지면 Fall 상태로 전환
            if (!playerController.IsGrounded && playerController.Velocity.y < -0.1f)
            {
                // Fall 상태로의 전환은 PlayerEventType.LeaveGround 이벤트에 의해 자동 처리됨
                return;
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