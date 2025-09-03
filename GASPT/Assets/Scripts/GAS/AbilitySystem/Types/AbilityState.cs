// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Types/AbilityState.cs
// ================================
namespace GAS.AbilitySystem
{
    /// <summary>
    /// Represents the current state of an ability
    /// </summary>
    public enum AbilityState
    {
        /// <summary>
        /// Ability is not active
        /// </summary>
        Idle,

        /// <summary>
        /// Ability is checking activation conditions
        /// </summary>
        PreActivation,

        /// <summary>
        /// Ability is currently activating (playing startup animation, etc.)
        /// </summary>
        Activating,

        /// <summary>
        /// Ability is active and executing
        /// </summary>
        Active,

        /// <summary>
        /// Ability is channeling
        /// </summary>
        Channeling,

        /// <summary>
        /// Ability is in cooldown
        /// </summary>
        Cooldown,

        /// <summary>
        /// Ability is ending
        /// </summary>
        Ending,

        /// <summary>
        /// Ability was cancelled
        /// </summary>
        Cancelled,

        /// <summary>
        /// Ability is blocked by tags or conditions
        /// </summary>
        Blocked
    }

    /// <summary>
    /// Reason for ability failure
    /// </summary>
    public enum AbilityFailureReason
    {
        None,
        InsufficientResources,
        OnCooldown,
        RequiredTagsMissing,
        BlockedByTags,
        SourceDead,
        AlreadyActive,
        NotLearned,
        Silenced,
        Stunned,
        OutOfRange,
        InvalidTarget,
        Custom
    }

    /// <summary>
    /// Type of ability activation
    /// </summary>
    public enum AbilityActivationType
    {
        /// <summary>
        /// Activated by player input
        /// </summary>
        Manual,

        /// <summary>
        /// Activated automatically by conditions
        /// </summary>
        Automatic,

        /// <summary>
        /// Activated by another ability
        /// </summary>
        Triggered,

        /// <summary>
        /// Always active (passive)
        /// </summary>
        Passive
    }
}