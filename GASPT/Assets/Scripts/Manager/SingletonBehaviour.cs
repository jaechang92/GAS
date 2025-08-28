// File: Assets/Scripts/Manager/Core/SingletonBehaviour.cs
// Path: Assets/Scripts/Manager/Core/

using UnityEngine;

namespace NPS2.Manager.Core
{
    /// <summary>
    /// MonoBehaviour 싱글톤 패턴 베이스 클래스
    /// </summary>
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        private static T instance;
        private static readonly object lockObject = new object();
        private static bool applicationIsQuitting = false;

        /// <summary>
        /// 싱글톤 인스턴스
        /// </summary>
        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError($"[Singleton] Something went really wrong - there should never be more than 1 singleton! Reopening the scene might fix it.");
                            return instance;
                        }

                        if (instance == null)
                        {
                            GameObject singleton = new GameObject();
                            instance = singleton.AddComponent<T>();
                            singleton.name = $"(Singleton) {typeof(T).Name}";

                            DontDestroyOnLoad(singleton);

                            Debug.Log($"[Singleton] An instance of {typeof(T)} is needed in the scene, so '{singleton}' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            Debug.Log($"[Singleton] Using instance already created: {instance.gameObject.name}");
                        }
                    }

                    return instance;
                }
            }
        }

        /// <summary>
        /// 인스턴스가 존재하는지 확인
        /// </summary>
        public static bool HasInstance => instance != null;

        /// <summary>
        /// Awake에서 싱글톤 설정
        /// </summary>
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnAwakeInitialization();
            }
            else if (instance != this)
            {
                Debug.LogWarning($"[Singleton] Another instance of {typeof(T)} detected on {gameObject.name}. Destroying it.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 파생 클래스에서 Awake 초기화 로직 구현
        /// </summary>
        protected virtual void OnAwakeInitialization()
        {
            // Override in derived class
        }

        /// <summary>
        /// 애플리케이션 종료 시 플래그 설정
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                applicationIsQuitting = true;
            }
        }

        /// <summary>
        /// 애플리케이션 종료 시 플래그 설정
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }
    }
}