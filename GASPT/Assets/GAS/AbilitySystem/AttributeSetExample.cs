// ���� ��ġ: Assets/Scripts/GAS/Learning/Phase1/AttributeSetExample.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAS.Learning.Phase1
{
    /// <summary>
    /// Step 3: AttributeSet ���� �ý���
    /// </summary>
    public class AttributeSetExample : MonoBehaviour
    {
        // Attribute Ÿ�� enum
        public enum AttributeType
        {
            // �⺻ �Ӽ�
            Health,
            MaxHealth,
            Mana,
            MaxMana,
            Stamina,
            MaxStamina,

            // ���� �Ӽ�
            AttackPower,
            Defense,
            CriticalChance,
            CriticalDamage,

            // �̵� �Ӽ�
            MoveSpeed,
            JumpPower,

            // ���� �Ӽ�
            FireResistance,
            IceResistance,
            PoisonResistance
        }

        // AttributeSet Ŭ���� - ���� Attribute�� ����
        [Serializable]
        public class AttributeSet
        {
            [SerializeField]
            private SerializableDictionary<AttributeType, ModifierTestExample.ModifiableAttribute> attributes;

            public event Action<AttributeType, float, float> OnAnyAttributeChanged;

            public AttributeSet()
            {
                attributes = new SerializableDictionary<AttributeType, ModifierTestExample.ModifiableAttribute>();
            }

            // Attribute �ʱ�ȭ
            public void InitializeAttribute(AttributeType type, float baseValue, float minValue = 0, float maxValue = 999)
            {
                var attribute = new ModifierTestExample.ModifiableAttribute(
                    type.ToString(),
                    baseValue,
                    minValue,
                    maxValue
                );

                attribute.OnValueChanged += (oldVal, newVal) =>
                {
                    OnAnyAttributeChanged?.Invoke(type, oldVal, newVal);
                };

                attributes[type] = attribute;
                Debug.Log($"[AttributeSet] {type} �ʱ�ȭ: {baseValue}");
            }

            // Attribute ��������
            public ModifierTestExample.ModifiableAttribute GetAttribute(AttributeType type)
            {
                if (attributes.TryGetValue(type, out var attribute))
                {
                    return attribute;
                }
                Debug.LogWarning($"[AttributeSet] {type} �Ӽ��� ã�� �� �����ϴ�!");
                return null;
            }

            // �� ��������
            public float GetAttributeValue(AttributeType type)
            {
                var attr = GetAttribute(type);
                return attr?.CurrentValue ?? 0f;
            }

            // �⺻�� ��������
            public float GetAttributeBaseValue(AttributeType type)
            {
                var attr = GetAttribute(type);
                return attr?.BaseValue ?? 0f;
            }

            // Modifier ����
            public void ApplyModifier(AttributeType type, ModifierTestExample.AttributeModifier modifier)
            {
                var attr = GetAttribute(type);
                attr?.AddModifier(modifier);
            }

            // ���� �Ӽ��� Modifier ����
            public void ApplyModifiersToMultiple(AttributeType[] types, ModifierTestExample.AttributeModifier modifier)
            {
                foreach (var type in types)
                {
                    ApplyModifier(type, modifier);
                }
            }

            // ��� �Ӽ� ����
            public string GetAllAttributesInfo()
            {
                var info = "=== AttributeSet Status ===\n";
                foreach (var kvp in attributes.OrderBy(x => x.Key.ToString()))
                {
                    info += $"{kvp.Key}: {kvp.Value.CurrentValue:F1} (Base: {kvp.Value.BaseValue:F1})\n";
                }
                return info;
            }

            // ���� ���� ��� (�Ļ� �Ӽ�)
            public float CalculateEffectiveDamage(float baseDamage)
            {
                float attackPower = GetAttributeValue(AttributeType.AttackPower);
                float critChance = GetAttributeValue(AttributeType.CriticalChance) / 100f;
                float critDamage = GetAttributeValue(AttributeType.CriticalDamage) / 100f;

                // ũ��Ƽ�� ����
                bool isCritical = UnityEngine.Random.value < critChance;
                float damage = baseDamage + attackPower;

                if (isCritical)
                {
                    damage *= (1f + critDamage);
                    Debug.Log($"ũ��Ƽ��! ������: {damage:F1}");
                }

                return damage;
            }

            public float CalculateDamageReduction(float incomingDamage)
            {
                float defense = GetAttributeValue(AttributeType.Defense);
                // ������ ���� ����: ���� / (���� + 100)
                float reduction = defense / (defense + 100f);
                return incomingDamage * (1f - reduction);
            }
        }

        // ScriptableObject�� AttributeSet ������ ����
        [CreateAssetMenu(fileName = "AttributeSetData", menuName = "GAS/Learning/AttributeSetData")]
        public class AttributeSetData : ScriptableObject
        {
            [Serializable]
            public class AttributeDefinition
            {
                public AttributeType type;
                public float baseValue;
                public float minValue;
                public float maxValue;
            }

            public List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>();
        }

        [Header("AttributeSet �׽�Ʈ")]
        [SerializeField] private AttributeSet playerAttributes;
        [SerializeField] private AttributeSet enemyAttributes;

        [Header("�׽�Ʈ ����")]
        [SerializeField] private float testDamageAmount = 20f;
        [SerializeField] private bool showDebugInfo = true;

        private List<ModifierTestExample.AttributeModifier> temporaryBuffs = new List<ModifierTestExample.AttributeModifier>();

        private void Start()
        {
            InitializeAttributeSets();
            SubscribeToEvents();

            Debug.Log("=== Phase 1 - Step 3: AttributeSet �׽�Ʈ ���� ===");
            RunAttributeSetTests();
        }

        private void InitializeAttributeSets()
        {
            // �÷��̾� AttributeSet �ʱ�ȭ
            playerAttributes = new AttributeSet();

            // �⺻ �Ӽ�
            playerAttributes.InitializeAttribute(AttributeType.Health, 100f, 0f, 100f);
            playerAttributes.InitializeAttribute(AttributeType.MaxHealth, 100f, 50f, 200f);
            playerAttributes.InitializeAttribute(AttributeType.Mana, 50f, 0f, 50f);
            playerAttributes.InitializeAttribute(AttributeType.MaxMana, 50f, 20f, 100f);
            playerAttributes.InitializeAttribute(AttributeType.Stamina, 100f, 0f, 100f);
            playerAttributes.InitializeAttribute(AttributeType.MaxStamina, 100f, 50f, 150f);

            // ���� �Ӽ�
            playerAttributes.InitializeAttribute(AttributeType.AttackPower, 10f, 0f, 999f);
            playerAttributes.InitializeAttribute(AttributeType.Defense, 5f, 0f, 999f);
            playerAttributes.InitializeAttribute(AttributeType.CriticalChance, 10f, 0f, 100f);
            playerAttributes.InitializeAttribute(AttributeType.CriticalDamage, 50f, 0f, 500f);

            // �̵� �Ӽ�
            playerAttributes.InitializeAttribute(AttributeType.MoveSpeed, 5f, 0f, 20f);
            playerAttributes.InitializeAttribute(AttributeType.JumpPower, 10f, 0f, 30f);

            // ���� �Ӽ�
            playerAttributes.InitializeAttribute(AttributeType.FireResistance, 0f, -100f, 100f);
            playerAttributes.InitializeAttribute(AttributeType.IceResistance, 0f, -100f, 100f);
            playerAttributes.InitializeAttribute(AttributeType.PoisonResistance, 0f, -100f, 100f);

            // �� AttributeSet �ʱ�ȭ (�����ϰ�)
            enemyAttributes = new AttributeSet();
            enemyAttributes.InitializeAttribute(AttributeType.Health, 50f, 0f, 50f);
            enemyAttributes.InitializeAttribute(AttributeType.AttackPower, 5f, 0f, 999f);
            enemyAttributes.InitializeAttribute(AttributeType.Defense, 2f, 0f, 999f);
        }

        private void SubscribeToEvents()
        {
            playerAttributes.OnAnyAttributeChanged += (type, oldVal, newVal) =>
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[Player] {type}: {oldVal:F1} �� {newVal:F1}");
                }

                // Ư�� �Ӽ� ���濡 ���� �߰� ó��
                switch (type)
                {
                    case AttributeType.Health:
                        if (newVal <= 0)
                        {
                            Debug.LogWarning("�÷��̾� ���!");
                        }
                        else if (newVal < 30f && oldVal >= 30f)
                        {
                            Debug.LogWarning("ü�� ����!");
                        }
                        break;

                    case AttributeType.MaxHealth:
                        // MaxHealth ���� �� ���� Health ���� ����
                        var health = playerAttributes.GetAttribute(AttributeType.Health);
                        if (health != null && oldVal > 0)
                        {
                            float ratio = health.CurrentValue / oldVal;
                            health.SetCurrentValue(newVal * ratio);
                        }
                        break;
                }
            };
        }

        private void RunAttributeSetTests()
        {
            TestScenario1_BasicOperations();
            TestScenario2_BuffDebuff();
            TestScenario3_CombatCalculation();
            TestScenario4_AttributeRelationships();
        }

        // �ó����� 1: �⺻ �۾�
        private void TestScenario1_BasicOperations()
        {
            Debug.Log("\n========== �ó����� 1: �⺻ AttributeSet �۾� ==========");

            Debug.Log("�ʱ� �÷��̾� �Ӽ�:");
            Debug.Log(playerAttributes.GetAllAttributesInfo());

            // ������ �ޱ�
            var health = playerAttributes.GetAttribute(AttributeType.Health);
            health.AddToCurrentValue(-30f);
            Debug.Log($"30 ������ �� ü��: {health.CurrentValue}");

            // ���� ���
            var mana = playerAttributes.GetAttribute(AttributeType.Mana);
            mana.AddToCurrentValue(-20f);
            Debug.Log($"20 ���� ��� ��: {mana.CurrentValue}");

            // ȸ��
            health.AddToCurrentValue(20f);
            mana.AddToCurrentValue(15f);
            Debug.Log($"ȸ�� �� - ü��: {health.CurrentValue}, ����: {mana.CurrentValue}");
        }

        // �ó����� 2: ����/�����
        private void TestScenario2_BuffDebuff()
        {
            Debug.Log("\n========== �ó����� 2: ����/����� �ý��� ==========");

            // ���ݷ�, ���� ���� ����
            var powerBuff = new ModifierTestExample.AttributeModifier(
                "�Ŀ� ����",
                ModifierTestExample.ModifierOperation.Add,
                5f
            );

            playerAttributes.ApplyModifiersToMultiple(
                new[] { AttributeType.AttackPower, AttributeType.Defense },
                powerBuff
            );

            Debug.Log($"�Ŀ� ���� ��:");
            Debug.Log($"���ݷ�: {playerAttributes.GetAttributeValue(AttributeType.AttackPower)}");
            Debug.Log($"����: {playerAttributes.GetAttributeValue(AttributeType.Defense)}");

            // �̵��ӵ� �����
            var slowDebuff = new ModifierTestExample.AttributeModifier(
                "���ο�",
                ModifierTestExample.ModifierOperation.Multiply,
                0.5f
            );

            playerAttributes.ApplyModifier(AttributeType.MoveSpeed, slowDebuff);
            Debug.Log($"���ο� �� �̵��ӵ�: {playerAttributes.GetAttributeValue(AttributeType.MoveSpeed)}");

            temporaryBuffs.Add(powerBuff);
            temporaryBuffs.Add(slowDebuff);
        }

        // �ó����� 3: ���� ���
        private void TestScenario3_CombatCalculation()
        {
            Debug.Log("\n========== �ó����� 3: ���� ��� ==========");

            // �÷��̾� ����
            float playerDamage = playerAttributes.CalculateEffectiveDamage(10f);
            Debug.Log($"�÷��̾� �⺻ ���� ������: {playerDamage:F1}");

            // �� ��� ���
            float actualDamage = enemyAttributes.CalculateDamageReduction(playerDamage);
            Debug.Log($"�� ���� ���� �� ���� ������: {actualDamage:F1}");

            var enemyHealth = enemyAttributes.GetAttribute(AttributeType.Health);
            enemyHealth.AddToCurrentValue(-actualDamage);
            Debug.Log($"�� ���� ü��: {enemyHealth.CurrentValue:F1}");

            // ũ��Ƽ�� �׽�Ʈ
            Debug.Log("\nũ��Ƽ�� Ȯ�� �׽�Ʈ (5ȸ):");
            for (int i = 0; i < 5; i++)
            {
                float damage = playerAttributes.CalculateEffectiveDamage(10f);
                Debug.Log($"  ���� {i + 1}: {damage:F1} ������");
            }
        }

        // �ó����� 4: �Ӽ� �� ����
        private void TestScenario4_AttributeRelationships()
        {
            Debug.Log("\n========== �ó����� 4: �Ӽ� �� ���� ==========");

            // MaxHealth ���� (���� ü�� ���� ����)
            var maxHealthBuff = new ModifierTestExample.AttributeModifier(
                "ü�� ����",
                ModifierTestExample.ModifierOperation.Add,
                50f
            );

            float healthBefore = playerAttributes.GetAttributeValue(AttributeType.Health);
            float maxHealthBefore = playerAttributes.GetAttributeValue(AttributeType.MaxHealth);

            playerAttributes.ApplyModifier(AttributeType.MaxHealth, maxHealthBuff);

            float healthAfter = playerAttributes.GetAttributeValue(AttributeType.Health);
            float maxHealthAfter = playerAttributes.GetAttributeValue(AttributeType.MaxHealth);

            Debug.Log($"MaxHealth ���� ��: HP {healthBefore}/{maxHealthBefore}");
            Debug.Log($"MaxHealth ���� ��: HP {healthAfter}/{maxHealthAfter}");

            // ���� �Ӽ� �׽�Ʈ
            var fireResistBuff = new ModifierTestExample.AttributeModifier(
                "ȭ�� ����",
                ModifierTestExample.ModifierOperation.Add,
                25f
            );

            playerAttributes.ApplyModifier(AttributeType.FireResistance, fireResistBuff);
            Debug.Log($"ȭ�� ����: {playerAttributes.GetAttributeValue(AttributeType.FireResistance)}%");
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // �׽�Ʈ �Է�
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SimulateCombat();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ApplyRandomBuff();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ClearAllBuffs();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log(playerAttributes.GetAllAttributesInfo());
            }
        }

        private void SimulateCombat()
        {
            Debug.Log("\n=== ���� �ùķ��̼� ===");

            // �÷��̾ ���� ����
            float damage = playerAttributes.CalculateEffectiveDamage(testDamageAmount);
            float reducedDamage = enemyAttributes.CalculateDamageReduction(damage);

            var enemyHealth = enemyAttributes.GetAttribute(AttributeType.Health);
            enemyHealth.AddToCurrentValue(-reducedDamage);

            Debug.Log($"�÷��̾� ����: {damage:F1} �� {reducedDamage:F1} (��� ����)");
            Debug.Log($"�� ü��: {enemyHealth.CurrentValue:F1}/{enemyHealth.BaseValue:F1}");

            // ���� �ݰ�
            float enemyDamage = enemyAttributes.GetAttributeValue(AttributeType.AttackPower);
            float playerReducedDamage = playerAttributes.CalculateDamageReduction(enemyDamage);

            var playerHealth = playerAttributes.GetAttribute(AttributeType.Health);
            playerHealth.AddToCurrentValue(-playerReducedDamage);

            Debug.Log($"�� �ݰ�: {enemyDamage:F1} �� {playerReducedDamage:F1} (��� ����)");
            Debug.Log($"�÷��̾� ü��: {playerHealth.CurrentValue:F1}/{playerHealth.BaseValue:F1}");
        }

        private void ApplyRandomBuff()
        {
            var buffTypes = new[]
            {
                (AttributeType.AttackPower, "���ݷ�", 5f),
                (AttributeType.Defense, "����", 3f),
                (AttributeType.MoveSpeed, "�̵��ӵ�", 2f),
                (AttributeType.CriticalChance, "ũ��Ƽ��", 10f)
            };

            var randomBuff = buffTypes[UnityEngine.Random.Range(0, buffTypes.Length)];

            var modifier = new ModifierTestExample.AttributeModifier(
                $"{randomBuff.Item2} ����",
                ModifierTestExample.ModifierOperation.Add,
                randomBuff.Item3
            );

            playerAttributes.ApplyModifier(randomBuff.Item1, modifier);
            temporaryBuffs.Add(modifier);

            Debug.Log($"���� ���� ����: {randomBuff.Item2} +{randomBuff.Item3}");
        }

        private void ClearAllBuffs()
        {
            foreach (var buff in temporaryBuffs)
            {
                // ��� �Ӽ����� ���� ���� �õ�
                foreach (AttributeType type in Enum.GetValues(typeof(AttributeType)))
                {
                    var attr = playerAttributes.GetAttribute(type);
                    attr?.RemoveModifier(buff);
                }
            }
            temporaryBuffs.Clear();
            Debug.Log("��� ���� ���ŵ�");
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 400, 250), "Phase 1 - AttributeSet �׽�Ʈ");

            int y = 40;

            // �÷��̾� �ֿ� �Ӽ�
            GUI.Label(new Rect(20, y, 180, 20), "=== �÷��̾� ===");
            y += 25;

            var health = playerAttributes?.GetAttribute(AttributeType.Health);
            var mana = playerAttributes?.GetAttribute(AttributeType.Mana);

            GUI.Label(new Rect(20, y, 180, 20), $"HP: {health?.CurrentValue:F0}/{health?.ModifiedValue:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 180, 20), $"MP: {mana?.CurrentValue:F0}/{mana?.ModifiedValue:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 180, 20), $"���ݷ�: {playerAttributes?.GetAttributeValue(AttributeType.AttackPower):F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 180, 20), $"����: {playerAttributes?.GetAttributeValue(AttributeType.Defense):F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 180, 20), $"ũ��Ƽ��: {playerAttributes?.GetAttributeValue(AttributeType.CriticalChance):F0}%");

            // �� �Ӽ�
            y = 65;
            GUI.Label(new Rect(210, y, 180, 20), "=== �� ===");
            y += 25;

            var enemyHealthAttr = enemyAttributes?.GetAttribute(AttributeType.Health);
            GUI.Label(new Rect(210, y, 180, 20), $"HP: {enemyHealthAttr?.CurrentValue:F0}/{enemyHealthAttr?.BaseValue:F0}");
            y += 20;
            GUI.Label(new Rect(210, y, 180, 20), $"���ݷ�: {enemyAttributes?.GetAttributeValue(AttributeType.AttackPower):F0}");
            y += 20;
            GUI.Label(new Rect(210, y, 180, 20), $"����: {enemyAttributes?.GetAttributeValue(AttributeType.Defense):F0}");

            // ���� ����
            y = 180;
            GUI.Label(new Rect(20, y, 380, 60), "1: ���� �ùķ��̼�  2: ���� ����  3: ��� ���� ����\nSpace: ��ü �Ӽ� ���");

            // Ȱ�� ���� ��
            y = 220;
            GUI.Label(new Rect(20, y, 200, 20), $"Ȱ�� ����: {temporaryBuffs?.Count ?? 0}��");
        }
    }
}