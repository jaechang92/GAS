using System;
using System.Collections.Generic;

namespace GAS.Core
{
    /// <summary>
    /// 어빌리티 시스템의 핵심 인터페이스
    /// 다양한 게임 장르에서 사용할 수 있도록 범용적으로 설계
    /// </summary>
    public interface IAbilitySystem
    {
        // 어빌리티 관리
        IReadOnlyDictionary<string, IAbility> Abilities { get; }
        bool HasAbility(string abilityId);
        bool TryGetAbility(string abilityId, out IAbility ability);
        void AddAbility(IAbility ability);
        void AddAbility(IAbilityData abilityData);
        void RemoveAbility(string abilityId);

        // 어빌리티 실행
        bool CanUseAbility(string abilityId);
        bool TryUseAbility(string abilityId);
        void CancelAbility(string abilityId);
        void CancelAllAbilities();
        void ResetAllCooldowns();

        // 리소스 관리 (선택적)
        bool HasResource(string resourceType);
        float GetMaxResource(string resourceType);
        float GetResource(string resourceType);
        void SetMaxResource(string resourceType, float maxValue);
        void SetResource(string resourceType, float value);
        bool ConsumeResource(string resourceType, float amount);
        void RestoreResource(string resourceType, float amount);

        // 이벤트
        event Action<string> OnAbilityAdded;
        event Action<string> OnAbilityRemoved;
        event Action<string> OnAbilityUsed;
        event Action<string> OnAbilityCancelled;
        event Action<string, float> OnResourceChanged;
    }
}