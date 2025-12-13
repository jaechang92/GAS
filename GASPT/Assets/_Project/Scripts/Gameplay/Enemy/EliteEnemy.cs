using UnityEngine;

namespace GASPT.Gameplay.Enemies
{
    /// <summary>
    /// 정예 적
    /// BasicMeleeEnemy의 강화 버전
    /// FSM: Idle → Patrol → Chase → Attack → (ChargeAttack/AreaAttack) → Dead
    /// 특수 스킬: 돌진 공격, 범위 공격
    /// </summary>
    public class EliteEnemy : PlatformerEnemy
    {
        // ====== 추가 상태 ======

        /// <summary>
        /// 정예 적 전용 상태
        /// </summary>
        protected enum EliteState
        {
            Normal,         // 일반 공격
            Charging,       // 돌진 중
            AreaAttacking   // 범위 공격 중
        }

        private EliteState eliteState = EliteState.Normal;


        // ====== 스킬 쿨다운 ======

        private float lastChargeTime = -100f;
        private float lastAreaAttackTime = -100f;


        // ====== 돌진 공격 ======

        [Header("돌진 공격 설정")]
        [SerializeField] private LayerMask playerLayer = 1 << 7; // Player Layer
        [SerializeField] private float chargeCheckRadius = 0.5f;

        private Vector3 chargeStartPosition;
        private Vector2 chargeDirection;
        private bool isCharging;


        // ====== 범위 공격 ======

        [Header("범위 공격 설정")]
        [SerializeField] private GameObject areaAttackEffectPrefab;


        // ====== 타이머 ======

        private float idleTimer;
        private const float IdleDuration = 0.5f;
        private float areaAttackTimer;
        private const float AreaAttackDuration = 0.5f;


        // ====== Unity 생명주기 ======

        protected override void Start()
        {
            base.Start();

            // 정예 적은 약간 더 크게 표시 (선택적)
            transform.localScale = Vector3.one * 1.2f;
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
                    eliteState = EliteState.Normal;
                    break;

                case EnemyState.Patrol:
                    eliteState = EliteState.Normal;
                    break;

                case EnemyState.Chase:
                    eliteState = EliteState.Normal;
                    if (showDebugLogs)
                        Debug.Log($"[EliteEnemy] {Data?.enemyName} 플레이어 추격 시작!");
                    break;

                case EnemyState.Attack:
                    Stop();
                    eliteState = EliteState.Normal;
                    break;

                case EnemyState.Dead:
                    Stop();
                    isCharging = false;
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
                    UpdateEliteAttack();
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

            // 추격 중 스킬 사용 시도
            TryUseSkill();

            // 추격 이동 (PhysicsUpdate에서 처리)
        }


        // ====== EliteAttack 상태 ======

        private void UpdateEliteAttack()
        {
            switch (eliteState)
            {
                case EliteState.Normal:
                    UpdateNormalAttack();
                    break;

                case EliteState.Charging:
                    UpdateCharging();
                    break;

                case EliteState.AreaAttacking:
                    UpdateAreaAttacking();
                    break;
            }
        }

        private void UpdateNormalAttack()
        {
            // 일반 공격 실행
            if (CanAttack())
            {
                AttackPlayer();
            }

            // 스킬 사용 시도
            TryUseSkill();

            // 플레이어가 공격 범위를 벗어나면 Chase로 복귀
            if (!IsPlayerInAttackRange())
            {
                ChangeState(EnemyState.Chase);
                return;
            }
        }


        // ====== 스킬 시스템 ======

        /// <summary>
        /// 스킬 사용 시도
        /// </summary>
        private void TryUseSkill()
        {
            // 30% 확률로 스킬 사용
            if (Random.value > 0.3f) return;

            bool canCharge = Time.time - lastChargeTime >= Data.chargeCooldown;
            bool canArea = Time.time - lastAreaAttackTime >= Data.areaCooldown;

            // 플레이어와의 거리 확인
            float distanceToPlayer = GetDistanceToPlayer();

            if (canCharge && distanceToPlayer > 3f && distanceToPlayer <= Data.detectionRange)
            {
                // 거리가 적당하면 돌진 공격
                StartChargeAttack();
            }
            else if (canArea && distanceToPlayer <= Data.areaAttackRadius + 1f)
            {
                // 가까우면 범위 공격
                StartAreaAttack();
            }
        }


        // ====== 돌진 공격 ======

        /// <summary>
        /// 돌진 공격 시작
        /// </summary>
        private void StartChargeAttack()
        {
            if (playerTransform == null) return;

            chargeStartPosition = transform.position;
            chargeDirection = GetDirectionToPlayer();
            isCharging = true;
            eliteState = EliteState.Charging;

            lastChargeTime = Time.time;

            if (showDebugLogs)
                Debug.Log($"[EliteEnemy] {Data?.enemyName} 돌진 공격 시작!");
        }

        /// <summary>
        /// 돌진 중 업데이트
        /// </summary>
        private void UpdateCharging()
        {
            // 돌진 거리 체크
            float chargedDistance = Vector3.Distance(chargeStartPosition, transform.position);

            if (chargedDistance >= Data.chargeDistance)
            {
                // 돌진 종료
                isCharging = false;
                eliteState = EliteState.Normal;
                ChangeState(EnemyState.Chase);

                if (showDebugLogs)
                    Debug.Log($"[EliteEnemy] {Data?.enemyName} 돌진 공격 종료 (거리: {chargedDistance:F2})");
            }

            // 충돌 체크 (PhysicsUpdate에서 이동)
        }

        /// <summary>
        /// 돌진 물리 이동
        /// </summary>
        private void PhysicsCharge()
        {
            if (rb == null) return;

            // 돌진 방향으로 빠르게 이동
            rb.linearVelocity = new Vector2(chargeDirection.x * Data.chargeSpeed, rb.linearVelocity.y);

            // 플레이어 충돌 체크
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, chargeCheckRadius, playerLayer);

            foreach (var hit in hits)
            {
                var player = hit.GetComponent<GASPT.Stats.PlayerStats>();
                if (player != null && !player.IsDead)
                {
                    // 돌진 공격 데미지 (1.5배)
                    int chargeDamage = Mathf.RoundToInt(Attack * 1.5f);
                    player.TakeDamage(chargeDamage);

                    // 돌진 종료
                    isCharging = false;
                    eliteState = EliteState.Normal;
                    ChangeState(EnemyState.Chase);

                    if (showDebugLogs)
                        Debug.Log($"[EliteEnemy] {Data?.enemyName} 돌진 공격 명중! 데미지: {chargeDamage}");

                    break;
                }
            }
        }


        // ====== 범위 공격 ======

        /// <summary>
        /// 범위 공격 시작
        /// </summary>
        private void StartAreaAttack()
        {
            eliteState = EliteState.AreaAttacking;
            areaAttackTimer = 0f;

            lastAreaAttackTime = Time.time;

            if (showDebugLogs)
                Debug.Log($"[EliteEnemy] {Data?.enemyName} 범위 공격 시전!");

            // 범위 공격 실행
            ExecuteAreaAttack();
        }

        /// <summary>
        /// 범위 공격 중 업데이트
        /// </summary>
        private void UpdateAreaAttacking()
        {
            areaAttackTimer += Time.deltaTime;

            if (areaAttackTimer >= AreaAttackDuration)
            {
                // 범위 공격 종료
                eliteState = EliteState.Normal;

                if (showDebugLogs)
                    Debug.Log($"[EliteEnemy] {Data?.enemyName} 범위 공격 종료");
            }
        }

        /// <summary>
        /// 범위 공격 실행
        /// </summary>
        private void ExecuteAreaAttack()
        {
            // 범위 내 플레이어 찾기
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position,
                Data.areaAttackRadius,
                playerLayer
            );

            foreach (var hit in hits)
            {
                var player = hit.GetComponent<GASPT.Stats.PlayerStats>();
                if (player != null && !player.IsDead)
                {
                    // 범위 공격 데미지 (2배)
                    int areaDamage = Attack * 2;
                    player.TakeDamage(areaDamage);

                    if (showDebugLogs)
                        Debug.Log($"[EliteEnemy] {Data?.enemyName} 범위 공격 명중! 데미지: {areaDamage}");
                }
            }

            // 시각 효과
            SpawnAreaAttackEffect();
        }

        /// <summary>
        /// 범위 공격 시각 효과 생성
        /// </summary>
        private void SpawnAreaAttackEffect()
        {
            if (areaAttackEffectPrefab == null) return;

            GameObject effect = Instantiate(areaAttackEffectPrefab, transform.position, Quaternion.identity);

            // 범위 크기에 맞게 스케일 조정
            effect.transform.localScale = Vector3.one * Data.areaAttackRadius * 2f;

            Destroy(effect, 1f);
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
                    if (isCharging)
                    {
                        PhysicsCharge();
                    }
                    else
                    {
                        PhysicsChase();
                    }
                    break;

                case EnemyState.Attack:
                    if (eliteState == EliteState.Charging)
                    {
                        PhysicsCharge();
                    }
                    else
                    {
                        // Normal Attack는 정지 상태
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

            // 일정 거리마다 방향 반전
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


        // ====== Gizmos ======

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (!showGizmos || Data == null) return;

            // 범위 공격 반경 (빨간색)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Data.areaAttackRadius);

            // 돌진 체크 반경 (주황색)
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, chargeCheckRadius);
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (Data == null) return;

            // 돌진 방향 표시 (isCharging일 때)
            if (Application.isPlaying && isCharging)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, chargeDirection * Data.chargeDistance);
            }

            // 현재 eliteState 표시
            if (Application.isPlaying && currentState == EnemyState.Attack)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f,
                    $"EliteState: {eliteState}");
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Print EliteEnemy Info")]
        private void DebugPrintEliteInfo()
        {
            DebugPrintPlatformerInfo();

            float timeSinceCharge = Time.time - lastChargeTime;
            float timeSinceArea = Time.time - lastAreaAttackTime;

            Debug.Log($"=== {Data?.enemyName} EliteEnemy Info ===\n" +
                     $"Elite State: {eliteState}\n" +
                     $"Is Charging: {isCharging}\n" +
                     $"Charge Cooldown: {(timeSinceCharge >= Data.chargeCooldown ? "Ready" : $"{Data.chargeCooldown - timeSinceCharge:F1}s")}\n" +
                     $"Area Attack Cooldown: {(timeSinceArea >= Data.areaCooldown ? "Ready" : $"{Data.areaCooldown - timeSinceArea:F1}s")}\n" +
                     $"Can Attack: {CanAttack()}\n" +
                     $"==========================================");
        }

        [ContextMenu("Force Charge Attack")]
        private void DebugForceChargeAttack()
        {
            lastChargeTime = -100f;
            StartChargeAttack();
        }

        [ContextMenu("Force Area Attack")]
        private void DebugForceAreaAttack()
        {
            lastAreaAttackTime = -100f;
            StartAreaAttack();
        }
    }
}
