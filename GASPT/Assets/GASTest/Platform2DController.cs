// ================================
// File: Assets/GASTest/Platform2DController.cs
// 2D �÷����� �÷��̾� ��Ʈ�ѷ�
// ================================
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TestScene
{
    /// <summary>
    /// 2D �÷����� ĳ���� ��Ʈ�ѷ�
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

            // Rigidbody2D ����
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

            // Input Actions ����
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
            // �̵� �Է�
            if (moveAction != null)
            {
                moveInput = moveAction.ReadValue<Vector2>().x;
            }
            else
            {
                // Fallback to legacy input
                moveInput = Input.GetAxisRaw("Horizontal");
            }

            // ���� ���۸�
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

            // �ӵ� ����
            rb.linearVelocity = new Vector2(
                Mathf.Lerp(rb.linearVelocity.x, targetVelocity, smoothing),
                rb.linearVelocity.y
            );

            // ���� ��ȯ
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
            // ���� ���� üũ
            if (jumpBufferCounter > 0)
            {
                // �ٴڿ� �ְų� �ڿ��� Ÿ�� ��
                if (isGrounded || coyoteTimeCounter > 0)
                {
                    PerformJump();
                    jumpBufferCounter = 0;
                }
                // ���� ����
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

            // ���� ����
            if (isGrounded && !wasGroundedLastFrame)
            {
                OnLanded();
            }
            // �������� ��ȯ
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
            // ������ ���� ���� �ƴϸ� �ڿ��� Ÿ�� ����
            if (currentJumpCount == 0)
            {
                coyoteTimeCounter = COYOTE_TIME;
                currentJumpCount = 1; // ù ������ �̹� ���
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
            // ���� ���� �� �� Ž��
            Vector2 attackPosition = transform.position + (isFacingRight ? Vector3.right : Vector3.left) * (attackRange / 2);
            Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPosition, attackRange / 2, enemyLayer);

            if (showDebugInfo)
            {
                Debug.Log($"Attack! Hit {enemies.Length} enemies");
            }

            // ������ ������ ó�� (GAS �ý��� ���� �� ����)
            foreach (var enemy in enemies)
            {
                // TODO: Apply damage effect through GAS
            }

            // ���� �ִϸ��̼� ���
            await Task.Delay(300);
        }

        #endregion

        #region Timers

        private void UpdateTimers()
        {
            // �ڿ��� Ÿ��
            if (coyoteTimeCounter > 0)
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            // ���� ����
            if (jumpBufferCounter > 0)
            {
                jumpBufferCounter -= Time.deltaTime;
            }
        }

        #endregion

        #region Visual Updates

        private void UpdateVisuals()
        {
            // �߰� �ð��� ������Ʈ (�ִϸ��̼� ��)
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