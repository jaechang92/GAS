# 메타 진행 시스템 구현 계획

**기능 번호**: 018
**작성일**: 2025-12-01
**예상 기간**: 4주

---

## 1. 기술 컨텍스트

### 1.1 기존 시스템 활용

| 시스템 | 파일 위치 | 역할 |
|--------|----------|------|
| **SaveManager** | `Assets/_Project/Scripts/Core/SaveManager.cs` | 저장/로드 기반 |
| **CurrencySystem** | `Assets/_Project/Scripts/Gameplay/Economy/` | 재화 관리 참고 |
| **PlayerStats** | `Assets/_Project/Scripts/Stats/PlayerStats.cs` | 업그레이드 효과 적용 |
| **FormManager** | `Assets/_Project/Scripts/Forms/FormManager.cs` | 폼 해금 연동 |
| **LootSystem** | `Assets/_Project/Scripts/Gameplay/Loot/` | 드롭 풀 관리 |

### 1.2 신규 생성 필요

| 파일명 | 경로 | 설명 |
|--------|------|------|
| `MetaProgressionManager.cs` | `Assets/_Project/Scripts/Meta/` | 메타 진행 핵심 관리 |
| `MetaCurrency.cs` | `Assets/_Project/Scripts/Meta/` | Bone/Soul 재화 관리 |
| `PermanentUpgrade.cs` | `Assets/_Project/Scripts/Meta/` | 업그레이드 ScriptableObject |
| `UpgradeManager.cs` | `Assets/_Project/Scripts/Meta/` | 업그레이드 구매/적용 |
| `UnlockManager.cs` | `Assets/_Project/Scripts/Meta/` | 해금 시스템 |
| `AchievementManager.cs` | `Assets/_Project/Scripts/Meta/` | 업적 시스템 |
| `PlayerMetaProgress.cs` | `Assets/_Project/Scripts/Meta/` | 저장 데이터 클래스 |

### 1.3 의존성 구조

```
018-meta-progression
├── SaveManager (저장/로드)
├── PlayerStats (업그레이드 적용)
├── FormManager (폼 해금 연동)
├── LootSystem (드롭 풀 수정)
├── 017-form-swap-system (폼 해금 대상)
└── UISystem (로비 UI)
```

---

## 2. 아키텍처 설계

### 2.1 클래스 다이어그램

```
┌──────────────────────────────────────────┐
│         MetaProgressionManager           │
│            (Singleton)                   │
└────────────────┬─────────────────────────┘
                 │
    ┌────────────┼────────────┬────────────┐
    ▼            ▼            ▼            ▼
┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐
│ Meta    │ │ Upgrade │ │ Unlock  │ │Achievement│
│ Currency│ │ Manager │ │ Manager │ │ Manager │
└─────────┘ └─────────┘ └─────────┘ └─────────┘
    │            │            │            │
    └────────────┴────────────┴────────────┘
                       │
                       ▼
              ┌─────────────────┐
              │ PlayerMetaProgress │
              │   (Save Data)      │
              └─────────────────┘
```

### 2.2 데이터 흐름

```
[던전 플레이] → Bone 획득 (tempBone)
     │
     ▼
[런 종료] → tempBone → bone 확정 → 자동 저장
     │
     ▼
[로비] → 업그레이드 구매 → bone 차감 → 즉시 저장
     │
     ▼
[새 런 시작] → PlayerStats에 업그레이드 효과 적용
```

---

## 3. 핵심 컴포넌트 설계

### 3.1 MetaCurrency

```csharp
public class MetaCurrency
{
    public int Bone { get; private set; }
    public int Soul { get; private set; }
    public int TempBone { get; private set; }  // 런 중 임시 저장

    public event Action<int, int> OnBoneChanged;  // old, new
    public event Action<int, int> OnSoulChanged;

    public void AddTempBone(int amount);
    public void ConfirmTempBone();  // 런 종료 시 호출
    public bool TrySpendBone(int amount);
    public bool TrySpendSoul(int amount);
}
```

### 3.2 PermanentUpgrade ScriptableObject

```csharp
[CreateAssetMenu(fileName = "Upgrade", menuName = "GASPT/Meta/PermanentUpgrade")]
public class PermanentUpgrade : ScriptableObject
{
    public string upgradeId;
    public string upgradeName;
    [TextArea] public string description;

    public int maxLevel;
    public int[] costPerLevel;
    public float[] effectPerLevel;

    public CurrencyType currencyType;  // Bone or Soul
    public UpgradeType upgradeType;    // MaxHP, Attack, Defense, etc.

    public Sprite icon;
    public string[] prerequisiteIds;   // 선행 조건
}
```

### 3.3 PlayerMetaProgress (저장 데이터)

```csharp
[System.Serializable]
public class PlayerMetaProgress
{
    public int bone;
    public int soul;
    public Dictionary<string, int> upgradeLevels;
    public List<string> unlockedForms;
    public List<string> unlockedItems;
    public List<string> completedAchievements;
    public Dictionary<string, int> achievementProgress;

    // 통계
    public float totalPlayTime;
    public int totalRuns;
    public int highestStage;
    public int totalEnemiesKilled;
}
```

### 3.4 MetaProgressionManager

```csharp
public class MetaProgressionManager : MonoBehaviour, ISaveable
{
    public static MetaProgressionManager Instance { get; private set; }

    public MetaCurrency Currency { get; private set; }
    public UpgradeManager Upgrades { get; private set; }
    public UnlockManager Unlocks { get; private set; }
    public AchievementManager Achievements { get; private set; }

    private PlayerMetaProgress progress;

    // ISaveable 구현
    public void Save(SaveData data);
    public void Load(SaveData data);

    // 런 시작/종료
    public void OnRunStart();
    public void OnRunEnd(bool cleared);

    // 업그레이드 효과 적용
    public void ApplyUpgradesToPlayer(PlayerStats stats);
}
```

---

## 4. 업그레이드 데이터 정의

### 4.1 기본 업그레이드 목록

| ID | 이름 | 타입 | 레벨 | 효과 | 비용 (Bone) |
|----|------|------|------|------|------------|
| UP001 | 체력 강화 | MaxHP | 5 | +5/10/15/20/25 HP | 100/250/500/1000/2000 |
| UP002 | 공격 강화 | Attack | 5 | +5/10/15/20/25% | 100/250/500/1000/2000 |
| UP003 | 방어 강화 | Defense | 5 | -3/6/9/12/15% 피해 | 150/350/700/1400/2800 |
| UP004 | 이동 강화 | MoveSpeed | 5 | +3/6/9/12/15% | 100/200/400/800/1600 |
| UP005 | 골드 수집 | GoldBonus | 5 | +10/20/30/40/50% | 200/400/800/1600/3200 |
| UP006 | 경험치 | ExpBonus | 3 | +10/20/30% | 300/700/1500 |
| UP007 | 시작 골드 | StartGold | 4 | 50/100/150/200 | 150/350/700/1400 |

### 4.2 특수 업그레이드 (Soul)

| ID | 이름 | 효과 | 비용 (Soul) |
|----|------|------|------------|
| UP010 | 추가 대시 | 대시 +1회 | 200 |
| UP011 | 부활 | 런당 1회 부활 | 500 |

---

## 5. UI 설계

### 5.1 런 결과 화면

```
┌─────────────────────────────────────────┐
│           💀 런 종료! 💀               │
├─────────────────────────────────────────┤
│  도달 스테이지: 3-2                     │
│  처치한 적: 127                         │
│  플레이 시간: 12:34                     │
├─────────────────────────────────────────┤
│  획득 재화:                             │
│  🦴 Bone: +342                          │
│  💎 Soul: +15 (보스 처치)               │
├─────────────────────────────────────────┤
│          [로비로 돌아가기]              │
└─────────────────────────────────────────┘
```

### 5.2 업그레이드 트리 UI

```
┌─────────────────────────────────────────┐
│  🦴 보유 Bone: 1,234                   │
├─────────────────────────────────────────┤
│                                         │
│  [체력 +5]    [공격 +5%]   [방어 -3%]  │
│   Lv 3/5       Lv 2/5       Lv 1/5     │
│   (500 🦴)     (500 🦴)     (350 🦴)   │
│      │            │            │        │
│      ▼            ▼            ▼        │
│  [체력 +10]   [공격 +10%]  [방어 -6%]  │
│   잠김          Lv 2/5       잠김       │
│                                         │
├─────────────────────────────────────────┤
│          [구매]     [닫기]              │
└─────────────────────────────────────────┘
```

### 5.3 해금 UI

```
┌─────────────────────────────────────────┐
│  💎 보유 Soul: 156                     │
├─────────────────────────────────────────┤
│  🔥 화염 마법사     ❄️ 얼음 마법사     │
│    [해금됨]          [50 Soul]         │
│                                         │
│  ⚡ 번개 마법사     🌑 암흑 마법사     │
│    [50 Soul]         [잠김]            │
│                      (스테이지 4 클리어)│
├─────────────────────────────────────────┤
│          [해금]     [닫기]              │
└─────────────────────────────────────────┘
```

---

## 6. 구현 Phase 계획

### Phase 1: 기반 시스템 (Week 1)

| 태스크 | 설명 | 예상 시간 |
|--------|------|----------|
| MetaCurrency.cs | Bone/Soul 관리 | 2시간 |
| PlayerMetaProgress.cs | 저장 데이터 클래스 | 2시간 |
| PermanentUpgrade.cs | SO 정의 | 2시간 |
| MetaProgressionManager.cs | 기본 구조 | 4시간 |
| SaveManager 연동 | ISaveable 구현 | 3시간 |

### Phase 2: 업그레이드 시스템 (Week 2)

| 태스크 | 설명 | 예상 시간 |
|--------|------|----------|
| UpgradeManager.cs | 업그레이드 로직 | 4시간 |
| 업그레이드 에셋 생성 | 9개 업그레이드 SO | 2시간 |
| PlayerStats 연동 | 효과 적용 | 3시간 |
| 업그레이드 UI | 트리 형태 UI | 6시간 |

### Phase 3: 해금/재화 시스템 (Week 3)

| 태스크 | 설명 | 예상 시간 |
|--------|------|----------|
| UnlockManager.cs | 해금 로직 | 4시간 |
| 폼 해금 연동 | FormManager 수정 | 2시간 |
| 드롭 풀 수정 | LootSystem 연동 | 3시간 |
| 재화 획득 연동 | 적/상자 드롭 | 4시간 |
| 런 결과 UI | 재화 확정 화면 | 4시간 |

### Phase 4: 업적/폴리싱 (Week 4)

| 태스크 | 설명 | 예상 시간 |
|--------|------|----------|
| AchievementManager.cs | 업적 시스템 | 5시간 |
| Achievement.cs | SO 정의 | 2시간 |
| 업적 에셋 생성 | 15개 업적 | 2시간 |
| 업적 UI | 목록/알림 | 4시간 |
| 통합 테스트 | 전체 흐름 | 4시간 |

---

## 7. 파일 생성 목록

### 7.1 스크립트 파일

```
Assets/_Project/Scripts/Meta/
├── Data/
│   ├── MetaCurrency.cs
│   ├── PlayerMetaProgress.cs
│   ├── PermanentUpgrade.cs
│   └── Achievement.cs
├── System/
│   ├── MetaProgressionManager.cs
│   ├── UpgradeManager.cs
│   ├── UnlockManager.cs
│   └── AchievementManager.cs
└── Enums/
    └── MetaEnums.cs

Assets/_Project/Scripts/UI/Meta/
├── UpgradeTreeView.cs
├── UpgradeNodeView.cs
├── UnlockPanelView.cs
├── AchievementListView.cs
├── RunResultView.cs
└── MetaHUDView.cs
```

### 7.2 ScriptableObject 에셋

```
Assets/Resources/Data/Meta/
├── Upgrades/
│   ├── UP001_MaxHP.asset
│   ├── UP002_Attack.asset
│   ├── UP003_Defense.asset
│   ├── UP004_MoveSpeed.asset
│   ├── UP005_GoldBonus.asset
│   ├── UP006_ExpBonus.asset
│   ├── UP007_StartGold.asset
│   ├── UP010_ExtraDash.asset
│   └── UP011_Revive.asset
└── Achievements/
    ├── ACH001_Kill100.asset
    ├── ACH002_Stage3.asset
    └── ...
```

---

## 8. 검증 계획

### 8.1 단위 테스트

```csharp
[Test]
public void MetaCurrency_ConfirmTempBone_AddsToTotal()
{
    // Given: tempBone = 100
    // When: ConfirmTempBone()
    // Then: bone += 100, tempBone = 0
}

[Test]
public void UpgradeManager_Purchase_DeductsCurrency()
{
    // Given: bone = 500, 업그레이드 비용 = 100
    // When: 구매
    // Then: bone = 400
}
```

### 8.2 통합 테스트 체크리스트

- [ ] 런 중 Bone 획득 (tempBone)
- [ ] 런 종료 시 재화 확정
- [ ] 업그레이드 구매 및 효과 적용
- [ ] 폼 해금 및 드롭 풀 반영
- [ ] 저장/로드 정상 작동
- [ ] 업적 조건 추적 및 완료

---

## 9. 리스크 및 대응

| 리스크 | 영향 | 대응 |
|--------|------|------|
| 저장 데이터 손실 | 플레이어 이탈 | 백업 저장, 클라우드 동기화 |
| 밸런스 붕괴 | 게임 재미 저하 | 점진적 테스트, 수치 조정 용이한 구조 |
| UI 복잡도 | UX 저하 | 단순한 트리 구조, 툴팁 활용 |

---

## 10. 완료 조건

1. [ ] Bone/Soul 재화 시스템 작동
2. [ ] 런 종료 시 재화 확정
3. [ ] 9개 영구 업그레이드 구매 가능
4. [ ] 폼 해금 시스템 작동
5. [ ] 업적 추적 및 보상
6. [ ] 저장/로드 안정적 작동
7. [ ] 로비 UI 완성

---

*작성: GASPT Planning Agent*
*최종 수정: 2025-12-01*
