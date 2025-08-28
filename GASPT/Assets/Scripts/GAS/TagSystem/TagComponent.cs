// ���� ��ġ: Assets/Scripts/GAS/TagSystem/TagComponent.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.Core;

namespace GAS.TagSystem
{
    /// <summary>
    /// GameObject�� �±� �ý����� �ο��ϴ� ������Ʈ
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
        /// �±� �����̳�
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
        /// ���� �±� ����
        /// </summary>
        public int TagCount => Container.Count;

        /// <summary>
        /// �±װ� �ִ��� Ȯ��
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
            // GASManager�� ���
            GASManager.Instance.RegisterObject(gameObject);

            // �ʱ� �±� ����
            ApplyInitialTags();
        }

        private void OnDestroy()
        {
            // GASManager���� ����
            if (GASManager.Instance != null)
            {
                GASManager.Instance.UnregisterObject(gameObject);
            }

            // �̺�Ʈ ����
            if (container != null)
            {
                container.OnTagAdded -= HandleTagAdded;
                container.OnTagRemoved -= HandleTagRemoved;
                container.OnContainerChanged -= HandleContainerChanged;
            }
        }

        private void OnValidate()
        {
            // Inspector���� �±� ���� �� ��ȿ�� �˻�
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

            // �̺�Ʈ ����
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
        /// �±� �߰�
        /// </summary>
        public bool AddTag(string tagString)
        {
            return AddTag(new GameplayTag(tagString));
        }

        /// <summary>
        /// �±� �߰� (ī��Ʈ ����)
        /// </summary>
        public bool AddTag(GameplayTag tag)
        {
            if (tag == null || !tag.IsValid) return false;

            // �±� ī��Ʈ ����
            if (tagCountMap.ContainsKey(tag))
            {
                tagCountMap[tag]++;
                if (debugMode)
                {
                    Debug.Log($"[GAS] Tag count increased: {tag} x{tagCountMap[tag]} on {name}");
                }
                return false; // �̹� �ִ� �±�
            }

            // �� �±� �߰�
            if (Container.AddTag(tag))
            {
                tagCountMap[tag] = 1;
                return true;
            }

            return false;
        }

        /// <summary>
        /// ���� �±� �߰�
        /// </summary>
        public void AddTags(params string[] tagStrings)
        {
            foreach (var tagString in tagStrings)
            {
                AddTag(tagString);
            }
        }

        /// <summary>
        /// �±� ����
        /// </summary>
        public bool RemoveTag(string tagString)
        {
            return RemoveTag(new GameplayTag(tagString));
        }

        /// <summary>
        /// �±� ���� (ī��Ʈ ����)
        /// </summary>
        public bool RemoveTag(GameplayTag tag)
        {
            if (tag == null || !tag.IsValid) return false;

            // �±� ī��Ʈ ����
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
        /// ���� �±� ����
        /// </summary>
        public void RemoveTags(params string[] tagStrings)
        {
            foreach (var tagString in tagStrings)
            {
                RemoveTag(tagString);
            }
        }

        /// <summary>
        /// ��� �±� ����
        /// </summary>
        public void ClearTags()
        {
            Container.Clear();
            tagCountMap.Clear();
        }

        /// <summary>
        /// �±� ���
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
        /// �±� ���� Ȯ��
        /// </summary>
        public bool HasTag(string tagString)
        {
            return Container.HasTag(tagString);
        }

        /// <summary>
        /// �±� ���� Ȯ��
        /// </summary>
        public bool HasTag(GameplayTag tag)
        {
            return Container.HasTag(tag);
        }

        /// <summary>
        /// ��Ȯ�� �±� ��ġ Ȯ��
        /// </summary>
        public bool HasTagExact(GameplayTag tag)
        {
            return Container.HasTagExact(tag);
        }

        /// <summary>
        /// ���� �±� �� �ϳ��� ���� Ȯ��
        /// </summary>
        public bool HasAny(List<GameplayTag> tags)
        {
            return Container.HasAny(tags);
        }

        /// <summary>
        /// ���� �±� ��� ���� Ȯ��
        /// </summary>
        public bool HasAll(List<GameplayTag> tags)
        {
            return Container.HasAll(tags);
        }

        /// <summary>
        /// �±� �䱸���� ���� Ȯ��
        /// </summary>
        public bool SatisfiesRequirement(TagRequirement requirement)
        {
            return requirement != null && requirement.IsSatisfiedBy(Container);
        }

        /// <summary>
        /// �±� ���� ��������
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
        /// �±� ����Ʈ ��������
        /// </summary>
        public List<GameplayTag> GetAllTags()
        {
            return new List<GameplayTag>(Container.Tags);
        }

        /// <summary>
        /// �±� ���ڿ� �迭 ��������
        /// </summary>
        public string[] GetTagStrings()
        {
            return Container.ToStringArray();
        }

        /// <summary>
        /// �±� ���� ���
        /// </summary>
        public void PrintTags()
        {
            Debug.Log($"[GAS] Tags on {name}: {Container}");
        }
        #endregion

        #region Static Helper Methods
        /// <summary>
        /// GameObject���� TagComponent �������� (������ �߰�)
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
        /// Ư�� �±׸� ���� ��� GameObject ã��
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
        /// �±� �䱸������ �����ϴ� ��� GameObject ã��
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