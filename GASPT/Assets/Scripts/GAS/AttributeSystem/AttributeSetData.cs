using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// AttributeSet의 초기값을 설정하는 ScriptableObject
    /// Inspector에서 편집 가능한 Attribute 데이터를 제공합니다
    /// </summary>
    [CreateAssetMenu(fileName = "New AttributeSet Data", menuName = "GAS/Attribute Set Data")]
    public class AttributeSetData : ScriptableObject
    {
        #region Inner Classes

        /// <summary>
        /// Attribute 초기값 설정을 위한 직렬화 가능 클래스
        /// </summary>
        [Serializable]
        public class AttributeInitData
        {
            [Tooltip("설정할 Attribute 타입")]
            public AttributeType attributeType;

            [Tooltip("초기 Base 값")]
            public float baseValue = 0f;

            [Tooltip("최소값 사용 여부")]
            public bool useMinValue = false;

            [Tooltip("최소값")]
            [ConditionalField("useMinValue")]
            public float minValue = 0f;

            [Tooltip("최대값 사용 여부")]
            public bool useMaxValue = false;

            [Tooltip("최대값")]
            [ConditionalField("useMaxValue")]
            public float maxValue = 100f;

            [Tooltip("이 Attribute에 대한 설명")]
            [TextArea(2, 4)]
            public string description;

            /// <summary>
            /// BaseAttribute 인스턴스 생성
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
            /// 유효성 검증
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
        /// Attribute 프리셋 템플릿
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
        [Tooltip("이 AttributeSet의 이름")]
        [SerializeField] private string setName = "New Attribute Set";

        [Tooltip("이 AttributeSet의 설명")]
        [TextArea(3, 5)]
        [SerializeField] private string description;

        [Tooltip("대상 AttributeSet 타입 (예: CharacterAttributeSet)")]
        [SerializeField] private string targetSetTypeName;

        [Header("Attributes")]
        [Tooltip("이 Set에 포함될 Attribute들의 초기값")]
        [SerializeField] private List<AttributeInitData> attributes = new List<AttributeInitData>();

        [Header("Advanced")]
        [Tooltip("자동으로 기본 Attribute들을 추가")]
        [SerializeField] private bool autoAddDefaultAttributes = false;

        [Tooltip("프리셋 템플릿 (재사용 가능한 설정)")]
        [SerializeField] private List<AttributePreset> presets = new List<AttributePreset>();

        #endregion

        #region Properties

        /// <summary>
        /// AttributeSet 이름
        /// </summary>
        public string SetName => setName;

        /// <summary>
        /// 설명
        /// </summary>
        public string Description => description;

        /// <summary>
        /// 등록된 Attribute 개수
        /// </summary>
        public int AttributeCount => attributes?.Count ?? 0;

        /// <summary>
        /// 모든 Attribute 타입 목록
        /// </summary>
        public IEnumerable<AttributeType> AttributeTypes => attributes?.Select(a => a.attributeType) ?? Enumerable.Empty<AttributeType>();

        #endregion

        #region Public Methods

        /// <summary>
        /// AttributeSet 인스턴스 생성
        /// </summary>
        public AttributeSet CreateAttributeSet()
        {
            if (!ValidateData())
            {
                Debug.LogError($"[GAS] Failed to create AttributeSet from {name}: Validation failed");
                return null;
            }

            // 타입 이름으로 AttributeSet 타입 찾기
            Type setType = null;
            if (!string.IsNullOrEmpty(targetSetTypeName))
            {
                setType = Type.GetType($"GAS.AttributeSystem.{targetSetTypeName}")
                       ?? Type.GetType(targetSetTypeName);
            }

            // 기본 AttributeSet 생성
            AttributeSet attributeSet;
            if (setType != null && typeof(AttributeSet).IsAssignableFrom(setType))
            {
                attributeSet = Activator.CreateInstance(setType) as AttributeSet;
            }
            else
            {
                // 기본 AttributeSet 사용
                attributeSet = new AttributeSet();
                if (!string.IsNullOrEmpty(targetSetTypeName))
                {
                    Debug.LogWarning($"[GAS] Could not find AttributeSet type: {targetSetTypeName}, using default AttributeSet");
                }
            }

            // Attribute 초기화
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
        /// 특정 Attribute의 초기값 가져오기
        /// </summary>
        public AttributeInitData GetAttributeData(AttributeType type)
        {
            return attributes?.FirstOrDefault(a => a.attributeType == type);
        }

        /// <summary>
        /// Attribute 초기값 설정 또는 업데이트
        /// </summary>
        public void SetAttributeData(AttributeType type, float baseValue, float? minValue = null, float? maxValue = null)
        {
            var existing = GetAttributeData(type);

            if (existing != null)
            {
                // 기존 데이터 업데이트
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
                // 새 데이터 추가
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
        /// 프리셋 적용
        /// </summary>
        public void ApplyPreset(string presetName)
        {
            var preset = presets?.FirstOrDefault(p => p.presetName == presetName);

            if (preset == null)
            {
                Debug.LogError($"[GAS] Preset '{presetName}' not found");
                return;
            }

            // 기존 Attribute 클리어
            attributes.Clear();

            // 프리셋 Attribute 복사
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
        /// 현재 설정을 프리셋으로 저장
        /// </summary>
        public void SaveAsPreset(string presetName)
        {
            if (string.IsNullOrEmpty(presetName))
            {
                Debug.LogError("[GAS] Preset name cannot be empty");
                return;
            }

            // 기존 프리셋 확인
            var existing = presets?.FirstOrDefault(p => p.presetName == presetName);
            if (existing != null)
            {
                presets.Remove(existing);
            }

            // 새 프리셋 생성
            var newPreset = new AttributePreset
            {
                presetName = presetName,
                attributes = new List<AttributeInitData>()
            };

            // 현재 Attribute 복사
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
        /// 데이터 유효성 검증
        /// </summary>
        private bool ValidateData()
        {
            if (attributes == null || attributes.Count == 0)
            {
                Debug.LogWarning($"[GAS] AttributeSetData '{name}' has no attributes defined");
                return true; // 빈 Set도 허용
            }

            // 중복 AttributeType 체크
            var duplicates = attributes
                .GroupBy(a => a.attributeType)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicates.Any())
            {
                Debug.LogError($"[GAS] AttributeSetData '{name}' has duplicate AttributeTypes: {string.Join(", ", duplicates)}");
                return false;
            }

            // 각 Attribute 유효성 검증
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
            // Editor에서 값이 변경될 때 유효성 검증
            ValidateData();

            // 자동 기본 Attribute 추가
            if (autoAddDefaultAttributes)
            {
                AddDefaultAttributes();
                autoAddDefaultAttributes = false; // 한 번만 실행
            }
        }

        /// <summary>
        /// 기본 Attribute들 자동 추가
        /// </summary>
        private void AddDefaultAttributes()
        {
            // 기본 Attribute 타입들 (예시)
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
                // 이미 존재하는지 확인
                if (!attributes.Any(a => a.attributeType == type))
                {
                    var newAttr = new AttributeInitData
                    {
                        attributeType = type,
                        baseValue = GetDefaultValueForType(type),
                        description = GetDefaultDescriptionForType(type)
                    };

                    // 특정 타입에 대한 Min/Max 설정
                    ConfigureAttributeLimits(newAttr);

                    attributes.Add(newAttr);
                }
            }

            Debug.Log($"[GAS] Added {defaultTypes.Length} default attributes to '{name}'");
        }

        /// <summary>
        /// AttributeType에 따른 기본값
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
        /// AttributeType에 따른 기본 설명
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
        /// Attribute 제한값 설정
        /// </summary>
        private void ConfigureAttributeLimits(AttributeInitData attr)
        {
            switch (attr.attributeType)
            {
                case AttributeType.Health:
                    attr.useMinValue = true;
                    attr.minValue = 0f;
                    attr.useMaxValue = true;
                    attr.maxValue = 100f; // MaxHealth로 동적 설정 필요
                    break;

                case AttributeType.Mana:
                    attr.useMinValue = true;
                    attr.minValue = 0f;
                    attr.useMaxValue = true;
                    attr.maxValue = 50f; // MaxMana로 동적 설정 필요
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
    /// Inspector에서 조건부 표시를 위한 Attribute (Custom Property Drawer 필요)
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