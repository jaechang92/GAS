using UnityEngine;
using System;
using Core.Enums;
using Core.Utilities.Interfaces;

namespace Combat.Core
{
    /// <summary>
    /// 체력 관리 시스템
    /// 플레이어 및 적의 체력을 관리하고 데미지/힐 처리
    /// </summary>
    public class HealthSystem : MonoBehaviour, IHealthEventProvider
    {
        [Header("체력 설정")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private bool isInvincible = false;
        [SerializeField] private float invincibilityDuration = 1f;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = true;

        // 무적 타이머
        private float invincibilityTimer = 0f;

        // 이벤트
        public event Action<DamageData> OnDamaged;          // 데미지 받을 때
        public event Action<float> OnHealed;                // 힐 받을 때
        public event Action OnDeath;                        // 사망 시
        public event Action<float, float> OnHealthChanged;  // 체력 변경 시 (current, max)

        #region 프로퍼티

        /// <summary>
        /// 현재 체력
        /// </summary>
        public float CurrentHealth => currentHealth;

        /// <summary>
        /// 최대 체력
        /// </summary>
        public float MaxHealth => maxHealth;

        /// <summary>
        /// 체력 비율 (0~1)
        /// </summary>
        public float HealthPercentage => currentHealth / maxHealth;

        /// <summary>
        /// 생존 여부
        /// </summary>
        public bool IsAlive => currentHealth > 0f;

        /// <summary>
        /// 무적 상태 여부
        /// </summary>
        public bool IsInvincible => isInvincible || invincibilityTimer > 0f;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Update()
        {
            UpdateInvincibility();
        }

        #endregion

        #region 데미지 처리

        /// <summary>
        /// 데미지 받기
        /// </summary>
        public bool TakeDamage(DamageData damage)
        {
            // 사망 상태면 무시
            if (!IsAlive)
            {
                return false;
            }

            // 무적 상태 체크 (무시 플래그 확인)
            if (IsInvincible && !damage.ignoreInvincibility)
            {
                LogDebug($"무적 상태로 데미지 무시: {damage.amount}");
                return false;
            }

            // 데미지 적용
            float actualDamage = damage.amount;
            currentHealth -= actualDamage;
            currentHealth = Mathf.Max(0f, currentHealth);

            LogDebug($"데미지 받음: {actualDamage} (남은 체력: {currentHealth}/{maxHealth})");

            // 이벤트 발생
            OnDamaged?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // 무적 시간 적용
            if (invincibilityDuration > 0f)
            {
                invincibilityTimer = invincibilityDuration;
            }

            // 사망 체크
            if (currentHealth <= 0f)
            {
                Die();
            }

            return true;
        }

        /// <summary>
        /// 즉시 데미지 (간단 버전)
        /// </summary>
        public bool TakeDamage(float amount, GameObject source = null)
        {
            var damage = DamageData.Create(amount, DamageType.True, source);
            return TakeDamage(damage);
        }

        #endregion

        #region 힐 처리

        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(float amount)
        {
            if (!IsAlive)
            {
                return;
            }

            float previousHealth = currentHealth;
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            float actualHeal = currentHealth - previousHealth;

            if (actualHeal > 0f)
            {
                LogDebug($"체력 회복: {actualHeal} (현재 체력: {currentHealth}/{maxHealth})");

                OnHealed?.Invoke(actualHeal);
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// 체력 완전 회복
        /// </summary>
        public void HealFull()
        {
            Heal(maxHealth);
        }

        #endregion

        #region 상태 관리

        /// <summary>
        /// 사망 처리
        /// </summary>
        private void Die()
        {
            LogDebug("사망!");

            OnDeath?.Invoke();
        }

        /// <summary>
        /// 무적 상태 업데이트
        /// </summary>
        private void UpdateInvincibility()
        {
            if (invincibilityTimer > 0f)
            {
                invincibilityTimer -= Time.deltaTime;
            }
        }

        /// <summary>
        /// 무적 상태 설정
        /// </summary>
        public void SetInvincible(bool invincible)
        {
            isInvincible = invincible;
        }

        /// <summary>
        /// 일시적 무적 부여
        /// </summary>
        public void GrantTemporaryInvincibility(float duration)
        {
            invincibilityTimer = Mathf.Max(invincibilityTimer, duration);
        }

        #endregion

        #region 체력 설정

        /// <summary>
        /// 최대 체력 설정
        /// </summary>
        public void SetMaxHealth(float newMaxHealth, bool healToFull = false)
        {
            maxHealth = newMaxHealth;

            if (healToFull)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// 현재 체력 직접 설정
        /// </summary>
        public void SetCurrentHealth(float health)
        {
            bool wasAlive = IsAlive;
            currentHealth = Mathf.Clamp(health, 0f, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f && wasAlive)
            {
                Die();
            }
        }

        #endregion

        #region 디버그

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[HealthSystem - {gameObject.name}] {message}");
            }
        }

        [ContextMenu("테스트: 10 데미지")]
        private void TestDamage10()
        {
            TakeDamage(10f);
        }

        [ContextMenu("테스트: 50 회복")]
        private void TestHeal50()
        {
            Heal(50f);
        }

        [ContextMenu("테스트: 완전 회복")]
        private void TestHealFull()
        {
            HealFull();
        }

        [ContextMenu("테스트: 즉시 사망")]
        private void TestKill()
        {
            SetCurrentHealth(0f);
        }

        #endregion
    }
}
