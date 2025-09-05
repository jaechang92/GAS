// ================================
// File: Assets/Scripts/GAS/EffectSystem/Types/InstantEffect.cs
// ================================
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
    /// Effect that applies immediately and doesn't persist
    /// </summary>
    [CreateAssetMenu(fileName = "InstantEffect", menuName = "GAS/Effects/Instant Effect")]
    public class InstantEffect : GameplayEffect
    {
        [Header("Instant Effect Settings")]
        [SerializeField] protected List<AttributeModification> attributeModifications = new List<AttributeModification>();
        [SerializeField] protected bool scaleWithLevel = true;
        [SerializeField] protected float levelScaling = 0.1f; // 10% per level

        [Header("Conditional Effects")]
        [SerializeField] protected List<ConditionalEffect> conditionalEffects = new List<ConditionalEffect>();

        [Header("Execute Effects")]
        [SerializeField] protected List<GameplayEffect> effectsToExecute = new List<GameplayEffect>();

        [Header("Visual/Audio")]
        [SerializeField] protected GameObject hitEffectPrefab;
        [SerializeField] protected AudioClip hitSound;
        [SerializeField] protected float effectScale = 1f;

        [Header("Events")]
        [SerializeField] protected bool triggerCombatEvents = true;
        [SerializeField] protected string customEventName;

        /// <summary>
        /// Apply the instant effect
        /// </summary>
        public override void OnApply(EffectContext context)
        {
            if (context == null || context.target == null) return;

            // Get target components
            var targetAttributes = context.target.GetComponent<AttributeSetComponent>();
            var targetTags = context.target.GetComponent<TagComponent>();

            if (targetAttributes == null) return;

            // Calculate effect multiplier
            float multiplier = CalculateMultiplier(context);

            // Apply attribute modifications
            foreach (var modification in attributeModifications)
            {
                ApplyAttributeModification(targetAttributes, modification, multiplier, context);
            }

            // Check and apply conditional effects
            foreach (var conditional in conditionalEffects)
            {
                if (CheckCondition(conditional, context, targetTags))
                {
                    ApplyAttributeModification(targetAttributes, conditional.modification, multiplier, context);
                }
            }

            // Execute additional effects
            ExecuteAdditionalEffects(context);

            // Spawn visual effects
            SpawnVisualEffects(context);

            // Play audio
            PlayAudio(context);

            // Fire events
            if (triggerCombatEvents)
            {
                FireCombatEvents(context);
            }

            // Custom event
            if (!string.IsNullOrEmpty(customEventName))
            {
                FireCustomEvent(context);
            }
        }

        /// <summary>
        /// Instant effects don't need removal
        /// </summary>
        public override void OnRemove(EffectContext context)
        {
            // Instant effects complete immediately, nothing to remove
        }

        /// <summary>
        /// Apply a single attribute modification
        /// </summary>
        protected virtual void ApplyAttributeModification(
            AttributeSetComponent attributes,
            AttributeModification modification,
            float multiplier,
            EffectContext context)
        {
            if (modification == null) return;

            float value = modification.CalculateValue(context.level) * multiplier * context.magnitude;

            // Check for special handling
            if (modification.attributeType == AttributeType.Health)
            {
                // Health modification - could be damage or healing
                if (value < 0)
                {
                    ApplyDamage(attributes, -value, context);
                }
                else
                {
                    ApplyHealing(attributes, value, context);
                }
            }
            else
            {
                // Standard attribute modification
                attributes.ModifyAttribute(
                    modification.attributeType,
                    value,
                    modification.operation
                );
            }
        }

        /// <summary>
        /// Apply damage with additional processing
        /// </summary>
        protected virtual void ApplyDamage(AttributeSetComponent attributes, float damage, EffectContext context)
        {
            // Check for damage modifiers (armor, resistance, etc.)
            float finalDamage = CalculateFinalDamage(damage, attributes, context);

            // Apply the damage
            attributes.ModifyAttribute(AttributeType.Health, -finalDamage, ModifierOperation.Add);

            // Fire damage event
            if (triggerCombatEvents)
            {
                GASEvents.Trigger(GASEventType.DamageReceived, new DamageEventData
                {
                    source = context.instigator,
                    target = context.target,
                    damage = finalDamage,
                    isCritical = context.isCritical,
                    damageType = GetDamageType()
                });
            }
        }

        /// <summary>
        /// Apply healing with additional processing
        /// </summary>
        protected virtual void ApplyHealing(AttributeSetComponent attributes, float healing, EffectContext context)
        {
            // Get current and max health
            float currentHealth = attributes.GetAttributeValue(AttributeType.Health);
            float maxHealth = attributes.GetAttributeMaxValue(AttributeType.Health);

            // Calculate actual healing (can't exceed max)
            float actualHealing = Mathf.Min(healing, maxHealth - currentHealth);

            // Apply the healing
            attributes.ModifyAttribute(AttributeType.Health, actualHealing, ModifierOperation.Add);

            // Fire healing event
            if (triggerCombatEvents)
            {
                GASEvents.Trigger(GASEventType.HealingReceived, new HealingEventData
                {
                    source = context.instigator,
                    target = context.target,
                    healing = actualHealing,
                    isCritical = context.isCritical
                });
            }
        }

        /// <summary>
        /// Calculate final damage after mitigation
        /// </summary>
        protected virtual float CalculateFinalDamage(float baseDamage, AttributeSetComponent attributes, EffectContext context)
        {
            float finalDamage = baseDamage;

            // Apply armor reduction (physical damage)
            if (HasTag("Damage.Physical"))
            {
                float armor = attributes.GetAttributeValue(AttributeType.Armor);
                float armorReduction = armor / (armor + 100f); // Simple armor formula
                finalDamage *= (1f - armorReduction);
            }

            // Apply magic resistance (magic damage)
            if (HasTag("Damage.Magic"))
            {
                float magicResist = attributes.GetAttributeValue(AttributeType.MagicResistance);
                float resistReduction = magicResist / (magicResist + 100f);
                finalDamage *= (1f - resistReduction);
            }

            // Check for block
            if (!context.isBlocked)
            {
                float blockChance = attributes.GetAttributeValue(AttributeType.Block);
                if (UnityEngine.Random.Range(0f, 100f) < blockChance)
                {
                    context.isBlocked = true;
                    finalDamage *= 0.5f; // 50% damage on block
                }
            }

            // Check for critical
            if (!context.isCritical && context.instigator != null)
            {
                var sourceAttributes = context.instigator.GetComponent<AttributeSetComponent>();
                if (sourceAttributes != null)
                {
                    float critChance = sourceAttributes.GetAttributeValue(AttributeType.CriticalChance);
                    if (UnityEngine.Random.Range(0f, 100f) < critChance)
                    {
                        context.isCritical = true;
                        float critDamage = sourceAttributes.GetAttributeValue(AttributeType.CriticalDamage);
                        finalDamage *= (1f + critDamage / 100f); // Default 150% for 50% crit damage
                    }
                }
            }

            return Mathf.Max(1f, finalDamage); // Minimum 1 damage
        }

        /// <summary>
        /// Calculate effect multiplier based on context
        /// </summary>
        protected virtual float CalculateMultiplier(EffectContext context)
        {
            float multiplier = 1f;

            // Level scaling
            if (scaleWithLevel && context.level > 1)
            {
                multiplier += levelScaling * (context.level - 1);
            }

            // Critical multiplier (already applied in damage calculation, but can affect other attributes)
            if (context.isCritical && !IsHealthModification())
            {
                multiplier *= 1.5f;
            }

            return multiplier;
        }

        /// <summary>
        /// Check if this effect modifies health
        /// </summary>
        protected bool IsHealthModification()
        {
            return attributeModifications.Exists(m => m.attributeType == AttributeType.Health);
        }

        /// <summary>
        /// Check conditional effect requirements
        /// </summary>
        protected virtual bool CheckCondition(ConditionalEffect conditional, EffectContext context, TagComponent targetTags)
        {
            switch (conditional.conditionType)
            {
                case ConditionType.TargetHasTag:
                    return targetTags != null && targetTags.HasTag(conditional.requiredTag);

                case ConditionType.TargetMissingTag:
                    return targetTags == null || !targetTags.HasTag(conditional.requiredTag);

                case ConditionType.HealthBelow:
                    var attributes = context.target.GetComponent<AttributeSetComponent>();
                    if (attributes != null)
                    {
                        float healthPercent = attributes.GetAttributeValue(AttributeType.HealthPercent);
                        return healthPercent < conditional.threshold;
                    }
                    return false;

                case ConditionType.Random:
                    return UnityEngine.Random.Range(0f, 100f) < conditional.threshold;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Execute additional effects
        /// </summary>
        protected virtual void ExecuteAdditionalEffects(EffectContext context)
        {
            if (effectsToExecute == null || effectsToExecute.Count == 0) return;

            var effectComponent = context.target.GetComponent<EffectComponent>();
            if (effectComponent == null) return;

            foreach (var effect in effectsToExecute)
            {
                if (effect != null)
                {
                    // Create new context for the additional effect
                    var newContext = new EffectContext(context.instigator, context.target)
                    {
                        sourceAbility = context.sourceAbility,
                        level = context.level,
                        magnitude = context.magnitude,
                        effectLocation = context.effectLocation,
                        isCritical = context.isCritical
                    };

                    effectComponent.ApplyEffect(effect, newContext);
                }
            }
        }

        /// <summary>
        /// Spawn visual effects
        /// </summary>
        protected virtual void SpawnVisualEffects(EffectContext context)
        {
            if (hitEffectPrefab == null) return;

            Vector3 spawnPosition = context.hitLocation != Vector3.zero ?
                context.hitLocation : context.target.transform.position;

            var effect = Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);
            effect.transform.localScale = Vector3.one * effectScale;

            // Auto destroy after 5 seconds if no particle system
            var particleSystem = effect.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                Destroy(effect, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(effect, 5f);
            }
        }

        /// <summary>
        /// Play audio effects
        /// </summary>
        protected virtual void PlayAudio(EffectContext context)
        {
            if (hitSound == null) return;

            AudioSource.PlayClipAtPoint(hitSound, context.target.transform.position);
        }

        /// <summary>
        /// Fire combat events
        /// </summary>
        protected virtual void FireCombatEvents(EffectContext context)
        {
            // This is handled in ApplyDamage/ApplyHealing
        }

        /// <summary>
        /// Fire custom event
        /// </summary>
        protected virtual void FireCustomEvent(EffectContext context)
        {
            var eventData = new InstantEffectEventData
            {
                effect = this,
                context = context,
                eventName = customEventName,
                source = context.target
            };

            // You can trigger custom event through your event system
            Debug.Log($"Custom Event Fired: {customEventName}");
        }

        /// <summary>
        /// Check if effect has a specific tag
        /// </summary>
        protected bool HasTag(string tagName)
        {
            return grantedTags != null && grantedTags.Tags.Exists(t => t.TagName.Contains(tagName));
        }

        /// <summary>
        /// Get damage type based on tags
        /// </summary>
        protected virtual DamageType GetDamageType()
        {
            if (HasTag("Damage.Physical")) return DamageType.Physical;
            if (HasTag("Damage.Magic")) return DamageType.Magic;
            if (HasTag("Damage.True")) return DamageType.True;
            return DamageType.Physical;
        }
    }

    /// <summary>
    /// Attribute modification data
    /// </summary>
    [Serializable]
    public class AttributeModification
    {
        public AttributeType attributeType;
        public ModifierOperation operation = ModifierOperation.Add;
        public float baseValue;
        public float valuePerLevel;

        public float CalculateValue(int level)
        {
            return baseValue + (valuePerLevel * (level - 1));
        }
    }

    /// <summary>
    /// Conditional effect data
    /// </summary>
    [Serializable]
    public class ConditionalEffect
    {
        public ConditionType conditionType;
        public GameplayTag requiredTag;
        public float threshold;
        public AttributeModification modification;
    }

    /// <summary>
    /// Condition types for conditional effects
    /// </summary>
    public enum ConditionType
    {
        TargetHasTag,
        TargetMissingTag,
        HealthBelow,
        HealthAbove,
        ManaBelow,
        ManaAbove,
        Random,
        IsCritical,
        IsBlocked
    }

    /// <summary>
    /// Damage types
    /// </summary>
    public enum DamageType
    {
        Physical,
        Magic,
        True,
        Mixed
    }

    /// <summary>
    /// Event data for instant effects
    /// </summary>
    public class InstantEffectEventData : GASEventData
    {
        public InstantEffect effect;
        public EffectContext context;
        public string eventName;
    }

    /// <summary>
    /// Event data for damage
    /// </summary>
    public class DamageEventData : GASEventData
    {
        public GameObject target;
        public float damage;
        public DamageType damageType;
        public bool isCritical;
        public bool isBlocked;
        public bool isDodged;
    }

    /// <summary>
    /// Event data for healing
    /// </summary>
    public class HealingEventData : GASEventData
    {
        public GameObject target;
        public float healing;
        public bool isCritical;
        public bool isOverheal;
    }
}
