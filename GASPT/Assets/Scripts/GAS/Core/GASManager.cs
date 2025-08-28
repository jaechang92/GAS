// 파일 위치: Assets/Scripts/GAS/Core/GASManager.cs
using NPS2.Manager.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS.Core
{
    /// <summary>
    /// GAS 시스템의 중앙 관리자
    /// 모든 GAS 컴포넌트의 등록 및 관리를 담당
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
        /// GAS 시스템 초기화 완료 이벤트
        /// </summary>
        public static event Action OnSystemInitialized;

        /// <summary>
        /// GAS 컴포넌트 등록 이벤트
        /// </summary>
        public static event Action<GameObject> OnComponentRegistered;

        /// <summary>
        /// GAS 컴포넌트 해제 이벤트
        /// </summary>
        public static event Action<GameObject> OnComponentUnregistered;
        #endregion

        #region Private Fields
        private readonly HashSet<GameObject> registeredObjects = new HashSet<GameObject>();
        private bool isInitialized;
        #endregion

        #region Properties
        /// <summary>
        /// 시스템 초기화 여부
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// 등록된 객체 수
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
        /// GAS 컴포넌트를 가진 GameObject 등록
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
        /// GAS 컴포넌트를 가진 GameObject 해제
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
        /// 특정 타입의 GAS 컴포넌트를 가진 모든 GameObject 검색
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
        /// 시스템 리셋
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

            // 여기에 추가 초기화 로직

            isInitialized = true;
            OnSystemInitialized?.Invoke();

            Debug.Log("[GAS] GAS Manager initialized successfully");
        }
        #endregion
    }
}