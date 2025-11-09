# Combat Test Scene 설계 문서

**작성일**: 2025-11-09
**목적**: 통합 전투 시스템 테스트 환경 구축
**씬 이름**: `CombatTestScene.unity`

---

## 📋 목차
1. [개요](#개요)
2. [테스트 목표](#테스트-목표)
3. [씬 구조](#씬-구조)
4. [구현 요소](#구현-요소)
5. [테스트 시나리오](#테스트-시나리오)
6. [사용 방법](#사용-방법)

---

## 개요

### 목적
모든 전투 관련 시스템을 통합하여 실제 게임플레이와 유사한 환경에서 테스트할 수 있는 씬을 제공합니다.

### 테스트 대상 시스템
- **PlayerStats**: 체력, 마나, 공격력, 방어력
- **Enemy**: 적 AI, 전투, 상태이상
- **SkillSystem**: 스킬 사용, 쿨다운, 마나 소비
- **StatusEffectManager**: 버프, 디버프, DoT
- **PlayerLevel**: 레벨업, 경험치 획득
- **DamageNumberPool**: 데미지 텍스트 표시
- **UI 시스템**: HealthBar, ExpBar, ManaBar, SkillUI

### 기존 테스트와의 차이점
| 기존 테스트 | Combat Test Scene |
|------------|-------------------|
| 개별 시스템 테스트 | 통합 시스템 테스트 |
| Context Menu 기반 | 실시간 플레이 가능 |
| UI 미연동 | 모든 UI 통합 |
| 제한된 시나리오 | 자유로운 전투 테스트 |

---

## 테스트 목표

### 1차 목표 (필수)
- ✅ 플레이어 전투 시스템 동작 확인
- ✅ 스킬 사용 및 쿨다운 확인
- ✅ 적 전투 및 사망 처리 확인
- ✅ UI 업데이트 정상 동작 확인
- ✅ 레벨업 및 경험치 시스템 확인

### 2차 목표 (선택)
- 상태이상 효과 시각적 확인
- 여러 적 동시 전투 테스트
- 스킬 조합 테스트
- 성능 프로파일링

---

## 씬 구조

### Hierarchy 구조
```
CombatTestScene
├── [Managers]
│   ├── CombatTestManager (전체 테스트 제어)
│   └── SingletonPreloader (싱글톤 사전 로딩)
│
├── [Player]
│   ├── Player (GameObject)
│   │   ├── PlayerStats (MonoBehaviour)
│   │   └── Sprite/Model (Visual)
│   │
│   └── PlayerUI (Canvas)
│       ├── PlayerHealthBar
│       ├── PlayerExpBar
│       ├── PlayerManaBar
│       └── SkillUIPanel
│
├── [Enemies]
│   ├── TestEnemy_Weak (일반 적)
│   ├── TestEnemy_Normal (중급 적)
│   └── TestEnemy_Strong (강력한 적)
│
├── [Environment]
│   ├── Ground (바닥)
│   ├── Walls (경계)
│   └── SpawnPoints (적 생성 위치)
│
└── [UI - Screen Space]
    ├── TestControlPanel (테스트 제어 UI)
    └── DebugInfoPanel (디버그 정보 표시)
```

---

## 구현 요소

### 1. CombatTestManager 스크립트

**파일**: `Assets/_Project/Scripts/Testing/CombatTestManager.cs`

#### 주요 기능
```csharp
// 테스트 제어
public void ResetTest()              // 테스트 초기화
public void SpawnEnemy(EnemyType)    // 적 생성
public void ClearAllEnemies()        // 모든 적 제거
public void HealPlayer(int amount)   // 플레이어 회복

// 치트 기능
public void SetPlayerLevel(int level)     // 레벨 설정
public void GiveAllSkills()               // 모든 스킬 부여
public void ToggleGodMode()               // 무적 모드
public void AddGold(int amount)           // 골드 추가

// 디버그 정보
public void LogPlayerStats()         // 플레이어 스탯 출력
public void LogActiveEnemies()       // 활성 적 목록 출력
public void LogSkillStatus()         // 스킬 상태 출력
```

#### 필드
```csharp
[Header("플레이어 설정")]
[SerializeField] private GameObject playerObject;
[SerializeField] private PlayerStats playerStats;

[Header("적 설정")]
[SerializeField] private List<EnemyData> enemyDataList;
[SerializeField] private Transform[] spawnPoints;

[Header("스킬 설정")]
[SerializeField] private List<SkillData> testSkills;

[Header("UI 참조")]
[SerializeField] private GameObject testControlPanel;
```

---

### 2. TestControlPanel UI

#### 구성 요소
```
TestControlPanel (Canvas)
├── Title: "Combat Test Controls"
│
├── [Player Controls]
│   ├── Button: Reset Player (체력/마나 회복)
│   ├── Button: Level Up (+1)
│   ├── Button: Level Down (-1)
│   └── Toggle: God Mode
│
├── [Enemy Controls]
│   ├── Button: Spawn Weak Enemy
│   ├── Button: Spawn Normal Enemy
│   ├── Button: Spawn Strong Enemy
│   └── Button: Clear All Enemies
│
├── [Skill Controls]
│   ├── Button: Give All Skills
│   ├── Button: Reset All Cooldowns
│   └── Text: Cooldown Status
│
└── [Debug Info]
    ├── Text: Player HP/Mana
    ├── Text: Player Level/EXP
    ├── Text: Active Enemies Count
    └── Text: FPS
```

---

### 3. Player 설정

#### GameObject 구조
```
Player
├── PlayerStats (MonoBehaviour)
│   ├── baseHP: 100
│   ├── baseAttack: 15
│   ├── baseDefense: 5
│   └── baseMana: 100
│
├── SpriteRenderer (임시 비주얼)
│   └── Color: Blue
│
└── Collider2D (전투 감지용)
```

#### 초기 설정
- Tag: "Player"
- Layer: "Player"
- Position: (0, 0, 0)

---

### 4. Enemy 설정

#### 3종류 적 프리팹

##### TestEnemy_Weak (약한 적)
```
HP: 30
Attack: 5
Defense: 0
EXP Reward: 10
Gold: 5
Color: Green
```

##### TestEnemy_Normal (일반 적)
```
HP: 50
Attack: 10
Defense: 2
EXP Reward: 25
Gold: 15
Color: Yellow
```

##### TestEnemy_Strong (강한 적)
```
HP: 100
Attack: 20
Defense: 5
EXP Reward: 50
Gold: 30
Color: Red
```

#### Enemy GameObject 구조
```
Enemy
├── Enemy (MonoBehaviour)
│   └── enemyData: EnemyData Asset
│
├── SpriteRenderer
│   └── Color: (종류별 색상)
│
├── EnemyNameTag (World Space UI)
│
└── Collider2D
```

---

### 5. UI 설정

#### PlayerUI Canvas
```
PlayerUI (Screen Space - Overlay)
├── PlayerHealthBar
│   └── Position: Top-Left (10, -10)
│
├── PlayerExpBar
│   └── Position: Below HealthBar (10, -60)
│
├── PlayerManaBar
│   └── Position: Below ExpBar (10, -110)
│
└── SkillUIPanel
    └── Position: Bottom-Center (0, 50)
```

#### TestControlPanel
```
Position: Right-Side (Screen Width - 250, 0)
Size: 240 x 600
Alpha: 0.9 (반투명)
```

---

### 6. 스킬 설정

#### 테스트용 스킬 4개

**Slot 0: Fireball (화염구)**
- Type: Damage
- Mana Cost: 20
- Cooldown: 3초
- Damage: 30
- Target: Enemy
- 키: 1

**Slot 1: Ice Blast (얼음 폭발)**
- Type: Damage
- Mana Cost: 30
- Cooldown: 5초
- Damage: 50
- StatusEffect: Slow (2초)
- Target: Enemy
- 키: 2

**Slot 2: Heal (회복)**
- Type: Heal
- Mana Cost: 25
- Cooldown: 8초
- Heal Amount: 40
- Target: Self
- 키: 3

**Slot 3: Power Buff (공격력 증가)**
- Type: Buff
- Mana Cost: 35
- Cooldown: 15초
- Effect: Attack +10 (10초)
- Target: Self
- 키: 4

---

## 테스트 시나리오

### 시나리오 1: 기본 전투
1. 씬 시작
2. "Spawn Weak Enemy" 버튼 클릭
3. 스킬 1번(Fireball) 사용
4. 적 처치 확인
5. EXP/골드 획득 확인
6. DamageNumber 표시 확인

**예상 결과**:
- ✅ Fireball 데미지 30 표시
- ✅ 적 HP 30 → 0
- ✅ EXP +10 표시
- ✅ Gold +5

### 시나리오 2: 레벨업 테스트
1. "Spawn Normal Enemy" 여러 번 클릭
2. 적들을 모두 처치
3. EXP 바 증가 확인
4. 레벨업 시 애니메이션 확인
5. 스탯 증가 확인 (HP +10, Attack +2, Defense +1)

**예상 결과**:
- ✅ EXP 바 100% 도달
- ✅ 레벨업 텍스트 애니메이션
- ✅ HP 완전 회복

### 시나리오 3: 스킬 조합
1. "Spawn Strong Enemy" 클릭
2. 스킬 4번(Power Buff) 사용
3. Attack 증가 확인 (15 → 25)
4. 스킬 1번(Fireball) 사용
5. 증가된 데미지 확인 (~40)
6. 버프 지속시간 확인 (10초)

**예상 결과**:
- ✅ 버프 적용 시 Attack 증가
- ✅ 데미지 증가 확인
- ✅ 10초 후 버프 해제

### 시나리오 4: 상태이상 테스트
1. "Spawn Normal Enemy" 클릭
2. 스킬 2번(Ice Blast) 사용
3. Slow 효과 적용 확인
4. 2초 후 효과 해제 확인

**예상 결과**:
- ✅ Slow 아이콘 표시 (향후 BuffIconUI 구현 시)
- ✅ 2초 후 자동 해제

### 시나리오 5: 마나 관리
1. 스킬 여러 번 연속 사용
2. 마나 부족 상태 확인
3. 마나바 색상 변화 확인 (20% 이하 주황색)
4. "Reset Player" 버튼으로 마나 회복

**예상 결과**:
- ✅ 마나 부족 시 스킬 사용 불가
- ✅ 마나바 경고 색상 표시
- ✅ 마나 회복 플래시 애니메이션

### 시나리오 6: God Mode
1. "Spawn Strong Enemy" 클릭
2. God Mode 토글 활성화
3. 적의 공격을 받아도 HP 감소 없음 확인

**예상 결과**:
- ✅ 데미지를 받지 않음
- ✅ God Mode 활성화 표시

---

## 사용 방법

### 1. 씬 생성 (Unity Editor)

#### 방법 A: 수동 생성
1. `Assets/_Project/Scenes/` 폴더에서 우클릭
2. `Create > Scene` 선택
3. 이름: `CombatTestScene`

#### 방법 B: 스크립트로 생성 (권장)
1. `CombatTestSceneSetup.cs` 에디터 스크립트 실행
2. Menu: `Tools > GASPT > Combat Test > Create Scene`
3. 자동으로 모든 요소 배치

### 2. 씬 테스트

1. `CombatTestScene.unity` 열기
2. Play 모드 진입
3. 우측 TestControlPanel에서 버튼 클릭
4. 키보드 1,2,3,4로 스킬 사용

### 3. 디버그 정보 확인

#### Console 로그
```
[CombatTestManager] Player HP: 100/100, Mana: 80/100
[CombatTestManager] Active Enemies: 2
[SkillSystem] Skill used: Fireball (Slot 0)
[PlayerLevel] Level Up! Lv.1 → Lv.2
```

#### Scene 뷰
- Gizmos로 SpawnPoint 표시
- 적 체력바 World Space UI
- 데미지 숫자 표시

---

## 구현 단계

### Phase 1: 기본 구조 (우선)
- [x] 설계 문서 작성
- [ ] CombatTestManager.cs 작성
- [ ] CombatTestScene.unity 생성
- [ ] Player GameObject 설정
- [ ] Enemy 3종 프리팹 생성

### Phase 2: UI 통합
- [ ] PlayerUI Canvas 구성
- [ ] TestControlPanel UI 생성
- [ ] DebugInfoPanel UI 생성
- [ ] UI 이벤트 연결

### Phase 3: 스킬 설정
- [ ] 테스트 스킬 4개 ScriptableObject 생성
- [ ] SkillSystem 연동
- [ ] 키보드 입력 테스트

### Phase 4: 테스트 자동화
- [ ] CombatTestSceneSetup.cs 에디터 도구 작성
- [ ] 원클릭 씬 생성 기능
- [ ] 자동 테스트 시퀀스 구현

---

## 참고 파일

### 기존 테스트 스크립트
- `SkillSystemTest.cs` - 스킬 테스트 참고
- `CombatTest.cs` - 전투 로직 참고
- `LevelTest.cs` - 레벨 시스템 참고
- `StatusEffectTest.cs` - 상태이상 참고

### 필요한 스크립트
- `PlayerStats.cs` - 플레이어 스탯
- `Enemy.cs` - 적 MonoBehaviour
- `SkillSystem.cs` - 스킬 관리
- `PlayerLevel.cs` - 레벨 관리
- `StatusEffectManager.cs` - 상태이상

### UI 스크립트
- `PlayerHealthBar.cs`
- `PlayerExpBar.cs`
- `PlayerManaBar.cs`
- `SkillSlotUI.cs`
- `SkillUIPanel.cs`

---

## 예상 이슈 및 해결

### 이슈 1: 싱글톤 초기화 순서
**문제**: SingletonPreloader가 없으면 싱글톤 오류 발생

**해결**:
```csharp
// CombatTestScene에 SingletonPreloader GameObject 추가
// 또는 CombatTestManager.Awake()에서 초기화
```

### 이슈 2: UI Canvas 중복
**문제**: 여러 Canvas가 생성되어 UI 표시 문제

**해결**:
```csharp
// Canvas는 씬당 1개만 사용
// HealthBar, ExpBar, ManaBar는 동일 Canvas 하위
```

### 이슈 3: Enemy 스폰 위치
**문제**: 적이 플레이어와 겹쳐서 생성

**해결**:
```csharp
// SpawnPoints 배열 사용
// 플레이어 주변 랜덤 위치 생성
```

---

## 향후 확장

### 추가 기능 아이디어
- 웨이브 시스템 (적 연속 생성)
- 자동 전투 모드 (AI 플레이어)
- 성능 프로파일링 도구
- 리플레이 시스템
- 스크린샷 캡처 기능

### BuffIconUI 통합 (Phase 11 완료 후)
- 활성 버프/디버프 아이콘 표시
- 지속시간 시각화
- 스택 수 표시

### Item Drop 통합 (Phase 13 완료 후)
- 적 처치 시 아이템 드롭
- 아이템 픽업 테스트
- 인벤토리 연동 확인

---

**문서 작성**: 2025-11-09
**다음 작업**: CombatTestManager.cs 스크립트 작성
**관련 문서**: WORK_STATUS.md, SKILL_SYSTEM_TEST_GUIDE.md
