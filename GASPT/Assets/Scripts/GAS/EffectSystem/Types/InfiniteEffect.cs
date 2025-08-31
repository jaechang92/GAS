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
    /// �������� �����ϱ� ������ ���������� ���ӵǴ� GameplayEffect
    /// ��: �нú� ��ų, ���� ����, ����, ��� ȿ��
    /// </summary>
    [CreateAssetMenu(fileName = "InfiniteEffect", menuName = "GAS/Effects/Infinite Effect", order = 4)]
    public class InfiniteEffect : GameplayEffect
    {
        #region Additional Fields

        [Header("=== Infinite Effect Settings ===")]
        [SerializeField] private bool canBeDispelled = true;
        [SerializeField] private bool persistThroughDeath = false;
        [SerializeField] private bool persistThroughSceneChange = false;
        [SerializeField] private int dispelResistance = 0; // ���� ���� ����

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

        // Ȱ�� �ν��Ͻ� ����
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
        /// Infinite ȿ�� ����
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
                // ���� �ν��Ͻ� Ȯ��
                EffectInstance existingInstance = FindExistingInstance(target);

                if (existingInstance != null)
                {
                    return HandleStacking(existingInstance, context, target);
                }

                // �� �ν��Ͻ� ����
                var instance = CreateInfiniteInstance(context, target);

                // Modifier ����
                ApplyModifiers(instance, context, target);

                // �±� �ο�
                GrantTags(target);

                // �ð�/û�� ȿ��
                PlayApplicationEffects(instance, target);

                // ���Ǻ� ���� ���� ����
                if (autoRemovalCondition != null || removalTriggerTags.Count > 0)
                {
                    StartConditionalTracking(instance, target);
                }

                // Dynamic modifier ���� ����
                if (useDynamicModifiers)
                {
                    StartDynamicModifierTracking(instance, target);
                }

                // Aura ���� ����
                if (isAuraEffect)
                {
                    StartAuraTracking(instance, target);
                }

                // �̺�Ʈ �߻�
                TriggerApplicationEvents(instance, target);

                // �ν��Ͻ� ����
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
        /// Infinite ȿ�� ����
        /// </summary>
        public override bool Remove(EffectContext context, GameObject target)
        {
            var instance = FindExistingInstance(target);
            if (instance == null)
            {
                Debug.LogWarning($"[InfiniteEffect] No instance of {effectName} found on {target.name}");
                return false;
            }

            // Dispel ���� ���� Ȯ��
            if (!canBeDispelled)
            {
                Debug.LogWarning($"[InfiniteEffect] {effectName} cannot be dispelled");
                return false;
            }

            // Dispel ���� üũ
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
        /// InfiniteEffect�� �ֱ��� ������ �������� ����
        /// </summary>
        public override bool ExecutePeriodic(EffectContext context, GameObject target)
        {
            Debug.LogWarning($"[InfiniteEffect] {effectName} does not support periodic execution");
            return false;
        }

        #endregion

        #region Private Methods - Core

        /// <summary>
        /// Infinite �ν��Ͻ� ����
        /// </summary>
        private EffectInstance CreateInfiniteInstance(EffectContext context, GameObject target)
        {
            var instance = new EffectInstance(this, context);
            instance.RemainingDuration = float.MaxValue; // ����
            return instance;
        }

        /// <summary>
        /// Modifier ����
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

                // Modifier ���� �� ID ����
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
        /// Modifier ����
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
        /// ���Ǻ� ���� ����
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

                    // �ڵ� ���� ���� üũ
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

                    // ���� Ʈ���� �±� üũ
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
        /// Dynamic modifier ����
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

                    // Growth curve ��� modifier ������Ʈ
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
        /// Dynamic modifiers ������Ʈ
        /// </summary>
        private void UpdateDynamicModifiers(EffectInstance instance, GameObject target, float multiplier)
        {
            // ���� modifiers ����
            RemoveModifiers(instance, target);

            // ���ο� multiplier�� ������
            var context = instance.Context;
            context.Magnitude = multiplier;
            ApplyModifiers(instance, context, target);
        }

        #endregion

        #region Private Methods - Aura System

        /// <summary>
        /// Aura ���� ����
        /// </summary>
        private async void StartAuraTracking(EffectInstance instance, GameObject target)
        {
            var cts = new CancellationTokenSource();
            instance.Context.SetData("AuraCancellationToken", cts);

            var affectedTargets = new HashSet<GameObject>();
            instance.Context.SetData("AuraAffectedTargets", affectedTargets);

            try
            {
                float checkInterval = 0.5f; // 0.5�ʸ��� üũ

                while (!instance.IsExpired)
                {
                    await Awaitable.WaitForSecondsAsync(checkInterval, cts.Token);

                    if (target == null || instance.IsExpired)
                        break;

                    // ���� �� ��� ã��
                    var colliders = Physics.OverlapSphere(
                        target.transform.position,
                        auraRadius,
                        auraTargetLayers
                    );

                    var currentTargets = new HashSet<GameObject>();

                    foreach (var collider in colliders)
                    {
                        if (collider.gameObject == target) continue; // �ڽ� ����

                        var potentialTarget = collider.gameObject;

                        // Tag requirement üũ
                        if (auraTargetRequirement != null)
                        {
                            var tagComponent = potentialTarget.GetComponent<TagComponent>();
                            if (tagComponent == null || !tagComponent.SatisfiesRequirement(auraTargetRequirement))
                                continue;
                        }

                        currentTargets.Add(potentialTarget);

                        // ���ο� ��󿡰� aura effect ����
                        if (!affectedTargets.Contains(potentialTarget))
                        {
                            ApplyAuraEffect(potentialTarget, instance);
                            affectedTargets.Add(potentialTarget);
                        }
                    }

                    // ������ ��� ��󿡼� aura effect ����
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
                // ��� aura effect ����
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
        /// Aura effect ����
        /// </summary>
        private void ApplyAuraEffect(GameObject target, EffectInstance sourceInstance)
        {
            if (auraGrantedEffect == null) return;

            var context = new EffectContext(
                sourceInstance.Context.Target, // Aura �����ڰ� instigator
                target,
                auraGrantedEffect
            );
            context.SetData("IsAuraEffect", true);
            context.SetData("AuraSourceInstance", sourceInstance);

            auraGrantedEffect.Apply(context, target);

            Debug.Log($"[InfiniteEffect] Aura effect applied to {target.name}");
        }

        /// <summary>
        /// Aura effect ����
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
        /// ���� �ν��Ͻ� ã��
        /// </summary>
        private EffectInstance FindExistingInstance(GameObject target)
        {
            if (!activeInfiniteEffects.TryGetValue(target, out var instances))
                return null;

            return instances.Find(i => i.SourceEffect == this && !i.IsExpired);
        }

        /// <summary>
        /// �ν��Ͻ� ����
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
        /// �ν��Ͻ� ����
        /// </summary>
        private bool RemoveInstance(EffectInstance instance, GameObject target, bool dispelled)
        {
            if (instance == null || target == null) return false;

            instance.IsExpired = true;

            // ��� ���� ���
            CancelAllTracking(instance);

            // Modifier ����
            RemoveModifiers(instance, target);

            // �±� ����
            RemoveGrantedTags(target);

            // �ð� ȿ�� ����
            if (instance.VisualEffect != null)
            {
                UnityEngine.Object.Destroy(instance.VisualEffect);
            }

            // �ν��Ͻ� ��Ͽ��� ����
            if (activeInfiniteEffects.TryGetValue(target, out var instances))
            {
                instances.Remove(instance);
                if (instances.Count == 0)
                {
                    activeInfiniteEffects.Remove(target);
                }
            }

            // ���� �̺�Ʈ
            TriggerRemovalEvents(instance, target, dispelled);

            return true;
        }

        /// <summary>
        /// ��� ���� ���
        /// </summary>
        private void CancelAllTracking(EffectInstance instance)
        {
            // Conditional tracking ���
            if (instance.Context.HasData("ConditionalCancellationToken"))
            {
                var cts = instance.Context.GetData<CancellationTokenSource>("ConditionalCancellationToken");
                cts?.Cancel();
                cts?.Dispose();
            }

            // Dynamic tracking ���
            if (instance.Context.HasData("DynamicCancellationToken"))
            {
                var cts = instance.Context.GetData<CancellationTokenSource>("DynamicCancellationToken");
                cts?.Cancel();
                cts?.Dispose();
            }

            // Aura tracking ���
            if (instance.Context.HasData("AuraCancellationToken"))
            {
                var cts = instance.Context.GetData<CancellationTokenSource>("AuraCancellationToken");
                cts?.Cancel();
                cts?.Dispose();
            }
        }

        /// <summary>
        /// ���� ó��
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

                    // Dynamic modifier�� ������ ������Ʈ
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
        /// ���� ȿ�� ���
        /// </summary>
        private void PlayApplicationEffects(EffectInstance instance, GameObject target)
        {
            // �ð� ȿ��
            if (effectPrefab != null)
            {
                var vfx = CreateVisualEffect(target);
                instance.VisualEffect = vfx;
            }

            // ����
            if (applicationSound != null)
            {
                PlaySound(applicationSound, target.transform.position);
            }
        }

        /// <summary>
        /// ���� �̺�Ʈ �߻�
        /// </summary>
        private void TriggerApplicationEvents(EffectInstance instance, GameObject target)
        {
            GASEvents.TriggerEffectApplied(target, effectName);

            string effectTypeText = isAuraEffect ? "Aura" : "Passive";
            Debug.Log($"[InfiniteEffect] {effectTypeText} applied: {effectName} - Can be dispelled: {canBeDispelled}");
        }

        /// <summary>
        /// ���� �̺�Ʈ �߻�
        /// </summary>
        private void TriggerRemovalEvents(EffectInstance instance, GameObject target, bool dispelled)
        {
            GASEvents.TriggerEffectRemoved(target, effectName);

            string removalReason = dispelled ? "dispelled" : "condition met";
            Debug.Log($"[InfiniteEffect] Removed ({removalReason}): {effectName} from {target.name}");
        }

        /// <summary>
        /// ���� �̺�Ʈ �߻�
        /// </summary>
        private void TriggerStackEvent(EffectInstance instance, GameObject target)
        {
            GASEvents.TriggerEffectStacked(target, effectName, instance.CurrentStack);

            Debug.Log($"[InfiniteEffect] Stack changed: {effectName} - New stack count: {instance.CurrentStack}");
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Ư�� Ÿ���� ��� InfiniteEffect ����
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
        /// Ư�� Ÿ���� InfiniteEffect ���� Ȯ��
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

            // InfiniteEffect ���� ����
            effectType = EffectType.Infinite;
            durationPolicy = EffectDurationPolicy.Infinite;
            duration = 0f;
            period = 0f;

            // Aura ���� ����
            if (isAuraEffect)
            {
                auraRadius = Mathf.Max(0.1f, auraRadius);
            }

            // Growth ���� ����
            if (useDynamicModifiers)
            {
                growthUpdateInterval = Mathf.Max(0.1f, growthUpdateInterval);
                maxGrowthMultiplier = Mathf.Max(1f, maxGrowthMultiplier);
            }

            // Conditional check interval ����
            conditionalCheckInterval = Mathf.Max(0.1f, conditionalCheckInterval);
        }

        private void OnDrawGizmosSelected()
        {
            // Aura ���� �ð�ȭ
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

// ���� ��ġ: Assets/Scripts/GAS/EffectSystem/Types/InfiniteEffect.cs