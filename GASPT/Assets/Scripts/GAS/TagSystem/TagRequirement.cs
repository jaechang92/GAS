// 파일 위치: Assets/Scripts/GAS/TagSystem/TagRequirement.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GAS.TagSystem
{
    /// <summary>
    /// 태그 요구사항을 정의하는 클래스
    /// 능력이나 효과 활성화 조건 검사에 사용
    /// </summary>
    [Serializable]
    public class TagRequirement
    {
        #region Nested Types
        /// <summary>
        /// 태그 조건 타입
        /// </summary>
        public enum RequirementType
        {
            RequireAll,     // 모든 태그 필요
            RequireAny,     // 하나 이상 필요
            RequireNone,    // 아무것도 없어야 함
            Custom          // 커스텀 로직
        }

        /// <summary>
        /// 태그 조건 항목
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
        /// 요구사항 이름
        /// </summary>
        public string RequirementName
        {
            get => requirementName;
            set => requirementName = value;
        }

        /// <summary>
        /// 요구사항 타입
        /// </summary>
        public RequirementType Type
        {
            get => type;
            set => type = value;
        }

        /// <summary>
        /// 요구사항이 비어있는지 확인
        /// </summary>
        public bool IsEmpty =>
            requiredTags.Count == 0 &&
            blockedTags.Count == 0 &&
            advancedRequirements.Count == 0;

        /// <summary>
        /// 필수 태그 리스트
        /// </summary>
        public IReadOnlyList<GameplayTag> RequiredTags => requiredTags.AsReadOnly();

        /// <summary>
        /// 차단 태그 리스트
        /// </summary>
        public IReadOnlyList<GameplayTag> BlockedTags => blockedTags.AsReadOnly();
        #endregion

        #region Constructors
        /// <summary>
        /// 빈 요구사항 생성
        /// </summary>
        public TagRequirement()
        {
            requirementName = "New Requirement";
            type = RequirementType.RequireAll;
        }

        /// <summary>
        /// 이름과 타입으로 요구사항 생성
        /// </summary>
        public TagRequirement(string name, RequirementType requirementType = RequirementType.RequireAll)
        {
            requirementName = name;
            type = requirementType;
        }

        /// <summary>
        /// 태그 배열로 요구사항 생성
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
        /// 태그 컨테이너가 요구사항을 만족하는지 검사
        /// </summary>
        public bool IsSatisfiedBy(TagContainer container)
        {
            // 빈 요구사항 처리
            if (IsEmpty)
            {
                return ignoreIfEmpty;
            }

            // 차단 태그 검사 (항상 우선)
            if (!CheckBlockedTags(container))
            {
                return false;
            }

            // 고급 요구사항 검사
            if (advancedRequirements.Count > 0)
            {
                if (!CheckAdvancedRequirements(container))
                {
                    return false;
                }
            }

            // 필수 태그 검사
            return CheckRequiredTags(container);
        }

        /// <summary>
        /// 필수 태그 검사
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
            // 커스텀 로직은 상속받아 구현하거나 델리게이트로 처리
            // 기본적으로는 RequireAll과 동일하게 동작
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
        /// 필수 태그 추가
        /// </summary>
        public void AddRequiredTag(GameplayTag tag)
        {
            if (tag != null && tag.IsValid && !requiredTags.Contains(tag))
            {
                requiredTags.Add(tag);
            }
        }

        /// <summary>
        /// 필수 태그 제거
        /// </summary>
        public void RemoveRequiredTag(GameplayTag tag)
        {
            requiredTags.Remove(tag);
        }

        /// <summary>
        /// 차단 태그 추가
        /// </summary>
        public void AddBlockedTag(GameplayTag tag)
        {
            if (tag != null && tag.IsValid && !blockedTags.Contains(tag))
            {
                blockedTags.Add(tag);
            }
        }

        /// <summary>
        /// 차단 태그 제거
        /// </summary>
        public void RemoveBlockedTag(GameplayTag tag)
        {
            blockedTags.Remove(tag);
        }

        /// <summary>
        /// 고급 요구사항 추가
        /// </summary>
        public void AddAdvancedRequirement(GameplayTag tag, bool matchExact, bool inverted)
        {
            if (tag != null && tag.IsValid)
            {
                advancedRequirements.Add(new RequirementEntry(tag, matchExact, inverted));
            }
        }

        /// <summary>
        /// 모든 요구사항 초기화
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
        /// 요구사항 복사
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
        /// 충족되지 않은 태그 가져오기
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
        /// 차단된 태그 중 보유한 태그 가져오기
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
        /// 디버그용 문자열 출력
        /// </summary>
        public override string ToString()
        {
            return $"{requirementName} [{type}] Required: {requiredTags.Count}, Blocked: {blockedTags.Count}";
        }
        #endregion
    }
}