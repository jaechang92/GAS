using GAS.AttributeSystem;
using GAS.Core;
using GAS.TagSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace GAS.EffectSystem
{
    /// <summary>
    /// 일정 시간 동안 지속되는 GameplayEffect
    /// 예: 버프, 디버프, 일시적 스탯 증가 등
    /// </summary>
    [CreateAssetMenu(fileName = "DurationEffect", menuName = "GAS/Effects/Duration Effect", order = 2)]
    public class DurationEffect : GameplayEffect
    {
        #region Additional Fields

        [Header("=== Duration Effect Settings ===")]
        [SerializeField] private bool scaleDurationWithStackCount = false;
        [SerializeField] private float durationPerStack = 0f;
        [SerializeField] private bool removeOnDeath = true;
        [SerializeField] private bool persistThroughDeath = false;

        [Header("=== Expiration Settings ===")]
        [SerializeField] private bool triggerExpirationEffects = true;
        [SerializeField] private List<InstantEffect> onExpirationEffects = new List<InstantEffect>();
        [SerializeField] private bool removeTagsOnExpiration = true;

        [Header("=== Modifier Scaling ===")]
        [SerializeField] private bool scaleModifiersOverTime = false;
        [SerializeField] private AnimationCurve modifierScalingCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Header("=== UI & Feedback ===")]
        [SerializeField] private bool showInUI = true;
        [SerializeField] private Color effectColor = Color.white;
        [SerializeField] private bool showTimer = true;
        [SerializeField] private bool showStacks = false;

        #endregion

        #region Runtime Data

        // 활성 인스턴스 추적 (런타임용, serialize 안함)
        private static Dictionary<GameObject, List<EffectInstance>> activeInstances =
            new Dictionary<GameObject, List<EffectInstance>>();

        #endregion

        #region Constructor

        public DurationEffect()
        {
            effectType = EffectType.Duration;
            durationPolicy = EffectDurationPolicy.HasDuration;
            duration = 5f; // 기본 5초
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Duration 효과 적용
        /// </summary>
        public override bool Apply(EffectContext context, GameObject target)
        {
            if (!CanApply(context, target))
            {
                Debug.LogWarning($"[DurationEffect] Cannot apply {effectName} to {target.name}");
                return false;
            }

            try
            {
                // 1. 기존 효과와의 스택 처리
                EffectInstance existingInstance = FindExistingInstance(target);

                if (existingInstance != null)
                {
                    return HandleStacking(existingInstance, context, target);
                }

                // 2. 새 인스턴스 생성
                var instance = CreateInstance(context, target);

                // 3. Modifier 적용
                ApplyModifiers(instance, context, target);

                // 4. 태그 부여
                GrantTags(target);

                // 5. 시각/청각 효과
                PlayApplicationEffects(instance, target);

                // 6. Duration 추적 시작
                StartDurationTracking(instance, target);

                // 7. 이벤트 발생
                TriggerApplicationEvents(instance, target);

                // 8. 인스턴스 저장
                StoreInstance(target, instance);

                Debug.Log($"[DurationEffect] Applied {effectName} to {target.name} for {duration}s");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DurationEffect] Failed to apply {effectName}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Duration 효과 제거
        /// </summary>
        public override bool Remove(EffectContext context, GameObject target)
        {
            var instance = FindExistingInstance(target);
            if (instance == null)
            {
                Debug.LogWarning($"[DurationEffect] No instance of {effectName} found on {target.name}");
                return false;
            }

            return RemoveInstance(instance, target, true);
        }

        /// <summary>
        /// DurationEffect는 주기적 실행을 지원하지 않음
        /// </summary>
        public override bool ExecutePeriodic(EffectContext context, GameObject target)
        {
            Debug.LogWarning($"[DurationEffect] {effectName} does not support periodic execution. Use PeriodicEffect instead.");
            return false;
        }

        #endregion

        #region Private Methods - Core

        /// <summary>
        /// Effect 인스턴스 생성
        /// </summary>
        private EffectInstance CreateInstance(EffectContext context, GameObject target)
        {
            var instance = new EffectInstance(this, context);

            // Duration 계산
            float finalDuration = CalculateDuration(context);
            instance.RemainingDuration = finalDuration;

            return instance;
        }

        /// <summary>
        /// Duration 계산
        /// </summary>
        protected float CalculateDuration(EffectContext context)
        {
            float baseDuration = duration;

            // Stack 기반 duration scaling
            if (scaleDurationWithStackCount && context.StackCount > 1)
            {
                baseDuration += durationPerStack * (context.StackCount - 1);
            }

            // Context magnitude scaling
            baseDuration *= context.Magnitude;

            return Mathf.Max(0.1f, baseDuration);
        }

        /// <summary>
        /// Modifier 적용
        /// </summary>
        protected void ApplyModifiers(EffectInstance instance, EffectContext context, GameObject target)
        {
            var attributeComponent = target.GetComponent<IAttributeComponent>();
            if (attributeComponent == null || modifiers.Count == 0) return;

            float magnitude = context.Magnitude;

            // 시간에 따른 scaling 적용
            if (scaleModifiersOverTime && modifierScalingCurve != null)
            {
                magnitude *= modifierScalingCurve.Evaluate(0f);
            }

            foreach (var config in modifiers)
            {
                if (config == null || config.timing != ModifierApplicationTiming.OnApplication)
                    continue;

                var modifier = new AttributeModifier(
                    config.operation,
                    config.value * magnitude,
                    config.priority
                );

                // Modifier 적용 및 ID 저장
                var modifierId = attributeComponent.AddModifier(
                    config.attributeType,
                    modifier,
                    instance
                );

                instance.AddModifierId(modifierId);

                Debug.Log($"[DurationEffect] Applied modifier to {config.attributeType}: {modifier.Operation} {modifier.Value}");
            }
        }

        /// <summary>
        /// Modifier 제거
        /// </summary>
        private void RemoveModifiers(EffectInstance instance, GameObject target)
        {
            var attributeComponent = target.GetComponent<IAttributeComponent>();
            if (attributeComponent == null) return;

            // 모든 modifier를 source(instance)로 제거
            attributeComponent.RemoveAllModifiersFromSource(instance);

            Debug.Log($"[DurationEffect] Removed all modifiers from {target.name}");
        }

        #endregion

        #region Private Methods - Duration Tracking

        /// <summary>
        /// Duration 추적 시작
        /// </summary>
        private async void StartDurationTracking(EffectInstance instance, GameObject target)
        {
            var cts = new CancellationTokenSource();
            instance.Context.SetData("CancellationToken", cts);

            try
            {
                float updateInterval = 0.1f; // 100ms마다 업데이트
                float elapsed = 0f;

                while (!instance.IsExpired && instance.RemainingDuration > 0)
                {
                    await Awaitable.WaitForSecondsAsync(updateInterval, cts.Token);

                    if (target == null || instance.IsExpired)
                        break;

                    elapsed += updateInterval;
                    instance.RemainingDuration -= updateInterval;

                    // 시간에 따른 Modifier scaling 업데이트
                    if (scaleModifiersOverTime)
                    {
                        UpdateModifierScaling(instance, target, instance.Progress);
                    }

                    // UI 업데이트 로그
                    if (showInUI && elapsed % 1f < updateInterval) // 1초마다 로그
                    {
                        Debug.Log($"[DurationEffect] {effectName} - Remaining: {instance.RemainingDuration:F1}s, Progress: {instance.Progress:P0}");
                    }

                    // 만료 체크
                    if (instance.RemainingDuration <= 0)
                    {
                        await HandleExpiration(instance, target);
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[DurationEffect] Duration tracking cancelled for {effectName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DurationEffect] Error in duration tracking: {e.Message}");
            }
            finally
            {
                cts?.Dispose();
            }
        }

        /// <summary>
        /// 만료 처리
        /// </summary>
        protected async Awaitable HandleExpiration(EffectInstance instance, GameObject target)
        {
            if (instance.IsExpired) return;

            instance.IsExpired = true;

            Debug.Log($"[DurationEffect] {effectName} expired on {target.name}");

            // 만료 효과 실행
            if (triggerExpirationEffects)
            {
                ExecuteExpirationEffects(instance, target);
            }

            // 인스턴스 제거
            RemoveInstance(instance, target, false);

            // 만료 이벤트
            GASEvents.TriggerEffectRemoved(target, effectName);

            Debug.Log($"[DurationEffect] Effect expired: {effectName} on {target.name}");

            await Awaitable.EndOfFrameAsync();
        }

        /// <summary>
        /// Modifier scaling 업데이트
        /// </summary>
        private void UpdateModifierScaling(EffectInstance instance, GameObject target, float progress)
        {
            if (!scaleModifiersOverTime || modifierScalingCurve == null) return;

            var attributeComponent = target.GetComponent<IAttributeComponent>();
            if (attributeComponent == null) return;

            float scaleFactor = modifierScalingCurve.Evaluate(progress);

            // 기존 modifier 제거 후 새로운 값으로 재적용
            RemoveModifiers(instance, target);

            var context = instance.Context;
            context.Magnitude = scaleFactor;
            ApplyModifiers(instance, context, target);
        }

        #endregion

        #region Private Methods - Stacking

        /// <summary>
        /// 스택 처리
        /// </summary>
        protected virtual bool HandleStacking(EffectInstance existingInstance, EffectContext context, GameObject target)
        {
            if (!CanStack(this, context))
                return false;

            switch (stackingPolicy)
            {
                case EffectStackingPolicy.Override:
                    // 기존 효과 제거 후 새로 적용
                    RemoveInstance(existingInstance, target, false);
                    return Apply(context, target);

                case EffectStackingPolicy.Stack:
                    // 스택 증가
                    existingInstance.AddStack(1);
                    context.StackCount = existingInstance.CurrentStack;

                    // Duration refresh
                    if (refreshDurationOnStack)
                    {
                        existingInstance.RefreshDuration();

                        // Duration tracking 재시작
                        CancelDurationTracking(existingInstance);
                        StartDurationTracking(existingInstance, target);
                    }

                    // 스택 이벤트
                    TriggerStackEvent(existingInstance, target);

                    Debug.Log($"[DurationEffect] Stacked {effectName} on {target.name}. Stack: {existingInstance.CurrentStack}");
                    return true;

                case EffectStackingPolicy.AggregateBySource:
                case EffectStackingPolicy.AggregateByTarget:
                    // 동일 source/target의 효과 집계
                    return HandleAggregation(existingInstance, context, target);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 집계 처리
        /// </summary>
        private bool HandleAggregation(EffectInstance existingInstance, EffectContext context, GameObject target)
        {
            // 구현 예정: source/target별 집계 로직
            Debug.LogWarning($"[DurationEffect] Aggregation not yet implemented for {effectName}");
            return false;
        }

        #endregion

        #region Private Methods - Instance Management

        /// <summary>
        /// 기존 인스턴스 찾기
        /// </summary>
        protected EffectInstance FindExistingInstance(GameObject target)
        {
            if (!activeInstances.TryGetValue(target, out var instances))
                return null;

            return instances.Find(i => i.SourceEffect == this && !i.IsExpired);
        }

        /// <summary>
        /// 인스턴스 저장
        /// </summary>
        protected void StoreInstance(GameObject target, EffectInstance instance)
        {
            if (!activeInstances.ContainsKey(target))
            {
                activeInstances[target] = new List<EffectInstance>();
            }

            activeInstances[target].Add(instance);
        }

        /// <summary>
        /// 인스턴스 제거
        /// </summary>
        private bool RemoveInstance(EffectInstance instance, GameObject target, bool cancelled)
        {
            if (instance == null || target == null) return false;

            // Duration tracking 취소
            CancelDurationTracking(instance);

            // Modifier 제거
            RemoveModifiers(instance, target);

            // 태그 제거
            if (removeTagsOnExpiration)
            {
                RemoveGrantedTags(target);
            }

            // 시각 효과 제거
            if (instance.VisualEffect != null)
            {
                UnityEngine.Object.Destroy(instance.VisualEffect);
            }

            // 인스턴스 목록에서 제거
            if (activeInstances.TryGetValue(target, out var instances))
            {
                instances.Remove(instance);
                if (instances.Count == 0)
                {
                    activeInstances.Remove(target);
                }
            }

            // 제거 이벤트
            TriggerRemovalEvents(instance, target, cancelled);

            return true;
        }

        /// <summary>
        /// Duration tracking 취소
        /// </summary>
        private void CancelDurationTracking(EffectInstance instance)
        {
            if (instance.Context.HasData("CancellationToken"))
            {
                var cts = instance.Context.GetData<CancellationTokenSource>("CancellationToken");
                cts?.Cancel();
                cts?.Dispose();
                instance.Context.RemoveData("CancellationToken");
            }
        }

        #endregion

        #region Private Methods - Effects & Events

        /// <summary>
        /// 적용 효과 재생
        /// </summary>
        protected void PlayApplicationEffects(EffectInstance instance, GameObject target)
        {
            // 시각 효과
            if (effectPrefab != null)
            {
                var vfx = CreateVisualEffect(target);
                instance.VisualEffect = vfx;
            }

            // 사운드
            if (applicationSound != null)
            {
                PlaySound(applicationSound, target.transform.position);
            }
        }

        /// <summary>
        /// 만료 효과 실행
        /// </summary>
        private void ExecuteExpirationEffects(EffectInstance instance, GameObject target)
        {
            foreach (var effect in onExpirationEffects)
            {
                if (effect != null)
                {
                    effect.Apply(instance.Context, target);
                }
            }

            // 만료 사운드
            if (removalSound != null)
            {
                PlaySound(removalSound, target.transform.position);
            }
        }

        /// <summary>
        /// 적용 이벤트 발생
        /// </summary>
        protected void TriggerApplicationEvents(EffectInstance instance, GameObject target)
        {
            GASEvents.TriggerEffectApplied(target, effectName);

            Debug.Log($"[DurationEffect] Applied: {effectName} - Duration: {instance.RemainingDuration:F1}s, Stack: {instance.CurrentStack}");
        }

        /// <summary>
        /// 제거 이벤트 발생
        /// </summary>
        private void TriggerRemovalEvents(EffectInstance instance, GameObject target, bool cancelled)
        {
            GASEvents.TriggerEffectRemoved(target, effectName);

            string cancelText = cancelled ? "cancelled" : "expired";
            Debug.Log($"[DurationEffect] Removed ({cancelText}): {effectName} - Elapsed: {instance.ElapsedTime:F1}s");
        }

        /// <summary>
        /// 스택 이벤트 발생
        /// </summary>
        private void TriggerStackEvent(EffectInstance instance, GameObject target)
        {
            GASEvents.TriggerEffectStacked(target, effectName, instance.CurrentStack);

            Debug.Log($"[DurationEffect] Stack changed: {effectName} - New stack count: {instance.CurrentStack}");
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// 특정 타겟의 모든 DurationEffect 제거
        /// </summary>
        public static void RemoveAllFromTarget(GameObject target)
        {
            if (!activeInstances.TryGetValue(target, out var instances))
                return;

            var toRemove = new List<EffectInstance>(instances);
            foreach (var instance in toRemove)
            {
                if (instance.SourceEffect is DurationEffect durationEffect)
                {
                    durationEffect.RemoveInstance(instance, target, true);
                }
            }
        }

        /// <summary>
        /// 특정 타겟의 DurationEffect 개수 확인
        /// </summary>
        public static int GetActiveCount(GameObject target)
        {
            if (!activeInstances.TryGetValue(target, out var instances))
                return 0;

            return instances.Count(i => !i.IsExpired);
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            // DurationEffect 강제 설정
            effectType = EffectType.Duration;
            durationPolicy = EffectDurationPolicy.HasDuration;

            // Duration 검증
            if (duration <= 0f) duration = 1f;

            // Stack duration 검증
            if (scaleDurationWithStackCount && durationPerStack < 0f)
            {
                durationPerStack = 0f;
            }
        }
#endif

        #endregion
    }
}

// 파일 위치: Assets/Scripts/GAS/EffectSystem/Types/DurationEffect.cs