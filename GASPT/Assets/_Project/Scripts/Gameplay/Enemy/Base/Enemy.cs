using System;
using UnityEngine;
using GASPT.Data;
using GASPT.Core.Pooling;
using GASPT.Core.Enums;

namespace GASPT.Gameplay.Enemies
{
    /// <summary>
    /// 적 MonoBehaviour (추상 클래스)
    /// EnemyData를 기반으로 적의 스탯과 행동을 관리
    /// 오브젝트 풀링 지원
    ///
    /// ⚠️ 주의: 이 클래스는 직접 Component로 추가할 수 없습니다.
    /// PlatformerEnemy, BasicMeleeEnemy 등 상속받은 클래스를 사용하세요.
    ///
    /// 파일 분할:
    /// - Enemy.cs: 필드, 프로퍼티, 생명주기, 초기화
    /// - Enemy.Combat.cs: 전투 (데미지, 공격) + 보상 (골드, 경험치, 아이템, 메타재화)
    /// - Enemy.StatusEffect.cs: StatusEffect 통합 + IPoolable 구현 + 디버그
    /// </summary>
    [RequireComponent(typeof(PooledObject))]
    public abstract partial class Enemy : MonoBehaviour, IPoolable
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


        // ====== 스케일링된 스탯 (MonsterSpawnManager에서 설정) ======

        private int scaledMaxHp = -1;      // -1 = 미설정 (EnemyData 기본값 사용)
        private int scaledAttack = -1;
        private int scaledMinGold = -1;
        private int scaledMaxGold = -1;
        private int scaledExp = -1;


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
        public int Attack => GetAttackWithStatusEffects();

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
    }
}
