#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GASPT.Data;
using GASPT.Core.Enums;

namespace GASPT.Editor
{
    /// <summary>
    /// 보스 ScriptableObject 에셋 자동 생성 도구
    /// Unity 메뉴에서 GASPT > Bosses > Generate Default Boss Assets 클릭
    /// </summary>
    public class BossAssetGenerator : EditorWindow
    {
        private const string FolderPath = "Assets/Resources/Data/Bosses";

        [MenuItem("GASPT/Bosses/Generate Default Boss Assets")]
        public static void GenerateDefaultBossAssets()
        {
            EnsureFolderExists(FolderPath);

            int created = 0;

            // B001 - FlameGolem (불꽃 골렘)
            if (CreateFlameGolem()) created++;

            // B002 - FrostSpirit (얼음 정령)
            if (CreateFrostSpirit()) created++;

            // B003 - ThunderDragon (번개 드래곤)
            if (CreateThunderDragon()) created++;

            // B004 - DarkLord (어둠의 군주)
            if (CreateDarkLord()) created++;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[BossAssetGenerator] {created}개의 보스 에셋이 생성되었습니다. 경로: {FolderPath}");
            EditorUtility.DisplayDialog(
                "보스 에셋 생성 완료",
                $"{created}개의 보스 에셋이 생성되었습니다.\n\n경로: {FolderPath}",
                "확인"
            );
        }

        /// <summary>
        /// B001 - FlameGolem (불꽃 골렘) - 미니보스
        /// </summary>
        private static bool CreateFlameGolem()
        {
            string assetPath = $"{FolderPath}/FlameGolem.asset";
            if (AssetDatabase.LoadAssetAtPath<BossData>(assetPath) != null)
            {
                Debug.Log("[BossAssetGenerator] FlameGolem.asset 이미 존재함, 스킵");
                return false;
            }

            BossData boss = ScriptableObject.CreateInstance<BossData>();

            // 기본 정보
            boss.bossId = "B001";
            boss.bossName = "불꽃 골렘";
            boss.description = "용암 지대를 지키는 거대한 화염 골렘. 화염 속성 공격에 주의하라.";
            boss.bossGrade = BossGrade.MiniBoss;
            boss.elementType = ElementType.Fire;

            // 스탯 (미니보스 - 45초 전투 기준)
            boss.maxHealth = 800;
            boss.baseAttack = 25;
            boss.defense = 10;
            boss.moveSpeed = 3f;

            // 페이즈 (2페이즈: 70%, 35%)
            boss.phases = CreateFlameGolemPhases();

            // 전투 설정
            boss.detectionRange = 12f;
            boss.attackRange = 2.5f;
            boss.attackCooldown = 2f;
            boss.timeLimit = 120f;

            // 보상
            boss.goldReward = 200;
            boss.expReward = 80;
            boss.boneDrop = 30;
            boss.soulDrop = 5;
            boss.firstClearBonusGold = 300;

            AssetDatabase.CreateAsset(boss, assetPath);
            Debug.Log($"[BossAssetGenerator] 생성됨: {assetPath}");
            return true;
        }

        /// <summary>
        /// B002 - FrostSpirit (얼음 정령) - 미니보스
        /// </summary>
        private static bool CreateFrostSpirit()
        {
            string assetPath = $"{FolderPath}/FrostSpirit.asset";
            if (AssetDatabase.LoadAssetAtPath<BossData>(assetPath) != null)
            {
                Debug.Log("[BossAssetGenerator] FrostSpirit.asset 이미 존재함, 스킵");
                return false;
            }

            BossData boss = ScriptableObject.CreateInstance<BossData>();

            // 기본 정보
            boss.bossId = "B002";
            boss.bossName = "얼음 정령";
            boss.description = "빙하 동굴에서 태어난 얼음 정령. 빙결 상태에 걸리면 움직일 수 없다.";
            boss.bossGrade = BossGrade.MiniBoss;
            boss.elementType = ElementType.Ice;

            // 스탯 (미니보스 - 45초 전투 기준)
            boss.maxHealth = 700;
            boss.baseAttack = 20;
            boss.defense = 5;
            boss.moveSpeed = 4f;

            // 페이즈 (2페이즈: 65%, 30%)
            boss.phases = CreateFrostSpiritPhases();

            // 전투 설정
            boss.detectionRange = 14f;
            boss.attackRange = 5f;
            boss.attackCooldown = 1.8f;
            boss.timeLimit = 120f;

            // 보상
            boss.goldReward = 200;
            boss.expReward = 80;
            boss.boneDrop = 30;
            boss.soulDrop = 5;
            boss.firstClearBonusGold = 300;

            AssetDatabase.CreateAsset(boss, assetPath);
            Debug.Log($"[BossAssetGenerator] 생성됨: {assetPath}");
            return true;
        }

        /// <summary>
        /// B003 - ThunderDragon (번개 드래곤) - 중간보스
        /// </summary>
        private static bool CreateThunderDragon()
        {
            string assetPath = $"{FolderPath}/ThunderDragon.asset";
            if (AssetDatabase.LoadAssetAtPath<BossData>(assetPath) != null)
            {
                Debug.Log("[BossAssetGenerator] ThunderDragon.asset 이미 존재함, 스킵");
                return false;
            }

            BossData boss = ScriptableObject.CreateInstance<BossData>();

            // 기본 정보
            boss.bossId = "B003";
            boss.bossName = "번개 드래곤";
            boss.description = "하늘을 지배하는 번개 드래곤. 빠른 공격과 강력한 범위 공격이 특징.";
            boss.bossGrade = BossGrade.MidBoss;
            boss.elementType = ElementType.Thunder;

            // 스탯 (중간보스 - 90초 전투 기준)
            boss.maxHealth = 2000;
            boss.baseAttack = 40;
            boss.defense = 15;
            boss.moveSpeed = 5f;

            // 페이즈 (3페이즈: 75%, 50%, 25%)
            boss.phases = CreateThunderDragonPhases();

            // 전투 설정
            boss.detectionRange = 18f;
            boss.attackRange = 4f;
            boss.attackCooldown = 1.5f;
            boss.timeLimit = 180f;

            // 보상
            boss.goldReward = 500;
            boss.expReward = 200;
            boss.boneDrop = 60;
            boss.soulDrop = 15;
            boss.firstClearBonusGold = 800;

            AssetDatabase.CreateAsset(boss, assetPath);
            Debug.Log($"[BossAssetGenerator] 생성됨: {assetPath}");
            return true;
        }

        /// <summary>
        /// B004 - DarkLord (어둠의 군주) - 최종보스
        /// </summary>
        private static bool CreateDarkLord()
        {
            string assetPath = $"{FolderPath}/DarkLord.asset";
            if (AssetDatabase.LoadAssetAtPath<BossData>(assetPath) != null)
            {
                Debug.Log("[BossAssetGenerator] DarkLord.asset 이미 존재함, 스킵");
                return false;
            }

            BossData boss = ScriptableObject.CreateInstance<BossData>();

            // 기본 정보
            boss.bossId = "B004";
            boss.bossName = "어둠의 군주";
            boss.description = "암흑 세계의 지배자. 분신 소환과 생명력 흡수로 전장을 지배한다.";
            boss.bossGrade = BossGrade.FinalBoss;
            boss.elementType = ElementType.Dark;

            // 스탯 (최종보스 - 180초 전투 기준)
            boss.maxHealth = 5000;
            boss.baseAttack = 60;
            boss.defense = 25;
            boss.moveSpeed = 4f;

            // 페이즈 (4페이즈: 80%, 60%, 40%, 20%)
            boss.phases = CreateDarkLordPhases();

            // 전투 설정
            boss.detectionRange = 20f;
            boss.attackRange = 3.5f;
            boss.attackCooldown = 1.2f;
            boss.timeLimit = 300f;

            // 보상
            boss.goldReward = 1500;
            boss.expReward = 600;
            boss.boneDrop = 150;
            boss.soulDrop = 50;
            boss.firstClearBonusGold = 2000;

            AssetDatabase.CreateAsset(boss, assetPath);
            Debug.Log($"[BossAssetGenerator] 생성됨: {assetPath}");
            return true;
        }

        // ====== 페이즈 생성 헬퍼 ======

        private static BossPhaseData[] CreateFlameGolemPhases()
        {
            return new BossPhaseData[]
            {
                new BossPhaseData
                {
                    phaseIndex = 1,
                    healthThreshold = 1f,
                    attackMultiplier = 1f,
                    speedMultiplier = 1f,
                    attackSpeedMultiplier = 1f,
                    invulnerabilityDuration = 1f,
                    cameraShakeIntensity = 0.2f,
                    availablePatternIndices = new int[] { 0, 1 }  // 화염 내려찍기, 화염구
                },
                new BossPhaseData
                {
                    phaseIndex = 2,
                    healthThreshold = 0.35f,
                    attackMultiplier = 1.4f,
                    speedMultiplier = 1.2f,
                    attackSpeedMultiplier = 1.3f,
                    invulnerabilityDuration = 1.5f,
                    cameraShakeIntensity = 0.5f,
                    availablePatternIndices = new int[] { 0, 1, 2 }  // + 폭발
                }
            };
        }

        private static BossPhaseData[] CreateFrostSpiritPhases()
        {
            return new BossPhaseData[]
            {
                new BossPhaseData
                {
                    phaseIndex = 1,
                    healthThreshold = 1f,
                    attackMultiplier = 1f,
                    speedMultiplier = 1f,
                    attackSpeedMultiplier = 1f,
                    invulnerabilityDuration = 1f,
                    cameraShakeIntensity = 0.2f,
                    availablePatternIndices = new int[] { 0, 1 }  // 얼음 창, 빙결 지대
                },
                new BossPhaseData
                {
                    phaseIndex = 2,
                    healthThreshold = 0.3f,
                    attackMultiplier = 1.3f,
                    speedMultiplier = 1.3f,
                    attackSpeedMultiplier = 1.4f,
                    invulnerabilityDuration = 1.5f,
                    cameraShakeIntensity = 0.4f,
                    availablePatternIndices = new int[] { 0, 1, 2 }  // + 얼음 소환수
                }
            };
        }

        private static BossPhaseData[] CreateThunderDragonPhases()
        {
            return new BossPhaseData[]
            {
                new BossPhaseData
                {
                    phaseIndex = 1,
                    healthThreshold = 1f,
                    attackMultiplier = 1f,
                    speedMultiplier = 1f,
                    attackSpeedMultiplier = 1f,
                    invulnerabilityDuration = 1f,
                    cameraShakeIntensity = 0.2f,
                    availablePatternIndices = new int[] { 0, 1 }  // 낙뢰, 연쇄 번개
                },
                new BossPhaseData
                {
                    phaseIndex = 2,
                    healthThreshold = 0.5f,
                    attackMultiplier = 1.25f,
                    speedMultiplier = 1.15f,
                    attackSpeedMultiplier = 1.2f,
                    invulnerabilityDuration = 1.5f,
                    cameraShakeIntensity = 0.4f,
                    availablePatternIndices = new int[] { 0, 1, 2 }  // + 돌진
                },
                new BossPhaseData
                {
                    phaseIndex = 3,
                    healthThreshold = 0.25f,
                    attackMultiplier = 1.5f,
                    speedMultiplier = 1.3f,
                    attackSpeedMultiplier = 1.4f,
                    invulnerabilityDuration = 2f,
                    cameraShakeIntensity = 0.6f,
                    availablePatternIndices = new int[] { 0, 1, 2, 3, 4 }  // + 분노, 번개 폭풍
                }
            };
        }

        private static BossPhaseData[] CreateDarkLordPhases()
        {
            return new BossPhaseData[]
            {
                new BossPhaseData
                {
                    phaseIndex = 1,
                    healthThreshold = 1f,
                    attackMultiplier = 1f,
                    speedMultiplier = 1f,
                    attackSpeedMultiplier = 1f,
                    invulnerabilityDuration = 1f,
                    cameraShakeIntensity = 0.2f,
                    availablePatternIndices = new int[] { 0, 1 }  // 암흑 베기, 암흑구
                },
                new BossPhaseData
                {
                    phaseIndex = 2,
                    healthThreshold = 0.6f,
                    attackMultiplier = 1.2f,
                    speedMultiplier = 1.1f,
                    attackSpeedMultiplier = 1.15f,
                    invulnerabilityDuration = 1.5f,
                    cameraShakeIntensity = 0.35f,
                    availablePatternIndices = new int[] { 0, 1, 2, 3 }  // + 생명력 흡수, 분신 소환
                },
                new BossPhaseData
                {
                    phaseIndex = 3,
                    healthThreshold = 0.4f,
                    attackMultiplier = 1.4f,
                    speedMultiplier = 1.2f,
                    attackSpeedMultiplier = 1.3f,
                    invulnerabilityDuration = 2f,
                    cameraShakeIntensity = 0.5f,
                    availablePatternIndices = new int[] { 0, 1, 2, 3, 4 }  // + 암흑 폭풍
                },
                new BossPhaseData
                {
                    phaseIndex = 4,
                    healthThreshold = 0.2f,
                    attackMultiplier = 1.7f,
                    speedMultiplier = 1.3f,
                    attackSpeedMultiplier = 1.5f,
                    invulnerabilityDuration = 2.5f,
                    cameraShakeIntensity = 0.8f,
                    availablePatternIndices = new int[] { 0, 1, 2, 3, 4, 5 }  // + 멸망의 일격
                }
            };
        }

        // ====== 유틸리티 ======

        private static void EnsureFolderExists(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                    Debug.Log($"[BossAssetGenerator] 폴더 생성: {nextPath}");
                }
                currentPath = nextPath;
            }
        }
    }
}
#endif
