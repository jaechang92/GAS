using Combat.Core;
using Enemy.Data;
using FSM.Core;
using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// Enemy 컨트롤러
    /// FSM 기반 적 AI 제어 및 전투 시스템 통합
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy 데이터")]
        [SerializeField] private EnemyData enemyData;

        [Header("타겟 설정")]
        [SerializeField] private Transform target; // 플레이어

        [Header("디버그")]
        [SerializeField] private bool showDebugInfo = true;

        // === 핵심 컴포넌트 ===
        private Rigidbody2D rb;
        private Collider2D col;
        private SpriteRenderer spriteRenderer;

        // === Combat 시스템 ===
        private HealthSystem healthSystem;

        // === FSM ===
        private StateMachine stateMachine;
        private EnemyStateType currentState = EnemyStateType.Idle;

        // === 상태 변수 ===
        private int facingDirection = 1; // 1: 오른쪽, -1: 왼쪽
        private bool isAttacking = false;
        private float lastAttackTime = 0f;

        // === 감지 변수 ===
        private float detectionTimer = 0f;

        // === 정찰 변수 ===
        private Vector3 patrolStartPosition;
        private bool patrolMovingRight = true;

        // === Combat 데이터 임시 저장 ===
        private DamageData pendingDamageData;

        // === 프로퍼티 ===
        public EnemyData Data => enemyData;
        public Transform Target { get => target; set => target = value; }
        public Rigidbody2D Rigidbody => rb;
        public Collider2D Collider => col;
        public HealthSystem Health => healthSystem;
        public int FacingDirection { get => facingDirection; set => facingDirection = value; }
        public bool IsAlive => healthSystem?.IsAlive ?? true;
        public bool IsAttacking { get => isAttacking; set => isAttacking = value; }
        public float LastAttackTime { get => lastAttackTime; set => lastAttackTime = value; }
        public Vector3 PatrolStartPosition { get => patrolStartPosition; set => patrolStartPosition = value; }
        public bool PatrolMovingRight { get => patrolMovingRight; set => patrolMovingRight = value; }

        // === 거리 계산 ===
        public float DistanceToTarget => target != null ? Vector2.Distance(transform.position, target.position) : float.MaxValue;

        // === 이벤트 ===
        public event System.Action<EnemyStateType, EnemyStateType> OnStateChanged;
        public event System.Action OnDeath;

        private void Awake()
        {
            InitializeComponents();
            InitializeFSM();
        }

        private void Start()
        {
            // 정찰 시작 위치 저장
            patrolStartPosition = transform.position;

            // 초기 상태로 전환
            if (stateMachine != null)
            {
                stateMachine.StartStateMachine(EnemyStateType.Idle.ToString());
                LogDebug($"Enemy FSM 시작 - 초기 상태: {currentState}");
            }
        }

        private void Update()
        {
            if (!IsAlive || enemyData == null) return;

            // 주기적 플레이어 감지
            detectionTimer += Time.deltaTime;

            if (detectionTimer >= enemyData.detectionInterval)
            {
                detectionTimer = 0f;
                CheckPlayerDetection();
            }
        }

        private void OnDestroy()
        {
            // HealthSystem 이벤트 구독 해제
            if (healthSystem != null)
            {
                healthSystem.OnDamaged -= HandleDamaged;
                healthSystem.OnDeath -= HandleDeath;
            }

            stateMachine?.Stop();
        }

        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        private void InitializeComponents()
        {
            // Rigidbody2D
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 3f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            // Collider2D
            col = GetComponent<Collider2D>();

            // SpriteRenderer
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            // 적 색상 설정
            if (enemyData != null && spriteRenderer != null)
            {
                spriteRenderer.color = enemyData.enemyColor;
            }

            // HealthSystem 초기화
            healthSystem = GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                healthSystem = gameObject.AddComponent<HealthSystem>();
            }

            // 체력 설정
            if (enemyData != null && healthSystem != null)
            {
                healthSystem.SetMaxHealth(enemyData.maxHealth);
                healthSystem.SetCurrentHealth(enemyData.maxHealth);
            }

            // HealthSystem 이벤트 구독
            if (healthSystem != null)
            {
                healthSystem.OnDamaged += HandleDamaged;
                healthSystem.OnDeath += HandleDeath;
            }

            LogDebug("Enemy 컴포넌트 초기화 완료");
        }

        /// <summary>
        /// FSM 초기화
        /// </summary>
        private void InitializeFSM()
        {
            // StateMachine은 MonoBehaviour이므로 AddComponent로 추가
            stateMachine = gameObject.AddComponent<StateMachine>();

            // 상태 생성 및 등록
            var idleState = new EnemyIdleState();
            var patrolState = new EnemyPatrolState();
            var traceState = new EnemyTraceState(); // Chase -> Trace로 변경
            var attackState = new EnemyAttackState();
            var hitState = new EnemyHitState();
            var deathState = new EnemyDeathState();

            stateMachine.AddState(idleState);
            stateMachine.AddState(patrolState);
            stateMachine.AddState(traceState);
            stateMachine.AddState(attackState);
            stateMachine.AddState(hitState);
            stateMachine.AddState(deathState);

            LogDebug("Enemy FSM 초기화 완료");
        }

        /// <summary>
        /// 상태 전환
        /// </summary>
        public void ChangeState(EnemyStateType newState)
        {
            if (!IsAlive && newState != EnemyStateType.Death) return;

            var previousState = currentState;
            currentState = newState;

            stateMachine.ForceTransitionTo(newState.ToString());

            OnStateChanged?.Invoke(previousState, newState);
            LogDebug($"상태 전환: {previousState} → {newState}");
        }

        /// <summary>
        /// 공격 쿨다운 확인
        /// </summary>
        public bool CanAttack()
        {
            if (enemyData == null) return false;
            return Time.time - lastAttackTime >= enemyData.attackCooldown;
        }

        /// <summary>
        /// 스프라이트 방향 업데이트
        /// </summary>
        public void UpdateSpriteDirection()
        {
            if (spriteRenderer == null) return;

            // FacingDirection에 따라 스프라이트 Flip
            spriteRenderer.flipX = facingDirection < 0;
        }

        /// <summary>
        /// 주기적 플레이어 감지 및 상태 전환
        /// </summary>
        private void CheckPlayerDetection()
        {
            if (target == null) return;

            float distance = DistanceToTarget;

            // 현재 상태에 따른 전환 로직
            switch (currentState)
            {
                case EnemyStateType.Idle:
                    // Idle → Trace or Patrol
                    if (distance <= enemyData.detectionRange)
                    {
                        LogDebug($"플레이어 감지! Trace로 전환 (거리: {distance:F2})");
                        ChangeState(EnemyStateType.Trace);
                    }
                    else if (enemyData.enablePatrol)
                    {
                        LogDebug("Patrol로 전환");
                        ChangeState(EnemyStateType.Patrol);
                    }
                    break;

                case EnemyStateType.Patrol:
                    // Patrol → Trace
                    if (distance <= enemyData.detectionRange)
                    {
                        LogDebug($"정찰 중 플레이어 감지! Trace로 전환 (거리: {distance:F2})");
                        ChangeState(EnemyStateType.Trace);
                    }
                    break;

                case EnemyStateType.Trace:
                    // Trace → Attack or Patrol/Idle
                    if (distance <= enemyData.attackRange && CanAttack())
                    {
                        LogDebug($"공격 범위 진입! Attack으로 전환 (거리: {distance:F2})");
                        ChangeState(EnemyStateType.Attack);
                    }
                    else if (distance > enemyData.chaseRange)
                    {
                        LogDebug($"추적 범위 이탈 (거리: {distance:F2})");
                        ChangeState(enemyData.enablePatrol ? EnemyStateType.Patrol : EnemyStateType.Idle);
                    }
                    break;

                case EnemyStateType.Attack:
                    // Attack → Trace (거리 이탈 시 즉시 취소)
                    if (distance > enemyData.attackRange + 1f) // 여유 거리 1m
                    {
                        LogDebug($"공격 중 타겟 이탈! Trace로 전환 (거리: {distance:F2})");
                        ChangeState(EnemyStateType.Trace);
                    }
                    break;

                case EnemyStateType.Hit:
                    // Hit 상태에서는 감지하지 않음 (HitState에서 처리)
                    break;

                case EnemyStateType.Death:
                    // Death 상태에서는 아무것도 하지 않음
                    break;
            }
        }

        /// <summary>
        /// 데미지 받았을 때 처리
        /// </summary>
        private void HandleDamaged(DamageData damageData)
        {
            LogDebug($"피격! 데미지: {damageData.amount}");

            // DamageData 임시 저장 (EnemyHitState가 EnterState에서 가져감)
            pendingDamageData = damageData;

            // Hit 상태로 전환
            ChangeState(EnemyStateType.Hit);
        }

        /// <summary>
        /// 대기 중인 DamageData 가져오기 (EnemyHitState용)
        /// </summary>
        public DamageData GetPendingDamageData()
        {
            return pendingDamageData;
        }

        /// <summary>
        /// 사망 처리
        /// </summary>
        private void HandleDeath()
        {
            LogDebug("Enemy 사망");
            ChangeState(EnemyStateType.Death);
            OnDeath?.Invoke();
        }

        private void LogDebug(string message)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[{enemyData?.enemyName ?? "Enemy"}] {message}");
            }
        }

        private void OnDrawGizmos()
        {
            if (enemyData == null) return;

            // 감지 범위
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);

            // 추적 범위
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, enemyData.chaseRange);

            // 공격 범위
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
        }
    }
}

// Debug.DrawCircle 확장 메서드
public static class DebugExtensions
{
    public static void DrawCircle(Vector3 center, float radius, Color color, int segments = 36)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Debug.DrawLine(prevPoint, newPoint, color);
            prevPoint = newPoint;
        }
    }
}
