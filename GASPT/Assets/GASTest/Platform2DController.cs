// ================================
// File: Assets/Scripts/TestScene/Platform2DController.cs
// 2D �÷����� �÷��̾� ��Ʈ�ѷ�
// ================================
using UnityEngine;
using GAS.AbilitySystem;
using GAS.AttributeSystem;
using GAS.TagSystem;
using static GAS.Core.GASConstants;

namespace TestScene
{
    /// <summary>
    /// 2D �÷����� �÷��̾� �̵� �� �Է� ó��
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Platform2DController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float airControl = 0.5f;
        [SerializeField] private int maxJumps = 2; // ���� ����

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayer = 1 << 10; // Platform layer
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0, -1f);

        [Header("Combat")]
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private LayerMask enemyLayer = 1 << 9; // Enemy layer

        [Header("Animation")]
        [SerializeField] private bool flipSpriteOnDirection = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool showGizmos = true;

        // Components
        private Rigidbody2D rb2d;
        private Collider2D coll2d;
        private AbilitySystemComponent abilitySystem;
        private AttributeSetComponent attributes;
        private TagComponent tags;
        private SpriteRenderer spriteRenderer;

        // State
        private bool isGrounded;
        private int currentJumps;
        private float horizontalInput;
        private bool jumpInput;
        private bool facingRight = true;
        private GameObject currentTarget;

        // Ability input mapping
        private const int ABILITY_SLOT_1 = 1; // Q or 1
        private const int ABILITY_SLOT_2 = 2; // W or 2
        private const int ABILITY_SLOT_3 = 3; // E or 3

        private void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
            coll2d = GetComponent<Collider2D>();
            abilitySystem = GetComponent<AbilitySystemComponent>();
            attributes = GetComponent<AttributeSetComponent>();
            tags = GetComponent<TagComponent>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // ��������Ʈ �������� ������ �ڽĿ��� ã��
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void Start()
        {
            // �÷��̾� �±� �߰�
            if (tags != null)
            {
                // tags.AddTag("Player"); // GameplayTag�� �ʿ�
            }
        }

        private void Update()
        {
            HandleInput();
            CheckGround();
            UpdateFacing();
            UpdateTarget();

            if (showDebugInfo)
            {
                ShowDebugInfo();
            }
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        /// <summary>
        /// �Է� ó��
        /// </summary>
        private void HandleInput()
        {
            // �̵� �Է�
            horizontalInput = Input.GetAxisRaw("Horizontal");

            // ���� �Է�
            if (Input.GetKeyDown(KeyCode.Space))
            {
                jumpInput = true;
            }

            // �ɷ� �Է� (����Ű �Ǵ� Q,W,E)
            HandleAbilityInput();

            // ���콺 Ŭ������ Ÿ�� ����
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseTargeting();
            }
        }

        /// <summary>
        /// �ɷ� �Է� ó��
        /// </summary>
        private async void HandleAbilityInput()
        {
            if (abilitySystem == null) return;

            // Slot 1: Q or 1
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Alpha1))
            {
                await TryActivateAbility(ABILITY_SLOT_1);
            }

            // Slot 2: W or 2
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Alpha2))
            {
                await TryActivateAbility(ABILITY_SLOT_2);
            }

            // Slot 3: E or 3
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Alpha3))
            {
                await TryActivateAbility(ABILITY_SLOT_3);
            }

            // Ultimate: R or 4
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Alpha4))
            {
                await TryActivateAbility(4);
            }
        }

        /// <summary>
        /// �ɷ� Ȱ��ȭ �õ�
        /// </summary>
        private async Awaitable TryActivateAbility(int slotId)
        {
            // Ÿ���� ������ Ÿ�� �ɷ� ���
            if (currentTarget != null)
            {
                bool success = await abilitySystem.TryActivateAbilityByInput(slotId);
                if (!success && showDebugInfo)
                {
                    Debug.Log($"[Platform2DController] Failed to activate ability in slot {slotId}");
                }
            }
            else
            {
                // Ÿ���� ������ �ڵ� Ÿ�� ã��
                FindNearestTarget();
                if (currentTarget != null)
                {
                    await abilitySystem.TryActivateAbilityByInput(slotId);
                }
                else
                {
                    // ��Ÿ�� �ɷ� ����
                    await abilitySystem.TryActivateAbilityByInput(slotId);
                }
            }
        }

        /// <summary>
        /// ���콺 Ŭ�� Ÿ����
        /// </summary>
        private void HandleMouseTargeting()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, enemyLayer);

            if (hit.collider != null)
            {
                SetTarget(hit.collider.gameObject);
            }
            else
            {
                // Ŭ�� ��ġ�� �ɷ� ��� (Point Ÿ��)
                Vector3 worldPos = new Vector3(mousePos.x, mousePos.y, 0);
                // abilitySystem.TryActivateAbilityAtPosition(ability, worldPos);
            }
        }

        /// <summary>
        /// �̵� ó��
        /// </summary>
        private void HandleMovement()
        {
            // ���� �̵�
            float targetVelocityX = horizontalInput * moveSpeed;

            if (isGrounded)
            {
                // ���󿡼� ��Ȯ�� ����
                rb2d.velocity = new Vector2(targetVelocityX, rb2d.linearVelocityY);
            }
            else
            {
                // ���߿��� �κ����� ����
                float currentVelocityX = rb2d.linearVelocityX;
                float newVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, airControl * Time.fixedDeltaTime);
                rb2d.linearVelocity = new Vector2(newVelocityX, rb2d.linearVelocityY);
            }

            // ���� ó��
            if (jumpInput && CanJump())
            {
                Jump();
            }
            jumpInput = false;
        }

        /// <summary>
        /// ���� ���� ���� Ȯ��
        /// </summary>
        private bool CanJump()
        {
            return currentJumps < maxJumps;
        }

        /// <summary>
        /// ���� ����
        /// </summary>
        private void Jump()
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocityX, 0); // Y �ӵ� ����
            rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            currentJumps++;

            if (showDebugInfo)
            {
                Debug.Log($"[Platform2DController] Jump {currentJumps}/{maxJumps}");
            }
        }

        /// <summary>
        /// ���� üũ
        /// </summary>
        private void CheckGround()
        {
            Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer);

            // ���� ��
            if (!wasGrounded && isGrounded)
            {
                currentJumps = 0;
                OnLanded();
            }
            // ������ ��
            else if (wasGrounded && !isGrounded)
            {
                // ���� ���� ������ ��� ���� ī��Ʈ 1 ���
                if (currentJumps == 0)
                {
                    currentJumps = 1;
                }
            }
        }

        /// <summary>
        /// ���� �̺�Ʈ
        /// </summary>
        private void OnLanded()
        {
            if (showDebugInfo)
            {
                Debug.Log("[Platform2DController] Landed");
            }
        }

        /// <summary>
        /// ���� ������Ʈ
        /// </summary>
        private void UpdateFacing()
        {
            if (horizontalInput != 0)
            {
                facingRight = horizontalInput > 0;

                if (flipSpriteOnDirection && spriteRenderer != null)
                {
                    spriteRenderer.flipX = !facingRight;
                }
            }
        }

        /// <summary>
        /// Ÿ�� ������Ʈ
        /// </summary>
        private void UpdateTarget()
        {
            if (currentTarget != null)
            {
                // Ÿ���� �׾��ų� ������ ������� Ȯ��
                float distance = Vector2.Distance(transform.position, currentTarget.transform.position);
                if (distance > attackRange * 2)
                {
                    ClearTarget();
                }

                // Ÿ���� ü�� Ȯ��
                var targetAttributes = currentTarget.GetComponent<AttributeSetComponent>();
                if (targetAttributes != null)
                {
                    float health = targetAttributes.GetAttributeValue(AttributeType.Health);
                    if (health <= 0)
                    {
                        ClearTarget();
                    }
                }
            }
        }

        /// <summary>
        /// ���� ����� Ÿ�� ã��
        /// </summary>
        private void FindNearestTarget()
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

            GameObject nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy.gameObject;
                }
            }

            if (nearestEnemy != null)
            {
                SetTarget(nearestEnemy);
            }
        }

        /// <summary>
        /// Ÿ�� ����
        /// </summary>
        private void SetTarget(GameObject target)
        {
            currentTarget = target;

            if (showDebugInfo)
            {
                Debug.Log($"[Platform2DController] Target set: {target.name}");
            }
        }

        /// <summary>
        /// Ÿ�� ����
        /// </summary>
        private void ClearTarget()
        {
            if (currentTarget != null && showDebugInfo)
            {
                Debug.Log($"[Platform2DController] Target cleared: {currentTarget.name}");
            }
            currentTarget = null;
        }

        /// <summary>
        /// ����� ���� ǥ��
        /// </summary>
        private void ShowDebugInfo()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log($"=== Platform2D Controller Debug ===");
                Debug.Log($"Position: {transform.position}");
                Debug.Log($"Velocity: {rb2d.linearVelocity}");
                Debug.Log($"Grounded: {isGrounded}");
                Debug.Log($"Jumps: {currentJumps}/{maxJumps}");
                Debug.Log($"Facing: {(facingRight ? "Right" : "Left")}");
                Debug.Log($"Target: {(currentTarget != null ? currentTarget.name : "None")}");

                if (attributes != null)
                {
                    Debug.Log($"Health: {attributes.GetAttributeValue(AttributeType.Health)}/{attributes.GetAttributeMaxValue(AttributeType.Health)}");
                    Debug.Log($"Mana: {attributes.GetAttributeValue(AttributeType.Mana)}/{attributes.GetAttributeMaxValue(AttributeType.Mana)}");
                }
                Debug.Log("===================================");
            }
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // Ground check ǥ��
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckRadius);

            // Attack range ǥ��
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Target ǥ��
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
                Gizmos.DrawWireSphere(currentTarget.transform.position, 0.5f);
            }
        }
    }
}