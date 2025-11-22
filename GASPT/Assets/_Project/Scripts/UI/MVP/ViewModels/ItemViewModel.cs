using GASPT.Data;
using Core.Enums;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// 아이템 ViewModel
    /// View에 표시할 아이템 데이터를 담는 클래스
    /// </summary>
    public class ItemViewModel
    {
        // ====== 원본 데이터 ======

        /// <summary>
        /// 원본 아이템 (ScriptableObject)
        /// </summary>
        public Item OriginalItem { get; set; }


        // ====== 표시 데이터 ======

        /// <summary>
        /// 아이템 이름
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 아이템 설명
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 아이콘 경로 또는 이름
        /// </summary>
        public string IconPath { get; set; }

        /// <summary>
        /// 장비 슬롯 타입
        /// </summary>
        public EquipmentSlot Slot { get; set; }

        /// <summary>
        /// 장착 가능 여부
        /// </summary>
        public bool IsEquippable { get; set; }

        /// <summary>
        /// 현재 장착 중인지 여부
        /// </summary>
        public bool IsEquipped { get; set; }


        // ====== 정적 팩토리 메서드 ======

        /// <summary>
        /// Item으로부터 ViewModel 생성
        /// </summary>
        public static ItemViewModel FromItem(Item item, bool isEquipped = false)
        {
            if (item == null)
                return null;

            return new ItemViewModel
            {
                OriginalItem = item,
                Name = item.itemName,
                Description = item.description,
                IconPath = item.icon?.name,
                Slot = item.slot,
                IsEquippable = true,
                IsEquipped = isEquipped
            };
        }
    }
}
