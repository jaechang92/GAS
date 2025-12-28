# API Contracts: 022-item-inventory

**기능명**: 아이템/인벤토리 시스템
**작성일**: 2025-12-28
**목적**: 공개 API 인터페이스 정의

---

## 1. InventoryManager API

### 1.1 클래스 정의

```csharp
namespace GASPT.Inventory
{
    public class InventoryManager : SingletonManager<InventoryManager>, ISaveable
    {
        // Properties
        public int Capacity { get; }
        public int UsedSlots { get; }
        public int FreeSlots { get; }
        public int Gold { get; set; }

        // Events
        public event Action<ItemInstance, int> OnItemAdded;
        public event Action<ItemInstance, int> OnItemRemoved;
        public event Action<int, int> OnGoldChanged;
        public event Action OnInventoryFull;
        public event Action OnInventoryChanged;
    }
}
```

### 1.2 아이템 추가

```csharp
/// <summary>
/// 아이템을 인벤토리에 추가합니다.
/// </summary>
/// <param name="itemData">추가할 아이템 데이터</param>
/// <param name="quantity">수량 (스택 가능 시)</param>
/// <returns>추가된 수량 (0 = 실패)</returns>
public int AddItem(ItemData itemData, int quantity = 1);

/// <summary>
/// 아이템 인스턴스를 인벤토리에 추가합니다.
/// </summary>
/// <param name="instance">아이템 인스턴스</param>
/// <returns>true = 성공</returns>
public bool AddItemInstance(ItemInstance instance);
```

**응답 예시:**
| 상황 | 반환값 | 이벤트 |
|-----|:------:|-------|
| 성공 (3개 추가) | 3 | OnItemAdded(instance, 3) |
| 부분 성공 (5개 요청, 2개만 추가) | 2 | OnItemAdded(instance, 2), OnInventoryFull |
| 실패 (가득 참) | 0 | OnInventoryFull |

### 1.3 아이템 제거

```csharp
/// <summary>
/// 아이템을 인벤토리에서 제거합니다.
/// </summary>
/// <param name="slotIndex">슬롯 인덱스</param>
/// <param name="quantity">제거할 수량</param>
/// <returns>제거된 수량</returns>
public int RemoveItem(int slotIndex, int quantity = 1);

/// <summary>
/// 특정 아이템 인스턴스를 제거합니다.
/// </summary>
/// <param name="instance">제거할 인스턴스</param>
/// <returns>true = 성공</returns>
public bool RemoveItemInstance(ItemInstance instance);
```

### 1.4 아이템 조회

```csharp
/// <summary>
/// 슬롯의 아이템을 조회합니다.
/// </summary>
/// <param name="slotIndex">슬롯 인덱스</param>
/// <returns>인벤토리 슬롯 (null = 유효하지 않은 인덱스)</returns>
public InventorySlot GetSlot(int slotIndex);

/// <summary>
/// 모든 슬롯을 조회합니다.
/// </summary>
/// <returns>읽기 전용 슬롯 배열</returns>
public IReadOnlyList<InventorySlot> GetAllSlots();

/// <summary>
/// 특정 카테고리의 아이템을 조회합니다.
/// </summary>
/// <param name="category">아이템 카테고리</param>
/// <returns>해당 카테고리의 슬롯 목록</returns>
public List<InventorySlot> GetItemsByCategory(ItemCategory category);

/// <summary>
/// 아이템 보유 여부를 확인합니다.
/// </summary>
/// <param name="itemId">아이템 ID</param>
/// <returns>보유 수량 (0 = 없음)</returns>
public int GetItemCount(string itemId);
```

### 1.5 슬롯 관리

```csharp
/// <summary>
/// 아이템을 다른 슬롯으로 이동합니다.
/// </summary>
/// <param name="fromSlot">원본 슬롯</param>
/// <param name="toSlot">대상 슬롯</param>
/// <returns>true = 성공</returns>
public bool MoveItem(int fromSlot, int toSlot);

/// <summary>
/// 두 슬롯의 아이템을 교환합니다.
/// </summary>
/// <param name="slotA">슬롯 A</param>
/// <param name="slotB">슬롯 B</param>
/// <returns>true = 성공</returns>
public bool SwapItems(int slotA, int slotB);

/// <summary>
/// 슬롯을 잠금/해제합니다.
/// </summary>
/// <param name="slotIndex">슬롯 인덱스</param>
/// <param name="locked">잠금 상태</param>
public void SetSlotLocked(int slotIndex, bool locked);

/// <summary>
/// 인벤토리를 정렬합니다.
/// </summary>
/// <param name="sortType">정렬 기준</param>
public void SortInventory(InventorySortType sortType);
```

### 1.6 용량 관리

```csharp
/// <summary>
/// 인벤토리 용량을 확장합니다.
/// </summary>
/// <param name="additionalSlots">추가할 슬롯 수</param>
/// <returns>true = 성공 (최대 용량 미초과)</returns>
public bool ExpandCapacity(int additionalSlots);
```

---

## 2. EquipmentManager API

### 2.1 클래스 정의

```csharp
namespace GASPT.Inventory
{
    public class EquipmentManager : SingletonManager<EquipmentManager>
    {
        // Events
        public event Action<EquipmentSlot, ItemInstance> OnEquipped;
        public event Action<EquipmentSlot, ItemInstance> OnUnequipped;
        public event Action OnEquipmentChanged;
    }
}
```

### 2.2 장비 장착

```csharp
/// <summary>
/// 장비를 장착합니다.
/// </summary>
/// <param name="instance">장착할 아이템 인스턴스</param>
/// <returns>결과 코드</returns>
public EquipResult Equip(ItemInstance instance);

/// <summary>
/// 장비 장착 가능 여부를 확인합니다.
/// </summary>
/// <param name="instance">확인할 아이템 인스턴스</param>
/// <returns>장착 가능 여부 및 사유</returns>
public (bool canEquip, string reason) CanEquip(ItemInstance instance);
```

**EquipResult 열거형:**
```csharp
public enum EquipResult
{
    Success,
    InventoryFull,          // 기존 장비를 인벤토리에 넣을 공간 없음
    LevelTooLow,            // 레벨 부족
    WrongForm,              // 폼 불일치
    InvalidSlot,            // 잘못된 슬롯
    ItemNotOwned            // 아이템 미보유
}
```

### 2.3 장비 해제

```csharp
/// <summary>
/// 장비를 해제합니다.
/// </summary>
/// <param name="slot">해제할 슬롯</param>
/// <returns>true = 성공</returns>
public bool Unequip(EquipmentSlot slot);

/// <summary>
/// 모든 장비를 해제합니다.
/// </summary>
/// <returns>해제된 장비 수</returns>
public int UnequipAll();
```

### 2.4 장비 조회

```csharp
/// <summary>
/// 특정 슬롯의 장착 아이템을 조회합니다.
/// </summary>
/// <param name="slot">조회할 슬롯</param>
/// <returns>장착된 아이템 인스턴스 (null = 빈 슬롯)</returns>
public ItemInstance GetEquipped(EquipmentSlot slot);

/// <summary>
/// 모든 장착 아이템을 조회합니다.
/// </summary>
/// <returns>슬롯-아이템 딕셔너리</returns>
public IReadOnlyDictionary<EquipmentSlot, ItemInstance> GetAllEquipped();
```

---

## 3. ConsumableManager API

### 3.1 클래스 정의

```csharp
namespace GASPT.Inventory
{
    public class ConsumableManager : SingletonManager<ConsumableManager>
    {
        // Events
        public event Action<ItemInstance> OnItemUsed;
        public event Action<ItemInstance, float> OnCooldownStarted;
    }
}
```

### 3.2 아이템 사용

```csharp
/// <summary>
/// 소비 아이템을 사용합니다.
/// </summary>
/// <param name="slotIndex">사용할 아이템의 슬롯 인덱스</param>
/// <returns>결과 코드</returns>
public UseResult UseItem(int slotIndex);

/// <summary>
/// 아이템 사용 가능 여부를 확인합니다.
/// </summary>
/// <param name="instance">확인할 아이템</param>
/// <returns>사용 가능 여부</returns>
public bool CanUse(ItemInstance instance);

/// <summary>
/// 특정 아이템의 쿨다운 잔여 시간을 조회합니다.
/// </summary>
/// <param name="itemId">아이템 ID</param>
/// <returns>잔여 시간 (초), 0 = 사용 가능</returns>
public float GetCooldownRemaining(string itemId);
```

**UseResult 열거형:**
```csharp
public enum UseResult
{
    Success,
    OnCooldown,
    NotConsumable,
    EffectFailed,
    NotOwned
}
```

---

## 4. QuickSlotManager API

### 4.1 클래스 정의

```csharp
namespace GASPT.Inventory
{
    public class QuickSlotManager : SingletonManager<QuickSlotManager>
    {
        public const int MaxQuickSlots = 5;

        // Events
        public event Action<int, ItemInstance> OnQuickSlotChanged;
    }
}
```

### 4.2 퀵슬롯 관리

```csharp
/// <summary>
/// 퀵슬롯에 아이템을 등록합니다.
/// </summary>
/// <param name="quickSlotIndex">퀵슬롯 인덱스 (0-4)</param>
/// <param name="inventorySlotIndex">인벤토리 슬롯 인덱스</param>
/// <returns>true = 성공</returns>
public bool AssignToQuickSlot(int quickSlotIndex, int inventorySlotIndex);

/// <summary>
/// 퀵슬롯을 비웁니다.
/// </summary>
/// <param name="quickSlotIndex">퀵슬롯 인덱스</param>
public void ClearQuickSlot(int quickSlotIndex);

/// <summary>
/// 퀵슬롯 아이템을 사용합니다.
/// </summary>
/// <param name="quickSlotIndex">퀵슬롯 인덱스</param>
/// <returns>사용 결과</returns>
public UseResult UseQuickSlot(int quickSlotIndex);

/// <summary>
/// 퀵슬롯 상태를 조회합니다.
/// </summary>
/// <param name="quickSlotIndex">퀵슬롯 인덱스</param>
/// <returns>등록된 아이템 정보</returns>
public QuickSlotInfo GetQuickSlotInfo(int quickSlotIndex);
```

**QuickSlotInfo 구조체:**
```csharp
public struct QuickSlotInfo
{
    public bool IsEmpty;
    public ItemInstance Item;
    public int Quantity;
    public float CooldownRemaining;
}
```

---

## 5. ItemDropManager API

### 5.1 클래스 정의

```csharp
namespace GASPT.Loot
{
    public class ItemDropManager : SingletonManager<ItemDropManager>
    {
        // Events
        public event Action<ItemInstance, Vector3> OnItemDropped;
        public event Action<ItemInstance> OnItemPickedUp;
    }
}
```

### 5.2 드롭 API

```csharp
/// <summary>
/// 아이템을 월드에 드롭합니다.
/// </summary>
/// <param name="itemData">드롭할 아이템 데이터</param>
/// <param name="position">드롭 위치</param>
/// <param name="quantity">수량</param>
/// <returns>생성된 드롭 아이템</returns>
public DroppedItem DropItem(ItemData itemData, Vector3 position, int quantity = 1);

/// <summary>
/// 드롭 테이블에서 랜덤 아이템을 드롭합니다.
/// </summary>
/// <param name="lootTable">드롭 테이블</param>
/// <param name="position">드롭 위치</param>
public void DropFromTable(LootTable lootTable, Vector3 position);

/// <summary>
/// 드롭된 아이템을 획득합니다.
/// </summary>
/// <param name="droppedItem">획득할 드롭 아이템</param>
/// <returns>획득 결과</returns>
public PickupResult PickupItem(DroppedItem droppedItem);
```

**PickupResult 열거형:**
```csharp
public enum PickupResult
{
    Success,
    InventoryFull,
    AlreadyPickedUp,
    TooFar
}
```

---

## 6. StatCalculator API

### 6.1 클래스 정의

```csharp
namespace GASPT.Stats
{
    public static class StatCalculator
    {
        /// <summary>
        /// 모든 장비의 스탯을 계산합니다.
        /// </summary>
        /// <param name="equippedItems">장착된 아이템 목록</param>
        /// <returns>합산된 스탯 딕셔너리</returns>
        public static Dictionary<StatType, float> CalculateEquipmentStats(
            IEnumerable<ItemInstance> equippedItems);

        /// <summary>
        /// 세트 효과를 계산합니다.
        /// </summary>
        /// <param name="equippedItems">장착된 아이템 목록</param>
        /// <returns>세트 효과 목록</returns>
        public static List<SetBonus> CalculateSetBonuses(
            IEnumerable<ItemInstance> equippedItems);

        /// <summary>
        /// 최종 스탯을 계산합니다.
        /// </summary>
        /// <param name="baseStats">기본 스탯</param>
        /// <param name="modifiers">스탯 수정자 목록</param>
        /// <returns>최종 스탯 값</returns>
        public static float CalculateFinalStat(
            float baseValue,
            IEnumerable<StatModifier> modifiers);
    }
}
```

---

## 7. UI Interfaces

### 7.1 IInventoryView

```csharp
namespace GASPT.UI.MVP
{
    public interface IInventoryView
    {
        // Properties
        bool IsVisible { get; }

        // View Commands
        void ShowUI();
        void HideUI();
        void DisplaySlots(IReadOnlyList<InventorySlotViewModel> slots);
        void DisplayEquipment(EquipmentViewModel equipment);
        void DisplayGold(int amount);
        void ShowError(string message);
        void ShowSuccess(string message);
        void ShowItemTooltip(ItemViewModel item, Vector2 position);
        void HideItemTooltip();

        // View Events
        event Action OnOpenRequested;
        event Action OnCloseRequested;
        event Action<int> OnSlotClicked;
        event Action<int> OnSlotRightClicked;
        event Action<int, int> OnSlotDragDropped;
        event Action<EquipmentSlot> OnEquipmentSlotClicked;
        event Action<InventorySortType> OnSortRequested;
    }
}
```

### 7.2 ViewModels

```csharp
namespace GASPT.UI.MVP
{
    public class InventorySlotViewModel
    {
        public int SlotIndex;
        public bool IsEmpty;
        public bool IsLocked;
        public string ItemName;
        public Sprite Icon;
        public Color RarityColor;
        public int Quantity;
        public bool IsEquipped;
        public bool IsNew;

        public ItemInstance OriginalInstance;
    }

    public class ItemViewModel
    {
        public string Name;
        public string Description;
        public Sprite Icon;
        public ItemCategory Category;
        public ItemRarity Rarity;
        public Color RarityColor;
        public List<string> StatLines;
        public int SellPrice;
        public bool CanEquip;
        public bool CanUse;

        public ItemInstance OriginalInstance;
    }

    public class EquipmentViewModel
    {
        public ItemInstance Weapon;
        public ItemInstance Armor;
        public ItemInstance Helmet;
        public ItemInstance Gloves;
        public ItemInstance Boots;
        public ItemInstance Accessory1;
        public ItemInstance Accessory2;
    }
}
```

---

## 8. Constants

### 8.1 인벤토리 상수

```csharp
namespace GASPT.Inventory
{
    public static class InventoryConstants
    {
        public const int DefaultCapacity = 40;
        public const int ExpandStep = 20;
        public const int MaxCapacity = 100;
        public const int MaxStackSize = 99;
    }
}
```

### 8.2 아이템 상수

```csharp
namespace GASPT.Data
{
    public static class ItemConstants
    {
        public static readonly Color CommonColor = Color.white;
        public static readonly Color UncommonColor = new Color(0.298f, 0.686f, 0.314f);  // #4CAF50
        public static readonly Color RareColor = new Color(0.129f, 0.588f, 0.953f);      // #2196F3
        public static readonly Color EpicColor = new Color(0.612f, 0.153f, 0.690f);      // #9C27B0
        public static readonly Color LegendaryColor = new Color(1f, 0.596f, 0f);          // #FF9800
    }
}
```

---

*작성: Claude Code*
*최종 수정: 2025-12-28*
