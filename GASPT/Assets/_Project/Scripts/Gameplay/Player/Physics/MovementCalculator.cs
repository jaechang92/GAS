using UnityEngine;

namespace Character.Physics
{
    /// <summary>
    /// Skul 스타일 캐릭터 이동 계산 시스템
    /// 무중력 고정 속도 기반 플랫폼 액션 물리 엔진
    /// </summary>
    public class MovementCalculator : MonoBehaviour
    {
        [Header("물리 설정")]
        [SerializeField] private CharacterPhysicsConfig config;

        [Header("현재 상태")]
        [SerializeField] private Vector3 velocity;
        [SerializeField] private bool isGrounded;
        [SerializeField] private bool isTouchingWall;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;

        // 컴포넌트 참조
        private RaycastController raycastController;

        // 상태 추적
        private bool jumpPressed;
        private bool jumpHeld;
        private float jumpBufferTimer;
        private float coyoteTimer;
        private bool hasDoubleJump;
        private float dashTimer;
        private float dashCooldownTimer;
        private Vector2 dashDirection;
        private bool isDashing;

        // 이벤트
        public System.Action<Vector3> OnVelocityChanged;
        public System.Action OnGroundedChanged;
        public System.Action OnJump;
        public System.Action OnDash;

        // 프로퍼티
        public Vector3 Velocity => velocity;
        public bool IsGrounded => isGrounded;
        public bool IsTouchingWall => isTouchingWall;
        public bool IsDashing => isDashing;
        public bool CanDash => dashCooldownTimer <= 0 && (IsGrounded || config.canDashInAir);

        private void Awake()
        {
            raycastController = GetComponent<RaycastController>();
            if (raycastController == null)
            {
                Debug.LogError("[MovementCalculator] RaycastController가 필요합니다!");
            }

            if (config == null)
            {
                Debug.LogWarning("[MovementCalculator] CharacterPhysicsConfig가 할당되지 않았습니다!");
            }
        }

        private void Update()
        {
            UpdateTimers();
            UpdatePhysics();
            UpdateMovement();
        }

        /// <summary>
        /// 타이머 업데이트
        /// </summary>
        private void UpdateTimers()
        {
            // 점프 버퍼 타이머
            if (jumpBufferTimer > 0)
                jumpBufferTimer -= Time.deltaTime;

            // 코요테 타이머
            if (coyoteTimer > 0)
                coyoteTimer -= Time.deltaTime;

            // 대시 타이머
            if (dashTimer > 0)
                dashTimer -= Time.deltaTime;
            else if (isDashing)
                EndDash();

            // 대시 쿨다운
            if (dashCooldownTimer > 0)
                dashCooldownTimer -= Time.deltaTime;
        }

        /// <summary>
        /// 물리 상태 업데이트
        /// </summary>
        private void UpdatePhysics()
        {
            if (config == null) return;

            bool wasGrounded = isGrounded;
            isGrounded = raycastController.IsGrounded();
            isTouchingWall = raycastController.IsTouchingWall();

            // 접지 상태 변경 시 이벤트 호출
            if (wasGrounded != isGrounded)
            {
                OnGroundedStateChanged(isGrounded);
            }

            // 대시 중이 아닐 때만 중력 적용
            if (!isDashing)
            {
                ApplyGravity();
            }

            // 점프 처리
            HandleJump();

            // 속도 제한
            LimitVelocity();
        }

        /// <summary>
        /// 이동 처리
        /// </summary>
        private void UpdateMovement()
        {
            if (raycastController != null)
            {
                raycastController.Move(velocity * Time.deltaTime);
                OnVelocityChanged?.Invoke(velocity);
            }
        }

        /// <summary>
        /// 중력 적용
        /// </summary>
        private void ApplyGravity()
        {
            if (config == null) return;

            if (!isGrounded)
            {
                float gravityMultiplier = 1f;

                // 점프 높이 조절을 위한 중력 조정
                if (velocity.y < 0)
                {
                    gravityMultiplier = config.fallMultiplier;
                }
                else if (velocity.y > 0 && !jumpHeld)
                {
                    gravityMultiplier = config.lowJumpMultiplier;
                }

                velocity.y -= config.gravity * gravityMultiplier * Time.deltaTime;
            }
        }

        /// <summary>
        /// 점프 처리
        /// </summary>
        private void HandleJump()
        {
            if (config == null) return;

            // 점프 버퍼 체크
            if (jumpPressed && jumpBufferTimer > 0)
            {
                if (isGrounded || coyoteTimer > 0)
                {
                    PerformJump();
                    jumpBufferTimer = 0;
                    coyoteTimer = 0;
                }
                else if (isTouchingWall && config.wallJumpForce > 0)
                {
                    PerformWallJump();
                    jumpBufferTimer = 0;
                }
                else if (hasDoubleJump)
                {
                    PerformDoubleJump();
                    jumpBufferTimer = 0;
                }
            }

            jumpPressed = false;
        }

        /// <summary>
        /// 속도 제한
        /// </summary>
        private void LimitVelocity()
        {
            if (config == null) return;

            // 최대 낙하 속도 제한
            if (velocity.y < config.maxFallSpeed)
            {
                velocity.y = config.maxFallSpeed;
            }

            // 퀵 폴 적용 시 더 빠른 낙하
            if (velocity.y < config.quickFallSpeed && Input.GetKey(KeyCode.S))
            {
                velocity.y = config.quickFallSpeed;
            }
        }

        /// <summary>
        /// 접지 상태 변경 처리
        /// </summary>
        private void OnGroundedStateChanged(bool grounded)
        {
            if (grounded)
            {
                // 착지 시 처리
                velocity.y = 0;
                hasDoubleJump = true;
                coyoteTimer = 0;

                if (enableDebugLogs)
                    Debug.Log("[MovementCalculator] 착지");
            }
            else
            {
                // 공중으로 나갈 때 코요테 타임 시작
                coyoteTimer = config.coyoteTime;

                if (enableDebugLogs)
                    Debug.Log("[MovementCalculator] 공중");
            }

            OnGroundedChanged?.Invoke();
        }

        /// <summary>
        /// 수평 이동 설정
        /// </summary>
        public void SetHorizontalMovement(float moveInput)
        {
            if (config == null || isDashing) return;

            float targetSpeed = moveInput * (isGrounded ? config.moveSpeed : config.airMoveSpeed);
            float acceleration = (Mathf.Abs(moveInput) > 0.1f) ? config.acceleration : config.deceleration;

            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, acceleration * Time.deltaTime);
        }

        /// <summary>
        /// 점프 입력 처리
        /// </summary>
        public void SetJumpInput(bool pressed, bool held)
        {
            if (pressed)
            {
                jumpPressed = true;
                jumpBufferTimer = config.jumpBufferTime;
            }
            jumpHeld = held;
        }

        /// <summary>
        /// 점프 실행
        /// </summary>
        private void PerformJump()
        {
            if (config == null) return;

            velocity.y = config.jumpVelocity;
            OnJump?.Invoke();

            if (enableDebugLogs)
                Debug.Log("[MovementCalculator] 점프");
        }

        /// <summary>
        /// 벽 점프 실행
        /// </summary>
        private void PerformWallJump()
        {
            if (config == null) return;

            float wallDirection = raycastController.collisions.left ? 1 : -1;
            Vector2 wallJumpVelocity = new Vector2(
                wallDirection * config.wallJumpForce * Mathf.Cos(config.wallJumpAngle * Mathf.Deg2Rad),
                config.wallJumpForce * Mathf.Sin(config.wallJumpAngle * Mathf.Deg2Rad)
            );

            velocity = wallJumpVelocity;
            OnJump?.Invoke();

            if (enableDebugLogs)
                Debug.Log("[MovementCalculator] 벽 점프");
        }

        /// <summary>
        /// 더블 점프 실행
        /// </summary>
        private void PerformDoubleJump()
        {
            if (config == null) return;

            velocity.y = config.jumpVelocity;
            hasDoubleJump = false;
            OnJump?.Invoke();

            if (enableDebugLogs)
                Debug.Log("[MovementCalculator] 더블 점프");
        }

        /// <summary>
        /// 대시 실행
        /// </summary>
        public void PerformDash(Vector2 direction)
        {
            if (config == null || !CanDash) return;

            dashDirection = direction.normalized;
            dashTimer = config.dashDuration;
            dashCooldownTimer = config.dashCooldown;
            isDashing = true;

            velocity = dashDirection * config.dashSpeed;
            OnDash?.Invoke();

            if (enableDebugLogs)
                Debug.Log($"[MovementCalculator] 대시: {direction}");
        }

        /// <summary>
        /// 대시 종료
        /// </summary>
        private void EndDash()
        {
            isDashing = false;
            // 대시 종료 시 속도를 부드럽게 감소
            velocity *= 0.5f;

            if (enableDebugLogs)
                Debug.Log("[MovementCalculator] 대시 종료");
        }

        /// <summary>
        /// 원웨이 플랫폼 관통
        /// </summary>
        public void FallThroughPlatform()
        {
            if (raycastController != null)
            {
                raycastController.SetFallingThroughPlatform(true);
                Invoke(nameof(ResetFallingThroughPlatform), 0.3f);
            }
        }

        private void ResetFallingThroughPlatform()
        {
            if (raycastController != null)
            {
                raycastController.SetFallingThroughPlatform(false);
            }
        }

        /// <summary>
        /// 외부에서 속도 직접 설정
        /// </summary>
        public void SetVelocity(Vector3 newVelocity)
        {
            velocity = newVelocity;
            OnVelocityChanged?.Invoke(velocity);
        }

        /// <summary>
        /// 수직 속도만 설정
        /// </summary>
        public void SetVerticalVelocity(float yVelocity)
        {
            velocity.y = yVelocity;
            OnVelocityChanged?.Invoke(velocity);
        }

        /// <summary>
        /// 수평 속도만 설정
        /// </summary>
        public void SetHorizontalVelocity(float xVelocity)
        {
            velocity.x = xVelocity;
            OnVelocityChanged?.Invoke(velocity);
        }

        /// <summary>
        /// 강제로 접지 상태로 설정
        /// </summary>
        public void ForceGrounded()
        {
            velocity.y = 0;
            isGrounded = true;
            hasDoubleJump = true;
        }

        /// <summary>
        /// 디버그 정보 표시
        /// </summary>
        private void OnGUI()
        {
            if (!enableDebugLogs) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Velocity: {velocity}");
            GUILayout.Label($"Grounded: {isGrounded}");
            GUILayout.Label($"Touching Wall: {isTouchingWall}");
            GUILayout.Label($"Can Dash: {CanDash}");
            GUILayout.Label($"Is Dashing: {isDashing}");
            GUILayout.Label($"Coyote Timer: {coyoteTimer:F2}");
            GUILayout.Label($"Jump Buffer: {jumpBufferTimer:F2}");
            GUILayout.EndArea();
        }
    }
}