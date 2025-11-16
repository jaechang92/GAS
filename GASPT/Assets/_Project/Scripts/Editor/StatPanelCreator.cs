using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using GASPT.UI;

namespace GASPT.Editor
{
    /// <summary>
    /// StatPanel UI 프리팹을 자동으로 생성하는 Editor Tool
    /// Menu: Tools > GASPT > Create StatPanel UI
    /// </summary>
    public static class StatPanelCreator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/UI/StatPanel.prefab";
        private const string LOG_PREFIX = "[StatPanelCreator]";

        /// <summary>
        /// StatPanel UI 프리팹 생성 (메뉴 항목)
        /// </summary>
        [MenuItem("Tools/GASPT/Create StatPanel UI")]
        public static void CreateStatPanelUI()
        {
            Debug.Log($"{LOG_PREFIX} StatPanel UI 생성 시작...");

            // 1. Canvas 찾기 또는 생성 (EditorUtilities 사용)
            Canvas canvas = EditorUtilities.FindOrCreateCanvas(LOG_PREFIX);

            // 2. StatPanel GameObject 생성
            GameObject statPanel = CreateStatPanelGameObject(canvas.transform);

            // 3. Background Image 추가
            CreateBackgroundImage(statPanel.transform);

            // 4. TextMeshPro 텍스트 3개 추가
            GameObject hpText = CreateStatText(statPanel.transform, "HP_Text", "HP: 100", -30f);
            GameObject attackText = CreateStatText(statPanel.transform, "Attack_Text", "Attack: 10", -70f);
            GameObject defenseText = CreateStatText(statPanel.transform, "Defense_Text", "Defense: 5", -110f);

            // 5. StatPanelUI 스크립트 추가 및 참조 연결
            StatPanelUI statPanelUI = statPanel.AddComponent<StatPanelUI>();
            AssignReferences(statPanelUI, hpText, attackText, defenseText);

            // 6. 프리팹으로 저장 (EditorUtilities 사용)
            EditorUtilities.SaveAsPrefab(statPanel, PREFAB_PATH, LOG_PREFIX);

            Debug.Log($"{LOG_PREFIX} StatPanel UI 생성 완료! 프리팹 경로: {PREFAB_PATH}");
            Debug.Log($"{LOG_PREFIX} Scene에 생성된 StatPanel은 프리팹으로 저장되었습니다. Scene에서 사용하려면 프리팹을 드래그하세요.");

            // Scene에서 선택
            Selection.activeGameObject = statPanel;
        }


        // ====== StatPanel GameObject 생성 ======

        /// <summary>
        /// StatPanel GameObject 생성 및 RectTransform 설정
        /// </summary>
        private static GameObject CreateStatPanelGameObject(Transform parent)
        {
            GameObject statPanel = new GameObject("StatPanel");
            statPanel.transform.SetParent(parent, false);

            RectTransform rectTransform = statPanel.AddComponent<RectTransform>();

            // Anchor: Top Left
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            // Position and Size
            rectTransform.anchoredPosition = new Vector2(20, -20);
            rectTransform.sizeDelta = new Vector2(300, 150);

            Debug.Log("[StatPanelCreator] StatPanel GameObject 생성 완료");
            return statPanel;
        }


        // ====== Background Image 생성 ======

        /// <summary>
        /// Background Image 추가
        /// </summary>
        private static void CreateBackgroundImage(Transform parent)
        {
            GameObject background = new GameObject("Background");
            background.transform.SetParent(parent, false);

            RectTransform rectTransform = background.AddComponent<RectTransform>();

            // Stretch (가로/세로 모두)
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Image 컴포넌트 추가
            Image image = background.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.6f); // 검은색 반투명

            Debug.Log("[StatPanelCreator] Background Image 생성 완료");
        }


        // ====== TextMeshPro 텍스트 생성 ======

        /// <summary>
        /// StatText (TextMeshPro) 생성
        /// </summary>
        /// <param name="parent">부모 Transform</param>
        /// <param name="name">GameObject 이름</param>
        /// <param name="text">표시할 텍스트</param>
        /// <param name="posY">Y 위치</param>
        private static GameObject CreateStatText(Transform parent, string name, string text, float posY)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform rectTransform = textObj.AddComponent<RectTransform>();

            // Anchor: Top Left
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            // Position and Size
            rectTransform.anchoredPosition = new Vector2(20, posY);
            rectTransform.sizeDelta = new Vector2(260, 30);

            // TextMeshProUGUI 컴포넌트 추가
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;

            Debug.Log($"[StatPanelCreator] {name} 생성 완료");
            return textObj;
        }


        // ====== StatPanelUI 스크립트 참조 연결 ======

        /// <summary>
        /// StatPanelUI 컴포넌트의 참조 연결
        /// </summary>
        private static void AssignReferences(StatPanelUI statPanelUI, GameObject hpText, GameObject attackText, GameObject defenseText)
        {
            // Reflection을 사용하여 private 필드에 접근
            SerializedObject serializedObject = new SerializedObject(statPanelUI);

            SerializedProperty hpTextProp = serializedObject.FindProperty("hpText");
            SerializedProperty attackTextProp = serializedObject.FindProperty("attackText");
            SerializedProperty defenseTextProp = serializedObject.FindProperty("defenseText");

            if (hpTextProp != null && attackTextProp != null && defenseTextProp != null)
            {
                hpTextProp.objectReferenceValue = hpText.GetComponent<TextMeshProUGUI>();
                attackTextProp.objectReferenceValue = attackText.GetComponent<TextMeshProUGUI>();
                defenseTextProp.objectReferenceValue = defenseText.GetComponent<TextMeshProUGUI>();

                serializedObject.ApplyModifiedProperties();

                Debug.Log("[StatPanelCreator] StatPanelUI 참조 연결 완료");
            }
            else
            {
                Debug.LogError("[StatPanelCreator] StatPanelUI 필드를 찾을 수 없습니다. 필드 이름을 확인하세요.");
            }
        }


    }
}
