using UnityEngine;
using NUnit.Framework;
using Combat.Core;
using Core.Enums;

namespace Combat.Tests.Unit
{
    /// <summary>
    /// DamageSystem 통합 테스트
    /// </summary>
    public class DamageSystemTests
    {
        private GameObject damageSystemObject;
        private GameObject targetObject;
        private GameObject sourceObject;
        private HealthSystem targetHealth;

        [SetUp]
        public void SetUp()
        {
            // DamageSystem 생성
            damageSystemObject = new GameObject("DamageSystem");
            var damageSystem = damageSystemObject.AddComponent<DamageSystem>();

            // DamageSystem Awake 호출
            var damageAwake = typeof(DamageSystem).GetMethod("Awake",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            damageAwake?.Invoke(damageSystem, null);

            // 타겟 생성
            targetObject = new GameObject("Target");
            targetHealth = targetObject.AddComponent<HealthSystem>();

            // HealthSystem Awake 호출
            var healthAwake = typeof(HealthSystem).GetMethod("Awake",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthAwake?.Invoke(targetHealth, null);

            // 공격자 생성
            sourceObject = new GameObject("Source");
        }

        [TearDown]
        public void TearDown()
        {
            if (damageSystemObject != null)
                Object.DestroyImmediate(damageSystemObject);
            if (targetObject != null)
                Object.DestroyImmediate(targetObject);
            if (sourceObject != null)
                Object.DestroyImmediate(sourceObject);

            // DamageSystem 싱글톤 인스턴스 정리
            var instanceField = typeof(DamageSystem).GetField("instance",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            instanceField?.SetValue(null, null);
        }

        #region 기본 데미지 적용 테스트

        [Test]
        public void ApplyDamage_기본_데미지_적용()
        {
            // Arrange
            float damageAmount = 30f;

            // Act
            bool result = DamageSystem.ApplyDamage(targetObject, damageAmount, DamageType.Physical, sourceObject);

            // Assert
            Assert.IsTrue(result, "데미지 적용이 성공해야 함");
            Assert.AreEqual(70f, targetHealth.CurrentHealth, "타겟의 체력이 70으로 감소해야 함");
        }

        [Test]
        public void ApplyDamage_Null_타겟_시_실패()
        {
            // Act
            bool result = DamageSystem.ApplyDamage(null, 50f, DamageType.Physical, sourceObject);

            // Assert
            Assert.IsFalse(result, "null 타겟에 데미지 적용 실패해야 함");
        }

        [Test]
        public void ApplyDamage_HealthSystem_없는_타겟_시_실패()
        {
            // Arrange
            var noHealthTarget = new GameObject("NoHealth");

            // Act
            bool result = DamageSystem.ApplyDamage(noHealthTarget, 50f, DamageType.Physical, sourceObject);

            // Assert
            Assert.IsFalse(result, "HealthSystem 없는 타겟에 데미지 적용 실패해야 함");

            // Cleanup
            Object.DestroyImmediate(noHealthTarget);
        }

        #endregion

        #region DamageData 테스트

        [Test]
        public void ApplyDamage_DamageData_사용()
        {
            // Arrange
            var damageData = DamageData.Create(40f, DamageType.Magical, sourceObject);

            // Act
            bool result = DamageSystem.ApplyDamage(targetObject, damageData);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(60f, targetHealth.CurrentHealth, "체력이 60으로 감소해야 함");
        }

        [Test]
        public void ApplyDamageWithKnockback_넉백_포함_데미지()
        {
            // Arrange
            Vector2 knockback = Vector2.right * 5f;

            // Act
            bool result = DamageSystem.ApplyDamageWithKnockback(
                targetObject, 25f, DamageType.Physical, sourceObject, knockback);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(75f, targetHealth.CurrentHealth, "체력이 75로 감소해야 함");
        }

        #endregion

        #region 데미지 타입 테스트

        [Test]
        public void ApplyDamage_물리_데미지_타입()
        {
            // Act
            DamageSystem.ApplyDamage(targetObject, 30f, DamageType.Physical, sourceObject);

            // Assert
            Assert.AreEqual(70f, targetHealth.CurrentHealth);
        }

        [Test]
        public void ApplyDamage_마법_데미지_타입()
        {
            // Act
            DamageSystem.ApplyDamage(targetObject, 30f, DamageType.Magical, sourceObject);

            // Assert
            Assert.AreEqual(70f, targetHealth.CurrentHealth);
        }

        [Test]
        public void ApplyDamage_고정_데미지_타입()
        {
            // Act
            DamageSystem.ApplyDamage(targetObject, 30f, DamageType.True, sourceObject);

            // Assert
            Assert.AreEqual(70f, targetHealth.CurrentHealth);
        }

        [Test]
        public void ApplyDamage_환경_데미지_무적_무시()
        {
            // Arrange
            targetHealth.SetInvincible(true);

            // Act
            DamageSystem.ApplyDamage(targetObject, 30f, DamageType.Environmental, sourceObject);

            // Assert
            Assert.AreEqual(70f, targetHealth.CurrentHealth, "환경 데미지는 무적을 무시해야 함");
        }

        #endregion

        #region 범위 데미지 테스트

        [Test]
        public void ApplyRadialDamage_원형_범위_데미지()
        {
            // Arrange
            var target1 = CreateTargetWithHealth("Target1", Vector3.zero);
            var target2 = CreateTargetWithHealth("Target2", Vector3.right * 2f);
            var target3 = CreateTargetWithHealth("Target3", Vector3.right * 10f); // 범위 밖

            var damageData = DamageData.Create(20f, DamageType.Physical, sourceObject);
            LayerMask targetLayer = -1; // 모든 레이어

            // Act
            var hitTargets = DamageSystem.ApplyRadialDamage(Vector3.zero, 5f, damageData, targetLayer);

            // Assert
            Assert.AreEqual(2, hitTargets.Count, "2개의 타겟이 맞아야 함");
            Assert.AreEqual(80f, target1.GetComponent<HealthSystem>().CurrentHealth);
            Assert.AreEqual(80f, target2.GetComponent<HealthSystem>().CurrentHealth);
            Assert.AreEqual(100f, target3.GetComponent<HealthSystem>().CurrentHealth, "범위 밖 타겟은 안 맞아야 함");

            // Cleanup
            Object.DestroyImmediate(target1);
            Object.DestroyImmediate(target2);
            Object.DestroyImmediate(target3);
        }

        [Test]
        public void ApplyBoxDamage_박스_범위_데미지()
        {
            // Arrange
            var target1 = CreateTargetWithHealth("Target1", Vector3.zero);
            var target2 = CreateTargetWithHealth("Target2", Vector3.right * 1f);

            var damageData = DamageData.Create(15f, DamageType.Physical, sourceObject);
            LayerMask targetLayer = -1;

            // Act
            var hitTargets = DamageSystem.ApplyBoxDamage(
                Vector3.zero, new Vector2(3f, 3f), 0f, damageData, targetLayer);

            // Assert
            Assert.GreaterOrEqual(hitTargets.Count, 1, "최소 1개 이상의 타겟이 맞아야 함");

            // 개별 체력 확인
            Assert.AreEqual(85f, target1.GetComponent<HealthSystem>().CurrentHealth, "Target1이 데미지를 받아야 함");

            // Cleanup
            Object.DestroyImmediate(target1);
            Object.DestroyImmediate(target2);
        }

        #endregion

        #region 설정 테스트

        [Test]
        public void SetGlobalDamageMultiplier_전역_배율_적용()
        {
            // Arrange
            var damageSystem = damageSystemObject.GetComponent<DamageSystem>();
            damageSystem.SetGlobalDamageMultiplier(2f);

            // Act
            DamageSystem.ApplyDamage(targetObject, 25f, DamageType.Physical, sourceObject);

            // Assert
            Assert.AreEqual(50f, targetHealth.CurrentHealth, "2배 데미지로 50 감소해야 함");
        }

        [Test]
        public void SetCriticalChance_크리티컬_확률_설정()
        {
            // Arrange
            var damageSystem = damageSystemObject.GetComponent<DamageSystem>();
            damageSystem.SetCriticalChance(1f); // 100% 크리티컬

            var damageData = DamageData.CreateCritical(20f, DamageType.Physical, sourceObject, 2f);

            // Act
            DamageSystem.ApplyDamage(targetObject, damageData);

            // Assert - 크리티컬 적용으로 40 데미지 (20 * 2)
            Assert.AreEqual(60f, targetHealth.CurrentHealth, "크리티컬 데미지가 적용되어야 함");
        }

        #endregion

        #region 헬퍼 메서드

        private GameObject CreateTargetWithHealth(string name, Vector3 position)
        {
            var target = new GameObject(name);
            target.transform.position = position;

            var healthSystem = target.AddComponent<HealthSystem>();
            // EditMode 테스트에서는 Awake가 자동 호출 안됨 - 리플렉션으로 강제 호출
            var awakeMethod = typeof(HealthSystem).GetMethod("Awake",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            awakeMethod?.Invoke(healthSystem, null);

            target.AddComponent<CircleCollider2D>(); // 범위 데미지 감지용

            // Rigidbody2D 추가 (2D 물리 시스템 활성화)
            var rb = target.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            return target;
        }

        #endregion
    }
}
