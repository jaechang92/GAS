using UnityEngine;
using GASPT.Stats;
using GASPT.Gameplay.Enemy;
using GASPT.UI;

namespace GASPT.Tests
{
    /// <summary>
    /// Combat UI (DamageNumber) 시스템 통합 테스트
    /// Context Menu를 통해 다양한 시나리오 테스트
    /// </summary>
    public class CombatUITest : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] [Tooltip("테스트용 플레이어 스탯")]
        private PlayerStats playerStats;

        [SerializeField] [Tooltip("테스트용 적")]
        private Enemy testEnemy;

        [SerializeField] [Tooltip("DamageNumberPool")]
        private DamageNumberPool damageNumberPool;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // 자동 참조 찾기
            if (playerStats == null)
            {
                playerStats = FindAnyObjectByType<PlayerStats>();
            }

            if (testEnemy == null)
            {
                testEnemy = FindAnyObjectByType<Enemy>();
            }

            if (damageNumberPool == null)
            {
                damageNumberPool = DamageNumberPool.Instance;
            }
        }


        // ====== 테스트 시나리오 ======

        /// <summary>
        /// 시나리오 1: 플레이어가 데미지를 받음
        /// </summary>
        [ContextMenu("1. Player Takes Damage")]
        private void Test1_PlayerTakesDamage()
        {
            Debug.Log("========== Test 1: Player Takes Damage ==========");

            if (playerStats == null)
            {
                Debug.LogError("[CombatUITest] PlayerStats를 찾을 수 없습니다.");
                return;
            }

            int damage = 50;
            Debug.Log($"[CombatUITest] 플레이어에게 {damage} 데미지 적용...");

            playerStats.TakeDamage(damage);

            Debug.Log($"[CombatUITest] 현재 HP: {playerStats.CurrentHP}/{playerStats.MaxHP}");
            Debug.Log("========== Test 1 완료 ==========");
        }

        /// <summary>
        /// 시나리오 2: 플레이어가 회복함
        /// </summary>
        [ContextMenu("2. Player Heals")]
        private void Test2_PlayerHeals()
        {
            Debug.Log("========== Test 2: Player Heals ==========");

            if (playerStats == null)
            {
                Debug.LogError("[CombatUITest] PlayerStats를 찾을 수 없습니다.");
                return;
            }

            int healAmount = 30;
            Debug.Log($"[CombatUITest] 플레이어 {healAmount} 회복...");

            playerStats.Heal(healAmount);

            Debug.Log($"[CombatUITest] 현재 HP: {playerStats.CurrentHP}/{playerStats.MaxHP}");
            Debug.Log("========== Test 2 완료 ==========");
        }

        /// <summary>
        /// 시나리오 3: 적이 데미지를 받음
        /// </summary>
        [ContextMenu("3. Enemy Takes Damage")]
        private void Test3_EnemyTakesDamage()
        {
            Debug.Log("========== Test 3: Enemy Takes Damage ==========");

            if (testEnemy == null)
            {
                Debug.LogError("[CombatUITest] testEnemy가 설정되지 않았습니다.");
                return;
            }

            int damage = 100;
            Debug.Log($"[CombatUITest] {testEnemy.Data.enemyName}에게 {damage} 데미지 적용...");

            testEnemy.TakeDamage(damage);

            Debug.Log($"[CombatUITest] 적 HP: {testEnemy.CurrentHp}/{testEnemy.MaxHp}");
            Debug.Log("========== Test 3 완료 ==========");
        }

        /// <summary>
        /// 시나리오 4: 적 처치 시 EXP 표시
        /// </summary>
        [ContextMenu("4. Kill Enemy and Show EXP")]
        private void Test4_KillEnemyShowExp()
        {
            Debug.Log("========== Test 4: Kill Enemy and Show EXP ==========");

            if (testEnemy == null)
            {
                Debug.LogError("[CombatUITest] testEnemy가 설정되지 않았습니다.");
                return;
            }

            Debug.Log($"[CombatUITest] {testEnemy.Data.enemyName} 처치 (EXP: {testEnemy.Data.expReward})...");

            // 적 즉사
            testEnemy.TakeDamage(testEnemy.CurrentHp);

            Debug.Log("========== Test 4 완료 ==========");
        }

        /// <summary>
        /// 시나리오 5: 다중 데미지 숫자 동시 표시
        /// </summary>
        [ContextMenu("5. Multiple Damage Numbers")]
        private void Test5_MultipleDamageNumbers()
        {
            Debug.Log("========== Test 5: Multiple Damage Numbers ==========");

            if (damageNumberPool == null)
            {
                Debug.LogError("[CombatUITest] DamageNumberPool을 찾을 수 없습니다.");
                return;
            }

            Vector3 basePosition = transform.position + Vector3.up * 2f;

            Debug.Log("[CombatUITest] 5개의 데미지 숫자 동시 표시...");

            for (int i = 0; i < 5; i++)
            {
                int damage = Random.Range(50, 150);
                bool isCritical = Random.value > 0.7f;

                Vector3 position = basePosition + Random.insideUnitSphere * 0.5f;
                damageNumberPool.ShowDamage(damage, position, isCritical);
            }

            Debug.Log("========== Test 5 완료 ==========");
        }

        /// <summary>
        /// 시나리오 6: 크리티컬 데미지 표시
        /// </summary>
        [ContextMenu("6. Show Critical Damage")]
        private void Test6_ShowCriticalDamage()
        {
            Debug.Log("========== Test 6: Show Critical Damage ==========");

            if (damageNumberPool == null)
            {
                Debug.LogError("[CombatUITest] DamageNumberPool을 찾을 수 없습니다.");
                return;
            }

            Vector3 position = transform.position + Vector3.up * 2f;
            int criticalDamage = 250;

            Debug.Log($"[CombatUITest] 크리티컬 데미지 표시: {criticalDamage}");

            damageNumberPool.ShowDamage(criticalDamage, position, true);

            Debug.Log("========== Test 6 완료 ==========");
        }

        /// <summary>
        /// 시나리오 7: 전투 시뮬레이션 (Player vs Enemy)
        /// </summary>
        [ContextMenu("7. Combat Simulation")]
        private void Test7_CombatSimulation()
        {
            Debug.Log("========== Test 7: Combat Simulation ==========");

            if (playerStats == null || testEnemy == null)
            {
                Debug.LogError("[CombatUITest] PlayerStats 또는 Enemy를 찾을 수 없습니다.");
                return;
            }

            Debug.Log("[CombatUITest] 전투 시작!");

            // 라운드 1: 플레이어가 적 공격
            Debug.Log("[CombatUITest] 라운드 1: 플레이어 공격");
            playerStats.DealDamageTo(testEnemy);

            // 라운드 2: 적이 플레이어 공격
            Debug.Log("[CombatUITest] 라운드 2: 적 공격");
            if (!testEnemy.IsDead)
            {
                testEnemy.DealDamageTo(playerStats);
            }

            // 라운드 3: 플레이어 회복
            Debug.Log("[CombatUITest] 라운드 3: 플레이어 회복");
            playerStats.Heal(20);

            // 라운드 4: 플레이어가 적 처치
            Debug.Log("[CombatUITest] 라운드 4: 플레이어 마무리 공격");
            if (!testEnemy.IsDead)
            {
                playerStats.DealDamageTo(testEnemy);
            }

            Debug.Log($"[CombatUITest] 전투 종료 - Player HP: {playerStats.CurrentHP}/{playerStats.MaxHP}, Enemy HP: {testEnemy.CurrentHp}/{testEnemy.MaxHp}");
            Debug.Log("========== Test 7 완료 ==========");
        }

        /// <summary>
        /// 시나리오 8: 오브젝트 풀 스트레스 테스트
        /// </summary>
        [ContextMenu("8. Object Pool Stress Test")]
        private void Test8_ObjectPoolStressTest()
        {
            Debug.Log("========== Test 8: Object Pool Stress Test ==========");

            if (damageNumberPool == null)
            {
                Debug.LogError("[CombatUITest] DamageNumberPool을 찾을 수 없습니다.");
                return;
            }

            Vector3 basePosition = transform.position + Vector3.up * 2f;

            Debug.Log("[CombatUITest] 20개의 데미지 숫자 스트레스 테스트...");

            for (int i = 0; i < 20; i++)
            {
                int damage = Random.Range(10, 200);
                bool isCritical = Random.value > 0.8f;

                Vector3 position = basePosition + Random.insideUnitSphere * 2f;
                damageNumberPool.ShowDamage(damage, position, isCritical);
            }

            // 풀 상태 출력
            damageNumberPool.DebugPrintPoolStatus();

            Debug.Log("========== Test 8 완료 ==========");
        }


        // ====== 현재 상태 출력 ======

        /// <summary>
        /// 현재 전투 상태 출력
        /// </summary>
        [ContextMenu("Print Combat Status")]
        private void PrintCombatStatus()
        {
            Debug.Log("========== Combat Status ==========");

            if (playerStats != null)
            {
                Debug.Log($"Player HP: {playerStats.CurrentHP}/{playerStats.MaxHP}");
                Debug.Log($"Player Attack: {playerStats.Attack}");
                Debug.Log($"Player Defense: {playerStats.Defense}");
            }
            else
            {
                Debug.LogWarning("PlayerStats를 찾을 수 없습니다.");
            }

            if (testEnemy != null)
            {
                Debug.Log($"Enemy: {testEnemy.Data.enemyName}");
                Debug.Log($"Enemy HP: {testEnemy.CurrentHp}/{testEnemy.MaxHp}");
                Debug.Log($"Enemy Attack: {testEnemy.Attack}");
                Debug.Log($"Enemy EXP: {testEnemy.Data.expReward}");
                Debug.Log($"Enemy IsDead: {testEnemy.IsDead}");
            }
            else
            {
                Debug.LogWarning("Enemy를 찾을 수 없습니다.");
            }

            if (damageNumberPool != null)
            {
                damageNumberPool.DebugPrintPoolStatus();
            }
            else
            {
                Debug.LogWarning("DamageNumberPool을 찾을 수 없습니다.");
            }

            Debug.Log("===================================");
        }
    }
}
