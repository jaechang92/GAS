using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GASPT.Gameplay.Level;
using GASPT.ResourceManagement;

namespace GASPT.Editor
{
    /// <summary>
    /// StartRoom 씬 자동 생성 에디터 툴
    /// Tools > GASPT > Create StartRoom Scene 메뉴로 실행
    /// </summary>
    public class StartRoomSceneCreator : EditorWindow
    {
        private string sceneName = "StartRoom";
        private string scenePath = "Assets/_Project/Scenes/";

        [MenuItem("Tools/GASPT/Create StartRoom Scene")]
        public static void ShowWindow()
        {
            var window = GetWindow<StartRoomSceneCreator>("StartRoom Scene Creator");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            GUILayout.Label("StartRoom Scene Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "이 툴은 StartRoom 씬을 자동으로 생성합니다.\n" +
                "- Main Camera, EventSystem, Canvas\n" +
                "- DungeonEntrance Portal\n" +
                "- 기본 배경 및 바닥\n" +
                "- UI 요소들",
                MessageType.Info
            );

            GUILayout.Space(10);

            // 씬 이름 입력
            sceneName = EditorGUILayout.TextField("Scene Name:", sceneName);

            // 씬 경로 입력
            scenePath = EditorGUILayout.TextField("Scene Path:", scenePath);

            GUILayout.Space(10);

            // 생성 버튼
            if (GUILayout.Button("Create StartRoom Scene", GUILayout.Height(40)))
            {
                EditorApplication.delayCall += CreateStartRoomScene;
            }

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "생성 후:\n" +
                "1. File > Build Settings에서 씬을 추가하세요\n" +
                "2. StartRoom을 Build Settings의 첫 번째 씬으로 설정하세요\n" +
                "3. GameplayScene은 두 번째 씬으로 유지하세요",
                MessageType.Warning
            );
        }

        private void CreateStartRoomScene()
        {
            // 경로 유효성 검사
            if (!AssetDatabase.IsValidFolder(scenePath.TrimEnd('/')))
            {
                if (!EditorUtility.DisplayDialog("폴더 없음",
                    $"경로 '{scenePath}'가 존재하지 않습니다. 생성하시겠습니까?",
                    "생성", "취소"))
                {
                    return;
                }

                // 폴더 생성
                string[] folders = scenePath.TrimEnd('/').Split('/');
                string currentPath = "";
                foreach (var folder in folders)
                {
                    if (string.IsNullOrEmpty(folder)) continue;

                    string parentPath = currentPath;
                    currentPath = string.IsNullOrEmpty(currentPath) ? folder : $"{currentPath}/{folder}";

                    if (!AssetDatabase.IsValidFolder(currentPath))
                    {
                        AssetDatabase.CreateFolder(parentPath, folder);
                    }
                }
            }

            // 씬 경로
            string fullScenePath = $"{scenePath.TrimEnd('/')}/{sceneName}.unity";

            // 이미 존재하는 씬인지 확인
            if (System.IO.File.Exists(fullScenePath))
            {
                if (!EditorUtility.DisplayDialog("씬 이미 존재",
                    $"'{fullScenePath}' 씬이 이미 존재합니다. 덮어쓰시겠습니까?",
                    "덮어쓰기", "취소"))
                {
                    return;
                }
            }

            // 새 씬 생성
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 씬 설정
            SetupStartRoomScene();

            // 씬 저장
            bool saved = EditorSceneManager.SaveScene(newScene, fullScenePath);

            if (saved)
            {
                Debug.Log($"[StartRoomSceneCreator] StartRoom 씬 생성 완료: {fullScenePath}");
                EditorUtility.DisplayDialog("생성 완료",
                    $"StartRoom 씬이 생성되었습니다!\n\n" +
                    $"경로: {fullScenePath}\n\n" +
                    $"다음 단계:\n" +
                    $"1. File > Build Settings 열기\n" +
                    $"2. StartRoom 씬을 Build Settings에 추가\n" +
                    $"3. StartRoom을 Index 0으로 설정",
                    "확인");

                // 씬 선택
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(fullScenePath);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            else
            {
                Debug.LogError("[StartRoomSceneCreator] 씬 저장 실패!");
                EditorUtility.DisplayDialog("생성 실패", "씬 저장에 실패했습니다.", "확인");
            }
        }

        private void SetupStartRoomScene()
        {
            // 기본 Camera와 Directional Light는 DefaultGameObjects로 자동 생성됨

            // Main Camera 설정
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f); // 어두운 파란색
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 5f;
            }

            // EventSystem 생성 (UI용)
            CreateEventSystem();

            // Canvas 생성 (UI 컨테이너)
            GameObject canvas = CreateCanvas();

            // StartRoom UI 생성
            CreateStartRoomUI(canvas);

            // 바닥 생성
            CreateGround();

            // DungeonEntrance Portal 생성
            CreateDungeonPortal();

            // 배경 생성 (선택사항)
            CreateBackground();

            // 플레이어 캐릭터 생성
            CreatePlayer();

            Debug.Log("[StartRoomSceneCreator] StartRoom 씬 설정 완료!");
        }

        private void CreateEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                Debug.Log("[StartRoomSceneCreator] EventSystem 이미 존재 - 스킵");
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            Debug.Log("[StartRoomSceneCreator] EventSystem 생성 완료");
        }

        private GameObject CreateCanvas()
        {
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // CanvasScaler 설정
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            Debug.Log("[StartRoomSceneCreator] Canvas 생성 완료");
            return canvasObj;
        }

        private void CreateStartRoomUI(GameObject canvas)
        {
            // 타이틀 텍스트
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(canvas.transform, false);

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "준비실";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 60;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.sizeDelta = new Vector2(400, 100);

            // 안내 텍스트
            GameObject instructionObj = new GameObject("Instruction");
            instructionObj.transform.SetParent(canvas.transform, false);

            Text instructionText = instructionObj.AddComponent<Text>();
            instructionText.text = "포탈로 이동하여 던전에 입장하세요";
            instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            instructionText.fontSize = 24;
            instructionText.alignment = TextAnchor.MiddleCenter;
            instructionText.color = new Color(0.8f, 0.8f, 0.8f);

            RectTransform instructionRect = instructionObj.GetComponent<RectTransform>();
            instructionRect.anchorMin = new Vector2(0.5f, 0.7f);
            instructionRect.anchorMax = new Vector2(0.5f, 0.7f);
            instructionRect.sizeDelta = new Vector2(600, 50);

            Debug.Log("[StartRoomSceneCreator] StartRoom UI 생성 완료");
        }

        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0, -3, 0);
            ground.transform.localScale = new Vector3(20, 1, 1);

            // Material 설정 (어두운 회색)
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Edit Mode에서는 새 Material 인스턴스를 생성하여 할당
                // 2D 프로젝트이므로 Sprites/Default 쉐이더 사용 (Standard는 3D 전용)
                Material newMaterial = new Material(Shader.Find("Sprites/Default"));
                newMaterial.color = new Color(0.3f, 0.3f, 0.3f);
                renderer.sharedMaterial = newMaterial;
            }

            // Collider 설정
            BoxCollider collider = ground.GetComponent<BoxCollider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            // BoxCollider2D 추가
            ground.AddComponent<BoxCollider2D>();

            Debug.Log("[StartRoomSceneCreator] Ground 생성 완료");
        }

        private void CreateDungeonPortal()
        {
            GameObject portalObj = new GameObject("DungeonEntrance_Portal");
            portalObj.transform.position = new Vector3(5, 0, 0);

            // SpriteRenderer 추가 (시각적 표현)
            SpriteRenderer spriteRenderer = portalObj.AddComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(0, 1f, 1f, 0.8f); // 시안색

            // 기본 스프라이트 생성 (원형)
            Texture2D texture = new Texture2D(64, 64);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(32, 32));
                    if (distance < 30)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }
            texture.Apply();

            Sprite portalSprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
            spriteRenderer.sprite = portalSprite;

            // CircleCollider2D 추가
            CircleCollider2D collider = portalObj.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;

            // Portal 컴포넌트 추가
            Portal portal = portalObj.AddComponent<Portal>();

            // Portal 설정 (Reflection 사용)
            SerializedObject serializedPortal = new SerializedObject(portal);
            serializedPortal.FindProperty("portalType").enumValueIndex = (int)PortalType.DungeonEntrance;
            serializedPortal.FindProperty("autoActivateOnRoomClear").boolValue = false;
            serializedPortal.FindProperty("startActive").boolValue = true;
            serializedPortal.FindProperty("portalSprite").objectReferenceValue = spriteRenderer;
            serializedPortal.ApplyModifiedProperties();

            Debug.Log("[StartRoomSceneCreator] DungeonEntrance Portal 생성 완료");
        }

        private void CreateBackground()
        {
            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Quad);
            background.name = "Background";
            background.transform.position = new Vector3(0, 0, 10);
            background.transform.localScale = new Vector3(25, 15, 1);

            // Material 설정 (어두운 파란색)
            Renderer renderer = background.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Edit Mode에서는 새 Material 인스턴스를 생성하여 할당
                // 2D 프로젝트이므로 Sprites/Default 쉐이더 사용 (Standard는 3D 전용)
                Material newMaterial = new Material(Shader.Find("Sprites/Default"));
                newMaterial.color = new Color(0.05f, 0.05f, 0.1f);
                renderer.sharedMaterial = newMaterial;
            }

            // Collider 제거
            Collider collider = background.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            Debug.Log("[StartRoomSceneCreator] Background 생성 완료");
        }

        private void CreatePlayer()
        {
            // MageForm 프리팹 로드
            GameObject mageFormPrefab = Resources.Load<GameObject>(ResourcePaths.Prefabs.Player.MageForm);

            if (mageFormPrefab == null)
            {
                Debug.LogError("[StartRoomSceneCreator] MageForm 프리팹을 찾을 수 없습니다! 경로: " + ResourcePaths.Prefabs.Player.MageForm);
                return;
            }

            // 프리팹 인스턴스화
            GameObject player = PrefabUtility.InstantiatePrefab(mageFormPrefab) as GameObject;
            if (player == null)
            {
                Debug.LogError("[StartRoomSceneCreator] MageForm 프리팹 인스턴스화 실패!");
                return;
            }

            // 플레이어 설정
            player.name = "Player";
            player.transform.position = new Vector3(-5f, 0f, 0f); // 포탈 반대편에 배치
            player.tag = "Player";

            Debug.Log("[StartRoomSceneCreator] Player (MageForm) 생성 완료");
        }
    }
}
