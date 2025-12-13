using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Projectiles;
using GASPT.ResourceManagement;

namespace GASPT.Gameplay.Enemies
{
    /// <summary>
    /// 원거리 공격 적
    /// FSM: Idle → Patrol → Chase → RangedAttack → Retreat → Dead
    /// 일정 거리에서 투사체 발사, 플레이어가 너무 가까우면 후퇴
    /// </summary>
    public class RangedEnemy : PlatformerEnemy
    {
        // ====== 추가 상태 ======

        /// <summary>
        /// 원거리 적 전용 상태
        /// </summary>
        protected enum RangedEnemyState
        {
            RangedAttack,   // 원거리 공격
            Retreat         // 후퇴
        }

        private RangedEnemyState rangedState;


        // ====== 투사체 발사 ======

        [Header("투사체 설정")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float retreatSpeed = 2f;


        // ====== 타이머 ======

        private float idleTimer;
        private const float IdleDuration = 0.5f;


        // ====== Unity 생명주기 ======

        protected override void Start()
        {
            base.Start();

            // FirePoint 자동 생성 (없으면)
            if (firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(transform);
                firePointObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0f);
                firePoint = firePointObj.transform;

                if (showDebugLogs)
                    Debug.Log($"[RangedEnemy] {Data?.enemyName} FirePoint 자동 생성");
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
                    // 순찰 시작
                    break;

                case EnemyState.Chase:
                    if (showDebugLogs)
                        Debug.Log($"[RangedEnemy] {Data?.enemyName} 플레이어 추격 시작!");
                    break;

                case EnemyState.Attack:
                    Stop();
                    rangedState = RangedEnemyState.RangedAttack;
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
                    UpdateRangedAttack();
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
            float distanceToPlayer = GetDistanceToPlayer();

            // 거리 체크
            if (distanceToPlayer < Data.minDistance)
            {
                // 너무 가까우면 후퇴
                ChangeStateToRetreat();
                return;
            }
            else if (distanceToPlayer <= Data.optimalAttackDistance)
            {
                // 최적 공격 거리 진입
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


        // ====== RangedAttack 상태 ======

        private void UpdateRangedAttack()
        {
            float distanceToPlayer = GetDistanceToPlayer();

            // 거리 체크
            if (distanceToPlayer < Data.minDistance)
            {
                // 너무 가까우면 후퇴
                ChangeStateToRetreat();
                return;
            }
            else if (distanceToPlayer > Data.optimalAttackDistance)
            {
                // 최적 거리 벗어나면 추격
                ChangeState(EnemyState.Chase);
                return;
            }

            // 투사체 발사
            if (CanAttack())
            {
                FireProjectile();
            }
        }


        // ====== Retreat 상태 ======

        private void ChangeStateToRetreat()
        {
            currentState = EnemyState.Attack;
            rangedState = RangedEnemyState.Retreat;

            if (showDebugLogs)
                Debug.Log($"[RangedEnemy] {Data?.enemyName} 후퇴 시작!");
        }

        private void UpdateRetreat()
        {
            float distanceToPlayer = GetDistanceToPlayer();

            // 충분히 멀어지면 공격 상태로 복귀
            if (distanceToPlayer >= Data.optimalAttackDistance)
            {
                rangedState = RangedEnemyState.RangedAttack;
                return;
            }

            // 후퇴 이동 (PhysicsUpdate에서 처리)
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

                case EnemyState.Attack:
                    if (rangedState == RangedEnemyState.Retreat)
                    {
                        PhysicsRetreat();
                    }
                    else
                    {
                        // RangedAttack는 정지 상태 유지
                        Stop();
                    }
                    break;
            }
        }


        // ====== Patrol 물리 이동 ======

        private void PhysicsPatrol()
        {
            // 좌우 왕복 순찰
            Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
            Move(direction, Data.moveSpeed);

            // 일정 거리마다 방향 반전 (간단한 구현)
            if (Random.value < 0.01f)
            {
                Flip();
            }
        }


        // ====== Chase 물리 이동 ======

        private void PhysicsChase()
        {
            // 플레이어 방향으로 이동
            Vector2 direction = GetDirectionToPlayer();
            Move(direction, Data.chaseSpeed);
        }


        // ====== Retreat 물리 이동 ======

        private void PhysicsRetreat()
        {
            // 플레이어 반대 방향으로 이동
            Vector2 directionToPlayer = GetDirectionToPlayer();
            Vector2 retreatDirection = -directionToPlayer;

            Move(retreatDirection, retreatSpeed);

            if (showDebugLogs && Time.frameCount % 60 == 0)
                Debug.Log($"[RangedEnemy] {Data?.enemyName} 후퇴 중... 거리: {GetDistanceToPlayer():F2}");
        }


        // ====== 투사체 발사 ======

        /// <summary>
        /// 투사체 발사
        /// </summary>
        private void FireProjectile()
        {
            if (PoolManager.Instance == null)
            {
                Debug.LogError("[RangedEnemy] PoolManager를 찾을 수 없습니다!");
                return;
            }

            // EnemyProjectile 스폰 (ResourcePaths 상수 사용)
            EnemyProjectile projectile = PoolManager.Instance.Spawn<EnemyProjectile>(
                ResourcePaths.Prefabs.Projectiles.EnemyProjectile,
                firePoint.position,
                Quaternion.identity
            );

            if (projectile != null)
            {
                // 플레이어 방향으로 발사
                Vector2 direction = GetDirectionToPlayer();
                projectile.Launch(direction);

                lastAttackTime = Time.time;

                if (showDebugLogs)
                    Debug.Log($"[RangedEnemy] {Data?.enemyName} 투사체 발사!");
            }
            else
            {
                Debug.LogError($"[RangedEnemy] {Data?.enemyName} 투사체 스폰 실패");
            }
        }


        // ====== Gizmos ======

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (!showGizmos || Data == null) return;

            // 최적 공격 거리 (녹색)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, Data.optimalAttackDistance);

            // 최소 안전 거리 (주황색)
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, Data.minDistance);

            // FirePoint 표시
            if (firePoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(firePoint.position, 0.2f);
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (Data == null) return;

            // 현재 rangedState 표시
            if (Application.isPlaying && currentState == EnemyState.Attack)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f,
                    $"RangedState: {rangedState}");
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Print RangedEnemy Info")]
        private void DebugPrintRangedInfo()
        {
            DebugPrintPlatformerInfo();

            Debug.Log($"=== {Data?.enemyName} RangedEnemy Info ===\n" +
                     $"Ranged State: {rangedState}\n" +
                     $"Optimal Attack Distance: {Data?.optimalAttackDistance}\n" +
                     $"Min Distance: {Data?.minDistance}\n" +
                     $"Distance to Player: {GetDistanceToPlayer():F2}\n" +
                     $"Can Attack: {CanAttack()}\n" +
                     $"FirePoint: {(firePoint != null ? firePoint.position.ToString() : "null")}\n" +
                     $"==========================================");
        }

        [ContextMenu("Force Fire Projectile")]
        private void DebugForceFireProjectile()
        {
            FireProjectile();
        }
    }
}
