# Phase B-2: 적 스폰 및 전투 시스템 테스트 가이드

**작성일**: 2025-11-12
**브랜치**: `015-playable-prototype-phase-b1`
**커밋**: `447d184`

---

## 📋 목차

1. [테스트 개요](#테스트-개요)
2. [필수 사전 작업](#필수-사전-작업)
3. [환경 설정 체크리스트](#환경-설정-체크리스트)
4. [기능 테스트](#기능-테스트)
5. [통합 테스트](#통합-테스트)
6. [문제 해결 가이드](#문제-해결-가이드)
7. [다음 작업 준비](#다음-작업-준비)

---

## 테스트 개요

Phase B-2는 **적 스폰 및 전투 시스템**을 구현했습니다. 이 문서는 다음 세션에서 기능을 검증하기 위한 상세 테스트 케이스를 제공합니다.

### 구현된 기능
- ✅ EnemySpawnPoint 자동 EnemyData 할당
- ✅ Room 자식 구조로 스폰 포인트 배치
- ✅ 게임 시작 시 자동 던전 시작
- ✅ 투사체-적 충돌 시스템 (Layer 기반)
- ✅ Enemy Layer 및 targetLayers 자동 설정

### 예상 동작
```
[게임 시작] → [첫 방 자동 진입] → [Room_1 진입 시 적 스폰]
    ↓
[플레이어 공격] → [투사체 발사] → [Enemy Layer 충돌 감지]
    ↓
[Enemy.TakeDamage()] → [HP 감소] → [DamageNumber 표시]
    ↓
[HP = 0] → [사망] → [골드/EXP/아이템 드롭] → [풀로 반환]
```

---

## 필수 사전 작업

### ⚠️ 중요: "Enemy" Layer 추가 (필수!)

이 작업을 하지 않으면 **투사체-적 충돌이 전혀 작동하지 않습니다!**

#### 단계별 가이드

1. **Unity 에디터 열기**
   - 프로젝트 열기: `D:\JaeChang\UintyDev\GASPT\GASPT`

2. **Project Settings 열기**
   - 상단 메뉴: `Edit > Project Settings`
   - 또는 단축키: `Ctrl + Shift + ,`

3. **Tags and Layers 선택**
   - 왼쪽 패널에서 `Tags and Layers` 클릭

4. **Layer 6 설정**
   - `Layers` 섹션 확장
   - `Layer 6` 찾기 (빈 칸으로 표시됨)
   - 클릭하여 입력 모드 진입
   - `Enemy` 입력 (정확히 대소문자 일치해야 함)
   - Enter 키로 확인

5. **저장 확인**
   - Layer 6 옆에 "Enemy"가 표시되는지 확인
   - Project Settings 창 닫기 (자동 저장됨)

#### 스크린샷 예시
```
Tags and Layers
├── Builtin Layer 0: Default
├── Builtin Layer 1: TransparentFX
├── Builtin Layer 2: Ignore Raycast
├── Builtin Layer 3: (비어있음)
├── Builtin Layer 4: Water
├── Builtin Layer 5: UI
├── User Layer 6: Enemy          ← 여기에 "Enemy" 입력!
├── User Layer 7: (비어있음)
└── ...
```

---

## 환경 설정 체크리스트

다음 항목을 순서대로 확인하세요.

### 1. Unity 프로젝트 상태 ✅

- [ ] Unity 에디터 실행 (Unity 6.0 이상)
- [ ] 프로젝트 경로: `D:\JaeChang\UintyDev\GASPT\GASPT`
- [ ] Console 창에 에러 없음
- [ ] "Enemy" Layer 추가 완료 (Layer 6)

### 2. Git 브랜치 확인 ✅

```bash
cd D:\JaeChang\UintyDev\GASPT\GASPT
git branch --show-current
# 출력: 015-playable-prototype-phase-b1

git log -1 --oneline
# 출력: 447d184 기능: Phase B-2 적 스폰 및 전투 시스템 완료
```

### 3. 프리팹 재생성 ✅

기존에 생성한 프리팹은 Layer 설정이 없으므로 **반드시 재생성**해야 합니다!

#### 프리팹 재생성 단계
1. Unity 에디터에서 상단 메뉴 클릭: `Tools > GASPT > Prefab Creator`
2. Prefab Creator 창 열림
3. 모든 체크박스 활성화 확인:
   - ✅ MageForm (플레이어)
   - ✅ Projectiles (투사체)
   - ✅ VisualEffect (효과)
   - ✅ BasicMeleeEnemy (적)
4. "🚀 모든 프리팹 생성" 버튼 클릭
5. Console 확인:
   ```
   [PrefabCreator] MageForm 프리팹 생성 완료: Assets/Resources/Prefabs/Player/MageForm.prefab
   [PrefabCreator] MagicMissileProjectile 프리팹 생성 완료: ...
   [PrefabCreator] FireballProjectile 프리팹 생성 완료: ...
   [PrefabCreator] BasicMeleeEnemy 프리팹 생성 완료: ...
   ```
6. 경고 메시지가 없는지 확인 (특히 "'Enemy' Layer가 없습니다" 경고)

#### 프리팹 검증
- [ ] `Assets/Resources/Prefabs/Player/MageForm.prefab` 존재
- [ ] `Assets/Resources/Prefabs/Projectiles/MagicMissileProjectile.prefab` 존재
- [ ] `Assets/Resources/Prefabs/Projectiles/FireballProjectile.prefab` 존재
- [ ] `Assets/Resources/Prefabs/Enemies/BasicMeleeEnemy.prefab` 존재
- [ ] `Assets/Resources/Prefabs/Effects/VisualEffect.prefab` 존재

### 4. GameplayScene 재생성 ✅

기존에 생성한 씬은 스폰 포인트가 Room의 자식이 아니므로 **반드시 재생성**해야 합니다!

#### 씬 재생성 단계
1. Unity 에디터에서 상단 메뉴 클릭: `Tools > GASPT > 🎮 Gameplay Scene Creator`
2. Gameplay Scene Creator 창 열림
3. 모든 체크박스 활성화 확인:
   - ✅ 플레이어
   - ✅ 방 시스템
   - ✅ 플랫폼
   - ✅ 적 스폰 포인트
   - ✅ UI
   - ✅ 카메라
   - ✅ Singleton Manager
4. "🚀 GameplayScene 생성" 버튼 클릭
5. Console 확인:
   ```
   [GameplaySceneCreator] SingletonPreloader 생성 완료
   [GameplaySceneCreator] 3개 방 시스템 생성 완료
   [GameplaySceneCreator] 플랫폼 생성 완료
   [GameplaySceneCreator] 플레이어 생성 완료
   [GameplaySceneCreator] 적 스폰 포인트 생성 완료 (총 X개, EnemyData: TestGoblin)
   [GameplaySceneCreator] UI Canvas 및 EventSystem 생성 완료
   [GameplaySceneCreator] CameraFollow 생성 완료
   === GameplayScene 생성 완료! ===
   ```
6. 완료 다이얼로그 확인

#### 씬 구조 검증
Hierarchy 창에서 다음 구조 확인:

```
GameplayScene
├── Main Camera (CameraFollow 컴포넌트)
├── === SINGLETONS === (SingletonPreloader)
├── === ROOMS ===
│   ├── RoomManager
│   ├── StartRoom (Room 컴포넌트)
│   │   ├── Boundary
│   │   └── (스폰 포인트 없음 - 시작 방)
│   ├── Room_1 (Room 컴포넌트)
│   │   ├── Boundary
│   │   ├── EnemySpawnPoint_0 (EnemySpawnPoint 컴포넌트) ← Room 자식!
│   │   ├── EnemySpawnPoint_1
│   │   └── ...
│   └── BossRoom (Room 컴포넌트)
│       ├── Boundary
│       └── EnemySpawnPoint_0
├── === PLATFORMS ===
│   ├── Ground_Room0
│   ├── Ground_Room1
│   ├── Ground_Room2
│   └── ...
├── Player (MageForm 프리팹)
└── === UI CANVAS === (Canvas + EventSystem)
```

**중요 체크포인트**:
- [ ] EnemySpawnPoint가 Room의 **자식 오브젝트**인지 확인
- [ ] 각 EnemySpawnPoint의 Inspector에서 "Enemy Data: TestGoblin" 확인
- [ ] RoomManager의 Inspector에서 "Auto Start First Room: ✅" 확인

---

## 기능 테스트

### 테스트 1: Layer 설정 확인 🔍

**목적**: 프리팹의 Layer가 올바르게 설정되었는지 확인

#### 단계
1. Project 창에서 프리팹 선택:
   - `Assets/Resources/Prefabs/Enemies/BasicMeleeEnemy.prefab`
2. Inspector 창 상단 확인
3. Layer 드롭다운이 **"Enemy"**로 설정되어 있는지 확인

#### 예상 결과
- ✅ BasicMeleeEnemy Layer: **Enemy**
- ✅ MageForm Layer: **Default**
- ✅ Ground Layer: **Default**

#### 실패 시
- "Enemy" Layer를 추가하지 않았거나
- 프리팹을 재생성하지 않았을 가능성
- → [필수 사전 작업](#필수-사전-작업) 다시 수행

---

### 테스트 2: targetLayers 설정 확인 🎯

**목적**: 투사체의 targetLayers가 Enemy Layer를 포함하는지 확인

#### 단계
1. Project 창에서 프리팹 선택:
   - `Assets/Resources/Prefabs/Projectiles/MagicMissileProjectile.prefab`
2. Inspector 창에서 "MagicMissileProjectile" 스크립트 찾기
3. "Target Layers" 필드 확인

#### 예상 결과
```
MagicMissileProjectile (Script)
├── Speed: 15
├── Max Distance: 20
├── Damage: 10
├── Collision Radius: 0.3
├── Target Layers: Enemy    ← "Enemy" Layer만 체크되어 있음
└── ...
```

#### 체크 방법
- Target Layers 드롭다운 클릭
- "Enemy" 체크박스가 **체크되어 있는지** 확인
- 다른 Layer는 체크 해제되어 있어야 함

#### 실패 시
- targetLayers가 "Nothing" 또는 "Everything"으로 설정됨
- → 프리팹 재생성 필요

---

### 테스트 3: RoomManager 자동 시작 확인 🚀

**목적**: 게임 시작 시 자동으로 첫 방에 진입하는지 확인

#### 단계
1. Hierarchy 창에서 `=== ROOMS === > RoomManager` 선택
2. Inspector 확인
3. RoomManager 스크립트 섹션 확인

#### 예상 결과
```
Room Manager (Script)
├── 방 목록
│   └── Auto Find Rooms: ✅
├── 자동 시작
│   └── Auto Start First Room: ✅    ← 체크되어 있어야 함!
└── 디버그 (읽기 전용)
    └── Rooms: (크기: 3)
```

#### 실패 시
- "Auto Start First Room" 체크박스가 해제됨
- → GameplayScene 재생성 필요

---

### 테스트 4: EnemySpawnPoint 데이터 할당 확인 📦

**목적**: 스폰 포인트에 TestGoblin 데이터가 할당되었는지 확인

#### 단계
1. Hierarchy 창에서 `=== ROOMS === > Room_1 > EnemySpawnPoint_0` 선택
2. Inspector 확인
3. EnemySpawnPoint 스크립트 섹션 확인

#### 예상 결과
```
Enemy Spawn Point (Script)
├── 스폰 설정
│   ├── Enemy Data: TestGoblin    ← TestGoblin 에셋이 할당됨!
│   ├── Spawn Effect: None
│   └── Spawn Delay: 0
└── 디버그
    └── Show Gizmos: ✅
```

#### 체크 포인트
- [ ] "Enemy Data" 필드에 "TestGoblin" 에셋이 할당됨
- [ ] 필드 옆 아이콘을 더블클릭하면 TestGoblin.asset이 열림
- [ ] Scene 뷰에서 빨간색 Gizmo 표시 (스폰 위치)

#### 실패 시
- Enemy Data가 "None"으로 표시됨
- → TestGoblin.asset이 없거나, GameplayScene 재생성 필요

---

## 통합 테스트

### 테스트 5: Play 모드 던전 시작 테스트 ▶️

**목적**: 게임 시작 시 자동으로 첫 방에 진입하는지 확인

#### 단계
1. Hierarchy 창에서 `GameplayScene` 확인
2. Play 버튼 클릭 (또는 `Ctrl + P`)
3. Console 창 모니터링

#### 예상 결과 (Console 로그)
```
[RoomManager] 3개의 방 자동 탐색 (비활성 포함)
[RoomManager] 총 3개의 방 초기화 완료
[RoomManager] 던전 시작!
[Room] StartRoom으로 이동 (1/3)
[Room] StartRoom 진입 완료 - 적 0마리
[RoomManager] 방 진입: StartRoom
```

#### 체크 포인트
- [ ] "던전 시작!" 로그 출력됨
- [ ] "StartRoom으로 이동" 로그 출력됨
- [ ] 에러 없이 게임이 시작됨
- [ ] 플레이어가 Scene 뷰에 보임

#### 실패 시
- RoomManager가 없거나 autoStartFirstRoom이 false
- → [테스트 3](#테스트-3-roommanager-자동-시작-확인-) 확인

---

### 테스트 6: 적 스폰 테스트 👹

**목적**: Room_1 진입 시 적이 스폰되는지 확인

#### 단계
1. Play 모드 실행 중
2. 플레이어를 Room_1으로 이동 (WASD 키 사용)
3. Room_1 경계 진입 시 Console 확인
4. Scene 뷰에서 적 오브젝트 확인

#### 예상 결과 (Console 로그)
```
[Room] Room_1으로 이동 (2/3)
[Room] Room_1: RoomData가 없으므로 스폰 포인트 기본 설정 사용
[EnemySpawnPoint] TestGoblin 스폰 at (x, y, 0)
[EnemySpawnPoint] TestGoblin 스폰 at (x, y, 0)
...
[Room] Room_1 진입 완료 - 적 X마리
```

#### 체크 포인트
- [ ] "TestGoblin 스폰" 로그가 2~4번 출력됨
- [ ] "진입 완료 - 적 X마리" 로그 출력됨
- [ ] Scene 뷰에서 빨간색 스프라이트(적) 보임
- [ ] Hierarchy 창에 "BasicMeleeEnemy(Clone)" 오브젝트 생성됨

#### 수동 진입 방법
플레이어가 자동으로 이동하지 않으면:
1. Hierarchy에서 `=== ROOMS === > RoomManager` 우클릭
2. Context Menu: `Move To Next Room (Test)` 클릭

#### 실패 시
- 적이 스폰되지 않음
- → [테스트 4](#테스트-4-enemyspawnpoint-데이터-할당-확인-) 확인
- → Console에 "EnemyData가 null입니다" 에러 확인

---

### 테스트 7: 플레이어 컨트롤 테스트 🎮

**목적**: 플레이어 이동 및 스킬 사용 확인

#### 단계
1. Play 모드 실행 중
2. 키보드 입력 테스트

#### 컨트롤 체크리스트
- [ ] **A 키**: 왼쪽 이동
- [ ] **D 키**: 오른쪽 이동
- [ ] **Space**: 점프
- [ ] **마우스 좌클릭**: Magic Missile 발사
- [ ] **1번 키**: Teleport (쿨다운 3초)
- [ ] **2번 키**: Fireball (쿨다운 5초)

#### 예상 결과
- 플레이어가 부드럽게 이동함
- 점프 시 포물선 궤적
- 마우스 방향으로 투사체 발사
- Console에 "[MagicMissileAbility] 발사!" 로그 출력

#### 실패 시
- 입력이 동작하지 않음
- → FormInputHandler 컴포넌트 누락
- → PlayerController 컴포넌트 누락

---

### 테스트 8: 투사체-적 충돌 테스트 💥

**목적**: 투사체가 적과 충돌하여 데미지를 주는지 확인

#### 단계
1. Play 모드 실행 중
2. Room_1 진입하여 적 스폰 확인
3. 적을 향해 마우스 좌클릭 (Magic Missile 발사)
4. Console 및 Scene 뷰 모니터링

#### 예상 결과 (Console 로그)
```
[MagicMissileAbility] 발사!
[Projectile] TestGoblin에 10 데미지!
[Enemy] TestGoblin: 10 데미지 받음 (30 → 20)
```

#### 시각적 확인
- [ ] 투사체(청록색 작은 구체)가 적을 향해 날아감
- [ ] 적과 충돌 시 투사체 소멸
- [ ] DamageNumber "10" 텍스트가 적 위에 표시됨 (빨간색)
- [ ] 적 HP가 감소함 (30 → 20 → 10 → 0)

#### 실패 시 (투사체가 적을 관통)
- **원인**: Layer 설정 누락
- **해결**:
  1. Play 모드 중지
  2. [필수 사전 작업](#필수-사전-작업) 다시 수행
  3. 프리팹 재생성

---

### 테스트 9: 적 사망 및 드롭 테스트 💀

**목적**: 적 처치 시 골드/EXP/아이템 드롭 확인

#### 단계
1. Play 모드 실행 중
2. 적에게 Magic Missile 3번 발사 (10 × 3 = 30 데미지)
3. 적 HP가 0이 되면 Console 확인

#### 예상 결과 (Console 로그)
```
[Enemy] TestGoblin: 10 데미지 받음 (30 → 20)
[Enemy] TestGoblin: 10 데미지 받음 (20 → 10)
[Enemy] TestGoblin: 10 데미지 받음 (10 → 0)
[Enemy] TestGoblin 사망!
[Enemy] TestGoblin 골드 드롭: 15~25 골드
[Enemy] TestGoblin EXP 지급: 10 EXP
[Enemy] TestGoblin 아이템 드롭 시도
[Room] Room_1: 적 사망 - 남은 적: X마리
```

#### 시각적 확인
- [ ] 적 오브젝트 비활성화 (1초 후)
- [ ] DamageNumber "10" 표시 (EXP, 파란색)
- [ ] Hierarchy에서 "BasicMeleeEnemy(Clone)" 사라짐

#### 골드/EXP 확인 (선택사항)
1. Hierarchy에서 `=== SINGLETONS === > SingletonPreloader` 선택
2. Inspector에서 "Currency System" 섹션 확인
3. "Current Gold" 값이 증가했는지 확인 (0 → 15~25)

#### 실패 시
- 적이 사망하지 않음
- → Projectile.damage 값 확인 (10이어야 함)
- → Enemy.maxHp 값 확인 (30이어야 함)

---

### 테스트 10: 방 클리어 조건 테스트 🏆

**목적**: 모든 적 처치 시 방이 클리어되는지 확인

#### 단계
1. Play 모드 실행 중
2. Room_1의 모든 적 처치
3. Console 확인

#### 예상 결과 (Console 로그)
```
[Room] Room_1: 적 사망 - 남은 적: 2마리
[Room] Room_1: 적 사망 - 남은 적: 1마리
[Room] Room_1: 적 사망 - 남은 적: 0마리
[Room] Room_1 클리어!
[RoomManager] 방 클리어: Room_1
```

#### 체크 포인트
- [ ] "방 클리어!" 로그 출력됨
- [ ] 모든 적이 사망함
- [ ] 보너스 골드/EXP 지급 (RoomData 설정 시)

#### 실패 시
- 적을 모두 처치해도 클리어 안됨
- → Room 컴포넌트의 Clear Condition 확인
- → Enemy.OnDeath 이벤트 구독 확인

---

## 문제 해결 가이드

### 문제 1: 투사체가 적을 관통함 🚨

**증상**:
- Magic Missile이 적을 통과하여 날아감
- 데미지가 적용되지 않음
- Console에 "[Projectile] TestGoblin에 X 데미지!" 로그 없음

**원인**:
- "Enemy" Layer가 추가되지 않음
- 프리팹이 재생성되지 않음

**해결 방법**:
1. Play 모드 중지
2. [필수 사전 작업](#필수-사전-작업) 단계별로 수행
3. 특히 "Enemy" Layer 추가 확인
4. 프리팹 재생성 필수
5. Play 모드 재실행

---

### 문제 2: 적이 스폰되지 않음 🚨

**증상**:
- Room_1 진입 시 적이 나타나지 않음
- Console에 "TestGoblin 스폰" 로그 없음

**원인**:
- EnemySpawnPoint에 EnemyData가 할당되지 않음
- TestGoblin.asset이 없음
- EnemySpawnPoint가 Room의 자식이 아님

**해결 방법**:
1. TestGoblin.asset 확인:
   - Project 창에서 `Assets/_Project/Data/Enemies/TestGoblin.asset` 찾기
   - 없으면 생성 필요: `Create > GASPT > Enemies > Enemy`
2. GameplayScene 재생성:
   - `Tools > GASPT > Gameplay Scene Creator` 실행
3. EnemySpawnPoint 구조 확인:
   - Hierarchy에서 EnemySpawnPoint가 Room_1의 **자식**인지 확인

---

### 문제 3: 플레이어가 움직이지 않음 🚨

**증상**:
- WASD 키를 눌러도 플레이어가 반응 없음
- 점프도 안됨

**원인**:
- PlayerController 컴포넌트 누락
- FormInputHandler 컴포넌트 누락
- Rigidbody2D 컴포넌트 누락

**해결 방법**:
1. Hierarchy에서 `Player` 선택
2. Inspector 확인:
   - ✅ Rigidbody2D
   - ✅ BoxCollider2D
   - ✅ SpriteRenderer
   - ✅ PlayerController (Script)
   - ✅ FormInputHandler (Script)
   - ✅ MageForm (Script)
3. 프리팹 확인:
   - `Assets/Resources/Prefabs/Player/MageForm.prefab` 열기
   - 모든 컴포넌트 있는지 확인
4. 프리팹 재생성 후 씬 재생성

---

### 문제 4: 게임 시작 시 아무 일도 안 일어남 🚨

**증상**:
- Play 버튼 클릭해도 정적임
- Console에 "던전 시작!" 로그 없음

**원인**:
- RoomManager의 autoStartFirstRoom이 false
- SingletonPreloader가 없음

**해결 방법**:
1. Hierarchy에서 `=== SINGLETONS ===` 확인
   - 없으면 GameplayScene 재생성
2. `=== ROOMS === > RoomManager` 선택
3. Inspector에서 "Auto Start First Room" 체크박스 **활성화**
4. Play 모드 재실행

---

### 문제 5: DamageNumber가 표시되지 않음 🚨

**증상**:
- 적이 데미지를 받지만 숫자가 안 보임
- Console에 "[Enemy] X 데미지 받음" 로그는 있음

**원인**:
- DamageNumberPool이 초기화되지 않음
- DamageNumber 프리팹이 없음

**해결 방법**:
1. DamageNumber 프리팹 확인:
   - `Assets/Resources/Prefabs/UI/DamageNumber.prefab` 찾기
   - 없으면 생성: `Tools > GASPT > Create DamageNumber Prefab`
2. SingletonPreloader 확인:
   - Hierarchy에서 `=== SINGLETONS ===` 선택
   - Inspector에서 "DamageNumberPool" 섹션 확인
3. Play 모드 재실행

---

## 다음 작업 준비

### 테스트 완료 후 체크리스트 ✅

모든 테스트를 통과했다면 다음을 확인하세요:

- [ ] 투사체-적 충돌 정상 작동
- [ ] 적 스폰 정상 작동
- [ ] 데미지 시스템 정상 작동
- [ ] 골드/EXP 드롭 정상 작동
- [ ] 방 클리어 정상 작동
- [ ] 플레이어 컨트롤 정상 작동

### 다음 Phase B 작업 옵션

#### 옵션 1: Phase B-3 - UI 시스템 통합 🎨
- PlayerHealthBar 배치
- PlayerExpBar 배치
- BuffIconPanel 배치
- ItemPickupUI 배치
- 방 정보 UI 추가

#### 옵션 2: Phase B-4 - 다양한 적 추가 👹
- RangedEnemy (원거리 공격)
- FlyingEnemy (비행)
- EliteEnemy (정예)
- BossEnemy (보스)

#### 옵션 3: Phase B-5 - 추가 Form 구현 🦸
- WarriorForm (전사)
- AssassinForm (암살자)
- TankForm (탱커)

---

## 추가 디버그 도구

### Context Menu 활용

#### RoomManager 디버그
1. Hierarchy에서 `=== ROOMS === > RoomManager` 우클릭
2. Context Menu 선택:
   - `Print Room List` - 모든 방 목록 출력
   - `Refresh Rooms` - 방 목록 새로고침
   - `Start Dungeon (Test)` - 던전 수동 시작
   - `Move To Next Room (Test)` - 다음 방으로 이동

#### EnemySpawnPoint 디버그
1. Hierarchy에서 `EnemySpawnPoint_0` 우클릭
2. Context Menu 선택:
   - `Test Spawn` - 적 수동 스폰
   - `Print Info` - 스폰 포인트 정보 출력

#### Enemy 디버그
1. Hierarchy에서 `BasicMeleeEnemy(Clone)` 우클릭
2. Context Menu 선택:
   - `Print Enemy Info` - 적 정보 출력
   - `Take 10 Damage (Test)` - 10 데미지 테스트
   - `Instant Death (Test)` - 즉사 테스트

### Console 필터링

중요 로그만 보려면 Console 창에서 필터 설정:
- `[Room]` - 방 관련 로그
- `[Enemy]` - 적 관련 로그
- `[Projectile]` - 투사체 관련 로그
- `[RoomManager]` - 던전 관련 로그

---

## 최종 점검

### 모든 테스트 통과 확인 ✅

- ✅ **테스트 1**: Layer 설정 확인
- ✅ **테스트 2**: targetLayers 설정 확인
- ✅ **테스트 3**: RoomManager 자동 시작 확인
- ✅ **테스트 4**: EnemySpawnPoint 데이터 할당 확인
- ✅ **테스트 5**: Play 모드 던전 시작 테스트
- ✅ **테스트 6**: 적 스폰 테스트
- ✅ **테스트 7**: 플레이어 컨트롤 테스트
- ✅ **테스트 8**: 투사체-적 충돌 테스트
- ✅ **테스트 9**: 적 사망 및 드롭 테스트
- ✅ **테스트 10**: 방 클리어 조건 테스트

### 다음 세션 준비 완료! 🎉

Phase B-2 적 스폰 및 전투 시스템이 정상적으로 작동한다면 다음 작업을 시작할 수 있습니다!

---

**작성일**: 2025-11-12
**문서 버전**: 1.0
**다음 업데이트**: Phase B-3 작업 시
