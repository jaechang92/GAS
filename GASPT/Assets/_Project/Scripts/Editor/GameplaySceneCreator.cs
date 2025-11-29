using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using GASPT.Gameplay.Level;
using GASPT.ResourceManagement;

namespace GASPT.Editor
{
    /// <summary>
    /// GameplayScene Content Scene 자동 구성 에디터 도구
    ///
    /// [Additive Scene Loading 구조]
    /// - 이 씬은 Content Scene으로 사용됨
    /// - 공통 UI는 PersistentManagers Scene에 존재
    /// - 이 씬에는 Content 요소만 포함:
    ///   - Player (MageForm)
    ///   - Room System (방들)
    ///   - 플랫폼/지면
    ///   - 적 스폰 포인트
    ///   - 카메라 (CameraFollow)
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

        private int roomCount = 3;
        private float roomWidth = 40f;
        private float roomHeight = 20f;

        [MenuItem("Tools/GASPT/🎮 Gameplay Scene Creator")]
        public static void ShowWindow()
        {
            GameplaySceneCreator window = GetWindow<GameplaySceneCreator>("Gameplay Scene Creator");
            window.minSize = new Vector2(450, 550);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("=== GameplayScene Content Creator ===", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "[Additive Scene Loading 구조]\n" +
                "이 씬은 Content Scene으로 사용됩니다.\n" +
                "Camera, EventSystem, UI는 PersistentManagers에 있습니다.\n\n" +
                "포함 요소:\n" +
                "✓ 플레이어 (MageForm)\n" +
                "✓ 방 시스템 (Room System)\n" +
                "✓ 플랫폼 및 지면\n" +
                "✓ 적 스폰 포인트\n\n" +
                "❌ Camera, EventSystem, UI → PersistentManagers",
                MessageType.Info
            );

            GUILayout.Space(20);

            // 생성 옵션
            EditorGUILayout.LabelField("생성 옵션:", EditorStyles.boldLabel);
            createPlayer = EditorGUILayout.Toggle("플레이어", createPlayer);
            createRooms = EditorGUILayout.Toggle("방 시스템", createRooms);
            createPlatforms = EditorGUILayout.Toggle("플랫폼", createPlatforms);
            createEnemies = EditorGUILayout.Toggle("적 스폰 포인트", createEnemies);

            GUILayout.Space(10);

            // 방 설정
            EditorGUILayout.LabelField("방 설정:", EditorStyles.boldLabel);
            roomCount = EditorGUILayout.IntSlider("방 개수", roomCount, 1, 5);
            roomWidth = EditorGUILayout.Slider("방 너비", roomWidth, 20f, 60f);
            roomHeight = EditorGUILayout.Slider("방 높이", roomHeight, 10f, 30f);

            GUILayout.Space(20);

            // 씬 생성 버튼
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🚀 GameplayScene Content 생성", GUILayout.Height(50)))
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

            GUILayout.Space(20);

            EditorGUILayout.HelpBox(
                "게임 실행 순서:\n" +
                "1. Bootstrap Scene (Index 0) 에서 시작\n" +
                "2. PersistentManagers Scene 로드 (UI 포함)\n" +
                "3. StartRoom에서 포탈 진입 시 GameplayScene 로드\n\n" +
                "Build Settings 설정:\n" +
                "- Index 0: Bootstrap\n" +
                "- Index 1: PersistentManagers\n" +
                "- Index 2: StartRoom\n" +
                "- Index 3: GameplayScene",
                MessageType.Warning
            );

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// GameplayScene Content 생성
        /// </summary>
        private void CreateGameplayScene()
        {
            Debug.Log("=== GameplayScene Content 생성 시작 ===");

            // 새 씬 생성 (빈 씬 - Camera와 EventSystem은 PersistentManagers에 있음)
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 참고: Main Camera와 EventSystem은 PersistentManagers Scene에 있음
            // Content Scene에는 게임 오브젝트만 배치

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

            // 씬 저장
            string scenesFolder = "Assets/_Project/Scenes";
            if (!AssetDatabase.IsValidFolder(scenesFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }

            EditorSceneManager.SaveScene(newScene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"=== GameplayScene Content 생성 완료! ===\n위치: {ScenePath}");
            EditorUtility.DisplayDialog("완료",
                "GameplayScene Content가 생성되었습니다!\n\n" +
                "UI는 PersistentManagers Scene에서 관리됩니다.", "확인");
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
            sr.sprite = CreatePlaceholderSprite(new Color(0.3f, 0.3f, 0.3f));
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(roomWidth, 1f);

            // BoxCollider2D 추가 (2D 충돌)
            BoxCollider2D collider = ground.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(roomWidth, 1f);

            // Layer 설정 (Ground)
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer == -1)
            {
                Debug.LogWarning("[GameplaySceneCreator] 'Ground' Layer가 없습니다! Project Settings > Tags and Layers에서 추가하세요.");
                ground.layer = 0;
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
                sr.sprite = CreatePlaceholderSprite(new Color(0.5f, 0.5f, 0.5f));
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(8f, 0.5f);

                // BoxCollider2D 추가 (2D 충돌)
                BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(8f, 0.5f);

                // Layer 설정 (Ground)
                int groundLayer = LayerMask.NameToLayer("Ground");
                if (groundLayer != -1)
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
            GameObject mageFormPrefab = Resources.Load<GameObject>(ResourcePaths.Prefabs.Player.MageForm);

            if (mageFormPrefab == null)
            {
                Debug.LogError("[GameplaySceneCreator] MageForm 프리팹을 찾을 수 없습니다!");
                return;
            }

            GameObject player = PrefabUtility.InstantiatePrefab(mageFormPrefab) as GameObject;
            player.name = "Player";
            player.transform.position = new Vector3(0f, 2f, 0f);
            player.tag = "Player";

            Debug.Log("[GameplaySceneCreator] 플레이어 생성 완료");
        }

        /// <summary>
        /// 적 스폰 포인트 생성
        /// </summary>
        private void CreateEnemySpawnPoints()
        {
            string[] enemyDataPaths = new string[]
            {
                "Assets/_Project/Data/Enemies/TestGoblin.asset",
                "Assets/_Project/Data/Enemies/RangedGoblin.asset",
                "Assets/_Project/Data/Enemies/FlyingBat.asset",
                "Assets/_Project/Data/Enemies/EliteOrc.asset"
            };

            GASPT.Data.EnemyData[] enemyDatas = new GASPT.Data.EnemyData[enemyDataPaths.Length];
            for (int i = 0; i < enemyDataPaths.Length; i++)
            {
                enemyDatas[i] = AssetDatabase.LoadAssetAtPath<GASPT.Data.EnemyData>(enemyDataPaths[i]);
            }

            GASPT.Data.EnemyData fallbackData = enemyDatas[0];

            GameObject roomsParent = GameObject.Find("=== ROOMS ===");
            if (roomsParent == null)
            {
                Debug.LogError("[GameplaySceneCreator] === ROOMS === 부모 오브젝트를 찾을 수 없습니다!");
                return;
            }

            int totalSpawnPoints = 0;
            int[] enemyTypeCounts = new int[4];

            for (int roomIndex = 0; roomIndex < roomCount; roomIndex++)
            {
                if (roomIndex == 0) continue;

                string roomName = roomIndex == roomCount - 1 ? "BossRoom" : $"Room_{roomIndex}";
                Transform roomTransform = roomsParent.transform.Find(roomName);

                if (roomTransform == null) continue;

                int spawnCount = roomIndex == roomCount - 1 ? 1 : Random.Range(2, 5);

                for (int i = 0; i < spawnCount; i++)
                {
                    GameObject spawnPoint = new GameObject($"EnemySpawnPoint_{i}");
                    spawnPoint.transform.SetParent(roomTransform);

                    float xOffset = Random.Range(-15f, 15f);
                    float yPos = 2f;
                    spawnPoint.transform.localPosition = new Vector3(xOffset, yPos, 0f);

                    var spawnPointComponent = spawnPoint.AddComponent<EnemySpawnPoint>();

                    GASPT.Data.EnemyData selectedData = GetWeightedRandomEnemyData(enemyDatas, fallbackData, ref enemyTypeCounts);

                    if (selectedData != null)
                    {
                        SerializedObject so = new SerializedObject(spawnPointComponent);
                        SerializedProperty enemyDataProp = so.FindProperty("enemyData");
                        enemyDataProp.objectReferenceValue = selectedData;
                        so.ApplyModifiedProperties();
                    }

                    totalSpawnPoints++;
                }
            }

            Debug.Log($"[GameplaySceneCreator] 적 스폰 포인트 생성 완료 (총 {totalSpawnPoints}개)");
        }

        /// <summary>
        /// 가중치 랜덤으로 EnemyData 선택
        /// </summary>
        private GASPT.Data.EnemyData GetWeightedRandomEnemyData(GASPT.Data.EnemyData[] enemyDatas, GASPT.Data.EnemyData fallback, ref int[] counts)
        {
            float rand = Random.value;

            int selectedIndex;
            if (rand < 0.4f)
                selectedIndex = 0;
            else if (rand < 0.7f)
                selectedIndex = 1;
            else if (rand < 0.9f)
                selectedIndex = 2;
            else
                selectedIndex = 3;

            GASPT.Data.EnemyData selected = enemyDatas[selectedIndex];
            if (selected == null)
            {
                selected = fallback;
                selectedIndex = 0;
            }

            if (counts != null && selectedIndex >= 0 && selectedIndex < counts.Length)
            {
                counts[selectedIndex]++;
            }

            return selected;
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
                EditorUtility.DisplayDialog("경고", "GameplayScene이 아직 생성되지 않았습니다!", "확인");
            }
        }

        /// <summary>
        /// Placeholder 스프라이트 생성 및 에셋으로 저장
        /// </summary>
        private Sprite CreatePlaceholderSprite(Color color)
        {
            CreateFolderIfNotExists(TexturesPath);

            string colorName = GetColorName(color);
            string texturePath = $"{TexturesPath}/Placeholder_{colorName}.png";

            Texture2D existingTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (existingTexture != null)
            {
                Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                if (existingSprite != null)
                {
                    return existingSprite;
                }
            }

            Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            byte[] pngData = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(texturePath, pngData);

            AssetDatabase.ImportAsset(texturePath);

            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 32f;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        }

        /// <summary>
        /// 색상에 따른 이름 생성
        /// </summary>
        private string GetColorName(Color color)
        {
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

                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    CreateFolderIfNotExists(parentPath);
                }

                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }
    }
}
