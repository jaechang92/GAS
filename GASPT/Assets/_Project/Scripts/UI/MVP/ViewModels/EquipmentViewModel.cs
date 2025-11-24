using GASPT.Data;
using Core.Enums;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// 장비 ViewModel
    /// View에 표시할 장비 슬롯 데이터를 담는 클래스
    /// </summary>
    public class EquipmentViewModel
    {
        /// <summary>
        /// 무기 슬롯에 장착된 아이템
        /// </summary>
        public Item WeaponItem { get; set; }

        /// <summary>
        /// 방어구 슬롯에 장착된 아이템
        /// </summary>
        public Item ArmorItem { get; set; }

        /// <summary>
        /// 반지 슬롯에 장착된 아이템
        /// </summary>
        public Item AccessoryItem { get; set; }


        // ====== 편의 메서드 ======

        /// <summary>
        /// 특정 슬롯의 아이템 가져오기
        /// </summary>
        public Item GetItemBySlot(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Weapon => WeaponItem,
                EquipmentSlot.Armor => ArmorItem,
                EquipmentSlot.Accessory => AccessoryItem,
                _ => null
            };
        }

        /// <summary>
        /// 특정 슬롯에 아이템 설정
        /// </summary>
        public void SetItemBySlot(EquipmentSlot slot, Item item)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon:
                    WeaponItem = item;
                    break;
                case EquipmentSlot.Armor:
                    ArmorItem = item;
                    break;
                case EquipmentSlot.Accessory:
                    AccessoryItem = item;
                    break;
            }
        }
    }
}
