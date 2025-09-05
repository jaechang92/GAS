// ���� ��ġ: Assets/Scripts/GAS/TagSystem/TagRequirement.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GAS.TagSystem
{
    /// <summary>
    /// �±� �䱸������ �����ϴ� Ŭ����
    /// �ɷ��̳� ȿ�� Ȱ��ȭ ���� �˻翡 ���
    /// </summary>
    [Serializable]
    public class TagRequirement
    {
        #region Nested Types
        /// <summary>
        /// �±� ���� Ÿ��
        /// </summary>
        public enum RequirementType
        {
            RequireAll,     // ��� �±� �ʿ�
            RequireAny,     // �ϳ� �̻� �ʿ�
            RequireNone,    // �ƹ��͵� ����� ��
            Custom          // Ŀ���� ����
        }

        /// <summary>
        /// �±� ���� �׸�
        /// </summary>
        [Serializable]
        public class RequirementEntry
        {
            [SerializeField] private GameplayTag tag;
            [SerializeField] private bool matchExact = false;
            [SerializeField] private bool inverted = false;

            public GameplayTag Tag => tag;
            public bool MatchExact => matchExact;
            public bool Inverted => inverted;

            public RequirementEntry(GameplayTag tag, bool matchExact = false, bool inverted = false)
            {
                this.tag = tag;
                this.matchExact = matchExact;
                this.inverted = inverted;
            }

            public bool Check(TagContainer container)
            {
                if (container == null) return inverted;

                bool hasTag = matchExact ?
                    container.HasTagExact(tag) :
                    container.HasTag(tag);

                return inverted ? !hasTag : hasTag;
            }
        }
        #endregion

        #region Serialized Fields
        [SerializeField] private string requirementName;
        [SerializeField] private RequirementType type = RequirementType.RequireAll;

        [Header("Required Tags")]
        [SerializeField] private List<GameplayTag> requiredTags = new List<GameplayTag>();

        [Header("Blocked Tags")]
        [SerializeField] private List<GameplayTag> blockedTags = new List<GameplayTag>();

        [Header("Advanced Requirements")]
        [SerializeField] private List<RequirementEntry> advancedRequirements = new List<RequirementEntry>();

        [Header("Options")]
        [SerializeField] private bool matchExact = false;
        [SerializeField] private bool ignoreIfEmpty = true;
        #endregion

        #region Properties
        public bool IgnoreIfEmpty => ignoreIfEmpty;

        /// <summary>
        /// �䱸���� �̸�
        /// </summary>
        public string RequirementName
        {
            get => requirementName;
            set => requirementName = value;
        }

        /// <summary>
        /// �䱸���� Ÿ��
        /// </summary>
        public RequirementType Type
        {
            get => type;
            set => type = value;
        }

        /// <summary>
        /// �䱸������ ����ִ��� Ȯ��
        /// </summary>
        public bool IsEmpty =>
            requiredTags.Count == 0 &&
            blockedTags.Count == 0 &&
            advancedRequirements.Count == 0;

        /// <summary>
        /// �ʼ� �±� ����Ʈ
        /// </summary>
        public IReadOnlyList<GameplayTag> RequiredTags => requiredTags.AsReadOnly();

        /// <summary>
        /// ���� �±� ����Ʈ
        /// </summary>
        public IReadOnlyList<GameplayTag> BlockedTags => blockedTags.AsReadOnly();
        #endregion

        #region Constructors
        /// <summary>
        /// �� �䱸���� ����
        /// </summary>
        public TagRequirement()
        {
            requirementName = "New Requirement";
            type = RequirementType.RequireAll;
        }

        /// <summary>
        /// �̸��� Ÿ������ �䱸���� ����
        /// </summary>
        public TagRequirement(string name, RequirementType requirementType = RequirementType.RequireAll)
        {
            requirementName = name;
            type = requirementType;
        }

        /// <summary>
        /// �±� �迭�� �䱸���� ����
        /// </summary>
        public TagRequirement(string name, RequirementType requirementType, params GameplayTag[] required)
        {
            requirementName = name;
            type = requirementType;
            requiredTags = new List<GameplayTag>(required.Where(t => t != null && t.IsValid));
        }
        #endregion

        #region Check Methods
        /// <summary>
        /// �±� �����̳ʰ� �䱸������ �����ϴ��� �˻�
        /// </summary>
        public bool IsSatisfiedBy(TagContainer container)
        {
            // �� �䱸���� ó��
            if (IsEmpty)
            {
                return ignoreIfEmpty;
            }

            // ���� �±� �˻� (�׻� �켱)
            if (!CheckBlockedTags(container))
            {
                return false;
            }

            // ��� �䱸���� �˻�
            if (advancedRequirements.Count > 0)
            {
                if (!CheckAdvancedRequirements(container))
                {
                    return false;
                }
            }

            // �ʼ� �±� �˻�
            return CheckRequiredTags(container);
        }

        /// <summary>
        /// �ʼ� �±� �˻�
        /// </summary>
        private bool CheckRequiredTags(TagContainer container)
        {
            if (requiredTags.Count == 0) return true;

            switch (type)
            {
                case RequirementType.RequireAll:
                    return CheckRequireAll(container);

                case RequirementType.RequireAny:
                    return CheckRequireAny(container);

                case RequirementType.RequireNone:
                    return CheckRequireNone(container);

                case RequirementType.Custom:
                    return CheckCustom(container);

                default:
                    return false;
            }
        }

        private bool CheckRequireAll(TagContainer container)
        {
            if (container == null || container.IsEmpty)
            {
                return requiredTags.Count == 0;
            }

            foreach (var tag in requiredTags)
            {
                if (tag == null || !tag.IsValid) continue;

                bool hasTag = matchExact ?
                    container.HasTagExact(tag) :
                    container.HasTag(tag);

                if (!hasTag) return false;
            }

            return true;
        }

        private bool CheckRequireAny(TagContainer container)
        {
            if (container == null || container.IsEmpty)
            {
                return requiredTags.Count == 0;
            }

            foreach (var tag in requiredTags)
            {
                if (tag == null || !tag.IsValid) continue;

                bool hasTag = matchExact ?
                    container.HasTagExact(tag) :
                    container.HasTag(tag);

                if (hasTag) return true;
            }

            return requiredTags.Count == 0;
        }

        private bool CheckRequireNone(TagContainer container)
        {
            if (container == null || container.IsEmpty)
            {
                return true;
            }

            foreach (var tag in requiredTags)
            {
                if (tag == null || !tag.IsValid) continue;

                bool hasTag = matchExact ?
                    container.HasTagExact(tag) :
                    container.HasTag(tag);

                if (hasTag) return false;
            }

            return true;
        }

        private bool CheckCustom(TagContainer container)
        {
            // Ŀ���� ������ ��ӹ޾� �����ϰų� ��������Ʈ�� ó��
            // �⺻�����δ� RequireAll�� �����ϰ� ����
            return CheckRequireAll(container);
        }

        private bool CheckBlockedTags(TagContainer container)
        {
            if (blockedTags.Count == 0) return true;
            if (container == null || container.IsEmpty) return true;

            foreach (var tag in blockedTags)
            {
                if (tag == null || !tag.IsValid) continue;

                bool hasTag = matchExact ?
                    container.HasTagExact(tag) :
                    container.HasTag(tag);

                if (hasTag) return false;
            }

            return true;
        }

        private bool CheckAdvancedRequirements(TagContainer container)
        {
            foreach (var requirement in advancedRequirements)
            {
                if (!requirement.Check(container))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Modification Methods
        /// <summary>
        /// �ʼ� �±� �߰�
        /// </summary>
        public void AddRequiredTag(GameplayTag tag)
        {
            if (tag != null && tag.IsValid && !requiredTags.Contains(tag))
            {
                requiredTags.Add(tag);
            }
        }

        /// <summary>
        /// �ʼ� �±� ����
        /// </summary>
        public void RemoveRequiredTag(GameplayTag tag)
        {
            requiredTags.Remove(tag);
        }

        /// <summary>
        /// ���� �±� �߰�
        /// </summary>
        public void AddBlockedTag(GameplayTag tag)
        {
            if (tag != null && tag.IsValid && !blockedTags.Contains(tag))
            {
                blockedTags.Add(tag);
            }
        }

        /// <summary>
        /// ���� �±� ����
        /// </summary>
        public void RemoveBlockedTag(GameplayTag tag)
        {
            blockedTags.Remove(tag);
        }

        /// <summary>
        /// ��� �䱸���� �߰�
        /// </summary>
        public void AddAdvancedRequirement(GameplayTag tag, bool matchExact, bool inverted)
        {
            if (tag != null && tag.IsValid)
            {
                advancedRequirements.Add(new RequirementEntry(tag, matchExact, inverted));
            }
        }

        /// <summary>
        /// ��� �䱸���� �ʱ�ȭ
        /// </summary>
        public void Clear()
        {
            requiredTags.Clear();
            blockedTags.Clear();
            advancedRequirements.Clear();
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// �䱸���� ����
        /// </summary>
        public TagRequirement Clone()
        {
            var clone = new TagRequirement(requirementName, type)
            {
                requiredTags = new List<GameplayTag>(requiredTags),
                blockedTags = new List<GameplayTag>(blockedTags),
                advancedRequirements = new List<RequirementEntry>(advancedRequirements),
                matchExact = matchExact,
                ignoreIfEmpty = ignoreIfEmpty
            };

            return clone;
        }

        /// <summary>
        /// �������� ���� �±� ��������
        /// </summary>
        public List<GameplayTag> GetMissingTags(TagContainer container)
        {
            var missing = new List<GameplayTag>();

            if (type == RequirementType.RequireAll)
            {
                foreach (var tag in requiredTags)
                {
                    if (tag == null || !tag.IsValid) continue;

                    bool hasTag = matchExact ?
                        container?.HasTagExact(tag) ?? false :
                        container?.HasTag(tag) ?? false;

                    if (!hasTag)
                    {
                        missing.Add(tag);
                    }
                }
            }

            return missing;
        }

        /// <summary>
        /// ���ܵ� �±� �� ������ �±� ��������
        /// </summary>
        public List<GameplayTag> GetBlockingTags(TagContainer container)
        {
            var blocking = new List<GameplayTag>();

            if (container == null || container.IsEmpty) return blocking;

            foreach (var tag in blockedTags)
            {
                if (tag == null || !tag.IsValid) continue;

                bool hasTag = matchExact ?
                    container.HasTagExact(tag) :
                    container.HasTag(tag);

                if (hasTag)
                {
                    blocking.Add(tag);
                }
            }

            return blocking;
        }

        /// <summary>
        /// ����׿� ���ڿ� ���
        /// </summary>
        public override string ToString()
        {
            return $"{requirementName} [{type}] Required: {requiredTags.Count}, Blocked: {blockedTags.Count}";
        }
        #endregion
    }
}