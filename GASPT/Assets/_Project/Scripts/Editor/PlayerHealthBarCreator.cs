using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using GASPT.UI;

namespace GASPT.Editor
{
    /// <summary>
    /// PlayerHealthBar UI 자동 생성 에디터 도구
    /// Tools > GASPT > Create Player HealthBar UI
    /// </summary>
    public class PlayerHealthBarCreator : EditorWindow
    {
        [MenuItem("Tools/GASPT/Create Player HealthBar UI")]
        public static void CreatePlayerHealthBarUI()
        {
            // 1. Canvas 찾기 또는 생성
            Canvas canvas = FindOrCreateCanvas();

            // 2. PlayerHealthBar 패널 생성
            GameObject healthBarPanel = CreateHealthBarPanel(canvas);

            // 3. 완료 메시지
            Debug.Log($"[PlayerHealthBarCreator] PlayerHealthBar UI 생성 완료: {healthBarPanel.name}");
            EditorUtility.DisplayDialog(
                "PlayerHealthBar UI 생성 완료",
                $"PlayerHealthBar UI가 생성되었습니다!\n\n" +
                $"경로: {healthBarPanel.name}\n\n" +
                $"Canvas를 선택하여 위치를 조정하세요.",
                "확인"
            );

            // 4. Hierarchy에서 선택
            Selection.activeGameObject = healthBarPanel;
        }


        // ====== Canvas 생성/찾기 ======

        private static Canvas FindOrCreateCanvas()
        {
            // Scene에서 Canvas 찾기
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();

            if (canvas != null)
            {
                Debug.Log("[PlayerHealthBarCreator] 기존 Canvas 사용");
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

            Debug.Log("[PlayerHealthBarCreator] Canvas 생성 완료");
            return canvas;
        }


        // ====== PlayerHealthBar 패널 생성 ======

        private static GameObject CreateHealthBarPanel(Canvas canvas)
        {
            // 패널 생성
            GameObject panel = new GameObject("PlayerHealthBar");
            panel.transform.SetParent(canvas.transform, false);

            // RectTransform 설정
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f); // 상단 중앙
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0, -50); // 상단에서 50px 아래
            panelRect.sizeDelta = new Vector2(400, 40); // 너비 400, 높이 40

            // 배경 이미지 (선택사항)
            Image panelImage = panel.AddComponent<Image>();
            panelImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            panelImage.type = Image.Type.Sliced;
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f); // 반투명 검은색

            // 1. Background 생성
            GameObject background = CreateBackground(panel);

            // 2. HP Slider 생성
            Slider hpSlider = CreateHPSlider(panel);

            // 3. HP Text 생성
            TextMeshProUGUI hpText = CreateHPText(panel);

            // 4. PlayerHealthBar 스크립트 추가 및 참조 연결
            PlayerHealthBar healthBarScript = panel.AddComponent<PlayerHealthBar>();

            // Reflection으로 private 필드 설정
            var hpSliderField = typeof(PlayerHealthBar).GetField("hpSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hpTextField = typeof(PlayerHealthBar).GetField("hpText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fillImageField = typeof(PlayerHealthBar).GetField("fillImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (hpSliderField != null)
            {
                hpSliderField.SetValue(healthBarScript, hpSlider);
            }

            if (hpTextField != null)
            {
                hpTextField.SetValue(healthBarScript, hpText);
            }

            if (fillImageField != null)
            {
                // Slider의 Fill 이미지 가져오기
                Image fillImage = hpSlider.fillRect?.GetComponent<Image>();
                fillImageField.SetValue(healthBarScript, fillImage);
            }

            // EditorUtility로 변경사항 저장
            EditorUtility.SetDirty(healthBarScript);

            return panel;
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
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(-20, 20); // 패널보다 양옆 10px 작게

            Image bgImage = background.AddComponent<Image>();
            bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            bgImage.type = Image.Type.Sliced;
            bgImage.color = new Color(0.2f, 0.2f, 0.2f); // 어두운 회색

            return background;
        }


        // ====== HP Slider 생성 ======

        private static Slider CreateHPSlider(GameObject parent)
        {
            GameObject sliderObj = new GameObject("HPSlider");
            sliderObj.transform.SetParent(parent.transform, false);

            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0.5f);
            sliderRect.anchorMax = new Vector2(1f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = Vector2.zero;
            sliderRect.sizeDelta = new Vector2(-20, 20);

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
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
            fillImage.color = new Color(0.2f, 0.8f, 0.2f); // 녹색

            // Slider 참조 설정
            slider.fillRect = fillRect;

            return slider;
        }


        // ====== HP Text 생성 ======

        private static TextMeshProUGUI CreateHPText(GameObject parent)
        {
            GameObject textObj = new GameObject("HPText");
            textObj.transform.SetParent(parent.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = "100/100";
            tmpText.fontSize = 18;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontStyle = FontStyles.Bold;

            return tmpText;
        }
    }
}
