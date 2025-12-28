#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using GASPT.Data;
using GASPT.Core.Enums;

namespace GASPT.Editor
{
    /// <summary>
    /// 아이템 에셋 생성 에디터 도구
    /// </summary>
    public class ItemAssetGenerator : EditorWindow
    {
        // ====== 설정 ======

        private string outputPath = "Assets/_Project/Resources/Items";
        private string itemName = "NewItem";
        private ItemCategory category = ItemCategory.Equipment;
        private ItemRarity rarity = ItemRarity.Common;
        private EquipmentSlot equipSlot = EquipmentSlot.Weapon;
        private ConsumeType consumeType = ConsumeType.Heal;

        // 장비 설정
        private int requiredLevel = 1;
        private int attackBonus = 0;
        private int defenseBonus = 0;
        private int hpBonus = 0;

        // 소비 설정
        private float effectValue = 50f;
        private float cooldown = 5f;


        // ====== 메뉴 ======

        [MenuItem("GASPT/Item Asset Generator")]
        public static void ShowWindow()
        {
            GetWindow<ItemAssetGenerator>("Item Generator");
        }


        // ====== GUI ======

        private void OnGUI()
        {
            GUILayout.Label("아이템 에셋 생성기", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 기본 설정
            GUILayout.Label("기본 설정", EditorStyles.boldLabel);
            outputPath = EditorGUILayout.TextField("출력 경로", outputPath);
            itemName = EditorGUILayout.TextField("아이템 이름", itemName);
            category = (ItemCategory)EditorGUILayout.EnumPopup("카테고리", category);
            rarity = (ItemRarity)EditorGUILayout.EnumPopup("등급", rarity);

            EditorGUILayout.Space();

            // 카테고리별 설정
            switch (category)
            {
                case ItemCategory.Equipment:
                    DrawEquipmentSettings();
                    break;

                case ItemCategory.Consumable:
                    DrawConsumableSettings();
                    break;
            }

            EditorGUILayout.Space();

            // 생성 버튼
            if (GUILayout.Button("에셋 생성", GUILayout.Height(30)))
            {
                CreateItemAsset();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("생성된 에셋은 지정된 경로에 저장됩니다.", MessageType.Info);
        }

        private void DrawEquipmentSettings()
        {
            GUILayout.Label("장비 설정", EditorStyles.boldLabel);
            equipSlot = (EquipmentSlot)EditorGUILayout.EnumPopup("장비 슬롯", equipSlot);
            requiredLevel = EditorGUILayout.IntField("필요 레벨", requiredLevel);

            EditorGUILayout.Space();
            GUILayout.Label("기본 스탯", EditorStyles.boldLabel);
            attackBonus = EditorGUILayout.IntField("공격력", attackBonus);
            defenseBonus = EditorGUILayout.IntField("방어력", defenseBonus);
            hpBonus = EditorGUILayout.IntField("체력", hpBonus);
        }

        private void DrawConsumableSettings()
        {
            GUILayout.Label("소비 아이템 설정", EditorStyles.boldLabel);
            consumeType = (ConsumeType)EditorGUILayout.EnumPopup("효과 타입", consumeType);
            effectValue = EditorGUILayout.FloatField("효과 수치", effectValue);
            cooldown = EditorGUILayout.FloatField("쿨다운 (초)", cooldown);
        }


        // ====== 에셋 생성 ======

        private void CreateItemAsset()
        {
            // 경로 확인
            string subFolder = category switch
            {
                ItemCategory.Equipment => GetEquipmentSubFolder(),
                ItemCategory.Consumable => "Consumables",
                ItemCategory.Material => "Materials",
                _ => "Misc"
            };

            string fullPath = Path.Combine(outputPath, subFolder);

            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                CreateFolders(fullPath);
            }

            // 에셋 생성
            ScriptableObject asset = category switch
            {
                ItemCategory.Equipment => CreateEquipmentData(),
                ItemCategory.Consumable => CreateConsumableData(),
                _ => CreateItemData()
            };

            if (asset == null)
            {
                Debug.LogError("[ItemAssetGenerator] 에셋 생성 실패");
                return;
            }

            // 저장
            string assetPath = $"{fullPath}/{itemName}.asset";
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 선택
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);

            Debug.Log($"[ItemAssetGenerator] 에셋 생성 완료: {assetPath}");
        }

        private string GetEquipmentSubFolder()
        {
            return equipSlot switch
            {
                EquipmentSlot.Weapon => "Weapons",
                EquipmentSlot.Armor => "Armors",
                EquipmentSlot.Helmet => "Helmets",
                EquipmentSlot.Gloves => "Gloves",
                EquipmentSlot.Boots => "Boots",
                EquipmentSlot.Accessory1 or EquipmentSlot.Accessory2 => "Accessories",
                _ => "Equipment"
            };
        }

        private void CreateFolders(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = "";

            for (int i = 0; i < folders.Length; i++)
            {
                string parentPath = currentPath;
                currentPath = string.IsNullOrEmpty(currentPath)
                    ? folders[i]
                    : $"{currentPath}/{folders[i]}";

                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    if (string.IsNullOrEmpty(parentPath))
                    {
                        parentPath = "Assets";
                        currentPath = $"Assets/{folders[i]}";
                    }

                    AssetDatabase.CreateFolder(parentPath, folders[i]);
                }
            }
        }

        private ItemData CreateItemData()
        {
            ItemData data = ScriptableObject.CreateInstance<ItemData>();
            data.itemId = $"ITM_{itemName}";
            data.itemName = itemName;
            data.description = $"{itemName} 설명";
            data.category = category;
            data.rarity = rarity;
            data.stackable = true;
            data.maxStack = 99;
            data.sellPrice = GetBasePrice();
            data.buyPrice = GetBasePrice() * 2;

            return data;
        }

        private EquipmentData CreateEquipmentData()
        {
            EquipmentData data = ScriptableObject.CreateInstance<EquipmentData>();

            string prefix = equipSlot switch
            {
                EquipmentSlot.Weapon => "WPN",
                EquipmentSlot.Armor => "ARM",
                EquipmentSlot.Helmet => "HLM",
                EquipmentSlot.Gloves => "GLV",
                EquipmentSlot.Boots => "BOT",
                _ => "ACC"
            };

            data.itemId = $"{prefix}_{itemName}";
            data.itemName = itemName;
            data.description = $"{itemName} 장비";
            data.category = ItemCategory.Equipment;
            data.rarity = rarity;
            data.stackable = false;
            data.maxStack = 1;
            data.sellPrice = GetBasePrice();
            data.buyPrice = GetBasePrice() * 2;

            data.equipSlot = equipSlot;
            data.requiredLevel = requiredLevel;

            // 기본 스탯
            if (attackBonus > 0)
            {
                data.baseStats.Add(new StatModifier(StatType.Attack, ModifierType.Flat, attackBonus));
            }
            if (defenseBonus > 0)
            {
                data.baseStats.Add(new StatModifier(StatType.Defense, ModifierType.Flat, defenseBonus));
            }
            if (hpBonus > 0)
            {
                data.baseStats.Add(new StatModifier(StatType.HP, ModifierType.Flat, hpBonus));
            }

            return data;
        }

        private ConsumableData CreateConsumableData()
        {
            ConsumableData data = ScriptableObject.CreateInstance<ConsumableData>();

            string prefix = consumeType switch
            {
                ConsumeType.Heal => "POT",
                ConsumeType.RestoreMana => "POT",
                ConsumeType.Buff => "BUF",
                _ => "CON"
            };

            data.itemId = $"{prefix}_{itemName}";
            data.itemName = itemName;
            data.description = $"{itemName} 소비 아이템";
            data.category = ItemCategory.Consumable;
            data.rarity = rarity;
            data.stackable = true;
            data.maxStack = 99;
            data.sellPrice = GetBasePrice();
            data.buyPrice = GetBasePrice() * 2;

            data.consumeType = consumeType;
            data.effectValue = effectValue;
            data.cooldown = cooldown;

            return data;
        }

        private int GetBasePrice()
        {
            int basePrice = rarity switch
            {
                ItemRarity.Common => 10,
                ItemRarity.Uncommon => 50,
                ItemRarity.Rare => 200,
                ItemRarity.Epic => 1000,
                ItemRarity.Legendary => 5000,
                _ => 10
            };

            return basePrice;
        }


        // ====== 테스트 에셋 일괄 생성 ======

        [MenuItem("GASPT/Generate Test Items")]
        public static void GenerateTestItems()
        {
            GenerateTestWeapons();
            GenerateTestArmors();
            GenerateTestConsumables();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ItemAssetGenerator] 테스트 아이템 생성 완료!");
        }

        [MenuItem("GASPT/Generate Test Items/Weapons Only")]
        public static void GenerateTestWeapons()
        {
            string basePath = "Assets/_Project/Resources/Items/Weapons";
            EnsureFolderExists(basePath);

            // 5개 무기 생성
            CreateTestEquipment(basePath, "IronSword", "철검", EquipmentSlot.Weapon, ItemRarity.Common, 1, 15, 0, 0);
            CreateTestEquipment(basePath, "SteelBlade", "강철 검", EquipmentSlot.Weapon, ItemRarity.Uncommon, 5, 30, 0, 10);
            CreateTestEquipment(basePath, "MagicStaff", "마법 지팡이", EquipmentSlot.Weapon, ItemRarity.Rare, 10, 45, 0, 50);
            CreateTestEquipment(basePath, "FlameSword", "화염검", EquipmentSlot.Weapon, ItemRarity.Epic, 15, 80, 5, 0);
            CreateTestEquipment(basePath, "DragonSlayer", "용살자", EquipmentSlot.Weapon, ItemRarity.Legendary, 20, 150, 10, 20);

            Debug.Log("[ItemAssetGenerator] 무기 테스트 아이템 5개 생성");
        }

        [MenuItem("GASPT/Generate Test Items/Armors Only")]
        public static void GenerateTestArmors()
        {
            string basePath = "Assets/_Project/Resources/Items/Armors";
            EnsureFolderExists(basePath);

            // 5개 방어구 생성
            CreateTestEquipment(basePath, "LeatherArmor", "가죽 갑옷", EquipmentSlot.Armor, ItemRarity.Common, 1, 0, 10, 20);
            CreateTestEquipment(basePath, "ChainMail", "사슬 갑옷", EquipmentSlot.Armor, ItemRarity.Uncommon, 5, 0, 25, 40);
            CreateTestEquipment(basePath, "PlateArmor", "판금 갑옷", EquipmentSlot.Armor, ItemRarity.Rare, 10, 0, 50, 80);
            CreateTestEquipment(basePath, "MithrilArmor", "미스릴 갑옷", EquipmentSlot.Armor, ItemRarity.Epic, 15, 10, 80, 120);
            CreateTestEquipment(basePath, "DragonScale", "용린 갑옷", EquipmentSlot.Armor, ItemRarity.Legendary, 20, 20, 150, 200);

            Debug.Log("[ItemAssetGenerator] 방어구 테스트 아이템 5개 생성");
        }

        [MenuItem("GASPT/Generate Test Items/Consumables Only")]
        public static void GenerateTestConsumables()
        {
            string basePath = "Assets/_Project/Resources/Items/Consumables";
            EnsureFolderExists(basePath);

            // 5개 소비 아이템 생성
            CreateTestConsumable(basePath, "SmallPotion", "소형 체력 포션", ConsumeType.Heal, 50f, 3f, ItemRarity.Common);
            CreateTestConsumable(basePath, "MediumPotion", "중형 체력 포션", ConsumeType.Heal, 150f, 5f, ItemRarity.Uncommon);
            CreateTestConsumable(basePath, "LargePotion", "대형 체력 포션", ConsumeType.Heal, 300f, 8f, ItemRarity.Rare);
            CreateTestConsumable(basePath, "ManaPotion", "마나 포션", ConsumeType.RestoreMana, 100f, 5f, ItemRarity.Uncommon);
            CreateTestConsumable(basePath, "StrengthElixir", "힘의 비약", ConsumeType.Buff, 30f, 60f, ItemRarity.Epic);

            Debug.Log("[ItemAssetGenerator] 소비 아이템 테스트 5개 생성");
        }

        private static void CreateTestEquipment(string path, string id, string displayName, EquipmentSlot slot, ItemRarity rarity, int reqLevel, int atk, int def, int hp)
        {
            string assetPath = $"{path}/{id}.asset";

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<EquipmentData>(assetPath) != null)
            {
                Debug.Log($"[ItemAssetGenerator] 이미 존재: {assetPath}");
                return;
            }

            EquipmentData data = ScriptableObject.CreateInstance<EquipmentData>();
            data.itemId = $"WPN_{id}";
            data.itemName = displayName;
            data.description = $"{displayName} - 등급: {rarity}";
            data.category = ItemCategory.Equipment;
            data.rarity = rarity;
            data.stackable = false;
            data.maxStack = 1;
            data.sellPrice = GetPriceByRarity(rarity);
            data.buyPrice = GetPriceByRarity(rarity) * 2;

            data.equipSlot = slot;
            data.requiredLevel = reqLevel;
            data.maxDurability = 100;

            if (atk > 0)
                data.baseStats.Add(new StatModifier(StatType.Attack, ModifierType.Flat, atk));
            if (def > 0)
                data.baseStats.Add(new StatModifier(StatType.Defense, ModifierType.Flat, def));
            if (hp > 0)
                data.baseStats.Add(new StatModifier(StatType.HP, ModifierType.Flat, hp));

            AssetDatabase.CreateAsset(data, assetPath);
        }

        private static void CreateTestConsumable(string path, string id, string displayName, ConsumeType consumeType, float effectValue, float cooldown, ItemRarity rarity)
        {
            string assetPath = $"{path}/{id}.asset";

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<ConsumableData>(assetPath) != null)
            {
                Debug.Log($"[ItemAssetGenerator] 이미 존재: {assetPath}");
                return;
            }

            ConsumableData data = ScriptableObject.CreateInstance<ConsumableData>();
            data.itemId = $"CON_{id}";
            data.itemName = displayName;
            data.description = $"{displayName} - {consumeType}";
            data.category = ItemCategory.Consumable;
            data.rarity = rarity;
            data.stackable = true;
            data.maxStack = 99;
            data.sellPrice = GetPriceByRarity(rarity) / 2;
            data.buyPrice = GetPriceByRarity(rarity);

            data.consumeType = consumeType;
            data.effectValue = effectValue;
            data.cooldown = cooldown;

            AssetDatabase.CreateAsset(data, assetPath);
        }

        private static int GetPriceByRarity(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => 10,
                ItemRarity.Uncommon => 50,
                ItemRarity.Rare => 200,
                ItemRarity.Epic => 1000,
                ItemRarity.Legendary => 5000,
                _ => 10
            };
        }

        private static void EnsureFolderExists(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = "";

            for (int i = 0; i < folders.Length; i++)
            {
                string parentPath = currentPath;
                currentPath = string.IsNullOrEmpty(currentPath)
                    ? folders[i]
                    : $"{currentPath}/{folders[i]}";

                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    if (string.IsNullOrEmpty(parentPath))
                    {
                        parentPath = "Assets";
                        currentPath = $"Assets/{folders[i]}";
                    }

                    AssetDatabase.CreateFolder(parentPath, folders[i]);
                }
            }
        }
    }
}
#endif
