# 메타 진행 시스템 구현 태스크

**기능 번호**: 018
**생성일**: 2025-12-01
**총 태스크**: 32개
**예상 기간**: 4주

---

## Phase 1: 기반 시스템 (Setup)

> 메타 진행 시스템의 핵심 데이터 구조를 정의합니다.

### 완료 조건
- [ ] 메타 재화 시스템 작동
- [ ] 저장/로드 연동 완료
- [ ] 기본 관리자 구조 완성

### 태스크 목록

- [ ] T001 [P] CurrencyType, UpgradeType 열거형 정의 in `Assets/_Project/Scripts/Meta/Enums/MetaEnums.cs`
- [ ] T002 MetaCurrency 클래스 생성 in `Assets/_Project/Scripts/Meta/Data/MetaCurrency.cs`
- [ ] T003 PlayerMetaProgress 저장 데이터 클래스 생성 in `Assets/_Project/Scripts/Meta/Data/PlayerMetaProgress.cs`
- [ ] T004 PermanentUpgrade ScriptableObject 정의 in `Assets/_Project/Scripts/Meta/Data/PermanentUpgrade.cs`
- [ ] T005 MetaProgressionManager 기본 구조 생성 in `Assets/_Project/Scripts/Meta/System/MetaProgressionManager.cs`
- [ ] T006 SaveManager 연동 (ISaveable 구현) in `Assets/_Project/Scripts/Meta/System/MetaProgressionManager.cs`

---

## Phase 2: 업그레이드 시스템 (US1 - Upgrades)

> 영구 업그레이드 구매 및 적용 기능을 구현합니다.

### 사용자 스토리
*"플레이어로서 Bone을 사용하여 영구 업그레이드를 구매하고 다음 런에서 더 강해지고 싶다"*

### 완료 조건
- [ ] 업그레이드 구매 가능
- [ ] 구매 시 재화 차감
- [ ] 런 시작 시 효과 적용

### 태스크 목록

- [ ] T007 [US1] UpgradeManager 클래스 생성 in `Assets/_Project/Scripts/Meta/System/UpgradeManager.cs`
- [ ] T008 [P] [US1] UP001_MaxHP 업그레이드 에셋 생성 in `Assets/Resources/Data/Meta/Upgrades/UP001_MaxHP.asset`
- [ ] T009 [P] [US1] UP002_Attack 업그레이드 에셋 생성 in `Assets/Resources/Data/Meta/Upgrades/UP002_Attack.asset`
- [ ] T010 [P] [US1] UP003_Defense 업그레이드 에셋 생성 in `Assets/Resources/Data/Meta/Upgrades/UP003_Defense.asset`
- [ ] T011 [P] [US1] UP004_MoveSpeed 업그레이드 에셋 생성 in `Assets/Resources/Data/Meta/Upgrades/UP004_MoveSpeed.asset`
- [ ] T012 [P] [US1] UP005_GoldBonus 업그레이드 에셋 생성 in `Assets/Resources/Data/Meta/Upgrades/UP005_GoldBonus.asset`
- [ ] T013 [P] [US1] UP006_ExpBonus 업그레이드 에셋 생성 in `Assets/Resources/Data/Meta/Upgrades/UP006_ExpBonus.asset`
- [ ] T014 [P] [US1] UP007_StartGold 업그레이드 에셋 생성 in `Assets/Resources/Data/Meta/Upgrades/UP007_StartGold.asset`
- [ ] T015 [US1] PlayerStats 연동 (업그레이드 효과 적용) in `Assets/_Project/Scripts/Meta/System/UpgradeManager.cs`

---

## Phase 3: 재화 획득 시스템 (US2 - Currency Flow)

> 런 중 재화 획득 및 확정 시스템을 구현합니다.

### 사용자 스토리
*"플레이어로서 적을 처치하고 상자를 열어 Bone을 획득하고, 런 종료 시 확정하고 싶다"*

### 완료 조건
- [ ] 적 처치 시 Bone 드롭
- [ ] 보스 처치 시 Soul 획득
- [ ] 런 종료 시 tempBone → bone 확정

### 태스크 목록

- [ ] T016 [US2] 적 처치 시 Bone 드롭 연동 in `Assets/_Project/Scripts/Meta/System/MetaProgressionManager.cs`
- [ ] T017 [US2] 보스 처치 시 Soul 획득 연동 in `Assets/_Project/Scripts/Meta/System/MetaProgressionManager.cs`
- [ ] T018 [US2] 런 종료 시 재화 확정 로직 in `Assets/_Project/Scripts/Meta/System/MetaProgressionManager.cs`
- [ ] T019 [US2] RunResultView (런 결과 화면) 생성 in `Assets/_Project/Scripts/UI/Meta/RunResultView.cs`
- [ ] T020 [US2] MetaHUDView (재화 표시) 생성 in `Assets/_Project/Scripts/UI/Meta/MetaHUDView.cs`

---

## Phase 4: 해금 시스템 (US3 - Unlocks)

> 폼/아이템 해금 기능을 구현합니다.

### 사용자 스토리
*"플레이어로서 Soul을 사용하여 새로운 폼을 해금하고 드롭 풀에 추가하고 싶다"*

### 완료 조건
- [ ] Soul로 폼 해금 가능
- [ ] 해금된 폼이 드롭 풀에 추가
- [ ] 해금 상태 저장/로드

### 태스크 목록

- [ ] T021 [US3] UnlockManager 클래스 생성 in `Assets/_Project/Scripts/Meta/System/UnlockManager.cs`
- [ ] T022 [US3] FormManager 연동 (해금된 폼 드롭 풀 반영) in `Assets/_Project/Scripts/Meta/System/UnlockManager.cs`
- [ ] T023 [US3] UnlockPanelView UI 생성 in `Assets/_Project/Scripts/UI/Meta/UnlockPanelView.cs`

---

## Phase 5: 업그레이드 UI (US4 - Upgrade UI)

> 업그레이드 트리 UI를 구현합니다.

### 사용자 스토리
*"플레이어로서 로비에서 업그레이드 트리를 보고 원하는 업그레이드를 구매하고 싶다"*

### 완료 조건
- [ ] 업그레이드 트리 UI 표시
- [ ] 구매 가능/불가 상태 표시
- [ ] 구매 후 즉시 반영

### 태스크 목록

- [ ] T024 [US4] UpgradeTreeView 생성 in `Assets/_Project/Scripts/UI/Meta/UpgradeTreeView.cs`
- [ ] T025 [US4] UpgradeNodeView 생성 in `Assets/_Project/Scripts/UI/Meta/UpgradeNodeView.cs`
- [ ] T026 [US4] UpgradeTree 프리팹 생성 in `Assets/_Project/Prefabs/UI/Meta/UpgradeTree.prefab`

---

## Phase 6: 업적 시스템 (US5 - Achievements)

> 업적 추적 및 보상 시스템을 구현합니다.

### 사용자 스토리
*"플레이어로서 업적을 달성하고 보상을 받고 싶다"*

### 완료 조건
- [ ] 업적 조건 추적
- [ ] 달성 시 알림 표시
- [ ] 보상 수령 가능

### 태스크 목록

- [ ] T027 [US5] Achievement ScriptableObject 정의 in `Assets/_Project/Scripts/Meta/Data/Achievement.cs`
- [ ] T028 [US5] AchievementManager 클래스 생성 in `Assets/_Project/Scripts/Meta/System/AchievementManager.cs`
- [ ] T029 [P] [US5] 기본 업적 에셋 생성 (5개) in `Assets/Resources/Data/Meta/Achievements/`
- [ ] T030 [US5] AchievementListView UI 생성 in `Assets/_Project/Scripts/UI/Meta/AchievementListView.cs`

---

## Phase 7: 폴리싱 (Polish)

> 최종 테스트 및 버그 수정을 수행합니다.

### 완료 조건
- [ ] 전체 시스템 통합 테스트 완료
- [ ] 저장/로드 안정성 확인

### 태스크 목록

- [ ] T031 특수 업그레이드 (ExtraDash, Revive) 추가 in `Assets/Resources/Data/Meta/Upgrades/`
- [ ] T032 통합 테스트 및 버그 수정 in `Assets/_Project/Scripts/Meta/`

---

## 의존성 그래프

```
Phase 1 (기반)
    ↓
┌───┴───┬───────┐
↓       ↓       ↓
Phase 2 Phase 3 Phase 4
(업그레이드)(재화)(해금)
    ↓       ↓       ↓
    └───────┴───┬───┘
                ↓
            Phase 5 (UI)
                ↓
            Phase 6 (업적)
                ↓
            Phase 7 (폴리싱)
```

---

## 병렬 실행 가능 태스크

### Phase 2 내 병렬
- T008~T014 (업그레이드 에셋 생성 독립)

### Phase 2~4 병렬
- Phase 2 (업그레이드), Phase 3 (재화), Phase 4 (해금)는 Phase 1 완료 후 **동시 진행 가능**

---

## MVP 범위 (최소 구현)

**MVP = Phase 1 + Phase 2 + Phase 3**

| 항목 | MVP 포함 |
|------|:--------:|
| 메타 재화 (Bone/Soul) | ✅ |
| 런 종료 시 재화 확정 | ✅ |
| 영구 업그레이드 구매 | ✅ |
| 폼 해금 | ❌ |
| 업적 시스템 | ❌ |
| 업그레이드 트리 UI | ❌ (기본 UI만) |

---

## 구현 전략

1. **점진적 전달**: Phase 1-3 완료 시 기본 메타 진행 플레이 가능
2. **데이터 기반**: ScriptableObject로 업그레이드/업적 데이터 관리
3. **저장 안정성 우선**: 백업 저장, 오류 처리 철저
4. **밸런스 조정 용이**: 모든 수치를 에셋에서 관리

---

## 재화 획득 참조

| 소스 | Bone | Soul |
|------|------|------|
| 일반 적 | 1-3 | - |
| 엘리트 적 | 5-10 | - |
| 상자 | 10-30 | - |
| 스테이지 보스 | 50 | 10 |
| 최종 보스 | 100 | 30 |
| 스테이지 클리어 | 스테이지 × 20 | 스테이지 × 5 |

---

*생성: GASPT Task Generator*
*최종 수정: 2025-12-01*
