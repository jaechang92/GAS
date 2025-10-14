# GASPT 빠른 참조 가이드

> 자주 사용하는 패턴과 코드 스니펫 모음

## 📋 목차
1. [매니저 접근 방법](#매니저-접근-방법)
2. [Panel 사용법](#panel-사용법)
3. [GameState 전환](#gamestate-전환)
4. [씬 로딩](#씬-로딩)
5. [FSM 사용법](#fsm-사용법)
6. [GAS 사용법](#gas-사용법)
7. [이벤트 구독/해제](#이벤트-구독해제)
8. [자주 하는 실수](#자주-하는-실수)

---

## 매니저 접근 방법

### ✅ 올바른 방법 (싱글톤)

```csharp
// GameFlowManager 접근
if (GameFlowManager.Instance != null)
{
    GameFlowManager.Instance.StartGame();
}

// UIManager 접근
await UIManager.Instance.OpenPanel(PanelType.MainMenu);

// SceneLoader 접근
await SceneLoader.Instance.LoadSceneAsync(SceneType.Main);

// SceneTransitionManager 접근
await SceneTransitionManager.Instance.FadeOutAsync(0.5f);
```

### ❌ 잘못된 방법

```csharp
// ❌ FindAnyObjectByType 사용 (매번 검색, 느림)
var manager = FindAnyObjectByType<GameFlowManager>();

// ❌ Reflection 사용 (불필요, 느림)
var managerType = Type.GetType("GameFlowManager");
var instance = managerType.GetProperty("Instance").GetValue(null);
```

---

## Panel 사용법

### Panel 열기

```csharp
// 단순 열기 (Lazy Load)
await UIManager.Instance.OpenPanel(PanelType.MainMenu);

// 사전 로딩 후 열기 (권장: 로딩 시간 단축)
await UIManager.Instance.PreloadPanel(PanelType.Loading);
await UIManager.Instance.OpenPanel(PanelType.Loading);
```

### Panel 닫기

```csharp
// 단순 닫기 (메모리에 유지)
UIManager.Instance.ClosePanel(PanelType.MainMenu);

// 닫고 메모리에서 제거
UIManager.Instance.ClosePanel(PanelType.Loading);
UIManager.Instance.UnloadPanel(PanelType.Loading);
```

### Panel 진행률 업데이트

```csharp
// LoadingPanel 진행률 업데이트
var loadingPanel = await UIManager.Instance.OpenPanel(PanelType.Loading);
if (loadingPanel != null)
{
    loadingPanel.UpdateProgress(0.5f); // 50%
}
```

### 신규 Panel 작성 템플릿

```csharp
using UnityEngine;
using UnityEngine.UI;
using UI.Core;

namespace UI.Panels
{
    /// <summary>
    /// 설명 추가
    /// </summary>
    public class YourPanel : BasePanel
    {
        [Header("UI 참조")]
        [SerializeField] private Button yourButton;
        [SerializeField] private Text yourText;

        protected override void Awake()
        {
            base.Awake();

            // Panel 설정
            panelType = PanelType.YourPanel;
            layer = UILayer.Normal;
            openTransition = TransitionType.Fade;
            closeTransition = TransitionType.Fade;
            transitionDuration = 0.3f;
            closeOnEscape = true; // ESC로 닫기 가능

            // 버튼 이벤트 연결
            if (yourButton != null)
            {
                yourButton.onClick.AddListener(OnYourButtonClicked);
            }

            // Panel 이벤트 구독
            OnOpened += OnPanelOpened;
            OnClosed += OnPanelClosed;
        }

        private void OnPanelOpened(BasePanel panel)
        {
            Debug.Log("[YourPanel] 열림");
            // 초기화 로직
        }

        private void OnPanelClosed(BasePanel panel)
        {
            Debug.Log("[YourPanel] 닫힘");
            // 정리 로직
        }

        private void OnYourButtonClicked()
        {
            Debug.Log("[YourPanel] 버튼 클릭");
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (yourButton != null)
            {
                yourButton.onClick.RemoveListener(OnYourButtonClicked);
            }

            OnOpened -= OnPanelOpened;
            OnClosed -= OnPanelClosed;
        }
    }
}
```

---

## GameState 전환

### 게임 흐름 제어

```csharp
using GameFlow;

// 게임 시작 (Main → Loading → Ingame)
GameFlowManager.Instance.StartGame();

// 일시정지 (Ingame → Pause)
GameFlowManager.Instance.PauseGame();

// 계속하기 (Pause → Ingame)
GameFlowManager.Instance.ResumeGame();

// 메인 메뉴로 (Any → Main)
GameFlowManager.Instance.BackToMainMenu();
```

### 신규 GameState 작성 템플릿

```csharp
using UnityEngine;
using System.Threading;
using Core.Managers;
using Core.Enums;

namespace GameFlow
{
    /// <summary>
    /// 설명 추가
    /// </summary>
    public class YourState : GameStateBase
    {
        public YourState(GameFlowManager manager) : base(manager) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[YourState] 진입");

            // 1. FadeOut
            await SceneTransitionManager.Instance.FadeOutAsync(0.3f);

            // 2. 씬 로드
            await SceneLoader.Instance.LoadSceneAsync(SceneType.YourScene);

            // 3. 씬 초기화 대기
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();

            // 4. UI 표시
            await UIManager.Instance.OpenPanel(UI.Core.PanelType.YourPanel);

            // 5. FadeIn
            await SceneTransitionManager.Instance.FadeInAsync(0.5f);
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[YourState] 종료");

            // UI 닫기
            UIManager.Instance.ClosePanel(UI.Core.PanelType.YourPanel);

            await Awaitable.NextFrameAsync();
        }
    }
}
```

---

## 씬 로딩

### 기본 씬 로딩

```csharp
// 단순 로딩
await SceneLoader.Instance.LoadSceneAsync(SceneType.Main);

// 진행률 추적
var operation = SceneLoader.Instance.LoadSceneAsync(SceneType.Gameplay);
while (!operation.isDone)
{
    float progress = operation.progress;
    Debug.Log($"로딩 진행률: {progress * 100}%");
    await Awaitable.NextFrameAsync();
}
```

### Transition과 함께 사용

```csharp
// FadeOut → 씬 로드 → FadeIn
await SceneTransitionManager.Instance.FadeOutAsync(0.3f);
await SceneLoader.Instance.LoadSceneAsync(SceneType.Main);
await Awaitable.NextFrameAsync(); // 씬 초기화 대기
await SceneTransitionManager.Instance.FadeInAsync(0.5f);
```

---

## FSM 사용법

### StateMachine 초기화

```csharp
using FSM.Core;

public class PlayerController : MonoBehaviour
{
    private StateMachine<PlayerStateType> stateMachine;

    private void Awake()
    {
        // StateMachine 생성
        stateMachine = new StateMachine<PlayerStateType>();

        // 상태 추가
        stateMachine.AddState(PlayerStateType.Idle, new PlayerIdleState(this));
        stateMachine.AddState(PlayerStateType.Move, new PlayerMoveState(this));
        stateMachine.AddState(PlayerStateType.Jump, new PlayerJumpState(this));
        stateMachine.AddState(PlayerStateType.Attack, new PlayerAttackState(this));

        // 전환 조건 추가
        stateMachine.AddTransition(
            PlayerStateType.Idle,
            PlayerStateType.Move,
            () => Mathf.Abs(moveInput.x) > 0.1f
        );

        stateMachine.AddTransition(
            PlayerStateType.Move,
            PlayerStateType.Idle,
            () => Mathf.Abs(moveInput.x) < 0.1f
        );

        // 초기 상태 설정
        stateMachine.TransitionTo(PlayerStateType.Idle);
    }

    private void Update()
    {
        // StateMachine 업데이트
        stateMachine.Update();
    }
}
```

### State 클래스 템플릿

```csharp
using FSM.Core;
using UnityEngine;

namespace Player.States
{
    public class PlayerIdleState : IState<PlayerStateType>
    {
        private PlayerController player;

        public PlayerIdleState(PlayerController player)
        {
            this.player = player;
        }

        public void Enter()
        {
            Debug.Log("[PlayerIdleState] 진입");
            player.SetAnimation("Idle");
        }

        public void Update()
        {
            // 매 프레임 실행
        }

        public void Exit()
        {
            Debug.Log("[PlayerIdleState] 종료");
        }

        public async Awaitable EnterAsync(CancellationToken cancellationToken)
        {
            Enter();
            await Awaitable.NextFrameAsync();
        }

        public async Awaitable ExitAsync(CancellationToken cancellationToken)
        {
            Exit();
            await Awaitable.NextFrameAsync();
        }
    }
}
```

---

## GAS 사용법

### AbilitySystemComponent 초기화

```csharp
using GAS.Core;
using UnityEngine;

public class PlayerAbilitySystem : MonoBehaviour
{
    private AbilitySystemComponent abilitySystem;

    private void Awake()
    {
        abilitySystem = GetComponent<AbilitySystemComponent>();

        // Ability 부여
        abilitySystem.GiveAbility<JumpAbility>();
        abilitySystem.GiveAbility<DashAbility>();
        abilitySystem.GiveAbility<AttackAbility>();
    }

    private void Update()
    {
        // Ability 실행
        if (Input.GetKeyDown(KeyCode.Space))
        {
            abilitySystem.TryActivateAbility<JumpAbility>();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            abilitySystem.TryActivateAbility<DashAbility>();
        }
    }
}
```

### Ability 클래스 템플릿

```csharp
using GAS.Core;
using UnityEngine;

namespace Player.Abilities
{
    public class JumpAbility : GameplayAbility
    {
        [SerializeField] private float jumpForce = 10f;

        public override bool CanActivate()
        {
            // 활성화 가능 조건
            bool isGrounded = Owner.GetComponent<PlayerController>().IsGrounded;
            bool hasTag = Owner.GetComponent<AbilitySystemComponent>().HasTag("Grounded");
            return isGrounded && hasTag;
        }

        public override void Activate()
        {
            Debug.Log("[JumpAbility] 점프!");

            // 점프 실행
            var rb = Owner.GetComponent<Rigidbody2D>();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // 태그 제거
            Owner.GetComponent<AbilitySystemComponent>().RemoveTag("Grounded");
        }

        public override void End()
        {
            Debug.Log("[JumpAbility] 점프 종료");
        }
    }
}
```

---

## 이벤트 구독/해제

### 올바른 패턴

```csharp
public class GameplayHUDPanel : BasePanel
{
    private HealthSystem playerHealthSystem;

    protected override void Awake()
    {
        base.Awake();

        // Panel 이벤트 구독
        OnOpened += OnPanelOpened;
        OnClosed += OnPanelClosed;
    }

    private void OnPanelOpened(BasePanel panel)
    {
        // 체력 시스템 연결
        playerHealthSystem = FindPlayerHealthSystem();
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged += OnHealthChanged;
        }
    }

    private void OnPanelClosed(BasePanel panel)
    {
        // 체력 시스템 연결 해제
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged -= OnHealthChanged;
            playerHealthSystem = null;
        }
    }

    private void OnHealthChanged(float current, float max)
    {
        UpdateHealthBar(current, max);
    }

    private void OnDestroy()
    {
        // Panel 이벤트 구독 해제
        OnOpened -= OnPanelOpened;
        OnClosed -= OnPanelClosed;

        // 혹시 남아있을 이벤트 정리
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged -= OnHealthChanged;
        }
    }
}
```

### 이벤트 구독/해제 체크리스트

- ✅ **구독**: `OnOpened` 또는 `Start`에서
- ✅ **해제**: `OnClosed` 또는 `OnDestroy`에서
- ✅ **null 체크**: 이벤트 발생 전 항상 확인
- ✅ **메모리 누수**: 구독한 이벤트는 반드시 해제

```csharp
// ✅ 올바른 이벤트 발생
OnHealthChanged?.Invoke(currentHealth, maxHealth);

// ❌ 잘못된 이벤트 발생 (null 체크 없음)
OnHealthChanged(currentHealth, maxHealth); // NullReferenceException 가능
```

---

## 자주 하는 실수

### 1. Coroutine 사용 (❌)

```csharp
// ❌ 잘못된 방법 (Coroutine 사용 금지)
private IEnumerator LoadSceneCoroutine()
{
    yield return SceneManager.LoadSceneAsync("Main");
}

// ✅ 올바른 방법 (Awaitable 사용)
private async Awaitable LoadSceneAsync()
{
    await SceneLoader.Instance.LoadSceneAsync(SceneType.Main);
}
```

### 2. FindAnyObjectByType 반복 호출 (❌)

```csharp
// ❌ 잘못된 방법 (매번 검색)
void Update()
{
    var player = FindAnyObjectByType<PlayerController>();
    player.DoSomething();
}

// ✅ 올바른 방법 (캐싱)
private PlayerController player;

void Start()
{
    player = FindAnyObjectByType<PlayerController>();
}

void Update()
{
    if (player != null)
    {
        player.DoSomething();
    }
}
```

### 3. 이벤트 구독 해제 누락 (❌)

```csharp
// ❌ 잘못된 방법 (메모리 누수)
void Start()
{
    healthSystem.OnHealthChanged += OnHealthChanged;
    // OnDestroy에서 해제 안 함!
}

// ✅ 올바른 방법
void Start()
{
    healthSystem.OnHealthChanged += OnHealthChanged;
}

void OnDestroy()
{
    if (healthSystem != null)
    {
        healthSystem.OnHealthChanged -= OnHealthChanged;
    }
}
```

### 4. DontDestroyOnLoad 중복 생성 (❌)

```csharp
// ❌ 잘못된 방법 (중복 체크 없음)
void Awake()
{
    DontDestroyOnLoad(gameObject);
}

// ✅ 올바른 방법 (싱글톤 패턴)
private static GameManager instance;
public static GameManager Instance => instance;

void Awake()
{
    if (instance != null && instance != this)
    {
        Destroy(gameObject);
        return;
    }

    instance = this;
    DontDestroyOnLoad(gameObject);
}
```

### 5. velocity 사용 (❌)

```csharp
// ❌ 잘못된 방법 (Unity 6.0에서 deprecated)
rigidbody2D.velocity = new Vector2(speed, rigidbody2D.velocity.y);

// ✅ 올바른 방법 (linearVelocity 사용)
rigidbody2D.linearVelocity = new Vector2(speed, rigidbody2D.linearVelocity.y);
```

### 6. Panel을 직접 SetActive (❌)

```csharp
// ❌ 잘못된 방법 (UIManager 우회)
mainMenuPanel.gameObject.SetActive(true);

// ✅ 올바른 방법 (UIManager 사용)
await UIManager.Instance.OpenPanel(PanelType.MainMenu);
```

### 7. 씬 이름으로 로드 (❌)

```csharp
// ❌ 잘못된 방법 (문자열 하드코딩)
SceneManager.LoadScene("MainMenu");

// ✅ 올바른 방법 (SceneType enum 사용)
await SceneLoader.Instance.LoadSceneAsync(SceneType.Main);
```

---

## 코딩 컨벤션

### 네이밍

```csharp
// ✅ 올바른 네이밍
public class PlayerController         // PascalCase (클래스)
{
    private float moveSpeed;          // camelCase (필드, 변수)
    public int MaxHealth { get; set; } // PascalCase (프로퍼티)

    public void Move()                // PascalCase (메서드)
    {
        float deltaTime = Time.deltaTime; // camelCase (로컬 변수)
    }

    private const float GRAVITY = 9.8f; // UPPER_CASE (상수)
}

// ❌ 잘못된 네이밍
private float _moveSpeed;  // '_' 접두사 사용 금지
private float m_moveSpeed; // 'm_' 접두사 사용 금지
```

### 주석

```csharp
/// <summary>
/// 플레이어를 이동시킵니다.
/// </summary>
/// <param name="direction">이동 방향 (-1: 왼쪽, 1: 오른쪽)</param>
public void Move(int direction)
{
    // 이동 속도 계산
    float speed = moveSpeed * direction;

    // Rigidbody2D 업데이트
    rigidbody2D.linearVelocity = new Vector2(speed, rigidbody2D.linearVelocity.y);
}
```

### 파일 구조

```csharp
// 1. using 문
using UnityEngine;
using System.Collections.Generic;

// 2. namespace
namespace Player
{
    // 3. 클래스
    public class PlayerController : MonoBehaviour
    {
        // 4. 필드 (직렬화된 필드 먼저)
        [Header("이동 설정")]
        [SerializeField] private float moveSpeed = 5f;

        // 5. 필드 (private)
        private Rigidbody2D rb;
        private bool isGrounded;

        // 6. 프로퍼티
        public bool IsGrounded => isGrounded;

        // 7. Unity 메시지 (Awake, Start, Update...)
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            HandleInput();
        }

        // 8. Public 메서드
        public void Jump()
        {
            // ...
        }

        // 9. Private 메서드
        private void HandleInput()
        {
            // ...
        }

        // 10. 이벤트 핸들러
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // ...
        }
    }
}
```

---

## 디버깅 팁

### 로그 레벨

```csharp
// 일반 정보
Debug.Log("[PlayerController] 플레이어 이동");

// 경고
Debug.LogWarning("[PlayerController] 이동 속도가 너무 빠릅니다!");

// 에러
Debug.LogError("[PlayerController] Rigidbody2D를 찾을 수 없습니다!");
```

### 조건부 컴파일

```csharp
#if UNITY_EDITOR
Debug.Log("[DEBUG] 에디터에서만 실행");
#endif

#if DEVELOPMENT_BUILD || UNITY_EDITOR
Debug.Log("[DEV] 개발 빌드에서만 실행");
#endif
```

### Context Menu (에디터 테스트)

```csharp
#if UNITY_EDITOR
[ContextMenu("체력 10 감소")]
private void DebugReduceHealth()
{
    TakeDamage(10);
}

[ContextMenu("체력 완전 회복")]
private void DebugFullHeal()
{
    currentHealth = maxHealth;
}
#endif
```

---

## 성능 최적화 체크리스트

- ✅ **Update에서 FindAnyObjectByType 사용 금지** (Start에서 캐싱)
- ✅ **string 비교 대신 enum 사용**
- ✅ **GetComponent 캐싱** (Awake/Start에서 한 번만)
- ✅ **불필요한 로그 제거** (릴리즈 빌드)
- ✅ **Object Pooling** (자주 생성/파괴되는 객체)
- ✅ **Event 구독 해제** (OnDestroy)

---

## 유용한 단축키

| 단축키 | 기능 |
|--------|------|
| `Ctrl+K, Ctrl+D` | 코드 자동 정렬 (Visual Studio) |
| `Ctrl+R, Ctrl+R` | 이름 변경 (Refactor) |
| `F12` | 정의로 이동 |
| `Shift+F12` | 참조 찾기 |
| `Ctrl+.` | Quick Actions |

---

**작성일**: 2025-10-15
**버전**: 1.0
