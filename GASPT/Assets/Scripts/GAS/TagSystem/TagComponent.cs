// 파일 위치: Assets/Scripts/GAS/TagSystem/TagComponent.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.Core;

namespace GAS.TagSystem
{
    /// <summary>
    /// GameObject에 태그 시스템을 부여하는 컴포넌트
    /// </summary>
    [AddComponentMenu("GAS/Tag Component")]
    public class TagComponent : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Initial Tags")]
        [SerializeField] private List<string> initialTags = new List<string>();

        [Header("Settings")]
        [SerializeField] private bool persistTags = false;
        [SerializeField] private bool debugMode = false;
        #endregion

        #region Private Fields
        private TagContainer container;
        private Dictionary<GameplayTag, int> tagCountMap = new Dictionary<GameplayTag, int>();
        #endregion

        #region Properties
        /// <summary>
        /// 태그 컨테이너
        /// </summary>
        public TagContainer Container
        {
            get
            {
                if (container == null)
                {
                    container = new TagContainer();
                }
                return container;
            }
        }

        /// <summary>
        /// 현재 태그 개수
        /// </summary>
        public int TagCount => Container.Count;

        /// <summary>
        /// 태그가 있는지 확인
        /// </summary>
        public bool HasTags => !Container.IsEmpty;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeContainer();
        }

        private void Start()
        {
            // GASManager에 등록
            GASManager.Instance.RegisterObject(gameObject);

            // 초기 태그 적용
            ApplyInitialTags();
        }

        private void OnDestroy()
        {
            // GASManager에서 해제
            if (GASManager.Instance != null)
            {
                GASManager.Instance.UnregisterObject(gameObject);
            }

            // 이벤트 정리
            if (container != null)
            {
                container.OnTagAdded -= HandleTagAdded;
                container.OnTagRemoved -= HandleTagRemoved;
                container.OnContainerChanged -= HandleContainerChanged;
            }
        }

        private void OnValidate()
        {
            // Inspector에서 태그 수정 시 유효성 검사
            for (int i = initialTags.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(initialTags[i]))
                {
                    initialTags.RemoveAt(i);
                }
            }
        }
        #endregion

        #region Initialization
        private void InitializeContainer()
        {
            container = new TagContainer();

            // 이벤트 연결
            container.OnTagAdded += HandleTagAdded;
            container.OnTagRemoved += HandleTagRemoved;
            container.OnContainerChanged += HandleContainerChanged;
        }

        private void ApplyInitialTags()
        {
            foreach (var tagString in initialTags)
            {
                if (!string.IsNullOrEmpty(tagString))
                {
                    AddTag(tagString);
                }
            }
        }
        #endregion

        #region Tag Management
        /// <summary>
        /// 태그 추가
        /// </summary>
        public bool AddTag(string tagString)
        {
            return AddTag(new GameplayTag(tagString));
        }

        /// <summary>
        /// 태그 추가 (카운트 지원)
        /// </summary>
        public bool AddTag(GameplayTag tag)
        {
            if (tag == null || !tag.IsValid) return false;

            // 태그 카운트 관리
            if (tagCountMap.ContainsKey(tag))
            {
                tagCountMap[tag]++;
                if (debugMode)
                {
                    Debug.Log($"[GAS] Tag count increased: {tag} x{tagCountMap[tag]} on {name}");
                }
                return false; // 이미 있는 태그
            }

            // 새 태그 추가
            if (Container.AddTag(tag))
            {
                tagCountMap[tag] = 1;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 여러 태그 추가
        /// </summary>
        public void AddTags(params string[] tagStrings)
        {
            foreach (var tagString in tagStrings)
            {
                AddTag(tagString);
            }
        }

        /// <summary>
        /// 태그 제거
        /// </summary>
        public bool RemoveTag(string tagString)
        {
            return RemoveTag(new GameplayTag(tagString));
        }

        /// <summary>
        /// 태그 제거 (카운트 지원)
        /// </summary>
        public bool RemoveTag(GameplayTag tag)
        {
            if (tag == null || !tag.IsValid) return false;

            // 태그 카운트 관리
            if (tagCountMap.ContainsKey(tag))
            {
                tagCountMap[tag]--;

                if (tagCountMap[tag] <= 0)
                {
                    tagCountMap.Remove(tag);
                    return Container.RemoveTag(tag);
                }
                else
                {
                    if (debugMode)
                    {
                        Debug.Log($"[GAS] Tag count decreased: {tag} x{tagCountMap[tag]} on {name}");
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 여러 태그 제거
        /// </summary>
        public void RemoveTags(params string[] tagStrings)
        {
            foreach (var tagString in tagStrings)
            {
                RemoveTag(tagString);
            }
        }

        /// <summary>
        /// 모든 태그 제거
        /// </summary>
        public void ClearTags()
        {
            Container.Clear();
            tagCountMap.Clear();
        }

        /// <summary>
        /// 태그 토글
        /// </summary>
        public void ToggleTag(string tagString)
        {
            var tag = new GameplayTag(tagString);
            if (HasTag(tag))
            {
                RemoveTag(tag);
            }
            else
            {
                AddTag(tag);
            }
        }
        #endregion

        #region Tag Queries
        /// <summary>
        /// 태그 보유 확인
        /// </summary>
        public bool HasTag(string tagString)
        {
            return Container.HasTag(tagString);
        }

        /// <summary>
        /// 태그 보유 확인
        /// </summary>
        public bool HasTag(GameplayTag tag)
        {
            return Container.HasTag(tag);
        }

        /// <summary>
        /// 정확한 태그 일치 확인
        /// </summary>
        public bool HasTagExact(GameplayTag tag)
        {
            return Container.HasTagExact(tag);
        }

        /// <summary>
        /// 여러 태그 중 하나라도 보유 확인
        /// </summary>
        public bool HasAny(List<GameplayTag> tags)
        {
            return Container.HasAny(tags);
        }

        /// <summary>
        /// 여러 태그 모두 보유 확인
        /// </summary>
        public bool HasAll(List<GameplayTag> tags)
        {
            return Container.HasAll(tags);
        }

        /// <summary>
        /// 태그 요구사항 충족 확인
        /// </summary>
        public bool SatisfiesRequirement(TagRequirement requirement)
        {
            return requirement != null && requirement.IsSatisfiedBy(Container);
        }

        /// <summary>
        /// 태그 개수 가져오기
        /// </summary>
        public int GetTagCount(GameplayTag tag)
        {
            return tagCountMap.ContainsKey(tag) ? tagCountMap[tag] : 0;
        }
        #endregion

        #region Event Handlers
        private void HandleTagAdded(GameplayTag tag)
        {
            GASEvents.TriggerTagAdded(gameObject, tag.TagString);

            if (debugMode)
            {
                Debug.Log($"[GAS] Tag added: {tag} to {name}");
            }
        }

        private void HandleTagRemoved(GameplayTag tag)
        {
            GASEvents.TriggerTagRemoved(gameObject, tag.TagString);

            if (debugMode)
            {
                Debug.Log($"[GAS] Tag removed: {tag} from {name}");
            }
        }

        private void HandleContainerChanged()
        {
            GASEvents.TriggerTagsChanged(gameObject);
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// 태그 리스트 가져오기
        /// </summary>
        public List<GameplayTag> GetAllTags()
        {
            return new List<GameplayTag>(Container.Tags);
        }

        /// <summary>
        /// 태그 문자열 배열 가져오기
        /// </summary>
        public string[] GetTagStrings()
        {
            return Container.ToStringArray();
        }

        /// <summary>
        /// 태그 정보 출력
        /// </summary>
        public void PrintTags()
        {
            Debug.Log($"[GAS] Tags on {name}: {Container}");
        }
        #endregion

        #region Static Helper Methods
        /// <summary>
        /// GameObject에서 TagComponent 가져오기 (없으면 추가)
        /// </summary>
        public static TagComponent GetOrAdd(GameObject gameObject)
        {
            if (gameObject == null) return null;

            var component = gameObject.GetComponent<TagComponent>();
            if (component == null)
            {
                component = gameObject.AddComponent<TagComponent>();
            }

            return component;
        }

        /// <summary>
        /// 특정 태그를 가진 모든 GameObject 찾기
        /// </summary>
        public static List<GameObject> FindObjectsWithTag(GameplayTag tag)
        {
            var results = new List<GameObject>();
            var allComponents = FindObjectsByType<TagComponent>(FindObjectsSortMode.None);

            foreach (var component in allComponents)
            {
                if (component.HasTag(tag))
                {
                    results.Add(component.gameObject);
                }
            }

            return results;
        }

        /// <summary>
        /// 태그 요구사항을 만족하는 모든 GameObject 찾기
        /// </summary>
        public static List<GameObject> FindObjectsSatisfyingRequirement(TagRequirement requirement)
        {
            var results = new List<GameObject>();
            var allComponents = FindObjectsByType<TagComponent>(FindObjectsSortMode.None);

            foreach (var component in allComponents)
            {
                if (component.SatisfiesRequirement(requirement))
                {
                    results.Add(component.gameObject);
                }
            }

            return results;
        }
        #endregion
    }
}