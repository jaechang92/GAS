#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GASPT.Meta;

namespace GASPT.Editor
{
    /// <summary>
    /// 업그레이드 에셋 생성 도구
    /// 기본 업그레이드와 특수 업그레이드(ExtraDash, Revive) 생성
    /// </summary>
    public class UpgradeAssetGenerator : EditorWindow
    {
        [MenuItem("GASPT/Meta/Generate Default Upgrades")]
        public static void GenerateDefaultUpgrades()
        {
            string folderPath = "Assets/Resources/Data/Meta/Upgrades";
            EnsureFolderExists(folderPath);

            // 기본 스탯 업그레이드
            CreateUpgrade(folderPath, new UpgradeData
            {
                id = "UP_MAX_HP",
                name = "체력 강화",
                description = "최대 체력을 증가시킵니다.",
                type = UpgradeType.MaxHP,
                currency = CurrencyType.Bone,
                maxLevel = 5,
                costs = new int[] { 100, 200, 400, 800, 1600 },
                effects = new float[] { 10, 25, 50, 80, 120 }
            });

            CreateUpgrade(folderPath, new UpgradeData
            {
                id = "UP_ATTACK",
                name = "공격력 강화",
                description = "공격력을 증가시킵니다.",
                type = UpgradeType.Attack,
                currency = CurrencyType.Bone,
                maxLevel = 5,
                costs = new int[] { 150, 300, 600, 1200, 2400 },
                effects = new float[] { 5, 10, 15, 20, 30 }
            });

            CreateUpgrade(folderPath, new UpgradeData
            {
                id = "UP_DEFENSE",
                name = "방어력 강화",
                description = "받는 피해를 감소시킵니다.",
                type = UpgradeType.Defense,
                currency = CurrencyType.Bone,
                maxLevel = 5,
                costs = new int[] { 120, 240, 480, 960, 1920 },
                effects = new float[] { 3, 6, 10, 15, 20 }
            });

            CreateUpgrade(folderPath, new UpgradeData
            {
                id = "UP_MOVE_SPEED",
                name = "이동속도 강화",
                description = "이동속도를 증가시킵니다.",
                type = UpgradeType.MoveSpeed,
                currency = CurrencyType.Bone,
                maxLevel = 3,
                costs = new int[] { 200, 500, 1000 },
                effects = new float[] { 5, 10, 15 }
            });

            // 유틸리티 업그레이드
            CreateUpgrade(folderPath, new UpgradeData
            {
                id = "UP_GOLD_BONUS",
                name = "황금 손",
                description = "골드 획득량을 증가시킵니다.",
                type = UpgradeType.GoldBonus,
                currency = CurrencyType.Bone,
                maxLevel = 5,
                costs = new int[] { 100, 200, 400, 800, 1600 },
                effects = new float[] { 10, 20, 35, 50, 75 }
            });

            CreateUpgrade(folderPath, new UpgradeData
            {
                id = "UP_EXP_BONUS",
                name = "빠른 성장",
                description = "경험치 획득량을 증가시킵니다.",
                type = UpgradeType.ExpBonus,
                currency = CurrencyType.Bone,
                maxLevel = 5,
                costs = new int[] { 100, 200, 400, 800, 1600 },
                effects = new float[] { 10, 20, 35, 50, 75 }
            });

            CreateUpgrade(folderPath, new UpgradeData
            {
                id = "UP_START_GOLD",
                name = "자본금",
                description = "런 시작 시 골드를 지급합니다.",
                type = UpgradeType.StartGold,
                currency = CurrencyType.Bone,
                maxLevel = 5,
                costs = new int[] { 200, 400, 800, 1600, 3200 },
                effects = new float[] { 50, 100, 200, 400, 800 }
            });

            // 특수 업그레이드 (Soul 사용)
            CreateUpgrade(folderPath, new UpgradeData
            {
                id = "UP_EXTRA_DASH",
                name = "추가 대시",
                description = "대시 횟수를 증가시킵니다. 연속 회피에 유용합니다.",
                type = UpgradeType.ExtraDash,
                currency = CurrencyType.Soul,
                maxLevel = 2,
                costs = new int[] { 30, 100 },
                effects = new float[] { 1, 2 }
            });

            CreateUpgrade(folderPath, new UpgradeData
            {
                id = "UP_REVIVE",
                name = "부활",
                description = "런당 사망 시 부활할 수 있는 횟수를 증가시킵니다.",
                type = UpgradeType.Revive,
                currency = CurrencyType.Soul,
                maxLevel = 2,
                costs = new int[] { 50, 150 },
                effects = new float[] { 1, 2 }
            });

            AssetDatabase.Refresh();
            Debug.Log("기본 업그레이드 9개 생성 완료!");
        }

        private struct UpgradeData
        {
            public string id;
            public string name;
            public string description;
            public UpgradeType type;
            public CurrencyType currency;
            public int maxLevel;
            public int[] costs;
            public float[] effects;
            public string[] prerequisites;
        }

        private static void CreateUpgrade(string folderPath, UpgradeData data)
        {
            string assetPath = $"{folderPath}/{data.id}.asset";

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<PermanentUpgrade>(assetPath) != null)
            {
                Debug.Log($"업그레이드 이미 존재: {data.id}");
                return;
            }

            PermanentUpgrade upgrade = ScriptableObject.CreateInstance<PermanentUpgrade>();

            upgrade.upgradeId = data.id;
            upgrade.upgradeName = data.name;
            upgrade.description = data.description;
            upgrade.upgradeType = data.type;
            upgrade.currencyType = data.currency;
            upgrade.maxLevel = data.maxLevel;
            upgrade.costPerLevel = data.costs;
            upgrade.effectPerLevel = data.effects;
            upgrade.prerequisiteIds = data.prerequisites ?? new string[0];

            AssetDatabase.CreateAsset(upgrade, assetPath);
            Debug.Log($"업그레이드 생성: {data.name} ({assetPath})");
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
