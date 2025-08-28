// 파일 위치: Assets/Scripts/GAS/TagSystem/GameplayTag.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS.TagSystem
{
    /// <summary>
    /// 게임플레이 태그를 나타내는 클래스
    /// 계층적 구조를 지원 (예: "Status.Buff.Speed")
    /// </summary>
    [Serializable]
    public class GameplayTag : IEquatable<GameplayTag>, IComparable<GameplayTag>
    {
        #region Constants
        private const char SEPARATOR = '.';
        private const string EMPTY_TAG = "None";
        #endregion

        #region Serialized Fields
        [SerializeField] private string tagString;
        #endregion

        #region Private Fields
        private string[] segments;
        private int depth;
        private int cachedHashCode;
        #endregion

        #region Properties
        /// <summary>
        /// 태그 문자열
        /// </summary>
        public string TagString => tagString;

        /// <summary>
        /// 태그의 깊이 (점으로 구분된 세그먼트 수)
        /// </summary>
        public int Depth
        {
            get
            {
                if (depth == 0 && !string.IsNullOrEmpty(tagString))
                {
                    ParseTag();
                }
                return depth;
            }
        }

        /// <summary>
        /// 태그 세그먼트 배열
        /// </summary>
        public string[] Segments
        {
            get
            {
                if (segments == null && !string.IsNullOrEmpty(tagString))
                {
                    ParseTag();
                }
                return segments ?? Array.Empty<string>();
            }
        }

        /// <summary>
        /// 태그가 비어있는지 확인
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(tagString) || tagString == EMPTY_TAG;

        /// <summary>
        /// 태그가 유효한지 확인
        /// </summary>
        public bool IsValid => !IsEmpty && segments != null && segments.Length > 0;
        #endregion

        #region Constructors
        /// <summary>
        /// 빈 태그 생성
        /// </summary>
        public GameplayTag()
        {
            tagString = EMPTY_TAG;
            segments = Array.Empty<string>();
            depth = 0;
        }

        /// <summary>
        /// 문자열로부터 태그 생성
        /// </summary>
        public GameplayTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                tagString = EMPTY_TAG;
                segments = Array.Empty<string>();
                depth = 0;
            }
            else
            {
                tagString = tag.Trim();
                ParseTag();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 이 태그가 다른 태그와 정확히 일치하는지 확인
        /// </summary>
        public bool MatchesExact(GameplayTag other)
        {
            if (other == null) return false;
            return string.Equals(tagString, other.tagString, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 이 태그가 다른 태그의 자식인지 확인
        /// 예: "Status.Buff.Speed"는 "Status.Buff"의 자식
        /// </summary>
        public bool IsChildOf(GameplayTag parent)
        {
            if (parent == null || parent.IsEmpty) return false;
            if (IsEmpty) return false;
            if (parent.Depth >= Depth) return false;

            for (int i = 0; i < parent.Segments.Length; i++)
            {
                if (!string.Equals(Segments[i], parent.Segments[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 이 태그가 다른 태그의 부모인지 확인
        /// 예: "Status.Buff"는 "Status.Buff.Speed"의 부모
        /// </summary>
        public bool IsParentOf(GameplayTag child)
        {
            if (child == null) return false;
            return child.IsChildOf(this);
        }

        /// <summary>
        /// 이 태그가 다른 태그와 일치하거나 자식인지 확인
        /// </summary>
        public bool Matches(GameplayTag other)
        {
            return MatchesExact(other) || IsChildOf(other);
        }

        /// <summary>
        /// 부모 태그 가져오기
        /// 예: "Status.Buff.Speed"의 부모는 "Status.Buff"
        /// </summary>
        public GameplayTag GetParent()
        {
            if (Depth <= 1) return new GameplayTag();

            string[] parentSegments = new string[segments.Length - 1];
            Array.Copy(segments, parentSegments, parentSegments.Length);
            return new GameplayTag(string.Join(SEPARATOR.ToString(), parentSegments));
        }

        /// <summary>
        /// 루트 태그 가져오기
        /// 예: "Status.Buff.Speed"의 루트는 "Status"
        /// </summary>
        public GameplayTag GetRoot()
        {
            if (IsEmpty || segments.Length == 0) return new GameplayTag();
            return new GameplayTag(segments[0]);
        }

        /// <summary>
        /// 특정 깊이까지의 태그 가져오기
        /// </summary>
        public GameplayTag GetTagAtDepth(int targetDepth)
        {
            if (targetDepth <= 0 || targetDepth > Depth) return this;

            string[] newSegments = new string[targetDepth];
            Array.Copy(segments, newSegments, targetDepth);
            return new GameplayTag(string.Join(SEPARATOR.ToString(), newSegments));
        }

        /// <summary>
        /// 자식 태그 생성
        /// </summary>
        public GameplayTag CreateChild(string childSegment)
        {
            if (string.IsNullOrEmpty(childSegment)) return this;
            if (IsEmpty) return new GameplayTag(childSegment);

            return new GameplayTag($"{tagString}{SEPARATOR}{childSegment}");
        }

        /// <summary>
        /// 두 태그 간의 공통 부모 찾기
        /// </summary>
        public static GameplayTag GetCommonParent(GameplayTag tag1, GameplayTag tag2)
        {
            if (tag1 == null || tag2 == null || tag1.IsEmpty || tag2.IsEmpty)
                return new GameplayTag();

            int minDepth = Math.Min(tag1.Depth, tag2.Depth);

            for (int i = 0; i < minDepth; i++)
            {
                if (!string.Equals(tag1.Segments[i], tag2.Segments[i], StringComparison.OrdinalIgnoreCase))
                {
                    if (i == 0) return new GameplayTag();

                    string[] commonSegments = new string[i];
                    Array.Copy(tag1.Segments, commonSegments, i);
                    return new GameplayTag(string.Join(SEPARATOR.ToString(), commonSegments));
                }
            }

            return tag1.Depth < tag2.Depth ? tag1 : tag2;
        }
        #endregion

        #region Private Methods
        private void ParseTag()
        {
            if (string.IsNullOrEmpty(tagString))
            {
                segments = Array.Empty<string>();
                depth = 0;
                return;
            }

            segments = tagString.Split(SEPARATOR);
            depth = segments.Length;

            // Validate segments
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = segments[i].Trim();
                if (string.IsNullOrEmpty(segments[i]))
                {
                    Debug.LogError($"[GAS] Invalid tag format: {tagString}");
                    segments = Array.Empty<string>();
                    depth = 0;
                    return;
                }
            }

            // Cache hash code
            cachedHashCode = tagString.GetHashCode();
        }
        #endregion

        #region Equality and Comparison
        public bool Equals(GameplayTag other)
        {
            if (other == null) return false;
            return string.Equals(tagString, other.tagString, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is GameplayTag tag && Equals(tag);
        }

        public override int GetHashCode()
        {
            if (cachedHashCode == 0 && !string.IsNullOrEmpty(tagString))
            {
                cachedHashCode = tagString.GetHashCode();
            }
            return cachedHashCode;
        }

        public int CompareTo(GameplayTag other)
        {
            if (other == null) return 1;
            return string.Compare(tagString, other.tagString, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return tagString ?? EMPTY_TAG;
        }
        #endregion

        #region Operators
        public static bool operator ==(GameplayTag left, GameplayTag right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(GameplayTag left, GameplayTag right)
        {
            return !(left == right);
        }

        public static implicit operator string(GameplayTag tag)
        {
            return tag?.tagString ?? EMPTY_TAG;
        }

        public static implicit operator GameplayTag(string tagString)
        {
            return new GameplayTag(tagString);
        }
        #endregion
    }
}