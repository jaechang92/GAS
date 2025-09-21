using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GAS.Core
{
    /// <summary>
    /// ���� �����Ƽ�� �������̽�
    /// </summary>
    public interface IAbility
    {
        // �⺻ ����
        string Id { get; }
        string Name { get; }
        string Description { get; }
        AbilityState State { get; }

        // ������
        IAbilityData Data { get; }

        // ���� ����
        bool CanExecute();
        Awaitable<bool> ExecuteAsync(CancellationToken cancellationToken = default);
        void Cancel();

        // ��ٿ�
        bool IsOnCooldown { get; }
        float CooldownRemaining { get; }
        float CooldownProgress { get; }

        // �̺�Ʈ
        event Action<IAbility> OnAbilityStarted;
        event Action<IAbility> OnAbilityCompleted;
        event Action<IAbility> OnAbilityCancelled;
        event Action<IAbility> OnCooldownStarted;
        event Action<IAbility> OnCooldownCompleted;
    }
}