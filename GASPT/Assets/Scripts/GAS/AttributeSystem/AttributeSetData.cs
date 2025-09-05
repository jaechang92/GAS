// ================================
// File: Assets/Scripts/GAS/AttributeSystem/AttributeSetData.cs
// ================================
using System;
using System.Collections.Generic;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// ScriptableObject that defines a set of attributes
    /// </summary>
    [CreateAssetMenu(fileName = "AttributeSet", menuName = "GAS/Attribute Set")]
    public class AttributeSetData : ScriptableObject
    {
        [Header("Attributes")]
        [SerializeField] private List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>();

        /// <summary>
        /// Initialize attributes on a component
        /// </summary>
        public virtual void InitializeAttributes(AttributeSetComponent component)
        {
            foreach (var definition in attributeDefinitions)
            {
                var attribute = definition.CreateAttribute();
                component.RegisterAttribute(definition.type, attribute);
            }

            // Register any derived attributes
            RegisterDerivedAttributes(component);
        }

        /// <summary>
        /// Override to register derived attributes
        /// </summary>
        protected virtual void RegisterDerivedAttributes(AttributeSetComponent component)
        {
            // Example: Register percentage attributes
            if (component.HasAttribute(AttributeType.Health))
            {
                component.RegisterAttribute(AttributeType.HealthPercent,
                    new PercentageAttribute(AttributeType.Health, AttributeType.HealthMax));
            }

            if (component.HasAttribute(AttributeType.Mana))
            {
                component.RegisterAttribute(AttributeType.ManaPercent,
                    new PercentageAttribute(AttributeType.Mana, AttributeType.ManaMax));
            }

            if (component.HasAttribute(AttributeType.Stamina))
            {
                component.RegisterAttribute(AttributeType.StaminaPercent,
                    new PercentageAttribute(AttributeType.Stamina, AttributeType.StaminaMax));
            }
        }
    }

    /// <summary>
    /// Definition for creating an attribute
    /// </summary>
    [Serializable]
    public class AttributeDefinition
    {
        public AttributeType type;
        public string name;
        public string description;
        public float baseValue = 100f;
        public float minValue = 0f;
        public float maxValue = 999f;
        public bool regenerates = false;
        public float regenRate = 0f;

        public BaseAttribute CreateAttribute()
        {
            var attribute = new BaseAttribute
            {
                AttributeType = type,
                BaseValue = baseValue,
                CurrentValue = baseValue,
                MinValue = minValue,
                MaxValue = maxValue
            };

            return attribute;
        }
    }
}