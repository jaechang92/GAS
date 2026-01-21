using System;
using UnityEngine;
using GASPT.Data;
using GASPT.Inventory;
using GASPT.UI.MVP.Views;

namespace GASPT.UI.Components
{
    /// <summary>
    /// 퀵슬롯 패널 (5개 슬롯 관리)
    /// QuickSlotManager 이벤트 구독 및 실시간 업데이트
    /// </summary>
    public class QuickSlotPanel : MonoBehaviour
    {
        // ====== UI 참조 ======

        [Header("퀵슬롯 UI")]
        [SerializeField] [Tooltip("5개의 QuickSlotUI")]
        private QuickSlotUI[] quickSlots = new QuickSlotUI[5];

        [SerializeField] [Tooltip("아이템 툴팁 (선택사항)")]
        private ItemTooltip itemTooltip;


        // ====== 이벤트 ======

        /// <summary>
        /// 퀵슬롯 우클릭 이벤트 (할당 해제 요청)
        /// 매개변수: (퀵슬롯 인덱스)
        /// </summary>
        public event Action<int> OnQuickSlotRightClicked;


        // ====== Unity 생명주기 ======

        private void Start()
        {
            InitializeSlots();
            SubscribeToSlotEvents();
            RefreshAllSlots();
        }

        private void OnDestroy()
        {
            UnsubscribeFromSlotEvents();
        }


        // ====== 초기화 ======

        /// <summary>
        /// 슬롯 초기화
        /// </summary>
        private void InitializeSlots()
        {
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (quickSlots[i] != null)
                {
                    quickSlots[i].SetSlotIndex(i);
                }
            }
        }

        /// <summary>
        /// 슬롯 이벤트 구독
        /// </summary>
        private void SubscribeToSlotEvents()
        {
            foreach (var slot in quickSlots)
            {
                if (slot != null)
                {
                    slot.OnSlotClicked += OnSlotClicked;
                    slot.OnSlotHoverEnter += OnSlotHoverEnter;
                    slot.OnSlotHoverExit += OnSlotHoverExit;
                }
            }
        }

        /// <summary>
        /// 슬롯 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromSlotEvents()
        {
            foreach (var slot in quickSlots)
            {
                if (slot != null)
                {
                    slot.OnSlotClicked -= OnSlotClicked;
                    slot.OnSlotHoverEnter -= OnSlotHoverEnter;
                    slot.OnSlotHoverExit -= OnSlotHoverExit;
                }
            }
        }


        // ====== 슬롯 갱신 ======

        /// <summary>
        /// 모든 슬롯 갱신
        /// </summary>
        public void RefreshAllSlots()
        {
            foreach (var slot in quickSlots)
            {
                if (slot != null)
                {
                    slot.RefreshSlot();
                }
            }
        }

        /// <summary>
        /// 특정 슬롯 갱신
        /// </summary>
        public void RefreshSlot(int slotIndex)
        {
            if (IsValidSlotIndex(slotIndex) && quickSlots[slotIndex] != null)
            {
                quickSlots[slotIndex].RefreshSlot();
            }
        }


        // ====== 슬롯 이벤트 핸들러 ======

        /// <summary>
        /// 슬롯 클릭 (우클릭)
        /// </summary>
        private void OnSlotClicked(int slotIndex)
        {
            // 우클릭 시 할당 해제
            if (QuickSlotManager.HasInstance)
            {
                QuickSlotManager.Instance.ClearQuickSlot(slotIndex);
            }

            OnQuickSlotRightClicked?.Invoke(slotIndex);
        }

        /// <summary>
        /// 슬롯 호버 진입
        /// </summary>
        private void OnSlotHoverEnter(int slotIndex, ItemInstance item)
        {
            if (itemTooltip != null && item != null)
            {
                itemTooltip.Show(item);
            }
        }

        /// <summary>
        /// 슬롯 호버 이탈
        /// </summary>
        private void OnSlotHoverExit()
        {
            if (itemTooltip != null)
            {
                itemTooltip.Hide();
            }
        }


        // ====== 외부 인터페이스 ======

        /// <summary>
        /// 아이템을 퀵슬롯에 할당
        /// </summary>
        /// <param name="quickSlotIndex">퀵슬롯 인덱스</param>
        /// <param name="inventorySlotIndex">인벤토리 슬롯 인덱스</param>
        /// <returns>할당 성공 여부</returns>
        public bool AssignItem(int quickSlotIndex, int inventorySlotIndex)
        {
            if (!QuickSlotManager.HasInstance)
                return false;

            return QuickSlotManager.Instance.AssignToQuickSlot(quickSlotIndex, inventorySlotIndex);
        }

        /// <summary>
        /// 퀵슬롯 할당 해제
        /// </summary>
        /// <param name="quickSlotIndex">퀵슬롯 인덱스</param>
        public void ClearSlot(int quickSlotIndex)
        {
            if (!QuickSlotManager.HasInstance)
                return;

            QuickSlotManager.Instance.ClearQuickSlot(quickSlotIndex);
        }

        /// <summary>
        /// 모든 퀵슬롯 할당 해제
        /// </summary>
        public void ClearAllSlots()
        {
            if (!QuickSlotManager.HasInstance)
                return;

            QuickSlotManager.Instance.ClearAllQuickSlots();
        }

        /// <summary>
        /// 특정 슬롯 UI 가져오기
        /// </summary>
        public QuickSlotUI GetSlotUI(int slotIndex)
        {
            if (IsValidSlotIndex(slotIndex))
            {
                return quickSlots[slotIndex];
            }
            return null;
        }


        // ====== 유틸리티 ======

        private bool IsValidSlotIndex(int index)
        {
            return index >= 0 && index < quickSlots.Length;
        }


        // ====== 디버그 ======

        [ContextMenu("Print Slot Status")]
        private void DebugPrintSlotStatus()
        {
            Debug.Log("[QuickSlotPanel] ========== 퀵슬롯 상태 ==========");

            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (quickSlots[i] != null)
                {
                    string itemName = quickSlots[i].IsEmpty
                        ? "(비어있음)"
                        : quickSlots[i].CurrentItem?.cachedItemData?.itemName ?? "Unknown";

                    Debug.Log($"[QuickSlotPanel] 슬롯 {i}: {itemName}");
                }
                else
                {
                    Debug.Log($"[QuickSlotPanel] 슬롯 {i}: UI 컴포넌트 없음");
                }
            }

            Debug.Log("[QuickSlotPanel] ===================================");
        }

        [ContextMenu("Refresh All Slots")]
        private void DebugRefreshAll()
        {
            RefreshAllSlots();
            Debug.Log("[QuickSlotPanel] 모든 슬롯 갱신 완료");
        }
    }
}
