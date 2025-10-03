using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using Core.Enums;
using Core.Data;

namespace Core.Managers
{
    /// <summary>
    /// 리소스 관리 매니저
    /// 게임에 필요한 모든 리소스를 카테고리별로 중앙에서 로드하고 관리
    /// </summary>
    public class ResourceManager : SingletonManager<ResourceManager>
    {
        [Header("리소스 매니페스트")]
        [Tooltip("Resources/Manifests/ 폴더에서 자동으로 모든 매니페스트를 로드합니다")]
        [SerializeField] private List<ResourceManifest> manifests = new List<ResourceManifest>();

        [Header("로드 상태")]
        [SerializeField] private bool isInitialized = false;

        // 리소스 캐시
        private Dictionary<string, UnityEngine.Object> resourceCache = new Dictionary<string, UnityEngine.Object>();

        // 카테고리별 로드 상태
        private HashSet<ResourceCategory> loadedCategories = new HashSet<ResourceCategory>();

        // 현재 로딩 진행률
        private float currentLoadProgress = 0f;

        #region 이벤트

        /// <summary>
        /// 카테고리 로드 진행률 이벤트 (카테고리, 진행률 0~1, 현재 리소스 이름)
        /// </summary>
        public event Action<ResourceCategory, float, string> OnLoadProgress;

        /// <summary>
        /// 카테고리 로드 완료 이벤트
        /// </summary>
        public event Action<ResourceCategory> OnCategoryLoaded;

        /// <summary>
        /// 전체 로드 완료 이벤트
        /// </summary>
        public event Action OnAllLoaded;

        #endregion

        #region 프로퍼티

        public bool IsInitialized => isInitialized;
        public float CurrentLoadProgress => currentLoadProgress;

        #endregion

        #region Unity 생명주기

        protected override void Awake()
        {
            base.Awake();
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 매니페스트 초기화 - Resources/Manifests 폴더에서 자동 로드
        /// </summary>
        private void InitializeManifests()
        {
            ResourceManifest[] loadedManifests = Resources.LoadAll<ResourceManifest>("Manifests");

            if (loadedManifests != null && loadedManifests.Length > 0)
            {
                manifests.AddRange(loadedManifests);
                Debug.Log($"[ResourceManager] {loadedManifests.Length}개의 매니페스트 로드 완료");
            }
            else
            {
                Debug.LogWarning("[ResourceManager] Resources/Manifests 폴더에 매니페스트가 없습니다.");
            }
        }

        #endregion

        #region 카테고리별 로딩

        /// <summary>
        /// 특정 카테고리의 리소스를 비동기로 로드
        /// </summary>
        public async Awaitable<bool> LoadCategoryAsync(ResourceCategory category, CancellationToken cancellationToken = default)
        {
            // 이미 로드된 카테고리는 스킵
            if (loadedCategories.Contains(category))
            {
                Debug.Log($"[ResourceManager] {category} 카테고리는 이미 로드되었습니다.");
                return true;
            }

            // 해당 카테고리의 매니페스트 찾기
            ResourceManifest manifest = manifests.Find(m => m.category == category);

            if (manifest == null)
            {
                Debug.LogWarning($"[ResourceManager] {category} 카테고리의 매니페스트를 찾을 수 없습니다.");
                return false;
            }

            Debug.Log($"[ResourceManager] {category} 카테고리 로딩 시작... ({manifest.ResourceCount}개 리소스)");

            int totalResources = manifest.ResourceCount;
            int loadedCount = 0;

            // 리소스 로드
            foreach (var entry in manifest.resources)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.LogWarning($"[ResourceManager] {category} 로딩이 취소되었습니다.");
                    return false;
                }

                // 리소스 로드
                bool success = await LoadResourceAsync(entry, cancellationToken);

                loadedCount++;
                float progress = (float)loadedCount / totalResources;
                currentLoadProgress = progress;

                // 진행률 이벤트 발생
                OnLoadProgress?.Invoke(category, progress, entry.path);

                // 프레임 양보 (UI 업데이트를 위해)
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            // 최소 로딩 시간 시뮬레이션 (테스트용)
            if (manifest.minimumLoadTime > 0f)
            {
                await Awaitable.WaitForSecondsAsync(manifest.minimumLoadTime, cancellationToken);
            }

            // 카테고리 로드 완료
            loadedCategories.Add(category);
            OnCategoryLoaded?.Invoke(category);

            Debug.Log($"[ResourceManager] {category} 카테고리 로딩 완료!");
            return true;
        }

        /// <summary>
        /// 여러 카테고리를 순차적으로 로드
        /// </summary>
        public async Awaitable<bool> LoadCategoriesAsync(ResourceCategory[] categories, CancellationToken cancellationToken = default)
        {
            foreach (var category in categories)
            {
                bool success = await LoadCategoryAsync(category, cancellationToken);
                if (!success)
                {
                    return false;
                }
            }

            OnAllLoaded?.Invoke();
            return true;
        }

        /// <summary>
        /// 개별 리소스를 비동기로 로드
        /// </summary>
        private async Awaitable<bool> LoadResourceAsync(ResourceEntry entry, CancellationToken cancellationToken = default)
        {
            // 이미 캐시에 있으면 스킵
            if (resourceCache.ContainsKey(entry.path))
            {
                return true;
            }

            try
            {
                // 타입 이름에서 실제 타입 가져오기
                Type resourceType = GetTypeFromName(entry.typeName);

                if (resourceType == null)
                {
                    Debug.LogError($"[ResourceManager] 타입을 찾을 수 없습니다: {entry.typeName}");
                    return false;
                }

                // Resources.Load는 동기이지만, 프레임 양보를 통해 비동기처럼 처리
                UnityEngine.Object resource = Resources.Load(entry.path, resourceType);

                if (resource != null)
                {
                    resourceCache[entry.path] = resource;
                    Debug.Log($"[ResourceManager] 로드 성공: {entry.path} ({entry.typeName})");
                    return true;
                }
                else
                {
                    Debug.LogError($"[ResourceManager] 로드 실패: {entry.path}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ResourceManager] 로드 중 오류: {entry.path}\n{e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 타입 이름에서 실제 Type 객체 가져오기
        /// </summary>
        private Type GetTypeFromName(string typeName)
        {
            // UnityEngine 타입
            Type type = Type.GetType($"UnityEngine.{typeName}, UnityEngine");
            if (type != null) return type;

            // ScriptableObject 타입 (현재 어셈블리)
            type = Type.GetType(typeName);
            if (type != null) return type;

            // 전체 어셈블리 검색
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null) return type;
            }

            return null;
        }

        #endregion

        #region 리소스 언로드

        /// <summary>
        /// 특정 카테고리의 리소스 언로드
        /// </summary>
        public void UnloadCategory(ResourceCategory category)
        {
            ResourceManifest manifest = manifests.Find(m => m.category == category);
            if (manifest == null) return;

            foreach (var entry in manifest.resources)
            {
                if (resourceCache.ContainsKey(entry.path))
                {
                    resourceCache.Remove(entry.path);
                }
            }

            loadedCategories.Remove(category);
            Debug.Log($"[ResourceManager] {category} 카테고리 언로드 완료");
        }

        /// <summary>
        /// 모든 리소스 언로드
        /// </summary>
        public void UnloadAll()
        {
            resourceCache.Clear();
            loadedCategories.Clear();
            Resources.UnloadUnusedAssets();
            Debug.Log("[ResourceManager] 모든 리소스 언로드 완료");
        }

        #endregion

        #region 제네릭 리소스 로드 (기존 호환성)

        /// <summary>
        /// 제네릭 리소스 로드 (캐시 사용)
        /// </summary>
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            if (resourceCache.ContainsKey(path))
            {
                return resourceCache[path] as T;
            }

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
                return null;
            }
        }

        /// <summary>
        /// 제네릭 리소스 로드 with 폴백 (ScriptableObject 전용)
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

        public static T GetResource<T>(string path) where T : UnityEngine.Object
        {
            return Instance.Load<T>(path);
        }

        public static T GetResourceWithFallback<T>(string path) where T : ScriptableObject
        {
            return Instance.LoadWithFallback<T>(path);
        }

        #endregion

        #region 카테고리 상태 확인

        /// <summary>
        /// 특정 카테고리가 로드되었는지 확인
        /// </summary>
        public bool IsCategoryLoaded(ResourceCategory category)
        {
            return loadedCategories.Contains(category);
        }

        /// <summary>
        /// 로드된 카테고리 목록
        /// </summary>
        public ResourceCategory[] GetLoadedCategories()
        {
            ResourceCategory[] categories = new ResourceCategory[loadedCategories.Count];
            loadedCategories.CopyTo(categories);
            return categories;
        }

        #endregion

        #region 디버그

        [ContextMenu("Print Resource Status")]
        public void PrintResourceStatus()
        {
            Debug.Log("=== ResourceManager 상태 ===");
            Debug.Log($"초기화 완료: {isInitialized}");
            Debug.Log($"매니페스트 수: {manifests.Count}");
            Debug.Log($"로드된 카테고리: {string.Join(", ", loadedCategories)}");
            Debug.Log($"캐시된 리소스 수: {resourceCache.Count}");
        }

        [ContextMenu("Clear Cache")]
        public void ClearCache()
        {
            resourceCache.Clear();
            loadedCategories.Clear();
            Debug.Log("[ResourceManager] 캐시 클리어 완료");
        }

        #endregion
    }
}
