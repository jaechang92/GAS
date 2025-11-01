using UnityEngine;
using System;
using System.Collections.Generic;
using Core.Managers;
using Gameplay.Environment;
using Gameplay.Common;
using Gameplay.Player.Physics;

namespace Player.Physics
{
    /// <summary>
    /// 벽 감지 데이터 구조체
    /// </summary>
    public struct WallDetectionData
    {
        public bool isOnWall;
        public WallDirection wallDirection;
        public Vector2 wallNormal;
        public float distanceToWall;
        public RaycastHit2D wallHit;

        public WallDetectionData(bool onWall, WallDirection direction, Vector2 normal, float distance, RaycastHit2D hit)
        {
            isOnWall = onWall;
            wallDirection = direction;
            wallNormal = normal;
            distanceToWall = distance;
            wallHit = hit;
        }
    }

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
        [SerializeField] private SkullMovementProfile defaultProfile;

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
                // 없으면 GameResourceManager에서 로드 (제네릭 메서드 사용)
                configOverride = GameResourceManager.GetResource<SkulPhysicsConfig>("Data/SkulPhysicsConfig");
                return configOverride;
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
        private bool downPressed; // 아래 방향 입력 (플랫폼 통과용)

        // === 물리 상태 ===
        private bool isGrounded;
        private bool isTouchingWall;
        private bool isTouchingLeftWall;
        private bool isTouchingRightWall;
        private WallDirection wallDirection; // 벽 방향 (None, Left, Right)
        private bool isWallSliding; // 벽 슬라이딩 상태
        private WallDetectionData currentWallData; // 현재 벽 감지 데이터

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

        // === 플랫폼 상호작용 ===
        private readonly Dictionary<Collider2D, float> activePlatformCooldowns = new Dictionary<Collider2D, float>();

        // === 스컬 프로필 ===
        private SkullMovementProfile currentSkullProfile;

        // === 점프 상태 ===
        private bool isJumping;
        private bool hasJumped;

        // === 프로퍼티 ===
        public bool IsGrounded => isGrounded;
        public bool IsTouchingWall => isTouchingWall;
        public bool IsWallSliding => isWallSliding;
        public bool CanJump => (isGrounded || coyoteTimeCounter > 0) && !hasJumped;
        public bool CanWallJump => isTouchingWall && !isGrounded && wallJumpCooldownTimer <= 0;
        public bool CanDash => dashCooldownTimer <= 0 && (!config.canDashInAir || airDashesUsed < config.maxAirDashes || isGrounded);
        public bool IsDashing => isDashing;
        public Vector2 Velocity => rb.linearVelocity;
        public WallDirection WallDirectionState => wallDirection;
        public WallDetectionData CurrentWallData => currentWallData;
        public SkullMovementProfile CurrentSkullProfile => currentSkullProfile;

        // === 이벤트 ===
        public event Action OnGroundedChanged;
        public event Action OnJump;
        public event Action OnDash;
        public event Action OnWallTouch;
        public event Action OnWallSlideStart;
        public event Action OnWallSlideEnd;
        public event Action OnWallJump;

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

        private void OnDestroy()
        {
            // SkullManager 이벤트 구독 해제
            var skullManager = FindSkullManager();
            if (skullManager != null)
            {
                skullManager.OnSkullChanged -= HandleSkullChanged;
            }
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
                Debug.LogError($"[CharacterPhysics] SkulPhysicsConfig를 로드할 수 없습니다! GameObject: {gameObject.name}");
                Debug.LogError($"[CharacterPhysics] 해결 방법:");
                Debug.LogError($"  1. GameResourceManager가 초기화되었는지 확인 (Bootstrap 씬 로드 필요)");
                Debug.LogError($"  2. Resources/Data/SkulPhysicsConfig.asset 파일이 존재하는지 확인");
                Debug.LogError($"  3. 또는 Inspector에서 Config Override를 직접 설정");
                Debug.LogError($"[CharacterPhysics] ⚠️ 물리 시스템이 작동하지 않습니다! (점프, 이동, 중력 모두 비활성화)");
                return;
            }

            // Inspector override 사용 여부 로그
            if (configOverride != null)
            {
                LogDebug("Inspector에서 설정된 SkulPhysicsConfig 사용");
            }
            else
            {
                LogDebug("ResourceManager에서 SkulPhysicsConfig 로드 성공");
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

            // 기본 스컬 프로필 적용
            if (defaultProfile != null)
            {
                ApplySkullProfile(defaultProfile);
            }
            else
            {
                LogDebug("경고: 기본 스컬 프로필이 설정되지 않았습니다.");
            }

            // Skull System 이벤트 구독
            SubscribeToSkullSystem();

            LogDebug("물리 시스템 초기화 완료");
        }

        /// <summary>
        /// Skull System 이벤트 구독
        /// </summary>
        private void SubscribeToSkullSystem()
        {
            // SkullManager 찾기 (싱글톤)
            var skullManager = FindSkullManager();

            if (skullManager != null)
            {
                skullManager.OnSkullChanged += HandleSkullChanged;
                LogDebug("SkullManager 이벤트 구독 완료");
            }
            else
            {
                LogDebug("SkullManager를 찾을 수 없습니다. 스컬 변경 이벤트 구독 불가.");
            }
        }

        /// <summary>
        /// 스컬 변경 이벤트 핸들러
        /// </summary>
        private void HandleSkullChanged(ISkullController previousSkull, ISkullController newSkull)
        {
            if (newSkull == null || newSkull.SkullData == null) return;

            // 새 스컬의 SkullMovementProfile 가져오기
            // 현재는 스컬 이름 기반으로 프로필을 찾는 로직이 필요
            // 일단 기본 프로필 사용 (실제로는 스컬 데이터에서 가져와야 함)
            LogDebug($"스컬 변경 감지: {previousSkull?.SkullData?.SkullName} -> {newSkull.SkullData?.SkullName}");

            // TODO: 실제로는 SkullData에서 SkullMovementProfile을 가져와서 적용
            // 현재는 기본 프로필 유지
        }

        /// <summary>
        /// SkullManager 찾기 (인터페이스를 통한 간접 참조)
        /// </summary>
        private ISkullManager FindSkullManager()
        {
            // FindObjectsByType을 사용하여 ISkullManager를 구현한 MonoBehaviour 찾기
            var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var mb in allMonoBehaviours)
            {
                if (mb is ISkullManager skullManager)
                {
                    return skullManager;
                }
            }
            return null;
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

                // 아래 방향 + 점프 입력 동시 감지 (플랫폼 통과)
                if (downPressed)
                {
                    RequestPlatformPassthrough();
                }
            }

            jumpPressed = pressed;
            jumpHeld = held;

            if (jumpHeld)
            {
                jumpHoldTime += Time.deltaTime;
            }
        }

        /// <summary>
        /// 아래 방향 입력 설정
        /// </summary>
        public void SetDownInput(bool pressed)
        {
            downPressed = pressed;
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
            UpdatePlatformCooldowns(deltaTime);
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
            float baseSpeed = isGrounded ? config.moveSpeed : config.airMoveSpeed;
            float modifiedSpeed = isGrounded ? GetModifiedSpeed(baseSpeed) : GetModifiedAirSpeed(baseSpeed);
            float targetSpeed = horizontalInput * modifiedSpeed;
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }

        private void StopHorizontalMovement()
        {
            // 지면과 공중 모두에서 즉시 정지 - 수평 속도만 0으로 설정
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        private void HandleJump(float deltaTime)
        {
            // 접지 상태에서 hasJumped 리셋 (안전장치 - 좁은 공간 점프 문제 해결)
            if (isGrounded && hasJumped && rb.linearVelocity.y <= 0.1f)
            {
                hasJumped = false;
                isJumping = false;
                LogDebug("점프 상태 강제 리셋 (접지됨)");
            }

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

            // 하강 시작하면 isJumping 종료 (천장 충돌 대응)
            if (isJumping && rb.linearVelocity.y <= 0)
            {
                isJumping = false;
                LogDebug("점프 상승 종료 (하강 시작)");
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
            float baseJumpForce = Mathf.Lerp(config.minJumpVelocity, config.jumpVelocity,
                Mathf.Clamp01(jumpHoldTime / config.minJumpHoldTime));

            // 스컬 프로필 배율 적용
            float jumpForce = GetModifiedJumpForce(baseJumpForce);

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
            // 벽 슬라이딩 시작/종료 조건 체크
            bool shouldBeWallSliding = isTouchingWall && rb.linearVelocity.y < 0 && !isGrounded;

            // 벽 슬라이딩 상태 변경 처리
            if (shouldBeWallSliding && !isWallSliding)
            {
                StartWallSlide();
            }
            else if (!shouldBeWallSliding && isWallSliding)
            {
                StopWallSlide();
            }

            // 벽 슬라이딩 중 속도 제어
            if (isWallSliding)
            {
                float slideSpeed = Mathf.Max(rb.linearVelocity.y, config.wallSlideSpeed);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, slideSpeed);
            }

            // 벽 점프 처리
            if (jumpBufferCounter > 0 && CanWallJump)
            {
                ExecuteWallJump();
            }
        }

        /// <summary>
        /// 벽 슬라이딩 시작
        /// </summary>
        private void StartWallSlide()
        {
            isWallSliding = true;
            OnWallSlideStart?.Invoke();
            LogDebug($"벽 슬라이딩 시작 - 방향: {wallDirection}");
        }

        /// <summary>
        /// 벽 슬라이딩 종료
        /// </summary>
        private void StopWallSlide()
        {
            isWallSliding = false;
            OnWallSlideEnd?.Invoke();
            LogDebug("벽 슬라이딩 종료");
        }

        private void ExecuteWallJump()
        {
            Vector2 baseWallJumpForce = new Vector2(
                config.wallJumpVelocity.x * -(int)wallDirection,
                config.wallJumpVelocity.y
            );

            // 스컬 프로필 배율 적용
            Vector2 wallJumpForce = GetModifiedWallJumpVelocity(baseWallJumpForce);

            rb.linearVelocity = wallJumpForce;

            // 상태 업데이트
            isWallSliding = false; // 벽 점프 시 슬라이딩 종료
            wallJumpCooldownTimer = config.wallJumpCooldown;
            jumpBufferCounter = 0f;
            hasJumped = true;

            OnJump?.Invoke();
            OnWallJump?.Invoke();
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

        #region 플랫폼 상호작용

        /// <summary>
        /// 일방향 플랫폼 통과 요청
        /// 아래 방향 입력 + 점프 입력이 동시에 눌렸을 때 호출
        /// </summary>
        public void RequestPlatformPassthrough()
        {
            // 현재 접촉 중인 모든 OneWayPlatform 찾기
            Collider2D[] colliders = Physics2D.OverlapBoxAll(
                (Vector2)transform.position + col.offset,
                col.size,
                0f
            );

            foreach (var hitCollider in colliders)
            {
                // 자기 자신 제외
                if (hitCollider == col) continue;

                // OneWayPlatform 컴포넌트 확인
                var platform = hitCollider.GetComponent<OneWayPlatform>();
                if (platform != null && platform.Type == PlatformType.OneWay)
                {
                    // 플랫폼 통과 요청
                    platform.RequestPassthrough(col);
                    LogDebug($"플랫폼 통과 요청: {hitCollider.gameObject.name}");
                }
            }
        }

        /// <summary>
        /// 플랫폼 쿨다운 타이머 업데이트
        /// </summary>
        private void UpdatePlatformCooldowns(float deltaTime)
        {
            // 만료된 플랫폼 수집
            var expiredPlatforms = new List<Collider2D>();

            foreach (var kvp in activePlatformCooldowns)
            {
                activePlatformCooldowns[kvp.Key] -= deltaTime;

                if (activePlatformCooldowns[kvp.Key] <= 0)
                {
                    expiredPlatforms.Add(kvp.Key);
                }
            }

            // 만료된 쿨다운 제거
            foreach (var platform in expiredPlatforms)
            {
                activePlatformCooldowns.Remove(platform);
            }
        }

        #endregion

        #region 스컬 프로필 관리

        /// <summary>
        /// 스컬 프로필 적용
        /// </summary>
        public void ApplySkullProfile(SkullMovementProfile profile)
        {
            // Null 체크 및 기본값 처리
            if (profile == null)
            {
                LogDebug("경고: Null 프로필이 전달되었습니다. 기본 프로필을 사용합니다.");
                profile = defaultProfile;

                if (profile == null)
                {
                    LogDebug("오류: 기본 프로필도 없습니다. 프로필 적용 실패.");
                    return;
                }
            }

            var previousProfile = currentSkullProfile;
            currentSkullProfile = profile;

            LogDebug($"스컬 프로필 적용: {profile.SkullName}");

            // 공중/벽 슬라이딩 중 스컬 변경 처리
            if (!isGrounded || isWallSliding)
            {
                // 현재 속도를 새 배율로 재조정
                if (previousProfile != null)
                {
                    // 수평 속도 재조정
                    float oldSpeedMultiplier = previousProfile.MoveSpeedMultiplier;
                    float newSpeedMultiplier = profile.MoveSpeedMultiplier;

                    if (oldSpeedMultiplier > 0)
                    {
                        float speedRatio = newSpeedMultiplier / oldSpeedMultiplier;
                        Vector2 currentVel = rb.linearVelocity;
                        rb.linearVelocity = new Vector2(currentVel.x * speedRatio, currentVel.y);

                        LogDebug($"공중 속도 재조정: {currentVel.x:F2} -> {rb.linearVelocity.x:F2}");
                    }
                }
            }
        }

        /// <summary>
        /// 프로필 배율이 적용된 이동 속도 반환
        /// </summary>
        private float GetModifiedSpeed(float baseSpeed)
        {
            if (currentSkullProfile == null) return baseSpeed;
            return baseSpeed * currentSkullProfile.MoveSpeedMultiplier;
        }

        /// <summary>
        /// 프로필 배율이 적용된 공중 이동 속도 반환
        /// </summary>
        private float GetModifiedAirSpeed(float baseSpeed)
        {
            if (currentSkullProfile == null) return baseSpeed;
            return baseSpeed * currentSkullProfile.AirControlMultiplier;
        }

        /// <summary>
        /// 프로필 배율이 적용된 점프력 반환
        /// </summary>
        private float GetModifiedJumpForce(float baseForce)
        {
            if (currentSkullProfile == null) return baseForce;
            return baseForce * currentSkullProfile.JumpHeightMultiplier;
        }

        /// <summary>
        /// 프로필 배율이 적용된 벽 점프 속도 반환
        /// </summary>
        private Vector2 GetModifiedWallJumpVelocity(Vector2 baseVelocity)
        {
            if (currentSkullProfile == null) return baseVelocity;

            return new Vector2(
                baseVelocity.x * currentSkullProfile.WallJumpHorizontalMultiplier,
                baseVelocity.y * currentSkullProfile.WallJumpVerticalMultiplier
            );
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

            // 모든 충돌체 감지 후 자기 자신 제외
            Collider2D[] colliders = Physics2D.OverlapBoxAll(boxPosition, boxSize, 0f, config.groundLayerMask);
            isGrounded = false;

            foreach (var hitCollider in colliders)
            {
                // 자기 자신의 Collider는 제외
                if (hitCollider != col && hitCollider.gameObject != gameObject)
                {
                    isGrounded = true;
                    break;
                }
            }

            // 바닥 감지 실패 경고 (5초마다 출력)
            if (!isGrounded && !wasGrounded && Time.frameCount % 300 == 0)
            {
                Collider2D[] allColliders = Physics2D.OverlapBoxAll(boxPosition, boxSize, 0f);
                if (allColliders.Length > 0)
                {
                    // 자기 자신을 제외한 충돌체만 필터링
                    var otherColliders = System.Array.FindAll(allColliders, c => c != col && c.gameObject != gameObject);

                    if (otherColliders.Length > 0)
                    {
                        Debug.LogWarning($"[CharacterPhysics] 바닥 감지 실패! GameObject: {gameObject.name}");
                        Debug.LogWarning($"  충돌체는 감지되었으나 Ground Layer가 아닙니다.");
                        Debug.LogWarning($"  감지된 GameObject: {string.Join(", ", System.Array.ConvertAll(otherColliders, c => $"{c.gameObject.name} (Layer: {LayerMask.LayerToName(c.gameObject.layer)})"))}");
                        Debug.LogWarning($"  설정된 Ground LayerMask: {config.groundLayerMask.value}");
                        Debug.LogWarning($"  해결 방법: Ground GameObject의 Layer를 'Ground'로 설정하거나, SkulPhysicsConfig의 groundLayerMask를 수정하세요.");
                    }
                    else if (allColliders.Length > 0 && allColliders[0] == col)
                    {
                        Debug.LogWarning($"[CharacterPhysics] 자기 자신의 Collider만 감지됨! GameObject: {gameObject.name}");
                        Debug.LogWarning($"  Ground 체크 영역에 다른 Collider가 없습니다. 바닥이 없거나 너무 멀리 떨어져 있을 수 있습니다.");
                    }
                }
            }

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

            // 왼쪽 벽 체크 (자기 자신 제외)
            Vector2 leftBoxPosition = boxCenter + Vector2.left * (col.size.x * 0.5f + checkDistance * 0.5f);
            Collider2D[] leftColliders = Physics2D.OverlapBoxAll(leftBoxPosition,
                new Vector2(checkDistance, col.size.y * 0.8f), 0f, config.wallLayerMask);
            isTouchingLeftWall = false;
            foreach (var hitCollider in leftColliders)
            {
                if (hitCollider != col && hitCollider.gameObject != gameObject)
                {
                    isTouchingLeftWall = true;
                    break;
                }
            }

            // 오른쪽 벽 체크 (자기 자신 제외)
            Vector2 rightBoxPosition = boxCenter + Vector2.right * (col.size.x * 0.5f + checkDistance * 0.5f);
            Collider2D[] rightColliders = Physics2D.OverlapBoxAll(rightBoxPosition,
                new Vector2(checkDistance, col.size.y * 0.8f), 0f, config.wallLayerMask);
            isTouchingRightWall = false;
            foreach (var hitCollider in rightColliders)
            {
                if (hitCollider != col && hitCollider.gameObject != gameObject)
                {
                    isTouchingRightWall = true;
                    break;
                }
            }

            // 벽 상태 업데이트
            isTouchingWall = isTouchingLeftWall || isTouchingRightWall;

            if (isTouchingLeftWall && !isTouchingRightWall)
                wallDirection = WallDirection.Left;
            else if (!isTouchingLeftWall && isTouchingRightWall)
                wallDirection = WallDirection.Right;
            else
                wallDirection = WallDirection.None;

            // WallDetectionData 업데이트
            if (isTouchingWall)
            {
                Vector2 normal = wallDirection == WallDirection.Left ? Vector2.right : Vector2.left;
                float distance = checkDistance;
                RaycastHit2D hit = default; // BoxCast를 사용하지 않으므로 기본값
                currentWallData = new WallDetectionData(true, wallDirection, normal, distance, hit);
            }
            else
            {
                currentWallData = new WallDetectionData(false, WallDirection.None, Vector2.zero, 0f, default);
            }

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

                // 벽 슬라이딩 상태 리셋
                if (isWallSliding)
                {
                    StopWallSlide();
                }

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
