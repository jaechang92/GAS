using GASPT.Data;
using UnityEngine;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// 상점 아이템 ViewModel (MVP 패턴)
    /// View 표시에 필요한 데이터만 포함
    /// </summary>
    public class ShopItemViewModel
    {
        // ====== 표시 데이터 ======

        /// <summary>
        /// 아이템 이름
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 아이템 가격
        /// </summary>
        public int Price { get; private set; }

        /// <summary>
        /// 아이템 아이콘
        /// </summary>
        public Sprite Icon { get; private set; }

        /// <summary>
        /// 아이템 설명
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// 구매 가능 여부 (골드가 충분한지)
        /// </summary>
        public bool CanAfford { get; private set; }

        /// <summary>
        /// 원본 Item 참조 (Presenter가 구매 처리 시 사용)
        /// </summary>
        public Item OriginalItem { get; private set; }


        // ====== 생성자 ======

        /// <summary>
        /// ShopItemViewModel 생성자
        /// </summary>
        /// <param name="item">원본 아이템</param>
        /// <param name="price">가격</param>
        /// <param name="currentGold">현재 골드</param>
        public ShopItemViewModel(Item item, int price, int currentGold)
        {
            if (item == null)
            {
                Debug.LogError("[ShopItemViewModel] item이 null입니다.");
                return;
            }

            OriginalItem = item;
            Name = item.itemName;
            Price = price;
            Icon = item.icon;
            Description = item.description;
            CanAfford = currentGold >= price;
        }


        // ====== 갱신 메서드 ======

        /// <summary>
        /// 골드 변경 시 구매 가능 여부 갱신
        /// </summary>
        /// <param name="currentGold">현재 골드</param>
        public void UpdateAffordability(int currentGold)
        {
            CanAfford = currentGold >= Price;
        }


        // ====== 디버그 ======

        /// <summary>
        /// ViewModel 정보 문자열 반환
        /// </summary>
        public override string ToString()
        {
            return $"[ShopItemViewModel] {Name} - {Price}G (구매 가능: {CanAfford})";
        }
    }
}
