using UnityEngine;
using UnityEngine.UI;
using Core.Managers;

namespace UI.HUD
{
    /// <summary>
    /// 리소스 패널 (오른쪽 하단)
    /// 골드, 다이아 표시 및 설정 버튼 관리
    /// </summary>
    public class ResourcePanel : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Text goldText;
        [SerializeField] private Text diamondText;
        [SerializeField] private Button settingsButton;

        [Header("설정")]
        [SerializeField] private bool autoConnectToGameManager = true;
        [SerializeField] private string goldFormat = "{0}";
        [SerializeField] private string diamondFormat = "{0}";

        private GameManager gameManager;

        private void Start()
        {
            // 설정 버튼 이벤트 연결
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }

            // GameManager 연결
            if (autoConnectToGameManager)
            {
                ConnectToGameManager();
            }
        }

        private void OnDestroy()
        {
            // 이벤트 해제
            DisconnectFromGameManager();

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            }
        }

        /// <summary>
        /// GameManager 연결
        /// </summary>
        private void ConnectToGameManager()
        {
            if (GameManager.HasInstance)
            {
                gameManager = GameManager.Instance;

                // 이벤트 구독
                gameManager.OnGoldChanged += OnGoldChanged;
                gameManager.OnDiamondChanged += OnDiamondChanged;

                // 현재 값으로 초기화
                UpdateGoldDisplay(gameManager.CurrentGold);
                UpdateDiamondDisplay(gameManager.CurrentDiamond);

                Debug.Log("[ResourcePanel] GameManager 연결 완료");
            }
            else
            {
                Debug.LogWarning("[ResourcePanel] GameManager를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// GameManager 연결 해제
        /// </summary>
        private void DisconnectFromGameManager()
        {
            if (gameManager != null)
            {
                gameManager.OnGoldChanged -= OnGoldChanged;
                gameManager.OnDiamondChanged -= OnDiamondChanged;
                gameManager = null;
            }
        }

        /// <summary>
        /// 골드 변경 이벤트 핸들러
        /// </summary>
        private void OnGoldChanged(int newGold)
        {
            UpdateGoldDisplay(newGold);
        }

        /// <summary>
        /// 다이아 변경 이벤트 핸들러
        /// </summary>
        private void OnDiamondChanged(int newDiamond)
        {
            UpdateDiamondDisplay(newDiamond);
        }

        /// <summary>
        /// 골드 표시 업데이트
        /// </summary>
        private void UpdateGoldDisplay(int gold)
        {
            if (goldText != null)
            {
                goldText.text = string.Format(goldFormat, gold);
            }
        }

        /// <summary>
        /// 다이아 표시 업데이트
        /// </summary>
        private void UpdateDiamondDisplay(int diamond)
        {
            if (diamondText != null)
            {
                diamondText.text = string.Format(diamondFormat, diamond);
            }
        }

        /// <summary>
        /// 설정 버튼 클릭 핸들러
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            Debug.Log("[ResourcePanel] 설정 버튼 클릭");
            // TODO: 설정 메뉴 열기
            // UIManager를 통해 설정 패널 표시
        }

        /// <summary>
        /// 골드 직접 설정 (수동)
        /// </summary>
        public void SetGold(int gold)
        {
            UpdateGoldDisplay(gold);
        }

        /// <summary>
        /// 다이아 직접 설정 (수동)
        /// </summary>
        public void SetDiamond(int diamond)
        {
            UpdateDiamondDisplay(diamond);
        }
    }
}
