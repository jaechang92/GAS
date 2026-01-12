using UnityEngine;
using GASPT.Data;

namespace GASPT.Inventory
{
    /// <summary>
    /// 인벤토리 매니저 - 아이템 추가/제거/이동/교환 작업
    /// </summary>
    public partial class InventoryManager
    {
        // ====== 아이템 추가 ======

        /// <summary>
        /// ItemData로부터 아이템 추가
        /// </summary>
        /// <param name="itemData">아이템 데이터</param>
        /// <param name="quantity">수량</param>
        /// <returns>true = 추가 성공</returns>
        public bool AddItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0)
            {
                Debug.LogWarning("[InventoryManager] AddItem: itemData가 null이거나 수량이 0 이하입니다.");
                return false;
            }

            // ItemInstance 생성
            ItemInstance newInstance = ItemInstance.CreateFromData(itemData);
            if (newInstance == null)
                return false;

            return AddItemInstance(newInstance, quantity);
        }

        /// <summary>
        /// ItemInstance 추가
        /// </summary>
        /// <param name="itemInstance">아이템 인스턴스</param>
        /// <param name="quantity">수량</param>
        /// <returns>true = 추가 성공</returns>
        public bool AddItemInstance(ItemInstance itemInstance, int quantity = 1)
        {
            if (itemInstance == null || !itemInstance.IsValid)
            {
                Debug.LogWarning("[InventoryManager] AddItemInstance: 유효하지 않은 아이템 인스턴스입니다.");
                return false;
            }

            if (quantity <= 0)
                return false;

            int remaining = quantity;
            ItemData itemData = itemInstance.cachedItemData;

            // 스택 가능 아이템인 경우 기존 슬롯에 먼저 추가
            if (itemData.stackable)
            {
                foreach (var slot in slots)
                {
                    if (slot.CanStackWith(itemInstance) && !slot.IsFull)
                    {
                        int added = slot.AddQuantity(remaining);
                        remaining -= added;

                        if (added > 0)
                        {
                            OnSlotChanged?.Invoke(slot.SlotIndex);
                            OnItemAdded?.Invoke(slot.SlotIndex, itemInstance, added);
                        }

                        if (remaining <= 0)
                            break;
                    }
                }
            }

            // 남은 수량은 빈 슬롯에 추가
            while (remaining > 0)
            {
                int emptyIndex = FindEmptySlotIndex();

                if (emptyIndex < 0)
                {
                    // 인벤토리 가득 참
                    OnInventoryFull?.Invoke();
                    Debug.LogWarning($"[InventoryManager] 인벤토리 가득 참. 남은 수량: {remaining}");
                    return remaining < quantity; // 일부라도 추가했으면 true
                }

                // 새 인스턴스 생성 (스택 가능 아이템 외에는 각각 고유 인스턴스)
                ItemInstance slotInstance;
                if (itemData.stackable)
                {
                    slotInstance = itemInstance;
                }
                else
                {
                    // 비스택 아이템은 첫 번째만 기존 인스턴스 사용, 나머지는 새로 생성
                    slotInstance = remaining == quantity ? itemInstance : ItemInstance.CreateFromData(itemData);
                }

                int toAdd = itemData.stackable ? Mathf.Min(remaining, itemData.maxStack) : 1;

                slots[emptyIndex].SetItem(slotInstance, toAdd);
                remaining -= toAdd;

                OnSlotChanged?.Invoke(emptyIndex);
                OnItemAdded?.Invoke(emptyIndex, slotInstance, toAdd);
            }

            Debug.Log($"[InventoryManager] 아이템 추가: {itemData.itemName} x{quantity}");
            return true;
        }


        // ====== 아이템 제거 ======

        /// <summary>
        /// 슬롯 인덱스로 아이템 제거
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <param name="quantity">제거 수량</param>
        /// <returns>제거된 아이템 인스턴스 (없으면 null)</returns>
        public ItemInstance RemoveItem(int slotIndex, int quantity = 1)
        {
            if (!IsValidSlotIndex(slotIndex))
                return null;

            InventorySlot slot = slots[slotIndex];

            if (slot.IsEmpty)
                return null;

            ItemInstance removedItem = slot.Item;
            int actualRemoved = slot.RemoveQuantity(quantity);

            if (actualRemoved > 0)
            {
                OnSlotChanged?.Invoke(slotIndex);
                OnItemRemoved?.Invoke(slotIndex, removedItem, actualRemoved);

                Debug.Log($"[InventoryManager] 아이템 제거: {removedItem?.cachedItemData?.itemName} x{actualRemoved}");
            }

            return removedItem;
        }

        /// <summary>
        /// ItemInstance로 아이템 제거
        /// </summary>
        /// <param name="itemInstance">제거할 아이템 인스턴스</param>
        /// <param name="quantity">제거 수량</param>
        /// <returns>true = 제거 성공</returns>
        public bool RemoveItemInstance(ItemInstance itemInstance, int quantity = 1)
        {
            if (itemInstance == null)
                return false;

            int slotIndex = FindSlotIndex(itemInstance);

            if (slotIndex < 0)
            {
                Debug.LogWarning("[InventoryManager] RemoveItemInstance: 아이템을 찾을 수 없습니다.");
                return false;
            }

            return RemoveItem(slotIndex, quantity) != null;
        }

        /// <summary>
        /// ItemData로 아이템 제거 (첫 번째 발견된 것)
        /// </summary>
        /// <param name="itemData">아이템 데이터</param>
        /// <param name="quantity">제거 수량</param>
        /// <returns>true = 제거 성공</returns>
        public bool RemoveItemByData(ItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0)
                return false;

            int remaining = quantity;

            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                InventorySlot slot = slots[i];

                if (!slot.IsEmpty && slot.ItemData == itemData)
                {
                    int toRemove = Mathf.Min(remaining, slot.Quantity);
                    RemoveItem(i, toRemove);
                    remaining -= toRemove;
                }
            }

            return remaining < quantity;
        }


        // ====== 슬롯 이동/교환 ======

        /// <summary>
        /// 아이템 이동 (슬롯 간)
        /// </summary>
        /// <param name="fromIndex">원본 슬롯</param>
        /// <param name="toIndex">대상 슬롯</param>
        /// <returns>true = 이동 성공</returns>
        public bool MoveItem(int fromIndex, int toIndex)
        {
            if (!IsValidSlotIndex(fromIndex) || !IsValidSlotIndex(toIndex))
                return false;

            if (fromIndex == toIndex)
                return true;

            InventorySlot fromSlot = slots[fromIndex];
            InventorySlot toSlot = slots[toIndex];

            if (fromSlot.IsEmpty)
                return false;

            // 대상 슬롯이 비어있으면 단순 이동
            if (toSlot.IsEmpty)
            {
                toSlot.SetItem(fromSlot.Item, fromSlot.Quantity);
                fromSlot.Clear();

                OnSlotChanged?.Invoke(fromIndex);
                OnSlotChanged?.Invoke(toIndex);

                return true;
            }

            // 스택 가능하면 병합 시도
            if (fromSlot.CanStackWith(toSlot) && !toSlot.IsFull)
            {
                int added = toSlot.AddQuantity(fromSlot.Quantity);
                fromSlot.RemoveQuantity(added);

                OnSlotChanged?.Invoke(fromIndex);
                OnSlotChanged?.Invoke(toIndex);

                return true;
            }

            // 그 외에는 교환
            return SwapItems(fromIndex, toIndex);
        }

        /// <summary>
        /// 아이템 교환 (슬롯 간)
        /// </summary>
        /// <param name="indexA">슬롯 A</param>
        /// <param name="indexB">슬롯 B</param>
        /// <returns>true = 교환 성공</returns>
        public bool SwapItems(int indexA, int indexB)
        {
            if (!IsValidSlotIndex(indexA) || !IsValidSlotIndex(indexB))
                return false;

            if (indexA == indexB)
                return true;

            InventorySlot slotA = slots[indexA];
            InventorySlot slotB = slots[indexB];

            // 임시 저장
            ItemInstance tempItem = slotA.Item;
            int tempQuantity = slotA.Quantity;

            // 교환
            slotA.SetItem(slotB.Item, slotB.Quantity);
            slotB.SetItem(tempItem, tempQuantity);

            OnSlotChanged?.Invoke(indexA);
            OnSlotChanged?.Invoke(indexB);

            return true;
        }


        // ====== 용량 확장 ======

        /// <summary>
        /// 인벤토리 용량 확장
        /// </summary>
        /// <param name="additionalSlots">추가할 슬롯 수</param>
        /// <returns>true = 확장 성공</returns>
        public bool ExpandCapacity(int additionalSlots)
        {
            if (additionalSlots <= 0)
                return false;

            int newCapacity = capacity + additionalSlots;

            if (newCapacity > InventoryConstants.MaxCapacity)
            {
                Debug.LogWarning($"[InventoryManager] 최대 용량 초과. 현재: {capacity}, 최대: {InventoryConstants.MaxCapacity}");
                return false;
            }

            for (int i = capacity; i < newCapacity; i++)
            {
                slots.Add(new InventorySlot(i));
            }

            capacity = newCapacity;
            Debug.Log($"[InventoryManager] 용량 확장: {capacity - additionalSlots} → {capacity}");

            return true;
        }
    }
}
