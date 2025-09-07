// ================================
// File: Assets/Scripts/GAS/AbilitySystem/AbilitySystemComponent.cs
// 능력 시스템을 관리하는 컴포넌트
// ================================
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GAS.Core;
using GAS.TagSystem;
using GAS.AttributeSystem;
using GAS.EffectSystem;
using static GAS.Core.GASConstants;

namespace GAS.AbilitySystem
{
    /// <summary>
    /// Component that manages abilities on a GameObject
    /// </summary>
    public class AbilitySystemComponent : MonoBehaviour, IAbilitySource
    {
        [Header("Configuration")]
        [SerializeField] private int characterLevel = 1;
        [SerializeField] private int teamId = 0;
        [SerializeField] private bool debugMode = true;

        [Header("Granted Abilities")]
        [SerializeField] private List<AbilityGrantInfo> grantedAbilities = new List<AbilityGrantInfo>();

        [Header("Runtime Info - Read Only")]
        [SerializeField] private List<AbilitySpec> abilitySpecs = new List<AbilitySpec>();
        [SerializeField] private List<AbilitySpec> activeAbilities = new List<AbilitySpec>();

        // Component references
        private AttributeSetComponent attributeComponent;
        private TagComponent tagComponent;
        private EffectComponent effectComponent;

        // Ability tracking
        private Dictionary<int, AbilitySpec> inputBindings = new Dictionary<int, AbilitySpec>();
        private Dictionary<GameplayAbility, AbilitySpec> abilityLookup = new Dictionary<GameplayAbility, AbilitySpec>();

        // Events
        public event Action<AbilitySpec> AbilityGranted;
        public event Action<AbilitySpec> AbilityRevoked;
        public event Action<AbilitySpec> AbilityActivated;
        public event Action<AbilitySpec> AbilityEnded;
        public event Action<AbilitySpec> AbilityCooldownStarted;
        public event Action<AbilitySpec> AbilityCooldownEnded;

        #region IAbilitySource Implementation

        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public AttributeSetComponent AttributeComponent => attributeComponent;
        public TagComponent TagComponent => tagComponent;
        public EffectComponent EffectComponent => effectComponent;
        public int Level => characterLevel;
        public bool IsAlive => attributeComponent != null &&
                               attributeComponent.GetAttributeValue(AttributeType.Health) > 0;
        public int TeamId => teamId;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            GrantInitialAbilities();
            RegisterEvents();
        }

        private void Update()
        {
            UpdateCooldowns();
            HandleDebugInput();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
            RevokeAllAbilities();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            attributeComponent = GetComponent<AttributeSetComponent>();
            tagComponent = GetComponent<TagComponent>();
            effectComponent = GetComponent<EffectComponent>();

            if (attributeComponent == null)
            {
                Debug.LogWarning($"[AbilitySystemComponent] No AttributeSetComponent found on {gameObject.name}");
            }

            if (tagComponent == null)
            {
                Debug.LogWarning($"[AbilitySystemComponent] No TagComponent found on {gameObject.name}");
            }

            if (effectComponent == null)
            {
                Debug.LogWarning($"[AbilitySystemComponent] No EffectComponent found on {gameObject.name}");
            }
        }

        private void GrantInitialAbilities()
        {
            foreach (var grantInfo in grantedAbilities)
            {
                if (grantInfo.ability != null)
                {
                    GrantAbility(grantInfo.ability, grantInfo.level, grantInfo.inputId);
                }
            }
        }

        private void RegisterEvents()
        {
            GASEvents.Subscribe(GASEventType.AbilityCancelRequested, OnAbilityCancelRequested);
        }

        private void UnregisterEvents()
        {
            GASEvents.Unsubscribe(GASEventType.AbilityCancelRequested, OnAbilityCancelRequested);
        }

        #endregion

        #region Ability Management

        /// <summary>
        /// Grant an ability to this component
        /// </summary>
        public AbilitySpec GrantAbility(GameplayAbility ability, int level = 1, int inputId = -1)
        {
            if (ability == null)
            {
                Debug.LogWarning("[AbilitySystemComponent] Attempted to grant null ability");
                return null;
            }

            // Check if already has this ability
            if (abilityLookup.ContainsKey(ability))
            {
                Debug.LogWarning($"[AbilitySystemComponent] Already has ability: {ability.AbilityName}");
                return abilityLookup[ability];
            }

            // Create spec
            var spec = new AbilitySpec(ability, level, inputId);

            // Add to tracking
            abilitySpecs.Add(spec);
            abilityLookup[ability] = spec;

            // Bind to input if specified
            if (inputId >= 0)
            {
                inputBindings[inputId] = spec;
            }

            // Fire events
            AbilityGranted?.Invoke(spec);
            GASEvents.Trigger(GASEventType.AbilityGranted, new AbilityGrantEventData(this, ability, level)
            {
                spec = spec,
                inputId = inputId
            });

            if (debugMode)
            {
                Debug.Log($"[AbilitySystemComponent] Granted ability: {ability.AbilityName} (Level {level}, Input {inputId})");
            }

            return spec;
        }

        /// <summary>
        /// Revoke an ability
        /// </summary>
        public void RevokeAbility(GameplayAbility ability)
        {
            if (ability == null) return;

            if (!abilityLookup.TryGetValue(ability, out var spec))
            {
                Debug.LogWarning($"[AbilitySystemComponent] Doesn't have ability: {ability.AbilityName}");
                return;
            }

            // Cancel if active
            if (spec.IsActive)
            {
                _ = ability.CancelAbility();
            }

            // Remove from tracking
            abilitySpecs.Remove(spec);
            abilityLookup.Remove(ability);
            activeAbilities.Remove(spec);

            // Remove input binding
            if (spec.inputId >= 0 && inputBindings.ContainsKey(spec.inputId))
            {
                inputBindings.Remove(spec.inputId);
            }

            // Fire events
            AbilityRevoked?.Invoke(spec);
            GASEvents.Trigger(GASEventType.AbilityRevoked, new AbilityGrantEventData(this, ability, spec.level));

            if (debugMode)
            {
                Debug.Log($"[AbilitySystemComponent] Revoked ability: {ability.AbilityName}");
            }
        }

        /// <summary>
        /// Revoke all abilities
        /// </summary>
        public void RevokeAllAbilities()
        {
            var abilitiesToRevoke = new List<GameplayAbility>(abilityLookup.Keys);
            foreach (var ability in abilitiesToRevoke)
            {
                RevokeAbility(ability);
            }
        }

        #endregion

        #region Ability Activation

        /// <summary>
        /// Try to activate an ability by input ID
        /// </summary>
        public async Awaitable<bool> TryActivateAbilityByInput(int inputId)
        {
            if (!inputBindings.TryGetValue(inputId, out var spec))
            {
                if (debugMode)
                    Debug.LogWarning($"[AbilitySystemComponent] No ability bound to input {inputId}");
                return false;
            }

            return await TryActivateAbility(spec.ability);
        }

        /// <summary>
        /// Try to activate an ability
        /// </summary>
        public async Awaitable<bool> TryActivateAbility(GameplayAbility ability, GameObject target = null)
        {
            if (ability == null) return false;

            if (!abilityLookup.TryGetValue(ability, out var spec))
            {
                if (debugMode)
                    Debug.LogWarning($"[AbilitySystemComponent] Doesn't have ability: {ability.AbilityName}");
                return false;
            }

            // Create activation info
            var activationInfo = new AbilityActivationInfo(this, target)
            {
                activationType = AbilityActivationType.Manual,
                inputId = spec.inputId
            };

            // Try to activate
            var result = await ability.Activate(spec, activationInfo);

            if (result.success)
            {
                if (!activeAbilities.Contains(spec))
                    activeAbilities.Add(spec);

                AbilityActivated?.Invoke(spec);

                if (debugMode)
                    Debug.Log($"[AbilitySystemComponent] Activated ability: {ability.AbilityName}");
            }
            else if (debugMode)
            {
                Debug.LogWarning($"[AbilitySystemComponent] Failed to activate {ability.AbilityName}: {result.failureReason}");
            }

            return result.success;
        }

        /// <summary>
        /// Try to activate ability at position
        /// </summary>
        public async Awaitable<bool> TryActivateAbilityAtPosition(GameplayAbility ability, Vector3 position)
        {
            if (ability == null) return false;

            if (!abilityLookup.TryGetValue(ability, out var spec))
                return false;

            var activationInfo = AbilityActivationInfo.CreateWithPosition(this, position, Vector3.forward);
            var result = await ability.Activate(spec, activationInfo);

            if (result.success && !activeAbilities.Contains(spec))
                activeAbilities.Add(spec);

            return result.success;
        }

        /// <summary>
        /// Cancel an ability
        /// </summary>
        public async Awaitable CancelAbility(GameplayAbility ability)
        {
            if (ability == null) return;

            if (!abilityLookup.TryGetValue(ability, out var spec))
                return;

            if (spec.IsActive)
            {
                await ability.CancelAbility();
                activeAbilities.Remove(spec);

                if (debugMode)
                    Debug.Log($"[AbilitySystemComponent] Cancelled ability: {ability.AbilityName}");
            }
        }

        /// <summary>
        /// Cancel all active abilities
        /// </summary>
        public async Awaitable CancelAllAbilities()
        {
            var abilitiesToCancel = new List<AbilitySpec>(activeAbilities);
            foreach (var spec in abilitiesToCancel)
            {
                await CancelAbility(spec.ability);
            }
        }

        #endregion

        #region Cooldown Management

        private void UpdateCooldowns()
        {
            foreach (var spec in abilitySpecs)
            {
                if (spec.IsOnCooldown)
                {
                    // Check if cooldown finished
                    if (!spec.IsOnCooldown && spec.state == AbilityState.Cooldown)
                    {
                        spec.state = AbilityState.Idle;
                        AbilityCooldownEnded?.Invoke(spec);

                        if (debugMode)
                            Debug.Log($"[AbilitySystemComponent] Cooldown ended for {spec.ability.AbilityName}");
                    }
                }

                // Update active abilities
                if (spec.IsActive && !activeAbilities.Contains(spec))
                {
                    activeAbilities.Add(spec);
                }
                else if (!spec.IsActive && activeAbilities.Contains(spec))
                {
                    activeAbilities.Remove(spec);
                    AbilityEnded?.Invoke(spec);
                }
            }
        }

        #endregion

        #region Event Handlers

        private void OnAbilityCancelRequested(object data)
        {
            if (data is AbilityCancelEventData cancelData && cancelData.source == this)
            {
                foreach (var spec in activeAbilities.ToList())
                {
                    bool shouldCancel = false;

                    // Check if ability has any of the tags to cancel
                    foreach (var tag in cancelData.tagsToCancel)
                    {
                        if (spec.ability.AbilityTags.Contains(tag))
                        {
                            shouldCancel = true;
                            break;
                        }
                    }

                    if (shouldCancel)
                    {
                        _ = CancelAbility(spec.ability);
                    }
                }
            }
        }

        #endregion

        #region Debug

        private void HandleDebugInput()
        {
            if (!debugMode) return;

            // Test ability activation with number keys
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    _ = TryActivateAbilityByInput(i);
                }
            }

            // Cancel all abilities with Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _ = CancelAllAbilities();
            }

            // Debug info with Tab
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                PrintDebugInfo();
            }
        }

        public void PrintDebugInfo()
        {
            Debug.Log("=== Ability System Debug Info ===");
            Debug.Log($"Character Level: {characterLevel}");
            Debug.Log($"Is Alive: {IsAlive}");
            Debug.Log($"Total Abilities: {abilitySpecs.Count}");
            Debug.Log($"Active Abilities: {activeAbilities.Count}");

            foreach (var spec in abilitySpecs)
            {
                string status = spec.IsActive ? "ACTIVE" :
                               spec.IsOnCooldown ? $"COOLDOWN ({spec.RemainingCooldown:F1}s)" :
                               "READY";
                Debug.Log($"  - {spec.ability.AbilityName} (Lvl {spec.level}): {status}");
            }

            Debug.Log("=================================");
        }

        #endregion

        #region Utility

        /// <summary>
        /// Get ability spec
        /// </summary>
        public AbilitySpec GetAbilitySpec(GameplayAbility ability)
        {
            abilityLookup.TryGetValue(ability, out var spec);
            return spec;
        }

        /// <summary>
        /// Get all ability specs
        /// </summary>
        public List<AbilitySpec> GetAllAbilitySpecs()
        {
            return new List<AbilitySpec>(abilitySpecs);
        }

        /// <summary>
        /// Check if has ability
        /// </summary>
        public bool HasAbility(GameplayAbility ability)
        {
            return abilityLookup.ContainsKey(ability);
        }

        #endregion
    }

    /// <summary>
    /// Info for granting abilities in inspector
    /// </summary>
    [Serializable]
    public class AbilityGrantInfo
    {
        public GameplayAbility ability;
        public int level = 1;
        public int inputId = -1;
    }
}