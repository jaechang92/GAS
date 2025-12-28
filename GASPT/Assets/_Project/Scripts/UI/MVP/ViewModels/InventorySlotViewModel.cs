using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.UI.MVP.ViewModels
{
    /// <summary>
    /// 인벤토리 슬롯 뷰모델
    /// UI 표시용 데이터
    /// </summary>
    public class InventorySlotViewModel
    {
        // ====== 슬롯 정보 ======

        /// <summary>
        /// 슬롯 인덱스
        /// </summary>
        public int SlotIndex { get; set; }

        /// <summary>
        /// 슬롯이 비어있는지 여부
        /// </summary>
        public bool IsEmpty { get; set; }


        // ====== 아이템 정보 ======

        /// <summary>
        /// 아이템 인스턴스 ID
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// 아이템 이름
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 아이템 설명
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 아이템 아이콘
        /// </summary>
        public Sprite Icon { get; set; }


        // ====== 분류 ======

        /// <summary>
        /// 아이템 카테고리
        /// </summary>
        public ItemCategory Category { get; set; }

        /// <summary>
        /// 아이템 등급
        /// </summary>
        public ItemRarity Rarity { get; set; }

        /// <summary>
        /// 등급 색상
        /// </summary>
        public Color RarityColor { get; set; }


        // ====== 수량 ======

        /// <summary>
        /// 현재 수량
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 스택 가능 여부
        /// </summary>
        public bool IsStackable { get; set; }


        // ====== 장비 정보 ======

        /// <summary>
        /// 장비 아이템 여부
        /// </summary>
        public bool IsEquipment { get; set; }

        /// <summary>
        /// 장착 중 여부
        /// </summary>
        public bool IsEquipped { get; set; }

        /// <summary>
        /// 장비 슬롯 (장비 아이템인 경우)
        /// </summary>
        public EquipmentSlot EquipSlot { get; set; }


        // ====== 소비 정보 ======

        /// <summary>
        /// 소비 아이템 여부
        /// </summary>
        public bool IsConsumable { get; set; }

        /// <summary>
        /// 쿨다운 중 여부
        /// </summary>
        public bool IsOnCooldown { get; set; }

        /// <summary>
        /// 남은 쿨다운 시간
        /// </summary>
        public float RemainingCooldown { get; set; }


        // ====== 생성자 ======

        /// <summary>
        /// 기본 생성자 (빈 슬롯)
        /// </summary>
        public InventorySlotViewModel()
        {
            IsEmpty = true;
            SlotIndex = -1;
            RarityColor = Color.white;
        }

        /// <summary>
        /// 인덱스 지정 생성자
        /// </summary>
        public InventorySlotViewModel(int slotIndex)
        {
            SlotIndex = slotIndex;
            IsEmpty = true;
            RarityColor = Color.white;
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 수량 텍스트 (1개면 빈 문자열)
        /// </summary>
        public string QuantityText => Quantity > 1 ? Quantity.ToString() : "";

        /// <summary>
        /// 쿨다운 텍스트
        /// </summary>
        public string CooldownText => IsOnCooldown ? $"{RemainingCooldown:F1}s" : "";
    }
}
