#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GASPT.Meta;
using GASPT.Meta.Enums;

namespace GASPT.Editor
{
    /// <summary>
    /// 업적 에셋 생성 도구
    /// </summary>
    public class AchievementAssetGenerator : EditorWindow
    {
        [MenuItem("GASPT/Meta/Generate Default Achievements")]
        public static void GenerateDefaultAchievements()
        {
            string folderPath = "Assets/Resources/Data/Meta/Achievements";
            EnsureFolderExists(folderPath);

            // 5개 기본 업적 생성
            CreateAchievement(folderPath, new AchievementData
            {
                id = "ACH_FIRST_CLEAR",
                name = "첫 번째 클리어",
                description = "게임을 처음으로 클리어하세요.",
                type = AchievementType.RunClear,
                tier = AchievementTier.Bronze,
                targetValue = 1,
                rewardBone = 500,
                rewardSoul = 10
            });

            CreateAchievement(folderPath, new AchievementData
            {
                id = "ACH_ENEMY_SLAYER",
                name = "적 학살자",
                description = "100마리의 적을 처치하세요.",
                type = AchievementType.EnemyKill,
                tier = AchievementTier.Bronze,
                targetValue = 100,
                rewardBone = 300
            });

            CreateAchievement(folderPath, new AchievementData
            {
                id = "ACH_BOSS_HUNTER",
                name = "보스 사냥꾼",
                description = "보스를 5회 처치하세요.",
                type = AchievementType.BossKill,
                tier = AchievementTier.Silver,
                targetValue = 5,
                rewardBone = 500,
                rewardSoul = 20
            });

            CreateAchievement(folderPath, new AchievementData
            {
                id = "ACH_FORM_COLLECTOR",
                name = "폼 수집가",
                description = "10개의 폼을 수집하세요.",
                type = AchievementType.FormCollect,
                tier = AchievementTier.Silver,
                targetValue = 10,
                rewardSoul = 30
            });

            CreateAchievement(folderPath, new AchievementData
            {
                id = "ACH_MASTER_CLEAR",
                name = "마스터 클리어",
                description = "게임을 10회 클리어하세요.",
                type = AchievementType.RunClear,
                tier = AchievementTier.Gold,
                targetValue = 10,
                rewardBone = 1000,
                rewardSoul = 50
            });

            AssetDatabase.Refresh();
            Debug.Log("기본 업적 5개 생성 완료!");
        }

        private struct AchievementData
        {
            public string id;
            public string name;
            public string description;
            public AchievementType type;
            public AchievementTier tier;
            public int targetValue;
            public int rewardBone;
            public int rewardSoul;
        }

        private static void CreateAchievement(string folderPath, AchievementData data)
        {
            string assetPath = $"{folderPath}/{data.id}.asset";

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<Achievement>(assetPath) != null)
            {
                Debug.Log($"업적 이미 존재: {data.id}");
                return;
            }

            Achievement achievement = ScriptableObject.CreateInstance<Achievement>();

            achievement.achievementId = data.id;
            achievement.achievementName = data.name;
            achievement.description = data.description;
            achievement.achievementType = data.type;
            achievement.tier = data.tier;
            achievement.targetValue = data.targetValue;
            achievement.rewardBone = data.rewardBone;
            achievement.rewardSoul = data.rewardSoul;
            achievement.showProgress = true;
            achievement.isHidden = false;

            AssetDatabase.CreateAsset(achievement, assetPath);
            Debug.Log($"업적 생성: {data.name} ({assetPath})");
        }

        private static void EnsureFolderExists(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
    }
}
#endif
