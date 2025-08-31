using GAS.AttributeSystem;
using GAS.Core;
using GAS.TagSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.EffectSystem
{
    /// <summary>
    /// GameplayEffect의 타입을 정의
    /// </summary>
    public enum EffectType
    {
        Instant,    // 즉시 적용 후 종료
        Duration,   // 일정 시간 지속
        Periodic,   // 주기적으로 틱 발생
        Infinite    // 수동 제거까지 영구 지속
    }

    /// <summary>
    /// Effect의 스택 정책을 정의
    /// </summary>
    public enum EffectStackingPolicy
    {
        None,               // 스택 불가 (기존 Effect 유지)
        Override,           // 기존 Effect를 새 Effect로 덮어쓰기
        Stack,              // 개별 Effect로 스택 (각각 독립적으로 동작)
        AggregateBySource,  // 같은 source의 Effect는 하나로 합침
        AggregateByTarget   // 같은 target의 Effect는 하나로 합침
    }

    /// <summary>
    /// Effect의 지속시간 정책을 정의
    /// </summary>
    public enum EffectDurationPolicy
    {
        Instant,        // 즉시 적용
        HasDuration,    // 지속 시간 있음
        Infinite        // 무한 지속
    }

    /// <summary>
    /// Modifier 적용 시점을 정의
    /// </summary>
    public enum ModifierApplicationTiming
    {
        OnApplication,  // Effect 적용 시점
        OnPeriodic,     // 주기적 틱 시점
        OnRemoval,      // Effect 제거 시점
        OnStack         // 스택 변경 시점
    }

    /// <summary>
    /// GameplayEffect의 베이스 클래스
    /// 모든 Effect 타입의 기반이 되는 추상 클래스
    /// </summary>
    public abstract class GameplayEffect : ScriptableObject
    {
        #region Serialized Fields

        [Header("=== Effect Identity ===")]
        [SerializeField] protected string effectId;
        [SerializeField] protected string effectName;
        [TextArea(2, 4)]
        [SerializeField] protected string description;

        [Header("=== Effect Type ===")]
        [SerializeField] protected EffectType effectType = EffectType.Instant;
        [SerializeField] protected EffectDurationPolicy durationPolicy = EffectDurationPolicy.Instant;

        [Header("=== Duration Settings ===")]
        [SerializeField] protected float duration = 1f;
        [SerializeField] protected float period = 1f;
        [SerializeField] protected bool executePeriodicOnApplication = true;

        [Header("=== Stack Settings ===")]
        [SerializeField] protected EffectStackingPolicy stackingPolicy = EffectStackingPolicy.None;
        [SerializeField] protected int maxStackCount = 1;
        [SerializeField] protected bool refreshDurationOnStack = true;
        [SerializeField] protected bool resetPeriodicOnStack = false;

        [Header("=== Tag Requirements ===")]
        [SerializeField] protected TagRequirement applicationRequirement;
        [SerializeField] protected TagRequirement ongoingRequirement;
        [SerializeField] protected TagRequirement removalRequirement;

        [Header("=== Effect Tags ===")]
        [SerializeField] protected TagContainer grantedTags;
        [SerializeField] protected TagContainer assetTags;

        [Header("=== Modifiers ===")]
        [SerializeField] protected List<ModifierConfig> modifiers = new List<ModifierConfig>();

        [Header("=== Visual & Audio ===")]
        [SerializeField] protected GameObject effectPrefab;
        [SerializeField] protected AudioClip applicationSound;
        [SerializeField] protected AudioClip removalSound;

        #endregion

        #region Properties

        public string EffectId => effectId;
        public string EffectName => effectName;
        public string Description => description;
        public EffectType Type => effectType;
        public EffectDurationPolicy DurationPolicy => durationPolicy;
        public float Duration => duration;
        public float Period => period;
        public EffectStackingPolicy StackingPolicy => stackingPolicy;
        public int MaxStackCount => maxStackCount;
        public TagRequirement ApplicationRequirement => applicationRequirement;
        public TagRequirement OngoingRequirement => ongoingRequirement;
        public TagContainer GrantedTags => grantedTags;
        public TagContainer AssetTags => assetTags;
        public List<ModifierConfig> Modifiers => modifiers;
        public GameObject EffectPrefab => effectPrefab;

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Effect를 대상에 적용
        /// </summary>
        /// <param name="context">Effect 실행 컨텍스트</param>
        /// <param name="target">적용 대상</param>
        /// <returns>적용 성공 여부</returns>
        public abstract bool Apply(EffectContext context, GameObject target);

        /// <summary>
        /// Effect를 대상에서 제거
        /// </summary>
        /// <param name="context">Effect 실행 컨텍스트</param>
        /// <param name="target">제거 대상</param>
        /// <returns>제거 성공 여부</returns>
        public abstract bool Remove(EffectContext context, GameObject target);

        /// <summary>
        /// 주기적 Effect 실행 (Periodic 타입용)
        /// </summary>
        /// <param name="context">Effect 실행 컨텍스트</param>
        /// <param name="target">실행 대상</param>
        /// <returns>실행 성공 여부</returns>
        public abstract bool ExecutePeriodic(EffectContext context, GameObject target);

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Effect 적용 가능 여부 검사
        /// </summary>
        public virtual bool CanApply(EffectContext context, GameObject target)
        {
            if (target == null) return false;

            // Tag requirement 검사
            if (applicationRequirement != null)
            {
                var tagComponent = target.GetComponent<TagComponent>();
                if (tagComponent != null && !tagComponent.SatisfiesRequirement(applicationRequirement))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Effect 지속 가능 여부 검사 (Duration/Infinite 타입용)
        /// </summary>
        public virtual bool CanContinue(EffectContext context, GameObject target)
        {
            if (target == null) return false;

            // Ongoing requirement 검사
            if (ongoingRequirement != null)
            {
                var tagComponent = target.GetComponent<TagComponent>();
                if (tagComponent != null && !tagComponent.SatisfiesRequirement(ongoingRequirement))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Effect 스택 가능 여부 검사
        /// </summary>
        public virtual bool CanStack(GameplayEffect existingEffect, EffectContext context)
        {
            if (stackingPolicy == EffectStackingPolicy.None)
                return false;

            if (existingEffect == null || existingEffect.effectId != effectId)
                return false;

            return true;
        }

        /// <summary>
        /// Modifier 생성
        /// </summary>
        protected virtual List<AttributeModifier> CreateModifiers(EffectContext context, float magnitude = 1f)
        {
            var createdModifiers = new List<AttributeModifier>();

            foreach (var config in modifiers)
            {
                if (config == null) continue;

                var modifier = new AttributeModifier(
                    config.operation,
                    config.value * magnitude,
                    config.priority
                );

                createdModifiers.Add(modifier);
            }

            return createdModifiers;
        }

        /// <summary>
        /// 태그 부여
        /// </summary>
        protected virtual void GrantTags(GameObject target)
        {
            if (grantedTags == null || grantedTags.IsEmpty) return;

            var tagComponent = target.GetComponent<TagComponent>();
            if (tagComponent != null)
            {
                foreach (var tag in grantedTags.Tags)
                {
                    tagComponent.AddTag(tag);
                }
            }
        }

        /// <summary>
        /// 부여된 태그 제거
        /// </summary>
        protected virtual void RemoveGrantedTags(GameObject target)
        {
            if (grantedTags == null || grantedTags.IsEmpty) return;

            var tagComponent = target.GetComponent<TagComponent>();
            if (tagComponent != null)
            {
                foreach (var tag in grantedTags.Tags)
                {
                    tagComponent.RemoveTag(tag);
                }
            }
        }

        /// <summary>
        /// Effect 시각 효과 생성
        /// </summary>
        protected virtual GameObject CreateVisualEffect(GameObject target)
        {
            if (effectPrefab == null) return null;

            return Instantiate(effectPrefab, target.transform.position, Quaternion.identity, target.transform);
        }

        /// <summary>
        /// 사운드 재생
        /// </summary>
        protected virtual void PlaySound(AudioClip clip, Vector3 position)
        {
            if (clip == null) return;

            // AudioSource를 통한 사운드 재생 (구현 필요)
            // AudioSource.PlayClipAtPoint(clip, position);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Effect ID 생성 (GUID)
        /// </summary>
        protected virtual void GenerateEffectId()
        {
            if (string.IsNullOrEmpty(effectId))
            {
                effectId = Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Effect 정보 문자열 반환
        /// </summary>
        public override string ToString()
        {
            return $"[{effectType}] {effectName} (ID: {effectId})";
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            // 자동 ID 생성
            GenerateEffectId();

            // Duration 설정 검증
            if (durationPolicy == EffectDurationPolicy.Instant)
            {
                duration = 0f;
            }
            else if (duration < 0f)
            {
                duration = 0f;
            }

            // Period 검증
            if (effectType == EffectType.Periodic && period <= 0f)
            {
                period = 1f;
            }

            // Stack 설정 검증
            if (maxStackCount < 1)
            {
                maxStackCount = 1;
            }
        }
#endif

        #endregion
    }

    /// <summary>
    /// Modifier 설정 구조체
    /// </summary>
    [Serializable]
    public class ModifierConfig
    {
        [Header("Modifier Settings")]
        public AttributeType attributeType;
        public ModifierOperation operation = ModifierOperation.Add;
        public float value;
        public ModifierPriority priority = ModifierPriority.Normal;
        public ModifierApplicationTiming timing = ModifierApplicationTiming.OnApplication;

        [Header("Scaling")]
        public bool useContextScaling = false;
        public float scalingFactor = 1f;

        public ModifierConfig()
        {
            operation = ModifierOperation.Add;
            priority = 0;
            timing = ModifierApplicationTiming.OnApplication;
            scalingFactor = 1f;
        }

        public ModifierConfig(AttributeType type, ModifierOperation op, float val, ModifierPriority prio = 0)
        {
            attributeType = type;
            operation = op;
            value = val;
            priority = prio;
            timing = ModifierApplicationTiming.OnApplication;
            scalingFactor = 1f;
        }
    }
}