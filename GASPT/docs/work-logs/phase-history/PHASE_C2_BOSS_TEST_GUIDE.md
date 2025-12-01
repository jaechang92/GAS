# Phase C-2 보스 전투 시스템 테스트 가이드

**작성 날짜**: 2025-11-16
**작성자**: Claude Code
**목표**: 보스 전투 시스템 구현 완료 및 테스트

---

## 📋 Phase C-2 개요

Phase C-2에서는 **3단계 보스 전투 시스템**을 구현했습니다.

### 구현된 주요 기능

1. **BossPhaseController** - HP 비율 기반 Phase 자동 전환
2. **BossEnemy** - Phase별 패턴 공격 시스템
3. **EnemyData 확장** - 보스 전용 스탯 설정
4. **EnemyClass.Boss** - 새로운 적 클래스 타입
5. **FireDragon.asset** - 보스 데이터 에셋

---

## 🎯 보스 Phase 시스템

### Phase 1 (HP 100% ~ 70%)
**패턴**:
- 근접 공격 (플레이어 접근 시)
- 원거리 공격 (3초마다 투사체 발사)

**스탯 배율**:
- 공격력: x1.0
- 이동속도: x1.0

### Phase 2 (HP 70% ~ 30%)
**패턴**:
- 근접 공격
- 원거리 공격 (3초마다)
- 돌진 공격 (5초마다, 10유닛 거리)
- 소환 (10초마다, 최대 3마리)

**스탯 배율**:
- 공격력: x1.2 (+20%)
- 이동속도: x1.3 (+30%)

### Phase 3 (HP 30% ~ 0%)
**패턴**:
- 근접 공격
- 원거리 공격 (3초마다)
- 돌진 공격 (5초마다)
- 범위 공격 (7초마다, 반경 5유닛)

**스탯 배율**:
- 공격력: x1.5 (+50%)
- 이동속도: x1.3 (+30%)

---

## 🛠️ 사용 방법

### 1단계: FireDragon 데이터 에셋 생성

Unity Editor에서:

1. **Tools > GASPT > Enemy Data Creator** 메뉴 열기
2. **"⭐ FireDragon 데이터 생성 (보스)"** 버튼 클릭
3. 생성 확인: `Assets/_Project/Data/Enemies/FireDragon.asset`

**또는 전체 생성**:
- **"🎯 모든 EnemyData 에셋 생성"** 버튼 클릭
- RangedGoblin, FlyingBat, EliteOrc, FireDragon 모두 생성

---

### 2단계: BossEnemy 프리팹 생성 (수동)

#### 방법 A: Hierarchy에서 직접 생성

1. **Hierarchy 우클릭 > Create Empty**
2. 이름을 **"BossEnemy_FireDragon"**으로 변경
3. **Add Component > Boss Enemy** 스크립트 추가
4. **Add Component > Rigidbody 2D** 추가
   - Gravity Scale: 2
   - Freeze Rotation: true
   - Collision Detection: Continuous
5. **Add Component > Box Collider 2D** 또는 **Capsule Collider 2D** 추가
6. **Add Component > Sprite Renderer** 추가 (임시 스프라이트)

#### BossEnemy Inspector 설정

**Enemy 설정 (상속)**:
- **Data**: `FireDragon.asset` 할당

**BossEnemy 전용 설정**:
- **Boss Health Bar Prefab**: `BossHealthBar.prefab` 할당
  - 위치: `Assets/_Project/Prefabs/UI/BossHealthBar.prefab`
- **Minion Prefab**: `BasicMeleeEnemy.prefab` 할당 (소환용)
  - 위치: `Assets/_Project/Prefabs/Enemies/BasicMeleeEnemy.prefab`
- **Max Summon Count**: 3

#### 7. 프리팹으로 저장

1. **Project 창**에서 `Assets/_Project/Prefabs/Enemies/` 폴더로 이동
2. **Hierarchy의 BossEnemy_FireDragon**을 **Project 창으로 드래그**
3. **프리팹 생성 완료**

---

### 3단계: 씬에서 보스 테스트

#### 테스트 씬 설정

1. **GameplayScene 열기**
2. **BossEnemy_FireDragon 프리팹**을 씬에 배치
3. **플레이어** 위치 확인 (PlayerStats 컴포넌트가 있는 GameObject)
4. **Canvas** 존재 확인 (BossHealthBar UI용)

#### 플레이 모드 진입

1. **Play 버튼** 클릭
2. 보스 초기화 로그 확인:
   ```
   [BossEnemy] 화염 드래곤 보스 초기화 완료
   [BossEnemy] Phase Controller 초기화 완료
   [BossEnemy] 화염 드래곤 체력바 생성 완료
   ```
3. **BossHealthBar**가 화면 상단에 표시되는지 확인

---

## ✅ 테스트 체크리스트

### 기본 기능 테스트

- [ ] **보스 스폰**: BossEnemy가 씬에서 정상적으로 생성됨
- [ ] **BossHealthBar UI**: 화면 상단에 보스 이름과 체력바 표시
- [ ] **플레이어 감지**: 보스가 플레이어를 감지하고 추격

### Phase 1 테스트 (HP 100% ~ 70%)

- [ ] **근접 공격**: 플레이어 접근 시 근접 공격 실행
- [ ] **원거리 공격**: 3초마다 투사체 발사
- [ ] **투사체 데미지**: 플레이어가 투사체에 맞으면 데미지 (20)

### Phase 2 테스트 (HP 70% ~ 30%)

- [ ] **Phase 전환 로그**: HP 70% 도달 시 "Phase 2 진입" 로그 출력
- [ ] **돌진 공격**: 5초마다 플레이어에게 돌진
- [ ] **소환**: 10초마다 BasicMeleeEnemy 소환 (최대 3마리)
- [ ] **스탯 증가**: 공격력 +20%, 이동속도 +30% 확인

### Phase 3 테스트 (HP 30% ~ 0%)

- [ ] **Phase 전환 로그**: HP 30% 도달 시 "Phase 3 진입! 광폭화 상태!" 로그 출력
- [ ] **범위 공격**: 7초마다 반경 5유닛 범위 공격
- [ ] **범위 공격 데미지**: 플레이어가 범위 내에 있으면 데미지 (60, Phase 3 배율 적용)
- [ ] **스탯 증가**: 공격력 +50%, 이동속도 +30% 확인

### UI 테스트

- [ ] **체력바 감소**: 보스 HP 감소 시 체력바가 부드럽게 감소
- [ ] **체력 텍스트**: "현재 HP / 최대 HP" 텍스트 정상 업데이트
- [ ] **체력바 색상**: HP 30% 이하일 때 빨간색으로 변경
- [ ] **보스 사망**: 보스 처치 시 체력바 제거

### 보상 테스트

- [ ] **골드 드롭**: 보스 처치 시 200-300 골드 드롭
- [ ] **경험치 보상**: 500 경험치 획득

---

## 🐛 디버깅 가이드

### Console 로그 확인

BossEnemy의 `showDebugLogs`를 **true**로 설정하면 자세한 로그 출력:

```
[BossEnemy] 원거리 공격 발사! 방향: (0.8, 0.6)
[BossEnemy] 돌진 공격 시작! (10.0, 2.0) → (18.0, 2.0)
[BossEnemy] 돌진 공격 완료!
[BossEnemy] 소환 완료! 위치: (12.5, 2.0), 현재 소환 수: 1
[BossEnemy] 범위 공격 적중! 데미지: 60
```

### 일반적인 문제 및 해결

#### 문제 1: BossHealthBar가 표시되지 않음
**원인**: Canvas가 씬에 없거나 BossHealthBarPrefab이 할당되지 않음
**해결**:
- Canvas 확인: `FindAnyObjectByType<Canvas>()` 결과 null 여부
- Inspector에서 **Boss Health Bar Prefab** 할당 확인

#### 문제 2: 원거리 공격이 발사되지 않음
**원인**: EnemyProjectile이 PoolManager에 등록되지 않음
**해결**:
- **PoolManager**에서 `EnemyProjectile` 프리팹 등록 확인
- PoolInitializer에 EnemyProjectile 추가

#### 문제 3: 소환된 적이 나타나지 않음
**원인**: Minion Prefab이 할당되지 않음
**해결**:
- Inspector에서 **Minion Prefab** 필드에 `BasicMeleeEnemy.prefab` 할당

#### 문제 4: Phase 전환이 안 됨
**원인**: OnHpChanged 이벤트가 발생하지 않거나 phaseController가 null
**해결**:
- Enemy.cs의 `TakeDamage()` 메서드에서 `OnHpChanged?.Invoke()` 호출 확인
- BossEnemy.Start()에서 phaseController 초기화 로그 확인

---

## 📊 성능 확인

### 프레임레이트 체크

- **보스 + 소환 3마리 + 플레이어**: 60 FPS 이상 유지 권장
- **범위 공격 시각 효과**: DrawCircle 호출 시 프레임 드롭 확인

### 메모리 사용량

- **BossHealthBar**: 1개 생성, 보스 사망 시 자동 제거
- **소환된 적**: 최대 3마리로 제한, 메모리 누수 없음

---

## 🎮 플레이 테스트 시나리오

### 시나리오 1: Phase 전환 확인

1. **보스 HP를 70%까지 감소**
   - "Phase 2 진입!" 로그 확인
   - 돌진 공격 시작 확인
2. **보스 HP를 30%까지 감소**
   - "Phase 3 진입! 광폭화 상태!" 로그 확인
   - 범위 공격 시작 확인

### 시나리오 2: 소환 시스템 확인

1. **Phase 2 진입 후 10초 대기**
2. **BasicMeleeEnemy 1마리 소환 확인**
3. **추가로 20초 대기**
4. **총 3마리까지 소환되는지 확인**
5. **3마리 제한 확인** (더 이상 소환되지 않음)

### 시나리오 3: 범위 공격 회피

1. **Phase 3 진입**
2. **보스 주변 5유닛 이내 위치**
3. **7초마다 범위 공격 발동 확인**
4. **범위 밖으로 이동하여 회피 가능한지 확인**

---

## 📝 추가 개선 사항 (선택)

### 우선순위: 낮음

- [ ] **보스 등장 연출**: 보스 스폰 시 컷신 또는 특수 이펙트
- [ ] **범위 공격 시각 효과**: VisualEffect 풀 사용하여 범위 표시
- [ ] **돌진 공격 이펙트**: 돌진 중 잔상 또는 파티클
- [ ] **Phase 전환 이펙트**: Phase 변경 시 화면 효과 또는 사운드
- [ ] **보스 애니메이션**: Animator 컨트롤러 추가
- [ ] **보스 사운드**: 공격, Phase 전환, 사망 사운드

### 우선순위: 중간

- [ ] **보스 방 진입 연출**: Room.cs에서 보스 방 진입 시 문 잠금
- [ ] **보스 처치 보상**: 특별 아이템 드롭 (고급 장비)
- [ ] **보스 AI 개선**: 플레이어 패턴 학습, 예측 공격

---

## ✅ Phase C-2 완료 기준

### 필수 요구사항

- [x] BossPhaseController 구현 (HP 비율 기반 Phase 전환)
- [x] BossEnemy 구현 (3가지 Phase별 패턴)
- [x] EnemyData 보스 전용 설정 추가
- [x] EnemyClass.Boss 타입 추가
- [x] FireDragon 데이터 에셋 생성 도구
- [ ] 보스 전투 테스트 완료 (모든 Phase 정상 작동)
- [ ] BossHealthBar UI 정상 표시

### 선택 요구사항

- [ ] 보스 스폰 포인트 (BossSpawnPoint.cs)
- [ ] Room.cs 보스 방 로직 추가
- [ ] GameplaySceneCreator.cs 보스 방 생성 자동화

---

## 🚀 다음 단계

Phase C-2 완료 후 진행 가능한 작업:

1. **Phase C-3**: 던전 진행 완성 (포탈, 방 클리어 보상)
2. **Phase C-4**: 아이템 드롭 및 장착
3. **보스 밸런싱**: 플레이 테스트 기반 HP, 데미지 조정
4. **보스 추가**: 다른 타입의 보스 적 구현

---

**작성자**: Claude Code
**최종 수정**: 2025-11-16
**문서 버전**: 1.0
**Phase C-2 상태**: ✅ 구현 완료, 테스트 대기 중
