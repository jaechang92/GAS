// ================================
// File: Assets/GASTest/SceneSetupHelper.cs
// �׽�Ʈ �� �ڵ� ���� ����
// ================================
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TestScene
{
    /// <summary>
    /// 2D �÷����� �׽�Ʈ �� �ڵ� ����
    /// </summary>
    public class SceneSetupHelper : MonoBehaviour
    {
        [Header("Setup Options")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool create2DPhysics = true;

        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject platformPrefab;
        [SerializeField] private GameObject uiCanvasPrefab;

        [Header("Platform Generation")]
        [SerializeField] private int platformCount = 10;
        [SerializeField] private float platformSpacing = 3f;
        [SerializeField] private Vector2 platformSizeRange = new Vector2(3f, 8f);
        [SerializeField] private float levelWidth = 50f;

        [Header("Enemy Spawning")]
        [SerializeField] private int enemyCount = 5;
        [SerializeField] private float enemySpawnHeight = 1f;

        // Runtime
        private GameObject playerInstance;
        private List<GameObject> platforms = new List<GameObject>();
        private List<GameObject> enemies = new List<GameObject>();
        private Camera mainCamera;

        #region Unity Lifecycle

        private async void Start()
        {
            if (autoSetupOnStart)
            {
                await SetupScene();
            }
        }

        #endregion

        #region Scene Setup

        public async Task SetupScene()
        {
            Debug.Log("[SceneSetupHelper] Starting scene setup...");

            // 1. Physics ����
            if (create2DPhysics)
            {
                SetupPhysics2D();
            }

            // 2. ī�޶� ����
            SetupCamera();

            // 3. �÷��� ����
            await CreatePlatforms();

            // 4. �÷��̾� ����
            SpawnPlayer();

            // 5. �� ����
            await SpawnEnemies();

            // 6. UI ����
            SetupUI();

            Debug.Log("[SceneSetupHelper] Scene setup complete!");
        }

        private void SetupPhysics2D()
        {
            // Layer ����
            int groundLayer = 10;
            int enemyLayer = 9;
            int playerLayer = 8;

            // Layer �̸� ���� (�����Ϳ�����)
#if UNITY_EDITOR
            SetLayerName(groundLayer, "Ground");
            SetLayerName(enemyLayer, "Enemy");
            SetLayerName(playerLayer, "Player");
#endif

            // Physics2D ����
            Physics2D.gravity = new Vector2(0, -20f);

            // Layer �浹 ����
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer); // �������� �浹 ����

            Debug.Log("[SceneSetupHelper] Physics2D setup complete");
        }

        private void SetupCamera()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCamera = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                camObj.tag = "MainCamera";
            }

            // 2D ī�޶� ����
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5f;
            mainCamera.transform.position = new Vector3(0, 0, -10);

            Debug.Log("[SceneSetupHelper] Camera setup complete");
        }

        #endregion

        #region Platform Generation

        private async Task CreatePlatforms()
        {
            ClearPlatforms();

            // ���� �÷��� (�ٴ�)
            GameObject mainPlatform = CreatePlatform(
                new Vector3(0, -3, 0),
                new Vector3(levelWidth, 1, 1)
            );
            mainPlatform.name = "Main Platform";
            platforms.Add(mainPlatform);

            // �߰� �÷�����
            for (int i = 0; i < platformCount; i++)
            {
                float xPos = Random.Range(-levelWidth / 2 + 5, levelWidth / 2 - 5);
                float yPos = Random.Range(-1f, 5f);
                float width = Random.Range(platformSizeRange.x, platformSizeRange.y);

                GameObject platform = CreatePlatform(
                    new Vector3(xPos, yPos, 0),
                    new Vector3(width, 0.5f, 1)
                );
                platform.name = $"Platform_{i}";
                platforms.Add(platform);

                await Task.Delay(50); // �ð��� ȿ��
            }

            Debug.Log($"[SceneSetupHelper] Created {platforms.Count} platforms");
        }

        private GameObject CreatePlatform(Vector3 position, Vector3 size)
        {
            GameObject platform;

            if (platformPrefab != null)
            {
                platform = Instantiate(platformPrefab, position, Quaternion.identity);
                platform.transform.localScale = size;
            }
            else
            {
                // �������� ������ �⺻ ť��� ����
                platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                platform.transform.position = position;
                platform.transform.localScale = size;

                // 2D ����
                Destroy(platform.GetComponent<BoxCollider>());
                platform.AddComponent<BoxCollider2D>();

                // Material ����
                Renderer renderer = platform.GetComponent<Renderer>();
                renderer.material.color = new Color(0.5f, 0.3f, 0.1f); // ����
            }

            // Layer ����
            platform.layer = 10; // Ground Layer

            // Static ���� (����ȭ)
            platform.isStatic = true;

            return platform;
        }

        private void ClearPlatforms()
        {
            foreach (var platform in platforms)
            {
                if (platform != null)
                {
                    DestroyImmediate(platform);
                }
            }
            platforms.Clear();
        }

        #endregion

        #region Player Spawning

        private void SpawnPlayer()
        {
            if (playerInstance != null)
            {
                DestroyImmediate(playerInstance);
            }

            Vector3 spawnPosition = new Vector3(0, 0, 0);

            if (playerPrefab != null)
            {
                playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                playerInstance.name = "Player";
            }
            else
            {
                // �������� ������ �⺻ �÷��̾� ����
                playerInstance = CreateDefaultPlayer(spawnPosition);
            }

            // ī�޶� �ȷο� ����
            SetupCameraFollow(playerInstance);

            Debug.Log("[SceneSetupHelper] Player spawned");
        }

        private GameObject CreateDefaultPlayer(Vector3 position)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = position;
            player.transform.localScale = new Vector3(1, 1, 1);

            // 2D ������Ʈ ����
            Destroy(player.GetComponent<CapsuleCollider>());
            player.AddComponent<CapsuleCollider2D>();

            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.freezeRotation = true;

            // ��Ʈ�ѷ� �߰�
            player.AddComponent<Platform2DController>();

            // Layer ����
            player.layer = 8; // Player Layer

            // ���� ����
            player.GetComponent<Renderer>().material.color = Color.blue;

            return player;
        }

        #endregion

        #region Enemy Spawning

        private async Task SpawnEnemies()
        {
            ClearEnemies();

            for (int i = 0; i < enemyCount; i++)
            {
                // �÷��� ���� �����ϰ� ��ġ
                if (platforms.Count > 1)
                {
                    GameObject platform = platforms[Random.Range(1, platforms.Count)];
                    Vector3 spawnPos = platform.transform.position + Vector3.up * enemySpawnHeight;

                    GameObject enemy = SpawnEnemy(spawnPos);
                    enemy.name = $"Enemy_{i}";
                    enemies.Add(enemy);

                    await Task.Delay(100); // �ð��� ȿ��
                }
            }

            Debug.Log($"[SceneSetupHelper] Spawned {enemies.Count} enemies");
        }

        private GameObject SpawnEnemy(Vector3 position)
        {
            GameObject enemy;

            if (enemyPrefab != null)
            {
                enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            }
            else
            {
                // �������� ������ �⺻ �� ����
                enemy = CreateDefaultEnemy(position);
            }

            return enemy;
        }

        private GameObject CreateDefaultEnemy(Vector3 position)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.transform.position = position;
            enemy.transform.localScale = new Vector3(0.8f, 0.8f, 1);

            // 2D ������Ʈ ����
            Destroy(enemy.GetComponent<BoxCollider>());
            enemy.AddComponent<BoxCollider2D>();

            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.freezeRotation = true;

            // AI ��Ʈ�ѷ� �߰�
            enemy.AddComponent<Enemy2DController>();

            // Layer ����
            enemy.layer = 9; // Enemy Layer

            // ���� ����
            enemy.GetComponent<Renderer>().material.color = Color.red;

            return enemy;
        }

        private void ClearEnemies()
        {
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    DestroyImmediate(enemy);
                }
            }
            enemies.Clear();
        }

        #endregion

        #region Camera Follow

        private void SetupCameraFollow(GameObject target)
        {
            if (mainCamera == null || target == null) return;

            Camera2DFollow followScript = mainCamera.GetComponent<Camera2DFollow>();
            if (followScript == null)
            {
                followScript = mainCamera.gameObject.AddComponent<Camera2DFollow>();
            }

            followScript.SetTarget(target.transform);

            Debug.Log("[SceneSetupHelper] Camera follow setup complete");
        }

        #endregion

        #region UI Setup

        private void SetupUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();

            if (canvas == null)
            {
                if (uiCanvasPrefab != null)
                {
                    GameObject canvasObj = Instantiate(uiCanvasPrefab);
                    canvas = canvasObj.GetComponent<Canvas>();
                }
                else
                {
                    // �⺻ ĵ���� ����
                    canvas = CreateDefaultCanvas();
                }
            }

            Debug.Log("[SceneSetupHelper] UI setup complete");
        }

        private Canvas CreateDefaultCanvas()
        {
            GameObject canvasObj = new GameObject("UI Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem ����
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            return canvas;
        }

        #endregion

        #region Utility

#if UNITY_EDITOR
        private void SetLayerName(int layer, string name)
        {
            UnityEditor.SerializedObject tagManager =
                new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            UnityEditor.SerializedProperty layers = tagManager.FindProperty("layers");

            if (layers != null && layer >= 0 && layer < 32)
            {
                UnityEditor.SerializedProperty layerSP = layers.GetArrayElementAtIndex(layer);
                if (layerSP != null)
                {
                    layerSP.stringValue = name;
                    tagManager.ApplyModifiedProperties();
                }
            }
        }
#endif

        public void ResetScene()
        {
            ClearPlatforms();
            ClearEnemies();

            if (playerInstance != null)
            {
                DestroyImmediate(playerInstance);
            }

            Debug.Log("[SceneSetupHelper] Scene reset complete");
        }

        #endregion
    }
}