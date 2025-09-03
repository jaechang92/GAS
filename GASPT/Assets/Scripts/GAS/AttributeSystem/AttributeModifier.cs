// ================================
// File: Assets/Scripts/GAS/AttributeSystem/AttributeModifier.cs
// ================================
using System;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// Modifier that can be applied to attributes
    /// </summary>
    [Serializable]
    public class AttributeModifier
    {
        [Header("Target")]
        public AttributeType targetAttributeType;

        [Header("Operation")]
        public ModifierOperation operation;
        public float value;

        [Header("Source")]
        public object source;
        public string sourceName;

        [Header("Settings")]
        public int priority = 0;
        public float duration = -1f; // -1 = permanent
        public bool refreshable = true;

        [Header("Runtime")]
        public float appliedTime;
        public Guid id;

        /// <summary>
        /// Constructor
        /// </summary>
        public AttributeModifier(AttributeType type, ModifierOperation op, float val)
        {
            targetAttributeType = type;
            operation = op;
            value = val;
            priority = 0;
            duration = -1f;
            id = Guid.NewGuid();
            appliedTime = Time.time;
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public AttributeModifier(AttributeType type, ModifierOperation op, float val, object source, float duration, int priority)
        {
            targetAttributeType = type;
            operation = op;
            value = val;
            this.source = source;
            this.duration = duration;
            this.priority = priority;
            id = Guid.NewGuid();
            appliedTime = Time.time;

            if (source != null)
            {
                sourceName = source.ToString();
            }
        }

        /// <summary>
        /// Is this modifier expired?
        /// </summary>
        public bool IsExpired => duration > 0 && (Time.time - appliedTime) >= duration;

        /// <summary>
        /// Remaining duration
        /// </summary>
        public float RemainingDuration => duration > 0 ? Mathf.Max(0, duration - (Time.time - appliedTime)) : -1f;

        /// <summary>
        /// Refresh the duration
        /// </summary>
        public void Refresh()
        {
            if (refreshable && duration > 0)
            {
                appliedTime = Time.time;
            }
        }

        /// <summary>
        /// Clone this modifier
        /// </summary>
        public AttributeModifier Clone()
        {
            return new AttributeModifier(targetAttributeType, operation, value, source, duration, priority)
            {
                refreshable = this.refreshable,
                sourceName = this.sourceName
            };
        }
    }
}