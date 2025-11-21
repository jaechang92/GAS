# GASPT 아키텍처 다이어그램

> **목적**: 아키텍처를 시각적으로 이해하기 위한 다이어그램 모음
> **도구**: Mermaid (GitHub, Notion 등에서 렌더링 가능)

---

## 1. 전체 시스템 아키텍처

```mermaid
graph TB
    subgraph "전역 계층 (DontDestroyOnLoad)"
        GM[GameManager<br/>Singleton]
        RM[RunManager<br/>런 데이터]
        MPM[MetaProgressionManager<br/>영구 데이터]
        SM[SaveManager<br/>ISaveService]
        UIM[UIManager<br/>Singleton]

        GM --> RM
        GM --> MPM
        GM --> SM
    end

    subgraph "씬 계층 (씬마다 생성/파괴)"
        DM[DungeonManager<br/>SceneSingleton]
        Player[Player]
        PS[PlayerStats]
        INV[InventorySystem<br/>Singleton]

        Player --> PS
    end

    subgraph "ScriptableObject 계층"
        IEC[InventoryEventChannel]
        REC[RunEventChannel]
        PEC[PlayerEventChannel]
    end

    subgraph "UI 계층"
        IUI[InventoryUI]
        HUI[HudUI]
        PUI[PauseUI]
        MUI[MinimapUI]

        UIM --> IUI
        UIM --> HUI
        UIM --> PUI
        UIM --> MUI
    end

    RM -.참조.-> PS
    GM -.프록시.-> PS
    INV -.이벤트.-> IEC
    RM -.이벤트.-> REC
    PS -.이벤트.-> PEC
    IUI -.구독.-> IEC
    HUI -.구독.-> REC
    HUI -.구독.-> PEC

    style GM fill:#ff6b6b
    style RM fill:#4ecdc4
    style MPM fill:#45b7d1
    style UIM fill:#f9ca24
    style IEC fill:#6c5ce7
    style REC fill:#6c5ce7
    style PEC fill:#6c5ce7
```

---

## 2. GameManager 클래스 다이어그램

```mermaid
classDiagram
    class SingletonManager~T~ {
        <<abstract>>
        +static T Instance
        +static bool HasInstance
        #virtual OnAwake()
    }

    class GameManager {
        +RunManager Run
        +MetaProgressionManager Meta
        +SaveManager Save
        +PlayerStats PlayerStats
        +int CurrentStage
        +GameState CurrentState
        +StartNewRun()
        +EndRunVictory()
        +EndRunDefeat()
        +Pause()
        +Resume()
    }

    class RunManager {
        +PlayerStats PlayerStats
        +int CurrentStage
        +int CollectedGold
        +string CurrentSkull
        +List~string~ ClearedRooms
        +StartNewRun()
        +EndRun()
        +AdvanceStage()
        +AddGold(int)
    }

    class MetaProgressionManager {
        +int TotalGold
        +HashSet~string~ UnlockedSkulls
        +Dictionary~string,int~ MetaUpgrades
        +AddGold(int)
        +SpendGold(int)
        +UnlockSkull(string)
        +UpgradeMetaStat(string)
        +Save()
        +Load()
    }

    class SaveManager {
        +SaveMetaData(MetaProgressionManager)
        +LoadMetaData(MetaProgressionManager)
    }

    class ISaveService {
        <<interface>>
        +SaveMetaData(MetaProgressionManager)
        +LoadMetaData(MetaProgressionManager)
    }

    SingletonManager~T~ <|-- GameManager
    GameManager *-- RunManager : contains
    GameManager *-- MetaProgressionManager : contains
    GameManager *-- SaveManager : contains
    SaveManager ..|> ISaveService : implements
    RunManager ..> PlayerStats : references
```

---

## 3. 런 생명주기 상태 다이어그램

```mermaid
stateDiagram-v2
    [*] --> MainMenu

    MainMenu --> InRun : StartNewRun()

    state InRun {
        [*] --> Stage1
        Stage1 --> Stage2 : AdvanceStage()
        Stage2 --> Stage3 : AdvanceStage()
        Stage3 --> BossFight : AdvanceStage()

        Stage1 --> PlayerDeath : HP = 0
        Stage2 --> PlayerDeath : HP = 0
        Stage3 --> PlayerDeath : HP = 0
        BossFight --> PlayerDeath : HP = 0
        BossFight --> Victory : Boss Defeated
    }

    InRun --> Paused : Pause()
    Paused --> InRun : Resume()

    PlayerDeath --> RunEnd : EndRunDefeat()
    Victory --> RunEnd : EndRunVictory()

    RunEnd --> MetaProgression : Show Meta UI
    MetaProgression --> MainMenu : Return to Menu

    MainMenu --> [*]
```

---

## 4. 이벤트 시스템 시퀀스 다이어그램

```mermaid
sequenceDiagram
    participant IS as InventorySystem
    participant IEC as InventoryEventChannel<br/>(ScriptableObject)
    participant IUI as InventoryUI
    participant HUI as HudUI
    participant Player as Player

    Note over IUI,HUI: Start() 시 이벤트 구독
    IUI->>IEC: OnItemAdded += RefreshUI
    HUI->>IEC: OnItemAdded += ShowNotification

    Note over Player: 적 처치, 아이템 드롭
    Player->>IS: AddItem(swordItem)

    activate IS
    IS->>IS: items.Add(swordItem)
    IS->>IEC: RaiseItemAdded(swordItem)
    deactivate IS

    activate IEC
    IEC-->>IUI: OnItemAdded.Invoke(swordItem)
    IEC-->>HUI: OnItemAdded.Invoke(swordItem)
    deactivate IEC

    activate IUI
    IUI->>IUI: RefreshUI()
    Note over IUI: 아이템 목록 갱신
    deactivate IUI

    activate HUI
    HUI->>HUI: ShowNotification("검 획득!")
    Note over HUI: 화면에 알림 표시
    deactivate HUI
```

---

## 5. 데이터 흐름도 (런 vs 메타)

```mermaid
graph LR
    subgraph "런 시작"
        A[StartNewRun]
        B[RunManager 초기화]
        C[PlayerStats 초기화]
        D[Stage = 1, Gold = 0]

        A --> B --> C --> D
    end

    subgraph "게임 플레이"
        E[아이템 획득]
        F[골드 수집]
        G[스테이지 진행]

        D --> E
        D --> F
        D --> G
    end

    subgraph "런 종료"
        H{승리 or 패배?}
        I[CollectedGold 계산]
        J[MetaProgressionManager<br/>AddGold]
        K[UnlockSkull<br/>if 조건 충족]
        L[SaveManager<br/>Save]

        G --> H
        E --> H
        H --> I
        I --> J
        J --> K
        K --> L
    end

    subgraph "다음 런"
        M[RunManager<br/>데이터 클리어]
        N[PlayerStats = null]
        O[Stage = 0, Gold = 0]

        L --> M
        M --> N
        N --> O
        O --> A
    end

    style A fill:#4ecdc4
    style H fill:#ff6b6b
    style J fill:#45b7d1
    style L fill:#f9ca24
```

---

## 6. 저장 시스템 구조

```mermaid
graph TB
    subgraph "저장 요청"
        MPM[MetaProgressionManager]
        Save["Save() 호출"]

        MPM --> Save
    end

    subgraph "GameManager"
        GM[GameManager]
        SaveRef[Save : ISaveService]

        Save --> SaveRef
    end

    subgraph "인터페이스"
        ISS{ISaveService}
    end

    subgraph "구현체 (현재)"
        LSM[SaveManager<br/>LocalSaveManager]
        PP[PlayerPrefs 저장]

        LSM --> PP
    end

    subgraph "구현체 (미래)"
        SSS[ServerSaveService]
        API[HTTP POST<br/>서버 API]

        SSS -.-> API
    end

    SaveRef --> ISS
    ISS -.현재 구현.-> LSM
    ISS -.미래 확장.-> SSS

    style ISS fill:#6c5ce7
    style LSM fill:#00b894
    style SSS fill:#fdcb6e,stroke-dasharray: 5 5
```

---

## 사용 방법

이 다이어그램들은 Mermaid 문법으로 작성되어 다음 도구에서 렌더링할 수 있습니다:

1. **GitHub**: README.md에 직접 삽입
2. **VS Code**: Mermaid Preview 플러그인 설치
3. **Notion**: /code 블록에서 mermaid 선택
4. **온라인**: https://mermaid.live/

포트폴리오 문서에 이미지로 포함하려면:
1. https://mermaid.live/ 에서 코드 붙여넣기
2. SVG/PNG로 다운로드
3. 문서에 이미지 삽입
