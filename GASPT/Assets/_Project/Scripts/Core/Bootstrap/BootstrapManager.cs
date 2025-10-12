using UnityEngine;
using Core.Managers;
using Core.Enums;

namespace Core.Bootstrap
{
    /// <summary>
    /// 게임 진입점 - 핵심 매니저 초기화 및 Preload 씬으로 전환
    /// Bootstrap 씬은 Build Settings의 첫 번째 씬이어야 함
    /// </summary>
    public class BootstrapManager : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private bool autoStartGame = true;
        [SerializeField] private SceneType initialScene = SceneType.Preload;

        private async void Start()
        {
            Debug.Log("========================================");
            Debug.Log("=== Bootstrap: 게임 시작 ===");
            Debug.Log("========================================");

            // 핵심 매니저 초기화
            await InitializeManagers();

            // 잠시 대기 (초기화 완료 보장)
            await Awaitable.WaitForSecondsAsync(0.1f);

            if (autoStartGame)
            {
                // 초기 씬으로 전환
                await LoadInitialScene();
            }

            Debug.Log("[Bootstrap] 초기화 완료");
        }

        /// <summary>
        /// 핵심 매니저들 초기화 및 DontDestroyOnLoad 설정
        /// </summary>
        private async Awaitable InitializeManagers()
        {
            Debug.Log("[Bootstrap] 매니저 초기화 시작...");

            // 1. SceneLoader 생성
            CreateManager<SceneLoader>("SceneLoader");
            await Awaitable.NextFrameAsync();

            // 2. SceneTransitionManager 생성
            CreateManager<SceneTransitionManager>("SceneTransitionManager");
            await Awaitable.NextFrameAsync();

            // 3. GameFlowManager 생성
            var gameFlowManager = CreateManager<GameFlow.GameFlowManager>("GameFlowManager");
            if (gameFlowManager != null)
            {
                // autoStart를 false로 설정 (Preload 씬 로드 후 수동 시작)
                var autoStartField = typeof(GameFlow.GameFlowManager).GetField("autoStart",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                autoStartField?.SetValue(gameFlowManager, false);
            }
            await Awaitable.NextFrameAsync();

            Debug.Log("[Bootstrap] 매니저 초기화 완료");
        }

        /// <summary>
        /// 매니저 GameObject 생성 및 DontDestroyOnLoad 설정
        /// </summary>
        private T CreateManager<T>(string managerName) where T : MonoBehaviour
        {
            // 이미 존재하는지 확인
            T existingManager = FindAnyObjectByType<T>();
            if (existingManager != null)
            {
                Debug.LogWarning($"[Bootstrap] {managerName}가 이미 존재합니다. 기존 인스턴스 사용.");
                return existingManager;
            }

            // 새로 생성
            GameObject managerGO = new GameObject(managerName);
            T manager = managerGO.AddComponent<T>();

            Debug.Log($"[Bootstrap] {managerName} 생성 완료");

            return manager;
        }

        /// <summary>
        /// 초기 씬 로드
        /// </summary>
        private async Awaitable LoadInitialScene()
        {
            Debug.Log($"[Bootstrap] {initialScene} 씬으로 전환 중...");

            // SceneLoader가 초기화될 때까지 대기
            while (SceneLoader.Instance == null)
            {
                await Awaitable.NextFrameAsync();
            }

            // GameFlowManager가 초기화될 때까지 대기
            while (GameFlow.GameFlowManager.Instance == null)
            {
                await Awaitable.NextFrameAsync();
            }

            // 페이드 아웃 (검은 화면으로 시작)
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.SetFadeOut();
            }

            // 초기 씬 로드
            await SceneLoader.Instance.LoadSceneAsync(initialScene);

            // 프레임 대기 (씬 로드 완료 보장)
            await Awaitable.NextFrameAsync();

            // GameFlowManager를 Preload 상태로 시작
            if (GameFlow.GameFlowManager.Instance != null)
            {
                Debug.Log("[Bootstrap] GameFlowManager를 Preload 상태로 시작");
                GameFlow.GameFlowManager.Instance.StartManually(GameFlow.GameStateType.Preload);
            }

            // 페이드 인
            if (SceneTransitionManager.Instance != null)
            {
                await SceneTransitionManager.Instance.FadeInAsync();
            }

            Debug.Log($"[Bootstrap] {initialScene} 씬 로드 완료");
        }

        #region Context Menu (에디터 테스트용)

#if UNITY_EDITOR
        [ContextMenu("Preload 씬으로 이동")]
        private void LoadPreloadScene()
        {
            _ = LoadInitialScene();
        }

        [ContextMenu("매니저 재초기화")]
        private void ReinitializeManagers()
        {
            _ = InitializeManagers();
        }
#endif

        #endregion
    }
}
