# Platformer Enemy System

**작성일**: 2025-11-10
**Phase**: A-2 (Enemy AI + Combat Integration)
**상태**: 완료 ✅

---

## 📖 개요

플랫포머 로그라이크용 적 AI 시스템입니다. 기존 RPG Enemy 시스템을 상속받아 데미지/드롭 로직을 재사용하며, FSM 기반 플랫포머 AI를 추가했습니다.

---

## 🏗️ 아키텍처

### 클래스 계층

```
Enemy.cs (기존 RPG 시스템)
    ├── TakeDamage(), Die()
    ├── 골드/EXP/아이템 드롭
    ├── StatusEffect 통합
    └── DamageNumber 표시
        ↓
PlatformerEnemy.cs (플랫포머 베이스)
    ├── Rigidbody2D 물리 이동
    ├── 플레이어 감지
    ├── FSM 상태 관리
    └── 가상 메서드 제공
        ↓
BasicMeleeEnemy.cs (구체적 구현)
    └── FSM: Idle → Patrol → Chase → Attack → Dead
```

---

## 📋 주요 파일

### 1. EnemyData.cs (확장됨)
기존 RPG 데이터에 플랫포머용 필드 추가:

```csharp
[Header("플랫포머 설정")]
public float moveSpeed = 2f;           // 기본 이동 속도
public float detectionRange = 5f;      // 플레이어 감지 거리
public float attackRange = 1.5f;       // 공격 범위
public float patrolDistance = 3f;      // 순찰 거리
public float chaseSpeed = 3f;          // 추격 속도
public float attackCooldown = 1.5f;    // 공격 쿨다운
```

### 2. PlatformerEnemy.cs (293줄)
추상 베이스 클래스:

**주요 기능**:
- Rigidbody2D 기반 물리 이동
- 플레이어 감지 (거리 계산)
- FSM 상태 관리 (ChangeState, OnStateEnter/Exit)
- Move(), Stop(), Flip() - 이동 제어
- AttackPlayer() - 기존 Enemy.DealDamageTo() 활용
- Gizmos 디버그 시각화

**가상 메서드**:
```csharp
protected virtual void OnStateEnter(EnemyState state)
protected virtual void OnStateExit(EnemyState state)
protected virtual void UpdateState()
protected virtual void PhysicsUpdate()
```

### 3. BasicMeleeEnemy.cs (300줄)
근접 공격 적 구현:

**FSM 상태 전환**:
```
Idle (0.5초 대기)
  ↓
Patrol (순찰 중)
  ↓ 플레이어 감지
Chase (추격)
  ↓ 공격 범위 진입
Attack (공격)
  ↓ HP 0
Dead (사망)
```

**특수 기능**:
- 순찰 중 낭떠러지 감지 (Raycast로 바닥 체크)
- 순찰 범위 제한 (patrolDistance 기반)
- 공격 쿨다운 관리
- Context Menu 디버그 메서드

---

## 🎮 MageForm 스킬 연동

### MagicMissileAbility.cs
- **방식**: Raycast로 즉시 타격
- **데미지**: 10
- **범위**: 10m
- **효과**: Enemy.TakeDamage() 호출 → DamageNumber 표시

### FireballAbility.cs
- **방식**: Physics2D.OverlapCircleAll로 범위 탐색
- **데미지**: 50 (직격 + 폭발)
- **범위**: 3m 반경
- **효과**: 범위 내 모든 Enemy에 데미지

### TeleportAbility.cs
- 데미지 없음 (이동만)

---

## 🧪 테스트 방법

### 1. Unity 에디터 셋업

#### Step 1: EnemyData 생성
```
Assets 폴더에서 우클릭:
Create > GASPT > Enemies > Enemy

설정 예시:
- enemyName: "Goblin"
- enemyType: Normal
- maxHp: 30
- attack: 5
- moveSpeed: 2
- detectionRange: 5
- attackRange: 1.5
- patrolDistance: 3
- chaseSpeed: 3
- attackCooldown: 1.5
```

#### Step 2: BasicMeleeEnemy GameObject 생성
```
1. Hierarchy에서 우클릭 > Create Empty
2. 이름: "Goblin"
3. 컴포넌트 추가:
   - BasicMeleeEnemy (스크립트)
   - Rigidbody2D
     - Gravity Scale: 2
     - Freeze Rotation: Z 체크
   - BoxCollider2D (또는 CapsuleCollider2D)
   - SpriteRenderer (선택사항)

4. BasicMeleeEnemy 설정:
   - Enemy Data: 위에서 만든 EnemyData 할당
   - Ground Layer: Default (또는 Ground 레이어)
   - Edge Check Distance: 0.5
   - Show Debug Logs: true (테스트용)
   - Show Gizmos: true
```

#### Step 3: MageForm 플레이어 생성
```
1. Hierarchy에서 우클릭 > Create Empty
2. 이름: "Player"
3. 컴포넌트 추가:
   - MageForm (스크립트)
   - PlayerStats (스크립트) - 기존 RPG 시스템
   - Rigidbody2D
   - BoxCollider2D
   - SpriteRenderer (선택사항)

4. FormData 생성:
   Create > GASPT > Form > Form Data
   - formName: "Mage"
   - formType: Mage
   - maxHealth: 100
   - moveSpeed: 5
   - jumpPower: 10

5. MageForm 설정:
   - Form Data: 위에서 만든 FormData 할당
```

#### Step 4: 플랫폼/바닥 생성
```
1. Hierarchy > Create > 2D Object > Sprite > Square
2. 이름: "Ground"
3. 스케일 조정 (예: X=20, Y=1)
4. BoxCollider2D 추가
5. 레이어 설정 (선택사항)
```

#### Step 5: 카메라 설정
```
Main Camera 설정:
- Projection: Orthographic
- Size: 5~10 (적절히 조정)
- 위치: Player 높이에 맞춤
```

---

### 2. 테스트 시나리오

#### 테스트 1: Patrol 상태
1. Play 모드 진입
2. Scene 뷰에서 Goblin 관찰
3. **기대 동작**:
   - Idle 0.5초 후 Patrol 상태로 전환
   - 좌우로 순찰 (patrolDistance 범위 내)
   - 낭떠러지 감지 시 방향 전환
   - Gizmos로 순찰 범위 확인 (파란색 선)

#### 테스트 2: Chase 상태
1. Player를 Goblin 근처로 이동 (detectionRange 안)
2. **기대 동작**:
   - Patrol → Chase 상태 전환
   - Goblin이 Player를 추격
   - Gizmos로 감지 범위 확인 (노란색 원)

#### 테스트 3: Attack 상태
1. Player를 Goblin 공격 범위 안으로 이동
2. **기대 동작**:
   - Chase → Attack 상태 전환
   - attackCooldown마다 공격 실행
   - PlayerStats.TakeDamage() 호출
   - DamageNumber 표시 (빨간색)

#### 테스트 4: MagicMissile 공격
1. Play 모드에서 Player 선택
2. MageForm에서 우클릭 > Test Magic Missile
   - **또는** 코드로 스킬 실행
3. **기대 동작**:
   - 마우스 방향으로 Raycast
   - Goblin 타격 시 데미지 10
   - DamageNumber 표시
   - Goblin HP 감소

#### 테스트 5: Fireball 범위 공격
1. Goblin 2~3마리 가까이 배치
2. Fireball 스킬 실행
3. **기대 동작**:
   - 폭발 위치에서 3m 반경 탐색
   - 범위 내 모든 Goblin에 데미지 50
   - DamageNumber 여러 개 표시
   - Scene 뷰에 노란색 십자가 표시 (2초)

#### 테스트 6: Enemy 사망
1. Goblin HP를 0으로 만들기 (스킬 공격 반복)
2. **기대 동작**:
   - Die() 호출
   - 골드 드롭 (CurrencySystem)
   - EXP 지급 (PlayerLevel)
   - 아이템 드롭 (LootSystem, lootTable 설정 시)
   - EXP Number 표시 (파란색)
   - 1초 후 GameObject 파괴

---

### 3. Context Menu 디버그

#### Enemy 우클릭 메뉴:
```
- Print Enemy Info          - Enemy 기본 정보
- Take 10 Damage (Test)     - 10 데미지 받기
- Instant Death (Test)      - 즉시 사망

- Print Platformer Info     - 플랫포머 정보
- Print BasicMelee Info     - FSM 상태 정보

- Force State: Idle         - 강제로 Idle 상태로
- Force State: Patrol       - 강제로 Patrol 상태로
- Force State: Chase        - 강제로 Chase 상태로
- Force State: Attack       - 강제로 Attack 상태로
```

#### MageForm 우클릭 메뉴:
```
- Print Form Info           - Form 정보
- Test Magic Missile        - Magic Missile 테스트 (미구현)
- Test Teleport             - Teleport 테스트 (미구현)
- Test Fireball             - Fireball 테스트 (미구현)
```

---

## 🔧 주요 설정 값

### 권장 EnemyData 설정

| 적 타입 | HP | Attack | MoveSpeed | DetectionRange | AttackRange | ChaseSpeed | AttackCooldown |
|---------|-----|--------|-----------|----------------|-------------|------------|----------------|
| 약한 고블린 | 20 | 3 | 1.5 | 4 | 1.2 | 2.5 | 1.5 |
| 일반 고블린 | 30 | 5 | 2 | 5 | 1.5 | 3 | 1.5 |
| 강한 오크 | 50 | 10 | 1.8 | 6 | 1.8 | 2.8 | 2 |

### 낭떠러지 체크 설정
- **Ground Layer**: 바닥으로 사용할 레이어 (Default 또는 Ground)
- **Edge Check Distance**: 0.5~1.0 (바닥 체크 거리)
- **Edge Check Offset**: (0.5, -0.5) - 적 앞쪽 바닥 체크

---

## ⚠️ 주의사항

### 1. Rigidbody2D 필수 설정
```csharp
rb.gravityScale = 2f;
rb.freezeRotation = true;  // Z축 회전 고정
rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
```

### 2. Collider Layer 설정
- Enemy와 Player가 서로 충돌하지 않도록 Layer 설정 필요
- Edit > Project Settings > Physics 2D > Layer Collision Matrix

### 3. PlayerStats 필수
- PlatformerEnemy.FindPlayer()는 PlayerStats를 찾음
- Player GameObject에 PlayerStats 컴포넌트 필수

### 4. 현재 제한사항
- **투사체 미구현**: MagicMissile, Fireball이 즉시 Raycast/OverlapCircle 사용
- **플레이어 입력 미구현**: 스킬 실행이 수동 (Context Menu)
- **애니메이션 미구현**: 스프라이트 변경 없음
- **이펙트 미구현**: 폭발, 타격 이펙트 없음

---

## 📝 TODO

### Phase A-2 완료 항목 ✅
- [x] EnemyData 플랫포머 필드 추가
- [x] PlatformerEnemy 베이스 클래스
- [x] BasicMeleeEnemy FSM 구현
- [x] MagicMissile Enemy 데미지 연동
- [x] Fireball 범위 데미지 연동
- [x] 골드/EXP/아이템 드롭 통합 (기존 시스템 재사용)

### Phase A-3 (예정)
- [ ] Room System (방 단위 레벨)
- [ ] 적 스폰 포인트
- [ ] 방 클리어 조건
- [ ] 포탈 (다음 방 이동)

### 개선 사항
- [ ] 투사체 프리팹 생성 (Magic Missile, Fireball)
- [ ] 플레이어 입력 시스템 (InputSystem)
- [ ] 애니메이션 통합 (Animator)
- [ ] 이펙트 프리팹 (폭발, 타격, 텔레포트)
- [ ] 사운드 이펙트
- [ ] Enemy 다양화 (원거리, 보스 등)

---

## 🐛 알려진 이슈

### 1. PlatformerEnemy.OnDrawGizmosSelected() 에디터 전용
```csharp
// UnityEditor 네임스페이스 사용으로 빌드 에러 가능
// 해결: #if UNITY_EDITOR 가드 추가 필요
```

### 2. FindAnyObjectByType<PlayerStats>() 성능
- 매 프레임 호출 아님 (Start에서 한 번만)
- 하지만 Scene에 PlayerStats가 여러 개면 문제
- 해결: Singleton 패턴 또는 Tag 검색

### 3. 낭떠러지 체크 Layer 의존성
- Ground Layer 설정 필수
- 잘못된 Layer 설정 시 낭떠러지 감지 실패

---

## 📚 참고 문서

- **Enemy.cs** - 기존 RPG Enemy 시스템
- **EnemyData.cs** - 적 데이터 ScriptableObject
- **Form/README.md** - MageForm 시스템 가이드
- **FORM_PLATFORMER_IMPLEMENTATION_PLAN.md** - 전체 구현 계획

---

**최종 업데이트**: 2025-11-10
**작성자**: Phase A-2 Implementation
