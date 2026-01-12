# V1/V2 마이그레이션 계획

## 개요

| 구분 | V1 | V2 | 차이점 |
|------|----|----|--------|
| 보스 페이즈 | `BossPhaseController` | `BossPhaseControllerV2` | 하드코딩 → 데이터 드리븐 |
| 드롭 테이블 | `LootTable` + `Item` | `LootTableV2` + `ItemData` | 단일 드롭 → 다중 드롭 |
| 드롭 아이템 | `DroppedItem` + `LootSystem` | `DroppedItemV2` + `ItemDropManager` | Item → ItemInstance |

---

## 1. BossPhaseController 마이그레이션

### 현황
| 버전 | 사용처 | 사용 파일 수 |
|------|--------|:----------:|
| V1 (`BossPhaseController`) | `BossEnemy.cs` | 1개 |
| V2 (`BossPhaseControllerV2`) | `BaseBoss.cs`, 4개 보스 클래스 | 5개 |

### 주요 차이점
```
V1: 하드코딩된 3 Phase (70%, 30% 임계값)
    - enum BossPhase { Phase1, Phase2, Phase3 }
    - 고정 배율: Attack 1.0→1.2→1.5, Speed 1.0→1.3→1.3

V2: BossData 기반 동적 Phase
    - BossPhaseData[] phases 배열
    - 유연한 임계값 및 배율
    - 페이즈별 사용 가능 패턴 지정
```

### 마이그레이션 계획

#### Phase A: BossEnemy 분석 (선행 작업)
1. `BossEnemy.cs` 분석 - BaseBoss로 대체 가능한지 확인
2. BossEnemy가 사용하는 고유 기능 식별
3. 결정: BossEnemy를 BaseBoss로 마이그레이션 또는 V2 컨트롤러만 적용

#### Phase B: 마이그레이션 실행
**옵션 1: BossEnemy → BaseBoss 마이그레이션**
- BossEnemy의 로직을 BaseBoss로 이전
- BossEnemy 삭제
- BossPhaseController (V1) 삭제

**옵션 2: BossEnemy에 V2 컨트롤러 적용**
- BossEnemy에서 BossPhaseController → BossPhaseControllerV2 교체
- BossData 생성 및 연결
- BossPhaseController (V1) 삭제

#### Phase C: 정리
- `BossPhaseController.cs` 삭제
- `BossPhase` enum을 공통 위치로 이동 (필요시)

### 예상 작업량
- 분석: 1단계
- 구현: 2-3단계
- 테스트: 1단계

---

## 2. LootTable/DroppedItem 마이그레이션

### 현황
| 버전 | 시스템 | 사용처 |
|------|--------|--------|
| V1 | `LootSystem` + `LootTable` + `DroppedItem` | Enemy, BossRewardSystem, SkillItemManager 등 8개 |
| V2 | `ItemDropManager` + `LootTableV2` + `DroppedItemV2` | DroppedItemV2만 사용 |

### 주요 차이점
```
V1 (Item 기반):
- LootTable: Item 참조, 단일 드롭, 확률 기반
- DroppedItem: Item 저장, LootSystem으로 픽업
- LootEntry: Item item, float dropChance

V2 (ItemData/ItemInstance 기반):
- LootTableV2: ItemData 참조, min/max 드롭, 가중치 기반, 확정 드롭 지원
- DroppedItemV2: ItemInstance 저장, 수량 지원, ItemDropManager로 픽업
- LootEntryV2: ItemData itemData, float weight, min/max quantity
```

### 마이그레이션 계획

#### Phase A: 데이터 모델 통일
1. `Item` vs `ItemData` 관계 정리
   - Item: 런타임 아이템 객체
   - ItemData: ScriptableObject 정의
   - ItemInstance: 인스턴스화된 아이템 (장비 랜덤옵션 등)

2. 결정: ItemData/ItemInstance 기반으로 통일

#### Phase B: LootTable 마이그레이션
1. 기존 LootTable 에셋을 LootTableV2로 변환
2. LootEntry → LootEntryV2 변환 (dropChance → weight)
3. Enemy.cs, BossRewardSystem.cs 등에서 LootTable → LootTableV2 참조 변경

#### Phase C: DroppedItem 마이그레이션
1. LootSystem.CreateDroppedItem() → ItemDropManager.DropItem() 변경
2. DroppedItem 참조를 DroppedItemV2로 변경
3. 픽업 로직 통합

#### Phase D: 시스템 정리
1. LootSystem 역할 재정의 또는 ItemDropManager로 통합
2. 레거시 클래스 삭제:
   - `LootTable.cs` → 삭제
   - `LootEntry.cs` → 삭제 (LootEntryV2로 대체)
   - `DroppedItem.cs` → 삭제
   - `LootSystem.cs` → 삭제 또는 통합

#### Phase E: 테스트 및 검증
1. 적 처치 시 드롭 테스트
2. 보스 보상 드롭 테스트
3. 스킬 아이템 드롭 테스트

### 예상 작업량
- Phase A: 1단계 (분석)
- Phase B: 2단계 (LootTable 마이그레이션)
- Phase C: 2단계 (DroppedItem 마이그레이션)
- Phase D: 1단계 (정리)
- Phase E: 1단계 (테스트)

---

## 3. 마이그레이션 우선순위

### 권장 순서
1. **BossPhaseController** (영향 범위 작음, 1개 파일만 수정)
2. **LootTable/DroppedItem** (영향 범위 넓음, 8개+ 파일 수정)

### 리스크 평가
| 항목 | 리스크 | 이유 |
|------|:------:|------|
| BossPhaseController | 낮음 | 사용처 1개, 명확한 대체 경로 |
| LootTable | 중간 | 여러 시스템에서 사용, 데이터 변환 필요 |
| DroppedItem | 중간 | LootSystem과 연동, 픽업 로직 변경 |

---

## 4. 파일 삭제 체크리스트

### 마이그레이션 완료 후 삭제할 파일
- [ ] `Scripts/Gameplay/Enemy/BossPhaseController.cs` (V1)
- [ ] `Scripts/Loot/LootTable.cs` (V1)
- [ ] `Scripts/Loot/LootEntry.cs` (V1 전용)
- [ ] `Scripts/Loot/DroppedItem.cs` (V1)
- [ ] `Scripts/Loot/LootSystem.cs` (V1 - 통합 또는 삭제)

### 마이그레이션 완료 후 이름 변경할 파일
- [ ] `BossPhaseControllerV2.cs` → `BossPhaseController.cs`
- [ ] `LootTableV2.cs` → `LootTable.cs`
- [ ] `LootEntryV2` → `LootEntry`
- [ ] `DroppedItemV2.cs` → `DroppedItem.cs`

---

## 5. 롤백 계획

마이그레이션 실패 시:
1. Git으로 이전 커밋으로 되돌리기
2. V1/V2 공존 상태 유지
3. 문제 분석 후 재시도

---

## 변경 이력

| 날짜 | 작성자 | 내용 |
|------|--------|------|
| 2026-01-12 | Claude | 초안 작성 |
