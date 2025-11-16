using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using GASPT.UI;

namespace GASPT.Editor
{
    /// <summary>
    /// ShopUI 프리팹을 자동으로 생성하는 Editor Tool
    /// Menu: Tools > GASPT > Create ShopUI
    /// ShopPanel + ItemSlotPrefab 두 개의 프리팹을 생성합니다.
    /// </summary>
    public static class ShopUICreator
    {
        private const string SHOP_PANEL_PREFAB_PATH = "Assets/_Project/Prefabs/UI/ShopPanel.prefab";
        private const string ITEM_SLOT_PREFAB_PATH = "Assets/_Project/Prefabs/UI/ItemSlotPrefab.prefab";

        /// <summary>
        /// ShopUI 프리팹 생성 (메뉴 항목)
        /// </summary>
        [MenuItem("Tools/GASPT/Create ShopUI")]
        public static void CreateShopUI()
        {
            Debug.Log("[ShopUICreator] ShopUI 생성 시작...");

            // 1. Canvas 찾기 또는 생성
            Canvas canvas = EditorUtilities.FindOrCreateCanvas("[ShopUICreator]");

            // 2. ItemSlotPrefab 먼저 생성 (ShopUI가 참조하므로)
            GameObject itemSlotPrefab = CreateItemSlotPrefab();

            // 3. ShopPanel GameObject 생성
            GameObject shopPanel = CreateShopPanelGameObject(canvas.transform);

            // 4. GoldText 생성
            GameObject goldText = CreateGoldText(shopPanel.transform);

            // 5. MessageText 생성 (비활성화)
            GameObject messageText = CreateMessageText(shopPanel.transform);

            // 6. ScrollView 생성
            GameObject scrollView = CreateScrollView(shopPanel.transform);
            Transform contentTransform = scrollView.transform.Find("Viewport/Content");

            // 7. ShopUI 스크립트 추가 및 참조 연결
            ShopUI shopUI = shopPanel.AddComponent<ShopUI>();
            AssignShopUIReferences(shopUI, goldText, messageText, contentTransform, itemSlotPrefab);

            // 8. ShopPanel 프리팹으로 저장
            EditorUtilities.SaveAsPrefab(shopPanel, SHOP_PANEL_PREFAB_PATH, "[ShopUICreator]");

            Debug.Log($"[ShopUICreator] ShopUI 생성 완료!");
            Debug.Log($"[ShopUICreator] - ShopPanel 프리팹: {SHOP_PANEL_PREFAB_PATH}");
            Debug.Log($"[ShopUICreator] - ItemSlotPrefab: {ITEM_SLOT_PREFAB_PATH}");
            Debug.Log("[ShopUICreator] Scene에 생성된 ShopPanel은 프리팹으로 저장되었습니다. Scene에서 사용하려면 프리팹을 드래그하세요.");

            // Scene에서 선택
            Selection.activeGameObject = shopPanel;
        }



        // ====== ShopPanel GameObject 생성 ======

        /// <summary>
        /// ShopPanel GameObject 생성 및 RectTransform 설정
        /// </summary>
        private static GameObject CreateShopPanelGameObject(Transform parent)
        {
            GameObject shopPanel = new GameObject("ShopPanel");
            shopPanel.transform.SetParent(parent, false);

            RectTransform rectTransform = shopPanel.AddComponent<RectTransform>();

            // Anchor: Center
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Position and Size
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(600, 800);

            Debug.Log("[ShopUICreator] ShopPanel GameObject 생성 완료");
            return shopPanel;
        }


        // ====== GoldText 생성 ======

        /// <summary>
        /// GoldText (TextMeshPro) 생성
        /// </summary>
        private static GameObject CreateGoldText(Transform parent)
        {
            GameObject goldText = new GameObject("GoldText");
            goldText.transform.SetParent(parent, false);

            RectTransform rectTransform = goldText.AddComponent<RectTransform>();

            // Anchor: Top Center
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);

            // Position and Size
            rectTransform.anchoredPosition = new Vector2(0, -30);
            rectTransform.sizeDelta = new Vector2(560, 40);

            // TextMeshProUGUI 컴포넌트 추가
            TextMeshProUGUI tmp = goldText.AddComponent<TextMeshProUGUI>();
            tmp.text = "Gold: 100";
            tmp.fontSize = 28;
            tmp.color = new Color(1f, 0.78f, 0f); // Yellow (R:255, G:200, B:0)
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;

            Debug.Log("[ShopUICreator] GoldText 생성 완료");
            return goldText;
        }


        // ====== MessageText 생성 ======

        /// <summary>
        /// MessageText (TextMeshPro) 생성 (기본 비활성화)
        /// </summary>
        private static GameObject CreateMessageText(Transform parent)
        {
            GameObject messageText = new GameObject("MessageText");
            messageText.transform.SetParent(parent, false);

            RectTransform rectTransform = messageText.AddComponent<RectTransform>();

            // Anchor: Bottom Center
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.anchorMax = new Vector2(0.5f, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);

            // Position and Size
            rectTransform.anchoredPosition = new Vector2(0, 30);
            rectTransform.sizeDelta = new Vector2(560, 40);

            // TextMeshProUGUI 컴포넌트 추가
            TextMeshProUGUI tmp = messageText.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 24;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;

            // GameObject 비활성화
            messageText.SetActive(false);

            Debug.Log("[ShopUICreator] MessageText 생성 완료 (비활성화)");
            return messageText;
        }


        // ====== ScrollView 생성 ======

        /// <summary>
        /// ScrollView (Scroll Rect + Viewport + Content) 생성
        /// </summary>
        private static GameObject CreateScrollView(Transform parent)
        {
            // ScrollView GameObject
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(parent, false);

            RectTransform scrollViewRect = scrollView.AddComponent<RectTransform>();

            // Anchor: Stretch (가로/세로 모두)
            scrollViewRect.anchorMin = Vector2.zero;
            scrollViewRect.anchorMax = Vector2.one;

            // Offsets (Left: 20, Right: 20, Top: 90, Bottom: 90)
            scrollViewRect.offsetMin = new Vector2(20, 90);  // Left, Bottom
            scrollViewRect.offsetMax = new Vector2(-20, -90); // Right, Top

            // Scroll Rect 컴포넌트 추가
            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.vertical = true;
            scrollRect.horizontal = false;

            // Viewport 생성
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            // Viewport에 Mask 추가
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0.01f); // 거의 투명
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content 생성
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRect = content.AddComponent<RectTransform>();

            // Anchor: Top Center
            contentRect.anchorMin = new Vector2(0.5f, 1);
            contentRect.anchorMax = new Vector2(0.5f, 1);
            contentRect.pivot = new Vector2(0.5f, 1);

            // Size
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(560, 0); // Height는 Content Size Fitter가 자동 조정

            // Content Size Fitter 추가
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Vertical Layout Group 추가
            VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            // ScrollRect에 Viewport와 Content 연결
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;

            Debug.Log("[ShopUICreator] ScrollView 생성 완료 (Viewport + Content)");
            return scrollView;
        }


        // ====== ItemSlotPrefab 생성 ======

        /// <summary>
        /// ItemSlotPrefab 생성 (별도 프리팹)
        /// </summary>
        private static GameObject CreateItemSlotPrefab()
        {
            Debug.Log("[ShopUICreator] ItemSlotPrefab 생성 시작...");

            // ItemSlot GameObject
            GameObject itemSlot = new GameObject("ItemSlot");

            RectTransform itemSlotRect = itemSlot.AddComponent<RectTransform>();
            itemSlotRect.sizeDelta = new Vector2(540, 80);

            // Background Image
            GameObject background = new GameObject("Background");
            background.transform.SetParent(itemSlot.transform, false);

            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.39f, 0.39f, 0.39f, 0.78f); // 회색 (R:100, G:100, B:100, A:200)

            // ItemNameText
            GameObject itemNameText = new GameObject("ItemNameText");
            itemNameText.transform.SetParent(itemSlot.transform, false);

            RectTransform itemNameRect = itemNameText.AddComponent<RectTransform>();
            itemNameRect.anchorMin = new Vector2(0, 1);
            itemNameRect.anchorMax = new Vector2(0, 1);
            itemNameRect.pivot = new Vector2(0, 1);
            itemNameRect.anchoredPosition = new Vector2(20, -20);
            itemNameRect.sizeDelta = new Vector2(300, 30);

            TextMeshProUGUI itemNameTmp = itemNameText.AddComponent<TextMeshProUGUI>();
            itemNameTmp.text = "Fire Sword";
            itemNameTmp.fontSize = 22;
            itemNameTmp.color = Color.white;
            itemNameTmp.alignment = TextAlignmentOptions.Left;
            itemNameTmp.verticalAlignment = VerticalAlignmentOptions.Middle;

            // PriceText
            GameObject priceText = new GameObject("PriceText");
            priceText.transform.SetParent(itemSlot.transform, false);

            RectTransform priceRect = priceText.AddComponent<RectTransform>();
            priceRect.anchorMin = new Vector2(0, 0);
            priceRect.anchorMax = new Vector2(0, 0);
            priceRect.pivot = new Vector2(0, 0);
            priceRect.anchoredPosition = new Vector2(20, 10);
            priceRect.sizeDelta = new Vector2(150, 25);

            TextMeshProUGUI priceTmp = priceText.AddComponent<TextMeshProUGUI>();
            priceTmp.text = "80 Gold";
            priceTmp.fontSize = 18;
            priceTmp.color = new Color(1f, 0.78f, 0f); // Yellow
            priceTmp.alignment = TextAlignmentOptions.Left;
            priceTmp.verticalAlignment = VerticalAlignmentOptions.Middle;

            // PurchaseButton
            GameObject purchaseButton = new GameObject("PurchaseButton");
            purchaseButton.transform.SetParent(itemSlot.transform, false);

            RectTransform buttonRect = purchaseButton.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1, 0.5f);
            buttonRect.anchorMax = new Vector2(1, 0.5f);
            buttonRect.pivot = new Vector2(1, 0.5f);
            buttonRect.anchoredPosition = new Vector2(-80, 0);
            buttonRect.sizeDelta = new Vector2(120, 50);

            Image buttonImage = purchaseButton.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 1f); // 파란색
            Button button = purchaseButton.AddComponent<Button>();

            // ButtonText
            GameObject buttonText = new GameObject("ButtonText");
            buttonText.transform.SetParent(purchaseButton.transform, false);

            RectTransform buttonTextRect = buttonText.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI buttonTmp = buttonText.AddComponent<TextMeshProUGUI>();
            buttonTmp.text = "Purchase";
            buttonTmp.fontSize = 20;
            buttonTmp.color = Color.white;
            buttonTmp.alignment = TextAlignmentOptions.Center;
            buttonTmp.verticalAlignment = VerticalAlignmentOptions.Middle;

            // ShopItemSlot 스크립트 추가
            ShopItemSlot shopItemSlot = itemSlot.AddComponent<ShopItemSlot>();
            AssignItemSlotReferences(shopItemSlot, itemNameText, priceText, purchaseButton);

            // 프리팹으로 저장
            EditorUtilities.SaveAsPrefab(itemSlot, ITEM_SLOT_PREFAB_PATH, "[ShopUICreator]");

            Debug.Log("[ShopUICreator] ItemSlotPrefab 생성 완료");

            // Scene에서 ItemSlot 제거 (프리팹만 유지)
            Object.DestroyImmediate(itemSlot);

            // 저장된 프리팹 로드하여 반환
            return AssetDatabase.LoadAssetAtPath<GameObject>(ITEM_SLOT_PREFAB_PATH);
        }


        // ====== ShopItemSlot 참조 연결 ======

        /// <summary>
        /// ShopItemSlot 컴포넌트의 참조 연결
        /// </summary>
        private static void AssignItemSlotReferences(ShopItemSlot shopItemSlot, GameObject itemNameText, GameObject priceText, GameObject purchaseButton)
        {
            SerializedObject serializedObject = new SerializedObject(shopItemSlot);

            SerializedProperty itemNameTextProp = serializedObject.FindProperty("itemNameText");
            SerializedProperty priceTextProp = serializedObject.FindProperty("priceText");
            SerializedProperty purchaseButtonProp = serializedObject.FindProperty("purchaseButton");

            if (itemNameTextProp != null && priceTextProp != null && purchaseButtonProp != null)
            {
                itemNameTextProp.objectReferenceValue = itemNameText.GetComponent<TextMeshProUGUI>();
                priceTextProp.objectReferenceValue = priceText.GetComponent<TextMeshProUGUI>();
                purchaseButtonProp.objectReferenceValue = purchaseButton.GetComponent<Button>();

                serializedObject.ApplyModifiedProperties();

                Debug.Log("[ShopUICreator] ShopItemSlot 참조 연결 완료");
            }
            else
            {
                Debug.LogError("[ShopUICreator] ShopItemSlot 필드를 찾을 수 없습니다. 필드 이름을 확인하세요.");
            }
        }


        // ====== ShopUI 참조 연결 ======

        /// <summary>
        /// ShopUI 컴포넌트의 참조 연결
        /// </summary>
        private static void AssignShopUIReferences(ShopUI shopUI, GameObject goldText, GameObject messageText, Transform content, GameObject itemSlotPrefab)
        {
            SerializedObject serializedObject = new SerializedObject(shopUI);

            SerializedProperty goldTextProp = serializedObject.FindProperty("goldText");
            SerializedProperty itemListParentProp = serializedObject.FindProperty("itemListParent");
            SerializedProperty itemSlotPrefabProp = serializedObject.FindProperty("itemSlotPrefab");
            SerializedProperty messageTextProp = serializedObject.FindProperty("messageText");

            if (goldTextProp != null && itemListParentProp != null && itemSlotPrefabProp != null && messageTextProp != null)
            {
                goldTextProp.objectReferenceValue = goldText.GetComponent<TextMeshProUGUI>();
                itemListParentProp.objectReferenceValue = content;
                itemSlotPrefabProp.objectReferenceValue = itemSlotPrefab;
                messageTextProp.objectReferenceValue = messageText.GetComponent<TextMeshProUGUI>();

                serializedObject.ApplyModifiedProperties();

                Debug.Log("[ShopUICreator] ShopUI 참조 연결 완료");
            }
            else
            {
                Debug.LogError("[ShopUICreator] ShopUI 필드를 찾을 수 없습니다. 필드 이름을 확인하세요.");
            }
        }
    }
}
