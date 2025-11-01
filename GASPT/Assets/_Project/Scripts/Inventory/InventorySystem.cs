using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Stats;
using Core.Enums;

namespace GASPT.Inventory
{
    /// <summary>
    /// 인벤토리 시스템 싱글톤
    /// 아이템 저장 및 PlayerStats와 통합하여 장비 관리
    /// </summary>
    public class InventorySystem : SingletonManager<InventorySystem>
    {
        // ====== 인벤토리 ======

        /// <summary>
        /// 보유한 아이템 목록
        /// </summary>
        private List<Item> items = new List<Item>();


        // ====== PlayerStats 참조 ======

        private PlayerStats playerStats;


        // ====== 이벤트 ======

        /// <summary>
        /// 아이템 추가 시 발생하는 이벤트
        /// 매개변수: (추가된 아이템)
        /// </summary>
        public event Action<Item> OnItemAdded;

        /// <summary>
        /// 아이템 제거 시 발생하는 이벤트
        /// 매개변수: (제거된 아이템)
        /// </summary>
        public event Action<Item> OnItemRemoved;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            // PlayerStats 찾기
            playerStats = FindAnyObjectByType<PlayerStats>();

            if (playerStats == null)
            {
                Debug.LogWarning("[InventorySystem] PlayerStats를 찾을 수 없습니다. Scene에 PlayerStats가 있는지 확인하세요.");
            }

            Debug.Log($"[InventorySystem] 초기화 완료");
        }


        // ====== 아이템 관리 ======

        /// <summary>
        /// 아이템 추가
        /// </summary>
        /// <param name="item">추가할 아이템</param>
        public void AddItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("[InventorySystem] AddItem(): item이 null입니다.");
                return;
            }

            items.Add(item);

            // 이벤트 발생
            OnItemAdded?.Invoke(item);

            Debug.Log($"[InventorySystem] 아이템 추가: {item.itemName} (총 {items.Count}개)");
        }

        /// <summary>
        /// 아이템 제거
        /// </summary>
        /// <param name="item">제거할 아이템</param>
        /// <returns>true: 제거 성공, false: 아이템 없음</returns>
        public bool RemoveItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("[InventorySystem] RemoveItem(): item이 null입니다.");
                return false;
            }

            bool removed = items.Remove(item);

            if (removed)
            {
                // 이벤트 발생
                OnItemRemoved?.Invoke(item);

                Debug.Log($"[InventorySystem] 아이템 제거: {item.itemName} (남은 개수: {items.Count})");
            }
            else
            {
                Debug.LogWarning($"[InventorySystem] 아이템 제거 실패: {item.itemName}을(를) 찾을 수 없습니다.");
            }

            return removed;
        }

        /// <summary>
        /// 아이템 보유 여부 확인
        /// </summary>
        /// <param name="item">확인할 아이템</param>
        /// <returns>true: 보유 중, false: 없음</returns>
        public bool HasItem(Item item)
        {
            return items.Contains(item);
        }

        /// <summary>
        /// 모든 아이템 목록 가져오기 (읽기 전용)
        /// </summary>
        /// <returns>아이템 목록 복사본</returns>
        public List<Item> GetItems()
        {
            return new List<Item>(items);
        }

        /// <summary>
        /// 인벤토리 아이템 개수
        /// </summary>
        public int ItemCount => items.Count;


        // ====== PlayerStats 통합 (장비 시스템) ======

        /// <summary>
        /// 아이템 장착 (PlayerStats 호출)
        /// </summary>
        /// <param name="item">장착할 아이템</param>
        /// <returns>true: 장착 성공, false: 장착 실패</returns>
        public bool EquipItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("[InventorySystem] EquipItem(): item이 null입니다.");
                return false;
            }

            // PlayerStats가 없으면 실패
            if (playerStats == null)
            {
                Debug.LogError("[InventorySystem] EquipItem(): PlayerStats를 찾을 수 없습니다.");
                return false;
            }

            // 인벤토리에 아이템이 없으면 실패
            if (!HasItem(item))
            {
                Debug.LogWarning($"[InventorySystem] EquipItem(): {item.itemName}을(를) 보유하고 있지 않습니다.");
                return false;
            }

            // PlayerStats에 장착 요청
            bool success = playerStats.EquipItem(item);

            if (success)
            {
                Debug.Log($"[InventorySystem] {item.itemName} 장착 완료");
            }
            else
            {
                Debug.LogWarning($"[InventorySystem] {item.itemName} 장착 실패");
            }

            return success;
        }

        /// <summary>
        /// 아이템 장착 해제 (PlayerStats 호출)
        /// </summary>
        /// <param name="slot">해제할 슬롯</param>
        /// <returns>true: 해제 성공, false: 해제 실패</returns>
        public bool UnequipItem(EquipmentSlot slot)
        {
            // PlayerStats가 없으면 실패
            if (playerStats == null)
            {
                Debug.LogError("[InventorySystem] UnequipItem(): PlayerStats를 찾을 수 없습니다.");
                return false;
            }

            // PlayerStats에 장착 해제 요청
            bool success = playerStats.UnequipItem(slot);

            if (success)
            {
                Debug.Log($"[InventorySystem] {slot} 슬롯 장착 해제 완료");
            }
            else
            {
                Debug.LogWarning($"[InventorySystem] {slot} 슬롯 장착 해제 실패");
            }

            return success;
        }

        /// <summary>
        /// 특정 슬롯에 장착된 아이템 가져오기 (PlayerStats 호출)
        /// </summary>
        /// <param name="slot">확인할 슬롯</param>
        /// <returns>장착된 아이템 (없으면 null)</returns>
        public Item GetEquippedItem(EquipmentSlot slot)
        {
            if (playerStats == null)
            {
                Debug.LogError("[InventorySystem] GetEquippedItem(): PlayerStats를 찾을 수 없습니다.");
                return null;
            }

            return playerStats.GetEquippedItem(slot);
        }


        // ====== 디버그 ======

        /// <summary>
        /// 인벤토리 정보 출력
        /// </summary>
        [ContextMenu("Print Inventory")]
        public void DebugPrintInventory()
        {
            Debug.Log($"[InventorySystem] ========== 인벤토리 ({items.Count}개) ==========");

            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                Debug.Log($"[InventorySystem] {i + 1}. {item.itemName} ({item.slot})");
            }

            Debug.Log("[InventorySystem] =====================================");
        }

        /// <summary>
        /// 모든 아이템 제거 (테스트용)
        /// </summary>
        [ContextMenu("Clear Inventory (Test)")]
        private void DebugClearInventory()
        {
            items.Clear();
            Debug.Log("[InventorySystem] 인벤토리 비우기 완료");
        }
    }
}
