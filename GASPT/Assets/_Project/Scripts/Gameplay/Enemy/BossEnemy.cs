using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Projectiles;
using GASPT.Gameplay.Boss;
using GASPT.UI;

namespace GASPT.Gameplay.Enemies
{
    /// <summary>
    /// 보스 적
    /// 3단계 Phase 전투 시스템
    /// Phase 1: 근접 공격 + 원거리 공격
    /// Phase 2: 돌진 공격 + 소환
    /// Phase 3: 광폭화 + 범위 공격
    /// </summary>
    public class BossEnemy : PlatformerEnemy
    {
        // ====== Phase Controller ======

        private BossPhaseController phaseController;


        // ====== UI ======

        [Header("보스 UI")]
        [SerializeField] [Tooltip("보스 체력바 프리팹")]
        private BossHealthBar bossHealthBarPrefab;

        private BossHealthBar bossHealthBarInstance;


        // ====== Phase 1 패턴: 원거리 공격 ======

        private float lastRangedAttackTime;


        // ====== Phase 2 패턴: 돌진 공격 ======

        private float lastChargeAttackTime;
        private bool isCharging = false;
        private Vector3 chargeStartPos;
        private Vector3 chargeTargetPos;
        private Vector2 chargeDirection;


        // ====== Phase 2 패턴: 소환 ======

        [Header("Phase 2: 소환")]
        [SerializeField] [Tooltip("최대 소환 수")]
        private int maxSummonCount = 3;

        [SerializeField] [Tooltip("소환할 적 타입 (BasicMeleeEnemy)")]
        private GameObject minionPrefab;

        [SerializeField] [Tooltip("소환할 적의 EnemyData")]
        private GASPT.Data.EnemyData minionData;

        private float lastSummonTime;
        private int currentSummonCount = 0;


        // ====== Phase 3 패턴: 범위 공격 ======

        private float lastAreaAttackTime;


        // ====== Unity 생명주기 ======

        protected override void Start()
        {
            base.Start();

            // Phase Controller 초기화
            InitializePhaseController();

            // BossHealthBar 생성
            CreateBossHealthBar();

            Debug.Log($"[BossEnemy] {Data?.enemyName} 보스 초기화 완료");
        }

        protected override void Update()
        {
            base.Update();

            if (IsDead) return;

            // 돌진 중이면 돌진 처리
            if (isCharging)
            {
                UpdateCharge();
            }
            else
            {
                // 돌진 중이 아닐 때만 Phase별 패턴 실행
                ExecutePhasePatterns();
            }
        }

        /// <summary>
        /// 상태 업데이트 (기본 추격 로직)
        /// </summary>
        protected override void UpdateState()
        {
            // 돌진 중에는 상태 업데이트 안 함
            if (isCharging) return;

            // 플레이어가 감지 범위 안에 있으면 추격
            if (IsPlayerInDetectionRange())
            {
                ChasePlayer();

                // 공격 범위 안에 들어오면 공격
                if (IsPlayerInAttackRange() && CanAttack())
                {
                    AttackPlayer();
                }
            }
        }

        /// <summary>
        /// 플레이어 추격 (기본 이동)
        /// </summary>
        private void ChasePlayer()
        {
            if (playerTransform == null || Data == null || rb == null) return;

            // 플레이어 방향 계산
            Vector2 direction = GetDirectionToPlayer();

            // 수평 이동 (X축만)
            float horizontalDirection = Mathf.Sign(direction.x);

            // Rigidbody2D로 이동
            Vector2 moveVelocity = new Vector2(
                horizontalDirection * Data.chaseSpeed,
                rb.linearVelocity.y // Y축은 중력 유지
            );

            rb.linearVelocity = moveVelocity;

            // 스프라이트 방향 전환
            if (horizontalDirection > 0.01f && !isFacingRight)
            {
                Flip();
            }
            else if (horizontalDirection < -0.01f && isFacingRight)
            {
                Flip();
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// Phase Controller 초기화
        /// </summary>
        private void InitializePhaseController()
        {
            // V2 기본 생성자 사용 (V1 호환 3페이즈)
            phaseController = new BossPhaseController();

            // HP 변경 시 Phase 업데이트
            OnHpChanged += (currentHp, maxHp) =>
            {
                phaseController.UpdatePhase(currentHp, maxHp);
            };

            // Phase 전환 이벤트 구독
            phaseController.OnPhaseChanged += OnPhaseChanged;

            Debug.Log($"[BossEnemy] Phase Controller 초기화 완료 (V2)");
        }

        /// <summary>
        /// BossHealthBar 생성
        /// </summary>
        private void CreateBossHealthBar()
        {
            if (bossHealthBarPrefab == null)
            {
                Debug.LogWarning("[BossEnemy] bossHealthBarPrefab이 null입니다. Inspector에서 설정하세요.");
                return;
            }

            // Canvas 찾기
            Canvas canvas = FindAnyObjectByType<Canvas>();

            if (canvas == null)
            {
                Debug.LogWarning("[BossEnemy] Canvas를 찾을 수 없습니다. BossHealthBar를 생성할 수 없습니다.");
                return;
            }

            // BossHealthBar 생성
            bossHealthBarInstance = Instantiate(bossHealthBarPrefab, canvas.transform);

            // 초기화
            bossHealthBarInstance.Initialize(this);

            Debug.Log($"[BossEnemy] {Data?.enemyName} 체력바 생성 완료");
        }


        // ====== Phase 전환 ======

        /// <summary>
        /// Phase 전환 시 호출
        /// </summary>
        /// <param name="newPhaseIndex">새 페이즈 인덱스 (0부터 시작)</param>
        private void OnPhaseChanged(int newPhaseIndex)
        {
            int phaseNumber = newPhaseIndex + 1; // 표시용 (1부터 시작)
            Debug.Log($"[BossEnemy] {Data?.enemyName} Phase 전환: Phase {phaseNumber}");

            // Phase별 초기화 (인덱스 기반)
            switch (newPhaseIndex)
            {
                case 0: // Phase 1
                    // Phase 1 진입
                    break;

                case 1: // Phase 2
                    // Phase 2 진입 - 광폭화 이펙트
                    Debug.Log($"[BossEnemy] Phase 2 진입! 공격 속도 증가!");
                    break;

                case 2: // Phase 3
                    // Phase 3 진입 - 최종 광폭화
                    Debug.Log($"[BossEnemy] Phase 3 진입! 광폭화 상태!");
                    break;
            }
        }


        // ====== Phase별 패턴 실행 ======

        /// <summary>
        /// 현재 Phase에 맞는 패턴 실행
        /// </summary>
        private void ExecutePhasePatterns()
        {
            if (phaseController == null) return;

            int currentPhaseIndex = phaseController.CurrentPhaseIndex;

            switch (currentPhaseIndex)
            {
                case 0: // Phase 1: 근접 공격 + 원거리 공격
                    if (CanUseRangedAttack())
                    {
                        ExecuteRangedAttack();
                    }
                    break;

                case 1: // Phase 2: 근접 + 원거리 + 돌진 + 소환
                    if (CanUseRangedAttack())
                    {
                        ExecuteRangedAttack();
                    }
                    if (CanUseChargeAttack())
                    {
                        ExecuteChargeAttack();
                    }
                    if (CanUseSummon())
                    {
                        ExecuteSummon();
                    }
                    break;

                case 2: // Phase 3: 근접 + 원거리 + 돌진 + 범위 공격
                    if (CanUseRangedAttack())
                    {
                        ExecuteRangedAttack();
                    }
                    if (CanUseChargeAttack())
                    {
                        ExecuteChargeAttack();
                    }
                    if (CanUseAreaAttack())
                    {
                        ExecuteAreaAttack();
                    }
                    break;
            }
        }


        // ====== Phase 1: 원거리 공격 ======

        private bool CanUseRangedAttack()
        {
            if (playerTransform == null) return false;
            if (Data == null) return false;
            if (Time.time - lastRangedAttackTime < Data.bossRangedCooldown) return false;
            if (!IsPlayerInDetectionRange()) return false;
            return true;
        }

        private void ExecuteRangedAttack()
        {
            if (playerTransform == null || Data == null) return;

            // 투사체 발사 방향
            Vector2 direction = (playerTransform.position - transform.position).normalized;

            // 풀에서 투사체 가져오기
            var projectile = PoolManager.Instance.Spawn<EnemyProjectile>(
                transform.position,
                Quaternion.identity
            );

            if (projectile != null)
            {
                projectile.Initialize(direction, Data.bossProjectileSpeed, Data.bossProjectileDamage);

                if (showDebugLogs)
                    Debug.Log($"[BossEnemy] 원거리 공격 발사! 방향: {direction}");
            }
            else
            {
                Debug.LogWarning("[BossEnemy] EnemyProjectile을 풀에서 가져올 수 없습니다!");
            }

            lastRangedAttackTime = Time.time;
        }


        // ====== Phase 2: 돌진 공격 ======

        private bool CanUseChargeAttack()
        {
            if (playerTransform == null) return false;
            if (Data == null) return false;
            if (isCharging) return false;
            if (Time.time - lastChargeAttackTime < Data.bossChargeCooldown) return false;
            if (!IsPlayerInDetectionRange()) return false;
            return true;
        }

        private void ExecuteChargeAttack()
        {
            if (playerTransform == null || Data == null || col == null) return;

            // 돌진 시작 위치 설정
            chargeStartPos = transform.position;

            // 플레이어 방향 계산 (수평만)
            Vector2 directionToPlayer = new Vector2(
                playerTransform.position.x - transform.position.x,
                0f
            );

            float distanceToPlayer = directionToPlayer.magnitude;

            // 돌진 거리 결정 (플레이어까지의 거리 vs 최대 돌진 거리)
            float chargeDistance = Mathf.Min(distanceToPlayer, Data.bossChargeDistance);
            chargeDirection = directionToPlayer.normalized;

            // CapsuleCollider2D 크기 가져오기
            CapsuleCollider2D capsule = col as CapsuleCollider2D;
            if (capsule == null)
            {
                Debug.LogWarning("[BossEnemy] CapsuleCollider2D가 아닙니다. 돌진 취소!");
                return;
            }

            // Collider 크기 (스케일 적용)
            Vector2 capsuleSize = capsule.size;
            Vector2 scaledSize = new Vector2(
                capsuleSize.x * Mathf.Abs(transform.localScale.x),
                capsuleSize.y * Mathf.Abs(transform.localScale.y)
            );

            // CapsuleCast2D로 보스 크기를 고려한 장애물 체크
            RaycastHit2D hit = Physics2D.CapsuleCast(
                transform.position,                    // 시작 위치
                scaledSize,                            // Capsule 크기
                CapsuleDirection2D.Vertical,           // 방향
                0f,                                    // 회전 각도
                chargeDirection,                       // Cast 방향
                chargeDistance,                        // Cast 거리
                LayerMask.GetMask("Ground", "Platform") // 지형 레이어
            );

            if (hit.collider != null)
            {
                // 장애물이 있으면 돌진 취소
                if (showDebugLogs)
                    Debug.Log($"[BossEnemy] 돌진 경로에 장애물 감지: {hit.collider.name} (거리: {hit.distance:F1}m). 돌진 취소!");

                lastChargeAttackTime = Time.time; // 쿨다운만 적용
                return;
            }

            // 목표 위치 설정 (Y축은 현재 위치 유지)
            chargeTargetPos = chargeStartPos + new Vector3(
                chargeDirection.x * chargeDistance,
                0f,
                0f
            );

            isCharging = true;
            lastChargeAttackTime = Time.time;

            if (showDebugLogs)
                Debug.Log($"[BossEnemy] 돌진 공격 시작! {chargeStartPos} → {chargeTargetPos} (거리: {chargeDistance:F1}m)");
        }

        private void UpdateCharge()
        {
            if (Data == null || rb == null) return;

            // Rigidbody2D로 이동 (물리 기반)
            Vector2 moveVelocity = chargeDirection * Data.bossChargeSpeed;
            moveVelocity.y = rb.linearVelocity.y; // Y축은 중력 유지

            rb.linearVelocity = moveVelocity;

            // 도착 확인 (X축 거리만 체크)
            float horizontalDistance = Mathf.Abs(transform.position.x - chargeTargetPos.x);
            if (horizontalDistance < 0.5f)
            {
                isCharging = false;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // 돌진 종료 시 정지

                if (showDebugLogs)
                    Debug.Log($"[BossEnemy] 돌진 공격 완료!");
            }
        }

        /// <summary>
        /// 충돌 처리 (돌진 중 장애물과 충돌 시 중단)
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 돌진 중일 때만 충돌 처리
            if (!isCharging) return;

            // Ground 또는 Platform 레이어와 충돌 시 돌진 중단
            int groundLayer = LayerMask.NameToLayer("Ground");
            int platformLayer = LayerMask.NameToLayer("Platform");

            if (collision.gameObject.layer == groundLayer || collision.gameObject.layer == platformLayer)
            {
                // 돌진 중단
                isCharging = false;

                // 속도 0으로 설정
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                }

                if (showDebugLogs)
                    Debug.Log($"[BossEnemy] 돌진 중 장애물 충돌! ({collision.gameObject.name}) 돌진 중단!");
            }
        }


        // ====== Phase 2: 소환 ======

        private bool CanUseSummon()
        {
            if (Data == null) return false;
            if (Time.time - lastSummonTime < Data.bossSummonCooldown) return false;
            if (currentSummonCount >= maxSummonCount) return false;
            return true;
        }

        private void ExecuteSummon()
        {
            if (minionPrefab == null)
            {
                Debug.LogWarning("[BossEnemy] minionPrefab이 null입니다. Inspector에서 설정하세요.");
                return;
            }

            if (minionData == null)
            {
                Debug.LogWarning("[BossEnemy] minionData가 null입니다. Inspector에서 설정하세요.");
                return;
            }

            // 소환 위치 (보스 주변 랜덤)
            Vector3 summonPos = transform.position + new Vector3(
                Random.Range(-3f, 3f),
                0f,
                0f
            );

            // 소환
            GameObject minion = Instantiate(minionPrefab, summonPos, Quaternion.identity);

            if (minion != null)
            {
                // 소환된 적 사망 시 카운트 감소
                Enemy minionEnemy = minion.GetComponent<Enemy>();
                if (minionEnemy != null)
                {
                    // EnemyData 설정 (중요!)
                    minionEnemy.InitializeWithData(minionData);

                    currentSummonCount++;

                    minionEnemy.OnDeath += (enemy) =>
                    {
                        currentSummonCount--;
                    };

                    if (showDebugLogs)
                        Debug.Log($"[BossEnemy] 소환 완료! 위치: {summonPos}, 현재 소환 수: {currentSummonCount}");
                }
                else
                {
                    Debug.LogError("[BossEnemy] 소환된 프리팹에 Enemy 컴포넌트가 없습니다!");
                    Destroy(minion);
                }
            }

            lastSummonTime = Time.time;
        }


        // ====== Phase 3: 범위 공격 ======

        private bool CanUseAreaAttack()
        {
            if (playerTransform == null) return false;
            if (Data == null) return false;
            if (Time.time - lastAreaAttackTime < Data.bossAreaCooldown) return false;
            if (!IsPlayerInDetectionRange()) return false;
            return true;
        }

        private void ExecuteAreaAttack()
        {
            if (Data == null) return;

            // 범위 내 플레이어 확인
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, Data.bossAreaRadius);

            int hitCount = 0;

            foreach (var hit in hits)
            {
                // PlayerStats 확인
                var playerStats = hit.GetComponent<GASPT.Stats.PlayerStats>();

                if (playerStats != null)
                {
                    // 데미지 적용
                    int finalDamage = Mathf.RoundToInt(Data.bossAreaDamage * phaseController.GetAttackMultiplier());
                    playerStats.TakeDamage(finalDamage);

                    hitCount++;

                    if (showDebugLogs)
                        Debug.Log($"[BossEnemy] 범위 공격 적중! 데미지: {finalDamage}");
                }
            }

            if (hitCount == 0 && showDebugLogs)
            {
                Debug.Log($"[BossEnemy] 범위 공격 빗나감");
            }

            // TODO: 범위 공격 시각 효과 (VisualEffect 풀 사용)
            // DebugExtensions.DrawCircle(transform.position, Data.bossAreaRadius, Color.red, 1f);

            lastAreaAttackTime = Time.time;
        }


        // ====== Gizmos ======

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (!showGizmos || Data == null) return;

            // Phase 2 돌진 경로 시각화 (Phase 2 = 인덱스 1)
            if (phaseController != null && phaseController.CurrentPhaseIndex == 1)
            {
                if (playerTransform != null && col != null)
                {
                    CapsuleCollider2D capsule = col as CapsuleCollider2D;
                    if (capsule != null)
                    {
                        // Collider 크기 (스케일 적용)
                        Vector2 capsuleSize = capsule.size;
                        Vector2 scaledSize = new Vector2(
                            capsuleSize.x * Mathf.Abs(transform.localScale.x),
                            capsuleSize.y * Mathf.Abs(transform.localScale.y)
                        );

                        // 돌진 가능 거리
                        Vector2 horizontalDir = new Vector2(
                            playerTransform.position.x - transform.position.x,
                            0f
                        ).normalized;

                        Vector3 chargeEnd = transform.position + new Vector3(
                            horizontalDir.x * Data.bossChargeDistance,
                            0f,
                            0f
                        );

                        // CapsuleCast 영역 시각화 (초록색 캡슐)
                        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                        DrawCapsuleGizmo(transform.position, scaledSize);
                        DrawCapsuleGizmo(chargeEnd, scaledSize);

                        // 돌진 경로 선 (초록색)
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(transform.position, chargeEnd);

                        // 돌진 중일 때 (빨간색)
                        if (isCharging)
                        {
                            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                            DrawCapsuleGizmo(transform.position, scaledSize);
                            DrawCapsuleGizmo(chargeTargetPos, scaledSize);

                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(transform.position, chargeTargetPos);
                            Gizmos.DrawWireSphere(chargeTargetPos, 0.5f);
                        }
                    }
                }
            }

            // Phase 3 범위 공격 범위 (빨간색 원) (Phase 3 = 인덱스 2)
            if (phaseController != null && phaseController.CurrentPhaseIndex == 2)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, Data.bossAreaRadius);
            }
        }

        /// <summary>
        /// Capsule Gizmo 그리기 (디버깅용)
        /// </summary>
        private void DrawCapsuleGizmo(Vector3 position, Vector2 size)
        {
            float radius = size.x * 0.5f;
            float height = size.y;

            // 상단 반원
            Vector3 topCenter = position + Vector3.up * (height * 0.5f - radius);
            DrawHalfCircle(topCenter, radius, true);

            // 하단 반원
            Vector3 bottomCenter = position + Vector3.down * (height * 0.5f - radius);
            DrawHalfCircle(bottomCenter, radius, false);

            // 좌우 세로선
            Gizmos.DrawLine(
                topCenter + Vector3.left * radius,
                bottomCenter + Vector3.left * radius
            );
            Gizmos.DrawLine(
                topCenter + Vector3.right * radius,
                bottomCenter + Vector3.right * radius
            );
        }

        /// <summary>
        /// 반원 그리기 (디버깅용)
        /// </summary>
        private void DrawHalfCircle(Vector3 center, float radius, bool top)
        {
            int segments = 16;
            float angleStart = top ? 0f : 180f;
            float angleEnd = top ? 180f : 360f;

            Vector3 prevPoint = center + new Vector3(
                Mathf.Cos(angleStart * Mathf.Deg2Rad) * radius,
                Mathf.Sin(angleStart * Mathf.Deg2Rad) * radius,
                0f
            );

            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Lerp(angleStart, angleEnd, i / (float)segments);
                Vector3 newPoint = center + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                    0f
                );

                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }


        // ====== 정리 ======

        protected override void OnDisable()
        {
            base.OnDisable();

            // Phase Controller 이벤트 구독 해제
            if (phaseController != null)
            {
                phaseController.OnPhaseChanged -= OnPhaseChanged;
            }
        }
    }
}
