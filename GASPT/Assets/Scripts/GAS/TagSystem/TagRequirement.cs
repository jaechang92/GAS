// ================================
// File: Assets/Scripts/GAS/TagSystem/TagRequirement.cs
// ================================
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GAS.TagSystem
{
    /// <summary>
    /// Defines tag requirements for various game systems
    /// </summary>
    [Serializable]
    public class TagRequirement
    {
        [Header("Basic Settings")]
        [SerializeField] public string requirementName = "New Requirement";
        [SerializeField] public RequirementType type = RequirementType.RequireAll;

        [Header("Required Tags")]
        [Tooltip("Tags that must be present")]
        [SerializeField] public List<GameplayTag> requiredTags = new List<GameplayTag>();

        [Header("Blocked Tags")]
        [Tooltip("Tags that must NOT be present")]
        [SerializeField] public List<GameplayTag> blockedTags = new List<GameplayTag>();

        [Header("Advanced Requirements")]
        [SerializeField] public List<AdvancedRequirement> advancedRequirements = new List<AdvancedRequirement>();

        [Header("Options")]
        [Tooltip("If true, requires exact tag match (no parent/child tags)")]
        [SerializeField] public bool matchExact = false;

        [Tooltip("If true, empty requirements always pass")]
        [SerializeField] public bool ignoreIfEmpty = true;

        /// <summary>
        /// Gets whether to ignore if empty
        /// </summary>
        public bool IgnoreIfEmpty => ignoreIfEmpty;

        /// <summary>
        /// Check if requirements are met
        /// </summary>
        public bool CheckRequirement(TagComponent tagComponent)
        {
            if (tagComponent == null)
                return false;

            // If ignoreIfEmpty is true and no requirements, pass
            if (ignoreIfEmpty && IsEmpty())
                return true;

            // Check blocked tags first (if any blocked tag is present, fail)
            if (blockedTags != null && blockedTags.Count > 0)
            {
                foreach (var blockedTag in blockedTags)
                {
                    if (blockedTag != null && tagComponent.HasTag(blockedTag))
                    {
                        return false;
                    }
                }
            }

            // Check required tags based on type
            bool requirementsMet = CheckRequiredTags(tagComponent);

            // Check advanced requirements if any
            if (advancedRequirements != null && advancedRequirements.Count > 0)
            {
                requirementsMet = requirementsMet && CheckAdvancedRequirements(tagComponent);
            }

            return requirementsMet;
        }

        /// <summary>
        /// Check required tags based on requirement type
        /// </summary>
        private bool CheckRequiredTags(TagComponent tagComponent)
        {
            if (requiredTags == null || requiredTags.Count == 0)
                return true;

            switch (type)
            {
                case RequirementType.RequireAll:
                    return CheckRequireAll(tagComponent);

                case RequirementType.RequireAny:
                    return CheckRequireAny(tagComponent);

                case RequirementType.RequireNone:
                    return CheckRequireNone(tagComponent);

                case RequirementType.RequireExact:
                    return CheckRequireExact(tagComponent);

                default:
                    return true;
            }
        }

        /// <summary>
        /// Check if all required tags are present
        /// </summary>
        private bool CheckRequireAll(TagComponent tagComponent)
        {
            foreach (var tag in requiredTags)
            {
                if (tag != null && !HasTagWithMatch(tagComponent, tag))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if any required tag is present
        /// </summary>
        private bool CheckRequireAny(TagComponent tagComponent)
        {
            if (requiredTags.Count == 0)
                return true;

            foreach (var tag in requiredTags)
            {
                if (tag != null && HasTagWithMatch(tagComponent, tag))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if none of the required tags are present
        /// </summary>
        private bool CheckRequireNone(TagComponent tagComponent)
        {
            foreach (var tag in requiredTags)
            {
                if (tag != null && HasTagWithMatch(tagComponent, tag))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if tags match exactly
        /// </summary>
        private bool CheckRequireExact(TagComponent tagComponent)
        {
            var componentTags = tagComponent.GetAllTags();

            // Must have exact same count
            if (componentTags.Count != requiredTags.Count)
                return false;

            // All required tags must be present
            foreach (var tag in requiredTags)
            {
                if (tag != null && !componentTags.Any(t => t.TagName == tag.TagName))
                {
                    return false;
                }
            }

            // No extra tags allowed
            foreach (var tag in componentTags)
            {
                if (!requiredTags.Any(t => t != null && t.TagName == tag.TagName))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if tag matches based on match settings
        /// </summary>
        private bool HasTagWithMatch(TagComponent tagComponent, GameplayTag tag)
        {
            if (matchExact)
            {
                // Exact match - must be the exact tag, not parent/child
                return tagComponent.HasTag(tag);
            }
            else
            {
                // Partial match - can match parent/child tags
                var componentTags = tagComponent.GetAllTags();
                return componentTags.Any(t =>
                    t.TagName == tag.TagName ||
                    t.TagName.StartsWith(tag.TagName + ".") ||
                    tag.TagName.StartsWith(t.TagName + ".")
                );
            }
        }

        /// <summary>
        /// Check advanced requirements
        /// </summary>
        private bool CheckAdvancedRequirements(TagComponent tagComponent)
        {
            foreach (var requirement in advancedRequirements)
            {
                if (requirement != null && !requirement.CheckRequirement(tagComponent))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if this requirement is empty
        /// </summary>
        public bool IsEmpty()
        {
            bool hasNoRequired = requiredTags == null || requiredTags.Count == 0 || requiredTags.All(t => t == null);
            bool hasNoBlocked = blockedTags == null || blockedTags.Count == 0 || blockedTags.All(t => t == null);
            bool hasNoAdvanced = advancedRequirements == null || advancedRequirements.Count == 0;

            return hasNoRequired && hasNoBlocked && hasNoAdvanced;
        }

        /// <summary>
        /// Get all tags involved in this requirement
        /// </summary>
        public List<GameplayTag> GetAllTags()
        {
            var allTags = new List<GameplayTag>();

            if (requiredTags != null)
                allTags.AddRange(requiredTags.Where(t => t != null));

            if (blockedTags != null)
                allTags.AddRange(blockedTags.Where(t => t != null));

            if (advancedRequirements != null)
            {
                foreach (var req in advancedRequirements)
                {
                    if (req != null)
                        allTags.AddRange(req.GetAllTags());
                }
            }

            return allTags.Distinct().ToList();
        }

        /// <summary>
        /// Clone this requirement
        /// </summary>
        public TagRequirement Clone()
        {
            var clone = new TagRequirement
            {
                requirementName = requirementName,
                type = type,
                requiredTags = new List<GameplayTag>(requiredTags),
                blockedTags = new List<GameplayTag>(blockedTags),
                matchExact = matchExact,
                ignoreIfEmpty = ignoreIfEmpty
            };

            if (advancedRequirements != null)
            {
                clone.advancedRequirements = new List<AdvancedRequirement>();
                foreach (var req in advancedRequirements)
                {
                    if (req != null)
                        clone.advancedRequirements.Add(req.Clone());
                }
            }

            return clone;
        }

        /// <summary>
        /// Get a description of this requirement
        /// </summary>
        public string GetDescription()
        {
            if (IsEmpty())
                return "No requirements";

            var description = $"{requirementName}: ";

            switch (type)
            {
                case RequirementType.RequireAll:
                    description += "Requires ALL tags";
                    break;
                case RequirementType.RequireAny:
                    description += "Requires ANY tag";
                    break;
                case RequirementType.RequireNone:
                    description += "Requires NONE of tags";
                    break;
                case RequirementType.RequireExact:
                    description += "Requires EXACT tags";
                    break;
            }

            if (requiredTags?.Count > 0)
            {
                description += "\nRequired: " + string.Join(", ", requiredTags.Where(t => t != null).Select(t => t.TagName));
            }

            if (blockedTags?.Count > 0)
            {
                description += "\nBlocked: " + string.Join(", ", blockedTags.Where(t => t != null).Select(t => t.TagName));
            }

            return description;
        }
    }

    /// <summary>
    /// Type of tag requirement
    /// </summary>
    public enum RequirementType
    {
        /// <summary>
        /// All required tags must be present
        /// </summary>
        RequireAll,

        /// <summary>
        /// At least one required tag must be present
        /// </summary>
        RequireAny,

        /// <summary>
        /// None of the required tags can be present
        /// </summary>
        RequireNone,

        /// <summary>
        /// Tags must match exactly (no more, no less)
        /// </summary>
        RequireExact
    }

    /// <summary>
    /// Advanced requirement for complex tag logic
    /// </summary>
    [Serializable]
    public class AdvancedRequirement
    {
        [Header("Requirement Settings")]
        [SerializeField] public string name = "Advanced Requirement";
        [SerializeField] public AdvancedRequirementType type = AdvancedRequirementType.TagCount;

        [Header("Tag Count Requirements")]
        [Tooltip("For TagCount type")]
        [SerializeField] public GameplayTag targetTag;
        [SerializeField] public ComparisonOperator comparison = ComparisonOperator.GreaterOrEqual;
        [SerializeField] public int count = 1;

        [Header("Category Requirements")]
        [Tooltip("For CategoryCount type")]
        [SerializeField] public string categoryName;
        [SerializeField] public int minCategoryCount = 1;

        [Header("Complex Requirements")]
        [Tooltip("For nested requirements")]
        [SerializeField] public List<TagRequirement> nestedRequirements = new List<TagRequirement>();
        [SerializeField] public LogicalOperator logicalOperator = LogicalOperator.And;

        /// <summary>
        /// Check if this advanced requirement is met
        /// </summary>
        public bool CheckRequirement(TagComponent tagComponent)
        {
            if (tagComponent == null)
                return false;

            switch (type)
            {
                case AdvancedRequirementType.TagCount:
                    return CheckTagCount(tagComponent);

                case AdvancedRequirementType.CategoryCount:
                    return CheckCategoryCount(tagComponent);

                case AdvancedRequirementType.Nested:
                    return CheckNestedRequirements(tagComponent);

                default:
                    return true;
            }
        }

        /// <summary>
        /// Check tag count requirement
        /// </summary>
        private bool CheckTagCount(TagComponent tagComponent)
        {
            if (targetTag == null)
                return false;

            int tagCount = tagComponent.GetTagCount(targetTag);

            switch (comparison)
            {
                case ComparisonOperator.Equal:
                    return tagCount == count;
                case ComparisonOperator.NotEqual:
                    return tagCount != count;
                case ComparisonOperator.Greater:
                    return tagCount > count;
                case ComparisonOperator.GreaterOrEqual:
                    return tagCount >= count;
                case ComparisonOperator.Less:
                    return tagCount < count;
                case ComparisonOperator.LessOrEqual:
                    return tagCount <= count;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Check category count requirement
        /// </summary>
        private bool CheckCategoryCount(TagComponent tagComponent)
        {
            if (string.IsNullOrEmpty(categoryName))
                return false;

            var categoryTags = tagComponent.GetTagsByCategory(categoryName);
            return categoryTags.Count >= minCategoryCount;
        }

        /// <summary>
        /// Check nested requirements
        /// </summary>
        private bool CheckNestedRequirements(TagComponent tagComponent)
        {
            if (nestedRequirements == null || nestedRequirements.Count == 0)
                return true;

            if (logicalOperator == LogicalOperator.And)
            {
                // All nested requirements must pass
                foreach (var req in nestedRequirements)
                {
                    if (req != null && !req.CheckRequirement(tagComponent))
                        return false;
                }
                return true;
            }
            else // LogicalOperator.Or
            {
                // At least one nested requirement must pass
                foreach (var req in nestedRequirements)
                {
                    if (req != null && req.CheckRequirement(tagComponent))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Get all tags from this requirement
        /// </summary>
        public List<GameplayTag> GetAllTags()
        {
            var tags = new List<GameplayTag>();

            if (targetTag != null)
                tags.Add(targetTag);

            if (nestedRequirements != null)
            {
                foreach (var req in nestedRequirements)
                {
                    if (req != null)
                        tags.AddRange(req.GetAllTags());
                }
            }

            return tags;
        }

        /// <summary>
        /// Clone this advanced requirement
        /// </summary>
        public AdvancedRequirement Clone()
        {
            var clone = new AdvancedRequirement
            {
                name = name,
                type = type,
                targetTag = targetTag,
                comparison = comparison,
                count = count,
                categoryName = categoryName,
                minCategoryCount = minCategoryCount,
                logicalOperator = logicalOperator
            };

            if (nestedRequirements != null)
            {
                clone.nestedRequirements = new List<TagRequirement>();
                foreach (var req in nestedRequirements)
                {
                    if (req != null)
                        clone.nestedRequirements.Add(req.Clone());
                }
            }

            return clone;
        }
    }

    /// <summary>
    /// Type of advanced requirement
    /// </summary>
    public enum AdvancedRequirementType
    {
        /// <summary>
        /// Check tag stack count
        /// </summary>
        TagCount,

        /// <summary>
        /// Check number of tags in a category
        /// </summary>
        CategoryCount,

        /// <summary>
        /// Check nested requirements
        /// </summary>
        Nested
    }

    /// <summary>
    /// Comparison operators
    /// </summary>
    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual
    }

    /// <summary>
    /// Logical operators for combining requirements
    /// </summary>
    public enum LogicalOperator
    {
        And,
        Or
    }
}