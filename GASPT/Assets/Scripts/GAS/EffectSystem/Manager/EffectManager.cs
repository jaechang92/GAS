// ================================
// File: Assets/Scripts/GAS/EffectSystem/Manager/EffectManager.cs
// Complete Effect Manager implementation
// ================================
using GAS.Core;
using NPS2.Manager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GAS.Core.GASConstants;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Central manager for the Effect System
    /// </summary>
    public class EffectManager : SingletonBehaviour<EffectManager>
    {
        [Header("Configuration")]
        [SerializeField] private EffectDatabase effectDatabase;
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool debugMode = false;

        [Header("Pooling Settings")]
        [SerializeField] private bool usePooling = true;
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private int maxPoolSize = 100;

        [Header("Performance Settings")]
        [SerializeField] private int maxEffectsPerTarget = 50;
        [SerializeField] private float cleanupInterval = 5f;
        [SerializeField] private bool batchUpdates = true;
        [SerializeField] private int batchSize = 10;

        [Header("Runtime Statistics - Read Only")]
        [SerializeField] private int totalActiveEffects;
        [SerializeField] private int totalPooledEffects;
        [SerializeField] private int totalTargetsWithEffects;
        [SerializeField] private float lastCleanupTime;

        // Effect tracking
        private Dictionary<GameObject, List<EffectInstance>> activeEffects;
        private Dictionary<string, Queue<GameObject>> effectPools;
        private Dictionary<GameplayEffect, List<GameObject>> effectsByType;
        private HashSet<EffectComponent> registeredComponents;

        // Update batching
        private List<EffectInstance> periodicEffects;
        private List<EffectInstance> effectUpdateQueue;
        private int currentBatchIndex;

        // Events
        public event Action<GameplayEffect, GameObject> OnEffectApplied;
        public event Action<GameplayEffect, GameObject> OnEffectRemoved;
        public event Action<int> OnEffectCountChanged;
        public event Action<GameObject> OnTargetRegistered;
        public event Action<GameObject> OnTargetUnregistered;

        #region Initialization

        protected override void Awake()
        {
            base.Awake();

            if (autoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initialize the Effect Manager
        /// </summary>
        public void Initialize()
        {
            // Initialize collections
            activeEffects = new Dictionary<GameObject, List<EffectInstance>>();
            effectPools = new Dictionary<string, Queue<GameObject>>();
            effectsByType = new Dictionary<GameplayEffect, List<GameObject>>();
            registeredComponents = new HashSet<EffectComponent>();
            periodicEffects = new List<EffectInstance>();
            effectUpdateQueue = new List<EffectInstance>();

            // Initialize database
            if (effectDatabase != null)
            {
                effectDatabase.Initialize();
                Debug.Log("[EffectManager] Database initialized");
            }
            else
            {
                Debug.LogWarning("[EffectManager] No EffectDatabase assigned!");
            }

            // Subscribe to events
            SubscribeToEvents();

            // Start cleanup coroutine
            StartCleanup();

            Debug.Log("[EffectManager] Initialized successfully");
        }

        private void SubscribeToEvents()
        {
            GASEvents.Subscribe(GASEventType.EffectApplied, OnEffectAppliedEvent);
            GASEvents.Subscribe(GASEventType.EffectRemoved, OnEffectRemovedEvent);
            GASEvents.Subscribe(GASEventType.ComponentAdded, OnComponentAdded);
            GASEvents.Subscribe(GASEventType.ComponentRemoved, OnComponentRemoved);
            GASEvents.Subscribe(GASEventType.Death, OnTargetDeath);
        }

        private void UnsubscribeFromEvents()
        {
            GASEvents.Unsubscribe(GASEventType.EffectApplied, OnEffectAppliedEvent);
            GASEvents.Unsubscribe(GASEventType.EffectRemoved, OnEffectRemovedEvent);
            GASEvents.Unsubscribe(GASEventType.ComponentAdded, OnComponentAdded);
            GASEvents.Unsubscribe(GASEventType.ComponentRemoved, OnComponentRemoved);
            GASEvents.Unsubscribe(GASEventType.Death, OnTargetDeath);
        }

        #endregion

        #region Effect Application

        /// <summary>
        /// Apply an effect to a target
        /// </summary>
        public bool ApplyEffect(GameObject target, GameplayEffect effect, EffectContext context = null)
        {
            if (target == null || effect == null)
            {
                Debug.LogWarning("[EffectManager] Cannot apply effect: null target or effect");
                return false;
            }

            // Get or add EffectComponent
            var effectComponent = target.GetComponent<EffectComponent>();
            if (effectComponent == null)
            {
                if (debugMode)
                    Debug.Log($"[EffectManager] Adding EffectComponent to {target.name}");
                effectComponent = target.AddComponent<EffectComponent>();
            }

            // Create context if not provided
            if (context == null)
            {
                context = new EffectContext(null, target);
            }

            // Apply through component
            bool success = effectComponent.ApplyEffect(effect, context);

            if (success)
            {
                TrackEffect(target, effect, context);
                UpdateStatistics();
            }

            return success;
        }

        /// <summary>
        /// Apply an effect by ID
        /// </summary>
        public bool ApplyEffectById(GameObject target, string effectId, EffectContext context = null)
        {
            if (effectDatabase == null)
            {
                Debug.LogError("[EffectManager] No EffectDatabase assigned!");
                return false;
            }

            var effect = effectDatabase.GetEffect(effectId);
            if (effect == null)
            {
                Debug.LogWarning($"[EffectManager] Effect with ID '{effectId}' not found in database");
                return false;
            }

            return ApplyEffect(target, effect, context);
        }

        /// <summary>
        /// Apply an effect from preset
        /// </summary>
        public bool ApplyEffectFromPreset(GameObject target, string presetName, int level, EffectContext context = null)
        {
            if (effectDatabase == null)
            {
                Debug.LogError("[EffectManager] No EffectDatabase assigned!");
                return false;
            }

            var effect = effectDatabase.CreateEffectFromPreset(presetName, level);
            if (effect == null)
            {
                Debug.LogWarning($"[EffectManager] Preset '{presetName}' not found in database");
                return false;
            }

            bool success = ApplyEffect(target, effect, context);

            // Clean up the cloned effect after application
            if (success)
            {
                // The effect is now tracked, we can destroy the ScriptableObject clone later
                ScheduleEffectCleanup(effect);
            }
            else
            {
                // Failed to apply, destroy immediately
                Destroy(effect);
            }

            return success;
        }

        /// <summary>
        /// Remove an effect from a target
        /// </summary>
        public bool RemoveEffect(GameObject target, GameplayEffect effect)
        {
            if (target == null || effect == null)
                return false;

            var effectComponent = target.GetComponent<EffectComponent>();
            if (effectComponent == null)
                return false;

            bool success = effectComponent.RemoveEffect(effect);

            if (success)
            {
                UntrackEffect(target, effect);
                UpdateStatistics();
            }

            return success;
        }

        /// <summary>
        /// Remove all effects from a target
        /// </summary>
        public void RemoveAllEffects(GameObject target)
        {
            if (target == null) return;

            var effectComponent = target.GetComponent<EffectComponent>();
            if (effectComponent == null) return;

            effectComponent.RemoveAllEffects();

            if (activeEffects.ContainsKey(target))
            {
                activeEffects.Remove(target);
            }

            UpdateStatistics();
        }

        /// <summary>
        /// Remove effects by tag from a target
        /// </summary>
        public void RemoveEffectsByTag(GameObject target, string tag)
        {
            if (target == null) return;

            var effectComponent = target.GetComponent<EffectComponent>();
            if (effectComponent == null) return;

            effectComponent.RemoveEffectsByTag(tag);
            UpdateStatistics();
        }

        #endregion

        #region Effect Tracking

        private void TrackEffect(GameObject target, GameplayEffect effect, EffectContext context)
        {
            // Track in active effects
            if (!activeEffects.ContainsKey(target))
            {
                activeEffects[target] = new List<EffectInstance>();
            }

            var instance = effect.CreateInstance(context);
            activeEffects[target].Add(instance);

            // Track by type
            if (!effectsByType.ContainsKey(effect))
            {
                effectsByType[effect] = new List<GameObject>();
            }
            if (!effectsByType[effect].Contains(target))
            {
                effectsByType[effect].Add(target);
            }

            // Track periodic effects
            if (effect.EffectType == EffectType.Periodic)
            {
                periodicEffects.Add(instance);
            }

            // Fire event
            OnEffectApplied?.Invoke(effect, target);

            if (debugMode)
                Debug.Log($"[EffectManager] Tracking effect {effect.EffectName} on {target.name}");
        }

        private void UntrackEffect(GameObject target, GameplayEffect effect)
        {
            // Remove from active effects
            if (activeEffects.ContainsKey(target))
            {
                activeEffects[target].RemoveAll(e => e.effect == effect);
                if (activeEffects[target].Count == 0)
                {
                    activeEffects.Remove(target);
                }
            }

            // Remove from type tracking
            if (effectsByType.ContainsKey(effect))
            {
                effectsByType[effect].Remove(target);
                if (effectsByType[effect].Count == 0)
                {
                    effectsByType.Remove(effect);
                }
            }

            // Remove from periodic effects
            periodicEffects.RemoveAll(e => e.effect == effect && e.context.target == target);

            // Fire event
            OnEffectRemoved?.Invoke(effect, target);

            if (debugMode)
                Debug.Log($"[EffectManager] Untracking effect {effect.EffectName} from {target.name}");
        }

        #endregion

        #region Component Registration

        /// <summary>
        /// Register an EffectComponent
        /// </summary>
        public void RegisterComponent(EffectComponent component)
        {
            if (component == null) return;

            if (registeredComponents.Add(component))
            {
                OnTargetRegistered?.Invoke(component.gameObject);

                if (debugMode)
                    Debug.Log($"[EffectManager] Registered component on {component.gameObject.name}");
            }
        }

        /// <summary>
        /// Unregister an EffectComponent
        /// </summary>
        public void UnregisterComponent(EffectComponent component)
        {
            if (component == null) return;

            if (registeredComponents.Remove(component))
            {
                // Clean up effects for this component
                RemoveAllEffects(component.gameObject);

                OnTargetUnregistered?.Invoke(component.gameObject);

                if (debugMode)
                    Debug.Log($"[EffectManager] Unregistered component on {component.gameObject.name}");
            }
        }

        #endregion

        #region Update System

        private void Update()
        {
            if (batchUpdates)
            {
                UpdateBatched();
            }
            else
            {
                UpdateAll();
            }
        }

        private void UpdateBatched()
        {
            // Update a batch of effects each frame
            if (effectUpdateQueue.Count == 0)
            {
                // Rebuild queue
                effectUpdateQueue.Clear();
                foreach (var kvp in activeEffects)
                {
                    effectUpdateQueue.AddRange(kvp.Value);
                }
                currentBatchIndex = 0;
            }

            // Process batch
            int endIndex = Mathf.Min(currentBatchIndex + batchSize, effectUpdateQueue.Count);
            for (int i = currentBatchIndex; i < endIndex; i++)
            {
                var instance = effectUpdateQueue[i];
                if (instance != null && instance.effect != null && !instance.isPaused)
                {
                    // Update would be handled by EffectComponent
                    // This is for manager-level tracking
                }
            }

            currentBatchIndex = endIndex;
            if (currentBatchIndex >= effectUpdateQueue.Count)
            {
                effectUpdateQueue.Clear();
            }
        }

        private void UpdateAll()
        {
            // Update all effects every frame (not recommended for large numbers)
            foreach (var kvp in activeEffects)
            {
                foreach (var instance in kvp.Value)
                {
                    if (instance != null && instance.effect != null && !instance.isPaused)
                    {
                        // Update would be handled by EffectComponent
                        // This is for manager-level tracking
                    }
                }
            }
        }

        #endregion

        #region Pooling System

        /// <summary>
        /// Get a pooled effect visual
        /// </summary>
        public GameObject GetPooledEffect(GameObject prefab)
        {
            if (!usePooling || prefab == null)
                return Instantiate(prefab);

            string poolKey = prefab.name;

            if (!effectPools.ContainsKey(poolKey))
            {
                effectPools[poolKey] = new Queue<GameObject>();
                PrewarmPool(prefab, initialPoolSize);
            }

            if (effectPools[poolKey].Count > 0)
            {
                var pooled = effectPools[poolKey].Dequeue();
                pooled.SetActive(true);
                totalPooledEffects--;
                return pooled;
            }

            // Pool is empty, create new instance
            return CreatePooledEffect(prefab);
        }

        /// <summary>
        /// Return effect visual to pool
        /// </summary>
        public void ReturnToPool(GameObject effect)
        {
            if (!usePooling || effect == null)
            {
                Destroy(effect);
                return;
            }

            string poolKey = effect.name.Replace("(Clone)", "").Trim();

            if (!effectPools.ContainsKey(poolKey))
            {
                effectPools[poolKey] = new Queue<GameObject>();
            }

            if (effectPools[poolKey].Count < maxPoolSize)
            {
                effect.SetActive(false);
                effect.transform.SetParent(transform);
                effect.transform.position = Vector3.zero;
                effectPools[poolKey].Enqueue(effect);
                totalPooledEffects++;
            }
            else
            {
                Destroy(effect);
            }
        }

        private void PrewarmPool(GameObject prefab, int count)
        {
            string poolKey = prefab.name;

            for (int i = 0; i < count; i++)
            {
                var instance = CreatePooledEffect(prefab);
                instance.SetActive(false);
                effectPools[poolKey].Enqueue(instance);
                totalPooledEffects++;
            }

            if (debugMode)
                Debug.Log($"[EffectManager] Prewarmed pool for {poolKey} with {count} instances");
        }

        private GameObject CreatePooledEffect(GameObject prefab)
        {
            var instance = Instantiate(prefab);
            instance.transform.SetParent(transform);
            instance.name = prefab.name; // Remove "(Clone)" suffix
            return instance;
        }

        #endregion

        #region Cleanup System

        private void StartCleanup()
        {
            InvokeRepeating(nameof(PerformCleanup), cleanupInterval, cleanupInterval);
        }

        private void PerformCleanup()
        {
            lastCleanupTime = Time.time;

            // Remove null entries
            var keysToRemove = new List<GameObject>();

            foreach (var kvp in activeEffects)
            {
                if (kvp.Key == null)
                {
                    keysToRemove.Add(kvp.Key);
                    continue;
                }

                // Remove expired instances
                kvp.Value.RemoveAll(e => e == null || e.IsExpired());
            }

            foreach (var key in keysToRemove)
            {
                activeEffects.Remove(key);
            }

            // Clean up effect pools
            CleanupPools();

            // Clean up periodic effects list
            periodicEffects.RemoveAll(e => e == null || e.IsExpired());

            UpdateStatistics();

            if (debugMode)
                Debug.Log($"[EffectManager] Cleanup performed. Active effects: {totalActiveEffects}");
        }

        private void CleanupPools()
        {
            foreach (var pool in effectPools.Values)
            {
                // Remove destroyed objects from pools
                var validObjects = new Queue<GameObject>();
                while (pool.Count > 0)
                {
                    var obj = pool.Dequeue();
                    if (obj != null)
                    {
                        validObjects.Enqueue(obj);
                    }
                }

                // Rebuild pool
                while (validObjects.Count > 0)
                {
                    pool.Enqueue(validObjects.Dequeue());
                }
            }
        }

        private void ScheduleEffectCleanup(GameplayEffect effect)
        {
            // Schedule destruction of cloned ScriptableObject after a delay
            // This ensures it's not destroyed while still being used
            Destroy(effect, 60f); // Destroy after 1 minute
        }

        #endregion

        #region Event Handlers

        private void OnEffectAppliedEvent(object data)
        {
            if (data is EffectAppliedEventData eventData)
            {
                TrackEffect(eventData.target, eventData.effect, eventData.context);
            }
        }

        private void OnEffectRemovedEvent(object data)
        {
            if (data is EffectRemovedEventData eventData)
            {
                UntrackEffect(eventData.target, eventData.effect);
            }
        }

        private void OnComponentAdded(object data)
        {
            if (data is SimpleGASEventData eventData && eventData.data is EffectComponent component)
            {
                RegisterComponent(component);
            }
        }

        private void OnComponentRemoved(object data)
        {
            if (data is SimpleGASEventData eventData && eventData.data is EffectComponent component)
            {
                UnregisterComponent(component);
            }
        }

        private void OnTargetDeath(object data)
        {
            if (data is DeathEventData deathData)
            {
                HandleTargetDeath(deathData.source);
            }
        }

        private void HandleTargetDeath(GameObject target)
        {
            if (target == null) return;

            // Remove effects that should be removed on death
            var effectComponent = target.GetComponent<EffectComponent>();
            if (effectComponent != null)
            {
                var activeEffectsList = effectComponent.GetActiveEffects();
                foreach (var instance in activeEffectsList)
                {
                    if (instance.effect.AssetTags?.Tags.Any(t => t.TagName.Contains("RemoveOnDeath")) == true)
                    {
                        effectComponent.RemoveEffect(instance.effect);
                    }
                }
            }
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Get all active effects on a target
        /// </summary>
        public List<EffectInstance> GetActiveEffects(GameObject target)
        {
            if (activeEffects.TryGetValue(target, out var effects))
            {
                return new List<EffectInstance>(effects);
            }
            return new List<EffectInstance>();
        }

        /// <summary>
        /// Get all targets with a specific effect
        /// </summary>
        public List<GameObject> GetTargetsWithEffect(GameplayEffect effect)
        {
            if (effectsByType.TryGetValue(effect, out var targets))
            {
                return new List<GameObject>(targets);
            }
            return new List<GameObject>();
        }

        /// <summary>
        /// Check if target has effect
        /// </summary>
        public bool HasEffect(GameObject target, GameplayEffect effect)
        {
            if (activeEffects.TryGetValue(target, out var effects))
            {
                return effects.Any(e => e.effect == effect);
            }
            return false;
        }

        /// <summary>
        /// Get effect count on target
        /// </summary>
        public int GetEffectCount(GameObject target)
        {
            if (activeEffects.TryGetValue(target, out var effects))
            {
                return effects.Count;
            }
            return 0;
        }

        /// <summary>
        /// Get total active effect count
        /// </summary>
        public int GetTotalActiveEffects()
        {
            return totalActiveEffects;
        }

        #endregion

        #region Statistics

        private void UpdateStatistics()
        {
            totalActiveEffects = activeEffects.Values.Sum(list => list.Count);
            totalTargetsWithEffects = activeEffects.Count;
            OnEffectCountChanged?.Invoke(totalActiveEffects);
        }

        /// <summary>
        /// Get effect statistics
        /// </summary>
        public EffectStatistics GetStatistics()
        {
            return new EffectStatistics
            {
                totalActiveEffects = totalActiveEffects,
                totalPooledEffects = totalPooledEffects,
                totalTargetsWithEffects = totalTargetsWithEffects,
                periodicEffectCount = periodicEffects.Count,
                registeredComponentCount = registeredComponents.Count,
                effectTypeCount = effectsByType.Count,
                poolCount = effectPools.Count
            };
        }

        #endregion

        #region Database Access

        /// <summary>
        /// Get the effect database
        /// </summary>
        public EffectDatabase GetDatabase()
        {
            return effectDatabase;
        }

        /// <summary>
        /// Set the effect database
        /// </summary>
        public void SetDatabase(EffectDatabase database)
        {
            effectDatabase = database;
            if (database != null)
            {
                database.Initialize();
            }
        }

        #endregion

        #region Cleanup

        protected override void OnDestroy()
        {
            UnsubscribeFromEvents();
            CancelInvoke();

            // Clear all effects
            foreach (var target in activeEffects.Keys.ToList())
            {
                RemoveAllEffects(target);
            }

            // Clear pools
            foreach (var pool in effectPools.Values)
            {
                while (pool.Count > 0)
                {
                    var obj = pool.Dequeue();
                    if (obj != null)
                        Destroy(obj);
                }
            }

            base.OnDestroy();
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!debugMode) return;

            GUI.Box(new Rect(10, 10, 250, 150), "Effect Manager Statistics");
            GUI.Label(new Rect(20, 35, 230, 20), $"Active Effects: {totalActiveEffects}");
            GUI.Label(new Rect(20, 55, 230, 20), $"Pooled Effects: {totalPooledEffects}");
            GUI.Label(new Rect(20, 75, 230, 20), $"Targets with Effects: {totalTargetsWithEffects}");
            GUI.Label(new Rect(20, 95, 230, 20), $"Periodic Effects: {periodicEffects.Count}");
            GUI.Label(new Rect(20, 115, 230, 20), $"Registered Components: {registeredComponents.Count}");
            GUI.Label(new Rect(20, 135, 230, 20), $"Last Cleanup: {Time.time - lastCleanupTime:F1}s ago");
        }

        #endregion
    }

    /// <summary>
    /// Effect system statistics
    /// </summary>
    [Serializable]
    public struct EffectStatistics
    {
        public int totalActiveEffects;
        public int totalPooledEffects;
        public int totalTargetsWithEffects;
        public int periodicEffectCount;
        public int registeredComponentCount;
        public int effectTypeCount;
        public int poolCount;
    }
}