# V1/V2 마이그레이션 계획

## 개요

| 구분 | V1 | V2 | 차이점 | 상태 |
|------|----|----|--------|:----:|
| 보스 페이즈 | `BossPhaseController` | `BossPhaseController` (통합) | 하드코딩 → 데이터 드리븐 | **완료** |
| 드롭 테이블 | `LootTable` + `Item` | `LootTableV2` + `ItemData` | 단일 드롭 → 다중 드롭 | **V2 우선** |
| 드롭 아이템 | `DroppedItem` + `LootSystem` | `DroppedItemV2` + `ItemDropManager` | Item → ItemInstance | **V2 우선** |
| 스킬 아이템 | `SkillItem` : `Item` | `SkillItemData` : `ItemData` | Item 상속 → ItemData 상속 | **V2 우선** |

---

## 1. BossPhaseController 마이그레이션 - **완료**

### 완료 내용 (2026-01-12)

1. **BossPhaseControllerV2 → BossPhaseController 통합**
   - V2 기능을 유지하면서 V1 호환 메서드 추가
   - 기본 생성자: 3페이즈 기본값 (V1 호환)
   - `UpdatePhase(int currentHp, int maxHp)`: V1 호환 오버로드

2. **BossEnemy.cs 업데이트**
   - `BossPhaseController` (통합) 사용으로 변경
   - enum 기반 → int 인덱스 기반 전환

3. **V1 파일 삭제**
   - `Scripts/Gameplay/Enemy/BossPhaseController.cs` (삭제됨)

---

## 2. LootTable/DroppedItem 마이그레이션 - **진행 중**

### 현재 상태: V2 우선 사용 + V1 폴백

코드에서 V2를 우선 사용하고, V2가 없으면 V1로 폴백합니다.

### 완료 내용 (2026-01-12)

#### Phase A: 데이터 필드 추가
- [x] `EnemyData.cs`: `lootTableV2` 필드 추가, `lootTable` Obsolete 표시
- [x] `BossData.cs`: `lootTableV2` 필드 추가, `lootTable` Obsolete 표시

#### Phase B: V2 우선 사용 로직
- [x] `Enemy.cs`: `DropLoot()` V2 우선 + V1 폴백
- [x] `BossRewardSystem.cs`: `DropLoot()` V2 우선 + V1 폴백

#### Phase C: V1 시스템 Obsolete 표시
- [x] `LootTable.cs`: `[Obsolete]` 속성 추가
- [x] `LootSystem.cs`: `[Obsolete]` 속성 추가
- [x] `DroppedItem.cs`: `[Obsolete]` 속성 추가

### 남은 작업

#### Phase D: 에셋 마이그레이션 (수동)
- [ ] 기존 LootTable 에셋을 LootTableV2로 변환
- [ ] EnemyData/BossData 에셋에서 lootTableV2 필드 설정

#### Phase E: V1 완전 제거 (에셋 마이그레이션 완료 후)
- [ ] `Scripts/Loot/LootTable.cs` 삭제
- [ ] `Scripts/Loot/LootEntry.cs` 삭제
- [ ] `Scripts/Loot/DroppedItem.cs` 삭제
- [ ] `Scripts/Loot/LootSystem.cs` 삭제

#### Phase F: V2 이름 정규화
- [ ] `LootTableV2.cs` → `LootTable.cs`
- [ ] `DroppedItemV2.cs` → `DroppedItem.cs`

---

## 3. SkillItem 마이그레이션 - **V2 우선**

### 완료 내용 (2026-01-21)

#### Phase A: V2 클래스 생성
- [x] `SkillItemData.cs` 생성 (ItemData 상속)
  - AbilityType, targetSlotIndex, cooldown, damage, range, duration, manaCost
  - effectPrefab, projectilePrefab, skillSound 필드 추가
  - CreateAbilityInstance() 팩토리 메서드
- [x] `ItemCategory.cs`에 `Skill` 카테고리 추가

#### Phase B: SkillItemManager V2 지원
- [x] V2 메서드 추가: `EquipSkillItemV2()`, `UnequipSkillItemV2()`
- [x] V2 이벤트 추가: `OnSkillEquippedV2`, `OnSkillUnequippedV2`
- [x] ItemDropManager.OnItemPickedUp 이벤트 구독
- [x] V1 메서드에 `[Obsolete]` 표시

### 남은 작업

#### Phase C: 에셋 마이그레이션 (수동)
- [ ] 기존 SkillItem 에셋을 SkillItemData로 변환
  - SkillItem_FireBall.asset
  - SkillItem_IceBlast.asset
  - SkillItem_LightningBolt.asset
  - SkillItem_Shield.asset
  - SkillItem_Teleport.asset

#### Phase D: V1 완전 제거 (에셋 마이그레이션 완료 후)
- [ ] `Scripts/Data/SkillItem.cs`에 `[Obsolete]` 추가
- [ ] V1 참조 코드 정리

---

## 4. 현재 아키텍처

### 드롭 시스템 흐름 (V2)
```
Enemy Death
    ↓
Enemy.DropLoot()
    ↓
[lootTableV2 있음?] → Yes → ItemDropManager.DropFromTable()
    ↓ No                         ↓
[V1 폴백]                   LootTableV2.Roll()
    ↓                            ↓
LootSystem.DropLoot()       ItemDropManager.DropItem()
                                 ↓
                            DroppedItemV2 생성
```

### 데이터 모델
```
V1: Item (ScriptableObject) → LootEntry → LootTable
V2: ItemData (SO) → ItemInstance (Runtime) → LootEntryV2 → LootTableV2
```

### 스킬 아이템 모델
```
V1: SkillItem : Item → SkillItemManager → Form
V2: SkillItemData : ItemData → SkillItemManager → Form
```

---

## 4. 리스크 및 주의사항

### V2 우선 사용 시
- `lootTableV2`가 null이면 자동으로 V1 `lootTable` 사용
- V1 코드는 `#pragma warning disable CS0618`로 Obsolete 경고 무시
- 점진적 마이그레이션 가능

### 에셋 마이그레이션
- 기존 LootTable 에셋은 유지됨 (V1 폴백으로 작동)
- 새로운 적/보스는 LootTableV2 사용 권장
- Unity Inspector에서 수동 설정 필요

---

## 5. 롤백 계획

마이그레이션 실패 시:
1. Git으로 이전 커밋으로 되돌리기
2. V1/V2 공존 상태 유지 (현재 상태)
3. 문제 분석 후 재시도

---

## 변경 이력

| 날짜 | 작성자 | 내용 |
|------|--------|------|
| 2026-01-12 | Claude | 초안 작성 |
| 2026-01-12 | Claude | BossPhaseController 마이그레이션 완료 |
| 2026-01-12 | Claude | LootTable V2 우선 사용 마이그레이션 완료 |
| 2026-01-21 | Claude | SkillItem V2 마이그레이션 (SkillItemData, SkillItemManager V2 지원) |
