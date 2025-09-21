using UnityEngine;
using System.Collections;

namespace GameFlow
{
    /// <summary>
    /// GameFlow 시스템의 핵심 기능을 코드로 테스트하는 러너
    /// </summary>
    public class GameFlowTestRunner : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private float testInterval = 2f;

        private GameFlowManager gameFlowManager;

        private void Start()
        {
            if (runOnStart)
            {
                InitializeAndTest();
            }
        }

        public void InitializeAndTest()
        {
            StartCoroutine(RunFullTest());
        }

        private IEnumerator RunFullTest()
        {
            Debug.Log("=== GameFlow 시스템 테스트 시작 ===");

            // 1. GameFlow 매니저 생성 및 초기화 테스트
            yield return StartCoroutine(TestGameFlowManagerCreation());

            // 2. 상태 전환 테스트
            yield return StartCoroutine(TestStateTransitions());

            // 3. 이벤트 시스템 테스트
            yield return StartCoroutine(TestEventSystem());

            // 4. FSM 통합 테스트
            yield return StartCoroutine(TestFSMIntegration());

            Debug.Log("=== GameFlow 시스템 테스트 완료 ===");
        }

        private IEnumerator TestGameFlowManagerCreation()
        {
            Debug.Log("1. GameFlow 매니저 생성 테스트 시작");

            // GameFlow 매니저가 이미 있는지 확인
            gameFlowManager = FindObjectOfType<GameFlowManager>();

            if (gameFlowManager == null)
            {
                Debug.Log("GameFlowManager가 없어서 새로 생성합니다.");

                // GameFlow 매니저 생성
                GameObject gameFlowGO = new GameObject("GameFlowManager");
                gameFlowManager = gameFlowGO.AddComponent<GameFlowManager>();

                // 테스트용 UI 오브젝트들 생성
                CreateTestUIObjects();
            }
            else
            {
                Debug.Log("기존 GameFlowManager를 찾았습니다.");
            }

            yield return new WaitForSeconds(1f);

            // 초기 상태 확인
            Debug.Log($"초기 상태: {gameFlowManager.CurrentState}");
            Debug.Log("GameFlow 매니저 생성 테스트 완료");
        }

        private void CreateTestUIObjects()
        {
            // Canvas 생성
            GameObject canvasGO = new GameObject("TestCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // 테스트용 UI 패널들 생성
            CreateTestUIPanel(canvasGO, "MainMenuUI");
            CreateTestUIPanel(canvasGO, "LoadingScreenUI");
            CreateTestUIPanel(canvasGO, "IngameUI");
            CreateTestUIPanel(canvasGO, "PauseMenuUI");
            CreateTestUIPanel(canvasGO, "InGameMenuUI");
            CreateTestUIPanel(canvasGO, "LobbyUI");

            Debug.Log("테스트용 UI 오브젝트들을 생성했습니다.");
        }

        private GameObject CreateTestUIPanel(GameObject parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent.transform, false);

            var rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            var image = panel.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0.5f); // 반투명 검정

            // 텍스트 추가
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(panel.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var text = textGO.AddComponent<UnityEngine.UI.Text>();
            text.text = name;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            panel.SetActive(false); // 초기에는 비활성화

            return panel;
        }

        private IEnumerator TestStateTransitions()
        {
            Debug.Log("2. 상태 전환 테스트 시작");

            if (gameFlowManager == null)
            {
                Debug.LogError("GameFlowManager가 없습니다!");
                yield break;
            }

            // Main -> Loading
            Debug.Log("Main → Loading 테스트");
            gameFlowManager.StartGame();
            yield return new WaitForSeconds(testInterval);

            // Loading은 자동으로 Ingame으로 전환됨 (약 5초 후)
            Debug.Log("Loading → Ingame 자동 전환 대기");
            yield return new WaitForSeconds(6f);

            // Ingame -> Pause
            Debug.Log("Ingame → Pause 테스트");
            gameFlowManager.PauseGame();
            yield return new WaitForSeconds(testInterval);

            // Pause -> Ingame
            Debug.Log("Pause → Ingame 테스트");
            gameFlowManager.ResumeGame();
            yield return new WaitForSeconds(testInterval);

            Debug.Log("상태 전환 테스트 완료");
        }

        private IEnumerator TestEventSystem()
        {
            Debug.Log("3. 이벤트 시스템 테스트 시작");

            bool eventReceived = false;
            GameEventType receivedEventType = GameEventType.StartGame;

            // 이벤트 리스너 등록
            gameFlowManager.OnEventTriggered += (eventType) =>
            {
                eventReceived = true;
                receivedEventType = eventType;
                Debug.Log($"이벤트 수신: {eventType}");
            };

            // 여러 이벤트 테스트
            GameEventType[] testEvents = {
                GameEventType.OpenMenu,
                GameEventType.CloseMenu,
                GameEventType.GoToLobby,
                GameEventType.GoToMain
            };

            foreach (var eventType in testEvents)
            {
                eventReceived = false;
                gameFlowManager.TriggerEvent(eventType);

                yield return new WaitForSeconds(0.5f);

                if (eventReceived && receivedEventType == eventType)
                {
                    Debug.Log($"✓ {eventType} 이벤트 정상 동작");
                }
                else
                {
                    Debug.LogError($"✗ {eventType} 이벤트 실패");
                }

                yield return new WaitForSeconds(testInterval);
            }

            Debug.Log("이벤트 시스템 테스트 완료");
        }

        private IEnumerator TestFSMIntegration()
        {
            Debug.Log("4. FSM 통합 테스트 시작");

            if (gameFlowManager.StateMachine == null)
            {
                Debug.LogError("StateMachine이 초기화되지 않았습니다!");
                yield break;
            }

            // FSM 상태 정보 확인
            Debug.Log($"FSM 현재 상태: {gameFlowManager.StateMachine.CurrentStateId}");
            Debug.Log($"GameFlow 현재 상태: {gameFlowManager.CurrentState}");

            // 상태 동기화 테스트
            var fsmStateId = gameFlowManager.StateMachine.CurrentStateId;
            var gameFlowState = gameFlowManager.CurrentState.ToString();

            if (fsmStateId == gameFlowState)
            {
                Debug.Log("✓ FSM과 GameFlow 상태 동기화 정상");
            }
            else
            {
                Debug.LogError($"✗ 상태 동기화 실패: FSM={fsmStateId}, GameFlow={gameFlowState}");
            }

            // 강제 전환 테스트
            Debug.Log("강제 전환 테스트: Lobby로 이동");
            gameFlowManager.TransitionTo(GameStateType.Lobby);
            yield return new WaitForSeconds(1f);

            Debug.Log($"전환 후 상태: {gameFlowManager.CurrentState}");

            Debug.Log("FSM 통합 테스트 완료");
        }

        // 수동 테스트 메서드들
        [ContextMenu("GameFlow 매니저 생성 테스트")]
        public void TestManagerCreation()
        {
            StartCoroutine(TestGameFlowManagerCreation());
        }

        [ContextMenu("상태 전환 테스트")]
        public void TestTransitions()
        {
            StartCoroutine(TestStateTransitions());
        }

        [ContextMenu("이벤트 시스템 테스트")]
        public void TestEvents()
        {
            StartCoroutine(TestEventSystem());
        }

        [ContextMenu("현재 상태 정보 출력")]
        public void PrintCurrentState()
        {
            if (gameFlowManager != null)
            {
                Debug.Log($"현재 GameFlow 상태: {gameFlowManager.CurrentState}");
                Debug.Log($"현재 FSM 상태: {gameFlowManager.StateMachine?.CurrentStateId}");
            }
            else
            {
                Debug.Log("GameFlowManager가 없습니다.");
            }
        }
    }
}