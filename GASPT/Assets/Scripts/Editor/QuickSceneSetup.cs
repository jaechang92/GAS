using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Editor
{
    /// <summary>
    /// 빠른 씬 설정을 위한 에디터 스크립트
    /// 메뉴에서 바로 접근 가능
    /// </summary>
    public class QuickSceneSetup
    {
        private const string SCENE_FOLDER = "Assets/Scenes/";

        [MenuItem("GASPT/Quick Setup/Create All Scenes")]
        public static void CreateAllScenes()
        {
            string[] sceneNames = {
                "00_Bootstrap",
                "01_MainMenu",
                "02_LevelSelect",
                "03_Gameplay",
                "04_PauseMenu",
                "05_Settings",
                "06_GameOver",
                "07_LevelComplete"
            };

            // Scenes 폴더 생성
            if (!System.IO.Directory.Exists(SCENE_FOLDER))
            {
                System.IO.Directory.CreateDirectory(SCENE_FOLDER);
            }

            int createdCount = 0;

            foreach (string sceneName in sceneNames)
            {
                string scenePath = $"{SCENE_FOLDER}{sceneName}.unity";

                if (!System.IO.File.Exists(scenePath))
                {
                    // 새 씬 생성
                    Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(newScene, scenePath);
                    createdCount++;
                    Debug.Log($"[QuickSetup] 씬 생성: {sceneName}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[QuickSetup] 총 {createdCount}개 씬 생성 완료");

            // 빌드 설정에 추가
            AddScenesToBuildSettings(sceneNames);
        }

        [MenuItem("GASPT/Quick Setup/Setup Bootstrap Scene")]
        public static void SetupBootstrapScene()
        {
            OpenSceneAndSetup("00_Bootstrap", () => {
                // SystemBootstrap 오브젝트 생성
                GameObject bootstrapGO = new GameObject("SystemBootstrap");
                bootstrapGO.AddComponent<Scenes.Bootstrap.BootstrapManager>();

                Debug.Log("[QuickSetup] Bootstrap 씬 설정 완료");
            });
        }

        [MenuItem("GASPT/Quick Setup/Setup MainMenu Scene")]
        public static void SetupMainMenuScene()
        {
            OpenSceneAndSetup("01_MainMenu", () => {
                // EventSystem 생성
                CreateEventSystem();

                // Main Canvas 생성
                GameObject canvasGO = CreateMainCanvas();

                // MainMenu Manager 생성
                GameObject managerGO = new GameObject("MainMenuManager");
                managerGO.AddComponent<Scenes.MainMenu.MainMenuUI>();

                // 타이틀 텍스트 생성
                CreateTitleText(canvasGO);

                // 메뉴 버튼들 생성
                CreateMenuButtons(canvasGO);

                Debug.Log("[QuickSetup] MainMenu 씬 설정 완료");
            });
        }

        [MenuItem("GASPT/Quick Setup/Setup Gameplay Scene")]
        public static void SetupGameplayScene()
        {
            OpenSceneAndSetup("03_Gameplay", () => {
                // Main Camera 설정
                SetupMainCamera();

                // Player 생성
                CreatePlayer();

                // Ground 플랫폼 생성
                CreateGround();

                // Gameplay UI
                CreateEventSystem();
                CreateGameplayCanvas();

                Debug.Log("[QuickSetup] Gameplay 씬 설정 완료");
            });
        }

        [MenuItem("GASPT/Quick Setup/Setup All Core Scenes")]
        public static void SetupAllCoreScenes()
        {
            SetupBootstrapScene();
            SetupMainMenuScene();
            SetupGameplayScene();

            Debug.Log("[QuickSetup] 모든 핵심 씬 설정 완료");
        }

        // 유틸리티 메서드들
        private static void OpenSceneAndSetup(string sceneName, System.Action setupAction)
        {
            string scenePath = $"{SCENE_FOLDER}{sceneName}.unity";

            if (!System.IO.File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("오류", $"{sceneName} 씬이 존재하지 않습니다.\n먼저 'Create All Scenes'를 실행해주세요.", "확인");
                return;
            }

            // 현재 씬 저장 확인
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                if (EditorUtility.DisplayDialog("씬 저장", "현재 씬에 저장되지 않은 변경사항이 있습니다.\n저장하시겠습니까?", "저장", "저장하지 않음"))
                {
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                }
            }

            // 씬 열기
            EditorSceneManager.OpenScene(scenePath);

            // 설정 작업 실행
            setupAction?.Invoke();

            // 씬 저장
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        private static void CreateEventSystem()
        {
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private static GameObject CreateMainCanvas()
        {
            GameObject canvasGO = new GameObject("MainCanvas");

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var canvasScaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            return canvasGO;
        }

        private static void CreateTitleText(GameObject canvas)
        {
            GameObject titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(canvas.transform, false);

            var rectTransform = titleGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(800, 100);
            rectTransform.anchoredPosition = new Vector2(0, 300);

            var text = titleGO.AddComponent<UnityEngine.UI.Text>();
            text.text = "GASPT 플랫포머";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 48;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
        }

        private static void CreateMenuButtons(GameObject canvas)
        {
            string[] buttonData = {
                "StartGameButton|게임 시작",
                "LevelSelectButton|레벨 선택",
                "SettingsButton|설정",
                "CreditsButton|크레딧",
                "QuitButton|종료"
            };

            for (int i = 0; i < buttonData.Length; i++)
            {
                string[] parts = buttonData[i].Split('|');
                string buttonName = parts[0];
                string buttonText = parts[1];

                GameObject buttonGO = CreateMenuButton(buttonName, buttonText, canvas);

                var rectTransform = buttonGO.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(300, 60);
                rectTransform.anchoredPosition = new Vector2(0, 100 - i * 80);
            }
        }

        private static GameObject CreateMenuButton(string name, string text, GameObject parent)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent.transform, false);

            var rectTransform = buttonGO.AddComponent<RectTransform>();
            var button = buttonGO.AddComponent<UnityEngine.UI.Button>();
            var image = buttonGO.AddComponent<UnityEngine.UI.Image>();

            // 기본 UI 스프라이트 사용
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = new Color(0.8f, 0.8f, 0.8f, 1f);

            // 버튼 텍스트
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            var textRect = textGO.AddComponent<RectTransform>();
            textRect.sizeDelta = Vector2.zero;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;

            var textComponent = textGO.AddComponent<UnityEngine.UI.Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 20;
            textComponent.color = Color.black;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.fontStyle = FontStyle.Bold;

            return buttonGO;
        }

        private static void SetupMainCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraGO = new GameObject("Main Camera");
                mainCamera = cameraGO.AddComponent<Camera>();
                cameraGO.tag = "MainCamera";
                cameraGO.AddComponent<AudioListener>();
            }

            // 2D 플랫포머를 위한 카메라 설정
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5f;
            mainCamera.transform.position = new Vector3(0, 0, -10);
            mainCamera.backgroundColor = new Color(0.3f, 0.5f, 0.8f); // 하늘색 배경
        }

        private static void CreatePlayer()
        {
            GameObject playerGO = new GameObject("Player");
            playerGO.transform.position = new Vector3(0, 1, 0);
            playerGO.tag = "Player";

            // Rigidbody2D 추가
            var rb = playerGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // CapsuleCollider2D 추가 (플레이어에게 더 적합)
            var collider = playerGO.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.8f, 1.8f);

            // 기본 스프라이트 추가
            var spriteRenderer = playerGO.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.blue;
            spriteRenderer.sortingOrder = 10;

            // 간단한 사각형 스프라이트 생성
            CreateSimpleSprite(spriteRenderer, new Vector2(0.8f, 1.8f));
        }

        private static void CreateGround()
        {
            GameObject groundGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            groundGO.name = "Ground";
            groundGO.transform.position = new Vector3(0, -3, 0);
            groundGO.transform.localScale = new Vector3(20, 1, 1);
            groundGO.tag = "Ground";

            // 3D Collider 제거하고 2D Collider 추가
            Object.DestroyImmediate(groundGO.GetComponent<BoxCollider>());
            groundGO.AddComponent<BoxCollider2D>();

            // 색상 변경
            var renderer = groundGO.GetComponent<Renderer>();
            renderer.material.color = Color.green;
            renderer.material.shader = Shader.Find("Sprites/Default");
        }

        private static void CreateGameplayCanvas()
        {
            GameObject canvasGO = CreateMainCanvas();
            canvasGO.name = "GameplayCanvas";

            // 기본 게임 UI 요소들 생성
            CreateGameplayUI(canvasGO);
        }

        private static void CreateGameplayUI(GameObject canvas)
        {
            // 점수 텍스트
            GameObject scoreGO = new GameObject("ScoreText");
            scoreGO.transform.SetParent(canvas.transform, false);

            var scoreRect = scoreGO.AddComponent<RectTransform>();
            scoreRect.sizeDelta = new Vector2(200, 50);
            scoreRect.anchorMin = new Vector2(0, 1);
            scoreRect.anchorMax = new Vector2(0, 1);
            scoreRect.anchoredPosition = new Vector2(100, -25);

            var scoreText = scoreGO.AddComponent<UnityEngine.UI.Text>();
            scoreText.text = "Score: 0";
            scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            scoreText.fontSize = 24;
            scoreText.color = Color.white;
            scoreText.alignment = TextAnchor.MiddleLeft;

            // 생명 텍스트
            GameObject livesGO = new GameObject("LivesText");
            livesGO.transform.SetParent(canvas.transform, false);

            var livesRect = livesGO.AddComponent<RectTransform>();
            livesRect.sizeDelta = new Vector2(200, 50);
            livesRect.anchorMin = new Vector2(1, 1);
            livesRect.anchorMax = new Vector2(1, 1);
            livesRect.anchoredPosition = new Vector2(-100, -25);

            var livesText = livesGO.AddComponent<UnityEngine.UI.Text>();
            livesText.text = "Lives: 3";
            livesText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            livesText.fontSize = 24;
            livesText.color = Color.white;
            livesText.alignment = TextAnchor.MiddleRight;
        }

        private static void CreateSimpleSprite(SpriteRenderer renderer, Vector2 size)
        {
            // 간단한 컬러 텍스처 생성
            int width = Mathf.RoundToInt(size.x * 100);
            int height = Mathf.RoundToInt(size.y * 100);

            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            renderer.sprite = sprite;
        }

        private static void AddScenesToBuildSettings(string[] sceneNames)
        {
            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

            foreach (string sceneName in sceneNames)
            {
                string scenePath = $"{SCENE_FOLDER}{sceneName}.unity";
                if (System.IO.File.Exists(scenePath))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log($"[QuickSetup] {buildScenes.Count}개 씬이 빌드 설정에 추가되었습니다.");
        }
    }
}