using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GASPT.Core.SceneManagement
{
    /// <summary>
    /// Additive Scene Loading 기반 씬 관리 시스템
    ///
    /// 씬 구조:
    /// - Bootstrap: 진입점 (Index 0), 즉시 PersistentManagers 로드 후 비활성화
    /// - PersistentManagers: UI, Manager들 (DontDestroyOnLoad)
    /// - Content Scenes: StartRoom, GameplayScene 등 (교체됨)
    /// </summary>
    public class AdditiveSceneLoader : SingletonManager<AdditiveSceneLoader>
    {
        [Header("씬 설정")]
        [SerializeField] private string persistentManagersSceneName = "PersistentManagers";
        [SerializeField] private string defaultContentSceneName = "StartRoom";

        [Header("현재 상태")]
        [SerializeField] private string currentContentScene = "";
        [SerializeField] private bool isPersistentManagersLoaded = false;

        // 씬 전환 이벤트
        public event System.Action<string> OnContentSceneLoadStarted;
        public event System.Action<string> OnContentSceneLoadCompleted;
        public event System.Action<string> OnContentSceneUnloadStarted;
        public event System.Action<string> OnContentSceneUnloadCompleted;

        // 프로퍼티
        public string CurrentContentScene => currentContentScene;
        public bool IsPersistentManagersLoaded => isPersistentManagersLoaded;
        public bool IsLoading { get; private set; }


        // ====== 초기화 ======

        protected override void OnAwake()
        {
        }

        /// <summary>
        /// Bootstrap에서 호출: PersistentManagers 로드 후 기본 Content Scene 로드
        /// </summary>
        public async Awaitable InitializeFromBootstrap(CancellationToken cancellationToken = default)
        {
            // 1. PersistentManagers Scene 로드
            await LoadPersistentManagersAsync(cancellationToken);

            // 2. 기본 Content Scene 로드 (StartRoom)
            await LoadContentSceneAsync(defaultContentSceneName, cancellationToken);
        }


        // ====== PersistentManagers Scene ======

        /// <summary>
        /// PersistentManagers Scene을 Additive로 로드
        /// </summary>
        public async Awaitable LoadPersistentManagersAsync(CancellationToken cancellationToken = default)
        {
            if (isPersistentManagersLoaded)
            {
                Debug.LogWarning("[AdditiveSceneLoader] PersistentManagers 이미 로드됨");
                return;
            }

            // Additive 모드로 로드
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
                persistentManagersSceneName,
                LoadSceneMode.Additive
            );

            if (loadOperation == null)
            {
                Debug.LogError($"[AdditiveSceneLoader] {persistentManagersSceneName} 로드 실패! Build Settings 확인 필요");
                return;
            }

            while (!loadOperation.isDone)
            {
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            isPersistentManagersLoaded = true;
        }


        // ====== Content Scene (교체 방식) ======

        /// <summary>
        /// Content Scene 전환 (기존 Unload → 새로운 Load)
        /// </summary>
        public async Awaitable SwitchContentSceneAsync(
            string newSceneName,
            CancellationToken cancellationToken = default)
        {
            if (IsLoading)
            {
                Debug.LogWarning("[AdditiveSceneLoader] 이미 로딩 중입니다.");
                return;
            }

            IsLoading = true;

            try
            {
                // 기존 Content Scene 언로드
                if (!string.IsNullOrEmpty(currentContentScene))
                {
                    await UnloadContentSceneAsync(currentContentScene, cancellationToken);
                }

                // 새로운 Content Scene 로드
                await LoadContentSceneAsync(newSceneName, cancellationToken);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Content Scene 로드 (Additive)
        /// </summary>
        public async Awaitable LoadContentSceneAsync(
            string sceneName,
            CancellationToken cancellationToken = default)
        {
            OnContentSceneLoadStarted?.Invoke(sceneName);

            // 씬이 이미 로드되어 있는지 확인
            Scene existingScene = SceneManager.GetSceneByName(sceneName);
            if (existingScene.isLoaded)
            {
                Debug.LogWarning($"[AdditiveSceneLoader] '{sceneName}'은 이미 로드됨");
                currentContentScene = sceneName;
                SetActiveContentScene(sceneName);
                OnContentSceneLoadCompleted?.Invoke(sceneName);
                return;
            }

            // Additive 모드로 로드
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Additive
            );

            if (loadOperation == null)
            {
                Debug.LogError($"[AdditiveSceneLoader] '{sceneName}' 로드 실패! Build Settings 확인 필요");
                OnContentSceneLoadCompleted?.Invoke(sceneName);
                return;
            }

            // 로딩 완료 대기
            while (!loadOperation.isDone)
            {
                // 진행도 로깅 (필요시)
                // Debug.Log($"[AdditiveSceneLoader] 로딩 진행도: {loadOperation.progress * 100:F0}%");
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            currentContentScene = sceneName;

            // Active Scene 설정 (라이팅, 물리 등 적용)
            SetActiveContentScene(sceneName);

            OnContentSceneLoadCompleted?.Invoke(sceneName);
        }

        /// <summary>
        /// Content Scene 언로드
        /// </summary>
        public async Awaitable UnloadContentSceneAsync(
            string sceneName,
            CancellationToken cancellationToken = default)
        {
            OnContentSceneUnloadStarted?.Invoke(sceneName);

            // 씬이 로드되어 있는지 확인
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded)
            {
                Debug.LogWarning($"[AdditiveSceneLoader] '{sceneName}'은 로드되어 있지 않음");
                OnContentSceneUnloadCompleted?.Invoke(sceneName);
                return;
            }

            // 언로드
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(sceneName);

            if (unloadOperation == null)
            {
                Debug.LogError($"[AdditiveSceneLoader] '{sceneName}' 언로드 실패!");
                OnContentSceneUnloadCompleted?.Invoke(sceneName);
                return;
            }

            while (!unloadOperation.isDone)
            {
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            // 현재 Content Scene이면 클리어
            if (currentContentScene == sceneName)
            {
                currentContentScene = "";
            }

            // 메모리 정리
            await Resources.UnloadUnusedAssets();

            OnContentSceneUnloadCompleted?.Invoke(sceneName);
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// Active Scene 설정 (라이팅, Physics 등 적용)
        /// </summary>
        private void SetActiveContentScene(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                SceneManager.SetActiveScene(scene);
            }
        }

        /// <summary>
        /// 특정 씬이 로드되어 있는지 확인
        /// </summary>
        public bool IsSceneLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.isLoaded;
        }

        /// <summary>
        /// 현재 로드된 모든 씬 목록 반환
        /// </summary>
        public string[] GetLoadedSceneNames()
        {
            int sceneCount = SceneManager.sceneCount;
            string[] sceneNames = new string[sceneCount];

            for (int i = 0; i < sceneCount; i++)
            {
                sceneNames[i] = SceneManager.GetSceneAt(i).name;
            }

            return sceneNames;
        }


        // ====== 디버그 ======

        [ContextMenu("현재 로드된 씬 출력")]
        private void DebugPrintLoadedScenes()
        {
            Debug.Log("========== 현재 로드된 씬 ==========");
            string[] scenes = GetLoadedSceneNames();
            for (int i = 0; i < scenes.Length; i++)
            {
                Scene scene = SceneManager.GetSceneByName(scenes[i]);
                string activeMarker = scene == SceneManager.GetActiveScene() ? " [ACTIVE]" : "";
                Debug.Log($"  {i + 1}. {scenes[i]}{activeMarker}");
            }
            Debug.Log($"Current Content Scene: {currentContentScene}");
            Debug.Log($"PersistentManagers Loaded: {isPersistentManagersLoaded}");
            Debug.Log("====================================");
        }
    }
}
