using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using GASPT.Core.Enums;
using GASPT.Data;
using GASPT.Inventory;
using GASPT.Stats;

namespace GASPT.UI.Components
{
    /// <summary>
    /// 퀵슬롯 UI 컴포넌트
    /// 소비 아이템 퀵 사용 슬롯
    /// 키보드 단축키(1~5) 또는 클릭으로 아이템 사용
    /// </summary>
    public class QuickSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        // ====== UI 참조 ======

        [Header("UI 요소")]
        [SerializeField] [Tooltip("아이템 아이콘")]
        private Image iconImage;

        [SerializeField] [Tooltip("배경 이미지")]
        private Image backgroundImage;

        [SerializeField] [Tooltip("등급 테두리")]
        private Image rarityBorder;

        [SerializeField] [Tooltip("수량 텍스트")]
        private TextMeshProUGUI quantityText;

        [SerializeField] [Tooltip("쿨다운 오버레이 (fillAmount)")]
        private Image cooldownOverlay;

        [SerializeField] [Tooltip("쿨다운 텍스트")]
        private TextMeshProUGUI cooldownText;

        [SerializeField] [Tooltip("단축키 텍스트")]
        private TextMeshProUGUI hotkeyText;

        [SerializeField] [Tooltip("비활성 오버레이")]
        private Image disabledOverlay;


        // ====== 설정 ======

        [Header("슬롯 설정")]
        [SerializeField] [Tooltip("퀵슬롯 인덱스 (0~4)")]
        private int slotIndex = 0;

        [SerializeField] [Tooltip("단축키")]
        private KeyCode hotkey = KeyCode.Alpha1;


        [Header("색상 설정")]
        [SerializeField]
        private Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        [SerializeField]
        private Color filledSlotColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        [SerializeField]
        private Color cooldownColor = new Color(0f, 0f, 0f, 0.7f);

        [SerializeField]
        private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);


        // ====== 내부 상태 ======

        private bool isEmpty = true;
        private ItemInstance currentItem;
        private ConsumableData currentConsumable;
        private PlayerStats playerStats;
        private bool isSubscribed = false;


        // ====== 이벤트 ======

        /// <summary>
        /// 슬롯 클릭 이벤트
        /// 매개변수: (퀵슬롯 인덱스)
        /// </summary>
        public event Action<int> OnSlotClicked;

        /// <summary>
        /// 슬롯 호버 진입 이벤트
        /// 매개변수: (퀵슬롯 인덱스, 아이템)
        /// </summary>
        public event Action<int, ItemInstance> OnSlotHoverEnter;

        /// <summary>
        /// 슬롯 호버 이탈 이벤트
        /// </summary>
        public event Action OnSlotHoverExit;


        // ====== 프로퍼티 ======

        public int SlotIndex => slotIndex;
        public bool IsEmpty => isEmpty;
        public ItemInstance CurrentItem => currentItem;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            InitializeUI();
        }

        private void Start()
        {
            FindPlayerStats();
            SubscribeToEvents();
            UpdateHotkeyText();
            RefreshSlot();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            // 키 입력 처리
            if (Input.GetKeyDown(hotkey))
            {
                TryUseItem();
            }

            // 쿨다운 UI 업데이트
            if (!isEmpty && currentConsumable != null)
            {
                UpdateCooldownUI();
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 초기 상태: 빈 슬롯
            if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = emptySlotColor;
            }

            if (rarityBorder != null)
            {
                rarityBorder.enabled = false;
            }

            if (quantityText != null)
            {
                quantityText.enabled = false;
            }

            if (cooldownOverlay != null)
            {
                cooldownOverlay.enabled = false;
                cooldownOverlay.color = cooldownColor;
            }

            if (cooldownText != null)
            {
                cooldownText.enabled = false;
            }

            if (disabledOverlay != null)
            {
                disabledOverlay.enabled = false;
                disabledOverlay.color = disabledColor;
            }
        }

        /// <summary>
        /// PlayerStats 찾기
        /// </summary>
        private void FindPlayerStats()
        {
            playerStats = FindAnyObjectByType<PlayerStats>();
        }

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            if (isSubscribed)
                return;

            if (QuickSlotManager.HasInstance)
            {
                QuickSlotManager.Instance.OnQuickSlotChanged += OnQuickSlotChanged;
                isSubscribed = true;
            }

            // 인벤토리 변경 구독 (수량 변화 감지)
            if (InventoryManager.HasInstance)
            {
                InventoryManager.Instance.OnSlotChanged += OnInventorySlotChanged;
            }
        }

        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (QuickSlotManager.HasInstance)
            {
                QuickSlotManager.Instance.OnQuickSlotChanged -= OnQuickSlotChanged;
            }

            if (InventoryManager.HasInstance)
            {
                InventoryManager.Instance.OnSlotChanged -= OnInventorySlotChanged;
            }

            isSubscribed = false;
        }


        // ====== 슬롯 갱신 ======

        /// <summary>
        /// 슬롯 UI 갱신
        /// </summary>
        public void RefreshSlot()
        {
            if (!QuickSlotManager.HasInstance)
            {
                ClearSlot();
                return;
            }

            QuickSlotInfo info = QuickSlotManager.Instance.GetQuickSlot(slotIndex);

            if (info.IsEmpty)
            {
                ClearSlot();
                return;
            }

            ItemInstance item = QuickSlotManager.Instance.GetQuickSlotItem(slotIndex);

            if (item == null)
            {
                ClearSlot();
                return;
            }

            SetItem(item, QuickSlotManager.Instance.GetQuickSlotQuantity(slotIndex));
        }

        /// <summary>
        /// 아이템 설정
        /// </summary>
        private void SetItem(ItemInstance item, int quantity)
        {
            if (item == null || item.cachedItemData == null)
            {
                ClearSlot();
                return;
            }

            isEmpty = false;
            currentItem = item;
            currentConsumable = item.cachedItemData as ConsumableData;

            // 아이콘
            if (iconImage != null)
            {
                iconImage.sprite = item.cachedItemData.icon;
                iconImage.enabled = item.cachedItemData.icon != null;
            }

            // 배경
            if (backgroundImage != null)
            {
                backgroundImage.color = filledSlotColor;
            }

            // 등급 테두리
            if (rarityBorder != null)
            {
                rarityBorder.color = GetRarityColor(item.cachedItemData.rarity);
                rarityBorder.enabled = true;
            }

            // 수량
            if (quantityText != null)
            {
                quantityText.text = quantity.ToString();
                quantityText.enabled = quantity > 1;
            }

            // 쿨다운 초기화
            UpdateCooldownUI();
        }

        /// <summary>
        /// 슬롯 비우기
        /// </summary>
        public void ClearSlot()
        {
            isEmpty = true;
            currentItem = null;
            currentConsumable = null;

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = emptySlotColor;
            }

            if (rarityBorder != null)
            {
                rarityBorder.enabled = false;
            }

            if (quantityText != null)
            {
                quantityText.text = "";
                quantityText.enabled = false;
            }

            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.enabled = false;
            }

            if (cooldownText != null)
            {
                cooldownText.enabled = false;
            }

            if (disabledOverlay != null)
            {
                disabledOverlay.enabled = false;
            }
        }


        // ====== 아이템 사용 ======

        /// <summary>
        /// 아이템 사용 시도
        /// </summary>
        private void TryUseItem()
        {
            if (isEmpty)
                return;

            if (!QuickSlotManager.HasInstance)
                return;

            if (playerStats == null)
            {
                FindPlayerStats();
                if (playerStats == null)
                    return;
            }

            UseResult result = QuickSlotManager.Instance.UseQuickSlot(slotIndex, playerStats);

            // 사용 결과에 따른 피드백 (선택사항)
            if (result != UseResult.Success)
            {
                Debug.Log($"[QuickSlotUI] 슬롯 {slotIndex} 사용 실패: {result}");
            }
        }


        // ====== UI 업데이트 ======

        /// <summary>
        /// 쿨다운 UI 업데이트
        /// </summary>
        private void UpdateCooldownUI()
        {
            if (currentConsumable == null)
                return;

            // ConsumableManager에서 쿨다운 정보 조회
            if (!ConsumableManager.HasInstance)
                return;

            float remaining = ConsumableManager.Instance.GetRemainingCooldown(currentConsumable.itemId);
            bool isOnCooldown = remaining > 0f;

            // 쿨다운 오버레이
            if (cooldownOverlay != null)
            {
                cooldownOverlay.enabled = isOnCooldown;

                if (isOnCooldown)
                {
                    float total = currentConsumable.cooldown;
                    cooldownOverlay.fillAmount = total > 0f ? remaining / total : 0f;
                }
            }

            // 쿨다운 텍스트
            if (cooldownText != null)
            {
                cooldownText.enabled = isOnCooldown;

                if (isOnCooldown)
                {
                    cooldownText.text = $"{remaining:F1}";
                }
            }
        }

        /// <summary>
        /// 단축키 텍스트 업데이트
        /// </summary>
        private void UpdateHotkeyText()
        {
            if (hotkeyText != null)
            {
                // KeyCode를 숫자로 변환 (Alpha1 → "1")
                string key = hotkey.ToString().Replace("Alpha", "");
                hotkeyText.text = key;
            }
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 퀵슬롯 변경 이벤트
        /// </summary>
        private void OnQuickSlotChanged(int changedSlotIndex)
        {
            if (changedSlotIndex == slotIndex)
            {
                RefreshSlot();
            }
        }

        /// <summary>
        /// 인벤토리 슬롯 변경 이벤트
        /// </summary>
        private void OnInventorySlotChanged(int inventorySlotIndex)
        {
            // 연결된 인벤토리 슬롯이 변경되었는지 확인
            if (!QuickSlotManager.HasInstance)
                return;

            QuickSlotInfo info = QuickSlotManager.Instance.GetQuickSlot(slotIndex);

            if (info.inventorySlotIndex == inventorySlotIndex)
            {
                RefreshSlot();
            }
        }


        // ====== Pointer 이벤트 ======

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isEmpty && currentItem != null)
            {
                OnSlotHoverEnter?.Invoke(slotIndex, currentItem);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnSlotHoverExit?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // 좌클릭: 아이템 사용
                TryUseItem();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // 우클릭: 슬롯 클릭 이벤트 (할당 해제 등)
                OnSlotClicked?.Invoke(slotIndex);
            }
        }


        // ====== 외부 설정 ======

        /// <summary>
        /// 슬롯 인덱스 설정
        /// </summary>
        public void SetSlotIndex(int index)
        {
            slotIndex = index;

            // 단축키 자동 설정 (0→1, 1→2, 2→3, 3→4, 4→5)
            switch (index)
            {
                case 0: hotkey = KeyCode.Alpha1; break;
                case 1: hotkey = KeyCode.Alpha2; break;
                case 2: hotkey = KeyCode.Alpha3; break;
                case 3: hotkey = KeyCode.Alpha4; break;
                case 4: hotkey = KeyCode.Alpha5; break;
            }

            UpdateHotkeyText();
            RefreshSlot();
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 등급 색상 가져오기
        /// </summary>
        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => Color.green,
                ItemRarity.Rare => Color.blue,
                ItemRarity.Epic => new Color(0.6f, 0.2f, 0.8f), // 보라색
                ItemRarity.Legendary => new Color(1f, 0.65f, 0f), // 주황색
                ItemRarity.Mythic => Color.red,
                _ => Color.gray
            };
        }


        // ====== 디버그 ======

        [ContextMenu("Print Quick Slot UI Info")]
        private void DebugPrintInfo()
        {
            string itemName = currentItem?.cachedItemData?.itemName ?? "(없음)";
            int quantity = QuickSlotManager.HasInstance
                ? QuickSlotManager.Instance.GetQuickSlotQuantity(slotIndex)
                : 0;

            Debug.Log($"[QuickSlotUI] 슬롯 {slotIndex}\n" +
                     $"  비어있음: {isEmpty}\n" +
                     $"  아이템: {itemName}\n" +
                     $"  수량: {quantity}\n" +
                     $"  단축키: {hotkey}");
        }

        [ContextMenu("Test Use")]
        private void TestUse()
        {
            TryUseItem();
        }
    }
}
