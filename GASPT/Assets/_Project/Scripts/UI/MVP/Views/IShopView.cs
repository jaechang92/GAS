using System;
using System.Collections.Generic;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// Shop View 인터페이스 (MVP 패턴)
    /// View는 순수 렌더링만 담당, 비즈니스 로직은 Presenter가 처리
    /// </summary>
    public interface IShopView
    {
        // ====== View → Presenter 이벤트 ======

        /// <summary>
        /// 상점 열기 요청 이벤트
        /// </summary>
        event Action OnOpenRequested;

        /// <summary>
        /// 상점 닫기 요청 이벤트
        /// </summary>
        event Action OnCloseRequested;

        /// <summary>
        /// 아이템 구매 요청 이벤트
        /// 매개변수: (아이템 인덱스, ViewModel)
        /// </summary>
        event Action<int, ShopItemViewModel> OnPurchaseRequested;


        // ====== Presenter → View 명령 ======

        /// <summary>
        /// 상점 UI 표시
        /// </summary>
        void ShowUI();

        /// <summary>
        /// 상점 UI 숨김
        /// </summary>
        void HideUI();

        /// <summary>
        /// 상점 아이템 목록 표시 (순수 렌더링)
        /// </summary>
        /// <param name="items">표시할 아이템 ViewModel 목록</param>
        void DisplayShopItems(List<ShopItemViewModel> items);

        /// <summary>
        /// 골드 표시 (순수 렌더링)
        /// </summary>
        /// <param name="gold">현재 골드</param>
        void DisplayGold(int gold);

        /// <summary>
        /// 성공 메시지 표시
        /// </summary>
        /// <param name="message">메시지 내용</param>
        void ShowSuccess(string message);

        /// <summary>
        /// 에러 메시지 표시
        /// </summary>
        /// <param name="message">에러 메시지</param>
        void ShowError(string message);
    }
}
