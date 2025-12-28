using System;
using UnityEngine;
using GASPT.Data;

namespace GASPT.Inventory
{
    /// <summary>
    /// 인벤토리 슬롯 클래스
    /// 단일 아이템 인스턴스와 수량을 관리
    /// </summary>
    [Serializable]
    public class InventorySlot
    {
        // ====== 데이터 ======

        /// <summary>
        /// 슬롯에 담긴 아이템 인스턴스
        /// </summary>
        [SerializeField]
        private ItemInstance item;

        /// <summary>
        /// 아이템 수량 (스택 가능 아이템용)
        /// </summary>
        [SerializeField]
        private int quantity;

        /// <summary>
        /// 슬롯 인덱스 (인벤토리 내 위치)
        /// </summary>
        [SerializeField]
        private int slotIndex;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 슬롯에 담긴 아이템 인스턴스 (읽기 전용)
        /// </summary>
        public ItemInstance Item => item;

        /// <summary>
        /// 아이템 수량 (읽기 전용)
        /// </summary>
        public int Quantity => quantity;

        /// <summary>
        /// 슬롯 인덱스 (읽기 전용)
        /// </summary>
        public int SlotIndex => slotIndex;

        /// <summary>
        /// 슬롯이 비어있는지 여부
        /// </summary>
        public bool IsEmpty => item == null || quantity <= 0;

        /// <summary>
        /// 슬롯이 가득 찼는지 여부 (스택 최대)
        /// </summary>
        public bool IsFull
        {
            get
            {
                if (item == null || !item.IsValid)
                    return false;

                // 스택 불가 아이템은 1개만 보유 가능
                if (!item.cachedItemData.stackable)
                    return quantity >= 1;

                // 스택 가능 아이템은 maxStack 확인
                return quantity >= item.cachedItemData.maxStack;
            }
        }

        /// <summary>
        /// 추가 가능한 수량
        /// </summary>
        public int AvailableSpace
        {
            get
            {
                if (item == null || !item.IsValid)
                    return 0;

                if (!item.cachedItemData.stackable)
                    return quantity >= 1 ? 0 : 1;

                return item.cachedItemData.maxStack - quantity;
            }
        }

        /// <summary>
        /// ItemData 캐시 (편의 프로퍼티)
        /// </summary>
        public ItemData ItemData => item?.cachedItemData;


        // ====== 생성자 ======

        /// <summary>
        /// 기본 생성자 (빈 슬롯)
        /// </summary>
        public InventorySlot()
        {
            item = null;
            quantity = 0;
            slotIndex = -1;
        }

        /// <summary>
        /// 인덱스 지정 생성자
        /// </summary>
        /// <param name="index">슬롯 인덱스</param>
        public InventorySlot(int index)
        {
            item = null;
            quantity = 0;
            slotIndex = index;
        }

        /// <summary>
        /// 아이템 포함 생성자
        /// </summary>
        /// <param name="index">슬롯 인덱스</param>
        /// <param name="itemInstance">아이템 인스턴스</param>
        /// <param name="count">수량</param>
        public InventorySlot(int index, ItemInstance itemInstance, int count = 1)
        {
            slotIndex = index;
            item = itemInstance;
            quantity = Mathf.Max(0, count);
        }


        // ====== 슬롯 조작 ======

        /// <summary>
        /// 슬롯 인덱스 설정
        /// </summary>
        /// <param name="index">새 인덱스</param>
        public void SetSlotIndex(int index)
        {
            slotIndex = index;
        }

        /// <summary>
        /// 아이템 설정 (기존 아이템 교체)
        /// </summary>
        /// <param name="itemInstance">새 아이템 인스턴스</param>
        /// <param name="count">수량</param>
        public void SetItem(ItemInstance itemInstance, int count = 1)
        {
            item = itemInstance;
            quantity = Mathf.Max(0, count);
        }

        /// <summary>
        /// 슬롯 비우기
        /// </summary>
        public void Clear()
        {
            item = null;
            quantity = 0;
        }


        // ====== 수량 조작 ======

        /// <summary>
        /// 수량 추가
        /// </summary>
        /// <param name="amount">추가할 수량</param>
        /// <returns>실제 추가된 수량</returns>
        public int AddQuantity(int amount)
        {
            if (item == null || !item.IsValid || amount <= 0)
                return 0;

            int maxAdd = AvailableSpace;
            int actualAdd = Mathf.Min(amount, maxAdd);

            quantity += actualAdd;

            return actualAdd;
        }

        /// <summary>
        /// 수량 감소
        /// </summary>
        /// <param name="amount">감소할 수량</param>
        /// <returns>실제 감소된 수량</returns>
        public int RemoveQuantity(int amount)
        {
            if (item == null || amount <= 0)
                return 0;

            int actualRemove = Mathf.Min(amount, quantity);
            quantity -= actualRemove;

            // 수량이 0이 되면 슬롯 비우기
            if (quantity <= 0)
            {
                Clear();
            }

            return actualRemove;
        }

        /// <summary>
        /// 수량 직접 설정
        /// </summary>
        /// <param name="newQuantity">새 수량</param>
        public void SetQuantity(int newQuantity)
        {
            quantity = Mathf.Max(0, newQuantity);

            if (quantity <= 0)
            {
                Clear();
            }
        }


        // ====== 스택 병합 ======

        /// <summary>
        /// 같은 아이템인지 확인 (스택 병합용)
        /// </summary>
        /// <param name="otherItem">비교할 아이템 인스턴스</param>
        /// <returns>true = 같은 아이템 (병합 가능)</returns>
        public bool CanStackWith(ItemInstance otherItem)
        {
            if (item == null || otherItem == null)
                return false;

            if (!item.IsValid || !otherItem.IsValid)
                return false;

            // 스택 불가 아이템
            if (!item.cachedItemData.stackable)
                return false;

            // 같은 ItemData인지 확인
            return item.cachedItemData == otherItem.cachedItemData;
        }

        /// <summary>
        /// 다른 슬롯과 스택 병합 가능 여부
        /// </summary>
        /// <param name="otherSlot">다른 슬롯</param>
        /// <returns>true = 병합 가능</returns>
        public bool CanStackWith(InventorySlot otherSlot)
        {
            if (otherSlot == null || otherSlot.IsEmpty)
                return false;

            return CanStackWith(otherSlot.item);
        }


        // ====== 복사 ======

        /// <summary>
        /// 슬롯 복사본 생성
        /// </summary>
        /// <returns>복사된 슬롯</returns>
        public InventorySlot Clone()
        {
            return new InventorySlot(slotIndex, item, quantity);
        }


        // ====== 디버그 ======

        /// <summary>
        /// 디버그용 문자열 출력
        /// </summary>
        public override string ToString()
        {
            if (IsEmpty)
            {
                return $"[Slot {slotIndex}] Empty";
            }

            string itemName = item?.cachedItemData?.itemName ?? "Unknown";
            return $"[Slot {slotIndex}] {itemName} x{quantity}";
        }
    }
}
