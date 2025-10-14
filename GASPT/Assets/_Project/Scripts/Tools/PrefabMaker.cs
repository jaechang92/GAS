using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tools
{
    /// <summary>
    /// Panel Prefab을 자동으로 생성하는 유틸리티
    /// ContextMenu를 사용하여 Inspector에서 실행 가능
    /// </summary>
    public class PrefabMaker : MonoBehaviour
    {
        [Header("Prefab 저장 경로")]
        [SerializeField] private string prefabSavePath = "Assets/_Project/Resources/UI/Panels/";

        [Header("Canvas 설정")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);

#if UNITY_EDITOR
        /// <summary>
        /// 모든 Panel Prefab 생성
        /// </summary>
        [ContextMenu("Create All Panel Prefabs")]
        private void CreateAllPanelPrefabs()
        {
            Debug.Log("[PrefabMaker] 모든 Panel Prefab 생성 시작...");

            CreateMainMenuPanelPrefab();
            CreateLoadingPanelPrefab();
            CreateGameplayHUDPanelPrefab();
            CreatePausePanelPrefab();

            Debug.Log("[PrefabMaker] 모든 Panel Prefab 생성 완료!");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #region MainMenuPanel

        /// <summary>
        /// MainMenuPanel Prefab 생성
        /// </summary>
        [ContextMenu("Create MainMenuPanel Prefab")]
        private void CreateMainMenuPanelPrefab()
        {
            Debug.Log("[PrefabMaker] MainMenuPanel Prefab 생성 중...");

            // Panel Root 생성
            GameObject panelObj = CreatePanelRoot("MainMenuPanel");
            var panel = panelObj.AddComponent<UI.Panels.MainMenuPanel>();

            // 제목 텍스트
            GameObject titleObj = CreateText(panelObj.transform, "TitleText", "GASPT", 72);
            SetRectTransform(titleObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -150), new Vector2(400, 100));

            // 시작 버튼
            GameObject startButtonObj = CreateButton(panelObj.transform, "StartButton", "게임 시작", 24);
            SetRectTransform(startButtonObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(200, 60));

            // 설정 버튼
            GameObject settingsButtonObj = CreateButton(panelObj.transform, "SettingsButton", "설정", 24);
            SetRectTransform(settingsButtonObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -80), new Vector2(200, 60));

            // 종료 버튼
            GameObject quitButtonObj = CreateButton(panelObj.transform, "QuitButton", "종료", 24);
            SetRectTransform(quitButtonObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -160), new Vector2(200, 60));

            // 스크립트 필드 연결
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("startButton").objectReferenceValue = startButtonObj.GetComponent<Button>();
            so.FindProperty("settingsButton").objectReferenceValue = settingsButtonObj.GetComponent<Button>();
            so.FindProperty("quitButton").objectReferenceValue = quitButtonObj.GetComponent<Button>();
            so.FindProperty("titleText").objectReferenceValue = titleObj.GetComponent<Text>();
            so.ApplyModifiedProperties();

            // Prefab 저장
            SavePrefab(panelObj, "MainMenuPanel");

            Debug.Log("[PrefabMaker] MainMenuPanel Prefab 생성 완료!");
        }

        #endregion

        #region LoadingPanel

        /// <summary>
        /// LoadingPanel Prefab 생성
        /// </summary>
        [ContextMenu("Create LoadingPanel Prefab")]
        private void CreateLoadingPanelPrefab()
        {
            Debug.Log("[PrefabMaker] LoadingPanel Prefab 생성 중...");

            // Panel Root 생성
            GameObject panelObj = CreatePanelRoot("LoadingPanel");
            var panel = panelObj.AddComponent<UI.Panels.LoadingPanel>();

            // 배경 패널
            GameObject bgObj = CreateImage(panelObj.transform, "BackgroundPanel", new Color(0.1f, 0.1f, 0.1f, 1f));
            SetRectTransformStretch(bgObj);

            // 로딩 텍스트
            GameObject loadingTextObj = CreateText(panelObj.transform, "LoadingText", "Loading...", 48);
            SetRectTransform(loadingTextObj, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 80));

            // 진행률 바
            GameObject progressBarObj = CreateSlider(panelObj.transform, "ProgressBar");
            SetRectTransform(progressBarObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(600, 30));

            // 진행률 텍스트
            GameObject progressTextObj = CreateText(panelObj.transform, "ProgressText", "0%", 24);
            SetRectTransform(progressTextObj, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(200, 40));

            // 팁 텍스트
            GameObject tipTextObj = CreateText(panelObj.transform, "LoadingTipText", "TIP: 게임을 시작합니다...", 18);
            tipTextObj.GetComponent<Text>().color = new Color(0.8f, 0.8f, 0.8f, 1f);
            SetRectTransform(tipTextObj, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(600, 60));

            // 스크립트 필드 연결
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("progressBar").objectReferenceValue = progressBarObj.GetComponent<Slider>();
            so.FindProperty("progressText").objectReferenceValue = progressTextObj.GetComponent<Text>();
            so.FindProperty("loadingTipText").objectReferenceValue = tipTextObj.GetComponent<Text>();
            so.FindProperty("loadingText").objectReferenceValue = loadingTextObj.GetComponent<Text>();
            so.ApplyModifiedProperties();

            // Prefab 저장
            SavePrefab(panelObj, "LoadingPanel");

            Debug.Log("[PrefabMaker] LoadingPanel Prefab 생성 완료!");
        }

        #endregion

        #region GameplayHUDPanel

        /// <summary>
        /// GameplayHUDPanel Prefab 생성
        /// </summary>
        [ContextMenu("Create GameplayHUDPanel Prefab")]
        private void CreateGameplayHUDPanelPrefab()
        {
            Debug.Log("[PrefabMaker] GameplayHUDPanel Prefab 생성 중...");

            // Panel Root 생성
            GameObject panelObj = CreatePanelRoot("GameplayHUDPanel");
            var panel = panelObj.AddComponent<UI.Panels.GameplayHUDPanel>();

            // 체력바 (빈 GameObject - 나중에 HealthBarUI 추가)
            GameObject healthBarObj = new GameObject("HealthBar");
            healthBarObj.transform.SetParent(panelObj.transform, false);
            var healthBarRect = healthBarObj.AddComponent<RectTransform>();
            SetRectTransform(healthBarObj, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(20, -20), new Vector2(300, 40));

            // 콤보 텍스트
            GameObject comboTextObj = CreateText(panelObj.transform, "ComboText", "5 COMBO!", 48);
            var comboText = comboTextObj.GetComponent<Text>();
            comboText.color = new Color(1f, 0.8f, 0f, 1f);
            comboText.fontStyle = FontStyle.Bold;
            SetRectTransform(comboTextObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -80), new Vector2(400, 80));
            comboTextObj.SetActive(false);

            // 적 카운트 텍스트
            GameObject enemyCountTextObj = CreateText(panelObj.transform, "EnemyCountText", "적: 0", 24);
            SetRectTransform(enemyCountTextObj, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-20, -80), new Vector2(200, 40));

            // 점수 텍스트
            GameObject scoreTextObj = CreateText(panelObj.transform, "ScoreText", "점수: 0", 24);
            SetRectTransform(scoreTextObj, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
                new Vector2(20, 20), new Vector2(200, 40));

            // 일시정지 버튼
            GameObject pauseButtonObj = CreateButton(panelObj.transform, "PauseButton", "II", 24);
            SetRectTransform(pauseButtonObj, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-20, -20), new Vector2(80, 40));

            // 스크립트 필드 연결
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("healthBar").objectReferenceValue = healthBarObj.GetComponent<MonoBehaviour>();
            so.FindProperty("comboText").objectReferenceValue = comboText;
            so.FindProperty("enemyCountText").objectReferenceValue = enemyCountTextObj.GetComponent<Text>();
            so.FindProperty("scoreText").objectReferenceValue = scoreTextObj.GetComponent<Text>();
            so.FindProperty("pauseButton").objectReferenceValue = pauseButtonObj.GetComponent<Button>();
            so.ApplyModifiedProperties();

            // Prefab 저장
            SavePrefab(panelObj, "GameplayHUDPanel");

            Debug.Log("[PrefabMaker] GameplayHUDPanel Prefab 생성 완료!");
        }

        #endregion

        #region PausePanel

        /// <summary>
        /// PausePanel Prefab 생성
        /// </summary>
        [ContextMenu("Create PausePanel Prefab")]
        private void CreatePausePanelPrefab()
        {
            Debug.Log("[PrefabMaker] PausePanel Prefab 생성 중...");

            // Panel Root 생성
            GameObject panelObj = CreatePanelRoot("PausePanel");
            var panel = panelObj.AddComponent<UI.Panels.PausePanel>();

            // 어두운 배경
            GameObject dimmedBgObj = CreateImage(panelObj.transform, "DimmedBackground", new Color(0, 0, 0, 0.7f));
            SetRectTransformStretch(dimmedBgObj);

            // 팝업 패널
            GameObject popupPanelObj = CreateImage(panelObj.transform, "PopupPanel", new Color(0.2f, 0.2f, 0.2f, 0.95f));
            SetRectTransform(popupPanelObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 500));

            // 제목 텍스트
            GameObject titleObj = CreateText(popupPanelObj.transform, "TitleText", "일시정지", 48);
            SetRectTransform(titleObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -50), new Vector2(300, 80));

            // 재개 버튼
            GameObject resumeButtonObj = CreateButton(popupPanelObj.transform, "ResumeButton", "재개", 24);
            SetRectTransform(resumeButtonObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 50), new Vector2(250, 60));

            // 설정 버튼
            GameObject settingsButtonObj = CreateButton(popupPanelObj.transform, "SettingsButton", "설정", 24);
            SetRectTransform(settingsButtonObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -30), new Vector2(250, 60));

            // 메인 메뉴 버튼
            GameObject mainMenuButtonObj = CreateButton(popupPanelObj.transform, "MainMenuButton", "메인 메뉴", 24);
            SetRectTransform(mainMenuButtonObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -110), new Vector2(250, 60));

            // 스크립트 필드 연결
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("resumeButton").objectReferenceValue = resumeButtonObj.GetComponent<Button>();
            so.FindProperty("settingsButton").objectReferenceValue = settingsButtonObj.GetComponent<Button>();
            so.FindProperty("mainMenuButton").objectReferenceValue = mainMenuButtonObj.GetComponent<Button>();
            so.FindProperty("titleText").objectReferenceValue = titleObj.GetComponent<Text>();
            so.ApplyModifiedProperties();

            // Prefab 저장
            SavePrefab(panelObj, "PausePanel");

            Debug.Log("[PrefabMaker] PausePanel Prefab 생성 완료!");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Panel Root GameObject 생성 (Canvas 없이 RectTransform만)
        /// BasePanel이 자동으로 Canvas를 추가하므로 여기서는 생성하지 않음
        /// </summary>
        private GameObject CreatePanelRoot(string name)
        {
            GameObject panelObj = new GameObject(name);

            // RectTransform 추가 (Canvas 없음)
            RectTransform rectTransform = panelObj.AddComponent<RectTransform>();

            // 전체 화면으로 설정
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            return panelObj;
        }

        /// <summary>
        /// Text UI 생성
        /// </summary>
        private GameObject CreateText(Transform parent, string name, string text, int fontSize)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            Text textComp = textObj.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;

            // Maplestory Light 폰트 사용 (Maplestory Light)
            //textComp.font = Resources.GetBuiltinResource<Font>("Maplestory Light SDF");

            return textObj;
        }

        /// <summary>
        /// Button UI 생성
        /// </summary>
        private GameObject CreateButton(Transform parent, string name, string text, int fontSize)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            // Button 이미지 배경
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Button 컴포넌트
            Button button = buttonObj.AddComponent<Button>();

            // Button 텍스트
            GameObject textObj = CreateText(buttonObj.transform, "Text", text, fontSize);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return buttonObj;
        }

        /// <summary>
        /// Image UI 생성
        /// </summary>
        private GameObject CreateImage(Transform parent, string name, Color color)
        {
            GameObject imageObj = new GameObject(name);
            imageObj.transform.SetParent(parent, false);

            Image image = imageObj.AddComponent<Image>();
            image.color = color;

            return imageObj;
        }

        /// <summary>
        /// Slider UI 생성
        /// </summary>
        private GameObject CreateSlider(Transform parent, string name)
        {
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);

            // Slider 컴포넌트
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;

            // Background
            GameObject bgObj = CreateImage(sliderObj.transform, "Background", new Color(0.2f, 0.2f, 0.2f, 1f));
            SetRectTransformStretch(bgObj);

            // Fill Area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            SetRectTransformStretch(fillAreaObj);

            // Fill
            GameObject fillObj = CreateImage(fillAreaObj.transform, "Fill", new Color(0.2f, 0.8f, 0.2f, 1f));
            SetRectTransformStretch(fillObj);

            // Slider 연결
            slider.fillRect = fillObj.GetComponent<RectTransform>();

            return sliderObj;
        }

        /// <summary>
        /// RectTransform 설정
        /// </summary>
        private void SetRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect == null)
                rect = obj.AddComponent<RectTransform>();

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        /// <summary>
        /// RectTransform 전체 화면으로 설정 (Stretch)
        /// </summary>
        private void SetRectTransformStretch(GameObject obj)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect == null)
                rect = obj.AddComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Prefab 저장
        /// </summary>
        private void SavePrefab(GameObject obj, string prefabName)
        {
            // 저장 폴더 확인
            if (!AssetDatabase.IsValidFolder(prefabSavePath))
            {
                Debug.LogError($"[PrefabMaker] 저장 경로가 존재하지 않습니다: {prefabSavePath}");
                DestroyImmediate(obj);
                return;
            }

            string prefabPath = $"{prefabSavePath}{prefabName}.prefab";

            // 기존 Prefab이 있으면 덮어쓰기
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.LogWarning($"[PrefabMaker] 기존 Prefab을 덮어씁니다: {prefabPath}");
            }

            // Prefab 저장
            PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);

            // Hierarchy에서 삭제
            DestroyImmediate(obj);

            Debug.Log($"[PrefabMaker] Prefab 저장 완료: {prefabPath}");
        }

        #endregion
#endif
    }
}
