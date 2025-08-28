// ���� ��ġ: Assets/Scripts/GAS/TagSystem/TagContainer.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GAS.TagSystem
{
    /// <summary>
    /// ���� GameplayTag�� �����ϴ� �����̳�
    /// </summary>
    [Serializable]
    public class TagContainer : IEnumerable<GameplayTag>
    {
        #region Serialized Fields
        [SerializeField] private List<GameplayTag> tags = new List<GameplayTag>();
        #endregion

        #region Events
        /// <summary>
        /// �±װ� �߰��Ǿ��� ��
        /// </summary>
        public event Action<GameplayTag> OnTagAdded;

        /// <summary>
        /// �±װ� ���ŵǾ��� ��
        /// </summary>
        public event Action<GameplayTag> OnTagRemoved;

        /// <summary>
        /// �����̳ʰ� ����Ǿ��� ��
        /// </summary>
        public event Action OnContainerChanged;
        #endregion

        #region Properties
        /// <summary>
        /// �±� ����
        /// </summary>
        public int Count => tags.Count;

        /// <summary>
        /// �����̳ʰ� ����ִ��� Ȯ��
        /// </summary>
        public bool IsEmpty => tags.Count == 0;

        /// <summary>
        /// ��� �±� ����Ʈ (�б� ����)
        /// </summary>
        public IReadOnlyList<GameplayTag> Tags => tags.AsReadOnly();
        #endregion

        #region Constructors
        /// <summary>
        /// �� �����̳� ����
        /// </summary>
        public TagContainer()
        {
            tags = new List<GameplayTag>();
        }

        /// <summary>
        /// �±� �迭�� �����̳� ����
        /// </summary>
        public TagContainer(params GameplayTag[] initialTags)
        {
            tags = new List<GameplayTag>(initialTags.Where(t => t != null && t.IsValid));
        }

        /// <summary>
        /// �±� ���ڿ� �迭�� �����̳� ����
        /// </summary>
        public TagContainer(params string[] tagStrings)
        {
            tags = new List<GameplayTag>();
            foreach (var tagString in tagStrings)
            {
                if (!string.IsNullOrEmpty(tagString))
                {
                    tags.Add(new GameplayTag(tagString));
                }
            }
        }

        /// <summary>
        /// �ٸ� �����̳ʷκ��� ���� ����
        /// </summary>
        public TagContainer(TagContainer other)
        {
            tags = new List<GameplayTag>(other.tags);
        }
        #endregion

        #region Add Methods
        /// <summary>
        /// �±� �߰�
        /// </summary>
        public bool AddTag(GameplayTag tag)
        {
            if (tag == null || !tag.IsValid) return false;

            if (!HasTagExact(tag))
            {
                tags.Add(tag);
                OnTagAdded?.Invoke(tag);
                OnContainerChanged?.Invoke();
                return true;
            }

            return false;
        }

        /// <summary>
        /// ���ڿ��� �±� �߰�
        /// </summary>
        public bool AddTag(string tagString)
        {
            return AddTag(new GameplayTag(tagString));
        }

        /// <summary>
        /// ���� �±� �߰�
        /// </summary>
        public void AddTags(params GameplayTag[] tagsToAdd)
        {
            bool changed = false;
            foreach (var tag in tagsToAdd)
            {
                if (AddTag(tag))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                OnContainerChanged?.Invoke();
            }
        }

        /// <summary>
        /// �ٸ� �����̳��� ��� �±� �߰�
        /// </summary>
        public void AddTags(TagContainer other)
        {
            if (other == null || other.IsEmpty) return;
            AddTags(other.tags.ToArray());
        }
        #endregion

        #region Remove Methods
        /// <summary>
        /// �±� ����
        /// </summary>
        public bool RemoveTag(GameplayTag tag)
        {
            if (tag == null) return false;

            bool removed = tags.Remove(tag);
            if (removed)
            {
                OnTagRemoved?.Invoke(tag);
                OnContainerChanged?.Invoke();
            }

            return removed;
        }

        /// <summary>
        /// ���ڿ��� �±� ����
        /// </summary>
        public bool RemoveTag(string tagString)
        {
            var tag = tags.FirstOrDefault(t => t.TagString == tagString);
            if (tag != null)
            {
                return RemoveTag(tag);
            }
            return false;
        }

        /// <summary>
        /// ���� �±� ����
        /// </summary>
        public void RemoveTags(params GameplayTag[] tagsToRemove)
        {
            bool changed = false;
            foreach (var tag in tagsToRemove)
            {
                if (RemoveTag(tag))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                OnContainerChanged?.Invoke();
            }
        }

        /// <summary>
        /// �ٸ� �����̳��� ��� �±� ����
        /// </summary>
        public void RemoveTags(TagContainer other)
        {
            if (other == null || other.IsEmpty) return;
            RemoveTags(other.tags.ToArray());
        }

        /// <summary>
        /// �θ� �±׿� ��ġ�ϴ� ��� �±� ����
        /// </summary>
        public void RemoveTagsWithParent(GameplayTag parent)
        {
            if (parent == null || !parent.IsValid) return;

            var tagsToRemove = tags.Where(t => t.IsChildOf(parent) || t.MatchesExact(parent)).ToList();
            RemoveTags(tagsToRemove.ToArray());
        }

        /// <summary>
        /// ��� �±� ����
        /// </summary>
        public void Clear()
        {
            if (tags.Count > 0)
            {
                tags.Clear();
                OnContainerChanged?.Invoke();
            }
        }
        #endregion

        #region Query Methods
        /// <summary>
        /// ��Ȯ�� ��ġ�ϴ� �±װ� �ִ��� Ȯ��
        /// </summary>
        public bool HasTagExact(GameplayTag tag)
        {
            if (tag == null || !tag.IsValid) return false;
            return tags.Any(t => t.MatchesExact(tag));
        }

        /// <summary>
        /// �±� �Ǵ� �� �ڽ� �±װ� �ִ��� Ȯ��
        /// </summary>
        public bool HasTag(GameplayTag tag)
        {
            if (tag == null || !tag.IsValid) return false;
            return tags.Any(t => t.Matches(tag));
        }

        /// <summary>
        /// ���ڿ��� �±� Ȯ��
        /// </summary>
        public bool HasTag(string tagString)
        {
            return HasTag(new GameplayTag(tagString));
        }

        /// <summary>
        /// ���� �±� �� �ϳ��� �ִ��� Ȯ��
        /// </summary>
        public bool HasAny(List<GameplayTag> tagsToCheck)
        {
            foreach (var tag in tagsToCheck)
            {
                if (HasTag(tag)) return true;
            }
            return false;
        }

        /// <summary>
        /// ���� �±װ� ��� �ִ��� Ȯ��
        /// </summary>
        public bool HasAll(List<GameplayTag> tagsToCheck)
        {
            foreach (var tag in tagsToCheck)
            {
                if (!HasTag(tag)) return false;
            }
            return true;
        }

        /// <summary>
        /// �ٸ� �����̳��� �±� �� �ϳ��� �ִ��� Ȯ��
        /// </summary>
        public bool HasAny(TagContainer other)
        {
            if (other == null || other.IsEmpty) return false;
            return HasAny(other.tags);
        }

        /// <summary>
        /// �ٸ� �����̳��� ��� �±װ� �ִ��� Ȯ��
        /// </summary>
        public bool HasAll(TagContainer other)
        {
            if (other == null || other.IsEmpty) return true;
            return HasAll(other.tags);
        }

        /// <summary>
        /// Ư�� �θ� ���� �±׵� ��������
        /// </summary>
        public List<GameplayTag> GetTagsWithParent(GameplayTag parent)
        {
            if (parent == null || !parent.IsValid) return new List<GameplayTag>();
            return tags.Where(t => t.IsChildOf(parent)).ToList();
        }

        /// <summary>
        /// Ư�� ������ �±׵� ��������
        /// </summary>
        public List<GameplayTag> GetTagsAtDepth(int depth)
        {
            return tags.Where(t => t.Depth == depth).ToList();
        }

        /// <summary>
        /// ��Ʈ �±׵� ��������
        /// </summary>
        public List<GameplayTag> GetRootTags()
        {
            return GetTagsAtDepth(1);
        }
        #endregion

        #region Set Operations
        /// <summary>
        /// �� �����̳��� ������
        /// </summary>
        public static TagContainer Union(TagContainer a, TagContainer b)
        {
            var result = new TagContainer(a);
            if (b != null)
            {
                result.AddTags(b);
            }
            return result;
        }

        /// <summary>
        /// �� �����̳��� ������
        /// </summary>
        public static TagContainer Intersection(TagContainer a, TagContainer b)
        {
            var result = new TagContainer();
            if (a == null || b == null) return result;

            foreach (var tag in a.tags)
            {
                if (b.HasTagExact(tag))
                {
                    result.AddTag(tag);
                }
            }

            return result;
        }

        /// <summary>
        /// �� �����̳��� ������ (a - b)
        /// </summary>
        public static TagContainer Difference(TagContainer a, TagContainer b)
        {
            var result = new TagContainer(a);
            if (b != null)
            {
                result.RemoveTags(b);
            }
            return result;
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// �����̳� ����
        /// </summary>
        public TagContainer Clone()
        {
            return new TagContainer(this);
        }

        /// <summary>
        /// �±� ����
        /// </summary>
        public void Sort()
        {
            tags.Sort();
            OnContainerChanged?.Invoke();
        }

        /// <summary>
        /// �ߺ� �±� ����
        /// </summary>
        public void RemoveDuplicates()
        {
            var uniqueTags = tags.Distinct().ToList();
            if (uniqueTags.Count != tags.Count)
            {
                tags = uniqueTags;
                OnContainerChanged?.Invoke();
            }
        }

        /// <summary>
        /// ���ڿ� �迭�� ��ȯ
        /// </summary>
        public string[] ToStringArray()
        {
            return tags.Select(t => t.TagString).ToArray();
        }

        /// <summary>
        /// ����׿� ���ڿ� ���
        /// </summary>
        public override string ToString()
        {
            if (IsEmpty) return "[]";
            return $"[{string.Join(", ", tags.Select(t => t.ToString()))}]";
        }
        #endregion

        #region IEnumerable Implementation
        public IEnumerator<GameplayTag> GetEnumerator()
        {
            return tags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}