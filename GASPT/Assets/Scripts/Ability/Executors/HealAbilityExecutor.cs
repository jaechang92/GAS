// ===================================
// ����: Assets/Scripts/Ability/Executors/HealAbilityExecutor.cs
// ===================================
using AbilitySystem.Platformer;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// ���� �����ϴ� �����Ƽ ������
    /// </summary>
    [CreateAssetMenu(fileName = "HealExecutor", menuName = "Ability/Executors/Heal")]
    public class HealAbilityExecutor : AbilityExecutor
    {
        [Header("�� ����")]
        [SerializeField] private float healMultiplier = 1.0f;
        [SerializeField] private bool canOverheal = false;
        [SerializeField] private float overhealShieldPercent = 0.5f;

        [Header("�� Ÿ��")]
        [SerializeField] private bool isInstant = true;
        [SerializeField] private float healOverTime = 0f;
        [SerializeField] private int tickCount = 1;

        /// <summary>
        /// �� �����Ƽ ����
        /// </summary>
        public override async Awaitable ExecuteAsync(GameObject caster, PlatformerAbilityData data, List<IAbilityTarget> targets)
        {
            // Ÿ�ٿ��� �� ����
            await AwaitableHelper.CompletedTask;
        }

        /// <summary>
        /// ���� ���
        /// </summary>
        private float CalculateHealAmount(GameObject caster, PlatformerAbilityData data, IAbilityTarget target)
        {
            // ���� ���� ���
            return 0f;
        }

        /// <summary>
        /// ��� �� ����
        /// </summary>
        private void ApplyInstantHeal(GameObject caster, IAbilityTarget target, float amount)
        {
            // ��� ü�� ȸ��
        }

        /// <summary>
        /// ��Ʈ�� ����
        /// </summary>
        private async Awaitable ApplyHealOverTime(GameObject caster, IAbilityTarget target, float totalAmount)
        {
            // �ð��� ���� ü�� ȸ��
            await AwaitableHelper.CompletedTask;
        }

        /// <summary>
        /// ������ ó��
        /// </summary>
        private void HandleOverheal(IAbilityTarget target, float overhealAmount)
        {
            // �ʰ� ���� ��ȣ������ ��ȯ
        }

        /// <summary>
        /// �� ����Ʈ ǥ��
        /// </summary>
        private void ShowHealEffect(Vector3 position, float amount)
        {
            // �� �ð� ȿ�� �� ���� ǥ��
        }
    }
}