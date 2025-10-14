#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using Gameplay.NPC;

namespace Editor.Tools
{
    /// <summary>
    /// NPC 생성 도구 (간소화 버전)
    /// Menu: GASPT → NPC Creator
    /// </summary>
    public static class NPCCreator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/NPC";
        private const string DATA_PATH = "Assets/_Project/Resources/NPCData";

        #region 메뉴 항목

        [MenuItem("GASPT/NPC Creator/Create StoryNPC (마을사람)", priority = 1)]
        private static void CreateStoryNPC()
        {
            EditorUtility.DisplayProgressBar("NPC 생성", "StoryNPC 생성 중...", 0.5f);

            try
            {
                // 1. 폴더 생성
                EnsureDirectories();

                // 2. NPCData 생성
                var data = CreateStoryNPCData();
                if (data == null)
                {
                    EditorUtility.DisplayDialog("오류", "NPCData 생성 실패", "확인");
                    return;
                }

                // 3. Prefab 생성
                var prefab = CreateStoryNPCPrefab(data);
                if (prefab == null)
                {
                    EditorUtility.DisplayDialog("오류", "Prefab 생성 실패", "확인");
                    return;
                }

                // 4. 에셋 데이터베이스 강제 갱신
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                // 5. 생성된 프리팹 선택 및 표시
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);

                Debug.Log($"[NPCCreator] StoryNPC 생성 완료!");
                EditorUtility.DisplayDialog("생성 완료",
                    "StoryNPC가 생성되었습니다.\n" +
                    $"Data: {DATA_PATH}\n" +
                    $"Prefab: {PREFAB_PATH}",
                    "확인");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("GASPT/NPC Creator/Create ShopNPC (상인)", priority = 2)]
        private static void CreateShopNPC()
        {
            EditorUtility.DisplayProgressBar("NPC 생성", "ShopNPC 생성 중...", 0.5f);

            try
            {
                // 1. 폴더 생성
                EnsureDirectories();

                // 2. NPCData 생성
                var data = CreateShopNPCData();
                if (data == null)
                {
                    EditorUtility.DisplayDialog("오류", "NPCData 생성 실패", "확인");
                    return;
                }

                // 3. Prefab 생성
                var prefab = CreateShopNPCPrefab(data);
                if (prefab == null)
                {
                    EditorUtility.DisplayDialog("오류", "Prefab 생성 실패", "확인");
                    return;
                }

                // 4. 에셋 데이터베이스 강제 갱신
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                // 5. 생성된 프리팹 선택 및 표시
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);

                Debug.Log($"[NPCCreator] ShopNPC 생성 완료!");
                EditorUtility.DisplayDialog("생성 완료",
                    "ShopNPC가 생성되었습니다.\n" +
                    $"Data: {DATA_PATH}\n" +
                    $"Prefab: {PREFAB_PATH}",
                    "확인");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("GASPT/NPC Creator/Create All NPCs", priority = 10)]
        private static void CreateAllNPCs()
        {
            if (!EditorUtility.DisplayDialog(
                "NPC 생성",
                "모든 NPC를 생성하시겠습니까?",
                "생성",
                "취소"))
            {
                return;
            }

            CreateStoryNPC();
            CreateShopNPC();
        }

        [MenuItem("GASPT/NPC Creator/Open NPC Folder", priority = 20)]
        private static void OpenNPCFolder()
        {
            EnsureDirectories();
            EditorUtility.RevealInFinder(PREFAB_PATH);
        }

        #endregion

        #region NPCData 생성

        private static NPCData CreateStoryNPCData()
        {
            string path = $"{DATA_PATH}/StoryNPC_VillagerData.asset";

            // 기존 파일 확인
            NPCData existing = AssetDatabase.LoadAssetAtPath<NPCData>(path);
            if (existing != null)
            {
                Debug.Log($"[NPCCreator] 기존 StoryNPC Data 사용: {path}");
                return existing;
            }

            // 새로 생성
            NPCData data = ScriptableObject.CreateInstance<NPCData>();
            data.npcName = "마을사람";
            data.npcType = NPCType.Story;
            data.episodeIDs.Add("EP_STORY_001");
            data.episodeIDs.Add("EP_STORY_002");
            data.interactionRange = 2f;
            data.interactionKey = KeyCode.E;
            data.showInteractionPrompt = true;
            data.interactionPromptText = "E를 눌러 대화하기";

            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();

            Debug.Log($"[NPCCreator] StoryNPC Data 생성: {path}");
            return data;
        }

        private static NPCData CreateShopNPCData()
        {
            string path = $"{DATA_PATH}/ShopNPC_MerchantData.asset";

            // 기존 파일 확인
            NPCData existing = AssetDatabase.LoadAssetAtPath<NPCData>(path);
            if (existing != null)
            {
                Debug.Log($"[NPCCreator] 기존 ShopNPC Data 사용: {path}");
                return existing;
            }

            // 새로 생성
            NPCData data = ScriptableObject.CreateInstance<NPCData>();
            data.npcName = "상인";
            data.npcType = NPCType.Shop;
            data.episodeIDs.Add("EP_SHOP_001");
            data.interactionRange = 2f;
            data.interactionKey = KeyCode.E;
            data.showInteractionPrompt = true;
            data.interactionPromptText = "E를 눌러 상점 열기";

            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();

            Debug.Log($"[NPCCreator] ShopNPC Data 생성: {path}");
            return data;
        }

        #endregion

        #region Prefab 생성

        private static GameObject CreateStoryNPCPrefab(NPCData data)
        {
            string path = $"{PREFAB_PATH}/StoryNPC_Villager.prefab";

            // 기존 프리팹 삭제
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[NPCCreator] 기존 StoryNPC Prefab 삭제: {path}");
            }

            // GameObject 생성
            GameObject obj = new GameObject("StoryNPC_Villager");

            // SpriteRenderer
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateTempSprite(new Color(0.3f, 0.6f, 0.9f)); // 파란색
            sr.sortingOrder = 10;

            // BoxCollider2D
            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1.5f);
            col.offset = new Vector2(0f, 0.75f);
            col.isTrigger = true;

            // StoryNPC 컴포넌트
            StoryNPC npc = obj.AddComponent<StoryNPC>();

            // SerializedObject로 NPCData 연결
            SerializedObject so = new SerializedObject(npc);
            so.FindProperty("npcData").objectReferenceValue = data;
            so.FindProperty("showDebugLog").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Prefab 저장
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);

            // Scene에서 제거
            Object.DestroyImmediate(obj);

            Debug.Log($"[NPCCreator] StoryNPC Prefab 생성: {path}");
            return prefab;
        }

        private static GameObject CreateShopNPCPrefab(NPCData data)
        {
            string path = $"{PREFAB_PATH}/ShopNPC_Merchant.prefab";

            // 기존 프리팹 삭제
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[NPCCreator] 기존 ShopNPC Prefab 삭제: {path}");
            }

            // GameObject 생성
            GameObject obj = new GameObject("ShopNPC_Merchant");

            // SpriteRenderer
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateTempSprite(new Color(0.9f, 0.6f, 0.2f)); // 주황색
            sr.sortingOrder = 10;

            // BoxCollider2D
            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1.5f);
            col.offset = new Vector2(0f, 0.75f);
            col.isTrigger = true;

            // ShopNPC 컴포넌트
            ShopNPC npc = obj.AddComponent<ShopNPC>();

            // SerializedObject로 NPCData 연결
            SerializedObject so = new SerializedObject(npc);
            so.FindProperty("npcData").objectReferenceValue = data;
            so.FindProperty("showDebugLog").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Prefab 저장
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);

            // Scene에서 제거
            Object.DestroyImmediate(obj);

            Debug.Log($"[NPCCreator] ShopNPC Prefab 생성: {path}");
            return prefab;
        }

        #endregion

        #region Helper Methods

        private static void EnsureDirectories()
        {
            // Prefab 폴더
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                Debug.Log($"[NPCCreator] 폴더 생성: {PREFAB_PATH}");
            }

            // NPCData 폴더
            if (!Directory.Exists(DATA_PATH))
            {
                Directory.CreateDirectory(DATA_PATH);
                Debug.Log($"[NPCCreator] 폴더 생성: {DATA_PATH}");
            }

            AssetDatabase.Refresh();
        }

        private static Sprite CreateTempSprite(Color color)
        {
            Texture2D tex = new Texture2D(32, 48);
            Color[] pixels = new Color[32 * 48];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(
                tex,
                new Rect(0, 0, 32, 48),
                new Vector2(0.5f, 0f), // Pivot at bottom center
                32f
            );
        }

        #endregion
    }
}
#endif
