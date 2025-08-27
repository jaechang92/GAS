// ���� ��ġ: Assets/Scripts/GAS/Learning/Phase3/Phase3TestController.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GAS.Learning.Phase1;
using GAS.Learning.Phase2;

namespace GAS.Learning.Phase3
{
    /// <summary>
    /// Phase 3 ���� �׽�Ʈ ��Ʈ�ѷ�
    /// Effect System + Attribute System + Tag System ����
    /// </summary>
    public class Phase3TestController : MonoBehaviour
    {
        [Header("�׽�Ʈ ���")]
        [SerializeField] private TestMode currentTestMode = TestMode.Integration;

        private enum TestMode
        {
            BasicEffect,     // Step 1 �׽�Ʈ
            Duration,        // Step 2 �׽�Ʈ
            Periodic,        // Step 3 �׽�Ʈ
            Integration      // ���� �׽�Ʈ
        }

        // ���� ĳ���� �ý���
        [Serializable]
        public class IntegratedCharacter
        {
            public string characterName;

            // Phase 1: Attributes
            public Dictionary<string, float> attributes = new Dictionary<string, float>();
            public Dictionary<string, float> baseAttributes = new Dictionary<string, float>();

            // Phase 2: Tags
            public List<string> tags = new List<string>();

            // Phase 3: Effects
            public List<IntegratedEffect> activeEffects = new List<IntegratedEffect>();
            public List<EffectInstance> effectHistory = new List<EffectInstance>();

            // ����
            public float health;
            public float maxHealth;
            public float mana;
            public float maxMana;
            public bool isDead;

            public IntegratedCharacter(string name)
            {
                characterName = name;
                InitializeAttributes();
            }

            private void InitializeAttributes()
            {
                // �⺻ �Ӽ� ����
                baseAttributes["Health"] = 100f;
                baseAttributes["MaxHealth"] = 100f;
                baseAttributes["Mana"] = 50f;
                baseAttributes["MaxMana"] = 50f;
                baseAttributes["AttackPower"] = 10f;
                baseAttributes["Defense"] = 5f;
                baseAttributes["MoveSpeed"] = 5f;
                baseAttributes["CritChance"] = 10f;
                baseAttributes["CritDamage"] = 50f;

                // ���� �Ӽ� �ʱ�ȭ
                foreach (var kvp in baseAttributes)
                {
                    attributes[kvp.Key] = kvp.Value;
                }

                health = attributes["Health"];
                maxHealth = attributes["MaxHealth"];
                mana = attributes["Mana"];
                maxMana = attributes["MaxMana"];
            }

            public void ApplyEffect(IntegratedEffect effect)
            {
                if (!effect.CanApply(this))
                {
                    Debug.LogWarning($"[{characterName}] {effect.effectName} ���� �Ұ�!");
                    return;
                }

                effect.Apply(this);

                if (effect.durationType != IntegratedEffect.DurationType.Instant)
                {
                    activeEffects.Add(effect);
                }

                // �����丮 �߰�
                effectHistory.Add(new EffectInstance
                {
                    effectName = effect.effectName,
                    appliedTime = Time.time,
                    duration = effect.duration
                });

                Debug.Log($"[{characterName}] {effect.effectName} ����!");
            }

            public void RemoveEffect(IntegratedEffect effect)
            {
                effect.Remove(this);
                activeEffects.Remove(effect);
                Debug.Log($"[{characterName}] {effect.effectName} ����!");
            }

            public void UpdateEffects(float deltaTime)
            {
                var effectsToRemove = new List<IntegratedEffect>();

                foreach (var effect in activeEffects)
                {
                    if (effect.Update(deltaTime, this))
                    {
                        effectsToRemove.Add(effect);
                    }
                }

                foreach (var effect in effectsToRemove)
                {
                    RemoveEffect(effect);
                }

                // ü��/���� ������Ʈ
                health = Mathf.Clamp(attributes["Health"], 0, attributes["MaxHealth"]);
                maxHealth = attributes["MaxHealth"];
                mana = Mathf.Clamp(attributes["Mana"], 0, attributes["MaxMana"]);
                maxMana = attributes["MaxMana"];

                // ��� üũ
                if (health <= 0 && !isDead)
                {
                    isDead = true;
                    tags.Add("State.Dead");
                    Debug.Log($"[{characterName}] ���!");
                }
            }

            public void TakeDamage(float damage)
            {
                float defense = attributes["Defense"];
                float actualDamage = Mathf.Max(1, damage - defense * 0.5f);
                attributes["Health"] -= actualDamage;
                Debug.Log($"[{characterName}] {actualDamage:F1} ������ ���� (���: {defense})");
            }

            public void Heal(float amount)
            {
                attributes["Health"] = Mathf.Min(attributes["Health"] + amount, attributes["MaxHealth"]);
                Debug.Log($"[{characterName}] {amount:F1} ȸ��");
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

            public string GetStatus()
            {
                string status = $"=== {characterName} ===\n";
                status += $"HP: {health:F0}/{maxHealth:F0} | MP: {mana:F0}/{maxMana:F0}\n";
                status += $"ATK: {attributes["AttackPower"]:F0} | DEF: {attributes["Defense"]:F0} | SPD: {attributes["MoveSpeed"]:F1}\n";
                status += $"Tags: {string.Join(", ", tags)}\n";
                status += $"Active Effects: {activeEffects.Count}";
                return status;
            }
        }

        // ���� Effect
        [Serializable]
        public class IntegratedEffect
        {
            public string effectName;
            public string description;

            public enum DurationType
            {
                Instant,
                Duration,
                Infinite
            }

            public DurationType durationType;
            public float duration;
            public float remainingTime;

            // Periodic
            public bool isPeriodic;
            public float tickInterval;
            public float nextTickTime;
            public int tickCount;

            // Modifiers
            public List<AttributeModifier> modifiers = new List<AttributeModifier>();

            // Tags
            public List<string> grantedTags = new List<string>();
            public List<string> requiredTags = new List<string>();
            public List<string> blockingTags = new List<string>();

            // Stack
            public bool isStackable;
            public int currentStacks;
            public int maxStacks;

            [Serializable]
            public class AttributeModifier
            {
                public string attributeName;
                public float value;
                public ModifierType type;

                public enum ModifierType
                {
                    Flat,
                    Percent,
                    Override
                }
            }

            public IntegratedEffect(string name, DurationType type, float dur = 0)
            {
                effectName = name;
                durationType = type;
                duration = dur;
                remainingTime = dur;
                currentStacks = 1;
                maxStacks = 1;
            }

            public bool CanApply(IntegratedCharacter target)
            {
                // �ʼ� �±� üũ
                foreach (var tag in requiredTags)
                {
                    if (!target.HasTag(tag))
                    {
                        return false;
                    }
                }

                // ���� �±� üũ
                foreach (var tag in blockingTags)
                {
                    if (target.HasTag(tag))
                    {
                        return false;
                    }
                }

                return true;
            }

            public void Apply(IntegratedCharacter target)
            {
                // Attribute ����
                foreach (var modifier in modifiers)
                {
                    ApplyModifier(target, modifier);
                }

                // Tag �ο�
                foreach (var tag in grantedTags)
                {
                    target.AddTag(tag);
                }

                // Periodic �ʱ�ȭ
                if (isPeriodic)
                {
                    nextTickTime = tickInterval;
                    tickCount = 0;
                }
            }

            public void Remove(IntegratedCharacter target)
            {
                // Duration/Infinite ȿ�� ����
                if (durationType != DurationType.Instant)
                {
                    // Modifier ���� (������ ����)
                    foreach (var modifier in modifiers)
                    {
                        RemoveModifier(target, modifier);
                    }

                    // Tag ����
                    foreach (var tag in grantedTags)
                    {
                        target.RemoveTag(tag);
                    }
                }
            }

            public bool Update(float deltaTime, IntegratedCharacter target)
            {
                if (durationType == DurationType.Instant)
                    return true;

                if (durationType == DurationType.Infinite)
                    return false;

                remainingTime -= deltaTime;

                // Periodic ó��
                if (isPeriodic)
                {
                    nextTickTime -= deltaTime;
                    if (nextTickTime <= 0)
                    {
                        Tick(target);
                        nextTickTime = tickInterval;
                    }
                }

                return remainingTime <= 0;
            }

            private void Tick(IntegratedCharacter target)
            {
                tickCount++;
                Debug.Log($"[{effectName}] Tick #{tickCount}");

                // Periodic ȿ�� ����
                foreach (var modifier in modifiers)
                {
                    if (modifier.type == AttributeModifier.ModifierType.Flat)
                    {
                        ApplyModifier(target, modifier);
                    }
                }
            }

            private void ApplyModifier(IntegratedCharacter target, AttributeModifier modifier)
            {
                if (!target.attributes.ContainsKey(modifier.attributeName))
                    return;

                switch (modifier.type)
                {
                    case AttributeModifier.ModifierType.Flat:
                        target.attributes[modifier.attributeName] += modifier.value * currentStacks;
                        break;
                    case AttributeModifier.ModifierType.Percent:
                        target.attributes[modifier.attributeName] *= (1 + modifier.value * currentStacks);
                        break;
                    case AttributeModifier.ModifierType.Override:
                        target.attributes[modifier.attributeName] = modifier.value;
                        break;
                }
            }

            private void RemoveModifier(IntegratedCharacter target, AttributeModifier modifier)
            {
                // ������ ���� (�����δ� �� ����)
                if (!target.attributes.ContainsKey(modifier.attributeName))
                    return;

                switch (modifier.type)
                {
                    case AttributeModifier.ModifierType.Flat:
                        target.attributes[modifier.attributeName] -= modifier.value * currentStacks;
                        break;
                    case AttributeModifier.ModifierType.Percent:
                        target.attributes[modifier.attributeName] /= (1 + modifier.value * currentStacks);
                        break;
                }
            }
        }

        // Effect Instance (�����丮��)
        [Serializable]
        public class EffectInstance
        {
            public string effectName;
            public float appliedTime;
            public float duration;
        }

        // ���� �ùķ��̼�
        public class CombatSimulation
        {
            private IntegratedCharacter player;
            private IntegratedCharacter enemy;

            public CombatSimulation(IntegratedCharacter p, IntegratedCharacter e)
            {
                player = p;
                enemy = e;
            }

            public async Task RunCombat()
            {
                Debug.Log("\n=== ���� �ùķ��̼� ���� ===");

                // Phase 1: ���� �غ�
                Debug.Log("\n[Phase 1] ���� �غ�");
                player.AddTag("State.Combat");
                enemy.AddTag("State.Combat");

                // ���� ����
                var attackBuff = CreateAttackBuff();
                player.ApplyEffect(attackBuff);

                await Task.Delay(1000);

                // Phase 2: ����
                Debug.Log("\n[Phase 2] ���� ����");

                for (int round = 1; round <= 3; round++)
                {
                    Debug.Log($"\n--- Round {round} ---");

                    // �÷��̾� ����
                    float playerDamage = player.attributes["AttackPower"];
                    enemy.TakeDamage(playerDamage);

                    // DoT ����
                    if (round == 1)
                    {
                        var poisonEffect = CreatePoisonEffect();
                        enemy.ApplyEffect(poisonEffect);
                    }

                    await Task.Delay(1000);

                    // �� �ݰ�
                    if (!enemy.isDead)
                    {
                        float enemyDamage = enemy.attributes["AttackPower"];
                        player.TakeDamage(enemyDamage);

                        // ����� ����
                        if (round == 2)
                        {
                            var slowEffect = CreateSlowEffect();
                            player.ApplyEffect(slowEffect);
                        }
                    }

                    await Task.Delay(1000);

                    if (player.isDead || enemy.isDead)
                        break;
                }

                // Phase 3: ���� ����
                Debug.Log("\n[Phase 3] ���� ����");
                Debug.Log(player.GetStatus());
                Debug.Log(enemy.GetStatus());
            }

            private IntegratedEffect CreateAttackBuff()
            {
                var buff = new IntegratedEffect("���ݷ� ����", IntegratedEffect.DurationType.Duration, 10f);
                buff.modifiers.Add(new IntegratedEffect.AttributeModifier
                {
                    attributeName = "AttackPower",
                    value = 5,
                    type = IntegratedEffect.AttributeModifier.ModifierType.Flat
                });
                buff.grantedTags.Add("Status.Buff.Attack");
                return buff;
            }

            private IntegratedEffect CreatePoisonEffect()
            {
                var poison = new IntegratedEffect("��", IntegratedEffect.DurationType.Duration, 5f);
                poison.isPeriodic = true;
                poison.tickInterval = 1f;
                poison.modifiers.Add(new IntegratedEffect.AttributeModifier
                {
                    attributeName = "Health",
                    value = -3,
                    type = IntegratedEffect.AttributeModifier.ModifierType.Flat
                });
                poison.grantedTags.Add("Status.Debuff.Poison");
                return poison;
            }

            private IntegratedEffect CreateSlowEffect()
            {
                var slow = new IntegratedEffect("���ο�", IntegratedEffect.DurationType.Duration, 3f);
                slow.modifiers.Add(new IntegratedEffect.AttributeModifier
                {
                    attributeName = "MoveSpeed",
                    value = -0.5f,
                    type = IntegratedEffect.AttributeModifier.ModifierType.Percent
                });
                slow.grantedTags.Add("Status.Debuff.Slow");
                return slow;
            }
        }

        [Header("���� �ý���")]
        [SerializeField] private IntegratedCharacter player;
        [SerializeField] private IntegratedCharacter enemy;
        [SerializeField] private CombatSimulation combatSim;

        [Header("�̸� ���ǵ� ȿ��")]
        [SerializeField] private List<IntegratedEffect> predefinedEffects = new List<IntegratedEffect>();

        private async void Start()
        {
            InitializeSystems();
            CreatePredefinedEffects();

            await Task.Delay(1000);

            Debug.Log("=== Phase 3 ���� �׽�Ʈ ���� ===");
            await RunIntegrationTest();
        }

        private void InitializeSystems()
        {
            player = new IntegratedCharacter("Player");
            enemy = new IntegratedCharacter("Enemy");
            combatSim = new CombatSimulation(player, enemy);
        }

        private void CreatePredefinedEffects()
        {
            // ��� ������
            var damage = new IntegratedEffect("���̾", IntegratedEffect.DurationType.Instant);
            damage.modifiers.Add(new IntegratedEffect.AttributeModifier
            {
                attributeName = "Health",
                value = -20,
                type = IntegratedEffect.AttributeModifier.ModifierType.Flat
            });
            predefinedEffects.Add(damage);

            // ��� ȸ��
            var heal = new IntegratedEffect("��", IntegratedEffect.DurationType.Instant);
            heal.modifiers.Add(new IntegratedEffect.AttributeModifier
            {
                attributeName = "Health",
                value = 15,
                type = IntegratedEffect.AttributeModifier.ModifierType.Flat
            });
            predefinedEffects.Add(heal);

            // ����
            var buff = new IntegratedEffect("�Ŀ� ����", IntegratedEffect.DurationType.Duration, 10f);
            buff.modifiers.Add(new IntegratedEffect.AttributeModifier
            {
                attributeName = "AttackPower",
                value = 0.5f,
                type = IntegratedEffect.AttributeModifier.ModifierType.Percent
            });
            buff.grantedTags.Add("Status.Buff.Power");
            predefinedEffects.Add(buff);

            // DoT
            var dot = new IntegratedEffect("��", IntegratedEffect.DurationType.Duration, 6f);
            dot.isPeriodic = true;
            dot.tickInterval = 0.5f;
            dot.modifiers.Add(new IntegratedEffect.AttributeModifier
            {
                attributeName = "Health",
                value = -2,
                type = IntegratedEffect.AttributeModifier.ModifierType.Flat
            });
            dot.grantedTags.Add("Status.Debuff.Burn");
            predefinedEffects.Add(dot);

            // ������ ȿ��
            var stackable = new IntegratedEffect("��ø ��", IntegratedEffect.DurationType.Duration, 8f);
            stackable.isStackable = true;
            stackable.maxStacks = 5;
            stackable.isPeriodic = true;
            stackable.tickInterval = 1f;
            stackable.modifiers.Add(new IntegratedEffect.AttributeModifier
            {
                attributeName = "Health",
                value = -1,
                type = IntegratedEffect.AttributeModifier.ModifierType.Flat
            });
            predefinedEffects.Add(stackable);
        }

        private async Task RunIntegrationTest()
        {
            switch (currentTestMode)
            {
                case TestMode.BasicEffect:
                    await TestBasicEffects();
                    break;
                case TestMode.Duration:
                    await TestDurationEffects();
                    break;
                case TestMode.Periodic:
                    await TestPeriodicEffects();
                    break;
                case TestMode.Integration:
                    await TestFullIntegration();
                    break;
            }
        }

        private async Task TestBasicEffects()
        {
            Debug.Log("\n=== �⺻ ȿ�� �׽�Ʈ ===");

            player.ApplyEffect(predefinedEffects[0]); // ������
            await Task.Delay(1000);

            player.ApplyEffect(predefinedEffects[1]); // ��
            await Task.Delay(1000);
        }

        private async Task TestDurationEffects()
        {
            Debug.Log("\n=== Duration ȿ�� �׽�Ʈ ===");

            player.ApplyEffect(predefinedEffects[2]); // ����
            await Task.Delay(2000);

            enemy.ApplyEffect(predefinedEffects[3]); // DoT
            await Task.Delay(2000);
        }

        private async Task TestPeriodicEffects()
        {
            Debug.Log("\n=== �ֱ��� ȿ�� �׽�Ʈ ===");

            enemy.ApplyEffect(predefinedEffects[3]); // ��
            enemy.ApplyEffect(predefinedEffects[4]); // ��ø ��

            await Task.Delay(3000);
        }

        private async Task TestFullIntegration()
        {
            Debug.Log("\n=== ��ü ���� �׽�Ʈ ===");
            await combatSim.RunCombat();
        }

        private void Update()
        {
            // ȿ�� ������Ʈ
            player?.UpdateEffects(Time.deltaTime);
            enemy?.UpdateEffects(Time.deltaTime);

            HandleInput();
        }

        private void HandleInput()
        {
            // ȿ�� ����
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ApplyEffectToPlayer(0); // ������
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ApplyEffectToPlayer(1); // ��
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ApplyEffectToPlayer(2); // ����
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                ApplyEffectToEnemy(3); // DoT
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                ApplyStackableEffect(); // ������
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                StartCombat();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetAll();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void ApplyEffectToPlayer(int index)
        {
            if (index < 0 || index >= predefinedEffects.Count) return;

            // �� �ν��Ͻ� ����
            var template = predefinedEffects[index];
            var newEffect = new IntegratedEffect(template.effectName, template.durationType, template.duration);
            newEffect.modifiers.AddRange(template.modifiers);
            newEffect.grantedTags.AddRange(template.grantedTags);
            newEffect.isPeriodic = template.isPeriodic;
            newEffect.tickInterval = template.tickInterval;

            player.ApplyEffect(newEffect);
        }

        private void ApplyEffectToEnemy(int index)
        {
            if (index < 0 || index >= predefinedEffects.Count) return;

            var template = predefinedEffects[index];
            var newEffect = new IntegratedEffect(template.effectName, template.durationType, template.duration);
            newEffect.modifiers.AddRange(template.modifiers);
            newEffect.grantedTags.AddRange(template.grantedTags);
            newEffect.isPeriodic = template.isPeriodic;
            newEffect.tickInterval = template.tickInterval;

            enemy.ApplyEffect(newEffect);
        }

        private void ApplyStackableEffect()
        {
            var existing = player.activeEffects.Find(e => e.effectName == "��ø ��");
            if (existing != null && existing.currentStacks < existing.maxStacks)
            {
                existing.currentStacks++;
                Debug.Log($"��ø �� ����: {existing.currentStacks}/{existing.maxStacks}");
            }
            else
            {
                ApplyEffectToPlayer(4);
            }
        }

        private async void StartCombat()
        {
            await combatSim.RunCombat();
        }

        private void ResetAll()
        {
            player = new IntegratedCharacter("Player");
            enemy = new IntegratedCharacter("Enemy");
            combatSim = new CombatSimulation(player, enemy);
            Debug.Log("�ý��� �ʱ�ȭ �Ϸ�!");
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== ���� ���� ===");
            Debug.Log(player.GetStatus());
            Debug.Log(enemy.GetStatus());
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 500, 400), "Phase 3 - ���� �׽�Ʈ");

            // �׽�Ʈ ���
            GUI.Label(new Rect(20, 40, 200, 20), $"�׽�Ʈ ���: {currentTestMode}");

            int y = 70;

            // Player ����
            GUI.Label(new Rect(20, y, 230, 20), "=== Player ===");
            y += 25;
            GUI.Label(new Rect(20, y, 230, 20), $"HP: {player?.health:F0}/{player?.maxHealth:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 230, 20), $"MP: {player?.mana:F0}/{player?.maxMana:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 230, 20), $"ATK: {player?.attributes["AttackPower"]:F0} | DEF: {player?.attributes["Defense"]:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 230, 20), $"SPD: {player?.attributes["MoveSpeed"]:F1}");
            y += 20;
            GUI.Label(new Rect(20, y, 230, 20), $"Effects: {player?.activeEffects.Count ?? 0}");
            y += 20;

            // Player Effects
            if (player != null && player.activeEffects.Count > 0)
            {
                foreach (var effect in player.activeEffects)
                {
                    string info = effect.durationType == IntegratedEffect.DurationType.Duration
                        ? $" ({effect.remainingTime:F1}s)"
                        : effect.durationType == IntegratedEffect.DurationType.Infinite
                        ? " (��)"
                        : "";
                    GUI.Label(new Rect(30, y, 220, 20), $" {effect.effectName}{info}");
                    y += 20;
                }
            }

            // Enemy ����
            y = 95;
            GUI.Label(new Rect(260, y, 230, 20), "=== Enemy ===");
            y += 25;
            GUI.Label(new Rect(260, y, 230, 20), $"HP: {enemy?.health:F0}/{enemy?.maxHealth:F0}");
            y += 20;
            GUI.Label(new Rect(260, y, 230, 20), $"MP: {enemy?.mana:F0}/{enemy?.maxMana:F0}");
            y += 20;
            GUI.Label(new Rect(260, y, 230, 20), $"ATK: {enemy?.attributes["AttackPower"]:F0} | DEF: {enemy?.attributes["Defense"]:F0}");
            y += 20;
            GUI.Label(new Rect(260, y, 230, 20), $"Effects: {enemy?.activeEffects.Count ?? 0}");
            y += 20;

            // Enemy Effects
            if (enemy != null && enemy.activeEffects.Count > 0)
            {
                foreach (var effect in enemy.activeEffects)
                {
                    string info = effect.durationType == IntegratedEffect.DurationType.Duration
                        ? $" ({effect.remainingTime:F1}s)"
                        : "";
                    GUI.Label(new Rect(270, y, 220, 20), $" {effect.effectName}{info}");
                    y += 20;
                }
            }

            // Tags
            y = 250;
            GUI.Label(new Rect(20, y, 480, 20), $"Player Tags: {string.Join(", ", player?.tags ?? new List<string>())}");
            y += 20;
            GUI.Label(new Rect(20, y, 480, 20), $"Enemy Tags: {string.Join(", ", enemy?.tags ?? new List<string>())}");

            // ���۹�
            y = 300;
            GUI.Box(new Rect(10, y, 500, 100), "���۹�");
            y += 25;
            GUI.Label(new Rect(20, y, 480, 70),
                "ȿ��: 1-������ 2-�� 3-���� 4-DoT 5-������\n" +
                "Q: ���� �ùķ��̼�  R: �ʱ�ȭ  Space: ���� ���\n\n" +
                "Phase 1 (Attributes) + Phase 2 (Tags) + Phase 3 (Effects) ����!");
        }
    }
}