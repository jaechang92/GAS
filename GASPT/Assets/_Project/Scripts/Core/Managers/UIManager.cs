using UnityEngine;
using System.Collections.Generic;
using Core;
using System.Linq;

namespace Managers
{
    /// <summary>
    /// UI 관리 매니저
    /// 최소 기능: UI 패널 표시/숨김, 로딩 오버레이
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("메인 캔버스")]
        [SerializeField] private Canvas mainCanvas;

        [Header("공통 UI")]
        [SerializeField] private GameObject loadingOverlay;

        // 현재 활성 UI 패널들
        private HashSet<GameObject> activePanels = new HashSet<GameObject>();

        // 이벤트
        public event System.Action<string> OnUIChanged;

        protected override void OnSingletonAwake()
        {
            Debug.Log("[UIManager] UI 매니저 초기화");
            SetupMainCanvas();
        }

        /// <summary>
        /// 메인 캔버스 설정
        /// </summary>
        private void SetupMainCanvas()
        {
            if (mainCanvas == null)
            {
                GameObject canvasGO = new GameObject("MainCanvas");
                canvasGO.transform.SetParent(transform);

                mainCanvas = canvasGO.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mainCanvas.sortingOrder = 0;

                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                Debug.Log("[UIManager] 메인 캔버스 생성 완료");
            }
        }

        /// <summary>
        /// UI 패널 활성화/비활성화
        /// </summary>
        public void SetUIActive(GameObject uiPanel, bool active)
        {
            if (uiPanel == null) return;

            if (active)
            {
                activePanels.Add(uiPanel);
                uiPanel.SetActive(true);
                OnUIChanged?.Invoke($"{uiPanel.name}_Opened");
                Debug.Log($"[UIManager] UI 활성화: {uiPanel.name}");
            }
            else
            {
                activePanels.Remove(uiPanel);
                uiPanel.SetActive(false);
                OnUIChanged?.Invoke($"{uiPanel.name}_Closed");
                Debug.Log($"[UIManager] UI 비활성화: {uiPanel.name}");
            }
        }

        /// <summary>
        /// 모든 UI 패널 숨김
        /// </summary>
        public void HideAllUI()
        {
            foreach (var panel in activePanels.ToArray())
            {
                panel.SetActive(false);
            }

            activePanels.Clear();
            OnUIChanged?.Invoke("AllUI_Closed");
            Debug.Log("[UIManager] 모든 UI 숨김");
        }

        /// <summary>
        /// 로딩 오버레이 표시/숨김
        /// </summary>
        public void ShowLoadingOverlay(bool show)
        {
            if (loadingOverlay == null)
            {
                CreateLoadingOverlay();
            }

            SetUIActive(loadingOverlay, show);
        }

        /// <summary>
        /// 기본 로딩 오버레이 생성
        /// </summary>
        private void CreateLoadingOverlay()
        {
            if (mainCanvas == null) return;

            GameObject overlayGO = new GameObject("LoadingOverlay");
            overlayGO.transform.SetParent(mainCanvas.transform, false);

            // 전체 화면을 덮는 이미지
            var rectTransform = overlayGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            var image = overlayGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0.8f); // 반투명 검정

            // 로딩 텍스트
            GameObject textGO = new GameObject("LoadingText");
            textGO.transform.SetParent(overlayGO.transform, false);

            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var text = textGO.AddComponent<UnityEngine.UI.Text>();
            text.text = "Loading...";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            loadingOverlay = overlayGO;
            loadingOverlay.SetActive(false);

            Debug.Log("[UIManager] 로딩 오버레이 생성 완료");
        }

        /// <summary>
        /// 메인 캔버스 가져오기
        /// </summary>
        public Canvas GetMainCanvas()
        {
            return mainCanvas;
        }

        /// <summary>
        /// 활성 UI 개수 확인
        /// </summary>
        public int GetActiveUICount()
        {
            return activePanels.Count;
        }

        /// <summary>
        /// 특정 UI가 활성화되어 있는지 확인
        /// </summary>
        public bool IsUIActive(GameObject uiPanel)
        {
            return activePanels.Contains(uiPanel);
        }

        // TODO: 차후 구현 예정
        // - UI 스택 관리 (뒤로가기 기능)
        // - 애니메이션 효과
        // - 알림 패널 시스템
        // - 확인 대화상자
        // - UI 사운드 효과
        // - 해상도별 UI 스케일링
        // - UI 테마 시스템
    }
}
