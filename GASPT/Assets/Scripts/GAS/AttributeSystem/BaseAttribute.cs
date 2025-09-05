// ================================
// File: Assets/Scripts/GAS/AttributeSystem/BaseAttribute.cs
// ================================
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// Base class for all attributes
    /// </summary>
    [Serializable]
    public class BaseAttribute
    {
        [Header("Configuration")]
        [SerializeField] protected AttributeType attributeType;
        [SerializeField] protected string attributeName;
        [SerializeField] protected string description;

        [Header("Values")]
        [SerializeField] protected float baseValue;
        [SerializeField] protected float currentValue;
        [SerializeField] protected float minValue = 0f;
        [SerializeField] protected float maxValue = float.MaxValue;

        [Header("Settings")]
        [SerializeField] protected bool isReadOnly = false;
        [SerializeField] protected bool isDerived = false;
        [SerializeField] protected bool regenerates = false;
        [SerializeField] protected float regenRate = 0f;

        // Runtime
        protected List<AttributeModifier> modifiers = new List<AttributeModifier>();
        protected AttributeSetComponent ownerComponent;

        // Properties
        public AttributeType AttributeType
        {
            get => attributeType;
            set => attributeType = value;
        }

        public string AttributeName => attributeName;
        public string Description => description;

        public float BaseValue
        {
            get => baseValue;
            set => baseValue = value;
        }

        public float CurrentValue
        {
            get => currentValue;
            set => currentValue = Mathf.Clamp(value, minValue, maxValue);
        }

        public float MinValue
        {
            get => minValue;
            set => minValue = value;
        }

        public float MaxValue
        {
            get => maxValue;
            set => maxValue = value;
        }

        public bool IsReadOnly => isReadOnly;
        public bool IsDerived => isDerived;
        public bool Regenerates => regenerates;
        public float RegenRate => regenRate;

        public BaseAttribute ()
        {

        }

        /// <summary>
        /// Initialize with owner component
        /// </summary>
        public virtual void Initialize(AttributeSetComponent component)
        {
            ownerComponent = component;
            if (!isDerived && currentValue == 0)
            {
                currentValue = baseValue;
            }
        }

        /// <summary>
        /// Get value (can be overridden for derived attributes)
        /// </summary>
        public virtual float GetValue()
        {
            if (isDerived)
            {
                return CalculateDerivedValue();
            }
            return currentValue;
        }

        /// <summary>
        /// Calculate final value with modifiers
        /// </summary>
        public virtual float CalculateFinalValue()
        {
            if (isDerived)
            {
                return CalculateDerivedValue();
            }

            float finalValue = baseValue;

            // Apply modifiers by priority and operation type
            // First: Add operations
            var addModifiers = modifiers.Where(m => m.operation == ModifierOperation.Add)
                                       .OrderBy(m => m.priority);
            foreach (var modifier in addModifiers)
            {
                finalValue += modifier.value;
            }

            // Second: Multiply operations
            var multiplyModifiers = modifiers.Where(m => m.operation == ModifierOperation.Multiply)
                                            .OrderBy(m => m.priority);
            foreach (var modifier in multiplyModifiers)
            {
                finalValue *= modifier.value;
            }

            // Last: Override operations (use highest priority override)
            var overrideModifier = modifiers.Where(m => m.operation == ModifierOperation.Override)
                                          .OrderByDescending(m => m.priority)
                                          .FirstOrDefault();
            if (overrideModifier != null)
            {
                finalValue = overrideModifier.value;
            }

            return Mathf.Clamp(finalValue, minValue, maxValue);
        }

        /// <summary>
        /// For derived attributes - override this
        /// </summary>
        protected virtual float CalculateDerivedValue()
        {
            return currentValue;
        }

        /// <summary>
        /// Add a modifier
        /// </summary>
        public void AddModifier(AttributeModifier modifier)
        {
            modifiers.Add(modifier);
        }

        /// <summary>
        /// Remove a modifier
        /// </summary>
        public void RemoveModifier(AttributeModifier modifier)
        {
            modifiers.Remove(modifier);
        }

        /// <summary>
        /// Clear all modifiers
        /// </summary>
        public void ClearModifiers()
        {
            modifiers.Clear();
        }

        /// <summary>
        /// Get all modifiers
        /// </summary>
        public List<AttributeModifier> GetModifiers()
        {
            return new List<AttributeModifier>(modifiers);
        }

        /// <summary>
        /// Process regeneration
        /// </summary>
        public virtual void ProcessRegeneration(float deltaTime)
        {
            if (regenerates && !isReadOnly && !isDerived)
            {
                float regenAmount = regenRate * deltaTime;
                CurrentValue = Mathf.Min(CurrentValue + regenAmount, MaxValue);
            }
        }

        /// <summary>
        /// Clone this attribute
        /// </summary>
        public virtual BaseAttribute Clone()
        {
            var clone = (BaseAttribute)MemberwiseClone();
            clone.modifiers = new List<AttributeModifier>();
            return clone;
        }
    }
}
