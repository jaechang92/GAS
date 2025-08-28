using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// AttributeSet�� �ʱⰪ�� �����ϴ� ScriptableObject
    /// Inspector���� ���� ������ Attribute �����͸� �����մϴ�
    /// </summary>
    [CreateAssetMenu(fileName = "New AttributeSet Data", menuName = "GAS/Attribute Set Data")]
    public class AttributeSetData : ScriptableObject
    {
        #region Inner Classes

        /// <summary>
        /// Attribute �ʱⰪ ������ ���� ����ȭ ���� Ŭ����
        /// </summary>
        [Serializable]
        public class AttributeInitData
        {
            [Tooltip("������ Attribute Ÿ��")]
            public AttributeType attributeType;

            [Tooltip("�ʱ� Base ��")]
            public float baseValue = 0f;

            [Tooltip("�ּҰ� ��� ����")]
            public bool useMinValue = false;

            [Tooltip("�ּҰ�")]
            [ConditionalField("useMinValue")]
            public float minValue = 0f;

            [Tooltip("�ִ밪 ��� ����")]
            public bool useMaxValue = false;

            [Tooltip("�ִ밪")]
            [ConditionalField("useMaxValue")]
            public float maxValue = 100f;

            [Tooltip("�� Attribute�� ���� ����")]
            [TextArea(2, 4)]
            public string description;

            /// <summary>
            /// BaseAttribute �ν��Ͻ� ����
            /// </summary>
            public BaseAttribute CreateAttribute()
            {
                var attribute = new BaseAttribute(attributeType.GetType().Name ,attributeType, baseValue);
                
                if (useMinValue)
                {
                    attribute.SetMinValue(minValue);
                }

                if (useMaxValue)
                {
                    attribute.SetMaxValue(maxValue);
                }

                return attribute;
            }

            /// <summary>
            /// ��ȿ�� ����
            /// </summary>
            public bool Validate()
            {
                if (useMinValue && useMaxValue && minValue > maxValue)
                {
                    Debug.LogError($"[GAS] AttributeInitData: Min value ({minValue}) is greater than Max value ({maxValue}) for {attributeType}");
                    return false;
                }

                if (useMinValue && baseValue < minValue)
                {
                    Debug.LogWarning($"[GAS] AttributeInitData: Base value ({baseValue}) is less than Min value ({minValue}) for {attributeType}");
                }

                if (useMaxValue && baseValue > maxValue)
                {
                    Debug.LogWarning($"[GAS] AttributeInitData: Base value ({baseValue}) is greater than Max value ({maxValue}) for {attributeType}");
                }

                return true;
            }
        }

        /// <summary>
        /// Attribute ������ ���ø�
        /// </summary>
        [Serializable]
        public class AttributePreset
        {
            public string presetName;
            public List<AttributeInitData> attributes = new List<AttributeInitData>();
        }

        #endregion

        #region Serialized Fields

        [Header("Basic Configuration")]
        [Tooltip("�� AttributeSet�� �̸�")]
        [SerializeField] private string setName = "New Attribute Set";

        [Tooltip("�� AttributeSet�� ����")]
        [TextArea(3, 5)]
        [SerializeField] private string description;

        [Tooltip("��� AttributeSet Ÿ�� (��: CharacterAttributeSet)")]
        [SerializeField] private string targetSetTypeName;

        [Header("Attributes")]
        [Tooltip("�� Set�� ���Ե� Attribute���� �ʱⰪ")]
        [SerializeField] private List<AttributeInitData> attributes = new List<AttributeInitData>();

        [Header("Advanced")]
        [Tooltip("�ڵ����� �⺻ Attribute���� �߰�")]
        [SerializeField] private bool autoAddDefaultAttributes = false;

        [Tooltip("������ ���ø� (���� ������ ����)")]
        [SerializeField] private List<AttributePreset> presets = new List<AttributePreset>();

        #endregion

        #region Properties

        /// <summary>
        /// AttributeSet �̸�
        /// </summary>
        public string SetName => setName;

        /// <summary>
        /// ����
        /// </summary>
        public string Description => description;

        /// <summary>
        /// ��ϵ� Attribute ����
        /// </summary>
        public int AttributeCount => attributes?.Count ?? 0;

        /// <summary>
        /// ��� Attribute Ÿ�� ���
        /// </summary>
        public IEnumerable<AttributeType> AttributeTypes => attributes?.Select(a => a.attributeType) ?? Enumerable.Empty<AttributeType>();

        #endregion

        #region Public Methods

        /// <summary>
        /// AttributeSet �ν��Ͻ� ����
        /// </summary>
        public AttributeSet CreateAttributeSet()
        {
            if (!ValidateData())
            {
                Debug.LogError($"[GAS] Failed to create AttributeSet from {name}: Validation failed");
                return null;
            }

            // Ÿ�� �̸����� AttributeSet Ÿ�� ã��
            Type setType = null;
            if (!string.IsNullOrEmpty(targetSetTypeName))
            {
                setType = Type.GetType($"GAS.AttributeSystem.{targetSetTypeName}")
                       ?? Type.GetType(targetSetTypeName);
            }

            // �⺻ AttributeSet ����
            AttributeSet attributeSet;
            if (setType != null && typeof(AttributeSet).IsAssignableFrom(setType))
            {
                attributeSet = Activator.CreateInstance(setType) as AttributeSet;
            }
            else
            {
                // �⺻ AttributeSet ���
                attributeSet = new AttributeSet();
                if (!string.IsNullOrEmpty(targetSetTypeName))
                {
                    Debug.LogWarning($"[GAS] Could not find AttributeSet type: {targetSetTypeName}, using default AttributeSet");
                }
            }

            // Attribute �ʱ�ȭ
            foreach (var initData in attributes)
            {
                if (initData != null && initData.Validate())
                {
                    var attribute = initData.CreateAttribute();
                    attributeSet.AddAttribute(attribute);
                }
            }

            Debug.Log($"[GAS] Created AttributeSet '{setName}' with {attributes.Count} attributes");

            return attributeSet;
        }

        /// <summary>
        /// Ư�� Attribute�� �ʱⰪ ��������
        /// </summary>
        public AttributeInitData GetAttributeData(AttributeType type)
        {
            return attributes?.FirstOrDefault(a => a.attributeType == type);
        }

        /// <summary>
        /// Attribute �ʱⰪ ���� �Ǵ� ������Ʈ
        /// </summary>
        public void SetAttributeData(AttributeType type, float baseValue, float? minValue = null, float? maxValue = null)
        {
            var existing = GetAttributeData(type);

            if (existing != null)
            {
                // ���� ������ ������Ʈ
                existing.baseValue = baseValue;

                if (minValue.HasValue)
                {
                    existing.useMinValue = true;
                    existing.minValue = minValue.Value;
                }

                if (maxValue.HasValue)
                {
                    existing.useMaxValue = true;
                    existing.maxValue = maxValue.Value;
                }
            }
            else
            {
                // �� ������ �߰�
                var newData = new AttributeInitData
                {
                    attributeType = type,
                    baseValue = baseValue
                };

                if (minValue.HasValue)
                {
                    newData.useMinValue = true;
                    newData.minValue = minValue.Value;
                }

                if (maxValue.HasValue)
                {
                    newData.useMaxValue = true;
                    newData.maxValue = maxValue.Value;
                }

                attributes.Add(newData);
            }
        }

        /// <summary>
        /// ������ ����
        /// </summary>
        public void ApplyPreset(string presetName)
        {
            var preset = presets?.FirstOrDefault(p => p.presetName == presetName);

            if (preset == null)
            {
                Debug.LogError($"[GAS] Preset '{presetName}' not found");
                return;
            }

            // ���� Attribute Ŭ����
            attributes.Clear();

            // ������ Attribute ����
            foreach (var presetAttr in preset.attributes)
            {
                var newAttr = new AttributeInitData
                {
                    attributeType = presetAttr.attributeType,
                    baseValue = presetAttr.baseValue,
                    useMinValue = presetAttr.useMinValue,
                    minValue = presetAttr.minValue,
                    useMaxValue = presetAttr.useMaxValue,
                    maxValue = presetAttr.maxValue,
                    description = presetAttr.description
                };

                attributes.Add(newAttr);
            }

            Debug.Log($"[GAS] Applied preset '{presetName}' with {preset.attributes.Count} attributes");
        }

        /// <summary>
        /// ���� ������ ���������� ����
        /// </summary>
        public void SaveAsPreset(string presetName)
        {
            if (string.IsNullOrEmpty(presetName))
            {
                Debug.LogError("[GAS] Preset name cannot be empty");
                return;
            }

            // ���� ������ Ȯ��
            var existing = presets?.FirstOrDefault(p => p.presetName == presetName);
            if (existing != null)
            {
                presets.Remove(existing);
            }

            // �� ������ ����
            var newPreset = new AttributePreset
            {
                presetName = presetName,
                attributes = new List<AttributeInitData>()
            };

            // ���� Attribute ����
            foreach (var attr in attributes)
            {
                var copy = new AttributeInitData
                {
                    attributeType = attr.attributeType,
                    baseValue = attr.baseValue,
                    useMinValue = attr.useMinValue,
                    minValue = attr.minValue,
                    useMaxValue = attr.useMaxValue,
                    maxValue = attr.maxValue,
                    description = attr.description
                };

                newPreset.attributes.Add(copy);
            }

            if (presets == null)
                presets = new List<AttributePreset>();

            presets.Add(newPreset);

            Debug.Log($"[GAS] Saved preset '{presetName}' with {attributes.Count} attributes");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// ������ ��ȿ�� ����
        /// </summary>
        private bool ValidateData()
        {
            if (attributes == null || attributes.Count == 0)
            {
                Debug.LogWarning($"[GAS] AttributeSetData '{name}' has no attributes defined");
                return true; // �� Set�� ���
            }

            // �ߺ� AttributeType üũ
            var duplicates = attributes
                .GroupBy(a => a.attributeType)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicates.Any())
            {
                Debug.LogError($"[GAS] AttributeSetData '{name}' has duplicate AttributeTypes: {string.Join(", ", duplicates)}");
                return false;
            }

            // �� Attribute ��ȿ�� ����
            foreach (var attr in attributes)
            {
                if (!attr.Validate())
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Editor Support

        private void OnValidate()
        {
            // Editor���� ���� ����� �� ��ȿ�� ����
            ValidateData();

            // �ڵ� �⺻ Attribute �߰�
            if (autoAddDefaultAttributes)
            {
                AddDefaultAttributes();
                autoAddDefaultAttributes = false; // �� ���� ����
            }
        }

        /// <summary>
        /// �⺻ Attribute�� �ڵ� �߰�
        /// </summary>
        private void AddDefaultAttributes()
        {
            // �⺻ Attribute Ÿ�Ե� (����)
            var defaultTypes = new[]
            {
                AttributeType.Health,
                AttributeType.MaxHealth,
                AttributeType.Mana,
                AttributeType.MaxMana,
                AttributeType.AttackPower,
                AttributeType.DefensePower,
                AttributeType.MoveSpeed,
                AttributeType.AttackSpeed
            };

            foreach (var type in defaultTypes)
            {
                // �̹� �����ϴ��� Ȯ��
                if (!attributes.Any(a => a.attributeType == type))
                {
                    var newAttr = new AttributeInitData
                    {
                        attributeType = type,
                        baseValue = GetDefaultValueForType(type),
                        description = GetDefaultDescriptionForType(type)
                    };

                    // Ư�� Ÿ�Կ� ���� Min/Max ����
                    ConfigureAttributeLimits(newAttr);

                    attributes.Add(newAttr);
                }
            }

            Debug.Log($"[GAS] Added {defaultTypes.Length} default attributes to '{name}'");
        }

        /// <summary>
        /// AttributeType�� ���� �⺻��
        /// </summary>
        private float GetDefaultValueForType(AttributeType type)
        {
            switch (type)
            {
                case AttributeType.Health:
                case AttributeType.MaxHealth:
                    return 100f;
                case AttributeType.Mana:
                case AttributeType.MaxMana:
                    return 50f;
                case AttributeType.AttackPower:
                case AttributeType.DefensePower:
                    return 10f;
                case AttributeType.MoveSpeed:
                    return 5f;
                case AttributeType.AttackSpeed:
                    return 1f;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// AttributeType�� ���� �⺻ ����
        /// </summary>
        private string GetDefaultDescriptionForType(AttributeType type)
        {
            switch (type)
            {
                case AttributeType.Health:
                    return "Current health points";
                case AttributeType.MaxHealth:
                    return "Maximum health points";
                case AttributeType.Mana:
                    return "Current mana points";
                case AttributeType.MaxMana:
                    return "Maximum mana points";
                case AttributeType.AttackPower:
                    return "Physical damage output";
                case AttributeType.DefensePower:
                    return "Physical damage reduction";
                case AttributeType.MoveSpeed:
                    return "Movement speed in units/second";
                case AttributeType.AttackSpeed:
                    return "Attacks per second";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Attribute ���Ѱ� ����
        /// </summary>
        private void ConfigureAttributeLimits(AttributeInitData attr)
        {
            switch (attr.attributeType)
            {
                case AttributeType.Health:
                    attr.useMinValue = true;
                    attr.minValue = 0f;
                    attr.useMaxValue = true;
                    attr.maxValue = 100f; // MaxHealth�� ���� ���� �ʿ�
                    break;

                case AttributeType.Mana:
                    attr.useMinValue = true;
                    attr.minValue = 0f;
                    attr.useMaxValue = true;
                    attr.maxValue = 50f; // MaxMana�� ���� ���� �ʿ�
                    break;

                case AttributeType.MoveSpeed:
                case AttributeType.AttackSpeed:
                    attr.useMinValue = true;
                    attr.minValue = 0f;
                    break;
            }
        }

        #endregion
    }

    /// <summary>
    /// Inspector���� ���Ǻ� ǥ�ø� ���� Attribute (Custom Property Drawer �ʿ�)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ConditionalFieldAttribute : PropertyAttribute
    {
        public string ConditionalField { get; }

        public ConditionalFieldAttribute(string conditionalField)
        {
            ConditionalField = conditionalField;
        }
    }
}