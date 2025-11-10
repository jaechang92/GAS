using UnityEngine;
using GASPT.Core.Pooling;

namespace GASPT.Gameplay.Effects
{
    /// <summary>
    /// Effect 풀 초기화
    /// 게임 시작 시 모든 Effect 풀을 미리 생성
    /// </summary>
    public static class EffectPoolInitializer
    {
        private static bool isInitialized = false;

        /// <summary>
        /// 모든 Effect 풀 초기화
        /// </summary>
        public static void InitializeAllPools()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[EffectPoolInitializer] 이미 초기화됨");
                return;
            }

            Debug.Log("[EffectPoolInitializer] Effect 풀 초기화 시작...");

            // VisualEffect 풀 생성
            InitializeVisualEffectPool();

            isInitialized = true;
            Debug.Log("[EffectPoolInitializer] Effect 풀 초기화 완료");
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

            Debug.Log("[EffectPoolInitializer] VisualEffect 풀 생성 완료");
        }


        // ====== 프리팹 생성 (런타임) ======

        /// <summary>
        /// VisualEffect 프리팹 생성
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
