using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GASPT.Data;
using GASPT.Save;
using GASPT.Core;
using GASPT.Core.Enums;

namespace GASPT.Inventory
{
    /// <summary>
    /// 인벤토리 매니저
    /// 슬롯 기반 인벤토리 시스템 관리
    /// </summary>
    public class InventoryManager : SingletonManager<InventoryManager>, ISaveable
    {
        // ====== 인벤토리 데이터 ======

        /// <summary>
        /// 인벤토리 슬롯 목록
        /// </summary>
        private List<InventorySlot> slots = new List<InventorySlot>();

        /// <summary>
        /// 현재 인벤토리 용량
        /// </summary>
        private int capacity;


        // ====== 이벤트 ======

        /// <summary>
        /// 아이템 추가 이벤트
        /// 매개변수: (슬롯 인덱스, 아이템 인스턴스, 수량)
        /// </summary>
        public event Action<int, ItemInstance, int> OnItemAdded;

        /// <summary>
        /// 아이템 제거 이벤트
        /// 매개변수: (슬롯 인덱스, 아이템 인스턴스, 수량)
        /// </summary>
        public event Action<int, ItemInstance, int> OnItemRemoved;

        /// <summary>
        /// 슬롯 변경 이벤트
        /// 매개변수: (슬롯 인덱스)
        /// </summary>
        public event Action<int> OnSlotChanged;

        /// <summary>
        /// 인벤토리 가득 참 이벤트
        /// </summary>
        public event Action OnInventoryFull;

        /// <summary>
        /// 인벤토리 정렬 이벤트
        /// </summary>
        public event Action OnInventorySorted;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 인벤토리 용량
        /// </summary>
        public int Capacity => capacity;

        /// <summary>
        /// 사용 중인 슬롯 수
        /// </summary>
        public int UsedSlots => slots.Count(s => !s.IsEmpty);

        /// <summary>
        /// 빈 슬롯 수
        /// </summary>
        public int EmptySlots => slots.Count(s => s.IsEmpty);

        /// <summary>
        /// 인벤토리가 가득 찼는지 여부
        /// </summary>
        public bool IsFull => EmptySlots == 0;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            InitializeSlots(InventoryConstants.DefaultCapacity);
            Debug.Log($"[InventoryManager] 초기화 완료 (용량: {capacity})");
        }


        // ====== 초기화 ======

        /// <summary>
        /// 슬롯 초기화
        /// </summary>
        /// <param name="newCapacity">초기 용량</param>
        private void InitializeSlots(int newCapacity)
        {
            capacity = Mathf.Clamp(newCapacity, InventoryConstants.MinCapacity, InventoryConstants.MaxCapacity);
            slots.Clear();

            for (int i = 0; i < capacity; i++)
            {
                slots.Add(new InventorySlot(i));
            }
        }


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


        // ====== ISaveable 구현 ======

        /// <summary>
        /// 저장 ID
        /// </summary>
        public string SaveID => "InventoryManager";

        /// <summary>
        /// 저장 데이터 반환
        /// </summary>
        object ISaveable.GetSaveData()
        {
            return GetSaveData();
        }

        /// <summary>
        /// 저장 데이터 로드
        /// </summary>
        void ISaveable.LoadFromSaveData(object data)
        {
            if (data is InventoryDataV2 invData)
            {
                LoadFromSaveData(invData);
            }
            else
            {
                Debug.LogError("[InventoryManager] LoadFromSaveData: 잘못된 데이터 타입");
            }
        }

        /// <summary>
        /// 저장 데이터 생성
        /// </summary>
        public InventoryDataV2 GetSaveData()
        {
            InventoryDataV2 data = new InventoryDataV2
            {
                capacity = this.capacity,
                slots = new List<InventorySlotData>()
            };

            foreach (var slot in slots)
            {
                if (!slot.IsEmpty)
                {
                    InventorySlotData slotData = new InventorySlotData
                    {
                        slotIndex = slot.SlotIndex,
                        itemDataPath = slot.Item?.itemDataPath ?? "",
                        instanceId = slot.Item?.instanceId ?? "",
                        quantity = slot.Quantity,
                        currentDurability = slot.Item?.currentDurability ?? -1,
                        isEquipped = slot.Item?.isEquipped ?? false,
                        acquireTimeTicks = slot.Item?.acquireTimeTicks ?? 0,
                        randomStats = new List<StatModifierData>()
                    };

                    // 랜덤 스탯 저장
                    if (slot.Item?.randomStats != null)
                    {
                        foreach (var stat in slot.Item.randomStats)
                        {
                            slotData.randomStats.Add(new StatModifierData
                            {
                                statType = stat.statType,
                                modifierType = stat.modifierType,
                                value = stat.value
                            });
                        }
                    }

                    data.slots.Add(slotData);
                }
            }

            Debug.Log($"[InventoryManager] GetSaveData: {data.slots.Count}개 슬롯 저장");
            return data;
        }

        /// <summary>
        /// 저장 데이터에서 로드
        /// </summary>
        public void LoadFromSaveData(InventoryDataV2 data)
        {
            if (data == null)
            {
                Debug.LogError("[InventoryManager] LoadFromSaveData: data가 null입니다.");
                return;
            }

            // 용량 설정
            InitializeSlots(data.capacity);

            // 슬롯 데이터 복원
            foreach (var slotData in data.slots)
            {
                if (slotData.slotIndex < 0 || slotData.slotIndex >= capacity)
                    continue;

                if (string.IsNullOrEmpty(slotData.itemDataPath))
                    continue;

                // ItemInstance 복원 (팩토리 메서드 사용)
                ItemInstance instance = ItemInstance.RestoreFromSaveData(
                    slotData.instanceId,
                    slotData.itemDataPath,
                    slotData.currentDurability,
                    slotData.isEquipped,
                    slotData.acquireTimeTicks,
                    slotData.randomStats
                );

                if (instance != null)
                {
                    slots[slotData.slotIndex].SetItem(instance, slotData.quantity);
                }
            }

            Debug.Log($"[InventoryManager] LoadFromSaveData 완료: {UsedSlots}개 슬롯 복원");
        }


        // ====== 디버그 ======

        /// <summary>
        /// 인벤토리 정보 출력
        /// </summary>
        [ContextMenu("Print Inventory")]
        public void DebugPrintInventory()
        {
            Debug.Log($"[InventoryManager] ========== 인벤토리 ({UsedSlots}/{capacity}) ==========");

            foreach (var slot in slots)
            {
                if (!slot.IsEmpty)
                {
                    Debug.Log($"[InventoryManager] {slot}");
                }
            }

            Debug.Log("[InventoryManager] =====================================");
        }

        /// <summary>
        /// 인벤토리 비우기 (테스트용)
        /// </summary>
        [ContextMenu("Clear Inventory (Test)")]
        private void DebugClearInventory()
        {
            foreach (var slot in slots)
            {
                slot.Clear();
            }
            Debug.Log("[InventoryManager] 인벤토리 비우기 완료");
        }
    }
}
