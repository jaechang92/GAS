// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Data/AbilitySpec.cs
// ================================
using System;
using UnityEngine;

namespace GAS.AbilitySystem
{
    /// <summary>
    /// Represents an instance of an ability granted to a character
    /// </summary>
    [Serializable]
    public class AbilitySpec
    {
        /// <summary>
        /// Unique ID for this ability instance
        /// </summary>
        public int instanceId;

        /// <summary>
        /// Reference to the ability definition
        /// </summary>
        public GameplayAbility ability;

        /// <summary>
        /// Current level of this ability
        /// </summary>
        public int level;

        /// <summary>
        /// Input binding for this ability
        /// </summary>
        public int inputId;

        /// <summary>
        /// Current state of the ability
        /// </summary>
        public AbilityState state;

        /// <summary>
        /// Time when cooldown ends
        /// </summary>
        public float cooldownEndTime;

        /// <summary>
        /// Number of times this ability has been used
        /// </summary>
        public int useCount;

        /// <summary>
        /// Custom data for this ability instance
        /// </summary>
        public object customData;

        /// <summary>
        /// Is this ability currently active?
        /// </summary>
        public bool IsActive => state == AbilityState.Active ||
                                state == AbilityState.Channeling ||
                                state == AbilityState.Activating;

        /// <summary>
        /// Is this ability on cooldown?
        /// </summary>
        public bool IsOnCooldown => Time.time < cooldownEndTime;

        /// <summary>
        /// Remaining cooldown time
        /// </summary>
        public float RemainingCooldown => Mathf.Max(0, cooldownEndTime - Time.time);

        /// <summary>
        /// Creates a new ability spec
        /// </summary>
        public AbilitySpec(GameplayAbility ability, int level = 1, int inputId = -1)
        {
            this.instanceId = GetNextInstanceId();
            this.ability = ability;
            this.level = level;
            this.inputId = inputId;
            this.state = AbilityState.Idle;
            this.cooldownEndTime = 0;
            this.useCount = 0;
            this.customData = null;
        }

        /// <summary>
        /// Sets the cooldown for this ability
        /// </summary>
        public void SetCooldown(float duration)
        {
            cooldownEndTime = Time.time + duration;
            if (state == AbilityState.Idle)
            {
                state = AbilityState.Cooldown;
            }
        }

        /// <summary>
        /// Clears the cooldown
        /// </summary>
        public void ClearCooldown()
        {
            cooldownEndTime = 0;
            if (state == AbilityState.Cooldown)
            {
                state = AbilityState.Idle;
            }
        }

        /// <summary>
        /// Increments use count
        /// </summary>
        public void IncrementUseCount()
        {
            useCount++;
        }

        // Static counter for generating unique instance IDs
        private static int nextInstanceId = 1;

        private static int GetNextInstanceId()
        {
            return nextInstanceId++;
        }
    }
}