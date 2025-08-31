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
    /// ���� �ð� ���� ���ӵǴ� GameplayEffect
    /// ��: ����, �����, �Ͻ��� ���� ���� ��
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

        // Ȱ�� �ν��Ͻ� ���� (��Ÿ�ӿ�, serialize ����)
        private static Dictionary<GameObject, List<EffectInstance>> activeInstances =
            new Dictionary<GameObject, List<EffectInstance>>();

        #endregion

        #region Constructor

        public DurationEffect()
        {
            effectType = EffectType.Duration;
            durationPolicy = EffectDurationPolicy.HasDuration;
            duration = 5f; // �⺻ 5��
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Duration ȿ�� ����
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
                // 1. ���� ȿ������ ���� ó��
                EffectInstance existingInstance = FindExistingInstance(target);

                if (existingInstance != null)
                {
                    return HandleStacking(existingInstance, context, target);
                }

                // 2. �� �ν��Ͻ� ����
                var instance = CreateInstance(context, target);

                // 3. Modifier ����
                ApplyModifiers(instance, context, target);

                // 4. �±� �ο�
                GrantTags(target);

                // 5. �ð�/û�� ȿ��
                PlayApplicationEffects(instance, target);

                // 6. Duration ���� ����
                StartDurationTracking(instance, target);

                // 7. �̺�Ʈ �߻�
                TriggerApplicationEvents(instance, target);

                // 8. �ν��Ͻ� ����
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
        /// Duration ȿ�� ����
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
        /// DurationEffect�� �ֱ��� ������ �������� ����
        /// </summary>
        public override bool ExecutePeriodic(EffectContext context, GameObject target)
        {
            Debug.LogWarning($"[DurationEffect] {effectName} does not support periodic execution. Use PeriodicEffect instead.");
            return false;
        }

        #endregion

        #region Private Methods - Core

        /// <summary>
        /// Effect �ν��Ͻ� ����
        /// </summary>
        private EffectInstance CreateInstance(EffectContext context, GameObject target)
        {
            var instance = new EffectInstance(this, context);

            // Duration ���
            float finalDuration = CalculateDuration(context);
            instance.RemainingDuration = finalDuration;

            return instance;
        }

        /// <summary>
        /// Duration ���
        /// </summary>
        protected float CalculateDuration(EffectContext context)
        {
            float baseDuration = duration;

            // Stack ��� duration scaling
            if (scaleDurationWithStackCount && context.StackCount > 1)
            {
                baseDuration += durationPerStack * (context.StackCount - 1);
            }

            // Context magnitude scaling
            baseDuration *= context.Magnitude;

            return Mathf.Max(0.1f, baseDuration);
        }

        /// <summary>
        /// Modifier ����
        /// </summary>
        protected void ApplyModifiers(EffectInstance instance, EffectContext context, GameObject target)
        {
            var attributeComponent = target.GetComponent<IAttributeComponent>();
            if (attributeComponent == null || modifiers.Count == 0) return;

            float magnitude = context.Magnitude;

            // �ð��� ���� scaling ����
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

                // Modifier ���� �� ID ����
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
        /// Modifier ����
        /// </summary>
        private void RemoveModifiers(EffectInstance instance, GameObject target)
        {
            var attributeComponent = target.GetComponent<IAttributeComponent>();
            if (attributeComponent == null) return;

            // ��� modifier�� source(instance)�� ����
            attributeComponent.RemoveAllModifiersFromSource(instance);

            Debug.Log($"[DurationEffect] Removed all modifiers from {target.name}");
        }

        #endregion

        #region Private Methods - Duration Tracking

        /// <summary>
        /// Duration ���� ����
        /// </summary>
        private async void StartDurationTracking(EffectInstance instance, GameObject target)
        {
            var cts = new CancellationTokenSource();
            instance.Context.SetData("CancellationToken", cts);

            try
            {
                float updateInterval = 0.1f; // 100ms���� ������Ʈ
                float elapsed = 0f;

                while (!instance.IsExpired && instance.RemainingDuration > 0)
                {
                    await Awaitable.WaitForSecondsAsync(updateInterval, cts.Token);

                    if (target == null || instance.IsExpired)
                        break;

                    elapsed += updateInterval;
                    instance.RemainingDuration -= updateInterval;

                    // �ð��� ���� Modifier scaling ������Ʈ
                    if (scaleModifiersOverTime)
                    {
                        UpdateModifierScaling(instance, target, instance.Progress);
                    }

                    // UI ������Ʈ �α�
                    if (showInUI && elapsed % 1f < updateInterval) // 1�ʸ��� �α�
                    {
                        Debug.Log($"[DurationEffect] {effectName} - Remaining: {instance.RemainingDuration:F1}s, Progress: {instance.Progress:P0}");
                    }

                    // ���� üũ
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
        /// ���� ó��
        /// </summary>
        protected async Awaitable HandleExpiration(EffectInstance instance, GameObject target)
        {
            if (instance.IsExpired) return;

            instance.IsExpired = true;

            Debug.Log($"[DurationEffect] {effectName} expired on {target.name}");

            // ���� ȿ�� ����
            if (triggerExpirationEffects)
            {
                ExecuteExpirationEffects(instance, target);
            }

            // �ν��Ͻ� ����
            RemoveInstance(instance, target, false);

            // ���� �̺�Ʈ
            GASEvents.TriggerEffectRemoved(target, effectName);

            Debug.Log($"[DurationEffect] Effect expired: {effectName} on {target.name}");

            await Awaitable.EndOfFrameAsync();
        }

        /// <summary>
        /// Modifier scaling ������Ʈ
        /// </summary>
        private void UpdateModifierScaling(EffectInstance instance, GameObject target, float progress)
        {
            if (!scaleModifiersOverTime || modifierScalingCurve == null) return;

            var attributeComponent = target.GetComponent<IAttributeComponent>();
            if (attributeComponent == null) return;

            float scaleFactor = modifierScalingCurve.Evaluate(progress);

            // ���� modifier ���� �� ���ο� ������ ������
            RemoveModifiers(instance, target);

            var context = instance.Context;
            context.Magnitude = scaleFactor;
            ApplyModifiers(instance, context, target);
        }

        #endregion

        #region Private Methods - Stacking

        /// <summary>
        /// ���� ó��
        /// </summary>
        protected virtual bool HandleStacking(EffectInstance existingInstance, EffectContext context, GameObject target)
        {
            if (!CanStack(this, context))
                return false;

            switch (stackingPolicy)
            {
                case EffectStackingPolicy.Override:
                    // ���� ȿ�� ���� �� ���� ����
                    RemoveInstance(existingInstance, target, false);
                    return Apply(context, target);

                case EffectStackingPolicy.Stack:
                    // ���� ����
                    existingInstance.AddStack(1);
                    context.StackCount = existingInstance.CurrentStack;

                    // Duration refresh
                    if (refreshDurationOnStack)
                    {
                        existingInstance.RefreshDuration();

                        // Duration tracking �����
                        CancelDurationTracking(existingInstance);
                        StartDurationTracking(existingInstance, target);
                    }

                    // ���� �̺�Ʈ
                    TriggerStackEvent(existingInstance, target);

                    Debug.Log($"[DurationEffect] Stacked {effectName} on {target.name}. Stack: {existingInstance.CurrentStack}");
                    return true;

                case EffectStackingPolicy.AggregateBySource:
                case EffectStackingPolicy.AggregateByTarget:
                    // ���� source/target�� ȿ�� ����
                    return HandleAggregation(existingInstance, context, target);

                default:
                    return false;
            }
        }

        /// <summary>
        /// ���� ó��
        /// </summary>
        private bool HandleAggregation(EffectInstance existingInstance, EffectContext context, GameObject target)
        {
            // ���� ����: source/target�� ���� ����
            Debug.LogWarning($"[DurationEffect] Aggregation not yet implemented for {effectName}");
            return false;
        }

        #endregion

        #region Private Methods - Instance Management

        /// <summary>
        /// ���� �ν��Ͻ� ã��
        /// </summary>
        protected EffectInstance FindExistingInstance(GameObject target)
        {
            if (!activeInstances.TryGetValue(target, out var instances))
                return null;

            return instances.Find(i => i.SourceEffect == this && !i.IsExpired);
        }

        /// <summary>
        /// �ν��Ͻ� ����
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
        /// �ν��Ͻ� ����
        /// </summary>
        private bool RemoveInstance(EffectInstance instance, GameObject target, bool cancelled)
        {
            if (instance == null || target == null) return false;

            // Duration tracking ���
            CancelDurationTracking(instance);

            // Modifier ����
            RemoveModifiers(instance, target);

            // �±� ����
            if (removeTagsOnExpiration)
            {
                RemoveGrantedTags(target);
            }

            // �ð� ȿ�� ����
            if (instance.VisualEffect != null)
            {
                UnityEngine.Object.Destroy(instance.VisualEffect);
            }

            // �ν��Ͻ� ��Ͽ��� ����
            if (activeInstances.TryGetValue(target, out var instances))
            {
                instances.Remove(instance);
                if (instances.Count == 0)
                {
                    activeInstances.Remove(target);
                }
            }

            // ���� �̺�Ʈ
            TriggerRemovalEvents(instance, target, cancelled);

            return true;
        }

        /// <summary>
        /// Duration tracking ���
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
        /// ���� ȿ�� ���
        /// </summary>
        protected void PlayApplicationEffects(EffectInstance instance, GameObject target)
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
        /// ���� ȿ�� ����
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

            // ���� ����
            if (removalSound != null)
            {
                PlaySound(removalSound, target.transform.position);
            }
        }

        /// <summary>
        /// ���� �̺�Ʈ �߻�
        /// </summary>
        protected void TriggerApplicationEvents(EffectInstance instance, GameObject target)
        {
            GASEvents.TriggerEffectApplied(target, effectName);

            Debug.Log($"[DurationEffect] Applied: {effectName} - Duration: {instance.RemainingDuration:F1}s, Stack: {instance.CurrentStack}");
        }

        /// <summary>
        /// ���� �̺�Ʈ �߻�
        /// </summary>
        private void TriggerRemovalEvents(EffectInstance instance, GameObject target, bool cancelled)
        {
            GASEvents.TriggerEffectRemoved(target, effectName);

            string cancelText = cancelled ? "cancelled" : "expired";
            Debug.Log($"[DurationEffect] Removed ({cancelText}): {effectName} - Elapsed: {instance.ElapsedTime:F1}s");
        }

        /// <summary>
        /// ���� �̺�Ʈ �߻�
        /// </summary>
        private void TriggerStackEvent(EffectInstance instance, GameObject target)
        {
            GASEvents.TriggerEffectStacked(target, effectName, instance.CurrentStack);

            Debug.Log($"[DurationEffect] Stack changed: {effectName} - New stack count: {instance.CurrentStack}");
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Ư�� Ÿ���� ��� DurationEffect ����
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
        /// Ư�� Ÿ���� DurationEffect ���� Ȯ��
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

            // DurationEffect ���� ����
            effectType = EffectType.Duration;
            durationPolicy = EffectDurationPolicy.HasDuration;

            // Duration ����
            if (duration <= 0f) duration = 1f;

            // Stack duration ����
            if (scaleDurationWithStackCount && durationPerStack < 0f)
            {
                durationPerStack = 0f;
            }
        }
#endif

        #endregion
    }
}

// ���� ��ġ: Assets/Scripts/GAS/EffectSystem/Types/DurationEffect.cs