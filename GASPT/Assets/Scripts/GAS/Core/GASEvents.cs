// ================================
// File: Assets/Scripts/GAS/Core/GASEvents.cs (Updated)
// ================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS.Core
{
    /// <summary>
    /// Central event types for the GAS system
    /// </summary>
    public enum GASEventType
    {
        // Attribute Events
        AttributeChanged,
        AttributeMaxChanged,
        AttributeModifierAdded,
        AttributeModifierRemoved,

        // Tag Events
        TagAdded,
        TagRemoved,
        TagCountChanged,

        // Effect Events
        EffectApplied,
        EffectRemoved,
        EffectStackChanged,
        EffectDurationUpdated,
        EffectPeriodic,

        // Ability Events (NEW)
        AbilityEvent,
        AbilityGranted,
        AbilityRevoked,
        AbilityCooldownChanged,
        AbilityCancelRequested,
        AbilityInputReceived,
        AbilityTargetChanged,

        // System Events
        SystemInitialized,
        SystemShutdown,
        ComponentAdded,
        ComponentRemoved,

        // Combat Events
        DamageDealt,
        DamageReceived,
        HealingDealt,
        HealingReceived,
        Death,
        Respawn
    }

    /// <summary>
    /// Central event manager for the GAS system
    /// </summary>
    public static class GASEvents
    {
        private static Dictionary<GASEventType, List<Action<object>>> eventHandlers =
            new Dictionary<GASEventType, List<Action<object>>>();

        private static Dictionary<GASEventType, List<WeakReference>> weakHandlers =
            new Dictionary<GASEventType, List<WeakReference>>();

        /// <summary>
        /// Subscribe to an event
        /// </summary>
        public static void Subscribe(GASEventType eventType, Action<object> handler, bool useWeakReference = false)
        {
            if (handler == null) return;

            if (useWeakReference)
            {
                if (!weakHandlers.ContainsKey(eventType))
                {
                    weakHandlers[eventType] = new List<WeakReference>();
                }
                weakHandlers[eventType].Add(new WeakReference(handler));
            }
            else
            {
                if (!eventHandlers.ContainsKey(eventType))
                {
                    eventHandlers[eventType] = new List<Action<object>>();
                }

                if (!eventHandlers[eventType].Contains(handler))
                {
                    eventHandlers[eventType].Add(handler);
                }
            }
        }

        /// <summary>
        /// Unsubscribe from an event
        /// </summary>
        public static void Unsubscribe(GASEventType eventType, Action<object> handler)
        {
            if (handler == null) return;

            // Check strong references
            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType].Remove(handler);

                if (eventHandlers[eventType].Count == 0)
                {
                    eventHandlers.Remove(eventType);
                }
            }

            // Check weak references
            if (weakHandlers.ContainsKey(eventType))
            {
                weakHandlers[eventType].RemoveAll(wr => !wr.IsAlive || wr.Target.Equals(handler));

                if (weakHandlers[eventType].Count == 0)
                {
                    weakHandlers.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Trigger an event
        /// </summary>
        public static void Trigger(GASEventType eventType, object data = null)
        {
            // Trigger strong reference handlers
            if (eventHandlers.ContainsKey(eventType))
            {
                var handlers = new List<Action<object>>(eventHandlers[eventType]);
                foreach (var handler in handlers)
                {
                    try
                    {
                        handler?.Invoke(data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error in GAS event handler for {eventType}: {e.Message}");
                    }
                }
            }

            // Trigger weak reference handlers
            if (weakHandlers.ContainsKey(eventType))
            {
                var deadReferences = new List<WeakReference>();

                foreach (var weakRef in weakHandlers[eventType])
                {
                    if (weakRef.IsAlive)
                    {
                        try
                        {
                            (weakRef.Target as Action<object>)?.Invoke(data);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error in weak GAS event handler for {eventType}: {e.Message}");
                        }
                    }
                    else
                    {
                        deadReferences.Add(weakRef);
                    }
                }

                // Clean up dead references
                foreach (var deadRef in deadReferences)
                {
                    weakHandlers[eventType].Remove(deadRef);
                }

                if (weakHandlers[eventType].Count == 0)
                {
                    weakHandlers.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Clear all event handlers
        /// </summary>
        public static void ClearAll()
        {
            eventHandlers.Clear();
            weakHandlers.Clear();
        }

        /// <summary>
        /// Clear handlers for a specific event type
        /// </summary>
        public static void Clear(GASEventType eventType)
        {
            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers.Remove(eventType);
            }

            if (weakHandlers.ContainsKey(eventType))
            {
                weakHandlers.Remove(eventType);
            }
        }

        /// <summary>
        /// Get the number of handlers for an event type
        /// </summary>
        public static int GetHandlerCount(GASEventType eventType)
        {
            int count = 0;

            if (eventHandlers.ContainsKey(eventType))
            {
                count += eventHandlers[eventType].Count;
            }

            if (weakHandlers.ContainsKey(eventType))
            {
                count += weakHandlers[eventType].Count;
            }

            return count;
        }

        /// <summary>
        /// Check if an event type has any handlers
        /// </summary>
        public static bool HasHandlers(GASEventType eventType)
        {
            return GetHandlerCount(eventType) > 0;
        }
    }

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

    /// <summary>
    /// Generic event data for simple events
    /// </summary>
    public class SimpleGASEventData : GASEventData
    {
        public object data;

        public SimpleGASEventData(GameObject source, object data = null)
        {
            this.source = source;
            this.data = data;
        }
    }
}