using UnityEngine;
using System.Collections.Generic;

namespace Core.Managers
{
    /// <summary>
    /// 리소스 관리 매니저
    /// 게임에 필요한 모든 리소스를 중앙에서 로드하고 관리
    /// 제네릭 방식으로 순환 참조 방지
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        private static ResourceManager instance;
        public static ResourceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    // 씬에서 찾기
                    instance = FindFirstObjectByType<ResourceManager>();

                    // 없으면 생성
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ResourceManager");
                        instance = go.AddComponent<ResourceManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("로드 상태")]
        [SerializeField] private bool isInitialized = false;

        // 리소스 캐시 (경로별로 저장)
        private Dictionary<string, ScriptableObject> resourceCache = new Dictionary<string, ScriptableObject>();

        #region 프로퍼티

        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        public bool IsInitialized => isInitialized;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            // 싱글톤 패턴
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 리소스 매니저 초기화
        /// </summary>
        [ContextMenu("Initialize Resources")]
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[ResourceManager] 이미 초기화되었습니다.");
                return;
            }

            Debug.Log("[ResourceManager] 리소스 로드 시작...");

            isInitialized = true;
            Debug.Log("[ResourceManager] 리소스 로드 완료!");
        }

        #endregion

        #region 제네릭 리소스 로드

        /// <summary>
        /// 제네릭 리소스 로드 (캐시 사용)
        /// </summary>
        public T Load<T>(string path) where T : ScriptableObject
        {
            // 캐시 확인
            if (resourceCache.ContainsKey(path))
            {
                return resourceCache[path] as T;
            }

            // Resources 폴더에서 로드
            T resource = Resources.Load<T>(path);

            if (resource != null)
            {
                resourceCache[path] = resource;
                Debug.Log($"[ResourceManager] {typeof(T).Name} 로드 성공: {path}");
                return resource;
            }
            else
            {
                Debug.LogError($"[ResourceManager] {typeof(T).Name} 로드 실패: {path}");
                Debug.LogError($"[ResourceManager] 파일이 Assets/_Project/Resources/{path}.asset 경로에 있는지 확인하세요!");
                return null;
            }
        }

        /// <summary>
        /// 제네릭 리소스 로드 with 폴백
        /// </summary>
        public T LoadWithFallback<T>(string path) where T : ScriptableObject
        {
            T resource = Load<T>(path);

            if (resource == null)
            {
                Debug.LogWarning($"[ResourceManager] {typeof(T).Name} 로드 실패. 기본 인스턴스 생성.");
                resource = ScriptableObject.CreateInstance<T>();
                resourceCache[path] = resource;
            }

            return resource;
        }

        /// <summary>
        /// 정적 메서드: 제네릭 리소스 로드
        /// </summary>
        public static T GetResource<T>(string path) where T : ScriptableObject
        {
            return Instance.Load<T>(path);
        }

        /// <summary>
        /// 정적 메서드: 제네릭 리소스 로드 with 폴백
        /// </summary>
        public static T GetResourceWithFallback<T>(string path) where T : ScriptableObject
        {
            return Instance.LoadWithFallback<T>(path);
        }

        #endregion

        #region 디버그

        /// <summary>
        /// 리소스 상태 출력
        /// </summary>
        [ContextMenu("Print Resource Status")]
        public void PrintResourceStatus()
        {
            Debug.Log("=== ResourceManager 상태 ===");
            Debug.Log($"초기화 완료: {isInitialized}");
            Debug.Log($"캐시된 리소스 수: {resourceCache.Count}");

            foreach (var kvp in resourceCache)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value?.GetType().Name ?? "null"}");
            }
        }

        /// <summary>
        /// 리소스 재로드
        /// </summary>
        [ContextMenu("Reload Resources")]
        public void ReloadResources()
        {
            Debug.Log("[ResourceManager] 리소스 재로드 시작...");

            resourceCache.Clear();
            isInitialized = false;

            Initialize();
        }

        /// <summary>
        /// 캐시 클리어
        /// </summary>
        [ContextMenu("Clear Cache")]
        public void ClearCache()
        {
            resourceCache.Clear();
            Debug.Log("[ResourceManager] 캐시 클리어 완료");
        }

        #endregion
    }
}
