using UnityEngine;
using UnityEngine.UI;
using GameFlow;
using Core;

namespace UI.Menu
{
    /// <summary>
    /// 메인 메뉴 UI 관리
    /// 게임 시작, 설정, 종료 버튼 제공
    /// SingletonManager 패턴으로 DontDestroyOnLoad 자동 설정
    /// </summary>
    public class MainMenuUI : SingletonManager<MainMenuUI>
    {
        [Header("UI 버튼")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("제목")]
        [SerializeField] private Text titleText;

        [Header("설정")]
        [SerializeField] private bool createUIOnStart = true;

        private Canvas mainMenuCanvas;

        protected override void OnSingletonAwake()
        {
            if (createUIOnStart)
            {
                CreateUI();
            }

            SetupButtons();

            // 초기에는 숨김
            Hide();
        }

        /// <summary>
        /// 메인 메뉴 표시
        /// </summary>
        public void Show()
        {
            if (mainMenuCanvas != null)
            {
                mainMenuCanvas.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 메인 메뉴 숨김
        /// </summary>
        public void Hide()
        {
            if (mainMenuCanvas != null)
            {
                mainMenuCanvas.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// UI 자동 생성 (빈 씬에서 테스트용)
        /// </summary>
        private void CreateUI()
        {
            // Canvas 찾기 또는 생성 (FadeCanvas 제외)
            mainMenuCanvas = FindMainCanvas();
            if (mainMenuCanvas == null)
            {
                GameObject canvasObj = new GameObject("MainMenuCanvas");
                canvasObj.transform.SetParent(transform); // MainMenuUI 하위로 설정
                mainMenuCanvas = canvasObj.AddComponent<Canvas>();
                mainMenuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mainMenuCanvas.sortingOrder = 0; // 일반 UI 레이어
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("[MainMenuUI] MainMenuCanvas 생성 완료");
            }

            Canvas canvas = mainMenuCanvas;

            // 제목 텍스트
            if (titleText == null)
            {
                GameObject titleObj = new GameObject("TitleText");
                titleObj.transform.SetParent(canvas.transform);
                titleText = titleObj.AddComponent<Text>();
                titleText.text = "GASPT";
                titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                titleText.fontSize = 72;
                titleText.alignment = TextAnchor.MiddleCenter;
                titleText.color = Color.white;

                RectTransform titleRect = titleObj.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0.5f, 0.7f);
                titleRect.anchorMax = new Vector2(0.5f, 0.7f);
                titleRect.sizeDelta = new Vector2(400, 100);
                titleRect.anchoredPosition = Vector2.zero;
            }

            // 시작 버튼
            if (startButton == null)
            {
                startButton = CreateButton("StartButton", "게임 시작", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(200, 60));
            }

            // 설정 버튼
            if (settingsButton == null)
            {
                settingsButton = CreateButton("SettingsButton", "설정", canvas.transform, new Vector2(0.5f, 0.4f), new Vector2(200, 60));
            }

            // 종료 버튼
            if (quitButton == null)
            {
                quitButton = CreateButton("QuitButton", "종료", canvas.transform, new Vector2(0.5f, 0.3f), new Vector2(200, 60));
            }

            Debug.Log("[MainMenuUI] UI 자동 생성 완료");
        }

        /// <summary>
        /// MainMenu용 Canvas 찾기 (FadeCanvas 제외)
        /// </summary>
        private Canvas FindMainCanvas()
        {
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in allCanvases)
            {
                // FadeCanvas가 아닌 Canvas 찾기
                if (c.gameObject.name != "FadeCanvas")
                {
                    Debug.Log($"[MainMenuUI] 기존 Canvas 발견: {c.gameObject.name}");
                    return c;
                }
            }
            return null;
        }

        /// <summary>
        /// 버튼 생성 헬퍼 메서드
        /// </summary>
        private Button CreateButton(string name, string text, Transform parent, Vector2 anchor, Vector2 size)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent);

            // Button 컴포넌트
            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // RectTransform 설정
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchor;
            buttonRect.anchorMax = anchor;
            buttonRect.sizeDelta = size;
            buttonRect.anchoredPosition = Vector2.zero;

            // 텍스트 자식 생성
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 24;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            return button;
        }

        /// <summary>
        /// 버튼 이벤트 설정
        /// </summary>
        private void SetupButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            }

            Debug.Log("[MainMenuUI] 버튼 이벤트 설정 완료");
        }

        /// <summary>
        /// 시작 버튼 클릭
        /// </summary>
        private void OnStartButtonClicked()
        {
            Debug.Log("[MainMenuUI] 게임 시작 버튼 클릭!");

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.StartGame();
            }
            else
            {
                Debug.LogError("[MainMenuUI] GameFlowManager가 없습니다!");
            }
        }

        /// <summary>
        /// 설정 버튼 클릭
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            Debug.Log("[MainMenuUI] 설정 버튼 클릭 (미구현)");
            // TODO: 설정 메뉴 열기
        }

        /// <summary>
        /// 종료 버튼 클릭
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("[MainMenuUI] 종료 버튼 클릭");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartButtonClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            }
        }
    }
}
