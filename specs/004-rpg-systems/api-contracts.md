# API Contracts: RPG Systems with Minimal GAS

**프로젝트**: GASPT - RPG Systems Feature
**브랜치**: `004-rpg-systems`
**작성일**: 2025-11-01
**상태**: Draft - Phase 1

---

## 1. 개요

이 문서는 RPG Systems 구현을 위한 공개 API 계약을 정의합니다.
모든 인터페이스와 공개 메서드의 시그니처, 전제조건, 후제조건을 명시합니다.

### 1.1 계약 규칙

- **async 메서드**: `Awaitable` 반환 (Coroutine 금지)
- **예외 처리**: 예외를 던지지 않고 bool 또는 null 반환
- **이벤트**: C# event 패턴 사용
- **명명 규칙**: camelCase (언더스코어 금지)

---

## 2. GAS Core 인터페이스

### 2.1 IAbility

**위치**: `Assets/Plugins/GAS_Core/Interfaces/IAbility.cs`

```csharp
namespace GAS.Core
{
    /// <summary>
    /// 모든 어빌리티가 구현해야 하는 인터페이스
    /// </summary>
    public interface IAbility
    {
        // ====== 프로퍼티 ======

        /// <summary>
        /// 어빌리티 고유 식별자 (예: "FireMagic")
        /// </summary>
        string AbilityName { get; }

        /// <summary>
        /// 현재 실행 중인지 여부
        /// </summary>
        bool IsExecuting { get; }

        /// <summary>
        /// 쿨다운 관리 객체 (null 가능)
        /// </summary>
        AbilityCooldown Cooldown { get; }


        // ====== 메서드 ======

        /// <summary>
        /// 어빌리티 실행 가능 여부 확인
        /// </summary>
        /// <param name="context">게임플레이 컨텍스트 (소유자, 타겟 등)</param>
        /// <returns>true: 실행 가능, false: 불가능</returns>
        /// <remarks>
        /// 전제조건:
        /// - context != null
        ///
        /// 검증 항목:
        /// - context.IsAlive == true
        /// - context.CanAct == true
        /// - Cooldown == null || Cooldown.CanUse() == true
        /// </remarks>
        bool CanExecute(IGameplayContext context);

        /// <summary>
        /// 어빌리티 비동기 실행
        /// </summary>
        /// <param name="context">게임플레이 컨텍스트</param>
        /// <returns>Awaitable (완료 대기 가능)</returns>
        /// <remarks>
        /// 전제조건:
        /// - CanExecute(context) == true
        /// - IsExecuting == false
        ///
        /// 후제조건:
        /// - IsExecuting == true (실행 중)
        /// - Cooldown.StartCooldown() 호출됨 (쿨다운 있는 경우)
        /// - 실행 완료 후 IsExecuting == false
        ///
        /// 예외:
        /// - 예외를 던지지 않음 (내부에서 처리)
        /// </remarks>
        Awaitable ExecuteAsync(IGameplayContext context);

        /// <summary>
        /// 실행 중인 어빌리티 취소
        /// </summary>
        /// <remarks>
        /// 전제조건:
        /// - IsExecuting == true
        ///
        /// 후제조건:
        /// - IsExecuting == false
        /// - ExecuteAsync() 태스크 종료됨
        /// </remarks>
        void Cancel();
    }
}
```

**사용 예시**:
```csharp
// 1. CanExecute 검증
if (ability.CanExecute(playerContext))
{
    // 2. 실행
    await ability.ExecuteAsync(playerContext);

    // 3. 쿨다운 확인
    if (ability.Cooldown != null)
    {
        Debug.Log($"Cooldown: {ability.Cooldown.RemainingTime}s");
    }
}
```

---

### 2.2 IAbilitySystem

**위치**: `Assets/Plugins/GAS_Core/Interfaces/IAbilitySystem.cs`

```csharp
namespace GAS.Core
{
    /// <summary>
    /// 어빌리티 시스템 관리 인터페이스
    /// </summary>
    public interface IAbilitySystem
    {
        // ====== 이벤트 ======

        /// <summary>
        /// 어빌리티 실행 성공 시 발생
        /// </summary>
        event Action<string> OnAbilityExecuted; // abilityName

        /// <summary>
        /// 어빌리티 실행 실패 시 발생 (CanExecute 실패)
        /// </summary>
        event Action<string> OnAbilityFailed; // abilityName

        /// <summary>
        /// 쿨다운 시작 시 발생
        /// </summary>
        event Action<string, float> OnCooldownStarted; // (abilityName, duration)


        // ====== 생명주기 ======

        /// <summary>
        /// 시스템 초기화
        /// </summary>
        /// <param name="context">소유자의 게임플레이 컨텍스트</param>
        /// <remarks>
        /// 전제조건:
        /// - context != null
        ///
        /// 호출 시점:
        /// - MonoBehaviour.Start() 또는 Awake()
        /// </remarks>
        void Initialize(IGameplayContext context);

        /// <summary>
        /// 매 프레임 업데이트 (쿨다운 갱신)
        /// </summary>
        /// <remarks>
        /// 호출 시점:
        /// - MonoBehaviour.Update()
        /// </remarks>
        void Update();


        // ====== 어빌리티 관리 ======

        /// <summary>
        /// 어빌리티 등록
        /// </summary>
        /// <param name="ability">등록할 어빌리티 인스턴스</param>
        /// <remarks>
        /// 전제조건:
        /// - ability != null
        /// - ability.AbilityName != null && != ""
        ///
        /// 후제조건:
        /// - HasAbility(ability.AbilityName) == true
        ///
        /// 중복 처리:
        /// - 동일 이름이 이미 존재하면 덮어씀 (경고 로그)
        /// </remarks>
        void RegisterAbility(IAbility ability);

        /// <summary>
        /// 어빌리티 등록 해제
        /// </summary>
        /// <param name="abilityName">제거할 어빌리티 이름</param>
        /// <remarks>
        /// 전제조건:
        /// - abilityName != null
        ///
        /// 후제조건:
        /// - HasAbility(abilityName) == false
        ///
        /// 없는 이름 처리:
        /// - 조용히 무시 (로그 없음)
        /// </remarks>
        void UnregisterAbility(string abilityName);

        /// <summary>
        /// 어빌리티 존재 확인
        /// </summary>
        /// <param name="abilityName">확인할 어빌리티 이름</param>
        /// <returns>true: 등록됨, false: 미등록</returns>
        bool HasAbility(string abilityName);

        /// <summary>
        /// 등록된 어빌리티 가져오기
        /// </summary>
        /// <param name="abilityName">가져올 어빌리티 이름</param>
        /// <returns>IAbility 인스턴스 (없으면 null)</returns>
        IAbility GetAbility(string abilityName);


        // ====== 어빌리티 실행 ======

        /// <summary>
        /// 어빌리티 실행 시도 (비동기)
        /// </summary>
        /// <param name="abilityName">실행할 어빌리티 이름</param>
        /// <returns>true: 실행 성공, false: 실행 실패</returns>
        /// <remarks>
        /// 전제조건:
        /// - abilityName != null
        ///
        /// 실행 플로우:
        /// 1. HasAbility(abilityName) 확인 → false면 return false
        /// 2. ability.CanExecute(context) 확인 → false면 OnAbilityFailed 발생, return false
        /// 3. await ability.ExecuteAsync(context) 실행
        /// 4. OnAbilityExecuted 이벤트 발생
        /// 5. return true
        ///
        /// 예외 처리:
        /// - ExecuteAsync() 내부 예외 발생 시 catch하고 false 반환
        /// </remarks>
        Awaitable<bool> TryExecuteAbilityAsync(string abilityName);

        /// <summary>
        /// 모든 실행 중인 어빌리티 취소
        /// </summary>
        /// <remarks>
        /// 사용 시나리오:
        /// - 플레이어 사망 시
        /// - 게임 일시정지 시
        /// - FSM 상태 강제 전환 시
        /// </remarks>
        void CancelAllAbilities();
    }
}
```

**사용 예시**:
```csharp
public class PlayerAbilityController : MonoBehaviour
{
    private IAbilitySystem abilitySystem;

    private void Start()
    {
        // 1. 시스템 초기화
        abilitySystem = AbilitySystem.Instance;
        abilitySystem.Initialize(GetComponent<IGameplayContext>());

        // 2. 이벤트 구독
        abilitySystem.OnAbilityExecuted += OnAbilitySuccess;
        abilitySystem.OnAbilityFailed += OnAbilityFail;
        abilitySystem.OnCooldownStarted += OnCooldownStart;

        // 3. Fire Magic 등록
        var fireMagic = GetComponent<FireMagicAbility>();
        abilitySystem.RegisterAbility(fireMagic);
    }

    private void Update()
    {
        // 4. 쿨다운 업데이트
        abilitySystem.Update();

        // 5. 입력 처리
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _ = TryFireMagicAsync();
        }
    }

    private async Awaitable TryFireMagicAsync()
    {
        // 6. 실행 시도
        bool success = await abilitySystem.TryExecuteAbilityAsync("FireMagic");

        if (!success)
        {
            Debug.Log("Fire Magic 사용 불가!");
        }
    }

    private void OnAbilitySuccess(string name)
    {
        Debug.Log($"{name} 실행 성공!");
    }

    private void OnAbilityFail(string name)
    {
        Debug.Log($"{name} 실행 실패 (쿨다운 또는 조건 불만족)");
    }

    private void OnCooldownStart(string name, float duration)
    {
        Debug.Log($"{name} 쿨다운 시작: {duration}초");
    }
}
```

---

### 2.3 IGameplayContext

**위치**: `Assets/Plugins/GAS_Core/Interfaces/IGameplayContext.cs` (보존됨)

```csharp
namespace GAS.Core
{
    /// <summary>
    /// 게임플레이 컨텍스트 인터페이스 (어빌리티가 게임 상태를 파악)
    /// </summary>
    public interface IGameplayContext
    {
        // ====== 기본 정보 ======

        /// <summary>
        /// 컨텍스트 소유자 GameObject
        /// </summary>
        GameObject Owner { get; }

        /// <summary>
        /// 소유자 Transform
        /// </summary>
        Transform Transform { get; }


        // ====== 상태 정보 ======

        /// <summary>
        /// 생존 여부
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 행동 가능 여부 (스턴, 경직 등으로 불가능할 수 있음)
        /// </summary>
        bool CanAct { get; }


        // ====== 위치/방향 정보 ======

        /// <summary>
        /// 현재 위치
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// 전방 방향 벡터 (발사 방향)
        /// </summary>
        Vector3 Forward { get; }


        // ====== 상태 관리 ======

        /// <summary>
        /// 특정 상태 여부 확인 (예: "Attacking", "Jumping")
        /// </summary>
        /// <param name="stateName">상태 이름</param>
        /// <returns>true: 해당 상태, false: 아님</returns>
        bool IsInState(string stateName);

        /// <summary>
        /// 상태 설정
        /// </summary>
        /// <param name="stateName">상태 이름</param>
        /// <param name="value">true: 진입, false: 퇴장</param>
        void SetState(string stateName, bool value);


        // ====== 타겟팅 ======

        /// <summary>
        /// 현재 타겟 가져오기 (null 가능)
        /// </summary>
        /// <returns>타겟 Transform (없으면 null)</returns>
        Transform GetTarget();

        /// <summary>
        /// 타겟 설정
        /// </summary>
        /// <param name="target">타겟 Transform (null 허용)</param>
        void SetTarget(Transform target);


        // ====== 커스텀 데이터 ======

        /// <summary>
        /// 커스텀 데이터 가져오기 (타입 안전)
        /// </summary>
        /// <typeparam name="T">데이터 타입</typeparam>
        /// <param name="key">데이터 키</param>
        /// <returns>데이터 (없으면 null)</returns>
        T GetCustomData<T>(string key) where T : class;

        /// <summary>
        /// 커스텀 데이터 설정
        /// </summary>
        /// <typeparam name="T">데이터 타입</typeparam>
        /// <param name="key">데이터 키</param>
        /// <param name="data">데이터</param>
        void SetCustomData<T>(string key, T data) where T : class;
    }
}
```

**사용 예시**:
```csharp
public class FireMagicAbility : Ability
{
    public override bool CanExecute(IGameplayContext context)
    {
        // 1. 생존 확인
        if (!context.IsAlive) return false;

        // 2. 행동 가능 확인
        if (!context.CanAct) return false;

        // 3. 쿨다운 확인
        if (Cooldown != null && !Cooldown.CanUse()) return false;

        // 4. 커스텀 조건 (예: 마나)
        var stats = context.GetCustomData<PlayerStats>("Stats");
        if (stats != null && stats.GetStat(StatType.MP) < 10)
            return false;

        return true;
    }

    public override async Awaitable ExecuteAsync(IGameplayContext context)
    {
        // 1. 발사 위치/방향 가져오기
        Vector3 spawnPos = context.Position + context.Forward * 0.5f;
        Vector3 direction = context.Forward;

        // 2. 발사체 생성
        GameObject projectile = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);

        // 3. 비행 처리
        await FireProjectileAsync(projectile, direction);
    }
}
```

---

## 3. RPG Systems 공개 API

### 3.1 PlayerStats

**위치**: `Assets/_Project/Scripts/Stats/PlayerStats.cs`

```csharp
namespace GASPT.Stats
{
    /// <summary>
    /// 플레이어 스탯 관리 (HP, Attack, Defense)
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        // ====== 이벤트 ======

        /// <summary>
        /// 스탯 변경 시 발생
        /// </summary>
        /// <param name="type">변경된 스탯 타입</param>
        /// <param name="oldValue">이전 값</param>
        /// <param name="newValue">새 값</param>
        public event Action<StatType, int, int> OnStatChanged;


        // ====== 공개 메서드 ======

        /// <summary>
        /// 특정 스탯 값 가져오기 (최종 값 = 기본 + 장비 보너스)
        /// </summary>
        /// <param name="type">스탯 타입 (HP, Attack, Defense)</param>
        /// <returns>최종 스탯 값</returns>
        /// <remarks>
        /// 성능:
        /// - 더티 플래그 사용: 변경 시에만 재계산
        /// - 목표: <50ms (SC-001 요구사항)
        /// </remarks>
        public int GetStat(StatType type);

        /// <summary>
        /// 기본 스탯 값 가져오기 (장비 보너스 제외)
        /// </summary>
        /// <param name="type">스탯 타입</param>
        /// <returns>기본 스탯 값</returns>
        public int GetBaseStat(StatType type);

        /// <summary>
        /// 아이템 장착 (스탯 보너스 적용)
        /// </summary>
        /// <param name="item">장착할 아이템</param>
        /// <remarks>
        /// 전제조건:
        /// - item != null
        ///
        /// 후제조건:
        /// - 스탯 재계산됨
        /// - OnStatChanged 이벤트 발생 (변경된 스탯마다)
        /// </remarks>
        public void EquipItem(Item item);

        /// <summary>
        /// 아이템 해제 (스탯 보너스 제거)
        /// </summary>
        /// <param name="item">해제할 아이템</param>
        /// <remarks>
        /// 전제조건:
        /// - item != null
        /// - 해당 아이템이 장착되어 있어야 함
        ///
        /// 후제조건:
        /// - 스탯 재계산됨
        /// - OnStatChanged 이벤트 발생
        /// </remarks>
        public void UnequipItem(Item item);

        /// <summary>
        /// 모든 장착 아이템 가져오기
        /// </summary>
        /// <returns>장착 아이템 리스트 (읽기 전용)</returns>
        public IReadOnlyList<Item> GetEquippedItems();

        /// <summary>
        /// 스탯 더티 플래그 설정 (다음 GetStat() 시 재계산)
        /// </summary>
        /// <remarks>
        /// 내부 사용:
        /// - EquipItem(), UnequipItem() 호출 시 자동 호출
        /// </remarks>
        public void MarkDirty();
    }
}
```

**사용 예시**:
```csharp
public class FireMagicAbility : Ability
{
    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    public override async Awaitable ExecuteAsync(IGameplayContext context)
    {
        // 1. 현재 Attack 스탯 가져오기
        int attackStat = playerStats.GetStat(StatType.Attack);

        // 2. 데미지 계산 (2.5x Attack)
        float damage = attackStat * data.damageMultiplier;

        // 3. 발사체 생성 및 데미지 적용
        await FireProjectileAsync(damage);
    }
}
```

---

### 3.2 CurrencySystem

**위치**: `Assets/_Project/Scripts/Economy/CurrencySystem.cs`

```csharp
namespace GASPT.Economy
{
    /// <summary>
    /// 골드 화폐 시스템 (싱글톤)
    /// </summary>
    public class CurrencySystem : SingletonManager<CurrencySystem>
    {
        // ====== 이벤트 ======

        /// <summary>
        /// 골드 변경 시 발생
        /// </summary>
        /// <param name="oldAmount">이전 골드</param>
        /// <param name="newAmount">새 골드</param>
        public event Action<int, int> OnGoldChanged;


        // ====== 공개 메서드 ======

        /// <summary>
        /// 현재 골드 가져오기
        /// </summary>
        /// <returns>골드 양 (>= 0)</returns>
        public int GetGold();

        /// <summary>
        /// 골드 추가 (적 처치 보상 등)
        /// </summary>
        /// <param name="amount">추가할 골드 (> 0)</param>
        /// <remarks>
        /// 전제조건:
        /// - amount > 0
        ///
        /// 후제조건:
        /// - currentGold += amount
        /// - OnGoldChanged 이벤트 발생
        ///
        /// 유효성:
        /// - amount <= 0이면 무시 (경고 로그)
        /// </remarks>
        public void AddGold(int amount);

        /// <summary>
        /// 골드 소비 시도 (상점 구매 등)
        /// </summary>
        /// <param name="amount">소비할 골드 (> 0)</param>
        /// <returns>true: 소비 성공, false: 골드 부족</returns>
        /// <remarks>
        /// 전제조건:
        /// - amount > 0
        ///
        /// 성공 시:
        /// - currentGold -= amount
        /// - OnGoldChanged 이벤트 발생
        /// - return true
        ///
        /// 실패 시:
        /// - currentGold < amount인 경우
        /// - 골드 변경 없음
        /// - 이벤트 발생 없음
        /// - return false
        /// </remarks>
        public bool TrySpendGold(int amount);

        /// <summary>
        /// 골드 직접 설정 (세이브 로드 전용)
        /// </summary>
        /// <param name="amount">설정할 골드 (>= 0)</param>
        /// <remarks>
        /// 경고: 게임플레이 중 사용 금지 (AddGold/TrySpendGold 사용)
        ///
        /// 사용 시나리오:
        /// - SaveLoadManager.LoadAsync() 에서만 호출
        /// </remarks>
        public void SetGold(int amount);
    }
}
```

**사용 예시**:
```csharp
public class ShopSystem : MonoBehaviour
{
    public void PurchaseItem(Item item)
    {
        // 1. 골드 소비 시도
        bool success = CurrencySystem.Instance.TrySpendGold(item.goldPrice);

        if (success)
        {
            // 2. 아이템 지급
            InventorySystem.Instance.AddItem(item);
            Debug.Log($"{item.itemName} 구매 성공!");
        }
        else
        {
            // 3. 구매 실패 (골드 부족)
            Debug.Log("골드가 부족합니다!");
            ShowInsufficientGoldMessage();
        }
    }
}

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;

    private void Die()
    {
        // 골드 드랍
        int goldAmount = Random.Range(data.minGoldDrop, data.maxGoldDrop + 1);
        CurrencySystem.Instance.AddGold(goldAmount);

        Debug.Log($"{data.enemyName} 처치! 골드 +{goldAmount}");
    }
}
```

---

### 3.3 InventorySystem

**위치**: `Assets/_Project/Scripts/Inventory/InventorySystem.cs`

```csharp
namespace GASPT.Inventory
{
    /// <summary>
    /// 인벤토리 및 장비 관리 시스템 (싱글톤)
    /// </summary>
    public class InventorySystem : SingletonManager<InventorySystem>
    {
        // ====== 이벤트 ======

        /// <summary>
        /// 아이템 추가 시 발생
        /// </summary>
        public event Action<Item> OnItemAdded;

        /// <summary>
        /// 아이템 제거 시 발생
        /// </summary>
        public event Action<Item> OnItemRemoved;

        /// <summary>
        /// 아이템 장착 시 발생
        /// </summary>
        public event Action<Item> OnItemEquipped;

        /// <summary>
        /// 아이템 해제 시 발생
        /// </summary>
        public event Action<Item> OnItemUnequipped;


        // ====== 공개 메서드 ======

        /// <summary>
        /// 시스템 초기화
        /// </summary>
        /// <param name="playerStats">플레이어 스탯 참조 (장비 보너스 적용용)</param>
        public void Initialize(PlayerStats playerStats);

        /// <summary>
        /// 아이템 추가 (상점 구매, 드랍 습득)
        /// </summary>
        /// <param name="item">추가할 아이템</param>
        /// <remarks>
        /// 전제조건:
        /// - item != null
        ///
        /// 후제조건:
        /// - 인벤토리에 아이템 추가됨
        /// - OnItemAdded 이벤트 발생
        ///
        /// 인벤토리 제한:
        /// - 무제한 슬롯 (MVP, FR-011)
        /// </remarks>
        public void AddItem(Item item);

        /// <summary>
        /// 아이템 제거 (아이템 판매 등, MVP에서는 미사용)
        /// </summary>
        /// <param name="item">제거할 아이템</param>
        /// <returns>true: 제거 성공, false: 아이템 없음</returns>
        public bool RemoveItem(Item item);

        /// <summary>
        /// 아이템 소유 여부 확인
        /// </summary>
        /// <param name="item">확인할 아이템</param>
        /// <returns>true: 소유 중, false: 미소유</returns>
        public bool HasItem(Item item);

        /// <summary>
        /// 모든 소유 아이템 가져오기
        /// </summary>
        /// <returns>아이템 리스트 (읽기 전용)</returns>
        public IReadOnlyList<Item> GetAllItems();

        /// <summary>
        /// 아이템 장착 (장비 슬롯에 할당 + 스탯 보너스 적용)
        /// </summary>
        /// <param name="item">장착할 아이템</param>
        /// <returns>true: 장착 성공, false: 이미 장착됨 또는 소유하지 않음</returns>
        /// <remarks>
        /// 전제조건:
        /// - HasItem(item) == true
        /// - item이 아직 장착되지 않음
        ///
        /// 후제조건:
        /// - 장비 슬롯에 아이템 할당됨
        /// - PlayerStats.EquipItem(item) 호출됨
        /// - OnItemEquipped 이벤트 발생
        ///
        /// 슬롯 충돌:
        /// - 동일 슬롯에 이미 아이템이 있으면 자동 해제 후 장착
        /// </remarks>
        public bool EquipItem(Item item);

        /// <summary>
        /// 아이템 해제
        /// </summary>
        /// <param name="item">해제할 아이템</param>
        /// <returns>true: 해제 성공, false: 장착되지 않음</returns>
        /// <remarks>
        /// 전제조건:
        /// - item이 현재 장착되어 있어야 함
        ///
        /// 후제조건:
        /// - 장비 슬롯에서 제거됨
        /// - PlayerStats.UnequipItem(item) 호출됨
        /// - OnItemUnequipped 이벤트 발생
        /// </remarks>
        public bool UnequipItem(Item item);

        /// <summary>
        /// 특정 슬롯에 장착된 아이템 가져오기
        /// </summary>
        /// <param name="slot">장비 슬롯 (Weapon, Armor, Accessory)</param>
        /// <returns>장착된 아이템 (없으면 null)</returns>
        public Item GetEquippedItem(EquipmentSlot slot);

        /// <summary>
        /// 모든 장착 아이템 가져오기
        /// </summary>
        /// <returns>슬롯-아이템 딕셔너리 (읽기 전용)</returns>
        public IReadOnlyDictionary<EquipmentSlot, Item> GetAllEquippedItems();
    }
}
```

**사용 예시**:
```csharp
public class InventoryUI : MonoBehaviour
{
    private InventorySystem inventory;

    private void Start()
    {
        inventory = InventorySystem.Instance;

        // 이벤트 구독
        inventory.OnItemAdded += OnItemAddedHandler;
        inventory.OnItemEquipped += OnItemEquippedHandler;
    }

    public void OnEquipButtonClicked(Item item)
    {
        // 아이템 장착
        bool success = inventory.EquipItem(item);

        if (success)
        {
            Debug.Log($"{item.itemName} 장착 완료!");
            RefreshUI();
        }
    }

    private void OnItemAddedHandler(Item item)
    {
        Debug.Log($"새 아이템 획득: {item.itemName}");
        RefreshUI();
    }

    private void OnItemEquippedHandler(Item item)
    {
        Debug.Log($"{item.itemName} 장착됨!");
        // 장비 슬롯 UI 업데이트
    }
}
```

---

### 3.4 SaveLoadManager

**위치**: `Assets/_Project/Scripts/Save/SaveLoadManager.cs`

```csharp
namespace GASPT.Save
{
    /// <summary>
    /// 세이브/로드 시스템 (싱글톤)
    /// </summary>
    public class SaveLoadManager : SingletonManager<SaveLoadManager>
    {
        // ====== 공개 메서드 ======

        /// <summary>
        /// 현재 게임 상태를 파일에 비동기 저장
        /// </summary>
        /// <returns>Awaitable (완료 대기 가능)</returns>
        /// <remarks>
        /// 저장 위치:
        /// - Application.persistentDataPath/savedata.json
        ///
        /// 성능:
        /// - 목표: <2초 (SC-010, FR-043)
        /// - JSON 크기: <100KB
        ///
        /// 저장 내용:
        /// - PlayerStats (HP, Attack, Defense)
        /// - Inventory (소유/장착 아이템)
        /// - Currency (골드)
        /// - Progress (체크포인트, 보스 처치 여부)
        ///
        /// 예외 처리:
        /// - 저장 실패 시 에러 로그 출력
        /// - 사용자에게 실패 메시지 표시 (콜백)
        /// </remarks>
        public async Awaitable SaveAsync();

        /// <summary>
        /// 파일에서 게임 상태 비동기 로드
        /// </summary>
        /// <returns>true: 로드 성공, false: 세이브 파일 없음</returns>
        /// <remarks>
        /// 로드 위치:
        /// - Application.persistentDataPath/savedata.json
        ///
        /// 성능:
        /// - 목표: <2초 (SC-010)
        ///
        /// 로드 후 처리:
        /// - PlayerStats.SetStat() 호출
        /// - InventorySystem.AddItem() + EquipItem() 호출
        /// - CurrencySystem.SetGold() 호출
        /// - 플레이어 위치 이동
        ///
        /// 파일 없음:
        /// - return false
        /// - 새 게임 시작 (FR-047)
        ///
        /// 파일 손상:
        /// - 에러 로그 출력
        /// - return false
        /// - 새 게임 시작 또는 사용자 선택
        /// </remarks>
        public async Awaitable<bool> LoadAsync();

        /// <summary>
        /// 세이브 파일 존재 여부 확인
        /// </summary>
        /// <returns>true: 파일 있음, false: 없음</returns>
        public bool HasSaveFile();

        /// <summary>
        /// 세이브 파일 삭제
        /// </summary>
        /// <remarks>
        /// 사용 시나리오:
        /// - "새 게임" 시작 시
        /// - 세이브 슬롯 삭제 (향후 기능)
        /// </remarks>
        public void DeleteSaveFile();

        /// <summary>
        /// 자동 저장 (주요 이벤트 시 자동 호출)
        /// </summary>
        /// <remarks>
        /// 호출 시점:
        /// - 레벨 완료 시
        /// - 보스 처치 시
        /// - 체크포인트 도달 시
        ///
        /// FR-045: 자동 저장 요구사항
        /// </remarks>
        public async Awaitable AutoSaveAsync();
    }
}
```

**사용 예시**:
```csharp
public class GameManager : MonoBehaviour
{
    private SaveLoadManager saveLoad;

    private async void Start()
    {
        saveLoad = SaveLoadManager.Instance;

        // 세이브 파일 존재 확인
        if (saveLoad.HasSaveFile())
        {
            // 로드
            bool success = await saveLoad.LoadAsync();

            if (success)
            {
                Debug.Log("게임 로드 완료!");
            }
            else
            {
                Debug.Log("로드 실패, 새 게임 시작");
                StartNewGame();
            }
        }
        else
        {
            Debug.Log("세이브 파일 없음, 새 게임 시작");
            StartNewGame();
        }
    }

    public async void OnSaveButtonClicked()
    {
        // 수동 저장
        await saveLoad.SaveAsync();
        Debug.Log("게임 저장 완료!");
    }

    private void OnBossDefeated()
    {
        // 자동 저장
        _ = saveLoad.AutoSaveAsync();
    }
}
```

---

## 4. 다음 단계

### Phase 1 계속
1. ✅ data-model.md 완료
2. ✅ api-contracts.md 완료
3. ⏳ quickstart.md 작성 (실제 사용 예제 + 테스트 시나리오)

### Phase 2 준비
- API 계약 기반으로 실제 구현 시작
- 우선순위: IAbility → Ability → AbilitySystem → FireMagicAbility

---

## 5. 참고 자료

- Data Model: [data-model.md](./data-model.md)
- Research: [research.md](./research.md)
- Spec: [spec.md](./spec.md)
- Unity Awaitable: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Awaitable.html
- C# Events: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/events/

---

**버전**: 1.0
**상태**: Draft - Phase 1
**최종 수정**: 2025-11-01
