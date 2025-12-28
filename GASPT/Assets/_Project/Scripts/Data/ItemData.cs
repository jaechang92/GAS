using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Data
{
    /// <summary>
    /// 아이템 기본 데이터 (ScriptableObject)
    /// 모든 아이템 타입의 기본 클래스
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "GASPT/Items/ItemData")]
    public class ItemData : ScriptableObject
    {
        // ====== 기본 정보 ======

        [Header("기본 정보")]
        [Tooltip("고유 식별자 (ITM001, WPN001 등)")]
        public string itemId;

        [Tooltip("아이템 이름 (UI 표시용)")]
        public string itemName;

        [TextArea(2, 4)]
        [Tooltip("아이템 설명")]
        public string description;

        [Tooltip("아이템 아이콘")]
        public Sprite icon;


        // ====== 분류 ======

        [Header("분류")]
        [Tooltip("아이템 카테고리")]
        public ItemCategory category;

        [Tooltip("희귀도")]
        public ItemRarity rarity;


        // ====== 스택 ======

        [Header("스택")]
        [Tooltip("중첩 가능 여부")]
        public bool stackable;

        [Tooltip("최대 중첩 수 (stackable=true일 때 적용)")]
        [Range(1, 99)]
        public int maxStack = 1;


        // ====== 경제 ======

        [Header("경제")]
        [Tooltip("판매 가격")]
        [Min(0)]
        public int sellPrice;

        [Tooltip("구매 가격")]
        [Min(0)]
        public int buyPrice;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 등급별 색상 반환
        /// </summary>
        public Color RarityColor => GetRarityColor();


        // ====== 메서드 ======

        /// <summary>
        /// 등급에 따른 색상 반환
        /// </summary>
        /// <returns>등급에 해당하는 색상</returns>
        public Color GetRarityColor()
        {
            return ItemConstants.GetRarityColor(rarity);
        }

        /// <summary>
        /// 등급에 따른 스탯 배율 반환
        /// </summary>
        /// <returns>등급에 해당하는 스탯 배율</returns>
        public float GetStatMultiplier()
        {
            return ItemConstants.GetStatMultiplier(rarity);
        }


        // ====== Unity 콜백 ======

        private void OnValidate()
        {
            // 스택 불가능 아이템은 maxStack = 1
            if (!stackable)
            {
                maxStack = 1;
            }

            // itemId 자동 생성 (비어있을 때)
            if (string.IsNullOrEmpty(itemId))
            {
                itemId = $"ITM_{name}";
            }
        }


        // ====== 디버그 ======

        /// <summary>
        /// 디버그용 문자열 출력
        /// </summary>
        public override string ToString()
        {
            return $"[{rarity}] {itemName} ({category})";
        }
    }
}
