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
    /// 즉시 적용되고 종료되는 GameplayEffect
    /// 예: 즉시 데미지, 즉시 회복, 스탯 영구 증가 등
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
        /// 조건부 즉시 효과
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
        /// 즉시 효과 적용
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
                // 효과 강도 계산
                float finalMagnitude = CalculateFinalMagnitude(context, target);

                // Critical 판정
                bool isCritical = false;
                if (allowCritical && UnityEngine.Random.value < criticalChance)
                {
                    finalMagnitude *= criticalMultiplier;
                    isCritical = true;

                    // Critical 태그 추가
                    context.AddContextTag(new GameplayTag("Effect.Critical"));
                }

                // 1. Modifier 적용
                ApplyModifiers(context, target, finalMagnitude);

                // 2. 태그 부여 (영구적)
                GrantTags(target);

                // 3. 시각/청각 효과
                PlayEffects(context, target, isCritical);

                // 4. 조건부 효과 적용
                ApplyConditionalEffects(context, target);

                // 5. 이벤트 발생
                if (triggerGlobalEvents)
                {
                    TriggerEvents(context, target, finalMagnitude, isCritical);
                }

                // 6. 로깅
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
        /// InstantEffect는 제거 개념이 없음
        /// </summary>
        public override bool Remove(EffectContext context, GameObject target)
        {
            Debug.LogWarning($"[InstantEffect] {effectName} is instant and cannot be removed");
            return false;
        }

        /// <summary>
        /// InstantEffect는 주기적 실행이 없음
        /// </summary>
        public override bool ExecutePeriodic(EffectContext context, GameObject target)
        {
            Debug.LogWarning($"[InstantEffect] {effectName} does not support periodic execution");
            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 최종 효과 강도 계산
        /// </summary>
        private float CalculateFinalMagnitude(EffectContext context, GameObject target)
        {
            float magnitude = context.Magnitude;

            // Attribute scaling 적용
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

            // Stack 기반 scaling
            if (stackingPolicy == EffectStackingPolicy.Stack)
            {
                magnitude *= context.StackCount;
            }

            return magnitude;
        }

        /// <summary>
        /// Modifier 적용
        /// </summary>
        private void ApplyModifiers(EffectContext context, GameObject target, float magnitude)
        {
            var attributeComponent = target.GetComponent<IAttributeComponent>();
            if (attributeComponent == null || modifiers.Count == 0) return;

            var createdModifiers = CreateModifiers(context, magnitude);

            foreach (var config in modifiers)
            {
                if (config == null) continue;

                // Modifier 생성
                var modifier = new AttributeModifier(
                    config.operation,
                    config.value * magnitude,
                    config.priority
                );

                // 적용
                if (applyModifiersPermanently)
                {
                    // 영구 적용 - source를 null로 설정하여 추적하지 않음
                    attributeComponent.AddModifier(config.attributeType, modifier, null);
                }
                else
                {
                    // 임시 적용 - 하지만 InstantEffect이므로 바로 값만 변경
                    ApplyInstantModifier(attributeComponent, config.attributeType, modifier);
                }

                // Modifier 적용 로그 (이벤트 대신)
                Debug.Log($"[InstantEffect] Modifier applied: {config.attributeType} {modifier.Operation} {modifier.Value}");
            }
        }

        /// <summary>
        /// 즉시 Modifier 적용 (Attribute 값 직접 변경)
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

            // BaseValue 직접 변경
            if (component is AttributeSetComponent attrComponent)
            {
                var attributeSet = attrComponent.GetAttributeSet<AttributeSet>();
                BaseAttribute attribute = attributeSet.GetAttribute(type);
                if (attributeSet != null && attribute != null)
                {
                    attribute.BaseValue = newValue;

                    // 변경 이벤트 수동 호출
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
        /// 조건부 효과 적용
        /// </summary>
        private void ApplyConditionalEffects(EffectContext context, GameObject target)
        {
            if (conditionalEffects == null || conditionalEffects.Count == 0) return;

            var tagComponent = target.GetComponent<TagComponent>();

            foreach (var conditional in conditionalEffects)
            {
                if (conditional.effectToApply == null) continue;

                // 조건 확인
                bool conditionMet = conditional.condition == null ||
                    (tagComponent != null && tagComponent.SatisfiesRequirement(conditional.condition));

                if (conditionMet)
                {
                    // 확률 판정
                    if (UnityEngine.Random.value <= conditional.applyChance)
                    {
                        // 조건부 효과 적용
                        var newContext = context.Clone();
                        newContext.SetData("IsConditional", true);
                        conditional.effectToApply.Apply(newContext, target);
                    }
                }
            }
        }

        /// <summary>
        /// 효과 재생 (시각/청각)
        /// </summary>
        private void PlayEffects(EffectContext context, GameObject target, bool isCritical)
        {
            // 시각 효과
            if (effectPrefab != null)
            {
                var vfx = CreateVisualEffect(target);
                if (vfx != null && isCritical)
                {
                    // Critical 효과 강화
                    vfx.transform.localScale *= 1.5f;
                }

                // 일정 시간 후 제거 (InstantEffect이므로)
                if (vfx != null)
                {
                    UnityEngine.Object.Destroy(vfx, 2f);
                }
            }

            // 사운드 재생
            if (applicationSound != null)
            {
                PlaySound(applicationSound, target.transform.position);
            }
        }

        /// <summary>
        /// 이벤트 발생
        /// </summary>
        private void TriggerEvents(EffectContext context, GameObject target, float magnitude, bool isCritical)
        {
            // 기본 Effect 적용 이벤트
            GASEvents.TriggerEffectApplied(target, effectName);

            // 데미지/회복 판정 로그
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

            // 특수 로그 출력
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
        /// 적용 로그
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
        /// 데미지 InstantEffect 생성 헬퍼
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
        /// 회복 InstantEffect 생성 헬퍼
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
        /// 스탯 부스트 InstantEffect 생성 헬퍼
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

            // InstantEffect 강제 설정
            effectType = EffectType.Instant;
            durationPolicy = EffectDurationPolicy.Instant;
            duration = 0f;
            period = 0f;

            // Critical 설정 검증
            if (allowCritical)
            {
                criticalChance = Mathf.Clamp01(criticalChance);
                criticalMultiplier = Mathf.Max(1f, criticalMultiplier);
            }

            // Scaling 검증
            if (useAttributeScaling)
            {
                scalingFactor = Mathf.Max(0f, scalingFactor);
            }
        }
#endif

        #endregion
    }
}

// 파일 위치: Assets/Scripts/GAS/EffectSystem/Types/InstantEffect.cs