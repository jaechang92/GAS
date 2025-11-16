using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using GASPT.UI;

namespace GASPT.Editor
{
    /// <summary>
    /// SkillUI 자동 생성 에디터 툴
    /// Tools > GASPT > Create Skill UI Panel
    /// </summary>
    public static class SkillUICreator
    {
        [MenuItem("Tools/GASPT/Create Skill UI Panel", priority = 50)]
        public static void CreateSkillUIPanel()
        {
            Debug.Log("========== Skill UI Panel 생성 시작 ==========");

            // Canvas 찾기 또는 생성
            Canvas canvas = EditorUtilities.FindOrCreateCanvas("[SkillUICreator]");

            // SkillUIPanel 생성
            GameObject panelObj = CreatePanel(canvas);
            SkillUIPanel panel = panelObj.AddComponent<SkillUIPanel>();

            // 4개의 SkillSlot 생성
            SkillSlotUI[] slots = new SkillSlotUI[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject slotObj = CreateSkillSlot(panelObj.transform, i);
                slots[i] = slotObj.GetComponent<SkillSlotUI>();
            }

            // SkillUIPanel에 슬롯 연결
            SerializedObject so = new SerializedObject(panel);
            SerializedProperty slotsProp = so.FindProperty("skillSlots");
            if (slotsProp != null && slotsProp.isArray)
            {
                slotsProp.arraySize = 4;
                for (int i = 0; i < 4; i++)
                {
                    slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
                }
                so.ApplyModifiedProperties();
            }

            // 선택
            Selection.activeGameObject = panelObj;

            Debug.Log("========== Skill UI Panel 생성 완료 ==========");
            Debug.Log($"✅ Canvas: {canvas.name}");
            Debug.Log($"✅ SkillUIPanel: {panelObj.name}");
            Debug.Log($"✅ SkillSlotUI 4개 생성 완료");
            Debug.Log($"✅ 모든 참조 자동 연결 완료");
        }



        // ====== Panel 생성 ======

        private static GameObject CreatePanel(Canvas canvas)
        {
            GameObject panelObj = new GameObject("SkillUIPanel");
            panelObj.transform.SetParent(canvas.transform, false);

            RectTransform rect = panelObj.AddComponent<RectTransform>();

            // 하단 중앙 배치
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 50f);
            rect.sizeDelta = new Vector2(400f, 80f);

            // 배경 (선택사항)
            Image bg = panelObj.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.3f);

            // Horizontal Layout
            HorizontalLayoutGroup layout = panelObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Debug.Log($"✓ SkillUIPanel 생성: {panelObj.name}");
            return panelObj;
        }


        // ====== SkillSlot 생성 ======

        private static GameObject CreateSkillSlot(Transform parent, int slotIndex)
        {
            GameObject slotObj = new GameObject($"SkillSlot_{slotIndex}");
            slotObj.transform.SetParent(parent, false);

            RectTransform rect = slotObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80f, 80f);

            // 배경
            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // SkillSlotUI 컴포넌트
            SkillSlotUI slotUI = slotObj.AddComponent<SkillSlotUI>();

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;
            iconImage.enabled = false;

            // Cooldown Overlay
            GameObject cooldownObj = new GameObject("CooldownOverlay");
            cooldownObj.transform.SetParent(slotObj.transform, false);
            RectTransform cooldownRect = cooldownObj.AddComponent<RectTransform>();
            cooldownRect.anchorMin = Vector2.zero;
            cooldownRect.anchorMax = Vector2.one;
            cooldownRect.offsetMin = Vector2.zero;
            cooldownRect.offsetMax = Vector2.zero;
            Image cooldownImage = cooldownObj.AddComponent<Image>();
            cooldownImage.color = new Color(0f, 0f, 0f, 0.7f);
            cooldownImage.type = Image.Type.Filled;
            cooldownImage.fillMethod = Image.FillMethod.Radial360;
            cooldownImage.fillOrigin = (int)Image.Origin360.Top;
            cooldownImage.fillClockwise = false;
            cooldownImage.enabled = false;

            // Cooldown Text
            GameObject cooldownTextObj = new GameObject("CooldownText");
            cooldownTextObj.transform.SetParent(slotObj.transform, false);
            RectTransform cooldownTextRect = cooldownTextObj.AddComponent<RectTransform>();
            cooldownTextRect.anchorMin = Vector2.zero;
            cooldownTextRect.anchorMax = Vector2.one;
            cooldownTextRect.offsetMin = Vector2.zero;
            cooldownTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI cooldownText = cooldownTextObj.AddComponent<TextMeshProUGUI>();
            cooldownText.text = "";
            cooldownText.fontSize = 20;
            cooldownText.fontStyle = FontStyles.Bold;
            cooldownText.alignment = TextAlignmentOptions.Center;
            cooldownText.color = Color.white;
            cooldownText.enabled = false;

            // Hotkey Text
            GameObject hotkeyObj = new GameObject("HotkeyText");
            hotkeyObj.transform.SetParent(slotObj.transform, false);
            RectTransform hotkeyRect = hotkeyObj.AddComponent<RectTransform>();
            hotkeyRect.anchorMin = new Vector2(0f, 0f);
            hotkeyRect.anchorMax = new Vector2(1f, 0f);
            hotkeyRect.pivot = new Vector2(0.5f, 0f);
            hotkeyRect.anchoredPosition = Vector2.zero;
            hotkeyRect.sizeDelta = new Vector2(0f, 20f);
            TextMeshProUGUI hotkeyText = hotkeyObj.AddComponent<TextMeshProUGUI>();
            hotkeyText.text = (slotIndex + 1).ToString();
            hotkeyText.fontSize = 14;
            hotkeyText.fontStyle = FontStyles.Bold;
            hotkeyText.alignment = TextAlignmentOptions.Center;
            hotkeyText.color = Color.yellow;

            // Disabled Overlay
            GameObject disabledObj = new GameObject("DisabledOverlay");
            disabledObj.transform.SetParent(slotObj.transform, false);
            RectTransform disabledRect = disabledObj.AddComponent<RectTransform>();
            disabledRect.anchorMin = Vector2.zero;
            disabledRect.anchorMax = Vector2.one;
            disabledRect.offsetMin = Vector2.zero;
            disabledRect.offsetMax = Vector2.zero;
            Image disabledImage = disabledObj.AddComponent<Image>();
            disabledImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            disabledImage.enabled = false;

            // SkillSlotUI 참조 연결
            SerializedObject so = new SerializedObject(slotUI);
            so.FindProperty("slotIndex").intValue = slotIndex;
            so.FindProperty("iconImage").objectReferenceValue = iconImage;
            so.FindProperty("cooldownOverlay").objectReferenceValue = cooldownImage;
            so.FindProperty("cooldownText").objectReferenceValue = cooldownText;
            so.FindProperty("hotkeyText").objectReferenceValue = hotkeyText;
            so.FindProperty("disabledOverlay").objectReferenceValue = disabledImage;

            // Hotkey 설정
            SerializedProperty hotkeyProp = so.FindProperty("hotkey");
            if (hotkeyProp != null)
            {
                switch (slotIndex)
                {
                    case 0: hotkeyProp.enumValueIndex = (int)KeyCode.Alpha1; break;
                    case 1: hotkeyProp.enumValueIndex = (int)KeyCode.Alpha2; break;
                    case 2: hotkeyProp.enumValueIndex = (int)KeyCode.Alpha3; break;
                    case 3: hotkeyProp.enumValueIndex = (int)KeyCode.Alpha4; break;
                }
            }

            so.ApplyModifiedProperties();

            Debug.Log($"✓ SkillSlot_{slotIndex} 생성 완료");
            return slotObj;
        }


        // ====== 유틸리티 ======

        [MenuItem("Tools/GASPT/Delete Skill UI Panel", priority = 51)]
        public static void DeleteSkillUIPanel()
        {
            SkillUIPanel panel = Object.FindAnyObjectByType<SkillUIPanel>();
            if (panel != null)
            {
                if (EditorUtility.DisplayDialog("Delete Skill UI Panel",
                    "SkillUIPanel을 삭제하시겠습니까?",
                    "삭제", "취소"))
                {
                    Object.DestroyImmediate(panel.gameObject);
                    Debug.Log("✓ SkillUIPanel 삭제 완료");
                }
            }
            else
            {
                Debug.LogWarning("SkillUIPanel을 찾을 수 없습니다.");
            }
        }
    }
}
