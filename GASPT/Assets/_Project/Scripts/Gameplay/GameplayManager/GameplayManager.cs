using UnityEngine;
using Combat.Core;
using Player;
using Enemy;
using Enemy.Data;

namespace Gameplay
{
    /// <summary>
    /// Gameplay 씬 관리
    /// 플레이어, 적, 게임 콘텐츠 초기화
    /// </summary>
    public class GameplayManager : MonoBehaviour
    {
        [Header("생성 설정")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private bool createPlayer = false; // PlayerSpawner 사용으로 변경
        [SerializeField] private bool createEnemies = true;
        [SerializeField] private bool createGround = true;
        [SerializeField] private int enemyCount = 3;

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

        private void Start()
        {
            if (autoSetup)
            {
                Setup();
            }
        }

        /// <summary>
        /// Gameplay 씬 초기화
        /// </summary>
        public async void Setup()
        {
            Debug.Log("[GameplayManager] Gameplay 씬 초기화 시작");

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

            // Player 생성 (PlayerSpawner 사용)
            if (createPlayer)
            {
                CreatePlayer();
            }

            // 프레임 대기 (PlayerSpawner가 플레이어를 생성할 시간 확보)
            await Awaitable.NextFrameAsync();

            // Enemy 생성
            if (createEnemies)
            {
                CreateEnemies();
            }

            Debug.Log("[GameplayManager] Gameplay 씬 초기화 완료");
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
            }

            Debug.Log("[GameplayManager] 플레이어 생성 완료");
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

            Debug.Log($"[GameplayManager] Enemy {enemyCount}개 생성 완료");
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
            var data = ScriptableObject.CreateInstance<EnemyData>();
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

            // Reflection으로 Data 설정
            var dataField = typeof(EnemyController).GetField("enemyData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dataField != null)
            {
                dataField.SetValue(controller, data);
            }

            // Target 설정 (플레이어)
            // PlayerSpawner가 생성한 플레이어 찾기
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                controller.Target = player.transform;
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
            sr.sprite = CreateSimpleSprite(Color.gray);

            groundObject.transform.localScale = new Vector3(30f, 1f, 1f);

            // BoxCollider2D
            var col = groundObject.AddComponent<BoxCollider2D>();

            Debug.Log("[GameplayManager] Ground 생성 완료");
        }

        /// <summary>
        /// 간단한 Sprite 생성
        /// </summary>
        private Sprite CreateSimpleSprite(Color color)
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size);

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        /// <summary>
        /// 플레이어 참조 반환
        /// </summary>
        public GameObject GetPlayer()
        {
            return playerObject;
        }

        /// <summary>
        /// Enemy 배열 반환
        /// </summary>
        public GameObject[] GetEnemies()
        {
            return enemyObjects;
        }
    }
}
