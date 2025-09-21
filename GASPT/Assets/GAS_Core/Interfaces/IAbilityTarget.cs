using UnityEngine;
using System.Collections.Generic;

namespace GAS.Core
{
    /// <summary>
    /// 어빌리티의 대상이 될 수 있는 객체가 구현해야 하는 인터페이스
    /// </summary>
    public interface IAbilityTarget
    {
        // 기본 정보
        GameObject GameObject { get; }
        Transform Transform { get; }
        bool IsAlive { get; }
        bool IsTargetable { get; }

        // 체력 시스템
        float CurrentHealth { get; }
        float MaxHealth { get; }

        // 팀/소속 정보
        string TeamId { get; }
        bool IsHostileTo(IAbilityTarget other);
        bool IsFriendlyTo(IAbilityTarget other);

        // 어빌리티 효과 적용
        void ApplyEffect(IGameplayEffect effect);
        void RemoveEffect(string effectId);
        bool HasEffect(string effectId);
        IReadOnlyList<IGameplayEffect> GetActiveEffects();

        // 데미지/힐링 (선택적 구현)
        void TakeDamage(float damage, IAbilityTarget source = null);
        void Heal(float amount, IAbilityTarget source = null);

        // 상태 변화 알림
        void OnBecomeTarget(IAbility ability);
        void OnLoseTarget(IAbility ability);
    }

    /// <summary>
    /// 게임플레이 효과 인터페이스
    /// </summary>
    public interface IGameplayEffect
    {
        string EffectId { get; }
        string EffectName { get; }
        float Duration { get; }
        float RemainingTime { get; }
        bool IsPermanent { get; }
        bool IsExpired { get; }

        void Apply(IAbilityTarget target);
        void Remove(IAbilityTarget target);
        void Update(float deltaTime);
    }
}