using UnityEngine;
using UnityEngine.UI;
using Combat.Core;
using Core;

namespace UI.HUD
{
    /// <summary>
    /// 게임플레이 중 표시되는 전체 HUD UI 관리
    /// 체력바, 콤보, 점수, 일시정지 버튼 등을 자동 생성
    /// SingletonManager 패턴으로 DontDestroyOnLoad 자동 설정
    /// </summary>
    public class GameplayUI : SingletonManager<GameplayUI>
    {
        [Header("UI 참조")]
        [SerializeField] private HealthBarUI healthBar;
        [SerializeField] private Text comboText;
        [SerializeField] private Text enemyCountText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Button pauseButton;

        [Header("설정")]
        [SerializeField] private bool createUIOnStart = true;
        [SerializeField] private bool showDebugInfo = true;

        // 상태 추적
        private int currentCombo = 0;
        private int enemyCount = 0;
        private int score = 0;

        // 체력 시스템 참조
        private HealthSystem playerHealthSystem;

        // Canvas 참조
        private Canvas gameplayCanvas;

        protected override void OnSingletonAwake()
        {
            if (createUIOnStart)
            {
                CreateUI();
            }

            SetupPauseButton();

            // 초기에는 숨김
            Hide();

            Debug.Log("[GameplayUI] 초기화 완료");
        }

        /// <summary>
        /// 게임플레이 UI 표시 및 플레이어 연결
        /// </summary>
        public void Show()
        {
            if (gameplayCanvas != null)
            {
                gameplayCanvas.gameObject.SetActive(true);
            }

            // 플레이어 HealthSystem 연결 (씬 전환 후 새로운 플레이어 찾기)
            SetupHealthSystem();

            Debug.Log("[GameplayUI] 표시");
        }

        /// <summary>
        /// 게임플레이 UI 숨김
        /// </summary>
        public void Hide()
        {
            if (gameplayCanvas != null)
            {
                gameplayCanvas.gameObject.SetActive(false);
            }

            // 이전 플레이어 연결 해제
            if (playerHealthSystem != null)
            {
                playerHealthSystem.OnHealthChanged -= OnPlayerHealthChanged;
                playerHealthSystem = null;
            }

            Debug.Log("[GameplayUI] 숨김");
        }

        /// <summary>
        /// UI 자동 생성 (빈 씬에서 테스트용)
        /// </summary>
        private void CreateUI()
        {
            // Canvas 찾기 또는 생성 (FadeCanvas 제외)
            gameplayCanvas = FindGameplayCanvas();
            if (gameplayCanvas == null)
            {
                GameObject canvasObj = new GameObject("GameplayCanvas");
                canvasObj.transform.SetParent(transform); // GameplayUI 하위로 설정
                gameplayCanvas = canvasObj.AddComponent<Canvas>();
                gameplayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameplayCanvas.sortingOrder = 50; // 일반 UI와 로딩 UI 사이
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("[GameplayUI] GameplayCanvas 생성 완료");
            }

            Canvas canvas = gameplayCanvas;

            // 체력바 생성 (좌측 상단)
            if (healthBar == null)
            {
                healthBar = CreateHealthBar(canvas.transform);
            }

            // 콤보 텍스트 (중앙 상단)
            if (comboText == null)
            {
                comboText = CreateComboText(canvas.transform);
            }

            // 적 카운트 (우측 상단)
            if (enemyCountText == null)
            {
                enemyCountText = CreateEnemyCountText(canvas.transform);
            }

            // 점수 (좌측 하단)
            if (scoreText == null && showDebugInfo)
            {
                scoreText = CreateScoreText(canvas.transform);
            }

            // 일시정지 버튼 (우측 상단)
            if (pauseButton == null)
            {
                pauseButton = CreatePauseButton(canvas.transform);
            }

            Debug.Log("[GameplayUI] UI 자동 생성 완료");
        }

        /// <summary>
        /// Gameplay용 Canvas 찾기 (FadeCanvas 제외)
        /// </summary>
        private Canvas FindGameplayCanvas()
        {
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in allCanvases)
            {
                // FadeCanvas가 아닌 Canvas 찾기
                if (c.gameObject.name != "FadeCanvas")
                {
                    Debug.Log($"[GameplayUI] 기존 Canvas 발견: {c.gameObject.name}");
                    return c;
                }
            }
            return null;
        }

        /// <summary>
        /// 체력바 생성 (좌측 상단)
        /// </summary>
        private HealthBarUI CreateHealthBar(Transform parent)
        {
            GameObject healthBarObj = new GameObject("HealthBar");
            healthBarObj.transform.SetParent(parent);

            RectTransform rect = healthBarObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -20f);
            rect.sizeDelta = new Vector2(300f, 40f);

            // 배경
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(healthBarObj.transform);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(healthBarObj.transform);
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = new Color(0.6f, 0.2f, 0.8f); // 보라색
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 1f;

            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = new Vector2(-4f, -4f);
            fillRect.anchoredPosition = Vector2.zero;

            // HP 텍스트
            GameObject textObj = new GameObject("HPText");
            textObj.transform.SetParent(healthBarObj.transform);
            Text hpText = textObj.AddComponent<Text>();
            hpText.text = "100/100";
            hpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hpText.fontSize = 20;
            hpText.alignment = TextAnchor.MiddleCenter;
            hpText.color = Color.white;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            // HealthBarUI 컴포넌트 추가
            HealthBarUI healthBarUI = healthBarObj.AddComponent<HealthBarUI>();

            // Reflection으로 필드 설정
            var fillImageField = typeof(HealthBarUI).GetField("fillImage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillImageField?.SetValue(healthBarUI, fillImage);

            var healthTextField = typeof(HealthBarUI).GetField("healthText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthTextField?.SetValue(healthBarUI, hpText);

            return healthBarUI;
        }

        /// <summary>
        /// 콤보 텍스트 생성 (중앙 상단)
        /// </summary>
        private Text CreateComboText(Transform parent)
        {
            GameObject comboObj = new GameObject("ComboText");
            comboObj.transform.SetParent(parent);

            Text combo = comboObj.AddComponent<Text>();
            combo.text = "";
            combo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            combo.fontSize = 48;
            combo.fontStyle = FontStyle.Bold;
            combo.alignment = TextAnchor.MiddleCenter;
            combo.color = new Color(1f, 0.8f, 0f); // 노란색

            RectTransform rect = comboObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -80f);
            rect.sizeDelta = new Vector2(400f, 80f);

            comboObj.SetActive(false); // 초기에는 숨김

            return combo;
        }

        /// <summary>
        /// 적 카운트 텍스트 생성 (우측 상단)
        /// </summary>
        private Text CreateEnemyCountText(Transform parent)
        {
            GameObject enemyObj = new GameObject("EnemyCountText");
            enemyObj.transform.SetParent(parent);

            Text enemy = enemyObj.AddComponent<Text>();
            enemy.text = "적: 0";
            enemy.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            enemy.fontSize = 24;
            enemy.alignment = TextAnchor.MiddleRight;
            enemy.color = Color.white;

            RectTransform rect = enemyObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-20f, -80f);
            rect.sizeDelta = new Vector2(200f, 40f);

            return enemy;
        }

        /// <summary>
        /// 점수 텍스트 생성 (좌측 하단)
        /// </summary>
        private Text CreateScoreText(Transform parent)
        {
            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(parent);

            Text scoreText = scoreObj.AddComponent<Text>();
            scoreText.text = "점수: 0";
            scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            scoreText.fontSize = 24;
            scoreText.alignment = TextAnchor.MiddleLeft;
            scoreText.color = Color.white;

            RectTransform rect = scoreObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(20f, 20f);
            rect.sizeDelta = new Vector2(200f, 40f);

            return scoreText;
        }

        /// <summary>
        /// 일시정지 버튼 생성 (우측 상단)
        /// </summary>
        private Button CreatePauseButton(Transform parent)
        {
            GameObject buttonObj = new GameObject("PauseButton");
            buttonObj.transform.SetParent(parent);

            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-20f, -20f);
            rect.sizeDelta = new Vector2(80f, 40f);

            // 버튼 텍스트
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = "II"; // 일시정지 아이콘
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 24;
            buttonText.fontStyle = FontStyle.Bold;
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
        /// 플레이어 체력 시스템 연결
        /// </summary>
        private void SetupHealthSystem()
        {
            // 플레이어 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[GameplayUI] 플레이어를 찾을 수 없습니다. Tag를 확인하세요.");
                return;
            }

            // HealthSystem 찾기
            playerHealthSystem = player.GetComponent<HealthSystem>();
            if (playerHealthSystem != null && healthBar != null)
            {
                // 초기 체력 설정
                healthBar.Initialize(playerHealthSystem.CurrentHealth, playerHealthSystem.MaxHealth);

                // 체력 변경 이벤트 구독
                playerHealthSystem.OnHealthChanged += OnPlayerHealthChanged;
                Debug.Log("[GameplayUI] 플레이어 HealthSystem 연결 완료");
            }
        }

        /// <summary>
        /// 일시정지 버튼 설정
        /// </summary>
        private void SetupPauseButton()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
            }
        }

        /// <summary>
        /// 플레이어 체력 변경 이벤트 핸들러
        /// </summary>
        private void OnPlayerHealthChanged(float current, float max)
        {
            if (healthBar != null)
            {
                healthBar.UpdateHealth(current, max);
            }
        }

        /// <summary>
        /// 일시정지 버튼 클릭
        /// </summary>
        private void OnPauseButtonClicked()
        {
            Debug.Log("[GameplayUI] 일시정지 버튼 클릭");

            // GameFlowManager를 통해 일시정지
            if (GameFlow.GameFlowManager.Instance != null)
            {
                GameFlow.GameFlowManager.Instance.PauseGame();
            }
            else
            {
                Debug.LogWarning("[GameplayUI] GameFlowManager가 없습니다!");
            }
        }

        #region 공개 API

        /// <summary>
        /// 콤보 업데이트
        /// </summary>
        public void UpdateCombo(int combo)
        {
            currentCombo = combo;

            if (comboText != null)
            {
                if (combo > 1)
                {
                    comboText.text = $"{combo} COMBO!";
                    comboText.gameObject.SetActive(true);
                }
                else
                {
                    comboText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 적 카운트 업데이트
        /// </summary>
        public void UpdateEnemyCount(int count)
        {
            enemyCount = count;

            if (enemyCountText != null)
            {
                enemyCountText.text = $"적: {count}";
            }
        }

        /// <summary>
        /// 점수 업데이트
        /// </summary>
        public void UpdateScore(int newScore)
        {
            score = newScore;

            if (scoreText != null)
            {
                scoreText.text = $"점수: {score}";
            }
        }

        /// <summary>
        /// 점수 추가
        /// </summary>
        public void AddScore(int amount)
        {
            UpdateScore(score + amount);
        }

        #endregion

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (playerHealthSystem != null)
            {
                playerHealthSystem.OnHealthChanged -= OnPlayerHealthChanged;
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
            }
        }
    }
}
