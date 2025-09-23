using UnityEngine;
using Core;

namespace Scenes
{
    /// <summary>
    /// 씬 관리 시스템
    /// 씬 로드, 전환, 상태 관리
    /// </summary>
    public enum SceneType
    {
        Bootstrap = 0,
        MainMenu = 1,
        LevelSelect = 2,
        Gameplay = 3,
        PauseMenu = 4,
        Settings = 5,
        GameOver = 6,
        LevelComplete = 7
    }

    public class SceneManager : Singleton<SceneManager>
    {
        [Header("씬 설정")]
        [SerializeField] private SceneType currentScene = SceneType.Bootstrap;
        [SerializeField] private bool enableTransitionEffects = true;

        // 씬 이름 매핑
        private readonly string[] sceneNames = {
            "00_Bootstrap",
            "01_MainMenu",
            "02_LevelSelect",
            "03_Gameplay",
            "04_PauseMenu",
            "05_Settings",
            "06_GameOver",
            "07_LevelComplete"
        };

        // 이벤트
        public event System.Action<SceneType> OnSceneLoadStarted;
        public event System.Action<SceneType> OnSceneLoadCompleted;
        public event System.Action<SceneType, SceneType> OnSceneTransition;

        // 프로퍼티
        public SceneType CurrentScene => currentScene;
        public bool IsTransitioning { get; private set; }

        protected override void OnSingletonAwake()
        {
            Debug.Log("[SceneManager] 씬 매니저 초기화");

            // 현재 활성 씬 확인
            DetectCurrentScene();
        }

        /// <summary>
        /// 현재 활성 씬 감지
        /// </summary>
        private void DetectCurrentScene()
        {
            string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            for (int i = 0; i < sceneNames.Length; i++)
            {
                if (sceneNames[i] == activeSceneName)
                {
                    currentScene = (SceneType)i;
                    Debug.Log($"[SceneManager] 현재 씬: {currentScene}");
                    return;
                }
            }

            Debug.LogWarning($"[SceneManager] 알 수 없는 씬: {activeSceneName}");
        }

        /// <summary>
        /// 특정 씬으로 전환
        /// </summary>
        public async Awaitable LoadScene(SceneType targetScene)
        {
            if (IsTransitioning)
            {
                Debug.LogWarning("[SceneManager] 이미 씬 전환 중입니다.");
                return;
            }

            if (targetScene == currentScene)
            {
                Debug.LogWarning($"[SceneManager] 이미 {targetScene} 씬에 있습니다.");
                return;
            }

            Debug.Log($"[SceneManager] 씬 전환 시작: {currentScene} → {targetScene}");

            IsTransitioning = true;
            var previousScene = currentScene;

            OnSceneLoadStarted?.Invoke(targetScene);
            OnSceneTransition?.Invoke(previousScene, targetScene);

            // 로딩 화면 표시
            if (enableTransitionEffects)
            {
                Managers.UIManager.Instance.ShowLoadingOverlay(true);
            }

            try
            {
                // 비동기 씬 로드
                await LoadSceneAsync(targetScene);

                currentScene = targetScene;
                OnSceneLoadCompleted?.Invoke(targetScene);

                Debug.Log($"[SceneManager] 씬 전환 완료: {currentScene}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SceneManager] 씬 로드 실패: {e.Message}");
            }
            finally
            {
                // 로딩 화면 숨김
                if (enableTransitionEffects)
                {
                    Managers.UIManager.Instance.ShowLoadingOverlay(false);
                }

                IsTransitioning = false;
            }
        }

        /// <summary>
        /// 비동기 씬 로드
        /// </summary>
        private async Awaitable LoadSceneAsync(SceneType targetScene)
        {
            string sceneName = GetSceneName(targetScene);

            var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

            if (asyncOperation == null)
            {
                throw new System.Exception($"씬을 찾을 수 없습니다: {sceneName}");
            }

            // 로딩 완료까지 대기
            while (!asyncOperation.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
        }

        /// <summary>
        /// 씬 타입을 씬 이름으로 변환
        /// </summary>
        public string GetSceneName(SceneType sceneType)
        {
            int index = (int)sceneType;
            if (index >= 0 && index < sceneNames.Length)
            {
                return sceneNames[index];
            }

            Debug.LogError($"[SceneManager] 잘못된 씬 타입: {sceneType}");
            return sceneNames[0]; // Bootstrap으로 기본값
        }

        /// <summary>
        /// 이전 씬으로 돌아가기 (스택 기반)
        /// </summary>
        public async Awaitable GoToPreviousScene()
        {
            // 간단한 이전 씬 로직 (확장 가능)
            switch (currentScene)
            {
                case SceneType.LevelSelect:
                    await LoadScene(SceneType.MainMenu);
                    break;
                case SceneType.Gameplay:
                    await LoadScene(SceneType.LevelSelect);
                    break;
                case SceneType.Settings:
                    await LoadScene(SceneType.MainMenu);
                    break;
                case SceneType.GameOver:
                case SceneType.LevelComplete:
                    await LoadScene(SceneType.LevelSelect);
                    break;
                default:
                    await LoadScene(SceneType.MainMenu);
                    break;
            }
        }

        /// <summary>
        /// 메인 메뉴로 이동
        /// </summary>
        public async Awaitable GoToMainMenu()
        {
            await LoadScene(SceneType.MainMenu);
        }

        /// <summary>
        /// 게임플레이 씬으로 이동
        /// </summary>
        public async Awaitable StartGameplay()
        {
            await LoadScene(SceneType.Gameplay);
        }

        /// <summary>
        /// 설정 씬으로 이동
        /// </summary>
        public async Awaitable OpenSettings()
        {
            await LoadScene(SceneType.Settings);
        }

        // TODO: 차후 구현 예정
        // - 씬 히스토리 스택 관리
        // - 씬 전환 애니메이션 효과
        // - 씬별 리소스 프리로딩
        // - 메모리 관리 및 최적화
        // - 씬 전환 시 데이터 전달
    }
}