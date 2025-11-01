# Quick Start Guide: 최소 GAS 사용법

**프로젝트**: GASPT - RPG Systems Feature
**브랜치**: `004-rpg-systems`
**작성일**: 2025-11-01
**상태**: Draft - Phase 1

---

## 1. 개요

이 가이드는 최소 GAS (Gameplay Ability System)를 사용하여 **5분 안에 Fire Magic Ability**를 실행하는 방법을 안내합니다.

### 1.1 전제조건

- Unity 2023.3+ 설치됨
- GASPT 프로젝트 열림
- 브랜치: `004-rpg-systems`
- FSM_Core, GAS_Core 프레임워크 존재

---

## 2. 빠른 시작 (5분)

### 2.1 Fire Magic Data 생성

**1단계**: ScriptableObject 데이터 생성

```
Unity Editor:
1. Project 창에서 우클릭
2. Create → GASPT → Abilities → FireMagic
3. 이름: "FireMagicData"
4. Inspector에서 설정:
   - abilityName: "FireMagic"
   - damageMultiplier: 2.5
   - cooldownDuration: 7.0
   - projectileSpeed: 15.0
   - maxRange: 10.0
   - projectilePrefab: (Fire 이펙트 프리팹 할당)
```

---

### 2.2 Fire Magic Ability 스크립트 작성

**위치**: `Assets/_Project/Scripts/Abilities/FireMagicAbility.cs`

```csharp
using UnityEngine;
using GAS.Core;
using GASPT.Stats;

namespace GASPT.Abilities
{
    /// <summary>
    /// Fire Magic 어빌리티 구현 (Attack 스탯 스케일링)
    /// </summary>
    public class FireMagicAbility : Ability
    {
        [Header("Data")]
        [SerializeField] private FireMagicData data;

        [Header("Dependencies")]
        [SerializeField] private PlayerStats playerStats;


        // ====== 초기화 ======

        private void Awake()
        {
            // 어빌리티 이름 설정
            abilityName = "FireMagic";

            // 쿨다운 초기화
            cooldown = new AbilityCooldown();
            cooldown.Initialize(data.cooldownDuration);
        }


        // ====== IAbility 구현 ======

        public override bool CanExecute(IGameplayContext context)
        {
            // 1. 생존 확인
            if (!context.IsAlive) return false;

            // 2. 행동 가능 확인
            if (!context.CanAct) return false;

            // 3. 쿨다운 확인
            if (!cooldown.CanUse()) return false;

            return true;
        }

        public override async Awaitable ExecuteAsync(IGameplayContext context)
        {
            isExecuting = true;

            // 1. Attack 스탯 가져오기
            int attackStat = playerStats.GetStat(StatType.Attack);

            // 2. 데미지 계산 (2.5x Attack)
            float damage = attackStat * data.damageMultiplier;

            // 3. 발사 위치/방향
            Vector3 spawnPos = context.Position + context.Forward * 0.5f;
            Vector3 direction = context.Forward;

            // 4. 발사체 생성
            GameObject projectile = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);

            // 5. 발사체 비행
            await FireProjectileAsync(projectile, direction, damage);

            // 6. 쿨다운 시작
            cooldown.StartCooldown();

            isExecuting = false;
        }

        public override void Cancel()
        {
            // Fire Magic은 즉발이므로 취소 불가
            isExecuting = false;
        }


        // ====== 발사체 처리 ======

        private async Awaitable FireProjectileAsync(GameObject projectile, Vector3 direction, float damage)
        {
            float elapsed = 0f;
            float maxTime = data.maxRange / data.projectileSpeed;

            while (elapsed < maxTime)
            {
                // 이동
                projectile.transform.position += direction * data.projectileSpeed * Time.deltaTime;

                // 충돌 체크
                Collider2D hit = Physics2D.OverlapCircle(
                    projectile.transform.position,
                    0.3f,
                    data.enemyLayer
                );

                if (hit != null)
                {
                    // 데미지 적용
                    var enemy = hit.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                    }

                    // 발사체 파괴
                    Destroy(projectile);
                    return;
                }

                // 다음 프레임 대기
                await Awaitable.NextFrameAsync();
                elapsed += Time.deltaTime;
            }

            // 최대 사거리 도달 시 파괴
            Destroy(projectile);
        }
    }
}
```

---

### 2.3 플레이어에 컴포넌트 추가

**Unity Editor**:

```
1. Hierarchy에서 Player GameObject 선택
2. Add Component:
   - PlayerStats (스탯 관리)
   - DefaultGameplayContext (게임플레이 컨텍스트)
   - FireMagicAbility (Fire Magic)
3. FireMagicAbility Inspector 설정:
   - data: FireMagicData asset 할당
   - playerStats: PlayerStats 컴포넌트 할당
```

---

### 2.4 AbilitySystem에 등록

**위치**: `Assets/_Project/Scripts/Player/PlayerAbilityController.cs`

```csharp
using UnityEngine;
using GAS.Core;

namespace GASPT.Player
{
    /// <summary>
    /// 플레이어 어빌리티 입력 처리 및 AbilitySystem 관리
    /// </summary>
    public class PlayerAbilityController : MonoBehaviour
    {
        private IAbilitySystem abilitySystem;
        private IGameplayContext context;

        private void Start()
        {
            // AbilitySystem 초기화
            abilitySystem = AbilitySystem.Instance;
            context = GetComponent<IGameplayContext>();
            abilitySystem.Initialize(context);

            // Fire Magic 등록
            var fireMagic = GetComponent<FireMagicAbility>();
            abilitySystem.RegisterAbility(fireMagic);

            // 이벤트 구독
            abilitySystem.OnAbilityExecuted += OnAbilitySuccess;
            abilitySystem.OnAbilityFailed += OnAbilityFail;
            abilitySystem.OnCooldownStarted += OnCooldownStart;
        }

        private void Update()
        {
            // 쿨다운 업데이트
            abilitySystem.Update();

            // Fire Magic 입력 (Q 키)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _ = TryFireMagicAsync();
            }
        }

        private async Awaitable TryFireMagicAsync()
        {
            bool success = await abilitySystem.TryExecuteAbilityAsync("FireMagic");

            if (!success)
            {
                Debug.Log("Fire Magic 사용 불가 (쿨다운 또는 조건 불만족)");
            }
        }

        private void OnAbilitySuccess(string name)
        {
            Debug.Log($"{name} 실행 성공!");
        }

        private void OnAbilityFail(string name)
        {
            Debug.Log($"{name} 실행 실패");
        }

        private void OnCooldownStart(string name, float duration)
        {
            Debug.Log($"{name} 쿨다운 시작: {duration}초");
        }
    }
}
```

---

### 2.5 테스트 실행

**Unity Editor**:

```
1. Play 버튼 클릭
2. Q 키 입력
3. 결과 확인:
   - Fire 발사체 생성됨
   - 콘솔: "FireMagic 실행 성공!"
   - 콘솔: "FireMagic 쿨다운 시작: 7초"
   - 7초 동안 Q 키 무반응
   - 7초 후 다시 Fire Magic 사용 가능
```

---

## 3. FSM 통합 예제

### 3.1 AttackState에서 Fire Magic 호출

**위치**: `Assets/_Project/Scripts/Player/States/AttackState.cs`

```csharp
using UnityEngine;
using FSM.Core;
using GAS.Core;

namespace GASPT.Player.States
{
    /// <summary>
    /// 공격 상태 (Fire Magic 실행)
    /// </summary>
    public class AttackState : State
    {
        private IAbilitySystem abilitySystem;
        private bool abilityExecuted;

        public override async void OnEnter()
        {
            Debug.Log("AttackState 진입");

            abilityExecuted = false;

            // AbilitySystem 가져오기
            abilitySystem = AbilitySystem.Instance;

            // Fire Magic 실행
            bool success = await abilitySystem.TryExecuteAbilityAsync("FireMagic");

            if (success)
            {
                abilityExecuted = true;
                Debug.Log("Fire Magic 발사!");
            }
            else
            {
                Debug.Log("Fire Magic 실패 (쿨다운)");
            }

            // 상태 전환 (Idle로 복귀)
            stateMachine.ChangeState("Idle");
        }

        public override void OnExit()
        {
            Debug.Log("AttackState 퇴장");
        }
    }
}
```

**FSM 구성**:
```
[Idle]
  → (Q 키 입력) → [Attack]
      → Fire Magic 실행
      → [Idle]로 복귀
```

---

### 3.2 FSMAbilityBridge 사용

**위치**: `Assets/Plugins/FSM_GAS_Integration/FSMAbilityBridge.cs`

```csharp
using UnityEngine;
using FSM.Core;
using GAS.Core;

namespace FSM.Core.Integration
{
    /// <summary>
    /// FSM과 GAS를 연결하는 브릿지
    /// </summary>
    public class FSMAbilityBridge : MonoBehaviour
    {
        [SerializeField] private StateMachine fsm;
        private IAbilitySystem abilitySystem;

        private void Start()
        {
            abilitySystem = AbilitySystem.Instance;
        }

        /// <summary>
        /// FSM 상태에서 호출: 어빌리티 실행
        /// </summary>
        /// <param name="abilityName">실행할 어빌리티 이름</param>
        public async void TriggerAbility(string abilityName)
        {
            bool success = await abilitySystem.TryExecuteAbilityAsync(abilityName);

            if (success)
            {
                Debug.Log($"FSM → {abilityName} 실행 성공");
            }
            else
            {
                Debug.Log($"FSM → {abilityName} 실행 실패");
            }
        }

        /// <summary>
        /// FSM 상태 전환 시 모든 어빌리티 취소
        /// </summary>
        public void CancelAllAbilities()
        {
            abilitySystem.CancelAllAbilities();
            Debug.Log("FSM 상태 전환 → 모든 어빌리티 취소");
        }
    }
}
```

**AttackState에서 사용**:
```csharp
public class AttackState : State
{
    private FSMAbilityBridge bridge;

    public override void OnEnter()
    {
        bridge = stateMachine.GetComponent<FSMAbilityBridge>();
        bridge.TriggerAbility("FireMagic");
    }
}
```

---

## 4. Stat 스케일링 연동

### 4.1 PlayerStats 설정

**Unity Editor**:

```
Player GameObject:
- PlayerStats 컴포넌트
  - Base Hp: 100
  - Base Attack: 15
  - Base Defense: 5
```

### 4.2 장비 아이템으로 Attack 증가

**테스트 시나리오**:
```csharp
// 1. 초기 상태
int attack = playerStats.GetStat(StatType.Attack);
// → 15 (기본 Attack)

// 2. FireSword 장착 (Attack +5)
Item fireSword = Resources.Load<Item>("Items/FireSword");
inventorySystem.EquipItem(fireSword);

// 3. 스탯 재계산
attack = playerStats.GetStat(StatType.Attack);
// → 20 (15 + 5)

// 4. Fire Magic 실행 시 데미지
// damage = 20 * 2.5 = 50
await abilitySystem.TryExecuteAbilityAsync("FireMagic");
```

**결과**:
- Fire Magic 데미지가 **37.5 → 50**으로 증가 (Attack 15 → 20)

---

## 5. 테스트 시나리오

### 5.1 단위 테스트: Fire Magic 쿨다운

**위치**: `Assets/_Project/Scripts/Tests/FireMagicAbilityTests.cs`

```csharp
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using GAS.Core;

namespace GASPT.Tests
{
    public class FireMagicAbilityTests
    {
        [UnityTest]
        public IEnumerator FireMagic_Cooldown_WorksCorrectly()
        {
            // Arrange
            var player = new GameObject("Player");
            var context = player.AddComponent<DefaultGameplayContext>();
            var stats = player.AddComponent<PlayerStats>();
            var fireMagic = player.AddComponent<FireMagicAbility>();

            // Act
            bool firstExecution = fireMagic.CanExecute(context);
            yield return fireMagic.ExecuteAsync(context).AsCoroutine();

            // Assert - 쿨다운 중이므로 실행 불가
            bool secondExecution = fireMagic.CanExecute(context);
            Assert.IsFalse(secondExecution, "쿨다운 중에는 실행 불가해야 함");

            // Wait for cooldown
            yield return new WaitForSeconds(7.5f);

            // Assert - 쿨다운 완료 후 실행 가능
            bool thirdExecution = fireMagic.CanExecute(context);
            Assert.IsTrue(thirdExecution, "쿨다운 완료 후 실행 가능해야 함");

            // Cleanup
            Object.Destroy(player);
        }
    }
}
```

---

### 5.2 통합 테스트: Fire Magic + Stat 스케일링

```csharp
[UnityTest]
public IEnumerator FireMagic_DamageScalesWithAttack()
{
    // Arrange
    var player = CreatePlayerWithFireMagic();
    var enemy = CreateEnemy(maxHp: 100);
    var playerStats = player.GetComponent<PlayerStats>();

    // Act 1: Base Attack (15) → 데미지 37.5
    yield return FireMagicAt(player, enemy);

    // Assert 1
    Assert.AreEqual(62.5f, enemy.CurrentHp, "적 HP는 62.5여야 함 (100 - 37.5)");

    // Act 2: FireSword 장착 (Attack +5 → 20) → 데미지 50
    Item fireSword = Resources.Load<Item>("Items/FireSword");
    playerStats.EquipItem(fireSword);

    enemy.SetHp(100); // 적 HP 초기화
    yield return new WaitForSeconds(7.5f); // 쿨다운 대기

    yield return FireMagicAt(player, enemy);

    // Assert 2
    Assert.AreEqual(50f, enemy.CurrentHp, "적 HP는 50이어야 함 (100 - 50)");
}
```

---

### 5.3 E2E 테스트: 전체 플로우

**시나리오**: 플레이어가 상점에서 아이템 구매 → 장착 → Fire Magic으로 적 처치

```csharp
[UnityTest]
public IEnumerator FullGameplayLoop_ShopToCombat()
{
    // 1. 초기 골드 설정
    CurrencySystem.Instance.AddGold(100);

    // 2. 상점에서 FireSword 구매 (80 gold)
    Item fireSword = Resources.Load<Item>("Items/FireSword");
    bool purchased = shopSystem.PurchaseItem(fireSword);
    Assert.IsTrue(purchased, "구매 성공해야 함");
    Assert.AreEqual(20, CurrencySystem.Instance.GetGold(), "골드 20 남아야 함");

    // 3. FireSword 장착
    bool equipped = inventorySystem.EquipItem(fireSword);
    Assert.IsTrue(equipped, "장착 성공해야 함");

    // 4. Attack 스탯 확인
    int attack = playerStats.GetStat(StatType.Attack);
    Assert.AreEqual(20, attack, "Attack은 20이어야 함 (15 + 5)");

    // 5. Fire Magic으로 적 처치
    var enemy = CreateEnemy(maxHp: 50);
    yield return FireMagicAt(player, enemy);

    // Assert - 적 사망 (50 데미지, HP 50 → 0)
    Assert.IsTrue(enemy.IsDead, "적이 사망해야 함");

    // 6. 골드 드랍 확인
    Assert.AreEqual(40, CurrencySystem.Instance.GetGold(), "골드 40이어야 함 (20 + 20 드랍)");
}
```

---

## 6. 문제 해결

### 6.1 "Fire Magic이 실행되지 않음"

**증상**: Q 키를 눌러도 Fire Magic이 발사되지 않음

**원인 및 해결**:

| 원인 | 해결 방법 |
|------|----------|
| 쿨다운 중 | 7초 대기 후 재시도 |
| IsAlive == false | Player 사망 상태 확인 |
| CanAct == false | 스턴/경직 상태 확인 |
| 어빌리티 미등록 | `abilitySystem.RegisterAbility()` 호출 확인 |
| PlayerStats null | FireMagicAbility Inspector에 PlayerStats 할당 |

**디버깅**:
```csharp
private async Awaitable TryFireMagicAsync()
{
    // 1. 등록 확인
    bool hasAbility = abilitySystem.HasAbility("FireMagic");
    Debug.Log($"Fire Magic 등록됨: {hasAbility}");

    // 2. CanExecute 확인
    var ability = abilitySystem.GetAbility("FireMagic");
    bool canExecute = ability.CanExecute(context);
    Debug.Log($"CanExecute: {canExecute}");

    // 3. 쿨다운 확인
    if (ability.Cooldown != null)
    {
        Debug.Log($"Cooldown: {ability.Cooldown.RemainingTime}s");
    }

    // 4. 실행
    bool success = await abilitySystem.TryExecuteAbilityAsync("FireMagic");
    Debug.Log($"실행 결과: {success}");
}
```

---

### 6.2 "스탯 변경이 Fire Magic 데미지에 반영되지 않음"

**증상**: FireSword 장착해도 데미지가 여전히 37.5

**원인 및 해결**:

| 원인 | 해결 방법 |
|------|----------|
| PlayerStats.MarkDirty() 미호출 | EquipItem() 내부에서 자동 호출되는지 확인 |
| 캐싱된 스탯 사용 중 | 더티 플래그 초기화 확인 |
| GetStat() 호출 안 함 | FireMagicAbility.ExecuteAsync()에서 `playerStats.GetStat()` 호출 확인 |

**디버깅**:
```csharp
public override async Awaitable ExecuteAsync(IGameplayContext context)
{
    // 스탯 확인
    int attack = playerStats.GetStat(StatType.Attack);
    Debug.Log($"현재 Attack: {attack}");

    // 데미지 계산
    float damage = attack * data.damageMultiplier;
    Debug.Log($"Fire Magic 데미지: {damage}");

    // ...
}
```

---

### 6.3 "Awaitable 사용 시 컴파일 에러"

**증상**: `'Awaitable' could not be found`

**해결**:
```csharp
// 1. using 추가
using UnityEngine;

// 2. Unity 버전 확인
// Unity 2023.3+ 이상이어야 Awaitable 지원됨

// 3. .NET Standard 2.1 확인
// Project Settings → Player → Other Settings → API Compatibility Level
// → .NET Standard 2.1 선택
```

---

## 7. 다음 단계

### Phase 1 완료
1. ✅ data-model.md
2. ✅ api-contracts.md
3. ✅ quickstart.md

### Phase 2: 구현 시작
1. **IAbility 인터페이스** 작성 (~50 라인)
2. **Ability 베이스 클래스** 작성 (~200 라인)
3. **AbilitySystem** 작성 (~250 라인)
4. **FireMagicAbility** 작성 (~200 라인)
5. **PlayerStats** 작성 (~150 라인)

### Phase 2: Tasks 생성
- `/speckit.tasks` 명령 실행
- P1-P6 user stories를 30-50개 태스크로 분해

---

## 8. 참고 자료

- API Contracts: [api-contracts.md](./api-contracts.md)
- Data Model: [data-model.md](./data-model.md)
- Research: [research.md](./research.md)
- Unity Awaitable 가이드: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Awaitable.html
- Unity Test Framework: https://docs.unity3d.com/Packages/com.unity.test-framework@2.0/manual/index.html

---

**버전**: 1.0
**상태**: Draft - Phase 1
**최종 수정**: 2025-11-01
