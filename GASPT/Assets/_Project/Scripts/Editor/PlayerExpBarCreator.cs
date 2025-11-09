using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using GASPT.UI;

namespace GASPT.Editor
{
    /// <summary>
    /// PlayerExpBar UI 자동 생성 에디터 도구
    /// Tools > GASPT > Create Player ExpBar UI
    /// </summary>
    public class PlayerExpBarCreator : EditorWindow
    {
        [MenuItem("Tools/GASPT/Create Player ExpBar UI")]
        public static void CreatePlayerExpBarUI()
        {
            // 1. Canvas 찾기 또는 생성
            Canvas canvas = FindOrCreateCanvas();

            // 2. PlayerExpBar 패널 생성
            GameObject expBarPanel = CreateExpBarPanel(canvas);

            // 3. 완료 메시지
            Debug.Log($"[PlayerExpBarCreator] PlayerExpBar UI 생성 완료: {expBarPanel.name}");
            EditorUtility.DisplayDialog(
                "PlayerExpBar UI 생성 완료",
                $"PlayerExpBar UI가 생성되었습니다!\n\n" +
                $"경로: {expBarPanel.name}\n\n" +
                $"Canvas를 선택하여 위치를 조정하세요.",
                "확인"
            );

            // 4. Hierarchy에서 선택
            Selection.activeGameObject = expBarPanel;
        }


        // ====== Canvas 생성/찾기 ======

        private static Canvas FindOrCreateCanvas()
        {
            // Scene에서 Canvas 찾기
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();

            if (canvas != null)
            {
                Debug.Log("[PlayerExpBarCreator] 기존 Canvas 사용");
                return canvas;
            }

            // Canvas 생성
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // CanvasScaler 추가
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // GraphicRaycaster 추가
            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem 확인/생성
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Debug.Log("[PlayerExpBarCreator] Canvas 생성 완료");
            return canvas;
        }


        // ====== PlayerExpBar 패널 생성 ======

        private static GameObject CreateExpBarPanel(Canvas canvas)
        {
            // 패널 생성
            GameObject panel = new GameObject("PlayerExpBar");
            panel.transform.SetParent(canvas.transform, false);

            // RectTransform 설정
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f); // 상단 중앙
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0, -100); // 상단에서 100px 아래 (HealthBar 아래)
            panelRect.sizeDelta = new Vector2(400, 50); // 너비 400, 높이 50

            // 배경 이미지 (선택사항)
            Image panelImage = panel.AddComponent<Image>();
            panelImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            panelImage.type = Image.Type.Sliced;
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f); // 반투명 검은색

            // 1. Background 생성
            GameObject background = CreateBackground(panel);

            // 2. EXP Slider 생성
            Slider expSlider = CreateExpSlider(panel);

            // 3. EXP Text 생성
            TextMeshProUGUI expText = CreateExpText(panel);

            // 4. Level Text 생성 (마지막에 생성하여 위에 렌더링)
            TextMeshProUGUI levelText = CreateLevelText(panel);

            // 5. PlayerExpBar 스크립트 추가 및 참조 연결
            PlayerExpBar expBarScript = panel.AddComponent<PlayerExpBar>();

            // Reflection으로 private 필드 설정
            var expSliderField = typeof(PlayerExpBar).GetField("expSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var expTextField = typeof(PlayerExpBar).GetField("expText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var levelTextField = typeof(PlayerExpBar).GetField("levelText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fillImageField = typeof(PlayerExpBar).GetField("fillImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (expSliderField != null)
            {
                expSliderField.SetValue(expBarScript, expSlider);
            }

            if (expTextField != null)
            {
                expTextField.SetValue(expBarScript, expText);
            }

            if (levelTextField != null)
            {
                levelTextField.SetValue(expBarScript, levelText);
            }

            if (fillImageField != null)
            {
                // Slider의 Fill 이미지 가져오기
                Image fillImage = expSlider.fillRect?.GetComponent<Image>();
                fillImageField.SetValue(expBarScript, fillImage);
            }

            // EditorUtility로 변경사항 저장
            EditorUtility.SetDirty(expBarScript);

            return panel;
        }


        // ====== Level Text 생성 ======

        private static TextMeshProUGUI CreateLevelText(GameObject parent)
        {
            GameObject textObj = new GameObject("LevelText");
            textObj.transform.SetParent(parent.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0.5f);
            textRect.anchorMax = new Vector2(0f, 0.5f);
            textRect.pivot = new Vector2(0f, 0.5f);
            textRect.anchoredPosition = new Vector2(10, 0); // 왼쪽에서 10px
            textRect.sizeDelta = new Vector2(80, 40);

            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = "Lv.1";
            tmpText.fontSize = 20;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.Left;
            tmpText.fontStyle = FontStyles.Bold;

            return tmpText;
        }


        // ====== Background 생성 ======

        private static GameObject CreateBackground(GameObject parent)
        {
            GameObject background = new GameObject("Background");
            background.transform.SetParent(parent.transform, false);

            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.5f);
            bgRect.anchorMax = new Vector2(1f, 0.5f);
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = new Vector2(50, 0); // 레벨 텍스트 옆으로
            bgRect.sizeDelta = new Vector2(-120, 20); // 레벨 텍스트 공간 제외

            Image bgImage = background.AddComponent<Image>();
            bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            bgImage.type = Image.Type.Sliced;
            bgImage.color = new Color(0.2f, 0.2f, 0.2f); // 어두운 회색

            return background;
        }


        // ====== EXP Slider 생성 ======

        private static Slider CreateExpSlider(GameObject parent)
        {
            GameObject sliderObj = new GameObject("ExpSlider");
            sliderObj.transform.SetParent(parent.transform, false);

            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0.5f);
            sliderRect.anchorMax = new Vector2(1f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(50, 0); // 레벨 텍스트 옆으로
            sliderRect.sizeDelta = new Vector2(-120, 20); // 레벨 텍스트 공간 제외

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.interactable = false; // 읽기 전용

            // Fill Area 생성
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);

            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;
            fillAreaRect.anchoredPosition = Vector2.zero;

            // Fill 생성
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);

            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;

            Image fillImage = fill.AddComponent<Image>();
            fillImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            fillImage.type = Image.Type.Sliced;
            fillImage.color = new Color(0.2f, 0.6f, 1f); // 파란색

            // Slider 참조 설정
            slider.fillRect = fillRect;

            return slider;
        }


        // ====== EXP Text 생성 ======

        private static TextMeshProUGUI CreateExpText(GameObject parent)
        {
            GameObject textObj = new GameObject("ExpText");
            textObj.transform.SetParent(parent.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0.5f);
            textRect.anchorMax = new Vector2(1f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(50, 0); // 레벨 텍스트 옆으로
            textRect.sizeDelta = new Vector2(-120, 40);

            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = "0/100";
            tmpText.fontSize = 16;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontStyle = FontStyles.Bold;

            return tmpText;
        }
    }
}
