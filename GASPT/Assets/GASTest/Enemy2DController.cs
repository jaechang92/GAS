// ================================
// File: Assets/GASTest/Enemy2DController.cs
// 2D �÷����� �� AI ��Ʈ�ѷ�
// ================================
using System.Threading.Tasks;
using UnityEngine;

namespace TestScene
{
    /// <summary>
    /// �� AI ��Ʈ�ѷ� - ��Ʈ�Ѱ� ���� ���
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Enemy2DController : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private AIState currentState = AIState.Patrol;
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float loseTargetRange = 8f;

        [Header("Movement")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float patrolDistance = 3f;
        [SerializeField] private bool flipSpriteOnDirection = true;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayer = 1 << 10;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0, -0.5f);

        [Header("Edge Detection")]
        [SerializeField] private float edgeCheckDistance = 1f;
        [SerializeField] private Vector2 edgeCheckOffset = new Vector2(0.5f, -0.5f);

        [Header("Combat")]
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float attackDamage = 10f;

        [Header("Target")]
        [SerializeField] private LayerMask targetLayer = 1 << 8; // Player layer
        [SerializeField] private string targetTag = "Player";

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private bool showDebugLogs = false;

        // Components
        private Rigidbody2D rb;
        private Collider2D col;
        private SpriteRenderer spriteRenderer;

        // State
        private Transform target;
        private Vector3 patrolStartPosition;
        private Vector3 patrolEndPosition;
        private bool movingRight = true;
        private bool isGrounded;
        private float lastAttackTime;
        private bool isAtEdge;
        private bool isBlocked;

        // AI States
        public enum AIState
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            ReturnToPatrol
        }

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializePatrolPoints();
        }

        private void Update()
        {
            UpdateGroundCheck();
            UpdateEdgeDetection();
            UpdateAI();
        }

        private void FixedUpdate()
        {
            ExecuteMovement();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // Rigidbody ����
            rb.gravityScale = 1f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void InitializePatrolPoints()
        {
            patrolStartPosition = transform.position - Vector3.right * (patrolDistance / 2);
            patrolEndPosition = transform.position + Vector3.right * (patrolDistance / 2);
        }

        #endregion

        #region AI System

        private void UpdateAI()
        {
            switch (currentState)
            {
                case AIState.Idle:
                    HandleIdleState();
                    break;

                case AIState.Patrol:
                    HandlePatrolState();
                    break;

                case AIState.Chase:
                    HandleChaseState();
                    break;

                case AIState.Attack:
                    HandleAttackState();
                    break;

                case AIState.ReturnToPatrol:
                    HandleReturnState();
                    break;
            }
        }

        private void HandleIdleState()
        {
            // ��� ���� - ���� �ð� �� ��Ʈ�ѷ� ��ȯ
            if (Time.time > lastAttackTime + 2f)
            {
                ChangeState(AIState.Patrol);
            }
        }

        private void HandlePatrolState()
        {
            // �÷��̾� ����
            if (DetectPlayer())
            {
                ChangeState(AIState.Chase);
                return;
            }

            // ��Ʈ�� ��� �� ���� �Ǵ� ��ֹ�/���� ����
            if (IsAtPatrolEnd() || isAtEdge || isBlocked)
            {
                TurnAround();
            }
        }

        private void HandleChaseState()
        {
            if (target == null)
            {
                ChangeState(AIState.ReturnToPatrol);
                return;
            }

            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            // ���� ���� ��
            if (distanceToTarget <= attackRange)
            {
                ChangeState(AIState.Attack);
            }
            // ���� ���� ���
            else if (distanceToTarget > loseTargetRange)
            {
                target = null;
                ChangeState(AIState.ReturnToPatrol);
            }
            // �����̳� ��ֹ� ����
            else if (isAtEdge || isBlocked)
            {
                // ���� �ߴ�
                ChangeState(AIState.Idle);
            }
            else
            {
                // Ÿ�� �������� �̵�
                UpdateDirectionToTarget();
            }
        }

        private void HandleAttackState()
        {
            if (target == null)
            {
                ChangeState(AIState.Patrol);
                return;
            }

            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            if (distanceToTarget > attackRange)
            {
                ChangeState(AIState.Chase);
            }
            else if (Time.time >= lastAttackTime + attackCooldown)
            {
                PerformAttack();
            }
        }

        private void HandleReturnState()
        {
            // ��Ʈ�� ���� �������� ����
            float distanceToStart = Vector2.Distance(transform.position, patrolStartPosition);

            if (distanceToStart < 0.5f)
            {
                ChangeState(AIState.Patrol);
            }
            else
            {
                // ���� ���� ���� ����
                movingRight = patrolStartPosition.x > transform.position.x;
            }
        }

        private void ChangeState(AIState newState)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[Enemy] State changed: {currentState} -> {newState}");
            }

            currentState = newState;
        }

        #endregion

        #region Movement

        private void ExecuteMovement()
        {
            if (!isGrounded) return;

            float speed = 0f;

            switch (currentState)
            {
                case AIState.Patrol:
                case AIState.ReturnToPatrol:
                    speed = patrolSpeed;
                    break;

                case AIState.Chase:
                    speed = chaseSpeed;
                    break;

                case AIState.Attack:
                case AIState.Idle:
                    speed = 0;
                    break;
            }

            // �̵� ����
            if (speed > 0)
            {
                float direction = movingRight ? 1 : -1;
                rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocityY);

                UpdateVisualDirection();
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocityY);
            }
        }

        private void TurnAround()
        {
            movingRight = !movingRight;
            UpdateVisualDirection();
        }

        private void UpdateVisualDirection()
        {
            if (!flipSpriteOnDirection) return;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !movingRight;
            }
            else
            {
                Vector3 scale = transform.localScale;
                scale.x = movingRight ? 1 : -1;
                transform.localScale = scale;
            }
        }

        private void UpdateDirectionToTarget()
        {
            if (target == null) return;

            movingRight = target.position.x > transform.position.x;
            UpdateVisualDirection();
        }

        #endregion

        #region Detection

        private bool DetectPlayer()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, targetLayer);

            foreach (var collider in colliders)
            {
                if (collider.CompareTag(targetTag))
                {
                    target = collider.transform;

                    if (showDebugLogs)
                    {
                        Debug.Log($"[Enemy] Player detected!");
                    }

                    return true;
                }
            }

            return false;
        }

        private void UpdateGroundCheck()
        {
            Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
            isGrounded = Physics2D.Raycast(checkPosition, Vector2.down, groundCheckDistance, groundLayer);
        }

        private void UpdateEdgeDetection()
        {
            // ���� �Ʒ� üũ (���� ����)
            float direction = movingRight ? 1 : -1;
            Vector2 edgeCheckPos = (Vector2)transform.position + new Vector2(edgeCheckOffset.x * direction, edgeCheckOffset.y);

            isAtEdge = !Physics2D.Raycast(edgeCheckPos, Vector2.down, edgeCheckDistance, groundLayer);

            // ���� �� üũ
            Vector2 wallCheckPos = (Vector2)transform.position;
            isBlocked = Physics2D.Raycast(wallCheckPos, Vector2.right * direction, edgeCheckDistance, groundLayer);
        }

        private bool IsAtPatrolEnd()
        {
            if (movingRight)
            {
                return transform.position.x >= patrolEndPosition.x;
            }
            else
            {
                return transform.position.x <= patrolStartPosition.x;
            }
        }

        #endregion

        #region Combat

        private async void PerformAttack()
        {
            lastAttackTime = Time.time;

            if (showDebugLogs)
            {
                Debug.Log($"[Enemy] Attacking! Damage: {attackDamage}");
            }

            // TODO: GAS �ý��۰� �����Ͽ� ������ ó��
            // �ӽ÷� ���� ������ ó��
            if (target != null)
            {
                // var healthComponent = target.GetComponent<HealthComponent>();
                // if (healthComponent != null)
                // {
                //     healthComponent.TakeDamage(attackDamage);
                // }
            }

            // ���� �ִϸ��̼� ���
            await Task.Delay(500);
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            // Detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Patrol points
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(patrolStartPosition, patrolEndPosition);
                Gizmos.DrawWireSphere(patrolStartPosition, 0.2f);
                Gizmos.DrawWireSphere(patrolEndPosition, 0.2f);
            }

            // Ground check
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 groundCheckPos = transform.position + (Vector3)groundCheckOffset;
            Gizmos.DrawLine(groundCheckPos, groundCheckPos + Vector3.down * groundCheckDistance);

            // Edge check
            float dir = movingRight ? 1 : -1;
            Vector3 edgeCheckPos = transform.position + new Vector3(edgeCheckOffset.x * dir, edgeCheckOffset.y, 0);
            Gizmos.color = isAtEdge ? Color.red : Color.green;
            Gizmos.DrawLine(edgeCheckPos, edgeCheckPos + Vector3.down * edgeCheckDistance);
        }

        #endregion
    }
}