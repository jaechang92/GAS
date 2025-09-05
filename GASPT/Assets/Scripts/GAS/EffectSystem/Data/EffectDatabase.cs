// ================================
// File: Assets/Scripts/GAS/EffectSystem/Data/EffectDatabase.cs
// Updated to use Clone method and modification APIs
// ================================
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Centralized database for managing all game effects
    /// </summary>
    [CreateAssetMenu(fileName = "EffectDatabase", menuName = "GAS/Effect Database")]
    public class EffectDatabase : ScriptableObject
    {
        [Header("Effect Collections")]
        [SerializeField] private List<GameplayEffect> allEffects = new List<GameplayEffect>();
        [SerializeField] private List<EffectCategory> categories = new List<EffectCategory>();

        [Header("Effect Presets")]
        [SerializeField] private List<EffectPreset> effectPresets = new List<EffectPreset>();

        [Header("Database Settings")]
        [SerializeField] private bool autoGenerateIds = true;
        [SerializeField] private string idPrefix = "EFFECT_";

        // Runtime cache
        private Dictionary<string, GameplayEffect> effectLookup;
        private Dictionary<string, List<GameplayEffect>> categoryLookup;
        private Dictionary<string, EffectPreset> presetLookup;

        /// <summary>
        /// Initialize the database caches
        /// </summary>
        public void Initialize()
        {
            BuildEffectLookup();
            BuildCategoryLookup();
            BuildPresetLookup();

            if (autoGenerateIds)
            {
                GenerateEffectIds();
            }
        }

        /// <summary>
        /// Get effect by ID
        /// </summary>
        public GameplayEffect GetEffect(string effectId)
        {
            if (effectLookup == null)
                BuildEffectLookup();

            effectLookup.TryGetValue(effectId, out GameplayEffect effect);
            return effect;
        }

        /// <summary>
        /// Get all effects in a category
        /// </summary>
        public List<GameplayEffect> GetEffectsByCategory(string categoryName)
        {
            if (categoryLookup == null)
                BuildCategoryLookup();

            categoryLookup.TryGetValue(categoryName, out List<GameplayEffect> effects);
            return effects ?? new List<GameplayEffect>();
        }

        /// <summary>
        /// Get effects by tag
        /// </summary>
        public List<GameplayEffect> GetEffectsByTag(string tagName)
        {
            return allEffects.Where(e =>
                e.AssetTags != null &&
                e.AssetTags.Tags.Any(t => t.TagName.Contains(tagName))
            ).ToList();
        }

        /// <summary>
        /// Get effect preset
        /// </summary>
        public EffectPreset GetPreset(string presetName)
        {
            if (presetLookup == null)
                BuildPresetLookup();

            presetLookup.TryGetValue(presetName, out EffectPreset preset);
            return preset;
        }

        /// <summary>
        /// Create effect from preset
        /// </summary>
        public GameplayEffect CreateEffectFromPreset(string presetName, int level = 1)
        {
            var preset = GetPreset(presetName);
            if (preset == null)
                return null;

            return preset.CreateEffect(level);
        }

        /// <summary>
        /// Search effects by name
        /// </summary>
        public List<GameplayEffect> SearchEffects(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();
            return allEffects.Where(e =>
                e.EffectName.ToLower().Contains(searchTerm) ||
                e.Description.ToLower().Contains(searchTerm)
            ).ToList();
        }

        /// <summary>
        /// Get all buff effects
        /// </summary>
        public List<GameplayEffect> GetBuffs()
        {
            return GetEffectsByTag("Buff");
        }

        /// <summary>
        /// Get all debuff effects
        /// </summary>
        public List<GameplayEffect> GetDebuffs()
        {
            return GetEffectsByTag("Debuff");
        }

        /// <summary>
        /// Get all damage effects
        /// </summary>
        public List<GameplayEffect> GetDamageEffects()
        {
            return GetEffectsByTag("Damage");
        }

        /// <summary>
        /// Get all healing effects
        /// </summary>
        public List<GameplayEffect> GetHealingEffects()
        {
            return GetEffectsByTag("Heal");
        }

        /// <summary>
        /// Build effect lookup cache
        /// </summary>
        private void BuildEffectLookup()
        {
            effectLookup = new Dictionary<string, GameplayEffect>();
            foreach (var effect in allEffects)
            {
                if (effect != null && !string.IsNullOrEmpty(effect.EffectId))
                {
                    effectLookup[effect.EffectId] = effect;
                }
            }
        }

        /// <summary>
        /// Build category lookup cache
        /// </summary>
        private void BuildCategoryLookup()
        {
            categoryLookup = new Dictionary<string, List<GameplayEffect>>();
            foreach (var category in categories)
            {
                if (category != null && !string.IsNullOrEmpty(category.categoryName))
                {
                    categoryLookup[category.categoryName] = category.effects;
                }
            }
        }

        /// <summary>
        /// Build preset lookup cache
        /// </summary>
        private void BuildPresetLookup()
        {
            presetLookup = new Dictionary<string, EffectPreset>();
            foreach (var preset in effectPresets)
            {
                if (preset != null && !string.IsNullOrEmpty(preset.presetName))
                {
                    presetLookup[preset.presetName] = preset;
                }
            }
        }

        /// <summary>
        /// Generate unique IDs for effects
        /// </summary>
        private void GenerateEffectIds()
        {
            int counter = 1;
            foreach (var effect in allEffects)
            {
                if (effect != null && string.IsNullOrEmpty(effect.EffectId))
                {
                    // This would need editor script to modify ScriptableObject
                    // effect.EffectId = $"{idPrefix}{counter:D4}";
                    counter++;
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validate database in editor
        /// </summary>
        public void ValidateDatabase()
        {
            // Check for duplicate IDs
            var duplicates = allEffects
                .Where(e => e != null && !string.IsNullOrEmpty(e.EffectId))
                .GroupBy(e => e.EffectId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var duplicate in duplicates)
            {
                Debug.LogError($"Duplicate effect ID found: {duplicate}");
            }

            // Check for null entries
            for (int i = allEffects.Count - 1; i >= 0; i--)
            {
                if (allEffects[i] == null)
                {
                    Debug.LogWarning($"Null effect found at index {i}");
                    allEffects.RemoveAt(i);
                }
            }

            // Check for missing IDs
            foreach (var effect in allEffects)
            {
                if (effect != null && string.IsNullOrEmpty(effect.EffectId))
                {
                    Debug.LogWarning($"Effect '{effect.EffectName}' has no ID");
                }
            }
        }

        /// <summary>
        /// Add effect to database
        /// </summary>
        public void AddEffect(GameplayEffect effect)
        {
            if (effect != null && !allEffects.Contains(effect))
            {
                allEffects.Add(effect);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// Remove effect from database
        /// </summary>
        public void RemoveEffect(GameplayEffect effect)
        {
            if (allEffects.Remove(effect))
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }

    /// <summary>
    /// Category for organizing effects
    /// </summary>
    [Serializable]
    public class EffectCategory
    {
        public string categoryName;
        public string description;
        public Color categoryColor = Color.white;
        public List<GameplayEffect> effects = new List<GameplayEffect>();
    }

    /// <summary>
    /// Preset configuration for effects
    /// </summary>
    [Serializable]
    public class EffectPreset
    {
        public string presetName;
        public string description;
        public GameplayEffect baseEffect;
        public List<EffectModification> modifications = new List<EffectModification>();

        public GameplayEffect CreateEffect(int level)
        {
            if (baseEffect == null)
                return null;

            // Use the Clone method to create a safe copy
            var effect = baseEffect.Clone();

            // Apply modifications using the new modification API
            foreach (var mod in modifications)
            {
                mod.ApplyToEffect(effect, level);
            }

            return effect;
        }
    }

    /// <summary>
    /// Modification to apply to an effect preset
    /// </summary>
    [Serializable]
    public class EffectModification
    {
        public ModificationType type;
        public string propertyName;
        public float floatValue;
        public int intValue;
        public bool boolValue;
        public AnimationCurve curveValue;

        public void ApplyToEffect(GameplayEffect effect, int level)
        {
            if (effect == null) return;

            switch (type)
            {
                case ModificationType.Duration:
                    ApplyDurationModification(effect, level);
                    break;

                case ModificationType.Magnitude:
                    ApplyMagnitudeModification(effect, level);
                    break;

                case ModificationType.StackCount:
                    ApplyStackCountModification(effect, level);
                    break;

                case ModificationType.Period:
                    ApplyPeriodModification(effect, level);
                    break;

                case ModificationType.Custom:
                    ApplyCustomModification(effect, level);
                    break;
            }
        }

        private void ApplyDurationModification(GameplayEffect effect, int level)
        {
            switch (curveValue != null && curveValue.keys.Length > 0)
            {
                case true:
                    // Use curve for level-based scaling
                    float curveValue = this.curveValue.Evaluate((float)level);
                    effect.SetDuration(floatValue * curveValue);
                    break;
                default:
                    // Simple linear scaling
                    effect.SetDuration(floatValue * level);
                    break;
            }
        }

        private void ApplyMagnitudeModification(GameplayEffect effect, int level)
        {
            switch (curveValue != null && curveValue.keys.Length > 0)
            {
                case true:
                    // Use curve for level-based scaling
                    float curveMultiplier = this.curveValue.Evaluate((float)level);
                    effect.ModifyModifierMagnitude(1f + (floatValue * curveMultiplier));
                    break;
                default:
                    // Simple multiplier based on level
                    float multiplier = 1f + (floatValue * (level - 1));
                    effect.ModifyModifierMagnitude(multiplier);
                    break;
            }
        }

        private void ApplyStackCountModification(GameplayEffect effect, int level)
        {
            // Stack count increases with level
            int additionalStacks = intValue * (level - 1);
            effect.ModifyMaxStackCount(additionalStacks);
        }

        private void ApplyPeriodModification(GameplayEffect effect, int level)
        {
            switch (curveValue != null && curveValue.keys.Length > 0)
            {
                case true:
                    // Use curve for period scaling
                    float curveValue = this.curveValue.Evaluate((float)level);
                    effect.SetPeriod(floatValue * curveValue);
                    break;
                default:
                    // Period decreases as level increases (faster ticks)
                    float periodReduction = floatValue * (level - 1) * 0.1f;
                    effect.ModifyPeriod(-periodReduction);
                    break;
            }
        }

        private void ApplyCustomModification(GameplayEffect effect, int level)
        {
            // For custom modifications, you might want to extend this
            // or create specific modification methods in GameplayEffect

            if (!string.IsNullOrEmpty(propertyName))
            {
                // Example custom modifications
                switch (propertyName.ToLower())
                {
                    case "name":
                        effect.SetEffectName($"{effect.EffectName} Lv.{level}");
                        break;

                    case "description":
                        effect.SetDescription($"{effect.Description} (Level {level})");
                        break;

                    case "refreshduration":
                        // This would need a setter in GameplayEffect
                        // effect.SetRefreshDurationOnStack(boolValue);
                        break;

                    default:
                        Debug.LogWarning($"Unknown custom property: {propertyName}");
                        break;
                }
            }
        }
    }

    public enum ModificationType
    {
        Duration,
        Magnitude,
        StackCount,
        Period,
        Custom
    }
}