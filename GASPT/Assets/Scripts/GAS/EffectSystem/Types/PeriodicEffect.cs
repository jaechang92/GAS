using GAS.AttributeSystem;
using GAS.Core;
using GAS.TagSystem;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.EffectSystem
{
    /// <summary>
    /// �ֱ������� ȿ���� �߻��ϴ� GameplayEffect
    /// ��: ��, ȭ��, ���, DOT(Damage Over Time), HOT(Heal Over Time)
    /// </summary>
    [CreateAssetMenu(fileName = "PeriodicEffect", menuName = "GAS/Effects/Periodic Effect", order = 3)]
    public class PeriodicEffect : DurationEffect
    {
        #region Additional Fields

        [Header("=== Periodic Settings ===")]
        [SerializeField] private int maxTicks = -1; // -1 = ������
        [SerializeField] private bool executeOnFirstApplication = true;
        [SerializeField] private bool executeOnLastTick = true;
        [SerializeField] private bool scaleTickWithStack = true;

        [Header("=== Tick Modifiers ===")]
        [SerializeField] private List<ModifierConfig> tickModifiers = new List<ModifierConfig>();
        [SerializeField] private AnimationCurve tickDamageCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Header("=== Tick Effects ===")]
        [SerializeField] private List<InstantEffect> onTickEffects = new List<InstantEffect>();
        [SerializeField] private bool useRandomTickEffect = false;

        [Header("=== Tick Visuals ===")]
        [SerializeField] private GameObject tickVisualPrefab;
        [SerializeField] private AudioClip tickSound;
        [SerializeField] private float tickVisualDuration = 0.5f;

        [Header("=== Advanced Periodic ===")]
        [SerializeField] private bool variablePeriod = false;
        [SerializeField] private float periodVariance = 0.1f;
        [SerializeField] private bool accelerateOverTime = false;
        [SerializeField] private float accelerationRate = 0.9f;

        #endregion

        #region Constructor

        public PeriodicEffect()
        {
            effectType = EffectType.Periodic;
            durationPolicy = EffectDurationPolicy.HasDuration;
            period = 1f; // �⺻ 1�ʸ��� ƽ
            duration = 5f; // �⺻ 5�� ����
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Periodic ȿ�� ����
        /// </summary>
        public override bool Apply(EffectContext context, GameObject target)
        {
            if (!CanApply(context, target))
            {
                Debug.LogWarning($"[PeriodicEffect] Cannot apply {effectName} to {target.name}");
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
                var instance = CreatePeriodicInstance(context, target);

                // �ʱ� Modifier ���� (Duration ȿ��)
                if (modifiers.Count > 0)
                {
                    ApplyModifiers(instance, context, target);
                }

                // �±� �ο�
                GrantTags(target);

                // �ð�/û�� ȿ��
                PlayApplicationEffects(instance, target);

                // ù ƽ ��� ����
                if (executeOnFirstApplication)
                {
                    ExecutePeriodicTick(instance, context, target);
                }

                // Periodic ���� ����
                StartPeriodicTracking(instance, target);

                // �̺�Ʈ �߻�
                TriggerApplicationEvents(instance, target);

                // �ν��Ͻ� ����
                StoreInstance(target, instance);

                int expectedTicks = CalculateExpectedTicks(instance);
                Debug.Log($"[PeriodicEffect] Applied {effectName} to {target.name} - Period: {period}s, Expected ticks: {expectedTicks}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PeriodicEffect] Failed to apply {effectName}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// �ֱ��� ���� (DurationEffect override)
        /// </summary>
        public override bool ExecutePeriodic(EffectContext context, GameObject target)
        {
            var instance = FindExistingInstance(target);
            if (instance == null)
            {
                Debug.LogWarning($"[PeriodicEffect] No instance found for periodic execution");
                return false;
            }

            return ExecutePeriodicTick(instance, context, target);
        }

        #endregion

        #region Private Methods - Periodic Logic

        /// <summary>
        /// Periodic �ν��Ͻ� ����
        /// </summary>
        private EffectInstance CreatePeriodicInstance(EffectContext context, GameObject target)
        {
            var instance = new EffectInstance(this, context);

            // Duration ���
            float finalDuration = CalculateDuration(context);
            instance.RemainingDuration = finalDuration;

            // ���� ƽ �ð� ����
            float nextPeriod = CalculateNextPeriod(0);
            instance.NextPeriodicTime = Time.time + nextPeriod;

            return instance;
        }

        /// <summary>
        /// Periodic ���� ����
        /// </summary>
        private async void StartPeriodicTracking(EffectInstance instance, GameObject target)
        {
            var cts = new CancellationTokenSource();
            instance.Context.SetData("PeriodicCancellationToken", cts);

            try
            {
                float updateInterval = 0.05f; // 50ms ���е�
                float currentPeriod = period;
                int tickCount = executeOnFirstApplication ? 1 : 0;

                while (!instance.IsExpired && instance.RemainingDuration > 0)
                {
                    await Awaitable.WaitForSecondsAsync(updateInterval, cts.Token);

                    if (target == null || instance.IsExpired)
                        break;

                    // Duration ����
                    instance.RemainingDuration -= updateInterval;

                    // �ֱ� üũ
                    if (Time.time >= instance.NextPeriodicTime)
                    {
                        // �ִ� ƽ üũ
                        if (maxTicks > 0 && tickCount >= maxTicks)
                        {
                            Debug.Log($"[PeriodicEffect] Max ticks ({maxTicks}) reached for {effectName}");
                            break;
                        }

                        // ƽ ����
                        bool success = ExecutePeriodicTick(instance, instance.Context, target);
                        if (!success)
                        {
                            Debug.LogWarning($"[PeriodicEffect] Tick execution failed for {effectName}");
                        }

                        tickCount++;

                        // ���� �ֱ� ���
                        currentPeriod = CalculateNextPeriod(tickCount);
                        instance.NextPeriodicTime = Time.time + currentPeriod;

                        // ���� ����
                        if (accelerateOverTime)
                        {
                            currentPeriod *= accelerationRate;
                            currentPeriod = Mathf.Max(0.1f, currentPeriod);
                        }
                    }

                    // ���� ���� ������ ƽ
                    if (instance.RemainingDuration <= updateInterval && executeOnLastTick)
                    {
                        if (tickCount == 0 || Time.time - instance.NextPeriodicTime + currentPeriod > updateInterval)
                        {
                            ExecutePeriodicTick(instance, instance.Context, target);
                            Debug.Log($"[PeriodicEffect] Final tick executed for {effectName}");
                        }
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
                Debug.Log($"[PeriodicEffect] Periodic tracking cancelled for {effectName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[PeriodicEffect] Error in periodic tracking: {e.Message}");
            }
            finally
            {
                cts?.Dispose();
            }
        }

        /// <summary>
        /// �ֱ��� ƽ ����
        /// </summary>
        private bool ExecutePeriodicTick(EffectInstance instance, EffectContext context, GameObject target)
        {
            if (target == null) return false;

            instance.IncrementPeriodicCount();

            Debug.Log($"[PeriodicEffect] Tick #{instance.PeriodicExecutionCount} for {effectName} on {target.name}");

            try
            {
                // ƽ ���� ���
                float tickMagnitude = CalculateTickMagnitude(instance, context);

                // 1. Tick Modifiers ����
                ApplyTickModifiers(target, tickMagnitude, instance);

                // 2. Tick Effects ����
                ApplyTickEffects(target, context, instance);

                // 3. �ð�/û�� ȿ��
                PlayTickEffects(target);

                // 4. �̺�Ʈ �߻�
                TriggerTickEvent(instance, target, tickMagnitude);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PeriodicEffect] Failed to execute tick: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// ƽ ���� ���
        /// </summary>
        private float CalculateTickMagnitude(EffectInstance instance, EffectContext context)
        {
            float magnitude = context.Magnitude;

            // ���� ��� scaling
            if (scaleTickWithStack && instance.CurrentStack > 1)
            {
                magnitude *= instance.CurrentStack;
            }

            // �ð� ��� curve
            if (tickDamageCurve != null && tickDamageCurve.length > 0)
            {
                float progress = instance.Progress;
                magnitude *= tickDamageCurve.Evaluate(progress);
            }

            return magnitude;
        }

        /// <summary>
        /// Tick Modifiers ����
        /// </summary>
        private void ApplyTickModifiers(GameObject target, float magnitude, EffectInstance instance)
        {
            if (tickModifiers == null || tickModifiers.Count == 0) return;

            var attributeComponent = target.GetComponent<IAttributeComponent>();
            if (attributeComponent == null) return;

            foreach (var config in tickModifiers)
            {
                if (config == null) continue;

                // ��� ���� modifier (tick�� ���������� ����)
                float currentValue = attributeComponent.GetAttributeValue(config.attributeType);
                float modifiedValue = currentValue;

                switch (config.operation)
                {
                    case ModifierOperation.Add:
                        modifiedValue = currentValue + (config.value * magnitude);
                        break;
                    case ModifierOperation.Multiply:
                        modifiedValue = currentValue * (config.value * magnitude);
                        break;
                    case ModifierOperation.Override:
                        modifiedValue = config.value * magnitude;
                        break;
                }

                // AttributeSetComponent�� BaseValue ���� ����
                if (attributeComponent is AttributeSetComponent attrComponent)
                {
                    var attributeSet = attrComponent.GetAttributeSet<AttributeSet>();
                    BaseAttribute attribute = attributeSet.GetAttribute(config.attributeType);
                    if (attributeSet != null && attribute != null)
                    {
                        attribute.BaseValue = modifiedValue;

                        // ���� �̺�Ʈ
                        GASEvents.TriggerAttributeChanged(
                            target,
                            config.attributeType.ToString(),
                            currentValue,
                            modifiedValue
                        );

                        // ƽ ������/�� �α�
                        float changeAmount = modifiedValue - currentValue;
                        string changeType = changeAmount < 0 ? "damage" : "heal";
                        Debug.Log($"[PeriodicEffect] Tick {changeType}: {config.attributeType} {changeAmount:+0.##}");
                    }
                }
            }
        }

        /// <summary>
        /// Tick Effects ����
        /// </summary>
        private void ApplyTickEffects(GameObject target, EffectContext context, EffectInstance instance)
        {
            if (onTickEffects == null || onTickEffects.Count == 0) return;

            if (useRandomTickEffect)
            {
                // �����ϰ� �ϳ� ����
                int randomIndex = UnityEngine.Random.Range(0, onTickEffects.Count);
                var effect = onTickEffects[randomIndex];
                if (effect != null)
                {
                    var tickContext = context.Clone();
                    tickContext.SetData("TickNumber", instance.PeriodicExecutionCount);
                    effect.Apply(tickContext, target);
                }
            }
            else
            {
                // ��� tick effect ����
                foreach (var effect in onTickEffects)
                {
                    if (effect != null)
                    {
                        var tickContext = context.Clone();
                        tickContext.SetData("TickNumber", instance.PeriodicExecutionCount);
                        effect.Apply(tickContext, target);
                    }
                }
            }
        }

        /// <summary>
        /// ���� �ֱ� ���
        /// </summary>
        private float CalculateNextPeriod(int tickCount)
        {
            float nextPeriod = period;

            // ���� �ֱ�
            if (variablePeriod)
            {
                float variance = UnityEngine.Random.Range(-periodVariance, periodVariance);
                nextPeriod += nextPeriod * variance;
            }

            // ����
            if (accelerateOverTime && tickCount > 0)
            {
                nextPeriod *= Mathf.Pow(accelerationRate, tickCount);
            }

            return Mathf.Max(0.1f, nextPeriod);
        }

        /// <summary>
        /// ���� ƽ Ƚ�� ���
        /// </summary>
        private int CalculateExpectedTicks(EffectInstance instance)
        {
            if (maxTicks > 0) return maxTicks;

            int ticks = Mathf.FloorToInt(instance.RemainingDuration / period);
            if (executeOnFirstApplication) ticks++;
            if (executeOnLastTick && instance.RemainingDuration % period > 0.01f) ticks++;

            return ticks;
        }

        #endregion

        #region Private Methods - Effects

        /// <summary>
        /// ƽ ȿ�� ���
        /// </summary>
        private void PlayTickEffects(GameObject target)
        {
            // �ð� ȿ��
            if (tickVisualPrefab != null)
            {
                var vfx = Instantiate(tickVisualPrefab, target.transform.position, Quaternion.identity);
                vfx.transform.SetParent(target.transform);
                Destroy(vfx, tickVisualDuration);
            }

            // ����
            if (tickSound != null)
            {
                PlaySound(tickSound, target.transform.position);
            }
        }

        /// <summary>
        /// ƽ �̺�Ʈ �߻�
        /// </summary>
        private void TriggerTickEvent(EffectInstance instance, GameObject target, float magnitude)
        {
            Debug.Log($"[PeriodicEffect] Tick event: {effectName} - Tick #{instance.PeriodicExecutionCount}, Magnitude: {magnitude:F2}");

            // ������/�� ����
            bool isDamage = false;
            bool isHeal = false;

            foreach (var modifier in tickModifiers)
            {
                if (modifier.attributeType == AttributeType.Health)
                {
                    if (modifier.value < 0) isDamage = true;
                    else if (modifier.value > 0) isHeal = true;
                }
            }

            if (isDamage)
            {
                Debug.Log($"[PeriodicEffect] DOT tick: {effectName} - Damage: {magnitude:F2}");
            }
            else if (isHeal)
            {
                Debug.Log($"[PeriodicEffect] HOT tick: {effectName} - Healing: {magnitude:F2}");
            }
        }

        #endregion

        #region Override Stacking

        /// <summary>
        /// ���� ó�� (override)
        /// </summary>
        protected override bool HandleStacking(EffectInstance existingInstance, EffectContext context, GameObject target)
        {
            bool result = base.HandleStacking(existingInstance, context, target);

            if (result && resetPeriodicOnStack)
            {
                // Periodic Ÿ�̸� ����
                existingInstance.ResetPeriodicTimer();
                existingInstance.Context.SetData("PeriodicExecutionCount", 0);

                Debug.Log($"[PeriodicEffect] Stack applied, periodic timer reset for {effectName}");
            }

            return result;
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// DOT(Damage Over Time) Effect ����
        /// </summary>
        public static PeriodicEffect CreateDOT(string name, float damage, float duration, float period)
        {
            var effect = CreateInstance<PeriodicEffect>();
            effect.effectName = name;
            effect.effectId = Guid.NewGuid().ToString();
            effect.duration = duration;
            effect.period = period;
            effect.tickModifiers = new List<ModifierConfig>
            {
                new ModifierConfig(AttributeType.Health, ModifierOperation.Add, -damage, 0)
            };
            return effect;
        }

        /// <summary>
        /// HOT(Heal Over Time) Effect ����
        /// </summary>
        public static PeriodicEffect CreateHOT(string name, float healing, float duration, float period)
        {
            var effect = CreateInstance<PeriodicEffect>();
            effect.effectName = name;
            effect.effectId = Guid.NewGuid().ToString();
            effect.duration = duration;
            effect.period = period;
            effect.tickModifiers = new List<ModifierConfig>
            {
                new ModifierConfig(AttributeType.Health, ModifierOperation.Add, healing, 0)
            };
            return effect;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            // PeriodicEffect ���� ����
            effectType = EffectType.Periodic;

            // Period ����
            if (period <= 0f) period = 1f;

            // Duration�� period���� ª���� ����
            if (duration < period && duration > 0)
            {
                duration = period * 3; // �ּ� 3ƽ
            }

            // Max ticks ����
            if (maxTicks == 0) maxTicks = -1;

            // Acceleration ����
            if (accelerateOverTime)
            {
                accelerationRate = Mathf.Clamp(accelerationRate, 0.1f, 1f);
            }

            // Period variance ����
            periodVariance = Mathf.Clamp01(periodVariance);
        }
#endif

        #endregion
    }
}

// ���� ��ġ: Assets/Scripts/GAS/EffectSystem/Types/PeriodicEffect.cs