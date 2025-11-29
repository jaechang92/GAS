using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using GASPT.Testing;
using GASPT.Gameplay.Level;
using GASPT.UI.Minimap;
using GASPT.EditorTools;
using TMPro;

namespace GASPT.Editor
{
    /// <summary>
    /// 절차적 던전 테스트 씬 원클릭 생성 에디터 도구
    /// Menu: Tools > GASPT > Procedural Dungeon > Create Test Scene
    /// </summary>
    public class ProceduralDungeonTestSceneSetup
    {
        private const string SCENE_NAME = "ProceduralDungeonTestScene";
        private const string SCENE_PATH = "Assets/_Project/Scenes/" + SCENE_NAME + ".unity";

        // 에셋 경로
        private const string DUNGEON_CONFIG_PATH = "Assets/Resources/Data/Dungeons/TestDungeon.asset";
        private const string MINIMAP_CONFIG_PATH = "Assets/Resources/Data/UI/MinimapConfig.asset";
        private const string ROOM_TEMPLATE_PATH = "Assets/Resources/Prefabs/Rooms/RoomTemplate.prefab";


        [MenuItem("Tools/GASPT/Procedural Dungeon/🎮 Create Test Scene", false, 200)]
        public static void CreateTestScene()
        {
            Debug.Log("========== Procedural Dungeon Test Scene 생성 시작 ==========");

            // 1. 필수 에셋 확인
            if (!ValidateRequiredAssets())
            {
                if (EditorUtility.DisplayDialog("에셋 필요",
                    "필수 에셋이 없습니다.\n먼저 'Create All Assets'를 실행하시겠습니까?",
                    "예", "아니오"))
                {
                    ProceduralDungeonAssetCreator.ShowWindow();
                }
                return;
            }

            // 2. 새 씬 생성
            Scene scene = CreateNewScene();

            // 3. 매니저들 생성
            GameObject managers = CreateManagers();

            // 4. 테스트 컴포넌트 생성
            GameObject testManager = CreateTestManager();

            // 5. UI Canvas 생성
            GameObject canvas = CreateUICanvas();

            // 6. 미니맵 시스템 생성
            GameObject minimapSystem = CreateMinimapSystem(canvas);

            // 7. 카메라 설정
            SetupCamera();

            // 8. 조명 설정
            SetupLighting();

            // 9. 바닥 생성
            CreateFloor();

            // 10. 참조 연결
            LinkReferences(testManager, minimapSystem);

            // 11. 씬 저장
            EnsureSceneFolder();
            EditorSceneManager.SaveScene(scene, SCENE_PATH);

            Debug.Log("========== Procedural Dungeon Test Scene 생성 완료 ==========");
            Debug.Log($"씬 경로: {SCENE_PATH}");
            EditorUtility.DisplayDialog("완료",
                $"테스트 씬이 생성되었습니다!\n\n" +
                $"경로: {SCENE_PATH}\n\n" +
                $"테스트 방법:\n" +
                $"1. Play 모드 진입\n" +
                $"2. G키: 던전 생성\n" +
                $"3. R키: 랜덤 시드 테스트\n" +
                $"4. V키: 유효성 검증\n" +
                $"5. P키: 경로 탐색\n" +
                $"6. M/Tab: 미니맵 토글",
                "확인");
        }


        // ====== 유효성 검사 ======

        private static bool ValidateRequiredAssets()
        {
            bool hasConfig = AssetDatabase.LoadAssetAtPath<DungeonConfig>(DUNGEON_CONFIG_PATH) != null;

            if (!hasConfig)
            {
                Debug.LogWarning($"[Setup] DungeonConfig가 없습니다: {DUNGEON_CONFIG_PATH}");
            }

            return hasConfig;
        }


        // ====== 씬 생성 ======

        private static Scene CreateNewScene()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Debug.Log("[Setup] 새 씬 생성 완료");
            return newScene;
        }

        private static void EnsureSceneFolder()
        {
            string folder = "Assets/_Project/Scenes";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }
        }


        // ====== 매니저 생성 ======

        private static GameObject CreateManagers()
        {
            GameObject managersRoot = new GameObject("--- Managers ---");

            // RoomManager
            GameObject roomManagerObj = new GameObject("RoomManager");
            roomManagerObj.transform.SetParent(managersRoot.transform);
            RoomManager roomManager = roomManagerObj.AddComponent<RoomManager>();

            // RoomManager 설정 (autoStartFirstRoom = false)
            SerializedObject rmSO = new SerializedObject(roomManager);
            var autoStartProp = rmSO.FindProperty("autoStartFirstRoom");
            if (autoStartProp != null)
            {
                autoStartProp.boolValue = false;
            }
            rmSO.ApplyModifiedProperties();

            // StageManager
            GameObject stageManagerObj = new GameObject("StageManager");
            stageManagerObj.transform.SetParent(managersRoot.transform);
            stageManagerObj.AddComponent<StageManager>();

            // Room Container (동적 생성된 Room들의 부모)
            GameObject roomContainer = new GameObject("RoomContainer");
            roomContainer.transform.SetParent(managersRoot.transform);

            Debug.Log("[Setup] 매니저 생성 완료 (RoomManager, StageManager)");
            return managersRoot;
        }


        // ====== 테스트 매니저 생성 ======

        private static GameObject CreateTestManager()
        {
            GameObject testObj = new GameObject("ProceduralDungeonTest");
            ProceduralDungeonTest test = testObj.AddComponent<ProceduralDungeonTest>();

            // DungeonConfig 로드 및 설정
            DungeonConfig dungeonConfig = AssetDatabase.LoadAssetAtPath<DungeonConfig>(DUNGEON_CONFIG_PATH);

            SerializedObject so = new SerializedObject(test);

            var configProp = so.FindProperty("dungeonConfig");
            if (configProp != null)
            {
                configProp.objectReferenceValue = dungeonConfig;
            }

            var seedProp = so.FindProperty("testSeed");
            if (seedProp != null)
            {
                seedProp.intValue = 12345;
            }

            var randomSeedProp = so.FindProperty("useRandomSeed");
            if (randomSeedProp != null)
            {
                randomSeedProp.boolValue = false;
            }

            var autoTestProp = so.FindProperty("autoTestOnStart");
            if (autoTestProp != null)
            {
                autoTestProp.boolValue = false; // 수동 시작
            }

            var logDetailedProp = so.FindProperty("logDetailedInfo");
            if (logDetailedProp != null)
            {
                logDetailedProp.boolValue = true;
            }

            so.ApplyModifiedProperties();

            Debug.Log("[Setup] ProceduralDungeonTest 생성 완료");
            return testObj;
        }


        // ====== UI Canvas 생성 ======

        private static GameObject CreateUICanvas()
        {
            // Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem
            if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Debug.Log("[Setup] UI Canvas 생성 완료");
            return canvasObj;
        }


        // ====== 미니맵 시스템 생성 ======

        private static GameObject CreateMinimapSystem(GameObject canvas)
        {
            // MinimapPresenter (씬 루트에)
            GameObject presenterObj = new GameObject("MinimapPresenter");
            MinimapPresenter presenter = presenterObj.AddComponent<MinimapPresenter>();

            // MinimapView (Canvas 하위에)
            GameObject minimapViewObj = CreateMinimapViewUI(canvas.transform);
            MinimapView view = minimapViewObj.GetComponent<MinimapView>();

            // Presenter에 View 연결
            SerializedObject so = new SerializedObject(presenter);
            var viewProp = so.FindProperty("view");
            if (viewProp != null)
            {
                viewProp.objectReferenceValue = view;
            }

            var autoFindViewProp = so.FindProperty("autoFindView");
            if (autoFindViewProp != null)
            {
                autoFindViewProp.boolValue = false; // 수동 연결
            }

            so.ApplyModifiedProperties();

            Debug.Log("[Setup] 미니맵 시스템 생성 완료");
            return presenterObj;
        }

        private static GameObject CreateMinimapViewUI(Transform canvasTransform)
        {
            // MinimapView Panel
            GameObject minimapPanel = new GameObject("MinimapView");
            minimapPanel.transform.SetParent(canvasTransform, false);

            RectTransform panelRect = minimapPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.anchoredPosition = new Vector2(-20, -20);
            panelRect.sizeDelta = new Vector2(300, 400);

            // 배경 이미지
            Image bgImage = minimapPanel.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // MinimapView 컴포넌트
            MinimapView minimapView = minimapPanel.AddComponent<MinimapView>();

            // Content (노드/엣지 컨테이너)
            GameObject content = new GameObject("Content");
            content.transform.SetParent(minimapPanel.transform, false);

            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(10, 50);
            contentRect.offsetMax = new Vector2(-10, -40);

            // Nodes Container
            GameObject nodesContainer = new GameObject("NodesContainer");
            nodesContainer.transform.SetParent(content.transform, false);
            RectTransform nodesRect = nodesContainer.AddComponent<RectTransform>();
            nodesRect.anchorMin = Vector2.zero;
            nodesRect.anchorMax = Vector2.one;
            nodesRect.offsetMin = Vector2.zero;
            nodesRect.offsetMax = Vector2.zero;

            // Edges Container
            GameObject edgesContainer = new GameObject("EdgesContainer");
            edgesContainer.transform.SetParent(content.transform, false);
            RectTransform edgesRect = edgesContainer.AddComponent<RectTransform>();
            edgesRect.anchorMin = Vector2.zero;
            edgesRect.anchorMax = Vector2.one;
            edgesRect.offsetMin = Vector2.zero;
            edgesRect.offsetMax = Vector2.zero;
            edgesContainer.transform.SetSiblingIndex(0); // 엣지를 노드 아래에

            // 타이틀
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(minimapPanel.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(0, 35);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "DUNGEON MAP";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            // 안내 텍스트
            GameObject infoObj = new GameObject("Info");
            infoObj.transform.SetParent(minimapPanel.transform, false);

            RectTransform infoRect = infoObj.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0, 0);
            infoRect.anchorMax = new Vector2(1, 0);
            infoRect.pivot = new Vector2(0.5f, 0);
            infoRect.anchoredPosition = Vector2.zero;
            infoRect.sizeDelta = new Vector2(0, 40);

            TextMeshProUGUI infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.text = "Press M or Tab to toggle";
            infoText.fontSize = 12;
            infoText.alignment = TextAlignmentOptions.Center;
            infoText.color = new Color(0.7f, 0.7f, 0.7f);

            // MinimapView SerializedObject 설정
            SerializedObject so = new SerializedObject(minimapView);

            var contentProp = so.FindProperty("mapContent");
            if (contentProp != null)
            {
                contentProp.objectReferenceValue = contentRect;
            }

            var nodeContainerProp = so.FindProperty("nodeContainer");
            if (nodeContainerProp != null)
            {
                nodeContainerProp.objectReferenceValue = nodesRect;
            }

            var edgeContainerProp = so.FindProperty("edgeContainer");
            if (edgeContainerProp != null)
            {
                edgeContainerProp.objectReferenceValue = edgesRect;
            }

            // MinimapConfig 로드
            MinimapConfig config = AssetDatabase.LoadAssetAtPath<MinimapConfig>(MINIMAP_CONFIG_PATH);
            var configProp = so.FindProperty("config");
            if (configProp != null && config != null)
            {
                configProp.objectReferenceValue = config;
            }

            so.ApplyModifiedProperties();

            return minimapPanel;
        }


        // ====== 카메라 설정 ======

        private static void SetupCamera()
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";

            Camera camera = cameraObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            camera.orthographic = true;
            camera.orthographicSize = 8f;
            camera.nearClipPlane = -10f;
            camera.farClipPlane = 100f;

            cameraObj.transform.position = new Vector3(0f, 0f, -10f);

            // AudioListener 추가
            cameraObj.AddComponent<AudioListener>();

            Debug.Log("[Setup] 카메라 설정 완료");
        }


        // ====== 조명 설정 ======

        private static void SetupLighting()
        {
            // Directional Light
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Debug.Log("[Setup] 조명 설정 완료");
        }


        // ====== 바닥 생성 ======

        private static void CreateFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = new Vector3(0f, -1f, 0f);
            floor.transform.localScale = new Vector3(5f, 1f, 5f);

            // 머티리얼 색상 변경
            Renderer renderer = floor.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.3f, 0.3f, 0.35f);
            renderer.material = mat;

            Debug.Log("[Setup] 바닥 생성 완료");
        }


        // ====== 참조 연결 ======

        private static void LinkReferences(GameObject testManager, GameObject minimapPresenter)
        {
            ProceduralDungeonTest test = testManager.GetComponent<ProceduralDungeonTest>();
            MinimapPresenter presenter = minimapPresenter.GetComponent<MinimapPresenter>();

            if (test != null && presenter != null)
            {
                SerializedObject so = new SerializedObject(test);
                var presenterProp = so.FindProperty("minimapPresenter");
                if (presenterProp != null)
                {
                    presenterProp.objectReferenceValue = presenter;
                }
                so.ApplyModifiedProperties();
            }

            Debug.Log("[Setup] 참조 연결 완료");
        }


        // ====== 추가 메뉴 ======

        [MenuItem("Tools/GASPT/Procedural Dungeon/🔧 Open Test Scene", false, 201)]
        public static void OpenTestScene()
        {
            if (System.IO.File.Exists(SCENE_PATH))
            {
                EditorSceneManager.OpenScene(SCENE_PATH);
                Debug.Log($"[Setup] 테스트 씬 열기: {SCENE_PATH}");
            }
            else
            {
                EditorUtility.DisplayDialog("오류",
                    "테스트 씬이 존재하지 않습니다.\n먼저 'Create Test Scene'을 실행해주세요.",
                    "확인");
            }
        }

        [MenuItem("Tools/GASPT/Procedural Dungeon/📋 Quick Test Guide", false, 202)]
        public static void ShowTestGuide()
        {
            string guide =
                "=== 절차적 던전 테스트 가이드 ===\n\n" +
                "1. 테스트 씬 생성: Create Test Scene\n" +
                "2. Play 모드 진입\n\n" +
                "키 바인딩:\n" +
                "• G: 던전 그래프 생성\n" +
                "• R: 랜덤 시드 10회 테스트\n" +
                "• V: 그래프 유효성 검증\n" +
                "• P: Entry→Boss 경로 탐색\n" +
                "• M/Tab: 미니맵 토글\n" +
                "• F10: UI 토글\n\n" +
                "Inspector 옵션:\n" +
                "• Test Seed: 고정 시드값\n" +
                "• Use Random Seed: 랜덤 시드 사용\n" +
                "• Auto Test On Start: 시작 시 자동 생성\n" +
                "• Log Detailed Info: 상세 로그 출력";

            EditorUtility.DisplayDialog("테스트 가이드", guide, "확인");
        }
    }
}
