# Tasks: 022-item-inventory

**기능명**: 아이템/인벤토리 시스템
**브랜치**: `022-item-inventory`
**생성일**: 2025-12-28
**상태**: 코드 구현 완료 (플레이 테스트 대기)

---

## 요약

| 항목 | 값 |
|-----|------|
| 총 태스크 수 | 52개 |
| Phase 수 | 8개 |
| 예상 신규 파일 | 26개 |
| 예상 수정 파일 | 8개 |
| MVP 범위 | Phase 1-3 (데이터 + 인벤토리 + 장비) |

---

## Phase 1: Setup (프로젝트 초기화)

**목표**: 기본 enum 및 데이터 구조 정의

- [x] T001 [P] ItemCategory enum 생성 in `Assets/_Project/Scripts/Core/Enums/ItemCategory.cs`
- [x] T002 [P] ItemRarity enum 생성 in `Assets/_Project/Scripts/Core/Enums/ItemRarity.cs`
- [x] T003 [P] ConsumeType enum 생성 in `Assets/_Project/Scripts/Core/Enums/ConsumeType.cs`
- [x] T004 [P] ModifierType enum 생성 in `Assets/_Project/Scripts/Core/Enums/ModifierType.cs`
- [x] T005 EquipmentSlot enum 확장 (7슬롯) in `Assets/_Project/Scripts/Core/Enums/EquipmentSlot.cs`
- [x] T006 EquipResult enum 생성 in `Assets/_Project/Scripts/Core/Enums/EquipResult.cs`
- [x] T007 UseResult enum 생성 in `Assets/_Project/Scripts/Core/Enums/UseResult.cs`
- [x] T008 PickupResult enum 생성 in `Assets/_Project/Scripts/Core/Enums/PickupResult.cs`
- [x] T009 InventorySortType enum 생성 in `Assets/_Project/Scripts/Inventory/InventorySortType.cs`

---

## Phase 2: Foundational (데이터 레이어)

**목표**: 아이템 데이터 클래스 및 인스턴스 구조 정의
**의존성**: Phase 1 완료 필수

- [x] T010 StatModifier 구조체 생성 in `Assets/_Project/Scripts/Data/StatModifier.cs`
- [x] T011 ItemData 기본 클래스 (ScriptableObject) 생성 in `Assets/_Project/Scripts/Data/ItemData.cs`
- [x] T012 ItemData.GetRarityColor() 메서드 구현 in `Assets/_Project/Scripts/Data/ItemData.cs`
- [x] T013 EquipmentData 클래스 (ItemData 상속) 생성 in `Assets/_Project/Scripts/Data/EquipmentData.cs`
- [x] T014 ConsumableData 클래스 (ItemData 상속) 생성 in `Assets/_Project/Scripts/Data/ConsumableData.cs`
- [x] T015 ItemInstance 클래스 생성 in `Assets/_Project/Scripts/Data/ItemInstance.cs`
- [x] T016 ItemInstance.GenerateRandomStats() 메서드 구현 in `Assets/_Project/Scripts/Data/ItemInstance.cs`
- [x] T017 ItemInstance.CreateFromData() 팩토리 메서드 구현 in `Assets/_Project/Scripts/Data/ItemInstance.cs`
- [x] T018 ItemConstants 클래스 (등급 색상) 생성 in `Assets/_Project/Scripts/Data/ItemConstants.cs`

---

## Phase 3: 인벤토리 코어

**목표**: 슬롯 기반 인벤토리 시스템 구현
**의존성**: Phase 2 완료 필수

### 3.1 인벤토리 슬롯

- [x] T019 InventorySlot 클래스 생성 in `Assets/_Project/Scripts/Inventory/InventorySlot.cs`
- [x] T020 InventorySlot.IsEmpty, IsFull 프로퍼티 구현 in `Assets/_Project/Scripts/Inventory/InventorySlot.cs`

### 3.2 인벤토리 매니저

- [x] T021 InventoryManager 클래스 골격 생성 in `Assets/_Project/Scripts/Inventory/InventoryManager.cs`
- [x] T022 InventoryManager.AddItem() 메서드 구현 in `Assets/_Project/Scripts/Inventory/InventoryManager.cs`
- [x] T023 InventoryManager.AddItemInstance() 메서드 구현 in `Assets/_Project/Scripts/Inventory/InventoryManager.cs`
- [x] T024 InventoryManager.RemoveItem() 메서드 구현 in `Assets/_Project/Scripts/Inventory/InventoryManager.cs`
- [x] T025 InventoryManager.GetSlot(), GetAllSlots() 메서드 구현 in `Assets/_Project/Scripts/Inventory/InventoryManager.cs`
- [x] T026 InventoryManager.GetItemsByCategory() 메서드 구현 in `Assets/_Project/Scripts/Inventory/InventoryManager.cs`
- [x] T027 InventoryManager.MoveItem(), SwapItems() 메서드 구현 in `Assets/_Project/Scripts/Inventory/InventoryManager.cs`
- [x] T028 InventoryManager.SortInventory() 메서드 구현 in `Assets/_Project/Scripts/Inventory/InventoryManager.cs`
- [x] T029 InventoryManager.ExpandCapacity() 메서드 구현 in `Assets/_Project/Scripts/Inventory/InventoryManager.cs`
- [x] T030 InventoryConstants 클래스 생성 in `Assets/_Project/Scripts/Inventory/InventoryConstants.cs`

### 3.3 저장 데이터

- [x] T031 InventoryData 클래스 확장 (v2) in `Assets/_Project/Scripts/Save/InventorySlotData.cs`
- [x] T032 InventorySlotData 클래스 생성 in `Assets/_Project/Scripts/Save/InventorySlotData.cs`
- [x] T033 InventoryManager ISaveable 구현 in `Assets/_Project/Scripts/Inventory/InventoryManager.cs`

---

## Phase 4: 장비 시스템

**목표**: 7슬롯 장비 장착 및 스탯 계산
**의존성**: Phase 3 완료 필수

### 4.1 스탯 계산

- [x] T034 StatCalculator 클래스 생성 in `Assets/_Project/Scripts/Stats/StatCalculator.cs`
- [x] T035 StatCalculator.CalculateEquipmentStats() 구현 in `Assets/_Project/Scripts/Stats/StatCalculator.cs`
- [x] T036 StatCalculator.CalculateFinalStat() 구현 in `Assets/_Project/Scripts/Stats/StatCalculator.cs`

### 4.2 장비 매니저

- [x] T037 EquipmentManager 클래스 생성 in `Assets/_Project/Scripts/Inventory/EquipmentManager.cs`
- [x] T038 EquipmentManager.Equip() 메서드 구현 in `Assets/_Project/Scripts/Inventory/EquipmentManager.cs`
- [x] T039 EquipmentManager.CanEquip() 조건 검증 구현 in `Assets/_Project/Scripts/Inventory/EquipmentManager.cs`
- [x] T040 EquipmentManager.Unequip() 메서드 구현 in `Assets/_Project/Scripts/Inventory/EquipmentManager.cs`
- [x] T041 EquipmentManager.GetEquipped(), GetAllEquipped() 구현 in `Assets/_Project/Scripts/Inventory/EquipmentManager.cs`

### 4.3 PlayerStats 연동

- [x] T042 PlayerStats 장비 딕셔너리 7슬롯 확장 in `Assets/_Project/Scripts/Inventory/EquipmentManager.cs`
- [x] T043 PlayerStats.RecalculateStats() StatCalculator 연동 in `Assets/_Project/Scripts/Stats/StatCalculator.cs`
- [x] T044 PlayerEquipmentData 저장 클래스 생성 in `Assets/_Project/Scripts/Save/PlayerEquipmentData.cs`

---

## Phase 5: 소비 아이템 & 드롭

**목표**: 소비 아이템 사용 및 월드 드롭 시스템
**의존성**: Phase 3 완료 필수

### 5.1 소비 아이템

- [x] T045 ConsumableManager 클래스 생성 in `Assets/_Project/Scripts/Inventory/ConsumableManager.cs`
- [x] T046 ConsumableManager.UseItem() 메서드 구현 in `Assets/_Project/Scripts/Inventory/ConsumableManager.cs`
- [x] T047 ConsumableManager 쿨다운 시스템 구현 in `Assets/_Project/Scripts/Inventory/ConsumableManager.cs`

### 5.2 퀵슬롯

- [x] T048 QuickSlotManager 클래스 생성 in `Assets/_Project/Scripts/Inventory/QuickSlotManager.cs`
- [x] T049 QuickSlotManager.AssignToQuickSlot(), UseQuickSlot() 구현 in `Assets/_Project/Scripts/Inventory/QuickSlotManager.cs`
- [x] T050 QuickSlotInfo 구조체 생성 in `Assets/_Project/Scripts/Inventory/QuickSlotManager.cs`

### 5.3 드롭 시스템

- [x] T051 ItemDropManager 클래스 생성 in `Assets/_Project/Scripts/Loot/ItemDropManager.cs`
- [x] T052 ItemDropManager.DropItem() 메서드 구현 in `Assets/_Project/Scripts/Loot/ItemDropManager.cs`
- [x] T053 ItemDropManager.DropFromTable() 메서드 구현 in `Assets/_Project/Scripts/Loot/ItemDropManager.cs`
- [x] T054 ItemDropManager.PickupItem() 메서드 구현 in `Assets/_Project/Scripts/Loot/ItemDropManager.cs`
- [x] T055 DroppedItemV2 (ItemInstance 지원) 생성 in `Assets/_Project/Scripts/Loot/DroppedItemV2.cs`
- [x] T056 LootTableV2 (ItemData 지원) 생성 in `Assets/_Project/Scripts/Loot/LootTableV2.cs`

---

## Phase 6: UI

**목표**: MVP 패턴 인벤토리 UI 확장
**의존성**: Phase 4, Phase 5 완료 필수

### 6.1 ViewModel

- [x] T057 [P] InventorySlotViewModel 클래스 생성 in `Assets/_Project/Scripts/UI/MVP/ViewModels/InventorySlotViewModel.cs`
- [x] T058 [P] ItemViewModelV2 클래스 생성 in `Assets/_Project/Scripts/UI/MVP/ViewModels/ItemViewModelV2.cs`
- [x] T059 EquipmentViewModelV2 확장 (7슬롯) in `Assets/_Project/Scripts/UI/MVP/ViewModels/EquipmentViewModelV2.cs`

### 6.2 View/Presenter

- [x] T060 IInventoryView 인터페이스 확장 - (기존 유지, V2 ViewModel 호환)
- [x] T061 InventoryView 슬롯 기반 UI 확장 - (ItemSlotUI 사용)
- [x] T062 InventoryPresenter ViewModel 변환 로직 확장 - (ItemViewModelV2 팩토리)

### 6.3 UI 컴포넌트

- [x] T063 ItemSlotUI 컴포넌트 생성 in `Assets/_Project/Scripts/UI/Components/ItemSlotUI.cs`
- [x] T064 ItemTooltip 컴포넌트 생성 in `Assets/_Project/Scripts/UI/MVP/Views/ItemTooltip.cs`
- [x] T065 EquipmentSlotUI 7슬롯 확장 in `Assets/_Project/Scripts/UI/Components/EquipmentSlotUI.cs`

---

## Phase 7: 폴리싱

**목표**: 추가 기능 및 에셋 생성
**의존성**: Phase 6 완료 필수

### 7.1 세트 아이템

- [x] T066 SetItemData ScriptableObject 생성 in `Assets/_Project/Scripts/Data/SetItemData.cs`
- [x] T067 StatCalculator.CalculateSetBonuses() 구현 in `Assets/_Project/Scripts/Stats/StatCalculator.cs`

### 7.2 필터링

- [x] T068 InventoryManager.FilterItems() 메서드 구현 in `Assets/_Project/Scripts/Inventory/InventoryManager.cs`
  - InventoryFilter 클래스 추가 (카테고리/등급/장비슬롯/이름검색/상태 필터)

### 7.3 테스트 에셋

- [x] T069 [P] 무기 테스트 아이템 5개 생성 in `Assets/_Project/Resources/Items/Weapons/`
  - 에디터 메뉴 GASPT > Generate Test Items > Weapons Only
- [x] T070 [P] 방어구 테스트 아이템 5개 생성 in `Assets/_Project/Resources/Items/Armors/`
  - 에디터 메뉴 GASPT > Generate Test Items > Armors Only
- [x] T071 [P] 소비 아이템 테스트 5개 생성 in `Assets/_Project/Resources/Items/Consumables/`
  - 에디터 메뉴 GASPT > Generate Test Items > Consumables Only

### 7.4 에디터 도구

- [x] T072 ItemAssetGenerator 에디터 도구 생성 in `Assets/_Project/Editor/ItemAssetGenerator.cs`
  - 개별 아이템 생성 GUI
  - 테스트 아이템 일괄 생성 메뉴

---

## Phase 8: 통합 및 검증

**목표**: 시스템 통합 및 최종 검증
**의존성**: Phase 7 완료 필수

- [x] T073 기존 Item.cs 참조 마이그레이션 확인 in 프로젝트 전체
  - Item.cs: 레거시 호환 유지 (3슬롯 시스템용)
  - ItemData.cs: 새로운 기본 클래스 (7슬롯 시스템용)
  - 기존 Item 사용 코드는 점진적 마이그레이션 권장
- [x] T074 EquipmentSlot 마이그레이션 검증 (Accessory → Accessory1) in 저장 데이터
  - Obsolete 속성으로 Accessory = 5 추가 (Accessory1과 동일 값)
  - 기존 저장 데이터 호환성 유지
- [x] T075 SaveSystem 호환성 테스트 in `Assets/_Project/Scripts/Save/`
  - GameSaveData에 inventoryV2, equipmentV2 필드 추가
  - dataVersion 필드 추가 (v2)
  - 레거시 inventory 필드 유지
- [ ] T076 플레이 테스트: 아이템 획득 → 장착 → 스탯 적용 흐름
  - Unity 에디터에서 GASPT > Generate Test Items 메뉴 실행 필요
  - 생성된 에셋으로 테스트 진행
- [ ] T077 플레이 테스트: 소비 아이템 사용 및 쿨다운
  - ConsumableManager 통합 테스트 필요
- [ ] T078 플레이 테스트: 인벤토리 UI 조작
  - 인벤토리 Prefab 설정 필요

---

## 의존성 그래프

```
Phase 1 (Setup)
    │
    ▼
Phase 2 (Foundational)
    │
    ▼
Phase 3 (인벤토리 코어)
    │
    ├───────────┬───────────┐
    ▼           ▼           ▼
Phase 4     Phase 5     (독립 가능)
(장비)      (소비/드롭)
    │           │
    └─────┬─────┘
          ▼
      Phase 6 (UI)
          │
          ▼
      Phase 7 (폴리싱)
          │
          ▼
      Phase 8 (통합)
```

---

## 병렬 실행 가능 태스크

### Phase 1 (전체 병렬)
- T001, T002, T003, T004 (모든 enum 병렬 생성)

### Phase 2
- T010~T018 중 의존성 없는 태스크 병렬 가능

### Phase 3-5
- Phase 4와 Phase 5는 Phase 3 완료 후 병렬 진행 가능

### Phase 6
- T057, T058 (ViewModel) 병렬 생성

### Phase 7
- T069, T070, T071 (테스트 에셋) 병렬 생성

---

## MVP 범위

**최소 기능 제품 (MVP)**: Phase 1 ~ Phase 4

| Phase | 내용 | MVP |
|:-----:|------|:---:|
| 1 | Setup (Enums) | ✓ |
| 2 | Foundational (Data) | ✓ |
| 3 | 인벤토리 코어 | ✓ |
| 4 | 장비 시스템 | ✓ |
| 5 | 소비/드롭 | - |
| 6 | UI | - |
| 7 | 폴리싱 | - |
| 8 | 통합 | - |

MVP 완료 시 다음 기능 사용 가능:
- 아이템 데이터 정의 (ScriptableObject)
- 인벤토리 아이템 관리 (추가/제거/이동)
- 7슬롯 장비 장착/해제
- 장비 스탯이 PlayerStats에 반영
- 저장/로드

---

## 구현 전략

1. **데이터 먼저**: Phase 1-2에서 데이터 구조 완성 후 매니저 구현
2. **기존 코드 확장**: InventorySystem → InventoryManager 점진적 리팩토링
3. **마이그레이션 주의**: EquipmentSlot 변경 시 기존 저장 데이터 호환성
4. **UI 후순위**: 코어 기능 완성 후 UI 연동

---

## 검증 체크리스트

### Phase 3 완료 시
- [ ] InventoryManager.AddItem() 동작 확인
- [ ] 스택 가능 아이템 수량 증가 확인
- [ ] 인벤토리 가득 참 이벤트 발생 확인

### Phase 4 완료 시
- [ ] 7슬롯 장비 장착 확인
- [ ] 레벨/폼 조건 검증 동작 확인
- [ ] 스탯 재계산 정상 동작 확인

### Phase 6 완료 시
- [ ] 인벤토리 UI 열기/닫기
- [ ] 아이템 드래그 앤 드롭
- [ ] 툴팁 표시

---

*작성: Claude Code*
*최종 수정: 2025-12-28*
