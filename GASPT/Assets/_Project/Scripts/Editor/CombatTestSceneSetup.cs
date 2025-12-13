using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using GASPT.Testing;
using GASPT.Stats;
using GASPT.Data;
using GASPT.Skills;
using GASPT.Core.Enums;
using System.IO;
using GASPT.Gameplay.Enemies;

namespace GASPT.Editor
{
    /// <summary>
    /// CombatTestScene 원클릭 생성 에디터 도구
    /// Menu: Tools > GASPT > Combat Test > Create Combat Test Scene
    /// </summary>
    public class CombatTestSceneSetup
    {
        private const string SCENE_NAME = "CombatTestScene";
        private const string SCENE_PATH = "Assets/_Project/Scenes/" + SCENE_NAME + ".unity";
        private const string ENEMY_DATA_FOLDER = "Assets/_Project/Data/Enemies/TestEnemies";
        private const string SKILL_DATA_FOLDER = "Assets/_Project/Data/Skills";

        [MenuItem("Tools/GASPT/Combat Test/🚀 Create Combat Test Scene", false, 100)]
        public static void CreateCombatTestScene()
        {
            Debug.Log("========== Combat Test Scene 생성 시작 ==========");

            // 1. 새 씬 생성
            Scene scene = CreateNewScene();

            // 2. 폴더 생성
            CreateFolders();

            // 3. Enemy Data 생성
            EnemyData weakEnemy = CreateWeakEnemyData();
            EnemyData normalEnemy = CreateNormalEnemyData();
            EnemyData strongEnemy = CreateStrongEnemyData();

            // 4. Skill Data 생성
            SkillData fireball = CreateFireballSkill();
            SkillData iceBlast = CreateIceBlastSkill();
            SkillData heal = CreateHealSkill();
            SkillData powerBuff = CreatePowerBuffSkill();

            // 5. Player 생성
            GameObject player = CreatePlayer();

            // 6. Enemy Prefab 생성
            GameObject enemyPrefab = CreateEnemyPrefab();

            // 7. SpawnPoints 생성
            Transform[] spawnPoints = CreateSpawnPoints();

            // 8. CombatTestManager 생성 및 설정
            GameObject manager = CreateCombatTestManager(player, enemyPrefab, spawnPoints,
                weakEnemy, normalEnemy, strongEnemy,
                fireball, iceBlast, heal, powerBuff);

            // 9. 카메라 설정
            SetupCamera(player);

            // 10. 씬 저장
            EditorSceneManager.SaveScene(scene, SCENE_PATH);

            Debug.Log("========== Combat Test Scene 생성 완료 ==========");
            Debug.Log($"씬 경로: {SCENE_PATH}");
            EditorUtility.DisplayDialog("완료", $"Combat Test Scene이 생성되었습니다!\n경로: {SCENE_PATH}", "확인");
        }

        // ====== 씬 생성 ======

        private static Scene CreateNewScene()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Debug.Log("[Setup] 새 씬 생성 완료");
            return newScene;
        }

        // ====== 폴더 생성 ======

        private static void CreateFolders()
        {
            if (!AssetDatabase.IsValidFolder(ENEMY_DATA_FOLDER))
            {
                string parentFolder = "Assets/_Project/Data/Enemies";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets/_Project/Data", "Enemies");
                }
                AssetDatabase.CreateFolder(parentFolder, "TestEnemies");
            }

            if (!AssetDatabase.IsValidFolder(SKILL_DATA_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Skills");
            }

            Debug.Log("[Setup] 폴더 생성 완료");
        }

        // ====== EnemyData 생성 ======

        private static EnemyData CreateWeakEnemyData()
        {
            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();
            SetEnemyDataFields(data, "TEST_WeakEnemy", EnemyType.Normal, 30, 5, 0, 10, 5);

            string path = $"{ENEMY_DATA_FOLDER}/TEST_WeakEnemy.asset";
            AssetDatabase.CreateAsset(data, path);
            Debug.Log($"[Setup] EnemyData 생성: {path}");
            return data;
        }

        private static EnemyData CreateNormalEnemyData()
        {
            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();
            SetEnemyDataFields(data, "TEST_NormalEnemy", EnemyType.Normal, 50, 10, 2, 25, 15);

            string path = $"{ENEMY_DATA_FOLDER}/TEST_NormalEnemy.asset";
            AssetDatabase.CreateAsset(data, path);
            Debug.Log($"[Setup] EnemyData 생성: {path}");
            return data;
        }

        private static EnemyData CreateStrongEnemyData()
        {
            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();
            SetEnemyDataFields(data, "TEST_StrongEnemy", EnemyType.Named, 100, 20, 5, 50, 30);

            string path = $"{ENEMY_DATA_FOLDER}/TEST_StrongEnemy.asset";
            AssetDatabase.CreateAsset(data, path);
            Debug.Log($"[Setup] EnemyData 생성: {path}");
            return data;
        }

        private static void SetEnemyDataFields(EnemyData data, string name, EnemyType type,
            int maxHp, int attack, int defense, int expReward, int goldReward)
        {
            var enemyNameField = typeof(EnemyData).GetField("enemyName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var enemyTypeField = typeof(EnemyData).GetField("enemyType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxHpField = typeof(EnemyData).GetField("maxHp",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var attackField = typeof(EnemyData).GetField("attack",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var defenseField = typeof(EnemyData).GetField("defense",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var expRewardField = typeof(EnemyData).GetField("expReward",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var goldRewardField = typeof(EnemyData).GetField("goldReward",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            enemyNameField?.SetValue(data, name);
            enemyTypeField?.SetValue(data, type);
            maxHpField?.SetValue(data, maxHp);
            attackField?.SetValue(data, attack);
            defenseField?.SetValue(data, defense);
            expRewardField?.SetValue(data, expReward);
            goldRewardField?.SetValue(data, goldReward);
        }

        // ====== SkillData 생성 ======

        private static SkillData CreateFireballSkill()
        {
            SkillData data = ScriptableObject.CreateInstance<SkillData>();
            SetSkillDataFields(data, "Fireball", "화염구를 발사하여 적에게 피해를 입힙니다.",
                SkillType.Damage, TargetType.Enemy, 20, 3f, 0f, 30, 0, null);

            string path = $"{SKILL_DATA_FOLDER}/TEST_FireballSkill.asset";
            AssetDatabase.CreateAsset(data, path);
            Debug.Log($"[Setup] SkillData 생성: {path}");
            return data;
        }

        private static SkillData CreateIceBlastSkill()
        {
            SkillData data = ScriptableObject.CreateInstance<SkillData>();
            SetSkillDataFields(data, "Ice Blast", "강력한 얼음 폭발로 적에게 큰 피해를 입힙니다.",
                SkillType.Damage, TargetType.Enemy, 30, 5f, 0f, 50, 0, null);

            string path = $"{SKILL_DATA_FOLDER}/TEST_IceBlastSkill.asset";
            AssetDatabase.CreateAsset(data, path);
            Debug.Log($"[Setup] SkillData 생성: {path}");
            return data;
        }

        private static SkillData CreateHealSkill()
        {
            SkillData data = ScriptableObject.CreateInstance<SkillData>();
            SetSkillDataFields(data, "Heal", "자신의 체력을 회복합니다.",
                SkillType.Heal, TargetType.Self, 25, 8f, 0f, 0, 40, null);

            string path = $"{SKILL_DATA_FOLDER}/TEST_HealSkill.asset";
            AssetDatabase.CreateAsset(data, path);
            Debug.Log($"[Setup] SkillData 생성: {path}");
            return data;
        }

        private static SkillData CreatePowerBuffSkill()
        {
            SkillData data = ScriptableObject.CreateInstance<SkillData>();
            // Buff 스킬은 StatusEffectData 필요 (null로 설정, 나중에 수동 할당)
            SetSkillDataFields(data, "Power Buff", "일정 시간 동안 공격력을 증가시킵니다.",
                SkillType.Buff, TargetType.Self, 35, 15f, 0f, 0, 0, null);

            string path = $"{SKILL_DATA_FOLDER}/TEST_AttackBuffSkill.asset";
            AssetDatabase.CreateAsset(data, path);
            Debug.Log($"[Setup] SkillData 생성: {path}");
            return data;
        }

        private static void SetSkillDataFields(SkillData data, string name, string desc,
            SkillType skillType, TargetType targetType, int manaCost, float cooldown, float castTime,
            int damageAmount, int healAmount, object statusEffect)
        {
            // SkillData의 필드들이 public이므로 직접 할당 가능
            data.skillName = name;
            data.description = desc;
            data.skillType = skillType;
            data.targetType = targetType;
            data.manaCost = manaCost;
            data.cooldown = cooldown;
            data.castTime = castTime;
            data.damageAmount = damageAmount;
            data.healAmount = healAmount;
            data.statusEffect = statusEffect as StatusEffectData;
        }

        // ====== Player 생성 ======

        private static GameObject CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            // PlayerStats 추가
            PlayerStats stats = player.AddComponent<PlayerStats>();

            // 기본 스탯 설정 (Reflection)
            var baseHpField = typeof(PlayerStats).GetField("baseHP",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseAttackField = typeof(PlayerStats).GetField("baseAttack",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseDefenseField = typeof(PlayerStats).GetField("baseDefense",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseManaField = typeof(PlayerStats).GetField("baseMana",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            baseHpField?.SetValue(stats, 100);
            baseAttackField?.SetValue(stats, 15);
            baseDefenseField?.SetValue(stats, 5);
            baseManaField?.SetValue(stats, 100);

            // 비주얼 (임시 Cube)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(player.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.GetComponent<Renderer>().material.color = Color.blue;

            Debug.Log("[Setup] Player 생성 완료");
            return player;
        }

        // ====== Enemy Prefab 생성 ======

        private static GameObject CreateEnemyPrefab()
        {
            GameObject enemy = new GameObject("EnemyPrefab");

            // Enemy 컴포넌트 추가
            enemy.AddComponent<Enemy>();

            // 비주얼 (임시 Sphere)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.transform.SetParent(enemy.transform);
            visual.transform.localPosition = Vector3.zero;

            // Prefab으로 저장
            string prefabPath = "Assets/_Project/Prefabs/Enemies/EnemyPrefab.prefab";
            string prefabFolder = "Assets/_Project/Prefabs/Enemies";

            if (!AssetDatabase.IsValidFolder(prefabFolder))
            {
                string parentFolder = "Assets/_Project/Prefabs";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
                }
                AssetDatabase.CreateFolder(parentFolder, "Enemies");
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, prefabPath);
            GameObject.DestroyImmediate(enemy); // 씬에서 제거

            Debug.Log($"[Setup] Enemy Prefab 생성: {prefabPath}");
            return prefab;
        }

        // ====== SpawnPoints 생성 ======

        private static Transform[] CreateSpawnPoints()
        {
            GameObject spawnParent = new GameObject("SpawnPoints");
            Transform[] spawnPoints = new Transform[5];

            // 플레이어 우측에 5개 위치
            Vector3[] positions = new Vector3[]
            {
                new Vector3(5f, 0f, 0f),
                new Vector3(5f, 2f, 0f),
                new Vector3(5f, -2f, 0f),
                new Vector3(7f, 1f, 0f),
                new Vector3(7f, -1f, 0f)
            };

            for (int i = 0; i < 5; i++)
            {
                GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
                spawnPoint.transform.SetParent(spawnParent.transform);
                spawnPoint.transform.position = positions[i];
                spawnPoints[i] = spawnPoint.transform;
            }

            Debug.Log("[Setup] SpawnPoints 생성 완료 (5개)");
            return spawnPoints;
        }

        // ====== CombatTestManager 생성 ======

        private static GameObject CreateCombatTestManager(GameObject player, GameObject enemyPrefab,
            Transform[] spawnPoints, EnemyData weakEnemy, EnemyData normalEnemy, EnemyData strongEnemy,
            SkillData fireball, SkillData iceBlast, SkillData heal, SkillData powerBuff)
        {
            GameObject managerObj = new GameObject("CombatTestManager");
            CombatTestManager manager = managerObj.AddComponent<CombatTestManager>();

            // SerializedObject로 필드 설정
            SerializedObject so = new SerializedObject(manager);

            so.FindProperty("playerObject").objectReferenceValue = player;
            so.FindProperty("playerStats").objectReferenceValue = player.GetComponent<PlayerStats>();
            so.FindProperty("weakEnemyData").objectReferenceValue = weakEnemy;
            so.FindProperty("normalEnemyData").objectReferenceValue = normalEnemy;
            so.FindProperty("strongEnemyData").objectReferenceValue = strongEnemy;
            so.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;

            // SpawnPoints 배열 설정
            SerializedProperty spawnProp = so.FindProperty("spawnPoints");
            spawnProp.arraySize = spawnPoints.Length;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                spawnProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
            }

            // TestSkills 리스트 설정
            SerializedProperty skillsProp = so.FindProperty("testSkills");
            skillsProp.arraySize = 4;
            skillsProp.GetArrayElementAtIndex(0).objectReferenceValue = fireball;
            skillsProp.GetArrayElementAtIndex(1).objectReferenceValue = iceBlast;
            skillsProp.GetArrayElementAtIndex(2).objectReferenceValue = heal;
            skillsProp.GetArrayElementAtIndex(3).objectReferenceValue = powerBuff;

            so.ApplyModifiedProperties();

            Debug.Log("[Setup] CombatTestManager 생성 및 설정 완료");
            return managerObj;
        }

        // ====== 카메라 설정 ======

        private static void SetupCamera(GameObject player)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            cameraObj.transform.position = new Vector3(0f, 0f, -10f);

            // 카메라가 플레이어를 따라가도록 설정 (간단한 Follow 스크립트)
            // SimpleCameraFollow 컴포넌트가 있다면 추가

            Debug.Log("[Setup] 카메라 설정 완료");
        }
    }
}
