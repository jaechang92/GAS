// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Data/AbilityActivationInfo.cs
// ================================
using System;
using UnityEngine;

namespace GAS.AbilitySystem
{
    /// <summary>
    /// Contains information about ability activation
    /// </summary>
    [Serializable]
    public struct AbilityActivationInfo
    {
        /// <summary>
        /// The source that activated the ability
        /// </summary>
        public IAbilitySource source;

        /// <summary>
        /// The target of the ability (can be null)
        /// </summary>
        public GameObject target;

        /// <summary>
        /// The position where the ability was activated
        /// </summary>
        public Vector3 activationPosition;

        /// <summary>
        /// The direction of activation
        /// </summary>
        public Vector3 activationDirection;

        /// <summary>
        /// The time when the ability was activated
        /// </summary>
        public float activationTime;

        /// <summary>
        /// Type of activation
        /// </summary>
        public AbilityActivationType activationType;

        /// <summary>
        /// Input ID that triggered this ability
        /// </summary>
        public int inputId;

        /// <summary>
        /// Custom data for specific abilities
        /// </summary>
        public object customData;

        /// <summary>
        /// Creates a new activation info
        /// </summary>
        public AbilityActivationInfo(IAbilitySource source, GameObject target = null)
        {
            this.source = source;
            this.target = target;
            this.activationPosition = source.Transform.position;
            this.activationDirection = source.Transform.forward;
            this.activationTime = Time.time;
            this.activationType = AbilityActivationType.Manual;
            this.inputId = -1;
            this.customData = null;
        }

        /// <summary>
        /// Creates activation info with position and direction
        /// </summary>
        public static AbilityActivationInfo CreateWithPosition(
            IAbilitySource source,
            Vector3 position,
            Vector3 direction)
        {
            return new AbilityActivationInfo
            {
                source = source,
                target = null,
                activationPosition = position,
                activationDirection = direction.normalized,
                activationTime = Time.time,
                activationType = AbilityActivationType.Manual,
                inputId = -1,
                customData = null
            };
        }
    }
}
