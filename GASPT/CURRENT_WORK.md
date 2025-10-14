# 현재 작업 상태 (2025-10-14)

## ✅ 방금 완료한 작업

### UI 시스템 완전 재설계 (Panel 기반 Prefab 시스템)
**날짜**: 2025-10-14
**목적**: 상용 게임 수준의 체계적인 UI 아키텍처 구축

---

## 🎯 Phase 1: UI 시스템 설계 ✅ (완료)

### 1. 설계 문서 작성
**파일**: `UI_SYSTEM_DESIGN.md`

#### 주요 설계 내용:
- **Panel 기반 아키텍처**: 모든 UI가 BasePanel 상속
- **5단계 Layer 시스템**: Background(0) → Normal(100) → Popup(200) → System(300) → Transition(9999)
- **Prefab 기반 관리**: Resources/UI/Panels/ 폴더에서 동적 로드
- **UIManager 중앙 관리**: Panel 생명주기, Stack 관리, Layer Canvas 관리
- **Transition 애니메이션**: Fade, Scale, Slide 효과 내장

---

## 🔧 Phase 2: Core 클래스 구현 ✅ (완료)

### 2.1 PanelType Enum
**파일**: `Assets/_Project/Scripts/UI/Core/PanelType.cs`

```csharp
public enum PanelType
{
    None,
    // Background Layer
    MainMenu, Lobby,
    // Normal Layer
    GameplayHUD, Inventory, Shop, CharacterStatus,
    // Popup Layer
    Pause, Settings, Dialog, Reward, Confirm,
    // System Layer
    Loading, Toast,
    // Transition Layer
    Fade
}
```

### 2.2 UILayer Enum
**파일**: `Assets/_Project/Scripts/UI/Core/UILayer.cs`

```csharp
public enum UILayer
{
    Background = 0,
    Normal = 100,
    Popup = 200,
    System = 300,
    Transition = 9999
}
```

### 2.3 TransitionType Enum
**파일**: `Assets/_Project/Scripts/UI/Core/TransitionType.cs`

```csharp
public enum TransitionType
{
    None, Fade, Scale,
    SlideFromBottom, SlideToBottom,
    SlideFromLeft, SlideToLeft,
    SlideFromRight, SlideToRight
}
```

### 2.4 BasePanel 추상 클래스
**파일**: `Assets/_Project/Scripts/UI/Core/BasePanel.cs`

**주요 기능**:
- ✅ 생명주기 관리 (OnBeforeOpen, OnAfterOpen, OnBeforeClose, OnAfterClose)
- ✅ Canvas/CanvasGroup/GraphicRaycaster 자동 설정
- ✅ Layer 기반 sortingOrder 자동 설정
- ✅ 9가지 Transition 애니메이션 내장
- ✅ Awaitable 기반 비동기 처리
- ✅ 이벤트 시스템 (OnOpened, OnClosed)

**중요 수정**:
- `canvas.overrideSorting = true`: 부모 Canvas 무시하고 독립 Layer 설정

### 2.5 UIManager 완전 재작성
**파일**: `Assets/_Project/Scripts/Core/Managers/UIManager.cs`

**주요 기능**:
- ✅ Layer별 Canvas 자동 생성 (5개)
- ✅ Prefab 동적 로드 (Resources.LoadAsync)
- ✅ Panel 캐싱 (Dictionary<PanelType, BasePanel>)
- ✅ Panel Stack 관리 (뒤로가기 기능)
- ✅ ESC 키 자동 처리
- ✅ 디버그 로그 및 상태 출력

**제거된 기능**:
- ❌ UIProxy 구조체
- ❌ RegisterAllUI() 메서드
- ❌ 코드 기반 UI 생성

---

## 📝 Phase 3: Panel 스크립트 작성 ✅ (완료)

### 3.1 Assembly Definition
**파일**: `Assets/_Project/Scripts/UI/Panels/UI.Panels.asmdef`

**참조**:
- UI.Core
- Core.Enums
- Core.Utilities
- Combat.Core

**제거된 참조** (순환 참조 방지):
- ❌ Core.Managers
- ❌ UI.HUD

### 3.2 MainMenuPanel
**파일**: `Assets/_Project/Scripts/UI/Panels/MainMenuPanel.cs`

**기능**:
- 게임 시작 버튼 → GameFlowManager.StartGame() (Reflection)
- 설정 버튼 (미구현)
- 종료 버튼 → Application.Quit()

**Layer**: Background (0)
**Transition**: Fade In/Out

### 3.3 LoadingPanel
**파일**: `Assets/_Project/Scripts/UI/Panels/LoadingPanel.cs`

**기능**:
- 진행률 바 (Slider)
- 진행률 텍스트 (%)
- 로딩 팁 랜덤 표시
- UpdateProgress(float progress) 메서드

**Layer**: System (300)
**Transition**: Fade In/Out

### 3.4 GameplayHUDPanel
**파일**: `Assets/_Project/Scripts/UI/Panels/GameplayHUDPanel.cs`

**기능**:
- 체력바 (MonoBehaviour → Reflection으로 HealthBarUI 접근)
- 콤보 텍스트
- 적 카운트
- 점수
- 일시정지 버튼

**Layer**: Normal (100)
**Transition**: Fade In/Out

**중요 수정**:
- `HealthBarUI healthBar` → `MonoBehaviour healthBar`로 변경 (순환 참조 방지)
- Reflection으로 Initialize(), UpdateHealth() 호출

### 3.5 PausePanel
**파일**: `Assets/_Project/Scripts/UI/Panels/PausePanel.cs`

**기능**:
- 재개 버튼 → Close()
- 설정 버튼 (미구현)
- 메인 메뉴 버튼 → GameFlowManager.TriggerEvent(ReturnToMainMenu)
- Time.timeScale 제어 (0 ↔ 1)

**Layer**: Popup (200)
**Transition**: Scale In/Out
**ESC 키**: closeOnEscape = true

---

## 🔨 Phase 4: 기존 시스템 통합 ✅ (완료)

### 4.1 GameState.cs 수정
**파일**: `Assets/_Project/Scripts/Core/Managers/GameState.cs`

**변경 내용**:
- **MainState**: `await UIManager.Instance.OpenPanel(PanelType.MainMenu)`
- **LoadingState**:
  - `loadingPanel = await UIManager.Instance.OpenPanel(PanelType.Loading)`
  - `UpdateProgress()` 호출 방식 변경
- **IngameState**: `await UIManager.Instance.OpenPanel(PanelType.GameplayHUD)`

### 4.2 BootstrapManager.cs 수정
**파일**: `Assets/_Project/Scripts/Core/Bootstrap/BootstrapManager.cs`

**제거된 코드**:
- ❌ `CreateLoadingUI()`
- ❌ `CreateMainMenuUI()`
- ❌ `CreateGameplayUI()`
- ❌ `UIManager.Instance.RegisterAllUI()`

**현재 방식**: Panel은 필요할 때 Prefab에서 동적 로드

### 4.3 Core.Managers.asmdef 수정
**파일**: `Assets/_Project/Scripts/Core/Managers/Core.Managers.asmdef`

**추가된 참조**:
- UI.Panels (LoadingPanel 타입 접근용)

---

## 🐛 해결된 문제들

### 문제 1: 순환 참조 (UI.Panels ↔ Core.Managers)
**해결**: Reflection 패턴 사용
- MainMenuPanel: GameFlowManager.StartGame() → Reflection
- PausePanel: GameFlowManager.TriggerEvent() → Reflection
- GameplayHUDPanel: GameFlowManager.PauseGame() → Reflection

### 문제 2: 순환 참조 (UI.Panels ↔ UI.HUD)
**해결**: MonoBehaviour 타입 + Reflection
- `HealthBarUI healthBar` → `MonoBehaviour healthBar`
- Initialize(), UpdateHealth() → Reflection 호출

### 문제 3: MainMenuUI가 TransitionCanvas 하위에 생성 ⭐ **최신 해결**
**원인**: PrefabMaker가 독립 Canvas를 가진 Prefab 생성 → UIManager가 Layer Canvas 하위에 Instantiate하지만 Prefab의 Canvas가 우선시됨

**해결**:
1. `CreateCanvas()` → `CreatePanelRoot()`로 변경
2. **Canvas를 생성하지 않고 RectTransform만** 생성
3. BasePanel의 `Awake()`에서 Canvas 자동 추가
4. `canvas.overrideSorting = true`로 부모 Canvas Layer 따름

**수정 파일**: `Assets/_Project/Scripts/Tools/PrefabMaker.cs`

---

## 🛠️ Phase 5: PrefabMaker 자동화 도구 ✅ (완료)

### 5.1 PrefabMaker 스크립트
**파일**: `Assets/_Project/Scripts/Tools/PrefabMaker.cs`

**기능**:
- ✅ ContextMenu 기반 Prefab 생성
- ✅ 4개 Panel Prefab 자동 생성 (MainMenu, Loading, GameplayHUD, Pause)
- ✅ UI 요소 자동 배치 (Text, Button, Image, Slider)
- ✅ RectTransform 자동 설정
- ✅ 스크립트 필드 자동 연결 (SerializedObject)
- ✅ Resources/UI/Panels/ 폴더에 자동 저장

**ContextMenu 항목**:
- `Create All Panel Prefabs` - 전체 생성
- `Create MainMenuPanel Prefab`
- `Create LoadingPanel Prefab`
- `Create GameplayHUDPanel Prefab`
- `Create PausePanel Prefab`

**중요 수정** (2025-10-14):
- `CreateCanvas()` 제거
- `CreatePanelRoot()` 추가: Canvas 없이 RectTransform만 생성
- 각 Panel 생성 메서드에서 `CreatePanelRoot()` 사용

### 5.2 사용 가이드
**파일**: `PREFAB_MAKER_USAGE.md`

**단계**:
1. PrefabTest 씬 생성
2. PrefabMaker GameObject + 스크립트 추가
3. ContextMenu에서 "Create All Panel Prefabs" 실행
4. Bootstrap 씬에서 테스트

---

## 📁 생성된 파일 목록

### 설계 문서
- ✅ `UI_SYSTEM_DESIGN.md`
- ✅ `PREFAB_MAKER_USAGE.md`
- ✅ `PREFAB_CREATION_GUIDE.md`

### Core 시스템
- ✅ `Assets/_Project/Scripts/UI/Core/PanelType.cs`
- ✅ `Assets/_Project/Scripts/UI/Core/UILayer.cs`
- ✅ `Assets/_Project/Scripts/UI/Core/TransitionType.cs`
- ✅ `Assets/_Project/Scripts/UI/Core/BasePanel.cs`

### Panel 스크립트
- ✅ `Assets/_Project/Scripts/UI/Panels/MainMenuPanel.cs`
- ✅ `Assets/_Project/Scripts/UI/Panels/LoadingPanel.cs`
- ✅ `Assets/_Project/Scripts/UI/Panels/GameplayHUDPanel.cs`
- ✅ `Assets/_Project/Scripts/UI/Panels/PausePanel.cs`
- ✅ `Assets/_Project/Scripts/UI/Panels/UI.Panels.asmdef`

### 도구 및 자동화
- ✅ `Assets/_Project/Scripts/Tools/PrefabMaker.cs`

### 수정된 파일
- ✅ `Assets/_Project/Scripts/Core/Managers/UIManager.cs` (완전 재작성)
- ✅ `Assets/_Project/Scripts/Core/Managers/GameState.cs` (UI 호출 방식 변경)
- ✅ `Assets/_Project/Scripts/Core/Bootstrap/BootstrapManager.cs` (UI 생성 코드 제거)
- ✅ `Assets/_Project/Scripts/Core/Managers/Core.Managers.asmdef` (UI.Panels 참조 추가)

---

## 🔍 테스트 필요 사항

Unity Editor에서 다음을 테스트해야 함:

### 1. Prefab 재생성 ⚠️ **필수**
기존 Prefab 삭제 후 새로 생성 필요:

1. **기존 Prefab 삭제**:
   ```
   Assets/_Project/Resources/UI/Panels/ 폴더에서
   - MainMenuPanel.prefab 삭제
   - LoadingPanel.prefab 삭제
   - GameplayHUDPanel.prefab 삭제
   - PausePanel.prefab 삭제
   ```

2. **PrefabMaker로 재생성**:
   ```
   1. PrefabTest 씬 열기
   2. PrefabMaker GameObject 선택
   3. Inspector에서 PrefabMaker 컴포넌트 우클릭
   4. "Create All Panel Prefabs" 선택
   ```

### 2. Bootstrap 씬 실행
**테스트 플로우**:
1. Bootstrap 씬 실행
2. Preload → Main (MainMenuPanel 표시 확인)
3. "게임 시작" 버튼 클릭 → Loading (LoadingPanel 표시, 진행률 확인)
4. Ingame 진입 (GameplayHUDPanel 표시 확인)
5. ESC 키 (PausePanel 표시 확인)
6. "재개" 버튼 (PausePanel 닫힘 확인)

### 3. Layer Canvas 계층 확인
Hierarchy에서 UIManager 하위 확인:
```
UIManager
├── BackgroundCanvas (sortingOrder: 0)
│   └── MainMenuPanel
├── NormalCanvas (sortingOrder: 100)
│   └── GameplayHUDPanel
├── PopupCanvas (sortingOrder: 200)
│   └── PausePanel
├── SystemCanvas (sortingOrder: 300)
│   └── LoadingPanel
└── TransitionCanvas (sortingOrder: 9999)
```

### 4. Transition 애니메이션 확인
- MainMenuPanel: Fade In/Out
- LoadingPanel: Fade In/Out
- GameplayHUDPanel: Fade In/Out
- PausePanel: Scale In/Out

### 5. Panel 기능 확인
- [ ] MainMenuPanel 버튼 동작
- [ ] LoadingPanel 진행률 업데이트
- [ ] GameplayHUDPanel 콤보/점수/적 카운트 업데이트
- [ ] PausePanel Time.timeScale 제어

---

## 📊 UI 시스템 아키텍처

### Canvas 계층 구조 (sortingOrder)
```
┌─────────────────────────────────────┐
│  TransitionCanvas (9999)             │  ← Fade 효과
├─────────────────────────────────────┤
│  SystemCanvas (300)                  │  ← LoadingPanel
├─────────────────────────────────────┤
│  PopupCanvas (200)                   │  ← PausePanel
├─────────────────────────────────────┤
│  NormalCanvas (100)                  │  ← GameplayHUDPanel
├─────────────────────────────────────┤
│  BackgroundCanvas (0)                │  ← MainMenuPanel
└─────────────────────────────────────┘
```

### Panel 생명주기
```
┌──────────────────────────────────────┐
│  UIManager.OpenPanel(PanelType)      │
│         ↓                            │
│  LoadPanel() (Prefab 로드)           │
│         ↓                            │
│  Instantiate(parentCanvas)           │
│         ↓                            │
│  BasePanel.Awake()                   │
│    - Canvas 자동 추가                │
│    - overrideSorting = true          │
│    - sortingOrder = Layer 값         │
│         ↓                            │
│  BasePanel.Open()                    │
│    - OnBeforeOpen()                  │
│    - PlayOpenTransition()            │
│    - OnAfterOpen()                   │
└──────────────────────────────────────┘
```

### Reflection 패턴
순환 참조 방지를 위해 사용:

```csharp
// GameFlowManager 접근
var gameFlowManagerType = System.Type.GetType("GameFlow.GameFlowManager, Core.Managers");
var instanceProperty = gameFlowManagerType.GetProperty("Instance");
var instance = instanceProperty?.GetValue(null);
var startGameMethod = gameFlowManagerType.GetMethod("StartGame");
startGameMethod?.Invoke(instance, null);

// HealthBarUI 접근
var healthBarType = healthBar.GetType();
var initializeMethod = healthBarType.GetMethod("Initialize");
initializeMethod?.Invoke(healthBar, new object[] { current, max });
```

---

## 🎯 다음 작업 계획

### 즉시 할 일
- [ ] Unity Editor에서 Prefab 재생성 ⚠️ **최우선**
- [ ] Bootstrap 씬 테스트
- [ ] Canvas 계층 구조 확인
- [ ] Transition 애니메이션 확인
- [ ] Panel 기능 동작 테스트

### Phase 6: UI 추가 구현 (예정)
- [ ] **SettingsPanel** 구현
  - 사운드 볼륨 조절
  - 해상도 설정
  - 키 바인딩

- [ ] **InventoryPanel** 구현
  - 아이템 슬롯 그리드
  - 드래그 앤 드롭
  - 아이템 툴팁

- [ ] **DialogPanel** 구현
  - 확인/취소 버튼
  - 커스텀 메시지
  - 콜백 함수 지원

### 향후 UI 시스템 개선
- [ ] Panel Pooling (성능 최적화)
- [ ] Panel Preloading (로딩 시간 단축)
- [ ] Custom Editor Window (Panel 관리 도구)
- [ ] UI Sound 시스템 통합
- [ ] UI Animation Timeline 통합

---

## 💡 핵심 설계 원칙

### 1. 단일 책임 원칙
- UIManager: Panel 생명주기 관리만
- BasePanel: UI 표시/숨김/Transition만
- 각 Panel: 자신의 UI 로직만

### 2. 개방-폐쇄 원칙
- BasePanel 상속으로 새 Panel 추가 용이
- TransitionType Enum으로 애니메이션 확장 가능

### 3. 의존성 역전 원칙
- Panel → UIManager 의존 (❌)
- UIManager → Panel 의존 (✅)
- Reflection으로 Manager 간 결합도 최소화

### 4. Prefab 기반 개발
- 디자이너 친화적
- Unity Editor에서 직접 편집 가능
- 재사용성 극대화

---

## 📞 관련 문서

- **UI 시스템 설계**: [UI_SYSTEM_DESIGN.md](UI_SYSTEM_DESIGN.md)
- **PrefabMaker 사용법**: [PREFAB_MAKER_USAGE.md](PREFAB_MAKER_USAGE.md)
- **Prefab 제작 가이드**: [PREFAB_CREATION_GUIDE.md](PREFAB_CREATION_GUIDE.md)
- **프로젝트 현황**: [docs/development/CurrentStatus.md](docs/development/CurrentStatus.md)

---

## 📝 이전 세션 작업 (참고)

### 로딩 시스템 재설계 (이전 작업)
- 실제 진행률 기반 로딩 시스템 구현
- GameFlowManager 중복 전환 버그 수정
- LoadingUI RectTransform 버그 수정

**상세 내용은 git history 참고**

---

**최종 업데이트**: 2025-10-14
**작성자**: GASPT 개발팀 + Claude Code
**세션 목표**: UI 시스템 Panel 기반 재설계 완료 ✅

---

## 🚀 다음 세션 시작 시

새 세션에서 이렇게 말하세요:

```
CURRENT_WORK.md 파일을 읽었어. Unity에서 Prefab을 재생성하고 테스트 결과를 알려줄게.
```

또는 바로 테스트 결과를 알려주세요:

```
UI 시스템 테스트 완료했어. [문제 설명 또는 정상 작동 여부]
```
