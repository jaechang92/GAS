// ================================
// File: Assets/Scripts/GAS/TagSystem/TagComponent.cs
// ================================
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GAS.Core;

namespace GAS.TagSystem
{
    /// <summary>
    /// Component that manages gameplay tags on a GameObject
    /// </summary>
    public class TagComponent : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private List<GameplayTag> initialTags = new List<GameplayTag>();
        [SerializeField] private bool debugMode = false;

        [Header("Tag Limits")]
        [SerializeField] private int maxTotalTags = 100;
        [SerializeField] private int maxTagStacks = 99;

        [Header("Runtime Tags - Read Only")]
        [SerializeField] private TagContainer runtimeTags = new TagContainer();
        [SerializeField] private Dictionary<string, int> tagCounts = new Dictionary<string, int>();

        // Tag tracking
        private Dictionary<GameplayTag, List<object>> tagSources = new Dictionary<GameplayTag, List<object>>();
        private Dictionary<string, GameplayTag> tagLookup = new Dictionary<string, GameplayTag>();
        private HashSet<string> blockedTags = new HashSet<string>();

        // Events
        public event Action<GameplayTag> TagAdded;
        public event Action<GameplayTag> TagRemoved;
        public event Action<GameplayTag, int, int> TagCountChanged;
        public event Action<List<GameplayTag>> TagsChanged;

        // Properties
        public TagContainer Tags => runtimeTags;
        public int TotalTagCount => runtimeTags.Tags.Count;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeTags();
        }

        private void Start()
        {
            ApplyInitialTags();
            RegisterEvents();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
            CleanupTags();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the tag system
        /// </summary>
        private void InitializeTags()
        {
            runtimeTags = new TagContainer();
            tagCounts = new Dictionary<string, int>();
            tagSources = new Dictionary<GameplayTag, List<object>>();
            tagLookup = new Dictionary<string, GameplayTag>();
            blockedTags = new HashSet<string>();

            GASEvents.Trigger(GASEventType.ComponentAdded,
                new SimpleGASEventData(gameObject, this));
        }

        /// <summary>
        /// Apply initial tags
        /// </summary>
        private void ApplyInitialTags()
        {
            foreach (var tag in initialTags)
            {
                if (tag != null)
                {
                    AddTag(tag, this);
                }
            }
        }

        /// <summary>
        /// Register for events
        /// </summary>
        private void RegisterEvents()
        {
            // Can subscribe to other system events if needed
        }

        /// <summary>
        /// Unregister from events
        /// </summary>
        private void UnregisterEvents()
        {
            // Unsubscribe from events
        }

        /// <summary>
        /// Cleanup on destroy
        /// </summary>
        private void CleanupTags()
        {
            RemoveAllTags();

            GASEvents.Trigger(GASEventType.ComponentRemoved,
                new SimpleGASEventData(gameObject, this));
        }

        #endregion

        #region Tag Management

        /// <summary>
        /// Add a tag to this component
        /// </summary>
        public bool AddTag(GameplayTag tag, object source = null)
        {
            if (!CanAddTag(tag))
                return false;

            // Check if tag is blocked
            if (IsTagBlocked(tag))
            {
                if (debugMode)
                    Debug.Log($"[TagComponent] Tag {tag.TagName} is blocked on {gameObject.name}");
                return false;
            }

            // Check tag limit
            if (runtimeTags.Tags.Count >= maxTotalTags)
            {
                if (debugMode)
                    Debug.LogWarning($"[TagComponent] Max tag limit reached ({maxTotalTags})");
                return false;
            }

            // Track source
            if (!tagSources.ContainsKey(tag))
            {
                tagSources[tag] = new List<object>();
            }
            tagSources[tag].Add(source ?? this);

            // Update tag count
            int previousCount = GetTagCount(tag.TagName);

            if (!tagCounts.ContainsKey(tag.TagName))
            {
                tagCounts[tag.TagName] = 0;
            }

            tagCounts[tag.TagName]++;
            int newCount = tagCounts[tag.TagName];

            // Check max stacks
            if (newCount > maxTagStacks)
            {
                tagCounts[tag.TagName] = maxTagStacks;
                newCount = maxTagStacks;

                if (debugMode)
                    Debug.LogWarning($"[TagComponent] Tag {tag.TagName} reached max stacks ({maxTagStacks})");
            }

            // Add to container if first instance
            if (previousCount == 0)
            {
                runtimeTags.AddTag(tag);
                tagLookup[tag.TagName] = tag;

                // Fire events
                TagAdded?.Invoke(tag);
                FireTagAddedEvent(tag);

                if (debugMode)
                    Debug.Log($"[TagComponent] Added tag {tag.TagName} to {gameObject.name}");
            }
            else if (newCount != previousCount)
            {
                // Fire count changed event
                TagCountChanged?.Invoke(tag, newCount, previousCount);
                FireTagCountChangedEvent(tag, newCount, previousCount);

                if (debugMode)
                    Debug.Log($"[TagComponent] Tag {tag.TagName} count: {previousCount} -> {newCount}");
            }

            // Fire general tags changed event
            TagsChanged?.Invoke(runtimeTags.Tags.ToList());

            return true;
        }

        /// <summary>
        /// Add multiple tags
        /// </summary>
        public void AddTags(List<GameplayTag> tags, object source = null)
        {
            foreach (var tag in tags)
            {
                if (tag != null)
                {
                    AddTag(tag, source);
                }
            }
        }

        /// <summary>
        /// Remove a tag from this component
        /// </summary>
        public bool RemoveTag(GameplayTag tag, object source = null)
        {
            if (tag == null || !HasTag(tag))
                return false;

            // Remove from source tracking
            if (tagSources.ContainsKey(tag))
            {
                if (source != null)
                {
                    tagSources[tag].Remove(source);
                }
                else
                {
                    // Remove one instance if no source specified
                    if (tagSources[tag].Count > 0)
                    {
                        tagSources[tag].RemoveAt(0);
                    }
                }

                // If no more sources, clean up
                if (tagSources[tag].Count == 0)
                {
                    tagSources.Remove(tag);
                }
            }

            // Update tag count
            int previousCount = GetTagCount(tag.TagName);

            if (tagCounts.ContainsKey(tag.TagName))
            {
                tagCounts[tag.TagName]--;
                int newCount = tagCounts[tag.TagName];

                if (newCount <= 0)
                {
                    // Remove from container
                    runtimeTags.RemoveTag(tag);
                    tagCounts.Remove(tag.TagName);
                    tagLookup.Remove(tag.TagName);

                    // Fire events
                    TagRemoved?.Invoke(tag);
                    FireTagRemovedEvent(tag);

                    if (debugMode)
                        Debug.Log($"[TagComponent] Removed tag {tag.TagName} from {gameObject.name}");
                }
                else
                {
                    // Fire count changed event
                    TagCountChanged?.Invoke(tag, newCount, previousCount);
                    FireTagCountChangedEvent(tag, newCount, previousCount);

                    if (debugMode)
                        Debug.Log($"[TagComponent] Tag {tag.TagName} count: {previousCount} -> {newCount}");
                }
            }

            // Fire general tags changed event
            TagsChanged?.Invoke(runtimeTags.Tags.ToList());

            return true;
        }

        /// <summary>
        /// Remove multiple tags
        /// </summary>
        public void RemoveTags(List<GameplayTag> tags, object source = null)
        {
            foreach (var tag in tags)
            {
                if (tag != null)
                {
                    RemoveTag(tag, source);
                }
            }
        }

        /// <summary>
        /// Remove all tags from a specific source
        /// </summary>
        public void RemoveAllTagsFromSource(object source)
        {
            if (source == null) return;

            var tagsToRemove = new List<GameplayTag>();

            foreach (var kvp in tagSources)
            {
                if (kvp.Value.Contains(source))
                {
                    // Count how many times this source added the tag
                    int count = kvp.Value.Count(s => s == source);

                    // Remove that many instances
                    for (int i = 0; i < count; i++)
                    {
                        tagsToRemove.Add(kvp.Key);
                    }
                }
            }

            foreach (var tag in tagsToRemove)
            {
                RemoveTag(tag, source);
            }
        }

        /// <summary>
        /// Remove all tags
        /// </summary>
        public void RemoveAllTags()
        {
            var tagsToRemove = new List<GameplayTag>(runtimeTags.Tags);

            foreach (var tag in tagsToRemove)
            {
                // Remove all instances
                while (HasTag(tag))
                {
                    RemoveTag(tag);
                }
            }

            // Clear everything
            runtimeTags.Clear();
            tagCounts.Clear();
            tagSources.Clear();
            tagLookup.Clear();
        }

        /// <summary>
        /// Remove tags by category
        /// </summary>
        public void RemoveTagsByCategory(string category)
        {
            var tagsToRemove = runtimeTags.Tags
                .Where(t => t.TagName.StartsWith(category))
                .ToList();

            foreach (var tag in tagsToRemove)
            {
                while (HasTag(tag))
                {
                    RemoveTag(tag);
                }
            }
        }

        /// <summary>
        /// Set tag count to a specific value
        /// </summary>
        public void SetTagCount(GameplayTag tag, int count)
        {
            if (tag == null) return;

            int currentCount = GetTagCount(tag.TagName);
            int difference = count - currentCount;

            if (difference > 0)
            {
                // Add tags
                for (int i = 0; i < difference; i++)
                {
                    AddTag(tag);
                }
            }
            else if (difference < 0)
            {
                // Remove tags
                for (int i = 0; i < Math.Abs(difference); i++)
                {
                    RemoveTag(tag);
                }
            }
        }

        #endregion

        #region Tag Queries

        /// <summary>
        /// Check if has a specific tag
        /// </summary>
        public bool HasTag(GameplayTag tag)
        {
            return tag != null && runtimeTags.HasTag(tag);
        }

        /// <summary>
        /// Check if has a tag by name
        /// </summary>
        public bool HasTag(string tagName)
        {
            return !string.IsNullOrEmpty(tagName) && tagCounts.ContainsKey(tagName) && tagCounts[tagName] > 0;
        }

        /// <summary>
        /// Check if has all specified tags
        /// </summary>
        public bool HasAllTags(List<GameplayTag> tags)
        {
            if (tags == null || tags.Count == 0)
                return true;

            foreach (var tag in tags)
            {
                if (!HasTag(tag))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if has any of the specified tags
        /// </summary>
        public bool HasAnyTag(List<GameplayTag> tags)
        {
            if (tags == null || tags.Count == 0)
                return false;

            foreach (var tag in tags)
            {
                if (HasTag(tag))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check if has exact tags (no more, no less)
        /// </summary>
        public bool HasExactTags(List<GameplayTag> tags)
        {
            if (tags == null)
                return runtimeTags.Tags.Count == 0;

            if (tags.Count != runtimeTags.Tags.Count)
                return false;

            foreach (var tag in tags)
            {
                if (!HasTag(tag))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get tag count
        /// </summary>
        public int GetTagCount(string tagName)
        {
            if (tagCounts.TryGetValue(tagName, out int count))
            {
                return count;
            }
            return 0;
        }

        /// <summary>
        /// Get tag count
        /// </summary>
        public int GetTagCount(GameplayTag tag)
        {
            if (tag == null) return 0;
            return GetTagCount(tag.TagName);
        }

        /// <summary>
        /// Get all tags
        /// </summary>
        public List<GameplayTag> GetAllTags()
        {
            return new List<GameplayTag>(runtimeTags.Tags);
        }

        /// <summary>
        /// Get tags by category
        /// </summary>
        public List<GameplayTag> GetTagsByCategory(string category)
        {
            return runtimeTags.Tags
                .Where(t => t.TagName.StartsWith(category))
                .ToList();
        }

        /// <summary>
        /// Get tag by name
        /// </summary>
        public GameplayTag GetTag(string tagName)
        {
            tagLookup.TryGetValue(tagName, out var tag);
            return tag;
        }

        /// <summary>
        /// Get all tag sources for a tag
        /// </summary>
        public List<object> GetTagSources(GameplayTag tag)
        {
            if (tag != null && tagSources.TryGetValue(tag, out var sources))
            {
                return new List<object>(sources);
            }
            return new List<object>();
        }

        #endregion

        #region Tag Blocking

        /// <summary>
        /// Block a tag from being added
        /// </summary>
        public void BlockTag(string tagName)
        {
            if (!string.IsNullOrEmpty(tagName))
            {
                blockedTags.Add(tagName);

                // Remove if currently has the tag
                var tag = GetTag(tagName);
                if (tag != null)
                {
                    while (HasTag(tag))
                    {
                        RemoveTag(tag);
                    }
                }
            }
        }

        /// <summary>
        /// Unblock a tag
        /// </summary>
        public void UnblockTag(string tagName)
        {
            blockedTags.Remove(tagName);
        }

        /// <summary>
        /// Check if tag is blocked
        /// </summary>
        public bool IsTagBlocked(GameplayTag tag)
        {
            return tag != null && blockedTags.Contains(tag.TagName);
        }

        /// <summary>
        /// Check if tag is blocked by name
        /// </summary>
        public bool IsTagBlocked(string tagName)
        {
            return blockedTags.Contains(tagName);
        }

        /// <summary>
        /// Clear all blocked tags
        /// </summary>
        public void ClearBlockedTags()
        {
            blockedTags.Clear();
        }

        #endregion

        #region Tag Requirements

        /// <summary>
        /// Check if meets tag requirements
        /// </summary>
        public bool MeetsRequirements(TagRequirement requirement)
        {
            if (requirement == null || requirement.IgnoreIfEmpty)
                return true;

            return requirement.CheckRequirement(this);
        }

        /// <summary>
        /// Check if meets multiple requirements
        /// </summary>
        public bool MeetsAllRequirements(List<TagRequirement> requirements)
        {
            if (requirements == null || requirements.Count == 0)
                return true;

            foreach (var requirement in requirements)
            {
                if (!MeetsRequirements(requirement))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if meets any requirement
        /// </summary>
        public bool MeetsAnyRequirement(List<TagRequirement> requirements)
        {
            if (requirements == null || requirements.Count == 0)
                return false;

            foreach (var requirement in requirements)
            {
                if (MeetsRequirements(requirement))
                    return true;
            }

            return false;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Check if can add tag
        /// </summary>
        private bool CanAddTag(GameplayTag tag)
        {
            if (tag == null)
            {
                if (debugMode)
                    Debug.LogWarning("[TagComponent] Attempted to add null tag");
                return false;
            }

            if (!tag.IsValid)
            {
                if (debugMode)
                    Debug.LogWarning($"[TagComponent] Tag {tag.TagName} is invalid");
                return false;
            }

            return true;
        }

        #endregion

        #region Events

        /// <summary>
        /// Fire tag added event
        /// </summary>
        private void FireTagAddedEvent(GameplayTag tag)
        {
            GASEvents.Trigger(GASEventType.TagAdded, new TagEventData
            {
                tag = tag,
                count = GetTagCount(tag),
                source = gameObject
            });
        }

        /// <summary>
        /// Fire tag removed event
        /// </summary>
        private void FireTagRemovedEvent(GameplayTag tag)
        {
            GASEvents.Trigger(GASEventType.TagRemoved, new TagEventData
            {
                tag = tag,
                count = 0,
                source = gameObject
            });
        }

        /// <summary>
        /// Fire tag count changed event
        /// </summary>
        private void FireTagCountChangedEvent(GameplayTag tag, int newCount, int previousCount)
        {
            GASEvents.Trigger(GASEventType.TagCountChanged, new TagEventData
            {
                tag = tag,
                count = newCount,
                previousCount = previousCount,
                source = gameObject
            });
        }

        #endregion

        #region Debug

        /// <summary>
        /// Get debug info
        /// </summary>
        public string GetDebugInfo()
        {
            var info = $"TagComponent on {gameObject.name}\n";
            info += $"Total Tags: {TotalTagCount}\n";
            info += $"Tags:\n";

            foreach (var tag in runtimeTags.Tags)
            {
                int count = GetTagCount(tag);
                info += $"  - {tag.TagName} x{count}\n";
            }

            if (blockedTags.Count > 0)
            {
                info += $"Blocked Tags: {string.Join(", ", blockedTags)}\n";
            }

            return info;
        }

        private void OnGUI()
        {
            if (!debugMode) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
            if (screenPos.z < 0) return;

            float yOffset = 0;
            GUI.Label(new Rect(screenPos.x - 100, Screen.height - screenPos.y - yOffset, 200, 20),
                $"Tags: {TotalTagCount}");

            yOffset += 20;
            foreach (var tag in runtimeTags.Tags.Take(5)) // Show first 5 tags
            {
                int count = GetTagCount(tag);
                string tagInfo = count > 1 ? $"{tag.TagName} x{count}" : tag.TagName;
                GUI.Label(new Rect(screenPos.x - 100, Screen.height - screenPos.y - yOffset, 200, 20), tagInfo);
                yOffset += 20;
            }

            if (runtimeTags.Tags.Count > 5)
            {
                GUI.Label(new Rect(screenPos.x - 100, Screen.height - screenPos.y - yOffset, 200, 20),
                    $"... and {runtimeTags.Tags.Count - 5} more");
            }
        }

        #endregion
    }
}