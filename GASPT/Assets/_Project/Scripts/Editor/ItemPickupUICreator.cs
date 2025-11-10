using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;

namespace GASPT.Editor
{
    /// <summary>
    /// ItemPickupUI 자동 생성 에디터 도구
    /// Tools > GASPT > UI > Create Item Pickup UI
    /// </summary>
    public static class ItemPickupUICreator
    {
        private const string MENU_PATH = "Tools/GASPT/UI/Create Item Pickup UI";
        private const string PREFAB_PATH = "Assets/Resources/Prefabs/UI";
        private const string PREFAB_NAME = "ItemPickupSlot.prefab";


        [MenuItem(MENU_PATH)]
        public static void CreateItemPickupUI()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = FindOrCreateCanvas();

            // ItemPickupUIPanel 생성
            GameObject panel = CreateItemPickupUIPanel(canvas);

            // ItemPickupSlot 프리팹 생성
            GameObject slotPrefab = CreateItemPickupSlotPrefab();

            // ItemPickupUI 컴포넌트 참조 연결
            ConnectReferences(panel, slotPrefab);

            Debug.Log($"[ItemPickupUICreator] ItemPickupUI 생성 완료!");
            Debug.Log($"  - Panel: {panel.name}");
            Debug.Log($"  - Prefab: {PREFAB_PATH}/{PREFAB_NAME}");

            Selection.activeGameObject = panel;
        }


        // ====== Canvas 찾기/생성 ======

        private static Canvas FindOrCreateCanvas()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();

            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObj.AddComponent<GraphicRaycaster>();

                Debug.Log("[ItemPickupUICreator] Canvas 생성 완료");
            }

            return canvas;
        }


        // ====== ItemPickupUIPanel 생성 ======

        private static GameObject CreateItemPickupUIPanel(Canvas canvas)
        {
            GameObject panel = new GameObject("ItemPickupUIPanel");
            panel.transform.SetParent(canvas.transform, false);

            // RectTransform 설정 (화면 상단 중앙)
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -50f);
            rt.sizeDelta = new Vector2(400f, 300f);

            // VerticalLayoutGroup 추가
            VerticalLayoutGroup layoutGroup = panel.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 10f;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            // ItemPickupUI 컴포넌트 추가
            panel.AddComponent<UI.ItemPickupUI>();

            return panel;
        }


        // ====== ItemPickupSlot 프리팹 생성 ======

        private static GameObject CreateItemPickupSlotPrefab()
        {
            // 임시 오브젝트 생성
            GameObject slot = new GameObject("ItemPickupSlot");

            // RectTransform 설정
            RectTransform rt = slot.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(350f, 60f);

            // CanvasGroup 추가
            slot.AddComponent<CanvasGroup>();

            // 배경 이미지
            Image bgImage = slot.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.8f);

            // 자식 오브젝트들 생성
            GameObject iconObj = CreateIconObject(slot.transform);
            GameObject textObj = CreateTextObject(slot.transform);

            // ItemPickupSlot 컴포넌트 추가 및 참조 연결
            var slotComponent = slot.AddComponent<UI.ItemPickupSlot>();
            ConnectSlotReferences(slotComponent, iconObj, textObj, slot.GetComponent<CanvasGroup>());

            // 프리팹으로 저장
            GameObject prefab = SaveAsPrefab(slot);

            // 임시 오브젝트 삭제
            Object.DestroyImmediate(slot);

            return prefab;
        }

        private static GameObject CreateIconObject(Transform parent)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(parent, false);

            RectTransform rt = iconObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(40f, 0f);
            rt.sizeDelta = new Vector2(50f, 50f);

            Image image = iconObj.AddComponent<Image>();
            image.color = Color.white;

            return iconObj;
        }

        private static GameObject CreateTextObject(Transform parent)
        {
            GameObject textObj = new GameObject("NameText");
            textObj.transform.SetParent(parent, false);

            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(30f, 0f);
            rt.sizeDelta = new Vector2(-100f, 50f);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "아이템 획득!";
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Left;
            text.color = Color.white;

            return textObj;
        }


        // ====== 참조 연결 ======

        private static void ConnectReferences(GameObject panel, GameObject slotPrefab)
        {
            var pickupUI = panel.GetComponent<UI.ItemPickupUI>();
            if (pickupUI == null)
                return;

            SerializedObject so = new SerializedObject(pickupUI);

            // slotContainer는 panel 자신
            so.FindProperty("slotContainer").objectReferenceValue = panel.transform;

            // pickupSlotPrefab 연결
            so.FindProperty("pickupSlotPrefab").objectReferenceValue = slotPrefab;

            // maxSlots, displayDuration, fadeDuration은 기본값 사용

            so.ApplyModifiedProperties();

            Debug.Log("[ItemPickupUICreator] ItemPickupUI 참조 연결 완료");
        }

        private static void ConnectSlotReferences(UI.ItemPickupSlot slot, GameObject icon, GameObject text, CanvasGroup canvasGroup)
        {
            SerializedObject so = new SerializedObject(slot);

            so.FindProperty("iconImage").objectReferenceValue = icon.GetComponent<Image>();
            so.FindProperty("nameText").objectReferenceValue = text.GetComponent<TextMeshProUGUI>();
            so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;

            so.ApplyModifiedProperties();
        }


        // ====== 프리팹 저장 ======

        private static GameObject SaveAsPrefab(GameObject obj)
        {
            // Resources 폴더 생성
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }

            string fullPath = $"{PREFAB_PATH}/{PREFAB_NAME}";

            // 기존 프리팹 삭제
            if (File.Exists(fullPath))
            {
                AssetDatabase.DeleteAsset(fullPath);
            }

            // 프리팹 저장
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, fullPath);

            Debug.Log($"[ItemPickupUICreator] 프리팹 저장 완료: {fullPath}");

            return prefab;
        }


        // ====== 삭제 도구 ======

        [MenuItem("Tools/GASPT/UI/Delete Item Pickup UI")]
        public static void DeleteItemPickupUI()
        {
            GameObject panel = GameObject.Find("ItemPickupUIPanel");

            if (panel != null)
            {
                Object.DestroyImmediate(panel);
                Debug.Log("[ItemPickupUICreator] ItemPickupUIPanel 삭제 완료");
            }
            else
            {
                Debug.LogWarning("[ItemPickupUICreator] ItemPickupUIPanel을 찾을 수 없습니다.");
            }
        }
    }
}
