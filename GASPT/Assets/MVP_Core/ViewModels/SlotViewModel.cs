using System;
using UnityEngine;

namespace MVP_Core
{
    /// <summary>
    /// 범용 슬롯 ViewModel
    /// 인벤토리, 퀵슬롯, 장비 슬롯 등에 사용
    /// </summary>
    public class SlotViewModel : SlotViewModelBase
    {
        /// <summary>
        /// 슬롯 내 아이템 ID (null이면 빈 슬롯)
        /// </summary>
        public string ItemId { get; }

        /// <summary>
        /// 아이콘 스프라이트
        /// </summary>
        public Sprite Icon { get; }

        /// <summary>
        /// 아이템 이름
        /// </summary>
        public string ItemName { get; }

        /// <summary>
        /// 수량 (스택)
        /// </summary>
        public int Quantity { get; }

        /// <summary>
        /// 최대 수량 (스택 제한)
        /// </summary>
        public int MaxQuantity { get; }

        /// <summary>
        /// 테두리 색상 (등급 표시용)
        /// </summary>
        public Color BorderColor { get; }

        /// <summary>
        /// 사용 가능 여부 (쿨다운, 레벨 제한 등)
        /// </summary>
        public bool IsUsable { get; }

        /// <summary>
        /// 선택됨 여부
        /// </summary>
        public bool IsSelected { get; }

        /// <summary>
        /// 장착됨 여부 (장비용)
        /// </summary>
        public bool IsEquipped { get; }

        /// <summary>
        /// 쿨다운 비율 (0.0 ~ 1.0, 0이면 쿨다운 없음)
        /// </summary>
        public float CooldownRatio { get; }

        /// <summary>
        /// 슬롯이 비어있는지 여부
        /// </summary>
        public override bool IsEmpty => string.IsNullOrEmpty(ItemId);

        /// <summary>
        /// 빈 슬롯 생성자
        /// </summary>
        public SlotViewModel(int slotIndex)
            : base(slotIndex)
        {
            ItemId = null;
            Icon = null;
            ItemName = "";
            Quantity = 0;
            MaxQuantity = 1;
            BorderColor = Color.gray;
            IsUsable = false;
            IsSelected = false;
            IsEquipped = false;
            CooldownRatio = 0f;
        }

        /// <summary>
        /// 아이템 있는 슬롯 생성자
        /// </summary>
        public SlotViewModel(
            int slotIndex,
            string itemId,
            Sprite icon,
            string itemName,
            int quantity = 1,
            int maxQuantity = 1,
            Color? borderColor = null,
            bool isUsable = true,
            bool isSelected = false,
            bool isEquipped = false,
            float cooldownRatio = 0f)
            : base(slotIndex)
        {
            ItemId = itemId;
            Icon = icon;
            ItemName = itemName;
            Quantity = quantity;
            MaxQuantity = maxQuantity;
            BorderColor = borderColor ?? Color.white;
            IsUsable = isUsable;
            IsSelected = isSelected;
            IsEquipped = isEquipped;
            CooldownRatio = Mathf.Clamp01(cooldownRatio);
        }

        /// <summary>
        /// 선택 상태만 변경된 새 ViewModel 반환
        /// </summary>
        public SlotViewModel WithSelected(bool selected)
        {
            return new SlotViewModel(
                SlotIndex, ItemId, Icon, ItemName, Quantity, MaxQuantity,
                BorderColor, IsUsable, selected, IsEquipped, CooldownRatio);
        }

        /// <summary>
        /// 쿨다운만 변경된 새 ViewModel 반환
        /// </summary>
        public SlotViewModel WithCooldown(float cooldownRatio)
        {
            return new SlotViewModel(
                SlotIndex, ItemId, Icon, ItemName, Quantity, MaxQuantity,
                BorderColor, IsUsable, IsSelected, IsEquipped, cooldownRatio);
        }

        /// <summary>
        /// 수량만 변경된 새 ViewModel 반환
        /// </summary>
        public SlotViewModel WithQuantity(int newQuantity)
        {
            return new SlotViewModel(
                SlotIndex, ItemId, Icon, ItemName, newQuantity, MaxQuantity,
                BorderColor, IsUsable, IsSelected, IsEquipped, CooldownRatio);
        }

        public override bool Equals(ViewModelBase other)
        {
            return other is SlotViewModel vm &&
                   SlotIndex == vm.SlotIndex &&
                   ItemId == vm.ItemId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SlotIndex, ItemId);
        }

        public override string ToString()
        {
            if (IsEmpty)
                return $"[SlotViewModel] Index={SlotIndex} (Empty)";

            return $"[SlotViewModel] Index={SlotIndex}, Item={ItemName}, Qty={Quantity}";
        }
    }

    /// <summary>
    /// 퀵슬롯 전용 ViewModel
    /// 단축키 정보 포함
    /// </summary>
    public class QuickSlotViewModel : SlotViewModel
    {
        /// <summary>
        /// 단축키 (예: "1", "2", "Q", "E")
        /// </summary>
        public string Hotkey { get; }

        /// <summary>
        /// 빈 퀵슬롯 생성자
        /// </summary>
        public QuickSlotViewModel(int slotIndex, string hotkey)
            : base(slotIndex)
        {
            Hotkey = hotkey;
        }

        /// <summary>
        /// 아이템 있는 퀵슬롯 생성자
        /// </summary>
        public QuickSlotViewModel(
            int slotIndex,
            string hotkey,
            string itemId,
            Sprite icon,
            string itemName,
            int quantity = 1,
            int maxQuantity = 1,
            Color? borderColor = null,
            bool isUsable = true,
            float cooldownRatio = 0f)
            : base(slotIndex, itemId, icon, itemName, quantity, maxQuantity,
                   borderColor, isUsable, false, false, cooldownRatio)
        {
            Hotkey = hotkey;
        }

        public override string ToString()
        {
            if (IsEmpty)
                return $"[QuickSlotViewModel] [{Hotkey}] Index={SlotIndex} (Empty)";

            return $"[QuickSlotViewModel] [{Hotkey}] Index={SlotIndex}, Item={ItemName}, Qty={Quantity}";
        }
    }
}
