// ================================
// File: Assets/Scripts/GAS/EffectSystem/Types/PeriodicEffect.cs
// ================================
using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.Core;
using GAS.TagSystem;
using GAS.AttributeSystem;
using static GAS.Core.GASConstants;
using System.Linq;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Effect that executes periodically over time
    /// </summary>
    [CreateAssetMenu(fileName = "PeriodicEffect", menuName = "GAS/Effects/Periodic Effect")]
    public class PeriodicEffect : GameplayEffect
    {
        [Header("Periodic Execution")]
        [SerializeField] protected int maxTicks = -1; // -1 for unlimited
        [SerializeField] protected bool executeOnFirstApplication = true;
        [SerializeField] protected bool executeOnLastTick = true;
        [SerializeField] protected bool scaleTickWithStack = true;

        [Header("Tick Modifiers")]
        [SerializeField] protected List<AttributeModification> tickModifiers = new List<AttributeModification>();
        [SerializeField] protected AnimationCurve tickDamageCurve;

        [Header("Tick Effects")]
        [SerializeField] protected List<GameplayEffect> onTickEffects = new List<GameplayEffect>();
        [SerializeField] protected bool useRandomTickEffect = false;

        [Header("Visual & Audio")]
        [SerializeField] protected GameObject tickVisualPrefab;
        [SerializeField] protected AudioClip tickSound;
        [SerializeField] protected float tickVisualDuration = 0.5f;

        [Header("Variable Period")]
        [SerializeField] protected bool variablePeriod = false;
        [SerializeField] protected float periodVariance = 0.1f; // 10% variance
        [SerializeField] protected bool accelerateOverTime = false;
        [SerializeField] protected float accelerationRate = 0.9f; // Multiplier per tick

        [Header("UI Display")]
        [SerializeField] protected bool showInUI = true;
        [SerializeField] protected Color effectColor = Color.white;

        // Runtime tracking
        protected Dictionary<GameObject, PeriodicEffectState> activeStates = new Dictionary<GameObject, PeriodicEffectState>();

        /// <summary>
        /// Apply the periodic effect
        /// </summary>
        public override void OnApply(EffectContext context)
        {
            if (context == null || context.target == null) return;

            var state = GetOrCreateState(context.target);
            state.context = context;
            state.startTime = Time.time;
            state.lastTickTime = Time.time;
            state.currentPeriod = CalculateInitialPeriod(context);
            state.duration = CalculateActualDuration(context);
            state.maxTicksLimit = maxTicks;

            // Apply granted tags
            ApplyGrantedTags(context);

            // Apply persistent modifiers (if any)
            ApplyPersistentModifiers(context, state);

            // Execute first tick if configured
            if (executeOnFirstApplication)
            {
                ExecuteTick(context, state, true);
                state.tickCount++;
            }

            // Spawn visual effects
            state.visualEffect = SpawnVisualEffect(context);

            // Play application sound
            PlayApplicationSound(context);

            // Fire applied event
            FireEffectAppliedEvent(context);

            Debug.Log($"[PeriodicEffect] Applied {effectName} to {context.target.name} (Period: {state.currentPeriod}s)");
        }

        /// <summary>
        /// Remove the periodic effect
        /// </summary>
        public override void OnRemove(EffectContext context)
        {
            if (context == null || context.target == null) return;

            if (!activeStates.TryGetValue(context.target, out var state))
                return;

            // Execute last tick if configured and not at max ticks
            if (executeOnLastTick && !state.HasReachedMaxTicks())
            {
                ExecuteTick(context, state, false, true);
            }

            // Remove persistent modifiers
            RemovePersistentModifiers(context, state);

            // Remove granted tags
            RemoveGrantedTags(context);

            // Clean up visual effects
            if (state.visualEffect != null)
            {
                Destroy(state.visualEffect);
            }

            // Clean up tick visuals
            CleanupTickVisuals(state);

            // Play removal sound
            PlayRemovalSound(context);

            // Fire removed event
            FireEffectRemovedEvent(context);

            // Remove state
            activeStates.Remove(context.target);

            Debug.Log($"[PeriodicEffect] Removed {effectName} from {context.target.name} (Total ticks: {state.tickCount})");
        }

        /// <summary>
        /// Update tick for the periodic effect
        /// </summary>
        public override void OnTick(EffectContext context, float deltaTime)
        {
            if (context == null || context.target == null) return;

            if (!activeStates.TryGetValue(context.target, out var state))
                return;

            state.elapsedTime += deltaTime;

            // Check if duration expired
            if (state.duration > 0 && state.elapsedTime >= state.duration)
            {
                var effectComponent = context.target.GetComponent<EffectComponent>();
                effectComponent?.RemoveEffect(this);
                return;
            }

            // Check if periodic execution is due
            float timeSinceLastTick = Time.time - state.lastTickTime;
            if (timeSinceLastTick >= state.currentPeriod)
            {
                // Check max ticks
                if (state.HasReachedMaxTicks())
                {
                    var effectComponent = context.target.GetComponent<EffectComponent>();
                    effectComponent?.RemoveEffect(this);
                    return;
                }

                // Execute periodic tick
                ExecuteTick(context, state, false);
                state.lastTickTime = Time.time;
                state.tickCount++;

                // Update period for next tick
                UpdatePeriod(state);
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
        /// Handle periodic execution
        /// </summary>
        public override void OnPeriodic(EffectContext context)
        {
            // This is called by the base system, but we handle periodics in OnTick
            // Can be used for additional periodic logic if needed
        }

        /// <summary>
        /// Handle stacking
        /// </summary>
        public override void OnStack(EffectContext context, int newStackCount, int previousStackCount)
        {
            if (!activeStates.TryGetValue(context.target, out var state))
                return;

            state.stackCount = newStackCount;

            // Refresh duration if configured
            if (refreshDurationOnStack)
            {
                state.startTime = Time.time;
                state.elapsedTime = 0;
            }

            // Reset periodic if configured
            if (resetPeriodicOnStack)
            {
                state.lastTickTime = Time.time;
                state.currentPeriod = CalculateInitialPeriod(context);
            }

            // Update persistent modifiers for new stack count
            UpdateStackModifiers(context, state, newStackCount);

            // Fire stack event
            FireStackEvent(context, newStackCount, previousStackCount);

            Debug.Log($"[PeriodicEffect] {effectName} stacked to {newStackCount}x on {context.target.name}");
        }

        #region Tick Execution

        /// <summary>
        /// Execute a periodic tick
        /// </summary>
        protected virtual void ExecuteTick(EffectContext context, PeriodicEffectState state, bool isFirst = false, bool isLast = false)
        {
            // Calculate tick strength
            float tickStrength = CalculateTickStrength(state, isFirst, isLast);

            // Apply tick modifiers
            ApplyTickModifiers(context, state, tickStrength);

            // Execute tick effects
            ExecuteTickEffects(context, state);

            // Spawn tick visual
            SpawnTickVisual(context, state);

            // Play tick sound
            PlayTickSound(context);

            // Fire periodic event
            FirePeriodicEvent(context, state);

            Debug.Log($"[PeriodicEffect] Tick #{state.tickCount} executed for {effectName} (Strength: {tickStrength:F2})");
        }

        /// <summary>
        /// Apply modifiers for this tick
        /// </summary>
        protected virtual void ApplyTickModifiers(EffectContext context, PeriodicEffectState state, float strength)
        {
            if (tickModifiers == null || tickModifiers.Count == 0)
                return;

            var attributeComponent = context.target.GetComponent<AttributeSetComponent>();
            if (attributeComponent == null)
                return;

            foreach (var modification in tickModifiers)
            {
                if (modification == null) continue;

                float value = modification.CalculateValue(context.level);

                // Scale with stack count if configured
                if (scaleTickWithStack && state.stackCount > 1)
                {
                    value *= state.stackCount;
                }

                // Apply strength multiplier
                value *= strength;

                // Apply tick damage curve if available
                if (tickDamageCurve != null && tickDamageCurve.keys.Length > 0)
                {
                    float progress = state.GetProgress();
                    value *= tickDamageCurve.Evaluate(progress);
                }

                // Apply the modification
                ApplyAttributeModification(attributeComponent, modification, value, context);
            }
        }

        /// <summary>
        /// Apply a single attribute modification
        /// </summary>
        protected virtual void ApplyAttributeModification(
            AttributeSetComponent attributes,
            AttributeModification modification,
            float value,
            EffectContext context)
        {
            // Special handling for health
            if (modification.attributeType == AttributeType.Health)
            {
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
                attributes.ModifyAttribute(
                    modification.attributeType,
                    value,
                    modification.operation
                );
            }
        }

        /// <summary>
        /// Apply damage with processing
        /// </summary>
        protected virtual void ApplyDamage(AttributeSetComponent attributes, float damage, EffectContext context)
        {
            // Apply damage
            attributes.ModifyAttribute(AttributeType.Health, -damage, ModifierOperation.Add);

            // Fire damage event
            GASEvents.Trigger(GASEventType.DamageReceived, new DamageEventData
            {
                source = context.instigator,
                target = context.target,
                damage = damage,
                isCritical = context.isCritical,
                damageType = GetDamageType()
            });
        }

        /// <summary>
        /// Apply healing with processing
        /// </summary>
        protected virtual void ApplyHealing(AttributeSetComponent attributes, float healing, EffectContext context)
        {
            float currentHealth = attributes.GetAttributeValue(AttributeType.Health);
            float maxHealth = attributes.GetAttributeMaxValue(AttributeType.Health);
            float actualHealing = Mathf.Min(healing, maxHealth - currentHealth);

            attributes.ModifyAttribute(AttributeType.Health, actualHealing, ModifierOperation.Add);

            // Fire healing event
            GASEvents.Trigger(GASEventType.HealingReceived, new HealingEventData
            {
                source = context.instigator,
                target = context.target,
                healing = actualHealing,
                isCritical = context.isCritical
            });
        }

        /// <summary>
        /// Execute tick effects
        /// </summary>
        protected virtual void ExecuteTickEffects(EffectContext context, PeriodicEffectState state)
        {
            if (onTickEffects == null || onTickEffects.Count == 0)
                return;

            var effectComponent = context.target.GetComponent<EffectComponent>();
            if (effectComponent == null)
                return;

            if (useRandomTickEffect && onTickEffects.Count > 0)
            {
                // Apply random effect
                int randomIndex = UnityEngine.Random.Range(0, onTickEffects.Count);
                var randomEffect = onTickEffects[randomIndex];
                if (randomEffect != null)
                {
                    var tickContext = CreateTickContext(context, state);
                    effectComponent.ApplyEffect(randomEffect, tickContext);
                }
            }
            else
            {
                // Apply all effects
                foreach (var effect in onTickEffects)
                {
                    if (effect != null)
                    {
                        var tickContext = CreateTickContext(context, state);
                        effectComponent.ApplyEffect(effect, tickContext);
                    }
                }
            }
        }

        /// <summary>
        /// Create context for tick effects
        /// </summary>
        protected virtual EffectContext CreateTickContext(EffectContext originalContext, PeriodicEffectState state)
        {
            var tickContext = originalContext.Clone();
            tickContext.sourceEffect = this;
            tickContext.customData = new PeriodicTickData
            {
                tickNumber = state.tickCount,
                tickStrength = CalculateTickStrength(state),
                isLastTick = state.HasReachedMaxTicks()
            };
            return tickContext;
        }

        /// <summary>
        /// Calculate tick strength
        /// </summary>
        protected virtual float CalculateTickStrength(PeriodicEffectState state, bool isFirst = false, bool isLast = false)
        {
            float strength = 1f;

            // Apply stack multiplier
            if (scaleTickWithStack && state.stackCount > 1)
            {
                strength *= state.stackCount;
            }

            // Apply tick curve
            if (tickDamageCurve != null && tickDamageCurve.keys.Length > 0)
            {
                float progress = state.GetProgress();
                strength *= tickDamageCurve.Evaluate(progress);
            }

            // Special multipliers for first/last tick
            if (isFirst)
            {
                strength *= 0.5f; // Half damage on first tick
            }
            else if (isLast)
            {
                strength *= 1.5f; // Bonus damage on last tick
            }

            return strength;
        }

        #endregion

        #region Persistent Modifiers

        /// <summary>
        /// Apply persistent modifiers that last for the effect duration
        /// </summary>
        protected virtual void ApplyPersistentModifiers(EffectContext context, PeriodicEffectState state)
        {
            if (modifiers == null || modifiers.Count == 0)
                return;

            var attributeComponent = context.target.GetComponent<AttributeSetComponent>();
            if (attributeComponent == null)
                return;

            state.persistentModifiers.Clear();

            foreach (var modifierTemplate in modifiers)
            {
                if (modifierTemplate != null)
                {
                    var modifier = modifierTemplate.Clone();
                    modifier.source = this;
                    modifier.sourceName = effectName;

                    attributeComponent.AddModifier(modifier.targetAttributeType, modifier);
                    state.persistentModifiers.Add(modifier);
                }
            }
        }

        /// <summary>
        /// Remove persistent modifiers
        /// </summary>
        protected virtual void RemovePersistentModifiers(EffectContext context, PeriodicEffectState state)
        {
            var attributeComponent = context.target.GetComponent<AttributeSetComponent>();
            if (attributeComponent == null)
                return;

            foreach (var modifier in state.persistentModifiers)
            {
                if (modifier != null)
                {
                    attributeComponent.RemoveModifier(modifier.targetAttributeType, modifier);
                }
            }

            state.persistentModifiers.Clear();
        }

        /// <summary>
        /// Update modifiers for stack changes
        /// </summary>
        protected virtual void UpdateStackModifiers(EffectContext context, PeriodicEffectState state, int stacks)
        {
            var attributeComponent = context.target.GetComponent<AttributeSetComponent>();
            if (attributeComponent == null)
                return;

            // Remove old modifiers
            foreach (var modifier in state.persistentModifiers)
            {
                if (modifier != null)
                {
                    attributeComponent.RemoveModifier(modifier.targetAttributeType, modifier);
                }
            }

            state.persistentModifiers.Clear();

            // Apply new modifiers with stack multiplier
            foreach (var modifierTemplate in modifiers)
            {
                if (modifierTemplate != null)
                {
                    var modifier = modifierTemplate.Clone();
                    modifier.source = this;
                    modifier.sourceName = effectName;
                    modifier.value *= stacks;

                    attributeComponent.AddModifier(modifier.targetAttributeType, modifier);
                    state.persistentModifiers.Add(modifier);
                }
            }
        }

        #endregion

        #region Period Management

        /// <summary>
        /// Calculate initial period
        /// </summary>
        protected virtual float CalculateInitialPeriod(EffectContext context)
        {
            float initialPeriod = period;

            // Apply context multiplier
            initialPeriod *= context.periodMultiplier;

            // Apply variance if configured
            if (variablePeriod && periodVariance > 0)
            {
                float variance = UnityEngine.Random.Range(-periodVariance, periodVariance);
                initialPeriod *= (1f + variance);
            }

            return Mathf.Max(0.1f, initialPeriod);
        }

        /// <summary>
        /// Update period for next tick
        /// </summary>
        protected virtual void UpdatePeriod(PeriodicEffectState state)
        {
            // Apply acceleration if configured
            if (accelerateOverTime && accelerationRate > 0 && accelerationRate < 1)
            {
                state.currentPeriod *= accelerationRate;
                state.currentPeriod = Mathf.Max(0.1f, state.currentPeriod);
            }

            // Apply variance for next tick
            if (variablePeriod && periodVariance > 0)
            {
                float basePeriod = state.currentPeriod / (1f + state.lastVariance); // Remove last variance
                float variance = UnityEngine.Random.Range(-periodVariance, periodVariance);
                state.currentPeriod = basePeriod * (1f + variance);
                state.lastVariance = variance;
            }
        }

        /// <summary>
        /// Calculate actual duration
        /// </summary>
        protected virtual float CalculateActualDuration(EffectContext context)
        {
            if (durationPolicy == DurationPolicy.Infinite)
                return -1f;

            if (durationPolicy == DurationPolicy.Instant)
                return 0f;

            float actualDuration = duration;

            // Apply context multiplier
            actualDuration *= context.durationMultiplier;

            // If max ticks is set, calculate duration based on ticks
            if (maxTicks > 0)
            {
                float tickDuration = maxTicks * CalculateInitialPeriod(context);
                actualDuration = Mathf.Min(actualDuration, tickDuration);
            }

            return actualDuration;
        }

        #endregion

        #region Visual & Audio

        /// <summary>
        /// Spawn tick visual effect
        /// </summary>
        protected virtual void SpawnTickVisual(EffectContext context, PeriodicEffectState state)
        {
            if (tickVisualPrefab == null)
                return;

            Vector3 spawnPosition = context.hitLocation != Vector3.zero ?
                context.hitLocation : context.target.transform.position;

            var tickVisual = Instantiate(tickVisualPrefab, spawnPosition, Quaternion.identity);

            // Add to tracking for cleanup
            state.tickVisuals.Add(new TickVisualInfo
            {
                visual = tickVisual,
                spawnTime = Time.time,
                duration = tickVisualDuration
            });

            // Auto destroy
            Destroy(tickVisual, tickVisualDuration);
        }

        /// <summary>
        /// Play tick sound
        /// </summary>
        protected virtual void PlayTickSound(EffectContext context)
        {
            if (tickSound == null)
                return;

            AudioSource.PlayClipAtPoint(tickSound, context.target.transform.position);
        }

        /// <summary>
        /// Update visual effect
        /// </summary>
        protected virtual void UpdateVisualEffect(PeriodicEffectState state)
        {
            if (state.visualEffect == null)
                return;

            // Pulse effect based on time until next tick
            float timeUntilNextTick = state.currentPeriod - (Time.time - state.lastTickTime);
            float pulseProgress = 1f - (timeUntilNextTick / state.currentPeriod);

            // Update particle systems
            var particleSystems = state.visualEffect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var emission = ps.emission;
                emission.rateOverTime = 5f + (10f * pulseProgress);
            }
        }

        /// <summary>
        /// Clean up tick visuals
        /// </summary>
        protected virtual void CleanupTickVisuals(PeriodicEffectState state)
        {
            foreach (var tickVisual in state.tickVisuals)
            {
                if (tickVisual.visual != null)
                {
                    Destroy(tickVisual.visual);
                }
            }
            state.tickVisuals.Clear();
        }

        #endregion

        #region Events

        /// <summary>
        /// Fire effect applied event
        /// </summary>
        protected virtual void FireEffectAppliedEvent(EffectContext context)
        {
            GASEvents.Trigger(GASEventType.EffectApplied, new EffectAppliedEventData
            {
                effect = this,
                context = context,
                target = context.target,
                source = context.instigator
            });
        }

        /// <summary>
        /// Fire effect removed event
        /// </summary>
        protected virtual void FireEffectRemovedEvent(EffectContext context)
        {
            GASEvents.Trigger(GASEventType.EffectRemoved, new EffectRemovedEventData
            {
                effect = this,
                context = context,
                target = context.target,
                source = context.instigator
            });
        }

        /// <summary>
        /// Fire periodic event
        /// </summary>
        protected virtual void FirePeriodicEvent(EffectContext context, PeriodicEffectState state)
        {
            GASEvents.Trigger(GASEventType.EffectPeriodic, new EffectPeriodicEventData
            {
                effect = this,
                context = context,
                target = context.target,
                tickCount = state.tickCount,
                source = context.instigator
            });
        }

        /// <summary>
        /// Fire stack event
        /// </summary>
        protected virtual void FireStackEvent(EffectContext context, int newStack, int oldStack)
        {
            GASEvents.Trigger(GASEventType.EffectStackChanged, new EffectStackEventData
            {
                effect = this,
                target = context.target,
                newStackCount = newStack,
                previousStackCount = oldStack,
                source = context.instigator
            });
        }

        #endregion

        #region State Management

        /// <summary>
        /// Get or create state for target
        /// </summary>
        protected PeriodicEffectState GetOrCreateState(GameObject target)
        {
            if (!activeStates.TryGetValue(target, out var state))
            {
                state = new PeriodicEffectState();
                activeStates[target] = state;
            }
            return state;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Get damage type based on tags
        /// </summary>
        protected virtual DamageType GetDamageType()
        {
            if (assetTags?.Tags.Contains<GameplayTag>("Physical") == true)
                return DamageType.Physical;
            if (assetTags?.Tags.Contains<GameplayTag>("Magic") == true)
                return DamageType.Magic;
            if (assetTags?.Tags.Contains<GameplayTag>("True") == true)
                return DamageType.True;
            return DamageType.Physical;
        }

        #endregion

        #region Display Info

        /// <summary>
        /// Get display info for UI
        /// </summary>
        public PeriodicEffectDisplayInfo GetDisplayInfo(GameObject target)
        {
            if (!activeStates.TryGetValue(target, out var state))
                return null;

            return new PeriodicEffectDisplayInfo
            {
                effectName = effectName,
                description = description,
                icon = icon,
                color = effectColor,
                tickCount = state.tickCount,
                maxTicks = state.maxTicksLimit,
                currentPeriod = state.currentPeriod,
                timeUntilNextTick = state.currentPeriod - (Time.time - state.lastTickTime),
                remainingDuration = state.GetRemainingDuration(),
                stackCount = state.stackCount,
                showInUI = showInUI
            };
        }

        #endregion
    }

    /// <summary>
    /// Runtime state for periodic effects
    /// </summary>
    [Serializable]
    public class PeriodicEffectState
    {
        public EffectContext context;
        public float startTime;
        public float elapsedTime;
        public float duration;
        public float lastTickTime;
        public float currentPeriod;
        public float lastVariance;
        public int tickCount = 0;
        public int stackCount = 1;
        public int maxTicksLimit = -1;
        public GameObject visualEffect;
        public List<AttributeModifier> persistentModifiers = new List<AttributeModifier>();
        public List<TickVisualInfo> tickVisuals = new List<TickVisualInfo>();

        public bool HasReachedMaxTicks()
        {
            return maxTicksLimit > 0 && tickCount >= maxTicksLimit;
        }

        public float GetRemainingDuration()
        {
            if (duration < 0) return -1f;
            return Mathf.Max(0, duration - elapsedTime);
        }

        public float GetProgress()
        {
            if (duration <= 0) return 0f;
            return Mathf.Clamp01(elapsedTime / duration);
        }
    }

    /// <summary>
    /// Info for tick visuals
    /// </summary>
    [Serializable]
    public class TickVisualInfo
    {
        public GameObject visual;
        public float spawnTime;
        public float duration;
    }

    /// <summary>
    /// Data passed to tick effects
    /// </summary>
    [Serializable]
    public class PeriodicTickData
    {
        public int tickNumber;
        public float tickStrength;
        public bool isLastTick;
    }

    /// <summary>
    /// Display info for periodic effect UI
    /// </summary>
    [Serializable]
    public class PeriodicEffectDisplayInfo
    {
        public string effectName;
        public string description;
        public Sprite icon;
        public Color color;
        public int tickCount;
        public int maxTicks;
        public float currentPeriod;
        public float timeUntilNextTick;
        public float remainingDuration;
        public int stackCount;
        public bool showInUI;

        public string GetFormattedName()
        {
            string name = effectName;

            if (stackCount > 1)
                name += $" x{stackCount}";

            if (maxTicks > 0)
                name += $" ({tickCount}/{maxTicks})";

            return name;
        }

        public string GetTickProgress()
        {
            if (maxTicks > 0)
                return $"Tick {tickCount}/{maxTicks}";
            return $"Tick {tickCount}";
        }

        public string GetNextTickTime()
        {
            return $"Next: {timeUntilNextTick:F1}s";
        }
    }
}