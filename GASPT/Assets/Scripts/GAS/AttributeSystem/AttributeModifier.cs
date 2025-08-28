// ���� ��ġ: Assets/Scripts/GAS/AttributeSystem/AttributeModifier.cs
using System;
using UnityEngine;
using GAS.Core;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// �Ӽ��� �����ϴ� ������ Ŭ����
    /// </summary>
    [Serializable]
    public class AttributeModifier
    {
        #region Serialized Fields
        [SerializeField] private string modifierName;
        [SerializeField] private GASConstants.AttributeType targetAttribute;
        [SerializeField] private GASConstants.ModifierOperation operation;
        [SerializeField] private float value;
        [SerializeField] private GASConstants.ModifierPriority priority;
        [SerializeField] private bool isTemporary;
        [SerializeField] private float duration;
        #endregion

        #region Private Fields
        private object source;
        private float timeRemaining;
        private bool isActive = true;
        private Guid uniqueId;
        #endregion

        #region Properties
        /// <summary>
        /// ������ �̸�
        /// </summary>
        public string ModifierName
        {
            get => modifierName;
            set => modifierName = value;
        }

        /// <summary>
        /// ��� �Ӽ� Ÿ��
        /// </summary>
        public GASConstants.AttributeType TargetAttribute
        {
            get => targetAttribute;
            set => targetAttribute = value;
        }

        /// <summary>
        /// ���� ���� Ÿ��
        /// </summary>
        public GASConstants.ModifierOperation Operation
        {
            get => operation;
            set => operation = value;
        }

        /// <summary>
        /// ������
        /// </summary>
        public float Value
        {
            get => value;
            set => this.value = value;
        }

        /// <summary>
        /// ���� �켱����
        /// </summary>
        public GASConstants.ModifierPriority Priority
        {
            get => priority;
            set => priority = value;
        }

        /// <summary>
        /// �켱���� ���ڰ�
        /// </summary>
        public int PriorityValue => (int)priority;

        /// <summary>
        /// �ӽ� ������ ����
        /// </summary>
        public bool IsTemporary
        {
            get => isTemporary;
            set => isTemporary = value;
        }

        /// <summary>
        /// ���� �ð�
        /// </summary>
        public float Duration
        {
            get => duration;
            set
            {
                duration = value;
                timeRemaining = value;
            }
        }

        /// <summary>
        /// ���� �ð�
        /// </summary>
        public float TimeRemaining
        {
            get => timeRemaining;
            set => timeRemaining = value;
        }

        /// <summary>
        /// ������ �ҽ� (ȿ��, ��� ��)
        /// </summary>
        public object Source
        {
            get => source;
            set => source = value;
        }

        /// <summary>
        /// Ȱ�� ����
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        /// <summary>
        /// ���� ����
        /// </summary>
        public bool IsExpired => isTemporary && timeRemaining <= 0;

        /// <summary>
        /// ���� ID
        /// </summary>
        public Guid UniqueId => uniqueId;

        /// <summary>
        /// ���� �ð� ���� (0~1)
        /// </summary>
        public float TimeRemainingRatio
        {
            get
            {
                if (!isTemporary || duration <= 0) return 1f;
                return Mathf.Clamp01(timeRemaining / duration);
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// �⺻ ������
        /// </summary>
        public AttributeModifier()
        {
            modifierName = "New Modifier";
            operation = GASConstants.ModifierOperation.Add;
            priority = GASConstants.ModifierPriority.Normal;
            uniqueId = Guid.NewGuid();
        }

        /// <summary>
        /// ���� ������ ����
        /// </summary>
        public AttributeModifier(string name, GASConstants.AttributeType target,
            GASConstants.ModifierOperation op, float val)
        {
            modifierName = name;
            targetAttribute = target;
            operation = op;
            value = val;
            priority = GASConstants.ModifierPriority.Normal;
            isTemporary = false;
            uniqueId = Guid.NewGuid();
        }

        /// <summary>
        /// �ӽ� ������ ����
        /// </summary>
        public AttributeModifier(string name, GASConstants.AttributeType target,
            GASConstants.ModifierOperation op, float val, float dur)
        {
            modifierName = name;
            targetAttribute = target;
            operation = op;
            value = val;
            priority = GASConstants.ModifierPriority.Normal;
            isTemporary = true;
            duration = dur;
            timeRemaining = dur;
            uniqueId = Guid.NewGuid();
        }

        /// <summary>
        /// �켱���� ���� ������
        /// </summary>
        public AttributeModifier(string name, GASConstants.AttributeType target,
            GASConstants.ModifierOperation op, float val, GASConstants.ModifierPriority prio)
        {
            modifierName = name;
            targetAttribute = target;
            operation = op;
            value = val;
            priority = prio;
            isTemporary = false;
            uniqueId = Guid.NewGuid();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// �ð� ������Ʈ (�ӽ� �����ڿ�)
        /// </summary>
        public void UpdateTime(float deltaTime)
        {
            if (!isTemporary || !isActive) return;

            timeRemaining -= deltaTime;

            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                isActive = false;
            }
        }

        /// <summary>
        /// ���ӽð� ����
        /// </summary>
        public void RefreshDuration()
        {
            if (isTemporary)
            {
                timeRemaining = duration;
                isActive = true;
            }
        }

        /// <summary>
        /// ���ӽð� ����
        /// </summary>
        public void ExtendDuration(float additionalTime)
        {
            if (isTemporary)
            {
                timeRemaining += additionalTime;
                isActive = true;
            }
        }

        /// <summary>
        /// ������ Ȱ��ȭ
        /// </summary>
        public void Activate()
        {
            isActive = true;
        }

        /// <summary>
        /// ������ ��Ȱ��ȭ
        /// </summary>
        public void Deactivate()
        {
            isActive = false;
        }

        /// <summary>
        /// ������ ����
        /// </summary>
        public AttributeModifier Clone()
        {
            return new AttributeModifier
            {
                modifierName = modifierName,
                targetAttribute = targetAttribute,
                operation = operation,
                value = value,
                priority = priority,
                isTemporary = isTemporary,
                duration = duration,
                timeRemaining = timeRemaining,
                source = source,
                isActive = isActive,
                uniqueId = Guid.NewGuid() // ���ο� ID ����
            };
        }

        /// <summary>
        /// ���� �� �������� (�⺻���� ����)
        /// </summary>
        public float CalculateValue(float baseValue)
        {
            if (!isActive) return baseValue;

            switch (operation)
            {
                case GASConstants.ModifierOperation.Add:
                    return baseValue + value;

                case GASConstants.ModifierOperation.Subtract:
                    return baseValue - value;

                case GASConstants.ModifierOperation.Multiply:
                    return baseValue * value;

                case GASConstants.ModifierOperation.Divide:
                    return value != 0 ? baseValue / value : baseValue;

                case GASConstants.ModifierOperation.Override:
                    return value;

                case GASConstants.ModifierOperation.AddPercent:
                    return baseValue * (1f + value / 100f);

                case GASConstants.ModifierOperation.SubtractPercent:
                    return baseValue * (1f - value / 100f);

                default:
                    return baseValue;
            }
        }

        /// <summary>
        /// ������ ���������� Ȯ��
        /// </summary>
        public bool Equals(AttributeModifier other)
        {
            if (other == null) return false;
            return uniqueId == other.uniqueId;
        }

        /// <summary>
        /// ���� �������� Ȯ��
        /// </summary>
        public bool CanStackWith(AttributeModifier other)
        {
            if (other == null) return false;

            return modifierName == other.modifierName &&
                   targetAttribute == other.targetAttribute &&
                   operation == other.operation &&
                   priority == other.priority &&
                   source == other.source;
        }

        /// <summary>
        /// �ٸ� �����ڿ� ����
        /// </summary>
        public void StackWith(AttributeModifier other)
        {
            if (!CanStackWith(other)) return;

            // �� �ջ�
            value += other.value;

            // ���ӽð� ó��
            if (isTemporary && other.isTemporary)
            {
                // �� �� ���ӽð� ���
                if (other.duration > duration)
                {
                    duration = other.duration;
                }

                // ���� �ð��� �� �� �� ���
                if (other.timeRemaining > timeRemaining)
                {
                    timeRemaining = other.timeRemaining;
                }
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// ����׿� ���ڿ�
        /// </summary>
        public override string ToString()
        {
            string operationStr = GetOperationString();
            string timeStr = isTemporary ? $" ({timeRemaining:F1}s/{duration:F1}s)" : "";
            string activeStr = isActive ? "" : " [Inactive]";

            return $"{modifierName}: {targetAttribute} {operationStr} [{priority}]{timeStr}{activeStr}";
        }

        private string GetOperationString()
        {
            switch (operation)
            {
                case GASConstants.ModifierOperation.Add:
                    return $"+{value:F2}";
                case GASConstants.ModifierOperation.Subtract:
                    return $"-{value:F2}";
                case GASConstants.ModifierOperation.Multiply:
                    return $"x{value:F2}";
                case GASConstants.ModifierOperation.Divide:
                    return $"/{value:F2}";
                case GASConstants.ModifierOperation.Override:
                    return $"={value:F2}";
                case GASConstants.ModifierOperation.AddPercent:
                    return $"+{value:F1}%";
                case GASConstants.ModifierOperation.SubtractPercent:
                    return $"-{value:F1}%";
                default:
                    return value.ToString("F2");
            }
        }

        /// <summary>
        /// Inspector ǥ�ÿ� �̸�
        /// </summary>
        public string GetDisplayName()
        {
            return $"{modifierName} ({GetOperationString()})";
        }
        #endregion

        #region Static Factory Methods
        /// <summary>
        /// �÷� ���ʽ� ������ ����
        /// </summary>
        public static AttributeModifier CreateFlatBonus(string name, GASConstants.AttributeType target, float value)
        {
            return new AttributeModifier(name, target, GASConstants.ModifierOperation.Add, value);
        }

        /// <summary>
        /// �ۼ�Ʈ ���ʽ� ������ ����
        /// </summary>
        public static AttributeModifier CreatePercentBonus(string name, GASConstants.AttributeType target, float percent)
        {
            return new AttributeModifier(name, target, GASConstants.ModifierOperation.AddPercent, percent);
        }

        /// <summary>
        /// �÷� ���� ������ ����
        /// </summary>
        public static AttributeModifier CreateFlatPenalty(string name, GASConstants.AttributeType target, float value)
        {
            return new AttributeModifier(name, target, GASConstants.ModifierOperation.Subtract, value);
        }

        /// <summary>
        /// �ۼ�Ʈ ���� ������ ����
        /// </summary>
        public static AttributeModifier CreatePercentPenalty(string name, GASConstants.AttributeType target, float percent)
        {
            return new AttributeModifier(name, target, GASConstants.ModifierOperation.SubtractPercent, percent);
        }

        /// <summary>
        /// ����� ������ ����
        /// </summary>
        public static AttributeModifier CreateOverride(string name, GASConstants.AttributeType target, float value)
        {
            return new AttributeModifier(name, target, GASConstants.ModifierOperation.Override, value,
                GASConstants.ModifierPriority.Override);
        }
        #endregion
    }
}