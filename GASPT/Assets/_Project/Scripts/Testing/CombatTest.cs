using UnityEngine;
using GASPT.Stats;
using GASPT.Gameplay.Enemies;

namespace GASPT.Combat
{
    /// <summary>
    /// Combat 시스템 통합 테스트 스크립트
    /// PlayerStats와 Enemy 간의 전투 시뮬레이션
    /// </summary>
    public class CombatTest : MonoBehaviour
    {
        // ====== 참조 ======

        [Header("Combat 참조")]
        [SerializeField] [Tooltip("플레이어 스탯")]
        private PlayerStats player;

        [SerializeField] [Tooltip("적")]
        private Enemy enemy;


        // ====== Unity 생명주기 ======

        private void Start()
        {
            ValidateReferences();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }


        // ====== 유효성 검증 ======

        private void ValidateReferences()
        {
            if (player == null)
            {
                Debug.LogWarning("[CombatTest] PlayerStats가 설정되지 않았습니다. Inspector에서 설정하세요.");
            }

            if (enemy == null)
            {
                Debug.LogWarning("[CombatTest] Enemy가 설정되지 않았습니다. Inspector에서 설정하세요.");
            }
        }


        // ====== 이벤트 구독 ======

        private void SubscribeToEvents()
        {
            if (player != null)
            {
                player.OnDamaged += OnPlayerDamaged;
                player.OnHealed += OnPlayerHealed;
                player.OnDeath += OnPlayerDeath;
            }

            if (enemy != null)
            {
                enemy.OnHpChanged += OnEnemyHpChanged;
                enemy.OnDeath += OnEnemyDeath;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (player != null)
            {
                player.OnDamaged -= OnPlayerDamaged;
                player.OnHealed -= OnPlayerHealed;
                player.OnDeath -= OnPlayerDeath;
            }

            if (enemy != null)
            {
                enemy.OnHpChanged -= OnEnemyHpChanged;
                enemy.OnDeath -= OnEnemyDeath;
            }
        }


        // ====== 이벤트 핸들러 ======

        private void OnPlayerDamaged(int damage, int currentHP, int maxHP)
        {
            Debug.Log($"[CombatTest] 플레이어 피격! 데미지: {damage}, HP: {currentHP}/{maxHP}");
        }

        private void OnPlayerHealed(int healAmount, int currentHP, int maxHP)
        {
            Debug.Log($"[CombatTest] 플레이어 회복! 회복량: {healAmount}, HP: {currentHP}/{maxHP}");
        }

        private void OnPlayerDeath()
        {
            Debug.Log("[CombatTest] 플레이어 사망!");
        }

        private void OnEnemyHpChanged(int currentHP, int maxHP)
        {
            Debug.Log($"[CombatTest] 적 HP 변경: {currentHP}/{maxHP}");
        }

        private void OnEnemyDeath(Enemy deadEnemy)
        {
            Debug.Log($"[CombatTest] 적 사망: {deadEnemy.name}");
        }


        // ====== Context Menu 테스트 ======

        [ContextMenu("1. Player Attacks Enemy")]
        private void TestPlayerAttacksEnemy()
        {
            if (player == null || enemy == null)
            {
                Debug.LogError("[CombatTest] PlayerStats 또는 Enemy가 설정되지 않았습니다.");
                return;
            }

            Debug.Log("\n========== Player Attacks Enemy ==========");
            Debug.Log($"플레이어 공격력: {player.Attack}");
            Debug.Log($"적 현재 HP: {enemy.CurrentHp}/{enemy.MaxHp}");

            player.DealDamageTo(enemy);

            Debug.Log($"적 남은 HP: {enemy.CurrentHp}/{enemy.MaxHp}");
            Debug.Log("==========================================\n");
        }

        [ContextMenu("2. Enemy Attacks Player")]
        private void TestEnemyAttacksPlayer()
        {
            if (player == null || enemy == null)
            {
                Debug.LogError("[CombatTest] PlayerStats 또는 Enemy가 설정되지 않았습니다.");
                return;
            }

            Debug.Log("\n========== Enemy Attacks Player ==========");
            Debug.Log($"적 공격력: {enemy.Attack}");
            Debug.Log($"플레이어 방어력: {player.Defense}");
            Debug.Log($"플레이어 현재 HP: {player.CurrentHP}/{player.MaxHP}");

            enemy.DealDamageTo(player);

            Debug.Log($"플레이어 남은 HP: {player.CurrentHP}/{player.MaxHP}");
            Debug.Log("==========================================\n");
        }

        [ContextMenu("3. Full Combat Simulation (5 Rounds)")]
        private void TestFullCombatSimulation()
        {
            if (player == null || enemy == null)
            {
                Debug.LogError("[CombatTest] PlayerStats 또는 Enemy가 설정되지 않았습니다.");
                return;
            }

            Debug.Log("\n========== Full Combat Simulation ==========");
            Debug.Log($"시작 상태:");
            Debug.Log($"  플레이어: HP {player.CurrentHP}/{player.MaxHP}, Attack {player.Attack}, Defense {player.Defense}");
            Debug.Log($"  적: HP {enemy.CurrentHp}/{enemy.MaxHp}, Attack {enemy.Attack}");
            Debug.Log("");

            for (int round = 1; round <= 5; round++)
            {
                if (player.IsDead || enemy.IsDead)
                {
                    Debug.Log($"라운드 {round}: 전투 종료 (사망자 발생)");
                    break;
                }

                Debug.Log($"========== 라운드 {round} ==========");

                // 플레이어 공격
                Debug.Log("플레이어 턴:");
                player.DealDamageTo(enemy);

                // 적이 살아있으면 반격
                if (!enemy.IsDead)
                {
                    Debug.Log("적 턴:");
                    enemy.DealDamageTo(player);
                }

                Debug.Log($"라운드 {round} 종료 - 플레이어 HP: {player.CurrentHP}/{player.MaxHP}, 적 HP: {enemy.CurrentHp}/{enemy.MaxHp}");
                Debug.Log("");
            }

            Debug.Log("========== 전투 결과 ==========");
            Debug.Log($"플레이어: {(player.IsDead ? "사망" : $"생존 (HP {player.CurrentHP}/{player.MaxHP})")}");
            Debug.Log($"적: {(enemy.IsDead ? "사망" : $"생존 (HP {enemy.CurrentHp}/{enemy.MaxHp})")}");
            Debug.Log("==========================================\n");
        }

        [ContextMenu("4. Test Damage Calculation")]
        private void TestDamageCalculation()
        {
            if (player == null)
            {
                Debug.LogError("[CombatTest] PlayerStats가 설정되지 않았습니다.");
                return;
            }

            Debug.Log("\n========== Damage Calculation Test ==========");
            Debug.Log($"플레이어 공격력: {player.Attack}");
            Debug.Log($"플레이어 방어력: {player.Defense}");
            Debug.Log("");

            // 공격 데미지 계산 테스트 (10회)
            Debug.Log("공격 데미지 계산 (10회):");
            for (int i = 0; i < 10; i++)
            {
                int damage = DamageCalculator.CalculateDamageDealt(player.Attack);
                Debug.Log($"  시도 {i + 1}: {damage} 데미지");
            }

            Debug.Log("");

            // 방어력 적용 테스트
            Debug.Log("방어력 적용 테스트:");
            int[] incomingDamages = { 5, 10, 20, 30, 50 };

            foreach (int incoming in incomingDamages)
            {
                int received = DamageCalculator.CalculateDamageReceived(incoming, player.Defense);
                Debug.Log($"  들어오는 데미지 {incoming} → 방어력 {player.Defense} 적용 → 받는 데미지 {received}");
            }

            Debug.Log("");
            Debug.Log($"데미지 계산 공식:\n{DamageCalculator.GetFormulaInfo()}");
            Debug.Log("==========================================\n");
        }

        [ContextMenu("5. Print Combat Status")]
        private void PrintCombatStatus()
        {
            Debug.Log("\n========== Combat Status ==========");

            if (player != null)
            {
                Debug.Log($"플레이어:");
                Debug.Log($"  HP: {player.CurrentHP}/{player.MaxHP}");
                Debug.Log($"  Attack: {player.Attack}");
                Debug.Log($"  Defense: {player.Defense}");
                Debug.Log($"  IsDead: {player.IsDead}");
            }
            else
            {
                Debug.Log("플레이어: 설정되지 않음");
            }

            Debug.Log("");

            if (enemy != null)
            {
                Debug.Log($"적:");
                Debug.Log($"  이름: {enemy.Data?.enemyName ?? "없음"}");
                Debug.Log($"  HP: {enemy.CurrentHp}/{enemy.MaxHp}");
                Debug.Log($"  Attack: {enemy.Attack}");
                Debug.Log($"  IsDead: {enemy.IsDead}");
            }
            else
            {
                Debug.Log("적: 설정되지 않음");
            }

            Debug.Log("===================================\n");
        }

        [ContextMenu("6. Reset Combat (Revive All)")]
        private void ResetCombat()
        {
            Debug.Log("\n========== Reset Combat ==========");

            if (player != null && player.IsDead)
            {
                player.Revive();
                Debug.Log("플레이어 부활!");
            }

            if (enemy != null && enemy.IsDead)
            {
                Debug.LogWarning("적은 부활 기능이 없습니다. Scene을 다시 로드하거나 새 적을 생성하세요.");
            }

            Debug.Log("==================================\n");
        }
    }
}
