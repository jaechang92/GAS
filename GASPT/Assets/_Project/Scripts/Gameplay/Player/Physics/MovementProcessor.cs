using UnityEngine;
using System;

namespace Character.Physics
{
    /// <summary>
    /// 이동 처리 시스템
    /// 단일 책임: 입력 기반 이동, 점프, 대시 계산만 담당
    /// </summary>
    public class MovementProcessor : MonoBehaviour, IMovementProcessor
    {
        [Header("현재 상태")]
        [SerializeField] private float horizontalInput;
        [SerializeField] private Vector3 finalVelocity;

        // 점프 상태
        private JumpState jumpState;
        private DashState dashState;

        // 설정
        private CharacterPhysicsConfig config;

        // 이벤트
        public event Action OnJump;
        public event Action OnDash;

        #region IMovementProcessor 구현

        public bool CanDash => dashState.CanDash;
        public bool IsDashing => dashState.IsDashing;
        public IJumpState JumpState => jumpState;
        public IDashState DashState => dashState;

        #endregion

        private void Awake()
        {
            jumpState = new JumpState();
            dashState = new DashState();
        }

        public void Initialize(CharacterPhysicsConfig physicsConfig)
        {
            config = physicsConfig;
            jumpState.Initialize(config);
            dashState.Initialize(config);
        }

        public void SetHorizontalInput(float input)
        {
            horizontalInput = input;
        }

        public void SetJumpInput(bool pressed, bool held)
        {
            jumpState.SetInput(pressed, held);
        }

        public void PerformDash(Vector2 direction)
        {
            if (dashState.CanPerformDash())
            {
                dashState.StartDash(direction);
                OnDash?.Invoke();
            }
        }

        public void ProcessMovement(IPhysicsState physicsState, float deltaTime)
        {
            if (config == null) return;

            // 입력을 물리 상태에 전달
            physicsState.SetHorizontalInput(horizontalInput);

            Vector3 inputVelocity = physicsState.Velocity;
            finalVelocity = inputVelocity;

            if (config.enableDebugLogs)
            {
                Debug.Log($"[MovementProcessor] 입력 velocity: {inputVelocity}, isGrounded: {physicsState.IsGrounded}");
            }

            // 수평 이동 처리
            ProcessHorizontalMovement(physicsState, deltaTime);

            // 점프 처리
            ProcessJump(physicsState, deltaTime);

            // 대시 처리
            ProcessDash(physicsState, deltaTime);

            if (config.enableDebugLogs)
            {
                Debug.Log($"[MovementProcessor] 최종 velocity: {finalVelocity}");
            }

            // 상태 업데이트
            jumpState.UpdateState(physicsState, deltaTime);
            dashState.UpdateState(physicsState, deltaTime);
        }

        public Vector3 GetFinalVelocity()
        {
            return finalVelocity;
        }

        #region 이동 처리 메서드

        private void ProcessHorizontalMovement(IPhysicsState physicsState, float deltaTime)
        {
            if (dashState.IsDashing)
            {
                // 대시 중에는 수평 이동 무시
                return;
            }

            // 기존 시스템처럼 즉시 속도 변경 (가속도 제거)
            float targetSpeed = horizontalInput * physicsState.MovementSpeed;
            finalVelocity.x = targetSpeed;

            // 간단한 마찰력 적용 (접지 상태에서만)
            if (Mathf.Abs(horizontalInput) < 0.1f && physicsState.IsGrounded)
            {
                finalVelocity.x = 0f;
            }
        }

        private void ProcessJump(IPhysicsState physicsState, float deltaTime)
        {
            // 점프 입력 처리
            if (jumpState.ShouldJump(physicsState))
            {
                PerformJump(physicsState);
            }

            // 가변 점프 높이 처리 (점프 중일 때만)
            if (jumpState.IsJumping && jumpState.ShouldCutJump())
            {
                if (finalVelocity.y > 0)
                {
                    finalVelocity.y *= config.jumpCutMultiplier;
                }
            }
        }

        private void ProcessDash(IPhysicsState physicsState, float deltaTime)
        {
            if (dashState.IsDashing)
            {
                // 대시 속도 적용
                var dashVelocity = dashState.GetDashVelocity();
                finalVelocity.x = dashVelocity.x;
                finalVelocity.y = dashVelocity.y;

                // 대시 중에는 중력 무시
                if (config.ignorGravityDuringDash)
                {
                    finalVelocity.y = dashVelocity.y;
                }
            }
        }

        private void PerformJump(IPhysicsState physicsState)
        {
            if (physicsState.IsGrounded || physicsState.CanCoyoteJump)
            {
                // 일반 점프
                finalVelocity.y = config.jumpForce;
                jumpState.ConsumeJump(JumpType.Ground);
            }
            else if (physicsState.CanWallJump && jumpState.CanWallJump())
            {
                // 벽 점프
                finalVelocity.y = config.jumpForce * config.wallJumpForceMultiplier;
                finalVelocity.x = -physicsState.WallDirection * config.wallJumpHorizontalForce;
                jumpState.ConsumeJump(JumpType.Wall);
            }
            else if (jumpState.CanDoubleJump())
            {
                // 더블 점프
                finalVelocity.y = config.jumpForce * config.doubleJumpForceMultiplier;
                jumpState.ConsumeJump(JumpType.Double);
            }

            OnJump?.Invoke();
        }

        #endregion
    }

    #region 상태 클래스들

    /// <summary>
    /// 점프 상태 관리
    /// </summary>
    [System.Serializable]
    public class JumpState : IJumpState
    {
        [SerializeField] private bool isJumping;
        [SerializeField] private bool jumpPressed;
        [SerializeField] private bool jumpHeld;
        [SerializeField] private float jumpBufferTimer;
        [SerializeField] private bool hasDoubleJump;
        [SerializeField] private int jumpCount;
        [SerializeField] private float jumpTime;

        private CharacterPhysicsConfig config;

        // IJumpState 구현
        public bool IsJumping => isJumping;
        public bool JumpPressed => jumpPressed;
        public bool JumpHeld => jumpHeld;
        public float JumpBufferTime => jumpBufferTimer;
        public bool HasDoubleJump => hasDoubleJump;
        public int JumpCount => jumpCount;
        public float JumpTime => jumpTime;

        public void Initialize(CharacterPhysicsConfig physicsConfig)
        {
            config = physicsConfig;
            Reset();
        }

        public void SetInput(bool pressed, bool held)
        {
            if (pressed)
            {
                jumpPressed = true;
                jumpBufferTimer = config?.jumpBufferTime ?? 0.1f;
            }
            jumpHeld = held;
        }

        public void UpdateState(IPhysicsState physicsState, float deltaTime)
        {
            // 점프 버퍼 타이머 감소
            if (jumpBufferTimer > 0)
            {
                jumpBufferTimer -= deltaTime;
            }

            // 접지 상태에서 더블 점프 리셋
            if (physicsState.IsGrounded)
            {
                hasDoubleJump = config?.allowDoubleJump ?? false;
                jumpCount = 0;
                isJumping = false;
                jumpTime = 0;
            }

            // 점프 시간 업데이트
            if (isJumping)
            {
                jumpTime += deltaTime;
            }

            // 점프 입력 리셋 (매 프레임 후)
            jumpPressed = false;
        }

        public bool ShouldJump(IPhysicsState physicsState)
        {
            return jumpBufferTimer > 0 && (
                physicsState.IsGrounded ||
                physicsState.CanCoyoteJump ||
                CanDoubleJump() ||
                CanWallJump()
            );
        }

        public bool ShouldCutJump()
        {
            return isJumping && !jumpHeld && jumpTime > config?.minJumpTime;
        }

        public bool CanDoubleJump()
        {
            return hasDoubleJump && config?.allowDoubleJump == true;
        }

        public bool CanWallJump()
        {
            return config?.allowWallJump == true;
        }

        public void ConsumeJump(JumpType jumpType)
        {
            isJumping = true;
            jumpTime = 0;
            jumpCount++;
            jumpBufferTimer = 0;

            switch (jumpType)
            {
                case JumpType.Double:
                    hasDoubleJump = false;
                    break;
            }
        }

        private void Reset()
        {
            isJumping = false;
            jumpPressed = false;
            jumpHeld = false;
            jumpBufferTimer = 0;
            hasDoubleJump = config?.allowDoubleJump ?? false;
            jumpCount = 0;
            jumpTime = 0;
        }
    }

    /// <summary>
    /// 대시 상태 관리
    /// </summary>
    [System.Serializable]
    public class DashState : IDashState
    {
        [SerializeField] private bool isDashing;
        [SerializeField] private Vector2 dashDirection;
        [SerializeField] private float dashTime;
        [SerializeField] private float dashCooldown;
        [SerializeField] private int dashCount;

        private CharacterPhysicsConfig config;

        // IDashState 구현
        public bool IsDashing => isDashing;
        public bool CanDash => dashCooldown <= 0 && (config?.canDashInAir == true || dashCount < config?.maxDashCount);
        public Vector2 DashDirection => dashDirection;
        public float DashTime => dashTime;
        public float DashCooldown => dashCooldown;
        public int DashCount => dashCount;

        public void Initialize(CharacterPhysicsConfig physicsConfig)
        {
            config = physicsConfig;
            Reset();
        }

        public void UpdateState(IPhysicsState physicsState, float deltaTime)
        {
            // 대시 쿨다운 감소
            if (dashCooldown > 0)
            {
                dashCooldown -= deltaTime;
            }

            // 대시 시간 업데이트
            if (isDashing)
            {
                dashTime += deltaTime;

                // 대시 지속시간 체크
                if (dashTime >= config?.dashDuration)
                {
                    EndDash();
                }
            }

            // 접지 상태에서 대시 카운트 리셋
            if (physicsState.IsGrounded)
            {
                dashCount = 0;
            }
        }

        public bool CanPerformDash()
        {
            return CanDash && !isDashing;
        }

        public void StartDash(Vector2 direction)
        {
            if (!CanPerformDash()) return;

            isDashing = true;
            dashDirection = direction.normalized;
            dashTime = 0;
            dashCooldown = config?.dashCooldown ?? 1f;
            dashCount++;
        }

        public Vector2 GetDashVelocity()
        {
            if (!isDashing) return Vector2.zero;

            float speed = config?.dashSpeed ?? 15f;
            return dashDirection * speed;
        }

        private void EndDash()
        {
            isDashing = false;
            dashTime = 0;
        }

        private void Reset()
        {
            isDashing = false;
            dashDirection = Vector2.zero;
            dashTime = 0;
            dashCooldown = 0;
            dashCount = 0;
        }
    }

    public enum JumpType
    {
        Ground,
        Wall,
        Double
    }

    #endregion
}