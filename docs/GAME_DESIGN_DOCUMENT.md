# GASPT 게임 디자인 문서 (GDD)
**Game Design Document for GASPT Project**

---

## 문서 정보

| 항목 | 내용 |
|-----|------|
| **프로젝트명** | GASPT (Generic Ability System + FSM for Unity) |
| **장르** | 2D 액션 플랫포머 (Skul 스타일) |
| **플랫폼** | PC (Windows) |
| **엔진** | Unity 6.0+ |
| **개발 언어** | C# |
| **타겟 플레이어** | 중급 게이머 (액션 게임 경험자) |
| **세션 길이** | 15-30분 (짧은 세션) |
| **작성일** | 2025-10-29 |
| **버전** | 1.0 |

---

## 0. 프로젝트 헌법 및 기본 원칙

이 Game Design Document는 **GASPT Constitution v1.1.0**을 기반으로 작성되었습니다.

**필수 참조 문서**: `.specify/memory/constitution.md`

### 0.1 9가지 핵심 원칙

모든 설계 결정은 다음 9가지 핵심 원칙을 따릅니다:

1. **Completion-First Development (완성 우선)**
   - 완벽한 시스템보다 플레이 가능한 게임을 먼저
   - 기능 추가보다 기존 기능의 완성도에 집중
   - 우선 완성 후 점진적 확장

2. **Incremental Development & Testing (단계적 개발)**
   - 작은 단위로 나누어 개발하고 지속적으로 테스트
   - 각 단계마다 플레이 가능한 상태 유지
   - 기반 시스템 먼저, 콘텐츠는 나중에

3. **Productivity-First Architecture (생산성 우선 설계)**
   - 시스템 설계에 충분한 시간 투자 (초기 생산성을 위함)
   - 재사용 가능한 컴포넌트 중심 설계
   - 코드 중복 최소화 및 모듈화

4. **Player Experience Priority (플레이어 경험 우선)**
   - 복잡한 시스템보다 직관적이고 재미있는 게임플레이
   - 중급 게이머를 타겟으로 한 적절한 난이도
   - 짧은 세션 플레이에 최적화

5. **Code Design Standards (코드 설계 표준)**
   - OOP, SOLID 원칙 준수
   - 명확한 인터페이스와 추상화
   - 의존성 주입 및 느슨한 결합

6. **Async Pattern Enforcement (비동기 패턴 강제)** ⚠️ **NON-NEGOTIABLE**
   - **Coroutine 절대 금지** - Unity의 Awaitable 패턴만 사용
   - 모든 비동기 작업은 async/await로 구현
   - 예외 없이 준수 (레거시 코드 포함)

7. **Localization & Encoding (현지화 및 인코딩)**
   - 한글 주석 허용 (UTF-8 인코딩)
   - 변수명은 영어 camelCase (언더스코어 금지)
   - 코드 설명은 한글로 작성 가능

8. **Token Efficiency (토큰 효율성)**
   - 작은 수정: Edit/MultiEdit 사용
   - 큰 변경: 전체 파일 재작성 고려
   - 한글 주석 보존에 주의

9. **Unity 6.0+ Compatibility (최신 API 사용)**
   - `velocity` → `linearVelocity`
   - `FindObjectOfType` → `FindAnyObjectByType`
   - 더 이상 사용되지 않는 API 사용 금지

### 0.2 이 문서의 역할

이 GDD는 위 9가지 원칙을 구체적인 게임 시스템 설계로 구현한 문서입니다. 모든 시스템 설계, 기술 결정, 개발 로드맵은 Constitution의 원칙을 준수합니다.

**특히 Principle VI (비동기 패턴)는 절대 예외 없이 준수됩니다.**

---

## 목차

0. [프로젝트 헌법 및 기본 원칙](#0-프로젝트-헌법-및-기본-원칙)
1. [프로젝트 개요](#1-프로젝트-개요)
2. [게임 컨셉](#2-게임-컨셉)
3. [핵심 시스템 기획](#3-핵심-시스템-기획)
4. [게임플레이 메커니즘](#4-게임플레이-메커니즘)
5. [기술 아키텍처](#5-기술-아키텍처)
6. [데이터 구조](#6-데이터-구조)
7. [UI/UX 설계](#7-uiux-설계)
8. [적 AI 설계](#8-적-ai-설계)
9. [레벨 디자인](#9-레벨-디자인)
10. [사운드 및 비주얼](#10-사운드-및-비주얼)
11. [개발 로드맵](#11-개발-로드맵)
12. [기술 제약사항](#12-기술-제약사항)

---

## 1. 프로젝트 개요

### 1.1 프로젝트 목표

GASPT는 **범용 게임플레이 어빌리티 시스템(GAS)**과 **유한상태머신(FSM)**을 결합한 **2D 액션 플랫포머 프레임워크**입니다. Skul: The Hero Slayer의 핵심 메커니즘(스컬 교체, 유동적인 전투)에서 영감을 받아, 재사용 가능한 게임 시스템을 구축하는 것이 목표입니다.

### 1.2 핵심 가치

1. **완성 우선 원칙**: 복잡한 시스템보다 플레이 가능한 게임을 먼저 완성
2. **모듈화 설계**: 각 시스템이 독립적으로 동작하며 재사용 가능
3. **확장성**: 새로운 스컬, 어빌리티, 적을 쉽게 추가 가능
4. **명확한 피드백**: 플레이어 행동에 즉각적인 반응 제공

### 1.3 게임 특징

- **스컬 교체 시스템**: 실시간으로 캐릭터(스컬)를 교체하며 전투 스타일 변경
- **유동적인 전투**: 콤보 체이닝, 대시, 벽 점프로 역동적인 액션
- **커스텀 물리**: Transform 기반 즉시 반응형 물리 시스템
- **데이터 중심 설계**: ScriptableObject로 모든 게임 데이터 관리

---

## 2. 게임 컨셉

### 2.1 게임 세계관

플레이어는 **다양한 스컬(해골 전사)**을 교체하며 적의 던전을 탐험합니다. 각 스컬은 고유한 능력과 전투 스타일을 가지고 있으며, 상황에 맞게 스컬을 교체하여 적을 물리쳐야 합니다.

### 2.2 플레이어 목표

1. **단기 목표**: 현재 스테이지의 모든 적 처치 및 보스 격파
2. **중기 목표**: 새로운 스컬 획득 및 강화
3. **장기 목표**: 던전의 모든 스테이지 클리어

### 2.3 핵심 게임플레이 루프

```
전투 시작
    ↓
[1] 적 패턴 파악
    ↓
[2] 최적의 스컬 선택
    ↓
[3] 콤보 공격 + 회피
    ↓
[4] 스컬 교체 (상황 대응)
    ↓
[5] 적 처치
    ↓
[6] 보상 획득 (새 스컬/업그레이드)
    ↓
다음 전투로
```

### 2.4 플레이어 경험 목표

- **숙련도 성장**: 처음에는 서툴지만, 연습하면 화려한 콤보 가능
- **전략적 선택**: 적 타입에 따라 최적의 스컬 선택
- **즉각적 만족**: 공격 적중 시 명확한 피드백(히트스톱, 사운드, VFX)
- **리듬감**: 음악과 동기화된 전투 흐름

---

## 3. 핵심 시스템 기획

### 3.1 GAS (Generic Ability System)

#### 3.1.1 시스템 목적
모든 플레이어/적 행동을 **어빌리티**로 추상화하여 일관된 방식으로 관리합니다.

#### 3.1.2 핵심 구성요소

##### **AbilitySystem (Manager)**
- 어빌리티 등록/제거
- 실행 가능 여부 검증 (쿨다운, 리소스)
- 리소스 관리 (MP, 스태미나)
- 콤보 체이닝 처리

##### **Ability (Executor)**
- 실제 어빌리티 실행 로직
- 상태 머신 (Idle → Casting → Active → Cooldown)
- 비동기 실행 (`async Awaitable`)

##### **AbilityData (ScriptableObject)**
- 어빌리티 메타데이터 (ID, 이름, 아이콘)
- 쿨다운, 리소스 코스트
- 콤보 체인 정보 (NextAbilityId, ChainWindowDuration)

#### 3.1.3 어빌리티 카테고리

| 카테고리 | 설명 | 예시 |
|---------|------|------|
| **공격** | 데미지를 주는 어빌리티 | PlayerAttack_1/2/3 (콤보) |
| **이동** | 위치 변경 | Dash, Teleport |
| **방어** | 데미지 감소/회피 | Shield, Dodge |
| **회복** | 체력/MP 회복 | Heal, Meditation |
| **버프** | 능력치 강화 | Berserk, Haste |
| **스컬 고유** | 특정 스컬 전용 | WarriorSlash, MageFireball |

#### 3.1.4 콤보 체이닝 메커니즘

**기본 원리**:
1. 어빌리티 A 실행 완료 → "NextAbilityId"가 있으면 체인 준비
2. ChainWindowDuration(0.5초) 내에 입력 → 어빌리티 B 실행
3. 타임아웃 또는 체인 종료 → 처음부터 다시

**예시**: 플레이어 기본 공격 3단 콤보
```
[좌클릭 #1]
PlayerAttack_1 실행
    ↓ (NextAbilityId = "PlayerAttack_2", 0.5초 윈도우)
[0.5초 내 좌클릭 #2]
PlayerAttack_2 실행
    ↓ (NextAbilityId = "PlayerAttack_3", 0.5초 윈도우)
[0.5초 내 좌클릭 #3]
PlayerAttack_3 실행
    ↓ (NextAbilityId = "", 체인 종료)
[좌클릭 #4]
PlayerAttack_1 실행 (처음부터)
```

#### 3.1.5 리소스 시스템

**리소스 타입**:
- **MP (Mana Points)**: 스킬 사용 비용
- **Stamina**: 대시, 회피 비용
- **Custom**: 스컬별 고유 리소스 (예: 분노, 영혼)

**리소스 관리**:
```csharp
public bool CanUseAbility(string abilityId)
{
    var ability = GetAbility(abilityId);

    // 쿨다운 체크
    if (cooldowns.ContainsKey(abilityId))
        return false;

    // 리소스 체크
    foreach (var cost in ability.Data.ResourceCosts)
    {
        if (resources[cost.Key] < cost.Value)
            return false;
    }

    return true;
}
```

---

### 3.2 FSM (Finite State Machine)

#### 3.2.1 시스템 목적
복잡한 상태 로직을 **상태 클래스**로 분리하여 유지보수성을 높입니다.

#### 3.2.2 핵심 구성요소

##### **StateMachine**
- 상태 등록/제거
- 전환 규칙 관리
- 동기/비동기 전환 지원

##### **State**
- 진입/갱신/종료 로직
- 조건부 전환 체크

##### **Transition**
- From → To 전환 규칙
- 조건 (Condition) 평가
- 우선순위 기반 정렬

#### 3.2.3 FSM 활용 사례

##### **1. GameFlowManager (게임 흐름)**
```
Preload (리소스 로딩)
    ↓
Main (메인 메뉴)
    ↓ [시작 버튼]
Loading (씬 로딩)
    ↓
Ingame (게임플레이)
    ⇄ [ESC 키]
Pause (일시정지)
```

##### **2. PlayerController (플레이어 행동)**
```
Idle (대기)
    ├─ [WASD] → Move (이동)
    ├─ [Space] → Jump (점프) → Fall (낙하)
    ├─ [Shift] → Dash (대시)
    ├─ [좌클릭] → Attack (공격) → [콤보] → Attack
    └─ [피격] → Hit (피격) → [체력 0] → Dead (사망)
```

##### **3. EnemyController (적 AI)**
```
Idle (대기)
    ↓ [플레이어 감지]
Patrol (정찰)
    ↓ [플레이어 감지]
Chase (추적)
    ↓ [공격 거리]
Attack (공격)
    ↓ [피격]
Hit (피격)
    ↓ [체력 0]
Death (사망)
```

#### 3.2.4 동기 vs 비동기 전환

**동기 전환** (Combat 상황):
```csharp
// 즉시 전환 필요 (피격, 공격)
stateMachine.ForceTransitionTo("Hit");
```

**비동기 전환** (씬 로딩):
```csharp
// 페이드 아웃 → 씬 로드 → 페이드 인
await stateMachine.ForceTransitionToAsync("Loading");
```

---

### 3.3 CharacterPhysics 시스템

#### 3.3.1 시스템 철학
**"Skul 스타일 즉시 반응형 물리"**: 입력 즉시 캐릭터가 반응하며, 현실적인 물리보다 **플레이어 의도 존중**.

#### 3.3.2 핵심 특징

1. **Transform 기반 제어**: Physics2D 최소화
2. **즉시 가속**: 입력 시 바로 최대 속도 (관성 없음)
3. **커스텀 중력**: 상황별 중력 배율 (하강 시 강화)
4. **3단계 점프 안전장치**:
   - 코요테 타임 (Coyote Time): 0.15초
   - 점프 버퍼 (Jump Buffer): 0.2초
   - 가변 점프 높이 (Variable Jump Height)

#### 3.3.3 물리 파라미터

| 파라미터 | 기본값 | 설명 |
|---------|-------|------|
| **moveSpeed** | 5 m/s | 지면 이동 속도 |
| **airMoveSpeed** | 4 m/s | 공중 이동 속도 |
| **jumpPower** | 10 m/s | 점프 초기 속도 |
| **gravity** | 20 m/s² | 기본 중력 |
| **fallGravityMultiplier** | 1.5× | 하강 시 중력 배율 |
| **maxFallSpeed** | 20 m/s | 최대 낙하 속도 |
| **dashSpeed** | 15 m/s | 대시 속도 |
| **dashDuration** | 0.2초 | 대시 지속 시간 |
| **dashCooldown** | 1초 | 대시 쿨다운 |
| **maxAirDashes** | 1 | 공중 대시 횟수 |
| **coyoteTime** | 0.15초 | 발이 떨어진 후 점프 가능 시간 |
| **jumpBufferTime** | 0.2초 | 점프 입력 버퍼 시간 |

#### 3.3.4 벽 점프/슬라이딩 (US1)

**벽 감지**:
- BoxCast로 좌우 0.1유닛 거리에서 벽 감지
- Ground Layer와 Wall Layer 구분

**벽 슬라이딩**:
- 벽에 닿고 + 하강 중 + 공중 → 슬라이딩 시작
- 슬라이딩 속도: 일반 낙하의 30% 이하

**벽 점프**:
- 벽 슬라이딩 중 점프 입력 → 반대 방향으로 점프
- 수평 속도: 120% (벽에서 멀어지는 힘)
- 수직 속도: 85% (일반 점프보다 낮음)

#### 3.3.5 낙하 플랫폼 (US2)

**OneWayPlatform 메커니즘**:
- 위에서만 착지 가능 (아래에서는 통과)
- 아래 방향 + 점프 입력 → 플랫폼 통과
- Physics2D.IgnoreCollision으로 0.5초간 충돌 무시
- 쿨다운 후 자동으로 충돌 재활성화

#### 3.3.6 스컬별 이동 특성 (US3)

**SkullMovementProfile (ScriptableObject)**:
- **moveSpeedMultiplier**: 이동 속도 배율 (0.1 ~ 3.0)
- **jumpHeightMultiplier**: 점프 높이 배율
- **airControlMultiplier**: 공중 제어력 배율
- **wallJumpHorizontalMultiplier**: 벽 점프 수평 배율
- **wallJumpVerticalMultiplier**: 벽 점프 수직 배율

**예시 프로필**:

| 스컬 | 이동 | 점프 | 공중 제어 | 특징 |
|-----|------|------|---------|------|
| **Default** | 1.0 | 1.0 | 1.0 | 균형잡힌 기본 |
| **Warrior** | 0.9 | 0.85 | 0.8 | 느리지만 강력 |
| **Mage** | 1.15 | 1.1 | 1.25 | 빠르고 기민 |

---

### 3.4 Combat 시스템

#### 3.4.1 데미지 처리 흐름

```
공격자 (Attacker)
    ↓
HitboxController.CreateHitbox()
    ↓
DamageSystem.ApplyBoxDamage()
    ├─ Physics2D.OverlapBoxAll() (타겟 탐색)
    └─ foreach target:
        ↓
        HealthSystem.TakeDamage(DamageData)
            ├─ 무적 체크
            ├─ 데미지 계산 (크리티컬, 타입별 배율)
            ├─ 체력 감소
            ├─ OnDamaged 이벤트
            ├─ [체력 0] OnDeath 이벤트
            └─ 무적 시간 시작 (0.3초)
```

#### 3.4.2 DamageData 구조

```csharp
public struct DamageData
{
    public float amount;              // 데미지 양
    public DamageType damageType;     // Physical, Magical, True
    public GameObject source;         // 공격자
    public Vector2 knockback;         // 넉백 벡터
    public bool canCritical;          // 크리티컬 가능
    public bool ignoreInvincibility;  // 무적 무시
    public float criticalMultiplier;  // 크리티컬 배율 (기본 1.5)
}
```

#### 3.4.3 DamageType 시스템

| 타입 | 설명 | 방어 가능 | 예시 |
|-----|------|---------|------|
| **Physical** | 물리 공격 | 방어력으로 감소 | 칼, 화살 |
| **Magical** | 마법 공격 | 마법 저항으로 감소 | 파이어볼, 라이트닝 |
| **True** | 고정 데미지 | 감소 불가 | 독, 낙하 |
| **Pure** | 순수 데미지 | 무적 무시 | 스크립트 이벤트 |

#### 3.4.4 Hitbox vs Hurtbox

**Hitbox (공격 판정)**:
- 공격자가 생성
- DamageData 포함
- 0.1~0.3초 지속 후 자동 파괴

**Hurtbox (피격 판정)**:
- 피격자에 부착
- 항상 활성화 (무적 시 비활성화)

**충돌 처리**:
```csharp
void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Hurtbox"))
    {
        var target = other.GetComponentInParent<HealthSystem>();
        DamageSystem.ApplyDamage(target.gameObject, damageData);
    }
}
```

#### 3.4.5 ComboSystem

**콤보 카운트**:
- 첫 공격: 콤보 시작 (comboIndex = 0)
- 연속 공격 (0.5초 내): 콤보 진행 (comboIndex++)
- 타임아웃: 콤보 리셋

**콤보 이벤트**:
```csharp
public event Action OnComboStarted;        // 콤보 시작
public event Action<int> OnComboAdvanced;  // 콤보 진행 (인덱스)
public event Action OnComboCompleted;      // 최대 콤보 도달
public event Action OnComboReset;          // 콤보 리셋
```

**콤보 데이터**:
```csharp
[CreateAssetMenu]
public class ComboData : ScriptableObject
{
    public int comboIndex;           // 콤보 인덱스 (0, 1, 2)
    public float damage;             // 데미지
    public float duration;           // 공격 지속 시간
    public Vector2 hitboxSize;       // 히트박스 크기
    public Vector2 hitboxOffset;     // 히트박스 오프셋
    public string animationName;     // 애니메이션 이름
}
```

---

### 3.5 Skull 시스템

#### 3.5.1 시스템 목적
플레이어가 **실시간으로 스컬(캐릭터 클래스)을 교체**하여 전투 스타일을 변경합니다.

#### 3.5.2 Skull 구조

**ISkullController (인터페이스)**:
```csharp
public interface ISkullController
{
    SkullType SkullType { get; }
    SkullData SkullData { get; }
    bool IsActive { get; }

    Awaitable ActivateAsync();         // 스컬 활성화
    Awaitable DeactivateAsync();       // 스컬 비활성화
    Awaitable PerformPrimaryAttack();  // 기본 공격
    Awaitable PerformSecondaryAttack();// 보조 공격
    Awaitable PerformUltimate();       // 궁극기
    Awaitable PerformSkullThrow(Vector2 direction); // 스컬 던지기

    SkullStatus GetStatus();           // 상태 (준비 여부, 쿨다운)
}
```

#### 3.5.3 Skull 카테고리

| 스컬 | 타입 | 특징 | 기본 공격 | 궁극기 |
|-----|------|------|---------|-------|
| **Default** | 균형형 | 모든 능력 평균 | 근접 3단 콤보 | 광역 회전 공격 |
| **Warrior** | 근접형 | 높은 공격력, 낮은 이동속도 | 강력한 한방 | 돌진 베기 |
| **Mage** | 원거리형 | 높은 이동속도, 낮은 방어력 | 파이어볼 | 메테오 |
| **Assassin** | 기동형 | 빠른 이동, 크리티컬 특화 | 빠른 연타 | 그림자 분신 |
| **Tank** | 방어형 | 높은 체력, 느린 이동 | 방패 강타 | 도발 |

#### 3.5.4 Skull 교체 흐름

```
[Q 키 입력]
SkullSystem.SwitchToNextSkull()
    ↓
SkullManager.SwitchToNextSlot()
    ├─ [1] CurrentSkull.DeactivateAsync()
    │   ├─ UnregisterAbilities() (GAS에서 어빌리티 제거)
    │   └─ PlayDeactivateAnimation() (교체 애니메이션)
    │
    ├─ [2] OnSkullChanged 이벤트 발생
    │
    ├─ [3] NewSkull.ActivateAsync()
    │   ├─ RegisterAbilities() (GAS에 어빌리티 추가)
    │   ├─ ApplySkullStats() (능력치 적용)
    │   └─ PlayActivateAnimation() (등장 애니메이션)
    │
    └─ [4] CharacterPhysics.ApplySkullProfile()
        ├─ 이동 속도 배율 적용
        ├─ 점프 높이 배율 적용
        └─ 공중 제어력 배율 적용
```

#### 3.5.5 Skull 획득 방법

1. **드롭**: 적 처치 시 확률로 획득
2. **보상**: 보스 처치 후 선택
3. **상점**: 골드로 구매
4. **퀘스트**: 특정 조건 달성

#### 3.5.6 Skull 강화 시스템 (향후 구현)

- **레벨업**: 경험치로 능력치 상승
- **스킬 트리**: 새로운 어빌리티 해금
- **진화**: 상위 스컬로 변환

#### 3.5.7 Skull 시스템 구현 전제조건

**Phase 3 시작 전 필수 준비사항**:

Skull 시스템은 GAS, FSM, CharacterPhysics 3개 핵심 시스템과 통합되므로, 다음 사항을 반드시 준비해야 합니다:

- [ ] **CharacterPhysics.ApplySkullProfile() 구현**
  - Skull별 이동 속도, 점프 높이, 공중 제어력 배율 적용 기능
  - 실시간 프로필 교체 지원

- [ ] **AbilitySystem.RegisterAbilities()/UnregisterAbilities() 테스트**
  - 런타임에 어빌리티 동적 추가/제거 가능 확인
  - 어빌리티 ID 충돌 방지 검증

- [ ] **PlayerController FSM에 SkullSwitch 상태 추가**
  - Skull 교체 중 다른 입력 차단
  - 교체 애니메이션 재생 상태

- [ ] **UI에 SkullIcon 표시 기능 추가**
  - HUD에 현재 활성 Skull 아이콘 표시
  - Q/E 키로 교체 가능한 Skull 미리보기

**⚠️ Skull 없이도 플레이 가능 설계 (완성 우선 원칙)**:

GASPT는 "완성 우선" 원칙에 따라, **Skull 시스템이 없어도 게임이 플레이 가능**해야 합니다:

1. **DefaultProfile로 단독 동작 보장**
   - CharacterPhysics는 Skull 시스템 없이도 DefaultProfile로 동작
   - 기본 이동/점프 파라미터가 하드코딩되지 않음

2. **기본 어빌리티는 Skull 독립적**
   - 공격, 점프, 대시 등 기본 어빌리티는 항상 사용 가능
   - Skull 교체 시 추가 어빌리티만 등록/해제

3. **Skull 시스템은 "확장 기능"**
   - Phase 2까지는 Skull 없이 플레이 가능한 버전 완성
   - Phase 3에서 Skull 추가 시 기존 시스템 수정 최소화
   - DefaultSkull 구현 후 → WarriorSkull, MageSkull 순차 추가

**구현 우선순위**:
1. **Phase 2 완료**: Skull 없이 전투/이동 완벽 동작
2. **Phase 3 시작**: DefaultSkull 최소 구현
3. **Phase 3 중반**: WarriorSkull, MageSkull 추가
4. **Phase 3 후반**: AssassinSkull, TankSkull 추가

이 접근 방식은 각 단계마다 플레이 가능한 상태를 유지하며, Skull 시스템이 실패하더라도 게임 자체는 완성할 수 있도록 합니다.

---

## 4. 게임플레이 메커니즘

### 4.1 플레이어 조작

#### 4.1.1 입력 매핑

| 입력 | 행동 | 설명 |
|-----|------|------|
| **WASD** | 이동 | 4방향 이동 |
| **Space** | 점프 | 단일 점프 (공중 불가) |
| **LShift** | 대시 | 짧은 거리 고속 이동 |
| **좌클릭** | 기본 공격 | 연타 시 3단 콤보 |
| **우클릭** | 보조 공격 | 스컬별 다름 |
| **R** | 궁극기 | 스컬별 다름 |
| **Q/E** | 스컬 교체 | 이전/다음 스컬 |
| **ESC** | 일시정지 | Pause 메뉴 |

#### 4.1.2 이동 메커니즘

**지면 이동**:
- 입력 즉시 최대 속도 (관성 없음)
- 입력 해제 즉시 정지

**공중 이동**:
- 지면보다 약간 느림 (airMoveSpeed = 80%)
- 공중 제어 가능

**대시**:
- 0.2초간 고속 이동
- 1초 쿨다운
- 공중 대시 1회 허용
- 무적 프레임 없음 (순수 이동 기술)

#### 4.1.3 점프 메커니즘

**코요테 타임** (발이 떨어진 후 점프 가능):
```
플레이어가 바닥에서 걸어 나감
    ↓
isGrounded = false
    ↓
coyoteTimeCounter = 0.15초
    ↓
[0.15초 내 점프 입력] → 점프 실행 ✅
[0.15초 초과] → 점프 불가 ❌
```

**점프 버퍼** (점프 입력 미리 저장):
```
공중에서 점프 입력
    ↓
jumpBufferCounter = 0.2초
    ↓
[0.2초 내 착지] → 즉시 점프 실행 ✅
[0.2초 초과] → 버퍼 만료 ❌
```

**가변 점프 높이**:
- 점프 키 길게 누름 → 높이 점프
- 점프 키 짧게 누름 → 낮은 점프 (속도 50% 감소)

#### 4.1.4 벽 점프/슬라이딩

**벽 슬라이딩 진입 조건**:
1. 벽에 접촉 (`isTouchingWall = true`)
2. 하강 중 (`rb.linearVelocity.y < 0`)
3. 공중 (`isGrounded = false`)

**벽 점프 실행**:
- 벽 슬라이딩 중 점프 입력
- 수평 속도: 벽 반대 방향으로 120%
- 수직 속도: 일반 점프의 85%

### 4.2 전투 시스템

#### 4.2.1 공격 타이밍

**기본 공격 (3단 콤보)**:
```
[좌클릭 #1]
공격 1 (0.3초)
    ↓ [0.5초 체인 윈도우]
[좌클릭 #2]
공격 2 (0.35초)
    ↓ [0.5초 체인 윈도우]
[좌클릭 #3]
공격 3 (0.4초)
    ↓ [콤보 종료]
```

**콤보 데미지 배율**:
- 1단: 100% (10 데미지)
- 2단: 120% (12 데미지)
- 3단: 150% (15 데미지)

#### 4.2.2 히트스톱 (Hit Stop)

적 피격 시 **시간 지연 효과**로 타격감 강화:
- **경타**: 0.05초 정지
- **강타**: 0.1초 정지
- **크리티컬**: 0.15초 정지

구현:
```csharp
public async Awaitable ApplyHitStop(float duration)
{
    Time.timeScale = 0f;
    await Awaitable.WaitForSecondsAsync(duration, ignoreTimeScale: true);
    Time.timeScale = 1f;
}
```

#### 4.2.3 크리티컬 시스템

**크리티컬 조건**:
- 확률: 10% (기본)
- 배율: 1.5× (기본)
- 스컬/장비로 확률/배율 증가 가능

**크리티컬 연출**:
- 빨간색 데미지 폰트
- 더 큰 히트스톱
- 특수 사운드 효과

#### 4.2.4 넉백 시스템

**넉백 벡터**:
```csharp
Vector2 knockback = (target.position - attacker.position).normalized * knockbackForce;
target.SetVelocity(knockback);
```

**넉백 타입**:
- **경타**: 짧은 밀림 (1 유닛)
- **강타**: 긴 밀림 (3 유닛)
- **궁극기**: 날림 (5 유닛 + 공중으로)

---

### 4.3 적 AI 행동

#### 4.3.1 적 행동 패턴

**Idle (대기)**:
- 제자리에서 대기
- 플레이어 감지 범위(DetectRange) 체크
- 일정 시간 후 Patrol로 전환 (옵션)

**Patrol (정찰)**:
- 좌우 왕복 이동 (patrolDistance)
- 각 끝에서 patrolWaitTime(2초) 대기
- 플레이어 감지 시 Chase로 전환

**Chase (추적)**:
- 플레이어 방향으로 이동 (moveSpeed)
- 공격 거리(AttackRange) 도달 시 Attack으로
- 추적 범위(ChaseRange) 벗어나면 Idle로

**Attack (공격)**:
- 공격 애니메이션 재생
- 히트박스 생성 (attackSize)
- 쿨다운 후 Chase 또는 Idle로

**Hit (피격)**:
- 피격 애니메이션 재생
- 넉백 적용
- 경직 시간(hitStunDuration) 대기
- 경직 종료 후 Chase로

**Death (사망)**:
- 사망 애니메이션 재생
- Collider/Rigidbody 비활성화
- 페이드아웃 (1초)
- GameObject 파괴

#### 4.3.2 적 타입

| 적 타입 | 특징 | 행동 | 약점 |
|--------|------|------|------|
| **Goblin** | 기본 근접 | Idle → Chase → Attack | 낮은 체력 |
| **Archer** | 원거리 | Chase 유지, 원거리 공격 | 근접 시 도망 |
| **Shieldman** | 방어형 | 정면 방어, 측면 약함 | 측면/후방 공격 |
| **Berserker** | 공격형 | 빠른 돌진 공격 | 회피 후 반격 |
| **Boss** | 보스 | 페이즈별 패턴 | 특정 타이밍 |

---

## 5. 기술 아키텍처

### 5.1 프로젝트 구조

```
GASPT/
├── Assets/
│   ├── _Project/              # 게임 프로젝트 전용
│   │   ├── Scripts/           # 게임 로직 (23개 어셈블리)
│   │   │   ├── Core/         # 핵심 매니저
│   │   │   ├── Gameplay/     # 플레이어/적/스컬
│   │   │   ├── UI/           # UI 시스템
│   │   │   └── Tests/        # 테스트
│   │   ├── Scenes/           # 씬 파일
│   │   ├── Resources/        # ScriptableObject 데이터
│   │   └── Prefabs/          # Prefab
│   └── Plugins/              # 재사용 가능한 시스템
│       ├── FSM_Core/         # 유한상태머신
│       ├── GAS_Core/         # 어빌리티 시스템
│       └── FSM_GAS_Integration/
└── docs/                     # 문서
```

### 5.2 어셈블리 구조

**계층적 의존성** (하위 → 상위로만):

```
Level 1 (Plugin Layer)
├── FSM.Core
├── GAS.Core
└── FSM.Core.Integration

Level 2 (Foundation Layer)
├── Core.Enums
├── Core.Utilities
└── Core.Data

Level 3 (Core Layer)
├── Core.Managers
└── Core.Audio

Level 4 (UI Layer)
├── UI.Core
├── UI.Panels
└── UI.Components

Level 5 (Gameplay Layer)
├── Player
├── Combat.Core
├── Enemy
└── Skull
```

### 5.3 데이터 흐름

#### 5.3.1 게임 초기화 순서

게임 시작부터 플레이어 입장까지의 전체 초기화 순서는 다음과 같습니다:

**1. BootstrapManager.Awake()**
```
Scene: Bootstrap.unity (게임 시작 시 첫 씬)
    ↓
SingletonManager 초기화
    ├─ GameResourceManager 싱글톤 생성
    ├─ GameFlowManager 싱글톤 생성
    └─ InputManager 싱글톤 생성 (향후)
    ↓
GameResourceManager.Initialize()
```

**2. GameResourceManager.LoadManifest()**
```
Essential Resources 동기 로드 (게임 시작에 필수):
    ├─ SkulPhysicsConfig (CharacterPhysics에 필요)
    ├─ InputMappingData (향후 구현)
    └─ GameSettings (게임 설정)
    ↓
resourceCache에 캐싱 완료
    ↓
OnResourcesLoaded 이벤트 발생
```

**3. GameFlowManager.Start()**
```
Preload 상태 진입
    ↓
나머지 ScriptableObject 비동기 로드:
    ├─ AbilityData (모든 어빌리티)
    ├─ SkullData (모든 스컬)
    ├─ EnemyData (모든 적)
    └─ UIAssets (UI 프리팹)
    ↓
로딩 진행률 UI 업데이트
    ↓
로딩 완료 → Main 상태 진입
```

**4. SceneLoader.LoadMainMenu()**
```
메인 메뉴 씬 로드 (비동기)
    ↓
UIManager 초기화
    ├─ MainMenuPanel 표시
    └─ 버튼 이벤트 연결
    ↓
플레이어 대기 상태
```

**5. 플레이어 입장 (Ingame 전환)**
```
[플레이 버튼 클릭]
    ↓
GameFlowManager.TransitionTo(GameFlowState.Ingame)
    ↓
SceneLoader.LoadGameScene()
    ├─ 던전 씬 로드
    └─ PlayerController 생성
    ↓
PlayerController.Initialize()
    ├─ GameResourceManager.GetResource<SkulPhysicsConfig>()
    ├─ CharacterPhysics 초기화
    ├─ AbilitySystem 초기화
    └─ FSM 초기화 (Idle 상태)
    ↓
게임 플레이 시작
```

**⚠️ 초기화 순서 주의사항**:

1. **GameResourceManager는 모든 컴포넌트보다 먼저 초기화**
   - PlayerController, EnemyController 등이 Start()에서 리소스 요청 가능하도록

2. **Essential Resources는 동기 로드**
   - SkulPhysicsConfig 등 즉시 필요한 데이터는 비동기 로드하지 않음
   - Preload 상태에서 동기 로드 완료 후 다음 단계 진행

3. **나머지 Resources는 비동기 로드**
   - 대량의 ScriptableObject는 Preload 상태에서 비동기 로드
   - 로딩 화면 표시로 사용자 경험 개선

4. **리소스 로드 실패 처리**
   - GameResourceManager.GetResource()는 null 반환 가능
   - 호출하는 쪽에서 null 체크 및 에러 처리 필수

#### 5.3.2 ScriptableObject 로딩 흐름

**ScriptableObject 기반 데이터 관리**:

```
Bootstrap
    ↓
GameResourceManager.LoadManifest()
    ├─ Resources/Manifests/GameplayManifest.asset
    └─ LoadEssentialResources()
        ├─ SkulPhysicsConfig
        ├─ AbilityData (모든 어빌리티)
        ├─ SkullData (모든 스컬)
        └─ EnemyData (모든 적)
            ↓
resourceCache (Dictionary)
    ↓
GetResource<T>("Path")
    ↓
컴포넌트에서 사용
```

### 5.4 이벤트 시스템

**관찰자 패턴** (Observer Pattern):

```csharp
// 발행자 (Publisher)
public event Action<DamageData> OnDamaged;

// 구독자 (Subscriber)
healthSystem.OnDamaged += OnDamaged;

// 발행
OnDamaged?.Invoke(damageData);
```

**주요 이벤트 체인**:

```
[플레이어 공격]
AttackState.CreateHitbox()
    ↓ OnHit 이벤트
DamageSystem.ApplyDamage()
    ↓ OnDamaged 이벤트
HealthSystem.TakeDamage()
    ├─ OnHealthChanged 이벤트 → UI 업데이트
    └─ [체력 0] OnDeath 이벤트 → 사망 처리
```

### 5.5 비동기 패턴

**Unity 6.0+ Awaitable**:

```csharp
// ❌ 구버전 (Coroutine)
public IEnumerator LoadSceneCoroutine()
{
    yield return new WaitForSeconds(1f);
}

// ✅ 신버전 (Awaitable)
public async Awaitable LoadSceneAsync()
{
    await Awaitable.WaitForSecondsAsync(1f);
}
```

**Fire-and-Forget 패턴**:
```csharp
_ = ExecuteAbilityAsync(ability);
```

---

## 6. 데이터 구조

### 6.1 ScriptableObject 목록

| ScriptableObject | 경로 | 용도 |
|------------------|------|------|
| **AbilityData** | Resources/Data/Abilities/ | 어빌리티 메타데이터 |
| **SkulPhysicsConfig** | Resources/Data/SkulPhysicsConfig | 물리 설정 |
| **SkullData** | Resources/Data/Skulls/ | 스컬 데이터 |
| **SkullMovementProfile** | Resources/Data/Physics/ | 스컬 이동 프로필 |
| **EnemyData** | Resources/Data/Enemies/ | 적 데이터 |
| **ComboData** | Resources/Data/Combos/ | 콤보 데이터 |
| **ResourceManifest** | Resources/Manifests/ | 리소스 목록 |

### 6.2 데이터 예시

#### AbilityData

```yaml
# PlayerAttack_1.asset
abilityId: "PlayerAttack_1"
abilityName: "기본 공격 1단"
icon: (Sprite)
cooldownTime: 0
resourceCosts: {}
isComboAbility: true
nextAbilityId: "PlayerAttack_2"
chainWindowDuration: 0.5
executor: (DamageExecutor)
```

#### SkullData

```yaml
# WarriorSkull.asset
skullName: "전사"
skullType: Warrior
skullIcon: (Sprite)
skullPrefab: (GameObject)
baseStats:
  maxHealth: 150
  moveSpeed: 4.5
  attackPower: 20
  defense: 10
abilities:
  - PlayerAttack_Warrior_1
  - PlayerAttack_Warrior_2
  - WarriorUltimate
```

#### EnemyData

```yaml
# Goblin.asset
enemyType: Goblin
maxHealth: 30
moveSpeed: 3
detectRange: 5
chaseRange: 8
attackRange: 1.5
attackDamage: 10
attackCooldown: 1.5
enablePatrol: true
patrolDistance: 5
```

---

## 7. UI/UX 설계

### 7.1 UI 구조

**Panel 기반 아키텍처**:

```
UIManager
├── MainMenuPanel (메인 메뉴)
│   ├── StartButton
│   ├── OptionsButton
│   └── QuitButton
├── LoadingPanel (로딩 화면)
│   ├── LoadingText
│   ├── ProgressBar
│   └── TipText
├── GameplayHUDPanel (게임플레이 HUD)
│   ├── HealthBar
│   ├── MPBar
│   ├── SkullIcon (현재 스컬)
│   ├── ComboCounter
│   └── BossHealthBar
└── PausePanel (일시정지)
    ├── ResumeButton
    ├── OptionsButton
    └── MainMenuButton
```

### 7.2 HUD 레이아웃

```
┌─────────────────────────────────────┐
│ HP: ████████░░ 80/100               │ ← 좌상단
│ MP: ██████░░░░ 60/100               │
│                                     │
│         [스컬 아이콘]                 │ ← 중상단
│                                     │
│                              COMBO  │ ← 우상단
│                               x3    │
│                                     │
│                                     │
│                                     │
│                                     │
│ ════════════════════════════════    │ ← 하단 (보스 체력)
│ Boss: Ancient Dragon   HP: 50%     │
└─────────────────────────────────────┘
```

### 7.3 메뉴 플로우

```
[게임 시작]
    ↓
MainMenuPanel
    ├─ [시작] → LoadingPanel → GameplayHUDPanel
    ├─ [옵션] → OptionsPanel → MainMenuPanel
    └─ [종료] → Application.Quit()

[게임 중 ESC]
    ↓
PausePanel
    ├─ [계속하기] → GameplayHUDPanel
    ├─ [옵션] → OptionsPanel → PausePanel
    └─ [메인 메뉴] → MainMenuPanel
```

### 7.4 피드백 시스템

**시각적 피드백**:
- 데미지 폰트 (숫자 튀어오름)
- 히트 이펙트 (파티클)
- 화면 흔들림 (Screen Shake)
- 색상 플래시 (피격 시 붉은색)

**청각적 피드백**:
- 공격 사운드 (타격음)
- 피격 사운드 (고통 소리)
- 크리티컬 사운드 (특수 효과음)
- 배경 음악 (전투 중 빠르게)

**촉각적 피드백** (향후 지원):
- 컨트롤러 진동

### 7.5 UI 이벤트 시스템 연결

UI는 게임 시스템의 상태를 실시간으로 반영해야 합니다. 이를 위해 Observer 패턴을 사용하여 이벤트 기반 UI 업데이트를 구현합니다.

#### 7.5.1 HealthBarUI 구현 예시

**목적**: 플레이어 체력이 변경될 때마다 HealthBar UI를 자동으로 업데이트합니다.

```csharp
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;     // 체력바 Fill 이미지
    [SerializeField] private Text healthText;         // "80/100" 텍스트
    private HealthSystem playerHealth;

    void Start()
    {
        // PlayerController에서 HealthSystem 참조 획득
        playerHealth = FindAnyObjectByType<PlayerController>()
            ?.GetComponent<HealthSystem>();

        if (playerHealth == null)
        {
            Debug.LogError("HealthBarUI: PlayerController의 HealthSystem을 찾을 수 없습니다.");
            return;
        }

        // HealthSystem의 OnHealthChanged 이벤트 구독
        playerHealth.OnHealthChanged += UpdateHealthBar;

        // 초기 체력 표시
        UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    void UpdateHealthBar(float current, float max)
    {
        // Fill Amount 업데이트 (0.0 ~ 1.0)
        healthBarFill.fillAmount = current / max;

        // 텍스트 업데이트
        healthText.text = $"{current:F0}/{max:F0}";

        // 체력 비율에 따라 색상 변경 (선택적)
        if (current / max < 0.3f)
            healthBarFill.color = Color.red;      // 30% 이하: 빨강
        else if (current / max < 0.6f)
            healthBarFill.color = Color.yellow;   // 60% 이하: 노랑
        else
            healthBarFill.color = Color.green;    // 그 외: 초록
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthBar;
    }
}
```

**HealthSystem.cs 이벤트 정의**:
```csharp
public class HealthSystem : MonoBehaviour
{
    public event Action<float, float> OnHealthChanged; // (current, max)

    private float currentHealth;
    private float maxHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // 이벤트 발생
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
}
```

#### 7.5.2 ComboCounterUI 구현 예시

**목적**: 콤보가 증가할 때마다 화면에 표시하고, 일정 시간 후 페이드 아웃합니다.

```csharp
using UnityEngine;
using UnityEngine.UI;

public class ComboCounterUI : MonoBehaviour
{
    [SerializeField] private Text comboText;           // "x3" 텍스트
    [SerializeField] private CanvasGroup canvasGroup;  // 페이드 아웃용
    [SerializeField] private float displayDuration = 0.5f; // 표시 시간
    private ComboSystem comboSystem;

    void Start()
    {
        // ComboSystem 참조 획득
        comboSystem = FindAnyObjectByType<ComboSystem>();

        if (comboSystem == null)
        {
            Debug.LogError("ComboCounterUI: ComboSystem을 찾을 수 없습니다.");
            return;
        }

        // 이벤트 구독
        comboSystem.OnComboAdvanced += ShowCombo;
        comboSystem.OnComboReset += HideCombo;

        // 초기 상태: 숨김
        HideCombo();
    }

    async void ShowCombo(int comboCount)
    {
        // 콤보 텍스트 업데이트
        comboText.text = $"x{comboCount}";

        // 즉시 표시
        canvasGroup.alpha = 1f;

        // displayDuration초 대기
        await Awaitable.WaitForSecondsAsync(displayDuration);

        // 페이드 아웃
        await FadeOutAsync(0.3f);
    }

    async Awaitable FadeOutAsync(float duration)
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            await Awaitable.NextFrameAsync();
        }

        canvasGroup.alpha = 0f;
    }

    void HideCombo()
    {
        canvasGroup.alpha = 0f;
    }

    void OnDestroy()
    {
        if (comboSystem != null)
        {
            comboSystem.OnComboAdvanced -= ShowCombo;
            comboSystem.OnComboReset -= HideCombo;
        }
    }
}
```

**ComboSystem.cs 이벤트 정의**:
```csharp
public class ComboSystem : MonoBehaviour
{
    public event Action<int> OnComboAdvanced;  // 콤보 증가 시 (콤보 수)
    public event Action OnComboReset;          // 콤보 리셋 시

    private int currentCombo = 0;

    public void AdvanceCombo()
    {
        currentCombo++;
        OnComboAdvanced?.Invoke(currentCombo);
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        OnComboReset?.Invoke();
    }
}
```

#### 7.5.3 BossHealthBarUI 구현 예시

**목적**: 보스 전투 시작 시 보스 체력바를 표시하고, 체력 변화를 추적합니다.

```csharp
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [SerializeField] private GameObject bossHealthBarContainer; // 보스 체력바 전체 컨테이너
    [SerializeField] private Image bossHealthFill;
    [SerializeField] private Text bossNameText;
    [SerializeField] private Text bossHealthPercentText;

    private HealthSystem bossHealth;

    void Start()
    {
        // 초기 상태: 숨김
        bossHealthBarContainer.SetActive(false);

        // BossSpawner 이벤트 구독 (보스 등장 시)
        var bossSpawner = FindAnyObjectByType<BossSpawner>();
        if (bossSpawner != null)
        {
            bossSpawner.OnBossSpawned += OnBossSpawned;
        }
    }

    void OnBossSpawned(GameObject bossObject)
    {
        // 보스 HealthSystem 획득
        bossHealth = bossObject.GetComponent<HealthSystem>();

        if (bossHealth == null)
        {
            Debug.LogError("BossHealthBarUI: 보스에 HealthSystem이 없습니다.");
            return;
        }

        // 보스 이름 표시
        var bossController = bossObject.GetComponent<BossController>();
        bossNameText.text = bossController != null ? bossController.BossName : "Boss";

        // 체력바 표시
        bossHealthBarContainer.SetActive(true);

        // 이벤트 구독
        bossHealth.OnHealthChanged += UpdateBossHealthBar;
        bossHealth.OnDeath += OnBossDeath;

        // 초기 체력 표시
        UpdateBossHealthBar(bossHealth.CurrentHealth, bossHealth.MaxHealth);
    }

    void UpdateBossHealthBar(float current, float max)
    {
        bossHealthFill.fillAmount = current / max;
        bossHealthPercentText.text = $"{(current / max * 100):F0}%";
    }

    void OnBossDeath()
    {
        // 보스 사망 시 체력바 숨김
        bossHealthBarContainer.SetActive(false);

        // 이벤트 구독 해제
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateBossHealthBar;
            bossHealth.OnDeath -= OnBossDeath;
        }
    }

    void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateBossHealthBar;
            bossHealth.OnDeath -= OnBossDeath;
        }
    }
}
```

#### 7.5.4 UI 이벤트 연결 요약

**이벤트 체인 예시**:
```
[플레이어 피격]
    ↓
EnemyAttack.OnHit 발생
    ↓
DamageSystem.ApplyDamage()
    ↓
HealthSystem.TakeDamage()
    ├─ OnHealthChanged 이벤트 발생
    │   ↓
    │   HealthBarUI.UpdateHealthBar() 실행
    │   ├─ healthBarFill.fillAmount 업데이트
    │   └─ healthText.text 업데이트
    │
    └─ [체력 0] OnDeath 이벤트 발생
        ↓
        GameOverUI 표시
```

**주요 원칙**:
1. **이벤트 기반 업데이트**: Update()에서 매 프레임 체크하지 않음
2. **구독 해제 필수**: OnDestroy()에서 이벤트 구독 해제 (메모리 누수 방지)
3. **Null 체크**: 이벤트 호출 전 항상 `?.Invoke()` 사용
4. **Awaitable 사용**: 애니메이션/페이드 아웃은 Coroutine 대신 async/await 사용

---

## 8. 적 AI 설계

### 8.1 AI 아키텍처 (FSM 기반)

**GASPT는 FSM 기반 적 AI를 사용합니다.**

행동 트리(Behavior Tree)가 아닌 **FSM Core 시스템을 활용한 상태 기반 AI**를 사용합니다.

**선택 이유**:
- 프로젝트는 이미 FSM Core를 핵심 시스템으로 사용 중 (GameFlow, Player, Enemy)
- 단순한 적 AI는 FSM으로 충분 (5~6개 상태)
- 복잡한 보스 AI는 Phase 4 이후 검토 가능
- 팀 전체가 FSM 패턴에 익숙하여 생산성 향상

#### 8.1.1 EnemyController FSM 구조

**기본 적 AI 상태 머신**:
```
Idle (대기)
    ↓ [플레이어 감지 범위 진입]
Chase (추적)
    ↓ [공격 범위 도달]
Attack (공격)
    ├─ [공격 완료] → Chase
    ├─ [플레이어 멀어짐] → Chase
    └─ [피격] → Hit
Hit (피격)
    ├─ [경직 종료] → Chase
    └─ [체력 0] → Death
Death (사망)
    └─ [사망 애니메이션 종료] → Destroy
```

**선택적 상태** (특정 적 타입에만 사용):
- **Patrol**: 정찰 (특정 경로 순회)
- **Defend**: 방어 (방패 올리기)
- **Enrage**: 분노 (체력 30% 이하 시 공격력 증가)

#### 8.1.2 EnemyController 구현 예시

```csharp
using UnityEngine;
using FSM.Core; // FSM Core 시스템 사용

public class EnemyController : MonoBehaviour
{
    [Header("AI 설정")]
    [SerializeField] private float detectRange = 10f;   // 플레이어 감지 범위
    [SerializeField] private float attackRange = 2f;    // 공격 범위
    [SerializeField] private float moveSpeed = 3f;      // 이동 속도

    private StateMachine stateMachine;
    private Transform player;
    private HealthSystem healthSystem;

    void Start()
    {
        // 플레이어 참조 획득
        player = FindAnyObjectByType<PlayerController>()?.transform;

        // HealthSystem 참조
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.OnDeath += OnDeath;

        // FSM 초기화
        InitializeStateMachine();
    }

    void InitializeStateMachine()
    {
        stateMachine = new StateMachine();

        // 상태 등록
        stateMachine.AddState("Idle", new EnemyIdleState(this));
        stateMachine.AddState("Chase", new EnemyChaseState(this));
        stateMachine.AddState("Attack", new EnemyAttackState(this));
        stateMachine.AddState("Hit", new EnemyHitState(this));
        stateMachine.AddState("Death", new EnemyDeathState(this));

        // 시작 상태
        stateMachine.Start("Idle");
    }

    void Update()
    {
        stateMachine.Update();
    }

    // AI 판단용 헬퍼 메서드
    public bool IsPlayerInDetectRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= detectRange;
    }

    public bool IsPlayerInAttackRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= attackRange;
    }

    public void MoveTowardsPlayer(float speed)
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnDeath()
    {
        stateMachine.TransitionTo("Death");
    }

    void OnDestroy()
    {
        if (healthSystem != null)
            healthSystem.OnDeath -= OnDeath;
    }
}
```

#### 8.1.3 상태 구현 예시

**EnemyIdleState (대기 상태)**:
```csharp
public class EnemyIdleState : IState
{
    private EnemyController controller;

    public EnemyIdleState(EnemyController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        // 대기 애니메이션 재생
        Debug.Log("Enemy: Idle 상태 진입");
    }

    public void OnUpdate()
    {
        // 플레이어 감지 시 Chase로 전환
        if (controller.IsPlayerInDetectRange())
        {
            controller.StateMachine.TransitionTo("Chase");
        }
    }

    public void OnExit()
    {
        // 정리 작업
    }
}
```

**EnemyChaseState (추적 상태)**:
```csharp
public class EnemyChaseState : IState
{
    private EnemyController controller;

    public EnemyChaseState(EnemyController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        Debug.Log("Enemy: Chase 상태 진입");
    }

    public void OnUpdate()
    {
        // 공격 범위 도달 시 Attack으로 전환
        if (controller.IsPlayerInAttackRange())
        {
            controller.StateMachine.TransitionTo("Attack");
            return;
        }

        // 플레이어가 멀어지면 Idle로 복귀
        if (!controller.IsPlayerInDetectRange())
        {
            controller.StateMachine.TransitionTo("Idle");
            return;
        }

        // 플레이어 추적
        controller.MoveTowardsPlayer(controller.MoveSpeed);
    }

    public void OnExit()
    {
        // 정리 작업
    }
}
```

**EnemyAttackState (공격 상태)**:
```csharp
public class EnemyAttackState : IState
{
    private EnemyController controller;
    private float attackCooldown = 1.5f;
    private float lastAttackTime;

    public EnemyAttackState(EnemyController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        Debug.Log("Enemy: Attack 상태 진입");
        // 공격 애니메이션 트리거
        PerformAttack();
    }

    public void OnUpdate()
    {
        // 쿨다운 체크
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        // 플레이어가 공격 범위 내에 있으면 재공격
        if (controller.IsPlayerInAttackRange())
        {
            PerformAttack();
        }
        else
        {
            // 범위 밖이면 Chase로 전환
            controller.StateMachine.TransitionTo("Chase");
        }
    }

    void PerformAttack()
    {
        lastAttackTime = Time.time;
        // 히트박스 생성, 데미지 처리 등
        Debug.Log("Enemy: 공격 실행!");
    }

    public void OnExit()
    {
        // 정리 작업
    }
}
```

#### 8.1.4 향후 확장 가능성

**Phase 4 이후 고려사항**:
- **복잡한 보스 AI**: 행동 트리 또는 계층적 FSM (HFSM) 도입 검토 가능
- **AI 디자이너 툴**: Unity 에디터 확장으로 FSM 상태 그래프 시각화
- **동적 상태 추가**: 런타임에 상태 동적 등록 (모딩 지원)

**하지만 Phase 3까지는 기본 FSM으로 충분합니다.**

### 8.2 적 난이도 설계

| 난이도 | 체력 배율 | 공격력 배율 | 이동속도 배율 | 특징 |
|--------|---------|-----------|-------------|------|
| **Easy** | 0.7× | 0.7× | 0.8× | 느린 공격, 긴 텔레그래프 |
| **Normal** | 1.0× | 1.0× | 1.0× | 기본 밸런스 |
| **Hard** | 1.3× | 1.3× | 1.2× | 빠른 공격, 짧은 텔레그래프 |
| **Nightmare** | 1.5× | 1.5× | 1.5× | 새로운 패턴 추가 |

### 8.3 보스 페이즈 시스템

**페이즈 전환**:
```
Phase 1 (100% ~ 70% HP)
├── 기본 공격 패턴
├── 느린 이동
└── 텔레그래프 1초

Phase 2 (70% ~ 40% HP)
├── 새로운 공격 추가
├── 빠른 이동
└── 텔레그래프 0.5초

Phase 3 (40% ~ 0% HP)
├── 광역 공격
├── 매우 빠른 이동
├── 텔레그래프 0.3초
└── 분노 모드 (공격력 +50%)
```

---

## 9. 레벨 디자인

### 9.1 레벨 구성

**던전 구조**:
```
시작 방
    ↓
일반 방 #1 (적 3~5마리)
    ↓
일반 방 #2 (적 5~8마리)
    ↓
보상 방 (스컬/아이템 선택)
    ↓
일반 방 #3 (적 8~12마리)
    ↓
엘리트 방 (강한 적 2~3마리)
    ↓
보스 방 (보스 1마리)
    ↓
다음 던전 또는 엔딩
```

### 9.2 난이도 곡선

```
[난이도]
   ↑
 10│                        ┌─ 보스
   │                    ┌───┘
  8│                ┌───┘
   │            ┌───┘
  6│        ┌───┘
   │    ┌───┘  ← 엘리트
  4│┌───┘
   │└─ 일반 적
  2│    ↑보상방 (난이도 하락)
   │
  0└──────────────────────────→ [시간]
```

### 9.3 환경 기믹

**플랫폼**:
- 고정 플랫폼 (Solid)
- 낙하 플랫폼 (OneWay)
- 이동 플랫폼 (Moving) - 향후
- 붕괴 플랫폼 (Crumbling) - 향후

**위험 요소**:
- 가시 (즉사 or 고정 데미지)
- 용암 (지속 데미지)
- 독 구름 (디버프)

**상호작용 오브젝트**:
- 레버 (문 개방)
- 상자 (아이템 획득)
- 텔레포트 (순간 이동)

### 9.4 레벨 제작 워크플로우

Phase 4에서 콘텐츠(적, 보스, 던전)를 제작하려면 **레벨 제작 도구와 워크플로우**가 필요합니다. 이 섹션은 Phase 4 시작 전에 설계해야 합니다.

#### 9.4.1 RoomData ScriptableObject

각 방의 정보를 ScriptableObject로 정의합니다.

**RoomData.cs 구조**:
```csharp
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "GASPT/Level/RoomData", fileName = "Room_")]
public class RoomData : ScriptableObject
{
    [Header("방 기본 정보")]
    public string roomName;                  // 방 이름 (예: "Room_Normal_01")
    public RoomType roomType;                // Normal, Elite, Boss, Reward
    public Vector2Int roomSize;              // 방 크기 (타일 단위)

    [Header("적 스폰 정보")]
    public List<EnemySpawnData> enemies;     // 스폰할 적 목록

    [Header("출구 정보")]
    public List<Vector2> exitPositions;      // 출구 위치 (방 상대 좌표)
    public List<ExitDirection> exitDirections; // 출구 방향 (North, South, East, West)

    [Header("환경 오브젝트")]
    public List<EnvironmentObject> objects;  // 가시, 상자, 레버 등

    [Header("보상 (보상 방 전용)")]
    public List<RewardData> rewards;         // 스컬, 아이템 등
}

[System.Serializable]
public class EnemySpawnData
{
    public EnemyType enemyType;              // 적 타입
    public Vector2 spawnPosition;            // 스폰 위치 (방 상대 좌표)
    public float spawnDelay;                 // 스폰 지연 시간 (0 = 즉시)
}

[System.Serializable]
public class EnvironmentObject
{
    public EnvironmentType objectType;       // Spike, Box, Lever, etc.
    public Vector2 position;
    public bool isInteractable;
}

public enum RoomType
{
    Normal,    // 일반 전투 방
    Elite,     // 엘리트 전투 방
    Boss,      // 보스 방
    Reward,    // 보상 방
    Start,     // 시작 방
    Safe       // 안전 방 (상점, 휴식)
}

public enum ExitDirection
{
    North,
    South,
    East,
    West
}
```

**RoomData 에셋 예시**:
```
Assets/Data/Rooms/
├── Room_Normal_01.asset
│   - roomType: Normal
│   - enemies: [Goblin x3, Skeleton x2]
│   - exitPositions: [(0, 10), (20, 5)]
├── Room_Elite_01.asset
│   - roomType: Elite
│   - enemies: [OrcWarrior x2, Shaman x1]
├── Room_Boss_01.asset
│   - roomType: Boss
│   - enemies: [DragonBoss x1]
└── Room_Reward_01.asset
    - roomType: Reward
    - rewards: [WarriorSkull, MageSkull, HealthPotion x3]
```

#### 9.4.2 DungeonGenerator

RoomData 리스트를 기반으로 런타임에 던전을 생성합니다.

**DungeonGenerator.cs 구조**:
```csharp
using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [Header("던전 구성")]
    [SerializeField] private DungeonConfig dungeonConfig; // 던전 설정 (몇 개 방, 보상 방 위치 등)
    [SerializeField] private List<RoomData> normalRoomPool;
    [SerializeField] private List<RoomData> eliteRoomPool;
    [SerializeField] private List<RoomData> bossRoomPool;
    [SerializeField] private List<RoomData> rewardRoomPool;

    [Header("프리팹")]
    [SerializeField] private GameObject roomPrefab;      // 빈 방 프리팹
    [SerializeField] private GameObject doorPrefab;      // 문 프리팹

    private List<Room> generatedRooms = new List<Room>();
    private int currentRoomIndex = 0;

    public async Awaitable GenerateDungeonAsync()
    {
        // 던전 생성 로직
        // 1. 방 순서 결정 (Normal → Reward → Elite → Boss)
        // 2. 각 방 RoomData 선택 (풀에서 랜덤)
        // 3. 방 인스턴스화 및 적 스폰
        // 4. 출구 연결

        Debug.Log("던전 생성 시작");

        // 예시: 7개 방 던전
        // Start → Normal x2 → Reward → Normal x2 → Elite → Boss
        List<RoomData> selectedRooms = new List<RoomData>();

        selectedRooms.Add(GetStartRoom());
        selectedRooms.Add(GetRandomRoom(normalRoomPool));
        selectedRooms.Add(GetRandomRoom(normalRoomPool));
        selectedRooms.Add(GetRandomRoom(rewardRoomPool));
        selectedRooms.Add(GetRandomRoom(normalRoomPool));
        selectedRooms.Add(GetRandomRoom(normalRoomPool));
        selectedRooms.Add(GetRandomRoom(eliteRoomPool));
        selectedRooms.Add(GetRandomRoom(bossRoomPool));

        // 각 방 생성
        foreach (var roomData in selectedRooms)
        {
            Room room = await CreateRoomAsync(roomData);
            generatedRooms.Add(room);
        }

        // 방 연결
        ConnectRooms();

        Debug.Log($"던전 생성 완료: {generatedRooms.Count}개 방");
    }

    async Awaitable<Room> CreateRoomAsync(RoomData roomData)
    {
        // 방 프리팹 인스턴스화
        GameObject roomObject = Instantiate(roomPrefab);
        Room room = roomObject.AddComponent<Room>();
        room.Initialize(roomData);

        // 적 스폰
        foreach (var enemyData in roomData.enemies)
        {
            await SpawnEnemyAsync(enemyData, room);
        }

        return room;
    }

    async Awaitable SpawnEnemyAsync(EnemySpawnData enemyData, Room room)
    {
        // 지연 시간 대기
        await Awaitable.WaitForSecondsAsync(enemyData.spawnDelay);

        // 적 인스턴스화
        // GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"적 스폰: {enemyData.enemyType} at {enemyData.spawnPosition}");
    }

    RoomData GetRandomRoom(List<RoomData> pool)
    {
        return pool[Random.Range(0, pool.Count)];
    }

    RoomData GetStartRoom()
    {
        // 시작 방은 고정
        return Resources.Load<RoomData>("Data/Rooms/Room_Start");
    }

    void ConnectRooms()
    {
        // 방 사이 출구 연결 로직
        for (int i = 0; i < generatedRooms.Count - 1; i++)
        {
            Room currentRoom = generatedRooms[i];
            Room nextRoom = generatedRooms[i + 1];

            // 출구 생성 및 연결
            CreateDoor(currentRoom, nextRoom);
        }
    }

    void CreateDoor(Room from, Room to)
    {
        // 문 생성 및 씬 전환 트리거 설정
        Debug.Log($"문 생성: {from.RoomData.roomName} → {to.RoomData.roomName}");
    }
}
```

#### 9.4.3 에디터 툴: RoomEditorWindow

Unity 에디터에서 방을 비주얼하게 디자인할 수 있는 도구를 제공합니다.

**RoomEditorWindow.cs 개념**:
```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class RoomEditorWindow : EditorWindow
{
    private RoomData currentRoom;
    private Vector2 scrollPosition;

    [MenuItem("GASPT/Room Editor")]
    public static void ShowWindow()
    {
        GetWindow<RoomEditorWindow>("Room Editor");
    }

    void OnGUI()
    {
        GUILayout.Label("방 에디터", EditorStyles.boldLabel);

        // RoomData 선택
        currentRoom = (RoomData)EditorGUILayout.ObjectField("편집할 방", currentRoom, typeof(RoomData), false);

        if (currentRoom == null)
        {
            EditorGUILayout.HelpBox("편집할 RoomData를 선택하세요.", MessageType.Info);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 기본 정보
        currentRoom.roomName = EditorGUILayout.TextField("방 이름", currentRoom.roomName);
        currentRoom.roomType = (RoomType)EditorGUILayout.EnumPopup("방 타입", currentRoom.roomType);
        currentRoom.roomSize = EditorGUILayout.Vector2IntField("방 크기", currentRoom.roomSize);

        EditorGUILayout.Space();

        // 적 스폰 리스트 (드래그 앤 드롭 지원)
        GUILayout.Label("적 스폰 설정", EditorStyles.boldLabel);
        // ... 적 추가/제거 UI

        EditorGUILayout.Space();

        // 시각적 방 레이아웃 (그리드)
        DrawRoomLayout();

        EditorGUILayout.EndScrollView();

        // 저장 버튼
        if (GUILayout.Button("변경사항 저장"))
        {
            EditorUtility.SetDirty(currentRoom);
            AssetDatabase.SaveAssets();
        }
    }

    void DrawRoomLayout()
    {
        // 방을 그리드로 표시
        // 적 스폰 위치를 드래그 앤 드롭으로 배치
        // 환경 오브젝트 배치
        GUILayout.Label("방 레이아웃 (그리드)", EditorStyles.boldLabel);
        // TODO: Scene View에 Gizmos로 시각화
    }
}
#endif
```

**에디터 기능**:
1. **RoomData 생성/편집**: 인스펙터 대신 전용 UI
2. **시각적 배치**: Scene View에 적 스폰 위치를 Gizmos로 표시
3. **드래그 앤 드롭**: 적을 그리드에 배치
4. **프리뷰**: 방을 실제로 생성하지 않고 미리보기
5. **검증**: 적 수, 출구 위치 등 유효성 검사

#### 9.4.4 RoomTransitionManager

방 이동 시 플레이어 위치 조정 및 카메라 전환을 처리합니다.

**RoomTransitionManager.cs 개념**:
```csharp
using UnityEngine;

public class RoomTransitionManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float transitionDuration = 0.5f;

    private Room currentRoom;

    public async Awaitable TransitionToRoomAsync(Room nextRoom, ExitDirection direction)
    {
        Debug.Log($"방 전환: {currentRoom?.RoomData.roomName} → {nextRoom.RoomData.roomName}");

        // 1. 페이드 아웃
        await FadeOutAsync(transitionDuration / 2);

        // 2. 플레이어 위치 이동
        MovePlayerToEntrance(nextRoom, direction);

        // 3. 카메라 이동
        MoveCameraToRoom(nextRoom);

        // 4. 적 스폰
        nextRoom.SpawnEnemies();

        // 5. 페이드 인
        await FadeInAsync(transitionDuration / 2);

        currentRoom = nextRoom;
    }

    void MovePlayerToEntrance(Room room, ExitDirection entryDirection)
    {
        // 입구 방향에 따라 플레이어 위치 결정
        Vector2 entrancePosition = room.GetEntrancePosition(entryDirection);

        var player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.transform.position = entrancePosition;
        }
    }

    void MoveCameraToRoom(Room room)
    {
        // 카메라를 방 중앙으로 이동
        Vector3 roomCenter = room.GetCenterPosition();
        mainCamera.transform.position = new Vector3(roomCenter.x, roomCenter.y, mainCamera.transform.position.z);
    }

    async Awaitable FadeOutAsync(float duration)
    {
        // 화면 페이드 아웃
        // CanvasGroup alpha 조정
        await Awaitable.WaitForSecondsAsync(duration);
    }

    async Awaitable FadeInAsync(float duration)
    {
        // 화면 페이드 인
        await Awaitable.WaitForSecondsAsync(duration);
    }
}
```

#### 9.4.5 Phase 4 시작 전 체크리스트

**레벨 제작 도구 준비**:
- [ ] RoomData ScriptableObject 구조 정의
- [ ] DungeonGenerator 기본 구조 구현
- [ ] RoomEditorWindow 에디터 툴 제작 (선택적)
- [ ] RoomTransitionManager 구현
- [ ] 테스트용 RoomData 에셋 3개 이상 제작

**워크플로우 검증**:
- [ ] RoomData 에셋만으로 방 생성 가능
- [ ] DungeonGenerator가 7개 방 던전 생성 성공
- [ ] 방 전환 시 플레이어/카메라 정상 이동
- [ ] 적 스폰이 RoomData 설정대로 동작

**Phase 4 Content 제작 시**:
1. 5가지 일반 적 → EnemyData ScriptableObject 제작
2. 첫 번째 보스 → BossData + BossController 구현
3. 첫 번째 던전 → RoomData 5~7개 제작 및 배치

---

## 10. 사운드 및 비주얼

### 10.1 사운드 설계

**사운드 카테고리**:

| 카테고리 | 예시 | 볼륨 |
|---------|------|------|
| **BGM** | 메인 테마, 전투 음악, 보스 음악 | 70% |
| **SFX** | 공격음, 피격음, 발소리 | 100% |
| **Voice** | 플레이어 대사, 적 대사 | 80% |
| **UI** | 버튼 클릭, 메뉴 이동 | 60% |

**사운드 매핑**:
- 공격 히트: `sfx_hit_light`, `sfx_hit_heavy`
- 크리티컬: `sfx_critical`
- 피격: `sfx_damaged_player`, `sfx_damaged_enemy`
- 점프: `sfx_jump`
- 대시: `sfx_dash`
- 스컬 교체: `sfx_skull_switch`

### 10.2 VFX 설계

**파티클 이펙트**:
- 공격 이펙트 (칼 궤적, 마법 탄환)
- 피격 이펙트 (피 튀김, 충격파)
- 스킬 이펙트 (궁극기 연출)
- 환경 이펙트 (불, 연기)

**셰이더 이펙트**:
- 피격 플래시 (빨간색 점멸)
- 무적 플래시 (흰색 점멸)
- 그림자 (그림자 쉐이더)
- 외곽선 (아웃라인 쉐이더)

### 10.3 에셋 준비 체크리스트

Phase 5 (Polish)를 시작하기 전에 필요한 사운드 및 VFX 에셋을 미리 준비해야 합니다. 이 섹션은 **Phase 4 완료 후, Phase 5 시작 전에 확인**합니다.

#### 10.3.1 사운드 에셋 체크리스트

**Phase 5 시작 전 필수 준비**:

**A. BGM (배경 음악) - 총 5개**
- [ ] MainThemeBGM (메인 메뉴 테마, 2분 루프)
- [ ] CombatBGM (일반 전투 음악, 2분 루프)
- [ ] BossBGM (보스 전투 음악, 3분 루프)
- [ ] VictoryBGM (승리 팡파르, 10초)
- [ ] GameOverBGM (게임 오버 음악, 15초)

**형식**: `.ogg` (용량 효율) 또는 `.mp3`
**샘플레이트**: 44.1kHz
**비트레이트**: 128kbps (배경 음악 충분)

**B. SFX (효과음) - 총 15개**

**전투 사운드 (8개)**:
- [ ] sfx_hit_light (경타격, 0.1초)
- [ ] sfx_hit_medium (중타격, 0.15초)
- [ ] sfx_hit_heavy (강타격, 0.2초)
- [ ] sfx_critical (크리티컬, 0.3초)
- [ ] sfx_damaged_player (플레이어 피격, 0.2초)
- [ ] sfx_damaged_enemy (적 피격, 0.15초)
- [ ] sfx_death_player (플레이어 사망, 1초)
- [ ] sfx_death_enemy (적 사망, 0.5초)

**이동 사운드 (5개)**:
- [ ] sfx_jump (점프, 0.1초)
- [ ] sfx_land (착지, 0.1초)
- [ ] sfx_dash (대시, 0.2초)
- [ ] sfx_footstep (발소리, 0.05초)
- [ ] sfx_wallslide (벽 슬라이딩, 0.5초 루프)

**스킬 사운드 (2개)**:
- [ ] sfx_skull_switch (스컬 교체, 0.5초)
- [ ] sfx_ultimate (궁극기, 1초)

**형식**: `.wav` (짧은 효과음은 압축 불필요)
**샘플레이트**: 44.1kHz
**비트레이트**: 16-bit

**C. UI 사운드 - 총 5개**
- [ ] ui_button_click (버튼 클릭, 0.05초)
- [ ] ui_menu_open (메뉴 열기, 0.1초)
- [ ] ui_menu_close (메뉴 닫기, 0.1초)
- [ ] ui_item_acquire (아이템 획득, 0.3초)
- [ ] ui_level_up (레벨업, 0.5초)

**형식**: `.wav`
**샘플레이트**: 22.05kHz (UI는 낮은 샘플레이트로도 충분)

**총 사운드 에셋**: 25개

#### 10.3.2 VFX (Visual Effects) 에셋 체크리스트

**Phase 5 시작 전 필수 준비**:

**A. 공격 이펙트 (5개)**
- [ ] vfx_slash_sword (칼 궤적, 0.3초)
- [ ] vfx_slash_heavy (강공격 궤적, 0.5초)
- [ ] vfx_fireball (파이어볼, 1초)
- [ ] vfx_lightning (라이트닝, 0.5초)
- [ ] vfx_shockwave (충격파, 0.7초)

**구성**: Sprite Sheet + ParticleSystem
**크기**: 128×128 ~ 256×256px
**프레임**: 8~12프레임

**B. 피격 이펙트 (3개)**
- [ ] vfx_blood_splash (피 튀김, 0.3초)
- [ ] vfx_dust_impact (먼지, 0.5초)
- [ ] vfx_spark (불꽃, 0.2초)

**구성**: ParticleSystem
**파티클 수**: 10~20개
**수명**: 0.3~0.5초

**C. 스킬 이펙트 (5개)**
- [ ] vfx_dash_trail (대시 잔상, 0.3초)
- [ ] vfx_skull_switch (스컬 교체, 1초)
- [ ] vfx_ultimate_charge (궁극기 차징, 2초)
- [ ] vfx_ultimate_activate (궁극기 발동, 1초)
- [ ] vfx_buff_aura (버프 오라, 루프)

**구성**: Sprite Sheet + ParticleSystem + Shader
**크기**: 256×256px
**프레임**: 12~24프레임

**D. 환경 이펙트 (2개)**
- [ ] vfx_fire (불, 루프)
- [ ] vfx_smoke (연기, 루프)

**구성**: ParticleSystem
**파티클 수**: 30~50개
**수명**: 1~2초

**총 VFX 에셋**: 15개

#### 10.3.3 에셋 소스 추천

**무료 에셋 소스**:
1. **Unity Asset Store** (무료 카테고리)
   - https://assetstore.unity.com/
   - 추천: "Free Sound Effects Pack", "Free VFX Pack"

2. **Freesound.org** (사운드 효과)
   - https://freesound.org/
   - 라이센스: CC0 또는 CC BY (크레딧 표기 필요)

3. **OpenGameArt.org** (VFX 스프라이트)
   - https://opengameart.org/
   - 라이센스: CC0, CC BY, OGA-BY

4. **itch.io** (인디 에셋)
   - https://itch.io/game-assets/free
   - 2D VFX, Sound Effects 검색

**유료 에셋 소스** (예산 있는 경우):
1. **Unity Asset Store** (프리미엄)
   - 추천: "Epic Toon FX", "Sound Effects Bundle"
   - 가격: $10~$50

2. **GameDev Market**
   - https://www.gamedevmarket.net/
   - 2D VFX, Sound Packs

3. **Envato Elements** (구독형)
   - 월 $16.50, 무제한 다운로드

**자체 제작 도구** (권장):
1. **Aseprite** (픽셀 아트 / VFX 스프라이트)
   - https://www.aseprite.org/
   - 가격: $19.99 (일회성)

2. **Audacity** (사운드 편집)
   - https://www.audacityteam.org/
   - 무료 오픈소스

3. **BFXR** (8-bit 효과음 생성)
   - https://www.bfxr.net/
   - 무료 웹 도구

#### 10.3.4 에셋 통합 가이드

**사운드 통합**:
```csharp
// AudioManager.cs 예시
public class AudioManager : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private AudioClip mainThemeBGM;
    [SerializeField] private AudioClip combatBGM;
    [SerializeField] private AudioClip bossBGM;

    [Header("SFX")]
    [SerializeField] private AudioClip sfxHitLight;
    [SerializeField] private AudioClip sfxHitHeavy;
    [SerializeField] private AudioClip sfxCritical;

    private AudioSource bgmSource;
    private List<AudioSource> sfxSources = new List<AudioSource>();

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        // SFX Pool에서 사용 가능한 AudioSource 찾기
        AudioSource source = GetAvailableSFXSource();
        source.clip = clip;
        source.volume = volume;
        source.Play();
    }
}
```

**VFX 통합**:
```csharp
// VFXManager.cs 예시
public class VFXManager : MonoBehaviour
{
    [Header("공격 이펙트")]
    [SerializeField] private GameObject vfxSlashSword;
    [SerializeField] private GameObject vfxSlashHeavy;

    [Header("피격 이펙트")]
    [SerializeField] private GameObject vfxBloodSplash;
    [SerializeField] private GameObject vfxDustImpact;

    public void PlayVFX(GameObject vfxPrefab, Vector3 position, Quaternion rotation)
    {
        GameObject vfx = Instantiate(vfxPrefab, position, rotation);
        // 0.5초 후 자동 삭제
        Destroy(vfx, 0.5f);
    }
}
```

#### 10.3.5 Phase 5 시작 전 최종 체크

**에셋 준비 완료 확인**:
- [ ] 사운드 에셋 25개 모두 준비 완료
- [ ] VFX 에셋 15개 모두 준비 완료
- [ ] Unity 프로젝트에 에셋 Import 완료
- [ ] AudioManager 스크립트 작성 완료
- [ ] VFXManager 스크립트 작성 완료

**테스트 확인**:
- [ ] 모든 사운드가 Unity에서 재생되는지 확인
- [ ] 모든 VFX가 Scene에서 표시되는지 확인
- [ ] 사운드 볼륨 밸런스 조정 완료
- [ ] VFX 크기/색상 조정 완료

**라이센스 확인**:
- [ ] 사용한 에셋의 라이센스 기록 (CREDITS.txt)
- [ ] CC BY 라이센스의 경우 크레딧 표기 준비
- [ ] 상업적 사용 가능 여부 확인

---

## 11. 개발 로드맵

### 11.1 Phase 1: Core Systems (완료 ✅)

| 시스템 | 상태 | 완성도 |
|--------|------|--------|
| GAS Core | ✅ | 100% |
| FSM Core | ✅ | 100% |
| GameFlow | ✅ | 100% |
| CharacterPhysics (기본) | ✅ | 70% |
| Combat System (기본) | ✅ | 60% |
| Enemy AI | ✅ | 100% |

### 11.2 Phase 2: Advanced Physics & Combat (진행 중 🔄)

#### 11.2.1 Phase 2.1: CharacterPhysics 완료 기준

**현재 상태**: 85% 진행 중

| 기능 | 상태 | 완성도 | 세부 작업 |
|-----|------|--------|----------|
| 벽 점프/슬라이딩 | ✅ | 100% | - 벽 감지 로직 완료<br>- 슬라이딩 중력 적용 완료<br>- 벽 점프 방향 전환 완료 |
| 낙하 플랫폼 | ✅ | 100% | - 하강 키 입력 처리 완료<br>- 일시적 충돌 해제 완료<br>- 플랫폼 재충돌 방지 완료 |
| 스컬별 이동 특성 | ✅ | 100% | - SkullMovementProfile 구조 완료<br>- 프로필 교체 로직 완료<br>- 배율 적용 시스템 완료 |
| **히트박스/허트박스 정교화** | ⏳ | **30% → 70% 필요** | **남은 작업**:<br>- [ ] 히트박스 크기 조정 (공격별 정확한 범위)<br>- [ ] 허트박스 레이어 분리 (머리/몸통/다리)<br>- [ ] 디버그 시각화 (Gizmos로 박스 표시) |

**Phase 2.1 완료 조건**:
- [x] 벽 점프/슬라이딩이 모든 상황에서 안정적으로 동작
- [x] 낙하 플랫폼 통과 후 재충돌 없음
- [x] Skull 프로필 교체 시 이동 특성 즉시 반영
- [ ] **히트박스 크기가 시각적 공격 범위와 일치** (남은 작업)
- [ ] **허트박스가 캐릭터 스프라이트와 정확히 일치** (남은 작업)
- [ ] **디버그 모드에서 모든 박스 시각화 가능** (남은 작업)

**예상 완료 시점**: 2주 추가 필요 (히트박스 정교화)

#### 11.2.2 Phase 2.2: Combat System 완료 기준

**현재 상태**: 70% 진행 중

| 기능 | 상태 | 완성도 | 세부 작업 |
|-----|------|--------|----------|
| DamageSystem | ✅ | 100% | - 데미지 계산 완료<br>- 크리티컬 시스템 완료<br>- 속성 데미지 완료 |
| HealthSystem | ✅ | 100% | - 체력 관리 완료<br>- 피격 무적 시간 완료<br>- 사망 처리 완료 |
| ComboSystem | ✅ | 100% | - 콤보 카운트 완료<br>- 콤보 타이머 완료<br>- 콤보 리셋 완료 |
| **콤보 체이닝 안정성** | ⏳ | **진행 중** | **최근 버그 수정 이력**:<br>- c28c63c: 어빌리티 하드코딩 제거<br>- ccfa101: isChainStarter 제거<br>- c162936: 체인 진행 조건 변경<br>- bcf681e: 체인 중복/건너뛰기 버그 수정<br><br>**집중 테스트 필요**:<br>- [ ] 3단 콤보가 100% 연결되는가?<br>- [ ] 타이밍 윈도우가 적절한가?<br>- [ ] 체인 중단 시 정상 리셋되는가?<br>- [ ] 다양한 어빌리티 조합 테스트 |

**Phase 2.2 완료 조건**:
- [x] DamageSystem이 모든 데미지 타입 처리
- [x] HealthSystem이 사망/부활 처리
- [x] ComboSystem이 콤보 수 추적
- [ ] **3단 콤보 체이닝이 안정적으로 동작** (집중 테스트 필요)
- [ ] **NextAbilityId 기반 체인 진행이 버그 없음** (최근 수정 사항 검증)
- [ ] **타이밍 윈도우(ChainWindowDuration)가 플레이 테스트 통과** (0.5초가 적절한지 확인)

**예상 완료 시점**: 1주 추가 필요 (콤보 체이닝 안정성 테스트)

#### 11.2.3 Phase 2 전체 완료 조건 요약

**Phase 2 → Phase 3 진입 전 필수**:

1. ✅ **CharacterPhysics 100% 완료**
   - 벽 점프/슬라이딩/낙하 플랫폼 완벽 동작
   - 히트박스/허트박스 정교화 완료
   - Skull 프로필 교체 지원

2. ✅ **Combat System 100% 완료**
   - DamageSystem, HealthSystem, ComboSystem 완료
   - 콤보 체이닝 안정성 검증 완료
   - 최근 버그 수정 사항 전수 테스트 완료

3. ✅ **플레이 가능 버전 제공**
   - Skull 없이도 전체 게임 플레이 가능
   - 적 3~5마리와 전투 가능
   - 체력/데미지 시스템 정상 동작

**Phase 2 완료 없이 Phase 3 진입 금지**: Phase 2가 불완전한 상태에서 Skull 시스템을 추가하면, Skull 교체 시 버그가 복합적으로 발생할 위험이 높습니다.

### 11.3 Phase 3: Skull System (계획 📋)

| 기능 | 우선순위 | 예상 기간 |
|-----|---------|----------|
| DefaultSkull 구현 | P1 | 3일 |
| WarriorSkull 구현 | P1 | 3일 |
| MageSkull 구현 | P2 | 3일 |
| 스컬 교체 UI | P1 | 2일 |
| 스컬 강화 시스템 | P3 | 5일 |

### 11.4 Phase 4: Content (계획 📋)

| 기능 | 우선순위 | 예상 기간 |
|-----|---------|----------|
| 5가지 일반 적 구현 | P1 | 1주 |
| 첫 번째 보스 구현 | P1 | 1주 |
| 첫 번째 던전 (5개 방) | P1 | 1주 |
| 아이템 시스템 | P2 | 1주 |
| 업그레이드 시스템 | P3 | 1주 |

### 11.5 Phase 5: Polish (계획 📋)

| 기능 | 우선순위 | 예상 기간 |
|-----|---------|----------|
| 애니메이션 시스템 | P1 | 2주 |
| VFX 통합 | P1 | 1주 |
| 사운드 통합 | P2 | 1주 |
| UI 완성 | P2 | 1주 |
| 튜토리얼 | P3 | 3일 |

### 11.6 Phase 6: Optimization (계획 📋)

| 기능 | 우선순위 | 예상 기간 |
|-----|---------|----------|
| Object Pool | P1 | 3일 |
| 메모리 최적화 | P2 | 3일 |
| 프레임 최적화 | P2 | 3일 |
| 로딩 최적화 | P3 | 2일 |

---

## 12. 기술 제약사항

### 12.1 코딩 규칙

**엄격한 규칙**:
1. **Coroutine 절대 금지** ❌ → Awaitable 사용 ✅
2. **변수명에 언더스코어(_) 금지** ❌ → camelCase ✅
3. **Unity 6.0+ API 사용** (velocity → linearVelocity)
4. **한글 주석 허용** (UTF-8 인코딩)
5. **파일당 500줄 제한** (초과 시 분할)

**네이밍 컨벤션**:
- 클래스/인터페이스: PascalCase (예: `PlayerController`, `IAbility`)
- 메서드/프로퍼티: PascalCase (예: `ActivateAbility()`, `IsGrounded`)
- 필드: camelCase (예: `abilitySystem`, `currentHealth`)
- 상수: PascalCase 또는 UPPER_SNAKE_CASE

### 12.2 성능 제약

**타겟 스펙**:
- **해상도**: 1920×1080 (Full HD)
- **프레임**: 60 FPS (고정)
- **메모리**: 2GB 이하 (RAM)
- **저장공간**: 500MB 이하

**최적화 전략**:
1. Object Pool 사용 (히트박스, 이펙트)
2. 불필요한 GetComponent 호출 최소화 (캐싱)
3. 프레임당 Physics2D 호출 제한
4. 비동기 로딩 (씬, 리소스)

#### 12.2.1 성능 최적화 목표 (Phase 6 기준)

**동시 처리 한계 (Target Limits)**:

Phase 6 (Optimization) 시작 전에 명확한 성능 목표를 설정해야 합니다. 아래 수치는 **60 FPS 유지를 위한 최대 허용치**입니다.

| 리소스 | 최대 허용 | 권장 | 비고 |
|--------|---------|------|------|
| **동시 활성 적** | 20마리 | 10마리 이하 | 10마리 초과 시 Physics2D 병목 가능 |
| **히트박스** | 50개 | 30개 이하 | Object Pool 필수 (Phase 6 P1) |
| **파티클 이펙트** | 30개 | 20개 이하 | ParticleSystem 재활용 필요 |
| **데미지 폰트** | 15개 | 10개 이하 | 0.5초 수명, 페이드 아웃 후 재활용 |
| **동시 사운드** | 32채널 | 20채널 이하 | Unity AudioSource 제한 |

**프레임 예산 (60 FPS = 16.67ms per frame)**:

| 시스템 | 예산 | 비율 | 최적화 방법 |
|--------|------|------|-----------|
| **물리 계산** (FixedUpdate) | 2ms 이하 | 12% | - Physics2D.OverlapBoxAll 호출 최소화<br>- 적 수 제한<br>- 충돌 레이어 최적화 |
| **렌더링** (Render) | 10ms 이하 | 60% | - 드로우 콜 최소화 (Sprite Atlas)<br>- 오버드로우 방지<br>- 카메라 Culling |
| **스크립트 실행** (Update) | 4ms 이하 | 24% | - Update() 사용 최소화<br>- 이벤트 기반 업데이트<br>- 캐싱 (GetComponent) |
| **여유** | 0.67ms | 4% | 버퍼 |

**Phase 6 최적화 우선순위**:

**P1 (필수 - Phase 6 시작 즉시)**:
1. **Hitbox Object Pool**
   - 문제: HitboxController에서 Instantiate/Destroy 반복
   - 해결: HitboxPool 관리자 추가 (50개 미리 생성)
   - 예상 효과: 스크립트 실행 시간 50% 감소

2. **Enemy Object Pool**
   - 문제: EnemySpawner에서 Instantiate/Destroy 반복
   - 해결: EnemyPool 관리자 추가 (20마리 미리 생성)
   - 예상 효과: 스크립트 실행 시간 30% 감소

**P2 (권장 - Phase 6 중반)**:
3. **파티클 재활용**
   - 문제: 파티클 이펙트 Instantiate 반복
   - 해결: ParticleSystem.Stop() 후 재사용
   - 예상 효과: 렌더링 시간 10% 감소

4. **불필요한 Update 제거**
   - 문제: 매 프레임 체크하는 Update() 다수
   - 해결: 이벤트 기반 업데이트로 전환
   - 예상 효과: 스크립트 실행 시간 20% 감소

**P3 (선택적 - Phase 6 후반)**:
5. **GetComponent 캐싱**
   - 문제: Update()에서 GetComponent 반복 호출
   - 해결: Awake/Start에서 캐싱
   - 예상 효과: 스크립트 실행 시간 10% 감소

6. **Physics2D 호출 최적화**
   - 문제: FixedUpdate에서 OverlapBoxAll 다수 호출
   - 해결: 레이어 마스크 최적화, 호출 빈도 감소
   - 예상 효과: 물리 계산 시간 20% 감소

**성능 측정 도구**:

Unity Profiler를 사용하여 다음 항목을 지속적으로 모니터링합니다:

```csharp
// PerformanceMonitor.cs (디버그 전용)
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceMonitor : MonoBehaviour
{
    private float deltaTime;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = Color.white;

        float fps = 1.0f / deltaTime;
        string text = $"FPS: {fps:F1}\n";
        text += $"Active Enemies: {FindObjectsOfType<EnemyController>().Length}\n";
        text += $"Hitboxes: {FindObjectsOfType<HitboxController>().Length}\n";
        text += $"Particles: {FindObjectsOfType<ParticleSystem>().Length}\n";
        text += $"Memory: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024} MB";

        GUI.Label(rect, text, style);
    }
}
#endif
```

**Phase 6 완료 기준**:

- [ ] 20마리 적 동시 활성 시 60 FPS 유지
- [ ] 50개 히트박스 동시 생성 시 프레임 드롭 없음
- [ ] 30개 파티클 이펙트 동시 재생 시 60 FPS 유지
- [ ] Unity Profiler에서 프레임 예산 준수 확인
- [ ] 메모리 사용량 2GB 이하 유지

**최적화 실패 시 대안**:

만약 위 최적화로도 60 FPS를 유지할 수 없다면:
1. **동시 적 수 감소**: 20마리 → 15마리
2. **파티클 품질 하향**: High → Medium
3. **해상도 옵션 추가**: 1920×1080 → 1280×720
4. **프레임 제한 완화**: 60 FPS → 45 FPS (최후의 수단)

### 12.3 플랫폼 제약

**현재 지원**:
- Windows PC (x64)

**향후 지원 검토**:
- macOS
- Linux
- Nintendo Switch (최적화 필요)

---

## 부록

### A. 용어 사전

| 용어 | 설명 |
|-----|------|
| **GAS** | Generic Ability System (범용 어빌리티 시스템) |
| **FSM** | Finite State Machine (유한상태머신) |
| **Skull** | 플레이어가 교체 가능한 캐릭터 클래스 |
| **Coyote Time** | 플랫폼에서 벗어난 후 점프 가능한 시간 |
| **Jump Buffer** | 점프 입력을 미리 저장하는 시간 |
| **Hit Stop** | 타격 시 시간 지연 효과 |
| **Knockback** | 피격 시 밀리는 효과 |
| **ScriptableObject** | Unity의 데이터 에셋 타입 |
| **Awaitable** | Unity 6.0+ 비동기 패턴 |

### B. 참고 게임

- **Skul: The Hero Slayer**: 스컬 교체 메커니즘
- **Dead Cells**: 유동적인 전투, 콤보 시스템
- **Hollow Knight**: 정밀한 플랫포밍, 벽 점프
- **Celeste**: 대시 메커니즘, 코요테 타임

### C. 관련 문서

- [Architecture Overview](./architecture/ARCHITECTURE_OVERVIEW.md)
- [GAS Core Documentation](./reference/GAS_CORE_REFERENCE.md)
- [FSM Core Documentation](./reference/FSM_CORE_REFERENCE.md)
- [CharacterPhysics Implementation](./specs/001-character-physics-completion/)
- [Coding Standards](./ENCODING_GUIDE.md)

---

**문서 버전**: 1.0
**최종 수정**: 2025-10-29
**작성자**: Claude (AI) + 프로젝트 분석
**검토자**: 검증 대기 중
