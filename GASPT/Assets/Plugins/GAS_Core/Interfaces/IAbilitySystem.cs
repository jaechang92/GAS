using System;
using System.Collections.Generic;

namespace GAS.Core
{
    /// <summary>
    /// �����Ƽ �ý����� �ٽ� �������̽�
    /// �پ��� ���� �帣���� ����� �� �ֵ��� ���������� ����
    /// </summary>
    public interface IAbilitySystem
    {
        // �����Ƽ ����
        IReadOnlyDictionary<string, IAbility> Abilities { get; }
        bool HasAbility(string abilityId);
        bool TryGetAbility(string abilityId, out IAbility ability);
        void AddAbility(IAbility ability);
        void AddAbility(IAbilityData abilityData);
        void RemoveAbility(string abilityId);

        // �����Ƽ ����
        bool CanUseAbility(string abilityId);
        bool TryUseAbility(string abilityId);
        void CancelAbility(string abilityId);
        void CancelAllAbilities();
        void ResetAllCooldowns();

        // ���ҽ� ���� (������)
        bool HasResource(string resourceType);
        float GetMaxResource(string resourceType);
        float GetResource(string resourceType);
        void SetMaxResource(string resourceType, float maxValue);
        void SetResource(string resourceType, float value);
        bool ConsumeResource(string resourceType, float amount);
        void RestoreResource(string resourceType, float amount);

        // �̺�Ʈ
        event Action<string> OnAbilityAdded;
        event Action<string> OnAbilityRemoved;
        event Action<string> OnAbilityUsed;
        event Action<string> OnAbilityCancelled;
        event Action<string, float> OnResourceChanged;
    }
}