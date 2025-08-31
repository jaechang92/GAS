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
    /// GameObject에 Effect를 적용하고 관리하는 컴포넌트
    /// IEffectReceiver 인터페이스를 구현하여 Effect 시스템과 통합
    /// </summary>
    [RequireComponent(typeof(TagComponent))]
    public class EffectComponent : MonoBehaviour, IEffectReceiver
    {
        #region Fields

        [Header("=== Effect Settings ===")]
        [SerializeField] private bool acceptEffects = true;
        [SerializeField] private int maxActiveEffects = 50;
        [SerializeField] private bool debugMode = false;

        [Header("=== Immunity Settings ===")]
        [SerializeField] private List<GameplayTag> immunityTags = new List<GameplayTag>();
        [SerializeField] private float immunityDuration = 0f;

        [Header("=== Effect Filters ===")]
        [SerializeField] private TagRequirement effectApplicationFilter;
        [SerializeField] private List<string> blacklistedEffectIds = new List<string>();

        [Header("=== Runtime Info (Read Only) ===")]
        [SerializeField] private List<EffectInstance> activeEffects = new List<EffectInstance>();
        [SerializeField] private int currentEffectCount = 0;

        // 컴포넌트 캐시
        private TagComponent tagComponent;
        private AttributeSetComponent attributeComponent;

        // Immunity 관리
        private Dictionary<string, float> immunityTimers = new Dictionary<string, float>();

        // Effect 그룹 관리 (같은 ID의 Effect들)
        private Dictionary<string, List<EffectInstance>> effectGroups = new Dictionary<string, List<EffectInstance>>();

        #endregion

        #region Properties

        /// <summary>
        /// 활성화된 Effect 인스턴스 목록 (읽기 전용)
        /// </summary>
        public IReadOnlyList<EffectInstance> ActiveEffects => activeEffects.AsReadOnly();

        /// <summary>
        /// Effect 수신 가능 여부
        /// </summary>
        public bool AcceptEffects
        {
            get => acceptEffects;
            set => acceptEffects = value;
        }

        /// <summary>
        /// 현재 활성 Effect 개수
        /// </summary>
        public int ActiveEffectCount => activeEffects.Count(e => !e.IsExpired);

        #endregion

        #region Events

        public event Action<EffectInstance> OnEffectApplied;
        public event Action<EffectInstance> OnEffectRemoved;
        public event Action<EffectInstance, int> OnEffectStackChanged;
        public event Action<EffectInstance> OnEffectExpired;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 컴포넌트 캐시
            tagComponent = GetComponent<TagComponent>();
            if (tagComponent == null)
            {
                tagComponent = gameObject.AddComponent<TagComponent>();
            }

            attributeComponent = GetComponent<AttributeSetComponent>();

            // 초기화
            activeEffects = new List<EffectInstance>();
            effectGroups = new Dictionary<string, List<EffectInstance>>();
            immunityTimers = new Dictionary<string, float>();
        }

        private void Start()
        {
            // GAS Manager에 등록
            if (GASManager.Instance != null)
            {
                Debug.Log($"[EffectComponent] Registered on {gameObject.name}");
            }
        }

        private void Update()
        {
            // Immunity 타이머 업데이트
            UpdateImmunityTimers();

            // Debug 표시
            if (debugMode)
            {
                currentEffectCount = ActiveEffectCount;
            }
        }

        private void OnDestroy()
        {
            // 모든 Effect 정리
            RemoveAllEffects();
        }

        #endregion

        #region IEffectReceiver Implementation

        /// <summary>
        /// Effect 적용 가능 여부 확인
        /// </summary>
        public bool CanReceiveEffect(GameplayEffect effect, EffectContext context)
        {
            if (!acceptEffects)
            {
                Debug.Log($"[EffectComponent] {gameObject.name} is not accepting effects");
                return false;
            }

            if (effect == null || context == null)
                return false;

            // 최대 Effect 수 체크
            if (activeEffects.Count >= maxActiveEffects)
            {
                Debug.LogWarning($"[EffectComponent] Max effect limit reached on {gameObject.name}");
                return false;
            }

            // 블랙리스트 체크
            if (blacklistedEffectIds.Contains(effect.EffectId))
            {
                Debug.Log($"[EffectComponent] Effect {effect.EffectName} is blacklisted");
                return false;
            }

            // Immunity 체크
            if (IsImmuneToEffect(effect))
            {
                Debug.Log($"[EffectComponent] {gameObject.name} is immune to {effect.EffectName}");
                return false;
            }

            // Effect의 적용 조건 체크
            if (!effect.CanApply(context, gameObject))
            {
                return false;
            }

            // 필터 체크
            if (effectApplicationFilter != null)
            {
                var effectTags = new TagContainer();
                if (effect.AssetTags != null)
                {
                    foreach (var tag in effect.AssetTags.Tags)
                    {
                        effectTags.AddTag(tag);
                    }
                }

                // Effect의 태그가 필터 조건을 만족하는지 확인
                bool satisfiesFilter = true;
                if (effectApplicationFilter.RequiredTags != null)
                {
                    foreach (var requiredTag in effectApplicationFilter.RequiredTags)
                    {
                        if (!effectTags.HasTag(requiredTag))
                        {
                            satisfiesFilter = false;
                            break;
                        }
                    }
                }

                if (!satisfiesFilter)
                {
                    Debug.Log($"[EffectComponent] Effect {effect.EffectName} doesn't satisfy filter requirements");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Effect 적용
        /// </summary>
        public EffectInstance ApplyEffect(GameplayEffect effect, EffectContext context)
        {
            if (!CanReceiveEffect(effect, context))
                return null;

            try
            {
                // Context의 Target이 현재 GameObject인지 확인
                if (context.Target != gameObject)
                {
                    context.Target = gameObject;
                }

                // Effect 적용
                bool success = effect.Apply(context, gameObject);
                if (!success)
                {
                    Debug.LogWarning($"[EffectComponent] Failed to apply {effect.EffectName} to {gameObject.name}");
                    return null;
                }

                // 새 인스턴스 생성 (Effect 타입에 따라 다르게 처리)
                var instance = new EffectInstance(effect, context);

                // 인스턴스 등록
                RegisterInstance(instance);

                // 이벤트 발생
                OnEffectApplied?.Invoke(instance);
                GASEvents.TriggerEffectApplied(gameObject, effect.EffectName);

                if (debugMode)
                {
                    Debug.Log($"[EffectComponent] Applied {effect.EffectName} to {gameObject.name}");
                }

                return instance;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EffectComponent] Error applying effect: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Effect 제거
        /// </summary>
        public bool RemoveEffect(EffectInstance instance)
        {
            if (instance == null || !activeEffects.Contains(instance))
                return false;

            try
            {
                // Effect 제거 호출
                instance.SourceEffect.Remove(instance.Context, gameObject);

                // 인스턴스 등록 해제
                UnregisterInstance(instance);

                // 이벤트 발생
                OnEffectRemoved?.Invoke(instance);
                GASEvents.TriggerEffectRemoved(gameObject, instance.SourceEffect.EffectName);

                if (debugMode)
                {
                    Debug.Log($"[EffectComponent] Removed {instance.SourceEffect.EffectName} from {gameObject.name}");
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EffectComponent] Error removing effect: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Effect ID로 제거
        /// </summary>
        public int RemoveEffectById(string effectId)
        {
            if (string.IsNullOrEmpty(effectId))
                return 0;

            var toRemove = activeEffects
                .Where(e => e.SourceEffect.EffectId == effectId)
                .ToList();

            int removedCount = 0;
            foreach (var instance in toRemove)
            {
                if (RemoveEffect(instance))
                    removedCount++;
            }

            return removedCount;
        }

        /// <summary>
        /// 특정 Source의 모든 Effect 제거
        /// </summary>
        public int RemoveEffectsFromSource(object source)
        {
            if (source == null)
                return 0;

            var toRemove = activeEffects
                .Where(e => e.Context.Instigator == source || e.Context.SourceAbility == source)
                .ToList();

            int removedCount = 0;
            foreach (var instance in toRemove)
            {
                if (RemoveEffect(instance))
                    removedCount++;
            }

            return removedCount;
        }

        /// <summary>
        /// 모든 Effect 제거
        /// </summary>
        public void RemoveAllEffects()
        {
            var toRemove = new List<EffectInstance>(activeEffects);

            foreach (var instance in toRemove)
            {
                RemoveEffect(instance);
            }

            activeEffects.Clear();
            effectGroups.Clear();

            Debug.Log($"[EffectComponent] All effects removed from {gameObject.name}");
        }

        /// <summary>
        /// 특정 ID의 활성 Effect 인스턴스 가져오기
        /// </summary>
        public List<EffectInstance> GetActiveEffectsByID(string effectId)
        {
            if (string.IsNullOrEmpty(effectId))
                return new List<EffectInstance>();

            if (effectGroups.TryGetValue(effectId, out var instances))
            {
                return instances.Where(e => !e.IsExpired).ToList();
            }

            return new List<EffectInstance>();
        }

        /// <summary>
        /// 특정 태그를 가진 Effect 인스턴스 가져오기
        /// </summary>
        public List<EffectInstance> GetActiveEffectsByTag(GameplayTag tag)
        {
            if (tag == null)
                return new List<EffectInstance>();

            return activeEffects
                .Where(e => !e.IsExpired && e.SourceEffect.AssetTags != null &&
                       e.SourceEffect.AssetTags.HasTag(tag))
                .ToList();
        }

        /// <summary>
        /// Effect 스택 수 가져오기
        /// </summary>
        public int GetEffectStackCount(string effectId)
        {
            var effects = GetActiveEffectsByID(effectId);

            if (effects.Count == 0)
                return 0;

            // 스택 정책에 따라 다르게 계산
            var firstEffect = effects.First();
            if (firstEffect.SourceEffect.StackingPolicy == EffectStackingPolicy.Stack)
            {
                return firstEffect.CurrentStack;
            }
            else
            {
                return effects.Count;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 특정 Effect에 대한 면역 부여
        /// </summary>
        public void GrantImmunity(string effectId, float duration)
        {
            if (string.IsNullOrEmpty(effectId))
                return;

            immunityTimers[effectId] = Time.time + duration;

            Debug.Log($"[EffectComponent] Granted immunity to {effectId} for {duration}s");
        }

        /// <summary>
        /// 디버프 제거 (Purge)
        /// </summary>
        public int PurgeDebuffs(int purgeLevel = 1)
        {
            var debuffs = activeEffects
                .Where(e => !e.IsExpired &&
                       e.SourceEffect.AssetTags != null &&
                       e.SourceEffect.AssetTags.HasTag(new GameplayTag("Effect.Type.Debuff")))
                .ToList();

            int purgedCount = 0;
            foreach (var debuff in debuffs)
            {
                // Purge 가능 여부 체크
                if (debuff.SourceEffect is InfiniteEffect infinite && !infinite.CanBeDispelled)
                    continue;

                var context = debuff.Context.Clone();
                context.SetData("DispelPower", purgeLevel);

                if (RemoveEffect(debuff))
                    purgedCount++;
            }

            Debug.Log($"[EffectComponent] Purged {purgedCount} debuffs from {gameObject.name}");
            return purgedCount;
        }

        /// <summary>
        /// 버프 제거 (Dispel)
        /// </summary>
        public int DispelBuffs(int dispelLevel = 1)
        {
            var buffs = activeEffects
                .Where(e => !e.IsExpired &&
                       e.SourceEffect.AssetTags != null &&
                       e.SourceEffect.AssetTags.HasTag(new GameplayTag("Effect.Type.Buff")))
                .ToList();

            int dispelledCount = 0;
            foreach (var buff in buffs)
            {
                // Dispel 가능 여부 체크
                if (buff.SourceEffect is InfiniteEffect infinite && !infinite.CanBeDispelled)
                    continue;

                var context = buff.Context.Clone();
                context.SetData("DispelPower", dispelLevel);

                if (RemoveEffect(buff))
                    dispelledCount++;
            }

            Debug.Log($"[EffectComponent] Dispelled {dispelledCount} buffs from {gameObject.name}");
            return dispelledCount;
        }

        /// <summary>
        /// Effect 존재 여부 확인
        /// </summary>
        public bool HasEffect(string effectId)
        {
            return GetActiveEffectsByID(effectId).Count > 0;
        }

        /// <summary>
        /// 특정 태그를 가진 Effect 존재 여부
        /// </summary>
        public bool HasEffectWithTag(GameplayTag tag)
        {
            return GetActiveEffectsByTag(tag).Count > 0;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 인스턴스 등록
        /// </summary>
        private void RegisterInstance(EffectInstance instance)
        {
            if (instance == null) return;

            activeEffects.Add(instance);

            // Effect 그룹에 추가
            string effectId = instance.SourceEffect.EffectId;
            if (!effectGroups.ContainsKey(effectId))
            {
                effectGroups[effectId] = new List<EffectInstance>();
            }
            effectGroups[effectId].Add(instance);
        }

        /// <summary>
        /// 인스턴스 등록 해제
        /// </summary>
        private void UnregisterInstance(EffectInstance instance)
        {
            if (instance == null) return;

            activeEffects.Remove(instance);

            // Effect 그룹에서 제거
            string effectId = instance.SourceEffect.EffectId;
            if (effectGroups.TryGetValue(effectId, out var group))
            {
                group.Remove(instance);
                if (group.Count == 0)
                {
                    effectGroups.Remove(effectId);
                }
            }
        }

        /// <summary>
        /// Effect 면역 확인
        /// </summary>
        private bool IsImmuneToEffect(GameplayEffect effect)
        {
            // Effect ID 기반 면역
            if (immunityTimers.ContainsKey(effect.EffectId))
            {
                if (immunityTimers[effect.EffectId] > Time.time)
                    return true;
            }

            // 태그 기반 면역
            if (effect.AssetTags != null)
            {
                foreach (var immunityTag in immunityTags)
                {
                    if (effect.AssetTags.HasTag(immunityTag))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 면역 타이머 업데이트
        /// </summary>
        private void UpdateImmunityTimers()
        {
            var expiredImmunities = new List<string>();

            foreach (var kvp in immunityTimers)
            {
                if (kvp.Value <= Time.time)
                {
                    expiredImmunities.Add(kvp.Key);
                }
            }

            foreach (var key in expiredImmunities)
            {
                immunityTimers.Remove(key);
                Debug.Log($"[EffectComponent] Immunity to {key} expired");
            }
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxActiveEffects = Mathf.Max(1, maxActiveEffects);
            immunityDuration = Mathf.Max(0f, immunityDuration);
        }

        private void OnDrawGizmosSelected()
        {
            // Active effects 시각화
            if (activeEffects != null && activeEffects.Count > 0)
            {
                Gizmos.color = Color.green;
                float offset = 0.5f;
                foreach (var effect in activeEffects)
                {
                    if (effect != null && !effect.IsExpired)
                    {
                        Gizmos.DrawWireCube(
                            transform.position + Vector3.up * offset,
                            Vector3.one * 0.1f
                        );
                        offset += 0.2f;
                    }
                }
            }
        }
#endif

        #endregion
    }
}

// 파일 위치: Assets/Scripts/GAS/EffectSystem/EffectComponent.cs