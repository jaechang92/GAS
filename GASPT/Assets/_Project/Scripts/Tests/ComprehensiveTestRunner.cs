using UnityEngine;
using Player;
using Enemy;
using GameFlow;
using Combat.Core;
using Enemy.Data;
using System.Threading;

namespace Tests
{
    /// <summary>
    /// 이전 작업들을 종합적으로 테스트하는 러너
    /// - Player/Enemy State 동기 전환 테스트
    /// - GameFlow 하이브리드 FSM 테스트
    /// - Combat 시스템 통합 테스트
    /// </summary>
    public class ComprehensiveTestRunner : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private float testInterval = 2f;

        [Header("테스트 대상")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private EnemyData testEnemyData;

        private PlayerController player;
        private EnemyController enemy;
        private GameFlowManager gameFlowManager;

        private int passedTests = 0;
        private int totalTests = 0;

        private void Start()
        {
            if (runOnStart)
            {
                _ = RunAllTests();
            }
        }

        private async Awaitable RunAllTests()
        {
            Debug.Log("========================================");
            Debug.Log("=== 종합 테스트 시작 ===");
            Debug.Log("========================================");

            // 1. Player State 시스템 테스트
            await TestPlayerStateSystem();

            // 2. Enemy State 시스템 테스트
            await TestEnemyStateSystem();

            // 3. GameFlow 하이브리드 FSM 테스트
            await TestGameFlowSystem();

            // 4. Combat 시스템 통합 테스트
            await TestCombatSystem();

            // 테스트 결과 출력
            PrintTestResults();

            Debug.Log("========================================");
            Debug.Log("=== 종합 테스트 완료 ===");
            Debug.Log("========================================");
        }

        #region Player State System Tests

        private async Awaitable TestPlayerStateSystem()
        {
            Debug.Log("\n[1] Player State 시스템 테스트 시작");

            // Player 생성
            if (playerPrefab != null)
            {
                GameObject playerGO = Instantiate(playerPrefab, new Vector3(-5, 0, 0), Quaternion.identity);
                player = playerGO.GetComponent<PlayerController>();
            }
            else
            {
                GameObject playerGO = new GameObject("TestPlayer");
                player = playerGO.AddComponent<PlayerController>();
                playerGO.AddComponent<Rigidbody2D>();
                playerGO.AddComponent<BoxCollider2D>();
            }

            await Awaitable.WaitForSecondsAsync(1f);

            // 테스트 1: 초기 상태 확인
            TestAssert(player.CurrentState == PlayerStateType.Idle, "Player 초기 상태는 Idle");

            // 테스트 2: Idle → Move 전환 (수동 트리거)
            player.TriggerEvent(PlayerEventType.StartMove);
            await Awaitable.WaitForSecondsAsync(0.5f);

            // 테스트 3: Move → Jump 전환
            player.TriggerEvent(PlayerEventType.JumpPressed);
            await Awaitable.WaitForSecondsAsync(0.5f);

            // 테스트 4: Attack 상태 전환
            player.ChangeState(PlayerStateType.Attack);
            await Awaitable.WaitForSecondsAsync(0.5f);
            TestAssert(player.CurrentState == PlayerStateType.Attack, "Player Attack 상태 전환");

            // 테스트 5: Idle로 복귀
            player.ChangeState(PlayerStateType.Idle);
            await Awaitable.WaitForSecondsAsync(0.5f);
            TestAssert(player.CurrentState == PlayerStateType.Idle, "Player Idle 상태 복귀");

            Debug.Log("[1] Player State 시스템 테스트 완료\n");
        }

        #endregion

        #region Enemy State System Tests

        private async Awaitable TestEnemyStateSystem()
        {
            Debug.Log("\n[2] Enemy State 시스템 테스트 시작");

            // Enemy 생성
            if (enemyPrefab != null)
            {
                GameObject enemyGO = Instantiate(enemyPrefab, new Vector3(5, 0, 0), Quaternion.identity);
                enemy = enemyGO.GetComponent<EnemyController>();
            }
            else
            {
                GameObject enemyGO = new GameObject("TestEnemy");
                enemy = enemyGO.AddComponent<EnemyController>();
                enemyGO.AddComponent<Rigidbody2D>();
                enemyGO.AddComponent<BoxCollider2D>();

                // EnemyData 설정
                if (testEnemyData != null)
                {
                    var dataField = typeof(EnemyController).GetField("enemyData",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    dataField?.SetValue(enemy, testEnemyData);
                }
            }

            // Player를 타겟으로 설정
            if (player != null)
            {
                enemy.Target = player.transform;
            }

            await Awaitable.WaitForSecondsAsync(1f);

            // 테스트 6: Enemy 초기 상태 확인
            TestAssert(enemy != null, "Enemy 생성 성공");

            // 테스트 7: Idle → Patrol 전환
            enemy.ChangeState(EnemyStateType.Patrol);
            await Awaitable.WaitForSecondsAsync(0.5f);

            // 테스트 8: Patrol → Trace 전환
            enemy.ChangeState(EnemyStateType.Trace);
            await Awaitable.WaitForSecondsAsync(0.5f);

            // 테스트 9: Trace → Attack 전환
            enemy.ChangeState(EnemyStateType.Attack);
            await Awaitable.WaitForSecondsAsync(0.5f);

            // 테스트 10: Idle로 복귀
            enemy.ChangeState(EnemyStateType.Idle);
            await Awaitable.WaitForSecondsAsync(0.5f);

            Debug.Log("[2] Enemy State 시스템 테스트 완료\n");
        }

        #endregion

        #region GameFlow System Tests

        private async Awaitable TestGameFlowSystem()
        {
            Debug.Log("\n[3] GameFlow 하이브리드 FSM 테스트 시작");

            // GameFlow 매니저 찾기 또는 생성
            gameFlowManager = Object.FindAnyObjectByType<GameFlowManager>();

            if (gameFlowManager == null)
            {
                GameObject gameFlowGO = new GameObject("GameFlowManager");
                gameFlowManager = gameFlowGO.AddComponent<GameFlowManager>();
            }

            await Awaitable.WaitForSecondsAsync(1f);

            // 테스트 11: GameFlow 초기 상태 확인
            TestAssert(gameFlowManager != null, "GameFlowManager 생성 성공");

            // 테스트 12: Main → Loading 전환
            gameFlowManager.TransitionTo(GameStateType.Main);
            await Awaitable.WaitForSecondsAsync(0.5f);
            gameFlowManager.StartGame();
            await Awaitable.WaitForSecondsAsync(0.5f);

            // 테스트 13: Loading → Ingame 전환 (자동)
            await Awaitable.WaitForSecondsAsync(6f);

            // 테스트 14: Ingame → Pause 전환
            gameFlowManager.PauseGame();
            await Awaitable.WaitForSecondsAsync(0.5f);

            // 테스트 15: Pause → Ingame 전환
            gameFlowManager.ResumeGame();
            await Awaitable.WaitForSecondsAsync(0.5f);

            Debug.Log("[3] GameFlow 하이브리드 FSM 테스트 완료\n");
        }

        #endregion

        #region Combat System Tests

        private async Awaitable TestCombatSystem()
        {
            Debug.Log("\n[4] Combat 시스템 통합 테스트 시작");

            if (player == null || enemy == null)
            {
                Debug.LogWarning("Player 또는 Enemy가 없어서 Combat 테스트를 건너뜁니다.");
                return;
            }

            // 테스트 16: Player 체력 시스템 확인
            TestAssert(player.HealthSystem != null, "Player HealthSystem 존재");

            // 테스트 17: Enemy 체력 시스템 확인
            TestAssert(enemy.Health != null, "Enemy HealthSystem 존재");

            // 테스트 18: Player → Enemy 데미지
            float enemyHealthBefore = enemy.Health.CurrentHealth;
            DamageData testDamage = new DamageData();
            testDamage.amount = 20f;
            testDamage.source = player.gameObject;
            enemy.Health.TakeDamage(testDamage);
            await Awaitable.WaitForSecondsAsync(0.5f);
            TestAssert(enemy.Health.CurrentHealth < enemyHealthBefore, "Enemy 데미지 받음");

            // 테스트 19: Enemy → Player 데미지
            float playerHealthBefore = player.HealthSystem.CurrentHealth;
            testDamage = new DamageData();
            testDamage.amount = 15f;
            testDamage.source = enemy.gameObject;
            player.HealthSystem.TakeDamage(testDamage);
            await Awaitable.WaitForSecondsAsync(0.5f);
            TestAssert(player.HealthSystem.CurrentHealth < playerHealthBefore, "Player 데미지 받음");

            // 테스트 20: 사망 테스트 (Enemy)
            enemy.Health.TakeDamage(DamageData.Create(9999,Core.Enums.DamageType.True,player.gameObject));
            await Awaitable.WaitForSecondsAsync(0.5f); 
            TestAssert(!enemy.IsAlive, "Enemy 사망 처리");

            Debug.Log("[4] Combat 시스템 통합 테스트 완료\n");
        }

        #endregion

        #region Test Utilities

        private void TestAssert(bool condition, string testName)
        {
            totalTests++;
            if (condition)
            {
                passedTests++;
                Debug.Log($"✓ [{totalTests}] {testName} - 성공");
            }
            else
            {
                Debug.LogError($"✗ [{totalTests}] {testName} - 실패");
            }
        }

        private void PrintTestResults()
        {
            Debug.Log("\n========================================");
            Debug.Log("=== 테스트 결과 ===");
            Debug.Log($"총 테스트: {totalTests}");
            Debug.Log($"성공: {passedTests}");
            Debug.Log($"실패: {totalTests - passedTests}");
            Debug.Log($"성공률: {(float)passedTests / totalTests * 100:F1}%");
            Debug.Log("========================================\n");
        }

        #endregion

        #region Context Menu

        [ContextMenu("전체 테스트 실행")]
        public void RunTests()
        {
            _ = RunAllTests();
        }

        [ContextMenu("Player 테스트만 실행")]
        public void RunPlayerTests()
        {
            _ = TestPlayerStateSystem();
        }

        [ContextMenu("Enemy 테스트만 실행")]
        public void RunEnemyTests()
        {
            _ = TestEnemyStateSystem();
        }

        [ContextMenu("GameFlow 테스트만 실행")]
        public void RunGameFlowTests()
        {
            _ = TestGameFlowSystem();
        }

        [ContextMenu("Combat 테스트만 실행")]
        public void RunCombatTests()
        {
            _ = TestCombatSystem();
        }

        #endregion
    }
}
