using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using GASPT.UI;
using System.IO;

namespace GASPT.EditorTools
{
    /// <summary>
    /// InventoryUI 자동 생성 에디터 도구
    /// Phase C-4: 아이템 드롭 및 장착 시스템
    /// </summary>
    public class InventoryUICreator : EditorWindow
    {
        // ====== 경로 ======

        private const string PrefabPath = "Assets/Resources/Prefabs/UI/";


        // ====== 에디터 창 ======

        [MenuItem("Tools/GASPT/UI/Create Inventory UI")]
        public static void ShowWindow()
        {
            InventoryUICreator window = GetWindow<InventoryUICreator>("Inventory UI Creator");
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
            EditorGUILayout.LabelField("Inventory UI Creator", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Phase C-4: 아이템 드롭 및 장착 시스템", EditorStyles.miniLabel);

            GUILayout.Space(10f);
            EditorGUILayout.HelpBox(
                "인벤토리 UI를 자동으로 생성합니다.\n" +
                "1. InventoryPanel (Canvas 자식으로 추가)\n" +
                "2. ItemSlot 프리팹 (Resources/Prefabs/UI/)\n" +
                "3. EquipmentSlot 프리팹 (Resources/Prefabs/UI/)",
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

            if (GUILayout.Button("1. InventoryPanel 생성 (Canvas 자식)"))
            {
                CreateInventoryPanel();
            }

            if (GUILayout.Button("2. ItemSlot 프리팹 생성"))
            {
                CreateItemSlotPrefab();
            }

            if (GUILayout.Button("3. EquipmentSlot 프리팹 생성"))
            {
                CreateEquipmentSlotPrefab();
            }

            EditorGUILayout.EndScrollView();
        }


        // ====== UI 생성 ======

        /// <summary>
        /// 모든 UI 자동 생성
        /// </summary>
        private void CreateAllUI()
        {
            CreateItemSlotPrefab();
            CreateEquipmentSlotPrefab();
            CreateInventoryPanel();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[InventoryUICreator] 모든 UI 생성 완료!");
            EditorUtility.DisplayDialog("완료", "인벤토리 UI가 생성되었습니다!", "확인");
        }

        /// <summary>
        /// InventoryPanel 생성 (Canvas 자식)
        /// </summary>
        private void CreateInventoryPanel()
        {
            // Canvas 찾기
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("오류", "Scene에 Canvas가 없습니다!", "확인");
                return;
            }

            // InventoryPanel 생성
            GameObject panel = new GameObject("InventoryPanel");
            panel.transform.SetParent(canvas.transform, false);

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
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

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
            titleText.text = "인벤토리";
            titleText.fontSize = 36f;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            // Item List Panel (ScrollView)
            CreateItemListPanel(panel);

            // Equipment Panel
            CreateEquipmentPanel(panel);

            // Close Button
            CreateCloseButton(panel);

            // InventoryUI 컴포넌트 추가
            InventoryUI inventoryUI = panel.AddComponent<InventoryUI>();

            // SerializedObject로 참조 설정
            SerializedObject so = new SerializedObject(inventoryUI);

            so.FindProperty("inventoryPanel").objectReferenceValue = panel;
            so.FindProperty("itemListContent").objectReferenceValue = panel.transform.Find("ItemListPanel/Viewport/Content");
            so.FindProperty("weaponSlot").objectReferenceValue = panel.transform.Find("EquipmentPanel/WeaponSlot")?.GetComponent<EquipmentSlotUI>();
            so.FindProperty("armorSlot").objectReferenceValue = panel.transform.Find("EquipmentPanel/ArmorSlot")?.GetComponent<EquipmentSlotUI>();
            so.FindProperty("ringSlot").objectReferenceValue = panel.transform.Find("EquipmentPanel/RingSlot")?.GetComponent<EquipmentSlotUI>();
            so.FindProperty("closeButton").objectReferenceValue = panel.transform.Find("CloseButton")?.GetComponent<Button>();

            // ItemSlot 프리팹 로드
            GameObject itemSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "ItemSlot.prefab");
            so.FindProperty("itemSlotPrefab").objectReferenceValue = itemSlotPrefab;

            so.ApplyModifiedProperties();

            Debug.Log("[InventoryUICreator] InventoryPanel 생성 완료");

            EditorGUIUtility.PingObject(panel);
            Selection.activeGameObject = panel;
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
            scrollRect.anchorMax = new Vector2(0.6f, 0.85f);
            scrollRect.offsetMin = new Vector2(20f, 100f);
            scrollRect.offsetMax = new Vector2(-20f, -100f);

            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

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
            layoutGroup.childControlHeight = false;
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
        /// EquipmentPanel 생성
        /// </summary>
        private void CreateEquipmentPanel(GameObject parent)
        {
            GameObject equipPanel = new GameObject("EquipmentPanel");
            equipPanel.transform.SetParent(parent.transform, false);

            RectTransform equipRect = equipPanel.AddComponent<RectTransform>();
            equipRect.anchorMin = new Vector2(0.65f, 0f);
            equipRect.anchorMax = new Vector2(1f, 0.85f);
            equipRect.offsetMin = new Vector2(20f, 100f);
            equipRect.offsetMax = new Vector2(-20f, -100f);

            Image equipBg = equipPanel.AddComponent<Image>();
            equipBg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

            // Vertical Layout
            VerticalLayoutGroup layout = equipPanel.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 20f;
            layout.padding = new RectOffset(20, 20, 20, 20);

            // Equipment Slots
            GameObject weaponSlot = CreateEquipmentSlot("WeaponSlot", equipPanel);
            GameObject armorSlot = CreateEquipmentSlot("ArmorSlot", equipPanel);
            GameObject ringSlot = CreateEquipmentSlot("RingSlot", equipPanel);
        }

        /// <summary>
        /// EquipmentSlot 생성 (프리팹 인스턴스화)
        /// </summary>
        private GameObject CreateEquipmentSlot(string name, GameObject parent)
        {
            // 프리팹 로드
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "EquipmentSlot.prefab");

            if (prefab == null)
            {
                Debug.LogError($"[InventoryUICreator] EquipmentSlot 프리팹을 찾을 수 없습니다: {PrefabPath}EquipmentSlot.prefab");
                return null;
            }

            // 프리팹 인스턴스화
            GameObject slot = PrefabUtility.InstantiatePrefab(prefab, parent.transform) as GameObject;
            slot.name = name;

            // SlotNameText만 업데이트
            TextMeshProUGUI slotNameText = slot.transform.Find("SlotNameText")?.GetComponent<TextMeshProUGUI>();
            if (slotNameText != null)
            {
                slotNameText.text = name.Replace("Slot", "");
            }

            return slot;
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
        /// ItemSlot 프리팹 생성
        /// </summary>
        private void CreateItemSlotPrefab()
        {
            // 폴더 확인
            if (!Directory.Exists(PrefabPath))
            {
                Directory.CreateDirectory(PrefabPath);
            }

            GameObject slot = new GameObject("ItemSlot");

            RectTransform slotRect = slot.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0f, 0.5f);
            slotRect.anchorMax = new Vector2(1f, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            slotRect.sizeDelta = new Vector2(0f, 80f);

            Image slotBg = slot.AddComponent<Image>();
            slotBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            // IconImage
            GameObject iconObj = new GameObject("IconImage");
            iconObj.transform.SetParent(slot.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(10f, 0f);
            iconRect.sizeDelta = new Vector2(60f, 60f);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // NameText
            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(slot.transform, false);

            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(0f, 0.5f);
            nameRect.pivot = new Vector2(0f, 0.5f);
            nameRect.anchoredPosition = new Vector2(80f, 10f);
            nameRect.sizeDelta = new Vector2(200f, 30f);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Item Name";
            nameText.fontSize = 20f;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.color = Color.white;

            // SlotText
            GameObject slotTextObj = new GameObject("SlotText");
            slotTextObj.transform.SetParent(slot.transform, false);

            RectTransform slotTextRect = slotTextObj.AddComponent<RectTransform>();
            slotTextRect.anchorMin = new Vector2(0f, 0.5f);
            slotTextRect.anchorMax = new Vector2(0f, 0.5f);
            slotTextRect.pivot = new Vector2(0f, 0.5f);
            slotTextRect.anchoredPosition = new Vector2(80f, -15f);
            slotTextRect.sizeDelta = new Vector2(200f, 25f);

            TextMeshProUGUI slotText = slotTextObj.AddComponent<TextMeshProUGUI>();
            slotText.text = "[Slot Type]";
            slotText.fontSize = 16f;
            slotText.alignment = TextAlignmentOptions.Left;
            slotText.color = Color.gray;

            // EquipButton
            GameObject buttonObj = new GameObject("EquipButton");
            buttonObj.transform.SetParent(slot.transform, false);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 0.5f);
            buttonRect.anchorMax = new Vector2(1f, 0.5f);
            buttonRect.pivot = new Vector2(1f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(-10f, 0f);
            buttonRect.sizeDelta = new Vector2(80f, 50f);

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
            btnText.text = "장착";
            btnText.fontSize = 18f;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;

            // 프리팹 저장
            string prefabPath = PrefabPath + "ItemSlot.prefab";
            PrefabUtility.SaveAsPrefabAsset(slot, prefabPath);

            DestroyImmediate(slot);

            Debug.Log($"[InventoryUICreator] ItemSlot 프리팹 생성 완료: {prefabPath}");
        }

        /// <summary>
        /// EquipmentSlot 프리팹 생성 (템플릿)
        /// </summary>
        private void CreateEquipmentSlotPrefab()
        {
            // 폴더 확인
            if (!Directory.Exists(PrefabPath))
            {
                Directory.CreateDirectory(PrefabPath);
            }

            GameObject slot = new GameObject("EquipmentSlot");

            RectTransform slotRect = slot.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0f, 0.5f);
            slotRect.anchorMax = new Vector2(1f, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            slotRect.sizeDelta = new Vector2(0f, 120f);

            Image slotBg = slot.AddComponent<Image>();
            slotBg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

            // SlotNameText
            GameObject slotNameObj = new GameObject("SlotNameText");
            slotNameObj.transform.SetParent(slot.transform, false);

            RectTransform slotNameRect = slotNameObj.AddComponent<RectTransform>();
            slotNameRect.anchorMin = new Vector2(0f, 1f);
            slotNameRect.anchorMax = new Vector2(1f, 1f);
            slotNameRect.pivot = new Vector2(0.5f, 1f);
            slotNameRect.anchoredPosition = new Vector2(0f, -10f);
            slotNameRect.sizeDelta = new Vector2(-20f, 30f);

            TextMeshProUGUI slotNameText = slotNameObj.AddComponent<TextMeshProUGUI>();
            slotNameText.text = "Slot";
            slotNameText.fontSize = 20f;
            slotNameText.alignment = TextAlignmentOptions.Center;
            slotNameText.color = Color.yellow;

            // IconImage
            GameObject iconObj = new GameObject("IconImage");
            iconObj.transform.SetParent(slot.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(10f, -10f);
            iconRect.sizeDelta = new Vector2(60f, 60f);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            // ItemNameText
            GameObject itemNameObj = new GameObject("ItemNameText");
            itemNameObj.transform.SetParent(slot.transform, false);

            RectTransform itemNameRect = itemNameObj.AddComponent<RectTransform>();
            itemNameRect.anchorMin = new Vector2(0f, 0.5f);
            itemNameRect.anchorMax = new Vector2(1f, 0.5f);
            itemNameRect.pivot = new Vector2(0.5f, 0.5f);
            itemNameRect.anchoredPosition = new Vector2(40f, -10f);
            itemNameRect.sizeDelta = new Vector2(-100f, 40f);

            TextMeshProUGUI itemNameText = itemNameObj.AddComponent<TextMeshProUGUI>();
            itemNameText.text = "비어있음";
            itemNameText.fontSize = 18f;
            itemNameText.alignment = TextAlignmentOptions.Left;
            itemNameText.color = Color.gray;

            // EmptySlotObject
            GameObject emptyObj = new GameObject("EmptySlotObject");
            emptyObj.transform.SetParent(slot.transform, false);

            RectTransform emptyRect = emptyObj.AddComponent<RectTransform>();
            emptyRect.anchorMin = Vector2.zero;
            emptyRect.anchorMax = Vector2.one;
            emptyRect.sizeDelta = Vector2.zero;

            // EquipmentSlotUI 컴포넌트 추가
            EquipmentSlotUI slotUI = slot.AddComponent<EquipmentSlotUI>();

            // SerializedObject로 참조 설정
            SerializedObject so = new SerializedObject(slotUI);
            so.FindProperty("slotNameText").objectReferenceValue = slotNameText;
            so.FindProperty("iconImage").objectReferenceValue = iconImage;
            so.FindProperty("itemNameText").objectReferenceValue = itemNameText;
            so.FindProperty("emptySlotObject").objectReferenceValue = emptyObj;
            so.ApplyModifiedProperties();

            // 프리팹 저장
            string prefabPath = PrefabPath + "EquipmentSlot.prefab";
            PrefabUtility.SaveAsPrefabAsset(slot, prefabPath);

            DestroyImmediate(slot);

            Debug.Log($"[InventoryUICreator] EquipmentSlot 프리팹 생성 완료: {prefabPath}");
        }


        // ====== UI 삭제 ======

        /// <summary>
        /// 모든 UI 삭제
        /// </summary>
        private void DeleteAllUI()
        {
            if (!EditorUtility.DisplayDialog("확인", "모든 인벤토리 UI를 삭제하시겠습니까?", "삭제", "취소"))
                return;

            // InventoryPanel 삭제 (Scene에서)
            InventoryUI inventoryUI = FindAnyObjectByType<InventoryUI>();
            if (inventoryUI != null)
            {
                DestroyImmediate(inventoryUI.gameObject);
                Debug.Log("[InventoryUICreator] InventoryPanel 삭제 완료");
            }

            // ItemSlot 프리팹 삭제
            string itemSlotPath = PrefabPath + "ItemSlot.prefab";
            if (File.Exists(itemSlotPath))
            {
                AssetDatabase.DeleteAsset(itemSlotPath);
                Debug.Log("[InventoryUICreator] ItemSlot 프리팹 삭제 완료");
            }

            // EquipmentSlot 프리팹 삭제
            string equipmentSlotPath = PrefabPath + "EquipmentSlot.prefab";
            if (File.Exists(equipmentSlotPath))
            {
                AssetDatabase.DeleteAsset(equipmentSlotPath);
                Debug.Log("[InventoryUICreator] EquipmentSlot 프리팹 삭제 완료");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("완료", "모든 인벤토리 UI가 삭제되었습니다.", "확인");
        }
    }
}
