# Implementation Plan: 022-item-inventory

**기능명**: 아이템/인벤토리 시스템
**브랜치**: `022-item-inventory`
**작성일**: 2025-12-28
**상태**: 계획 완료

---

## 1. 기술 컨텍스트

### 1.1 기존 시스템 현황

| 컴포넌트 | 상태 | 비고 |
|---------|:----:|------|
| Item.cs | 확장 필요 | 장비만 지원, 카테고리/등급 없음 |
| InventorySystem.cs | 리팩토링 필요 | 슬롯 구조 없음, 스택 미지원 |
| PlayerStats.cs | 일부 수정 | 7슬롯 확장, StatModifier 연동 |
| LootSystem.cs | 확장 필요 | ItemInstance 지원 필요 |
| InventoryView.cs (MVP) | 확장 필요 | 새 ViewModel 연동 |

### 1.2 의존성

| 시스템 | 용도 | 상태 |
|-------|------|:----:|
| PlayerStats | 스탯 적용 | 존재 |
| SaveSystem | 저장/로드 | 존재 |
| PoolManager | 드롭 아이템 풀링 | 존재 |
| UISystem (MVP) | 인벤토리 UI | 존재 |
| StatusEffectManager | 소비 아이템 버프 | 존재 |

### 1.3 신규 파일 목록

```
Assets/_Project/Scripts/
├── Core/Enums/
│   ├── ItemCategory.cs (신규)
│   ├── ItemRarity.cs (신규)
│   ├── ConsumeType.cs (신규)
│   ├── ModifierType.cs (신규)
│   └── EquipmentSlot.cs (수정)
├── Data/
│   ├── ItemData.cs (신규 - 기존 Item.cs 대체)
│   ├── EquipmentData.cs (신규)
│   ├── ConsumableData.cs (신규)
│   ├── ItemInstance.cs (신규)
│   └── StatModifier.cs (신규)
├── Inventory/
│   ├── InventorySlot.cs (신규)
│   ├── InventoryManager.cs (InventorySystem 리팩토링)
│   ├── EquipmentManager.cs (신규)
│   ├── ConsumableManager.cs (신규)
│   ├── QuickSlotManager.cs (신규)
│   └── InventorySortType.cs (신규)
├── Loot/
│   └── ItemDropManager.cs (LootSystem 확장)
├── Stats/
│   └── StatCalculator.cs (신규)
└── UI/MVP/
    ├── ViewModels/InventorySlotViewModel.cs (신규)
    └── Views/ItemTooltip.cs (신규)
```

---

## 2. Constitution Check

### 2.1 프로젝트 원칙 준수

| 원칙 | 적용 방법 | 준수 |
|-----|----------|:----:|
| 완성 우선 | 기본 기능 먼저, 세트 효과는 Phase 6 | ✓ |
| 단계적 개발 | 6 Phase로 분리, 각 단계 테스트 | ✓ |
| 생산성 우선 | 기존 시스템 확장, 새 추상화 최소화 | ✓ |
| 플레이어 경험 | 직관적 UI, 빠른 아이템 사용 | ✓ |
| OOP/SOLID | 단일 책임, 인터페이스 분리 | ✓ |

### 2.2 코딩 표준

| 규칙 | 적용 |
|-----|:----:|
| 카멜케이스 (언더스코어 금지) | ✓ |
| 500줄 이하 분할 | ✓ |
| 한글 주석 | ✓ |
| Awaitable 사용 (Coroutine 금지) | ✓ |
| Unity 6.0 호환 API | ✓ |

---

## 3. Phase별 구현 계획

### Phase 1: 데이터 구조 (기반)

**목표**: 아이템 데이터 구조 정의

| 태스크 | 파일 | 의존성 |
|-------|------|--------|
| 1.1 ItemCategory enum | `Core/Enums/ItemCategory.cs` | 없음 |
| 1.2 ItemRarity enum | `Core/Enums/ItemRarity.cs` | 없음 |
| 1.3 ConsumeType enum | `Core/Enums/ConsumeType.cs` | 없음 |
| 1.4 ModifierType enum | `Core/Enums/ModifierType.cs` | 없음 |
| 1.5 EquipmentSlot 확장 | `Core/Enums/EquipmentSlot.cs` | 없음 |
| 1.6 StatModifier 구조체 | `Data/StatModifier.cs` | 1.4 |
| 1.7 ItemData 기본 클래스 | `Data/ItemData.cs` | 1.1, 1.2 |
| 1.8 EquipmentData 클래스 | `Data/EquipmentData.cs` | 1.5, 1.6, 1.7 |
| 1.9 ConsumableData 클래스 | `Data/ConsumableData.cs` | 1.3, 1.7 |
| 1.10 ItemInstance 클래스 | `Data/ItemInstance.cs` | 1.6, 1.7 |

**예상 파일 수**: 10개
**예상 LOC**: ~500

---

### Phase 2: 인벤토리 코어

**목표**: 슬롯 기반 인벤토리 시스템

| 태스크 | 파일 | 의존성 |
|-------|------|--------|
| 2.1 InventorySlot 클래스 | `Inventory/InventorySlot.cs` | Phase 1 |
| 2.2 InventorySortType enum | `Inventory/InventorySortType.cs` | 없음 |
| 2.3 InventoryManager 리팩토링 | `Inventory/InventoryManager.cs` | 2.1 |
| 2.4 인벤토리 용량 시스템 | InventoryManager 내 | 2.3 |
| 2.5 아이템 정렬/이동 | InventoryManager 내 | 2.3 |
| 2.6 ISaveable 업데이트 | InventoryManager 내 | 2.3 |
| 2.7 InventoryData 확장 | `Save/InventoryData.cs` | 2.1 |

**예상 파일 수**: 4개
**예상 LOC**: ~600

---

### Phase 3: 장비 시스템

**목표**: 7슬롯 장비 장착 및 스탯 계산

| 태스크 | 파일 | 의존성 |
|-------|------|--------|
| 3.1 StatCalculator 클래스 | `Stats/StatCalculator.cs` | Phase 1 |
| 3.2 EquipmentManager 클래스 | `Inventory/EquipmentManager.cs` | 3.1 |
| 3.3 착용 조건 검증 | EquipmentManager 내 | 3.2 |
| 3.4 PlayerStats 장비 로직 업데이트 | `Stats/PlayerStats.cs` | 3.1, 3.2 |
| 3.5 7슬롯 장비 시스템 | PlayerStats 내 | 3.4 |
| 3.6 장비 저장 데이터 확장 | `Save/PlayerEquipmentData.cs` | 3.4 |

**예상 파일 수**: 3개 (+ 수정 2개)
**예상 LOC**: ~500

---

### Phase 4: 소비 아이템 & 드롭

**목표**: 소비 아이템 사용 및 드롭 시스템

| 태스크 | 파일 | 의존성 |
|-------|------|--------|
| 4.1 ConsumableManager 클래스 | `Inventory/ConsumableManager.cs` | Phase 1 |
| 4.2 쿨다운 시스템 | ConsumableManager 내 | 4.1 |
| 4.3 QuickSlotManager 클래스 | `Inventory/QuickSlotManager.cs` | 4.1 |
| 4.4 ItemDropManager 클래스 | `Loot/ItemDropManager.cs` | Phase 2 |
| 4.5 LootTable/LootEntry 확장 | `Loot/LootTable.cs` | 4.4 |
| 4.6 DroppedItem 확장 | `Loot/DroppedItem.cs` | 4.4 |

**예상 파일 수**: 3개 (+ 수정 3개)
**예상 LOC**: ~600

---

### Phase 5: UI

**목표**: MVP 패턴 인벤토리 UI

| 태스크 | 파일 | 의존성 |
|-------|------|--------|
| 5.1 InventorySlotViewModel | `UI/MVP/ViewModels/InventorySlotViewModel.cs` | Phase 2 |
| 5.2 IInventoryView 확장 | `UI/MVP/Interfaces/IInventoryView.cs` | 5.1 |
| 5.3 InventoryView 확장 | `UI/MVP/Views/InventoryView.cs` | 5.2 |
| 5.4 InventoryPresenter 확장 | `UI/MVP/Presenters/InventoryPresenter.cs` | 5.3 |
| 5.5 ItemSlotUI 컴포넌트 | `UI/Components/ItemSlotUI.cs` | 5.1 |
| 5.6 ItemTooltip 컴포넌트 | `UI/MVP/Views/ItemTooltip.cs` | 5.1 |
| 5.7 EquipmentPanel 확장 | `UI/Components/EquipmentSlotUI.cs` | Phase 3 |

**예상 파일 수**: 4개 (+ 수정 3개)
**예상 LOC**: ~700

---

### Phase 6: 폴리싱

**목표**: 추가 기능 및 밸런싱

| 태스크 | 파일 | 의존성 |
|-------|------|--------|
| 6.1 아이템 필터링 | InventoryManager 내 | Phase 5 |
| 6.2 세트 아이템 시스템 | `Data/SetItemData.cs` | Phase 3 |
| 6.3 세트 효과 계산 | StatCalculator 내 | 6.2 |
| 6.4 테스트 아이템 에셋 생성 | `Resources/Items/` | 전체 |
| 6.5 에디터 도구 | `Editor/ItemAssetGenerator.cs` | 6.4 |
| 6.6 통합 테스트 | - | 전체 |

**예상 파일 수**: 2개 (+ 수정 2개, + 에셋 20개)
**예상 LOC**: ~300

---

## 4. 리스크 및 대응

### 4.1 기술적 리스크

| 리스크 | 확률 | 영향 | 대응 |
|-------|:----:|:----:|------|
| 기존 Item.cs 호환성 | 중 | 고 | 상속/어댑터 패턴으로 점진적 마이그레이션 |
| EquipmentSlot 변경 | 중 | 중 | 마이그레이션 스크립트 준비 |
| SaveData 호환성 | 중 | 중 | 버전 관리 및 마이그레이션 로직 |

### 4.2 일정 리스크

| 리스크 | 확률 | 영향 | 대응 |
|-------|:----:|:----:|------|
| UI 복잡도 증가 | 중 | 중 | 기본 UI 먼저, 고급 기능 후순위 |
| 밸런싱 시간 | 고 | 저 | 플레이스홀더 값으로 시작 |

---

## 5. 테스트 계획

### 5.1 단위 테스트

| 대상 | 테스트 케이스 |
|-----|-------------|
| ItemInstance | 생성, 스탯 계산, 직렬화 |
| InventorySlot | 스택, 잠금, 빈 슬롯 |
| InventoryManager | 추가/제거/이동/정렬 |
| EquipmentManager | 장착/해제/조건 검증 |
| ConsumableManager | 사용/쿨다운 |
| StatCalculator | 스탯 합산, 세트 효과 |

### 5.2 통합 테스트

| 시나리오 | 검증 항목 |
|---------|----------|
| 아이템 획득 → 장착 → 스탯 적용 | 전체 흐름 |
| 저장 → 로드 → 복원 | 데이터 영속성 |
| 인벤토리 가득 참 → 드롭 | 엣지 케이스 |
| 레벨/폼 제한 장비 | 착용 조건 |

### 5.3 플레이 테스트

- 아이템 획득 피드백 (시각/청각)
- 인벤토리 조작 편의성
- 퀵슬롯 사용 반응성
- 툴팁 가독성

---

## 6. 마이그레이션 계획

### 6.1 기존 Item.cs → ItemData

```csharp
// 기존 Item 사용처
PlayerStats.EquipItem(Item item)  // → ItemInstance로 변경

// 마이그레이션 단계
1. ItemData 기본 클래스 생성
2. EquipmentData가 ItemData 상속
3. 기존 Item을 EquipmentData로 변환 (에셋 마이그레이션)
4. PlayerStats의 Item → ItemInstance 변경
5. 기존 Item.cs 제거
```

### 6.2 EquipmentSlot 확장

```csharp
// 기존 enum 값
Weapon = 0, Armor = 1, Accessory = 2

// 확장 enum 값
Weapon = 0, Armor = 1, Helmet = 2, Gloves = 3, Boots = 4, Accessory1 = 5, Accessory2 = 6

// Accessory → Accessory1 마이그레이션
// 저장 데이터에서 slot=2(Accessory)를 5(Accessory1)로 변환
```

### 6.3 InventorySystem → InventoryManager

```csharp
// 기존 구조
List<Item> items

// 신규 구조
InventorySlot[] slots
```

- 기존 ISaveable 인터페이스 유지
- 저장 데이터 버전 관리 (version 필드)

---

## 7. 생성 아티팩트

### 7.1 Phase 1 완료 시
- `research.md` - 기술 리서치 문서
- `data-model.md` - 데이터 모델 정의
- `contracts/inventory-api.md` - API 계약
- `quickstart.md` - 빠른 시작 가이드

### 7.2 tasks.md 생성 대기
- `/speckit.tasks` 명령으로 태스크 목록 생성 예정

---

## 8. 관련 문서

- [spec.md](./spec.md) - 기능 명세서
- [research.md](./research.md) - 기술 리서치
- [data-model.md](./data-model.md) - 데이터 모델
- [contracts/inventory-api.md](./contracts/inventory-api.md) - API 계약
- [quickstart.md](./quickstart.md) - 빠른 시작 가이드

---

*작성: Claude Code*
*최종 수정: 2025-12-28*
