using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Core;
using GASPT.Core.Enums;
using GASPT.Stats;

namespace GASPT.Inventory
{
    /// <summary>
    /// 퀵슬롯 정보
    /// </summary>
    [Serializable]
    public struct QuickSlotInfo
    {
        /// <summary>
        /// 퀵슬롯 인덱스 (0-4)
        /// </summary>
        public int quickSlotIndex;

        /// <summary>
        /// 연결된 인벤토리 슬롯 인덱스 (-1 = 비어있음)
        /// </summary>
        public int inventorySlotIndex;

        /// <summary>
        /// 비어있는지 여부
        /// </summary>
        public bool IsEmpty => inventorySlotIndex < 0;


        /// <summary>
        /// 생성자
        /// </summary>
        public QuickSlotInfo(int quickSlotIndex, int inventorySlotIndex = -1)
        {
            this.quickSlotIndex = quickSlotIndex;
            this.inventorySlotIndex = inventorySlotIndex;
        }

        /// <summary>
        /// 비우기
        /// </summary>
        public void Clear()
        {
            inventorySlotIndex = -1;
        }
    }


    /// <summary>
    /// 퀵슬롯 매니저
    /// 소비 아이템 퀵 사용 관리
    /// </summary>
    public class QuickSlotManager : SingletonManager<QuickSlotManager>
    {
        // ====== 퀵슬롯 데이터 ======

        /// <summary>
        /// 퀵슬롯 목록
        /// </summary>
        private QuickSlotInfo[] quickSlots;


        // ====== 이벤트 ======

        /// <summary>
        /// 퀵슬롯 변경 이벤트
        /// 매개변수: (퀵슬롯 인덱스)
        /// </summary>
        public event Action<int> OnQuickSlotChanged;

        /// <summary>
        /// 퀵슬롯 사용 이벤트
        /// 매개변수: (퀵슬롯 인덱스, 사용 결과)
        /// </summary>
        public event Action<int, UseResult> OnQuickSlotUsed;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            InitializeQuickSlots();
            Debug.Log($"[QuickSlotManager] 초기화 완료 ({InventoryConstants.QuickSlotCount}슬롯)");
        }


        // ====== 초기화 ======

        /// <summary>
        /// 퀵슬롯 초기화
        /// </summary>
        private void InitializeQuickSlots()
        {
            quickSlots = new QuickSlotInfo[InventoryConstants.QuickSlotCount];

            for (int i = 0; i < quickSlots.Length; i++)
            {
                quickSlots[i] = new QuickSlotInfo(i, -1);
            }
        }


        // ====== 퀵슬롯 할당 ======

        /// <summary>
        /// 인벤토리 슬롯을 퀵슬롯에 할당
        /// </summary>
        /// <param name="quickSlotIndex">퀵슬롯 인덱스 (0-4)</param>
        /// <param name="inventorySlotIndex">인벤토리 슬롯 인덱스</param>
        /// <returns>true = 할당 성공</returns>
        public bool AssignToQuickSlot(int quickSlotIndex, int inventorySlotIndex)
        {
            if (!IsValidQuickSlotIndex(quickSlotIndex))
            {
                Debug.LogWarning($"[QuickSlotManager] 잘못된 퀵슬롯 인덱스: {quickSlotIndex}");
                return false;
            }

            // 인벤토리 슬롯 유효성 검사
            if (!InventoryManager.HasInstance)
            {
                return false;
            }

            InventorySlot slot = InventoryManager.Instance.GetSlot(inventorySlotIndex);

            if (slot == null || slot.IsEmpty)
            {
                Debug.LogWarning($"[QuickSlotManager] 빈 인벤토리 슬롯: {inventorySlotIndex}");
                return false;
            }

            // 소비 아이템인지 확인
            if (!slot.Item.IsConsumable)
            {
                Debug.LogWarning($"[QuickSlotManager] 소비 아이템이 아닙니다: {slot.ItemData?.itemName}");
                return false;
            }

            // 기존에 같은 인벤토리 슬롯이 다른 퀵슬롯에 할당되어 있으면 해제
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (quickSlots[i].inventorySlotIndex == inventorySlotIndex)
                {
                    quickSlots[i].Clear();
                    OnQuickSlotChanged?.Invoke(i);
                }
            }

            // 할당
            quickSlots[quickSlotIndex].inventorySlotIndex = inventorySlotIndex;

            OnQuickSlotChanged?.Invoke(quickSlotIndex);

            Debug.Log($"[QuickSlotManager] 퀵슬롯 {quickSlotIndex} ← 슬롯 {inventorySlotIndex} ({slot.ItemData?.itemName})");

            return true;
        }

        /// <summary>
        /// 퀵슬롯 할당 해제
        /// </summary>
        /// <param name="quickSlotIndex">퀵슬롯 인덱스</param>
        public void ClearQuickSlot(int quickSlotIndex)
        {
            if (!IsValidQuickSlotIndex(quickSlotIndex))
                return;

            if (quickSlots[quickSlotIndex].IsEmpty)
                return;

            quickSlots[quickSlotIndex].Clear();
            OnQuickSlotChanged?.Invoke(quickSlotIndex);

            Debug.Log($"[QuickSlotManager] 퀵슬롯 {quickSlotIndex} 해제");
        }

        /// <summary>
        /// 모든 퀵슬롯 해제
        /// </summary>
        public void ClearAllQuickSlots()
        {
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (!quickSlots[i].IsEmpty)
                {
                    quickSlots[i].Clear();
                    OnQuickSlotChanged?.Invoke(i);
                }
            }
        }


        // ====== 퀵슬롯 사용 ======

        /// <summary>
        /// 퀵슬롯 아이템 사용
        /// </summary>
        /// <param name="quickSlotIndex">퀵슬롯 인덱스</param>
        /// <param name="target">효과 대상</param>
        /// <returns>사용 결과</returns>
        public UseResult UseQuickSlot(int quickSlotIndex, PlayerStats target)
        {
            if (!IsValidQuickSlotIndex(quickSlotIndex))
            {
                return UseResult.InvalidItem;
            }

            QuickSlotInfo quickSlot = quickSlots[quickSlotIndex];

            if (quickSlot.IsEmpty)
            {
                return UseResult.InvalidItem;
            }

            if (!ConsumableManager.HasInstance)
            {
                return UseResult.InvalidItem;
            }

            // 아이템 사용
            UseResult result = ConsumableManager.Instance.UseItemFromSlot(quickSlot.inventorySlotIndex, target);

            OnQuickSlotUsed?.Invoke(quickSlotIndex, result);

            // 아이템이 소진되면 퀵슬롯 갱신
            if (result == UseResult.Success)
            {
                RefreshQuickSlot(quickSlotIndex);
            }

            return result;
        }

        /// <summary>
        /// 퀵슬롯 상태 갱신 (아이템 소진 체크)
        /// </summary>
        private void RefreshQuickSlot(int quickSlotIndex)
        {
            if (!IsValidQuickSlotIndex(quickSlotIndex))
                return;

            QuickSlotInfo quickSlot = quickSlots[quickSlotIndex];

            if (quickSlot.IsEmpty)
                return;

            if (!InventoryManager.HasInstance)
                return;

            InventorySlot invSlot = InventoryManager.Instance.GetSlot(quickSlot.inventorySlotIndex);

            // 인벤토리 슬롯이 비어있으면 퀵슬롯도 해제
            if (invSlot == null || invSlot.IsEmpty)
            {
                ClearQuickSlot(quickSlotIndex);
            }
            else
            {
                // 아이템은 있지만 변경되었을 수 있으므로 UI 업데이트
                OnQuickSlotChanged?.Invoke(quickSlotIndex);
            }
        }


        // ====== 조회 ======

        /// <summary>
        /// 퀵슬롯 정보 가져오기
        /// </summary>
        /// <param name="quickSlotIndex">퀵슬롯 인덱스</param>
        /// <returns>퀵슬롯 정보</returns>
        public QuickSlotInfo GetQuickSlot(int quickSlotIndex)
        {
            if (!IsValidQuickSlotIndex(quickSlotIndex))
            {
                return new QuickSlotInfo(-1, -1);
            }

            return quickSlots[quickSlotIndex];
        }

        /// <summary>
        /// 퀵슬롯에 연결된 아이템 가져오기
        /// </summary>
        /// <param name="quickSlotIndex">퀵슬롯 인덱스</param>
        /// <returns>연결된 아이템 (없으면 null)</returns>
        public ItemInstance GetQuickSlotItem(int quickSlotIndex)
        {
            if (!IsValidQuickSlotIndex(quickSlotIndex))
                return null;

            QuickSlotInfo quickSlot = quickSlots[quickSlotIndex];

            if (quickSlot.IsEmpty)
                return null;

            if (!InventoryManager.HasInstance)
                return null;

            InventorySlot invSlot = InventoryManager.Instance.GetSlot(quickSlot.inventorySlotIndex);

            return invSlot?.Item;
        }

        /// <summary>
        /// 퀵슬롯에 연결된 아이템 수량 가져오기
        /// </summary>
        /// <param name="quickSlotIndex">퀵슬롯 인덱스</param>
        /// <returns>수량 (0 = 없음)</returns>
        public int GetQuickSlotQuantity(int quickSlotIndex)
        {
            if (!IsValidQuickSlotIndex(quickSlotIndex))
                return 0;

            QuickSlotInfo quickSlot = quickSlots[quickSlotIndex];

            if (quickSlot.IsEmpty)
                return 0;

            if (!InventoryManager.HasInstance)
                return 0;

            InventorySlot invSlot = InventoryManager.Instance.GetSlot(quickSlot.inventorySlotIndex);

            return invSlot?.Quantity ?? 0;
        }

        /// <summary>
        /// 모든 퀵슬롯 가져오기
        /// </summary>
        /// <returns>퀵슬롯 배열 복사본</returns>
        public QuickSlotInfo[] GetAllQuickSlots()
        {
            QuickSlotInfo[] copy = new QuickSlotInfo[quickSlots.Length];
            Array.Copy(quickSlots, copy, quickSlots.Length);
            return copy;
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 유효한 퀵슬롯 인덱스인지 확인
        /// </summary>
        private bool IsValidQuickSlotIndex(int index)
        {
            return index >= 0 && index < InventoryConstants.QuickSlotCount;
        }


        // ====== 저장/로드 ======

        /// <summary>
        /// 퀵슬롯 저장 데이터 생성
        /// </summary>
        public List<int> GetSaveData()
        {
            List<int> data = new List<int>();

            for (int i = 0; i < quickSlots.Length; i++)
            {
                data.Add(quickSlots[i].inventorySlotIndex);
            }

            return data;
        }

        /// <summary>
        /// 저장 데이터로부터 로드
        /// </summary>
        public void LoadFromSaveData(List<int> data)
        {
            if (data == null)
                return;

            for (int i = 0; i < quickSlots.Length && i < data.Count; i++)
            {
                quickSlots[i].inventorySlotIndex = data[i];
                OnQuickSlotChanged?.Invoke(i);
            }
        }


        // ====== 디버그 ======

        /// <summary>
        /// 퀵슬롯 상태 출력
        /// </summary>
        [ContextMenu("Print Quick Slots")]
        public void DebugPrintQuickSlots()
        {
            Debug.Log("[QuickSlotManager] ========== 퀵슬롯 ==========");

            for (int i = 0; i < quickSlots.Length; i++)
            {
                QuickSlotInfo qs = quickSlots[i];
                string info = qs.IsEmpty ? "(비어있음)" : $"슬롯 {qs.inventorySlotIndex}";
                ItemInstance item = GetQuickSlotItem(i);

                if (item != null)
                {
                    info += $" - {item.cachedItemData?.itemName} x{GetQuickSlotQuantity(i)}";
                }

                Debug.Log($"[QuickSlotManager] [{i}] {info}");
            }

            Debug.Log("[QuickSlotManager] ===============================");
        }
    }
}
