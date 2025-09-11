// ===================================
// ����: Assets/Scripts/Ability/Core/AbilityCooldown.cs
// ===================================
using System;

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
            // ��ٿ� �ð� ����
        }

        /// <summary>
        /// ��ٿ� ����
        /// </summary>
        public void StartCooldown()
        {
            // ��ٿ� Ÿ�̸� ����
        }

        /// <summary>
        /// ��ٿ� ���� (Ŀ���� �ð�)
        /// </summary>
        public void StartCooldown(float customDuration)
        {
            // Ư�� �ð����� ��ٿ� ����
        }

        /// <summary>
        /// ��ٿ� ������Ʈ (�� ������ ȣ��)
        /// </summary>
        public void Update(float deltaTime)
        {
            // ���� �ð� ���� �� �Ϸ� üũ
        }

        /// <summary>
        /// ��ٿ� ��� �Ϸ�
        /// </summary>
        public void CompleteCooldown()
        {
            // ��ٿ� ���� �Ϸ�
        }

        /// <summary>
        /// ��ٿ� ����
        /// </summary>
        public void Reset()
        {
            // ��ٿ� ���� �ʱ�ȭ
        }

        /// <summary>
        /// ��ٿ� ���� (������/���� ȿ��)
        /// </summary>
        public void ReduceCooldown(float amount)
        {
            // ���� ��ٿ� �ð� ����
        }

        /// <summary>
        /// ��ٿ� ���� ���� (0~1)
        /// </summary>
        public void ReduceCooldownByPercent(float percent)
        {
            // �ۼ�Ʈ ��� ��ٿ� ����
        }
    }
}