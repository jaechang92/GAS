using UnityEngine;
using GASPT.UI;

namespace GASPT.Core
{
    /// <summary>
    /// 모든 UI 중앙 관리
    /// UI 참조 보유 및 표시/숨김 제어
    /// </summary>
    public class UIManager : SingletonManager<UIManager>
    {
        // ====== UI 참조 ======

        [Header("UI References")]
        [Tooltip("인벤토리 UI")]
        [SerializeField] private InventoryUI inventoryUI;

        [Tooltip("HUD UI (HP, 골드 등)")]
        [SerializeField] private BaseUI hudUI;

        [Tooltip("일시정지 UI")]
        [SerializeField] private BaseUI pauseUI;

        [Tooltip("미니맵 UI")]
        [SerializeField] private BaseUI minimapUI;


        // ====== 프로퍼티 (외부 접근) ======

        public InventoryUI Inventory => inventoryUI;
        public BaseUI Hud => hudUI;
        public BaseUI Pause => pauseUI;
        public BaseUI Minimap => minimapUI;


        // ====== 초기화 ======

        protected override void OnAwake()
        {
            // UI 자동 찾기 (Inspector에 할당 안 했을 경우)
            if (inventoryUI == null)
            {
                inventoryUI = FindAnyObjectByType<InventoryUI>();
                if (inventoryUI != null)
                {
                    Debug.Log("[UIManager] InventoryUI 자동 찾기 완료");
                }
            }

            // 초기 상태 설정
            HideAllUI();

            // HUD는 항상 표시 (게임 플레이 중)
            hudUI?.Show();

            Debug.Log("[UIManager] 초기화 완료");
        }


        // ====== 인벤토리 UI ======

        public void ShowInventory()
        {
            inventoryUI?.Show();
            Debug.Log("[UIManager] 인벤토리 표시");
        }

        public void HideInventory()
        {
            inventoryUI?.Hide();
            Debug.Log("[UIManager] 인벤토리 숨김");
        }

        public void ToggleInventory()
        {
            inventoryUI?.Toggle();
            Debug.Log($"[UIManager] 인벤토리 토글: {inventoryUI?.IsVisible}");
        }


        // ====== 일시정지 UI ======

        public void ShowPause()
        {
            pauseUI?.Show();
            GameManager.Instance?.Pause();
            Debug.Log("[UIManager] 일시정지 UI 표시");
        }

        public void HidePause()
        {
            pauseUI?.Hide();
            GameManager.Instance?.Resume();
            Debug.Log("[UIManager] 일시정지 UI 숨김");
        }


        // ====== HUD UI ======

        public void ShowHud()
        {
            hudUI?.Show();
            Debug.Log("[UIManager] HUD 표시");
        }

        public void HideHud()
        {
            hudUI?.Hide();
            Debug.Log("[UIManager] HUD 숨김");
        }


        // ====== 미니맵 UI ======

        public void ShowMinimap()
        {
            minimapUI?.Show();
            Debug.Log("[UIManager] 미니맵 표시");
        }

        public void HideMinimap()
        {
            minimapUI?.Hide();
            Debug.Log("[UIManager] 미니맵 숨김");
        }


        // ====== 전체 UI 제어 ======

        /// <summary>
        /// 모든 UI 숨김
        /// </summary>
        public void HideAllUI()
        {
            inventoryUI?.Hide();
            pauseUI?.Hide();
            minimapUI?.Hide();
            hudUI?.Hide();

            Debug.Log("[UIManager] 모든 UI 숨김");
        }

        /// <summary>
        /// 게임플레이 UI 표시 (HUD, Minimap)
        /// </summary>
        public void ShowGameplayUI()
        {
            HideAllUI();
            hudUI?.Show();
            minimapUI?.Show();

            Debug.Log("[UIManager] 게임플레이 UI 표시");
        }

        /// <summary>
        /// 메뉴 UI 표시 (HUD 숨김)
        /// </summary>
        public void ShowMenuUI()
        {
            HideAllUI();

            Debug.Log("[UIManager] 메뉴 UI 표시");
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("테스트: 인벤토리 표시")]
        private void TestShowInventory()
        {
            ShowInventory();
        }

        [ContextMenu("테스트: 인벤토리 숨김")]
        private void TestHideInventory()
        {
            HideInventory();
        }

        [ContextMenu("테스트: 일시정지 표시")]
        private void TestShowPause()
        {
            ShowPause();
        }

        [ContextMenu("테스트: 모든 UI 숨김")]
        private void TestHideAllUI()
        {
            HideAllUI();
        }

        [ContextMenu("디버그: UI 상태 출력")]
        private void DebugLogUIState()
        {
            Debug.Log("========== UI State ==========");
            Debug.Log($"Inventory: {(inventoryUI?.IsVisible ?? false)}");
            Debug.Log($"HUD: {(hudUI?.IsVisible ?? false)}");
            Debug.Log($"Pause: {(pauseUI?.IsVisible ?? false)}");
            Debug.Log($"Minimap: {(minimapUI?.IsVisible ?? false)}");
            Debug.Log("==============================");
        }
    }
}
