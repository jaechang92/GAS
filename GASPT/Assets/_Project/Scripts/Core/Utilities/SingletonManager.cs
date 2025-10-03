using UnityEngine;

namespace Core
{
    /// <summary>
    /// 제네릭 싱글톤 패턴 기본 클래스
    /// MonoBehaviour를 상속받는 Unity 컴포넌트용 싱글톤
    /// </summary>
    /// <typeparam name="T">싱글톤으로 구현할 클래스 타입</typeparam>
    public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object lockObject = new object();
        private static bool applicationIsQuitting = false;

        /// <summary>
        /// 싱글톤 인스턴스 접근자
        /// </summary>
        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[SingletonManager] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindAnyObjectByType<T>();

                        if (instance == null)
                        {
                            GameObject singleton = new GameObject();
                            instance = singleton.AddComponent<T>();
                            singleton.name = $"[SingletonManager] {typeof(T).Name}";

                            DontDestroyOnLoad(singleton);

                            Debug.Log($"[SingletonManager] Created new instance: {typeof(T).Name}");
                        }
                        else
                        {
                            Debug.Log($"[SingletonManager] Using existing instance: {typeof(T).Name}");
                        }
                    }

                    return instance;
                }
            }
        }

        /// <summary>
        /// 싱글톤 인스턴스가 존재하는지 확인
        /// </summary>
        public static bool HasInstance => instance != null && !applicationIsQuitting;

        /// <summary>
        /// 인스턴스를 안전하게 가져오기 (없으면 null 반환)
        /// </summary>
        public static T GetInstanceSafe()
        {
            return HasInstance ? instance : null;
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnAwake();
                OnSingletonAwake();
            }
            else if (instance != this)
            {
                Debug.LogWarning($"[SingletonManager] Duplicate instance detected for {typeof(T).Name}. Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 싱글톤 초기화 시 호출되는 메서드
        /// 상속받은 클래스에서 오버라이드하여 초기화 로직 구현
        /// </summary>
        protected virtual void OnAwake() { }

        /// <summary>
        /// 싱글톤 초기화 시 호출되는 메서드 (매니저들과의 호환성을 위한 별칭)
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// 싱글톤 인스턴스를 강제로 생성
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void EnsureInstance()
        {
            if (!HasInstance)
            {
                var _ = Instance; // 인스턴스 생성 트리거
            }
        }
    }

    /// <summary>
    /// 씬 전환 시 파괴되는 싱글톤 (DontDestroyOnLoad 없음)
    /// </summary>
    /// <typeparam name="T">싱글톤으로 구현할 클래스 타입</typeparam>
    public abstract class SceneSingletonManager<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<T>();

                    if (instance == null)
                    {
                        GameObject singleton = new GameObject();
                        instance = singleton.AddComponent<T>();
                        singleton.name = $"[SceneSingletonManager] {typeof(T).Name}";

                        Debug.Log($"[SceneSingletonManager] Created new instance: {typeof(T).Name}");
                    }
                }

                return instance;
            }
        }

        public static bool HasInstance => instance != null;

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                OnAwake();
                OnSingletonAwake();
            }
            else if (instance != this)
            {
                Debug.LogWarning($"[SceneSingletonManager] Duplicate instance detected for {typeof(T).Name}. Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnAwake() { }

        protected virtual void OnSingletonAwake() { }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
