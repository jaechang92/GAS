

// ===================================
// ����: Assets/Scripts/Ability/Executors/BuffAbilityExecutor.cs
// ===================================
using AbilitySystem.Platformer;
using Helper;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// ����/������� �����ϴ� �����Ƽ ������
    /// </summary>
    [CreateAssetMenu(fileName = "BuffExecutor", menuName = "Ability/Executors/Buff")]
    public class BuffAbilityExecutor : AbilityExecutor
    {
        [Header("���� ����")]
        [SerializeField] private string buffId;
        [SerializeField] private float duration = 5f;
        [SerializeField] private bool isStackable = false;
        [SerializeField] private int maxStacks = 1;

        [Header("���� ����")]
        [SerializeField] private float attackModifier = 0f;
        [SerializeField] private float defenseModifier = 0f;
        [SerializeField] private float speedModifier = 0f;
        [SerializeField] private float cooldownReduction = 0f;

        [Header("���� Ÿ��")]
        [SerializeField] private bool isBuff = true;  // true: ����, false: �����
        [SerializeField] private bool isDispellable = true;
        [SerializeField] private bool isPurgeable = true;

        /// <summary>
        /// ���� �����Ƽ ����
        /// </summary>
        public override async Awaitable ExecuteAsync(GameObject caster, PlatformerAbilityData data, List<IAbilityTarget> targets)
        {
            // Ÿ�ٿ��� ����/����� ����
            await AwaitableHelper.CompletedTask;
        }

        /// <summary>
        /// ���� ����
        /// </summary>
        private void ApplyBuff(GameObject caster, IAbilityTarget target)
        {
            // ���� ȿ�� ����
        }

        /// <summary>
        /// ����� ����
        /// </summary>
        private void ApplyDebuff(GameObject caster, IAbilityTarget target)
        {
            // ����� ȿ�� ����
        }

        /// <summary>
        /// ���� ó��
        /// </summary>
        private void HandleStacking(IAbilityTarget target)
        {
            // ���� ���� ����
        }

        /// <summary>
        /// ���� ����
        /// </summary>
        private void RefreshBuff(IAbilityTarget target)
        {
            // ���� ���� ���ӽð� ����
        }

        /// <summary>
        /// ���� ����
        /// </summary>
        private void RemoveBuff(IAbilityTarget target)
        {
            // ���� ȿ�� ����
        }

        /// <summary>
        /// ���� ������ ǥ��
        /// </summary>
        private void ShowBuffIcon(IAbilityTarget target, bool isActive)
        {
            // UI�� ���� ������ ǥ��/����
        }
    }
}