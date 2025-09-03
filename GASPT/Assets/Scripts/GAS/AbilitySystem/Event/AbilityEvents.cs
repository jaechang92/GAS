// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Events/AbilityEvents.cs
// ================================
using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.TagSystem;

namespace GAS.AbilitySystem
{
    /// <summary>
    /// Types of ability events
    /// </summary>
    public enum AbilityEventType
    {
        PreActivate,
        Activated,
        Executed,
        Ended,
        Cancelled,
        Failed,
        CooldownStarted,
        CooldownEnded,
        InputPressed,
        InputReleased,
        TargetAcquired,
        TargetLost
    }

    /// <summary>
    /// Data for ability events
    /// </summary>
    public class AbilityEventData
    {
        public GameplayAbility ability;
        public AbilitySpec spec;
        public AbilityActivationInfo activationInfo;
        public AbilityEventType eventType;
        public object customData;

        public AbilityEventData()
        {
        }

        public AbilityEventData(GameplayAbility ability, AbilityEventType eventType)
        {
            this.ability = ability;
            this.eventType = eventType;
        }
    }

    /// <summary>
    /// Event data for ability cancellation requests
    /// </summary>
    public class AbilityCancelEventData
    {
        public IAbilitySource source;
        public List<GameplayTag> tagsToCancel;
        public GameplayAbility cancellingAbility;
        public bool cancelAll;

        public AbilityCancelEventData()
        {
            tagsToCancel = new List<GameplayTag>();
        }
    }

    /// <summary>
    /// Event data for ability grants
    /// </summary>
    public class AbilityGrantEventData
    {
        public IAbilitySource source;
        public GameplayAbility ability;
        public AbilitySpec spec;
        public int level;
        public int inputId;

        public AbilityGrantEventData(IAbilitySource source, GameplayAbility ability, int level = 1)
        {
            this.source = source;
            this.ability = ability;
            this.level = level;
            this.inputId = -1;
        }
    }

    /// <summary>
    /// Event data for cooldown changes
    /// </summary>
    public class AbilityCooldownEventData
    {
        public GameplayAbility ability;
        public AbilitySpec spec;
        public float duration;
        public float remaining;
        public bool started;

        public AbilityCooldownEventData(AbilitySpec spec, float duration, bool started)
        {
            this.spec = spec;
            this.ability = spec?.ability;
            this.duration = duration;
            this.remaining = spec?.RemainingCooldown ?? 0;
            this.started = started;
        }
    }
}