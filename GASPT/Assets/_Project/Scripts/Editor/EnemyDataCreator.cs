using UnityEngine;
using UnityEditor;
using System.IO;
using GASPT.Data;
using Core.Enums;

namespace GASPT.Editor
{
    /// <summary>
    /// EnemyData 에셋 자동 생성 에디터 도구
    /// Phase C-1에서 추가된 3가지 적 타입의 데이터 에셋 생성
    /// </summary>
    public class EnemyDataCreator : EditorWindow
    {
        private const string EnemyDataPath = "Assets/_Project/Data/Enemies";

        [MenuItem("Tools/GASPT/Enemy Data Creator")]
        public static void ShowWindow()
        {
            EnemyDataCreator window = GetWindow<EnemyDataCreator>("Enemy Data Creator");
            window.minSize = new Vector2(450, 400);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("=== GASPT Enemy Data Creator ===", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Phase C-1, C-2 적 타입 데이터 에셋을 자동으로 생성합니다.\n\n" +
                "생성 위치: Assets/_Project/Data/Enemies/\n\n" +
                "생성될 에셋:\n" +
                "1. RangedGoblin (원거리 공격 고블린)\n" +
                "2. FlyingBat (비행 박쥐)\n" +
                "3. EliteOrc (정예 오크)\n" +
                "4. FireDragon (보스 - Phase C-2) ⭐\n" +
                "   - 3단계 Phase 전투 시스템\n" +
                "   - Phase별 고유 패턴 공격",
                MessageType.Info
            );

            GUILayout.Space(20);

            // 전체 생성 버튼
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🎯 모든 EnemyData 에셋 생성", GUILayout.Height(40)))
            {
                CreateAllEnemyData();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // 개별 생성 버튼들
            EditorGUILayout.LabelField("개별 생성:", EditorStyles.boldLabel);

            if (GUILayout.Button("RangedGoblin 데이터 생성"))
            {
                CreateRangedGoblinData();
            }

            if (GUILayout.Button("FlyingBat 데이터 생성"))
            {
                CreateFlyingBatData();
            }

            if (GUILayout.Button("EliteOrc 데이터 생성"))
            {
                CreateEliteOrcData();
            }

            GUILayout.Space(10);

            // Phase C-2: 보스 생성
            EditorGUILayout.LabelField("Phase C-2 보스:", EditorStyles.boldLabel);
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("⭐ FireDragon 데이터 생성 (보스)", GUILayout.Height(30)))
            {
                CreateFireDragonData();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(20);

            // 기존 에셋 확인 버튼
            if (GUILayout.Button("📁 생성된 에셋 폴더 열기"))
            {
                string path = Path.Combine(Application.dataPath, "_Project/Data/Enemies");
                if (Directory.Exists(path))
                {
                    EditorUtility.RevealInFinder(path);
                }
                else
                {
                    Debug.LogWarning($"[EnemyDataCreator] 폴더가 존재하지 않습니다: {path}");
                }
            }
        }


        // ====== 전체 생성 ======

        private void CreateAllEnemyData()
        {
            Debug.Log("[EnemyDataCreator] 모든 EnemyData 에셋 생성 시작...");

            EnsureDirectoryExists();

            CreateRangedGoblinData();
            CreateFlyingBatData();
            CreateEliteOrcData();
            CreateFireDragonData();

            AssetDatabase.Refresh();

            Debug.Log("[EnemyDataCreator] ✅ 모든 EnemyData 에셋 생성 완료!");
            EditorUtility.DisplayDialog(
                "생성 완료",
                "4개의 EnemyData 에셋이 생성되었습니다!\n\n" +
                "- RangedGoblin.asset\n" +
                "- FlyingBat.asset\n" +
                "- EliteOrc.asset\n" +
                "- FireDragon.asset (보스) ⭐\n\n" +
                $"위치: {EnemyDataPath}",
                "확인"
            );
        }


        // ====== 개별 생성 메서드 ======

        /// <summary>
        /// RangedGoblin 데이터 생성 (원거리 공격 적)
        /// </summary>
        private void CreateRangedGoblinData()
        {
            string assetPath = Path.Combine(EnemyDataPath, "RangedGoblin.asset");

            // 이미 존재하면 경고
            if (AssetDatabase.LoadAssetAtPath<EnemyData>(assetPath) != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "덮어쓰기 확인",
                    "RangedGoblin.asset이 이미 존재합니다.\n덮어쓰시겠습니까?",
                    "덮어쓰기",
                    "취소"))
                {
                    return;
                }
            }

            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();

            // ====== 기본 정보 ======
            data.enemyType = EnemyType.Normal;
            data.enemyClass = EnemyClass.Ranged;
            data.enemyName = "원거리 고블린";

            // ====== 스탯 ======
            data.maxHp = 25;
            data.attack = 7;

            // ====== 보상 ======
            data.minGoldDrop = 10;
            data.maxGoldDrop = 20;
            data.expReward = 15;

            // ====== 플랫포머 설정 ======
            data.moveSpeed = 1.5f;
            data.detectionRange = 12f;
            data.attackRange = 2f;
            data.patrolDistance = 4f;
            data.chaseSpeed = 2f;
            data.attackCooldown = 2f;

            // ====== 원거리 적 설정 ======
            data.optimalAttackDistance = 8f;
            data.minDistance = 4f;

            // ====== UI ======
            data.showNameTag = false;
            data.showBossHealthBar = false;

            AssetDatabase.CreateAsset(data, assetPath);
            Debug.Log($"[EnemyDataCreator] ✅ RangedGoblin 생성: {assetPath}");
        }

        /// <summary>
        /// FlyingBat 데이터 생성 (비행 적)
        /// </summary>
        private void CreateFlyingBatData()
        {
            string assetPath = Path.Combine(EnemyDataPath, "FlyingBat.asset");

            // 이미 존재하면 경고
            if (AssetDatabase.LoadAssetAtPath<EnemyData>(assetPath) != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "덮어쓰기 확인",
                    "FlyingBat.asset이 이미 존재합니다.\n덮어쓰시겠습니까?",
                    "덮어쓰기",
                    "취소"))
                {
                    return;
                }
            }

            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();

            // ====== 기본 정보 ======
            data.enemyType = EnemyType.Normal;
            data.enemyClass = EnemyClass.Flying;
            data.enemyName = "비행 박쥐";

            // ====== 스탯 ======
            data.maxHp = 20;
            data.attack = 8;

            // ====== 보상 ======
            data.minGoldDrop = 12;
            data.maxGoldDrop = 18;
            data.expReward = 18;

            // ====== 플랫포머 설정 ======
            data.moveSpeed = 2f;
            data.detectionRange = 10f;
            data.attackRange = 1.5f;
            data.patrolDistance = 5f;
            data.chaseSpeed = 3f;
            data.attackCooldown = 1.5f;

            // ====== 비행 적 설정 ======
            data.flyHeight = 5f;
            data.diveSpeed = 8f;
            data.flySpeed = 2.5f;

            // ====== UI ======
            data.showNameTag = false;
            data.showBossHealthBar = false;

            AssetDatabase.CreateAsset(data, assetPath);
            Debug.Log($"[EnemyDataCreator] ✅ FlyingBat 생성: {assetPath}");
        }

        /// <summary>
        /// EliteOrc 데이터 생성 (정예 적)
        /// </summary>
        private void CreateEliteOrcData()
        {
            string assetPath = Path.Combine(EnemyDataPath, "EliteOrc.asset");

            // 이미 존재하면 경고
            if (AssetDatabase.LoadAssetAtPath<EnemyData>(assetPath) != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "덮어쓰기 확인",
                    "EliteOrc.asset이 이미 존재합니다.\n덮어쓰시겠습니까?",
                    "덮어쓰기",
                    "취소"))
                {
                    return;
                }
            }

            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();

            // ====== 기본 정보 ======
            data.enemyType = EnemyType.Named;
            data.enemyClass = EnemyClass.Elite;
            data.enemyName = "정예 오크";

            // ====== 스탯 ======
            data.maxHp = 80;
            data.attack = 15;

            // ====== 보상 ======
            data.minGoldDrop = 40;
            data.maxGoldDrop = 60;
            data.expReward = 50;

            // ====== 플랫포머 설정 ======
            data.moveSpeed = 1.8f;
            data.detectionRange = 8f;
            data.attackRange = 2f;
            data.patrolDistance = 3f;
            data.chaseSpeed = 3f;
            data.attackCooldown = 1.2f;

            // ====== 정예 적 스킬 설정 ======
            data.chargeCooldown = 6f;
            data.areaCooldown = 8f;
            data.areaAttackRadius = 3.5f;
            data.chargeSpeed = 10f;
            data.chargeDistance = 6f;

            // ====== UI ======
            data.showNameTag = true;
            data.showBossHealthBar = false;

            AssetDatabase.CreateAsset(data, assetPath);
            Debug.Log($"[EnemyDataCreator] ✅ EliteOrc 생성: {assetPath}");
        }

        /// <summary>
        /// FireDragon 데이터 생성 (보스)
        /// Phase C-2: 3단계 보스 전투 시스템
        /// </summary>
        private void CreateFireDragonData()
        {
            string assetPath = Path.Combine(EnemyDataPath, "FireDragon.asset");

            // 이미 존재하면 경고
            if (AssetDatabase.LoadAssetAtPath<EnemyData>(assetPath) != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "덮어쓰기 확인",
                    "FireDragon.asset이 이미 존재합니다.\n덮어쓰시겠습니까?",
                    "덮어쓰기",
                    "취소"))
                {
                    return;
                }
            }

            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();

            // ====== 기본 정보 ======
            data.enemyType = EnemyType.Boss;
            data.enemyClass = EnemyClass.Boss;
            data.enemyName = "화염 드래곤";

            // ====== 스탯 ======
            data.maxHp = 500;
            data.attack = 25;

            // ====== 보상 ======
            data.minGoldDrop = 200;
            data.maxGoldDrop = 300;
            data.expReward = 500;

            // ====== 플랫포머 설정 ======
            data.moveSpeed = 2f;
            data.detectionRange = 15f;
            data.attackRange = 2.5f;
            data.patrolDistance = 5f;
            data.chaseSpeed = 3.5f;
            data.attackCooldown = 1f;

            // ====== 보스 전용 설정 ======
            data.bossRangedCooldown = 3f;          // Phase 1 원거리 공격
            data.bossChargeCooldown = 5f;          // Phase 2 돌진 공격
            data.bossSummonCooldown = 10f;         // Phase 2 소환
            data.bossAreaCooldown = 7f;            // Phase 3 범위 공격
            data.bossAreaRadius = 5f;              // Phase 3 범위 공격 반경
            data.bossProjectileSpeed = 10f;        // 투사체 속도
            data.bossProjectileDamage = 20;        // 투사체 데미지
            data.bossAreaDamage = 40;              // 범위 공격 데미지
            data.bossChargeSpeed = 12f;            // 돌진 속도
            data.bossChargeDistance = 10f;         // 돌진 거리

            // ====== UI ======
            data.showNameTag = false;
            data.showBossHealthBar = true;

            AssetDatabase.CreateAsset(data, assetPath);
            Debug.Log($"[EnemyDataCreator] ✅ FireDragon (보스) 생성: {assetPath}");
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 디렉토리 존재 확인 및 생성
        /// </summary>
        private void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder(EnemyDataPath))
            {
                // _Project/Data/Enemies 폴더 생성
                string parentPath = "Assets/_Project/Data";
                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    parentPath = "Assets/_Project";
                    if (!AssetDatabase.IsValidFolder(parentPath))
                    {
                        AssetDatabase.CreateFolder("Assets", "_Project");
                    }
                    AssetDatabase.CreateFolder("Assets/_Project", "Data");
                }

                AssetDatabase.CreateFolder("Assets/_Project/Data", "Enemies");
                Debug.Log($"[EnemyDataCreator] 폴더 생성: {EnemyDataPath}");
            }
        }
    }
}
