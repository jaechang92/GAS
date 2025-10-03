using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UI.HUD;
using System.IO;

namespace Editor.UI
{
    /// <summary>
    /// HUD 프리팹 자동 생성 Editor 스크립트
    /// </summary>
    public static class HUDPrefabCreator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/UI";
        private const string PREFAB_NAME = "GameHUD";

        [MenuItem("GASPT/UI/Create HUD Prefab")]
        public static void CreateHUDPrefab()
        {
            // 프리팹 폴더 생성
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }

            // 기존 HUD Canvas 찾기 (있으면 삭제할지 물어봄)
            GameObject existingHUD = GameObject.Find("GameHUD");
            if (existingHUD != null)
            {
                if (!EditorUtility.DisplayDialog("HUD 생성",
                    "씬에 이미 GameHUD가 있습니다. 삭제하고 새로 만들까요?",
                    "예", "아니오"))
                {
                    return;
                }
                Object.DestroyImmediate(existingHUD);
            }

            // HUD 생성
            GameObject hudRoot = CreateHUDCanvas();

            // 프리팹으로 저장
            string prefabPath = $"{PREFAB_PATH}/{PREFAB_NAME}.prefab";
            PrefabUtility.SaveAsPrefabAsset(hudRoot, prefabPath);

            // 선택
            Selection.activeGameObject = hudRoot;

            Debug.Log($"[HUDPrefabCreator] HUD 프리팹 생성 완료: {prefabPath}");
            EditorUtility.DisplayDialog("완료", $"HUD 프리팹이 생성되었습니다!\n경로: {prefabPath}", "확인");
        }

        private static GameObject CreateHUDCanvas()
        {
            // Canvas 생성
            GameObject canvasObj = new GameObject("GameHUD");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // HUDManager 추가
            HUDManager hudManager = canvasObj.AddComponent<HUDManager>();

            // PlayerInfoPanel 생성 (왼쪽 하단)
            GameObject playerPanel = CreatePlayerInfoPanel(canvasObj.transform);

            // ResourcePanel 생성 (오른쪽 하단)
            GameObject resourcePanel = CreateResourcePanel(canvasObj.transform);

            // HUDManager에 참조 연결
            SerializedObject so = new SerializedObject(hudManager);
            so.FindProperty("playerInfoPanel").objectReferenceValue = playerPanel.GetComponent<PlayerInfoPanel>();
            so.FindProperty("resourcePanel").objectReferenceValue = resourcePanel.GetComponent<ResourcePanel>();
            so.ApplyModifiedProperties();

            return canvasObj;
        }

        private static GameObject CreatePlayerInfoPanel(Transform parent)
        {
            // Panel Root
            GameObject panel = new GameObject("PlayerInfoPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(20, 20);
            rt.sizeDelta = new Vector2(400, 150);

            PlayerInfoPanel panelScript = panel.AddComponent<PlayerInfoPanel>();

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(panel.transform, false);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            // Character Icon
            GameObject icon = new GameObject("CharacterIcon");
            icon.transform.SetParent(panel.transform, false);
            Image iconImage = icon.AddComponent<Image>();
            iconImage.color = new Color(0.8f, 0.8f, 0.8f);
            RectTransform iconRt = icon.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0, 0.5f);
            iconRt.anchorMax = new Vector2(0, 0.5f);
            iconRt.pivot = new Vector2(0, 0.5f);
            iconRt.anchoredPosition = new Vector2(10, 0);
            iconRt.sizeDelta = new Vector2(80, 80);

            // Health Bar
            GameObject healthBar = CreateHealthBar(panel.transform);

            // Item Slots
            GameObject slot1 = CreateItemSlot(panel.transform, "ItemSlot1", new Vector2(100, 10));
            GameObject slot2 = CreateItemSlot(panel.transform, "ItemSlot2", new Vector2(170, 10));

            // 참조 연결
            SerializedObject so = new SerializedObject(panelScript);
            so.FindProperty("characterIcon").objectReferenceValue = iconImage;
            so.FindProperty("healthBar").objectReferenceValue = healthBar.GetComponent<HealthBarUI>();

            SerializedProperty slotsArray = so.FindProperty("itemSlots");
            slotsArray.arraySize = 2;
            slotsArray.GetArrayElementAtIndex(0).objectReferenceValue = slot1.GetComponent<ItemSlotUI>();
            slotsArray.GetArrayElementAtIndex(1).objectReferenceValue = slot2.GetComponent<ItemSlotUI>();

            so.ApplyModifiedProperties();

            return panel;
        }

        private static GameObject CreateHealthBar(Transform parent)
        {
            GameObject healthBar = new GameObject("HealthBar");
            healthBar.transform.SetParent(parent, false);

            RectTransform rt = healthBar.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 100);
            rt.sizeDelta = new Vector2(-20, 30);

            HealthBarUI healthBarScript = healthBar.AddComponent<HealthBarUI>();

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(healthBar.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(healthBar.transform, false);
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = new Color(0.6f, 0.2f, 0.8f); // 보라색
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            RectTransform fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;

            // Text
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(healthBar.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = "100/100";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            // 참조 연결
            SerializedObject so = new SerializedObject(healthBarScript);
            so.FindProperty("fillImage").objectReferenceValue = fillImage;
            so.FindProperty("healthText").objectReferenceValue = text;
            so.ApplyModifiedProperties();

            return healthBar;
        }

        private static GameObject CreateItemSlot(Transform parent, string name, Vector2 position)
        {
            GameObject slot = new GameObject(name);
            slot.transform.SetParent(parent, false);

            RectTransform rt = slot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(60, 60);

            ItemSlotUI slotScript = slot.AddComponent<ItemSlotUI>();

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(slot.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slot.transform, false);
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = new Color(1, 1, 1, 0.3f);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.sizeDelta = new Vector2(-10, -10);

            // Cooldown Overlay
            GameObject cooldownObj = new GameObject("CooldownOverlay");
            cooldownObj.transform.SetParent(slot.transform, false);
            Image cooldownImage = cooldownObj.AddComponent<Image>();
            cooldownImage.color = new Color(0, 0, 0, 0.7f);
            cooldownImage.type = Image.Type.Filled;
            cooldownImage.fillMethod = Image.FillMethod.Radial360;
            cooldownImage.fillOrigin = (int)Image.Origin360.Top;
            cooldownImage.fillClockwise = false;
            RectTransform cooldownRt = cooldownObj.GetComponent<RectTransform>();
            cooldownRt.anchorMin = Vector2.zero;
            cooldownRt.anchorMax = Vector2.one;
            cooldownRt.sizeDelta = Vector2.zero;
            cooldownObj.SetActive(false);

            // Count Text
            GameObject countObj = new GameObject("CountText");
            countObj.transform.SetParent(slot.transform, false);
            Text countText = countObj.AddComponent<Text>();
            countText.text = "99";
            countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            countText.fontSize = 14;
            countText.color = Color.white;
            countText.alignment = TextAnchor.LowerRight;
            RectTransform countRt = countObj.GetComponent<RectTransform>();
            countRt.anchorMin = Vector2.zero;
            countRt.anchorMax = Vector2.one;
            countRt.sizeDelta = new Vector2(-5, -5);
            countObj.SetActive(false);

            // 참조 연결
            SerializedObject so = new SerializedObject(slotScript);
            so.FindProperty("iconImage").objectReferenceValue = iconImage;
            so.FindProperty("cooldownOverlay").objectReferenceValue = cooldownImage;
            so.FindProperty("countText").objectReferenceValue = countText;
            so.ApplyModifiedProperties();

            return slot;
        }

        private static GameObject CreateResourcePanel(Transform parent)
        {
            GameObject panel = new GameObject("ResourcePanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(1, 0);
            rt.anchoredPosition = new Vector2(-20, 20);
            rt.sizeDelta = new Vector2(250, 100);

            ResourcePanel panelScript = panel.AddComponent<ResourcePanel>();

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(panel.transform, false);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            // Gold Text
            GameObject goldObj = new GameObject("GoldText");
            goldObj.transform.SetParent(panel.transform, false);
            Text goldText = goldObj.AddComponent<Text>();
            goldText.text = "Gold: 0";
            goldText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            goldText.fontSize = 20;
            goldText.color = new Color(1f, 0.84f, 0f); // 금색
            goldText.alignment = TextAnchor.MiddleLeft;
            RectTransform goldRt = goldObj.GetComponent<RectTransform>();
            goldRt.anchorMin = new Vector2(0, 0.6f);
            goldRt.anchorMax = new Vector2(1, 1);
            goldRt.sizeDelta = new Vector2(-20, 0);
            goldRt.anchoredPosition = new Vector2(10, 0);

            // Diamond Text
            GameObject diamondObj = new GameObject("DiamondText");
            diamondObj.transform.SetParent(panel.transform, false);
            Text diamondText = diamondObj.AddComponent<Text>();
            diamondText.text = "Diamond: 0";
            diamondText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            diamondText.fontSize = 20;
            diamondText.color = new Color(0.4f, 0.8f, 1f); // 다이아 색
            diamondText.alignment = TextAnchor.MiddleLeft;
            RectTransform diamondRt = diamondObj.GetComponent<RectTransform>();
            diamondRt.anchorMin = new Vector2(0, 0.2f);
            diamondRt.anchorMax = new Vector2(1, 0.6f);
            diamondRt.sizeDelta = new Vector2(-20, 0);
            diamondRt.anchoredPosition = new Vector2(10, 0);

            // Settings Button
            GameObject buttonObj = new GameObject("SettingsButton");
            buttonObj.transform.SetParent(panel.transform, false);
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.3f);
            Button button = buttonObj.AddComponent<Button>();
            RectTransform buttonRt = buttonObj.GetComponent<RectTransform>();
            buttonRt.anchorMin = new Vector2(1, 0);
            buttonRt.anchorMax = new Vector2(1, 0);
            buttonRt.pivot = new Vector2(1, 0);
            buttonRt.anchoredPosition = new Vector2(-10, 10);
            buttonRt.sizeDelta = new Vector2(40, 40);

            // Button Text
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = "⚙";
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 24;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            RectTransform buttonTextRt = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRt.anchorMin = Vector2.zero;
            buttonTextRt.anchorMax = Vector2.one;
            buttonTextRt.sizeDelta = Vector2.zero;

            // 참조 연결
            SerializedObject so = new SerializedObject(panelScript);
            so.FindProperty("goldText").objectReferenceValue = goldText;
            so.FindProperty("diamondText").objectReferenceValue = diamondText;
            so.FindProperty("settingsButton").objectReferenceValue = button;
            so.FindProperty("goldFormat").stringValue = "Gold: {0}";
            so.FindProperty("diamondFormat").stringValue = "Diamond: {0}";
            so.ApplyModifiedProperties();

            return panel;
        }
    }
}
