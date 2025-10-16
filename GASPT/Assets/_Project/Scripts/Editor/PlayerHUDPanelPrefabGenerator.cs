using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UI.Panels;
using UI.Components;
using UI.Core;

namespace Editor.Tools
{
    /// <summary>
    /// PlayerHUDPanel Prefab을 프로그래매틱하게 생성하는 Editor 도구
    /// Menu: Tools/UI/Create PlayerHUDPanel Prefab
    /// </summary>
    public static class PlayerHUDPanelPrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Resources/UI/Panels/PlayerHUDPanel.prefab";
        private const string FOLDER_PATH = "Assets/_Project/Resources/UI/Panels";

        [MenuItem("Tools/UI/Create PlayerHUDPanel Prefab")]
        public static void CreatePlayerHUDPanelPrefab()
        {
            // 폴더 생성
            CreateFoldersIfNeeded();

            // GameObject 생성
            GameObject playerHUDObj = CreatePlayerHUDPanelHierarchy();

            // Prefab으로 저장
            SaveAsPrefab(playerHUDObj);

            Debug.Log($"[PlayerHUDPanelPrefabGenerator] PlayerHUDPanel Prefab 생성 완료: {PREFAB_PATH}");
        }

        private static void CreateFoldersIfNeeded()
        {
            string[] folders = FOLDER_PATH.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                    Debug.Log($"[PlayerHUDPanelPrefabGenerator] 폴더 생성: {nextPath}");
                }
                currentPath = nextPath;
            }
        }

        private static GameObject CreatePlayerHUDPanelHierarchy()
        {
            // Root: PlayerHUDPanel
            GameObject root = new GameObject("PlayerHUDPanel");

            // RectTransform 설정 (왼쪽 상단 배치)
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0, 1);
            rootRect.anchorMax = new Vector2(0, 1);
            rootRect.pivot = new Vector2(0, 1);
            rootRect.anchoredPosition = new Vector2(20, -20);
            rootRect.sizeDelta = new Vector2(300, 200);

            // HPBarContainer
            GameObject hpBarContainer = CreateHPBarContainer(root.transform);

            // MPBarContainer
            GameObject mpBarContainer = CreateMPBarContainer(root.transform);

            // SkillContainer
            GameObject skillContainer = CreateSkillContainer(root.transform);

            // PlayerHUDPanel Script 추가 및 참조 설정
            PlayerHUDPanel hudPanel = root.AddComponent<PlayerHUDPanel>();
            SetPlayerHUDPanelReferences(hudPanel, hpBarContainer, mpBarContainer, skillContainer);

            return root;
        }

        #region HP Bar

        private static GameObject CreateHPBarContainer(Transform parent)
        {
            GameObject container = new GameObject("HPBarContainer");
            container.transform.SetParent(parent, false);

            RectTransform rect = container.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0, 50);

            // 자식: HPBar (Slider)
            GameObject hpBar = CreateSlider(container.transform, "HPBar", new Color(0.8f, 0.2f, 0.2f, 1f));
            hpBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10);
            hpBar.GetComponent<RectTransform>().sizeDelta = new Vector2(-20, 25);

            // 자식: HPText
            GameObject hpText = CreateBarText(container.transform, "HPText", "100 / 100");
            hpText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10);

            return container;
        }

        #endregion

        #region MP Bar

        private static GameObject CreateMPBarContainer(Transform parent)
        {
            GameObject container = new GameObject("MPBarContainer");
            container.transform.SetParent(parent, false);

            RectTransform rect = container.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -55);
            rect.sizeDelta = new Vector2(0, 50);

            // 자식: MPBar (Slider)
            GameObject mpBar = CreateSlider(container.transform, "MPBar", new Color(0.2f, 0.4f, 0.8f, 1f));
            mpBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10);
            mpBar.GetComponent<RectTransform>().sizeDelta = new Vector2(-20, 25);

            // 자식: MPText
            GameObject mpText = CreateBarText(container.transform, "MPText", "100 / 100");
            mpText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10);

            return container;
        }

        #endregion

        #region Skill Container

        private static GameObject CreateSkillContainer(Transform parent)
        {
            GameObject container = new GameObject("SkillContainer");
            container.transform.SetParent(parent, false);

            RectTransform rect = container.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -110);
            rect.sizeDelta = new Vector2(0, 80);

            // Horizontal Layout Group
            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 15;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(10, 10, 10, 10);

            // Skill Slots
            GameObject skill1Slot = CreateSkillSlot(container.transform, "Skill1Slot", "Q");
            GameObject skill2Slot = CreateSkillSlot(container.transform, "Skill2Slot", "E");

            return container;
        }

        private static GameObject CreateSkillSlot(Transform parent, string name, string keyText)
        {
            GameObject slot = new GameObject(name);
            slot.transform.SetParent(parent, false);

            RectTransform rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(70, 70);

            // Background Image
            Image bgImage = slot.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            // SkillSlotUI Component
            SkillSlotUI slotUI = slot.AddComponent<SkillSlotUI>();

            // 자식: SkillIcon
            GameObject skillIcon = CreateImage(slot.transform, "SkillIcon", Color.white);
            RectTransform iconRect = skillIcon.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = new Vector2(-10, -10);
            iconRect.anchoredPosition = Vector2.zero;

            // 자식: CooldownOverlay (Radial Filled)
            GameObject cooldownOverlay = CreateImage(slot.transform, "CooldownOverlay", new Color(0, 0, 0, 0.7f));
            RectTransform overlayRect = cooldownOverlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;
            overlayRect.anchoredPosition = Vector2.zero;

            Image overlayImage = cooldownOverlay.GetComponent<Image>();
            overlayImage.type = Image.Type.Filled;
            overlayImage.fillMethod = Image.FillMethod.Radial360;
            overlayImage.fillOrigin = (int)Image.Origin360.Top;
            overlayImage.fillAmount = 0f;
            overlayImage.fillClockwise = false;

            // 자식: CooldownText
            GameObject cooldownText = CreateText(slot.transform, "CooldownText", "0", 32);
            RectTransform cdTextRect = cooldownText.GetComponent<RectTransform>();
            cdTextRect.anchorMin = Vector2.zero;
            cdTextRect.anchorMax = Vector2.one;
            cdTextRect.sizeDelta = Vector2.zero;
            cdTextRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI cdText = cooldownText.GetComponent<TextMeshProUGUI>();
            cdText.alignment = TextAlignmentOptions.Center;
            cdText.fontStyle = FontStyles.Bold;
            cooldownText.SetActive(false);

            // 자식: KeyText (하단 표시)
            GameObject keyTextObj = CreateText(slot.transform, "KeyText", keyText, 18);
            RectTransform keyRect = keyTextObj.GetComponent<RectTransform>();
            keyRect.anchorMin = new Vector2(0, 0);
            keyRect.anchorMax = new Vector2(1, 0);
            keyRect.pivot = new Vector2(0.5f, 0);
            keyRect.anchoredPosition = new Vector2(0, 5);
            keyRect.sizeDelta = new Vector2(0, 25);

            TextMeshProUGUI keyTMP = keyTextObj.GetComponent<TextMeshProUGUI>();
            keyTMP.alignment = TextAlignmentOptions.Center;
            keyTMP.fontStyle = FontStyles.Bold;

            // SkillSlotUI 참조 설정
            SetSkillSlotUIReferences(slotUI, skillIcon, cooldownOverlay, cooldownText, keyTextObj);

            return slot;
        }

        private static void SetSkillSlotUIReferences(SkillSlotUI slotUI, GameObject skillIcon, GameObject cooldownOverlay, GameObject cooldownText, GameObject keyText)
        {
            SerializedObject serializedSlot = new SerializedObject(slotUI);

            serializedSlot.FindProperty("skillIcon").objectReferenceValue = skillIcon.GetComponent<Image>();
            serializedSlot.FindProperty("cooldownOverlay").objectReferenceValue = cooldownOverlay.GetComponent<Image>();
            serializedSlot.FindProperty("cooldownText").objectReferenceValue = cooldownText.GetComponent<TextMeshProUGUI>();
            serializedSlot.FindProperty("keyText").objectReferenceValue = keyText.GetComponent<TextMeshProUGUI>();

            serializedSlot.FindProperty("showCooldownText").boolValue = true;
            serializedSlot.FindProperty("cooldownColor").colorValue = new Color(0, 0, 0, 0.7f);

            serializedSlot.ApplyModifiedProperties();
        }

        #endregion

        #region Helper Methods

        private static GameObject CreateSlider(Transform parent, string name, Color fillColor)
        {
            GameObject slider = new GameObject(name);
            slider.transform.SetParent(parent, false);

            RectTransform rect = slider.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Slider sliderComponent = slider.AddComponent<Slider>();
            sliderComponent.minValue = 0f;
            sliderComponent.maxValue = 1f;
            sliderComponent.value = 1f;

            // Background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(slider.transform, false);

            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(slider.transform, false);

            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;
            fillAreaRect.anchoredPosition = Vector2.zero;

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);

            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;

            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

            // Slider 참조 설정
            sliderComponent.fillRect = fillRect;
            sliderComponent.targetGraphic = fillImage;

            // Handle Slide Area 없음 (읽기 전용 바)

            return slider;
        }

        private static GameObject CreateBarText(Transform parent, string name, string text)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(-20, 25);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 16;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return textObj;
        }

        private static GameObject CreateImage(Transform parent, string name, Color color)
        {
            GameObject imageObj = new GameObject(name);
            imageObj.transform.SetParent(parent, false);

            RectTransform rect = imageObj.AddComponent<RectTransform>();

            Image image = imageObj.AddComponent<Image>();
            image.color = color;

            return imageObj;
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;

            return textObj;
        }

        #endregion

        #region Set References

        private static void SetPlayerHUDPanelReferences(PlayerHUDPanel panel, GameObject hpBarContainer, GameObject mpBarContainer, GameObject skillContainer)
        {
            SerializedObject serializedPanel = new SerializedObject(panel);

            // HP Bar 참조
            Slider hpBar = hpBarContainer.transform.Find("HPBar").GetComponent<Slider>();
            TextMeshProUGUI hpText = hpBarContainer.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
            serializedPanel.FindProperty("hpBar").objectReferenceValue = hpBar;
            serializedPanel.FindProperty("hpText").objectReferenceValue = hpText;

            // MP Bar 참조
            Slider mpBar = mpBarContainer.transform.Find("MPBar").GetComponent<Slider>();
            TextMeshProUGUI mpText = mpBarContainer.transform.Find("MPText").GetComponent<TextMeshProUGUI>();
            serializedPanel.FindProperty("mpBar").objectReferenceValue = mpBar;
            serializedPanel.FindProperty("mpText").objectReferenceValue = mpText;

            // Skill Slots 참조
            SkillSlotUI skill1Slot = skillContainer.transform.Find("Skill1Slot").GetComponent<SkillSlotUI>();
            SkillSlotUI skill2Slot = skillContainer.transform.Find("Skill2Slot").GetComponent<SkillSlotUI>();
            serializedPanel.FindProperty("skill1Slot").objectReferenceValue = skill1Slot;
            serializedPanel.FindProperty("skill2Slot").objectReferenceValue = skill2Slot;

            // 스킬 설정 (KeyCode, Cooldown)
            serializedPanel.FindProperty("skill1Key").enumValueIndex = System.Array.IndexOf(System.Enum.GetValues(typeof(KeyCode)), KeyCode.Q);
            serializedPanel.FindProperty("skill2Key").enumValueIndex = System.Array.IndexOf(System.Enum.GetValues(typeof(KeyCode)), KeyCode.E);
            serializedPanel.FindProperty("skill1Cooldown").floatValue = 5f;
            serializedPanel.FindProperty("skill2Cooldown").floatValue = 8f;

            // 디버그 설정
            serializedPanel.FindProperty("showDebugLog").boolValue = true;

            // BasePanel 설정 (PanelType, UILayer)
            SerializedProperty panelTypeProperty = serializedPanel.FindProperty("panelType");
            if (panelTypeProperty != null)
            {
                panelTypeProperty.enumValueIndex = System.Array.IndexOf(System.Enum.GetValues(typeof(PanelType)), PanelType.PlayerHUD);
            }

            SerializedProperty layerProperty = serializedPanel.FindProperty("layer");
            if (layerProperty != null)
            {
                layerProperty.enumValueIndex = System.Array.IndexOf(System.Enum.GetValues(typeof(UILayer)), UILayer.Normal);
            }

            serializedPanel.ApplyModifiedProperties();
        }

        #endregion

        #region Save Prefab

        private static void SaveAsPrefab(GameObject obj)
        {
            // 기존 Prefab이 있으면 삭제
            if (File.Exists(PREFAB_PATH))
            {
                AssetDatabase.DeleteAsset(PREFAB_PATH);
            }

            // Prefab으로 저장
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, PREFAB_PATH);

            // Scene에서 제거
            Object.DestroyImmediate(obj);

            // Prefab 선택
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion
    }
}
