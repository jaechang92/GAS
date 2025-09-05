// ================================
// File: Assets/Scripts/GAS/EffectSystem/Types/DurationEffect.cs
// ================================
using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.Core;
using GAS.TagSystem;
using GAS.AttributeSystem;
using static GAS.Core.GASConstants;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Effect that lasts for a specific duration
    /// </summary>
    [CreateAssetMenu(fileName = "DurationEffect", menuName = "GAS/Effects/Duration Effect")]
    public class DurationEffect : GameplayEffect
    {
        [Header("Duration Settings")]
        [SerializeField] protected bool scaleDurationWithStackCount = false;
        [SerializeField] protected float durationPerStack = 0f;
        [SerializeField] protected AnimationCurve durationScalingCurve;

        [Header("Behavior")]
        [SerializeField] protected bool removeOnDeath = true;
        [SerializeField] protected bool persistThroughDeath = false;
        [SerializeField] protected bool triggerExpirationEffects = true;
        [SerializeField] protected List<GameplayEffect> onExpirationEffects = new List<GameplayEffect>();
        [SerializeField] protected bool removeTagsOnExpiration = true;

        [Header("Modifier Scaling")]
        [SerializeField] protected bool scaleModifiersOverTime = false;
        [SerializeField] protected AnimationCurve modifierScalingCurve;

        [Header("UI Display")]
        [SerializeField] protected bool showInUI = true;
        [SerializeField] protected Color effectColor = Color.white;
        [SerializeField] protected bool showTimer = true;
        [SerializeField] protected bool showStacks = false;

        // Runtime tracking
        protected Dictionary<GameObject, DurationEffectState> activeStates = new Dictionary<GameObject, DurationEffectState>();

        /// <summary>
        /// Apply the duration effect
        /// </summary>
        public override void OnApply(EffectContext context)
        {
            if (context == null || context.target == null) return;

            var state = GetOrCreateState(context.target);
            state.context = context;
            state.startTime = Time.time;
            state.duration = CalculateActualDuration(context);

            // Apply granted tags
            ApplyGrantedTags(context);

            // Apply initial modifiers
            ApplyInitialModifiers(context, state);

            // Spawn visual effects
            state.visualEffect = SpawnVisualEffect(context);

            // Play application sound
            PlayApplicationSound(context);

            // Register for death events if needed
            if (removeOnDeath)
            {
                RegisterForDeathEvent(context.target);
            }

            // Fire applied event
            FireEffectAppliedEvent(context);

            Debug.Log($"[DurationEffect] Applied {effectName} to {context.target.name} for {state.duration} seconds");
        }

        /// <summary>
        /// Remove the duration effect
        /// </summary>
        public override void OnRemove(EffectContext context)
        {
            if (context == null || context.target == null) return;

            if (!activeStates.TryGetValue(context.target, out var state))
                return;

            // Check if naturally expired
            bool naturalExpiration = state.IsExpired();

            // Trigger expiration effects if natural expiration
            if (naturalExpiration && triggerExpirationEffects)
            {
                ApplyExpirationEffects(context);
            }

            // Remove modifiers
            RemoveModifiers(context);

            // Remove granted tags if configured
            if (removeTagsOnExpiration)
            {
                RemoveGrantedTags(context);
            }

            // Clean up visual effects
            if (state.visualEffect != null)
            {
                Destroy(state.visualEffect);
            }

            // Play removal sound
            PlayRemovalSound(context);

            // Unregister from death events
            if (removeOnDeath)
            {
                UnregisterFromDeathEvent(context.target);
            }

            // Fire removed event
            FireEffectRemovedEvent(context, naturalExpiration);

            // Remove state
            activeStates.Remove(context.target);

            Debug.Log($"[DurationEffect] Removed {effectName} from {context.target.name} (Natural: {naturalExpiration})");
        }

        /// <summary>
        /// Update tick for the duration effect
        /// </summary>
        public override void OnTick(EffectContext context, float deltaTime)
        {
            if (context == null || context.target == null) return;

            if (!activeStates.TryGetValue(context.target, out var state))
                return;

            // Update elapsed time
            state.elapsedTime += deltaTime;

            // Check if expired
            if (state.IsExpired())
            {
                // Trigger removal
                var effectComponent = context.target.GetComponent<EffectComponent>();
                effectComponent?.RemoveEffect(this);
                return;
            }

            // Scale modifiers over time if configured
            if (scaleModifiersOverTime)
            {
                UpdateScaledModifiers(context, state);
            }

            // Update visual effect
            UpdateVisualEffect(state);

            // Check ongoing requirements
            if (!CheckOngoing(context))
            {
                var effectComponent = context.target.GetComponent<EffectComponent>();
                effectComponent?.RemoveEffect(this);
            }
        }

        /// <summary>
        /// Handle stacking
        /// </summary>
        public override void OnStack(EffectContext context, int newStackCount, int previousStackCount)
        {
            if (!activeStates.TryGetValue(context.target, out var state))
                return;

            state.stackCount = newStackCount;

            // Refresh duration based on stacking policy
            if (refreshDurationOnStack)
            {
                state.startTime = Time.time;
                state.elapsedTime = 0;

                if (scaleDurationWithStackCount)
                {
                    state.duration = CalculateStackedDuration(context, newStackCount);
                }
            }

            // Update modifiers for new stack count
            UpdateStackModifiers(context, state, newStackCount);

            // Fire stack event
            GASEvents.Trigger(GASEventType.EffectStackChanged, new EffectStackEventData
            {
                effect = this,
                target = context.target,
                newStackCount = newStackCount,
                previousStackCount = previousStackCount,
                source = context.target
            });

            Debug.Log($"[DurationEffect] {effectName} stacked to {newStackCount}x on {context.target.name}");
        }

        /// <summary>
        /// Calculate actual duration
        /// </summary>
        protected float CalculateActualDuration(EffectContext context)
        {
            float actualDuration = duration;

            // Apply context multiplier
            actualDuration *= context.durationMultiplier;

            // Apply level scaling
            if (context.level > 1)
            {
                actualDuration *= (1f + (context.level - 1) * 0.1f); // 10% per level
            }

            // Apply curve if available
            if (durationScalingCurve != null && durationScalingCurve.keys.Length > 0)
            {
                actualDuration *= durationScalingCurve.Evaluate(context.level);
            }

            return Mathf.Max(0.1f, actualDuration);
        }

        /// <summary>
        /// Calculate stacked duration
        /// </summary>
        protected float CalculateStackedDuration(EffectContext context, int stacks)
        {
            float baseDuration = CalculateActualDuration(context);

            if (scaleDurationWithStackCount && stacks > 1)
            {
                baseDuration += durationPerStack * (stacks - 1);
            }

            return baseDuration;
        }

        /// <summary>
        /// Apply initial modifiers
        /// </summary>
        protected void ApplyInitialModifiers(EffectContext context, DurationEffectState state)
        {
            if (modifiers == null || modifiers.Count == 0)
                return;

            var attributeComponent = context.target.GetComponent<AttributeSetComponent>();
            if (attributeComponent == null)
                return;

            state.appliedModifiers.Clear();

            foreach (var modifierTemplate in modifiers)
            {
                if (modifierTemplate != null)
                {
                    var modifier = modifierTemplate.Clone();
                    modifier.source = this;
                    modifier.sourceName = effectName;

                    // Apply initial scaling if needed
                    if (scaleModifiersOverTime && modifierScalingCurve != null)
                    {
                        float scale = modifierScalingCurve.Evaluate(0);
                        modifier.value *= scale;
                    }

                    attributeComponent.AddModifier(modifier.targetAttributeType, modifier);
                    state.appliedModifiers.Add(modifier);
                }
            }
        }

        /// <summary>
        /// Update scaled modifiers over time
        /// </summary>
        protected void UpdateScaledModifiers(EffectContext context, DurationEffectState state)
        {
            if (modifierScalingCurve == null || modifierScalingCurve.keys.Length == 0)
                return;

            var attributeComponent = context.target.GetComponent<AttributeSetComponent>();
            if (attributeComponent == null)
                return;

            float progress = state.GetProgress();
            float scale = modifierScalingCurve.Evaluate(progress);

            // Update each modifier's value
            for (int i = 0; i < modifiers.Count && i < state.appliedModifiers.Count; i++)
            {
                var originalModifier = modifiers[i];
                var appliedModifier = state.appliedModifiers[i];

                if (originalModifier != null && appliedModifier != null)
                {
                    // Remove old modifier
                    attributeComponent.RemoveModifier(appliedModifier.targetAttributeType, appliedModifier);

                    // Update value
                    appliedModifier.value = originalModifier.value * scale;

                    // Re-add modifier
                    attributeComponent.AddModifier(appliedModifier.targetAttributeType, appliedModifier);
                }
            }
        }

        /// <summary>
        /// Update modifiers for stack changes
        /// </summary>
        protected void UpdateStackModifiers(EffectContext context, DurationEffectState state, int stacks)
        {
            var attributeComponent = context.target.GetComponent<AttributeSetComponent>();
            if (attributeComponent == null)
                return;

            // Remove old modifiers
            foreach (var modifier in state.appliedModifiers)
            {
                if (modifier != null)
                {
                    attributeComponent.RemoveModifier(modifier.targetAttributeType, modifier);
                }
            }

            state.appliedModifiers.Clear();

            // Apply new modifiers with stack multiplier
            foreach (var modifierTemplate in modifiers)
            {
                if (modifierTemplate != null)
                {
                    var modifier = modifierTemplate.Clone();
                    modifier.source = this;
                    modifier.sourceName = effectName;
                    modifier.value *= stacks; // Scale with stack count

                    attributeComponent.AddModifier(modifier.targetAttributeType, modifier);
                    state.appliedModifiers.Add(modifier);
                }
            }
        }

        /// <summary>
        /// Apply effects when duration expires
        /// </summary>
        protected void ApplyExpirationEffects(EffectContext context)
        {
            if (onExpirationEffects == null || onExpirationEffects.Count == 0)
                return;

            var effectComponent = context.target.GetComponent<EffectComponent>();
            if (effectComponent == null)
                return;

            foreach (var effect in onExpirationEffects)
            {
                if (effect != null)
                {
                    var expirationContext = context.Clone();
                    expirationContext.sourceEffect = this;
                    effectComponent.ApplyEffect(effect, expirationContext);
                }
            }
        }

        /// <summary>
        /// Update visual effect
        /// </summary>
        protected void UpdateVisualEffect(DurationEffectState state)
        {
            if (state.visualEffect == null)
                return;

            // Update particle effects based on remaining duration
            var particleSystems = state.visualEffect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                float progress = state.GetProgress();

                // Fade out near the end
                if (progress > 0.8f)
                {
                    float fadeProgress = (progress - 0.8f) / 0.2f;
                    var col = ps.colorOverLifetime;
                    col.enabled = true;
                }
            }
        }

        /// <summary>
        /// Register for death events
        /// </summary>
        protected void RegisterForDeathEvent(GameObject target)
        {
            // Would subscribe to death events from health component
            GASEvents.Subscribe(GASEventType.Death, OnTargetDeath);
        }

        /// <summary>
        /// Unregister from death events
        /// </summary>
        protected void UnregisterFromDeathEvent(GameObject target)
        {
            GASEvents.Unsubscribe(GASEventType.Death, OnTargetDeath);
        }

        /// <summary>
        /// Handle target death
        /// </summary>
        protected void OnTargetDeath(object data)
        {
            if (!removeOnDeath || persistThroughDeath)
                return;

            if (data is GASEventData eventData && activeStates.ContainsKey(eventData.source))
            {
                var effectComponent = eventData.source.GetComponent<EffectComponent>();
                effectComponent?.RemoveEffect(this);
            }
        }

        /// <summary>
        /// Fire effect applied event
        /// </summary>
        protected void FireEffectAppliedEvent(EffectContext context)
        {
            GASEvents.Trigger(GASEventType.EffectApplied, new EffectEventData
            {
                effect = this,
                context = context,
                target = context.target,
                source = context.target
            });
        }

        /// <summary>
        /// Fire effect removed event
        /// </summary>
        protected void FireEffectRemovedEvent(EffectContext context, bool natural)
        {
            GASEvents.Trigger(GASEventType.EffectRemoved, new EffectEventData
            {
                effect = this,
                context = context,
                target = context.target,
                wasNaturalExpiration = natural,
                source = context.target
            });
        }

        /// <summary>
        /// Get or create state for target
        /// </summary>
        protected DurationEffectState GetOrCreateState(GameObject target)
        {
            if (!activeStates.TryGetValue(target, out var state))
            {
                state = new DurationEffectState();
                activeStates[target] = state;
            }
            return state;
        }

        /// <summary>
        /// Get display info for UI
        /// </summary>
        public DurationEffectDisplayInfo GetDisplayInfo(GameObject target)
        {
            if (!activeStates.TryGetValue(target, out var state))
                return null;

            return new DurationEffectDisplayInfo
            {
                effectName = effectName,
                description = description,
                icon = icon,
                color = effectColor,
                remainingTime = state.GetRemainingTime(),
                totalDuration = state.duration,
                progress = state.GetProgress(),
                stackCount = state.stackCount,
                showTimer = showTimer,
                showStacks = showStacks,
                showInUI = showInUI
            };
        }
    }

    /// <summary>
    /// Runtime state for duration effects
    /// </summary>
    [Serializable]
    public class DurationEffectState
    {
        public EffectContext context;
        public float startTime;
        public float duration;
        public float elapsedTime;
        public int stackCount = 1;
        public GameObject visualEffect;
        public List<AttributeModifier> appliedModifiers = new List<AttributeModifier>();

        public bool IsExpired()
        {
            return elapsedTime >= duration;
        }

        public float GetRemainingTime()
        {
            return Mathf.Max(0, duration - elapsedTime);
        }

        public float GetProgress()
        {
            if (duration <= 0) return 1f;
            return Mathf.Clamp01(elapsedTime / duration);
        }
    }

    /// <summary>
    /// Display info for duration effect UI
    /// </summary>
    [Serializable]
    public class DurationEffectDisplayInfo
    {
        public string effectName;
        public string description;
        public Sprite icon;
        public Color color;
        public float remainingTime;
        public float totalDuration;
        public float progress;
        public int stackCount;
        public bool showTimer;
        public bool showStacks;
        public bool showInUI;

        public string GetFormattedTime()
        {
            if (remainingTime < 0)
                return "¡Ä";

            if (remainingTime < 60)
                return $"{remainingTime:F1}s";

            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            return $"{minutes}:{seconds:D2}";
        }

        public string GetFormattedName()
        {
            if (showStacks && stackCount > 1)
                return $"{effectName} x{stackCount}";
            return effectName;
        }
    }

    /// <summary>
    /// Event data for effect events
    /// </summary>
    public class EffectEventData : GASEventData
    {
        public GameplayEffect effect;
        public EffectContext context;
        public GameObject target;
        public bool wasNaturalExpiration;
    }

    /// <summary>
    /// Event data for stack changes
    /// </summary>
    public class EffectStackEventData : GASEventData
    {
        public GameplayEffect effect;
        public GameObject target;
        public int newStackCount;
        public int previousStackCount;
    }
}