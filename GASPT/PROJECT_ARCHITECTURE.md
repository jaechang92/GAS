# GASPT 프로젝트 아키텍처 가이드

> 신입 프로그래머를 위한 프로젝트 구조 설명서

## 📋 목차
1. [프로젝트 개요](#프로젝트-개요)
2. [아키텍처 레이어 구조](#아키텍처-레이어-구조)
3. [핵심 시스템](#핵심-시스템)
4. [게임 실행 흐름](#게임-실행-흐름)
5. [어셈블리 의존성](#어셈블리-의존성)
6. [디렉토리 구조](#디렉토리-구조)
7. [개발 가이드](#개발-가이드)
8. [리팩토링 필요 사항](#리팩토링-필요-사항)

---

## 프로젝트 개요

**GASPT (Generic Ability System + FSM)**는 Unity 기반 2D 액션 게임 프로젝트입니다.

### 주요 기술 스택
- **Unity 6.0+** (Awaitable 패턴 사용, Coroutine 미사용)
- **Input System** (New Input System)
- **URP** (Universal Render Pipeline)
- **Assembly Definition** 기반 모듈화

### 핵심 설계 원칙
1. **완성 우선 원칙**: 완벽한 시스템보다 플레이 가능한 게임 우선
2. **단계적 개발**: 작은 단위로 개발하고 지속적 테스트
3. **모듈화**: Assembly Definition을 통한 명확한 의존성 관리
4. **이벤트 드리븐**: 느슨한 결합을 위한 이벤트 기반 아키텍처

---

## 아키텍처 레이어 구조

프로젝트는 **5개 레이어**로 구성되며, 하위 레이어는 상위 레이어를 참조할 수 없습니다.

```
┌─────────────────────────────────────────────────────────┐
│                    5. GAMEPLAY LAYER                    │
│  (Player, Enemy, Combat, Skull, Gameplay Manager)       │
│  - 게임플레이 로직 및 캐릭터 구현                        │
└─────────────────────────────────────────────────────────┘
                            ▲
                            │
┌─────────────────────────────────────────────────────────┐
│                      4. UI LAYER                        │
│            (UI.Panels, UI.Menu, UI.HUD)                 │
│  - Panel 기반 UI 시스템                                  │
└─────────────────────────────────────────────────────────┘
                            ▲
                            │
┌─────────────────────────────────────────────────────────┐
│                    3. CORE LAYER                        │
│  (Core.Managers, Core.Bootstrap, SceneManagement)       │
│  - 게임 흐름 관리, 씬 관리, 핵심 매니저                  │
└─────────────────────────────────────────────────────────┘
                            ▲
                            │
┌─────────────────────────────────────────────────────────┐
│                  2. FOUNDATION LAYER                    │
│     (Core.Enums, Core.Utilities, Core.Data, UI.Core)    │
│  - 공통 유틸리티, 열거형, 데이터 구조                    │
└─────────────────────────────────────────────────────────┘
                            ▲
                            │
┌─────────────────────────────────────────────────────────┐
│                   1. PLUGIN LAYER                       │
│           (FSM.Core, GAS.Core, FSM.GAS.Integration)     │
│  - 재사용 가능한 범용 시스템 (다른 프로젝트에서도 사용)  │
└─────────────────────────────────────────────────────────┘
```

### 레이어별 책임

| 레이어 | 책임 | 특징 |
|--------|------|------|
| **Plugin** | 범용 시스템 (FSM, GAS) | 프로젝트 독립적 |
| **Foundation** | 공통 유틸리티, 상수 | 의존성 최소화 |
| **Core** | 게임 흐름, 씬/UI 관리 | 게임 전체 제어 |
| **UI** | Panel 기반 UI | 이벤트 기반 |
| **Gameplay** | 캐릭터, 전투, 게임 로직 | 구체적 구현 |

---

## 핵심 시스템

### 1. Bootstrap 시스템 (게임 진입점)

**위치**: `Assets/_Project/Scripts/Core/Bootstrap/BootstrapManager.cs`

```
Bootstrap 씬 시작
    ↓
1. EventSystem 생성 (DontDestroyOnLoad)
    ↓
2. SceneLoader 생성
    ↓
3. SceneTransitionManager 생성
    ↓
4. UIManager 생성
    ↓
5. GameFlowManager 생성
    ↓
6. Preload 씬 로드
    ↓
7. FadeIn
```

**핵심 코드**:
```csharp
// D:\JaeChang\UintyDev\GASPT\GASPT\Assets\_Project\Scripts\Core\Bootstrap\BootstrapManager.cs:18
private async void Start()
{
    await InitializeManagers();
    await LoadInitialScene();
}
```

### 2. GameFlow 시스템 (게임 상태 관리)

**위치**: `Assets/_Project/Scripts/Core/Managers/GameFlowManager.cs`

FSM 패턴 기반으로 게임 전체 흐름을 관리합니다.

```
┌──────────┐
│ Preload  │ ← 게임 시작 후 리소스 로딩
└────┬─────┘
     │ LoadComplete
     ▼
┌──────────┐
│   Main   │ ← 메인 메뉴 (MainMenuPanel 표시)
└────┬─────┘
     │ StartGame
     ▼
┌──────────┐
│ Loading  │ ← Gameplay 씬 로딩 (LoadingPanel 표시)
└────┬─────┘
     │ LoadComplete
     ▼
┌──────────┐      ┌────────┐
│  Ingame  │◄────►│ Pause  │ ← ESC 키로 전환
└────┬─────┘      └────────┘
     │ GameOver / BackToMenu
     ▼
   Main
```

**상태별 UI 매핑**:
| GameState | Panel | 씬 |
|-----------|-------|-----|
| Preload | - | Preload |
| Main | MainMenuPanel | Main |
| Loading | LoadingPanel | Gameplay (로딩 중) |
| Ingame | GameplayHUDPanel | Gameplay |
| Pause | PausePanel | Gameplay |

**핵심 코드**:
```csharp
// D:\JaeChang\UintyDev\GASPT\GASPT\Assets\_Project\Scripts\Core\Managers\GameFlowManager.cs
public class GameFlowManager : MonoBehaviour
{
    private StateMachine<GameStateType> stateMachine;

    public void StartGame() => stateMachine.TransitionTo(GameStateType.Loading);
    public void PauseGame() => stateMachine.TransitionTo(GameStateType.Pause);
    public void ResumeGame() => stateMachine.TransitionTo(GameStateType.Ingame);
}
```

### 3. UI 시스템 (Panel 기반)

**위치**: `Assets/_Project/Scripts/UI/`

**아키텍처**:
```
UIManager (싱글톤)
    ├─ panelCache (Dictionary<PanelType, BasePanel>)
    ├─ LoadPanel() ← Prefab에서 동적 로딩
    ├─ OpenPanel()
    ├─ ClosePanel()
    ├─ PreloadPanel() ← 선택적 사전 로딩
    └─ UnloadPanel() ← 메모리 해제
```

**Panel 라이프사이클**:
```
1. LoadPanel()
   ├─ Resources.Load()
   ├─ Instantiate(prefab)
   ├─ SetActive(false) ← 비활성 상태로 생성
   └─ panelCache에 저장

2. OpenPanel()
   ├─ LoadPanel() (캐시에 없으면)
   ├─ SetActive(true)
   └─ OnOpened 이벤트 발생

3. ClosePanel()
   ├─ SetActive(false)
   └─ OnClosed 이벤트 발생

4. UnloadPanel()
   ├─ IsOpen 체크
   ├─ Destroy(panel)
   └─ panelCache에서 제거
```

**핵심 코드**:
```csharp
// D:\JaeChang\UintyDev\GASPT\GASPT\Assets\_Project\Scripts\UI\Core\BasePanel.cs:60
public void Open()
{
    gameObject.SetActive(true);
    IsOpen = true;
    OnOpened?.Invoke(this);
}
```

### 4. Scene 관리 시스템

**위치**: `Assets/_Project/Scripts/Core/Managers/SceneLoader.cs`

**기능**:
- 씬 로딩 (Async)
- 로딩 진행률 추적
- 씬 전환 효과 (Fade In/Out)

**Transition 시퀀스**:
```
1. FadeOut (0.3초)
    ↓
2. Scene.LoadAsync()
    ↓
3. 씬 초기화 대기 (NextFrameAsync × 2)
    ↓
4. Panel Open
    ↓
5. FadeIn (0.5초)
```

**핵심 코드**:
```csharp
// D:\JaeChang\UintyDev\GASPT\GASPT\Assets\_Project\Scripts\Core\Managers\SceneTransitionManager.cs
public async Awaitable FadeOutAsync(float duration)
public async Awaitable FadeInAsync(float duration)
```

### 5. FSM 시스템 (Finite State Machine)

**위치**: `Assets/Plugins/FSM_Core/`

**범용 상태 머신**:
```csharp
public interface IState<TStateType> where TStateType : Enum
{
    void Enter();
    void Update();
    void Exit();
    Awaitable EnterAsync(CancellationToken cancellationToken);
    Awaitable ExitAsync(CancellationToken cancellationToken);
}

public class StateMachine<TStateType> where TStateType : Enum
{
    public void TransitionTo(TStateType stateType);
    public void AddTransition(TStateType from, TStateType to, Func<bool> condition);
}
```

**사용 예시**:
- GameFlowManager: 게임 흐름 상태 관리
- Player: 플레이어 행동 상태 (Idle, Move, Jump, Attack...)
- Enemy: 적 AI 상태 (Patrol, Chase, Attack, Dead...)

### 6. GAS 시스템 (Generic Ability System)

**위치**: `Assets/Plugins/GAS_Core/`

**핵심 개념**:
- **Ability**: 실행 가능한 행동 (점프, 공격, 스킬...)
- **Attribute**: 캐릭터 속성 (HP, MP, Attack, Defense...)
- **Effect**: 속성에 영향을 주는 효과 (버프, 디버프, DoT...)
- **Tag**: 상태 식별 (Stunned, Invincible, Attacking...)

**구조**:
```
AbilitySystemComponent
    ├─ GrantedAbilities (List<GameplayAbility>)
    ├─ AttributeSet
    ├─ ActiveEffects (List<GameplayEffect>)
    └─ ActiveTags (GameplayTagContainer)
```

---

## 게임 실행 흐름

### 전체 플로우
```
[게임 시작]
    ↓
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 1. Bootstrap 씬
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    ↓
1-1. BootstrapManager.Start()
    ├─ CreateEventSystem() → DontDestroyOnLoad
    ├─ CreateManager<SceneLoader>() → DontDestroyOnLoad
    ├─ CreateManager<SceneTransitionManager>() → DontDestroyOnLoad
    ├─ CreateManager<UIManager>() → DontDestroyOnLoad
    └─ CreateManager<GameFlowManager>() → DontDestroyOnLoad
    ↓
1-2. LoadInitialScene()
    ├─ FadeOut
    ├─ SceneLoader.LoadSceneAsync(Preload)
    ├─ GameFlowManager.StartManually(Preload)
    └─ FadeIn
    ↓
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 2. Preload 씬
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    ↓
2-1. PreloadState.EnterState()
    ├─ 리소스 로딩
    ├─ 데이터 초기화
    └─ TransitionTo(Main)
    ↓
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 3. Main 씬 (메인 메뉴)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    ↓
3-1. MainState.EnterState()
    ├─ FadeOut (0.3초)
    ├─ SceneLoader.LoadSceneAsync(Main)
    ├─ 씬 초기화 대기
    ├─ UIManager.OpenPanel(MainMenu)
    └─ FadeIn (0.5초)
    ↓
3-2. MainMenuPanel 표시
    ├─ 시작 버튼 클릭
    │   └─ GameFlowManager.StartGame()
    ├─ 설정 버튼 (미구현)
    └─ 종료 버튼
    ↓
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 4. Loading 씬
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    ↓
4-1. LoadingState.EnterState()
    ├─ UIManager.OpenPanel(Loading)
    ├─ LoadingPanel.ShowRandomTip()
    ├─ SceneLoader.LoadSceneAsync(Gameplay)
    │   └─ LoadingPanel.UpdateProgress(progress)
    └─ TransitionTo(Ingame)
    ↓
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 5. Ingame (게임플레이)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    ↓
5-1. IngameState.EnterState()
    ├─ UIManager.ClosePanel(Loading)
    ├─ UIManager.OpenPanel(GameplayHUD)
    ├─ GameplayHUDPanel.SetupHealthSystem()
    └─ Time.timeScale = 1
    ↓
5-2. 게임플레이 루프
    ├─ 플레이어 입력 처리
    ├─ FSM 업데이트 (Player, Enemy)
    ├─ Combat 시스템
    ├─ GAS 시스템 (Ability, Effect)
    └─ HUD 업데이트
    ↓
5-3. ESC 키 → Pause
    ├─ GameFlowManager.PauseGame()
    ├─ UIManager.OpenPanel(Pause)
    ├─ Time.timeScale = 0
    └─ PausePanel 표시
    ↓
5-4. Pause에서 계속하기
    ├─ GameFlowManager.ResumeGame()
    ├─ UIManager.ClosePanel(Pause)
    └─ Time.timeScale = 1
    ↓
5-5. Pause에서 메인 메뉴
    └─ GameFlowManager.BackToMainMenu()
```

---

## 어셈블리 의존성

### 의존성 그래프

```
                    ┌─────────────┐
                    │ Unity.Input │
                    │   System    │
                    └──────┬──────┘
                           │
┌──────────┐               │
│ FSM.Core │               │
└────┬─────┘               │
     │                     │
     ▼                     ▼
┌─────────────────────────────────────────┐
│          Core.Managers                  │
│  (GameFlowManager, SceneLoader,         │
│   UIManager, SceneTransitionManager)    │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│            UI.Panels                    │
│  (MainMenuPanel, PausePanel,            │
│   GameplayHUDPanel, LoadingPanel)       │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│           Combat.Core                   │
│  (HealthSystem, DamageSystem,           │
│   ComboSystem)                          │
└────────────────┬────────────────────────┘
                 │
    ┌────────────┼────────────┐
    ▼            ▼            ▼
┌────────┐  ┌────────┐  ┌────────┐
│ Player │  │ Enemy  │  │ Skull  │
└────────┘  └────────┘  └────────┘
```

### 주요 어셈블리 상세

#### 1. Plugin Layer

| Assembly | 의존성 | 설명 |
|----------|--------|------|
| **FSM.Core** | 없음 | 범용 유한상태머신 |
| **GAS.Core** | Core.Enums | 범용 어빌리티 시스템 |
| **FSM.GAS.Integration** | FSM.Core, GAS.Core | FSM과 GAS 통합 |

#### 2. Foundation Layer

| Assembly | 의존성 | 설명 |
|----------|--------|------|
| **Core.Enums** | 없음 | 열거형 정의 |
| **Core.Utilities** | 없음 | 유틸리티 함수 |
| **Core.Data** | - | 데이터 구조 |
| **UI.Core** | Core.Utilities | UI 기본 클래스 |

#### 3. Core Layer

| Assembly | 의존성 | 설명 |
|----------|--------|------|
| **Core.Managers** | FSM.Core, Core.Utilities, Core.Enums, Core.Data, UI.Core | 핵심 매니저 |

#### 4. UI Layer

| Assembly | 의존성 | 설명 |
|----------|--------|------|
| **UI.Panels** | UI.Core, Core.Enums, Core.Utilities, Combat.Core, Core.Managers | Panel 구현 |
| **UI.Menu** | - | 메뉴 UI |
| **UI.HUD** | - | HUD UI |

#### 5. Gameplay Layer

| Assembly | 의존성 | 설명 |
|----------|--------|------|
| **Combat.Core** | Core.Enums | 전투 시스템 |
| **Player** | Unity.InputSystem, FSM.Core, GAS.Core, Combat.Core, Combat.Attack, Core.Enums, Core.Managers, Core.Utilities | 플레이어 |
| **Enemy** | - | 적 AI |
| **Skull** | - | Skull 캐릭터 |

---

## 디렉토리 구조

```
GASPT/
├─ Assets/
│  ├─ Plugins/                          ← 재사용 가능한 시스템
│  │  ├─ FSM_Core/                      ← 유한상태머신
│  │  │  ├─ Core/
│  │  │  │  ├─ StateMachine.cs
│  │  │  │  └─ State.cs
│  │  │  ├─ Interfaces/
│  │  │  │  └─ IStateMachine.cs
│  │  │  └─ FSM.Core.asmdef
│  │  ├─ GAS_Core/                      ← 게임플레이 어빌리티 시스템
│  │  │  ├─ Core/
│  │  │  │  ├─ Abilities/
│  │  │  │  │  └─ GameplayAbility.cs
│  │  │  │  ├─ Attributes/
│  │  │  │  │  └─ AttributeSet.cs
│  │  │  │  ├─ Effects/
│  │  │  │  │  └─ GameplayEffect.cs
│  │  │  │  └─ Tags/
│  │  │  │     └─ GameplayTag.cs
│  │  │  └─ GAS.Core.asmdef
│  │  └─ FSM_GAS_Integration/           ← FSM + GAS 통합
│  │
│  ├─ _Project/                         ← 프로젝트 전용 코드
│  │  ├─ Scenes/                        ← 씬 파일
│  │  │  ├─ Bootstrap.unity             ← 진입점 (Build Settings 첫 번째)
│  │  │  ├─ Preload.unity               ← 리소스 로딩
│  │  │  ├─ Main.unity                  ← 메인 메뉴
│  │  │  └─ Gameplay.unity              ← 게임플레이
│  │  │
│  │  ├─ Scripts/
│  │  │  ├─ Core/                       ← 핵심 시스템
│  │  │  │  ├─ Bootstrap/
│  │  │  │  │  └─ BootstrapManager.cs  ← 게임 진입점
│  │  │  │  ├─ Managers/
│  │  │  │  │  ├─ GameFlowManager.cs   ← 게임 흐름 관리
│  │  │  │  │  ├─ GameState.cs         ← GameFlow 상태들
│  │  │  │  │  ├─ SceneLoader.cs       ← 씬 로딩
│  │  │  │  │  ├─ SceneTransitionManager.cs ← 전환 효과
│  │  │  │  │  ├─ UIManager.cs         ← UI 관리
│  │  │  │  │  └─ Core.Managers.asmdef
│  │  │  │  ├─ Enums/
│  │  │  │  │  └─ Core.Enums.asmdef
│  │  │  │  ├─ Utilities/
│  │  │  │  │  └─ Core.Utilities.asmdef
│  │  │  │  └─ Data/
│  │  │  │     └─ Core.Data.asmdef
│  │  │  │
│  │  │  ├─ UI/                         ← UI 시스템
│  │  │  │  ├─ Core/
│  │  │  │  │  ├─ BasePanel.cs         ← Panel 기본 클래스
│  │  │  │  │  └─ UI.Core.asmdef
│  │  │  │  ├─ Panels/
│  │  │  │  │  ├─ MainMenuPanel.cs     ← 메인 메뉴
│  │  │  │  │  ├─ LoadingPanel.cs      ← 로딩 화면
│  │  │  │  │  ├─ GameplayHUDPanel.cs  ← 게임플레이 HUD
│  │  │  │  │  ├─ PausePanel.cs        ← 일시정지 메뉴
│  │  │  │  │  └─ UI.Panels.asmdef
│  │  │  │  ├─ Menu/
│  │  │  │  │  └─ UI.Menu.asmdef
│  │  │  │  └─ HUD/
│  │  │  │     └─ UI.HUD.asmdef
│  │  │  │
│  │  │  ├─ Gameplay/                   ← 게임플레이 로직
│  │  │  │  ├─ Combat/
│  │  │  │  │  ├─ Core/
│  │  │  │  │  │  ├─ HealthSystem.cs
│  │  │  │  │  │  ├─ DamageSystem.cs
│  │  │  │  │  │  └─ Combat.Core.asmdef
│  │  │  │  │  ├─ Attack/
│  │  │  │  │  │  ├─ ComboSystem.cs
│  │  │  │  │  │  └─ Combat.Attack.asmdef
│  │  │  │  │  └─ Hitbox/
│  │  │  │  │     └─ Combat.Hitbox.asmdef
│  │  │  │  ├─ Player/
│  │  │  │  │  ├─ States/               ← FSM 상태들
│  │  │  │  │  │  ├─ PlayerIdleState.cs
│  │  │  │  │  │  ├─ PlayerMoveState.cs
│  │  │  │  │  │  ├─ PlayerJumpState.cs
│  │  │  │  │  │  └─ PlayerAttackState.cs
│  │  │  │  │  └─ Player.asmdef
│  │  │  │  ├─ Enemy/
│  │  │  │  │  ├─ States/
│  │  │  │  │  │  ├─ EnemyPatrolState.cs
│  │  │  │  │  │  ├─ EnemyChaseState.cs
│  │  │  │  │  │  └─ EnemyAttackState.cs
│  │  │  │  │  └─ Enemy.asmdef
│  │  │  │  └─ Skull/
│  │  │  │     └─ Skull.asmdef
│  │  │  │
│  │  │  └─ Tests/                      ← 테스트 코드
│  │  │     ├─ Unit/
│  │  │     ├─ Demo/
│  │  │     │  └─ Combat.Demo.asmdef
│  │  │     └─ Tests.asmdef
│  │  │
│  │  └─ Resources/                     ← 리소스 파일
│  │     ├─ UI/                         ← UI Prefab
│  │     │  ├─ MainMenuPanel.prefab
│  │     │  ├─ LoadingPanel.prefab
│  │     │  ├─ GameplayHUDPanel.prefab
│  │     │  └─ PausePanel.prefab
│  │     └─ Manifests/
│  │        └─ GameplayManifest.asset
│  │
│  └─ TextMesh Pro/                     ← TextMesh Pro 에셋
│
└─ ProjectSettings/
   └─ EditorBuildSettings.asset         ← Bootstrap 씬이 첫 번째
```

---

## 개발 가이드

### 신규 Panel 추가하기

1. **Prefab 생성**
   - `Assets/_Project/Resources/UI/` 에 Prefab 생성
   - Canvas, UI 요소 구성

2. **PanelType 추가**
   ```csharp
   // Core.Enums 어딘가에 정의됨
   public enum PanelType
   {
       MainMenu,
       Loading,
       GameplayHUD,
       Pause,
       YourNewPanel  // ← 여기에 추가
   }
   ```

3. **Panel 클래스 작성**
   ```csharp
   // Assets/_Project/Scripts/UI/Panels/YourNewPanel.cs
   using UnityEngine;
   using UI.Core;

   namespace UI.Panels
   {
       public class YourNewPanel : BasePanel
       {
           protected override void Awake()
           {
               base.Awake();

               panelType = PanelType.YourNewPanel;
               layer = UILayer.Normal;
               openTransition = TransitionType.Fade;
               closeTransition = TransitionType.Fade;
               transitionDuration = 0.3f;

               OnOpened += OnPanelOpened;
               OnClosed += OnPanelClosed;
           }

           private void OnPanelOpened(BasePanel panel)
           {
               Debug.Log("[YourNewPanel] 열림");
           }

           private void OnPanelClosed(BasePanel panel)
           {
               Debug.Log("[YourNewPanel] 닫힘");
           }

           private void OnDestroy()
           {
               OnOpened -= OnPanelOpened;
               OnClosed -= OnPanelClosed;
           }
       }
   }
   ```

4. **UIManager에서 사용**
   ```csharp
   await UIManager.Instance.OpenPanel(PanelType.YourNewPanel);
   ```

### 신규 GameState 추가하기

1. **GameStateType 추가**
   ```csharp
   // Assets/_Project/Scripts/Core/Managers/GameStateEnums.cs
   public enum GameStateType
   {
       Preload,
       Main,
       Loading,
       Ingame,
       Pause,
       YourNewState  // ← 여기에 추가
   }
   ```

2. **State 클래스 작성**
   ```csharp
   // Assets/_Project/Scripts/Core/Managers/GameState.cs에 추가
   public class YourNewState : GameStateBase
   {
       public YourNewState(GameFlowManager manager) : base(manager) { }

       protected override async Awaitable EnterState(CancellationToken cancellationToken)
       {
           Debug.Log("[YourNewState] 진입");

           // 씬 로드
           await SceneLoader.Instance.LoadSceneAsync(SceneType.YourScene);

           // UI 표시
           await UIManager.Instance.OpenPanel(PanelType.YourPanel);
       }

       protected override async Awaitable ExitState(CancellationToken cancellationToken)
       {
           Debug.Log("[YourNewState] 종료");

           // UI 닫기
           UIManager.Instance.ClosePanel(PanelType.YourPanel);
           await Awaitable.NextFrameAsync();
       }
   }
   ```

3. **GameFlowManager에 등록**
   ```csharp
   // GameFlowManager.InitializeStates()에 추가
   stateMachine.AddState(GameStateType.YourNewState, new YourNewState(this));
   ```

4. **전환 로직 추가**
   ```csharp
   // GameFlowManager에 메서드 추가
   public void GoToYourNewState()
   {
       stateMachine.TransitionTo(GameStateType.YourNewState);
   }
   ```

### 신규 플레이어 상태 추가하기 (FSM)

1. **상태 클래스 작성**
   ```csharp
   // Assets/_Project/Scripts/Gameplay/Player/States/PlayerDashState.cs
   using UnityEngine;
   using FSM.Core;

   namespace Player.States
   {
       public class PlayerDashState : IState<PlayerStateType>
       {
           private PlayerController player;

           public PlayerDashState(PlayerController player)
           {
               this.player = player;
           }

           public void Enter()
           {
               Debug.Log("대시 시작");
               player.StartDash();
           }

           public void Update()
           {
               player.UpdateDash();
           }

           public void Exit()
           {
               Debug.Log("대시 종료");
               player.EndDash();
           }
       }
   }
   ```

2. **PlayerController에 등록**
   ```csharp
   stateMachine.AddState(PlayerStateType.Dash, new PlayerDashState(this));
   ```

3. **전환 조건 추가**
   ```csharp
   stateMachine.AddTransition(
       PlayerStateType.Move,
       PlayerStateType.Dash,
       () => Input.GetKeyDown(KeyCode.LeftShift)
   );
   ```

---

## 리팩토링 필요 사항

### 🔴 높은 우선순위

#### 1. GameState.cs 파일 분리
**문제점**: 모든 GameState가 하나의 파일에 있어 유지보수 어려움

**개선안**:
```
Assets/_Project/Scripts/Core/Managers/
├─ GameFlowManager.cs
├─ GameStates/
│  ├─ GameStateBase.cs
│  ├─ PreloadState.cs
│  ├─ MainState.cs
│  ├─ LoadingState.cs
│  ├─ IngameState.cs
│  └─ PauseState.cs
└─ GameStateEnums.cs
```

#### 2. UI.Panels 어셈블리의 Combat.Core 의존성 제거
**문제점**: UI가 게임플레이 로직에 의존하면 레이어 구조 위반

**현재**:
```
UI.Panels → Combat.Core (HealthSystem 때문)
```

**개선안**:
```csharp
// GameplayHUDPanel.cs에서 Reflection 제거하고 이벤트 사용
public class GameplayHUDPanel : BasePanel
{
    // Combat.Core 의존성 제거
    // 대신 이벤트로 체력 변경 수신
    public void OnHealthChanged(float current, float max)
    {
        UpdateHealthBar(current, max);
    }
}
```

#### 3. DontDestroyOnLoad 객체 관리 개선
**문제점**: 여러 곳에서 생성되어 중복 체크 필요

**개선안**: PersistentObjectManager 싱글톤 생성
```csharp
public class PersistentObjectManager : MonoBehaviour
{
    private static PersistentObjectManager instance;

    public static void RegisterPersistent(GameObject obj)
    {
        DontDestroyOnLoad(obj);
        // 중복 체크 로직
    }
}
```

### 🟡 중간 우선순위

#### 4. SceneType과 GameStateType 통합 검토
**문제점**: 씬과 게임 상태가 1:1 매핑되지 않아 혼란

**현재**:
```csharp
public enum SceneType { Bootstrap, Preload, Main, Gameplay, ... }
public enum GameStateType { Preload, Main, Loading, Ingame, Pause, ... }
```

**개선안**: SceneType은 순수하게 씬만, GameStateType은 게임 흐름만
```csharp
// SceneType: 물리적 씬
public enum SceneType { Bootstrap, Preload, MainMenu, Game }

// GameStateType: 논리적 상태
public enum GameStateType { Init, Menu, Loading, Playing, Paused, GameOver }
```

#### 5. UIManager의 Panel Preloading 전략 명확화
**문제점**: 모든 Panel을 Lazy Load하면 첫 Open시 지연 발생

**개선안**:
```csharp
// UIManager.cs
[Header("Preload 설정")]
[SerializeField] private PanelType[] preloadPanels = new[]
{
    PanelType.Loading,  // 자주 사용
    PanelType.Pause     // 빠른 반응 필요
};

private async void Start()
{
    await PreloadPanels(preloadPanels);
}
```

### 🟢 낮은 우선순위

#### 6. Assembly Definition 정리
**문제점**: UI.Menu, UI.HUD 어셈블리가 비어있거나 사용되지 않음

**개선안**: 실제 사용하지 않는 어셈블리 제거 또는 통합

#### 7. Test 어셈블리 구조화
**문제점**: Tests, Combat.Demo, Combat.Tests.Unit이 혼재

**개선안**:
```
Assets/_Project/Scripts/Tests/
├─ Unit/                        ← 단위 테스트
│  └─ Combat/
│     └─ Combat.Tests.Unit.asmdef
├─ Integration/                 ← 통합 테스트
│  └─ GameFlow/
│     └─ GameFlow.Tests.Integration.asmdef
└─ PlayMode/                    ← PlayMode 테스트
   └─ Demo/
      └─ Combat.Demo.asmdef
```

#### 8. 이벤트 시스템 통합 검토
**문제점**: Panel마다 개별 이벤트 정의

**개선안**: 중앙화된 이벤트 시스템 구축
```csharp
// Core.Utilities
public static class GameEvents
{
    public static event Action<float, float> OnPlayerHealthChanged;
    public static event Action<int> OnComboChanged;
    public static event Action<int> OnScoreChanged;
}
```

---

## 추가 문서

- **FULL_GAME_FLOW_GUIDE.md**: 게임 흐름 상세 가이드
- **UI_SYSTEM_DESIGN.md**: UI 시스템 설계 문서
- **TESTING_GUIDE.md**: 테스트 가이드
- **ENCODING_GUIDE.md**: 한글 인코딩 가이드

---

## 문의 및 기여

프로젝트 관련 문의나 개선 제안은 팀 리드에게 연락하세요.

**작성일**: 2025-10-15
**버전**: 1.0
