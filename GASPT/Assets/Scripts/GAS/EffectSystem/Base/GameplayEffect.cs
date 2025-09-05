// ================================
// File: Assets/Scripts/GAS/EffectSystem/Base/GameplayEffect.cs
// Complete implementation with Clone method and modification methods
// ================================
using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.TagSystem;
using GAS.AttributeSystem;
using static GAS.Core.GASConstants;
using System.Linq;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Base class for all gameplay effects
    /// </summary>
    public abstract class GameplayEffect : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] protected string effectId;
        [SerializeField] protected string effectName;
        [SerializeField] protected string description;
        [SerializeField] protected Sprite icon;
        [SerializeField] protected Color effectColor = Color.white;

        [Header("Duration Settings")]
        [SerializeField] protected EffectType effectType = EffectType.Instant;
        [SerializeField] protected DurationPolicy durationPolicy = DurationPolicy.Instant;
        [SerializeField] protected float duration = 0f;
        [SerializeField] protected float period = 1f;
        [SerializeField] protected bool executePeriodicOnApplication = true;

        [Header("Stacking")]
        [SerializeField] protected StackingPolicy stackingPolicy = StackingPolicy.None;
        [SerializeField] protected int maxStackCount = 1;
        [SerializeField] protected bool refreshDurationOnStack = true;
        [SerializeField] protected bool resetPeriodicOnStack = false;

        [Header("Tag Requirements")]
        [SerializeField] protected TagRequirement applicationRequirement;
        [SerializeField] protected TagRequirement ongoingRequirement;
        [SerializeField] protected TagRequirement removalRequirement;

        [Header("Tags")]
        [SerializeField] protected TagContainer grantedTags;
        [SerializeField] protected TagContainer assetTags;

        [Header("Modifiers")]
        [SerializeField] protected List<AttributeModifier> modifiers = new List<AttributeModifier>();

        [Header("Visual & Audio")]
        [SerializeField] protected GameObject effectPrefab;
        [SerializeField] protected AudioClip applicationSound;
        [SerializeField] protected AudioClip removalSound;

        // Properties
        public string EffectId => effectId;
        public string EffectName => effectName;
        public string Description => description;
        public Sprite Icon => icon;
        public EffectType EffectType => effectType;
        public DurationPolicy DurationPolicy => durationPolicy;
        public float Duration => duration;
        public float Period => period;
        public StackingPolicy StackingPolicy => stackingPolicy;
        public int MaxStackCount => maxStackCount;
        public TagContainer GrantedTags => grantedTags;
        public TagContainer AssetTags => assetTags;
        public List<AttributeModifier> Modifiers => modifiers;
        public bool RefreshDurationOnStack => refreshDurationOnStack;

        #region Clone and Modification Methods

        /// <summary>
        /// Creates a deep copy of this effect
        /// </summary>
        public virtual GameplayEffect Clone()
        {
            // Create instance of the same type
            var clone = CreateInstance(GetType()) as GameplayEffect;

            // Copy basic fields
            clone.effectId = effectId;
            clone.effectName = effectName;
            clone.description = description;
            clone.icon = icon;

            // Copy duration settings
            clone.effectType = effectType;
            clone.durationPolicy = durationPolicy;
            clone.duration = duration;
            clone.period = period;
            clone.executePeriodicOnApplication = executePeriodicOnApplication;

            // Copy stacking settings
            clone.stackingPolicy = stackingPolicy;
            clone.maxStackCount = maxStackCount;
            clone.refreshDurationOnStack = refreshDurationOnStack;
            clone.resetPeriodicOnStack = resetPeriodicOnStack;

            // Deep copy tag requirements
            if (applicationRequirement != null)
                clone.applicationRequirement = applicationRequirement.Clone();
            if (ongoingRequirement != null)
                clone.ongoingRequirement = ongoingRequirement.Clone();
            if (removalRequirement != null)
                clone.removalRequirement = removalRequirement.Clone();

            // Deep copy tag containers
            if (grantedTags != null)
                clone.grantedTags = CloneTagContainer(grantedTags);
            if (assetTags != null)
                clone.assetTags = CloneTagContainer(assetTags);

            // Deep copy modifiers
            clone.modifiers = new List<AttributeModifier>();
            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    if (modifier != null)
                        clone.modifiers.Add(modifier.Clone());
                }
            }

            // Copy visual/audio references (these are references, not deep copies)
            clone.effectPrefab = effectPrefab;
            clone.applicationSound = applicationSound;
            clone.removalSound = removalSound;

            // Call virtual method for derived class specific cloning
            OnClone(clone);

            return clone;
        }

        /// <summary>
        /// Override in derived classes to copy specific fields
        /// </summary>
        protected virtual void OnClone(GameplayEffect clone)
        {
            // Override in derived classes for specific field copying
        }

        /// <summary>
        /// Helper method to clone TagContainer
        /// </summary>
        private TagContainer CloneTagContainer(TagContainer original)
        {
            // Use TagContainer's Clone method
            return original.Clone();
        }

        #endregion

        #region Preset Modification Methods

        /// <summary>
        /// Sets the effect duration
        /// </summary>
        public void SetDuration(float value)
        {
            duration = Mathf.Max(0f, value);
        }

        /// <summary>
        /// Modifies the effect duration by a delta
        /// </summary>
        public void ModifyDuration(float delta)
        {
            duration = Mathf.Max(0f, duration + delta);
        }

        /// <summary>
        /// Sets the effect period for periodic effects
        /// </summary>
        public void SetPeriod(float value)
        {
            period = Mathf.Max(0.1f, value);
        }

        /// <summary>
        /// Modifies the effect period by a delta
        /// </summary>
        public void ModifyPeriod(float delta)
        {
            period = Mathf.Max(0.1f, period + delta);
        }

        /// <summary>
        /// Sets the maximum stack count
        /// </summary>
        public void SetMaxStackCount(int value)
        {
            maxStackCount = Mathf.Max(1, value);
        }

        /// <summary>
        /// Modifies the maximum stack count by a delta
        /// </summary>
        public void ModifyMaxStackCount(int delta)
        {
            maxStackCount = Mathf.Max(1, maxStackCount + delta);
        }

        /// <summary>
        /// Modifies all modifier magnitudes by a multiplier
        /// </summary>
        public void ModifyModifierMagnitude(float multiplier)
        {
            if (modifiers == null) return;

            foreach (var modifier in modifiers)
            {
                if (modifier != null)
                {
                    modifier.value *= multiplier;
                }
            }
        }

        /// <summary>
        /// Adds modifier magnitude by a flat value
        /// </summary>
        public void AddModifierMagnitude(float value)
        {
            if (modifiers == null) return;

            foreach (var modifier in modifiers)
            {
                if (modifier != null)
                {
                    modifier.value += value;
                }
            }
        }

        /// <summary>
        /// Sets the effect name
        /// </summary>
        public void SetEffectName(string name)
        {
            effectName = name;
        }

        /// <summary>
        /// Sets the effect description
        /// </summary>
        public void SetDescription(string desc)
        {
            description = desc;
        }

        /// <summary>
        /// Sets the effect icon
        /// </summary>
        public void SetIcon(Sprite newIcon)
        {
            icon = newIcon;
        }

        /// <summary>
        /// Sets the stacking policy
        /// </summary>
        public void SetStackingPolicy(StackingPolicy policy)
        {
            stackingPolicy = policy;
        }

        /// <summary>
        /// Adds a new modifier to the effect
        /// </summary>
        public void AddModifier(AttributeModifier modifier)
        {
            if (modifiers == null)
                modifiers = new List<AttributeModifier>();

            if (modifier != null)
                modifiers.Add(modifier);
        }

        /// <summary>
        /// Removes all modifiers of a specific attribute type
        /// </summary>
        public void RemoveModifiersByType(AttributeType type)
        {
            if (modifiers == null) return;

            modifiers.RemoveAll(m => m.targetAttributeType == type);
        }

        /// <summary>
        /// Clears all modifiers
        /// </summary>
        public void ClearModifiers()
        {
            modifiers?.Clear();
        }

        #endregion

        #region Original Abstract Methods

        /// <summary>
        /// Checks if the effect can be applied
        /// </summary>
        public virtual bool CanApply(EffectContext context)
        {
            if (context == null || context.target == null)
                return false;

            // Check application requirements
            if (applicationRequirement != null && !applicationRequirement.IgnoreIfEmpty)
            {
                var tagComponent = context.target.GetComponent<TagComponent>();
                if (tagComponent == null)
                    return false;

                if (!applicationRequirement.CheckRequirement(tagComponent))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Called when the effect is applied
        /// </summary>
        public abstract void OnApply(EffectContext context);

        /// <summary>
        /// Called when the effect is removed
        /// </summary>
        public abstract void OnRemove(EffectContext context);

        /// <summary>
        /// Called periodically for periodic effects
        /// </summary>
        public virtual void OnPeriodic(EffectContext context)
        {
            // Override in derived classes for periodic behavior
        }

        /// <summary>
        /// Called each frame for duration-based effects
        /// </summary>
        public virtual void OnTick(EffectContext context, float deltaTime)
        {
            // Override in derived classes for per-frame updates
        }

        /// <summary>
        /// Called when the effect stacks
        /// </summary>
        public virtual void OnStack(EffectContext context, int newStackCount, int previousStackCount)
        {
            // Override in derived classes for stacking behavior
        }

        /// <summary>
        /// Checks if the effect should continue
        /// </summary>
        public virtual bool CheckOngoing(EffectContext context)
        {
            if (ongoingRequirement == null || ongoingRequirement.IgnoreIfEmpty)
                return true;

            var tagComponent = context.target?.GetComponent<TagComponent>();
            if (tagComponent == null)
                return false;

            return ongoingRequirement.CheckRequirement(tagComponent);
        }

        /// <summary>
        /// Checks if the effect should be removed
        /// </summary>
        public virtual bool CheckRemoval(EffectContext context)
        {
            if (removalRequirement == null || removalRequirement.IgnoreIfEmpty)
                return false;

            var tagComponent = context.target?.GetComponent<TagComponent>();
            if (tagComponent == null)
                return false;

            return removalRequirement.CheckRequirement(tagComponent);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Applies attribute modifiers to the target
        /// </summary>
        protected virtual void ApplyModifiers(EffectContext context)
        {
            if (modifiers == null || modifiers.Count == 0)
                return;

            var attributeComponent = context.target?.GetComponent<AttributeSetComponent>();
            if (attributeComponent == null)
                return;

            foreach (var modifier in modifiers)
            {
                if (modifier != null)
                {
                    var modifierCopy = modifier.Clone();
                    modifierCopy.source = this;
                    modifierCopy.sourceName = effectName;
                    attributeComponent.AddModifier(modifier.targetAttributeType, modifierCopy);
                }
            }
        }

        /// <summary>
        /// Removes attribute modifiers from the target
        /// </summary>
        protected virtual void RemoveModifiers(EffectContext context)
        {
            if (modifiers == null || modifiers.Count == 0)
                return;

            var attributeComponent = context.target?.GetComponent<AttributeSetComponent>();
            if (attributeComponent == null)
                return;

            attributeComponent.RemoveAllModifiersFromSource(this);
        }

        /// <summary>
        /// Applies granted tags to the target
        /// </summary>
        protected virtual void ApplyGrantedTags(EffectContext context)
        {
            if (grantedTags == null || grantedTags.Tags.Count == 0)
                return;

            var tagComponent = context.target?.GetComponent<TagComponent>();
            if (tagComponent == null)
                return;

            foreach (var tag in grantedTags.Tags)
            {
                if (tag != null)
                {
                    tagComponent.AddTag(tag, this);
                }
            }
        }

        /// <summary>
        /// Removes granted tags from the target
        /// </summary>
        protected virtual void RemoveGrantedTags(EffectContext context)
        {
            if (grantedTags == null || grantedTags.Tags.Count == 0)
                return;

            var tagComponent = context.target?.GetComponent<TagComponent>();
            if (tagComponent == null)
                return;

            foreach (var tag in grantedTags.Tags)
            {
                if (tag != null)
                {
                    tagComponent.RemoveTag(tag, this);
                }
            }
        }

        /// <summary>
        /// Spawns visual effects
        /// </summary>
        protected virtual GameObject SpawnVisualEffect(EffectContext context)
        {
            if (effectPrefab == null)
                return null;

            var position = context.effectLocation != Vector3.zero ?
                context.effectLocation : context.target.transform.position;

            var effect = Instantiate(effectPrefab, position, Quaternion.identity);

            // Parent to target if it's a persistent effect
            if (durationPolicy != DurationPolicy.Instant && context.target != null)
            {
                effect.transform.SetParent(context.target.transform);
                effect.transform.localPosition = Vector3.zero;
            }

            return effect;
        }

        /// <summary>
        /// Plays application sound
        /// </summary>
        protected virtual void PlayApplicationSound(EffectContext context)
        {
            if (applicationSound == null)
                return;

            var position = context.target != null ?
                context.target.transform.position : context.effectLocation;

            AudioSource.PlayClipAtPoint(applicationSound, position);
        }

        /// <summary>
        /// Plays removal sound
        /// </summary>
        protected virtual void PlayRemovalSound(EffectContext context)
        {
            if (removalSound == null)
                return;

            var position = context.target != null ?
                context.target.transform.position : context.effectLocation;

            AudioSource.PlayClipAtPoint(removalSound, position);
        }

        /// <summary>
        /// Creates an effect instance for runtime tracking
        /// </summary>
        public virtual EffectInstance CreateInstance(EffectContext context)
        {
            var instance = new EffectInstance
            {
                effect = this,
                context = context,
                startTime = Time.time,
                lastPeriodicTime = Time.time,
                stackCount = 1,
                instanceId = Guid.NewGuid()
            };

            // Calculate actual duration based on context
            if (durationPolicy == DurationPolicy.HasDuration)
            {
                instance.duration = CalculateDuration(context);
            }
            else if (durationPolicy == DurationPolicy.Infinite)
            {
                instance.duration = -1f;
            }

            return instance;
        }

        /// <summary>
        /// Calculates the actual duration based on context
        /// </summary>
        protected virtual float CalculateDuration(EffectContext context)
        {
            float actualDuration = duration;

            // Apply duration modifiers from context
            if (context.durationMultiplier != 1f)
            {
                actualDuration *= context.durationMultiplier;
            }

            return Mathf.Max(0.1f, actualDuration);
        }

        /// <summary>
        /// Calculates the actual period based on context
        /// </summary>
        protected virtual float CalculatePeriod(EffectContext context)
        {
            float actualPeriod = period;

            // Apply period modifiers from context
            if (context.periodMultiplier != 1f)
            {
                actualPeriod *= context.periodMultiplier;
            }

            return Mathf.Max(0.1f, actualPeriod);
        }

        /// <summary>
        /// Calculates effect magnitude based on context and level
        /// </summary>
        protected virtual float CalculateEffectMagnitude()
        {
            // Base implementation - can be overridden in derived classes
            return 1f;
        }

        /// <summary>
        /// Checks if this effect conflicts with another
        /// </summary>
        public virtual bool ConflictsWith(GameplayEffect other)
        {
            if (other == null)
                return false;

            // Same effect always conflicts based on stacking policy
            if (other == this)
            {
                return stackingPolicy == StackingPolicy.None;
            }

            // Check if effects share exclusive tags
            if (assetTags != null && other.assetTags != null)
            {
                foreach (var tag in assetTags.Tags)
                {
                    if (tag != null && tag.TagName.Contains("Exclusive"))
                    {
                        foreach (var otherTag in other.assetTags.Tags)
                        {
                            if (otherTag != null && otherTag.TagName == tag.TagName)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the display priority for UI
        /// </summary>
        public virtual int GetDisplayPriority()
        {
            // Instant effects don't display
            if (durationPolicy == DurationPolicy.Instant)
                return -1;

            // Debuffs show before buffs
            bool isDebuff = assetTags != null &&
                assetTags.Tags.Any(t => t != null && t.TagName.Contains("Debuff"));

            return isDebuff ? 100 : 50;
        }

        /// <summary>
        /// Gets the effect color for UI
        /// </summary>
        public virtual Color GetEffectColor()
        {
            // Red for debuffs
            if (assetTags != null && assetTags.Tags.Any(t => t != null && t.TagName.Contains("Debuff")))
                return Color.red;

            // Green for buffs
            if (assetTags != null && assetTags.Tags.Any(t => t != null && t.TagName.Contains("Buff")))
                return Color.green;

            // Default white
            return Color.white;
        }

        /// <summary>
        /// Validates the effect configuration
        /// </summary>
        public virtual bool ValidateConfiguration()
        {
            bool isValid = true;

            // Validate basic info
            if (string.IsNullOrEmpty(effectName))
            {
                Debug.LogWarning($"Effect has no name");
                isValid = false;
            }

            // Validate duration settings
            if (durationPolicy == DurationPolicy.HasDuration && duration <= 0)
            {
                Debug.LogWarning($"Effect {effectName} has duration policy but no duration set");
                isValid = false;
            }

            // Validate periodic settings
            if (effectType == EffectType.Periodic && period <= 0)
            {
                Debug.LogWarning($"Periodic effect {effectName} has invalid period");
                isValid = false;
            }

            // Validate stacking
            if (stackingPolicy == StackingPolicy.Stack && maxStackCount <= 1)
            {
                Debug.LogWarning($"Effect {effectName} has stack policy but max stack count is {maxStackCount}");
                isValid = false;
            }

            return isValid;
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        /// <summary>
        /// Called when values are changed in the inspector
        /// </summary>
        protected virtual void OnValidate()
        {
            // Ensure valid ranges
            duration = Mathf.Max(0f, duration);
            period = Mathf.Max(0.1f, period);
            maxStackCount = Mathf.Max(1, maxStackCount);

            // Auto-set effect type based on configuration
            if (durationPolicy == DurationPolicy.Instant)
            {
                effectType = EffectType.Instant;
            }
            else if (durationPolicy == DurationPolicy.Infinite)
            {
                effectType = EffectType.Infinite;
            }
            else if (period > 0 && period < duration)
            {
                effectType = EffectType.Periodic;
            }
            else
            {
                effectType = EffectType.Duration;
            }
        }
#endif

        #endregion
    }

    /// <summary>
    /// Duration policy for effects
    /// </summary>
    public enum DurationPolicy
    {
        /// <summary>
        /// Effect applies instantly and doesn't persist
        /// </summary>
        Instant,

        /// <summary>
        /// Effect has a specific duration
        /// </summary>
        HasDuration,

        /// <summary>
        /// Effect lasts until manually removed
        /// </summary>
        Infinite
    }

    /// <summary>
    /// Effect stacking policy
    /// </summary>
    public enum StackingPolicy
    {
        /// <summary>
        /// Effect cannot stack
        /// </summary>
        None,

        /// <summary>
        /// Effect can stack up to max count
        /// </summary>
        Stack,

        /// <summary>
        /// New application replaces old one
        /// </summary>
        Replace,

        /// <summary>
        /// New application refreshes duration
        /// </summary>
        Refresh
    }

    /// <summary>
    /// Effect type classification
    /// </summary>
    public enum EffectType
    {
        /// <summary>
        /// One-time instant effect
        /// </summary>
        Instant,

        /// <summary>
        /// Effect that lasts for a duration
        /// </summary>
        Duration,

        /// <summary>
        /// Effect that executes periodically
        /// </summary>
        Periodic,

        /// <summary>
        /// Effect that lasts forever until removed
        /// </summary>
        Infinite
    }
}