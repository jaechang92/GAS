// ���� ��ġ: Assets/Scripts/GAS/Learning/Phase2/TagRequirement.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAS.Learning.Phase2
{
    /// <summary>
    /// Step 3: TagRequirement - �±� ��� ���� �ý���
    /// </summary>
    public class TagRequirement : MonoBehaviour
    {
        // TagRequirement ����
        [Serializable]
        public class SimpleTagRequirement
        {
            public enum RequirementType
            {
                RequireAll,    // ��� �±� �ʿ� (AND)
                RequireAny,    // �ϳ��� ������ �� (OR)
                RequireNone    // ��� ����� �� (NOT)
            }

            [SerializeField] private string name;
            [SerializeField] private RequirementType requirementType = RequirementType.RequireAll;

            [SerializeField] private List<string> requiredTagNames = new List<string>();
            [SerializeField] private List<string> blockingTagNames = new List<string>();
            [SerializeField] private List<string> ignoredTagNames = new List<string>();

            // ���� �±� ���� (��Ÿ��)
            private List<GameplayTag.SimpleGameplayTag> requiredTags = new List<GameplayTag.SimpleGameplayTag>();
            private List<GameplayTag.SimpleGameplayTag> blockingTags = new List<GameplayTag.SimpleGameplayTag>();
            private List<GameplayTag.SimpleGameplayTag> ignoredTags = new List<GameplayTag.SimpleGameplayTag>();

            public string Name => name;

            public SimpleTagRequirement(string name, RequirementType type = RequirementType.RequireAll)
            {
                this.name = name;
                this.requirementType = type;
            }

            // ���� ����
            public SimpleTagRequirement WithRequiredTags(params string[] tagNames)
            {
                requiredTagNames.AddRange(tagNames);
                foreach (var tagName in tagNames)
                {
                    requiredTags.Add(GameplayTag.SimpleGameplayTag.RegisterTag(tagName));
                }
                return this;
            }

            public SimpleTagRequirement WithBlockingTags(params string[] tagNames)
            {
                blockingTagNames.AddRange(tagNames);
                foreach (var tagName in tagNames)
                {
                    blockingTags.Add(GameplayTag.SimpleGameplayTag.RegisterTag(tagName));
                }
                return this;
            }

            public SimpleTagRequirement WithIgnoredTags(params string[] tagNames)
            {
                ignoredTagNames.AddRange(tagNames);
                foreach (var tagName in tagNames)
                {
                    ignoredTags.Add(GameplayTag.SimpleGameplayTag.RegisterTag(tagName));
                }
                return this;
            }

            public SimpleTagRequirement WithRequirementType(RequirementType type)
            {
                this.requirementType = type;
                return this;
            }

            // ���� ���� üũ
            public bool IsSatisfied(TagContainer.SimpleTagContainer container)
            {
                if (container == null) return false;

                // 1. Blocking �±� üũ (�ϳ��� ������ ����)
                foreach (var blockingTag in blockingTags)
                {
                    if (container.HasTag(blockingTag))
                    {
                        Debug.Log($"[Requirement] {name} ����: ���� �±� {blockingTag} �߰�");
                        return false;
                    }
                }

                // 2. Required �±� üũ
                if (requiredTags.Count > 0)
                {
                    switch (requirementType)
                    {
                        case RequirementType.RequireAll:
                            foreach (var tag in requiredTags)
                            {
                                if (!container.HasTag(tag))
                                {
                                    Debug.Log($"[Requirement] {name} ����: �ʼ� �±� {tag} ����");
                                    return false;
                                }
                            }
                            break;

                        case RequirementType.RequireAny:
                            bool hasAny = false;
                            foreach (var tag in requiredTags)
                            {
                                if (container.HasTag(tag))
                                {
                                    hasAny = true;
                                    break;
                                }
                            }
                            if (!hasAny)
                            {
                                Debug.Log($"[Requirement] {name} ����: �ʼ� �±� �� �ƹ��͵� ����");
                                return false;
                            }
                            break;

                        case RequirementType.RequireNone:
                            foreach (var tag in requiredTags)
                            {
                                if (container.HasTag(tag))
                                {
                                    Debug.Log($"[Requirement] {name} ����: ������ �±� {tag} �߰�");
                                    return false;
                                }
                            }
                            break;
                    }
                }

                Debug.Log($"[Requirement] {name} ����!");
                return true;
            }

            // ������ �±� ��������
            public List<GameplayTag.SimpleGameplayTag> GetMissingTags(TagContainer.SimpleTagContainer container)
            {
                var missing = new List<GameplayTag.SimpleGameplayTag>();

                if (requirementType == RequirementType.RequireAll)
                {
                    foreach (var tag in requiredTags)
                    {
                        if (!container.HasTag(tag))
                        {
                            missing.Add(tag);
                        }
                    }
                }

                return missing;
            }

            // ����� ����
            public string GetDebugInfo()
            {
                var info = $"[{name}] Type: {requirementType}\n";

                if (requiredTags.Count > 0)
                    info += $"  Required: {string.Join(", ", requiredTags.Select(t => t.ToString()))}\n";

                if (blockingTags.Count > 0)
                    info += $"  Blocking: {string.Join(", ", blockingTags.Select(t => t.ToString()))}\n";

                if (ignoredTags.Count > 0)
                    info += $"  Ignored: {string.Join(", ", ignoredTags.Select(t => t.ToString()))}\n";

                return info;
            }
        }

        // ���� ���� (���� Requirement ����)
        [Serializable]
        public class CompositeTagRequirement
        {
            public enum CompositeType
            {
                AND,  // ��� ���� ����
                OR    // �ϳ��� ����
            }

            [SerializeField] private string name;
            [SerializeField] private CompositeType type = CompositeType.AND;
            [SerializeField] private List<SimpleTagRequirement> requirements = new List<SimpleTagRequirement>();

            public CompositeTagRequirement(string name, CompositeType type = CompositeType.AND)
            {
                this.name = name;
                this.type = type;
            }

            public void AddRequirement(SimpleTagRequirement requirement)
            {
                requirements.Add(requirement);
            }

            public bool IsSatisfied(TagContainer.SimpleTagContainer container)
            {
                if (requirements.Count == 0) return true;

                switch (type)
                {
                    case CompositeType.AND:
                        foreach (var req in requirements)
                        {
                            if (!req.IsSatisfied(container))
                            {
                                Debug.Log($"[Composite] {name} (AND) ����: {req.Name} ���� �Ҹ���");
                                return false;
                            }
                        }
                        Debug.Log($"[Composite] {name} (AND) ����!");
                        return true;

                    case CompositeType.OR:
                        foreach (var req in requirements)
                        {
                            if (req.IsSatisfied(container))
                            {
                                Debug.Log($"[Composite] {name} (OR) ����: {req.Name} ���� ����");
                                return true;
                            }
                        }
                        Debug.Log($"[Composite] {name} (OR) ����: ��� ���� �Ҹ���");
                        return false;

                    default:
                        return false;
                }
            }
        }

        // ��ų/�ɷ� ����
        [Serializable]
        public class AbilityDefinition
        {
            public string name;
            public SimpleTagRequirement activationRequirement;
            public List<string> grantedTags = new List<string>();
            public List<string> removedTags = new List<string>();

            public AbilityDefinition(string name)
            {
                this.name = name;
                this.activationRequirement = new SimpleTagRequirement($"{name}_Requirement");
            }

            public bool CanActivate(TagContainer.SimpleTagContainer container)
            {
                return activationRequirement.IsSatisfied(container);
            }

            public void Execute(TagContainer.SimpleTagContainer container)
            {
                if (!CanActivate(container))
                {
                    Debug.LogWarning($"[Ability] {name} Ȱ��ȭ ���� �Ҹ���!");
                    return;
                }

                Debug.Log($"[Ability] {name} ����!");

                // �±� ����
                foreach (var tagName in removedTags)
                {
                    var tag = GameplayTag.SimpleGameplayTag.RegisterTag(tagName);
                    container.RemoveTag(tag);
                }

                // �±� �ο�
                foreach (var tagName in grantedTags)
                {
                    var tag = GameplayTag.SimpleGameplayTag.RegisterTag(tagName);
                    container.AddTag(tag);
                }
            }
        }

        [Header("ĳ���� �±�")]
        [SerializeField] private TagContainer.SimpleTagContainer characterTags;

        [Header("�ɷ� ����")]
        [SerializeField] private List<AbilityDefinition> abilities = new List<AbilityDefinition>();

        [Header("�׽�Ʈ ����")]
        [SerializeField] private bool verboseLogging = true;

        private void Start()
        {
            InitializeSystem();
            Debug.Log("=== Phase 2 - Step 3: TagRequirement �׽�Ʈ ===");
            RunRequirementTests();
        }

        private void InitializeSystem()
        {
            characterTags = new TagContainer.SimpleTagContainer();
            SetupAbilities();
        }

        private void SetupAbilities()
        {
            // �⺻ ����
            var basicAttack = new AbilityDefinition("�⺻ ����");
            basicAttack.activationRequirement
                .WithRequiredTags("State.Combat")
                .WithBlockingTags("State.Dead", "Status.Stunned")
                .WithRequirementType(SimpleTagRequirement.RequirementType.RequireAll);
            basicAttack.grantedTags.Add("State.Combat.Attacking");
            basicAttack.grantedTags.Add("Cooldown.Attack");
            abilities.Add(basicAttack);

            // ���
            var defend = new AbilityDefinition("���");
            defend.activationRequirement
                .WithRequiredTags("State.Combat")
                .WithBlockingTags("State.Combat.Attacking", "Status.Stunned")
                .WithRequirementType(SimpleTagRequirement.RequirementType.RequireAll);
            defend.grantedTags.Add("State.Combat.Defending");
            defend.grantedTags.Add("Status.Buff.DefenseUp");
            abilities.Add(defend);

            // �ñر�
            var ultimate = new AbilityDefinition("�ñر�");
            ultimate.activationRequirement
                .WithRequiredTags("State.Combat", "Resource.UltimateReady")
                .WithBlockingTags("Cooldown.Ultimate", "Status.Silenced")
                .WithRequirementType(SimpleTagRequirement.RequirementType.RequireAll);
            ultimate.grantedTags.Add("State.Ultimate");
            ultimate.grantedTags.Add("Status.Buff.Invincible");
            ultimate.grantedTags.Add("Cooldown.Ultimate");
            ultimate.removedTags.Add("Resource.UltimateReady");
            abilities.Add(ultimate);

            // ����
            var flee = new AbilityDefinition("����");
            flee.activationRequirement
                .WithRequiredTags("State.Combat")
                .WithBlockingTags("Status.Rooted", "State.Combat.Attacking")
                .WithRequirementType(SimpleTagRequirement.RequirementType.RequireAll);
            flee.removedTags.Add("State.Combat");
            flee.grantedTags.Add("State.Fleeing");
            abilities.Add(flee);
        }

        private void RunRequirementTests()
        {
            TestBasicRequirements();
            TestComplexRequirements();
            TestCompositeRequirements();
            TestAbilitySystem();
        }

        // �׽�Ʈ 1: �⺻ �䱸����
        private void TestBasicRequirements()
        {
            Debug.Log("\n========== �׽�Ʈ 1: �⺻ �䱸���� ==========");

            characterTags.Clear();

            // RequireAll �׽�Ʈ
            var req1 = new SimpleTagRequirement("���� �غ� üũ", SimpleTagRequirement.RequirementType.RequireAll)
                .WithRequiredTags("State.Combat", "Resource.Mana");

            Debug.Log(req1.GetDebugInfo());

            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat"));
            Debug.Log($"State.Combat�� ���� ��: {req1.IsSatisfied(characterTags)}");

            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.Mana"));
            Debug.Log($"��� ���� ��: {req1.IsSatisfied(characterTags)}");

            // RequireAny �׽�Ʈ
            var req2 = new SimpleTagRequirement("���� üũ", SimpleTagRequirement.RequirementType.RequireAny)
                .WithRequiredTags("Status.Buff.Speed", "Status.Buff.Strength");

            Debug.Log($"\n���� ���� ��: {req2.IsSatisfied(characterTags)}");

            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Buff.Speed"));
            Debug.Log($"Speed ������: {req2.IsSatisfied(characterTags)}");
        }

        // �׽�Ʈ 2: ������ �䱸����
        private void TestComplexRequirements()
        {
            Debug.Log("\n========== �׽�Ʈ 2: ������ �䱸���� ==========");

            characterTags.Clear();

            // �ʼ� + ���� �±�
            var complexReq = new SimpleTagRequirement("��ų ��� ����", SimpleTagRequirement.RequirementType.RequireAll)
                .WithRequiredTags("State.Combat", "Resource.Mana")
                .WithBlockingTags("Status.Silenced", "Cooldown.Skill");

            Debug.Log(complexReq.GetDebugInfo());

            // ���� ����
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat"));
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.Mana"));
            Debug.Log($"���� ����: {complexReq.IsSatisfied(characterTags)}");

            // ���� �±� �߰�
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Silenced"));
            Debug.Log($"ħ�� ����: {complexReq.IsSatisfied(characterTags)}");

            // ������ �±� Ȯ��
            characterTags.Clear();
            var missing = complexReq.GetMissingTags(characterTags);
            Debug.Log($"������ �±�: {string.Join(", ", missing.Select(t => t.ToString()))}");
        }

        // �׽�Ʈ 3: ���� ����
        private void TestCompositeRequirements()
        {
            Debug.Log("\n========== �׽�Ʈ 3: ���� ���� ==========");

            characterTags.Clear();

            // OR ���� ����
            var composite = new CompositeTagRequirement("Ư�� ���� ����", CompositeTagRequirement.CompositeType.OR);

            // ���� 1: ����ũ ���
            var berserkReq = new SimpleTagRequirement("����ũ", SimpleTagRequirement.RequirementType.RequireAll)
                .WithRequiredTags("Status.Berserk");

            // ���� 2: Ǯ �޺�
            var comboReq = new SimpleTagRequirement("�޺�", SimpleTagRequirement.RequirementType.RequireAll)
                .WithRequiredTags("Combo.Full");

            // ���� 3: �ñر� �غ�
            var ultReq = new SimpleTagRequirement("�ñر�", SimpleTagRequirement.RequirementType.RequireAll)
                .WithRequiredTags("Resource.UltimateReady");

            composite.AddRequirement(berserkReq);
            composite.AddRequirement(comboReq);
            composite.AddRequirement(ultReq);

            Debug.Log("�ƹ� ���ǵ� ���� ��: " + composite.IsSatisfied(characterTags));

            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Combo.Full"));
            Debug.Log("�޺��� ���� ��: " + composite.IsSatisfied(characterTags));
        }

        // �׽�Ʈ 4: �ɷ� �ý���
        private void TestAbilitySystem()
        {
            Debug.Log("\n========== �׽�Ʈ 4: �ɷ� �ý��� ==========");

            characterTags.Clear();

            // �ʱ� ���� ����
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat"));
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.UltimateReady"));

            Debug.Log("�ʱ� �±�:");
            Debug.Log(characterTags.GetDebugInfo());

            // �� �ɷ� üũ
            Debug.Log("\n=== �ɷ� ��� ���� üũ ===");
            foreach (var ability in abilities)
            {
                bool canUse = ability.CanActivate(characterTags);
                Debug.Log($"{ability.name}: {(canUse ? "����" : "�Ұ���")}");

                if (ability.name == "�⺻ ����" && canUse)
                {
                    ability.Execute(characterTags);
                    Debug.Log("\n���� �� �±�:");
                    Debug.Log(characterTags.GetDebugInfo());
                }
            }

            // �ñر� �׽�Ʈ
            Debug.Log("\n=== �ñر� �׽�Ʈ ===");
            characterTags.Clear();
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat"));
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.UltimateReady"));

            var ultimateAbility = abilities.Find(a => a.name == "�ñر�");
            if (ultimateAbility != null && ultimateAbility.CanActivate(characterTags))
            {
                ultimateAbility.Execute(characterTags);
                Debug.Log("\n�ñر� ��� �� �±�:");
                Debug.Log(characterTags.GetDebugInfo());
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // �ɷ� ���
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TryUseAbility(0); // �⺻ ����
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TryUseAbility(1); // ���
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TryUseAbility(2); // �ñر�
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TryUseAbility(3); // ����
            }

            // �±� ����
            if (Input.GetKeyDown(KeyCode.Q))
            {
                characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat"));
                Debug.Log("���� ���� ����");
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.UltimateReady"));
                Debug.Log("�ñر� �غ� �Ϸ�");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Stunned"));
                Debug.Log("���� ����");
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                characterTags.Clear();
                Debug.Log("��� �±� ����");
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void TryUseAbility(int index)
        {
            if (index < 0 || index >= abilities.Count) return;

            var ability = abilities[index];
            if (ability.CanActivate(characterTags))
            {
                ability.Execute(characterTags);
            }
            else
            {
                Debug.LogWarning($"{ability.name} ��� �Ұ�!");

                // ������ �±� ǥ��
                var missing = ability.activationRequirement.GetMissingTags(characterTags);
                if (missing.Count > 0)
                {
                    Debug.Log($"������ �±�: {string.Join(", ", missing.Select(t => t.ToString()))}");
                }
            }
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== ���� ���� ===");
            Debug.Log(characterTags.GetDebugInfo());

            Debug.Log("\n=== ��� ������ �ɷ� ===");
            foreach (var ability in abilities)
            {
                if (ability.CanActivate(characterTags))
                {
                    Debug.Log($" {ability.name}");
                }
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 400, 300), "Phase 2 - TagRequirement");

            int y = 40;

            // ���� �±�
            GUI.Label(new Rect(20, y, 380, 20), $"=== ���� �±� ({characterTags?.Count ?? 0}) ===");
            y += 25;

            if (characterTags != null && characterTags.Count > 0)
            {
                var tags = characterTags.GetAllTags();
                int col = 0;
                foreach (var tag in tags)
                {
                    GUI.Label(new Rect(30 + (col * 180), y, 170, 20), $" {tag}");
                    col++;
                    if (col >= 2)
                    {
                        col = 0;
                        y += 20;
                    }
                }
                if (col > 0) y += 20;
            }
            else
            {
                GUI.Label(new Rect(30, y, 360, 20), "�±� ����");
                y += 20;
            }

            // �ɷ� ����
            y += 10;
            GUI.Label(new Rect(20, y, 380, 20), "=== �ɷ� ���� ===");
            y += 25;

            for (int i = 0; i < abilities.Count && i < 4; i++)
            {
                var ability = abilities[i];
                bool canUse = ability.CanActivate(characterTags);
                string status = canUse ? "can Use" : "can't Use";
                GUI.Label(new Rect(30, y, 360, 20), $"{i + 1}. {ability.name} [{status}]");
                y += 20;
            }

            // ���۹�
            y += 10;
            GUI.Box(new Rect(10, y, 400, 100), "���۹�");
            y += 25;
            GUI.Label(new Rect(20, y, 380, 70),
                "�ɷ�: 1-�⺻���� 2-��� 3-�ñر� 4-����\n" +
                "�±�: Q-���� W-�ñر��غ� E-���� R-�ʱ�ȭ\n" +
                "Space: ���� ���");
        }
    }
}