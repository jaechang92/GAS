using UnityEngine;
using UnityEngine.UI;
using UI.Core;
using GameFlow;

namespace UI.Panels
{
    /// <summary>
    /// 메인 메뉴 Panel
    /// 게임 시작, 설정, 종료 버튼 제공
    /// </summary>
    public class MainMenuPanel : BasePanel
    {
        [Header("UI 버튼")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("제목")]
        [SerializeField] private Text titleText;

        protected override void Awake()
        {
            base.Awake();

            // Panel 설정
            panelType = PanelType.MainMenu;
            layer = UILayer.Background;
            openTransition = TransitionType.Fade;
            closeTransition = TransitionType.Fade;
            transitionDuration = 0.5f;

            // 버튼 이벤트 연결
            SetupButtons();
        }

        /// <summary>
        /// 버튼 이벤트 설정
        /// </summary>
        private void SetupButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            }
        }

        /// <summary>
        /// 시작 버튼 클릭
        /// </summary>
        private void OnStartButtonClicked()
        {
            Debug.Log("[MainMenuPanel] 게임 시작 버튼 클릭!");

            // GameFlowManager 직접 접근
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.StartGame();
            }
            else
            {
                Debug.LogError("[MainMenuPanel] GameFlowManager가 없습니다!");
            }
        }

        /// <summary>
        /// 설정 버튼 클릭
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            Debug.Log("[MainMenuPanel] 설정 버튼 클릭 (미구현)");

            // TODO: 설정 Panel 구현 후 활성화
        }

        /// <summary>
        /// 종료 버튼 클릭
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("[MainMenuPanel] 종료 버튼 클릭");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartButtonClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            }
        }
    }
}
