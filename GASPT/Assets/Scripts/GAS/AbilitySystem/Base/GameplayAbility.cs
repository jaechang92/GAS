// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Base/GameplayAbility.cs
// ================================
using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.Core;
using GAS.TagSystem;
using GAS.AttributeSystem;
using GAS.EffectSystem;
using static GAS.Core.GASConstants;

namespace GAS.AbilitySystem
{
    /// <summary>
    /// Base class for all gameplay abilities
    /// </summary>
    public abstract class GameplayAbility : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] protected string abilityId;
        [SerializeField] protected string abilityName;
        [SerializeField] protected string description;
        [SerializeField] protected Sprite icon;
        [SerializeField] protected int maxLevel = 10;

        [Header("Execution Settings")]
        [SerializeField] protected AbilityExecutionPolicy executionPolicy = AbilityExecutionPolicy.Single;
        [SerializeField] protected AbilityPriority priority = AbilityPriority.Normal;
        [SerializeField] protected AbilityTargetType targetType = AbilityTargetType.None;
        [SerializeField] protected bool canCancelOtherAbilities = false;
        [SerializeField] protected bool canBeCancelled = true;

        [Header("Cost & Cooldown")]
        [SerializeField] protected List<AbilityCostData> costs = new List<AbilityCostData>();
        [SerializeField] protected AbilityCooldownData cooldown;

        [Header("Tag Requirements")]
        [SerializeField] protected AbilityTagRequirements tagRequirements;

        [Header("Tags Applied")]
        [SerializeField] protected List<GameplayTag> abilityTags = new List<GameplayTag>();
        [SerializeField] protected List<GameplayTag> activationOwnedTags = new List<GameplayTag>();
        [SerializeField] protected List<GameplayTag> cancelAbilitiesWithTag = new List<GameplayTag>();

        [Header("Effects")]
        [SerializeField] protected List<GameplayEffect> effectsToApplyOnStart = new List<GameplayEffect>();
        [SerializeField] protected List<GameplayEffect> effectsToApplyOnEnd = new List<GameplayEffect>();

        // Runtime data
        protected AbilitySpec currentSpec;
        protected AbilityActivationInfo activationInfo;
        protected bool isExecuting = false;
        protected AwaitableCompletionSource completionSource;

        // Properties
        public string AbilityId => abilityId;
        public string AbilityName => abilityName;
        public string Description => description;
        public Sprite Icon => icon;
        public int MaxLevel => maxLevel;
        public AbilityExecutionPolicy ExecutionPolicy => executionPolicy;
        public AbilityPriority Priority => priority;
        public AbilityTargetType TargetType => targetType;
        public AbilityTagRequirements TagRequirements => tagRequirements;
        public List<GameplayTag> AbilityTags => abilityTags;

        /// <summary>
        /// Checks if the ability can be activated
        /// </summary>
        public virtual AbilityExecutionResult CanActivate(AbilitySpec spec, AbilityActivationInfo info)
        {
            // Check if source is valid
            if (info.source == null || !info.source.IsAlive)
            {
                return AbilityExecutionResult.Failure(AbilityFailureReason.SourceDead);
            }

            // Check current state
            if (spec.state != AbilityState.Idle && spec.state != AbilityState.Cooldown)
            {
                if (executionPolicy == AbilityExecutionPolicy.Single)
                {
                    return AbilityExecutionResult.Failure(AbilityFailureReason.AlreadyActive);
                }
            }

            // Check cooldown
            if (spec.IsOnCooldown)
            {
                return AbilityExecutionResult.Failure(AbilityFailureReason.OnCooldown);
            }

            // Check tag requirements
            if (!CheckTagRequirements(info.source))
            {
                return AbilityExecutionResult.Failure(AbilityFailureReason.RequiredTagsMissing);
            }

            // Check costs
            if (!CheckCosts(spec, info.source))
            {
                return AbilityExecutionResult.Failure(AbilityFailureReason.InsufficientResources);
            }

            // Check target validity
            if (!ValidateTarget(info))
            {
                return AbilityExecutionResult.Failure(AbilityFailureReason.InvalidTarget);
            }

            return AbilityExecutionResult.Success();
        }

        /// <summary>
        /// Activates the ability
        /// </summary>
        public virtual async Awaitable<AbilityExecutionResult> Activate(AbilitySpec spec, AbilityActivationInfo info)
        {
            // Store runtime data
            currentSpec = spec;
            activationInfo = info;

            // Check if can activate
            var canActivateResult = CanActivate(spec, info);
            if (!canActivateResult.success)
            {
                return canActivateResult;
            }

            // Update state
            spec.state = AbilityState.PreActivation;

            // Fire pre-activation event
            await FireEvent(AbilityEventType.PreActivate);

            // Apply costs
            if (!ApplyCosts(spec, info.source))
            {
                spec.state = AbilityState.Idle;
                return AbilityExecutionResult.Failure(AbilityFailureReason.InsufficientResources);
            }

            // Cancel other abilities if needed
            if (canCancelOtherAbilities && cancelAbilitiesWithTag.Count > 0)
            {
                CancelAbilitiesWithTags(info.source);
            }

            // Apply activation tags
            ApplyActivationTags(info.source, true);

            // Apply start effects
            ApplyEffects(effectsToApplyOnStart, info.source);

            // Update state to activating
            spec.state = AbilityState.Activating;

            // Fire activation event
            await FireEvent(AbilityEventType.Activated);

            // Start cooldown if configured
            if (cooldown.startOnActivation)
            {
                StartCooldown(spec);
            }

            // Execute ability logic
            spec.state = AbilityState.Active;
            isExecuting = true;
            completionSource = new AwaitableCompletionSource();

            try
            {
                var result = await ExecuteAbility();

                // Ability completed successfully
                await EndAbility(true);
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Ability {abilityName} execution failed: {e.Message}");
                await EndAbility(false);
                return AbilityExecutionResult.Failure(AbilityFailureReason.Custom, e.Message);
            }
        }

        /// <summary>
        /// Core ability execution logic - must be implemented by derived classes
        /// </summary>
        protected abstract Awaitable<AbilityExecutionResult> ExecuteAbility();

        /// <summary>
        /// Ends the ability
        /// </summary>
        public virtual async Awaitable EndAbility(bool wasSuccessful)
        {
            if (!isExecuting) return;

            isExecuting = false;
            currentSpec.state = AbilityState.Ending;

            // Remove activation tags
            ApplyActivationTags(activationInfo.source, false);

            // Apply end effects
            if (wasSuccessful)
            {
                ApplyEffects(effectsToApplyOnEnd, activationInfo.source);
            }

            // Start cooldown if not already started
            if (!cooldown.startOnActivation)
            {
                StartCooldown(currentSpec);
            }

            // Fire end event
            await FireEvent(AbilityEventType.Ended);

            // Update state
            if (currentSpec.IsOnCooldown)
            {
                currentSpec.state = AbilityState.Cooldown;
            }
            else
            {
                currentSpec.state = AbilityState.Idle;
            }

            // Increment use count
            currentSpec.IncrementUseCount();

            // Complete awaitable
            completionSource?.SetResult();
        }

        /// <summary>
        /// Cancels the ability
        /// </summary>
        public virtual async Awaitable CancelAbility()
        {
            if (!canBeCancelled || !isExecuting) return;

            currentSpec.state = AbilityState.Cancelled;

            // Fire cancel event
            await FireEvent(AbilityEventType.Cancelled);

            await EndAbility(false);
        }

        /// <summary>
        /// Checks if tag requirements are met
        /// </summary>
        protected virtual bool CheckTagRequirements(IAbilitySource source)
        {
            if (tagRequirements == null || source.TagComponent == null)
                return true;

            return tagRequirements.CheckRequirements(source.TagComponent);
        }

        /// <summary>
        /// Checks if costs can be paid
        /// </summary>
        protected virtual bool CheckCosts(AbilitySpec spec, IAbilitySource source)
        {
            if (costs == null || costs.Count == 0)
                return true;

            var attributes = source.AttributeComponent;
            if (attributes == null) return false;

            foreach (var cost in costs)
            {
                float actualCost = cost.CalculateCost(spec.level);
                float currentValue = attributes.GetAttributeValue(cost.attributeType);

                if (currentValue < actualCost)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Applies costs to the source
        /// </summary>
        protected virtual bool ApplyCosts(AbilitySpec spec, IAbilitySource source)
        {
            if (costs == null || costs.Count == 0)
                return true;

            var attributes = source.AttributeComponent;
            if (attributes == null) return false;

            foreach (var cost in costs)
            {
                float actualCost = cost.CalculateCost(spec.level);
                attributes.ModifyAttribute(cost.attributeType, -actualCost, ModifierOperation.Add);
            }

            return true;
        }

        /// <summary>
        /// Validates the target based on target type
        /// </summary>
        protected virtual bool ValidateTarget(AbilityActivationInfo info)
        {
            switch (targetType)
            {
                case AbilityTargetType.None:
                case AbilityTargetType.Self:
                    return true;

                case AbilityTargetType.Target:
                    return info.target != null;

                case AbilityTargetType.Point:
                    return info.activationPosition != Vector3.zero;

                case AbilityTargetType.Direction:
                    return info.activationDirection != Vector3.zero;

                case AbilityTargetType.Area:
                    return true;

                default:
                    return ValidateCustomTarget(info);
            }
        }

        /// <summary>
        /// Override for custom target validation
        /// </summary>
        protected virtual bool ValidateCustomTarget(AbilityActivationInfo info)
        {
            return true;
        }

        /// <summary>
        /// Starts the cooldown
        /// </summary>
        protected virtual void StartCooldown(AbilitySpec spec)
        {
            float cooldownDuration = cooldown.CalculateCooldown(spec.level);
            spec.SetCooldown(cooldownDuration);
        }

        /// <summary>
        /// Applies or removes activation tags
        /// </summary>
        protected virtual void ApplyActivationTags(IAbilitySource source, bool apply)
        {
            if (source.TagComponent == null || activationOwnedTags == null)
                return;

            foreach (var tag in activationOwnedTags)
            {
                if (apply)
                    source.TagComponent.AddTag(tag);
                else
                    source.TagComponent.RemoveTag(tag);
            }
        }

        /// <summary>
        /// Cancels abilities with specified tags
        /// </summary>
        protected virtual void CancelAbilitiesWithTags(IAbilitySource source)
        {
            // This will be implemented in AbilitySystemComponent
            GASEvents.Trigger(GASEventType.AbilityCancelRequested,
                new AbilityCancelEventData
                {
                    source = source,
                    tagsToCancel = cancelAbilitiesWithTag
                });
        }

        /// <summary>
        /// Applies effects to the source
        /// </summary>
        protected virtual void ApplyEffects(List<GameplayEffect> effects, IAbilitySource source)
        {
            if (effects == null || source.EffectComponent == null)
                return;

            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    // Create effect context from ability
                    var context = CreateEffectContext(source);

                    // Apply effect with context
                    source.EffectComponent.ApplyEffect(effect, context);
                }
            }
        }

        protected virtual EffectContext CreateEffectContext(IAbilitySource source)
        {
            var context = new EffectContext(source.GameObject, activationInfo.target)
            {
                sourceAbility = this,
                level = currentSpec?.level ?? 1,
                effectLocation = activationInfo.activationPosition,
                magnitude = CalculateEffectMagnitude()
            };

            return context;
        }

        /// <summary>
        /// Fires ability events
        /// </summary>
        protected virtual async Awaitable FireEvent(AbilityEventType eventType)
        {
            var eventData = new AbilityEventData
            {
                ability = this,
                spec = currentSpec,
                activationInfo = activationInfo,
                eventType = eventType
            };

            GASEvents.Trigger(GASEventType.AbilityEvent, eventData);
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// Gets the cooldown remaining time
        /// </summary>
        public float GetCooldownRemaining(AbilitySpec spec)
        {
            return spec?.RemainingCooldown ?? 0f;
        }

        /// <summary>
        /// Gets the cooldown duration for a level
        /// </summary>
        public float GetCooldownDuration(int level)
        {
            return cooldown.CalculateCooldown(level);
        }

        /// <summary>
        /// Calculates total cost for a level
        /// </summary>
        public Dictionary<string, float> CalculateCosts(int level)
        {
            var result = new Dictionary<string, float>();

            foreach (var cost in costs)
            {
                result[cost.attributeName] = cost.CalculateCost(level);
            }

            return result;
        }
    }
}