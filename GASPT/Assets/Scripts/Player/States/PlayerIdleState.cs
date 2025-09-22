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

            // 대기 상태에서는 수평 이동 정지
            if (rb != null)
            {
                Vector2 velocity = rb.linearVelocity;
                velocity.x = Mathf.Lerp(velocity.x, 0, Time.fixedDeltaTime * 10f); // 부드럽게 정지
                rb.linearVelocity = velocity;
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
            // 중력 적용
            ApplyGravity();

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
            if (!playerController.IsGrounded && rb.linearVelocity.y < -0.1f)
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