using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Projectiles;
using GASPT.UI;

namespace GASPT.Gameplay.Enemy
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


        // ====== Phase 2 패턴: 소환 ======

        [Header("Phase 2: 소환")]
        [SerializeField] [Tooltip("최대 소환 수")]
        private int maxSummonCount = 3;

        [SerializeField] [Tooltip("소환할 적 타입 (BasicMeleeEnemy)")]
        private GameObject minionPrefab;

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

            // Phase별 패턴 실행
            ExecutePhasePatterns();

            // 돌진 중이면 돌진 처리
            if (isCharging)
            {
                UpdateCharge();
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// Phase Controller 초기화
        /// </summary>
        private void InitializePhaseController()
        {
            phaseController = new BossPhaseController();

            // HP 변경 시 Phase 업데이트
            OnHpChanged += (currentHp, maxHp) =>
            {
                phaseController.UpdatePhase(currentHp, maxHp);
            };

            // Phase 전환 이벤트 구독
            phaseController.OnPhaseChanged += OnPhaseChanged;

            Debug.Log($"[BossEnemy] Phase Controller 초기화 완료");
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
        private void OnPhaseChanged(BossPhase newPhase)
        {
            Debug.Log($"[BossEnemy] {Data?.enemyName} Phase 전환: {newPhase}");

            // Phase별 초기화
            switch (newPhase)
            {
                case BossPhase.Phase1:
                    // Phase 1 진입
                    break;

                case BossPhase.Phase2:
                    // Phase 2 진입 - 광폭화 이펙트
                    Debug.Log($"[BossEnemy] Phase 2 진입! 공격 속도 증가!");
                    break;

                case BossPhase.Phase3:
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

            BossPhase currentPhase = phaseController.CurrentPhase;

            switch (currentPhase)
            {
                case BossPhase.Phase1:
                    // Phase 1: 근접 공격 + 원거리 공격
                    if (CanUseRangedAttack())
                    {
                        ExecuteRangedAttack();
                    }
                    break;

                case BossPhase.Phase2:
                    // Phase 2: 근접 + 원거리 + 돌진 + 소환
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

                case BossPhase.Phase3:
                    // Phase 3: 근접 + 원거리 + 돌진 + 범위 공격
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
            if (playerTransform == null || Data == null) return;

            // 돌진 시작 위치 및 목표 위치 설정
            chargeStartPos = transform.position;
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            chargeTargetPos = chargeStartPos + (Vector3)(direction * Data.bossChargeDistance);

            isCharging = true;
            lastChargeAttackTime = Time.time;

            if (showDebugLogs)
                Debug.Log($"[BossEnemy] 돌진 공격 시작! {chargeStartPos} → {chargeTargetPos}");
        }

        private void UpdateCharge()
        {
            if (Data == null) return;

            // 목표 위치까지 이동
            transform.position = Vector3.MoveTowards(
                transform.position,
                chargeTargetPos,
                Data.bossChargeSpeed * Time.deltaTime
            );

            // 도착 확인
            if (Vector3.Distance(transform.position, chargeTargetPos) < 0.1f)
            {
                isCharging = false;

                if (showDebugLogs)
                    Debug.Log($"[BossEnemy] 돌진 공격 완료!");
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
                currentSummonCount++;

                // 소환된 적 사망 시 카운트 감소
                Enemy minionEnemy = minion.GetComponent<Enemy>();
                if (minionEnemy != null)
                {
                    minionEnemy.OnDeath += (enemy) =>
                    {
                        currentSummonCount--;
                    };
                }

                if (showDebugLogs)
                    Debug.Log($"[BossEnemy] 소환 완료! 위치: {summonPos}, 현재 소환 수: {currentSummonCount}");
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

            // Phase 3 범위 공격 범위 (빨간색 원)
            if (phaseController != null && phaseController.CurrentPhase == BossPhase.Phase3)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, Data.bossAreaRadius);
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
