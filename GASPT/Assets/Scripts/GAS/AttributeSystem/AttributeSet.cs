// ================================
// File: Assets/Scripts/GAS/AttributeSystem/AttributeSet.cs
// ================================
using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.Core;
using static GAS.Core.GASConstants;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// Base class for defining a set of attributes
    /// </summary>
    [Serializable]
    public abstract class AttributeSet
    {
        [Header("Set Information")]
        [SerializeField] protected string setName;
        [SerializeField] protected string description;

        // Attribute storage
        protected Dictionary<AttributeType, BaseAttribute> attributes;
        protected AttributeSetComponent ownerComponent;

        // Properties
        public string SetName => setName;
        public string Description => description;
        public Dictionary<AttributeType, BaseAttribute> Attributes => attributes;

        /// <summary>
        /// Constructor
        /// </summary>
        public AttributeSet()
        {
            attributes = new Dictionary<AttributeType, BaseAttribute>();
            InitializeDefaultAttributes();
        }

        /// <summary>
        /// Initialize this attribute set with an owner component
        /// </summary>
        public virtual void Initialize(AttributeSetComponent component)
        {
            ownerComponent = component;

            // Initialize each attribute
            foreach (var kvp in attributes)
            {
                kvp.Value.Initialize(component);
            }

            // Setup attribute relationships
            SetupAttributeRelationships();

            // Register attributes with component
            RegisterAttributes();
        }

        /// <summary>
        /// Initialize default attributes - Override in derived classes
        /// </summary>
        protected abstract void InitializeDefaultAttributes();

        /// <summary>
        /// Setup relationships between attributes
        /// </summary>
        protected virtual void SetupAttributeRelationships()
        {
            // Override in derived classes to setup dependencies
        }

        /// <summary>
        /// Register attributes with the owner component
        /// </summary>
        protected virtual void RegisterAttributes()
        {
            if (ownerComponent == null) return;

            foreach (var kvp in attributes)
            {
                ownerComponent.RegisterAttribute(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Add an attribute to this set
        /// </summary>
        protected void AddAttribute(AttributeType type, BaseAttribute attribute)
        {
            if (attribute == null) return;

            attribute.AttributeType = type;
            attributes[type] = attribute;
        }

        /// <summary>
        /// Get an attribute by type
        /// </summary>
        public BaseAttribute GetAttribute(AttributeType type)
        {
            attributes.TryGetValue(type, out var attribute);
            return attribute;
        }

        /// <summary>
        /// Check if this set contains an attribute
        /// </summary>
        public bool HasAttribute(AttributeType type)
        {
            return attributes.ContainsKey(type);
        }

        /// <summary>
        /// Pre-attribute change callback
        /// </summary>
        public virtual void PreAttributeChange(AttributeType type, float oldValue, float newValue)
        {
            // Override in derived classes for custom logic
        }

        /// <summary>
        /// Post-attribute change callback
        /// </summary>
        public virtual void PostAttributeChange(AttributeType type, float oldValue, float newValue)
        {
            // Override in derived classes for custom logic
        }
    }

    /// <summary>
    /// Character attribute set - common RPG attributes
    /// </summary>
    [Serializable]
    public class CharacterAttributeSet : AttributeSet
    {
        protected override void InitializeDefaultAttributes()
        {
            setName = "Character Attributes";
            description = "Basic character attributes for RPG gameplay";

            // Vital Attributes
            AddAttribute(AttributeType.Health, new VitalAttribute
            {
                BaseValue = 100f,
                CurrentValue = 100f,
                MinValue = 0f,
                MaxValue = 100f
            });

            AddAttribute(AttributeType.HealthMax, new BaseAttribute
            {
                BaseValue = 100f,
                CurrentValue = 100f,
                MinValue = 1f,
                MaxValue = 9999f
            });

            AddAttribute(AttributeType.Mana, new VitalAttribute
            {
                BaseValue = 50f,
                CurrentValue = 50f,
                MinValue = 0f,
                MaxValue = 50f
            });

            AddAttribute(AttributeType.ManaMax, new BaseAttribute
            {
                BaseValue = 50f,
                CurrentValue = 50f,
                MinValue = 0f,
                MaxValue = 9999f
            });

            AddAttribute(AttributeType.Stamina, new VitalAttribute
            {
                BaseValue = 100f,
                CurrentValue = 100f,
                MinValue = 0f,
                MaxValue = 100f
            });

            AddAttribute(AttributeType.StaminaMax, new BaseAttribute
            {
                BaseValue = 100f,
                CurrentValue = 100f,
                MinValue = 0f,
                MaxValue = 9999f
            });

            // Combat Attributes
            AddAttribute(AttributeType.AttackPower, new BaseAttribute
            {
                BaseValue = 10f,
                MinValue = 0f,
                MaxValue = 9999f
            });

            AddAttribute(AttributeType.AttackSpeed, new BaseAttribute
            {
                BaseValue = 1f,
                MinValue = 0.1f,
                MaxValue = 5f
            });

            AddAttribute(AttributeType.DefensePower, new BaseAttribute
            {
                BaseValue = 5f,
                MinValue = 0f,
                MaxValue = 9999f
            });

            AddAttribute(AttributeType.Armor, new BaseAttribute
            {
                BaseValue = 10f,
                MinValue = 0f,
                MaxValue = 9999f
            });

            AddAttribute(AttributeType.MagicResistance, new BaseAttribute
            {
                BaseValue = 10f,
                MinValue = 0f,
                MaxValue = 9999f
            });

            // Critical Attributes
            AddAttribute(AttributeType.CriticalChance, new BaseAttribute
            {
                BaseValue = 5f,
                MinValue = 0f,
                MaxValue = 100f
            });

            AddAttribute(AttributeType.CriticalDamage, new BaseAttribute
            {
                BaseValue = 50f,
                MinValue = 0f,
                MaxValue = 500f
            });

            // Movement Attributes
            AddAttribute(AttributeType.MovementSpeed, new BaseAttribute
            {
                BaseValue = 5f,
                MinValue = 0f,
                MaxValue = 20f
            });

            // Defensive Attributes
            AddAttribute(AttributeType.Block, new BaseAttribute
            {
                BaseValue = 0f,
                MinValue = 0f,
                MaxValue = 100f
            });

            AddAttribute(AttributeType.Dodge, new BaseAttribute
            {
                BaseValue = 0f,
                MinValue = 0f,
                MaxValue = 100f
            });

            AddAttribute(AttributeType.Tenacity, new BaseAttribute
            {
                BaseValue = 0f,
                MinValue = 0f,
                MaxValue = 100f
            });
        }

        protected override void SetupAttributeRelationships()
        {
            // Setup max value relationships
            if (HasAttribute(AttributeType.Health) && HasAttribute(AttributeType.HealthMax))
            {
                var health = GetAttribute(AttributeType.Health);
                var healthMax = GetAttribute(AttributeType.HealthMax);
                health.MaxValue = healthMax.CurrentValue;
            }

            if (HasAttribute(AttributeType.Mana) && HasAttribute(AttributeType.ManaMax))
            {
                var mana = GetAttribute(AttributeType.Mana);
                var manaMax = GetAttribute(AttributeType.ManaMax);
                mana.MaxValue = manaMax.CurrentValue;
            }

            if (HasAttribute(AttributeType.Stamina) && HasAttribute(AttributeType.StaminaMax))
            {
                var stamina = GetAttribute(AttributeType.Stamina);
                var staminaMax = GetAttribute(AttributeType.StaminaMax);
                stamina.MaxValue = staminaMax.CurrentValue;
            }
        }

        public override void PostAttributeChange(AttributeType type, float oldValue, float newValue)
        {
            // Update max values when max attributes change
            switch (type)
            {
                case AttributeType.HealthMax:
                    if (HasAttribute(AttributeType.Health))
                    {
                        var health = GetAttribute(AttributeType.Health);
                        health.MaxValue = newValue;
                        health.CurrentValue = Mathf.Min(health.CurrentValue, newValue);
                    }
                    break;

                case AttributeType.ManaMax:
                    if (HasAttribute(AttributeType.Mana))
                    {
                        var mana = GetAttribute(AttributeType.Mana);
                        mana.MaxValue = newValue;
                        mana.CurrentValue = Mathf.Min(mana.CurrentValue, newValue);
                    }
                    break;

                case AttributeType.StaminaMax:
                    if (HasAttribute(AttributeType.Stamina))
                    {
                        var stamina = GetAttribute(AttributeType.Stamina);
                        stamina.MaxValue = newValue;
                        stamina.CurrentValue = Mathf.Min(stamina.CurrentValue, newValue);
                    }
                    break;

                case AttributeType.Health:
                    // Check for death
                    if (newValue <= 0 && oldValue > 0)
                    {
                        FireDeathEvent();
                    }
                    // Check for revival
                    else if (newValue > 0 && oldValue <= 0)
                    {
                        FireReviveEvent();
                    }
                    break;
            }
        }

        private void FireDeathEvent()
        {
            if (ownerComponent != null)
            {
                GASEvents.Trigger(GASEventType.Death, new DeathEventData
                {
                    source = ownerComponent.gameObject
                });
            }
        }

        private void FireReviveEvent()
        {
            if (ownerComponent != null)
            {
                GASEvents.Trigger(GASEventType.Respawn, new RespawnEventData
                {
                    source = ownerComponent.gameObject,
                    respawnHealth = GetAttribute(AttributeType.Health)?.CurrentValue ?? 0
                });
            }
        }
    }

    /// <summary>
    /// Vital attribute with regeneration support
    /// </summary>
    [Serializable]
    public class VitalAttribute : BaseAttribute
    {
        [Header("Regeneration")]
        [SerializeField] private float baseRegenRate = 0f;
        [SerializeField] private float regenDelay = 5f;
        [SerializeField] private float lastDamageTime;

        public float BaseRegenRate
        {
            get => baseRegenRate;
            set => baseRegenRate = value;
        }

        public float RegenDelay
        {
            get => regenDelay;
            set => regenDelay = value;
        }

        public override void Initialize(AttributeSetComponent component)
        {
            base.Initialize(component);
            regenerates = baseRegenRate > 0;
            regenRate = baseRegenRate;
        }

        public override void ProcessRegeneration(float deltaTime)
        {
            if (!regenerates || isReadOnly || isDerived)
                return;

            // Check regen delay
            if (Time.time - lastDamageTime < regenDelay)
                return;

            // Apply regeneration
            float regenAmount = regenRate * deltaTime;
            CurrentValue = Mathf.Min(CurrentValue + regenAmount, MaxValue);
        }

        public void OnDamageTaken()
        {
            lastDamageTime = Time.time;
        }
    }

    /// <summary>
    /// Custom attribute set for specific game mechanics
    /// </summary>
    [Serializable]
    public class CustomAttributeSet : AttributeSet
    {
        [SerializeField] private List<AttributeDefinition> customAttributes = new List<AttributeDefinition>();

        public CustomAttributeSet(List<AttributeDefinition> definitions)
        {
            customAttributes = definitions;
        }

        protected override void InitializeDefaultAttributes()
        {
            setName = "Custom Attributes";
            description = "Custom attribute set for specific gameplay";

            foreach (var definition in customAttributes)
            {
                if (definition != null)
                {
                    var attribute = definition.CreateAttribute();
                    AddAttribute(definition.type, attribute);
                }
            }
        }
    }

    /// <summary>
    /// Minimal attribute set for simple entities
    /// </summary>
    [Serializable]
    public class MinimalAttributeSet : AttributeSet
    {
        protected override void InitializeDefaultAttributes()
        {
            setName = "Minimal Attributes";
            description = "Minimal attribute set for simple entities";

            // Only Health
            AddAttribute(AttributeType.Health, new BaseAttribute
            {
                BaseValue = 100f,
                CurrentValue = 100f,
                MinValue = 0f,
                MaxValue = 100f
            });

            AddAttribute(AttributeType.HealthMax, new BaseAttribute
            {
                BaseValue = 100f,
                CurrentValue = 100f,
                MinValue = 1f,
                MaxValue = 9999f
            });
        }
    }
}