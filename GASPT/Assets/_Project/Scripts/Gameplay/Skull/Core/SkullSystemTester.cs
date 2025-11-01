using UnityEngine;
using Skull.Core;
using Gameplay.Common;
using System.Collections.Generic;

namespace Skull.Testing
{
    /// <summary>
    /// 스컬 시스템 테스터
    /// 스컬 시스템의 모든 기능을 체계적으로 테스트
    /// </summary>
    public class SkullSystemTester : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private float testInterval = 2f;
        [SerializeField] private bool enableAutomaticTesting = false;

        [Header("테스트 대상")]
        [SerializeField] private SkullSystem skullSystem;
        [SerializeField] private SkullManager skullManager;
        [SerializeField] private SkullGASBridge gasBridge;

        [Header("테스트 데이터")]
        [SerializeField] private SkullData[] testSkullData;

        // 테스트 상태
        private bool isTestingRunning = false;
        private List<TestResult> testResults = new List<TestResult>();
        private int currentTestIndex = 0;

        // 테스트 결과 구조체
        [System.Serializable]
        public struct TestResult
        {
            public string testName;
            public bool passed;
            public float executionTime;
            public string errorMessage;
            public System.DateTime timestamp;
        }

        #region Unity 생명주기

        private void Start()
        {
            InitializeTester();

            if (runTestsOnStart)
            {
                _ = RunAllTestsCoroutine();
            }

            if (enableAutomaticTesting)
            {
                _ = AutomaticTestingLoop();
            }
        }

        private void Update()
        {
            HandleTestInput();
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 테스터 초기화
        /// </summary>
        private void InitializeTester()
        {
            // 컴포넌트 자동 찾기
            if (skullSystem == null)
                skullSystem = FindAnyObjectByType<SkullSystem>();

            if (skullManager == null)
                skullManager = FindAnyObjectByType<SkullManager>();

            if (gasBridge == null)
                gasBridge = FindAnyObjectByType<SkullGASBridge>();

            // 테스트 결과 초기화
            testResults.Clear();

            LogTest("스컬 시스템 테스터 초기화 완료");
        }

        #endregion

        #region 입력 처리

        /// <summary>
        /// 테스트 입력 처리
        /// </summary>
        private void HandleTestInput()
        {
            if (isTestingRunning) return;

            // 개별 테스트 실행
            if (Input.GetKeyDown(KeyCode.F1)) _ = TestSkullSwitching();
            if (Input.GetKeyDown(KeyCode.F2)) _ = TestSkullAbilities();
            if (Input.GetKeyDown(KeyCode.F3)) _ = TestGASIntegration();
            if (Input.GetKeyDown(KeyCode.F4)) _ = TestSkullThrow();
            if (Input.GetKeyDown(KeyCode.F5)) _ = TestSystemStability();

            // 전체 테스트 실행
            if (Input.GetKeyDown(KeyCode.F10)) _ = RunAllTestsCoroutine();

            // 테스트 결과 출력
            if (Input.GetKeyDown(KeyCode.F11)) PrintTestResults();

            // 테스트 결과 초기화
            if (Input.GetKeyDown(KeyCode.F12)) ClearTestResults();
        }

        #endregion

        #region 전체 테스트

        /// <summary>
        /// 모든 테스트 실행
        /// </summary>
        public async Awaitable RunAllTestsCoroutine()
        {
            if (isTestingRunning) return;

            isTestingRunning = true;
            LogTest("=== 스컬 시스템 전체 테스트 시작 ===");

            await TestSkullSwitching();
            await Awaitable.WaitForSecondsAsync(testInterval);

            await TestSkullAbilities();
            await Awaitable.WaitForSecondsAsync(testInterval);

            await TestGASIntegration();
            await Awaitable.WaitForSecondsAsync(testInterval);

            await TestSkullThrow();
            await Awaitable.WaitForSecondsAsync(testInterval);

            await TestSystemStability();
            await Awaitable.WaitForSecondsAsync(testInterval);

            LogTest("=== 전체 테스트 완료 ===");
            PrintTestSummary();

            isTestingRunning = false;
        }

        /// <summary>
        /// 자동 테스트 루프
        /// </summary>
        private async Awaitable AutomaticTestingLoop()
        {
            while (enableAutomaticTesting)
            {
                await Awaitable.WaitForSecondsAsync(30f); // 30초마다 자동 테스트

                if (!isTestingRunning)
                {
                    LogTest("자동 테스트 실행");
                    await RunAllTestsCoroutine();
                }
            }
        }

        #endregion

        #region 개별 테스트

        /// <summary>
        /// 스컬 교체 테스트
        /// </summary>
        public async Awaitable TestSkullSwitching()
        {
            var result = StartTest("스컬 교체 테스트");

            try
            {
                LogTest("--- 스컬 교체 테스트 시작 ---");

                if (skullManager == null)
                {
                    throw new System.Exception("SkullManager가 없습니다.");
                }

                // 초기 스컬 확인
                var initialSkull = skullManager.CurrentSkull;
                LogTest($"초기 스컬: {initialSkull?.SkullData?.SkullName ?? "없음"}");

                // 다음 스컬로 교체 테스트
                LogTest("다음 스컬로 교체 테스트");
                await skullManager.SwitchToNextSlot();
                await Awaitable.WaitForSecondsAsync(1f);

                var nextSkull = skullManager.CurrentSkull;
                LogTest($"교체 후 스컬: {nextSkull?.SkullData?.SkullName ?? "없음"}");

                // 이전 스컬로 교체 테스트
                LogTest("이전 스컬로 교체 테스트");
                await skullManager.SwitchToPreviousSlot();
                await Awaitable.WaitForSecondsAsync(1f);

                var prevSkull = skullManager.CurrentSkull;
                LogTest($"재교체 후 스컬: {prevSkull?.SkullData?.SkullName ?? "없음"}");

                // 교체 가능 상태 테스트
                bool canSwitch = skullManager.CanSwitch;
                LogTest($"교체 가능 상태: {canSwitch}");

                CompleteTest(result, true);
            }
            catch (System.Exception e)
            {
                CompleteTest(result, false, e.Message);
            }
        }

        /// <summary>
        /// 스컬 어빌리티 테스트
        /// </summary>
        public async Awaitable TestSkullAbilities()
        {
            var result = StartTest("스컬 어빌리티 테스트");

            try
            {
                LogTest("--- 스컬 어빌리티 테스트 시작 ---");

                var currentSkull = skullManager?.CurrentSkull;
                if (currentSkull == null)
                {
                    throw new System.Exception("활성화된 스컬이 없습니다.");
                }

                LogTest($"테스트 대상 스컬: {currentSkull.SkullData?.SkullName}");

                // 기본 공격 테스트
                LogTest("기본 공격 테스트");
                await currentSkull.PerformPrimaryAttack();
                await Awaitable.WaitForSecondsAsync(0.5f);

                // 보조 공격 테스트
                LogTest("보조 공격 테스트");
                await currentSkull.PerformSecondaryAttack();
                await Awaitable.WaitForSecondsAsync(0.5f);

                // 궁극기 테스트
                LogTest("궁극기 테스트");
                await currentSkull.PerformUltimate();
                await Awaitable.WaitForSecondsAsync(0.5f);

                // 스컬 상태 확인
                var status = currentSkull.GetStatus();
                LogTest($"스컬 상태: 준비={status.isReady}, 쿨다운={status.cooldownRemaining:F1}초");

                CompleteTest(result, true);
            }
            catch (System.Exception e)
            {
                CompleteTest(result, false, e.Message);
            }
        }

        /// <summary>
        /// GAS 통합 테스트
        /// </summary>
        public async Awaitable TestGASIntegration()
        {
            var result = StartTest("GAS 통합 테스트");

            try
            {
                LogTest("--- GAS 통합 테스트 시작 ---");

                if (gasBridge == null)
                {
                    throw new System.Exception("SkullGASBridge가 없습니다.");
                }

                var abilitySystem = gasBridge.GetAbilitySystem();
                if (abilitySystem == null)
                {
                    throw new System.Exception("AbilitySystem이 연결되지 않았습니다.");
                }

                // 리소스 동기화 테스트
                LogTest("리소스 동기화 테스트");
                float health = abilitySystem.GetResource("Health");
                float mana = abilitySystem.GetResource("Mana");
                LogTest($"현재 리소스: HP={health}, MP={mana}");

                // 스컬 교체 후 리소스 변화 확인
                var currentSkull = skullManager?.CurrentSkull;
                if (currentSkull != null)
                {
                    LogTest("스컬 교체 후 리소스 확인");
                    await skullManager.SwitchToNextSlot();
                    await Awaitable.WaitForSecondsAsync(1f);

                    float newHealth = abilitySystem.GetResource("Health");
                    float newMana = abilitySystem.GetResource("Mana");
                    LogTest($"교체 후 리소스: HP={newHealth}, MP={newMana}");
                }

                // 수동 동기화 테스트
                LogTest("수동 동기화 테스트");
                gasBridge.ManualSync();

                CompleteTest(result, true);
            }
            catch (System.Exception e)
            {
                CompleteTest(result, false, e.Message);
            }
        }

        /// <summary>
        /// 스컬 던지기 테스트
        /// </summary>
        public async Awaitable TestSkullThrow()
        {
            var result = StartTest("스컬 던지기 테스트");

            try
            {
                LogTest("--- 스컬 던지기 테스트 시작 ---");

                var currentSkull = skullManager?.CurrentSkull;
                if (currentSkull == null)
                {
                    throw new System.Exception("활성화된 스컬이 없습니다.");
                }

                Vector3 initialPosition = transform.position;
                LogTest($"초기 위치: {initialPosition}");

                // 스컬 던지기 실행
                Vector2 throwDirection = Vector2.right;
                LogTest($"던지기 방향: {throwDirection}");

                await currentSkull.PerformSkullThrow(throwDirection);
                await Awaitable.WaitForSecondsAsync(2f);

                Vector3 finalPosition = transform.position;
                LogTest($"최종 위치: {finalPosition}");

                float distance = Vector3.Distance(initialPosition, finalPosition);
                LogTest($"이동 거리: {distance:F2}");

                CompleteTest(result, true);
            }
            catch (System.Exception e)
            {
                CompleteTest(result, false, e.Message);
            }
        }

        /// <summary>
        /// 시스템 안정성 테스트
        /// </summary>
        public async Awaitable TestSystemStability()
        {
            var result = StartTest("시스템 안정성 테스트");

            try
            {
                LogTest("--- 시스템 안정성 테스트 시작 ---");

                // 빠른 교체 테스트
                LogTest("빠른 스컬 교체 테스트");
                for (int i = 0; i < 5; i++)
                {
                    await skullManager.SwitchToNextSlot();
                    await Awaitable.WaitForSecondsAsync(0.2f);
                }

                // 동시 어빌리티 실행 테스트
                LogTest("동시 어빌리티 실행 테스트");
                var currentSkull = skullManager?.CurrentSkull;
                if (currentSkull != null)
                {
                    // 여러 어빌리티를 빠르게 연속 실행
                    _ = currentSkull.PerformPrimaryAttack();
                    _ = currentSkull.PerformSecondaryAttack();
                    await Awaitable.WaitForSecondsAsync(1f);
                }

                // 메모리 사용량 체크
                LogTest("메모리 사용량 체크");
                long beforeMemory = System.GC.GetTotalMemory(false);
                System.GC.Collect();
                long afterMemory = System.GC.GetTotalMemory(true);
                LogTest($"메모리: {beforeMemory} → {afterMemory} (차이: {beforeMemory - afterMemory})");

                CompleteTest(result, true);
            }
            catch (System.Exception e)
            {
                CompleteTest(result, false, e.Message);
            }
        }

        #endregion

        #region 테스트 헬퍼

        /// <summary>
        /// 테스트 시작
        /// </summary>
        private TestResult StartTest(string testName)
        {
            var result = new TestResult
            {
                testName = testName,
                timestamp = System.DateTime.Now,
                passed = false,
                executionTime = Time.time
            };

            LogTest($"테스트 시작: {testName}");
            return result;
        }

        /// <summary>
        /// 테스트 완료
        /// </summary>
        private void CompleteTest(TestResult result, bool passed, string errorMessage = "")
        {
            result.passed = passed;
            result.executionTime = Time.time - result.executionTime;
            result.errorMessage = errorMessage;

            testResults.Add(result);

            string status = passed ? "성공" : "실패";
            LogTest($"테스트 완료: {result.testName} - {status} ({result.executionTime:F2}초)");

            if (!passed && !string.IsNullOrEmpty(errorMessage))
            {
                LogTest($"오류: {errorMessage}");
            }
        }

        #endregion

        #region 결과 출력

        /// <summary>
        /// 테스트 결과 출력
        /// </summary>
        public void PrintTestResults()
        {
            LogTest("=== 테스트 결과 ===");

            if (testResults.Count == 0)
            {
                LogTest("실행된 테스트가 없습니다.");
                return;
            }

            foreach (var result in testResults)
            {
                string status = result.passed ? "✓" : "✗";
                LogTest($"{status} {result.testName}: {(result.passed ? "성공" : "실패")} ({result.executionTime:F2}초)");

                if (!result.passed && !string.IsNullOrEmpty(result.errorMessage))
                {
                    LogTest($"  오류: {result.errorMessage}");
                }
            }
        }

        /// <summary>
        /// 테스트 요약 출력
        /// </summary>
        private void PrintTestSummary()
        {
            int totalTests = testResults.Count;
            int passedTests = 0;
            float totalTime = 0f;

            foreach (var result in testResults)
            {
                if (result.passed) passedTests++;
                totalTime += result.executionTime;
            }

            LogTest($"=== 테스트 요약 ===");
            LogTest($"전체 테스트: {totalTests}개");
            LogTest($"성공: {passedTests}개");
            LogTest($"실패: {totalTests - passedTests}개");
            LogTest($"성공률: {(float)passedTests / totalTests * 100f:F1}%");
            LogTest($"총 실행 시간: {totalTime:F2}초");
        }

        /// <summary>
        /// 테스트 결과 초기화
        /// </summary>
        public void ClearTestResults()
        {
            testResults.Clear();
            LogTest("테스트 결과가 초기화되었습니다.");
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 특정 테스트 실행
        /// </summary>
        public void RunSpecificTest(string testName)
        {
            switch (testName.ToLower())
            {
                case "switching":
                    _ = TestSkullSwitching();
                    break;
                case "abilities":
                    _ = TestSkullAbilities();
                    break;
                case "gas":
                    _ = TestGASIntegration();
                    break;
                case "throw":
                    _ = TestSkullThrow();
                    break;
                case "stability":
                    _ = TestSystemStability();
                    break;
                default:
                    LogTest($"알 수 없는 테스트: {testName}");
                    break;
            }
        }

        /// <summary>
        /// 모든 테스트 실행 (공개 메서드)
        /// </summary>
        public void RunAllTests()
        {
            _ = RunAllTestsCoroutine();
        }

        /// <summary>
        /// 자동 테스트 활성화/비활성화
        /// </summary>
        public void SetAutomaticTesting(bool enabled)
        {
            enableAutomaticTesting = enabled;
            LogTest($"자동 테스트: {(enabled ? "활성화" : "비활성화")}");
        }

        #endregion

        #region 디버그 및 로깅

        private void LogTest(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SkullSystemTester] {message}");
            }
        }

        #endregion

        #region GUI 디버그

        private void OnGUI()
        {
            if (!enableDebugLogs) return;

            GUILayout.BeginArea(new Rect(Screen.width - 350, 10, 330, 300));
            GUILayout.Label("=== 스컬 시스템 테스터 ===", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });

            GUILayout.Label($"테스트 상태: {(isTestingRunning ? "실행중" : "대기")}");
            GUILayout.Label($"완료된 테스트: {testResults.Count}개");

            if (testResults.Count > 0)
            {
                int passed = testResults.FindAll(r => r.passed).Count;
                GUILayout.Label($"성공률: {(float)passed / testResults.Count * 100f:F1}%");
            }

            GUILayout.Space(10);
            GUILayout.Label("=== 테스트 키 ===");
            GUILayout.Label("F1: 스컬 교체");
            GUILayout.Label("F2: 어빌리티");
            GUILayout.Label("F3: GAS 통합");
            GUILayout.Label("F4: 스컬 던지기");
            GUILayout.Label("F5: 안정성");
            GUILayout.Label("F10: 전체 테스트");
            GUILayout.Label("F11: 결과 출력");
            GUILayout.Label("F12: 결과 초기화");

            GUILayout.EndArea();
        }

        #endregion

        #region 에디터 전용

        [ContextMenu("전체 테스트 실행")]
        private void EditorRunAllTests()
        {
            if (Application.isPlaying)
            {
                RunAllTests();
            }
        }

        [ContextMenu("테스트 결과 출력")]
        private void EditorPrintResults()
        {
            if (Application.isPlaying)
            {
                PrintTestResults();
            }
        }

        #endregion
    }
}
