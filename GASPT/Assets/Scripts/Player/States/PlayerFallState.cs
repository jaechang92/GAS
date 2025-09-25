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
            Debug.Log(playerController.PreviousState);
            //UnityEditor.EditorApplication.isPaused = true;


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
            // GroundChecker가 FixedUpdate에서 자동으로 지면 체크 수행

            // 코요테 타임 감소
            if (coyoteTimeCounter > 0)
            {
                coyoteTimeCounter -= deltaTime;
            }

            // 공중 이동 처리
            HandleAirMovement();

            // Fall 상태에서는 접지 상태와 관계없이 강화된 중력 적용 (강제 적용)
            //playerController.ForceApplyGravity(2.5f);

            ApplyGravity();

            // 최대 낙하 속도 제한 (ForceApplyGravity에서 이미 처리되므로 제거)
            // LimitFallSpeed(); // 제거됨

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

        private void LimitFallSpeed()
        {
            // 최대 낙하 속도 제한은 PlayerController.ApplyGravity()에서 처리됨
            // 여기서는 추가 제한 없이 기본 중력 시스템 사용
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null) return;

            // 땅에 착지하면 상태 전환 (하강 중일 때만 진짜 착지)
            if (playerController.IsGrounded && playerController.Velocity.y <= 0.1f)
            {
                // 입력 상태에 따른 상태 전환 결정
                Vector2 input = playerController.GetInputVector();

                if (Mathf.Abs(input.x) > 0.1f)
                {
                    // 이동 입력이 있으면 Move 상태로
                    LogStateDebug("착지 - Move 상태로 전환");
                    StateMachine?.ForceTransitionTo(PlayerStateType.Move.ToString());
                }
                else
                {
                    // 이동 입력이 없으면 Idle 상태로
                    LogStateDebug("착지 - Idle 상태로 전환");
                    StateMachine?.ForceTransitionTo(PlayerStateType.Idle.ToString());
                }
                return;
            }

            // 코요테 타임 내에 점프 입력이 있으면 점프 허용
            if (playerController.IsJumpPressed() && coyoteTimeCounter > 0)
            {
                StateMachine?.ForceTransitionTo(PlayerStateType.Jump.ToString());
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
