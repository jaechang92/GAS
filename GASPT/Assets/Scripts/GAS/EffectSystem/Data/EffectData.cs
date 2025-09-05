// ================================
// File: Assets/Scripts/GAS/EffectSystem/Data/EffectData.cs
// ================================
using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.TagSystem;
using GAS.AttributeSystem;
using static GAS.Core.GASConstants;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Serializable data container for effect configuration
    /// </summary>
    [Serializable]
    public class EffectData
    {
        [Header("Basic Information")]
        public string effectId;
        public string effectName;
        public string description;
        public Sprite icon;

        [Header("Effect Classification")]
        public EffectType effectType = EffectType.Instant;
        public DurationPolicy durationPolicy = DurationPolicy.Instant;
        public List<GameplayTag> effectTags = new List<GameplayTag>();

        [Header("Duration & Period")]
        public float baseDuration = 0f;
        public float basePeriod = 1f;
        public bool scaleWithLevel = false;
        public float durationPerLevel = 0f;
        public float periodPerLevel = 0f;

        [Header("Stacking")]
        public StackingPolicy stackingPolicy = StackingPolicy.None;
        public int maxStackCount = 1;
        public StackBehavior stackBehavior = StackBehavior.RefreshDuration;
        public float stackMagnitudeMultiplier = 1f;

        [Header("Modifiers")]
        public List<EffectModifierData> modifiers = new List<EffectModifierData>();

        [Header("Conditional Modifiers")]
        public List<ConditionalModifierData> conditionalModifiers = new List<ConditionalModifierData>();

        [Header("Tags")]
        public List<GameplayTag> grantedTags = new List<GameplayTag>();
        public List<GameplayTag> requiredTags = new List<GameplayTag>();
        public List<GameplayTag> blockedTags = new List<GameplayTag>();
        public List<GameplayTag> removalTags = new List<GameplayTag>();

        [Header("Visual & Audio")]
        public GameObject effectPrefab;
        public GameObject impactPrefab;
        public AudioClip applicationSound;
        public AudioClip periodicSound;
        public AudioClip removalSound;
        public Color effectColor = Color.white;

        [Header("UI Display")]
        public bool showInUI = true;
        public int displayPriority = 0;
        public bool showStacks = true;
        public bool showDuration = true;
        public string displayFormat = "{name} ({stacks}x) - {duration}s";

        /// <summary>
        /// Creates a runtime effect instance from this data
        /// </summary>
        public virtual EffectSpec CreateSpec(EffectContext context)
        {
            var spec = new EffectSpec
            {
                data = this,
                level = context?.level ?? 1,
                magnitude = context?.magnitude ?? 1f,
                source = context?.instigator,
                appliedTime = Time.time
            };

            // Calculate actual duration
            spec.duration = CalculateDuration(spec.level);
            spec.period = CalculatePeriod(spec.level);

            return spec;
        }

        /// <summary>
        /// Calculates actual duration for a level
        /// </summary>
        public float CalculateDuration(int level)
        {
            if (durationPolicy == DurationPolicy.Instant)
                return 0f;
            if (durationPolicy == DurationPolicy.Infinite)
                return -1f;

            float duration = baseDuration;
            if (scaleWithLevel && level > 1)
            {
                duration += durationPerLevel * (level - 1);
            }
            return duration;
        }

        /// <summary>
        /// Calculates actual period for a level
        /// </summary>
        public float CalculatePeriod(int level)
        {
            float period = basePeriod;
            if (scaleWithLevel && level > 1 && periodPerLevel != 0)
            {
                period += periodPerLevel * (level - 1);
            }
            return Mathf.Max(0.1f, period);
        }

        /// <summary>
        /// Checks if requirements are met
        /// </summary>
        public bool CheckRequirements(TagComponent targetTags)
        {
            if (targetTags == null)
                return requiredTags.Count == 0;

            // Check required tags
            foreach (var tag in requiredTags)
            {
                if (!targetTags.HasTag(tag))
                    return false;
            }

            // Check blocked tags
            foreach (var tag in blockedTags)
            {
                if (targetTags.HasTag(tag))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if effect should be removed
        /// </summary>
        public bool CheckRemovalConditions(TagComponent targetTags)
        {
            if (targetTags == null || removalTags.Count == 0)
                return false;

            foreach (var tag in removalTags)
            {
                if (targetTags.HasTag(tag))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets display information for UI
        /// </summary>
        public EffectDisplayInfo GetDisplayInfo(EffectSpec spec)
        {
            return new EffectDisplayInfo
            {
                name = effectName,
                description = description,
                icon = icon,
                color = effectColor,
                duration = spec.GetRemainingDuration(),
                stacks = spec.stackCount,
                showInUI = showInUI,
                priority = displayPriority,
                formattedText = FormatDisplayText(spec)
            };
        }

        /// <summary>
        /// Formats display text for UI
        /// </summary>
        private string FormatDisplayText(EffectSpec spec)
        {
            string text = displayFormat;
            text = text.Replace("{name}", effectName);
            text = text.Replace("{stacks}", spec.stackCount.ToString());
            text = text.Replace("{duration}", spec.GetRemainingDuration().ToString("F1"));
            text = text.Replace("{level}", spec.level.ToString());
            return text;
        }
    }

    /// <summary>
    /// Data for effect modifiers
    /// </summary>
    [Serializable]
    public class EffectModifierData
    {
        public AttributeType attributeType;
        public ModifierOperation operation = ModifierOperation.Add;
        public float baseValue;
        public float valuePerLevel;
        public float valuePerStack;
        public AnimationCurve scalingCurve;
        public bool isPermanent = false;

        public float CalculateValue(int level, int stacks = 1)
        {
            float value = baseValue;
            value += valuePerLevel * (level - 1);
            value += valuePerStack * (stacks - 1);

            if (scalingCurve != null && scalingCurve.keys.Length > 0)
            {
                value *= scalingCurve.Evaluate(level);
            }

            return value;
        }
    }

    /// <summary>
    /// Conditional modifier data
    /// </summary>
    [Serializable]
    public class ConditionalModifierData
    {
        public EffectConditionType conditionType;
        public float threshold;
        public GameplayTag requiredTag;
        public AttributeType checkAttribute;
        public ComparisonOperator comparisonOperator;
        public EffectModifierData modifier;

        public bool CheckCondition(GameObject target)
        {
            switch (conditionType)
            {
                case EffectConditionType.HasTag:
                    var tagComponent = target.GetComponent<TagComponent>();
                    return tagComponent != null && tagComponent.HasTag(requiredTag);

                case EffectConditionType.AttributeThreshold:
                    var attributes = target.GetComponent<AttributeSetComponent>();
                    if (attributes == null) return false;
                    float value = attributes.GetAttributeValue(checkAttribute);
                    return CompareValue(value, threshold, comparisonOperator);

                case EffectConditionType.Random:
                    return UnityEngine.Random.Range(0f, 100f) < threshold;

                case EffectConditionType.HealthPercentage:
                    var healthAttrs = target.GetComponent<AttributeSetComponent>();
                    if (healthAttrs == null) return false;
                    float healthPercent = healthAttrs.GetAttributeValue(AttributeType.HealthPercent);
                    return CompareValue(healthPercent, threshold, comparisonOperator);

                default:
                    return false;
            }
        }

        private bool CompareValue(float value, float threshold, ComparisonOperator op)
        {
            switch (op)
            {
                case ComparisonOperator.Equal:
                    return Mathf.Approximately(value, threshold);
                case ComparisonOperator.NotEqual:
                    return !Mathf.Approximately(value, threshold);
                case ComparisonOperator.Greater:
                    return value > threshold;
                case ComparisonOperator.GreaterOrEqual:
                    return value >= threshold;
                case ComparisonOperator.Less:
                    return value < threshold;
                case ComparisonOperator.LessOrEqual:
                    return value <= threshold;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Runtime specification of an effect
    /// </summary>
    [Serializable]
    public class EffectSpec
    {
        public EffectData data;
        public int level;
        public float magnitude;
        public float duration;
        public float period;
        public int stackCount = 1;
        public float appliedTime;
        public float lastPeriodicTime;
        public GameObject source;
        public object customData;

        public float GetRemainingDuration()
        {
            if (duration < 0) return -1f;
            float elapsed = Time.time - appliedTime;
            return Mathf.Max(0, duration - elapsed);
        }

        public float GetProgress()
        {
            if (duration <= 0) return 0f;
            float elapsed = Time.time - appliedTime;
            return Mathf.Clamp01(elapsed / duration);
        }

        public bool IsExpired()
        {
            if (duration < 0) return false;
            return Time.time - appliedTime >= duration;
        }

        public bool IsPeriodicReady()
        {
            return Time.time - lastPeriodicTime >= period;
        }

        public void UpdatePeriodicTime()
        {
            lastPeriodicTime = Time.time;
        }

        public void RefreshDuration()
        {
            appliedTime = Time.time;
        }

        public void AddStacks(int amount)
        {
            stackCount = Mathf.Min(stackCount + amount, data.maxStackCount);
        }
    }

    /// <summary>
    /// UI display information for effects
    /// </summary>
    [Serializable]
    public class EffectDisplayInfo
    {
        public string name;
        public string description;
        public Sprite icon;
        public Color color;
        public float duration;
        public int stacks;
        public bool showInUI;
        public int priority;
        public string formattedText;
    }

    /// <summary>
    /// Stack behavior options
    /// </summary>
    public enum StackBehavior
    {
        RefreshDuration,
        AddDuration,
        Independent,
        Replace
    }

    /// <summary>
    /// Effect condition types
    /// </summary>
    public enum EffectConditionType
    {
        None,
        HasTag,
        MissingTag,
        AttributeThreshold,
        HealthPercentage,
        ManaPercentage,
        Random,
        TimeElapsed,
        StackCount
    }

    /// <summary>
    /// Comparison operators for conditions
    /// </summary>
    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual
    }
}