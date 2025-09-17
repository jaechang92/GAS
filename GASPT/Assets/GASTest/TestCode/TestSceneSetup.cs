// ===================================
// 파일: Assets/Scripts/Ability/Test/TestSceneSetup.cs
// ===================================
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AbilitySystem.Platformer.Test
{
    /// <summary>
    /// 테스트 씬 자동 셋업 도구
    /// </summary>
    public class TestSceneSetup : MonoBehaviour
    {
        [Header("테스트 옵션")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool createTestLevel = true;
        [SerializeField] private bool createTestEnemies = true;
        [SerializeField] private bool createDebugUI = true;

        [Header("프리팹 참조")]
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
            Debug.Log("=== 테스트 씬 셋업 시작 ===");
            Debug.Log(this.gameObject.name);
            // 1. 레이어 생성 확인
            CheckAndCreateLayers();

            // 2. 테스트 레벨 생성
            if (createTestLevel)
                CreateTestLevel();

            // 3. 플레이어 생성
            CreatePlayer();

            // 4. 테스트 적 생성
            if (createTestEnemies)
                CreateTestEnemies();

            // 5. 디버그 UI 생성
            if (createDebugUI)
                CreateDebugUI();

            // 6. 카메라 설정
            SetupCamera();

            Debug.Log("=== 테스트 씬 셋업 완료 ===");
        }

        /// <summary>
        /// 레이어 확인 및 생성
        /// </summary>
        private void CheckAndCreateLayers()
        {
            // 필수 레이어 목록
            string[] requiredLayers = { "Player", "Enemy", "Ground", "Invulnerable" };

            foreach (string layerName in requiredLayers)
            {
                if (LayerMask.NameToLayer(layerName) == -1)
                {
                    Debug.LogWarning($"레이어 '{layerName}'이 없습니다. Project Settings > Tags and Layers에서 추가해주세요.");
                }
            }
        }

        /// <summary>
        /// 테스트 레벨 생성
        /// </summary>
        private void CreateTestLevel()
        {
            GameObject level = new GameObject("Test Level");

            //// 바닥 플랫폼
            CreatePlatform(new Vector3(0, -3, 0), new Vector3(20, 1, 1), "Ground");

            // 계단식 플랫폼
            CreatePlatform(new Vector3(-5, -1, 0), new Vector3(3, 0.5f, 1), "Platform 1");
            CreatePlatform(new Vector3(0, 0, 0), new Vector3(3, 0.5f, 1), "Platform 2");
            CreatePlatform(new Vector3(5, 1, 0), new Vector3(3, 0.5f, 1), "Platform 3");

            // 벽
            CreatePlatform(new Vector3(-10, 0, 0), new Vector3(1, 10, 1), "Left Wall");
            CreatePlatform(new Vector3(10, 0, 0), new Vector3(1, 10, 1), "Right Wall");
        }

        /// <summary>
        /// 플랫폼 생성
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

            // Collider 설정
            var collider = platform.GetComponent<Collider2D>();
            if (collider == null)
            {
                platform.AddComponent<BoxCollider2D>();
            }

            // 시각적 구분을 위한 색상
            var renderer = platform.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.gray;
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private void CreatePlayer()
        {
            GameObject player = playerPrefab != null ?
                Instantiate(playerPrefab) :
                new GameObject("Player");

            player.transform.position = new Vector3(0, 0, 0);

            // 필수 컴포넌트 확인 및 추가
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

            // Attack Point 생성
            GameObject attackPoint = new GameObject("Attack Point");
            attackPoint.transform.parent = player.transform;
            attackPoint.transform.localPosition = new Vector3(0.5f, 0, 0);

            player.layer = LayerMask.NameToLayer("Player");

            // 시각적 표시
            var sprite = player.GetComponent<SpriteRenderer>();
            if (sprite == null)
            {
                sprite = player.AddComponent<SpriteRenderer>();
                sprite.color = Color.blue;
            }
        }

        /// <summary>
        /// 테스트용 적 생성
        /// </summary>
        private void CreateTestEnemies()
        {
            // 더미 적 3개 생성
            CreateDummyEnemy(new Vector3(3, -2, 0), "Dummy Enemy 1");
            CreateDummyEnemy(new Vector3(-3, -2, 0), "Dummy Enemy 2");
            CreateDummyEnemy(new Vector3(0, 2, 0), "Dummy Enemy 3");
        }

        /// <summary>
        /// 더미 적 생성
        /// </summary>
        private void CreateDummyEnemy(Vector3 position, string name)
        {
            GameObject enemy = dummyEnemyPrefab != null ?
                Instantiate(dummyEnemyPrefab, position, Quaternion.identity) :
                new GameObject(name);

            enemy.transform.position = position;
            enemy.layer = LayerMask.NameToLayer("Enemy");

            // 컴포넌트 추가
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

            // 시각적 표시
            var sprite = enemy.GetComponent<SpriteRenderer>();
            if (sprite == null)
            {
                sprite = enemy.AddComponent<SpriteRenderer>();
                sprite.color = Color.red;
            }
        }

        /// <summary>
        /// 디버그 UI 생성
        /// </summary>
        private void CreateDebugUI()
        {
            // Canvas 생성
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
        /// 카메라 설정
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