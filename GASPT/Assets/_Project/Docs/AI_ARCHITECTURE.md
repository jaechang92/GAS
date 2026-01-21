# AI 시스템 아키텍처 설계 문서

## 1. 현재 구조 분석

### 1.1 클래스 계층 구조
```
Enemy (추상, partial)
├── Enemy.cs: 필드, 프로퍼티, 생명주기, 초기화
├── Enemy.Combat.cs: 전투 (데미지, 공격) + 보상
└── Enemy.StatusEffect.cs: StatusEffect + IPoolable

PlatformerEnemy (추상) : Enemy
├── EnemyState enum (Idle, Patrol, Chase, Attack, Dead)
├── FSM 로직 (ChangeState, OnStateEnter, OnStateExit, UpdateState)
├── 플레이어 감지 (IsPlayerInDetectionRange, IsPlayerInAttackRange)
├── 이동 (Move, Stop, Flip)
└── 공격 (CanAttack, AttackPlayer)

BasicMeleeEnemy : PlatformerEnemy
├── 순찰 로직 (patrolLeftBound, patrolRightBound)
├── 낭떠러지 체크 (IsEdgeAhead)
└── 상태별 업데이트 (UpdateIdle, UpdatePatrol, UpdateChase, UpdateAttack)

RangedEnemy : PlatformerEnemy
├── 추가 상태 (RangedEnemyState: RangedAttack, Retreat)
├── 투사체 발사 (FireProjectile)
└── 후퇴 로직 (ChangeStateToRetreat, PhysicsRetreat)

FlyingEnemy : PlatformerEnemy
├── 별도 FSM (FlyingState: Idle, Fly, PositionAbove, DiveAttack, ReturnToAir, Dead)
├── 비행 로직 (targetFlyHeight, UpdateFlyingDirection)
└── 급강하 공격 (PhysicsDiveAttack)
```

### 1.2 현재 문제점

1. **FSM 로직 분산**: 각 적 클래스에 FSM 로직이 중복
2. **상태 전환 하드코딩**: switch문으로 상태 전환 처리
3. **확장성 부족**: 새 상태 추가 시 여러 파일 수정 필요
4. **FlyingEnemy 별도 FSM**: PlatformerEnemy의 EnemyState와 분리

---

## 2. 개선된 아키텍처 설계

### 2.1 설계 목표

1. **모듈화**: 상태를 독립적인 클래스로 분리
2. **데이터 드리븐**: EnemyData에서 AI 행동 설정
3. **재사용성**: 상태 클래스를 여러 적 타입에서 공유
4. **확장성**: 새 상태 추가 시 기존 코드 수정 최소화

### 2.2 새 클래스 구조

```
AI/
├── States/
│   ├── IEnemyAIState.cs          (인터페이스)
│   ├── EnemyAIStateBase.cs       (추상 베이스)
│   ├── IdleState.cs
│   ├── PatrolState.cs
│   ├── ChaseState.cs
│   ├── AttackState.cs
│   ├── RetreatState.cs           (원거리용)
│   ├── FlyState.cs               (비행용)
│   ├── DiveAttackState.cs        (비행용)
│   └── DeadState.cs
├── EnemyAIController.cs          (FSM 관리 컴포넌트)
└── EnemyAIContext.cs             (공유 데이터)
```

### 2.3 핵심 인터페이스

```csharp
/// <summary>
/// 적 AI 상태 인터페이스
/// </summary>
public interface IEnemyAIState
{
    /// <summary>상태 이름</summary>
    string StateName { get; }

    /// <summary>상태 진입 시 호출</summary>
    void Enter(EnemyAIContext context);

    /// <summary>상태 퇴장 시 호출</summary>
    void Exit(EnemyAIContext context);

    /// <summary>매 프레임 업데이트</summary>
    void Update(EnemyAIContext context);

    /// <summary>물리 업데이트 (FixedUpdate)</summary>
    void PhysicsUpdate(EnemyAIContext context);

    /// <summary>다음 상태 결정 (null이면 현재 상태 유지)</summary>
    IEnemyAIState CheckTransitions(EnemyAIContext context);
}
```

### 2.4 컨텍스트 클래스

```csharp
/// <summary>
/// AI 상태 간 공유 데이터
/// </summary>
public class EnemyAIContext
{
    // 컴포넌트 참조
    public PlatformerEnemy Enemy { get; set; }
    public Rigidbody2D Rigidbody { get; set; }
    public Transform Transform { get; set; }

    // 플레이어 참조
    public Transform PlayerTransform { get; set; }
    public PlayerStats PlayerStats { get; set; }

    // 설정 (EnemyData에서)
    public EnemyData Data { get; set; }

    // 상태 공유 변수
    public Vector3 StartPosition { get; set; }
    public float LastAttackTime { get; set; }
    public bool IsFacingRight { get; set; }

    // 순찰
    public float PatrolLeftBound { get; set; }
    public float PatrolRightBound { get; set; }
    public bool PatrolMovingRight { get; set; }

    // 타이머
    public float StateTimer { get; set; }
}
```

### 2.5 상태 전환 로직

```csharp
// IdleState에서
public IEnemyAIState CheckTransitions(EnemyAIContext context)
{
    if (context.StateTimer >= IdleDuration)
    {
        return new PatrolState();
    }
    return null; // 현재 상태 유지
}

// PatrolState에서
public IEnemyAIState CheckTransitions(EnemyAIContext context)
{
    if (IsPlayerInDetectionRange(context))
    {
        return new ChaseState();
    }
    return null;
}
```

---

## 3. 구현 계획

### Phase 3-2: EnemyAIState 베이스 클래스 생성
- `IEnemyAIState.cs` 인터페이스 정의
- `EnemyAIStateBase.cs` 추상 클래스 (공통 유틸리티)
- `EnemyAIContext.cs` 컨텍스트 클래스

### Phase 3-3: 구체 상태 클래스 구현
- `IdleState.cs`: 대기 (일정 시간 후 Patrol로 전환)
- `PatrolState.cs`: 순찰 (범위 내 왕복, 플레이어 감지 시 Chase)
- `ChaseState.cs`: 추격 (플레이어 방향 이동, 공격 범위 진입 시 Attack)
- `AttackState.cs`: 공격 (쿨다운, 공격 실행)
- `DeadState.cs`: 사망 (정지, 콜라이더 비활성화)

### Phase 3-4: EnemyAIController 컴포넌트 생성
- FSM 상태 관리
- 상태 전환 처리
- Update/FixedUpdate에서 현재 상태 호출

### Phase 3-5: 기존 적 클래스 리팩토링
- `BasicMeleeEnemy` → `EnemyAIController` + 기본 상태들
- `RangedEnemy` → 추가 상태 (RetreatState)
- `FlyingEnemy` → 비행 전용 상태들

---

## 4. 마이그레이션 전략

### 4.1 점진적 마이그레이션
1. 새 AI 시스템을 별도 폴더에 구현
2. `BasicMeleeEnemy`를 먼저 마이그레이션
3. 테스트 후 나머지 적 타입 마이그레이션
4. 기존 FSM 로직은 deprecated 처리

### 4.2 하위 호환성
- 기존 `EnemyState` enum 유지 (디버깅용)
- `PlatformerEnemy`의 기존 메서드 유지 (호출 위임)

---

## 5. 확장 가능성

### 5.1 Behavior Tree 통합 (미래)
- 현재 FSM → BT 마이그레이션 경로 확보
- IEnemyAIState를 BT 노드로 래핑 가능

### 5.2 데이터 드리븐 AI
- `EnemyAIData` ScriptableObject로 상태 전환 조건 설정
- 에디터에서 AI 행동 튜닝

### 5.3 AI 디버거
- 현재 상태, 전환 조건 시각화
- Gizmos로 감지 범위, 경로 표시

---

## 6. 구현 완료 (2026-01-21)

### 생성된 파일 목록

```
Assets/_Project/Scripts/Gameplay/Enemy/AI/
├── IEnemyAIState.cs              (인터페이스)
├── EnemyAIContext.cs             (공유 데이터)
├── EnemyAIStateBase.cs           (추상 베이스)
├── EnemyAIController.cs          (FSM 컨트롤러)
└── States/
    ├── IdleState.cs
    ├── PatrolState.cs
    ├── ChaseState.cs
    ├── AttackState.cs
    └── DeadState.cs

Assets/_Project/Scripts/Gameplay/Enemy/
└── BasicMeleeEnemyV2.cs          (새 AI 시스템 적용 예시)
```

### 사용 방법

1. **새 적 생성 (EnemyAIController 직접 사용)**
   ```csharp
   // GameObject에 EnemyAIController 추가
   // EnemyData 설정
   // 자동으로 Idle → Patrol → Chase → Attack FSM 작동
   ```

2. **기존 Enemy 클래스와 통합 (BasicMeleeEnemyV2 방식)**
   ```csharp
   [RequireComponent(typeof(EnemyAIController))]
   public class MyEnemy : Enemy
   {
       private EnemyAIController aiController;

       protected override void Start()
       {
           base.Start();
           aiController = GetComponent<EnemyAIController>();
           aiController.SetEnemy(this);
       }
   }
   ```

3. **커스텀 상태 추가**
   ```csharp
   public class MyCustomState : EnemyAIStateBase
   {
       public override string StateName => "MyCustom";

       public override void Update(EnemyAIContext context) { /* ... */ }

       public override IEnemyAIState CheckTransitions(EnemyAIContext context)
       {
           // 전환 조건 체크
           return null;
       }
   }
   ```

### 기존 코드와의 호환성

- `BasicMeleeEnemy`, `RangedEnemy`, `FlyingEnemy` 등 기존 클래스는 그대로 유지
- 새 적 타입은 `BasicMeleeEnemyV2` 패턴으로 구현 권장
- 점진적 마이그레이션 가능
