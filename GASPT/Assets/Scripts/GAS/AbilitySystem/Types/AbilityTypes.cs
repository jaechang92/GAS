// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Types/AbilityTypes.cs
// ================================
using System;
using UnityEngine;
using GAS.TagSystem;
using static GAS.Core.GASConstants;

namespace GAS.AbilitySystem
{
    /// <summary>
    /// Defines how an ability targets
    /// </summary>
    public enum AbilityTargetType
    {
        None,
        Self,
        Target,
        Point,
        Direction,
        Area,
        Custom
    }

    /// <summary>
    /// Defines the execution policy of an ability
    /// </summary>
    public enum AbilityExecutionPolicy
    {
        /// <summary>
        /// Can only have one instance active
        /// </summary>
        Single,

        /// <summary>
        /// Can have multiple instances
        /// </summary>
        Multiple,

        /// <summary>
        /// New activation cancels previous
        /// </summary>
        Replace
    }

    /// <summary>
    /// Priority for ability execution
    /// </summary>
    public enum AbilityPriority
    {
        Low = 0,
        Normal = 50,
        High = 100,
        Critical = 200
    }

    /// <summary>
    /// Ability cost type
    /// </summary>
    [Serializable]
    public struct AbilityCostData
    {
        private string attributeName;

        public AttributeType attributeType;

        /// <summary>
        /// Base cost amount
        /// </summary>
        public float baseAmount;

        /// <summary>
        /// Cost multiplier per level
        /// </summary>
        public float levelMultiplier;


        public AbilityCostData(AttributeType type, float baseAmount)
        {
            this.attributeName = type.ToString();
            this.attributeType = type;
            this.baseAmount = baseAmount;
            this.levelMultiplier = 1.0f;
        }

        /// <summary>
        /// Calculate actual cost for a given level
        /// </summary>
        public float CalculateCost(int level)
        {
            return baseAmount * Mathf.Pow(levelMultiplier, level - 1);
        }

        public string GetAttributeName()
        {
            if (string.IsNullOrEmpty(attributeName) && attributeType != AttributeType.None)
            {
                attributeName = attributeType.ToString();
            }
            return attributeName;
        }
    }

    /// <summary>
    /// Cooldown configuration
    /// </summary>
    [Serializable]
    public struct AbilityCooldownData
    {
        /// <summary>
        /// Base cooldown duration in seconds
        /// </summary>
        public float baseDuration;

        /// <summary>
        /// Cooldown reduction per level
        /// </summary>
        public float levelReduction;

        /// <summary>
        /// Minimum cooldown (cannot go below this)
        /// </summary>
        public float minimumDuration;

        /// <summary>
        /// Tags that affect cooldown
        /// </summary>
        public GameplayTag cooldownTag;

        /// <summary>
        /// Should cooldown start on activation or completion?
        /// </summary>
        public bool startOnActivation;

        public AbilityCooldownData(float baseDuration)
        {
            this.baseDuration = baseDuration;
            this.levelReduction = 0;
            this.minimumDuration = 0.1f;
            this.cooldownTag = null;
            this.startOnActivation = false;
        }

        /// <summary>
        /// Calculate actual cooldown for a given level
        /// </summary>
        public float CalculateCooldown(int level)
        {
            float cooldown = baseDuration - (levelReduction * (level - 1));
            return Mathf.Max(minimumDuration, cooldown);
        }
    }

    /// <summary>
    /// Result of ability execution
    /// </summary>
    public struct AbilityExecutionResult
    {
        public bool success;
        public AbilityFailureReason failureReason;
        public string customMessage;
        public object resultData;

        public static AbilityExecutionResult Success(object data = null)
        {
            return new AbilityExecutionResult
            {
                success = true,
                failureReason = AbilityFailureReason.None,
                customMessage = null,
                resultData = data
            };
        }

        public static AbilityExecutionResult Failure(AbilityFailureReason reason, string message = null)
        {
            return new AbilityExecutionResult
            {
                success = false,
                failureReason = reason,
                customMessage = message,
                resultData = null
            };
        }
    }
}