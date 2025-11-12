using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using GASPT.Gameplay.Level;
using GASPT.Core;

namespace GASPT.Editor
{
    /// <summary>
    /// GameplayScene 자동 구성 에디터 도구
    /// 플레이 가능한 씬을 한 번의 클릭으로 생성
    /// </summary>
    public class GameplaySceneCreator : EditorWindow
    {
        private const string ScenePath = "Assets/_Project/Scenes/GameplayScene.unity";
        private const string PrefabsPath = "Prefabs";
        private const string TexturesPath = "Assets/Resources/Textures/Placeholders";

        private Vector2 scrollPosition;
        private bool createPlayer = true;
        private bool createRooms = true;
        private bool createPlatforms = true;
        private bool createEnemies = true;
        private bool createUI = true;
        private bool createCamera = true;
        private bool createSingletons = true;

        private int roomCount = 3;
        private float roomWidth = 40f;
        private float roomHeight = 20f;

        [MenuItem("Tools/GASPT/🎮 Gameplay Scene Creator")]
        public static void ShowWindow()
        {
            GameplaySceneCreator window = GetWindow<GameplaySceneCreator>("Gameplay Scene Creator");
            window.minSize = new Vector2(450, 650);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("=== GameplayScene Creator ===", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "플레이 가능한 GameplayScene을 자동으로 생성합니다.\n\n" +
                "포함 요소:\n" +
                "✓ 플레이어 (MageForm)\n" +
                "✓ 3개 방 (Room System)\n" +
                "✓ 플랫폼 및 지면\n" +
                "✓ 적 스폰 포인트\n" +
                "✓ UI (Health, Mana, Exp, BuffIcon, ItemPickup)\n" +
                "✓ 카메라 (CameraFollow)\n" +
                "✓ Singleton Manager",
                MessageType.Info
            );

            GUILayout.Space(20);

            // 생성 옵션
            EditorGUILayout.LabelField("생성 옵션:", EditorStyles.boldLabel);
            createPlayer = EditorGUILayout.Toggle("플레이어", createPlayer);
            createRooms = EditorGUILayout.Toggle("방 시스템", createRooms);
            createPlatforms = EditorGUILayout.Toggle("플랫폼", createPlatforms);
            createEnemies = EditorGUILayout.Toggle("적 스폰 포인트", createEnemies);
            createUI = EditorGUILayout.Toggle("UI", createUI);
            createCamera = EditorGUILayout.Toggle("카메라", createCamera);
            createSingletons = EditorGUILayout.Toggle("Singleton Manager", createSingletons);

            GUILayout.Space(10);

            // 방 설정
            EditorGUILayout.LabelField("방 설정:", EditorStyles.boldLabel);
            roomCount = EditorGUILayout.IntSlider("방 개수", roomCount, 1, 5);
            roomWidth = EditorGUILayout.Slider("방 너비", roomWidth, 20f, 60f);
            roomHeight = EditorGUILayout.Slider("방 높이", roomHeight, 10f, 30f);

            GUILayout.Space(20);

            // 씬 생성 버튼
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🚀 GameplayScene 생성", GUILayout.Height(50)))
            {
                CreateGameplayScene();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // 개별 생성 버튼
            EditorGUILayout.LabelField("개별 생성 (현재 씬에):", EditorStyles.boldLabel);

            if (GUILayout.Button("플레이어만 생성"))
            {
                CreatePlayer();
            }

            if (GUILayout.Button("방 시스템만 생성"))
            {
                CreateRoomSystem();
            }

            if (GUILayout.Button("UI만 생성"))
            {
                CreateAllUI();
            }

            if (GUILayout.Button("카메라만 생성"))
            {
                CreateCameraSystem();
            }

            GUILayout.Space(20);

            EditorGUILayout.HelpBox(
                $"씬 저장 위치: {ScenePath}\n\n" +
                "프리팹이 먼저 생성되어 있어야 합니다!\n" +
                "Tools > GASPT > Prefab Creator 실행",
                MessageType.Warning
            );

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// GameplayScene 생성
        /// </summary>
        private void CreateGameplayScene()
        {
            Debug.Log("=== GameplayScene 생성 시작 ===");

            // 새 씬 생성
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Directional Light 삭제 (2D 게임이므로 불필요)
            GameObject directionalLight = GameObject.Find("Directional Light");
            if (directionalLight != null)
            {
                DestroyImmediate(directionalLight);
            }

            // Main Camera 설정
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 10f;
                mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f); // 어두운 파란색 배경
            }

            // Singleton Manager 생성
            if (createSingletons)
            {
                CreateSingletonManager();
            }

            // Room System 생성
            if (createRooms)
            {
                CreateRoomSystem();
            }

            // 플랫폼 생성
            if (createPlatforms)
            {
                CreatePlatforms();
            }

            // 플레이어 생성
            if (createPlayer)
            {
                CreatePlayer();
            }

            // 적 스폰 포인트 생성
            if (createEnemies)
            {
                CreateEnemySpawnPoints();
            }

            // UI 생성
            if (createUI)
            {
                CreateAllUI();
            }

            // 카메라 시스템 생성
            if (createCamera)
            {
                CreateCameraSystem();
            }

            // 씬 저장
            string scenesFolder = "Assets/_Project/Scenes";
            if (!AssetDatabase.IsValidFolder(scenesFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }

            EditorSceneManager.SaveScene(newScene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"=== GameplayScene 생성 완료! ===\n위치: {ScenePath}");
            EditorUtility.DisplayDialog("완료", "GameplayScene이 생성되었습니다!\n\n이제 Play 버튼을 눌러 테스트해보세요!", "확인");
        }

        /// <summary>
        /// Singleton Manager 생성
        /// </summary>
        private void CreateSingletonManager()
        {
            GameObject singletonObj = new GameObject("=== SINGLETONS ===");
            singletonObj.AddComponent<SingletonPreloader>();
            Debug.Log("[GameplaySceneCreator] SingletonPreloader 생성 완료");
        }

        /// <summary>
        /// Room System 생성
        /// </summary>
        private void CreateRoomSystem()
        {
            GameObject roomsParent = new GameObject("=== ROOMS ===");

            // RoomManager 생성
            GameObject roomManagerObj = new GameObject("RoomManager");
            roomManagerObj.transform.SetParent(roomsParent.transform);
            RoomManager roomManager = roomManagerObj.AddComponent<RoomManager>();

            // 방들 생성
            for (int i = 0; i < roomCount; i++)
            {
                CreateRoom(i, roomsParent.transform);
            }

            Debug.Log($"[GameplaySceneCreator] {roomCount}개 방 시스템 생성 완료");
        }

        /// <summary>
        /// 개별 방 생성
        /// </summary>
        private void CreateRoom(int roomIndex, Transform parent)
        {
            string roomName = roomIndex == 0 ? "StartRoom" :
                             roomIndex == roomCount - 1 ? "BossRoom" :
                             $"Room_{roomIndex}";

            GameObject roomObj = new GameObject(roomName);
            roomObj.transform.SetParent(parent);
            roomObj.transform.position = new Vector3(roomIndex * roomWidth, 0, 0);

            Room room = roomObj.AddComponent<Room>();

            // 방 경계 시각화 (Gizmo용)
            GameObject boundary = new GameObject("Boundary");
            boundary.transform.SetParent(roomObj.transform);
            boundary.transform.localPosition = Vector3.zero;

            // 방 정보 로그
            Debug.Log($"[GameplaySceneCreator] {roomName} 생성 (위치: {roomObj.transform.position})");
        }

        /// <summary>
        /// 플랫폼 생성
        /// </summary>
        private void CreatePlatforms()
        {
            GameObject platformsParent = new GameObject("=== PLATFORMS ===");

            // 각 방마다 지면 생성
            for (int i = 0; i < roomCount; i++)
            {
                CreateGroundPlatform(i, platformsParent.transform);
                CreateJumpPlatforms(i, platformsParent.transform);
            }

            Debug.Log("[GameplaySceneCreator] 플랫폼 생성 완료");
        }

        /// <summary>
        /// 지면 플랫폼 생성
        /// </summary>
        private void CreateGroundPlatform(int roomIndex, Transform parent)
        {
            GameObject ground = new GameObject($"Ground_Room{roomIndex}");
            ground.transform.SetParent(parent);

            float xPos = roomIndex * roomWidth;
            ground.transform.position = new Vector3(xPos, -2f, 0f);

            // SpriteRenderer 추가 (2D)
            SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.3f, 0.3f, 0.3f)); // 회색
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(roomWidth, 1f);

            // BoxCollider2D 추가 (2D 충돌)
            BoxCollider2D collider = ground.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(roomWidth, 1f);

            ground.layer = LayerMask.NameToLayer("Default");
        }

        /// <summary>
        /// 점프 플랫폼 생성
        /// </summary>
        private void CreateJumpPlatforms(int roomIndex, Transform parent)
        {
            // 각 방에 2~3개 점프 플랫폼 생성
            int platformCount = Random.Range(2, 4);

            for (int i = 0; i < platformCount; i++)
            {
                GameObject platform = new GameObject($"Platform_Room{roomIndex}_{i}");
                platform.transform.SetParent(parent);

                float xPos = roomIndex * roomWidth + Random.Range(-15f, 15f);
                float yPos = Random.Range(2f, 10f);
                platform.transform.position = new Vector3(xPos, yPos, 0f);

                // SpriteRenderer 추가 (2D)
                SpriteRenderer sr = platform.AddComponent<SpriteRenderer>();
                sr.sprite = CreatePlaceholderSprite(new Color(0.5f, 0.5f, 0.5f)); // 회색
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(8f, 0.5f);

                // BoxCollider2D 추가 (2D 충돌)
                BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(8f, 0.5f);
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private void CreatePlayer()
        {
            // 프리팹 로드
            GameObject mageFormPrefab = Resources.Load<GameObject>($"{PrefabsPath}/Player/MageForm");

            if (mageFormPrefab == null)
            {
                Debug.LogError("[GameplaySceneCreator] MageForm 프리팹을 찾을 수 없습니다! Prefab Creator를 먼저 실행하세요.");
                EditorUtility.DisplayDialog("오류", "MageForm 프리팹이 없습니다!\n\nTools > GASPT > Prefab Creator를 먼저 실행하세요.", "확인");
                return;
            }

            GameObject player = PrefabUtility.InstantiatePrefab(mageFormPrefab) as GameObject;
            player.name = "Player";
            player.transform.position = new Vector3(0f, 2f, 0f); // 시작 방 위치
            player.tag = "Player";

            Debug.Log("[GameplaySceneCreator] 플레이어 생성 완료");
        }

        /// <summary>
        /// 적 스폰 포인트 생성
        /// </summary>
        private void CreateEnemySpawnPoints()
        {
            // TestGoblin EnemyData 로드
            GASPT.Data.EnemyData testGoblinData = AssetDatabase.LoadAssetAtPath<GASPT.Data.EnemyData>("Assets/_Project/Data/Enemies/TestGoblin.asset");

            if (testGoblinData == null)
            {
                Debug.LogWarning("[GameplaySceneCreator] TestGoblin EnemyData를 찾을 수 없습니다! 스폰 포인트에 EnemyData가 할당되지 않습니다.");
            }

            // === ROOMS === GameObject 찾기
            GameObject roomsParent = GameObject.Find("=== ROOMS ===");
            if (roomsParent == null)
            {
                Debug.LogError("[GameplaySceneCreator] === ROOMS === 부모 오브젝트를 찾을 수 없습니다!");
                return;
            }

            int totalSpawnPoints = 0;

            // 각 방마다 2~4개 스폰 포인트 생성
            for (int roomIndex = 0; roomIndex < roomCount; roomIndex++)
            {
                // 첫 번째 방(시작 방)은 스폰 포인트 없음
                if (roomIndex == 0) continue;

                // 해당 Room GameObject 찾기
                string roomName = roomIndex == roomCount - 1 ? "BossRoom" : $"Room_{roomIndex}";
                Transform roomTransform = roomsParent.transform.Find(roomName);

                if (roomTransform == null)
                {
                    Debug.LogWarning($"[GameplaySceneCreator] {roomName}을 찾을 수 없습니다!");
                    continue;
                }

                int spawnCount = roomIndex == roomCount - 1 ? 1 : Random.Range(2, 5); // 보스방은 1개

                for (int i = 0; i < spawnCount; i++)
                {
                    GameObject spawnPoint = new GameObject($"EnemySpawnPoint_{i}");
                    // Room의 자식으로 설정
                    spawnPoint.transform.SetParent(roomTransform);

                    float xOffset = Random.Range(-15f, 15f);
                    float yPos = 2f;
                    spawnPoint.transform.localPosition = new Vector3(xOffset, yPos, 0f);

                    // EnemySpawnPoint 컴포넌트 추가
                    var spawnPointComponent = spawnPoint.AddComponent<EnemySpawnPoint>();

                    // EnemyData 자동 할당
                    if (testGoblinData != null)
                    {
                        SerializedObject so = new SerializedObject(spawnPointComponent);
                        SerializedProperty enemyDataProp = so.FindProperty("enemyData");
                        enemyDataProp.objectReferenceValue = testGoblinData;
                        so.ApplyModifiedProperties();
                    }

                    // Gizmo 표시용 아이콘
                    #if UNITY_EDITOR
                    UnityEditor.EditorGUIUtility.SetIconForObject(spawnPoint, UnityEditor.EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo").image as Texture2D);
                    #endif

                    totalSpawnPoints++;
                }
            }

            Debug.Log($"[GameplaySceneCreator] 적 스폰 포인트 생성 완료 (총 {totalSpawnPoints}개, EnemyData: {(testGoblinData != null ? testGoblinData.enemyName : "None")})");
        }

        /// <summary>
        /// 모든 UI 생성
        /// </summary>
        private void CreateAllUI()
        {
            // Canvas 생성
            GameObject canvasObj = new GameObject("=== UI CANVAS ===");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var scaler = canvasObj.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // EventSystem 생성
            if (GameObject.Find("EventSystem") == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Debug.Log("[GameplaySceneCreator] UI Canvas 및 EventSystem 생성 완료");
            Debug.Log("[GameplaySceneCreator] UI 요소는 Tools > GASPT > UI Creator 메뉴에서 개별 생성하세요");
        }

        /// <summary>
        /// 카메라 시스템 생성
        /// </summary>
        private void CreateCameraSystem()
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("[GameplaySceneCreator] Main Camera를 찾을 수 없습니다!");
                return;
            }

            // CameraFollow 컴포넌트 추가
            var cameraFollow = mainCamera.GetComponent<GASPT.Gameplay.Camera.CameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<GASPT.Gameplay.Camera.CameraFollow>();
            }

            // 플레이어 타겟 설정 (런타임에 자동으로 찾도록 설정됨)
            Debug.Log("[GameplaySceneCreator] CameraFollow 생성 완료");
        }

        /// <summary>
        /// 씬 열기 버튼
        /// </summary>
        [MenuItem("Tools/GASPT/📂 Open GameplayScene")]
        public static void OpenGameplayScene()
        {
            if (System.IO.File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
                Debug.Log($"[GameplaySceneCreator] GameplayScene 열기 완료: {ScenePath}");
            }
            else
            {
                Debug.LogWarning($"[GameplaySceneCreator] GameplayScene을 찾을 수 없습니다: {ScenePath}");
                EditorUtility.DisplayDialog("경고", "GameplayScene이 아직 생성되지 않았습니다!\n\nGameplay Scene Creator를 먼저 실행하세요.", "확인");
            }
        }

        /// <summary>
        /// Placeholder 스프라이트 생성 및 에셋으로 저장
        /// </summary>
        private Sprite CreatePlaceholderSprite(Color color)
        {
            // Textures 폴더 생성
            CreateFolderIfNotExists(TexturesPath);

            // 색상 기반 파일명 생성
            string colorName = GetColorName(color);
            string texturePath = $"{TexturesPath}/Placeholder_{colorName}.png";
            string textureAssetPath = texturePath;

            // 이미 존재하면 로드
            Texture2D existingTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureAssetPath);
            if (existingTexture != null)
            {
                Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(textureAssetPath);
                if (existingSprite != null)
                {
                    return existingSprite;
                }
            }

            // 새 텍스처 생성
            Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            // PNG로 인코딩 및 저장
            byte[] pngData = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(texturePath, pngData);

            AssetDatabase.ImportAsset(textureAssetPath);

            // TextureImporter 설정 (Sprite로 변환)
            TextureImporter importer = AssetImporter.GetAtPath(textureAssetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 32f;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            // 생성된 Sprite 로드
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(textureAssetPath);

            if (sprite == null)
            {
                Debug.LogError($"[GameplaySceneCreator] Sprite 생성 실패: {textureAssetPath}");
            }

            return sprite;
        }

        /// <summary>
        /// 색상에 따른 이름 생성
        /// </summary>
        private string GetColorName(Color color)
        {
            // RGB 값을 16진수로 변환
            int r = Mathf.RoundToInt(color.r * 255f);
            int g = Mathf.RoundToInt(color.g * 255f);
            int b = Mathf.RoundToInt(color.b * 255f);
            return $"{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// 폴더가 없으면 생성
        /// </summary>
        private void CreateFolderIfNotExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentPath = System.IO.Path.GetDirectoryName(path);
                string folderName = System.IO.Path.GetFileName(path);

                // 부모 폴더가 없으면 재귀적으로 생성
                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    CreateFolderIfNotExists(parentPath);
                }

                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }
    }
}
