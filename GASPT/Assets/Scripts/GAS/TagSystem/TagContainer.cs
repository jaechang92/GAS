// 파일 위치: Assets/Scripts/GAS/TagSystem/TagContainer.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GAS.TagSystem
{
    /// <summary>
    /// 여러 GameplayTag를 관리하는 컨테이너
    /// </summary>
    [Serializable]
    public class TagContainer : IEnumerable<GameplayTag>
    {
        #region Serialized Fields
        [SerializeField] private List<GameplayTag> tags = new List<GameplayTag>();
        #endregion

        #region Events
        /// <summary>
        /// 태그가 추가되었을 때
        /// </summary>
        public event Action<GameplayTag> OnTagAdded;

        /// <summary>
        /// 태그가 제거되었을 때
        /// </summary>
        public event Action<GameplayTag> OnTagRemoved;

        /// <summary>
        /// 컨테이너가 변경되었을 때
        /// </summary>
        public event Action OnContainerChanged;
        #endregion

        #region Properties
        /// <summary>
        /// 태그 개수
        /// </summary>
        public int Count => tags.Count;

        /// <summary>
        /// 컨테이너가 비어있는지 확인
        /// </summary>
        public bool IsEmpty => tags.Count == 0;

        /// <summary>
        /// 모든 태그 리스트 (읽기 전용)
        /// </summary>
        public IReadOnlyList<GameplayTag> Tags => tags.AsReadOnly();
        #endregion

        #region Constructors
        /// <summary>
        /// 빈 컨테이너 생성
        /// </summary>
        public TagContainer()
        {
            tags = new List<GameplayTag>();
        }

        /// <summary>
        /// 태그 배열로 컨테이너 생성
        /// </summary>
        public TagContainer(params GameplayTag[] initialTags)
        {
            tags = new List<GameplayTag>(initialTags.Where(t => t != null && t.IsValid));
        }

        /// <summary>
        /// 태그 문자열 배열로 컨테이너 생성
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
        /// 다른 컨테이너로부터 복사 생성
        /// </summary>
        public TagContainer(TagContainer other)
        {
            tags = new List<GameplayTag>(other.tags);
        }
        #endregion

        #region Add Methods
        /// <summary>
        /// 태그 추가
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
        /// 문자열로 태그 추가
        /// </summary>
        public bool AddTag(string tagString)
        {
            return AddTag(new GameplayTag(tagString));
        }

        /// <summary>
        /// 여러 태그 추가
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
        /// 다른 컨테이너의 모든 태그 추가
        /// </summary>
        public void AddTags(TagContainer other)
        {
            if (other == null || other.IsEmpty) return;
            AddTags(other.tags.ToArray());
        }
        #endregion

        #region Remove Methods
        /// <summary>
        /// 태그 제거
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
        /// 문자열로 태그 제거
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
        /// 여러 태그 제거
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
        /// 다른 컨테이너의 모든 태그 제거
        /// </summary>
        public void RemoveTags(TagContainer other)
        {
            if (other == null || other.IsEmpty) return;
            RemoveTags(other.tags.ToArray());
        }

        /// <summary>
        /// 부모 태그와 일치하는 모든 태그 제거
        /// </summary>
        public void RemoveTagsWithParent(GameplayTag parent)
        {
            if (parent == null || !parent.IsValid) return;

            var tagsToRemove = tags.Where(t => t.IsChildOf(parent) || t.MatchesExact(parent)).ToList();
            RemoveTags(tagsToRemove.ToArray());
        }

        /// <summary>
        /// 모든 태그 제거
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
        /// 정확히 일치하는 태그가 있는지 확인
        /// </summary>
        public bool HasTagExact(GameplayTag tag)
        {
            if (tag == null || !tag.IsValid) return false;
            return tags.Any(t => t.MatchesExact(tag));
        }

        /// <summary>
        /// 태그 또는 그 자식 태그가 있는지 확인
        /// </summary>
        public bool HasTag(GameplayTag tag)
        {
            if (tag == null || !tag.IsValid) return false;
            return tags.Any(t => t.Matches(tag));
        }

        /// <summary>
        /// 문자열로 태그 확인
        /// </summary>
        public bool HasTag(string tagString)
        {
            return HasTag(new GameplayTag(tagString));
        }

        /// <summary>
        /// 여러 태그 중 하나라도 있는지 확인
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
        /// 여러 태그가 모두 있는지 확인
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
        /// 다른 컨테이너의 태그 중 하나라도 있는지 확인
        /// </summary>
        public bool HasAny(TagContainer other)
        {
            if (other == null || other.IsEmpty) return false;
            return HasAny(other.tags);
        }

        /// <summary>
        /// 다른 컨테이너의 모든 태그가 있는지 확인
        /// </summary>
        public bool HasAll(TagContainer other)
        {
            if (other == null || other.IsEmpty) return true;
            return HasAll(other.tags);
        }

        /// <summary>
        /// 특정 부모를 가진 태그들 가져오기
        /// </summary>
        public List<GameplayTag> GetTagsWithParent(GameplayTag parent)
        {
            if (parent == null || !parent.IsValid) return new List<GameplayTag>();
            return tags.Where(t => t.IsChildOf(parent)).ToList();
        }

        /// <summary>
        /// 특정 깊이의 태그들 가져오기
        /// </summary>
        public List<GameplayTag> GetTagsAtDepth(int depth)
        {
            return tags.Where(t => t.Depth == depth).ToList();
        }

        /// <summary>
        /// 루트 태그들 가져오기
        /// </summary>
        public List<GameplayTag> GetRootTags()
        {
            return GetTagsAtDepth(1);
        }
        #endregion

        #region Set Operations
        /// <summary>
        /// 두 컨테이너의 합집합
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
        /// 두 컨테이너의 교집합
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
        /// 두 컨테이너의 차집합 (a - b)
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
        /// 컨테이너 복사
        /// </summary>
        public TagContainer Clone()
        {
            return new TagContainer(this);
        }

        /// <summary>
        /// 태그 정렬
        /// </summary>
        public void Sort()
        {
            tags.Sort();
            OnContainerChanged?.Invoke();
        }

        /// <summary>
        /// 중복 태그 제거
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
        /// 문자열 배열로 변환
        /// </summary>
        public string[] ToStringArray()
        {
            return tags.Select(t => t.TagString).ToArray();
        }

        /// <summary>
        /// 디버그용 문자열 출력
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