using UnityEngine;
using System;
using Core.Managers;

namespace Player.Physics
{
    /// <summary>
    /// Skul 스타일 캐릭터 물리 시스템
    /// 단일 컴포넌트로 모든 물리 처리를 담당
    /// Rigidbody2D 기반의 즉시 반응형 물리 시스템
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class CharacterPhysics : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private SkulPhysicsConfig configOverride;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;

        // 실제 사용할 config (프로퍼티로 자동 로드)
        private SkulPhysicsConfig config
        {
            get
            {
                // Inspector에서 설정된 override가 있으면 사용
                if (configOverride != null)
                {
                    return configOverride;
                }

                // 없으면 ResourceManager에서 로드
                return ResourceManager.GetSkulPhysicsConfig();
            }
        }

        // === 컴포넌트 참조 ===
        private Rigidbody2D rb;
        private BoxCollider2D col;

        // === 입력 상태 ===
        private float horizontalInput;
        private bool jumpPressed;
        private bool jumpHeld;
        private float jumpHoldTime;

        // === 물리 상태 ===
        private bool isGrounded;
        private bool isTouchingWall;
        private bool isTouchingLeftWall;
        private bool isTouchingRightWall;
        private int wallDirection; // -1: 왼쪽, 1: 오른쪽, 0: 없음

        // === 타이머 ===
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private float dashCooldownTimer;
        private float wallJumpCooldownTimer;
        private float wallStickTimer;

        // === 대시 상태 ===
        private bool isDashing;
        private float dashTimer;
        private Vector2 dashDirection;
        private int airDashesUsed;

        // === 점프 상태 ===
        private bool isJumping;
        private bool hasJumped;

        // === 프로퍼티 ===
        public bool IsGrounded => isGrounded;
        public bool IsTouchingWall => isTouchingWall;
        public bool CanJump => (isGrounded || coyoteTimeCounter > 0) && !hasJumped;
        public bool CanDash => dashCooldownTimer <= 0 && (!config.canDashInAir || airDashesUsed < config.maxAirDashes || isGrounded);
        public bool IsDashing => isDashing;
        public Vector2 Velocity => rb.linearVelocity;
        public int WallDirection => wallDirection;

        // === 이벤트 ===
        public event Action OnGroundedChanged;
        public event Action OnJump;
        public event Action OnDash;
        public event Action OnWallTouch;

        #region Unity 생명주기

        private void Awake()
        {
            InitializeComponents();
            ValidateConfiguration();
        }

        private void Start()
        {
            InitializePhysics();
        }

        private void Update()
        {
            UpdateTimers(Time.deltaTime);
            CheckCollisions();
        }

        private void FixedUpdate()
        {
            HandlePhysics(Time.fixedDeltaTime);
        }

        #endregion

        #region 초기화

        private void InitializeComponents()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<BoxCollider2D>();

            if (rb == null || col == null)
            {
                Debug.LogError($"[CharacterPhysics] 필수 컴포넌트가 없습니다! {gameObject.name}");
            }
        }

        private void ValidateConfiguration()
        {
            if (config == null)
            {
                Debug.LogError($"[CharacterPhysics] SkulPhysicsConfig를 로드할 수 없습니다! {gameObject.name}");
                Debug.LogError($"[CharacterPhysics] ResourceManager가 초기화되었는지 확인하세요!");
                return;
            }

            // Inspector override 사용 여부 로그
            if (configOverride != null)
            {
                LogDebug("Inspector에서 설정된 SkulPhysicsConfig 사용");
            }
            else
            {
                LogDebug("ResourceManager에서 SkulPhysicsConfig 로드");
            }

            config.ValidateSettings();
        }

        private void InitializePhysics()
        {
            if (rb == null) return;

            // Rigidbody2D 설정
            rb.gravityScale = 0f; // 커스텀 중력 사용
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            LogDebug("물리 시스템 초기화 완료");
        }

        #endregion

        #region 입력 인터페이스

        /// <summary>
        /// 수평 이동 입력 설정
        /// </summary>
        public void SetHorizontalInput(float input)
        {
            horizontalInput = Mathf.Clamp(input, -1f, 1f);
        }

        /// <summary>
        /// 점프 입력 설정
        /// </summary>
        public void SetJumpInput(bool pressed, bool held)
        {
            if (pressed && !jumpPressed)
            {
                jumpBufferCounter = config.jumpBufferTime;
                jumpHoldTime = 0f;
            }

            jumpPressed = pressed;
            jumpHeld = held;

            if (jumpHeld)
            {
                jumpHoldTime += Time.deltaTime;
            }
        }

        /// <summary>
        /// 대시 실행
        /// </summary>
        public void PerformDash(Vector2 direction)
        {
            if (!CanDash || direction.magnitude < 0.1f) return;

            StartDash(direction.normalized);
        }

        #endregion

        #region 물리 처리

        private void HandlePhysics(float deltaTime)
        {
            if (config == null) return;

            // 대시 처리가 최우선
            if (isDashing)
            {
                HandleDash(deltaTime);
                return;
            }

            // 일반 물리 처리
            HandleMovement(deltaTime);
            HandleJump(deltaTime);
            HandleGravity(deltaTime);
            HandleWallInteraction(deltaTime);
        }

        private void HandleMovement(float deltaTime)
        {
            if (Mathf.Abs(horizontalInput) < 0.01f)
            {
                // 입력 없음 -> 즉시 정지 (지면/공중 모두)
                StopHorizontalMovement();
                return;
            }

            // 입력 있음 -> 즉시 반응 (가속도 없이 직접 속도 설정)
            float targetSpeed = horizontalInput * (isGrounded ? config.moveSpeed : config.airMoveSpeed);
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }

        private void StopHorizontalMovement()
        {
            // 지면과 공중 모두에서 즉시 정지 - 수평 속도만 0으로 설정
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        private void HandleJump(float deltaTime)
        {
            // 점프 실행 조건 체크
            if (jumpBufferCounter > 0 && CanJump)
            {
                ExecuteJump();
            }

            // 점프 키를 떼면 상승 속도 감소 (가변 점프 높이)
            if (!jumpHeld && isJumping && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * config.jumpCutMultiplier);
                isJumping = false;
            }

            // 점프 홀드 시간에 따른 점프 높이 조절
            if (isJumping && jumpHeld && jumpHoldTime < config.maxJumpHoldTime)
            {
                float jumpForceMultiplier = Mathf.Lerp(1f, 1.5f, jumpHoldTime / config.maxJumpHoldTime);
                float additionalForce = config.jumpVelocity * jumpForceMultiplier * deltaTime * 2f;
                rb.AddForce(Vector2.up * additionalForce, ForceMode2D.Force);
            }
        }

        private void ExecuteJump()
        {
            // 점프 힘 계산 (홀드 시간 고려)
            float jumpForce = Mathf.Lerp(config.minJumpVelocity, config.jumpVelocity,
                Mathf.Clamp01(jumpHoldTime / config.minJumpHoldTime));

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // 상태 업데이트
            isJumping = true;
            hasJumped = true;
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;

            OnJump?.Invoke();
            LogDebug($"점프 실행 - 힘: {jumpForce:F1}");
        }

        private void HandleGravity(float deltaTime)
        {
            if (isDashing && config.dashIgnoreGravity) return;

            float gravityAcceleration = config.gravity;

            // 상황별 중력 조절
            if (rb.linearVelocity.y < 0) // 떨어지는 중
            {
                gravityAcceleration *= config.fallGravityMultiplier;
            }
            else if (rb.linearVelocity.y > 0 && !jumpHeld) // 상승 중이지만 점프키를 떼었을 때
            {
                gravityAcceleration *= config.lowJumpGravityMultiplier;
            }

            // 직접 속도 변경으로 더 강한 중력 효과
            float newYVelocity = rb.linearVelocity.y - gravityAcceleration * deltaTime;

            // 최대 낙하 속도 제한
            newYVelocity = Mathf.Max(newYVelocity, -config.maxFallSpeed);

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newYVelocity);
        }

        private void HandleWallInteraction(float deltaTime)
        {
            if (!isTouchingWall) return;

            // 벽 슬라이딩
            if (rb.linearVelocity.y < 0 && !isGrounded)
            {
                float slideSpeed = Mathf.Max(rb.linearVelocity.y, config.wallSlideSpeed);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, slideSpeed);
            }

            // 벽 점프 처리
            if (jumpBufferCounter > 0 && wallJumpCooldownTimer <= 0)
            {
                ExecuteWallJump();
            }
        }

        private void ExecuteWallJump()
        {
            Vector2 wallJumpForce = new Vector2(
                config.wallJumpVelocity.x * -wallDirection,
                config.wallJumpVelocity.y
            );

            rb.linearVelocity = wallJumpForce;

            // 상태 업데이트
            wallJumpCooldownTimer = config.wallJumpCooldown;
            jumpBufferCounter = 0f;
            hasJumped = true;

            OnJump?.Invoke();
            LogDebug($"벽점프 실행 - 방향: {wallDirection}, 힘: {wallJumpForce}");
        }

        private void HandleDash(float deltaTime)
        {
            dashTimer -= deltaTime;

            if (dashTimer <= 0)
            {
                EndDash();
                return;
            }

            // 대시 속도 유지
            rb.linearVelocity = dashDirection * config.dashSpeed;
        }

        private void StartDash(Vector2 direction)
        {
            isDashing = true;
            dashTimer = config.dashDuration;
            dashDirection = direction;
            dashCooldownTimer = config.dashCooldown;

            // 공중 대시 카운터 증가
            if (!isGrounded)
            {
                airDashesUsed++;
            }

            OnDash?.Invoke();
            LogDebug($"대시 시작 - 방향: {direction}");
        }

        private void EndDash()
        {
            isDashing = false;
            dashTimer = 0f;

            // 대시 후 속도 조절 (관성 유지)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.7f, rb.linearVelocity.y * 0.5f);

            LogDebug("대시 종료");
        }

        #endregion

        #region 충돌 감지

        private void CheckCollisions()
        {
            CheckGroundCollision();
            CheckWallCollision();
        }

        private void CheckGroundCollision()
        {
            bool wasGrounded = isGrounded;

            // 바닥 체크를 위한 레이캐스트
            Vector2 boxCenter = (Vector2)transform.position + col.offset;
            Vector2 boxSize = new Vector2(config.groundCheckWidth, config.groundCheckDistance);
            Vector2 boxPosition = boxCenter + Vector2.down * (col.size.y * 0.5f + config.groundCheckDistance * 0.5f);

            isGrounded = Physics2D.OverlapBox(boxPosition, boxSize, 0f, config.groundLayerMask);

            // 접지 상태 변경 처리
            if (wasGrounded != isGrounded)
            {
                OnGroundedStateChanged();
            }
        }

        private void CheckWallCollision()
        {
            bool wasTouchingWall = isTouchingWall;

            Vector2 boxCenter = (Vector2)transform.position + col.offset;
            float checkDistance = config.wallCheckDistance;

            // 왼쪽 벽 체크
            Vector2 leftBoxPosition = boxCenter + Vector2.left * (col.size.x * 0.5f + checkDistance * 0.5f);
            isTouchingLeftWall = Physics2D.OverlapBox(leftBoxPosition,
                new Vector2(checkDistance, col.size.y * 0.8f), 0f, config.wallLayerMask);

            // 오른쪽 벽 체크
            Vector2 rightBoxPosition = boxCenter + Vector2.right * (col.size.x * 0.5f + checkDistance * 0.5f);
            isTouchingRightWall = Physics2D.OverlapBox(rightBoxPosition,
                new Vector2(checkDistance, col.size.y * 0.8f), 0f, config.wallLayerMask);

            // 벽 상태 업데이트
            isTouchingWall = isTouchingLeftWall || isTouchingRightWall;

            if (isTouchingLeftWall && !isTouchingRightWall)
                wallDirection = -1;
            else if (!isTouchingLeftWall && isTouchingRightWall)
                wallDirection = 1;
            else
                wallDirection = 0;

            // 벽 터치 상태 변경 처리
            if (wasTouchingWall != isTouchingWall && isTouchingWall)
            {
                OnWallTouch?.Invoke();
            }
        }

        private void OnGroundedStateChanged()
        {
            if (isGrounded)
            {
                // 착지 시
                hasJumped = false;
                isJumping = false;
                airDashesUsed = 0; // 공중 대시 카운터 리셋
                coyoteTimeCounter = config.coyoteTime;
                LogDebug("착지");
            }
            else
            {
                // 공중으로 나갈 때
                if (!hasJumped) // 점프가 아닌 낙하
                {
                    coyoteTimeCounter = config.coyoteTime;
                }
                LogDebug("공중 상태");
            }

            OnGroundedChanged?.Invoke();
        }

        #endregion

        #region 타이머 업데이트

        private void UpdateTimers(float deltaTime)
        {
            // 코요테 타임
            if (coyoteTimeCounter > 0)
                coyoteTimeCounter -= deltaTime;

            // 점프 버퍼
            if (jumpBufferCounter > 0)
                jumpBufferCounter -= deltaTime;

            // 대시 쿨다운
            if (dashCooldownTimer > 0)
                dashCooldownTimer -= deltaTime;

            // 벽점프 쿨다운
            if (wallJumpCooldownTimer > 0)
                wallJumpCooldownTimer -= deltaTime;

            // 벽 붙기 타이머
            if (wallStickTimer > 0)
                wallStickTimer -= deltaTime;
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 속도 직접 설정
        /// </summary>
        public void SetVelocity(Vector2 newVelocity)
        {
            if (rb != null)
            {
                rb.linearVelocity = newVelocity;
            }
        }

        /// <summary>
        /// 힘 추가
        /// </summary>
        public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
        {
            if (rb != null)
            {
                rb.AddForce(force, mode);
            }
        }

        #endregion

        #region 디버그

        private void LogDebug(string message)
        {
            if (enableDebugLogs || (config != null && config.enableDebugLogs))
            {
                Debug.Log($"[CharacterPhysics] {message}");
            }
        }

        private void OnDrawGizmos()
        {
            if (config == null || !config.showPhysicsGizmos) return;

            if (col == null) return;

            Vector2 boxCenter = (Vector2)transform.position + col.offset;

            // 바닥 체크 영역
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector2 groundCheckPos = boxCenter + Vector2.down * (col.size.y * 0.5f + config.groundCheckDistance * 0.5f);
            Gizmos.DrawWireCube(groundCheckPos, new Vector2(config.groundCheckWidth, config.groundCheckDistance));

            // 벽 체크 영역
            Gizmos.color = isTouchingLeftWall ? Color.blue : Color.cyan;
            Vector2 leftWallCheckPos = boxCenter + Vector2.left * (col.size.x * 0.5f + config.wallCheckDistance * 0.5f);
            Gizmos.DrawWireCube(leftWallCheckPos, new Vector2(config.wallCheckDistance, col.size.y * 0.8f));

            Gizmos.color = isTouchingRightWall ? Color.blue : Color.cyan;
            Vector2 rightWallCheckPos = boxCenter + Vector2.right * (col.size.x * 0.5f + config.wallCheckDistance * 0.5f);
            Gizmos.DrawWireCube(rightWallCheckPos, new Vector2(config.wallCheckDistance, col.size.y * 0.8f));

            // 대시 상태 표시
            if (isDashing)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }

            // 속도 벡터
            if (rb != null && rb.linearVelocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)rb.linearVelocity * 0.1f);
            }
        }

        #endregion
    }
}
