// ===================================
// ����: Assets/Scripts/Ability/Executors/DamageAbilityExecutor.cs
// ===================================
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// �������� ������ �����Ƽ ������
    /// </summary>
    [CreateAssetMenu(fileName = "DamageExecutor", menuName = "Ability/Executors/Damage")]
    public class DamageAbilityExecutor : AbilityExecutor
    {
        [Header("������ ����")]
        [SerializeField] private float damageMultiplier = 1.0f;
        [SerializeField] private bool canCritical = true;
        [SerializeField] private float criticalChance = 0.1f;
        [SerializeField] private float criticalMultiplier = 2.0f;

        [Header("Ÿ�� ����")]
        [SerializeField] private bool isPiercing = false;  // ���� ����
        [SerializeField] private int maxTargets = 1;       // �ִ� Ÿ�� ��
        [SerializeField] private float knockbackForce = 0f;

        /// <summary>
        /// ������ �����Ƽ ����
        /// </summary>
        public override async Awaitable ExecuteAsync(GameObject caster, AbilityData data, List<IAbilityTarget> targets)
        {
            // Ÿ�ٿ��� ������ ����
            await AwaitableHelper.CompletedTask;
        }

        /// <summary>
        /// ������ ���
        /// </summary>
        private float CalculateDamage(GameObject caster, AbilityData data, IAbilityTarget target)
        {
            // ���� ������ ��� (�⺻ ������ * ���� * ũ��Ƽ�� ��)
            return 0f;
        }

        /// <summary>
        /// ũ��Ƽ�� ����
        /// </summary>
        private bool RollCritical()
        {
            // ũ��Ƽ�� Ȯ�� ���
            return false;
        }

        /// <summary>
        /// �˹� ����
        /// </summary>
        private void ApplyKnockback(GameObject caster, IAbilityTarget target)
        {
            // Ÿ�ٿ��� �˹� ȿ�� ����
        }

        /// <summary>
        /// ������ ���� ǥ��
        /// </summary>
        private void ShowDamageNumber(Vector3 position, float damage, bool isCritical)
        {
            // UI�� ������ ���� ǥ��
        }
    }
}