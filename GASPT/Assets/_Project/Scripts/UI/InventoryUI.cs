using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading;
using GASPT.Data;
using GASPT.Inventory;
using GASPT.Stats;
using Core.Enums;
using Core;

namespace GASPT.UI
{
    /// <summary>
    /// 인벤토리 UI
    /// I키로 열기/닫기, 아이템 목록 표시, 장착/해제
    /// Phase C-4: 아이템 드롭 및 장착 시스템
    /// </summary>
    public class InventoryUI : BaseUI
    {
        // ====== UI 참조 ======

        [Header("Inventory UI 참조")]

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

        private List<GameObject> itemSlots = new List<GameObject>();


        // ====== Unity 생명주기 ======

        protected override void Awake()
        {
            // BaseUI Awake 호출 (Panel 자동 숨김)
            base.Awake();

            // 닫기 버튼 설정
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseInventory);
            }
        }

        private void Start()
        {
            // 비동기 초기화
            InitializeAsync().Forget();
        }

        /// <summary>
        /// 비동기 초기화 (PlayerStats를 찾을 때까지 대기)
        /// </summary>
        private async Awaitable InitializeAsync()
        {
            // 시스템 참조 가져오기
            inventorySystem = InventorySystem.Instance;

            if (inventorySystem == null)
            {
                Debug.LogError("[InventoryUI] InventorySystem을 찾을 수 없습니다.");
                return;
            }

            // PlayerStats를 찾을 때까지 재시도 (최대 5초)
            int maxAttempts = 50; // 50 * 0.1초 = 5초
            int attempts = 0;

            while (playerStats == null && attempts < maxAttempts)
            {
                // RunManager 우선
                if (GASPT.Core.RunManager.HasInstance && GASPT.Core.RunManager.Instance.CurrentPlayer != null)
                {
                    playerStats = GASPT.Core.RunManager.Instance.CurrentPlayer;
                }
                // GameManager 차선
                else if (GASPT.Core.GameManager.HasInstance && GASPT.Core.GameManager.Instance.PlayerStats != null)
                {
                    playerStats = GASPT.Core.GameManager.Instance.PlayerStats;
                }

                if (playerStats != null)
                {
                    Debug.Log("[InventoryUI] PlayerStats 찾기 성공!");
                    break;
                }

                await Awaitable.WaitForSecondsAsync(0.1f);
                attempts++;
            }

            if (playerStats == null)
            {
                Debug.LogError("[InventoryUI] PlayerStats를 찾을 수 없습니다. (타임아웃)");
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
                Toggle();
            }
        }


        // ====== 인벤토리 열기/닫기 ======

        /// <summary>
        /// 인벤토리 열기 (BaseUI.Show() override)
        /// </summary>
        public override void Show()
        {
            if (IsVisible)
                return;

            // BaseUI의 Show() 호출 (Panel 활성화)
            base.Show();

            // 아이템 목록 갱신
            RefreshItemList();

            // 장비 슬롯 갱신
            RefreshEquipmentSlots();

            Debug.Log("[InventoryUI] 인벤토리 열기");
        }

        /// <summary>
        /// 인벤토리 닫기 (BaseUI.Hide() override)
        /// </summary>
        public override void Hide()
        {
            if (!IsVisible)
                return;

            // BaseUI의 Hide() 호출 (Panel 비활성화)
            base.Hide();

            Debug.Log("[InventoryUI] 인벤토리 닫기");
        }

        /// <summary>
        /// 인벤토리 열기 (기존 메서드명 호환)
        /// </summary>
        public void OpenInventory()
        {
            Show();
        }

        /// <summary>
        /// 인벤토리 닫기 (기존 메서드명 호환)
        /// </summary>
        public void CloseInventory()
        {
            Hide();
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
                ringSlot.Initialize(EquipmentSlot.Accessory, OnEquipmentSlotClicked);
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
                Item ringItem = playerStats.GetEquippedItem(EquipmentSlot.Accessory);
                ringSlot.SetItem(ringItem);
            }
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 아이템 추가 이벤트 핸들러
        /// </summary>
        private void OnItemAddedHandler(Item item)
        {
            if (IsVisible)
            {
                RefreshItemList();
            }
        }

        /// <summary>
        /// 아이템 제거 이벤트 핸들러
        /// </summary>
        private void OnItemRemovedHandler(Item item)
        {
            if (IsVisible)
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


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Refresh Item List")]
        private void TestRefreshItemList()
        {
            RefreshItemList();
        }

        [ContextMenu("테스트: 무기 더미 아이템 추가")]
        private void TestAddDummyWeapon()
        {
            Item weapon = CreateDummyItem("테스트 검", EquipmentSlot.Weapon, 0, 15, 0);
            if (inventorySystem != null)
            {
                inventorySystem.AddItem(weapon);
                Debug.Log("[InventoryUI] 테스트 무기 아이템 추가 완료");
            }
        }

        [ContextMenu("테스트: 방어구 더미 아이템 추가")]
        private void TestAddDummyArmor()
        {
            Item armor = CreateDummyItem("테스트 갑옷", EquipmentSlot.Armor, 50, 0, 10);
            if (inventorySystem != null)
            {
                inventorySystem.AddItem(armor);
                Debug.Log("[InventoryUI] 테스트 방어구 아이템 추가 완료");
            }
        }

        [ContextMenu("테스트: 악세서리 더미 아이템 추가")]
        private void TestAddDummyAccessory()
        {
            Item accessory = CreateDummyItem("테스트 반지", EquipmentSlot.Accessory, 20, 5, 5);
            if (inventorySystem != null)
            {
                inventorySystem.AddItem(accessory);
                Debug.Log("[InventoryUI] 테스트 악세서리 아이템 추가 완료");
            }
        }

        [ContextMenu("테스트: 모든 종류 더미 아이템 추가")]
        private void TestAddAllDummyItems()
        {
            if (inventorySystem == null)
            {
                Debug.LogError("[InventoryUI] InventorySystem이 없습니다.");
                return;
            }

            // 무기 3개
            inventorySystem.AddItem(CreateDummyItem("초보자의 검", EquipmentSlot.Weapon, 0, 10, 0));
            inventorySystem.AddItem(CreateDummyItem("강철 검", EquipmentSlot.Weapon, 0, 20, 0));
            inventorySystem.AddItem(CreateDummyItem("전설의 검", EquipmentSlot.Weapon, 0, 35, 5));

            // 방어구 3개
            inventorySystem.AddItem(CreateDummyItem("가죽 갑옷", EquipmentSlot.Armor, 30, 0, 5));
            inventorySystem.AddItem(CreateDummyItem("강철 갑옷", EquipmentSlot.Armor, 60, 0, 15));
            inventorySystem.AddItem(CreateDummyItem("드래곤 갑옷", EquipmentSlot.Armor, 100, 0, 30));

            // 악세서리 3개
            inventorySystem.AddItem(CreateDummyItem("힘의 반지", EquipmentSlot.Accessory, 0, 8, 0));
            inventorySystem.AddItem(CreateDummyItem("생명의 반지", EquipmentSlot.Accessory, 40, 0, 0));
            inventorySystem.AddItem(CreateDummyItem("균형의 반지", EquipmentSlot.Accessory, 20, 5, 5));

            Debug.Log("[InventoryUI] 모든 테스트 아이템 추가 완료 (9개)");
        }

        [ContextMenu("테스트: 랜덤 더미 아이템 5개 추가")]
        private void TestAddRandomDummyItems()
        {
            if (inventorySystem == null)
            {
                Debug.LogError("[InventoryUI] InventorySystem이 없습니다.");
                return;
            }

            EquipmentSlot[] slots = { EquipmentSlot.Weapon, EquipmentSlot.Armor, EquipmentSlot.Accessory };
            string[] weaponNames = { "검", "도끼", "창", "활", "지팡이" };
            string[] armorNames = { "갑옷", "로브", "경갑", "중갑", "망토" };
            string[] accessoryNames = { "반지", "목걸이", "팔찌", "귀걸이", "부적" };

            for (int i = 0; i < 5; i++)
            {
                EquipmentSlot randomSlot = slots[Random.Range(0, slots.Length)];
                string itemName = "";
                int hp = Random.Range(0, 50);
                int attack = Random.Range(0, 25);
                int defense = Random.Range(0, 20);

                switch (randomSlot)
                {
                    case EquipmentSlot.Weapon:
                        itemName = $"랜덤 {weaponNames[Random.Range(0, weaponNames.Length)]} {i + 1}";
                        break;
                    case EquipmentSlot.Armor:
                        itemName = $"랜덤 {armorNames[Random.Range(0, armorNames.Length)]} {i + 1}";
                        break;
                    case EquipmentSlot.Accessory:
                        itemName = $"랜덤 {accessoryNames[Random.Range(0, accessoryNames.Length)]} {i + 1}";
                        break;
                }

                inventorySystem.AddItem(CreateDummyItem(itemName, randomSlot, hp, attack, defense));
            }

            Debug.Log("[InventoryUI] 랜덤 테스트 아이템 5개 추가 완료");
        }

        [ContextMenu("테스트: 모든 아이템 제거")]
        private void TestClearAllItems()
        {
            if (inventorySystem == null)
            {
                Debug.LogError("[InventoryUI] InventorySystem이 없습니다.");
                return;
            }

            // 모든 장착 해제
            if (playerStats != null)
            {
                playerStats.UnequipAll();
            }

            // 인벤토리의 모든 아이템 제거
            List<Item> items = inventorySystem.GetItems();
            if (items != null)
            {
                int count = items.Count;
                // 역순으로 제거 (리스트 수정 중 인덱스 문제 방지)
                for (int i = count - 1; i >= 0; i--)
                {
                    inventorySystem.RemoveItem(items[i]);
                }
                Debug.Log($"[InventoryUI] 모든 아이템 제거 완료 ({count}개)");
            }

            RefreshItemList();
            RefreshEquipmentSlots();
        }

        /// <summary>
        /// 런타임 더미 아이템 생성 헬퍼 메서드
        /// </summary>
        private Item CreateDummyItem(string name, EquipmentSlot slot, int hp, int attack, int defense)
        {
            Item item = ScriptableObject.CreateInstance<Item>();
            item.itemName = name;
            item.description = $"테스트용 {slot} 아이템입니다.";
            item.slot = slot;
            item.hpBonus = hp;
            item.attackBonus = attack;
            item.defenseBonus = defense;
            // icon은 null (UI에서 기본 이미지 표시됨)

            return item;
        }
    }
}
