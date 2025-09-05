// ================================
// File: Assets/Scripts/GAS/EffectSystem/Base/IEffectReceiver.cs
// ================================
using System.Collections.Generic;
using UnityEngine;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Interface for GameObjects that can receive effects
    /// </summary>
    public interface IEffectReceiver
    {
        /// <summary>
        /// GameObject that implements this interface
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Transform of the receiver
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Can this receiver currently receive effects?
        /// </summary>
        bool CanReceiveEffects { get; }

        /// <summary>
        /// Is the receiver immune to effects?
        /// </summary>
        bool IsImmune { get; }

        /// <summary>
        /// Called before an effect is applied
        /// </summary>
        void OnPreEffectApply(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Called after an effect is applied
        /// </summary>
        void OnPostEffectApply(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Called before an effect is removed
        /// </summary>
        void OnPreEffectRemove(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Called after an effect is removed
        /// </summary>
        void OnPostEffectRemove(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Gets immunity tags (effects with these tags are ignored)
        /// </summary>
        List<string> GetImmunityTags();

        /// <summary>
        /// Checks if immune to a specific effect
        /// </summary>
        bool IsImmuneToEffect(GameplayEffect effect);

        /// <summary>
        /// Gets effect resistance percentage (0-100)
        /// </summary>
        float GetEffectResistance(GameplayEffect effect);

        /// <summary>
        /// Modifies incoming effect context
        /// </summary>
        void ModifyIncomingEffect(GameplayEffect effect, ref EffectContext context);
    }

    /// <summary>
    /// Optional interface for advanced effect handling
    /// </summary>
    public interface IAdvancedEffectReceiver : IEffectReceiver
    {
        /// <summary>
        /// Called when an effect stacks
        /// </summary>
        void OnEffectStack(GameplayEffect effect, int newStackCount, int previousStackCount);

        /// <summary>
        /// Called periodically for periodic effects
        /// </summary>
        void OnEffectPeriodic(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Called when an effect expires naturally
        /// </summary>
        void OnEffectExpired(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Called when an effect is dispelled
        /// </summary>
        void OnEffectDispelled(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Called when an effect is refreshed
        /// </summary>
        void OnEffectRefreshed(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Validates if an effect can be applied
        /// </summary>
        bool ValidateEffectApplication(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Gets the maximum stack count override for an effect
        /// </summary>
        int GetMaxStackOverride(GameplayEffect effect);

        /// <summary>
        /// Gets duration modifier for incoming effects
        /// </summary>
        float GetDurationModifier(GameplayEffect effect);

        /// <summary>
        /// Gets magnitude modifier for incoming effects
        /// </summary>
        float GetMagnitudeModifier(GameplayEffect effect);
    }

    /// <summary>
    /// Interface for objects that can grant effects to others
    /// </summary>
    public interface IEffectGranter
    {
        /// <summary>
        /// GameObject that grants effects
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Gets bonus effect power
        /// </summary>
        float GetEffectPower();

        /// <summary>
        /// Gets bonus effect duration
        /// </summary>
        float GetEffectDurationBonus();

        /// <summary>
        /// Modifies outgoing effect context
        /// </summary>
        void ModifyOutgoingEffect(GameplayEffect effect, ref EffectContext context);

        /// <summary>
        /// Called when an effect is successfully granted
        /// </summary>
        void OnEffectGranted(GameObject target, GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Called when an effect grant fails
        /// </summary>
        void OnEffectGrantFailed(GameObject target, GameplayEffect effect, string reason);

        /// <summary>
        /// Gets additional effects to apply with the main effect
        /// </summary>
        List<GameplayEffect> GetAdditionalEffects(GameplayEffect mainEffect);
    }

    /// <summary>
    /// Interface for objects that can reflect effects
    /// </summary>
    public interface IEffectReflector
    {
        /// <summary>
        /// Chance to reflect effects (0-100)
        /// </summary>
        float ReflectChance { get; }

        /// <summary>
        /// Can this effect be reflected?
        /// </summary>
        bool CanReflect(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Called when an effect is reflected
        /// </summary>
        void OnEffectReflected(GameplayEffect effect, EffectContext originalContext, GameObject newTarget);

        /// <summary>
        /// Modifies the reflected effect context
        /// </summary>
        EffectContext ModifyReflectedContext(EffectContext originalContext, GameObject newTarget);
    }

    /// <summary>
    /// Interface for objects that can absorb effects
    /// </summary>
    public interface IEffectAbsorber
    {
        /// <summary>
        /// Maximum absorption amount
        /// </summary>
        float MaxAbsorption { get; }

        /// <summary>
        /// Current absorption remaining
        /// </summary>
        float CurrentAbsorption { get; }

        /// <summary>
        /// Can this effect be absorbed?
        /// </summary>
        bool CanAbsorb(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Absorbs damage/effect magnitude
        /// </summary>
        float AbsorbMagnitude(float magnitude, GameplayEffect effect);

        /// <summary>
        /// Called when absorption shield breaks
        /// </summary>
        void OnAbsorptionDepleted();

        /// <summary>
        /// Called when effect is absorbed
        /// </summary>
        void OnEffectAbsorbed(GameplayEffect effect, float absorbedAmount);
    }
}