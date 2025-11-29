using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Data;
using Core.Enums;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// Inventory View (MVP 패턴)
    /// 순수하게 UI 렌더링만 담당
    /// 비즈니스 로직은 InventoryPresenter가 처리
    /// </summary>
    public class InventoryView : MonoBehaviour, IInventoryView
    {
        // ====== UI 참조 (SerializeField만) ======

        [Header("Main Panel")]
        [SerializeField] [Tooltip("인벤토리 패널 (활성화/비활성화)")]
        private GameObject panel;

        [Header("Item List")]
        [SerializeField] [Tooltip("아이템 목록 Content (Scroll View의 자식)")]
        private Transform itemListContent;

        [SerializeField] [Tooltip("아이템 슬롯 프리팹")]
        private GameObject itemSlotPrefab;

        [Header("Equipment Slots")]
        [SerializeField] [Tooltip("무기 슬롯")]
        private EquipmentSlotUI weaponSlot;

        [SerializeField] [Tooltip("방어구 슬롯")]
        private EquipmentSlotUI armorSlot;

        [SerializeField] [Tooltip("반지 슬롯")]
        private EquipmentSlotUI accessorySlot;

        [Header("Buttons")]
        [SerializeField] [Tooltip("닫기 버튼")]
        private Button closeButton;

        [Header("Settings")]
        [SerializeField] [Tooltip("인벤토리 열기/닫기 키")]
        private KeyCode toggleKey = KeyCode.I;


        // ====== Presenter 참조 ======

        private InventoryPresenter presenter;


        // ====== 내부 상태 (렌더링용) ======

        private List<GameObject> itemSlots = new List<GameObject>();

        // 장비 슬롯 이벤트 핸들러 참조 (구독 해제용)
        private Action weaponSlotHandler;
        private Action armorSlotHandler;
        private Action accessorySlotHandler;


        // ====== IInventoryView 이벤트 (View → Presenter) ======

        public event Action OnOpenRequested;
        public event Action OnCloseRequested;
        public event Action<Item> OnItemEquipRequested;
        public event Action<EquipmentSlot> OnEquipmentSlotUnequipRequested;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // Presenter 생성 및 초기화
            presenter = new InventoryPresenter(this);

            // 닫기 버튼 연결
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
            }

            // 장비 슬롯 이벤트 연결
            InitializeEquipmentSlots();

            // 초기 상태: 패널 숨김
            if (panel != null)
            {
                panel.SetActive(false);
            }

            Debug.Log("[InventoryView] Awake 완료");
        }

        private void Start()
        {
            // Presenter 초기화 (Model 참조 획득)
            presenter.Initialize();

            Debug.Log("[InventoryView] Start 완료");
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

            // 장비 슬롯 리스너 제거
            CleanupEquipmentSlots();

            Debug.Log("[InventoryView] OnDestroy 완료");
        }


        // ====== IInventoryView 구현 (Presenter → View 명령) ======

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
        /// 아이템 목록 표시 (순수 렌더링)
        /// </summary>
        public void DisplayItems(List<ItemViewModel> items)
        {
            // 기존 슬롯 제거
            ClearItemSlots();

            if (items == null || itemListContent == null || itemSlotPrefab == null)
                return;

            // ViewModel 기반 슬롯 생성
            foreach (var itemVM in items)
            {
                if (itemVM == null)
                    continue;

                CreateItemSlot(itemVM);
            }

            Debug.Log($"[InventoryView] DisplayItems: {items.Count}개 렌더링");
        }

        /// <summary>
        /// 장비 슬롯 표시 (순수 렌더링)
        /// </summary>
        public void DisplayEquipment(EquipmentViewModel equipment)
        {
            if (equipment == null)
                return;

            // 무기 슬롯
            if (weaponSlot != null)
            {
                weaponSlot.SetItem(equipment.WeaponItem);
            }

            // 방어구 슬롯
            if (armorSlot != null)
            {
                armorSlot.SetItem(equipment.ArmorItem);
            }

            // 반지 슬롯
            if (accessorySlot != null)
            {
                accessorySlot.SetItem(equipment.AccessoryItem);
            }

            Debug.Log("[InventoryView] DisplayEquipment 완료");
        }

        /// <summary>
        /// 에러 메시지 표시
        /// </summary>
        public void ShowError(string message)
        {
            Debug.LogWarning($"[InventoryView] Error: {message}");
            // TODO: 에러 팝업 UI 표시
        }

        /// <summary>
        /// 성공 메시지 표시
        /// </summary>
        public void ShowSuccess(string message)
        {
            Debug.Log($"[InventoryView] Success: {message}");
            // TODO: 성공 팝업 UI 표시
        }


        // ====== 아이템 슬롯 렌더링 (Private) ======

        /// <summary>
        /// 아이템 슬롯 생성 (순수 렌더링)
        /// </summary>
        private void CreateItemSlot(ItemViewModel itemVM)
        {
            // 슬롯 생성
            GameObject slotObj = Instantiate(itemSlotPrefab, itemListContent);
            itemSlots.Add(slotObj);

            // UI 요소 찾기
            var nameText = slotObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var slotText = slotObj.transform.Find("SlotText")?.GetComponent<TextMeshProUGUI>();
            var iconImage = slotObj.transform.Find("IconImage")?.GetComponent<Image>();
            var equipButton = slotObj.transform.Find("EquipButton")?.GetComponent<Button>();

            // ViewModel 데이터 표시 (순수 렌더링)
            if (nameText != null)
            {
                nameText.text = itemVM.Name;
            }

            if (slotText != null)
            {
                slotText.text = $"[{itemVM.Slot}]";
            }

            if (iconImage != null && itemVM.OriginalItem?.icon != null)
            {
                iconImage.sprite = itemVM.OriginalItem.icon;
            }

            // 장착 버튼 설정
            if (equipButton != null)
            {
                // 버튼 텍스트 (장착/해제)
                var buttonText = equipButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = itemVM.IsEquipped ? "해제" : "장착";
                }

                // 버튼 이벤트 연결 (View → Presenter)
                equipButton.onClick.AddListener(() =>
                {
                    OnItemEquipRequested?.Invoke(itemVM.OriginalItem);
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


        // ====== 장비 슬롯 초기화 (Private) ======

        /// <summary>
        /// 장비 슬롯 이벤트 연결
        /// </summary>
        private void InitializeEquipmentSlots()
        {
            if (weaponSlot != null)
            {
                weaponSlotHandler = () =>
                {
                    OnEquipmentSlotUnequipRequested?.Invoke(EquipmentSlot.Weapon);
                };
                weaponSlot.OnSlotClicked += weaponSlotHandler;
            }

            if (armorSlot != null)
            {
                armorSlotHandler = () =>
                {
                    OnEquipmentSlotUnequipRequested?.Invoke(EquipmentSlot.Armor);
                };
                armorSlot.OnSlotClicked += armorSlotHandler;
            }

            if (accessorySlot != null)
            {
                accessorySlotHandler = () =>
                {
                    OnEquipmentSlotUnequipRequested?.Invoke(EquipmentSlot.Accessory);
                };
                accessorySlot.OnSlotClicked += accessorySlotHandler;
            }
        }

        /// <summary>
        /// 장비 슬롯 이벤트 정리
        /// </summary>
        private void CleanupEquipmentSlots()
        {
            if (weaponSlot != null && weaponSlotHandler != null)
            {
                weaponSlot.OnSlotClicked -= weaponSlotHandler;
            }

            if (armorSlot != null && armorSlotHandler != null)
            {
                armorSlot.OnSlotClicked -= armorSlotHandler;
            }

            if (accessorySlot != null && accessorySlotHandler != null)
            {
                accessorySlot.OnSlotClicked -= accessorySlotHandler;
            }
        }


        // ====== Public API (외부에서 호출 가능) ======

        /// <summary>
        /// 인벤토리 열기 (외부 호출용)
        /// </summary>
        public void Open()
        {
            OnOpenRequested?.Invoke();
        }

        /// <summary>
        /// 인벤토리 닫기 (외부 호출용)
        /// </summary>
        public void Close()
        {
            OnCloseRequested?.Invoke();
        }


        [ContextMenu("Auto Fill References")]
        private void AutoFillReferences()
        {
            // Main Panel
            if (panel == null)
                panel = this.transform.Find("Panel")?.gameObject;
            // Item List
            if (itemListContent == null)
                itemListContent = this.transform.Find("Panel/ItemListPanel/Viewport/Content");
            if (itemSlotPrefab == null)
            {
                var prefab = Resources.Load<GameObject>("Prefabs/UI/ItemSlot");
                if (prefab != null)
                    itemSlotPrefab = prefab;
            }
            // Equipment Slots
            if (weaponSlot == null)
                weaponSlot = this.transform.Find("Panel/EquipmentPanel/WeaponSlot")?.GetComponent<EquipmentSlotUI>();
            if (armorSlot == null)
                armorSlot = this.transform.Find("Panel/EquipmentPanel/ArmorSlot")?.GetComponent<EquipmentSlotUI>();
            if (accessorySlot == null)
                accessorySlot = this.transform.Find("Panel/EquipmentPanel/AccessorySlot")?.GetComponent<EquipmentSlotUI>();
            // Buttons
            if (closeButton == null)
                closeButton = this.transform.Find("Panel/CloseButton")?.GetComponent<Button>();
            Debug.Log("[InventoryView] AutoFillReferences 완료");
        }
    }
}
