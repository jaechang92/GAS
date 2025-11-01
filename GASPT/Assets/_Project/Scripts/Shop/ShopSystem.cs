using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Economy;
using GASPT.Inventory;

namespace GASPT.Shop
{
    /// <summary>
    /// 상점 아이템 데이터 (아이템 + 가격)
    /// </summary>
    [System.Serializable]
    public struct ShopItemData
    {
        [Tooltip("판매할 아이템")]
        public Item item;

        [Tooltip("가격 (골드)")]
        [Range(1, 1000)]
        public int price;
    }

    /// <summary>
    /// 상점 시스템 MonoBehaviour
    /// 아이템 구매 처리 (골드 소비 → 인벤토리 추가)
    /// </summary>
    public class ShopSystem : MonoBehaviour
    {
        // ====== 상점 아이템 ======

        [Header("상점 아이템")]
        [SerializeField] [Tooltip("판매할 아이템 목록")]
        private List<ShopItemData> shopItems = new List<ShopItemData>();


        // ====== 시스템 참조 ======

        private CurrencySystem currencySystem;
        private InventorySystem inventorySystem;


        // ====== 이벤트 ======

        /// <summary>
        /// 구매 성공 시 발생하는 이벤트
        /// 매개변수: (구매한 아이템, 가격)
        /// </summary>
        public event Action<Item, int> OnPurchaseSuccess;

        /// <summary>
        /// 구매 실패 시 발생하는 이벤트
        /// 매개변수: (실패한 아이템, 가격, 실패 이유)
        /// </summary>
        public event Action<Item, int, string> OnPurchaseFailed;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // 싱글톤 시스템 참조
            currencySystem = CurrencySystem.Instance;
            inventorySystem = InventorySystem.Instance;

            ValidateReferences();

            Debug.Log($"[ShopSystem] 초기화 완료 - 상점 아이템 {shopItems.Count}개");
        }


        // ====== 상점 아이템 접근 ======

        /// <summary>
        /// 상점 아이템 목록 가져오기 (읽기 전용)
        /// </summary>
        /// <returns>상점 아이템 목록 복사본</returns>
        public List<ShopItemData> GetShopItems()
        {
            return new List<ShopItemData>(shopItems);
        }

        /// <summary>
        /// 특정 아이템의 가격 가져오기
        /// </summary>
        /// <param name="item">확인할 아이템</param>
        /// <returns>가격 (아이템이 없으면 -1)</returns>
        public int GetPrice(Item item)
        {
            foreach (var shopItem in shopItems)
            {
                if (shopItem.item == item)
                {
                    return shopItem.price;
                }
            }

            return -1; // 아이템 없음
        }


        // ====== 구매 처리 ======

        /// <summary>
        /// 아이템 구매 시도
        /// </summary>
        /// <param name="item">구매할 아이템</param>
        /// <returns>true: 구매 성공, false: 구매 실패</returns>
        public bool PurchaseItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("[ShopSystem] PurchaseItem(): item이 null입니다.");
                return false;
            }

            // 상점에서 아이템과 가격 찾기
            int price = GetPrice(item);

            if (price < 0)
            {
                string reason = "상점에 없는 아이템입니다.";
                Debug.LogWarning($"[ShopSystem] {reason}: {item.itemName}");
                OnPurchaseFailed?.Invoke(item, 0, reason);
                return false;
            }

            return PurchaseItem(item, price);
        }

        /// <summary>
        /// 아이템 구매 시도 (가격 지정)
        /// </summary>
        /// <param name="item">구매할 아이템</param>
        /// <param name="price">가격</param>
        /// <returns>true: 구매 성공, false: 구매 실패</returns>
        public bool PurchaseItem(Item item, int price)
        {
            if (item == null)
            {
                Debug.LogWarning("[ShopSystem] PurchaseItem(): item이 null입니다.");
                return false;
            }

            // 1. 골드 충분한지 확인
            if (!currencySystem.HasEnoughGold(price))
            {
                string reason = $"골드가 부족합니다. (현재: {currencySystem.Gold}, 필요: {price})";
                Debug.Log($"[ShopSystem] 구매 실패: {reason}");
                OnPurchaseFailed?.Invoke(item, price, reason);
                return false;
            }

            // 2. 골드 소비
            bool spendSuccess = currencySystem.TrySpendGold(price);

            if (!spendSuccess)
            {
                string reason = "골드 소비 실패";
                Debug.LogError($"[ShopSystem] {reason}: {item.itemName}");
                OnPurchaseFailed?.Invoke(item, price, reason);
                return false;
            }

            // 3. 인벤토리에 아이템 추가
            inventorySystem.AddItem(item);

            // 4. 구매 성공 이벤트 발생
            OnPurchaseSuccess?.Invoke(item, price);

            Debug.Log($"[ShopSystem] 구매 성공: {item.itemName} ({price} 골드)");
            return true;
        }


        // ====== 유효성 검증 ======

        /// <summary>
        /// 필수 참조 유효성 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (currencySystem == null)
            {
                Debug.LogError("[ShopSystem] CurrencySystem을 찾을 수 없습니다.");
            }

            if (inventorySystem == null)
            {
                Debug.LogError("[ShopSystem] InventorySystem을 찾을 수 없습니다.");
            }

            // 상점 아이템 유효성 검증
            for (int i = 0; i < shopItems.Count; i++)
            {
                ShopItemData shopItem = shopItems[i];

                if (shopItem.item == null)
                {
                    Debug.LogWarning($"[ShopSystem] shopItems[{i}]: item이 null입니다.");
                }

                if (shopItem.price <= 0)
                {
                    Debug.LogWarning($"[ShopSystem] shopItems[{i}]: price가 0 이하입니다.");
                }
            }
        }


        // ====== 디버그 ======

        /// <summary>
        /// 상점 아이템 정보 출력
        /// </summary>
        [ContextMenu("Print Shop Items")]
        public void DebugPrintShopItems()
        {
            Debug.Log($"[ShopSystem] ========== 상점 아이템 ({shopItems.Count}개) ==========");

            for (int i = 0; i < shopItems.Count; i++)
            {
                ShopItemData shopItem = shopItems[i];
                string itemName = shopItem.item != null ? shopItem.item.itemName : "null";
                Debug.Log($"[ShopSystem] {i + 1}. {itemName} - {shopItem.price} 골드");
            }

            Debug.Log("[ShopSystem] =========================================");
        }
    }
}
