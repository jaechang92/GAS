# GASPT 아키텍처 빠른 참조 가이드

> **목적**: 구현 시 빠르게 참조할 수 있는 체크리스트 및 코드 스니펫
> **대상**: 개발자 (본인 또는 팀원)

---

## 📋 구현 체크리스트

### Phase 1: Core Manager 생성 (필수) ⭐

- [ ] `Assets/_Project/Scripts/Core/` 폴더 생성
- [ ] `GameManager.cs` 작성
- [ ] `RunManager.cs` 작성
- [ ] `MetaProgressionManager.cs` 작성
- [ ] `UIManager.cs` 작성
- [ ] Hierarchy에 `GameManager` 오브젝트 추가
- [ ] 테스트: 게임 시작 시 GameManager.Instance 접근 확인

### Phase 2: 인터페이스 및 DTO (필수)

- [ ] `Assets/_Project/Scripts/Interfaces/` 폴더 생성
- [ ] `ISaveService.cs` 작성
- [ ] `Assets/_Project/Scripts/DTOs/` 폴더 생성
- [ ] `MetaDataDTO.cs` 작성
- [ ] `SaveManager.cs` 작성
- [ ] 테스트: 메타 데이터 저장/로드 확인

### Phase 3: 이벤트 채널 (선택, 점진적)

- [ ] `Assets/_Project/Scripts/Events/` 폴더 생성
- [ ] `RunEventChannel.cs` 작성
- [ ] `InventoryEventChannel.cs` 작성
- [ ] `PlayerEventChannel.cs` 작성
- [ ] `Assets/_Project/Data/EventChannels/` 폴더 생성
- [ ] SO 에셋 생성 (Create > GASPT > Events)
- [ ] 테스트: 이벤트 발행/구독 확인

### Phase 4: 기존 코드 리팩토링 (필수)

- [ ] `InventorySystem.cs` 수정 (FindObject 제거)
- [ ] `InventoryUI.cs` 수정 (GameManager 통한 접근)
- [ ] `PlayerStats.cs` 수정 (필요 시)
- [ ] 모든 `FindAnyObjectByType` 검색 및 제거
- [ ] 테스트: 기존 기능 정상 동작 확인

---

## 🚀 빠른 코드 스니펫

### 1. GameManager 접근 패턴

```csharp
// ❌ Before: FindObject
playerStats = FindAnyObjectByType<PlayerStats>();

// ✅ After: GameManager
playerStats = GameManager.Instance.PlayerStats;
```

### 2. 이벤트 발행/구독 패턴

```csharp
// === 발행자 (InventorySystem) ===
[SerializeField] private InventoryEventChannel inventoryEvents;

public void AddItem(Item item)
{
    items.Add(item);
    inventoryEvents?.RaiseItemAdded(item);
}

// === 구독자 (InventoryUI) ===
[SerializeField] private InventoryEventChannel inventoryEvents;

private void Start()
{
    inventoryEvents.OnItemAdded += OnItemAdded;
}

private void OnDestroy()
{
    if (inventoryEvents != null)
    {
        inventoryEvents.OnItemAdded -= OnItemAdded;
    }
}

private void OnItemAdded(Item item)
{
    RefreshUI();
}
```

### 3. 런 시작/종료 패턴

```csharp
// 런 시작
public void StartGame()
{
    GameManager.Instance.StartNewRun();
    // UI 전환
    UIManager.Instance.Hud.Show();
    UIManager.Instance.ShowGameplay();
}

// 런 종료 (승리)
public void OnBossDefeated()
{
    GameManager.Instance.EndRunVictory();
    // UI 전환
    UIManager.Instance.ShowRunEndScreen(true);
}

// 런 종료 (패배)
public void OnPlayerDeath()
{
    GameManager.Instance.EndRunDefeat();
    // UI 전환
    UIManager.Instance.ShowRunEndScreen(false);
}
```

### 4. 메타 데이터 저장/로드 패턴

```csharp
// 메타 골드 추가
GameManager.Instance.Meta.AddGold(500);
// 자동 저장됨

// 스컬 언락
GameManager.Instance.Meta.UnlockSkull("FireSkull");
// 자동 저장됨

// 언락 여부 확인
if (GameManager.Instance.Meta.IsSkullUnlocked("FireSkull"))
{
    // 스컬 선택 가능
}
```

### 5. UI 제어 패턴

```csharp
// 인벤토리 표시
UIManager.Instance.ShowInventory();

// 인벤토리 숨김
UIManager.Instance.HideInventory();

// 인벤토리 토글
UIManager.Instance.ToggleInventory();

// 일시정지
UIManager.Instance.ShowPause();  // 자동으로 Time.timeScale = 0

// 재개
UIManager.Instance.HidePause();  // 자동으로 Time.timeScale = 1
```

---

## 🎯 디자인 패턴 선택 가이드

### 언제 Singleton을 사용하는가?

✅ **사용해야 할 때:**
- 게임 전체에서 단 하나만 존재해야 하는 시스템
- 전역 접근이 필요한 시스템
- 예: GameManager, UIManager, AudioManager

❌ **사용하지 말아야 할 때:**
- 일반 게임 오브젝트 (적, 아이템 등)
- 여러 인스턴스가 필요한 시스템
- 예: Enemy, Bullet, PickupItem

### 언제 ScriptableObject Event를 사용하는가?

✅ **사용해야 할 때:**
- 시스템 간 느슨한 결합이 필요할 때
- UI 업데이트 (게임 로직 → UI)
- 여러 구독자가 동일 이벤트를 받아야 할 때

❌ **사용하지 말아야 할 때:**
- 직접적인 메서드 호출이 더 명확할 때
- 이벤트가 하나의 수신자만 있을 때
- 성능이 매우 중요한 Update() 내부

### 언제 Interface를 사용하는가?

✅ **사용해야 할 때:**
- 여러 구현체가 예상될 때
- 플랫폼별 구현이 필요할 때
- 테스트를 위한 Mock이 필요할 때
- 예: ISaveService (로컬/서버), IAudioService (Unity/FMOD)

❌ **사용하지 말아야 할 때:**
- 구현체가 명확히 하나뿐일 때
- 간단한 유틸리티 클래스
- Unity 특화 MonoBehaviour 메서드

---

## 📁 파일 구조 템플릿

```
Assets/_Project/Scripts/
├── Core/                          # 핵심 Manager
│   ├── GameManager.cs
│   ├── RunManager.cs
│   ├── MetaProgressionManager.cs
│   ├── UIManager.cs
│   └── SaveManager.cs
│
├── Events/                        # ScriptableObject 이벤트
│   ├── InventoryEventChannel.cs
│   ├── RunEventChannel.cs
│   ├── PlayerEventChannel.cs
│   └── UIEventChannel.cs
│
├── Interfaces/                    # 인터페이스
│   ├── ISaveService.cs
│   ├── IDataPersistence.cs
│   └── IEventChannel.cs
│
├── DTOs/                          # 데이터 전송 객체
│   ├── MetaDataDTO.cs
│   ├── RunDataDTO.cs
│   └── PlayerDataDTO.cs
│
├── Inventory/                     # 기존 시스템
│   └── InventorySystem.cs
│
├── Stats/
│   └── PlayerStats.cs
│
└── UI/
    ├── InventoryUI.cs
    ├── HudUI.cs
    └── PauseUI.cs
```

---

## ⚠️ 주의사항 및 함정

### 1. 이벤트 구독 해제 필수

```csharp
// ❌ 나쁜 예: 메모리 누수
private void Start()
{
    inventoryEvents.OnItemAdded += OnItemAdded;
}
// OnDestroy가 없음!

// ✅ 좋은 예
private void Start()
{
    inventoryEvents.OnItemAdded += OnItemAdded;
}

private void OnDestroy()
{
    if (inventoryEvents != null)
    {
        inventoryEvents.OnItemAdded -= OnItemAdded;
    }
}
```

### 2. Singleton 초기화 순서

```csharp
// ❌ 나쁜 예: Awake에서 다른 Singleton 접근
protected override void OnAwake()
{
    // UIManager가 아직 초기화 안 됐을 수 있음!
    UIManager.Instance.ShowHud();
}

// ✅ 좋은 예: Start에서 접근
private void Start()
{
    // 모든 Singleton Awake가 끝난 후
    UIManager.Instance.ShowHud();
}
```

### 3. ScriptableObject 값 초기화

```csharp
// ⚠️ SO는 게임 실행 중 유지됨
// 이벤트나 값을 초기화해야 함

private void OnDisable()
{
    // 모든 이벤트 구독자 제거
    OnItemAdded = null;
    OnItemRemoved = null;
}
```

### 4. FindObject는 런 시작 시 한 번만

```csharp
// ❌ 나쁜 예: 매번 찾기
void Update()
{
    var player = FindAnyObjectByType<PlayerStats>();
}

// ✅ 좋은 예: RunManager에서 한 번만
public void StartNewRun()
{
    PlayerStats = FindAnyObjectByType<PlayerStats>();  // 여기서만!
}
```

### 5. DontDestroyOnLoad 오브젝트 관리

```csharp
// ✅ SingletonManager가 자동 처리
// 중복 인스턴스 자동 파괴
// OnApplicationQuit에서 정리
```

---

## 🧪 테스트 체크리스트

### GameManager 테스트

```csharp
[ContextMenu("Test: Start Run")]
private void TestStartRun()
{
    GameManager.Instance.StartNewRun();
    Debug.Log($"Current Stage: {GameManager.Instance.CurrentStage}");
    Debug.Log($"Player HP: {GameManager.Instance.PlayerStats?.CurrentHP}");
}

[ContextMenu("Test: End Run Victory")]
private void TestEndRunVictory()
{
    GameManager.Instance.EndRunVictory();
    Debug.Log($"Total Gold: {GameManager.Instance.Meta.TotalGold}");
}
```

### 이벤트 채널 테스트

```csharp
[ContextMenu("Test: Fire Inventory Event")]
private void TestInventoryEvent()
{
    Item testItem = CreateDummyItem("Test Sword", EquipmentSlot.Weapon, 0, 15, 0);
    inventoryEvents.RaiseItemAdded(testItem);
    // Console에서 구독자들의 반응 확인
}
```

### SaveManager 테스트

```csharp
[ContextMenu("Test: Save Meta Data")]
private void TestSave()
{
    GameManager.Instance.Meta.AddGold(1000);
    GameManager.Instance.Meta.UnlockSkull("TestSkull");
    // PlayerPrefs에 저장되었는지 확인
}

[ContextMenu("Test: Load Meta Data")]
private void TestLoad()
{
    GameManager.Instance.Meta.Load();
    Debug.Log($"Loaded Gold: {GameManager.Instance.Meta.TotalGold}");
}
```

---

## 📊 성능 비교

| 방식 | 평균 시간 | 메모리 | 비고 |
|-----|---------|-------|-----|
| `FindAnyObjectByType` | ~0.5ms | 0 | 씬 크기에 비례 |
| `Singleton.Instance` | <0.001ms | 8 bytes | 캐싱된 참조 |
| `GameManager 프록시` | <0.001ms | 8 bytes | Singleton + 1회 간접 참조 |

**결론:** FindObject 대비 500배 이상 빠름

---

## 🎓 학습 리소스

### 추천 읽을거리

1. **디자인 패턴**
   - [Game Programming Patterns](https://gameprogrammingpatterns.com/) - Chapter 5 (Singleton)
   - [Refactoring Guru - Unity Patterns](https://refactoring.guru/design-patterns/unity)

2. **Unity 아키텍처**
   - [Unity Learn: Create Modular Game Architecture](https://learn.unity.com/tutorial/create-modular-game-architecture)
   - [Unity Atoms (SO Events)](https://github.com/unity-atoms/unity-atoms)

3. **로그라이크 설계**
   - [Designing Roguelike Metagame Progression](https://gamedevelopment.tutsplus.com/articles/roguelike-game-progression--cms-23570)

### 참고 오픈소스

- **VContainer**: DI 프레임워크 (미래 확장 시)
- **Zenject/Extenject**: DI 프레임워크 (레거시)
- **Unity Atoms**: ScriptableObject Events

---

## 🔧 디버깅 팁

### GameManager 상태 확인

```csharp
[ContextMenu("Debug: Log Game State")]
private void DebugLogGameState()
{
    Debug.Log("========== Game State ==========");
    Debug.Log($"Current State: {GameManager.Instance.CurrentState}");
    Debug.Log($"Current Stage: {GameManager.Instance.CurrentStage}");
    Debug.Log($"Player HP: {GameManager.Instance.PlayerStats?.CurrentHP}/{GameManager.Instance.PlayerStats?.MaxHP}");
    Debug.Log($"Collected Gold: {GameManager.Instance.Run.CollectedGold}");
    Debug.Log($"Total Meta Gold: {GameManager.Instance.Meta.TotalGold}");
    Debug.Log("================================");
}
```

### 모든 Singleton 확인

```csharp
[ContextMenu("Debug: Log All Singletons")]
private void DebugLogSingletons()
{
    SingletonManager<GameManager>.LogAllSingletons();
}
```

### 이벤트 구독자 확인

```csharp
// EventChannel에 추가
[ContextMenu("Debug: Log Subscribers")]
private void DebugLogSubscribers()
{
    int count = OnItemAdded?.GetInvocationList().Length ?? 0;
    Debug.Log($"OnItemAdded subscribers: {count}");
}
```

---

## 📝 포트폴리오 작성 팁

### 어필 포인트

1. **문제 인식**
   - "FindObject 성능 문제 발견"
   - "의존성 관리 복잡도 증가"

2. **해결 과정**
   - "5가지 패턴 비교 분석"
   - "프로젝트 특성에 맞는 하이브리드 선택"

3. **설계 결정**
   - "로그라이크 특성 반영 (런/메타 분리)"
   - "서버 확장 대비 인터페이스 설계"

4. **구현 결과**
   - "FindObject 완전 제거, 500배 성능 향상"
   - "확장성 확보 (서버 추가 시 1줄 수정)"

### 문서 구성 제안

```markdown
# GASPT 아키텍처 설계

## 1. 문제 정의
- 기존 코드의 3가지 문제점

## 2. 해결 방안 탐색
- 5가지 패턴 비교표

## 3. 최종 설계
- 아키텍처 다이어그램
- 핵심 컴포넌트 설명

## 4. 구현 결과
- Before/After 코드
- 성능 개선 수치

## 5. 학습 내용
- 적용한 디자인 패턴
- Unity 특화 설계

## 6. 향후 계획
- DI 프레임워크 도입 검토
```

---

**작성**: GASPT 프로젝트
**최종 수정**: 2025-01-19
**버전**: 1.0

이 가이드는 실제 구현 시 빠르게 참조하기 위한 체크리스트입니다.
