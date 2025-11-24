using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using GASPT.UI.MVP;
using System.IO;

namespace GASPT.EditorTools
{
    /// <summary>
    /// ShopView 자동 생성 에디터 도구
    /// Phase 7-B: ShopSystem MVP 패턴 리팩토링
    /// </summary>
    public class ShopViewCreator : EditorWindow
    {
        // ====== 경로 ======

        private const string PrefabPath = "Assets/Resources/Prefabs/UI/";


        // ====== 에디터 창 ======

        [MenuItem("Tools/GASPT/UI/Create Shop View (MVP)")]
        public static void ShowWindow()
        {
            ShopViewCreator window = GetWindow<ShopViewCreator>("Shop View Creator");
            window.minSize = new Vector2(400f, 500f);
            window.Show();
        }


        // ====== GUI ======

        private Vector2 scrollPosition;

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10f);

            // 타이틀
            EditorGUILayout.LabelField("Shop View Creator (MVP Pattern)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Phase 7-B: ShopSystem MVP 패턴 리팩토링", EditorStyles.miniLabel);

            GUILayout.Space(10f);
            EditorGUILayout.HelpBox(
                "ShopView (MVP 패턴)를 자동으로 생성합니다.\n" +
                "1. ShopView (Canvas 자식으로 추가)\n" +
                "2. ShopItemSlot 프리팹 (Resources/Prefabs/UI/)\n\n" +
                "MVP 패턴:\n" +
                "- ShopView: 순수 렌더링 (MonoBehaviour)\n" +
                "- ShopPresenter: 비즈니스 로직 (Pure C#)\n" +
                "- ShopItemViewModel: 표시 데이터",
                MessageType.Info
            );

            GUILayout.Space(20f);

            // 버튼
            if (GUILayout.Button("🎨 모든 UI 자동 생성", GUILayout.Height(40f)))
            {
                EditorApplication.delayCall += CreateAllUI;
            }

            GUILayout.Space(10f);

            if (GUILayout.Button("🗑️ 모든 UI 삭제", GUILayout.Height(30f)))
            {
                EditorApplication.delayCall += DeleteAllUI;
            }

            GUILayout.Space(20f);

            // 개별 생성 버튼
            EditorGUILayout.LabelField("개별 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("1. ShopView 생성 (Canvas 자식)"))
            {
                CreateShopView();
            }

            if (GUILayout.Button("2. ShopItemSlot 프리팹 생성"))
            {
                CreateShopItemSlotPrefab();
            }

            EditorGUILayout.EndScrollView();
        }


        // ====== UI 생성 ======

        /// <summary>
        /// 모든 UI 자동 생성
        /// </summary>
        private void CreateAllUI()
        {
            CreateShopItemSlotPrefab();
            CreateShopView();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ShopViewCreator] 모든 UI 생성 완료!");
            EditorUtility.DisplayDialog("완료", "ShopView (MVP)가 생성되었습니다!", "확인");
        }

        /// <summary>
        /// ShopView 생성 (UI CANVAS 자식)
        /// </summary>
        private void CreateShopView()
        {
            // UI CANVAS 찾기
            GameObject canvasObj = GameObject.Find("=== UI CANVAS ===");
            if (canvasObj == null)
            {
                EditorUtility.DisplayDialog("오류", "Scene에 '=== UI CANVAS ===' 오브젝트가 없습니다!", "확인");
                return;
            }

            // ShopView 부모 생성 (항상 활성화, ShopView 컴포넌트 포함)
            GameObject shopViewObj = new GameObject("ShopView");
            shopViewObj.transform.SetParent(canvasObj.transform, false);

            RectTransform shopViewRect = shopViewObj.AddComponent<RectTransform>();
            shopViewRect.anchorMin = Vector2.zero;
            shopViewRect.anchorMax = Vector2.one;
            shopViewRect.sizeDelta = Vector2.zero;
            shopViewRect.anchoredPosition = Vector2.zero;

            // Panel 자식 생성 (실제 UI, SetActive로 제어됨)
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(shopViewObj.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800f, 600f);
            panelRect.anchoredPosition = Vector2.zero;

            // Background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(panel.transform, false);

            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // Title Text
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
            titleRect.sizeDelta = new Vector2(760f, 60f);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "상점";
            titleText.fontSize = 36f;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            // Gold Text
            GameObject goldObj = new GameObject("GoldText");
            goldObj.transform.SetParent(panel.transform, false);

            RectTransform goldRect = goldObj.AddComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0.5f, 1f);
            goldRect.anchorMax = new Vector2(0.5f, 1f);
            goldRect.pivot = new Vector2(0.5f, 1f);
            goldRect.anchoredPosition = new Vector2(0f, -90f);
            goldRect.sizeDelta = new Vector2(760f, 40f);

            TextMeshProUGUI goldText = goldObj.AddComponent<TextMeshProUGUI>();
            goldText.text = "Gold: 0";
            goldText.fontSize = 24f;
            goldText.alignment = TextAlignmentOptions.Center;
            goldText.color = Color.yellow;

            // Item List Panel (ScrollView)
            CreateItemListPanel(panel);

            // Message Text
            CreateMessageText(panel);

            // Close Button
            CreateCloseButton(panel);

            // ShopView 컴포넌트 추가 (부모 오브젝트에 추가)
            ShopView shopView = shopViewObj.AddComponent<ShopView>();

            // SerializedObject로 참조 설정
            SerializedObject so = new SerializedObject(shopView);

            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("goldText").objectReferenceValue = goldText;
            so.FindProperty("itemListContent").objectReferenceValue = panel.transform.Find("ItemListPanel/Viewport/Content");
            so.FindProperty("messageText").objectReferenceValue = panel.transform.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("closeButton").objectReferenceValue = panel.transform.Find("CloseButton")?.GetComponent<Button>();

            // ShopItemSlot 프리팹 로드
            GameObject itemSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "ShopItemSlot.prefab");
            so.FindProperty("itemSlotPrefab").objectReferenceValue = itemSlotPrefab;

            so.ApplyModifiedProperties();

            Debug.Log("[ShopViewCreator] ShopView 생성 완료 (구조: ShopView > Panel)");

            EditorGUIUtility.PingObject(shopViewObj);
            Selection.activeGameObject = shopViewObj;
        }

        /// <summary>
        /// ItemListPanel 생성 (ScrollView)
        /// </summary>
        private void CreateItemListPanel(GameObject parent)
        {
            // Scroll View
            GameObject scrollView = new GameObject("ItemListPanel");
            scrollView.transform.SetParent(parent.transform, false);

            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 1f);
            scrollRect.offsetMin = new Vector2(20f, 120f);
            scrollRect.offsetMax = new Vector2(-20f, -140f);

            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);

            ScrollRect scrollRectComponent = scrollView.AddComponent<ScrollRect>();

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            Image viewportMask = viewport.AddComponent<Image>();
            viewportMask.color = Color.white;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 800f);

            VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;    // 자식의 너비를 제어함
            layoutGroup.childControlHeight = false;  // 자식의 높이를 제어하지 않음 (LayoutElement 사용)
            layoutGroup.childForceExpandWidth = true;  // 너비를 강제로 확장 (Stretch)
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 10f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ScrollRect 설정
            scrollRectComponent.content = contentRect;
            scrollRectComponent.viewport = viewportRect;
            scrollRectComponent.horizontal = false;
            scrollRectComponent.vertical = true;
        }

        /// <summary>
        /// MessageText 생성
        /// </summary>
        private void CreateMessageText(GameObject parent)
        {
            GameObject messageObj = new GameObject("MessageText");
            messageObj.transform.SetParent(parent.transform, false);

            RectTransform messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0.5f, 0f);
            messageRect.anchorMax = new Vector2(0.5f, 0f);
            messageRect.pivot = new Vector2(0.5f, 0f);
            messageRect.anchoredPosition = new Vector2(0f, 80f);
            messageRect.sizeDelta = new Vector2(760f, 40f);

            TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = "";
            messageText.fontSize = 20f;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.color = Color.white;

            messageObj.SetActive(false);
        }

        /// <summary>
        /// CloseButton 생성
        /// </summary>
        private void CreateCloseButton(GameObject parent)
        {
            GameObject buttonObj = new GameObject("CloseButton");
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.anchoredPosition = new Vector2(0f, 20f);
            buttonRect.sizeDelta = new Vector2(200f, 50f);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            Button button = buttonObj.AddComponent<Button>();

            // Button Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "닫기";
            buttonText.fontSize = 24f;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
        }


        // ====== 프리팹 생성 ======

        /// <summary>
        /// ShopItemSlot 프리팹 생성
        /// </summary>
        private void CreateShopItemSlotPrefab()
        {
            // 폴더 확인
            if (!Directory.Exists(PrefabPath))
            {
                Directory.CreateDirectory(PrefabPath);
            }

            GameObject slot = new GameObject("ShopItemSlot");

            RectTransform slotRect = slot.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0f, 0.5f);
            slotRect.anchorMax = new Vector2(1f, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            slotRect.sizeDelta = new Vector2(0f, 100f);

            Image slotBg = slot.AddComponent<Image>();
            slotBg.color = new Color(0.3f, 0.3f, 0.35f, 1f);

            // LayoutElement 추가 (LayoutGroup 사용 시 크기 제어)
            LayoutElement layoutElement = slot.AddComponent<LayoutElement>();
            layoutElement.minWidth = -1f;          // -1 = LayoutGroup의 기본 동작 따름
            layoutElement.preferredWidth = -1f;    // -1 = LayoutGroup의 기본 동작 따름
            layoutElement.flexibleWidth = -1f;     // -1 = LayoutGroup의 기본 동작 따름 (childForceExpandWidth 사용)
            layoutElement.minHeight = 100f;        // 최소 높이
            layoutElement.preferredHeight = 100f;  // 선호 높이
            layoutElement.flexibleHeight = 0f;     // 높이는 고정

            // IconImage
            GameObject iconObj = new GameObject("IconImage");
            iconObj.transform.SetParent(slot.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(10f, 0f);
            iconRect.sizeDelta = new Vector2(80f, 80f);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // NameText
            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(slot.transform, false);

            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(0f, 0.5f);
            nameRect.pivot = new Vector2(0f, 0.5f);
            nameRect.anchoredPosition = new Vector2(100f, 20f);
            nameRect.sizeDelta = new Vector2(350f, 30f);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Item Name";
            nameText.fontSize = 22f;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.color = Color.white;

            // PriceText
            GameObject priceObj = new GameObject("PriceText");
            priceObj.transform.SetParent(slot.transform, false);

            RectTransform priceRect = priceObj.AddComponent<RectTransform>();
            priceRect.anchorMin = new Vector2(0f, 0.5f);
            priceRect.anchorMax = new Vector2(0f, 0.5f);
            priceRect.pivot = new Vector2(0f, 0.5f);
            priceRect.anchoredPosition = new Vector2(100f, -20f);
            priceRect.sizeDelta = new Vector2(200f, 25f);

            TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
            priceText.text = "100G";
            priceText.fontSize = 18f;
            priceText.alignment = TextAlignmentOptions.Left;
            priceText.color = Color.yellow;

            // PurchaseButton
            GameObject buttonObj = new GameObject("PurchaseButton");
            buttonObj.transform.SetParent(slot.transform, false);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 0.5f);
            buttonRect.anchorMax = new Vector2(1f, 0.5f);
            buttonRect.pivot = new Vector2(1f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(-10f, 0f);
            buttonRect.sizeDelta = new Vector2(100f, 60f);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);

            Button button = buttonObj.AddComponent<Button>();

            // Button Text
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(buttonObj.transform, false);

            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "구매";
            btnText.fontSize = 20f;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;

            // 프리팹 저장
            string prefabPath = PrefabPath + "ShopItemSlot.prefab";
            PrefabUtility.SaveAsPrefabAsset(slot, prefabPath);

            DestroyImmediate(slot);

            Debug.Log($"[ShopViewCreator] ShopItemSlot 프리팹 생성 완료: {prefabPath}");
        }


        // ====== UI 삭제 ======

        /// <summary>
        /// 모든 UI 삭제
        /// </summary>
        private void DeleteAllUI()
        {
            if (!EditorUtility.DisplayDialog("확인", "모든 ShopView UI를 삭제하시겠습니까?", "삭제", "취소"))
                return;

            // ShopView 삭제 (Scene에서)
            ShopView shopView = FindAnyObjectByType<ShopView>();
            if (shopView != null)
            {
                DestroyImmediate(shopView.gameObject);
                Debug.Log("[ShopViewCreator] ShopView 삭제 완료");
            }

            // ShopItemSlot 프리팹 삭제
            string shopItemSlotPath = PrefabPath + "ShopItemSlot.prefab";
            if (File.Exists(shopItemSlotPath))
            {
                AssetDatabase.DeleteAsset(shopItemSlotPath);
                Debug.Log("[ShopViewCreator] ShopItemSlot 프리팹 삭제 완료");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("완료", "모든 ShopView UI가 삭제되었습니다.", "확인");
        }
    }
}
