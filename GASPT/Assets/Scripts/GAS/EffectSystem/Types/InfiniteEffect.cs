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
    /// 수동으로 제거하기 전까지 영구적으로 지속되는 GameplayEffect
    /// 예: 패시브 스킬, 영구 버프, 저주, 장비 효과
    /// </summary>
    [CreateAssetMenu(fileName = "InfiniteEffect", menuName = "GAS/Effects/Infinite Effect", order = 4)]
    public class InfiniteEffect : GameplayEffect
    {
        #region Additional Fields

        [Header("=== Infinite Effect Settings ===")]
        [SerializeField] private bool canBeDispelled = true;
        [SerializeField] private bool persistThroughDeath = false;
        [SerializeField] private bool persistThroughSceneChange = false;
        [SerializeField] private int dispelResistance = 0; // 해제 저항 레벨

        [Header("=== Conditional Removal ===")]
        [SerializeField] private TagRequirement autoRemovalCondition;
        [SerializeField] private List<GameplayTag> removalTriggerTags = new List<GameplayTag>();
        [SerializeField] private float conditionalCheckInterval = 1f;

        [Header("=== Dynamic Modifiers ===")]
        [SerializeField] private bool useDynamicModifiers = false;
        [SerializeField] private AnimationCurve modifierGrowthCurve = AnimationCurve.Linear(0f, 1f, 60f, 2f);
        [SerializeField] private float growthUpdateInterval = 1f;
        [SerializeField] private float maxGrowthMultiplier = 5f;

        [Header("=== Aura Effect ===")]
        [SerializeField] private bool isAuraEffect = false;
        [SerializeField] private float auraRadius = 5f;
        [SerializeField] private LayerMask auraTargetLayers = -1;
        [SerializeField] private TagRequirement auraTargetRequirement;
        [SerializeField] private InfiniteEffect auraGrantedEffect;

        [Header("=== UI & Feedback ===")]
        [SerializeField] private bool showAsPassive = true;
        [SerializeField] private int displayPriority = 0;
        [SerializeField] private Sprite passiveIcon;

        #endregion

        #region properties

        public bool CanBeDispelled => canBeDispelled;

        #endregion

        #region Runtime Data

        // 활성 인스턴스 추적
        private static Dictionary<GameObject, List<EffectInstance>> activeInfiniteEffects =
            new Dictionary<GameObject, List<EffectInstance>>();

        #endregion

        #region Constructor

        public InfiniteEffect()
        {
            effectType = EffectType.Infinite;
            durationPolicy = EffectDurationPolicy.Infinite;
            duration = 0f;
            
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Infinite 효과 적용
        /// </summary>
        public override bool Apply(EffectContext context, GameObject target)
        {
            if (!CanApply(context, target))
            {
                Debug.LogWarning($"[InfiniteEffect] Cannot apply {effectName} to {target.name}");
                return false;
            }

            try
            {
                // 기존 인스턴스 확인
                EffectInstance existingInstance = FindExistingInstance(target);

                if (existingInstance != null)
                {
                    return HandleStacking(existingInstance, context, target);
                }

                // 새 인스턴스 생성
                var instance = CreateInfiniteInstance(context, target);

                // Modifier 적용
                ApplyModifiers(instance, context, target);

                // 태그 부여
                GrantTags(target);

                // 시각/청각 효과
                PlayApplicationEffects(instance, target);

                // 조건부 제거 추적 시작
                if (autoRemovalCondition != null || removalTriggerTags.Count > 0)
                {
                    StartConditionalTracking(instance, target);
                }

                // Dynamic modifier 추적 시작
                if (useDynamicModifiers)
                {
                    StartDynamicModifierTracking(instance, target);
                }

                // Aura 추적 시작
                if (isAuraEffect)
                {
                    StartAuraTracking(instance, target);
                }

                // 이벤트 발생
                TriggerApplicationEvents(instance, target);

                // 인스턴스 저장
                StoreInstance(target, instance);

                Debug.Log($"[InfiniteEffect] Applied {effectName} to {target.name} (Infinite duration)");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[InfiniteEffect] Failed to apply {effectName}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Infinite 효과 제거
        /// </summary>
        public override bool Remove(EffectContext context, GameObject target)
        {
            var instance = FindExistingInstance(target);
            if (instance == null)
            {
                Debug.LogWarning($"[InfiniteEffect] No instance of {effectName} found on {target.name}");
                return false;
            }

            // Dispel 가능 여부 확인
            if (!canBeDispelled)
            {
                Debug.LogWarning($"[InfiniteEffect] {effectName} cannot be dispelled");
                return false;
            }

            // Dispel 저항 체크
            if (dispelResistance > 0)
            {
                int dispelPower = context.GetData<int>("DispelPower", 0);
                if (dispelPower < dispelResistance)
                {
                    Debug.Log($"[InfiniteEffect] Dispel failed - Required power: {dispelResistance}, Actual: {dispelPower}");
                    return false;
                }
            }

            return RemoveInstance(instance, target, true);
        }

        /// <summary>
        /// InfiniteEffect는 주기적 실행을 지원하지 않음
        /// </summary>
        public override bool ExecutePeriodic(EffectContext context, GameObject target)
        {
            Debug.LogWarning($"[InfiniteEffect] {effectName} does not support periodic execution");
            return false;
        }

        #endregion

        #region Private Methods - Core

        /// <summary>
        /// Infinite 인스턴스 생성
        /// </summary>
        private EffectInstance CreateInfiniteInstance(EffectContext context, GameObject target)
        {
            var instance = new EffectInstance(this, context);
            instance.RemainingDuration = float.MaxValue; // 무한
            return instance;
        }

        /// <summary>
        /// Modifier 적용
        /// </summary>
        private void ApplyModifiers(EffectInstance instance, EffectContext context, GameObject target)
        {
            var attributeComponent = target.GetComponent<IAttributeComponent>();
            if (attributeComponent == null || modifiers.Count == 0) return;

            float magnitude = context.Magnitude;

            foreach (var config in modifiers)
            {
                if (config == null) continue;

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

                Debug.Log($"[InfiniteEffect] Applied infinite modifier to {config.attributeType}: {modifier.Operation} {modifier.Value}");
            }
        }

        /// <summary>
        /// Modifier 제거
        /// </summary>
        private void RemoveModifiers(EffectInstance instance, GameObject target)
        {
            var attributeComponent = target.GetComponent<IAttributeComponent>();
            if (attributeComponent == null) return;

            attributeComponent.RemoveAllModifiersFromSource(instance);

            Debug.Log($"[InfiniteEffect] Removed all modifiers from {target.name}");
        }

        #endregion

        #region Private Methods - Conditional Removal

        /// <summary>
        /// 조건부 제거 추적
        /// </summary>
        private async void StartConditionalTracking(EffectInstance instance, GameObject target)
        {
            var cts = new CancellationTokenSource();
            instance.Context.SetData("ConditionalCancellationToken", cts);

            try
            {
                while (!instance.IsExpired)
                {
                    await Awaitable.WaitForSecondsAsync(conditionalCheckInterval, cts.Token);

                    if (target == null || instance.IsExpired)
                        break;

                    // 자동 제거 조건 체크
                    if (autoRemovalCondition != null)
                    {
                        var tagComponent = target.GetComponent<TagComponent>();
                        if (tagComponent != null && tagComponent.SatisfiesRequirement(autoRemovalCondition))
                        {
                            Debug.Log($"[InfiniteEffect] Auto-removal condition met for {effectName}");
                            RemoveInstance(instance, target, false);
                            break;
                        }
                    }

                    // 제거 트리거 태그 체크
                    if (removalTriggerTags.Count > 0)
                    {
                        var tagComponent = target.GetComponent<TagComponent>();
                        if (tagComponent != null)
                        {
                            foreach (var triggerTag in removalTriggerTags)
                            {
                                if (tagComponent.HasTag(triggerTag))
                                {
                                    Debug.Log($"[InfiniteEffect] Removal trigger tag found: {triggerTag.TagString}");
                                    RemoveInstance(instance, target, false);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[InfiniteEffect] Conditional tracking cancelled for {effectName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[InfiniteEffect] Error in conditional tracking: {e.Message}");
            }
            finally
            {
                cts?.Dispose();
            }
        }

        #endregion

        #region Private Methods - Dynamic Modifiers

        /// <summary>
        /// Dynamic modifier 추적
        /// </summary>
        private async void StartDynamicModifierTracking(EffectInstance instance, GameObject target)
        {
            var cts = new CancellationTokenSource();
            instance.Context.SetData("DynamicCancellationToken", cts);

            try
            {
                float elapsedTime = 0f;

                while (!instance.IsExpired)
                {
                    await Awaitable.WaitForSecondsAsync(growthUpdateInterval, cts.Token);

                    if (target == null || instance.IsExpired)
                        break;

                    elapsedTime += growthUpdateInterval;

                    // Growth curve 기반 modifier 업데이트
                    float growthMultiplier = modifierGrowthCurve.Evaluate(elapsedTime);
                    growthMultiplier = Mathf.Min(growthMultiplier, maxGrowthMultiplier);

                    UpdateDynamicModifiers(instance, target, growthMultiplier);

                    Debug.Log($"[InfiniteEffect] Dynamic modifier updated: {effectName} - Multiplier: {growthMultiplier:F2}x");
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[InfiniteEffect] Dynamic tracking cancelled for {effectName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[InfiniteEffect] Error in dynamic tracking: {e.Message}");
            }
            finally
            {
                cts?.Dispose();
            }
        }

        /// <summary>
        /// Dynamic modifiers 업데이트
        /// </summary>
        private void UpdateDynamicModifiers(EffectInstance instance, GameObject target, float multiplier)
        {
            // 기존 modifiers 제거
            RemoveModifiers(instance, target);

            // 새로운 multiplier로 재적용
            var context = instance.Context;
            context.Magnitude = multiplier;
            ApplyModifiers(instance, context, target);
        }

        #endregion

        #region Private Methods - Aura System

        /// <summary>
        /// Aura 추적 시작
        /// </summary>
        private async void StartAuraTracking(EffectInstance instance, GameObject target)
        {
            var cts = new CancellationTokenSource();
            instance.Context.SetData("AuraCancellationToken", cts);

            var affectedTargets = new HashSet<GameObject>();
            instance.Context.SetData("AuraAffectedTargets", affectedTargets);

            try
            {
                float checkInterval = 0.5f; // 0.5초마다 체크

                while (!instance.IsExpired)
                {
                    await Awaitable.WaitForSecondsAsync(checkInterval, cts.Token);

                    if (target == null || instance.IsExpired)
                        break;

                    // 범위 내 대상 찾기
                    var colliders = Physics.OverlapSphere(
                        target.transform.position,
                        auraRadius,
                        auraTargetLayers
                    );

                    var currentTargets = new HashSet<GameObject>();

                    foreach (var collider in colliders)
                    {
                        if (collider.gameObject == target) continue; // 자신 제외

                        var potentialTarget = collider.gameObject;

                        // Tag requirement 체크
                        if (auraTargetRequirement != null)
                        {
                            var tagComponent = potentialTarget.GetComponent<TagComponent>();
                            if (tagComponent == null || !tagComponent.SatisfiesRequirement(auraTargetRequirement))
                                continue;
                        }

                        currentTargets.Add(potentialTarget);

                        // 새로운 대상에게 aura effect 적용
                        if (!affectedTargets.Contains(potentialTarget))
                        {
                            ApplyAuraEffect(potentialTarget, instance);
                            affectedTargets.Add(potentialTarget);
                        }
                    }

                    // 범위를 벗어난 대상에서 aura effect 제거
                    var targetsToRemove = new List<GameObject>();
                    foreach (var affected in affectedTargets)
                    {
                        if (!currentTargets.Contains(affected))
                        {
                            RemoveAuraEffect(affected, instance);
                            targetsToRemove.Add(affected);
                        }
                    }

                    foreach (var toRemove in targetsToRemove)
                    {
                        affectedTargets.Remove(toRemove);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[InfiniteEffect] Aura tracking cancelled for {effectName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[InfiniteEffect] Error in aura tracking: {e.Message}");
            }
            finally
            {
                // 모든 aura effect 제거
                if (instance.Context.HasData("AuraAffectedTargets"))
                {
                    var affected = instance.Context.GetData<HashSet<GameObject>>("AuraAffectedTargets");
                    foreach (var target2 in affected)
                    {
                        if (target2 != null)
                            RemoveAuraEffect(target2, instance);
                    }
                }

                cts?.Dispose();
            }
        }

        /// <summary>
        /// Aura effect 적용
        /// </summary>
        private void ApplyAuraEffect(GameObject target, EffectInstance sourceInstance)
        {
            if (auraGrantedEffect == null) return;

            var context = new EffectContext(
                sourceInstance.Context.Target, // Aura 소유자가 instigator
                target,
                auraGrantedEffect
            );
            context.SetData("IsAuraEffect", true);
            context.SetData("AuraSourceInstance", sourceInstance);

            auraGrantedEffect.Apply(context, target);

            Debug.Log($"[InfiniteEffect] Aura effect applied to {target.name}");
        }

        /// <summary>
        /// Aura effect 제거
        /// </summary>
        private void RemoveAuraEffect(GameObject target, EffectInstance sourceInstance)
        {
            if (auraGrantedEffect == null) return;

            var context = new EffectContext(
                sourceInstance.Context.Target,
                target,
                auraGrantedEffect
            );

            auraGrantedEffect.Remove(context, target);

            Debug.Log($"[InfiniteEffect] Aura effect removed from {target.name}");
        }

        #endregion

        #region Private Methods - Instance Management

        /// <summary>
        /// 기존 인스턴스 찾기
        /// </summary>
        private EffectInstance FindExistingInstance(GameObject target)
        {
            if (!activeInfiniteEffects.TryGetValue(target, out var instances))
                return null;

            return instances.Find(i => i.SourceEffect == this && !i.IsExpired);
        }

        /// <summary>
        /// 인스턴스 저장
        /// </summary>
        private void StoreInstance(GameObject target, EffectInstance instance)
        {
            if (!activeInfiniteEffects.ContainsKey(target))
            {
                activeInfiniteEffects[target] = new List<EffectInstance>();
            }

            activeInfiniteEffects[target].Add(instance);
        }

        /// <summary>
        /// 인스턴스 제거
        /// </summary>
        private bool RemoveInstance(EffectInstance instance, GameObject target, bool dispelled)
        {
            if (instance == null || target == null) return false;

            instance.IsExpired = true;

            // 모든 추적 취소
            CancelAllTracking(instance);

            // Modifier 제거
            RemoveModifiers(instance, target);

            // 태그 제거
            RemoveGrantedTags(target);

            // 시각 효과 제거
            if (instance.VisualEffect != null)
            {
                UnityEngine.Object.Destroy(instance.VisualEffect);
            }

            // 인스턴스 목록에서 제거
            if (activeInfiniteEffects.TryGetValue(target, out var instances))
            {
                instances.Remove(instance);
                if (instances.Count == 0)
                {
                    activeInfiniteEffects.Remove(target);
                }
            }

            // 제거 이벤트
            TriggerRemovalEvents(instance, target, dispelled);

            return true;
        }

        /// <summary>
        /// 모든 추적 취소
        /// </summary>
        private void CancelAllTracking(EffectInstance instance)
        {
            // Conditional tracking 취소
            if (instance.Context.HasData("ConditionalCancellationToken"))
            {
                var cts = instance.Context.GetData<CancellationTokenSource>("ConditionalCancellationToken");
                cts?.Cancel();
                cts?.Dispose();
            }

            // Dynamic tracking 취소
            if (instance.Context.HasData("DynamicCancellationToken"))
            {
                var cts = instance.Context.GetData<CancellationTokenSource>("DynamicCancellationToken");
                cts?.Cancel();
                cts?.Dispose();
            }

            // Aura tracking 취소
            if (instance.Context.HasData("AuraCancellationToken"))
            {
                var cts = instance.Context.GetData<CancellationTokenSource>("AuraCancellationToken");
                cts?.Cancel();
                cts?.Dispose();
            }
        }

        /// <summary>
        /// 스택 처리
        /// </summary>
        private bool HandleStacking(EffectInstance existingInstance, EffectContext context, GameObject target)
        {
            if (!CanStack(this, context))
                return false;

            switch (stackingPolicy)
            {
                case EffectStackingPolicy.Override:
                    RemoveInstance(existingInstance, target, false);
                    return Apply(context, target);

                case EffectStackingPolicy.Stack:
                    existingInstance.AddStack(1);
                    context.StackCount = existingInstance.CurrentStack;

                    // Dynamic modifier가 있으면 업데이트
                    if (useDynamicModifiers)
                    {
                        var multiplier = existingInstance.CurrentStack * context.Magnitude;
                        UpdateDynamicModifiers(existingInstance, target, multiplier);
                    }

                    TriggerStackEvent(existingInstance, target);

                    Debug.Log($"[InfiniteEffect] Stacked {effectName} on {target.name}. Stack: {existingInstance.CurrentStack}");
                    return true;

                default:
                    return false;
            }
        }

        #endregion

        #region Private Methods - Effects & Events

        /// <summary>
        /// 적용 효과 재생
        /// </summary>
        private void PlayApplicationEffects(EffectInstance instance, GameObject target)
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
        /// 적용 이벤트 발생
        /// </summary>
        private void TriggerApplicationEvents(EffectInstance instance, GameObject target)
        {
            GASEvents.TriggerEffectApplied(target, effectName);

            string effectTypeText = isAuraEffect ? "Aura" : "Passive";
            Debug.Log($"[InfiniteEffect] {effectTypeText} applied: {effectName} - Can be dispelled: {canBeDispelled}");
        }

        /// <summary>
        /// 제거 이벤트 발생
        /// </summary>
        private void TriggerRemovalEvents(EffectInstance instance, GameObject target, bool dispelled)
        {
            GASEvents.TriggerEffectRemoved(target, effectName);

            string removalReason = dispelled ? "dispelled" : "condition met";
            Debug.Log($"[InfiniteEffect] Removed ({removalReason}): {effectName} from {target.name}");
        }

        /// <summary>
        /// 스택 이벤트 발생
        /// </summary>
        private void TriggerStackEvent(EffectInstance instance, GameObject target)
        {
            GASEvents.TriggerEffectStacked(target, effectName, instance.CurrentStack);

            Debug.Log($"[InfiniteEffect] Stack changed: {effectName} - New stack count: {instance.CurrentStack}");
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// 특정 타겟의 모든 InfiniteEffect 제거
        /// </summary>
        public static void RemoveAllFromTarget(GameObject target, bool dispellableOnly = false)
        {
            if (!activeInfiniteEffects.TryGetValue(target, out var instances))
                return;

            var toRemove = new List<EffectInstance>(instances);
            foreach (var instance in toRemove)
            {
                if (instance.SourceEffect is InfiniteEffect infiniteEffect)
                {
                    if (!dispellableOnly || infiniteEffect.canBeDispelled)
                    {
                        infiniteEffect.RemoveInstance(instance, target, true);
                    }
                }
            }
        }

        /// <summary>
        /// 특정 타겟의 InfiniteEffect 개수 확인
        /// </summary>
        public static int GetActiveCount(GameObject target)
        {
            if (!activeInfiniteEffects.TryGetValue(target, out var instances))
                return 0;

            return instances.Count(i => !i.IsExpired);
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            // InfiniteEffect 강제 설정
            effectType = EffectType.Infinite;
            durationPolicy = EffectDurationPolicy.Infinite;
            duration = 0f;
            period = 0f;

            // Aura 설정 검증
            if (isAuraEffect)
            {
                auraRadius = Mathf.Max(0.1f, auraRadius);
            }

            // Growth 설정 검증
            if (useDynamicModifiers)
            {
                growthUpdateInterval = Mathf.Max(0.1f, growthUpdateInterval);
                maxGrowthMultiplier = Mathf.Max(1f, maxGrowthMultiplier);
            }

            // Conditional check interval 검증
            conditionalCheckInterval = Mathf.Max(0.1f, conditionalCheckInterval);
        }

        private void OnDrawGizmosSelected()
        {
            // Aura 범위 시각화
            if (isAuraEffect)
            {
                Gizmos.color = new Color(0, 1, 1, 0.3f);
                //Gizmos.DrawWireSphere(transform.position, auraRadius);
            }
        }
#endif

        #endregion
    }
}

// 파일 위치: Assets/Scripts/GAS/EffectSystem/Types/InfiniteEffect.cs