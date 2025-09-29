using UnityEngine;

namespace Skull.Tests
{
    /// <summary>
    /// 테스트 설정 ScriptableObject
    /// 테스트 실행 시 사용할 설정들을 관리
    /// </summary>
    [CreateAssetMenu(fileName = "TestConfiguration", menuName = "Skull System/Test Configuration")]
    public class TestConfiguration : ScriptableObject
    {
        [Header("테스트 실행 설정")]
        [SerializeField] private bool enableUnitTests = true;
        [SerializeField] private bool enableIntegrationTests = true;
        [SerializeField] private bool enablePerformanceTests = true;
        [SerializeField] private bool enableStressTests = false;

        [Header("성능 테스트 임계값")]
        [SerializeField] private float maxSkullSwitchTimeMs = 16f;
        [SerializeField] private float maxAbilityExecutionTimeMs = 8f;
        [SerializeField] private float maxMemoryAllocationMB = 1f;
        [SerializeField] private float maxFpsImpactPercent = 20f;

        [Header("스트레스 테스트 설정")]
        [SerializeField] private int stressTestIterations = 1000;
        [SerializeField] private int concurrentSkullsCount = 10;
        [SerializeField] private float stressTestTimeoutMinutes = 5f;

        [Header("결과 출력 설정")]
        [SerializeField] private bool saveResultsToFile = true;
        [SerializeField] private bool enableDetailedLogging = true;
        [SerializeField] private bool showPerformanceMetrics = true;
        [SerializeField] private string resultFilePrefix = "SkullSystemTest";

        [Header("테스트 환경 설정")]
        [SerializeField] private bool cleanupAfterTests = true;
        [SerializeField] private bool resetStateBetweenTests = true;
        [SerializeField] private float testTimeoutSeconds = 30f;

        // Properties
        public bool EnableUnitTests => enableUnitTests;
        public bool EnableIntegrationTests => enableIntegrationTests;
        public bool EnablePerformanceTests => enablePerformanceTests;
        public bool EnableStressTests => enableStressTests;

        public float MaxSkullSwitchTimeMs => maxSkullSwitchTimeMs;
        public float MaxAbilityExecutionTimeMs => maxAbilityExecutionTimeMs;
        public float MaxMemoryAllocationMB => maxMemoryAllocationMB;
        public float MaxFpsImpactPercent => maxFpsImpactPercent;

        public int StressTestIterations => stressTestIterations;
        public int ConcurrentSkullsCount => concurrentSkullsCount;
        public float StressTestTimeoutMinutes => stressTestTimeoutMinutes;

        public bool SaveResultsToFile => saveResultsToFile;
        public bool EnableDetailedLogging => enableDetailedLogging;
        public bool ShowPerformanceMetrics => showPerformanceMetrics;
        public string ResultFilePrefix => resultFilePrefix;

        public bool CleanupAfterTests => cleanupAfterTests;
        public bool ResetStateBetweenTests => resetStateBetweenTests;
        public float TestTimeoutSeconds => testTimeoutSeconds;

        /// <summary>
        /// 기본 설정 생성
        /// </summary>
        public static TestConfiguration CreateDefault()
        {
            var config = CreateInstance<TestConfiguration>();
            config.name = "DefaultTestConfiguration";
            return config;
        }

        /// <summary>
        /// 설정 유효성 검증
        /// </summary>
        public bool ValidateConfiguration()
        {
            if (maxSkullSwitchTimeMs <= 0 || maxAbilityExecutionTimeMs <= 0)
            {
                Debug.LogError("성능 임계값이 유효하지 않습니다.");
                return false;
            }

            if (stressTestIterations <= 0 || concurrentSkullsCount <= 0)
            {
                Debug.LogError("스트레스 테스트 설정이 유효하지 않습니다.");
                return false;
            }

            if (testTimeoutSeconds <= 0)
            {
                Debug.LogError("테스트 타임아웃 설정이 유효하지 않습니다.");
                return false;
            }

            return true;
        }

        private void OnValidate()
        {
            // 에디터에서 값 변경 시 자동 검증
            maxSkullSwitchTimeMs = Mathf.Max(1f, maxSkullSwitchTimeMs);
            maxAbilityExecutionTimeMs = Mathf.Max(1f, maxAbilityExecutionTimeMs);
            maxMemoryAllocationMB = Mathf.Max(0.1f, maxMemoryAllocationMB);
            maxFpsImpactPercent = Mathf.Clamp(maxFpsImpactPercent, 0f, 100f);

            stressTestIterations = Mathf.Max(1, stressTestIterations);
            concurrentSkullsCount = Mathf.Max(1, concurrentSkullsCount);
            stressTestTimeoutMinutes = Mathf.Max(0.1f, stressTestTimeoutMinutes);

            testTimeoutSeconds = Mathf.Max(1f, testTimeoutSeconds);
        }
    }
}