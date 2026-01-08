using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.ResourceManagement;
using GASPT.Gameplay.Effects;
using GASPT.Gameplay.Projectiles;
using GASPT.Gameplay.Enemies;

namespace GASPT.Core.Pooling
{
    /// <summary>
    /// 모든 오브젝트 풀 초기화를 관리하는 통합 클래스
    ///
    /// 초기화 순서:
    /// 1. Effect 풀 (VisualEffect)
    /// 2. Projectile 풀 (FireballProjectile, MagicMissileProjectile, EnemyProjectile)
    /// 3. Enemy 풀 (BasicMeleeEnemy, RangedEnemy, FlyingEnemy, EliteEnemy)
    ///
    /// 작성일: 2025-11-16
    /// 목적: 분산된 Initializer 파일들을 통합하여 초기화 순서 명확화 및 중복 제거 (50-60줄 절감)
    /// </summary>
    public static class PoolInitializer
    {
        private static bool isInitialized = false;

        /// <summary>
        /// 게임 시작 시 모든 풀 초기화 (자동 호출)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeAllPools()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[PoolInitializer] 이미 초기화됨");
                return;
            }

            // 1. Effect 풀 초기화
            InitializeEffectPools();

            // 2. Projectile 풀 초기화
            InitializeProjectilePools();

            // 3. Enemy 풀 초기화
            InitializeEnemyPools();

            isInitialized = true;
        }

        #region Effect 풀 초기화

        /// <summary>
        /// Effect 관련 풀 초기화
        /// </summary>
        private static void InitializeEffectPools()
        {
            // VisualEffect 풀 생성
            InitializeVisualEffectPool();
        }

        /// <summary>
        /// VisualEffect 풀 초기화
        /// </summary>
        private static void InitializeVisualEffectPool()
        {
            // 런타임에서 VisualEffect 프리팹 생성
            GameObject effectPrefab = CreateVisualEffectPrefab();

            // 풀 생성 (초기 10개, 확장 가능)
            PoolManager.Instance.CreatePool(
                effectPrefab.GetComponent<VisualEffect>(),
                initialSize: 10,
                canGrow: true
            );
        }

        /// <summary>
        /// VisualEffect 프리팹 생성 (런타임)
        /// </summary>
        private static GameObject CreateVisualEffectPrefab()
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            prefab.name = "VisualEffect";
            prefab.transform.localScale = Vector3.one * 0.5f;

            // Collider 제거
            Object.Destroy(prefab.GetComponent<Collider>());

            // 머티리얼 설정
            var renderer = prefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = Color.white;
                renderer.material = mat;
            }

            // 컴포넌트 추가
            prefab.AddComponent<VisualEffect>();
            prefab.AddComponent<PooledObject>();

            // 비활성화
            prefab.SetActive(false);

            return prefab;
        }

        #endregion

        #region Projectile 풀 초기화

        /// <summary>
        /// Projectile 관련 풀 초기화
        /// </summary>
        private static void InitializeProjectilePools()
        {
            // FireballProjectile 풀 생성
            InitializeFireballPool();

            // MagicMissileProjectile 풀 생성
            InitializeMagicMissilePool();

            // IceLanceProjectile 풀 생성 (프리팹 생성 후 활성화)
            // InitializeIceLancePool();

            // EnemyProjectile 풀 생성
            InitializeEnemyProjectilePool();
        }

        /// <summary>
        /// FireballProjectile 풀 초기화
        /// </summary>
        private static void InitializeFireballPool()
        {
            GameObject fireballPrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Projectiles.FireballProjectile
            );

            if (fireballPrefab == null)
            {
                Debug.LogError("[PoolInitializer] FireballProjectile 프리팹을 찾을 수 없습니다!");
                return;
            }

            PoolManager.Instance.CreatePool(
                fireballPrefab.GetComponent<FireballProjectile>(),
                initialSize: 5,
                canGrow: true
            );
        }

        /// <summary>
        /// MagicMissileProjectile 풀 초기화
        /// </summary>
        private static void InitializeMagicMissilePool()
        {
            GameObject missilePrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Projectiles.MagicMissileProjectile
            );

            if (missilePrefab == null)
            {
                Debug.LogError("[PoolInitializer] MagicMissileProjectile 프리팹을 찾을 수 없습니다!");
                return;
            }

            PoolManager.Instance.CreatePool(
                missilePrefab.GetComponent<MagicMissileProjectile>(),
                initialSize: 10,
                canGrow: true
            );
        }

        /// <summary>
        /// IceLanceProjectile 풀 초기화
        /// </summary>
        private static void InitializeIceLancePool()
        {
            GameObject iceLancePrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Projectiles.IceLanceProjectile
            );

            if (iceLancePrefab == null)
            {
                Debug.LogWarning("[PoolInitializer] IceLanceProjectile 프리팹을 찾을 수 없습니다!");
                return;
            }

            PoolManager.Instance.CreatePool(
                iceLancePrefab.GetComponent<IceLanceProjectile>(),
                initialSize: 5,
                canGrow: true
            );
        }

        /// <summary>
        /// EnemyProjectile 풀 초기화
        /// </summary>
        private static void InitializeEnemyProjectilePool()
        {
            GameObject enemyProjectilePrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Projectiles.EnemyProjectile
            );

            if (enemyProjectilePrefab == null)
            {
                Debug.LogError("[PoolInitializer] EnemyProjectile 프리팹을 찾을 수 없습니다!");
                return;
            }

            PoolManager.Instance.CreatePool(
                enemyProjectilePrefab.GetComponent<EnemyProjectile>(),
                initialSize: 10,
                canGrow: true
            );
        }

        #endregion

        #region Enemy 풀 초기화

        /// <summary>
        /// Enemy 관련 풀 초기화
        /// </summary>
        private static void InitializeEnemyPools()
        {
            // BasicMeleeEnemy 풀 생성
            InitializeBasicMeleeEnemyPool();

            // RangedEnemy 풀 생성
            InitializeRangedEnemyPool();

            // FlyingEnemy 풀 생성
            InitializeFlyingEnemyPool();

            // EliteEnemy 풀 생성
            InitializeEliteEnemyPool();
        }

        /// <summary>
        /// BasicMeleeEnemy 풀 초기화
        /// </summary>
        private static void InitializeBasicMeleeEnemyPool()
        {
            GameObject enemyPrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Enemies.Basic
            );

            if (enemyPrefab == null)
            {
                Debug.LogError("[PoolInitializer] BasicMeleeEnemy 프리팹을 찾을 수 없습니다!");
                return;
            }

            PoolManager.Instance.CreatePool(
                enemyPrefab.GetComponent<BasicMeleeEnemy>(),
                initialSize: 5,
                canGrow: true
            );
        }

        /// <summary>
        /// RangedEnemy 풀 초기화
        /// </summary>
        private static void InitializeRangedEnemyPool()
        {
            GameObject enemyPrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Enemies.Ranged
            );

            if (enemyPrefab == null)
            {
                Debug.LogWarning("[PoolInitializer] RangedEnemy 프리팹을 찾을 수 없습니다!");
                return;
            }

            PoolManager.Instance.CreatePool(
                enemyPrefab.GetComponent<RangedEnemy>(),
                initialSize: 3,
                canGrow: true
            );
        }

        /// <summary>
        /// FlyingEnemy 풀 초기화
        /// </summary>
        private static void InitializeFlyingEnemyPool()
        {
            GameObject enemyPrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Enemies.Flying
            );

            if (enemyPrefab == null)
            {
                Debug.LogWarning("[PoolInitializer] FlyingEnemy 프리팹을 찾을 수 없습니다!");
                return;
            }

            PoolManager.Instance.CreatePool(
                enemyPrefab.GetComponent<FlyingEnemy>(),
                initialSize: 3,
                canGrow: true
            );
        }

        /// <summary>
        /// EliteEnemy 풀 초기화
        /// </summary>
        private static void InitializeEliteEnemyPool()
        {
            GameObject enemyPrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Enemies.Elite
            );

            if (enemyPrefab == null)
            {
                Debug.LogWarning("[PoolInitializer] EliteEnemy 프리팹을 찾을 수 없습니다!");
                return;
            }

            PoolManager.Instance.CreatePool(
                enemyPrefab.GetComponent<EliteEnemy>(),
                initialSize: 2,
                canGrow: true
            );
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 초기화 상태 리셋 (테스트용)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            isInitialized = false;
        }

        #endregion
    }
}
