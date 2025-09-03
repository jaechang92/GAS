// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Base/IAbilitySource.cs
// ================================
using GAS.AttributeSystem;
using GAS.EffectSystem;
using GAS.TagSystem;
using System;
using UnityEngine;

namespace GAS.AbilitySystem
{
    /// <summary>
    /// Interface for objects that can own and execute abilities
    /// </summary>
    public interface IAbilitySource
    {
        /// <summary>
        /// Gets the GameObject that owns this ability source
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Gets the transform of the ability source
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Gets the AttributeSetComponent for cost calculations
        /// </summary>
        AttributeSetComponent AttributeComponent { get; }

        /// <summary>
        /// Gets the TagComponent for tag requirements
        /// </summary>
        TagComponent TagComponent { get; }

        /// <summary>
        /// Gets the EffectComponent for applying effects
        /// </summary>
        EffectComponent EffectComponent { get; }

        /// <summary>
        /// Gets the current level of the ability source
        /// </summary>
        int Level { get; }

        /// <summary>
        /// Checks if the source is alive and can use abilities
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Gets the team ID for targeting purposes
        /// </summary>
        int TeamId { get; }
    }
}