using System;
using UnityEngine;
using GASPT.Data;
using GASPT.Core.Enums;
using GASPT.Combat;
using GASPT.Economy;
using GASPT.Level;
using GASPT.Meta;
using GASPT.UI;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 보스 기본 클래스 (추상)
    /// BossData 기반으로 페이즈 전환, 패턴 실행, 보상 처리를 담당
    /// 개별 보스는 이 클래스를 상속하여 구현
    /// </summary>
    public abstract class BaseBoss : MonoBehaviour
    {
        // ====== BossData 참조 ======

        [Header("보스 데이터")]
        [SerializeField]
        [Tooltip("보스 데이터 ScriptableObject")]
        protected BossData bossData;


        // ====== 컴포넌트 참조 ======

        [Header("컴포넌트")]
        [SerializeField]
        protected Rigidbody2D rb;

        [SerializeField]
        protected Collider2D col;

        [SerializeField]
        protected SpriteRenderer spriteRenderer;


        // ====== 상태 ======

        protected int currentHp;
        protected bool isDead = false;
        protected bool isInvulnerable = false;
        protected bool isFacingRight = true;

        protected Transform playerTransform;


        // ====== 페이즈 ======

        protected BossPhaseControllerV2 phaseController;
        protected int currentPhaseIndex = 0;


        // ====== 패턴 ======

        protected PatternSelector patternSelector;
        protected BossPattern currentPattern;
        protected bool isExecutingPattern = false;


        // ====== 전투 타이머 ======

        protected float combatStartTime;
        protected bool isInCombat = false;
        protected bool wasHit = false;


        // ====== 디버그 ======

        [Header("디버그")]
        [SerializeField]
        protected bool showDebugLogs = false;

        [SerializeField]
        protected bool showGizmos = true;


        // ====== 이벤트 ======

        /// <summary>
        /// HP 변경 이벤트 (현재HP, 최대HP)
        /// </summary>
        public event Action<int, int> OnHpChanged;

        /// <summary>
        /// 페이즈 변경 이벤트 (현재페이즈, 총페이즈)
        /// </summary>
        public event Action<int, int> OnPhaseChanged;

        /// <summary>
        /// 보스 처치 이벤트
        /// </summary>
        public event Action<BaseBoss> OnBossDefeated;

        /// <summary>
        /// 무적 상태 변경 이벤트
        /// </summary>
        public event Action<bool> OnInvulnerableChanged;


        // ====== 프로퍼티 ======

        public BossData Data => bossData;
        public int CurrentHp => currentHp;
        public int MaxHp => bossData != null ? bossData.maxHealth : 0;
        public float HealthRatio => MaxHp > 0 ? (float)currentHp / MaxHp : 0f;
        public bool IsDead => isDead;
        public bool IsInvulnerable => isInvulnerable;
        public int CurrentPhase => currentPhaseIndex + 1;
        public int TotalPhases => bossData != null ? bossData.PhaseCount : 1;
        public bool IsInCombat => isInCombat;


        // ====== Unity 생명주기 ======

        protected virtual void Awake()
        {
            // 컴포넌트 캐싱
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (col == null) col = GetComponent<Collider2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void Update()
        {
            if (isDead || !isInCombat) return;

            // 시간 제한 체크
            CheckTimeLimit();

            // 패턴 실행 중이 아니면 다음 패턴 선택
            if (!isExecutingPattern && !isInvulnerable)
            {
                TryExecuteNextPattern();
            }
        }

        protected virtual void FixedUpdate()
        {
            if (isDead || isInvulnerable) return;

            // 플레이어 방향으로 회전
            UpdateFacing();
        }


        // ====== 초기화 ======

        /// <summary>
        /// 보스 초기화
        /// </summary>
        protected virtual void Initialize()
        {
            if (bossData == null)
            {
                Debug.LogError($"[BaseBoss] {gameObject.name}: BossData가 null입니다.");
                return;
            }

            // HP 초기화
            currentHp = bossData.maxHealth;

            // 플레이어 찾기
            FindPlayer();

            // 페이즈 컨트롤러 초기화
            InitializePhaseController();

            // 패턴 셀렉터 초기화
            InitializePatternSelector();

            if (showDebugLogs)
                Debug.Log($"[BaseBoss] {bossData.bossName} 초기화 완료: HP={currentHp}/{MaxHp}");
        }

        /// <summary>
        /// BossData로 외부 초기화
        /// </summary>
        public void InitializeWithData(BossData data)
        {
            if (data == null)
            {
                Debug.LogError($"[BaseBoss] InitializeWithData: data가 null입니다.");
                return;
            }

            bossData = data;
            Initialize();
        }

        /// <summary>
        /// 플레이어 찾기
        /// </summary>
        protected void FindPlayer()
        {
            var player = FindAnyObjectByType<GASPT.Stats.PlayerStats>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("[BaseBoss] 플레이어를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// 페이즈 컨트롤러 초기화
        /// </summary>
        protected virtual void InitializePhaseController()
        {
            phaseController = new BossPhaseControllerV2(bossData);
            phaseController.OnPhaseChanged += HandlePhaseChanged;
        }

        /// <summary>
        /// 패턴 셀렉터 초기화 (하위 클래스에서 구현)
        /// </summary>
        protected abstract void InitializePatternSelector();


        // ====== 전투 시작/종료 ======

        /// <summary>
        /// 전투 시작
        /// </summary>
        public virtual void StartCombat()
        {
            if (isInCombat) return;

            isInCombat = true;
            combatStartTime = Time.time;
            wasHit = false;

            OnHpChanged?.Invoke(currentHp, MaxHp);
            OnPhaseChanged?.Invoke(CurrentPhase, TotalPhases);

            if (showDebugLogs)
                Debug.Log($"[BaseBoss] {bossData.bossName} 전투 시작!");
        }

        /// <summary>
        /// 전투 종료 (타임아웃 또는 플레이어 사망)
        /// </summary>
        public virtual void EndCombat()
        {
            isInCombat = false;

            if (showDebugLogs)
                Debug.Log($"[BaseBoss] {bossData.bossName} 전투 종료");
        }


        // ====== 데미지 처리 ======

        /// <summary>
        /// 데미지 받기
        /// </summary>
        public virtual void TakeDamage(int damage)
        {
            if (isDead)
            {
                Debug.LogWarning($"[BaseBoss] {bossData?.bossName}: 이미 사망한 보스입니다.");
                return;
            }

            if (isInvulnerable)
            {
                if (showDebugLogs)
                    Debug.Log($"[BaseBoss] {bossData?.bossName}: 무적 상태입니다.");
                return;
            }

            if (damage <= 0) return;

            // 방어력 적용
            int finalDamage = Mathf.Max(1, damage - bossData.defense);

            // HP 감소
            int previousHp = currentHp;
            currentHp -= finalDamage;
            currentHp = Mathf.Max(0, currentHp);

            wasHit = true;

            if (showDebugLogs)
                Debug.Log($"[BaseBoss] {bossData.bossName}: {finalDamage} 데미지 받음 ({previousHp} → {currentHp})");

            // 데미지 넘버 표시
            if (DamageNumberPool.Instance != null)
            {
                Vector3 damagePos = transform.position + Vector3.up * 1.5f;
                DamageNumberPool.Instance.ShowDamage(finalDamage, damagePos, false);
            }

            // 이벤트 발생
            OnHpChanged?.Invoke(currentHp, MaxHp);

            // 페이즈 체크
            phaseController?.UpdatePhase(HealthRatio);

            // 사망 체크
            if (currentHp <= 0)
            {
                Die();
            }
        }


        // ====== 사망 처리 ======

        /// <summary>
        /// 사망 처리
        /// </summary>
        protected virtual void Die()
        {
            if (isDead) return;

            isDead = true;
            isInCombat = false;

            float clearTime = Time.time - combatStartTime;
            bool noHit = !wasHit;

            if (showDebugLogs)
                Debug.Log($"[BaseBoss] {bossData.bossName} 처치! (클리어 시간: {clearTime:F1}초, 노히트: {noHit})");

            // 보상 지급
            ProcessRewards(noHit, clearTime);

            // 사망 이벤트
            OnBossDefeated?.Invoke(this);

            // 사망 연출 후 파괴
            PlayDeathSequence();
        }

        /// <summary>
        /// 보상 처리
        /// </summary>
        protected virtual void ProcessRewards(bool noHit, float clearTime)
        {
            if (bossData == null) return;

            // 기본 보상 계산
            int gold = bossData.goldReward;
            int exp = bossData.expReward;

            // 노히트 보너스 (+50% 골드)
            if (noHit)
            {
                gold = Mathf.RoundToInt(gold * 1.5f);
                if (showDebugLogs)
                    Debug.Log("[BaseBoss] 노히트 보너스 적용! 골드 +50%");
            }

            // 빠른 클리어 보너스 (+30% 경험치)
            if (bossData.timeLimit > 0 && clearTime < bossData.timeLimit * 0.5f)
            {
                exp = Mathf.RoundToInt(exp * 1.3f);
                if (showDebugLogs)
                    Debug.Log("[BaseBoss] 빠른 클리어 보너스 적용! 경험치 +30%");
            }

            // 골드 지급
            if (CurrencySystem.Instance != null)
            {
                CurrencySystem.Instance.AddGold(gold);
            }

            // 경험치 지급
            if (PlayerLevel.Instance != null)
            {
                PlayerLevel.Instance.AddExp(exp);
            }

            // 메타 재화 지급
            if (MetaProgressionManager.HasInstance && MetaProgressionManager.Instance.IsInRun)
            {
                var meta = MetaProgressionManager.Instance;

                if (bossData.boneDrop > 0)
                    meta.Currency.AddTempBone(bossData.boneDrop);

                if (bossData.soulDrop > 0)
                    meta.Currency.AddTempSoul(bossData.soulDrop);
            }

            if (showDebugLogs)
                Debug.Log($"[BaseBoss] 보상 지급: 골드={gold}, EXP={exp}, Bone={bossData.boneDrop}, Soul={bossData.soulDrop}");
        }

        /// <summary>
        /// 사망 연출 (하위 클래스에서 재정의 가능)
        /// </summary>
        protected virtual async void PlayDeathSequence()
        {
            // 간단한 사망 연출
            await Awaitable.WaitForSecondsAsync(1f);
            Destroy(gameObject);
        }


        // ====== 페이즈 전환 ======

        /// <summary>
        /// 페이즈 전환 핸들러
        /// </summary>
        protected virtual void HandlePhaseChanged(int newPhaseIndex)
        {
            if (newPhaseIndex == currentPhaseIndex) return;

            currentPhaseIndex = newPhaseIndex;

            if (showDebugLogs)
                Debug.Log($"[BaseBoss] {bossData.bossName} 페이즈 전환: Phase {CurrentPhase}/{TotalPhases}");

            // 페이즈 전환 연출
            PlayPhaseTransition(newPhaseIndex);

            // 이벤트 발생
            OnPhaseChanged?.Invoke(CurrentPhase, TotalPhases);
        }

        /// <summary>
        /// 페이즈 전환 연출
        /// </summary>
        protected virtual async void PlayPhaseTransition(int newPhaseIndex)
        {
            if (bossData.phases == null || newPhaseIndex >= bossData.phases.Length)
                return;

            var phaseData = bossData.phases[newPhaseIndex];

            // 무적 상태 시작
            SetInvulnerable(true);

            // 현재 패턴 취소
            CancelCurrentPattern();

            // 무적 시간 대기
            await Awaitable.WaitForSecondsAsync(phaseData.invulnerabilityDuration);

            // 무적 상태 해제
            SetInvulnerable(false);
        }

        /// <summary>
        /// 무적 상태 설정
        /// </summary>
        protected void SetInvulnerable(bool invulnerable)
        {
            isInvulnerable = invulnerable;
            OnInvulnerableChanged?.Invoke(invulnerable);

            if (showDebugLogs)
                Debug.Log($"[BaseBoss] {bossData?.bossName} 무적: {invulnerable}");
        }


        // ====== 패턴 실행 ======

        /// <summary>
        /// 다음 패턴 실행 시도
        /// </summary>
        protected virtual void TryExecuteNextPattern()
        {
            if (patternSelector == null) return;
            if (playerTransform == null) return;

            // 플레이어가 감지 범위 내에 있는지 확인
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance > bossData.detectionRange) return;

            // 사용 가능한 패턴 선택
            var pattern = patternSelector.SelectPattern(currentPhaseIndex);
            if (pattern != null)
            {
                ExecutePattern(pattern);
            }
        }

        /// <summary>
        /// 패턴 실행
        /// </summary>
        protected virtual async void ExecutePattern(BossPattern pattern)
        {
            if (pattern == null || isExecutingPattern) return;

            isExecutingPattern = true;
            currentPattern = pattern;

            try
            {
                await pattern.Execute(this, playerTransform);
            }
            catch (Exception e)
            {
                Debug.LogError($"[BaseBoss] 패턴 실행 중 오류: {e.Message}");
            }
            finally
            {
                isExecutingPattern = false;
                currentPattern = null;
            }
        }

        /// <summary>
        /// 현재 패턴 취소
        /// </summary>
        protected virtual void CancelCurrentPattern()
        {
            if (currentPattern != null)
            {
                currentPattern.Cancel();
                isExecutingPattern = false;
                currentPattern = null;
            }
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 플레이어 방향으로 스프라이트 회전
        /// </summary>
        protected virtual void UpdateFacing()
        {
            if (playerTransform == null) return;

            float direction = playerTransform.position.x - transform.position.x;

            if (direction > 0.1f && !isFacingRight)
            {
                Flip();
            }
            else if (direction < -0.1f && isFacingRight)
            {
                Flip();
            }
        }

        /// <summary>
        /// 스프라이트 뒤집기
        /// </summary>
        protected virtual void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        /// <summary>
        /// 시간 제한 체크
        /// </summary>
        protected virtual void CheckTimeLimit()
        {
            if (bossData.timeLimit <= 0) return;

            float elapsed = Time.time - combatStartTime;
            if (elapsed >= bossData.timeLimit)
            {
                if (showDebugLogs)
                    Debug.Log($"[BaseBoss] {bossData.bossName} 시간 초과!");

                // 시간 초과 처리 (플레이어 패배)
                EndCombat();
            }
        }

        /// <summary>
        /// 현재 페이즈 데이터 반환
        /// </summary>
        protected BossPhaseData GetCurrentPhaseData()
        {
            if (bossData?.phases == null || currentPhaseIndex >= bossData.phases.Length)
                return default;

            return bossData.phases[currentPhaseIndex];
        }


        // ====== 기즈모 ======

        protected virtual void OnDrawGizmosSelected()
        {
            if (!showGizmos || bossData == null) return;

            // 감지 범위
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, bossData.detectionRange);

            // 공격 범위
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, bossData.attackRange);
        }
    }
}
