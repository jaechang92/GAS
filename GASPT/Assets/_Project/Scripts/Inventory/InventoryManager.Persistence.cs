using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Save;

namespace GASPT.Inventory
{
    /// <summary>
    /// 인벤토리 매니저 - ISaveable 구현 및 디버그 기능
    /// </summary>
    public partial class InventoryManager
    {
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
