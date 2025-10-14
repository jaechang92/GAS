using UnityEngine;
using UnityEngine.UI;
using UI.Core;
using GameFlow;

namespace UI.Panels
{
    /// <summary>
    /// 일시정지 Panel
    /// 게임 재개, 설정, 메인 메뉴 복귀 버튼 제공
    /// </summary>
    public class PausePanel : BasePanel
    {
        [Header("UI 버튼")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("제목")]
        [SerializeField] private Text titleText;

        protected override void Awake()
        {
            base.Awake();

            // Panel 설정
            panelType = PanelType.Pause;
            layer = UILayer.Popup;
            closeOnEscape = true;  // ESC 키로 닫기 가능
            openTransition = TransitionType.Scale;
            closeTransition = TransitionType.Scale;
            transitionDuration = 0.3f;

            // 버튼 이벤트 연결
            SetupButtons();

            // Panel 이벤트 구독 (Time.timeScale 제어)
            OnOpened += OnPanelOpened;
            OnClosed += OnPanelClosed;
        }

        private void OnPanelOpened(BasePanel panel)
        {
            Debug.Log("[PausePanel] 일시정지");
            Time.timeScale = 0f;
        }

        private void OnPanelClosed(BasePanel panel)
        {
            Debug.Log("[PausePanel] 재개");
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 버튼 이벤트 설정
        /// </summary>
        private void SetupButtons()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeButtonClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
            }
        }

        /// <summary>
        /// 재개 버튼 클릭
        /// </summary>
        private void OnResumeButtonClicked()
        {
            Debug.Log("[PausePanel] 재개 버튼 클릭");
            Close();
        }

        /// <summary>
        /// 설정 버튼 클릭
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            Debug.Log("[PausePanel] 설정 버튼 클릭 (미구현)");

            // TODO: 설정 Panel 구현 후 활성화
            // 이벤트 방식으로 처리하거나 GameFlowManager를 통해 처리
        }

        /// <summary>
        /// 메인 메뉴 버튼 클릭
        /// </summary>
        private void OnMainMenuButtonClicked()
        {
            Debug.Log("[PausePanel] 메인 메뉴 버튼 클릭");

            // Panel 닫기
            Close();

            // GameFlowManager 직접 접근
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.TriggerEvent(GameEventType.ReturnToMainMenu);
            }
            else
            {
                Debug.LogError("[PausePanel] GameFlowManager가 없습니다!");
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
            }

            // Panel 이벤트 구독 해제
            OnOpened -= OnPanelOpened;
            OnClosed -= OnPanelClosed;
        }
    }
}
