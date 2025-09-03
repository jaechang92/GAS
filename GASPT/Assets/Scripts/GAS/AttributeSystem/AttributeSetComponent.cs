// ================================
// File: Assets/Scripts/GAS/AttributeSystem/AttributeSetComponent.cs
// Complete implementation with all features
// ================================
using GAS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GAS.Core.GASConstants;
using static Unity.VisualScripting.Member;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// Component that manages attributes on a GameObject
    /// </summary>
    public class AttributeSetComponent : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private AttributeSetData attributeSetData;

        [Header("Runtime Data")]
        [SerializeField] private Dictionary<AttributeType, BaseAttribute> attributes;
        [SerializeField] private List<AttributeModifier> activeModifiers;

        // Events
        public event Action<AttributeType, float, float> OnAttributeChanged;
        public event Action<AttributeType, AttributeModifier> OnModifierAdded;
        public event Action<AttributeType, AttributeModifier> OnModifierRemoved;

        /// <summary>
        /// Gets all attributes
        /// </summary>
        public Dictionary<AttributeType, BaseAttribute> Attributes => attributes;

        /// <summary>
        /// Gets the attribute set data
        /// </summary>
        public AttributeSetData AttributeSetData => attributeSetData;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeAttributes();
        }

        private void OnDestroy()
        {
            CleanupAttributes();
        }

        private void Update()
        {
            UpdateTimedModifiers();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes attributes from AttributeSetData
        /// </summary>
        private void InitializeAttributes()
        {
            attributes = new Dictionary<AttributeType, BaseAttribute>();
            activeModifiers = new List<AttributeModifier>();

            if (attributeSetData != null)
            {
                attributeSetData.InitializeAttributes(this);
            }

            // Fire initialization event
            GASEvents.Trigger(GASEventType.ComponentAdded,
                new SimpleGASEventData(gameObject, this));
        }

        /// <summary>
        /// Cleanup on destroy
        /// </summary>
        private void CleanupAttributes()
        {
            // Remove all modifiers
            activeModifiers.Clear();

            // Clear attributes
            attributes?.Clear();

            // Fire cleanup event
            GASEvents.Trigger(GASEventType.ComponentRemoved,
                new SimpleGASEventData(gameObject, this));
        }

        /// <summary>
        /// Registers a new attribute at runtime
        /// </summary>
        public void RegisterAttribute(AttributeType type, BaseAttribute attribute)
        {
            if (attributes == null)
                attributes = new Dictionary<AttributeType, BaseAttribute>();

            if (!attributes.ContainsKey(type))
            {
                attribute.Initialize(this);
                attribute.AttributeType = type;
                attributes[type] = attribute;

                Debug.Log($"Registered attribute: {type}");
            }
            else
            {
                Debug.LogWarning($"Attribute {type} already exists!");
            }
        }

        #endregion

        #region Attribute Accessors

        /// <summary>
        /// Gets the current value of an attribute
        /// </summary>
        public float GetAttributeValue(AttributeType type)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                // For derived attributes, get calculated value
                if (attribute.IsDerived)
                {
                    return attribute.GetValue();
                }

                // For normal attributes, calculate with modifiers
                return attribute.CalculateFinalValue();
            }

            Debug.LogWarning($"[AttributeSetComponent] Attribute {type} not found!");
            return 0f;
        }

        /// <summary>
        /// Gets the base value of an attribute (without modifiers)
        /// </summary>
        public float GetAttributeBaseValue(AttributeType type)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                return attribute.BaseValue;
            }

            Debug.LogWarning($"[AttributeSetComponent] Attribute {type} not found!");
            return 0f;
        }

        /// <summary>
        /// Gets the current value without recalculation
        /// </summary>
        public float GetAttributeCurrentValue(AttributeType type)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                return attribute.CurrentValue;
            }

            Debug.LogWarning($"[AttributeSetComponent] Attribute {type} not found!");
            return 0f;
        }

        /// <summary>
        /// Gets the maximum value of an attribute
        /// </summary>
        public float GetAttributeMaxValue(AttributeType type)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                return attribute.MaxValue;
            }

            Debug.LogWarning($"[AttributeSetComponent] Attribute {type} not found!");
            return 0f;
        }

        /// <summary>
        /// Gets the minimum value of an attribute
        /// </summary>
        public float GetAttributeMinValue(AttributeType type)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                return attribute.MinValue;
            }

            Debug.LogWarning($"[AttributeSetComponent] Attribute {type} not found!");
            return 0f;
        }

        /// <summary>
        /// Gets an attribute object by type
        /// </summary>
        public BaseAttribute GetAttribute(AttributeType type)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                return attribute;
            }
            return null;
        }

        /// <summary>
        /// Checks if an attribute exists
        /// </summary>
        public bool HasAttribute(AttributeType type)
        {
            return attributes != null && attributes.ContainsKey(type);
        }

        #endregion

        #region Attribute Modification

        /// <summary>
        /// Sets the current value of an attribute
        /// </summary>
        public void SetAttributeValue(AttributeType type, float value)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                if (attribute.IsReadOnly)
                {
                    Debug.LogWarning($"[AttributeSetComponent] Cannot set value of read-only attribute: {type}");
                    return;
                }

                float oldValue = attribute.CurrentValue;
                attribute.CurrentValue = Mathf.Clamp(value, attribute.MinValue, attribute.MaxValue);

                if (!Mathf.Approximately(oldValue, attribute.CurrentValue))
                {
                    OnAttributeChanged?.Invoke(type, oldValue, attribute.CurrentValue);
                    FireAttributeChangedEvent(type, oldValue, attribute.CurrentValue);
                }
            }
            else
            {
                Debug.LogWarning($"[AttributeSetComponent] Attribute {type} not found!");
            }
        }

        /// <summary>
        /// Sets the base value of an attribute
        /// </summary>
        public void SetAttributeBaseValue(AttributeType type, float value)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                if (attribute.IsReadOnly)
                {
                    Debug.LogWarning($"[AttributeSetComponent] Cannot set base value of read-only attribute: {type}");
                    return;
                }

                attribute.BaseValue = value;
                RecalculateAttribute(type);
            }
            else
            {
                Debug.LogWarning($"[AttributeSetComponent] Attribute {type} not found!");
            }
        }

        /// <summary>
        /// Modifies an attribute value
        /// </summary>
        public void ModifyAttribute(AttributeType type, float amount, ModifierOperation operation)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                if (attribute.IsReadOnly)
                {
                    Debug.LogWarning($"[AttributeSetComponent] Cannot modify read-only attribute: {type}");
                    return;
                }

                float oldValue = attribute.CurrentValue;

                switch (operation)
                {
                    case ModifierOperation.Add:
                        attribute.CurrentValue += amount;
                        break;
                    case ModifierOperation.Multiply:
                        attribute.CurrentValue *= amount;
                        break;
                    case ModifierOperation.Override:
                        attribute.CurrentValue = amount;
                        break;
                }

                attribute.CurrentValue = Mathf.Clamp(attribute.CurrentValue, attribute.MinValue, attribute.MaxValue);

                if (!Mathf.Approximately(oldValue, attribute.CurrentValue))
                {
                    OnAttributeChanged?.Invoke(type, oldValue, attribute.CurrentValue);
                    FireAttributeChangedEvent(type, oldValue, attribute.CurrentValue);
                }
            }
            else
            {
                Debug.LogWarning($"[AttributeSetComponent] Attribute {type} not found!");
            }
        }

        #endregion

        #region Modifier Management

        /// <summary>
        /// Adds a modifier to an attribute
        /// </summary>
        public void AddModifier(AttributeType type, AttributeModifier modifier)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                if (attribute.IsReadOnly)
                {
                    Debug.LogWarning($"[AttributeSetComponent] Cannot add modifier to read-only attribute: {type}");
                    return;
                }

                modifier.targetAttributeType = type;
                modifier.appliedTime = Time.time;
                activeModifiers.Add(modifier);
                attribute.AddModifier(modifier);

                RecalculateAttribute(type);

                OnModifierAdded?.Invoke(type, modifier);

                GASEvents.Trigger(GASEventType.AttributeModifierAdded,
                    new AttributeModifierEventData
                    {
                        attributeType = type,
                        modifier = modifier,
                        source = gameObject
                    });
            }
            else
            {
                Debug.LogWarning($"[AttributeSetComponent] Attribute {type} not found!");
            }
        }

        /// <summary>
        /// Creates and adds a modifier
        /// </summary>
        public AttributeModifier AddModifier(AttributeType type, ModifierOperation operation, float value, object source = null, float duration = -1f)
        {
            var modifier = new AttributeModifier(type, operation, value)
            {
                source = source,
                duration = duration
            };

            AddModifier(type, modifier);
            return modifier;
        }

        /// <summary>
        /// Removes a specific modifier
        /// </summary>
        public void RemoveModifier(AttributeType type, AttributeModifier modifier)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                activeModifiers.Remove(modifier);
                attribute.RemoveModifier(modifier);

                RecalculateAttribute(type);

                OnModifierRemoved?.Invoke(type, modifier);

                GASEvents.Trigger(GASEventType.AttributeModifierRemoved,
                    new AttributeModifierEventData
                    {
                        attributeType = type,
                        modifier = modifier,
                        source = gameObject
                    });
            }
        }

        /// <summary>
        /// Removes all modifiers from a specific source
        /// </summary>
        public void RemoveAllModifiersFromSource(object source)
        {
            var modifiersToRemove = new List<AttributeModifier>();

            foreach (var modifier in activeModifiers)
            {
                if (modifier.source == source)
                {
                    modifiersToRemove.Add(modifier);
                }
            }

            foreach (var modifier in modifiersToRemove)
            {
                RemoveModifier(modifier.targetAttributeType, modifier);
            }
        }

        /// <summary>
        /// Removes all modifiers for a specific attribute
        /// </summary>
        public void RemoveAllModifiersForAttribute(AttributeType type)
        {
            var modifiersToRemove = activeModifiers
                .Where(m => m.targetAttributeType == type)
                .ToList();

            foreach (var modifier in modifiersToRemove)
            {
                RemoveModifier(type, modifier);
            }
        }

        /// <summary>
        /// Gets all active modifiers
        /// </summary>
        public List<AttributeModifier> GetActiveModifiers()
        {
            return new List<AttributeModifier>(activeModifiers);
        }

        /// <summary>
        /// Gets modifiers for a specific attribute
        /// </summary>
        public List<AttributeModifier> GetModifiersForAttribute(AttributeType type)
        {
            return activeModifiers.Where(m => m.targetAttributeType == type).ToList();
        }

        /// <summary>
        /// Updates timed modifiers
        /// </summary>
        private void UpdateTimedModifiers()
        {
            var expiredModifiers = new List<AttributeModifier>();

            foreach (var modifier in activeModifiers)
            {
                if (modifier.duration > 0)
                {
                    float elapsed = Time.time - modifier.appliedTime;
                    if (elapsed >= modifier.duration)
                    {
                        expiredModifiers.Add(modifier);
                    }
                }
            }

            foreach (var modifier in expiredModifiers)
            {
                RemoveModifier(modifier.targetAttributeType, modifier);
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Recalculates the final value of an attribute
        /// </summary>
        private void RecalculateAttribute(AttributeType type)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                float oldValue = attribute.CurrentValue;
                float newValue = attribute.CalculateFinalValue();

                if (!Mathf.Approximately(oldValue, newValue))
                {
                    attribute.CurrentValue = newValue;
                    OnAttributeChanged?.Invoke(type, oldValue, newValue);
                    FireAttributeChangedEvent(type, oldValue, newValue);
                }
            }
        }

        /// <summary>
        /// Recalculates all attributes
        /// </summary>
        public void RecalculateAllAttributes()
        {
            foreach (var kvp in attributes)
            {
                if (!kvp.Value.IsReadOnly)
                {
                    RecalculateAttribute(kvp.Key);
                }
            }
        }

        /// <summary>
        /// Resets all attributes to their base values
        /// </summary>
        public void ResetAllAttributes()
        {
            // Remove all modifiers first
            activeModifiers.Clear();

            foreach (var kvp in attributes)
            {
                var attribute = kvp.Value;
                if (!attribute.IsReadOnly)
                {
                    attribute.ClearModifiers();

                    float oldValue = attribute.CurrentValue;
                    attribute.CurrentValue = attribute.BaseValue;

                    if (!Mathf.Approximately(oldValue, attribute.CurrentValue))
                    {
                        OnAttributeChanged?.Invoke(kvp.Key, oldValue, attribute.CurrentValue);
                        FireAttributeChangedEvent(kvp.Key, oldValue, attribute.CurrentValue);
                    }
                }
            }
        }

        /// <summary>
        /// Resets a specific attribute to base value
        /// </summary>
        public void ResetAttribute(AttributeType type)
        {
            if (attributes != null && attributes.TryGetValue(type, out var attribute))
            {
                if (!attribute.IsReadOnly)
                {
                    RemoveAllModifiersForAttribute(type);

                    float oldValue = attribute.CurrentValue;
                    attribute.CurrentValue = attribute.BaseValue;

                    if (!Mathf.Approximately(oldValue, attribute.CurrentValue))
                    {
                        OnAttributeChanged?.Invoke(type, oldValue, attribute.CurrentValue);
                        FireAttributeChangedEvent(type, oldValue, attribute.CurrentValue);
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Fires attribute changed event through GAS event system
        /// </summary>
        private void FireAttributeChangedEvent(AttributeType type, float oldValue, float newValue)
        {
            GASEvents.Trigger(GASEventType.AttributeChanged,
                new AttributeChangedEventData
                {
                    attributeType = type,
                    oldValue = oldValue,
                    newValue = newValue,
                    delta = newValue - oldValue,
                    source = gameObject
                });
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// Serializes current attribute values for save/load
        /// </summary>
        public AttributeSaveData SerializeAttributeValues()
        {
            var saveData = new AttributeSaveData();

            foreach (var kvp in attributes)
            {
                if (!kvp.Value.IsReadOnly && !kvp.Value.IsDerived)
                {
                    saveData.attributeValues[kvp.Key] = kvp.Value.CurrentValue;
                    saveData.baseValues[kvp.Key] = kvp.Value.BaseValue;
                }
            }

            // Save active modifiers if needed
            foreach (var modifier in activeModifiers)
            {
                if (modifier.duration < 0 || (Time.time - modifier.appliedTime) < modifier.duration)
                {
                    saveData.modifiers.Add(new ModifierSaveData
                    {
                        attributeType = modifier.targetAttributeType,
                        operation = modifier.operation,
                        value = modifier.value,
                        remainingDuration = modifier.duration > 0 ?
                            modifier.duration - (Time.time - modifier.appliedTime) : -1f
                    });
                }
            }

            return saveData;
        }

        /// <summary>
        /// Deserializes attribute values for save/load
        /// </summary>
        public void DeserializeAttributeValues(AttributeSaveData saveData)
        {
            if (saveData == null) return;

            // Restore base values
            foreach (var kvp in saveData.baseValues)
            {
                SetAttributeBaseValue(kvp.Key, kvp.Value);
            }

            // Restore current values
            foreach (var kvp in saveData.attributeValues)
            {
                SetAttributeValue(kvp.Key, kvp.Value);
            }

            // Restore modifiers
            foreach (var modifierData in saveData.modifiers)
            {
                var modifier = new AttributeModifier(
                    modifierData.attributeType,
                    modifierData.operation,
                    modifierData.value
                )
                {
                    duration = modifierData.remainingDuration,
                    source = "SavedModifier"
                };

                AddModifier(modifierData.attributeType, modifier);
            }
        }

        #endregion
    }

    #region Save Data Structures

    /// <summary>
    /// Data structure for saving attribute values
    /// </summary>
    [Serializable]
    public class AttributeSaveData
    {
        public Dictionary<AttributeType, float> attributeValues = new Dictionary<AttributeType, float>();
        public Dictionary<AttributeType, float> baseValues = new Dictionary<AttributeType, float>();
        public List<ModifierSaveData> modifiers = new List<ModifierSaveData>();
    }

    /// <summary>
    /// Data structure for saving modifiers
    /// </summary>
    [Serializable]
    public class ModifierSaveData
    {
        public AttributeType attributeType;
        public ModifierOperation operation;
        public float value;
        public float remainingDuration;
    }

    #endregion
}