using UnityEngine;
using Skull.Data;
using GAS.Core;
using System.Threading;

namespace Skull.Core
{
    /// <summary>
    /// 모든 스컬의 기본 클래스
    /// 공통 기능들을 구현하고 각 스컬별로 오버라이드할 수 있는 가상 메서드 제공
    /// </summary>
    public abstract class BaseSkull : MonoBehaviour, ISkullController
    {
        [Header("스컬 설정")]
        [SerializeField] protected SkullData skullData;
        [SerializeField] protected bool enableDebugLogs = false;

        [Header("컴포넌트 참조")]
        [SerializeField] protected Transform playerTransform;
        [SerializeField] protected SpriteRenderer playerRenderer;
        [SerializeField] protected Animator playerAnimator;
        [SerializeField] protected AbilitySystem abilitySystem;

        // 상태 관리
        protected bool isActive = false;
        protected bool isInitialized = false;
        protected float lastAttackTime = 0f;
        protected float lastAbilityTime = 0f;

        // 쿨다운 관리
        protected float primaryCooldown = 0f;
        protected float secondaryCooldown = 0f;
        protected float ultimateCooldown = 0f;
        protected float throwCooldown = 0f;

        // ISkullController 구현
        public SkullType SkullType => skullData != null ? skullData.Type : SkullType.None;
        public SkullData SkullData => skullData;
        public bool IsActive => isActive;

        #region Unity 생명주기

        protected virtual void Awake()
        {
            InitializeComponents();
        }

        protected virtual void Start()
        {
            if (!isInitialized)
            {
                InitializeSkull();
            }
        }

        protected virtual void Update()
        {
            if (!isActive) return;

            UpdateCooldowns();
            OnUpdate();
        }

        protected virtual void FixedUpdate()
        {
            if (!isActive) return;

            OnFixedUpdate();
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 컴포넌트 참조 초기화
        /// </summary>
        protected virtual void InitializeComponents()
        {
            if (playerTransform == null)
                playerTransform = transform.parent ?? transform;

            if (playerRenderer == null)
                playerRenderer = GetComponentInParent<SpriteRenderer>();

            if (playerAnimator == null)
                playerAnimator = GetComponentInParent<Animator>();

            if (abilitySystem == null)
                abilitySystem = GetComponentInParent<AbilitySystem>();
        }

        /// <summary>
        /// 스컬 초기화
        /// </summary>
        protected virtual void InitializeSkull()
        {
            if (skullData == null)
            {
                LogWarning("스컬 데이터가 설정되지 않았습니다.");
                return;
            }

            if (!skullData.IsValid())
            {
                LogWarning($"스컬 데이터가 유효하지 않습니다: {skullData.name}");
                return;
            }

            InitializeAbilities();
            isInitialized = true;

            LogDebug($"스컬 초기화 완료: {skullData.SkullName}");
        }

        /// <summary>
        /// 어빌리티 시스템 초기화
        /// </summary>
        protected virtual void InitializeAbilities()
        {
            if (abilitySystem == null || skullData.SkullAbilities == null) return;

            foreach (var ability in skullData.SkullAbilities)
            {
                if (ability != null)
                {
                    abilitySystem.AddAbility(ability);
                }
            }
        }

        #endregion

        #region ISkullController 구현

        public virtual async Awaitable OnEquip(CancellationToken cancellationToken = default)
        {
            if (!isInitialized)
            {
                InitializeSkull();
            }

            LogDebug($"스컬 장착 시작: {skullData.SkullName}");

            // 비주얼 변경
            await UpdateVisuals(cancellationToken);

            // 능력치 적용
            ApplyStats();

            // 사운드 재생
            PlayEquipSound();

            LogDebug($"스컬 장착 완료: {skullData.SkullName}");
        }

        public virtual async Awaitable OnUnequip(CancellationToken cancellationToken = default)
        {
            LogDebug($"스컬 해제 시작: {skullData.SkullName}");

            // 진행 중인 어빌리티 취소
            CancelActiveAbilities();

            // 비주얼 리셋은 새로운 스컬에서 처리
            await Awaitable.NextFrameAsync(cancellationToken);

            LogDebug($"스컬 해제 완료: {skullData.SkullName}");
        }

        public virtual void OnActivate()
        {
            isActive = true;
            LogDebug($"스컬 활성화: {skullData.SkullName}");
        }

        public virtual void OnDeactivate()
        {
            isActive = false;
            LogDebug($"스컬 비활성화: {skullData.SkullName}");
        }

        public virtual async Awaitable PerformPrimaryAttack(CancellationToken cancellationToken = default)
        {
            if (!CanUsePrimary()) return;

            primaryCooldown = GetPrimaryCooldown();
            lastAttackTime = Time.time;

            LogDebug($"기본 공격 실행: {skullData.SkullName}");

            // 기본 공격 애니메이션
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Attack");
            }

            // 파생 클래스에서 구체적인 공격 로직 구현
            await ExecutePrimaryAttack(cancellationToken);
        }

        public virtual async Awaitable PerformSecondaryAttack(CancellationToken cancellationToken = default)
        {
            if (!CanUseSecondary()) return;

            secondaryCooldown = GetSecondaryCooldown();
            lastAbilityTime = Time.time;

            LogDebug($"보조 공격 실행: {skullData.SkullName}");

            await ExecuteSecondaryAttack(cancellationToken);
        }

        public virtual async Awaitable PerformUltimate(CancellationToken cancellationToken = default)
        {
            if (!CanUseUltimate()) return;

            ultimateCooldown = GetUltimateCooldown();
            lastAbilityTime = Time.time;

            LogDebug($"궁극기 실행: {skullData.SkullName}");

            await ExecuteUltimate(cancellationToken);
        }

        public virtual async Awaitable PerformSkullThrow(Vector2 direction, CancellationToken cancellationToken = default)
        {
            if (!CanUseSkullThrow()) return;

            throwCooldown = GetSkullThrowCooldown();

            LogDebug($"스컬 던지기 실행: {skullData.SkullName}, 방향: {direction}");

            await ExecuteSkullThrow(direction, cancellationToken);
        }

        public virtual void OnUpdate()
        {
            // 파생 클래스에서 프레임별 로직 구현
        }

        public virtual void OnFixedUpdate()
        {
            // 파생 클래스에서 물리 프레임 로직 구현
        }

        public virtual SkullStatus GetStatus()
        {
            return new SkullStatus
            {
                isReady = isActive && isInitialized,
                cooldownRemaining = Mathf.Max(primaryCooldown, secondaryCooldown, ultimateCooldown, throwCooldown),
                manaRemaining = abilitySystem?.GetResource("Mana") ?? 0f,
                canUseAbilities = CanUseAbilities()
            };
        }

        #endregion

        #region 추상 메서드 (파생 클래스에서 구현 필수)

        /// <summary>
        /// 기본 공격 구체적 실행 로직
        /// </summary>
        protected abstract Awaitable ExecutePrimaryAttack(CancellationToken cancellationToken);

        /// <summary>
        /// 보조 공격 구체적 실행 로직
        /// </summary>
        protected abstract Awaitable ExecuteSecondaryAttack(CancellationToken cancellationToken);

        /// <summary>
        /// 궁극기 구체적 실행 로직
        /// </summary>
        protected abstract Awaitable ExecuteUltimate(CancellationToken cancellationToken);

        /// <summary>
        /// 스컬 던지기 구체적 실행 로직
        /// </summary>
        protected abstract Awaitable ExecuteSkullThrow(Vector2 direction, CancellationToken cancellationToken);

        #endregion

        #region 가상 메서드 (파생 클래스에서 선택적 오버라이드)

        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        protected virtual async Awaitable UpdateVisuals(CancellationToken cancellationToken)
        {
            if (playerRenderer != null && skullData.PlayerSprite != null)
            {
                playerRenderer.sprite = skullData.PlayerSprite;
            }

            if (playerRenderer != null && skullData.PlayerMaterial != null)
            {
                playerRenderer.material = skullData.PlayerMaterial;
            }

            if (playerAnimator != null && skullData.AnimatorController != null)
            {
                playerAnimator.runtimeAnimatorController = skullData.AnimatorController;
            }

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        /// <summary>
        /// 능력치 적용
        /// </summary>
        protected virtual void ApplyStats()
        {
            if (skullData?.BaseStats == null) return;

            // GAS 시스템을 통한 능력치 적용
            if (abilitySystem != null)
            {
                abilitySystem.SetMaxResource("Health", skullData.BaseStats.MaxHealth);
                abilitySystem.SetMaxResource("Mana", skullData.BaseStats.MaxMana);
            }

            LogDebug($"능력치 적용 완료: HP={skullData.BaseStats.MaxHealth}, MP={skullData.BaseStats.MaxMana}");
        }

        /// <summary>
        /// 장착 사운드 재생
        /// </summary>
        protected virtual void PlayEquipSound()
        {
            if (skullData?.EquipSound != null)
            {
                // AudioManager를 통한 사운드 재생 (향후 구현)
                LogDebug("장착 사운드 재생");
            }
        }

        /// <summary>
        /// 진행 중인 어빌리티 취소
        /// </summary>
        protected virtual void CancelActiveAbilities()
        {
            // 진행 중인 어빌리티들 취소 로직
            LogDebug("진행 중인 어빌리티 취소");
        }

        #endregion

        #region 쿨다운 및 조건 체크

        protected virtual void UpdateCooldowns()
        {
            primaryCooldown = Mathf.Max(0f, primaryCooldown - Time.deltaTime);
            secondaryCooldown = Mathf.Max(0f, secondaryCooldown - Time.deltaTime);
            ultimateCooldown = Mathf.Max(0f, ultimateCooldown - Time.deltaTime);
            throwCooldown = Mathf.Max(0f, throwCooldown - Time.deltaTime);
        }

        protected virtual bool CanUsePrimary()
        {
            return isActive && primaryCooldown <= 0f;
        }

        protected virtual bool CanUseSecondary()
        {
            return isActive && secondaryCooldown <= 0f && HasEnoughMana(GetSecondaryManaCost());
        }

        protected virtual bool CanUseUltimate()
        {
            return isActive && ultimateCooldown <= 0f && HasEnoughMana(GetUltimateManaCost());
        }

        protected virtual bool CanUseSkullThrow()
        {
            return isActive && throwCooldown <= 0f;
        }

        protected virtual bool CanUseAbilities()
        {
            return isActive && isInitialized;
        }

        protected virtual bool HasEnoughMana(float requiredMana)
        {
            if (abilitySystem == null) return true;
            return abilitySystem.GetResource("Mana") >= requiredMana;
        }

        #endregion

        #region 쿨다운 및 마나 비용 (파생 클래스에서 오버라이드 가능)

        protected virtual float GetPrimaryCooldown() => 1f / (skullData?.BaseStats?.AttackSpeed ?? 1f);
        protected virtual float GetSecondaryCooldown() => 3f;
        protected virtual float GetUltimateCooldown() => 10f;
        protected virtual float GetSkullThrowCooldown() => 0.5f;

        protected virtual float GetSecondaryManaCost() => 20f;
        protected virtual float GetUltimateManaCost() => 50f;

        #endregion

        #region 디버그 및 로깅

        protected void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[{GetType().Name}] {message}");
            }
        }

        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{GetType().Name}] {message}");
        }

        protected void LogError(string message)
        {
            Debug.LogError($"[{GetType().Name}] {message}");
        }

        #endregion

        #region 에디터 전용

        [ContextMenu("스컬 정보 출력")]
        private void PrintSkullInfo()
        {
            if (skullData != null)
            {
                Debug.Log($"스컬 이름: {skullData.SkullName}\n" +
                         $"타입: {skullData.Type}\n" +
                         $"설명: {skullData.Description}\n" +
                         $"활성화: {isActive}\n" +
                         $"초기화: {isInitialized}");
            }
        }

        #endregion
    }
}
