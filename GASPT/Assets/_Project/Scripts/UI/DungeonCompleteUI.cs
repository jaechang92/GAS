using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace GASPT.UI
{
    /// <summary>
    /// 던전 클리어 UI
    /// 던전 완주 시 보상 정보를 표시하고 다음 행동 선택
    /// </summary>
    public class DungeonCompleteUI : MonoBehaviour
    {
        // ====== UI 요소 ======

        [Header("UI 요소")]
        [Tooltip("UI 패널 (활성화/비활성화)")]
        [SerializeField] private GameObject uiPanel;

        [Tooltip("타이틀 텍스트")]
        [SerializeField] private Text titleText;

        [Tooltip("보상 정보 텍스트")]
        [SerializeField] private Text rewardText;

        [Tooltip("다음 던전 버튼")]
        [SerializeField] private Button nextDungeonButton;

        [Tooltip("메인 메뉴 버튼")]
        [SerializeField] private Button mainMenuButton;


        // ====== 설정 ======

        [Header("설정")]
        [Tooltip("타이틀 메시지")]
        [SerializeField] private string titleMessage = "던전 클리어!";

        [Tooltip("표시 시 시간 정지")]
        [SerializeField] private bool pauseGameOnShow = true;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // 초기 상태: 숨김
            Hide();

            // 버튼 이벤트 연결
            if (nextDungeonButton != null)
            {
                nextDungeonButton.onClick.AddListener(OnNextDungeonClick);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClick);
            }

            // 타이틀 설정
            if (titleText != null)
            {
                titleText.text = titleMessage;
            }
        }


        // ====== UI 표시/숨김 ======

        /// <summary>
        /// UI 표시
        /// </summary>
        /// <param name="totalGold">총 획득 골드</param>
        /// <param name="totalExp">총 획득 경험치</param>
        public void Show(int totalGold, int totalExp)
        {
            if (uiPanel != null)
            {
                uiPanel.SetActive(true);
            }

            // 보상 정보 업데이트
            UpdateRewardText(totalGold, totalExp);

            // 시간 정지
            if (pauseGameOnShow)
            {
                Time.timeScale = 0f;
            }

            Debug.Log($"[DungeonCompleteUI] UI 표시 - 골드: {totalGold}, 경험치: {totalExp}");
        }

        /// <summary>
        /// UI 숨김
        /// </summary>
        public void Hide()
        {
            if (uiPanel != null)
            {
                uiPanel.SetActive(false);
            }

            // 시간 재개
            if (pauseGameOnShow)
            {
                Time.timeScale = 1f;
            }
        }

        /// <summary>
        /// 보상 텍스트 업데이트
        /// </summary>
        private void UpdateRewardText(int totalGold, int totalExp)
        {
            if (rewardText == null) return;

            string rewardInfo = $"총 획득 골드: {totalGold}\n" +
                               $"총 획득 경험치: {totalExp}\n\n" +
                               $"축하합니다!";

            rewardText.text = rewardInfo;
        }


        // ====== 버튼 이벤트 ======

        /// <summary>
        /// 다음 던전 버튼 클릭
        /// </summary>
        private void OnNextDungeonClick()
        {
            Debug.Log("[DungeonCompleteUI] 다음 던전 버튼 클릭");

            // UI 숨김
            Hide();

            // TODO: 다음 던전으로 이동 (현재는 씬 재시작)
            RestartCurrentScene();
        }

        /// <summary>
        /// 메인 메뉴 버튼 클릭
        /// </summary>
        private void OnMainMenuClick()
        {
            Debug.Log("[DungeonCompleteUI] 메인 메뉴 버튼 클릭");

            // UI 숨김
            Hide();

            // TODO: 메인 메뉴로 이동
            LoadMainMenu();
        }

        /// <summary>
        /// 현재 씬 재시작
        /// </summary>
        private void RestartCurrentScene()
        {
            // 시간 재개 (씬 로드 전)
            Time.timeScale = 1f;

            // 현재 씬 재로드
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }

        /// <summary>
        /// 메인 메뉴로 이동
        /// </summary>
        private void LoadMainMenu()
        {
            // 시간 재개 (씬 로드 전)
            Time.timeScale = 1f;

            // 메인 메뉴 씬 로드 (씬 이름 수정 필요)
            // SceneManager.LoadScene("MainMenu");

            // 임시: 현재 씬 재시작
            Debug.LogWarning("[DungeonCompleteUI] 메인 메뉴 씬이 없으므로 현재 씬 재시작");
            RestartCurrentScene();
        }


        // ====== 디버그 ======

        [ContextMenu("Show UI (Test)")]
        private void DebugShow()
        {
            Show(1000, 500);
        }

        [ContextMenu("Hide UI (Test)")]
        private void DebugHide()
        {
            Hide();
        }
    }
}
