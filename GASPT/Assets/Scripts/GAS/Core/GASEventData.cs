// ================================
// File: Assets/Scripts/GAS/Core/GASEventData.cs
// Complete collection of all GAS Event Data classes
// ================================
using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.AttributeSystem;
using GAS.TagSystem;
using GAS.EffectSystem;
using GAS.AbilitySystem;
using static GAS.Core.GASConstants;

namespace GAS.Core
{
    /// <summary>
    /// Base class for GAS event data
    /// </summary>
    public abstract class GASEventData
    {
        public float timestamp;
        public GameObject source;

        protected GASEventData()
        {
            timestamp = Time.time;
        }
    }

    #region Attribute Events

    /// <summary>
    /// Event data for attribute changes
    /// </summary>
    public class AttributeChangedEventData : GASEventData
    {
        public AttributeType attributeType;
        public float oldValue;
        public float newValue;
        public float delta;

        public AttributeChangedEventData()
        {
        }
    }

    /// <summary>
    /// Event data for attribute modifier events
    /// </summary>
    public class AttributeModifierEventData : GASEventData
    {
        public AttributeType attributeType;
        public AttributeModifier modifier;

        public AttributeModifierEventData()
        {
        }
    }

    #endregion

    #region Tag Events

    /// <summary>
    /// Event data for tag changes
    /// </summary>
    public class TagEventData : GASEventData
    {
        public GameplayTag tag;
        public int count;
        public int previousCount;

        public TagEventData()
        {
        }
    }

    /// <summary>
    /// Event data for tag requirement checks
    /// </summary>
    public class TagRequirementEventData : GASEventData
    {
        public TagRequirement requirement;
        public bool passed;
        public List<GameplayTag> missingTags;
        public List<GameplayTag> blockedTags;

        public TagRequirementEventData()
        {
            missingTags = new List<GameplayTag>();
            blockedTags = new List<GameplayTag>();
        }
    }

    #endregion

    #region Effect Events

    /// <summary>
    /// Event data for effect applied
    /// </summary>
    public class EffectAppliedEventData : GASEventData
    {
        public GameplayEffect effect;
        public EffectContext context;
        public GameObject target;

        public EffectAppliedEventData()
        {
        }
    }

    /// <summary>
    /// Event data for effect removed
    /// </summary>
    public class EffectRemovedEventData : GASEventData
    {
        public GameplayEffect effect;
        public EffectContext context;
        public GameObject target;
        public bool wasNaturalExpiration;

        public EffectRemovedEventData()
        {
        }
    }

    /// <summary>
    /// Event data for effect stack changes
    /// </summary>
    public class EffectStackEventData : GASEventData
    {
        public GameplayEffect effect;
        public GameObject target;
        public int newStackCount;
        public int previousStackCount;

        public EffectStackEventData()
        {
        }
    }

    /// <summary>
    /// Event data for periodic effects
    /// </summary>
    public class EffectPeriodicEventData : GASEventData
    {
        public GameplayEffect effect;
        public EffectContext context;
        public GameObject target;
        public int tickCount;

        public EffectPeriodicEventData()
        {
        }
    }

    /// <summary>
    /// Event data for effect duration updates
    /// </summary>
    public class EffectDurationEventData : GASEventData
    {
        public GameplayEffect effect;
        public GameObject target;
        public float newDuration;
        public float remainingDuration;

        public EffectDurationEventData()
        {
        }
    }

    #endregion

    #region Ability Events

    /// <summary>
    /// Event data for ability events
    /// </summary>
    public class AbilityEventData : GASEventData
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
    public class AbilityCancelEventData : GASEventData
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
    public class AbilityGrantEventData : GASEventData
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
    public class AbilityCooldownEventData : GASEventData
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

    /// <summary>
    /// Event data for ability input
    /// </summary>
    public class AbilityInputEventData : GASEventData
    {
        public int inputId;
        public bool pressed;
        public bool released;
        public bool held;
        public Vector2 screenPosition;
        public Vector3 worldPosition;
        public GameObject targetObject;

        public AbilityInputEventData()
        {
        }
    }

    /// <summary>
    /// Event data for ability target changes
    /// </summary>
    public class AbilityTargetEventData : GASEventData
    {
        public GameplayAbility ability;
        public GameObject previousTarget;
        public GameObject newTarget;
        public Vector3 targetPosition;
        public bool isValid;

        public AbilityTargetEventData()
        {
        }
    }

    #endregion

    #region Combat Events

    /// <summary>
    /// Event data for damage
    /// </summary>
    public class DamageEventData : GASEventData
    {
        public GameObject target;
        public float damage;
        public DamageType damageType;
        public bool isCritical;
        public bool isBlocked;
        public bool isDodged;

        public DamageEventData()
        {
        }
    }

    /// <summary>
    /// Event data for healing
    /// </summary>
    public class HealingEventData : GASEventData
    {
        public GameObject target;
        public float healing;
        public bool isCritical;
        public bool isOverheal;

        public HealingEventData()
        {
        }
    }

    /// <summary>
    /// Event data for death
    /// </summary>
    public class DeathEventData : GASEventData
    {
        public GameObject killer;
        public DamageType lastDamageType;
        public GameplayAbility killingAbility;
        public GameplayEffect killingEffect;

        public DeathEventData()
        {
        }
    }

    /// <summary>
    /// Event data for respawn
    /// </summary>
    public class RespawnEventData : GASEventData
    {
        public Vector3 respawnPosition;
        public float respawnHealth;
        public float respawnMana;
        public bool fullRestore;

        public RespawnEventData()
        {
        }
    }

    #endregion

    #region System Events

    /// <summary>
    /// Event data for system initialization
    /// </summary>
    public class SystemInitEventData : GASEventData
    {
        public string systemName;
        public bool success;
        public string errorMessage;

        public SystemInitEventData()
        {
        }
    }

    /// <summary>
    /// Event data for component events
    /// </summary>
    public class ComponentEventData : GASEventData
    {
        public Component component;
        public Type componentType;
        public bool added;

        public ComponentEventData()
        {
        }
    }

    #endregion
}