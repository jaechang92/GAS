using UnityEngine;
using UnityEditor;
using GASPT.Forms;

namespace GASPT.Editor
{
    /// <summary>
    /// 폼 에셋 생성 에디터 도구
    /// </summary>
    public class FormAssetCreator
    {
        private const string FORMS_PATH = "Assets/Resources/Data/Forms/";

        [MenuItem("Tools/GASPT/Forms/Create Default Forms", false, 100)]
        public static void CreateDefaultForms()
        {
            EnsureFolder();

            CreateBasicMage();
            CreateFlameMage();
            CreateFrostMage();
            CreateThunderMage();
            CreateDarkMage();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[FormAssetCreator] 기본 폼 5종 생성 완료!");
            EditorUtility.DisplayDialog("완료", "기본 폼 5종이 생성되었습니다.\n\n경로: " + FORMS_PATH, "확인");
        }

        [MenuItem("Tools/GASPT/Forms/Create Basic Mage Only", false, 101)]
        public static void CreateBasicMageOnly()
        {
            EnsureFolder();
            CreateBasicMage();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[FormAssetCreator] BasicMage 폼 생성 완료!");
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Data"))
                AssetDatabase.CreateFolder("Assets/Resources", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Data/Forms"))
                AssetDatabase.CreateFolder("Assets/Resources/Data", "Forms");
        }

        private static void CreateBasicMage()
        {
            var form = ScriptableObject.CreateInstance<FormData>();
            form.formId = "form_basic_mage";
            form.formName = "기본 마법사";
            form.description = "균형 잡힌 스탯을 가진 기본 마법사입니다.\n모든 상황에 무난하게 대응할 수 있습니다.";
            form.formType = FormType.Basic;
            form.baseRarity = FormRarity.Common;
            form.baseStats = new FormStats
            {
                attackPower = 10f,
                attackSpeed = 1f,
                criticalChance = 0.05f,
                criticalDamage = 1.5f,
                maxHealthBonus = 0f,
                defense = 0f,
                moveSpeed = 1f,
                jumpPower = 1f,
                cooldownReduction = 0f,
                manaRegen = 1f
            };
            form.formColor = new Color(0.6f, 0.6f, 1f); // 연한 파랑
            form.dropWeight = 100;
            form.playstyleHint = "균형형 - 초보자 추천";
            form.primarySkillId = "skill_basic_attack";
            form.secondarySkillId = "skill_basic_special";

            SaveAsset(form, "BasicMage");
        }

        private static void CreateFlameMage()
        {
            var form = ScriptableObject.CreateInstance<FormData>();
            form.formId = "form_flame_mage";
            form.formName = "화염 마법사";
            form.description = "높은 공격력과 범위 공격이 특징인 화염 마법사입니다.\n적을 한꺼번에 쓸어버리세요!";
            form.formType = FormType.Flame;
            form.baseRarity = FormRarity.Common;
            form.baseStats = new FormStats
            {
                attackPower = 15f,
                attackSpeed = 0.9f,
                criticalChance = 0.1f,
                criticalDamage = 1.8f,
                maxHealthBonus = -10f,
                defense = -2f,
                moveSpeed = 0.95f,
                jumpPower = 1f,
                cooldownReduction = 0f,
                manaRegen = 0.9f
            };
            form.formColor = new Color(1f, 0.4f, 0.2f); // 주황-빨강
            form.dropWeight = 60;
            form.playstyleHint = "공격형 - 범위 딜러";
            form.primarySkillId = "skill_flame_attack";
            form.secondarySkillId = "skill_flame_burst";

            SaveAsset(form, "FlameMage");
        }

        private static void CreateFrostMage()
        {
            var form = ScriptableObject.CreateInstance<FormData>();
            form.formId = "form_frost_mage";
            form.formName = "얼음 마법사";
            form.description = "적을 둔화시키고 안전하게 싸우는 얼음 마법사입니다.\n신중한 플레이를 선호하는 분께 추천합니다.";
            form.formType = FormType.Frost;
            form.baseRarity = FormRarity.Common;
            form.baseStats = new FormStats
            {
                attackPower = 8f,
                attackSpeed = 1.1f,
                criticalChance = 0.03f,
                criticalDamage = 1.4f,
                maxHealthBonus = 20f,
                defense = 5f,
                moveSpeed = 0.9f,
                jumpPower = 1f,
                cooldownReduction = 0.1f,
                manaRegen = 1.1f
            };
            form.formColor = new Color(0.4f, 0.8f, 1f); // 하늘색
            form.dropWeight = 60;
            form.playstyleHint = "방어형 - CC 특화";
            form.primarySkillId = "skill_frost_attack";
            form.secondarySkillId = "skill_frost_nova";

            SaveAsset(form, "FrostMage");
        }

        private static void CreateThunderMage()
        {
            var form = ScriptableObject.CreateInstance<FormData>();
            form.formId = "form_thunder_mage";
            form.formName = "번개 마법사";
            form.description = "빠른 공격 속도와 높은 기동성을 가진 번개 마법사입니다.\n적을 휘몰아치듯 공격하세요!";
            form.formType = FormType.Thunder;
            form.baseRarity = FormRarity.Common;
            form.baseStats = new FormStats
            {
                attackPower = 7f,
                attackSpeed = 1.4f,
                criticalChance = 0.15f,
                criticalDamage = 1.6f,
                maxHealthBonus = -5f,
                defense = 0f,
                moveSpeed = 1.2f,
                jumpPower = 1.15f,
                cooldownReduction = 0.15f,
                manaRegen = 1.2f
            };
            form.formColor = new Color(1f, 1f, 0.3f); // 노란색
            form.dropWeight = 50;
            form.playstyleHint = "속공형 - 연속 공격";
            form.primarySkillId = "skill_thunder_attack";
            form.secondarySkillId = "skill_thunder_dash";

            SaveAsset(form, "ThunderMage");
        }

        private static void CreateDarkMage()
        {
            var form = ScriptableObject.CreateInstance<FormData>();
            form.formId = "form_dark_mage";
            form.formName = "암흑 마법사";
            form.description = "위험하지만 강력한 암흑 마법사입니다.\n체력 흡수로 전선에서 버틸 수 있습니다.";
            form.formType = FormType.Dark;
            form.baseRarity = FormRarity.Rare;
            form.baseStats = new FormStats
            {
                attackPower = 18f,
                attackSpeed = 0.85f,
                criticalChance = 0.2f,
                criticalDamage = 2f,
                maxHealthBonus = -30f,
                defense = -5f,
                moveSpeed = 1f,
                jumpPower = 1f,
                cooldownReduction = 0f,
                manaRegen = 0.8f
            };
            form.formColor = new Color(0.5f, 0.2f, 0.6f); // 보라색
            form.dropWeight = 30;
            form.playstyleHint = "고위험 고보상 - 흡혈 특화";
            form.primarySkillId = "skill_dark_attack";
            form.secondarySkillId = "skill_dark_drain";

            SaveAsset(form, "DarkMage");
        }

        private static void SaveAsset(FormData form, string fileName)
        {
            string path = FORMS_PATH + fileName + ".asset";

            // 기존 에셋 있으면 덮어쓰기
            var existing = AssetDatabase.LoadAssetAtPath<FormData>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(form, existing);
                EditorUtility.SetDirty(existing);
                Debug.Log($"[FormAssetCreator] 업데이트: {path}");
            }
            else
            {
                AssetDatabase.CreateAsset(form, path);
                Debug.Log($"[FormAssetCreator] 생성: {path}");
            }
        }
    }
}
