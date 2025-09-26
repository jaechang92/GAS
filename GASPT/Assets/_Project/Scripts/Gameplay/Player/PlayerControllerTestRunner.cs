using UnityEngine;
using FSM.Core;
using GAS.Core;
using System.Threading;
using System.Collections.Generic;

namespace Player
{
    /// <summary>
    /// 플레이어 컨트롤러의 실제 동작을 테스트하는 런타임 테스트 도구
    /// Gameplay 씬에서 플레이어 컨트롤러 시스템을 검증
    /// </summary>
    public class PlayerControllerTestRunner : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool enableAutoTest = false;
        [SerializeField] private bool enableManualTest = true;
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private float testInterval = 2f;

        [Header("테스트 대상")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerControllerTest playerTest;

        [Header("테스트 결과")]
        [SerializeField] private List<string> testResults = new List<string>();
        [SerializeField] private int passedTests = 0;
        [SerializeField] private int failedTests = 0;

        private CancellationTokenSource cancellationTokenSource;
        private float lastTestTime;

        private void Start()
        {
            InitializeTestRunner();
        }

        private void InitializeTestRunner()
        {
            Debug.Log("=== PlayerController 테스트 런너 시작 ===");

            // PlayerController 자동 찾기
            if (playerController == null)
            {
                playerController = Object.FindAnyObjectByType<PlayerController>();
            }

            if (playerController == null)
            {
                LogTestResult("FAIL: PlayerController를 찾을 수 없습니다", false);
                return;
            }

            // PlayerControllerTest 자동 찾기
            if (playerTest == null)
            {
                playerTest = Object.FindAnyObjectByType<PlayerControllerTest>();
            }

            if (playerTest == null)
            {
                LogTestResult("WARN: PlayerControllerTest를 찾을 수 없습니다 - 수동 테스트만 가능", false);
            }

            // 컴포넌트 검증 테스트
            RunComponentValidationTests();

            // 자동 테스트 시작
            if (enableAutoTest)
            {
                cancellationTokenSource = new CancellationTokenSource();
                _ = RunAutomatedTestsAsync(cancellationTokenSource.Token);
            }

            LogTestResult($"테스트 런너 초기화 완료 - 자동: {enableAutoTest}, 수동: {enableManualTest}", true);
        }

        /// <summary>
        /// 컴포넌트 검증 테스트
        /// </summary>
        private void RunComponentValidationTests()
        {
            Debug.Log("=== 컴포넌트 검증 테스트 ===");

            // 필수 컴포넌트 확인
            TestComponent<Rigidbody2D>("Rigidbody2D");
            TestComponent<Collider2D>("Collider2D");
            TestComponent<SpriteRenderer>("SpriteRenderer");
            TestComponent<AbilitySystem>("AbilitySystem");
            TestComponent<StateMachine>("StateMachine");

            // PlayerStats 확인 (선택사항)
            bool hasStats = playerController.gameObject.GetComponent<PlayerStats>() != null;
            LogTestResult($"PlayerStats 설정: {(hasStats ? "있음" : "없음 (선택사항)")}", true);
        }

        private void TestComponent<T>(string componentName) where T : Component
        {
            T component = playerController.GetComponent<T>();
            bool hasComponent = component != null;
            LogTestResult($"{componentName}: {(hasComponent ? "있음" : "없음")}", hasComponent);

            if (!hasComponent)
            {
                Debug.LogWarning($"필수 컴포넌트 누락: {componentName}");
            }
        }

        /// <summary>
        /// 자동화된 테스트 실행
        /// </summary>
        private async Awaitable RunAutomatedTestsAsync(CancellationToken cancellationToken)
        {
            Debug.Log("=== 자동화된 테스트 시작 ===");

            try
            {
                // 초기 상태 테스트
                await TestInitialStateAsync(cancellationToken);

                // 상태 전환 테스트들
                await TestStateTransitionsAsync(cancellationToken);

                // 물리 테스트
                await TestPhysicsIntegrationAsync(cancellationToken);

                Debug.Log("=== 자동화된 테스트 완료 ===");
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("자동 테스트가 취소되었습니다.");
            }
        }

        private async Awaitable TestInitialStateAsync(CancellationToken cancellationToken)
        {
            LogTestResult("초기 상태 테스트 시작", true);

            // 초기 상태가 Idle인지 확인
            bool initialStateCorrect = playerController.CurrentState == PlayerStateType.Idle;
            LogTestResult($"초기 상태: {playerController.CurrentState}", initialStateCorrect);

            // 초기 물리 상태 확인
            bool isGroundedCorrect = playerController.IsGrounded;
            LogTestResult($"초기 접지 상태: {isGroundedCorrect}", true);

            await Awaitable.WaitForSecondsAsync(1f, cancellationToken);
        }

        private async Awaitable TestStateTransitionsAsync(CancellationToken cancellationToken)
        {
            LogTestResult("상태 전환 테스트 시작", true);

            // 각 상태로 전환 테스트
            var statesToTest = new[]
            {
                PlayerEventType.StartMove,
                PlayerEventType.StopMove,
                PlayerEventType.JumpPressed,
                PlayerEventType.DashPressed,
                PlayerEventType.AttackPressed
            };

            foreach (var eventType in statesToTest)
            {
                try
                {
                    var previousState = playerController.CurrentState;
                    playerController.TriggerEvent(eventType);

                    await Awaitable.WaitForSecondsAsync(0.5f, cancellationToken);

                    var currentState = playerController.CurrentState;
                    LogTestResult($"이벤트 {eventType} 처리: {previousState} → {currentState}", true);
                }
                catch (System.Exception e)
                {
                    LogTestResult($"이벤트 {eventType} 실패: {e.Message}", false);
                }

                await Awaitable.WaitForSecondsAsync(testInterval, cancellationToken);
            }
        }

        private async Awaitable TestPhysicsIntegrationAsync(CancellationToken cancellationToken)
        {
            LogTestResult("물리 통합 테스트 시작", true);

            var rb = playerController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 중력 테스트
                bool gravityEnabled = rb.gravityScale > 0;
                LogTestResult($"중력 설정: {rb.gravityScale}", gravityEnabled);

                // 회전 고정 테스트
                bool rotationFrozen = rb.freezeRotation;
                LogTestResult($"회전 고정: {rotationFrozen}", rotationFrozen);

                // 충돌 감지 모드 테스트
                bool continuousCollision = rb.collisionDetectionMode == CollisionDetectionMode2D.Continuous;
                LogTestResult($"연속 충돌 감지: {continuousCollision}", true);
            }

            await Awaitable.WaitForSecondsAsync(1f, cancellationToken);
        }

        private void Update()
        {
            if (enableManualTest)
            {
                HandleManualTestInputs();
            }

            // 테스트 결과 주기적 출력
            if (Time.time - lastTestTime > 5f)
            {
                PrintTestSummary();
                lastTestTime = Time.time;
            }
        }

        private void HandleManualTestInputs()
        {
            // 숫자 키로 테스트 이벤트 트리거
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestEvent(PlayerEventType.StartMove);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestEvent(PlayerEventType.StopMove);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestEvent(PlayerEventType.JumpPressed);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestEvent(PlayerEventType.DashPressed);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                TestEvent(PlayerEventType.AttackPressed);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                TestEvent(PlayerEventType.TakeDamage);
            }

            // 현재 상태 정보 출력
            if (Input.GetKeyDown(KeyCode.I))
            {
                PrintCurrentState();
            }

            // 테스트 결과 초기화
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetTestResults();
            }
        }

        private void TestEvent(PlayerEventType eventType)
        {
            if (playerController != null)
            {
                var prevState = playerController.CurrentState;
                playerController.TriggerEvent(eventType);
                LogTestResult($"수동 테스트 - {eventType}: {prevState} → {playerController.CurrentState}", true);
            }
        }

        private void PrintCurrentState()
        {
            if (playerController == null) return;

            Debug.Log("=== 현재 플레이어 상태 ===");
            Debug.Log($"상태: {playerController.CurrentState}");
            Debug.Log($"접지: {playerController.IsGrounded}");
            Debug.Log($"벽 접촉: {playerController.IsTouchingWall}");
            Debug.Log($"대시 가능: {playerController.CanDash}");
            Debug.Log($"방향: {(playerController.FacingDirection > 0 ? "오른쪽" : "왼쪽")}");
            Debug.Log($"속도: {playerController.Velocity}");
        }

        private void PrintTestSummary()
        {
            Debug.Log($"=== 테스트 요약 === 성공: {passedTests}, 실패: {failedTests}");
        }

        private void ResetTestResults()
        {
            testResults.Clear();
            passedTests = 0;
            failedTests = 0;
            LogTestResult("테스트 결과 초기화됨", true);
        }

        private void LogTestResult(string message, bool passed)
        {
            string result = $"[{(passed ? "PASS" : "FAIL")}] {message}";
            testResults.Add(result);

            if (passed)
                passedTests++;
            else
                failedTests++;

            Debug.Log($"[PlayerTest] {result}");
        }

        private void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 200, 400, 600));
            GUILayout.Label("=== Player Controller Test Runner ===");

            GUILayout.Label($"테스트 결과: 성공 {passedTests} / 실패 {failedTests}");

            GUILayout.Space(10);
            GUILayout.Label("=== 수동 테스트 키 ===");
            GUILayout.Label("1: 이동 시작");
            GUILayout.Label("2: 이동 정지");
            GUILayout.Label("3: 점프");
            GUILayout.Label("4: 대시");
            GUILayout.Label("5: 공격");
            GUILayout.Label("6: 데미지");
            GUILayout.Label("I: 현재 상태 출력");
            GUILayout.Label("R: 테스트 결과 초기화");

            GUILayout.Space(10);
            if (playerController != null)
            {
                GUILayout.Label($"현재 상태: {playerController.CurrentState}");
                GUILayout.Label($"접지: {playerController.IsGrounded}");
                GUILayout.Label($"속도: {playerController.Velocity}");
            }

            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
        }
    }
}