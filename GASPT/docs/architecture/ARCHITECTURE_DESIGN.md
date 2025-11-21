# GASPT 아키텍처 설계 문서

> **작성 목적**: 학습 및 포트폴리오
> **작성일**: 2025-01-19
> **프로젝트**: GASPT (Generic Ability System + FSM Platformer)

---

## 📑 목차

1. [프로젝트 개요](#1-프로젝트-개요)
2. [아키텍처 설계 배경](#2-아키텍처-설계-배경)
3. [패턴 비교 분석](#3-패턴-비교-분석)
4. [최종 아키텍처 선택](#4-최종-아키텍처-선택)
5. [상세 시스템 설계](#5-상세-시스템-설계)
6. [핵심 컴포넌트 구현](#6-핵심-컴포넌트-구현)
7. [확장성 고려사항](#7-확장성-고려사항)
8. [학습 포인트](#8-학습-포인트)

---

## 1. 프로젝트 개요

### 1.1 프로젝트 정보

- **장르**: 2D 플랫포머 로그라이크 (Skul 영감)
- **규모**: 중형 (Phase A~F, 6개월+ 개발 예정)
- **타겟 플랫폼**: PC (Steam)
- **서버**: 미정 (로컬 세이브 우선, 서버 확장 가능성 대비)

### 1.2 기술 스택

- **엔진**: Unity 6.0+
- **언어**: C# (.NET Standard 2.1)
- **아키텍처**: GAS (Gameplay Ability System) + FSM (Finite State Machine)
- **패턴**: Singleton, ScriptableObject Events, Interface-based Design

### 1.3 로그라이크 게임의 특수성

로그라이크 장르는 일반 게임과 다른 데이터 구조를 요구합니다:

```
일반 게임: 단일 진행도 (세이브 파일 하나)
로그라이크: 이중 진행도 구조
    ├─ 일시적 데이터 (Run Data): 한 판 동안만 유지
    │  └─ 현재 HP, 장착 아이템, 현재 스테이지, 수집 골드 등
    └─ 영구 데이터 (Meta Progression): 판 간 유지
       └─ 언락 스컬, 메타 업그레이드, 총 골드, 업적 등
```

이러한 특성을 효과적으로 관리할 수 있는 아키텍처가 필요했습니다.

---

## 2. 아키텍처 설계 배경

### 2.1 문제 상황

**초기 코드의 문제점:**

```csharp
// InventorySystem.cs
protected override void OnAwake()
{
    playerStats = FindAnyObjectByType<PlayerStats>();  // ❌ 문제 1
}

// InventoryUI.cs
private void Start()
{
    inventorySystem = InventorySystem.Instance;
    playerStats = FindAnyObjectByType<PlayerStats>();  // ❌ 문제 2
}
```

**발견된 문제들:**

1. **성능 문제**
   - `FindAnyObjectByType<T>()`는 O(n) 검색 (씬의 모든 오브젝트 순회)
   - 여러 컴포넌트에서 반복 호출 시 성능 저하

2. **의존성 불명확**
   - 코드만 봐서는 어떤 컴포넌트가 필요한지 알 수 없음
   - 생성자나 필드를 봐도 의존성이 숨겨져 있음

3. **테스트 불가능**
   - Mock 객체 주입 불가
   - 단위 테스트 작성 어려움

4. **런타임 에러 위험**
   - PlayerStats가 없으면 NullReferenceException
   - Awake/Start 호출 순서에 따라 null일 수 있음

5. **확장성 부족**
   - 서버 추가 시 코드 전면 수정 필요
   - 새 시스템 추가 시 의존성 관리 복잡

### 2.2 설계 목표

이러한 문제를 해결하기 위한 설계 목표를 수립했습니다:

| 목표 | 설명 | 우선순위 |
|-----|------|---------|
| **성능** | FindObject 제거, 캐싱된 참조 사용 | 높음 |
| **명확성** | 의존성을 명시적으로 표현 | 높음 |
| **확장성** | 서버 추가 시 최소한의 코드 수정 | 중간 |
| **유지보수성** | 코드 구조를 쉽게 이해 가능 | 높음 |
| **Unity 친화성** | Inspector 활용, SO 패턴 사용 | 높음 |
| **학습 곡선** | 팀원이 쉽게 이해 가능 | 중간 |

---

## 3. 패턴 비교 분석

Unity 게임 아키텍처에서 사용 가능한 주요 패턴들을 비교 분석했습니다.

### 3.1 FindAnyObjectByType 패턴 (초기 상태)

```csharp
private PlayerStats playerStats;

void Start()
{
    playerStats = FindAnyObjectByType<PlayerStats>();
}
```

**장점:**
- ✅ 구현이 매우 간단
- ✅ 추가 설정 불필요

**단점:**
- ❌ **성능**: O(n) 검색, 씬의 모든 오브젝트 순회
- ❌ **의존성 숨김**: 어떤 컴포넌트가 필요한지 불명확
- ❌ **테스트 불가**: Mock 주입 불가
- ❌ **런타임 에러**: null 체크 필수
- ❌ **순서 의존성**: Awake/Start 순서에 영향받음

**평가:** ❌ 프로토타입 외에는 부적합

---

### 3.2 Singleton Manager 패턴

```csharp
public class GameManager : SingletonManager<GameManager>
{
    public PlayerStats PlayerStats { get; private set; }
    public int CurrentStage { get; set; }
}

// 사용
var stats = GameManager.Instance.PlayerStats;
```

**장점:**
- ✅ 전역 접근 용이
- ✅ FindObject보다 빠름 (캐싱)
- ✅ 명확한 중앙 집중화
- ✅ 구현 간단

**단점:**
- ⚠️ **God Object 위험**: 모든 것을 GameManager에 넣으면 비대화
- ⚠️ **강한 결합**: 모든 코드가 GameManager 의존
- ⚠️ **테스트 어려움**: Singleton Mock 어려움
- ⚠️ **순환 참조**: A → GM → B → GM → A 가능성

**평가:** ⚠️ 중소형 프로젝트 적합, 설계 주의 필요

---

### 3.3 Service Locator 패턴

```csharp
public class ServiceLocator
{
    private static Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
        services[typeof(T)] = service;
    }

    public static T Get<T>() where T : class
    {
        return services[typeof(T)] as T;
    }
}

// 등록
ServiceLocator.Register<PlayerStats>(playerStats);

// 사용
var stats = ServiceLocator.Get<PlayerStats>();
```

**장점:**
- ✅ Singleton보다 유연함
- ✅ 런타임 서비스 교체 가능
- ✅ 테스트 시 Mock 등록 가능
- ✅ 여러 구현체 관리 가능

**단점:**
- ⚠️ **의존성 여전히 숨김**: 코드만 봐서는 필요한 서비스 모름
- ⚠️ **런타임 에러**: 등록 안 하면 null
- ⚠️ **타입 안정성**: 컴파일 타임 체크 불가
- ⚠️ **전역 상태**: Singleton과 유사한 문제

**평가:** ⚠️ Singleton보다 나음, DI보다는 부족

---

### 3.4 Dependency Injection (DI)

```csharp
// VContainer 예시
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<PlayerStats>(Lifetime.Singleton);
        builder.Register<InventorySystem>(Lifetime.Singleton);
    }
}

// 사용 - 생성자 주입
public class InventoryUI : MonoBehaviour
{
    private readonly InventorySystem inventorySystem;
    private readonly PlayerStats playerStats;

    [Inject]
    public InventoryUI(InventorySystem inventory, PlayerStats stats)
    {
        this.inventorySystem = inventory;
        this.playerStats = stats;
    }
}
```

**장점:**
- ✅ **명시적 의존성**: 생성자만 보면 필요한 것 즉시 파악
- ✅ **테스트 최고**: Mock 주입 매우 간단
- ✅ **느슨한 결합**: 인터페이스 기반 설계
- ✅ **컴파일 타임 체크**: 의존성 누락 시 에러
- ✅ **수명 관리**: Singleton, Transient, Scoped
- ✅ **확장성**: 새 시스템 추가 쉬움

**단점:**
- ❌ **학습 곡선**: 개념 이해 필요
- ❌ **초기 설정**: 프레임워크 설치 및 설정
- ❌ **오버엔지니어링**: 작은 프로젝트엔 과함
- ❌ **Unity 충돌**: MonoBehaviour 생명주기와 맞추기 어려움
- ❌ **디버깅**: 프레임워크 내부 동작 이해 필요

**평가:** ✅ 대형 프로젝트 표준, 중형 이상 추천하지만 학습 필요

---

### 3.5 ScriptableObject Event 패턴

```csharp
[CreateAssetMenu(menuName = "Events/Item Event Channel")]
public class ItemEventChannel : ScriptableObject
{
    public event Action<Item> OnItemAdded;

    public void RaiseItemAdded(Item item)
    {
        OnItemAdded?.Invoke(item);
    }
}

// 사용
[SerializeField] private ItemEventChannel itemEvents;

void Start()
{
    itemEvents.OnItemAdded += OnItemAddedHandler;
}
```

**장점:**
- ✅ **Unity 네이티브**: Inspector에서 연결
- ✅ **씬 독립적**: DontDestroyOnLoad 불필요
- ✅ **느슨한 결합**: 이벤트 기반 통신
- ✅ **디자이너 친화적**: 코드 없이 연결 가능
- ✅ **디버깅**: 런타임에 SO 값 확인 가능
- ✅ **재사용성**: 여러 씬에서 동일 SO 사용

**단점:**
- ⚠️ **레퍼런스 관리**: SO에 저장된 런타임 레퍼런스는 씬 전환 시 null
- ⚠️ **직렬화 제한**: MonoBehaviour 직접 저장 불가
- ⚠️ **초기화 복잡**: SO 값 리셋 타이밍 중요
- ⚠️ **의존성 파악**: Inspector 열어봐야 연결 확인 가능

**평가:** ✅ Unity 중소형 프로젝트 매우 적합, 이벤트 통신용

---

### 3.6 하이브리드 접근 (최종 선택) ⭐

각 패턴의 장점을 조합한 하이브리드 접근:

```csharp
// 1. Core 시스템: Singleton (접근 빈도 높음)
public class GameManager : SingletonManager<GameManager>
{
    public PlayerStats PlayerStats { get; private set; }
    public RunManager Run { get; private set; }
}

// 2. 이벤트 통신: ScriptableObject (느슨한 결합)
[SerializeField] private InventoryEventChannel inventoryEvents;

// 3. 서비스 인터페이스: DI 개념 차용 (확장성)
public interface ISaveService
{
    void Save();
    void Load();
}
```

**장점:**
- ✅ **균형잡힌 복잡도**: 너무 간단하지도, 복잡하지도 않음
- ✅ **Unity 친화적**: Inspector + 코드 조화
- ✅ **확장 가능**: 필요 시 DI로 마이그레이션 쉬움
- ✅ **팀 친화적**: 프로그래머/디자이너 모두 이해 가능
- ✅ **성능**: FindObject 제거, 캐싱 활용
- ✅ **학습 곡선**: 기존 Unity 개발자에게 친숙

**단점:**
- ⚠️ **일관성**: 여러 패턴 혼용으로 규칙 필요
- ⚠️ **문서화**: 어떤 상황에 어떤 패턴 쓸지 가이드 필요

**평가:** ✅ **중형 프로젝트 최적**, GASPT에 선택

---

### 3.7 패턴 선택 기준

| 프로젝트 규모 | 팀 크기 | 추천 패턴 | 이유 |
|------------|---------|----------|-----|
| 프로토타입 (1주) | 1명 | FindObject + Singleton | 빠른 개발 우선 |
| 소형 (1-3개월) | 1-2명 | Singleton + SO Events | 균형잡힌 구조 |
| **중형 (3-12개월)** | **2-5명** | **하이브리드** ⭐ | **확장성 + Unity 친화** |
| 대형 (1년+) | 5명+ | DI (VContainer) | 테스트/유지보수 필수 |

**GASPT 프로젝트:**
- 규모: 중형 (Phase A~F, 6개월+)
- 팀: 1-2명 (현재 단독, 확장 가능성)
- 서버: 미정 (확장성 필요)
- **선택: 하이브리드 패턴** ✅

---

## 4. 최종 아키텍처 선택

### 4.1 아키텍처 개요

```
┌─────────────────────────────────────────────────────────────┐
│                        전역 계층                              │
│  GameManager (Singleton, DontDestroyOnLoad)                 │
│  ├─ RunManager: 일시적 런 데이터 관리                          │
│  ├─ MetaProgressionManager: 영구 진행도 관리                  │
│  └─ SaveManager: 로컬/서버 저장 (인터페이스)                   │
│                                                              │
│  UIManager (Singleton, DontDestroyOnLoad)                   │
│  ├─ InventoryUI                                             │
│  ├─ HudUI                                                   │
│  ├─ PauseUI                                                 │
│  └─ MinimapUI                                               │
└─────────────────────────────────────────────────────────────┘
                            ↕ (참조)
┌─────────────────────────────────────────────────────────────┐
│                        씬 계층                                │
│  DungeonManager (SceneSingleton, 런마다 재생성)               │
│  ├─ RoomGenerator                                           │
│  ├─ EnemySpawner                                            │
│  └─ LootManager                                             │
│                                                              │
│  Player (MonoBehaviour)                                     │
│  ├─ PlayerController                                        │
│  ├─ PlayerStats                                             │
│  └─ AbilitySystem                                           │
└─────────────────────────────────────────────────────────────┘
                            ↕ (이벤트)
┌─────────────────────────────────────────────────────────────┐
│                   ScriptableObject 계층                       │
│  EventChannels (느슨한 결합)                                  │
│  ├─ InventoryEventChannel                                   │
│  ├─ PlayerEventChannel                                      │
│  ├─ RunEventChannel                                         │
│  └─ UIEventChannel                                          │
│                                                              │
│  GameContext (공유 데이터)                                    │
│  └─ 설정, 상수, 공유 상태                                      │
└─────────────────────────────────────────────────────────────┘
```

### 4.2 핵심 설계 원칙

#### 1. **단일 책임 원칙 (SRP)**
각 Manager는 명확한 단일 책임을 가집니다.

```csharp
// ✅ 좋은 예: 각자 명확한 책임
GameManager → 게임 전체 생명주기
RunManager → 런 데이터 관리
MetaProgressionManager → 영구 진행도 관리
UIManager → UI 표시/숨김 관리

// ❌ 나쁜 예: God Object
GameManager → 모든 기능 (게임 상태, UI, 인벤토리, 적 스폰 등)
```

#### 2. **의존성 역전 원칙 (DIP)**
구체 클래스가 아닌 인터페이스에 의존합니다.

```csharp
// ✅ 좋은 예: 인터페이스 의존
public interface ISaveService
{
    void Save();
    void Load();
}

public class SaveManager : MonoBehaviour, ISaveService
{
    // 로컬 구현
}

public class ServerSaveService : MonoBehaviour, ISaveService
{
    // 서버 구현 (나중에 교체 가능)
}

// ❌ 나쁜 예: 구체 클래스 의존
public class GameManager
{
    private LocalSaveManager saveManager;  // 서버 추가 시 코드 전면 수정
}
```

#### 3. **이벤트 기반 통신**
직접 참조 대신 이벤트로 통신하여 결합도를 낮춥니다.

```csharp
// ✅ 좋은 예: 이벤트 통신 (느슨한 결합)
// InventorySystem
inventoryEvents.RaiseItemAdded(item);

// InventoryUI (다른 곳에서)
inventoryEvents.OnItemAdded += RefreshUI;

// ❌ 나쁜 예: 직접 호출 (강한 결합)
// InventorySystem
inventoryUI.RefreshUI();  // InventorySystem이 UI를 직접 알고 있음
```

#### 4. **런/메타 분리**
로그라이크의 핵심: 일시적 데이터와 영구 데이터 분리

```csharp
// ✅ 런 데이터 (RunManager)
- 현재 HP, 장착 아이템, 현재 스테이지
- 런 종료 시 모두 삭제

// ✅ 메타 데이터 (MetaProgressionManager)
- 언락 스컬, 메타 업그레이드, 총 골드
- 런 간 영구 유지, 저장됨
```

---

## 5. 상세 시스템 설계

### 5.1 GameManager (게임 전체 생명주기)

**책임:**
- 게임 전체 상태 관리 (메뉴, 런 진행, 일시정지 등)
- Sub-Manager 생명주기 관리
- 빠른 접근을 위한 프록시 제공

**구조:**
```csharp
public class GameManager : SingletonManager<GameManager>
{
    // Sub-Manager 참조
    public RunManager Run { get; private set; }
    public MetaProgressionManager Meta { get; private set; }
    public SaveManager Save { get; private set; }

    // 빠른 접근용 프록시
    public PlayerStats PlayerStats => Run?.PlayerStats;
    public int CurrentStage => Run?.CurrentStage ?? 0;

    // 게임 상태
    public enum GameState
    {
        MainMenu,
        InRun,
        Paused,
        RunEnd,
        MetaProgression
    }
    public GameState CurrentState { get; private set; }

    // 런 관리
    public void StartNewRun() { ... }
    public void EndRunVictory() { ... }
    public void EndRunDefeat() { ... }

    // 상태 전환
    public void Pause() { ... }
    public void Resume() { ... }
}
```

**설계 근거:**
- ✅ **중앙 접근점**: 다른 시스템이 쉽게 접근
- ✅ **Sub-Manager 패턴**: God Object 방지
- ✅ **상태 명확화**: 현재 게임 상태 즉시 파악
- ✅ **프록시 제공**: `GameManager.Instance.PlayerStats` 간결한 접근

---

### 5.2 RunManager (런 데이터 관리)

**책임:**
- 현재 런의 일시적 데이터 관리
- 런 시작 시 초기화
- 런 종료 시 데이터 정리

**구조:**
```csharp
public class RunManager : MonoBehaviour
{
    // 런 데이터 (일시적)
    public PlayerStats PlayerStats { get; private set; }
    public int CurrentStage { get; private set; }
    public int CollectedGold { get; private set; }
    public string CurrentSkull { get; private set; }
    public List<string> ClearedRooms { get; private set; }

    // 런 생명주기
    public void StartNewRun()
    {
        // PlayerStats 찾기 (이 타이밍에 한 번만!)
        PlayerStats = FindAnyObjectByType<PlayerStats>();

        // 초기화
        CurrentStage = 1;
        CollectedGold = 0;
        CurrentSkull = "BasicSkull";
        ClearedRooms.Clear();

        // 플레이어 스탯 초기화
        PlayerStats.ResetToBaseStats();
    }

    public void EndRun()
    {
        // 런 데이터 클리어
        CurrentStage = 0;
        CollectedGold = 0;
        ClearedRooms.Clear();
        PlayerStats = null;  // 참조 해제
    }

    // 런 진행 메서드
    public void AdvanceStage() { CurrentStage++; }
    public void AddGold(int amount) { CollectedGold += amount; }
    public void MarkRoomCleared(string roomId) { ... }
}
```

**설계 근거:**
- ✅ **명확한 생명주기**: StartNewRun()과 EndRun()으로 관리
- ✅ **FindObject 최소화**: 런 시작 시 딱 한 번만 호출
- ✅ **메모리 관리**: 런 종료 시 참조 해제
- ✅ **로그라이크 특화**: 런 단위 데이터 관리

**런 플로우:**
```
StartNewRun()
    ↓
PlayerStats 찾기 (FindObject 여기서만 1회!)
    ↓
데이터 초기화 (Stage 1, Gold 0)
    ↓
... 게임 진행 ...
    ↓
EndRun()
    ↓
데이터 클리어, 참조 해제
```

---

### 5.3 MetaProgressionManager (영구 진행도 관리)

**책임:**
- 런 간 유지되는 영구 데이터 관리
- 메타 업그레이드, 언락 관리
- 자동 저장

**구조:**
```csharp
public class MetaProgressionManager : MonoBehaviour
{
    // 영구 데이터
    public int TotalGold { get; private set; }
    public HashSet<string> UnlockedSkulls { get; private set; }
    public Dictionary<string, int> MetaUpgrades { get; private set; }
    public Dictionary<string, bool> Achievements { get; private set; }

    // 골드 관리
    public void AddGold(int amount)
    {
        TotalGold += amount;
        Save();  // 자동 저장
    }

    public bool SpendGold(int amount)
    {
        if (TotalGold >= amount)
        {
            TotalGold -= amount;
            Save();
            return true;
        }
        return false;
    }

    // 스컬 언락
    public void UnlockSkull(string skullId)
    {
        if (UnlockedSkulls.Add(skullId))
        {
            Save();
        }
    }

    public bool IsSkullUnlocked(string skullId)
    {
        return UnlockedSkulls.Contains(skullId);
    }

    // 메타 업그레이드
    public void UpgradeMetaStat(string statId)
    {
        if (!MetaUpgrades.ContainsKey(statId))
            MetaUpgrades[statId] = 0;

        MetaUpgrades[statId]++;
        Save();
    }

    public int GetMetaUpgradeLevel(string statId)
    {
        return MetaUpgrades.GetValueOrDefault(statId, 0);
    }

    // 저장/로드
    public void Save()
    {
        GameManager.Instance.Save.SaveMetaData(this);
    }

    public void Load()
    {
        GameManager.Instance.Save.LoadMetaData(this);
    }
}
```

**설계 근거:**
- ✅ **영구성**: 게임 종료 후에도 유지
- ✅ **자동 저장**: 데이터 변경 시 즉시 저장
- ✅ **타입 안정성**: Dictionary 대신 명확한 메서드
- ✅ **확장성**: 새 메타 요소 추가 쉬움

**데이터 구조:**
```json
{
  "totalGold": 15420,
  "unlockedSkulls": ["BasicSkull", "FireSkull", "IceSkull"],
  "metaUpgrades": {
    "MaxHP": 5,
    "StartGold": 3,
    "CritChance": 2
  },
  "achievements": {
    "FirstBossKill": true,
    "Speedrun10Min": false
  }
}
```

---

### 5.4 UIManager (UI 중앙 관리)

**책임:**
- 모든 UI 참조 보유
- UI 표시/숨김 중앙 관리
- UI 간 전환 제어

**구조:**
```csharp
public class UIManager : SingletonManager<UIManager>
{
    // UI 참조
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private HudUI hudUI;
    [SerializeField] private PauseUI pauseUI;
    [SerializeField] private MinimapUI minimapUI;
    [SerializeField] private MetaProgressionUI metaUI;

    // 프로퍼티로 외부 접근
    public InventoryUI Inventory => inventoryUI;
    public HudUI Hud => hudUI;

    protected override void OnAwake()
    {
        // UI 자동 찾기 (Inspector 미할당 시)
        if (inventoryUI == null)
            inventoryUI = FindAnyObjectByType<InventoryUI>();
        // ... 나머지 UI

        // 초기 상태
        HideAllUI();
        hudUI?.Show();
    }

    // UI 제어 메서드
    public void ShowInventory() => inventoryUI?.Show();
    public void HideInventory() => inventoryUI?.Hide();
    public void ToggleInventory() => inventoryUI?.Toggle();

    public void ShowPause()
    {
        pauseUI?.Show();
        GameManager.Instance.Pause();
    }

    public void HidePause()
    {
        pauseUI?.Hide();
        GameManager.Instance.Resume();
    }

    public void ShowMetaProgression()
    {
        HideAllUI();
        metaUI?.Show();
    }

    private void HideAllUI()
    {
        inventoryUI?.Hide();
        pauseUI?.Hide();
        minimapUI?.Hide();
        metaUI?.Hide();
    }
}
```

**설계 근거:**
- ✅ **중앙 집중**: 모든 UI 한 곳에서 관리
- ✅ **Inspector 연결**: SerializeField로 명확한 참조
- ✅ **Fallback**: 미할당 시 자동 찾기
- ✅ **일관성**: 모든 UI가 동일한 방식으로 제어

**UI 접근 방식 비교:**
```csharp
// ❌ Before: FindObject 남발
var inventory = FindAnyObjectByType<InventoryUI>();
inventory.Show();

// ✅ After: UIManager 통한 접근
UIManager.Instance.ShowInventory();
```

---

### 5.5 SaveManager (저장 시스템)

**책임:**
- 메타 데이터 저장/로드
- 로컬 저장 (현재)
- 서버 저장 (확장 대비)

**구조:**
```csharp
/// <summary>
/// 저장 서비스 인터페이스
/// 로컬/서버 구현을 쉽게 교체 가능하도록 설계
/// </summary>
public interface ISaveService
{
    void SaveMetaData(MetaProgressionManager meta);
    void LoadMetaData(MetaProgressionManager meta);
}

/// <summary>
/// 로컬 저장 구현 (PlayerPrefs 사용)
/// 서버 추가 시 ServerSaveService로 교체 가능
/// </summary>
public class SaveManager : MonoBehaviour, ISaveService
{
    private const string SAVE_KEY = "GASPT_MetaData_v1";

    public void SaveMetaData(MetaProgressionManager meta)
    {
        // DTO로 변환 (직렬화 가능한 형태)
        MetaDataDTO dto = new MetaDataDTO
        {
            totalGold = meta.TotalGold,
            unlockedSkulls = new List<string>(meta.UnlockedSkulls),
            metaUpgrades = new Dictionary<string, int>(meta.MetaUpgrades),
            achievements = new Dictionary<string, bool>(meta.Achievements)
        };

        // JSON 직렬화
        string json = JsonUtility.ToJson(dto);

        // PlayerPrefs에 저장
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log($"[SaveManager] 메타 데이터 저장 완료: {json.Length} bytes");
    }

    public void LoadMetaData(MetaProgressionManager meta)
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            Debug.Log("[SaveManager] 저장 파일 없음, 새 게임 시작");
            return;
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);
        MetaDataDTO dto = JsonUtility.FromJson<MetaDataDTO>(json);

        // 데이터 복원
        // meta에 dto 데이터 적용
        // (실제 구현 시 reflection 또는 명시적 할당)

        Debug.Log($"[SaveManager] 메타 데이터 로드 완료: {dto.totalGold} 골드");
    }
}

/// <summary>
/// DTO (Data Transfer Object)
/// 직렬화 가능한 순수 데이터 클래스
/// </summary>
[Serializable]
public class MetaDataDTO
{
    public int totalGold;
    public List<string> unlockedSkulls;
    public Dictionary<string, int> metaUpgrades;
    public Dictionary<string, bool> achievements;
}
```

**서버 확장 예시:**
```csharp
/// <summary>
/// 서버 저장 구현 (나중에 추가)
/// ISaveService 인터페이스만 구현하면 됨
/// </summary>
public class ServerSaveService : MonoBehaviour, ISaveService
{
    private const string API_URL = "https://api.gaspt.com/save";

    public async void SaveMetaData(MetaProgressionManager meta)
    {
        MetaDataDTO dto = CreateDTO(meta);
        string json = JsonUtility.ToJson(dto);

        // HTTP POST 요청
        using (UnityWebRequest request = UnityWebRequest.Post(API_URL, json))
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[ServerSaveService] 서버 저장 완료");
            }
            else
            {
                Debug.LogError($"[ServerSaveService] 저장 실패: {request.error}");
            }
        }
    }

    public async void LoadMetaData(MetaProgressionManager meta)
    {
        // HTTP GET 요청
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL))
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                MetaDataDTO dto = JsonUtility.FromJson<MetaDataDTO>(json);
                // meta에 적용

                Debug.Log("[ServerSaveService] 서버 로드 완료");
            }
        }
    }
}
```

**설계 근거:**
- ✅ **인터페이스 기반**: 구현체 교체 쉬움
- ✅ **DTO 패턴**: 직렬화 문제 분리
- ✅ **버전 관리**: SAVE_KEY에 버전 포함
- ✅ **확장성**: 서버 추가 시 코드 최소 수정

**저장 시점:**
```
1. 메타 데이터 변경 시 즉시 자동 저장
   - 골드 획득/소비
   - 스컬 언락
   - 메타 업그레이드

2. 게임 종료 시 최종 저장 (OnApplicationQuit)

3. 런 종료 시 저장 (승리/패배)
```

---

### 5.6 EventChannel (ScriptableObject 이벤트)

**책임:**
- 시스템 간 느슨한 결합 통신
- 이벤트 기반 아키텍처
- Unity Inspector 연결

**구조:**
```csharp
/// <summary>
/// 인벤토리 이벤트 채널
/// 아이템 추가/제거/장착/해제 이벤트 발행
/// </summary>
[CreateAssetMenu(menuName = "GASPT/Events/Inventory Event Channel")]
public class InventoryEventChannel : ScriptableObject
{
    // 이벤트 정의
    public event Action<Item> OnItemAdded;
    public event Action<Item> OnItemRemoved;
    public event Action<Item, EquipmentSlot> OnItemEquipped;
    public event Action<EquipmentSlot> OnItemUnequipped;

    // 이벤트 발행 메서드
    public void RaiseItemAdded(Item item)
    {
        OnItemAdded?.Invoke(item);
        Debug.Log($"[InventoryEventChannel] 아이템 추가 이벤트: {item.itemName}");
    }

    public void RaiseItemRemoved(Item item)
    {
        OnItemRemoved?.Invoke(item);
    }

    public void RaiseItemEquipped(Item item, EquipmentSlot slot)
    {
        OnItemEquipped?.Invoke(item, slot);
    }

    public void RaiseItemUnequipped(EquipmentSlot slot)
    {
        OnItemUnequipped?.Invoke(slot);
    }

    // OnEnable/OnDisable에서 이벤트 클리어 (메모리 누수 방지)
    private void OnEnable()
    {
        // SO는 씬 전환 시에도 유지되므로
        // 이벤트 리스너 클리어 필요
    }

    private void OnDisable()
    {
        OnItemAdded = null;
        OnItemRemoved = null;
        OnItemEquipped = null;
        OnItemUnequipped = null;
    }
}

/// <summary>
/// 런 이벤트 채널
/// 런 시작/종료, 스테이지 진행 이벤트
/// </summary>
[CreateAssetMenu(menuName = "GASPT/Events/Run Event Channel")]
public class RunEventChannel : ScriptableObject
{
    public event Action OnRunStarted;
    public event Action OnRunEnded;
    public event Action<int> OnStageChanged;
    public event Action<int> OnGoldCollected;
    public event Action<string> OnRoomCleared;

    public void RaiseRunStarted() => OnRunStarted?.Invoke();
    public void RaiseRunEnded() => OnRunEnded?.Invoke();
    public void RaiseStageChanged(int stage) => OnStageChanged?.Invoke(stage);
    public void RaiseGoldCollected(int amount) => OnGoldCollected?.Invoke(amount);
    public void RaiseRoomCleared(string roomId) => OnRoomCleared?.Invoke(roomId);
}

/// <summary>
/// 플레이어 이벤트 채널
/// HP 변경, 죽음, 레벨업 등
/// </summary>
[CreateAssetMenu(menuName = "GASPT/Events/Player Event Channel")]
public class PlayerEventChannel : ScriptableObject
{
    public event Action<int, int> OnHPChanged;  // (현재 HP, 최대 HP)
    public event Action OnPlayerDied;
    public event Action<int> OnLevelUp;

    public void RaiseHPChanged(int currentHP, int maxHP)
    {
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    public void RaisePlayerDied()
    {
        OnPlayerDied?.Invoke();
    }

    public void RaiseLevelUp(int newLevel)
    {
        OnLevelUp?.Invoke(newLevel);
    }
}
```

**사용 예시:**
```csharp
// === InventorySystem (발행자) ===
public class InventorySystem : SingletonManager<InventorySystem>
{
    [SerializeField] private InventoryEventChannel inventoryEvents;

    public void AddItem(Item item)
    {
        items.Add(item);

        // 이벤트 발행
        inventoryEvents?.RaiseItemAdded(item);
    }
}

// === InventoryUI (구독자 1) ===
public class InventoryUI : BaseUI
{
    [SerializeField] private InventoryEventChannel inventoryEvents;

    private void Start()
    {
        // 이벤트 구독
        inventoryEvents.OnItemAdded += OnItemAdded;
        inventoryEvents.OnItemRemoved += OnItemRemoved;
    }

    private void OnDestroy()
    {
        // 구독 해제 (메모리 누수 방지)
        inventoryEvents.OnItemAdded -= OnItemAdded;
        inventoryEvents.OnItemRemoved -= OnItemRemoved;
    }

    private void OnItemAdded(Item item)
    {
        RefreshItemList();
    }
}

// === HudUI (구독자 2) ===
public class HudUI : BaseUI
{
    [SerializeField] private InventoryEventChannel inventoryEvents;

    private void Start()
    {
        // 동일 이벤트를 다른 UI도 구독 가능
        inventoryEvents.OnItemAdded += ShowItemNotification;
    }

    private void ShowItemNotification(Item item)
    {
        // "검을 획득했습니다!" 팝업 표시
    }
}
```

**설계 근거:**
- ✅ **느슨한 결합**: InventorySystem은 UI를 몰라도 됨
- ✅ **확장성**: 새 구독자 추가 쉬움
- ✅ **Unity 친화적**: Inspector에서 SO 연결
- ✅ **디버깅**: SO에서 이벤트 발생 확인 가능
- ✅ **재사용성**: 여러 씬에서 동일 SO 사용

**주의사항:**
```csharp
// ⚠️ 이벤트 구독 해제 필수 (메모리 누수 방지)
private void OnDestroy()
{
    if (inventoryEvents != null)
    {
        inventoryEvents.OnItemAdded -= OnItemAdded;
    }
}

// ⚠️ SO는 게임 실행 중 유지되므로 값 초기화 필요
private void OnDisable()
{
    // 이벤트 전체 클리어
    OnItemAdded = null;
}
```

---

## 6. 핵심 컴포넌트 구현

### 6.1 전체 파일 구조

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs ⭐ (새로 생성)
│   │   │   ├── RunManager.cs ⭐ (새로 생성)
│   │   │   ├── MetaProgressionManager.cs ⭐ (새로 생성)
│   │   │   ├── UIManager.cs ⭐ (새로 생성)
│   │   │   └── SaveManager.cs ⭐ (새로 생성)
│   │   │
│   │   ├── Events/ ⭐ (새로 생성)
│   │   │   ├── InventoryEventChannel.cs
│   │   │   ├── RunEventChannel.cs
│   │   │   ├── PlayerEventChannel.cs
│   │   │   └── UIEventChannel.cs
│   │   │
│   │   ├── Interfaces/ ⭐ (새로 생성)
│   │   │   ├── ISaveService.cs
│   │   │   └── IDataPersistence.cs
│   │   │
│   │   ├── DTOs/ ⭐ (새로 생성)
│   │   │   └── MetaDataDTO.cs
│   │   │
│   │   ├── Inventory/
│   │   │   ├── InventorySystem.cs (수정)
│   │   │   └── ...
│   │   │
│   │   ├── Stats/
│   │   │   ├── PlayerStats.cs (수정)
│   │   │   └── ...
│   │   │
│   │   └── UI/
│   │       ├── InventoryUI.cs (수정)
│   │       ├── HudUI.cs
│   │       └── ...
│   │
│   └── Data/
│       └── EventChannels/ ⭐ (SO 에셋 폴더)
│           ├── InventoryEventChannel.asset
│           ├── RunEventChannel.asset
│           └── PlayerEventChannel.asset
│
└── Plugins/
    └── Global/
        └── SingletonManager.cs (기존)
```

### 6.2 구현 우선순위

**Phase 1: Core Manager 생성 (필수) - 2시간**
1. GameManager.cs
2. RunManager.cs
3. MetaProgressionManager.cs
4. UIManager.cs

**Phase 2: 인터페이스 및 DTO (필수) - 30분**
1. ISaveService.cs
2. MetaDataDTO.cs
3. SaveManager.cs

**Phase 3: 이벤트 채널 (선택, 점진적 도입) - 1시간**
1. RunEventChannel.cs
2. InventoryEventChannel.cs
3. PlayerEventChannel.cs

**Phase 4: 기존 코드 리팩토링 (필수) - 1시간**
1. InventorySystem.cs 수정
2. InventoryUI.cs 수정
3. PlayerStats.cs 수정
4. 기타 FindObject 제거

**총 예상 시간: 4-5시간**

### 6.3 단계별 마이그레이션 전략

#### Step 1: GameManager 추가 (기존 코드 영향 없음)
```csharp
// 1. GameManager 생성
// 2. Hierarchy에 GameManager 오브젝트 추가
// 3. 기존 코드는 그대로 동작
```

#### Step 2: RunManager 통합 (점진적)
```csharp
// Before
playerStats = FindAnyObjectByType<PlayerStats>();

// After (하나씩 교체)
playerStats = GameManager.Instance.PlayerStats;
```

#### Step 3: 이벤트 채널 도입 (선택적)
```csharp
// Before (직접 호출)
OnItemAdded?.Invoke(item);

// After (SO 이벤트)
inventoryEvents.RaiseItemAdded(item);
```

**장점:**
- ✅ **점진적 마이그레이션**: 한 번에 모든 코드 바꿀 필요 없음
- ✅ **리스크 최소화**: 기존 코드 동작하면서 새 구조 추가
- ✅ **테스트 가능**: 단계별로 테스트하며 진행

---

## 7. 확장성 고려사항

### 7.1 서버 추가 시나리오

**현재 (로컬 저장):**
```csharp
public class SaveManager : MonoBehaviour, ISaveService
{
    public void SaveMetaData(MetaProgressionManager meta)
    {
        // PlayerPrefs에 저장
    }
}
```

**서버 추가 후:**
```csharp
// GameManager.cs에서 한 줄만 수정
protected override void OnAwake()
{
    // Before
    Save = gameObject.AddComponent<SaveManager>();

    // After (서버 추가 시)
    Save = gameObject.AddComponent<ServerSaveService>();
}
```

**변경 범위:**
- ✅ GameManager.cs: 1줄 수정
- ✅ ServerSaveService.cs: 새 파일 추가
- ✅ 나머지 코드: 수정 불필요 (인터페이스 덕분)

### 7.2 멀티플레이어 추가 시나리오

**현재 (싱글플레이어):**
```csharp
public class RunManager : MonoBehaviour
{
    public PlayerStats PlayerStats { get; private set; }  // 단일 플레이어
}
```

**멀티플레이어 확장:**
```csharp
public class RunManager : MonoBehaviour
{
    // 멀티플레이어 지원
    public Dictionary<int, PlayerStats> Players { get; private set; }

    public PlayerStats GetPlayer(int playerId)
    {
        return Players.GetValueOrDefault(playerId);
    }

    public PlayerStats LocalPlayer => Players[localPlayerId];
}

// 기존 코드 호환성 유지
public PlayerStats PlayerStats => LocalPlayer;
```

**변경 범위:**
- ⚠️ RunManager.cs: 내부 구조 수정
- ✅ 외부 코드: `GameManager.Instance.PlayerStats` 그대로 동작

### 7.3 새 시스템 추가 예시

**퀘스트 시스템 추가:**
```csharp
// 1. Manager 추가
public class QuestManager : MonoBehaviour
{
    public List<Quest> ActiveQuests { get; private set; }

    public void AddQuest(Quest quest) { ... }
    public void CompleteQuest(string questId) { ... }
}

// 2. GameManager에 등록
public class GameManager : SingletonManager<GameManager>
{
    public QuestManager Quest { get; private set; }  // 추가

    protected override void OnAwake()
    {
        Run = gameObject.AddComponent<RunManager>();
        Meta = gameObject.AddComponent<MetaProgressionManager>();
        Save = gameObject.AddComponent<SaveManager>();
        Quest = gameObject.AddComponent<QuestManager>();  // 추가
    }
}

// 3. 이벤트 채널 추가 (선택)
[CreateAssetMenu(menuName = "GASPT/Events/Quest Event Channel")]
public class QuestEventChannel : ScriptableObject
{
    public event Action<Quest> OnQuestAdded;
    public event Action<Quest> OnQuestCompleted;
}

// 4. 사용
GameManager.Instance.Quest.AddQuest(newQuest);
```

**변경 범위:**
- ✅ QuestManager.cs: 새 파일
- ✅ QuestEventChannel.cs: 새 파일
- ✅ GameManager.cs: 3줄 추가
- ✅ 기존 코드: 수정 불필요

---

## 8. 학습 포인트

### 8.1 아키텍처 패턴 학습

#### Singleton 패턴
**배운 점:**
- ✅ 전역 접근이 필요한 시스템에 유용
- ⚠️ 남용 시 God Object 위험
- ✅ DontDestroyOnLoad로 씬 간 유지

**언제 사용:**
- GameManager, UIManager 같은 코어 시스템
- 게임 전체에서 단 하나만 존재해야 하는 객체

**언제 피해야:**
- 일반 게임 오브젝트 (적, 아이템 등)
- 여러 인스턴스가 필요한 시스템

#### ScriptableObject Events
**배운 점:**
- ✅ 느슨한 결합 구현에 최적
- ✅ Unity Inspector 친화적
- ✅ 런타임 디버깅 용이

**언제 사용:**
- UI 업데이트 (게임 로직 → UI)
- 시스템 간 통신 (결합 피하고 싶을 때)
- 크로스 씬 이벤트

**주의사항:**
- 이벤트 구독 해제 필수 (메모리 누수)
- SO는 게임 실행 중 유지됨 (값 초기화 필요)

#### Interface-based Design
**배운 점:**
- ✅ 확장성 최고
- ✅ 테스트 가능
- ✅ 구현 교체 쉬움

**언제 사용:**
- 여러 구현체가 예상될 때 (로컬/서버 저장)
- 플랫폼별 구현 필요 시
- Mock이 필요한 시스템

### 8.2 로그라이크 게임 설계

**핵심 개념:**
```
일시적 데이터 (Run Data)
- 런 시작 시 초기화
- 런 종료 시 삭제
- 저장하지 않음
예: 현재 HP, 장착 아이템, 현재 스테이지

영구 데이터 (Meta Progression)
- 게임 전체에서 유지
- 런 종료 시 업데이트
- 저장됨
예: 총 골드, 언락 스컬, 메타 업그레이드
```

**왜 분리?**
- 로그라이크는 "다시 시작"이 핵심 재미
- 하지만 완전 리셋은 답답함 → 메타 진행도로 성장감 제공
- 런 데이터와 메타 데이터를 명확히 분리해야 관리 쉬움

### 8.3 Unity 특화 설계

**Unity의 특수성:**
1. **MonoBehaviour 생명주기**
   - Awake → OnEnable → Start → Update
   - 순서 의존성 문제

2. **씬 전환**
   - 기본: 씬 전환 시 모든 오브젝트 파괴
   - DontDestroyOnLoad로 유지 가능

3. **Inspector 직렬화**
   - SerializeField로 참조 연결
   - ScriptableObject는 에셋으로 존재

**이를 고려한 설계:**
- Manager는 DontDestroyOnLoad
- UI는 SerializeField로 연결
- 이벤트는 ScriptableObject 활용

### 8.4 성능 최적화

**FindObject 문제:**
```csharp
// ❌ 나쁜 예: 매번 검색
void Update()
{
    var player = FindAnyObjectByType<PlayerStats>();  // O(n) 검색
}

// ✅ 좋은 예: 한 번만 찾고 캐싱
private PlayerStats player;

void Start()
{
    player = FindAnyObjectByType<PlayerStats>();  // 1회만
}

void Update()
{
    // player 사용
}

// ✅ 더 좋은 예: Manager 통해 접근
void Update()
{
    var player = GameManager.Instance.PlayerStats;  // O(1) 접근
}
```

**성능 비교:**
- FindAnyObjectByType: O(n), 씬의 모든 오브젝트 순회
- Singleton.Instance: O(1), 캐싱된 참조
- GameManager 프록시: O(1), 이미 찾아둔 참조

### 8.5 확장성 설계

**SOLID 원칙 적용:**

1. **Single Responsibility (단일 책임)**
   ```csharp
   // ✅ 좋은 예: 각자 명확한 책임
   GameManager → 게임 생명주기
   RunManager → 런 데이터
   UIManager → UI 관리

   // ❌ 나쁜 예: 모든 것을 하나에
   GameManager → 게임 상태 + UI + 인벤토리 + 적 스폰 + ...
   ```

2. **Open/Closed (개방/폐쇄)**
   ```csharp
   // ✅ 좋은 예: 확장에 개방, 수정에 폐쇄
   public interface ISaveService { ... }

   // 새 저장 방식 추가 시 기존 코드 수정 불필요
   public class ServerSaveService : ISaveService { ... }

   // ❌ 나쁜 예: 새 기능 추가 시 기존 코드 수정
   public class SaveManager
   {
       public void Save(bool useServer)
       {
           if (useServer)
           {
               // 서버 저장 (기존 코드 수정됨)
           }
           else
           {
               // 로컬 저장
           }
       }
   }
   ```

3. **Dependency Inversion (의존성 역전)**
   ```csharp
   // ✅ 좋은 예: 인터페이스 의존
   private ISaveService saveService;

   // ❌ 나쁜 예: 구체 클래스 의존
   private LocalSaveManager saveManager;
   ```

### 8.6 포트폴리오 어필 포인트

**이 설계를 통해 보여줄 수 있는 역량:**

1. **아키텍처 설계 능력**
   - 여러 패턴 비교 분석
   - 프로젝트 특성에 맞는 선택
   - 확장 가능한 구조 설계

2. **Unity 이해도**
   - MonoBehaviour 생명주기 활용
   - ScriptableObject 패턴 적용
   - DontDestroyOnLoad 관리

3. **문제 해결 능력**
   - FindObject 성능 문제 인식
   - 의존성 관리 개선
   - 로그라이크 특수성 반영

4. **확장성 고려**
   - 서버 추가 대비 인터페이스 설계
   - 멀티플레이어 확장 가능성
   - 새 시스템 추가 용이

5. **코드 품질**
   - SOLID 원칙 적용
   - 명확한 주석 및 문서화
   - 일관된 코딩 스타일

**포트폴리오 문서 구성 제안:**
```
1. 문제 인식
   - 기존 코드의 문제점 (FindObject, 의존성 숨김)

2. 해결 방안 탐색
   - 5가지 패턴 비교 분석표

3. 최종 선택
   - 하이브리드 패턴 선택 근거

4. 설계 문서
   - 아키텍처 다이어그램
   - 컴포넌트 책임 명세

5. 구현 결과
   - Before/After 코드 비교
   - 성능 개선 수치 (있다면)

6. 학습 내용
   - 적용한 디자인 패턴
   - Unity 특화 설계
   - 확장성 고려사항

7. 향후 개선 방향
   - DI 프레임워크 도입 검토
   - 이벤트 시스템 확장
```

---

## 9. 다음 단계

### 9.1 즉시 시작할 작업

1. **GameManager, RunManager, UIManager 구현** (2시간)
   - 이 문서의 코드 기반으로 작성
   - Hierarchy에 GameManager 오브젝트 추가
   - 테스트

2. **FindObject 제거** (1시간)
   - InventorySystem, InventoryUI 수정
   - GameManager 통한 접근으로 교체

3. **SaveManager 및 인터페이스 구현** (30분)
   - ISaveService.cs
   - MetaDataDTO.cs
   - SaveManager.cs

### 9.2 선택적 작업 (점진적 도입)

4. **이벤트 채널 도입** (1-2시간)
   - RunEventChannel 먼저
   - 나머지는 필요 시 추가

5. **런 시작/종료 플로우 구현** (2시간)
   - StartNewRun() 로직
   - EndRun() 로직
   - 테스트

6. **메타 진행도 시스템 구현** (Phase E 이후)
   - 메타 업그레이드 UI
   - 스컬 언락 시스템
   - 저장/로드 테스트

### 9.3 문서 업데이트

- [x] 아키텍처 설계 문서 작성 (현재 문서)
- [ ] 구현 가이드 작성 (코드 예제)
- [ ] API 레퍼런스 작성
- [ ] 포트폴리오 정리

---

## 10. 참고 자료

### 10.1 Unity 공식 문서
- [ScriptableObject](https://docs.unity3d.com/Manual/class-ScriptableObject.html)
- [Singleton Pattern in Unity](https://unity.com/how-to/create-modular-and-maintainable-code-unity)

### 10.2 디자인 패턴
- [Game Programming Patterns](https://gameprogrammingpatterns.com/)
- [Unity Design Patterns](https://refactoring.guru/design-patterns/unity)

### 10.3 참고한 오픈소스
- [VContainer](https://github.com/hadashiA/VContainer)
- [Zenject](https://github.com/modesttree/Zenject)
- [Unity Atoms](https://github.com/unity-atoms/unity-atoms) (SO Events)

---

## 부록: 용어 정리

| 용어 | 설명 |
|-----|------|
| **Singleton** | 게임 전체에서 단 하나만 존재하는 인스턴스 |
| **DontDestroyOnLoad** | 씬 전환 시에도 파괴되지 않도록 설정 |
| **ScriptableObject (SO)** | 에셋으로 저장되는 데이터 컨테이너 |
| **Event Channel** | SO를 활용한 이벤트 발행/구독 패턴 |
| **DTO (Data Transfer Object)** | 데이터 전송/직렬화를 위한 순수 데이터 클래스 |
| **DI (Dependency Injection)** | 의존성을 외부에서 주입하는 패턴 |
| **Service Locator** | 서비스를 중앙에서 관리하고 제공하는 패턴 |
| **God Object** | 너무 많은 책임을 가진 비대한 클래스 (안티패턴) |
| **Run Data** | 로그라이크에서 한 판 동안만 유지되는 일시적 데이터 |
| **Meta Progression** | 로그라이크에서 판 간 유지되는 영구 진행도 |

---

**작성**: GASPT 프로젝트 개발팀
**최종 수정**: 2025-01-19
**문서 버전**: 1.0

이 문서는 학습 및 포트폴리오 목적으로 작성되었으며, 실제 구현 과정에서 지속적으로 업데이트됩니다.
