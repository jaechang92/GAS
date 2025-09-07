// ================================
// File: Assets/Scripts/TestScene/SceneSetupHelper.cs
// 2D 플랫포머 테스트 씬 자동 구성 도구
// ================================
using UnityEngine;
using UnityEngine.UI;
using GAS.Core;
using GAS.AttributeSystem;
using GAS.TagSystem;
using GAS.EffectSystem;
using GAS.AbilitySystem;
using System.Collections.Generic;

namespace TestScene
{
    /// <summary>
    /// 2D 플랫포머 테스트 씬을 자동으로 구성하는 헬퍼 클래스
    /// </summary>
    public class SceneSetupHelper : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool create2DPhysics = true;

        [Header("Prefab References")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject platformPrefab;
        [SerializeField] private GameObject uiCanvasPrefab;

        [Header("Level Design")]
        [SerializeField] private int platformCount = 10;
        [SerializeField] private float platformSpacing = 3f;
        [SerializeField] private Vector2 platformSizeRange = new Vector2(3f, 8f);
        [SerializeField] private float levelWidth = 50f;

        [Header("Enemy Spawning")]
        [SerializeField] private int enemyCount = 5;
        [SerializeField] private float enemySpawnHeight = 1f;

        private GameObject player;
        private List<GameObject> enemies = new List<GameObject>();
        private List<GameObject> platforms = new List<GameObject>();
        private GameObject uiCanvas;

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupScene();
            }
        }

        /// <summary>
        /// 테스트 씬 전체 설정
        /// </summary>
        public void SetupScene()
        {
            Debug.Log("[SceneSetupHelper] Starting 2D platformer scene setup...");

            Setup2DPhysics();
            SetupCamera();
            CreatePlatforms();
            SpawnPlayer();
            SpawnEnemies();
            CreateUI();
            SetupLighting();

            Debug.Log("[SceneSetupHelper] Scene setup complete!");
        }

        /// <summary>
        /// 2D 물리 설정
        /// </summary>
        private void Setup2DPhysics()
        {
            if (!create2DPhysics) return;

            // 2D 중력 설정
            Physics2D.gravity = new Vector2(0, -20f);

            // 충돌 레이어 설정
            SetupCollisionLayers();

            Debug.Log("[SceneSetupHelper] 2D physics configured");
        }

        /// <summary>
        /// 충돌 레이어 설정
        /// </summary>
        private void SetupCollisionLayers()
        {
            // Layer 설정 (에디터에서 미리 설정되어 있어야 함)
            // 8: Player
            // 9: Enemy
            // 10: Platform
            // 11: Projectile

            // Player는 Platform과 Enemy와 충돌
            Physics2D.IgnoreLayerCollision(8, 11, false); // Player-Projectile

            // Enemy는 Platform과 충돌, 서로는 통과
            Physics2D.IgnoreLayerCollision(9, 9, true); // Enemy-Enemy

            // Projectile은 Platform과 충돌
            Physics2D.IgnoreLayerCollision(11, 10, false); // Projectile-Platform
        }

        /// <summary>
        /// 카메라 설정 (2D 용)
        /// </summary>
        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.tag = "MainCamera";
            }

            // 2D 카메라 설정
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 10f;
            mainCamera.transform.position = new Vector3(0, 5, -10);
            mainCamera.backgroundColor = new Color(0.2f, 0.3f, 0.5f); // 하늘색 배경

            // 카메라 팔로우 컴포넌트 추가 (나중에 구현)
            if (mainCamera.GetComponent<Camera2DFollow>() == null)
            {
                mainCamera.gameObject.AddComponent<Camera2DFollow>();
            }

            Debug.Log("[SceneSetupHelper] Camera configured for 2D");
        }

        /// <summary>
        /// 플랫폼 생성
        /// </summary>
        private void CreatePlatforms()
        {
            // 바닥 플랫폼 생성
            CreateGroundPlatform();

            // 공중 플랫폼들 생성
            for (int i = 0; i < platformCount; i++)
            {
                CreatePlatform(i);
            }

            Debug.Log($"[SceneSetupHelper] Created {platformCount + 1} platforms");
        }

        /// <summary>
        /// 바닥 플랫폼 생성
        /// </summary>
        private void CreateGroundPlatform()
        {
            GameObject ground = CreatePlatformObject("Ground", new Vector3(0, -5, 0));
            ground.transform.localScale = new Vector3(levelWidth * 2, 1, 1);
            platforms.Add(ground);
        }

        /// <summary>
        /// 개별 플랫폼 생성
        /// </summary>
        private void CreatePlatform(int index)
        {
            float xPos = -levelWidth / 2 + (levelWidth / platformCount) * index + Random.Range(-1f, 1f);
            float yPos = Random.Range(-2f, 8f);
            float width = Random.Range(platformSizeRange.x, platformSizeRange.y);

            Vector3 position = new Vector3(xPos, yPos, 0);
            GameObject platform = CreatePlatformObject($"Platform_{index}", position);
            platform.transform.localScale = new Vector3(width, 0.5f, 1);

            platforms.Add(platform);
        }

        /// <summary>
        /// 플랫폼 오브젝트 생성
        /// </summary>
        private GameObject CreatePlatformObject(string name, Vector3 position)
        {
            GameObject platform;

            if (platformPrefab != null)
            {
                platform = Instantiate(platformPrefab, position, Quaternion.identity);
                platform.name = name;
            }
            else
            {
                // 프리팹이 없으면 기본 큐브로 생성
                platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                platform.name = name;
                platform.transform.position = position;

                // 2D 설정
                Destroy(platform.GetComponent<BoxCollider>());
                platform.AddComponent<BoxCollider2D>();

                // 머티리얼 설정
                Renderer renderer = platform.GetComponent<Renderer>();
                renderer.material.color = new Color(0.5f, 0.3f, 0.1f); // 갈색
            }

            platform.layer = 10; // Platform layer
            platform.tag = "Platform";

            return platform;
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private void SpawnPlayer()
        {
            Vector3 spawnPosition = new Vector3(-levelWidth / 3, 0, 0);

            if (playerPrefab != null)
            {
                player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                player.name = "Player";
            }
            else
            {
                player = CreateDefaultPlayer(spawnPosition);
            }

            // 카메라가 플레이어를 따라가도록 설정
            Camera2DFollow cameraFollow = Camera.main.GetComponent<Camera2DFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(player.transform);
            }

            Debug.Log("[SceneSetupHelper] Player spawned");
        }

        /// <summary>
        /// 기본 플레이어 생성 (프리팹이 없을 때)
        /// </summary>
        private GameObject CreateDefaultPlayer(Vector3 position)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = position;
            player.transform.localScale = new Vector3(1, 2, 1);

            // 2D 컴포넌트 설정
            Destroy(player.GetComponent<CapsuleCollider>());
            CapsuleCollider2D collider2D = player.AddComponent<CapsuleCollider2D>();
            collider2D.size = new Vector2(1, 2);

            Rigidbody2D rb2d = player.AddComponent<Rigidbody2D>();
            rb2d.gravityScale = 2f;
            rb2d.freezeRotation = true;

            // GAS 컴포넌트 추가
            player.AddComponent<AttributeSetComponent>();
            player.AddComponent<TagComponent>();
            player.AddComponent<EffectComponent>();
            player.AddComponent<AbilitySystemComponent>();

            // 플레이어 컨트롤러 추가
            player.AddComponent<Platform2DController>();

            // 시각적 구분
            player.GetComponent<Renderer>().material.color = Color.blue;
            player.layer = 8; // Player layer
            player.tag = "Player";

            return player;
        }

        /// <summary>
        /// 적 생성
        /// </summary>
        private void SpawnEnemies()
        {
            for (int i = 0; i < enemyCount; i++)
            {
                float xPos = Random.Range(-levelWidth / 2, levelWidth / 2);
                float yPos = enemySpawnHeight;
                Vector3 spawnPosition = new Vector3(xPos, yPos, 0);

                GameObject enemy;
                if (enemyPrefab != null)
                {
                    enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                }
                else
                {
                    enemy = CreateDefaultEnemy(spawnPosition);
                }

                enemy.name = $"Enemy_{i}";
                enemies.Add(enemy);
            }

            Debug.Log($"[SceneSetupHelper] Spawned {enemyCount} enemies");
        }

        /// <summary>
        /// 기본 적 생성 (프리팹이 없을 때)
        /// </summary>
        private GameObject CreateDefaultEnemy(Vector3 position)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.transform.position = position;
            enemy.transform.localScale = new Vector3(1.5f, 1.5f, 1);

            // 2D 컴포넌트 설정
            Destroy(enemy.GetComponent<BoxCollider>());
            BoxCollider2D collider2D = enemy.AddComponent<BoxCollider2D>();

            Rigidbody2D rb2d = enemy.AddComponent<Rigidbody2D>();
            rb2d.gravityScale = 2f;
            rb2d.freezeRotation = true;

            // GAS 컴포넌트 추가
            AttributeSetComponent attributes = enemy.AddComponent<AttributeSetComponent>();
            TagComponent tags = enemy.AddComponent<TagComponent>();
            enemy.AddComponent<EffectComponent>();

            // Enemy2D AI 추가
            enemy.AddComponent<Enemy2DController>();

            // 시각적 구분
            enemy.GetComponent<Renderer>().material.color = Color.red;
            enemy.layer = 9; // Enemy layer
            enemy.tag = "Enemy";

            return enemy;
        }

        /// <summary>
        /// UI 생성
        /// </summary>
        private void CreateUI()
        {
            if (uiCanvasPrefab != null)
            {
                uiCanvas = Instantiate(uiCanvasPrefab);
                uiCanvas.name = "GameplayUI";
            }
            else
            {
                CreateDefaultUI();
            }

            Debug.Log("[SceneSetupHelper] UI created");
        }

        /// <summary>
        /// 기본 UI 생성
        /// </summary>
        private void CreateDefaultUI()
        {
            // Canvas 생성
            uiCanvas = new GameObject("GameplayUI");
            Canvas canvas = uiCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.AddComponent<CanvasScaler>();
            uiCanvas.AddComponent<GraphicRaycaster>();

            // EventSystem 생성
            if (GameObject.Find("EventSystem") == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        /// <summary>
        /// 조명 설정
        /// </summary>
        private void SetupLighting()
        {
            // 2D 게임용 글로벌 조명
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;

            Debug.Log("[SceneSetupHelper] Lighting configured for 2D");
        }

        /// <summary>
        /// 씬 초기화
        /// </summary>
        public void ClearScene()
        {
            // 플레이어 제거
            if (player != null)
                DestroyImmediate(player);

            // 적들 제거
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                    DestroyImmediate(enemy);
            }
            enemies.Clear();

            // 플랫폼 제거
            foreach (var platform in platforms)
            {
                if (platform != null)
                    DestroyImmediate(platform);
            }
            platforms.Clear();

            // UI 제거
            if (uiCanvas != null)
                DestroyImmediate(uiCanvas);

            Debug.Log("[SceneSetupHelper] Scene cleared");
        }
    }
}