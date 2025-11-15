using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using GASPT.Gameplay.Level;
using GASPT.Core;
using GASPT.UI;
using GASPT.ResourceManagement;

namespace GASPT.Editor
{
    /// <summary>
    /// GameplayScene 자동 구성 에디터 도구
    /// 플레이 가능한 씬을 한 번의 클릭으로 생성
    /// </summary>
    public class GameplaySceneCreator : EditorWindow
    {
        private const string ScenePath = "Assets/_Project/Scenes/GameplayScene.unity";
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
                EditorApplication.delayCall += CreateGameplayScene;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // 개별 생성 버튼
            EditorGUILayout.LabelField("개별 생성 (현재 씬에):", EditorStyles.boldLabel);

            if (GUILayout.Button("플레이어만 생성"))
            {
                EditorApplication.delayCall += CreatePlayer;
            }

            if (GUILayout.Button("방 시스템만 생성"))
            {
                EditorApplication.delayCall += CreateRoomSystem;
            }

            if (GUILayout.Button("UI만 생성"))
            {
                EditorApplication.delayCall += CreateAllUI;
            }

            if (GUILayout.Button("카메라만 생성"))
            {
                EditorApplication.delayCall += CreateCameraSystem;
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

            // Layer 설정 (Ground)
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer == -1)
            {
                Debug.LogWarning("[GameplaySceneCreator] 'Ground' Layer가 없습니다! Project Settings > Tags and Layers에서 Layer 8을 'Ground'로 추가하세요.");
                ground.layer = 0; // Default layer
            }
            else
            {
                ground.layer = groundLayer;
            }
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

                // Layer 설정 (Ground)
                int groundLayer = LayerMask.NameToLayer("Ground");
                if (groundLayer == -1)
                {
                    Debug.LogWarning("[GameplaySceneCreator] 'Ground' Layer가 없습니다! Project Settings > Tags and Layers에서 Layer 8을 'Ground'로 추가하세요.");
                    platform.layer = 0; // Default layer
                }
                else
                {
                    platform.layer = groundLayer;
                }
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private void CreatePlayer()
        {
            // 프리팹 로드 (ResourcePaths 사용)
            GameObject mageFormPrefab = Resources.Load<GameObject>(ResourcePaths.Prefabs.Player.MageForm);

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
            // 다양한 EnemyData 로드
            string[] enemyDataPaths = new string[]
            {
                "Assets/_Project/Data/Enemies/TestGoblin.asset",      // BasicMelee
                "Assets/_Project/Data/Enemies/RangedGoblin.asset",    // Ranged (없으면 TestGoblin)
                "Assets/_Project/Data/Enemies/FlyingBat.asset",       // Flying (없으면 TestGoblin)
                "Assets/_Project/Data/Enemies/EliteOrc.asset"         // Elite (없으면 TestGoblin)
            };

            GASPT.Data.EnemyData[] enemyDatas = new GASPT.Data.EnemyData[enemyDataPaths.Length];
            for (int i = 0; i < enemyDataPaths.Length; i++)
            {
                enemyDatas[i] = AssetDatabase.LoadAssetAtPath<GASPT.Data.EnemyData>(enemyDataPaths[i]);
            }

            // TestGoblin은 반드시 있어야 함 (fallback)
            GASPT.Data.EnemyData fallbackData = enemyDatas[0];
            if (fallbackData == null)
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
            int[] enemyTypeCounts = new int[4]; // 각 타입별 생성 수 카운트

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

                    // 가중치 랜덤으로 EnemyData 선택
                    GASPT.Data.EnemyData selectedData = GetWeightedRandomEnemyData(enemyDatas, fallbackData, ref enemyTypeCounts);

                    // EnemyData 자동 할당
                    if (selectedData != null)
                    {
                        SerializedObject so = new SerializedObject(spawnPointComponent);
                        SerializedProperty enemyDataProp = so.FindProperty("enemyData");
                        enemyDataProp.objectReferenceValue = selectedData;
                        so.ApplyModifiedProperties();
                    }

                    // Gizmo 표시용 아이콘
                    #if UNITY_EDITOR
                    UnityEditor.EditorGUIUtility.SetIconForObject(spawnPoint, UnityEditor.EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo").image as Texture2D);
                    #endif

                    totalSpawnPoints++;
                }
            }

            Debug.Log($"[GameplaySceneCreator] 적 스폰 포인트 생성 완료 (총 {totalSpawnPoints}개)\n" +
                     $"BasicMelee: {enemyTypeCounts[0]}, Ranged: {enemyTypeCounts[1]}, Flying: {enemyTypeCounts[2]}, Elite: {enemyTypeCounts[3]}");
        }

        /// <summary>
        /// 가중치 랜덤으로 EnemyData 선택
        /// </summary>
        private GASPT.Data.EnemyData GetWeightedRandomEnemyData(GASPT.Data.EnemyData[] enemyDatas, GASPT.Data.EnemyData fallback, ref int[] counts)
        {
            float rand = Random.value;

            // 가중치: BasicMelee 40%, Ranged 30%, Flying 20%, Elite 10%
            int selectedIndex;
            if (rand < 0.4f)
                selectedIndex = 0; // BasicMelee
            else if (rand < 0.7f)
                selectedIndex = 1; // Ranged
            else if (rand < 0.9f)
                selectedIndex = 2; // Flying
            else
                selectedIndex = 3; // Elite

            // 선택된 EnemyData 가져오기 (없으면 fallback)
            GASPT.Data.EnemyData selected = enemyDatas[selectedIndex];
            if (selected == null)
            {
                selected = fallback;
                selectedIndex = 0; // fallback은 BasicMelee로 간주
            }

            // 카운트 증가
            if (counts != null && selectedIndex >= 0 && selectedIndex < counts.Length)
            {
                counts[selectedIndex]++;
            }

            return selected;
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

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem 생성
            if (GameObject.Find("EventSystem") == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Debug.Log("[GameplaySceneCreator] UI Canvas 및 EventSystem 생성 완료");

            // 모든 UI 요소 생성
            CreatePlayerHealthBarUI(canvas);
            CreatePlayerManaBarUI(canvas);
            CreatePlayerExpBarUI(canvas);
            CreateBuffIconPanelUI(canvas);
            CreateItemPickupUI(canvas);
            CreateRoomInfoUI(canvas);

            Debug.Log("[GameplaySceneCreator] 모든 UI 요소 생성 완료");
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


        // ====== UI 생성 메서드들 ======

        /// <summary>
        /// PlayerHealthBar UI 생성
        /// </summary>
        private void CreatePlayerHealthBarUI(Canvas canvas)
        {
            GameObject healthBarPanel = new GameObject("PlayerHealthBar");
            healthBarPanel.transform.SetParent(canvas.transform, false);

            // RectTransform 설정
            RectTransform rect = healthBarPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f); // 상단 중앙
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -50);
            rect.sizeDelta = new Vector2(400, 40);

            // 배경
            Image bgImage = healthBarPanel.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            // HP 슬라이더
            CreateUISlider(healthBarPanel, "HPSlider", new Vector2(0, 0), new Vector2(380, 20),
                          new Color(0.2f, 0.8f, 0.2f, 1f), out Slider hpSlider);

            // HP 텍스트
            CreateUIText(healthBarPanel, "HPText", new Vector2(0, 0), new Vector2(380, 30),
                        "100 / 100", 18, out TMP_Text hpText);

            // PlayerHealthBar 컴포넌트 추가
            PlayerHealthBar healthBar = healthBarPanel.AddComponent<PlayerHealthBar>();
            SerializedObject so = new SerializedObject(healthBar);
            so.FindProperty("hpSlider").objectReferenceValue = hpSlider;
            so.FindProperty("hpText").objectReferenceValue = hpText;
            so.ApplyModifiedProperties();

            Debug.Log("[GameplaySceneCreator] PlayerHealthBar 생성 완료");
        }

        /// <summary>
        /// PlayerManaBar UI 생성
        /// </summary>
        private void CreatePlayerManaBarUI(Canvas canvas)
        {
            GameObject manaBarPanel = new GameObject("PlayerManaBar");
            manaBarPanel.transform.SetParent(canvas.transform, false);

            // RectTransform 설정
            RectTransform rect = manaBarPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -100); // HealthBar 아래
            rect.sizeDelta = new Vector2(400, 30);

            // 배경
            Image bgImage = manaBarPanel.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            // Mana 슬라이더
            CreateUISlider(manaBarPanel, "ManaSlider", new Vector2(0, 0), new Vector2(380, 15),
                          new Color(0.2f, 0.5f, 1f, 1f), out Slider manaSlider);

            // Mana 텍스트
            CreateUIText(manaBarPanel, "ManaText", new Vector2(0, 0), new Vector2(380, 25),
                        "100 / 100", 14, out TMP_Text manaText);

            // PlayerManaBar 컴포넌트 추가
            PlayerManaBar manaBar = manaBarPanel.AddComponent<PlayerManaBar>();
            SerializedObject so = new SerializedObject(manaBar);
            so.FindProperty("manaSlider").objectReferenceValue = manaSlider;
            so.FindProperty("manaText").objectReferenceValue = manaText;
            so.ApplyModifiedProperties();

            Debug.Log("[GameplaySceneCreator] PlayerManaBar 생성 완료");
        }

        /// <summary>
        /// PlayerExpBar UI 생성
        /// </summary>
        private void CreatePlayerExpBarUI(Canvas canvas)
        {
            GameObject expBarPanel = new GameObject("PlayerExpBar");
            expBarPanel.transform.SetParent(canvas.transform, false);

            // RectTransform 설정
            RectTransform rect = expBarPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f); // 하단 중앙
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0, 10);
            rect.sizeDelta = new Vector2(600, 30);

            // 배경
            Image bgImage = expBarPanel.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);

            // Exp 슬라이더
            CreateUISlider(expBarPanel, "ExpSlider", new Vector2(50, 0), new Vector2(480, 15),
                          new Color(1f, 0.8f, 0.2f, 1f), out Slider expSlider);

            // Exp 텍스트
            CreateUIText(expBarPanel, "ExpText", new Vector2(50, 0), new Vector2(480, 25),
                        "0 / 100", 14, out TMP_Text expText);

            // 레벨 텍스트
            CreateUIText(expBarPanel, "LevelText", new Vector2(-240, 0), new Vector2(100, 30),
                        "Lv.1", 18, out TMP_Text levelText);

            // PlayerExpBar 컴포넌트 추가
            PlayerExpBar expBar = expBarPanel.AddComponent<PlayerExpBar>();
            SerializedObject so = new SerializedObject(expBar);
            so.FindProperty("expSlider").objectReferenceValue = expSlider;
            so.FindProperty("expText").objectReferenceValue = expText;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.ApplyModifiedProperties();

            Debug.Log("[GameplaySceneCreator] PlayerExpBar 생성 완료");
        }

        /// <summary>
        /// BuffIconPanel UI 생성
        /// </summary>
        private void CreateBuffIconPanelUI(Canvas canvas)
        {
            GameObject buffPanel = new GameObject("BuffIconPanel");
            buffPanel.transform.SetParent(canvas.transform, false);

            // RectTransform 설정
            RectTransform rect = buffPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f); // 왼쪽 상단
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20, -20);
            rect.sizeDelta = new Vector2(400, 80);

            // HorizontalLayoutGroup 추가
            HorizontalLayoutGroup layoutGroup = buffPanel.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 5;
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;

            // BuffIconPanel 컴포넌트 추가
            BuffIconPanel buffIconPanel = buffPanel.AddComponent<BuffIconPanel>();
            SerializedObject so = new SerializedObject(buffIconPanel);
            so.FindProperty("buffIconPrefab").objectReferenceValue = Resources.Load<GameObject>(ResourcePaths.Prefabs.UI.BuffIcon);
            so.FindProperty("iconContainer").objectReferenceValue = buffPanel.transform;
            so.ApplyModifiedProperties();

            Debug.Log("[GameplaySceneCreator] BuffIconPanel 생성 완료");
        }

        /// <summary>
        /// ItemPickupUI 생성
        /// </summary>
        private void CreateItemPickupUI(Canvas canvas)
        {
            GameObject pickupPanel = new GameObject("ItemPickupUIPanel");
            pickupPanel.transform.SetParent(canvas.transform, false);

            // RectTransform 설정
            RectTransform rect = pickupPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f); // 상단 중앙
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -150);
            rect.sizeDelta = new Vector2(400, 150);

            // VerticalLayoutGroup 추가
            VerticalLayoutGroup layoutGroup = pickupPanel.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 5;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;

            // ItemPickupUI 컴포넌트 추가
            ItemPickupUI pickupUI = pickupPanel.AddComponent<ItemPickupUI>();
            SerializedObject so = new SerializedObject(pickupUI);
            so.FindProperty("pickupSlotPrefab").objectReferenceValue = Resources.Load<GameObject>(ResourcePaths.Prefabs.UI.PickupSlot);
            so.FindProperty("slotContainer").objectReferenceValue = pickupPanel.transform;
            so.ApplyModifiedProperties();

            Debug.Log("[GameplaySceneCreator] ItemPickupUI 생성 완료");
        }

        /// <summary>
        /// 방 정보 UI 생성 (현재 방/총 방 수, 적 수)
        /// </summary>
        private void CreateRoomInfoUI(Canvas canvas)
        {
            GameObject roomInfoPanel = new GameObject("RoomInfoPanel");
            roomInfoPanel.transform.SetParent(canvas.transform, false);

            // RectTransform 설정
            RectTransform rect = roomInfoPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f); // 오른쪽 상단
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-20, -20);
            rect.sizeDelta = new Vector2(200, 80);

            // 배경
            Image bgImage = roomInfoPanel.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);

            // 방 정보 텍스트
            CreateUIText(roomInfoPanel, "RoomText", new Vector2(0, 15), new Vector2(180, 30),
                        "Room 1 / 3", 16, out TMP_Text roomText);

            // 적 수 텍스트
            CreateUIText(roomInfoPanel, "EnemyText", new Vector2(0, -15), new Vector2(180, 30),
                        "Enemies: 0", 16, out TMP_Text enemyText);

            // RoomInfoUI 컴포넌트 추가
            RoomInfoUI roomInfoUI = roomInfoPanel.AddComponent<RoomInfoUI>();
            SerializedObject so = new SerializedObject(roomInfoUI);
            so.FindProperty("roomText").objectReferenceValue = roomText;
            so.FindProperty("enemyText").objectReferenceValue = enemyText;
            so.ApplyModifiedProperties();

            Debug.Log("[GameplaySceneCreator] RoomInfoUI 생성 완료");
        }

        /// <summary>
        /// UI 슬라이더 생성 헬퍼
        /// </summary>
        private void CreateUISlider(GameObject parent, string name, Vector2 position, Vector2 size,
                                    Color fillColor, out Slider slider)
        {
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent.transform, false);

            RectTransform rect = sliderObj.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = fillColor;

            slider.fillRect = fillRect;
        }

        /// <summary>
        /// UI 텍스트 생성 헬퍼 (TextMeshPro)
        /// </summary>
        private void CreateUIText(GameObject parent, string name, Vector2 position, Vector2 size,
                                  string text, int fontSize, out TMP_Text tmpText)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = fontSize;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.Center;
        }
    }
}
