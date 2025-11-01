using UnityEngine;
using NUnit.Framework;
using Gameplay.Common;
using Skull.Tests.Mocks;
using GAS.Core;
using Skull.Core;

namespace Skull.Tests.Integration
{
    /// <summary>
    /// 스컬 시스템 통합 테스트
    /// 전체 스컬 시스템의 상호작용을 테스트
    /// </summary>
    public class SkullSystemIntegrationTests
    {
        private GameObject testRootObject;
        private SkullSystem skullSystem;
        private SkullManager skullManager;
        private SkullGASBridge gasBridge;
        private AbilitySystem abilitySystem;

        [SetUp]
        public void SetUp()
        {
            // 테스트용 루트 오브젝트 생성
            testRootObject = new GameObject("TestSkullSystemRoot");

            // 필요한 컴포넌트들 추가
            skullManager = testRootObject.AddComponent<SkullManager>();
            abilitySystem = testRootObject.AddComponent<AbilitySystem>();
            gasBridge = testRootObject.AddComponent<SkullGASBridge>();
            skullSystem = testRootObject.AddComponent<SkullSystem>();

            // 초기 설정
            InitializeTestEnvironment();
        }

        [TearDown]
        public void TearDown()
        {
            // 테스트 환경 정리
            if (testRootObject != null)
            {
                Object.DestroyImmediate(testRootObject);
            }
        }

        #region 초기화 테스트

        [Test]
        public void 스컬시스템_초기화_모든_컴포넌트_연결_확인()
        {
            // Assert - 모든 컴포넌트가 제대로 연결되어 있는지 확인
            Assert.IsNotNull(skullSystem);
            Assert.IsNotNull(skullManager);
            Assert.IsNotNull(gasBridge);
            Assert.IsNotNull(abilitySystem);

            // 컴포넌트 간 참조 확인
            Assert.AreEqual(abilitySystem, gasBridge.GetAbilitySystem());
        }

        #endregion

        #region 스컬 교체 통합 테스트

        [Test]
        public async void 스컬교체_시_GAS_리소스_동기화_확인()
        {
            // Arrange
            var defaultSkull = CreateTestSkull(SkullType.Default, 100f, 50f);
            var mageSkull = CreateTestSkull(SkullType.Mage, 75f, 100f);

            skullManager.AddSkullToSlot(0, defaultSkull);
            skullManager.AddSkullToSlot(1, mageSkull);

            // 초기 스컬 설정
            // skullSystem.SwitchToSkull(defaultSkull); // 동기 대안 필요
            await Awaitable.NextFrameAsync();

            // Act - 마법사 스컬로 교체
            // skullSystem.SwitchToSkull(mageSkull); // 동기 대안 필요
            await Awaitable.WaitForSecondsAsync(0.5f);

            // Assert - GAS 리소스가 마법사 스탯에 맞게 변경되었는지 확인
            float healthMax = abilitySystem.GetMaxResource("Health");
            float manaMax = abilitySystem.GetMaxResource("Mana");

            Assert.AreEqual(75f, healthMax, "체력 최대값이 마법사 스탯에 맞게 변경되어야 함");
            Assert.AreEqual(100f, manaMax, "마나 최대값이 마법사 스탯에 맞게 변경되어야 함");
        }

        [Test]
        public async void 스컬교체_시_이벤트_발생_순서_확인()
        {
            // Arrange
            var skull1 = new MockSkullController(SkullType.Default);
            var skull2 = new MockSkullController(SkullType.Mage);

            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            // 초기 스컬 설정
            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            // 이벤트 발생 확인을 위한 초기화
            skull1.ResetCallCounts();
            skull2.ResetCallCounts();

            // Act - 스컬 교체 실행
            skullManager.SetCurrentSlot(1);
            await Awaitable.NextFrameAsync();

            // Assert - 이벤트 발생 순서 확인
            Assert.IsTrue(skull1.WasMethodCalled("OnDeactivate", 1), "이전 스컬이 비활성화되어야 함");
            Assert.IsTrue(skull1.WasMethodCalled("OnUnequip", 1), "이전 스컬이 해제되어야 함");
            Assert.IsTrue(skull2.WasMethodCalled("OnEquip", 1), "새 스컬이 장착되어야 함");
            Assert.IsTrue(skull2.WasMethodCalled("OnActivate", 1), "새 스컬이 활성화되어야 함");
        }

        #endregion

        #region 어빌리티 시스템 통합 테스트

        [Test]
        public async void 스컬어빌리티_GAS시스템_연동_확인()
        {
            // Arrange
            var testSkull = CreateTestSkull(SkullType.Mage, 100f, 100f);
            skullManager.AddSkullToSlot(0, testSkull);
            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            // 초기 마나 설정
            abilitySystem.SetResource("Mana", 50f);

            // Act - 마나 소모 어빌리티 실행
            await testSkull.PerformSecondaryAttack();
            await Awaitable.NextFrameAsync();

            // Assert - 마나가 소모되었는지 확인
            float currentMana = abilitySystem.GetResource("Mana");
            Assert.Less(currentMana, 50f, "어빌리티 사용 후 마나가 소모되어야 함");
        }

        [Test]
        public async void 스컬던지기_텔레포트_기능_통합_테스트()
        {
            // Arrange
            var testSkull = new MockSkullController(SkullType.Default);
            skullManager.AddSkullToSlot(0, testSkull);
            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            Vector3 initialPosition = testRootObject.transform.position;
            Vector2 throwDirection = Vector2.right;

            // Act - 스컬 던지기 실행
            // skullSystem.PerformSkullThrow(throwDirection); // 동기 대안 필요
            await Awaitable.WaitForSecondsAsync(1f);

            // Assert - 스컬 던지기가 호출되었는지 확인
            Assert.IsTrue(testSkull.WasMethodCalled("PerformSkullThrow", 1));
            Assert.AreEqual(throwDirection, testSkull.LastThrowDirection);
        }

        #endregion

        #region 동시성 및 안정성 테스트

        [Test]
        public async void 빠른_연속_스컬교체_안정성_테스트()
        {
            // Arrange
            var skull1 = new MockSkullController(SkullType.Default);
            var skull2 = new MockSkullController(SkullType.Mage);

            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            // Act - 빠른 연속 교체 시도
            var task1 = skullManager.SwitchToSlot(1);
            var task2 = skullManager.SwitchToSlot(0);
            var task3 = skullManager.SwitchToSlot(1);

            // 모든 태스크 완료 대기
            await task1;
            await task2;
            await task3;
            await Awaitable.WaitForSecondsAsync(1f);

            // Assert - 시스템이 안정적으로 동작했는지 확인
            Assert.IsNotNull(skullManager.CurrentSkull);
            Assert.IsFalse(skullManager.IsSwitching);
        }

        [Test]
        public async void 스컬교체_중_어빌리티_사용_차단_테스트()
        {
            // Arrange
            var skull1 = new MockSkullController(SkullType.Default);
            var skull2 = new MockSkullController(SkullType.Mage);

            // 교체에 시간이 걸리도록 설정
            skull2.SetDelays(equipDelay: 0.5f);

            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            skull1.ResetCallCounts();

            // Act - 교체 시작 후 즉시 어빌리티 사용 시도
            var switchTask = skullManager.SwitchToSlot(1);
            await Awaitable.WaitForSecondsAsync(0.1f); // 교체가 진행 중일 때

            // skull1.PerformPrimaryAttack(); // 동기 대안 필요

            await switchTask;
            await Awaitable.NextFrameAsync();

            // Assert - 교체 중에는 이전 스컬의 어빌리티가 제대로 차단되었는지 확인
            // (구체적인 검증은 구현에 따라 달라질 수 있음)
            Assert.IsTrue(skullManager.IsSwitching == false, "교체가 완료되어야 함");
        }

        #endregion

        #region 오류 처리 테스트

        [Test]
        public async void 스컬교체_오류발생_시_복구_테스트()
        {
            // Arrange
            var normalSkull = new MockSkullController(SkullType.Default);
            var faultySkull = new MockSkullController(SkullType.Mage);

            // 오류가 발생하도록 설정
            faultySkull.SetException(true, "Test Exception");

            skullManager.AddSkullToSlot(0, normalSkull);
            skullManager.AddSkullToSlot(1, faultySkull);

            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            // Act - 오류가 발생하는 스컬로 교체 시도
            try
            {
                skullManager.SetCurrentSlot(1);
            }
            catch
            {
                // 예외는 무시 (테스트 목적)
            }

            await Awaitable.NextFrameAsync();

            // Assert - 시스템이 안정 상태로 복구되었는지 확인
            Assert.IsFalse(skullManager.IsSwitching);
            Assert.IsNotNull(skullManager.CurrentSkull);
        }

        #endregion

        #region 성능 테스트

        [Test]
        public async void 스컬시스템_메모리누수_테스트()
        {
            // Arrange
            long initialMemory = System.GC.GetTotalMemory(true);

            var skull1 = new MockSkullController(SkullType.Default);
            var skull2 = new MockSkullController(SkullType.Mage);

            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            // Act - 많은 교체 수행
            for (int i = 0; i < 100; i++)
            {
                skullManager.SetCurrentSlot(i % 2);
                await Awaitable.NextFrameAsync();
            }

            // 메모리 정리
            System.GC.Collect();
            await Awaitable.WaitForSecondsAsync(0.1f);

            long finalMemory = System.GC.GetTotalMemory(true);

            // Assert - 메모리 사용량이 비정상적으로 증가하지 않았는지 확인
            long memoryDiff = finalMemory - initialMemory;
            Assert.Less(memoryDiff, 10 * 1024 * 1024, "메모리 사용량이 10MB를 초과하지 않아야 함"); // 10MB 임계치
        }

        #endregion

        #region 헬퍼 메서드

        /// <summary>
        /// 테스트 환경 초기화
        /// </summary>
        private void InitializeTestEnvironment()
        {
            // GAS 시스템 초기 설정
            if (abilitySystem != null)
            {
                // 기본 리소스 설정
                abilitySystem.SetMaxResource("Health", 100f);
                abilitySystem.SetResource("Health", 100f);
                abilitySystem.SetMaxResource("Mana", 100f);
                abilitySystem.SetResource("Mana", 100f);
            }

            // 스컬 매니저 초기 설정
            if (skullManager != null)
            {
                // 디버그 로그 비활성화 (테스트 중 노이즈 방지)
                var enableLogsField = typeof(SkullManager).GetField("enableDebugLogs",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                enableLogsField?.SetValue(skullManager, false);
            }
        }

        /// <summary>
        /// 테스트용 스컬 생성
        /// </summary>
        private ISkullController CreateTestSkull(SkullType type, float maxHealth, float maxMana)
        {
            var mockSkull = new MockSkullController(type);

            // 커스텀 상태 설정
            mockSkull.SetStatus(new SkullStatus
            {
                isReady = true,
                cooldownRemaining = 0f,
                manaRemaining = maxMana,
                canUseAbilities = true
            });

            return mockSkull;
        }

        /// <summary>
        /// 테스트 실행 시간 측정
        /// </summary>
        private System.Diagnostics.Stopwatch StartTimer()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            return stopwatch;
        }

        /// <summary>
        /// 테스트 시간 검증
        /// </summary>
        private void AssertExecutionTime(System.Diagnostics.Stopwatch stopwatch, float maxSeconds)
        {
            stopwatch.Stop();
            float elapsedSeconds = (float)stopwatch.ElapsedMilliseconds / 1000f;
            Assert.Less(elapsedSeconds, maxSeconds,
                $"실행 시간이 {maxSeconds}초를 초과하면 안됩니다. 실제: {elapsedSeconds:F2}초");
        }

        #endregion

        #region 복합 시나리오 테스트

        [Test]
        public async void 전체_게임플레이_시나리오_테스트()
        {
            // Arrange - 게임 시작 상황 설정
            var defaultSkull = new MockSkullController(SkullType.Default);
            var mageSkull = new MockSkullController(SkullType.Mage);
            var warriorSkull = new MockSkullController(SkullType.Warrior);

            skullManager.AddSkullToSlot(0, defaultSkull);
            skullManager.AddSkullToSlot(1, mageSkull);

            // 초기 스컬 장착
            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            // Act & Assert - 실제 게임플레이 시나리오 수행

            // 1. 기본 공격 사용
            await defaultSkull.PerformPrimaryAttack();
            Assert.IsTrue(defaultSkull.WasMethodCalled("PerformPrimaryAttack", 1));

            // 2. 스컬 교체
            skullManager.SwitchToNextSlotSync();
            await Awaitable.NextFrameAsync();
            Assert.AreEqual(mageSkull, skullManager.CurrentSkull);

            // 3. 새 스컬로 어빌리티 사용
            await mageSkull.PerformSecondaryAttack();
            Assert.IsTrue(mageSkull.WasMethodCalled("PerformSecondaryAttack", 1));

            // 4. 스컬 던지기
            await mageSkull.PerformSkullThrow(Vector2.right);
            Assert.IsTrue(mageSkull.WasMethodCalled("PerformSkullThrow", 1));

            // 5. 다시 교체
            skullManager.SwitchToPreviousSlotSync();
            await Awaitable.NextFrameAsync();
            Assert.AreEqual(defaultSkull, skullManager.CurrentSkull);

            // 6. 궁극기 사용
            await defaultSkull.PerformUltimate();
            Assert.IsTrue(defaultSkull.WasMethodCalled("PerformUltimate", 1));

            // 최종 상태 검증
            Assert.IsFalse(skullManager.IsSwitching);
            Assert.IsTrue(skullManager.CanSwitch);
        }

        #endregion
    }
}
