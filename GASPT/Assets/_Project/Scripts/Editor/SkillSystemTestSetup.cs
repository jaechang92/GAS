using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using GASPT.Skills;
using GASPT.Stats;
using GASPT.Gameplay.Enemy;
using GASPT.Testing;
using GASPT.Core;
using GASPT.Data;
using Core.Enums;

namespace GASPT.Editor
{
    /// <summary>
    /// SkillSystem 테스트 환경 자동 생성 에디터 툴
    /// Tools > GASPT > Setup Skill System Test Scene
    /// </summary>
    public static class SkillSystemTestSetup
    {
        private const string ASSET_PATH_ROOT = "Assets/_Project/Data";
        private const string SKILL_DATA_PATH = ASSET_PATH_ROOT + "/Skills";
        private const string ENEMY_DATA_PATH = ASSET_PATH_ROOT + "/Enemies";
        private const string STATUS_EFFECT_PATH = ASSET_PATH_ROOT + "/StatusEffects";

        [MenuItem("Tools/GASPT/🚀 One-Click Setup (Create + Setup)", priority = 1)]
        public static void OneClickSetup()
        {
            Debug.Log("========== 원클릭 테스트 환경 생성 시작 ==========");

            // 1. 씬 생성
            CreateTestScene();

            // 2. 약간의 딜레이 (씬 생성 완료 대기)
            System.Threading.Thread.Sleep(100);

            // 3. 테스트 환경 설정
            SetupTestScene();

            Debug.Log("========== 원클릭 테스트 환경 생성 완료 ==========");
            Debug.Log("✅ 준비 완료! Play 버튼을 누르고 SkillSystemTest 우클릭 > Run All Tests!");
        }

        [MenuItem("Tools/GASPT/Create Skill System Test Scene", priority = 10)]
        public static void CreateTestScene()
        {
            // 1. 새 씬 생성
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. 씬 저장
            string scenePath = "Assets/_Project/Scenes/SkillSystemTest.unity";

            // Scenes 폴더 확인
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }

            bool saved = EditorSceneManager.SaveScene(newScene, scenePath);

            if (saved)
            {
                Debug.Log($"✓ 테스트 씬 생성 완료: {scenePath}");
                Debug.Log("이제 'Tools > GASPT > Setup Skill System Test Scene'을 실행하세요!");
            }
            else
            {
                Debug.LogError("❌ 테스트 씬 저장 실패");
            }
        }

        [MenuItem("Tools/GASPT/Setup Skill System Test Scene", priority = 11)]
        public static void SetupTestScene()
        {
            Debug.Log("========== SkillSystem 테스트 환경 자동 생성 시작 ==========");

            // 1. 디렉토리 생성
            CreateDirectories();

            // 2. ScriptableObject 에셋 생성
            var fireballSkill = CreateFireballSkill();
            var healSkill = CreateHealSkill();
            var attackUpEffect = CreateAttackUpStatusEffect();
            var buffSkill = CreateBuffSkill(attackUpEffect);
            var testEnemyData = CreateTestEnemyData();

            // 3. 씬 오브젝트 생성
            var singletonPreloader = CreateSingletonPreloader();
            var player = CreatePlayer();
            var enemy = CreateEnemy(testEnemyData);
            var skillSystemTest = CreateSkillSystemTest(fireballSkill, healSkill, buffSkill, enemy);

            // 4. 씬 저장
            MarkSceneDirty();

            Debug.Log("========== SkillSystem 테스트 환경 자동 생성 완료 ==========");
            Debug.Log("✅ Player, Enemy, SkillSystemTest, SingletonPreloader 생성 완료");
            Debug.Log("✅ SkillData 3개, EnemyData 1개, StatusEffectData 1개 생성 완료");
            Debug.Log("✅ 모든 참조 자동 연결 완료");
            Debug.Log("▶️ Play 버튼을 누르고 SkillSystemTest 우클릭 > Run All Tests 실행!");
        }


        // ====== 디렉토리 생성 ======

        private static void CreateDirectories()
        {
            if (!AssetDatabase.IsValidFolder(ASSET_PATH_ROOT))
                AssetDatabase.CreateFolder("Assets/_Project", "Data");

            if (!AssetDatabase.IsValidFolder(SKILL_DATA_PATH))
                AssetDatabase.CreateFolder(ASSET_PATH_ROOT, "Skills");

            if (!AssetDatabase.IsValidFolder(ENEMY_DATA_PATH))
                AssetDatabase.CreateFolder(ASSET_PATH_ROOT, "Enemies");

            if (!AssetDatabase.IsValidFolder(STATUS_EFFECT_PATH))
                AssetDatabase.CreateFolder(ASSET_PATH_ROOT, "StatusEffects");

            AssetDatabase.Refresh();
        }


        // ====== ScriptableObject 생성 ======

        private static SkillData CreateFireballSkill()
        {
            string assetPath = $"{SKILL_DATA_PATH}/TEST_FireballSkill.asset";

            // 기존 에셋 재사용
            SkillData existing = AssetDatabase.LoadAssetAtPath<SkillData>(assetPath);
            if (existing != null)
            {
                Debug.Log($"✓ 기존 FireballSkill 재사용: {assetPath}");
                return existing;
            }

            SkillData skill = ScriptableObject.CreateInstance<SkillData>();
            skill.skillName = "TEST Fireball";
            skill.description = "테스트용 화염구 스킬";
            skill.skillType = SkillType.Damage;
            skill.targetType = TargetType.Enemy;
            skill.manaCost = 20;
            skill.cooldown = 3f;
            skill.damageAmount = 50;
            skill.skillRange = 10f;

            AssetDatabase.CreateAsset(skill, assetPath);
            Debug.Log($"✓ FireballSkill 생성: {assetPath}");
            return skill;
        }

        private static SkillData CreateHealSkill()
        {
            string assetPath = $"{SKILL_DATA_PATH}/TEST_HealSkill.asset";

            SkillData existing = AssetDatabase.LoadAssetAtPath<SkillData>(assetPath);
            if (existing != null)
            {
                Debug.Log($"✓ 기존 HealSkill 재사용: {assetPath}");
                return existing;
            }

            SkillData skill = ScriptableObject.CreateInstance<SkillData>();
            skill.skillName = "TEST Heal";
            skill.description = "테스트용 회복 스킬";
            skill.skillType = SkillType.Heal;
            skill.targetType = TargetType.Self;
            skill.manaCost = 15;
            skill.cooldown = 5f;
            skill.healAmount = 30;

            AssetDatabase.CreateAsset(skill, assetPath);
            Debug.Log($"✓ HealSkill 생성: {assetPath}");
            return skill;
        }

        private static StatusEffectData CreateAttackUpStatusEffect()
        {
            string assetPath = $"{STATUS_EFFECT_PATH}/TEST_AttackUp.asset";

            StatusEffectData existing = AssetDatabase.LoadAssetAtPath<StatusEffectData>(assetPath);
            if (existing != null)
            {
                Debug.Log($"✓ 기존 AttackUp StatusEffect 재사용: {assetPath}");
                return existing;
            }

            StatusEffectData effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.effectType = StatusEffectType.AttackUp;
            effect.displayName = "TEST AttackUp";
            effect.description = "테스트용 공격력 증가 버프";
            effect.isBuff = true;
            effect.value = 10f;
            effect.duration = 10f;
            effect.tickInterval = 0f;
            effect.maxStack = 3;

            AssetDatabase.CreateAsset(effect, assetPath);
            Debug.Log($"✓ AttackUp StatusEffect 생성: {assetPath}");
            return effect;
        }

        private static SkillData CreateBuffSkill(StatusEffectData statusEffect)
        {
            string assetPath = $"{SKILL_DATA_PATH}/TEST_AttackBuffSkill.asset";

            SkillData existing = AssetDatabase.LoadAssetAtPath<SkillData>(assetPath);
            if (existing != null)
            {
                // 기존 에셋의 statusEffect 필드 업데이트
                existing.statusEffect = statusEffect;
                EditorUtility.SetDirty(existing);
                Debug.Log($"✓ 기존 BuffSkill 재사용 및 업데이트: {assetPath}");
                return existing;
            }

            SkillData skill = ScriptableObject.CreateInstance<SkillData>();
            skill.skillName = "TEST Attack Buff";
            skill.description = "테스트용 공격력 버프 스킬";
            skill.skillType = SkillType.Buff;
            skill.targetType = TargetType.Self;
            skill.manaCost = 10;
            skill.cooldown = 10f;
            skill.statusEffect = statusEffect;

            AssetDatabase.CreateAsset(skill, assetPath);
            Debug.Log($"✓ BuffSkill 생성: {assetPath}");
            return skill;
        }

        private static EnemyData CreateTestEnemyData()
        {
            string assetPath = $"{ENEMY_DATA_PATH}/TEST_Enemy.asset";

            EnemyData existing = AssetDatabase.LoadAssetAtPath<EnemyData>(assetPath);
            if (existing != null)
            {
                Debug.Log($"✓ 기존 EnemyData 재사용: {assetPath}");
                return existing;
            }

            EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.enemyName = "TEST Enemy";
            enemy.enemyType = EnemyType.Normal;
            enemy.maxHp = 100;
            enemy.attack = 15;
            enemy.minGoldDrop = 10;
            enemy.maxGoldDrop = 20;

            AssetDatabase.CreateAsset(enemy, assetPath);
            Debug.Log($"✓ EnemyData 생성: {assetPath}");
            return enemy;
        }


        // ====== 씬 오브젝트 생성 ======

        private static GameObject CreateSingletonPreloader()
        {
            // 기존 SingletonPreloader 찾기
            SingletonPreloader existing = Object.FindAnyObjectByType<SingletonPreloader>();
            if (existing != null)
            {
                Debug.Log($"✓ 기존 SingletonPreloader 재사용: {existing.name}");
                return existing.gameObject;
            }

            GameObject obj = new GameObject("SingletonPreloader");
            obj.AddComponent<SingletonPreloader>();

            Debug.Log($"✓ SingletonPreloader 생성: {obj.name}");
            return obj;
        }

        private static GameObject CreatePlayer()
        {
            // 기존 Player 찾기
            GameObject existing = GameObject.FindWithTag("Player");
            if (existing != null)
            {
                // PlayerStats 컴포넌트 확인
                PlayerStats playerStats = existing.GetComponent<PlayerStats>();
                if (playerStats == null)
                {
                    playerStats = existing.AddComponent<PlayerStats>();
                    Debug.Log($"✓ 기존 Player에 PlayerStats 추가: {existing.name}");
                }
                else
                {
                    Debug.Log($"✓ 기존 Player 재사용: {existing.name}");
                }
                return existing;
            }

            GameObject player = new GameObject("Player");
            player.tag = "Player";
            PlayerStats stats = player.AddComponent<PlayerStats>();

            // Inspector에서 설정 (Reflection으로 private 필드 설정)
            var baseHPField = typeof(PlayerStats).GetField("baseHP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseAttackField = typeof(PlayerStats).GetField("baseAttack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseDefenseField = typeof(PlayerStats).GetField("baseDefense", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseManaField = typeof(PlayerStats).GetField("baseMana", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (baseHPField != null) baseHPField.SetValue(stats, 100);
            if (baseAttackField != null) baseAttackField.SetValue(stats, 10);
            if (baseDefenseField != null) baseDefenseField.SetValue(stats, 5);
            if (baseManaField != null) baseManaField.SetValue(stats, 100);

            player.transform.position = Vector3.zero;

            Debug.Log($"✓ Player 생성: {player.name}");
            return player;
        }

        private static GameObject CreateEnemy(EnemyData enemyData)
        {
            // 기존 TestEnemy 찾기
            GameObject existing = GameObject.Find("TestEnemy");
            if (existing != null)
            {
                Enemy enemyComp = existing.GetComponent<Enemy>();
                if (enemyComp == null)
                {
                    enemyComp = existing.AddComponent<Enemy>();
                    SetEnemyData(enemyComp, enemyData);
                    Debug.Log($"✓ 기존 TestEnemy에 Enemy 컴포넌트 추가: {existing.name}");
                }
                else
                {
                    SetEnemyData(enemyComp, enemyData);
                    Debug.Log($"✓ 기존 TestEnemy 재사용: {existing.name}");
                }
                return existing;
            }

            GameObject enemy = new GameObject("TestEnemy");
            Enemy enemyComponent = enemy.AddComponent<Enemy>();
            SetEnemyData(enemyComponent, enemyData);

            enemy.transform.position = new Vector3(3f, 0f, 0f);

            Debug.Log($"✓ TestEnemy 생성: {enemy.name}");
            return enemy;
        }

        private static void SetEnemyData(Enemy enemy, EnemyData enemyData)
        {
            // Reflection으로 private enemyData 필드 설정
            var enemyDataField = typeof(Enemy).GetField("enemyData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (enemyDataField != null)
            {
                enemyDataField.SetValue(enemy, enemyData);
            }

            // SerializedObject로 설정 (더 안전한 방법)
            SerializedObject so = new SerializedObject(enemy);
            SerializedProperty prop = so.FindProperty("enemyData");
            if (prop != null)
            {
                prop.objectReferenceValue = enemyData;
                so.ApplyModifiedProperties();
            }
        }

        private static GameObject CreateSkillSystemTest(SkillData skill1, SkillData skill2, SkillData skill3, GameObject enemy)
        {
            // 기존 SkillSystemTest 찾기
            SkillSystemTest existing = Object.FindAnyObjectByType<SkillSystemTest>();
            if (existing != null)
            {
                SetSkillSystemTestFields(existing, skill1, skill2, skill3, enemy);
                Debug.Log($"✓ 기존 SkillSystemTest 재사용 및 업데이트: {existing.name}");
                return existing.gameObject;
            }

            GameObject obj = new GameObject("SkillSystemTest");
            SkillSystemTest test = obj.AddComponent<SkillSystemTest>();
            SetSkillSystemTestFields(test, skill1, skill2, skill3, enemy);

            Debug.Log($"✓ SkillSystemTest 생성: {obj.name}");
            return obj;
        }

        private static void SetSkillSystemTestFields(SkillSystemTest test, SkillData skill1, SkillData skill2, SkillData skill3, GameObject enemy)
        {
            SerializedObject so = new SerializedObject(test);

            SerializedProperty testSkill1 = so.FindProperty("testSkill1");
            SerializedProperty testSkill2 = so.FindProperty("testSkill2");
            SerializedProperty testSkill3 = so.FindProperty("testSkill3");
            SerializedProperty testEnemy = so.FindProperty("testEnemy");
            SerializedProperty autoCreateEnemy = so.FindProperty("autoCreateEnemy");

            if (testSkill1 != null) testSkill1.objectReferenceValue = skill1;
            if (testSkill2 != null) testSkill2.objectReferenceValue = skill2;
            if (testSkill3 != null) testSkill3.objectReferenceValue = skill3;
            if (testEnemy != null) testEnemy.objectReferenceValue = enemy;
            if (autoCreateEnemy != null) autoCreateEnemy.boolValue = false; // 이미 생성했으므로

            so.ApplyModifiedProperties();
        }


        // ====== 유틸리티 ======

        private static void MarkSceneDirty()
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        // ====== 추가 유틸리티 메뉴 ======

        [MenuItem("Tools/GASPT/Clear Test Scene", priority = 100)]
        public static void ClearTestScene()
        {
            if (!EditorUtility.DisplayDialog("Clear Test Scene",
                "테스트 씬의 모든 오브젝트를 삭제하시겠습니까?\n(ScriptableObject 에셋은 유지됩니다)",
                "삭제", "취소"))
            {
                return;
            }

            // 씬의 모든 오브젝트 삭제
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                Object.DestroyImmediate(obj);
            }

            MarkSceneDirty();
            Debug.Log("✓ 테스트 씬 오브젝트 삭제 완료 (에셋은 유지됨)");
        }

        [MenuItem("Tools/GASPT/Delete Test Assets", priority = 101)]
        public static void DeleteTestAssets()
        {
            if (!EditorUtility.DisplayDialog("Delete Test Assets",
                "테스트용 ScriptableObject 에셋을 모두 삭제하시겠습니까?\n(TEST_ 접두사 파일들)",
                "삭제", "취소"))
            {
                return;
            }

            int deletedCount = 0;

            // TEST_ 접두사 파일 찾기 및 삭제
            string[] guids = AssetDatabase.FindAssets("TEST_", new[] { ASSET_PATH_ROOT });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.DeleteAsset(path);
                deletedCount++;
                Debug.Log($"✓ 삭제: {path}");
            }

            AssetDatabase.Refresh();
            Debug.Log($"✓ 테스트 에셋 {deletedCount}개 삭제 완료");
        }
    }
}
