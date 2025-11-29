using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// Shop View (MVP 패턴)
    /// 순수하게 UI 렌더링만 담당
    /// 비즈니스 로직은 ShopPresenter가 처리
    /// </summary>
    public class ShopView : MonoBehaviour, IShopView
    {
        // ====== UI 참조 (SerializeField만) ======

        [Header("Main Panel")]
        [SerializeField] [Tooltip("상점 패널 (활성화/비활성화)")]
        private GameObject panel;

        [Header("Gold Display")]
        [SerializeField] [Tooltip("골드 표시 텍스트")]
        private TextMeshProUGUI goldText;

        [Header("Item List")]
        [SerializeField] [Tooltip("아이템 목록 Content (Scroll View의 자식)")]
        private Transform itemListContent;

        [SerializeField] [Tooltip("아이템 슬롯 프리팹")]
        private GameObject itemSlotPrefab;

        [Header("Message")]
        [SerializeField] [Tooltip("메시지 표시 텍스트")]
        private TextMeshProUGUI messageText;

        [SerializeField] [Tooltip("메시지 표시 시간 (초)")]
        private float messageDisplayTime = 2f;

        [Header("Buttons")]
        [SerializeField] [Tooltip("닫기 버튼")]
        private Button closeButton;

        [Header("Settings")]
        [SerializeField] [Tooltip("상점 열기/닫기 키")]
        private KeyCode toggleKey = KeyCode.B;


        // ====== Presenter 참조 ======

        private ShopPresenter presenter;


        // ====== 내부 상태 (렌더링용) ======

        private List<GameObject> itemSlots = new List<GameObject>();

        private float messageTimer = 0f;
        private bool showingMessage = false;


        // ====== IShopView 이벤트 (View → Presenter) ======

        public event Action OnOpenRequested;
        public event Action OnCloseRequested;
        public event Action<int, ShopItemViewModel> OnPurchaseRequested;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // Presenter 생성 및 초기화
            presenter = new ShopPresenter(this);

            // 닫기 버튼 연결
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
            }

            // 초기 상태: 패널 숨김
            if (panel != null)
            {
                panel.SetActive(false);
            }

            // 메시지 숨김
            HideMessageInternal();

            Debug.Log("[ShopView] Awake 완료");
        }

        private void Start()
        {
            // Presenter 초기화 (Model 참조 획득)
            presenter.Initialize();

            Debug.Log("[ShopView] Start 완료");
        }

        private void Update()
        {
            // Input 감지 → 이벤트 발생 (처리는 Presenter가)
            if (Input.GetKeyDown(toggleKey))
            {
                // 현재 상태에 따라 열기/닫기 이벤트 발생
                if (panel != null && panel.activeSelf)
                {
                    OnCloseRequested?.Invoke();
                }
                else
                {
                    OnOpenRequested?.Invoke();
                }
            }

            // 메시지 타이머 업데이트
            if (showingMessage)
            {
                messageTimer -= Time.deltaTime;

                if (messageTimer <= 0f)
                {
                    HideMessageInternal();
                }
            }
        }

        private void OnDestroy()
        {
            // Presenter 정리
            presenter?.Cleanup();

            // 닫기 버튼 리스너 제거
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }

            Debug.Log("[ShopView] OnDestroy 완료");
        }


        // ====== IShopView 구현 (Presenter → View 명령) ======

        /// <summary>
        /// UI가 현재 표시 중인지 여부
        /// </summary>
        public bool IsVisible => panel != null && panel.activeSelf;

        /// <summary>
        /// UI 표시
        /// </summary>
        public void ShowUI()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        /// <summary>
        /// UI 숨김
        /// </summary>
        public void HideUI()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        /// <summary>
        /// 상점 아이템 목록 표시 (순수 렌더링)
        /// </summary>
        public void DisplayShopItems(List<ShopItemViewModel> items)
        {
            // 기존 슬롯 제거
            ClearItemSlots();

            if (items == null || itemListContent == null || itemSlotPrefab == null)
                return;

            // ViewModel 기반 슬롯 생성
            for (int i = 0; i < items.Count; i++)
            {
                var itemVM = items[i];
                if (itemVM == null)
                    continue;

                CreateItemSlot(i, itemVM);
            }

            Debug.Log($"[ShopView] DisplayShopItems: {items.Count}개 렌더링");
        }

        /// <summary>
        /// 골드 표시 (순수 렌더링)
        /// </summary>
        public void DisplayGold(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: {gold}";
            }
        }

        /// <summary>
        /// 성공 메시지 표시
        /// </summary>
        public void ShowSuccess(string message)
        {
            ShowMessageInternal(message, Color.green);
        }

        /// <summary>
        /// 에러 메시지 표시
        /// </summary>
        public void ShowError(string message)
        {
            ShowMessageInternal(message, Color.red);
        }


        // ====== 아이템 슬롯 렌더링 (Private) ======

        /// <summary>
        /// 아이템 슬롯 생성 (순수 렌더링)
        /// </summary>
        private void CreateItemSlot(int index, ShopItemViewModel itemVM)
        {
            // 슬롯 생성
            GameObject slotObj = Instantiate(itemSlotPrefab, itemListContent);
            itemSlots.Add(slotObj);

            // UI 요소 찾기
            var nameText = slotObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var priceText = slotObj.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            var iconImage = slotObj.transform.Find("IconImage")?.GetComponent<Image>();
            var purchaseButton = slotObj.transform.Find("PurchaseButton")?.GetComponent<Button>();

            // ViewModel 데이터 표시 (순수 렌더링)
            if (nameText != null)
            {
                nameText.text = itemVM.Name;
            }

            if (priceText != null)
            {
                priceText.text = $"{itemVM.Price}G";
            }

            if (iconImage != null && itemVM.Icon != null)
            {
                iconImage.sprite = itemVM.Icon;
            }

            // 구매 버튼 설정
            if (purchaseButton != null)
            {
                // 버튼 활성화/비활성화 (구매 가능 여부)
                purchaseButton.interactable = itemVM.CanAfford;

                // 버튼 텍스트
                var buttonText = purchaseButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = itemVM.CanAfford ? "구매" : "골드 부족";
                }

                // 버튼 이벤트 연결 (View → Presenter)
                int capturedIndex = index; // 클로저 캡처
                purchaseButton.onClick.AddListener(() =>
                {
                    OnPurchaseRequested?.Invoke(capturedIndex, itemVM);
                });
            }
        }

        /// <summary>
        /// 모든 아이템 슬롯 제거
        /// </summary>
        private void ClearItemSlots()
        {
            foreach (var slot in itemSlots)
            {
                if (slot != null)
                {
                    Destroy(slot);
                }
            }

            itemSlots.Clear();
        }


        // ====== 메시지 표시 (Private) ======

        /// <summary>
        /// 메시지 표시 (내부)
        /// </summary>
        private void ShowMessageInternal(string message, Color color)
        {
            if (messageText != null)
            {
                messageText.text = message;
                messageText.color = color;
                messageText.gameObject.SetActive(true);

                messageTimer = messageDisplayTime;
                showingMessage = true;

                Debug.Log($"[ShopView] 메시지 표시: {message}");
            }
        }

        /// <summary>
        /// 메시지 숨김 (내부)
        /// </summary>
        private void HideMessageInternal()
        {
            if (messageText != null)
            {
                messageText.gameObject.SetActive(false);
                showingMessage = false;
            }
        }


        // ====== Public API (외부에서 호출 가능) ======

        /// <summary>
        /// 상점 열기 (외부 호출용)
        /// </summary>
        public void Open()
        {
            OnOpenRequested?.Invoke();
        }

        /// <summary>
        /// 상점 닫기 (외부 호출용)
        /// </summary>
        public void Close()
        {
            OnCloseRequested?.Invoke();
        }
    }
}
