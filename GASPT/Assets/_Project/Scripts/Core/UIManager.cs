using UnityEngine;
using GASPT.UI;
using GASPT.UI.MVP;
using GASPT.UI.MVP.Views;
using GASPT.UI.Meta;

namespace GASPT.Core
{
    /// <summary>
    /// 모든 UI 중앙 관리
    /// UI 참조 보유 및 표시/숨김 제어
    ///
    /// FullScreen UI (인벤토리, 상점 등) 열면 게임 자동 일시정지
    /// Popup UI (알림, 확인창 등)는 일시정지 없이 표시
    /// </summary>
    public partial class UIManager : SingletonManager<UIManager>
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

        [Tooltip("런 결과 UI (MVP 패턴)")]
        [SerializeField] private RunResultView runResultView;

        [Tooltip("메타 재화 HUD (Bone/Soul 표시)")]
        [SerializeField] private MetaHUDView metaHudView;


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
        public RunResultView RunResult => runResultView;
        public MetaHUDView MetaHud => metaHudView;

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
            }

            if (shopView == null)
            {
                shopView = FindAnyObjectByType<ShopView>();
            }

            if (runResultView == null)
            {
                runResultView = FindAnyObjectByType<RunResultView>();
            }

            if (metaHudView == null)
            {
                metaHudView = FindAnyObjectByType<MetaHUDView>();
            }

            // 초기 상태 설정
            HideAllUI();

            // HUD는 항상 표시 (게임 플레이 중)
            hudUI?.Show();
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
        }

        public void HidePause()
        {
            pauseUI?.Hide();
            GameManager.Instance?.Resume();
        }


        // ====== HUD UI ======

        public void ShowHud()
        {
            hudUI?.Show();
        }

        public void HideHud()
        {
            hudUI?.Hide();
        }


        // ====== 미니맵 UI ======

        public void ShowMinimap()
        {
            minimapUI?.Show();
        }

        public void HideMinimap()
        {
            minimapUI?.Hide();
        }


        // ====== 런 결과 UI ======

        /// <summary>
        /// 런 결과 표시 (외부 API)
        /// GameOverState, DungeonClearedState에서 호출
        /// </summary>
        /// <param name="cleared">클리어 여부</param>
        /// <param name="stage">도달 스테이지</param>
        /// <param name="rooms">클리어한 방 수</param>
        /// <param name="enemies">처치한 적 수</param>
        /// <param name="time">플레이 시간 (초)</param>
        /// <param name="gold">획득 골드</param>
        /// <param name="bone">획득 Bone</param>
        /// <param name="soul">획득 Soul</param>
        public void ShowRunResult(bool cleared, int stage, int rooms, int enemies, float time, int gold, int bone, int soul)
        {
            if (runResultView == null)
            {
                Debug.LogWarning("[UIManager] RunResultView가 없습니다.");
                return;
            }

            runResultView.ShowRunResult(cleared, stage, rooms, enemies, time, gold, bone, soul);
        }

        /// <summary>
        /// 런 결과 UI 숨기기
        /// </summary>
        public void HideRunResult()
        {
            if (runResultView != null && runResultView.IsVisible)
            {
                runResultView.HideUI();
            }
        }

        /// <summary>
        /// 런 결과 UI에 로비 복귀 콜백 설정
        /// </summary>
        public void SetRunResultReturnCallback(System.Action callback)
        {
            runResultView?.SetReturnCallback(callback);
        }

        /// <summary>
        /// 런 결과 UI에 재시작 콜백 설정
        /// </summary>
        public void SetRunResultRestartCallback(System.Action callback)
        {
            runResultView?.SetRestartCallback(callback);
        }

        /// <summary>
        /// 런 결과 UI가 표시 중인지 확인
        /// </summary>
        public bool IsRunResultVisible => runResultView != null && runResultView.IsVisible;


        // ====== 메타 재화 HUD ======

        /// <summary>
        /// 메타 재화 HUD 표시
        /// </summary>
        public void ShowMetaHud()
        {
            metaHudView?.Show();
        }

        /// <summary>
        /// 메타 재화 HUD 숨기기
        /// </summary>
        public void HideMetaHud()
        {
            metaHudView?.Hide();
        }

        /// <summary>
        /// 메타 재화 HUD 임시 재화 모드 (런 진행 중)
        /// </summary>
        public void ShowMetaHudTempMode()
        {
            if (metaHudView != null)
            {
                metaHudView.ShowTempCurrency();
                metaHudView.Show();
            }
        }

        /// <summary>
        /// 메타 재화 HUD 영구 재화 모드 (로비)
        /// </summary>
        public void ShowMetaHudPermanentMode()
        {
            if (metaHudView != null)
            {
                metaHudView.ShowPermanentCurrency();
                metaHudView.Show();
            }
        }

        /// <summary>
        /// 메타 재화 HUD가 표시 중인지 확인
        /// </summary>
        public bool IsMetaHudVisible => metaHudView != null && metaHudView.IsVisible;
    }
}
