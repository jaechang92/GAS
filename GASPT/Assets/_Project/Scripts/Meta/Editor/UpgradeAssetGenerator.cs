#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GASPT.Meta.Editor
{
    /// <summary>
    /// 업그레이드 에셋 자동 생성 에디터 도구
    /// </summary>
    public static class UpgradeAssetGenerator
    {
        private const string AssetPath = "Assets/Resources/Data/Meta/Upgrades";

        [MenuItem("GASPT/Meta/Generate All Upgrade Assets")]
        public static void GenerateAllUpgradeAssets()
        {
            // 폴더 생성
            if (!Directory.Exists(AssetPath))
            {
                Directory.CreateDirectory(AssetPath);
                AssetDatabase.Refresh();
            }

            int created = 0;

            // UP001: 최대 체력
            if (CreateUpgradeAsset(new UpgradeDefinition
            {
                id = "UP001",
                name = "체력 강화",
                description = "최대 HP를 증가시킵니다.",
                type = UpgradeType.MaxHP,
                currency = CurrencyType.Bone,
                maxLevel = 5,
                costs = new int[] { 100, 250, 500, 1000, 2000 },
                effects = new float[] { 5, 10, 15, 20, 25 }
            })) created++;

            // UP002: 공격력
            if (CreateUpgradeAsset(new UpgradeDefinition
            {
                id = "UP002",
                name = "공격 강화",
                description = "기본 공격력이 증가합니다. (% 증가)",
                type = UpgradeType.Attack,
                currency = CurrencyType.Bone,
                maxLevel = 5,
                costs = new int[] { 100, 250, 500, 1000, 2000 },
                effects = new float[] { 5, 10, 15, 20, 25 }
            })) created++;

            // UP003: 방어력
            if (CreateUpgradeAsset(new UpgradeDefinition
            {
                id = "UP003",
                name = "방어 강화",
                description = "받는 피해가 감소합니다. (% 감소)",
                type = UpgradeType.Defense,
                currency = CurrencyType.Bone,
                maxLevel = 5,
                costs = new int[] { 150, 350, 700, 1400, 2800 },
                effects = new float[] { 3, 6, 9, 12, 15 }
            })) created++;

            // UP004: 이동속도
            if (CreateUpgradeAsset(new UpgradeDefinition
            {
                id = "UP004",
                name = "이동 강화",
                description = "이동 속도가 증가합니다. (% 증가)",
                type = UpgradeType.MoveSpeed,
                currency = CurrencyType.Bone,
                maxLevel = 5,
                costs = new int[] { 100, 200, 400, 800, 1600 },
                effects = new float[] { 3, 6, 9, 12, 15 }
            })) created++;

            // UP005: 골드 보너스
            if (CreateUpgradeAsset(new UpgradeDefinition
            {
                id = "UP005",
                name = "골드 수집",
                description = "골드 획득량이 증가합니다. (% 증가)",
                type = UpgradeType.GoldBonus,
                currency = CurrencyType.Bone,
                maxLevel = 5,
                costs = new int[] { 200, 400, 800, 1600, 3200 },
                effects = new float[] { 10, 20, 30, 40, 50 }
            })) created++;

            // UP006: 경험치 보너스
            if (CreateUpgradeAsset(new UpgradeDefinition
            {
                id = "UP006",
                name = "경험치 증폭",
                description = "경험치 획득량이 증가합니다. (% 증가)",
                type = UpgradeType.ExpBonus,
                currency = CurrencyType.Bone,
                maxLevel = 3,
                costs = new int[] { 300, 700, 1500 },
                effects = new float[] { 10, 20, 30 }
            })) created++;

            // UP007: 시작 골드
            if (CreateUpgradeAsset(new UpgradeDefinition
            {
                id = "UP007",
                name = "시작 자금",
                description = "런 시작 시 골드를 보유합니다.",
                type = UpgradeType.StartGold,
                currency = CurrencyType.Bone,
                maxLevel = 4,
                costs = new int[] { 150, 350, 700, 1400 },
                effects = new float[] { 50, 100, 150, 200 }
            })) created++;

            // UP008: 추가 대시 (Soul)
            if (CreateUpgradeAsset(new UpgradeDefinition
            {
                id = "UP008",
                name = "추가 대시",
                description = "대시 횟수가 1회 증가합니다.",
                type = UpgradeType.ExtraDash,
                currency = CurrencyType.Soul,
                maxLevel = 1,
                costs = new int[] { 200 },
                effects = new float[] { 1 }
            })) created++;

            // UP009: 부활 (Soul)
            if (CreateUpgradeAsset(new UpgradeDefinition
            {
                id = "UP009",
                name = "불사의 의지",
                description = "런당 1회 사망 시 부활합니다.",
                type = UpgradeType.Revive,
                currency = CurrencyType.Soul,
                maxLevel = 1,
                costs = new int[] { 500 },
                effects = new float[] { 1 }
            })) created++;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[UpgradeAssetGenerator] {created}개 업그레이드 에셋 생성 완료!");
            EditorUtility.DisplayDialog("완료", $"{created}개 업그레이드 에셋이 생성되었습니다.\n경로: {AssetPath}", "확인");
        }

        private static bool CreateUpgradeAsset(UpgradeDefinition def)
        {
            string assetPath = $"{AssetPath}/{def.id}_{def.type}.asset";

            // 이미 존재하면 스킵
            if (File.Exists(assetPath))
            {
                Debug.Log($"[UpgradeAssetGenerator] 이미 존재: {def.id}");
                return false;
            }

            PermanentUpgrade upgrade = ScriptableObject.CreateInstance<PermanentUpgrade>();
            upgrade.upgradeId = def.id;
            upgrade.upgradeName = def.name;
            upgrade.description = def.description;
            upgrade.upgradeType = def.type;
            upgrade.currencyType = def.currency;
            upgrade.maxLevel = def.maxLevel;
            upgrade.costPerLevel = def.costs;
            upgrade.effectPerLevel = def.effects;
            upgrade.prerequisiteIds = new string[0];

            AssetDatabase.CreateAsset(upgrade, assetPath);
            Debug.Log($"[UpgradeAssetGenerator] 생성: {def.id} - {def.name}");

            return true;
        }

        [MenuItem("GASPT/Meta/Delete All Upgrade Assets")]
        public static void DeleteAllUpgradeAssets()
        {
            if (!EditorUtility.DisplayDialog("확인",
                "모든 업그레이드 에셋을 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.",
                "삭제", "취소"))
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:PermanentUpgrade", new[] { AssetPath });
            int deleted = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.DeleteAsset(path))
                {
                    deleted++;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[UpgradeAssetGenerator] {deleted}개 에셋 삭제 완료");
        }

        [MenuItem("GASPT/Meta/List All Upgrade Assets")]
        public static void ListAllUpgradeAssets()
        {
            PermanentUpgrade[] upgrades = Resources.LoadAll<PermanentUpgrade>("Data/Meta/Upgrades");

            Debug.Log($"========== 업그레이드 에셋 목록 ({upgrades.Length}개) ==========");
            foreach (var upgrade in upgrades)
            {
                Debug.Log($"  [{upgrade.upgradeId}] {upgrade.upgradeName} " +
                         $"(Type: {upgrade.upgradeType}, Max: Lv{upgrade.maxLevel}, " +
                         $"Cost: {upgrade.currencyType})");
            }
            Debug.Log("=================================================");
        }

        /// <summary>
        /// 업그레이드 정의 헬퍼 클래스
        /// </summary>
        private class UpgradeDefinition
        {
            public string id;
            public string name;
            public string description;
            public UpgradeType type;
            public CurrencyType currency;
            public int maxLevel;
            public int[] costs;
            public float[] effects;
        }
    }
}
#endif
