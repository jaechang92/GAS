// ===================================
// 파일: Assets/Scripts/Ability/Test/TestSkulCreator.cs
// ===================================
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AbilitySystem.Platformer.Test
{
    /// <summary>
    /// 테스트용 스컬 데이터 생성기
    /// </summary>
    public class TestSkulCreator : MonoBehaviour
    {
        [Header("테스트 스컬 생성")]
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
            // 폴더 생성
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

            // 전사 스컬 생성
            CreateWarriorSkul();

            // 마법사 스컬 생성
            CreateMageSkul();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("테스트 스컬 생성 완료!");
#else
            Debug.LogWarning("에디터에서만 사용 가능합니다.");
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 전사 스컬 생성
        /// </summary>
        private void CreateWarriorSkul()
        {
            // 스컬 데이터
            SkulData warrior = ScriptableObject.CreateInstance<SkulData>();
            warrior.skulId = "warrior_test";
            warrior.skulName = "테스트 전사";
            warrior.description = "근접 공격 테스트용 전사";
            warrior.skulType = SkulType.Power;
            warrior.attackPower = 15f;
            warrior.attackSpeed = 1.2f;
            warrior.moveSpeed = 5f;
            warrior.jumpPower = 10f;
            warrior.maxComboCount = 3;
            warrior.comboResetTime = 1f;
            warrior.comboDamageMultipliers = new float[] { 1f, 1.3f, 1.6f };

            // 기본 공격
            PlatformerAbilityData basicAttack = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            basicAttack.abilityId = "warrior_basic";
            basicAttack.abilityName = "기본 공격";
            basicAttack.abilityType = PlatformerAbilityType.BasicAttack;
            basicAttack.damageMultiplier = 1f;
            basicAttack.range = 2f;
            basicAttack.hitboxSize = new Vector2(2f, 1.5f);
            basicAttack.hitboxOffset = new Vector2(1f, 0);
            basicAttack.hitboxDuration = 0.2f;
            basicAttack.cooldownTime = 0f;
            warrior.basicAttack = basicAttack;

            // 스킬 1: 강타
            PlatformerAbilityData skill1 = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            skill1.abilityId = "warrior_skill1";
            skill1.abilityName = "강타";
            skill1.abilityType = PlatformerAbilityType.Skill;
            skill1.damageMultiplier = 2f;
            skill1.range = 3f;
            skill1.hitboxSize = new Vector2(3f, 2f);
            skill1.hitboxOffset = new Vector2(1.5f, 0);
            skill1.hitboxDuration = 0.3f;
            skill1.cooldownTime = 3f;
            skill1.knockbackPower = 5f;
            warrior.skill1 = skill1;

            // 스킬 2: 회전 베기
            PlatformerAbilityData skill2 = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            skill2.abilityId = "warrior_skill2";
            skill2.abilityName = "회전 베기";
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

            // 대시
            PlatformerAbilityData dash = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            dash.abilityId = "warrior_dash";
            dash.abilityName = "대시";
            dash.abilityType = PlatformerAbilityType.Movement;
            dash.range = 5f;
            dash.cooldownTime = 2f;
            warrior.dashAbility = dash;

            // 저장
            AssetDatabase.CreateAsset(warrior, "Assets/TestData/Skuls/TestWarrior.asset");
            AssetDatabase.CreateAsset(basicAttack, "Assets/TestData/Abilities/WarriorBasicAttack.asset");
            AssetDatabase.CreateAsset(skill1, "Assets/TestData/Abilities/WarriorSkill1.asset");
            AssetDatabase.CreateAsset(skill2, "Assets/TestData/Abilities/WarriorSkill2.asset");
            AssetDatabase.CreateAsset(dash, "Assets/TestData/Abilities/WarriorDash.asset");
        }

        /// <summary>
        /// 마법사 스컬 생성
        /// </summary>
        private void CreateMageSkul()
        {
            // 스컬 데이터
            SkulData mage = ScriptableObject.CreateInstance<SkulData>();
            mage.skulId = "mage_test";
            mage.skulName = "테스트 마법사";
            mage.description = "원거리 공격 테스트용 마법사";
            mage.skulType = SkulType.Range;
            mage.attackPower = 10f;
            mage.attackSpeed = 1f;
            mage.moveSpeed = 4f;
            mage.jumpPower = 9f;
            mage.maxComboCount = 2;
            mage.comboResetTime = 1.5f;
            mage.comboDamageMultipliers = new float[] { 1f, 1.5f };

            // 기본 공격: 파이어볼
            PlatformerAbilityData basicAttack = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            basicAttack.abilityId = "mage_basic";
            basicAttack.abilityName = "파이어볼";
            basicAttack.abilityType = PlatformerAbilityType.BasicAttack;
            basicAttack.damageMultiplier = 1f;
            basicAttack.range = 5f;
            basicAttack.hitboxSize = new Vector2(1f, 1f);
            basicAttack.hitboxOffset = new Vector2(2f, 0);
            basicAttack.hitboxDuration = 0.1f;
            basicAttack.cooldownTime = 0f;
            mage.basicAttack = basicAttack;

            // 스킬 1: 차징 레이저
            PlatformerAbilityData skill1 = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            skill1.abilityId = "mage_skill1";
            skill1.abilityName = "차징 레이저";
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

            // 스킬 2: 메테오
            PlatformerAbilityData skill2 = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            skill2.abilityId = "mage_skill2";
            skill2.abilityName = "메테오";
            skill2.abilityType = PlatformerAbilityType.Skill;
            skill2.damageMultiplier = 2.5f;
            skill2.range = 6f;
            skill2.hitboxSize = new Vector2(3f, 3f);
            skill2.hitboxOffset = new Vector2(3f, 0);
            skill2.hitboxDuration = 0.3f;
            skill2.cooldownTime = 6f;
            skill2.knockbackPower = 8f;
            mage.skill2 = skill2;

            // 대시: 텔레포트
            PlatformerAbilityData dash = ScriptableObject.CreateInstance<PlatformerAbilityData>();
            dash.abilityId = "mage_dash";
            dash.abilityName = "텔레포트";
            dash.abilityType = PlatformerAbilityType.Movement;
            dash.range = 4f;
            dash.cooldownTime = 3f;
            mage.dashAbility = dash;

            // 저장
            AssetDatabase.CreateAsset(mage, "Assets/TestData/Skuls/TestMage.asset");
            AssetDatabase.CreateAsset(basicAttack, "Assets/TestData/Abilities/MageBasicAttack.asset");
            AssetDatabase.CreateAsset(skill1, "Assets/TestData/Abilities/MageSkill1.asset");
            AssetDatabase.CreateAsset(skill2, "Assets/TestData/Abilities/MageSkill2.asset");
            AssetDatabase.CreateAsset(dash, "Assets/TestData/Abilities/MageDash.asset");
        }
#endif
    }
}