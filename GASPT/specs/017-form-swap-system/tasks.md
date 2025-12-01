# 폼 교체 시스템 구현 태스크

**기능 번호**: 017
**생성일**: 2025-12-01
**총 태스크**: 28개
**예상 기간**: 3주

---

## Phase 1: 기반 시스템 (Setup)

> 폼 시스템의 핵심 데이터 구조를 정의합니다.

### 완료 조건
- [ ] FormData ScriptableObject 정의 완료
- [ ] FormManager 기본 구조 완성
- [ ] 기본 폼 데이터 생성

### 태스크 목록

- [ ] T001 [P] FormType, FormRarity 열거형 정의 in `Assets/_Project/Scripts/Forms/Data/FormEnums.cs`
- [ ] T002 [P] FormStats 구조체 정의 in `Assets/_Project/Scripts/Forms/Data/FormStats.cs`
- [ ] T003 FormData ScriptableObject 정의 in `Assets/_Project/Scripts/Forms/Data/FormData.cs`
- [ ] T004 FormInstance 런타임 클래스 생성 in `Assets/_Project/Scripts/Forms/Data/FormInstance.cs`
- [ ] T005 FormManager 기본 구조 생성 in `Assets/_Project/Scripts/Forms/System/FormManager.cs`
- [ ] T006 BasicMage 폼 에셋 생성 in `Assets/Resources/Data/Forms/BasicMage.asset`

---

## Phase 2: 폼 교체 로직 (US1 - Core Swap)

> Q키 입력으로 폼을 교체하는 핵심 기능을 구현합니다.

### 사용자 스토리
*"플레이어로서 Q키를 눌러 보유한 두 폼 사이를 즉시 교체하고 싶다"*

### 완료 조건
- [ ] Q키 입력 시 폼 교체 실행
- [ ] 교체 시 스탯/스킬/외형 즉시 변경
- [ ] 쿨다운 시스템 작동

### 태스크 목록

- [ ] T007 [US1] FormSwapSystem 클래스 생성 in `Assets/_Project/Scripts/Forms/System/FormSwapSystem.cs`
- [ ] T008 [US1] Q키 입력 처리 연동 in `Assets/_Project/Scripts/Forms/System/FormManager.cs`
- [ ] T009 [US1] PlayerStats 연동 (폼 보너스 적용/제거) in `Assets/_Project/Scripts/Forms/System/FormSwapSystem.cs`
- [ ] T010 [US1] SkillSystem 연동 (스킬 교체) in `Assets/_Project/Scripts/Forms/System/FormSwapSystem.cs`
- [ ] T011 [US1] 애니메이터 컨트롤러 교체 로직 in `Assets/_Project/Scripts/Forms/System/FormSwapSystem.cs`
- [ ] T012 [US1] 교체 쿨다운 시스템 구현 in `Assets/_Project/Scripts/Forms/System/FormManager.cs`
- [ ] T013 [US1] 교체 시 무적 프레임 (0.2초) 적용 in `Assets/_Project/Scripts/Forms/System/FormSwapSystem.cs`
- [ ] T014 [US1] 교체 이펙트 VFX 재생 in `Assets/_Project/Scripts/Forms/System/FormSwapSystem.cs`

---

## Phase 3: 폼 획득 시스템 (US2 - Acquisition)

> 던전에서 폼을 획득하는 기능을 구현합니다.

### 사용자 스토리
*"플레이어로서 던전에서 발견한 새로운 폼을 획득하고 싶다"*

### 완료 조건
- [ ] 폼 아이템 상호작용 가능
- [ ] 슬롯이 비어있으면 즉시 획득
- [ ] 슬롯이 가득 차면 교체 선택 UI 표시

### 태스크 목록

- [ ] T015 [US2] FormPickup 컴포넌트 생성 in `Assets/_Project/Scripts/Forms/Pickup/FormPickup.cs`
- [ ] T016 [US2] FormPickup 프리팹 생성 in `Assets/_Project/Prefabs/Forms/FormPickup.prefab`
- [ ] T017 [US2] FormManager.AcquireForm() 구현 in `Assets/_Project/Scripts/Forms/System/FormManager.cs`
- [ ] T018 [US2] 폼 드롭 로직 (버린 폼) in `Assets/_Project/Scripts/Forms/Pickup/FormPickup.cs`

---

## Phase 4: 폼 UI 시스템 (US3 - UI Display)

> HUD와 팝업 UI를 구현합니다.

### 사용자 스토리
*"플레이어로서 현재 폼 상태와 쿨다운을 HUD에서 확인하고 싶다"*

### 완료 조건
- [ ] HUD에 현재/대기 폼 아이콘 표시
- [ ] 쿨다운 오버레이 표시
- [ ] 폼 정보 팝업 작동

### 태스크 목록

- [ ] T019 [US3] FormHUDView MVP 구조 생성 in `Assets/_Project/Scripts/UI/Forms/FormHUDView.cs`
- [ ] T020 [US3] FormHUDPresenter 생성 in `Assets/_Project/Scripts/UI/Forms/FormHUDPresenter.cs`
- [ ] T021 [US3] FormHUD 프리팹 생성 in `Assets/_Project/Prefabs/UI/Forms/FormHUD.prefab`
- [ ] T022 [US3] 쿨다운 오버레이 UI 구현 in `Assets/_Project/Scripts/UI/Forms/FormHUDView.cs`
- [ ] T023 [US3] FormInfoPopup 생성 in `Assets/_Project/Scripts/UI/Forms/FormInfoPopup.cs`
- [ ] T024 [US3] FormSelectPopup (교체 선택) 생성 in `Assets/_Project/Scripts/UI/Forms/FormSelectPopup.cs`

---

## Phase 5: 각성 시스템 (US4 - Awakening)

> 동일 폼 획득 시 자동 각성 기능을 구현합니다.

### 사용자 스토리
*"플레이어로서 같은 폼을 획득하면 자동으로 각성되어 강해지고 싶다"*

### 완료 조건
- [ ] 동일 폼 획득 시 자동 각성
- [ ] 등급 상승 및 스탯 강화
- [ ] 각성 이펙트 재생

### 태스크 목록

- [ ] T025 [US4] FormAwakening 시스템 구현 in `Assets/_Project/Scripts/Forms/System/FormAwakening.cs`
- [ ] T026 [US4] 각성 시 스탯 재계산 로직 in `Assets/_Project/Scripts/Forms/Data/FormInstance.cs`
- [ ] T027 [US4] 각성 VFX 및 메시지 표시 in `Assets/_Project/Scripts/Forms/System/FormAwakening.cs`

---

## Phase 6: 폴리싱 (Polish)

> 최종 테스트 및 버그 수정을 수행합니다.

### 완료 조건
- [ ] 모든 기능 정상 작동
- [ ] 성능 최적화 완료

### 태스크 목록

- [ ] T028 통합 테스트 및 버그 수정 in `Assets/_Project/Scripts/Forms/`

---

## 의존성 그래프

```
Phase 1 (기반)
    ↓
Phase 2 (교체 로직) ─────┐
    ↓                    │
Phase 3 (획득) ──────────┤
    ↓                    │
Phase 4 (UI) ←───────────┘
    ↓
Phase 5 (각성)
    ↓
Phase 6 (폴리싱)
```

---

## 병렬 실행 가능 태스크

### Phase 1 내 병렬
- T001, T002 (Enum과 Struct 독립)

### Phase 2~4 병렬
- Phase 3 (획득)과 Phase 4 (UI)는 Phase 2 진행 중 일부 병렬 가능

---

## MVP 범위 (최소 구현)

**MVP = Phase 1 + Phase 2**

| 항목 | MVP 포함 |
|------|:--------:|
| FormData 구조 | ✅ |
| Q키 폼 교체 | ✅ |
| 스탯/스킬 변경 | ✅ |
| 쿨다운 | ✅ |
| 폼 획득 | ❌ |
| HUD UI | ❌ |
| 각성 시스템 | ❌ |

---

## 구현 전략

1. **점진적 전달**: Phase 1-2 완료 시 기본 교체 기능 사용 가능
2. **이벤트 기반**: OnFormSwapped 등 이벤트로 UI와 느슨한 결합
3. **ScriptableObject**: 폼 데이터를 에셋으로 관리하여 확장 용이
4. **019 병렬 개발**: 폼 컨텐츠(019)와 시스템(017) 동시 개발 가능

---

*생성: GASPT Task Generator*
*최종 수정: 2025-12-01*
