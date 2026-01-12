using System.Collections.Generic;
using UnityEngine;
using GASPT.Core.Extensions;
using GASPT.ResourceManagement;

namespace GASPT.Core.Pooling
{
    /// <summary>
    /// 오브젝트 풀 매니저 싱글톤
    /// 게임의 모든 오브젝트 풀을 중앙에서 관리
    /// </summary>
    public class PoolManager : SingletonManager<PoolManager>
    {
        // ====== 풀 저장소 ======

        private Dictionary<string, object> pools = new Dictionary<string, object>();
        private Transform poolRoot;


        // ====== 디버그 ======

        [Header("디버그")]
        [SerializeField] private bool showDebugLogs = false;


        // ====== 초기화 ======

        protected override void Awake()
        {
            base.Awake();

            // 풀 루트 생성
            poolRoot = new GameObject("PoolRoot").transform;
            poolRoot.SetParent(transform);

            if (showDebugLogs)
                Debug.Log("[PoolManager] 초기화 완료");
        }


        // ====== 풀 생성 ======

        /// <summary>
        /// 새로운 풀 생성 또는 기존 풀 반환
        /// </summary>
        public ObjectPool<T> CreatePool<T>(T prefab, int initialSize = 10, bool canGrow = true) where T : Component
        {
            string poolKey = typeof(T).Name;

            // 이미 풀이 존재하면 반환
            if (pools.ContainsKey(poolKey))
            {
                if (showDebugLogs)
                    Debug.Log($"[PoolManager] 기존 풀 반환: {poolKey}");
                return pools[poolKey] as ObjectPool<T>;
            }

            // 풀 부모 GameObject 생성
            GameObject poolParent = new GameObject($"Pool_{poolKey}");
            poolParent.transform.SetParent(poolRoot);

            // 새 풀 생성
            var pool = new ObjectPool<T>(prefab, poolParent.transform, initialSize, canGrow);
            pools[poolKey] = pool;

            if (showDebugLogs)
                Debug.Log($"[PoolManager] 새 풀 생성: {poolKey} (초기 크기: {initialSize}, 확장: {canGrow})");

            return pool;
        }


        // ====== 풀 가져오기 ======

        /// <summary>
        /// 기존 풀 가져오기 (TryGetValue 최적화)
        /// </summary>
        public ObjectPool<T> GetPool<T>() where T : Component
        {
            string poolKey = typeof(T).Name;

            if (pools.TryGetValue(poolKey, out var pool))
            {
                return pool as ObjectPool<T>;
            }

            Debug.LogError($"[PoolManager] {poolKey} 풀이 존재하지 않습니다! CreatePool을 먼저 호출하세요.");
            return null;
        }

        /// <summary>
        /// 풀 존재 여부 확인
        /// </summary>
        public bool HasPool<T>() where T : Component
        {
            string poolKey = typeof(T).Name;
            return pools.ContainsKey(poolKey);
        }


        // ====== 편의 메서드 ======

        /// <summary>
        /// 풀에서 오브젝트 가져오기 (편의 메서드)
        /// </summary>
        public T Spawn<T>(Vector3 position, Quaternion rotation) where T : Component
        {
            var pool = GetPool<T>();
            if (pool == null)
            {
                Debug.LogError($"[PoolManager] {typeof(T).Name} 풀이 없습니다!");
                return null;
            }

            return pool.Get(position, rotation);
        }

        /// <summary>
        /// 프리팹 경로로 오브젝트 스폰 (편의 메서드)
        /// 풀이 없으면 자동 생성
        /// </summary>
        public T Spawn<T>(string prefabPath, Vector3 position, Quaternion rotation) where T : Component
        {
            // 풀이 이미 존재하면 바로 스폰 (성능 최적화)
            if (HasPool<T>())
            {
                return Spawn<T>(position, rotation);
            }

            // 풀이 없을 때만 프리팹 로드 및 풀 생성
            if (!GameResourceManager.HasInstance)
            {
                Debug.LogError($"[PoolManager] GameResourceManager를 찾을 수 없습니다!");
                return null;
            }

            // 프리팹 로드
            GameObject prefab = GameResourceManager.Instance.LoadPrefab(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[PoolManager] 프리팹을 로드할 수 없습니다: {prefabPath}");
                return null;
            }

            // T 컴포넌트 확인
            T component = prefab.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"[PoolManager] 프리팹에 {typeof(T).Name} 컴포넌트가 없습니다: {prefabPath}");
                return null;
            }

            // 풀 생성 (초기 크기 10, 자동 확장)
            CreatePool(component, initialSize: 10, canGrow: true);

            if (showDebugLogs)
                Debug.Log($"[PoolManager] {typeof(T).Name} 풀 자동 생성 완료 (경로: {prefabPath})");

            // 생성된 풀에서 스폰
            return Spawn<T>(position, rotation);
        }

        /// <summary>
        /// 오브젝트를 풀로 반환 (TryGetValue 최적화)
        /// </summary>
        public void Despawn<T>(T obj) where T : Component
        {
            if (obj == null) return;

            // 런타임 타입 사용 (FireballProjectile, BasicMeleeEnemy 등)
            System.Type actualType = obj.GetType();
            string poolKey = actualType.Name;

            // 풀 찾기 (TryGetValue 최적화)
            if (!pools.TryGetValue(poolKey, out var pool))
            {
                Debug.LogWarning($"[PoolManager] {poolKey} 풀이 없습니다. GameObject 파괴합니다.");
                Destroy(obj.gameObject);
                return;
            }

            // Reflection으로 Release 호출
            var releaseMethod = pool.GetType().GetMethod("Release");
            if (releaseMethod != null)
            {
                releaseMethod.Invoke(pool, new object[] { obj });
            }
            else
            {
                Debug.LogError($"[PoolManager] {poolKey} 풀의 Release 메서드를 찾을 수 없습니다.");
                Destroy(obj.gameObject);
            }
        }

        /// <summary>
        /// 특정 풀의 모든 오브젝트 반환
        /// </summary>
        public void DespawnAll<T>() where T : Component
        {
            var pool = GetPool<T>();
            if (pool == null)
            {
                Debug.LogWarning($"[PoolManager] {typeof(T).Name} 풀이 없습니다.");
                return;
            }

            pool.ReleaseAll();
        }

        /// <summary>
        /// 모든 풀의 모든 오브젝트 반환
        /// </summary>
        public void DespawnAll()
        {
            foreach (var pool in pools.Values)
            {
                // Reflection으로 ReleaseAll 호출
                var releaseAllMethod = pool.GetType().GetMethod("ReleaseAll");
                if (releaseAllMethod != null)
                {
                    releaseAllMethod.Invoke(pool, null);
                }
            }

            if (showDebugLogs)
                Debug.Log("[PoolManager] 모든 풀의 오브젝트 반환 완료");
        }


        // ====== 디버그 ======

        /// <summary>
        /// 모든 풀 정보 출력
        /// </summary>
        [ContextMenu("Print Pool Info")]
        public void PrintPoolInfo()
        {
            Debug.Log("========== Pool Manager Info ==========");
            Debug.Log($"Total Pools: {pools.Count}");

            foreach (var kvp in pools)
            {
                var pool = kvp.Value;
                var getInfoMethod = pool.GetType().GetMethod("GetInfo");
                if (getInfoMethod != null)
                {
                    var info = getInfoMethod.Invoke(pool, null) as PoolInfo?;
                    if (info.HasValue)
                    {
                        var poolInfo = info.Value;
                        Debug.Log($"  [{poolInfo.typeName}] Total: {poolInfo.totalCount}, " +
                                 $"Active: {poolInfo.activeCount}, " +
                                 $"Available: {poolInfo.availableCount}, " +
                                 $"Initial: {poolInfo.initialSize}, " +
                                 $"CanGrow: {poolInfo.canGrow}");
                    }
                }
            }

            Debug.Log("=======================================");
        }

        /// <summary>
        /// 모든 풀 초기화
        /// </summary>
        [ContextMenu("Clear All Pools")]
        public void ClearAllPools()
        {
            DespawnAll();
            pools.Clear();

            // PoolRoot 하위 모든 오브젝트 제거
            foreach (Transform child in poolRoot)
            {
                Destroy(child.gameObject);
            }

            if (showDebugLogs)
                Debug.Log("[PoolManager] 모든 풀 초기화 완료");
        }
    }
}
