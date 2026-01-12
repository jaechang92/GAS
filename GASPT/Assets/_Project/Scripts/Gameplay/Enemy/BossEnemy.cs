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
    ///
    /// partial class 분할:
    /// - BossEnemy.cs: 메인 (필드, 생명주기, 초기화, Phase 전환)
    /// - BossEnemy.Combat.cs: 전투 패턴 (원거리, 돌진, 소환, 범위)
    /// - BossEnemy.Gizmos.cs: 디버깅 시각화
    /// </summary>
    public partial class BossEnemy : PlatformerEnemy
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
