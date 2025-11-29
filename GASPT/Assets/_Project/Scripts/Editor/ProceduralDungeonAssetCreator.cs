#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using GASPT.Gameplay.Level;
using GASPT.UI.Minimap;
using GASPT.UI.MVP;

namespace GASPT.EditorTools
{
    /// <summary>
    /// 절차적 던전 시스템 에셋 자동 생성 에디터 도구
    /// </summary>
    public class ProceduralDungeonAssetCreator : EditorWindow
    {
        // ====== 경로 ======

        private const string PrefabUIPath = "Assets/Resources/Prefabs/UI/";
        private const string DataUIPath = "Assets/Resources/Data/UI/";
        private const string DataStagesPath = "Assets/Resources/Data/Stages/";
        private const string DataDungeonsPath = "Assets/Resources/Data/Dungeons/";


        // ====== 에디터 창 ======

        [MenuItem("Tools/GASPT/Procedural Dungeon/Create All Assets")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProceduralDungeonAssetCreator>("Procedural Dungeon Assets");
            window.minSize = new Vector2(450f, 600f);
            window.Show();
        }


        // ====== GUI ======

        private Vector2 scrollPosition;

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10f);

            // 타이틀
            EditorGUILayout.LabelField("Procedural Dungeon Asset Creator", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("절차적 레벨 생성 시스템 에셋 자동 생성", EditorStyles.miniLabel);

            GUILayout.Space(10f);
            EditorGUILayout.HelpBox(
                "절차적 던전 시스템에 필요한 에셋들을 자동으로 생성합니다.\n\n" +
                "1. BranchSelectionUI.prefab - 분기 선택 UI\n" +
                "2. MinimapUI.prefab - 미니맵 UI\n" +
                "3. MinimapConfig.asset - 미니맵 설정\n" +
                "4. Stage1~5 Config - 5개 스테이지 설정\n" +
                "5. TestDungeon.asset - 테스트용 던전 설정",
                MessageType.Info
            );

            GUILayout.Space(20f);

            // 전체 생성 버튼
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("🎨 모든 에셋 생성", GUILayout.Height(40f)))
            {
                CreateAllAssets();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(20f);

            // 개별 생성 섹션
            EditorGUILayout.LabelField("═══ UI Prefabs ═══", EditorStyles.boldLabel);
            GUILayout.Space(5f);

            if (GUILayout.Button("1. BranchSelectionUI.prefab 생성"))
            {
                CreateBranchSelectionUIPrefab();
            }

            if (GUILayout.Button("2. MinimapUI.prefab 생성"))
            {
                CreateMinimapUIPrefab();
            }

            GUILayout.Space(15f);
            EditorGUILayout.LabelField("═══ ScriptableObjects ═══", EditorStyles.boldLabel);
            GUILayout.Space(5f);

            if (GUILayout.Button("3. MinimapConfig.asset 생성"))
            {
                CreateMinimapConfig();
            }

            if (GUILayout.Button("4. StageConfig 5개 생성"))
            {
                CreateAllStageConfigs();
            }

            if (GUILayout.Button("5. TestDungeon.asset 생성"))
            {
                CreateTestDungeonConfig();
            }

            GUILayout.Space(20f);
            EditorGUILayout.LabelField("═══ 폴더 경로 ═══", EditorStyles.boldLabel);
            GUILayout.Space(5f);

            EditorGUILayout.HelpBox(
                $"UI Prefabs: {PrefabUIPath}\n" +
                $"UI Data: {DataUIPath}\n" +
                $"Stages: {DataStagesPath}\n" +
                $"Dungeons: {DataDungeonsPath}",
                MessageType.None
            );

            EditorGUILayout.EndScrollView();
        }


        // ====== 전체 생성 ======

        private void CreateAllAssets()
        {
            EnsureDirectoriesExist();

            CreateBranchSelectionUIPrefab();
            CreateMinimapUIPrefab();
            CreateMinimapConfig();
            CreateAllStageConfigs();
            CreateTestDungeonConfig();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ProceduralDungeonAssetCreator] 모든 에셋 생성 완료!");
            EditorUtility.DisplayDialog("완료", "모든 절차적 던전 에셋이 생성되었습니다!", "확인");
        }

        private void EnsureDirectoriesExist()
        {
            CreateDirectoryIfNotExists(PrefabUIPath);
            CreateDirectoryIfNotExists(DataUIPath);
            CreateDirectoryIfNotExists(DataStagesPath);
            CreateDirectoryIfNotExists(DataDungeonsPath);
        }

        private void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"[ProceduralDungeonAssetCreator] 폴더 생성: {path}");
            }
        }


        // ====== 1. BranchSelectionUI Prefab ======

        private void CreateBranchSelectionUIPrefab()
        {
            EnsureDirectoriesExist();

            string prefabPath = PrefabUIPath + "BranchSelectionUI.prefab";

            // 이미 존재하면 스킵
            if (File.Exists(prefabPath))
            {
                Debug.LogWarning($"[ProceduralDungeonAssetCreator] 이미 존재: {prefabPath}");
                return;
            }

            // Root Panel
            GameObject root = new GameObject("BranchSelectionUI");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;

            // BranchSelectionView 컴포넌트
            var view = root.AddComponent<BranchSelectionView>();

            // Background Dim
            GameObject bgDim = CreateUIElement("BackgroundDim", root.transform);
            var bgDimRect = bgDim.GetComponent<RectTransform>();
            bgDimRect.anchorMin = Vector2.zero;
            bgDimRect.anchorMax = Vector2.one;
            bgDimRect.sizeDelta = Vector2.zero;
            var bgDimImage = bgDim.AddComponent<Image>();
            bgDimImage.color = new Color(0f, 0f, 0f, 0.7f);

            // Panel
            GameObject panel = CreateUIElement("Panel", root.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600f, 400f);
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Title Text
            GameObject titleObj = CreateUIElement("TitleText", panel.transform);
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
            titleRect.sizeDelta = new Vector2(560f, 50f);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "다음 방을 선택하세요";
            titleText.fontSize = 28f;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            // Options Container
            GameObject optionsContainer = CreateUIElement("OptionsContainer", panel.transform);
            var optionsRect = optionsContainer.GetComponent<RectTransform>();
            optionsRect.anchorMin = new Vector2(0f, 0f);
            optionsRect.anchorMax = new Vector2(1f, 1f);
            optionsRect.offsetMin = new Vector2(20f, 80f);
            optionsRect.offsetMax = new Vector2(-20f, -80f);
            var hLayout = optionsContainer.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 20f;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;

            // Hint Text
            GameObject hintObj = CreateUIElement("HintText", panel.transform);
            var hintRect = hintObj.GetComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0f);
            hintRect.anchorMax = new Vector2(0.5f, 0f);
            hintRect.pivot = new Vector2(0.5f, 0f);
            hintRect.anchoredPosition = new Vector2(0f, 20f);
            hintRect.sizeDelta = new Vector2(560f, 40f);
            var hintText = hintObj.AddComponent<TextMeshProUGUI>();
            hintText.text = "← → 또는 A D 로 선택, Enter 또는 Space 로 확정";
            hintText.fontSize = 16f;
            hintText.alignment = TextAlignmentOptions.Center;
            hintText.color = new Color(0.7f, 0.7f, 0.7f);

            // SerializedObject로 참조 설정
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("rootPanel").objectReferenceValue = root;
            so.FindProperty("optionsContainer").objectReferenceValue = optionsContainer.transform;
            so.FindProperty("titleText").objectReferenceValue = titleText;
            so.FindProperty("hintText").objectReferenceValue = hintText;
            so.ApplyModifiedProperties();

            // Option Button Prefab 생성 및 연결
            CreateBranchOptionButtonPrefab();
            var optionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabUIPath + "BranchOptionButton.prefab");
            so.FindProperty("optionButtonPrefab").objectReferenceValue = optionPrefab;
            so.ApplyModifiedProperties();

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);

            Debug.Log($"[ProceduralDungeonAssetCreator] BranchSelectionUI 생성 완료: {prefabPath}");
        }

        private void CreateBranchOptionButtonPrefab()
        {
            string prefabPath = PrefabUIPath + "BranchOptionButton.prefab";

            if (File.Exists(prefabPath)) return;

            GameObject button = new GameObject("BranchOptionButton");
            var buttonRect = button.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(150f, 200f);

            var buttonImage = button.AddComponent<Image>();
            buttonImage.color = new Color(0.25f, 0.25f, 0.3f, 1f);

            var buttonComp = button.AddComponent<Button>();

            var layoutElement = button.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 150f;
            layoutElement.preferredHeight = 200f;

            // Icon
            GameObject iconObj = CreateUIElement("IconImage", button.transform);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = new Vector2(0f, -20f);
            iconRect.sizeDelta = new Vector2(60f, 60f);
            var iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // TypeName Text
            GameObject typeNameObj = CreateUIElement("TypeNameText", button.transform);
            var typeNameRect = typeNameObj.GetComponent<RectTransform>();
            typeNameRect.anchorMin = new Vector2(0.5f, 0.5f);
            typeNameRect.anchorMax = new Vector2(0.5f, 0.5f);
            typeNameRect.anchoredPosition = new Vector2(0f, 10f);
            typeNameRect.sizeDelta = new Vector2(140f, 40f);
            var typeNameText = typeNameObj.AddComponent<TextMeshProUGUI>();
            typeNameText.text = "전투";
            typeNameText.fontSize = 22f;
            typeNameText.alignment = TextAlignmentOptions.Center;
            typeNameText.color = Color.white;

            // Difficulty Text
            GameObject diffObj = CreateUIElement("DifficultyText", button.transform);
            var diffRect = diffObj.GetComponent<RectTransform>();
            diffRect.anchorMin = new Vector2(0.5f, 0f);
            diffRect.anchorMax = new Vector2(0.5f, 0f);
            diffRect.pivot = new Vector2(0.5f, 0f);
            diffRect.anchoredPosition = new Vector2(0f, 50f);
            diffRect.sizeDelta = new Vector2(140f, 30f);
            var diffText = diffObj.AddComponent<TextMeshProUGUI>();
            diffText.text = "보통";
            diffText.fontSize = 16f;
            diffText.alignment = TextAlignmentOptions.Center;
            diffText.color = new Color(0.8f, 0.8f, 0.8f);

            // Reward Text
            GameObject rewardObj = CreateUIElement("RewardText", button.transform);
            var rewardRect = rewardObj.GetComponent<RectTransform>();
            rewardRect.anchorMin = new Vector2(0.5f, 0f);
            rewardRect.anchorMax = new Vector2(0.5f, 0f);
            rewardRect.pivot = new Vector2(0.5f, 0f);
            rewardRect.anchoredPosition = new Vector2(0f, 20f);
            rewardRect.sizeDelta = new Vector2(140f, 25f);
            var rewardText = rewardObj.AddComponent<TextMeshProUGUI>();
            rewardText.text = "골드, 경험치";
            rewardText.fontSize = 14f;
            rewardText.alignment = TextAlignmentOptions.Center;
            rewardText.color = Color.yellow;

            // Background (선택 표시용)
            GameObject bgObj = CreateUIElement("Background", button.transform);
            bgObj.transform.SetAsFirstSibling();
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = new Vector2(10f, 10f);
            bgRect.anchoredPosition = Vector2.zero;
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(1f, 0.8f, 0.2f, 0f); // 기본 투명

            // BranchOptionButton 컴포넌트 추가
            var optionButton = button.AddComponent<BranchOptionButton>();
            SerializedObject so = new SerializedObject(optionButton);
            so.FindProperty("iconImage").objectReferenceValue = iconImage;
            so.FindProperty("typeNameText").objectReferenceValue = typeNameText;
            so.FindProperty("difficultyText").objectReferenceValue = diffText;
            so.FindProperty("rewardText").objectReferenceValue = rewardText;
            so.FindProperty("backgroundImage").objectReferenceValue = bgImage;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(button, prefabPath);
            DestroyImmediate(button);

            Debug.Log($"[ProceduralDungeonAssetCreator] BranchOptionButton 생성 완료: {prefabPath}");
        }


        // ====== 2. MinimapUI Prefab ======

        private void CreateMinimapUIPrefab()
        {
            EnsureDirectoriesExist();

            string prefabPath = PrefabUIPath + "MinimapUI.prefab";

            if (File.Exists(prefabPath))
            {
                Debug.LogWarning($"[ProceduralDungeonAssetCreator] 이미 존재: {prefabPath}");
                return;
            }

            // Root
            GameObject root = new GameObject("MinimapUI");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;

            // MinimapView 컴포넌트
            var view = root.AddComponent<MinimapView>();

            // Panel (메인 패널)
            GameObject panel = CreateUIElement("Panel", root.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-20f, -20f);
            panelRect.sizeDelta = new Vector2(250f, 300f);
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Map Container (줌/팬 적용)
            GameObject mapContainer = CreateUIElement("MapContainer", panel.transform);
            var mapContainerRect = mapContainer.GetComponent<RectTransform>();
            mapContainerRect.anchorMin = Vector2.zero;
            mapContainerRect.anchorMax = Vector2.one;
            mapContainerRect.offsetMin = new Vector2(10f, 10f);
            mapContainerRect.offsetMax = new Vector2(-10f, -10f);
            var mask = mapContainer.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            mapContainer.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.3f);

            // Edges Container
            GameObject edgesContainer = CreateUIElement("EdgesContainer", mapContainer.transform);
            var edgesRect = edgesContainer.GetComponent<RectTransform>();
            edgesRect.anchorMin = new Vector2(0.5f, 0.5f);
            edgesRect.anchorMax = new Vector2(0.5f, 0.5f);
            edgesRect.sizeDelta = new Vector2(400f, 600f);

            // Nodes Container
            GameObject nodesContainer = CreateUIElement("NodesContainer", mapContainer.transform);
            var nodesRect = nodesContainer.GetComponent<RectTransform>();
            nodesRect.anchorMin = new Vector2(0.5f, 0.5f);
            nodesRect.anchorMax = new Vector2(0.5f, 0.5f);
            nodesRect.sizeDelta = new Vector2(400f, 600f);

            // Node Prefab 생성
            CreateMinimapNodePrefab();
            CreateMinimapEdgePrefab();

            // SerializedObject로 참조 설정
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("rootPanel").objectReferenceValue = root;
            so.FindProperty("mapContainer").objectReferenceValue = mapContainerRect;
            so.FindProperty("nodesContainer").objectReferenceValue = nodesContainer.transform;
            so.FindProperty("edgesContainer").objectReferenceValue = edgesContainer.transform;

            var nodePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabUIPath + "MinimapNode.prefab");
            var edgePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabUIPath + "MinimapEdge.prefab");
            so.FindProperty("nodePrefab").objectReferenceValue = nodePrefab;
            so.FindProperty("edgePrefab").objectReferenceValue = edgePrefab;

            // MinimapConfig 로드
            var minimapConfig = AssetDatabase.LoadAssetAtPath<MinimapConfig>(DataUIPath + "MinimapConfig.asset");
            if (minimapConfig != null)
            {
                so.FindProperty("config").objectReferenceValue = minimapConfig;
            }

            so.ApplyModifiedProperties();

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);

            Debug.Log($"[ProceduralDungeonAssetCreator] MinimapUI 생성 완료: {prefabPath}");
        }

        private void CreateMinimapNodePrefab()
        {
            string prefabPath = PrefabUIPath + "MinimapNode.prefab";

            if (File.Exists(prefabPath)) return;

            GameObject node = new GameObject("MinimapNode");
            var nodeRect = node.AddComponent<RectTransform>();
            nodeRect.sizeDelta = new Vector2(24f, 24f);

            // Background
            GameObject bgObj = CreateUIElement("Background", node.transform);
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

            // Border
            GameObject borderObj = CreateUIElement("Border", node.transform);
            var borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = new Vector2(4f, 4f);
            var borderImage = borderObj.AddComponent<Image>();
            borderImage.color = Color.white;

            // Icon
            GameObject iconObj = CreateUIElement("Icon", node.transform);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(16f, 16f);
            var iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // Highlight
            GameObject highlightObj = CreateUIElement("Highlight", node.transform);
            var highlightRect = highlightObj.GetComponent<RectTransform>();
            highlightRect.anchorMin = Vector2.zero;
            highlightRect.anchorMax = Vector2.one;
            highlightRect.sizeDelta = new Vector2(8f, 8f);
            var highlightImage = highlightObj.AddComponent<Image>();
            highlightImage.color = new Color(1f, 1f, 0f, 0.5f);
            highlightObj.SetActive(false);

            // MinimapNodeUI 컴포넌트
            var nodeUI = node.AddComponent<MinimapNodeUI>();
            SerializedObject so = new SerializedObject(nodeUI);
            so.FindProperty("backgroundImage").objectReferenceValue = bgImage;
            so.FindProperty("iconImage").objectReferenceValue = iconImage;
            so.FindProperty("borderImage").objectReferenceValue = borderImage;
            so.FindProperty("highlightImage").objectReferenceValue = highlightImage;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(node, prefabPath);
            DestroyImmediate(node);

            Debug.Log($"[ProceduralDungeonAssetCreator] MinimapNode 생성 완료: {prefabPath}");
        }

        private void CreateMinimapEdgePrefab()
        {
            string prefabPath = PrefabUIPath + "MinimapEdge.prefab";

            if (File.Exists(prefabPath)) return;

            GameObject edge = new GameObject("MinimapEdge");
            var edgeRect = edge.AddComponent<RectTransform>();
            edgeRect.sizeDelta = new Vector2(100f, 2f);
            edgeRect.pivot = new Vector2(0.5f, 0.5f);

            var edgeImage = edge.AddComponent<Image>();
            edgeImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);

            // MinimapEdgeUI 컴포넌트
            edge.AddComponent<MinimapEdgeUI>();

            PrefabUtility.SaveAsPrefabAsset(edge, prefabPath);
            DestroyImmediate(edge);

            Debug.Log($"[ProceduralDungeonAssetCreator] MinimapEdge 생성 완료: {prefabPath}");
        }


        // ====== 3. MinimapConfig ScriptableObject ======

        private void CreateMinimapConfig()
        {
            EnsureDirectoriesExist();

            string assetPath = DataUIPath + "MinimapConfig.asset";

            if (File.Exists(assetPath))
            {
                Debug.LogWarning($"[ProceduralDungeonAssetCreator] 이미 존재: {assetPath}");
                return;
            }

            var config = ScriptableObject.CreateInstance<MinimapConfig>();

            // 기본값 설정 (MinimapConfig의 기본값 사용)

            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[ProceduralDungeonAssetCreator] MinimapConfig 생성 완료: {assetPath}");

            EditorGUIUtility.PingObject(config);
        }


        // ====== 4. StageConfig 5개 ======

        private void CreateAllStageConfigs()
        {
            EnsureDirectoriesExist();

            CreateStageConfig("Stage1_Forest", "숲", 1, 8, 1);
            CreateStageConfig("Stage2_Cave", "동굴", 2, 10, 2);
            CreateStageConfig("Stage3_Ruins", "폐허", 3, 12, 3);
            CreateStageConfig("Stage4_Castle", "성", 4, 14, 4);
            CreateStageConfig("Stage5_Boss", "마왕성", 5, 15, 5);

            Debug.Log("[ProceduralDungeonAssetCreator] 모든 StageConfig 생성 완료");
        }

        private void CreateStageConfig(string fileName, string stageName, int stageNum, int totalFloors, int baseDifficulty)
        {
            string assetPath = DataStagesPath + fileName + ".asset";

            if (File.Exists(assetPath))
            {
                Debug.LogWarning($"[ProceduralDungeonAssetCreator] 이미 존재: {assetPath}");
                return;
            }

            var config = ScriptableObject.CreateInstance<StageConfig>();
            config.stageName = $"스테이지 {stageNum}: {stageName}";
            config.stageId = $"stage_{stageNum}";
            config.description = $"{stageName} 지역을 탐험하세요.";
            config.baseDifficulty = baseDifficulty;
            config.difficultyPerFloor = 0.5f;
            config.recommendedLevel = baseDifficulty * 5;
            config.baseClearGold = 100 * stageNum;
            config.baseClearExp = 50 * stageNum;
            config.allowRevive = true;
            config.maxRevives = Mathf.Max(1, 3 - (stageNum / 2));
            config.requiredLevel = (stageNum - 1) * 5;

            // 선행 스테이지 설정
            if (stageNum > 1)
            {
                config.prerequisiteStageIds = new string[] { $"stage_{stageNum - 1}" };
            }

            AssetDatabase.CreateAsset(config, assetPath);

            Debug.Log($"[ProceduralDungeonAssetCreator] StageConfig 생성: {assetPath}");
        }


        // ====== 5. TestDungeon Config ======

        private void CreateTestDungeonConfig()
        {
            EnsureDirectoriesExist();

            string assetPath = DataDungeonsPath + "TestDungeon.asset";

            if (File.Exists(assetPath))
            {
                Debug.LogWarning($"[ProceduralDungeonAssetCreator] 이미 존재: {assetPath}");
                return;
            }

            var config = ScriptableObject.CreateInstance<DungeonConfig>();
            config.dungeonName = "테스트 던전";
            config.recommendedLevel = 1;
            config.description = "절차적 생성 테스트용 던전입니다.";
            config.generationType = DungeonGenerationType.Procedural;

            // RoomGenerationRules 생성
            CreateTestRoomGenerationRules();
            var rules = AssetDatabase.LoadAssetAtPath<RoomGenerationRules>(DataDungeonsPath + "TestRoomRules.asset");
            config.generationRules = rules;

            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[ProceduralDungeonAssetCreator] TestDungeon 생성 완료: {assetPath}");

            EditorGUIUtility.PingObject(config);
        }

        private void CreateTestRoomGenerationRules()
        {
            string assetPath = DataDungeonsPath + "TestRoomRules.asset";

            if (File.Exists(assetPath)) return;

            var rules = ScriptableObject.CreateInstance<RoomGenerationRules>();
            rules.minRooms = 5;
            rules.maxRooms = 10;
            rules.includeBossRoom = true;
            rules.totalFloors = 8;
            rules.branchingFactor = 0.4f;
            rules.maxBranches = 2;
            rules.minNodesPerFloor = 1;
            rules.maxNodesPerFloor = 3;
            rules.eliteRoomRatio = 0.15f;
            rules.shopRoomRatio = 0.1f;
            rules.restRoomRatio = 0.1f;

            AssetDatabase.CreateAsset(rules, assetPath);

            Debug.Log($"[ProceduralDungeonAssetCreator] TestRoomRules 생성: {assetPath}");
        }


        // ====== 유틸리티 ======

        private GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }
    }
}
#endif
