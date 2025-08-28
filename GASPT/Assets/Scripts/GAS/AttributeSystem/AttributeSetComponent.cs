using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// GameObject�� Attribute �ý����� �����ϴ� ������Ʈ
    /// ���� AttributeSet�� �����ϰ� Modifier�� �����մϴ�
    /// </summary>
    public class AttributeSetComponent : MonoBehaviour, IAttributeComponent
    {
        #region Inner Classes

        /// <summary>
        /// Modifier ���� ������ �����ϴ� �ڵ�
        /// </summary>
        [Serializable]
        public class ModifierHandle
        {
            public Guid id;
            public AttributeModifier modifier;
            public object source; // Effect�� Ability �� Modifier�� �ҽ�
            public float? expireTime; // null�̸� ���� ����

            public ModifierHandle(AttributeModifier modifier, object source = null)
            {
                this.id = Guid.NewGuid();
                this.modifier = modifier;
                this.source = source;
                this.expireTime = null;
            }
        }

        /// <summary>
        /// ���� Attribute �ν��Ͻ� ����
        /// </summary>
        [Serializable]
        public class AttributeInstance
        {
            public BaseAttribute baseAttribute;
            public float baseValue;
            private List<ModifierHandle> modifiers = new List<ModifierHandle>();
            private float cachedValue;
            private bool isDirty = true;

            public AttributeInstance(BaseAttribute attribute)
            {
                this.baseAttribute = attribute;
                this.baseValue = attribute.BaseValue;
                this.cachedValue = baseValue;
            }

            /// <summary>
            /// Modifier �߰�
            /// </summary>
            public ModifierHandle AddModifier(AttributeModifier modifier, object source = null)
            {
                var handle = new ModifierHandle(modifier, source);
                modifiers.Add(handle);
                isDirty = true;
                return handle;
            }

            /// <summary>
            /// Modifier ����
            /// </summary>
            public bool RemoveModifier(Guid handleId)
            {
                var removed = modifiers.RemoveAll(m => m.id == handleId) > 0;
                if (removed)
                {
                    isDirty = true;
                }
                return removed;
            }

            /// <summary>
            /// �ҽ����� ��� Modifier ����
            /// </summary>
            public int RemoveModifiersBySource(object source)
            {
                var removed = modifiers.RemoveAll(m => m.source == source);
                if (removed > 0)
                {
                    isDirty = true;
                }
                return removed;
            }

            /// <summary>
            /// ����� Modifier ����
            /// </summary>
            public int CleanupExpiredModifiers(float currentTime)
            {
                var removed = modifiers.RemoveAll(m =>
                    m.expireTime.HasValue && m.expireTime.Value <= currentTime);
                if (removed > 0)
                {
                    isDirty = true;
                }
                return removed;
            }

            /// <summary>
            /// ���� �� ��� (Base -> Add -> Multiply -> Override)
            /// </summary>
            public float CalculateFinalValue()
            {
                if (!isDirty)
                {
                    return cachedValue;
                }

                // Override�� ������ ���� ���� �켱������ Override �� ���
                var overrideModifiers = modifiers
                    .Where(m => m.modifier.Operation == ModifierOperation.Override)
                    .OrderByDescending(m => m.modifier.Priority)
                    .ToList();

                if (overrideModifiers.Any())
                {
                    cachedValue = overrideModifiers.First().modifier.Value;
                    isDirty = false;
                    return cachedValue;
                }

                // Base ������ ����
                float finalValue = baseValue;

                // Add ���� (Priority �������)
                var addModifiers = modifiers
                    .Where(m => m.modifier.Operation == ModifierOperation.Add)
                    .OrderBy(m => m.modifier.Priority);

                foreach (var mod in addModifiers)
                {
                    finalValue += mod.modifier.Value;
                }

                // Multiply ���� (Priority �������)
                var multiplyModifiers = modifiers
                    .Where(m => m.modifier.Operation == ModifierOperation.Multiply)
                    .OrderBy(m => m.modifier.Priority);

                foreach (var mod in multiplyModifiers)
                {
                    finalValue *= mod.modifier.Value;
                }

                // Min/Max ���� ����
                if (baseAttribute.HasMinValue)
                {
                    finalValue = Mathf.Max(finalValue, baseAttribute.MinValue);
                }
                if (baseAttribute.HasMaxValue)
                {
                    finalValue = Mathf.Min(finalValue, baseAttribute.MaxValue);
                }

                cachedValue = finalValue;
                isDirty = false;
                return cachedValue;
            }

            public List<ModifierHandle> GetModifiers() => new List<ModifierHandle>(modifiers);

            public void SetDirty() => isDirty = true;
        }

        #endregion

        #region Events

        public event Action<AttributeType, float, float> OnAttributeChanged;
        public event Action<AttributeType, AttributeModifier> OnModifierAdded;
        public event Action<AttributeType, AttributeModifier> OnModifierRemoved;

        #endregion

        #region Serialized Fields

        [Header("Configuration")]
        [SerializeField] private List<AttributeSetData> defaultAttributeData;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool logAttributeChanges = false;

        #endregion

        #region Private Fields

        private Dictionary<Type, AttributeSet> attributeSets = new Dictionary<Type, AttributeSet>();
        private Dictionary<AttributeType, AttributeInstance> attributes = new Dictionary<AttributeType, AttributeInstance>();
        private float lastCleanupTime;
        private const float CLEANUP_INTERVAL = 1.0f; // 1�ʸ��� ����� Modifier ����

        #endregion

        #region Properties

        /// <summary>
        /// ��ϵ� ��� AttributeType ���
        /// </summary>
        public IEnumerable<AttributeType> RegisteredAttributes => attributes.Keys;

        /// <summary>
        /// ��ϵ� AttributeSet Ÿ�� ���
        /// </summary>
        public IEnumerable<Type> RegisteredSetTypes => attributeSets.Keys;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeDefaultAttributes();
            RegisterToGASManager();
        }

        private void Start()
        {
            // GAS �̺�Ʈ ����
            GAS.Core.GASEvents.OnGlobalAttributeQuery += HandleGlobalAttributeQuery;
            
        }

        private void Update()
        {
            // �ֱ������� ����� Modifier ����
            if (Time.time - lastCleanupTime > CLEANUP_INTERVAL)
            {
                CleanupExpiredModifiers();
                lastCleanupTime = Time.time;
            }
        }

        private void OnDestroy()
        {
            // GAS �̺�Ʈ ���� ����
            GAS.Core.GASEvents.OnGlobalAttributeQuery -= HandleGlobalAttributeQuery;
            UnregisterFromGASManager();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// �⺻ Attribute �ʱ�ȭ
        /// </summary>
        private void InitializeDefaultAttributes()
        {
            if (defaultAttributeData == null || defaultAttributeData.Count == 0)
            {
                Debug.LogWarning($"[GAS] {gameObject.name}: No default AttributeSetData configured");
                return;
            }

            foreach (var data in defaultAttributeData)
            {
                if (data != null)
                {
                    InitializeFromData(data);
                }
            }
        }

        /// <summary>
        /// AttributeSetData�κ��� �ʱ�ȭ
        /// </summary>
        public void InitializeFromData(AttributeSetData data)
        {
            if (data == null)
            {
                Debug.LogError("[GAS] AttributeSetData is null");
                return;
            }

            // AttributeSet ���� �� ���
            var attributeSet = data.CreateAttributeSet();
            if (attributeSet != null)
            {
                RegisterAttributeSet(attributeSet);
            }
        }

        /// <summary>
        /// AttributeSet ���
        /// </summary>
        public void RegisterAttributeSet(AttributeSet attributeSet)
        {
            if (attributeSet == null)
            {
                Debug.LogError("[GAS] Cannot register null AttributeSet");
                return;
            }

            var setType = attributeSet.GetType();

            // �̹� ��ϵ� Ÿ������ Ȯ��
            if (attributeSets.ContainsKey(setType))
            {
                Debug.LogWarning($"[GAS] AttributeSet type {setType.Name} is already registered");
                return;
            }

            // AttributeSet ���
            attributeSets[setType] = attributeSet;

            // ���� Attribute ���
            foreach (var attribute in attributeSet.GetAllAttributes())
            {
                if (attribute != null)
                {
                    RegisterAttribute(attribute);
                }
            }

            Debug.Log($"[GAS] Registered AttributeSet: {setType.Name} with {attributeSet.GetAllAttributes().Count()} attributes");
        }

        /// <summary>
        /// ���� Attribute ���
        /// </summary>
        private void RegisterAttribute(BaseAttribute attribute)
        {
            if (!attributes.ContainsKey(attribute.AttributeType))
            {
                attributes[attribute.AttributeType] = new AttributeInstance(attribute);

                if (logAttributeChanges)
                {
                    Debug.Log($"[GAS] Registered Attribute: {attribute.AttributeType} (Base: {attribute.BaseValue})");
                }
            }
        }

        #endregion

        #region Modifier Management

        /// <summary>
        /// Modifier �߰�
        /// </summary>
        public Guid AddModifier(AttributeType type, AttributeModifier modifier, object source = null)
        {
            if (!attributes.TryGetValue(type, out var instance))
            {
                Debug.LogWarning($"[GAS] Attribute type {type} not found");
                return Guid.Empty;
            }

            float oldValue = instance.CalculateFinalValue();
            var handle = instance.AddModifier(modifier, source);
            float newValue = instance.CalculateFinalValue();

            // �̺�Ʈ �߻�
            OnModifierAdded?.Invoke(type, modifier);

            if (!Mathf.Approximately(oldValue, newValue))
            {
                OnAttributeChanged?.Invoke(type, oldValue, newValue);

                if (logAttributeChanges)
                {
                    Debug.Log($"[GAS] {type} changed: {oldValue:F2} -> {newValue:F2} (Modifier: {modifier.Operation} {modifier.Value:F2})");
                }
            }

            return handle.id;
        }

        /// <summary>
        /// Modifier ����
        /// </summary>
        public bool RemoveModifier(AttributeType type, Guid modifierId)
        {
            if (!attributes.TryGetValue(type, out var instance))
            {
                return false;
            }

            float oldValue = instance.CalculateFinalValue();

            // ������ Modifier ���� ��������
            var modifierToRemove = instance.GetModifiers()
                .FirstOrDefault(m => m.id == modifierId);

            if (modifierToRemove == null)
            {
                return false;
            }

            bool removed = instance.RemoveModifier(modifierId);

            if (removed)
            {
                float newValue = instance.CalculateFinalValue();

                // �̺�Ʈ �߻�
                OnModifierRemoved?.Invoke(type, modifierToRemove.modifier);

                if (!Mathf.Approximately(oldValue, newValue))
                {
                    OnAttributeChanged?.Invoke(type, oldValue, newValue);

                    if (logAttributeChanges)
                    {
                        Debug.Log($"[GAS] {type} changed: {oldValue:F2} -> {newValue:F2} (Modifier removed)");
                    }
                }
            }

            return removed;
        }

        /// <summary>
        /// �ҽ����� ��� Modifier ����
        /// </summary>
        public void RemoveAllModifiersFromSource(object source)
        {
            foreach (var kvp in attributes)
            {
                var type = kvp.Key;
                var instance = kvp.Value;

                float oldValue = instance.CalculateFinalValue();
                int removedCount = instance.RemoveModifiersBySource(source);

                if (removedCount > 0)
                {
                    float newValue = instance.CalculateFinalValue();

                    if (!Mathf.Approximately(oldValue, newValue))
                    {
                        OnAttributeChanged?.Invoke(type, oldValue, newValue);

                        if (logAttributeChanges)
                        {
                            Debug.Log($"[GAS] {type}: Removed {removedCount} modifiers from source");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �ð� ������ �ִ� Modifier �߰�
        /// </summary>
        public Guid AddTimedModifier(AttributeType type, AttributeModifier modifier, float duration, object source = null)
        {
            var modifierId = AddModifier(type, modifier, source);

            if (modifierId != Guid.Empty && attributes.TryGetValue(type, out var instance))
            {
                var handle = instance.GetModifiers().FirstOrDefault(m => m.id == modifierId);
                if (handle != null)
                {
                    handle.expireTime = Time.time + duration;
                }
            }

            return modifierId;
        }

        /// <summary>
        /// ����� Modifier ����
        /// </summary>
        private void CleanupExpiredModifiers()
        {
            foreach (var kvp in attributes)
            {
                var type = kvp.Key;
                var instance = kvp.Value;

                float oldValue = instance.CalculateFinalValue();
                int cleanedCount = instance.CleanupExpiredModifiers(Time.time);

                if (cleanedCount > 0)
                {
                    float newValue = instance.CalculateFinalValue();

                    if (!Mathf.Approximately(oldValue, newValue))
                    {
                        OnAttributeChanged?.Invoke(type, oldValue, newValue);

                        if (logAttributeChanges)
                        {
                            Debug.Log($"[GAS] {type}: Cleaned up {cleanedCount} expired modifiers");
                        }
                    }
                }
            }
        }

        #endregion

        #region Attribute Access

        /// <summary>
        /// Attribute �� ��������
        /// </summary>
        public float GetAttributeValue(AttributeType type)
        {
            if (attributes.TryGetValue(type, out var instance))
            {
                return instance.CalculateFinalValue();
            }

            Debug.LogWarning($"[GAS] Attribute type {type} not found");
            return 0f;
        }

        /// <summary>
        /// Attribute Base �� ��������
        /// </summary>
        public float GetAttributeBaseValue(AttributeType type)
        {
            if (attributes.TryGetValue(type, out var instance))
            {
                return instance.baseValue;
            }

            return 0f;
        }

        /// <summary>
        /// Attribute Base �� ����
        /// </summary>
        public void SetAttributeBaseValue(AttributeType type, float value)
        {
            if (!attributes.TryGetValue(type, out var instance))
            {
                Debug.LogWarning($"[GAS] Attribute type {type} not found");
                return;
            }

            float oldValue = instance.CalculateFinalValue();
            instance.baseValue = value;
            instance.SetDirty();
            float newValue = instance.CalculateFinalValue();

            if (!Mathf.Approximately(oldValue, newValue))
            {
                OnAttributeChanged?.Invoke(type, oldValue, newValue);

                if (logAttributeChanges)
                {
                    Debug.Log($"[GAS] {type} base value changed: {oldValue:F2} -> {newValue:F2}");
                }
            }
        }

        /// <summary>
        /// Attribute ���� ���� Ȯ��
        /// </summary>
        public bool HasAttribute(AttributeType type)
        {
            return attributes.ContainsKey(type);
        }

        /// <summary>
        /// AttributeSet ��������
        /// </summary>
        public T GetAttributeSet<T>() where T : AttributeSet
        {
            var type = typeof(T);
            if (attributeSets.TryGetValue(type, out var set))
            {
                return set as T;
            }
            return null;
        }

        /// <summary>
        /// Ư�� Attribute�� ��� Modifier ��������
        /// </summary>
        public List<AttributeModifier> GetModifiers(AttributeType type)
        {
            if (attributes.TryGetValue(type, out var instance))
            {
                return instance.GetModifiers().Select(h => h.modifier).ToList();
            }
            return new List<AttributeModifier>();
        }

        #endregion

        #region Utility

        /// <summary>
        /// ��� Attribute�� �ʱⰪ���� ����
        /// </summary>
        public void ResetAllAttributes()
        {
            foreach (var kvp in attributes)
            {
                var instance = kvp.Value;
                float oldValue = instance.CalculateFinalValue();

                // ��� Modifier ����
                instance.GetModifiers().Clear();
                instance.baseValue = instance.baseAttribute.BaseValue;
                instance.SetDirty();

                float newValue = instance.CalculateFinalValue();

                if (!Mathf.Approximately(oldValue, newValue))
                {
                    OnAttributeChanged?.Invoke(kvp.Key, oldValue, newValue);
                }
            }

            Debug.Log("[GAS] All attributes reset to base values");
        }

        /// <summary>
        /// ��� �Ͻ��� Modifier ����
        /// </summary>
        public void ClearAllTemporaryModifiers()
        {
            foreach (var kvp in attributes)
            {
                var instance = kvp.Value;
                float oldValue = instance.CalculateFinalValue();

                // expireTime�� ������ ��� Modifier ����
                instance.GetModifiers().RemoveAll(m => m.expireTime.HasValue);
                instance.SetDirty();

                float newValue = instance.CalculateFinalValue();

                if (!Mathf.Approximately(oldValue, newValue))
                {
                    OnAttributeChanged?.Invoke(kvp.Key, oldValue, newValue);
                }
            }
        }

        /// <summary>
        /// Attribute ������ ���ڿ��� ���
        /// </summary>
        public string GetAttributeInfo()
        {
            var info = $"[{gameObject.name} Attributes]\n";

            foreach (var kvp in attributes)
            {
                var type = kvp.Key;
                var instance = kvp.Value;
                var value = instance.CalculateFinalValue();
                var modifierCount = instance.GetModifiers().Count;

                info += $"  {type}: {value:F2} (Base: {instance.baseValue:F2}, Modifiers: {modifierCount})\n";
            }

            return info;
        }

        #endregion

        #region GAS Integration

        private void RegisterToGASManager()
        {
            // GASManager�� ��� (�ʿ��)
            var gasManager = GAS.Core.GASManager.Instance;
            if (gasManager != null)
            {
                // �ʿ��� ��� GASManager�� ������Ʈ ���
            }
        }

        private void UnregisterFromGASManager()
        {
            // GASManager���� ��� ���� (�ʿ��)
            var gasManager = GAS.Core.GASManager.Instance;
            if (gasManager != null)
            {
                // �ʿ��� ��� GASManager���� ������Ʈ ��� ����
            }
        }

        private void HandleGlobalAttributeQuery(GameObject target, AttributeType type)
        {
            // �۷ι� Attribute ���� ó��
            if (target == gameObject && HasAttribute(type))
            {
                float value = GetAttributeValue(type);
                Debug.Log($"[GAS] Query result for {type}: {value:F2}");
            }
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            if (!showDebugInfo)
                return;

            // Scene �信 Attribute ���� ǥ��
            var position = transform.position + Vector3.up * 2f;
            UnityEditor.Handles.Label(position, GetAttributeInfo());
        }

        #endregion
    }

    /// <summary>
    /// AttributeComponent �������̽� (Effect System���� ������)
    /// </summary>
    public interface IAttributeComponent
    {
        float GetAttributeValue(AttributeType type);
        Guid AddModifier(AttributeType type, AttributeModifier modifier, object source = null);
        bool RemoveModifier(AttributeType type, Guid modifierId);
        void RemoveAllModifiersFromSource(object source);
        bool HasAttribute(AttributeType type);

        event Action<AttributeType, float, float> OnAttributeChanged;
    }
}