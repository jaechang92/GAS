# Combat Test Scene 사용 가이드

**작성일**: 2025-11-09
**대상**: GASPT 프로젝트 개발자
**목적**: Combat Test Scene 생성 및 사용 방법 안내

---

## 📋 목차
1. [빠른 시작](#빠른-시작)
2. [씬 생성 방법](#씬-생성-방법)
3. [테스트 방법](#테스트-방법)
4. [키보드 단축키](#키보드-단축키)
5. [문제 해결](#문제-해결)

---

## 빠른 시작

### 1단계: 씬 생성 (원클릭)

Unity Editor에서 다음 메뉴를 실행하세요:

```
Tools > GASPT > Combat Test > 🚀 Create Combat Test Scene
```

### 2단계: 씬 열기

```
Assets/_Project/Scenes/CombatTestScene.unity
```

### 3단계: Play 모드 진입

Unity Editor에서 Play 버튼 클릭 (또는 Ctrl+P)

### 4단계: 테스트 시작!

- **우측 OnGUI 패널**: 마우스로 버튼 클릭
- **F2**: 약한 적 생성
- **F3**: 일반 적 생성
- **F4**: 강한 적 생성
- **1, 2, 3, 4**: 스킬 사용
- **F10**: OnGUI UI 토글 (숨김/표시)

---

## 씬 생성 방법

### 자동 생성 (권장) ⭐

**Menu**: `Tools > GASPT > Combat Test > 🚀 Create Combat Test Scene`

#### 생성되는 것들:

**씬**:
- `Assets/_Project/Scenes/CombatTestScene.unity`

**EnemyData 3종**:
- `Assets/_Project/Data/Enemies/TestEnemies/TEST_WeakEnemy.asset`
- `Assets/_Project/Data/Enemies/TestEnemies/TEST_NormalEnemy.asset`
- `Assets/_Project/Data/Enemies/TestEnemies/TEST_StrongEnemy.asset`

**SkillData 4종**:
- `Assets/_Project/Data/Skills/TestSkills/TEST_Fireball.asset`
- `Assets/_Project/Data/Skills/TestSkills/TEST_IceBlast.asset`
- `Assets/_Project/Data/Skills/TestSkills/TEST_Heal.asset`
- `Assets/_Project/Data/Skills/TestSkills/TEST_PowerBuff.asset`

**Enemy Prefab**:
- `Assets/_Project/Prefabs/Enemies/EnemyPrefab.prefab`

#### 생성되는 GameObject:

```
CombatTestScene (Hierarchy)
├── Player (PlayerStats 컴포넌트)
├── CombatTestManager (테스트 제어)
├── SpawnPoints (적 생성 위치 5개)
│   ├── SpawnPoint_0
│   ├── SpawnPoint_1
│   ├── SpawnPoint_2
│   ├── SpawnPoint_3
│   └── SpawnPoint_4
└── Main Camera
```

#### 소요 시간
약 5초 (자동 생성)

---

### 수동 생성 (선택)

자동 생성을 사용하지 않는 경우, 다음 단계를 따르세요:

1. 새 씬 생성: `File > New Scene`
2. Player GameObject 생성 및 PlayerStats 추가
3. EnemyData 3종 ScriptableObject 생성
4. SkillData 4종 ScriptableObject 생성
5. Enemy Prefab 생성
6. SpawnPoints 생성 (5개)
7. CombatTestManager GameObject 추가 및 필드 연결

**권장하지 않습니다** - 자동 생성을 사용하세요!

---

## 테스트 방법

### 기본 전투 테스트

#### 시나리오 1: 약한 적 처치
1. Play 모드 진입
2. **F2** 키 - 약한 적 생성 (초록색 구)
3. **1** 키 - Fireball 스킬 사용
4. 적 HP가 30 → 0으로 감소 확인
5. EXP +10 획득 확인
6. Gold +5 획득 확인

**예상 결과**:
```
Console 로그:
[CombatTestManager] 적 생성: TEST_WeakEnemy at (5, 0, 0)
[SkillSystem] Skill used: TEST_Fireball
[Enemy] HP: 30 → 0
[PlayerLevel] EXP: 0 → 10
```

---

#### 시나리오 2: 일반 적 처치
1. **F3** 키 - 일반 적 생성 (노란색 구)
2. **1** 키 - Fireball 스킬 사용 (데미지 30)
3. 적 HP: 50 → 20
4. 3초 대기 (쿨다운)
5. **1** 키 - Fireball 다시 사용
6. 적 HP: 20 → 0
7. EXP +25, Gold +15 획득 확인

**예상 결과**:
```
[CombatTestManager] 적 생성: TEST_NormalEnemy at (5, 2, 0)
[SkillSystem] Skill used: TEST_Fireball
[Enemy] HP: 50 → 20
... (3초 대기)
[SkillSystem] Skill used: TEST_Fireball
[Enemy] HP: 20 → 0
[PlayerLevel] EXP: 10 → 35
```

---

#### 시나리오 3: 강한 적 처치
1. **F4** 키 - 강한 적 생성 (빨간색 구)
2. **4** 키 - PowerBuff 스킬 사용 (Attack +10)
3. **1** 키 - Fireball 사용 (증가된 데미지 적용)
4. 적 HP: 100 → ~60
5. 쿨다운 대기 후 반복

**예상 결과**:
```
[CombatTestManager] 적 생성: TEST_StrongEnemy at (5, -2, 0)
[SkillSystem] Skill used: TEST_PowerBuff
[PlayerStats] Attack: 15 → 25 (버프 적용)
[SkillSystem] Skill used: TEST_Fireball
[Enemy] HP: 100 → 60 (데미지 증가)
```

---

### 스킬 테스트

#### Fireball (슬롯 0, 키: 1)
- **타입**: Damage
- **마나**: 20
- **쿨다운**: 3초
- **데미지**: 30

**테스트**:
1. **1** 키 - 스킬 사용
2. 데미지 30 표시 확인
3. 마나 100 → 80 확인
4. 쿨다운 3초 대기
5. **1** 키 - 다시 사용 가능 확인

---

#### Ice Blast (슬롯 1, 키: 2)
- **타입**: Damage
- **마나**: 30
- **쿨다운**: 5초
- **데미지**: 50

**테스트**:
1. **2** 키 - 스킬 사용
2. 데미지 50 표시 확인
3. 마나 80 → 50 확인
4. 쿨다운 5초 대기

---

#### Heal (슬롯 2, 키: 3)
- **타입**: Heal
- **마나**: 25
- **쿨다운**: 8초
- **회복**: 40

**테스트**:
1. 적의 공격으로 체력 감소 (임의로 설정)
2. **3** 키 - 힐 스킬 사용
3. HP +40 회복 확인
4. 마나 50 → 25 확인

---

#### Power Buff (슬롯 3, 키: 4)
- **타입**: Buff
- **마나**: 35
- **쿨다운**: 15초
- **효과**: Attack +10 (10초)

**테스트**:
1. **4** 키 - 버프 사용
2. Attack 15 → 25 확인 (Console 또는 StatPanel)
3. 10초 대기
4. Attack 25 → 15 복구 확인

---

### 레벨업 테스트

#### 시나리오: 레벨 1 → 2
1. **F3** 키 - 일반 적 4마리 생성
2. 스킬로 모두 처치 (EXP +100)
3. 레벨업 애니메이션 확인
4. HP 완전 회복 확인
5. 스탯 증가 확인:
   - HP +10 (100 → 110)
   - Attack +2 (15 → 17)
   - Defense +1 (5 → 6)

**예상 결과**:
```
[PlayerLevel] Level Up! Lv.1 → Lv.2
[PlayerStats] HP: 100/100 → 110/110
[PlayerStats] Attack: 15 → 17
[PlayerStats] Defense: 5 → 6
```

---

### 치트 기능 테스트

#### God Mode (무적)
1. **F9** 키 - God Mode 토글
2. Console 확인: `[CombatTestManager] God Mode: 활성화`
3. 적의 공격을 받아도 HP 감소 없음 (향후 구현)

#### 플레이어 초기화
1. **F1** 키 - 플레이어 체력/마나 풀 회복
2. HP → MaxHP, Mana → MaxMana

#### 모든 적 제거
1. **F5** 키 - 모든 적 제거
2. Hierarchy에서 적 GameObject 삭제 확인

#### 디버그 정보 출력
1. **F12** 키 - 플레이어/적/스킬 정보 Console 출력

---

## OnGUI 테스트 UI ⭐ 새로 추가!

Play 모드 진입 시 **우측 상단**에 OnGUI 패널이 자동으로 표시됩니다!

### UI 구성

#### 우측 패널 (메인 컨트롤)
- **Player Controls**: Reset Player, God Mode, Pause
- **Enemy Controls**: Spawn Weak/Normal/Strong, Clear All
- **Skill Controls**: Reset Cooldowns
- **Cheat Controls**: Add Gold, Level Up
- **Real-time Info**: HP, Mana, Level, Gold, FPS 실시간 표시
- **Debug Logs**: 상태 정보 Console 출력
- **Shortcuts**: 단축키 안내

#### 좌측 하단 패널 (스킬 쿨다운)
- **Skill Cooldowns**: 4개 스킬 쿨다운 실시간 표시
  - READY (초록색): 사용 가능
  - CD: XX% (빨간색): 쿨다운 중

### 새로운 기능

#### 일시정지 (Pause)
- **버튼**: OnGUI 패널의 "Pause" 버튼 클릭
- **효과**: Time.timeScale = 0 (게임 일시정지)
- **해제**: 다시 클릭하면 재개

#### FPS 표시
- **위치**: Real-time Info 섹션 하단
- **색상**:
  - 초록색: 60 FPS 이상
  - 노란색: 30~60 FPS
  - 빨간색: 30 FPS 미만

#### UI 토글
- **F10** 키로 OnGUI UI 숨김/표시 전환
- 스크린샷 촬영 시 유용

---

## 키보드 단축키

### 전투 제어
| 키 | 기능 | 설명 |
|----|------|------|
| **F1** | 플레이어 초기화 | HP/마나 풀 회복 |
| **F2** | 약한 적 생성 | HP 30, Attack 5 |
| **F3** | 일반 적 생성 | HP 50, Attack 10 |
| **F4** | 강한 적 생성 | HP 100, Attack 20 |
| **F5** | 모든 적 제거 | 즉시 제거 |

### 스킬 사용
| 키 | 스킬 | 타입 | 마나 | 쿨다운 |
|----|------|------|------|--------|
| **1** | Fireball | Damage | 20 | 3초 |
| **2** | Ice Blast | Damage | 30 | 5초 |
| **3** | Heal | Heal | 25 | 8초 |
| **4** | Power Buff | Buff | 35 | 15초 |

### 치트 & 디버그
| 키 | 기능 | 설명 |
|----|------|------|
| **F9** | God Mode | 무적 모드 토글 |
| **F10** | UI 토글 | OnGUI 숨김/표시 |
| **F12** | 디버그 정보 | Console에 정보 출력 |

---

## Context Menu (Inspector)

CombatTestManager GameObject를 선택한 상태에서 Inspector의 Context Menu 사용:

### 테스트 제어
- `Reset Test` - 전체 초기화
- `Reset Player` - 플레이어 초기화
- `Spawn Weak Enemy` - 약한 적 생성
- `Spawn Normal Enemy` - 일반 적 생성
- `Spawn Strong Enemy` - 강한 적 생성
- `Clear All Enemies` - 모든 적 제거

### 치트
- `Reset All Cooldowns` - 모든 스킬 쿨다운 초기화
- `Toggle God Mode` - 무적 모드

### 디버그
- `Log Player Stats` - 플레이어 스탯 출력
- `Log Active Enemies` - 활성 적 목록 출력
- `Log Skill Status` - 스킬 상태 출력

---

## 문제 해결

### 문제 1: 씬 생성 시 오류 발생

**증상**:
```
NullReferenceException: Object reference not set to an instance of an object
```

**원인**: 필요한 폴더가 없거나 스크립트 컴파일 오류

**해결**:
1. 프로젝트 전체 재컴파일: `Assets > Reimport All`
2. Unity Editor 재시작
3. 다시 씬 생성 시도

---

### 문제 2: 스킬이 사용되지 않음

**증상**: 1,2,3,4 키를 눌러도 스킬이 발동하지 않음

**원인**: SkillSystem 미초기화 또는 스킬 미등록

**해결**:
1. CombatTestManager GameObject 선택
2. Inspector에서 `testSkills` 리스트 확인
3. 4개 스킬이 모두 할당되어 있는지 확인
4. Play 모드 재진입

---

### 문제 3: 적이 생성되지 않음

**증상**: F2, F3, F4 키를 눌러도 적이 나타나지 않음

**원인**: EnemyPrefab 미할당 또는 EnemyData 오류

**해결**:
1. CombatTestManager GameObject 선택
2. Inspector 확인:
   - `enemyPrefab`: EnemyPrefab.prefab 할당 확인
   - `weakEnemyData`, `normalEnemyData`, `strongEnemyData`: 할당 확인
3. Console에서 오류 로그 확인

---

### 문제 4: UI가 표시되지 않음

**증상**: HealthBar, ExpBar, ManaBar, SkillUI가 보이지 않음

**원인**: UI 생성 도구 미실행

**해결**:
이 씬은 UI를 자동 생성하지 않습니다. 수동으로 추가하세요:

```
1. Tools > GASPT > Create Player HealthBar UI
2. Tools > GASPT > Create Player ExpBar UI
3. Tools > GASPT > Create Player ManaBar UI
4. Tools > GASPT > Create Skill UI Panel
```

또는 Bootstrap 씬에서 UI를 복사하세요.

---

### 문제 5: Player를 찾을 수 없음

**증상**:
```
[CombatTestManager] Player GameObject를 찾을 수 없습니다!
```

**원인**: Player GameObject에 "Player" 태그 미설정

**해결**:
1. Hierarchy에서 Player GameObject 선택
2. Inspector 상단 Tag 드롭다운에서 "Player" 선택
3. Play 모드 재진입

---

### 문제 6: 싱글톤 오류

**증상**:
```
NullReferenceException: SkillSystem.Instance is null
```

**원인**: SingletonPreloader 미작동

**해결**:
1. Hierarchy에 SingletonPreloader GameObject 추가 (없는 경우)
2. 또는 CombatTestManager.Awake()에서 싱글톤 수동 초기화

---

## 추가 팁

### Tip 1: 빠른 테스트 시퀀스
```
F2 → 1 → (3초 대기) → 1
```
→ 약한 적 생성 및 2회 공격으로 처치

### Tip 2: 레벨업 빠르게 하기
```
F3 → F3 → F3 → F3 (일반 적 4마리)
→ 각각 처치하여 EXP 100 획득 → 레벨업
```

### Tip 3: 버프 효과 확인
```
F4 (강한 적) → 4 (버프) → 1 (공격)
→ 데미지 증가 확인
```

### Tip 4: 마나 관리 연습
```
1 → 2 → 3 → 4 (연속 스킬 사용)
→ 마나 부족 상태 확인
→ F1 (초기화)
```

### Tip 5: 다수 적 동시 전투
```
F2 → F3 → F4 (3종 적 동시 소환)
→ 우선순위 공격 전략 테스트
```

---

## 다음 단계

### UI 통합 (선택)
Combat Test Scene에 UI를 추가하려면:

1. **Canvas 생성**
2. **HealthBar, ExpBar, ManaBar 추가**:
   - `Tools > GASPT > Create Player HealthBar UI`
   - `Tools > GASPT > Create Player ExpBar UI`
   - `Tools > GASPT > Create Player ManaBar UI`
3. **SkillUI 추가**:
   - `Tools > GASPT > Create Skill UI Panel`

### 추가 적 타입 생성
더 다양한 적을 테스트하려면:

1. `Assets/_Project/Data/Enemies/TestEnemies/` 폴더
2. 우클릭 → `Create > GASPT > Enemies > Enemy`
3. Inspector에서 스탯 설정
4. CombatTestManager에 할당

### 추가 스킬 생성
더 많은 스킬을 테스트하려면:

1. `Assets/_Project/Data/Skills/TestSkills/` 폴더
2. 우클릭 → `Create > GASPT > Skills > Skill`
3. Inspector에서 설정
4. CombatTestManager.testSkills 리스트에 추가 (최대 4개)

---

## 관련 문서

- **설계 문서**: `COMBAT_TEST_SCENE_DESIGN.md`
- **전체 작업 상황**: `WORK_STATUS.md`
- **스킬 시스템 테스트**: `SKILL_SYSTEM_TEST_GUIDE.md`

---

**작성**: 2025-11-09
**문서 버전**: 1.0
**다음 업데이트**: UI 통합 후
