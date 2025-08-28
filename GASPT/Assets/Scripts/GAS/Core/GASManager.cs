// ���� ��ġ: Assets/Scripts/GAS/Core/GASManager.cs
using NPS2.Manager.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS.Core
{
    /// <summary>
    /// GAS �ý����� �߾� ������
    /// ��� GAS ������Ʈ�� ��� �� ������ ���
    /// </summary>
    public class GASManager : SingletonManager<GASManager>
    {
        protected override void OnCreated()
        {
            base.OnCreated();
            gameObject.name = "[GAS Manager]";
        }

        #region Events
        /// <summary>
        /// GAS �ý��� �ʱ�ȭ �Ϸ� �̺�Ʈ
        /// </summary>
        public static event Action OnSystemInitialized;

        /// <summary>
        /// GAS ������Ʈ ��� �̺�Ʈ
        /// </summary>
        public static event Action<GameObject> OnComponentRegistered;

        /// <summary>
        /// GAS ������Ʈ ���� �̺�Ʈ
        /// </summary>
        public static event Action<GameObject> OnComponentUnregistered;
        #endregion

        #region Private Fields
        private readonly HashSet<GameObject> registeredObjects = new HashSet<GameObject>();
        private bool isInitialized;
        #endregion

        #region Properties
        /// <summary>
        /// �ý��� �ʱ�ȭ ����
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// ��ϵ� ��ü ��
        /// </summary>
        public int RegisteredObjectCount => registeredObjects.Count;
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
            InitializeSystem();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// GAS ������Ʈ�� ���� GameObject ���
        /// </summary>
        public void RegisterObject(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[GAS] Cannot register null object");
                return;
            }

            if (registeredObjects.Add(obj))
            {
                Debug.Log($"[GAS] Registered object: {obj.name}");
                OnComponentRegistered?.Invoke(obj);
            }
        }

        /// <summary>
        /// GAS ������Ʈ�� ���� GameObject ����
        /// </summary>
        public void UnregisterObject(GameObject obj)
        {
            if (obj == null)
                return;

            if (registeredObjects.Remove(obj))
            {
                Debug.Log($"[GAS] Unregistered object: {obj.name}");
                OnComponentUnregistered?.Invoke(obj);
            }
        }

        /// <summary>
        /// Ư�� Ÿ���� GAS ������Ʈ�� ���� ��� GameObject �˻�
        /// </summary>
        public List<T> GetComponentsOfType<T>() where T : Component
        {
            List<T> components = new List<T>();

            foreach (var obj in registeredObjects)
            {
                if (obj != null)
                {
                    T component = obj.GetComponent<T>();
                    if (component != null)
                    {
                        components.Add(component);
                    }
                }
            }

            return components;
        }

        /// <summary>
        /// �ý��� ����
        /// </summary>
        public void ResetSystem()
        {
            registeredObjects.Clear();
            Debug.Log("[GAS] System reset completed");
        }
        #endregion

        #region Private Methods
        private void InitializeSystem()
        {
            if (isInitialized)
                return;

            Debug.Log("[GAS] Initializing GAS Manager...");

            // ���⿡ �߰� �ʱ�ȭ ����

            isInitialized = true;
            OnSystemInitialized?.Invoke();

            Debug.Log("[GAS] GAS Manager initialized successfully");
        }
        #endregion
    }
}