using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.ResourceManagement;

namespace GASPT.Gameplay.Projectiles
{
    /// <summary>
    /// 투사체 풀 초기화
    /// 게임 시작 시 모든 투사체 풀을 미리 생성
    /// </summary>
    public static class ProjectilePoolInitializer
    {
        private static bool isInitialized = false;

        /// <summary>
        /// 모든 투사체 풀 초기화
        /// </summary>
        public static void InitializeAllPools()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[ProjectilePoolInitializer] 이미 초기화됨");
                return;
            }

            Debug.Log("[ProjectilePoolInitializer] 투사체 풀 초기화 시작...");

            // FireballProjectile 풀 생성
            InitializeFireballPool();

            // MagicMissileProjectile 풀 생성
            InitializeMagicMissilePool();

            // EnemyProjectile 풀 생성
            InitializeEnemyProjectilePool();

            isInitialized = true;
            Debug.Log("[ProjectilePoolInitializer] 투사체 풀 초기화 완료");
        }

        /// <summary>
        /// FireballProjectile 풀 초기화
        /// </summary>
        private static void InitializeFireballPool()
        {
            // Resources 폴더에서 기존 프리팹 로드
            GameObject fireballPrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Projectiles.FireballProjectile
            );

            if (fireballPrefab == null)
            {
                Debug.LogError("[ProjectilePoolInitializer] FireballProjectile 프리팹을 찾을 수 없습니다!");
                return;
            }

            // 풀 생성 (초기 5개, 확장 가능)
            PoolManager.Instance.CreatePool(
                fireballPrefab.GetComponent<FireballProjectile>(),
                initialSize: 5,
                canGrow: true
            );

            Debug.Log("[ProjectilePoolInitializer] FireballProjectile 풀 생성 완료");
        }

        /// <summary>
        /// MagicMissileProjectile 풀 초기화
        /// </summary>
        private static void InitializeMagicMissilePool()
        {
            // Resources 폴더에서 기존 프리팹 로드
            GameObject missilePrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Projectiles.MagicMissileProjectile
            );

            if (missilePrefab == null)
            {
                Debug.LogError("[ProjectilePoolInitializer] MagicMissileProjectile 프리팹을 찾을 수 없습니다!");
                return;
            }

            // 풀 생성 (초기 10개, 확장 가능)
            PoolManager.Instance.CreatePool(
                missilePrefab.GetComponent<MagicMissileProjectile>(),
                initialSize: 10,
                canGrow: true
            );

            Debug.Log("[ProjectilePoolInitializer] MagicMissileProjectile 풀 생성 완료");
        }

        /// <summary>
        /// EnemyProjectile 풀 초기화
        /// </summary>
        private static void InitializeEnemyProjectilePool()
        {
            // Resources 폴더에서 기존 프리팹 로드
            GameObject enemyProjectilePrefab = GameResourceManager.Instance.LoadPrefab(
                ResourcePaths.Prefabs.Projectiles.EnemyProjectile
            );

            if (enemyProjectilePrefab == null)
            {
                Debug.LogError("[ProjectilePoolInitializer] EnemyProjectile 프리팹을 찾을 수 없습니다!");
                return;
            }

            // 풀 생성 (초기 10개, 확장 가능)
            PoolManager.Instance.CreatePool(
                enemyProjectilePrefab.GetComponent<EnemyProjectile>(),
                initialSize: 10,
                canGrow: true
            );

            Debug.Log("[ProjectilePoolInitializer] EnemyProjectile 풀 생성 완료");
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
