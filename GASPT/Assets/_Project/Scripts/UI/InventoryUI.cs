using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading;
using GASPT.Data;
using GASPT.Inventory;
using GASPT.Stats;
using Core.Enums;

namespace GASPT.UI
{
    /// <summary>
    /// 인벤토리 UI
    /// I키로 열기/닫기, 아이템 목록 표시, 장착/해제
    /// Phase C-4: 아이템 드롭 및 장착 시스템
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        // ====== UI 참조 ======

        [Header("UI 참조")]
        [Tooltip("인벤토리 패널 (전체 UI)")]
        [SerializeField] private GameObject inventoryPanel;

        [Tooltip("아이템 목록 Content (Scroll View의 자식)")]
        [SerializeField] private Transform itemListContent;

        [Tooltip("아이템 슬롯 프리팹")]
        [SerializeField] private GameObject itemSlotPrefab;

        [Tooltip("장비 슬롯들 (Weapon, Armor, Ring)")]
        [SerializeField] private EquipmentSlotUI weaponSlot;
        [SerializeField] private EquipmentSlotUI armorSlot;
        [SerializeField] private EquipmentSlotUI ringSlot;

        [Tooltip("닫기 버튼")]
        [SerializeField] private Button closeButton;


        // ====== 설정 ======

        [Header("설정")]
        [Tooltip("인벤토리 열기/닫기 키")]
        [SerializeField] private KeyCode toggleKey = KeyCode.I;


        // ====== 시스템 참조 ======

        private InventorySystem inventorySystem;
        private PlayerStats playerStats;


        // ====== 내부 상태 ======

        private bool isOpen = false;
        private List<GameObject> itemSlots = new List<GameObject>();


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // 인벤토리 패널 비활성화
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }

            // 닫기 버튼 설정
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseInventory);
            }
        }

        private void Start()
        {
            // 시스템 참조 가져오기
            inventorySystem = InventorySystem.Instance;
            playerStats = FindAnyObjectByType<PlayerStats>();

            if (inventorySystem == null)
            {
                Debug.LogError("[InventoryUI] InventorySystem을 찾을 수 없습니다.");
                return;
            }

            if (playerStats == null)
            {
                Debug.LogError("[InventoryUI] PlayerStats를 찾을 수 없습니다.");
                return;
            }

            // 이벤트 구독
            inventorySystem.OnItemAdded += OnItemAddedHandler;
            inventorySystem.OnItemRemoved += OnItemRemovedHandler;

            // 장비 슬롯 초기화
            InitializeEquipmentSlots();

            Debug.Log("[InventoryUI] 초기화 완료");
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (inventorySystem != null)
            {
                inventorySystem.OnItemAdded -= OnItemAddedHandler;
                inventorySystem.OnItemRemoved -= OnItemRemovedHandler;
            }

            // 닫기 버튼 리스너 제거
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseInventory);
            }
        }

        private void Update()
        {
            // I키로 인벤토리 열기/닫기
            if (Input.GetKeyDown(toggleKey))
            {
                if (isOpen)
                {
                    CloseInventory();
                }
                else
                {
                    OpenInventory();
                }
            }
        }


        // ====== 인벤토리 열기/닫기 ======

        /// <summary>
        /// 인벤토리 열기
        /// </summary>
        public void OpenInventory()
        {
            if (isOpen)
                return;

            isOpen = true;

            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
            }

            // 아이템 목록 갱신
            RefreshItemList();

            // 장비 슬롯 갱신
            RefreshEquipmentSlots();

            Debug.Log("[InventoryUI] 인벤토리 열기");
        }

        /// <summary>
        /// 인벤토리 닫기
        /// </summary>
        public void CloseInventory()
        {
            if (!isOpen)
                return;

            isOpen = false;

            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }

            Debug.Log("[InventoryUI] 인벤토리 닫기");
        }


        // ====== 아이템 목록 갱신 ======

        /// <summary>
        /// 아이템 목록 갱신
        /// </summary>
        private void RefreshItemList()
        {
            // 기존 슬롯 삭제
            ClearItemSlots();

            if (inventorySystem == null || itemListContent == null || itemSlotPrefab == null)
                return;

            // 보유 아이템 목록 가져오기
            List<Item> items = inventorySystem.GetItems();

            if (items == null || items.Count == 0)
            {
                Debug.Log("[InventoryUI] 보유 아이템 없음");
                return;
            }

            // 아이템 슬롯 생성
            foreach (Item item in items)
            {
                CreateItemSlot(item);
            }

            Debug.Log($"[InventoryUI] 아이템 목록 갱신: {items.Count}개");
        }

        /// <summary>
        /// 아이템 슬롯 생성
        /// </summary>
        private void CreateItemSlot(Item item)
        {
            if (item == null)
                return;

            // 슬롯 인스턴스화
            GameObject slotObj = Instantiate(itemSlotPrefab, itemListContent);
            itemSlots.Add(slotObj);

            // 아이템 정보 설정
            TextMeshProUGUI nameText = slotObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI slotText = slotObj.transform.Find("SlotText")?.GetComponent<TextMeshProUGUI>();
            Image iconImage = slotObj.transform.Find("IconImage")?.GetComponent<Image>();
            Button equipButton = slotObj.transform.Find("EquipButton")?.GetComponent<Button>();

            if (nameText != null)
            {
                nameText.text = item.itemName;
            }

            if (slotText != null)
            {
                slotText.text = $"[{item.slot}]";
            }

            if (iconImage != null && item.icon != null)
            {
                iconImage.sprite = item.icon;
            }

            if (equipButton != null)
            {
                // 장착 버튼 클릭 이벤트
                equipButton.onClick.AddListener(() => OnEquipButtonClicked(item));

                // 버튼 텍스트 설정 (장착 여부에 따라)
                TextMeshProUGUI buttonText = equipButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    Item equippedItem = playerStats?.GetEquippedItem(item.slot);
                    buttonText.text = (equippedItem == item) ? "해제" : "장착";
                }
            }
        }

        /// <summary>
        /// 기존 아이템 슬롯 삭제
        /// </summary>
        private void ClearItemSlots()
        {
            foreach (GameObject slot in itemSlots)
            {
                Destroy(slot);
            }

            itemSlots.Clear();
        }


        // ====== 장비 슬롯 ======

        /// <summary>
        /// 장비 슬롯 초기화
        /// </summary>
        private void InitializeEquipmentSlots()
        {
            if (weaponSlot != null)
            {
                weaponSlot.Initialize(EquipmentSlot.Weapon, OnEquipmentSlotClicked);
            }

            if (armorSlot != null)
            {
                armorSlot.Initialize(EquipmentSlot.Armor, OnEquipmentSlotClicked);
            }

            if (ringSlot != null)
            {
                ringSlot.Initialize(EquipmentSlot.Ring, OnEquipmentSlotClicked);
            }
        }

        /// <summary>
        /// 장비 슬롯 갱신
        /// </summary>
        private void RefreshEquipmentSlots()
        {
            if (playerStats == null)
                return;

            if (weaponSlot != null)
            {
                Item weaponItem = playerStats.GetEquippedItem(EquipmentSlot.Weapon);
                weaponSlot.SetItem(weaponItem);
            }

            if (armorSlot != null)
            {
                Item armorItem = playerStats.GetEquippedItem(EquipmentSlot.Armor);
                armorSlot.SetItem(armorItem);
            }

            if (ringSlot != null)
            {
                Item ringItem = playerStats.GetEquippedItem(EquipmentSlot.Ring);
                ringSlot.SetItem(ringItem);
            }
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 아이템 추가 이벤트 핸들러
        /// </summary>
        private void OnItemAddedHandler(Item item)
        {
            if (isOpen)
            {
                RefreshItemList();
            }
        }

        /// <summary>
        /// 아이템 제거 이벤트 핸들러
        /// </summary>
        private void OnItemRemovedHandler(Item item)
        {
            if (isOpen)
            {
                RefreshItemList();
                RefreshEquipmentSlots();
            }
        }

        /// <summary>
        /// 장착 버튼 클릭 이벤트
        /// </summary>
        private void OnEquipButtonClicked(Item item)
        {
            if (inventorySystem == null || playerStats == null || item == null)
                return;

            // 이미 장착된 아이템인지 확인
            Item equippedItem = playerStats.GetEquippedItem(item.slot);

            if (equippedItem == item)
            {
                // 장착 해제
                bool success = inventorySystem.UnequipItem(item.slot);
                if (success)
                {
                    Debug.Log($"[InventoryUI] {item.itemName} 장착 해제");
                }
            }
            else
            {
                // 장착
                bool success = inventorySystem.EquipItem(item);
                if (success)
                {
                    Debug.Log($"[InventoryUI] {item.itemName} 장착");
                }
            }

            // UI 갱신
            RefreshItemList();
            RefreshEquipmentSlots();
        }

        /// <summary>
        /// 장비 슬롯 클릭 이벤트
        /// </summary>
        private void OnEquipmentSlotClicked(EquipmentSlot slot)
        {
            if (inventorySystem == null)
                return;

            // 장착 해제
            bool success = inventorySystem.UnequipItem(slot);
            if (success)
            {
                Debug.Log($"[InventoryUI] {slot} 슬롯 장착 해제");

                // UI 갱신
                RefreshItemList();
                RefreshEquipmentSlots();
            }
        }


        // ====== Getters ======

        public bool IsOpen => isOpen;


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Open Inventory")]
        private void TestOpenInventory()
        {
            OpenInventory();
        }

        [ContextMenu("Close Inventory")]
        private void TestCloseInventory()
        {
            CloseInventory();
        }

        [ContextMenu("Refresh Item List")]
        private void TestRefreshItemList()
        {
            RefreshItemList();
        }
    }
}
