// ================================
// File: Assets/Scripts/GAS/TagSystem/TagContainer.cs
// 일반 클래스 버전 - ScriptableObject 제거
// ================================
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GAS.TagSystem
{
    /// <summary>
    /// Container for managing collections of gameplay tags
    /// 일반 클래스로 변경하여 MonoBehaviour에서 자유롭게 생성 가능
    /// </summary>
    [Serializable]
    public class TagContainer
    {
        [Header("Tags")]
        [SerializeField] private List<GameplayTag> tags = new List<GameplayTag>();

        /// <summary>
        /// Gets or sets the tags list
        /// </summary>
        public List<GameplayTag> Tags
        {
            get => tags;
            set => tags = value ?? new List<GameplayTag>();
        }

        /// <summary>
        /// Gets the number of tags in the container
        /// </summary>
        public int Count => tags?.Count ?? 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TagContainer()
        {
            tags = new List<GameplayTag>();
        }

        /// <summary>
        /// Constructor with initial tags
        /// </summary>
        public TagContainer(List<GameplayTag> initialTags)
        {
            tags = initialTags != null ? new List<GameplayTag>(initialTags) : new List<GameplayTag>();
        }

        /// <summary>
        /// Creates a deep copy of this TagContainer
        /// </summary>
        public TagContainer Clone()
        {
            var clone = new TagContainer();

            // Deep copy the tags list
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    if (tag != null)
                    {
                        // GameplayTag is a ScriptableObject, so we reference the same instances
                        // (tags are meant to be shared references, not duplicated)
                        clone.tags.Add(tag);
                    }
                }
            }

            return clone;
        }

        /// <summary>
        /// Adds a tag to the container
        /// </summary>
        public void AddTag(GameplayTag tag)
        {
            if (tag == null) return;

            if (!HasTag(tag))
            {
                tags.Add(tag);
            }
        }

        /// <summary>
        /// Adds multiple tags to the container
        /// </summary>
        public void AddTags(IEnumerable<GameplayTag> tagsToAdd)
        {
            if (tagsToAdd == null) return;

            foreach (var tag in tagsToAdd)
            {
                AddTag(tag);
            }
        }

        /// <summary>
        /// Removes a tag from the container
        /// </summary>
        public void RemoveTag(GameplayTag tag)
        {
            if (tag == null) return;
            tags.Remove(tag);
        }

        /// <summary>
        /// Removes multiple tags from the container
        /// </summary>
        public void RemoveTags(IEnumerable<GameplayTag> tagsToRemove)
        {
            if (tagsToRemove == null) return;

            foreach (var tag in tagsToRemove)
            {
                RemoveTag(tag);
            }
        }

        /// <summary>
        /// Clears all tags from the container
        /// </summary>
        public void Clear()
        {
            tags.Clear();
        }

        /// <summary>
        /// Checks if the container has a specific tag
        /// </summary>
        public bool HasTag(GameplayTag tag)
        {
            if (tag == null || tags == null)
                return false;

            return tags.Contains(tag);
        }

        /// <summary>
        /// Checks if the container has a tag with the specified name
        /// </summary>
        public bool HasTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName) || tags == null)
                return false;

            return tags.Any(t => t != null && t.TagName == tagName);
        }

        /// <summary>
        /// Checks if the container has all specified tags
        /// </summary>
        public bool HasAllTags(IEnumerable<GameplayTag> tagsToCheck)
        {
            if (tagsToCheck == null)
                return false;

            foreach (var tag in tagsToCheck)
            {
                if (!HasTag(tag))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the container has any of the specified tags
        /// </summary>
        public bool HasAnyTag(IEnumerable<GameplayTag> tagsToCheck)
        {
            if (tagsToCheck == null)
                return false;

            foreach (var tag in tagsToCheck)
            {
                if (HasTag(tag))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all tags as a list
        /// </summary>
        public List<GameplayTag> ToList()
        {
            return tags?.ToList() ?? new List<GameplayTag>();
        }

        /// <summary>
        /// Gets a tag by name
        /// </summary>
        public GameplayTag GetTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName) || tags == null)
                return null;

            return tags.FirstOrDefault(t => t != null && t.TagName == tagName);
        }

        /// <summary>
        /// Combines this container with another
        /// </summary>
        public TagContainer CombineWith(TagContainer other)
        {
            if (other == null) return Clone();

            var combined = Clone();
            combined.AddTags(other.tags);
            return combined;
        }

        /// <summary>
        /// Creates a new container with tags that exist in both containers
        /// </summary>
        public TagContainer IntersectWith(TagContainer other)
        {
            var intersection = new TagContainer();

            if (other == null || tags == null)
                return intersection;

            intersection.tags = tags.Where(t => other.HasTag(t)).ToList();
            return intersection;
        }

        /// <summary>
        /// Creates a new container with tags from this container that don't exist in the other
        /// </summary>
        public TagContainer ExceptWith(TagContainer other)
        {
            var difference = new TagContainer();

            if (tags == null)
                return difference;

            if (other == null)
                return Clone();

            difference.tags = tags.Where(t => !other.HasTag(t)).ToList();
            return difference;
        }

        /// <summary>
        /// Checks if this container is a subset of another
        /// </summary>
        public bool IsSubsetOf(TagContainer other)
        {
            if (other == null) return false;
            if (tags == null || Count == 0) return true;

            return tags.All(t => other.HasTag(t));
        }

        /// <summary>
        /// Checks if this container is a superset of another
        /// </summary>
        public bool IsSupersetOf(TagContainer other)
        {
            if (other == null) return true;
            return other.IsSubsetOf(this);
        }

        /// <summary>
        /// Converts the container to a formatted string
        /// </summary>
        public override string ToString()
        {
            if (tags == null || Count == 0)
                return "Empty TagContainer";

            return $"TagContainer({Count} tags): {string.Join(", ", tags.Where(t => t != null).Select(t => t.TagName))}";
        }

        /// <summary>
        /// Validates the container
        /// </summary>
        public void Validate()
        {
            // Remove null entries
            tags?.RemoveAll(t => t == null);

            // Remove duplicates
            if (tags != null && tags.Count > 0)
            {
                tags = tags.Distinct().ToList();
            }
        }
    }
}