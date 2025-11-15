using GASPT.Core.Pooling;
using GASPT.Data;
using GASPT.ResourceManagement;
using UnityEngine;

namespace GASPT.Gameplay.Enemy
{
    /// <summary>
    /// Enemy 풀 초기화
    /// 게임 시작 시 모든 Enemy 풀을 미리 생성
    /// </summary>
    public static class EnemyPoolInitializer
    {
        private static bool isInitialized = false;

        /// <summary>
        /// 모든 Enemy 풀 초기화
        /// </summary>
        public static void InitializeAllPools()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[EnemyPoolInitializer] 이미 초기화됨");
                return;
            }

            Debug.Log("[EnemyPoolInitializer] Enemy 풀 초기화 시작...");

            // BasicMeleeEnemy 풀 생성
            InitializeBasicMeleeEnemyPool();

            // RangedEnemy 풀 생성
            InitializeRangedEnemyPool();

            // FlyingEnemy 풀 생성
            InitializeFlyingEnemyPool();

            // EliteEnemy 풀 생성
            InitializeEliteEnemyPool();

            isInitialized = true;
            Debug.Log("[EnemyPoolInitializer] Enemy 풀 초기화 완료");
        }

        /// <summary>
        /// BasicMeleeEnemy 풀 초기화
        /// </summary>
        private static void InitializeBasicMeleeEnemyPool()
        {
            // Resources 폴더에서 기존 프리팹 로드
            GameObject enemyPrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Enemies.Basic
            );

            // 풀 생성 (초기 5개, 확장 가능)
            PoolManager.Instance.CreatePool(
                enemyPrefab.GetComponent<BasicMeleeEnemy>(),
                initialSize: 5,
                canGrow: true
            );

            Debug.Log("[EnemyPoolInitializer] BasicMeleeEnemy 풀 생성 완료");
        }

        /// <summary>
        /// RangedEnemy 풀 초기화
        /// </summary>
        private static void InitializeRangedEnemyPool()
        {
            // Resources 폴더에서 기존 프리팹 로드
            GameObject enemyPrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Enemies.Ranged
            );

            if (enemyPrefab == null)
            {
                Debug.LogWarning("[EnemyPoolInitializer] RangedEnemy 프리팹을 찾을 수 없습니다!");
                return;
            }

            // 풀 생성 (초기 3개, 확장 가능)
            PoolManager.Instance.CreatePool(
                enemyPrefab.GetComponent<RangedEnemy>(),
                initialSize: 3,
                canGrow: true
            );

            Debug.Log("[EnemyPoolInitializer] RangedEnemy 풀 생성 완료");
        }

        /// <summary>
        /// FlyingEnemy 풀 초기화
        /// </summary>
        private static void InitializeFlyingEnemyPool()
        {
            // Resources 폴더에서 기존 프리팹 로드
            GameObject enemyPrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Enemies.Flying
            );

            if (enemyPrefab == null)
            {
                Debug.LogWarning("[EnemyPoolInitializer] FlyingEnemy 프리팹을 찾을 수 없습니다!");
                return;
            }

            // 풀 생성 (초기 3개, 확장 가능)
            PoolManager.Instance.CreatePool(
                enemyPrefab.GetComponent<FlyingEnemy>(),
                initialSize: 3,
                canGrow: true
            );

            Debug.Log("[EnemyPoolInitializer] FlyingEnemy 풀 생성 완료");
        }

        /// <summary>
        /// EliteEnemy 풀 초기화
        /// </summary>
        private static void InitializeEliteEnemyPool()
        {
            // Resources 폴더에서 기존 프리팹 로드
            GameObject enemyPrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Enemies.Elite
            );

            if (enemyPrefab == null)
            {
                Debug.LogWarning("[EnemyPoolInitializer] EliteEnemy 프리팹을 찾을 수 없습니다!");
                return;
            }

            // 풀 생성 (초기 2개, 확장 가능)
            PoolManager.Instance.CreatePool(
                enemyPrefab.GetComponent<EliteEnemy>(),
                initialSize: 2,
                canGrow: true
            );

            Debug.Log("[EnemyPoolInitializer] EliteEnemy 풀 생성 완료");
        }


        // ====== 프리팹 생성 (런타임) ======

        /// <summary>
        /// BasicMeleeEnemy 프리팹 생성
        /// </summary>
        private static GameObject CreateBasicMeleeEnemyPrefab()
        {
            GameObject prefab = new GameObject("BasicMeleeEnemy");

            // Rigidbody2D 추가
            var rb = prefab.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // BoxCollider2D 추가
            var col = prefab.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);

            // SpriteRenderer 추가 (기본 빨간 사각형)
            var sr = prefab.AddComponent<SpriteRenderer>();
            sr.sprite = CreateDefaultSprite();
            sr.color = Color.red;

            // BasicMeleeEnemy 컴포넌트 추가
            prefab.AddComponent<BasicMeleeEnemy>();

            // PooledObject 컴포넌트 추가
            prefab.AddComponent<PooledObject>();

            // 비활성화 (풀에서 관리)
            prefab.SetActive(false);

            return prefab;
        }

        /// <summary>
        /// 기본 사각형 Sprite 생성 (1x1 픽셀)
        /// </summary>
        private static Sprite CreateDefaultSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                1f
            );

            return sprite;
        }

        /// <summary>
        /// 초기화 상태 리셋 (테스트용)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            isInitialized = false;
        }
    }
}
