using UnityEngine;
using UnityEditor;
using System.IO;
using GASPT.Data;
using GASPT.Gameplay.Enemy;
using GASPT.Gameplay.Projectiles;
using GASPT.UI;
using GASPT.Core.Pooling;
using Core.Enums;

namespace GASPT.Editor
{
    /// <summary>
    /// Phase C-2 보스 전투 테스트 환경 자동 생성 도구
    /// FireDragon 데이터, BossEnemy 프리팹, EnemyProjectile 프리팹 생성
    /// </summary>
    public class BossSetupCreator : EditorWindow
    {
        // ====== 경로 상수 ======

        private const string EnemyDataPath = "Assets/_Project/Data/Enemies";
        private const string PrefabsPath = "Assets/Resources/Prefabs";
        private const string EnemyPrefabsPath = "Assets/Resources/Prefabs/Enemies";
        private const string ProjectilePrefabsPath = "Assets/Resources/Prefabs/Projectiles";
        private const string UIPrefabsPath = "Assets/Resources/Prefabs/UI";


        // ====== 생성 옵션 ======

        private bool createFireDragonData = true;
        private bool createBossPrefab = true;
        private bool createEnemyProjectile = true;
        private bool setupPoolManager = false;

        private Vector2 scrollPosition;


        // ====== 메뉴 ======

        [MenuItem("Tools/GASPT/Boss Setup Creator")]
        public static void ShowWindow()
        {
            BossSetupCreator window = GetWindow<BossSetupCreator>("Boss Setup Creator");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }


        // ====== GUI ======

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("=== Phase C-2 Boss Setup Creator ===", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Phase C-2 보스 전투 테스트 환경을 자동으로 생성합니다.\n\n" +
                "생성될 에셋:\n" +
                "1. FireDragon.asset (EnemyData) - 보스 데이터\n" +
                "2. BossEnemy_FireDragon.prefab - 보스 프리팹\n" +
                "3. EnemyProjectile.prefab - 적 투사체\n" +
                "4. PoolManager 설정 (선택사항)\n\n" +
                "⚠️ 기존 파일이 있으면 덮어씁니다!",
                MessageType.Info
            );

            GUILayout.Space(20);

            // 생성 옵션
            EditorGUILayout.LabelField("생성할 에셋 선택:", EditorStyles.boldLabel);
            createFireDragonData = EditorGUILayout.Toggle("FireDragon.asset (EnemyData)", createFireDragonData);
            createBossPrefab = EditorGUILayout.Toggle("BossEnemy_FireDragon.prefab", createBossPrefab);
            createEnemyProjectile = EditorGUILayout.Toggle("EnemyProjectile.prefab", createEnemyProjectile);
            setupPoolManager = EditorGUILayout.Toggle("PoolManager 설정 (씬 필요)", setupPoolManager);

            GUILayout.Space(20);

            // 전체 생성 버튼
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🚀 모든 에셋 자동 생성", GUILayout.Height(50)))
            {
                CreateAllAssets();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // 개별 생성 버튼
            EditorGUILayout.LabelField("개별 생성:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. FireDragon.asset 생성"))
            {
                CreateFireDragonData();
            }

            if (GUILayout.Button("2. EnemyProjectile.prefab 생성"))
            {
                CreateEnemyProjectilePrefab();
            }

            if (GUILayout.Button("3. BossEnemy_FireDragon.prefab 생성"))
            {
                CreateBossEnemyPrefab();
            }

            if (GUILayout.Button("4. PoolManager 설정 (씬에서 실행)"))
            {
                SetupPoolManager();
            }

            GUILayout.Space(20);

            // 도움말
            EditorGUILayout.HelpBox(
                "생성 순서:\n" +
                "1. FireDragon.asset (보스 데이터)\n" +
                "2. EnemyProjectile.prefab (원거리 공격용)\n" +
                "3. BossEnemy_FireDragon.prefab (보스 프리팹)\n" +
                "4. PoolManager 설정 (씬 열어야 함)\n\n" +
                "전체 자동 생성 버튼을 누르면 순서대로 모두 생성됩니다.",
                MessageType.None
            );

            GUILayout.Space(10);

            // 폴더 열기
            if (GUILayout.Button("📁 생성된 프리팹 폴더 열기"))
            {
                EditorUtility.RevealInFinder(Path.Combine(Application.dataPath, "_Project/Prefabs/Enemies"));
            }

            EditorGUILayout.EndScrollView();
        }


        // ====== 전체 생성 ======

        private void CreateAllAssets()
        {
            Debug.Log("[BossSetupCreator] 보스 전투 환경 자동 생성 시작...");

            EnsureDirectoriesExist();

            int createdCount = 0;

            // 1. FireDragon 데이터 생성
            if (createFireDragonData)
            {
                if (CreateFireDragonData())
                    createdCount++;
            }

            // 2. EnemyProjectile 프리팹 생성
            if (createEnemyProjectile)
            {
                if (CreateEnemyProjectilePrefab())
                    createdCount++;
            }

            // 3. BossEnemy 프리팹 생성
            if (createBossPrefab)
            {
                if (CreateBossEnemyPrefab())
                    createdCount++;
            }

            // 4. PoolManager 설정
            if (setupPoolManager)
            {
                SetupPoolManager();
            }

            AssetDatabase.Refresh();

            Debug.Log($"[BossSetupCreator] ✅ 보스 전투 환경 생성 완료! ({createdCount}개 에셋 생성)");

            EditorUtility.DisplayDialog(
                "생성 완료",
                $"보스 전투 환경이 생성되었습니다!\n\n" +
                $"생성된 에셋: {createdCount}개\n\n" +
                "- FireDragon.asset\n" +
                "- EnemyProjectile.prefab\n" +
                "- BossEnemy_FireDragon.prefab\n\n" +
                "이제 GameplayScene에 BossEnemy_FireDragon을 배치하여 테스트하세요!",
                "확인"
            );
        }


        // ====== 1. FireDragon 데이터 생성 ======

        private bool CreateFireDragonData()
        {
            string assetPath = Path.Combine(EnemyDataPath, "FireDragon.asset");

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<EnemyData>(assetPath) != null)
            {
                Debug.LogWarning($"[BossSetupCreator] FireDragon.asset이 이미 존재합니다. 스킵합니다.");
                return false;
            }

            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();

            // 기본 정보
            data.enemyType = EnemyType.Boss;
            data.enemyClass = EnemyClass.Boss;
            data.enemyName = "화염 드래곤";

            // 스탯
            data.maxHp = 500;
            data.attack = 25;

            // 보상
            data.minGoldDrop = 200;
            data.maxGoldDrop = 300;
            data.expReward = 500;

            // 플랫포머 설정
            data.moveSpeed = 2f;
            data.detectionRange = 15f;
            data.attackRange = 2.5f;
            data.patrolDistance = 5f;
            data.chaseSpeed = 3.5f;
            data.attackCooldown = 1f;

            // 보스 전용 설정
            data.bossRangedCooldown = 3f;
            data.bossChargeCooldown = 5f;
            data.bossSummonCooldown = 10f;
            data.bossAreaCooldown = 7f;
            data.bossAreaRadius = 5f;
            data.bossProjectileSpeed = 10f;
            data.bossProjectileDamage = 20;
            data.bossAreaDamage = 40;
            data.bossChargeSpeed = 12f;
            data.bossChargeDistance = 10f;

            // UI
            data.showNameTag = false;
            data.showBossHealthBar = true;

            AssetDatabase.CreateAsset(data, assetPath);
            Debug.Log($"[BossSetupCreator] ✅ FireDragon.asset 생성: {assetPath}");

            return true;
        }


        // ====== 2. EnemyProjectile 프리팹 생성 ======

        private bool CreateEnemyProjectilePrefab()
        {
            string prefabPath = Path.Combine(ProjectilePrefabsPath, "EnemyProjectile.prefab");

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.LogWarning($"[BossSetupCreator] EnemyProjectile.prefab이 이미 존재합니다. 스킵합니다.");
                return false;
            }

            // GameObject 생성
            GameObject projectile = new GameObject("EnemyProjectile");

            // SpriteRenderer 추가
            SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = Color.red;

            // Transform 설정
            projectile.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            // CircleCollider2D 추가 (트리거)
            CircleCollider2D collider = projectile.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = true;

            // EnemyProjectile 스크립트 추가
            EnemyProjectile enemyProj = projectile.AddComponent<EnemyProjectile>();

            // PooledObject 스크립트 추가
            PooledObject pooledObject = projectile.AddComponent<PooledObject>();

            // Projectile 기본 설정 (SerializedObject 사용)
            SerializedObject so = new SerializedObject(enemyProj);
            so.FindProperty("speed").floatValue = 8f;
            so.FindProperty("maxDistance").floatValue = 20f;
            so.FindProperty("damage").floatValue = 15f;
            so.FindProperty("collisionRadius").floatValue = 0.3f;
            so.FindProperty("targetLayers").intValue = LayerMask.GetMask("Player");
            so.ApplyModifiedProperties();

            // 프리팹으로 저장
            PrefabUtility.SaveAsPrefabAsset(projectile, prefabPath);
            DestroyImmediate(projectile);

            Debug.Log($"[BossSetupCreator] ✅ EnemyProjectile.prefab 생성: {prefabPath}");

            return true;
        }


        // ====== 3. BossEnemy 프리팹 생성 ======

        private bool CreateBossEnemyPrefab()
        {
            string prefabPath = Path.Combine(EnemyPrefabsPath, "BossEnemy_FireDragon.prefab");

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.LogWarning($"[BossSetupCreator] BossEnemy_FireDragon.prefab이 이미 존재합니다. 스킵합니다.");
                return false;
            }

            // FireDragon 데이터 로드
            string dataPath = Path.Combine(EnemyDataPath, "FireDragon.asset");
            EnemyData fireDragonData = AssetDatabase.LoadAssetAtPath<EnemyData>(dataPath);

            if (fireDragonData == null)
            {
                Debug.LogError("[BossSetupCreator] FireDragon.asset을 찾을 수 없습니다! 먼저 FireDragon 데이터를 생성하세요.");
                return false;
            }

            // BossHealthBar 프리팹 로드
            string healthBarPath = Path.Combine(UIPrefabsPath, "BossHealthBar.prefab");
            BossHealthBar healthBarPrefab = AssetDatabase.LoadAssetAtPath<BossHealthBar>(healthBarPath);

            // BasicMeleeEnemy 프리팹 로드 (소환용)
            string minionPath = Path.Combine(EnemyPrefabsPath, "BasicMeleeEnemy.prefab");
            GameObject minionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(minionPath);

            // BasicMeleeEnemy 데이터 로드 (소환용 - 적절한 데이터로 변경 가능)
            // 여러 가능한 경로 시도 (TestGoblin, NormalGoblin 등)
            string[] possibleMinionDataPaths = new string[]
            {
                Path.Combine(EnemyDataPath, "TestGoblin.asset"),
                Path.Combine(EnemyDataPath, "Normal Goblin.asset"),
                Path.Combine(EnemyDataPath, "NormalGoblin.asset"),
                Path.Combine(EnemyDataPath, "BasicMeleeGoblin.asset")
            };

            EnemyData minionData = null;
            foreach (string path in possibleMinionDataPaths)
            {
                minionData = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                if (minionData != null)
                {
                    Debug.Log($"[BossSetupCreator] 미니언 데이터 로드 성공: {path}");
                    break;
                }
            }

            // GameObject 생성
            GameObject boss = new GameObject("BossEnemy_FireDragon");

            // SpriteRenderer 추가
            SpriteRenderer sr = boss.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = Color.red;
            sr.sortingOrder = 0;

            // Transform 설정
            boss.transform.localScale = new Vector3(3f, 3f, 1f);

            // Layer 설정
            boss.layer = LayerMask.NameToLayer("Enemy");

            // Rigidbody2D 추가
            Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // CapsuleCollider2D 추가
            CapsuleCollider2D collider = boss.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(1f, 2f);
            collider.direction = CapsuleDirection2D.Vertical;

            // BossEnemy 스크립트 추가
            BossEnemy bossEnemy = boss.AddComponent<BossEnemy>();

            // BossEnemy 설정 (SerializedObject 사용)
            SerializedObject so = new SerializedObject(bossEnemy);

            // Data 할당
            so.FindProperty("enemyData").objectReferenceValue = fireDragonData;

            // BossHealthBar 할당
            if (healthBarPrefab != null)
            {
                so.FindProperty("bossHealthBarPrefab").objectReferenceValue = healthBarPrefab;
            }
            else
            {
                Debug.LogWarning("[BossSetupCreator] BossHealthBar.prefab을 찾을 수 없습니다.");
            }

            // Minion 할당
            if (minionPrefab != null)
            {
                so.FindProperty("minionPrefab").objectReferenceValue = minionPrefab;
            }
            else
            {
                Debug.LogWarning("[BossSetupCreator] BasicMeleeEnemy.prefab을 찾을 수 없습니다.");
            }

            // Minion Data 할당
            if (minionData != null)
            {
                so.FindProperty("minionData").objectReferenceValue = minionData;
                Debug.Log($"[BossSetupCreator] ✅ Minion Data 할당: {minionData.enemyName}");
            }
            else
            {
                Debug.LogWarning("[BossSetupCreator] ⚠️ Minion EnemyData를 찾을 수 없습니다. Inspector에서 수동으로 설정하세요.");
            }

            // maxSummonCount
            so.FindProperty("maxSummonCount").intValue = 3;

            // showDebugLogs
            so.FindProperty("showDebugLogs").boolValue = true;

            // showGizmos
            so.FindProperty("showGizmos").boolValue = true;

            so.ApplyModifiedProperties();

            // 프리팹으로 저장
            PrefabUtility.SaveAsPrefabAsset(boss, prefabPath);
            DestroyImmediate(boss);

            Debug.Log($"[BossSetupCreator] ✅ BossEnemy_FireDragon.prefab 생성: {prefabPath}");

            return true;
        }


        // ====== 4. PoolManager 설정 ======

        private void SetupPoolManager()
        {
            // 씬에서 PoolManager 찾기
            PoolManager poolManager = FindAnyObjectByType<PoolManager>();

            if (poolManager == null)
            {
                Debug.LogWarning("[BossSetupCreator] PoolManager를 찾을 수 없습니다. GameplayScene을 열어주세요.");
                EditorUtility.DisplayDialog(
                    "PoolManager 없음",
                    "PoolManager를 찾을 수 없습니다.\n\n" +
                    "GameplayScene을 열고 다시 시도하세요.",
                    "확인"
                );
                return;
            }

            // EnemyProjectile 프리팹 로드
            string projectilePath = Path.Combine(ProjectilePrefabsPath, "EnemyProjectile.prefab");
            GameObject projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projectilePath);

            if (projectilePrefab == null)
            {
                Debug.LogError("[BossSetupCreator] EnemyProjectile.prefab을 찾을 수 없습니다!");
                return;
            }

            // PoolManager 설정 (SerializedObject 사용)
            SerializedObject so = new SerializedObject(poolManager);
            SerializedProperty poolsProp = so.FindProperty("prefabPools");

            // EnemyProjectile이 이미 등록되어 있는지 확인
            bool alreadyExists = false;
            for (int i = 0; i < poolsProp.arraySize; i++)
            {
                SerializedProperty poolProp = poolsProp.GetArrayElementAtIndex(i);
                SerializedProperty prefabProp = poolProp.FindPropertyRelative("prefab");

                if (prefabProp.objectReferenceValue == projectilePrefab)
                {
                    alreadyExists = true;
                    Debug.Log("[BossSetupCreator] EnemyProjectile이 이미 PoolManager에 등록되어 있습니다.");
                    break;
                }
            }

            if (!alreadyExists)
            {
                // 새 Pool 추가
                poolsProp.InsertArrayElementAtIndex(poolsProp.arraySize);
                SerializedProperty newPoolProp = poolsProp.GetArrayElementAtIndex(poolsProp.arraySize - 1);

                newPoolProp.FindPropertyRelative("prefab").objectReferenceValue = projectilePrefab;
                newPoolProp.FindPropertyRelative("initialSize").intValue = 10;
                newPoolProp.FindPropertyRelative("maxSize").intValue = 50;

                so.ApplyModifiedProperties();

                Debug.Log("[BossSetupCreator] ✅ EnemyProjectile이 PoolManager에 등록되었습니다.");

                // 씬 저장
                EditorUtility.SetDirty(poolManager);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
                );
            }
        }


        // ====== 유틸리티 ======

        private void EnsureDirectoriesExist()
        {
            // EnemyData 폴더
            if (!AssetDatabase.IsValidFolder(EnemyDataPath))
            {
                CreateFolderRecursive(EnemyDataPath);
            }

            // Prefabs 폴더들
            if (!AssetDatabase.IsValidFolder(EnemyPrefabsPath))
            {
                CreateFolderRecursive(EnemyPrefabsPath);
            }

            if (!AssetDatabase.IsValidFolder(ProjectilePrefabsPath))
            {
                CreateFolderRecursive(ProjectilePrefabsPath);
            }
        }

        private void CreateFolderRecursive(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = currentPath + "/" + folders[i];

                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                    Debug.Log($"[BossSetupCreator] 폴더 생성: {nextPath}");
                }

                currentPath = nextPath;
            }
        }
    }
}
