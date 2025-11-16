using UnityEngine;

namespace GASPT.Gameplay.Enemy
{
    /// <summary>
    /// 플랫포머용 적 베이스 클래스
    /// Enemy.cs를 상속받아 데미지/드롭 시스템 재사용
    /// FSM 기반 AI 및 물리 이동 제공
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public abstract class PlatformerEnemy : Enemy
    {
        // ====== 상태 ======

        /// <summary>
        /// 적 AI 상태
        /// </summary>
        public enum EnemyState
        {
            Idle,       // 대기
            Patrol,     // 순찰
            Chase,      // 추격
            Attack,     // 공격
            Dead        // 사망
        }

        protected EnemyState currentState = EnemyState.Idle;
        protected EnemyState previousState = EnemyState.Idle;


        // ====== 컴포넌트 ======

        protected Rigidbody2D rb;
        protected Collider2D col;
        protected SpriteRenderer spriteRenderer;


        // ====== 플레이어 참조 ======

        protected Transform playerTransform;
        protected GASPT.Stats.PlayerStats playerStats;


        // ====== 이동 ======

        protected Vector2 moveDirection = Vector2.right;
        protected bool isFacingRight = true;
        protected Vector3 startPosition;


        // ====== 공격 ======

        protected float lastAttackTime;


        // ====== 디버그 ======

        [Header("디버그")]
        [SerializeField] protected bool showDebugLogs = false;
        [SerializeField] protected bool showGizmos = true;


        // ====== Unity 생명주기 ======

        protected virtual void Start()
        {
            InitializeComponents();
            FindPlayer();

            startPosition = transform.position;
            ChangeState(EnemyState.Idle);
        }

        protected virtual void Update()
        {
            if (IsDead)
            {
                if (currentState != EnemyState.Dead)
                {
                    ChangeState(EnemyState.Dead);
                }
                return;
            }

            UpdateState();
        }

        protected virtual void FixedUpdate()
        {
            if (IsDead) return;

            PhysicsUpdate();
        }


        // ====== 초기화 ======

        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        protected virtual void InitializeComponents()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Rigidbody2D 설정
            if (rb != null)
            {
                rb.gravityScale = 2f;
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            if (showDebugLogs)
                Debug.Log($"[PlatformerEnemy] {Data?.enemyName} 컴포넌트 초기화 완료");
        }

        /// <summary>
        /// 플레이어 찾기
        /// </summary>
        protected virtual void FindPlayer()
        {
            // PlayerStats로 플레이어 찾기
            playerStats = FindAnyObjectByType<GASPT.Stats.PlayerStats>();

            if (playerStats != null)
            {
                playerTransform = playerStats.transform;
                if (showDebugLogs)
                    Debug.Log($"[PlatformerEnemy] {Data?.enemyName} 플레이어 찾기 성공");
            }
            else
            {
                Debug.LogWarning($"[PlatformerEnemy] {Data?.enemyName} 플레이어를 찾을 수 없습니다!");
            }
        }


        // ====== FSM 상태 관리 ======

        /// <summary>
        /// 상태 변경
        /// </summary>
        protected virtual void ChangeState(EnemyState newState)
        {
            if (currentState == newState) return;

            previousState = currentState;
            currentState = newState;

            if (showDebugLogs)
                Debug.Log($"[PlatformerEnemy] {Data?.enemyName} 상태 변경: {previousState} → {currentState}");

            OnStateExit(previousState);
            OnStateEnter(currentState);
        }

        /// <summary>
        /// 상태 진입 시 호출
        /// </summary>
        protected virtual void OnStateEnter(EnemyState state)
        {
            // 하위 클래스에서 구현
        }

        /// <summary>
        /// 상태 퇴장 시 호출
        /// </summary>
        protected virtual void OnStateExit(EnemyState state)
        {
            // 하위 클래스에서 구현
        }

        /// <summary>
        /// 상태 업데이트 (매 프레임)
        /// </summary>
        protected virtual void UpdateState()
        {
            // 하위 클래스에서 구현
        }

        /// <summary>
        /// 물리 업데이트 (FixedUpdate)
        /// </summary>
        protected virtual void PhysicsUpdate()
        {
            // 하위 클래스에서 구현
        }


        // ====== 플레이어 감지 ======

        /// <summary>
        /// 플레이어 거리 계산
        /// </summary>
        protected virtual float GetDistanceToPlayer()
        {
            if (playerTransform == null) return float.MaxValue;
            return Vector2.Distance(transform.position, playerTransform.position);
        }

        /// <summary>
        /// 플레이어가 감지 범위 안에 있는지 확인
        /// </summary>
        protected virtual bool IsPlayerInDetectionRange()
        {
            if (Data == null) return false;
            return GetDistanceToPlayer() <= Data.detectionRange;
        }

        /// <summary>
        /// 플레이어가 공격 범위 안에 있는지 확인
        /// </summary>
        protected virtual bool IsPlayerInAttackRange()
        {
            if (Data == null) return false;
            return GetDistanceToPlayer() <= Data.attackRange;
        }

        /// <summary>
        /// 플레이어 방향 계산
        /// </summary>
        protected virtual Vector2 GetDirectionToPlayer()
        {
            if (playerTransform == null) return Vector2.right;
            return (playerTransform.position - transform.position).normalized;
        }


        // ====== 이동 ======

        /// <summary>
        /// 이동 (수평)
        /// </summary>
        protected virtual void Move(Vector2 direction, float speed)
        {
            if (rb == null) return;

            rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

            // 이동 방향에 따라 회전
            if (direction.x > 0.01f && !isFacingRight)
            {
                Flip();
            }
            else if (direction.x < -0.01f && isFacingRight)
            {
                Flip();
            }
        }

        /// <summary>
        /// 정지
        /// </summary>
        protected virtual void Stop()
        {
            if (rb == null) return;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        /// <summary>
        /// 좌우 반전
        /// </summary>
        protected virtual void Flip()
        {
            isFacingRight = !isFacingRight;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !isFacingRight;
            }
            else
            {
                // SpriteRenderer가 없으면 transform.localScale로 반전
                Vector3 scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }
        }


        // ====== 공격 ======

        /// <summary>
        /// 공격 쿨다운 체크
        /// </summary>
        protected virtual bool CanAttack()
        {
            if (Data == null) return false;
            return Time.time - lastAttackTime >= Data.attackCooldown;
        }

        /// <summary>
        /// 플레이어 공격 실행
        /// </summary>
        protected virtual void AttackPlayer()
        {
            if (playerStats == null) return;
            if (!CanAttack()) return;

            // 기존 Enemy.DealDamageTo() 사용
            DealDamageTo(playerStats);

            lastAttackTime = Time.time;

            if (showDebugLogs)
                Debug.Log($"[PlatformerEnemy] {Data?.enemyName} 플레이어 공격!");
        }


        // ====== Gizmos ======

        protected virtual void OnDrawGizmos()
        {
            if (!showGizmos || Data == null) return;

            // 감지 범위 (노란색)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Data.detectionRange);

            // 공격 범위 (빨간색)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Data.attackRange);

            // 순찰 범위 (파란색)
            if (Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(startPosition - Vector3.right * Data.patrolDistance,
                               startPosition + Vector3.right * Data.patrolDistance);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (Data == null) return;

            // 플레이어까지의 선 (마젠타)
            if (playerTransform != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, playerTransform.position);
            }

            // 현재 상태 표시
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
                $"State: {currentState}\nHP: {CurrentHp}/{MaxHp}");
        }


        // ====== 디버그 ======

        [ContextMenu("Print Platformer Info")]
        protected void DebugPrintPlatformerInfo()
        {
            Debug.Log($"=== {Data?.enemyName} Platformer Info ===\n" +
                     $"State: {currentState}\n" +
                     $"Position: {transform.position}\n" +
                     $"Player Distance: {GetDistanceToPlayer():F2}\n" +
                     $"In Detection Range: {IsPlayerInDetectionRange()}\n" +
                     $"In Attack Range: {IsPlayerInAttackRange()}\n" +
                     $"Facing Right: {isFacingRight}\n" +
                     $"=====================================");
        }
    }
}
