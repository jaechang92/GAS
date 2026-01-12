using System;
using UnityEngine;
using GASPT.Data;
using GASPT.Economy;
using GASPT.Combat;
using GASPT.Level;
using GASPT.UI;
using GASPT.StatusEffects;
using GASPT.Core.Pooling;
using GASPT.Meta;
using GASPT.Core.Enums;
using GASPT.Loot;

namespace GASPT.Gameplay.Enemies
{
    /// <summary>
    /// 적 MonoBehaviour (추상 클래스)
    /// EnemyData를 기반으로 적의 스탯과 행동을 관리
    /// 오브젝트 풀링 지원
    ///
    /// ⚠️ 주의: 이 클래스는 직접 Component로 추가할 수 없습니다.
    /// PlatformerEnemy, BasicMeleeEnemy 등 상속받은 클래스를 사용하세요.
    /// </summary>
    [RequireComponent(typeof(PooledObject))]
    public abstract class Enemy : MonoBehaviour, IPoolable
    {
        // ====== EnemyData 참조 ======

        [Header("Enemy Data")]
        [SerializeField] [Tooltip("이 적의 데이터 (ScriptableObject)")]
        private EnemyData enemyData;


        // ====== 현재 상태 ======

        [Header("현재 상태")]
        [SerializeField] [Tooltip("현재 HP (읽기 전용)")]
        private int currentHp;

        private bool isDead = false;


        // ====== 이벤트 ======

        /// <summary>
        /// HP 변경 시 발생하는 이벤트
        /// 매개변수: (현재 HP, 최대 HP)
        /// </summary>
        public event Action<int, int> OnHpChanged;

        /// <summary>
        /// 사망 시 발생하는 이벤트
        /// 매개변수: (Enemy 인스턴스)
        /// </summary>
        public event Action<Enemy> OnDeath;


        // ====== 프로퍼티 ======

        /// <summary>
        /// EnemyData 읽기 전용 프로퍼티
        /// </summary>
        public EnemyData Data => enemyData;

        /// <summary>
        /// 현재 HP 읽기 전용 프로퍼티
        /// </summary>
        public int CurrentHp => currentHp;

        /// <summary>
        /// 최대 HP 읽기 전용 프로퍼티 (스케일링 적용)
        /// </summary>
        public int MaxHp => scaledMaxHp > 0 ? scaledMaxHp : (enemyData != null ? enemyData.maxHp : 0);

        /// <summary>
        /// 공격력 읽기 전용 프로퍼티 (스케일링 + 버프/디버프 적용)
        /// </summary>
        public int Attack
        {
            get
            {
                int baseAttack = scaledAttack > 0 ? scaledAttack : (enemyData != null ? enemyData.attack : 0);
                return ApplyStatusEffects(baseAttack, StatusEffectType.AttackUp, StatusEffectType.AttackDown);
            }
        }

        /// <summary>
        /// 사망 여부
        /// </summary>
        public bool IsDead => isDead;

        /// <summary>
        /// 적 타입
        /// </summary>
        public EnemyType EnemyType => enemyData != null ? enemyData.enemyType : EnemyType.Normal;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // Initialize는 외부에서 InitializeWithData() 호출 시 실행
            // Awake에서 호출하면 AddComponent 시 enemyData가 아직 설정되지 않아 에러 발생
        }

        protected virtual void Start()
        {
            // Inspector에서 직접 설정한 경우 Start에서 초기화
            if (enemyData != null && currentHp == 0)
            {
                Initialize();
            }
        }

        private void OnEnable()
        {
            // StatusEffect 이벤트 구독 (OnEnable에서 구독해야 StatusEffectManager 초기화 후 구독 가능)
            SubscribeToStatusEffectEvents();
        }

        protected virtual void OnDisable()
        {
            // StatusEffect 이벤트 구독 해제
            UnsubscribeFromStatusEffectEvents();
        }

        // ====== 초기화 ======

        /// <summary>
        /// 외부에서 EnemyData 설정 및 초기화
        /// EnemySpawnPoint 등에서 AddComponent 후 호출
        /// </summary>
        public void InitializeWithData(EnemyData data)
        {
            if (data == null)
            {
                Debug.LogError($"[Enemy] {gameObject.name}: InitializeWithData에 null이 전달되었습니다!");
                return;
            }

            enemyData = data;
            Initialize();
        }

        /// <summary>
        /// 적 초기화 (EnemyData 기반)
        /// </summary>
        private void Initialize()
        {
            // EnemyData 유효성 검증
            ValidateEnemyData();

            if (enemyData == null)
            {
                Debug.LogError($"[Enemy] {gameObject.name}: enemyData가 null입니다. Inspector에서 설정하세요.");
                return;
            }

            // HP 초기화
            currentHp = enemyData.maxHp;

            // 이벤트 발생
            OnHpChanged?.Invoke(currentHp, enemyData.maxHp);
        }

        /// <summary>
        /// 스케일링된 스탯 적용 (MonsterSpawnManager에서 호출)
        /// </summary>
        /// <param name="stats">스케일링된 스탯</param>
        public void ApplyScaledStats(ScaledEnemyStats stats)
        {
            scaledMaxHp = stats.maxHp;
            scaledAttack = stats.attack;
            scaledMinGold = stats.minGold;
            scaledMaxGold = stats.maxGold;
            scaledExp = stats.exp;

            // HP를 스케일링된 최대 HP로 갱신
            currentHp = scaledMaxHp;

            // 이벤트 발생
            OnHpChanged?.Invoke(currentHp, scaledMaxHp);
        }


        // ====== 스케일링된 스탯 (MonsterSpawnManager에서 설정) ======

        private int scaledMaxHp = -1;      // -1 = 미설정 (EnemyData 기본값 사용)
        private int scaledAttack = -1;
        private int scaledMinGold = -1;
        private int scaledMaxGold = -1;
        private int scaledExp = -1;


        // ====== 데미지 처리 ======

        /// <summary>
        /// 속성 데미지 받기 (속성 상성 적용)
        /// </summary>
        /// <param name="baseDamage">기본 데미지</param>
        /// <param name="attackElement">공격 속성</param>
        public void TakeDamage(int baseDamage, ElementType attackElement)
        {
            if (enemyData == null) return;

            // ElementDamageCalculator로 속성 상성 계산
            int finalDamage = ElementDamageCalculator.CalculateDamage(
                baseDamage,
                attackElement,
                enemyData.elementType
            );

            // 기존 TakeDamage 호출
            TakeDamage(finalDamage);
        }

        /// <summary>
        /// 데미지 받기 (기본)
        /// </summary>
        /// <param name="damage">받을 데미지</param>
        public void TakeDamage(int damage)
        {
            if (isDead)
            {
                Debug.LogWarning($"[Enemy] {enemyData.enemyName}: 이미 사망한 적입니다.");
                return;
            }

            if (damage <= 0)
            {
                Debug.LogWarning($"[Enemy] {enemyData.enemyName}: 데미지가 0 이하입니다: {damage}");
                return;
            }

            // HP 감소
            currentHp -= damage;
            currentHp = Mathf.Max(0, currentHp);

            // DamageNumber 표시
            if (DamageNumberPool.Instance != null)
            {
                Vector3 damagePosition = transform.position + Vector3.up * 1.5f;
                DamageNumberPool.Instance.ShowDamage(damage, damagePosition, false);
            }

            // 이벤트 발생
            OnHpChanged?.Invoke(currentHp, enemyData.maxHp);

            // 사망 체크
            if (currentHp <= 0)
            {
                Die();
            }
        }


        // ====== 사망 처리 ======

        /// <summary>
        /// 사망 처리 (골드 드롭 포함)
        /// </summary>
        private void Die()
        {
            if (isDead)
            {
                Debug.LogWarning($"[Enemy] {enemyData.enemyName}: 이미 사망한 적입니다.");
                return;
            }

            isDead = true;

            // 골드 드롭
            DropGold();

            // 경험치 지급
            GiveExp();

            // 아이템 드롭
            DropLoot();

            // 메타 재화 드롭 (Bone/Soul)
            DropMetaCurrency();

            // 사망 이벤트 발생
            OnDeath?.Invoke(this);

            // 풀로 반환 (1초 후 - 사망 애니메이션용)
            ReturnToPoolDelayed(1f);
        }


        // ====== 공격 ======

        /// <summary>
        /// 플레이어를 공격합니다
        /// </summary>
        /// <param name="target">공격할 플레이어</param>
        public void DealDamageTo(GASPT.Stats.PlayerStats target)
        {
            if (target == null)
            {
                Debug.LogWarning($"[Enemy] {enemyData.enemyName}: target이 null입니다.");
                return;
            }

            if (isDead)
            {
                Debug.LogWarning($"[Enemy] {enemyData.enemyName}: 사망한 상태에서는 공격할 수 없습니다.");
                return;
            }

            if (target.IsDead)
            {
                Debug.LogWarning($"[Enemy] {enemyData.enemyName}: 플레이어가 이미 사망했습니다.");
                return;
            }

            // DamageCalculator를 사용하여 데미지 계산
            int damage = DamageCalculator.CalculateDamageDealt(Attack);

            // 플레이어에게 데미지 적용
            target.TakeDamage(damage);
        }

        // ====== 골드 드롭 ======

        /// <summary>
        /// 골드 드롭 처리 (스케일링 적용)
        /// </summary>
        private void DropGold()
        {
            if (enemyData == null) return;

            // 스케일링된 골드 또는 기본 골드 계산
            int goldDrop;
            if (scaledMinGold > 0 && scaledMaxGold > 0)
            {
                goldDrop = UnityEngine.Random.Range(scaledMinGold, scaledMaxGold + 1);
            }
            else
            {
                goldDrop = enemyData.GetRandomGoldDrop();
            }

            // CurrencySystem에 골드 추가
            CurrencySystem currencySystem = CurrencySystem.Instance;

            if (currencySystem != null)
            {
                currencySystem.AddGold(goldDrop);
            }
            else
            {
                Debug.LogError($"[Enemy] CurrencySystem을 찾을 수 없습니다. 골드 드롭 실패: {goldDrop}");
            }
        }


        // ====== 경험치 지급 ======

        /// <summary>
        /// 경험치 지급 처리 (스케일링 적용)
        /// </summary>
        private void GiveExp()
        {
            if (enemyData == null) return;

            // 스케일링된 경험치 또는 기본 경험치
            int expReward = scaledExp > 0 ? scaledExp : enemyData.expReward;

            // PlayerLevel에 경험치 추가
            PlayerLevel playerLevel = PlayerLevel.Instance;

            if (playerLevel != null)
            {
                playerLevel.AddExp(expReward);

                // EXP Number 표시
                if (DamageNumberPool.Instance != null)
                {
                    Vector3 expPosition = transform.position + Vector3.up * 2f;
                    DamageNumberPool.Instance.ShowExp(expReward, expPosition);
                }
            }
            else
            {
                Debug.LogError($"[Enemy] PlayerLevel을 찾을 수 없습니다. EXP 지급 실패: {enemyData.expReward}");
            }
        }


        // ====== 아이템 드롭 ======

        /// <summary>
        /// 아이템 드롭 처리 (V2 우선, V1 폴백)
        /// </summary>
        private void DropLoot()
        {
            if (enemyData == null) return;

            // V2 LootTableV2 우선 사용
            if (enemyData.lootTableV2 != null)
            {
                if (ItemDropManager.HasInstance)
                {
                    ItemDropManager.Instance.DropFromTable(enemyData.lootTableV2, transform.position);
                }
                else
                {
                    Debug.LogWarning("[Enemy] ItemDropManager를 찾을 수 없습니다. V1 폴백 시도...");
                    DropLootV1Fallback();
                }
                return;
            }

            // V1 폴백 (lootTable)
            DropLootV1Fallback();
        }

        /// <summary>
        /// V1 LootSystem 폴백
        /// </summary>
        #pragma warning disable CS0618 // Obsolete 경고 무시
        private void DropLootV1Fallback()
        {
            if (enemyData.lootTable == null) return;

            if (GASPT.Loot.LootSystem.HasInstance)
            {
                GASPT.Loot.LootSystem.Instance.DropLoot(enemyData.lootTable, transform.position);
            }
            else
            {
                Debug.LogError("[Enemy] LootSystem을 찾을 수 없습니다. 아이템 드롭 실패");
            }
        }
        #pragma warning restore CS0618


        // ====== 메타 재화 드롭 ======

        /// <summary>
        /// 메타 재화 (Bone/Soul) 드롭 처리
        /// </summary>
        private void DropMetaCurrency()
        {
            if (enemyData == null) return;

            // MetaProgressionManager가 없거나 런 진행 중이 아니면 스킵
            if (!MetaProgressionManager.HasInstance || !MetaProgressionManager.Instance.IsInRun)
            {
                return;
            }

            var meta = MetaProgressionManager.Instance;

            // Bone 드롭 (일반 적 포함 모든 적)
            int boneDrop = enemyData.GetRandomBoneDrop();
            if (boneDrop > 0)
            {
                meta.Currency.AddTempBone(boneDrop);
            }

            // Soul 드롭 (보스 전용)
            if (enemyData.enemyType == EnemyType.Boss)
            {
                int soulDrop = enemyData.GetSoulDrop();
                if (soulDrop > 0)
                {
                    meta.Currency.AddTempSoul(soulDrop);
                }
            }
        }


        // ====== 유효성 검증 ======

        /// <summary>
        /// EnemyData 유효성 검증
        /// </summary>
        private void ValidateEnemyData()
        {
            if (enemyData == null)
            {
                Debug.LogError($"[Enemy] {gameObject.name}: enemyData가 null입니다. Inspector에서 EnemyData를 설정하세요.");
            }
        }


        // ====== IPoolable 구현 ======

        /// <summary>
        /// 풀에서 스폰될 때 호출
        /// </summary>
        public void OnSpawn()
        {
            // 상태 초기화
            isDead = false;

            // HP 복원 (enemyData가 설정되어 있으면)
            if (enemyData != null)
            {
                currentHp = enemyData.maxHp;
                OnHpChanged?.Invoke(currentHp, enemyData.maxHp);
            }
        }

        /// <summary>
        /// 풀로 반환될 때 호출
        /// </summary>
        public void OnDespawn()
        {
            // StatusEffect 정리
            UnsubscribeFromStatusEffectEvents();

            // 이벤트 구독자 정리
            OnHpChanged = null;
            OnDeath = null;
        }

        /// <summary>
        /// 지연 후 풀로 반환
        /// </summary>
        private async void ReturnToPoolDelayed(float delay)
        {
            // 지연 대기
            await Awaitable.WaitForSecondsAsync(delay);

            // PoolManager를 통해 풀로 반환
            if (PoolManager.Instance != null)
            {
                // Enemy 타입에 맞게 Despawn
                if (this is BasicMeleeEnemy basicMelee)
                {
                    PoolManager.Instance.Despawn(basicMelee);
                }
                else if (this is RangedEnemy rangedEnemy)
                {
                    PoolManager.Instance.Despawn(rangedEnemy);
                }
                else if (this is FlyingEnemy flyingEnemy)
                {
                    PoolManager.Instance.Despawn(flyingEnemy);
                }
                else if (this is EliteEnemy eliteEnemy)
                {
                    PoolManager.Instance.Despawn(eliteEnemy);
                }
                else
                {
                    Debug.LogWarning($"[Enemy] {enemyData?.enemyName} 알 수 없는 Enemy 타입. GameObject를 파괴합니다.");
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogWarning($"[Enemy] {enemyData?.enemyName} PoolManager가 없어 GameObject를 파괴합니다.");
                Destroy(gameObject);
            }
        }


        // ====== StatusEffect 통합 ======

        /// <summary>
        /// StatusEffect 이벤트 구독
        /// </summary>
        private void SubscribeToStatusEffectEvents()
        {
            // Instance 호출로 StatusEffectManager가 없으면 자동 생성
            StatusEffectManager manager = StatusEffectManager.Instance;

            if (manager != null)
            {
                // 중복 구독 방지를 위해 먼저 구독 해제
                manager.OnEffectApplied -= OnEffectApplied;
                manager.OnEffectRemoved -= OnEffectRemoved;

                // 구독
                manager.OnEffectApplied += OnEffectApplied;
                manager.OnEffectRemoved += OnEffectRemoved;
            }
            else
            {
                Debug.LogError("[Enemy] StatusEffectManager를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// StatusEffect 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromStatusEffectEvents()
        {
            if (StatusEffectManager.HasInstance)
            {
                StatusEffectManager.Instance.OnEffectApplied -= OnEffectApplied;
                StatusEffectManager.Instance.OnEffectRemoved -= OnEffectRemoved;
            }
        }

        /// <summary>
        /// 효과 적용 시 호출
        /// </summary>
        private void OnEffectApplied(GameObject target, StatusEffect effect)
        {
            if (target != gameObject) return;

            // DoT 효과 처리
            if (effect.TickInterval > 0f)
            {
                effect.OnTick += OnStatusEffectTick;
            }
        }

        /// <summary>
        /// 효과 제거 시 호출
        /// </summary>
        private void OnEffectRemoved(GameObject target, StatusEffect effect)
        {
            if (target != gameObject) return;

            // DoT 이벤트 구독 해제
            if (effect.TickInterval > 0f)
            {
                effect.OnTick -= OnStatusEffectTick;
            }
        }

        /// <summary>
        /// StatusEffect 틱 발생 시 호출 (DoT/Regeneration)
        /// </summary>
        private void OnStatusEffectTick(StatusEffect effect, float tickValue)
        {
            if (effect.Target != gameObject) return;

            // Regeneration (회복)
            if (effect.EffectType == StatusEffectType.Regeneration)
            {
                int healAmount = Mathf.RoundToInt(tickValue);
                currentHp += healAmount;
                currentHp = Mathf.Min(currentHp, MaxHp);

                OnHpChanged?.Invoke(currentHp, MaxHp);
            }
            // Poison, Burn, Bleed (지속 데미지)
            else if (effect.EffectType == StatusEffectType.Poison ||
                     effect.EffectType == StatusEffectType.Burn ||
                     effect.EffectType == StatusEffectType.Bleed)
            {
                int damage = Mathf.RoundToInt(Mathf.Abs(tickValue));
                currentHp -= damage;
                currentHp = Mathf.Max(0, currentHp);

                // DamageNumber 표시
                if (DamageNumberPool.Instance != null)
                {
                    Vector3 damagePosition = transform.position + Vector3.up * 1.5f;
                    DamageNumberPool.Instance.ShowDamage(damage, damagePosition, false);
                }

                // 이벤트 발생
                OnHpChanged?.Invoke(currentHp, MaxHp);

                // 사망 체크
                if (currentHp <= 0)
                {
                    Die();
                }
            }
        }

        /// <summary>
        /// StatusEffect 버프/디버프 적용
        /// </summary>
        private int ApplyStatusEffects(int baseStat, StatusEffectType buffType, StatusEffectType debuffType)
        {
            if (!StatusEffectManager.HasInstance)
            {
                return baseStat;
            }

            float modifier = 0f;

            // 버프 합산
            StatusEffect buffEffect = StatusEffectManager.Instance.GetEffect(gameObject, buffType);
            if (buffEffect != null && buffEffect.IsActive)
            {
                modifier += buffEffect.Value * buffEffect.StackCount;
            }

            // 디버프 합산
            StatusEffect debuffEffect = StatusEffectManager.Instance.GetEffect(gameObject, debuffType);
            if (debuffEffect != null && debuffEffect.IsActive)
            {
                modifier -= debuffEffect.Value * debuffEffect.StackCount;
            }

            int finalValue = baseStat + Mathf.RoundToInt(modifier);
            return Mathf.Max(finalValue, 1); // 최소 1 보장
        }


        // ====== 디버그 ======

        /// <summary>
        /// 현재 적 정보 출력
        /// </summary>
        [ContextMenu("Print Enemy Info")]
        private void DebugPrintEnemyInfo()
        {
            if (enemyData == null)
            {
                Debug.LogWarning("[Enemy] enemyData가 null입니다.");
                return;
            }

            Debug.Log($"[Enemy] ========== {enemyData.enemyName} 정보 ==========");
            Debug.Log($"[Enemy] 타입: {enemyData.enemyType}");
            Debug.Log($"[Enemy] HP: {currentHp}/{enemyData.maxHp}");
            Debug.Log($"[Enemy] 공격력: {enemyData.attack}");
            Debug.Log($"[Enemy] 골드 드롭: {enemyData.minGoldDrop}-{enemyData.maxGoldDrop}");
            Debug.Log($"[Enemy] 사망 여부: {isDead}");
            Debug.Log($"[Enemy] =====================================");
        }

        /// <summary>
        /// 테스트용 데미지 받기 (10 데미지)
        /// </summary>
        [ContextMenu("Take 10 Damage (Test)")]
        private void DebugTakeDamage()
        {
            TakeDamage(10);
        }

        /// <summary>
        /// 테스트용 즉사 (Test)
        /// </summary>
        [ContextMenu("Instant Death (Test)")]
        private void DebugInstantDeath()
        {
            TakeDamage(currentHp);
        }
    }
}
