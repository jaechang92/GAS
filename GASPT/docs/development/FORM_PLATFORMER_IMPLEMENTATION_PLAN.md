# Skul 스타일 로그라이크 플랫포머 구현 계획

> **프로젝트**: GASPT - Skul: The Hero Slayer 오마주
> **장르**: 2D 플랫포머 로그라이크 액션 게임
> **작성일**: 2025-11-10
> **버전**: 1.0

---

## 📋 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [현재 상태 분석](#현재-상태-분석)
3. [개발 방향 및 전략](#개발-방향-및-전략)
4. [Phase A: 최소 플레이 가능 프로토타입](#phase-a-최소-플레이-가능-프로토타입)
5. [Phase B: 로그라이크 루프 완성](#phase-b-로그라이크-루프-완성)
6. [일정 및 마일스톤](#일정-및-마일스톤)
7. [작업 시작 가이드](#작업-시작-가이드)

---

## 🎮 프로젝트 개요

### 게임 컨셉

**핵심 아이디어**: Skul: The Hero Slayer를 오마주한 2D 플랫포머 로그라이크

**주요 특징**:
- ⭐ **폼 교체 시스템**: 2개 폼 동시 장착, 실시간 전환(Q키)
- ⭐ **아이템-스킬 연동**: 아이템 획득 시 사용 가능한 스킬 변경
- ⭐ **절차적 던전**: 매번 다른 구조의 던전 생성
- ⭐ **로그라이크 루프**: 죽으면 처음부터, 메타 진행으로 점진적 강화
- ⭐ **폼 각성**: 업그레이드를 통한 능력 향상

### 플레이어 캐릭터

**기본 캐릭터**: 마법사(MageForm)
- 원거리 마법 공격
- 빠른 이동 속도
- 순간이동/화염구 등 마법 스킬

**확장 폼 (Phase B 이후)**:
- Warrior: 근접 전투형
- Assassin: 빠른 공격형
- Tank: 방어형

### 게임 루프

```
시작 방 스폰
   ↓
적 처치 → 아이템 드롭 → 스킬 변경
   ↓
다음 방으로 이동 (전투방/상점방/보물방)
   ↓
보스 방 도달 → 보스 처치
   ↓
던전 클리어 → 뼈/영혼 획득 → 메타 업그레이드
   ↓
새 던전 시작 (더 어려움)
```

---

## 📊 현재 상태 분석

### ✅ 완성된 시스템 (활용 가능)

1. **GAS Core** (Gameplay Ability System)
   - ScriptableObject 기반 어빌리티 시스템
   - 쿨다운, 리소스 관리
   - **활용**: 스킬을 Ability로 구현

2. **FSM Core** (Finite State Machine)
   - Unity Awaitable 기반 상태머신
   - **활용**: 플레이어/적 상태 관리

3. **CharacterPhysics**
   - Skul 스타일 플랫포머 물리
   - 점프, 대시 구현 완료
   - **활용**: 플레이어 이동 시스템

4. **Combat System**
   - 콤보, 데미지, 히트박스/허트박스
   - **활용**: 전투 로직

5. **HUD System**
   - 체력바, 스킬 슬롯 UI
   - **활용**: 인게임 UI

### ⚠️ 기존 시스템 처리 방침

**Phase 1-13에서 구현한 RPG 시스템들**:
- 스탯 시스템 (PlayerStats)
- 상점 & 인벤토리
- 레벨 & EXP
- 버프/디버프
- Loot System

**처리 방침**:
- 당장 사용하지 않음 (보류/아카이브)
- 나중에 필요 시 부분적으로 통합 가능
- Phase B 이후 메타 진행 시스템에 활용 검토

### ❌ 구현 필요 시스템

1. **폼 교체 시스템** (핵심!)
2. **아이템-스킬 연동**
3. **절차적 던전 생성**
4. **적 AI 완성**
5. **로그라이크 루프**

---

## 🎯 개발 방향 및 전략

### 핵심 전략

1. **완성 우선 원칙**
   - 기능 추가보다 플레이 가능한 게임 우선
   - 작은 단위로 완성하며 진행

2. **기존 시스템 최대 활용**
   - GAS Core → 스킬 시스템
   - FSM Core → 상태 관리
   - CharacterPhysics → 이동
   - Combat → 전투

3. **단계적 확장**
   - Phase A: 최소 플레이 가능 (3주)
   - Phase B: 로그라이크 루프 (4-6주)
   - Phase C: 콘텐츠 확장 (이후)

### 성공 지표

**Phase A 완료 시**:
- ✅ 마법사로 던전 탐험 가능
- ✅ 적과 전투 가능
- ✅ 아이템 획득 → 스킬 변경
- ✅ 3개 방 진행 가능

**Phase B 완료 시**:
- ✅ 절차적 던전 생성
- ✅ 2개 폼 교체 가능
- ✅ 메타 진행 (영구 업그레이드)
- ✅ 완전한 로그라이크 루프

---

## 🚀 Phase A: 최소 플레이 가능 프로토타입

### 목표
**"마법사 폼로 던전을 돌아다니며 적을 처치하고 아이템을 먹으면 스킬이 바뀌는 게임"**

**예상 기간**: 3주 (15-20일)

---

## 📝 Phase A-1: MageForm 기본 구현

**작업 기간**: 1주 (6일)
**담당**: Player 시스템

### 목표
마법사 폼로 플레이 가능한 기본 캐릭터 구현

### 구현 파일 구조

```
Assets/_Project/Scripts/Gameplay/Form/
├── Core/
│   ├── IFormController.cs          # 폼 인터페이스
│   ├── BaseForm.cs                 # 폼 기본 클래스
│   ├── FormData.cs                 # ScriptableObject 폼 데이터
│   └── FormManager.cs              # 폼 관리 싱글톤
│
├── Implementations/
│   └── MageForm.cs                 # 마법사 폼 구현
│
└── Abilities/
    ├── MagicMissileAbility.cs       # 기본 공격 (마법 미사일)
    ├── TeleportAbility.cs           # 스킬 1 (순간이동)
    └── FireballAbility.cs           # 스킬 2 (화염구)
```

### 핵심 코드 구조

#### 1. IFormController 인터페이스

```csharp
// Assets/_Project/Scripts/Gameplay/Form/Core/IFormController.cs
namespace GASPT.Form
{
    public interface IFormController
    {
        string SkullName { get; }
        FormType FormType { get; }

        // 폼 활성화/비활성화
        void Activate();
        void Deactivate();

        // 스탯
        float MaxHealth { get; }
        float MoveSpeed { get; }
        float JumpPower { get; }

        // 스킬
        void SetAbility(int slotIndex, IAbility ability);
        IAbility GetAbility(int slotIndex);
    }

    public enum FormType
    {
        Mage,      // 마법사
        Warrior,   // 전사
        Assassin,  // 암살자
        Tank       // 탱커
    }
}
```

#### 2. BaseForm 추상 클래스

```csharp
// Assets/_Project/Scripts/Gameplay/Form/Core/BaseForm.cs
using UnityEngine;
using GASPT.GAS;

namespace GASPT.Form
{
    public abstract class BaseForm : MonoBehaviour, IFormController
    {
        [Header("Form Info")]
        [SerializeField] protected FormData formData;

        [Header("Abilities")]
        protected IAbility[] abilities = new IAbility[4];  // 0: 기본공격, 1-3: 스킬

        public abstract string SkullName { get; }
        public abstract FormType FormType { get; }

        public virtual float MaxHealth => formData.maxHealth;
        public virtual float MoveSpeed => formData.moveSpeed;
        public virtual float JumpPower => formData.jumpPower;

        public virtual void Activate()
        {
            gameObject.SetActive(true);
            Debug.Log($"[Form] {SkullName} Activated");
        }

        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
            Debug.Log($"[Form] {SkullName} Deactivated");
        }

        public void SetAbility(int slotIndex, IAbility ability)
        {
            if (slotIndex < 0 || slotIndex >= abilities.Length) return;
            abilities[slotIndex] = ability;
        }

        public IAbility GetAbility(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= abilities.Length) return null;
            return abilities[slotIndex];
        }
    }
}
```

#### 3. FormData ScriptableObject

```csharp
// Assets/_Project/Scripts/Gameplay/Form/Core/FormData.cs
using UnityEngine;

namespace GASPT.Form
{
    [CreateAssetMenu(fileName = "FormData", menuName = "GASPT/Form/Form Data")]
    public class FormData : ScriptableObject
    {
        [Header("Basic Info")]
        public string formName;
        public FormType formType;
        public Sprite icon;

        [Header("Stats")]
        public float maxHealth = 100f;
        public float moveSpeed = 5f;
        public float jumpPower = 10f;

        [Header("Abilities")]
        public AbilityData basicAttack;        // 기본 공격
        public AbilityData[] defaultSkills;    // 기본 스킬들
    }
}
```

#### 4. MageForm 구현

```csharp
// Assets/_Project/Scripts/Gameplay/Form/Implementations/MageForm.cs
using UnityEngine;
using GASPT.Player;

namespace GASPT.Form
{
    public class MageForm : BaseForm
    {
        public override string SkullName => "Mage";
        public override FormType FormType => FormType.Mage;

        private CharacterPhysics physics;

        private void Awake()
        {
            physics = GetComponent<CharacterPhysics>();
            InitializeDefaultAbilities();
        }

        private void InitializeDefaultAbilities()
        {
            // 기본 공격: 마법 미사일
            SetAbility(0, new MagicMissileAbility());

            // 스킬 1: 순간이동
            SetAbility(1, new TeleportAbility());

            // 스킬 2: 화염구
            SetAbility(2, new FireballAbility());
        }

        public override void Activate()
        {
            base.Activate();

            // 마법사 스탯 적용
            if (physics != null)
            {
                physics.moveSpeed = MoveSpeed;
                physics.jumpForce = JumpPower;
            }
        }
    }
}
```

#### 5. MagicMissileAbility (기본 공격)

```csharp
// Assets/_Project/Scripts/Gameplay/Form/Abilities/MagicMissileAbility.cs
using System.Threading;
using UnityEngine;

namespace GASPT.Form
{
    public class MagicMissileAbility : IAbility
    {
        public string AbilityName => "Magic Missile";
        public float Cooldown => 0.5f;

        private float lastUsedTime;

        public async Awaitable ExecuteAsync(GameObject caster, CancellationToken token)
        {
            if (Time.time - lastUsedTime < Cooldown) return;

            // 투사체 생성
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - caster.transform.position).normalized;

            // TODO: 실제 투사체 생성 로직
            Debug.Log($"[MagicMissile] Fired towards {direction}");

            lastUsedTime = Time.time;
            await Awaitable.NextFrameAsync(token);
        }
    }
}
```

### 테스트 계획

#### 테스트 씬: MageFormTest.unity

**플레이어 오브젝트 구성**:
```
Player (GameObject)
├── CharacterPhysics (Component)
├── MageForm (Component)
├── InputHandler (Component)
├── Animator (Component)
├── SpriteRenderer (Component)
└── BoxCollider2D (Component)
```

#### 테스트 체크리스트

- [ ] WASD로 이동 가능
- [ ] Space로 점프 가능
- [ ] 마우스 좌클릭으로 마법 미사일 발사
- [ ] 스탯이 마법사 기준으로 적용됨 (빠른 이동, 높은 점프)
- [ ] 스킬 슬롯에 3개 스킬 등록됨
- [ ] Debug.Log로 스킬 발동 확인

### 작업 분할

| 일차 | 작업 내용 | 산출물 |
|------|-----------|--------|
| 1일 | 인터페이스/베이스 클래스 | IFormController, BaseForm, FormData |
| 2-3일 | MageForm 구현 | MageForm.cs, 스탯 적용 |
| 4-5일 | Abilities 구현 | MagicMissileAbility, TeleportAbility, FireballAbility |
| 6일 | 테스트 씬 구성 및 디버깅 | MageFormTest.unity |

### 완료 조건

✅ 마법사 폼로 이동/점프/공격이 가능하며, 스킬 슬롯이 정상 작동함

---

## 📝 Phase A-2: 기본 적 AI + 전투 시스템

**작업 기간**: 1주 (6일)
**담당**: Enemy & Combat

### 목표
플레이어가 적과 싸울 수 있는 기본 전투 루프 구현

### 구현 파일 구조

```
Assets/_Project/Scripts/Gameplay/Enemy/
├── AI/
│   ├── BasicEnemyAI.cs              # 기본 적 AI
│   ├── EnemyState.cs                # 적 상태 (Idle, Chase, Attack)
│   └── EnemyDetection.cs            # 플레이어 감지
│
└── Types/
    ├── MeleeEnemy.cs                # 근접 공격 적
    └── RangedEnemy.cs               # 원거리 공격 적 (선택)
```

### 핵심 코드: BasicEnemyAI

```csharp
// Assets/_Project/Scripts/Gameplay/Enemy/AI/BasicEnemyAI.cs
using UnityEngine;
using GASPT.Combat;

namespace GASPT.Enemy
{
    public class BasicEnemyAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float attackDamage = 10f;

        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private HealthSystem healthSystem;

        [Header("Loot")]
        [SerializeField] private GameObject[] itemDropPrefabs;
        [SerializeField] private float dropChance = 0.3f;  // 30% 확률

        private EnemyState currentState = EnemyState.Idle;
        private float lastAttackTime;

        private void Start()
        {
            if (healthSystem != null)
            {
                healthSystem.OnDeath += OnDeath;
            }
        }

        private void Update()
        {
            if (player == null) FindPlayer();
            if (healthSystem != null && healthSystem.IsDead) return;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // 상태 전환
            switch (currentState)
            {
                case EnemyState.Idle:
                    if (distanceToPlayer < detectionRange)
                        currentState = EnemyState.Chase;
                    break;

                case EnemyState.Chase:
                    if (distanceToPlayer > detectionRange)
                        currentState = EnemyState.Idle;
                    else if (distanceToPlayer < attackRange)
                        currentState = EnemyState.Attack;
                    else
                        MoveTowardsPlayer();
                    break;

                case EnemyState.Attack:
                    if (distanceToPlayer > attackRange)
                        currentState = EnemyState.Chase;
                    else
                        TryAttack();
                    break;
            }
        }

        private void MoveTowardsPlayer()
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);

            // 스프라이트 플립
            if (direction.x < 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else
                transform.localScale = Vector3.one;
        }

        private void TryAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;

            // 플레이어에게 데미지
            var playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage, gameObject);
                Debug.Log($"[Enemy] Attacked player for {attackDamage} damage!");
            }

            lastAttackTime = Time.time;
        }

        private void FindPlayer()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        private void OnDeath()
        {
            Debug.Log("[Enemy] Died!");

            // 랜덤 드롭
            if (Random.value < dropChance && itemDropPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, itemDropPrefabs.Length);
                Instantiate(itemDropPrefabs[randomIndex], transform.position, Quaternion.identity);
                Debug.Log("[Enemy] Dropped item!");
            }

            // 비활성화 (나중에 풀링으로 전환)
            gameObject.SetActive(false);
        }
    }

    public enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }
}
```

### 기존 HealthSystem 수정

```csharp
// 기존 HealthSystem.cs에 추가
public class HealthSystem : MonoBehaviour
{
    public bool IsDead { get; private set; }
    public event System.Action OnDeath;

    public void TakeDamage(float amount, GameObject attacker)
    {
        if (IsDead) return;

        currentHealth -= amount;
        OnDamaged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        OnDeath?.Invoke();
        Debug.Log($"[Health] {gameObject.name} died!");
    }
}
```

### 적 프리팹 구성

**BasicEnemy Prefab**:
```
BasicEnemy (GameObject)
├── SpriteRenderer (빨간 사각형)
├── BoxCollider2D (Trigger = false)
├── Rigidbody2D (Kinematic)
├── HealthSystem (maxHealth = 50)
├── BasicEnemyAI
└── DamageSystem
```

### 테스트 체크리스트

- [ ] 플레이어가 5m 안에 들어오면 추적
- [ ] 1.5m 안에 들어오면 공격 (2초 쿨다운)
- [ ] 공격 시 플레이어 HP 감소
- [ ] 플레이어 공격 시 적 HP 감소
- [ ] HP 0되면 적 사망 (비활성화)
- [ ] 30% 확률로 아이템 드롭

### 작업 분할

| 일차 | 작업 내용 | 산출물 |
|------|-----------|--------|
| 1-2일 | BasicEnemyAI 구현 | AI 로직, 상태 전환 |
| 3-4일 | Combat 시스템 연동 | HealthSystem 수정, 데미지 처리 |
| 5일 | 적 프리팹 제작 | BasicEnemy.prefab |
| 6일 | 테스트 및 밸런싱 | 데미지/체력 조정 |

### 완료 조건

✅ 적이 플레이어를 추적하고 공격하며, 플레이어도 적을 공격하여 처치 가능

---

## 📝 Phase A-3: 단순 Room 시스템

**작업 기간**: 4일
**담당**: Level System

### 목표
여러 방을 이동하며 플레이할 수 있는 기본 던전 구조

### 구현 파일 구조

```
Assets/_Project/Scripts/Gameplay/Level/
├── Room/
│   ├── RoomData.cs                  # ScriptableObject 방 데이터
│   ├── Room.cs                      # 방 MonoBehaviour
│   ├── RoomType.cs                  # 방 타입 enum
│   └── RoomDoor.cs                  # 방 입구/출구
│
└── Manager/
    └── DungeonManager.cs            # 던전 진행 관리
```

### 핵심 코드

#### 1. RoomType & RoomData

```csharp
// Assets/_Project/Scripts/Gameplay/Level/Room/RoomType.cs
namespace GASPT.Level
{
    public enum RoomType
    {
        Start,      // 시작 방
        Combat,     // 전투 방
        Shop,       // 상점 방
        Treasure,   // 보물 방
        Boss        // 보스 방
    }
}

// Assets/_Project/Scripts/Gameplay/Level/Room/RoomData.cs
using UnityEngine;

namespace GASPT.Level
{
    [CreateAssetMenu(fileName = "RoomData", menuName = "GASPT/Level/Room Data")]
    public class RoomData : ScriptableObject
    {
        public RoomType roomType;
        public int enemyCount;           // 스폰할 적 수
        public GameObject[] enemyPrefabs; // 적 프리팹들
        public Vector2 roomSize = new Vector2(20f, 10f);
    }
}
```

#### 2. Room 클래스

```csharp
// Assets/_Project/Scripts/Gameplay/Level/Room/Room.cs
using UnityEngine;
using System.Collections.Generic;

namespace GASPT.Level
{
    public class Room : MonoBehaviour
    {
        [Header("Room Settings")]
        [SerializeField] private RoomData roomData;
        [SerializeField] private Transform[] enemySpawnPoints;
        [SerializeField] private RoomDoor exitDoor;

        private List<GameObject> activeEnemies = new List<GameObject>();
        private bool isCleared = false;

        public bool IsCleared => isCleared;
        public RoomType RoomType => roomData.roomType;

        private void Start()
        {
            SpawnEnemies();
            if (exitDoor != null) exitDoor.Lock();
        }

        private void Update()
        {
            if (isCleared) return;

            // 모든 적 처치 확인
            activeEnemies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);

            if (activeEnemies.Count == 0)
            {
                OnRoomCleared();
            }
        }

        private void SpawnEnemies()
        {
            if (roomData.roomType == RoomType.Start) return;

            for (int i = 0; i < roomData.enemyCount && i < enemySpawnPoints.Length; i++)
            {
                int randomIndex = Random.Range(0, roomData.enemyPrefabs.Length);
                GameObject enemy = Instantiate(
                    roomData.enemyPrefabs[randomIndex],
                    enemySpawnPoints[i].position,
                    Quaternion.identity,
                    transform
                );

                activeEnemies.Add(enemy);
            }

            Debug.Log($"[Room] Spawned {activeEnemies.Count} enemies");
        }

        private void OnRoomCleared()
        {
            isCleared = true;
            Debug.Log("[Room] Room Cleared!");

            if (exitDoor != null) exitDoor.Unlock();
        }
    }
}
```

#### 3. RoomDoor

```csharp
// Assets/_Project/Scripts/Gameplay/Level/Room/RoomDoor.cs
using UnityEngine;

namespace GASPT.Level
{
    public class RoomDoor : MonoBehaviour
    {
        [SerializeField] private Room nextRoom;
        [SerializeField] private SpriteRenderer doorSprite;
        [SerializeField] private Color lockedColor = Color.red;
        [SerializeField] private Color unlockedColor = Color.green;

        private bool isLocked = true;

        public void Lock()
        {
            isLocked = true;
            if (doorSprite != null) doorSprite.color = lockedColor;
        }

        public void Unlock()
        {
            isLocked = false;
            if (doorSprite != null) doorSprite.color = unlockedColor;
            Debug.Log("[Door] Unlocked!");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isLocked) return;

            if (collision.CompareTag("Player") && nextRoom != null)
            {
                EnterNextRoom(collision.gameObject);
            }
        }

        private void EnterNextRoom(GameObject player)
        {
            Debug.Log($"[Door] Entering next room: {nextRoom.RoomType}");

            // 카메라를 다음 방으로 이동
            Camera.main.transform.position = new Vector3(
                nextRoom.transform.position.x,
                nextRoom.transform.position.y,
                Camera.main.transform.position.z
            );

            // 플레이어 위치 이동
            player.transform.position = nextRoom.transform.position + Vector3.left * 5f;
        }
    }
}
```

### 테스트 씬 구조

**DungeonTestScene**:
```
Hierarchy
├── Camera (Main Camera)
├── Room_Start (위치: 0, 0)
│   ├── Ground (Tilemap or Sprite)
│   ├── ExitDoor (BoxCollider2D Trigger) → Room_Combat
│   └── PlayerSpawnPoint
│
├── Room_Combat (위치: 25, 0)
│   ├── Ground
│   ├── EnemySpawnPoint (x3)
│   └── ExitDoor → Room_Boss
│
└── Room_Boss (위치: 50, 0)
    ├── Ground
    ├── BossSpawnPoint
    └── VictoryTrigger
```

### 테스트 체크리스트

- [ ] 시작 방에서 플레이어 스폰
- [ ] 출구 문이 빨간색(잠김)
- [ ] 적 처치 시 문이 초록색(열림)
- [ ] 문에 닿으면 다음 방으로 이동
- [ ] 카메라가 자동으로 이동
- [ ] 3개 방 순서대로 진행 가능

### 작업 분할

| 일차 | 작업 내용 | 산출물 |
|------|-----------|--------|
| 1-2일 | Room/Door 시스템 구현 | Room.cs, RoomDoor.cs, RoomData.cs |
| 3일 | 테스트 씬 구성 | DungeonTestScene.unity, 3개 방 배치 |
| 4일 | 디버깅 및 개선 | 카메라 전환, 플레이어 이동 보정 |

### 완료 조건

✅ 3개 방을 순서대로 진행할 수 있으며, 각 방의 적을 처치하면 다음 방으로 이동 가능

---

## 📝 Phase A-4: 아이템 드롭 + 스킬 변경

**작업 기간**: 4일
**담당**: Item System

### 목표
"적 처치 → 아이템 드롭 → 획득 → 스킬 변경" 루프 구현

### 구현 파일 구조

```
Assets/_Project/Scripts/Gameplay/Item/
├── ItemData.cs                      # ScriptableObject 아이템 데이터
├── DroppedItem.cs                   # 바닥의 아이템
├── ItemPickup.cs                    # 아이템 획득 처리
└── SkillItemData.cs                 # 스킬을 주는 아이템
```

### 핵심 코드

#### 1. ItemData & SkillItemData

```csharp
// Assets/_Project/Scripts/Gameplay/Item/ItemData.cs
using UnityEngine;

namespace GASPT.Item
{
    public enum ItemType
    {
        Skill,      // 스킬 변경
        Passive,    // 패시브 효과
        Consumable  // 소모품
    }

    [CreateAssetMenu(fileName = "ItemData", menuName = "GASPT/Item/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public ItemType itemType;
        public Sprite icon;
        public string description;
    }
}

// Assets/_Project/Scripts/Gameplay/Item/SkillItemData.cs
using UnityEngine;
using GASPT.GAS;

namespace GASPT.Item
{
    [CreateAssetMenu(fileName = "SkillItem", menuName = "GASPT/Item/Skill Item")]
    public class SkillItemData : ItemData
    {
        [Header("Skill Settings")]
        public int targetSlotIndex = 1;      // 어느 스킬 슬롯을 바꿀지 (1~3)
        public AbilityData abilityData;      // 변경할 Ability
    }
}
```

#### 2. DroppedItem

```csharp
// Assets/_Project/Scripts/Gameplay/Item/DroppedItem.cs
using UnityEngine;

namespace GASPT.Item
{
    public class DroppedItem : MonoBehaviour
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float floatAmplitude = 0.3f;
        [SerializeField] private float floatSpeed = 2f;

        private Vector3 startPosition;

        public ItemData ItemData => itemData;

        private void Start()
        {
            startPosition = transform.position;

            if (spriteRenderer != null && itemData != null)
                spriteRenderer.sprite = itemData.icon;
        }

        private void Update()
        {
            // 부유 애니메이션
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
```

#### 3. ItemPickup

```csharp
// Assets/_Project/Scripts/Gameplay/Item/ItemPickup.cs
using UnityEngine;
using GASPT.Form;

namespace GASPT.Item
{
    public class ItemPickup : MonoBehaviour
    {
        private IFormController currentSkull;

        private void Start()
        {
            currentSkull = GetComponent<IFormController>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var droppedItem = collision.GetComponent<DroppedItem>();
            if (droppedItem == null) return;

            PickupItem(droppedItem);
        }

        private void PickupItem(DroppedItem droppedItem)
        {
            var itemData = droppedItem.ItemData;

            Debug.Log($"[ItemPickup] Picked up: {itemData.itemName}");

            if (itemData is SkillItemData skillItem)
            {
                ApplySkillItem(skillItem);
            }

            // 아이템 제거
            Destroy(droppedItem.gameObject);
        }

        private void ApplySkillItem(SkillItemData skillItem)
        {
            if (currentSkull == null) return;

            // 스킬 슬롯에 새 Ability 적용
            var newAbility = CreateAbilityFromData(skillItem.abilityData);
            currentSkull.SetAbility(skillItem.targetSlotIndex, newAbility);

            Debug.Log($"[ItemPickup] Skill changed! Slot {skillItem.targetSlotIndex} → {skillItem.abilityData.abilityName}");

            // UI 업데이트 (기존 HUD 활용)
            // TODO: HUDManager에 스킬 아이콘 변경 알림
        }

        private IAbility CreateAbilityFromData(AbilityData abilityData)
        {
            // GAS Core의 AbilityData로부터 Ability 인스턴스 생성
            // TODO: AbilityFactory 구현 필요
            return null;
        }
    }
}
```

### 생성할 아이템 3종

1. **FireballSkill 아이템**
   - 이름: "화염구의 두루마리"
   - targetSlotIndex: 2
   - 효과: 스킬 2번을 강력한 화염구로 변경

2. **LightningBolt 아이템**
   - 이름: "번개의 지팡이"
   - targetSlotIndex: 2
   - 효과: 스킬 2번을 번개 공격으로 변경

3. **IceSpike 아이템**
   - 이름: "얼음의 오브"
   - targetSlotIndex: 2
   - 효과: 스킬 2번을 얼음창으로 변경

### 테스트 체크리스트

- [ ] 적 처치 시 아이템 드롭 (30% 확률)
- [ ] 아이템이 부유 애니메이션
- [ ] 플레이어가 아이템에 닿으면 획득
- [ ] 스킬 슬롯이 변경됨 (UI 아이콘 변경)
- [ ] 변경된 스킬을 사용 가능

### 작업 분할

| 일차 | 작업 내용 | 산출물 |
|------|-----------|--------|
| 1일 | ItemData/DroppedItem 구현 | ItemData.cs, DroppedItem.cs |
| 2일 | ItemPickup + 스킬 변경 로직 | ItemPickup.cs, 스킬 교체 |
| 3일 | 아이템 프리팹 3종 제작 | 3개 SkillItem ScriptableObject, 프리팹 |
| 4일 | 테스트 및 UI 연동 | HUD 스킬 아이콘 업데이트 |

### 완료 조건

✅ 아이템을 획득하면 스킬이 변경되고, UI에 반영되며, 새 스킬을 사용할 수 있음

---

## 📊 Phase A 전체 일정 요약

| Week | Phase | 작업 내용 | 산출물 | 완료 조건 |
|------|-------|-----------|--------|-----------|
| **1주** | A-1 | MageForm 구현 | MageForm.cs, 3개 Abilities | 마법사로 이동/공격 가능 |
| **2주** | A-2 | Enemy AI + Combat | BasicEnemyAI.cs, Combat 연동 | 적과 전투 가능 |
| **3주 전반** | A-3 | Room System | Room.cs, RoomDoor.cs, 테스트 씬 | 3개 방 진행 가능 |
| **3주 후반** | A-4 | Item-Skill | ItemPickup.cs, 아이템 3종 | 아이템 먹으면 스킬 변경 |

**총 예상 기간**: 3주 (15-20일)

---

## ✅ Phase A 완료 조건 (MVP)

Phase A가 완료되면 다음이 **모두** 가능해야 합니다:

1. ✅ 마법사 폼로 플레이 시작
2. ✅ WASD 이동, Space 점프, 마우스 클릭으로 마법 공격
3. ✅ 적이 플레이어를 추적하고 공격
4. ✅ 적을 처치하면 다음 방으로 진행
5. ✅ 적 처치 시 아이템 드롭 (30% 확률)
6. ✅ 아이템 획득 시 스킬 변경 (UI 반영)
7. ✅ 3개 방을 거쳐 보스 방 도달

**이 상태면 "플레이 가능한 게임"입니다!** 🎉

---

## 🎯 Phase B: 로그라이크 루프 완성

**예상 기간**: 4-6주
**시작**: Phase A 완료 후

### Phase B-1: 절차적 던전 생성 (2주)

**목표**: 매번 다른 던전 구조

**구현 내용**:
- Room Generator (랜덤 배치)
- Dungeon Graph (시작→보스 경로 보장)
- 선택적 경로 (보물방, 상점방)
- 미니맵 시스템

### Phase B-2: 폼 교체 시스템 (1-2주)

**목표**: 2개 폼 동시 장착 + Q키 전환

**구현 내용**:
- FormManager 확장 (2개 폼 슬롯)
- Q키 입력 처리
- 폼 전환 애니메이션
- 스탯/스킬 자동 적용

### Phase B-3: 메타 진행 시스템 (1-2주)

**목표**: 영구 업그레이드

**구현 내용**:
- 뼈/영혼 리소스 시스템
- 업그레이드 트리 (체력, 공격력, 새 폼 해금)
- PlayerPrefs/JSON 저장
- 업그레이드 UI

### Phase B-4: 게임 루프 완성 (1주)

**목표**: 시작→진행→종료→재시작

**구현 내용**:
- 게임오버 화면
- 승리 화면
- Run 통계 (킬 수, 획득 골드)
- 재시작 버튼

---

## 📅 일정 및 마일스톤

### 전체 로드맵

```
Phase A (3주)
   ↓
[ Milestone 1: Playable Prototype ]
   ↓
Phase B (4-6주)
   ↓
[ Milestone 2: Core Loop Complete ]
   ↓
Phase C (이후 확장)
```

### Milestone 1: Playable Prototype (Phase A 완료)

**완료 기준**:
- ✅ 마법사로 던전 탐험 가능
- ✅ 적과 전투 가능
- ✅ 아이템 획득 → 스킬 변경
- ✅ 3개 방 진행 가능

**데모 영상 촬영**: 10분 플레이 영상

### Milestone 2: Core Loop Complete (Phase B 완료)

**완료 기준**:
- ✅ 절차적 던전 생성
- ✅ 2개 폼 교체 가능
- ✅ 메타 진행 (영구 업그레이드)
- ✅ 완전한 로그라이크 루프

**알파 테스트**: 외부 플레이 테스트 진행

---

## 🚀 작업 시작 가이드

### 1단계: Git 브랜치 생성

```bash
cd D:/JaeChang/UintyDev/GASPT/GASPT
git checkout master
git pull origin master
git checkout -b 014-form-platformer-phase-a
```

### 2단계: 폴더 구조 생성

**Windows Command**:
```cmd
mkdir Assets\_Project\Scripts\Gameplay\Form\Core
mkdir Assets\_Project\Scripts\Gameplay\Form\Implementations
mkdir Assets\_Project\Scripts\Gameplay\Form\Abilities
mkdir Assets\_Project\Scripts\Gameplay\Level\Room
mkdir Assets\_Project\Scripts\Gameplay\Level\Manager
mkdir Assets\_Project\Scripts\Gameplay\Item
```

**확인**:
```
Assets/_Project/Scripts/Gameplay/
├── Form/
│   ├── Core/
│   ├── Implementations/
│   └── Abilities/
├── Level/
│   ├── Room/
│   └── Manager/
└── Item/
```

### 3단계: 작업 순서

1. **Week 1 (Phase A-1)**
   - [ ] IFormController.cs 작성
   - [ ] BaseForm.cs 작성
   - [ ] FormData.cs 작성
   - [ ] MageForm.cs 작성
   - [ ] 3개 Abilities 작성
   - [ ] 테스트 씬 구성

2. **Week 2 (Phase A-2)**
   - [ ] BasicEnemyAI.cs 작성
   - [ ] HealthSystem 수정
   - [ ] 적 프리팹 제작
   - [ ] Combat 연동 테스트

3. **Week 3-A (Phase A-3)**
   - [ ] RoomData.cs, Room.cs 작성
   - [ ] RoomDoor.cs 작성
   - [ ] 테스트 씬 (3개 방) 구성

4. **Week 3-B (Phase A-4)**
   - [ ] ItemData.cs, DroppedItem.cs 작성
   - [ ] ItemPickup.cs 작성
   - [ ] 아이템 3종 제작

### 4단계: 일일 작업 흐름

**매일**:
1. 해당 일차 파일 작성
2. 간단한 테스트 (Debug.Log)
3. 커밋 (명확한 메시지)
4. 진행 상황 기록

**매주 금요일**:
1. 해당 Phase 통합 테스트
2. 버그 수정
3. PR 생성 (선택)
4. 다음 주 계획 확인

### 5단계: 테스트 가이드

**Phase A-1 테스트**:
```
1. MageFormTest.unity 씬 열기
2. Play 모드 진입
3. WASD로 이동 확인
4. Space로 점프 확인
5. 마우스 좌클릭으로 공격 확인
6. Console에서 스킬 발동 로그 확인
```

**Phase A-2 테스트**:
```
1. BasicEnemy 프리팹을 씬에 배치
2. Play 모드 진입
3. 플레이어가 5m 안에 들어가기
4. 적이 추적하는지 확인
5. 1.5m 안에서 공격 확인
6. 플레이어가 적 공격 시 HP 감소 확인
```

---

## 📝 문서 관리

### 작업 문서

- **이 문서**: `SKULL_PLATFORMER_IMPLEMENTATION_PLAN.md`
- **진행 상황**: `WORK_STATUS.md` (업데이트 필요)
- **일일 작업 로그**: Git 커밋 메시지

### 커밋 메시지 규칙

```
[Phase A-1] IFormController 인터페이스 구현
[Phase A-1] MageForm 기본 구조 완성
[Phase A-2] BasicEnemyAI 추적 로직 구현
[Phase A-3] Room 시스템 완성
[Phase A-4] 아이템 드롭 시스템 구현
```

### PR 생성 시점

- Phase A-1 완료 시: PR #8
- Phase A-2 완료 시: PR #9
- Phase A-3 완료 시: PR #10
- Phase A-4 완료 시: PR #11

또는

- Phase A 전체 완료 시: PR #8 (큰 단위)

---

## ⚠️ 주의사항 및 팁

### 개발 시 주의사항

1. **기존 시스템과의 충돌 방지**
   - 새 네임스페이스 사용 (`GASPT.Form`, `GASPT.Level`, `GASPT.Item`)
   - 기존 RPG 시스템 파일 수정 최소화

2. **Awaitable 패턴 준수**
   - Coroutine 사용 금지
   - async/await 사용
   - CancellationToken 처리

3. **코딩 규칙**
   - 카멜케이스 (변수명에 `_` 붙이지 않음)
   - 한글 주석 허용
   - 500줄 넘으면 파일 분할

4. **Unity 6.0 호환성**
   - `velocity` → `linearVelocity`
   - `FindObjectOfType` → `FindAnyObjectByType`

### 개발 팁

1. **작은 단위로 자주 테스트**
   - 하루 작업 후 반드시 Play 모드 확인
   - Debug.Log 적극 활용

2. **프리팹 관리**
   - Player, Enemy, Item은 프리팹으로 관리
   - 프리팹 변경 시 씬에 반영 확인

3. **ScriptableObject 활용**
   - 데이터는 모두 ScriptableObject
   - 코드와 데이터 분리

4. **GAS Core 활용**
   - 스킬은 가능한 GAS Ability로 구현
   - AbilityData ScriptableObject 활용

---

## 🎯 성공 지표

### Phase A 성공 지표

- ✅ 코드 품질: 컴파일 에러 0개
- ✅ 테스트 통과율: 100% (모든 체크리스트)
- ✅ 플레이 시간: 5분 이상 플레이 가능
- ✅ 버그: 게임 진행 방해하는 버그 0개

### Phase B 성공 지표

- ✅ 재플레이성: 던전이 매번 다름
- ✅ 폼 교체: 2개 폼 자유롭게 전환
- ✅ 메타 진행: 업그레이드로 점진적 강화
- ✅ 플레이 시간: 30분 이상 플레이 가능

---

## 📚 참고 자료

### 프로젝트 내부 문서

- `docs/development/SkulSystemDesign.md` - 원본 Skul 시스템 설계
- `docs/development/ROGUELIKE_PLATFORMER_ROADMAP.md` - 로그라이크 로드맵
- `docs/getting-started/ProjectOverview.md` - 프로젝트 개요
- `WORK_STATUS.md` - 기존 작업 현황 (Phase 1-13)

### Unity 관련

- Unity 6.0 API 문서
- GAS Core 사용 가이드 (프로젝트 내)
- FSM Core 사용 가이드 (프로젝트 내)

### 게임 디자인 참고

- Skul: The Hero Slayer (Steam)
- Dead Cells (로그라이크 참고)
- Hades (로그라이크 참고)

---

## 🔄 업데이트 이력

### v1.0 - 2025-11-10
- 초기 문서 작성
- Phase A 상세 계획 수립 (4개 세부 Phase)
- Phase B 개요 작성
- 작업 시작 가이드 포함

---

## 💬 피드백 및 질문

작업 중 질문이나 피드백이 있으면:

1. **기술적 질문**: 이 문서의 해당 섹션 참고
2. **계획 변경**: `WORK_STATUS.md` 업데이트
3. **버그 발견**: GitHub Issues 등록 (선택)

---

**다음 작업**: [Phase A-1: MageForm 기본 구현](#phase-a-1-mageform-기본-구현) 시작

**시작 명령어**:
```bash
git checkout -b 014-form-platformer-phase-a
```

**첫 파일**: `Assets/_Project/Scripts/Gameplay/Form/Core/IFormController.cs`

---

*작성일: 2025-11-10*
*작성자: Claude Code Assistant*
*프로젝트: GASPT - Skul Style Roguelike Platformer*

🎮 **Let's make a great game!**
