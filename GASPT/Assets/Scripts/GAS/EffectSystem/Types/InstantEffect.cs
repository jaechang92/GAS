using GAS.AttributeSystem;
using GAS.Core;
using GAS.TagSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.EffectSystem
{
    /// <summary>
    /// ��� ����ǰ� ����Ǵ� GameplayEffect
    /// ��: ��� ������, ��� ȸ��, ���� ���� ���� ��
    /// </summary>
    [CreateAssetMenu(fileName = "InstantEffect", menuName = "GAS/Effects/Instant Effect", order = 1)]
    public class InstantEffect : GameplayEffect
    {
        #region Additional Fields

        [Header("=== Instant Effect Settings ===")]
        [SerializeField] private bool applyModifiersPermanently = false;
        [SerializeField] private bool triggerGlobalEvents = true;
        [SerializeField] private bool allowCritical = false;
        [Range(0f, 1f)]
        [SerializeField] private float criticalChance = 0.1f;
        [SerializeField] private float criticalMultiplier = 2f;

        [Header("=== Value Calculation ===")]
        [SerializeField] private bool useAttributeScaling = false;
        [SerializeField] private AttributeType scalingAttribute;
        [SerializeField] private float scalingFactor = 1f;
        [SerializeField] private AnimationCurve scalingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("=== Conditional Effects ===")]
        [SerializeField] private List<ConditionalInstantEffect> conditionalEffects = new List<ConditionalInstantEffect>();

        #endregion

        #region Nested Types

        /// <summary>
        /// ���Ǻ� ��� ȿ��
        /// </summary>
        [Serializable]
        public class ConditionalInstantEffect
        {
            public TagRequirement condition;
            public InstantEffect effectToApply;
            [Range(0f, 1f)]
            public float applyChance = 1f;
        }

        #endregion

        #region Properties

        public bool ApplyModifiersPermanently => applyModifiersPermanently;
        public bool AllowCritical => allowCritical;
        public float CriticalChance => criticalChance;
        public float CriticalMultiplier => criticalMultiplier;

        #endregion

        #region Constructor

        public InstantEffect()
        {
            effectType = EffectType.Instant;
            durationPolicy = EffectDurationPolicy.Instant;
            duration = 0f;
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// ��� ȿ�� ����
        /// </summary>
        public override bool Apply(EffectContext context, GameObject target)
        {
            if (!CanApply(context, target))
            {
                Debug.LogWarning($"[InstantEffect] Cannot apply {effectName} to {target.name}");
                return false;
            }

            try
            {
                // ȿ�� ���� ���
                float finalMagnitude = CalculateFinalMagnitude(context, target);

                // Critical ����
                bool isCritical = false;
                if (allowCritical && UnityEngine.Random.value < criticalChance)
                {
                    finalMagnitude *= criticalMultiplier;
                    isCritical = true;

                    // Critical �±� �߰�
                    context.AddContextTag(new GameplayTag("Effect.Critical"));
                }

                // 1. Modifier ����
                ApplyModifiers(context, target, finalMagnitude);

                // 2. �±� �ο� (������)
                GrantTags(target);

                // 3. �ð�/û�� ȿ��
                PlayEffects(context, target, isCritical);

                // 4. ���Ǻ� ȿ�� ����
                ApplyConditionalEffects(context, target);

                // 5. �̺�Ʈ �߻�
                if (triggerGlobalEvents)
                {
                    TriggerEvents(context, target, finalMagnitude, isCritical);
                }

                // 6. �α�
                LogApplication(context, target, finalMagnitude, isCritical);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[InstantEffect] Failed to apply {effectName}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// InstantEffect�� ���� ������ ����
        /// </summary>
        public override bool Remove(EffectContext context, GameObject target)
        {
            Debug.LogWarning($"[InstantEffect] {effectName} is instant and cannot be removed");
            return false;
        }

        /// <summary>
        /// InstantEffect�� �ֱ��� ������ ����
        /// </summary>
        public override bool ExecutePeriodic(EffectContext context, GameObject target)
        {
            Debug.LogWarning($"[InstantEffect] {effectName} does not support periodic execution");
            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// ���� ȿ�� ���� ���
        /// </summary>
        private float CalculateFinalMagnitude(EffectContext context, GameObject target)
        {
            float magnitude = context.Magnitude;

            // Attribute scaling ����
            if (useAttributeScaling && target != null)
            {
                var attributeComponent = target.GetComponent<IAttributeComponent>();
                if (attributeComponent != null)
                {
                    float attributeValue = attributeComponent.GetAttributeValue(scalingAttribute);
                    float scaledValue = attributeValue * scalingFactor;

                    if (scalingCurve != null && scalingCurve.length > 0)
                    {
                        scaledValue = scalingCurve.Evaluate(scaledValue);
                    }

                    magnitude *= scaledValue;
                }
            }

            // Stack ��� scaling
            if (stackingPolicy == EffectStackingPolicy.Stack)
            {
                magnitude *= context.StackCount;
            }

            return magnitude;
        }

        /// <summary>
        /// Modifier ����
        /// </summary>
        private void ApplyModifiers(EffectContext context, GameObject target, float magnitude)
        {
            var attributeComponent = target.GetComponent<IAttributeComponent>();
            if (attributeComponent == null || modifiers.Count == 0) return;

            var createdModifiers = CreateModifiers(context, magnitude);

            foreach (var config in modifiers)
            {
                if (config == null) continue;

                // Modifier ����
                var modifier = new AttributeModifier(
                    config.operation,
                    config.value * magnitude,
                    config.priority
                );

                // ����
                if (applyModifiersPermanently)
                {
                    // ���� ���� - source�� null�� �����Ͽ� �������� ����
                    attributeComponent.AddModifier(config.attributeType, modifier, null);
                }
                else
                {
                    // �ӽ� ���� - ������ InstantEffect�̹Ƿ� �ٷ� ���� ����
                    ApplyInstantModifier(attributeComponent, config.attributeType, modifier);
                }

                // Modifier ���� �α� (�̺�Ʈ ���)
                Debug.Log($"[InstantEffect] Modifier applied: {config.attributeType} {modifier.Operation} {modifier.Value}");
            }
        }

        /// <summary>
        /// ��� Modifier ���� (Attribute �� ���� ����)
        /// </summary>
        private void ApplyInstantModifier(IAttributeComponent component, AttributeType type, AttributeModifier modifier)
        {
            float currentValue = component.GetAttributeValue(type);
            float newValue = currentValue;

            switch (modifier.Operation)
            {
                case ModifierOperation.Add:
                    newValue = currentValue + modifier.Value;
                    break;
                case ModifierOperation.Multiply:
                    newValue = currentValue * modifier.Value;
                    break;
                case ModifierOperation.Override:
                    newValue = modifier.Value;
                    break;
            }

            // BaseValue ���� ����
            if (component is AttributeSetComponent attrComponent)
            {
                var attributeSet = attrComponent.GetAttributeSet<AttributeSet>();
                BaseAttribute attribute = attributeSet.GetAttribute(type);
                if (attributeSet != null && attribute != null)
                {
                    attribute.BaseValue = newValue;

                    // ���� �̺�Ʈ ���� ȣ��
                    GASEvents.TriggerAttributeChanged(
                        attrComponent.gameObject,
                        type.ToString(),
                        currentValue,
                        newValue
                    );
                }
            }
        }

        /// <summary>
        /// ���Ǻ� ȿ�� ����
        /// </summary>
        private void ApplyConditionalEffects(EffectContext context, GameObject target)
        {
            if (conditionalEffects == null || conditionalEffects.Count == 0) return;

            var tagComponent = target.GetComponent<TagComponent>();

            foreach (var conditional in conditionalEffects)
            {
                if (conditional.effectToApply == null) continue;

                // ���� Ȯ��
                bool conditionMet = conditional.condition == null ||
                    (tagComponent != null && tagComponent.SatisfiesRequirement(conditional.condition));

                if (conditionMet)
                {
                    // Ȯ�� ����
                    if (UnityEngine.Random.value <= conditional.applyChance)
                    {
                        // ���Ǻ� ȿ�� ����
                        var newContext = context.Clone();
                        newContext.SetData("IsConditional", true);
                        conditional.effectToApply.Apply(newContext, target);
                    }
                }
            }
        }

        /// <summary>
        /// ȿ�� ��� (�ð�/û��)
        /// </summary>
        private void PlayEffects(EffectContext context, GameObject target, bool isCritical)
        {
            // �ð� ȿ��
            if (effectPrefab != null)
            {
                var vfx = CreateVisualEffect(target);
                if (vfx != null && isCritical)
                {
                    // Critical ȿ�� ��ȭ
                    vfx.transform.localScale *= 1.5f;
                }

                // ���� �ð� �� ���� (InstantEffect�̹Ƿ�)
                if (vfx != null)
                {
                    UnityEngine.Object.Destroy(vfx, 2f);
                }
            }

            // ���� ���
            if (applicationSound != null)
            {
                PlaySound(applicationSound, target.transform.position);
            }
        }

        /// <summary>
        /// �̺�Ʈ �߻�
        /// </summary>
        private void TriggerEvents(EffectContext context, GameObject target, float magnitude, bool isCritical)
        {
            // �⺻ Effect ���� �̺�Ʈ
            GASEvents.TriggerEffectApplied(target, effectName);

            // ������/ȸ�� ���� �α�
            bool isDamage = false;
            bool isHeal = false;

            foreach (var modifier in modifiers)
            {
                if (modifier.attributeType == AttributeType.Health)
                {
                    if (modifier.value < 0) isDamage = true;
                    else if (modifier.value > 0) isHeal = true;
                }
            }

            // Ư�� �α� ���
            if (isDamage)
            {
                Debug.Log($"[InstantEffect] Damage dealt: {effectName} - Magnitude: {magnitude}, Critical: {isCritical}");
            }
            else if (isHeal)
            {
                Debug.Log($"[InstantEffect] Healing done: {effectName} - Magnitude: {magnitude}");
            }

            if (isCritical)
            {
                Debug.Log($"[InstantEffect] CRITICAL HIT! {effectName} - Multiplier: {criticalMultiplier}x");
            }
        }

        /// <summary>
        /// ���� �α�
        /// </summary>
        private void LogApplication(EffectContext context, GameObject target, float magnitude, bool isCritical)
        {
            if (!Application.isEditor) return;

            string criticalText = isCritical ? " [CRITICAL]" : "";
            string log = $"[InstantEffect] Applied {effectName}{criticalText} to {target.name}\n" +
                        $"  Magnitude: {magnitude:F2}\n" +
                        $"  Instigator: {context.Instigator?.name ?? "None"}\n";

            if (modifiers.Count > 0)
            {
                log += "  Modifiers:\n";
                foreach (var mod in modifiers)
                {
                    float value = mod.value * magnitude;
                    log += $"    - {mod.attributeType}: {mod.operation} {value:F2}\n";
                }
            }

            Debug.Log(log);
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// ������ InstantEffect ���� ����
        /// </summary>
        public static InstantEffect CreateDamageEffect(float damage, string name = "Damage")
        {
            var effect = CreateInstance<InstantEffect>();
            effect.effectName = name;
            effect.effectId = Guid.NewGuid().ToString();
            effect.modifiers = new List<ModifierConfig>
            {
                new ModifierConfig(AttributeType.Health, ModifierOperation.Add, -damage, 0)
            };
            return effect;
        }

        /// <summary>
        /// ȸ�� InstantEffect ���� ����
        /// </summary>
        public static InstantEffect CreateHealEffect(float healing, string name = "Heal")
        {
            var effect = CreateInstance<InstantEffect>();
            effect.effectName = name;
            effect.effectId = Guid.NewGuid().ToString();
            effect.modifiers = new List<ModifierConfig>
            {
                new ModifierConfig(AttributeType.Health, ModifierOperation.Add, healing, 0)
            };
            return effect;
        }

        /// <summary>
        /// ���� �ν�Ʈ InstantEffect ���� ����
        /// </summary>
        public static InstantEffect CreateStatBoostEffect(AttributeType stat, float amount, bool permanent = false)
        {
            var effect = CreateInstance<InstantEffect>();
            effect.effectName = $"{stat} Boost";
            effect.effectId = Guid.NewGuid().ToString();
            effect.applyModifiersPermanently = permanent;
            effect.modifiers = new List<ModifierConfig>
            {
                new ModifierConfig(stat, ModifierOperation.Add, amount, 0)
            };
            return effect;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            // InstantEffect ���� ����
            effectType = EffectType.Instant;
            durationPolicy = EffectDurationPolicy.Instant;
            duration = 0f;
            period = 0f;

            // Critical ���� ����
            if (allowCritical)
            {
                criticalChance = Mathf.Clamp01(criticalChance);
                criticalMultiplier = Mathf.Max(1f, criticalMultiplier);
            }

            // Scaling ����
            if (useAttributeScaling)
            {
                scalingFactor = Mathf.Max(0f, scalingFactor);
            }
        }
#endif

        #endregion
    }
}

// ���� ��ġ: Assets/Scripts/GAS/EffectSystem/Types/InstantEffect.cs