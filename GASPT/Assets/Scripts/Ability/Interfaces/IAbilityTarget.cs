// ===================================
// 파일: Assets/Scripts/Ability/Interfaces/IAbilityTarget.cs
// ===================================
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// 어빌리티 타겟이 될 수 있는 객체의 인터페이스
    /// </summary>
    public interface IAbilityTarget
    {
        GameObject GameObject { get; }
        Transform Transform { get; }
        bool IsAlive { get; }
        bool IsTargetable { get; }

        /// <summary>
        /// 데미지 받기
        /// </summary>
        void TakeDamage(float damage, GameObject source);

        /// <summary>
        /// 힐 받기
        /// </summary>
        void Heal(float amount, GameObject source);

        /// <summary>
        /// 버프/디버프 적용
        /// </summary>
        void ApplyEffect(string effectId, float duration, GameObject source);
    }
}