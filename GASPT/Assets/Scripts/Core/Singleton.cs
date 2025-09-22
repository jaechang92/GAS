using UnityEngine;

namespace Core
{
    /// <summary>
    /// 제네릭 싱글톤 패턴을 제공하는 MonoBehaviour 기반 클래스
    /// 사용법: public class MyManager : Singleton<MyManager>
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T instance;
        private static readonly object lockObject = new object();
        private static bool applicationIsQuitting = false;

        /// <summary>
        /// 싱글톤 인스턴스에 접근하는 프로퍼티
        /// </summary>
        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] 애플리케이션이 종료 중이므로 {typeof(T)} 인스턴스를 반환하지 않습니다.");
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        // 씬에서 기존 인스턴스 찾기
                        instance = FindFirstObjectByType<T>();

                        if (instance == null)
                        {
                            // 새로운 게임오브젝트 생성하여 인스턴스 생성
                            GameObject singletonGO = new GameObject(typeof(T).Name);
                            instance = singletonGO.AddComponent<T>();
                            DontDestroyOnLoad(singletonGO);

                            Debug.Log($"[Singleton] {typeof(T).Name} 인스턴스가 생성되었습니다.");
                        }
                        else
                        {
                            Debug.Log($"[Singleton] 기존 {typeof(T).Name} 인스턴스를 찾았습니다.");
                        }
                    }

                    return instance;
                }
            }
        }

        /// <summary>
        /// 인스턴스가 존재하는지 확인
        /// </summary>
        public static bool HasInstance => instance != null && !applicationIsQuitting;

        /// <summary>
        /// 안전하게 인스턴스를 가져오기 (null 체크 포함)
        /// </summary>
        public static bool TryGetInstance(out T singletonInstance)
        {
            singletonInstance = HasInstance ? Instance : null;
            return singletonInstance != null;
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else if (instance != this)
            {
                Debug.LogWarning($"[Singleton] {typeof(T).Name}의 중복 인스턴스가 감지되어 제거합니다.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                OnSingletonDestroy();
                instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        /// <summary>
        /// 싱글톤이 처음 생성될 때 호출되는 메서드
        /// 하위 클래스에서 오버라이드하여 초기화 로직 구현
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        /// <summary>
        /// 싱글톤이 파괴될 때 호출되는 메서드
        /// 하위 클래스에서 오버라이드하여 정리 로직 구현
        /// </summary>
        protected virtual void OnSingletonDestroy() { }

        /// <summary>
        /// 싱글톤을 수동으로 파괴
        /// </summary>
        public static void DestroySingleton()
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
                instance = null;
            }
        }
    }

    /// <summary>
    /// 씬 전환 시에도 유지되지 않는 싱글톤 클래스
    /// 각 씬마다 새로운 인스턴스가 생성됨
    /// </summary>
    public abstract class SceneSingleton<T> : MonoBehaviour where T : SceneSingleton<T>
    {
        private static T instance;
        private static readonly object lockObject = new object();

        public static T Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindFirstObjectByType<T>();

                        if (instance == null)
                        {
                            GameObject singletonGO = new GameObject(typeof(T).Name);
                            instance = singletonGO.AddComponent<T>();

                            Debug.Log($"[SceneSingleton] {typeof(T).Name} 인스턴스가 생성되었습니다.");
                        }
                    }

                    return instance;
                }
            }
        }

        public static bool HasInstance => instance != null;

        public static bool TryGetInstance(out T singletonInstance)
        {
            singletonInstance = HasInstance ? Instance : null;
            return singletonInstance != null;
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                OnSingletonAwake();
            }
            else if (instance != this)
            {
                Debug.LogWarning($"[SceneSingleton] {typeof(T).Name}의 중복 인스턴스가 감지되어 제거합니다.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                OnSingletonDestroy();
                instance = null;
            }
        }

        protected virtual void OnSingletonAwake() { }
        protected virtual void OnSingletonDestroy() { }
    }
}