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
    /// GameplayEffect�� Ÿ���� ����
    /// </summary>
    public enum EffectType
    {
        Instant,    // ��� ���� �� ����
        Duration,   // ���� �ð� ����
        Periodic,   // �ֱ������� ƽ �߻�
        Infinite    // ���� ���ű��� ���� ����
    }

    /// <summary>
    /// Effect�� ���� ��å�� ����
    /// </summary>
    public enum EffectStackingPolicy
    {
        None,               // ���� �Ұ� (���� Effect ����)
        Override,           // ���� Effect�� �� Effect�� �����
        Stack,              // ���� Effect�� ���� (���� ���������� ����)
        AggregateBySource,  // ���� source�� Effect�� �ϳ��� ��ħ
        AggregateByTarget   // ���� target�� Effect�� �ϳ��� ��ħ
    }

    /// <summary>
    /// Effect�� ���ӽð� ��å�� ����
    /// </summary>
    public enum EffectDurationPolicy
    {
        Instant,        // ��� ����
        HasDuration,    // ���� �ð� ����
        Infinite        // ���� ����
    }

    /// <summary>
    /// Modifier ���� ������ ����
    /// </summary>
    public enum ModifierApplicationTiming
    {
        OnApplication,  // Effect ���� ����
        OnPeriodic,     // �ֱ��� ƽ ����
        OnRemoval,      // Effect ���� ����
        OnStack         // ���� ���� ����
    }

    /// <summary>
    /// GameplayEffect�� ���̽� Ŭ����
    /// ��� Effect Ÿ���� ����� �Ǵ� �߻� Ŭ����
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
        /// Effect�� ��� ����
        /// </summary>
        /// <param name="context">Effect ���� ���ؽ�Ʈ</param>
        /// <param name="target">���� ���</param>
        /// <returns>���� ���� ����</returns>
        public abstract bool Apply(EffectContext context, GameObject target);

        /// <summary>
        /// Effect�� ��󿡼� ����
        /// </summary>
        /// <param name="context">Effect ���� ���ؽ�Ʈ</param>
        /// <param name="target">���� ���</param>
        /// <returns>���� ���� ����</returns>
        public abstract bool Remove(EffectContext context, GameObject target);

        /// <summary>
        /// �ֱ��� Effect ���� (Periodic Ÿ�Կ�)
        /// </summary>
        /// <param name="context">Effect ���� ���ؽ�Ʈ</param>
        /// <param name="target">���� ���</param>
        /// <returns>���� ���� ����</returns>
        public abstract bool ExecutePeriodic(EffectContext context, GameObject target);

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Effect ���� ���� ���� �˻�
        /// </summary>
        public virtual bool CanApply(EffectContext context, GameObject target)
        {
            if (target == null) return false;

            // Tag requirement �˻�
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
        /// Effect ���� ���� ���� �˻� (Duration/Infinite Ÿ�Կ�)
        /// </summary>
        public virtual bool CanContinue(EffectContext context, GameObject target)
        {
            if (target == null) return false;

            // Ongoing requirement �˻�
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
        /// Effect ���� ���� ���� �˻�
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
        /// Modifier ����
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
        /// �±� �ο�
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
        /// �ο��� �±� ����
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
        /// Effect �ð� ȿ�� ����
        /// </summary>
        protected virtual GameObject CreateVisualEffect(GameObject target)
        {
            if (effectPrefab == null) return null;

            return Instantiate(effectPrefab, target.transform.position, Quaternion.identity, target.transform);
        }

        /// <summary>
        /// ���� ���
        /// </summary>
        protected virtual void PlaySound(AudioClip clip, Vector3 position)
        {
            if (clip == null) return;

            // AudioSource�� ���� ���� ��� (���� �ʿ�)
            // AudioSource.PlayClipAtPoint(clip, position);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Effect ID ���� (GUID)
        /// </summary>
        protected virtual void GenerateEffectId()
        {
            if (string.IsNullOrEmpty(effectId))
            {
                effectId = Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Effect ���� ���ڿ� ��ȯ
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
            // �ڵ� ID ����
            GenerateEffectId();

            // Duration ���� ����
            if (durationPolicy == EffectDurationPolicy.Instant)
            {
                duration = 0f;
            }
            else if (duration < 0f)
            {
                duration = 0f;
            }

            // Period ����
            if (effectType == EffectType.Periodic && period <= 0f)
            {
                period = 1f;
            }

            // Stack ���� ����
            if (maxStackCount < 1)
            {
                maxStackCount = 1;
            }
        }
#endif

        #endregion
    }

    /// <summary>
    /// Modifier ���� ����ü
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