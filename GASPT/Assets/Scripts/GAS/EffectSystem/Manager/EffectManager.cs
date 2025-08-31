using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GAS.Core;
using GAS.AttributeSystem;
using GAS.TagSystem;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Effect 시스템의 전역 관리자
    /// Effect 적용, 추적, Object Pooling, 성능 최적화를 담당
    /// </summary>
    public class EffectManager : SingletonManager<EffectManager>
    {
        #region Fields

        [Header("=== Manager Settings ===")]
        [SerializeField] private bool enableObjectPooling = true;
        [SerializeField] private int poolInitialSize = 10;
        [SerializeField] private int poolMaxSize = 100;
        [SerializeField] private bool autoCleanup = true;
        [SerializeField] private float cleanupInterval = 30f;

        [Header("=== Performance Settings ===")]
        [SerializeField] private int maxConcurrentEffects = 500;
        [SerializeField] private bool throttleEffectApplication = true;
        [SerializeField] private float effectApplicationCooldown = 0.01f;

        [Header("=== Debug Settings ===")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool logDetailedInfo = false;

        [Header("=== Runtime Statistics ===")]
        [SerializeField] private int totalActiveEffects = 0;
        [SerializeField] private int totalEffectsApplied = 0;
        [SerializeField] private int totalEffectsRemoved = 0;
        [SerializeField] private float averageEffectDuration = 0f;

        // Effect 추적
        private Dictionary<GameObject, List<EffectInstance>> targetEffectMap;
        private Dictionary<string, List<EffectInstance>> effectTypeMap;
        private List<EffectInstance> allActiveEffects;

        // Object Pooling
        private Dictionary<string, Queue<GameObject>> visualEffectPools;
        private Dictionary<string, int> poolUsageStats;

        // 성능 관리
        private float lastEffectApplicationTime;
        private Queue<float> effectDurationSamples;
        private const int MaxDurationSamples = 100;

        // Cleanup
        private float nextCleanupTime;

        #endregion

        #region Properties

        /// <summary>
        /// 모든 활성 Effect 인스턴스
        /// </summary>
        public IReadOnlyList<EffectInstance> AllActiveEffects => allActiveEffects.AsReadOnly();

        /// <summary>
        /// 활성 Effect 총 개수
        /// </summary>
        public int ActiveEffectCount => allActiveEffects.Count(e => !e.IsExpired);

        /// <summary>
        /// Effect 적용 가능 여부
        /// </summary>
        public bool CanApplyEffects => ActiveEffectCount < maxConcurrentEffects;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            InitializeManager();
        }

        private void Start()
        {
            // Object Pool 초기화
            if (enableObjectPooling)
            {
                InitializeObjectPools();
            }

            // GAS Manager 연동
            if (GASManager.Instance != null)
            {
                Debug.Log("[EffectManager] Initialized and registered with GASManager");
            }
        }

        private void Update()
        {
            // 통계 업데이트
            UpdateStatistics();

            // 자동 정리
            if (autoCleanup && Time.time >= nextCleanupTime)
            {
                PerformCleanup();
                nextCleanupTime = Time.time + cleanupInterval;
            }
        }

        private void OnDestroy()
        {
            CleanupAll();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 매니저 초기화
        /// </summary>
        private void InitializeManager()
        {
            targetEffectMap = new Dictionary<GameObject, List<EffectInstance>>();
            effectTypeMap = new Dictionary<string, List<EffectInstance>>();
            allActiveEffects = new List<EffectInstance>();

            visualEffectPools = new Dictionary<string, Queue<GameObject>>();
            poolUsageStats = new Dictionary<string, int>();

            effectDurationSamples = new Queue<float>(MaxDurationSamples);

            nextCleanupTime = Time.time + cleanupInterval;

            // 이벤트 구독
            SubscribeToEvents();
        }

        /// <summary>
        /// Object Pool 초기화
        /// </summary>
        private void InitializeObjectPools()
        {
            // 기본 Effect 프리팹들에 대한 풀 생성
            // 실제 프로젝트에서는 Resources나 Addressables에서 로드
            Debug.Log("[EffectManager] Object pooling initialized");
        }

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            GASEvents.OnEffectApplied += OnEffectAppliedHandler;
            GASEvents.OnEffectRemoved += OnEffectRemovedHandler;
            GASEvents.OnEffectStacked += OnEffectStackChangedHandler;
        }

        #endregion

        #region Public Methods - Effect Application

        /// <summary>
        /// Effect를 대상에 적용
        /// </summary>
        public EffectInstance ApplyEffect(GameplayEffect effect, GameObject instigator, GameObject target, float magnitude = 1f)
        {
            if (!CanApplyEffect(effect, target))
                return null;

            // Throttling 체크
            if (throttleEffectApplication)
            {
                float timeSinceLastApplication = Time.time - lastEffectApplicationTime;
                if (timeSinceLastApplication < effectApplicationCooldown)
                {
                    Debug.LogWarning("[EffectManager] Effect application throttled");
                    return null;
                }
            }

            // EffectComponent 확인 및 추가
            var effectComponent = target.GetComponent<EffectComponent>();
            if (effectComponent == null)
            {
                effectComponent = target.AddComponent<EffectComponent>();
                Debug.Log($"[EffectManager] Added EffectComponent to {target.name}");
            }

            // Context 생성
            var context = new EffectContext(instigator, target, effect)
            {
                Magnitude = magnitude
            };

            // Effect 적용
            var instance = effectComponent.ApplyEffect(effect, context);
            if (instance != null)
            {
                RegisterEffectInstance(instance, target);
                lastEffectApplicationTime = Time.time;
                totalEffectsApplied++;

                if (debugMode)
                {
                    Debug.Log($"[EffectManager] Applied {effect.EffectName} from {instigator?.name} to {target.name}");
                }
            }

            return instance;
        }

        /// <summary>
        /// AOE Effect 적용
        /// </summary>
        public List<EffectInstance> ApplyAOEEffect(
            GameplayEffect effect,
            GameObject instigator,
            Vector3 center,
            float radius,
            LayerMask targetLayers,
            int maxTargets = -1)
        {
            var instances = new List<EffectInstance>();

            // 범위 내 대상 찾기
            var colliders = Physics.OverlapSphere(center, radius, targetLayers);
            var targets = colliders
                .Select(c => c.gameObject)
                .Where(g => g != instigator)
                .Take(maxTargets > 0 ? maxTargets : colliders.Length)
                .ToList();

            foreach (var target in targets)
            {
                var instance = ApplyEffect(effect, instigator, target);
                if (instance != null)
                {
                    instances.Add(instance);
                }
            }

            Debug.Log($"[EffectManager] Applied AOE effect to {instances.Count} targets");
            return instances;
        }

        /// <summary>
        /// Chain Effect 적용
        /// </summary>
        public async Awaitable ApplyChainEffectAsync(
            GameplayEffect effect,
            GameObject instigator,
            GameObject firstTarget,
            int maxChains = 3,
            float chainRadius = 5f,
            float chainDelay = 0.2f)
        {
            var processedTargets = new HashSet<GameObject> { firstTarget };
            var currentTarget = firstTarget;

            // 첫 대상에 적용
            var firstInstance = ApplyEffect(effect, instigator, firstTarget);
            if (firstInstance == null) return;

            for (int i = 0; i < maxChains; i++)
            {
                await Awaitable.WaitForSecondsAsync(chainDelay);

                // 다음 대상 찾기
                var nextTarget = FindNearestTarget(currentTarget, chainRadius, processedTargets);
                if (nextTarget == null) break;

                // Chain effect 적용
                var chainInstance = ApplyEffect(effect, instigator, nextTarget, 0.8f); // 80% 강도
                if (chainInstance != null)
                {
                    processedTargets.Add(nextTarget);
                    currentTarget = nextTarget;
                }
            }

            Debug.Log($"[EffectManager] Chain effect completed with {processedTargets.Count} targets");
        }

        #endregion

        #region Public Methods - Effect Management

        /// <summary>
        /// 특정 대상의 모든 Effect 제거
        /// </summary>
        public void RemoveAllEffectsFromTarget(GameObject target)
        {
            var effectComponent = target.GetComponent<EffectComponent>();
            if (effectComponent != null)
            {
                effectComponent.RemoveAllEffects();
            }

            // 추적 정보 제거
            if (targetEffectMap.ContainsKey(target))
            {
                var instances = targetEffectMap[target];
                foreach (var instance in instances)
                {
                    UnregisterEffectInstance(instance, target);
                }
            }
        }

        /// <summary>
        /// 특정 타입의 모든 Effect 제거
        /// </summary>
        public int RemoveAllEffectsOfType(string effectId)
        {
            if (!effectTypeMap.TryGetValue(effectId, out var instances))
                return 0;

            int removedCount = 0;
            var toRemove = new List<EffectInstance>(instances);

            foreach (var instance in toRemove)
            {
                if (instance.Context.Target != null)
                {
                    var effectComponent = instance.Context.Target.GetComponent<EffectComponent>();
                    if (effectComponent != null && effectComponent.RemoveEffect(instance))
                    {
                        removedCount++;
                    }
                }
            }

            return removedCount;
        }

        /// <summary>
        /// 전역 Dispel
        /// </summary>
        public void GlobalDispel(bool includeBuffs = false, bool includeDebuffs = true)
        {
            var allTargets = targetEffectMap.Keys.ToList();

            foreach (var target in allTargets)
            {
                var effectComponent = target.GetComponent<EffectComponent>();
                if (effectComponent != null)
                {
                    if (includeDebuffs)
                        effectComponent.PurgeDebuffs(99);
                    if (includeBuffs)
                        effectComponent.DispelBuffs(99);
                }
            }

            Debug.Log("[EffectManager] Global dispel executed");
        }

        #endregion

        #region Public Methods - Queries

        /// <summary>
        /// 특정 대상의 Effect 목록 가져오기
        /// </summary>
        public List<EffectInstance> GetTargetEffects(GameObject target)
        {
            if (targetEffectMap.TryGetValue(target, out var instances))
            {
                return instances.Where(e => !e.IsExpired).ToList();
            }
            return new List<EffectInstance>();
        }

        /// <summary>
        /// 특정 타입의 모든 활성 Effect 가져오기
        /// </summary>
        public List<EffectInstance> GetActiveEffectsByType(string effectId)
        {
            if (effectTypeMap.TryGetValue(effectId, out var instances))
            {
                return instances.Where(e => !e.IsExpired).ToList();
            }
            return new List<EffectInstance>();
        }

        /// <summary>
        /// Effect 적용 가능 여부 확인
        /// </summary>
        public bool CanApplyEffect(GameplayEffect effect, GameObject target)
        {
            if (effect == null || target == null)
                return false;

            if (!CanApplyEffects)
            {
                Debug.LogWarning("[EffectManager] Maximum concurrent effects reached");
                return false;
            }

            return true;
        }

        #endregion

        #region Public Methods - Object Pooling

        /// <summary>
        /// Visual Effect 가져오기 (Pool에서)
        /// </summary>
        public GameObject GetVisualEffect(GameObject prefab)
        {
            if (!enableObjectPooling || prefab == null)
            {
                return prefab != null ? Instantiate(prefab) : null;
            }

            string poolKey = prefab.name;

            if (!visualEffectPools.ContainsKey(poolKey))
            {
                visualEffectPools[poolKey] = new Queue<GameObject>();
            }

            GameObject effect = null;

            if (visualEffectPools[poolKey].Count > 0)
            {
                effect = visualEffectPools[poolKey].Dequeue();
                effect.SetActive(true);
            }
            else
            {
                effect = Instantiate(prefab);
                effect.name = poolKey;
            }

            // 사용 통계 업데이트
            if (!poolUsageStats.ContainsKey(poolKey))
                poolUsageStats[poolKey] = 0;
            poolUsageStats[poolKey]++;

            return effect;
        }

        /// <summary>
        /// Visual Effect 반환 (Pool로)
        /// </summary>
        public void ReturnVisualEffect(GameObject effect)
        {
            if (!enableObjectPooling || effect == null)
            {
                if (effect != null) Destroy(effect);
                return;
            }

            string poolKey = effect.name;

            if (!visualEffectPools.ContainsKey(poolKey))
            {
                visualEffectPools[poolKey] = new Queue<GameObject>();
            }

            if (visualEffectPools[poolKey].Count < poolMaxSize)
            {
                effect.SetActive(false);
                effect.transform.SetParent(transform);
                visualEffectPools[poolKey].Enqueue(effect);
            }
            else
            {
                Destroy(effect);
            }
        }

        #endregion

        #region Private Methods - Registration

        /// <summary>
        /// Effect 인스턴스 등록
        /// </summary>
        private void RegisterEffectInstance(EffectInstance instance, GameObject target)
        {
            if (instance == null || target == null) return;

            // 전체 목록에 추가
            allActiveEffects.Add(instance);

            // Target별 추적
            if (!targetEffectMap.ContainsKey(target))
            {
                targetEffectMap[target] = new List<EffectInstance>();
            }
            targetEffectMap[target].Add(instance);

            // Type별 추적
            string effectId = instance.SourceEffect.EffectId;
            if (!effectTypeMap.ContainsKey(effectId))
            {
                effectTypeMap[effectId] = new List<EffectInstance>();
            }
            effectTypeMap[effectId].Add(instance);
        }

        /// <summary>
        /// Effect 인스턴스 등록 해제
        /// </summary>
        private void UnregisterEffectInstance(EffectInstance instance, GameObject target)
        {
            if (instance == null) return;

            // 전체 목록에서 제거
            allActiveEffects.Remove(instance);

            // Type별 추적에서 제거
            string effectId = instance.SourceEffect.EffectId;
            if (effectTypeMap.TryGetValue(effectId, out var typeInstances))
            {
                typeInstances.Remove(instance);
                if (typeInstances.Count == 0)
                {
                    effectTypeMap.Remove(effectId);
                }
            }

            // Duration 샘플 추가
            if (instance.SourceEffect.DurationPolicy == EffectDurationPolicy.HasDuration)
            {
                AddDurationSample(instance.ElapsedTime);
            }

            totalEffectsRemoved++;
        }

        #endregion

        #region Private Methods - Utility

        /// <summary>
        /// 가장 가까운 대상 찾기
        /// </summary>
        private GameObject FindNearestTarget(GameObject from, float radius, HashSet<GameObject> exclude)
        {
            var colliders = Physics.OverlapSphere(from.transform.position, radius);
            GameObject nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var collider in colliders)
            {
                if (exclude.Contains(collider.gameObject)) continue;

                float distance = Vector3.Distance(from.transform.position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearest = collider.gameObject;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Duration 샘플 추가
        /// </summary>
        private void AddDurationSample(float duration)
        {
            effectDurationSamples.Enqueue(duration);
            if (effectDurationSamples.Count > MaxDurationSamples)
            {
                effectDurationSamples.Dequeue();
            }

            // 평균 계산
            if (effectDurationSamples.Count > 0)
            {
                averageEffectDuration = effectDurationSamples.Average();
            }
        }

        /// <summary>
        /// 통계 업데이트
        /// </summary>
        private void UpdateStatistics()
        {
            totalActiveEffects = ActiveEffectCount;

            if (debugMode && logDetailedInfo && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[EffectManager] Stats - Active: {totalActiveEffects}, Applied: {totalEffectsApplied}, Removed: {totalEffectsRemoved}");
            }
        }

        /// <summary>
        /// 정리 수행
        /// </summary>
        private void PerformCleanup()
        {
            // 만료된 인스턴스 제거
            var expiredInstances = allActiveEffects.Where(e => e.IsExpired).ToList();
            foreach (var instance in expiredInstances)
            {
                UnregisterEffectInstance(instance, instance.Context.Target);
            }

            // null 대상 정리
            var nullTargets = targetEffectMap.Where(kvp => kvp.Key == null).Select(kvp => kvp.Key).ToList();
            foreach (var target in nullTargets)
            {
                targetEffectMap.Remove(target);
            }

            if (debugMode)
            {
                Debug.Log($"[EffectManager] Cleanup performed - Removed {expiredInstances.Count} expired effects");
            }
        }

        /// <summary>
        /// 전체 정리
        /// </summary>
        private void CleanupAll()
        {
            // 모든 활성 Effect 제거
            foreach (var kvp in targetEffectMap)
            {
                if (kvp.Key != null)
                {
                    RemoveAllEffectsFromTarget(kvp.Key);
                }
            }

            targetEffectMap.Clear();
            effectTypeMap.Clear();
            allActiveEffects.Clear();

            // Pool 정리
            foreach (var pool in visualEffectPools.Values)
            {
                while (pool.Count > 0)
                {
                    var obj = pool.Dequeue();
                    if (obj != null) Destroy(obj);
                }
            }
            visualEffectPools.Clear();
        }

        #endregion

        #region Event Handlers

        private void OnEffectAppliedHandler(GameObject target, string effectName)
        {
            if (debugMode)
            {
                Debug.Log($"[EffectManager] Effect applied event: {effectName} on {target.name}");
            }
        }

        private void OnEffectRemovedHandler(GameObject target, string effectName)
        {
            if (debugMode)
            {
                Debug.Log($"[EffectManager] Effect removed event: {effectName} from {target.name}");
            }
        }

        private void OnEffectStackChangedHandler(GameObject target, string effectName, int newStack)
        {
            if (debugMode)
            {
                Debug.Log($"[EffectManager] Stack changed event: {effectName} on {target.name} - New stack: {newStack}");
            }
        }

        #endregion
    }
}

// 파일 위치: Assets/Scripts/GAS/EffectSystem/Manager/EffectManager.cs