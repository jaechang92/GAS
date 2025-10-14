# UI System Design Document

## 📋 목차

1. [개요](#개요)
2. [아키텍처 설계](#아키텍처-설계)
3. [핵심 컴포넌트](#핵심-컴포넌트)
4. [사용 가이드](#사용-가이드)
5. [마이그레이션 계획](#마이그레이션-계획)

---

## 개요

### 목표
- **상용 게임 수준의 Prefab 기반 UI 시스템 구축**
- Panel 상속 구조를 통한 체계적인 UI 관리
- 재사용 가능하고 확장 가능한 아키텍처

### 설계 원칙
1. **계층 분리**: Background, Normal, Popup, System, Transition 레이어로 구분
2. **생명주기 관리**: Open/Close + Transition 애니메이션 자동 처리
3. **Prefab 기반**: 모든 UI는 Prefab으로 제작하여 디자이너 친화적
4. **Stack 관리**: 뒤로가기(ESC) 기능 지원
5. **비동기 처리**: Awaitable 기반의 현대적인 비동기 패턴

---

## 아키텍처 설계

### 전체 구조도

```
┌─────────────────────────────────────────────────────────┐
│                      UIManager                          │
│  ┌───────────────────────────────────────────────────┐  │
│  │ - Panel 생명주기 관리 (Open/Close)                 │  │
│  │ - Prefab 로딩 및 캐싱                              │  │
│  │ - Layer별 Canvas 관리                              │  │
│  │ - Panel Stack (뒤로가기)                           │  │
│  │ - 전역 이벤트 처리                                 │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                            │
        ┌───────────────────┼────────────────────┐
        ▼                   ▼                    ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ UILayer      │    │ UILayer      │    │ UILayer      │
│ Background   │    │ Normal       │    │ Popup        │
│              │    │              │    │              │
│ order: 0     │    │ order: 100   │    │ order: 200   │
└──────────────┘    └──────────────┘    └──────────────┘
        │                   │                    │
        └───────────────────┼────────────────────┘
                            ▼
                ┌─────────────────────┐
                │     BasePanel       │
                │                     │
                │ ┌─────────────────┐ │
                │ │ - Show/Hide     │ │
                │ │ - Transition    │ │
                │ │ - Lifecycle     │ │
                │ │ - Events        │ │
                │ └─────────────────┘ │
                └─────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        ▼                   ▼                   ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│MainMenuPanel │    │ LoadingPanel │    │GameplayHUD   │
└──────────────┘    └──────────────┘    └──────────────┘
```

### Layer 시스템

| Layer | SortingOrder | 용도 | 예시 |
|-------|--------------|------|------|
| **Background** | 0 | 전체 화면 배경 UI | MainMenu, Lobby |
| **Normal** | 100 | 일반 게임 UI | GameplayHUD, Inventory |
| **Popup** | 200 | 팝업 창 | Pause, Settings, Dialog |
| **System** | 300 | 시스템 UI | Loading, Toast |
| **Transition** | 9999 | 화면 전환 | FadePanel |

### Panel Type 정의

```csharp
public enum PanelType
{
    None,

    // Background Layer (0)
    MainMenu,           // 메인 메뉴
    Lobby,              // 로비 화면

    // Normal Layer (100)
    GameplayHUD,        // 게임플레이 HUD
    Inventory,          // 인벤토리
    Shop,               // 상점
    CharacterStatus,    // 캐릭터 정보

    // Popup Layer (200)
    Pause,              // 일시정지
    Settings,           // 설정
    Dialog,             // 대화창
    Reward,             // 보상 팝업
    Confirm,            // 확인 대화상자

    // System Layer (300)
    Loading,            // 로딩 화면
    Toast,              // 토스트 메시지

    // Transition Layer (9999)
    Fade                // 페이드 전환
}
```

---

## 핵심 컴포넌트

### 1. BasePanel

**역할**: 모든 UI Panel의 기본 클래스

**주요 기능**:
- ✅ 생명주기 관리 (Open/Close)
- ✅ Transition 애니메이션 (Fade, Scale, Slide 등)
- ✅ Layer 자동 설정
- ✅ 이벤트 시스템 (OnOpened, OnClosed)
- ✅ ESC 키 처리 지원

**생명주기**:
```
Open() 호출
    ↓
OnBeforeOpen() - 전처리
    ↓
PlayOpenTransition() - 애니메이션
    ↓
OnAfterOpen() - 후처리
    ↓
OnOpened 이벤트 발생

Close() 호출
    ↓
OnBeforeClose() - 전처리
    ↓
PlayCloseTransition() - 애니메이션
    ↓
OnAfterClose() - 후처리
    ↓
OnClosed 이벤트 발생
```

**하위 클래스에서 오버라이드 가능한 메서드**:
```csharp
protected virtual Awaitable OnBeforeOpen()   // Panel 열기 전
protected virtual Awaitable OnAfterOpen()    // Panel 열린 후
protected virtual Awaitable OnBeforeClose()  // Panel 닫기 전
protected virtual Awaitable OnAfterClose()   // Panel 닫힌 후
```

### 2. UIManager

**역할**: 모든 Panel을 중앙에서 관리

**주요 기능**:
- ✅ Prefab 로딩 및 캐싱
- ✅ Panel 생명주기 관리
- ✅ Layer별 Canvas 자동 생성
- ✅ Panel Stack (뒤로가기 기능)
- ✅ 열려있는 Panel 추적

**API**:
```csharp
// Panel 열기
await UIManager.Instance.OpenPanel<MainMenuPanel>();
await UIManager.Instance.OpenPanel(PanelType.Pause, addToStack: true);

// Panel 닫기
await UIManager.Instance.ClosePanel<PausePanel>();
await UIManager.Instance.ClosePanel(PanelType.Settings);

// 뒤로가기
await UIManager.Instance.GoBack();

// 모든 Panel 닫기
await UIManager.Instance.CloseAllPanels();
await UIManager.Instance.CloseAllPanels(UILayer.Popup); // 특정 Layer만
```

### 3. Transition System

**지원하는 Transition 타입**:

| Type | 설명 | 사용 예시 |
|------|------|----------|
| **None** | 애니메이션 없음 | 즉시 표시 |
| **Fade** | 투명도 전환 | 일반적인 UI |
| **Scale** | 크기 전환 | 팝업 |
| **SlideFromBottom** | 아래에서 슬라이드 | 토스트, 하단 팝업 |
| **SlideToBottom** | 아래로 슬라이드 | 닫기 애니메이션 |
| **SlideFromLeft** | 왼쪽에서 슬라이드 | 메뉴, 인벤토리 |
| **SlideToLeft** | 왼쪽으로 슬라이드 | 닫기 애니메이션 |

**설정 방법**:
```csharp
[SerializeField] protected TransitionType openTransition = TransitionType.Fade;
[SerializeField] protected TransitionType closeTransition = TransitionType.Fade;
[SerializeField] protected float transitionDuration = 0.3f;
```

### 4. Panel Stack (뒤로가기)

**동작 방식**:
1. Panel 열 때 `addToStack: true` 옵션 사용
2. ESC 키 입력 시 Stack 최상위 Panel 닫기
3. LIFO(Last In First Out) 방식

**예시**:
```csharp
// 메인 메뉴 → 설정 → 키 설정
await UIManager.Instance.OpenPanel(PanelType.MainMenu);
await UIManager.Instance.OpenPanel(PanelType.Settings, addToStack: true);
await UIManager.Instance.OpenPanel(PanelType.KeyBinding, addToStack: true);

// ESC 키 3번으로 메인 메뉴까지 복귀
// 1. KeyBinding 닫기
// 2. Settings 닫기
// 3. MainMenu는 Stack에 없으므로 유지
```

---

## 사용 가이드

### 새로운 Panel 만들기

#### Step 1: Prefab 제작
1. Hierarchy에서 UI → Canvas 생성
2. Canvas 하위에 Panel UI 구성 (버튼, 텍스트 등)
3. `Resources/UI/Panels/` 폴더에 Prefab 저장
   - 파일명: `{PanelType}Panel.prefab` (예: `PausePanel.prefab`)

#### Step 2: PanelType 등록
```csharp
// Assets/_Project/Scripts/UI/Core/PanelType.cs
public enum PanelType
{
    // ...
    Pause,  // 추가
}
```

#### Step 3: Panel 스크립트 작성
```csharp
// Assets/_Project/Scripts/UI/Panels/PausePanel.cs
using UnityEngine;
using UnityEngine.UI;
using UI.Core;

namespace UI.Panels
{
    public class PausePanel : BasePanel
    {
        [Header("UI References")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        protected override void Awake()
        {
            base.Awake();

            // Panel 설정
            panelType = PanelType.Pause;
            layer = UILayer.Popup;
            closeOnEscape = true;

            // 이벤트 연결
            resumeButton.onClick.AddListener(OnResumeClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        protected override async Awaitable OnAfterOpen()
        {
            // 게임 일시정지
            Time.timeScale = 0f;
            Debug.Log("[PausePanel] 일시정지");
            await base.OnAfterOpen();
        }

        protected override async Awaitable OnBeforeClose()
        {
            // 게임 재개
            Time.timeScale = 1f;
            Debug.Log("[PausePanel] 재개");
            await base.OnBeforeClose();
        }

        private void OnResumeClicked()
        {
            _ = Close();
        }

        private async void OnSettingsClicked()
        {
            await UIManager.Instance.OpenPanel(PanelType.Settings, addToStack: true);
        }

        private void OnQuitClicked()
        {
            _ = Close();
            // 메인 메뉴로 복귀 로직
        }
    }
}
```

#### Step 4: Prefab에 스크립트 연결
1. Prefab 열기
2. Root에 `PausePanel` 스크립트 추가
3. Inspector에서 버튼 연결
4. Transition 설정 (Fade, Scale 등)
5. 저장

### GameState에서 사용하기

```csharp
// PauseState.cs
protected override async Awaitable EnterState(CancellationToken cancellationToken)
{
    // 일시정지 Panel 표시
    await UIManager.Instance.OpenPanel(PanelType.Pause, addToStack: true);
    await Awaitable.NextFrameAsync();
}

protected override async Awaitable ExitState(CancellationToken cancellationToken)
{
    // 일시정지 Panel 닫기
    await UIManager.Instance.ClosePanel(PanelType.Pause);
    await Awaitable.NextFrameAsync();
}
```

### 커스텀 Transition 만들기

```csharp
public class MyCustomPanel : BasePanel
{
    protected override async Awaitable PlayOpenTransition()
    {
        // 커스텀 애니메이션
        transform.localScale = Vector3.zero;
        transform.rotation = Quaternion.Euler(0, 0, 180);

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            transform.rotation = Quaternion.Lerp(
                Quaternion.Euler(0, 0, 180),
                Quaternion.identity,
                t
            );

            await Awaitable.NextFrameAsync();
        }
    }
}
```

---

## 마이그레이션 계획

### Phase 1: 기반 시스템 구축 (1-2일)

**목표**: 새로운 UI 시스템의 핵심 인프라 구축

**작업 항목**:
- [x] 설계 문서 작성
- [ ] `PanelType.cs` 작성
- [ ] `UILayer.cs` 작성
- [ ] `TransitionType.cs` 작성
- [ ] `BasePanel.cs` 작성
- [ ] 새로운 `UIManager.cs` 작성

**파일 위치**:
```
Assets/_Project/Scripts/UI/Core/
├── PanelType.cs
├── UILayer.cs
├── TransitionType.cs
├── BasePanel.cs
└── UIManager.cs
```

### Phase 2: Prefab 제작 (2-3일)

**목표**: 기존 UI를 Prefab으로 재작성

**작업 항목**:
- [ ] MainMenuPanel Prefab 제작
- [ ] LoadingPanel Prefab 제작
- [ ] GameplayHUDPanel Prefab 제작
- [ ] PausePanel Prefab 제작 (신규)

**Prefab 위치**:
```
Assets/_Project/Resources/UI/Panels/
├── MainMenuPanel.prefab
├── LoadingPanel.prefab
├── GameplayHUDPanel.prefab
└── PausePanel.prefab
```

### Phase 3: Panel 스크립트 작성 (1-2일)

**목표**: 각 Panel의 로직 구현

**작업 항목**:
- [ ] `MainMenuPanel.cs` 작성
- [ ] `LoadingPanel.cs` 작성
- [ ] `GameplayHUDPanel.cs` 작성
- [ ] `PausePanel.cs` 작성

**파일 위치**:
```
Assets/_Project/Scripts/UI/Panels/
├── MainMenuPanel.cs
├── LoadingPanel.cs
├── GameplayHUDPanel.cs
└── PausePanel.cs
```

### Phase 4: 통합 및 테스트 (1-2일)

**목표**: 기존 시스템과 통합 및 검증

**작업 항목**:
- [ ] GameState와 새 UIManager 통합
- [ ] BootstrapManager 수정
- [ ] 기존 UI 코드 제거 (LoadingUI, MainMenuUI 등)
- [ ] 전체 게임 플로우 테스트
- [ ] 버그 수정 및 최적화

### 기존 코드와의 호환성

**제거할 파일들**:
```
Assets/_Project/Scripts/UI/Menu/
├── LoadingUI.cs (삭제)
├── MainMenuUI.cs (삭제)

Assets/_Project/Scripts/UI/HUD/
└── GameplayUI.cs (삭제)

Assets/_Project/Scripts/Core/Managers/
└── UIManager.cs (새로운 버전으로 교체)
```

**수정할 파일들**:
```
GameState.cs
├── LoadingState → LoadingPanel 사용
├── MainState → MainMenuPanel 사용
├── IngameState → GameplayHUDPanel 사용
└── PauseState → PausePanel 사용

BootstrapManager.cs
└── 새로운 UIManager 초기화
```

---

## 추가 기능 (향후 확장)

### 1. UI 애니메이션 프리셋
- DOTween 통합
- 애니메이션 프리셋 ScriptableObject

### 2. UI 사운드 시스템
- Panel Open/Close 사운드
- 버튼 클릭 사운드

### 3. UI Pooling
- 자주 사용하는 Panel Pool 관리
- 성능 최적화

### 4. UI Theme 시스템
- 색상, 폰트 등 테마 ScriptableObject
- 다크모드 지원

### 5. UI Analytics
- Panel 오픈/클로즈 로그
- 사용자 행동 분석

---

## 참고 자료

### 유사 시스템을 사용하는 게임
- **Genshin Impact**: Panel 기반 UI 시스템
- **Honkai: Star Rail**: Layer 구조 + Panel 관리
- **Zenless Zone Zero**: Transition 효과 + Stack 관리

### Unity Asset Store 참고
- UI Manager (Zenject 기반)
- NGUI Panel System
- UI Toolkit Panel Framework

---

## 버전 히스토리

| 버전 | 날짜 | 내용 |
|------|------|------|
| 1.0.0 | 2025-01-XX | 초기 설계 문서 작성 |

---

**작성자**: Claude Code
**최종 수정일**: 2025-01-XX
