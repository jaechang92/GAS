using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using GASPT.Shop;
using GASPT.Economy;
using GASPT.Data;

namespace GASPT.UI
{
    /// <summary>
    /// 상점 UI 컨트롤러
    /// ShopSystem과 연동하여 아이템 목록 표시 및 구매 처리
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        // ====== UI 요소 ======

        [Header("UI 요소")]
        [SerializeField] [Tooltip("골드 표시 텍스트")]
        private TextMeshProUGUI goldText;

        [SerializeField] [Tooltip("아이템 목록 부모 Transform (ScrollView Content)")]
        private Transform itemListParent;

        [SerializeField] [Tooltip("아이템 슬롯 프리팹")]
        private GameObject itemSlotPrefab;

        [SerializeField] [Tooltip("메시지 표시 텍스트")]
        private TextMeshProUGUI messageText;

        [SerializeField] [Tooltip("메시지 표시 시간 (초)")]
        private float messageDisplayTime = 2f;


        // ====== 시스템 참조 ======

        [Header("시스템 참조")]
        [SerializeField] [Tooltip("ShopSystem 컴포넌트")]
        private ShopSystem shopSystem;

        private CurrencySystem currencySystem;


        // ====== 메시지 타이머 ======

        private float messageTimer = 0f;
        private bool showingMessage = false;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // ShopSystem 자동 찾기
            if (shopSystem == null)
            {
                shopSystem = FindAnyObjectByType<ShopSystem>();
                if (shopSystem == null)
                {
                    Debug.LogError("[ShopUI] ShopSystem을 찾을 수 없습니다.");
                }
            }

            // CurrencySystem 참조
            currencySystem = CurrencySystem.Instance;

            ValidateReferences();
        }

        private void OnEnable()
        {
            if (currencySystem != null)
            {
                // 골드 변경 이벤트 구독
                currencySystem.OnGoldChanged += OnGoldChanged;

                // 초기 골드 표시
                UpdateGoldText();
            }

            if (shopSystem != null)
            {
                // 구매 이벤트 구독
                shopSystem.OnPurchaseSuccess += OnPurchaseSuccess;
                shopSystem.OnPurchaseFailed += OnPurchaseFailed;

                // 아이템 목록 표시
                DisplayShopItems();
            }

            // 메시지 숨김
            HideMessage();
        }

        private void OnDisable()
        {
            if (currencySystem != null)
            {
                // 이벤트 구독 해제
                currencySystem.OnGoldChanged -= OnGoldChanged;
            }

            if (shopSystem != null)
            {
                // 이벤트 구독 해제
                shopSystem.OnPurchaseSuccess -= OnPurchaseSuccess;
                shopSystem.OnPurchaseFailed -= OnPurchaseFailed;
            }
        }

        private void Update()
        {
            // 메시지 타이머 업데이트
            if (showingMessage)
            {
                messageTimer -= Time.deltaTime;

                if (messageTimer <= 0f)
                {
                    HideMessage();
                }
            }
        }


        // ====== 상점 아이템 표시 ======

        /// <summary>
        /// 상점 아이템 목록 표시
        /// </summary>
        private void DisplayShopItems()
        {
            // 기존 아이템 슬롯 제거
            ClearItemList();

            // ShopSystem에서 아이템 목록 가져오기
            List<ShopItemData> shopItems = shopSystem.GetShopItems();

            // 각 아이템에 대해 슬롯 생성
            foreach (var shopItem in shopItems)
            {
                if (shopItem.item != null)
                {
                    CreateItemSlot(shopItem);
                }
            }

            Debug.Log($"[ShopUI] 상점 아이템 {shopItems.Count}개 표시 완료");
        }

        /// <summary>
        /// 아이템 슬롯 생성
        /// </summary>
        /// <param name="shopItem">상점 아이템 데이터</param>
        private void CreateItemSlot(ShopItemData shopItem)
        {
            if (itemSlotPrefab == null || itemListParent == null)
            {
                Debug.LogError("[ShopUI] itemSlotPrefab 또는 itemListParent가 null입니다.");
                return;
            }

            // 슬롯 인스턴스 생성
            GameObject slotObj = Instantiate(itemSlotPrefab, itemListParent);
            ShopItemSlot slot = slotObj.GetComponent<ShopItemSlot>();

            if (slot != null)
            {
                // 슬롯 초기화
                slot.Initialize(shopItem, this);
            }
            else
            {
                Debug.LogError("[ShopUI] itemSlotPrefab에 ShopItemSlot 컴포넌트가 없습니다.");
            }
        }

        /// <summary>
        /// 아이템 목록 초기화
        /// </summary>
        private void ClearItemList()
        {
            if (itemListParent == null) return;

            // 모든 자식 제거
            foreach (Transform child in itemListParent)
            {
                Destroy(child.gameObject);
            }
        }


        // ====== 구매 처리 ======

        /// <summary>
        /// 아이템 구매 (외부에서 호출)
        /// </summary>
        /// <param name="item">구매할 아이템</param>
        public void OnPurchaseButtonClicked(Item item)
        {
            if (shopSystem != null)
            {
                shopSystem.PurchaseItem(item);
            }
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 골드 변경 이벤트 핸들러
        /// </summary>
        private void OnGoldChanged(int oldGold, int newGold)
        {
            UpdateGoldText();
        }

        /// <summary>
        /// 구매 성공 이벤트 핸들러
        /// </summary>
        private void OnPurchaseSuccess(Item item, int price)
        {
            ShowMessage($"{item.itemName} 구매 완료! (-{price} 골드)", Color.green);
        }

        /// <summary>
        /// 구매 실패 이벤트 핸들러
        /// </summary>
        private void OnPurchaseFailed(Item item, int price, string reason)
        {
            ShowMessage($"구매 실패: {reason}", Color.red);
        }


        // ====== UI 업데이트 ======

        /// <summary>
        /// 골드 텍스트 업데이트
        /// </summary>
        private void UpdateGoldText()
        {
            if (goldText != null && currencySystem != null)
            {
                goldText.text = $"Gold: {currencySystem.Gold}";
            }
        }

        /// <summary>
        /// 메시지 표시
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        /// <param name="color">메시지 색상</param>
        private void ShowMessage(string message, Color color)
        {
            if (messageText != null)
            {
                messageText.text = message;
                messageText.color = color;
                messageText.gameObject.SetActive(true);

                messageTimer = messageDisplayTime;
                showingMessage = true;

                Debug.Log($"[ShopUI] 메시지 표시: {message}");
            }
        }

        /// <summary>
        /// 메시지 숨김
        /// </summary>
        private void HideMessage()
        {
            if (messageText != null)
            {
                messageText.gameObject.SetActive(false);
                showingMessage = false;
            }
        }


        // ====== 유효성 검증 ======

        /// <summary>
        /// 필수 참조 유효성 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (goldText == null)
            {
                Debug.LogError("[ShopUI] goldText가 null입니다.");
            }

            if (itemListParent == null)
            {
                Debug.LogError("[ShopUI] itemListParent가 null입니다.");
            }

            if (itemSlotPrefab == null)
            {
                Debug.LogError("[ShopUI] itemSlotPrefab가 null입니다.");
            }

            if (messageText == null)
            {
                Debug.LogWarning("[ShopUI] messageText가 null입니다.");
            }

            if (shopSystem == null)
            {
                Debug.LogError("[ShopUI] shopSystem이 null입니다.");
            }

            if (currencySystem == null)
            {
                Debug.LogError("[ShopUI] CurrencySystem을 찾을 수 없습니다.");
            }
        }
    }
}
