// ���� ��ġ: Assets/Scripts/Ability/Core/AbilityCooldown.cs
using System;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// �����Ƽ ��ٿ� ���� Ŭ����
    /// </summary>
    [Serializable]
    public class AbilityCooldown
    {
        private float cooldownDuration;
        private float remainingTime;
        private bool isOnCooldown;
        private float lastUsedTime;

        // �̺�Ʈ
        public event Action OnCooldownStarted;
        public event Action OnCooldownCompleted;
        public event Action<float> OnCooldownUpdated;

        // ������Ƽ
        public float Duration => cooldownDuration;
        public float RemainingTime => remainingTime;
        public bool IsOnCooldown => isOnCooldown;
        public float Progress => cooldownDuration > 0 ? 1f - (remainingTime / cooldownDuration) : 1f;

        /// <summary>
        /// ��ٿ� �ʱ�ȭ
        /// </summary>
        public void Initialize(float duration)
        {
            cooldownDuration = duration;
            remainingTime = 0f;
            isOnCooldown = false;
            lastUsedTime = -duration; // ��� ��� �����ϵ���
        }

        /// <summary>
        /// ��ٿ� ����
        /// </summary>
        public void StartCooldown()
        {
            if (cooldownDuration <= 0) return;

            remainingTime = cooldownDuration;
            isOnCooldown = true;
            lastUsedTime = Time.time;

            OnCooldownStarted?.Invoke();
            Debug.Log($"Cooldown started: {cooldownDuration}s");
        }

        /// <summary>
        /// ��ٿ� ���� (Ŀ���� �ð�)
        /// </summary>
        public void StartCooldown(float customDuration)
        {
            if (customDuration <= 0) return;

            cooldownDuration = customDuration;
            remainingTime = customDuration;
            isOnCooldown = true;
            lastUsedTime = Time.time;

            OnCooldownStarted?.Invoke();
            Debug.Log($"Cooldown started with custom duration: {customDuration}s");
        }

        /// <summary>
        /// ��ٿ� ������Ʈ (�� ������ ȣ��)
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!isOnCooldown) return;

            remainingTime -= deltaTime;
            OnCooldownUpdated?.Invoke(remainingTime);

            if (remainingTime <= 0)
            {
                CompleteCooldown();
            }
        }

        /// <summary>
        /// ��ٿ� ��� �Ϸ�
        /// </summary>
        public void CompleteCooldown()
        {
            if (!isOnCooldown) return;

            remainingTime = 0f;
            isOnCooldown = false;

            OnCooldownCompleted?.Invoke();
            Debug.Log("Cooldown completed");
        }

        /// <summary>
        /// ��ٿ� ����
        /// </summary>
        public void Reset()
        {
            remainingTime = 0f;
            isOnCooldown = false;
            lastUsedTime = -cooldownDuration;

            Debug.Log("Cooldown reset");
        }

        /// <summary>
        /// ��ٿ� ���� (������/���� ȿ��)
        /// </summary>
        public void ReduceCooldown(float amount)
        {
            if (!isOnCooldown || amount <= 0) return;

            remainingTime = Mathf.Max(0, remainingTime - amount);

            if (remainingTime <= 0)
            {
                CompleteCooldown();
            }
            else
            {
                OnCooldownUpdated?.Invoke(remainingTime);
            }

            Debug.Log($"Cooldown reduced by {amount}s, remaining: {remainingTime}s");
        }

        /// <summary>
        /// ��ٿ� ���� ���� (0~1)
        /// </summary>
        public void ReduceCooldownByPercent(float percent)
        {
            if (!isOnCooldown || percent <= 0) return;

            float reduction = remainingTime * Mathf.Clamp01(percent);
            ReduceCooldown(reduction);
        }

        /// <summary>
        /// ��ٿ� �ð� ���� (����/����� ȿ��)
        /// </summary>
        public void ModifyCooldownDuration(float newDuration)
        {
            cooldownDuration = Mathf.Max(0, newDuration);

            // ���� ��ٿ� ���̸� ���� ����
            if (isOnCooldown && cooldownDuration > 0)
            {
                float progressRatio = Progress;
                remainingTime = cooldownDuration * (1f - progressRatio);
            }
        }

        /// <summary>
        /// ��ٿ� ������ ���� (0~1, 0.2 = 20% ����)
        /// </summary>
        public void ApplyCooldownReduction(float reductionRate)
        {
            float reduction = Mathf.Clamp01(reductionRate);
            ModifyCooldownDuration(cooldownDuration * (1f - reduction));
        }

        /// <summary>
        /// ��� ���� ���� üũ
        /// </summary>
        public bool CanUse()
        {
            return !isOnCooldown;
        }

        /// <summary>
        /// ������ ��� �� ��� �ð�
        /// </summary>
        public float GetTimeSinceLastUse()
        {
            return Time.time - lastUsedTime;
        }
    }
}