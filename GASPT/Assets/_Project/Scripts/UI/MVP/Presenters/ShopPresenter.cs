using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Shop;
using GASPT.Economy;
using GASPT.Core;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// Shop Presenter (MVP 패턴 - Pure C#)
    /// View와 Model(ShopSystem, CurrencySystem) 사이의 비즈니스 로직 처리
    /// Unity MonoBehaviour 의존성 없음 → 테스트 가능!
    /// </summary>
    public class ShopPresenter
    {
        // ====== 참조 ======

        private readonly IShopView view;
        private ShopSystem shopSystem;
        private CurrencySystem currencySystem;


        // ====== ViewModel 캐시 ======

        private List<ShopItemViewModel> cachedViewModels;


        // ====== 생성자 ======

        /// <summary>
        /// ShopPresenter 생성자
        /// </summary>
        /// <param name="view">Shop View 인터페이스</param>
        public ShopPresenter(IShopView view)
        {
            this.view = view ?? throw new ArgumentNullException(nameof(view));

            // View 이벤트 구독
            this.view.OnOpenRequested += HandleOpenRequested;
            this.view.OnCloseRequested += HandleCloseRequested;
            this.view.OnPurchaseRequested += HandlePurchaseRequested;

            Debug.Log("[ShopPresenter] Presenter 생성 완료");
        }


        // ====== 초기화 ======

        /// <summary>
        /// Presenter 초기화 (Model 참조 획득)
        /// View의 Start()에서 호출
        /// </summary>
        public void Initialize()
        {
            // ShopSystem 참조 (Property 패턴)
            shopSystem = ShopSystem;

            if (shopSystem == null)
            {
                Debug.LogError("[ShopPresenter] ShopSystem을 찾을 수 없습니다.");
                return;
            }

            // CurrencySystem 참조 (Property 패턴)
            currencySystem = CurrencySystem;

            if (currencySystem == null)
            {
                Debug.LogError("[ShopPresenter] CurrencySystem을 찾을 수 없습니다.");
                return;
            }

            // ShopSystem 이벤트 구독
            shopSystem.OnPurchaseSuccess += HandlePurchaseSuccess;
            shopSystem.OnPurchaseFailed += HandlePurchaseFailed;

            // CurrencySystem 이벤트 구독
            currencySystem.OnGoldChanged += HandleGoldChanged;

            Debug.Log("[ShopPresenter] Initialize 완료");
        }


        // ====== 정리 ======

        /// <summary>
        /// Presenter 정리 (이벤트 구독 해제)
        /// View의 OnDestroy()에서 호출
        /// </summary>
        public void Cleanup()
        {
            // View 이벤트 구독 해제
            view.OnOpenRequested -= HandleOpenRequested;
            view.OnCloseRequested -= HandleCloseRequested;
            view.OnPurchaseRequested -= HandlePurchaseRequested;

            // ShopSystem 이벤트 구독 해제
            if (shopSystem != null)
            {
                shopSystem.OnPurchaseSuccess -= HandlePurchaseSuccess;
                shopSystem.OnPurchaseFailed -= HandlePurchaseFailed;
            }

            // CurrencySystem 이벤트 구독 해제
            if (currencySystem != null)
            {
                currencySystem.OnGoldChanged -= HandleGoldChanged;
            }

            Debug.Log("[ShopPresenter] Cleanup 완료");
        }


        // ====== Property 패턴 (느슨한 결합) ======

        /// <summary>
        /// ShopSystem 참조 (자동 갱신)
        /// </summary>
        private ShopSystem ShopSystem
        {
            get
            {
                if (shopSystem == null && GASPT.Shop.ShopSystem.HasInstance)
                {
                    shopSystem = GASPT.Shop.ShopSystem.Instance;
                }
                return shopSystem;
            }
        }

        /// <summary>
        /// CurrencySystem 참조 (자동 갱신)
        /// </summary>
        private CurrencySystem CurrencySystem
        {
            get
            {
                if (currencySystem == null && GASPT.Economy.CurrencySystem.HasInstance)
                {
                    currencySystem = GASPT.Economy.CurrencySystem.Instance;
                }
                return currencySystem;
            }
        }


        // ====== View 이벤트 핸들러 (View → Presenter) ======

        /// <summary>
        /// 상점 열기 요청 처리
        /// </summary>
        private void HandleOpenRequested()
        {
            Debug.Log("[ShopPresenter] HandleOpenRequested");

            // 이미 열려있으면 무시
            if (view.IsVisible) return;

            // 초기화 확인
            if (shopSystem == null || currencySystem == null)
            {
                Debug.LogError("[ShopPresenter] HandleOpenRequested: 시스템이 초기화되지 않았습니다. Initialize()를 먼저 호출하세요.");
                return;
            }

            // 1. 상점 아이템 ViewModel 생성 및 표시
            RefreshShopItems();

            // 2. 골드 표시
            RefreshGold();

            // 3. View 표시 (Presenter가 직접 제어)
            view.ShowUI();

            // 4. 게임 일시정지 (UIManager가 있으면 Pause 처리)
            if (UIManager.HasInstance)
            {
                UIManager.Instance.NotifyFullScreenUIOpened();
            }
            else
            {
                // UIManager 없으면 직접 GameManager로 Pause
                GameManager.Instance?.Pause();
            }
        }

        /// <summary>
        /// 상점 닫기 요청 처리
        /// </summary>
        private void HandleCloseRequested()
        {
            Debug.Log("[ShopPresenter] HandleCloseRequested");

            // 이미 닫혀있으면 무시
            if (!view.IsVisible) return;

            // View 숨김 (Presenter가 직접 제어)
            view.HideUI();

            // 게임 재개 (UIManager가 있으면 Resume 처리)
            if (UIManager.HasInstance)
            {
                UIManager.Instance.NotifyFullScreenUIClosed();
            }
            else
            {
                // UIManager 없으면 직접 GameManager로 Resume
                GameManager.Instance?.Resume();
            }
        }

        /// <summary>
        /// 아이템 구매 요청 처리
        /// </summary>
        /// <param name="itemIndex">아이템 인덱스</param>
        /// <param name="itemVM">아이템 ViewModel</param>
        private void HandlePurchaseRequested(int itemIndex, ShopItemViewModel itemVM)
        {
            if (itemVM == null)
            {
                Debug.LogError("[ShopPresenter] HandlePurchaseRequested: itemVM이 null입니다.");
                return;
            }

            if (shopSystem == null)
            {
                Debug.LogError("[ShopPresenter] HandlePurchaseRequested: ShopSystem이 null입니다.");
                return;
            }

            Debug.Log($"[ShopPresenter] HandlePurchaseRequested: {itemVM.Name} (Index: {itemIndex})");

            // ShopSystem에 구매 요청
            shopSystem.PurchaseItem(itemVM.OriginalItem, itemVM.Price);
        }


        // ====== Model 이벤트 핸들러 (Model → Presenter → View) ======

        /// <summary>
        /// 구매 성공 이벤트 핸들러
        /// </summary>
        private void HandlePurchaseSuccess(GASPT.Data.Item item, int price)
        {
            Debug.Log($"[ShopPresenter] HandlePurchaseSuccess: {item.itemName} ({price}G)");

            // View에 성공 메시지 표시
            view.ShowSuccess($"{item.itemName} 구매 완료! (-{price} 골드)");

            // 골드 갱신 (OnGoldChanged 이벤트에서 자동 처리됨)
        }

        /// <summary>
        /// 구매 실패 이벤트 핸들러
        /// </summary>
        private void HandlePurchaseFailed(GASPT.Data.Item item, int price, string reason)
        {
            Debug.Log($"[ShopPresenter] HandlePurchaseFailed: {reason}");

            // View에 에러 메시지 표시
            view.ShowError($"구매 실패: {reason}");
        }

        /// <summary>
        /// 골드 변경 이벤트 핸들러
        /// </summary>
        private void HandleGoldChanged(int oldGold, int newGold)
        {
            Debug.Log($"[ShopPresenter] HandleGoldChanged: {oldGold} → {newGold}");

            // 골드 표시 갱신
            RefreshGold();

            // 모든 아이템의 구매 가능 여부 갱신
            RefreshAffordability(newGold);
        }


        // ====== 비즈니스 로직 (Private) ======

        /// <summary>
        /// 상점 아이템 목록 갱신 (Model → ViewModel → View)
        /// </summary>
        private void RefreshShopItems()
        {
            if (shopSystem == null || currencySystem == null)
            {
                Debug.LogError("[ShopPresenter] RefreshShopItems: ShopSystem 또는 CurrencySystem이 null입니다.");
                return;
            }

            // 1. Model에서 데이터 가져오기
            var shopItems = shopSystem.GetShopItems();
            int currentGold = currencySystem.Gold;

            // 2. ViewModel 생성
            cachedViewModels = new List<ShopItemViewModel>();

            foreach (var shopItem in shopItems)
            {
                if (shopItem.item != null)
                {
                    var viewModel = new ShopItemViewModel(shopItem.item, shopItem.price, currentGold);
                    cachedViewModels.Add(viewModel);
                }
            }

            // 3. View에 표시
            view.DisplayShopItems(cachedViewModels);

            Debug.Log($"[ShopPresenter] RefreshShopItems: {cachedViewModels.Count}개 아이템 표시");
        }

        /// <summary>
        /// 골드 표시 갱신
        /// </summary>
        private void RefreshGold()
        {
            if (currencySystem == null)
            {
                Debug.LogError("[ShopPresenter] RefreshGold: CurrencySystem이 null입니다.");
                return;
            }

            // View에 골드 표시
            view.DisplayGold(currencySystem.Gold);
        }

        /// <summary>
        /// 모든 아이템의 구매 가능 여부 갱신 (골드 변경 시)
        /// </summary>
        /// <param name="currentGold">현재 골드</param>
        private void RefreshAffordability(int currentGold)
        {
            if (cachedViewModels == null)
                return;

            // 모든 ViewModel의 구매 가능 여부 갱신
            foreach (var viewModel in cachedViewModels)
            {
                viewModel.UpdateAffordability(currentGold);
            }

            // View에 다시 표시 (구매 가능 여부 변경 반영)
            view.DisplayShopItems(cachedViewModels);

            Debug.Log($"[ShopPresenter] RefreshAffordability: {currentGold}G");
        }
    }
}
