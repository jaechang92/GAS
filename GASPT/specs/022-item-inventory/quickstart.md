# Quickstart Guide: 022-item-inventory

**기능명**: 아이템/인벤토리 시스템
**작성일**: 2025-12-28
**목적**: 신규 개발자를 위한 빠른 시작 가이드

---

## 1. 아키텍처 개요

```
┌─────────────────────────────────────────────────────────────┐
│                         UI Layer                            │
│  InventoryView ─── InventoryPresenter ─── ViewModels        │
└────────────────────────────┬────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────┐
│                       Manager Layer                         │
│  InventoryManager   EquipmentManager   ConsumableManager    │
│  QuickSlotManager   ItemDropManager    StatCalculator       │
└────────────────────────────┬────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────┐
│                        Data Layer                           │
│  ItemData (SO)   ItemInstance   InventorySlot               │
│  EquipmentData   ConsumableData   LootTable                 │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. 핵심 컴포넌트

### 2.1 데이터 정의 (ScriptableObject)

| 클래스 | 용도 | 파일 위치 |
|-------|------|----------|
| ItemData | 아이템 기본 데이터 | `Scripts/Data/ItemData.cs` |
| EquipmentData | 장비 아이템 데이터 | `Scripts/Data/EquipmentData.cs` |
| ConsumableData | 소비 아이템 데이터 | `Scripts/Data/ConsumableData.cs` |
| LootTable | 드롭 테이블 | `Scripts/Loot/LootTable.cs` |

### 2.2 런타임 객체

| 클래스 | 용도 | 파일 위치 |
|-------|------|----------|
| ItemInstance | 아이템 인스턴스 | `Scripts/Data/ItemInstance.cs` |
| InventorySlot | 인벤토리 슬롯 | `Scripts/Inventory/InventorySlot.cs` |

### 2.3 매니저

| 클래스 | 용도 | 파일 위치 |
|-------|------|----------|
| InventoryManager | 인벤토리 관리 | `Scripts/Inventory/InventoryManager.cs` |
| EquipmentManager | 장비 관리 | `Scripts/Inventory/EquipmentManager.cs` |
| ConsumableManager | 소비 아이템 | `Scripts/Inventory/ConsumableManager.cs` |
| QuickSlotManager | 퀵슬롯 관리 | `Scripts/Inventory/QuickSlotManager.cs` |
| ItemDropManager | 드롭 관리 | `Scripts/Loot/ItemDropManager.cs` |

---

## 3. 빠른 시작 예제

### 3.1 아이템 데이터 생성 (에디터)

1. Project 창에서 우클릭
2. `Create > GASPT > Items > EquipmentData`
3. Inspector에서 데이터 입력:
   - itemId: "WPN001"
   - itemName: "철검"
   - category: Equipment
   - rarity: Common
   - equipSlot: Weapon
   - baseStats: Attack +10

### 3.2 아이템 추가 (코드)

```csharp
// EquipmentData 로드
EquipmentData swordData = Resources.Load<EquipmentData>("Items/WPN001_IronSword");

// 인벤토리에 추가
int addedCount = InventoryManager.Instance.AddItem(swordData, 1);

if (addedCount > 0)
{
    Debug.Log("아이템 추가 성공!");
}
```

### 3.3 아이템 장착 (코드)

```csharp
// 인벤토리에서 슬롯 0번 아이템 가져오기
InventorySlot slot = InventoryManager.Instance.GetSlot(0);

if (slot != null && !slot.IsEmpty)
{
    // 장착 시도
    EquipResult result = EquipmentManager.Instance.Equip(slot.item);

    if (result == EquipResult.Success)
    {
        Debug.Log("장착 성공!");
    }
    else
    {
        Debug.Log($"장착 실패: {result}");
    }
}
```

### 3.4 소비 아이템 사용 (코드)

```csharp
// 슬롯 5번의 포션 사용
UseResult result = ConsumableManager.Instance.UseItem(5);

if (result == UseResult.Success)
{
    Debug.Log("포션 사용 완료!");
}
else if (result == UseResult.OnCooldown)
{
    float remaining = ConsumableManager.Instance.GetCooldownRemaining("POT001");
    Debug.Log($"쿨다운 중: {remaining:F1}초 남음");
}
```

### 3.5 퀵슬롯 설정 (코드)

```csharp
// 인벤토리 5번 슬롯을 퀵슬롯 1번에 등록
QuickSlotManager.Instance.AssignToQuickSlot(0, 5);

// 퀵슬롯 1번 사용 (단축키 1)
QuickSlotManager.Instance.UseQuickSlot(0);
```

### 3.6 아이템 드롭 (코드)

```csharp
// 적 사망 위치에서 드롭 테이블로 아이템 드롭
LootTable enemyLoot = enemy.GetLootTable();
ItemDropManager.Instance.DropFromTable(enemyLoot, enemy.transform.position);
```

---

## 4. 이벤트 구독 예제

### 4.1 인벤토리 변경 감지

```csharp
void Start()
{
    InventoryManager.Instance.OnItemAdded += HandleItemAdded;
    InventoryManager.Instance.OnInventoryFull += HandleInventoryFull;
}

void OnDestroy()
{
    if (InventoryManager.HasInstance)
    {
        InventoryManager.Instance.OnItemAdded -= HandleItemAdded;
        InventoryManager.Instance.OnInventoryFull -= HandleInventoryFull;
    }
}

void HandleItemAdded(ItemInstance item, int quantity)
{
    Debug.Log($"{item.cachedItemData.itemName} x{quantity} 획득!");
    // UI 업데이트, 효과음 재생 등
}

void HandleInventoryFull()
{
    Debug.Log("인벤토리가 가득 찼습니다!");
    // 경고 메시지 표시
}
```

### 4.2 장비 변경 감지

```csharp
void Start()
{
    EquipmentManager.Instance.OnEquipmentChanged += HandleEquipmentChanged;
}

void HandleEquipmentChanged()
{
    // 캐릭터 외형 업데이트
    // 스탯 UI 갱신
}
```

---

## 5. 저장/로드

### 5.1 자동 저장 (ISaveable)

InventoryManager는 ISaveable을 구현하므로 SaveManager에 자동 등록됩니다.

```csharp
// 게임 저장 시 자동 호출
SaveSystem.Instance.SaveGame();

// 게임 로드 시 자동 복원
SaveSystem.Instance.LoadGame();
```

### 5.2 수동 데이터 접근

```csharp
// 현재 인벤토리 데이터 가져오기
InventoryData data = InventoryManager.Instance.GetSaveData();

// 데이터 복원
InventoryManager.Instance.LoadFromSaveData(data);
```

---

## 6. UI 연동 (MVP)

### 6.1 Presenter 초기화

```csharp
public class InventoryPresenter
{
    private IInventoryView view;
    private InventoryManager inventory;

    public void Initialize()
    {
        inventory = InventoryManager.Instance;

        // Model 이벤트 구독
        inventory.OnInventoryChanged += RefreshView;

        // View 이벤트 구독
        view.OnSlotClicked += HandleSlotClick;
    }

    void RefreshView()
    {
        var slots = ConvertToViewModels(inventory.GetAllSlots());
        view.DisplaySlots(slots);
    }
}
```

### 6.2 ViewModel 변환

```csharp
InventorySlotViewModel ConvertToViewModel(InventorySlot slot)
{
    return new InventorySlotViewModel
    {
        SlotIndex = slot.slotIndex,
        IsEmpty = slot.IsEmpty,
        ItemName = slot.item?.cachedItemData?.itemName ?? "",
        Icon = slot.item?.cachedItemData?.icon,
        RarityColor = GetRarityColor(slot.item?.cachedItemData?.rarity),
        Quantity = slot.quantity,
        IsEquipped = slot.item?.isEquipped ?? false,
        OriginalInstance = slot.item
    };
}
```

---

## 7. 디버그 명령

### 7.1 Context Menu

Inspector에서 InventoryManager 오른쪽 클릭:
- `Print Inventory` - 인벤토리 내용 출력
- `Clear Inventory (Test)` - 인벤토리 비우기
- `Add Test Item` - 테스트 아이템 추가

### 7.2 콘솔 명령 (개발 중)

```csharp
// TODO: 콘솔 명령 구현 예정
// /additem WPN001 - 아이템 추가
// /clearinv - 인벤토리 비우기
// /setgold 9999 - 골드 설정
```

---

## 8. 주의사항

### 8.1 인스턴스 관리
- ItemInstance는 런타임에 생성되는 객체입니다.
- ScriptableObject(ItemData)와 혼동하지 마세요.
- 저장 시 ItemData 경로와 랜덤 스탯이 별도로 저장됩니다.

### 8.2 싱글톤 접근
- `HasInstance` 프로퍼티로 존재 여부 확인 후 접근하세요.
- OnDestroy에서 이벤트 구독 해제 시 주의하세요.

### 8.3 스레드 안전성
- 모든 Manager는 메인 스레드에서만 호출하세요.
- 비동기 작업 후 UI 업데이트는 메인 스레드에서 수행하세요.

---

## 9. 관련 문서

- [spec.md](./spec.md) - 기능 명세서
- [data-model.md](./data-model.md) - 데이터 모델
- [contracts/inventory-api.md](./contracts/inventory-api.md) - API 계약
- [research.md](./research.md) - 기술 리서치

---

*작성: Claude Code*
*최종 수정: 2025-12-28*
