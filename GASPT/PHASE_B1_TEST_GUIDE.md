# Phase B-1 플레이 가능한 프로토타입 테스트 가이드

**작성일**: 2025-11-12
**브랜치**: 015-playable-prototype-phase-b1
**목적**: 프리팹 및 GameplayScene 생성 확인 및 플레이 테스트

---

## 📋 사전 준비 체크리스트

### 1. 프리팹 생성 확인
- [ ] `Tools > GASPT > Prefab Creator` 실행 완료
- [ ] 5개 프리팹 생성 확인:
  - [ ] `Resources/Prefabs/Player/MageForm.prefab`
  - [ ] `Resources/Prefabs/Projectiles/MagicMissileProjectile.prefab`
  - [ ] `Resources/Prefabs/Projectiles/FireballProjectile.prefab`
  - [ ] `Resources/Prefabs/Effects/VisualEffect.prefab`
  - [ ] `Resources/Prefabs/Enemies/BasicMeleeEnemy.prefab`

### 2. Sprite 에셋 생성 확인
- [ ] `Resources/Textures/Placeholders/` 폴더 존재
- [ ] 최소 5개 Sprite 에셋 생성:
  - [ ] `Placeholder_7F7FFF.png` (파란색 - 플레이어)
  - [ ] `Placeholder_00FFFF.png` (청록색 - MagicMissile)
  - [ ] `Placeholder_FF7F00.png` (주황색 - Fireball)
  - [ ] `Placeholder_FFFFFF.png` (흰색 - VisualEffect)
  - [ ] `Placeholder_FF4C4C.png` (빨간색 - Enemy)

### 3. GameplayScene 생성 확인
- [ ] `Tools > GASPT > 🎮 Gameplay Scene Creator` 실행 완료
- [ ] `Assets/_Project/Scenes/GameplayScene.unity` 생성됨
- [ ] Scene이 정상적으로 열림

---

## 🎮 GameplayScene 구조 확인

### Hierarchy 체크리스트

#### 1. 카메라 시스템
- [ ] **Main Camera** 존재
  - [ ] Orthographic 카메라 설정
  - [ ] Orthographic Size: 10
  - [ ] CameraFollow 컴포넌트 존재
  - [ ] Background Color: 어두운 파란색 (약 0.1, 0.1, 0.15)

#### 2. Singleton Manager
- [ ] **=== SINGLETONS ===** 오브젝트 존재
  - [ ] SingletonPreloader 컴포넌트 존재

#### 3. Room System
- [ ] **=== ROOMS ===** 오브젝트 존재
  - [ ] **RoomManager** 자식 오브젝트 존재
    - [ ] RoomManager 컴포넌트 존재
  - [ ] **StartRoom** 존재 (위치: 0, 0, 0)
  - [ ] **Room_1** 존재 (위치: 40, 0, 0 또는 설정한 roomWidth)
  - [ ] **Room_2** 또는 **BossRoom** 존재

#### 4. 플랫폼
- [ ] **=== PLATFORMS ===** 오브젝트 존재
  - [ ] 각 방마다 **Ground_RoomN** 존재
    - [ ] SpriteRenderer 존재 (회색)
    - [ ] BoxCollider2D 존재
    - [ ] 위치: Y = -2
  - [ ] 각 방마다 2~3개 **Platform_RoomN_M** 존재
    - [ ] SpriteRenderer 존재 (밝은 회색)
    - [ ] BoxCollider2D 존재
    - [ ] 위치: Y = 2~10 (랜덤)

#### 5. 플레이어
- [ ] **Player** 오브젝트 존재
  - [ ] 위치: 약 (0, 2, 0)
  - [ ] Tag: "Player"
  - [ ] **컴포넌트 확인**:
    - [ ] Rigidbody2D 존재 (Gravity Scale: 3)
    - [ ] BoxCollider2D 존재
    - [ ] SpriteRenderer 존재
      - [ ] Sprite: Placeholder_7F7FFF (파란색)
      - [ ] Color: White
    - [ ] PlayerController 존재
    - [ ] FormInputHandler 존재
    - [ ] MageForm 존재
  - [ ] **GroundCheck** 자식 오브젝트 존재
    - [ ] 위치: (0, 0, 0) - 플레이어 발 위치

#### 6. 적 스폰 포인트
- [ ] **=== ENEMY SPAWN POINTS ===** 오브젝트 존재
  - [ ] Room_1 이상에 **EnemySpawnPoint_RoomN_M** 존재
    - [ ] EnemySpawnPoint 컴포넌트 존재
    - [ ] 위치: 각 방의 랜덤 위치
  - [ ] 시작 방(StartRoom)에는 스폰 포인트 없음 확인

#### 7. UI 시스템
- [ ] **=== UI CANVAS ===** 오브젝트 존재
  - [ ] Canvas 컴포넌트 존재
    - [ ] Render Mode: Screen Space - Overlay
    - [ ] Canvas Scaler 존재
    - [ ] Graphic Raycaster 존재
- [ ] **EventSystem** 오브젝트 존재
  - [ ] Event System 컴포넌트 존재
  - [ ] Standalone Input Module 존재

---

## 🕹️ 플레이 테스트 체크리스트

### 1단계: Scene 뷰 확인
- [ ] Scene 뷰에서 플레이어가 **파란색 사각형**으로 보임
- [ ] Ground가 **회색 긴 사각형**으로 보임 (방 전체 너비)
- [ ] 점프 플랫폼이 **밝은 회색 사각형**으로 보임 (2~3개씩)
- [ ] 적 스폰 포인트에 **빨간색 아이콘** 표시 (Gizmo)

### 2단계: Play 모드 진입
- [ ] Play 버튼 클릭
- [ ] Console에 에러 없음
- [ ] Console에 Singleton 초기화 로그 확인:
  ```
  [SingletonPreloader] GameResourceManager 초기화 완료
  [SingletonPreloader] PoolManager 초기화 완료
  ...
  ```

### 3단계: 기본 조작 테스트

#### 이동 테스트
- [ ] **A키**: 플레이어가 왼쪽으로 이동
- [ ] **D키**: 플레이어가 오른쪽으로 이동
- [ ] **S키**: (아무 동작 없음 - 정상)
- [ ] **W키**: (아무 동작 없음 - 정상)
- [ ] 플레이어 스프라이트가 이동 방향에 따라 **좌우 반전**됨

#### 점프 테스트
- [ ] **Space키**: 플레이어가 점프
- [ ] 점프 높이: 적절함 (약 4~5 유닛)
- [ ] 공중에서 좌우 이동 가능 (airControlMultiplier: 0.8)
- [ ] 2단 점프 불가 (정상)
- [ ] Ground에 착지 후 다시 점프 가능

#### 지면 충돌 테스트
- [ ] 플레이어가 Ground 위에 **서있음** (떨어지지 않음)
- [ ] 점프 플랫폼 위에 **착지 가능**
- [ ] 플랫폼 아래에서 위로 뚫고 올라갈 수 있음 (One-way platform 아님)

#### 카메라 추적 테스트
- [ ] 카메라가 플레이어를 **부드럽게 따라감**
- [ ] 플레이어가 항상 화면 중앙에 위치
- [ ] 카메라가 플레이어보다 약간 **위쪽**을 비춤 (offset.y = 1)

### 4단계: 스킬 테스트

#### 기본 공격 (Magic Missile) - 슬롯 0
- [ ] **마우스 왼쪽 클릭**: 스킬 실행
- [ ] Console에 로그:
  ```
  [MagicMissileAbility] Magic Missile 발사!
  ```
- [ ] 청록색 투사체가 **마우스 방향**으로 발사됨
- [ ] 투사체가 화면 밖으로 나가면 **자동 소멸**
- [ ] 쿨다운: 0.5초

#### 스킬 1 (Teleport) - Q키
- [ ] **Q키**: 텔레포트 실행
- [ ] Console에 로그:
  ```
  [TeleportAbility] 텔레포트 시전!
  ```
- [ ] 플레이어가 **마우스 방향**으로 5m 이동
- [ ] 쿨다운: 3초
- [ ] 쿨다운 중에는 스킬 사용 불가
  ```
  [TeleportAbility] 쿨다운 중...
  ```

#### 스킬 2 (Fireball) - E키
- [ ] **E키**: Fireball 실행
- [ ] Console에 로그:
  ```
  [FireballAbility] 화염구 발사!
  ```
- [ ] 주황색 투사체가 **마우스 방향**으로 발사됨
- [ ] 투사체가 크다 (MagicMissile보다 큼)
- [ ] 투사체 속도가 느림 (약 8 units/s)
- [ ] 쿨다운: 5초

### 5단계: 물리 및 충돌 테스트

#### Rigidbody2D 테스트
- [ ] 플레이어가 **중력**의 영향을 받음 (떨어짐)
- [ ] Gravity Scale: 3 (적절한 중력)
- [ ] 플레이어가 회전하지 않음 (Freeze Rotation: true)

#### Collider 테스트
- [ ] 플레이어와 Ground가 **충돌** (통과하지 않음)
- [ ] 플레이어와 플랫폼이 **충돌** (통과하지 않음)
- [ ] 투사체가 벽을 통과하지 않음 (만약 벽이 있다면)

#### GroundCheck 테스트
- [ ] Scene 뷰에서 **GroundCheck Gizmo** 표시됨
  - [ ] 지면에 있을 때: **초록색** 원
  - [ ] 공중에 있을 때: **빨간색** 원
- [ ] GroundCheck가 플레이어 **발 위치**에 있음
- [ ] Radius: 0.2 (적절한 크기)

### 6단계: Singleton 시스템 테스트

#### Console 로그 확인
- [ ] 게임 시작 시 모든 Singleton 초기화 로그:
  ```
  [SingletonPreloader] GameResourceManager 초기화 완료
  [SingletonPreloader] PoolManager 초기화 완료
  [SingletonPreloader] DamageNumberPool 초기화 완료
  [SingletonPreloader] CurrencySystem 초기화 완료
  [SingletonPreloader] InventorySystem 초기화 완료
  [SingletonPreloader] PlayerLevel 초기화 완료
  [SingletonPreloader] SaveSystem 초기화 완료
  [SingletonPreloader] StatusEffectManager 초기화 완료
  [SingletonPreloader] SkillSystem 초기화 완료
  [SingletonPreloader] LootSystem 초기화 완료
  [SingletonPreloader] SkillItemManager 초기화 완료
  ```
- [ ] 총 11개 Singleton 초기화 완료

### 7단계: 오브젝트 풀링 테스트

#### 투사체 풀링
- [ ] MagicMissile을 **10회 이상** 발사
  - [ ] Console에 "Pool not found" 에러 없음
  - [ ] 투사체가 정상적으로 재사용됨
- [ ] Fireball을 **5회 이상** 발사
  - [ ] 투사체가 정상적으로 재사용됨

#### VisualEffect 풀링 (스킬 사용 시)
- [ ] Teleport, Fireball 사용 시 효과 표시
  - [ ] 효과가 자동으로 사라짐 (풀로 반환)

---

## ⚠️ 알려진 제한사항

### 현재 미구현 기능
- [ ] **적 AI**: EnemySpawnPoint가 있지만 아직 적이 스폰되지 않음
- [ ] **전투**: 투사체가 적과 충돌하지 않음 (적이 없음)
- [ ] **UI**: HealthBar, ManaBar 등 UI 미생성 (별도 생성 필요)
- [ ] **Room 전환**: 방 사이 이동 로직 미구현
- [ ] **스킬 쿨다운 UI**: 스킬 쿨다운 시각화 없음
- [ ] **사운드**: 효과음 및 배경음악 없음
- [ ] **애니메이션**: 스프라이트 애니메이션 없음

### 예상 동작
- **적이 없음**: EnemySpawnPoint만 있고 실제 적 스폰 안 됨
- **투사체가 적중해도 효과 없음**: 적이 없어서 데미지 적용 안 됨
- **UI 없음**: HP, Mana 정보 표시 안 됨 (별도 생성 필요)
- **방 전환 안 됨**: 모든 방이 한 화면에 나열됨

---

## 🐛 문제 발생 시 해결 방법

### 1. 플레이어가 보이지 않음
**증상**: Scene 뷰에서 플레이어가 안 보임

**해결**:
1. Player 오브젝트 선택
2. SpriteRenderer 컴포넌트 확인
3. Sprite 필드가 None이면:
   - 프리팹 재생성 (Prefab Creator 재실행)
   - 또는 수동으로 Placeholder_7F7FFF Sprite 할당

### 2. 플레이어가 Ground를 통과함
**증상**: 플레이어가 바닥을 뚫고 떨어짐

**해결**:
1. Ground 오브젝트 선택
2. **BoxCollider2D** 컴포넌트 확인 (3D Collider 아님!)
3. 없으면 BoxCollider2D 추가
4. 또는 씬 재생성 (Gameplay Scene Creator 재실행)

### 3. Console에 에러
**증상**: Play 모드에서 빨간 에러 메시지

**일반적인 해결**:
1. 에러 메시지 복사
2. 관련 스크립트 확인
3. 누락된 컴포넌트 확인
4. Singleton 초기화 실패 시 → SingletonPreloader 확인

### 4. 스킬이 작동하지 않음
**증상**: 마우스 클릭/키보드 입력 시 아무 반응 없음

**해결**:
1. Player 오브젝트 선택
2. FormInputHandler 컴포넌트 확인
3. Target Form 필드 확인 (자동으로 MageForm 연결되어야 함)
4. MageForm 컴포넌트의 Ability 슬롯 확인

### 5. 카메라가 플레이어를 따라가지 않음
**증상**: 플레이어 이동 시 화면 고정

**해결**:
1. Main Camera 선택
2. CameraFollow 컴포넌트 확인
3. Target 필드가 비어있으면:
   - Play 모드에서 자동으로 "Player" 태그 찾음
   - 안 되면 수동으로 Player 오브젝트 할당

### 6. 투사체가 발사되지 않음
**증상**: 스킬 사용 시 투사체가 나타나지 않음

**해결**:
1. Console 로그 확인
2. "Pool not found" 메시지 있으면:
   - SingletonPreloader 확인
   - PoolManager 초기화 확인
   - 프리팹 경로 확인 (Resources/Prefabs/)

---

## ✅ 최종 확인 체크리스트

### 기본 동작
- [ ] 플레이어 이동 (WASD) 정상
- [ ] 점프 (Space) 정상
- [ ] 지면 충돌 정상
- [ ] 카메라 추적 정상

### 스킬 시스템
- [ ] 기본 공격 (마우스 클릭) 정상
- [ ] 스킬 1 (Q) 정상
- [ ] 스킬 2 (E) 정상
- [ ] 쿨다운 시스템 정상

### 시스템
- [ ] Singleton 초기화 정상
- [ ] 오브젝트 풀링 정상
- [ ] Console에 에러 없음

### 씬 구조
- [ ] Hierarchy 구조 올바름
- [ ] 모든 필수 오브젝트 존재
- [ ] 컴포넌트 설정 올바름

---

## 📝 테스트 완료 후 다음 단계

### 1. UI 추가 (선택사항)
```
Tools > GASPT > UI > Create Player HealthBar UI
Tools > GASPT > UI > Create Player ManaBar UI
Tools > GASPT > UI > Create Player ExpBar UI
Tools > GASPT > UI > Create Buff Icon UI
Tools > GASPT > UI > Create Item Pickup UI
```

### 2. 적 스폰 테스트
- EnemyData 생성 및 EnemySpawnPoint에 할당
- Play 모드에서 적 스폰 확인

### 3. 전투 테스트
- 투사체와 적 충돌 확인
- 데미지 적용 확인
- DamageNumber 표시 확인

### 4. 밸런싱
- 플레이어 이동속도 조정
- 점프력 조정
- 스킬 쿨다운 조정
- 데미지 조정

### 5. 버그 수정
- 발견된 문제 기록
- 우선순위 설정
- 수정 작업

---

## 📊 테스트 결과 기록

**테스트 일시**: ____________________
**테스터**: ____________________
**브랜치**: 015-playable-prototype-phase-b1

### 전체 통과율
- [ ] 100% 통과 (모든 항목 체크)
- [ ] 90% 이상 통과 (1~2개 항목 실패)
- [ ] 80% 이상 통과 (3~5개 항목 실패)
- [ ] 80% 미만 통과 (재생성 필요)

### 주요 발견 사항
```
(여기에 테스트 중 발견한 문제나 개선점 기록)




```

### 다음 작업 우선순위
```
(다음에 진행할 작업 기록)

1.
2.
3.
```

---

**테스트 가이드 끝**
