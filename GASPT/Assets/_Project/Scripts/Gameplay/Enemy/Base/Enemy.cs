using System;
using UnityEngine;
using GASPT.Data;
using GASPT.Economy;
using GASPT.Combat;
using GASPT.Level;
using GASPT.UI;
using GASPT.StatusEffects;
using GASPT.Core.Pooling;
using Core.Enums;

namespace GASPT.Gameplay.Enemy
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
        /// 최대 HP 읽기 전용 프로퍼티
        /// </summary>
        public int MaxHp => enemyData != null ? enemyData.maxHp : 0;

        /// <summary>
        /// 공격력 읽기 전용 프로퍼티 (버프/디버프 적용)
        /// </summary>
        public int Attack
        {
            get
            {
                int baseAttack = enemyData != null ? enemyData.attack : 0;
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

            Debug.Log($"[Enemy] {enemyData.enemyName} 외부 초기화 완료");
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

            Debug.Log($"[Enemy] {enemyData.enemyName} 초기화 완료: HP={currentHp}/{enemyData.maxHp}, Attack={enemyData.attack}");
        }


        // ====== 데미지 처리 ======

        /// <summary>
        /// 데미지 받기
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
            int previousHp = currentHp;
            currentHp -= damage;
            currentHp = Mathf.Max(0, currentHp);

            Debug.Log($"[Enemy] {enemyData.enemyName}: {damage} 데미지 받음 ({previousHp} → {currentHp})");

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

            Debug.Log($"[Enemy] {enemyData.enemyName} 사망!");

            // 골드 드롭
            DropGold();

            // 경험치 지급
            GiveExp();

            // 아이템 드롭
            DropLoot();

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

            Debug.Log($"[Enemy] {enemyData.enemyName}이(가) 플레이어를 공격! 공격력 {Attack} → 데미지 {damage}");

            // 플레이어에게 데미지 적용
            target.TakeDamage(damage);
        }


        // ====== 골드 드롭 ======

        /// <summary>
        /// 골드 드롭 처리
        /// </summary>
        private void DropGold()
        {
            if (enemyData == null) return;

            // 랜덤 골드 계산
            int goldDrop = enemyData.GetRandomGoldDrop();

            // CurrencySystem에 골드 추가
            CurrencySystem currencySystem = CurrencySystem.Instance;

            if (currencySystem != null)
            {
                currencySystem.AddGold(goldDrop);
                Debug.Log($"[Enemy] {enemyData.enemyName} 골드 드롭: {goldDrop} 골드");
            }
            else
            {
                Debug.LogError($"[Enemy] CurrencySystem을 찾을 수 없습니다. 골드 드롭 실패: {goldDrop}");
            }
        }


        // ====== 경험치 지급 ======

        /// <summary>
        /// 경험치 지급 처리
        /// </summary>
        private void GiveExp()
        {
            if (enemyData == null) return;

            // PlayerLevel에 경험치 추가
            PlayerLevel playerLevel = PlayerLevel.Instance;

            if (playerLevel != null)
            {
                playerLevel.AddExp(enemyData.expReward);
                Debug.Log($"[Enemy] {enemyData.enemyName} EXP 지급: {enemyData.expReward} EXP");

                // EXP Number 표시
                if (DamageNumberPool.Instance != null)
                {
                    Vector3 expPosition = transform.position + Vector3.up * 2f;
                    DamageNumberPool.Instance.ShowExp(enemyData.expReward, expPosition);
                }
            }
            else
            {
                Debug.LogError($"[Enemy] PlayerLevel을 찾을 수 없습니다. EXP 지급 실패: {enemyData.expReward}");
            }
        }


        // ====== 아이템 드롭 ======

        /// <summary>
        /// 아이템 드롭 처리
        /// </summary>
        private void DropLoot()
        {
            if (enemyData == null || enemyData.lootTable == null)
            {
                // LootTable이 없으면 아이템 드롭 없음
                return;
            }

            // LootSystem에 드롭 요청
            if (GASPT.Loot.LootSystem.HasInstance)
            {
                GASPT.Loot.LootSystem.Instance.DropLoot(enemyData.lootTable, transform.position);
                Debug.Log($"[Enemy] {enemyData.enemyName} 아이템 드롭 시도");
            }
            else
            {
                Debug.LogError("[Enemy] LootSystem을 찾을 수 없습니다. 아이템 드롭 실패");
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

            Debug.Log($"[Enemy] {enemyData?.enemyName} 풀에서 스폰");
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

            Debug.Log($"[Enemy] {enemyData?.enemyName} 풀로 반환");
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
                if (this is GASPT.Gameplay.Enemy.BasicMeleeEnemy basicMelee)
                {
                    PoolManager.Instance.Despawn(basicMelee);
                }
                else if (this is GASPT.Gameplay.Enemy.RangedEnemy rangedEnemy)
                {
                    PoolManager.Instance.Despawn(rangedEnemy);
                }
                else if (this is GASPT.Gameplay.Enemy.FlyingEnemy flyingEnemy)
                {
                    PoolManager.Instance.Despawn(flyingEnemy);
                }
                else if (this is GASPT.Gameplay.Enemy.EliteEnemy eliteEnemy)
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

                Debug.Log($"[Enemy] {enemyData?.enemyName} StatusEffectManager 이벤트 구독 완료");
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

                Debug.Log($"[Enemy] {enemyData?.enemyName} StatusEffectManager 이벤트 구독 해제");
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

            Debug.Log($"[Enemy] {enemyData.enemyName} StatusEffect 적용: {effect.DisplayName}");
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

            Debug.Log($"[Enemy] {enemyData.enemyName} StatusEffect 제거: {effect.DisplayName}");
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
                int previousHp = currentHp;
                currentHp += healAmount;
                currentHp = Mathf.Min(currentHp, MaxHp);

                int actualHealed = currentHp - previousHp;

                Debug.Log($"[Enemy] {enemyData.enemyName} 회복: {actualHealed} (HP {previousHp} → {currentHp})");

                OnHpChanged?.Invoke(currentHp, MaxHp);
            }
            // Poison, Burn, Bleed (지속 데미지)
            else if (effect.EffectType == StatusEffectType.Poison ||
                     effect.EffectType == StatusEffectType.Burn ||
                     effect.EffectType == StatusEffectType.Bleed)
            {
                int damage = Mathf.RoundToInt(Mathf.Abs(tickValue));
                int previousHp = currentHp;
                currentHp -= damage;
                currentHp = Mathf.Max(0, currentHp);

                Debug.Log($"[Enemy] {enemyData.enemyName} {effect.DisplayName} 틱 데미지: {damage} (HP {previousHp} → {currentHp})");

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
