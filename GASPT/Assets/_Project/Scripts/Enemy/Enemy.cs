using System;
using UnityEngine;
using GASPT.Data;
using GASPT.Economy;
using Core.Enums;

namespace GASPT.Enemy
{
    /// <summary>
    /// 적 MonoBehaviour
    /// EnemyData를 기반으로 적의 스탯과 행동을 관리
    /// </summary>
    public class Enemy : MonoBehaviour
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
        /// 공격력 읽기 전용 프로퍼티
        /// </summary>
        public int Attack => enemyData != null ? enemyData.attack : 0;

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
            ValidateEnemyData();
            Initialize();
        }


        // ====== 초기화 ======

        /// <summary>
        /// 적 초기화 (EnemyData 기반)
        /// </summary>
        private void Initialize()
        {
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

            // 사망 이벤트 발생
            OnDeath?.Invoke(this);

            // GameObject 파괴 (1초 후 - 사망 애니메이션용)
            Destroy(gameObject, 1f);
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
