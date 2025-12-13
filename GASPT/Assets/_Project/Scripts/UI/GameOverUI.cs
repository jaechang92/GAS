using System;
using GASPT.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GASPT.UI
{
    /// <summary>
    /// 게임 오버 UI
    /// 플레이어 사망 시 표시
    /// </summary>
    public class GameOverUI : BaseUI
    {
        // ====== 싱글톤 ======

        private static GameOverUI instance;
        public static GameOverUI Instance => instance;
        public static bool HasInstance => instance != null;


        // ====== UI 요소 ======

        [Header("GameOver UI 요소")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text roomCountText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;


        // ====== 이벤트 ======

        public event Action OnRetryClicked;
        public event Action OnMainMenuClicked;


        // ====== Unity 생명주기 ======

        protected override void Awake()
        {
            // 싱글톤 설정
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            base.Awake();
        }

        protected override void Initialize()
        {
            // 버튼 이벤트 연결
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryButtonClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }


        // ====== UI 표시 ======

        /// <summary>
        /// 게임 오버 UI 표시 (통계 포함)
        /// </summary>
        public void ShowGameOver(int goldEarned = 0, int roomsCleared = 0)
        {
            // 제목 설정
            if (titleText != null)
            {
                titleText.text = "게임 오버";
            }

            // 메시지 설정
            if (messageText != null)
            {
                messageText.text = "당신은 쓰러졌습니다...";
            }

            // 통계 표시
            if (goldText != null)
            {
                goldText.text = $"획득 골드: {goldEarned}G";
            }

            if (roomCountText != null)
            {
                roomCountText.text = $"클리어한 방: {roomsCleared}개";
            }

            Show();

            Debug.Log($"[GameOverUI] 게임 오버 표시 - 골드: {goldEarned}G, 방: {roomsCleared}개");
        }

        /// <summary>
        /// 간단한 게임 오버 표시
        /// </summary>
        public override void Show()
        {
            base.Show();

            // 게임 일시정지
            Time.timeScale = 0f;
        }

        /// <summary>
        /// UI 숨기기
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            // 게임 재개
            Time.timeScale = 1f;
        }


        // ====== 버튼 핸들러 ======

        private void OnRetryButtonClicked()
        {
            Debug.Log("[GameOverUI] 재시도 버튼 클릭");

            Hide();

            // 이벤트 발생
            OnRetryClicked?.Invoke();

            // GameFlowStateMachine을 통해 StartRoom으로 복귀
            if (GameFlowStateMachine.Instance != null)
            {
                GameFlowStateMachine.Instance.TriggerRestartGame();
            }
        }

        private void OnMainMenuButtonClicked()
        {
            Debug.Log("[GameOverUI] 메인 메뉴 버튼 클릭");

            Hide();

            // 이벤트 발생
            OnMainMenuClicked?.Invoke();

            // GameFlowStateMachine을 통해 StartRoom으로 복귀
            if (GameFlowStateMachine.Instance != null)
            {
                GameFlowStateMachine.Instance.TriggerRestartGame();
            }
        }
    }
}
