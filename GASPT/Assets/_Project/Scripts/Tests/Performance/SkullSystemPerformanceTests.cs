using UnityEngine;
using NUnit.Framework;
using Skull.Core;
using Skull.Data;
using Skull.Tests.Mocks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine.Profiling;

namespace Skull.Tests.Performance
{
    /// <summary>
    /// 스컬 시스템 성능 테스트
    /// 메모리 사용량, 실행 시간, FPS 영향도 등을 측정
    /// </summary>
    public class SkullSystemPerformanceTests
    {
        private GameObject testRootObject;
        private SkullSystem skullSystem;
        private SkullManager skullManager;
        private List<ISkullController> testSkulls;

        // 성능 측정 도구
        private ProfilerRecorder memoryRecorder;
        private ProfilerRecorder renderTimeRecorder;
        private Stopwatch stopwatch;

        // 성능 임계값 설정
        private const float MAX_SWITCH_TIME_MS = 16f; // 1프레임 (60FPS 기준)
        private const float MAX_ABILITY_TIME_MS = 8f;
        private const long MAX_MEMORY_ALLOCATION_BYTES = 1024 * 1024; // 1MB
        private const int STRESS_TEST_ITERATIONS = 1000;
        private const int CONCURRENT_SKULLS_COUNT = 10;

        [SetUp]
        public void SetUp()
        {
            // 테스트 환경 구성
            SetupTestEnvironment();

            // 성능 측정 도구 초기화
            InitializePerformanceTools();

            // 테스트용 스컬들 생성
            CreateTestSkulls();
        }

        [TearDown]
        public void TearDown()
        {
            // 성능 측정 정리
            CleanupPerformanceTools();

            // 테스트 환경 정리
            CleanupTestEnvironment();
        }

        #region 스컬 교체 성능 테스트

        [Test]
        public async void 스컬교체_실행시간_성능_테스트()
        {
            // Arrange
            var skull1 = testSkulls[0];
            var skull2 = testSkulls[1];

            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            // Switch to slot 0 (synchronous for now)
            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            // Warm up
            for (int i = 0; i < 10; i++)
            {
                skullManager.SwitchToNextSlotSync();
                await Awaitable.NextFrameAsync();
            }

            // Act & Assert - 실행 시간 측정
            var measurements = new List<float>();

            for (int i = 0; i < 100; i++)
            {
                stopwatch.Restart();

                skullManager.SwitchToNextSlotSync();
                await Awaitable.NextFrameAsync();

                stopwatch.Stop();
                float elapsedMs = (float)stopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond;
                measurements.Add(elapsedMs);

                // 즉시 실패 조건
                Assert.Less(elapsedMs, MAX_SWITCH_TIME_MS * 2,
                    $"스컬 교체가 {MAX_SWITCH_TIME_MS * 2}ms를 초과하면 안됩니다. 실제: {elapsedMs:F2}ms");
            }

            // 통계 분석
            float averageTime = CalculateAverage(measurements);
            float maxTime = CalculateMax(measurements);
            float percentile95 = CalculatePercentile(measurements, 95);

            UnityEngine.Debug.Log($"스컬 교체 성능 결과:\n" +
                                $"평균: {averageTime:F2}ms\n" +
                                $"최대: {maxTime:F2}ms\n" +
                                $"95퍼센타일: {percentile95:F2}ms");

            // 성능 기준 검증
            Assert.Less(averageTime, MAX_SWITCH_TIME_MS,
                $"평균 교체 시간이 {MAX_SWITCH_TIME_MS}ms를 초과하면 안됩니다");
            Assert.Less(percentile95, MAX_SWITCH_TIME_MS * 1.5f,
                $"95퍼센타일이 {MAX_SWITCH_TIME_MS * 1.5f}ms를 초과하면 안됩니다");
        }

        [Test]
        public async void 스컬교체_메모리_할당_테스트()
        {
            // Arrange
            var skull1 = testSkulls[0];
            var skull2 = testSkulls[1];

            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            // Warm up & GC
            for (int i = 0; i < 5; i++)
            {
                skullManager.SwitchToNextSlotSync();
                await Awaitable.NextFrameAsync();
            }

            System.GC.Collect();
            await Awaitable.WaitForSecondsAsync(0.1f);

            // Act - 메모리 할당 측정
            long initialMemory = System.GC.GetTotalMemory(false);

            // ProfilerRecorder는 이미 StartNew로 시작됨
            for (int i = 0; i < 50; i++)
            {
                skullManager.SwitchToNextSlotSync();
                await Awaitable.NextFrameAsync();
            }

            long finalMemory = System.GC.GetTotalMemory(false);
            long memoryDifference = finalMemory - initialMemory;

            // Assert
            UnityEngine.Debug.Log($"스컬 교체 메모리 사용량:\n" +
                                $"초기: {initialMemory / 1024}KB\n" +
                                $"최종: {finalMemory / 1024}KB\n" +
                                $"차이: {memoryDifference / 1024}KB");

            Assert.Less(memoryDifference, MAX_MEMORY_ALLOCATION_BYTES,
                $"메모리 할당이 {MAX_MEMORY_ALLOCATION_BYTES / 1024}KB를 초과하면 안됩니다");
        }

        #endregion

        #region 어빌리티 성능 테스트

        [Test]
        public async void 어빌리티_실행시간_성능_테스트()
        {
            // Arrange
            var testSkull = testSkulls[0];
            skullManager.AddSkullToSlot(0, testSkull);
            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            var measurements = new Dictionary<string, List<float>>
            {
                { "Primary", new List<float>() },
                { "Secondary", new List<float>() },
                { "Ultimate", new List<float>() },
                { "SkullThrow", new List<float>() }
            };

            // Act - 각 어빌리티 성능 측정
            for (int i = 0; i < 100; i++)
            {
                // Primary Attack
                stopwatch.Restart();
                await testSkull.PerformPrimaryAttack();
                stopwatch.Stop();
                measurements["Primary"].Add((float)stopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond);

                await Awaitable.WaitForSecondsAsync(0.01f);

                // Secondary Attack
                stopwatch.Restart();
                await testSkull.PerformSecondaryAttack();
                stopwatch.Stop();
                measurements["Secondary"].Add((float)stopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond);

                await Awaitable.WaitForSecondsAsync(0.01f);

                // Ultimate
                stopwatch.Restart();
                await testSkull.PerformUltimate();
                stopwatch.Stop();
                measurements["Ultimate"].Add((float)stopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond);

                await Awaitable.WaitForSecondsAsync(0.01f);

                // Skull Throw
                stopwatch.Restart();
                await testSkull.PerformSkullThrow(Vector2.right);
                stopwatch.Stop();
                measurements["SkullThrow"].Add((float)stopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond);

                await Awaitable.WaitForSecondsAsync(0.01f);
            }

            // Assert - 각 어빌리티의 성능 검증
            foreach (var kvp in measurements)
            {
                string abilityName = kvp.Key;
                var times = kvp.Value;

                float averageTime = CalculateAverage(times);
                float maxTime = CalculateMax(times);

                UnityEngine.Debug.Log($"{abilityName} 어빌리티 성능:\n" +
                                    $"평균: {averageTime:F2}ms, 최대: {maxTime:F2}ms");

                Assert.Less(averageTime, MAX_ABILITY_TIME_MS,
                    $"{abilityName} 평균 실행시간이 {MAX_ABILITY_TIME_MS}ms를 초과하면 안됩니다");
            }
        }

        #endregion

        #region 스트레스 테스트

        [Test]
        public async void 대량_스컬교체_스트레스_테스트()
        {
            // Arrange
            var skull1 = testSkulls[0];
            var skull2 = testSkulls[1];

            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            // Act - 대량 교체 수행
            stopwatch.Start();

            for (int i = 0; i < STRESS_TEST_ITERATIONS; i++)
            {
                skullManager.SwitchToNextSlotSync();

                // 매 100회마다 프레임 대기
                if (i % 100 == 0)
                {
                    await Awaitable.NextFrameAsync();
                }
            }

            stopwatch.Stop();

            // Assert
            float totalTimeSeconds = (float)stopwatch.ElapsedMilliseconds / 1000f;
            float averageTimePerSwitch = (float)stopwatch.ElapsedMilliseconds / STRESS_TEST_ITERATIONS;

            UnityEngine.Debug.Log($"스트레스 테스트 결과:\n" +
                                $"총 교체 수: {STRESS_TEST_ITERATIONS}\n" +
                                $"총 소요시간: {totalTimeSeconds:F2}초\n" +
                                $"교체당 평균시간: {averageTimePerSwitch:F2}ms");

            Assert.Less(averageTimePerSwitch, MAX_SWITCH_TIME_MS,
                "스트레스 테스트에서도 평균 교체 시간이 기준을 만족해야 합니다");

            // 시스템 상태 검증
            Assert.IsNotNull(skullManager.CurrentSkull);
            Assert.IsFalse(skullManager.IsSwitching);
        }

        [Test]
        public async void 다중_스컬_동시_업데이트_테스트()
        {
            // Arrange - 많은 수의 스컬 생성
            var concurrentSkulls = new List<MockSkullController>();

            for (int i = 0; i < CONCURRENT_SKULLS_COUNT; i++)
            {
                var skull = new MockSkullController((SkullType)(i % 3 + 1));
                concurrentSkulls.Add(skull);
            }

            // Act - 모든 스컬의 Update 메서드 동시 호출
            stopwatch.Start();

            for (int frame = 0; frame < 1000; frame++)
            {
                foreach (var skull in concurrentSkulls)
                {
                    skull.OnUpdate();
                    skull.OnFixedUpdate();
                }

                if (frame % 100 == 0)
                {
                    await Awaitable.NextFrameAsync();
                }
            }

            stopwatch.Stop();

            // Assert
            float totalTime = (float)stopwatch.ElapsedMilliseconds;
            float timePerFrame = totalTime / 1000f;
            float timePerSkullPerFrame = timePerFrame / CONCURRENT_SKULLS_COUNT;

            UnityEngine.Debug.Log($"다중 스컬 업데이트 성능:\n" +
                                $"스컬 수: {CONCURRENT_SKULLS_COUNT}\n" +
                                $"프레임당 시간: {timePerFrame:F2}ms\n" +
                                $"스컬당 프레임 시간: {timePerSkullPerFrame:F4}ms");

            Assert.Less(timePerFrame, MAX_SWITCH_TIME_MS,
                "프레임당 업데이트 시간이 1프레임 시간을 초과하면 안됩니다");
        }

        #endregion

        #region 메모리 누수 테스트

        [Test]
        public async void 장기간_사용_메모리누수_테스트()
        {
            // Arrange
            System.GC.Collect();
            await Awaitable.WaitForSecondsAsync(0.5f);

            long initialMemory = System.GC.GetTotalMemory(true);
            var memorySnapshots = new List<long>();

            var skull1 = testSkulls[0];
            var skull2 = testSkulls[1];

            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            // Act - 장기간 반복 사용
            for (int cycle = 0; cycle < 20; cycle++)
            {
                // 50회 교체 수행
                for (int i = 0; i < 50; i++)
                {
                    skullManager.SwitchToNextSlotSync();

                    if (i % 10 == 0)
                        await Awaitable.NextFrameAsync();
                }

                // 어빌리티 사용
                for (int i = 0; i < 10; i++)
                {
                    await skullManager.CurrentSkull.PerformPrimaryAttack();
                    await skullManager.CurrentSkull.PerformSecondaryAttack();
                    await Awaitable.NextFrameAsync();
                }

                // 메모리 스냅샷
                System.GC.Collect();
                await Awaitable.WaitForSecondsAsync(0.1f);

                long currentMemory = System.GC.GetTotalMemory(true);
                memorySnapshots.Add(currentMemory);

                UnityEngine.Debug.Log($"사이클 {cycle}: {currentMemory / 1024}KB");
            }

            // Assert - 메모리 누수 검사
            long finalMemory = memorySnapshots[memorySnapshots.Count - 1];
            long memoryGrowth = finalMemory - initialMemory;

            // 메모리 증가 추세 분석
            bool hasMemoryLeak = AnalyzeMemoryTrend(memorySnapshots);

            UnityEngine.Debug.Log($"메모리 누수 테스트 결과:\n" +
                                $"초기 메모리: {initialMemory / 1024}KB\n" +
                                $"최종 메모리: {finalMemory / 1024}KB\n" +
                                $"메모리 증가: {memoryGrowth / 1024}KB\n" +
                                $"누수 감지: {hasMemoryLeak}");

            Assert.IsFalse(hasMemoryLeak, "메모리 누수가 감지되었습니다");
            Assert.Less(memoryGrowth, MAX_MEMORY_ALLOCATION_BYTES * 2,
                "메모리 증가량이 허용 범위를 초과했습니다");
        }

        #endregion

        #region FPS 영향도 테스트

        [Test]
        public async void FPS_영향도_측정_테스트()
        {
            // Arrange
            var frameTimeBefore = new List<float>();
            var frameTimeAfter = new List<float>();

            // 기준 FPS 측정 (스컬 시스템 비활성화)
            skullSystem.SetSystemActive(false);

            for (int i = 0; i < 100; i++)
            {
                float frameStart = Time.realtimeSinceStartup;
                await Awaitable.NextFrameAsync();
                float frameTime = (Time.realtimeSinceStartup - frameStart) * 1000f;
                frameTimeBefore.Add(frameTime);
            }

            // 스컬 시스템 활성화 후 FPS 측정
            var skull1 = testSkulls[0];
            var skull2 = testSkulls[1];

            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);
            skullSystem.SetSystemActive(true);

            skullManager.SetCurrentSlot(0);

            // 활성 상태에서 프레임 시간 측정
            for (int i = 0; i < 100; i++)
            {
                float frameStart = Time.realtimeSinceStartup;

                // 스컬 시스템 작업 수행
                if (i % 10 == 0)
                    skullManager.SwitchToNextSlotSync();

                if (i % 5 == 0)
                    await skullManager.CurrentSkull.PerformPrimaryAttack();

                await Awaitable.NextFrameAsync();

                float frameTime = (Time.realtimeSinceStartup - frameStart) * 1000f;
                frameTimeAfter.Add(frameTime);
            }

            // Assert - FPS 영향도 분석
            float avgFrameTimeBefore = CalculateAverage(frameTimeBefore);
            float avgFrameTimeAfter = CalculateAverage(frameTimeAfter);
            float fpsImpact = ((avgFrameTimeAfter - avgFrameTimeBefore) / avgFrameTimeBefore) * 100f;

            UnityEngine.Debug.Log($"FPS 영향도 테스트 결과:\n" +
                                $"기준 프레임 시간: {avgFrameTimeBefore:F2}ms\n" +
                                $"활성화 후 프레임 시간: {avgFrameTimeAfter:F2}ms\n" +
                                $"FPS 영향도: {fpsImpact:F1}%");

            Assert.Less(fpsImpact, 20f, "스컬 시스템이 FPS에 20% 이상 영향을 주면 안됩니다");
            Assert.Less(avgFrameTimeAfter, MAX_SWITCH_TIME_MS,
                "활성화 상태에서도 프레임 시간이 기준을 만족해야 합니다");
        }

        #endregion

        #region 헬퍼 메서드

        private void SetupTestEnvironment()
        {
            testRootObject = new GameObject("PerformanceTestRoot");
            skullManager = testRootObject.AddComponent<SkullManager>();
            skullSystem = testRootObject.AddComponent<SkullSystem>();

            // 디버그 로그 비활성화 (성능 테스트에서 노이즈 방지)
            var enableLogsField = typeof(SkullManager).GetField("enableDebugLogs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            enableLogsField?.SetValue(skullManager, false);
        }

        private void CleanupTestEnvironment()
        {
            if (testRootObject != null)
            {
                Object.DestroyImmediate(testRootObject);
            }

            if (testSkulls != null)
            {
                testSkulls.Clear();
            }
        }

        private void InitializePerformanceTools()
        {
            stopwatch = new Stopwatch();

            // Unity Profiler 레코더 설정 (올바른 카테고리와 통계명 사용)
            memoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Allocated Memory");
            renderTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Main Thread");
        }

        private void CleanupPerformanceTools()
        {
            if (memoryRecorder.Valid)
                memoryRecorder.Dispose();

            if (renderTimeRecorder.Valid)
                renderTimeRecorder.Dispose();
        }

        private void CreateTestSkulls()
        {
            testSkulls = new List<ISkullController>();

            for (int i = 0; i < 5; i++)
            {
                var skullType = (SkullType)(i % 3 + 1);
                var mockSkull = new MockSkullController(skullType);

                // 성능 테스트에 적합한 설정
                mockSkull.SetDelays(0f, 0f, 0f); // 지연 없음
                mockSkull.SetException(false); // 예외 없음

                testSkulls.Add(mockSkull);
            }
        }

        // 통계 계산 헬퍼 메서드들
        private float CalculateAverage(List<float> values)
        {
            if (values.Count == 0) return 0f;

            float sum = 0f;
            foreach (var value in values)
                sum += value;

            return sum / values.Count;
        }

        private float CalculateMax(List<float> values)
        {
            if (values.Count == 0) return 0f;

            float max = values[0];
            foreach (var value in values)
                if (value > max) max = value;

            return max;
        }

        private float CalculatePercentile(List<float> values, int percentile)
        {
            if (values.Count == 0) return 0f;

            var sorted = new List<float>(values);
            sorted.Sort();

            int index = (int)((percentile / 100f) * (sorted.Count - 1));
            return sorted[index];
        }

        private bool AnalyzeMemoryTrend(List<long> memorySnapshots)
        {
            if (memorySnapshots.Count < 5) return false;

            // 최근 5개 스냅샷의 증가 추세 확인
            int recentCount = 5;
            var recentSnapshots = memorySnapshots.GetRange(
                memorySnapshots.Count - recentCount,
                recentCount
            );

            // 지속적인 증가 패턴 감지
            int increasingCount = 0;
            for (int i = 1; i < recentSnapshots.Count; i++)
            {
                if (recentSnapshots[i] > recentSnapshots[i - 1])
                    increasingCount++;
            }

            // 80% 이상이 증가 패턴이면 누수 의심
            return (float)increasingCount / (recentCount - 1) > 0.8f;
        }

        #endregion

        #region 성능 벤치마크

        [Test]
        public async void 종합_성능_벤치마크()
        {
            // Arrange
            var benchmarkResults = new Dictionary<string, float>();

            var skull1 = testSkulls[0];
            var skull2 = testSkulls[1];

            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            // 1. 스컬 교체 벤치마크
            stopwatch.Start();
            for (int i = 0; i < 100; i++)
            {
                skullManager.SwitchToNextSlotSync();
                if (i % 10 == 0) await Awaitable.NextFrameAsync();
            }
            stopwatch.Stop();
            benchmarkResults["스컬교체_100회"] = (float)stopwatch.ElapsedMilliseconds;

            // 2. 어빌리티 실행 벤치마크
            stopwatch.Restart();
            for (int i = 0; i < 200; i++)
            {
                await skullManager.CurrentSkull.PerformPrimaryAttack();
                if (i % 20 == 0) await Awaitable.NextFrameAsync();
            }
            stopwatch.Stop();
            benchmarkResults["기본공격_200회"] = (float)stopwatch.ElapsedMilliseconds;

            // 3. 복합 작업 벤치마크
            stopwatch.Restart();
            for (int i = 0; i < 50; i++)
            {
                skullManager.SwitchToNextSlotSync();
                await skullManager.CurrentSkull.PerformPrimaryAttack();
                await skullManager.CurrentSkull.PerformSkullThrow(Vector2.right);

                if (i % 10 == 0) await Awaitable.NextFrameAsync();
            }
            stopwatch.Stop();
            benchmarkResults["복합작업_50회"] = (float)stopwatch.ElapsedMilliseconds;

            // 결과 출력 및 검증
            UnityEngine.Debug.Log("=== 스컬 시스템 성능 벤치마크 결과 ===");
            foreach (var result in benchmarkResults)
            {
                UnityEngine.Debug.Log($"{result.Key}: {result.Value:F2}ms");

                // 기본 성능 기준 검증
                Assert.Less(result.Value, 10000f,
                    $"{result.Key} 벤치마크가 10초를 초과하면 안됩니다");
            }

            // 전체 성능 등급 산정
            float totalScore = CalculatePerformanceScore(benchmarkResults);
            UnityEngine.Debug.Log($"종합 성능 점수: {totalScore:F1}/100");

            Assert.Greater(totalScore, 60f, "종합 성능 점수가 60점 이상이어야 합니다");
        }

        private float CalculatePerformanceScore(Dictionary<string, float> results)
        {
            float score = 100f;

            // 각 벤치마크별 감점 계산
            if (results.ContainsKey("스컬교체_100회"))
            {
                float switchTime = results["스컬교체_100회"];
                if (switchTime > 1000f) score -= 20f; // 1초 초과 시 -20점
                else if (switchTime > 500f) score -= 10f; // 0.5초 초과 시 -10점
            }

            if (results.ContainsKey("기본공격_200회"))
            {
                float attackTime = results["기본공격_200회"];
                if (attackTime > 2000f) score -= 15f;
                else if (attackTime > 1000f) score -= 8f;
            }

            if (results.ContainsKey("복합작업_50회"))
            {
                float complexTime = results["복합작업_50회"];
                if (complexTime > 3000f) score -= 25f;
                else if (complexTime > 1500f) score -= 12f;
            }

            return Mathf.Max(0f, score);
        }

        #endregion
    }
}
