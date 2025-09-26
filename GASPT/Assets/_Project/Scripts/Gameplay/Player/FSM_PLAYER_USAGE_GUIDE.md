# FSM 기반 플레이어 컨트롤러 사용 가이드 (컴포넌트 분리 아키텍처)

## 개요
GASPT 프로젝트의 FSM(유한상태머신) 기반 플레이어 컨트롤러는 **단일책임원칙**을 준수하는 **컴포넌트 조합 패턴**으로 설계되었습니다. 각 컴포넌트가 명확한 역할을 가져 유지보수성과 확장성을 크게 향상시켰습니다.

## 시스템 아키텍처

### 🔧 핵심 컴포넌트 구조

```
PlayerController (코디네이터)
├── InputHandler (입력 처리)
├── PhysicsController (물리 시스템)
├── EnvironmentChecker (환경 검사)
├── AnimationController (애니메이션 제어)
├── GroundChecker (지면 검사)
└── StateMachine (FSM Core)
```

#### **1. PlayerController** - 🎛️ 메인 코디네이터
- **역할**: FSM 상태 관리 및 컴포넌트 조합
- **책임**: 상태 전환, 이벤트 중계, 컴포넌트 초기화
- **특징**: 단일책임원칙 준수, 다른 컴포넌트들의 조율자

#### **2. InputHandler** - 🎮 입력 처리 전담
- **역할**: 모든 사용자 입력 감지 및 이벤트 발생
- **책임**: 키보드/마우스 입력, 입력 상태 관리, 입력 이벤트 발생
- **이벤트**: OnJumpPressed, OnDashPressed, OnMovementInput 등

#### **3. PhysicsController** - ⚡ 커스텀 물리 시스템
- **역할**: Transform 기반 커스텀 물리 계산
- **책임**: 속도 관리, 중력 적용, 이동 처리, 충돌 예측
- **특징**: 착지 시 묻힘 방지 시스템 포함

#### **4. EnvironmentChecker** - 🌍 환경 검사 전담
- **역할**: 벽 충돌, 대시 쿨다운 관리
- **책임**: 벽 감지, 대시 가능 상태 관리, 방향 업데이트
- **이벤트**: OnTouchWall, OnLeaveWall, OnDashAvailable

#### **5. AnimationController** - 🎨 애니메이션 제어 전담
- **역할**: Animator 파라미터 관리 및 스프라이트 제어
- **책임**: 애니메이션 파라미터 설정, 방향 제어, 시각적 효과
- **특징**: 애니메이션과 로직의 완전 분리

#### **6. GroundChecker** - 🌱 지면 검사 전담
- **역할**: 정밀한 지면 상태 감지
- **책임**: 착지/이륙 감지, 접지 상태 관리
- **특징**: 점프 중 오감지 방지, 다중 레이 검사

### 📁 파일 구조
```
Assets/Scripts/Player/
├── PlayerController.cs              # 메인 코디네이터 (708줄 → SRP 준수)
├── InputHandler.cs                  # 입력 처리 전담 (193줄)
├── PhysicsController.cs             # 물리 시스템 전담 (320줄)
├── EnvironmentChecker.cs            # 환경 검사 전담 (258줄)
├── AnimationController.cs           # 애니메이션 제어 전담 (311줄)
├── GroundChecker.cs                 # 지면 검사 전담 (291줄)
├── PlayerStateType.cs               # 상태/이벤트 열거형
├── PlayerStats.cs                   # 플레이어 능력치
├── PlayerStateTransitions.cs        # 상태 전환 규칙
├── PlayerSetupGuide.cs             # 설정 도우미
├── States/                         # 상태별 구현 파일들
│   ├── PlayerBaseState.cs          # 베이스 상태 클래스
│   ├── PlayerIdleState.cs          # 대기 상태
│   ├── PlayerMoveState.cs          # 이동 상태
│   ├── PlayerJumpState.cs          # 점프 상태
│   ├── PlayerFallState.cs          # 낙하 상태
│   ├── PlayerDashState.cs          # 대시 상태
│   ├── PlayerAttackState.cs        # 공격 상태
│   ├── PlayerHitState.cs           # 피격 상태
│   ├── PlayerDeadState.cs          # 사망 상태
│   ├── PlayerWallGrabState.cs      # 벽잡기 상태
│   ├── PlayerWallJumpState.cs      # 벽점프 상태
│   └── PlayerSlideState.cs         # 슬라이딩 상태
└── Tests/                          # 테스트 관련 파일들
```

## 🚀 빠른 시작

### 1. 플레이어 오브젝트 설정

#### ✅ 자동 설정 (강력 권장)
```csharp
// PlayerSetupGuide 컴포넌트를 플레이어 오브젝트에 추가
// Inspector에서 "플레이어 컴포넌트 자동 설정" 버튼 클릭
// 또는 코드에서:
playerGameObject.GetComponent<PlayerSetupGuide>().SetupPlayerComponents();
```

#### 🔧 수동 설정
필수 컴포넌트들을 플레이어 오브젝트에 추가:
```
✅ PlayerController (메인 컨트롤러)
✅ Collider2D (CapsuleCollider2D 권장)
✅ SpriteRenderer
✅ AbilitySystem (GAS 연동)

자동 생성되는 컴포넌트들:
- InputHandler
- PhysicsController
- EnvironmentChecker
- AnimationController
- GroundChecker
- StateMachine
```

### 2. 🎛️ 기본 설정 값

#### PlayerController 설정
```csharp
[Header("완전 커스텀 물리 설정")]
moveSpeed = 8f;        // 이동 속도
jumpForce = 15f;       // 점프 힘
dashSpeed = 20f;       // 대시 속도
dashDuration = 0.2f;   // 대시 지속 시간
gravity = 30f;         // 중력 값
maxFallSpeed = 20f;    // 최대 낙하 속도
airMoveSpeed = 6f;     // 공중 이동 속도

[Header("접지 검사")]
groundCheckRadius = 0.1f;      // 땅 감지 반경
groundLayerMask = Ground;      // 땅 레이어
```

#### PhysicsController 설정
```csharp
[Header("물리 설정")]
gravity = 30f;                    // 중력 값
maxFallSpeed = 20f;              // 최대 낙하 속도
enableDebugLogs = false;         // 디버그 로그
enableGroundCorrection = true;   // 지면 보정 (묻힘 방지)
```

#### GroundChecker 설정
```csharp
[Header("Ground Check Settings")]
groundCheckDistance = 0.1f;      // 지면 검사 거리
groundCheckWidth = 0.6f;         // 검사 폭
rayCount = 3;                    // 레이 개수
showDebugRays = true;            // 디버그 레이 표시
```

#### EnvironmentChecker 설정
```csharp
[Header("환경 검사 설정")]
wallCheckDistance = 0.5f;        // 벽 감지 거리
dashCooldownTime = 1f;          // 대시 쿨다운 시간
```

### 3. 🎮 입력 설정
기본 Unity Input Manager 사용:
- **이동**: Horizontal 축 (A/D 키, 화살표)
- **점프**: Jump 버튼 (스페이스바)
- **대시**: Shift 키
- **공격**: 마우스 좌클릭 또는 X 키
- **슬라이드**: S 키 + 이동

## 🔄 컴포넌트 간 통신

### 이벤트 기반 통신 구조
```csharp
// InputHandler → PlayerController
inputHandler.OnJumpPressed += () => TriggerEvent(PlayerEventType.JumpPressed);
inputHandler.OnMovementInput += (input) => { /* 방향 업데이트 */ };

// EnvironmentChecker → PlayerController
environmentChecker.OnTouchWall += () => TriggerEvent(PlayerEventType.TouchWall);
environmentChecker.OnDashAvailable += () => { /* 대시 가능 상태 */ };

// PhysicsController → 모든 컴포넌트
physicsController.OnVelocityChanged += (velocity) => { /* 속도 변경 처리 */ };

// GroundChecker → PlayerController
groundChecker.OnTouchGround += HandleGroundTouchEvent;
groundChecker.OnLeaveGround += HandleGroundLeaveEvent;
```

### 메서드 위임 패턴
```csharp
// PlayerController가 다른 컴포넌트의 메서드를 위임
public void SetVelocity(Vector3 velocity) => physicsController?.SetVelocity(velocity);
public void ApplyJump(float jumpPower) => physicsController?.ApplyJump(jumpPower);
public void StartDashCooldown() => environmentChecker?.StartDashCooldown();
```

## 🎯 상태별 동작 설명

### 기본 상태들

#### Idle (대기)
- **진입 조건**: 이동 입력 없음, 땅에 서 있음
- **동작**: 수평 이동 서서히 정지, 중력 적용
- **전환**: 이동 입력 시 Move로, 점프 입력 시 Jump로
- **컴포넌트 활용**: PhysicsController로 감속, InputHandler로 입력 감지

#### Move (이동)
- **진입 조건**: 수평 이동 입력 감지
- **동작**: 부드러운 가속/감속으로 이동
- **전환**: 입력 중단 시 Idle로, 다른 액션 입력 시 해당 상태로
- **컴포넌트 활용**: PhysicsController로 이동, AnimationController로 애니메이션

#### Jump (점프)
- **진입 조건**: 점프 입력 + (접지 상태 또는 벽 접촉)
- **동작**: 위쪽 힘 적용, 공중에서 좌우 이동 가능
- **전환**: 하강 시작 시 Fall로
- **컴포넌트 활용**: PhysicsController로 점프 힘 적용

#### Fall (낙하)
- **진입 조건**: 공중에서 하강 속도 > 0.1f
- **동작**: 향상된 중력 적용, 공중 이동 제한적
- **전환**: 착지 시 적절한 상태로 (GroundChecker로 감지)
- **특징**: 착지 시 묻힘 방지 시스템 자동 작동

#### Dash (대시)
- **진입 조건**: 대시 입력 + 대시 가능 상태
- **동작**: 빠른 수평 이동, 중력 무시, 쿨타임 적용
- **전환**: 지속시간 종료 시 이전 상태에 따라
- **컴포넌트 활용**: EnvironmentChecker로 쿨다운 관리

#### Attack (공격)
- **진입 조건**: 공격 입력
- **동작**: 공격 애니메이션, 피해 판정 처리
- **전환**: 공격 완료 시 이동 상태에 따라
- **컴포넌트 활용**: AnimationController로 공격 애니메이션

### 고급 상태들

#### WallGrab (벽잡기)
- **진입 조건**: 벽 접촉 + 공중 상태
- **동작**: 벽에 매달린 상태, 슬라이딩 감속
- **전환**: 벽에서 떨어지거나 벽점프 입력 시
- **컴포넌트 활용**: EnvironmentChecker로 벽 감지

#### WallJump (벽점프)
- **진입 조건**: 벽잡기 상태에서 점프 입력
- **동작**: 벽 반대 방향으로 점프
- **전환**: 점프 완료 후 적절한 공중 상태로

#### Slide (슬라이딩)
- **진입 조건**: 이동 중 + S 키 입력 + 접지 상태
- **동작**: 낮은 자세로 빠른 슬라이딩
- **전환**: 속도 감소 시 또는 입력 해제 시

#### Hit (피격)
- **진입 조건**: 데미지 받음
- **동작**: 피격 애니메이션, 무적 시간, 넉백
- **전환**: 피격 애니메이션 완료 시

#### Dead (사망)
- **진입 조건**: HP 0 이하
- **동작**: 사망 애니메이션, 입력 무시
- **전환**: 리스폰 이벤트 시

## 🎪 이벤트 시스템

### 이벤트 타입들
```csharp
// 이동 관련 (InputHandler 발생)
StartMove, StopMove

// 점프 관련 (InputHandler, GroundChecker 발생)
JumpPressed, JumpReleased, TouchGround, LeaveGround

// 대시 관련 (InputHandler, EnvironmentChecker 발생)
DashPressed, DashCompleted

// 공격 관련 (InputHandler 발생)
AttackPressed, AttackCompleted

// 피격/사망 관련
TakeDamage, RecoverFromHit, Die, Respawn

// 벽 관련 (EnvironmentChecker 발생)
TouchWall, LeaveWall, WallJumpPressed

// 슬라이딩 관련 (InputHandler 발생)
SlidePressed, SlideCompleted
```

### 이벤트 발생 방법
```csharp
// PlayerController에서 이벤트 트리거
playerController.TriggerEvent(PlayerEventType.JumpPressed);

// 컴포넌트에서 자동으로 이벤트 발생
inputHandler.OnJumpPressed?.Invoke(); // → PlayerController로 전달
```

## 🔧 커스터마이징

### 1. 새로운 상태 추가

#### 단계 1: 상태 타입 추가
```csharp
// PlayerStateType.cs에 추가
public enum PlayerStateType
{
    // ... 기존 상태들
    NewCustomState  // 새로운 상태 추가
}
```

#### 단계 2: 상태 클래스 생성
```csharp
// Assets/Scripts/Player/States/PlayerNewCustomState.cs
using System.Threading;
using UnityEngine;

namespace Player
{
    public class PlayerNewCustomState : PlayerBaseState
    {
        public PlayerNewCustomState() : base(PlayerStateType.NewCustomState) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("커스텀 상태 진입");

            // 컴포넌트 활용 예시
            playerController.SetVelocity(Vector3.zero);
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("커스텀 상태 종료");
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 컴포넌트를 통한 상태 관리
            HandleCustomBehavior();
            CheckForStateTransitions();
        }

        private void HandleCustomBehavior()
        {
            // InputHandler를 통한 입력 처리
            Vector2 input = playerController.GetInputVector();

            // PhysicsController를 통한 물리 처리
            playerController.ApplyGravity();

            // AnimationController를 통한 애니메이션 처리
            // (PlayerController.Update에서 자동 처리됨)
        }

        private void CheckForStateTransitions()
        {
            // GroundChecker를 통한 지면 상태 확인
            if (!playerController.IsGrounded)
            {
                // 낙하 상태로 전환
            }
        }
    }
}
```

#### 단계 3: 상태 머신에 등록
```csharp
// PlayerController.InitializeStateMachine()에 추가
stateMachine.AddState(new PlayerNewCustomState());
```

### 2. 컴포넌트 확장

#### 새로운 전담 컴포넌트 추가
```csharp
// 예: HealthController.cs (체력 관리 전담)
public class HealthController : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }
}
```

#### PlayerController에 통합
```csharp
// PlayerController.InitializeComponents()에 추가
private HealthController healthController;

healthController = GetComponent<HealthController>();
if (healthController == null)
{
    healthController = gameObject.AddComponent<HealthController>();
}

// 이벤트 구독
healthController.OnDeath += () => TriggerEvent(PlayerEventType.Die);
```

### 3. 입력 시스템 확장

#### InputHandler에 새로운 입력 추가
```csharp
// InputHandler.cs에서
[Header("확장 입력")]
private bool skillPressed;
public event Action OnSkillPressed;

private void HandleSkillInput()
{
    bool skillInput = Input.GetKeyDown(KeyCode.Q);
    if (skillInput && !skillPressed)
    {
        skillPressed = true;
        OnSkillPressed?.Invoke();
    }
}

// Update()에 추가
HandleSkillInput();
```

## 🔍 디버깅

### 1. 컴포넌트별 디버그 설정
```csharp
// PlayerController
showDebugInfo = true;          // 전체 디버그 정보
showDetailedLogs = true;       // 상세 로그 (1초마다)

// PhysicsController
enableDebugLogs = true;        // 물리 관련 로그
enableGroundCorrection = true; // 지면 보정 활성화

// GroundChecker
showDebugRays = true;          // Scene 뷰에 레이 표시

// EnvironmentChecker
enableDebugLogs = true;        // 환경 검사 로그

// AnimationController
enableDebugLogs = true;        // 애니메이션 로그
```

### 2. 상태 정보 확인
```csharp
// 현재 상태 확인
PlayerStateType currentState = playerController.CurrentState;
Debug.Log($"현재 상태: {currentState}");

// 컴포넌트별 상태 확인
Debug.Log(playerController.GetInputVector());           // 입력 상태
Debug.Log(playerController.Velocity);                   // 물리 상태
Debug.Log(playerController.IsGrounded);                // 지면 상태
Debug.Log(playerController.IsTouchingWall);            // 벽 접촉 상태

// 상태 변경 이벤트 구독
playerController.OnStateChanged += (from, to) => {
    Debug.Log($"상태 변경: {from} → {to}");
};
```

### 3. Scene 뷰 디버그 정보
- **GroundChecker**: 지면 검사 레이 표시 (빨강/초록)
- **EnvironmentChecker**: 벽 검사 레이 표시
- **PlayerController**: Ground Check 원형 영역 표시

## ⚡ 성능 최적화

### 1. 컴포넌트 분리로 인한 이점
- **개별 최적화**: 각 컴포넌트별로 독립적인 최적화 가능
- **선택적 업데이트**: 필요한 컴포넌트만 업데이트
- **메모리 효율성**: 불필요한 참조 최소화

### 2. 이벤트 시스템 최적화
- **매 프레임 체크 감소**: 이벤트 기반으로 필요할 때만 처리
- **조건부 처리**: enableDebugLogs 등으로 불필요한 처리 방지

### 3. 물리 최적화
- **예측 충돌 검사**: 묻힘 현상 방지로 추가 보정 작업 불필요
- **효율적인 지면 검사**: 다중 레이로 정확하면서도 효율적인 검사

## ⚠️ 주의사항

### 1. 컴포넌트 의존성 관리
```csharp
// null 체크 필수
physicsController?.SetVelocity(velocity);
inputHandler?.ResetJump();
```

### 2. 이벤트 구독 해제
```csharp
// OnDestroy에서 모든 이벤트 구독 해제
private void OnDestroy()
{
    if (inputHandler != null)
    {
        inputHandler.OnJumpPressed -= HandleJumpPressed;
        // ... 다른 이벤트들도 해제
    }
}
```

### 3. 상태 전환 우선순위
- 사망 > 피격 > 특수 액션 > 기본 이동 순으로 우선순위 설정

### 4. 착지 시스템 주의사항
- `enableGroundCorrection = true` 유지 (묻힘 방지)
- GroundChecker의 `groundCheckDistance`를 너무 크게 설정하지 않기

## 🔧 문제 해결

### 자주 발생하는 문제들

#### 1. 플레이어가 움직이지 않음
- **체크 리스트**:
  - ✅ PlayerController가 제대로 초기화되었는지
  - ✅ InputHandler가 자동 생성되었는지
  - ✅ PhysicsController의 enableGroundCorrection 설정
  - ✅ Ground Layer Mask 설정
  - ✅ Input System 설정

#### 2. 상태 전환이 안됨
- **체크 리스트**:
  - ✅ PlayerStateTransitions 설정 확인
  - ✅ 이벤트 구독이 제대로 되었는지 확인
  - ✅ Debug 로그로 이벤트 발생 추적
  - ✅ 컴포넌트 자동 생성 확인

#### 3. 착지 시 묻힘 현상
- **해결책**:
  - ✅ PhysicsController의 `enableGroundCorrection = true`
  - ✅ GroundChecker의 `groundCheckDistance` 조정 (0.1f 권장)
  - ✅ enableDebugLogs로 보정 작업 확인

#### 4. 점프/대시가 안됨
- **체크 리스트**:
  - ✅ EnvironmentChecker의 CanDash 상태 확인
  - ✅ GroundChecker의 IsGrounded 상태 확인
  - ✅ 쿨타임 설정 확인
  - ✅ InputHandler의 입력 감지 확인

#### 5. 벽 충돌 감지 안됨
- **체크 리스트**:
  - ✅ EnvironmentChecker의 wallCheckDistance 조정
  - ✅ Layer Mask 설정 확인
  - ✅ Collider 크기 및 위치 확인
  - ✅ Scene 뷰에서 디버그 레이 확인

## 🚀 확장 가능성

현재 컴포넌트 분리 아키텍처는 다음과 같은 확장이 매우 용이합니다:

### 새로운 컴포넌트 추가
- **HealthController**: 체력 관리 전담
- **InventoryController**: 인벤토리 관리 전담
- **SkillController**: 스킬 시스템 전담
- **EffectController**: 시각/음향 효과 전담

### 새로운 이동 모드
- **SwimController**: 수영 물리 시스템
- **FlyController**: 비행 물리 시스템
- **ClimbController**: 등반 시스템

### AI 시스템 확장
- 동일한 컴포넌트 구조를 NPC AI에도 활용 가능
- InputHandler를 AIController로 교체하여 AI 행동 구현

이 컴포넌트 분리 아키텍처를 통해 **단일책임원칙**을 준수하면서도 **확장성과 유지보수성**을 크게 향상시킨 플레이어 시스템을 구축할 수 있습니다.