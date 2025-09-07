// ================================
// File: Assets/GASTest/Platform2DController.cs
// 2D 플랫포머 플레이어 컨트롤러
// ================================
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TestScene
{
    /// <summary>
    /// 2D 플랫포머 캐릭터 컨트롤러
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Platform2DController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float airControl = 0.5f;
        [SerializeField] private int maxJumps = 2;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0, -1f);

        [Header("Combat Settings")]
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Visual Settings")]
        [SerializeField] private bool flipSpriteOnDirection = true;
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool showGizmos = true;

        // Components
        private Rigidbody2D rb;
        private Collider2D col;
        private SpriteRenderer spriteRenderer;

        // Input
        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction attackAction;

        // State
        private float moveInput;
        private int currentJumpCount;
        private bool isGrounded;
        private bool wasGroundedLastFrame;
        private bool isFacingRight = true;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;

        // Constants
        private const float COYOTE_TIME = 0.15f;
        private const float JUMP_BUFFER_TIME = 0.2f;
        private const float GROUND_CHECK_DELAY = 0.1f;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            InitializeInput();
        }

        private void Start()
        {
            SetupInitialState();
        }

        private void Update()
        {
            UpdateInput();
            UpdateGroundCheck();
            UpdateTimers();
            HandleJump();
            HandleAttack();
            UpdateVisuals();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void OnDestroy()
        {
            CleanupInput();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // Rigidbody2D 설정
            rb.gravityScale = 1f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void InitializeInput()
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = gameObject.AddComponent<PlayerInput>();
            }

            // Input Actions 설정
            var actionMap = playerInput.currentActionMap;
            if (actionMap != null)
            {
                moveAction = actionMap.FindAction("Move");
                jumpAction = actionMap.FindAction("Jump");
                attackAction = actionMap.FindAction("Attack");
            }
        }

        private void SetupInitialState()
        {
            currentJumpCount = 0;
            isGrounded = false;
            isFacingRight = true;
        }

        #endregion

        #region Input Handling

        private void UpdateInput()
        {
            // 이동 입력
            if (moveAction != null)
            {
                moveInput = moveAction.ReadValue<Vector2>().x;
            }
            else
            {
                // Fallback to legacy input
                moveInput = Input.GetAxisRaw("Horizontal");
            }

            // 점프 버퍼링
            if ((jumpAction != null && jumpAction.WasPressedThisFrame()) ||
                Input.GetButtonDown("Jump"))
            {
                jumpBufferCounter = JUMP_BUFFER_TIME;
            }
        }

        #endregion

        #region Movement

        private void HandleMovement()
        {
            float targetVelocity = moveInput * moveSpeed;
            float smoothing = isGrounded ? 0.05f : airControl;

            // 속도 보간
            rb.linearVelocity = new Vector2(
                Mathf.Lerp(rb.linearVelocity.x, targetVelocity, smoothing),
                rb.linearVelocity.y
            );

            // 방향 전환
            if (flipSpriteOnDirection && moveInput != 0)
            {
                bool shouldFaceRight = moveInput > 0;
                if (shouldFaceRight != isFacingRight)
                {
                    Flip();
                }
            }
        }

        private void Flip()
        {
            isFacingRight = !isFacingRight;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !isFacingRight;
            }
            else
            {
                Vector3 scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }
        }

        #endregion

        #region Jump System

        private void HandleJump()
        {
            // 점프 버퍼 체크
            if (jumpBufferCounter > 0)
            {
                // 바닥에 있거나 코요테 타임 중
                if (isGrounded || coyoteTimeCounter > 0)
                {
                    PerformJump();
                    jumpBufferCounter = 0;
                }
                // 공중 점프
                else if (currentJumpCount < maxJumps)
                {
                    PerformJump();
                    jumpBufferCounter = 0;
                }
            }
        }

        private void PerformJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
            currentJumpCount++;
            coyoteTimeCounter = 0;

            if (showDebugInfo)
            {
                Debug.Log($"Jump performed! Count: {currentJumpCount}/{maxJumps}");
            }
        }

        #endregion

        #region Ground Check

        private void UpdateGroundCheck()
        {
            wasGroundedLastFrame = isGrounded;

            Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
            isGrounded = Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer);

            // 착지 감지
            if (isGrounded && !wasGroundedLastFrame)
            {
                OnLanded();
            }
            // 공중으로 전환
            else if (!isGrounded && wasGroundedLastFrame)
            {
                OnBecameAirborne();
            }
        }

        private void OnLanded()
        {
            currentJumpCount = 0;

            if (showDebugInfo)
            {
                Debug.Log("Landed!");
            }
        }

        private void OnBecameAirborne()
        {
            // 점프로 인한 것이 아니면 코요테 타임 시작
            if (currentJumpCount == 0)
            {
                coyoteTimeCounter = COYOTE_TIME;
                currentJumpCount = 1; // 첫 점프는 이미 사용
            }
        }

        #endregion

        #region Combat

        private void HandleAttack()
        {
            if ((attackAction != null && attackAction.WasPressedThisFrame()) ||
                Input.GetButtonDown("Fire1"))
            {
                PerformAttack();
            }
        }

        private async void PerformAttack()
        {
            // 공격 범위 내 적 탐색
            Vector2 attackPosition = transform.position + (isFacingRight ? Vector3.right : Vector3.left) * (attackRange / 2);
            Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPosition, attackRange / 2, enemyLayer);

            if (showDebugInfo)
            {
                Debug.Log($"Attack! Hit {enemies.Length} enemies");
            }

            // 적에게 데미지 처리 (GAS 시스템 연동 시 구현)
            foreach (var enemy in enemies)
            {
                // TODO: Apply damage effect through GAS
            }

            // 공격 애니메이션 대기
            await Task.Delay(300);
        }

        #endregion

        #region Timers

        private void UpdateTimers()
        {
            // 코요테 타임
            if (coyoteTimeCounter > 0)
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            // 점프 버퍼
            if (jumpBufferCounter > 0)
            {
                jumpBufferCounter -= Time.deltaTime;
            }
        }

        #endregion

        #region Visual Updates

        private void UpdateVisuals()
        {
            // 추가 시각적 업데이트 (애니메이션 등)
        }

        #endregion

        #region Cleanup

        private void CleanupInput()
        {
            if (moveAction != null) moveAction.Disable();
            if (jumpAction != null) jumpAction.Disable();
            if (attackAction != null) attackAction.Disable();
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // Ground check
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 groundCheckPos = transform.position + (Vector3)groundCheckOffset;
            Gizmos.DrawWireSphere(groundCheckPos, groundCheckRadius);

            // Attack range
            Gizmos.color = Color.yellow;
            Vector3 attackPos = transform.position + (isFacingRight ? Vector3.right : Vector3.left) * (attackRange / 2);
            Gizmos.DrawWireSphere(attackPos, attackRange / 2);
        }

        #endregion
    }
}