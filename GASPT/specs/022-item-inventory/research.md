# Research: 022-item-inventory

**기능명**: 아이템/인벤토리 시스템
**작성일**: 2025-12-28
**목적**: 구현 전 기술 리서치 및 의사결정 문서화

---

## 1. 기존 시스템 분석

### 1.1 현재 아이템 시스템 상태

| 컴포넌트 | 상태 | 파일 경로 |
|---------|------|----------|
| Item.cs | 기본 구현 | `Assets/_Project/Scripts/Data/Item.cs` |
| InventorySystem.cs | 기본 구현 | `Assets/_Project/Scripts/Inventory/InventorySystem.cs` |
| PlayerStats.cs | 장비 장착 로직 존재 | `Assets/_Project/Scripts/Stats/PlayerStats.cs` |
| LootSystem.cs | 드롭 시스템 기본 구현 | `Assets/_Project/Scripts/Loot/LootSystem.cs` |
| LootTable.cs | 드롭 테이블 ScriptableObject | `Assets/_Project/Scripts/Loot/LootTable.cs` |
| DroppedItem.cs | 월드 드롭 아이템 | `Assets/_Project/Scripts/Loot/DroppedItem.cs` |
| InventoryView.cs | MVP View | `Assets/_Project/Scripts/UI/MVP/Views/InventoryView.cs` |
| InventoryPresenter.cs | MVP Presenter | `Assets/_Project/Scripts/UI/MVP/Presenters/InventoryPresenter.cs` |

### 1.2 기존 Item.cs 분석

**현재 구조:**
```csharp
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public EquipmentSlot slot;  // Weapon, Armor, Accessory
    public int hpBonus;
    public int attackBonus;
    public int defenseBonus;
}
```

**한계점:**
- 장비 아이템만 지원 (소비/재료/재화 미지원)
- 아이템 등급(Rarity) 없음
- 스택 기능 없음
- 랜덤 스탯 미지원
- 착용 조건 (레벨, 폼) 없음

### 1.3 현재 EquipmentSlot enum

```csharp
public enum EquipmentSlot
{
    Weapon,
    Armor,
    Accessory
}
```

**한계점:**
- 슬롯이 3개로 제한
- spec에서 요구하는 7개 슬롯 부족 (Weapon, Armor, Helmet, Gloves, Boots, Accessory1, Accessory2)

### 1.4 현재 인벤토리 시스템

**기능:**
- 아이템 추가/제거
- ISaveable 구현으로 저장 지원
- 이벤트 기반 알림 (OnItemAdded, OnItemRemoved)

**한계점:**
- 슬롯 기반 구조 없음 (단순 List<Item>)
- 스택 기능 없음
- 인벤토리 용량 제한 없음
- 정렬/필터링 기능 없음

---

## 2. 의사결정 로그

### 2.1 기존 Item.cs 확장 vs 신규 구조

**결정**: 기존 Item.cs를 확장하고, 새 ItemData 베이스 클래스 도입

**근거:**
- 기존 코드 호환성 유지 (PlayerStats의 EquipItem 메서드)
- 점진적 마이그레이션 가능
- ScriptableObject 상속 구조 활용

**대안 고려:**
1. 기존 Item.cs를 직접 수정 → 호환성 문제 가능
2. 완전 새 시스템 → 기존 코드 폐기 필요
3. 래퍼 패턴 → 복잡도 증가

### 2.2 EquipmentSlot enum 확장

**결정**: 기존 EquipmentSlot을 확장하여 7개 슬롯 지원

**새 구조:**
```csharp
public enum EquipmentSlot
{
    Weapon,
    Armor,
    Helmet,
    Gloves,
    Boots,
    Accessory1,
    Accessory2
}
```

**호환성 처리:**
- 기존 `Accessory` → `Accessory1`으로 마이그레이션
- PlayerStats의 장비 딕셔너리 자동 확장

### 2.3 StatModifier 구조

**결정**: 새 StatModifier 구조체 도입

```csharp
[Serializable]
public struct StatModifier
{
    public StatType statType;
    public ModifierType modifierType;  // Flat, Percent
    public float value;
}
```

**근거:**
- 유연한 스탯 시스템
- 기존 StatType enum 활용
- 퍼센트 보너스 지원

### 2.4 인벤토리 슬롯 시스템

**결정**: InventorySlot 클래스 기반 슬롯 시스템 도입

**근거:**
- 스택 가능 아이템 지원
- 슬롯 잠금 기능
- UI와 1:1 매핑 용이

### 2.5 아이템 인스턴스 시스템

**결정**: ItemInstance 클래스로 런타임 인스턴스 관리

**근거:**
- ScriptableObject(템플릿) + 런타임 인스턴스 분리
- 랜덤 추가 스탯 지원
- 고유 ID로 저장/로드 용이

---

## 3. 기술 스택 결정

### 3.1 데이터 저장

| 항목 | 결정 | 근거 |
|-----|------|------|
| 아이템 정의 | ScriptableObject | 기존 방식 유지, 에디터 친화적 |
| 인벤토리 저장 | JSON (SaveSystem) | 기존 ISaveable 인터페이스 활용 |
| 아이템 인스턴스 ID | GUID | 고유성 보장 |

### 3.2 UI 아키텍처

| 항목 | 결정 | 근거 |
|-----|------|------|
| 패턴 | MVP | 기존 프로젝트 패턴 유지 |
| 슬롯 UI | 풀링 적용 | 성능 최적화 |
| 툴팁 | 동적 생성 | 필요시에만 생성 |

### 3.3 드롭 시스템

| 항목 | 결정 | 근거 |
|-----|------|------|
| 드롭 테이블 | 기존 LootTable 확장 | 호환성 유지 |
| 월드 아이템 | PoolManager 활용 | 성능 최적화 |
| 자동 획득 | Trigger Collider | 기존 DroppedItem 방식 유지 |

---

## 4. 리스크 분석

### 4.1 호환성 리스크

| 리스크 | 영향도 | 대응 방안 |
|-------|:-----:|---------|
| EquipmentSlot 변경 | 중 | 기존 값 마이그레이션 스크립트 |
| Item 구조 변경 | 고 | 기존 Item을 상속하여 호환성 유지 |
| SaveData 구조 변경 | 중 | 버전 관리 및 마이그레이션 로직 |

### 4.2 성능 리스크

| 리스크 | 영향도 | 대응 방안 |
|-------|:-----:|---------|
| 대량 아이템 슬롯 렌더링 | 중 | UI 풀링, 가상 스크롤 |
| 스탯 재계산 빈도 | 저 | Dirty flag 패턴 (기존 PlayerStats 방식) |
| 드롭 아이템 스폰 | 저 | PoolManager 활용 |

---

## 5. 구현 순서 결정

### Phase 1: 데이터 구조 (기반)
1. ItemCategory, ItemRarity enum
2. EquipmentSlot 확장
3. StatModifier 구조체
4. ItemData 기본 클래스 (ScriptableObject)
5. EquipmentData, ConsumableData 파생 클래스
6. ItemInstance 클래스

### Phase 2: 인벤토리 코어
1. InventorySlot 클래스
2. InventoryManager 확장 (기존 InventorySystem 리팩토링)
3. 슬롯 기반 아이템 관리
4. 인벤토리 용량 시스템
5. ISaveable 구현 업데이트

### Phase 3: 장비 시스템
1. PlayerStats 장비 로직 업데이트
2. StatCalculator 클래스 분리
3. 7 슬롯 장비 시스템
4. 착용 조건 검증

### Phase 4: 소비 아이템 & 드롭
1. ConsumableItem 사용 시스템
2. QuickSlotManager
3. LootTable/LootEntry 확장
4. ItemDropManager

### Phase 5: UI
1. InventoryView 확장
2. ItemSlotUI 컴포넌트
3. ItemTooltip
4. EquipmentPanel 확장

### Phase 6: 폴리싱
1. 아이템 정렬/필터링
2. 세트 아이템 효과
3. 테스트 아이템 에셋 생성

---

## 6. 참고 자료

### 6.1 기존 코드 패턴
- SingletonManager 패턴: `GASPT.Core.SingletonManager<T>`
- ISaveable 인터페이스: `GASPT.Save.ISaveable`
- MVP 패턴: IView + Presenter 구조
- PoolManager: `GASPT.Core.Pooling.PoolManager`

### 6.2 네임스페이스 구조
```
GASPT.Data         - 데이터 클래스 (Item, ItemData)
GASPT.Inventory    - 인벤토리 시스템
GASPT.Stats        - 플레이어 스탯
GASPT.Loot         - 드롭 시스템
GASPT.Core.Enums   - 공통 enum
GASPT.UI.MVP       - UI 컴포넌트
```

---

*작성: Claude Code*
*최종 수정: 2025-12-28*
