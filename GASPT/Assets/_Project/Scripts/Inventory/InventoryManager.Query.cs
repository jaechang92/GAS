using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GASPT.Data;
using GASPT.Core.Enums;

namespace GASPT.Inventory
{
    /// <summary>
    /// 인벤토리 매니저 - 조회, 필터링, 정렬, 유틸리티
    /// </summary>
    public partial class InventoryManager
    {
        // ====== 슬롯 조회 ======

        /// <summary>
        /// 슬롯 가져오기
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <returns>슬롯 (없으면 null)</returns>
        public InventorySlot GetSlot(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
                return null;

            return slots[slotIndex];
        }

        /// <summary>
        /// 모든 슬롯 가져오기 (읽기 전용)
        /// </summary>
        /// <returns>슬롯 목록 복사본</returns>
        public List<InventorySlot> GetAllSlots()
        {
            return new List<InventorySlot>(slots);
        }

        /// <summary>
        /// 비어있지 않은 슬롯만 가져오기
        /// </summary>
        /// <returns>아이템이 있는 슬롯 목록</returns>
        public List<InventorySlot> GetOccupiedSlots()
        {
            return slots.Where(s => !s.IsEmpty).ToList();
        }

        /// <summary>
        /// 카테고리별 아이템 가져오기
        /// </summary>
        /// <param name="category">아이템 카테고리</param>
        /// <returns>해당 카테고리 슬롯 목록</returns>
        public List<InventorySlot> GetItemsByCategory(ItemCategory category)
        {
            return slots.Where(s => !s.IsEmpty && s.ItemData?.category == category).ToList();
        }

        /// <summary>
        /// 등급별 아이템 가져오기
        /// </summary>
        /// <param name="rarity">아이템 등급</param>
        /// <returns>해당 등급 슬롯 목록</returns>
        public List<InventorySlot> GetItemsByRarity(ItemRarity rarity)
        {
            return slots.Where(s => !s.IsEmpty && s.ItemData?.rarity == rarity).ToList();
        }

        /// <summary>
        /// 복합 필터로 아이템 검색
        /// </summary>
        /// <param name="filter">필터 조건</param>
        /// <returns>조건에 맞는 슬롯 목록</returns>
        public List<InventorySlot> FilterItems(InventoryFilter filter)
        {
            if (filter == null)
                return GetOccupiedSlots();

            IEnumerable<InventorySlot> query = slots.Where(s => !s.IsEmpty);

            // 카테고리 필터
            if (filter.category.HasValue)
            {
                query = query.Where(s => s.ItemData?.category == filter.category.Value);
            }

            // 등급 필터 (최소 등급 이상)
            if (filter.minRarity.HasValue)
            {
                query = query.Where(s => s.ItemData?.rarity >= filter.minRarity.Value);
            }

            // 장비 슬롯 필터
            if (filter.equipSlot.HasValue)
            {
                query = query.Where(s =>
                {
                    EquipmentData equipData = s.ItemData as EquipmentData;
                    return equipData != null && equipData.equipSlot == filter.equipSlot.Value;
                });
            }

            // 이름 검색 (부분 일치)
            if (!string.IsNullOrEmpty(filter.nameContains))
            {
                string searchLower = filter.nameContains.ToLower();
                query = query.Where(s =>
                    s.ItemData?.itemName?.ToLower().Contains(searchLower) == true);
            }

            // 스택 가능 여부
            if (filter.stackableOnly.HasValue)
            {
                query = query.Where(s => s.ItemData?.stackable == filter.stackableOnly.Value);
            }

            // 장착 여부
            if (filter.equippedOnly.HasValue)
            {
                query = query.Where(s => s.Item?.isEquipped == filter.equippedOnly.Value);
            }

            return query.ToList();
        }

        /// <summary>
        /// 카테고리 및 등급 조합 필터
        /// </summary>
        /// <param name="category">카테고리 (null = 전체)</param>
        /// <param name="minRarity">최소 등급 (null = 전체)</param>
        /// <returns>필터링된 슬롯 목록</returns>
        public List<InventorySlot> FilterItems(ItemCategory? category, ItemRarity? minRarity)
        {
            return FilterItems(new InventoryFilter
            {
                category = category,
                minRarity = minRarity
            });
        }


        // ====== 정렬 ======

        /// <summary>
        /// 인벤토리 정렬
        /// </summary>
        /// <param name="sortType">정렬 타입</param>
        public void SortInventory(InventorySortType sortType)
        {
            // 비어있지 않은 슬롯만 추출
            List<InventorySlot> occupiedSlots = GetOccupiedSlots();

            // 정렬
            IOrderedEnumerable<InventorySlot> sortedSlots = sortType switch
            {
                InventorySortType.ByCategory => occupiedSlots.OrderBy(s => s.ItemData?.category),
                InventorySortType.ByRarity => occupiedSlots.OrderByDescending(s => s.ItemData?.rarity),
                InventorySortType.ByName => occupiedSlots.OrderBy(s => s.ItemData?.itemName),
                InventorySortType.ByAcquireTime => occupiedSlots.OrderByDescending(s => s.Item?.AcquireTime),
                InventorySortType.BySlot => occupiedSlots.OrderBy(s => (s.Item as ItemInstance)?.EquipmentData?.equipSlot),
                _ => occupiedSlots.OrderBy(s => s.SlotIndex)
            };

            // 슬롯 재배치
            List<InventorySlot> sortedList = sortedSlots.ToList();

            for (int i = 0; i < capacity; i++)
            {
                if (i < sortedList.Count)
                {
                    slots[i].SetItem(sortedList[i].Item, sortedList[i].Quantity);
                }
                else
                {
                    slots[i].Clear();
                }
                slots[i].SetSlotIndex(i);
            }

            OnInventorySorted?.Invoke();
            Debug.Log($"[InventoryManager] 인벤토리 정렬 완료: {sortType}");
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 빈 슬롯 인덱스 찾기
        /// </summary>
        /// <returns>빈 슬롯 인덱스 (-1 = 없음)</returns>
        public int FindEmptySlotIndex()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// ItemInstance로 슬롯 인덱스 찾기
        /// </summary>
        /// <param name="itemInstance">찾을 아이템 인스턴스</param>
        /// <returns>슬롯 인덱스 (-1 = 없음)</returns>
        public int FindSlotIndex(ItemInstance itemInstance)
        {
            if (itemInstance == null)
                return -1;

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Item?.instanceId == itemInstance.instanceId)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 유효한 슬롯 인덱스인지 확인
        /// </summary>
        /// <param name="index">슬롯 인덱스</param>
        /// <returns>true = 유효</returns>
        public bool IsValidSlotIndex(int index)
        {
            return index >= 0 && index < slots.Count;
        }

        /// <summary>
        /// 아이템 보유 여부 확인
        /// </summary>
        /// <param name="itemData">아이템 데이터</param>
        /// <param name="quantity">필요 수량</param>
        /// <returns>true = 보유 중</returns>
        public bool HasItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null)
                return false;

            int totalCount = 0;

            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.ItemData == itemData)
                {
                    totalCount += slot.Quantity;
                    if (totalCount >= quantity)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 특정 아이템 총 수량 반환
        /// </summary>
        /// <param name="itemData">아이템 데이터</param>
        /// <returns>총 수량</returns>
        public int GetItemCount(ItemData itemData)
        {
            if (itemData == null)
                return 0;

            return slots.Where(s => !s.IsEmpty && s.ItemData == itemData).Sum(s => s.Quantity);
        }
    }
}
