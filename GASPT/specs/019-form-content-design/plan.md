# 폼/캐릭터 컨텐츠 구현 계획

**기능 번호**: 019
**작성일**: 2025-11-30
**예상 기간**: 4주 (Phase별)

---

## 1. 기술 컨텍스트

### 1.1 기존 시스템 활용

| 시스템 | 파일 위치 | 역할 |
|--------|----------|------|
| **SkillData** | `Assets/_Project/Scripts/Skills/SkillData.cs` | 스킬 정의 기반 |
| **SkillSystem** | `Assets/_Project/Scripts/Skills/SkillSystem.cs` | 스킬 실행 |
| **PlayerStats** | `Assets/_Project/Scripts/Stats/PlayerStats.cs` | 스탯 적용 |
| **StatusEffectManager** | `Assets/_Project/Scripts/StatusEffects/` | 상태 효과 |
| **Item System** | `Assets/_Project/Scripts/Gameplay/Item/` | 아이템 드롭 참고 |

### 1.2 신규 생성 필요

| 파일명 | 경로 | 설명 |
|--------|------|------|
| `FormData.cs` | `Assets/_Project/Scripts/Forms/` | 폼 ScriptableObject |
| `FormManager.cs` | `Assets/_Project/Scripts/Forms/` | 폼 교체/관리 |
| `FormInstance.cs` | `Assets/_Project/Scripts/Forms/` | 런타임 폼 인스턴스 |
| `FormSynergy.cs` | `Assets/_Project/Scripts/Forms/` | 교체 시너지 |
| `FormAwakening.cs` | `Assets/_Project/Scripts/Forms/` | 각성 시스템 |

### 1.3 의존성 확인

```
019-form-content-design
├── 017-form-swap-system (기반 시스템)
│   ├── SkillSystem (스킬 실행)
│   ├── PlayerStats (스탯 적용)
│   └── StatusEffectSystem (버프/디버프)
└── LootSystem (드롭)
```

---

## 2. 구현 Phase 계획

### Phase 1: 기반 시스템 (Week 1)

#### 1.1 FormData ScriptableObject 정의

**파일**: `Assets/_Project/Scripts/Forms/FormData.cs`

```csharp
[CreateAssetMenu(fileName = "FormData", menuName = "GASPT/Forms/FormData")]
public class FormData : ScriptableObject
{
    [Header("기본 정보")]
    public string formId;
    public string formName;
    [TextArea(2, 4)]
    public string description;
    public FormType formType;
    public FormRarity baseRarity;

    [Header("스탯 (등급별)")]
    public FormStats commonStats;
    public FormStats rareStats;
    public FormStats uniqueStats;
    public FormStats legendaryStats;

    [Header("스킬")]
    public SkillData skill1;
    public SkillData skill2;

    [Header("비주얼")]
    public Sprite icon;
    public RuntimeAnimatorController animatorController;
    public GameObject swapEffect;
    public Color themeColor;
}
```

#### 1.2 Enum 정의

**파일**: `Assets/_Project/Scripts/Forms/FormEnums.cs`

```csharp
public enum FormType
{
    Balance,    // 밸런스형
    Power,      // 파워형 (높은 공격력)
    Control,    // 컨트롤형 (CC)
    Speed,      // 스피드형 (빠른 공격)
    Drain       // 드레인형 (흡혈)
}

public enum FormRarity
{
    Common,
    Rare,
    Unique,
    Legendary
}
```

#### 1.3 기본 마법사 데이터 생성

**에셋 경로**: `Assets/Resources/Data/Forms/BasicMage.asset`

| 설정 | 값 |
|------|-----|
| formId | FM001 |
| formName | 기본 마법사 |
| formType | Balance |
| baseRarity | Common |

#### 1.4 기본 스킬 데이터 생성

**마법 화살**: `Assets/Resources/Data/Skills/MagicArrow.asset`
**보호막**: `Assets/Resources/Data/Skills/MagicShield.asset`

---

### Phase 2: 원소 폼 구현 (Week 2)

#### 2.1 화염 마법사

| 작업 | 세부 내용 |
|------|----------|
| ScriptableObject | FlameMage.asset 생성 |
| 스킬 1 | Fireball.asset - 범위 폭발 |
| 스킬 2 | FireStorm.asset - 지속 범위 |
| 상태 효과 | Burn StatusEffectData 생성 |
| VFX | 화염 이펙트 프리팹 |

#### 2.2 얼음 마법사

| 작업 | 세부 내용 |
|------|----------|
| ScriptableObject | FrostMage.asset 생성 |
| 스킬 1 | IceLance.asset - 관통 투사체 |
| 스킬 2 | FrozenGround.asset - 지속 범위 |
| 상태 효과 | Slow, Freeze StatusEffectData |
| VFX | 얼음 이펙트 프리팹 |

#### 2.3 번개 마법사

| 작업 | 세부 내용 |
|------|----------|
| ScriptableObject | ThunderMage.asset 생성 |
| 스킬 1 | ChainLightning.asset - 연쇄 타격 |
| 스킬 2 | ThunderRush.asset - 대시 공격 |
| 상태 효과 | Stun StatusEffectData |
| VFX | 번개 이펙트 프리팹 |

---

### Phase 3: 고급 폼 + 시스템 (Week 3)

#### 3.1 암흑 마법사

| 작업 | 세부 내용 |
|------|----------|
| ScriptableObject | DarkMage.asset 생성 |
| 스킬 1 | LifeDrain.asset - 채널링 흡혈 |
| 스킬 2 | SoulExplosion.asset - HP 소모 폭발 |
| 특수 시스템 | HP 소모 메커닉 |
| VFX | 암흑 이펙트 프리팹 |

#### 3.2 폼 교체 시너지 시스템

**파일**: `Assets/_Project/Scripts/Forms/FormSynergy.cs`

```csharp
public class FormSynergy
{
    // 교체 시 이전 폼 타입에 따른 보너스
    public static void ApplySwapBonus(FormType previous, FormType current, PlayerStats stats)
    {
        var bonus = GetSynergyBonus(previous, current);
        ApplyBonus(bonus, stats);
    }
}
```

#### 3.3 각성 시스템

**파일**: `Assets/_Project/Scripts/Forms/FormAwakening.cs`

```csharp
public class FormAwakening
{
    public static bool TryAwaken(FormInstance form)
    {
        if (form.Rarity >= FormRarity.Legendary)
            return false;

        form.Rarity++;
        form.RecalculateStats();
        return true;
    }
}
```

---

### Phase 4: 밸런싱 + 폴리싱 (Week 4)

#### 4.1 밸런스 조정

| 항목 | 테스트 방법 | 목표 |
|------|------------|------|
| DPS | 허수아비 대상 10초 딜량 | 등급별 ±15% |
| 생존력 | 스테이지 1 클리어 피격 횟수 | 역할별 차별화 |
| 쿨다운 | 실전 체감 | 과도하지 않게 |

#### 4.2 VFX/SFX 연동

| 폼 | VFX | SFX |
|----|-----|-----|
| 기본 | 마법 파티클 | 기본 마법 소리 |
| 화염 | 불꽃 이펙트 | 불 타는 소리 |
| 얼음 | 얼음 파편 | 얼음 깨지는 소리 |
| 번개 | 전기 스파크 | 전기 소리 |
| 암흑 | 어둠 입자 | 으스스한 소리 |

#### 4.3 드롭 확률 조정

```csharp
public class FormDropTable
{
    public static FormData RollDrop(int stageNumber)
    {
        // 스테이지별 확률 테이블 참조
        // spec.md의 획득 확률 테이블 기반
    }
}
```

---

## 3. 파일 생성 목록

### 3.1 스크립트 파일

```
Assets/_Project/Scripts/Forms/
├── FormData.cs              # ScriptableObject 정의
├── FormEnums.cs             # Enum 정의 (FormType, FormRarity)
├── FormStats.cs             # 스탯 구조체
├── FormInstance.cs          # 런타임 인스턴스
├── FormManager.cs           # 폼 관리 (017에서 확장)
├── FormSynergy.cs           # 교체 시너지
├── FormAwakening.cs         # 각성 시스템
└── FormDropTable.cs         # 드롭 확률
```

### 3.2 ScriptableObject 에셋

```
Assets/Resources/Data/Forms/
├── BasicMage.asset
├── FlameMage.asset
├── FrostMage.asset
├── ThunderMage.asset
└── DarkMage.asset
```

### 3.3 스킬 에셋

```
Assets/Resources/Data/Skills/Form/
├── Basic/
│   ├── MagicArrow.asset
│   └── MagicShield.asset
├── Flame/
│   ├── Fireball.asset
│   └── FireStorm.asset
├── Frost/
│   ├── IceLance.asset
│   └── FrozenGround.asset
├── Thunder/
│   ├── ChainLightning.asset
│   └── ThunderRush.asset
└── Dark/
    ├── LifeDrain.asset
    └── SoulExplosion.asset
```

### 3.4 상태 효과 에셋

```
Assets/Resources/Data/StatusEffects/
├── Burn.asset          # 화상 (DoT)
├── Slow.asset          # 감속
├── Freeze.asset        # 빙결 (행동 불가)
├── Stun.asset          # 기절
└── Vampirism.asset     # 흡혈 버프
```

---

## 4. 구현 순서 (태스크 목록)

### Week 1 태스크

| # | 태스크 | 예상 시간 | 의존성 |
|---|--------|----------|--------|
| 1 | FormEnums.cs 생성 | 30분 | 없음 |
| 2 | FormStats.cs 구조체 정의 | 30분 | 없음 |
| 3 | FormData.cs ScriptableObject 정의 | 1시간 | 1, 2 |
| 4 | FormInstance.cs 런타임 클래스 | 1시간 | 3 |
| 5 | BasicMage.asset 생성 | 30분 | 3 |
| 6 | MagicArrow 스킬 구현 | 2시간 | 3 |
| 7 | MagicShield 스킬 구현 | 2시간 | 3 |
| 8 | FormManager 기본 통합 | 2시간 | 4, 5 |

### Week 2 태스크

| # | 태스크 | 예상 시간 | 의존성 |
|---|--------|----------|--------|
| 9 | FlameMage.asset 생성 | 30분 | Week 1 완료 |
| 10 | Fireball 스킬 구현 | 3시간 | 9 |
| 11 | FireStorm 스킬 구현 | 3시간 | 9 |
| 12 | Burn 상태 효과 | 1시간 | 10 |
| 13 | FrostMage.asset 생성 | 30분 | 12 |
| 14 | IceLance 스킬 구현 | 3시간 | 13 |
| 15 | FrozenGround 스킬 구현 | 3시간 | 13 |
| 16 | Slow/Freeze 상태 효과 | 2시간 | 14 |
| 17 | ThunderMage.asset 생성 | 30분 | 16 |
| 18 | ChainLightning 스킬 구현 | 4시간 | 17 |
| 19 | ThunderRush 스킬 구현 | 3시간 | 17 |
| 20 | Stun 상태 효과 | 1시간 | 18 |

### Week 3 태스크

| # | 태스크 | 예상 시간 | 의존성 |
|---|--------|----------|--------|
| 21 | DarkMage.asset 생성 | 30분 | Week 2 완료 |
| 22 | LifeDrain 스킬 구현 | 4시간 | 21 |
| 23 | SoulExplosion 스킬 구현 | 4시간 | 21 |
| 24 | HP 소모 메커닉 | 2시간 | 22 |
| 25 | FormSynergy.cs 구현 | 4시간 | Week 2 완료 |
| 26 | FormAwakening.cs 구현 | 3시간 | Week 2 완료 |
| 27 | 각성 VFX 연동 | 2시간 | 26 |

### Week 4 태스크

| # | 태스크 | 예상 시간 | 의존성 |
|---|--------|----------|--------|
| 28 | 밸런스 테스트 시나리오 작성 | 2시간 | Week 3 완료 |
| 29 | DPS 밸런스 조정 | 4시간 | 28 |
| 30 | 드롭 확률 구현 | 2시간 | Week 3 완료 |
| 31 | VFX 통합 | 4시간 | Week 3 완료 |
| 32 | SFX 통합 | 2시간 | 31 |
| 33 | 최종 테스트 | 4시간 | 32 |
| 34 | 버그 수정 및 폴리싱 | 4시간 | 33 |

---

## 5. 검증 계획

### 5.1 자동 테스트

```csharp
[Test]
public void FormData_AllFormsHaveValidData()
{
    var forms = Resources.LoadAll<FormData>("Data/Forms");
    foreach (var form in forms)
    {
        Assert.IsNotNull(form.skill1, $"{form.formName} skill1 null");
        Assert.IsNotNull(form.skill2, $"{form.formName} skill2 null");
        Assert.IsNotNull(form.icon, $"{form.formName} icon null");
    }
}
```

### 5.2 수동 테스트 체크리스트

- [ ] 각 폼별 기본 공격 정상 작동
- [ ] 각 스킬 쿨다운 정상 작동
- [ ] 폼 교체 시 스탯 변경 확인
- [ ] 교체 시너지 효과 발동 확인
- [ ] 각성 시 등급 업그레이드 확인

---

## 6. 리스크 및 대응

| 리스크 | 영향 | 대응 |
|--------|------|------|
| 연쇄 번개 구현 복잡 | Week 2 지연 | 먼저 단순 버전 구현 후 개선 |
| 밸런스 조정 반복 | Week 4 초과 | 핵심 수치만 조정, 세부 조정은 후순위 |
| 상태 효과 충돌 | 버그 발생 | StatusEffectManager 우선순위 로직 확인 |

---

## 7. 완료 조건

1. [ ] 5종 폼 모두 플레이 가능
2. [ ] 10개 스킬 모두 정상 작동
3. [ ] 폼 교체 시 시너지 발동
4. [ ] 각성 시스템 작동
5. [ ] DPS 밸런스 ±30% 이내
6. [ ] 드롭 시스템 연동

---

*작성: GASPT Planning Agent*
*최종 수정: 2025-11-30*
