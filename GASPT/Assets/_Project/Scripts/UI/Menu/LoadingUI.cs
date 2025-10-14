using UnityEngine;
using UnityEngine.UI;
using Core.Managers;
using Core;

namespace UI.Menu
{
    /// <summary>
    /// 로딩 화면 UI 관리
    /// 로딩 진행률 표시 및 팁 표시
    /// SingletonManager 패턴으로 DontDestroyOnLoad 자동 설정
    /// </summary>
    public class LoadingUI : SingletonManager<LoadingUI>
    {
        [Header("UI 참조")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private Text progressText;
        [SerializeField] private Text loadingTipText;

        [Header("설정")]
        [SerializeField] private bool createUIOnStart = true;
        [SerializeField] private float minDisplayTime = 1f; // 최소 로딩 표시 시간

        [Header("로딩 팁")]
        [SerializeField] private string[] loadingTips = new string[]
        {
            "스페이스바를 눌러 점프할 수 있습니다.",
            "연속으로 공격하면 콤보가 발동됩니다!",
            "적의 공격 패턴을 관찰하세요.",
            "대시로 적의 공격을 회피하세요!",
            "체력이 부족하면 물약을 사용하세요."
        };

        private float loadingStartTime;
        private bool isLoading;
        private Canvas loadingCanvas;

        protected override void OnSingletonAwake()
        {
            if (createUIOnStart)
            {
                CreateUI();
            }

            SubscribeToSceneLoader();

            // 초기에는 숨김
            Hide();
        }

        /// <summary>
        /// 로딩 화면 표시
        /// </summary>
        public void Show()
        {
            if (loadingCanvas != null)
            {
                loadingCanvas.gameObject.SetActive(true);
            }

            ShowRandomTip();
            loadingStartTime = Time.time;
            isLoading = true;

            // 진행률 초기화
            UpdateProgress(0f);
        }

        /// <summary>
        /// 로딩 화면 숨김
        /// </summary>
        public void Hide()
        {
            if (loadingCanvas != null)
            {
                loadingCanvas.gameObject.SetActive(false);
            }
            isLoading = false;
        }

        /// <summary>
        /// UI 자동 생성 (빈 씬에서 테스트용)
        /// </summary>
        private void CreateUI()
        {
            // Canvas 찾기 또는 생성 (FadeCanvas 제외)
            loadingCanvas = FindLoadingCanvas();
            if (loadingCanvas == null)
            {
                GameObject canvasObj = new GameObject("LoadingCanvas");
                canvasObj.transform.SetParent(transform); // LoadingUI 하위로 설정
                loadingCanvas = canvasObj.AddComponent<Canvas>();
                loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                loadingCanvas.sortingOrder = 100; // 일반 UI보다 위에 표시
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("[LoadingUI] LoadingCanvas 생성 완료");
            }

            Canvas canvas = loadingCanvas;

            // 배경 패널
            GameObject bgPanel = new GameObject("BackgroundPanel");
            bgPanel.transform.SetParent(canvas.transform);
            Image bgImage = bgPanel.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            // 로딩 텍스트
            GameObject loadingTextObj = new GameObject("LoadingText");
            loadingTextObj.transform.SetParent(canvas.transform);
            Text loadingText = loadingTextObj.AddComponent<Text>();
            loadingText.text = "Loading...";
            loadingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            loadingText.fontSize = 48;
            loadingText.alignment = TextAnchor.MiddleCenter;
            loadingText.color = Color.white;

            RectTransform loadingTextRect = loadingTextObj.GetComponent<RectTransform>();
            loadingTextRect.anchorMin = new Vector2(0.5f, 0.6f);
            loadingTextRect.anchorMax = new Vector2(0.5f, 0.6f);
            loadingTextRect.sizeDelta = new Vector2(400, 80);
            loadingTextRect.anchoredPosition = Vector2.zero;

            // 진행률 바
            if (progressBar == null)
            {
                progressBar = CreateProgressBar(canvas.transform);
            }

            // 진행률 텍스트
            if (progressText == null)
            {
                GameObject progressTextObj = new GameObject("ProgressText");
                progressTextObj.transform.SetParent(canvas.transform);
                progressText = progressTextObj.AddComponent<Text>();
                progressText.text = "0%";
                progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                progressText.fontSize = 24;
                progressText.alignment = TextAnchor.MiddleCenter;
                progressText.color = Color.white;

                RectTransform progressTextRect = progressTextObj.GetComponent<RectTransform>();
                progressTextRect.anchorMin = new Vector2(0.5f, 0.45f);
                progressTextRect.anchorMax = new Vector2(0.5f, 0.45f);
                progressTextRect.sizeDelta = new Vector2(200, 40);
                progressTextRect.anchoredPosition = Vector2.zero;
            }

            // 로딩 팁 텍스트
            if (loadingTipText == null)
            {
                GameObject tipTextObj = new GameObject("TipText");
                tipTextObj.transform.SetParent(canvas.transform);
                loadingTipText = tipTextObj.AddComponent<Text>();
                loadingTipText.text = "TIP: 게임을 시작합니다...";
                loadingTipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                loadingTipText.fontSize = 18;
                loadingTipText.alignment = TextAnchor.MiddleCenter;
                loadingTipText.color = new Color(0.8f, 0.8f, 0.8f, 1f);

                RectTransform tipTextRect = tipTextObj.GetComponent<RectTransform>();
                tipTextRect.anchorMin = new Vector2(0.5f, 0.3f);
                tipTextRect.anchorMax = new Vector2(0.5f, 0.3f);
                tipTextRect.sizeDelta = new Vector2(600, 60);
                tipTextRect.anchoredPosition = Vector2.zero;
            }

            Debug.Log("[LoadingUI] UI 자동 생성 완료");
        }

        /// <summary>
        /// Loading용 Canvas 찾기 (FadeCanvas 제외)
        /// </summary>
        private Canvas FindLoadingCanvas()
        {
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in allCanvases)
            {
                // FadeCanvas가 아닌 Canvas 찾기
                if (c.gameObject.name != "FadeCanvas")
                {
                    Debug.Log($"[LoadingUI] 기존 Canvas 발견: {c.gameObject.name}");
                    return c;
                }
            }
            return null;
        }

        /// <summary>
        /// 진행률 바 생성
        /// </summary>
        private Slider CreateProgressBar(Transform parent)
        {
            GameObject sliderObj = new GameObject("ProgressBar");
            sliderObj.transform.SetParent(parent);

            Slider slider = sliderObj.AddComponent<Slider>();

            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
            sliderRect.sizeDelta = new Vector2(600, 30);
            sliderRect.anchoredPosition = Vector2.zero;

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            // Fill Area
            GameObject fillAreaObj = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaObj.transform.SetParent(sliderObj.transform);

            RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = new Vector2(-10, -10);
            fillAreaRect.anchoredPosition = Vector2.zero;

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform);
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);

            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;

            // Slider 설정
            slider.fillRect = fillRect;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;

            return slider;
        }

        /// <summary>
        /// SceneLoader 이벤트 구독
        /// </summary>
        private void SubscribeToSceneLoader()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.OnLoadProgressChanged += UpdateProgress;
                Debug.Log("[LoadingUI] SceneLoader 이벤트 구독 완료");
            }
        }

        /// <summary>
        /// 진행률 업데이트
        /// </summary>
        public void UpdateProgress(float progress)
        {
            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            if (progressText != null)
            {
                progressText.text = $"{(progress * 100):F0}%";
            }

            // 로딩 완료 확인
            if (progress >= 1f && isLoading)
            {
                CheckLoadingComplete();
            }
        }

        /// <summary>
        /// 로딩 완료 체크 (최소 표시 시간 확인)
        /// </summary>
        private async void CheckLoadingComplete()
        {
            float elapsedTime = Time.time - loadingStartTime;
            if (elapsedTime < minDisplayTime)
            {
                float waitTime = minDisplayTime - elapsedTime;
                await Awaitable.WaitForSecondsAsync(waitTime);
            }

            isLoading = false;
            Debug.Log("[LoadingUI] 로딩 완료!");
        }

        /// <summary>
        /// 랜덤 팁 표시
        /// </summary>
        private void ShowRandomTip()
        {
            if (loadingTipText != null && loadingTips.Length > 0)
            {
                int randomIndex = Random.Range(0, loadingTips.Length);
                loadingTipText.text = $"TIP: {loadingTips[randomIndex]}";
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.OnLoadProgressChanged -= UpdateProgress;
            }
        }
    }
}
