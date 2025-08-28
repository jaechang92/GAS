// 파일 위치: Assets/Scripts/GAS/AttributeSystem/AttributeSet.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GAS.Core;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// 여러 속성을 관리하는 속성 집합 클래스
    /// </summary>
    [Serializable]
    public class AttributeSet
    {
        #region Serialized Fields
        [SerializeField] private string setName = "Default Set";
        [SerializeField] private List<BaseAttribute> attributes = new List<BaseAttribute>();
        #endregion

        #region Private Fields
        private Dictionary<GASConstants.AttributeType, BaseAttribute> attributeMap;
        private Dictionary<string, BaseAttribute> attributeNameMap;
        #endregion

        #region Events
        /// <summary>
        /// 속성이 추가되었을 때
        /// </summary>
        public event Action<BaseAttribute> OnAttributeAdded;

        /// <summary>
        /// 속성이 제거되었을 때
        /// </summary>
        public event Action<BaseAttribute> OnAttributeRemoved;

        /// <summary>
        /// 속성값이 변경되었을 때
        /// </summary>
        public event Action<BaseAttribute, float, float> OnAttributeValueChanged;
        #endregion

        #region Properties
        /// <summary>
        /// 속성 집합 이름
        /// </summary>
        public string SetName
        {
            get => setName;
            set => setName = value;
        }

        /// <summary>
        /// 속성 개수
        /// </summary>
        public int Count => attributes.Count;

        /// <summary>
        /// 속성 리스트 (읽기 전용)
        /// </summary>
        public IReadOnlyList<BaseAttribute> Attributes => attributes.AsReadOnly();
        #endregion

        #region Constructors
        /// <summary>
        /// 기본 생성자
        /// </summary>
        public AttributeSet()
        {
            setName = "New Attribute Set";
            InitializeMaps();
        }

        /// <summary>
        /// 이름 지정 생성자
        /// </summary>
        public AttributeSet(string name)
        {
            setName = name;
            InitializeMaps();
        }

        /// <summary>
        /// 복사 생성자
        /// </summary>
        public AttributeSet(AttributeSet other)
        {
            setName = other.setName + " (Copy)";
            InitializeMaps();

            foreach (var attribute in other.attributes)
            {
                AddAttribute(attribute.Clone());
            }
        }
        #endregion

        #region Initialization
        private void InitializeMaps()
        {
            attributeMap = new Dictionary<GASConstants.AttributeType, BaseAttribute>();
            attributeNameMap = new Dictionary<string, BaseAttribute>();
        }

        private void RebuildMaps()
        {
            attributeMap.Clear();
            attributeNameMap.Clear();

            foreach (var attribute in attributes)
            {
                if (attribute != null)
                {
                    attributeMap[attribute.AttributeType] = attribute;
                    attributeNameMap[attribute.AttributeName] = attribute;
                }
            }
        }

        /// <summary>
        /// 기본 속성 세트 초기화 (HP, Mana, Stamina)
        /// </summary>
        public void InitializeDefaultAttributes()
        {
            AddAttribute(new BaseAttribute("Health", GASConstants.AttributeType.Health, 100f, 0f, 100f));
            AddAttribute(new BaseAttribute("MaxHealth", GASConstants.AttributeType.MaxHealth, 100f, 1f, 9999f));

            AddAttribute(new BaseAttribute("Mana", GASConstants.AttributeType.Mana, 50f, 0f, 50f));
            AddAttribute(new BaseAttribute("MaxMana", GASConstants.AttributeType.MaxMana, 50f, 0f, 9999f));

            AddAttribute(new BaseAttribute("Stamina", GASConstants.AttributeType.Stamina, 100f, 0f, 100f));
            AddAttribute(new BaseAttribute("MaxStamina", GASConstants.AttributeType.MaxStamina, 100f, 0f, 9999f));
        }

        /// <summary>
        /// 전투 속성 초기화
        /// </summary>
        public void InitializeCombatAttributes()
        {
            AddAttribute(new BaseAttribute("AttackPower", GASConstants.AttributeType.AttackPower, 10f, 0f, 9999f));
            AddAttribute(new BaseAttribute("SpellPower", GASConstants.AttributeType.SpellPower, 10f, 0f, 9999f));
            AddAttribute(new BaseAttribute("Defense", GASConstants.AttributeType.Defense, 5f, 0f, 9999f));
            AddAttribute(new BaseAttribute("MagicResist", GASConstants.AttributeType.MagicResist, 5f, 0f, 9999f));

            AddAttribute(new BaseAttribute("CriticalChance", GASConstants.AttributeType.CriticalChance, 5f, 0f, 100f));
            AddAttribute(new BaseAttribute("CriticalDamage", GASConstants.AttributeType.CriticalDamage, 150f, 100f, 500f));
            AddAttribute(new BaseAttribute("AttackSpeed", GASConstants.AttributeType.AttackSpeed, 1f, 0.1f, 5f));
            AddAttribute(new BaseAttribute("CastSpeed", GASConstants.AttributeType.CastSpeed, 1f, 0.1f, 5f));
        }
        #endregion

        #region Attribute Management
        /// <summary>
        /// 속성 추가
        /// </summary>
        public bool AddAttribute(BaseAttribute attribute)
        {
            if (attribute == null) return false;

            // 중복 체크
            if (HasAttribute(attribute.AttributeType))
            {
                Debug.LogWarning($"[GAS] Attribute type {attribute.AttributeType} already exists in set");
                return false;
            }

            attributes.Add(attribute);

            if (attributeMap == null) InitializeMaps();
            attributeMap[attribute.AttributeType] = attribute;
            attributeNameMap[attribute.AttributeName] = attribute;

            // 이벤트 연결
            attribute.OnValueChanged += (oldValue, newValue) => HandleAttributeValueChanged(attribute, oldValue, newValue);

            OnAttributeAdded?.Invoke(attribute);

            return true;
        }

        /// <summary>
        /// 속성 제거
        /// </summary>
        public bool RemoveAttribute(GASConstants.AttributeType type)
        {
            var attribute = GetAttribute(type);
            if (attribute == null) return false;

            attributes.Remove(attribute);
            attributeMap.Remove(type);
            attributeNameMap.Remove(attribute.AttributeName);

            OnAttributeRemoved?.Invoke(attribute);

            return true;
        }

        /// <summary>
        /// 모든 속성 제거
        /// </summary>
        public void ClearAttributes()
        {
            attributes.Clear();
            attributeMap?.Clear();
            attributeNameMap?.Clear();
        }
        #endregion

        #region Attribute Access
        /// <summary>
        /// 타입으로 속성 가져오기
        /// </summary>
        public BaseAttribute GetAttribute(GASConstants.AttributeType type)
        {
            if (attributeMap == null) RebuildMaps();

            attributeMap.TryGetValue(type, out BaseAttribute attribute);
            return attribute;
        }

        /// <summary>
        /// 이름으로 속성 가져오기
        /// </summary>
        public BaseAttribute GetAttribute(string name)
        {
            if (attributeNameMap == null) RebuildMaps();

            attributeNameMap.TryGetValue(name, out BaseAttribute attribute);
            return attribute;
        }

        public List<BaseAttribute> GetAllAttributes()
        {
            return attributes;
        }

        /// <summary>
        /// 속성 보유 확인
        /// </summary>
        public bool HasAttribute(GASConstants.AttributeType type)
        {
            if (attributeMap == null) RebuildMaps();
            return attributeMap.ContainsKey(type);
        }

        /// <summary>
        /// 속성값 가져오기
        /// </summary>
        public float GetAttributeValue(GASConstants.AttributeType type)
        {
            var attribute = GetAttribute(type);
            return attribute?.CurrentValue ?? 0f;
        }

        /// <summary>
        /// 속성 기본값 가져오기
        /// </summary>
        public float GetAttributeBaseValue(GASConstants.AttributeType type)
        {
            var attribute = GetAttribute(type);
            return attribute?.BaseValue ?? 0f;
        }
        #endregion

        #region Attribute Modification
        /// <summary>
        /// 속성값 설정
        /// </summary>
        public bool SetAttributeValue(GASConstants.AttributeType type, float value)
        {
            var attribute = GetAttribute(type);
            if (attribute == null) return false;

            attribute.CurrentValue = value;
            return true;
        }

        /// <summary>
        /// 속성 기본값 설정
        /// </summary>
        public bool SetAttributeBaseValue(GASConstants.AttributeType type, float value)
        {
            var attribute = GetAttribute(type);
            if (attribute == null) return false;

            attribute.BaseValue = value;
            return true;
        }

        /// <summary>
        /// 속성값 더하기
        /// </summary>
        public bool AddAttributeValue(GASConstants.AttributeType type, float amount)
        {
            var attribute = GetAttribute(type);
            if (attribute == null) return false;

            attribute.AddValue(amount);
            return true;
        }

        /// <summary>
        /// 수정자 적용
        /// </summary>
        public bool ApplyModifier(AttributeModifier modifier)
        {
            if (modifier == null) return false;

            var attribute = GetAttribute(modifier.TargetAttribute);
            if (attribute == null) return false;

            attribute.AddModifier(modifier);
            return true;
        }

        /// <summary>
        /// 수정자 제거
        /// </summary>
        public bool RemoveModifier(AttributeModifier modifier)
        {
            if (modifier == null) return false;

            var attribute = GetAttribute(modifier.TargetAttribute);
            if (attribute == null) return false;

            return attribute.RemoveModifier(modifier);
        }

        /// <summary>
        /// 소스별 모든 수정자 제거
        /// </summary>
        public int RemoveAllModifiersFromSource(object source)
        {
            int totalRemoved = 0;

            foreach (var attribute in attributes)
            {
                totalRemoved += attribute.RemoveModifiersBySource(source);
            }

            return totalRemoved;
        }

        /// <summary>
        /// 모든 수정자 제거
        /// </summary>
        public void ClearAllModifiers()
        {
            foreach (var attribute in attributes)
            {
                attribute.ClearModifiers();
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// 속성 집합 복사
        /// </summary>
        public AttributeSet Clone()
        {
            return new AttributeSet(this);
        }

        /// <summary>
        /// 모든 속성을 기본값으로 리셋
        /// </summary>
        public void ResetAllToBase()
        {
            foreach (var attribute in attributes)
            {
                attribute.ResetToBase();
            }
        }

        /// <summary>
        /// 리소스 속성들을 최대값으로 설정
        /// </summary>
        public void RestoreResources()
        {
            // Health
            var health = GetAttribute(GASConstants.AttributeType.Health);
            var maxHealth = GetAttribute(GASConstants.AttributeType.MaxHealth);
            if (health != null && maxHealth != null)
            {
                health.CurrentValue = maxHealth.CurrentValue;
            }

            // Mana
            var mana = GetAttribute(GASConstants.AttributeType.Mana);
            var maxMana = GetAttribute(GASConstants.AttributeType.MaxMana);
            if (mana != null && maxMana != null)
            {
                mana.CurrentValue = maxMana.CurrentValue;
            }

            // Stamina
            var stamina = GetAttribute(GASConstants.AttributeType.Stamina);
            var maxStamina = GetAttribute(GASConstants.AttributeType.MaxStamina);
            if (stamina != null && maxStamina != null)
            {
                stamina.CurrentValue = maxStamina.CurrentValue;
            }
        }

        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        public void PrintDebugInfo()
        {
            Debug.Log($"[GAS] AttributeSet: {setName}");
            Debug.Log($"  Attributes: {attributes.Count}");

            foreach (var attribute in attributes)
            {
                Debug.Log($"  - {attribute}");
            }
        }

        public override string ToString()
        {
            return $"AttributeSet: {setName} ({attributes.Count} attributes)";
        }
        #endregion

        #region Event Handlers
        private void HandleAttributeValueChanged(BaseAttribute attribute, float oldValue, float newValue)
        {
            OnAttributeValueChanged?.Invoke(attribute, oldValue, newValue);

            // GAS 이벤트 시스템에 전파
            GASEvents.TriggerAttributeChanged(null, attribute.AttributeName, oldValue, newValue);

            // 특수 상태 체크
            if (attribute.IsAtMax)
            {
                GASEvents.TriggerAttributeMaxed(null, attribute.AttributeName);
            }

            if (attribute.IsAtMin)
            {
                GASEvents.TriggerAttributeZero(null, attribute.AttributeName);
            }
        }
        #endregion
    }
}