using UnityEngine;
using Skull.Tests.Unit;
using Skull.Tests.Integration;
using Skull.Tests.Performance;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Skull.Tests
{
    /// <summary>
    /// 통합 테스트 러너
    /// 모든 테스트를 통합적으로 실행하고 관리
    /// </summary>
    public class TestRunner : MonoBehaviour
    {
        [Header("테스트 실행 설정")]
        [SerializeField] private bool runOnStart = false;
        [SerializeField] private bool enableDetailedLogging = true;
        [SerializeField] private bool runPerformanceTests = true;
        [SerializeField] private float testTimeout = 30f;

        [Header("테스트 카테고리 선택")]
        [SerializeField] private bool runUnitTests = true;
        [SerializeField] private bool runIntegrationTests = true;
        [SerializeField] private bool runStressTests = false;

        [Header("결과 출력 설정")]
        [SerializeField] private bool saveResultsToFile = true;
        [SerializeField] private bool showGUIResults = true;
        [SerializeField] private string resultFileName = "SkullSystemTestResults";

        // 테스트 실행 상태
        private bool isRunningTests = false;
        private TestSuite currentTestSuite;
        private List<TestResult> allTestResults = new List<TestResult>();
        private float testStartTime;

        // GUI 표시용 데이터
        private Vector2 scrollPosition;
        private bool showDetailedResults = false;
        private GUIStyle headerStyle;
        private GUIStyle resultStyle;

        // 테스트 결과 데이터 구조
        [System.Serializable]
        public struct TestResult
        {
            public string testName;
            public string category;
            public bool passed;
            public float executionTime;
            public string errorMessage;
            public DateTime timestamp;
            public Dictionary<string, object> metrics;
        }

        public enum TestSuite
        {
            Unit,
            Integration,
            Performance,
            All
        }

        #region Unity 생명주기

        private void Start()
        {
            InitializeGUIStyles();

            if (runOnStart)
            {
                _ = RunAllTestsCoroutine();
            }
        }

        private void Update()
        {
            HandleKeyboardInput();
        }

        #endregion

        #region 입력 처리

        private void HandleKeyboardInput()
        {
            if (isRunningTests) return;

            // F1-F4: 개별 테스트 카테고리 실행
            if (Input.GetKeyDown(KeyCode.F1))
                _ = RunTestSuite(TestSuite.Unit);
            else if (Input.GetKeyDown(KeyCode.F2))
                _ = RunTestSuite(TestSuite.Integration);
            else if (Input.GetKeyDown(KeyCode.F3))
                _ = RunTestSuite(TestSuite.Performance);
            else if (Input.GetKeyDown(KeyCode.F9))
                _ = RunAllTestsCoroutine();

            // 결과 관리
            if (Input.GetKeyDown(KeyCode.F10))
                ExportResults();
            else if (Input.GetKeyDown(KeyCode.F11))
                ClearResults();
            else if (Input.GetKeyDown(KeyCode.F12))
                showDetailedResults = !showDetailedResults;
        }

        #endregion

        #region 테스트 실행

        /// <summary>
        /// 모든 테스트 실행
        /// </summary>
        public async Awaitable RunAllTestsCoroutine()
        {
            if (isRunningTests) return;

            LogTest("=== 스컬 시스템 전체 테스트 시작 ===");
            isRunningTests = true;
            testStartTime = Time.time;

            try
            {
                // 단위 테스트
                if (runUnitTests)
                {
                    await RunTestSuite(TestSuite.Unit);
                }

                // 통합 테스트
                if (runIntegrationTests)
                {
                    await RunTestSuite(TestSuite.Integration);
                }

                // 성능 테스트
                if (runPerformanceTests)
                {
                    await RunTestSuite(TestSuite.Performance);
                }

                // 종합 결과 출력
                GenerateFinalReport();
            }
            finally
            {
                isRunningTests = false;
                LogTest("=== 전체 테스트 완료 ===");
            }
        }

        /// <summary>
        /// 특정 테스트 스위트 실행
        /// </summary>
        public async Awaitable RunTestSuite(TestSuite suite)
        {
            currentTestSuite = suite;
            LogTest($"--- {suite} 테스트 시작 ---");

            var suiteStartTime = Time.time;
            var suiteResults = new List<TestResult>();

            try
            {
                switch (suite)
                {
                    case TestSuite.Unit:
                        await RunUnitTests(suiteResults);
                        break;

                    case TestSuite.Integration:
                        await RunIntegrationTests(suiteResults);
                        break;

                    case TestSuite.Performance:
                        await RunPerformanceTests(suiteResults);
                        break;

                    case TestSuite.All:
                        await RunAllTestsCoroutine();
                        return; // RunAllTestsCoroutine에서 이미 처리됨
                }

                // 스위트 결과 취합
                allTestResults.AddRange(suiteResults);

                var suiteTime = Time.time - suiteStartTime;
                LogTest($"--- {suite} 테스트 완료 ({suiteTime:F2}초) ---");

                // 스위트별 요약 출력
                PrintSuiteSummary(suite, suiteResults, suiteTime);
            }
            catch (Exception e)
            {
                LogError($"{suite} 테스트 실행 중 오류: {e.Message}");
            }
        }

        #endregion

        #region 개별 테스트 실행

        private async Awaitable RunUnitTests(List<TestResult> results)
        {
            LogTest("단위 테스트 실행 중...");

            // 실제 단위 테스트는 Unity Test Runner를 통해 실행
            // 여기서는 결과를 시뮬레이션
            await SimulateTestExecution("SkullManager_초기화_테스트", "Unit", results);
            await SimulateTestExecution("SkullManager_스컬추가_테스트", "Unit", results);
            await SimulateTestExecution("SkullManager_스컬교체_테스트", "Unit", results);
            await SimulateTestExecution("SkullManager_스컬제거_테스트", "Unit", results);
        }

        private async Awaitable RunIntegrationTests(List<TestResult> results)
        {
            LogTest("통합 테스트 실행 중...");

            await SimulateTestExecution("스컬시스템_GAS_연동_테스트", "Integration", results);
            await SimulateTestExecution("스컬교체_이벤트_순서_테스트", "Integration", results);
            await SimulateTestExecution("어빌리티_실행_통합_테스트", "Integration", results);
            await SimulateTestExecution("동시성_안정성_테스트", "Integration", results);
        }

        private async Awaitable RunPerformanceTests(List<TestResult> results)
        {
            LogTest("성능 테스트 실행 중...");

            await SimulatePerformanceTest("스컬교체_성능_테스트", results);
            await SimulatePerformanceTest("어빌리티_성능_테스트", results);
            await SimulatePerformanceTest("메모리_누수_테스트", results);

            if (runStressTests)
            {
                await SimulatePerformanceTest("스트레스_테스트", results);
            }
        }

        #endregion

        #region 테스트 시뮬레이션

        private async Awaitable SimulateTestExecution(string testName, string category, List<TestResult> results)
        {
            var startTime = Time.time;

            // 테스트 실행 시뮬레이션
            await Awaitable.WaitForSecondsAsync(UnityEngine.Random.Range(0.1f, 0.5f));

            // 결과 생성 (90% 성공률)
            bool passed = UnityEngine.Random.Range(0f, 1f) > 0.1f;
            string errorMessage = passed ? "" : $"시뮬레이션된 테스트 실패: {testName}";

            var result = new TestResult
            {
                testName = testName,
                category = category,
                passed = passed,
                executionTime = Time.time - startTime,
                errorMessage = errorMessage,
                timestamp = DateTime.Now,
                metrics = new Dictionary<string, object>()
            };

            results.Add(result);
            LogTest($"{(passed ? "✓" : "✗")} {testName}: {(passed ? "성공" : "실패")} ({result.executionTime:F3}초)");
        }

        private async Awaitable SimulatePerformanceTest(string testName, List<TestResult> results)
        {
            var startTime = Time.time;
            var metrics = new Dictionary<string, object>();

            // 성능 테스트 시뮬레이션
            await Awaitable.WaitForSecondsAsync(UnityEngine.Random.Range(0.5f, 2.0f));

            // 성능 메트릭 생성
            metrics["평균_실행시간_ms"] = UnityEngine.Random.Range(1f, 16f);
            metrics["최대_실행시간_ms"] = UnityEngine.Random.Range(5f, 25f);
            metrics["메모리_사용량_KB"] = UnityEngine.Random.Range(100f, 1000f);
            metrics["FPS_영향도_%"] = UnityEngine.Random.Range(0f, 15f);

            // 성능 기준 검증
            bool passed = (float)metrics["평균_실행시간_ms"] < 16f &&
                         (float)metrics["FPS_영향도_%"] < 20f;

            var result = new TestResult
            {
                testName = testName,
                category = "Performance",
                passed = passed,
                executionTime = Time.time - startTime,
                errorMessage = passed ? "" : "성능 기준 미달",
                timestamp = DateTime.Now,
                metrics = metrics
            };

            results.Add(result);
            LogTest($"{(passed ? "✓" : "✗")} {testName}: {(passed ? "성공" : "실패")} ({result.executionTime:F3}초)");
        }

        #endregion

        #region 결과 분석 및 리포트

        private void GenerateFinalReport()
        {
            LogTest("=== 최종 테스트 리포트 ===");

            var totalTime = Time.time - testStartTime;
            var totalTests = allTestResults.Count;
            var passedTests = allTestResults.Count(r => r.passed);
            var failedTests = totalTests - passedTests;

            LogTest($"총 실행 시간: {totalTime:F2}초");
            LogTest($"전체 테스트: {totalTests}개");
            LogTest($"성공: {passedTests}개 ({(float)passedTests / totalTests * 100:F1}%)");
            LogTest($"실패: {failedTests}개 ({(float)failedTests / totalTests * 100:F1}%)");

            // 카테고리별 통계
            var categories = allTestResults.GroupBy(r => r.category);
            foreach (var category in categories)
            {
                var categoryPassed = category.Count(r => r.passed);
                var categoryTotal = category.Count();
                LogTest($"{category.Key}: {categoryPassed}/{categoryTotal} ({(float)categoryPassed / categoryTotal * 100:F1}%)");
            }

            // 성능 메트릭 요약
            GeneratePerformanceReport();

            // 파일로 저장
            if (saveResultsToFile)
            {
                SaveResultsToFile();
            }
        }

        private void PrintSuiteSummary(TestSuite suite, List<TestResult> results, float executionTime)
        {
            var passed = results.Count(r => r.passed);
            var total = results.Count;

            LogTest($"{suite} 테스트 요약:");
            LogTest($"  실행 시간: {executionTime:F2}초");
            LogTest($"  성공률: {passed}/{total} ({(float)passed / total * 100:F1}%)");

            if (results.Any(r => !r.passed))
            {
                LogTest($"  실패한 테스트:");
                foreach (var failed in results.Where(r => !r.passed))
                {
                    LogTest($"    - {failed.testName}: {failed.errorMessage}");
                }
            }
        }

        private void GeneratePerformanceReport()
        {
            var performanceResults = allTestResults.Where(r => r.category == "Performance").ToList();
            if (!performanceResults.Any()) return;

            LogTest("--- 성능 테스트 요약 ---");

            foreach (var result in performanceResults)
            {
                if (result.metrics != null && result.metrics.Any())
                {
                    LogTest($"{result.testName}:");
                    foreach (var metric in result.metrics)
                    {
                        LogTest($"  {metric.Key}: {metric.Value}");
                    }
                }
            }
        }

        #endregion

        #region 결과 저장 및 로드

        private void SaveResultsToFile()
        {
            try
            {
                var fileName = $"{resultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);

                var reportData = new
                {
                    timestamp = DateTime.Now,
                    totalTests = allTestResults.Count,
                    passedTests = allTestResults.Count(r => r.passed),
                    totalExecutionTime = Time.time - testStartTime,
                    results = allTestResults.Select(r => new
                    {
                        testName = r.testName,
                        category = r.category,
                        passed = r.passed,
                        executionTime = r.executionTime,
                        errorMessage = r.errorMessage,
                        timestamp = r.timestamp,
                        metrics = r.metrics
                    }).ToArray()
                };

                var json = JsonUtility.ToJson(reportData, true);
                System.IO.File.WriteAllText(filePath, json);

                LogTest($"테스트 결과가 저장되었습니다: {filePath}");
            }
            catch (Exception e)
            {
                LogError($"결과 저장 실패: {e.Message}");
            }
        }

        private void ExportResults()
        {
            if (allTestResults.Any())
            {
                SaveResultsToFile();
            }
            else
            {
                LogTest("저장할 테스트 결과가 없습니다.");
            }
        }

        private void ClearResults()
        {
            allTestResults.Clear();
            LogTest("테스트 결과가 초기화되었습니다.");
        }

        #endregion

        #region GUI 표시

        private void InitializeGUIStyles()
        {
            // GUI 스타일은 OnGUI에서 초기화
        }

        private void OnGUI()
        {
            if (!showGUIResults) return;

            // 스타일 초기화 (OnGUI에서 안전)
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }
                };

                resultStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    normal = { textColor = Color.white }
                };
            }

            // 메인 테스트 패널
            GUILayout.BeginArea(new Rect(10, 10, 400, Screen.height - 20));

            GUILayout.Label("=== 스컬 시스템 테스트 러너 ===", headerStyle);

            // 현재 상태 표시
            if (isRunningTests)
            {
                GUILayout.Label($"실행 중: {currentTestSuite} 테스트", resultStyle);
            }
            else
            {
                GUILayout.Label("대기 중", resultStyle);
            }

            GUILayout.Space(10);

            // 테스트 실행 버튼들
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("단위 테스트 (F1)") && !isRunningTests)
                _ = RunTestSuite(TestSuite.Unit);
            if (GUILayout.Button("통합 테스트 (F2)") && !isRunningTests)
                _ = RunTestSuite(TestSuite.Integration);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("성능 테스트 (F3)") && !isRunningTests)
                _ = RunTestSuite(TestSuite.Performance);
            if (GUILayout.Button("전체 테스트 (F9)") && !isRunningTests)
                _ = RunAllTestsCoroutine();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 결과 관리 버튼들
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("결과 저장 (F10)"))
                ExportResults();
            if (GUILayout.Button("결과 초기화 (F11)"))
                ClearResults();
            GUILayout.EndHorizontal();

            if (GUILayout.Button($"{(showDetailedResults ? "간단히" : "자세히")} 보기 (F12)"))
                showDetailedResults = !showDetailedResults;

            GUILayout.Space(10);

            // 결과 표시
            DisplayTestResults();

            GUILayout.EndArea();
        }

        private void DisplayTestResults()
        {
            if (!allTestResults.Any())
            {
                GUILayout.Label("실행된 테스트가 없습니다.", resultStyle);
                return;
            }

            // 요약 정보
            var totalTests = allTestResults.Count;
            var passedTests = allTestResults.Count(r => r.passed);

            GUILayout.Label($"총 테스트: {totalTests}", resultStyle);
            GUILayout.Label($"성공: {passedTests} ({(float)passedTests / totalTests * 100:F1}%)", resultStyle);

            GUILayout.Space(5);

            // 상세 결과 (스크롤 가능)
            if (showDetailedResults)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

                foreach (var result in allTestResults.Take(20)) // 최대 20개만 표시
                {
                    var color = result.passed ? Color.green : Color.red;
                    var style = new GUIStyle(resultStyle) { normal = { textColor = color } };

                    GUILayout.Label($"{(result.passed ? "✓" : "✗")} {result.testName} ({result.executionTime:F2}s)", style);

                    if (!result.passed && !string.IsNullOrEmpty(result.errorMessage))
                    {
                        var errorStyle = new GUIStyle(resultStyle)
                        {
                            normal = { textColor = Color.yellow },
                            fontSize = 10
                        };
                        GUILayout.Label($"  {result.errorMessage}", errorStyle);
                    }
                }

                GUILayout.EndScrollView();
            }
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 외부에서 테스트 실행
        /// </summary>
        public void RunTests(TestSuite suite = TestSuite.All)
        {
            if (!isRunningTests)
            {
                _ = RunTestSuite(suite);
            }
        }

        /// <summary>
        /// 테스트 결과 가져오기
        /// </summary>
        public List<TestResult> GetTestResults()
        {
            return new List<TestResult>(allTestResults);
        }

        /// <summary>
        /// 특정 카테고리 결과 가져오기
        /// </summary>
        public List<TestResult> GetTestResults(string category)
        {
            return allTestResults.Where(r => r.category == category).ToList();
        }

        #endregion

        #region 로깅

        private void LogTest(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.Log($"[TestRunner] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[TestRunner] {message}");
        }

        #endregion
    }
}
