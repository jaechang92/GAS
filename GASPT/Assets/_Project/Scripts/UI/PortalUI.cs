using UnityEngine;
using UnityEngine.UI;

namespace GASPT.UI
{
    /// <summary>
    /// 포탈 상호작용 UI
    /// 플레이어가 포탈 근처에 있을 때 "E키로 다음 방 이동" 메시지 표시
    /// </summary>
    public class PortalUI : BaseUI
    {
        // ====== UI 요소 ======

        [Header("Portal UI 요소")]
        [Tooltip("안내 텍스트")]
        [SerializeField] private Text messageText;

        [Tooltip("기본 메시지")]
        [SerializeField] private string defaultMessage = "E 키를 눌러 다음 방으로 이동";


        // ====== Unity 생명주기 ======

        protected override void Awake()
        {
            // BaseUI Awake 호출 (Panel 자동 숨김)
            base.Awake();

            // 기본 메시지 설정
            if (messageText != null)
            {
                messageText.text = defaultMessage;
            }
        }


        // ====== UI 기능 ======

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
    }
}
