# Phase C-1 테스트 및 검증 가이드

> **작성일**: 2025-09-21
> **목적**: Phase C-1 (다양한 적 타입 추가) 구현 완료 후 테스트 및 검증

---

## 📋 목차

1. [Unity 에셋 생성](#1-unity-에셋-생성)
2. [프리팹 생성](#2-프리팹-생성)
3. [씬 테스트](#3-씬-테스트)
4. [적 타입별 검증 체크리스트](#4-적-타입별-검증-체크리스트)
5. [문제 해결 (Troubleshooting)](#5-문제-해결-troubleshooting)

---

## 1. Unity 에셋 생성

### 1.1 EnemyData 에셋 생성

#### 자동 생성 (권장)

1. Unity 에디터 실행
2. 메뉴: `Tools → GASPT → Enemy Data Creator`
3. "🎯 모든 EnemyData 에셋 생성" 버튼 클릭
4. 생성 확인: `Assets/_Project/Data/Enemies/` 폴더 확인

**생성되는 에셋:**
- `RangedGoblin.asset` - 원거리 고블린
- `FlyingBat.asset` - 비행 박쥐
- `EliteOrc.asset` - 정예 오크

#### 수동 생성 (선택사항)

1. Project 창에서 `Assets/_Project/Data/Enemies/` 폴더 우클릭
2. `Create → GASPT/Enemies/Enemy` 선택
3. 이름 변경 (RangedGoblin, FlyingBat, EliteOrc)
4. Inspector에서 각 필드 수동 설정 (아래 권장값 참고)

**RangedGoblin 권장값:**
```
타입: Normal
이름: 원거리 고블린
HP: 25
공격력: 7
골드: 10-20
경험치: 15
이동속도: 1.5
감지거리: 12
최적공격거리: 8
최소거리: 4
공격쿨다운: 2초
```

**FlyingBat 권장값:**
```
타입: Normal
이름: 비행 박쥐
HP: 20
공격력: 8
골드: 12-18
경험치: 18
이동속도: 2
감지거리: 10
비행높이: 5
급강하속도: 8
비행속도: 2.5
공격쿨다운: 1.5초
```

**EliteOrc 권장값:**
```
타입: Named
이름: 정예 오크
HP: 80
공격력: 15
골드: 40-60
경험치: 50
이동속도: 1.8
감지거리: 8
돌진쿨다운: 6초
범위공격쿨다운: 8초
범위공격반경: 3.5
돌진속도: 10
돌진거리: 6
showNameTag: true
```

### 1.2 에셋 검증

생성된 에셋을 클릭하여 Inspector에서 다음 확인:

- ✅ `enemyName`이 한글로 표시됨
- ✅ 모든 스탯 값이 0이 아님
- ✅ `minGoldDrop ≤ maxGoldDrop`
- ✅ EliteOrc는 `showNameTag = true`

---

## 2. 프리팹 생성

### 2.1 적 프리팹 생성

1. Unity 에디터 실행
2. 메뉴: `Tools → GASPT → Prefab Creator`
3. "🚀 모든 프리팹 생성" 버튼 클릭 (또는 개별 생성)

**생성되는 프리팹 (Phase C-1 관련):**
- `BasicMeleeEnemy.prefab`
- `RangedEnemy.prefab` ⭐ NEW
- `FlyingEnemy.prefab` ⭐ NEW
- `EliteEnemy.prefab` ⭐ NEW
- `EnemyProjectile.prefab` ⭐ NEW (RangedEnemy용)

### 2.2 프리팹 검증

`Assets/Resources/Prefabs/Enemies/` 폴더에서 각 프리팹 확인:

**RangedEnemy.prefab:**
- ✅ `RangedEnemy` 컴포넌트 존재
- ✅ `PooledObject` 컴포넌트 존재
- ✅ `Rigidbody2D` (gravityScale = 2, freezeRotation = true)
- ✅ `BoxCollider2D` (크기 1x1)
- ✅ `FirePoint` 자식 오브젝트 (위치: 0.5, 0.5, 0)

**FlyingEnemy.prefab:**
- ✅ `FlyingEnemy` 컴포넌트 존재
- ✅ `PooledObject` 컴포넌트 존재
- ✅ `Rigidbody2D` (gravityScale = 0 ⚠️ 중요!)
- ✅ `BoxCollider2D` (isTrigger = true ⚠️ 중요!)

**EliteEnemy.prefab:**
- ✅ `EliteEnemy` 컴포넌트 존재
- ✅ `PooledObject` 컴포넌트 존재
- ✅ `Rigidbody2D` (gravityScale = 2, freezeRotation = true)
- ✅ `BoxCollider2D` (크기 1.2x1.2)

**EnemyProjectile.prefab:**
- ✅ `EnemyProjectile` 컴포넌트 존재
- ✅ `PooledObject` 컴포넌트 존재
- ✅ `Rigidbody2D` (gravityScale = 0, freezeRotation = true)
- ✅ `CircleCollider2D` (isTrigger = true, 반지름 0.2)

---

## 3. 씬 테스트

### 3.1 GameplayScene 생성 또는 열기

**옵션 A: 씬이 이미 있는 경우**
1. Project 창에서 `Assets/_Project/Scenes/GameplayScene.unity` 더블클릭
2. 씬 열기 완료 → 바로 **3.2 플레이 모드 테스트**로 이동

**옵션 B: 씬을 새로 생성하는 경우**
1. Unity 메뉴: `Tools → GASPT → Gameplay Scene Creator`
2. 창이 열리면 "🚀 GameplayScene 생성" 버튼 클릭
3. 생성 완료 대화상자 확인
4. `Assets/_Project/Scenes/GameplayScene.unity` 자동 생성 및 열림

**⚠️ 중요:**
- GameplaySceneCreator는 **씬의 GameObject가 아닌 에디터 도구**입니다 (Unity 메뉴에서 실행)
- 씬 생성 시 자동으로 다음 EnemyData를 로드 시도:
  - `Assets/_Project/Data/Enemies/TestGoblin.asset` (기본, fallback용)
  - `Assets/_Project/Data/Enemies/RangedGoblin.asset` (Phase C-1)
  - `Assets/_Project/Data/Enemies/FlyingBat.asset` (Phase C-1)
  - `Assets/_Project/Data/Enemies/EliteOrc.asset` (Phase C-1)
- 없는 에셋은 TestGoblin으로 fallback됨

**생성되는 씬 구조:**
```
GameplayScene
├── Player (MageForm)
├── === ROOMS ===
│   ├── Room_0 (시작 방, 적 없음)
│   ├── Room_1 (2~4개 EnemySpawner)
│   ├── Room_2 (2~4개 EnemySpawner)
│   └── BossRoom (2~4개 EnemySpawner)
├── === PLATFORMS === (장식용)
├── === UI ===
│   ├── Canvas (HUD)
│   └── BuffIconPanel
├── Main Camera
└── === SINGLETONS ===
    ├── PoolManager
    ├── GameResourceManager
    ├── StatusEffectManager
    └── DamageNumberPool
```

각 Room의 EnemySpawner는 가중치 랜덤으로 적을 스폰합니다:
- 40% BasicMeleeEnemy
- 30% RangedEnemy (Phase C-1)
- 20% FlyingEnemy (Phase C-1)
- 10% EliteEnemy (Phase C-1)

### 3.2 플레이 모드 테스트

#### 테스트 1: 적 스폰 확인

1. Play 버튼 클릭
2. 씬에 적들이 랜덤 스폰되는지 확인
   - 40% BasicMelee / 30% Ranged / 20% Flying / 10% Elite

**확인 사항:**
- ✅ 적들이 화면에 표시됨
- ✅ PoolRoot/Pool_XXX 하위에 오브젝트 생성됨
- ✅ Console에 풀 초기화 로그 출력:
  ```
  [EnemyPoolInitializer] BasicMeleeEnemy 풀 생성 완료
  [EnemyPoolInitializer] RangedEnemy 풀 생성 완료
  [EnemyPoolInitializer] FlyingEnemy 풀 생성 완료
  [EnemyPoolInitializer] EliteEnemy 풀 생성 완료
  [ProjectilePoolInitializer] EnemyProjectile 풀 생성 완료
  ```

#### 테스트 2: RangedEnemy 동작

1. RangedEnemy가 스폰될 때까지 대기 (기본 확률 30%)
   - **팁**: 더 빨리 테스트하려면 Hierarchy에서 각 Room의 EnemySpawner 찾아서 `EnemyData` 슬롯을 모두 RangedGoblin으로 변경
2. 플레이어를 RangedEnemy 근처로 이동

**예상 동작:**
- ✅ 감지 거리(12) 진입 → Chase 상태 전환
- ✅ 최적 공격 거리(8) 진입 → RangedAttack 상태 전환, 정지 후 발사
- ✅ EnemyProjectile이 플레이어 방향으로 발사됨
- ✅ 플레이어가 최소 거리(4) 안으로 진입 → Retreat 상태, 후퇴 이동
- ✅ 공격 쿨다운(2초) 존재

**디버그:**
- Scene 뷰에서 Gizmos 확인 (녹색: 최적 공격 거리, 주황색: 최소 거리)
- RangedEnemy 선택 → Inspector에서 현재 State 확인

#### 테스트 3: FlyingEnemy 동작

1. FlyingEnemy가 스폰될 때까지 대기
2. 플레이어를 FlyingEnemy 근처로 이동

**예상 동작:**
- ✅ 비행 높이(5) 유지하며 Fly 상태 순찰
- ✅ 감지 거리(10) 진입 → PositionAbove 상태, 플레이어 위로 이동
- ✅ 플레이어 진입 위 도달 → DiveAttack 상태, 급강하(속도 8)
- ✅ 충돌 또는 일정 시간 후 → ReturnToAir 상태, 다시 상승
- ✅ 중력 무시, 공중 부유

**디버그:**
- Rigidbody2D의 Velocity 확인 (Y축 값이 중력 영향 없이 제어됨)
- Collider의 isTrigger = true 확인 (물리적 충돌 없음)

#### 테스트 4: EliteEnemy 동작

1. EliteEnemy가 스폰될 때까지 대기 (확률 10%로 낮음)
2. 플레이어를 EliteEnemy 근처로 이동

**예상 동작:**
- ✅ 감지 거리(8) 진입 → Chase 상태
- ✅ 공격 범위(2) 진입 → Attack 상태
- ✅ 일정 쿨다운(6초) 후 → ChargeAttack (돌진 공격, 속도 10)
- ✅ 일정 쿨다운(8초) 후 → AreaAttack (범위 공격, 반경 3.5)
- ✅ AreaAttack 시 범위 내 플레이어에게 2배 데미지
- ✅ showNameTag = true이면 이름표 UI 표시 (UI 시스템 구현 시)

**디버그:**
- Scene 뷰에서 범위 공격 반경 Gizmos 확인 (빨간 원)
- EliteEnemy 선택 → Inspector에서 eliteState, 쿨다운 타이머 확인

#### 테스트 5: EnemyProjectile 동작

1. RangedEnemy가 투사체를 발사할 때까지 대기
2. 투사체와 플레이어 충돌 확인

**예상 동작:**
- ✅ 플레이어 방향으로 직선 이동 (속도 10)
- ✅ 플레이어 충돌 → PlayerStats.TakeDamage(7) 호출
- ✅ 충돌 후 즉시 풀로 반환 (비활성화)
- ✅ 벽이나 지형 충돌 → 풀로 반환
- ✅ 5초 후 자동 반환 (수명)

**디버그:**
- PlayerStats의 CurrentHp 감소 확인
- Console 로그 확인: `[EnemyProjectile] Player 충돌! 데미지: 7`

---

## 4. 적 타입별 검증 체크리스트

### 4.1 RangedEnemy (원거리 적)

- [ ] **스폰**: 풀에서 정상적으로 Get/Release됨
- [ ] **Idle → Patrol**: 초기 상태에서 순찰 시작
- [ ] **Patrol → Chase**: 플레이어 감지 거리 진입 시 추격
- [ ] **Chase → RangedAttack**: 최적 공격 거리에서 정지 후 공격
- [ ] **투사체 발사**: EnemyProjectile이 플레이어 방향으로 발사됨
- [ ] **RangedAttack → Retreat**: 플레이어가 최소 거리 안으로 진입 시 후퇴
- [ ] **Retreat → RangedAttack**: 충분히 멀어지면 다시 공격 상태
- [ ] **공격 쿨다운**: 2초 간격으로 발사됨
- [ ] **Chase → Patrol**: 플레이어가 감지 범위를 벗어나면 순찰 복귀
- [ ] **데미지**: 플레이어 공격 시 HP 감소 → Dead 상태
- [ ] **Dead → Despawn**: 사망 후 1초 뒤 풀로 반환

### 4.2 FlyingEnemy (비행 적)

- [ ] **스폰**: 풀에서 정상적으로 Get/Release됨
- [ ] **중력 무시**: Rigidbody2D.gravityScale = 0, 공중 부유
- [ ] **Trigger 충돌**: isTrigger = true, 물리적 충돌 없음
- [ ] **Fly**: 비행 높이 유지하며 순찰
- [ ] **Fly → PositionAbove**: 플레이어 감지 시 위로 이동
- [ ] **PositionAbove → DiveAttack**: 플레이어 위 도달 시 급강하
- [ ] **DiveAttack 속도**: 빠른 속도(8)로 하강
- [ ] **DiveAttack → ReturnToAir**: 충돌 또는 일정 시간 후 상승
- [ ] **ReturnToAir → Fly**: 비행 높이 복귀 후 순찰 재개
- [ ] **공격 쿨다운**: 1.5초 간격
- [ ] **데미지**: 플레이어 공격 시 HP 감소 → Dead 상태
- [ ] **Dead → Despawn**: 사망 후 1초 뒤 풀로 반환

### 4.3 EliteEnemy (정예 적)

- [ ] **스폰**: 풀에서 정상적으로 Get/Release됨
- [ ] **Idle → Patrol**: 초기 상태에서 순찰 시작
- [ ] **Patrol → Chase**: 플레이어 감지 거리 진입 시 추격
- [ ] **Chase → Attack**: 공격 범위 진입 시 공격
- [ ] **기본 공격**: 1.2초 간격으로 근접 공격
- [ ] **ChargeAttack**: 6초 쿨다운 후 돌진 공격 (속도 10, 거리 6)
- [ ] **AreaAttack**: 8초 쿨다운 후 범위 공격 (반경 3.5, 2배 데미지)
- [ ] **AreaAttack 범위**: 범위 내 플레이어만 피해
- [ ] **스킬 쿨다운**: 각 스킬이 독립적으로 쿨다운 관리됨
- [ ] **Chase → Patrol**: 플레이어가 감지 범위를 벗어나면 순찰 복귀
- [ ] **데미지**: 플레이어 공격 시 HP 감소 → Dead 상태
- [ ] **Dead → Despawn**: 사망 후 1초 뒤 풀로 반환
- [ ] **이름표**: showNameTag = true일 때 UI 표시 (UI 시스템 구현 시)

### 4.4 EnemyProjectile (적 투사체)

- [ ] **스폰**: 풀에서 정상적으로 Get/Release됨
- [ ] **발사**: RangedEnemy의 FirePoint에서 생성
- [ ] **방향**: 플레이어 방향으로 직선 이동
- [ ] **속도**: 일정 속도(10)로 이동
- [ ] **플레이어 충돌**: PlayerStats.TakeDamage(7) 호출 → HP 감소
- [ ] **충돌 후 반환**: 즉시 풀로 반환 (비활성화)
- [ ] **지형 충돌**: 벽/바닥 충돌 시 풀로 반환
- [ ] **수명**: 5초 후 자동 반환
- [ ] **레이어**: Player 레이어만 타겟 (적 무시)

---

## 5. 문제 해결 (Troubleshooting)

### 5.0 "GameplaySceneCreator GameObject를 찾을 수 없음"

**증상:**
- Hierarchy에서 GameplaySceneCreator GameObject를 찾을 수 없음
- 테스트 가이드 3.1 단계를 진행할 수 없음

**해결:**
- ✅ **정상입니다!** GameplaySceneCreator는 **씬의 GameObject가 아니라 에디터 도구**입니다
- Unity 메뉴에서 실행: `Tools → GASPT → Gameplay Scene Creator`
- 씬을 이미 생성했다면 Hierarchy에서 다음을 확인:
  - `Player` GameObject (MageForm 컴포넌트)
  - `=== ROOMS ===` GameObject (하위에 Room_0, Room_1, Room_2, BossRoom)
  - 각 Room 하위의 `EnemySpawner` GameObject들

### 5.1 적이 스폰되지 않음

**증상:**
- Play 모드에서 적이 하나도 나타나지 않음
- Console에 에러 없음

**해결 방법:**
1. EnemySpawner 확인:
   - Hierarchy에서 `=== ROOMS ===` → 각 Room → `EnemySpawner` 찾기
   - Inspector에서 `EnemyData` 필드가 할당되어 있는지 확인
   - 없으면 `Assets/_Project/Data/Enemies/` 폴더에서 EnemyData 에셋 드래그 앤 드롭
2. 풀 초기화 확인:
   - Hierarchy에서 `=== SINGLETONS ===` → `PoolManager` 존재 확인
   - Play 모드 진입 시 Console 로그 확인:
     ```
     [EnemyPoolInitializer] BasicMeleeEnemy 풀 생성 완료
     [EnemyPoolInitializer] RangedEnemy 풀 생성 완료
     [EnemyPoolInitializer] FlyingEnemy 풀 생성 완료
     [EnemyPoolInitializer] EliteEnemy 풀 생성 완료
     ```
3. 프리팹 존재 확인:
   - Project 창에서 `Assets/Resources/Prefabs/Enemies/` 폴더 확인
   - 다음 프리팹 존재 여부:
     - BasicMeleeEnemy.prefab
     - RangedEnemy.prefab
     - FlyingEnemy.prefab
     - EliteEnemy.prefab
   - 없으면 `Tools → GASPT → Prefab Creator` 실행

### 5.2 RangedEnemy가 투사체를 발사하지 않음

**증상:**
- RangedAttack 상태 진입은 하지만 투사체가 생성되지 않음
- Console 에러: `[RangedEnemy] XXX 투사체 스폰 실패`

**해결 방법:**
1. EnemyProjectile 풀 확인:
   - `ProjectilePoolInitializer.InitializeEnemyProjectilePool()` 호출 확인
   - Console 로그: `[ProjectilePoolInitializer] EnemyProjectile 풀 생성 완료`
2. 프리팹 확인:
   - `Resources/Prefabs/Projectiles/EnemyProjectile.prefab` 존재 확인
   - EnemyProjectile 컴포넌트 부착 확인
3. ResourcePaths 확인:
   - `ResourcePaths.Prefabs.Projectiles.EnemyProjectile` 경로 확인
   - RangedEnemy.cs 336번 줄에서 이 경로 사용 확인

### 5.3 FlyingEnemy가 땅으로 떨어짐

**증상:**
- FlyingEnemy가 스폰되자마자 땅으로 떨어짐
- 비행하지 못함

**해결 방법:**
1. Rigidbody2D 확인:
   - `gravityScale = 0`인지 확인 ⚠️ 중요!
   - FlyingEnemy.Start()에서 자동 설정되지만, 프리팹에도 0으로 설정 권장
2. 코드 확인:
   - FlyingEnemy.cs 79번 줄: `rb.gravityScale = 0f;`
3. 프리팹 재생성:
   - PrefabCreator로 FlyingEnemy 프리팹 재생성

### 5.4 EliteEnemy 스킬이 발동되지 않음

**증상:**
- EliteEnemy가 기본 공격만 하고 ChargeAttack, AreaAttack을 사용하지 않음

**해결 방법:**
1. EnemyData 확인:
   - `chargeCooldown`, `areaCooldown` 값이 0이 아닌지 확인
   - EliteOrc.asset의 스킬 쿨다운 확인 (6초, 8초)
2. 코드 확인:
   - EliteEnemy.cs의 `TryUseSkills()` 메서드 확인
   - 쿨다운 타이머 로직 확인
3. 디버그 로그:
   - `showDebugLogs = true` 설정
   - Console에서 스킬 발동 로그 확인

### 5.5 EnemyProjectile이 플레이어를 관통함

**증상:**
- 투사체가 플레이어와 충돌해도 데미지를 주지 않음
- 투사체가 사라지지 않고 계속 날아감

**해결 방법:**
1. 레이어 설정 확인:
   - PlayerStats가 있는 GameObject의 Layer가 "Player"인지 확인
   - EnemyProjectile.cs의 `targetLayer` 확인
2. Collider 확인:
   - PlayerStats GameObject에 Collider2D 존재 확인
   - Collider2D가 비활성화되지 않았는지 확인
3. OnTriggerEnter2D 확인:
   - EnemyProjectile.cs의 OnTriggerEnter2D 호출 여부 확인
   - Debug.Log 추가하여 충돌 이벤트 확인

### 5.6 적이 풀로 반환되지 않음

**증상:**
- 적 사망 후 비활성화되지 않고 계속 씬에 남아있음
- Hierarchy에 Dead 상태 오브젝트 누적

**해결 방법:**
1. PooledObject 확인:
   - 프리팹에 PooledObject 컴포넌트 존재 확인
2. Enemy.ReturnToPoolDelayed() 확인:
   - Enemy.cs 163번 줄: 타입별 Despawn 코드 확인
   - 새 적 타입이 추가되어 있는지 확인
3. PoolManager.Despawn() 확인:
   - Reflection으로 Release 메서드 호출 확인
   - Console 에러 확인

### 5.7 한글이 깨져서 표시됨

**증상:**
- EnemyData의 `enemyName`이 깨진 문자로 표시됨
- Console 로그가 깨짐

**해결 방법:**
1. 파일 인코딩 확인:
   - `.editorconfig` 존재 확인 (UTF-8 설정)
   - Visual Studio 또는 Rider에서 파일 인코딩 UTF-8 확인
2. Unity 재시작:
   - Unity 에디터 완전 종료 후 재시작
3. 에셋 재생성:
   - EnemyDataCreator로 에셋 재생성

---

## 6. 성능 최적화 확인

### 6.1 오브젝트 풀 효율성

**확인 방법:**
1. Play 모드에서 PoolManager 선택
2. Context Menu: "Print Pool Info" 클릭
3. Console에서 각 풀의 정보 확인:
   ```
   ========== Pool Manager Info ==========
   Total Pools: 8
     [BasicMeleeEnemy] Total: 5, Active: 3, Available: 2, Initial: 5, CanGrow: true
     [RangedEnemy] Total: 3, Active: 2, Available: 1, Initial: 3, CanGrow: true
     [FlyingEnemy] Total: 3, Active: 1, Available: 2, Initial: 3, CanGrow: true
     [EliteEnemy] Total: 2, Active: 1, Available: 1, Initial: 2, CanGrow: true
     [EnemyProjectile] Total: 10, Active: 4, Available: 6, Initial: 10, CanGrow: true
   =======================================
   ```

**최적화 기준:**
- ✅ `canGrow = true`: 필요 시 자동 확장
- ✅ Active < Total: 사용 가능한 오브젝트 존재
- ✅ Available > 0: 풀에 여유 있음
- ⚠️ Total이 지속적으로 증가: 초기 크기 부족 → Initializer에서 initialSize 증가 고려

### 6.2 GC 할당 확인

**확인 방법:**
1. Window → Analysis → Profiler 열기
2. Play 모드 실행
3. "GC Alloc" 열 확인

**최적화 포인트:**
- ✅ 적 스폰/반환 시 GC 할당 거의 없음 (풀링 효과)
- ⚠️ GetComponent 호출 최소화 (캐싱 사용)
- ⚠️ string 연결 최소화 (디버그 로그)

---

## 7. Phase C-1 완료 기준

다음 항목을 모두 만족하면 Phase C-1 완료:

- [x] **코드 작성**: 4개 클래스 작성 완료 (EnemyProjectile, RangedEnemy, FlyingEnemy, EliteEnemy)
- [x] **리소스 관리**: ResourcePaths.cs에 경로 추가
- [x] **풀 초기화**: Initializer 패턴 적용
- [x] **에디터 도구**: EnemyDataCreator.cs 작성
- [ ] **에셋 생성**: 3개 EnemyData 에셋 생성 확인
- [ ] **프리팹 생성**: 4개 프리팹 생성 확인
- [ ] **테스트**: 위 체크리스트 80% 이상 통과
- [ ] **문서화**: 이 가이드 작성 완료 ✅

---

## 8. 다음 단계 (Phase C-2 이후)

Phase C-1 완료 후 다음 작업:

1. **적 AI 개선** (Phase C-2):
   - 벽 감지 및 회피
   - 낭떠러지 인식
   - 더 정교한 순찰 경로

2. **적 애니메이션** (Phase C-3):
   - Idle, Walk, Attack, Hit, Dead 애니메이션
   - Animator Controller 설정
   - 애니메이션 이벤트 연동

3. **적 스킬 확장** (Phase C-4):
   - 보스 전용 스킬
   - 원거리 적의 다양한 탄막 패턴
   - 정예 적의 추가 스킬

4. **적 밸런싱** (Phase C-5):
   - 플레이 테스트 기반 스탯 조정
   - 보상 밸런싱
   - 난이도 곡선 조정

---

## 📝 참고 파일 위치

- **코드**: `Assets/_Project/Scripts/Gameplay/Enemy/`
- **데이터**: `Assets/_Project/Data/Enemies/`
- **프리팹**: `Assets/Resources/Prefabs/Enemies/`, `Assets/Resources/Prefabs/Projectiles/`
- **에디터**: `Assets/_Project/Scripts/Editor/EnemyDataCreator.cs`
- **풀 초기화**: `Assets/_Project/Scripts/Gameplay/Enemy/EnemyPoolInitializer.cs`
- **리소스 경로**: `Assets/_Project/Scripts/ResourceManagement/ResourcePaths.cs`

---

**작성자**: Claude Code
**버전**: 1.0
**마지막 수정**: 2025-09-21
