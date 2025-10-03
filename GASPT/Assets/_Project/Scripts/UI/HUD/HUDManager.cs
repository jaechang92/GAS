using UnityEngine;

namespace UI.HUD
{
    /// <summary>
    /// HUD 전체 관리 매니저
    /// 게임 중 표시되는 모든 HUD 요소를 통합 관리
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("패널 참조")]
        [SerializeField] private PlayerInfoPanel playerInfoPanel;
        [SerializeField] private ResourcePanel resourcePanel;

        [Header("설정")]
        [SerializeField] private bool showOnStart = true;

        private bool isHUDVisible = true;

        private void Start()
        {
            if (showOnStart)
            {
                ShowHUD();
            }
            else
            {
                HideHUD();
            }

            Debug.Log("[HUDManager] HUD 초기화 완료");
        }

        /// <summary>
        /// HUD 전체 표시
        /// </summary>
        public void ShowHUD()
        {
            isHUDVisible = true;
            gameObject.SetActive(true);
            Debug.Log("[HUDManager] HUD 표시");
        }

        /// <summary>
        /// HUD 전체 숨김
        /// </summary>
        public void HideHUD()
        {
            isHUDVisible = false;
            gameObject.SetActive(false);
            Debug.Log("[HUDManager] HUD 숨김");
        }

        /// <summary>
        /// HUD 표시/숨김 토글
        /// </summary>
        public void ToggleHUD()
        {
            if (isHUDVisible)
            {
                HideHUD();
            }
            else
            {
                ShowHUD();
            }
        }

        /// <summary>
        /// PlayerInfoPanel 가져오기
        /// </summary>
        public PlayerInfoPanel GetPlayerInfoPanel()
        {
            return playerInfoPanel;
        }

        /// <summary>
        /// ResourcePanel 가져오기
        /// </summary>
        public ResourcePanel GetResourcePanel()
        {
            return resourcePanel;
        }

        /// <summary>
        /// HUD가 표시 중인지 확인
        /// </summary>
        public bool IsVisible()
        {
            return isHUDVisible;
        }

        #region 편의 메서드

        /// <summary>
        /// 특정 슬롯에 아이템 설정
        /// </summary>
        public void SetItemSlot(int slotIndex, Sprite icon, int count = 1)
        {
            if (playerInfoPanel != null)
            {
                playerInfoPanel.SetItemSlot(slotIndex, icon, count);
            }
        }

        /// <summary>
        /// 슬롯 쿨다운 시작
        /// </summary>
        public void StartSlotCooldown(int slotIndex, float duration)
        {
            if (playerInfoPanel != null)
            {
                playerInfoPanel.StartSlotCooldown(slotIndex, duration);
            }
        }

        /// <summary>
        /// 캐릭터 아이콘 변경
        /// </summary>
        public void SetCharacterIcon(Sprite icon)
        {
            if (playerInfoPanel != null)
            {
                playerInfoPanel.SetCharacterIcon(icon);
            }
        }

        #endregion
    }
}
