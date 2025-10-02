using UnityEngine;
using NUnit.Framework;
using Combat.Attack;
using Combat.Core;
using Combat.Hitbox;
using GAS.Core;
using Core.Enums;

namespace Combat.Tests.Unit
{
    /// <summary>
    /// 공격 시스템 테스트
    /// </summary>
    public class AttackSystemTests
    {
        private GameObject attackerObject;
        private GameObject targetObject;
        private ComboSystem comboSystem;
        private AttackAnimationHandler animationHandler;
        private Animator animator;
        private HealthSystem targetHealth;

        [SetUp]
        public void SetUp()
        {
            // 공격자 생성
            attackerObject = new GameObject("Attacker");
            comboSystem = attackerObject.AddComponent<ComboSystem>();
            animator = attackerObject.AddComponent<Animator>();
            animationHandler = attackerObject.AddComponent<AttackAnimationHandler>();

            // AttackAnimationHandler 설정 (EditMode 테스트용)
            animationHandler.SetAnimator(animator);
            animationHandler.SetComboSystem(comboSystem);

            // 타겟 생성
            targetObject = new GameObject("Target");
            targetHealth = targetObject.AddComponent<HealthSystem>();

            // HealthSystem Awake 호출
            var healthAwake = typeof(HealthSystem).GetMethod("Awake",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthAwake?.Invoke(targetHealth, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (attackerObject != null)
                Object.DestroyImmediate(attackerObject);
            if (targetObject != null)
                Object.DestroyImmediate(targetObject);
        }

        #region ComboSystem 테스트

        [Test]
        public void ComboSystem_초기화_상태()
        {
            // Assert
            Assert.IsFalse(comboSystem.IsComboActive, "초기 상태는 비활성화여야 함");
            Assert.AreEqual(0, comboSystem.CurrentComboIndex, "초기 콤보 인덱스는 0이어야 함");
            Assert.AreEqual(0f, comboSystem.ComboProgress, "초기 콤보 진행률은 0이어야 함");
        }

        [Test]
        public void RegisterHit_콤보_시작()
        {
            // Arrange
            bool comboStarted = false;
            comboSystem.OnComboStarted += (index) => comboStarted = true;

            // Act
            bool result = comboSystem.RegisterHit(0);

            // Assert
            Assert.IsTrue(result, "첫 타격 등록 성공해야 함");
            Assert.IsTrue(comboSystem.IsComboActive, "콤보가 활성화되어야 함");
            Assert.IsTrue(comboStarted, "콤보 시작 이벤트가 발생해야 함");
        }

        [Test]
        public void RegisterHit_콤보_진행()
        {
            // Arrange
            AddTestCombos(3);
            comboSystem.RegisterHit(0); // 0번 실행 → index 1로 진행

            bool comboAdvanced = false;
            comboSystem.OnComboAdvanced += (index) => comboAdvanced = true;

            // Act
            bool result = comboSystem.RegisterHit(1); // 1번 실행 → index 2로 진행

            // Assert
            Assert.IsTrue(result, "두 번째 타격 등록 성공해야 함");
            Assert.AreEqual(2, comboSystem.CurrentComboIndex, "콤보 인덱스가 2로 진행되어야 함");
            Assert.IsTrue(comboAdvanced, "콤보 진행 이벤트가 발생해야 함");
        }

        [Test]
        public void RegisterHit_콤보_완료()
        {
            // Arrange
            AddTestCombos(2);
            bool comboCompleted = false;
            comboSystem.OnComboCompleted += (index) => comboCompleted = true;

            // Act
            comboSystem.RegisterHit(0);
            comboSystem.RegisterHit(1);

            // Assert
            Assert.IsTrue(comboCompleted, "콤보 완료 이벤트가 발생해야 함");
            Assert.IsFalse(comboSystem.IsComboActive, "콤보가 비활성화되어야 함");
        }

        [Test]
        public void ResetCombo_콤보_리셋()
        {
            // Arrange
            AddTestCombos(3);
            comboSystem.RegisterHit(0);
            comboSystem.RegisterHit(1);

            bool comboReset = false;
            comboSystem.OnComboReset += () => comboReset = true;

            // Act
            comboSystem.ResetCombo();

            // Assert
            Assert.IsTrue(comboReset, "콤보 리셋 이벤트가 발생해야 함");
            Assert.IsFalse(comboSystem.IsComboActive, "콤보가 비활성화되어야 함");
            Assert.AreEqual(0, comboSystem.CurrentComboIndex, "콤보 인덱스가 0으로 리셋되어야 함");
        }

        [Test]
        public void AddCombo_콤보_추가()
        {
            // Arrange
            var comboData = new ComboData
            {
                comboName = "Test Combo",
                comboIndex = 0,
                damageMultiplier = 1.5f
            };

            // Act
            comboSystem.AddCombo(comboData);

            // Assert
            Assert.AreEqual(1, comboSystem.GetComboCount(), "콤보 개수가 1이어야 함");
            Assert.AreEqual(comboData, comboSystem.GetComboData(0), "추가한 콤보 데이터와 일치해야 함");
        }

        [Test]
        public void RemoveCombo_콤보_제거()
        {
            // Arrange
            AddTestCombos(3);

            // Act
            comboSystem.RemoveCombo(1);

            // Assert
            Assert.AreEqual(2, comboSystem.GetComboCount(), "콤보 개수가 2여야 함");
        }

        [Test]
        public void ClearCombos_모든_콤보_제거()
        {
            // Arrange
            AddTestCombos(5);

            // Act
            comboSystem.ClearCombos();

            // Assert
            Assert.AreEqual(0, comboSystem.GetComboCount(), "콤보 개수가 0이어야 함");
            Assert.IsFalse(comboSystem.IsComboActive, "콤보가 비활성화되어야 함");
        }

        [Test]
        public void SetComboWindowTime_윈도우_시간_설정()
        {
            // Act
            comboSystem.SetComboWindowTime(1.5f);

            // Assert
            // (내부 값은 직접 확인 불가, 에러 없이 동작하는지만 확인)
            Assert.Pass();
        }

        [Test]
        public void SetComboResetTime_리셋_시간_설정()
        {
            // Act
            comboSystem.SetComboResetTime(2f);

            // Assert
            Assert.Pass();
        }

        #endregion

        #region AttackAnimationHandler 테스트

        [Test]
        public void AttackAnimationHandler_초기화_상태()
        {
            // Assert
            Assert.IsFalse(animationHandler.IsAttacking, "초기 상태는 공격하지 않음이어야 함");
            Assert.AreEqual(0, animationHandler.CurrentAnimationComboIndex, "초기 콤보 인덱스는 0이어야 함");
        }

        [Test]
        public void TriggerAttackAnimation_애니메이션_트리거()
        {
            // Act
            animationHandler.TriggerAttackAnimation(0, 1f);

            // Assert
            Assert.IsTrue(animationHandler.IsAttacking, "공격 중으로 설정되어야 함");
            Assert.AreEqual(0, animationHandler.CurrentAnimationComboIndex, "콤보 인덱스가 0이어야 함");
        }

        [Test]
        public void StopAttackAnimation_애니메이션_중단()
        {
            // Arrange
            animationHandler.TriggerAttackAnimation(0, 1f);

            // Act
            animationHandler.StopAttackAnimation();

            // Assert
            Assert.IsFalse(animationHandler.IsAttacking, "공격이 중단되어야 함");
        }

        [Test]
        public void SetAttackSpeed_공격_속도_설정()
        {
            // Act
            animationHandler.SetAttackSpeed(1.5f);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void OnAttackStart_애니메이션_이벤트_시작()
        {
            // Arrange
            bool eventFired = false;
            animationHandler.OnAttackAnimationStart += () => eventFired = true;

            // Act
            animationHandler.OnAttackStart();

            // Assert
            Assert.IsTrue(eventFired, "공격 시작 이벤트가 발생해야 함");
            Assert.IsTrue(animationHandler.IsAttacking, "공격 중으로 설정되어야 함");
        }

        [Test]
        public void OnAttackHit_애니메이션_이벤트_타격()
        {
            // Arrange
            bool eventFired = false;
            animationHandler.OnAttackAnimationHit += () => eventFired = true;

            // Act
            animationHandler.OnAttackHit();

            // Assert
            Assert.IsTrue(eventFired, "타격 이벤트가 발생해야 함");
        }

        [Test]
        public void OnAttackEnd_애니메이션_이벤트_종료()
        {
            // Arrange
            animationHandler.TriggerAttackAnimation(0, 1f);
            bool eventFired = false;
            animationHandler.OnAttackAnimationEnd += () => eventFired = true;

            // Act
            animationHandler.OnAttackEnd();

            // Assert
            Assert.IsTrue(eventFired, "공격 종료 이벤트가 발생해야 함");
            Assert.IsFalse(animationHandler.IsAttacking, "공격이 종료되어야 함");
        }

        [Test]
        public void SetAnimator_Animator_설정()
        {
            // Arrange
            var newAnimator = attackerObject.AddComponent<Animator>();

            // Act
            animationHandler.SetAnimator(newAnimator);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void SetComboSystem_ComboSystem_설정()
        {
            // Arrange
            var newComboSystem = attackerObject.AddComponent<ComboSystem>();

            // Act
            animationHandler.SetComboSystem(newComboSystem);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void SetDefaultAttackSpeed_기본_속도_설정()
        {
            // Act
            animationHandler.SetDefaultAttackSpeed(2f);

            // Assert
            Assert.Pass();
        }

        #endregion

        #region 헬퍼 메서드

        /// <summary>
        /// 테스트용 콤보 추가
        /// </summary>
        private void AddTestCombos(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var comboData = new ComboData
                {
                    comboName = $"Combo {i}",
                    comboIndex = i,
                    damageMultiplier = 1f + (i * 0.1f),
                    hitboxSize = Vector2.one,
                    hitboxOffset = Vector2.zero,
                    hitboxDuration = 0.2f
                };
                comboSystem.AddCombo(comboData);
            }
        }

        #endregion
    }
}
