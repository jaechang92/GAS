// ================================
// File: Assets/Scripts/GAS/AttributeSystem/DerivedAttributes.cs
// ================================
using System;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// Base class for attributes that derive their value from other attributes
    /// </summary>
    [Serializable]
    public abstract class DerivedAttribute : BaseAttribute
    {
        public override void Initialize(AttributeSetComponent component)
        {
            base.Initialize(component);
            isReadOnly = true;
            isDerived = true;
        }

        public override float GetValue()
        {
            return CalculateDerivedValue();
        }

        public override float CalculateFinalValue()
        {
            return CalculateDerivedValue();
        }
    }

    /// <summary>
    /// Attribute that represents a percentage of current/max
    /// </summary>
    [Serializable]
    public class PercentageAttribute : DerivedAttribute
    {
        private AttributeType currentAttributeType;
        private AttributeType maxAttributeType;

        public PercentageAttribute(AttributeType current, AttributeType max)
        {
            currentAttributeType = current;
            maxAttributeType = max;
            attributeName = $"{current}Percent";
            description = $"Percentage of {current}/{max}";
        }

        protected override float CalculateDerivedValue()
        {
            if (ownerComponent == null) return 0f;

            float current = ownerComponent.GetAttributeValue(currentAttributeType);
            float max = ownerComponent.GetAttributeValue(maxAttributeType);

            if (max <= 0) return 0f;

            return (current / max) * 100f;
        }
    }

    /// <summary>
    /// Attribute that represents a fixed percentage of another attribute's max value
    /// </summary>
    [Serializable]
    public class FixedPercentageAttribute : DerivedAttribute
    {
        private AttributeType sourceAttributeType;
        private float percentage;

        public FixedPercentageAttribute(AttributeType source, float percent)
        {
            sourceAttributeType = source;
            percentage = percent;
            attributeName = $"{source}Max{percent}Pct";
            description = $"{percent}% of max {source}";
        }

        protected override float CalculateDerivedValue()
        {
            if (ownerComponent == null) return 0f;

            float max = ownerComponent.GetAttributeMaxValue(sourceAttributeType);
            return max * (percentage / 100f);
        }
    }

    /// <summary>
    /// Attribute that represents a ratio between two attributes
    /// </summary>
    [Serializable]
    public class RatioAttribute : DerivedAttribute
    {
        private AttributeType numeratorType;
        private AttributeType denominatorType;
        private float multiplier;

        public RatioAttribute(AttributeType numerator, AttributeType denominator, float mult = 1f)
        {
            numeratorType = numerator;
            denominatorType = denominator;
            multiplier = mult;
            attributeName = $"{numerator}To{denominator}Ratio";
            description = $"Ratio of {numerator} to {denominator}";
        }

        protected override float CalculateDerivedValue()
        {
            if (ownerComponent == null) return 0f;

            float numerator = ownerComponent.GetAttributeValue(numeratorType);
            float denominator = ownerComponent.GetAttributeValue(denominatorType);

            if (denominator <= 0) return 0f;

            return (numerator / denominator) * multiplier;
        }
    }

    /// <summary>
    /// Attribute that sums multiple other attributes
    /// </summary>
    [Serializable]
    public class SumAttribute : DerivedAttribute
    {
        private AttributeType[] sourceTypes;
        private float[] weights;

        public SumAttribute(AttributeType[] sources, float[] weights = null)
        {
            sourceTypes = sources;
            this.weights = weights ?? new float[sources.Length];

            // Default weights to 1 if not specified
            for (int i = 0; i < this.weights.Length; i++)
            {
                if (this.weights[i] == 0)
                    this.weights[i] = 1f;
            }
        }

        protected override float CalculateDerivedValue()
        {
            if (ownerComponent == null) return 0f;

            float sum = 0f;
            for (int i = 0; i < sourceTypes.Length && i < weights.Length; i++)
            {
                sum += ownerComponent.GetAttributeValue(sourceTypes[i]) * weights[i];
            }

            return sum;
        }
    }
}