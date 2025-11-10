using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using GASPT.UI;

namespace GASPT.Editor
{
    /// <summary>
    /// BuffIcon UI 자동 생성 에디터 도구
    /// Menu: Tools > GASPT > Create Buff Icon UI
    /// </summary>
    public class BuffIconCreator
    {
        [MenuItem("Tools/GASPT/UI/Create Buff Icon UI", false, 107)]
        public static void CreateBuffIconUI()
        {
            Debug.Log("========== Buff Icon UI 생성 시작 ==========");

            // Canvas 찾기 또는 생성
            Canvas canvas = FindOrCreateCanvas();

            // BuffIconPanel 생성
            GameObject panelObj = CreateBuffIconPanel(canvas);

            // BuffIcon 프리팹 생성
            GameObject iconPrefabObj = CreateBuffIconPrefab();

            // BuffIconPanel에 프리팹 연결
            ConnectPrefabToPanel(panelObj, iconPrefabObj);

            Debug.Log("========== Buff Icon UI 생성 완료 ==========");
            EditorUtility.DisplayDialog("완료", "Buff Icon UI가 생성되었습니다!\n\n1. BuffIconPanel이 Canvas에 추가됨\n2. BuffIcon 프리팹이 Resources/Prefabs/UI에 생성됨\n3. targetObject를 Player로 설정하세요", "확인");
        }

        [MenuItem("Tools/GASPT/UI/Delete Buff Icon UI", false, 108)]
        public static void DeleteBuffIconUI()
        {
            GameObject panel = GameObject.Find("BuffIconPanel");
            if (panel != null)
            {
                Object.DestroyImmediate(panel);
                Debug.Log("[BuffIconCreator] BuffIconPanel 삭제 완료");
            }

            // 프리팹 삭제 (선택사항)
            string prefabPath = "Assets/Resources/Prefabs/UI/BuffIcon.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
                Debug.Log("[BuffIconCreator] BuffIcon 프리팹 삭제 완료");
            }

            EditorUtility.DisplayDialog("완료", "Buff Icon UI가 삭제되었습니다.", "확인");
        }


        // ====== Canvas 생성/찾기 ======

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

                Debug.Log("[BuffIconCreator] 새 Canvas 생성");
            }
            else
            {
                Debug.Log("[BuffIconCreator] 기존 Canvas 사용");
            }

            return canvas;
        }


        // ====== BuffIconPanel 생성 ======

        private static GameObject CreateBuffIconPanel(Canvas canvas)
        {
            // 기존 BuffIconPanel 확인
            GameObject existingPanel = GameObject.Find("BuffIconPanel");
            if (existingPanel != null)
            {
                Debug.LogWarning("[BuffIconCreator] BuffIconPanel이 이미 존재합니다. 덮어씁니다.");
                Object.DestroyImmediate(existingPanel);
            }

            // Panel 생성
            GameObject panelObj = new GameObject("BuffIconPanel");
            panelObj.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f); // 왼쪽 상단
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(20f, -20f); // 좌상단에서 약간 떨어짐
            panelRect.sizeDelta = new Vector2(400f, 80f); // 가로 400, 세로 80

            // HorizontalLayoutGroup 추가
            HorizontalLayoutGroup layoutGroup = panelObj.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.spacing = 5f;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            // BuffIconPanel 컴포넌트 추가
            BuffIconPanel panel = panelObj.AddComponent<BuffIconPanel>();

            // IconContainer 설정 (자기 자신)
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("iconContainer").objectReferenceValue = panelRect;
            so.FindProperty("maxIcons").intValue = 10;
            so.ApplyModifiedProperties();

            Debug.Log("[BuffIconCreator] BuffIconPanel 생성 완료");
            return panelObj;
        }


        // ====== BuffIcon 프리팹 생성 ======

        private static GameObject CreateBuffIconPrefab()
        {
            // 프리팹 폴더 확인
            string folderPath = "Assets/Resources/Prefabs/UI";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = "Assets/Resources/Prefabs";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    string resourcesFolder = "Assets/Resources";
                    if (!AssetDatabase.IsValidFolder(resourcesFolder))
                    {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    }
                    AssetDatabase.CreateFolder(resourcesFolder, "Prefabs");
                }
                AssetDatabase.CreateFolder(parentFolder, "UI");
            }

            // BuffIcon GameObject 생성
            GameObject iconObj = new GameObject("BuffIcon");
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(64f, 64f); // 64x64 아이콘

            // 1. Background (어두운 배경)
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(iconObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // 2. IconImage
            GameObject iconImgObj = new GameObject("IconImage");
            iconImgObj.transform.SetParent(iconObj.transform, false);
            RectTransform iconImgRect = iconImgObj.AddComponent<RectTransform>();
            iconImgRect.anchorMin = Vector2.zero;
            iconImgRect.anchorMax = Vector2.one;
            iconImgRect.sizeDelta = new Vector2(-8f, -8f); // 4px 여백
            Image iconImage = iconImgObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // 3. TimerFillImage (Radial360)
            GameObject timerObj = new GameObject("TimerFillImage");
            timerObj.transform.SetParent(iconObj.transform, false);
            RectTransform timerRect = timerObj.AddComponent<RectTransform>();
            timerRect.anchorMin = Vector2.zero;
            timerRect.anchorMax = Vector2.one;
            timerRect.sizeDelta = Vector2.zero;
            Image timerImage = timerObj.AddComponent<Image>();
            timerImage.type = Image.Type.Filled;
            timerImage.fillMethod = Image.FillMethod.Radial360;
            timerImage.fillOrigin = (int)Image.Origin360.Top;
            timerImage.fillClockwise = false;
            timerImage.fillAmount = 1f;
            timerImage.color = new Color(0f, 0f, 0f, 0.6f); // 어두운 오버레이

            // 4. BorderImage (테두리)
            GameObject borderObj = new GameObject("BorderImage");
            borderObj.transform.SetParent(iconObj.transform, false);
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = Vector2.zero;
            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.color = Color.green; // 기본 버프 색상

            // 5. StackText (우상단)
            GameObject stackTextObj = new GameObject("StackText");
            stackTextObj.transform.SetParent(iconObj.transform, false);
            RectTransform stackTextRect = stackTextObj.AddComponent<RectTransform>();
            stackTextRect.anchorMin = new Vector2(1f, 1f);
            stackTextRect.anchorMax = new Vector2(1f, 1f);
            stackTextRect.pivot = new Vector2(1f, 1f);
            stackTextRect.anchoredPosition = new Vector2(-2f, -2f);
            stackTextRect.sizeDelta = new Vector2(20f, 20f);
            TextMeshProUGUI stackText = stackTextObj.AddComponent<TextMeshProUGUI>();
            stackText.text = "2";
            stackText.fontSize = 14;
            stackText.fontStyle = FontStyles.Bold;
            stackText.color = Color.white;
            stackText.alignment = TextAlignmentOptions.BottomRight;

            // 6. TimeText (중앙 하단)
            GameObject timeTextObj = new GameObject("TimeText");
            timeTextObj.transform.SetParent(iconObj.transform, false);
            RectTransform timeTextRect = timeTextObj.AddComponent<RectTransform>();
            timeTextRect.anchorMin = new Vector2(0f, 0f);
            timeTextRect.anchorMax = new Vector2(1f, 0f);
            timeTextRect.pivot = new Vector2(0.5f, 0f);
            timeTextRect.anchoredPosition = Vector2.zero;
            timeTextRect.sizeDelta = new Vector2(0f, 18f);
            TextMeshProUGUI timeText = timeTextObj.AddComponent<TextMeshProUGUI>();
            timeText.text = "5.2s";
            timeText.fontSize = 12;
            timeText.fontStyle = FontStyles.Bold;
            timeText.color = Color.white;
            timeText.alignment = TextAlignmentOptions.Center;

            // BuffIcon 컴포넌트 추가 및 연결
            BuffIcon buffIcon = iconObj.AddComponent<BuffIcon>();
            SerializedObject iconSO = new SerializedObject(buffIcon);
            iconSO.FindProperty("iconImage").objectReferenceValue = iconImage;
            iconSO.FindProperty("timerFillImage").objectReferenceValue = timerImage;
            iconSO.FindProperty("stackText").objectReferenceValue = stackText;
            iconSO.FindProperty("timeText").objectReferenceValue = timeText;
            iconSO.FindProperty("borderImage").objectReferenceValue = borderImage;
            iconSO.ApplyModifiedProperties();

            // 프리팹으로 저장
            string prefabPath = folderPath + "/BuffIcon.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(iconObj, prefabPath);
            Object.DestroyImmediate(iconObj); // 씬에서 제거

            Debug.Log($"[BuffIconCreator] BuffIcon 프리팹 생성: {prefabPath}");
            return prefab;
        }


        // ====== 프리팹과 Panel 연결 ======

        private static void ConnectPrefabToPanel(GameObject panelObj, GameObject iconPrefab)
        {
            BuffIconPanel panel = panelObj.GetComponent<BuffIconPanel>();
            if (panel != null)
            {
                SerializedObject so = new SerializedObject(panel);
                so.FindProperty("buffIconPrefab").objectReferenceValue = iconPrefab;
                so.ApplyModifiedProperties();

                Debug.Log("[BuffIconCreator] BuffIconPanel에 프리팹 연결 완료");
            }
        }
    }
}
