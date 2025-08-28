// ���� ��ġ: Assets/Scripts/GAS/AttributeSystem/BaseAttribute.cs
using GAS.Core;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// �����÷��� �Ӽ��� �⺻ Ŭ����
    /// HP, Mana, AttackPower ���� ��ġ�� ����
    /// </summary>
    [Serializable]
    public class BaseAttribute
    {
        #region Constants
        private const float EPSILON = 0.001f;
        #endregion

        #region Serialized Fields
        [SerializeField] private string attributeName;
        [SerializeField] private GASConstants.AttributeType attributeType;
        [SerializeField] private float baseValue;
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 9999f;
        [SerializeField] private bool hasMaxValue = false;
        #endregion

        #region Private Fields
        private float currentValue;
        private List<AttributeModifier> modifiers = new List<AttributeModifier>();
        private float lastCalculatedValue;
        private bool isDirty = true;
        #endregion

        #region Events
        /// <summary>
        /// �Ӽ����� ����Ǿ��� ��
        /// </summary>
        public event Action<float, float> OnValueChanged;

        /// <summary>
        /// �Ӽ��� �ִ밪�� �������� ��
        /// </summary>
        public event Action OnMaxReached;

        /// <summary>
        /// �Ӽ��� �ּҰ��� �������� ��
        /// </summary>
        public event Action OnMinReached;

        /// <summary>
        /// �����ڰ� �߰��Ǿ��� ��
        /// </summary>
        public event Action<AttributeModifier> OnModifierAdded;

        /// <summary>
        /// �����ڰ� ���ŵǾ��� ��
        /// </summary>
        public event Action<AttributeModifier> OnModifierRemoved;
        #endregion

        #region Properties
        /// <summary>
        /// �Ӽ� �̸�
        /// </summary>
        public string AttributeName
        {
            get => attributeName;
            set => attributeName = value;
        }

        /// <summary>
        /// �Ӽ� Ÿ��
        /// </summary>
        public GASConstants.AttributeType AttributeType
        {
            get => attributeType;
            set => attributeType = value;
        }

        /// <summary>
        /// �⺻��
        /// </summary>
        public float BaseValue
        {
            get => baseValue;
            set
            {
                if (Math.Abs(baseValue - value) > EPSILON)
                {
                    baseValue = value;
                    MarkDirty();
                }
            }
        }

        /// <summary>
        /// ���簪 (������ ���� ��)
        /// </summary>
        public float CurrentValue
        {
            get
            {
                if (isDirty)
                {
                    CalculateCurrentValue();
                }
                return currentValue;
            }
            set
            {
                SetCurrentValue(value);
            }
        }

        /// <summary>
        /// �ּҰ�
        /// </summary>
        public float MinValue
        {
            get => minValue;
            set
            {
                minValue = value;
                MarkDirty();
            }
        }

        /// <summary>
        /// �ִ밪
        /// </summary>
        public float MaxValue
        {
            get => hasMaxValue ? maxValue : float.MaxValue;
            set
            {
                maxValue = value;
                hasMaxValue = true;
                MarkDirty();
            }
        }

        public bool HasMinValue => true; // �׻� �ּҰ��� ����

        /// <summary>
        /// �ִ밪 ���� ����
        /// </summary>
        public bool HasMaxValue
        {
            get => hasMaxValue;
            set => hasMaxValue = value;
        }

        /// <summary>
        /// �Ӽ��� �ִ밪���� Ȯ��
        /// </summary>
        public bool IsAtMax => hasMaxValue && Math.Abs(CurrentValue - maxValue) < EPSILON;

        /// <summary>
        /// �Ӽ��� �ּҰ����� Ȯ��
        /// </summary>
        public bool IsAtMin => Math.Abs(CurrentValue - minValue) < EPSILON;

        /// <summary>
        /// ���簪�� ���� (0~1)
        /// </summary>
        public float Ratio
        {
            get
            {
                if (!hasMaxValue) return 1f;
                float range = maxValue - minValue;
                if (range <= 0) return 0f;
                return Mathf.Clamp01((CurrentValue - minValue) / range);
            }
        }

        /// <summary>
        /// ������ ����
        /// </summary>
        public int ModifierCount => modifiers.Count;
        #endregion

        #region Constructors
        /// <summary>
        /// �⺻ ������
        /// </summary>
        public BaseAttribute()
        {
            attributeName = "New Attribute";
            attributeType = GASConstants.AttributeType.Health;
            baseValue = 0f;
            currentValue = 0f;
        }

        /// <summary>
        /// �̸��� Ÿ������ ����
        /// </summary>
        public BaseAttribute(string name, GASConstants.AttributeType type, float baseVal)
        {
            attributeName = name;
            attributeType = type;
            baseValue = baseVal;
            currentValue = baseVal;
        }


        /// <summary>
        /// ���� ���� ������
        /// </summary>
        public BaseAttribute(string name, GASConstants.AttributeType type, float baseVal, float min, float max)
        {
            attributeName = name;
            attributeType = type;
            baseValue = baseVal;
            minValue = min;
            maxValue = max;
            hasMaxValue = true;
            currentValue = baseVal;
        }
        #endregion

        #region Value Management
        /// <summary>
        /// ���簪 ���� ���� (������ ����)
        /// </summary>
        public void SetCurrentValue(float value)
        {
            float oldValue = currentValue;
            currentValue = ClampValue(value);

            if (Math.Abs(oldValue - currentValue) > EPSILON)
            {
                OnValueChanged?.Invoke(oldValue, currentValue);

                if (IsAtMax) OnMaxReached?.Invoke();
                if (IsAtMin) OnMinReached?.Invoke();
            }
        }

        /// <summary>
        /// �⺻������ ����
        /// </summary>
        public void ResetToBase()
        {
            modifiers.Clear();
            SetCurrentValue(baseValue);
        }

        /// <summary>
        /// �� ���ϱ�
        /// </summary>
        public void AddValue(float amount)
        {
            SetCurrentValue(CurrentValue + amount);
        }

        /// <summary>
        /// �� ����
        /// </summary>
        public void SubtractValue(float amount)
        {
            SetCurrentValue(CurrentValue - amount);
        }

        /// <summary>
        /// �� ���ϱ�
        /// </summary>
        public void MultiplyValue(float multiplier)
        {
            SetCurrentValue(CurrentValue * multiplier);
        }

        /// <summary>
        /// �ִ밪���� ����
        /// </summary>
        public void SetToMax()
        {
            if (hasMaxValue)
            {
                SetCurrentValue(maxValue);
            }
        }

        /// <summary>
        /// �ּҰ����� ����
        /// </summary>
        public void SetToMin()
        {
            SetCurrentValue(minValue);
        }

        public void SetMinValue(float value)
        {
            minValue = value;
        }

        public void SetMaxValue(float value)
        {
            maxValue = value;
            hasMaxValue = true;
        }

        #endregion

        #region Modifier Management
        /// <summary>
        /// ������ �߰�
        /// </summary>
        public void AddModifier(AttributeModifier modifier)
        {
            if (modifier == null) return;

            modifiers.Add(modifier);
            modifiers.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            OnModifierAdded?.Invoke(modifier);
            MarkDirty();
        }

        /// <summary>
        /// ������ ����
        /// </summary>
        public bool RemoveModifier(AttributeModifier modifier)
        {
            if (modifier == null) return false;

            bool removed = modifiers.Remove(modifier);
            if (removed)
            {
                OnModifierRemoved?.Invoke(modifier);
                MarkDirty();
            }

            return removed;
        }

        /// <summary>
        /// �ҽ��� ������ ����
        /// </summary>
        public int RemoveModifiersBySource(object source)
        {
            int removedCount = 0;

            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                if (modifiers[i].Source == source)
                {
                    var modifier = modifiers[i];
                    modifiers.RemoveAt(i);
                    OnModifierRemoved?.Invoke(modifier);
                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                MarkDirty();
            }

            return removedCount;
        }

        /// <summary>
        /// ��� ������ ����
        /// </summary>
        public void ClearModifiers()
        {
            if (modifiers.Count > 0)
            {
                modifiers.Clear();
                MarkDirty();
            }
        }

        /// <summary>
        /// ������ ����Ʈ ��������
        /// </summary>
        public List<AttributeModifier> GetModifiers()
        {
            return new List<AttributeModifier>(modifiers);
        }

        /// <summary>
        /// Ư�� Ÿ���� ������ ��������
        /// </summary>
        public List<AttributeModifier> GetModifiersByOperation(GASConstants.ModifierOperation operation)
        {
            return modifiers.FindAll(m => m.Operation == operation);
        }
        #endregion

        #region Calculation
        private void CalculateCurrentValue()
        {
            float calculatedValue = baseValue;

            // Priority ������� ������ ����
            foreach (var modifier in modifiers)
            {
                calculatedValue = ApplyModifier(calculatedValue, modifier);
            }

            float oldValue = currentValue;
            currentValue = ClampValue(calculatedValue);
            lastCalculatedValue = currentValue;
            isDirty = false;

            if (Math.Abs(oldValue - currentValue) > EPSILON)
            {
                OnValueChanged?.Invoke(oldValue, currentValue);

                if (IsAtMax) OnMaxReached?.Invoke();
                if (IsAtMin) OnMinReached?.Invoke();
            }
        }

        private float ApplyModifier(float value, AttributeModifier modifier)
        {
            switch (modifier.Operation)
            {
                case GASConstants.ModifierOperation.Add:
                    return value + modifier.Value;

                case GASConstants.ModifierOperation.Subtract:
                    return value - modifier.Value;

                case GASConstants.ModifierOperation.Multiply:
                    return value * modifier.Value;

                case GASConstants.ModifierOperation.Divide:
                    return modifier.Value != 0 ? value / modifier.Value : value;

                case GASConstants.ModifierOperation.Override:
                    return modifier.Value;

                case GASConstants.ModifierOperation.AddPercent:
                    return value * (1f + modifier.Value / 100f);

                case GASConstants.ModifierOperation.SubtractPercent:
                    return value * (1f - modifier.Value / 100f);

                default:
                    return value;
            }
        }

        private float ClampValue(float value)
        {
            value = Mathf.Max(value, minValue);

            if (hasMaxValue)
            {
                value = Mathf.Min(value, maxValue);
            }

            return value;
        }

        private void MarkDirty()
        {
            isDirty = true;
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// �Ӽ� ����
        /// </summary>
        public BaseAttribute Clone()
        {
            var clone = new BaseAttribute(attributeName, attributeType, baseValue, minValue, maxValue)
            {
                hasMaxValue = hasMaxValue,
                currentValue = currentValue
            };

            foreach (var modifier in modifiers)
            {
                clone.AddModifier(modifier.Clone());
            }

            return clone;
        }

        /// <summary>
        /// �Ӽ� ���� ���ڿ�
        /// </summary>
        public override string ToString()
        {
            return $"{attributeName}: {CurrentValue:F2}/{(hasMaxValue ? maxValue.ToString("F2") : "��")} (Base: {baseValue:F2})";
        }

        /// <summary>
        /// ����� ���� ���
        /// </summary>
        public void PrintDebugInfo()
        {
            Debug.Log($"[GAS] Attribute: {attributeName}");
            Debug.Log($"  Base: {baseValue}, Current: {CurrentValue}");
            Debug.Log($"  Range: [{minValue} - {(hasMaxValue ? maxValue.ToString() : "��")}]");
            Debug.Log($"  Modifiers: {modifiers.Count}");

            foreach (var modifier in modifiers)
            {
                Debug.Log($"    {modifier}");
            }
        }
        #endregion
    }
}