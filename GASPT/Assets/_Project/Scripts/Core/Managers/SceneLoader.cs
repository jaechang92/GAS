using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Enums;

namespace Core.Managers
{
    /// <summary>
    /// 씬 로드/언로드를 관리하는 매니저
    /// SingletonManager 패턴으로 DontDestroyOnLoad 자동 설정
    /// </summary>
    public class SceneLoader : SingletonManager<SceneLoader>
    {

        [Header("디버그 설정")]
        [SerializeField] private bool showDebugLog = true;

        // 현재 로드된 씬 목록
        private HashSet<SceneType> loadedScenes = new HashSet<SceneType>();

        // 로딩 진행률
        private float currentLoadProgress = 0f;

        // 이벤트
        public event Action<SceneType> OnSceneLoadStarted;
        public event Action<SceneType> OnSceneLoadCompleted;
        public event Action<SceneType> OnSceneUnloadStarted;
        public event Action<SceneType> OnSceneUnloadCompleted;
        public event Action<float> OnLoadProgressChanged;

        protected override void OnSingletonAwake()
        {
            Log("[SceneLoader] 초기화 완료");
        }

        /// <summary>
        /// 씬 로드 (Single 모드 - 기존 씬 언로드)
        /// </summary>
        public async Awaitable LoadSceneAsync(SceneType sceneType, LoadSceneMode mode = LoadSceneMode.Single)
        {
            string sceneName = sceneType.ToString();

            try
            {
                Log($"[SceneLoader] 씬 로드 시작: {sceneName} (Mode: {mode})");

                OnSceneLoadStarted?.Invoke(sceneType);

                // 비동기 로드 시작
                AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);

                if (operation == null)
                {
                    LogError($"[SceneLoader] 씬 로드 실패: {sceneName}");
                    return;
                }

                // 로딩 진행률 추적
                while (!operation.isDone)
                {
                    currentLoadProgress = operation.progress;
                    OnLoadProgressChanged?.Invoke(currentLoadProgress);
                    await Awaitable.NextFrameAsync();
                }

                currentLoadProgress = 1f;
                OnLoadProgressChanged?.Invoke(1f);

                // 씬 로드 완료
                if (mode == LoadSceneMode.Single)
                {
                    loadedScenes.Clear();
                }
                loadedScenes.Add(sceneType);

                OnSceneLoadCompleted?.Invoke(sceneType);
                Log($"[SceneLoader] 씬 로드 완료: {sceneName}");
            }
            catch (Exception ex)
            {
                LogError($"[SceneLoader] 씬 로드 중 예외 발생: {sceneName}, {ex.Message}");
            }
        }

        /// <summary>
        /// 씬 Additive 로드 (기존 씬 유지)
        /// </summary>
        public async Awaitable LoadSceneAdditiveAsync(SceneType sceneType)
        {
            await LoadSceneAsync(sceneType, LoadSceneMode.Additive);
        }

        /// <summary>
        /// 씬 언로드
        /// </summary>
        public async Awaitable UnloadSceneAsync(SceneType sceneType)
        {
            string sceneName = sceneType.ToString();

            try
            {
                // 이미 언로드된 씬인지 확인
                if (!IsSceneLoaded(sceneType))
                {
                    LogWarning($"[SceneLoader] 씬이 이미 언로드됨: {sceneName}");
                    return;
                }

                Log($"[SceneLoader] 씬 언로드 시작: {sceneName}");

                OnSceneUnloadStarted?.Invoke(sceneType);

                // 비동기 언로드 시작
                AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);

                if (operation == null)
                {
                    LogError($"[SceneLoader] 씬 언로드 실패: {sceneName}");
                    return;
                }

                // 언로드 완료 대기
                while (!operation.isDone)
                {
                    await Awaitable.NextFrameAsync();
                }

                loadedScenes.Remove(sceneType);

                OnSceneUnloadCompleted?.Invoke(sceneType);
                Log($"[SceneLoader] 씬 언로드 완료: {sceneName}");
            }
            catch (Exception ex)
            {
                LogError($"[SceneLoader] 씬 언로드 중 예외 발생: {sceneName}, {ex.Message}");
            }
        }

        /// <summary>
        /// 씬 전환 (페이드 효과 포함)
        /// </summary>
        public async Awaitable TransitionToSceneAsync(SceneType fromScene, SceneType toScene)
        {
            Log($"[SceneLoader] 씬 전환: {fromScene} → {toScene}");

            // 페이드 아웃
            if (SceneTransitionManager.Instance != null)
            {
                await SceneTransitionManager.Instance.FadeOutAsync();
            }

            // 씬 전환
            await LoadSceneAsync(toScene, LoadSceneMode.Single);

            // 페이드 인
            if (SceneTransitionManager.Instance != null)
            {
                await SceneTransitionManager.Instance.FadeInAsync();
            }

            Log($"[SceneLoader] 씬 전환 완료: {toScene}");
        }

        /// <summary>
        /// 현재 로딩 진행률 반환 (0.0 ~ 1.0)
        /// </summary>
        public float GetLoadProgress()
        {
            return currentLoadProgress;
        }

        /// <summary>
        /// 씬이 로드되어 있는지 확인
        /// </summary>
        public bool IsSceneLoaded(SceneType sceneType)
        {
            return loadedScenes.Contains(sceneType);
        }

        /// <summary>
        /// 현재 활성화된 씬 반환
        /// </summary>
        public Scene GetActiveScene()
        {
            return SceneManager.GetActiveScene();
        }

        /// <summary>
        /// 씬 이름으로 SceneType 변환
        /// </summary>
        public bool TryGetSceneType(string sceneName, out SceneType sceneType)
        {
            return Enum.TryParse(sceneName, out sceneType);
        }

        #region 로깅

        private void Log(string message)
        {
            if (showDebugLog)
            {
                Debug.Log(message);
            }
        }

        private void LogWarning(string message)
        {
            if (showDebugLog)
            {
                Debug.LogWarning(message);
            }
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
        }

        #endregion

        #region Context Menu (에디터 테스트용)

#if UNITY_EDITOR
        [ContextMenu("Bootstrap 로드")]
        private void LoadBootstrap()
        {
            _ = LoadSceneAsync(SceneType.Bootstrap);
        }

        [ContextMenu("Preload 로드")]
        private void LoadPreload()
        {
            _ = LoadSceneAsync(SceneType.Preload);
        }

        [ContextMenu("Main 로드")]
        private void LoadMain()
        {
            _ = LoadSceneAsync(SceneType.Main);
        }

        [ContextMenu("Gameplay 로드")]
        private void LoadGameplay()
        {
            _ = LoadSceneAsync(SceneType.Gameplay);
        }

        [ContextMenu("로드된 씬 목록 출력")]
        private void PrintLoadedScenes()
        {
            Debug.Log("=== 로드된 씬 목록 ===");
            foreach (var scene in loadedScenes)
            {
                Debug.Log($"  - {scene}");
            }
            Debug.Log($"총 {loadedScenes.Count}개의 씬이 로드됨");
        }
#endif

        #endregion
    }
}
