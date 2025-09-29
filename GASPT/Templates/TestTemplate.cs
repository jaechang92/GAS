using UnityEngine;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace PROJECT_NAMESPACE.Tests
{
    /// <summary>
    /// TEST_DESCRIPTION
    /// </summary>
    public class TEST_CLASS_NAME
    {
        [SetUp]
        public void SetUp()
        {
            // 테스트 초기화
        }

        [TearDown]
        public void TearDown()
        {
            // 테스트 정리
        }

        [Test]
        public void 동기_테스트_메서드()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(true);
        }

        [Test]
        public async void 비동기_테스트_메서드()
        {
            // Arrange

            // Act
            await Awaitable.NextFrameAsync();

            // Assert
            Assert.IsTrue(true);
        }
    }
}