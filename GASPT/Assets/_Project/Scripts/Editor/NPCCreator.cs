#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using Gameplay.NPC;

namespace Editor.Tools
{
    /// <summary>
    /// NPC 생성 도구 - 빠른 생성 (MenuItem)
    /// Menu: GASPT → NPC Creator
    /// </summary>
    public static class NPCCreator
    {
        #region 메뉴 항목

        [MenuItem("GASPT/NPC Creator/Create StoryNPC (마을사람)", priority = 1)]
        private static void CreateStoryNPC()
        {
            EditorUtility.DisplayProgressBar("NPC 생성", "StoryNPC 생성 중...", 0.5f);

            try
            {
                // 1. 폴더 생성
                NPCCreatorHelper.EnsureDirectories();

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
                    $"Data: {NPCCreatorHelper.DATA_PATH}\n" +
                    $"Prefab: {NPCCreatorHelper.PREFAB_PATH}",
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
                NPCCreatorHelper.EnsureDirectories();

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
                    $"Data: {NPCCreatorHelper.DATA_PATH}\n" +
                    $"Prefab: {NPCCreatorHelper.PREFAB_PATH}",
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
            NPCCreatorHelper.EnsureDirectories();
            EditorUtility.RevealInFinder(NPCCreatorHelper.PREFAB_PATH);
        }

        #endregion

        #region NPCData 생성

        private static NPCData CreateStoryNPCData()
        {
            string path = $"{NPCCreatorHelper.DATA_PATH}/StoryNPC_VillagerData.asset";

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
            string path = $"{NPCCreatorHelper.DATA_PATH}/ShopNPC_MerchantData.asset";

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
            string path = $"{NPCCreatorHelper.PREFAB_PATH}/StoryNPC_Villager.prefab";

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
            sr.sprite = NPCCreatorHelper.CreateTempSprite(new Color(0.3f, 0.6f, 0.9f)); // 파란색
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
            string path = $"{NPCCreatorHelper.PREFAB_PATH}/ShopNPC_Merchant.prefab";

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
            sr.sprite = NPCCreatorHelper.CreateTempSprite(new Color(0.9f, 0.6f, 0.2f)); // 주황색
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
    }
}
#endif
