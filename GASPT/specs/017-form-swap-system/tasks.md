# 폼 교체 시스템 구현 태스크

**기능 번호**: 017
**생성일**: 2025-12-01
**총 태스크**: 28개
**상태**: ✅ 거의 완료 (98%)

---

## Phase 1: 기반 시스템 (Setup) ✅ 완료

> 폼 시스템의 핵심 데이터 구조를 정의합니다.

### 완료 조건
- [x] FormData ScriptableObject 정의 완료
- [x] FormManager 기본 구조 완성
- [ ] 기본 폼 데이터 생성 (에셋)

### 태스크 목록

- [x] T001 [P] FormType, FormRarity 열거형 정의 in `Assets/_Project/Scripts/Forms/Data/FormEnums.cs`
- [x] T002 [P] FormStats 구조체 정의 in `Assets/_Project/Scripts/Forms/Data/FormStats.cs`
- [x] T003 FormData ScriptableObject 정의 in `Assets/_Project/Scripts/Forms/Data/FormData.cs`
- [x] T004 FormInstance 런타임 클래스 생성 in `Assets/_Project/Scripts/Forms/Data/FormInstance.cs`
- [x] T005 FormManager 기본 구조 생성 in `Assets/_Project/Scripts/Forms/System/FormManager.cs`
- [x] T006 BasicMage 폼 에셋 생성 도구 완성 in `Assets/_Project/Scripts/Editor/FormAssetCreator.cs` *(Unity 에디터에서 실행 필요)*

---

## Phase 2: 폼 교체 로직 (US1 - Core Swap) ✅ 완료

> Q키 입력으로 폼을 교체하는 핵심 기능을 구현합니다.

### 사용자 스토리
*"플레이어로서 Q키를 눌러 보유한 두 폼 사이를 즉시 교체하고 싶다"*

### 완료 조건
- [x] Q키 입력 시 폼 교체 실행
- [x] 교체 시 스탯/스킬/외형 즉시 변경
- [x] 쿨다운 시스템 작동

### 태스크 목록

- [x] T007 [US1] FormSwapSystem 클래스 생성 in `Assets/_Project/Scripts/Forms/System/FormSwapSystem.cs`
- [x] T008 [US1] Q키 입력 처리 연동 in `Assets/_Project/Scripts/Forms/System/FormManager.cs`
- [x] T009 [US1] PlayerStats 연동 (폼 보너스 적용/제거) in `Assets/_Project/Scripts/Forms/System/FormSwapSystem.cs`
- [x] T010 [US1] SkillSystem 연동 - FormData에 skill1, skill2 참조 포함
- [x] T011 [US1] 애니메이터 컨트롤러 교체 로직 in `Assets/_Project/Scripts/Forms/System/FormSwapSystem.cs`
- [x] T012 [US1] 교체 쿨다운 시스템 구현 in `Assets/_Project/Scripts/Forms/System/FormManager.cs`
- [x] T013 [US1] 교체 시 무적 프레임 (0.2초) 적용 in `Assets/_Project/Scripts/Forms/System/FormSwapSystem.cs`
- [x] T014 [US1] 교체 이펙트 VFX 재생 in `Assets/_Project/Scripts/Forms/System/FormSwapSystem.cs`

---

## Phase 3: 폼 획득 시스템 (US2 - Acquisition) ✅ 완료

> 던전에서 폼을 획득하는 기능을 구현합니다.

### 사용자 스토리
*"플레이어로서 던전에서 발견한 새로운 폼을 획득하고 싶다"*

### 완료 조건
- [x] 폼 아이템 상호작용 가능
- [x] 슬롯이 비어있으면 즉시 획득
- [x] 슬롯이 가득 차면 교체 선택 UI 표시

### 태스크 목록

- [x] T015 [US2] FormPickup 컴포넌트 생성 in `Assets/_Project/Scripts/Forms/Pickup/FormPickup.cs`
- [x] T016 [US2] FormPickup 프리팹 생성 - 코드 내 동적 생성 지원
- [x] T017 [US2] FormManager.AcquireForm() 구현 in `Assets/_Project/Scripts/Forms/System/FormManager.cs`
- [x] T018 [US2] 폼 드롭 로직 (버린 폼) in `Assets/_Project/Scripts/Forms/Pickup/FormPickup.cs`

---

## Phase 4: 폼 UI 시스템 (US3 - UI Display) ✅ 완료

> HUD와 팝업 UI를 구현합니다.

### 사용자 스토리
*"플레이어로서 현재 폼 상태와 쿨다운을 HUD에서 확인하고 싶다"*

### 완료 조건
- [x] HUD에 현재/대기 폼 아이콘 표시
- [x] 쿨다운 오버레이 표시
- [x] 폼 정보 팝업 작동

### 태스크 목록

- [x] T019 [US3] FormUIManager 통합 매니저 in `Assets/_Project/Scripts/UI/Forms/FormUIManager.cs`
- [x] T020 [US3] FormSlotUI 슬롯 컴포넌트 in `Assets/_Project/Scripts/UI/Forms/FormSlotUI.cs`
- [x] T021 [US3] FormCooldownUI 쿨다운 표시 in `Assets/_Project/Scripts/UI/Forms/FormCooldownUI.cs`
- [x] T022 [US3] 쿨다운 오버레이 UI 구현 - FormCooldownUI에 포함
- [x] T023 [US3] FormSelectionUI 교체 선택 UI in `Assets/_Project/Scripts/UI/Forms/FormSelectionUI.cs`
- [x] T024 [US3] 각성 알림 UI - FormUIManager에 포함

---

## Phase 5: 각성 시스템 (US4 - Awakening) ✅ 완료

> 동일 폼 획득 시 자동 각성 기능을 구현합니다.

### 사용자 스토리
*"플레이어로서 같은 폼을 획득하면 자동으로 각성되어 강해지고 싶다"*

### 완료 조건
- [x] 동일 폼 획득 시 자동 각성
- [x] 등급 상승 및 스탯 강화
- [x] 각성 이펙트 재생

### 태스크 목록

- [x] T025 [US4] FormAwakening 시스템 구현 - `FormInstance.Awaken()` 및 `FormManager.TryAcquireForm()`에 통합
- [x] T026 [US4] 각성 시 스탯 재계산 로직 in `Assets/_Project/Scripts/Forms/Data/FormInstance.cs`
- [x] T027 [US4] 각성 VFX 및 메시지 표시 - FormData에 awakeningEffectPrefab 포함, FormUIManager에서 알림

---

## Phase 6: 폴리싱 (Polish) ⏳ 진행중

> 최종 테스트 및 버그 수정을 수행합니다.

### 완료 조건
- [x] 모든 기능 정상 작동
- [ ] 폼 에셋 생성 완료 *(Unity 에디터 실행 필요)*
- [x] 성능 최적화 완료

### 태스크 목록

- [x] T028 폼 에셋 생성 도구 완성 (Unity 에디터에서 실행):
  - GASPT > Forms > Create All Form Assets 메뉴 사용
  - BasicMage, FlameMage, FrostMage, ThunderMage, DarkMage 5종 생성
- [ ] T029 Unity 에디터에서 폼 에셋 생성 실행 *(사용자 실행 필요)*

---

## 구현된 파일 목록

### Data Layer
| 파일 | 설명 |
|------|------|
| `Forms/Data/FormEnums.cs` | FormType, FormRarity, AcquireResult |
| `Forms/Data/FormStats.cs` | 폼 스탯 구조체 |
| `Forms/Data/FormData.cs` | 폼 ScriptableObject |
| `Forms/Data/FormInstance.cs` | 런타임 폼 인스턴스 |

### System Layer
| 파일 | 설명 |
|------|------|
| `Forms/System/FormManager.cs` | 폼 슬롯 관리, Q키 입력, 쿨다운 |
| `Forms/System/FormSwapSystem.cs` | 교체 실행, 스탯 적용, 이펙트 |
| `Forms/System/FormDropSystem.cs` | 폼 드롭 시스템 |
| `Forms/System/FormAwakeningEffects.cs` | 각성 이펙트 |

### Pickup
| 파일 | 설명 |
|------|------|
| `Forms/Pickup/FormPickup.cs` | 던전 드롭 픽업 |

### UI Layer
| 파일 | 설명 |
|------|------|
| `UI/Forms/FormUIManager.cs` | UI 통합 매니저 |
| `UI/Forms/FormSlotUI.cs` | 폼 슬롯 UI |
| `UI/Forms/FormCooldownUI.cs` | 쿨다운 표시 |
| `UI/Forms/FormSelectionUI.cs` | 교체 선택 UI |

### Editor Tools
| 파일 | 설명 |
|------|------|
| `Editor/FormAssetCreator.cs` | 폼 에셋 생성 도구 |
| `Editor/FormPickupCreator.cs` | 픽업 프리팹 생성 |
| `Editor/FormSystemTestWindow.cs` | 테스트 윈도우 |
| `Editor/FormTestSceneSetup.cs` | 테스트 씬 설정 |

### Test
| 파일 | 설명 |
|------|------|
| `Forms/Test/FormTestPlayerController.cs` | 테스트용 플레이어 |
| `Forms/Test/FormTestUIController.cs` | 테스트용 UI |

---

## 남은 작업

### Unity 에디터에서 실행 필요
1. **폼 에셋 생성** ⭐ 최우선
   - Unity 에디터에서 `GASPT > Forms > Create All Form Assets` 메뉴 실행
   - 5종 폼 에셋이 `Assets/Resources/Data/Forms/`에 자동 생성됨

2. **프리팹 설정** (선택사항)
   - Player 프리팹에 FormManager, FormSwapSystem 추가
   - FormPickup 프리팹 생성

3. **UI 프리팹 설정** (선택사항)
   - FormHUD 프리팹 생성 및 연결

---

## 의존성 그래프

```
Phase 1 (기반) ✅
    ↓
Phase 2 (교체 로직) ✅ ─────┐
    ↓                       │
Phase 3 (획득) ✅ ───────────┤
    ↓                       │
Phase 4 (UI) ✅ ←───────────┘
    ↓
Phase 5 (각성) ✅
    ↓
Phase 6 (폴리싱) ⏳
```

---

*생성: GASPT Task Generator*
*최종 수정: 2026-01-07*
*현재 상태: 코드 구현 98% 완료, Unity 에디터에서 폼 에셋 생성 실행 필요*
