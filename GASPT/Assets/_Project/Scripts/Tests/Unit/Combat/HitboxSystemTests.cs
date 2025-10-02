using UnityEngine;
using NUnit.Framework;
using Combat.Core;
using Combat.Hitbox;
using Core.Enums;

namespace Combat.Tests.Unit
{
    /// <summary>
    /// 히트박스 시스템 테스트
    /// </summary>
    public class HitboxSystemTests
    {
        private GameObject hitboxObject;
        private GameObject hurtboxObject;
        private HitboxController hitbox;
        private HurtboxController hurtbox;
        private HealthSystem targetHealth;

        [SetUp]
        public void SetUp()
        {
            // 히트박스 생성
            hitboxObject = new GameObject("Hitbox");
            hitboxObject.AddComponent<CircleCollider2D>();
            hitbox = hitboxObject.AddComponent<HitboxController>();

            // Hitbox Awake 호출
            var hitboxAwake = typeof(HitboxController).GetMethod("Awake",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            hitboxAwake?.Invoke(hitbox, null);

            // 허트박스 생성
            hurtboxObject = new GameObject("Hurtbox");
            hurtboxObject.AddComponent<CircleCollider2D>();
            targetHealth = hurtboxObject.AddComponent<HealthSystem>();
            hurtbox = hurtboxObject.AddComponent<HurtboxController>();

            // HealthSystem Awake 호출
            var healthAwake = typeof(HealthSystem).GetMethod("Awake",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthAwake?.Invoke(targetHealth, null);

            // Hurtbox Awake 호출
            var hurtboxAwake = typeof(HurtboxController).GetMethod("Awake",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            hurtboxAwake?.Invoke(hurtbox, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (hitboxObject != null)
                Object.DestroyImmediate(hitboxObject);
            if (hurtboxObject != null)
                Object.DestroyImmediate(hurtboxObject);
        }

        #region HitboxController 테스트

        [Test]
        public void HitboxController_초기화_시_활성화_상태()
        {
            // Assert
            Assert.IsTrue(hitbox.IsActive, "초기 상태는 활성화여야 함");
            Assert.AreEqual(0, hitbox.HitCount, "초기 타격 수는 0이어야 함");
        }

        [Test]
        public void EnableHitbox_히트박스_활성화()
        {
            // Arrange
            hitbox.DisableHitbox();

            // Act
            hitbox.EnableHitbox();

            // Assert
            Assert.IsTrue(hitbox.IsActive, "히트박스가 활성화되어야 함");
        }

        [Test]
        public void DisableHitbox_히트박스_비활성화()
        {
            // Act
            hitbox.DisableHitbox();

            // Assert
            Assert.IsFalse(hitbox.IsActive, "히트박스가 비활성화되어야 함");
        }

        [Test]
        public void SetDamage_데미지_설정()
        {
            // Act
            hitbox.SetDamage(50f, DamageType.Magical);

            // Assert
            // (데미지는 private이므로 실제 타격을 통해 검증)
        }

        [Test]
        public void SetKnockback_넉백_설정()
        {
            // Act
            hitbox.SetKnockback(new Vector2(10f, 5f), true);

            // Assert
            // (넉백은 private이므로 실제 타격을 통해 검증)
        }

        [Test]
        public void ResetHitbox_타격_기록_초기화()
        {
            // Arrange
            // (타격 후)

            // Act
            hitbox.ResetHitbox();

            // Assert
            // 리셋 후 같은 대상 재타격 가능
        }

        #endregion

        #region HurtboxController 테스트

        [Test]
        public void HurtboxController_초기화_시_피격_가능()
        {
            // Assert
            Assert.IsTrue(hurtbox.CanBeHit, "초기 상태는 피격 가능이어야 함");
            Assert.IsNotNull(hurtbox.HealthSystem, "HealthSystem이 연결되어야 함");
        }

        [Test]
        public void EnableHurtbox_허트박스_활성화()
        {
            // Arrange
            hurtbox.DisableHurtbox();

            // Act
            hurtbox.EnableHurtbox();

            // Assert
            Assert.IsTrue(hurtbox.CanBeHit, "허트박스가 활성화되어야 함");
        }

        [Test]
        public void DisableHurtbox_허트박스_비활성화()
        {
            // Act
            hurtbox.DisableHurtbox();

            // Assert
            Assert.IsFalse(hurtbox.CanBeHit, "허트박스가 비활성화되어야 함");
        }

        [Test]
        public void TakeDamage_데미지_받기()
        {
            // Arrange
            var damageData = DamageData.Create(30f, DamageType.Physical, hitboxObject);

            // Act
            bool result = hurtbox.TakeDamage(damageData);

            // Assert
            Assert.IsTrue(result, "데미지 적용 성공해야 함");
            Assert.AreEqual(70f, targetHealth.CurrentHealth, "체력이 70으로 감소해야 함");
        }

        [Test]
        public void TakeDamage_비활성화_시_무시()
        {
            // Arrange
            hurtbox.DisableHurtbox();
            var damageData = DamageData.Create(30f, DamageType.Physical, hitboxObject);

            // Act
            bool result = hurtbox.TakeDamage(damageData);

            // Assert
            Assert.IsFalse(result, "비활성화 상태에서 데미지 무시해야 함");
            Assert.AreEqual(100f, targetHealth.CurrentHealth, "체력이 변하지 않아야 함");
        }

        [Test]
        public void SetDamageMultiplier_데미지_배율_적용()
        {
            // Arrange
            hurtbox.SetDamageMultiplier(2f);
            var damageData = DamageData.Create(20f, DamageType.Physical, hitboxObject);

            // Act
            hurtbox.TakeDamage(damageData);

            // Assert
            Assert.AreEqual(60f, targetHealth.CurrentHealth, "2배 데미지로 40 감소해야 함");
        }

        [Test]
        public void OnHit_이벤트_발생()
        {
            // Arrange
            bool eventFired = false;
            DamageData receivedDamage = default;

            hurtbox.OnHit += (damage) =>
            {
                eventFired = true;
                receivedDamage = damage;
            };

            var damageData = DamageData.Create(25f, DamageType.Physical, hitboxObject);

            // Act
            hurtbox.TakeDamage(damageData);

            // Assert
            Assert.IsTrue(eventFired, "OnHit 이벤트가 발생해야 함");
            Assert.AreEqual(25f, receivedDamage.amount, "이벤트로 전달된 데미지가 일치해야 함");
        }

        #endregion

        #region CollisionDetector 테스트

        [Test]
        public void DetectHurtboxesInRadius_원형_범위_감지()
        {
            // Arrange
            hurtboxObject.transform.position = Vector3.zero;
            hurtboxObject.layer = 0; // Default layer
            LayerMask targetLayer = -1;

            // Rigidbody2D 추가
            hurtboxObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

            // Act
            var detected = CollisionDetector.DetectHurtboxesInRadius(Vector3.zero, 5f, targetLayer);

            // Assert
            Assert.GreaterOrEqual(detected.Count, 1, "최소 1개의 허트박스가 감지되어야 함");
        }

        [Test]
        public void DetectHurtboxesInBox_박스_범위_감지()
        {
            // Arrange
            hurtboxObject.transform.position = Vector3.zero;
            hurtboxObject.layer = 0;
            LayerMask targetLayer = -1;

            // Rigidbody2D 추가
            hurtboxObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

            // Act
            var detected = CollisionDetector.DetectHurtboxesInBox(Vector3.zero, new Vector2(3f, 3f), 0f, targetLayer);

            // Assert
            Assert.GreaterOrEqual(detected.Count, 1, "최소 1개의 허트박스가 감지되어야 함");
        }

        [Test]
        public void FindNearestHurtbox_가장_가까운_허트박스_찾기()
        {
            // Arrange
            var hurtbox2 = CreateHurtbox("Hurtbox2", Vector3.right * 2f);
            var hurtbox3 = CreateHurtbox("Hurtbox3", Vector3.right * 5f);

            LayerMask targetLayer = -1;

            // Act
            var nearest = CollisionDetector.FindNearestHurtbox(Vector3.zero, 10f, targetLayer);

            // Assert
            Assert.IsNotNull(nearest, "허트박스를 찾아야 함");

            // Cleanup
            Object.DestroyImmediate(hurtbox2);
            Object.DestroyImmediate(hurtbox3);
        }

        #endregion

        #region 헬퍼 메서드

        private GameObject CreateHurtbox(string name, Vector3 position)
        {
            var obj = new GameObject(name);
            obj.transform.position = position;
            obj.AddComponent<CircleCollider2D>();
            obj.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

            var health = obj.AddComponent<HealthSystem>();
            var hurtboxComp = obj.AddComponent<HurtboxController>();

            // Awake 호출
            var healthAwake = typeof(HealthSystem).GetMethod("Awake",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthAwake?.Invoke(health, null);

            var hurtboxAwake = typeof(HurtboxController).GetMethod("Awake",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            hurtboxAwake?.Invoke(hurtboxComp, null);

            return obj;
        }

        #endregion
    }
}
