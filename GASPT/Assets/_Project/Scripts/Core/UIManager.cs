using UnityEngine;
using GASPT.UI;
using GASPT.UI.MVP;

namespace GASPT.Core
{
    /// <summary>
    /// 모든 UI 중앙 관리
    /// UI 참조 보유 및 표시/숨김 제어
    ///
    /// FullScreen UI (인벤토리, 상점 등) 열면 게임 자동 일시정지
    /// Popup UI (알림, 확인창 등)는 일시정지 없이 표시
    /// </summary>
    public class UIManager : SingletonManager<UIManager>
    {
        // ====== UI 참조 ======

        [Header("UI References")]
        [Tooltip("인벤토리 UI (MVP 패턴)")]
        [SerializeField] private InventoryView inventoryView;

        [Tooltip("상점 UI (MVP 패턴)")]
        [SerializeField] private ShopView shopView;

        [Tooltip("HUD UI (HP, 골드 등)")]
        [SerializeField] private BaseUI hudUI;

        [Tooltip("일시정지 UI")]
        [SerializeField] private BaseUI pauseUI;

        [Tooltip("미니맵 UI")]
        [SerializeField] private BaseUI minimapUI;


        // ====== FullScreen UI 상태 ======

        /// <summary>
        /// 현재 열려있는 FullScreen UI 수 (중첩 처리용)
        /// </summary>
        private int activeFullScreenUICount = 0;


        // ====== 프로퍼티 (외부 접근) ======

        public InventoryView Inventory => inventoryView;
        public ShopView Shop => shopView;
        public BaseUI Hud => hudUI;
        public BaseUI Pause => pauseUI;
        public BaseUI Minimap => minimapUI;

        /// <summary>
        /// FullScreen UI가 열려있는지 여부
        /// </summary>
        public bool IsFullScreenUIOpen => activeFullScreenUICount > 0;


        // ====== 초기화 ======

        protected override void OnAwake()
        {
            // UI 자동 찾기 (Inspector에 할당 안 했을 경우)
            if (inventoryView == null)
            {
                inventoryView = FindAnyObjectByType<InventoryView>();
                if (inventoryView != null)
                {
                    Debug.Log("[UIManager] InventoryView 자동 찾기 완료");
                }
            }

            if (shopView == null)
            {
                shopView = FindAnyObjectByType<ShopView>();
                if (shopView != null)
                {
                    Debug.Log("[UIManager] ShopView 자동 찾기 완료");
                }
            }

            // 초기 상태 설정
            HideAllUI();

            // HUD는 항상 표시 (게임 플레이 중)
            hudUI?.Show();

            Debug.Log("[UIManager] 초기화 완료");
        }


        // ====== FullScreen UI 공통 (Pause 처리) ======

        /// <summary>
        /// FullScreen UI 열림 알림 (Presenter에서 호출)
        /// Pause만 처리, View 제어는 Presenter가 직접
        /// </summary>
        public void NotifyFullScreenUIOpened()
        {
            activeFullScreenUICount++;

            // 첫 번째 FullScreen UI가 열릴 때만 Pause
            if (activeFullScreenUICount == 1)
            {
                GameManager.Instance?.Pause();
                Debug.Log("[UIManager] FullScreen UI 열림 → 게임 일시정지");
            }
        }

        /// <summary>
        /// FullScreen UI 닫힘 알림 (Presenter에서 호출)
        /// Resume만 처리, View 제어는 Presenter가 직접
        /// </summary>
        public void NotifyFullScreenUIClosed()
        {
            activeFullScreenUICount--;

            // 음수 방지
            if (activeFullScreenUICount < 0)
            {
                activeFullScreenUICount = 0;
            }

            // 마지막 FullScreen UI가 닫힐 때만 Resume
            if (activeFullScreenUICount == 0)
            {
                GameManager.Instance?.Resume();
                Debug.Log("[UIManager] 모든 FullScreen UI 닫힘 → 게임 재개");
            }
        }


        // ====== 인벤토리 UI ======

        /// <summary>
        /// 인벤토리 열기 (외부 API)
        /// MVP Presenter를 통해 열기
        /// </summary>
        public void ShowInventory()
        {
            if (inventoryView == null) return;

            // 이미 열려있으면 무시
            if (inventoryView.IsVisible) return;

            // Presenter에게 열기 요청 (Presenter가 View 제어)
            inventoryView.Open();
            Debug.Log("[UIManager] 인벤토리 열기 요청");
        }

        /// <summary>
        /// 인벤토리 닫기 (외부 API)
        /// MVP Presenter를 통해 닫기
        /// </summary>
        public void HideInventory()
        {
            if (inventoryView == null) return;

            // 이미 닫혀있으면 무시
            if (!inventoryView.IsVisible) return;

            // Presenter에게 닫기 요청 (Presenter가 View 제어)
            inventoryView.Close();
            Debug.Log("[UIManager] 인벤토리 닫기 요청");
        }

        /// <summary>
        /// 인벤토리 토글 (외부 API)
        /// </summary>
        public void ToggleInventory()
        {
            if (inventoryView == null) return;

            if (inventoryView.IsVisible)
            {
                HideInventory();
            }
            else
            {
                ShowInventory();
            }
        }


        // ====== 상점 UI ======

        /// <summary>
        /// 상점 열기 (외부 API)
        /// MVP Presenter를 통해 열기
        /// </summary>
        public void ShowShop()
        {
            if (shopView == null) return;

            // 이미 열려있으면 무시
            if (shopView.IsVisible) return;

            // Presenter에게 열기 요청 (Presenter가 View 제어)
            shopView.Open();
            Debug.Log("[UIManager] 상점 열기 요청");
        }

        /// <summary>
        /// 상점 닫기 (외부 API)
        /// MVP Presenter를 통해 닫기
        /// </summary>
        public void HideShop()
        {
            if (shopView == null) return;

            // 이미 닫혀있으면 무시
            if (!shopView.IsVisible) return;

            // Presenter에게 닫기 요청 (Presenter가 View 제어)
            shopView.Close();
            Debug.Log("[UIManager] 상점 닫기 요청");
        }

        /// <summary>
        /// 상점 토글 (외부 API)
        /// </summary>
        public void ToggleShop()
        {
            if (shopView == null) return;

            if (shopView.IsVisible)
            {
                HideShop();
            }
            else
            {
                ShowShop();
            }
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
            // FullScreen UI 닫기 (Presenter를 통해)
            if (inventoryView != null && inventoryView.IsVisible)
            {
                inventoryView.Close();
            }

            if (shopView != null && shopView.IsVisible)
            {
                shopView.Close();
            }

            // 기타 UI
            pauseUI?.Hide();
            minimapUI?.Hide();
            hudUI?.Hide();

            // 카운터 리셋 (안전 처리)
            activeFullScreenUICount = 0;

            Debug.Log("[UIManager] 모든 UI 숨김");
        }

        /// <summary>
        /// 모든 FullScreen UI만 닫기 (게임 재개)
        /// </summary>
        public void CloseAllFullScreenUI()
        {
            // Presenter를 통해 닫기
            if (inventoryView != null && inventoryView.IsVisible)
            {
                inventoryView.Close();
            }

            if (shopView != null && shopView.IsVisible)
            {
                shopView.Close();
            }

            // 카운터 리셋 (안전 처리)
            activeFullScreenUICount = 0;

            Debug.Log("[UIManager] 모든 FullScreen UI 닫힘");
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

        [ContextMenu("테스트: 상점 표시")]
        private void TestShowShop()
        {
            ShowShop();
        }

        [ContextMenu("테스트: 상점 숨김")]
        private void TestHideShop()
        {
            HideShop();
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

        [ContextMenu("테스트: 모든 FullScreen UI 닫기")]
        private void TestCloseAllFullScreenUI()
        {
            CloseAllFullScreenUI();
        }

        [ContextMenu("디버그: UI 상태 출력")]
        private void DebugLogUIState()
        {
            Debug.Log("========== UI State ==========");
            Debug.Log($"Inventory: {(inventoryView?.IsVisible ?? false)}");
            Debug.Log($"Shop: {(shopView?.IsVisible ?? false)}");
            Debug.Log($"HUD: {(hudUI?.IsVisible ?? false)}");
            Debug.Log($"Pause: {(pauseUI?.IsVisible ?? false)}");
            Debug.Log($"Minimap: {(minimapUI?.IsVisible ?? false)}");
            Debug.Log($"FullScreen UI Count: {activeFullScreenUICount}");
            Debug.Log($"Is FullScreen UI Open: {IsFullScreenUIOpen}");
            Debug.Log("==============================");
        }
    }
}
