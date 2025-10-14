using UnityEngine;
using UnityEngine.UI;
using UI.Core;
using Core.Utilities.Interfaces;
using GameFlow;

namespace UI.Panels
{
    /// <summary>
    /// 게임플레이 HUD Panel
    /// 체력바, 콤보, 점수, 적 카운트 등 표시
    /// </summary>
    public class GameplayHUDPanel : BasePanel
    {
        [Header("UI 참조")]
        [SerializeField] private MonoBehaviour healthBar;  // HealthBarUI (Reflection으로 접근)
        [SerializeField] private Text comboText;
        [SerializeField] private Text enemyCountText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Button pauseButton;

        // 상태 추적
        private int currentCombo = 0;
        private int enemyCount = 0;
        private int score = 0;

        // 체력 시스템 참조 (인터페이스 사용)
        private IHealthEventProvider playerHealthSystem;

        protected override void Awake()
        {
            base.Awake();

            // Panel 설정
            panelType = PanelType.GameplayHUD;
            layer = UILayer.Normal;
            openTransition = TransitionType.Fade;
            closeTransition = TransitionType.Fade;
            transitionDuration = 0.3f;

            // 일시정지 버튼 이벤트 연결
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
            }

            // Panel 이벤트 구독
            OnOpened += OnPanelOpened;
            OnClosed += OnPanelClosed;
        }

        private void OnPanelOpened(BasePanel panel)
        {
            Debug.Log("[GameplayHUDPanel] 게임플레이 HUD 표시");
            SetupHealthSystem();
        }

        private void OnPanelClosed(BasePanel panel)
        {
            Debug.Log("[GameplayHUDPanel] 게임플레이 HUD 숨김");

            // 체력 시스템 연결 해제
            if (playerHealthSystem != null)
            {
                playerHealthSystem.OnHealthChanged -= OnPlayerHealthChanged;
                playerHealthSystem = null;
            }
        }

        /// <summary>
        /// 플레이어 체력 시스템 연결
        /// </summary>
        private void SetupHealthSystem()
        {
            // 플레이어 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[GameplayHUDPanel] 플레이어를 찾을 수 없습니다. Tag를 확인하세요.");
                return;
            }

            // IHealthEventProvider 찾기 (인터페이스 사용)
            playerHealthSystem = player.GetComponent<IHealthEventProvider>();
            if (playerHealthSystem != null && healthBar != null)
            {
                // 초기 체력 설정 (Reflection 사용)
                var healthBarType = healthBar.GetType();
                var initializeMethod = healthBarType.GetMethod("Initialize");
                if (initializeMethod != null)
                {
                    initializeMethod.Invoke(healthBar, new object[] {
                        playerHealthSystem.CurrentHealth,
                        playerHealthSystem.MaxHealth
                    });
                }

                // 체력 변경 이벤트 구독
                playerHealthSystem.OnHealthChanged += OnPlayerHealthChanged;
                Debug.Log("[GameplayHUDPanel] 플레이어 IHealthEventProvider 연결 완료");
            }
        }

        /// <summary>
        /// 플레이어 체력 변경 이벤트 핸들러
        /// </summary>
        private void OnPlayerHealthChanged(float current, float max)
        {
            if (healthBar != null)
            {
                // Reflection으로 UpdateHealth 호출
                var healthBarType = healthBar.GetType();
                var updateHealthMethod = healthBarType.GetMethod("UpdateHealth");
                if (updateHealthMethod != null)
                {
                    updateHealthMethod.Invoke(healthBar, new object[] { current, max });
                }
            }
        }

        /// <summary>
        /// 일시정지 버튼 클릭
        /// </summary>
        private void OnPauseButtonClicked()
        {
            Debug.Log("[GameplayHUDPanel] 일시정지 버튼 클릭");

            // GameFlowManager 직접 접근
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.PauseGame();
            }
            else
            {
                Debug.LogError("[GameplayHUDPanel] GameFlowManager가 없습니다!");
            }
        }

        #region 공개 API

        /// <summary>
        /// 콤보 업데이트
        /// </summary>
        public void UpdateCombo(int combo)
        {
            currentCombo = combo;

            if (comboText != null)
            {
                if (combo > 1)
                {
                    comboText.text = $"{combo} COMBO!";
                    comboText.gameObject.SetActive(true);
                }
                else
                {
                    comboText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 적 카운트 업데이트
        /// </summary>
        public void UpdateEnemyCount(int count)
        {
            enemyCount = count;

            if (enemyCountText != null)
            {
                enemyCountText.text = $"적: {count}";
            }
        }

        /// <summary>
        /// 점수 업데이트
        /// </summary>
        public void UpdateScore(int newScore)
        {
            score = newScore;

            if (scoreText != null)
            {
                scoreText.text = $"점수: {score}";
            }
        }

        /// <summary>
        /// 점수 추가
        /// </summary>
        public void AddScore(int amount)
        {
            UpdateScore(score + amount);
        }

        #endregion

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (playerHealthSystem != null)
            {
                playerHealthSystem.OnHealthChanged -= OnPlayerHealthChanged;
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
            }

            // Panel 이벤트 구독 해제
            OnOpened -= OnPanelOpened;
            OnClosed -= OnPanelClosed;
        }
    }
}
