using GAS.AttributeSystem;
using GAS.Core;
using GAS.TagSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.Tests
{
    /// <summary>
    /// Tag System�� Attribute System�� ���� �׽�Ʈ
    /// Unity Test Framework ��Ű���� �ʿ��մϴ�
    /// </summary>
    public class GASPhase1Tests : MonoBehaviour
    {
        #region Manual Test Methods

        /// <summary>
        /// ���� �׽�Ʈ�� ���� �޼����
        /// Play Mode���� �����ϰų� Editor ��ũ��Ʈ�� ȣ��
        /// </summary>

        [ContextMenu("Test All")]
        public void RunAllTests()
        {
            Debug.Log("=== Starting GAS Phase 1 Tests ===");

            TestTagSystem();
            TestAttributeSystem();
            TestIntegration();

            Debug.Log("=== All Tests Complete ===");
        }

        [ContextMenu("Test Tag System")]
        public void TestTagSystem()
        {
            Debug.Log("--- Tag System Tests ---");

            // GameplayTag ���� �׽�Ʈ
            TestGameplayTagCreation();

            // TagContainer �׽�Ʈ
            TestTagContainer();

            // TagRequirement �׽�Ʈ  
            TestTagRequirement();

            Debug.Log("Tag System Tests Complete!");
        }

        [ContextMenu("Test Attribute System")]
        public void TestAttributeSystem()
        {
            Debug.Log("--- Attribute System Tests ---");

            // BaseAttribute �׽�Ʈ
            TestBaseAttribute();

            // AttributeModifier �׽�Ʈ
            TestAttributeModifier();

            // AttributeSet �׽�Ʈ
            TestAttributeSet();

            Debug.Log("Attribute System Tests Complete!");
        }

        [ContextMenu("Test Integration")]
        public void TestIntegration()
        {
            Debug.Log("--- Integration Tests ---");

            // AttributeSetComponent �׽�Ʈ
            TestAttributeSetComponent();

            // TagComponent �׽�Ʈ
            TestTagComponent();

            Debug.Log("Integration Tests Complete!");
        }

        #endregion

        #region Tag System Test Methods

        void TestGameplayTagCreation()
        {
            Debug.Log("Testing GameplayTag Creation...");

            // �±� ����
            var tag1 = new GameplayTag("Status.Buff.Speed");
            var tag2 = new GameplayTag("Status.Buff");
            var tag3 = new GameplayTag("Ability.Fire");

            // �⺻ �Ӽ� Ȯ��
            AssertEquals(tag1.TagString, "Status.Buff.Speed", "Tag1 name check");
            AssertEquals(tag2.TagString, "Status.Buff", "Tag2 name check");

            // IsChildOf �׽�Ʈ (�޼��尡 �ִٸ�)
            // ���� ������ ���� ���� �ʿ�
            Debug.Log($"Tag1: {tag1.TagString}");
            Debug.Log($"Tag2: {tag2.TagString}");
            Debug.Log($"Tag3: {tag3.TagString}");

            Debug.Log(" GameplayTag Creation Test Passed");
        }

        void TestTagContainer()
        {
            Debug.Log("Testing TagContainer...");

            var container = new TagContainer();
            var tag1 = new GameplayTag("Status.Buff.Speed");
            var tag2 = new GameplayTag("Status.Debuff.Slow");

            // Add �׽�Ʈ
            container.AddTag(tag1);
            container.AddTag(tag2);

            AssertTrue(container.HasTag(tag1), "Container has tag1");
            AssertTrue(container.HasTag(tag2), "Container has tag2");
            AssertEquals(container.Count, 2, "Container count");

            // Remove �׽�Ʈ
            container.RemoveTag(tag1);
            AssertFalse(container.HasTag(tag1), "Tag1 removed");
            AssertTrue(container.HasTag(tag2), "Tag2 still exists");
            AssertEquals(container.Count, 1, "Container count after remove");

            // HasAny �׽�Ʈ
            container.Clear();
            container.AddTag(new GameplayTag("Status.Buff.Speed"));
            container.AddTag(new GameplayTag("Status.Buff.Attack"));

            var queryTags = new List<GameplayTag>
            {
                new GameplayTag("Status.Buff.Speed"),
                new GameplayTag("Status.Debuff.Slow")
            };

            AssertTrue(container.HasAny(queryTags), "HasAny test");

            // HasAll �׽�Ʈ
            var queryTags2 = new List<GameplayTag>
            {
                new GameplayTag("Status.Buff.Speed"),
                new GameplayTag("Status.Buff.Attack")
            };

            AssertTrue(container.HasAll(queryTags2), "HasAll test - should pass");

            queryTags2.Add(new GameplayTag("Status.Debuff.Slow"));
            AssertFalse(container.HasAll(queryTags2), "HasAll test - should fail");

            Debug.Log(" TagContainer Test Passed");
        }

        void TestTagRequirement()
        {
            Debug.Log("Testing TagRequirement...");

            var container = new TagContainer();
            container.AddTag(new GameplayTag("Status.Buff.Speed"));
            container.AddTag(new GameplayTag("Character.Alive"));

            var requirement = new TagRequirement();

            // RequiredTags �׽�Ʈ
            requirement.AddRequiredTag(new GameplayTag("Character.Alive"));
            AssertTrue(requirement.IsSatisfiedBy(container), "Required tags satisfied");

            // IgnoreTags �׽�Ʈ
            requirement.AddBlockedTag(new GameplayTag("Status.Debuff.Stun"));
            AssertTrue(requirement.IsSatisfiedBy(container), "Ignore tags not present");

            container.AddTag(new GameplayTag("Status.Debuff.Stun"));
            AssertFalse(requirement.IsSatisfiedBy(container), "Ignore tags present");

            Debug.Log(" TagRequirement Test Passed");
        }

        #endregion

        #region Attribute System Test Methods

        void TestBaseAttribute()
        {
            Debug.Log("Testing BaseAttribute...");

            // ������ �׽�Ʈ (���� �����ڿ� ���� ����)
            var attribute = new BaseAttribute();
            attribute.AttributeType = AttributeType.Health;
            attribute.BaseValue = 100f;
            attribute.SetMinValue(0f);
            attribute.SetMaxValue(150f);

            AssertEquals(attribute.AttributeType, AttributeType.Health, "Attribute type");
            AssertEquals(attribute.BaseValue, 100f, "Base value");
            AssertTrue(attribute.HasMinValue, "Has min value");
            AssertTrue(attribute.HasMaxValue, "Has max value");
            AssertEquals(attribute.MinValue, 0f, "Min value");
            AssertEquals(attribute.MaxValue, 150f, "Max value");

            Debug.Log(" BaseAttribute Test Passed");
        }

        void TestAttributeModifier()
        {
            Debug.Log("Testing AttributeModifier...");

            // ���� �����ڿ� ���� ����
            var modifier1 = new AttributeModifier();
            modifier1.Operation = ModifierOperation.Add;
            modifier1.Value = 10f;
            modifier1.Priority = ModifierPriority.Normal;

            var modifier2 = new AttributeModifier();
            modifier2.Operation = ModifierOperation.Multiply;
            modifier2.Value = 1.5f;
            modifier2.Priority = ModifierPriority.High;

            AssertEquals(modifier1.Operation, ModifierOperation.Add, "Modifier1 operation");
            AssertEquals(modifier1.Value, 10f, "Modifier1 value");
            AssertEquals(modifier2.Operation, ModifierOperation.Multiply, "Modifier2 operation");

            // Priority ���� �׽�Ʈ
            var modifiers = new List<AttributeModifier> { modifier2, modifier1 };
            modifiers.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            AssertEquals(modifiers[0], modifier1, "Priority sort");

            Debug.Log(" AttributeModifier Test Passed");
        }

        void TestAttributeSet()
        {
            Debug.Log("Testing AttributeSet...");

            var set = new AttributeSet();

            var healthAttr = new BaseAttribute();
            healthAttr.AttributeType = AttributeType.Health;
            healthAttr.BaseValue = 100f;

            var manaAttr = new BaseAttribute();
            manaAttr.AttributeType = AttributeType.Mana;
            manaAttr.BaseValue = 50f;

            // Add �׽�Ʈ
            set.AddAttribute(healthAttr);
            set.AddAttribute(manaAttr);

            AssertTrue(set.HasAttribute(AttributeType.Health), "Has health attribute");
            AssertTrue(set.HasAttribute(AttributeType.Mana), "Has mana attribute");

            var retrievedHealth = set.GetAttribute(AttributeType.Health);
            AssertNotNull(retrievedHealth, "Retrieved health attribute");
            AssertEquals(retrievedHealth.BaseValue, 100f, "Health base value");

            Debug.Log(" AttributeSet Test Passed");
        }

        #endregion

        #region Integration Test Methods

        void TestAttributeSetComponent()
        {
            Debug.Log("Testing AttributeSetComponent...");

            // GameObject ����
            var testObject = new GameObject("TestAttributeObject");
            var component = testObject.AddComponent<AttributeSetComponent>();

            // AttributeSetData ���� �� �ʱ�ȭ
            var data = ScriptableObject.CreateInstance<AttributeSetData>();
            data.SetAttributeData(AttributeType.Health, 100f, 0f, 200f);

            component.InitializeFromData(data);

            // Modifier �߰� �׽�Ʈ
            var modifier1 = new AttributeModifier();
            modifier1.Operation = ModifierOperation.Add;
            modifier1.Value = 20f;

            var modifier2 = new AttributeModifier();
            modifier2.Operation = ModifierOperation.Multiply;
            modifier2.Value = 1.5f;

            component.AddModifier(AttributeType.Health, modifier1);
            component.AddModifier(AttributeType.Health, modifier2);

            // ��� Ȯ��: Base 100 + Add 20 = 120, Multiply 1.5 = 180
            var finalValue = component.GetAttributeValue(AttributeType.Health);
            AssertApproximate(finalValue, 180f, 0.01f, "Modified health value");

            // Override �׽�Ʈ
            var overrideModifier = new AttributeModifier();
            overrideModifier.Operation = ModifierOperation.Override;
            overrideModifier.Value = 75f;

            component.AddModifier(AttributeType.Health, overrideModifier);
            finalValue = component.GetAttributeValue(AttributeType.Health);
            AssertApproximate(finalValue, 75f, 0.01f, "Override value");

            // ����
            GameObject.DestroyImmediate(testObject);

            Debug.Log(" AttributeSetComponent Test Passed");
        }

        void TestTagComponent()
        {
            Debug.Log("Testing TagComponent...");

            var testObject = new GameObject("TestTagObject");
            var tagComponent = testObject.AddComponent<TagComponent>();

            // �±� �߰�
            tagComponent.AddTag("Status.Buff.Speed");
            tagComponent.AddTag("Character.Alive");

            AssertTrue(tagComponent.HasTag("Status.Buff.Speed"), "Has speed buff tag");
            AssertTrue(tagComponent.HasTag("Character.Alive"), "Has alive tag");
            AssertTrue(tagComponent.HasTag("Status.Buff"), "Has matching parent tag");

            // Requirement �׽�Ʈ
            var requirement = new TagRequirement();
            requirement.AddRequiredTag(new GameplayTag("Character.Alive"));

            AssertTrue(tagComponent.SatisfiesRequirement(requirement), "Satisfies requirement");

            // �±� ����
            tagComponent.RemoveTag("Character.Alive");
            AssertFalse(tagComponent.SatisfiesRequirement(requirement), "No longer satisfies requirement");

            // ����
            GameObject.DestroyImmediate(testObject);

            Debug.Log(" TagComponent Test Passed");
        }

        #endregion

        #region Performance Test Methods

        [ContextMenu("Test Performance")]
        public void TestPerformance()
        {
            Debug.Log("--- Performance Tests ---");

            TestAttributeModifierPerformance();
            TestTagContainerPerformance();

            Debug.Log("Performance Tests Complete!");
        }

        void TestAttributeModifierPerformance()
        {
            Debug.Log("Testing AttributeModifier Performance...");

            var testObject = new GameObject("PerfTestObject");
            var component = testObject.AddComponent<AttributeSetComponent>();

            var data = ScriptableObject.CreateInstance<AttributeSetData>();
            data.SetAttributeData(AttributeType.Health, 100f);
            component.InitializeFromData(data);

            // ���� Modifier �߰�
            var startTime = Time.realtimeSinceStartup;

            for (int i = 0; i < 1000; i++)
            {
                var modifier = new AttributeModifier();
                modifier.Operation = ModifierOperation.Add;
                modifier.Value = 0.1f;
                component.AddModifier(AttributeType.Health, modifier);
            }

            var addTime = Time.realtimeSinceStartup - startTime;
            Debug.Log($"Added 1000 modifiers in {addTime * 1000:F2}ms");

            // ��� ���� ����
            startTime = Time.realtimeSinceStartup;
            var value = component.GetAttributeValue(AttributeType.Health);
            var calcTime = Time.realtimeSinceStartup - startTime;

            Debug.Log($"Calculated value in {calcTime * 1000:F2}ms");
            Debug.Log($"Final value: {value:F2} (expected ~200)");

            AssertTrue(calcTime < 0.01f, "Calculation should be fast");

            GameObject.DestroyImmediate(testObject);

            Debug.Log(" AttributeModifier Performance Test Passed");
        }

        void TestTagContainerPerformance()
        {
            Debug.Log("Testing TagContainer Performance...");

            var container = new TagContainer();

            // ���� �±� �߰�
            var startTime = Time.realtimeSinceStartup;

            for (int i = 0; i < 1000; i++)
            {
                container.AddTag(new GameplayTag($"Test.Tag.Number{i}"));
            }

            var addTime = Time.realtimeSinceStartup - startTime;
            Debug.Log($"Added 1000 tags in {addTime * 1000:F2}ms");

            // �˻� ���� ����
            startTime = Time.realtimeSinceStartup;
            bool hasTag = container.HasTag(new GameplayTag("Test.Tag.Number500"));
            var searchTime = Time.realtimeSinceStartup - startTime;

            Debug.Log($"Tag lookup in {searchTime * 1000:F2}ms");

            AssertTrue(hasTag, "Found tag");
            AssertEquals(container.Count, 1000, "Container count");
            AssertTrue(searchTime < 0.001f, "Lookup should be very fast");

            Debug.Log(" TagContainer Performance Test Passed");
        }

        #endregion

        #region Helper Methods

        void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                Debug.LogError($" FAILED: {message}");
            }
            else
            {
                Debug.Log($" {message}");
            }
        }

        void AssertFalse(bool condition, string message)
        {
            AssertTrue(!condition, message);
        }

        void AssertEquals<T>(T actual, T expected, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(actual, expected))
            {
                Debug.LogError($" FAILED: {message} (Expected: {expected}, Actual: {actual})");
            }
            else
            {
                Debug.Log($"  {message}");
            }
        }

        void AssertApproximate(float actual, float expected, float tolerance, string message)
        {
            if (Mathf.Abs(actual - expected) > tolerance)
            {
                Debug.LogError($" FAILED: {message} (Expected: {expected:F2}, Actual: {actual:F2})");
            }
            else
            {
                Debug.Log($"  {message}");
            }
        }

        void AssertNotNull(object obj, string message)
        {
            if (obj == null)
            {
                Debug.LogError($" FAILED: {message} (Object is null)");
            }
            else
            {
                Debug.Log($"  {message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Editor���� �׽�Ʈ�� �����ϱ� ���� ���� Ŭ����
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(GASPhase1Tests))]
    public class GASPhase1TestsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var tests = target as GASPhase1Tests;

            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.LabelField("Test Runner", UnityEditor.EditorStyles.boldLabel);

            if (GUILayout.Button("Run All Tests", GUILayout.Height(30)))
            {
                tests.RunAllTests();
            }

            UnityEditor.EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Tag System"))
            {
                tests.TestTagSystem();
            }
            if (GUILayout.Button("Test Attribute System"))
            {
                tests.TestAttributeSystem();
            }
            UnityEditor.EditorGUILayout.EndHorizontal();

            UnityEditor.EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Integration"))
            {
                tests.TestIntegration();
            }
            if (GUILayout.Button("Test Performance"))
            {
                tests.TestPerformance();
            }
            UnityEditor.EditorGUILayout.EndHorizontal();
        }
    }
#endif
}