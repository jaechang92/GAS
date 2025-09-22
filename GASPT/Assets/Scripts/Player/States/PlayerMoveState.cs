using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 이동 상태
    /// 플레이어가 좌우로 움직일 때의 상태
    /// </summary>
    public class PlayerMoveState : PlayerBaseState
    {
        private float moveSpeed = 8f;

        public PlayerMoveState() : base(PlayerStateType.Move) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("이동 상태 진입");

            // PlayerStats에서 이동 속도 가져오기 (있다면)
            if (playerController != null)
            {
                // TODO: PlayerStats 연결 시 여기서 속도 설정
                moveSpeed = 8f; // 기본값
            }

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("이동 상태 종료");
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 이동 처리
            HandleMovement();

            // 중력 적용
            ApplyGravity();

            // 상태 전환 체크
            CheckForStateTransitions();
        }

        private void HandleMovement()
        {
            if (playerController == null || rb == null) return;

            Vector2 input = playerController.GetInputVector();

            // 부드러운 이동 적용
            Vector2 velocity = rb.linearVelocity;
            float targetVelocityX = input.x * moveSpeed;

            // 가속/감속 적용
            float acceleration = 50f;
            velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, acceleration * Time.fixedDeltaTime);

            rb.linearVelocity = velocity;
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null) return;

            // 이동 입력이 없으면 Idle 상태로 전환
            Vector2 input = playerController.GetInputVector();
            if (Mathf.Abs(input.x) < 0.1f)
            {
                // Idle 상태로의 전환은 PlayerEventType.StopMove 이벤트에 의해 자동 처리됨
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

            // 슬라이딩 입력이 있으면 Slide 상태로 전환
            // (슬라이딩은 이동 중에만 가능)
            // SlidePressed 이벤트에 의해 자동 처리됨
        }
    }
}