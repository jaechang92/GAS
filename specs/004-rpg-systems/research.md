# Phase 0: 기술 연구 및 설계 - RPG Systems

**프로젝트**: GASPT - RPG Systems Feature
**브랜치**: `004-rpg-systems`
**작성일**: 2025-11-01
**상태**: Draft

---

## 1. 개요

이 문서는 RPG Systems 구현을 위한 기술 연구 및 설계 결정을 기록합니다.
특히 **최소 GAS (Gameplay Ability System) 재구현**에 초점을 맞춥니다.

### 1.1 연구 배경

기존 GAS_Core의 핵심 컴포넌트 28개 파일(2,690 라인)이 삭제되었습니다:
- ❌ Ability.cs (493 라인) - 어빌리티 실행 핵심
- ❌ AbilitySystem.cs (559 라인) - 어빌리티 시스템 매니저
- ❌ AbilityData.cs (207 라인) - ScriptableObject 데이터
- ❌ Executor 클래스들 (507 라인) - 데미지/힐 처리
- ❌ 인터페이스들 (206 라인) - 계약 정의
- ❌ FSM_GAS_Integration (422 라인) - FSM-GAS 통합

**보존된 컴포넌트** (333 라인):
- ✅ AbilityCooldown.cs (184 라인) - 쿨다운 관리
- ✅ DefaultGameplayContext.cs (114 라인) - 게임플레이 컨텍스트
- ✅ IGameplayContext.cs (35 라인) - 컨텍스트 인터페이스

### 1.2 연구 목표

Fire Magic Ability 구현에 필요한 **최소한의 GAS 아키텍처** 설계:
- 기존 2,690 라인 → 목표 ~750 라인 (28% 경량화)
- Fire Magic 전용 최적화
- RPG 스탯 시스템 통합
- FSM 상태 기반 실행

---

## 2. Fire Magic Ability 요구사항 매핑

### 2.1 Spec 요구사항

| 요구사항 ID | 내용 | GAS 컴포넌트 필요 |
|------------|------|------------------|
| **FR-004** (003) | Fire Grimoire의 special fire magic ability 활성화 | Ability 클래스 |
| **FR-007** (003) | Fire Grimoire는 하나의 고유 fire magic ability 제공 | AbilityData |
| **FR-008** (003) | Fire magic ability 쿨다운 (5-10초) | ✅ AbilityCooldown (보존) |
| **FR-009** (003) | 쿨다운 상태 UI 표시 | - (UI 시스템) |
| **FR-035** (004) | Fire Grimoire는 레벨 시작 시 장착 | AbilitySystem |
| **FR-036** (004) | Fire magic 데미지는 Attack 스탯에 스케일링 (2.5x) | Ability + Stat 통합 |
| **FR-037** (004) | Fire magic 쿨다운 (7초) | ✅ AbilityCooldown (보존) |
| **FR-038** (004) | Fire magic 쿨다운 UI 표시 | - (UI 시스템) |

### 2.2 최소 GAS 컴포넌트 도출

| 컴포넌트 | 필요 여부 | 이유 |
|---------|----------|------|
| **IAbility** | ✅ 필요 | 어빌리티 계약 정의 |
| **Ability** | ✅ 필요 | Fire Magic 구현 베이스 |
| **AbilityData** | ✅ 필요 | Fire Magic ScriptableObject 데이터 |
| **AbilitySystem** | ✅ 필요 | 어빌리티 실행 관리 (간소화) |
| **FSMAbilityBridge** | ✅ 필요 | FSM 상태 → Ability 호출 통합 |
| AbilityCooldown | ✅ 보존됨 | 쿨다운 관리 (이미 존재) |
| IGameplayContext | ✅ 보존됨 | 게임 상태 접근 (이미 존재) |
| ~~Executor 클래스~~ | ❌ 불필요 | Fire Magic은 직접 데미지 처리 |
| ~~IAbilityTarget~~ | ❌ 불필요 | Fire Magic은 단순 타겟팅 |

---

## 3. 최소 GAS 아키텍처 설계

### 3.1 컴포넌트 구조

```
GAS_Core/
├── Core/
│   ├── Ability.cs (새로 작성, ~200 라인)
│   ├── AbilitySystem.cs (새로 작성, ~250 라인)
│   ├── AbilityCooldown.cs ✅ (보존, 184 라인)
│   └── DefaultGameplayContext.cs ✅ (보존, 114 라인)
├── Data/
│   └── AbilityData.cs (새로 작성, ~100 라인)
├── Interfaces/
│   ├── IAbility.cs (새로 작성, ~50 라인)
│   ├── IAbilitySystem.cs (새로 작성, ~40 라인)
│   └── IGameplayContext.cs ✅ (보존, 35 라인)
└── Integration/
    └── FSMAbilityBridge.cs (새로 작성, ~150 라인)
```

**총 예상**: ~790 라인 (기존 3,023 라인 대비 26%)

### 3.2 인터페이스 설계

#### IAbility 인터페이스

```csharp
namespace GAS.Core
{
    public interface IAbility
    {
        string AbilityName { get; }
        bool CanExecute(IGameplayContext context);
        Awaitable ExecuteAsync(IGameplayContext context);
        void Cancel();
        bool IsExecuting { get; }
        AbilityCooldown Cooldown { get; }
    }
}
```

**설계 결정**:
- ✅ `Awaitable` 사용 (Coroutine 금지, 헌장 원칙 VII)
- ✅ `CanExecute` 검증 (쿨다운, 컨텍스트 상태)
- ✅ `Cancel` 지원 (FSM 상태 전환 시)
- ❌ 복잡한 타겟팅 시스템 제거 (Fire Magic은 단순 전방 발사)

#### IAbilitySystem 인터페이스

```csharp
namespace GAS.Core
{
    public interface IAbilitySystem
    {
        void RegisterAbility(IAbility ability);
        void UnregisterAbility(string abilityName);
        Awaitable<bool> TryExecuteAbilityAsync(string abilityName);
        IAbility GetAbility(string abilityName);
        bool HasAbility(string abilityName);
        void CancelAllAbilities();
    }
}
```

**설계 결정**:
- ✅ 간소화: 이름 기반 등록/실행만 지원
- ✅ `TryExecuteAbilityAsync` → bool 반환 (성공/실패)
- ❌ 복잡한 우선순위/큐잉 시스템 제거
- ❌ 태그 기반 필터링 제거 (Fire Magic만 존재)

---

## 4. Fire Magic Ability 구현 설계

### 4.1 FireMagicAbility 클래스 설계

```csharp
// Assets/_Project/Scripts/Abilities/FireMagicAbility.cs
public class FireMagicAbility : Ability
{
    private FireMagicData data;
    private PlayerStats playerStats; // RPG 스탯 참조

    public override async Awaitable ExecuteAsync(IGameplayContext context)
    {
        // 1. 스탯에서 Attack 값 가져오기
        float attackStat = playerStats.GetStat(StatType.Attack);

        // 2. 데미지 계산 (2.5x Attack)
        float damage = attackStat * data.damageMultiplier; // 2.5

        // 3. Fire 발사체 생성
        GameObject projectile = SpawnFireProjectile(context);

        // 4. 발사체 이동 및 충돌 처리
        await FireProjectileAsync(projectile, damage);

        // 5. 쿨다운 시작
        Cooldown.StartCooldown(data.cooldownDuration); // 7초
    }
}
```

### 4.2 FireMagicData (ScriptableObject)

```csharp
// Assets/_Project/Scripts/Data/FireMagicData.cs
[CreateAssetMenu(fileName = "FireMagicData", menuName = "GASPT/Abilities/FireMagic")]
public class FireMagicData : AbilityData
{
    [Header("Fire Magic 설정")]
    public float damageMultiplier = 2.5f; // Attack의 2.5배
    public float cooldownDuration = 7f; // 7초
    public float projectileSpeed = 15f; // 발사체 속도
    public GameObject projectilePrefab; // Fire 이펙트 프리팹
    public float maxRange = 10f; // 최대 사거리
}
```

### 4.3 FSM-GAS 통합 (FSMAbilityBridge)

```csharp
// Assets/Plugins/FSM_GAS_Integration/FSMAbilityBridge.cs
public class FSMAbilityBridge : MonoBehaviour
{
    private IAbilitySystem abilitySystem;
    private IStateMachine fsm;

    public async void TriggerAbilityFromState(string abilityName)
    {
        // FSM 상태에서 호출 (예: AttackState)
        bool success = await abilitySystem.TryExecuteAbilityAsync(abilityName);

        if (success)
        {
            // Fire Magic 실행 성공
            Debug.Log($"Ability {abilityName} executed successfully");
        }
    }
}
```

---

## 5. 어셈블리 참조 문제 해결

### 5.1 현재 문제

```
GAS.Core.asmdef
  └─ references: ["Core.Enums"] ❌ (존재하지 않음)

FSM.GAS.Integration.asmdef
  └─ references: ["GAS.Core", "FSM.Core", "Core.Enums"] ❌
```

### 5.2 해결 방안

#### Option A: Core.Enums 제거 (권장)

**장점**:
- 간단하고 빠름
- Fire Magic에 복잡한 Enum 불필요

**단점**:
- 향후 여러 어빌리티 추가 시 Enum 필요할 수 있음

**Action**:
```json
// GAS.Core.asmdef
{
  "name": "GAS.Core",
  "references": [], // Core.Enums 제거
  ...
}

// FSM.GAS.Integration.asmdef
{
  "name": "FSM.GAS.Integration",
  "references": ["FSM.Core", "GAS.Core"], // Core.Enums 제거
  ...
}
```

#### Option B: 최소 Core.Enums 생성

필요 시 최소한의 Enum 파일 생성:

```csharp
// Assets/_Project/Scripts/Core/Enums/AbilityEnums.cs
namespace Core.Enums
{
    public enum AbilityType
    {
        FireMagic
    }

    public enum TargetType
    {
        Enemy
    }
}
```

**결정**: **Option A 선택** - 불필요한 의존성 제거

---

## 6. 성능 최적화 고려사항

### 6.1 비동기 패턴 (Awaitable)

**요구사항**: 헌장 원칙 VI - Coroutine 금지, async/await Awaitable만 사용

**검증 결과**:
- ✅ Unity 2023.3+ Awaitable 지원 확인됨
- ✅ `await Awaitable.WaitForSecondsAsync()` 사용 가능
- ✅ Fire 발사체 이동에 Awaitable 적용 가능

**예시**:
```csharp
private async Awaitable FireProjectileAsync(GameObject projectile, float damage)
{
    float elapsed = 0f;
    while (elapsed < data.maxRange / data.projectileSpeed)
    {
        // 발사체 이동
        projectile.transform.position += transform.forward * data.projectileSpeed * Time.deltaTime;

        // 충돌 체크
        if (CheckHit(projectile, out Collider2D hit))
        {
            ApplyDamage(hit, damage);
            Destroy(projectile);
            return;
        }

        await Awaitable.NextFrameAsync();
        elapsed += Time.deltaTime;
    }

    Destroy(projectile); // 최대 사거리 도달
}
```

### 6.2 스탯 재계산 성능 (SC-001: <50ms)

**요구사항**: 스탯 디스플레이 업데이트 <50ms (004 spec.md SC-001)

**최적화 전략**:
- ✅ 더티 플래그 패턴 (스탯 변경 시에만 재계산)
- ✅ 캐싱: Fire Magic은 Attack 스탯만 참조
- ❌ LINQ 사용 제한 (헌장 원칙 III)

```csharp
public class PlayerStats
{
    private bool isDirty = true;
    private float cachedAttack;

    public float GetAttack()
    {
        if (isDirty)
        {
            cachedAttack = CalculateAttack(); // baseAttack + 장비 보너스
            isDirty = false;
        }
        return cachedAttack;
    }

    public void MarkDirty() => isDirty = true;
}
```

### 6.3 메모리 최적화

- Fire 발사체 Object Pooling 고려 (향후)
- ScriptableObject 데이터 공유 (메모리 효율)
- 이벤트 리스너 정리 (메모리 누수 방지)

---

## 7. 구현 우선순위

### Phase 0 (연구 - 현재)
- ✅ research.md 작성
- ⏳ 어셈블리 참조 문제 해결
- ⏳ 최소 GAS 설계 검증

### Phase 1 (설계 문서)
- ⏳ data-model.md 작성
- ⏳ api-contracts.md 작성
- ⏳ quickstart.md 작성

### Phase 2 (구현 순서)
1. **IAbility, IAbilitySystem 인터페이스** (~50 + 40 = 90 라인)
2. **Ability 베이스 클래스** (~200 라인)
3. **AbilityData ScriptableObject** (~100 라인)
4. **AbilitySystem 관리자** (~250 라인)
5. **FSMAbilityBridge 통합** (~150 라인)
6. **FireMagicAbility 구현** (~200 라인)
7. **FireMagicData 데이터** (~50 라인)

**총 예상**: ~1,040 라인 (통합 및 Fire Magic 구현 포함)

---

## 8. 위험 요소 및 완화

| 위험 | 확률 | 영향 | 완화 전략 |
|------|------|------|----------|
| Awaitable 패턴 미숙 | 중 | 높 | Phase 0에서 간단한 프로토타입 테스트 |
| 스탯 시스템 미완성 | 중 | 높 | Stat 시스템 먼저 구현 (P1) |
| FSM-GAS 통합 복잡도 | 중 | 중 | 간소화된 Bridge 패턴 사용 |
| 어셈블리 참조 깨짐 | 낮 | 낮 | Core.Enums 제거로 해결 |
| 성능 목표 미달 (<50ms) | 낮 | 중 | 더티 플래그 + 캐싱 |

---

## 9. 다음 단계

### 즉시 작업
1. ✅ research.md 완료
2. ⏳ 어셈블리 참조 수정 (GAS.Core.asmdef, FSM.GAS.Integration.asmdef)
3. ⏳ Unity Editor 컴파일 검증

### Phase 1 준비
1. data-model.md 작성 시작
2. IAbility, IAbilitySystem API 계약 정의
3. Stat System 설계 (P1 선행 작업)

---

## 10. 참고 자료

### Unity 문서
- Unity Awaitable: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Awaitable.html
- ScriptableObject: https://docs.unity3d.com/Manual/class-ScriptableObject.html
- Assembly Definitions: https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html

### 프로젝트 문서
- Spec: [004-rpg-systems/spec.md](./spec.md)
- Plan: [004-rpg-systems/plan.md](./plan.md)
- 헌장: [GASPT/CLAUDE.md](../../CLAUDE.md)
- 003 Spec (Fire Grimoire 원본): [003-first-playable-level/spec.md](../003-first-playable-level/spec.md)

---

**버전**: 1.0
**상태**: Draft - Phase 0
**최종 수정**: 2025-11-01
**작성자**: Claude Code + User
