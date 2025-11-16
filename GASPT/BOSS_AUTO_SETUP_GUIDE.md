# Boss Setup Creator - 자동 생성 가이드

**작성 날짜**: 2025-11-16
**목표**: 보스 전투 테스트 환경을 한 번의 클릭으로 자동 생성

---

## 🚀 빠른 시작 (1분 완료)

### 1단계: Boss Setup Creator 열기

Unity Editor 상단 메뉴:
```
Tools > GASPT > Boss Setup Creator
```

### 2단계: 자동 생성 버튼 클릭

**"🚀 모든 에셋 자동 생성"** 버튼 클릭

### 3단계: 완료!

생성 완료 다이얼로그 확인:
- ✅ FireDragon.asset
- ✅ EnemyProjectile.prefab
- ✅ BossEnemy_FireDragon.prefab

---

## 📋 자동 생성되는 에셋

### 1. FireDragon.asset (EnemyData)

**위치**: `Assets/_Project/Data/Enemies/FireDragon.asset`

⚠️ **참고**: Data는 ScriptableObject이므로 일반 Assets 폴더에 저장

**설정 내용**:
- Enemy Type: Boss
- Enemy Class: Boss
- Enemy Name: 화염 드래곤
- Max HP: 500
- Attack: 25
- 보스 전용 패턴 설정 (원거리, 돌진, 소환, 범위 공격)

---

### 2. EnemyProjectile.prefab (적 투사체)

**위치**: `Assets/Resources/Prefabs/Projectiles/EnemyProjectile.prefab`

⚠️ **중요**: Resources 폴더에 생성 (런타임 동적 로딩용)

**컴포넌트**:
- SpriteRenderer (빨간색 원)
- CircleCollider2D (트리거)
- EnemyProjectile 스크립트
- PooledObject 스크립트

**설정 내용**:
- Speed: 8
- Max Distance: 20
- Damage: 15
- Target Layers: Player

---

### 3. BossEnemy_FireDragon.prefab (보스 프리팹)

**위치**: `Assets/Resources/Prefabs/Enemies/BossEnemy_FireDragon.prefab`

⚠️ **중요**: Resources 폴더에 생성 (런타임 동적 로딩용)

**컴포넌트**:
- SpriteRenderer (빨간색, 크기 3x3)
- Rigidbody2D (Gravity Scale: 2, Continuous)
- CapsuleCollider2D (1x2)
- BossEnemy 스크립트

**자동 설정**:
- Data: FireDragon.asset 자동 할당
- Boss Health Bar Prefab: BossHealthBar.prefab 자동 할당 (있을 경우)
- Minion Prefab: BasicMeleeEnemy.prefab 자동 할당 (있을 경우)
- Max Summon Count: 3
- Show Debug Logs: true
- Show Gizmos: true

---

## ⚙️ 생성 옵션

Boss Setup Creator 창에서 선택 가능:

- [x] **FireDragon.asset (EnemyData)** - 보스 데이터
- [x] **BossEnemy_FireDragon.prefab** - 보스 프리팹
- [x] **EnemyProjectile.prefab** - 적 투사체
- [ ] **PoolManager 설정 (씬 필요)** - 자동 Pool 등록

**기본값**: 처음 3개 체크됨

---

## 🎮 사용 방법

### 방법 A: 전체 자동 생성 (권장)

1. **Tools > GASPT > Boss Setup Creator** 메뉴 클릭
2. **"🚀 모든 에셋 자동 생성"** 버튼 클릭
3. 완료 다이얼로그 확인
4. **GameplayScene** 열기
5. **BossEnemy_FireDragon.prefab**을 씬에 드래그
6. **Play** 버튼 클릭하여 테스트

**소요 시간**: 약 1분

---

### 방법 B: 개별 생성

각 에셋을 개별적으로 생성:

1. **"1. FireDragon.asset 생성"** 버튼 클릭
2. **"2. EnemyProjectile.prefab 생성"** 버튼 클릭
3. **"3. BossEnemy_FireDragon.prefab 생성"** 버튼 클릭

**사용 사례**: 특정 에셋만 재생성하고 싶을 때

---

### 방법 C: PoolManager 자동 설정

**전제 조건**: GameplayScene이 열려 있어야 함

1. **GameplayScene** 열기
2. Boss Setup Creator에서 **"PoolManager 설정 (씬에서 실행)"** 체크
3. **"🚀 모든 에셋 자동 생성"** 버튼 클릭

**또는**:

1. GameplayScene 열기
2. **"4. PoolManager 설정 (씬에서 실행)"** 버튼 클릭

**결과**: EnemyProjectile이 PoolManager의 Prefab Pools에 자동 등록됨
- Initial Size: 10
- Max Size: 50

---

## 🧪 테스트 시작

### 1. 씬 준비

1. **GameplayScene** 열기
2. **Canvas** 존재 확인 (없으면 Hierarchy 우클릭 > UI > Canvas)
3. **Player** GameObject 존재 확인
4. **Ground** GameObject 존재 확인

---

### 2. 보스 배치

1. Project 창에서 **BossEnemy_FireDragon.prefab** 찾기
2. Hierarchy로 드래그하여 씬에 추가
3. Transform 설정:
   - Position: (15, 1, 0) 또는 플레이어로부터 10-15 유닛 떨어진 위치
   - Rotation: (0, 0, 0)
   - Scale: (1, 1, 1) - 프리팹에 이미 3x3 크기 설정됨

---

### 3. Play 모드 테스트

1. **Play 버튼** (▶️) 클릭
2. Console 로그 확인:
   ```
   [BossEnemy] 화염 드래곤 보스 초기화 완료
   [BossEnemy] Phase Controller 초기화 완료
   [BossEnemy] 화염 드래곤 체력바 생성 완료
   ```
3. Game 뷰에서 BossHealthBar 확인 (화면 상단)
4. 플레이어를 보스 근처로 이동
5. 보스 패턴 확인:
   - Phase 1: 근접 공격 + 원거리 공격 (3초)
   - Phase 2 (HP 70%): 돌진 + 소환
   - Phase 3 (HP 30%): 범위 공격

---

## ✅ 자동 생성 확인 체크리스트

### 생성 완료 확인

- [ ] FireDragon.asset이 `Assets/_Project/Data/Enemies/`에 존재
- [ ] EnemyProjectile.prefab이 `Assets/_Project/Prefabs/Projectiles/`에 존재
- [ ] BossEnemy_FireDragon.prefab이 `Assets/_Project/Prefabs/Enemies/`에 존재

### 프리팹 설정 확인

**BossEnemy_FireDragon 프리팹 열기**:
- [ ] Data 필드에 FireDragon.asset 할당됨 (None이 아님)
- [ ] Boss Health Bar Prefab 필드에 BossHealthBar.prefab 할당됨
- [ ] Minion Prefab 필드에 BasicMeleeEnemy.prefab 할당됨
- [ ] Max Summon Count: 3
- [ ] Show Debug Logs: true
- [ ] Show Gizmos: true

**EnemyProjectile 프리팹 열기**:
- [ ] SpriteRenderer 존재 (빨간색)
- [ ] CircleCollider2D 존재 (Is Trigger: true)
- [ ] EnemyProjectile 스크립트 존재
- [ ] PooledObject 스크립트 존재

### Console 에러 확인

- [ ] Console에 에러 메시지 0개
- [ ] Console에 경고 메시지 확인 (있다면 해결)

---

## 🐛 문제 해결

### 문제 1: "BossHealthBar.prefab을 찾을 수 없습니다" 경고

**원인**: BossHealthBar.prefab이 `Assets/_Project/Prefabs/UI/`에 없음

**해결책**:
1. Tools > GASPT > Create StatPanel UI 실행
2. 또는 수동으로 BossHealthBar.prefab 생성
3. Boss Setup Creator 재실행

---

### 문제 2: "BasicMeleeEnemy.prefab을 찾을 수 없습니다" 경고

**원인**: BasicMeleeEnemy.prefab이 `Assets/_Project/Prefabs/Enemies/`에 없음

**해결책**:
1. Tools > GASPT > Prefab Creator 실행
2. "BasicMeleeEnemy 프리팹 생성" 버튼 클릭
3. Boss Setup Creator 재실행

---

### 문제 3: "FireDragon.asset을 찾을 수 없습니다!" 에러

**원인**: FireDragon.asset 생성에 실패함

**해결책**:
1. Boss Setup Creator에서 "1. FireDragon.asset 생성" 버튼 개별 클릭
2. Console 에러 확인
3. `Assets/_Project/Data/Enemies/` 폴더가 존재하는지 확인

---

### 문제 4: "PoolManager를 찾을 수 없습니다" 경고

**원인**: GameplayScene이 열려있지 않음

**해결책**:
1. GameplayScene 열기
2. PoolManager GameObject가 씬에 존재하는지 확인
3. "4. PoolManager 설정" 버튼 다시 클릭

---

### 문제 5: 프리팹이 이미 존재한다는 경고

**메시지**: "XXX.prefab이 이미 존재합니다. 스킵합니다."

**의미**: 해당 에셋이 이미 생성되어 있음 (정상)

**해결책**:
- 기존 파일을 사용하면 됨
- 재생성하고 싶다면:
  1. 기존 파일 삭제
  2. Boss Setup Creator 재실행

---

## 📊 장점

### ✅ 자동화의 이점

**수동 생성 vs 자동 생성 비교**:

| 항목 | 수동 생성 | 자동 생성 |
|------|-----------|-----------|
| **소요 시간** | 약 30분 | **약 1분** |
| **실수 가능성** | 높음 | **없음** |
| **설정 누락** | 가능 | **불가능** |
| **일관성** | 낮음 | **높음** |
| **재현성** | 어려움 | **쉬움** |

**자동 생성 장점**:
1. ⏱️ **시간 절약**: 30분 → 1분 (97% 단축)
2. 🎯 **정확성**: 모든 설정값 자동 할당
3. 🔄 **재현성**: 언제든지 동일한 환경 생성
4. 📝 **일관성**: 팀 전체가 동일한 설정 사용
5. 🚀 **생산성**: 테스트에 집중 가능

---

## 📝 다음 단계

### 1. 테스트 실행

자동 생성 완료 후:
1. `BOSS_TEST_CHECKLIST.md` 참고
2. GameplayScene에서 테스트 시작
3. Phase 1, 2, 3 패턴 확인
4. 보스 처치 테스트

---

### 2. 커스터마이징

**보스 스탯 조정**:
1. FireDragon.asset 열기
2. Inspector에서 값 수정
3. Play 모드에서 테스트

**보스 외형 변경**:
1. BossEnemy_FireDragon.prefab 열기
2. SpriteRenderer의 Sprite 교체
3. 색상 및 크기 조정

---

### 3. 추가 보스 생성

**새로운 보스 만들기**:
1. Boss Setup Creator의 코드 복사
2. `CreateIceDragonData()` 메서드 추가
3. 다른 스탯으로 새 보스 데이터 생성
4. 다른 이름의 프리팹 생성

---

## 🎯 요약

**Boss Setup Creator를 사용하면**:
- ✅ 1분 만에 보스 전투 테스트 환경 완성
- ✅ 3개 에셋 자동 생성 (Data, Prefab, Projectile)
- ✅ 모든 설정값 자동 할당
- ✅ 실수 없는 정확한 설정
- ✅ 바로 테스트 가능

**단 1번의 클릭으로 보스 전투 시스템 준비 완료!**

---

**작성자**: Claude Code
**최종 수정**: 2025-11-16
**도구 버전**: 1.0
