// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Data/AbilityTagRequirements.cs  
// ================================
using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.TagSystem;

namespace GAS.AbilitySystem
{
    /// <summary>
    /// Manages tag requirements for abilities
    /// </summary>
    [Serializable]
    public class AbilityTagRequirements
    {
        [Header("Required Tags")]
        [SerializeField] private List<GameplayTag> requiredTags = new List<GameplayTag>();
        [SerializeField] private bool requireAllRequired = true;

        [Header("Blocked Tags")]
        [SerializeField] private List<GameplayTag> blockedTags = new List<GameplayTag>();
        [SerializeField] private bool blockIfAnyPresent = true;

        [Header("Source Tags")]
        [SerializeField] private List<GameplayTag> sourceRequiredTags = new List<GameplayTag>();
        [SerializeField] private List<GameplayTag> sourceBlockedTags = new List<GameplayTag>();

        [Header("Target Tags")]
        [SerializeField] private List<GameplayTag> targetRequiredTags = new List<GameplayTag>();
        [SerializeField] private List<GameplayTag> targetBlockedTags = new List<GameplayTag>();

        /// <summary>
        /// Checks if all requirements are met
        /// </summary>
        public bool CheckRequirements(TagComponent sourceTagComponent, TagComponent targetTagComponent = null)
        {
            // Check source required tags
            if (sourceRequiredTags.Count > 0)
            {
                if (requireAllRequired)
                {
                    foreach (var tag in sourceRequiredTags)
                    {
                        if (!sourceTagComponent.HasTag(tag))
                            return false;
                    }
                }
                else
                {
                    bool hasAny = false;
                    foreach (var tag in sourceRequiredTags)
                    {
                        if (sourceTagComponent.HasTag(tag))
                        {
                            hasAny = true;
                            break;
                        }
                    }
                    if (!hasAny) return false;
                }
            }

            // Check source blocked tags
            if (sourceBlockedTags.Count > 0)
            {
                if (blockIfAnyPresent)
                {
                    foreach (var tag in sourceBlockedTags)
                    {
                        if (sourceTagComponent.HasTag(tag))
                            return false;
                    }
                }
                else
                {
                    bool hasAll = true;
                    foreach (var tag in sourceBlockedTags)
                    {
                        if (!sourceTagComponent.HasTag(tag))
                        {
                            hasAll = false;
                            break;
                        }
                    }
                    if (hasAll) return false;
                }
            }

            // Check general required tags
            if (requiredTags.Count > 0)
            {
                if (requireAllRequired)
                {
                    foreach (var tag in requiredTags)
                    {
                        if (!sourceTagComponent.HasTag(tag))
                            return false;
                    }
                }
                else
                {
                    bool hasAny = false;
                    foreach (var tag in requiredTags)
                    {
                        if (sourceTagComponent.HasTag(tag))
                        {
                            hasAny = true;
                            break;
                        }
                    }
                    if (!hasAny) return false;
                }
            }

            // Check general blocked tags
            if (blockedTags.Count > 0)
            {
                if (blockIfAnyPresent)
                {
                    foreach (var tag in blockedTags)
                    {
                        if (sourceTagComponent.HasTag(tag))
                            return false;
                    }
                }
            }

            // Check target tags if target exists
            if (targetTagComponent != null)
            {
                if (targetRequiredTags.Count > 0)
                {
                    foreach (var tag in targetRequiredTags)
                    {
                        if (!targetTagComponent.HasTag(tag))
                            return false;
                    }
                }

                if (targetBlockedTags.Count > 0)
                {
                    foreach (var tag in targetBlockedTags)
                    {
                        if (targetTagComponent.HasTag(tag))
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets all required tags
        /// </summary>
        public List<GameplayTag> GetAllRequiredTags()
        {
            var allTags = new List<GameplayTag>();
            allTags.AddRange(requiredTags);
            allTags.AddRange(sourceRequiredTags);
            return allTags;
        }

        /// <summary>
        /// Gets all blocked tags
        /// </summary>
        public List<GameplayTag> GetAllBlockedTags()
        {
            var allTags = new List<GameplayTag>();
            allTags.AddRange(blockedTags);
            allTags.AddRange(sourceBlockedTags);
            return allTags;
        }
    }
}