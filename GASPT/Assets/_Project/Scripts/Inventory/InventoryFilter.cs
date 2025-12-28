using GASPT.Core.Enums;

namespace GASPT.Inventory
{
    /// <summary>
    /// 인벤토리 필터 조건
    /// </summary>
    public class InventoryFilter
    {
        // ====== 카테고리/등급 ======

        /// <summary>
        /// 카테고리 필터 (null = 전체)
        /// </summary>
        public ItemCategory? category;

        /// <summary>
        /// 최소 등급 필터 (null = 전체)
        /// </summary>
        public ItemRarity? minRarity;


        // ====== 장비 전용 ======

        /// <summary>
        /// 장비 슬롯 필터 (null = 전체)
        /// </summary>
        public EquipmentSlot? equipSlot;


        // ====== 텍스트 검색 ======

        /// <summary>
        /// 이름 포함 검색 (부분 일치)
        /// </summary>
        public string nameContains;


        // ====== 상태 필터 ======

        /// <summary>
        /// 스택 가능 아이템만 (null = 전체)
        /// </summary>
        public bool? stackableOnly;

        /// <summary>
        /// 장착 중 아이템만 (null = 전체)
        /// </summary>
        public bool? equippedOnly;


        // ====== 팩토리 메서드 ======

        /// <summary>
        /// 빈 필터 생성 (전체 반환)
        /// </summary>
        public static InventoryFilter All()
        {
            return new InventoryFilter();
        }

        /// <summary>
        /// 카테고리 필터 생성
        /// </summary>
        public static InventoryFilter ByCategory(ItemCategory category)
        {
            return new InventoryFilter { category = category };
        }

        /// <summary>
        /// 등급 필터 생성
        /// </summary>
        public static InventoryFilter ByMinRarity(ItemRarity minRarity)
        {
            return new InventoryFilter { minRarity = minRarity };
        }

        /// <summary>
        /// 장비 슬롯 필터 생성
        /// </summary>
        public static InventoryFilter ByEquipSlot(EquipmentSlot slot)
        {
            return new InventoryFilter
            {
                category = ItemCategory.Equipment,
                equipSlot = slot
            };
        }

        /// <summary>
        /// 이름 검색 필터 생성
        /// </summary>
        public static InventoryFilter ByName(string nameContains)
        {
            return new InventoryFilter { nameContains = nameContains };
        }

        /// <summary>
        /// 장착 중 아이템 필터
        /// </summary>
        public static InventoryFilter Equipped()
        {
            return new InventoryFilter { equippedOnly = true };
        }
    }
}
