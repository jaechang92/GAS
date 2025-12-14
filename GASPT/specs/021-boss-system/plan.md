# 보스 시스템 구현 계획

**기능 번호**: 021
**작성일**: 2025-12-14
**예상 규모**: 대형 (4 Phase)
**관련 스펙**: [spec.md](./spec.md)

---

## 기술 컨텍스트

### 기존 시스템 분석

| 시스템 | 파일 위치 | 재사용 여부 |
|--------|----------|:-----------:|
| Enemy 기본 클래스 | `Gameplay/Enemy/Base/Enemy.cs` | ✅ 확장 |
| EnemyData | `Data/EnemyData.cs` | ✅ 확장 |
| EnemyType enum | `Core/Enums/EnemyType.cs` | ✅ Boss 타입 존재 |
| StatusEffectSystem | `StatusEffects/` | ✅ 연동 |
| PoolManager | `Core/Pooling/` | ✅ 사용 |
| MVP UI 패턴 | `UI/MVP/` | ✅ 패턴 적용 |

### 기존 Enemy 시스템 특징

```
Enemy.cs (추상 클래스)
├── EnemyData 참조
├── IPoolable 구현
├── StatusEffect 통합
├── OnHpChanged, OnDeath 이벤트
└── TakeDamage(), Die() 메서드
```

**EnemyData.cs 보스 관련 필드 (이미 존재)**:
- `bossRangedCooldown`, `bossChargeCooldown`, `bossSummonCooldown`
- `bossAreaCooldown`, `bossAreaRadius`, `bossAreaDamage`
- `bossProjectileSpeed`, `bossProjectileDamage`
- `showBossHealthBar`

---

## 신규 생성 파일

### Phase 1: 기반 시스템

| 파일 | 경로 | 설명 |
|------|------|------|
| BossData.cs | `Scripts/Data/` | 보스 전용 ScriptableObject |
| BossPhaseData.cs | `Scripts/Data/` | 페이즈 설정 데이터 |
| BossGrade.cs | `Scripts/Core/Enums/` | 보스 등급 enum |
| BaseBoss.cs | `Scripts/Gameplay/Boss/` | 보스 기본 클래스 |
| BossPhaseController.cs | `Scripts/Gameplay/Boss/` | 페이즈 전환 관리 |

### Phase 2: 패턴 AI

| 파일 | 경로 | 설명 |
|------|------|------|
| BossPattern.cs | `Scripts/Gameplay/Boss/Patterns/` | 패턴 기본 클래스 |
| MeleePattern.cs | `Scripts/Gameplay/Boss/Patterns/` | 근접 패턴 |
| RangedPattern.cs | `Scripts/Gameplay/Boss/Patterns/` | 원거리 패턴 |
| AreaPattern.cs | `Scripts/Gameplay/Boss/Patterns/` | 범위 패턴 |
| SummonPattern.cs | `Scripts/Gameplay/Boss/Patterns/` | 소환 패턴 |
| TelegraphController.cs | `Scripts/Gameplay/Boss/` | 텔레그래프 시스템 |
| PatternSelector.cs | `Scripts/Gameplay/Boss/` | 패턴 선택 로직 |

### Phase 3: UI

| 파일 | 경로 | 설명 |
|------|------|------|
| IBossHealthBarView.cs | `Scripts/UI/MVP/Interfaces/` | View 인터페이스 |
| BossHealthBarView.cs | `Scripts/UI/MVP/Views/` | 체력바 View |
| BossHealthBarPresenter.cs | `Scripts/UI/MVP/Presenters/` | 체력바 Presenter |
| BossIntroController.cs | `Scripts/UI/Boss/` | 등장 연출 |

### Phase 4: 보스 구현

| 파일 | 경로 | 설명 |
|------|------|------|
| FlameGolemBoss.cs | `Scripts/Gameplay/Boss/Bosses/` | 불꽃 골렘 |
| FrostSpiritBoss.cs | `Scripts/Gameplay/Boss/Bosses/` | 얼음 정령 |
| ThunderDragonBoss.cs | `Scripts/Gameplay/Boss/Bosses/` | 번개 드래곤 |
| DarkLordBoss.cs | `Scripts/Gameplay/Boss/Bosses/` | 어둠의 군주 |
| BossRewardSystem.cs | `Scripts/Gameplay/Boss/` | 보상 처리 |

---

## Phase 1: 기반 시스템 설계

### 1.1 BossGrade Enum

```
파일: Scripts/Core/Enums/BossGrade.cs
```

```csharp
public enum BossGrade
{
    MiniBoss = 0,    // 미니보스 (1-2 페이즈)
    MidBoss = 1,     // 중간보스 (2-3 페이즈)
    FinalBoss = 2    // 최종보스 (3-4 페이즈)
}
```

### 1.2 BossPhaseData

```
파일: Scripts/Data/BossPhaseData.cs
```

**필드**:
| 필드 | 타입 | 설명 |
|------|------|------|
| phaseIndex | int | 페이즈 번호 (1, 2, 3...) |
| healthThreshold | float | 전환 체력 비율 (0.7, 0.4 등) |
| attackMultiplier | float | 공격력 배율 |
| speedMultiplier | float | 속도 배율 |
| availablePatterns | BossPattern[] | 사용 가능 패턴 |

### 1.3 BossData ScriptableObject

```
파일: Scripts/Data/BossData.cs
```

**Enemy 확장 대신 별도 생성 이유**:
- 페이즈 시스템 필요
- 패턴 목록 관리
- 보상 시스템 별도 처리

**필드**:
| 필드 | 타입 | 설명 |
|------|------|------|
| bossId | string | B001, B002 등 |
| bossName | string | UI 표시용 |
| bossGrade | BossGrade | 등급 |
| elementType | ElementType | 속성 |
| maxHealth | int | 최대 체력 |
| baseAttack | int | 기본 공격력 |
| phases | BossPhaseData[] | 페이즈 목록 |
| goldReward | int | 골드 보상 |
| expReward | int | 경험치 보상 |
| firstClearReward | FormData | 첫 클리어 보상 |

### 1.4 BaseBoss 클래스

```
파일: Scripts/Gameplay/Boss/BaseBoss.cs
```

**Enemy 상속 vs 별도 구현**: Enemy 상속 선택
- 기존 TakeDamage, Die, StatusEffect 재사용
- OnHpChanged 이벤트 활용
- PoolManager 호환

**추가 멤버**:
```csharp
// 페이즈
protected BossPhaseController phaseController;
protected int currentPhase = 1;
protected bool isInvulnerable = false;

// 패턴
protected PatternSelector patternSelector;
protected BossPattern currentPattern;

// 이벤트
public event Action<int, int> OnPhaseChanged;  // (현재, 총)
public event Action OnBossDefeated;
```

**주요 메서드**:
```csharp
protected override void OnHpChanged(int current, int max);
protected virtual void TransitionToPhase(int newPhase);
protected virtual async Awaitable ExecutePattern(BossPattern pattern);
```

### 1.5 BossPhaseController

```
파일: Scripts/Gameplay/Boss/BossPhaseController.cs
```

**역할**: 체력 기반 페이즈 전환 감지 및 연출

**흐름**:
```
1. OnHpChanged 구독
2. 체력 비율 확인
3. 전환 조건 충족 시:
   - 보스 무적 설정
   - 전환 애니메이션 트리거
   - 화면 효과 실행
   - 새 페이즈 적용
   - 무적 해제
```

---

## Phase 2: 패턴 AI 설계

### 2.1 BossPattern 기본 클래스

```
파일: Scripts/Gameplay/Boss/Patterns/BossPattern.cs
```

**필드**:
| 필드 | 타입 | 설명 |
|------|------|------|
| patternName | string | 패턴 이름 |
| patternType | PatternType | Melee/Ranged/Area/Summon/Buff/Special |
| damage | int | 데미지 |
| cooldown | float | 쿨다운 |
| telegraphDuration | float | 텔레그래프 시간 |
| weight | int | 선택 가중치 |
| minPhase | int | 최소 페이즈 요구 |

**추상 메서드**:
```csharp
public abstract void ShowTelegraph(Vector3 targetPosition);
public abstract async Awaitable Execute(BaseBoss boss, Transform target);
public abstract void Cancel();
```

### 2.2 패턴 구현 예시

**MeleePattern** (근접):
```csharp
public class MeleePattern : BossPattern
{
    public float chargeSpeed;
    public float chargeDistance;

    public override async Awaitable Execute(BaseBoss boss, Transform target)
    {
        // 1. 텔레그래프 (직선)
        ShowTelegraph(target.position);
        await Awaitable.WaitForSecondsAsync(telegraphDuration);

        // 2. 돌진
        Vector3 direction = (target.position - boss.transform.position).normalized;
        float traveled = 0f;
        while (traveled < chargeDistance)
        {
            boss.transform.position += direction * chargeSpeed * Time.deltaTime;
            traveled += chargeSpeed * Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }

        // 3. 데미지 적용
        ApplyDamageInRange(boss.transform.position, 2f, damage);
    }
}
```

**AreaPattern** (범위):
```csharp
public class AreaPattern : BossPattern
{
    public float radius;
    public float delay;

    public override async Awaitable Execute(BaseBoss boss, Transform target)
    {
        // 1. 텔레그래프 (원형)
        Vector3 targetPos = target.position;
        ShowCircleTelegraph(targetPos, radius);
        await Awaitable.WaitForSecondsAsync(telegraphDuration);

        // 2. 딜레이
        await Awaitable.WaitForSecondsAsync(delay);

        // 3. 데미지 적용
        ApplyDamageInRadius(targetPos, radius, damage);
    }
}
```

### 2.3 TelegraphController

```
파일: Scripts/Gameplay/Boss/TelegraphController.cs
```

**역할**: 공격 예고 시각화

**텔레그래프 타입**:
| 타입 | 프리팹 | 사용 |
|------|--------|------|
| Circle | CircleTelegraph | 범위 공격 |
| Line | LineTelegraph | 돌진, 빔 |
| Marker | TargetMarker | 추적 공격 |
| FullScreen | ScreenFlash | 필살기 |

**메서드**:
```csharp
public void ShowCircle(Vector3 center, float radius, float duration);
public void ShowLine(Vector3 start, Vector3 end, float width, float duration);
public void ShowMarker(Transform target, float duration);
public void ShowFullScreen(float duration);
public void HideAll();
```

### 2.4 PatternSelector

```
파일: Scripts/Gameplay/Boss/PatternSelector.cs
```

**역할**: 가중치 기반 패턴 선택

**로직**:
```csharp
public BossPattern SelectPattern(int currentPhase, BossPattern[] available)
{
    // 1. 현재 페이즈에서 사용 가능한 패턴 필터
    var valid = available.Where(p => p.minPhase <= currentPhase && !p.IsOnCooldown);

    // 2. 가중치 합계 계산
    int totalWeight = valid.Sum(p => p.weight);

    // 3. 랜덤 선택
    int roll = Random.Range(0, totalWeight);
    int cumulative = 0;

    foreach (var pattern in valid)
    {
        cumulative += pattern.weight;
        if (roll < cumulative)
            return pattern;
    }

    return valid.First();
}
```

---

## Phase 3: UI 설계

### 3.1 BossHealthBarView

```
파일: Scripts/UI/MVP/Views/BossHealthBarView.cs
```

**UI 계층**:
```
BossHealthBarPanel (Canvas)
├── BackgroundPanel
├── BossNameText
├── HealthBarContainer
│   ├── HealthBarBackground
│   ├── HealthBarFill (Image)
│   └── HealthPercentText
├── PhaseContainer
│   ├── PhaseText ("Phase 2/3")
│   └── PhaseIcons (◆◆◇)
└── InvulnerabilityOverlay
```

**Interface**:
```csharp
public interface IBossHealthBarView
{
    void Show(string bossName, int totalPhases);
    void Hide();
    void UpdateHealth(float currentRatio);
    void UpdatePhase(int currentPhase, int totalPhases);
    void SetInvulnerable(bool invulnerable);
    void SetHealthBarColor(Color color);
}
```

### 3.2 BossHealthBarPresenter

```
파일: Scripts/UI/MVP/Presenters/BossHealthBarPresenter.cs
```

**역할**:
- BaseBoss 이벤트 구독
- View 업데이트 조정
- 페이즈별 색상 변경

**이벤트 연결**:
```csharp
private void SubscribeToBoss(BaseBoss boss)
{
    boss.OnHpChanged += HandleHpChanged;
    boss.OnPhaseChanged += HandlePhaseChanged;
    boss.OnBossDefeated += HandleBossDefeated;
}
```

### 3.3 BossIntroController

```
파일: Scripts/UI/Boss/BossIntroController.cs
```

**등장 연출 시퀀스**:
```csharp
public async Awaitable PlayIntro(BossData bossData, Transform spawnPoint)
{
    // 1. 화면 어두워짐 (0.5초)
    await FadeToBlack(0.5f);

    // 2. 보스 스폰
    SpawnBoss(bossData, spawnPoint);

    // 3. 보스 이름 표시 (1초)
    await ShowBossName(bossData.bossName, 1f);

    // 4. 플레이어 잠금 해제
    EnablePlayerControl();

    // 5. 전투 시작
    StartBossFight();
}
```

---

## Phase 4: 보스 구현 설계

### 4.1 개별 보스 클래스

**FlameGolemBoss** (B001):
```
파일: Scripts/Gameplay/Boss/Bosses/FlameGolemBoss.cs
```
- 페이즈 1: 화염 내려찍기, 화염구 발사
- 페이즈 2: + 폭발 추가

**FrostSpiritBoss** (B002):
```
파일: Scripts/Gameplay/Boss/Bosses/FrostSpiritBoss.cs
```
- 페이즈 1: 얼음 창, 빙결 지대
- 페이즈 2: + 얼음 소환수

**ThunderDragonBoss** (B003):
```
파일: Scripts/Gameplay/Boss/Bosses/ThunderDragonBoss.cs
```
- 페이즈 1: 낙뢰, 연쇄 번개
- 페이즈 2: + 돌진
- 페이즈 3: + 분노, 번개 폭풍

**DarkLordBoss** (B004):
```
파일: Scripts/Gameplay/Boss/Bosses/DarkLordBoss.cs
```
- 페이즈 1: 암흑 베기, 암흑구, 생명력 흡수
- 페이즈 2: + 분신 소환
- 페이즈 3: + 암흑 폭풍
- 페이즈 4: + 멸망의 일격

### 4.2 BossRewardSystem

```
파일: Scripts/Gameplay/Boss/BossRewardSystem.cs
```

**보상 처리 흐름**:
```csharp
public void ProcessReward(BossData bossData, bool noHit, float clearTime)
{
    // 1. 기본 보상
    int gold = bossData.goldReward;
    int exp = bossData.expReward;

    // 2. 추가 보상 조건
    if (noHit)
        gold = Mathf.RoundToInt(gold * 1.5f);

    if (clearTime < bossData.timeLimit * 0.5f)
        exp = Mathf.RoundToInt(exp * 1.3f);

    // 3. 첫 클리어 보상
    if (IsFirstClear(bossData.bossId))
    {
        GrantFirstClearReward(bossData.firstClearReward);
        MarkAsCleared(bossData.bossId);
    }

    // 4. 보상 지급
    CurrencySystem.Instance.AddGold(gold);
    PlayerLevel.Instance.AddExp(exp);

    // 5. UI 표시
    ShowRewardPopup(gold, exp);
}
```

---

## 던전-보스 연동

### 보스 방 생성

**RoomGenerator 수정**:
```csharp
// 5번째 방은 항상 보스 방
if (roomIndex == 4)  // 0-indexed
{
    roomType = RoomType.Boss;
    spawnBoss = true;
}
```

### BossRoomController

```
파일: Scripts/Gameplay/Level/Room/BossRoomController.cs
```

**역할**:
- 방 입장 시 문 잠금
- 보스 등장 연출 실행
- 보스 처치 시 문 열림
- 보상 지급 및 다음 스테이지 전환

---

## 의존성 그래프

```
Phase 1 (기반)
├── BossGrade.cs
├── BossPhaseData.cs
├── BossData.cs
├── BaseBoss.cs
└── BossPhaseController.cs
    ↓
Phase 2 (패턴)
├── BossPattern.cs (추상)
├── MeleePattern.cs
├── RangedPattern.cs
├── AreaPattern.cs
├── SummonPattern.cs
├── TelegraphController.cs
└── PatternSelector.cs
    ↓
Phase 3 (UI)
├── IBossHealthBarView.cs
├── BossHealthBarView.cs
├── BossHealthBarPresenter.cs
└── BossIntroController.cs
    ↓
Phase 4 (보스 & 보상)
├── FlameGolemBoss.cs
├── FrostSpiritBoss.cs
├── ThunderDragonBoss.cs
├── DarkLordBoss.cs
├── BossRewardSystem.cs
└── BossRoomController.cs
```

---

## 구현 순서

### Phase 1: 기반 시스템 (핵심)
1. `BossGrade.cs` - enum 정의
2. `BossPhaseData.cs` - 페이즈 데이터 구조
3. `BossData.cs` - ScriptableObject
4. `BaseBoss.cs` - Enemy 확장
5. `BossPhaseController.cs` - 페이즈 전환

### Phase 2: 패턴 AI
6. `BossPattern.cs` - 추상 클래스
7. `TelegraphController.cs` - 텔레그래프
8. `MeleePattern.cs`, `RangedPattern.cs` 등
9. `PatternSelector.cs` - 가중치 선택

### Phase 3: UI
10. `IBossHealthBarView.cs` - 인터페이스
11. `BossHealthBarView.cs` - View
12. `BossHealthBarPresenter.cs` - Presenter
13. `BossIntroController.cs` - 등장 연출

### Phase 4: 보스 & 연동
14. `FlameGolemBoss.cs` - 미니보스 1
15. `FrostSpiritBoss.cs` - 미니보스 2
16. `ThunderDragonBoss.cs` - 중간보스
17. `DarkLordBoss.cs` - 최종보스
18. `BossRewardSystem.cs` - 보상
19. `BossRoomController.cs` - 던전 연동

---

## 테스트 전략

### 단위 테스트
- BossPhaseController: 체력 기반 페이즈 전환
- PatternSelector: 가중치 랜덤 선택
- BossRewardSystem: 보상 계산

### 통합 테스트
- 보스 전투 전체 흐름
- UI 업데이트 동기화
- 던전-보스 방 연동

### 플레이 테스트
- 패턴 학습 가능 여부
- 페이즈 전환 긴장감
- 밸런스 (전투 시간)

---

## 다음 단계

```
/speckit.tasks  - 태스크 목록 생성
```

---

*작성: Claude Code*
*최종 수정: 2025-12-14*
