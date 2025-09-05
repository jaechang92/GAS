// ================================
// File: Assets/Scripts/GAS/EffectSystem/EffectComponent.cs
// ================================
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GAS.Core;
using GAS.TagSystem;
using GAS.AttributeSystem;
using static GAS.Core.GASConstants;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Component that manages effects on a GameObject
    /// </summary>
    public class EffectComponent : MonoBehaviour, IEffectReceiver, IAdvancedEffectReceiver
    {
        [Header("Configuration")]
        [SerializeField] private bool canReceiveEffects = true;
        [SerializeField] private bool debugMode = false;

        [Header("Immunities")]
        [SerializeField] private List<GameplayTag> immunityTags = new List<GameplayTag>();
        [SerializeField] private float globalEffectResistance = 0f; // 0-100%

        [Header("Limits")]
        [SerializeField] private int maxActiveEffects = 50;
        [SerializeField] private int maxBuffs = 20;
        [SerializeField] private int maxDebuffs = 20;

        [Header("Runtime Info - Read Only")]
        [SerializeField] private List<EffectInstance> activeEffects = new List<EffectInstance>();
        [SerializeField] private int activeBuffCount;
        [SerializeField] private int activeDebuffCount;

        // Component references
        private TagComponent tagComponent;
        private AttributeSetComponent attributeComponent;

        // Effect tracking
        private Dictionary<GameplayEffect, EffectInstance> effectLookup = new Dictionary<GameplayEffect, EffectInstance>();
        private Dictionary<string, List<EffectInstance>> effectsByTag = new Dictionary<string, List<EffectInstance>>();
        private List<EffectInstance> periodicEffects = new List<EffectInstance>();
        private List<EffectInstance> effectsToRemove = new List<EffectInstance>();

        // Events
        public event Action<GameplayEffect, EffectContext> EffectApplied;
        public event Action<GameplayEffect, EffectContext> EffectRemoved;
        public event Action<GameplayEffect, int> EffectStacked;
        public event Action<GameplayEffect> EffectExpired;
        public event Action<GameplayEffect> EffectBlocked;

        #region IEffectReceiver Implementation

        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public bool CanReceiveEffects => canReceiveEffects && isActiveAndEnabled;
        public bool IsImmune => false; // Can be overridden based on state

        public void OnPreEffectApply(GameplayEffect effect, EffectContext context)
        {
            if (debugMode)
                Debug.Log($"[EffectComponent] Pre-applying {effect.EffectName} to {gameObject.name}");
        }

        public void OnPostEffectApply(GameplayEffect effect, EffectContext context)
        {
            if (debugMode)
                Debug.Log($"[EffectComponent] Post-applied {effect.EffectName} to {gameObject.name}");

            EffectApplied?.Invoke(effect, context);
        }

        public void OnPreEffectRemove(GameplayEffect effect, EffectContext context)
        {
            if (debugMode)
                Debug.Log($"[EffectComponent] Pre-removing {effect.EffectName} from {gameObject.name}");
        }

        public void OnPostEffectRemove(GameplayEffect effect, EffectContext context)
        {
            if (debugMode)
                Debug.Log($"[EffectComponent] Post-removed {effect.EffectName} from {gameObject.name}");

            EffectRemoved?.Invoke(effect, context);
        }

        public List<string> GetImmunityTags()
        {
            return immunityTags.Select(t => t.TagName).ToList();
        }

        public bool IsImmuneToEffect(GameplayEffect effect)
        {
            if (effect == null) return true;

            // Check immunity tags
            if (effect.AssetTags != null)
            {
                foreach (var effectTag in effect.AssetTags.Tags)
                {
                    if (immunityTags.Any(immuneTag => immuneTag.TagName == effectTag.TagName))
                    {
                        if (debugMode)
                            Debug.Log($"[EffectComponent] Immune to {effect.EffectName} due to tag {effectTag.TagName}");
                        return true;
                    }
                }
            }

            return false;
        }

        public float GetEffectResistance(GameplayEffect effect)
        {
            float resistance = globalEffectResistance;

            // Add specific resistances based on effect type
            if (attributeComponent != null)
            {
                if (effect.AssetTags?.Tags.Any(t => t.TagName.Contains("Magic")) == true)
                {
                    resistance += attributeComponent.GetAttributeValue(AttributeType.MagicResistance);
                }
                else if (effect.AssetTags?.Tags.Any(t => t.TagName.Contains("Physical")) == true)
                {
                    resistance += attributeComponent.GetAttributeValue(AttributeType.Armor) * 0.5f;
                }
            }

            return Mathf.Clamp(resistance, 0, 100);
        }

        public void ModifyIncomingEffect(GameplayEffect effect, ref EffectContext context)
        {
            // Apply resistance to duration
            float resistance = GetEffectResistance(effect) / 100f;
            context.durationMultiplier *= (1f - resistance * 0.5f); // 50% max duration reduction
            context.magnitude *= (1f - resistance * 0.3f); // 30% max magnitude reduction
        }

        #endregion

        #region IAdvancedEffectReceiver Implementation

        public void OnEffectStack(GameplayEffect effect, int newStackCount, int previousStackCount)
        {
            if (debugMode)
                Debug.Log($"[EffectComponent] {effect.EffectName} stacked: {previousStackCount} -> {newStackCount}");

            EffectStacked?.Invoke(effect, newStackCount);
        }

        public void OnEffectPeriodic(GameplayEffect effect, EffectContext context)
        {
            if (debugMode)
                Debug.Log($"[EffectComponent] Periodic tick for {effect.EffectName}");
        }

        public void OnEffectExpired(GameplayEffect effect, EffectContext context)
        {
            if (debugMode)
                Debug.Log($"[EffectComponent] {effect.EffectName} expired naturally");

            EffectExpired?.Invoke(effect);
        }

        public void OnEffectDispelled(GameplayEffect effect, EffectContext context)
        {
            if (debugMode)
                Debug.Log($"[EffectComponent] {effect.EffectName} was dispelled");
        }

        public void OnEffectRefreshed(GameplayEffect effect, EffectContext context)
        {
            if (debugMode)
                Debug.Log($"[EffectComponent] {effect.EffectName} duration refreshed");
        }

        public bool ValidateEffectApplication(GameplayEffect effect, EffectContext context)
        {
            // Check max effect limits
            if (activeEffects.Count >= maxActiveEffects)
            {
                if (debugMode)
                    Debug.LogWarning($"[EffectComponent] Max active effects reached ({maxActiveEffects})");
                return false;
            }

            // Check buff/debuff limits
            bool isBuff = effect.AssetTags?.Tags.Any(t => t.TagName.Contains("Buff")) == true;
            bool isDebuff = effect.AssetTags?.Tags.Any(t => t.TagName.Contains("Debuff")) == true;

            if (isBuff && activeBuffCount >= maxBuffs)
            {
                if (debugMode)
                    Debug.LogWarning($"[EffectComponent] Max buffs reached ({maxBuffs})");
                return false;
            }

            if (isDebuff && activeDebuffCount >= maxDebuffs)
            {
                if (debugMode)
                    Debug.LogWarning($"[EffectComponent] Max debuffs reached ({maxDebuffs})");
                return false;
            }

            return true;
        }

        public int GetMaxStackOverride(GameplayEffect effect)
        {
            // Can be overridden based on attributes or other conditions
            return effect.MaxStackCount;
        }

        public float GetDurationModifier(GameplayEffect effect)
        {
            // Can modify based on attributes
            float modifier = 1f;

            if (attributeComponent != null)
            {
                // Example: Tenacity reduces debuff duration
                if (effect.AssetTags?.Tags.Any(t => t.TagName.Contains("Debuff")) == true)
                {
                    float tenacity = attributeComponent.GetAttributeValue(AttributeType.Tenacity);
                    modifier *= (1f - tenacity / 200f); // Max 50% reduction at 100 tenacity
                }
            }

            return modifier;
        }

        public float GetMagnitudeModifier(GameplayEffect effect)
        {
            return 1f; // Can be modified based on attributes
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponent();
        }

        private void Start()
        {
            RegisterEvents();
        }

        private void Update()
        {
            UpdateEffects();
            ProcessPeriodicEffects();
            RemoveExpiredEffects();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
            RemoveAllEffects();
        }

        #endregion

        #region Initialization

        private void InitializeComponent()
        {
            tagComponent = GetComponent<TagComponent>();
            attributeComponent = GetComponent<AttributeSetComponent>();

            activeEffects = new List<EffectInstance>();
            effectLookup = new Dictionary<GameplayEffect, EffectInstance>();
            effectsByTag = new Dictionary<string, List<EffectInstance>>();
            periodicEffects = new List<EffectInstance>();

            GASEvents.Trigger(GASEventType.ComponentAdded,
                new SimpleGASEventData(gameObject, this));
        }

        private void RegisterEvents()
        {
            GASEvents.Subscribe(GASEventType.Death, OnDeath);
            GASEvents.Subscribe(GASEventType.Respawn, OnRespawn);
        }

        private void UnregisterEvents()
        {
            GASEvents.Unsubscribe(GASEventType.Death, OnDeath);
            GASEvents.Unsubscribe(GASEventType.Respawn, OnRespawn);
        }

        #endregion

        #region Effect Application

        /// <summary>
        /// Apply an effect to this component
        /// </summary>
        public bool ApplyEffect(GameplayEffect effect, EffectContext context = null)
        {
            if (!CanApplyEffect(effect, context))
                return false;

            // Create context if not provided
            if (context == null)
            {
                context = new EffectContext(null, gameObject);
            }
            else if (context.target == null)
            {
                context.target = gameObject;
            }

            // Modify incoming effect
            ModifyIncomingEffect(effect, ref context);

            // Check for existing instance (stacking)
            if (effectLookup.TryGetValue(effect, out var existingInstance))
            {
                return HandleEffectStacking(effect, existingInstance, context);
            }

            // Create new instance
            var instance = CreateEffectInstance(effect, context);
            if (instance == null)
                return false;

            // Add to tracking
            AddEffectInstance(instance);

            // Pre-apply callback
            OnPreEffectApply(effect, context);

            // Apply the effect
            effect.OnApply(context);

            // Post-apply callback
            OnPostEffectApply(effect, context);

            // Fire events
            FireEffectAppliedEvent(effect, context);

            return true;
        }

        /// <summary>
        /// Apply multiple effects
        /// </summary>
        public void ApplyEffects(List<GameplayEffect> effects, EffectContext context = null)
        {
            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    ApplyEffect(effect, context?.Clone());
                }
            }
        }

        /// <summary>
        /// Check if effect can be applied
        /// </summary>
        private bool CanApplyEffect(GameplayEffect effect, EffectContext context)
        {
            if (effect == null)
                return false;

            if (!CanReceiveEffects)
            {
                EffectBlocked?.Invoke(effect);
                return false;
            }

            if (IsImmuneToEffect(effect))
            {
                EffectBlocked?.Invoke(effect);
                return false;
            }

            if (!effect.CanApply(context))
            {
                EffectBlocked?.Invoke(effect);
                return false;
            }

            if (!ValidateEffectApplication(effect, context))
            {
                EffectBlocked?.Invoke(effect);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handle effect stacking
        /// </summary>
        private bool HandleEffectStacking(GameplayEffect effect, EffectInstance existingInstance, EffectContext context)
        {
            switch (effect.StackingPolicy)
            {
                case StackingPolicy.None:
                    if (debugMode)
                        Debug.Log($"[EffectComponent] {effect.EffectName} does not stack");
                    return false;

                case StackingPolicy.Replace:
                    RemoveEffectInstance(existingInstance);
                    return ApplyEffect(effect, context);

                case StackingPolicy.Stack:
                    if (existingInstance.stackCount >= effect.MaxStackCount)
                    {
                        if (debugMode)
                            Debug.Log($"[EffectComponent] {effect.EffectName} at max stacks ({effect.MaxStackCount})");

                        // Refresh duration if configured
                        if (effect.RefreshDurationOnStack)
                        {
                            existingInstance.RefreshDuration();
                            OnEffectRefreshed(effect, context);
                        }
                        return false;
                    }

                    int previousStacks = existingInstance.stackCount;
                    existingInstance.AddStack();

                    effect.OnStack(context, existingInstance.stackCount, previousStacks);
                    OnEffectStack(effect, existingInstance.stackCount, previousStacks);
                    return true;

                case StackingPolicy.Refresh:
                    existingInstance.RefreshDuration();
                    OnEffectRefreshed(effect, context);
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Create effect instance
        /// </summary>
        private EffectInstance CreateEffectInstance(GameplayEffect effect, EffectContext context)
        {
            var instance = effect.CreateInstance(context);

            // Apply duration modifier
            float durationModifier = GetDurationModifier(effect);
            if (instance.duration > 0)
            {
                instance.duration *= durationModifier;
            }

            // Check if periodic
            if (effect.EffectType == EffectType.Periodic && effect.Period > 0)
            {
                periodicEffects.Add(instance);
            }

            return instance;
        }

        /// <summary>
        /// Add effect instance to tracking
        /// </summary>
        private void AddEffectInstance(EffectInstance instance)
        {
            activeEffects.Add(instance);
            effectLookup[instance.effect] = instance;

            // Track by tags
            if (instance.effect.AssetTags != null)
            {
                foreach (var tag in instance.effect.AssetTags.Tags)
                {
                    if (!effectsByTag.ContainsKey(tag.TagName))
                    {
                        effectsByTag[tag.TagName] = new List<EffectInstance>();
                    }
                    effectsByTag[tag.TagName].Add(instance);
                }

                // Update buff/debuff counts
                if (instance.effect.AssetTags.Tags.Any(t => t.TagName.Contains("Buff")))
                    activeBuffCount++;
                if (instance.effect.AssetTags.Tags.Any(t => t.TagName.Contains("Debuff")))
                    activeDebuffCount++;
            }
        }

        #endregion

        #region Effect Removal

        /// <summary>
        /// Remove a specific effect
        /// </summary>
        public bool RemoveEffect(GameplayEffect effect)
        {
            if (!effectLookup.TryGetValue(effect, out var instance))
                return false;

            RemoveEffectInstance(instance);
            return true;
        }

        /// <summary>
        /// Remove all effects with a specific tag
        /// </summary>
        public void RemoveEffectsByTag(string tagName)
        {
            if (!effectsByTag.TryGetValue(tagName, out var instances))
                return;

            var instancesToRemove = new List<EffectInstance>(instances);
            foreach (var instance in instancesToRemove)
            {
                RemoveEffectInstance(instance);
            }
        }

        /// <summary>
        /// Remove all debuffs
        /// </summary>
        public void RemoveAllDebuffs()
        {
            RemoveEffectsByTag("Debuff");
        }

        /// <summary>
        /// Remove all buffs
        /// </summary>
        public void RemoveAllBuffs()
        {
            RemoveEffectsByTag("Buff");
        }

        /// <summary>
        /// Remove all effects
        /// </summary>
        public void RemoveAllEffects()
        {
            var instancesToRemove = new List<EffectInstance>(activeEffects);
            foreach (var instance in instancesToRemove)
            {
                RemoveEffectInstance(instance);
            }
        }

        /// <summary>
        /// Dispel random debuff
        /// </summary>
        public bool DispelRandomDebuff()
        {
            if (!effectsByTag.TryGetValue("Debuff", out var debuffs) || debuffs.Count == 0)
                return false;

            var randomDebuff = debuffs[UnityEngine.Random.Range(0, debuffs.Count)];

            OnEffectDispelled(randomDebuff.effect, randomDebuff.context);
            RemoveEffectInstance(randomDebuff);
            return true;
        }

        /// <summary>
        /// Remove effect instance
        /// </summary>
        private void RemoveEffectInstance(EffectInstance instance)
        {
            if (instance == null || !activeEffects.Contains(instance))
                return;

            // Pre-remove callback
            OnPreEffectRemove(instance.effect, instance.context);

            // Remove the effect
            instance.effect.OnRemove(instance.context);

            // Remove from tracking
            activeEffects.Remove(instance);
            effectLookup.Remove(instance.effect);
            periodicEffects.Remove(instance);

            // Remove from tag tracking
            if (instance.effect.AssetTags != null)
            {
                foreach (var tag in instance.effect.AssetTags.Tags)
                {
                    if (effectsByTag.ContainsKey(tag.TagName))
                    {
                        effectsByTag[tag.TagName].Remove(instance);
                        if (effectsByTag[tag.TagName].Count == 0)
                        {
                            effectsByTag.Remove(tag.TagName);
                        }
                    }
                }

                // Update buff/debuff counts
                if (instance.effect.AssetTags.Tags.Any(t => t.TagName.Contains("Buff")))
                    activeBuffCount = Mathf.Max(0, activeBuffCount - 1);
                if (instance.effect.AssetTags.Tags.Any(t => t.TagName.Contains("Debuff")))
                    activeDebuffCount = Mathf.Max(0, activeDebuffCount - 1);
            }

            // Clean up visual effect
            if (instance.visualEffect != null)
            {
                Destroy(instance.visualEffect);
            }

            // Post-remove callback
            OnPostEffectRemove(instance.effect, instance.context);

            // Fire events
            FireEffectRemovedEvent(instance.effect, instance.context);
        }

        #endregion

        #region Effect Updates

        /// <summary>
        /// Update all active effects
        /// </summary>
        private void UpdateEffects()
        {
            foreach (var instance in activeEffects)
            {
                if (instance.effect != null && !instance.isPaused)
                {
                    instance.effect.OnTick(instance.context, Time.deltaTime);

                    // Check ongoing requirements
                    if (!instance.effect.CheckOngoing(instance.context))
                    {
                        effectsToRemove.Add(instance);
                    }

                    // Check removal conditions
                    if (instance.effect.CheckRemoval(instance.context))
                    {
                        effectsToRemove.Add(instance);
                    }
                }
            }
        }

        /// <summary>
        /// Process periodic effects
        /// </summary>
        private void ProcessPeriodicEffects()
        {
            foreach (var instance in periodicEffects)
            {
                if (instance.effect != null &&
                    !instance.isPaused &&
                    instance.IsPeriodicDue(instance.effect.Period))
                {
                    instance.UpdatePeriodicTime();
                    instance.effect.OnPeriodic(instance.context);
                    OnEffectPeriodic(instance.effect, instance.context);

                    FirePeriodicEvent(instance.effect, instance.context);
                }
            }
        }

        /// <summary>
        /// Remove expired effects
        /// </summary>
        private void RemoveExpiredEffects()
        {
            // Check for expired effects
            foreach (var instance in activeEffects)
            {
                if (instance.IsExpired())
                {
                    effectsToRemove.Add(instance);
                    OnEffectExpired(instance.effect, instance.context);
                }
            }

            // Remove collected effects
            foreach (var instance in effectsToRemove)
            {
                RemoveEffectInstance(instance);
            }

            effectsToRemove.Clear();
        }

        #endregion

        #region Effect Queries

        /// <summary>
        /// Get all active effects
        /// </summary>
        public List<EffectInstance> GetActiveEffects()
        {
            return new List<EffectInstance>(activeEffects);
        }

        /// <summary>
        /// Get effects by tag
        /// </summary>
        public List<EffectInstance> GetEffectsByTag(string tagName)
        {
            if (effectsByTag.TryGetValue(tagName, out var instances))
            {
                return new List<EffectInstance>(instances);
            }
            return new List<EffectInstance>();
        }

        /// <summary>
        /// Has effect?
        /// </summary>
        public bool HasEffect(GameplayEffect effect)
        {
            return effectLookup.ContainsKey(effect);
        }

        /// <summary>
        /// Has effect with tag?
        /// </summary>
        public bool HasEffectWithTag(string tagName)
        {
            return effectsByTag.ContainsKey(tagName) && effectsByTag[tagName].Count > 0;
        }

        /// <summary>
        /// Get effect instance
        /// </summary>
        public EffectInstance GetEffectInstance(GameplayEffect effect)
        {
            effectLookup.TryGetValue(effect, out var instance);
            return instance;
        }

        /// <summary>
        /// Get stack count for effect
        /// </summary>
        public int GetEffectStackCount(GameplayEffect effect)
        {
            if (effectLookup.TryGetValue(effect, out var instance))
            {
                return instance.stackCount;
            }
            return 0;
        }

        #endregion

        #region Effect Control

        /// <summary>
        /// Pause an effect
        /// </summary>
        public void PauseEffect(GameplayEffect effect)
        {
            if (effectLookup.TryGetValue(effect, out var instance))
            {
                instance.Pause();
            }
        }

        /// <summary>
        /// Resume an effect
        /// </summary>
        public void ResumeEffect(GameplayEffect effect)
        {
            if (effectLookup.TryGetValue(effect, out var instance))
            {
                instance.Resume();
            }
        }

        /// <summary>
        /// Pause all effects
        /// </summary>
        public void PauseAllEffects()
        {
            foreach (var instance in activeEffects)
            {
                instance.Pause();
            }
        }

        /// <summary>
        /// Resume all effects
        /// </summary>
        public void ResumeAllEffects()
        {
            foreach (var instance in activeEffects)
            {
                instance.Resume();
            }
        }

        /// <summary>
        /// Extend effect duration
        /// </summary>
        public void ExtendEffectDuration(GameplayEffect effect, float additionalDuration)
        {
            if (effectLookup.TryGetValue(effect, out var instance) && instance.duration > 0)
            {
                instance.duration += additionalDuration;
            }
        }

        #endregion

        #region Event Handlers

        private void OnDeath(object data)
        {
            if (data is GASEventData eventData && eventData.source == gameObject)
            {
                // Remove effects that should be removed on death
                var instancesToCheck = new List<EffectInstance>(activeEffects);
                foreach (var instance in instancesToCheck)
                {
                    if (instance.effect.AssetTags?.Tags.Any(t => t.TagName.Contains("RemoveOnDeath")) == true)
                    {
                        RemoveEffectInstance(instance);
                    }
                }

                // Pause remaining effects
                PauseAllEffects();
            }
        }

        private void OnRespawn(object data)
        {
            if (data is GASEventData eventData && eventData.source == gameObject)
            {
                // Resume paused effects
                ResumeAllEffects();

                // Remove effects that don't persist through death
                var instancesToCheck = new List<EffectInstance>(activeEffects);
                foreach (var instance in instancesToCheck)
                {
                    if (instance.effect.AssetTags?.Tags.Any(t => t.TagName.Contains("NoRespawnPersist")) == true)
                    {
                        RemoveEffectInstance(instance);
                    }
                }
            }
        }

        #endregion

        #region Events

        private void FireEffectAppliedEvent(GameplayEffect effect, EffectContext context)
        {
            GASEvents.Trigger(GASEventType.EffectApplied, new EffectAppliedEventData
            {
                effect = effect,
                context = context,
                target = gameObject,
                source = context.instigator
            });
        }

        private void FireEffectRemovedEvent(GameplayEffect effect, EffectContext context)
        {
            GASEvents.Trigger(GASEventType.EffectRemoved, new EffectRemovedEventData
            {
                effect = effect,
                context = context,
                target = gameObject,
                source = context.instigator
            });
        }

        private void FirePeriodicEvent(GameplayEffect effect, EffectContext context)
        {
            GASEvents.Trigger(GASEventType.EffectPeriodic, new EffectPeriodicEventData
            {
                effect = effect,
                context = context,
                target = gameObject,
                source = context.instigator
            });
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!debugMode) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2);
            if (screenPos.z < 0) return;

            float yOffset = 0;
            GUI.Label(new Rect(screenPos.x - 100, Screen.height - screenPos.y - yOffset, 200, 20),
                $"Effects: {activeEffects.Count} (B:{activeBuffCount} D:{activeDebuffCount})");

            yOffset += 20;
            foreach (var instance in activeEffects)
            {
                string effectInfo = $"{instance.effect.EffectName}";
                if (instance.stackCount > 1)
                    effectInfo += $" x{instance.stackCount}";
                if (instance.duration > 0)
                    effectInfo += $" ({instance.RemainingDuration:F1}s)";

                GUI.Label(new Rect(screenPos.x - 100, Screen.height - screenPos.y - yOffset, 200, 20), effectInfo);
                yOffset += 20;
            }
        }

        #endregion
    }

    #region Event Data Classes

    public class EffectAppliedEventData : GASEventData
    {
        public GameplayEffect effect;
        public EffectContext context;
        public GameObject target;
    }

    public class EffectRemovedEventData : GASEventData
    {
        public GameplayEffect effect;
        public EffectContext context;
        public GameObject target;
    }

    public class EffectPeriodicEventData : GASEventData
    {
        public GameplayEffect effect;
        public EffectContext context;
        public GameObject target;
        public int tickCount;
    }

    #endregion
}