using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Data
{
    /// <summary>
    /// 아이템 관련 상수 정의
    /// </summary>
    public static class ItemConstants
    {
        // ====== 등급별 색상 ======

        /// <summary>
        /// 일반 등급 색상 - 흰색
        /// </summary>
        public static readonly Color CommonColor = Color.white;

        /// <summary>
        /// 고급 등급 색상 - 녹색 (#4CAF50)
        /// </summary>
        public static readonly Color UncommonColor = new Color(0.298f, 0.686f, 0.314f);

        /// <summary>
        /// 희귀 등급 색상 - 파란색 (#2196F3)
        /// </summary>
        public static readonly Color RareColor = new Color(0.129f, 0.588f, 0.953f);

        /// <summary>
        /// 영웅 등급 색상 - 보라색 (#9C27B0)
        /// </summary>
        public static readonly Color EpicColor = new Color(0.612f, 0.153f, 0.690f);

        /// <summary>
        /// 전설 등급 색상 - 주황색 (#FF9800)
        /// </summary>
        public static readonly Color LegendaryColor = new Color(1f, 0.596f, 0f);


        // ====== 등급별 스탯 배율 ======

        /// <summary>
        /// 일반 등급 스탯 배율
        /// </summary>
        public const float CommonStatMultiplier = 1.0f;

        /// <summary>
        /// 고급 등급 스탯 배율
        /// </summary>
        public const float UncommonStatMultiplier = 1.2f;

        /// <summary>
        /// 희귀 등급 스탯 배율
        /// </summary>
        public const float RareStatMultiplier = 1.5f;

        /// <summary>
        /// 영웅 등급 스탯 배율
        /// </summary>
        public const float EpicStatMultiplier = 2.0f;

        /// <summary>
        /// 전설 등급 스탯 배율
        /// </summary>
        public const float LegendaryStatMultiplier = 3.0f;


        /// <summary>
        /// 등급에 따른 색상 반환
        /// </summary>
        /// <param name="rarity">아이템 등급</param>
        /// <returns>등급에 해당하는 색상</returns>
        public static Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => CommonColor,
                ItemRarity.Uncommon => UncommonColor,
                ItemRarity.Rare => RareColor,
                ItemRarity.Epic => EpicColor,
                ItemRarity.Legendary => LegendaryColor,
                _ => CommonColor
            };
        }

        /// <summary>
        /// 등급에 따른 스탯 배율 반환
        /// </summary>
        /// <param name="rarity">아이템 등급</param>
        /// <returns>등급에 해당하는 스탯 배율</returns>
        public static float GetStatMultiplier(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => CommonStatMultiplier,
                ItemRarity.Uncommon => UncommonStatMultiplier,
                ItemRarity.Rare => RareStatMultiplier,
                ItemRarity.Epic => EpicStatMultiplier,
                ItemRarity.Legendary => LegendaryStatMultiplier,
                _ => CommonStatMultiplier
            };
        }
    }
}
