using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Core;
using UI.Core;
using TMPro;

namespace Core.Managers
{
    /// <summary>
    /// 모든 UI Panel을 관리하는 중앙 매니저
    /// Panel Prefab 로딩, 생명주기 관리, Stack 관리
    /// </summary>
    public class UIManager : SingletonManager<UIManager>
    {
        [Header("Prefab 경로 설정")]
        private const string PREFAB_PATH = "UI/";  // Resources 폴더 기준 상대 경로
        private const string PANEL_PREFAB_PATH = "UI/Panels/";

        [Header("Preload 설정")]
        [Tooltip("게임 시작 시 미리 로드할 Panel 목록 (빠른 반응이 필요한 Panel)")]
        [SerializeField] private PanelType[] preloadPanels = new PanelType[]
        {
            PanelType.Loading,  // 자주 사용, 즉시 표시 필요
            PanelType.Pause     // ESC로 즉시 표시 필요
        };

        [Header("Layer Canvas")]
        [SerializeField] private Canvas backgroundCanvas;
        [SerializeField] private Canvas normalCanvas;
        [SerializeField] private Canvas popupCanvas;
        [SerializeField] private Canvas systemCanvas;
        [SerializeField] private Canvas transitionCanvas;

        [Header("Overlay Canvas (InteractionPrompt용)")]
        [SerializeField] private Canvas overlayCanvas;

        [Header("InteractionPrompt 설정")]
        [SerializeField] private string interactionPromptPrefabPath = $"{PREFAB_PATH}InteractionPrompt";
        [SerializeField] private Vector3 defaultPromptOffset = new Vector3(0, 1.5f, 0);
        [SerializeField] private float promptFontSize = 18f;

        [Header("디버그")]
        [SerializeField] private bool showDebugLog = true;

        // Panel 캐시 (PanelType별로 인스턴스 보관)
        private Dictionary<PanelType, BasePanel> panelCache = new Dictionary<PanelType, BasePanel>();

        // Panel Stack (뒤로가기 기능)
        private Stack<BasePanel> panelStack = new Stack<BasePanel>();

        // 현재 열려있는 Panel들
        private HashSet<BasePanel> openPanels = new HashSet<BasePanel>();

        // InteractionPrompt 관련
        private GameObject interactionPrompt;
        private TextMeshProUGUI interactionPromptText;
        private Transform interactionPromptTarget;
        private Vector3 interactionPromptOffset;

        protected override void OnSingletonAwake()
        {
            CreateLayerCanvases();
            CreateOverlayCanvas();
            CreateInteractionPrompt();

            // 설정된 Panel 자동 Preload
            _ = AutoPreloadPanels();

            Log("[UIManager] 초기화 완료");
        }

        /// <summary>
        /// 설정된 Panel들을 자동으로 Preload
        /// </summary>
        private async Awaitable AutoPreloadPanels()
        {
            if (preloadPanels != null && preloadPanels.Length > 0)
            {
                Log($"[UIManager] 자동 Preload 시작: {preloadPanels.Length}개 Panel");
                await PreloadPanels(preloadPanels);
            }
        }

        /// <summary>
        /// Layer별 Canvas 생성
        /// </summary>
        private void CreateLayerCanvases()
        {
            backgroundCanvas = CreateCanvas("BackgroundCanvas", UILayer.Background);
            normalCanvas = CreateCanvas("NormalCanvas", UILayer.Normal);
            popupCanvas = CreateCanvas("PopupCanvas", UILayer.Popup);
            systemCanvas = CreateCanvas("SystemCanvas", UILayer.System);
            transitionCanvas = CreateCanvas("TransitionCanvas", UILayer.Transition);

            Log("[UIManager] Layer Canvas 생성 완료");
        }

        /// <summary>
        /// Overlay Canvas 생성 (Screen Space - Camera)
        /// </summary>
        private void CreateOverlayCanvas()
        {
            GameObject canvasObj = new GameObject("OverlayCanvas");
            canvasObj.transform.SetParent(transform);

            overlayCanvas = canvasObj.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            overlayCanvas.worldCamera = Camera.main;
            overlayCanvas.sortingOrder = 1000; // 다른 UI 위에 표시

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            Log("[UIManager] Overlay Canvas 생성 완료");
        }

        /// <summary>
        /// InteractionPrompt 생성 (재사용 가능한 단일 인스턴스)
        /// </summary>
        private void CreateInteractionPrompt()
        {
            if (overlayCanvas == null)
            {
                Debug.LogError("[UIManager] Overlay Canvas가 null입니다. CreateInteractionPrompt 실패.");
                return;
            }

            // 이미 생성되어 있으면 스킵
            if (interactionPrompt != null)
            {
                Log("[UIManager] InteractionPrompt가 이미 존재합니다.");
                return;
            }

            Log("[UIManager] InteractionPrompt 생성 시작");

            // 1. GameResourceManager에서 Prefab 로드 시도
            if (GameResourceManager.Instance != null)
            {
                GameObject prefab = GameResourceManager.Instance.Load<GameObject>(interactionPromptPrefabPath);

                if (prefab != null)
                {
                    Log($"[UIManager] Prefab에서 InteractionPrompt 생성: {interactionPromptPrefabPath}");
                    interactionPrompt = Instantiate(prefab, overlayCanvas.transform, false);
                    interactionPrompt.name = "InteractionPrompt";

                    // Prefab에서 TextMeshProUGUI 컴포넌트 찾기
                    interactionPromptText = interactionPrompt.GetComponentInChildren<TextMeshProUGUI>();

                    if (interactionPromptText == null)
                    {
                        Debug.LogError("[UIManager] Prefab에 TextMeshProUGUI 컴포넌트가 없습니다!");
                        Destroy(interactionPrompt);
                        interactionPrompt = null;
                    }
                    else
                    {
                        // 초기에는 숨김
                        interactionPrompt.SetActive(false);
                        Log("[UIManager] InteractionPrompt Prefab 생성 완료");
                        return;
                    }
                }
                else
                {
                    Log($"[UIManager] Prefab을 찾을 수 없습니다: {interactionPromptPrefabPath}. 코드로 생성합니다.");
                }
            }
            else
            {
                Log("[UIManager] GameResourceManager가 없습니다. 코드로 생성합니다.");
            }

            // 2. Prefab이 없으면 코드로 생성
            CreateInteractionPromptFromCode();
        }

        /// <summary>
        /// InteractionPrompt를 코드로 생성 (Prefab 폴백)
        /// </summary>
        private void CreateInteractionPromptFromCode()
        {
            Log("[UIManager] 코드로 InteractionPrompt 생성 시작");

            // InteractionPrompt 오브젝트 생성
            interactionPrompt = new GameObject("InteractionPrompt");
            interactionPrompt.transform.SetParent(overlayCanvas.transform, false);

            // RectTransform 설정
            var rectTransform = interactionPrompt.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50); // 픽셀 단위 크기
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // 배경 추가 (가독성 향상)
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(interactionPrompt.transform, false);

            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);

            // TextMeshProUGUI 추가
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(interactionPrompt.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            interactionPromptText = textObj.AddComponent<TextMeshProUGUI>();
            interactionPromptText.text = ""; // 초기 비어있음
            interactionPromptText.fontSize = promptFontSize;
            interactionPromptText.enableAutoSizing = true;
            interactionPromptText.color = Color.white;
            interactionPromptText.alignment = TextAlignmentOptions.Center;
            interactionPromptText.fontStyle = FontStyles.Normal;

            // 초기에는 숨김
            interactionPrompt.SetActive(false);

            Log("[UIManager] 코드로 InteractionPrompt 생성 완료");
        }

        /// <summary>
        /// Canvas 생성 헬퍼
        /// </summary>
        private Canvas CreateCanvas(string name, UILayer layer)
        {
            GameObject canvasObj = new GameObject(name);
            canvasObj.transform.SetParent(transform);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = (int)layer;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            return canvas;
        }

        /// <summary>
        /// Panel 열기 (제네릭)
        /// </summary>
        public async Awaitable<T> OpenPanel<T>(bool addToStack = false) where T : BasePanel
        {
            PanelType panelType = GetPanelTypeFromClass<T>();
            BasePanel panel = await OpenPanel(panelType, addToStack);
            return panel as T;
        }

        /// <summary>
        /// Panel 열기 (PanelType)
        /// </summary>
        public async Awaitable<BasePanel> OpenPanel(PanelType panelType, bool addToStack = false)
        {
            Log($"[UIManager] Panel 열기 시도: {panelType}");

            // 캐시에서 찾기
            if (!panelCache.TryGetValue(panelType, out BasePanel panel))
            {
                // 없으면 로드
                panel = await LoadPanel(panelType);
                if (panel == null)
                {
                    Debug.LogError($"[UIManager] Panel을 로드할 수 없습니다: {panelType}");
                    return null;
                }
                panelCache[panelType] = panel;
            }

            // Stack에 추가
            if (addToStack)
            {
                panelStack.Push(panel);
                Log($"[UIManager] Panel을 Stack에 추가: {panelType}");
            }

            // Panel 열기 (단순 활성화)
            panel.Open();
            openPanels.Add(panel);

            Log($"[UIManager] Panel 열기 완료: {panelType}");
            return panel;
        }

        /// <summary>
        /// Panel 미리 로드 (생성만 하고 Open하지 않음)
        /// 첫 Open 시 지연을 없애기 위해 사용
        /// </summary>
        public async Awaitable PreloadPanel(PanelType panelType)
        {
            // 이미 캐시에 있으면 스킵
            if (panelCache.ContainsKey(panelType))
            {
                Log($"[UIManager] 이미 로드됨: {panelType}");
                return;
            }

            Log($"[UIManager] Panel Preload 시작: {panelType}");

            // LoadPanel만 호출 (Instantiate만 하고 Open은 안 함)
            BasePanel panel = await LoadPanel(panelType);
            if (panel != null)
            {
                panelCache[panelType] = panel;
                Log($"[UIManager] Panel Preload 완료: {panelType}");
            }
            else
            {
                Debug.LogError($"[UIManager] Panel Preload 실패: {panelType}");
            }
        }

        /// <summary>
        /// 여러 Panel을 한번에 Preload
        /// </summary>
        public async Awaitable PreloadPanels(PanelType[] panelTypes)
        {
            Log($"[UIManager] {panelTypes.Length}개 Panel Preload 시작");

            foreach (var panelType in panelTypes)
            {
                await PreloadPanel(panelType);
            }

            Log($"[UIManager] 전체 Panel Preload 완료");
        }

        /// <summary>
        /// Panel 닫기 (제네릭)
        /// </summary>
        public async Awaitable ClosePanel<T>() where T : BasePanel
        {
            PanelType panelType = GetPanelTypeFromClass<T>();
            await ClosePanel(panelType);
        }

        /// <summary>
        /// Panel 닫기 (PanelType)
        /// </summary>
        public async Awaitable ClosePanel(PanelType panelType)
        {
            Log($"[UIManager] Panel 닫기 시도: {panelType}");

            if (!panelCache.TryGetValue(panelType, out BasePanel panel))
            {
                Debug.LogWarning($"[UIManager] Panel을 찾을 수 없습니다: {panelType}");
                return;
            }

            // Panel 닫기 (단순 비활성화)
            panel.Close();
            openPanels.Remove(panel);

            // Stack에서 제거 (있다면)
            if (panelStack.Contains(panel))
            {
                // Stack을 임시 리스트로 변환 후 제거
                List<BasePanel> tempList = new List<BasePanel>(panelStack);
                tempList.Remove(panel);
                panelStack.Clear();
                foreach (var p in tempList)
                {
                    panelStack.Push(p);
                }
            }

            Log($"[UIManager] Panel 닫기 완료: {panelType}");
        }

        /// <summary>
        /// 뒤로가기 (Stack 최상위 Panel 닫기)
        /// </summary>
        public async Awaitable GoBack()
        {
            if (panelStack.Count > 0)
            {
                BasePanel panel = panelStack.Pop();
                panel.Close();
                openPanels.Remove(panel);

                Log($"[UIManager] 뒤로가기: {panel.PanelType} 닫음");
            }
            else
            {
                Log("[UIManager] 뒤로가기: Stack이 비어있습니다.");
            }
        }

        /// <summary>
        /// 모든 Panel 닫기
        /// </summary>
        public async Awaitable CloseAllPanels(UILayer? targetLayer = null)
        {
            Log($"[UIManager] 모든 Panel 닫기 (Layer: {targetLayer?.ToString() ?? "All"})");

            List<BasePanel> panelsToClose = new List<BasePanel>(openPanels);

            foreach (var panel in panelsToClose)
            {
                if (targetLayer == null || panel.Layer == targetLayer)
                {
                    panel.Close();
                    openPanels.Remove(panel);
                }
            }

            panelStack.Clear();
            Log("[UIManager] 모든 Panel 닫기 완료");
        }

        /// <summary>
        /// Panel이 열려있는지 확인
        /// </summary>
        public bool IsPanelOpen(PanelType panelType)
        {
            if (panelCache.TryGetValue(panelType, out BasePanel panel))
            {
                return panel.IsOpen;
            }
            return false;
        }

        /// <summary>
        /// Panel 인스턴스 가져오기
        /// </summary>
        public T GetPanel<T>(PanelType panelType) where T : BasePanel
        {
            if (panelCache.TryGetValue(panelType, out BasePanel panel))
            {
                return panel as T;
            }
            return null;
        }

        #region InteractionPrompt 관리

        /// <summary>
        /// 상호작용 프롬프트 표시
        /// </summary>
        /// <param name="target">프롬프트를 표시할 대상 Transform (보통 NPC)</param>
        /// <param name="text">표시할 텍스트</param>
        /// <param name="offset">대상으로부터의 오프셋 (기본값 사용 시 null)</param>
        public void ShowInteractionPrompt(Transform target, string text, Vector3? offset = null)
        {
            if (interactionPrompt == null || interactionPromptText == null)
            {
                Debug.LogWarning("[UIManager] InteractionPrompt가 초기화되지 않았습니다.");
                return;
            }

            if (target == null)
            {
                Debug.LogWarning("[UIManager] ShowInteractionPrompt: target이 null입니다.");
                return;
            }

            interactionPromptTarget = target;
            interactionPromptOffset = offset ?? defaultPromptOffset;
            interactionPromptText.text = text;

            // 프롬프트 활성화 및 위치 업데이트
            interactionPrompt.SetActive(true);
            UpdateInteractionPromptPosition();

            Log($"[UIManager] InteractionPrompt 표시: {text}");
        }

        /// <summary>
        /// 상호작용 프롬프트 숨김
        /// </summary>
        public void HideInteractionPrompt()
        {
            if (interactionPrompt == null) return;

            interactionPrompt.SetActive(false);
            interactionPromptTarget = null;

            Log("[UIManager] InteractionPrompt 숨김");
        }

        /// <summary>
        /// InteractionPrompt 위치 업데이트 (매 프레임 호출)
        /// </summary>
        private void UpdateInteractionPromptPosition()
        {
            if (interactionPrompt == null || interactionPromptTarget == null) return;
            if (Camera.main == null) return;

            // Target의 월드 위치 + 오프셋
            Vector3 worldPosition = interactionPromptTarget.position + interactionPromptOffset;

            // 월드 좌표를 스크린 좌표로 변환
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

            // 카메라 뒤에 있으면 숨김
            if (screenPosition.z < 0)
            {
                interactionPrompt.SetActive(false);
                return;
            }

            // RectTransform 위치 설정
            RectTransform rectTransform = interactionPrompt.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = screenPosition;
            }
        }

        #endregion

        /// <summary>
        /// Panel 언로드 (메모리에서 제거)
        /// </summary>
        public void UnloadPanel(PanelType panelType)
        {
            if (!panelCache.TryGetValue(panelType, out BasePanel panel))
            {
                Log($"[UIManager] Unload 실패: Panel이 로드되지 않음 - {panelType}");
                return;
            }

            // 열려있는 Panel은 Unload 불가
            if (panel.IsOpen)
            {
                Debug.LogWarning($"[UIManager] 열려있는 Panel은 Unload할 수 없습니다: {panelType}");
                return;
            }

            // GameObject 파괴 및 캐시에서 제거
            Destroy(panel.gameObject);
            panelCache.Remove(panelType);

            Log($"[UIManager] Panel Unload 완료: {panelType}");
        }

        /// <summary>
        /// 여러 Panel을 한번에 Unload
        /// </summary>
        public void UnloadPanels(PanelType[] panelTypes)
        {
            Log($"[UIManager] {panelTypes.Length}개 Panel Unload 시작");

            foreach (var panelType in panelTypes)
            {
                UnloadPanel(panelType);
            }

            Log($"[UIManager] 전체 Panel Unload 완료");
        }

        /// <summary>
        /// Panel Prefab 로드
        /// </summary>
        private async Awaitable<BasePanel> LoadPanel(PanelType panelType)
        {
            string path = $"{PANEL_PREFAB_PATH}{panelType}Panel";
            Log($"[UIManager] Panel Prefab 로드 시도: {path}");

            // Resources.LoadAsync 사용
            ResourceRequest request = Resources.LoadAsync<GameObject>(path);

            while (!request.isDone)
            {
                await Awaitable.NextFrameAsync();
            }

            GameObject prefab = request.asset as GameObject;
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] Panel Prefab을 찾을 수 없습니다: {path}");
                return null;
            }

            // Prefab에서 BasePanel 컴포넌트 확인
            BasePanel panelComponent = prefab.GetComponent<BasePanel>();
            if (panelComponent == null)
            {
                Debug.LogError($"[UIManager] Prefab에 BasePanel 컴포넌트가 없습니다: {path}");
                return null;
            }

            // 적절한 Layer Canvas 하위에 인스턴스화
            Canvas parentCanvas = GetCanvasForLayer(panelComponent.Layer);
            GameObject instance = Instantiate(prefab, parentCanvas.transform, false);

            // Instantiate 직후 명시적으로 비활성화 (Prefab 상태와 무관하게)
            instance.SetActive(false);
            instance.name = $"{panelType}Panel";

            BasePanel panel = instance.GetComponent<BasePanel>();
            Log($"[UIManager] Panel Prefab 로드 완료 (비활성 상태): {panelType}");

            return panel;
        }

        /// <summary>
        /// Layer에 맞는 Canvas 반환
        /// </summary>
        private Canvas GetCanvasForLayer(UILayer layer)
        {
            return layer switch
            {
                UILayer.Background => backgroundCanvas,
                UILayer.Normal => normalCanvas,
                UILayer.Popup => popupCanvas,
                UILayer.System => systemCanvas,
                UILayer.Transition => transitionCanvas,
                _ => normalCanvas
            };
        }

        /// <summary>
        /// 클래스 타입에서 PanelType 추출
        /// </summary>
        private PanelType GetPanelTypeFromClass<T>() where T : BasePanel
        {
            // Type 이름에서 "Panel" 제거
            string typeName = typeof(T).Name.Replace("Panel", "");

            // PanelType enum으로 변환
            if (System.Enum.TryParse(typeName, out PanelType result))
            {
                return result;
            }

            Debug.LogWarning($"[UIManager] PanelType을 찾을 수 없습니다: {typeName}");
            return PanelType.None;
        }

        private void Update()
        {
            // ESC 키 처리
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscapeKey();
            }

            // InteractionPrompt 위치 추적
            if (interactionPrompt != null && interactionPrompt.activeSelf && interactionPromptTarget != null)
            {
                UpdateInteractionPromptPosition();
            }
            else if (interactionPrompt != null && interactionPrompt.activeSelf && interactionPromptTarget == null)
            {
                // Target이 파괴되었으면 프롬프트 숨김
                HideInteractionPrompt();
            }
        }

        /// <summary>
        /// ESC 키 처리
        /// closeOnEscape가 true인 Panel 중 Layer가 가장 높은 것을 닫음
        /// </summary>
        private void HandleEscapeKey()
        {
            BasePanel targetPanel = null;
            int highestLayer = int.MinValue;

            // 열려있는 Panel 중 closeOnEscape가 true이고 Layer가 가장 높은 것 찾기
            foreach (var panel in openPanels)
            {
                if (panel.CloseOnEscape)
                {
                    int layerValue = (int)panel.Layer;

                    if (layerValue > highestLayer)
                    {
                        highestLayer = layerValue;
                        targetPanel = panel;
                    }
                }
            }

            // 대상 Panel 닫기
            if (targetPanel != null)
            {
                Log($"[UIManager] ESC 키로 Panel 닫기: {targetPanel.PanelType}");
                ClosePanel(targetPanel.PanelType);
            }
            else
            {
                Log("[UIManager] ESC 키: 닫을 수 있는 Panel이 없습니다.");
            }
        }

        /// <summary>
        /// 디버그 로그
        /// </summary>
        private void Log(string message)
        {
            if (showDebugLog)
            {
                Debug.Log(message);
            }
        }

        #region 디버그 정보

        /// <summary>
        /// 설정된 Panel들을 수동으로 Preload (테스트용)
        /// </summary>
        [ContextMenu("Preload Configured Panels")]
        private void TestPreloadPanels()
        {
            _ = AutoPreloadPanels();
        }

        /// <summary>
        /// 현재 상태 출력 (디버그용)
        /// </summary>
        [ContextMenu("Print UI State")]
        public void PrintUIState()
        {
            Debug.Log("=== UI Manager State ===");
            Debug.Log($"Cached Panels: {panelCache.Count}");
            Debug.Log($"Open Panels: {openPanels.Count}");
            Debug.Log($"Stack Size: {panelStack.Count}");

            Debug.Log("\n=== Open Panels ===");
            foreach (var panel in openPanels)
            {
                Debug.Log($"- {panel.PanelType} (Layer: {panel.Layer})");
            }

            Debug.Log("\n=== Panel Stack ===");
            int index = 0;
            foreach (var panel in panelStack)
            {
                Debug.Log($"{index++}. {panel.PanelType}");
            }
        }

        #endregion
    }
}
