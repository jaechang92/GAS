using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using GASPT.Data;
using GASPT.Gameplay.Level;
using GASPT.Gameplay.Form;
using GASPT.Gameplay.Player;
using GASPT.Gameplay.Camera;
using GASPT.Stats;
using GASPT.Core.Enums;

namespace GASPT.Editor
{
    /// <summary>
    /// IntegrationTestScene 원클릭 생성 에디터 도구
    /// Phase A-1/A-2/A-3 통합 테스트용 Scene 자동 구성
    /// Menu: Tools > GASPT > Integration Test > 🚀 Create Integration Test Scene
    /// </summary>
    public class IntegrationTestSceneSetup
    {
        private const string SCENE_NAME = "IntegrationTestScene";
        private const string SCENE_PATH = "Assets/_Project/Scenes/" + SCENE_NAME + ".unity";
        private const string ENEMY_DATA_FOLDER = "Assets/_Project/Data/Enemies";
        private const string ROOM_DATA_FOLDER = "Assets/_Project/Data/Rooms";

        [MenuItem("Tools/GASPT/Integration Test/🚀 Create Integration Test Scene", false, 100)]
        public static void CreateIntegrationTestScene()
        {
            bool confirm = EditorUtility.DisplayDialog(
                "Integration Test Scene 생성",
                "Phase A-1/A-2/A-3 통합 테스트 Scene을 자동 생성합니다.\n\n" +
                "생성 내용:\n" +
                "- Ground\n" +
                "- TestPlayer (MageForm, PlayerController, FormInputHandler)\n" +
                "- Main Camera (CameraFollow)\n" +
                "- Room_01, Room_02 (각 4개 SpawnPoint, Portal)\n" +
                "- RoomManager\n" +
                "- TestGoblin EnemyData\n" +
                "- TestRoom_Normal RoomData\n\n" +
                "생성하시겠습니까?",
                "생성",
                "취소"
            );

            if (!confirm) return;

            Debug.Log("========== Integration Test Scene 생성 시작 ==========");

            // 1. 새 Scene 생성
            Scene scene = CreateNewScene();

            // 2. 폴더 생성
            CreateFolders();

            // 3. EnemyData 생성 (TestGoblin)
            EnemyData testGoblin = CreateTestGoblinData();

            // 4. RoomData 생성 (TestRoom_Normal)
            RoomData testRoomNormal = CreateTestRoomNormalData(testGoblin);

            // 5. Ground 생성
            GameObject ground = CreateGround();

            // 6. TestPlayer 생성
            GameObject player = CreatePlayer();

            // 7. Main Camera 설정
            SetupCamera(player);

            // 8. Room_01 생성
            GameObject room01 = CreateRoom("Room_01", new Vector3(0, 0, 0), ground, testRoomNormal);

            // 9. Room_02 생성
            GameObject room02 = CreateRoom("Room_02", new Vector3(40, 0, 0), null, testRoomNormal);
            room02.SetActive(false); // 시작 시 비활성화

            // 10. RoomManager 생성
            CreateRoomManager();

            // 11. Scene 저장
            EditorSceneManager.SaveScene(scene, SCENE_PATH);

            // 12. 생성된 에셋 새로고침
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("========== Integration Test Scene 생성 완료 ==========");
            Debug.Log($"Scene 경로: {SCENE_PATH}");
            Debug.Log($"EnemyData: {ENEMY_DATA_FOLDER}/TestGoblin.asset");
            Debug.Log($"RoomData: {ROOM_DATA_FOLDER}/TestRoom_Normal.asset");

            EditorUtility.DisplayDialog(
                "완료",
                $"Integration Test Scene이 생성되었습니다!\n\n" +
                $"Scene: {SCENE_PATH}\n" +
                $"EnemyData: TestGoblin.asset\n" +
                $"RoomData: TestRoom_Normal.asset\n\n" +
                $"Play 모드 진입 후:\n" +
                $"1. RoomManager 우클릭 > \"Start Dungeon (Test)\"\n" +
                $"2. WASD: 이동, Space: 점프\n" +
                $"3. 마우스 좌클릭: Magic Missile\n" +
                $"4. Q: Teleport, E: Fireball",
                "확인"
            );
        }

        // ====== Scene 생성 ======

        private static Scene CreateNewScene()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Debug.Log("[Setup] 새 Scene 생성 완료 (2D Default)");
            return newScene;
        }

        // ====== 폴더 생성 ======

        private static void CreateFolders()
        {
            // Scenes 폴더
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }

            // Data 폴더
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Data");
            }

            // Enemies 폴더
            if (!AssetDatabase.IsValidFolder(ENEMY_DATA_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Enemies");
            }

            // Rooms 폴더
            if (!AssetDatabase.IsValidFolder(ROOM_DATA_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Rooms");
            }

            Debug.Log("[Setup] 폴더 구조 확인 완료");
        }

        // ====== EnemyData 생성 ======

        private static EnemyData CreateTestGoblinData()
        {
            string path = $"{ENEMY_DATA_FOLDER}/TestGoblin.asset";

            // 기존 에셋이 있으면 로드
            EnemyData existing = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (existing != null)
            {
                Debug.Log($"[Setup] 기존 EnemyData 사용: {path}");
                return existing;
            }

            // 새로 생성
            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();
            SetEnemyDataFields(
                data,
                enemyName: "Test Goblin",
                enemyType: EnemyType.Normal,
                level: 1,
                maxHealth: 30,
                attackPower: 5,
                defense: 0,
                expReward: 5,
                goldReward: 10,
                // 플랫포머 설정
                moveSpeed: 2f,
                detectionRange: 5f,
                attackRange: 1.5f,
                patrolDistance: 3f,
                chaseSpeed: 3f,
                attackCooldown: 1.5f
            );

            AssetDatabase.CreateAsset(data, path);
            Debug.Log($"[Setup] EnemyData 생성: {path}");
            return data;
        }

        private static void SetEnemyDataFields(
            EnemyData data,
            string enemyName,
            EnemyType enemyType,
            int level,
            int maxHealth,
            int attackPower,
            int defense,
            int expReward,
            int goldReward,
            float moveSpeed,
            float detectionRange,
            float attackRange,
            float patrolDistance,
            float chaseSpeed,
            float attackCooldown)
        {
            // EnemyData의 필드가 public이므로 직접 할당
            data.enemyName = enemyName;
            data.enemyType = enemyType;
            // level, defense는 EnemyData에 필드 없음 (무시)
            data.maxHp = maxHealth;
            data.attack = attackPower;
            data.expReward = expReward;
            data.minGoldDrop = goldReward; // 최소 골드
            data.maxGoldDrop = goldReward + 10; // 최대 골드 (+10)

            // 플랫포머 설정
            data.moveSpeed = moveSpeed;
            data.detectionRange = detectionRange;
            data.attackRange = attackRange;
            data.patrolDistance = patrolDistance;
            data.chaseSpeed = chaseSpeed;
            data.attackCooldown = attackCooldown;
        }

        // ====== RoomData 생성 ======

        private static RoomData CreateTestRoomNormalData(EnemyData testGoblin)
        {
            string path = $"{ROOM_DATA_FOLDER}/TestRoom_Normal.asset";

            // 기존 에셋이 있으면 로드
            RoomData existing = AssetDatabase.LoadAssetAtPath<RoomData>(path);
            if (existing != null)
            {
                Debug.Log($"[Setup] 기존 RoomData 사용: {path}");
                return existing;
            }

            // 새로 생성
            RoomData data = ScriptableObject.CreateInstance<RoomData>();
            SetRoomDataFields(
                data,
                roomName: "Test Room - Normal",
                roomType: RoomType.Normal,
                difficulty: 2,
                minEnemyCount: 2,
                maxEnemyCount: 3,
                enemySpawns: new EnemySpawnData[]
                {
                    new EnemySpawnData
                    {
                        enemyData = testGoblin,
                        spawnChance = 1.0f,
                        minCount = 2,
                        maxCount = 3
                    }
                },
                clearCondition: ClearCondition.KillAllEnemies,
                timeLimit: 0f,
                bonusGold: 50,
                bonusExp: 20
            );

            AssetDatabase.CreateAsset(data, path);
            Debug.Log($"[Setup] RoomData 생성: {path}");
            return data;
        }

        private static void SetRoomDataFields(
            RoomData data,
            string roomName,
            RoomType roomType,
            int difficulty,
            int minEnemyCount,
            int maxEnemyCount,
            EnemySpawnData[] enemySpawns,
            ClearCondition clearCondition,
            float timeLimit,
            int bonusGold,
            int bonusExp)
        {
            // Reflection으로 private 필드 설정
            var type = typeof(RoomData);

            type.GetField("roomName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(data, roomName);
            type.GetField("roomType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(data, roomType);
            type.GetField("difficulty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(data, difficulty);
            type.GetField("minEnemyCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(data, minEnemyCount);
            type.GetField("maxEnemyCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(data, maxEnemyCount);
            type.GetField("enemySpawns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(data, enemySpawns);
            type.GetField("clearCondition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(data, clearCondition);
            type.GetField("timeLimit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(data, timeLimit);
            type.GetField("bonusGold", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(data, bonusGold);
            type.GetField("bonusExp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(data, bonusExp);
        }

        // ====== Ground 생성 ======

        private static GameObject CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0, -3, 0);
            ground.transform.localScale = new Vector3(30, 1, 1);

            // 3D Collider 제거
            Object.DestroyImmediate(ground.GetComponent<BoxCollider>());

            // BoxCollider2D 추가
            var collider2D = ground.AddComponent<BoxCollider2D>();
            collider2D.size = new Vector2(1, 1); // localScale로 크기 조정되므로 1x1

            // Layer 설정
            int groundLayer = GetOrCreateLayer("Ground");
            ground.layer = groundLayer;

            // Material 색상 설정 (sharedMaterial 사용)
            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial.color = new Color(0.5f, 0.5f, 0.5f); // Gray
            }

            Debug.Log("[Setup] Ground 생성 완료 (BoxCollider2D, Layer: Ground)");
            return ground;
        }

        // ====== Player 생성 ======

        private static GameObject CreatePlayer()
        {
            GameObject player = new GameObject("TestPlayer");
            player.tag = "Player"; // 태그 설정 필수!
            player.transform.position = new Vector3(0, 0, 0);

            // Rigidbody2D
            var rb = player.AddComponent<Rigidbody2D>();
            rb.mass = 1f;
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // BoxCollider2D
            var collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.8f, 1.5f);

            // SpriteRenderer (임시 비주얼)
            var spriteRenderer = player.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateDefaultSprite();
            spriteRenderer.color = Color.blue;

            // MageForm
            var mageForm = player.AddComponent<MageForm>();

            // FormInputHandler
            var inputHandler = player.AddComponent<FormInputHandler>();

            // PlayerController
            var controller = player.AddComponent<PlayerController>();

            // PlayerController의 groundLayer 설정
            int groundLayerIndex = GetOrCreateLayer("Ground");
            var groundLayerField = typeof(PlayerController).GetField("groundLayer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (groundLayerField != null)
            {
                // LayerMask는 struct이므로 명시적 변환 필요
                LayerMask layerMask = 1 << groundLayerIndex;
                groundLayerField.SetValue(controller, layerMask);
            }

            // PlayerStats
            var stats = player.AddComponent<PlayerStats>();

            Debug.Log("[Setup] TestPlayer 생성 완료 (MageForm, FormInputHandler, PlayerController, PlayerStats)");
            return player;
        }

        // ====== Camera 설정 ======

        private static void SetupCamera(GameObject player)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
            }

            // Camera 설정
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 7f;
            mainCamera.transform.position = new Vector3(0, 0, -10);

            // CameraFollow 추가
            var cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();

            // Target 설정 (Reflection 또는 public이면 직접)
            var targetField = typeof(CameraFollow).GetField("target",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (targetField != null)
            {
                targetField.SetValue(cameraFollow, player.transform);
            }

            Debug.Log("[Setup] Main Camera 설정 완료 (CameraFollow)");
        }

        // ====== Room 생성 ======

        private static GameObject CreateRoom(string roomName, Vector3 position, GameObject existingGround, RoomData roomData)
        {
            GameObject room = new GameObject(roomName);
            room.transform.position = position;

            // Room 컴포넌트
            var roomComponent = room.AddComponent<Room>();

            // RoomData 할당 (Reflection)
            var roomDataField = typeof(Room).GetField("roomData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (roomDataField != null)
            {
                roomDataField.SetValue(roomComponent, roomData);
            }

            // Ground 추가 (Room_01은 기존 Ground 사용, Room_02는 새로 생성)
            GameObject ground;
            if (existingGround != null)
            {
                ground = existingGround;
                ground.transform.SetParent(room.transform);
            }
            else
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ground.name = "Ground";
                ground.transform.SetParent(room.transform);
                ground.transform.localPosition = new Vector3(0, -3, 0);
                ground.transform.localScale = new Vector3(30, 1, 1);

                // 3D Collider 제거
                Object.DestroyImmediate(ground.GetComponent<BoxCollider>());

                // BoxCollider2D 추가
                var collider2D = ground.AddComponent<BoxCollider2D>();
                collider2D.size = new Vector2(1, 1);

                // Layer 설정
                int groundLayer = GetOrCreateLayer("Ground");
                ground.layer = groundLayer;

                // Material 색상 설정 (sharedMaterial 사용)
                var renderer = ground.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial.color = new Color(0.5f, 0.5f, 0.5f);
                }
            }

            // SpawnPoint 4개 생성
            CreateSpawnPoint(room.transform, "SpawnPoint_01", new Vector3(5, 0, 0));
            CreateSpawnPoint(room.transform, "SpawnPoint_02", new Vector3(8, 0, 0));
            CreateSpawnPoint(room.transform, "SpawnPoint_03", new Vector3(-5, 0, 0));
            CreateSpawnPoint(room.transform, "SpawnPoint_04", new Vector3(-8, 0, 0));

            // Portal 생성
            CreatePortal(room.transform, new Vector3(12, -1, 0));

            Debug.Log($"[Setup] {roomName} 생성 완료 (SpawnPoint x4, Portal)");
            return room;
        }

        private static void CreateSpawnPoint(Transform parent, string name, Vector3 localPosition)
        {
            GameObject spawnPoint = new GameObject(name);
            spawnPoint.transform.SetParent(parent);
            spawnPoint.transform.localPosition = localPosition;

            var component = spawnPoint.AddComponent<EnemySpawnPoint>();

            // TestGoblin 할당
            EnemyData testGoblin = AssetDatabase.LoadAssetAtPath<EnemyData>($"{ENEMY_DATA_FOLDER}/TestGoblin.asset");
            var enemyDataField = typeof(EnemySpawnPoint).GetField("enemyData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (enemyDataField != null && testGoblin != null)
            {
                enemyDataField.SetValue(component, testGoblin);
            }
        }

        private static void CreatePortal(Transform parent, Vector3 localPosition)
        {
            GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            portal.name = "Portal";
            portal.transform.SetParent(parent);
            portal.transform.localPosition = localPosition;
            portal.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            // Collider를 CircleCollider2D로 교체
            Object.DestroyImmediate(portal.GetComponent<SphereCollider>());
            var collider = portal.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 1f;

            // SpriteRenderer 색상 (Cyan) - sharedMaterial 사용
            var renderer = portal.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial.color = new Color(0, 1, 1, 0.5f); // Cyan, 반투명
            }

            // Portal 컴포넌트
            var portalComponent = portal.AddComponent<Portal>();

            // portalSprite 할당 (SpriteRenderer가 아니므로 null)
            // startActive = false 설정 (Reflection)
            var startActiveField = typeof(Portal).GetField("startActive",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (startActiveField != null)
            {
                startActiveField.SetValue(portalComponent, false);
            }
        }

        // ====== RoomManager 생성 ======

        private static void CreateRoomManager()
        {
            GameObject manager = new GameObject("RoomManager");
            manager.transform.position = Vector3.zero;

            var component = manager.AddComponent<RoomManager>();

            Debug.Log("[Setup] RoomManager 생성 완료 (Auto Find Rooms: true)");
        }

        // ====== 유틸리티 ======

        /// <summary>
        /// Layer를 가져오거나 생성
        /// </summary>
        private static int GetOrCreateLayer(string layerName)
        {
            // 기존 Layer 확인
            int layer = LayerMask.NameToLayer(layerName);
            if (layer != -1)
            {
                return layer;
            }

            // Layer가 없으면 생성 (TagManager 수정)
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            // 빈 Layer 슬롯 찾기 (8-31번 슬롯, 0-7은 Unity 내장)
            for (int i = 8; i < 32; i++)
            {
                SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layerProp.stringValue))
                {
                    layerProp.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"[Setup] Layer '{layerName}' 생성 완료 (슬롯 {i})");
                    return i;
                }
            }

            Debug.LogWarning($"[Setup] Layer '{layerName}' 생성 실패 - 빈 슬롯 없음");
            return 0; // Default Layer 반환
        }

        /// <summary>
        /// 기본 사각형 Sprite 생성 (1x1 픽셀)
        /// 플레이어 임시 비주얼용
        /// </summary>
        private static Sprite CreateDefaultSprite()
        {
            // 1x1 흰색 텍스처 생성
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            // Sprite 생성
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f), // pivot (중앙)
                1f // pixels per unit
            );

            return sprite;
        }
    }
}
