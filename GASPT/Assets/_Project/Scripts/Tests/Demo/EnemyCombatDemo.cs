using UnityEngine;
using Combat.Core;
using Player;
using Enemy;
using Enemy.Data;
using Core.Managers;

namespace Tests.Demo
{
    /// <summary>
    /// Enemy Combat 시스템 데모
    /// Enemy AI, 전투 시스템 통합 테스트
    /// </summary>
    public class EnemyCombatDemo : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private bool createPlayer = true;
        [SerializeField] private bool createEnemies = true;
        [SerializeField] private int enemyCount = 3;
        [SerializeField] private bool createGround = true;
        [SerializeField] private bool showUI = true;

        [Header("스폰 위치")]
        [SerializeField] private Vector3 playerSpawnPosition = new Vector3(0f, 1f, 0f);
        [SerializeField] private Vector3[] enemySpawnPositions = new Vector3[]
        {
            new Vector3(5f, 1f, 0f),
            new Vector3(-5f, 1f, 0f),
            new Vector3(10f, 1f, 0f)
        };

        private GameObject playerObject;
        private GameObject[] enemyObjects;
        private GameObject groundObject;
        private DamageSystem damageSystem;

        // UI
        private bool showGUI = true;
        private string eventLog = "";
        private int maxLogLines = 30;

        // 통계
        private int totalEnemiesKilled = 0;
        private float totalDamageDealt = 0f;
        private float totalDamageTaken = 0f;

        private void Start()
        {
            SetupDemo();
        }

        private void Update()
        {
            // F12: GUI 토글
            if (Input.GetKeyDown(KeyCode.F12))
            {
                showGUI = !showGUI;
            }

            // R: 씬 리셋
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetScene();
            }

            // Y: Enemy 재생성
            if (Input.GetKeyDown(KeyCode.Y))
            {
                RespawnEnemies();
            }
        }

        private void OnGUI()
        {
            if (!showUI || !showGUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 350, 500));
            GUILayout.BeginVertical("box");

            GUILayout.Label("=== Enemy Combat Demo ===", GUI.skin.GetStyle("label"));

            // 플레이어 정보
            if (playerObject != null)
            {
                var health = playerObject.GetComponent<HealthSystem>();
                if (health != null)
                {
                    GUILayout.Label($"Player HP: {health.CurrentHealth:F0}/{health.MaxHealth:F0}");
                }
            }

            GUILayout.Space(10);

            // Enemy 정보
            GUILayout.Label("=== Enemies ===");
            if (enemyObjects != null)
            {
                int aliveCount = 0;
                foreach (var enemy in enemyObjects)
                {
                    if (enemy != null) aliveCount++;
                }
                GUILayout.Label($"Alive: {aliveCount} / {enemyCount}");
            }

            GUILayout.Space(10);

            // 통계
            GUILayout.Label("=== 통계 ===");
            GUILayout.Label($"총 처치: {totalEnemiesKilled}");
            GUILayout.Label($"플레이어 공격 피해: {totalDamageDealt:F1}");
            GUILayout.Label($"플레이어 받은 피해: {totalDamageTaken:F1}");

            GUILayout.Space(10);

            // 버튼
            if (GUILayout.Button("씬 리셋 (R)"))
            {
                ResetScene();
            }
            if (GUILayout.Button("Enemy 재생성 (Y)"))
            {
                RespawnEnemies();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();

            // Event Log (우측 상단)
            GUILayout.BeginArea(new Rect(Screen.width - 360, 10, 350, 400));
            GUILayout.BeginVertical("box");
            GUILayout.Label("=== Event Log ===");
            GUILayout.TextArea(eventLog, GUILayout.Height(350));
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 데모 설정
        /// </summary>
        private void SetupDemo()
        {
            AddEventLog("[Demo] Enemy Combat Demo 시작");

            // DamageSystem 초기화
            damageSystem = DamageSystem.Instance;
            if (damageSystem == null)
            {
                var dsObject = new GameObject("DamageSystem");
                damageSystem = dsObject.AddComponent<DamageSystem>();
            }

            // Ground 생성
            if (createGround)
            {
                CreateGround();
            }

            // Player 생성
            if (createPlayer)
            {
                CreatePlayer();
            }

            // Enemy 생성
            if (createEnemies)
            {
                CreateEnemies();
            }

            AddEventLog("[Demo] 설정 완료");
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private void CreatePlayer()
        {
            playerObject = new GameObject("Player");
            playerObject.transform.position = playerSpawnPosition;
            playerObject.layer = LayerMask.NameToLayer("Player");

            // SpriteRenderer
            var sr = playerObject.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite(Color.cyan);

            // Rigidbody2D
            var rb = playerObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // BoxCollider2D
            var col = playerObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.5f);

            // PlayerController 추가
            var controller = playerObject.AddComponent<PlayerController>();

            // HealthSystem 설정
            var health = playerObject.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.SetMaxHealth(100f);
                health.SetCurrentHealth(100f);

                // 이벤트 구독
                health.OnDamaged += (data) =>
                {
                    totalDamageTaken += data.amount;
                    AddEventLog($"[Player] {data.amount:F1} 피해 받음");
                };
            }

            AddEventLog("[Player] 생성 완료");
        }

        private Sprite CreateSimpleSprite(Color color)
        {
            // 64x64 텍스처 생성
            int size = 64;
            Texture2D texture = new Texture2D(size, size);

            // 모든 픽셀을 지정된 색상으로 채움
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            // 텍스처를 Sprite로 변환
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), // pivot (중심점)
                100f // pixels per unit
            );

            return sprite;
        }

        /// <summary>
        /// Enemy 생성
        /// </summary>
        private void CreateEnemies()
        {
            enemyObjects = new GameObject[enemyCount];

            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 spawnPos = i < enemySpawnPositions.Length ?
                    enemySpawnPositions[i] :
                    new Vector3(Random.Range(-10f, 10f), 1f, 0f);

                enemyObjects[i] = CreateEnemy($"Enemy_{i}", spawnPos);
            }

            AddEventLog($"[Enemy] {enemyCount}개 생성 완료");
        }

        /// <summary>
        /// 개별 Enemy 생성
        /// </summary>
        private GameObject CreateEnemy(string name, Vector3 position)
        {
            var enemy = new GameObject(name);
            enemy.transform.position = position;
            enemy.layer = LayerMask.NameToLayer("Enemy");

            // SpriteRenderer
            var sr = enemy.AddComponent<SpriteRenderer>();
            sr.color = Color.red;
            sr.sprite = CreateSimpleSprite(Color.red);

            // Rigidbody2D
            var rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // BoxCollider2D
            var col = enemy.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.2f);

            // EnemyController
            var controller = enemy.AddComponent<EnemyController>();

            // EnemyData 생성 및 설정
            var data = GameResourceManager.GetResource<EnemyData>("Data/DefualtEnemyData");
            if (data == null)
            {
                ScriptableObject.CreateInstance<EnemyData>();

                data.enemyName = name;
                data.enemyType = EnemyType.Melee;
                data.maxHealth = 30f;
                data.moveSpeed = 2.5f;
                data.attackDamage = 10f;
                data.attackCooldown = 1.5f;
                data.detectionRange = 8f;
                data.chaseRange = 10f;
                data.attackRange = 1.5f;
                data.enablePatrol = true;
                data.patrolRange = 3f;
                data.patrolWaitTime = 1.5f;
                data.hitboxSize = new Vector2(1.5f, 1.0f);
                data.hitboxOffset = new Vector2(0.8f, 0f);
                data.hitboxDuration = 0.2f;
                data.knockbackForce = 5f;
                data.hitStunDuration = 0.3f;
                data.enemyColor = Color.red;
            }

            // Reflection으로 Data 설정
            var dataField = typeof(EnemyController).GetField("enemyData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dataField != null)
            {
                dataField.SetValue(controller, data);
            }

            // Target 설정 (플레이어)
            if (playerObject != null)
            {
                controller.Target = playerObject.transform;
            }

            // HealthSystem 이벤트 구독
            var health = controller.Health;
            if (health != null)
            {
                health.OnDamaged += (damageData) =>
                {
                    totalDamageDealt += damageData.amount;
                    AddEventLog($"[{name}] {damageData.amount:F1} 피해");
                };

                health.OnDeath += () =>
                {
                    totalEnemiesKilled++;
                    AddEventLog($"[{name}] 사망!");
                };
            }

            return enemy;
        }

        /// <summary>
        /// Ground 생성
        /// </summary>
        private void CreateGround()
        {
            groundObject = new GameObject("Ground");
            groundObject.transform.position = new Vector3(0f, -1f, 0f);
            groundObject.layer = LayerMask.NameToLayer("Ground");

            // SpriteRenderer
            var sr = groundObject.AddComponent<SpriteRenderer>();
            sr.color = Color.gray;
            sr.sprite = CreateSimpleSprite(Color.gray);

            groundObject.transform.localScale = new Vector3(30f, 1f, 1f);

            // BoxCollider2D
            var col = groundObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(30f, 1f);

            AddEventLog("[Ground] 생성 완료");
        }

        /// <summary>
        /// Enemy 재생성
        /// </summary>
        private void RespawnEnemies()
        {
            // 기존 Enemy 삭제
            if (enemyObjects != null)
            {
                foreach (var enemy in enemyObjects)
                {
                    if (enemy != null) Destroy(enemy);
                }
            }

            // 재생성
            CreateEnemies();
            AddEventLog("[Demo] Enemy 재생성 완료");
        }

        /// <summary>
        /// 씬 리셋
        /// </summary>
        private void ResetScene()
        {
            // 모든 오브젝트 삭제
            if (playerObject != null) Destroy(playerObject);
            if (enemyObjects != null)
            {
                foreach (var enemy in enemyObjects)
                {
                    if (enemy != null) Destroy(enemy);
                }
            }
            if (groundObject != null) Destroy(groundObject);

            // 통계 리셋
            totalEnemiesKilled = 0;
            totalDamageDealt = 0f;
            totalDamageTaken = 0f;
            eventLog = "";

            // 재설정
            SetupDemo();
            AddEventLog("[Demo] 씬 리셋 완료");
        }

        /// <summary>
        /// 이벤트 로그 추가
        /// </summary>
        private void AddEventLog(string message)
        {
            string timestamp = Time.time.ToString("F1");
            eventLog += $"[{timestamp}] {message}\n";

            // 최대 라인 수 제한
            string[] lines = eventLog.Split('\n');
            if (lines.Length > maxLogLines)
            {
                eventLog = string.Join("\n", lines, lines.Length - maxLogLines, maxLogLines);
            }
        }
    }
}
