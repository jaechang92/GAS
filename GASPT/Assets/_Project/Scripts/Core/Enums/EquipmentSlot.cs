using System;

namespace GASPT.Core.Enums
{
    /// <summary>
    /// 장비 슬롯 타입 (7슬롯 시스템)
    /// </summary>
    public enum EquipmentSlot
    {
        /// <summary>
        /// 장비 불가 (비장비 아이템용)
        /// </summary>
        None = -1,

        /// <summary>
        /// 무기 슬롯 (공격력 증가)
        /// </summary>
        Weapon = 0,

        /// <summary>
        /// 방어구 슬롯 (HP, 방어력 증가)
        /// </summary>
        Armor = 1,

        /// <summary>
        /// 투구 슬롯 (방어력, 마나 증가)
        /// </summary>
        Helmet = 2,

        /// <summary>
        /// 장갑 슬롯 (공격속도, 치명타 증가)
        /// </summary>
        Gloves = 3,

        /// <summary>
        /// 신발 슬롯 (이동속도, 회피 증가)
        /// </summary>
        Boots = 4,

        /// <summary>
        /// 악세서리 1 슬롯 (반지)
        /// </summary>
        Accessory1 = 5,

        /// <summary>
        /// 악세서리 2 슬롯 (목걸이)
        /// </summary>
        Accessory2 = 6,

        /// <summary>
        /// [Deprecated] Accessory1로 대체됨. 레거시 호환용.
        /// </summary>
        [Obsolete("Use Accessory1 instead")]
        Accessory = 5
    }
}
