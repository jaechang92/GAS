using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using TMPro;
using GASPT.UI.MVP;
using GASPT.UI;
using GASPT.ResourceManagement;
using GASPT.CameraSystem;
using GASPT.Core.SceneManagement;
using Unity.Cinemachine;

namespace GASPT.Editor
{
    /// <summary>
    /// PersistentManagers Scene 자동 생성 에디터 툴
    ///
    /// PersistentManagers Scene에 포함되는 요소:
    /// - EventSystem
    /// - Main Canvas (모든 UI의 부모)
    ///   - HUD (HP, Mana, Gold)
    ///   - InventoryUI (MVP)
    ///   - ShopUI (MVP)
    ///   - AchievementUI (예정)
    ///   - PlayerInfoUI (예정)
    ///   - BuffIconPanel
    ///   - LoadingUI
    /// - UIManager
    /// - 오디오 매니저 (예정)
    ///
    /// ※ 이 씬의 모든 오브젝트는 DontDestroyOnLoad로 영구 유지됨
    /// </summary>
    public class PersistentManagersSceneCreator : EditorWindow
    {
        private const string ScenePath = "Assets/_Project/Scenes/PersistentManagers.unity";

        private Vector2 scrollPosition;

        // 생성 옵션
        private bool createMainCamera = true;
        private bool createEventSystem = true;
        private bool createHUD = true;
        private bool createInventoryUI = true;
        private bool createShopUI = true;
        private bool createBuffIconPanel = true;
        private bool createLoadingUI = true;

        [MenuItem("Tools/GASPT/🔧 Create PersistentManagers Scene")]
        public static void ShowWindow()
        {
            var window = GetWindow<PersistentManagersSceneCreator>("PersistentManagers Scene Creator");
            window.minSize = new Vector2(450, 550);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("=== PersistentManagers Scene Creator ===", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "PersistentManagers Scene은 모든 공통 요소를 포함합니다.\n\n" +
                "특징:\n" +
                "✓ Additive로 로드되어 항상 유지\n" +
                "✓ 씬 전환 시에도 파괴되지 않음\n" +
                "✓ 모든 Content Scene에서 공유\n\n" +
                "포함 요소:\n" +
                "- Main Camera (공유 카메라)\n" +
                "- EventSystem (UI 입력)\n" +
                "- Main Canvas (전체 UI 컨테이너)\n" +
                "- HUD (HP, Mana, Gold)\n" +
                "- InventoryUI, ShopUI (MVP 패턴)\n" +
                "- BuffIconPanel, LoadingUI\n" +
                "- CameraManager + CameraEffects (Post-Processing)",
                MessageType.Info
            );

            GUILayout.Space(10);

            // 생성 옵션
            EditorGUILayout.LabelField("생성 옵션:", EditorStyles.boldLabel);
            createMainCamera = EditorGUILayout.Toggle("Main Camera + CameraManager", createMainCamera);
            createEventSystem = EditorGUILayout.Toggle("EventSystem", createEventSystem);
            createHUD = EditorGUILayout.Toggle("HUD (HP, Mana, Gold)", createHUD);
            createInventoryUI = EditorGUILayout.Toggle("InventoryUI (MVP)", createInventoryUI);
            createShopUI = EditorGUILayout.Toggle("ShopUI (MVP)", createShopUI);
            createBuffIconPanel = EditorGUILayout.Toggle("BuffIconPanel", createBuffIconPanel);
            createLoadingUI = EditorGUILayout.Toggle("LoadingUI", createLoadingUI);

            GUILayout.Space(20);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🔧 PersistentManagers Scene 생성", GUILayout.Height(50)))
            {
                EditorApplication.delayCall += CreatePersistentManagersScene;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(20);

            EditorGUILayout.HelpBox(
                "⚠️ Build Settings 순서:\n" +
                "0: Bootstrap\n" +
                "1: PersistentManagers\n" +
                "2: StartRoom\n" +
                "3: GameplayScene",
                MessageType.Warning
            );

            EditorGUILayout.EndScrollView();
        }

        private void CreatePersistentManagersScene()
        {
            Debug.Log("=== PersistentManagers Scene 생성 시작 ===");

            // 새 씬 생성 (빈 씬)
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 씬 구성
            SetupPersistentManagersScene();

            // 폴더 확인/생성
            string scenesFolder = "Assets/_Project/Scenes";
            if (!AssetDatabase.IsValidFolder(scenesFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }

            // 씬 저장
            bool saved = EditorSceneManager.SaveScene(newScene, ScenePath);

            if (saved)
            {
                Debug.Log($"=== PersistentManagers Scene 생성 완료! ===\n위치: {ScenePath}");

                EditorUtility.DisplayDialog("생성 완료",
                    "PersistentManagers Scene이 생성되었습니다!\n\n" +
                    "이 씬의 모든 오브젝트는:\n" +
                    "- Additive로 로드됨\n" +
                    "- 씬 전환 시에도 유지됨\n" +
                    "- 모든 Content Scene에서 공유됨",
                    "확인");

                // 씬 선택
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            else
            {
                Debug.LogError("[PersistentManagersSceneCreator] 씬 저장 실패!");
            }
        }

        private void SetupPersistentManagersScene()
        {
            // PersistentManagers 루트 오브젝트 (DontDestroyOnLoad 마커)
            GameObject rootObj = new GameObject("=== PERSISTENT MANAGERS ===");
            var marker = rootObj.AddComponent<PersistentSceneMarker>();

            // Main Camera (공유 카메라)
            if (createMainCamera)
            {
                CreateMainCamera(rootObj.transform);
            }

            // EventSystem
            if (createEventSystem)
            {
                CreateEventSystem(rootObj.transform);
            }

            // Main Canvas
            GameObject canvas = CreateMainCanvas(rootObj.transform);

            // UIManager 홀더 생성
            CreateUIManagerHolder(rootObj.transform);

            // SceneValidationManager 생성
            CreateSceneValidationManager(rootObj.transform);

            // UI 요소들
            if (createHUD)
            {
                CreateHUD(canvas);
            }

            if (createInventoryUI)
            {
                CreateInventoryUI(canvas);
            }

            if (createShopUI)
            {
                CreateShopUI(canvas);
            }

            if (createBuffIconPanel)
            {
                CreateBuffIconPanel(canvas);
            }

            if (createLoadingUI)
            {
                CreateLoadingUI(canvas);
            }

            Debug.Log("[PersistentManagersSceneCreator] 씬 구성 완료");
        }

        /// <summary>
        /// Main Camera 생성 (모든 Content Scene에서 공유)
        /// Cinemachine Brain + Virtual Camera + CameraEffects 구성
        /// </summary>
        private void CreateMainCamera(Transform parent)
        {
            // === Main Camera + Cinemachine Brain ===
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.transform.SetParent(parent, false);
            cameraObj.tag = "MainCamera";

            UnityEngine.Camera camera = cameraObj.AddComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.nearClipPlane = -10f;
            camera.farClipPlane = 100f;

            // AudioListener 추가
            cameraObj.AddComponent<AudioListener>();

            // URP용 UniversalAdditionalCameraData 추가 (Post-Processing 활성화)
            var cameraData = cameraObj.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;

            // Cinemachine Brain 추가
            CinemachineBrain brain = cameraObj.AddComponent<CinemachineBrain>();
            brain.UpdateMethod = CinemachineBrain.UpdateMethods.SmartUpdate;
            brain.DefaultBlend = new CinemachineBlendDefinition(
                CinemachineBlendDefinition.Styles.EaseInOut, 0.5f);

            Debug.Log("[PersistentManagersSceneCreator] Main Camera + CinemachineBrain 생성 완료");

            // === Player Virtual Camera ===
            GameObject vcamObj = new GameObject("CM PlayerCamera");
            vcamObj.transform.SetParent(parent, false);
            vcamObj.transform.position = new Vector3(0, 0, -10f);

            CinemachineCamera virtualCamera = vcamObj.AddComponent<CinemachineCamera>();
            virtualCamera.Priority = 10;

            // Lens 설정 (Orthographic)
            var lens = virtualCamera.Lens;
            lens.OrthographicSize = 5f;
            lens.NearClipPlane = -10f;
            lens.FarClipPlane = 100f;
            virtualCamera.Lens = lens;

            // Position Composer 추가 (2D Follow)
            CinemachinePositionComposer positionComposer = vcamObj.AddComponent<CinemachinePositionComposer>();
            positionComposer.CameraDistance = 10f;
            positionComposer.Damping = new Vector3(0.5f, 0.7f, 0f);

            // Lookahead 설정
            var lookahead = positionComposer.Lookahead;
            lookahead.Time = 0.3f;
            lookahead.Smoothing = 5f;
            lookahead.IgnoreY = true;
            positionComposer.Lookahead = lookahead;

            // Composition 설정 (DeadZone, HardLimits)
            var composition = positionComposer.Composition;
            var deadZone = composition.DeadZone;
            deadZone.Size = new Vector2(0.1f, 0.05f);
            composition.DeadZone = deadZone;
            var hardLimits = composition.HardLimits;
            hardLimits.Size = new Vector2(0.8f, 0.6f);
            composition.HardLimits = hardLimits;
            composition.ScreenPosition = new Vector2(0.5f, 0.5f);
            positionComposer.Composition = composition;

            // Confiner2D 추가 (Room 경계용)
            CinemachineConfiner2D confiner = vcamObj.AddComponent<CinemachineConfiner2D>();

            // Impulse Listener 추가 (화면 흔들림)
            CinemachineImpulseListener impulseListener = vcamObj.AddComponent<CinemachineImpulseListener>();

            // CinemachinePlayerCamera 헬퍼 추가
            CinemachinePlayerCamera playerCameraHelper = vcamObj.AddComponent<CinemachinePlayerCamera>();

            Debug.Log("[PersistentManagersSceneCreator] CM PlayerCamera (Virtual Camera) 생성 완료");

            // === Impulse Source (화면 흔들림 발생기) ===
            GameObject impulseObj = new GameObject("ImpulseSource");
            impulseObj.transform.SetParent(parent, false);

            CinemachineImpulseSource impulseSource = impulseObj.AddComponent<CinemachineImpulseSource>();

            // CinemachineImpulseHelper 추가
            CinemachineImpulseHelper impulseHelper = impulseObj.AddComponent<CinemachineImpulseHelper>();

            // ImpulseHelper에 Source 연결
            SerializedObject ihSO = new SerializedObject(impulseHelper);
            ihSO.FindProperty("defaultSource").objectReferenceValue = impulseSource;
            ihSO.ApplyModifiedProperties();

            Debug.Log("[PersistentManagersSceneCreator] ImpulseSource + ImpulseHelper 생성 완료");

            // === CinemachineBridge (기존 API 호환) ===
            GameObject bridgeObj = new GameObject("CinemachineBridge");
            bridgeObj.transform.SetParent(parent, false);

            CinemachineBridge bridge = bridgeObj.AddComponent<CinemachineBridge>();

            // Bridge에 참조 연결
            SerializedObject bridgeSO = new SerializedObject(bridge);
            bridgeSO.FindProperty("playerVirtualCamera").objectReferenceValue = virtualCamera;
            bridgeSO.FindProperty("brain").objectReferenceValue = brain;
            bridgeSO.FindProperty("confiner").objectReferenceValue = confiner;
            bridgeSO.FindProperty("impulseHelper").objectReferenceValue = impulseHelper;
            bridgeSO.ApplyModifiedProperties();

            Debug.Log("[PersistentManagersSceneCreator] CinemachineBridge 생성 완료");

            // === CameraEffects (Post-Processing) ===
            CameraEffects cameraEffects = bridgeObj.AddComponent<CameraEffects>();

            // Bridge에 CameraEffects 연결
            bridgeSO = new SerializedObject(bridge);
            bridgeSO.FindProperty("cameraEffects").objectReferenceValue = cameraEffects;
            bridgeSO.ApplyModifiedProperties();

            Debug.Log("[PersistentManagersSceneCreator] CameraEffects 생성 완료");

            // === Global Volume (Post-Processing) ===
            GameObject volumeObj = new GameObject("Global Volume");
            volumeObj.transform.SetParent(parent, false);

            Volume globalVolume = volumeObj.AddComponent<Volume>();
            globalVolume.isGlobal = true;
            globalVolume.priority = 0;

            // 새 Volume Profile 생성
            var volumeProfile = ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();
            globalVolume.profile = volumeProfile;

            // CameraEffects에 Volume 참조 설정
            SerializedObject ceSO = new SerializedObject(cameraEffects);
            ceSO.FindProperty("globalVolume").objectReferenceValue = globalVolume;
            ceSO.ApplyModifiedProperties();

            Debug.Log("[PersistentManagersSceneCreator] Global Volume 생성 완료");

            // === 기존 CameraManager (레거시 호환, 선택적) ===
            // 기존 코드와의 호환성을 위해 CameraManager도 생성
            // 새 코드에서는 CinemachineBridge 사용 권장
            GameObject cameraManagerObj = new GameObject("CameraManager (Legacy)");
            cameraManagerObj.transform.SetParent(parent, false);

            CameraManager cameraManager = cameraManagerObj.AddComponent<CameraManager>();

            SerializedObject cmSO = new SerializedObject(cameraManager);
            cmSO.FindProperty("mainCamera").objectReferenceValue = camera;
            cmSO.FindProperty("defaultOrthographicSize").floatValue = 5f;
            cmSO.ApplyModifiedProperties();

            Debug.Log("[PersistentManagersSceneCreator] CameraManager (Legacy) 생성 완료");
        }

        /// <summary>
        /// EventSystem 생성
        /// </summary>
        private void CreateEventSystem(Transform parent)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.transform.SetParent(parent, false);

            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();

            Debug.Log("[PersistentManagersSceneCreator] EventSystem 생성 완료");
        }

        /// <summary>
        /// Main Canvas 생성
        /// </summary>
        private GameObject CreateMainCanvas(Transform parent)
        {
            GameObject canvasObj = new GameObject("MainCanvas");
            canvasObj.transform.SetParent(parent, false);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("[PersistentManagersSceneCreator] MainCanvas 생성 완료");
            return canvasObj;
        }

        /// <summary>
        /// UIManager 홀더 생성
        /// </summary>
        private void CreateUIManagerHolder(Transform parent)
        {
            GameObject uiManagerObj = new GameObject("UIManagerHolder");
            uiManagerObj.transform.SetParent(parent, false);

            // UIManager는 싱글톤이므로 여기서는 홀더만 생성
            // 실제 UIManager는 싱글톤 자동 생성됨

            Debug.Log("[PersistentManagersSceneCreator] UIManagerHolder 생성 완료");
        }

        /// <summary>
        /// SceneValidationManager 생성
        /// Scene/Room 전환 시 참조 검증 및 재할당을 담당
        /// </summary>
        private void CreateSceneValidationManager(Transform parent)
        {
            GameObject validationManagerObj = new GameObject("SceneValidationManager");
            validationManagerObj.transform.SetParent(parent, false);

            SceneValidationManager validationManager = validationManagerObj.AddComponent<SceneValidationManager>();

            Debug.Log("[PersistentManagersSceneCreator] SceneValidationManager 생성 완료");
        }

        /// <summary>
        /// HUD 생성
        /// </summary>
        private void CreateHUD(GameObject canvas)
        {
            GameObject hudPanel = new GameObject("HUD");
            hudPanel.transform.SetParent(canvas.transform, false);

            RectTransform hudRect = hudPanel.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0, 1);
            hudRect.anchorMax = new Vector2(0, 1);
            hudRect.pivot = new Vector2(0, 1);
            hudRect.anchoredPosition = new Vector2(20, -20);
            hudRect.sizeDelta = new Vector2(300, 120);

            Image bgImage = hudPanel.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.5f);

            // HP Bar
            CreateResourceBar(hudPanel, "HPBar", new Vector2(10, -10), new Color(0.2f, 0.8f, 0.2f));

            // Mana Bar
            CreateResourceBar(hudPanel, "ManaBar", new Vector2(10, -45), new Color(0.2f, 0.5f, 1f));

            // Gold Text
            GameObject goldObj = new GameObject("GoldText");
            goldObj.transform.SetParent(hudPanel.transform, false);

            TextMeshProUGUI goldText = goldObj.AddComponent<TextMeshProUGUI>();
            goldText.text = "Gold: 0";
            goldText.fontSize = 20;
            goldText.color = new Color(1f, 0.85f, 0f);
            goldText.alignment = TextAlignmentOptions.Left;

            RectTransform goldRect = goldObj.GetComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0, 1);
            goldRect.anchorMax = new Vector2(0, 1);
            goldRect.pivot = new Vector2(0, 1);
            goldRect.anchoredPosition = new Vector2(10, -80);
            goldRect.sizeDelta = new Vector2(200, 30);

            Debug.Log("[PersistentManagersSceneCreator] HUD 생성 완료");
        }

        private void CreateResourceBar(GameObject parent, string name, Vector2 position, Color fillColor)
        {
            GameObject barObj = new GameObject(name);
            barObj.transform.SetParent(parent.transform, false);

            RectTransform rect = barObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(280, 25);

            Image bgImage = barObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(barObj.transform, false);

            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.offsetMin = new Vector2(2, 2);
            fillRect.offsetMax = new Vector2(-2, -2);

            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = fillColor;

            Slider slider = barObj.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;
        }

        /// <summary>
        /// InventoryUI 생성 (MVP 패턴)
        /// </summary>
        private void CreateInventoryUI(GameObject canvas)
        {
            // 프리팹 로드 시도
            GameObject inventoryPrefab = Resources.Load<GameObject>(ResourcePaths.Prefabs.UI.InventoryUI);

            if (inventoryPrefab != null)
            {
                GameObject inventoryUI = PrefabUtility.InstantiatePrefab(inventoryPrefab) as GameObject;
                inventoryUI.transform.SetParent(canvas.transform, false);
                inventoryUI.name = "InventoryUI";
                Debug.Log("[PersistentManagersSceneCreator] InventoryUI (프리팹) 생성 완료");
            }
            else
            {
                CreateInventoryUIManual(canvas);
            }
        }

        private void CreateInventoryUIManual(GameObject canvas)
        {
            GameObject inventoryObj = new GameObject("InventoryUI");
            inventoryObj.transform.SetParent(canvas.transform, false);

            InventoryView inventoryView = inventoryObj.AddComponent<InventoryView>();

            // Panel
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(inventoryObj.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 400);

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // 타이틀
            CreateUITitle(panel, "인벤토리", Color.white);

            // 닫기 버튼
            Button closeButton = CreateCloseButton(panel);

            // SerializedObject로 참조 설정
            SerializedObject so = new SerializedObject(inventoryView);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("closeButton").objectReferenceValue = closeButton;
            so.ApplyModifiedProperties();

            panel.SetActive(false);

            Debug.Log("[PersistentManagersSceneCreator] InventoryUI (수동) 생성 완료");
        }

        /// <summary>
        /// ShopUI 생성 (MVP 패턴)
        /// </summary>
        private void CreateShopUI(GameObject canvas)
        {
            // 프리팹 로드 시도
            GameObject shopPrefab = Resources.Load<GameObject>(ResourcePaths.Prefabs.UI.ShopUI);

            if (shopPrefab != null)
            {
                GameObject shopUI = PrefabUtility.InstantiatePrefab(shopPrefab) as GameObject;
                shopUI.transform.SetParent(canvas.transform, false);
                shopUI.name = "ShopUI";
                Debug.Log("[PersistentManagersSceneCreator] ShopUI (프리팹) 생성 완료");
            }
            else
            {
                CreateShopUIManual(canvas);
            }
        }

        private void CreateShopUIManual(GameObject canvas)
        {
            GameObject shopObj = new GameObject("ShopUI");
            shopObj.transform.SetParent(canvas.transform, false);

            ShopView shopView = shopObj.AddComponent<ShopView>();

            // Panel
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(shopObj.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(500, 450);

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.15f, 0.1f, 0.05f, 0.95f);

            // 타이틀
            CreateUITitle(panel, "상점", new Color(1f, 0.85f, 0f));

            // 골드 표시
            GameObject goldObj = new GameObject("GoldText");
            goldObj.transform.SetParent(panel.transform, false);

            TextMeshProUGUI goldText = goldObj.AddComponent<TextMeshProUGUI>();
            goldText.text = "Gold: 0";
            goldText.fontSize = 22;
            goldText.alignment = TextAlignmentOptions.Right;
            goldText.color = new Color(1f, 0.85f, 0f);

            RectTransform goldRect = goldObj.GetComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(1, 1);
            goldRect.anchorMax = new Vector2(1, 1);
            goldRect.pivot = new Vector2(1, 1);
            goldRect.anchoredPosition = new Vector2(-60, -15);
            goldRect.sizeDelta = new Vector2(150, 30);

            // 닫기 버튼
            Button closeButton = CreateCloseButton(panel);

            // SerializedObject로 참조 설정
            SerializedObject so = new SerializedObject(shopView);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("goldText").objectReferenceValue = goldText;
            so.FindProperty("closeButton").objectReferenceValue = closeButton;
            so.ApplyModifiedProperties();

            panel.SetActive(false);

            Debug.Log("[PersistentManagersSceneCreator] ShopUI (수동) 생성 완료");
        }

        /// <summary>
        /// BuffIconPanel 생성
        /// </summary>
        private void CreateBuffIconPanel(GameObject canvas)
        {
            GameObject buffPanel = new GameObject("BuffIconPanel");
            buffPanel.transform.SetParent(canvas.transform, false);

            RectTransform rect = buffPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -150);
            rect.sizeDelta = new Vector2(400, 60);

            HorizontalLayoutGroup layoutGroup = buffPanel.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 5;
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;

            BuffIconPanelView buffIconPanelView = buffPanel.AddComponent<BuffIconPanelView>();

            GameObject buffIconPrefab = Resources.Load<GameObject>(ResourcePaths.Prefabs.UI.BuffIcon);

            SerializedObject so = new SerializedObject(buffIconPanelView);
            so.FindProperty("buffIconPrefab").objectReferenceValue = buffIconPrefab;
            so.FindProperty("iconContainer").objectReferenceValue = buffPanel.transform;
            so.ApplyModifiedProperties();

            Debug.Log("[PersistentManagersSceneCreator] BuffIconPanel 생성 완료");
        }

        /// <summary>
        /// LoadingUI 생성
        /// </summary>
        private void CreateLoadingUI(GameObject canvas)
        {
            GameObject loadingPanel = new GameObject("LoadingUI");
            loadingPanel.transform.SetParent(canvas.transform, false);

            RectTransform rect = loadingPanel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            // 배경 (검은색)
            Image bgImage = loadingPanel.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.9f);

            // 로딩 텍스트
            GameObject loadingTextObj = new GameObject("LoadingText");
            loadingTextObj.transform.SetParent(loadingPanel.transform, false);

            TextMeshProUGUI loadingText = loadingTextObj.AddComponent<TextMeshProUGUI>();
            loadingText.text = "Loading...";
            loadingText.fontSize = 40;
            loadingText.alignment = TextAlignmentOptions.Center;
            loadingText.color = Color.white;

            RectTransform textRect = loadingTextObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(400, 60);

            // 프로그레스 바
            GameObject progressBar = new GameObject("ProgressBar");
            progressBar.transform.SetParent(loadingPanel.transform, false);

            RectTransform progressRect = progressBar.AddComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0.5f, 0.5f);
            progressRect.anchorMax = new Vector2(0.5f, 0.5f);
            progressRect.anchoredPosition = new Vector2(0, -50);
            progressRect.sizeDelta = new Vector2(400, 20);

            Image progressBg = progressBar.AddComponent<Image>();
            progressBg.color = new Color(0.2f, 0.2f, 0.2f);

            GameObject progressFill = new GameObject("Fill");
            progressFill.transform.SetParent(progressBar.transform, false);

            RectTransform fillRect = progressFill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            Image fillImage = progressFill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.8f, 0.2f);

            Slider slider = progressBar.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 0;

            // 초기 상태: 숨김
            loadingPanel.SetActive(false);

            Debug.Log("[PersistentManagersSceneCreator] LoadingUI 생성 완료");
        }

        // ====== 헬퍼 메서드 ======

        private void CreateUITitle(GameObject parent, string text, Color color)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent.transform, false);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = text;
            titleText.fontSize = 30;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = color;

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -10);
            titleRect.sizeDelta = new Vector2(300, 40);
        }

        private Button CreateCloseButton(GameObject parent)
        {
            GameObject closeBtn = new GameObject("CloseButton");
            closeBtn.transform.SetParent(parent.transform, false);

            RectTransform closeBtnRect = closeBtn.AddComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(1, 1);
            closeBtnRect.anchorMax = new Vector2(1, 1);
            closeBtnRect.pivot = new Vector2(1, 1);
            closeBtnRect.anchoredPosition = new Vector2(-10, -10);
            closeBtnRect.sizeDelta = new Vector2(40, 40);

            Image closeBtnImage = closeBtn.AddComponent<Image>();
            closeBtnImage.color = new Color(0.8f, 0.2f, 0.2f);

            Button closeButton = closeBtn.AddComponent<Button>();

            GameObject closeBtnTextObj = new GameObject("Text");
            closeBtnTextObj.transform.SetParent(closeBtn.transform, false);

            TextMeshProUGUI closeBtnText = closeBtnTextObj.AddComponent<TextMeshProUGUI>();
            closeBtnText.text = "X";
            closeBtnText.fontSize = 24;
            closeBtnText.alignment = TextAlignmentOptions.Center;
            closeBtnText.color = Color.white;

            RectTransform closeBtnTextRect = closeBtnTextObj.GetComponent<RectTransform>();
            closeBtnTextRect.anchorMin = Vector2.zero;
            closeBtnTextRect.anchorMax = Vector2.one;
            closeBtnTextRect.sizeDelta = Vector2.zero;

            return closeButton;
        }
    }

    /// <summary>
    /// PersistentManagers Scene의 루트 오브젝트에 붙이는 마커
    /// DontDestroyOnLoad 처리
    /// </summary>
    public class PersistentSceneMarker : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log("[PersistentSceneMarker] DontDestroyOnLoad 설정됨");
        }
    }
}
