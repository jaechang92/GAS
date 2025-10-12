# 씬 관리 시스템 (Scene Management System)

## 📋 목차
1. [개요](#개요)
2. [아키텍처](#아키텍처)
3. [씬 구조](#씬-구조)
4. [핵심 컴포넌트](#핵심-컴포넌트)
5. [데이터 흐름](#데이터-흐름)
6. [사용 예시](#사용-예시)
7. [확장 가능성](#확장-가능성)
8. [베스트 프랙티스](#베스트-프랙티스)

---

## 개요

### 목적
대규모 팀 프로젝트에서 사용되는 실무 수준의 씬 관리 시스템을 구현합니다.

### 핵심 목표
- ✅ **메모리 효율성**: 필요한 씬만 로드/언로드
- ✅ **확장성**: 새로운 씬/스테이지 추가 용이
- ✅ **팀 협업**: 씬별 독립 작업 가능
- ✅ **디버깅**: 명확한 씬 라이프사이클
- ✅ **사용자 경험**: 부드러운 씬 전환 및 로딩

### 설계 원칙
1. **Single Responsibility**: 각 씬은 명확한 책임을 가짐
2. **Persistent Managers**: 핵심 매니저는 DontDestroyOnLoad
3. **Additive Loading**: 부드러운 전환을 위한 Additive 로딩 활용
4. **Scene Context**: 각 씬의 독립적인 컨텍스트 관리

---

## 아키텍처

### 전체 구조

```
┌─────────────────────────────────────────────────────────────┐
│                      게임 실행 시작                          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    Bootstrap 씬 (Entry Point)                │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│  역할: 게임 진입점 및 핵심 매니저 초기화                    │
│                                                              │
│  [생성]                                                      │
│  • GameManager (DontDestroyOnLoad)                          │
│  • SceneLoader (DontDestroyOnLoad)                          │
│  • SceneTransitionManager (DontDestroyOnLoad)               │
│  • GameFlowManager (DontDestroyOnLoad)                      │
│  • InputManager (DontDestroyOnLoad)                         │
│                                                              │
│  [수행]                                                      │
│  • 시스템 초기화                                             │
│  • 자동으로 Preload 씬 로드                                  │
│                                                              │
│  ⏱️ 소요 시간: ~0.1초 (사용자에게 보이지 않음)             │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                      Preload 씬                              │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│  역할: 필수 리소스 로딩 및 초기화                           │
│                                                              │
│  [리소스 로딩]                                               │
│  • Essential 카테고리 (AudioManager, PoolManager 등)        │
│  • MainMenu 카테고리 (메뉴 UI, 폰트, 사운드)                │
│                                                              │
│  [UI]                                                        │
│  • LoadingScreen (진행률 표시)                               │
│  • 게임 로고 표시                                            │
│                                                              │
│  [완료 조건]                                                 │
│  • 리소스 로딩 100% 완료                                     │
│  • 최소 2초 경과 (로고 노출 시간 보장)                       │
│                                                              │
│  [전환]                                                      │
│  • Bootstrap 씬 언로드                                       │
│  • Main 씬 로드 (SingleMode)                                 │
│                                                              │
│  ⏱️ 소요 시간: 2~5초                                        │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                       Main 씬                                │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│  역할: 메인 메뉴 및 로비 시스템                             │
│                                                              │
│  [포함 오브젝트]                                             │
│  • MainMenuContext (씬 컨텍스트)                             │
│  • MainMenuUI                                                │
│  • LobbyUI                                                   │
│  • SettingsUI                                                │
│  • EventSystem                                               │
│                                                              │
│  [사용자 액션]                                               │
│  • 게임 시작 → Loading 씬 (Additive)                         │
│  • 설정 → SettingsUI 표시                                    │
│  • 게임 종료 → Application.Quit()                            │
│                                                              │
│  [특징]                                                      │
│  • 항상 활성화 (메뉴로 돌아올 수 있음)                       │
│  • Gameplay 진입 시 비활성화 (메모리는 유지)                 │
│                                                              │
│  ⏱️ 체류 시간: 사용자에 따라 다름                           │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   Loading 씬 (Overlay)                       │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│  역할: Gameplay 리소스 로딩 (Additive로 로드)               │
│                                                              │
│  [리소스 로딩]                                               │
│  • Gameplay 카테고리                                         │
│    - Player Prefab                                           │
│    - Enemy Prefabs                                           │
│    - Level Objects                                           │
│    - Gameplay UI                                             │
│    - VFX, SFX                                                │
│                                                              │
│  [UI]                                                        │
│  • LoadingScreen (진행률 표시)                               │
│  • 팁 텍스트 (랜덤)                                          │
│                                                              │
│  [로딩 방식]                                                 │
│  1. Loading 씬 Additive 로드 (Main 씬 위에 오버레이)        │
│  2. Gameplay 리소스 로딩                                     │
│  3. Main 씬 언로드                                           │
│  4. Gameplay 씬 로드                                         │
│  5. Loading 씬 언로드                                        │
│                                                              │
│  ⏱️ 소요 시간: 1~3초                                        │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    Gameplay 씬                               │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│  역할: 실제 게임플레이                                       │
│                                                              │
│  [포함 오브젝트]                                             │
│  • GameplayContext (씬 컨텍스트)                             │
│  • Player                                                    │
│  • Enemies                                                   │
│  • Level Objects (Ground, Platforms, etc.)                  │
│  • IngameUI (체력바, 점수 등)                                │
│  • Main Camera (Gameplay용)                                  │
│                                                              │
│  [게임 루프]                                                 │
│  • Update: 게임 로직 실행                                    │
│  • Input: 플레이어 조작                                      │
│  • Combat: 전투 시스템                                       │
│                                                              │
│  [전환]                                                      │
│  • ESC 키 → Pause 씬 (Additive)                              │
│  • 게임 오버 → Result 씬                                     │
│  • 메뉴 복귀 → Main 씬                                       │
│                                                              │
│  ⏱️ 체류 시간: 게임플레이 시간                              │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                     Pause 씬 (Overlay)                       │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│  역할: 일시정지 메뉴 (Additive로 로드)                      │
│                                                              │
│  [포함 오브젝트]                                             │
│  • PauseMenuUI                                               │
│  • Time.timeScale = 0 설정                                   │
│                                                              │
│  [사용자 액션]                                               │
│  • Resume → Pause 씬 언로드                                  │
│  • Settings → 설정 UI 표시                                   │
│  • Main Menu → Gameplay 언로드 + Main 로드                   │
│                                                              │
│  ⏱️ 체류 시간: 사용자에 따라 다름                           │
└─────────────────────────────────────────────────────────────┘
```

---

## 씬 구조

### 씬 목록 및 역할

| 씬 이름 | 빌드 인덱스 | 로드 모드 | 역할 | 크기 |
|---------|-------------|-----------|------|------|
| **Bootstrap** | 0 | Single | 게임 진입점, 매니저 초기화 | ~100KB |
| **Preload** | 1 | Single | 필수 리소스 로딩 | ~200KB |
| **Main** | 2 | Single | 메인 메뉴, 로비 | ~5MB |
| **Loading** | 3 | Additive | 로딩 화면 (오버레이) | ~500KB |
| **Gameplay** | 4 | Single | 실제 게임플레이 | ~20MB |
| **Pause** | 5 | Additive | 일시정지 메뉴 | ~1MB |

### 씬별 라이프사이클

#### 1. Bootstrap 씬
```csharp
// 라이프사이클
Awake() → 매니저 생성 및 DontDestroyOnLoad 설정
Start() → Preload 씬 로드 시작
// → Bootstrap 씬은 메모리에서 제거됨
```

#### 2. Preload 씬
```csharp
// 라이프사이클
OnSceneLoaded() → PreloadState.EnterState()
EnterState() → 리소스 로딩 시작
  ├─ Essential 카테고리 로드
  ├─ MainMenu 카테고리 로드
  ├─ 최소 2초 대기
  └─ Main 씬으로 전환
ExitState() → LoadingScreen 숨김
// → Preload 씬은 메모리에서 제거됨
```

#### 3. Main 씬
```csharp
// 라이프사이클
OnSceneLoaded() → MainState.EnterState()
EnterState() → MainMenu UI 표시
UpdateState() → 사용자 입력 대기
  └─ "게임 시작" 클릭 시 → Loading 씬 (Additive)
ExitState() → MainMenu UI 숨김
// → Main 씬은 Gameplay 진입 시 언로드됨
```

#### 4. Gameplay 씬
```csharp
// 라이프사이클
OnSceneLoaded() → IngameState.EnterState()
EnterState() →
  ├─ Ingame UI 표시
  ├─ Player, Enemy 생성
  └─ Time.timeScale = 1
UpdateState() → 게임 로직 실행
  └─ ESC 키 → Pause 씬 (Additive)
ExitState() → 게임 정리
// → Gameplay 씬은 메뉴 복귀 시 언로드됨
```

---

## 핵심 컴포넌트

### 1. SceneLoader (씬 로드/언로드 관리)

**책임:**
- 씬 로드/언로드 수행
- 로딩 진행률 추적
- 비동기 로딩 관리
- 씬 전환 이벤트 발행

**주요 메서드:**
```csharp
// 단일 씬 로드 (기존 씬 언로드)
Awaitable LoadSceneAsync(SceneType sceneType, LoadSceneMode mode = LoadSceneMode.Single)

// 씬 언로드
Awaitable UnloadSceneAsync(SceneType sceneType)

// 씬 전환 (페이드 효과 포함)
Awaitable TransitionToSceneAsync(SceneType fromScene, SceneType toScene)

// Additive 로드
Awaitable LoadSceneAdditiveAsync(SceneType sceneType)

// 진행률 조회
float GetLoadProgress()

// 씬 로드 상태 확인
bool IsSceneLoaded(SceneType sceneType)
```

**사용 예시:**
```csharp
// Main → Gameplay 전환 (Loading 오버레이 사용)
await sceneLoader.LoadSceneAdditiveAsync(SceneType.Loading);
await sceneLoader.UnloadSceneAsync(SceneType.Main);
await sceneLoader.LoadSceneAsync(SceneType.Gameplay, LoadSceneMode.Single);
await sceneLoader.UnloadSceneAsync(SceneType.Loading);
```

---

### 2. SceneTransitionManager (씬 전환 효과)

**책임:**
- 페이드 인/아웃 효과
- 커스텀 전환 효과
- 전환 애니메이션 관리

**주요 메서드:**
```csharp
// 페이드 아웃 (화면 어두워짐)
Awaitable FadeOutAsync(float duration = 0.5f)

// 페이드 인 (화면 밝아짐)
Awaitable FadeInAsync(float duration = 0.5f)

// 커스텀 전환 효과
Awaitable PlayTransitionAsync(TransitionType type, float duration)
```

**전환 타입:**
```csharp
public enum TransitionType
{
    None,           // 전환 효과 없음
    Fade,           // 페이드 인/아웃
    Slide,          // 슬라이드
    Wipe,           // 와이프
    Custom          // 커스텀 애니메이션
}
```

---

### 3. SceneContext (씬 컨텍스트)

**책임:**
- 씬별 독립적인 데이터 관리
- 씬 초기화/정리
- 씬 내부 오브젝트 참조 관리

**구현 예시:**
```csharp
// MainSceneContext.cs
public class MainSceneContext : SceneContext
{
    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] private LobbyUI lobbyUI;

    public override void Initialize()
    {
        // Main 씬 초기화
        mainMenuUI.Initialize();
        lobbyUI.Initialize();
    }

    public override void Cleanup()
    {
        // Main 씬 정리
        mainMenuUI.Cleanup();
        lobbyUI.Cleanup();
    }
}

// GameplaySceneContext.cs
public class GameplaySceneContext : SceneContext
{
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform[] enemySpawnPoints;

    public Transform PlayerSpawnPoint => playerSpawnPoint;
    public Transform[] EnemySpawnPoints => enemySpawnPoints;

    public override void Initialize()
    {
        // Gameplay 씬 초기화
    }
}
```

---

### 4. PersistentManagers (DontDestroyOnLoad)

**포함 매니저:**
```csharp
// GameManager.cs - 게임 전반적인 관리
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

// 기타 Persistent 매니저들
- SceneLoader
- SceneTransitionManager
- GameFlowManager
- InputManager
- AudioManager
- PoolManager
```

---

## 데이터 흐름

### 씬 전환 시퀀스 다이어그램

```
사용자              GameFlowManager         SceneLoader         SceneTransition
  │                      │                      │                      │
  │  "게임 시작" 버튼    │                      │                      │
  ├─────────────────────>│                      │                      │
  │                      │                      │                      │
  │                      │  StartGame()         │                      │
  │                      ├─────────────────────>│                      │
  │                      │                      │                      │
  │                      │                      │  FadeOut()           │
  │                      │                      ├─────────────────────>│
  │                      │                      │                      │
  │                      │                      │<─────────────────────┤
  │                      │                      │   (화면 어두워짐)    │
  │                      │                      │                      │
  │                      │                      │  LoadSceneAdditive   │
  │                      │                      │  (Loading)           │
  │                      │                      ├──────────┐           │
  │                      │                      │          │           │
  │                      │                      │<─────────┘           │
  │                      │                      │                      │
  │                      │                      │  UnloadScene(Main)   │
  │                      │                      ├──────────┐           │
  │                      │                      │          │           │
  │                      │                      │<─────────┘           │
  │                      │                      │                      │
  │                      │                      │  LoadScene(Gameplay) │
  │                      │                      ├──────────┐           │
  │                      │                      │          │           │
  │                      │  [Progress Events]   │<─────────┘           │
  │<─────────────────────┼──────────────────────┤                      │
  │  (로딩바 업데이트)   │                      │                      │
  │                      │                      │                      │
  │                      │                      │  UnloadScene         │
  │                      │                      │  (Loading)           │
  │                      │                      ├──────────┐           │
  │                      │                      │          │           │
  │                      │                      │<─────────┘           │
  │                      │                      │                      │
  │                      │                      │  FadeIn()            │
  │                      │                      ├─────────────────────>│
  │                      │                      │                      │
  │                      │                      │<─────────────────────┤
  │                      │                      │   (화면 밝아짐)      │
  │                      │                      │                      │
  │                      │  OnSceneLoaded       │                      │
  │                      │<─────────────────────┤                      │
  │                      │                      │                      │
  │  게임플레이 시작     │                      │                      │
  │<─────────────────────┤                      │                      │
  │                      │                      │                      │
```

---

## 사용 예시

### 예제 1: Bootstrap에서 Preload로 전환
```csharp
// BootstrapManager.cs
public class BootstrapManager : MonoBehaviour
{
    private async void Start()
    {
        Debug.Log("[Bootstrap] 게임 시작 - 핵심 매니저 초기화");

        // 매니저 생성 및 DontDestroyOnLoad 설정
        InitializeManagers();

        // 잠시 대기 (초기화 완료 보장)
        await Awaitable.WaitForSecondsAsync(0.1f);

        // Preload 씬 로드
        Debug.Log("[Bootstrap] Preload 씬으로 전환");
        await SceneLoader.Instance.LoadSceneAsync(SceneType.Preload);
    }
}
```

### 예제 2: Preload에서 Main으로 전환
```csharp
// PreloadState.cs (GameState)
protected override async Awaitable EnterState(CancellationToken ct)
{
    Debug.Log("[Preload] 리소스 로딩 시작");

    // 로딩 화면 표시
    ShowLoadingScreen();

    // 리소스 로딩
    await ResourceManager.Instance.LoadCategoriesAsync(
        new[] { ResourceCategory.Essential, ResourceCategory.MainMenu },
        ct
    );

    // 최소 2초 보장
    float elapsed = Time.time - startTime;
    if (elapsed < 2f)
    {
        await Awaitable.WaitForSecondsAsync(2f - elapsed, ct);
    }

    Debug.Log("[Preload] Main 씬으로 전환");

    // 페이드 아웃
    await SceneTransitionManager.Instance.FadeOutAsync();

    // Main 씬 로드
    await SceneLoader.Instance.LoadSceneAsync(SceneType.Main);

    // 페이드 인
    await SceneTransitionManager.Instance.FadeInAsync();
}
```

### 예제 3: Main에서 Gameplay로 전환 (Loading 오버레이)
```csharp
// MainState.cs (GameState)
public void StartGame()
{
    _ = TransitionToGameplay();
}

private async Awaitable TransitionToGameplay()
{
    Debug.Log("[Main] Gameplay로 전환 시작");

    // 1. Loading 씬 Additive 로드 (화면 위에 오버레이)
    await SceneLoader.Instance.LoadSceneAdditiveAsync(SceneType.Loading);

    // 2. Gameplay 리소스 로딩
    await ResourceManager.Instance.LoadCategoryAsync(
        ResourceCategory.Gameplay,
        CancellationToken.None
    );

    // 3. Main 씬 언로드
    await SceneLoader.Instance.UnloadSceneAsync(SceneType.Main);

    // 4. Gameplay 씬 로드
    await SceneLoader.Instance.LoadSceneAsync(SceneType.Gameplay);

    // 5. Loading 씬 언로드
    await SceneLoader.Instance.UnloadSceneAsync(SceneType.Loading);

    Debug.Log("[Main] Gameplay 전환 완료");
}
```

### 예제 4: Gameplay에서 Pause (Additive)
```csharp
// IngameState.cs (GameState)
protected override void UpdateState(float deltaTime)
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        PauseGame();
    }
}

private async void PauseGame()
{
    Debug.Log("[Gameplay] 일시정지");

    // Time.timeScale = 0 (게임 멈춤)
    Time.timeScale = 0f;

    // Pause 씬 Additive 로드
    await SceneLoader.Instance.LoadSceneAdditiveAsync(SceneType.Pause);
}

public async void ResumeGame()
{
    Debug.Log("[Gameplay] 게임 재개");

    // Pause 씬 언로드
    await SceneLoader.Instance.UnloadSceneAsync(SceneType.Pause);

    // Time.timeScale = 1 (게임 재개)
    Time.timeScale = 1f;
}
```

---

## 확장 가능성

### 1. 멀티 스테이지 지원
```csharp
// 스테이지별 씬 추가
public enum SceneType
{
    Bootstrap,
    Preload,
    Main,
    Loading,

    // 스테이지 씬들
    Stage01,
    Stage02,
    Stage03,
    StageBoss,
}

// 스테이지 전환
await SceneLoader.Instance.LoadSceneAsync(SceneType.Stage02);
```

### 2. 씬 풀링 (Scene Pooling)
```csharp
// 자주 사용되는 씬을 미리 로드 후 비활성화
public class ScenePoolManager
{
    private Dictionary<SceneType, Scene> pooledScenes;

    public async Awaitable PreloadSceneAsync(SceneType sceneType)
    {
        // Additive로 로드 후 비활성화
        await SceneLoader.Instance.LoadSceneAdditiveAsync(sceneType);
        var scene = SceneManager.GetSceneByName(sceneType.ToString());

        // 씬의 모든 Root 오브젝트 비활성화
        foreach (var rootObj in scene.GetRootGameObjects())
        {
            rootObj.SetActive(false);
        }

        pooledScenes[sceneType] = scene;
    }

    public void ActivatePooledScene(SceneType sceneType)
    {
        if (pooledScenes.TryGetValue(sceneType, out var scene))
        {
            foreach (var rootObj in scene.GetRootGameObjects())
            {
                rootObj.SetActive(true);
            }
        }
    }
}
```

### 3. 동적 씬 로딩 (Streaming)
```csharp
// 오픈 월드 게임에서 플레이어 위치 기반 씬 로딩
public class StreamingSceneManager
{
    private Vector3 playerPosition;
    private HashSet<SceneType> loadedScenes;

    public void Update()
    {
        // 플레이어 위치 기반으로 주변 씬 로드/언로드
        var nearbyScenes = GetNearbyScenesForPosition(playerPosition);

        foreach (var scene in nearbyScenes)
        {
            if (!loadedScenes.Contains(scene))
            {
                _ = SceneLoader.Instance.LoadSceneAdditiveAsync(scene);
                loadedScenes.Add(scene);
            }
        }

        // 멀어진 씬 언로드
        var scenesToUnload = loadedScenes.Except(nearbyScenes).ToList();
        foreach (var scene in scenesToUnload)
        {
            _ = SceneLoader.Instance.UnloadSceneAsync(scene);
            loadedScenes.Remove(scene);
        }
    }
}
```

---

## 베스트 프랙티스

### 1. 씬 크기 관리
- ✅ **DO**: 씬 크기를 5MB 이하로 유지
- ✅ **DO**: 큰 오브젝트는 Addressables 사용
- ❌ **DON'T**: 한 씬에 모든 오브젝트 포함

### 2. DontDestroyOnLoad 사용
- ✅ **DO**: 핵심 매니저만 DontDestroyOnLoad
- ✅ **DO**: Singleton 패턴으로 중복 방지
- ❌ **DON'T**: 게임 오브젝트를 무분별하게 DontDestroyOnLoad

### 3. 씬 전환 시 정리
- ✅ **DO**: OnDestroy에서 리소스 해제
- ✅ **DO**: 이벤트 구독 해제
- ✅ **DO**: Coroutine 정리
- ❌ **DON'T**: 메모리 누수 발생

### 4. 로딩 화면
- ✅ **DO**: 진행률 표시
- ✅ **DO**: 최소 표시 시간 보장 (로고 노출)
- ✅ **DO**: 팁 텍스트 표시
- ❌ **DON'T**: 빈 화면 표시

### 5. 에러 처리
```csharp
public async Awaitable LoadSceneAsync(SceneType sceneType)
{
    try
    {
        var operation = SceneManager.LoadSceneAsync(sceneType.ToString());

        if (operation == null)
        {
            Debug.LogError($"[SceneLoader] 씬 로드 실패: {sceneType}");
            return;
        }

        while (!operation.isDone)
        {
            await Awaitable.NextFrameAsync();
        }

        Debug.Log($"[SceneLoader] 씬 로드 완료: {sceneType}");
    }
    catch (Exception ex)
    {
        Debug.LogError($"[SceneLoader] 씬 로드 중 예외: {ex.Message}");
        // Fallback 씬 로드 또는 에러 UI 표시
    }
}
```

### 6. 팀 협업
- ✅ **DO**: 씬별 담당자 지정
- ✅ **DO**: SceneContext로 씬 독립성 보장
- ✅ **DO**: Prefab 기반 작업
- ❌ **DON'T**: 여러 명이 동시에 한 씬 수정

---

## 성능 고려사항

### 메모리 사용량 (예상)

| 씬 | 메모리 사용량 | 상주 여부 |
|---|-------------|----------|
| Bootstrap | ~1MB | 일시적 (언로드됨) |
| Preload | ~2MB | 일시적 (언로드됨) |
| Main | ~5MB | Gameplay 시 언로드 |
| Loading | ~500KB | 일시적 (오버레이) |
| Gameplay | ~20MB | 게임 중 상주 |
| Pause | ~1MB | 일시정지 시에만 |
| **Persistent Managers** | ~3MB | **항상 상주** |

**총 메모리 사용량:**
- 메인 메뉴: ~9MB (Persistent + Main)
- 게임플레이: ~23MB (Persistent + Gameplay)
- 일시정지: ~24MB (Persistent + Gameplay + Pause)

### 최적화 팁
1. **Texture Streaming**: 큰 텍스처는 Streaming 사용
2. **Object Pooling**: 반복 생성 오브젝트는 풀링
3. **Lazy Loading**: 필요할 때만 로드
4. **Preloading**: 자주 사용되는 씬은 미리 로드

---

## 디버깅 도구

### Scene Debugger UI
```csharp
// 에디터 및 개발 빌드에서 씬 상태 표시
#if UNITY_EDITOR || DEVELOPMENT_BUILD
public class SceneDebugger : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.Label("=== Scene Debugger ===");
        GUILayout.Label($"Current Scene: {SceneManager.GetActiveScene().name}");
        GUILayout.Label($"Loaded Scenes: {SceneManager.sceneCount}");

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            GUILayout.Label($"  - {scene.name} (Active: {scene.isLoaded})");
        }

        if (GUILayout.Button("Load Main"))
        {
            _ = SceneLoader.Instance.LoadSceneAsync(SceneType.Main);
        }

        if (GUILayout.Button("Load Gameplay"))
        {
            _ = SceneLoader.Instance.LoadSceneAsync(SceneType.Gameplay);
        }
    }
}
#endif
```

---

## 참고 자료

### Unity 공식 문서
- [SceneManager API](https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.html)
- [Additive Scene Loading](https://docs.unity3d.com/Manual/MultiSceneEditing.html)
- [DontDestroyOnLoad](https://docs.unity3d.com/ScriptReference/Object.DontDestroyOnLoad.html)

### 관련 문서
- `GameFlowManager.cs` - 게임 흐름 관리
- `ResourceManifest.md` - 리소스 관리 시스템
- `FSM_Core.md` - 상태 머신 시스템

---

**문서 버전**: 1.0
**작성일**: 2025-10-12
**작성자**: GASPT Development Team
**최종 수정일**: 2025-10-12
