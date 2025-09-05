// ================================
// File: Assets/Scripts/GAS/EffectSystem/Types/InfiniteEffect.cs
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
    /// Effect that lasts indefinitely until removed
    /// </summary>
    [CreateAssetMenu(fileName = "InfiniteEffect", menuName = "GAS/Effects/Infinite Effect")]
    public class InfiniteEffect : GameplayEffect
    {
        [Header("Infinite Effect Settings")]
        [SerializeField] protected bool canBeDispelled = true;
        [SerializeField] protected bool persistThroughDeath = false;
        [SerializeField] protected bool persistThroughSceneChange = false;
        [SerializeField] protected float dispelResistance = 0f; // 0-100%

        [Header("Auto-Removal Conditions")]
        [SerializeField] protected TagRequirement autoRemovalCondition;
        [SerializeField] protected List<GameplayTag> removalTriggerTags = new List<GameplayTag>();
        [SerializeField] protected float conditionalCheckInterval = 1f;

        [Header("Dynamic Modifiers")]
        [SerializeField] protected bool useDynamicModifiers = false;
        [SerializeField] protected AnimationCurve modifierGrowthCurve;
        [SerializeField] protected float growthUpdateInterval = 1f;
        [SerializeField] protected float maxGrowthMultiplier = 5f;

        [Header("Aura System")]
        [SerializeField] protected bool isAuraEffect = false;
        [SerializeField] protected float auraRadius = 5f;
        [SerializeField] protected LayerMask auraTargetLayers = -1;
        [SerializeField] protected TagRequirement auraTargetRequirement;
        [SerializeField] protected GameplayEffect auraGrantedEffect;

        [Header("Passive Display")]
        [SerializeField] protected bool showAsPassive = true;
        [SerializeField] protected int displayPriority = 0;
        [SerializeField] protected Sprite passiveIcon;

        // Runtime tracking
        protected Dictionary<GameObject, InfiniteEffectState> activeStates = new Dictionary<GameObject, InfiniteEffectState>();
        protected Dictionary<GameObject, List<GameObject>> auraTargets = new Dictionary<GameObject, List<GameObject>>();

        /// <summary>
        /// Apply the infinite effect
        /// </summary>
        public override void OnApply(EffectContext context)
        {
            if (context == null || context.target == null) return;

            var state = GetOrCreateState(context.target);
            state.context = context;
            state.appliedTime = Time.time;
            state.lastGrowthUpdate = Time.time;
            state.lastConditionalCheck = Time.time;

            // Apply granted tags
            ApplyGrantedTags(context);

            // Apply initial modifiers
            ApplyInitialModifiers(context, state);

            // Setup aura if enabled
            if (isAuraEffect)
            {
                SetupAura(context.target, state);
            }

            // Spawn visual effects
            state.visualEffect = SpawnVisualEffect(context);
            if (state.visualEffect != null)
            {
                // Make visual effect persistent
                DontDestroyOnLoad(state.visualEffect);
            }

            // Play application sound
            PlayApplicationSound(context);

            // Register for persistent events
            if (!persistThroughDeath)
            {
                RegisterForDeathEvent(context.target);
            }

            if (persistThroughSceneChange)
            {
                RegisterForSceneChange(context.target);
            }

            // Fire applied event
            FireEffectAppliedEvent(context);

            Debug.Log($"[InfiniteEffect] Applied {effectName} to {context.target.name} (Infinite Duration)");
        }

        /// <summary>
        /// Remove the infinite effect
        /// </summary>
        public override void OnRemove(EffectContext context)
        {
            if (context == null || context.target == null) return;

            if (!activeStates.TryGetValue(context.target, out var state))
                return;

            // Clean up aura
            if (isAuraEffect)
            {
                CleanupAura(context.target, state);
            }

            // Remove modifiers
            RemoveModifiers(context);

            // Remove granted tags
            RemoveGrantedTags(context);

            // Clean up visual effects
            if (state.visualEffect != null)
            {
                Destroy(state.visualEffect);
            }

            // Play removal sound
            PlayRemovalSound(context);

            // Unregister from events
            if (!persistThroughDeath)
            {
                UnregisterFromDeathEvent(context.target);
            }

            if (persistThroughSceneChange)
            {
                UnregisterFromSceneChange(context.target);
            }

            // Fire removed event
            FireEffectRemovedEvent(context);

            // Remove state
            activeStates.Remove(context.target);

            Debug.Log($"[InfiniteEffect] Removed {effectName} from {context.target.name}");
        }

        /// <summary>
        /// Update tick for the infinite effect
        /// </summary>
        public override void OnTick(EffectContext context, float deltaTime)
        {
            if (context == null || context.target == null) return;

            if (!activeStates.TryGetValue(context.target, out var state))
                return;

            state.elapsedTime += deltaTime;

            // Check conditional removal
            if (Time.time - state.lastConditionalCheck >= conditionalCheckInterval)
            {
                state.lastConditionalCheck = Time.time;

                if (CheckAutoRemovalConditions(context))
                {
                    var effectComponent = context.target.GetComponent<EffectComponent>();
                    effectComponent?.RemoveEffect(this);
                    return;
                }
            }

            // Update dynamic modifiers
            if (useDynamicModifiers && Time.time - state.lastGrowthUpdate >= growthUpdateInterval)
            {
                state.lastGrowthUpdate = Time.time;
                UpdateDynamicModifiers(context, state);
            }

            // Update aura
            if (isAuraEffect)
            {
                UpdateAura(context.target, state);
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
        /// Handle periodic execution for infinite effects
        /// </summary>
        public override void OnPeriodic(EffectContext context)
        {
            if (context == null || context.target == null) return;

            if (!activeStates.TryGetValue(context.target, out var state))
                return;

            // Execute periodic logic
            ApplyPeriodicModifiers(context, state);

            // Fire periodic event
            GASEvents.Trigger(GASEventType.EffectPeriodic, new EffectPeriodicEventData
            {
                effect = this,
                context = context,
                target = context.target,
                tickCount = ++state.periodicCount,
                source = context.instigator
            });

            Debug.Log($"[InfiniteEffect] Periodic tick #{state.periodicCount} for {effectName}");
        }

        /// <summary>
        /// Handle stacking for infinite effects
        /// </summary>
        public override void OnStack(EffectContext context, int newStackCount, int previousStackCount)
        {
            if (!activeStates.TryGetValue(context.target, out var state))
                return;

            state.stackCount = newStackCount;

            // Update modifiers for new stack count
            UpdateStackModifiers(context, state, newStackCount);

            // Update aura strength if applicable
            if (isAuraEffect)
            {
                state.auraStrength = CalculateAuraStrength(newStackCount);
            }

            // Fire stack event
            GASEvents.Trigger(GASEventType.EffectStackChanged, new EffectStackEventData
            {
                effect = this,
                target = context.target,
                newStackCount = newStackCount,
                previousStackCount = previousStackCount,
                source = context.target
            });

            Debug.Log($"[InfiniteEffect] {effectName} stacked to {newStackCount}x on {context.target.name}");
        }

        /// <summary>
        /// Check if this effect can be dispelled
        /// </summary>
        public virtual bool CanBeDispelled(float dispelPower = 100f)
        {
            if (!canBeDispelled)
                return false;

            // Check dispel resistance
            if (dispelResistance > 0)
            {
                float successChance = dispelPower * (1f - dispelResistance / 100f);
                return UnityEngine.Random.Range(0f, 100f) < successChance;
            }

            return true;
        }

        /// <summary>
        /// Attempt to dispel this effect
        /// </summary>
        public virtual bool TryDispel(GameObject target, float dispelPower = 100f)
        {
            if (!CanBeDispelled(dispelPower))
            {
                Debug.Log($"[InfiniteEffect] Failed to dispel {effectName} (Resisted)");
                return false;
            }

            var effectComponent = target.GetComponent<EffectComponent>();
            if (effectComponent != null)
            {
                effectComponent.RemoveEffect(this);
                return true;
            }

            return false;
        }

        #region Modifier Management

        /// <summary>
        /// Apply initial modifiers
        /// </summary>
        protected void ApplyInitialModifiers(EffectContext context, InfiniteEffectState state)
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
                    modifier.duration = -1f; // Infinite

                    attributeComponent.AddModifier(modifier.targetAttributeType, modifier);
                    state.appliedModifiers.Add(modifier);
                }
            }
        }

        /// <summary>
        /// Update dynamic modifiers based on elapsed time
        /// </summary>
        protected void UpdateDynamicModifiers(EffectContext context, InfiniteEffectState state)
        {
            if (modifierGrowthCurve == null || modifierGrowthCurve.keys.Length == 0)
                return;

            var attributeComponent = context.target.GetComponent<AttributeSetComponent>();
            if (attributeComponent == null)
                return;

            // Calculate growth based on elapsed time
            float growthTime = state.elapsedTime / 60f; // Convert to minutes
            float growthMultiplier = Mathf.Min(modifierGrowthCurve.Evaluate(growthTime), maxGrowthMultiplier);

            // Update each modifier's value
            for (int i = 0; i < modifiers.Count && i < state.appliedModifiers.Count; i++)
            {
                var originalModifier = modifiers[i];
                var appliedModifier = state.appliedModifiers[i];

                if (originalModifier != null && appliedModifier != null)
                {
                    // Remove old modifier
                    attributeComponent.RemoveModifier(appliedModifier.targetAttributeType, appliedModifier);

                    // Update value with growth
                    appliedModifier.value = originalModifier.value * growthMultiplier;

                    // Re-add modifier
                    attributeComponent.AddModifier(appliedModifier.targetAttributeType, appliedModifier);
                }
            }

            state.currentGrowthMultiplier = growthMultiplier;
        }

        /// <summary>
        /// Apply periodic modifiers
        /// </summary>
        protected void ApplyPeriodicModifiers(EffectContext context, InfiniteEffectState state)
        {
            // Can be overridden to apply periodic damage/healing
            // This is separate from the persistent modifiers
        }

        /// <summary>
        /// Update modifiers for stack changes
        /// </summary>
        protected void UpdateStackModifiers(EffectContext context, InfiniteEffectState state, int stacks)
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
                    modifier.value *= stacks;
                    modifier.duration = -1f; // Infinite

                    // Apply growth if active
                    if (useDynamicModifiers && state.currentGrowthMultiplier > 1f)
                    {
                        modifier.value *= state.currentGrowthMultiplier;
                    }

                    attributeComponent.AddModifier(modifier.targetAttributeType, modifier);
                    state.appliedModifiers.Add(modifier);
                }
            }
        }

        #endregion

        #region Aura System

        /// <summary>
        /// Setup aura effect
        /// </summary>
        protected void SetupAura(GameObject source, InfiniteEffectState state)
        {
            if (!isAuraEffect || auraGrantedEffect == null)
                return;

            if (!auraTargets.ContainsKey(source))
            {
                auraTargets[source] = new List<GameObject>();
            }

            state.auraStrength = CalculateAuraStrength(state.stackCount);

            // Create aura visual if needed
            if (state.auraVisual == null)
            {
                state.auraVisual = CreateAuraVisual(source);
            }

            Debug.Log($"[InfiniteEffect] Aura setup for {effectName} with radius {auraRadius}");
        }

        /// <summary>
        /// Update aura effect
        /// </summary>
        protected void UpdateAura(GameObject source, InfiniteEffectState state)
        {
            if (!isAuraEffect || auraGrantedEffect == null)
                return;

            // Find targets in range
            var colliders = Physics.OverlapSphere(source.transform.position, auraRadius, auraTargetLayers);
            var currentTargets = new HashSet<GameObject>();

            foreach (var collider in colliders)
            {
                if (collider.gameObject == source) continue;

                // Check if target meets requirements
                var targetTags = collider.GetComponent<TagComponent>();
                if (auraTargetRequirement != null && !auraTargetRequirement.ignoreIfEmpty)
                {
                    if (targetTags == null || !auraTargetRequirement.CheckRequirement(targetTags))
                        continue;
                }

                currentTargets.Add(collider.gameObject);

                // Apply aura effect if new target
                if (!auraTargets[source].Contains(collider.gameObject))
                {
                    ApplyAuraEffect(collider.gameObject, state);
                    auraTargets[source].Add(collider.gameObject);
                }
            }

            // Remove aura from targets that left range
            var targetsToRemove = new List<GameObject>();
            foreach (var target in auraTargets[source])
            {
                if (!currentTargets.Contains(target))
                {
                    RemoveAuraEffect(target, state);
                    targetsToRemove.Add(target);
                }
            }

            foreach (var target in targetsToRemove)
            {
                auraTargets[source].Remove(target);
            }

            // Update aura visual
            UpdateAuraVisual(state);
        }

        /// <summary>
        /// Apply aura effect to target
        /// </summary>
        protected void ApplyAuraEffect(GameObject target, InfiniteEffectState state)
        {
            var effectComponent = target.GetComponent<EffectComponent>();
            if (effectComponent == null) return;

            var auraContext = state.context.Clone();
            auraContext.target = target;
            auraContext.sourceEffect = this;
            auraContext.magnitude = state.auraStrength;

            effectComponent.ApplyEffect(auraGrantedEffect, auraContext);

            Debug.Log($"[InfiniteEffect] Applied aura effect to {target.name}");
        }

        /// <summary>
        /// Remove aura effect from target
        /// </summary>
        protected void RemoveAuraEffect(GameObject target, InfiniteEffectState state)
        {
            var effectComponent = target.GetComponent<EffectComponent>();
            if (effectComponent == null) return;

            effectComponent.RemoveEffect(auraGrantedEffect);

            Debug.Log($"[InfiniteEffect] Removed aura effect from {target.name}");
        }

        /// <summary>
        /// Clean up aura
        /// </summary>
        protected void CleanupAura(GameObject source, InfiniteEffectState state)
        {
            if (!auraTargets.ContainsKey(source))
                return;

            // Remove aura from all targets
            foreach (var target in auraTargets[source])
            {
                RemoveAuraEffect(target, state);
            }

            auraTargets[source].Clear();
            auraTargets.Remove(source);

            // Destroy aura visual
            if (state.auraVisual != null)
            {
                Destroy(state.auraVisual);
            }
        }

        /// <summary>
        /// Create aura visual effect
        /// </summary>
        protected GameObject CreateAuraVisual(GameObject source)
        {
            // Create a simple sphere to represent aura radius (can be replaced with actual VFX)
            var auraVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            auraVisual.name = $"{effectName}_AuraVisual";
            auraVisual.transform.SetParent(source.transform);
            auraVisual.transform.localPosition = Vector3.zero;
            auraVisual.transform.localScale = Vector3.one * (auraRadius * 2f);

            // Make it transparent and non-collidable
            var renderer = auraVisual.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = renderer.material;
                material.color = new Color(effectColor.r, effectColor.g, effectColor.b, 0.2f);
                renderer.material = material;
            }

            var collider = auraVisual.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            return auraVisual;
        }

        /// <summary>
        /// Update aura visual
        /// </summary>
        protected void UpdateAuraVisual(InfiniteEffectState state)
        {
            if (state.auraVisual == null) return;

            // Pulse effect based on strength
            float pulse = Mathf.Sin(Time.time * 2f) * 0.1f + 1f;
            state.auraVisual.transform.localScale = Vector3.one * (auraRadius * 2f * pulse);
        }

        /// <summary>
        /// Calculate aura strength based on stacks
        /// </summary>
        protected float CalculateAuraStrength(int stacks)
        {
            return 1f + (stacks - 1) * 0.25f; // 25% increase per stack
        }

        #endregion

        #region Conditional Checks

        /// <summary>
        /// Check auto-removal conditions
        /// </summary>
        protected bool CheckAutoRemovalConditions(EffectContext context)
        {
            // Check tag-based removal
            if (removalTriggerTags != null && removalTriggerTags.Count > 0)
            {
                var tagComponent = context.target.GetComponent<TagComponent>();
                if (tagComponent != null)
                {
                    foreach (var tag in removalTriggerTags)
                    {
                        if (tagComponent.HasTag(tag))
                        {
                            Debug.Log($"[InfiniteEffect] Auto-removing {effectName} due to tag {tag.TagName}");
                            return true;
                        }
                    }
                }
            }

            // Check conditional removal
            if (autoRemovalCondition != null && !autoRemovalCondition.IgnoreIfEmpty)
            {
                var tagComponent = context.target.GetComponent<TagComponent>();
                if (tagComponent != null && autoRemovalCondition.CheckRequirement(tagComponent))
                {
                    Debug.Log($"[InfiniteEffect] Auto-removing {effectName} due to condition");
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Register for death events
        /// </summary>
        protected void RegisterForDeathEvent(GameObject target)
        {
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
            if (persistThroughDeath)
                return;

            if (data is GASEventData eventData && activeStates.ContainsKey(eventData.source))
            {
                var effectComponent = eventData.source.GetComponent<EffectComponent>();
                effectComponent?.RemoveEffect(this);
            }
        }

        /// <summary>
        /// Register for scene changes
        /// </summary>
        protected void RegisterForSceneChange(GameObject target)
        {
            // Would subscribe to scene change events
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        /// <summary>
        /// Unregister from scene changes
        /// </summary>
        protected void UnregisterFromSceneChange(GameObject target)
        {
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// Handle scene unload
        /// </summary>
        protected void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            if (!persistThroughSceneChange)
            {
                // Remove from all targets
                var targetsToRemove = new List<GameObject>(activeStates.Keys);
                foreach (var target in targetsToRemove)
                {
                    if (target != null)
                    {
                        var effectComponent = target.GetComponent<EffectComponent>();
                        effectComponent?.RemoveEffect(this);
                    }
                }
            }
        }

        #endregion

        #region Visual Updates

        /// <summary>
        /// Update visual effect
        /// </summary>
        protected void UpdateVisualEffect(InfiniteEffectState state)
        {
            if (state.visualEffect == null)
                return;

            // Update particle effects based on growth/stacks
            var particleSystems = state.visualEffect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var emission = ps.emission;
                emission.rateOverTime = 5f * state.stackCount * state.currentGrowthMultiplier;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Fire effect applied event
        /// </summary>
        protected void FireEffectAppliedEvent(EffectContext context)
        {
            GASEvents.Trigger(GASEventType.EffectApplied, new InfiniteEffectEventData
            {
                effect = this,
                context = context,
                target = context.target,
                isPassive = showAsPassive,
                source = context.target
            });
        }

        /// <summary>
        /// Fire effect removed event
        /// </summary>
        protected void FireEffectRemovedEvent(EffectContext context)
        {
            GASEvents.Trigger(GASEventType.EffectRemoved, new InfiniteEffectEventData
            {
                effect = this,
                context = context,
                target = context.target,
                isPassive = showAsPassive,
                source = context.target
            });
        }

        #endregion

        #region State Management

        /// <summary>
        /// Get or create state for target
        /// </summary>
        protected InfiniteEffectState GetOrCreateState(GameObject target)
        {
            if (!activeStates.TryGetValue(target, out var state))
            {
                state = new InfiniteEffectState();
                activeStates[target] = state;
            }
            return state;
        }

        #endregion

        #region Display Info

        /// <summary>
        /// Get display info for UI
        /// </summary>
        public InfiniteEffectDisplayInfo GetDisplayInfo(GameObject target)
        {
            if (!activeStates.TryGetValue(target, out var state))
                return null;

            return new InfiniteEffectDisplayInfo
            {
                effectName = effectName,
                description = description,
                icon = passiveIcon ?? icon,
                color = effectColor,
                stackCount = state.stackCount,
                isPassive = showAsPassive,
                isAura = isAuraEffect,
                auraRadius = auraRadius,
                growthMultiplier = state.currentGrowthMultiplier,
                displayPriority = displayPriority,
                canBeDispelled = canBeDispelled,
                dispelResistance = dispelResistance
            };
        }

        #endregion
    }

    /// <summary>
    /// Runtime state for infinite effects
    /// </summary>
    [Serializable]
    public class InfiniteEffectState
    {
        public EffectContext context;
        public float appliedTime;
        public float elapsedTime;
        public int stackCount = 1;
        public int periodicCount = 0;
        public GameObject visualEffect;
        public GameObject auraVisual;
        public List<AttributeModifier> appliedModifiers = new List<AttributeModifier>();

        public float lastGrowthUpdate;
        public float lastConditionalCheck;
        public float currentGrowthMultiplier = 1f;
        public float auraStrength = 1f;

        public float GetElapsedTime()
        {
            return elapsedTime;
        }
    }

    /// <summary>
    /// Display info for infinite effect UI
    /// </summary>
    [Serializable]
    public class InfiniteEffectDisplayInfo
    {
        public string effectName;
        public string description;
        public Sprite icon;
        public Color color;
        public int stackCount;
        public bool isPassive;
        public bool isAura;
        public float auraRadius;
        public float growthMultiplier;
        public int displayPriority;
        public bool canBeDispelled;
        public float dispelResistance;

        public string GetFormattedName()
        {
            string name = effectName;

            if (stackCount > 1)
                name += $" x{stackCount}";

            if (growthMultiplier > 1f)
                name += $" ({growthMultiplier:F1}x)";

            return name;
        }

        public string GetFormattedDescription()
        {
            string desc = description;

            if (isAura)
                desc += $"\nAura Radius: {auraRadius}m";

            if (!canBeDispelled)
                desc += "\n[Cannot be dispelled]";
            else if (dispelResistance > 0)
                desc += $"\n[{dispelResistance}% Dispel Resistance]";

            return desc;
        }
    }

    /// <summary>
    /// Event data for infinite effects
    /// </summary>
    public class InfiniteEffectEventData : GASEventData
    {
        public GameplayEffect effect;
        public EffectContext context;
        public GameObject target;
        public bool isPassive;
    }

}