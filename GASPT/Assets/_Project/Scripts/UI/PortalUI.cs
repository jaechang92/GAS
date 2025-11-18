using UnityEngine;
using UnityEngine.UI;

namespace GASPT.UI
{
    /// <summary>
    /// 포탈 상호작용 UI
    /// 플레이어가 포탈 근처에 있을 때 "E키로 다음 방 이동" 메시지 표시
    /// </summary>
    public class PortalUI : MonoBehaviour
    {
        // ====== UI 요소 ======

        [Header("UI 요소")]
        [Tooltip("UI 패널 (활성화/비활성화)")]
        [SerializeField] private GameObject uiPanel;

        [Tooltip("안내 텍스트")]
        [SerializeField] private Text messageText;

        [Tooltip("기본 메시지")]
        [SerializeField] private string defaultMessage = "E 키를 눌러 다음 방으로 이동";


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // 초기 상태: 숨김
            Hide();

            // 기본 메시지 설정
            if (messageText != null)
            {
                messageText.text = defaultMessage;
            }
        }


        // ====== UI 표시/숨김 ======

        /// <summary>
        /// UI 표시
        /// </summary>
        public void Show()
        {
            if (uiPanel != null)
            {
                uiPanel.SetActive(true);
            }
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
        }

        /// <summary>
        /// 메시지 변경
        /// </summary>
        public void SetMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Show UI (Test)")]
        private void DebugShow()
        {
            Show();
        }

        [ContextMenu("Hide UI (Test)")]
        private void DebugHide()
        {
            Hide();
        }
    }
}
