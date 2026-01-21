using UnityEngine;
using GASPT.Data;
using GASPT.Stats;
using GASPT.Gameplay.Enemies.AI.States;

namespace GASPT.Gameplay.Enemies.AI
{
    /// <summary>
    /// 적 AI 컨트롤러
    /// FSM 기반 상태 관리 컴포넌트
    /// PlatformerEnemy와 함께 사용하거나 독립적으로 사용 가능
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class EnemyAIController : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("AI 설정")]
        [SerializeField] [Tooltip("적 데이터 (ScriptableObject)")]
        private EnemyData enemyData;

        [SerializeField] [Tooltip("Enemy 컴포넌트 (선택사항, 없으면 자동 검색)")]
        private Enemy enemy;

        [Header("디버그")]
        [SerializeField] private bool showDebugLogs = false;
        [SerializeField] private bool showGizmos = true;


        // ====== 컨텍스트 ======

        private EnemyAIContext context;


        // ====== 현재 상태 ======

        private IEnemyAIState currentState;
        private IEnemyAIState previousState;


        // ====== 컴포넌트 캐시 ======

        private Rigidbody2D rb;
        private Collider2D col;
        private SpriteRenderer spriteRenderer;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 상태 이름 (디버깅용)
        /// </summary>
        public string CurrentStateName => currentState?.StateName ?? "None";

        /// <summary>
        /// AI 컨텍스트 읽기 전용
        /// </summary>
        public EnemyAIContext Context => context;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeContext();
            InitializePatrolBounds();
            SetInitialState();
        }

        private void Update()
        {
            if (currentState == null) return;

            // 상태 전환 체크
            IEnemyAIState nextState = currentState.CheckTransitions(context);
            if (nextState != null)
            {
                ChangeState(nextState);
            }

            // 현재 상태 업데이트
            currentState.Update(context);
        }

        private void FixedUpdate()
        {
            if (currentState == null) return;

            // 물리 업데이트
            currentState.PhysicsUpdate(context);
        }


        // ====== 초기화 ======

        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        private void InitializeComponents()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Enemy 컴포넌트 자동 검색
            if (enemy == null)
            {
                enemy = GetComponent<Enemy>();
            }

            // Rigidbody2D 설정
            if (rb != null)
            {
                rb.gravityScale = 2f;
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
        }

        /// <summary>
        /// AI 컨텍스트 초기화
        /// </summary>
        private void InitializeContext()
        {
            context = new EnemyAIContext
            {
                Enemy = enemy,
                Rigidbody = rb,
                Transform = transform,
                SpriteRenderer = spriteRenderer,
                Collider = col,
                Data = enemyData ?? enemy?.Data,
                StartPosition = transform.position,
                ShowDebugLogs = showDebugLogs,
                ShowGizmos = showGizmos
            };

            // 플레이어 찾기
            FindPlayer();
        }

        /// <summary>
        /// 순찰 범위 초기화
        /// </summary>
        private void InitializePatrolBounds()
        {
            if (context.Data != null)
            {
                float patrolDistance = context.Data.patrolDistance;
                context.PatrolLeftBound = context.StartPosition.x - patrolDistance;
                context.PatrolRightBound = context.StartPosition.x + patrolDistance;
            }
        }

        /// <summary>
        /// 초기 상태 설정
        /// </summary>
        private void SetInitialState()
        {
            ChangeState(new IdleState());
        }

        /// <summary>
        /// 플레이어 찾기
        /// </summary>
        private void FindPlayer()
        {
            PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();

            if (playerStats != null)
            {
                context.PlayerTransform = playerStats.transform;
                context.PlayerStats = playerStats;

                if (showDebugLogs)
                {
                    Debug.Log($"[EnemyAIController] {context.Data?.enemyName} 플레이어 찾기 성공");
                }
            }
            else
            {
                Debug.LogWarning($"[EnemyAIController] {context.Data?.enemyName} 플레이어를 찾을 수 없습니다!");
            }
        }


        // ====== 상태 관리 ======

        /// <summary>
        /// 상태 변경
        /// </summary>
        /// <param name="newState">새 상태</param>
        public void ChangeState(IEnemyAIState newState)
        {
            if (newState == null) return;

            // 이전 상태 퇴장
            if (currentState != null)
            {
                currentState.Exit(context);
                previousState = currentState;
            }

            // 새 상태 진입
            currentState = newState;
            currentState.Enter(context);

            if (showDebugLogs)
            {
                string prevName = previousState?.StateName ?? "None";
                Debug.Log($"[EnemyAIController] {context.Data?.enemyName} 상태 변경: {prevName} → {currentState.StateName}");
            }
        }

        /// <summary>
        /// 특정 상태로 강제 전환
        /// </summary>
        /// <typeparam name="T">상태 타입</typeparam>
        public void ForceChangeState<T>() where T : IEnemyAIState, new()
        {
            ChangeState(new T());
        }


        // ====== 외부 설정 ======

        /// <summary>
        /// EnemyData 설정 (런타임에서 호출 가능)
        /// </summary>
        /// <param name="data">적 데이터</param>
        public void SetEnemyData(EnemyData data)
        {
            enemyData = data;

            if (context != null)
            {
                context.Data = data;
                InitializePatrolBounds();
            }
        }

        /// <summary>
        /// Enemy 컴포넌트 설정
        /// </summary>
        /// <param name="enemyComponent">Enemy 컴포넌트</param>
        public void SetEnemy(Enemy enemyComponent)
        {
            enemy = enemyComponent;

            if (context != null)
            {
                context.Enemy = enemyComponent;

                // EnemyData가 없으면 Enemy에서 가져오기
                if (context.Data == null)
                {
                    context.Data = enemyComponent?.Data;
                    InitializePatrolBounds();
                }
            }
        }


        // ====== Gizmos ======

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            EnemyData data = enemyData ?? enemy?.Data;
            if (data == null) return;

            // 감지 범위 (노란색)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, data.detectionRange);

            // 공격 범위 (빨간색)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, data.attackRange);

            // 순찰 범위 (파란색)
            if (Application.isPlaying && context != null)
            {
                Gizmos.color = Color.blue;
                Vector3 startPos = context.StartPosition;
                Gizmos.DrawLine(
                    startPos - Vector3.right * data.patrolDistance,
                    startPos + Vector3.right * data.patrolDistance
                );
            }
        }

        private void OnDrawGizmosSelected()
        {
            EnemyData data = enemyData ?? enemy?.Data;
            if (data == null) return;

            // 플레이어까지의 선 (마젠타)
            if (context?.PlayerTransform != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, context.PlayerTransform.position);
            }

            // 현재 상태 표시
            if (Application.isPlaying)
            {
#if UNITY_EDITOR
                string stateInfo = $"State: {CurrentStateName}";
                if (enemy != null)
                {
                    stateInfo += $"\nHP: {enemy.CurrentHp}/{enemy.MaxHp}";
                }
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, stateInfo);
#endif
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Print AI Controller Info")]
        private void DebugPrintInfo()
        {
            Debug.Log($"=== EnemyAIController Info ===\n" +
                     $"Enemy: {context?.Data?.enemyName}\n" +
                     $"Current State: {CurrentStateName}\n" +
                     $"Previous State: {previousState?.StateName ?? "None"}\n" +
                     $"Position: {transform.position}\n" +
                     $"Player Distance: {context?.GetDistanceToPlayer():F2}\n" +
                     $"In Detection Range: {context?.IsPlayerInDetectionRange()}\n" +
                     $"In Attack Range: {context?.IsPlayerInAttackRange()}\n" +
                     $"Can Attack: {context?.CanAttack()}\n" +
                     $"=================================");
        }

        [ContextMenu("Force State: Idle")]
        private void DebugForceIdle() => ForceChangeState<IdleState>();

        [ContextMenu("Force State: Patrol")]
        private void DebugForcePatrol() => ForceChangeState<PatrolState>();

        [ContextMenu("Force State: Chase")]
        private void DebugForceChase() => ForceChangeState<ChaseState>();

        [ContextMenu("Force State: Attack")]
        private void DebugForceAttack() => ForceChangeState<AttackState>();

        [ContextMenu("Force State: Dead")]
        private void DebugForceDead() => ForceChangeState<DeadState>();
    }
}
