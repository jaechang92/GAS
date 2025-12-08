using UnityEngine;
using UnityEditor;
using GASPT.Forms;
using GASPT.Skills;
using System.Collections.Generic;

namespace GASPT.Editor
{
    /// <summary>
    /// 폼 에셋 생성 에디터 도구 (Spec 019)
    /// </summary>
    public class FormAssetCreator
    {
        private const string FORMS_PATH = "Assets/Resources/Data/Forms/";
        private const string SKILLS_PATH = "Assets/Resources/Data/Skills/"; // 가정된 경로

        [MenuItem("Tools/GASPT/Forms/Create Default Forms (Spec 019)", false, 100)]
        public static void CreateDefaultForms()
        {
            EnsureFolder();

            CreateFlameMage();
            CreateFrostMage();
            CreateThunderMage();
            CreateDarkMage();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[FormAssetCreator] Spec 019 기반 폼 4종 생성 완료!");
            EditorUtility.DisplayDialog("완료", "폼 4종이 생성되었습니다.\n\n경로: " + FORMS_PATH, "확인");
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

        private static SkillData LoadSkill(string skillId)
        {
            // 실제 구현 시 스킬 에셋 경로에 맞춰 수정 필요
            // 여기서는 이름으로 검색하거나 가상의 경로 사용
            // 예: Resources/Data/Skills/skillId.asset
            var skill = Resources.Load<SkillData>($"Data/Skills/{skillId}");
            if (skill == null)
            {
                // AssetDatabase 검색
                string[] guids = AssetDatabase.FindAssets($"t:SkillData {skillId}");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    skill = AssetDatabase.LoadAssetAtPath<SkillData>(path);
                }
            }
            return skill;
        }

        // Helper to avoid repetitive code
        private static FormData CreateFormAssetInstance(string id, string name, FormType type, FormRarity rarity)
        {
            var form = ScriptableObject.CreateInstance<FormData>();
            form.formId = id;
            form.formName = name;
            form.formType = type;
            form.baseRarity = rarity;
            form.maxAwakeningLevel = 3; 
            return form;
        }



        private static void CreateFlameMage()
        {
            var form = CreateFormAssetInstance("form_flame_mage", "화염 마법사", FormType.Flame, FormRarity.Rare);
            form.description = "높은 공격력과 범위 공격이 특징인 화염 마법사입니다.\n적을 한꺼번에 쓸어버리세요!";
            form.formColor = new Color(1f, 0.4f, 0.2f);
            form.dropWeight = 60;
            form.playstyleHint = "공격형 - 범위 딜러";

            // Spec 3.2: Rare, Unique, Legendary (Starts at Rare)
            form.statsByRarity = new FormStats[]
            {
                // Lv0 (Rare)
                new FormStats { attackPower = 8f, attackSpeed = 0.9f, moveSpeed = 0.95f, maxHealthBonus = 5f, maxManaBonus = 15f },
                // Lv1 (Unique)
                new FormStats { attackPower = 20f, attackSpeed = 0.95f, moveSpeed = 1.0f, maxHealthBonus = 15f, maxManaBonus = 35f },
                // Lv2 (Legendary)
                new FormStats { attackPower = 40f, attackSpeed = 1.0f, moveSpeed = 1.05f, maxHealthBonus = 30f, maxManaBonus = 60f },
                 // Lv3 (Legendary+) - Spec doesn't define, extrapolate
                new FormStats { attackPower = 50f, attackSpeed = 1.0f, moveSpeed = 1.05f, maxHealthBonus = 40f, maxManaBonus = 70f }
            };

            form.skill1 = LoadSkill("Fireball");
            form.skill2 = LoadSkill("FireStorm");

            SaveAsset(form, "FlameMage");
        }

        private static void CreateFrostMage()
        {
            var form = CreateFormAssetInstance("form_frost_mage", "얼음 마법사", FormType.Frost, FormRarity.Rare);
            form.description = "적을 둔화시키고 안전하게 싸우는 얼음 마법사입니다.\n신중한 플레이를 선호하는 분께 추천합니다.";
            form.formColor = new Color(0.4f, 0.8f, 1f);
            form.dropWeight = 60;
            form.playstyleHint = "방어형 - CC 특화";

            // Spec 3.3
            form.statsByRarity = new FormStats[]
            {
                // Lv0 (Rare)
                new FormStats { attackPower = 3f, attackSpeed = 0.95f, moveSpeed = 1.0f, maxHealthBonus = 10f, maxManaBonus = 25f },
                // Lv1 (Unique)
                new FormStats { attackPower = 8f, attackSpeed = 1.0f, moveSpeed = 1.05f, maxHealthBonus = 25f, maxManaBonus = 50f },
                // Lv2 (Legendary)
                new FormStats { attackPower = 18f, attackSpeed = 1.1f, moveSpeed = 1.1f, maxHealthBonus = 45f, maxManaBonus = 80f },
                // Lv3
                new FormStats { attackPower = 25f, attackSpeed = 1.15f, moveSpeed = 1.15f, maxHealthBonus = 60f, maxManaBonus = 100f }
            };

            form.skill1 = LoadSkill("IceLance");
            form.skill2 = LoadSkill("FrozenGround");

            SaveAsset(form, "FrostMage");
        }

        private static void CreateThunderMage()
        {
            var form = CreateFormAssetInstance("form_thunder_mage", "번개 마법사", FormType.Thunder, FormRarity.Rare);
            form.description = "빠른 공격 속도와 높은 기동성을 가진 번개 마법사입니다.\n적을 휘몰아치듯 공격하세요!";
            form.formColor = new Color(1f, 1f, 0.3f);
            form.dropWeight = 50;
            form.playstyleHint = "속공형 - 연속 공격";

            // Spec 3.4
            form.statsByRarity = new FormStats[]
            {
                // Lv0 (Rare)
                new FormStats { attackPower = 5f, attackSpeed = 1.2f, moveSpeed = 1.1f, maxHealthBonus = 0f, maxManaBonus = 10f },
                // Lv1 (Unique)
                new FormStats { attackPower = 12f, attackSpeed = 1.35f, moveSpeed = 1.2f, maxHealthBonus = 10f, maxManaBonus = 25f },
                // Lv2 (Legendary)
                new FormStats { attackPower = 25f, attackSpeed = 1.5f, moveSpeed = 1.3f, maxHealthBonus = 25f, maxManaBonus = 45f },
                // Lv3
                new FormStats { attackPower = 35f, attackSpeed = 1.6f, moveSpeed = 1.4f, maxHealthBonus = 40f, maxManaBonus = 60f }
            };

            form.skill1 = LoadSkill("ChainLightning");
            form.skill2 = LoadSkill("ThunderRush");

            SaveAsset(form, "ThunderMage");
        }

        private static void CreateDarkMage()
        {
            var form = CreateFormAssetInstance("form_dark_mage", "암흑 마법사", FormType.Dark, FormRarity.Unique);
            form.description = "위험하지만 강력한 암흑 마법사입니다.\n체력 흡수로 전선에서 버틸 수 있습니다.";
            form.formColor = new Color(0.5f, 0.2f, 0.6f);
            form.dropWeight = 30;
            form.playstyleHint = "고위험 고보상 - 흡혈 특화";

            // Spec 3.5: Unique, Legendary
            form.statsByRarity = new FormStats[]
            {
                // Lv0 (Unique)
                new FormStats { attackPower = 15f, attackSpeed = 1.0f, moveSpeed = 0.95f, maxHealthBonus = -10f, maxManaBonus = 20f, vampirismRate = 0.1f },
                // Lv1 (Legendary)
                new FormStats { attackPower = 35f, attackSpeed = 1.1f, moveSpeed = 1.0f, maxHealthBonus = 0f, maxManaBonus = 40f, vampirismRate = 0.2f },
                // Lv2 (Legendary+)
                new FormStats { attackPower = 45f, attackSpeed = 1.15f, moveSpeed = 1.05f, maxHealthBonus = 10f, maxManaBonus = 50f, vampirismRate = 0.25f },
                // Lv3
                new FormStats { attackPower = 55f, attackSpeed = 1.2f, moveSpeed = 1.1f, maxHealthBonus = 20f, maxManaBonus = 60f, vampirismRate = 0.3f }
            };

            form.skill1 = LoadSkill("LifeDrain");
            form.skill2 = LoadSkill("SoulExplosion");

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
