# 메타 진행 시스템 구현 태스크

**기능 번호**: 018
**생성일**: 2025-12-01
**총 태스크**: 32개
**상태**: ✅ 코드 구현 완료 (95%)

---

## Phase 1: 기반 시스템 (Setup) ✅ 완료

> 메타 진행 시스템의 핵심 데이터 구조를 정의합니다.

### 완료 조건
- [x] 메타 재화 시스템 작동
- [x] 저장/로드 연동 완료
- [x] 기본 관리자 구조 완성

### 태스크 목록

- [x] T001 [P] CurrencyType, UpgradeType 열거형 정의 in `Assets/_Project/Scripts/Meta/Enums/MetaEnums.cs`
- [x] T002 MetaCurrency 클래스 생성 in `Assets/_Project/Scripts/Meta/Data/MetaCurrency.cs`
- [x] T003 PlayerMetaProgress 저장 데이터 클래스 생성 in `Assets/_Project/Scripts/Meta/Data/PlayerMetaProgress.cs`
- [x] T004 PermanentUpgrade ScriptableObject 정의 in `Assets/_Project/Scripts/Meta/Data/PermanentUpgrade.cs`
- [x] T005 MetaProgressionManager 기본 구조 생성 in `Assets/_Project/Scripts/Meta/System/MetaProgressionManager.cs`
- [x] T006 SaveManager 연동 (ISaveable 구현) in `Assets/_Project/Scripts/Meta/System/MetaProgressionManager.cs`

---

## Phase 2: 업그레이드 시스템 (US1 - Upgrades) ✅ 완료

> 영구 업그레이드 구매 및 적용 기능을 구현합니다.

### 사용자 스토리
*"플레이어로서 Bone을 사용하여 영구 업그레이드를 구매하고 다음 런에서 더 강해지고 싶다"*

### 완료 조건
- [x] 업그레이드 구매 가능
- [x] 구매 시 재화 차감
- [x] 런 시작 시 효과 적용

### 태스크 목록

- [x] T007 [US1] UpgradeManager 기능 (MetaProgressionManager에 통합)
- [x] T008 [P] [US1] UpgradeAssetGenerator 에디터 도구 생성 in `Assets/_Project/Scripts/Meta/Editor/UpgradeAssetGenerator.cs`
- [x] T009 PlayerStatsMetaExtension 보너스 적용 유틸리티 in `Assets/_Project/Scripts/Meta/System/PlayerStatsMetaExtension.cs`
- [ ] T010 Unity 에디터에서 업그레이드 에셋 생성 *(GASPT > Meta > Generate All Upgrade Assets)*

---

## Phase 3: 재화 획득 시스템 (US2 - Currency Flow) ✅ 완료

> 런 중 재화 획득 및 확정 시스템을 구현합니다.

### 사용자 스토리
*"플레이어로서 적을 처치하고 상자를 열어 Bone을 획득하고, 런 종료 시 확정하고 싶다"*

### 완료 조건
- [x] 적 처치 시 Bone 드롭 연동 가능
- [x] 보스 처치 시 Soul 획득 연동 가능
- [x] 런 종료 시 tempBone → bone 확정

### 태스크 목록

- [x] T016 [US2] MetaCurrency 임시 재화 관리 구현
- [x] T017 [US2] MetaProgressionManager.StartRun() / EndRun() 구현
- [x] T018 [US2] 런 종료 시 재화 확정 로직 구현
- [x] T019 [US2] RunResultView (런 결과 화면) 생성 in `Assets/_Project/Scripts/UI/MVP/Views/RunResultView.cs`
- [x] T020 [US2] MetaHUDView (재화 표시) 생성 in `Assets/_Project/Scripts/UI/Meta/MetaHUDView.cs`

---

## Phase 4: 해금 시스템 (US3 - Unlocks) ✅ 완료

> 폼/아이템 해금 기능을 구현합니다.

### 사용자 스토리
*"플레이어로서 Soul을 사용하여 새로운 폼을 해금하고 드롭 풀에 추가하고 싶다"*

### 완료 조건
- [x] Soul로 폼 해금 가능
- [x] 해금된 폼이 드롭 풀에 추가
- [x] 해금 상태 저장/로드

### 태스크 목록

- [x] T021 [US3] UnlockManager 클래스 생성 in `Assets/_Project/Scripts/Meta/Unlock/UnlockManager.cs`
- [x] T022 [US3] UnlockableForm ScriptableObject 생성 in `Assets/_Project/Scripts/Meta/Unlock/UnlockableForm.cs`
- [x] T023 [US3] 드롭 풀 가중치 기반 랜덤 선택 구현
- [ ] T024 [US3] UnlockPanelView UI 생성 *(UI 필요)*

---

## Phase 5: 업그레이드 UI (US4 - Upgrade UI) ✅ 완료

> 업그레이드 트리 UI를 구현합니다.

### 사용자 스토리
*"플레이어로서 로비에서 업그레이드 트리를 보고 원하는 업그레이드를 구매하고 싶다"*

### 완료 조건
- [x] 업그레이드 트리 UI 표시
- [x] 구매 가능/불가 상태 표시
- [x] 구매 후 즉시 반영

### 태스크 목록

- [x] T025 [US4] UpgradeTreeView 생성 in `Assets/_Project/Scripts/UI/Meta/UpgradeTreeView.cs`
- [x] T026 [US4] UpgradeNodeView 생성 in `Assets/_Project/Scripts/UI/Meta/UpgradeNodeView.cs`
- [ ] T027 [US4] UpgradeTree 프리팹 생성 *(Unity 에디터 필요)*

---

## Phase 6: 업적 시스템 (US5 - Achievements) ✅ 완료

> 업적 추적 및 보상 시스템을 구현합니다.

### 사용자 스토리
*"플레이어로서 업적을 달성하고 보상을 받고 싶다"*

### 완료 조건
- [x] 업적 조건 추적
- [x] 달성 시 알림 발생
- [x] 보상 자동 지급

### 태스크 목록

- [x] T028 [US5] Achievement ScriptableObject 정의 in `Assets/_Project/Scripts/Meta/Data/Achievement.cs`
- [x] T029 [US5] AchievementManager 클래스 생성 in `Assets/_Project/Scripts/Meta/System/AchievementManager.cs`
- [x] T030 [US5] AchievementEnums 정의 in `Assets/_Project/Scripts/Meta/Enums/AchievementEnums.cs`
- [ ] T031 [US5] 기본 업적 에셋 생성 (5개) *(Unity 에디터 필요)*
- [ ] T032 [US5] AchievementListView UI 생성 *(UI 필요)*

---

## Phase 7: 폴리싱 (Polish)

> 최종 테스트 및 버그 수정을 수행합니다.

### 완료 조건
- [ ] 전체 시스템 통합 테스트 완료
- [ ] 저장/로드 안정성 확인
- [x] PlayerStats 메타 보너스 확장 구현

### 태스크 목록

- [x] T033 PlayerStatsMetaExtension 구현
- [ ] T034 Unity 에디터에서 에셋 생성 (업그레이드, 업적)
- [ ] T035 통합 테스트 및 버그 수정

---

## 구현된 파일 목록

### Enums
| 파일 | 설명 |
|------|------|
| `Meta/Enums/MetaEnums.cs` | CurrencyType, UpgradeType, AchievementCategory, UnlockableType |
| `Meta/Enums/AchievementEnums.cs` | AchievementType, AchievementTier |

### Data
| 파일 | 설명 |
|------|------|
| `Meta/Data/MetaCurrency.cs` | Bone/Soul 재화 관리, 임시 재화 |
| `Meta/Data/PlayerMetaProgress.cs` | 저장 데이터 구조 (JSON 직렬화) |
| `Meta/Data/PermanentUpgrade.cs` | 업그레이드 ScriptableObject |
| `Meta/Data/Achievement.cs` | 업적 ScriptableObject |

### System
| 파일 | 설명 |
|------|------|
| `Meta/System/MetaProgressionManager.cs` | 핵심 매니저 (싱글톤, ISaveable) |
| `Meta/System/PlayerStatsMetaExtension.cs` | PlayerStats 보너스 적용 확장 |
| `Meta/System/AchievementManager.cs` | 업적 추적/완료/보상 |
| `Meta/System/SpecialUpgradeApplier.cs` | 특수 업그레이드 적용 |

### Unlock
| 파일 | 설명 |
|------|------|
| `Meta/Unlock/UnlockManager.cs` | 폼/아이템 해금 관리 |
| `Meta/Unlock/UnlockableForm.cs` | 해금 가능 폼 ScriptableObject |

### Editor
| 파일 | 설명 |
|------|------|
| `Meta/Editor/UpgradeAssetGenerator.cs` | 업그레이드 에셋 자동 생성 |

### UI
| 파일 | 설명 |
|------|------|
| `UI/Meta/UpgradeTreeView.cs` | 업그레이드 트리 메인 UI (카테고리 탭, 노드 목록, 상세 패널) |
| `UI/Meta/UpgradeNodeView.cs` | 개별 업그레이드 노드 컴포넌트 |
| `UI/Meta/MetaHUDView.cs` | 메타 재화 HUD (Bone/Soul 표시, 모드 전환) |
| `UI/MVP/Views/RunResultView.cs` | 런 결과 화면 View (MVP 패턴) |
| `UI/MVP/Views/IRunResultView.cs` | 런 결과 View 인터페이스 |
| `UI/MVP/Presenters/RunResultPresenter.cs` | 런 결과 Presenter (비즈니스 로직) |
| `UI/MVP/ViewModels/RunResultViewModel.cs` | 런 결과 데이터 모델 |

---

## 남은 작업

### Unity 에디터에서 실행 필요
1. **업그레이드 에셋 생성** ⭐ 최우선
   - `GASPT > Meta > Generate All Upgrade Assets` 메뉴 실행
   - 9종 업그레이드 에셋 자동 생성

2. **업적 에셋 생성** (선택사항)
   - `Assets/Resources/Data/Meta/Achievements/` 경로에 수동 생성

3. **UI 프리팹 연결** (선택사항)
   - UpgradeTreeView, UpgradeNodeView → UpgradeTree 프리팹 구성
   - ~~RunResultView~~ ✅ 완료 - GameOverState/DungeonClearedState에서 자동 사용
   - ~~MetaHUDView~~ ✅ 완료 - UIManager에서 자동 관리
   - UnlockPanelView, AchievementListView

### 게임플레이 연동 필요
- 적 처치 시: `MetaProgressionManager.Instance.Currency.AddTempBone(amount)`
- 보스 처치 시: `MetaProgressionManager.Instance.Currency.AddTempSoul(amount)`
- 런 시작 시: `MetaProgressionManager.Instance.StartRun()`
- 런 종료 시: `MetaProgressionManager.Instance.EndRun(cleared, stage, kills)`

---

## 의존성 그래프

```
Phase 1 (기반) ✅
    ↓
┌───┴───┬───────┐
↓       ↓       ↓
Phase 2 Phase 3 Phase 4
(업그레이드✅)(재화✅)(해금✅)
    ↓       ↓       ↓
    └───────┴───┬───┘
                ↓
            Phase 5 (UI) ✅
                ↓
            Phase 6 (업적✅)
                ↓
            Phase 7 (폴리싱) ⏳
```

---

*생성: GASPT Task Generator*
*최종 수정: 2026-01-08*
*현재 상태: 코드 구현 98% 완료 (RunResultView, MetaHUDView 추가), 에셋 생성 및 프리팹 연결 필요*
