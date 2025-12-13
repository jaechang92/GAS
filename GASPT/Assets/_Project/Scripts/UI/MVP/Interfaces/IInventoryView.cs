using System;
using System.Collections.Generic;
using GASPT.Data;
using GASPT.Core.Enums;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// Inventory View 인터페이스
    /// Presenter가 View를 제어하기 위한 계약
    /// </summary>
    public interface IInventoryView
    {
        // ====== View → Presenter 이벤트 ======

        /// <summary>
        /// 인벤토리 열기 요청 (I키 등)
        /// </summary>
        event Action OnOpenRequested;

        /// <summary>
        /// 인벤토리 닫기 요청 (닫기 버튼 등)
        /// </summary>
        event Action OnCloseRequested;

        /// <summary>
        /// 아이템 장착 요청
        /// </summary>
        event Action<Item> OnItemEquipRequested;

        /// <summary>
        /// 장비 슬롯 해제 요청
        /// </summary>
        event Action<EquipmentSlot> OnEquipmentSlotUnequipRequested;


        // ====== Presenter → View 명령 ======

        /// <summary>
        /// UI가 현재 표시 중인지 여부
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// UI 표시
        /// </summary>
        void ShowUI();

        /// <summary>
        /// UI 숨김
        /// </summary>
        void HideUI();

        /// <summary>
        /// 아이템 목록 표시
        /// </summary>
        /// <param name="items">표시할 아이템 ViewModel 목록</param>
        void DisplayItems(List<ItemViewModel> items);

        /// <summary>
        /// 장비 슬롯 표시
        /// </summary>
        /// <param name="equipment">표시할 장비 ViewModel</param>
        void DisplayEquipment(EquipmentViewModel equipment);

        /// <summary>
        /// 에러 메시지 표시
        /// </summary>
        /// <param name="message">에러 메시지</param>
        void ShowError(string message);

        /// <summary>
        /// 성공 메시지 표시
        /// </summary>
        /// <param name="message">성공 메시지</param>
        void ShowSuccess(string message);
    }
}
