# Data Model: 022-item-inventory

**기능명**: 아이템/인벤토리 시스템
**작성일**: 2025-12-28
**목적**: 엔티티 정의 및 데이터 구조 설계

---

## 1. Enums

### 1.1 ItemCategory

```csharp
namespace GASPT.Core.Enums
{
    public enum ItemCategory
    {
        Equipment,    // 장비 (장착)
        Consumable,   // 소비 (사용)
        Material,     // 재료 (보관)
        Currency,     // 재화 (자동 획득)
        Quest         // 퀘스트
    }
}
```

### 1.2 ItemRarity

```csharp
namespace GASPT.Core.Enums
{
    public enum ItemRarity
    {
        Common = 0,     // 일반 - 흰색
        Uncommon = 1,   // 고급 - 녹색
        Rare = 2,       // 희귀 - 파란색
        Epic = 3,       // 영웅 - 보라색
        Legendary = 4   // 전설 - 주황색
    }
}
```

### 1.3 EquipmentSlot (확장)

```csharp
namespace GASPT.Core.Enums
{
    public enum EquipmentSlot
    {
        None = -1,      // 장비 불가
        Weapon = 0,     // 무기
        Armor = 1,      // 방어구
        Helmet = 2,     // 투구
        Gloves = 3,     // 장갑
        Boots = 4,      // 신발
        Accessory1 = 5, // 악세서리 1 (반지)
        Accessory2 = 6  // 악세서리 2 (목걸이)
    }
}
```

### 1.4 ConsumeType

```csharp
namespace GASPT.Core.Enums
{
    public enum ConsumeType
    {
        Heal,           // 즉시 HP 회복
        HealOverTime,   // 지속 HP 회복
        RestoreMana,    // MP 회복
        Buff,           // 버프 적용
        Cleanse,        // 상태이상 해제
        Teleport,       // 이동
        Revive          // 부활
    }
}
```

### 1.5 ModifierType

```csharp
namespace GASPT.Core.Enums
{
    public enum ModifierType
    {
        Flat,       // 고정 수치 (+10)
        Percent     // 퍼센트 (+10%)
    }
}
```

---

## 2. Data Structures

### 2.1 StatModifier

```csharp
namespace GASPT.Data
{
    [Serializable]
    public struct StatModifier
    {
        public StatType statType;
        public ModifierType modifierType;
        public float value;

        public StatModifier(StatType type, ModifierType modType, float val)
        {
            statType = type;
            modifierType = modType;
            value = val;
        }
    }
}
```

**필드 설명:**
| 필드 | 타입 | 설명 |
|-----|------|------|
| statType | StatType | HP, Attack, Defense, Mana, Speed, CriticalRate |
| modifierType | ModifierType | Flat(고정) 또는 Percent(퍼센트) |
| value | float | 수치 값 |

### 2.2 ItemInstance

```csharp
namespace GASPT.Data
{
    [Serializable]
    public class ItemInstance
    {
        public string instanceId;           // GUID
        public string itemDataId;           // ScriptableObject 참조용
        public List<StatModifier> randomStats;  // 랜덤 추가 스탯
        public int currentDurability;       // 현재 내구도 (-1 = 무한)
        public bool isEquipped;             // 장착 여부

        [NonSerialized]
        public ItemData cachedItemData;     // 런타임 캐시
    }
}
```

**필드 설명:**
| 필드 | 타입 | 설명 |
|-----|------|------|
| instanceId | string | 고유 인스턴스 ID (GUID) |
| itemDataId | string | ItemData ScriptableObject 경로 |
| randomStats | List<StatModifier> | 랜덤 생성된 추가 스탯 |
| currentDurability | int | 현재 내구도 (-1 = 내구도 없음) |
| isEquipped | bool | 장착 상태 플래그 |
| cachedItemData | ItemData | 런타임 데이터 캐시 (저장 안함) |

### 2.3 InventorySlot

```csharp
namespace GASPT.Inventory
{
    [Serializable]
    public class InventorySlot
    {
        public int slotIndex;
        public ItemInstance item;           // null = 빈 슬롯
        public int quantity;
        public bool isLocked;

        public bool IsEmpty => item == null;
        public bool IsFull => item != null && quantity >= item.cachedItemData?.maxStack;
    }
}
```

**필드 설명:**
| 필드 | 타입 | 설명 |
|-----|------|------|
| slotIndex | int | 슬롯 인덱스 (0-based) |
| item | ItemInstance | 아이템 인스턴스 (null = 빈 슬롯) |
| quantity | int | 수량 (스택 가능 아이템용) |
| isLocked | bool | 슬롯 잠금 상태 |

---

## 3. ScriptableObject Classes

### 3.1 ItemData (Base)

```csharp
namespace GASPT.Data
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "GASPT/Items/ItemData")]
    public class ItemData : ScriptableObject
    {
        [Header("기본 정보")]
        public string itemId;               // 고유 ID (ITM001)
        public string itemName;             // 표시 이름
        [TextArea(2, 4)]
        public string description;          // 설명
        public Sprite icon;                 // 아이콘

        [Header("분류")]
        public ItemCategory category;
        public ItemRarity rarity;

        [Header("스택")]
        public bool stackable;
        public int maxStack = 1;

        [Header("경제")]
        public int sellPrice;
        public int buyPrice;

        // 등급별 색상 반환
        public Color RarityColor => GetRarityColor();
    }
}
```

**필드 설명:**
| 필드 | 타입 | 기본값 | 설명 |
|-----|------|:------:|------|
| itemId | string | - | 고유 식별자 |
| itemName | string | - | UI 표시 이름 |
| description | string | - | 아이템 설명 |
| icon | Sprite | null | 아이콘 이미지 |
| category | ItemCategory | Equipment | 아이템 카테고리 |
| rarity | ItemRarity | Common | 희귀도 |
| stackable | bool | false | 스택 가능 여부 |
| maxStack | int | 1 | 최대 스택 수 |
| sellPrice | int | 0 | 판매가 |
| buyPrice | int | 0 | 구매가 |

### 3.2 EquipmentData

```csharp
namespace GASPT.Data
{
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "GASPT/Items/EquipmentData")]
    public class EquipmentData : ItemData
    {
        [Header("장비 정보")]
        public EquipmentSlot equipSlot;
        public int requiredLevel = 1;
        public FormType requiredForm = FormType.None;

        [Header("기본 스탯")]
        public List<StatModifier> baseStats;

        [Header("추가 스탯 생성")]
        public int minRandomStats = 0;
        public int maxRandomStats = 0;
        public List<StatType> possibleRandomStats;

        [Header("세트 아이템")]
        public string setId;                // 빈 문자열 = 세트 아님

        [Header("내구도")]
        public int maxDurability = -1;      // -1 = 내구도 없음
    }
}
```

**필드 설명:**
| 필드 | 타입 | 기본값 | 설명 |
|-----|------|:------:|------|
| equipSlot | EquipmentSlot | Weapon | 장착 슬롯 |
| requiredLevel | int | 1 | 착용 레벨 제한 |
| requiredForm | FormType | None | 착용 폼 제한 |
| baseStats | List<StatModifier> | [] | 기본 스탯 목록 |
| minRandomStats | int | 0 | 최소 랜덤 스탯 수 |
| maxRandomStats | int | 0 | 최대 랜덤 스탯 수 |
| possibleRandomStats | List<StatType> | [] | 가능한 랜덤 스탯 종류 |
| setId | string | "" | 세트 아이템 ID |
| maxDurability | int | -1 | 최대 내구도 |

### 3.3 ConsumableData

```csharp
namespace GASPT.Data
{
    [CreateAssetMenu(fileName = "ConsumableData", menuName = "GASPT/Items/ConsumableData")]
    public class ConsumableData : ItemData
    {
        [Header("소비 효과")]
        public ConsumeType consumeType;
        public float effectValue;
        public float effectDuration;        // 0 = 즉시
        public float cooldown;

        [Header("버프 효과 (Buff 타입용)")]
        public StatusEffectData buffEffect;
    }
}
```

**필드 설명:**
| 필드 | 타입 | 기본값 | 설명 |
|-----|------|:------:|------|
| consumeType | ConsumeType | Heal | 효과 타입 |
| effectValue | float | 0 | 효과 수치 |
| effectDuration | float | 0 | 지속 시간 (0 = 즉시) |
| cooldown | float | 0 | 재사용 대기시간 |
| buffEffect | StatusEffectData | null | 버프 효과 데이터 |

---

## 4. Save Data Structures

### 4.1 InventoryData (확장)

```csharp
namespace GASPT.Save
{
    [Serializable]
    public class InventoryData
    {
        public int version = 2;             // 데이터 버전
        public int capacity = 40;           // 인벤토리 용량
        public List<InventorySlotData> slots;
        public int gold;                    // 보유 골드

        // 레거시 호환 (v1)
        public List<string> itemPaths;
    }
}
```

### 4.2 InventorySlotData

```csharp
namespace GASPT.Save
{
    [Serializable]
    public class InventorySlotData
    {
        public int slotIndex;
        public string itemDataPath;         // ScriptableObject 경로
        public int quantity;
        public bool isLocked;

        // ItemInstance 데이터
        public string instanceId;
        public List<StatModifierData> randomStats;
        public int currentDurability;
    }
}
```

### 4.3 StatModifierData

```csharp
namespace GASPT.Save
{
    [Serializable]
    public class StatModifierData
    {
        public int statType;                // enum -> int
        public int modifierType;            // enum -> int
        public float value;
    }
}
```

### 4.4 EquipmentData (저장용)

```csharp
namespace GASPT.Save
{
    [Serializable]
    public class PlayerEquipmentData
    {
        public int version = 2;
        public List<EquippedItemEntry> equippedItems;
    }
}

[Serializable]
public class EquippedItemEntry
{
    public int slot;                        // EquipmentSlot enum -> int
    public string instanceId;               // ItemInstance ID
}
```

---

## 5. Entity Relationships

```
┌─────────────────────────────────────────────────────────────────┐
│                        ScriptableObject Layer                   │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  ItemData (Base)                                          │ │
│  │    ├── EquipmentData                                      │ │
│  │    │     └── baseStats: List<StatModifier>                │ │
│  │    └── ConsumableData                                     │ │
│  │          └── buffEffect: StatusEffectData                 │ │
│  └───────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                         Runtime Layer                           │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  ItemInstance                                              │ │
│  │    ├── cachedItemData → ItemData (참조)                   │ │
│  │    └── randomStats: List<StatModifier>                    │ │
│  └───────────────────────────────────────────────────────────┘ │
│                              ▼                                  │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  InventorySlot                                             │ │
│  │    └── item → ItemInstance                                │ │
│  └───────────────────────────────────────────────────────────┘ │
│                              ▼                                  │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  InventoryManager                                          │ │
│  │    └── slots: InventorySlot[]                              │ │
│  └───────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Integration Layer                         │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  PlayerStats                                               │ │
│  │    └── equippedItems: Dictionary<EquipmentSlot, ItemInstance> │
│  └───────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

---

## 6. Validation Rules

### 6.1 ItemData 검증

| 필드 | 규칙 |
|-----|------|
| itemId | 필수, 고유, "ITM" 접두사 권장 |
| itemName | 필수, 비어있지 않음 |
| maxStack | stackable=true일 때 1 이상 |
| sellPrice | 0 이상 |

### 6.2 EquipmentData 검증

| 필드 | 규칙 |
|-----|------|
| equipSlot | None이 아닌 값 |
| requiredLevel | 1 이상 |
| baseStats | 최소 1개 이상 권장 |
| maxRandomStats | minRandomStats 이상 |

### 6.3 ConsumableData 검증

| 필드 | 규칙 |
|-----|------|
| effectValue | Heal/RestoreMana: 0 초과 |
| effectDuration | HealOverTime/Buff: 0 초과 |
| buffEffect | Buff 타입일 때 필수 |

### 6.4 ItemInstance 검증

| 필드 | 규칙 |
|-----|------|
| instanceId | GUID 형식, 고유 |
| itemDataId | 유효한 ScriptableObject 경로 |

### 6.5 InventorySlot 검증

| 필드 | 규칙 |
|-----|------|
| slotIndex | 0 이상, 용량 미만 |
| quantity | 1 이상, maxStack 이하 |

---

## 7. State Transitions

### 7.1 아이템 생명주기

```
[Drop/Loot]
    ↓
ItemData → ItemInstance 생성
    ↓
[Pickup]
    ↓
InventorySlot에 할당
    ↓
[Equip] ←→ [Unequip]
    ↓
PlayerStats.equippedItems
    ↓
[Sell/Drop/Destroy]
    ↓
ItemInstance 해제
```

### 7.2 장비 상태

```
InInventory ←→ Equipped
     ↓
  Destroyed
```

### 7.3 인벤토리 슬롯 상태

```
Empty → HasItem
  ↑        ↓
  └── Cleared
```

---

*작성: Claude Code*
*최종 수정: 2025-12-28*
