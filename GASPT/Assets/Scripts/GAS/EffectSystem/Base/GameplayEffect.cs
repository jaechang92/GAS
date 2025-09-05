// ================================
// File: Assets/Scripts/GAS/EffectSystem/Base/GameplayEffect.cs
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
                    tagComponent.AddTag(tag);
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
                    tagComponent.RemoveTag(tag);
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
                assetTags.Tags.Contains<GameplayTag>("Debuff");

            return isDebuff ? 100 : 50;
        }

        /// <summary>
        /// Gets the effect color for UI
        /// </summary>
        public virtual Color GetEffectColor()
        {
            // Red for debuffs
            if (assetTags != null && assetTags.Tags.Contains<GameplayTag>("Debuff"))
                return Color.red;

            // Green for buffs
            if (assetTags != null && assetTags.Tags.Contains<GameplayTag>("Buff"))
                return Color.green;

            // Default white
            return Color.white;
        }
    }

    /// <summary>
    /// Duration policy for effects
    /// </summary>
    public enum DurationPolicy
    {
        Instant,
        HasDuration,
        Infinite
    }

    /// <summary>
    /// Effect stacking policy
    /// </summary>
    public enum StackingPolicy
    {
        None,
        Stack,
        Replace,
        Refresh
    }

    /// <summary>
    /// Effect type classification
    /// </summary>
    public enum EffectType
    {
        Instant,
        Duration,
        Periodic,
        Infinite
    }
}