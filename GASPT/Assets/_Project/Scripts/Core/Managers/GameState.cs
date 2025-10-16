using System.Threading;
using UnityEngine;
using FSM.Core;

namespace GameFlow
{
    /// <summary>
    /// 게임 상태 기본 클래스
    /// </summary>
    public abstract class GameState : IState
    {
        protected GameStateType stateType;
        protected GameFlowManager gameFlowManager;

        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        public GameObject Owner { get; private set; }
        public IStateMachine StateMachine { get; private set; }

        // 이벤트
        public event System.Action<IState> OnEntered;
        public event System.Action<IState> OnExited;

        protected GameState(GameStateType type)
        {
            stateType = type;
            Id = type.ToString();
        }

        public virtual void Initialize(string id, GameObject owner, IStateMachine stateMachine)
        {
            Id = id;
            Name = stateType.ToString();
            Owner = owner;
            StateMachine = stateMachine;
            gameFlowManager = owner.GetComponent<GameFlowManager>();
        }

        // === 동기 메서드 (기본 구현) ===
        public virtual void OnEnterSync()
        {
            Debug.Log($"[GameFlow] Entering {stateType} state (sync)");
            IsActive = true;
            EnterStateSync();
            OnEntered?.Invoke(this);
        }

        public virtual void OnExitSync()
        {
            Debug.Log($"[GameFlow] Exiting {stateType} state (sync)");
            IsActive = false;
            ExitStateSync();
            OnExited?.Invoke(this);
        }

        // === 비동기 메서드 (GameFlow 주 사용) ===
        public virtual async Awaitable OnEnter(CancellationToken cancellationToken = default)
        {
            Debug.Log($"[GameFlow] Entering {stateType} state (async)");
            IsActive = true;
            await EnterState(cancellationToken);
            OnEntered?.Invoke(this);
        }

        public virtual async Awaitable OnExit(CancellationToken cancellationToken = default)
        {
            Debug.Log($"[GameFlow] Exiting {stateType} state (async)");
            IsActive = false;
            await ExitState(cancellationToken);
            OnExited?.Invoke(this);
        }

        public virtual void OnUpdate(float deltaTime)
        {
            UpdateState(deltaTime);
        }

        // === 하위 클래스에서 구현할 메서드들 ===
        // 동기 메서드 (기본 구현 제공)
        protected virtual void EnterStateSync() { }
        protected virtual void ExitStateSync() { }

        // 비동기 메서드 (추상 - 하위 클래스에서 필수 구현)
        protected abstract Awaitable EnterState(CancellationToken cancellationToken);
        protected abstract Awaitable ExitState(CancellationToken cancellationToken);
        protected abstract void UpdateState(float deltaTime);
    }

    /// <summary>
    /// 메인 메뉴 상태
    /// </summary>
    public class MainState : GameState
    {
        public MainState() : base(GameStateType.Main) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[MainState] ========== 메인 메뉴 진입 시작 ==========");

            // 1. FadeOut (화면 어둡게)
            Debug.Log("[MainState] 1단계: 화면 FadeOut");
            if (Core.Managers.SceneTransitionManager.Instance != null)
            {
                await Core.Managers.SceneTransitionManager.Instance.FadeOutAsync(0.3f);
            }

            // 2. Main 씬 로드
            Debug.Log("[MainState] 2단계: Main 씬 로드");
            if (Core.Managers.SceneLoader.Instance != null)
            {
                await Core.Managers.SceneLoader.Instance.LoadSceneAsync(
                    Core.Enums.SceneType.MainMenu,
                    UnityEngine.SceneManagement.LoadSceneMode.Single
                );
            }

            // 3. 프레임 대기 (씬 로드 완료 보장, 모든 오브젝트 Awake/Start 실행)
            Debug.Log("[MainState] 3단계: 씬 로드 완료 대기");
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync(); // 한 프레임 더 대기 (안정성)

            // 4. MainMenuPanel Open
            Debug.Log("[MainState] 4단계: MainMenuPanel Open");
            if (Core.Managers.UIManager.Instance != null)
            {
                await Core.Managers.UIManager.Instance.OpenPanel(UI.Core.PanelType.MainMenu);
            }

            gameFlowManager?.ShowMainMenu();

            // 프레임 대기 (Panel 활성화 완료)
            await Awaitable.NextFrameAsync();

            // 5. FadeIn (Panel이 서서히 보임)
            Debug.Log("[MainState] 5단계: FadeIn");
            if (Core.Managers.SceneTransitionManager.Instance != null)
            {
                await Core.Managers.SceneTransitionManager.Instance.FadeInAsync(0.5f);
            }

            Debug.Log("[MainState] ========== 메인 메뉴 표시 완료 ==========");
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[MainState] 메인 메뉴 종료");

            // 메인 메뉴 Panel 닫기
            if (Core.Managers.UIManager.Instance != null)
            {
                await Core.Managers.UIManager.Instance.ClosePanel(UI.Core.PanelType.MainMenu);
            }

            gameFlowManager?.HideMainMenu();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 메인 메뉴에서의 업데이트 로직
        }
    }

    /// <summary>
    /// 게임플레이 리소스 로딩 상태
    /// 실제 진행률 기반 로딩 화면
    /// </summary>
    public class LoadingState : GameState
    {
        private float totalProgress = 0f;

        // 진행률 가중치
        private const float SCENE_WEIGHT = 0.5f;      // 씬 로드: 0~50%
        private const float RESOURCE_WEIGHT = 0.2f;   // 리소스: 50~70%
        private const float OBJECT_WEIGHT = 0.3f;     // 오브젝트: 70~100%

        // 타이밍 설정
        private const float FADE_OUT_DURATION = 0.3f;
        private const float LOADING_FADE_IN_DURATION = 0.2f;
        private const float LOADING_FADE_OUT_DURATION = 0.3f;
        private const float MINIMUM_DISPLAY_TIME = 1.5f;

        public LoadingState() : base(GameStateType.Loading) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[LoadingState] ========== 로딩 시작 ==========");
            float startTime = Time.time;

            // === 1단계: 현재 화면 FadeOut ===
            Debug.Log("[LoadingState] 1단계: 화면 FadeOut");
            if (Core.Managers.SceneTransitionManager.Instance != null)
            {
                await Core.Managers.SceneTransitionManager.Instance.FadeOutAsync(FADE_OUT_DURATION);
            }

            // === 2단계: LoadingUI 표시 ===
            Debug.Log("[LoadingState] 2단계: LoadingUI 표시");
            ShowLoadingUI();
            gameFlowManager?.ShowLoadingScreen();

            // FadeCanvas를 투명하게 해서 LoadingUI가 보이도록
            if (Core.Managers.SceneTransitionManager.Instance != null)
            {
                Core.Managers.SceneTransitionManager.Instance.SetFadeIn();
            }

            // 잠시 대기 후 로딩 화면이 안정화되도록
            await Awaitable.WaitForSecondsAsync(LOADING_FADE_IN_DURATION, cancellationToken);

            // === 3단계: 실제 로딩 작업 (진행률 추적) ===
            Debug.Log("[LoadingState] 3단계: 실제 로딩 시작");
            totalProgress = 0f;

            // 3-1. 씬 로드 (0% → 50%)
            await LoadSceneWithProgress(cancellationToken);

            // 3-2. 리소스 로드 (50% → 70%)
            await LoadResourcesWithProgress(cancellationToken);

            // 3-3. 오브젝트 초기화 (70% → 100%)
            await InitializeObjectsWithProgress(cancellationToken);

            // 100% 표시
            UpdateProgress(1f);
            Debug.Log("[LoadingState] 로딩 100% 완료!");

            // === 4단계: 최소 표시 시간 보장 ===
            float elapsed = Time.time - startTime;
            float remainingTime = Mathf.Max(0f, MINIMUM_DISPLAY_TIME - elapsed);
            if (remainingTime > 0f)
            {
                Debug.Log($"[LoadingState] 최소 시간 보장: {remainingTime:F1}초 대기");
                await Awaitable.WaitForSecondsAsync(remainingTime, cancellationToken);
            }

            // === 5단계: Loading 화면 FadeOut ===
            Debug.Log("[LoadingState] 5단계: Loading 화면 종료");
            // FadeOut은 Ingame 진입 시 처리

            // === 6단계: Ingame 전환 ===
            Debug.Log("[LoadingState] 6단계: Ingame 전환");
            await StateMachine.ForceTransitionToAsync(GameStateType.Ingame.ToString());

            Debug.Log("[LoadingState] ========== 로딩 완료 ==========");
        }

        /// <summary>
        /// 3-1. 씬 로드 (0% → 50%)
        /// </summary>
        private async Awaitable LoadSceneWithProgress(CancellationToken cancellationToken)
        {
            Debug.Log("[LoadingState] [1/3] 씬 로드 시작...");

            if (Core.Managers.SceneLoader.Instance == null)
            {
                Debug.LogError("[LoadingState] SceneLoader가 없습니다!");
                totalProgress += SCENE_WEIGHT;
                UpdateProgress(totalProgress);
                return;
            }

            float sceneProgress = 0f;

            // 씬 로드 진행률 이벤트 구독
            void OnSceneProgress(float progress)
            {
                // Unity의 AsyncOperation.progress는 0.9가 최대
                sceneProgress = Mathf.Clamp01(progress / 0.9f); // 0~0.9를 0~1로 정규화
                float currentProgress = sceneProgress * SCENE_WEIGHT;
                UpdateProgress(currentProgress);
            }

            Core.Managers.SceneLoader.Instance.OnLoadProgressChanged += OnSceneProgress;

            // 씬 로드 실행
            await Core.Managers.SceneLoader.Instance.LoadSceneAsync(
                Core.Enums.SceneType.Game,
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );

            // 이벤트 구독 해제
            Core.Managers.SceneLoader.Instance.OnLoadProgressChanged -= OnSceneProgress;

            totalProgress = SCENE_WEIGHT;
            UpdateProgress(totalProgress);
            Debug.Log($"[LoadingState] [1/3] 씬 로드 완료 ({totalProgress * 100:F0}%)");
        }

        /// <summary>
        /// 3-2. 리소스 로드 (50% → 70%)
        /// </summary>
        private async Awaitable LoadResourcesWithProgress(CancellationToken cancellationToken)
        {
            Debug.Log("[LoadingState] [2/3] 리소스 로드 시작...");

            try
            {
                var resourceManager = Core.Managers.GameResourceManager.Instance;

                if (resourceManager != null)
                {
                    float resourceProgress = 0f;

                    // 리소스 로드 진행률 이벤트 구독
                    void OnResourceProgress(Core.Enums.ResourceCategory category, float progress, string resourceName)
                    {
                        resourceProgress = progress;
                        float currentProgress = SCENE_WEIGHT + (resourceProgress * RESOURCE_WEIGHT);
                        UpdateProgress(currentProgress);
                    }

                    resourceManager.OnLoadProgress += OnResourceProgress;

                    // Gameplay 카테고리 로드
                    await resourceManager.LoadCategoryAsync(
                        Core.Enums.ResourceCategory.Gameplay,
                        cancellationToken
                    );

                    // 이벤트 구독 해제
                    resourceManager.OnLoadProgress -= OnResourceProgress;
                }
                else
                {
                    Debug.LogWarning("[LoadingState] GameResourceManager가 없습니다. 리소스 로딩을 건너뜁니다.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[LoadingState] 리소스 로딩 중 예외: {ex.Message}");
            }

            totalProgress = SCENE_WEIGHT + RESOURCE_WEIGHT;
            UpdateProgress(totalProgress);
            Debug.Log($"[LoadingState] [2/3] 리소스 로드 완료 ({totalProgress * 100:F0}%)");
        }

        /// <summary>
        /// 3-3. 오브젝트 초기화 (70% → 100%)
        /// </summary>
        private async Awaitable InitializeObjectsWithProgress(CancellationToken cancellationToken)
        {
            Debug.Log("[LoadingState] [3/3] 오브젝트 초기화 시작...");

            // 오브젝트 초기화 시뮬레이션 (실제로는 게임플레이 오브젝트 생성/초기화)
            int steps = 5;
            for (int i = 0; i < steps; i++)
            {
                await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken);

                float objectProgress = (float)(i + 1) / steps;
                float currentProgress = SCENE_WEIGHT + RESOURCE_WEIGHT + (objectProgress * OBJECT_WEIGHT);
                UpdateProgress(currentProgress);
            }

            totalProgress = 1f;
            UpdateProgress(totalProgress);
            Debug.Log($"[LoadingState] [3/3] 오브젝트 초기화 완료 (100%)");
        }

        /// <summary>
        /// 진행률 업데이트 (UI 반영)
        /// </summary>
        private void UpdateProgress(float progress)
        {
            UpdateLoadingUIProgress(progress);
            gameFlowManager?.UpdateLoadingProgress(progress);
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // 로딩 화면 비활성화
            gameFlowManager?.HideLoadingScreen();
            HideLoadingUI();

            // FadeIn (게임 화면 보이기)
            if (Core.Managers.SceneTransitionManager.Instance != null)
            {
                await Core.Managers.SceneTransitionManager.Instance.FadeInAsync(LOADING_FADE_OUT_DURATION);
            }

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 로딩 중에는 특별한 업데이트 로직 없음
        }

        // LoadingPanel 참조 (캐싱)
        private UI.Core.BasePanel loadingPanel;

        /// <summary>
        /// LoadingPanel 열기
        /// </summary>
        private async void ShowLoadingUI()
        {
            if (Core.Managers.UIManager.Instance != null)
            {
                loadingPanel = await Core.Managers.UIManager.Instance.OpenPanel(UI.Core.PanelType.Loading);
            }
        }

        /// <summary>
        /// LoadingPanel 닫기
        /// </summary>
        private async void HideLoadingUI()
        {
            if (Core.Managers.UIManager.Instance != null)
            {
                await Core.Managers.UIManager.Instance.ClosePanel(UI.Core.PanelType.Loading);
                loadingPanel = null;
            }
        }

        /// <summary>
        /// LoadingPanel의 UpdateProgress 메서드 호출
        /// </summary>
        private void UpdateLoadingUIProgress(float progress)
        {
            if (loadingPanel != null)
            {
                loadingPanel.UpdateProgress(progress);
            }
        }
    }

    /// <summary>
    /// 인게임 상태
    /// </summary>
    public class IngameState : GameState
    {
        public IngameState() : base(GameStateType.Ingame) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[IngameState] Ingame 상태 진입");

            // GameplayManager 찾기 또는 생성
            SetupGameplayManager();

            // GameplayHUD Panel 열기
            if (Core.Managers.UIManager.Instance != null)
            {
                await Core.Managers.UIManager.Instance.OpenPanel(UI.Core.PanelType.GameplayHUD);
            }

            // 인게임 UI 활성화
            gameFlowManager?.ShowIngameUI();

            await Awaitable.NextFrameAsync();

            // 게임 시간 복구
            Time.timeScale = 1f;

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // GameplayHUD Panel 닫기
            if (Core.Managers.UIManager.Instance != null)
            {
                await Core.Managers.UIManager.Instance.ClosePanel(UI.Core.PanelType.GameplayHUD);
            }
        }

        protected override void UpdateState(float deltaTime)
        {
            // 인게임 업데이트 로직
            // ESC 키로 일시정지
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameFlowManager?.TriggerEvent(GameEventType.PauseGame);
            }
        }

        /// <summary>
        /// GameplayManager 설정 (동적 검색)
        /// </summary>
        private void SetupGameplayManager()
        {
            // GameplayManager 타입을 동적으로 검색
            var gameplayManagerType = System.Type.GetType("Gameplay.GameplayManager, Gameplay.Manager");

            if (gameplayManagerType == null)
            {
                Debug.LogWarning("[IngameState] GameplayManager 타입을 찾을 수 없습니다.");
                return;
            }

            // 씬에서 GameplayManager 찾기
            var existingManager = Object.FindAnyObjectByType(gameplayManagerType);

            if (existingManager != null)
            {
                Debug.Log("[IngameState] GameplayManager 이미 존재함");
                return;
            }

            // 새로 생성
            GameObject managerObject = new GameObject("GameplayManager");
            managerObject.AddComponent(gameplayManagerType);
            Debug.Log("[IngameState] GameplayManager 생성 완료");
        }

    }

    /// <summary>
    /// 일시정지 상태
    /// </summary>
    public class PauseState : GameState
    {
        public PauseState() : base(GameStateType.Pause) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // 게임 시간 정지
            Time.timeScale = 0f;

            // 일시정지 UI 활성화
            gameFlowManager?.ShowPauseMenu();

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // 일시정지 UI 비활성화
            gameFlowManager?.HidePauseMenu();

            // 게임 시간 복구
            Time.timeScale = 1f;

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // ESC 키로 게임 복구
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameFlowManager?.TriggerEvent(GameEventType.ResumeGame);
            }
        }
    }

    /// <summary>
    /// 메뉴 상태
    /// </summary>
    public class MenuState : GameState
    {
        public MenuState() : base(GameStateType.Menu) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // 메뉴 UI 활성화
            gameFlowManager?.ShowInGameMenu();
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // 메뉴 UI 비활성화
            gameFlowManager?.HideInGameMenu();
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 메뉴 업데이트 로직
        }
    }

    /// <summary>
    /// 로비 상태
    /// </summary>
    public class LobbyState : GameState
    {
        public LobbyState() : base(GameStateType.Lobby) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[LobbyState] ========== 로비 진입 시작 ==========");

            // 1. FadeOut (화면 어둡게)
            Debug.Log("[LobbyState] 1단계: 화면 FadeOut");
            if (Core.Managers.SceneTransitionManager.Instance != null)
            {
                await Core.Managers.SceneTransitionManager.Instance.FadeOutAsync(0.3f);
            }

            // 2. Lobby 씬 로드
            Debug.Log("[LobbyState] 2단계: Lobby 씬 로드");
            if (Core.Managers.SceneLoader.Instance != null)
            {
                await Core.Managers.SceneLoader.Instance.LoadSceneAsync(
                    Core.Enums.SceneType.Lobby,
                    UnityEngine.SceneManagement.LoadSceneMode.Single
                );
            }

            // 3. 프레임 대기 (씬 로드 완료 보장)
            Debug.Log("[LobbyState] 3단계: 씬 로드 완료 대기");
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();

            // 4. LobbyManager 설정 (Player 생성)
            Debug.Log("[LobbyState] 4단계: LobbyManager 설정");
            SetupLobbyManager();

            // 5. 로비 UI 활성화
            Debug.Log("[LobbyState] 5단계: 로비 UI 활성화");
            gameFlowManager?.ShowLobby();

            await Awaitable.NextFrameAsync();

            // 6. FadeIn (로비 씬이 서서히 보임)
            Debug.Log("[LobbyState] 6단계: FadeIn");
            if (Core.Managers.SceneTransitionManager.Instance != null)
            {
                await Core.Managers.SceneTransitionManager.Instance.FadeInAsync(0.5f);
            }

            Debug.Log("[LobbyState] ========== 로비 표시 완료 ==========");
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[LobbyState] 로비 종료");
            // 로비 UI 비활성화
            gameFlowManager?.HideLobby();
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 로비 업데이트 로직
        }

        /// <summary>
        /// LobbyManager 설정 (동적 검색)
        /// </summary>
        private void SetupLobbyManager()
        {
            // LobbyManager 타입을 동적으로 검색
            var lobbyManagerType = System.Type.GetType("Gameplay.LobbyManager, Gameplay.Manager");

            if (lobbyManagerType == null)
            {
                Debug.LogWarning("[LobbyState] LobbyManager 타입을 찾을 수 없습니다.");
                return;
            }

            // 씬에서 LobbyManager 찾기
            var existingManager = Object.FindAnyObjectByType(lobbyManagerType);

            if (existingManager != null)
            {
                Debug.Log("[LobbyState] LobbyManager 이미 존재함");
                return;
            }

            // 새로 생성
            GameObject managerObject = new GameObject("LobbyManager");
            managerObject.AddComponent(lobbyManagerType);
            Debug.Log("[LobbyState] LobbyManager 생성 완료");
        }
    }

    /// <summary>
    /// 초기 리소스 로딩 상태 (Preload)
    /// Essential + MainMenu 카테고리 리소스를 로드
    /// </summary>
    public class PreloadState : GameState
    {
        public bool IsCompleted { get; private set; } = false;

        public PreloadState() : base(GameStateType.Preload) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[PreloadState] 초기 리소스 로딩 시작...");

            // 최소 2초 보장을 위한 시작 시간 기록
            float startTime = Time.time;
            float minimumDuration = 2f;

            // 로딩 화면 활성화
            gameFlowManager?.ShowLoadingScreen();

            bool success = true;

            // GameResourceManager가 존재하는 경우에만 리소스 로드 시도
            try
            {
                var resourceManager = Core.Managers.GameResourceManager.Instance;

                if (resourceManager != null)
                {
                    // 로드 진행률 이벤트 구독
                    resourceManager.OnLoadProgress += OnResourceLoadProgress;

                    // Essential + MainMenu 카테고리 로드
                    success = await resourceManager.LoadCategoriesAsync(
                        new Core.Enums.ResourceCategory[]
                        {
                            Core.Enums.ResourceCategory.Essential,
                            Core.Enums.ResourceCategory.MainMenu
                        },
                        cancellationToken
                    );

                    // 이벤트 구독 해제
                    resourceManager.OnLoadProgress -= OnResourceLoadProgress;
                }
                else
                {
                    Debug.LogWarning("[PreloadState] GameResourceManager가 없습니다. 리소스 로딩을 건너뜁니다.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[PreloadState] 리소스 로딩 중 예외 발생: {ex.Message}. 계속 진행합니다.");
                success = true; // 예외 발생 시에도 계속 진행
            }

            // 성공 여부와 관계없이 계속 진행 (데모/테스트 환경 고려)
            if (success)
            {
                Debug.Log("[PreloadState] 초기 리소스 로딩 완료!");
            }
            else
            {
                Debug.LogWarning("[PreloadState] 리소스 로딩 실패했지만 계속 진행합니다.");
            }

            // 최소 2초 보장 - 남은 시간만큼 대기
            float elapsed = Time.time - startTime;
            float remainingTime = Mathf.Max(0f, minimumDuration - elapsed);

            if (remainingTime > 0f)
            {
                Debug.Log($"[PreloadState] 최소 시간 보장을 위해 {remainingTime:F1}초 대기 중...");
                await Awaitable.WaitForSecondsAsync(remainingTime, cancellationToken);
            }

            // 작업 완료 - Transition이 자동으로 Main으로 전환
            IsCompleted = true;
            Debug.Log("[PreloadState] 리소스 로딩 완료 - Main으로 전환 대기 중...");
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // 로딩 화면 비활성화
            gameFlowManager?.HideLoadingScreen();

            // 완료 플래그 리셋
            IsCompleted = false;

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 로딩 중에는 특별한 업데이트 로직 없음
        }

        private void OnResourceLoadProgress(Core.Enums.ResourceCategory category, float progress, string resourceName)
        {
            // UI 업데이트
            gameFlowManager?.UpdateLoadingProgress(progress);
            Debug.Log($"[PreloadState] {category} 로딩 중... {progress * 100:F0}% - {resourceName}");
        }
    }
}
