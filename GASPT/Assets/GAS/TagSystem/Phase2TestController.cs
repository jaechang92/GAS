// ���� ��ġ: Assets/Scripts/GAS/Learning/Phase2/Phase2TestController.cs
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAS.Learning.Phase2
{
    /// <summary>
    /// Phase 2 ���� �׽�Ʈ ��Ʈ�ѷ�
    /// Tag System�� ��� ������ ������ �����ϴ� ����
    /// </summary>
    public class Phase2TestController : MonoBehaviour
    {
        [Header("�׽�Ʈ ���")]
        [SerializeField] private TestMode currentTestMode = TestMode.Integration;

        private enum TestMode
        {
            BasicTag,       // Step 1 �׽�Ʈ
            Container,      // Step 2 �׽�Ʈ
            Requirement,    // Step 3 �׽�Ʈ
            Integration     // ���� �׽�Ʈ
        }

        // ���� ĳ���� �ý���
        [Serializable]
        public class CharacterTagSystem
        {
            public string characterName;
            public TagContainer.SimpleTagContainer tags;
            public List<Skill> skills = new List<Skill>();
            public CharacterState currentState = CharacterState.Idle;

            public enum CharacterState
            {
                Idle,
                Combat,
                Moving,
                Casting,
                Dead
            }

            [Serializable]
            public class Skill
            {
                public string name;
                public float cooldown;
                public float remainingCooldown;
                public TagRequirement.SimpleTagRequirement requirement;
                public List<string> effectTags = new List<string>();

                public Skill(string name, float cooldown)
                {
                    this.name = name;
                    this.cooldown = cooldown;
                    this.requirement = new TagRequirement.SimpleTagRequirement(
                        $"{name}_Requirement",
                        TagRequirement.SimpleTagRequirement.RequirementType.RequireAll
                    );
                }

                public bool IsReady()
                {
                    return remainingCooldown <= 0;
                }

                public void Use(TagContainer.SimpleTagContainer container)
                {
                    if (!IsReady() || !requirement.IsSatisfied(container))
                    {
                        Debug.LogWarning($"[Skill] {name} ��� �Ұ�!");
                        return;
                    }

                    remainingCooldown = cooldown;

                    // ȿ�� �±� ����
                    foreach (var tagName in effectTags)
                    {
                        var tag = GameplayTag.SimpleGameplayTag.RegisterTag(tagName);
                        container.AddTag(tag);
                    }

                    Debug.Log($"[Skill] {name} ���! (��ٿ�: {cooldown}��)");
                }

                public void UpdateCooldown(float deltaTime)
                {
                    if (remainingCooldown > 0)
                    {
                        remainingCooldown -= deltaTime;
                        if (remainingCooldown <= 0)
                        {
                            Debug.Log($"[Skill] {name} �غ� �Ϸ�!");
                        }
                    }
                }
            }

            public void Initialize(string name)
            {
                characterName = name;
                tags = new TagContainer.SimpleTagContainer();
                InitializeSkills();
                UpdateStateTags();
            }

            private void InitializeSkills()
            {
                // ��ų 1: ���̾
                var fireball = new Skill("Fireball", 3f);
                fireball.requirement
                    .WithRequiredTags("State.Combat", "Resource.Mana")
                    .WithBlockingTags("Status.Silenced", "State.Dead");
                fireball.effectTags.Add("Damage.Fire");
                fireball.effectTags.Add("Effect.Burn");
                skills.Add(fireball);

                // ��ų 2: ��
                var heal = new Skill("Heal", 5f);
                heal.requirement
                    .WithRequiredTags("Resource.Mana")
                    .WithBlockingTags("Status.Silenced", "State.Dead");
                heal.effectTags.Add("Effect.Heal");
                heal.effectTags.Add("Status.Buff.Regeneration");
                skills.Add(heal);

                // ��ų 3: �뽬
                var dash = new Skill("Dash", 2f);
                dash.requirement
                    .WithRequiredTags("Resource.Stamina")
                    .WithBlockingTags("Status.Rooted", "State.Dead");
                dash.effectTags.Add("State.Movement.Dashing");
                dash.effectTags.Add("Status.Buff.Speed");
                skills.Add(dash);
            }

            public void ChangeState(CharacterState newState)
            {
                if (currentState == newState) return;

                currentState = newState;
                UpdateStateTags();
                Debug.Log($"[{characterName}] ���� ����: {newState}");
            }

            private void UpdateStateTags()
            {
                // ��� ���� �±� ����
                RemoveAllStateTags();

                // �� ���� �±� �߰�
                switch (currentState)
                {
                    case CharacterState.Idle:
                        tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Idle"));
                        break;
                    case CharacterState.Combat:
                        tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat"));
                        break;
                    case CharacterState.Moving:
                        tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Movement"));
                        break;
                    case CharacterState.Casting:
                        tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Casting"));
                        break;
                    case CharacterState.Dead:
                        tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Dead"));
                        break;
                }
            }

            private void RemoveAllStateTags()
            {
                var stateTags = tags.GetAllTags().Where(t => t.MatchesPattern("State*")).ToList();
                foreach (var tag in stateTags)
                {
                    tags.RemoveTag(tag);
                }
            }

            public void UpdateSkillCooldowns(float deltaTime)
            {
                foreach (var skill in skills)
                {
                    skill.UpdateCooldown(deltaTime);
                }
            }
        }

        // �޺� �ý���
        [Serializable]
        public class ComboSystem
        {
            private Queue<string> comboSequence = new Queue<string>();
            private float comboTimer = 0f;
            private float comboWindow = 2f;

            [SerializeField]
            private SerializableDictionary<string, List<string>> comboDictionary = new SerializableDictionary<string, List<string>>();

            public void Initialize()
            {
                // �޺� ����
                comboDictionary["FireStorm"] = new List<string> { "Fire", "Fire", "Wind" };
                comboDictionary["IceSpear"] = new List<string> { "Ice", "Ice", "Ice" };
                comboDictionary["Lightning"] = new List<string> { "Fire", "Ice", "Wind" };
            }

            public void AddInput(string input)
            {
                if (comboTimer <= 0)
                {
                    comboSequence.Clear();
                }

                comboSequence.Enqueue(input);
                comboTimer = comboWindow;

                // �ִ� 5�� �Է¸� ����
                while (comboSequence.Count > 5)
                {
                    comboSequence.Dequeue();
                }

                Debug.Log($"[Combo] �Է�: {input} | ������: {string.Join("-", comboSequence)}");

                CheckCombos();
            }

            private void CheckCombos()
            {
                var currentSequence = comboSequence.ToList();

                foreach (var combo in comboDictionary)
                {
                    if (MatchesCombo(currentSequence, combo.Value))
                    {
                        ExecuteCombo(combo.Key);
                        comboSequence.Clear();
                        break;
                    }
                }
            }

            private bool MatchesCombo(List<string> sequence, List<string> pattern)
            {
                if (sequence.Count < pattern.Count) return false;

                // �ڿ������� ���� ��Ī
                int startIndex = sequence.Count - pattern.Count;
                for (int i = 0; i < pattern.Count; i++)
                {
                    if (sequence[startIndex + i] != pattern[i])
                        return false;
                }
                return true;
            }

            private void ExecuteCombo(string comboName)
            {
                Debug.Log($"�ڡڡ� �޺� �ߵ�: {comboName} �ڡڡ�");
                // �޺� ȿ�� ����
            }

            public void Update(float deltaTime)
            {
                if (comboTimer > 0)
                {
                    comboTimer -= deltaTime;
                    if (comboTimer <= 0)
                    {
                        comboSequence.Clear();
                        Debug.Log("[Combo] �޺� Ÿ�̸� ����");
                    }
                }
            }
        }

        [Header("���� �ý���")]
        [SerializeField] private CharacterTagSystem playerSystem;
        [SerializeField] private CharacterTagSystem enemySystem;
        [SerializeField] private ComboSystem comboSystem;

        [Header("���ҽ�")]
        [SerializeField] private float playerMana = 100f;
        [SerializeField] private float playerStamina = 100f;

        private async void Start()
        {
            InitializeSystems();

            await Task.Delay(1000);

            Debug.Log("=== Phase 2 ���� �׽�Ʈ ���� ===");
            await RunIntegrationTest();
        }

        private void InitializeSystems()
        {
            // �÷��̾� �ʱ�ȭ
            playerSystem = new CharacterTagSystem();
            playerSystem.Initialize("Player");

            // �� �ʱ�ȭ
            enemySystem = new CharacterTagSystem();
            enemySystem.Initialize("Enemy");

            // �޺� �ý��� �ʱ�ȭ
            comboSystem = new ComboSystem();
            comboSystem.Initialize();

            // �ʱ� ���ҽ� �±�
            UpdateResourceTags();
        }

        private void UpdateResourceTags()
        {
            // ���� �±�
            if (playerMana > 20)
            {
                playerSystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.Mana"));
            }
            else
            {
                var manaTag = GameplayTag.SimpleGameplayTag.RegisterTag("Resource.Mana");
                playerSystem.tags.RemoveTag(manaTag);
            }

            // ���¹̳� �±�
            if (playerStamina > 10)
            {
                playerSystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.Stamina"));
            }
            else
            {
                var staminaTag = GameplayTag.SimpleGameplayTag.RegisterTag("Resource.Stamina");
                playerSystem.tags.RemoveTag(staminaTag);
            }
        }

        private async Task RunIntegrationTest()
        {
            switch (currentTestMode)
            {
                case TestMode.BasicTag:
                    await TestBasicTags();
                    break;
                case TestMode.Container:
                    await TestContainerOperations();
                    break;
                case TestMode.Requirement:
                    await TestRequirementSystem();
                    break;
                case TestMode.Integration:
                    await TestFullIntegration();
                    break;
            }
        }

        private async Task TestBasicTags()
        {
            Debug.Log("\n=== �⺻ �±� �׽�Ʈ ===");

            var tag1 = GameplayTag.SimpleGameplayTag.RegisterTag("Test.Parent");
            var tag2 = GameplayTag.SimpleGameplayTag.RegisterTag("Test.Parent.Child");

            Debug.Log($"Child matches Parent: {tag2.Matches(tag1)}");
            Debug.Log($"Parent matches Child: {tag1.Matches(tag2)}");

            await Task.Delay(1000);
        }

        private async Task TestContainerOperations()
        {
            Debug.Log("\n=== �����̳� �۾� �׽�Ʈ ===");

            playerSystem.tags.Clear();
            playerSystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Test.Tag1"));
            playerSystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Test.Tag2"));

            Debug.Log($"�±� ��: {playerSystem.tags.Count}");
            Debug.Log($"Has Test.Tag1: {playerSystem.tags.HasTag(GameplayTag.SimpleGameplayTag.RegisterTag("Test.Tag1"))}");

            await Task.Delay(1000);
        }

        private async Task TestRequirementSystem()
        {
            Debug.Log("\n=== �䱸���� �ý��� �׽�Ʈ ===");

            var requirement = new TagRequirement.SimpleTagRequirement("�׽�Ʈ �䱸����")
                .WithRequiredTags("State.Combat")
                .WithBlockingTags("State.Dead");

            playerSystem.ChangeState(CharacterTagSystem.CharacterState.Combat);
            Debug.Log($"���� ���¿���: {requirement.IsSatisfied(playerSystem.tags)}");

            playerSystem.ChangeState(CharacterTagSystem.CharacterState.Dead);
            Debug.Log($"���� ���¿���: {requirement.IsSatisfied(playerSystem.tags)}");

            await Task.Delay(1000);
        }

        private async Task TestFullIntegration()
        {
            Debug.Log("\n=== ��ü ���� �׽�Ʈ ===");

            // 1�ܰ�: ���� �غ�
            Debug.Log("\n[1�ܰ�] ���� �غ�");
            playerSystem.ChangeState(CharacterTagSystem.CharacterState.Idle);
            enemySystem.ChangeState(CharacterTagSystem.CharacterState.Idle);

            await Task.Delay(1500);

            // 2�ܰ�: ���� ����
            Debug.Log("\n[2�ܰ�] ���� ����");
            playerSystem.ChangeState(CharacterTagSystem.CharacterState.Combat);
            enemySystem.ChangeState(CharacterTagSystem.CharacterState.Combat);

            // ���� ����
            playerSystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Buff.AttackUp"));
            Debug.Log("���ݷ� ���� ����!");

            await Task.Delay(1500);

            // 3�ܰ�: ��ų ���
            Debug.Log("\n[3�ܰ�] ��ų ���");

            // ��ų ��� ���� üũ
            foreach (var skill in playerSystem.skills)
            {
                bool canUse = skill.IsReady() && skill.requirement.IsSatisfied(playerSystem.tags);
                Debug.Log($"{skill.name}: {(canUse ? "��� ����" : "��� �Ұ�")}");

                if (canUse && skill.name == "Fireball")
                {
                    skill.Use(playerSystem.tags);
                    ConsumeResource("Mana", 20);
                }
            }

            await Task.Delay(1500);

            // 4�ܰ�: ���� �̻�
            Debug.Log("\n[4�ܰ�] ���� �̻� ����");

            enemySystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Debuff.Poison"));
            enemySystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Debuff.Slow"));
            Debug.Log("������ ��, ���ο� ����!");

            // ����� üũ
            bool isPoisoned = enemySystem.tags.HasTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Debuff.Poison"));
            bool isSlowed = enemySystem.tags.HasTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Debuff.Slow"));
            Debug.Log($"�� ���� - �ߵ�: {isPoisoned}, ���ο�: {isSlowed}");

            await Task.Delay(1500);

            // 5�ܰ�: ���� ����
            Debug.Log("\n[5�ܰ�] ���� ����");
            playerSystem.ChangeState(CharacterTagSystem.CharacterState.Idle);
            enemySystem.ChangeState(CharacterTagSystem.CharacterState.Dead);

            Debug.Log("\n���� �±� ����:");
            Debug.Log($"[Player] {playerSystem.tags.GetDebugInfo()}");
            Debug.Log($"[Enemy] {enemySystem.tags.GetDebugInfo()}");
        }

        private void ConsumeResource(string resourceType, float amount)
        {
            switch (resourceType)
            {
                case "Mana":
                    playerMana -= amount;
                    Debug.Log($"���� �Һ�: {amount} (���� ����: {playerMana})");
                    break;
                case "Stamina":
                    playerStamina -= amount;
                    Debug.Log($"���¹̳� �Һ�: {amount} (���� ���¹̳�: {playerStamina})");
                    break;
            }
            UpdateResourceTags();
        }

        private void Update()
        {
            // ��ų ��ٿ� ������Ʈ
            if (playerSystem != null)
            {
                playerSystem.UpdateSkillCooldowns(Time.deltaTime);
            }
            if (enemySystem != null)
            {
                enemySystem.UpdateSkillCooldowns(Time.deltaTime);
            }

            // �޺� �ý��� ������Ʈ
            if (comboSystem != null)
            {
                comboSystem.Update(Time.deltaTime);
            }

            // ���ҽ� ȸ��
            playerMana = Mathf.Min(100, playerMana + Time.deltaTime * 5f);
            playerStamina = Mathf.Min(100, playerStamina + Time.deltaTime * 10f);

            // �Է� ó��
            HandleInput();
        }

        private void HandleInput()
        {
            // ���� ����
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                playerSystem.ChangeState(CharacterTagSystem.CharacterState.Idle);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playerSystem.ChangeState(CharacterTagSystem.CharacterState.Combat);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playerSystem.ChangeState(CharacterTagSystem.CharacterState.Moving);
            }

            // ��ų ���
            if (Input.GetKeyDown(KeyCode.Q))
            {
                TryUseSkill(0); // Fireball
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                TryUseSkill(1); // Heal
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryUseSkill(2); // Dash
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ClearPlayerTag(); // �±� �ʱ�ȭ
            }

            // �޺� �Է�
            if (Input.GetKeyDown(KeyCode.Z))
            {
                comboSystem.AddInput("Fire");
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                comboSystem.AddInput("Ice");
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                comboSystem.AddInput("Wind");
            }

            // ���� ���
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void TryUseSkill(int index)
        {
            if (index < 0 || index >= playerSystem.skills.Count) return;

            var skill = playerSystem.skills[index];
            skill.Use(playerSystem.tags);

            // ���ҽ� �Һ�
            if (skill.name == "Fireball" || skill.name == "Heal")
            {
                ConsumeResource("Mana", 20);
            }
            else if (skill.name == "Dash")
            {
                ConsumeResource("Stamina", 10);
            }
        }

        private void ClearPlayerTag()
        {
            playerSystem.tags.Clear();

        }

        private void PrintStatus()
        {
            Debug.Log("\n=== ���� ���� ===");
            Debug.Log($"[Player] State: {playerSystem.currentState}");
            Debug.Log(playerSystem.tags.GetDebugInfo());
            Debug.Log($"Mana: {playerMana:F0}/100 | Stamina: {playerStamina:F0}/100");

            Debug.Log($"\n[Enemy] State: {enemySystem.currentState}");
            Debug.Log(enemySystem.tags.GetDebugInfo());
        }

        private void OnGUI()
        {
            // ���� UI
            GUI.Box(new Rect(10, 10, 450, 350), "Phase 2 - ���� �׽�Ʈ");

            // �׽�Ʈ ���
            GUI.Label(new Rect(20, 40, 200, 20), $"�׽�Ʈ ���: {currentTestMode}");

            int y = 70;

            // �÷��̾� ����
            GUI.Label(new Rect(20, y, 200, 20), $"=== �÷��̾� ===");
            y += 25;
            GUI.Label(new Rect(20, y, 200, 20), $"����: {playerSystem?.currentState}");
            y += 20;
            GUI.Label(new Rect(20, y, 200, 20), $"�±�: {playerSystem?.tags.Count ?? 0}��");
            y += 20;

            // ���ҽ�
            GUI.Label(new Rect(20, y, 200, 20), $"����: {playerMana:F0}/100");
            y += 20;
            GUI.Label(new Rect(20, y, 200, 20), $"���¹̳�: {playerStamina:F0}/100");
            y += 25;

            // ��ų ��ٿ�
            GUI.Label(new Rect(20, y, 200, 20), "=== ��ų ===");
            y += 25;

            if (playerSystem != null)
            {
                for (int i = 0; i < playerSystem.skills.Count && i < 3; i++)
                {
                    var skill = playerSystem.skills[i];
                    string cooldownText = skill.IsReady() ? "�غ��" : $"{skill.remainingCooldown:F1}��";
                    GUI.Label(new Rect(20, y, 200, 20), $"{skill.name}: {cooldownText}");
                    y += 20;
                }
            }

            // �� ����
            y = 95;
            GUI.Label(new Rect(240, y, 200, 20), $"=== �� ===");
            y += 25;
            GUI.Label(new Rect(240, y, 200, 20), $"����: {enemySystem?.currentState}");
            y += 20;
            GUI.Label(new Rect(240, y, 200, 20), $"�±�: {enemySystem?.tags.Count ?? 0}��");

            // ���۹�
            y = 250;
            GUI.Box(new Rect(10, y, 450, 100), "���۹�");
            y += 25;
            GUI.Label(new Rect(20, y, 430, 70),
                "����: 1-Idle 2-Combat 3-Moving\n" +
                "��ų: Q-Fireball W-Heal E-Dash\n" +
                "�޺�: Z-Fire X-Ice C-Wind (Fire-Fire-Wind = FireStorm)\n" +
                "Space: ���� ���");
        }
    }
}