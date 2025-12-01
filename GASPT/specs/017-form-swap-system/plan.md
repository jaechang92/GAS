# 폼 교체 시스템 구현 계획

**기능 번호**: 017
**작성일**: 2025-12-01
**예상 기간**: 3주

---

## 1. 기술 컨텍스트

### 1.1 기존 시스템 활용

| 시스템 | 파일 위치 | 역할 |
|--------|----------|------|
| **PlayerStats** | `Assets/_Project/Scripts/Stats/PlayerStats.cs` | 스탯 적용/변경 |
| **SkillSystem** | `Assets/_Project/Scripts/Skills/SkillSystem.cs` | 스킬 교체/실행 |
| **SkillData** | `Assets/_Project/Scripts/Skills/SkillData.cs` | 스킬 정의 |
| **InputSystem** | `Assets/_Project/Scripts/Input/` | Q키 입력 처리 |
| **InventorySystem** | `Assets/_Project/Scripts/Gameplay/Inventory/` | 아이템 관리 참고 |

### 1.2 기존 Form 시스템 분석

```
Assets/_Project/Scripts/Player/Forms/
├── MageForm.cs          # 기존 폼 기반 클래스
├── IForm.cs             # 폼 인터페이스 (있다면)
└── FormController.cs    # 폼 전환 로직 (있다면)
```

### 1.3 신규 생성 필요

| 파일명 | 경로 | 설명 |
|--------|------|------|
| `FormData.cs` | `Assets/_Project/Scripts/Forms/` | 폼 ScriptableObject |
| `FormInstance.cs` | `Assets/_Project/Scripts/Forms/` | 런타임 폼 상태 |
| `FormManager.cs` | `Assets/_Project/Scripts/Forms/` | 폼 교체/관리 핵심 |
| `FormSlot.cs` | `Assets/_Project/Scripts/Forms/` | 폼 슬롯 데이터 |
| `FormSwapSystem.cs` | `Assets/_Project/Scripts/Forms/` | 교체 로직 |
| `FormPickup.cs` | `Assets/_Project/Scripts/Forms/` | 폼 획득 상호작용 |
| `FormHUDView.cs` | `Assets/_Project/Scripts/UI/Forms/` | HUD 폼 슬롯 표시 |
| `FormInfoPopup.cs` | `Assets/_Project/Scripts/UI/Forms/` | 폼 정보 팝업 |

### 1.4 의존성 구조

```
017-form-swap-system
├── PlayerStats (스탯 적용)
├── SkillSystem (스킬 교체)
├── InputSystem (Q키 입력)
├── UISystem (HUD, 팝업)
└── 019-form-content-design (폼 데이터 - 병렬 개발)
```

---

## 2. 아키텍처 설계

### 2.1 클래스 다이어그램

```
┌─────────────────┐      ┌─────────────────┐
│   FormData      │◄─────│  FormInstance   │
│ (ScriptableObj) │      │   (Runtime)     │
└────────┬────────┘      └────────┬────────┘
         │                        │
         │                        ▼
         │              ┌─────────────────┐
         └─────────────►│   FormManager   │
                        │ (Singleton)     │
                        └────────┬────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ FormSwapSystem  │    │   FormPickup    │    │  FormHUDView    │
│ (교체 로직)     │    │ (획득 처리)     │    │  (UI 표시)      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### 2.2 이벤트 흐름

```
Q키 입력 → FormManager.TrySwap() → FormSwapSystem.ExecuteSwap()
    ↓
┌──────────────────────────────────────────┐
│ 1. 쿨다운 확인                           │
│ 2. 현재 폼 비활성화                      │
│ 3. 대기 폼 활성화                        │
│ 4. PlayerStats에 새 스탯 적용            │
│ 5. SkillSystem에 새 스킬 교체            │
│ 6. 애니메이터 컨트롤러 교체              │
│ 7. 교체 이펙트 재생                      │
│ 8. 쿨다운 시작                           │
│ 9. OnFormSwapped 이벤트 발행             │
└──────────────────────────────────────────┘
    ↓
FormHUDView.UpdateDisplay() (이벤트 구독)
```

---

## 3. 핵심 컴포넌트 설계

### 3.1 FormData ScriptableObject

```csharp
[CreateAssetMenu(fileName = "FormData", menuName = "GASPT/Forms/FormData")]
public class FormData : ScriptableObject
{
    [Header("기본 정보")]
    public string formId;
    public string formName;
    [TextArea] public string description;
    public FormType formType;
    public FormRarity baseRarity;

    [Header("스탯")]
    public FormStats baseStats;

    [Header("스킬")]
    public SkillData skill1;
    public SkillData skill2;

    [Header("비주얼")]
    public Sprite icon;
    public RuntimeAnimatorController animatorController;
    public GameObject swapEffectPrefab;
    public Color themeColor;
}
```

### 3.2 FormManager

```csharp
public class FormManager : MonoBehaviour
{
    // 싱글톤 또는 ServiceLocator 패턴
    public static FormManager Instance { get; private set; }

    // 폼 슬롯 (최대 2개)
    private FormInstance currentForm;
    private FormInstance reserveForm;

    // 이벤트
    public event Action<FormInstance, FormInstance> OnFormSwapped;
    public event Action<FormInstance> OnFormAcquired;
    public event Action<FormInstance> OnFormAwakened;

    // 교체 관련
    private float swapCooldown = 5f;
    private float currentCooldown = 0f;
    public bool CanSwap => currentCooldown <= 0f && reserveForm != null;

    // 주요 메서드
    public void TrySwap();
    public void AcquireForm(FormData formData);
    public void ReplaceForm(int slotIndex, FormData newForm);
}
```

### 3.3 FormSwapSystem

```csharp
public class FormSwapSystem
{
    private readonly PlayerStats playerStats;
    private readonly SkillSystem skillSystem;
    private readonly Animator playerAnimator;

    public void ExecuteSwap(FormInstance from, FormInstance to)
    {
        // 1. 스탯 변경
        playerStats.RemoveFormBonus(from);
        playerStats.ApplyFormBonus(to);

        // 2. 스킬 교체
        skillSystem.SetSkill(0, to.FormData.skill1);
        skillSystem.SetSkill(1, to.FormData.skill2);

        // 3. 애니메이터 교체
        playerAnimator.runtimeAnimatorController = to.FormData.animatorController;

        // 4. 이펙트 재생
        SpawnSwapEffect(to.FormData.swapEffectPrefab);

        // 5. 무적 프레임 (0.2초)
        ApplyInvincibility(0.2f);
    }
}
```

---

## 4. UI 설계

### 4.1 HUD 폼 슬롯

```
┌─────────────────────────────────┐
│  [현재 폼]    [Q]    [대기 폼]  │
│  ┌─────┐           ┌─────┐     │
│  │ 🔥  │    ←→     │ ❄️  │     │
│  │ Lv3 │           │ Lv2 │     │
│  └─────┘           └─────┘     │
│            [CD: 3s]            │
└─────────────────────────────────┘
```

### 4.2 폼 정보 팝업

```
┌─────────────────────────────────────────┐
│ 🔥 화염 마법사                    [Rare]│
├─────────────────────────────────────────┤
│ "불꽃을 다루는 공격적인 마법사"          │
├─────────────────────────────────────────┤
│ 공격력: +8     이동속도: 0.95x          │
│ 공격속도: 0.9x  마나: +15               │
├─────────────────────────────────────────┤
│ 스킬 1: 화염구                          │
│   범위 폭발 공격, 쿨다운 5초            │
│                                         │
│ 스킬 2: 화염 폭풍                       │
│   지속 범위 공격, 쿨다운 15초           │
├─────────────────────────────────────────┤
│     [획득]        [취소]                │
└─────────────────────────────────────────┘
```

### 4.3 폼 교체 선택 UI

```
┌─────────────────────────────────────────┐
│     새로운 폼을 획득했습니다!           │
│                                         │
│  ┌──────────┐         ┌──────────┐     │
│  │ 🔥 화염  │   VS    │ ⚡ 번개   │     │
│  │ [현재]   │         │ [새 폼]   │     │
│  └──────────┘         └──────────┘     │
│                                         │
│  버릴 폼을 선택하세요:                  │
│  [화염 마법사]  [얼음 마법사]  [취소]   │
└─────────────────────────────────────────┘
```

---

## 5. 구현 Phase 계획

### Phase 1: 기반 시스템 (Week 1)

| 태스크 | 설명 | 예상 시간 |
|--------|------|----------|
| FormData.cs | ScriptableObject 정의 | 2시간 |
| FormInstance.cs | 런타임 데이터 클래스 | 1시간 |
| FormEnums.cs | FormType, FormRarity | 30분 |
| FormStats.cs | 스탯 구조체 | 30분 |
| FormManager.cs | 기본 관리 로직 | 4시간 |
| 기본 폼 에셋 | BasicMage.asset | 1시간 |

### Phase 2: 교체 시스템 (Week 2)

| 태스크 | 설명 | 예상 시간 |
|--------|------|----------|
| FormSwapSystem.cs | 교체 실행 로직 | 4시간 |
| 입력 연동 | Q키 → FormManager | 2시간 |
| 스탯 적용 | PlayerStats 연동 | 3시간 |
| 스킬 교체 | SkillSystem 연동 | 3시간 |
| 애니메이터 교체 | RuntimeAnimatorController | 2시간 |
| 교체 이펙트 | VFX 재생 | 2시간 |
| 무적 프레임 | 0.2초 무적 | 1시간 |

### Phase 3: 획득/UI (Week 3)

| 태스크 | 설명 | 예상 시간 |
|--------|------|----------|
| FormPickup.cs | 폼 아이템 상호작용 | 3시간 |
| FormHUDView.cs | HUD 폼 슬롯 | 4시간 |
| FormInfoPopup.cs | 정보 팝업 | 4시간 |
| FormSelectPopup.cs | 교체 선택 UI | 3시간 |
| 각성 시스템 | 자동 각성 로직 | 3시간 |
| 각성 이펙트 | VFX + 메시지 | 2시간 |

---

## 6. 파일 생성 목록

### 6.1 스크립트 파일

```
Assets/_Project/Scripts/Forms/
├── Data/
│   ├── FormData.cs
│   ├── FormInstance.cs
│   ├── FormEnums.cs
│   └── FormStats.cs
├── System/
│   ├── FormManager.cs
│   ├── FormSwapSystem.cs
│   └── FormAwakening.cs
└── Pickup/
    └── FormPickup.cs

Assets/_Project/Scripts/UI/Forms/
├── FormHUDView.cs
├── FormHUDPresenter.cs
├── FormInfoPopup.cs
└── FormSelectPopup.cs
```

### 6.2 프리팹

```
Assets/_Project/Prefabs/Forms/
├── FormPickup.prefab
└── SwapEffect.prefab

Assets/_Project/Prefabs/UI/Forms/
├── FormHUD.prefab
├── FormInfoPopup.prefab
└── FormSelectPopup.prefab
```

### 6.3 ScriptableObject 에셋

```
Assets/Resources/Data/Forms/
├── BasicMage.asset
└── (019에서 추가 폼 생성)
```

---

## 7. 검증 계획

### 7.1 단위 테스트

```csharp
[Test]
public void FormSwap_ChangesStats()
{
    // Given: 두 개의 다른 폼
    // When: 폼 교체 실행
    // Then: PlayerStats가 새 폼 기준으로 변경됨
}

[Test]
public void FormSwap_RespectsColldown()
{
    // Given: 쿨다운 중
    // When: 교체 시도
    // Then: 교체 실패
}
```

### 7.2 통합 테스트 체크리스트

- [ ] Q키 입력 시 폼 교체
- [ ] 교체 시 스탯 즉시 변경
- [ ] 교체 시 스킬 즉시 변경
- [ ] 교체 시 외형 즉시 변경
- [ ] 쿨다운 동안 교체 불가
- [ ] HUD에 현재/대기 폼 표시
- [ ] 폼 픽업 시 정보 팝업 표시
- [ ] 슬롯 가득 시 교체 선택 UI
- [ ] 동일 폼 획득 시 자동 각성

---

## 8. 리스크 및 대응

| 리스크 | 영향 | 대응 |
|--------|------|------|
| 기존 Form 시스템 충돌 | 코드 복잡도 증가 | 기존 시스템 분석 후 통합/대체 결정 |
| 애니메이터 전환 끊김 | UX 저하 | CrossFade 활용, 전환 애니메이션 |
| 스탯 적용 타이밍 | 버그 발생 | 명확한 순서 정의, 이벤트 기반 |

---

## 9. 완료 조건

1. [ ] Q키로 폼 교체 가능 (0.1초 이내)
2. [ ] 교체 시 스탯/스킬/외형 즉시 변경
3. [ ] 쿨다운 시스템 작동
4. [ ] 무적 프레임 적용
5. [ ] HUD에 폼 상태 표시
6. [ ] 폼 획득/교체 UI 작동
7. [ ] 각성 시스템 작동

---

*작성: GASPT Planning Agent*
*최종 수정: 2025-12-01*
