# Implementation Plan: First Playable Level MVP

**Feature**: `003-first-playable-level`
**Plan Created**: 2025-11-01
**Status**: Draft
**Based on Spec**: [spec.md](./spec.md)

---

## 목차

1. [개요](#개요)
2. [기술 아키텍처](#기술-아키텍처)
3. [우선순위별 구현 단계](#우선순위별-구현-단계)
4. [Phase 1: P1 구현 (필수 - 기본 레벨 트래버설)](#phase-1-p1-구현-필수---기본-레벨-트래버설)
5. [Phase 2: P2 구현 (전투 시스템)](#phase-2-p2-구현-전투-시스템)
6. [Phase 3: P3 구현 (Grimoire Magic 시스템)](#phase-3-p3-구현-grimoire-magic-시스템)
7. [통합 및 테스트](#통합-및-테스트)
8. [기술적 고려사항 및 리스크](#기술적-고려사항-및-리스크)
9. [의존성 맵](#의존성-맵)
10. [파일 구조](#파일-구조)

---

## 개요

### 목표

GASPT 프로젝트의 **첫 번째 플레이 가능한 레벨 MVP**를 구현합니다. 다크 판타지 2D 액션 플랫포머로, 로브를 입은 마법사 주인공이 기본 이동/전투/Grimoire 마법 능력을 사용하여 레벨을 완주하는 경험을 제공합니다.

### 핵심 가치 제안

- **기본 게임플레이 루프 검증**: 이동 → 전투 → 마법 사용 → 목표 도달
- **Grimoire 시스템 차별화**: 마법사의 클래스 전환(Fire, Ice, Poison 등) 메커니즘을 통한 전술적 깊이 제공
- **MVP 우선**: 완벽한 시스템보다 플레이 가능한 게임을 먼저 구현

### 성공 기준

- [ ] 플레이어가 레벨 시작부터 목표까지 5분 이내 완주 가능
- [ ] 기본 전투로 30초 이내 적 1마리 처치 가능
- [ ] Fire Grimoire 마법 능력을 플레이당 최소 1회 사용 가능
- [ ] 모든 액션에 0.2초 이내 피드백 제공
- [ ] 첫 플레이어의 70% 이상이 3회 시도 내 레벨 완료

---

## 기술 아키텍처

### 기존 시스템 활용

GASPT는 이미 강력한 기반 시스템을 갖추고 있으므로 이를 최대한 활용합니다:

| 기존 시스템 | 역할 | 활용 방안 |
|------------|------|-----------|
| **CharacterPhysics** | 2D 플랫포머 물리 | 마법사 이동, 점프, 벽슬라이드 처리 |
| **GAS Core** | 범용 어빌리티 시스템 | 마법사의 기본 공격 및 Fire Magic 어빌리티 구현 |
| **FSM Core** | 유한 상태 머신 | 마법사/적 상태 관리 (Idle, Moving, Attacking, etc.) |
| **HealthSystem** | 체력 관리 | 마법사 및 적의 HP, 데미지, 사망 처리 |
| **EnemyController** | 적 AI 및 전투 | 기존 Enemy FSM (Idle, Patrol, Trace, Attack, Death) 활용 |
| **SkullSystem** | 클래스 전환 메커니즘 | **Grimoire System**으로 재명명하여 활용 |
| **GameFlowManager** | 게임 상태 관리 | 레벨 시작/완료/재시작 흐름 제어 |

### 새로 구현할 시스템

| 시스템 | 목적 | 우선순위 |
|--------|------|----------|
| **MageController** | 마법사 캐릭터 통합 제어 | P1 |
| **MageInputHandler** | 입력 처리 (이동, 점프, 공격, 마법) | P1 |
| **LevelGoalTrigger** | 레벨 완료 감지 | P1 |
| **FireGrimoire** | Fire 마법 Grimoire 구현 | P3 |
| **FireMagicAbility** | Fire 마법 어빌리티 (GAS 확장) | P3 |
| **LevelManager** | 레벨 생명주기 관리 (시작/완료/재시작) | P2 |
| **MageVisualFeedback** | 피격/공격 시각 피드백 | P2 |

### 아키텍처 다이어그램

```
┌─────────────────────────────────────────────────────────────┐
│                      Game Flow Layer                        │
│  (GameFlowManager → LevelManager → Scene Transitions)       │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────────┐
│                   Gameplay Layer                            │
│                                                              │
│  ┌──────────────┐         ┌──────────────┐                 │
│  │ MageController│◄───────►│EnemyController│                │
│  │  - Physics   │         │  - FSM       │                 │
│  │  - Input     │         │  - Health    │                 │
│  │  - Health    │         │  - Combat    │                 │
│  │  - Grimoire  │         └──────────────┘                 │
│  └──────┬───────┘                                           │
│         │                                                    │
│  ┌──────┴───────┐       ┌──────────────┐                   │
│  │ Grimoire     │       │ Level Goal   │                   │
│  │ System       │       │ Trigger      │                   │
│  │ (Fire Magic) │       └──────────────┘                   │
│  └──────────────┘                                           │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────────┐
│                   Core Systems Layer                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │CharacterPhys.│  │  GAS Core    │  │  FSM Core    │     │
│  │  (Movement)  │  │ (Abilities)  │  │  (States)    │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐                        │
│  │HealthSystem  │  │ DamageSystem │                        │
│  └──────────────┘  └──────────────┘                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 우선순위별 구현 단계

### 우선순위 정의

- **P1 (Critical)**: 기본 레벨 트래버설 - 이동, 점프, 목표 도달
- **P2 (High)**: 전투 시스템 - 적 AI, 기본 공격, 데미지, 사망
- **P3 (Medium)**: Grimoire Magic - Fire Grimoire 마법 능력

### 단계별 마일스톤

| Phase | 마일스톤 | 예상 기간 | 검증 기준 |
|-------|---------|----------|-----------|
| **Phase 1 (P1)** | 플레이어가 레벨을 완주할 수 있다 | 2-3일 | 시작→목표 이동 가능, 레벨 완료 UI 표시 |
| **Phase 2 (P2)** | 플레이어가 적과 전투할 수 있다 | 3-4일 | 적 처치 가능, 플레이어 사망 시 재시작 |
| **Phase 3 (P3)** | 플레이어가 Fire Magic을 사용할 수 있다 | 2-3일 | Fire 마법 발동, 적에게 강력한 데미지 |
| **통합 테스트** | 전체 게임플레이 루프 검증 | 1-2일 | 모든 Success Criteria 통과 |

---

## Phase 1: P1 구현 (필수 - 기본 레벨 트래버설)

### 목표

플레이어가 기본 이동(좌우, 점프)만으로 레벨 시작 지점에서 목표 지점까지 이동하여 레벨을 완료할 수 있다.

### User Story

**US-001**: Basic Level Traversal
- **Given**: 레벨이 시작되었을 때
- **When**: 플레이어가 이동 키(A/D 또는 화살표)를 누르면
- **Then**: 캐릭터가 좌우로 부드럽게 이동한다
- **When**: 플레이어가 점프 키(Space)를 누르면
- **Then**: 캐릭터가 반응형으로 점프한다 (홀드 시간에 따라 높이 조절)
- **When**: 플레이어가 목표 영역에 도달하면
- **Then**: 레벨 완료 피드백과 함께 레벨이 완료된다

### 구현 작업 항목

#### 1.1 Mage Character 셋업 (2-3시간)

**파일**: `Assets/_Project/Scripts/Gameplay/Player/MageController.cs`

**책임**:
- CharacterPhysics 컴포넌트와 통합하여 이동/점프 처리
- 입력을 받아 CharacterPhysics에 전달
- 마법사 고유의 상태 관리 (Idle, Moving, Jumping, Falling)
- 애니메이션 제어 (향후 확장)

**주요 메서드**:
```csharp
public class MageController : MonoBehaviour
{
    // 컴포넌트
    private CharacterPhysics physics;
    private MageInputHandler inputHandler;
    private HealthSystem health;

    // 상태
    private MageState currentState;

    // 초기화
    private void Awake();
    private void Start();

    // 입력 처리
    private void Update();
    private void HandleInput();

    // 이동/점프
    public void Move(float horizontal);
    public void Jump();

    // 상태 관리
    private void UpdateState();
    public void SetState(MageState newState);
}
```

**의존성**:
- CharacterPhysics (기존)
- MageInputHandler (신규, 1.2에서 생성)

---

#### 1.2 Mage Input Handler (1-2시간)

**파일**: `Assets/_Project/Scripts/Gameplay/Player/Input/MageInputHandler.cs`

**책임**:
- Unity Input System 또는 기존 Input을 사용하여 플레이어 입력 감지
- 이동(Horizontal), 점프(Jump), 공격(Attack), 마법(Magic) 입력 처리
- MageController에 입력 이벤트 전달

**주요 메서드**:
```csharp
public class MageInputHandler : MonoBehaviour
{
    // 이벤트
    public event Action<float> OnMoveInput;      // horizontal: -1 ~ 1
    public event Action OnJumpPressed;
    public event Action OnJumpReleased;
    public event Action OnAttackPressed;         // P2에서 사용
    public event Action OnMagicPressed;          // P3에서 사용

    // 입력 감지
    private void Update();
    private void HandleMovementInput();
    private void HandleJumpInput();
    private void HandleActionInput();

    // 입력 활성화/비활성화
    public void SetInputEnabled(bool enabled);
}
```

**의존성**: 없음 (독립적)

---

#### 1.3 Level Scene 구성 (2-3시간)

**Scene**: `Assets/_Project/Scenes/Levels/Level_001_FirstPlayable.unity`

**구성 요소**:

1. **Ground/Platform Setup**
   - Tilemap 또는 Sprite로 플랫폼 배치
   - 적절한 Layer 설정 (Ground)
   - BoxCollider2D 또는 TilemapCollider2D 추가
   - 최소 3-4개의 플랫폼으로 점프 챌린지 구성

2. **Mage Character Prefab**
   - GameObject: "Mage"
   - Components:
     - SpriteRenderer (로브 입은 마법사 스프라이트)
     - Rigidbody2D (CharacterPhysics가 설정)
     - BoxCollider2D
     - CharacterPhysics (기존)
     - MageController (신규)
     - MageInputHandler (신규)
     - HealthSystem (P2에서 활성화)
   - Tag: "Player"
   - Layer: "Player"

3. **Spawn Point**
   - 빈 GameObject: "SpawnPoint"
   - 레벨 시작 위치 표시

4. **Level Goal**
   - GameObject: "LevelGoal"
   - Components:
     - SpriteRenderer (목표 표시 아이콘/포털)
     - BoxCollider2D (IsTrigger = true)
     - LevelGoalTrigger (신규, 1.4에서 생성)

**작업**:
- [ ] 기본 플랫폼 레이아웃 디자인 (종이 스케치)
- [ ] 유니티에서 플랫폼 배치 및 Collider 설정
- [ ] Mage Prefab 생성 및 컴포넌트 연결
- [ ] Spawn Point, Level Goal 배치

---

#### 1.4 Level Goal Trigger (1시간)

**파일**: `Assets/_Project/Scripts/Gameplay/Level/LevelGoalTrigger.cs`

**책임**:
- 플레이어가 목표 영역에 진입했는지 감지
- LevelManager에 레벨 완료 이벤트 전달
- 간단한 시각 피드백 (파티클, 사운드 등)

**주요 메서드**:
```csharp
public class LevelGoalTrigger : MonoBehaviour
{
    [SerializeField] private ParticleSystem completeEffect; // 옵션
    [SerializeField] private AudioClip completeSound;       // 옵션

    // 이벤트
    public static event Action OnLevelGoalReached;

    // 트리거 감지
    private void OnTriggerEnter2D(Collider2D other);

    // 피드백
    private void PlayCompleteFeedback();
}
```

**의존성**:
- LevelManager (1.5에서 생성, 이벤트 구독)

---

#### 1.5 Level Manager (2-3시간)

**파일**: `Assets/_Project/Scripts/Gameplay/Level/LevelManager.cs`

**책임**:
- 레벨 생명주기 관리 (Start → Playing → Completed/Failed)
- GameFlowManager와 연동하여 레벨 시작/완료 흐름 제어
- 레벨 재시작 처리
- 승리/패배 UI 표시

**주요 메서드**:
```csharp
public class LevelManager : MonoBehaviour
{
    // 상태
    private LevelState currentState; // NotStarted, Playing, Completed, Failed

    // 이벤트
    public event Action OnLevelStarted;
    public event Action OnLevelCompleted;
    public event Action OnLevelFailed;

    // 생명주기
    private void Start();
    public void StartLevel();
    public void CompleteLevel();
    public void FailLevel();
    public void RestartLevel();

    // GameFlowManager 연동
    private void TransitionToIngame();
    private void TransitionToMenu();

    // 이벤트 구독
    private void OnEnable();
    private void OnDisable();
}
```

**의존성**:
- GameFlowManager (기존)
- LevelGoalTrigger (1.4)
- HealthSystem (P2에서 플레이어 사망 연동)

---

#### 1.6 Basic UI Setup (1-2시간)

**파일**: `Assets/_Project/Scripts/UI/Level/LevelUI.cs`

**UI 요소**:
1. **Level Start UI**
   - "Press Space to Start" 텍스트
   - 페이드인 효과 (옵션)

2. **Level Complete UI**
   - "Level Complete!" 텍스트
   - "Press R to Restart" 또는 "Next Level" 버튼

3. **Level Failed UI** (P2에서 활성화)
   - "You Died!" 텍스트
   - "Press R to Restart" 버튼

**주요 메서드**:
```csharp
public class LevelUI : MonoBehaviour
{
    [SerializeField] private GameObject startUI;
    [SerializeField] private GameObject completeUI;
    [SerializeField] private GameObject failedUI;

    // UI 표시
    public void ShowStartUI();
    public void HideStartUI();
    public void ShowCompleteUI();
    public void ShowFailedUI();

    // 이벤트 구독
    private void OnEnable();
    private void OnDisable();
}
```

**의존성**:
- LevelManager (이벤트 구독)

---

### P1 검증 체크리스트

완료 후 다음을 검증합니다:

- [ ] Mage 캐릭터가 A/D 키로 좌우 이동 가능
- [ ] Mage 캐릭터가 Space 키로 점프 가능
- [ ] 점프 홀드 시간에 따라 점프 높이 조절 가능
- [ ] 플랫폼에 착지 가능
- [ ] Level Goal에 도달 시 "Level Complete" UI 표시
- [ ] R 키로 레벨 재시작 가능
- [ ] 모든 입력 반응 시간 100ms 이내

---

## Phase 2: P2 구현 (전투 시스템)

### 목표

플레이어가 기본 공격으로 적을 공격하고, 적이 플레이어를 공격하며, 체력이 0이 되면 사망하는 전투 시스템을 구현합니다.

### User Story

**US-002**: Basic Combat
- **Given**: 레벨에 적이 배치되었을 때
- **When**: 플레이어가 공격 버튼(좌클릭)을 누르면
- **Then**: 플레이어가 공격 애니메이션과 함께 적에게 데미지를 입힌다
- **When**: 적의 체력이 0이 되면
- **Then**: 적이 사망 애니메이션과 함께 제거된다
- **When**: 적이 플레이어를 공격하면
- **Then**: 플레이어가 피격 피드백(점멸, 사운드)과 함께 데미지를 받는다
- **When**: 플레이어 체력이 0이 되면
- **Then**: 게임 오버 UI가 표시되고 레벨이 재시작된다

### 구현 작업 항목

#### 2.1 Basic Attack Ability (2-3시간)

**파일**: `Assets/_Project/Scripts/Gameplay/Player/Abilities/BasicMagicAttack.cs`

**책임**:
- GAS Core의 `Ability` 클래스를 상속하여 기본 마법 공격 구현
- 근접 범위 공격 (짧은 거리의 마법 발사체 또는 근접 히트박스)
- 데미지 적용 및 히트 피드백

**주요 메서드**:
```csharp
public class BasicMagicAttack : Ability
{
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private GameObject attackEffectPrefab;

    // 어빌리티 실행
    protected override async Awaitable ExecuteAbilityEffect(CancellationToken token);

    // 타겟 탐지
    private IAbilityTarget[] FindEnemiesInRange();

    // 데미지 적용
    protected override void ApplyEffectToTarget(IAbilityTarget target);

    // 이펙트 생성
    private void SpawnAttackEffect();
}
```

**의존성**:
- GAS Core (기존)
- DamageSystem (기존)
- EnemyController (적이 IAbilityTarget 인터페이스 구현 필요)

---

#### 2.2 Enemy Setup in Level (2시간)

**Enemy Prefab**: `Assets/_Project/Prefabs/Enemies/BasicEnemy.prefab`

**구성**:
- GameObject: "BasicEnemy"
- Components:
  - SpriteRenderer (적 스프라이트)
  - Rigidbody2D
  - BoxCollider2D
  - EnemyController (기존)
  - HealthSystem (기존)
  - EnemyData (ScriptableObject)
- Tag: "Enemy"
- Layer: "Enemy"

**EnemyData 설정**:
```csharp
// 기존 EnemyData ScriptableObject 사용
- maxHealth: 30
- moveSpeed: 2f
- detectionRange: 5f
- chaseRange: 7f
- attackRange: 1.5f
- attackDamage: 5f
- attackCooldown: 2f
- enablePatrol: true
- patrolRange: 3f
```

**작업**:
- [ ] BasicEnemy Prefab 생성
- [ ] EnemyData ScriptableObject 생성 및 설정
- [ ] 레벨에 2-3마리의 적 배치 (플랫폼 위)

---

#### 2.3 Combat Integration (2-3시간)

**MageController 확장**:

**파일**: `Assets/_Project/Scripts/Gameplay/Player/MageController.cs` (수정)

**추가 기능**:
- BasicMagicAttack 어빌리티 연동
- 공격 입력 처리
- 공격 쿨다운 관리

**추가 메서드**:
```csharp
// MageController.cs에 추가
private AbilitySystem abilitySystem;
private BasicMagicAttack basicAttack;

private void InitializeAbilities();
public async Awaitable PerformBasicAttack();
private void HandleAttackInput();
```

**MageInputHandler 확장**:

**파일**: `Assets/_Project/Scripts/Gameplay/Player/Input/MageInputHandler.cs` (수정)

**추가 기능**:
- 공격 입력 이벤트 발생 (좌클릭 또는 J키)

---

#### 2.4 Player Health & Death (2시간)

**MageController Health Integration**:

**추가 컴포넌트**:
- HealthSystem (기존, Mage Prefab에 추가)

**추가 기능**:
```csharp
// MageController.cs에 추가
private HealthSystem health;

private void InitializeHealth();
private void OnPlayerDeath();
private void HandlePlayerDeath();
```

**Death Flow**:
1. HealthSystem의 `OnDeath` 이벤트 구독
2. 사망 시 입력 비활성화
3. 사망 애니메이션 재생 (옵션)
4. LevelManager에 `FailLevel()` 호출
5. "You Died!" UI 표시
6. 2초 후 자동 재시작 또는 R 키 입력 대기

**LevelManager 수정**:
- `FailLevel()` 메서드 구현
- 플레이어 사망 이벤트 구독
- 레벨 재시작 로직 추가

---

#### 2.5 Visual & Audio Feedback (2-3시간)

**파일**: `Assets/_Project/Scripts/Gameplay/Player/MageVisualFeedback.cs`

**책임**:
- 플레이어 피격 시 시각 피드백 (스프라이트 점멸, 색상 변경)
- 공격 시 이펙트 재생
- 사운드 재생 (공격, 피격, 사망)

**주요 메서드**:
```csharp
public class MageVisualFeedback : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AudioSource audioSource;

    [Header("Hit Feedback")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.2f;

    [Header("Sounds")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    // 피드백 실행
    public async Awaitable PlayHitFeedback();
    public void PlayAttackFeedback();
    public void PlayDeathFeedback();

    // 스프라이트 점멸
    private async Awaitable FlashSprite(Color color, float duration);
}
```

**의존성**:
- HealthSystem (OnDamaged 이벤트 구독)
- MageController (공격/사망 이벤트 구독)

---

#### 2.6 Enemy IAbilityTarget 구현 (1시간)

**파일**: `Assets/_Project/Scripts/Gameplay/Enemy/EnemyAbilityTarget.cs`

**책임**:
- GAS Core의 `IAbilityTarget` 인터페이스 구현
- EnemyController의 HealthSystem과 연동

**주요 메서드**:
```csharp
public class EnemyAbilityTarget : MonoBehaviour, IAbilityTarget
{
    private HealthSystem health;

    public bool IsTargetable => health.IsAlive;

    public void TakeDamage(float amount);
    public void Heal(float amount);
    public Vector3 GetPosition() => transform.position;
}
```

**작업**:
- [ ] 스크립트 작성
- [ ] BasicEnemy Prefab에 컴포넌트 추가

---

### P2 검증 체크리스트

- [ ] 플레이어가 좌클릭으로 기본 공격 수행
- [ ] 기본 공격이 적에게 데미지 적용
- [ ] 적 체력 0 시 사망 및 제거
- [ ] 적이 플레이어를 감지하고 추적
- [ ] 적이 공격 범위 내에서 플레이어 공격
- [ ] 플레이어가 피격 시 시각/청각 피드백 제공
- [ ] 플레이어 체력 0 시 게임 오버
- [ ] 게임 오버 후 R 키로 재시작 가능

---

## Phase 3: P3 구현 (Grimoire Magic 시스템)

### 목표

플레이어가 Fire Grimoire를 장착하고 강력한 Fire Magic 어빌리티를 사용하여 적에게 큰 데미지를 입힐 수 있습니다.

### User Story

**US-003**: Grimoire Magic Usage
- **Given**: Fire Grimoire가 장착되었을 때
- **When**: 플레이어가 마법 버튼(우클릭 또는 K키)을 누르면
- **Then**: Fire 마법 공격이 화염 파티클과 함께 발동된다
- **When**: Fire 마법이 적에게 적중하면
- **Then**: 적이 기본 공격의 2-3배 데미지를 받고 화염 이펙트가 표시된다
- **When**: Fire 마법을 사용한 후
- **Then**: 쿨다운 UI가 표시되고 5-10초 후 재사용 가능하다

### 구현 작업 항목

#### 3.1 Grimoire System Refactoring (2-3시간)

**기존 SkullSystem → GrimoireSystem으로 재명명**

**작업**:
1. **파일 및 클래스 이름 변경**:
   - `SkullSystem.cs` → `GrimoireSystem.cs`
   - `SkullManager.cs` → `GrimoireManager.cs`
   - `BaseSkull.cs` → `BaseGrimoire.cs`
   - `SkullData.cs` → `GrimoireData.cs`
   - `ISkullController` → `IGrimoireController`

2. **개념 매핑**:
   - Skull Type → Grimoire Type (Fire, Ice, Poison, etc.)
   - Skull Stats → Grimoire Stats (마법 데미지 증폭, 쿨다운 감소 등)
   - Skull Abilities → Grimoire Abilities (마법 어빌리티)

3. **코드 리팩토링**:
   - 모든 "Skull" 용어를 "Grimoire"로 변경
   - 주석 업데이트 (한글 주석 유지)
   - Enum 업데이트: `SkullType` → `GrimoireType`

**파일**:
- `Assets/_Project/Scripts/Gameplay/Grimoire/Core/GrimoireSystem.cs`
- `Assets/_Project/Scripts/Gameplay/Grimoire/Core/GrimoireManager.cs`
- `Assets/_Project/Scripts/Gameplay/Grimoire/Core/BaseGrimoire.cs`
- `Assets/_Project/Scripts/Gameplay/Common/GrimoireData.cs`
- `Assets/_Project/Scripts/Gameplay/Common/IGrimoireController.cs`

**주의사항**:
- Edit 도구를 사용하여 한글 주석 유지
- 기존 기능 유지 (던지기 메커니즘은 MVP에서 제외하되 코드는 유지)

---

#### 3.2 Fire Grimoire Implementation (3-4시간)

**파일**: `Assets/_Project/Scripts/Gameplay/Grimoire/Types/FireGrimoire.cs`

**책임**:
- BaseGrimoire 상속
- Fire Magic 어빌리티 제공
- Fire 속성 스탯 적용 (예: 마법 데미지 +20%, 화염 저항 등)

**주요 메서드**:
```csharp
public class FireGrimoire : BaseGrimoire
{
    [Header("Fire Grimoire Settings")]
    [SerializeField] private FireMagicAbility fireMagicAbility;

    // 초기화
    protected override void InitializeAbilities();

    // 어빌리티 실행
    public override async Awaitable PerformPrimaryAttack();    // 기본 공격 (상속)
    public override async Awaitable PerformSecondaryAttack();  // Fire Magic
    public override async Awaitable PerformUltimate();         // MVP 제외

    // Grimoire 활성화/비활성화
    public override void OnEquipped();
    public override void OnUnequipped();
}
```

**GrimoireData (ScriptableObject)**:
```csharp
// FireGrimoireData.asset
- grimoireName: "Fire Grimoire"
- grimoireType: Fire
- description: "화염 마법을 다루는 그리무아. 강력한 불꽃 공격을 사용할 수 있다."
- icon: FireGrimoireIcon
- stats:
  - magicDamageMultiplier: 1.2f
  - cooldownReduction: 0f (MVP에서는 없음)
```

---

#### 3.3 Fire Magic Ability (3-4시간)

**파일**: `Assets/_Project/Scripts/Gameplay/Grimoire/Abilities/FireMagicAbility.cs`

**책임**:
- GAS Core의 `Ability` 클래스를 상속하여 Fire Magic 구현
- 화염 발사체 또는 범위 공격 생성
- 적에게 기본 공격의 2-3배 데미지 적용
- 화염 파티클 이펙트 및 사운드 재생

**주요 메서드**:
```csharp
public class FireMagicAbility : Ability
{
    [Header("Fire Magic Settings")]
    [SerializeField] private float magicDamage = 25f;           // 기본 공격의 2.5배
    [SerializeField] private float cooldownDuration = 7f;       // 7초 쿨다운
    [SerializeField] private GameObject fireProjectilePrefab;
    [SerializeField] private ParticleSystem fireEffect;
    [SerializeField] private AudioClip fireMagicSound;

    // 어빌리티 실행
    protected override async Awaitable ExecuteAbilityEffect(CancellationToken token);

    // 발사체 생성
    private void SpawnFireProjectile();

    // 타겟 탐지
    private IAbilityTarget[] FindEnemiesInRange();

    // 데미지 적용
    protected override void ApplyEffectToTarget(IAbilityTarget target);

    // 이펙트 재생
    private void PlayFireEffect();
}
```

**Fire Projectile (옵션)**:
- 간단한 경우: 즉시 범위 공격 (BoxCast 또는 CircleCast)
- 복잡한 경우: 발사체 GameObject 생성, 이동, 충돌 감지

**AbilityData (ScriptableObject)**:
```csharp
// FireMagicAbilityData.asset
- abilityId: "fire_magic_001"
- abilityName: "Fire Magic"
- description: "강력한 화염 공격"
- abilityType: Active
- targetType: Area
- damageValue: 25f
- cooldownDuration: 7f
- castTime: 0.3f
- duration: 0f
- range: 5f
```

---

#### 3.4 Grimoire Manager Integration (2시간)

**MageController에 Grimoire 통합**:

**파일**: `Assets/_Project/Scripts/Gameplay/Player/MageController.cs` (수정)

**추가 기능**:
- GrimoireManager 컴포넌트 참조
- 게임 시작 시 Fire Grimoire 자동 장착
- Fire Magic 입력 처리

**추가 메서드**:
```csharp
// MageController.cs에 추가
private GrimoireManager grimoireManager;

private void InitializeGrimoire();
private void EquipFireGrimoire();
public async Awaitable PerformFireMagic();
private void HandleMagicInput();
```

**작업**:
- [ ] MageController에 GrimoireManager 컴포넌트 추가
- [ ] Fire Grimoire Prefab 생성
- [ ] FireGrimoireData ScriptableObject 생성
- [ ] 게임 시작 시 Fire Grimoire 자동 장착 로직 구현

---

#### 3.5 Cooldown UI (2-3시간)

**파일**: `Assets/_Project/Scripts/UI/HUD/AbilityCooldownUI.cs`

**책임**:
- Fire Magic 쿨다운 상태 표시
- 쿨다운 진행 바 (Radial Fill 또는 Linear Fill)
- 쿨다운 남은 시간 텍스트 (옵션)

**UI 구성**:
- Canvas
  - AbilityCooldownPanel
    - FireMagicIcon (Image)
    - CooldownOverlay (Image, Radial Fill)
    - CooldownText (TextMeshPro) - "7.2s" 형식

**주요 메서드**:
```csharp
public class AbilityCooldownUI : MonoBehaviour
{
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;

    private FireMagicAbility fireMagic;

    // UI 업데이트
    private void Update();
    private void UpdateCooldownDisplay();

    // 쿨다운 진행률 계산
    private float GetCooldownProgress();
}
```

**의존성**:
- FireMagicAbility (쿨다운 정보 읽기)

---

#### 3.6 Visual Effects & Particles (2시간)

**Fire Magic 이펙트**:

1. **Fire Projectile Prefab** (옵션):
   - GameObject: "FireProjectile"
   - Components:
     - SpriteRenderer (화염 스프라이트)
     - ParticleSystem (화염 파티클)
     - CircleCollider2D (IsTrigger = true)
     - Rigidbody2D (Kinematic)
     - FireProjectileController (이동 로직)

2. **Fire Impact Effect**:
   - ParticleSystem: 화염 폭발 이펙트
   - 적 히트 시 즉시 재생
   - 1-2초 후 자동 파괴

3. **Fire Burn Effect** (옵션):
   - ParticleSystem: 적에게 지속 화염 이펙트
   - 데미지 적용 시 적 위에 부착

**작업**:
- [ ] Fire Projectile Prefab 생성 (또는 즉시 공격 구현)
- [ ] Fire Impact ParticleSystem 생성
- [ ] FireMagicAbility에 이펙트 연동

---

### P3 검증 체크리스트

- [ ] Fire Grimoire가 게임 시작 시 자동 장착됨
- [ ] 우클릭(또는 K키)으로 Fire Magic 발동
- [ ] Fire Magic이 화염 이펙트와 함께 재생
- [ ] Fire Magic이 적에게 25 데미지 적용 (기본 공격의 2.5배)
- [ ] 쿨다운 UI가 7초 카운트다운 표시
- [ ] 쿨다운 중 Fire Magic 재사용 불가
- [ ] Fire Magic 사운드 재생

---

## 통합 및 테스트

### 통합 테스트 시나리오

#### 시나리오 1: Full Playthrough (5분 목표)

1. **레벨 시작**
   - [x] 레벨 로드 시간 3초 이내
   - [x] "Press Space to Start" UI 표시
   - [x] Space 입력 시 게임 시작

2. **이동 및 탐험**
   - [x] 플랫폼 3-4개 점프하며 이동
   - [x] 점프 반응성 확인 (홀드 시간에 따른 높이 조절)
   - [x] 모든 입력 100ms 이내 반응

3. **첫 번째 적 조우**
   - [x] 적이 플레이어 감지 (5m 이내)
   - [x] 적이 플레이어 추적
   - [x] 좌클릭으로 기본 공격 3회
   - [x] 적 사망 및 제거

4. **두 번째 적 조우 (Fire Magic 사용)**
   - [x] 적 접근
   - [x] 우클릭으로 Fire Magic 발동
   - [x] 화염 이펙트 및 사운드 재생
   - [x] 적이 큰 데미지로 빠르게 사망
   - [x] 쿨다운 UI 표시

5. **적에게 피격**
   - [x] 적의 공격이 플레이어에게 적중
   - [x] 피격 피드백 (점멸, 사운드)
   - [x] 체력 감소 (예: 100 → 95)

6. **레벨 완료**
   - [x] Level Goal 도달
   - [x] "Level Complete!" UI 표시
   - [x] 레벨 완료 사운드 재생

**예상 플레이 시간**: 3-5분

---

#### 시나리오 2: Death & Restart

1. **적에게 반복 피격**
   - [x] 적의 공격을 회피하지 않고 대기
   - [x] 체력 점진적 감소 (100 → 0)
   - [x] 각 피격 시 피드백 제공

2. **사망**
   - [x] 체력 0 도달
   - [x] 사망 애니메이션 재생 (옵션)
   - [x] "You Died!" UI 표시
   - [x] 사망 사운드 재생

3. **재시작**
   - [x] R 키 또는 2초 후 자동 재시작
   - [x] 레벨 초기화 (적 부활, 플레이어 체력 회복)
   - [x] Spawn Point에서 재시작

---

#### 시나리오 3: Fire Magic Cooldown

1. **Fire Magic 사용**
   - [x] 우클릭으로 Fire Magic 발동
   - [x] 쿨다운 UI 표시 (7초)

2. **쿨다운 중 재사용 시도**
   - [x] 우클릭 입력 무시
   - [x] 쿨다운 UI 계속 표시

3. **쿨다운 완료**
   - [x] 7초 경과 후 쿨다운 UI 사라짐
   - [x] 우클릭으로 Fire Magic 재사용 가능

---

### 성능 테스트

- [ ] 60 FPS 유지 (적 5마리 동시 활성)
- [ ] 레벨 로드 시간 3초 이내
- [ ] 입력 지연 100ms 이내
- [ ] 파티클 시스템 최적화 (동시 5개 이하)

---

### 버그 체크리스트

- [ ] 플랫폼 충돌 이슈 없음 (끼임, 관통 등)
- [ ] 적 AI 무한 루프 없음
- [ ] Fire Magic 다중 발동 방지 (쿨다운 중복 사용)
- [ ] 사망 후 입력 비활성화 확인
- [ ] 레벨 재시작 시 모든 상태 초기화

---

## 기술적 고려사항 및 리스크

### 고려사항

#### 1. CharacterPhysics 통합

**현황**:
- 기존 CharacterPhysics는 Skull System과 연동되어 SkullMovementProfile을 사용합니다.
- Grimoire System으로 리팩토링 시 SkullMovementProfile → GrimoireMovementProfile로 변경 필요합니다.

**해결책**:
- P1 단계에서는 기본 프로필 사용 (DefaultProfile)
- P3 단계에서 Fire Grimoire에 맞는 프로필 생성 (예: 이동 속도 약간 증가)

#### 2. GAS Core 확장

**현황**:
- 기존 Ability 클래스는 범용적이지만 2D 플랫포머 특화 기능이 부족합니다.

**해결책**:
- BasicMagicAttack, FireMagicAbility는 Ability를 상속하되 2D 전용 로직 추가
- 타겟팅: 3D Physics.OverlapSphere → 2D Physics2D.OverlapCircle 사용

#### 3. Grimoire vs Skull Refactoring

**리스크**:
- 기존 Skull System 코드가 많아 리팩토링 시 버그 발생 가능
- 한글 주석이 깨질 수 있음

**해결책**:
- Edit 도구를 사용하여 점진적 리팩토링
- 각 파일 수정 후 즉시 테스트
- 한글 주석은 UTF-8 인코딩 유지 (.editorconfig 확인)

#### 4. Enemy AI 최적화

**리스크**:
- 적이 많아질 경우 FSM 업데이트 부하 증가

**해결책**:
- MVP에서는 최대 3-5마리로 제한
- EnemyController의 주기적 감지(`detectionInterval`)를 0.2-0.3초로 설정

#### 5. Visual Feedback 구현 난이도

**리스크**:
- 파티클, 애니메이션, 사운드 통합 작업이 시간 소모적

**해결책**:
- P2에서는 최소한의 피드백만 구현 (스프라이트 점멸)
- P3에서 Fire Magic 이펙트에 집중
- 플레이스홀더 에셋 사용 (유니티 기본 파티클)

---

### 리스크 매트릭스

| 리스크 | 확률 | 영향도 | 완화 전략 |
|--------|------|--------|-----------|
| CharacterPhysics 통합 문제 | 중 | 높음 | 기본 프로필로 우선 진행, 문제 발생 시 디버깅 |
| Grimoire 리팩토링 버그 | 높음 | 중 | 점진적 리팩토링, 각 단계마다 테스트 |
| Enemy AI 성능 이슈 | 낮음 | 중 | 적 수 제한, 감지 주기 조정 |
| Fire Magic 이펙트 복잡도 | 중 | 낮음 | 플레이스홀더 사용, 필요 시 단순화 |
| 입력 지연 | 낮음 | 높음 | Update/FixedUpdate 주기 확인, 최적화 |

---

## 의존성 맵

### Phase별 의존성

```
Phase 1 (P1): Basic Level Traversal
├── MageController
│   ├── CharacterPhysics (기존)
│   └── MageInputHandler (신규)
├── LevelGoalTrigger (신규)
├── LevelManager (신규)
│   └── GameFlowManager (기존)
└── LevelUI (신규)

Phase 2 (P2): Combat System
├── BasicMagicAttack (신규)
│   ├── GAS Core (기존)
│   └── DamageSystem (기존)
├── EnemyController (기존)
│   ├── HealthSystem (기존)
│   └── FSM Core (기존)
├── MageController (확장)
│   └── HealthSystem (기존)
├── MageVisualFeedback (신규)
└── EnemyAbilityTarget (신규)

Phase 3 (P3): Grimoire Magic
├── GrimoireSystem (리팩토링)
│   ├── GrimoireManager (리팩토링)
│   └── BaseGrimoire (리팩토링)
├── FireGrimoire (신규)
│   └── FireMagicAbility (신규)
│       └── GAS Core (기존)
├── MageController (확장)
│   └── GrimoireManager
└── AbilityCooldownUI (신규)
```

---

## 파일 구조

### 최종 폴더 구조

```
Assets/
├── _Project/
│   ├── Scenes/
│   │   └── Levels/
│   │       └── Level_001_FirstPlayable.unity
│   │
│   ├── Scripts/
│   │   ├── Gameplay/
│   │   │   ├── Player/
│   │   │   │   ├── MageController.cs              [P1, P2, P3 확장]
│   │   │   │   ├── MageVisualFeedback.cs          [P2]
│   │   │   │   ├── Input/
│   │   │   │   │   └── MageInputHandler.cs        [P1]
│   │   │   │   └── Abilities/
│   │   │   │       └── BasicMagicAttack.cs        [P2]
│   │   │   │
│   │   │   ├── Grimoire/                          [P3 리팩토링]
│   │   │   │   ├── Core/
│   │   │   │   │   ├── GrimoireSystem.cs
│   │   │   │   │   ├── GrimoireManager.cs
│   │   │   │   │   └── BaseGrimoire.cs
│   │   │   │   ├── Types/
│   │   │   │   │   └── FireGrimoire.cs            [P3]
│   │   │   │   └── Abilities/
│   │   │   │       └── FireMagicAbility.cs        [P3]
│   │   │   │
│   │   │   ├── Enemy/                             [기존, P2 활용]
│   │   │   │   ├── EnemyController.cs
│   │   │   │   ├── EnemyAbilityTarget.cs          [P2]
│   │   │   │   └── States/
│   │   │   │       ├── EnemyIdleState.cs
│   │   │   │       ├── EnemyPatrolState.cs
│   │   │   │       ├── EnemyTraceState.cs
│   │   │   │       ├── EnemyAttackState.cs
│   │   │   │       ├── EnemyHitState.cs
│   │   │   │       └── EnemyDeathState.cs
│   │   │   │
│   │   │   ├── Level/
│   │   │   │   ├── LevelManager.cs                [P1]
│   │   │   │   └── LevelGoalTrigger.cs            [P1]
│   │   │   │
│   │   │   └── Common/
│   │   │       ├── GrimoireData.cs                [P3 리팩토링]
│   │   │       └── IGrimoireController.cs         [P3 리팩토링]
│   │   │
│   │   └── UI/
│   │       ├── Level/
│   │       │   └── LevelUI.cs                     [P1]
│   │       └── HUD/
│   │           └── AbilityCooldownUI.cs           [P3]
│   │
│   ├── Prefabs/
│   │   ├── Player/
│   │   │   └── Mage.prefab                        [P1, P2, P3 확장]
│   │   ├── Enemies/
│   │   │   └── BasicEnemy.prefab                  [P2]
│   │   ├── Grimoire/
│   │   │   └── FireGrimoire.prefab                [P3]
│   │   └── VFX/
│   │       ├── FireProjectile.prefab              [P3 옵션]
│   │       └── FireImpact.prefab                  [P3]
│   │
│   ├── Data/
│   │   ├── Grimoire/
│   │   │   ├── FireGrimoireData.asset             [P3]
│   │   │   └── FireGrimoireMovementProfile.asset  [P3 옵션]
│   │   ├── Abilities/
│   │   │   ├── BasicMagicAttackData.asset         [P2]
│   │   │   └── FireMagicAbilityData.asset         [P3]
│   │   └── Enemies/
│   │       └── BasicEnemyData.asset               [P2]
│   │
│   └── Art/
│       ├── Sprites/
│       │   ├── Mage/
│       │   │   └── mage_idle.png                  [플레이스홀더]
│       │   ├── Enemies/
│       │   │   └── basic_enemy.png                [플레이스홀더]
│       │   └── UI/
│       │       └── goal_icon.png
│       └── Particles/
│           └── FireMagic.prefab
│
└── Plugins/                                       [기존 시스템]
    ├── GAS_Core/
    ├── FSM_Core/
    └── FSM_GAS_Integration/
```

---

## 다음 단계

### 즉시 시작 가능한 작업 (P1)

1. **MageController 스크립트 작성** (2-3시간)
   - CharacterPhysics 통합
   - 기본 이동/점프 로직 구현

2. **MageInputHandler 스크립트 작성** (1-2시간)
   - 입력 이벤트 시스템 구현

3. **Level Scene 구성** (2-3시간)
   - 플랫폼 배치
   - Mage Prefab 생성
   - Level Goal 배치

### 예상 개발 일정

| 단계 | 작업 기간 | 완료 기준 |
|------|----------|-----------|
| **Phase 1 (P1)** | 2-3일 | 레벨 완주 가능 |
| **Phase 2 (P2)** | 3-4일 | 전투 시스템 동작 |
| **Phase 3 (P3)** | 2-3일 | Fire Magic 사용 가능 |
| **통합 테스트** | 1-2일 | 모든 Success Criteria 통과 |
| **버그 수정** | 1-2일 | 안정화 |
| **총 예상** | **9-14일** | MVP 완성 |

---

## 결론

본 구현 계획은 **단계적, 점진적 개발**을 통해 각 Phase마다 플레이 가능한 상태를 유지하면서 First Playable Level MVP를 완성하는 것을 목표로 합니다.

### 핵심 원칙

1. **완성 우선**: 완벽한 시스템보다 플레이 가능한 게임을 먼저
2. **기존 시스템 활용**: CharacterPhysics, GAS, FSM, HealthSystem 최대한 재사용
3. **Grimoire 차별화**: Skull System을 Grimoire System으로 재명명하여 마법사 테마에 맞게 조정
4. **단계별 검증**: 각 Phase 완료 후 즉시 테스트하여 문제 조기 발견

### 성공 측정

- [ ] 플레이어가 5분 내 레벨 완주
- [ ] 기본 공격으로 적 처치 가능
- [ ] Fire Magic으로 강력한 데미지 적용
- [ ] 모든 액션에 0.2초 이내 피드백
- [ ] 첫 플레이어의 70% 이상이 3회 시도 내 완료

**다음 작업**: Phase 1 구현 시작 - MageController 작성

---

**Document Version**: 1.0
**Last Updated**: 2025-11-01
**Author**: Claude Code (Game Design Specialist)
