// ===================================
// ����: Assets/Scripts/Ability/Core/Ability.cs
// ===================================
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// ��Ÿ�� �����Ƽ �ν��Ͻ� Ŭ����
    /// </summary>
    public class Ability
    {
        // �����Ƽ ������ ����
        private AbilityData data;

        // ���� ����
        private AbilityState currentState;
        private float currentCooldown;
        private float lastUsedTime;

        // ������ ����
        private GameObject owner;

        // �̺�Ʈ
        public event Action<Ability> OnAbilityStarted;
        public event Action<Ability> OnAbilityCompleted;
        public event Action<Ability> OnCooldownStarted;
        public event Action<Ability> OnCooldownCompleted;

        // ������Ƽ
        public string Id => data?.abilityId;
        public string Name => data?.abilityName;
        public AbilityData Data => data;
        public AbilityState State => currentState;
        public float CooldownRemaining => Mathf.Max(0, currentCooldown);
        public float CooldownProgress => data.cooldownTime > 0 ? 1 - (currentCooldown / data.cooldownTime) : 1;
        public bool IsReady => currentState == AbilityState.Ready;

        /// <summary>
        /// �����Ƽ �ʱ�ȭ
        /// </summary>
        public void Initialize(AbilityData abilityData, GameObject abilityOwner)
        {
            // �����Ϳ� ������ ����
        }

        /// <summary>
        /// �����Ƽ ��� ���� ���� üũ
        /// </summary>
        public bool CanUse()
        {
            // ����, ��ٿ�, �ڽ�Ʈ �� ���������� üũ
            return false;
        }

        /// <summary>
        /// �����Ƽ ����
        /// </summary>
        public async Awaitable ExecuteAsync()
        {
            // �����Ƽ ���� ���� (�񵿱�)
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// ��ٿ� ������Ʈ (�� ������ ȣ��)
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            // ��ٿ� �ð� ���� ó��
        }

        /// <summary>
        /// ��ٿ� ����
        /// </summary>
        private void StartCooldown()
        {
            // ��ٿ� ���� ó��
        }

        /// <summary>
        /// �����Ƽ ���� �ߴ�
        /// </summary>
        public void Cancel()
        {
            // ���� ���� �����Ƽ ���
        }

        /// <summary>
        /// �����Ƽ ����
        /// </summary>
        public void Reset()
        {
            // ���� �ʱ�ȭ
        }
    }
}