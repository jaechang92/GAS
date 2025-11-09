using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using GASPT.UI;

namespace GASPT.Editor
{
    /// <summary>
    /// PlayerManaBar UI 자동 생성 에디터 도구
    /// Tools > GASPT > Create Player ManaBar UI
    /// </summary>
    public class PlayerManaBarCreator : EditorWindow
    {
        [MenuItem("Tools/GASPT/Create Player ManaBar UI")]
        public static void CreatePlayerManaBarUI()
        {
            // 1. Canvas 찾기 또는 생성
            Canvas canvas = FindOrCreateCanvas();

            // 2. PlayerManaBar 패널 생성
            GameObject manaBarPanel = CreateManaBarPanel(canvas);

            // 3. 완료 메시지
            Debug.Log($"[PlayerManaBarCreator] PlayerManaBar UI 생성 완료: {manaBarPanel.name}");
            EditorUtility.DisplayDialog(
                "PlayerManaBar UI 생성 완료",
                $"PlayerManaBar UI가 생성되었습니다!\n\n" +
                $"경로: {manaBarPanel.name}\n\n" +
                $"HealthBar 아래에 배치되었습니다.\n" +
                $"Canvas를 선택하여 위치를 조정하세요.",
                "확인"
            );

            // 4. Hierarchy에서 선택
            Selection.activeGameObject = manaBarPanel;
        }


        // ====== Canvas 생성/찾기 ======

        private static Canvas FindOrCreateCanvas()
        {
            // Scene에서 Canvas 찾기
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();

            if (canvas != null)
            {
                Debug.Log("[PlayerManaBarCreator] 기존 Canvas 사용");
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

            Debug.Log("[PlayerManaBarCreator] Canvas 생성 완료");
            return canvas;
        }


        // ====== PlayerManaBar 패널 생성 ======

        private static GameObject CreateManaBarPanel(Canvas canvas)
        {
            // 패널 생성
            GameObject panel = new GameObject("PlayerManaBar");
            panel.transform.SetParent(canvas.transform, false);

            // RectTransform 설정
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f); // 상단 중앙
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0, -100); // 상단에서 100px 아래 (HealthBar 아래)
            panelRect.sizeDelta = new Vector2(400, 40); // 너비 400, 높이 40

            // 배경 이미지 (선택사항)
            Image panelImage = panel.AddComponent<Image>();
            panelImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            panelImage.type = Image.Type.Sliced;
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f); // 반투명 검은색

            // 1. Background 생성
            GameObject background = CreateBackground(panel);

            // 2. Mana Slider 생성
            Slider manaSlider = CreateManaSlider(panel);

            // 3. Mana Text 생성
            TextMeshProUGUI manaText = CreateManaText(panel);

            // 4. PlayerManaBar 스크립트 추가
            PlayerManaBar manaBarScript = panel.AddComponent<PlayerManaBar>();

            // 5. SerializedObject로 private 필드 설정
            SerializedObject so = new SerializedObject(manaBarScript);

            SerializedProperty manaSliderProp = so.FindProperty("manaSlider");
            SerializedProperty manaTextProp = so.FindProperty("manaText");
            SerializedProperty fillImageProp = so.FindProperty("fillImage");

            if (manaSliderProp != null)
            {
                manaSliderProp.objectReferenceValue = manaSlider;
            }

            if (manaTextProp != null)
            {
                manaTextProp.objectReferenceValue = manaText;
            }

            if (fillImageProp != null)
            {
                // Slider의 Fill 이미지 가져오기
                Image fillImage = manaSlider.fillRect?.GetComponent<Image>();
                fillImageProp.objectReferenceValue = fillImage;
            }

            so.ApplyModifiedProperties();

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


        // ====== Mana Slider 생성 ======

        private static Slider CreateManaSlider(GameObject parent)
        {
            GameObject sliderObj = new GameObject("ManaSlider");
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
            fillImage.color = new Color(0.2f, 0.4f, 1f); // 파란색

            // Slider 참조 설정
            slider.fillRect = fillRect;

            return slider;
        }


        // ====== Mana Text 생성 ======

        private static TextMeshProUGUI CreateManaText(GameObject parent)
        {
            GameObject textObj = new GameObject("ManaText");
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


        // ====== Delete 유틸리티 ======

        [MenuItem("Tools/GASPT/Delete Player ManaBar UI")]
        public static void DeletePlayerManaBarUI()
        {
            GameObject manaBar = GameObject.Find("PlayerManaBar");

            if (manaBar != null)
            {
                DestroyImmediate(manaBar);
                Debug.Log("[PlayerManaBarCreator] PlayerManaBar UI 삭제 완료");
                EditorUtility.DisplayDialog(
                    "PlayerManaBar UI 삭제 완료",
                    "PlayerManaBar UI가 삭제되었습니다.",
                    "확인"
                );
            }
            else
            {
                Debug.LogWarning("[PlayerManaBarCreator] PlayerManaBar UI를 찾을 수 없습니다.");
                EditorUtility.DisplayDialog(
                    "PlayerManaBar UI 삭제 실패",
                    "PlayerManaBar UI를 찾을 수 없습니다.",
                    "확인"
                );
            }
        }
    }
}
