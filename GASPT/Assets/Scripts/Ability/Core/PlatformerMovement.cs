// ���� ��ġ: Assets/Scripts/Ability/Core/PlatformerMovement.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Threading;

namespace AbilitySystem.Core
{
    /// <summary>
    /// ������ �÷����� �̵� ��Ʈ�ѷ�
    /// �� üũ, ������ ����, �������� �̵� ����
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(GroundChecker))]
    public class PlatformerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float airMoveSpeed = 4f;
        [SerializeField] private bool useAcceleration = false; // ���� ��� ����
        [SerializeField] private float acceleration = 10f; // �ɼ����� ����
        [SerializeField] private float deceleration = 10f;
        [SerializeField] private float airAcceleration = 8f;
        [SerializeField] private float airDeceleration = 6f;

        [Header("Instant Movement Settings")]
        [SerializeField] private bool instantStopOnGround = true; // ���󿡼� ��� ����
        [SerializeField] private bool instantDirectionChange = true; // ��� ���� ��ȯ
        [SerializeField] private float airControl = 1f; // ���� ����� (0~1, 1 = ���� ����)

        [Header("Jump Settings")]
        [SerializeField] private float jumpPower = 10f;
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;
        [SerializeField] private float maxFallSpeed = -20f;
        [SerializeField] private int maxJumpCount = 2; // ���� ����

        // ���� ������Ʈ �޼��� �߰�
        public void UpdateMovementStats(float newMoveSpeed, float newJumpPower)
        {
            moveSpeed = newMoveSpeed;
            airMoveSpeed = newMoveSpeed * 0.8f; // ���� �ӵ��� 80%
            jumpPower = newJumpPower;

            Debug.Log($"Movement stats updated - Speed: {moveSpeed}, Jump: {jumpPower}");
        }

        // ������ Ÿ�� ���� �޼���
        public void SetMovementType(bool smooth)
        {
            useAcceleration = smooth;
            Debug.Log($"Movement type: {(smooth ? "Smooth" : "Instant")}");
        }

        [Header("Wall Check Settings")]
        [SerializeField] private bool enableWallCheck = true;
        [SerializeField] private LayerMask wallLayer = 1 << 3;
        [SerializeField] private float wallCheckDistance = 0.1f;
        [SerializeField] private Vector2 wallCheckSize = new Vector2(0.05f, 0.8f);
        [SerializeField] private Vector2 wallCheckOffset = Vector2.zero;

        [Header("Platform Drop")]
        [SerializeField] private bool enablePlatformDrop = true;
        [SerializeField] private LayerMask oneWayPlatformLayer;
        [SerializeField] private float platformDropTime = 0.3f;

        [Header("Edge Correction")]
        [SerializeField] private bool enableEdgeCorrection = true;
        [SerializeField] private float edgeCorrectionRange = 0.3f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        // Components
        private Rigidbody2D rb;
        private BoxCollider2D boxCollider;
        private GroundChecker groundChecker;
        private Collider2D[] platformColliders;

        // Movement State
        public Vector2 MoveInput { get; private set; }
        public bool IsMoving => Mathf.Abs(MoveInput.x) > 0.01f;
        public bool IsFacingRight { get; private set; } = true;
        public bool IsJumping { get; private set; }
        public bool IsFalling => rb.linearVelocity.y < -0.01f && !groundChecker.IsGrounded;
        public bool IsTouchingWallRight { get; private set; }
        public bool IsTouchingWallLeft { get; private set; }
        public bool IsTouchingWall => IsTouchingWallLeft || IsTouchingWallRight;

        // Jump State
        private int currentJumpCount;
        private bool jumpInputReleased = true;
        private bool isJumpCut;

        // Platform Drop
        private bool isPlatformDropping;
        private float platformDropTimer;

        // Events
        public event Action<bool> OnFacingDirectionChanged;
        public event Action OnJumpPerformed;
        public event Action OnDoubleJumpPerformed;
        public event Action OnWallHit;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            groundChecker = GetComponent<GroundChecker>();

            SetupRigidbody();
            SetupGroundChecker();
        }

        private void SetupRigidbody()
        {
            rb.gravityScale = 1f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void SetupGroundChecker()
        {
            groundChecker.OnLanded += HandleLanded;
            groundChecker.OnLeftGround += HandleLeftGround;
        }

        private void Update()
        {
            UpdatePlatformDrop();
        }

        private void FixedUpdate()
        {
            PerformWallChecks();
            ApplyMovement();
            ApplyGravityModifiers();
            ClampFallSpeed();
            UpdateJumpState();
        }

        private void PerformWallChecks()
        {
            if (!enableWallCheck) return;

            Vector2 boxCenter = (Vector2)transform.position + wallCheckOffset;

            // Right wall check
            Vector2 rightCheckPos = boxCenter + Vector2.right * (boxCollider.size.x * 0.5f);
            IsTouchingWallRight = Physics2D.OverlapBox(
                rightCheckPos + Vector2.right * wallCheckDistance * 0.5f,
                wallCheckSize,
                0f,
                wallLayer
            );

            // Left wall check
            Vector2 leftCheckPos = boxCenter + Vector2.left * (boxCollider.size.x * 0.5f);
            IsTouchingWallLeft = Physics2D.OverlapBox(
                leftCheckPos + Vector2.left * wallCheckDistance * 0.5f,
                wallCheckSize,
                0f,
                wallLayer
            );

            // ���� ����� �� �̺�Ʈ
            if ((IsTouchingWallRight && MoveInput.x > 0) || (IsTouchingWallLeft && MoveInput.x < 0))
            {
                OnWallHit?.Invoke();
            }
        }

        private void ApplyMovement()
        {
            float targetSpeed = MoveInput.x * (groundChecker.IsGrounded ? moveSpeed : airMoveSpeed);

            // ���� ���������� �̵� ����
            if ((IsTouchingWallRight && MoveInput.x > 0) || (IsTouchingWallLeft && MoveInput.x < 0))
            {
                targetSpeed = 0f;
            }

            // ���鿡���� �̵� ���� ����
            if (groundChecker.IsGrounded && groundChecker.CurrentSlopeAngle > 0)
            {
                Vector2 slopeMovement = groundChecker.GetMovementDirectionOnSlope(new Vector2(targetSpeed, 0));
                rb.linearVelocity = new Vector2(slopeMovement.x, rb.linearVelocity.y + slopeMovement.y);
            }
            else
            {
                if (useAcceleration)
                {
                    // �ε巯�� ������ (�ɼ�)
                    float accel = groundChecker.IsGrounded ?
                        (IsMoving ? acceleration : deceleration) :
                        (IsMoving ? airAcceleration : airDeceleration);

                    float newVelocityX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accel * Time.fixedDeltaTime);
                    rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
                }
                else
                {
                    // �ﰢ���� ������ ���
                    if (groundChecker.IsGrounded)
                    {
                        // ���󿡼��� ������
                        if (instantStopOnGround && Mathf.Abs(MoveInput.x) < 0.01f)
                        {
                            // �Է��� ������ ��� ����
                            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                        }
                        else if (instantDirectionChange)
                        {
                            // ��� ���� ��ȯ �� �ӵ� ����
                            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
                        }
                        else
                        {
                            // �ణ�� ���� ���� (������)
                            float smoothing = 0.1f;
                            float newVelocityX = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, smoothing);
                            rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
                        }
                    }
                    else
                    {
                        // ���߿����� ������
                        if (Mathf.Abs(MoveInput.x) < 0.01f)
                        {
                            // ���߿��� �Է��� ���� �� - ���� ����
                            // X �ӵ��� �״�� �����ϰų� �ణ�� ���� ���� ����
                            if (airControl < 1f)
                            {
                                // ���� �������� õõ�� ����
                                float airDrag = 1f - (1f - airControl) * 0.05f;
                                rb.linearVelocity = new Vector2(rb.linearVelocity.x * airDrag, rb.linearVelocity.y);
                            }
                        }
                        else
                        {
                            // ���߿��� �Է��� ���� ��
                            if (airControl >= 1f)
                            {
                                // ������ ���� ����
                                rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
                            }
                            else
                            {
                                // �κ����� ���� ����
                                float currentX = rb.linearVelocity.x;
                                float newVelocityX = Mathf.Lerp(currentX, targetSpeed, airControl * Time.fixedDeltaTime * 10f);
                                rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
                            }
                        }
                    }
                }
            }

            // ���� ������Ʈ
            UpdateFacingDirection();
        }

        private void ApplyGravityModifiers()
        {
            if (rb.linearVelocity.y < 0)
            {
                // ������ �� �߷� ����
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.linearVelocity.y > 0 && !IsJumping)
            {
                // ���� ��ư�� ���� �� �߷� ����
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        private void ClampFallSpeed()
        {
            if (rb.linearVelocity.y < maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
            }
        }

        private void UpdateJumpState()
        {
            if (groundChecker.IsGrounded)
            {
                isJumpCut = false;
            }
        }

        private void UpdateFacingDirection()
        {
            if (MoveInput.x > 0.01f && !IsFacingRight)
            {
                IsFacingRight = true;
                OnFacingDirectionChanged?.Invoke(IsFacingRight);
            }
            else if (MoveInput.x < -0.01f && IsFacingRight)
            {
                IsFacingRight = false;
                OnFacingDirectionChanged?.Invoke(IsFacingRight);
            }
        }

        public void SetMoveInput(Vector2 input)
        {
            MoveInput = input;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                groundChecker.BufferJumpInput();
                TryJump();
            }
            //else if (context.canceled)
            //{
            //    OnJumpReleased();
            //}
        }

        public bool TryJump()
        {
            // Jump Buffer ó��
            if (!groundChecker.CanJump() && currentJumpCount >= maxJumpCount)
            {
                return false;
            }

            // �÷��� ��� ���̸� ���� �Ұ�
            if (isPlatformDropping)
            {
                return false;
            }

            // Coyote Time �Ǵ� �Ϲ� ����
            if (groundChecker.CanJump())
            {
                PerformJump();
                groundChecker.ConsumeCoyoteTime();
                groundChecker.ConsumeJumpBuffer();
                return true;
            }
            // ���� ����
            else if (currentJumpCount < maxJumpCount)
            {
                PerformDoubleJump();
                groundChecker.ConsumeJumpBuffer();
                return true;
            }

            return false;
        }

        private void PerformJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            IsJumping = true;
            currentJumpCount = 1;
            OnJumpPerformed?.Invoke();

            // Edge Correction - �𼭸����� ������ �ణ�� ���� ����
            if (enableEdgeCorrection && IsMoving)
            {
                ApplyEdgeCorrection();
            }
        }

        private void PerformDoubleJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            IsJumping = true;
            currentJumpCount++;
            OnDoubleJumpPerformed?.Invoke();
        }

        private void OnJumpReleased()
        {
            IsJumping = false;

            // Jump Cut - ���� ��ư�� ���� ���� �� ���� ���� ����
            if (rb.linearVelocity.y > 0 && !isJumpCut)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
                isJumpCut = true;
            }
        }

        private void ApplyEdgeCorrection()
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.down,
                boxCollider.size.y * 0.5f + 0.1f,
                groundChecker.GroundLayerMask
            );

            if (hit.collider != null)
            {
                float distanceFromEdge = Mathf.Abs(hit.point.x - transform.position.x);
                if (distanceFromEdge < edgeCorrectionRange)
                {
                    float correction = MoveInput.x * edgeCorrectionRange;
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x + correction, rb.linearVelocity.y);
                }
            }
        }

        public void TryPlatformDrop()
        {
            if (!enablePlatformDrop || !groundChecker.IsGrounded) return;

            // One-way platform üũ
            if (groundChecker.CurrentPlatform != null &&
                ((1 << groundChecker.CurrentPlatform.layer) & oneWayPlatformLayer) != 0)
            {
                StartPlatformDrop();
            }
        }

        private void StartPlatformDrop()
        {
            isPlatformDropping = true;
            platformDropTimer = platformDropTime;

            // ���� �÷������� �浹 �ӽ� ��Ȱ��ȭ
            platformColliders = groundChecker.CurrentPlatform.GetComponents<Collider2D>();
            foreach (var collider in platformColliders)
            {
                Physics2D.IgnoreCollision(boxCollider, collider, true);
            }
        }

        private void UpdatePlatformDrop()
        {
            if (!isPlatformDropping) return;

            platformDropTimer -= Time.deltaTime;

            if (platformDropTimer <= 0)
            {
                EndPlatformDrop();
            }
        }

        private void EndPlatformDrop()
        {
            isPlatformDropping = false;

            // �浹 ��Ȱ��ȭ
            if (platformColliders != null)
            {
                foreach (var collider in platformColliders)
                {
                    if (collider != null)
                    {
                        Physics2D.IgnoreCollision(boxCollider, collider, false);
                    }
                }
            }

            platformColliders = null;
        }

        private void HandleLanded()
        {
            currentJumpCount = 0;
            IsJumping = false;
            isJumpCut = false;

            // ������ �ﰢ���� ���� ���� (�ɼ�)
            if (!useAcceleration && instantStopOnGround && Mathf.Abs(MoveInput.x) < 0.01f)
            {
                // ���� ��� X �ӵ� 0���� (�̲����� ����)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            // Jump Buffer üũ
            if (groundChecker.HasJumpBuffered)
            {
                TryJump();
            }
        }

        private void HandleLeftGround()
        {
            // ������ �������µ� �������� �ʾҴٸ� (����)
            if (currentJumpCount == 0)
            {
                currentJumpCount = 1; // ���߿��� �� �� ���� ����
            }
        }

        private void OnDrawGizmos()
        {
            if (!showDebugInfo || !enableWallCheck) return;

            Vector2 boxCenter = (Vector2)transform.position + wallCheckOffset;

            // Right wall check
            Gizmos.color = IsTouchingWallRight ? Color.red : Color.yellow;
            Vector2 rightCheckPos = boxCenter + Vector2.right * (boxCollider != null ? boxCollider.size.x * 0.5f : 0.5f);
            Gizmos.DrawWireCube(rightCheckPos + Vector2.right * wallCheckDistance * 0.5f, wallCheckSize);

            // Left wall check
            Gizmos.color = IsTouchingWallLeft ? Color.red : Color.yellow;
            Vector2 leftCheckPos = boxCenter + Vector2.left * (boxCollider != null ? boxCollider.size.x * 0.5f : 0.5f);
            Gizmos.DrawWireCube(leftCheckPos + Vector2.left * wallCheckDistance * 0.5f, wallCheckSize);
        }

        private void OnDestroy()
        {
            if (groundChecker != null)
            {
                groundChecker.OnLanded -= HandleLanded;
                groundChecker.OnLeftGround -= HandleLeftGround;
            }
        }
    }
}