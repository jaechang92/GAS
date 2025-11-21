# GameFlow 시스템 사용 가이드

## 개요

FSM 기반 게임 Flow 관리 시스템입니다.
- **StartRoom** (준비실) ↔ **Dungeon** (던전) 씬 전환
- 던전 내 방 이동, 보상 선택, 게임 오버 등 모든 게임 상태 관리

---

## 📦 시스템 구성

### 1. 핵심 컴포넌트
- **GameFlowStateMachine** - FSM 관리자 (싱글톤)
- **GameManager** - 모든 시스템의 참조 허브

### 2. 상태 (States)
1. `StartRoomState` - 준비실 (상점, 업그레이드, 정비)
2. `LoadingDungeonState` - 던전 씬 로딩
3. `DungeonCombatState` - 전투 진행
4. `DungeonRewardState` - 보상 선택
5. `DungeonTransitionState` - 다음 방 전환
6. `DungeonClearedState` - 던전 클리어 결산
7. `LoadingStartRoomState` - 준비실 복귀
8. `GameOverState` - 게임 오버

---

## 🚀 Unity 설정 가이드

### Step 1: StartRoom 씬 생성

1. **Unity Editor에서 메뉴 열기**
   ```
   Tools > GASPT > Create StartRoom Scene
   ```

2. **씬 생성 창에서**
   - Scene Name: `StartRoom` (기본값)
   - Scene Path: `Assets/_Project/Scenes/` (기본값)
   - **"Create StartRoom Scene"** 버튼 클릭

3. **자동 생성되는 요소들**
   - Main Camera (Orthographic, Size: 5)
   - EventSystem (UI용)
   - Canvas (UI 컨테이너)
   - StartRoom UI (타이틀, 안내 텍스트)
   - Ground (바닥)
   - DungeonEntrance_Portal (던전 입장 포탈)
   - Background (배경)

### Step 2: Build Settings 설정 ⭐ 중요!

1. **File > Build Settings** 열기

2. **씬 추가**
   - `StartRoom.unity` 드래그 앤 드롭
   - `GameplayScene.unity` 드래그 앤 드롭

3. **씬 순서 설정** (매우 중요!)
   ```
   [0] StartRoom          ← 첫 번째 (Index 0)
   [1] GameplayScene      ← 두 번째 (Index 1)
   ```

4. **확인**
   - StartRoom이 Index 0인지 확인
   - 둘 다 체크박스가 켜져 있는지 확인

### Step 3: GameFlowStateMachine 시작 설정

**방법 A: 자동 시작 (권장)**

GameFlowStateMachine을 씬에 미리 배치하면 SingletonPreloader가 자동 초기화합니다.

**방법 B: 스크립트에서 수동 시작**

```csharp
// 게임 시작 시 (예: StartRoom의 GameStarter 스크립트)
void Start()
{
    var gameFlow = GameFlowStateMachine.Instance;
    if (gameFlow != null && !gameFlow.IsRunning)
    {
        gameFlow.StartGame(); // StartRoom 상태로 진입
    }
}
```

### Step 4: 싱글톤 DontDestroyOnLoad 설정

SingletonManager를 상속한 모든 싱글톤은 자동으로 DontDestroyOnLoad 처리됩니다:
- GameManager
- GameFlowStateMachine
- CurrencySystem
- SaveSystem
- 등...

씬 전환 시에도 싱글톤들이 유지되므로 별도 설정 불필요!

---

## 🎮 게임 Flow

```
[StartRoom Scene]
    │
    │ (DungeonEntrance 포탈 입장)
    ↓
[LoadingDungeon]
    │ GameplayScene 로드
    ↓
[GameplayScene]
    │
[DungeonCombat] ─┐ (적 전멸)
    ↓            │
[DungeonReward]  │ (NextRoom 포탈 입장)
    ↓            │
[DungeonTransition] ─┘ (다음 방 or 던전 클리어)
    │
    │ (마지막 방 클리어)
    ↓
[DungeonCleared]
    │ (3초 후 자동 복귀)
    ↓
[LoadingStartRoom]
    │ StartRoom 씬 로드
    ↓
[StartRoom Scene]
```

### 플레이어 사망 시
```
[Any State] → [GameOver] → (3초 후) → [StartRoom]
```

---

## 🛠 Portal 설정

### StartRoom의 DungeonEntrance Portal
```
GameObject: DungeonEntrance_Portal
- Portal (Script)
  - Portal Type: DungeonEntrance  ← 중요!
  - Auto Activate On Room Clear: false
  - Start Active: true
  - Portal Sprite: (SpriteRenderer)
```

### Dungeon 내 NextRoom Portal
```
GameObject: NextRoom_Portal
- Portal (Script)
  - Portal Type: NextRoom  ← 중요!
  - Auto Activate On Room Clear: true
  - Start Active: false
  - Portal Sprite: (SpriteRenderer)
```

---

## 🔍 디버깅

### GameFlowStateMachine Inspector
- **Current State Display**: 현재 상태 실시간 표시
- **Context Menu**:
  - "게임 시작" - StartRoom 상태로 진입
  - "던전 입장" - LoadingDungeon 상태로 전환
  - "적 전멸" - DungeonReward 상태로 전환
  - "다음 방 입장" - DungeonTransition 상태로 전환
  - "현재 상태 출력" - 로그로 상태 확인

### GameManager Inspector
- **Context Menu > "디버그: 게임 상태 출력"**
  - Is Paused
  - Is In Run
  - Current Stage
  - Current Gold
  - Player HP
  - Meta Gold
  - **GameFlow State** ← FSM 현재 상태
  - **GameFlow Running** ← FSM 실행 여부

---

## 💡 코드 사용 예시

### Portal에서 던전 입장 트리거
```csharp
// Portal 스크립트 내부에서 자동 처리됨
GameFlowStateMachine.Instance.TriggerEnterDungeon();
```

### Room 클리어 시 보상 상태로 전환
```csharp
// DungeonCombatState에서 자동 처리됨
GameFlowStateMachine.Instance.TriggerEnemiesCleared();
```

### 플레이어 사망 시 게임 오버
```csharp
// PlayerStats.OnDeath()에서 호출
GameFlowStateMachine.Instance.TriggerPlayerDied();
```

### GameManager를 통한 접근
```csharp
// 어디서든 쉽게 접근 가능
var gameFlow = GameManager.Instance.GameFlow;
if (gameFlow != null && gameFlow.IsRunning)
{
    Debug.Log($"현재 상태: {gameFlow.CurrentStateId}");
}
```

---

## ⚠️ 주의사항

### 1. Build Settings 필수!
- StartRoom과 GameplayScene이 **반드시** Build Settings에 추가되어야 합니다
- 순서: StartRoom (0) → GameplayScene (1)
- 추가하지 않으면 **씬 로딩 실패** 에러 발생

### 2. Portal Type 설정
- StartRoom의 포탈: **DungeonEntrance**
- Dungeon 내 포탈: **NextRoom**
- 잘못 설정 시 FSM이 제대로 작동하지 않음

### 3. SingletonPreloader 자동 초기화
- Play 모드 진입 시 자동으로 모든 싱글톤 초기화
- GameFlowStateMachine도 자동 생성됨
- 별도로 씬에 배치할 필요 없음

### 4. 씬 전환 시 싱글톤 유지
- SingletonManager를 상속한 모든 싱글톤은 DontDestroyOnLoad
- 씬이 바뀌어도 GameManager, GameFlowStateMachine 등 유지됨

---

## 🐛 문제 해결

### 문제: "GameplayScene 로드 실패"
- **원인**: Build Settings에 GameplayScene이 없음
- **해결**: File > Build Settings에서 GameplayScene 추가

### 문제: "StartRoom 씬 로드 실패"
- **원인**: Build Settings에 StartRoom이 없음
- **해결**: File > Build Settings에서 StartRoom 추가

### 문제: Portal 입장해도 던전 시작 안 됨
- **원인**: Portal Type이 잘못 설정됨
- **해결**: StartRoom 포탈의 Portal Type을 **DungeonEntrance**로 변경

### 문제: GameFlowStateMachine이 None 상태
- **원인**: StartGame()이 호출되지 않음
- **해결**: StartRoom에 GameStarter 스크립트 추가하여 GameFlow.StartGame() 호출

---

## 📝 TODO (추후 구현)

- [ ] LoadingUI (로딩 화면)
- [ ] DungeonCompleteUI (던전 클리어 화면)
- [ ] GameOverUI (게임 오버 화면)
- [ ] Reward System (보상 시스템)
- [ ] StartRoom UI (상점, 메타 업그레이드)

---

## 📚 관련 파일

### Core
- `GameFlowStateMachine.cs`
- `GameManager.cs`
- `GameFlow/*.cs` (8개 State 클래스)

### Editor
- `StartRoomSceneCreator.cs` (씬 생성 툴)

### Gameplay
- `Portal.cs` (포탈 시스템)
- `RoomManager.cs` (방 관리)

---

이 가이드를 따라하면 StartRoom ↔ Dungeon 씬 전환이 완벽하게 작동합니다! 🎉
