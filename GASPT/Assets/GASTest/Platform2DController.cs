// ================================
// File: Assets/Scripts/TestScene/Platform2DController.cs
// 2D 플랫포머 플레이어 컨트롤러
// ================================
using UnityEngine;
using GAS.AbilitySystem;
using GAS.AttributeSystem;
using GAS.TagSystem;
using static GAS.Core.GASConstants;

namespace TestScene
{
    /// <summary>
    /// 2D 플랫포머 플레이어 이동 및 입력 처리
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Platform2DController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float airControl = 0.5f;
        [SerializeField] private int maxJumps = 2; // 더블 점프

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

            // 스프라이트 렌더러가 없으면 자식에서 찾기
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void Start()
        {
            // 플레이어 태그 추가
            if (tags != null)
            {
                // tags.AddTag("Player"); // GameplayTag가 필요
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
        /// 입력 처리
        /// </summary>
        private void HandleInput()
        {
            // 이동 입력
            horizontalInput = Input.GetAxisRaw("Horizontal");

            // 점프 입력
            if (Input.GetKeyDown(KeyCode.Space))
            {
                jumpInput = true;
            }

            // 능력 입력 (숫자키 또는 Q,W,E)
            HandleAbilityInput();

            // 마우스 클릭으로 타겟 지정
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseTargeting();
            }
        }

        /// <summary>
        /// 능력 입력 처리
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
        /// 능력 활성화 시도
        /// </summary>
        private async Awaitable TryActivateAbility(int slotId)
        {
            // 타겟이 있으면 타겟 능력 사용
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
                // 타겟이 없으면 자동 타겟 찾기
                FindNearestTarget();
                if (currentTarget != null)
                {
                    await abilitySystem.TryActivateAbilityByInput(slotId);
                }
                else
                {
                    // 논타겟 능력 실행
                    await abilitySystem.TryActivateAbilityByInput(slotId);
                }
            }
        }

        /// <summary>
        /// 마우스 클릭 타겟팅
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
                // 클릭 위치로 능력 사용 (Point 타입)
                Vector3 worldPos = new Vector3(mousePos.x, mousePos.y, 0);
                // abilitySystem.TryActivateAbilityAtPosition(ability, worldPos);
            }
        }

        /// <summary>
        /// 이동 처리
        /// </summary>
        private void HandleMovement()
        {
            // 수평 이동
            float targetVelocityX = horizontalInput * moveSpeed;

            if (isGrounded)
            {
                // 지상에서 정확한 제어
                rb2d.velocity = new Vector2(targetVelocityX, rb2d.linearVelocityY);
            }
            else
            {
                // 공중에서 부분적인 제어
                float currentVelocityX = rb2d.linearVelocityX;
                float newVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, airControl * Time.fixedDeltaTime);
                rb2d.linearVelocity = new Vector2(newVelocityX, rb2d.linearVelocityY);
            }

            // 점프 처리
            if (jumpInput && CanJump())
            {
                Jump();
            }
            jumpInput = false;
        }

        /// <summary>
        /// 점프 가능 여부 확인
        /// </summary>
        private bool CanJump()
        {
            return currentJumps < maxJumps;
        }

        /// <summary>
        /// 점프 실행
        /// </summary>
        private void Jump()
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocityX, 0); // Y 속도 리셋
            rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            currentJumps++;

            if (showDebugInfo)
            {
                Debug.Log($"[Platform2DController] Jump {currentJumps}/{maxJumps}");
            }
        }

        /// <summary>
        /// 지면 체크
        /// </summary>
        private void CheckGround()
        {
            Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer);

            // 착지 시
            if (!wasGrounded && isGrounded)
            {
                currentJumps = 0;
                OnLanded();
            }
            // 떨어질 때
            else if (wasGrounded && !isGrounded)
            {
                // 점프 없이 떨어진 경우 점프 카운트 1 사용
                if (currentJumps == 0)
                {
                    currentJumps = 1;
                }
            }
        }

        /// <summary>
        /// 착지 이벤트
        /// </summary>
        private void OnLanded()
        {
            if (showDebugInfo)
            {
                Debug.Log("[Platform2DController] Landed");
            }
        }

        /// <summary>
        /// 방향 업데이트
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
        /// 타겟 업데이트
        /// </summary>
        private void UpdateTarget()
        {
            if (currentTarget != null)
            {
                // 타겟이 죽었거나 범위를 벗어났는지 확인
                float distance = Vector2.Distance(transform.position, currentTarget.transform.position);
                if (distance > attackRange * 2)
                {
                    ClearTarget();
                }

                // 타겟의 체력 확인
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
        /// 가장 가까운 타겟 찾기
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
        /// 타겟 설정
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
        /// 타겟 해제
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
        /// 디버그 정보 표시
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

            // Ground check 표시
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckRadius);

            // Attack range 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Target 표시
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
                Gizmos.DrawWireSphere(currentTarget.transform.position, 0.5f);
            }
        }
    }
}