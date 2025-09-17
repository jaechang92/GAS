// ===================================
// ����: Assets/Scripts/Ability/Test/TestSceneSetup.cs
// ===================================
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AbilitySystem.Platformer.Test
{
    /// <summary>
    /// �׽�Ʈ �� �ڵ� �¾� ����
    /// </summary>
    public class TestSceneSetup : MonoBehaviour
    {
        [Header("�׽�Ʈ �ɼ�")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool createTestLevel = true;
        [SerializeField] private bool createTestEnemies = true;
        [SerializeField] private bool createDebugUI = true;

        [Header("������ ����")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject dummyEnemyPrefab;
        [SerializeField] private GameObject platformPrefab;

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupTestScene();
            }
        }

        [ContextMenu("Setup Test Scene")]
        public void SetupTestScene()
        {
            Debug.Log("=== �׽�Ʈ �� �¾� ���� ===");
            Debug.Log(this.gameObject.name);
            // 1. ���̾� ���� Ȯ��
            CheckAndCreateLayers();

            // 2. �׽�Ʈ ���� ����
            if (createTestLevel)
                CreateTestLevel();

            // 3. �÷��̾� ����
            CreatePlayer();

            // 4. �׽�Ʈ �� ����
            if (createTestEnemies)
                CreateTestEnemies();

            // 5. ����� UI ����
            if (createDebugUI)
                CreateDebugUI();

            // 6. ī�޶� ����
            SetupCamera();

            Debug.Log("=== �׽�Ʈ �� �¾� �Ϸ� ===");
        }

        /// <summary>
        /// ���̾� Ȯ�� �� ����
        /// </summary>
        private void CheckAndCreateLayers()
        {
            // �ʼ� ���̾� ���
            string[] requiredLayers = { "Player", "Enemy", "Ground", "Invulnerable" };

            foreach (string layerName in requiredLayers)
            {
                if (LayerMask.NameToLayer(layerName) == -1)
                {
                    Debug.LogWarning($"���̾� '{layerName}'�� �����ϴ�. Project Settings > Tags and Layers���� �߰����ּ���.");
                }
            }
        }

        /// <summary>
        /// �׽�Ʈ ���� ����
        /// </summary>
        private void CreateTestLevel()
        {
            GameObject level = new GameObject("Test Level");

            //// �ٴ� �÷���
            CreatePlatform(new Vector3(0, -3, 0), new Vector3(20, 1, 1), "Ground");

            // ��ܽ� �÷���
            CreatePlatform(new Vector3(-5, -1, 0), new Vector3(3, 0.5f, 1), "Platform 1");
            CreatePlatform(new Vector3(0, 0, 0), new Vector3(3, 0.5f, 1), "Platform 2");
            CreatePlatform(new Vector3(5, 1, 0), new Vector3(3, 0.5f, 1), "Platform 3");

            // ��
            CreatePlatform(new Vector3(-10, 0, 0), new Vector3(1, 10, 1), "Left Wall");
            CreatePlatform(new Vector3(10, 0, 0), new Vector3(1, 10, 1), "Right Wall");
        }

        /// <summary>
        /// �÷��� ����
        /// </summary>
        private void CreatePlatform(Vector3 position, Vector3 scale, string name)
        {
            GameObject platform = platformPrefab != null ?
                Instantiate(platformPrefab, position, Quaternion.identity) :
                GameObject.CreatePrimitive(PrimitiveType.Cube);

            platform.name = name;
            platform.transform.position = position;
            platform.transform.localScale = scale;
            platform.layer = LayerMask.NameToLayer("Ground");

            // Collider ����
            var collider = platform.GetComponent<Collider2D>();
            if (collider == null)
            {
                platform.AddComponent<BoxCollider2D>();
            }

            // �ð��� ������ ���� ����
            var renderer = platform.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.gray;
            }
        }

        /// <summary>
        /// �÷��̾� ����
        /// </summary>
        private void CreatePlayer()
        {
            GameObject player = playerPrefab != null ?
                Instantiate(playerPrefab) :
                new GameObject("Player");

            player.transform.position = new Vector3(0, 0, 0);

            // �ʼ� ������Ʈ Ȯ�� �� �߰�
            if (!player.GetComponent<Rigidbody2D>())
            {
                var rb = player.AddComponent<Rigidbody2D>();
                rb.gravityScale = 2f;
                rb.freezeRotation = true;
            }

            if (!player.GetComponent<BoxCollider2D>())
            {
                var col = player.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 2f);
            }

            if (!player.GetComponent<PlatformerAbilityController>())
            {
                player.AddComponent<PlatformerAbilityController>();
            }

            // Attack Point ����
            GameObject attackPoint = new GameObject("Attack Point");
            attackPoint.transform.parent = player.transform;
            attackPoint.transform.localPosition = new Vector3(0.5f, 0, 0);

            player.layer = LayerMask.NameToLayer("Player");

            // �ð��� ǥ��
            var sprite = player.GetComponent<SpriteRenderer>();
            if (sprite == null)
            {
                sprite = player.AddComponent<SpriteRenderer>();
                sprite.color = Color.blue;
            }
        }

        /// <summary>
        /// �׽�Ʈ�� �� ����
        /// </summary>
        private void CreateTestEnemies()
        {
            // ���� �� 3�� ����
            CreateDummyEnemy(new Vector3(3, -2, 0), "Dummy Enemy 1");
            CreateDummyEnemy(new Vector3(-3, -2, 0), "Dummy Enemy 2");
            CreateDummyEnemy(new Vector3(0, 2, 0), "Dummy Enemy 3");
        }

        /// <summary>
        /// ���� �� ����
        /// </summary>
        private void CreateDummyEnemy(Vector3 position, string name)
        {
            GameObject enemy = dummyEnemyPrefab != null ?
                Instantiate(dummyEnemyPrefab, position, Quaternion.identity) :
                new GameObject(name);

            enemy.transform.position = position;
            enemy.layer = LayerMask.NameToLayer("Enemy");

            // ������Ʈ �߰�
            if (!enemy.GetComponent<DummyEnemy>())
            {
                enemy.AddComponent<DummyEnemy>();
            }

            if (!enemy.GetComponent<BoxCollider2D>())
            {
                var col = enemy.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 1.5f);
            }

            if (!enemy.GetComponent<Rigidbody2D>())
            {
                var rb = enemy.AddComponent<Rigidbody2D>();
                rb.gravityScale = 2f;
            }

            // �ð��� ǥ��
            var sprite = enemy.GetComponent<SpriteRenderer>();
            if (sprite == null)
            {
                sprite = enemy.AddComponent<SpriteRenderer>();
                sprite.color = Color.red;
            }
        }

        /// <summary>
        /// ����� UI ����
        /// </summary>
        private void CreateDebugUI()
        {
            // Canvas ����
            GameObject canvas = GameObject.Find("Debug Canvas");
            if (canvas == null)
            {
                canvas = new GameObject("Debug Canvas");
                var canvasComp = canvas.AddComponent<Canvas>();
                canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.AddComponent<CanvasScaler>();
                canvas.AddComponent<GraphicRaycaster>();
            }

            // Debug Info Panel
            GameObject panel = new GameObject("Debug Panel");
            panel.transform.SetParent(canvas.transform);
            var rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(10, -10);
            rectTransform.sizeDelta = new Vector2(300, 200);

            var image = panel.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.5f);

            // Debug Text
            GameObject debugText = new GameObject("Debug Text");
            debugText.transform.SetParent(panel.transform);
            var textRect = debugText.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            var text = debugText.AddComponent<TextMeshProUGUI>();
            text.text = "Debug Info\n- Press Keys to Test\n- Q: Swap Skul\n- Z: Attack\n- X/A: Skills";
            text.color = Color.white;
            text.fontSize = 14;
        }

        /// <summary>
        /// ī�޶� ����
        /// </summary>
        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.backgroundColor = new Color(0.2f, 0.3f, 0.4f);
        }
    }
}