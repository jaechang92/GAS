// ================================
// File: Assets/Scripts/TestScene/Enemy2DController.cs
// 2D 플랫포머 적 AI
// ================================
using UnityEngine;
using GAS.AttributeSystem;
using GAS.TagSystem;
using GAS.EffectSystem;
using static GAS.Core.GASConstants;

namespace TestScene
{
    /// <summary>
    /// 2D 플랫포머 적 AI 컨트롤러
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Enemy2DController : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private AIBehavior behavior = AIBehavior.Patrol;
        [SerializeField] private float detectionRange = 8f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float moveSpeed = 3f;

        [Header("Patrol Settings")]
        [SerializeField] private float patrolDistance = 5f;
        [SerializeField] private float waitTime = 2f;

        [Header("Combat")]
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackCooldown = 1.5f;

        [Header("Edge Detection")]
        [SerializeField] private bool avoidFalling = true;
        [SerializeField] private float edgeCheckDistance = 1f;
        [SerializeField] private LayerMask groundLayer = 1 << 10;

        [Header("Visual")]
        [SerializeField] private Color normalColor = Color.red;
        [SerializeField] private Color aggroColor = Color.magenta;
        [SerializeField] private bool flipSpriteOnDirection = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool showGizmos = true;

        public enum AIBehavior
        {
            Idle,       // 제자리
            Patrol,     // 좌우 순찰
            Guard,      // 특정 위치 방어
            Aggressive  // 적극적 추적
        }

        // Components
        private Rigidbody2D rb2d;
        private Collider2D coll2d;
        private AttributeSetComponent attributes;
        private TagComponent tags;
        private EffectComponent effects;
        private SpriteRenderer spriteRenderer;
        private Renderer meshRenderer;

        // AI State
        private GameObject player;
        private bool hasAggro = false;
        private float lastAttackTime;
        private bool facingRight = true;

        // Patrol
        private Vector3 startPosition;
        private Vector3 leftBound;
        private Vector3 rightBound;
        private bool movingRight = true;
        private float waitTimer = 0f;
        private bool isWaiting = false;

        // Health
        private float maxHealth;
        private bool isDead = false;

        private void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
            coll2d = GetComponent<Collider2D>();
            attributes = GetComponent<AttributeSetComponent>();
            tags = GetComponent<TagComponent>();
            effects = GetComponent<EffectComponent>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            meshRenderer = GetComponent<Renderer>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void Start()
        {
            // 플레이어 찾기
            player = GameObject.FindWithTag("Player");

            // 시작 위치 저장
            startPosition = transform.position;
            leftBound = startPosition - Vector3.right * patrolDistance;
            rightBound = startPosition + Vector3.right * patrolDistance;

            // 초기 체력 저장
            if (attributes != null)
            {
                maxHealth = attributes.GetAttributeMaxValue(AttributeType.Health);
            }

            // Enemy 태그 추가
            if (tags != null)
            {
                // tags.AddTag("Enemy");
            }

            // 시작 방향 랜덤
            movingRight = Random.Range(0, 2) == 0;
            facingRight = movingRight;
        }

        private void Update()
        {
            if (isDead) return;

            CheckHealth();
            DetectPlayer();

            switch (behavior)
            {
                case AIBehavior.Idle:
                    HandleIdle();
                    break;
                case AIBehavior.Patrol:
                    HandlePatrol();
                    break;
                case AIBehavior.Guard:
                    HandleGuard();
                    break;
                case AIBehavior.Aggressive:
                    HandleAggressive();
                    break;
            }

            UpdateVisuals();
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            // 물리 기반 이동은 여기서 처리
        }

        /// <summary>
        /// 체력 확인
        /// </summary>
        private void CheckHealth()
        {
            if (attributes == null) return;

            float currentHealth = attributes.GetAttributeValue(AttributeType.Health);

            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
        }

        /// <summary>
        /// 플레이어 감지
        /// </summary>
        private void DetectPlayer()
        {
            if (player == null)
            {
                player = GameObject.FindWithTag("Player");
                return;
            }

            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance <= detectionRange)
            {
                // 시야 확인 (벽에 막혀있는지)
                Vector2 direction = (player.transform.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, groundLayer);

                if (hit.collider == null)
                {
                    hasAggro = true;
                }
            }
            else
            {
                hasAggro = false;
            }
        }

        /// <summary>
        /// Idle 행동
        /// </summary>
        private void HandleIdle()
        {
            // 제자리에 가만히 있기
            rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocityY);

            // 플레이어 발견 시 추적
            if (hasAggro && player != null)
            {
                FaceTarget(player.transform.position);
                TryAttack();
            }
        }

        /// <summary>
        /// Patrol 행동
        /// </summary>
        private void HandlePatrol()
        {
            if (hasAggro && player != null)
            {
                // 플레이어 추적
                ChasePlayer();
            }
            else
            {
                // 순찰
                PatrolMovement();
            }
        }

        /// <summary>
        /// Guard 행동
        /// </summary>
        private void HandleGuard()
        {
            if (hasAggro && player != null)
            {
                // 플레이어 발견 시 공격만 (추적하지 않음)
                FaceTarget(player.transform.position);
                TryAttack();

                // 원위치로 돌아가기
                if (Vector2.Distance(transform.position, startPosition) > patrolDistance)
                {
                    MoveTowards(startPosition);
                }
            }
            else
            {
                // 원위치로 복귀
                if (Vector2.Distance(transform.position, startPosition) > 0.5f)
                {
                    MoveTowards(startPosition);
                }
                else
                {
                    rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocityY);
                }
            }
        }

        /// <summary>
        /// Aggressive 행동
        /// </summary>
        private void HandleAggressive()
        {
            if (player != null)
            {
                // 항상 플레이어 추적
                ChasePlayer();
            }
        }

        /// <summary>
        /// 순찰 이동
        /// </summary>
        private void PatrolMovement()
        {
            if (isWaiting)
            {
                rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocityY);
                waitTimer -= Time.deltaTime;

                if (waitTimer <= 0)
                {
                    isWaiting = false;
                    movingRight = !movingRight;
                    facingRight = movingRight;
                }
                return;
            }

            // 가장자리 체크
            if (avoidFalling && !CheckGround(movingRight))
            {
                StartWait();
                return;
            }

            // 이동
            float direction = movingRight ? 1 : -1;
            rb2d.linearVelocity = new Vector2(direction * moveSpeed, rb2d.linearVelocityY);

            // 경계 체크
            if (movingRight && transform.position.x >= rightBound.x)
            {
                StartWait();
            }
            else if (!movingRight && transform.position.x <= leftBound.x)
            {
                StartWait();
            }
        }

        /// <summary>
        /// 대기 시작
        /// </summary>
        private void StartWait()
        {
            isWaiting = true;
            waitTimer = waitTime;
        }

        /// <summary>
        /// 플레이어 추적
        /// </summary>
        private void ChasePlayer()
        {
            if (player == null) return;

            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance > attackRange)
            {
                // 가장자리 체크
                bool canMove = true;
                float direction = Mathf.Sign(player.transform.position.x - transform.position.x);

                if (avoidFalling)
                {
                    canMove = CheckGround(direction > 0);
                }

                if (canMove)
                {
                    MoveTowards(player.transform.position);
                }
                else
                {
                    rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocityY);
                }
            }
            else
            {
                // 공격 범위 내
                rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocityY);
                TryAttack();
            }

            FaceTarget(player.transform.position);
        }

        /// <summary>
        /// 특정 위치로 이동
        /// </summary>
        private void MoveTowards(Vector3 target)
        {
            float direction = Mathf.Sign(target.x - transform.position.x);
            rb2d.linearVelocity = new Vector2(direction * moveSpeed, rb2d.linearVelocityY);
            facingRight = direction > 0;
        }

        /// <summary>
        /// 타겟 방향 보기
        /// </summary>
        private void FaceTarget(Vector3 target)
        {
            facingRight = target.x > transform.position.x;
        }

        /// <summary>
        /// 공격 시도
        /// </summary>
        private void TryAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;

            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= attackRange)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }

        /// <summary>
        /// 공격 실행
        /// </summary>
        private void Attack()
        {
            if (player == null) return;

            var playerAttributes = player.GetComponent<AttributeSetComponent>();
            if (playerAttributes != null)
            {
                playerAttributes.ModifyAttribute(AttributeType.Health, -attackDamage, ModifierOperation.Add);

                if (showDebugInfo)
                {
                    Debug.Log($"[Enemy2D] Attacked player for {attackDamage} damage");
                }
            }
        }

        /// <summary>
        /// 지면 체크 (낙하 방지)
        /// </summary>
        private bool CheckGround(bool checkRight)
        {
            float direction = checkRight ? 1 : -1;
            Vector2 checkPos = (Vector2)transform.position + new Vector2(direction * edgeCheckDistance, -0.5f);

            return Physics2D.Raycast(checkPos, Vector2.down, 1f, groundLayer);
        }

        /// <summary>
        /// 시각 효과 업데이트
        /// </summary>
        private void UpdateVisuals()
        {
            // 방향 전환
            if (flipSpriteOnDirection)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = !facingRight;
                }
            }

            // 색상 변경
            Color targetColor = hasAggro ? aggroColor : normalColor;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = targetColor;
            }
            else if (meshRenderer != null)
            {
                meshRenderer.material.color = targetColor;
            }
        }

        /// <summary>
        /// 사망 처리
        /// </summary>
        private void Die()
        {
            isDead = true;
            rb2d.linearVelocity = Vector2.zero;

            if (showDebugInfo)
            {
                Debug.Log($"[Enemy2D] {gameObject.name} died");
            }

            // 2초 후 제거
            Destroy(gameObject, 2f);

            // 콜라이더 비활성화
            if (coll2d != null)
            {
                coll2d.enabled = false;
            }

            // 시각적 표시
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.gray;
            }
            else if (meshRenderer != null)
            {
                meshRenderer.material.color = Color.gray;
            }
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // Detection range
            Gizmos.color = hasAggro ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Patrol bounds
            if (behavior == AIBehavior.Patrol && Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(leftBound, rightBound);
                Gizmos.DrawWireSphere(leftBound, 0.2f);
                Gizmos.DrawWireSphere(rightBound, 0.2f);
            }

            // Edge check
            if (avoidFalling)
            {
                Gizmos.color = Color.green;
                Vector3 checkPosRight = transform.position + Vector3.right * edgeCheckDistance + Vector3.down * 0.5f;
                Vector3 checkPosLeft = transform.position + Vector3.left * edgeCheckDistance + Vector3.down * 0.5f;
                Gizmos.DrawRay(checkPosRight, Vector3.down);
                Gizmos.DrawRay(checkPosLeft, Vector3.down);
            }
        }
    }
}