using UnityEngine;

namespace GASPT.Gameplay.Enemy
{
    /// <summary>
    /// 기본 근접 공격 적
    /// FSM: Idle → Patrol → Chase → Attack → Dead
    /// 순찰 중 플레이어 감지 시 추격, 공격 범위 진입 시 공격
    /// </summary>
    public class BasicMeleeEnemy : PlatformerEnemy
    {
        // ====== 순찰 ======

        private float patrolStartX;
        private float patrolLeftBound;
        private float patrolRightBound;
        private bool patrolMovingRight = true;


        // ====== 타이머 ======

        private float idleTimer;
        private const float IdleDuration = 0.5f;


        // ====== 낭떠러지 체크 ======

        [Header("낭떠러지 체크")]
        [SerializeField] private LayerMask groundLayer = 1; // Default layer
        [SerializeField] private float edgeCheckDistance = 0.5f;
        [SerializeField] private Vector2 edgeCheckOffset = new Vector2(0.5f, -0.5f);


        // ====== Unity 생명주기 ======

        protected override void Start()
        {
            base.Start();

            // 순찰 범위 계산
            if (Data != null)
            {
                patrolStartX = startPosition.x;
                patrolLeftBound = patrolStartX - Data.patrolDistance;
                patrolRightBound = patrolStartX + Data.patrolDistance;
            }
        }


        // ====== FSM 상태 진입/퇴장 ======

        protected override void OnStateEnter(EnemyState state)
        {
            base.OnStateEnter(state);

            switch (state)
            {
                case EnemyState.Idle:
                    Stop();
                    idleTimer = 0f;
                    break;

                case EnemyState.Patrol:
                    // 순찰 방향 설정 (시작 위치 기준)
                    if (transform.position.x < patrolStartX)
                    {
                        patrolMovingRight = true;
                    }
                    else if (transform.position.x > patrolStartX)
                    {
                        patrolMovingRight = false;
                    }
                    break;

                case EnemyState.Chase:
                    // 추격 시작
                    if (showDebugLogs)
                        Debug.Log($"[BasicMeleeEnemy] {Data?.enemyName} 플레이어 추격 시작!");
                    break;

                case EnemyState.Attack:
                    Stop();
                    break;

                case EnemyState.Dead:
                    Stop();
                    if (col != null)
                        col.enabled = false;
                    break;
            }
        }


        // ====== FSM 상태 업데이트 ======

        protected override void UpdateState()
        {
            base.UpdateState();

            switch (currentState)
            {
                case EnemyState.Idle:
                    UpdateIdle();
                    break;

                case EnemyState.Patrol:
                    UpdatePatrol();
                    break;

                case EnemyState.Chase:
                    UpdateChase();
                    break;

                case EnemyState.Attack:
                    UpdateAttack();
                    break;

                case EnemyState.Dead:
                    // 아무것도 안 함
                    break;
            }
        }


        // ====== Idle 상태 ======

        private void UpdateIdle()
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= IdleDuration)
            {
                ChangeState(EnemyState.Patrol);
            }
        }


        // ====== Patrol 상태 ======

        private void UpdatePatrol()
        {
            // 플레이어 감지 체크
            if (IsPlayerInDetectionRange())
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            // 순찰 이동 (PhysicsUpdate에서 처리)
        }


        // ====== Chase 상태 ======

        private void UpdateChase()
        {
            // 공격 범위 진입 체크
            if (IsPlayerInAttackRange())
            {
                ChangeState(EnemyState.Attack);
                return;
            }

            // 플레이어가 감지 범위를 벗어나면 Patrol로 복귀
            if (!IsPlayerInDetectionRange())
            {
                ChangeState(EnemyState.Patrol);
                return;
            }

            // 추격 이동 (PhysicsUpdate에서 처리)
        }


        // ====== Attack 상태 ======

        private void UpdateAttack()
        {
            // 공격 실행
            if (CanAttack())
            {
                AttackPlayer();
            }

            // 플레이어가 공격 범위를 벗어나면 Chase로 복귀
            if (!IsPlayerInAttackRange())
            {
                ChangeState(EnemyState.Chase);
                return;
            }
        }


        // ====== 물리 업데이트 ======

        protected override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            if (Data == null) return;

            switch (currentState)
            {
                case EnemyState.Patrol:
                    PhysicsPatrol();
                    break;

                case EnemyState.Chase:
                    PhysicsChase();
                    break;
            }
        }


        // ====== Patrol 물리 이동 ======

        private void PhysicsPatrol()
        {
            // 순찰 방향으로 이동
            Vector2 direction = patrolMovingRight ? Vector2.right : Vector2.left;
            Move(direction, Data.moveSpeed);

            // 순찰 범위 체크
            if (patrolMovingRight && transform.position.x >= patrolRightBound)
            {
                patrolMovingRight = false;
            }
            else if (!patrolMovingRight && transform.position.x <= patrolLeftBound)
            {
                patrolMovingRight = true;
            }

            // 낭떠러지 체크
            if (IsEdgeAhead())
            {
                // 방향 반전
                patrolMovingRight = !patrolMovingRight;

                if (showDebugLogs)
                    Debug.Log($"[BasicMeleeEnemy] {Data?.enemyName} 낭떠러지 감지 - 방향 반전!");
            }
        }


        // ====== Chase 물리 이동 ======

        private void PhysicsChase()
        {
            // 플레이어 방향으로 이동
            Vector2 direction = GetDirectionToPlayer();
            Move(direction, Data.chaseSpeed);
        }


        // ====== 낭떠러지 체크 ======

        /// <summary>
        /// 앞쪽에 낭떠러지가 있는지 체크
        /// </summary>
        private bool IsEdgeAhead()
        {
            if (col == null) return false;

            // 체크 위치 계산 (적의 앞쪽 바닥)
            Vector2 checkDirection = isFacingRight ? Vector2.right : Vector2.left;
            Vector2 checkOrigin = (Vector2)transform.position +
                                 new Vector2(checkDirection.x * edgeCheckOffset.x, edgeCheckOffset.y);

            // Raycast로 바닥 체크
            RaycastHit2D hit = Physics2D.Raycast(checkOrigin, Vector2.down, edgeCheckDistance, groundLayer);

            // 디버그 라인
            if (showGizmos)
            {
                Debug.DrawRay(checkOrigin, Vector2.down * edgeCheckDistance,
                             hit.collider != null ? Color.green : Color.red);
            }

            // 바닥이 없으면 낭떠러지
            return hit.collider == null;
        }


        // ====== Gizmos ======

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (!showGizmos || Data == null) return;

            // 낭떠러지 체크 위치 표시
            if (Application.isPlaying)
            {
                Vector2 checkDirection = isFacingRight ? Vector2.right : Vector2.left;
                Vector2 checkOrigin = (Vector2)transform.position +
                                     new Vector2(checkDirection.x * edgeCheckOffset.x, edgeCheckOffset.y);

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(checkOrigin, 0.1f);
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (Data == null) return;

            // 순찰 경로 표시 (초록색)
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Vector3 leftPoint = new Vector3(patrolLeftBound, transform.position.y, transform.position.z);
                Vector3 rightPoint = new Vector3(patrolRightBound, transform.position.y, transform.position.z);
                Gizmos.DrawLine(leftPoint, rightPoint);

                // 현재 방향 표시
                Gizmos.color = Color.yellow;
                Vector3 directionIndicator = transform.position +
                    (patrolMovingRight ? Vector3.right : Vector3.left) * 0.5f;
                Gizmos.DrawLine(transform.position, directionIndicator);
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Print BasicMelee Info")]
        private void DebugPrintBasicMeleeInfo()
        {
            DebugPrintPlatformerInfo();

            Debug.Log($"=== {Data?.enemyName} BasicMelee Info ===\n" +
                     $"Patrol Bounds: [{patrolLeftBound:F2}, {patrolRightBound:F2}]\n" +
                     $"Patrol Direction: {(patrolMovingRight ? "Right" : "Left")}\n" +
                     $"Edge Ahead: {IsEdgeAhead()}\n" +
                     $"Can Attack: {CanAttack()}\n" +
                     $"============================================");
        }

        [ContextMenu("Force State: Idle")]
        private void DebugForceIdle() => ChangeState(EnemyState.Idle);

        [ContextMenu("Force State: Patrol")]
        private void DebugForcePatrol() => ChangeState(EnemyState.Patrol);

        [ContextMenu("Force State: Chase")]
        private void DebugForceChase() => ChangeState(EnemyState.Chase);

        [ContextMenu("Force State: Attack")]
        private void DebugForceAttack() => ChangeState(EnemyState.Attack);
    }
}
