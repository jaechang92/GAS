using UnityEngine;
using NUnit.Framework;
using Combat.Core;
using Core.Enums;

namespace Combat.Tests.Unit
{
    /// <summary>
    /// HealthSystem 단위 테스트
    /// </summary>
    public class HealthSystemTests
    {
        private GameObject testObject;
        private HealthSystem healthSystem;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestHealthSystem");
            healthSystem = testObject.AddComponent<HealthSystem>();

            // EditMode 테스트에서는 Awake가 자동 호출 안됨 - 리플렉션으로 강제 호출
            var awakeMethod = typeof(HealthSystem).GetMethod("Awake",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            awakeMethod?.Invoke(healthSystem, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        #region 초기화 테스트

        [Test]
        public void HealthSystem_초기화_시_최대체력으로_설정()
        {
            // Arrange & Act (SetUp에서 생성)

            // Assert
            Assert.AreEqual(100f, healthSystem.MaxHealth, "최대 체력이 100이어야 함");
            Assert.AreEqual(100f, healthSystem.CurrentHealth, "현재 체력이 최대 체력과 같아야 함");
            Assert.AreEqual(1f, healthSystem.HealthPercentage, "체력 비율이 100%여야 함");
            Assert.IsTrue(healthSystem.IsAlive, "생존 상태여야 함");
        }

        #endregion

        #region 데미지 테스트

        [Test]
        public void TakeDamage_데미지_받으면_체력_감소()
        {
            // Arrange
            float damageAmount = 30f;

            // Act
            bool result = healthSystem.TakeDamage(damageAmount);

            // Assert
            Assert.IsTrue(result, "데미지 적용이 성공해야 함");
            Assert.AreEqual(70f, healthSystem.CurrentHealth, "체력이 70으로 감소해야 함");
            Assert.AreEqual(0.7f, healthSystem.HealthPercentage, 0.01f, "체력 비율이 70%여야 함");
        }

        [Test]
        public void TakeDamage_과도한_데미지_받으면_0으로_제한()
        {
            // Arrange
            float damageAmount = 150f;

            // Act
            healthSystem.TakeDamage(damageAmount);

            // Assert
            Assert.AreEqual(0f, healthSystem.CurrentHealth, "체력이 0이어야 함");
            Assert.IsFalse(healthSystem.IsAlive, "사망 상태여야 함");
        }

        [Test]
        public void TakeDamage_사망_후_데미지_무시()
        {
            // Arrange
            healthSystem.TakeDamage(150f); // 사망
            float currentHealth = healthSystem.CurrentHealth;

            // Act
            bool result = healthSystem.TakeDamage(50f);

            // Assert
            Assert.IsFalse(result, "사망 상태에서 데미지 적용이 실패해야 함");
            Assert.AreEqual(currentHealth, healthSystem.CurrentHealth, "체력이 변하지 않아야 함");
        }

        [Test]
        public void TakeDamage_무적_상태에서_데미지_무시()
        {
            // Arrange
            healthSystem.SetInvincible(true);

            // Act
            bool result = healthSystem.TakeDamage(50f);

            // Assert
            Assert.IsFalse(result, "무적 상태에서 데미지 적용이 실패해야 함");
            Assert.AreEqual(100f, healthSystem.CurrentHealth, "체력이 변하지 않아야 함");
        }

        [Test]
        public void TakeDamage_무적무시_플래그_시_무적_상태에서도_데미지()
        {
            // Arrange
            healthSystem.SetInvincible(true);
            var damage = DamageData.Create(30f, DamageType.True, null);
            damage.ignoreInvincibility = true;

            // Act
            bool result = healthSystem.TakeDamage(damage);

            // Assert
            Assert.IsTrue(result, "무적 무시 데미지가 적용되어야 함");
            Assert.AreEqual(70f, healthSystem.CurrentHealth, "체력이 감소해야 함");
        }

        #endregion

        #region 힐 테스트

        [Test]
        public void Heal_체력_회복()
        {
            // Arrange
            healthSystem.TakeDamage(50f); // 체력 50으로 감소

            // Act
            healthSystem.Heal(30f);

            // Assert
            Assert.AreEqual(80f, healthSystem.CurrentHealth, "체력이 80으로 회복되어야 함");
        }

        [Test]
        public void Heal_최대체력_초과_불가()
        {
            // Arrange
            healthSystem.TakeDamage(20f); // 체력 80

            // Act
            healthSystem.Heal(50f); // 130이 되려고 시도

            // Assert
            Assert.AreEqual(100f, healthSystem.CurrentHealth, "최대 체력을 초과할 수 없음");
        }

        [Test]
        public void HealFull_완전_회복()
        {
            // Arrange
            healthSystem.TakeDamage(70f); // 체력 30

            // Act
            healthSystem.HealFull();

            // Assert
            Assert.AreEqual(100f, healthSystem.CurrentHealth, "체력이 완전 회복되어야 함");
        }

        [Test]
        public void Heal_사망_상태에서_회복_불가()
        {
            // Arrange
            healthSystem.TakeDamage(150f); // 사망

            // Act
            healthSystem.Heal(50f);

            // Assert
            Assert.AreEqual(0f, healthSystem.CurrentHealth, "사망 상태에서 회복 불가");
            Assert.IsFalse(healthSystem.IsAlive);
        }

        #endregion

        #region 이벤트 테스트

        [Test]
        public void OnDamaged_이벤트_발생()
        {
            // Arrange
            bool eventFired = false;
            DamageData receivedDamage = default;

            healthSystem.OnDamaged += (damage) =>
            {
                eventFired = true;
                receivedDamage = damage;
            };

            var damageData = DamageData.Create(25f, DamageType.Physical, null);

            // Act
            healthSystem.TakeDamage(damageData);

            // Assert
            Assert.IsTrue(eventFired, "OnDamaged 이벤트가 발생해야 함");
            Assert.AreEqual(25f, receivedDamage.amount, "이벤트로 전달된 데미지량이 일치해야 함");
        }

        [Test]
        public void OnDeath_이벤트_발생()
        {
            // Arrange
            bool deathEventFired = false;
            healthSystem.OnDeath += () => deathEventFired = true;

            // Act
            healthSystem.TakeDamage(150f);

            // Assert
            Assert.IsTrue(deathEventFired, "OnDeath 이벤트가 발생해야 함");
        }

        [Test]
        public void OnHealthChanged_이벤트_발생()
        {
            // Arrange
            bool eventFired = false;
            float currentHealth = 0f;
            float maxHealth = 0f;

            healthSystem.OnHealthChanged += (current, max) =>
            {
                eventFired = true;
                currentHealth = current;
                maxHealth = max;
            };

            // Act
            healthSystem.TakeDamage(40f);

            // Assert
            Assert.IsTrue(eventFired, "OnHealthChanged 이벤트가 발생해야 함");
            Assert.AreEqual(60f, currentHealth, "현재 체력이 60이어야 함");
            Assert.AreEqual(100f, maxHealth, "최대 체력이 100이어야 함");
        }

        #endregion

        #region 체력 설정 테스트

        [Test]
        public void SetMaxHealth_최대체력_변경()
        {
            // Act
            healthSystem.SetMaxHealth(150f);

            // Assert
            Assert.AreEqual(150f, healthSystem.MaxHealth, "최대 체력이 150으로 변경되어야 함");
            Assert.AreEqual(100f, healthSystem.CurrentHealth, "현재 체력은 유지되어야 함");
        }

        [Test]
        public void SetMaxHealth_현재체력_초과_시_조정()
        {
            // Arrange
            healthSystem.SetMaxHealth(100f);

            // Act
            healthSystem.SetMaxHealth(50f);

            // Assert
            Assert.AreEqual(50f, healthSystem.MaxHealth, "최대 체력이 50으로 변경되어야 함");
            Assert.AreEqual(50f, healthSystem.CurrentHealth, "현재 체력이 최대치로 조정되어야 함");
        }

        [Test]
        public void SetCurrentHealth_직접_설정()
        {
            // Act
            healthSystem.SetCurrentHealth(75f);

            // Assert
            Assert.AreEqual(75f, healthSystem.CurrentHealth, "체력이 75로 설정되어야 함");
        }

        [Test]
        public void SetCurrentHealth_0_설정_시_사망()
        {
            // Arrange
            bool deathEventFired = false;
            healthSystem.OnDeath += () => deathEventFired = true;

            // Act
            healthSystem.SetCurrentHealth(0f);

            // Assert
            Assert.AreEqual(0f, healthSystem.CurrentHealth);
            Assert.IsFalse(healthSystem.IsAlive, "사망 상태여야 함");
            Assert.IsTrue(deathEventFired, "사망 이벤트가 발생해야 함");
        }

        #endregion
    }
}
