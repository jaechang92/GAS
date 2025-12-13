using UnityEngine;
using UnityEditor;
using GASPT.Data;
using GASPT.Loot;
using System.IO;

namespace GASPT.Editor
{
    /// <summary>
    /// LootTable 자동 생성 에디터 도구
    /// Phase C-4: 아이템 드롭 시스템
    /// </summary>
    public class LootTableCreator : EditorWindow
    {
        // ====== 경로 ======

        private const string LootTablePath = "Assets/_Project/Data/Loot/";
        private const string ItemPath = "Assets/_Project/Data/Items/Equipment/";


        // ====== 에디터 창 ======

        [MenuItem("Tools/GASPT/Loot/LootTable Creator")]
        public static void ShowWindow()
        {
            LootTableCreator window = GetWindow<LootTableCreator>("LootTable Creator");
            window.minSize = new Vector2(400f, 600f);
            window.Show();
        }


        // ====== GUI ======

        private Vector2 scrollPosition;

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10f);

            // 타이틀
            EditorGUILayout.LabelField("LootTable Creator", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Phase C-4: 아이템 드롭 시스템", EditorStyles.miniLabel);

            GUILayout.Space(10f);
            EditorGUILayout.HelpBox(
                "Normal/Elite/Boss용 LootTable 3개를 자동 생성합니다.\n" +
                "기존 아이템 에셋(FireSword, LeatherArmor, IronRing)을 사용합니다.",
                MessageType.Info
            );

            GUILayout.Space(20f);

            // 버튼
            if (GUILayout.Button("🎲 모든 LootTable 생성", GUILayout.Height(40f)))
            {
                CreateAllLootTables();
            }

            GUILayout.Space(10f);

            if (GUILayout.Button("🗑️ 모든 LootTable 삭제", GUILayout.Height(30f)))
            {
                DeleteAllLootTables();
            }

            GUILayout.Space(20f);

            // 개별 생성 버튼
            EditorGUILayout.LabelField("개별 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("Normal LootTable 생성"))
            {
                CreateNormalLootTable();
            }

            if (GUILayout.Button("Elite LootTable 생성"))
            {
                CreateEliteLootTable();
            }

            if (GUILayout.Button("Boss LootTable 생성"))
            {
                CreateBossLootTable();
            }

            GUILayout.Space(20f);

            // 정보
            EditorGUILayout.LabelField("생성 정보", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Normal LootTable:\n" +
                "- FireSword: 10% 확률\n" +
                "- LeatherArmor: 10% 확률\n" +
                "- IronRing: 10% 확률\n" +
                "- 드롭 없음: 70%\n\n" +

                "Elite LootTable:\n" +
                "- FireSword: 20% 확률\n" +
                "- LeatherArmor: 20% 확률\n" +
                "- IronRing: 20% 확률\n" +
                "- 드롭 없음: 40%\n\n" +

                "Boss LootTable:\n" +
                "- FireSword: 40% 확률\n" +
                "- LeatherArmor: 30% 확률\n" +
                "- IronRing: 30% 확률\n" +
                "- 드롭 없음: 0% (100% 드롭)",
                MessageType.None
            );

            EditorGUILayout.EndScrollView();
        }


        // ====== LootTable 생성 ======

        /// <summary>
        /// 모든 LootTable 생성
        /// </summary>
        private void CreateAllLootTables()
        {
            CreateNormalLootTable();
            CreateEliteLootTable();
            CreateBossLootTable();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[LootTableCreator] 모든 LootTable 생성 완료!");
            EditorUtility.DisplayDialog("완료", "모든 LootTable이 생성되었습니다!", "확인");
        }

        /// <summary>
        /// Normal LootTable 생성
        /// </summary>
        private void CreateNormalLootTable()
        {
            LootTable lootTable = CreateLootTableAsset("NormalLootTable");

            // 아이템 로드
            Item fireSword = LoadItem("FireSword");
            Item leatherArmor = LoadItem("LeatherArmor");
            Item ironRing = LoadItem("IronRing");

            // LootEntry 추가 (각 10% 확률)
            lootTable.lootEntries.Clear();
            lootTable.lootEntries.Add(new LootEntry(fireSword, 0.1f));
            lootTable.lootEntries.Add(new LootEntry(leatherArmor, 0.1f));
            lootTable.lootEntries.Add(new LootEntry(ironRing, 0.1f));

            lootTable.allowNoDrop = true;
            lootTable.debugLog = false;

            EditorUtility.SetDirty(lootTable);
            Debug.Log("[LootTableCreator] NormalLootTable 생성 완료 (30% 드롭 확률)");
        }

        /// <summary>
        /// Elite LootTable 생성
        /// </summary>
        private void CreateEliteLootTable()
        {
            LootTable lootTable = CreateLootTableAsset("EliteLootTable");

            // 아이템 로드
            Item fireSword = LoadItem("FireSword");
            Item leatherArmor = LoadItem("LeatherArmor");
            Item ironRing = LoadItem("IronRing");

            // LootEntry 추가 (각 20% 확률)
            lootTable.lootEntries.Clear();
            lootTable.lootEntries.Add(new LootEntry(fireSword, 0.2f));
            lootTable.lootEntries.Add(new LootEntry(leatherArmor, 0.2f));
            lootTable.lootEntries.Add(new LootEntry(ironRing, 0.2f));

            lootTable.allowNoDrop = true;
            lootTable.debugLog = false;

            EditorUtility.SetDirty(lootTable);
            Debug.Log("[LootTableCreator] EliteLootTable 생성 완료 (60% 드롭 확률)");
        }

        /// <summary>
        /// Boss LootTable 생성
        /// </summary>
        private void CreateBossLootTable()
        {
            LootTable lootTable = CreateLootTableAsset("BossLootTable");

            // 아이템 로드
            Item fireSword = LoadItem("FireSword");
            Item leatherArmor = LoadItem("LeatherArmor");
            Item ironRing = LoadItem("IronRing");

            // LootEntry 추가 (FireSword 40%, 나머지 각 30%)
            lootTable.lootEntries.Clear();
            lootTable.lootEntries.Add(new LootEntry(fireSword, 0.4f));
            lootTable.lootEntries.Add(new LootEntry(leatherArmor, 0.3f));
            lootTable.lootEntries.Add(new LootEntry(ironRing, 0.3f));

            lootTable.allowNoDrop = false; // 100% 드롭
            lootTable.debugLog = true;

            EditorUtility.SetDirty(lootTable);
            Debug.Log("[LootTableCreator] BossLootTable 생성 완료 (100% 드롭)");
        }


        // ====== LootTable 삭제 ======

        /// <summary>
        /// 모든 LootTable 삭제
        /// </summary>
        private void DeleteAllLootTables()
        {
            if (!EditorUtility.DisplayDialog("확인", "모든 LootTable을 삭제하시겠습니까?", "삭제", "취소"))
                return;

            DeleteLootTableAsset("NormalLootTable");
            DeleteLootTableAsset("EliteLootTable");
            DeleteLootTableAsset("BossLootTable");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[LootTableCreator] 모든 LootTable 삭제 완료");
            EditorUtility.DisplayDialog("완료", "모든 LootTable이 삭제되었습니다.", "확인");
        }


        // ====== 헬퍼 메서드 ======

        /// <summary>
        /// LootTable 에셋 생성
        /// </summary>
        private LootTable CreateLootTableAsset(string fileName)
        {
            // 경로 확인 및 생성
            if (!Directory.Exists(LootTablePath))
            {
                Directory.CreateDirectory(LootTablePath);
                Debug.Log($"[LootTableCreator] 폴더 생성: {LootTablePath}");
            }

            string path = $"{LootTablePath}{fileName}.asset";

            // 기존 에셋이 있으면 로드, 없으면 생성
            LootTable lootTable = AssetDatabase.LoadAssetAtPath<LootTable>(path);
            if (lootTable == null)
            {
                lootTable = ScriptableObject.CreateInstance<LootTable>();
                AssetDatabase.CreateAsset(lootTable, path);
                Debug.Log($"[LootTableCreator] LootTable 생성: {path}");
            }
            else
            {
                Debug.Log($"[LootTableCreator] 기존 LootTable 덮어쓰기: {path}");
            }

            return lootTable;
        }

        /// <summary>
        /// LootTable 에셋 삭제
        /// </summary>
        private void DeleteLootTableAsset(string fileName)
        {
            string path = $"{LootTablePath}{fileName}.asset";

            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[LootTableCreator] LootTable 삭제: {path}");
            }
        }

        /// <summary>
        /// Item 에셋 로드
        /// </summary>
        private Item LoadItem(string itemName)
        {
            string path = $"{ItemPath}{itemName}.asset";
            Item item = AssetDatabase.LoadAssetAtPath<Item>(path);

            if (item == null)
            {
                Debug.LogError($"[LootTableCreator] 아이템을 찾을 수 없습니다: {path}");
            }

            return item;
        }
    }
}
