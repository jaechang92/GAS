// ���� ��ġ: Assets/Scripts/GAS/Learning/Phase1/ModifierTestExample.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAS.Learning.Phase1
{
    /// <summary>
    /// Step 2: AttributeModifier ���� �� �׽�Ʈ
    /// </summary>
    public class ModifierTestExample : MonoBehaviour
    {
        // Modifier �۾� Ÿ��
        public enum ModifierOperation
        {
            Add,        // ������ ���ϱ�
            Multiply,   // ��� ����
            Override    // �� ��ü
        }

        // Modifier Ÿ��
        public enum ModifierType
        {
            Permanent,  // ������
            Temporary,  // �Ͻ���
            Duration    // ���ӽð�
        }

        // Modifier Ŭ����
        [System.Serializable]
        public class AttributeModifier
        {
            public string id;
            public string name;
            public ModifierOperation operation;
            public float value;
            public ModifierType modifierType;
            public float duration; // Duration Ÿ���� ���� ���
            public float remainingTime;
            public int priority; // ���� �켱���� (�������� ���� ����)

            public AttributeModifier(string name, ModifierOperation op, float val, ModifierType type = ModifierType.Temporary, int priority = 0)
            {
                this.id = Guid.NewGuid().ToString();
                this.name = name;
                this.operation = op;
                this.value = val;
                this.modifierType = type;
                this.priority = priority;
                this.duration = 0;
                this.remainingTime = 0;
            }

            public override string ToString()
            {
                string opStr = operation switch
                {
                    ModifierOperation.Add => value >= 0 ? $"+{value}" : $"{value}",
                    ModifierOperation.Multiply => $"x{value}",
                    ModifierOperation.Override => $"={value}",
                    _ => "?"
                };
                return $"{name} ({opStr})";
            }
        }

        // ������ Attribute Ŭ���� (Modifier ����)
        [System.Serializable]
        public class ModifiableAttribute
        {
            [SerializeField] private string name;
            [SerializeField] private float baseValue;
            [SerializeField] private float currentValue;
            [SerializeField] private float minValue;
            [SerializeField] private float maxValue;

            private List<AttributeModifier> modifiers = new List<AttributeModifier>();

            public event Action<float, float> OnValueChanged;
            public event Action<AttributeModifier> OnModifierAdded;
            public event Action<AttributeModifier> OnModifierRemoved;

            public string Name => name;
            public float BaseValue => baseValue;
            public float CurrentValue => currentValue;
            public float MinValue => minValue;
            public float MaxValue => maxValue;
            public float ModifiedValue { get; private set; }

            public ModifiableAttribute(string name, float baseValue, float minValue, float maxValue)
            {
                this.name = name;
                this.baseValue = baseValue;
                this.minValue = minValue;
                this.maxValue = maxValue;
                this.currentValue = baseValue;
                this.ModifiedValue = baseValue;
            }

            // Modifier �߰�
            public void AddModifier(AttributeModifier modifier)
            {
                modifiers.Add(modifier);
                modifiers.Sort((a, b) => a.priority.CompareTo(b.priority));
                RecalculateValue();
                OnModifierAdded?.Invoke(modifier);

                Debug.Log($"[{name}] Modifier �߰�: {modifier}");
            }

            // Modifier ����
            public bool RemoveModifier(AttributeModifier modifier)
            {
                if (modifiers.Remove(modifier))
                {
                    RecalculateValue();
                    OnModifierRemoved?.Invoke(modifier);
                    Debug.Log($"[{name}] Modifier ����: {modifier}");
                    return true;
                }
                return false;
            }

            // ID�� Modifier ����
            public bool RemoveModifierById(string id)
            {
                var modifier = modifiers.FirstOrDefault(m => m.id == id);
                if (modifier != null)
                {
                    return RemoveModifier(modifier);
                }
                return false;
            }

            // ��� Modifier ����
            public void ClearModifiers()
            {
                modifiers.Clear();
                RecalculateValue();
                Debug.Log($"[{name}] ��� Modifier ���ŵ�");
            }

            // ���� �� ���� ���� (���� ���� ����)
            public void SetCurrentValue(float value)
            {
                float oldValue = currentValue;
                currentValue = Mathf.Clamp(value, minValue, maxValue);

                if (Math.Abs(oldValue - currentValue) > 0.01f)
                {
                    OnValueChanged?.Invoke(oldValue, currentValue);
                    Debug.Log($"[{name}] CurrentValue ����: {oldValue:F1} �� {currentValue:F1}");
                }
            }

            // ���� ���� ���ϱ� (���� ���� ����)
            public void AddToCurrentValue(float amount)
            {
                SetCurrentValue(currentValue + amount);
            }

            // ModifiedValue �������� CurrentValue ����
            public void ResetToModifiedValue()
            {
                SetCurrentValue(ModifiedValue);
                Debug.Log($"[{name}] CurrentValue�� ModifiedValue�� ����: {currentValue:F1}");
            }

            // �ۼ�Ʈ �������� (0~1)
            public float GetPercentage()
            {
                if (maxValue - minValue == 0) return 0;
                return (currentValue - minValue) / (maxValue - minValue);
            }

            // �ִ밪 ��� �ۼ�Ʈ�� ����
            public void SetCurrentValueByPercentage(float percentage)
            {
                float value = minValue + (maxValue - minValue) * Mathf.Clamp01(percentage);
                SetCurrentValue(value);
            }

            // �� ���� (Modifier ����)
            private void RecalculateValue()
            {
                float oldModified = ModifiedValue;
                float oldCurrent = currentValue;
                float calcValue = baseValue;

                // Priority ������� Modifier ����
                var sortedModifiers = modifiers.OrderBy(m => m.priority).ToList();

                // 1�ܰ�: Override ���� (������ �ٸ� ��� �� ����)
                var overrideModifier = sortedModifiers.FirstOrDefault(m => m.operation == ModifierOperation.Override);
                if (overrideModifier != null)
                {
                    calcValue = overrideModifier.value;
                }
                else
                {
                    // 2�ܰ�: Add ����
                    float totalAdd = sortedModifiers
                        .Where(m => m.operation == ModifierOperation.Add)
                        .Sum(m => m.value);
                    calcValue += totalAdd;

                    // 3�ܰ�: Multiply ����
                    foreach (var modifier in sortedModifiers.Where(m => m.operation == ModifierOperation.Multiply))
                    {
                        calcValue *= modifier.value;
                    }
                }

                ModifiedValue = calcValue;

                // CurrentValue�� ModifiedValue�� �����ϴ� ��� (�ɼ�)
                // �ּ� �����ϸ� Modifier ���� �� CurrentValue�� �ڵ� ������
                // if (Math.Abs(currentValue - oldModified) < 0.01f)
                // {
                //     currentValue = Mathf.Clamp(ModifiedValue, minValue, maxValue);
                // }

                if (Math.Abs(oldModified - ModifiedValue) > 0.01f)
                {
                    Debug.Log($"[{name}] ����: Base({baseValue:F1}) �� Modified({ModifiedValue:F1}) | Current({currentValue:F1})");
                }
            }

            // Ȱ�� Modifier ���
            public List<AttributeModifier> GetActiveModifiers()
            {
                return new List<AttributeModifier>(modifiers);
            }

            // ������/�� ó�� (���簪 ����)
            public float ApplyDamage(float damage)
            {
                float actualDamage = Mathf.Min(damage, currentValue - minValue);
                AddToCurrentValue(-actualDamage);
                return actualDamage; // ������ ����� ������ ��ȯ
            }

            public float ApplyHeal(float healAmount)
            {
                float actualHeal = Mathf.Min(healAmount, maxValue - currentValue);
                AddToCurrentValue(actualHeal);
                return actualHeal; // ������ ����� �� ��ȯ
            }

            public override string ToString()
            {
                return $"{name}: {currentValue:F1}/{ModifiedValue:F1} (Base: {baseValue:F1}) [{modifiers.Count} modifiers]";
            }
        }

        // �׽�Ʈ�� �Ӽ���
        [Header("�׽�Ʈ �Ӽ�")]
        [SerializeField] private ModifiableAttribute attackPower;
        [SerializeField] private ModifiableAttribute defense;
        [SerializeField] private ModifiableAttribute moveSpeed;

        [Header("Ȱ�� Modifiers")]
        [SerializeField] private List<AttributeModifier> activeModifiers = new List<AttributeModifier>();

        private void Start()
        {
            InitializeAttributes();
            Debug.Log("=== Phase 1 - Step 2: Modifier �׽�Ʈ ���� ===");
            RunModifierTests();
        }

        private void InitializeAttributes()
        {
            attackPower = new ModifiableAttribute("���ݷ�", 10f, 0f, 999f);
            defense = new ModifiableAttribute("����", 5f, 0f, 999f);
            moveSpeed = new ModifiableAttribute("�̵��ӵ�", 5f, 0f, 20f);

            // �̺�Ʈ ����
            attackPower.OnModifierAdded += (mod) => activeModifiers.Add(mod);
            attackPower.OnModifierRemoved += (mod) => activeModifiers.Remove(mod);
            defense.OnModifierAdded += (mod) => activeModifiers.Add(mod);
            defense.OnModifierRemoved += (mod) => activeModifiers.Remove(mod);
        }

        private void RunModifierTests()
        {
            TestScenario1_BasicModifiers();
            TestScenario2_ModifierStacking();
            TestScenario3_PrioritySystem();
            TestScenario4_OverrideModifier();
        }

        // �ó����� 1: �⺻ Modifier �׽�Ʈ
        private void TestScenario1_BasicModifiers()
        {
            Debug.Log("\n========== �ó����� 1: �⺻ Modifier ==========");
            Debug.Log($"�ʱ� ���ݷ�: {attackPower}");

            // ������ ����
            var atkBuff = new AttributeModifier("���ݷ� ����", ModifierOperation.Add, 5f);
            attackPower.AddModifier(atkBuff);
            Debug.Log($"���ݷ� +5 ���� ��: {attackPower}");

            // ��� ����
            var atkMultiplier = new AttributeModifier("���ݷ� ����", ModifierOperation.Multiply, 1.5f);
            attackPower.AddModifier(atkMultiplier);
            Debug.Log($"���ݷ� x1.5 ���� ��: {attackPower}");

            // ���� ����
            attackPower.RemoveModifier(atkBuff);
            Debug.Log($"������ ���� ���� ��: {attackPower}");

            attackPower.ClearModifiers();
            Debug.Log($"��� ���� ���� ��: {attackPower}");
        }

        // �ó����� 2: Modifier ����
        private void TestScenario2_ModifierStacking()
        {
            Debug.Log("\n========== �ó����� 2: Modifier ���� ==========");
            Debug.Log($"�ʱ� ����: {defense}");

            // ���� ���� ���� ����
            var def1 = new AttributeModifier("���� ���� 1", ModifierOperation.Add, 3f);
            var def2 = new AttributeModifier("���� ���� 2", ModifierOperation.Add, 2f);
            var def3 = new AttributeModifier("���� ����", ModifierOperation.Multiply, 2f);

            defense.AddModifier(def1);
            Debug.Log($"����1(+3) ��: {defense}");

            defense.AddModifier(def2);
            Debug.Log($"����2(+2) ��: {defense}");

            defense.AddModifier(def3);
            Debug.Log($"����(x2) ��: {defense}");
            // ���: (5 + 3 + 2) * 2 = 20

            defense.ClearModifiers();
        }

        // �ó����� 3: Priority �ý���
        private void TestScenario3_PrioritySystem()
        {
            Debug.Log("\n========== �ó����� 3: Priority �ý��� ==========");
            Debug.Log($"�ʱ� �̵��ӵ�: {moveSpeed}");

            // Priority�� �ٸ� Modifier��
            var speed1 = new AttributeModifier("�⺻ �Ź�", ModifierOperation.Add, 2f, ModifierType.Temporary, priority: 1);
            var speed2 = new AttributeModifier("������Ʈ", ModifierOperation.Multiply, 1.5f, ModifierType.Temporary, priority: 2);
            var speed3 = new AttributeModifier("�ʱ� ����", ModifierOperation.Add, 1f, ModifierType.Temporary, priority: 0);

            moveSpeed.AddModifier(speed1);
            moveSpeed.AddModifier(speed2);
            moveSpeed.AddModifier(speed3);

            Debug.Log($"��� ���� ���� ��: {moveSpeed}");
            // Priority ����: speed3(0) �� speed1(1) �� speed2(2)
            // ���: (5 + 1 + 2) * 1.5 = 12

            moveSpeed.ClearModifiers();
        }

        // �ó����� 4: Override Modifier
        private void TestScenario4_OverrideModifier()
        {
            Debug.Log("\n========== �ó����� 4: Override Modifier ==========");
            Debug.Log($"�ʱ� ���ݷ�: {attackPower}");

            var buff = new AttributeModifier("�Ϲ� ����", ModifierOperation.Add, 10f);
            attackPower.AddModifier(buff);
            Debug.Log($"�Ϲ� ����(+10) ��: {attackPower}");

            var overrideMod = new AttributeModifier("���� ����", ModifierOperation.Override, 50f);
            attackPower.AddModifier(overrideMod);
            Debug.Log($"Override(=50) ��: {attackPower}");
            // Override�� ������ �ٸ� ��� Modifier ����

            attackPower.RemoveModifier(overrideMod);
            Debug.Log($"Override ���� ��: {attackPower}");
            // �ٽ� �Ϲ� ������ ����

            attackPower.ClearModifiers();

            // �ó����� 5: CurrentValue ���� ���� �׽�Ʈ
            TestScenario5_CurrentValueManipulation();
        }

        // �ó����� 5: CurrentValue ���� ����
        private void TestScenario5_CurrentValueManipulation()
        {
            Debug.Log("\n========== �ó����� 5: CurrentValue ���� ���� ==========");

            // �ʱ� ����
            Debug.Log($"�ʱ� ����: {defense}");
            Debug.Log($"  Base: {defense.BaseValue}, Modified: {defense.ModifiedValue}, Current: {defense.CurrentValue}");

            // Modifier ����
            var defBuff = new AttributeModifier("��� ����", ModifierOperation.Add, 10f);
            defense.AddModifier(defBuff);
            Debug.Log($"���� ���� ��: Base: {defense.BaseValue}, Modified: {defense.ModifiedValue}, Current: {defense.CurrentValue}");

            // CurrentValue ���� ���� (������)
            float damage = defense.ApplyDamage(8f);
            Debug.Log($"8 ������ ���� (����: {damage:F1}): Current: {defense.CurrentValue}");

            // CurrentValue ���� ���� (��)
            float heal = defense.ApplyHeal(5f);
            Debug.Log($"5 �� ���� (����: {heal:F1}): Current: {defense.CurrentValue}");

            // CurrentValue�� ModifiedValue�� ����
            defense.ResetToModifiedValue();
            Debug.Log($"���� ��: Current: {defense.CurrentValue} (Modified: {defense.ModifiedValue})");

            // �ۼ�Ʈ�� ����
            defense.SetCurrentValueByPercentage(0.5f);
            Debug.Log($"50% ����: Current: {defense.CurrentValue} ({defense.GetPercentage():P0})");

            defense.ClearModifiers();
            defense.ResetToModifiedValue();
        }

        // Update���� Duration Ÿ�� Modifier ó��
        private void Update()
        {
            // Duration Ÿ�� Modifier���� �ð� ó��
            var modifiersToRemove = new List<AttributeModifier>();

            foreach (var modifier in activeModifiers.Where(m => m.modifierType == ModifierType.Duration))
            {
                modifier.remainingTime -= Time.deltaTime;
                if (modifier.remainingTime <= 0)
                {
                    modifiersToRemove.Add(modifier);
                }
            }

            // ����� Modifier ����
            foreach (var modifier in modifiersToRemove)
            {
                RemoveModifierFromAttribute(modifier);
            }

            // �׽�Ʈ �Է�
            HandleTestInput();
        }

        private void HandleTestInput()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                var mod = new AttributeModifier("�׽�Ʈ ����", ModifierOperation.Add, 5f);
                attackPower.AddModifier(mod);
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                var mod = new AttributeModifier("�׽�Ʈ ����", ModifierOperation.Multiply, 1.5f);
                attackPower.AddModifier(mod);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                attackPower.ClearModifiers();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                // CurrentValue�� ������
                float damage = attackPower.ApplyDamage(3f);
                Debug.Log($"���ݷ¿� {damage:F1} ������ ����");
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                // CurrentValue�� ��
                float heal = attackPower.ApplyHeal(5f);
                Debug.Log($"���ݷ¿� {heal:F1} �� ����");
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                // CurrentValue�� ModifiedValue�� ����
                attackPower.ResetToModifiedValue();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                // 50%�� ����
                attackPower.SetCurrentValueByPercentage(0.5f);
                Debug.Log($"���ݷ��� 50%�� ����: {attackPower.CurrentValue:F1}");
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintAllAttributes();
            }
        }

        private void RemoveModifierFromAttribute(AttributeModifier modifier)
        {
            attackPower.RemoveModifier(modifier);
            defense.RemoveModifier(modifier);
            moveSpeed.RemoveModifier(modifier);
        }

        private void PrintAllAttributes()
        {
            Debug.Log("\n=== ���� �Ӽ� ���� ===");
            Debug.Log($"���ݷ� - Base: {attackPower.BaseValue:F1} | Modified: {attackPower.ModifiedValue:F1} | Current: {attackPower.CurrentValue:F1} ({attackPower.GetPercentage():P0})");
            Debug.Log($"���� - Base: {defense.BaseValue:F1} | Modified: {defense.ModifiedValue:F1} | Current: {defense.CurrentValue:F1} ({defense.GetPercentage():P0})");
            Debug.Log($"�̵��ӵ� - Base: {moveSpeed.BaseValue:F1} | Modified: {moveSpeed.ModifiedValue:F1} | Current: {moveSpeed.CurrentValue:F1} ({moveSpeed.GetPercentage():P0})");
            Debug.Log($"Ȱ�� Modifier ��: {activeModifiers.Count}");

            if (activeModifiers.Count > 0)
            {
                Debug.Log("Ȱ�� Modifiers:");
                foreach (var mod in activeModifiers)
                {
                    Debug.Log($"  - {mod}");
                }
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 350, 250), "Phase 1 - Modifier �׽�Ʈ");

            int y = 40;
            GUI.Label(new Rect(20, y, 330, 20), $"���ݷ�: {attackPower?.CurrentValue:F1}/{attackPower?.ModifiedValue:F1} (Base: {attackPower?.BaseValue:F1})");
            y += 25;
            GUI.Label(new Rect(20, y, 330, 20), $"����: {defense?.CurrentValue:F1}/{defense?.ModifiedValue:F1} (Base: {defense?.BaseValue:F1})");
            y += 25;
            GUI.Label(new Rect(20, y, 330, 20), $"�̵��ӵ�: {moveSpeed?.CurrentValue:F1}/{moveSpeed?.ModifiedValue:F1} (Base: {moveSpeed?.BaseValue:F1})");
            y += 30;
            GUI.Label(new Rect(20, y, 330, 20), $"Ȱ�� Modifiers: {activeModifiers?.Count ?? 0}��");
            y += 30;

            // ���� ����
            GUI.Label(new Rect(20, y, 330, 30), "=== Modifier ���� ===");
            y += 25;
            GUI.Label(new Rect(20, y, 330, 20), "Q: ���ݷ�+5  W: ���ݷ�x1.5  E: ����������");
            y += 20;
            GUI.Label(new Rect(20, y, 330, 20), "=== CurrentValue ���� ===");
            y += 20;
            GUI.Label(new Rect(20, y, 330, 20), "A: ������  S: ��  D: ����  F: 50%����");
            y += 20;
            GUI.Label(new Rect(20, y, 330, 20), "Space: ���� ���");

            // �� ǥ�� ����
            y += 30;
            GUI.Label(new Rect(20, y, 330, 20), "ǥ��: Current/Modified (Base)");
        }
    }
}