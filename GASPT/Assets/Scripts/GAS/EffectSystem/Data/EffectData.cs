using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.Core;
using GAS.AttributeSystem;
using GAS.TagSystem;

namespace GAS.EffectSystem
{
    /// <summary>
    /// GameplayEffect의 데이터를 정의하는 ScriptableObject
    /// Inspector에서 Effect를 쉽게 설정할 수 있도록 지원
    /// </summary>
    [CreateAssetMenu(fileName = "NewEffectData", menuName = "GAS/Effect Data", order = 1)]
    public class EffectData : ScriptableObject
    {
        #region Nested Types

        /// <summary>
        /// 조건부 Effect 적용을 위한 규칙
        /// </summary>
        [Serializable]
        public class ConditionalEffect
        {
            [Header("Condition")]
            public TagRequirement condition;

            [Header("Effects to Apply")]
            public List<EffectData> effectsToApply = new List<EffectData>();

            [Header("Chance")]
            [Range(0f, 1f)]
            public float applyChance = 1f;
        }

        /// <summary>
        /// Modifier의 확장 설정
        /// </summary>
        [Serializable]
        public class ExtendedModifierConfig : ModifierConfig
        {
            [Header("Curve Settings")]
            public bool useCurve = false;
            public AnimationCurve valueCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

            [Header("Stack Scaling")]
            public bool scaleWithStack = false;
            public float stackScalingFactor = 1f;

            [Header("Conditional")]
            public bool hasCondition = false;
            public TagRequirement applyCondition;
        }

        /// <summary>
        /// 시각/청각 효과 설정
        /// </summary>
        [Serializable]
        public class EffectPresentation
        {
            [Header("Visual Effects")]
            public GameObject effectPrefab;
            public Vector3 effectOffset = Vector3.zero;
            public bool attachToTarget = true;
            public float effectScale = 1f;

            [Header("Audio")]
            public AudioClip applicationSound;
            public AudioClip loopSound;
            public AudioClip removalSound;
            [Range(0f, 1f)]
            public float soundVolume = 1f;

            [Header("UI")]
            public Sprite effectIcon;
            public Color effectColor = Color.white;
            public string displayName;
            [TextArea(2, 3)]
            public string displayDescription;

            [Header("Animation")]
            public string animationTrigger;
            public bool loopAnimation = false;
        }

        #endregion

        #region Core Settings

        [Header("=== Effect Identity ===")]
        [SerializeField] private string effectId;
        [SerializeField] private string effectName;
        [TextArea(3, 5)]
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [Header("=== Effect Classification ===")]
        [SerializeField] private EffectType effectType = EffectType.Instant;
        [SerializeField] private EffectDurationPolicy durationPolicy = EffectDurationPolicy.Instant;
        [SerializeField] private List<GameplayTag> effectTags = new List<GameplayTag>();

        #endregion

        #region Duration & Timing

        [Header("=== Duration & Timing ===")]
        [SerializeField] private float baseDuration = 1f;
        [SerializeField] private bool canRefreshDuration = true;
        [SerializeField] private AnimationCurve durationScaling = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Header("Periodic Settings")]
        [SerializeField] private float period = 1f;
        [SerializeField] private int maxPeriodicTicks = -1;
        [SerializeField] private bool executeOnApplication = true;
        [SerializeField] private bool executeOnExpiration = false;

        #endregion

        #region Stacking

        [Header("=== Stack Configuration ===")]
        [SerializeField] private EffectStackingPolicy stackingPolicy = EffectStackingPolicy.None;
        [SerializeField] private int maxStackCount = 1;
        [SerializeField] private bool refreshDurationOnStack = true;
        [SerializeField] private bool resetPeriodicOnStack = false;
        [SerializeField] private bool independentStackDuration = false;

        #endregion

        #region Requirements & Immunity

        [Header("=== Requirements & Immunity ===")]
        [SerializeField] private TagRequirement applicationRequirement;
        [SerializeField] private TagRequirement ongoingRequirement;
        [SerializeField] private TagRequirement removalRequirement;

        [Header("Immunity Tags")]
        [SerializeField] private List<GameplayTag> immunityTags = new List<GameplayTag>();
        [SerializeField] private List<GameplayTag> immunityGrantingTags = new List<GameplayTag>();

        #endregion

        #region Tags

        [Header("=== Tag Management ===")]
        [SerializeField] private TagContainer grantedTags;
        [SerializeField] private TagContainer requiredSourceTags;
        [SerializeField] private TagContainer blockedSourceTags;

        #endregion

        #region Modifiers

        [Header("=== Attribute Modifiers ===")]
        [SerializeField] private List<ExtendedModifierConfig> modifiers = new List<ExtendedModifierConfig>();
        [SerializeField] private bool removeModifiersOnExpiration = true;

        #endregion

        #region Conditional Effects

        [Header("=== Conditional Effects ===")]
        [SerializeField] private List<ConditionalEffect> conditionalEffects = new List<ConditionalEffect>();
        [SerializeField] private List<EffectData> onApplicationEffects = new List<EffectData>();
        [SerializeField] private List<EffectData> onExpirationEffects = new List<EffectData>();
        [SerializeField] private List<EffectData> onStackEffects = new List<EffectData>();

        #endregion

        #region Presentation

        [Header("=== Presentation ===")]
        [SerializeField] private EffectPresentation presentation = new EffectPresentation();

        #endregion

        #region Advanced Settings

        [Header("=== Advanced Settings ===")]
        [SerializeField] private bool canBePurged = true;
        [SerializeField] private bool canBeBlocked = true;
        [SerializeField] private int priority = 0;
        [SerializeField] private float magnitudeScaling = 1f;

        [Header("Network")]
        [SerializeField] private bool replicateToClients = true;
        [SerializeField] private bool predictOnClient = false;

        #endregion

        #region Properties

        public string EffectId => effectId;
        public string EffectName => effectName;
        public string Description => description;
        public Sprite Icon => icon;
        public EffectType Type => effectType;
        public EffectDurationPolicy DurationPolicy => durationPolicy;
        public float BaseDuration => baseDuration;
        public float Period => period;
        public EffectStackingPolicy StackingPolicy => stackingPolicy;
        public int MaxStackCount => maxStackCount;
        public TagRequirement ApplicationRequirement => applicationRequirement;
        public TagRequirement OngoingRequirement => ongoingRequirement;
        public TagContainer GrantedTags => grantedTags;
        public List<ExtendedModifierConfig> Modifiers => modifiers;
        public EffectPresentation Presentation => presentation;
        public int Priority => priority;
        public bool CanBePurged => canBePurged;
        public bool CanBeBlocked => canBeBlocked;
        public List<GameplayTag> EffectTags => effectTags;
        public List<ConditionalEffect> ConditionalEffects => conditionalEffects;

        #endregion

        #region Public Methods

        /// <summary>
        /// GameplayEffect 인스턴스 생성
        /// </summary>
        public GameplayEffect CreateEffect()
        {
            // 타입에 따라 적절한 Effect 생성 (Phase 2 후속 구현)
            switch (effectType)
            {
                case EffectType.Instant:
                    return CreateInstantEffect();
                case EffectType.Duration:
                    return CreateDurationEffect();
                case EffectType.Periodic:
                    return CreatePeriodicEffect();
                case EffectType.Infinite:
                    return CreateInfiniteEffect();
                default:
                    Debug.LogError($"Unknown effect type: {effectType}");
                    return null;
            }
        }

        /// <summary>
        /// 대상이 이 Effect에 면역인지 확인
        /// </summary>
        public bool IsTargetImmune(GameObject target)
        {
            if (target == null || immunityTags.Count == 0) return false;

            var tagComponent = target.GetComponent<TagComponent>();
            if (tagComponent == null) return false;

            foreach (var immunityTag in immunityTags)
            {
                if (tagComponent.HasTag(immunityTag))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Source가 이 Effect를 적용할 수 있는지 확인
        /// </summary>
        public bool CanSourceApply(GameObject source)
        {
            if (source == null) return true;

            var tagComponent = source.GetComponent<TagComponent>();
            if (tagComponent == null) return true;

            // Required source tags 확인
            if (requiredSourceTags != null && !requiredSourceTags.IsEmpty)
            {
                foreach (var tag in requiredSourceTags.Tags)
                {
                    if (!tagComponent.HasTag(tag))
                        return false;
                }
            }

            // Blocked source tags 확인
            if (blockedSourceTags != null && !blockedSourceTags.IsEmpty)
            {
                foreach (var tag in blockedSourceTags.Tags)
                {
                    if (tagComponent.HasTag(tag))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Duration 계산 (scaling 적용)
        /// </summary>
        public float CalculateDuration(float scaleFactor = 1f)
        {
            if (durationPolicy == EffectDurationPolicy.Instant) return 0f;
            if (durationPolicy == EffectDurationPolicy.Infinite) return float.MaxValue;

            float scaled = baseDuration * scaleFactor;
            if (durationScaling != null && durationScaling.length > 0)
            {
                scaled *= durationScaling.Evaluate(scaleFactor);
            }

            return Mathf.Max(0f, scaled);
        }

        /// <summary>
        /// Modifier 값 계산
        /// </summary>
        public float CalculateModifierValue(ExtendedModifierConfig config, float magnitude, int stackCount = 1)
        {
            float value = config.value * magnitude;

            // Curve 적용
            if (config.useCurve && config.valueCurve != null)
            {
                value *= config.valueCurve.Evaluate(magnitude);
            }

            // Stack scaling 적용
            if (config.scaleWithStack)
            {
                value *= 1f + (config.stackScalingFactor * (stackCount - 1));
            }

            return value;
        }

        #endregion

        #region Private Methods

        private GameplayEffect CreateInstantEffect()
        {
            // InstantEffect 생성 로직 (후속 구현)
            Debug.Log($"Creating InstantEffect from {effectName}");
            return null;
        }

        private GameplayEffect CreateDurationEffect()
        {
            // DurationEffect 생성 로직 (후속 구현)
            Debug.Log($"Creating DurationEffect from {effectName}");
            return null;
        }

        private GameplayEffect CreatePeriodicEffect()
        {
            // PeriodicEffect 생성 로직 (후속 구현)
            Debug.Log($"Creating PeriodicEffect from {effectName}");
            return null;
        }

        private GameplayEffect CreateInfiniteEffect()
        {
            // InfiniteEffect 생성 로직 (후속 구현)
            Debug.Log($"Creating InfiniteEffect from {effectName}");
            return null;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 자동 ID 생성
            if (string.IsNullOrEmpty(effectId))
            {
                effectId = Guid.NewGuid().ToString();
            }

            // 이름 자동 설정
            if (string.IsNullOrEmpty(effectName))
            {
                effectName = name;
            }

            // Duration 검증
            if (baseDuration < 0f) baseDuration = 0f;
            if (period <= 0f) period = 1f;

            // Stack 검증
            if (maxStackCount < 1) maxStackCount = 1;

            // Type에 따른 설정 조정
            switch (effectType)
            {
                case EffectType.Instant:
                    durationPolicy = EffectDurationPolicy.Instant;
                    baseDuration = 0f;
                    break;
                case EffectType.Duration:
                    durationPolicy = EffectDurationPolicy.HasDuration;
                    if (baseDuration <= 0f) baseDuration = 1f;
                    break;
                case EffectType.Periodic:
                    durationPolicy = EffectDurationPolicy.HasDuration;
                    if (baseDuration <= 0f) baseDuration = period * 3f;
                    break;
                case EffectType.Infinite:
                    durationPolicy = EffectDurationPolicy.Infinite;
                    break;
            }
        }

        private void Reset()
        {
            effectId = Guid.NewGuid().ToString();
            effectName = name;
            effectTags = new List<GameplayTag>();
            modifiers = new List<ExtendedModifierConfig>();
            conditionalEffects = new List<ConditionalEffect>();
            presentation = new EffectPresentation();
            grantedTags = new TagContainer();
        }
#endif

        #endregion
    }
}

// 파일 위치: Assets/Scripts/GAS/EffectSystem/Data/EffectData.cs