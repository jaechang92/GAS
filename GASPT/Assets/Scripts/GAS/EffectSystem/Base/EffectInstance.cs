// ================================
// File: Assets/Scripts/GAS/EffectSystem/Base/EffectInstance.cs
// ================================
using System;
using UnityEngine;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Runtime instance of an applied effect
    /// </summary>
    [Serializable]
    public class EffectInstance
    {
        /// <summary>
        /// Unique identifier for this instance
        /// </summary>
        public Guid instanceId;

        /// <summary>
        /// Reference to the effect definition
        /// </summary>
        public GameplayEffect effect;

        /// <summary>
        /// Context when the effect was applied
        /// </summary>
        public EffectContext context;

        /// <summary>
        /// Time when the effect was applied
        /// </summary>
        public float startTime;

        /// <summary>
        /// Duration of this instance (-1 for infinite)
        /// </summary>
        public float duration;

        /// <summary>
        /// Last time periodic execution occurred
        /// </summary>
        public float lastPeriodicTime;

        /// <summary>
        /// Current stack count
        /// </summary>
        public int stackCount;

        /// <summary>
        /// Visual effect instance
        /// </summary>
        public GameObject visualEffect;

        /// <summary>
        /// Custom data for this instance
        /// </summary>
        public object customData;

        /// <summary>
        /// Number of periodic executions
        /// </summary>
        public int periodicExecutionCount;

        /// <summary>
        /// Is this effect paused?
        /// </summary>
        public bool isPaused;

        /// <summary>
        /// Time when the effect was paused
        /// </summary>
        public float pauseTime;

        /// <summary>
        /// Total paused duration
        /// </summary>
        public float totalPausedDuration;

        /// <summary>
        /// Gets the remaining duration
        /// </summary>
        public float RemainingDuration
        {
            get
            {
                if (duration < 0) return -1f;

                float elapsed = GetElapsedTime();
                return Mathf.Max(0, duration - elapsed);
            }
        }

        /// <summary>
        /// Gets the elapsed time since application
        /// </summary>
        public float GetElapsedTime()
        {
            if (isPaused)
            {
                return pauseTime - startTime - totalPausedDuration;
            }
            return Time.time - startTime - totalPausedDuration;
        }

        /// <summary>
        /// Gets the progress percentage (0-1)
        /// </summary>
        public float GetProgress()
        {
            if (duration <= 0) return 0;
            return Mathf.Clamp01(GetElapsedTime() / duration);
        }

        /// <summary>
        /// Checks if the effect has expired
        /// </summary>
        public bool IsExpired()
        {
            if (duration < 0) return false;
            return GetElapsedTime() >= duration;
        }

        /// <summary>
        /// Checks if periodic execution is due
        /// </summary>
        public bool IsPeriodicDue(float period)
        {
            if (isPaused) return false;

            float timeSinceLastPeriodic = Time.time - lastPeriodicTime;
            return timeSinceLastPeriodic >= period;
        }

        /// <summary>
        /// Updates the last periodic time
        /// </summary>
        public void UpdatePeriodicTime()
        {
            lastPeriodicTime = Time.time;
            periodicExecutionCount++;
        }

        /// <summary>
        /// Refreshes the duration
        /// </summary>
        public void RefreshDuration()
        {
            startTime = Time.time;
            totalPausedDuration = 0;
        }

        /// <summary>
        /// Adds to the stack count
        /// </summary>
        public void AddStack(int amount = 1)
        {
            stackCount = Mathf.Min(stackCount + amount,
                effect?.MaxStackCount ?? int.MaxValue);
        }

        /// <summary>
        /// Removes from the stack count
        /// </summary>
        public void RemoveStack(int amount = 1)
        {
            stackCount = Mathf.Max(0, stackCount - amount);
        }

        /// <summary>
        /// Pauses the effect
        /// </summary>
        public void Pause()
        {
            if (!isPaused)
            {
                isPaused = true;
                pauseTime = Time.time;
            }
        }

        /// <summary>
        /// Resumes the effect
        /// </summary>
        public void Resume()
        {
            if (isPaused)
            {
                totalPausedDuration += Time.time - pauseTime;
                isPaused = false;
            }
        }

        /// <summary>
        /// Creates a copy of this instance
        /// </summary>
        public EffectInstance Clone()
        {
            return new EffectInstance
            {
                instanceId = Guid.NewGuid(),
                effect = effect,
                context = context?.Clone(),
                startTime = startTime,
                duration = duration,
                lastPeriodicTime = lastPeriodicTime,
                stackCount = stackCount,
                visualEffect = visualEffect,
                customData = customData,
                periodicExecutionCount = periodicExecutionCount,
                isPaused = isPaused,
                pauseTime = pauseTime,
                totalPausedDuration = totalPausedDuration
            };
        }
    }
}