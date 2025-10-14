using UnityEngine;
using UnityEngine.UI;
using Player;
using Enemy;
using Enemy.Data;
using GameFlow;
using Combat.Core;
using Core.Data;
using Core.Enums;
using System.Threading;
using System.IO;

namespace Tests
{
    /// <summary>
    /// 전체 게임 플레이 데모
    /// Preload -> Main -> Loading -> Ingame + 캐릭터 이동 + 전투
    /// </summary>
    public class FullGamePlayDemo : MonoBehaviour
    {
        [Header("데모 설정")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private bool skipToIngame = false; // 바로 Ingame으로 시작

        [Header("리소스 설정")]
        [SerializeField] private bool useResourceManifest = false;
        [Tooltip("리소스 매니페스트를 사용할 때 생성할지 여부")]
        [SerializeField] private bool createManifestsIfMissing = true;

        [Header("캐릭터 설정")]
        [SerializeField] private Vector3 playerSpawnPosition = new Vector3(-8f, 2f, 0f);
        [SerializeField] private Vector3 enemy1SpawnPosition = new Vector3(5f, 2f, 0f);
        [SerializeField] private Vector3 enemy2SpawnPosition = new Vector3(10f, 2f, 0f);

        [Header("Enemy 데이터")]
        [SerializeField] private EnemyData enemyData;

        [Header("환경 설정")]
        [SerializeField] private bool createGround = true;
        [SerializeField] private Vector3 groundPosition = new Vector3(0f, -1f, 0f);
        [SerializeField] private Vector2 groundSize = new Vector2(30f, 2f);

        // 참조
        private GameFlowManager gameFlowManager;
        private PlayerController player;
        private EnemyController enemy1;
        private EnemyController enemy2;
        private Canvas uiCanvas;
        private Text statusText;

        // UI 요소들
        private GameObject mainMenuUI;
        private GameObject loadingScreenUI;
        private GameObject ingameUI;
        private Text loadingProgressText;
        private Slider loadingProgressBar;
        private Text playerHealthText;
        private Text enemy1HealthText;
        private Text enemy2HealthText;

        private void Start()
        {
            if (autoStart)
            {
                _ = InitializeDemo();
            }
        }

        private async Awaitable InitializeDemo()
        {
            Debug.Log("========================================");
            Debug.Log("=== 전체 게임 플레이 데모 시작 ===");
            Debug.Log("========================================");

            // 1. 환경 설정
            SetupEnvironment();
            await Awaitable.WaitForSecondsAsync(0.5f);

            // 2. UI 생성
            SetupUI();
            await Awaitable.WaitForSecondsAsync(0.5f);

            // 3. GameFlow 시스템 시작
            if (skipToIngame)
            {
                await StartDirectlyToIngame();
            }
            else
            {
                await StartFullGameFlow();
            }

            Debug.Log("========================================");
            Debug.Log("=== 데모 초기화 완료 ===");
            Debug.Log("플레이어 조작:");
            Debug.Log("- WASD: 이동");
            Debug.Log("- Space: 점프");
            Debug.Log("- Shift: 대시");
            Debug.Log("- 마우스 좌클릭: 공격");
            Debug.Log("========================================");
        }

        #region 환경 설정

        private void SetupEnvironment()
        {
            Debug.Log("[환경] 게임 환경 설정 중...");

            // 지면 생성
            if (createGround)
            {
                CreateGround();
            }

            // 카메라 설정
            SetupCamera();

            Debug.Log("[환경] 게임 환경 설정 완료");
        }

        private void CreateGround()
        {
            GameObject ground = new GameObject("Ground");
            ground.transform.position = groundPosition;
            ground.layer = LayerMask.NameToLayer("Default");

            // BoxCollider2D 추가
            BoxCollider2D groundCollider = ground.AddComponent<BoxCollider2D>();
            groundCollider.size = groundSize;

            // 시각적 표현 (SpriteRenderer)
            SpriteRenderer groundSprite = ground.AddComponent<SpriteRenderer>();
            groundSprite.color = new Color(0.3f, 0.3f, 0.3f, 1f); // 회색

            // Sprite 생성 (사각형)
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            groundSprite.sprite = sprite;
            groundSprite.drawMode = SpriteDrawMode.Sliced;
            groundSprite.size = groundSize;

            Debug.Log($"[환경] 지면 생성 완료: {groundPosition}, 크기: {groundSize}");
        }

        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0f, 2f, -10f);
                mainCamera.orthographicSize = 8f;
                mainCamera.backgroundColor = new Color(0.2f, 0.3f, 0.4f); // 어두운 파란색
            }
        }

        #endregion

        #region UI 설정

        private void SetupUI()
        {
            Debug.Log("[UI] UI 시스템 설정 중...");

            // Canvas 생성
            GameObject canvasGO = new GameObject("DemoCanvas");
            uiCanvas = canvasGO.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Main Menu UI
            CreateMainMenuUI();

            // Loading Screen UI
            CreateLoadingScreenUI();

            // Ingame UI
            CreateIngameUI();

            // Status Text (디버그용)
            CreateStatusText();

            // 초기에는 모든 UI 비활성화
            HideAllUI();

            Debug.Log("[UI] UI 시스템 설정 완료");
        }

        private void CreateMainMenuUI()
        {
            mainMenuUI = CreateUIPanel(uiCanvas.gameObject, "MainMenuUI", new Color(0.1f, 0.1f, 0.2f, 0.95f));

            // Title Text
            CreateUIText(mainMenuUI, "TitleText", "GASPT DEMO", 48, new Vector2(0, 100));

            // Start Button
            GameObject startButton = CreateUIButton(mainMenuUI, "StartButton", "게임 시작", new Vector2(0, 0));
            Button startButtonComp = startButton.GetComponent<Button>();
            startButtonComp.onClick.AddListener(() =>
            {
                Debug.Log("[Main Menu] 게임 시작 버튼 클릭");
                gameFlowManager?.StartGame();
            });

            // Info Text
            CreateUIText(mainMenuUI, "InfoText",
                "플레이어 조작:\nWASD - 이동 | Space - 점프 | Shift - 대시 | 좌클릭 - 공격",
                18, new Vector2(0, -100));
        }

        private void CreateLoadingScreenUI()
        {
            loadingScreenUI = CreateUIPanel(uiCanvas.gameObject, "LoadingScreenUI", new Color(0.05f, 0.05f, 0.1f, 1f));

            // Loading Text
            CreateUIText(loadingScreenUI, "LoadingText", "로딩 중...", 36, new Vector2(0, 50));

            // Progress Bar Background
            GameObject progressBarBG = CreateUIPanel(loadingScreenUI, "ProgressBarBG", new Color(0.2f, 0.2f, 0.2f, 1f));
            RectTransform progressBarBGRect = progressBarBG.GetComponent<RectTransform>();
            progressBarBGRect.sizeDelta = new Vector2(400, 30);
            progressBarBGRect.anchoredPosition = new Vector2(0, -20);

            // Progress Bar
            GameObject progressBar = new GameObject("ProgressBar");
            progressBar.transform.SetParent(loadingScreenUI.transform, false);
            loadingProgressBar = progressBar.AddComponent<Slider>();
            RectTransform sliderRect = progressBar.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(400, 30);
            sliderRect.anchoredPosition = new Vector2(0, -20);

            // Background
            GameObject background = CreateUIPanel(progressBar, "Background", new Color(0.2f, 0.2f, 0.2f, 1f));
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // Fill
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(progressBar.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = new Vector2(-10, -10);

            GameObject fill = CreateUIPanel(fillArea, "Fill", new Color(0.2f, 0.8f, 0.2f, 1f));
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            // Slider 설정
            loadingProgressBar.fillRect = fillRect;
            loadingProgressBar.minValue = 0f;
            loadingProgressBar.maxValue = 1f;
            loadingProgressBar.value = 0f;

            // Progress Text
            GameObject progressTextGO = CreateUIText(loadingScreenUI, "ProgressText", "0%", 24, new Vector2(0, -70));
            loadingProgressText = progressTextGO.GetComponent<Text>();
        }

        private void CreateIngameUI()
        {
            ingameUI = new GameObject("IngameUI");
            ingameUI.transform.SetParent(uiCanvas.transform, false);

            // Player Health Text
            GameObject playerHealthGO = CreateUIText(ingameUI, "PlayerHealthText", "Player HP: 100", 20, new Vector2(-400, 250), TextAnchor.UpperLeft);
            playerHealthText = playerHealthGO.GetComponent<Text>();

            // Enemy 1 Health Text
            GameObject enemy1HealthGO = CreateUIText(ingameUI, "Enemy1HealthText", "Enemy1 HP: 100", 20, new Vector2(-400, 220), TextAnchor.UpperLeft);
            enemy1HealthText = enemy1HealthGO.GetComponent<Text>();

            // Enemy 2 Health Text
            GameObject enemy2HealthGO = CreateUIText(ingameUI, "Enemy2HealthText", "Enemy2 HP: 100", 20, new Vector2(-400, 190), TextAnchor.UpperLeft);
            enemy2HealthText = enemy2HealthGO.GetComponent<Text>();

            // Controls Text
            CreateUIText(ingameUI, "ControlsText",
                "조작: WASD-이동 | Space-점프 | Shift-대시 | 좌클릭-공격 | ESC-일시정지",
                16, new Vector2(0, -280), TextAnchor.LowerCenter);
        }

        private void CreateStatusText()
        {
            GameObject statusGO = CreateUIText(uiCanvas.gameObject, "StatusText", "", 18, new Vector2(400, 250), TextAnchor.UpperRight);
            statusText = statusGO.GetComponent<Text>();
            statusText.color = Color.yellow;
        }

        private GameObject CreateUIPanel(GameObject parent, string name, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent.transform, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            Image image = panel.AddComponent<Image>();
            image.color = color;

            return panel;
        }

        private GameObject CreateUIText(GameObject parent, string name, string text, int fontSize, Vector2 position, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent.transform, false);

            RectTransform rect = textGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 100);
            rect.anchoredPosition = position;

            Text textComp = textGO.AddComponent<Text>();
            textComp.text = text;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = fontSize;
            textComp.color = Color.white;
            textComp.alignment = alignment;
            textComp.horizontalOverflow = HorizontalWrapMode.Overflow;
            textComp.verticalOverflow = VerticalWrapMode.Overflow;

            return textGO;
        }

        private GameObject CreateUIButton(GameObject parent, string name, string text, Vector2 position)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent.transform, false);

            RectTransform rect = buttonGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 50);
            rect.anchoredPosition = position;

            Image image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.6f, 0.2f, 1f);

            Button button = buttonGO.AddComponent<Button>();

            // Button Text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            Text textComp = textGO.AddComponent<Text>();
            textComp.text = text;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 24;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleCenter;

            return buttonGO;
        }

        private void HideAllUI()
        {
            mainMenuUI?.SetActive(false);
            loadingScreenUI?.SetActive(false);
            ingameUI?.SetActive(false);
        }

        #endregion

        #region GameFlow 시작

        private async Awaitable StartFullGameFlow()
        {
            Debug.Log("[GameFlow] 전체 게임 흐름 시작");

            // 리소스 매니페스트 설정
            if (useResourceManifest)
            {
                Debug.Log("[리소스] 리소스 매니페스트 사용 모드");
                if (createManifestsIfMissing)
                {
                    CreateResourceManifests();
                }
            }
            else
            {
                Debug.Log("[리소스] 리소스 매니페스트 미사용 모드 (경고만 출력)");
            }

            // GameFlowManager 생성
            GameObject gameFlowGO = new GameObject("GameFlowManager");
            gameFlowManager = gameFlowGO.AddComponent<GameFlowManager>();

            // autoStart를 false로 설정 (UI 연결 후 수동으로 시작하기 위함)
            var autoStartField = typeof(GameFlowManager).GetField("autoStart",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            autoStartField?.SetValue(gameFlowManager, false);

            // 이벤트 구독
            gameFlowManager.OnStateChanged += OnGameStateChanged;

            // UI 연결 (리플렉션 사용)
            ConnectUIToGameFlowManager();

            // 프레임 대기 (GameFlowManager.Awake()가 완전히 실행되도록 보장)
            await Awaitable.NextFrameAsync();

            // 수동으로 Preload 상태 시작
            Debug.Log("[GameFlow] Preload 상태 수동 시작");
            gameFlowManager.StartManually(GameStateType.Preload);
        }

        /// <summary>
        /// 리소스 매니페스트 생성 (런타임용 - 에디터에서만 동작)
        /// </summary>
        private void CreateResourceManifests()
        {
#if UNITY_EDITOR
            Debug.Log("[리소스] 리소스 매니페스트 생성 중...");

            string manifestPath = "Assets/_Project/Resources/Manifests";

            // 폴더 생성
            if (!Directory.Exists(manifestPath))
            {
                Directory.CreateDirectory(manifestPath);
                UnityEditor.AssetDatabase.Refresh();
                Debug.Log($"[리소스] 매니페스트 폴더 생성: {manifestPath}");
            }

            // Essential Manifest
            CreateManifestIfNotExists(
                $"{manifestPath}/EssentialManifest.asset",
                ResourceCategory.Essential,
                "필수 리소스",
                0.5f
            );

            // MainMenu Manifest
            CreateManifestIfNotExists(
                $"{manifestPath}/MainMenuManifest.asset",
                ResourceCategory.MainMenu,
                "메인 메뉴 리소스",
                0.5f
            );

            // Gameplay Manifest
            CreateManifestIfNotExists(
                $"{manifestPath}/GameplayManifest.asset",
                ResourceCategory.Gameplay,
                "게임플레이 리소스",
                1.0f
            );

            // Common Manifest
            CreateManifestIfNotExists(
                $"{manifestPath}/CommonManifest.asset",
                ResourceCategory.Common,
                "공통 리소스",
                0.3f
            );

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log("[리소스] 리소스 매니페스트 생성 완료");
#else
            Debug.LogWarning("[리소스] 런타임에서는 매니페스트를 생성할 수 없습니다. 에디터에서 미리 생성하세요.");
#endif
        }

#if UNITY_EDITOR
        private void CreateManifestIfNotExists(string path, ResourceCategory category, string displayName, float minimumLoadTime)
        {
            // 이미 존재하면 스킵
            ResourceManifest existing = UnityEditor.AssetDatabase.LoadAssetAtPath<ResourceManifest>(path);
            if (existing != null)
            {
                Debug.Log($"[리소스] 매니페스트 이미 존재: {path}");
                return;
            }

            // 새로 생성
            ResourceManifest manifest = ScriptableObject.CreateInstance<ResourceManifest>();
            manifest.category = category;
            manifest.displayName = displayName;
            manifest.useAsyncLoading = true;
            manifest.minimumLoadTime = minimumLoadTime;
            manifest.resources.Clear();

            // 카테고리별 기본 리소스 추가 (예시)
            switch (category)
            {
                case ResourceCategory.Essential:
                    // 예: manifest.AddResource("Data/SkulPhysicsConfig", "Player.Physics.SkulPhysicsConfig", "플레이어 물리 설정");
                    break;
                case ResourceCategory.MainMenu:
                    // 예: manifest.AddResource("UI/MainMenuPanel", "GameObject", "메인 메뉴 패널");
                    break;
                case ResourceCategory.Gameplay:
                    // 예: manifest.AddResource("Prefabs/Player", "GameObject", "플레이어 프리팹");
                    break;
                case ResourceCategory.Common:
                    // 예: manifest.AddResource("VFX/HitEffect", "GameObject", "피격 이펙트");
                    break;
            }

            UnityEditor.AssetDatabase.CreateAsset(manifest, path);
            UnityEditor.EditorUtility.SetDirty(manifest);

            Debug.Log($"[리소스] 매니페스트 생성: {path}");
        }
#endif


        private async Awaitable StartDirectlyToIngame()
        {
            Debug.Log("[GameFlow] 바로 Ingame으로 시작");

            // 캐릭터 생성
            await CreateCharacters();

            // Ingame UI 표시
            ShowIngameUI();

            // 체력 UI 업데이트 시작
            _ = UpdateHealthUI();
        }

        private void ConnectUIToGameFlowManager()
        {
            // 리플렉션을 사용하여 private 필드에 UI 연결
            var type = typeof(GameFlowManager);

            var mainMenuField = type.GetField("mainMenuUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            mainMenuField?.SetValue(gameFlowManager, mainMenuUI);

            var loadingScreenField = type.GetField("loadingScreenUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loadingScreenField?.SetValue(gameFlowManager, loadingScreenUI);

            var ingameUIField = type.GetField("ingameUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ingameUIField?.SetValue(gameFlowManager, ingameUI);

            var loadingProgressBarField = type.GetField("loadingProgressBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loadingProgressBarField?.SetValue(gameFlowManager, loadingProgressBar);

            var loadingTextField = type.GetField("loadingText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loadingTextField?.SetValue(gameFlowManager, loadingProgressText);

            Debug.Log("[GameFlow] UI 연결 완료");
        }

        private void OnGameStateChanged(GameStateType fromState, GameStateType toState)
        {
            Debug.Log($"[GameFlow] 상태 변경: {fromState} → {toState}");
            UpdateStatusText($"GameState: {toState}");

            // Ingame 상태로 전환되면 캐릭터 생성
            if (toState == GameStateType.Ingame && player == null)
            {
                _ = CreateCharacters();
                _ = UpdateHealthUI();
            }
        }

        #endregion

        #region 캐릭터 생성

        private async Awaitable CreateCharacters()
        {
            Debug.Log("[캐릭터] 캐릭터 생성 중...");

            // Player 생성
            await CreatePlayer();

            // Enemy 생성
            await CreateEnemies();

            Debug.Log("[캐릭터] 캐릭터 생성 완료");
        }

        private async Awaitable CreatePlayer()
        {
            GameObject playerGO = new GameObject("Player");
            playerGO.transform.position = playerSpawnPosition;
            playerGO.tag = "Player";

            // 필수 컴포넌트
            Rigidbody2D playerRb = playerGO.AddComponent<Rigidbody2D>();
            playerRb.gravityScale = 3f;
            playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            BoxCollider2D playerCollider = playerGO.AddComponent<BoxCollider2D>();
            playerCollider.size = new Vector2(1f, 2f);

            // 시각적 표현
            SpriteRenderer playerSprite = playerGO.AddComponent<SpriteRenderer>();
            playerSprite.color = new Color(0.2f, 0.6f, 1f, 1f); // 파란색
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            playerSprite.sprite = sprite;
            playerSprite.drawMode = SpriteDrawMode.Sliced;
            playerSprite.size = new Vector2(1f, 2f);

            // PlayerController 추가
            player = playerGO.AddComponent<PlayerController>();

            await Awaitable.WaitForSecondsAsync(0.5f);

            Debug.Log($"[캐릭터] Player 생성 완료: {playerSpawnPosition}");
        }

        private async Awaitable CreateEnemies()
        {
            // Enemy Data가 없으면 기본 데이터 생성
            if (enemyData == null)
            {
                enemyData = ScriptableObject.CreateInstance<EnemyData>();
                enemyData.enemyName = "Test Enemy";
                enemyData.maxHealth = 100f;
                enemyData.moveSpeed = 3f;
                enemyData.detectionRange = 10f;
                enemyData.chaseRange = 12f;
                enemyData.attackRange = 2f;
                enemyData.attackDamage = 10f;
                enemyData.attackCooldown = 1.5f;
                enemyData.detectionInterval = 0.5f;
                enemyData.enablePatrol = true;
                enemyData.patrolRange = 5f;
                enemyData.enemyColor = new Color(1f, 0.2f, 0.2f, 1f); // 빨간색
            }

            // Enemy 1
            enemy1 = CreateSingleEnemy("Enemy1", enemy1SpawnPosition, enemyData);

            await Awaitable.WaitForSecondsAsync(0.3f);

            // Enemy 2
            enemy2 = CreateSingleEnemy("Enemy2", enemy2SpawnPosition, enemyData);

            await Awaitable.WaitForSecondsAsync(0.3f);

            Debug.Log("[캐릭터] Enemy 생성 완료");
        }

        private EnemyController CreateSingleEnemy(string name, Vector3 position, EnemyData data)
        {
            GameObject enemyGO = new GameObject(name);
            enemyGO.transform.position = position;
            enemyGO.tag = "Enemy";

            // 필수 컴포넌트
            Rigidbody2D enemyRb = enemyGO.AddComponent<Rigidbody2D>();
            enemyRb.gravityScale = 3f;
            enemyRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            BoxCollider2D enemyCollider = enemyGO.AddComponent<BoxCollider2D>();
            enemyCollider.size = new Vector2(1f, 2f);

            // 시각적 표현
            SpriteRenderer enemySprite = enemyGO.AddComponent<SpriteRenderer>();
            enemySprite.color = data.enemyColor;
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            enemySprite.sprite = sprite;
            enemySprite.drawMode = SpriteDrawMode.Sliced;
            enemySprite.size = new Vector2(1f, 2f);

            // EnemyController 추가
            EnemyController enemy = enemyGO.AddComponent<EnemyController>();

            // EnemyData 설정 (리플렉션)
            var dataField = typeof(EnemyController).GetField("enemyData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dataField?.SetValue(enemy, data);

            // Player를 타겟으로 설정
            if (player != null)
            {
                enemy.Target = player.transform;
            }

            Debug.Log($"[캐릭터] {name} 생성 완료: {position}");

            return enemy;
        }

        #endregion

        #region UI 업데이트

        private async Awaitable UpdateHealthUI()
        {
            while (true)
            {
                await Awaitable.WaitForSecondsAsync(0.1f);

                if (playerHealthText != null && player != null && player.HealthSystem != null)
                {
                    playerHealthText.text = $"Player HP: {player.HealthSystem.CurrentHealth:F0} / {player.HealthSystem.MaxHealth:F0}";
                    if (!player.IsAlive)
                    {
                        playerHealthText.text += " [사망]";
                        playerHealthText.color = Color.red;
                    }
                    else
                    {
                        playerHealthText.color = Color.white;
                    }
                }

                if (enemy1HealthText != null && enemy1 != null && enemy1.Health != null)
                {
                    enemy1HealthText.text = $"Enemy1 HP: {enemy1.Health.CurrentHealth:F0} / {enemy1.Health.MaxHealth:F0}";
                    if (!enemy1.IsAlive)
                    {
                        enemy1HealthText.text += " [사망]";
                        enemy1HealthText.color = Color.red;
                    }
                    else
                    {
                        enemy1HealthText.color = Color.white;
                    }
                }

                if (enemy2HealthText != null && enemy2 != null && enemy2.Health != null)
                {
                    enemy2HealthText.text = $"Enemy2 HP: {enemy2.Health.CurrentHealth:F0} / {enemy2.Health.MaxHealth:F0}";
                    if (!enemy2.IsAlive)
                    {
                        enemy2HealthText.text += " [사망]";
                        enemy2HealthText.color = Color.red;
                    }
                    else
                    {
                        enemy2HealthText.color = Color.white;
                    }
                }
            }
        }

        private void UpdateStatusText(string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }

        private void ShowIngameUI()
        {
            HideAllUI();
            ingameUI?.SetActive(true);
        }

        #endregion

        #region Context Menu

        [ContextMenu("데모 시작")]
        public void StartDemo()
        {
            _ = InitializeDemo();
        }

        [ContextMenu("캐릭터만 생성")]
        public void CreateCharactersOnly()
        {
            _ = CreateCharacters();
        }

        [ContextMenu("리소스 매니페스트 생성")]
        public void CreateManifestsManually()
        {
            CreateResourceManifests();
        }

        #endregion

        private void OnDestroy()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.OnStateChanged -= OnGameStateChanged;
            }
        }
    }
}
