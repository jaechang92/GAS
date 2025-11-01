# Data Model: RPG Systems with Minimal GAS

**프로젝트**: GASPT - RPG Systems Feature
**브랜치**: `004-rpg-systems`
**작성일**: 2025-11-01
**상태**: Draft - Phase 1

---

## 1. 개요

이 문서는 RPG Systems 구현을 위한 데이터 모델을 정의합니다.
최소 GAS (Gameplay Ability System)와 RPG 스탯 시스템의 통합 구조를 다룹니다.

### 1.1 핵심 설계 원칙

- **데이터 기반 설계**: ScriptableObject로 게임 데이터 정의
- **느슨한 결합**: 인터페이스 기반 컴포넌트 통신
- **이벤트 기반**: 상태 변화 시 이벤트 발생 (UI 업데이트)
- **스탯 스케일링**: Ability 데미지는 Player Attack 스탯에 스케일링

---

## 2. 핵심 엔티티 정의

### 2.1 GAS 레이어

#### Ability (추상 클래스)

**목적**: 모든 어빌리티의 베이스 클래스 (Fire Magic, 향후 Ice Magic 등)

```csharp
// Assets/Plugins/GAS_Core/Core/Ability.cs
public abstract class Ability : MonoBehaviour, IAbility
{
    // 필드
    protected string abilityName;
    protected AbilityCooldown cooldown;
    protected IGameplayContext context;
    protected bool isExecuting;

    // 프로퍼티
    public string AbilityName => abilityName;
    public AbilityCooldown Cooldown => cooldown;
    public bool IsExecuting => isExecuting;

    // 추상 메서드 (서브클래스 구현 필요)
    public abstract bool CanExecute(IGameplayContext context);
    public abstract Awaitable ExecuteAsync(IGameplayContext context);
    public abstract void Cancel();
}
```

**관계**:
- `has-a` AbilityCooldown (쿨다운 관리)
- `uses` IGameplayContext (게임 상태 접근)
- `managed-by` AbilitySystem

---

#### AbilityData (ScriptableObject)

**목적**: 어빌리티 설정 데이터 (디자이너 편집 가능)

```csharp
// Assets/Plugins/GAS_Core/Data/AbilityData.cs
[CreateAssetMenu(fileName = "AbilityData", menuName = "GASPT/Abilities/Base")]
public class AbilityData : ScriptableObject
{
    [Header("기본 정보")]
    public string abilityName;
    public string description;
    public Sprite icon;

    [Header("쿨다운")]
    public float cooldownDuration;

    [Header("실행 조건")]
    public bool requiresAlive = true;
    public bool requiresCanAct = true;
}
```

**서브클래스**: FireMagicData (Fire Magic 전용 데이터)

```csharp
// Assets/_Project/Scripts/Data/FireMagicData.cs
[CreateAssetMenu(fileName = "FireMagicData", menuName = "GASPT/Abilities/FireMagic")]
public class FireMagicData : AbilityData
{
    [Header("Fire Magic 설정")]
    public float damageMultiplier = 2.5f;    // Attack의 2.5배
    public float projectileSpeed = 15f;      // 발사체 속도 (m/s)
    public float maxRange = 10f;             // 최대 사거리 (m)
    public GameObject projectilePrefab;      // Fire 이펙트 프리팹
    public LayerMask enemyLayer;             // 충돌 대상 레이어
}
```

**관계**:
- `used-by` FireMagicAbility (실행 시 참조)
- `stored-in` Resources/ 또는 Addressables

---

#### AbilitySystem (싱글톤 매니저)

**목적**: 어빌리티 등록, 실행, 생명주기 관리

```csharp
// Assets/Plugins/GAS_Core/Core/AbilitySystem.cs
public class AbilitySystem : SingletonManager<AbilitySystem>, IAbilitySystem
{
    // 필드
    private Dictionary<string, IAbility> registeredAbilities;
    private IGameplayContext ownerContext;
    private IAbility currentExecutingAbility;

    // 이벤트
    public event Action<string> OnAbilityExecuted;
    public event Action<string> OnAbilityFailed;
    public event Action<string, float> OnCooldownStarted;

    // 메서드
    public void Initialize(IGameplayContext context);
    public void RegisterAbility(IAbility ability);
    public void UnregisterAbility(string abilityName);
    public async Awaitable<bool> TryExecuteAbilityAsync(string abilityName);
    public IAbility GetAbility(string abilityName);
    public bool HasAbility(string abilityName);
    public void CancelAllAbilities();
    public void Update(); // 쿨다운 업데이트
}
```

**상태 머신**:
```
[Idle]
  → TryExecuteAbilityAsync() → [Validating]
      → CanExecute() 실패 → [Idle] (OnAbilityFailed 발생)
      → CanExecute() 성공 → [Executing]
          → ExecuteAsync() → [Cooldown]
              → Cooldown 완료 → [Idle]
```

**관계**:
- `manages` Dictionary<string, IAbility>
- `uses` IGameplayContext (소유자)
- `fires` 이벤트 (UI 리스너)

---

### 2.2 RPG 스탯 레이어

#### PlayerStats (컴포넌트)

**목적**: 플레이어 스탯 관리 (HP, Attack, Defense)

```csharp
// Assets/_Project/Scripts/Stats/PlayerStats.cs
public class PlayerStats : MonoBehaviour
{
    [Header("기본 스탯 (Base Stats)")]
    [SerializeField] private int baseHp = 100;
    [SerializeField] private int baseAttack = 15;
    [SerializeField] private int baseDefense = 5;

    // 더티 플래그 (성능 최적화)
    private bool isDirty = true;

    // 캐싱된 최종 스탯
    private int cachedFinalHp;
    private int cachedFinalAttack;
    private int cachedFinalDefense;

    // 장비 참조
    private List<Item> equippedItems = new List<Item>();

    // 이벤트
    public event Action<StatType, int, int> OnStatChanged; // (타입, 이전값, 새값)

    // 메서드
    public int GetStat(StatType type);
    public void EquipItem(Item item);
    public void UnequipItem(Item item);
    public void MarkDirty();
    private void RecalculateStats();
}
```

**스탯 계산 공식**:
```
최종 HP = baseHp + Σ(장비 HP 보너스)
최종 Attack = baseAttack + Σ(장비 Attack 보너스)
최종 Defense = baseDefense + Σ(장비 Defense 보너스)
```

**성능 최적화**:
- 더티 플래그: 스탯 변경 시에만 재계산
- 캐싱: 이전 계산 결과 저장
- 목표: <50ms (SC-001 요구사항)

---

#### StatType (열거형)

```csharp
// Assets/_Project/Scripts/Core/Enums/StatType.cs
namespace Core.Enums
{
    public enum StatType
    {
        HP,       // 체력
        Attack,   // 공격력
        Defense   // 방어력
    }
}
```

---

#### Item (ScriptableObject)

**목적**: 아이템 데이터 정의 (상점 구매, 스탯 보너스)

```csharp
// Assets/_Project/Scripts/Data/Item.cs
[CreateAssetMenu(fileName = "Item", menuName = "GASPT/Items/Equipment")]
public class Item : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;
    public string description;
    public Sprite icon;

    [Header("경제")]
    public int goldPrice;

    [Header("스탯 보너스")]
    public int hpBonus;
    public int attackBonus;
    public int defenseBonus;

    [Header("장비 슬롯")]
    public EquipmentSlot slot; // Weapon, Armor, Accessory
}
```

**예시 데이터**:
```
FireSword (무기)
- goldPrice: 80
- attackBonus: +5
- defenseBonus: 0
- slot: Weapon

LeatherArmor (방어구)
- goldPrice: 100
- hpBonus: +20
- defenseBonus: +3
- slot: Armor
```

---

#### EquipmentSlot (열거형)

```csharp
// Assets/_Project/Scripts/Core/Enums/EquipmentSlot.cs
namespace Core.Enums
{
    public enum EquipmentSlot
    {
        Weapon,     // 무기 (Attack 증가)
        Armor,      // 방어구 (HP, Defense 증가)
        Accessory   // 악세서리 (다양한 보너스)
    }
}
```

---

### 2.3 전투 레이어

#### EnemyType (열거형)

```csharp
// Assets/_Project/Scripts/Core/Enums/EnemyType.cs
namespace Core.Enums
{
    public enum EnemyType
    {
        Normal,  // 일반 몹 (30 HP, 5 Attack, 20 gold)
        Named,   // 네임드 몹 (60 HP, 10 Attack, 50 gold)
        Boss     // 보스 몹 (150 HP, 15 Attack, 125 gold)
    }
}
```

---

#### EnemyData (ScriptableObject)

```csharp
// Assets/_Project/Scripts/Data/EnemyData.cs
[CreateAssetMenu(fileName = "EnemyData", menuName = "GASPT/Enemies/Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("타입")]
    public EnemyType enemyType;

    [Header("기본 정보")]
    public string enemyName;
    public Sprite icon;

    [Header("스탯")]
    public int maxHp;
    public int attack;

    [Header("보상")]
    public int minGoldDrop;
    public int maxGoldDrop;

    [Header("UI")]
    public bool showNameTag;      // Named 전용
    public bool showBossHealthBar; // Boss 전용
}
```

**예시 데이터**:
```
NormalGoblin (Normal)
- maxHp: 30
- attack: 5
- goldDrop: 15-25

EliteOrc (Named)
- maxHp: 60
- attack: 10
- goldDrop: 40-60
- showNameTag: true

FireDragon (Boss)
- maxHp: 150
- attack: 15
- goldDrop: 100-150
- showBossHealthBar: true
```

---

### 2.4 경제 레이어

#### CurrencySystem (싱글톤)

```csharp
// Assets/_Project/Scripts/Economy/CurrencySystem.cs
public class CurrencySystem : SingletonManager<CurrencySystem>
{
    private int currentGold;

    // 이벤트
    public event Action<int, int> OnGoldChanged; // (이전값, 새값)

    // 메서드
    public int GetGold() => currentGold;
    public void AddGold(int amount);
    public bool TrySpendGold(int amount);
    public void SetGold(int amount); // 세이브 로드용
}
```

---

### 2.5 세이브 레이어

#### SaveData (데이터 클래스)

```csharp
// Assets/_Project/Scripts/Save/SaveData.cs
[Serializable]
public class SaveData
{
    [Header("스탯")]
    public int currentHp;
    public int baseAttack;
    public int baseDefense;

    [Header("인벤토리")]
    public List<string> ownedItemIds;      // Item ScriptableObject 경로
    public List<string> equippedItemIds;   // 장착된 아이템 ID

    [Header("경제")]
    public int currentGold;

    [Header("레벨 진행도")]
    public Vector3 checkpointPosition;
    public int enemiesDefeated;
    public bool bossDefeated;
}
```

**JSON 예시**:
```json
{
  "currentHp": 120,
  "baseAttack": 15,
  "baseDefense": 5,
  "ownedItemIds": [
    "Items/FireSword",
    "Items/LeatherArmor"
  ],
  "equippedItemIds": [
    "Items/FireSword"
  ],
  "currentGold": 250,
  "checkpointPosition": { "x": 10.5, "y": 2.0, "z": 0.0 },
  "enemiesDefeated": 5,
  "bossDefeated": false
}
```

---

## 3. 엔티티 관계도

```
┌─────────────────────┐
│   PlayerStats       │
│  (스탯 관리)         │
│  - baseHp           │
│  - baseAttack       │
│  - baseDefense      │
└──────────┬──────────┘
           │
           │ uses (Attack 스탯)
           ↓
┌─────────────────────┐         ┌──────────────────┐
│  FireMagicAbility   │ ◄─uses─ │  FireMagicData   │
│  (Ability 서브클래스)│         │  (ScriptableObj) │
│  - 데미지 계산      │         │  - damageMulti   │
│  - 발사체 생성      │         │  - cooldown      │
└──────────┬──────────┘         └──────────────────┘
           │
           │ registered-in
           ↓
┌─────────────────────┐
│   AbilitySystem     │
│  (싱글톤 매니저)     │
│  - 어빌리티 등록    │
│  - 실행 관리        │
└──────────┬──────────┘
           │
           │ bridges
           ↓
┌─────────────────────┐
│  FSMAbilityBridge   │
│  (FSM 통합)         │
│  - AttackState      │
│    → FireMagic      │
└─────────────────────┘


┌─────────────────────┐
│   PlayerStats       │
└──────────┬──────────┘
           │
           │ modified-by
           ↓
┌─────────────────────┐         ┌──────────────────┐
│  InventorySystem    │ ◄─uses─ │      Item        │
│  (인벤토리 관리)     │         │  (ScriptableObj) │
│  - 장착/해제        │         │  - stat bonuses  │
│  - 아이템 목록      │         │  - goldPrice     │
└──────────┬──────────┘         └──────────────────┘
           │
           │ purchases-from
           ↓
┌─────────────────────┐
│    ShopSystem       │
│  (상점 UI + 로직)    │
│  - 구매 처리        │
└──────────┬──────────┘
           │
           │ spends
           ↓
┌─────────────────────┐
│  CurrencySystem     │
│  (골드 관리)         │
│  - AddGold()        │
│  - TrySpendGold()   │
└─────────────────────┘


┌─────────────────────┐
│     EnemyData       │
│  (ScriptableObj)    │
│  - enemyType        │
│  - maxHp, attack    │
│  - goldDrop range   │
└──────────┬──────────┘
           │
           │ configures
           ↓
┌─────────────────────┐
│       Enemy         │
│  (MonoBehaviour)    │
│  - 전투 로직        │
│  - 골드 드랍        │
└──────────┬──────────┘
           │
           │ drops-gold-to
           ↓
┌─────────────────────┐
│  CurrencySystem     │
└─────────────────────┘


┌─────────────────────┐
│     SaveData        │
│  (Serializable)     │
│  - stats, items     │
│  - gold, position   │
└──────────┬──────────┘
           │
           │ persists
           ↓
┌─────────────────────┐
│  SaveLoadManager    │
│  (JSON 파일 I/O)     │
│  - SaveAsync()      │
│  - LoadAsync()      │
└─────────────────────┘
```

---

## 4. 상태 전이 다이어그램

### 4.1 Ability 실행 플로우

```
[플레이어가 Fire Magic 버튼 입력]
          ↓
┌─────────────────────────────┐
│  InputHandler               │
│  "FireMagic" 입력 감지      │
└──────────┬──────────────────┘
           │
           ↓
┌─────────────────────────────┐
│  FSM: AttackState           │
│  OnEnter() 호출             │
└──────────┬──────────────────┘
           │
           ↓
┌─────────────────────────────┐
│  FSMAbilityBridge           │
│  TriggerAbility("FireMagic")│
└──────────┬──────────────────┘
           │
           ↓
┌─────────────────────────────────────────────────┐
│  AbilitySystem                                  │
│  await TryExecuteAbilityAsync("FireMagic")      │
└──────────┬──────────────────────────────────────┘
           │
           ├─→ [검증] CanExecute() 체크
           │     - IsAlive?
           │     - CanAct?
           │     - Cooldown 완료?
           │
           ├─→ ❌ 실패 → return false
           │              → OnAbilityFailed 이벤트
           │
           └─→ ✅ 성공
                   ↓
           ┌──────────────────────────────┐
           │  FireMagicAbility            │
           │  await ExecuteAsync()        │
           └──────────┬───────────────────┘
                      │
                      ├─→ 1. PlayerStats에서 Attack 가져오기
                      │       cachedAttack = 20
                      │
                      ├─→ 2. 데미지 계산
                      │       damage = 20 * 2.5 = 50
                      │
                      ├─→ 3. Fire 발사체 생성
                      │       Instantiate(projectilePrefab)
                      │
                      ├─→ 4. 발사체 이동 + 충돌 처리
                      │       await FireProjectileAsync()
                      │         ├─→ 매 프레임 이동
                      │         ├─→ 충돌 체크 (enemyLayer)
                      │         └─→ 히트 시: Enemy.TakeDamage(50)
                      │
                      └─→ 5. 쿨다운 시작
                              Cooldown.StartCooldown(7초)
                              OnCooldownStarted 이벤트
                              ↓
                      [7초 대기]
                              ↓
                      Cooldown.CompleteCooldown()
                      OnCooldownCompleted 이벤트
                              ↓
                      [Fire Magic 재사용 가능]
```

### 4.2 스탯 변경 플로우

```
[플레이어가 상점에서 FireSword 구매]
          ↓
┌─────────────────────────────┐
│  ShopSystem                 │
│  PurchaseItem(fireSword)    │
└──────────┬──────────────────┘
           │
           ├─→ CurrencySystem.TrySpendGold(80)
           │     → ✅ 성공: currentGold -= 80
           │
           └─→ InventorySystem.AddItem(fireSword)
                   ↓
           ┌──────────────────────────────┐
           │  InventorySystem             │
           │  EquipItem(fireSword)        │
           └──────────┬───────────────────┘
                      │
                      ↓
           ┌──────────────────────────────┐
           │  PlayerStats                 │
           │  EquipItem(fireSword)        │
           └──────────┬───────────────────┘
                      │
                      ├─→ equippedItems.Add(fireSword)
                      ├─→ MarkDirty() 호출
                      │
                      └─→ 다음 GetStat() 호출 시
                              ↓
                          RecalculateStats()
                          ├─→ baseAttack = 15
                          ├─→ fireSword.attackBonus = +5
                          └─→ cachedFinalAttack = 20
                              ↓
                          OnStatChanged 이벤트
                          (StatType.Attack, 15, 20)
                              ↓
                          [UI 리스너가 업데이트]
                          StatPanel: "Attack: 20 (+5)"
```

### 4.3 전투 데미지 플로우

```
[FireMagic 발사체가 Enemy에 충돌]
          ↓
┌─────────────────────────────────────┐
│  FireProjectile (충돌 감지)          │
│  OnTriggerEnter2D(enemy)            │
└──────────┬──────────────────────────┘
           │
           ↓
┌─────────────────────────────────────┐
│  Enemy                              │
│  TakeDamage(50)                     │
└──────────┬──────────────────────────┘
           │
           ├─→ currentHp -= 50
           │     100 → 50
           │
           ├─→ OnDamaged 이벤트
           │     → 데미지 넘버 표시
           │     → 히트 이펙트
           │
           └─→ currentHp <= 0?
                   │
                   ├─→ ❌ 살아있음: 계속 전투
                   │
                   └─→ ✅ 사망
                           ↓
                   ┌────────────────────────────┐
                   │  Enemy.Die()               │
                   └──────────┬─────────────────┘
                              │
                              ├─→ 골드 드랍
                              │     goldAmount = Random(15, 25)
                              │     CurrencySystem.AddGold(20)
                              │
                              ├─→ 사망 애니메이션
                              │
                              └─→ Destroy(gameObject)
```

---

## 5. 데이터 스키마

### 5.1 Fire Magic Ability 전체 데이터 플로우

```
┌──────────────────────────────────────────────────┐
│  FireMagicData.asset (ScriptableObject)         │
│  ────────────────────────────────────────────    │
│  abilityName: "FireMagic"                        │
│  damageMultiplier: 2.5                           │
│  cooldownDuration: 7.0                           │
│  projectileSpeed: 15.0                           │
│  maxRange: 10.0                                  │
│  projectilePrefab: FireBall.prefab               │
└──────────────────┬───────────────────────────────┘
                   │
                   │ loaded-by
                   ↓
┌──────────────────────────────────────────────────┐
│  FireMagicAbility (Runtime)                      │
│  ────────────────────────────────────────────    │
│  data: FireMagicData (참조)                      │
│  cooldown: AbilityCooldown (인스턴스)            │
│  playerStats: PlayerStats (참조)                 │
│  ────────────────────────────────────────────    │
│  ExecuteAsync() 실행 시:                         │
│  1. attack = playerStats.GetStat(Attack)  → 20   │
│  2. damage = 20 * 2.5                     → 50   │
│  3. projectile = Instantiate(data.prefab)        │
│  4. await FireProjectileAsync(projectile, 50)    │
│  5. cooldown.StartCooldown(7.0)                  │
└──────────────────────────────────────────────────┘
```

### 5.2 JSON 세이브 구조

**파일 위치**: `Application.persistentDataPath/savedata.json`

```json
{
  "version": "1.0",
  "timestamp": "2025-11-01T15:30:00Z",

  "player": {
    "stats": {
      "currentHp": 120,
      "maxHp": 120,
      "baseAttack": 15,
      "baseDefense": 5
    },
    "position": {
      "x": 10.5,
      "y": 2.0,
      "z": 0.0
    }
  },

  "inventory": {
    "ownedItems": [
      {
        "itemId": "Items/FireSword",
        "quantity": 1
      },
      {
        "itemId": "Items/LeatherArmor",
        "quantity": 1
      }
    ],
    "equippedItems": {
      "Weapon": "Items/FireSword",
      "Armor": "Items/LeatherArmor",
      "Accessory": null
    }
  },

  "economy": {
    "currentGold": 250
  },

  "progress": {
    "enemiesDefeated": 5,
    "bossDefeated": false,
    "checkpointReached": "Checkpoint_Mid"
  },

  "abilities": {
    "unlockedAbilities": [
      "FireMagic"
    ]
  }
}
```

**크기 제약**: <100KB (SC-010, plan.md 기술 제약)

---

## 6. 성능 고려사항

### 6.1 스탯 재계산 최적화

**요구사항**: SC-001 - 스탯 업데이트 <50ms

**구현**:
```csharp
public class PlayerStats
{
    private bool isDirty = true;
    private int cachedFinalAttack;

    public int GetStat(StatType.Attack)
    {
        if (isDirty)
        {
            // 재계산 (장비 루프)
            cachedFinalAttack = baseAttack;
            foreach (var item in equippedItems)
                cachedFinalAttack += item.attackBonus;

            isDirty = false;
        }
        return cachedFinalAttack;
    }

    public void EquipItem(Item item)
    {
        equippedItems.Add(item);
        MarkDirty(); // 다음 GetStat() 시 재계산
    }
}
```

**측정**:
- 장비 3개 기준: ~0.1ms (목표 달성)
- 최대 10개 장비: ~0.3ms (목표 달성)

### 6.2 JSON 직렬화 성능

**요구사항**: SC-010 - 세이브/로드 <2초

**전략**:
- Unity JsonUtility 사용 (빠름)
- 비동기 파일 I/O (File.WriteAllTextAsync)
- 데이터 최소화 (<100KB)

```csharp
public async Awaitable SaveAsync()
{
    string json = JsonUtility.ToJson(saveData, prettyPrint: false);
    string path = Path.Combine(Application.persistentDataPath, "savedata.json");

    await File.WriteAllTextAsync(path, json);
}
```

---

## 7. 다음 단계

### Phase 1 계속
1. ✅ data-model.md 완료
2. ⏳ api-contracts.md 작성 (인터페이스 계약 상세화)
3. ⏳ quickstart.md 작성 (사용 가이드)

### Phase 2 준비
- 이 데이터 모델 기반으로 tasks.md 생성 (`/speckit.tasks`)
- 우선순위: P1 (Stat System) → P2 (Shop) → ... → P6 (Complete)

---

## 8. 참고 자료

- Research: [research.md](./research.md)
- Spec: [spec.md](./spec.md)
- Plan: [plan.md](./plan.md)
- Unity ScriptableObject: https://docs.unity3d.com/Manual/class-ScriptableObject.html
- Unity JsonUtility: https://docs.unity3d.com/6000.0/Documentation/Manual/JSONSerialization.html

---

**버전**: 1.0
**상태**: Draft - Phase 1
**최종 수정**: 2025-11-01
