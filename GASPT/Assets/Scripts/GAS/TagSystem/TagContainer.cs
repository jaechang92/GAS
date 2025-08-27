// ���� ��ġ: Assets/Scripts/GAS/Learning/Phase2/TagContainer.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAS.Learning.Phase2
{
    /// <summary>
    /// Step 2: TagContainer - ���� �±׸� �����ϴ� �����̳�
    /// </summary>
    public class TagContainer : MonoBehaviour
    {
        // TagContainer ����
        [Serializable]
        public class SimpleTagContainer
        {
            // �±׿� ���� ī��Ʈ�� ����
            private Dictionary<GameplayTag.SimpleGameplayTag, int> tags;

            [SerializeField] private List<string> debugTagList = new List<string>(); // Inspector��

            // �̺�Ʈ
            public event Action<GameplayTag.SimpleGameplayTag> OnTagAdded;
            public event Action<GameplayTag.SimpleGameplayTag> OnTagRemoved;
            public event Action OnAnyTagChanged;

            public int Count => tags?.Count ?? 0;

            public SimpleTagContainer()
            {
                tags = new Dictionary<GameplayTag.SimpleGameplayTag, int>();
            }

            // �±� �߰� (���� ����)
            public bool AddTag(GameplayTag.SimpleGameplayTag tag)
            {
                if (tag == null) return false;

                if (tags.ContainsKey(tag))
                {
                    tags[tag]++;
                    Debug.Log($"[Container] �±� ���� ����: {tag} (����: {tags[tag]})");
                }
                else
                {
                    tags[tag] = 1;
                    debugTagList.Add(tag.FullName);
                    OnTagAdded?.Invoke(tag);
                    Debug.Log($"[Container] �±� �߰�: {tag}");
                }

                OnAnyTagChanged?.Invoke();
                return true;
            }

            // �±� ���� (���� ����)
            public bool RemoveTag(GameplayTag.SimpleGameplayTag tag)
            {
                if (tag == null || !tags.ContainsKey(tag)) return false;

                tags[tag]--;
                Debug.Log($"[Container] �±� ���� ����: {tag} (���� ����: {tags[tag]})");

                if (tags[tag] <= 0)
                {
                    tags.Remove(tag);
                    debugTagList.Remove(tag.FullName);
                    OnTagRemoved?.Invoke(tag);
                    Debug.Log($"[Container] �±� ���� ����: {tag}");
                }

                OnAnyTagChanged?.Invoke();
                return true;
            }

            // Ư�� �±� ���� üũ
            public bool HasTag(GameplayTag.SimpleGameplayTag tag)
            {
                return tag != null && tags.ContainsKey(tag);
            }

            // ������ �±� üũ (�θ� �±׵� ��Ī)
            public bool HasTagMatching(GameplayTag.SimpleGameplayTag parentTag)
            {
                if (parentTag == null) return false;

                foreach (var tag in tags.Keys)
                {
                    if (tag.Matches(parentTag))
                    {
                        return true;
                    }
                }
                return false;
            }

            // ���� ��Ī
            public bool HasTagMatchingPattern(string pattern)
            {
                foreach (var tag in tags.Keys)
                {
                    if (tag.MatchesPattern(pattern))
                    {
                        return true;
                    }
                }
                return false;
            }

            // ��� �±� ���� üũ
            public bool HasAllTags(params GameplayTag.SimpleGameplayTag[] requiredTags)
            {
                foreach (var tag in requiredTags)
                {
                    if (!HasTag(tag))
                    {
                        return false;
                    }
                }
                return true;
            }

            // �ϳ��� ���� üũ
            public bool HasAnyTag(params GameplayTag.SimpleGameplayTag[] checkTags)
            {
                foreach (var tag in checkTags)
                {
                    if (HasTag(tag))
                    {
                        return true;
                    }
                }
                return false;
            }

            // ��� ���� üũ
            public bool HasNoneTags(params GameplayTag.SimpleGameplayTag[] checkTags)
            {
                return !HasAnyTag(checkTags);
            }

            // �±� ���� �� ��������
            public int GetTagStack(GameplayTag.SimpleGameplayTag tag)
            {
                if (tag != null && tags.TryGetValue(tag, out int stack))
                {
                    return stack;
                }
                return 0;
            }

            // ��� �±� ��������
            public List<GameplayTag.SimpleGameplayTag> GetAllTags()
            {
                return new List<GameplayTag.SimpleGameplayTag>(tags.Keys);
            }

            // �����̳� ����
            public void AppendTags(SimpleTagContainer other)
            {
                if (other == null) return;

                foreach (var kvp in other.tags)
                {
                    for (int i = 0; i < kvp.Value; i++)
                    {
                        AddTag(kvp.Key);
                    }
                }
            }

            // �����̳� ������
            public void RemoveTags(SimpleTagContainer other)
            {
                if (other == null) return;

                foreach (var kvp in other.tags)
                {
                    for (int i = 0; i < kvp.Value; i++)
                    {
                        RemoveTag(kvp.Key);
                    }
                }
            }

            // ��� �±� ����
            public void Clear()
            {
                tags.Clear();
                debugTagList.Clear();
                OnAnyTagChanged?.Invoke();
                Debug.Log("[Container] ��� �±� ���ŵ�");
            }

            // ����� ����
            public string GetDebugInfo()
            {
                if (tags.Count == 0) return "�±� ����";

                var info = $"�±� {tags.Count}��:\n";
                foreach (var kvp in tags)
                {
                    info += $"  - {kvp.Key} (����: {kvp.Value})\n";
                }
                return info;
            }
        }

        [Header("ĳ���� �±� �����̳�")]
        [SerializeField] private SimpleTagContainer playerTags;
        [SerializeField] private SimpleTagContainer enemyTags;

        [Header("�׽�Ʈ ����")]
        [SerializeField] private bool showDebugInfo = true;

        // �±� ����
        private GameplayTag.SimpleGameplayTag stateIdleTag;
        private GameplayTag.SimpleGameplayTag stateCombatTag;
        private GameplayTag.SimpleGameplayTag stateAttackingTag;
        private GameplayTag.SimpleGameplayTag statusBuffSpeedTag;
        private GameplayTag.SimpleGameplayTag statusDebuffSlowTag;
        private GameplayTag.SimpleGameplayTag cooldownSkillTag;

        private void Start()
        {
            InitializeContainers();
            RegisterTags();
            SubscribeEvents();

            Debug.Log("=== Phase 2 - Step 2: TagContainer �׽�Ʈ ===");
            RunContainerTests();
        }

        private void InitializeContainers()
        {
            playerTags = new SimpleTagContainer();
            enemyTags = new SimpleTagContainer();
        }

        private void RegisterTags()
        {
            // ���� �±�
            stateIdleTag = GameplayTag.SimpleGameplayTag.RegisterTag("State.Idle");
            stateCombatTag = GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat");
            stateAttackingTag = GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat.Attacking");

            // ����/����� �±�
            statusBuffSpeedTag = GameplayTag.SimpleGameplayTag.RegisterTag("Status.Buff.Speed");
            statusDebuffSlowTag = GameplayTag.SimpleGameplayTag.RegisterTag("Status.Debuff.Slow");

            // ��ٿ� �±�
            cooldownSkillTag = GameplayTag.SimpleGameplayTag.RegisterTag("Cooldown.Skill.Primary");
        }

        private void SubscribeEvents()
        {
            // �÷��̾� �±� �̺�Ʈ
            playerTags.OnTagAdded += (tag) =>
            {
                if (showDebugInfo)
                    Debug.Log($"[Player] �±� �߰���: {tag}");
            };

            playerTags.OnTagRemoved += (tag) =>
            {
                if (showDebugInfo)
                    Debug.Log($"[Player] �±� ���ŵ�: {tag}");
            };

            // �� �±� �̺�Ʈ
            enemyTags.OnAnyTagChanged += () =>
            {
                if (showDebugInfo)
                    Debug.Log($"[Enemy] �±� �����. ���� �±� ��: {enemyTags.Count}");
            };
        }

        private void RunContainerTests()
        {
            TestBasicOperations();
            TestStackSystem();
            TestTagChecking();
            TestContainerOperations();
            TestGameplayScenario();
        }

        // �׽�Ʈ 1: �⺻ �۾�
        private void TestBasicOperations()
        {
            Debug.Log("\n========== �׽�Ʈ 1: �⺻ �۾� ==========");

            playerTags.Clear();

            // �±� �߰�
            playerTags.AddTag(stateIdleTag);
            Debug.Log($"�±� ��: {playerTags.Count}");

            // �±� üũ
            Debug.Log($"Has State.Idle: {playerTags.HasTag(stateIdleTag)}");
            Debug.Log($"Has State.Combat: {playerTags.HasTag(stateCombatTag)}");

            // �±� ����
            playerTags.RemoveTag(stateIdleTag);
            Debug.Log($"���� �� �±� ��: {playerTags.Count}");
        }

        // �׽�Ʈ 2: ���� �ý���
        private void TestStackSystem()
        {
            Debug.Log("\n========== �׽�Ʈ 2: ���� �ý��� ==========");

            playerTags.Clear();

            // ���� �±� ���� �� �߰�
            playerTags.AddTag(statusBuffSpeedTag);
            playerTags.AddTag(statusBuffSpeedTag);
            playerTags.AddTag(statusBuffSpeedTag);

            Debug.Log($"Speed ���� ����: {playerTags.GetTagStack(statusBuffSpeedTag)}");
            Debug.Log($"�±� ��: {playerTags.Count} (�ϳ��� ī��Ʈ)");

            // ���� ����
            playerTags.RemoveTag(statusBuffSpeedTag);
            Debug.Log($"���� �� Speed ���� ����: {playerTags.GetTagStack(statusBuffSpeedTag)}");
            Debug.Log($"������ ���� ����?: {playerTags.HasTag(statusBuffSpeedTag)}");

            // ��� ���� ����
            playerTags.RemoveTag(statusBuffSpeedTag);
            playerTags.RemoveTag(statusBuffSpeedTag);
            Debug.Log($"��� ���� �� ���� ����?: {playerTags.HasTag(statusBuffSpeedTag)}");
        }

        // �׽�Ʈ 3: �±� üũ
        private void TestTagChecking()
        {
            Debug.Log("\n========== �׽�Ʈ 3: �±� üũ ==========");

            playerTags.Clear();
            playerTags.AddTag(stateCombatTag);
            playerTags.AddTag(stateAttackingTag);
            playerTags.AddTag(statusBuffSpeedTag);

            // ������ ��Ī
            var stateTag = GameplayTag.SimpleGameplayTag.RegisterTag("State");
            Debug.Log($"Has parent 'State': {playerTags.HasTagMatching(stateTag)}");

            // ���� ��Ī
            Debug.Log($"Has pattern 'State*': {playerTags.HasTagMatchingPattern("State*")}");
            Debug.Log($"Has pattern 'Status.Buff*': {playerTags.HasTagMatchingPattern("Status.Buff*")}");

            // ���� �±� üũ
            Debug.Log($"Has ALL (Combat, Attacking): {playerTags.HasAllTags(stateCombatTag, stateAttackingTag)}");
            Debug.Log($"Has ALL (Combat, Idle): {playerTags.HasAllTags(stateCombatTag, stateIdleTag)}");
            Debug.Log($"Has ANY (Idle, Combat): {playerTags.HasAnyTag(stateIdleTag, stateCombatTag)}");
            Debug.Log($"Has NONE (Idle, Slow): {playerTags.HasNoneTags(stateIdleTag, statusDebuffSlowTag)}");
        }

        // �׽�Ʈ 4: �����̳� ����
        private void TestContainerOperations()
        {
            Debug.Log("\n========== �׽�Ʈ 4: �����̳� ���� ==========");

            // �÷��̾� ����
            playerTags.Clear();
            playerTags.AddTag(stateCombatTag);
            playerTags.AddTag(statusBuffSpeedTag);

            // �� ����
            enemyTags.Clear();
            enemyTags.AddTag(stateAttackingTag);
            enemyTags.AddTag(statusDebuffSlowTag);

            Debug.Log("�ʱ� ����:");
            Debug.Log($"Player: {string.Join(", ", playerTags.GetAllTags().Select(t => t.ToString()))}");
            Debug.Log($"Enemy: {string.Join(", ", enemyTags.GetAllTags().Select(t => t.ToString()))}");

            // �ӽ� �����̳� ����
            var tempContainer = new SimpleTagContainer();
            tempContainer.AddTag(cooldownSkillTag);
            tempContainer.AddTag(statusDebuffSlowTag);

            // �÷��̾ �߰�
            playerTags.AppendTags(tempContainer);
            Debug.Log($"\n�߰� �� Player: {string.Join(", ", playerTags.GetAllTags().Select(t => t.ToString()))}");

            // �÷��̾�� ����
            playerTags.RemoveTags(tempContainer);
            Debug.Log($"���� �� Player: {string.Join(", ", playerTags.GetAllTags().Select(t => t.ToString()))}");
        }

        // �׽�Ʈ 5: �����÷��� �ó�����
        private void TestGameplayScenario()
        {
            Debug.Log("\n========== �׽�Ʈ 5: �����÷��� �ó����� ==========");

            playerTags.Clear();

            // �ó�����: ���� ����
            Debug.Log("\n[�ó�����] ���� ����");
            playerTags.AddTag(stateCombatTag);

            // ���� ����
            Debug.Log("\n[�ó�����] ���� ����");
            playerTags.AddTag(stateAttackingTag);

            // ��ų ��� ���� üũ
            bool canUseSkill = playerTags.HasTag(stateCombatTag) &&
                              !playerTags.HasTag(cooldownSkillTag);
            Debug.Log($"��ų ��� ����?: {canUseSkill}");

            // ��ų ���
            if (canUseSkill)
            {
                Debug.Log("\n[�ó�����] ��ų ���!");
                playerTags.AddTag(cooldownSkillTag);

                // ���� ����
                playerTags.AddTag(statusBuffSpeedTag);
            }

            // �̵� ���� üũ
            bool canMove = !playerTags.HasTag(stateAttackingTag);
            Debug.Log($"�̵� ����?: {canMove}");

            // ����� ����
            Debug.Log("\n[�ó�����] �����κ��� ���ο� �����");
            playerTags.AddTag(statusDebuffSlowTag);

            // �ӵ� ������ ���ο� ����� ���� üũ
            bool hasSpeedBuff = playerTags.HasTag(statusBuffSpeedTag);
            bool hasSlowDebuff = playerTags.HasTag(statusDebuffSlowTag);
            Debug.Log($"�ӵ� ����: {hasSpeedBuff}, ���ο� �����: {hasSlowDebuff}");

            if (hasSpeedBuff && hasSlowDebuff)
            {
                Debug.Log("������ ������� ����!");
            }

            Debug.Log($"\n���� �±� ����:\n{playerTags.GetDebugInfo()}");
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // �÷��̾� �±� ����
            if (Input.GetKeyDown(KeyCode.Q))
            {
                playerTags.AddTag(stateCombatTag);
                Debug.Log("���� �±� �߰�");
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                playerTags.AddTag(statusBuffSpeedTag);
                Debug.Log("�ӵ� ���� �߰�");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                playerTags.AddTag(statusDebuffSlowTag);
                Debug.Log("���ο� ����� �߰�");
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                playerTags.Clear();
                Debug.Log("��� �±� ����");
            }

            // �� �±� ����
            if (Input.GetKeyDown(KeyCode.A))
            {
                enemyTags.AddTag(stateAttackingTag);
                Debug.Log("[Enemy] ���� �±� �߰�");
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                enemyTags.Clear();
                Debug.Log("[Enemy] ��� �±� ����");
            }

            // ���� ���
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== ���� �±� ���� ===");
            Debug.Log($"[Player]\n{playerTags.GetDebugInfo()}");
            Debug.Log($"[Enemy]\n{enemyTags.GetDebugInfo()}");
        }

        private void OnGUI()
        {
            // �÷��̾� �±� ǥ��
            GUI.Box(new Rect(10, 10, 350, 200), "Phase 2 - TagContainer");

            int y = 40;
            GUI.Label(new Rect(20, y, 330, 20), $"=== Player Tags ({playerTags?.Count ?? 0}) ===");
            y += 25;

            if (playerTags != null && playerTags.Count > 0)
            {
                foreach (var tag in playerTags.GetAllTags())
                {
                    int stack = playerTags.GetTagStack(tag);
                    string stackInfo = stack > 1 ? $" x{stack}" : "";
                    GUI.Label(new Rect(30, y, 320, 20), $" {tag}{stackInfo}");
                    y += 20;
                }
            }
            else
            {
                GUI.Label(new Rect(30, y, 320, 20), "�±� ����");
                y += 20;
            }

            // �� �±� ǥ��
            y += 10;
            GUI.Label(new Rect(20, y, 330, 20), $"=== Enemy Tags ({enemyTags?.Count ?? 0}) ===");
            y += 25;

            if (enemyTags != null && enemyTags.Count > 0)
            {
                foreach (var tag in enemyTags.GetAllTags())
                {
                    GUI.Label(new Rect(30, y, 320, 20), $" {tag}");
                    y += 20;
                }
            }
            else
            {
                GUI.Label(new Rect(30, y, 320, 20), "�±� ����");
                y += 20;
            }

            // ���� ����
            GUI.Box(new Rect(10, 220, 350, 80), "���۹�");
            GUI.Label(new Rect(20, 245, 330, 50),
                "Player: Q-���� W-���� E-����� R-�ʱ�ȭ\n" +
                "Enemy: A-���� S-�ʱ�ȭ\n" +
                "Space: ���� ���");
        }
    }
}