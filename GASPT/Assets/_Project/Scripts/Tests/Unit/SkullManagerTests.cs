using UnityEngine;
using NUnit.Framework;
using Gameplay.Common;
using System.Threading;
using System.Linq;
using Skull.Core;

namespace Skull.Tests.Unit
{
    /// <summary>
    /// SkullManager 단위 테스트
    /// </summary>
    public class SkullManagerTests
    {
        private GameObject testObject;
        private SkullManager skullManager;
        private SkullData[] testSkullData;

        [SetUp]
        public void SetUp()
        {
            // 테스트용 게임오브젝트 생성
            testObject = new GameObject("TestSkullManager");
            skullManager = testObject.AddComponent<SkullManager>();

            // 테스트용 스컬 데이터 생성
            CreateTestSkullData();
        }

        [TearDown]
        public void TearDown()
        {
            // 테스트 정리
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }

            // 테스트 데이터 정리
            CleanupTestData();
        }

        #region 초기화 테스트

        [Test]
        public void SkullManager_초기화_시_기본값_설정()
        {
            // Assert
            Assert.IsNotNull(skullManager);
            Assert.AreEqual(0, skullManager.SkullCount);
            Assert.AreEqual(2, skullManager.MaxSlots);
            Assert.IsNull(skullManager.CurrentSkull);
            Assert.IsFalse(skullManager.IsSwitching);
            Assert.IsTrue(skullManager.CanSwitch);
        }

        #endregion

        #region 스컬 추가/제거 테스트

        [Test]
        public void AddSkullToSlot_유효한_슬롯에_스컬_추가_성공()
        {
            // Arrange
            var mockSkull = CreateMockSkull(SkullType.Default);

            // Act
            bool result = skullManager.AddSkullToSlot(0, mockSkull);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(mockSkull, skullManager.GetSkullInSlot(0));
            Assert.AreEqual(1, skullManager.SkullCount);
        }

        [Test]
        public void AddSkullToSlot_잘못된_슬롯_인덱스_실패()
        {
            // Arrange
            var mockSkull = CreateMockSkull(SkullType.Default);

            // Act & Assert
            Assert.IsFalse(skullManager.AddSkullToSlot(-1, mockSkull));
            Assert.IsFalse(skullManager.AddSkullToSlot(10, mockSkull));
        }

        [Test]
        public void AddSkullToSlot_Null_스컬_실패()
        {
            // Act & Assert
            Assert.IsFalse(skullManager.AddSkullToSlot(0, null));
        }

        [Test]
        public void RemoveSkullFromSlot_존재하는_스컬_제거_성공()
        {
            // Arrange
            var mockSkull = CreateMockSkull(SkullType.Default);
            skullManager.AddSkullToSlot(0, mockSkull);

            // Act
            bool result = skullManager.RemoveSkullFromSlot(0);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNull(skullManager.GetSkullInSlot(0));
        }

        [Test]
        public void RemoveSkullFromSlot_빈_슬롯_제거_실패()
        {
            // Act & Assert
            Assert.IsFalse(skullManager.RemoveSkullFromSlot(0));
        }

        #endregion

        #region 스컬 검색 테스트

        [Test]
        public void GetSkullByType_존재하는_타입_반환_성공()
        {
            // Arrange
            var mockSkull = CreateMockSkull(SkullType.Mage);
            skullManager.AddSkullToSlot(0, mockSkull);

            // Act
            var result = skullManager.GetSkullByType(SkullType.Mage);

            // Assert
            Assert.AreEqual(mockSkull, result);
        }

        [Test]
        public void GetSkullByType_존재하지않는_타입_Null_반환()
        {
            // Act
            var result = skullManager.GetSkullByType(SkullType.Warrior);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetAllSkulls_모든_스컬_반환()
        {
            // Arrange
            var skull1 = CreateMockSkull(SkullType.Default);
            var skull2 = CreateMockSkull(SkullType.Mage);
            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            // Act
            var allSkulls = skullManager.GetAllSkulls();

            // Assert
            Assert.AreEqual(2, allSkulls.Count());
            Assert.Contains(skull1, allSkulls.ToArray());
            Assert.Contains(skull2, allSkulls.ToArray());
        }

        #endregion

        #region 스컬 상태 테스트

        [Test]
        public void GetSkullStatus_유효한_슬롯_상태_반환()
        {
            // Arrange
            var mockSkull = CreateMockSkullWithStatus(SkullType.Default, true, 0f, 100f, true);
            skullManager.AddSkullToSlot(0, mockSkull);

            // Act
            var status = skullManager.GetSkullStatus(0);

            // Assert
            Assert.IsTrue(status.isReady);
            Assert.AreEqual(0f, status.cooldownRemaining);
            Assert.AreEqual(100f, status.manaRemaining);
            Assert.IsTrue(status.canUseAbilities);
        }

        [Test]
        public void GetSkullStatus_빈_슬롯_NotReady_반환()
        {
            // Act
            var status = skullManager.GetSkullStatus(0);

            // Assert
            Assert.IsFalse(status.isReady);
        }

        #endregion

        #region 비동기 테스트

        [Test]
        public async void SwitchToSlot_유효한_슬롯_교체_성공()
        {
            // Arrange
            var skull1 = CreateMockSkull(SkullType.Default);
            var skull2 = CreateMockSkull(SkullType.Mage);
            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            // 초기 스컬 설정
            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            Assert.AreEqual(skull1, skullManager.CurrentSkull);

            // Act
            skullManager.SetCurrentSlot(1);
            await Awaitable.NextFrameAsync();

            // Assert
            Assert.AreEqual(skull2, skullManager.CurrentSkull);
            Assert.AreEqual(1, skullManager.CurrentSlotIndex);
        }

        [Test]
        public async void SwitchToNextSlot_순환_교체_성공()
        {
            // Arrange
            var skull1 = CreateMockSkull(SkullType.Default);
            var skull2 = CreateMockSkull(SkullType.Mage);
            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            // Act - 다음 슬롯으로
            skullManager.SwitchToNextSlotSync();
            await Awaitable.NextFrameAsync();

            // Assert
            Assert.AreEqual(skull2, skullManager.CurrentSkull);
            Assert.AreEqual(1, skullManager.CurrentSlotIndex);

            // Act - 다시 다음 슬롯으로 (순환)
            skullManager.SwitchToNextSlotSync();
            await Awaitable.NextFrameAsync();

            // Assert
            Assert.AreEqual(skull1, skullManager.CurrentSkull);
            Assert.AreEqual(0, skullManager.CurrentSlotIndex);
        }

        [Test]
        public async void SwitchToPreviousSlot_순환_교체_성공()
        {
            // Arrange
            var skull1 = CreateMockSkull(SkullType.Default);
            var skull2 = CreateMockSkull(SkullType.Mage);
            skullManager.AddSkullToSlot(0, skull1);
            skullManager.AddSkullToSlot(1, skull2);

            skullManager.SetCurrentSlot(0);
            await Awaitable.NextFrameAsync();

            // Act - 이전 슬롯으로 (순환)
            skullManager.SwitchToPreviousSlotSync();
            await Awaitable.NextFrameAsync();

            // Assert
            Assert.AreEqual(skull2, skullManager.CurrentSkull);
            Assert.AreEqual(1, skullManager.CurrentSlotIndex);
        }

        #endregion

        #region 헬퍼 메서드

        /// <summary>
        /// 테스트용 스컬 데이터 생성
        /// </summary>
        private void CreateTestSkullData()
        {
            testSkullData = new SkullData[3];

            for (int i = 0; i < testSkullData.Length; i++)
            {
                var skullData = ScriptableObject.CreateInstance<SkullData>();
                // 필요한 데이터 설정은 여기서...
                testSkullData[i] = skullData;
            }
        }

        /// <summary>
        /// 테스트 데이터 정리
        /// </summary>
        private void CleanupTestData()
        {
            if (testSkullData != null)
            {
                foreach (var data in testSkullData)
                {
                    if (data != null)
                    {
                        Object.DestroyImmediate(data);
                    }
                }
            }
        }

        /// <summary>
        /// 모킹 스컬 생성
        /// </summary>
        private ISkullController CreateMockSkull(SkullType type)
        {
            var mockSkull = new MockSkullController(type);
            return mockSkull;
        }

        /// <summary>
        /// 상태가 있는 모킹 스컬 생성
        /// </summary>
        private ISkullController CreateMockSkullWithStatus(SkullType type, bool isReady,
            float cooldown, float mana, bool canUseAbilities)
        {
            var mockSkull = new MockSkullController(type);
            mockSkull.SetStatus(new SkullStatus
            {
                isReady = isReady,
                cooldownRemaining = cooldown,
                manaRemaining = mana,
                canUseAbilities = canUseAbilities
            });
            return mockSkull;
        }

        #endregion
    }

    /// <summary>
    /// 테스트용 Mock 스컬 컨트롤러
    /// </summary>
    public class MockSkullController : ISkullController
    {
        private SkullStatus currentStatus;

        public SkullType SkullType { get; private set; }
        public SkullData SkullData { get; private set; }
        public bool IsActive { get; private set; }

        public MockSkullController(SkullType skullType)
        {
            SkullType = skullType;
            currentStatus = SkullStatus.Ready;
            IsActive = false;
        }

        public void SetStatus(SkullStatus status)
        {
            currentStatus = status;
        }

        public void SetSkullData(SkullData data)
        {
            SkullData = data;
        }

        public async Awaitable OnEquip(CancellationToken cancellationToken = default)
        {
            IsActive = true;
            await Awaitable.NextFrameAsync();
        }

        public async Awaitable OnUnequip(CancellationToken cancellationToken = default)
        {
            IsActive = false;
            await Awaitable.NextFrameAsync();
        }

        public void OnActivate()
        {
            IsActive = true;
        }

        public void OnDeactivate()
        {
            IsActive = false;
        }

        public async Awaitable PerformPrimaryAttack(CancellationToken cancellationToken = default)
        {
            await Awaitable.NextFrameAsync();
        }

        public async Awaitable PerformSecondaryAttack(CancellationToken cancellationToken = default)
        {
            await Awaitable.NextFrameAsync();
        }

        public async Awaitable PerformUltimate(CancellationToken cancellationToken = default)
        {
            await Awaitable.NextFrameAsync();
        }

        public async Awaitable PerformSkullThrow(Vector2 direction, CancellationToken cancellationToken = default)
        {
            await Awaitable.NextFrameAsync();
        }

        public void OnUpdate()
        {
            // Mock implementation - 아무 동작 안함
        }

        public void OnFixedUpdate()
        {
            // Mock implementation - 아무 동작 안함
        }

        public SkullStatus GetStatus()
        {
            return currentStatus;
        }
    }
}
