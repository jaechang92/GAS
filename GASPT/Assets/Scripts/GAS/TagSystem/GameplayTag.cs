// ���� ��ġ: Assets/Scripts/GAS/TagSystem/GameplayTag.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS.TagSystem
{
    /// <summary>
    /// �����÷��� �±׸� ��Ÿ���� Ŭ����
    /// ������ ������ ���� (��: "Status.Buff.Speed")
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
        /// �±� ���ڿ�
        /// </summary>
        public string TagString => tagString;

        /// <summary>
        /// �±��� ���� (������ ���е� ���׸�Ʈ ��)
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
        /// �±� ���׸�Ʈ �迭
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
        /// �±װ� ����ִ��� Ȯ��
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(tagString) || tagString == EMPTY_TAG;

        /// <summary>
        /// �±װ� ��ȿ���� Ȯ��
        /// </summary>
        public bool IsValid => !IsEmpty && segments != null && segments.Length > 0;
        #endregion

        #region Constructors
        /// <summary>
        /// �� �±� ����
        /// </summary>
        public GameplayTag()
        {
            tagString = EMPTY_TAG;
            segments = Array.Empty<string>();
            depth = 0;
        }

        /// <summary>
        /// ���ڿ��κ��� �±� ����
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
        /// �� �±װ� �ٸ� �±׿� ��Ȯ�� ��ġ�ϴ��� Ȯ��
        /// </summary>
        public bool MatchesExact(GameplayTag other)
        {
            if (other == null) return false;
            return string.Equals(tagString, other.tagString, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// �� �±װ� �ٸ� �±��� �ڽ����� Ȯ��
        /// ��: "Status.Buff.Speed"�� "Status.Buff"�� �ڽ�
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
        /// �� �±װ� �ٸ� �±��� �θ����� Ȯ��
        /// ��: "Status.Buff"�� "Status.Buff.Speed"�� �θ�
        /// </summary>
        public bool IsParentOf(GameplayTag child)
        {
            if (child == null) return false;
            return child.IsChildOf(this);
        }

        /// <summary>
        /// �� �±װ� �ٸ� �±׿� ��ġ�ϰų� �ڽ����� Ȯ��
        /// </summary>
        public bool Matches(GameplayTag other)
        {
            return MatchesExact(other) || IsChildOf(other);
        }

        /// <summary>
        /// �θ� �±� ��������
        /// ��: "Status.Buff.Speed"�� �θ�� "Status.Buff"
        /// </summary>
        public GameplayTag GetParent()
        {
            if (Depth <= 1) return new GameplayTag();

            string[] parentSegments = new string[segments.Length - 1];
            Array.Copy(segments, parentSegments, parentSegments.Length);
            return new GameplayTag(string.Join(SEPARATOR.ToString(), parentSegments));
        }

        /// <summary>
        /// ��Ʈ �±� ��������
        /// ��: "Status.Buff.Speed"�� ��Ʈ�� "Status"
        /// </summary>
        public GameplayTag GetRoot()
        {
            if (IsEmpty || segments.Length == 0) return new GameplayTag();
            return new GameplayTag(segments[0]);
        }

        /// <summary>
        /// Ư�� ���̱����� �±� ��������
        /// </summary>
        public GameplayTag GetTagAtDepth(int targetDepth)
        {
            if (targetDepth <= 0 || targetDepth > Depth) return this;

            string[] newSegments = new string[targetDepth];
            Array.Copy(segments, newSegments, targetDepth);
            return new GameplayTag(string.Join(SEPARATOR.ToString(), newSegments));
        }

        /// <summary>
        /// �ڽ� �±� ����
        /// </summary>
        public GameplayTag CreateChild(string childSegment)
        {
            if (string.IsNullOrEmpty(childSegment)) return this;
            if (IsEmpty) return new GameplayTag(childSegment);

            return new GameplayTag($"{tagString}{SEPARATOR}{childSegment}");
        }

        /// <summary>
        /// �� �±� ���� ���� �θ� ã��
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