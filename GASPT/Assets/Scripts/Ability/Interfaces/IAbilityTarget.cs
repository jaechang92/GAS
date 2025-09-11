// ===================================
// ����: Assets/Scripts/Ability/Interfaces/IAbilityTarget.cs
// ===================================
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// �����Ƽ Ÿ���� �� �� �ִ� ��ü�� �������̽�
    /// </summary>
    public interface IAbilityTarget
    {
        GameObject GameObject { get; }
        Transform Transform { get; }
        bool IsAlive { get; }
        bool IsTargetable { get; }

        /// <summary>
        /// ������ �ޱ�
        /// </summary>
        void TakeDamage(float damage, GameObject source);

        /// <summary>
        /// �� �ޱ�
        /// </summary>
        void Heal(float amount, GameObject source);

        /// <summary>
        /// ����/����� ����
        /// </summary>
        void ApplyEffect(string effectId, float duration, GameObject source);
    }
}