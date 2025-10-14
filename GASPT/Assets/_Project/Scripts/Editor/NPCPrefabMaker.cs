#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using Gameplay.NPC;

namespace Editor.Tools
{
    /// <summary>
    /// NPC Prefab 자동 생성 도구
    /// Menu: GASPT → NPC → Create NPC Prefabs
    /// </summary>
    public class NPCPrefabMaker : EditorWindow
    {
        private const string PREFAB_SAVE_PATH = "Assets/_Project/Prefabs/NPC/";
        private const string NPCDATA_SAVE_PATH = "Assets/_Project/Resources/NPCData/";

        private Vector2 scrollPosition;

        [MenuItem("GASPT/NPC/Open NPC Prefab Maker")]
        public static void ShowWindow()
        {
            var window = GetWindow<NPCPrefabMaker>("NPC Prefab Maker");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawButtons();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("NPC Prefab 자동 생성 도구", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "NPC 프리팹과 NPCData ScriptableObject를 자동으로 생성합니다.\n" +
                "- StoryNPC: 스토리를 전달하는 NPC\n" +
                "- ShopNPC: 아이템/스킬을 판매하는 NPC",
                MessageType.Info
            );
            EditorGUILayout.Space(10);
        }

        private void DrawButtons()
        {
            EditorGUILayout.LabelField("NPCData 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("StoryNPC Data 생성 (마을사람)", GUILayout.Height(40)))
            {
                CreateStoryNPCData();
            }

            if (GUILayout.Button("ShopNPC Data 생성 (상인)", GUILayout.Height(40)))
            {
                CreateShopNPCData();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("NPC Prefab 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("StoryNPC Prefab 생성 (마을사람)", GUILayout.Height(40)))
            {
                CreateStoryNPCPrefab();
            }

            if (GUILayout.Button("ShopNPC Prefab 생성 (상인)", GUILayout.Height(40)))
            {
                CreateShopNPCPrefab();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("모든 NPC 생성 (Data + Prefab)", GUILayout.Height(50)))
            {
                CreateAllNPCs();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Prefab 폴더 열기", GUILayout.Height(30)))
            {
                EditorUtility.RevealInFinder(PREFAB_SAVE_PATH);
            }
        }

        #region NPCData 생성

        /// <summary>
        /// StoryNPC용 NPCData 생성
        /// </summary>
        private void CreateStoryNPCData()
        {
            EnsureFolderExists(NPCDATA_SAVE_PATH);

            NPCData npcData = ScriptableObject.CreateInstance<NPCData>();
            npcData.npcName = "마을사람";
            npcData.npcType = NPCType.Story;
            npcData.episodeIDs.Add("EP_STORY_001");
            npcData.episodeIDs.Add("EP_STORY_002");
            npcData.interactionRange = 2f;
            npcData.interactionKey = KeyCode.E;
            npcData.showInteractionPrompt = true;
            npcData.interactionPromptText = "E를 눌러 대화하기";

            string assetPath = $"{NPCDATA_SAVE_PATH}StoryNPC_VillagerData.asset";
            AssetDatabase.CreateAsset(npcData, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[NPCPrefabMaker] StoryNPC Data 생성 완료: {assetPath}");
            EditorUtility.DisplayDialog("생성 완료", $"StoryNPC Data가 생성되었습니다.\n{assetPath}", "확인");
        }

        /// <summary>
        /// ShopNPC용 NPCData 생성
        /// </summary>
        private void CreateShopNPCData()
        {
            EnsureFolderExists(NPCDATA_SAVE_PATH);

            NPCData npcData = ScriptableObject.CreateInstance<NPCData>();
            npcData.npcName = "상인";
            npcData.npcType = NPCType.Shop;
            npcData.episodeIDs.Add("EP_SHOP_001");
            npcData.interactionRange = 2f;
            npcData.interactionKey = KeyCode.E;
            npcData.showInteractionPrompt = true;
            npcData.interactionPromptText = "E를 눌러 상점 열기";

            string assetPath = $"{NPCDATA_SAVE_PATH}ShopNPC_MerchantData.asset";
            AssetDatabase.CreateAsset(npcData, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[NPCPrefabMaker] ShopNPC Data 생성 완료: {assetPath}");
            EditorUtility.DisplayDialog("생성 완료", $"ShopNPC Data가 생성되었습니다.\n{assetPath}", "확인");
        }

        #endregion

        #region NPC Prefab 생성

        /// <summary>
        /// StoryNPC Prefab 생성
        /// </summary>
        private void CreateStoryNPCPrefab()
        {
            EnsureFolderExists(PREFAB_SAVE_PATH);

            // NPCData 로드
            NPCData npcData = AssetDatabase.LoadAssetAtPath<NPCData>($"{NPCDATA_SAVE_PATH}StoryNPC_VillagerData.asset");
            if (npcData == null)
            {
                Debug.LogWarning("[NPCPrefabMaker] StoryNPC Data를 먼저 생성해주세요.");
                EditorUtility.DisplayDialog("경고", "StoryNPC Data를 먼저 생성해주세요.", "확인");
                return;
            }

            // NPC GameObject 생성
            GameObject npcObj = new GameObject("StoryNPC_Villager");

            // SpriteRenderer 추가
            SpriteRenderer spriteRenderer = npcObj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateDefaultSprite(new Color(0.3f, 0.6f, 0.9f)); // 파란색
            spriteRenderer.sortingLayerName = "Default";
            spriteRenderer.sortingOrder = 10;

            // BoxCollider2D 추가 (Trigger)
            BoxCollider2D collider = npcObj.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1.5f);
            collider.offset = new Vector2(0f, 0.75f);
            collider.isTrigger = true;

            // StoryNPC 컴포넌트 추가
            StoryNPC storyNPC = npcObj.AddComponent<StoryNPC>();

            // NPCData 연결 (SerializedObject 사용)
            SerializedObject so = new SerializedObject(storyNPC);
            so.FindProperty("npcData").objectReferenceValue = npcData;
            so.FindProperty("showDebugLog").boolValue = true;
            so.ApplyModifiedProperties();

            // Prefab 저장
            string prefabPath = $"{PREFAB_SAVE_PATH}StoryNPC_Villager.prefab";
            PrefabUtility.SaveAsPrefabAsset(npcObj, prefabPath);

            // Hierarchy에서 삭제
            DestroyImmediate(npcObj);

            Debug.Log($"[NPCPrefabMaker] StoryNPC Prefab 생성 완료: {prefabPath}");
            EditorUtility.DisplayDialog("생성 완료", $"StoryNPC Prefab이 생성되었습니다.\n{prefabPath}", "확인");

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// ShopNPC Prefab 생성
        /// </summary>
        private void CreateShopNPCPrefab()
        {
            EnsureFolderExists(PREFAB_SAVE_PATH);

            // NPCData 로드
            NPCData npcData = AssetDatabase.LoadAssetAtPath<NPCData>($"{NPCDATA_SAVE_PATH}ShopNPC_MerchantData.asset");
            if (npcData == null)
            {
                Debug.LogWarning("[NPCPrefabMaker] ShopNPC Data를 먼저 생성해주세요.");
                EditorUtility.DisplayDialog("경고", "ShopNPC Data를 먼저 생성해주세요.", "확인");
                return;
            }

            // NPC GameObject 생성
            GameObject npcObj = new GameObject("ShopNPC_Merchant");

            // SpriteRenderer 추가
            SpriteRenderer spriteRenderer = npcObj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateDefaultSprite(new Color(0.9f, 0.6f, 0.2f)); // 주황색
            spriteRenderer.sortingLayerName = "Default";
            spriteRenderer.sortingOrder = 10;

            // BoxCollider2D 추가 (Trigger)
            BoxCollider2D collider = npcObj.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1.5f);
            collider.offset = new Vector2(0f, 0.75f);
            collider.isTrigger = true;

            // ShopNPC 컴포넌트 추가
            ShopNPC shopNPC = npcObj.AddComponent<ShopNPC>();

            // NPCData 연결 (SerializedObject 사용)
            SerializedObject so = new SerializedObject(shopNPC);
            so.FindProperty("npcData").objectReferenceValue = npcData;
            so.FindProperty("showDebugLog").boolValue = true;
            so.ApplyModifiedProperties();

            // Prefab 저장
            string prefabPath = $"{PREFAB_SAVE_PATH}ShopNPC_Merchant.prefab";
            PrefabUtility.SaveAsPrefabAsset(npcObj, prefabPath);

            // Hierarchy에서 삭제
            DestroyImmediate(npcObj);

            Debug.Log($"[NPCPrefabMaker] ShopNPC Prefab 생성 완료: {prefabPath}");
            EditorUtility.DisplayDialog("생성 완료", $"ShopNPC Prefab이 생성되었습니다.\n{prefabPath}", "확인");

            AssetDatabase.Refresh();
        }

        #endregion

        #region 전체 생성

        /// <summary>
        /// 모든 NPC 생성 (Data + Prefab)
        /// </summary>
        private void CreateAllNPCs()
        {
            if (!EditorUtility.DisplayDialog(
                "NPC 생성 확인",
                "모든 NPC Data와 Prefab을 생성하시겠습니까?\n기존 파일이 있으면 덮어씁니다.",
                "생성",
                "취소"))
            {
                return;
            }

            Debug.Log("[NPCPrefabMaker] 모든 NPC 생성 시작...");

            // NPCData 생성
            CreateStoryNPCData();
            CreateShopNPCData();

            // Prefab 생성
            CreateStoryNPCPrefab();
            CreateShopNPCPrefab();

            Debug.Log("[NPCPrefabMaker] 모든 NPC 생성 완료!");
            EditorUtility.DisplayDialog(
                "생성 완료",
                "모든 NPC Data와 Prefab이 생성되었습니다.\n\n" +
                $"NPCData: {NPCDATA_SAVE_PATH}\n" +
                $"Prefab: {PREFAB_SAVE_PATH}",
                "확인"
            );
        }

        #endregion

        #region Quick Menu

        [MenuItem("GASPT/NPC/Create All NPCs")]
        private static void QuickCreateAllNPCs()
        {
            var maker = CreateInstance<NPCPrefabMaker>();
            maker.CreateAllNPCs();
        }

        [MenuItem("GASPT/NPC/Create StoryNPC Data")]
        private static void QuickCreateStoryNPCData()
        {
            var maker = CreateInstance<NPCPrefabMaker>();
            maker.CreateStoryNPCData();
        }

        [MenuItem("GASPT/NPC/Create ShopNPC Data")]
        private static void QuickCreateShopNPCData()
        {
            var maker = CreateInstance<NPCPrefabMaker>();
            maker.CreateShopNPCData();
        }

        [MenuItem("GASPT/NPC/Open NPC Folder")]
        private static void OpenNPCFolder()
        {
            if (!Directory.Exists(PREFAB_SAVE_PATH))
            {
                Directory.CreateDirectory(PREFAB_SAVE_PATH);
                AssetDatabase.Refresh();
            }

            EditorUtility.RevealInFinder(PREFAB_SAVE_PATH);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 폴더 존재 확인 및 생성
        /// </summary>
        private void EnsureFolderExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentFolder = Path.GetDirectoryName(path).Replace("\\", "/");
                string folderName = Path.GetFileName(path);

                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    Directory.CreateDirectory(parentFolder);
                }

                AssetDatabase.CreateFolder(parentFolder, folderName);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 기본 스프라이트 생성 (단색 사각형)
        /// </summary>
        private Sprite CreateDefaultSprite(Color color)
        {
            Texture2D texture = new Texture2D(32, 48);
            Color[] pixels = new Color[32 * 48];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 32, 48),
                new Vector2(0.5f, 0f), // Pivot at bottom center
                32f
            );

            return sprite;
        }

        #endregion
    }
}
#endif
