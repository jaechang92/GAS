using UnityEngine;
using UnityEngine.UI;

namespace GameFlow
{
    /// <summary>
    /// GameFlow 시스템 테스트를 위한 스크립트
    /// </summary>
    public class GameFlowTestScript : MonoBehaviour
    {
        [Header("GameFlow Manager 참조")]
        [SerializeField] private GameFlowManager gameFlowManager;

        [Header("테스트 UI")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button pauseGameButton;
        [SerializeField] private Button resumeGameButton;
        [SerializeField] private Button openMenuButton;
        [SerializeField] private Button closeMenuButton;
        [SerializeField] private Button goToLobbyButton;
        [SerializeField] private Button goToMainButton;

        [Header("상태 정보 표시")]
        [SerializeField] private Text currentStateText;
        [SerializeField] private Text eventLogText;

        private string eventLog = "";

        private void Start()
        {
            InitializeTestUI();
            SubscribeToEvents();
            UpdateCurrentStateDisplay();
        }

        private void InitializeTestUI()
        {
            if (gameFlowManager == null)
                gameFlowManager = FindObjectOfType<GameFlowManager>();

            // 버튼 이벤트 연결
            if (startGameButton != null)
                startGameButton.onClick.AddListener(() => TestEvent("StartGame", () => gameFlowManager.StartGame()));

            if (pauseGameButton != null)
                pauseGameButton.onClick.AddListener(() => TestEvent("PauseGame", () => gameFlowManager.PauseGame()));

            if (resumeGameButton != null)
                resumeGameButton.onClick.AddListener(() => TestEvent("ResumeGame", () => gameFlowManager.ResumeGame()));

            if (openMenuButton != null)
                openMenuButton.onClick.AddListener(() => TestEvent("OpenMenu", () => gameFlowManager.OpenInGameMenu()));

            if (closeMenuButton != null)
                closeMenuButton.onClick.AddListener(() => TestEvent("CloseMenu", () => gameFlowManager.CloseInGameMenu()));

            if (goToLobbyButton != null)
                goToLobbyButton.onClick.AddListener(() => TestEvent("GoToLobby", () => gameFlowManager.GoToLobby()));

            if (goToMainButton != null)
                goToMainButton.onClick.AddListener(() => TestEvent("GoToMain", () => gameFlowManager.GoToMainMenu()));
        }

        private void SubscribeToEvents()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.OnStateChanged += OnStateChanged;
                gameFlowManager.OnEventTriggered += OnEventTriggered;
            }
        }

        private void OnStateChanged(GameStateType fromState, GameStateType toState)
        {
            LogEvent($"상태 변경: {fromState} → {toState}");
            UpdateCurrentStateDisplay();
        }

        private void OnEventTriggered(GameEventType eventType)
        {
            LogEvent($"이벤트 발생: {eventType}");
        }

        private void TestEvent(string eventName, System.Action action)
        {
            LogEvent($"테스트 실행: {eventName}");
            action?.Invoke();
        }

        private void LogEvent(string message)
        {
            eventLog += $"[{Time.time:F1}s] {message}\n";

            // 로그가 너무 길어지면 오래된 항목 제거
            string[] lines = eventLog.Split('\n');
            if (lines.Length > 10)
            {
                eventLog = string.Join("\n", lines, lines.Length - 10, 10);
            }

            if (eventLogText != null)
                eventLogText.text = eventLog;

            Debug.Log($"[GameFlowTest] {message}");
        }

        private void UpdateCurrentStateDisplay()
        {
            if (currentStateText != null && gameFlowManager != null)
            {
                currentStateText.text = $"현재 상태: {gameFlowManager.CurrentState}";
            }
        }

        // 키보드 테스트
        private void Update()
        {
            if (gameFlowManager == null) return;

            // 키보드 단축키 테스트
            if (Input.GetKeyDown(KeyCode.F1))
                TestEvent("F1 - StartGame", () => gameFlowManager.StartGame());

            if (Input.GetKeyDown(KeyCode.F2))
                TestEvent("F2 - PauseGame", () => gameFlowManager.PauseGame());

            if (Input.GetKeyDown(KeyCode.F3))
                TestEvent("F3 - ResumeGame", () => gameFlowManager.ResumeGame());

            if (Input.GetKeyDown(KeyCode.F4))
                TestEvent("F4 - GoToMain", () => gameFlowManager.GoToMainMenu());

            if (Input.GetKeyDown(KeyCode.F5))
                TestEvent("F5 - GoToLobby", () => gameFlowManager.GoToLobby());
        }

        private void OnDestroy()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.OnStateChanged -= OnStateChanged;
                gameFlowManager.OnEventTriggered -= OnEventTriggered;
            }
        }

        // 자동 테스트 시퀀스
        [ContextMenu("자동 테스트 실행")]
        public void RunAutomatedTest()
        {
            StartCoroutine(AutomatedTestSequence());
        }

        private System.Collections.IEnumerator AutomatedTestSequence()
        {
            LogEvent("=== 자동 테스트 시작 ===");

            yield return new WaitForSeconds(1f);
            TestEvent("Auto - StartGame", () => gameFlowManager.StartGame());

            yield return new WaitForSeconds(3f); // 로딩 대기
            TestEvent("Auto - PauseGame", () => gameFlowManager.PauseGame());

            yield return new WaitForSeconds(2f);
            TestEvent("Auto - ResumeGame", () => gameFlowManager.ResumeGame());

            yield return new WaitForSeconds(2f);
            TestEvent("Auto - OpenMenu", () => gameFlowManager.OpenInGameMenu());

            yield return new WaitForSeconds(2f);
            TestEvent("Auto - CloseMenu", () => gameFlowManager.CloseInGameMenu());

            yield return new WaitForSeconds(2f);
            TestEvent("Auto - GoToLobby", () => gameFlowManager.GoToLobby());

            yield return new WaitForSeconds(2f);
            TestEvent("Auto - GoToMain", () => gameFlowManager.GoToMainMenu());

            LogEvent("=== 자동 테스트 완료 ===");
        }

        // 디버그 정보 표시
        [ContextMenu("현재 상태 정보 출력")]
        public void PrintCurrentStateInfo()
        {
            if (gameFlowManager == null) return;

            Debug.Log($"현재 상태: {gameFlowManager.CurrentState}");
            Debug.Log($"StateMachine 활성화: {gameFlowManager.StateMachine != null}");

            if (gameFlowManager.StateMachine != null)
            {
                Debug.Log($"FSM 현재 상태 ID: {gameFlowManager.StateMachine.CurrentStateId}");
            }
        }
    }
}