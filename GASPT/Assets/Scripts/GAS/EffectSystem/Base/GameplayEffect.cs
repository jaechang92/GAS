// ���� ��ġ: Assets/Scripts/GAS/Learning/Phase3/BasicEffectExample.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using GAS.Learning.Phase1;
using GAS.Learning.Phase2;

namespace GAS.Learning.Phase3
{
    /// <summary>
    /// Step 1: GameplayEffect ���� - ȿ���� �⺻ ������ ����
    /// </summary>
    public class GameplayEffect : MonoBehaviour
    {
        // ������ Effect ����
        [Serializable]
        public class SimpleGameplayEffect
        {
            // ȿ�� �⺻ ����
            [Header("�⺻ ����")]
            public string effectName;
            public string description;
            public Sprite icon;

            // Duration Ÿ��
            public enum DurationType
            {
                Instant,    // ��� ����
                Duration,   // ���� �ð�
                Infinite    // ����
            }

            [Header("Duration ����")]
            public DurationType durationType = DurationType.Instant;
            public float duration = 0f;
            public float remainingTime = 0f;

            // Attribute ������
            [Header("Attribute Modifiers")]
            public List<AttributeModifier> modifiers = new List<AttributeModifier>();

            [Serializable]
            public class AttributeModifier
            {
                public string attributeName;
                public ModifierOperation operation;
                public float value;

                public enum ModifierOperation
                {
                    Add,
                    Multiply,
                    Override
                }

                public AttributeModifier(string name, ModifierOperation op, float val)
                {
                    attributeName = name;
                    operation = op;
                    value = val;
                }

                public void Apply(Dictionary<string, float> attributes)
                {
                    if (!attributes.ContainsKey(attributeName))
                    {
                        Debug.LogWarning($"Attribute {attributeName} not found!");
                        return;
                    }

                    switch (operation)
                    {
                        case ModifierOperation.Add:
                            attributes[attributeName] += value;
                            break;
                        case ModifierOperation.Multiply:
                            attributes[attributeName] *= value;
                            break;
                        case ModifierOperation.Override:
                            attributes[attributeName] = value;
                            break;
                    }

                    Debug.Log($"[Modifier] {attributeName} {operation} {value} �� Result: {attributes[attributeName]}");
                }

                public void Remove(Dictionary<string, float> attributes)
                {
                    // Duration ȿ�� ���� �� ������ ���� ����
                    // �����δ� �� ������ �ý��� �ʿ�
                    Debug.Log($"[Modifier] {attributeName} ȿ�� ����");
                }
            }

            // Tag ����
            [Header("Tag ����")]
            public List<string> grantedTags = new List<string>();
            public List<string> removedTags = new List<string>();

            // ���� ����
            [Header("���� ����")]
            public bool isStackable = false;
            public int maxStacks = 1;
            public int currentStacks = 0;

            // ȿ�� ID (���� �ĺ���)
            private string effectId;

            public SimpleGameplayEffect(string name, DurationType type, float duration = 0)
            {
                this.effectName = name;
                this.durationType = type;
                this.duration = duration;
                this.remainingTime = duration;
                this.effectId = Guid.NewGuid().ToString();
            }

            // ȿ�� ���� ���� üũ
            public bool CanApply(EffectTarget target)
            {
                // �±� ���� üũ
                foreach (var requiredTag in removedTags)
                {
                    if (target.HasTag(requiredTag))
                    {
                        Debug.Log($"[Effect] {effectName} ���� �Ұ�: ���� �±� {requiredTag}");
                        return false;
                    }
                }

                // ���� üũ
                if (!isStackable && target.HasEffect(effectName))
                {
                    Debug.Log($"[Effect] {effectName} ���� �Ұ�: �̹� ���� (���� �Ұ�)");
                    return false;
                }

                return true;
            }

            // ȿ�� ����
            public void Apply(EffectTarget target)
            {
                if (!CanApply(target))
                {
                    Debug.LogWarning($"[Effect] {effectName} ���� ����!");
                    return;
                }

                Debug.Log($"[Effect] === {effectName} ���� ���� ===");

                // 1. Attribute ����
                foreach (var modifier in modifiers)
                {
                    modifier.Apply(target.attributes);
                }

                // 2. Tag �ο�
                foreach (var tag in grantedTags)
                {
                    target.AddTag(tag);
                    Debug.Log($"[Effect] Tag �߰�: {tag}");
                }

                // 3. Tag ����
                foreach (var tag in removedTags)
                {
                    target.RemoveTag(tag);
                    Debug.Log($"[Effect] Tag ����: {tag}");
                }

                // 4. ���� ó��
                if (isStackable)
                {
                    currentStacks = Math.Min(currentStacks + 1, maxStacks);
                    Debug.Log($"[Effect] ���� ����: {currentStacks}/{maxStacks}");
                }

                Debug.Log($"[Effect] === {effectName} ���� �Ϸ� ===");
            }

            // ȿ�� ����
            public void Remove(EffectTarget target)
            {
                Debug.Log($"[Effect] === {effectName} ���� ���� ===");

                // Duration/Infinite ȿ���� ���� ó��
                if (durationType != DurationType.Instant)
                {
                    // Modifier ���� (�����δ� ������ ���� ���� �ʿ�)
                    foreach (var modifier in modifiers)
                    {
                        modifier.Remove(target.attributes);
                    }

                    // Tag ����
                    foreach (var tag in grantedTags)
                    {
                        target.RemoveTag(tag);
                    }
                }

                Debug.Log($"[Effect] === {effectName} ���� �Ϸ� ===");
            }

            // �ð� ������Ʈ
            public bool UpdateTime(float deltaTime)
            {
                if (durationType != DurationType.Duration)
                    return false;

                remainingTime -= deltaTime;

                if (remainingTime <= 0)
                {
                    Debug.Log($"[Effect] {effectName} �ð� ����!");
                    return true; // ���� �ʿ�
                }

                return false;
            }

            public string GetDebugInfo()
            {
                string info = $"[{effectName}]\n";
                info += $"  Type: {durationType}\n";
                if (durationType == DurationType.Duration)
                {
                    info += $"  Time: {remainingTime:F1}/{duration:F1}\n";
                }
                if (isStackable)
                {
                    info += $"  Stacks: {currentStacks}/{maxStacks}\n";
                }
                return info;
            }
        }

        // Effect ��� (ĳ����)
        [Serializable]
        public class EffectTarget
        {
            public string targetName;
            public Dictionary<string, float> attributes = new Dictionary<string, float>();
            public List<string> tags = new List<string>();
            public List<SimpleGameplayEffect> activeEffects = new List<SimpleGameplayEffect>();

            public EffectTarget(string name)
            {
                targetName = name;
                InitializeAttributes();
            }

            private void InitializeAttributes()
            {
                attributes["Health"] = 100f;
                attributes["MaxHealth"] = 100f;
                attributes["Mana"] = 50f;
                attributes["MaxMana"] = 50f;
                attributes["AttackPower"] = 10f;
                attributes["Defense"] = 5f;
                attributes["MoveSpeed"] = 5f;
            }

            public bool HasTag(string tag)
            {
                return tags.Contains(tag);
            }

            public void AddTag(string tag)
            {
                if (!tags.Contains(tag))
                {
                    tags.Add(tag);
                }
            }

            public void RemoveTag(string tag)
            {
                tags.Remove(tag);
            }

            public bool HasEffect(string effectName)
            {
                return activeEffects.Exists(e => e.effectName == effectName);
            }

            public void ApplyEffect(SimpleGameplayEffect effect)
            {
                effect.Apply(this);

                // Duration/Infinite ȿ���� ��Ͽ� �߰�
                if (effect.durationType != SimpleGameplayEffect.DurationType.Instant)
                {
                    activeEffects.Add(effect);
                }
            }

            public void RemoveEffect(SimpleGameplayEffect effect)
            {
                effect.Remove(this);
                activeEffects.Remove(effect);
            }

            public void UpdateEffects(float deltaTime)
            {
                var effectsToRemove = new List<SimpleGameplayEffect>();

                foreach (var effect in activeEffects)
                {
                    if (effect.UpdateTime(deltaTime))
                    {
                        effectsToRemove.Add(effect);
                    }
                }

                foreach (var effect in effectsToRemove)
                {
                    RemoveEffect(effect);
                }
            }

            public string GetStatus()
            {
                string status = $"=== {targetName} Status ===\n";

                // Attributes
                status += "Attributes:\n";
                foreach (var attr in attributes)
                {
                    status += $"  {attr.Key}: {attr.Value:F1}\n";
                }

                // Tags
                status += $"Tags: {string.Join(", ", tags)}\n";

                // Active Effects
                status += $"Active Effects ({activeEffects.Count}):\n";
                foreach (var effect in activeEffects)
                {
                    status += effect.GetDebugInfo();
                }

                return status;
            }
        }

        [Header("�׽�Ʈ ���")]
        [SerializeField] private EffectTarget playerTarget;
        [SerializeField] private EffectTarget enemyTarget;

        [Header("�̸� ���ǵ� ȿ��")]
        [SerializeField] private List<SimpleGameplayEffect> predefinedEffects = new List<SimpleGameplayEffect>();

        private void Start()
        {
            InitializeTargets();
            CreatePredefinedEffects();

            Debug.Log("=== Phase 3 - Step 1: Basic Effect �׽�Ʈ ===");
            RunBasicTests();
        }

        private void InitializeTargets()
        {
            playerTarget = new EffectTarget("Player");
            enemyTarget = new EffectTarget("Enemy");
        }

        private void CreatePredefinedEffects()
        {
            // 1. ��� ������
            var instantDamage = new SimpleGameplayEffect("��� ������", SimpleGameplayEffect.DurationType.Instant);
            instantDamage.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("Health",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Add, -20f));
            instantDamage.grantedTags.Add("Combat.Damaged");
            predefinedEffects.Add(instantDamage);

            // 2. ��� ȸ��
            var instantHeal = new SimpleGameplayEffect("��� ȸ��", SimpleGameplayEffect.DurationType.Instant);
            instantHeal.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("Health",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Add, 30f));
            instantHeal.grantedTags.Add("Combat.Healed");
            predefinedEffects.Add(instantHeal);

            // 3. ���ݷ� ���� (10��)
            var attackBuff = new SimpleGameplayEffect("���ݷ� ����", SimpleGameplayEffect.DurationType.Duration, 10f);
            attackBuff.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("AttackPower",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Add, 5f));
            attackBuff.grantedTags.Add("Status.Buff.Attack");
            predefinedEffects.Add(attackBuff);

            // 4. ���� ���� (15��)
            var defenseBuff = new SimpleGameplayEffect("���� ����", SimpleGameplayEffect.DurationType.Duration, 15f);
            defenseBuff.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("Defense",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Multiply, 1.5f));
            defenseBuff.grantedTags.Add("Status.Buff.Defense");
            predefinedEffects.Add(defenseBuff);

            // 5. ���ο� ����� (5��)
            var slowDebuff = new SimpleGameplayEffect("���ο�", SimpleGameplayEffect.DurationType.Duration, 5f);
            slowDebuff.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("MoveSpeed",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Multiply, 0.5f));
            slowDebuff.grantedTags.Add("Status.Debuff.Slow");
            predefinedEffects.Add(slowDebuff);

            // 6. ���� �нú�
            var passiveBuff = new SimpleGameplayEffect("���� �нú�", SimpleGameplayEffect.DurationType.Infinite);
            passiveBuff.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("MaxHealth",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Add, 20f));
            passiveBuff.grantedTags.Add("Status.Passive.HealthBoost");
            predefinedEffects.Add(passiveBuff);
        }

        private void RunBasicTests()
        {
            TestInstantEffects();
            TestDurationEffects();
            TestInfiniteEffects();
            TestEffectStacking();
        }

        // �׽�Ʈ 1: ��� ȿ��
        private void TestInstantEffects()
        {
            Debug.Log("\n========== �׽�Ʈ 1: Instant Effects ==========");

            Debug.Log("�ʱ� ����:");
            Debug.Log(playerTarget.GetStatus());

            // ��� ������
            var damage = predefinedEffects[0];
            playerTarget.ApplyEffect(damage);

            Debug.Log("\n��� ������ ��:");
            Debug.Log($"Health: {playerTarget.attributes["Health"]}");
            Debug.Log($"Tags: {string.Join(", ", playerTarget.tags)}");

            // ��� ȸ��
            var heal = predefinedEffects[1];
            playerTarget.ApplyEffect(heal);

            Debug.Log("\n��� ȸ�� ��:");
            Debug.Log($"Health: {playerTarget.attributes["Health"]}");
        }

        // �׽�Ʈ 2: Duration ȿ��
        private void TestDurationEffects()
        {
            Debug.Log("\n========== �׽�Ʈ 2: Duration Effects ==========");

            // ���ݷ� ���� ����
            var attackBuff = new SimpleGameplayEffect("���ݷ� ����", SimpleGameplayEffect.DurationType.Duration, 10f);
            attackBuff.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("AttackPower",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Add, 5f));
            attackBuff.grantedTags.Add("Status.Buff.Attack");

            Debug.Log($"���� �� ���ݷ�: {playerTarget.attributes["AttackPower"]}");
            playerTarget.ApplyEffect(attackBuff);
            Debug.Log($"���� �� ���ݷ�: {playerTarget.attributes["AttackPower"]}");
            Debug.Log($"Ȱ�� ȿ�� ��: {playerTarget.activeEffects.Count}");
        }

        // �׽�Ʈ 3: Infinite ȿ��
        private void TestInfiniteEffects()
        {
            Debug.Log("\n========== �׽�Ʈ 3: Infinite Effects ==========");

            var passive = predefinedEffects[5];

            Debug.Log($"�нú� �� MaxHealth: {enemyTarget.attributes["MaxHealth"]}");
            enemyTarget.ApplyEffect(passive);
            Debug.Log($"�нú� �� MaxHealth: {enemyTarget.attributes["MaxHealth"]}");
            Debug.Log($"���� ȿ���̹Ƿ� ���� ������ ������");
        }

        // �׽�Ʈ 4: ȿ�� ����
        private void TestEffectStacking()
        {
            Debug.Log("\n========== �׽�Ʈ 4: Effect Stacking ==========");

            // ���� ������ ȿ�� ����
            var stackableBuff = new SimpleGameplayEffect("������ ����", SimpleGameplayEffect.DurationType.Duration, 20f);
            stackableBuff.isStackable = true;
            stackableBuff.maxStacks = 3;
            stackableBuff.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("AttackPower",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Add, 2f));

            Debug.Log("������ ���� 3ȸ ����:");
            for (int i = 0; i < 4; i++)
            {
                playerTarget.ApplyEffect(stackableBuff);
                Debug.Log($"  ���� {i + 1}ȸ: ���� {stackableBuff.currentStacks}/{stackableBuff.maxStacks}");
            }
        }

        private void Update()
        {
            // Duration ȿ�� ������Ʈ
            if (playerTarget != null)
            {
                playerTarget.UpdateEffects(Time.deltaTime);
            }
            if (enemyTarget != null)
            {
                enemyTarget.UpdateEffects(Time.deltaTime);
            }

            HandleInput();
        }

        private void HandleInput()
        {
            // ȿ�� ���� �׽�Ʈ
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ApplyEffectToPlayer(0); // ��� ������
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ApplyEffectToPlayer(1); // ��� ȸ��
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ApplyEffectToPlayer(2); // ���ݷ� ����
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                ApplyEffectToPlayer(3); // ���� ����
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                ApplyEffectToPlayer(4); // ���ο�
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetTarget(playerTarget);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void ApplyEffectToPlayer(int index)
        {
            if (index < 0 || index >= predefinedEffects.Count) return;

            // �� �ν��Ͻ� ���� (�߿�!)
            var effectTemplate = predefinedEffects[index];
            var newEffect = new SimpleGameplayEffect(effectTemplate.effectName, effectTemplate.durationType, effectTemplate.duration);
            newEffect.modifiers.AddRange(effectTemplate.modifiers);
            newEffect.grantedTags.AddRange(effectTemplate.grantedTags);

            playerTarget.ApplyEffect(newEffect);
        }

        private void ResetTarget(EffectTarget target)
        {
            target.activeEffects.Clear();
            target.tags.Clear();
            target.attributes["Health"] = 100f;
            target.attributes["AttackPower"] = 10f;
            target.attributes["Defense"] = 5f;
            target.attributes["MoveSpeed"] = 5f;
            Debug.Log($"{target.targetName} �ʱ�ȭ �Ϸ�!");
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== ���� ���� ===");
            Debug.Log(playerTarget.GetStatus());
            Debug.Log(enemyTarget.GetStatus());
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 350, 280), "Phase 3 - Basic Effects");

            int y = 40;
            GUI.Label(new Rect(20, y, 330, 20), "=== Player Status ===");
            y += 25;

            // Attributes
            GUI.Label(new Rect(20, y, 330, 20), $"HP: {playerTarget?.attributes["Health"]:F0}/{playerTarget?.attributes["MaxHealth"]:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 330, 20), $"MP: {playerTarget?.attributes["Mana"]:F0}/{playerTarget?.attributes["MaxMana"]:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 330, 20), $"ATK: {playerTarget?.attributes["AttackPower"]:F0} | DEF: {playerTarget?.attributes["Defense"]:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 330, 20), $"Speed: {playerTarget?.attributes["MoveSpeed"]:F1}");
            y += 25;

            // Effects
            GUI.Label(new Rect(20, y, 330, 20), $"Active Effects: {playerTarget?.activeEffects.Count ?? 0}");
            y += 20;
            if (playerTarget != null && playerTarget.activeEffects.Count > 0)
            {
                foreach (var effect in playerTarget.activeEffects)
                {
                    string timeInfo = effect.durationType == SimpleGameplayEffect.DurationType.Duration
                        ? $" ({effect.remainingTime:F1}s)"
                        : effect.durationType == SimpleGameplayEffect.DurationType.Infinite
                        ? " (��)"
                        : "";
                    GUI.Label(new Rect(30, y, 320, 20), $" {effect.effectName}{timeInfo}");
                    y += 20;
                }
            }

            // Tags
            y += 5;
            GUI.Label(new Rect(20, y, 330, 20), $"Tags: {string.Join(", ", playerTarget?.tags ?? new List<string>())}");

            // ���۹�
            y = 230;
            GUI.Box(new Rect(10, y, 350, 80), "���۹�");
            GUI.Label(new Rect(20, y + 25, 330, 50),
                "1:������ 2:ȸ�� 3:���ݹ��� 4:������ 5:���ο�\n" +
                "R: �ʱ�ȭ  Space: ���� ���");
        }
    }
}