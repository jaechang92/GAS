using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

namespace Editor.Tools
{
    /// <summary>
    /// ChoiceButton Prefab을 프로그래매틱하게 생성하는 Editor 도구
    /// Menu: Tools/Dialogue/Create ChoiceButton Prefab
    /// </summary>
    public static class ChoiceButtonPrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Resources/UI/Prefabs/ChoiceButton.prefab";
        private const string FOLDER_PATH = "Assets/_Project/Resources/UI/Prefabs";

        [MenuItem("Tools/Dialogue/Create ChoiceButton Prefab")]
        public static void CreateChoiceButtonPrefab()
        {
            // 폴더 생성
            CreateFoldersIfNeeded();

            // GameObject 생성
            GameObject choiceButtonObj = CreateChoiceButtonHierarchy();

            // Prefab으로 저장
            SaveAsPrefab(choiceButtonObj);

            Debug.Log($"[ChoiceButtonPrefabGenerator] ChoiceButton Prefab 생성 완료: {PREFAB_PATH}");
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
                    Debug.Log($"[ChoiceButtonPrefabGenerator] 폴더 생성: {nextPath}");
                }
                currentPath = nextPath;
            }
        }

        private static GameObject CreateChoiceButtonHierarchy()
        {
            // Root: ChoiceButton
            GameObject root = new GameObject("ChoiceButton");

            // RectTransform 설정
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0, 0);
            rootRect.anchorMax = new Vector2(1, 1);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(0, 60);

            // Image Component (버튼 배경)
            Image buttonImage = root.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f); // 어두운 회색

            // Button Component
            Button button = root.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // ColorBlock 설정 (호버, 클릭 효과)
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            colors.selectedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            button.colors = colors;

            // 자식: Text (TextMeshProUGUI)
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(root.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "선택지 텍스트";
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = true;

            return root;
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
