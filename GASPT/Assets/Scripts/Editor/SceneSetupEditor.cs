using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace Editor
{
    /// <summary>
    /// Unity 에디터에서 씬을 자동으로 생성하고 설정하는 에디터 스크립트
    /// </summary>
    public class SceneSetupEditor : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool[] sceneCreationFlags = new bool[8];
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

        private readonly string sceneFolderPath = "Assets/Scenes/";

        [MenuItem("GASPT/Scene Setup Tool")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetupEditor>("Scene Setup Tool");
        }

        private void OnGUI()
        {
            GUILayout.Label("GASPT 플랫포머 씬 설정 도구", EditorStyles.boldLabel);
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 씬 폴더 확인 및 생성
            DrawFolderSetup();

            GUILayout.Space(10);

            // 씬 생성 옵션
            DrawSceneCreationOptions();

            GUILayout.Space(10);

            // 액션 버튼들
            DrawActionButtons();

            GUILayout.Space(10);

            // 빌드 설정
            DrawBuildSettings();

            EditorGUILayout.EndScrollView();
        }

        private void DrawFolderSetup()
        {
            EditorGUILayout.LabelField("1. 폴더 설정", EditorStyles.boldLabel);

            if (!Directory.Exists(sceneFolderPath))
            {
                EditorGUILayout.HelpBox("Scenes 폴더가 존재하지 않습니다.", MessageType.Warning);
                if (GUILayout.Button("Scenes 폴더 생성"))
                {
                    Directory.CreateDirectory(sceneFolderPath);
                    AssetDatabase.Refresh();
                    Debug.Log($"[SceneSetup] Scenes 폴더 생성됨: {sceneFolderPath}");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Scenes 폴더가 준비되었습니다.", MessageType.Info);
            }
        }

        private void DrawSceneCreationOptions()
        {
            EditorGUILayout.LabelField("2. 생성할 씬 선택", EditorStyles.boldLabel);

            for (int i = 0; i < sceneNames.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                sceneCreationFlags[i] = EditorGUILayout.Toggle(sceneCreationFlags[i], GUILayout.Width(20));
                EditorGUILayout.LabelField(sceneNames[i]);

                string scenePath = $"{sceneFolderPath}{sceneNames[i]}.unity";
                bool sceneExists = File.Exists(scenePath);

                if (sceneExists)
                {
                    EditorGUILayout.LabelField("(존재함)", GUILayout.Width(60));
                }
                else
                {
                    EditorGUILayout.LabelField("(없음)", GUILayout.Width(60));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("모두 선택"))
            {
                for (int i = 0; i < sceneCreationFlags.Length; i++)
                    sceneCreationFlags[i] = true;
            }
            if (GUILayout.Button("모두 해제"))
            {
                for (int i = 0; i < sceneCreationFlags.Length; i++)
                    sceneCreationFlags[i] = false;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.LabelField("3. 씬 생성 및 설정", EditorStyles.boldLabel);

            if (GUILayout.Button("선택된 씬들 생성", GUILayout.Height(30)))
            {
                CreateSelectedScenes();
            }

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Bootstrap 씬 설정"))
            {
                SetupBootstrapScene();
            }
            if (GUILayout.Button("MainMenu 씬 설정"))
            {
                SetupMainMenuScene();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Gameplay 씬 설정"))
            {
                SetupGameplayScene();
            }
            if (GUILayout.Button("모든 씬 기본 설정"))
            {
                SetupAllScenes();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBuildSettings()
        {
            EditorGUILayout.LabelField("4. 빌드 설정", EditorStyles.boldLabel);

            if (GUILayout.Button("생성된 씬들을 빌드 설정에 추가"))
            {
                AddScenesToBuildSettings();
            }

            EditorGUILayout.HelpBox(
                "빌드 설정에 씬을 추가하면 SceneManager.LoadScene()으로 로드할 수 있습니다.",
                MessageType.Info
            );
        }

        private void CreateSelectedScenes()
        {
            int createdCount = 0;

            for (int i = 0; i < sceneNames.Length; i++)
            {
                if (sceneCreationFlags[i])
                {
                    string scenePath = $"{sceneFolderPath}{sceneNames[i]}.unity";

                    if (!File.Exists(scenePath))
                    {
                        // 새 씬 생성
                        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                        // 씬 저장
                        EditorSceneManager.SaveScene(newScene, scenePath);
                        createdCount++;

                        Debug.Log($"[SceneSetup] 씬 생성됨: {sceneNames[i]}");
                    }
                    else
                    {
                        Debug.Log($"[SceneSetup] 씬이 이미 존재함: {sceneNames[i]}");
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[SceneSetup] 총 {createdCount}개 씬이 생성되었습니다.");
        }

        private void SetupBootstrapScene()
        {
            string scenePath = $"{sceneFolderPath}00_Bootstrap.unity";

            if (!File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("오류", "Bootstrap 씬이 존재하지 않습니다. 먼저 씬을 생성해주세요.", "확인");
                return;
            }

            // Bootstrap 씬 열기
            EditorSceneManager.OpenScene(scenePath);

            // SystemBootstrap 오브젝트 생성
            GameObject bootstrapGO = new GameObject("SystemBootstrap");
            bootstrapGO.AddComponent<Scenes.Bootstrap.BootstrapManager>();

            // 씬 저장
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            Debug.Log("[SceneSetup] Bootstrap 씬 설정 완료");
        }

        private void SetupMainMenuScene()
        {
            string scenePath = $"{sceneFolderPath}01_MainMenu.unity";

            if (!File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("오류", "MainMenu 씬이 존재하지 않습니다. 먼저 씬을 생성해주세요.", "확인");
                return;
            }

            // MainMenu 씬 열기
            EditorSceneManager.OpenScene(scenePath);

            // EventSystem 생성 (UI를 위해 필요)
            CreateEventSystem();

            // Main Canvas 생성
            GameObject canvasGO = CreateUICanvas("MainMenuCanvas");

            // MainMenu Manager 생성
            GameObject managerGO = new GameObject("MainMenuManager");
            managerGO.AddComponent<Scenes.MainMenu.MainMenuUI>();

            // 기본 UI 버튼들 생성
            CreateMainMenuButtons(canvasGO);

            // 씬 저장
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            Debug.Log("[SceneSetup] MainMenu 씬 설정 완료");
        }

        private void SetupGameplayScene()
        {
            string scenePath = $"{sceneFolderPath}03_Gameplay.unity";

            if (!File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("오류", "Gameplay 씬이 존재하지 않습니다. 먼저 씬을 생성해주세요.", "확인");
                return;
            }

            // Gameplay 씬 열기
            EditorSceneManager.OpenScene(scenePath);

            // Main Camera (기본으로 있지만 설정 확인)
            SetupMainCamera();

            // Player 오브젝트 생성
            CreatePlayerObject();

            // 기본 Ground 플랫폼 생성
            CreateGroundPlatform();

            // Gameplay UI Canvas 생성
            CreateEventSystem();
            CreateUICanvas("GameplayCanvas");

            // 씬 저장
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            Debug.Log("[SceneSetup] Gameplay 씬 설정 완료");
        }

        private void SetupAllScenes()
        {
            SetupBootstrapScene();
            SetupMainMenuScene();
            SetupGameplayScene();

            Debug.Log("[SceneSetup] 모든 씬 기본 설정 완료");
        }

        private void AddScenesToBuildSettings()
        {
            var buildScenes = new List<EditorBuildSettingsScene>();

            for (int i = 0; i < sceneNames.Length; i++)
            {
                string scenePath = $"{sceneFolderPath}{sceneNames[i]}.unity";

                if (File.Exists(scenePath))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log($"[SceneSetup] {buildScenes.Count}개 씬이 빌드 설정에 추가되었습니다.");
        }

        // 유틸리티 메서드들
        private GameObject CreateUICanvas(string name)
        {
            GameObject canvasGO = new GameObject(name);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var canvasScaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            return canvasGO;
        }

        private void CreateEventSystem()
        {
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private void CreateMainMenuButtons(GameObject canvas)
        {
            string[] buttonNames = { "StartGame", "LevelSelect", "Settings", "Credits", "Quit" };
            string[] buttonTexts = { "게임 시작", "레벨 선택", "설정", "크레딧", "종료" };

            for (int i = 0; i < buttonNames.Length; i++)
            {
                GameObject buttonGO = CreateUIButton($"{buttonNames[i]}Button", buttonTexts[i], canvas);

                var rectTransform = buttonGO.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(300, 60);
                rectTransform.anchoredPosition = new Vector2(0, 150 - i * 80);
            }
        }

        private GameObject CreateUIButton(string name, string text, GameObject parent)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent.transform, false);

            var rectTransform = buttonGO.AddComponent<RectTransform>();
            var button = buttonGO.AddComponent<UnityEngine.UI.Button>();
            var image = buttonGO.AddComponent<UnityEngine.UI.Image>();

            // 기본 UI 스프라이트 사용
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = UnityEngine.UI.Image.Type.Sliced;

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
            textComponent.fontSize = 24;
            textComponent.color = Color.black;
            textComponent.alignment = TextAnchor.MiddleCenter;

            return buttonGO;
        }

        private void SetupMainCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraGO = new GameObject("Main Camera");
                mainCamera = cameraGO.AddComponent<Camera>();
                cameraGO.tag = "MainCamera";
            }

            // 2D 플랫포머를 위한 카메라 설정
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5f;
            mainCamera.transform.position = new Vector3(0, 0, -10);
        }

        private void CreatePlayerObject()
        {
            GameObject playerGO = new GameObject("Player");
            playerGO.transform.position = Vector3.zero;
            playerGO.tag = "Player";

            // Rigidbody2D 추가
            var rb = playerGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;

            // BoxCollider2D 추가
            var collider = playerGO.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 2f);

            // 기본 스프라이트 추가 (임시)
            var spriteRenderer = playerGO.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.blue;

            // 기본 정사각형 스프라이트 생성
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        private void CreateGroundPlatform()
        {
            GameObject groundGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            groundGO.name = "Ground";
            groundGO.transform.position = new Vector3(0, -3, 0);
            groundGO.transform.localScale = new Vector3(20, 1, 1);
            groundGO.tag = "Ground";

            // 3D Collider 제거하고 2D Collider 추가
            DestroyImmediate(groundGO.GetComponent<BoxCollider>());
            groundGO.AddComponent<BoxCollider2D>();

            // 색상 변경
            var renderer = groundGO.GetComponent<Renderer>();
            renderer.material.color = Color.green;
        }
    }
}
