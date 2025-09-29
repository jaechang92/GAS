using System;
using UnityEngine;
using System.Threading;

namespace GAS.Core
{
    /// <summary>
    /// 범용 어빌리티 클래스
    /// 다양한 게임 장르에서 사용할 수 있도록 설계
    /// </summary>
    public class Ability : IAbility, IDisposable
    {
        // 어빌리티 데이터
        private IAbilityData data;

        // 쿨다운 관리
        private AbilityCooldown cooldown;

        // 현재 상태
        private AbilityState currentState;

        // 시스템 참조
        protected GameObject owner;
        private IAbilitySystem abilitySystem;
        private IGameplayContext gameplayContext;

        // 취소 토큰
        private CancellationTokenSource cancellationTokenSource;

        // 이벤트
        public event Action<IAbility> OnAbilityStarted;
        public event Action<IAbility> OnAbilityCompleted;
        public event Action<IAbility> OnAbilityCancelled;
        public event Action<IAbility> OnCooldownStarted;
        public event Action<IAbility> OnCooldownCompleted;

        // 프로퍼티
        public string Id => data?.AbilityId;
        public string Name => data?.AbilityName;
        public string Description => data?.Description;
        public IAbilityData Data => data;
        public AbilityState State => currentState;
        public bool IsOnCooldown => cooldown?.IsOnCooldown ?? false;
        public float CooldownRemaining => cooldown?.RemainingTime ?? 0f;
        public float CooldownProgress => cooldown?.Progress ?? 1f;

        /// <summary>
        /// 어빌리티 초기화
        /// </summary>
        public void Initialize(IAbilityData abilityData, GameObject abilityOwner,
            IAbilitySystem system = null, IGameplayContext context = null)
        {
            data = abilityData;
            owner = abilityOwner;
            abilitySystem = system;
            gameplayContext = context;

            // 쿨다운 초기화
            cooldown = new AbilityCooldown();
            cooldown.Initialize(data.CooldownDuration);

            // 쿨다운 이벤트 연결
            cooldown.OnCooldownStarted += () => OnCooldownStarted?.Invoke(this);
            cooldown.OnCooldownCompleted += () => {
                currentState = AbilityState.Ready;
                OnCooldownCompleted?.Invoke(this);
            };

            // 초기 상태
            currentState = AbilityState.Ready;
            cancellationTokenSource = new CancellationTokenSource();

            Debug.Log($"Ability initialized: {data.AbilityName}");
        }

        /// <summary>
        /// 어빌리티 사용 가능 여부 확인
        /// </summary>
        public virtual bool CanExecute()
        {
            // 상태 확인
            if (currentState != AbilityState.Ready) return false;

            // 쿨다운 확인
            if (!cooldown.CanUse()) return false;

            // 게임플레이 컨텍스트 확인
            if (gameplayContext != null)
            {
                if (!gameplayContext.IsAlive || !gameplayContext.CanAct)
                    return false;

                // 차단 태그 확인
                foreach (var tag in data.BlockTags)
                {
                    if (gameplayContext.IsInState(tag))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 어빌리티 실행
        /// </summary>
        public async Awaitable<bool> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            if (!CanExecute())
            {
                Debug.Log($"Cannot execute ability: {data.AbilityName}");
                return false;
            }

            try
            {
                // 상태 변경
                currentState = AbilityState.Casting;
                OnAbilityStarted?.Invoke(this);

                Debug.Log($"Executing ability: {data.AbilityName}");

                // 취소 태그 적용
                ApplyAbilityTags();

                // 캐스트 시간 대기
                if (data.CastTime > 0)
                {
                    await WaitForSeconds(data.CastTime, cancellationToken);
                }

                // 실제 어빌리티 효과 실행
                currentState = AbilityState.Active;
                await ExecuteAbilityEffect(cancellationToken);

                // 지속 시간 대기
                if (data.Duration > 0)
                {
                    await WaitForSeconds(data.Duration, cancellationToken);
                }

                // 쿨다운 시작
                StartCooldown();

                // 완료 이벤트
                OnAbilityCompleted?.Invoke(this);

                Debug.Log($"Ability completed: {data.AbilityName}");
                return true;
            }
            catch (OperationCanceledException)
            {
                Cancel();
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error executing ability {data.AbilityName}: {e.Message}");
                currentState = AbilityState.Ready;
                return false;
            }
            finally
            {
                // 태그 정리
                RemoveAbilityTags();
            }
        }

        /// <summary>
        /// 어빌리티 효과 실행 (오버라이드 가능)
        /// </summary>
        protected virtual async Awaitable ExecuteAbilityEffect(CancellationToken cancellationToken)
        {
            // 어빌리티 타입별 기본 실행
            switch (data.AbilityType)
            {
                case AbilityType.Active:
                    await ExecuteActiveAbility(cancellationToken);
                    break;

                case AbilityType.Instant:
                    ExecuteInstantAbility();
                    break;

                case AbilityType.Channeled:
                    await ExecuteChanneledAbility(cancellationToken);
                    break;

                case AbilityType.Toggle:
                    ExecuteToggleAbility();
                    break;

                case AbilityType.Passive:
                    ExecutePassiveAbility();
                    break;

                default:
                    await Awaitable.WaitForSecondsAsync(0.1f);
                    break;
            }
        }

        /// <summary>
        /// 액티브 어빌리티 실행
        /// </summary>
        protected virtual async Awaitable ExecuteActiveAbility(CancellationToken cancellationToken)
        {
            // 애니메이션 트리거
            if (data is AbilityData concreteData && !string.IsNullOrEmpty(concreteData.AnimationTrigger))
            {
                var animator = owner.GetComponent<Animator>();
                animator?.SetTrigger(concreteData.AnimationTrigger);
            }

            // 사운드 재생
            if (data is AbilityData concreteData2 && concreteData2.SoundEffect != null)
            {
                AudioSource.PlayClipAtPoint(concreteData2.SoundEffect, owner.transform.position);
            }

            // 이펙트 생성
            if (data is AbilityData concreteData3 && concreteData3.EffectPrefab != null)
            {
                var effect = GameObject.Instantiate(
                    concreteData3.EffectPrefab,
                    owner.transform.position,
                    owner.transform.rotation
                );
                GameObject.Destroy(effect, 5f);
            }

            // 타겟팅 및 효과 적용
            await ApplyAbilityEffects(cancellationToken);
        }

        /// <summary>
        /// 즉시 어빌리티 실행
        /// </summary>
        protected virtual void ExecuteInstantAbility()
        {
            Debug.Log($"Instant ability: {data.AbilityName}");
            // 즉시 효과 적용
            ApplyInstantEffects();
        }

        /// <summary>
        /// 채널링 어빌리티 실행
        /// </summary>
        protected virtual async Awaitable ExecuteChanneledAbility(CancellationToken cancellationToken)
        {
            float channelTime = data.Duration;
            float elapsed = 0f;

            while (elapsed < channelTime)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 채널링 중 효과
                ApplyChannelingTick(elapsed / channelTime);

                await Awaitable.WaitForSecondsAsync(0.1f);
                elapsed += 0.1f;
            }
        }

        /// <summary>
        /// 토글 어빌리티 실행
        /// </summary>
        protected virtual void ExecuteToggleAbility()
        {
            bool isActive = gameplayContext?.IsInState($"Toggle_{Id}") ?? false;

            if (isActive)
            {
                gameplayContext?.SetState($"Toggle_{Id}", false);
                Debug.Log($"Toggle ability deactivated: {data.AbilityName}");
            }
            else
            {
                gameplayContext?.SetState($"Toggle_{Id}", true);
                Debug.Log($"Toggle ability activated: {data.AbilityName}");
            }
        }

        /// <summary>
        /// 패시브 어빌리티 실행
        /// </summary>
        protected virtual void ExecutePassiveAbility()
        {
            Debug.Log($"Passive ability applied: {data.AbilityName}");
            // 패시브 효과는 지속적으로 적용됨
        }

        /// <summary>
        /// 어빌리티 효과 적용
        /// </summary>
        protected virtual async Awaitable ApplyAbilityEffects(CancellationToken cancellationToken)
        {
            // 타겟 찾기
            var targets = FindTargets();

            // 각 타겟에게 효과 적용
            foreach (var target in targets)
            {
                ApplyEffectToTarget(target);
            }

            await Awaitable.WaitForSecondsAsync(0.05f);
        }

        /// <summary>
        /// 타겟 찾기
        /// </summary>
        protected virtual IAbilityTarget[] FindTargets()
        {
            // 기본 타겟팅 로직
            switch (data.TargetType)
            {
                case TargetType.Self:
                    var selfTarget = owner.GetComponent<IAbilityTarget>();
                    return selfTarget != null ? new[] { selfTarget } : new IAbilityTarget[0];

                case TargetType.SingleTarget:
                    var target = gameplayContext?.GetTarget()?.GetComponent<IAbilityTarget>();
                    return target != null ? new[] { target } : new IAbilityTarget[0];

                case TargetType.Area:
                    return FindTargetsInArea();

                default:
                    return new IAbilityTarget[0];
            }
        }

        /// <summary>
        /// 범위 내 타겟 찾기
        /// </summary>
        protected virtual IAbilityTarget[] FindTargetsInArea()
        {
            var colliders = Physics.OverlapSphere(owner.transform.position, data.Range);
            var targets = new System.Collections.Generic.List<IAbilityTarget>();

            foreach (var collider in colliders)
            {
                var target = collider.GetComponent<IAbilityTarget>();
                if (target != null && target.IsTargetable)
                {
                    targets.Add(target);
                }
            }

            return targets.ToArray();
        }

        /// <summary>
        /// 타겟에게 효과 적용
        /// </summary>
        protected virtual void ApplyEffectToTarget(IAbilityTarget target)
        {
            if (data is AbilityData concreteData)
            {
                // 데미지 적용
                if (concreteData.DamageValue > 0)
                {
                    target.TakeDamage(concreteData.DamageValue);
                }

                // 힐링 적용
                if (concreteData.HealValue > 0)
                {
                    target.Heal(concreteData.HealValue);
                }
            }
        }

        /// <summary>
        /// 즉시 효과 적용
        /// </summary>
        protected virtual void ApplyInstantEffects()
        {
            var targets = FindTargets();
            foreach (var target in targets)
            {
                ApplyEffectToTarget(target);
            }
        }

        /// <summary>
        /// 채널링 틱 처리
        /// </summary>
        protected virtual void ApplyChannelingTick(float progress)
        {
            Debug.Log($"Channeling {data.AbilityName}: {progress:P0}");
        }

        /// <summary>
        /// 어빌리티 태그 적용
        /// </summary>
        private void ApplyAbilityTags()
        {
            if (gameplayContext == null) return;

            foreach (var tag in data.AbilityTags)
            {
                gameplayContext.SetState(tag, true);
            }
        }

        /// <summary>
        /// 어빌리티 태그 제거
        /// </summary>
        private void RemoveAbilityTags()
        {
            if (gameplayContext == null) return;

            foreach (var tag in data.AbilityTags)
            {
                gameplayContext.SetState(tag, false);
            }
        }

        /// <summary>
        /// 시간 대기 (취소 가능)
        /// </summary>
        private async Awaitable WaitForSeconds(float seconds, CancellationToken cancellationToken)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Awaitable.WaitForSecondsAsync(0.05f);
                elapsed += 0.05f;
            }
        }

        /// <summary>
        /// 쿨다운 업데이트
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            cooldown?.Update(deltaTime);
        }

        /// <summary>
        /// 쿨다운 시작
        /// </summary>
        private void StartCooldown()
        {
            currentState = AbilityState.Cooldown;
            cooldown?.StartCooldown();
        }

        /// <summary>
        /// 쿨다운 리셋
        /// </summary>
        public void ResetCooldown()
        {
            currentState = AbilityState.Ready;
            cooldown?.Reset();
        }

        /// <summary>
        /// 어빌리티 취소
        /// </summary>
        public virtual void Cancel()
        {
            if (currentState == AbilityState.Casting || currentState == AbilityState.Active)
            {
                cancellationTokenSource?.Cancel();
                currentState = AbilityState.Ready;
                RemoveAbilityTags();
                OnAbilityCancelled?.Invoke(this);
                Debug.Log($"Ability cancelled: {data.AbilityName}");
            }
        }

        /// <summary>
        /// 리소스 정리
        /// </summary>
        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
    }
}
