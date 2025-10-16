using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UI.Panels;
using UI.Core;

namespace Editor.Tools
{
    /// <summary>
    /// DialoguePanel Prefab을 프로그래매틱하게 생성하는 Editor 도구
    /// Menu: Tools/Dialogue/Create DialoguePanel Prefab
    /// </summary>
    public static class DialoguePanelPrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Resources/UI/Panels/DialogPanel.prefab";
        private const string FOLDER_PATH = "Assets/_Project/Resources/UI/Panels";
        private const string CHOICE_BUTTON_PATH = "UI/Prefabs/ChoiceButton";

        [MenuItem("Tools/Dialogue/Create DialoguePanel Prefab")]
        public static void CreateDialoguePanelPrefab()
        {
            // 폴더 생성
            CreateFoldersIfNeeded();

            // GameObject 생성
            GameObject dialogPanelObj = CreateDialoguePanelHierarchy();

            // Prefab으로 저장
            SaveAsPrefab(dialogPanelObj);

            Debug.Log($"[DialoguePanelPrefabGenerator] DialoguePanel Prefab 생성 완료: {PREFAB_PATH}");
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
                    Debug.Log($"[DialoguePanelPrefabGenerator] 폴더 생성: {nextPath}");
                }
                currentPath = nextPath;
            }
        }

        private static GameObject CreateDialoguePanelHierarchy()
        {
            // Root: DialogPanel
            GameObject root = new GameObject("DialogPanel");

            // RectTransform 설정
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;
            rootRect.anchoredPosition = Vector2.zero;

            // Background (반투명 배경)
            GameObject background = CreateBackground(root.transform);

            // DialogueBox (대화 창)
            GameObject dialogueBox = CreateDialogueBox(root.transform);

            // DialoguePanel Script 추가 및 참조 설정
            DialoguePanel dialoguePanel = root.AddComponent<DialoguePanel>();
            SetDialoguePanelReferences(dialoguePanel, dialogueBox);

            return root;
        }

        private static GameObject CreateBackground(Transform parent)
        {
            GameObject background = new GameObject("Background");
            background.transform.SetParent(parent, false);

            RectTransform rect = background.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            Image image = background.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.5f); // 반투명 검정

            return background;
        }

        private static GameObject CreateDialogueBox(Transform parent)
        {
            GameObject dialogueBox = new GameObject("DialogueBox");
            dialogueBox.transform.SetParent(parent, false);

            RectTransform rect = dialogueBox.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0);
            rect.anchorMax = new Vector2(0.9f, 0.4f);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = new Vector2(0, 50);

            Image image = dialogueBox.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 0.95f); // 어두운 회색

            // 자식 요소 생성
            GameObject speakerPanel = CreateSpeakerNamePanel(dialogueBox.transform);
            GameObject textArea = CreateDialogueTextArea(dialogueBox.transform);
            GameObject buttonArea = CreateButtonArea(dialogueBox.transform);

            return dialogueBox;
        }

        private static GameObject CreateSpeakerNamePanel(Transform parent)
        {
            GameObject panel = new GameObject("SpeakerNamePanel");
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -10);
            rect.sizeDelta = new Vector2(300, 60);

            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.25f, 0.25f, 0.3f, 1f);

            // 자식: SpeakerNameText
            GameObject textObj = new GameObject("SpeakerNameText");
            textObj.transform.SetParent(panel.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-20, -10);
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "화자";
            text.fontSize = 28;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.margin = new Vector4(10, 5, 10, 5);

            return panel;
        }

        private static GameObject CreateDialogueTextArea(Transform parent)
        {
            GameObject textArea = new GameObject("DialogueTextArea");
            textArea.transform.SetParent(parent, false);

            RectTransform rect = textArea.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.2f);
            rect.anchorMax = new Vector2(1, 0.85f);
            rect.sizeDelta = new Vector2(-40, 0);
            rect.anchoredPosition = Vector2.zero;

            // 자식: DialogueText
            GameObject textObj = new GameObject("DialogueText");
            textObj.transform.SetParent(textArea.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "대화 텍스트가 여기에 표시됩니다.";
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;
            text.margin = new Vector4(20, 10, 20, 10);

            return textArea;
        }

        private static GameObject CreateButtonArea(Transform parent)
        {
            GameObject buttonArea = new GameObject("ButtonArea");
            buttonArea.transform.SetParent(parent, false);

            RectTransform rect = buttonArea.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0.2f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            // 자식: ContinueButton
            GameObject continueButton = CreateContinueButton(buttonArea.transform);

            // 자식: ChoiceButtonContainer
            GameObject choiceContainer = CreateChoiceButtonContainer(buttonArea.transform);

            return buttonArea;
        }

        private static GameObject CreateContinueButton(Transform parent)
        {
            GameObject buttonObj = new GameObject("ContinueButton");
            buttonObj.transform.SetParent(parent, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 0.5f);
            rect.anchoredPosition = new Vector2(-20, 0);
            rect.sizeDelta = new Vector2(150, 50);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.35f, 1f);

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            // ColorBlock 설정
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.3f, 0.3f, 0.35f, 1f);
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.45f, 1f);
            colors.pressedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            colors.selectedColor = new Color(0.35f, 0.35f, 0.4f, 1f);
            button.colors = colors;

            // 자식: Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "계속 ▶";
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            return buttonObj;
        }

        private static GameObject CreateChoiceButtonContainer(Transform parent)
        {
            GameObject container = new GameObject("ChoiceButtonContainer");
            container.transform.SetParent(parent, false);

            RectTransform rect = container.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(-200, -20);
            rect.anchoredPosition = Vector2.zero;

            // Vertical Layout Group
            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(10, 10, 10, 10);

            // Content Size Fitter
            ContentSizeFitter fitter = container.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return container;
        }

        private static void SetDialoguePanelReferences(DialoguePanel panel, GameObject dialogueBox)
        {
            // SerializedObject를 통해 private 필드 설정
            SerializedObject serializedPanel = new SerializedObject(panel);

            // speakerNameText 참조 설정
            TextMeshProUGUI speakerNameText = dialogueBox.transform.Find("SpeakerNamePanel/SpeakerNameText").GetComponent<TextMeshProUGUI>();
            serializedPanel.FindProperty("speakerNameText").objectReferenceValue = speakerNameText;

            // dialogueText 참조 설정
            TextMeshProUGUI dialogueText = dialogueBox.transform.Find("DialogueTextArea/DialogueText").GetComponent<TextMeshProUGUI>();
            serializedPanel.FindProperty("dialogueText").objectReferenceValue = dialogueText;

            // continueButton 참조 설정
            Button continueButton = dialogueBox.transform.Find("ButtonArea/ContinueButton").GetComponent<Button>();
            serializedPanel.FindProperty("continueButton").objectReferenceValue = continueButton;

            // choiceButtonContainer 참조 설정
            Transform choiceContainer = dialogueBox.transform.Find("ButtonArea/ChoiceButtonContainer");
            serializedPanel.FindProperty("choiceButtonContainer").objectReferenceValue = choiceContainer;

            // choiceButtonPrefab 참조 설정 (Resources에서 로드)
            Button choiceButtonPrefab = Resources.Load<Button>(CHOICE_BUTTON_PATH);
            if (choiceButtonPrefab != null)
            {
                serializedPanel.FindProperty("choiceButtonPrefab").objectReferenceValue = choiceButtonPrefab;
                Debug.Log($"[DialoguePanelPrefabGenerator] ChoiceButton Prefab 로드 성공: {CHOICE_BUTTON_PATH}");
            }
            else
            {
                Debug.LogWarning($"[DialoguePanelPrefabGenerator] ChoiceButton Prefab을 찾을 수 없습니다: {CHOICE_BUTTON_PATH}");
                Debug.LogWarning("[DialoguePanelPrefabGenerator] Tools/Dialogue/Create ChoiceButton Prefab를 먼저 실행하세요.");
            }

            // 타이핑 효과 설정
            serializedPanel.FindProperty("useTypingEffect").boolValue = true;
            serializedPanel.FindProperty("typingSpeed").floatValue = 0.05f;
            serializedPanel.FindProperty("showDebugLog").boolValue = true;

            // BasePanel 설정 (PanelType, UILayer)
            // enumValueIndex는 선언 순서 인덱스를 사용해야 함
            SerializedProperty panelTypeProperty = serializedPanel.FindProperty("panelType");
            if (panelTypeProperty != null)
            {
                // PanelType.Dialog는 enum에서 9번째 (인덱스 9)
                panelTypeProperty.enumValueIndex = System.Array.IndexOf(System.Enum.GetValues(typeof(PanelType)), PanelType.Dialog);
            }

            SerializedProperty layerProperty = serializedPanel.FindProperty("layer");
            if (layerProperty != null)
            {
                // UILayer.Popup는 enum에서 3번째 (인덱스 2: Background=0, Normal=1, Popup=2)
                layerProperty.enumValueIndex = System.Array.IndexOf(System.Enum.GetValues(typeof(UILayer)), UILayer.Popup);
            }

            serializedPanel.ApplyModifiedProperties();
        }

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
    }
}
