#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

namespace Editor.Tools
{
    /// <summary>
    /// 씬 자동 생성 및 설정 도구
    /// Menu: GASPT → Scene Setup
    /// </summary>
    public class SceneSetupTool : EditorWindow
    {
        private const string SCENE_FOLDER = "Assets/_Project/Scenes";

        // 씬 이름 정의 (SceneType Enum과 일치)
        private static readonly string[] SCENE_NAMES = new string[]
        {
            "Bootstrap",  // 0
            "Preload",    // 1
            "Main",       // 2
            "Loading",    // 3
            "Gameplay",   // 4
            "Pause"       // 5
        };

        private bool createBootstrap = true;
        private bool createPreload = true;
        private bool createMain = true;
        private bool createLoading = true;
        private bool createGameplay = true;
        private bool createPause = true;

        private bool addToBuildSettings = true;
        private bool setupSceneObjects = true;

        private Vector2 scrollPosition;

        [MenuItem("GASPT/Scene Setup/Open Scene Setup Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneSetupTool>("Scene Setup Tool");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawSceneSelection();
            DrawOptions();
            DrawButtons();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("씬 자동 생성 및 설정 도구", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "이 도구는 GASPT 프로젝트의 씬 구조를 자동으로 생성합니다.\n" +
                "Bootstrap → Preload → Main → Gameplay 흐름에 필요한 모든 씬을 설정합니다.",
                MessageType.Info
            );
            EditorGUILayout.Space(10);
        }

        private void DrawSceneSelection()
        {
            EditorGUILayout.LabelField("생성할 씬 선택", EditorStyles.boldLabel);

            createBootstrap = EditorGUILayout.Toggle("Bootstrap (진입점)", createBootstrap);
            createPreload = EditorGUILayout.Toggle("Preload (초기 로딩)", createPreload);
            createMain = EditorGUILayout.Toggle("Main (메인 메뉴)", createMain);
            createLoading = EditorGUILayout.Toggle("Loading (로딩 화면)", createLoading);
            createGameplay = EditorGUILayout.Toggle("Gameplay (게임플레이)", createGameplay);
            createPause = EditorGUILayout.Toggle("Pause (일시정지)", createPause);

            EditorGUILayout.Space(10);
        }

        private void DrawOptions()
        {
            EditorGUILayout.LabelField("옵션", EditorStyles.boldLabel);

            addToBuildSettings = EditorGUILayout.Toggle("Build Settings에 추가", addToBuildSettings);
            setupSceneObjects = EditorGUILayout.Toggle("씬 오브젝트 자동 설정", setupSceneObjects);

            EditorGUILayout.Space(10);
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("모든 씬 생성", GUILayout.Height(40)))
            {
                CreateAllScenes();
            }

            if (GUILayout.Button("Build Settings 업데이트", GUILayout.Height(40)))
            {
                UpdateBuildSettings();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("씬 폴더 열기", GUILayout.Height(30)))
            {
                EditorUtility.RevealInFinder(SCENE_FOLDER);
            }
        }

        /// <summary>
        /// 선택된 모든 씬 생성
        /// </summary>
        private void CreateAllScenes()
        {
            if (!EditorUtility.DisplayDialog(
                "씬 생성 확인",
                "선택한 씬들을 생성하시겠습니까?\n기존 씬이 있으면 덮어쓰지 않습니다.",
                "생성",
                "취소"))
            {
                return;
            }

            // 씬 폴더 생성
            if (!Directory.Exists(SCENE_FOLDER))
            {
                Directory.CreateDirectory(SCENE_FOLDER);
                AssetDatabase.Refresh();
            }

            int createdCount = 0;

            if (createBootstrap) createdCount += CreateBootstrapScene() ? 1 : 0;
            if (createPreload) createdCount += CreatePreloadScene() ? 1 : 0;
            if (createMain) createdCount += CreateMainScene() ? 1 : 0;
            if (createLoading) createdCount += CreateLoadingScene() ? 1 : 0;
            if (createGameplay) createdCount += CreateGameplayScene() ? 1 : 0;
            if (createPause) createdCount += CreatePauseScene() ? 1 : 0;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (addToBuildSettings)
            {
                UpdateBuildSettings();
            }

            EditorUtility.DisplayDialog(
                "씬 생성 완료",
                $"{createdCount}개의 씬이 생성되었습니다.\n위치: {SCENE_FOLDER}",
                "확인"
            );

            Debug.Log($"[SceneSetupTool] {createdCount}개의 씬 생성 완료");
        }

        #region 씬 생성 메서드

        private bool CreateBootstrapScene()
        {
            string scenePath = $"{SCENE_FOLDER}/Bootstrap.unity";

            if (File.Exists(scenePath))
            {
                Debug.LogWarning($"[SceneSetupTool] Bootstrap 씬이 이미 존재합니다: {scenePath}");
                return false;
            }

            // 새 씬 생성
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (setupSceneObjects)
            {
                // BootstrapManager 추가 (리플렉션 사용)
                GameObject bootstrapManager = new GameObject("BootstrapManager");

                // Core.Bootstrap.BootstrapManager 타입 찾기
                Type bootstrapType = Type.GetType("Core.Bootstrap.BootstrapManager, Assembly-CSharp");
                if (bootstrapType != null)
                {
                    bootstrapManager.AddComponent(bootstrapType);
                }
                else
                {
                    Debug.LogWarning("[SceneSetupTool] BootstrapManager 타입을 찾을 수 없습니다. 수동으로 추가하세요.");
                }

                // Directional Light 추가
                GameObject light = new GameObject("Directional Light");
                Light lightComp = light.AddComponent<Light>();
                lightComp.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(50, -30, 0);
            }

            // 씬 저장
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneSetupTool] Bootstrap 씬 생성: {scenePath}");

            return true;
        }

        private bool CreatePreloadScene()
        {
            string scenePath = $"{SCENE_FOLDER}/Preload.unity";

            if (File.Exists(scenePath))
            {
                Debug.LogWarning($"[SceneSetupTool] Preload 씬이 이미 존재합니다: {scenePath}");
                return false;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (setupSceneObjects)
            {
                // Main Camera
                CreateCamera("Main Camera");

                // Directional Light
                CreateDirectionalLight();

                // (선택) LoadingScreen UI는 GameFlowManager가 자동 생성
            }

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneSetupTool] Preload 씬 생성: {scenePath}");

            return true;
        }

        private bool CreateMainScene()
        {
            string scenePath = $"{SCENE_FOLDER}/Main.unity";

            if (File.Exists(scenePath))
            {
                Debug.LogWarning($"[SceneSetupTool] Main 씬이 이미 존재합니다: {scenePath}");
                return false;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (setupSceneObjects)
            {
                // Main Camera
                CreateCamera("Main Camera");

                // Directional Light
                CreateDirectionalLight();

                // EventSystem
                CreateEventSystem();

                // (선택) MainMenu UI는 나중에 추가 가능
                GameObject mainMenuUI = new GameObject("MainMenuUI");
                mainMenuUI.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                mainMenuUI.AddComponent<UnityEngine.UI.CanvasScaler>();
                mainMenuUI.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneSetupTool] Main 씬 생성: {scenePath}");

            return true;
        }

        private bool CreateLoadingScene()
        {
            string scenePath = $"{SCENE_FOLDER}/Loading.unity";

            if (File.Exists(scenePath))
            {
                Debug.LogWarning($"[SceneSetupTool] Loading 씬이 이미 존재합니다: {scenePath}");
                return false;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (setupSceneObjects)
            {
                // LoadingScreen UI
                GameObject loadingUI = new GameObject("LoadingScreenUI");
                Canvas canvas = loadingUI.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // 최상위에 렌더링
                loadingUI.AddComponent<UnityEngine.UI.CanvasScaler>();
                loadingUI.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneSetupTool] Loading 씬 생성: {scenePath}");

            return true;
        }

        private bool CreateGameplayScene()
        {
            string scenePath = $"{SCENE_FOLDER}/Gameplay.unity";

            if (File.Exists(scenePath))
            {
                Debug.LogWarning($"[SceneSetupTool] Gameplay 씬이 이미 존재합니다: {scenePath}");
                return false;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (setupSceneObjects)
            {
                // Main Camera
                CreateCamera("Main Camera");

                // Directional Light
                CreateDirectionalLight();

                // EventSystem
                CreateEventSystem();

                // Ground (플랫폼)
                CreateGround();

                // Spawn Points
                CreateSpawnPoints();

                // Ingame UI
                GameObject ingameUI = new GameObject("IngameUI");
                Canvas canvas = ingameUI.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                ingameUI.AddComponent<UnityEngine.UI.CanvasScaler>();
                ingameUI.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneSetupTool] Gameplay 씬 생성: {scenePath}");

            return true;
        }

        private bool CreatePauseScene()
        {
            string scenePath = $"{SCENE_FOLDER}/Pause.unity";

            if (File.Exists(scenePath))
            {
                Debug.LogWarning($"[SceneSetupTool] Pause 씬이 이미 존재합니다: {scenePath}");
                return false;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (setupSceneObjects)
            {
                // Pause Menu UI
                GameObject pauseUI = new GameObject("PauseMenuUI");
                Canvas canvas = pauseUI.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;
                pauseUI.AddComponent<UnityEngine.UI.CanvasScaler>();
                pauseUI.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneSetupTool] Pause 씬 생성: {scenePath}");

            return true;
        }

        #endregion

        #region 오브젝트 생성 헬퍼

        private void CreateCamera(string name)
        {
            GameObject cameraGO = new GameObject(name);
            Camera camera = cameraGO.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = new Color(0.2f, 0.3f, 0.4f);
            cameraGO.tag = "MainCamera";
            cameraGO.transform.position = new Vector3(0f, 2f, -10f);
        }

        private void CreateDirectionalLight()
        {
            GameObject light = new GameObject("Directional Light");
            Light lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.intensity = 1f;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        private void CreateEventSystem()
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private void CreateGround()
        {
            GameObject ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0f, -1f, 0f);

            // BoxCollider2D
            BoxCollider2D collider = ground.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(30f, 2f);

            // SpriteRenderer
            SpriteRenderer renderer = ground.AddComponent<SpriteRenderer>();
            renderer.color = new Color(0.3f, 0.3f, 0.3f);

            // Sprite 생성
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = new Vector2(30f, 2f);
        }

        private void CreateSpawnPoints()
        {
            GameObject spawnPoints = new GameObject("SpawnPoints");

            // Player Spawn
            GameObject playerSpawn = new GameObject("PlayerSpawn");
            playerSpawn.transform.SetParent(spawnPoints.transform);
            playerSpawn.transform.position = new Vector3(-8f, 2f, 0f);

            // Enemy Spawns
            GameObject enemy1Spawn = new GameObject("Enemy1Spawn");
            enemy1Spawn.transform.SetParent(spawnPoints.transform);
            enemy1Spawn.transform.position = new Vector3(5f, 2f, 0f);

            GameObject enemy2Spawn = new GameObject("Enemy2Spawn");
            enemy2Spawn.transform.SetParent(spawnPoints.transform);
            enemy2Spawn.transform.position = new Vector3(10f, 2f, 0f);
        }

        #endregion

        #region Build Settings

        /// <summary>
        /// Build Settings에 씬 추가
        /// </summary>
        private void UpdateBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

            // SCENE_NAMES 배열 순서대로 추가
            for (int i = 0; i < SCENE_NAMES.Length; i++)
            {
                AddSceneToBuildSettings(scenes, SCENE_NAMES[i], i);
            }

            EditorBuildSettings.scenes = scenes.ToArray();

            Debug.Log($"[SceneSetupTool] Build Settings 업데이트 완료: {scenes.Count}개의 씬");

            EditorUtility.DisplayDialog(
                "Build Settings 업데이트",
                $"{scenes.Count}개의 씬이 Build Settings에 추가되었습니다.\n\nFile → Build Settings에서 확인하세요.",
                "확인"
            );
        }

        private void AddSceneToBuildSettings(List<EditorBuildSettingsScene> scenes, string sceneName, int expectedIndex)
        {
            string scenePath = $"{SCENE_FOLDER}/{sceneName}.unity";

            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"[SceneSetupTool] 씬을 찾을 수 없음: {scenePath}");
                return;
            }

            EditorBuildSettingsScene buildScene = new EditorBuildSettingsScene(scenePath, true);
            scenes.Add(buildScene);

            Debug.Log($"[SceneSetupTool] Build Settings에 추가: [{expectedIndex}] {sceneName}");
        }

        #endregion

        #region Quick Actions Menu

        [MenuItem("GASPT/Scene Setup/Create All Scenes")]
        private static void QuickCreateAllScenes()
        {
            var tool = CreateInstance<SceneSetupTool>();
            tool.CreateAllScenes();
        }

        [MenuItem("GASPT/Scene Setup/Update Build Settings")]
        private static void QuickUpdateBuildSettings()
        {
            var tool = CreateInstance<SceneSetupTool>();
            tool.UpdateBuildSettings();
        }

        [MenuItem("GASPT/Scene Setup/Open Scene Folder")]
        private static void OpenSceneFolder()
        {
            if (!Directory.Exists(SCENE_FOLDER))
            {
                Directory.CreateDirectory(SCENE_FOLDER);
                AssetDatabase.Refresh();
            }

            EditorUtility.RevealInFinder(SCENE_FOLDER);
        }

        #endregion
    }
}
#endif
