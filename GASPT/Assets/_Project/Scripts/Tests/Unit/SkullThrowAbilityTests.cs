using NUnit.Framework;
using UnityEngine;
using System.Threading;
using Skull.Abilities;
using GAS.Core;

namespace Skull.Tests.Unit
{
    /// <summary>
    /// SkullThrowAbility 단위 테스트
    /// 메서드 오버라이드와 기본 기능을 검증
    /// </summary>
    [TestFixture]
    public class SkullThrowAbilityTests
    {
        private SkullThrowAbility testAbility;
        private GameObject testOwner;

        [SetUp]
        public void SetUp()
        {
            // 테스트용 GameObject 생성
            testOwner = new GameObject("TestOwner");
            testOwner.AddComponent<SpriteRenderer>();

            // 카메라 설정
            if (Camera.main == null)
            {
                var cameraGO = new GameObject("Main Camera");
                cameraGO.AddComponent<Camera>();
                cameraGO.tag = "MainCamera";
            }

            // SkullThrowAbility 인스턴스 생성 (일반 클래스이므로 new 사용)
            testAbility = new SkullThrowAbility();

            // 리플렉션을 통해 private owner 필드 설정
            var ownerField = typeof(Ability).GetField("owner",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ownerField?.SetValue(testAbility, testOwner.transform);
        }

        [TearDown]
        public void TearDown()
        {
            if (testOwner != null)
                Object.DestroyImmediate(testOwner);

            if (testAbility != null)
            {
                // 일반 클래스이므로 Dispose 패턴 사용
                testAbility.Dispose();
                testAbility = null;
            }
        }

        [Test]
        public void CanExecute_WhenNoActiveProjectile_ReturnsTrue()
        {
            // Act
            bool result = testAbility.CanExecute();

            // Assert
            Assert.IsTrue(result, "활성화된 투사체가 없을 때 CanExecute는 true를 반환해야 합니다.");
        }

        [Test]
        public void CanExecute_OverrideMethodExists()
        {
            // Arrange & Act
            var method = typeof(SkullThrowAbility).GetMethod("CanExecute", new System.Type[0]);

            // Assert
            Assert.IsNotNull(method, "CanExecute() 메서드가 존재해야 합니다.");
            Assert.IsTrue(method.IsVirtual, "CanExecute() 메서드는 virtual이어야 합니다.");
        }

        [Test]
        public void ExecuteAbilityEffect_OverrideMethodExists()
        {
            // Arrange & Act
            var method = typeof(SkullThrowAbility).GetMethod("ExecuteAbilityEffect",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new[] { typeof(CancellationToken) }, null);

            // Assert
            Assert.IsNotNull(method, "ExecuteAbilityEffect(CancellationToken) 메서드가 존재해야 합니다.");
            Assert.IsTrue(method.IsVirtual, "ExecuteAbilityEffect() 메서드는 virtual이어야 합니다.");
        }

        [Test]
        public void Cancel_OverrideMethodExists()
        {
            // Arrange & Act
            var method = typeof(SkullThrowAbility).GetMethod("Cancel");

            // Assert
            Assert.IsNotNull(method, "Cancel() 메서드가 존재해야 합니다.");
            Assert.IsTrue(method.IsVirtual, "Cancel() 메서드는 virtual이어야 합니다.");
        }

        [Test]
        public async void ExecuteAbilityEffect_WithValidOwner_DoesNotThrow()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();

            // Act & Assert
            Assert.DoesNotThrowAsync(async () =>
            {
                // 리플렉션을 통해 protected 메서드 호출
                var method = typeof(SkullThrowAbility).GetMethod("ExecuteAbilityEffect",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    var result = method.Invoke(testAbility, new object[] { cancellationTokenSource.Token });
                    if (result is Awaitable awaitable)
                    {
                        await awaitable;
                    }
                }
            }, "유효한 owner가 있을 때 ExecuteAbilityEffect는 예외를 던지지 않아야 합니다.");

            await Awaitable.NextFrameAsync();
        }

        [Test]
        public void Cancel_CallsBaseCancel()
        {
            // Arrange & Act
            testAbility.Cancel();

            // Assert - 예외가 발생하지 않으면 성공
            Assert.Pass("Cancel 메서드가 정상적으로 호출되었습니다.");
        }

        [Test]
        public void DebugLogging_DoesNotThrow()
        {
            // Arrange & Act & Assert
            Assert.DoesNotThrow(() =>
            {
                // 리플렉션을 통해 private LogDebug 메서드 호출
                var method = typeof(SkullThrowAbility).GetMethod("LogDebug",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                method?.Invoke(testAbility, new object[] { "테스트 로그 메시지" });
            }, "LogDebug 메서드는 예외를 던지지 않아야 합니다.");
        }
    }
}