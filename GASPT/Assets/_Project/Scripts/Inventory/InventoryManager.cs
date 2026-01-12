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
    public partial class InventoryManager : SingletonManager<InventoryManager>, ISaveable
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
    }
}
