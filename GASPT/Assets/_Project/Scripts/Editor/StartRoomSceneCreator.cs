using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using GASPT.Core.Enums;
using GASPT.Gameplay.Level;
using GASPT.ResourceManagement;

namespace GASPT.Editor
{
    /// <summary>
    /// StartRoom Content Scene 자동 생성 에디터 툴
    /// Tools > GASPT > Create StartRoom Scene 메뉴로 실행
    ///
    /// [Additive Scene Loading 구조]
    /// - 이 씬은 Content Scene으로 사용됨
    /// - 공통 UI는 PersistentManagers Scene에 존재
    /// - 이 씬에는 Content 요소만 포함:
    ///   - Player (MageForm)
    ///   - DungeonEntrance Portal
    ///   - NPC (상점 상호작용)
    ///   - 바닥/배경
    ///   - 월드 스페이스 안내 텍스트
    /// </summary>
    public class StartRoomSceneCreator : EditorWindow
    {
        private string sceneName = "StartRoom";
        private string scenePath = "Assets/_Project/Scenes/";

        // 생성 옵션
        private bool createPlayer = true;
        private bool createPortal = true;
        private bool createNPC = true;

        private Vector2 scrollPosition;

        [MenuItem("Tools/GASPT/🏠 Create StartRoom Scene")]
        public static void ShowWindow()
        {
            var window = GetWindow<StartRoomSceneCreator>("StartRoom Scene Creator");
            window.minSize = new Vector2(450, 500);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("=== StartRoom Content Scene Creator ===", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "[Additive Scene Loading 구조]\n" +
                "이 씬은 Content Scene으로 사용됩니다.\n" +
                "공통 UI는 PersistentManagers Scene에 있습니다.\n\n" +
                "포함 요소:\n" +
                "✓ Main Camera\n" +
                "✓ Player (MageForm)\n" +
                "✓ DungeonEntrance Portal\n" +
                "✓ NPC (상점 상호작용)\n" +
                "✓ 기본 배경 및 바닥\n" +
                "✓ 월드 스페이스 안내 텍스트\n\n" +
                "❌ UI는 PersistentManagers Scene에서 관리",
                MessageType.Info
            );

            GUILayout.Space(10);

            // 씬 설정
            EditorGUILayout.LabelField("씬 설정:", EditorStyles.boldLabel);
            sceneName = EditorGUILayout.TextField("Scene Name:", sceneName);
            scenePath = EditorGUILayout.TextField("Scene Path:", scenePath);

            GUILayout.Space(10);

            // 생성 옵션
            EditorGUILayout.LabelField("생성 옵션:", EditorStyles.boldLabel);
            createPlayer = EditorGUILayout.Toggle("플레이어", createPlayer);
            createPortal = EditorGUILayout.Toggle("던전 포탈", createPortal);
            createNPC = EditorGUILayout.Toggle("NPC (상점)", createNPC);

            GUILayout.Space(20);

            // 전체 생성 버튼
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🚀 StartRoom Content Scene 생성", GUILayout.Height(50)))
            {
                EditorApplication.delayCall += CreateStartRoomScene;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // 개별 생성 버튼
            EditorGUILayout.LabelField("개별 생성 (현재 씬에):", EditorStyles.boldLabel);

            if (GUILayout.Button("NPC만 생성"))
            {
                EditorApplication.delayCall += CreateShopNPC;
            }

            if (GUILayout.Button("Portal만 생성"))
            {
                EditorApplication.delayCall += CreateDungeonPortal;
            }

            GUILayout.Space(20);

            EditorGUILayout.HelpBox(
                "게임 실행 순서:\n" +
                "1. Bootstrap Scene (Index 0) 에서 시작\n" +
                "2. PersistentManagers Scene 로드 (UI 포함)\n" +
                "3. StartRoom Content Scene 로드\n\n" +
                "Build Settings 설정:\n" +
                "- Index 0: Bootstrap\n" +
                "- Index 1: PersistentManagers\n" +
                "- Index 2: StartRoom\n" +
                "- Index 3: GameplayScene",
                MessageType.Warning
            );

            EditorGUILayout.EndScrollView();
        }

        private void CreateStartRoomScene()
        {
            Debug.Log("=== StartRoom Content Scene 생성 시작 ===");

            // 경로 유효성 검사
            if (!AssetDatabase.IsValidFolder(scenePath.TrimEnd('/')))
            {
                if (!EditorUtility.DisplayDialog("폴더 없음",
                    $"경로 '{scenePath}'가 존재하지 않습니다. 생성하시겠습니까?",
                    "생성", "취소"))
                {
                    return;
                }

                CreateFolderRecursive(scenePath.TrimEnd('/'));
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

            // 새 씬 생성 (빈 씬 - Camera와 EventSystem은 PersistentManagers에 있음)
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 씬 설정
            SetupStartRoomScene();

            // 씬 저장
            bool saved = EditorSceneManager.SaveScene(newScene, fullScenePath);

            if (saved)
            {
                Debug.Log($"=== StartRoom Content Scene 생성 완료! ===\n위치: {fullScenePath}");
                EditorUtility.DisplayDialog("생성 완료",
                    $"StartRoom Content Scene이 생성되었습니다!\n\n" +
                    $"경로: {fullScenePath}\n\n" +
                    $"다음 단계:\n" +
                    $"1. Bootstrap Scene 생성 (Tools > GASPT > Bootstrap Scene Creator)\n" +
                    $"2. PersistentManagers Scene 생성\n" +
                    $"3. Build Settings에서 씬 순서 설정",
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
            // 참고: Main Camera와 EventSystem은 PersistentManagers Scene에 있음
            // Content Scene에는 게임 오브젝트만 배치

            // 바닥 생성
            CreateGround();

            // 배경 생성
            CreateBackground();

            // Player 생성
            if (createPlayer)
            {
                CreatePlayer();
            }

            // Portal 생성
            if (createPortal)
            {
                CreateDungeonPortal();
            }

            // NPC 생성
            if (createNPC)
            {
                CreateShopNPC();
            }

            // 월드 스페이스 안내 텍스트
            CreateWorldSpaceInfoText();

            Debug.Log("[StartRoomSceneCreator] StartRoom Content Scene 설정 완료!");
        }

        /// <summary>
        /// 월드 스페이스 안내 텍스트 생성 (Canvas 없이)
        /// </summary>
        private void CreateWorldSpaceInfoText()
        {
            // 타이틀 텍스트
            GameObject titleObj = new GameObject("TitleText_WorldSpace");
            titleObj.transform.position = new Vector3(0, 4f, 0);

            TextMesh titleText = titleObj.AddComponent<TextMesh>();
            titleText.text = "준비실";
            titleText.fontSize = 80;
            titleText.characterSize = 0.1f;
            titleText.anchor = TextAnchor.MiddleCenter;
            titleText.alignment = TextAlignment.Center;
            titleText.color = Color.white;

            // 안내 텍스트
            GameObject instructionObj = new GameObject("InstructionText_WorldSpace");
            instructionObj.transform.position = new Vector3(0, -4f, 0);

            TextMesh instructionText = instructionObj.AddComponent<TextMesh>();
            instructionText.text = "I: 인벤토리 | B: 상점 | 포탈: 던전 입장";
            instructionText.fontSize = 40;
            instructionText.characterSize = 0.1f;
            instructionText.anchor = TextAnchor.MiddleCenter;
            instructionText.alignment = TextAlignment.Center;
            instructionText.color = new Color(0.8f, 0.8f, 0.8f);

            Debug.Log("[StartRoomSceneCreator] 월드 스페이스 안내 텍스트 생성 완료");
        }

        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0, -3, 0);
            ground.transform.localScale = new Vector3(20, 1, 1);

            // Material 설정
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material newMaterial = new Material(Shader.Find("Sprites/Default"));
                newMaterial.color = new Color(0.3f, 0.3f, 0.3f);
                renderer.sharedMaterial = newMaterial;
            }

            // 3D Collider 제거
            Collider collider = ground.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }

            // 2D Collider 추가
            BoxCollider2D collider2D = ground.AddComponent<BoxCollider2D>();

            // Layer 설정
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer != -1)
            {
                ground.layer = groundLayer;
            }

            Debug.Log("[StartRoomSceneCreator] Ground 생성 완료");
        }

        private void CreateDungeonPortal()
        {
            GameObject portalObj = new GameObject("DungeonEntrance_Portal");
            portalObj.transform.position = new Vector3(7, -1.5f, 0);

            // SpriteRenderer
            SpriteRenderer spriteRenderer = portalObj.AddComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(0, 1f, 1f, 0.8f);

            // 원형 스프라이트 생성
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

            // CircleCollider2D
            CircleCollider2D collider = portalObj.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.8f;

            // Portal 컴포넌트 추가
            Portal portal = portalObj.AddComponent<Portal>();

            // Portal 설정
            SerializedObject serializedPortal = new SerializedObject(portal);
            serializedPortal.FindProperty("portalType").enumValueIndex = (int)PortalType.DungeonEntrance;
            serializedPortal.FindProperty("autoActivateOnRoomClear").boolValue = false;
            serializedPortal.FindProperty("startActive").boolValue = true;
            serializedPortal.FindProperty("portalSprite").objectReferenceValue = spriteRenderer;
            serializedPortal.ApplyModifiedProperties();

            // 포탈 라벨
            GameObject labelObj = new GameObject("PortalLabel");
            labelObj.transform.SetParent(portalObj.transform);
            labelObj.transform.localPosition = new Vector3(0, 1.5f, 0);

            TextMesh textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = "던전 입장";
            textMesh.fontSize = 40;
            textMesh.characterSize = 0.1f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = new Color(0, 1f, 1f);

            Debug.Log("[StartRoomSceneCreator] DungeonEntrance Portal 생성 완료");
        }

        private void CreateBackground()
        {
            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Quad);
            background.name = "Background";
            background.transform.position = new Vector3(0, 0, 10);
            background.transform.localScale = new Vector3(25, 15, 1);

            // Material 설정
            Renderer renderer = background.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material newMaterial = new Material(Shader.Find("Sprites/Default"));
                newMaterial.color = new Color(0.05f, 0.05f, 0.1f);
                renderer.sharedMaterial = newMaterial;
            }

            // Collider 제거
            Collider collider = background.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
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

            player.name = "Player";
            player.transform.position = new Vector3(-5f, 0f, 0f);
            player.tag = "Player";

            Debug.Log("[StartRoomSceneCreator] Player (MageForm) 생성 완료");
        }

        /// <summary>
        /// 상점 NPC 생성
        /// </summary>
        private void CreateShopNPC()
        {
            GameObject npcObj = new GameObject("ShopNPC");
            npcObj.transform.position = new Vector3(0, -2f, 0);
            npcObj.tag = "NPC";

            // SpriteRenderer
            SpriteRenderer sr = npcObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.8f, 0.6f, 0.2f);

            // 기본 스프라이트 생성 (사각형)
            Texture2D texture = new Texture2D(32, 48);
            Color[] pixels = new Color[32 * 48];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            Sprite npcSprite = Sprite.Create(texture, new Rect(0, 0, 32, 48), new Vector2(0.5f, 0f), 32);
            sr.sprite = npcSprite;

            // BoxCollider2D (상호작용용)
            BoxCollider2D collider = npcObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(2f, 2f);

            // NPC 표시 텍스트
            GameObject textObj = new GameObject("NPCLabel");
            textObj.transform.SetParent(npcObj.transform);
            textObj.transform.localPosition = new Vector3(0, 2f, 0);

            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = "상점\n[E]";
            textMesh.fontSize = 40;
            textMesh.characterSize = 0.1f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = new Color(1f, 0.85f, 0f);

            Debug.Log("[StartRoomSceneCreator] Shop NPC 생성 완료");
        }

        /// <summary>
        /// 폴더 재귀 생성
        /// </summary>
        private void CreateFolderRecursive(string path)
        {
            string[] folders = path.Split('/');
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
    }
}
