// ================================
// File: Assets/Scripts/GAS/EffectSystem/Base/EffectContext.cs
// ================================
using System;
using UnityEngine;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Contains context information for effect application
    /// </summary>
    [Serializable]
    public class EffectContext
    {
        /// <summary>
        /// The GameObject that initiated the effect
        /// </summary>
        public GameObject instigator;

        /// <summary>
        /// The target GameObject of the effect
        /// </summary>
        public GameObject target;

        /// <summary>
        /// The ability that triggered this effect (if any)
        /// </summary>
        public object sourceAbility;

        /// <summary>
        /// The level of the effect or ability
        /// </summary>
        public int level = 1;

        /// <summary>
        /// Magnitude multiplier for the effect
        /// </summary>
        public float magnitude = 1f;

        /// <summary>
        /// Location where the effect should be applied
        /// </summary>
        public Vector3 effectLocation;

        /// <summary>
        /// Hit location for targeted effects
        /// </summary>
        public Vector3 hitLocation;

        /// <summary>
        /// Direction of the effect
        /// </summary>
        public Vector3 direction;

        /// <summary>
        /// Is this a critical effect?
        /// </summary>
        public bool isCritical;

        /// <summary>
        /// Was the effect blocked?
        /// </summary>
        public bool isBlocked;

        /// <summary>
        /// Was the effect dodged?
        /// </summary>
        public bool isDodged;

        /// <summary>
        /// Duration multiplier
        /// </summary>
        public float durationMultiplier = 1f;

        /// <summary>
        /// Period multiplier for periodic effects
        /// </summary>
        public float periodMultiplier = 1f;

        /// <summary>
        /// Custom data for specific effects
        /// </summary>
        public object customData;

        /// <summary>
        /// Time when the context was created
        /// </summary>
        public float timestamp;

        /// <summary>
        /// Source effect that created this context (for chained effects)
        /// </summary>
        public GameplayEffect sourceEffect;

        /// <summary>
        /// Tags to ignore during application
        /// </summary>
        public string[] ignoreTags;

        /// <summary>
        /// Additional targets for area effects
        /// </summary>
        public GameObject[] additionalTargets;

        /// <summary>
        /// Constructor with basic parameters
        /// </summary>
        public EffectContext(GameObject instigator, GameObject target)
        {
            this.instigator = instigator;
            this.target = target;
            this.timestamp = Time.time;
            this.effectLocation = target != null ? target.transform.position : Vector3.zero;
        }

        /// <summary>
        /// Constructor with full parameters
        /// </summary>
        public EffectContext(GameObject instigator, GameObject target, Vector3 location, int level = 1)
        {
            this.instigator = instigator;
            this.target = target;
            this.effectLocation = location;
            this.level = level;
            this.timestamp = Time.time;
        }

        /// <summary>
        /// Creates a copy of this context
        /// </summary>
        public EffectContext Clone()
        {
            return new EffectContext(instigator, target)
            {
                sourceAbility = sourceAbility,
                level = level,
                magnitude = magnitude,
                effectLocation = effectLocation,
                hitLocation = hitLocation,
                direction = direction,
                isCritical = isCritical,
                isBlocked = isBlocked,
                isDodged = isDodged,
                durationMultiplier = durationMultiplier,
                periodMultiplier = periodMultiplier,
                customData = customData,
                timestamp = timestamp,
                sourceEffect = sourceEffect,
                ignoreTags = ignoreTags,
                additionalTargets = additionalTargets
            };
        }

        /// <summary>
        /// Creates a context for a secondary target
        /// </summary>
        public EffectContext CreateForTarget(GameObject newTarget)
        {
            var newContext = Clone();
            newContext.target = newTarget;
            newContext.effectLocation = newTarget.transform.position;
            return newContext;
        }

        /// <summary>
        /// Checks if the context is valid
        /// </summary>
        public bool IsValid()
        {
            return target != null && target.activeInHierarchy;
        }

        /// <summary>
        /// Gets the distance between instigator and target
        /// </summary>
        public float GetDistance()
        {
            if (instigator == null || target == null)
                return 0;

            return Vector3.Distance(instigator.transform.position, target.transform.position);
        }

        /// <summary>
        /// Gets the direction from instigator to target
        /// </summary>
        public Vector3 GetDirection()
        {
            if (direction != Vector3.zero)
                return direction;

            if (instigator == null || target == null)
                return Vector3.forward;

            return (target.transform.position - instigator.transform.position).normalized;
        }

        /// <summary>
        /// Sets hit information
        /// </summary>
        public void SetHitInfo(RaycastHit hit)
        {
            hitLocation = hit.point;
            direction = hit.normal;
        }

        /// <summary>
        /// Adds additional context data
        /// </summary>
        public void AddCustomData(string key, object value)
        {
            if (customData == null)
            {
                customData = new System.Collections.Generic.Dictionary<string, object>();
            }

            if (customData is System.Collections.Generic.Dictionary<string, object> dict)
            {
                dict[key] = value;
            }
        }

        /// <summary>
        /// Gets custom data by key
        /// </summary>
        public T GetCustomData<T>(string key)
        {
            if (customData is System.Collections.Generic.Dictionary<string, object> dict)
            {
                if (dict.TryGetValue(key, out object value))
                {
                    return (T)value;
                }
            }
            return default(T);
        }
    }

    /// <summary>
    /// Extension class for effect context creation
    /// </summary>
    public static class EffectContextExtensions
    {
        /// <summary>
        /// Creates a basic effect context
        /// </summary>
        public static EffectContext CreateEffectContext(this GameObject instigator, GameObject target)
        {
            return new EffectContext(instigator, target);
        }

        /// <summary>
        /// Creates an effect context with location
        /// </summary>
        public static EffectContext CreateEffectContext(this GameObject instigator, GameObject target, Vector3 location)
        {
            return new EffectContext(instigator, target, location);
        }

        /// <summary>
        /// Creates a self-targeted effect context
        /// </summary>
        public static EffectContext CreateSelfEffectContext(this GameObject gameObject)
        {
            return new EffectContext(gameObject, gameObject);
        }
    }
}