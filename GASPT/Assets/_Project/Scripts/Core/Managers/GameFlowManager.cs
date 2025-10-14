using System;
using UnityEngine;
using UnityEngine.UI;
using FSM.Core;
using Core;

namespace GameFlow
{
    /// <summary>
    /// 게임 흐름을 관리하는 FSM 컨트롤러
    /// SingletonManager 패턴으로 DontDestroyOnLoad 자동 설정
    /// </summary>
    public class GameFlowManager : SingletonManager<GameFlowManager>
    {

        [Header("UI 참조")]
        [SerializeField] private GameObject mainMenuUI;
        [SerializeField] private GameObject loadingScreenUI;
        [SerializeField] private GameObject ingameUI;
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private GameObject inGameMenuUI;
        [SerializeField] private GameObject lobbyUI;

        [Header("로딩 화면")]
        [SerializeField] private Slider loadingProgressBar;
        [SerializeField] private Text loadingText;

        [Header("설정")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private GameStateType initialState = GameStateType.Preload;

        // FSM 관련
        private StateMachine stateMachine;

        // 현재 상태 정보
        public GameStateType CurrentState =>
            Enum.TryParse<GameStateType>(stateMachine?.CurrentStateId, out var state) ? state : GameStateType.Main;

        // StateMachine 접근용 프로퍼티
        public StateMachine StateMachine => stateMachine;

        // 이벤트
        public event Action<GameStateType, GameStateType> OnStateChanged;
        public event Action<GameEventType> OnEventTriggered;

        protected override void OnSingletonAwake()
        {
            InitializeStateMachine();
            InitializeUI();
        }

        private void Start()
        {
            // autoStart가 true일 때만 자동으로 시작
            if (autoStart)
            {
                stateMachine.StartStateMachine(initialState.ToString());
            }
        }

        /// <summary>
        /// FSM 초기화
        /// </summary>
        private void InitializeStateMachine()
        {
            stateMachine = gameObject.AddComponent<StateMachine>();

            // 상태들 추가
            stateMachine.AddState(new PreloadState());  // 초기 리소스 로딩 상태
            stateMachine.AddState(new MainState());
            stateMachine.AddState(new LoadingState());
            stateMachine.AddState(new IngameState());
            stateMachine.AddState(new PauseState());
            stateMachine.AddState(new MenuState());
            stateMachine.AddState(new LobbyState());

            // 기본 전환 설정
            GameFlowTransitionSetup.SetupDefaultTransitions(stateMachine, this);

            // 상태 변경 이벤트 구독
            stateMachine.OnStateChanged += OnGameStateChanged;

            // Transition 이벤트 구독 (씬 로딩 처리)
            stateMachine.OnTransitionStarted += OnTransitionStarted;
        }


        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 모든 UI 비활성화
            HideAllUI();
        }

        /// <summary>
        /// 게임 이벤트 트리거
        /// EventTransition이 이벤트를 받아서 FSM 전환 처리
        /// </summary>
        public void TriggerEvent(GameEventType eventType)
        {
            Debug.Log($"[GameFlow] Event triggered: {eventType}");
            OnEventTriggered?.Invoke(eventType);
        }

        /// <summary>
        /// 특정 상태로 전환 (비동기)
        /// </summary>
        public async void TransitionTo(GameStateType targetState)
        {
            Debug.Log($"[GameFlow] TransitionTo 호출: {CurrentState} -> {targetState}");

            if (stateMachine == null)
            {
                Debug.LogError("[GameFlow] StateMachine이 null입니다!");
                return;
            }

            if (!stateMachine.IsRunning)
            {
                Debug.LogError("[GameFlow] StateMachine이 실행 중이 아닙니다!");
                return;
            }

            Debug.Log($"[GameFlow] ForceTransitionToAsync 실행: {targetState}");
            await stateMachine.ForceTransitionToAsync(targetState.ToString());
        }

        /// <summary>
        /// FSM 수동 시작 (autoStart = false일 때 사용)
        /// </summary>
        public void StartManually(GameStateType startState)
        {
            if (stateMachine != null)
            {
                stateMachine.StartStateMachine(startState.ToString());
            }
        }

        /// <summary>
        /// 상태 변경 이벤트 핸들러
        /// </summary>
        private void OnGameStateChanged(string fromState, string toState)
        {
            if (Enum.TryParse<GameStateType>(fromState, out var from) &&
                Enum.TryParse<GameStateType>(toState, out var to))
            {
                OnStateChanged?.Invoke(from, to);
                Debug.Log($"[GameFlow] State changed: {from} -> {to}");
            }
        }

        /// <summary>
        /// Transition 시작 이벤트 핸들러 (씬 로딩 처리)
        /// </summary>
        private void OnTransitionStarted(FSM.Core.ITransition transition)
        {
            // Preload → Main 전환 시 씬 로드
            if (transition.FromStateId == GameStateType.Preload.ToString() &&
                transition.ToStateId == GameStateType.Main.ToString())
            {
                _ = LoadSceneForTransition(Core.Enums.SceneType.Preload, Core.Enums.SceneType.Main);
            }
            // 필요시 다른 씬 전환도 여기에 추가 가능
        }

        /// <summary>
        /// Transition을 위한 씬 로딩
        /// </summary>
        private async Awaitable LoadSceneForTransition(Core.Enums.SceneType fromScene, Core.Enums.SceneType toScene)
        {
            if (Core.Managers.SceneLoader.Instance != null)
            {
                Debug.Log($"[GameFlow] Transition에서 씬 로드: {fromScene} → {toScene}");
                await Core.Managers.SceneLoader.Instance.TransitionToSceneAsync(fromScene, toScene);
            }
            else
            {
                Debug.LogWarning("[GameFlow] SceneLoader가 없습니다. 씬 전환을 건너뜁니다.");
            }
        }

        #region UI 관리 메서드들

        // TODO: 향후 상태 전환 시 UI 정리에 사용 예정
        private void HideAllUI()
        {
            SetUIActive(mainMenuUI, false);
            SetUIActive(loadingScreenUI, false);
            SetUIActive(ingameUI, false);
            SetUIActive(pauseMenuUI, false);
            SetUIActive(inGameMenuUI, false);
            SetUIActive(lobbyUI, false);
        }

        // TODO: 향후 동적 UI 관리에 사용 예정
        private void SetUIActive(GameObject ui, bool active)
        {
            if (ui != null) ui.SetActive(active);
        }

        public void ShowMainMenu()
        {
            HideAllUI();
            SetUIActive(mainMenuUI, true);
        }

        public void HideMainMenu()
        {
            SetUIActive(mainMenuUI, false);
        }

        public void ShowLoadingScreen()
        {
            HideAllUI();
            SetUIActive(loadingScreenUI, true);
            UpdateLoadingProgress(0f);
        }

        public void HideLoadingScreen()
        {
            SetUIActive(loadingScreenUI, false);
        }

        public void UpdateLoadingProgress(float progress)
        {
            if (loadingProgressBar != null)
                loadingProgressBar.value = progress;

            if (loadingText != null)
                loadingText.text = $"Loading... {progress * 100:F0}%";
        }

        public void ShowIngameUI()
        {
            HideAllUI();
            SetUIActive(ingameUI, true);
        }

        public void ShowPauseMenu()
        {
            SetUIActive(pauseMenuUI, true);
        }

        public void HidePauseMenu()
        {
            SetUIActive(pauseMenuUI, false);
        }

        public void ShowInGameMenu()
        {
            SetUIActive(inGameMenuUI, true);
        }

        public void HideInGameMenu()
        {
            SetUIActive(inGameMenuUI, false);
        }

        public void ShowLobby()
        {
            HideAllUI();
            SetUIActive(lobbyUI, true);
        }

        public void HideLobby()
        {
            SetUIActive(lobbyUI, false);
        }

        #endregion

        #region 공개 API 메서드들

        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            TriggerEvent(GameEventType.StartGame);
        }

        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void PauseGame()
        {
            TriggerEvent(GameEventType.PauseGame);
        }

        /// <summary>
        /// 게임 재개
        /// </summary>
        public void ResumeGame()
        {
            TriggerEvent(GameEventType.ResumeGame);
        }

        /// <summary>
        /// 메인 메뉴로 이동
        /// </summary>
        public void GoToMainMenu()
        {
            TriggerEvent(GameEventType.GoToMain);
        }

        /// <summary>
        /// 로비로 이동
        /// </summary>
        public void GoToLobby()
        {
            TriggerEvent(GameEventType.GoToLobby);
        }

        /// <summary>
        /// 인게임 메뉴 열기
        /// </summary>
        public void OpenInGameMenu()
        {
            TriggerEvent(GameEventType.OpenMenu);
        }

        /// <summary>
        /// 인게임 메뉴 닫기
        /// </summary>
        public void CloseInGameMenu()
        {
            TriggerEvent(GameEventType.CloseMenu);
        }

        #endregion

        private void OnDestroy()
        {
            if (stateMachine != null)
            {
                stateMachine.OnStateChanged -= OnGameStateChanged;
                stateMachine.OnTransitionStarted -= OnTransitionStarted;
            }
        }

    }
}
