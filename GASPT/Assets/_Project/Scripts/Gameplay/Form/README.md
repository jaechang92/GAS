# Form System

**작성일**: 2025-11-10
**Phase**: A-1 (Platformer Implementation)
**상태**: 완료 ✅

---

## 📖 개요

Form System은 "Skul: The Hero Slayer"에서 영감을 받은 캐릭터 변신 시스템입니다. 플레이어는 다양한 Form으로 변신하여 각기 다른 스킬과 스탯을 사용할 수 있습니다.

**용어 변경**: "Skull" → "Form" (저작권 문제 회피)

---

## 🏗️ 아키텍처

### 핵심 구성요소

```
Form/
├── Core/                    # 핵심 인터페이스 및 베이스 클래스
│   ├── IFormController.cs   # Form 인터페이스
│   ├── FormData.cs          # ScriptableObject 데이터
│   └── BaseForm.cs          # 추상 베이스 클래스
├── Implementations/         # 구체적인 Form 구현
│   └── MageForm.cs          # 마법사 Form
└── Abilities/               # Form별 스킬 구현
    ├── MagicMissileAbility.cs
    ├── TeleportAbility.cs
    └── FireballAbility.cs
```

---

## 📋 인터페이스 및 타입

### IFormController

모든 Form이 구현해야 하는 핵심 인터페이스:

```csharp
public interface IFormController
{
    // 기본 정보
    string FormName { get; }
    FormType FormType { get; }

    // 생명주기
    void Activate();
    void Deactivate();

    // 스탯
    float MaxHealth { get; }
    float MoveSpeed { get; }
    float JumpPower { get; }

    // 스킬 관리
    void SetAbility(int slotIndex, IAbility ability);
    IAbility GetAbility(int slotIndex);
}
```

### FormType Enum

```csharp
public enum FormType
{
    Mage,      // 마법사 - 원거리 마법 공격
    Warrior,   // 전사 - 근접 물리 공격 (미구현)
    Assassin,  // 암살자 - 빠른 연속 공격 (미구현)
    Tank       // 탱커 - 높은 방어력 (미구현)
}
```

### IAbility

모든 스킬이 구현해야 하는 인터페이스:

```csharp
public interface IAbility
{
    string AbilityName { get; }
    float Cooldown { get; }
    Task ExecuteAsync(GameObject caster, CancellationToken token);
}
```

---

## 🧙 MageForm (마법사 Form)

### 스탯
- **MaxHealth**: FormData 기반 (기본 100)
- **MoveSpeed**: FormData 기반 (기본 5)
- **JumpPower**: FormData 기반 (기본 10)

### 기본 스킬 (4슬롯)

| 슬롯 | 스킬명 | 타입 | 쿨다운 | 설명 |
|-----|-------|------|--------|------|
| 0 | Magic Missile | 기본 공격 | 0.5초 | 마우스 방향으로 빠른 마법 투사체 발사 (데미지 10) |
| 1 | Teleport | 스킬 1 | 3초 | 마우스 방향으로 5m 순간이동 |
| 2 | Fireball | 스킬 2 | 5초 | 강력한 화염구 발사 (직격 50, 폭발 반경 3m) |
| 3 | (Empty) | - | - | 미할당 |

---

## 💻 사용 방법

### 1. FormData 생성

```
Unity Editor:
Create > GASPT > Form > Form Data
```

**설정 항목**:
- formName: "Mage"
- formType: FormType.Mage
- maxHealth: 100
- moveSpeed: 5
- jumpPower: 10
- icon, formSprite, formColor 등

### 2. Form 스크립트 작성

```csharp
public class MageForm : BaseForm
{
    public override string FormName => "Mage";
    public override FormType FormType => FormType.Mage;

    private void Awake()
    {
        // 기본 스킬 초기화
        SetAbility(0, new MagicMissileAbility());
        SetAbility(1, new TeleportAbility());
        SetAbility(2, new FireballAbility());
    }

    protected override void OnFormActivated()
    {
        base.OnFormActivated();
        // 활성화 시 추가 로직
    }
}
```

### 3. Ability 스크립트 작성

```csharp
public class MagicMissileAbility : IAbility
{
    public string AbilityName => "Magic Missile";
    public float Cooldown => 0.5f;

    private float lastUsedTime;

    public async Task ExecuteAsync(GameObject caster, CancellationToken token)
    {
        // 쿨다운 체크
        if (Time.time - lastUsedTime < Cooldown) return;

        // 마우스 방향 계산
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 direction = (mousePos - caster.transform.position).normalized;

        // 스킬 실행
        FireMissile(caster.transform.position, direction);

        // 쿨다운 시작
        lastUsedTime = Time.time;

        // 비동기 대기
        await Awaitable.NextFrameAsync(token);
    }

    private void FireMissile(Vector3 startPos, Vector2 direction)
    {
        // TODO: 투사체 생성
    }
}
```

---

## 🔄 생명주기

### Form 활성화

```
Activate() 호출
    ↓
gameObject.SetActive(true)
    ↓
OnFormActivated() (가상 메서드)
    ↓
ApplyMageStats() (MageForm 전용)
    ↓
PlayMagicAuraEffect()
```

### Form 비활성화

```
Deactivate() 호출
    ↓
OnFormDeactivated() (가상 메서드)
    ↓
StopMagicAuraEffect()
    ↓
gameObject.SetActive(false)
```

---

## 🎮 스킬 실행 흐름

### 1. 입력 감지 (미구현)
```
플레이어 입력 (마우스 클릭, 키보드)
    ↓
FormController.UseAbility(slotIndex)
```

### 2. 스킬 실행
```
GetAbility(slotIndex)
    ↓
ability.ExecuteAsync(caster, token)
    ↓
쿨다운 체크 (Time.time)
    ↓
마우스 방향 계산 (Camera.main.ScreenToWorldPoint)
    ↓
스킬 효과 적용 (TODO: 투사체, 이펙트)
    ↓
비동기 대기 (Awaitable)
```

---

## 📝 TODO 목록

### Phase A-1 (완료 ✅)
- [x] IFormController 인터페이스 정의
- [x] FormData ScriptableObject 구조
- [x] BaseForm 추상 클래스
- [x] MageForm 구현
- [x] MagicMissileAbility (기본 공격)
- [x] TeleportAbility (순간이동)
- [x] FireballAbility (화염구)

### Phase A-2 (예정)
- [ ] FormManager 싱글톤 (Form 전환 관리)
- [ ] 투사체 프리팹 생성 (Magic Missile, Fireball)
- [ ] 이펙트 프리팹 생성 (폭발, 텔레포트)
- [ ] 플레이어 입력 처리 (InputSystem)
- [ ] Enemy와 스킬 연동 (데미지, DamageNumber)

### Phase A-4 (아이템-스킬 시스템)
- [ ] SkillItemData ScriptableObject
- [ ] 아이템 획득 시 스킬 교체
- [ ] 스킬 UI 업데이트
- [ ] 2~3개 추가 스킬 아이템 구현

---

## 🧪 테스트 방법

### Context Menu 테스트

MageForm 컴포넌트에서 우클릭:
- `Print Form Info` - 현재 Form 정보 출력
- `Test Magic Missile` - Magic Missile 테스트 (미구현)
- `Test Teleport` - Teleport 테스트 (미구현)
- `Test Fireball` - Fireball 테스트 (미구현)

### 수동 테스트

```csharp
// Form 활성화
MageForm mageForm = GetComponent<MageForm>();
mageForm.Activate();

// 스킬 사용
IAbility ability = mageForm.GetAbility(0);
if (ability != null)
{
    await ability.ExecuteAsync(gameObject, default);
}

// Form 비활성화
mageForm.Deactivate();
```

---

## 🔧 설계 특징

### 1. Awaitable 패턴 (Coroutine 금지)
- 모든 비동기 로직에 `async/await` 사용
- `CancellationToken`으로 작업 취소 관리
- Unity 6.0 Awaitable API 활용

```csharp
public async Task ExecuteAsync(GameObject caster, CancellationToken token)
{
    await Awaitable.WaitForSecondsAsync(3f, token);
}
```

### 2. Interface 기반 설계
- `IFormController`, `IAbility`로 확장성 보장
- 다형성을 통한 유연한 시스템

### 3. ScriptableObject 데이터 분리
- 디자이너 친화적인 데이터 설정
- 런타임 코드와 데이터 분리
- 에셋 재사용 가능

### 4. 마우스 방향 계산
- 모든 스킬이 마우스 위치 기반 방향 결정
- `Camera.main.ScreenToWorldPoint` 사용
- 2D 플랫포머에 최적화

### 5. 쿨다운 시스템
- `Time.time` 기반 쿨다운 체크
- 각 Ability가 독립적으로 쿨다운 관리
- lastUsedTime 필드로 마지막 사용 시간 추적

---

## ⚠️ 주의사항

### 1. Ability 인스턴스 공유 문제
현재 구현에서는 각 Ability가 클래스 인스턴스이므로, 여러 Form이 같은 Ability 인스턴스를 공유하면 쿨다운이 충돌할 수 있습니다.

**해결 방법**:
- 각 Form마다 새로운 Ability 인스턴스 생성 (현재 방식)
- 또는 Ability를 ScriptableObject로 변경하여 데이터와 로직 분리

### 2. 마우스 방향 계산 (2D)
- `mousePos.z = 0`으로 2D 평면에 고정
- 3D 게임으로 확장 시 수정 필요

### 3. TODO 주석
- 투사체, 이펙트 등 많은 부분이 TODO 상태
- 실제 구현은 Phase A-2 이후 진행

---

## 📚 참고 문서

- **FORM_PLATFORMER_IMPLEMENTATION_PLAN.md** - Phase A 전체 구현 계획
- **WORK_STATUS.md** - 프로젝트 현황
- **IFormController.cs** - 인터페이스 정의
- **BaseForm.cs** - 베이스 클래스 구현

---

**최종 업데이트**: 2025-11-10
**작성자**: Phase A-1 Implementation
