# GASPT 아키텍처 다이어그램

> Mermaid 다이어그램으로 시각화한 프로젝트 구조

**Note**: 이 문서는 GitHub, GitLab, Visual Studio Code (Markdown Preview Enhanced) 등에서 Mermaid 렌더링을 지원합니다.

## 📋 목차
1. [전체 시스템 아키텍처](#전체-시스템-아키텍처)
2. [레이어 의존성 다이어그램](#레이어-의존성-다이어그램)
3. [GameFlow 상태 다이어그램](#gameflow-상태-다이어그램)
4. [씬 전환 시퀀스](#씬-전환-시퀀스)
5. [UI 시스템 구조](#ui-시스템-구조)
6. [클래스 다이어그램](#클래스-다이어그램)

---

## 전체 시스템 아키텍처

```mermaid
graph TB
    subgraph "Layer 5: Gameplay"
        Player[Player System]
        Enemy[Enemy System]
        Combat[Combat System]
        Skull[Skull System]
        GM[Gameplay Manager]
    end

    subgraph "Layer 4: UI"
        UIPanel[UI Panels]
        UIMenu[UI Menu]
        UIHUD[UI HUD]
    end

    subgraph "Layer 3: Core"
        Bootstrap[Bootstrap Manager]
        GameFlow[GameFlow Manager]
        SceneLoader[Scene Loader]
        SceneTransition[Scene Transition]
        UIManager[UI Manager]
    end

    subgraph "Layer 2: Foundation"
        CoreEnums[Core.Enums]
        CoreUtils[Core.Utilities]
        CoreData[Core.Data]
        UICore[UI.Core]
    end

    subgraph "Layer 1: Plugins"
        FSM[FSM.Core]
        GAS[GAS.Core]
        FSMGAS[FSM.GAS.Integration]
    end

    %% Dependencies
    Player --> FSM
    Player --> GAS
    Player --> Combat
    Enemy --> FSM
    Combat --> CoreEnums

    UIPanel --> UICore
    UIPanel --> GameFlow
    UIPanel --> Combat

    GameFlow --> FSM
    GameFlow --> UICore
    UIManager --> UICore

    UICore --> CoreUtils

    GAS --> CoreEnums
    FSMGAS --> FSM
    FSMGAS --> GAS

    %% Styling
    classDef plugin fill:#e1f5ff,stroke:#01579b,stroke-width:2px
    classDef foundation fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef core fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef ui fill:#e8f5e9,stroke:#1b5e20,stroke-width:2px
    classDef gameplay fill:#fce4ec,stroke:#880e4f,stroke-width:2px

    class FSM,GAS,FSMGAS plugin
    class CoreEnums,CoreUtils,CoreData,UICore foundation
    class Bootstrap,GameFlow,SceneLoader,SceneTransition,UIManager core
    class UIPanel,UIMenu,UIHUD ui
    class Player,Enemy,Combat,Skull,GM gameplay
```

---

## 레이어 의존성 다이어그램

```mermaid
graph TD
    subgraph "어셈블리 의존성 (위에서 아래로 참조)"
        direction TB

        Player[Player.asmdef]
        Enemy[Enemy.asmdef]
        Skull[Skull.asmdef]

        UIPanel[UI.Panels.asmdef]

        CoreMgr[Core.Managers.asmdef]

        CombatCore[Combat.Core.asmdef]
        CombatAttack[Combat.Attack.asmdef]

        UICore[UI.Core.asmdef]
        CoreEnum[Core.Enums.asmdef]
        CoreUtil[Core.Utilities.asmdef]
        CoreData[Core.Data.asmdef]

        FSM[FSM.Core.asmdef]
        GAS[GAS.Core.asmdef]

        Player --> FSM
        Player --> GAS
        Player --> CombatCore
        Player --> CombatAttack
        Player --> CoreMgr

        Enemy --> FSM
        Enemy --> CombatCore

        Skull --> GAS

        UIPanel --> UICore
        UIPanel --> CoreMgr
        UIPanel --> CombatCore

        CoreMgr --> FSM
        CoreMgr --> UICore
        CoreMgr --> CoreEnum
        CoreMgr --> CoreUtil
        CoreMgr --> CoreData

        CombatCore --> CoreEnum
        CombatAttack --> CombatCore

        UICore --> CoreUtil

        GAS --> CoreEnum
    end

    style Player fill:#ffcdd2
    style Enemy fill:#ffcdd2
    style Skull fill:#ffcdd2
    style UIPanel fill:#c8e6c9
    style CoreMgr fill:#ce93d8
    style CombatCore fill:#fff9c4
    style CombatAttack fill:#fff9c4
    style UICore fill:#c8e6c9
    style CoreEnum fill:#ffe0b2
    style CoreUtil fill:#ffe0b2
    style CoreData fill:#ffe0b2
    style FSM fill:#b3e5fc
    style GAS fill:#b3e5fc
```

---

## GameFlow 상태 다이어그램

```mermaid
stateDiagram-v2
    [*] --> Preload: 게임 시작

    Preload: Preload State
    note right of Preload
        리소스 로딩
        데이터 초기화
    end note

    Main: Main State
    note right of Main
        MainMenuPanel 표시
        메인 메뉴 씬
    end note

    Loading: Loading State
    note right of Loading
        LoadingPanel 표시
        Gameplay 씬 로딩
    end note

    Ingame: Ingame State
    note right of Ingame
        GameplayHUDPanel 표시
        게임플레이 진행
    end note

    Pause: Pause State
    note right of Pause
        PausePanel 표시
        Time.timeScale = 0
    end note

    Preload --> Main: LoadComplete
    Main --> Loading: StartGame()
    Loading --> Ingame: LoadComplete
    Ingame --> Pause: ESC / PauseGame()
    Pause --> Ingame: 계속하기 / ResumeGame()
    Pause --> Main: 메인 메뉴
    Ingame --> Main: GameOver / BackToMenu()
```

---

## 씬 전환 시퀀스

### Bootstrap → Preload → Main 시퀀스

```mermaid
sequenceDiagram
    participant Unity
    participant Bootstrap as BootstrapManager
    participant SceneLoader
    participant Transition as SceneTransitionManager
    participant GameFlow as GameFlowManager
    participant UI as UIManager

    Unity->>Bootstrap: Start()

    rect rgb(230, 245, 255)
        Note over Bootstrap: 1. 매니저 초기화
        Bootstrap->>Bootstrap: CreateEventSystem()
        Bootstrap->>SceneLoader: CreateManager<SceneLoader>()
        Bootstrap->>Transition: CreateManager<SceneTransitionManager>()
        Bootstrap->>UI: CreateManager<UIManager>()
        Bootstrap->>GameFlow: CreateManager<GameFlowManager>()
        Note over Bootstrap,GameFlow: 모두 DontDestroyOnLoad
    end

    rect rgb(255, 245, 230)
        Note over Bootstrap: 2. Preload 씬 로드
        Bootstrap->>Transition: SetFadeOut()
        Bootstrap->>SceneLoader: LoadSceneAsync(Preload)
        SceneLoader-->>Bootstrap: Awaitable
        Bootstrap->>GameFlow: StartManually(Preload)
        GameFlow->>GameFlow: TransitionTo(Preload)
        Bootstrap->>Transition: FadeInAsync()
    end

    rect rgb(245, 255, 230)
        Note over GameFlow: 3. Preload State
        GameFlow->>GameFlow: PreloadState.EnterState()
        GameFlow->>GameFlow: 리소스 로딩...
        GameFlow->>GameFlow: TransitionTo(Main)
    end

    rect rgb(255, 230, 245)
        Note over GameFlow: 4. Main State
        GameFlow->>Transition: FadeOutAsync(0.3s)
        GameFlow->>SceneLoader: LoadSceneAsync(Main)
        SceneLoader-->>GameFlow: Awaitable
        GameFlow->>UI: OpenPanel(MainMenu)
        UI->>UI: MainMenuPanel.Open()
        GameFlow->>Transition: FadeInAsync(0.5s)
    end
```

### Main → Loading → Ingame 시퀀스

```mermaid
sequenceDiagram
    participant User
    participant Panel as MainMenuPanel
    participant GameFlow as GameFlowManager
    participant UI as UIManager
    participant SceneLoader
    participant LoadingPanel

    rect rgb(230, 245, 255)
        Note over User,Panel: 1. 시작 버튼 클릭
        User->>Panel: Click 시작 버튼
        Panel->>GameFlow: StartGame()
        GameFlow->>GameFlow: TransitionTo(Loading)
    end

    rect rgb(255, 245, 230)
        Note over GameFlow: 2. Loading State
        GameFlow->>UI: ClosePanel(MainMenu)
        GameFlow->>UI: OpenPanel(Loading)
        UI->>LoadingPanel: Open()
        LoadingPanel->>LoadingPanel: ShowRandomTip()
        LoadingPanel->>LoadingPanel: UpdateProgress(0%)
    end

    rect rgb(245, 255, 230)
        Note over GameFlow: 3. Gameplay 씬 로딩
        loop 로딩 진행
            GameFlow->>SceneLoader: LoadSceneAsync(Gameplay)
            SceneLoader-->>LoadingPanel: progress
            LoadingPanel->>LoadingPanel: UpdateProgress(progress)
        end
        SceneLoader-->>GameFlow: 로딩 완료
    end

    rect rgb(255, 230, 245)
        Note over GameFlow: 4. Ingame State
        GameFlow->>GameFlow: TransitionTo(Ingame)
        GameFlow->>UI: ClosePanel(Loading)
        GameFlow->>UI: OpenPanel(GameplayHUD)
        UI->>UI: GameplayHUDPanel.Open()
        GameFlow->>GameFlow: Time.timeScale = 1
    end
```

---

## UI 시스템 구조

### Panel 생명주기

```mermaid
stateDiagram-v2
    [*] --> NotLoaded: 초기 상태

    NotLoaded --> Loaded: LoadPanel()
    note right of Loaded
        Instantiate(prefab)
        SetActive(false)
        panelCache에 저장
    end note

    Loaded --> Opened: OpenPanel()
    note right of Opened
        SetActive(true)
        OnOpened 이벤트
    end note

    Opened --> Closed: ClosePanel()
    note right of Closed
        SetActive(false)
        OnClosed 이벤트
    end note

    Closed --> Opened: OpenPanel()
    Closed --> Unloaded: UnloadPanel()
    note right of Unloaded
        Destroy(panel)
        panelCache에서 제거
    end note

    Unloaded --> [*]
```

### UIManager 구조

```mermaid
graph TB
    subgraph UIManager
        Cache[panelCache<br/>Dictionary]
        ParentCanvas[parentCanvas<br/>Transform]

        Load[LoadPanel]
        Open[OpenPanel]
        Close[ClosePanel]
        Preload[PreloadPanel]
        Unload[UnloadPanel]
    end

    subgraph "Panel Instances"
        MainMenu[MainMenuPanel]
        Loading[LoadingPanel]
        GameHUD[GameplayHUDPanel]
        Pause[PausePanel]
    end

    subgraph "Resources/UI"
        PrefabMain[MainMenuPanel.prefab]
        PrefabLoad[LoadingPanel.prefab]
        PrefabHUD[GameplayHUDPanel.prefab]
        PrefabPause[PausePanel.prefab]
    end

    Load --> Cache
    Load --> PrefabMain
    Load --> PrefabLoad
    Load --> PrefabHUD
    Load --> PrefabPause

    Open --> Load
    Open --> MainMenu
    Open --> Loading
    Open --> GameHUD
    Open --> Pause

    Close --> MainMenu
    Close --> Loading
    Close --> GameHUD
    Close --> Pause

    Unload --> Cache

    MainMenu --> ParentCanvas
    Loading --> ParentCanvas
    GameHUD --> ParentCanvas
    Pause --> ParentCanvas
```

---

## 클래스 다이어그램

### GameFlow 시스템

```mermaid
classDiagram
    class GameFlowManager {
        -StateMachine~GameStateType~ stateMachine
        -GameStateType currentStateType
        +Instance$ GameFlowManager
        +StartGame() void
        +PauseGame() void
        +ResumeGame() void
        +BackToMainMenu() void
        -InitializeStates() void
        -SetupTransitions() void
    }

    class GameStateBase {
        #GameFlowManager gameFlowManager
        +EnterAsync(CancellationToken) Awaitable
        +ExitAsync(CancellationToken) Awaitable
        #EnterState(CancellationToken) Awaitable
        #ExitState(CancellationToken) Awaitable
    }

    class PreloadState {
        #EnterState(CancellationToken) Awaitable
        #ExitState(CancellationToken) Awaitable
    }

    class MainState {
        #EnterState(CancellationToken) Awaitable
        #ExitState(CancellationToken) Awaitable
    }

    class LoadingState {
        #EnterState(CancellationToken) Awaitable
        #ExitState(CancellationToken) Awaitable
    }

    class IngameState {
        #EnterState(CancellationToken) Awaitable
        #ExitState(CancellationToken) Awaitable
    }

    class PauseState {
        #EnterState(CancellationToken) Awaitable
        #ExitState(CancellationToken) Awaitable
    }

    class IState~TStateType~ {
        <<interface>>
        +Enter() void
        +Update() void
        +Exit() void
        +EnterAsync(CancellationToken) Awaitable
        +ExitAsync(CancellationToken) Awaitable
    }

    class StateMachine~TStateType~ {
        -Dictionary~TStateType, IState~ states
        -IState currentState
        -TStateType currentStateType
        +AddState(TStateType, IState) void
        +TransitionTo(TStateType) void
        +Update() void
    }

    GameFlowManager --> StateMachine~GameStateType~
    GameFlowManager --> GameStateBase

    GameStateBase ..|> IState~GameStateType~

    PreloadState --|> GameStateBase
    MainState --|> GameStateBase
    LoadingState --|> GameStateBase
    IngameState --|> GameStateBase
    PauseState --|> GameStateBase

    StateMachine~GameStateType~ --> IState~GameStateType~
```

### UI 시스템

```mermaid
classDiagram
    class UIManager {
        -Dictionary~PanelType, BasePanel~ panelCache
        -Transform parentCanvas
        +Instance$ UIManager
        +OpenPanel(PanelType) Awaitable
        +ClosePanel(PanelType) void
        +PreloadPanel(PanelType) Awaitable
        +UnloadPanel(PanelType) void
        -LoadPanel(PanelType) Awaitable~BasePanel~
    }

    class BasePanel {
        #PanelType panelType
        #UILayer layer
        #TransitionType openTransition
        #TransitionType closeTransition
        #float transitionDuration
        #bool closeOnEscape
        +bool IsOpen
        +event Action~BasePanel~ OnOpened
        +event Action~BasePanel~ OnClosed
        +void Open()
        +void Close()
        +bool CloseOnEscape
        +virtual void UpdateProgress(float)
    }

    class MainMenuPanel {
        -Button startButton
        -Button settingsButton
        -Button quitButton
        -Text titleText
        -OnStartButtonClicked() void
        -OnSettingsButtonClicked() void
        -OnQuitButtonClicked() void
    }

    class LoadingPanel {
        -Slider progressBar
        -Text progressText
        -Text loadingTipText
        -string[] loadingTips
        +override void UpdateProgress(float)
        -ShowRandomTip() void
    }

    class GameplayHUDPanel {
        -MonoBehaviour healthBar
        -Text comboText
        -Text enemyCountText
        -Text scoreText
        -Button pauseButton
        +UpdateCombo(int) void
        +UpdateEnemyCount(int) void
        +UpdateScore(int) void
        +AddScore(int) void
        -SetupHealthSystem() void
    }

    class PausePanel {
        -Button resumeButton
        -Button settingsButton
        -Button mainMenuButton
        -OnResumeButtonClicked() void
        -OnMainMenuButtonClicked() void
    }

    UIManager --> BasePanel

    MainMenuPanel --|> BasePanel
    LoadingPanel --|> BasePanel
    GameplayHUDPanel --|> BasePanel
    PausePanel --|> BasePanel
```

### FSM 시스템

```mermaid
classDiagram
    class IStateMachine~TStateType~ {
        <<interface>>
        +AddState(TStateType, IState) void
        +RemoveState(TStateType) void
        +TransitionTo(TStateType) void
        +AddTransition(TStateType, TStateType, Func~bool~) void
        +Update() void
    }

    class StateMachine~TStateType~ {
        -Dictionary~TStateType, IState~ states
        -IState currentState
        -TStateType currentStateType
        -List~Transition~ transitions
        +AddState(TStateType, IState) void
        +RemoveState(TStateType) void
        +TransitionTo(TStateType) void
        +AddTransition(TStateType, TStateType, Func~bool~) void
        +Update() void
    }

    class IState~TStateType~ {
        <<interface>>
        +Enter() void
        +Update() void
        +Exit() void
        +EnterAsync(CancellationToken) Awaitable
        +ExitAsync(CancellationToken) Awaitable
    }

    class State~TStateType~ {
        +virtual void Enter()
        +virtual void Update()
        +virtual void Exit()
        +virtual Awaitable EnterAsync(CancellationToken)
        +virtual Awaitable ExitAsync(CancellationToken)
    }

    class Transition {
        +TStateType From
        +TStateType To
        +Func~bool~ Condition
    }

    StateMachine~TStateType~ ..|> IStateMachine~TStateType~
    StateMachine~TStateType~ --> IState~TStateType~
    StateMachine~TStateType~ --> Transition

    State~TStateType~ ..|> IState~TStateType~
```

---

## 데이터 흐름 다이어그램

### 게임 시작부터 플레이까지의 데이터 흐름

```mermaid
flowchart TB
    Start([게임 실행])

    subgraph Bootstrap["Bootstrap 씬"]
        B1[BootstrapManager.Start]
        B2[EventSystem 생성]
        B3[매니저들 생성<br/>DontDestroyOnLoad]
        B4[Preload 씬 로드]
    end

    subgraph Preload["Preload 씬"]
        P1[리소스 매니페스트 로드]
        P2[필수 리소스 로딩]
        P3[데이터 초기화]
        P4[Main으로 전환]
    end

    subgraph Main["Main 씬"]
        M1[Main 씬 로드]
        M2[MainMenuPanel 표시]
        M3{사용자 입력}
        M4[시작 버튼]
        M5[설정 버튼]
        M6[종료 버튼]
    end

    subgraph Loading["Loading 씬"]
        L1[LoadingPanel 표시]
        L2[Gameplay 씬 로드]
        L3[진행률 업데이트]
        L4[로딩 완료]
    end

    subgraph Gameplay["Gameplay 씬"]
        G1[GameplayHUDPanel 표시]
        G2[플레이어 초기화]
        G3[적 스폰]
        G4[게임 루프]
        G5{ESC?}
        G6[PausePanel 표시]
    end

    Start --> B1
    B1 --> B2 --> B3 --> B4
    B4 --> P1
    P1 --> P2 --> P3 --> P4
    P4 --> M1
    M1 --> M2 --> M3
    M3 --> M4
    M3 --> M5
    M3 --> M6
    M4 --> L1
    L1 --> L2 --> L3 --> L4
    L4 --> G1
    G1 --> G2 --> G3 --> G4
    G4 --> G5
    G5 -->|Yes| G6
    G5 -->|No| G4
    G6 -->|계속하기| G4
    G6 -->|메인 메뉴| M1
    M6 --> End([게임 종료])

    style Bootstrap fill:#e3f2fd
    style Preload fill:#fff3e0
    style Main fill:#f3e5f5
    style Loading fill:#e8f5e9
    style Gameplay fill:#fce4ec
```

---

## 성능 최적화 포인트

### Panel 로딩 전략

```mermaid
graph LR
    subgraph "즉시 로딩 (Preload)"
        LP[LoadingPanel<br/>즉시 사용]
        PP[PausePanel<br/>빠른 반응 필요]
    end

    subgraph "지연 로딩 (Lazy Load)"
        MP[MainMenuPanel<br/>시간 여유]
        GP[GameplayHUDPanel<br/>로딩 중 생성 가능]
    end

    subgraph "사용 후 언로드"
        LP2[LoadingPanel<br/>Ingame 진입 후]
    end

    Bootstrap[Bootstrap] --> LP
    Bootstrap --> PP

    MainState[MainState] --> MP
    LoadingState[LoadingState] --> GP

    IngameState[IngameState] -.Unload.-> LP2

    style LP fill:#ffcdd2
    style PP fill:#ffcdd2
    style MP fill:#c8e6c9
    style GP fill:#c8e6c9
    style LP2 fill:#fff9c4
```

---

**작성일**: 2025-10-15
**버전**: 1.0
**도구**: Mermaid.js

## 다이어그램 렌더링 방법

### VS Code
1. "Markdown Preview Enhanced" 확장 설치
2. Ctrl+Shift+V로 미리보기

### GitHub
- `.md` 파일을 GitHub에 푸시하면 자동 렌더링

### 온라인
- https://mermaid.live/ 에서 코드 복사하여 렌더링
