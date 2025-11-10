using UnityEngine;
using GASPT.Core.Pooling;

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

            isInitialized = true;
            Debug.Log("[ProjectilePoolInitializer] 투사체 풀 초기화 완료");
        }

        /// <summary>
        /// FireballProjectile 풀 초기화
        /// </summary>
        private static void InitializeFireballPool()
        {
            // 런타임에서 FireballProjectile 프리팹 생성
            GameObject fireballPrefab = CreateFireballPrefab();

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
            // 런타임에서 MagicMissileProjectile 프리팹 생성
            GameObject missilePrefab = CreateMagicMissilePrefab();

            // 풀 생성 (초기 10개, 확장 가능)
            PoolManager.Instance.CreatePool(
                missilePrefab.GetComponent<MagicMissileProjectile>(),
                initialSize: 10,
                canGrow: true
            );

            Debug.Log("[ProjectilePoolInitializer] MagicMissileProjectile 풀 생성 완료");
        }


        // ====== 프리팹 생성 (런타임) ======

        /// <summary>
        /// FireballProjectile 프리팹 생성
        /// </summary>
        private static GameObject CreateFireballPrefab()
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            prefab.name = "FireballProjectile";
            prefab.transform.localScale = Vector3.one * 0.5f;

            // Collider 제거 (투사체는 자체 충돌 검사 사용)
            Object.Destroy(prefab.GetComponent<Collider>());

            // 컴포넌트 추가
            prefab.AddComponent<FireballProjectile>();
            prefab.AddComponent<PooledObject>();

            // TrailRenderer 추가
            var trail = prefab.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 0.3f;
            trail.endWidth = 0.05f;

            // 비활성화 (풀에서 관리)
            prefab.SetActive(false);

            return prefab;
        }

        /// <summary>
        /// MagicMissileProjectile 프리팹 생성
        /// </summary>
        private static GameObject CreateMagicMissilePrefab()
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            prefab.name = "MagicMissileProjectile";
            prefab.transform.localScale = Vector3.one * 0.3f;

            // Collider 제거
            Object.Destroy(prefab.GetComponent<Collider>());

            // 컴포넌트 추가
            prefab.AddComponent<MagicMissileProjectile>();
            prefab.AddComponent<PooledObject>();

            // TrailRenderer 추가
            var trail = prefab.AddComponent<TrailRenderer>();
            trail.time = 0.3f;
            trail.startWidth = 0.15f;
            trail.endWidth = 0.02f;

            // 비활성화
            prefab.SetActive(false);

            return prefab;
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
