#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Gameplay.NPC;

namespace Editor.Tools
{
    /// <summary>
    /// NPC 생성 도구 - EditorWindow (확장 버전)
    /// Menu: GASPT → NPC Creator Window
    /// </summary>
    public class NPCCreatorWindow : EditorWindow
    {
        #region 필드

        // NPC 설정
        private NPCType npcType = NPCType.Story;
        private string npcName = "";
        private Sprite npcSprite;
        private string episodeID = "";

        // UI
        private Vector2 scrollPosition;
        private Texture2D previewTexture;
        private const int PREVIEW_SIZE = 200;

        // 스타일
        private GUIStyle headerStyle;
        private GUIStyle previewBoxStyle;

        #endregion

        #region 윈도우 열기

        [MenuItem("GASPT/NPC Creator/Open Creator Window", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<NPCCreatorWindow>("NPC Creator");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }

        #endregion

        #region Unity 콜백

        private void OnEnable()
        {
            InitializeStyles();
            ResetFields();
        }

        private void OnGUI()
        {
            InitializeStyles();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawNPCTypeSelection();
            DrawBasicSettings();
            DrawSpriteSelection();
            DrawPreview();
            DrawEpisodeSettings();
            DrawCreateButton();

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region UI 그리기

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.8f, 0.8f, 1f) }
                };
            }

            if (previewBoxStyle == null)
            {
                previewBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("NPC 생성 도구", headerStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "스프라이트를 선택하여 커스텀 NPC를 생성할 수 있습니다.\n" +
                "미리보기를 확인하고 생성 버튼을 클릭하세요.",
                MessageType.Info
            );

            EditorGUILayout.Space(10);
        }

        private void DrawNPCTypeSelection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("NPC 타입", EditorStyles.boldLabel);

            npcType = (NPCType)EditorGUILayout.EnumPopup("타입", npcType);

            // 타입별 설명
            string description = npcType == NPCType.Story
                ? "스토리를 전달하는 NPC입니다. 대화를 통해 이야기를 진행합니다."
                : "아이템과 스킬을 판매하는 NPC입니다. 상점 기능을 제공합니다.";

            EditorGUILayout.HelpBox(description, MessageType.None);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawBasicSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("기본 설정", EditorStyles.boldLabel);

            npcName = EditorGUILayout.TextField("NPC 이름", npcName);

            if (string.IsNullOrWhiteSpace(npcName))
            {
                EditorGUILayout.HelpBox("NPC 이름을 입력해주세요.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawSpriteSelection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("스프라이트 선택", EditorStyles.boldLabel);

            Sprite newSprite = (Sprite)EditorGUILayout.ObjectField(
                "스프라이트",
                npcSprite,
                typeof(Sprite),
                false
            );

            if (newSprite != npcSprite)
            {
                npcSprite = newSprite;
                UpdatePreview();
            }

            if (npcSprite == null)
            {
                EditorGUILayout.HelpBox(
                    "스프라이트를 선택하지 않으면 기본 색상 스프라이트가 생성됩니다.\n" +
                    $"기본 색상: {(npcType == NPCType.Story ? "파란색" : "주황색")}",
                    MessageType.Info
                );
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawPreview()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("미리보기", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (npcSprite != null && previewTexture != null)
            {
                // 스프라이트 미리보기
                GUILayout.Box(previewTexture, previewBoxStyle, GUILayout.Width(PREVIEW_SIZE), GUILayout.Height(PREVIEW_SIZE));
            }
            else
            {
                // 기본 색상 미리보기
                Color previewColor = npcType == NPCType.Story
                    ? new Color(0.3f, 0.6f, 0.9f)
                    : new Color(0.9f, 0.6f, 0.2f);

                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = previewColor;
                GUILayout.Box("기본 스프라이트\n(단색)", previewBoxStyle, GUILayout.Width(PREVIEW_SIZE), GUILayout.Height(PREVIEW_SIZE));
                GUI.backgroundColor = oldColor;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawEpisodeSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("대화 설정", EditorStyles.boldLabel);

            episodeID = EditorGUILayout.TextField("Episode ID", episodeID);

            if (string.IsNullOrWhiteSpace(episodeID))
            {
                string defaultEpisode = npcType == NPCType.Story ? "EP_STORY_001" : "EP_SHOP_001";
                EditorGUILayout.HelpBox(
                    $"Episode ID를 입력하지 않으면 기본값이 사용됩니다.\n기본값: {defaultEpisode}",
                    MessageType.Info
                );
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawCreateButton()
        {
            EditorGUILayout.Space(10);

            bool canCreate = !string.IsNullOrWhiteSpace(npcName);

            GUI.enabled = canCreate;

            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);

            if (GUILayout.Button("NPC 생성", GUILayout.Height(50)))
            {
                CreateCustomNPC();
            }

            GUI.backgroundColor = oldColor;
            GUI.enabled = true;

            if (!canCreate)
            {
                EditorGUILayout.HelpBox("NPC 이름을 입력해야 생성할 수 있습니다.", MessageType.Warning);
            }
        }

        #endregion

        #region 미리보기 업데이트

        private void UpdatePreview()
        {
            if (npcSprite == null)
            {
                previewTexture = null;
                return;
            }

            // Sprite를 Texture2D로 변환
            Texture2D spriteTexture = npcSprite.texture;
            Rect spriteRect = npcSprite.rect;

            // 미리보기 텍스처 생성
            previewTexture = new Texture2D((int)spriteRect.width, (int)spriteRect.height);
            Color[] pixels = spriteTexture.GetPixels(
                (int)spriteRect.x,
                (int)spriteRect.y,
                (int)spriteRect.width,
                (int)spriteRect.height
            );
            previewTexture.SetPixels(pixels);
            previewTexture.Apply();
        }

        #endregion

        #region NPC 생성

        private void CreateCustomNPC()
        {
            if (string.IsNullOrWhiteSpace(npcName))
            {
                EditorUtility.DisplayDialog("오류", "NPC 이름을 입력해주세요.", "확인");
                return;
            }

            EditorUtility.DisplayProgressBar("NPC 생성", $"{npcName} NPC 생성 중...", 0.5f);

            try
            {
                // 1. 폴더 확인
                NPCCreatorHelper.EnsureDirectories();

                // 2. NPCData 생성
                NPCData data = CreateNPCData();
                if (data == null)
                {
                    EditorUtility.DisplayDialog("오류", "NPCData 생성 실패", "확인");
                    return;
                }

                // 3. Prefab 생성
                GameObject prefab = CreateNPCPrefab(data);
                if (prefab == null)
                {
                    EditorUtility.DisplayDialog("오류", "Prefab 생성 실패", "확인");
                    return;
                }

                // 4. 강제 갱신
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                // 5. 선택 및 표시
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);

                Debug.Log($"[NPCCreatorWindow] {npcName} NPC 생성 완료!");
                EditorUtility.DisplayDialog("생성 완료",
                    $"{npcName} NPC가 생성되었습니다.\n\n" +
                    $"타입: {npcType}\n" +
                    $"Episode ID: {(string.IsNullOrWhiteSpace(episodeID) ? "기본값" : episodeID)}",
                    "확인");

                // 필드 리셋
                ResetFields();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private NPCData CreateNPCData()
        {
            string sanitizedName = npcName.Replace(" ", "_");
            string typeName = npcType == NPCType.Story ? "Story" : "Shop";
            string path = $"{NPCCreatorHelper.DATA_PATH}/{typeName}NPC_{sanitizedName}Data.asset";

            // 기존 파일 확인
            NPCData existing = AssetDatabase.LoadAssetAtPath<NPCData>(path);
            if (existing != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "기존 파일 발견",
                    $"{path}\n파일이 이미 존재합니다. 덮어쓰시겠습니까?",
                    "덮어쓰기",
                    "취소"
                );

                if (!overwrite)
                    return null;

                AssetDatabase.DeleteAsset(path);
            }

            // NPCData 생성
            NPCData data = ScriptableObject.CreateInstance<NPCData>();
            data.npcName = npcName;
            data.npcType = npcType;

            // Episode ID 설정
            if (string.IsNullOrWhiteSpace(episodeID))
            {
                episodeID = npcType == NPCType.Story ? "EP_STORY_001" : "EP_SHOP_001";
            }
            data.episodeIDs.Add(episodeID);

            // 기본 설정
            data.interactionRange = 2f;
            data.interactionKey = KeyCode.E;
            data.showInteractionPrompt = true;
            data.interactionPromptText = npcType == NPCType.Story ? "E를 눌러 대화하기" : "E를 눌러 상점 열기";

            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();

            Debug.Log($"[NPCCreatorWindow] NPCData 생성: {path}");
            return data;
        }

        private GameObject CreateNPCPrefab(NPCData data)
        {
            string sanitizedName = npcName.Replace(" ", "_");
            string typeName = npcType == NPCType.Story ? "Story" : "Shop";
            string path = $"{NPCCreatorHelper.PREFAB_PATH}/{typeName}NPC_{sanitizedName}.prefab";

            // 기존 프리팹 삭제
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            // GameObject 생성
            GameObject obj = new GameObject($"{typeName}NPC_{sanitizedName}");

            // SpriteRenderer
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            if (npcSprite != null)
            {
                sr.sprite = npcSprite;
            }
            else
            {
                // 기본 스프라이트 생성
                Color defaultColor = npcType == NPCType.Story
                    ? new Color(0.3f, 0.6f, 0.9f)
                    : new Color(0.9f, 0.6f, 0.2f);
                sr.sprite = NPCCreatorHelper.CreateTempSprite(defaultColor);
            }
            sr.sortingOrder = 10;

            // BoxCollider2D
            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1.5f);
            col.offset = new Vector2(0f, 0.75f);
            col.isTrigger = true;

            // NPC 컴포넌트
            if (npcType == NPCType.Story)
            {
                StoryNPC npc = obj.AddComponent<StoryNPC>();
                SerializedObject so = new SerializedObject(npc);
                so.FindProperty("npcData").objectReferenceValue = data;
                so.FindProperty("showDebugLog").boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                ShopNPC npc = obj.AddComponent<ShopNPC>();
                SerializedObject so = new SerializedObject(npc);
                so.FindProperty("npcData").objectReferenceValue = data;
                so.FindProperty("showDebugLog").boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Prefab 저장
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);

            // Scene에서 제거
            Object.DestroyImmediate(obj);

            Debug.Log($"[NPCCreatorWindow] Prefab 생성: {path}");
            return prefab;
        }

        #endregion

        #region Helper

        private void ResetFields()
        {
            npcType = NPCType.Story;
            npcName = "";
            npcSprite = null;
            episodeID = "";
            previewTexture = null;
        }

        #endregion
    }
}
#endif
