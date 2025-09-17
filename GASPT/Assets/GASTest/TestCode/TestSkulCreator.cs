// ===================================
// ����: Assets/Scripts/Ability/Test/TestSkulCreator.cs
// ===================================
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AbilitySystem.Platformer.Test
{
    /// <summary>
    /// �׽�Ʈ�� ���� ������ ������
    /// </summary>
    public class TestSkulCreator : MonoBehaviour
    {
        [Header("�׽�Ʈ ���� ����")]
        [SerializeField] private bool createOnStart = true;

        private void Start()
        {
            if (createOnStart)
            {
                CreateTestSkuls();
            }
        }

        [ContextMenu("Create Test Skuls")]
        public void CreateTestSkuls()
        {
#if UNITY_EDITOR
            // ���� ����
            if (!AssetDatabase.IsValidFolder("Assets/TestData"))
            {
                AssetDatabase.CreateFolder("Assets", "TestData");
            }
            if (!AssetDatabase.IsValidFolder("Assets/TestData/Skuls"))
            {
                AssetDatabase.CreateFolder("Assets/TestData", "Skuls");
            }
            if (!AssetDatabase.IsValidFolder("Assets/TestData/Abilities"))
            {
                AssetDatabase.CreateFolder("Assets/TestData", "Abilities");
            }

            // ���� ���� ����
            CreateWarriorSkul();

            // ������ ���� ����
            CreateMageSkul();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("�׽�Ʈ ���� ���� �Ϸ�!");
#else
            Debug.LogWarning("�����Ϳ����� ��� �����մϴ�.");
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// ���� ���� ����
        /// </summary>
        private void CreateWarriorSkul()
        {
            // ���� ������
            SkulData warrior = ScriptableObject.CreateInstance<SkulData>();
            warrior.skulId = "warrior_test";
            warrior.skulName = "�׽�Ʈ ����";
            warrior.description = "���� ���� �׽�Ʈ�� ����";
            warrior.skulType = SkulType.Power;
            warrior.attackPower = 15f;
            warrior.attackSpeed = 1.2f;
            warrior.moveSpeed = 5f;
            warrior.jumpPower = 10f;
            warrior.maxComboCount = 3;
            warrior.comboResetTime = 1f;
            warrior.comboDamageMultipliers = new float[] { 1f, 1.3f, 1.6f };

            // �⺻ ����
            PlatformerAbilityData basicAttack = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            basicAttack.abilityId = "warrior_basic";
            basicAttack.abilityName = "�⺻ ����";
            basicAttack.abilityType = PlatformerAbilityType.BasicAttack;
            basicAttack.damageMultiplier = 1f;
            basicAttack.range = 2f;
            basicAttack.hitboxSize = new Vector2(2f, 1.5f);
            basicAttack.hitboxOffset = new Vector2(1f, 0);
            basicAttack.hitboxDuration = 0.2f;
            basicAttack.cooldownTime = 0f;
            warrior.basicAttack = basicAttack;

            // ��ų 1: ��Ÿ
            PlatformerAbilityData skill1 = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            skill1.abilityId = "warrior_skill1";
            skill1.abilityName = "��Ÿ";
            skill1.abilityType = PlatformerAbilityType.Skill;
            skill1.damageMultiplier = 2f;
            skill1.range = 3f;
            skill1.hitboxSize = new Vector2(3f, 2f);
            skill1.hitboxOffset = new Vector2(1.5f, 0);
            skill1.hitboxDuration = 0.3f;
            skill1.cooldownTime = 3f;
            skill1.knockbackPower = 5f;
            warrior.skill1 = skill1;

            // ��ų 2: ȸ�� ����
            PlatformerAbilityData skill2 = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            skill2.abilityId = "warrior_skill2";
            skill2.abilityName = "ȸ�� ����";
            skill2.abilityType = PlatformerAbilityType.Skill;
            skill2.damageMultiplier = 1.5f;
            skill2.range = 2.5f;
            skill2.hitboxSize = new Vector2(5f, 2f);
            skill2.hitboxOffset = new Vector2(0, 0);
            skill2.hitboxDuration = 0.5f;
            skill2.cooldownTime = 5f;
            skill2.isMultiHit = true;
            skill2.hitCount = 3;
            skill2.canMoveWhileUsing = false;
            warrior.skill2 = skill2;

            // ���
            PlatformerAbilityData dash = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            dash.abilityId = "warrior_dash";
            dash.abilityName = "���";
            dash.abilityType = PlatformerAbilityType.Movement;
            dash.range = 5f;
            dash.cooldownTime = 2f;
            warrior.dashAbility = dash;

            // ����
            AssetDatabase.CreateAsset(warrior, "Assets/TestData/Skuls/TestWarrior.asset");
            AssetDatabase.CreateAsset(basicAttack, "Assets/TestData/Abilities/WarriorBasicAttack.asset");
            AssetDatabase.CreateAsset(skill1, "Assets/TestData/Abilities/WarriorSkill1.asset");
            AssetDatabase.CreateAsset(skill2, "Assets/TestData/Abilities/WarriorSkill2.asset");
            AssetDatabase.CreateAsset(dash, "Assets/TestData/Abilities/WarriorDash.asset");
        }

        /// <summary>
        /// ������ ���� ����
        /// </summary>
        private void CreateMageSkul()
        {
            // ���� ������
            SkulData mage = ScriptableObject.CreateInstance<SkulData>();
            mage.skulId = "mage_test";
            mage.skulName = "�׽�Ʈ ������";
            mage.description = "���Ÿ� ���� �׽�Ʈ�� ������";
            mage.skulType = SkulType.Range;
            mage.attackPower = 10f;
            mage.attackSpeed = 1f;
            mage.moveSpeed = 4f;
            mage.jumpPower = 9f;
            mage.maxComboCount = 2;
            mage.comboResetTime = 1.5f;
            mage.comboDamageMultipliers = new float[] { 1f, 1.5f };

            // �⺻ ����: ���̾
            PlatformerAbilityData basicAttack = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            basicAttack.abilityId = "mage_basic";
            basicAttack.abilityName = "���̾";
            basicAttack.abilityType = PlatformerAbilityType.BasicAttack;
            basicAttack.damageMultiplier = 1f;
            basicAttack.range = 5f;
            basicAttack.hitboxSize = new Vector2(1f, 1f);
            basicAttack.hitboxOffset = new Vector2(2f, 0);
            basicAttack.hitboxDuration = 0.1f;
            basicAttack.cooldownTime = 0f;
            mage.basicAttack = basicAttack;

            // ��ų 1: ��¡ ������
            PlatformerAbilityData skill1 = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            skill1.abilityId = "mage_skill1";
            skill1.abilityName = "��¡ ������";
            skill1.abilityType = PlatformerAbilityType.Skill;
            skill1.damageMultiplier = 3f;
            skill1.range = 8f;
            skill1.hitboxSize = new Vector2(8f, 0.5f);
            skill1.hitboxOffset = new Vector2(4f, 0);
            skill1.hitboxDuration = 0.5f;
            skill1.cooldownTime = 4f;
            skill1.isChargeSkill = true;
            skill1.maxChargeTime = 2f;
            skill1.canMoveWhileUsing = false;
            mage.skill1 = skill1;

            // ��ų 2: ���׿�
            PlatformerAbilityData skill2 = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            skill2.abilityId = "mage_skill2";
            skill2.abilityName = "���׿�";
            skill2.abilityType = PlatformerAbilityType.Skill;
            skill2.damageMultiplier = 2.5f;
            skill2.range = 6f;
            skill2.hitboxSize = new Vector2(3f, 3f);
            skill2.hitboxOffset = new Vector2(3f, 0);
            skill2.hitboxDuration = 0.3f;
            skill2.cooldownTime = 6f;
            skill2.knockbackPower = 8f;
            mage.skill2 = skill2;

            // ���: �ڷ���Ʈ
            PlatformerAbilityData dash = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            dash.abilityId = "mage_dash";
            dash.abilityName = "�ڷ���Ʈ";
            dash.abilityType = PlatformerAbilityType.Movement;
            dash.range = 4f;
            dash.cooldownTime = 3f;
            mage.dashAbility = dash;

            // ����
            AssetDatabase.CreateAsset(mage, "Assets/TestData/Skuls/TestMage.asset");
            AssetDatabase.CreateAsset(basicAttack, "Assets/TestData/Abilities/MageBasicAttack.asset");
            AssetDatabase.CreateAsset(skill1, "Assets/TestData/Abilities/MageSkill1.asset");
            AssetDatabase.CreateAsset(skill2, "Assets/TestData/Abilities/MageSkill2.asset");
            AssetDatabase.CreateAsset(dash, "Assets/TestData/Abilities/MageDash.asset");
        }
#endif
    }
}