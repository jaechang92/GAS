using UnityEngine;
using Skull.Tests.Unit;
using Skull.Tests.Integration;
using Skull.Tests.Performance;
using System.Collections.Generic;
using System.Linq;
using System;
using Gameplay.Common;

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

            // 실제 단위 테스트 실행
            await RunRealTest("SkullManager_초기화_테스트", "Unit", results, Test_SkullManager_초기화);
            await RunRealTest("SkullManager_스컬추가_테스트", "Unit", results, Test_SkullManager_스컬추가);
            await RunRealTest("SkullManager_스컬교체_테스트", "Unit", results, Test_SkullManager_스컬교체);
            await RunRealTest("SkullManager_스컬제거_테스트", "Unit", results, Test_SkullManager_스컬제거);
        }

        private async Awaitable RunIntegrationTests(List<TestResult> results)
        {
            LogTest("통합 테스트 실행 중...");
            LogTest("통합 테스트는 아직 구현되지 않았습니다.");

            // TODO: 실제 통합 테스트 구현 필요
            // await RunRealTest("스컬시스템_GAS_연동_테스트", "Integration", results, Test_GAS_연동);
            // await RunRealTest("스컬교체_이벤트_순서_테스트", "Integration", results, Test_스컬교체_이벤트);
            // await RunRealTest("어빌리티_실행_통합_테스트", "Integration", results, Test_어빌리티_통합);
            // await RunRealTest("동시성_안정성_테스트", "Integration", results, Test_동시성);

            await Awaitable.NextFrameAsync();
        }

        private async Awaitable RunPerformanceTests(List<TestResult> results)
        {
            LogTest("성능 테스트 실행 중...");
            LogTest("성능 테스트는 아직 구현되지 않았습니다.");

            // TODO: 실제 성능 테스트 구현 필요
            // await RunRealTest("스컬교체_성능_테스트", "Performance", results, Test_스컬교체_성능);
            // await RunRealTest("어빌리티_성능_테스트", "Performance", results, Test_어빌리티_성능);
            // await RunRealTest("메모리_누수_테스트", "Performance", results, Test_메모리_누수);

            await Awaitable.NextFrameAsync();
        }

        #endregion

        #region 실제 테스트 실행

        /// <summary>
        /// 실제 테스트 실행
        /// </summary>
        private async Awaitable RunRealTest(string testName, string category, List<TestResult> results, Func<Awaitable<(bool, string)>> testFunc)
        {
            var startTime = Time.time;

            try
            {
                // 실제 테스트 함수 실행
                var (passed, errorMessage) = await testFunc();

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

                if (!passed && !string.IsNullOrEmpty(errorMessage))
                {
                    LogError($"  실패 원인: {errorMessage}");
                }
            }
            catch (Exception e)
            {
                var result = new TestResult
                {
                    testName = testName,
                    category = category,
                    passed = false,
                    executionTime = Time.time - startTime,
                    errorMessage = $"예외 발생: {e.Message}\n{e.StackTrace}",
                    timestamp = DateTime.Now,
                    metrics = new Dictionary<string, object>()
                };

                results.Add(result);
                LogTest($"✗ {testName}: 실패 ({result.executionTime:F3}초)");
                LogError($"  예외: {e.Message}");
            }
        }

        // 시뮬레이션 테스트 메서드는 더 이상 사용하지 않음
        // 실제 테스트 구현 시 삭제 예정

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

        #region 실제 SkullManager 테스트 메서드들

        /// <summary>
        /// SkullManager 초기화 테스트
        /// </summary>
        private async Awaitable<(bool, string)> Test_SkullManager_초기화()
        {
            GameObject testObj = null;
            try
            {
                testObj = new GameObject("TestSkullManager");
                var skullManager = testObj.AddComponent<Skull.Core.SkullManager>();

                await Awaitable.NextFrameAsync();

                // 검증
                if (skullManager == null)
                    return (false, "SkullManager 생성 실패");

                if (skullManager.MaxSlots != 2)
                    return (false, $"MaxSlots 기본값 오류: 예상=2, 실제={skullManager.MaxSlots}");

                if (skullManager.CurrentSkull != null)
                    return (false, "초기 CurrentSkull이 null이어야 함");

                if (skullManager.IsSwitching)
                    return (false, "초기 IsSwitching이 false여야 함");

                return (true, "");
            }
            catch (Exception e)
            {
                return (false, $"예외 발생: {e.Message}");
            }
            finally
            {
                if (testObj != null)
                    Destroy(testObj);
            }
        }

        /// <summary>
        /// SkullManager 스컬 추가 테스트
        /// </summary>
        private async Awaitable<(bool, string)> Test_SkullManager_스컬추가()
        {
            GameObject testObj = null;
            try
            {
                testObj = new GameObject("TestSkullManager");
                var skullManager = testObj.AddComponent<Skull.Core.SkullManager>();

                await Awaitable.NextFrameAsync();

                // Mock 스컬 생성
                var mockSkull = new Unit.MockSkullController(SkullType.Default);

                // 스컬 추가
                bool addResult = skullManager.AddSkullToSlot(0, mockSkull);

                // 검증
                if (!addResult)
                    return (false, "AddSkullToSlot 실패");

                var retrievedSkull = skullManager.GetSkullInSlot(0);
                if (retrievedSkull != mockSkull)
                    return (false, "추가된 스컬을 찾을 수 없음");

                if (skullManager.SkullCount != 1)
                    return (false, $"SkullCount 오류: 예상=1, 실제={skullManager.SkullCount}");

                // 잘못된 슬롯 인덱스 테스트
                if (skullManager.AddSkullToSlot(-1, mockSkull))
                    return (false, "잘못된 슬롯 인덱스(-1)에 추가 성공해서는 안됨");

                if (skullManager.AddSkullToSlot(10, mockSkull))
                    return (false, "잘못된 슬롯 인덱스(10)에 추가 성공해서는 안됨");

                // null 스컬 추가 테스트
                if (skullManager.AddSkullToSlot(1, null))
                    return (false, "null 스컬 추가가 성공해서는 안됨");

                return (true, "");
            }
            catch (Exception e)
            {
                return (false, $"예외 발생: {e.Message}");
            }
            finally
            {
                if (testObj != null)
                    Destroy(testObj);
            }
        }

        /// <summary>
        /// SkullManager 스컬 교체 테스트
        /// </summary>
        private async Awaitable<(bool, string)> Test_SkullManager_스컬교체()
        {
            GameObject testObj = null;
            try
            {
                testObj = new GameObject("TestSkullManager");
                var skullManager = testObj.AddComponent<Skull.Core.SkullManager>();

                await Awaitable.NextFrameAsync();

                // 두 개의 Mock 스컬 생성
                var mockSkull1 = new Unit.MockSkullController(SkullType.Default);
                var mockSkull2 = new Unit.MockSkullController(SkullType.Mage);

                // 스컬들을 슬롯에 추가
                skullManager.AddSkullToSlot(0, mockSkull1);
                skullManager.AddSkullToSlot(1, mockSkull2);

                // 첫 번째 스컬로 설정
                skullManager.SetCurrentSlot(0);
                await Awaitable.NextFrameAsync();

                // 검증 1: 첫 번째 스컬이 현재 스컬인지
                if (skullManager.CurrentSkull != mockSkull1)
                    return (false, "첫 번째 스컬 설정 실패");

                if (skullManager.CurrentSlotIndex != 0)
                    return (false, $"CurrentSlotIndex 오류: 예상=0, 실제={skullManager.CurrentSlotIndex}");

                // 두 번째 스컬로 교체
                skullManager.SetCurrentSlot(1);
                await Awaitable.NextFrameAsync();

                // 검증 2: 두 번째 스컬로 교체되었는지
                if (skullManager.CurrentSkull != mockSkull2)
                    return (false, "두 번째 스컬로 교체 실패");

                if (skullManager.CurrentSlotIndex != 1)
                    return (false, $"교체 후 CurrentSlotIndex 오류: 예상=1, 실제={skullManager.CurrentSlotIndex}");

                // 다음 슬롯으로 교체 (순환 테스트)
                skullManager.SwitchToNextSlotSync();
                await Awaitable.NextFrameAsync();

                if (skullManager.CurrentSkull != mockSkull1)
                    return (false, "순환 교체 실패 (다음 슬롯)");

                if (skullManager.CurrentSlotIndex != 0)
                    return (false, $"순환 교체 후 인덱스 오류: 예상=0, 실제={skullManager.CurrentSlotIndex}");

                // 이전 슬롯으로 교체
                skullManager.SwitchToPreviousSlotSync();
                await Awaitable.NextFrameAsync();

                if (skullManager.CurrentSkull != mockSkull2)
                    return (false, "순환 교체 실패 (이전 슬롯)");

                return (true, "");
            }
            catch (Exception e)
            {
                return (false, $"예외 발생: {e.Message}");
            }
            finally
            {
                if (testObj != null)
                    Destroy(testObj);
            }
        }

        /// <summary>
        /// SkullManager 스컬 제거 테스트
        /// </summary>
        private async Awaitable<(bool, string)> Test_SkullManager_스컬제거()
        {
            GameObject testObj = null;
            try
            {
                testObj = new GameObject("TestSkullManager");
                var skullManager = testObj.AddComponent<Skull.Core.SkullManager>();

                await Awaitable.NextFrameAsync();

                // Mock 스컬 생성 및 추가
                var mockSkull = new Unit.MockSkullController(SkullType.Default);
                skullManager.AddSkullToSlot(0, mockSkull);

                // 검증 1: 스컬이 추가되었는지
                if (skullManager.SkullCount != 1)
                    return (false, "스컬 추가 실패");

                // 스컬 제거
                bool removeResult = skullManager.RemoveSkullFromSlot(0);

                // 검증 2: 스컬이 제거되었는지
                if (!removeResult)
                    return (false, "RemoveSkullFromSlot 실패");

                var retrievedSkull = skullManager.GetSkullInSlot(0);
                if (retrievedSkull != null)
                    return (false, "스컬이 제거되지 않음");

                // 빈 슬롯 제거 시도
                bool removeEmptyResult = skullManager.RemoveSkullFromSlot(0);
                if (removeEmptyResult)
                    return (false, "빈 슬롯 제거가 성공해서는 안됨");

                return (true, "");
            }
            catch (Exception e)
            {
                return (false, $"예외 발생: {e.Message}");
            }
            finally
            {
                if (testObj != null)
                    Destroy(testObj);
            }
        }

        #endregion
    }
}
