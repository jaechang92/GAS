// ���� ��ġ: Assets/Scripts/GAS/AttributeSystem/AttributeSet.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GAS.Core;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// ���� �Ӽ��� �����ϴ� �Ӽ� ���� Ŭ����
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
        /// �Ӽ��� �߰��Ǿ��� ��
        /// </summary>
        public event Action<BaseAttribute> OnAttributeAdded;

        /// <summary>
        /// �Ӽ��� ���ŵǾ��� ��
        /// </summary>
        public event Action<BaseAttribute> OnAttributeRemoved;

        /// <summary>
        /// �Ӽ����� ����Ǿ��� ��
        /// </summary>
        public event Action<BaseAttribute, float, float> OnAttributeValueChanged;
        #endregion

        #region Properties
        /// <summary>
        /// �Ӽ� ���� �̸�
        /// </summary>
        public string SetName
        {
            get => setName;
            set => setName = value;
        }

        /// <summary>
        /// �Ӽ� ����
        /// </summary>
        public int Count => attributes.Count;

        /// <summary>
        /// �Ӽ� ����Ʈ (�б� ����)
        /// </summary>
        public IReadOnlyList<BaseAttribute> Attributes => attributes.AsReadOnly();
        #endregion

        #region Constructors
        /// <summary>
        /// �⺻ ������
        /// </summary>
        public AttributeSet()
        {
            setName = "New Attribute Set";
            InitializeMaps();
        }

        /// <summary>
        /// �̸� ���� ������
        /// </summary>
        public AttributeSet(string name)
        {
            setName = name;
            InitializeMaps();
        }

        /// <summary>
        /// ���� ������
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
        /// �⺻ �Ӽ� ��Ʈ �ʱ�ȭ (HP, Mana, Stamina)
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
        /// ���� �Ӽ� �ʱ�ȭ
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
        /// �Ӽ� �߰�
        /// </summary>
        public bool AddAttribute(BaseAttribute attribute)
        {
            if (attribute == null) return false;

            // �ߺ� üũ
            if (HasAttribute(attribute.AttributeType))
            {
                Debug.LogWarning($"[GAS] Attribute type {attribute.AttributeType} already exists in set");
                return false;
            }

            attributes.Add(attribute);

            if (attributeMap == null) InitializeMaps();
            attributeMap[attribute.AttributeType] = attribute;
            attributeNameMap[attribute.AttributeName] = attribute;

            // �̺�Ʈ ����
            attribute.OnValueChanged += (oldValue, newValue) => HandleAttributeValueChanged(attribute, oldValue, newValue);

            OnAttributeAdded?.Invoke(attribute);

            return true;
        }

        /// <summary>
        /// �Ӽ� ����
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
        /// ��� �Ӽ� ����
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
        /// Ÿ������ �Ӽ� ��������
        /// </summary>
        public BaseAttribute GetAttribute(GASConstants.AttributeType type)
        {
            if (attributeMap == null) RebuildMaps();

            attributeMap.TryGetValue(type, out BaseAttribute attribute);
            return attribute;
        }

        /// <summary>
        /// �̸����� �Ӽ� ��������
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
        /// �Ӽ� ���� Ȯ��
        /// </summary>
        public bool HasAttribute(GASConstants.AttributeType type)
        {
            if (attributeMap == null) RebuildMaps();
            return attributeMap.ContainsKey(type);
        }

        /// <summary>
        /// �Ӽ��� ��������
        /// </summary>
        public float GetAttributeValue(GASConstants.AttributeType type)
        {
            var attribute = GetAttribute(type);
            return attribute?.CurrentValue ?? 0f;
        }

        /// <summary>
        /// �Ӽ� �⺻�� ��������
        /// </summary>
        public float GetAttributeBaseValue(GASConstants.AttributeType type)
        {
            var attribute = GetAttribute(type);
            return attribute?.BaseValue ?? 0f;
        }
        #endregion

        #region Attribute Modification
        /// <summary>
        /// �Ӽ��� ����
        /// </summary>
        public bool SetAttributeValue(GASConstants.AttributeType type, float value)
        {
            var attribute = GetAttribute(type);
            if (attribute == null) return false;

            attribute.CurrentValue = value;
            return true;
        }

        /// <summary>
        /// �Ӽ� �⺻�� ����
        /// </summary>
        public bool SetAttributeBaseValue(GASConstants.AttributeType type, float value)
        {
            var attribute = GetAttribute(type);
            if (attribute == null) return false;

            attribute.BaseValue = value;
            return true;
        }

        /// <summary>
        /// �Ӽ��� ���ϱ�
        /// </summary>
        public bool AddAttributeValue(GASConstants.AttributeType type, float amount)
        {
            var attribute = GetAttribute(type);
            if (attribute == null) return false;

            attribute.AddValue(amount);
            return true;
        }

        /// <summary>
        /// ������ ����
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
        /// ������ ����
        /// </summary>
        public bool RemoveModifier(AttributeModifier modifier)
        {
            if (modifier == null) return false;

            var attribute = GetAttribute(modifier.TargetAttribute);
            if (attribute == null) return false;

            return attribute.RemoveModifier(modifier);
        }

        /// <summary>
        /// �ҽ��� ��� ������ ����
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
        /// ��� ������ ����
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
        /// �Ӽ� ���� ����
        /// </summary>
        public AttributeSet Clone()
        {
            return new AttributeSet(this);
        }

        /// <summary>
        /// ��� �Ӽ��� �⺻������ ����
        /// </summary>
        public void ResetAllToBase()
        {
            foreach (var attribute in attributes)
            {
                attribute.ResetToBase();
            }
        }

        /// <summary>
        /// ���ҽ� �Ӽ����� �ִ밪���� ����
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
        /// ����� ���� ���
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

            // GAS �̺�Ʈ �ý��ۿ� ����
            GASEvents.TriggerAttributeChanged(null, attribute.AttributeName, oldValue, newValue);

            // Ư�� ���� üũ
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