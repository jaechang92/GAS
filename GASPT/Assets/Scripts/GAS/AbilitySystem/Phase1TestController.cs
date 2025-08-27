// ���� ��ġ: Assets/Scripts/GAS/Learning/Phase1/Phase1TestController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GAS.Learning.Phase1
{
    /// <summary>
    /// Phase 1 ���� �׽�Ʈ ��Ʈ�ѷ�
    /// �н��� ��� ������ ������ �����ϴ� ����
    /// </summary>
    public class Phase1TestController : MonoBehaviour
    {
        [Header("�׽�Ʈ ���")]
        [SerializeField] private TestMode currentTestMode = TestMode.Integration;

        private enum TestMode
        {
            BasicAttribute,    // Step 1 �׽�Ʈ
            Modifier,         // Step 2 �׽�Ʈ
            AttributeSet,     // Step 3 �׽�Ʈ
            Integration       // ���� �׽�Ʈ
        }

        [Header("���� �ý���")]
        [SerializeField] private CharacterAttributeSystem playerSystem;
        [SerializeField] private CharacterAttributeSystem enemySystem;

        [Header("�׽�Ʈ UI")]
        [SerializeField] private bool showDetailedInfo = false;
        [SerializeField] private float uiUpdateInterval = 0.1f;
        private float nextUiUpdate;

        // ���� Character Attribute System
        [System.Serializable]
        public class CharacterAttributeSystem
        {
            public string characterName;
            public AttributeSetExample.AttributeSet attributes;
            public List<ActiveEffect> activeEffects = new List<ActiveEffect>();

            [System.Serializable]
            public class ActiveEffect
            {
                public string name;
                public List<ModifierTestExample.AttributeModifier> modifiers;
                public float duration;
                public float remainingTime;
                public bool isPermanent;

                public ActiveEffect(string name, float duration = 0)
                {
                    this.name = name;
                    this.duration = duration;
                    this.remainingTime = duration;
                    this.isPermanent = duration <= 0;
                    this.modifiers = new List<ModifierTestExample.AttributeModifier>();
                }
            }

            public void Initialize(string name)
            {
                characterName = name;
                attributes = new AttributeSetExample.AttributeSet();

                // �⺻ �Ӽ� ����
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.Health, 100f, 0f, 100f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.MaxHealth, 100f, 50f, 200f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.Mana, 50f, 0f, 50f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.MaxMana, 50f, 20f, 100f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.AttackPower, 10f, 0f, 999f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.Defense, 5f, 0f, 999f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.MoveSpeed, 5f, 0f, 20f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.CriticalChance, 10f, 0f, 100f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.CriticalDamage, 50f, 0f, 500f);
            }

            public void ApplyEffect(string effectName, float duration, params (AttributeSetExample.AttributeType, ModifierTestExample.ModifierOperation, float)[] modifiers)
            {
                var effect = new ActiveEffect(effectName, duration);

                foreach (var (type, operation, value) in modifiers)
                {
                    var modifier = new ModifierTestExample.AttributeModifier(
                        effectName,
                        operation,
                        value
                    );

                    effect.modifiers.Add(modifier);
                    attributes.ApplyModifier(type, modifier);
                }

                activeEffects.Add(effect);
                Debug.Log($"[{characterName}] ȿ�� ����: {effectName} ({duration}��)");
            }

            public void RemoveEffect(ActiveEffect effect)
            {
                foreach (var modifier in effect.modifiers)
                {
                    // ��� �Ӽ����� modifier ���� �õ�
                    foreach (AttributeSetExample.AttributeType type in System.Enum.GetValues(typeof(AttributeSetExample.AttributeType)))
                    {
                        var attr = attributes.GetAttribute(type);
                        attr?.RemoveModifier(modifier);
                    }
                }

                activeEffects.Remove(effect);
                Debug.Log($"[{characterName}] ȿ�� ����: {effect.name}");
            }

            public void UpdateEffects(float deltaTime)
            {
                var effectsToRemove = new List<ActiveEffect>();

                foreach (var effect in activeEffects)
                {
                    if (!effect.isPermanent)
                    {
                        effect.remainingTime -= deltaTime;
                        if (effect.remainingTime <= 0)
                        {
                            effectsToRemove.Add(effect);
                        }
                    }
                }

                foreach (var effect in effectsToRemove)
                {
                    RemoveEffect(effect);
                }
            }
        }

        private async void Start()
        {
            InitializeSystems();

            await Task.Delay(1000); // 1�� ���

            Debug.Log("=== Phase 1 ���� �׽�Ʈ ���� ===");
            await RunIntegrationTest();
        }

        private void InitializeSystems()
        {
            playerSystem = new CharacterAttributeSystem();
            playerSystem.Initialize("Player");

            enemySystem = new CharacterAttributeSystem();
            enemySystem.Initialize("Enemy");

            // �̺�Ʈ ����
            playerSystem.attributes.OnAnyAttributeChanged += (type, oldVal, newVal) =>
            {
                if (type == AttributeSetExample.AttributeType.Health && newVal <= 0)
                {
                    Debug.LogError($"[{playerSystem.characterName}] ���!");
                }
            };

            enemySystem.attributes.OnAnyAttributeChanged += (type, oldVal, newVal) =>
            {
                if (type == AttributeSetExample.AttributeType.Health && newVal <= 0)
                {
                    Debug.LogError($"[{enemySystem.characterName}] óġ!");
                }
            };
        }

        private async Task RunIntegrationTest()
        {
            switch (currentTestMode)
            {
                case TestMode.BasicAttribute:
                    await TestBasicAttributeOperations();
                    break;
                case TestMode.Modifier:
                    await TestModifierSystem();
                    break;
                case TestMode.AttributeSet:
                    await TestAttributeSetOperations();
                    break;
                case TestMode.Integration:
                    await TestFullIntegration();
                    break;
            }
        }

        // �׽�Ʈ 1: �⺻ Attribute �۾�
        private async Task TestBasicAttributeOperations()
        {
            Debug.Log("\n=== �⺻ Attribute �׽�Ʈ ===");

            var health = playerSystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);

            Debug.Log($"�ʱ� ü��: {health.CurrentValue}");

            // ������ �׽�Ʈ
            health.AddToCurrentValue(-30);
            Debug.Log($"30 ������ ��: {health.CurrentValue}");
            await Task.Delay(1000);

            // �� �׽�Ʈ
            health.AddToCurrentValue(20);
            Debug.Log($"20 �� ��: {health.CurrentValue}");
            await Task.Delay(1000);

            // ������ �׽�Ʈ
            health.AddToCurrentValue(200);
            Debug.Log($"������ �׽�Ʈ: {health.CurrentValue} (�ִ밪���� ���ѵ�)");
        }

        // �׽�Ʈ 2: Modifier �ý���
        private async Task TestModifierSystem()
        {
            Debug.Log("\n=== Modifier �ý��� �׽�Ʈ ===");

            // ���� ����
            playerSystem.ApplyEffect(
                "���ݷ� ����",
                5f,
                (AttributeSetExample.AttributeType.AttackPower, ModifierTestExample.ModifierOperation.Add, 5f)
            );

            Debug.Log($"���� �� ���ݷ�: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.AttackPower)}");
            await Task.Delay(2000);

            // ����� ����
            playerSystem.ApplyEffect(
                "���ο�",
                3f,
                (AttributeSetExample.AttributeType.MoveSpeed, ModifierTestExample.ModifierOperation.Multiply, 0.5f)
            );

            Debug.Log($"���ο� �� �̵��ӵ�: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.MoveSpeed)}");
            await Task.Delay(3000);

            Debug.Log("����/����� ���� ��:");
            Debug.Log($"���ݷ�: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.AttackPower)}");
            Debug.Log($"�̵��ӵ�: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.MoveSpeed)}");
        }

        // �׽�Ʈ 3: AttributeSet �۾�
        private async Task TestAttributeSetOperations()
        {
            Debug.Log("\n=== AttributeSet �׽�Ʈ ===");

            // ���� �Ӽ� ���� ����
            playerSystem.ApplyEffect(
                "���� ��ȭ",
                10f,
                (AttributeSetExample.AttributeType.AttackPower, ModifierTestExample.ModifierOperation.Add, 10f),
                (AttributeSetExample.AttributeType.Defense, ModifierTestExample.ModifierOperation.Add, 5f),
                (AttributeSetExample.AttributeType.CriticalChance, ModifierTestExample.ModifierOperation.Add, 20f)
            );

            Debug.Log("���� ��ȭ ���� ����:");
            Debug.Log($"���ݷ�: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.AttackPower)}");
            Debug.Log($"����: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.Defense)}");
            Debug.Log($"ũ��Ƽ��: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.CriticalChance)}%");

            await Task.Delay(5000);
        }

        // �׽�Ʈ 4: ��ü ���� �׽�Ʈ
        private async Task TestFullIntegration()
        {
            Debug.Log("\n=== ��ü ���� �׽�Ʈ ===");

            // 1�ܰ�: ���� �غ�
            Debug.Log("\n[1�ܰ�] ���� �غ�");

            // �÷��̾� ����
            playerSystem.ApplyEffect(
                "������ �ູ",
                15f,
                (AttributeSetExample.AttributeType.AttackPower, ModifierTestExample.ModifierOperation.Multiply, 1.5f),
                (AttributeSetExample.AttributeType.Defense, ModifierTestExample.ModifierOperation.Add, 10f)
            );

            await Task.Delay(2000);

            // 2�ܰ�: ���� �ùķ��̼�
            Debug.Log("\n[2�ܰ�] ���� ����");

            for (int round = 1; round <= 3; round++)
            {
                Debug.Log($"\n=== ���� {round} ===");

                // �÷��̾� ����
                float playerDamage = playerSystem.attributes.CalculateEffectiveDamage(10f);
                float actualDamage = enemySystem.attributes.CalculateDamageReduction(playerDamage);

                var enemyHealth = enemySystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);
                enemyHealth.AddToCurrentValue(-actualDamage);

                Debug.Log($"�÷��̾� ����: {actualDamage:F1} ������");
                Debug.Log($"�� ü��: {enemyHealth.CurrentValue:F1}/{enemyHealth.BaseValue:F1}");

                if (enemyHealth.CurrentValue <= 0) break;

                await Task.Delay(1500);

                // �� �ݰ�
                float enemyDamage = enemySystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.AttackPower);
                float playerReducedDamage = playerSystem.attributes.CalculateDamageReduction(enemyDamage);

                var playerHealth = playerSystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);
                playerHealth.AddToCurrentValue(-playerReducedDamage);

                Debug.Log($"�� �ݰ�: {playerReducedDamage:F1} ������");
                Debug.Log($"�÷��̾� ü��: {playerHealth.CurrentValue:F1}/{playerHealth.BaseValue:F1}");

                if (playerHealth.CurrentValue <= 0) break;

                await Task.Delay(1500);
            }

            // 3�ܰ�: ���� ����
            Debug.Log("\n[3�ܰ�] ���� ����");
            Debug.Log(playerSystem.attributes.GetAllAttributesInfo());
        }

        private void Update()
        {
            // Effect Duration ������Ʈ
            if (playerSystem != null)
            {
                playerSystem.UpdateEffects(Time.deltaTime);
            }
            if (enemySystem != null)
            {
                enemySystem.UpdateEffects(Time.deltaTime);
            }

            // UI ������Ʈ
            if (Time.time >= nextUiUpdate)
            {
                nextUiUpdate = Time.time + uiUpdateInterval;
                // UI ������Ʈ ����
            }

            // �Է� ó��
            HandleInput();
        }

        private void HandleInput()
        {
            // �÷��̾� ����
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ApplyQuickBuff();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                ApplyQuickDebuff();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                SimulateQuickCombat();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestoreHealth();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void ApplyQuickBuff()
        {
            playerSystem.ApplyEffect(
                "���� ����",
                5f,
                (AttributeSetExample.AttributeType.AttackPower, ModifierTestExample.ModifierOperation.Add, 5f)
            );
        }

        private void ApplyQuickDebuff()
        {
            enemySystem.ApplyEffect(
                "��ȭ",
                5f,
                (AttributeSetExample.AttributeType.Defense, ModifierTestExample.ModifierOperation.Multiply, 0.5f)
            );
        }

        private void SimulateQuickCombat()
        {
            float damage = playerSystem.attributes.CalculateEffectiveDamage(15f);
            var enemyHealth = enemySystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);
            enemyHealth.AddToCurrentValue(-damage);
            Debug.Log($"���� ����! {damage:F1} ������");
        }

        private void RestoreHealth()
        {
            var playerHealth = playerSystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);
            var enemyHealth = enemySystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);

            playerHealth.SetCurrentValue(playerHealth.BaseValue);
            enemyHealth.SetCurrentValue(enemyHealth.BaseValue);

            Debug.Log("ü�� ���� ȸ��!");
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== ���� ���� ===");
            Debug.Log($"[�÷��̾�] Ȱ�� ȿ��: {playerSystem.activeEffects.Count}��");
            Debug.Log(playerSystem.attributes.GetAllAttributesInfo());
            Debug.Log($"\n[��] Ȱ�� ȿ��: {enemySystem.activeEffects.Count}��");
            Debug.Log(enemySystem.attributes.GetAllAttributesInfo());
        }

        private void OnGUI()
        {
            // ���� �ڽ�
            GUI.Box(new Rect(10, 10, 450, 300), "Phase 1 - ���� �׽�Ʈ");

            // �׽�Ʈ ���
            GUI.Label(new Rect(20, 40, 200, 20), $"�׽�Ʈ ���: {currentTestMode}");

            int y = 70;

            // �÷��̾� ����
            GUI.Label(new Rect(20, y, 200, 20), "=== �÷��̾� ===");
            y += 25;

            if (playerSystem != null)
            {
                var health = playerSystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);
                var mana = playerSystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Mana);

                GUI.Label(new Rect(20, y, 200, 20), $"HP: {health?.CurrentValue:F0}/{health?.BaseValue:F0}");
                y += 20;
                GUI.Label(new Rect(20, y, 200, 20), $"MP: {mana?.CurrentValue:F0}/{mana?.BaseValue:F0}");
                y += 20;
                GUI.Label(new Rect(20, y, 200, 20), $"���ݷ�: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.AttackPower):F0}");
                y += 20;
                GUI.Label(new Rect(20, y, 200, 20), $"����: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.Defense):F0}");
                y += 20;
                GUI.Label(new Rect(20, y, 200, 20), $"Ȱ�� ȿ��: {playerSystem.activeEffects.Count}��");
            }

            // �� ����
            y = 95;
            GUI.Label(new Rect(240, y, 200, 20), "=== �� ===");
            y += 25;

            if (enemySystem != null)
            {
                var health = enemySystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);

                GUI.Label(new Rect(240, y, 200, 20), $"HP: {health?.CurrentValue:F0}/{health?.BaseValue:F0}");
                y += 20;
                GUI.Label(new Rect(240, y, 200, 20), $"���ݷ�: {enemySystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.AttackPower):F0}");
                y += 20;
                GUI.Label(new Rect(240, y, 200, 20), $"����: {enemySystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.Defense):F0}");
                y += 20;
                GUI.Label(new Rect(240, y, 200, 20), $"Ȱ�� ȿ��: {enemySystem.activeEffects.Count}��");
            }

            // ���� ����
            y = 220;
            GUI.Label(new Rect(20, y, 430, 70),
                "Q: �÷��̾� ����  W: �� �����  E: ���� ����\n" +
                "R: ü�� ȸ��  Space: ���� ���\n\n" +
                "Phase 1 �н� �Ϸ�! ���� �ܰ�� �����ϼ���.");
        }
    }
}