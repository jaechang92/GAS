using UnityEngine;
using UnityEditor;
using GASPT.Data;
using GASPT.Core.Enums;
using System.Collections.Generic;

namespace GASPT.Editor
{
    /// <summary>
    /// 몬스터 에셋 자동 생성 도구
    /// spec.md의 몬스터 정의를 기반으로 EnemyData 에셋을 일괄 생성
    /// </summary>
    public class MonsterAssetGenerator : EditorWindow
    {
        private const string NORMAL_PATH = "Assets/Resources/Data/Enemies/Normal/";
        private const string NAMED_PATH = "Assets/Resources/Data/Enemies/Named/";
        private const string BOSS_PATH = "Assets/Resources/Data/Enemies/Boss/";

        private Vector2 scrollPos;
        private bool showNormal = true;
        private bool showNamed = true;
        private bool showBoss = true;

        [MenuItem("GASPT/Monsters/Generate All Monster Assets", false, 100)]
        public static void GenerateAllMonsters()
        {
            EnsureFolders();

            int created = 0;
            created += GenerateNormalMonsters();
            created += GenerateNamedMonsters();
            created += GenerateBossMonsters();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("완료",
                $"몬스터 에셋 {created}개 생성 완료!\n\n" +
                $"경로:\n" +
                $"- Normal: {NORMAL_PATH}\n" +
                $"- Named: {NAMED_PATH}\n" +
                $"- Boss: {BOSS_PATH}",
                "확인");
        }

        [MenuItem("GASPT/Monsters/Generate Normal Monsters Only", false, 101)]
        public static void GenerateNormalOnly()
        {
            EnsureFolders();
            int created = GenerateNormalMonsters();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MonsterAssetGenerator] Normal 몬스터 {created}개 생성 완료");
        }

        [MenuItem("GASPT/Monsters/Generate Named Monsters Only", false, 102)]
        public static void GenerateNamedOnly()
        {
            EnsureFolders();
            int created = GenerateNamedMonsters();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MonsterAssetGenerator] Named 몬스터 {created}개 생성 완료");
        }

        [MenuItem("GASPT/Monsters/Generate Boss Monsters Only", false, 103)]
        public static void GenerateBossOnly()
        {
            EnsureFolders();
            int created = GenerateBossMonsters();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MonsterAssetGenerator] Boss 몬스터 {created}개 생성 완료");
        }

        [MenuItem("GASPT/Monsters/Monster Asset Generator Window", false, 200)]
        public static void ShowWindow()
        {
            GetWindow<MonsterAssetGenerator>("Monster Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("몬스터 에셋 생성 도구", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("전체 몬스터 생성 (37종)", GUILayout.Height(40)))
            {
                GenerateAllMonsters();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Normal Monsters
            showNormal = EditorGUILayout.Foldout(showNormal, $"Normal 몬스터 ({GetNormalMonsterCount()}종)");
            if (showNormal)
            {
                EditorGUI.indentLevel++;
                ShowNormalMonsterList();
                if (GUILayout.Button("Normal 몬스터만 생성"))
                {
                    GenerateNormalOnly();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Named Monsters
            showNamed = EditorGUILayout.Foldout(showNamed, $"Named 몬스터 ({GetNamedMonsterCount()}종)");
            if (showNamed)
            {
                EditorGUI.indentLevel++;
                ShowNamedMonsterList();
                if (GUILayout.Button("Named 몬스터만 생성"))
                {
                    GenerateNamedOnly();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Boss Monsters
            showBoss = EditorGUILayout.Foldout(showBoss, $"Boss 몬스터 ({GetBossMonsterCount()}종)");
            if (showBoss)
            {
                EditorGUI.indentLevel++;
                ShowBossMonsterList();
                if (GUILayout.Button("Boss 몬스터만 생성"))
                {
                    GenerateBossOnly();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndScrollView();
        }

        private void ShowNormalMonsterList()
        {
            EditorGUILayout.LabelField("BasicMelee: M001-M005 (5종)");
            EditorGUILayout.LabelField("Ranged: M010-M014 (5종)");
            EditorGUILayout.LabelField("Flying: M020-M024 (5종)");
        }

        private void ShowNamedMonsterList()
        {
            EditorGUILayout.LabelField("Elite: N001-N008 (8종)");
            EditorGUILayout.LabelField("강화형: N010-N011 (2종)");
        }

        private void ShowBossMonsterList()
        {
            EditorGUILayout.LabelField("MiniBoss: B001-B003 (3종)");
            EditorGUILayout.LabelField("MidBoss: B004-B005 (2종)");
            EditorGUILayout.LabelField("FinalBoss: B006 (1종)");
        }

        private int GetNormalMonsterCount() => 15;
        private int GetNamedMonsterCount() => 10;
        private int GetBossMonsterCount() => 6;

        // ====== 폴더 생성 ======
        private static void EnsureFolders()
        {
            CreateFolderIfNotExists("Assets/Resources");
            CreateFolderIfNotExists("Assets/Resources/Data");
            CreateFolderIfNotExists("Assets/Resources/Data/Enemies");
            CreateFolderIfNotExists("Assets/Resources/Data/Enemies/Normal");
            CreateFolderIfNotExists("Assets/Resources/Data/Enemies/Named");
            CreateFolderIfNotExists("Assets/Resources/Data/Enemies/Boss");
        }

        private static void CreateFolderIfNotExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        // ====== Normal 몬스터 생성 ======
        private static int GenerateNormalMonsters()
        {
            int count = 0;

            // BasicMelee (5종)
            count += CreateEnemy("M001_ForestGolem", "숲 골렘", EnemyType.Normal, EnemyClass.BasicMelee,
                ElementType.Earth, 1, 35, 6, 15, 20, 10, 0, 1, NORMAL_PATH);

            count += CreateEnemy("M002_SkeletonWarrior", "스켈레톤 워리어", EnemyType.Normal, EnemyClass.BasicMelee,
                ElementType.None, 1, 25, 7, 18, 25, 12, 1, 2, NORMAL_PATH,
                additionalStages: new[] { 2 });

            count += CreateEnemy("M003_FlameImp", "화염 임프", EnemyType.Normal, EnemyClass.BasicMelee,
                ElementType.Fire, 2, 22, 8, 20, 28, 14, 0, 1, NORMAL_PATH);

            count += CreateEnemy("M004_FrozenCorpse", "빙결 시체", EnemyType.Normal, EnemyClass.BasicMelee,
                ElementType.Ice, 3, 40, 6, 22, 30, 15, 1, 2, NORMAL_PATH);

            count += CreateEnemy("M005_VenomousSpider", "독거미", EnemyType.Normal, EnemyClass.BasicMelee,
                ElementType.Poison, 4, 28, 5, 25, 35, 18, 0, 1, NORMAL_PATH);

            // Ranged (5종)
            count += CreateEnemy("M010_SkeletonArcher", "해골 궁수", EnemyType.Normal, EnemyClass.Ranged,
                ElementType.None, 1, 20, 6, 18, 25, 12, 1, 2, NORMAL_PATH,
                additionalStages: new[] { 2 });

            count += CreateEnemy("M011_FlameMage", "화염 마법사", EnemyType.Normal, EnemyClass.Ranged,
                ElementType.Fire, 2, 18, 9, 22, 32, 16, 0, 1, NORMAL_PATH);

            count += CreateEnemy("M012_IceMage", "얼음 마법사", EnemyType.Normal, EnemyClass.Ranged,
                ElementType.Ice, 3, 22, 7, 25, 35, 18, 0, 1, NORMAL_PATH);

            count += CreateEnemy("M013_PoisonDartTribesman", "독침 부족", EnemyType.Normal, EnemyClass.Ranged,
                ElementType.Poison, 4, 24, 4, 28, 38, 20, 1, 2, NORMAL_PATH);

            count += CreateEnemy("M014_DarkCaster", "어둠 술사", EnemyType.Normal, EnemyClass.Ranged,
                ElementType.Dark, 5, 26, 10, 32, 45, 24, 1, 2, NORMAL_PATH);

            // Flying (5종)
            count += CreateEnemy("M020_ForestBat", "숲 박쥐", EnemyType.Normal, EnemyClass.Flying,
                ElementType.None, 1, 15, 5, 12, 18, 8, 0, 1, NORMAL_PATH);

            count += CreateEnemy("M021_FireElemental", "화염 정령", EnemyType.Normal, EnemyClass.Flying,
                ElementType.Fire, 2, 20, 7, 20, 28, 14, 0, 0, NORMAL_PATH);

            count += CreateEnemy("M022_IceFairy", "얼음 요정", EnemyType.Normal, EnemyClass.Flying,
                ElementType.Ice, 3, 18, 6, 22, 32, 16, 0, 0, NORMAL_PATH);

            count += CreateEnemy("M023_MiasmaSpirit", "독안개 정령", EnemyType.Normal, EnemyClass.Flying,
                ElementType.Poison, 4, 25, 5, 26, 36, 18, 0, 1, NORMAL_PATH);

            count += CreateEnemy("M024_DarkRaven", "어둠의 까마귀", EnemyType.Normal, EnemyClass.Flying,
                ElementType.Dark, 5, 22, 9, 30, 42, 22, 1, 2, NORMAL_PATH);

            return count;
        }

        // ====== Named 몬스터 생성 ======
        private static int GenerateNamedMonsters()
        {
            int count = 0;

            // Elite (8종)
            count += CreateEnemy("N001_AncientForestGuardian", "고대 숲의 수호자", EnemyType.Named, EnemyClass.Elite,
                ElementType.Earth, 1, 80, 12, 50, 70, 40, 2, 4, NAMED_PATH, showNameTag: true);

            count += CreateEnemy("N002_BoneKnightCaptain", "뼈 기사단장", EnemyType.Named, EnemyClass.Elite,
                ElementType.None, 1, 70, 14, 55, 75, 45, 3, 5, NAMED_PATH, showNameTag: true,
                additionalStages: new[] { 2 });

            count += CreateEnemy("N003_FlameWitch", "화염의 마녀", EnemyType.Named, EnemyClass.Elite,
                ElementType.Fire, 2, 55, 16, 60, 85, 50, 1, 3, NAMED_PATH, showNameTag: true);

            count += CreateEnemy("N004_FrostKnight", "얼음 기사", EnemyType.Named, EnemyClass.Elite,
                ElementType.Ice, 3, 90, 13, 65, 90, 55, 2, 4, NAMED_PATH, showNameTag: true);

            count += CreateEnemy("N005_ThunderSpiritKing", "번개 정령왕", EnemyType.Named, EnemyClass.Elite,
                ElementType.Thunder, 3, 65, 18, 70, 95, 60, 1, 3, NAMED_PATH, showNameTag: true);

            count += CreateEnemy("N006_PoisonQueen", "독의 여왕", EnemyType.Named, EnemyClass.Elite,
                ElementType.Poison, 4, 75, 10, 75, 100, 65, 2, 4, NAMED_PATH, showNameTag: true);

            count += CreateEnemy("N007_ShadowAssassin", "그림자 암살자", EnemyType.Named, EnemyClass.Elite,
                ElementType.Dark, 4, 50, 22, 80, 110, 70, 3, 5, NAMED_PATH, showNameTag: true,
                additionalStages: new[] { 5 });

            count += CreateEnemy("N008_LightInquisitor", "빛의 심판관", EnemyType.Named, EnemyClass.Elite,
                ElementType.Light, 5, 100, 15, 90, 120, 80, 2, 4, NAMED_PATH, showNameTag: true);

            // 강화형 (2종)
            count += CreateEnemy("N010_MagmaGolem", "용암 골렘", EnemyType.Named, EnemyClass.BasicMelee,
                ElementType.Fire, 2, 100, 14, 55, 80, 45, 3, 5, NAMED_PATH, showNameTag: true);

            count += CreateEnemy("N011_GlacierGiant", "빙하 거인", EnemyType.Named, EnemyClass.BasicMelee,
                ElementType.Ice, 3, 120, 12, 65, 90, 55, 2, 4, NAMED_PATH, showNameTag: true);

            return count;
        }

        // ====== Boss 몬스터 생성 ======
        private static int GenerateBossMonsters()
        {
            int count = 0;

            // MiniBoss (3종)
            count += CreateBoss("B001_FlameGolem", "불꽃 골렘", BossGrade.MiniBoss,
                ElementType.Fire, 1, 500, 15, 100, 150, 80, 5, 8, 1);

            count += CreateBoss("B002_FrostSpirit", "얼음 정령", BossGrade.MiniBoss,
                ElementType.Ice, 2, 600, 12, 120, 170, 100, 6, 10, 1);

            count += CreateBoss("B003_SwampQueenSpider", "독 늪의 여왕 거미", BossGrade.MiniBoss,
                ElementType.Poison, 4, 550, 14, 140, 190, 110, 7, 12, 2);

            // MidBoss (2종)
            count += CreateBoss("B004_ThunderDragon", "번개 드래곤", BossGrade.MidBoss,
                ElementType.Thunder, 3, 1200, 25, 300, 400, 200, 10, 15, 3);

            count += CreateBoss("B005_ShadowLord", "그림자 군주", BossGrade.MidBoss,
                ElementType.Dark, 5, 1000, 28, 350, 450, 220, 12, 18, 4);

            // FinalBoss (1종)
            count += CreateBoss("B006_DarkArchmage", "어둠의 대마법사", BossGrade.FinalBoss,
                ElementType.Dark, 5, 2500, 40, 800, 1200, 500, 20, 30, 10);

            return count;
        }

        // ====== 에셋 생성 헬퍼 ======
        private static int CreateEnemy(string fileName, string enemyName, EnemyType type, EnemyClass enemyClass,
            ElementType element, int stage, int hp, int attack, int minGold, int maxGold, int exp,
            int minBone, int maxBone, string path, bool showNameTag = false, int[] additionalStages = null)
        {
            string fullPath = path + fileName + ".asset";

            // 기존 에셋 체크
            var existing = AssetDatabase.LoadAssetAtPath<EnemyData>(fullPath);
            if (existing != null)
            {
                UpdateExistingEnemy(existing, enemyName, type, enemyClass, element, stage,
                    hp, attack, minGold, maxGold, exp, minBone, maxBone, showNameTag, additionalStages);
                return 0; // 업데이트만
            }

            // 새 에셋 생성
            var data = ScriptableObject.CreateInstance<EnemyData>();
            data.enemyName = enemyName;
            data.enemyType = type;
            data.enemyClass = enemyClass;
            data.elementType = element;
            data.stageAppearance = stage;
            data.additionalStages = additionalStages;
            data.maxHp = hp;
            data.attack = attack;
            data.minGoldDrop = minGold;
            data.maxGoldDrop = maxGold;
            data.expReward = exp;
            data.minBoneDrop = minBone;
            data.maxBoneDrop = maxBone;
            data.showNameTag = showNameTag;

            // 클래스별 기본값 설정
            SetDefaultsByClass(data, enemyClass);

            AssetDatabase.CreateAsset(data, fullPath);
            Debug.Log($"[MonsterAssetGenerator] 생성: {fullPath}");
            return 1;
        }

        private static int CreateBoss(string fileName, string enemyName, BossGrade grade,
            ElementType element, int stage, int hp, int attack, int minGold, int maxGold, int exp,
            int minBone, int maxBone, int soul)
        {
            string fullPath = BOSS_PATH + fileName + ".asset";

            var existing = AssetDatabase.LoadAssetAtPath<EnemyData>(fullPath);
            if (existing != null)
            {
                // 업데이트
                existing.enemyName = enemyName;
                existing.enemyType = EnemyType.Boss;
                existing.enemyClass = EnemyClass.Boss;
                existing.bossGrade = grade;
                existing.elementType = element;
                existing.stageAppearance = stage;
                existing.maxHp = hp;
                existing.attack = attack;
                existing.minGoldDrop = minGold;
                existing.maxGoldDrop = maxGold;
                existing.expReward = exp;
                existing.minBoneDrop = minBone;
                existing.maxBoneDrop = maxBone;
                existing.soulDrop = soul;
                existing.showBossHealthBar = true;
                EditorUtility.SetDirty(existing);
                return 0;
            }

            var data = ScriptableObject.CreateInstance<EnemyData>();
            data.enemyName = enemyName;
            data.enemyType = EnemyType.Boss;
            data.enemyClass = EnemyClass.Boss;
            data.bossGrade = grade;
            data.elementType = element;
            data.stageAppearance = stage;
            data.maxHp = hp;
            data.attack = attack;
            data.minGoldDrop = minGold;
            data.maxGoldDrop = maxGold;
            data.expReward = exp;
            data.minBoneDrop = minBone;
            data.maxBoneDrop = maxBone;
            data.soulDrop = soul;
            data.showBossHealthBar = true;

            // 보스 기본값 설정
            SetBossDefaults(data, grade);

            AssetDatabase.CreateAsset(data, fullPath);
            Debug.Log($"[MonsterAssetGenerator] 보스 생성: {fullPath}");
            return 1;
        }

        private static void UpdateExistingEnemy(EnemyData data, string enemyName, EnemyType type, EnemyClass enemyClass,
            ElementType element, int stage, int hp, int attack, int minGold, int maxGold, int exp,
            int minBone, int maxBone, bool showNameTag, int[] additionalStages)
        {
            data.enemyName = enemyName;
            data.enemyType = type;
            data.enemyClass = enemyClass;
            data.elementType = element;
            data.stageAppearance = stage;
            data.additionalStages = additionalStages;
            data.maxHp = hp;
            data.attack = attack;
            data.minGoldDrop = minGold;
            data.maxGoldDrop = maxGold;
            data.expReward = exp;
            data.minBoneDrop = minBone;
            data.maxBoneDrop = maxBone;
            data.showNameTag = showNameTag;
            EditorUtility.SetDirty(data);
            Debug.Log($"[MonsterAssetGenerator] 업데이트: {data.enemyName}");
        }

        private static void SetDefaultsByClass(EnemyData data, EnemyClass enemyClass)
        {
            switch (enemyClass)
            {
                case EnemyClass.BasicMelee:
                    data.moveSpeed = 2f;
                    data.detectionRange = 5f;
                    data.attackRange = 1.5f;
                    data.patrolDistance = 3f;
                    data.chaseSpeed = 3f;
                    data.attackCooldown = 1.5f;
                    break;

                case EnemyClass.Ranged:
                    data.moveSpeed = 1.5f;
                    data.detectionRange = 8f;
                    data.attackRange = 1f;
                    data.optimalAttackDistance = 10f;
                    data.minDistance = 5f;
                    data.attackCooldown = 3f;
                    break;

                case EnemyClass.Flying:
                    data.moveSpeed = 3f;
                    data.detectionRange = 7f;
                    data.flyHeight = 5f;
                    data.flySpeed = 3.5f;
                    data.diveSpeed = 10f;
                    data.attackCooldown = 2f;
                    break;

                case EnemyClass.Elite:
                    data.moveSpeed = 2f;
                    data.detectionRange = 8f;
                    data.attackRange = 2f;
                    data.chargeCooldown = 5f;
                    data.areaCooldown = 7f;
                    data.areaAttackRadius = 3f;
                    data.chargeSpeed = 8f;
                    data.chargeDistance = 5f;
                    data.attackCooldown = 2f;
                    break;
            }
        }

        private static void SetBossDefaults(EnemyData data, BossGrade grade)
        {
            data.moveSpeed = 2f;
            data.detectionRange = 15f;
            data.attackRange = 2.5f;

            switch (grade)
            {
                case BossGrade.MiniBoss:
                    data.bossRangedCooldown = 3f;
                    data.bossChargeCooldown = 5f;
                    data.bossAreaCooldown = 8f;
                    data.bossAreaRadius = 3f;
                    break;

                case BossGrade.MidBoss:
                    data.bossRangedCooldown = 4f;
                    data.bossChargeCooldown = 6f;
                    data.bossSummonCooldown = 12f;
                    data.bossAreaCooldown = 10f;
                    data.bossAreaRadius = 5f;
                    break;

                case BossGrade.FinalBoss:
                    data.bossRangedCooldown = 2f;
                    data.bossChargeCooldown = 4f;
                    data.bossSummonCooldown = 15f;
                    data.bossAreaCooldown = 7f;
                    data.bossAreaRadius = 6f;
                    data.bossProjectileSpeed = 10f;
                    data.bossProjectileDamage = 30;
                    data.bossAreaDamage = 50;
                    break;
            }
        }
    }
}
