# Phase A 통합 테스트 가이드

**작성일**: 2025-11-10
**대상 Phase**: A-1 (MageForm), A-2 (Enemy AI), A-3 (Room System)
**테스트 목적**: 3개 Phase의 개별 기능 검증 + 통합 동작 확인
**예상 소요 시간**: 약 2-3시간

---

## 📋 목차

1. [개요](#개요)
2. [테스트 환경 구축](#테스트-환경-구축)
3. [Phase A-1 테스트: MageForm 시스템](#phase-a-1-테스트-mageform-시스템)
4. [Phase A-2 테스트: Enemy AI](#phase-a-2-테스트-enemy-ai)
5. [Phase A-3 테스트: Room System](#phase-a-3-테스트-room-system)
6. [통합 테스트: 전체 시스템 연동](#통합-테스트-전체-시스템-연동)
7. [트러블슈팅](#트러블슈팅)
8. [테스트 체크리스트](#테스트-체크리스트)

---

## 개요

### 테스트 대상 시스템

| Phase | 시스템 | 주요 기능 | 파일 수 |
|-------|--------|----------|---------|
| **A-1** | MageForm | 폼 전환, 스킬 시스템 (Magic Missile, Teleport, Fireball) | 7개 (607줄) |
| **A-2** | Enemy AI | 플랫포머 적 AI, FSM (Idle/Patrol/Chase/Attack), 데미지/드롭 | 3개 (1,225줄) |
| **A-3** | Room System | 방 단위 던전, 적 스폰, 클리어 조건, 포탈 이동 | 5개 (1,083줄) |

### 테스트 목표

- ✅ **개별 기능 검증**: 각 Phase의 핵심 기능이 정상 작동하는지 확인
- ✅ **통합 동작 검증**: 3개 시스템이 충돌 없이 연동되는지 확인
- ✅ **버그 발견**: 예상치 못한 동작, 크래시, 누락된 기능 확인
- ✅ **사용성 평가**: 실제 플레이 느낌, 컨트롤 반응성 체크

---

## 테스트 환경 구축

### 1. 사전 준비

#### 필수 파일 확인

```bash
# Unity 에디터에서 확인할 파일 목록
Assets/_Project/Scripts/Gameplay/Form/
  ├── Core/
  │   ├── BaseForm.cs
  │   ├── IFormController.cs
  ├── Implementations/
  │   ├── MageForm.cs
  ├── Abilities/
  │   ├── MagicMissileAbility.cs
  │   ├── TeleportAbility.cs
  │   ├── FireballAbility.cs
  ├── FormInputHandler.cs

Assets/_Project/Scripts/Gameplay/Enemy/
  ├── PlatformerEnemy.cs
  ├── BasicMeleeEnemy.cs

Assets/_Project/Scripts/Gameplay/Level/
  ├── Room/
  │   ├── Room.cs
  │   ├── Portal.cs
  │   ├── EnemySpawnPoint.cs
  ├── Manager/
  │   ├── RoomManager.cs
  ├── Data/
      ├── RoomData.cs

Assets/_Project/Scripts/Gameplay/Player/
  ├── PlayerController.cs

Assets/_Project/Scripts/Gameplay/Camera/
  ├── CameraFollow.cs
```

**확인 방법**: Unity 에디터에서 각 파일을 더블클릭 → 컴파일 에러 없는지 확인

---

### 2. Test Scene 생성

#### Step 1: Scene 생성 및 기본 설정

```
1. Unity 메뉴: File > New Scene > 2D (Template)
2. 이름: "IntegrationTestScene"
3. 저장 위치: Assets/_Project/Scenes/IntegrationTestScene.unity
4. Scene 저장 (Ctrl+S)
```

#### Step 2: Ground 생성

```
Hierarchy 우클릭 > 2D Object > Sprites > Square

GameObject: "Ground"
────────────────────────────────────
Transform:
  Position: (0, -3, 0)
  Rotation: (0, 0, 0)
  Scale: (30, 1, 1)  ← 넓은 바닥

Add Component: BoxCollider2D
  (기본 설정 유지)

SpriteRenderer:
  Color: Gray (0.5, 0.5, 0.5)

Layer: Default
```

**결과 확인**: Scene 뷰에서 긴 회색 바닥이 보여야 함

---

### 3. EnemyData 생성 (중요!)

Phase A-2, A-3 테스트에 필수입니다.

#### Step 1: EnemyData ScriptableObject 생성

```
Assets 폴더에서 우클릭
> Create > GASPT > Enemy > Enemy Data

이름: "TestGoblin"
저장 위치: Assets/_Project/Data/Enemies/TestGoblin.asset
```

#### Step 2: EnemyData 설정

```
TestGoblin.asset 선택 > Inspector 설정:

[기본 정보]
enemyName: "Test Goblin"
enemyType: Normal
level: 1

[스탯]
maxHealth: 30
attackPower: 5
defense: 0
moveSpeed: 2

[플랫포머 설정] ← 중요!
moveSpeed: 2
detectionRange: 5
attackRange: 1.5
patrolDistance: 3
chaseSpeed: 3
attackCooldown: 1.5

[드롭 보상]
goldReward: 10
expReward: 5
```

**저장**: Ctrl+S

---

### 4. RoomData 생성

Phase A-3 테스트에 필수입니다.

#### Step 1: RoomData ScriptableObject 생성

```
Assets 폴더에서 우클릭
> Create > GASPT > Level > Room Data

이름: "TestRoom_Normal"
저장 위치: Assets/_Project/Data/Rooms/TestRoom_Normal.asset
```

#### Step 2: RoomData 설정

```
TestRoom_Normal.asset 선택 > Inspector 설정:

[방 정보]
roomName: "Test Room - Normal"
roomType: Normal
difficulty: 2

[적 스폰 설정]
minEnemyCount: 2
maxEnemyCount: 4

Enemy Spawns (배열):
  Size: 1
  Element 0:
    enemyData: TestGoblin (위에서 만든 것)
    spawnChance: 1.0
    minCount: 2
    maxCount: 3

[클리어 조건]
clearCondition: KillAllEnemies
timeLimit: 0 (무제한)

[보상]
bonusGold: 50
bonusExp: 20
```

**저장**: Ctrl+S

---

## Phase A-1 테스트: MageForm 시스템

### 목표

- ✅ MageForm 활성화 및 스킬 슬롯 초기화 확인
- ✅ 3가지 스킬(Magic Missile, Teleport, Fireball) 실행 확인
- ✅ 쿨다운 시스템 작동 확인

---

### 테스트 환경 구축

#### 1. Player GameObject 생성

```
Hierarchy 우클릭 > Create Empty

GameObject: "TestPlayer"
Tag: "Player" ← 매우 중요!
────────────────────────────────────
Transform:
  Position: (0, 0, 0)
  Rotation: (0, 0, 0)
  Scale: (1, 1, 1)

Add Component: Rigidbody2D
  Body Type: Dynamic
  Mass: 1
  Linear Drag: 0
  Angular Drag: 0.05
  Gravity Scale: 3
  Collision Detection: Continuous
  Sleeping Mode: Never Sleep
  Interpolate: Interpolate
  Constraints:
    - Freeze Rotation Z: ✓

Add Component: BoxCollider2D
  Size: (0.8, 1.5)
  Offset: (0, 0)

Add Component: SpriteRenderer
  Sprite: Square (임시)
  Color: Blue (0, 0, 1, 1)
  Sorting Layer: Default
  Order in Layer: 0
```

#### 2. MageForm 컴포넌트 추가

```
TestPlayer 선택 > Add Component 검색: "MageForm"

MageForm (Script) 설정:
  Form Data: null (나중에 ScriptableObject 만들면 할당)
  Magic Aura Effect: null (선택사항)
  Magic Color: (0.5, 0.5, 1, 1) - 파란색
  Show Debug Logs: ✓ ← 체크 필수!
```

#### 3. FormInputHandler 추가

```
TestPlayer 선택 > Add Component 검색: "FormInputHandler"

FormInputHandler (Script) 설정:
  Target Form: 자동 탐색됨 (null로 두면 MageForm 자동 찾음)
  Basic Attack Key: Mouse0 (기본값)
  Skill1 Key: Q (기본값)
  Skill2 Key: E (기본값)
  Show Debug Logs: ✓ ← 체크 필수!
```

#### 4. Main Camera 설정

```
Main Camera 선택

Camera 컴포넌트:
  Projection: Orthographic
  Size: 7
  Clipping Planes: Near 0.3, Far 1000
  Culling Mask: Everything

Transform:
  Position: (0, 0, -10)
```

---

### 테스트 시나리오

#### 테스트 1: MageForm 초기화 확인

```
[실행]
1. Play 모드 진입 (Play 버튼 클릭)
2. Console 창 확인 (Ctrl+Shift+C)

[기대 결과]
Console에 다음 로그가 출력되어야 함:
  "[MageForm] 기본 스킬 초기화 완료"
  "[MageForm] 마법사 폼 활성화: 빠른 이동, 마법 공격 특화"

[검증]
TestPlayer 선택 > Inspector > MageForm (Script) 우클릭
> Context Menu > "Print Form Info"

Console 출력:
  === Mage Info ===
  Type: Mage
  Abilities:
    [0] Magic Missile
    [1] Teleport
    [2] Fireball
    [3] Empty
```

**✅ 성공 조건**: 3개 스킬이 모두 등록되어 있음

---

#### 테스트 2: Magic Missile (기본 공격)

```
[실행]
1. Play 모드 진행 중
2. Scene 뷰를 보면서 마우스 커서를 플레이어 오른쪽에 위치
3. 마우스 좌클릭 (Left Click)

[기대 결과]
Console 출력:
  "[MagicMissile] 발사! 방향: (0.XX, 0.XX)"
  "[MagicMissile] 투사체 발사 - 데미지: 10, 속도: 15, 범위: 10m"

Scene 뷰에서 Debug.DrawRay로 시안색 광선이 표시됨 (1초간)

[검증]
- 마우스 방향으로 광선이 발사되는지 확인
- 0.5초 쿨다운 (빠르게 연속 클릭 시 "쿨다운 중..." 메시지)

[추가 테스트]
- 마우스를 왼쪽, 위, 아래 등 다양한 방향에 두고 클릭
- 각각 다른 방향으로 발사되는지 확인
```

**✅ 성공 조건**:
- 마우스 방향으로 즉시 발사
- 쿨다운 0.5초 작동
- Debug Ray 표시됨

---

#### 테스트 3: Teleport (스킬 1)

```
[실행]
1. Play 모드 진행 중
2. 마우스 커서를 플레이어로부터 약간 떨어진 위치에 놓기
3. Q키 입력

[기대 결과]
Console 출력:
  "[Teleport] 순간이동! (0, 0, 0) → (X, Y, 0)"
  "[Teleport] 텔레포트 완료 - 거리: 5m"

플레이어가 즉시 마우스 방향으로 5m 이동

[검증]
- 플레이어 위치가 실제로 변경되었는지 확인
- Scene 뷰에서 마젠타색 Debug Line 표시 확인

[추가 테스트]
- 3초 쿨다운 확인 (Q키 연타 시 "쿨다운 중..." 메시지)
- 다양한 방향으로 텔레포트 시도
```

**✅ 성공 조건**:
- 마우스 방향으로 정확히 5m 이동
- 쿨다운 3초 작동
- 벽이나 장애물 무시 (임시 구현)

---

#### 테스트 4: Fireball (스킬 2)

```
[실행]
1. Play 모드 진행 중
2. E키 입력

[기대 결과]
Console 출력:
  "[Fireball] 화염구 발사! 방향: (X, Y)"
  "[Fireball] 폭발 - 반경: 2m"

(적이 없으므로 데미지는 없음)

[검증]
- Console에 발사 로그 확인
- 5초 쿨다운 확인 (E키 연타 시 "쿨다운 중...")

[추가 테스트]
- 5초 기다린 후 재사용 가능한지 확인
```

**✅ 성공 조건**:
- E키로 발동
- 쿨다운 5초 작동
- Console 로그 출력

---

#### 테스트 5: FormInputHandler Context Menu

```
[실행]
TestPlayer 선택 > FormInputHandler 우클릭

1. "Print Current Form" 클릭
   Console 출력:
     === FormInputHandler ===
     Form: Mage
     Type: Mage
     Abilities:
       [0] Magic Missile
       [1] Teleport
       [2] Fireball
     Key Bindings:
       Basic Attack: Mouse0
       Skill 1: Q
       Skill 2: E

2. "Test Basic Attack (Slot 0)" 클릭
   → Magic Missile 발동 확인

3. "Test Skill 1 (Slot 1)" 클릭
   → Teleport 발동 확인

4. "Test Skill 2 (Slot 2)" 클릭
   → Fireball 발동 확인
```

**✅ 성공 조건**: Context Menu로 모든 스킬 실행 가능

---

### Phase A-1 테스트 결과 요약

| 항목 | 예상 결과 | 실제 결과 | 상태 |
|------|----------|----------|------|
| MageForm 초기화 | 3개 스킬 등록 | | ⬜ |
| Magic Missile 발사 | 마우스 방향 즉시 발사 | | ⬜ |
| Magic Missile 쿨다운 | 0.5초 | | ⬜ |
| Teleport 이동 | 5m 순간이동 | | ⬜ |
| Teleport 쿨다운 | 3초 | | ⬜ |
| Fireball 발사 | Console 로그 출력 | | ⬜ |
| Fireball 쿨다운 | 5초 | | ⬜ |
| FormInputHandler | 키 입력 정상 작동 | | ⬜ |

**다음 단계**: 모든 항목이 ✅이면 Phase A-2 테스트로 진행

---

## Phase A-2 테스트: Enemy AI

### 목표

- ✅ BasicMeleeEnemy 스폰 및 FSM 작동 확인
- ✅ Idle → Patrol → Chase → Attack 상태 전환 확인
- ✅ MageForm 스킬로 적 데미지/사망 확인
- ✅ 골드/EXP 드롭 확인

---

### 테스트 환경 구축

#### 1. Enemy GameObject 생성

```
Hierarchy 우클릭 > Create Empty

GameObject: "TestEnemy"
────────────────────────────────────
Transform:
  Position: (5, 0, 0)  ← 플레이어로부터 5m 떨어진 곳
  Rotation: (0, 0, 0)
  Scale: (1, 1, 1)

Add Component: Rigidbody2D
  Body Type: Dynamic
  Mass: 1
  Gravity Scale: 3
  Freeze Rotation Z: ✓
  Collision Detection: Continuous
  Interpolate: Interpolate

Add Component: BoxCollider2D
  Size: (0.8, 1.2)

Add Component: SpriteRenderer
  Sprite: Square (임시)
  Color: Red (1, 0, 0, 1)

Add Component: BasicMeleeEnemy
  (스크립트 검색: "BasicMeleeEnemy")
```

#### 2. BasicMeleeEnemy 설정

```
TestEnemy 선택 > BasicMeleeEnemy (Script)

Show Debug Logs: ✓ ← 체크 필수!

※ enemyData는 코드에서 자동 설정되므로 비워둠
  (EnemySpawnPoint.CreateEnemyFromData()가 Reflection으로 설정)

임시 수동 설정:
  BasicMeleeEnemy.cs 파일을 열어 Start()에 추가:

  private void Start()
  {
      // 임시: TestGoblin 데이터 로드
      enemyData = Resources.Load<EnemyData>("Data/Enemies/TestGoblin");
      if (enemyData == null)
      {
          Debug.LogError("[BasicMeleeEnemy] TestGoblin을 찾을 수 없습니다!");
      }
  }
```

**중요**: 위 코드 수정이 번거로우면, Inspector에서 직접 설정:

```
TestEnemy 선택 > 우클릭 > Debug 모드 전환

BasicMeleeEnemy (Script):
  enemyData: TestGoblin (Assets/_Project/Data/Enemies/TestGoblin) 드래그

다시 Normal 모드로 전환
```

---

### 테스트 시나리오

#### 테스트 1: Enemy 스폰 및 Idle 상태

```
[실행]
1. Play 모드 진입
2. TestEnemy가 생성되는지 확인

[기대 결과]
Console 출력:
  "[BasicMeleeEnemy] Test Goblin 초기화 완료"
  "[PlatformerEnemy] Test Goblin 상태 변경: Idle → Patrol"

TestEnemy가 빨간색 사각형으로 표시됨

[검증]
TestEnemy 선택 > Inspector에서 Current State 확인
  → "Patrol" 또는 "Idle"로 표시되어야 함
```

**✅ 성공 조건**: Enemy가 정상 스폰되고 Console에 로그 출력

---

#### 테스트 2: Patrol 상태 (순찰)

```
[실행]
1. Play 모드에서 TestEnemy 관찰
2. 플레이어를 멀리 떨어뜨림 (10m 이상)

[기대 결과]
- TestEnemy가 좌우로 천천히 이동 (2m/s)
- 일정 거리 (3m) 이동 후 방향 전환
- Console에 "[BasicMeleeEnemy] 순찰 방향 전환" 로그

[검증]
- 적이 절벽 끝에서 멈추는지 확인 (IsEdgeAhead() 동작)
- SpriteRenderer가 좌우 반전되는지 확인 (flipX)

[추가 테스트]
- Ground 끝에 Enemy 배치 → 떨어지지 않고 방향 전환하는지 확인
```

**✅ 성공 조건**:
- 좌우 순찰 동작
- 절벽에서 방향 전환
- 일정 거리 후 자동 방향 전환

---

#### 테스트 3: Chase 상태 (추적)

```
[실행]
1. Play 모드에서 TestPlayer를 TestEnemy 가까이 이동 (5m 이내)
2. WASD로 좌우 이동하며 관찰

[기대 결과]
Console 출력:
  "[PlatformerEnemy] Test Goblin 상태 변경: Patrol → Chase"

TestEnemy가 플레이어를 향해 빠르게 이동 (3m/s)

[검증]
- 플레이어 위치에 따라 적이 좌우로 방향 전환
- Chase 속도가 Patrol보다 빠른지 확인
- 플레이어가 멀어지면 다시 Patrol로 전환

[추가 테스트]
- 플레이어가 점프해도 추적하는지 확인
- 플레이어가 10m 이상 멀어지면 Chase 해제되는지 확인
```

**✅ 성공 조건**:
- 5m 이내 접근 시 Chase 시작
- 플레이어 추적 동작
- 10m 이상 멀어지면 Patrol 복귀

---

#### 테스트 4: Attack 상태 (공격)

```
[실행]
1. Play 모드에서 TestPlayer를 TestEnemy 매우 가까이 이동 (1.5m 이내)
2. 가만히 있으면서 관찰

[기대 결과]
Console 출력:
  "[PlatformerEnemy] Test Goblin 상태 변경: Chase → Attack"
  "[PlatformerEnemy] Test Goblin이(가) Player에게 5 데미지!"
  (1.5초 후 재공격)

[검증]
- PlayerStats의 HP가 감소하는지 확인
  (TestPlayer에 PlayerStats 컴포넌트 필요)
- 1.5초 쿨다운 후 재공격

[주의]
TestPlayer에 PlayerStats가 없으면 에러 발생 가능
→ 임시 대처: Console에 "데미지!" 로그만 확인
```

**✅ 성공 조건**:
- 1.5m 이내 공격 시작
- 1.5초마다 반복 공격
- Console에 데미지 로그

---

#### 테스트 5: MagicMissile로 적 처치

```
[실행]
1. Play 모드에서 TestEnemy를 마우스로 조준
2. 마우스 좌클릭으로 Magic Missile 발사 (3회)

[기대 결과]
1회째:
  "[MagicMissile] Test Goblin에 10 데미지!"
  "[Enemy] Test Goblin: 20/30 HP"

2회째:
  "[Enemy] Test Goblin: 10/30 HP"

3회째:
  "[Enemy] Test Goblin: 0/30 HP - 사망!"
  "[PlatformerEnemy] Test Goblin 상태 변경: Chase → Dead"
  "[Enemy] Test Goblin 사망 - 골드 10, 경험치 5 드롭"

TestEnemy GameObject가 파괴됨 (Hierarchy에서 사라짐)

[검증]
- HP가 정확히 감소하는지 확인
- 사망 시 GameObject 파괴 확인
- Console에 골드/EXP 드롭 로그 확인
```

**✅ 성공 조건**:
- 데미지 정상 적용 (10 x 3 = 30)
- HP 0 도달 시 Dead 상태
- 골드 10, EXP 5 드롭
- GameObject 파괴

---

#### 테스트 6: Fireball AOE 데미지

```
[실행]
1. Play 모드에서 TestEnemy를 2-3개 복제 (가까이 배치)
2. 마우스를 적들 중앙에 위치
3. E키로 Fireball 발사

[기대 결과]
Console 출력:
  "[Fireball] 화염구 발사! 방향: (X, Y)"
  "[Fireball] 폭발 - 반경: 2m"
  "[MagicMissile] Test Goblin에 50 데미지!" (반경 내 모든 적)

반경 2m 내 모든 적이 즉사 (50 데미지 >> 30 HP)

[검증]
- AOE 범위 확인 (2m 반경)
- 범위 밖 적은 데미지 안 받는지 확인
```

**✅ 성공 조건**:
- AOE 반경 2m 작동
- 범위 내 모든 적 데미지
- 범위 밖 적은 무사

---

### Phase A-2 테스트 결과 요약

| 항목 | 예상 결과 | 실제 결과 | 상태 |
|------|----------|----------|------|
| Enemy 스폰 | 정상 생성 | | ⬜ |
| Idle/Patrol 상태 | 좌우 순찰 | | ⬜ |
| Chase 상태 | 플레이어 추적 | | ⬜ |
| Attack 상태 | 1.5초마다 공격 | | ⬜ |
| 절벽 감지 | 떨어지지 않음 | | ⬜ |
| Magic Missile 데미지 | 10 데미지/발 | | ⬜ |
| 적 사망 | HP 0 시 파괴 | | ⬜ |
| 골드/EXP 드롭 | 10골드, 5EXP | | ⬜ |
| Fireball AOE | 반경 2m 데미지 | | ⬜ |

**다음 단계**: 모든 항목이 ✅이면 Phase A-3 테스트로 진행

---

## Phase A-3 테스트: Room System

### 목표

- ✅ Room 생성 및 EnterRoomAsync() 동작 확인
- ✅ EnemySpawnPoint로 적 자동 스폰
- ✅ 모든 적 처치 시 Room 클리어
- ✅ Portal 활성화 및 다음 방 이동
- ✅ RoomManager로 여러 방 관리

---

### 테스트 환경 구축

#### 1. Room_01 생성

```
Hierarchy 우클릭 > Create Empty

GameObject: "Room_01"
────────────────────────────────────
Transform:
  Position: (0, 0, 0)
  Rotation: (0, 0, 0)
  Scale: (1, 1, 1)

GameObject > SetActive: ✓ (활성화 상태)

Add Component: Room
  (스크립트 검색: "Room")

Room (Script) 설정:
  Room Data: TestRoom_Normal (위에서 만든 것)
  Auto Find Spawn Points: ✓
  Show Debug Logs: ✓
```

#### 2. Room_01에 Ground 추가

```
기존 "Ground" GameObject를 Room_01 자식으로 이동
(Drag & Drop: Ground → Room_01)

Room_01
  └─ Ground
```

#### 3. EnemySpawnPoint 생성 (4개)

```
Room_01 우클릭 > Create Empty (x4)

SpawnPoint_01:
  Transform: Position (5, 0, 0)
  Add Component: EnemySpawnPoint
    Enemy Data: TestGoblin
    Show Gizmos: ✓

SpawnPoint_02:
  Transform: Position (8, 0, 0)
  Add Component: EnemySpawnPoint
    Enemy Data: TestGoblin
    Show Gizmos: ✓

SpawnPoint_03:
  Transform: Position (-5, 0, 0)
  Add Component: EnemySpawnPoint
    Enemy Data: TestGoblin
    Show Gizmos: ✓

SpawnPoint_04:
  Transform: Position (-8, 0, 0)
  Add Component: EnemySpawnPoint
    Enemy Data: TestGoblin
    Show Gizmos: ✓

[구조]
Room_01
  ├─ Ground
  ├─ SpawnPoint_01
  ├─ SpawnPoint_02
  ├─ SpawnPoint_03
  └─ SpawnPoint_04
```

**Scene 뷰 확인**: 4개의 스폰 포인트 Gizmos가 표시되어야 함 (노란색 구)

---

#### 4. Portal 생성

```
Room_01 우클릭 > 2D Object > Sprites > Circle

GameObject: "Portal"
────────────────────────────────────
Transform:
  Position: (12, -1, 0)  ← Ground 오른쪽 끝
  Rotation: (0, 0, 0)
  Scale: (0.8, 0.8, 1)

SpriteRenderer:
  Sprite: Circle
  Color: Cyan (0, 1, 1, 0.5) - 반투명 시안색

Add Component: CircleCollider2D
  Is Trigger: ✓ ← 중요!
  Radius: 1

Add Component: Portal
  Portal Type: NextRoom
  Auto Activate On Room Clear: ✓
  Start Active: false  ← 처음엔 비활성
  Portal Sprite: Portal (자동 할당됨)
  Inactive Color: Gray (0.5, 0.5, 0.5, 0.5)
  Active Color: Cyan (0, 1, 1, 1)
  Show Debug Logs: ✓

[구조]
Room_01
  ├─ Ground
  ├─ SpawnPoint_01
  ├─ SpawnPoint_02
  ├─ SpawnPoint_03
  ├─ SpawnPoint_04
  └─ Portal
```

---

#### 5. Room_02 생성 (복제)

```
Hierarchy에서 Room_01 선택 > Ctrl+D (복제)

Room_02:
  Transform: Position (40, 0, 0)  ← 멀리 떨어뜨림
  GameObject > SetActive: false  ← 비활성화!

Room_02 하위:
  Ground: Position (40, -3, 0) 확인
  Portal: Position (52, -1, 0) 확인
  SpawnPoint들: (40+5, 0, 0) 등으로 자동 이동됨
```

---

#### 6. RoomManager 생성

```
Hierarchy 우클릭 > Create Empty

GameObject: "RoomManager"
────────────────────────────────────
Transform:
  Position: (0, 0, 0)

Add Component: RoomManager
  Auto Find Rooms: ✓
  Show Debug Logs: ✓
```

---

### 테스트 시나리오

#### 테스트 1: RoomManager 초기화

```
[실행]
1. Play 모드 진입
2. Console 확인

[기대 결과]
Console 출력:
  "[RoomManager] 2개의 방 자동 탐색"
  "[RoomManager] 총 2개의 방 초기화 완료"

[검증]
RoomManager 우클릭 > "Print Room List"

Console 출력:
  === Room Manager ===
  Total Rooms: 2
  Current Room: None (-1/2)
  Rooms:
    [0] Room_01
    [1] Room_02
  ====================
```

**✅ 성공 조건**: 2개의 방이 정상 등록됨

---

#### 테스트 2: 던전 시작 (Room_01 진입)

```
[실행]
1. Play 모드 진행 중
2. RoomManager 우클릭 > "Start Dungeon (Test)"

[기대 결과]
Console 출력:
  "[RoomManager] 던전 시작!"
  "[Room] Room_01 진입 완료 - 적 2마리"  (또는 3마리)
  "[Room] Room_01 상태 변경: Inactive → Entering"
  "[Room] Room_01 상태 변경: Entering → InProgress"

Hierarchy에서:
  - Room_01: Active ✓
  - Room_02: Inactive

Scene 뷰에서:
  - 2-3마리의 BasicMeleeEnemy가 스폰됨 (빨간색 사각형)
  - Portal은 회색(비활성)

[검증]
- TestPlayer가 Room_01 Ground 위에 있는지 확인
- 적들이 Patrol 시작하는지 확인
```

**✅ 성공 조건**:
- Room_01 활성화
- 적 2-3마리 스폰
- Portal 비활성 (회색)
- Console에 진입 로그

---

#### 테스트 3: 적 스폰 확인

```
[실행]
1. Play 모드에서 Scene 뷰 확인
2. Hierarchy에서 새로 생성된 Enemy GameObject 확인

[기대 결과]
Hierarchy:
  Room_01
    ├─ Ground
    ├─ SpawnPoint_01
    ├─ SpawnPoint_02
    ├─ SpawnPoint_03
    ├─ SpawnPoint_04
    ├─ Portal
    ├─ Test Goblin (Clone)  ← 새로 생성됨
    ├─ Test Goblin (Clone)
    └─ Test Goblin (Clone)  (2-3개)

[검증]
각 Enemy 선택 > Inspector:
  - BasicMeleeEnemy 컴포넌트 확인
  - enemyData: TestGoblin (Reflection으로 자동 설정됨)
  - Current State: Patrol 또는 Chase

Console 로그:
  "[Room] Room_01: 적 사망 - 남은 적: 2마리"  (누군가 죽으면)
```

**✅ 성공 조건**:
- 2-3마리 정확히 스폰
- 각 적이 독립적으로 AI 동작
- enemyData 자동 설정

---

#### 테스트 4: 모든 적 처치 → Room 클리어

```
[실행]
1. Play 모드에서 TestPlayer로 모든 적 처치
   (Magic Missile 3회씩 x 적 수)
2. 마지막 적이 죽는 순간 관찰

[기대 결과]
마지막 적 사망 시:
  "[Room] Room_01: 적 사망 - 남은 적: 0마리"
  "[Room] Room_01 클리어!"
  "[Room] Room_01 상태 변경: InProgress → Cleared"
  "[Room] 보너스 골드 50 획득!"
  "[Room] 보너스 경험치 20 획득!"
  "[Portal] 방 클리어 - 포탈 활성화!"

Portal이 시안색(활성)으로 변함

[검증]
- Portal SpriteRenderer 색상: Cyan (0, 1, 1, 1)
- Portal에 가까이 가면 충돌 가능해짐
- CurrencySystem에 골드 50 추가 확인 (나중에 UI 추가 시)
```

**✅ 성공 조건**:
- 모든 적 처치 시 클리어
- Portal 활성화 (시안색)
- 보너스 골드/EXP 지급
- Console 로그 출력

---

#### 테스트 5: Portal 사용 → Room_02 이동

```
[실행]
1. Play 모드에서 TestPlayer를 Portal로 이동 (WASD)
2. Portal과 충돌하는 순간 관찰

[기대 결과]
Console 출력:
  "[Portal] 플레이어가 포탈에 진입!"
  "[Portal] 포탈 사용 완료!"
  "[RoomManager] Room_02으로 이동 (2/2)"
  "[Room] Room_02 진입 완료 - 적 2마리"

Hierarchy:
  - Room_01: Inactive
  - Room_02: Active ✓

Scene 뷰:
  - 화면이 Room_02로 전환됨 (Position X: 40)
  - 새로운 적 2-3마리 스폰
  - 새로운 Portal (회색)

TestPlayer 위치:
  - Position X: 약 40 근처 (Room_02 시작 위치)

[검증]
- Room_01이 비활성화되었는지 확인
- Room_02의 적들이 새로 스폰되었는지 확인
- Portal 재사용 시 "더 이상 방이 없습니다!" 메시지 (마지막 방)
```

**✅ 성공 조건**:
- Portal 충돌 시 Room_02 이동
- Room_01 비활성화
- Room_02 활성화 + 적 스폰
- Console 로그 정상

---

#### 테스트 6: Context Menu 기능

```
[테스트 A] Room 우클릭 메뉴

Room_01 선택 > 우클릭:
  1. "Print Room Info" 클릭
     Console:
       === Room: Room_01 ===
       State: InProgress
       RoomData: Test Room - Normal
       Spawn Points: 4
       Alive Enemies: 2
       ====================

  2. "Force Clear Room" 클릭
     → 즉시 클리어 처리 (테스트용)
     Console: "[Room] Room_01 클리어!"

[테스트 B] RoomManager 우클릭 메뉴

RoomManager 선택 > 우클릭:
  1. "Print Room List" 클릭
     (위에서 확인함)

  2. "Move To Next Room (Test)" 클릭
     → Room_02로 즉시 이동

[테스트 C] Portal 우클릭 메뉴

Portal 선택 > 우클릭:
  1. "Activate Portal" 클릭
     → 포탈 강제 활성화 (테스트용)

  2. "Deactivate Portal" 클릭
     → 포탈 비활성화

  3. "Print Portal Info" 클릭
     Console:
       [Portal] Portal
       Type: NextRoom
       Active: false
       Auto Activate On Clear: true
       Parent Room: Room_01
```

**✅ 성공 조건**: 모든 Context Menu 기능 작동

---

### Phase A-3 테스트 결과 요약

| 항목 | 예상 결과 | 실제 결과 | 상태 |
|------|----------|----------|------|
| RoomManager 초기화 | 2개 방 등록 | | ⬜ |
| Room 진입 | 적 스폰, InProgress | | ⬜ |
| 적 자동 스폰 | 2-3마리 생성 | | ⬜ |
| enemyData 자동 설정 | Reflection 동작 | | ⬜ |
| 적 전멸 감지 | 마지막 적 죽을 때 | | ⬜ |
| Room 클리어 | Cleared 상태 전환 | | ⬜ |
| 보상 지급 | 골드 50, EXP 20 | | ⬜ |
| Portal 활성화 | 시안색 변경 | | ⬜ |
| Portal 사용 | Room_02 이동 | | ⬜ |
| Room 전환 | Room_01 비활성화 | | ⬜ |
| Context Menu | 모든 기능 작동 | | ⬜ |

**다음 단계**: 모든 항목이 ✅이면 통합 테스트로 진행

---

## 통합 테스트: 전체 시스템 연동

### 목표

- ✅ 3개 Phase가 충돌 없이 동시 작동
- ✅ 실제 게임플레이 시나리오 테스트
- ✅ 전체 플로우 검증 (Room 진입 → 전투 → 클리어 → 이동)

---

### 통합 환경 최종 구성

이전 단계에서 만든 IntegrationTestScene을 사용합니다.

#### 추가 컴포넌트 확인

```
[TestPlayer]
✓ Rigidbody2D
✓ BoxCollider2D
✓ SpriteRenderer
✓ MageForm
✓ FormInputHandler
✓ PlayerController  ← 추가 (Stage 1에서 만든 것)
✓ PlayerStats  ← 필요 시 추가

[Main Camera]
✓ CameraFollow  ← 추가 (Stage 1에서 만든 것)
  Target: TestPlayer

[Hierarchy 구조]
TestPlayer (Tag: "Player")
Main Camera
Ground (Room_01 자식으로 이동됨)
Room_01
  ├─ Ground
  ├─ SpawnPoint_01~04
  └─ Portal
Room_02
  ├─ Ground
  ├─ SpawnPoint_01~04
  └─ Portal
RoomManager
```

---

### 통합 테스트 시나리오

#### 시나리오 1: 완전한 게임플레이 플로우

```
[목표]
Room_01 진입 → 적 처치 → Room_02 이동 → 적 처치 → 던전 클리어

[Step 1] 게임 시작
1. Play 모드 진입
2. RoomManager 우클릭 > "Start Dungeon (Test)"
3. TestPlayer가 Room_01 Ground 위에 있는지 확인

[Step 2] 이동 테스트 (PlayerController)
1. A/D 키로 좌우 이동
   → TestPlayer가 부드럽게 이동하는지 확인
2. 스페이스바로 점프
   → TestPlayer가 점프 후 Ground에 착지하는지 확인
3. Scene 뷰 확인
   → Camera가 TestPlayer를 부드럽게 따라가는지 확인 (CameraFollow)

[Step 3] 전투 테스트 (MageForm + Enemy AI)
1. 적들이 플레이어를 감지하고 Chase 시작하는지 확인
2. 마우스 좌클릭으로 Magic Missile 발사
   → 적에게 데미지 들어가는지 확인
3. Q키로 Teleport
   → 적의 공격을 회피할 수 있는지 확인
4. E키로 Fireball
   → 여러 적을 동시에 공격할 수 있는지 확인
5. 모든 적 처치
   → Console: "[Room] Room_01 클리어!"

[Step 4] Portal 이동 (Room System)
1. Portal이 시안색으로 활성화되었는지 확인
2. A/D 키로 Portal 위치로 이동
3. Portal 진입
   → 0.3초 대기 후 Room_02로 이동
   → Camera도 함께 이동하는지 확인

[Step 5] Room_02 전투
1. Room_02의 새로운 적들 확인
2. 다시 전투 (Magic Missile, Teleport, Fireball)
3. 모든 적 처치
   → Console: "[Room] Room_02 클리어!"

[Step 6] 던전 클리어
1. Room_02 Portal 진입
   → Console: "[RoomManager] 더 이상 방이 없습니다! (던전 클리어)"
   → Console: "[RoomManager] 던전 클리어!"

[검증 포인트]
□ 플레이어 이동이 자연스러운가?
□ 카메라가 플레이어를 부드럽게 따라가는가?
□ 스킬이 입력에 즉시 반응하는가?
□ 적 AI가 자연스럽게 동작하는가?
□ Room 전환이 부드러운가?
□ Console에 에러가 없는가?
```

**✅ 성공 조건**:
- 전체 플로우가 끊김 없이 진행
- 모든 시스템이 정상 작동
- Console에 에러 없음

---

#### 시나리오 2: 극한 상황 테스트

```
[테스트 A] 빠른 스킬 연타
1. Play 모드에서 마우스 좌클릭 연타 (10회)
   → 쿨다운 메시지가 정상 출력되는가?
   → 크래시 없는가?

2. Q키 연타 (5회)
   → Teleport가 3초마다 1회만 실행되는가?
   → CancellationToken 에러 없는가?

[테스트 B] 적 대량 스폰
1. RoomData 수정: maxEnemyCount = 10
2. Play 모드 진입 > Start Dungeon
   → 10마리가 모두 스폰되는가?
   → FPS 드롭 확인
   → 모든 적이 독립적으로 AI 동작하는가?

[테스트 C] Room 빠른 전환
1. Room_01 진입 > Force Clear Room (Context Menu)
2. 즉시 Portal 진입
3. Room_02 진입 > Force Clear Room
4. 즉시 Portal 진입
   → 빠른 전환에도 에러 없는가?
   → CancellationToken 정리 잘 되는가?

[테스트 D] 절벽에서 전투
1. Ground 끝에 적 배치
2. 플레이어가 적을 밀어낼 수 있는지 확인
   → 적이 절벽에서 떨어지는가? (IsEdgeAhead 무시됨)
   → 플레이어도 떨어질 수 있는가?

[테스트 E] Portal 중복 사용
1. Room_01 클리어 > Portal 활성화
2. Portal에 반복 진입 시도
   → 한 번만 작동하는가? (SetActive(false)로 방지)
   → 중복 이동 에러 없는가?
```

**✅ 성공 조건**:
- 극한 상황에서도 크래시 없음
- 예외 처리 정상 작동
- Console 에러 없음

---

#### 시나리오 3: 성능 테스트

```
[테스트]
1. Room_01에 적 10마리 스폰
2. Play 모드에서 5분간 전투
3. Stats 창 확인 (Window > Analysis > Profiler)

[확인 항목]
□ FPS: 60 유지되는가?
□ GC Alloc: 매 프레임 0 Byte인가? (누수 없음)
□ Draw Calls: 적정 수준인가? (< 100)
□ 메모리: 증가하지 않는가? (누수 없음)

[주의 사항]
- Debug.Log가 많으면 성능 저하 가능 → showDebugLogs = false로 설정
- Scene 뷰를 끄면 FPS 상승
```

**✅ 성공 조건**:
- FPS 60 유지
- 메모리 누수 없음

---

### 통합 테스트 결과 요약

| 항목 | 예상 결과 | 실제 결과 | 상태 |
|------|----------|----------|------|
| 완전한 플로우 | Room_01 → Room_02 클리어 | | ⬜ |
| 플레이어 이동 | WASD 정상 | | ⬜ |
| 카메라 추적 | 부드러운 이동 | | ⬜ |
| 스킬 실행 | 즉시 반응 | | ⬜ |
| 적 AI | 자연스러운 동작 | | ⬜ |
| 데미지 시스템 | 정확한 HP 감소 | | ⬜ |
| Room 전환 | 부드러운 이동 | | ⬜ |
| 쿨다운 시스템 | 정상 작동 | | ⬜ |
| 극한 상황 | 크래시 없음 | | ⬜ |
| 성능 | FPS 60 유지 | | ⬜ |

---

## 트러블슈팅

### 자주 발생하는 문제

#### 문제 1: "Player" 태그 없음

```
[증상]
Console 에러:
  "[CameraFollow] \"Player\" 태그를 가진 GameObject를 찾을 수 없습니다!"

[원인]
TestPlayer GameObject의 Tag가 "Untagged"로 설정됨

[해결]
TestPlayer 선택 > Inspector 상단 > Tag > "Player" 선택
(없으면 Add Tag... 클릭하여 "Player" 태그 생성)
```

---

#### 문제 2: Enemy가 스폰되지 않음

```
[증상]
Console:
  "[Room] Room_01 진입 완료 - 적 0마리"

[원인 A] RoomData가 할당되지 않음
[해결]
Room_01 선택 > Room (Script) > Room Data: TestRoom_Normal 드래그

[원인 B] EnemyData가 null
[해결]
SpawnPoint_01 선택 > EnemySpawnPoint > Enemy Data: TestGoblin 드래그

[원인 C] spawnChance가 0
[해결]
TestRoom_Normal.asset 선택 > Enemy Spawns > Element 0 > spawnChance: 1.0
```

---

#### 문제 3: Magic Missile이 적에게 안 맞음

```
[증상]
Console:
  "[MagicMissile] 투사체 발사..."
  (데미지 로그 없음)

[원인 A] Layer 설정 문제
[해결]
TestEnemy 선택 > Layer: Default (또는 "Enemy" 레이어 생성)
Physics2D.Raycast가 해당 Layer를 감지할 수 있어야 함

[원인 B] Collider 없음
[해결]
TestEnemy에 BoxCollider2D 또는 CapsuleCollider2D 추가

[원인 C] 적이 너무 멀리 있음
[해결]
Magic Missile 최대 사거리: 10m
TestEnemy를 10m 이내에 배치
```

---

#### 문제 4: Portal이 작동하지 않음

```
[증상]
Portal에 플레이어가 진입해도 아무 반응 없음

[원인 A] Portal Collider가 Trigger가 아님
[해결]
Portal 선택 > CircleCollider2D > Is Trigger: ✓ 체크

[원인 B] Portal이 비활성 상태
[해결]
Portal 선택 > Portal (Script) 우클릭 > "Activate Portal"

[원인 C] PlayerStats 컴포넌트 없음
[해결]
Portal.cs 92줄:
  if (other.TryGetComponent<GASPT.Stats.PlayerStats>(out var player))

TestPlayer에 PlayerStats 컴포넌트 추가 필수!
```

---

#### 문제 5: RoomManager가 방을 못 찾음

```
[증상]
Console:
  "[RoomManager] 0개의 방 자동 탐색"

[원인] Room GameObject가 비활성화 상태
[해결]
Room_01, Room_02 모두 한 번씩 활성화 상태로 Play 모드 진입
→ RoomManager.Awake()에서 FindObjectsByType()가 활성 GameObject만 찾음

대안:
RoomManager 선택 > Auto Find Rooms: ✗
Manual Rooms (배열):
  Size: 2
  Element 0: Room_01
  Element 1: Room_02
```

---

#### 문제 6: 적이 제자리에서 멈춤

```
[증상]
적이 스폰 후 이동하지 않음

[원인 A] Rigidbody2D가 없음
[해결]
TestEnemy에 Rigidbody2D 추가
(EnemySpawnPoint.CreateEnemyFromData()가 자동 추가하지만, 수동 생성 시 필요)

[원인 B] Ground와 충돌하지 않음
[해결]
TestEnemy 선택 > BoxCollider2D 확인
Ground 선택 > BoxCollider2D 확인

[원인 C] enemyData가 null
[해결]
Console에서 "[BasicMeleeEnemy] enemyData가 null입니다!" 확인
TestEnemy 선택 > Debug 모드 > enemyData: TestGoblin 드래그
```

---

#### 문제 7: Camera가 플레이어를 안 따라감

```
[증상]
플레이어가 이동해도 Camera 고정

[원인 A] CameraFollow Target이 null
[해결]
Main Camera 선택 > CameraFollow > Target: TestPlayer 드래그

[원인 B] TestPlayer Tag가 "Player"가 아님
[해결]
TestPlayer 선택 > Tag: "Player" 설정

[원인 C] Follow X/Y가 체크 해제됨
[해결]
Main Camera > CameraFollow > Follow X: ✓, Follow Y: ✓
```

---

#### 문제 8: 컴파일 에러 - CS1061 'Awaitable' does not contain a definition for 'Forget'

```
[증상]
Console 에러:
  CS1061: 'Awaitable' does not contain a definition for 'Forget'

[원인]
AwaitableExtensions.cs가 없거나 using Core; 누락

[해결]
1. Assets/_Project/Scripts/Core/AwaitableExtensions.cs 존재 확인
2. Room.cs, Portal.cs 상단에 using Core; 추가
3. Unity 메뉴: Assets > Reimport All (강제 재컴파일)
```

---

#### 문제 9: NullReferenceException - RoomManager.Instance is null

```
[증상]
Console 에러:
  NullReferenceException: Object reference not set to an instance of an object
  Portal.UsePortalAsync() (at Assets/.../Portal.cs:117)

[원인]
RoomManager가 Scene에 없거나 SingletonPreloader가 초기화 안 됨

[해결]
1. Hierarchy에 "RoomManager" GameObject 확인
2. RoomManager 컴포넌트 확인
3. Play 모드 재진입 (SingletonPreloader 자동 초기화)
```

---

## 테스트 체크리스트

### 최종 검증 항목

```
[ ] Phase A-1: MageForm
    [ ] MageForm 초기화 (3개 스킬 등록)
    [ ] Magic Missile 발사 (마우스 좌클릭)
    [ ] Teleport 이동 (Q키)
    [ ] Fireball 발사 (E키)
    [ ] 각 스킬 쿨다운 작동
    [ ] FormInputHandler 키 입력 정상

[ ] Phase A-2: Enemy AI
    [ ] BasicMeleeEnemy 스폰
    [ ] Idle/Patrol 상태 동작
    [ ] Chase 상태 (플레이어 추적)
    [ ] Attack 상태 (근접 공격)
    [ ] 절벽 감지 (떨어지지 않음)
    [ ] Magic Missile로 데미지
    [ ] HP 0 도달 시 사망
    [ ] 골드/EXP 드롭

[ ] Phase A-3: Room System
    [ ] RoomManager 초기화
    [ ] Room 진입 (EnterRoomAsync)
    [ ] 적 자동 스폰 (EnemySpawnPoint)
    [ ] 모든 적 처치 → Room 클리어
    [ ] Portal 활성화 (시안색)
    [ ] Portal 사용 → 다음 방 이동
    [ ] Room 전환 (비활성화/활성화)
    [ ] 보상 지급 (골드/EXP)

[ ] 통합 테스트
    [ ] PlayerController 이동 (WASD/점프)
    [ ] CameraFollow 추적
    [ ] 전체 플로우 (Room_01 → Room_02)
    [ ] 극한 상황 (연타, 대량 스폰)
    [ ] 성능 (FPS 60 유지)
    [ ] Console 에러 없음

[ ] 추가 검증
    [ ] Context Menu 모든 기능 작동
    [ ] Gizmos 시각화 표시
    [ ] Debug.Log 출력 정상
    [ ] Scene 저장 (Ctrl+S)
```

---

## 테스트 완료 후

### 다음 단계

모든 체크리스트가 ✅이면:

1. **Scene 저장**: IntegrationTestScene.unity 저장 (Ctrl+S)
2. **Git Commit**:
   ```bash
   git add .
   git commit -m "테스트: Phase A-1/A-2/A-3 통합 테스트 완료

   - MageForm 시스템 검증 완료
   - Enemy AI FSM 검증 완료
   - Room System 검증 완료
   - 통합 테스트 시나리오 통과
   - IntegrationTestScene 추가"
   ```

3. **다음 Phase 진행**: Stage 1 (PlayerController, CameraFollow) 또는 Stage 2 (Roguelike 메커닉)

---

### 문제 발견 시

❌ 체크리스트에서 하나라도 실패하면:

1. **트러블슈팅** 섹션 참조
2. **Console 로그** 상세 확인
3. **GitHub Issue** 등록 (버그 리포트)
4. **해당 Phase README.md** 재확인

---

## 참고 문서

- **Phase A-1**: `Assets/_Project/Scripts/Gameplay/Form/README.md`
- **Phase A-2**: `Assets/_Project/Scripts/Gameplay/Enemy/README.md`
- **Phase A-3**: `Assets/_Project/Scripts/Gameplay/Level/README.md`
- **AwaitableExtensions**: `Assets/_Project/Scripts/Core/AwaitableExtensions.cs`

---

**최종 업데이트**: 2025-11-10
**작성자**: Phase A Integration Team
**테스트 환경**: Unity 6.0, GASPT 프로젝트
