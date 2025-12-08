#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using GASPT.UI.Meta;

namespace GASPT.Editor
{
    /// <summary>
    /// 업그레이드 UI 프리팹 생성 도구
    /// </summary>
    public class UpgradeUIPrefabCreator : EditorWindow
    {
        [MenuItem("GASPT/UI/Create Upgrade UI Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<UpgradeUIPrefabCreator>("Upgrade UI Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("업그레이드 UI 프리팹 생성", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("UpgradeNodeView 프리팹 생성"))
            {
                CreateUpgradeNodePrefab();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("UpgradeTreeView 프리팹 생성"))
            {
                CreateUpgradeTreePrefab();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "생성된 프리팹은 Assets/_Project/Prefabs/UI/Meta/ 폴더에 저장됩니다.",
                MessageType.Info);
        }

        private void CreateUpgradeNodePrefab()
        {
            // 폴더 생성
            string folderPath = "Assets/_Project/Prefabs/UI/Meta";
            EnsureFolderExists(folderPath);

            // 루트 오브젝트
            GameObject nodeObj = new GameObject("UpgradeNode");
            RectTransform nodeRect = nodeObj.AddComponent<RectTransform>();
            nodeRect.sizeDelta = new Vector2(140, 160);

            // 배경
            GameObject bg = CreateUIElement("Background", nodeObj.transform);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            StretchFill(bgRect);

            // 테두리
            GameObject border = CreateUIElement("Border", nodeObj.transform);
            Image borderImage = border.AddComponent<Image>();
            borderImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            borderImage.type = Image.Type.Sliced;
            RectTransform borderRect = border.GetComponent<RectTransform>();
            StretchFill(borderRect);

            // 아이콘
            GameObject icon = CreateUIElement("Icon", nodeObj.transform);
            Image iconImage = icon.AddComponent<Image>();
            iconImage.color = Color.white;
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.6f);
            iconRect.anchorMax = new Vector2(0.5f, 0.6f);
            iconRect.sizeDelta = new Vector2(64, 64);
            iconRect.anchoredPosition = Vector2.zero;

            // 이름 텍스트
            GameObject nameObj = CreateUIElement("NameText", nodeObj.transform);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "업그레이드";
            nameText.fontSize = 14;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.25f);
            nameRect.anchorMax = new Vector2(1, 0.4f);
            nameRect.offsetMin = new Vector2(5, 0);
            nameRect.offsetMax = new Vector2(-5, 0);

            // 레벨 텍스트
            GameObject levelObj = CreateUIElement("LevelText", nodeObj.transform);
            TextMeshProUGUI levelText = levelObj.AddComponent<TextMeshProUGUI>();
            levelText.text = "Lv.0/5";
            levelText.fontSize = 12;
            levelText.alignment = TextAlignmentOptions.Center;
            levelText.color = new Color(0.8f, 0.8f, 0.8f);
            RectTransform levelRect = levelObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0, 0.05f);
            levelRect.anchorMax = new Vector2(1, 0.2f);
            levelRect.offsetMin = new Vector2(5, 0);
            levelRect.offsetMax = new Vector2(-5, 0);

            // 진행 바 배경
            GameObject progressBg = CreateUIElement("ProgressBarBG", nodeObj.transform);
            Image progressBgImage = progressBg.AddComponent<Image>();
            progressBgImage.color = new Color(0.2f, 0.2f, 0.2f);
            RectTransform progressBgRect = progressBg.GetComponent<RectTransform>();
            progressBgRect.anchorMin = new Vector2(0.1f, 0.02f);
            progressBgRect.anchorMax = new Vector2(0.9f, 0.06f);
            progressBgRect.offsetMin = Vector2.zero;
            progressBgRect.offsetMax = Vector2.zero;

            // 진행 바
            GameObject progress = CreateUIElement("ProgressBar", progressBg.transform);
            Image progressImage = progress.AddComponent<Image>();
            progressImage.color = new Color(0.2f, 0.8f, 0.2f);
            progressImage.type = Image.Type.Filled;
            progressImage.fillMethod = Image.FillMethod.Horizontal;
            progressImage.fillAmount = 0f;
            RectTransform progressRect = progress.GetComponent<RectTransform>();
            StretchFill(progressRect);

            // 잠금 오버레이
            GameObject locked = CreateUIElement("LockedOverlay", nodeObj.transform);
            Image lockedImage = locked.AddComponent<Image>();
            lockedImage.color = new Color(0, 0, 0, 0.7f);
            RectTransform lockedRect = locked.GetComponent<RectTransform>();
            StretchFill(lockedRect);
            locked.SetActive(false);

            // 최대 레벨 마크
            GameObject maxMark = CreateUIElement("MaxLevelMark", nodeObj.transform);
            Image maxImage = maxMark.AddComponent<Image>();
            maxImage.color = new Color(1f, 0.8f, 0.2f, 0.8f);
            RectTransform maxRect = maxMark.GetComponent<RectTransform>();
            maxRect.anchorMin = new Vector2(0.7f, 0.7f);
            maxRect.anchorMax = new Vector2(0.95f, 0.95f);
            maxRect.offsetMin = Vector2.zero;
            maxRect.offsetMax = Vector2.zero;
            maxMark.SetActive(false);

            // 버튼 컴포넌트
            Button button = nodeObj.AddComponent<Button>();
            button.targetGraphic = bgImage;

            // UpgradeNodeView 컴포넌트
            UpgradeNodeView nodeView = nodeObj.AddComponent<UpgradeNodeView>();

            // SerializedObject로 필드 설정
            SerializedObject so = new SerializedObject(nodeView);
            so.FindProperty("iconImage").objectReferenceValue = iconImage;
            so.FindProperty("backgroundImage").objectReferenceValue = bgImage;
            so.FindProperty("borderImage").objectReferenceValue = borderImage;
            so.FindProperty("nameText").objectReferenceValue = nameText;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("lockedOverlay").objectReferenceValue = locked;
            so.FindProperty("maxLevelMark").objectReferenceValue = maxMark;
            so.FindProperty("progressBar").objectReferenceValue = progressImage;
            so.FindProperty("selectButton").objectReferenceValue = button;
            so.ApplyModifiedProperties();

            // 프리팹 저장
            string prefabPath = $"{folderPath}/UpgradeNode.prefab";
            PrefabUtility.SaveAsPrefabAsset(nodeObj, prefabPath);
            DestroyImmediate(nodeObj);

            Debug.Log($"UpgradeNode 프리팹 생성 완료: {prefabPath}");
            AssetDatabase.Refresh();
        }

        private void CreateUpgradeTreePrefab()
        {
            // 폴더 생성
            string folderPath = "Assets/_Project/Prefabs/UI/Meta";
            EnsureFolderExists(folderPath);

            // 캔버스 생성 (테스트용)
            GameObject canvasObj = new GameObject("UpgradeTreeCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // 패널 루트
            GameObject panelRoot = CreateUIElement("UpgradeTreePanel", canvasObj.transform);
            Image panelBg = panelRoot.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // 헤더
            GameObject header = CreateUIElement("Header", panelRoot.transform);
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.9f);
            headerRect.anchorMax = new Vector2(1, 1f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            // 타이틀
            GameObject titleObj = CreateUIElement("Title", header.transform);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "업그레이드";
            titleText.fontSize = 28;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            StretchFill(titleRect);

            // 닫기 버튼
            GameObject closeBtn = CreateUIElement("CloseButton", header.transform);
            Image closeBtnImage = closeBtn.AddComponent<Image>();
            closeBtnImage.color = new Color(0.8f, 0.2f, 0.2f);
            Button closeButton = closeBtn.AddComponent<Button>();
            RectTransform closeBtnRect = closeBtn.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.95f, 0.2f);
            closeBtnRect.anchorMax = new Vector2(0.98f, 0.8f);
            closeBtnRect.offsetMin = Vector2.zero;
            closeBtnRect.offsetMax = Vector2.zero;

            // 재화 패널
            GameObject currencyPanel = CreateUIElement("CurrencyPanel", header.transform);
            RectTransform currencyRect = currencyPanel.GetComponent<RectTransform>();
            currencyRect.anchorMin = new Vector2(0.02f, 0.2f);
            currencyRect.anchorMax = new Vector2(0.3f, 0.8f);
            currencyRect.offsetMin = Vector2.zero;
            currencyRect.offsetMax = Vector2.zero;

            // Bone 텍스트
            GameObject boneObj = CreateUIElement("BoneText", currencyPanel.transform);
            TextMeshProUGUI boneText = boneObj.AddComponent<TextMeshProUGUI>();
            boneText.text = "Bone: 0";
            boneText.fontSize = 16;
            boneText.alignment = TextAlignmentOptions.Left;
            boneText.color = Color.white;
            RectTransform boneRect = boneObj.GetComponent<RectTransform>();
            boneRect.anchorMin = new Vector2(0, 0);
            boneRect.anchorMax = new Vector2(0.5f, 1);
            boneRect.offsetMin = Vector2.zero;
            boneRect.offsetMax = Vector2.zero;

            // Soul 텍스트
            GameObject soulObj = CreateUIElement("SoulText", currencyPanel.transform);
            TextMeshProUGUI soulText = soulObj.AddComponent<TextMeshProUGUI>();
            soulText.text = "Soul: 0";
            soulText.fontSize = 16;
            soulText.alignment = TextAlignmentOptions.Left;
            soulText.color = new Color(0.8f, 0.5f, 1f);
            RectTransform soulRect = soulObj.GetComponent<RectTransform>();
            soulRect.anchorMin = new Vector2(0.5f, 0);
            soulRect.anchorMax = new Vector2(1, 1);
            soulRect.offsetMin = Vector2.zero;
            soulRect.offsetMax = Vector2.zero;

            // 콘텐츠 영역 (스크롤뷰)
            GameObject scrollView = CreateUIElement("ScrollView", panelRoot.transform);
            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = new Color(0.05f, 0.05f, 0.05f, 0.5f);
            RectTransform scrollViewRect = scrollView.GetComponent<RectTransform>();
            scrollViewRect.anchorMin = new Vector2(0.02f, 0.15f);
            scrollViewRect.anchorMax = new Vector2(0.58f, 0.88f);
            scrollViewRect.offsetMin = Vector2.zero;
            scrollViewRect.offsetMax = Vector2.zero;

            // 콘텐츠 부모
            GameObject content = CreateUIElement("Content", scrollView.transform);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 600);
            GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(140, 160);
            grid.spacing = new Vector2(10, 10);
            grid.padding = new RectOffset(10, 10, 10, 10);
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // 상세 패널
            GameObject detailPanel = CreateUIElement("DetailPanel", panelRoot.transform);
            Image detailBg = detailPanel.AddComponent<Image>();
            detailBg.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);
            RectTransform detailRect = detailPanel.GetComponent<RectTransform>();
            detailRect.anchorMin = new Vector2(0.6f, 0.15f);
            detailRect.anchorMax = new Vector2(0.98f, 0.88f);
            detailRect.offsetMin = Vector2.zero;
            detailRect.offsetMax = Vector2.zero;

            // 상세 아이콘
            GameObject detailIcon = CreateUIElement("DetailIcon", detailPanel.transform);
            Image detailIconImage = detailIcon.AddComponent<Image>();
            RectTransform detailIconRect = detailIcon.GetComponent<RectTransform>();
            detailIconRect.anchorMin = new Vector2(0.3f, 0.7f);
            detailIconRect.anchorMax = new Vector2(0.7f, 0.95f);
            detailIconRect.offsetMin = Vector2.zero;
            detailIconRect.offsetMax = Vector2.zero;

            // 상세 이름
            GameObject detailName = CreateUIElement("DetailName", detailPanel.transform);
            TextMeshProUGUI detailNameText = detailName.AddComponent<TextMeshProUGUI>();
            detailNameText.text = "업그레이드 이름";
            detailNameText.fontSize = 20;
            detailNameText.alignment = TextAlignmentOptions.Center;
            detailNameText.color = Color.white;
            RectTransform detailNameRect = detailName.GetComponent<RectTransform>();
            detailNameRect.anchorMin = new Vector2(0.05f, 0.6f);
            detailNameRect.anchorMax = new Vector2(0.95f, 0.7f);
            detailNameRect.offsetMin = Vector2.zero;
            detailNameRect.offsetMax = Vector2.zero;

            // 상세 설명
            GameObject detailDesc = CreateUIElement("DetailDescription", detailPanel.transform);
            TextMeshProUGUI detailDescText = detailDesc.AddComponent<TextMeshProUGUI>();
            detailDescText.text = "업그레이드 설명";
            detailDescText.fontSize = 14;
            detailDescText.alignment = TextAlignmentOptions.Center;
            detailDescText.color = new Color(0.7f, 0.7f, 0.7f);
            RectTransform detailDescRect = detailDesc.GetComponent<RectTransform>();
            detailDescRect.anchorMin = new Vector2(0.05f, 0.5f);
            detailDescRect.anchorMax = new Vector2(0.95f, 0.6f);
            detailDescRect.offsetMin = Vector2.zero;
            detailDescRect.offsetMax = Vector2.zero;

            // 레벨 텍스트
            GameObject detailLevel = CreateUIElement("DetailLevel", detailPanel.transform);
            TextMeshProUGUI detailLevelText = detailLevel.AddComponent<TextMeshProUGUI>();
            detailLevelText.text = "Level 0 / 5";
            detailLevelText.fontSize = 16;
            detailLevelText.alignment = TextAlignmentOptions.Center;
            detailLevelText.color = Color.white;
            RectTransform detailLevelRect = detailLevel.GetComponent<RectTransform>();
            detailLevelRect.anchorMin = new Vector2(0.05f, 0.42f);
            detailLevelRect.anchorMax = new Vector2(0.95f, 0.5f);
            detailLevelRect.offsetMin = Vector2.zero;
            detailLevelRect.offsetMax = Vector2.zero;

            // 현재 효과
            GameObject currentEffect = CreateUIElement("CurrentEffect", detailPanel.transform);
            TextMeshProUGUI currentEffectText = currentEffect.AddComponent<TextMeshProUGUI>();
            currentEffectText.text = "현재: 없음";
            currentEffectText.fontSize = 14;
            currentEffectText.alignment = TextAlignmentOptions.Left;
            currentEffectText.color = new Color(0.6f, 0.8f, 0.6f);
            RectTransform currentEffectRect = currentEffect.GetComponent<RectTransform>();
            currentEffectRect.anchorMin = new Vector2(0.1f, 0.32f);
            currentEffectRect.anchorMax = new Vector2(0.9f, 0.4f);
            currentEffectRect.offsetMin = Vector2.zero;
            currentEffectRect.offsetMax = Vector2.zero;

            // 다음 효과
            GameObject nextEffect = CreateUIElement("NextEffect", detailPanel.transform);
            TextMeshProUGUI nextEffectText = nextEffect.AddComponent<TextMeshProUGUI>();
            nextEffectText.text = "다음: +5%";
            nextEffectText.fontSize = 14;
            nextEffectText.alignment = TextAlignmentOptions.Left;
            nextEffectText.color = Color.green;
            RectTransform nextEffectRect = nextEffect.GetComponent<RectTransform>();
            nextEffectRect.anchorMin = new Vector2(0.1f, 0.24f);
            nextEffectRect.anchorMax = new Vector2(0.9f, 0.32f);
            nextEffectRect.offsetMin = Vector2.zero;
            nextEffectRect.offsetMax = Vector2.zero;

            // 비용
            GameObject cost = CreateUIElement("Cost", detailPanel.transform);
            TextMeshProUGUI costText = cost.AddComponent<TextMeshProUGUI>();
            costText.text = "100 Bone";
            costText.fontSize = 18;
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = Color.yellow;
            RectTransform costRect = cost.GetComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0.1f, 0.14f);
            costRect.anchorMax = new Vector2(0.9f, 0.22f);
            costRect.offsetMin = Vector2.zero;
            costRect.offsetMax = Vector2.zero;

            // 구매 버튼
            GameObject purchaseBtn = CreateUIElement("PurchaseButton", detailPanel.transform);
            Image purchaseBtnImage = purchaseBtn.AddComponent<Image>();
            purchaseBtnImage.color = new Color(0.2f, 0.6f, 0.2f);
            Button purchaseButton = purchaseBtn.AddComponent<Button>();
            RectTransform purchaseBtnRect = purchaseBtn.GetComponent<RectTransform>();
            purchaseBtnRect.anchorMin = new Vector2(0.2f, 0.03f);
            purchaseBtnRect.anchorMax = new Vector2(0.8f, 0.12f);
            purchaseBtnRect.offsetMin = Vector2.zero;
            purchaseBtnRect.offsetMax = Vector2.zero;

            GameObject purchaseBtnTextObj = CreateUIElement("Text", purchaseBtn.transform);
            TextMeshProUGUI purchaseBtnText = purchaseBtnTextObj.AddComponent<TextMeshProUGUI>();
            purchaseBtnText.text = "업그레이드";
            purchaseBtnText.fontSize = 16;
            purchaseBtnText.alignment = TextAlignmentOptions.Center;
            purchaseBtnText.color = Color.white;
            RectTransform purchaseBtnTextRect = purchaseBtnTextObj.GetComponent<RectTransform>();
            StretchFill(purchaseBtnTextRect);

            // UpgradeTreeView 컴포넌트
            UpgradeTreeView treeView = panelRoot.AddComponent<UpgradeTreeView>();

            // SerializedObject로 필드 설정
            SerializedObject so = new SerializedObject(treeView);
            so.FindProperty("panelRoot").objectReferenceValue = panelRoot;
            so.FindProperty("closeButton").objectReferenceValue = closeButton;
            so.FindProperty("boneText").objectReferenceValue = boneText;
            so.FindProperty("soulText").objectReferenceValue = soulText;
            so.FindProperty("contentParent").objectReferenceValue = content.transform;
            so.FindProperty("detailPanel").objectReferenceValue = detailPanel;
            so.FindProperty("detailIcon").objectReferenceValue = detailIconImage;
            so.FindProperty("detailNameText").objectReferenceValue = detailNameText;
            so.FindProperty("detailDescriptionText").objectReferenceValue = detailDescText;
            so.FindProperty("detailLevelText").objectReferenceValue = detailLevelText;
            so.FindProperty("currentEffectText").objectReferenceValue = currentEffectText;
            so.FindProperty("nextEffectText").objectReferenceValue = nextEffectText;
            so.FindProperty("costText").objectReferenceValue = costText;
            so.FindProperty("purchaseButton").objectReferenceValue = purchaseButton;
            so.FindProperty("purchaseButtonText").objectReferenceValue = purchaseBtnText;
            so.ApplyModifiedProperties();

            // 프리팹 저장 (패널만)
            string prefabPath = $"{folderPath}/UpgradeTreePanel.prefab";
            PrefabUtility.SaveAsPrefabAsset(panelRoot, prefabPath);
            DestroyImmediate(canvasObj);

            Debug.Log($"UpgradeTreePanel 프리팹 생성 완료: {prefabPath}");
            AssetDatabase.Refresh();
        }

        private GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private void StretchFill(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void EnsureFolderExists(string path)
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
