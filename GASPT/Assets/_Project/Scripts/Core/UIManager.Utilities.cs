using UnityEngine;

namespace GASPT.Core
{
    /// <summary>
    /// UIManager - 유틸리티 및 테스트 기능
    /// 전체 UI 제어, ContextMenu 테스트 메서드
    /// </summary>
    public partial class UIManager
    {
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

            // 런 결과 UI
            if (runResultView != null && runResultView.IsVisible)
            {
                runResultView.HideUI();
            }

            // 기타 UI
            pauseUI?.Hide();
            minimapUI?.Hide();
            hudUI?.Hide();
            metaHudView?.Hide();

            // 카운터 리셋 (안전 처리)
            activeFullScreenUICount = 0;
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

            if (runResultView != null && runResultView.IsVisible)
            {
                runResultView.HideUI();
            }

            // 카운터 리셋 (안전 처리)
            activeFullScreenUICount = 0;
        }

        /// <summary>
        /// 게임플레이 UI 표시 (HUD, Minimap, MetaHUD)
        /// </summary>
        public void ShowGameplayUI()
        {
            HideAllUI();
            hudUI?.Show();
            minimapUI?.Show();
            ShowMetaHudTempMode();
        }

        /// <summary>
        /// 메뉴 UI 표시 (HUD 숨김)
        /// </summary>
        public void ShowMenuUI()
        {
            HideAllUI();
        }


        // ====== ContextMenu (테스트용) ======

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

        [ContextMenu("테스트: MetaHUD 표시 (임시 재화)")]
        private void TestShowMetaHudTemp()
        {
            ShowMetaHudTempMode();
        }

        [ContextMenu("테스트: MetaHUD 표시 (영구 재화)")]
        private void TestShowMetaHudPermanent()
        {
            ShowMetaHudPermanentMode();
        }

        [ContextMenu("테스트: MetaHUD 숨기기")]
        private void TestHideMetaHud()
        {
            HideMetaHud();
        }


        // ====== 디버그 ======

        [ContextMenu("디버그: UI 상태 출력")]
        private void DebugLogUIState()
        {
            Debug.Log("========== UI State ==========");
            Debug.Log($"Inventory: {(inventoryView?.IsVisible ?? false)}");
            Debug.Log($"Shop: {(shopView?.IsVisible ?? false)}");
            Debug.Log($"RunResult: {(runResultView?.IsVisible ?? false)}");
            Debug.Log($"MetaHUD: {(metaHudView?.IsVisible ?? false)}");
            Debug.Log($"HUD: {(hudUI?.IsVisible ?? false)}");
            Debug.Log($"Pause: {(pauseUI?.IsVisible ?? false)}");
            Debug.Log($"Minimap: {(minimapUI?.IsVisible ?? false)}");
            Debug.Log($"FullScreen UI Count: {activeFullScreenUICount}");
            Debug.Log($"Is FullScreen UI Open: {IsFullScreenUIOpen}");
            Debug.Log("==============================");
        }
    }
}
