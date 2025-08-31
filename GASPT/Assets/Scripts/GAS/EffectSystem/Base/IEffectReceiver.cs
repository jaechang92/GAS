using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.TagSystem;

namespace GAS.EffectSystem
{
    /// <summary>
    /// GameplayEffect�� �����ϰ� ó���� �� �ִ� ��ü�� �����ϴ� �������̽�
    /// </summary>
    public interface IEffectReceiver
    {
        /// <summary>
        /// Ȱ��ȭ�� Effect �ν��Ͻ� ���
        /// </summary>
        IReadOnlyList<EffectInstance> ActiveEffects { get; }

        /// <summary>
        /// Effect ���� ���� ���� Ȯ��
        /// </summary>
        /// <param name="effect">Ȯ���� Effect</param>
        /// <param name="context">Effect ���ؽ�Ʈ</param>
        /// <returns>���� ���� ����</returns>
        bool CanReceiveEffect(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Effect ����
        /// </summary>
        /// <param name="effect">������ Effect</param>
        /// <param name="context">Effect ���ؽ�Ʈ</param>
        /// <returns>������ Effect �ν��Ͻ� (���н� null)</returns>
        EffectInstance ApplyEffect(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Effect ����
        /// </summary>
        /// <param name="instance">������ Effect �ν��Ͻ�</param>
        /// <returns>���� ���� ����</returns>
        bool RemoveEffect(EffectInstance instance);

        /// <summary>
        /// Effect ID�� ����
        /// </summary>
        /// <param name="effectId">������ Effect ID</param>
        /// <returns>���ŵ� Effect ����</returns>
        int RemoveEffectById(string effectId);

        /// <summary>
        /// Ư�� Source�� ��� Effect ����
        /// </summary>
        /// <param name="source">Effect source</param>
        /// <returns>���ŵ� Effect ����</returns>
        int RemoveEffectsFromSource(object source);

        /// <summary>
        /// ��� Effect ����
        /// </summary>
        void RemoveAllEffects();

        /// <summary>
        /// Ư�� ID�� Ȱ�� Effect �ν��Ͻ� ��������
        /// </summary>
        /// <param name="effectId">ã�� Effect ID</param>
        /// <returns>Effect �ν��Ͻ� ���</returns>
        List<EffectInstance> GetActiveEffectsByID(string effectId);

        /// <summary>
        /// Ư�� �±׸� ���� Effect �ν��Ͻ� ��������
        /// </summary>
        /// <param name="tag">ã�� �±�</param>
        /// <returns>Effect �ν��Ͻ� ���</returns>
        List<EffectInstance> GetActiveEffectsByTag(GameplayTag tag);

        /// <summary>
        /// Effect ���� �� ��������
        /// </summary>
        /// <param name="effectId">Effect ID</param>
        /// <returns>���� ��</returns>
        int GetEffectStackCount(string effectId);

        /// <summary>
        /// Effect ���� �� �߻��ϴ� �̺�Ʈ
        /// </summary>
        event Action<EffectInstance> OnEffectApplied;

        /// <summary>
        /// Effect ���� �� �߻��ϴ� �̺�Ʈ
        /// </summary>
        event Action<EffectInstance> OnEffectRemoved;

        /// <summary>
        /// Effect ���� ���� �� �߻��ϴ� �̺�Ʈ
        /// </summary>
        event Action<EffectInstance, int> OnEffectStackChanged;

        /// <summary>
        /// Effect ���� �� �߻��ϴ� �̺�Ʈ
        /// </summary>
        event Action<EffectInstance> OnEffectExpired;
    }

    /// <summary>
    /// Ȱ��ȭ�� GameplayEffect�� ��Ÿ�� �ν��Ͻ�
    /// </summary>
    public class EffectInstance
    {
        #region Fields

        private readonly string instanceId;
        private readonly GameplayEffect sourceEffect;
        private readonly EffectContext context;
        private readonly float startTime;
        private float remainingDuration;
        private float nextPeriodicTime;
        private int currentStack;
        private int periodicExecutionCount;
        private bool isExpired;
        private List<Guid> appliedModifierIds;
        private GameObject visualEffect;

        #endregion

        #region Properties

        /// <summary>
        /// �ν��Ͻ� ���� ID
        /// </summary>
        public string InstanceId => instanceId;

        /// <summary>
        /// ���� Effect
        /// </summary>
        public GameplayEffect SourceEffect => sourceEffect;

        /// <summary>
        /// Effect ���ؽ�Ʈ
        /// </summary>
        public EffectContext Context => context;

        /// <summary>
        /// ���� �ð�
        /// </summary>
        public float StartTime => startTime;

        /// <summary>
        /// ���� ���� �ð�
        /// </summary>
        public float RemainingDuration
        {
            get => remainingDuration;
            set => remainingDuration = Mathf.Max(0f, value);
        }

        /// <summary>
        /// ���� �ֱ� ���� �ð�
        /// </summary>
        public float NextPeriodicTime
        {
            get => nextPeriodicTime;
            set => nextPeriodicTime = value;
        }

        /// <summary>
        /// ���� ���� ��
        /// </summary>
        public int CurrentStack
        {
            get => currentStack;
            set => currentStack = Mathf.Max(1, value);
        }

        /// <summary>
        /// �ֱ��� ���� Ƚ��
        /// </summary>
        public int PeriodicExecutionCount => periodicExecutionCount;

        /// <summary>
        /// ���� ����
        /// </summary>
        public bool IsExpired
        {
            get => isExpired;
            set => isExpired = value;
        }

        /// <summary>
        /// ���� ���� ����
        /// </summary>
        public bool IsInfinite => sourceEffect.DurationPolicy == EffectDurationPolicy.Infinite;

        /// <summary>
        /// �ֱ��� Effect ����
        /// </summary>
        public bool IsPeriodic => sourceEffect.Type == EffectType.Periodic;

        /// <summary>
        /// ����� Modifier ID ���
        /// </summary>
        public List<Guid> AppliedModifierIds => appliedModifierIds;

        /// <summary>
        /// �ð� ȿ�� GameObject
        /// </summary>
        public GameObject VisualEffect
        {
            get => visualEffect;
            set => visualEffect = value;
        }

        /// <summary>
        /// ��� �ð�
        /// </summary>
        public float ElapsedTime => Time.time - startTime;

        /// <summary>
        /// ����� (0~1)
        /// </summary>
        public float Progress
        {
            get
            {
                if (IsInfinite) return 0f;
                if (sourceEffect.Duration <= 0f) return 1f;
                return 1f - (remainingDuration / sourceEffect.Duration);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// EffectInstance ������
        /// </summary>
        public EffectInstance(GameplayEffect effect, EffectContext context)
        {
            instanceId = Guid.NewGuid().ToString();
            sourceEffect = effect;
            this.context = context.Clone();
            startTime = Time.time;

            // Duration ����
            if (effect.DurationPolicy == EffectDurationPolicy.HasDuration)
            {
                remainingDuration = effect.Duration;
            }
            else if (effect.DurationPolicy == EffectDurationPolicy.Infinite)
            {
                remainingDuration = float.MaxValue;
            }
            else
            {
                remainingDuration = 0f;
            }

            // Periodic ����
            if (effect.Type == EffectType.Periodic)
            {
                nextPeriodicTime = startTime + effect.Period;
            }

            currentStack = 1;
            periodicExecutionCount = 0;
            isExpired = false;
            appliedModifierIds = new List<Guid>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// �ֱ� ���� ī��Ʈ ����
        /// </summary>
        public void IncrementPeriodicCount()
        {
            periodicExecutionCount++;
        }

        /// <summary>
        /// ���� �߰�
        /// </summary>
        public void AddStack(int amount = 1)
        {
            currentStack += amount;
            currentStack = Mathf.Min(currentStack, sourceEffect.MaxStackCount);
        }

        /// <summary>
        /// Duration ����
        /// </summary>
        public void RefreshDuration()
        {
            if (sourceEffect.DurationPolicy == EffectDurationPolicy.HasDuration)
            {
                remainingDuration = sourceEffect.Duration;
            }
        }

        /// <summary>
        /// Periodic Ÿ�̸� ����
        /// </summary>
        public void ResetPeriodicTimer()
        {
            if (sourceEffect.Type == EffectType.Periodic)
            {
                nextPeriodicTime = Time.time + sourceEffect.Period;
            }
        }

        /// <summary>
        /// Modifier ID �߰�
        /// </summary>
        public void AddModifierId(Guid modifierId)
        {
            if (!appliedModifierIds.Contains(modifierId))
            {
                appliedModifierIds.Add(modifierId);
            }
        }

        /// <summary>
        /// Modifier ID ����
        /// </summary>
        public bool RemoveModifierId(Guid modifierId)
        {
            return appliedModifierIds.Remove(modifierId);
        }

        /// <summary>
        /// ��� Modifier ID ����
        /// </summary>
        public void ClearModifierIds()
        {
            appliedModifierIds.Clear();
        }

        /// <summary>
        /// �ν��Ͻ� ���� ���ڿ�
        /// </summary>
        public override string ToString()
        {
            return $"EffectInstance [{sourceEffect.EffectName}] " +
                   $"Stack: {currentStack}, " +
                   $"Duration: {remainingDuration:F1}s, " +
                   $"Expired: {isExpired}";
        }

        #endregion
    }
}

// ���� ��ġ: Assets/Scripts/GAS/EffectSystem/Base/IEffectReceiver.cs
// ���� IEffectComponent.cs ������ �� �������� ��ü