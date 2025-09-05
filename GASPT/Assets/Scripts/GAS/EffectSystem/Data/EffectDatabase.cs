// ================================
// File: Assets/Scripts/GAS/EffectSystem/Data/EffectDatabase.cs
// ================================
using System;
using System.Collections.Generic;
using System.Linq;
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

            // Create instance from base effect
            var effect = Instantiate(baseEffect);

            // Apply modifications
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
            // This would need reflection or specific implementation
            // based on the modification type
            switch (type)
            {
                case ModificationType.Duration:
                    // Modify duration
                    break;
                case ModificationType.Magnitude:
                    // Modify magnitude
                    break;
                case ModificationType.StackCount:
                    // Modify stack count
                    break;
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