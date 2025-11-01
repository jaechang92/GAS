using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Shop;
using GASPT.Data;

namespace GASPT.UI
{
    /// <summary>
    /// 상점 아이템 슬롯 UI
    /// ShopUI가 동적으로 생성하는 개별 아이템 슬롯
    /// </summary>
    public class ShopItemSlot : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button purchaseButton;

        private ShopItemData shopItemData;
        private ShopUI shopUI;

        /// <summary>
        /// 슬롯 초기화
        /// </summary>
        /// <param name="data">상점 아이템 데이터</param>
        /// <param name="ui">ShopUI 참조</param>
        public void Initialize(ShopItemData data, ShopUI ui)
        {
            shopItemData = data;
            shopUI = ui;

            // UI 업데이트
            if (itemNameText != null)
            {
                itemNameText.text = data.item.itemName;
            }

            if (priceText != null)
            {
                priceText.text = $"{data.price} Gold";
            }

            // 버튼 이벤트 연결
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
            }
        }

        /// <summary>
        /// 구매 버튼 클릭 핸들러
        /// </summary>
        private void OnPurchaseButtonClicked()
        {
            if (shopUI != null)
            {
                shopUI.OnPurchaseButtonClicked(shopItemData.item);
            }
        }

        private void OnDestroy()
        {
            // 버튼 이벤트 해제
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveListener(OnPurchaseButtonClicked);
            }
        }
    }
}
