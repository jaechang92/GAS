// ���� ��ġ: Assets/Scripts/GAS/Learning/Phase2/GameplayTag.cs
using UnityEngine;
using System;
using System.Collections.Generic;

namespace GAS.Learning.Phase2
{
    /// <summary>
    /// Step 1: GameplayTag ���� - ������ �±� �ý��� ����
    /// </summary>
    public class GameplayTag : MonoBehaviour
    {
        // ������ GameplayTag ����
        [Serializable]
        public class SimpleGameplayTag
        {
            [SerializeField] private string fullName;
            private string[] segments;
            private static Dictionary<string, SimpleGameplayTag> registeredTags = new Dictionary<string, SimpleGameplayTag>();

            public string FullName => fullName;
            public string[] Segments => segments;

            // Private ������ - RegisterTag�� ���ؼ��� ����
            private SimpleGameplayTag(string name)
            {
                this.fullName = name;
                this.segments = name.Split('.');
            }

            // �±� ��� (�̱��� ����)
            public static SimpleGameplayTag RegisterTag(string tagName)
            {
                if (string.IsNullOrEmpty(tagName))
                {
                    Debug.LogError("�±� �̸��� ����ֽ��ϴ�!");
                    return null;
                }

                // �̹� ��ϵ� �±״� ����
                if (registeredTags.TryGetValue(tagName, out var existingTag))
                {
                    return existingTag;
                }

                // �� �±� ���� �� ���
                var newTag = new SimpleGameplayTag(tagName);
                registeredTags[tagName] = newTag;
                Debug.Log($"[Tag] �� �±� ���: {tagName}");
                return newTag;
            }

            // ��Ȯ�� ��ġ�ϴ��� üũ
            public bool Equals(SimpleGameplayTag other)
            {
                if (other == null) return false;
                return fullName == other.fullName;
            }

            // ������ ��Ī (�ڽ��� �θ�� ��ġ)
            public bool Matches(SimpleGameplayTag parent)
            {
                if (parent == null) return false;

                // �ڽ��� parent�� �ڽ����� üũ
                if (parent.segments.Length > segments.Length)
                    return false;

                for (int i = 0; i < parent.segments.Length; i++)
                {
                    if (segments[i] != parent.segments[i])
                        return false;
                }

                return true;
            }

            // ���� ��Ī (���ϵ�ī�� ����)
            public bool MatchesPattern(string pattern)
            {
                if (pattern.EndsWith("*"))
                {
                    string prefix = pattern.Substring(0, pattern.Length - 1);
                    return fullName.StartsWith(prefix);
                }
                return fullName == pattern;
            }

            // �θ� �±� ��������
            public SimpleGameplayTag GetParent()
            {
                if (segments.Length <= 1) return null;

                string parentName = string.Join(".", segments, 0, segments.Length - 1);
                return RegisterTag(parentName);
            }

            // ���� ��������
            public int GetDepth()
            {
                return segments.Length;
            }

            public override string ToString()
            {
                return fullName;
            }

            // ����׿�: ��� ��ϵ� �±� ���
            public static void PrintAllRegisteredTags()
            {
                Debug.Log($"=== ��ϵ� �±� ({registeredTags.Count}��) ===");
                foreach (var tag in registeredTags.Values)
                {
                    Debug.Log($"  - {tag.fullName} (����: {tag.GetDepth()})");
                }
            }
        }

        [Header("�׽�Ʈ�� �±׵�")]
        private SimpleGameplayTag stateTag;
        private SimpleGameplayTag idleTag;
        private SimpleGameplayTag combatTag;
        private SimpleGameplayTag attackingTag;
        private SimpleGameplayTag defendingTag;
        private SimpleGameplayTag movementTag;
        private SimpleGameplayTag runningTag;

        [Header("���� Ȱ�� �±�")]
        [SerializeField] private List<string> activeTags = new List<string>();

        private void Start()
        {
            Debug.Log("=== Phase 2 - Step 1: GameplayTag ���� �׽�Ʈ ===");
            InitializeTags();
            RunBasicTests();
        }

        private void InitializeTags()
        {
            Debug.Log("\n[�׽�Ʈ 1] �±� ��� �� ���� ����");

            // ������ �±� ���
            stateTag = SimpleGameplayTag.RegisterTag("State");
            idleTag = SimpleGameplayTag.RegisterTag("State.Idle");
            combatTag = SimpleGameplayTag.RegisterTag("State.Combat");
            attackingTag = SimpleGameplayTag.RegisterTag("State.Combat.Attacking");
            defendingTag = SimpleGameplayTag.RegisterTag("State.Combat.Defending");
            movementTag = SimpleGameplayTag.RegisterTag("State.Movement");
            runningTag = SimpleGameplayTag.RegisterTag("State.Movement.Running");

            // �ɷ� �±�
            SimpleGameplayTag.RegisterTag("Ability.Skill.Fireball");
            SimpleGameplayTag.RegisterTag("Ability.Skill.Heal");
            SimpleGameplayTag.RegisterTag("Ability.Ultimate.Meteor");

            // ���� �±�
            SimpleGameplayTag.RegisterTag("Status.Buff.Speed");
            SimpleGameplayTag.RegisterTag("Status.Buff.Strength");
            SimpleGameplayTag.RegisterTag("Status.Debuff.Slow");
            SimpleGameplayTag.RegisterTag("Status.Debuff.Poison");

            SimpleGameplayTag.PrintAllRegisteredTags();
        }

        private void RunBasicTests()
        {
            TestExactMatching();
            TestHierarchicalMatching();
            TestPatternMatching();
            TestParentRelationship();
            TestTagReuse();
        }

        // �׽�Ʈ 1: ��Ȯ�� ��Ī
        private void TestExactMatching()
        {
            Debug.Log("\n[�׽�Ʈ 2] ��Ȯ�� ��Ī �׽�Ʈ");

            var combat1 = SimpleGameplayTag.RegisterTag("State.Combat");
            var combat2 = SimpleGameplayTag.RegisterTag("State.Combat");

            Debug.Log($"combat1 == combat2: {combat1.Equals(combat2)} (���� �ν��Ͻ�: {ReferenceEquals(combat1, combat2)})");
            Debug.Log($"combat != idle: {!combatTag.Equals(idleTag)}");
        }

        // �׽�Ʈ 2: ������ ��Ī
        private void TestHierarchicalMatching()
        {
            Debug.Log("\n[�׽�Ʈ 3] ������ ��Ī �׽�Ʈ");

            // �ڽ��� �θ�� ��ġ�ϴ���
            Debug.Log($"State.Combat.Attacking matches State: {attackingTag.Matches(stateTag)}");
            Debug.Log($"State.Combat.Attacking matches State.Combat: {attackingTag.Matches(combatTag)}");
            Debug.Log($"State.Combat matches State.Combat.Attacking: {combatTag.Matches(attackingTag)} (�θ�� �ڽİ� ��ġ �ȵ�)");

            // �ٸ� �귣ġ������ ��ġ �ȵ�
            Debug.Log($"State.Combat matches State.Movement: {combatTag.Matches(movementTag)}");
        }

        // �׽�Ʈ 3: ���� ��Ī
        private void TestPatternMatching()
        {
            Debug.Log("\n[�׽�Ʈ 4] ���� ��Ī �׽�Ʈ");

            var fireballTag = SimpleGameplayTag.RegisterTag("Ability.Skill.Fireball");

            Debug.Log($"Fireball matches 'Ability*': {fireballTag.MatchesPattern("Ability*")}");
            Debug.Log($"Fireball matches 'Ability.Skill*': {fireballTag.MatchesPattern("Ability.Skill*")}");
            Debug.Log($"Fireball matches 'Status*': {fireballTag.MatchesPattern("Status*")}");
            Debug.Log($"Fireball matches exact: {fireballTag.MatchesPattern("Ability.Skill.Fireball")}");
        }

        // �׽�Ʈ 4: �θ� ����
        private void TestParentRelationship()
        {
            Debug.Log("\n[�׽�Ʈ 5] �θ� �±� ���� �׽�Ʈ");

            var parent = attackingTag.GetParent();
            Debug.Log($"State.Combat.Attacking�� �θ�: {parent}");

            var grandParent = parent?.GetParent();
            Debug.Log($"State.Combat�� �θ�: {grandParent}");

            var root = grandParent?.GetParent();
            Debug.Log($"State�� �θ�: {root} (null = ��Ʈ)");

            Debug.Log($"\n���� �׽�Ʈ:");
            Debug.Log($"State ����: {stateTag.GetDepth()}");
            Debug.Log($"State.Combat ����: {combatTag.GetDepth()}");
            Debug.Log($"State.Combat.Attacking ����: {attackingTag.GetDepth()}");
        }

        // �׽�Ʈ 5: �±� ����
        private void TestTagReuse()
        {
            Debug.Log("\n[�׽�Ʈ 6] �±� ���� �׽�Ʈ");

            int beforeCount = GetRegisteredTagCount();

            // ���� �±׸� ���� �� ��� �õ�
            var tag1 = SimpleGameplayTag.RegisterTag("Test.Reuse");
            var tag2 = SimpleGameplayTag.RegisterTag("Test.Reuse");
            var tag3 = SimpleGameplayTag.RegisterTag("Test.Reuse");

            int afterCount = GetRegisteredTagCount();

            Debug.Log($"��� �� �±� ��: {beforeCount}");
            Debug.Log($"��� �� �±� ��: {afterCount}");
            Debug.Log($"������ �±� ��: {afterCount - beforeCount} (1���� �����ؾ� ��)");
            Debug.Log($"��� ���� �ν��Ͻ�?: {ReferenceEquals(tag1, tag2) && ReferenceEquals(tag2, tag3)}");
        }

        private int GetRegisteredTagCount()
        {
            // Reflection�� ����Ͽ� private static �ʵ� ����
            var field = typeof(SimpleGameplayTag).GetField("registeredTags",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var dict = field.GetValue(null) as Dictionary<string, SimpleGameplayTag>;
            return dict?.Count ?? 0;
        }

        // ���� ���� �ùķ��̼�
        private void SimulateGameplay()
        {
            Debug.Log("\n=== �����÷��� �ùķ��̼� ===");

            // ���� ����: Idle
            activeTags.Clear();
            activeTags.Add(idleTag.FullName);
            Debug.Log($"���� ����: {string.Join(", ", activeTags)}");

            // ���� ����
            activeTags.Clear();
            activeTags.Add(combatTag.FullName);
            Debug.Log($"���� ����: {string.Join(", ", activeTags)}");

            // ���� ��
            activeTags.Add(attackingTag.FullName);
            Debug.Log($"���� ����: {string.Join(", ", activeTags)}");

            // ��ų ��� ���� üũ
            bool canUseSkill = CheckTagCondition("State.Combat");
            Debug.Log($"��ų ��� ����? (State.Combat �ʿ�): {canUseSkill}");

            // �̵� ���� üũ
            bool canMove = !CheckTagCondition("State.Combat.Attacking");
            Debug.Log($"�̵� ����? (!State.Combat.Attacking): {canMove}");
        }

        private bool CheckTagCondition(string tagName)
        {
            var requiredTag = SimpleGameplayTag.RegisterTag(tagName);

            foreach (var activeTagName in activeTags)
            {
                var activeTag = SimpleGameplayTag.RegisterTag(activeTagName);
                if (activeTag.Matches(requiredTag))
                {
                    return true;
                }
            }
            return false;
        }

        private void Update()
        {
            // Ű���� �Է����� ���� ��ȯ �׽�Ʈ
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetState("State.Idle");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetState("State.Combat");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetState("State.Combat.Attacking");
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetState("State.Movement.Running");
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SimulateGameplay();
            }
        }

        private void SetState(string tagName)
        {
            activeTags.Clear();
            activeTags.Add(tagName);
            Debug.Log($"���� ����: {tagName}");
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 300, 180), "Phase 2 - GameplayTag ����");

            int y = 40;
            GUI.Label(new Rect(20, y, 280, 20), "=== �±� ���� ���� ===");
            y += 25;
            GUI.Label(new Rect(20, y, 280, 20), "State");
            y += 20;
            GUI.Label(new Rect(30, y, 270, 20), "���� State.Idle");
            y += 20;
            GUI.Label(new Rect(30, y, 270, 20), "���� State.Combat");
            y += 20;
            GUI.Label(new Rect(40, y, 260, 20), "��  ���� .Attacking");
            y += 20;
            GUI.Label(new Rect(40, y, 260, 20), "��  ���� .Defending");
            y += 20;
            GUI.Label(new Rect(30, y, 270, 20), "���� State.Movement");
            y += 30;

            GUI.Label(new Rect(20, y, 280, 20), "1-4: ���� ����  Space: �ùķ��̼�");

            // ���� Ȱ�� �±�
            y = 200;
            GUI.Box(new Rect(10, y, 300, 60), "���� Ȱ�� �±�");
            y += 25;
            string activeTagsStr = activeTags.Count > 0 ? string.Join(", ", activeTags) : "����";
            GUI.Label(new Rect(20, y, 280, 30), activeTagsStr);
        }
    }
}