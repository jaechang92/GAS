using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GAS.Core
{
    /// <summary>
    /// 개별 어빌리티의 인터페이스
    /// </summary>
    public interface IAbility
    {
        // 기본 정보
        string Id { get; }
        string Name { get; }
        string Description { get; }
        AbilityState State { get; }

        // 데이터
        IAbilityData Data { get; }

        // 실행 관련
        bool CanExecute();
        Awaitable<bool> ExecuteAsync(CancellationToken cancellationToken = default);
        void Cancel();

        // 쿨다운
        bool IsOnCooldown { get; }
        float CooldownRemaining { get; }
        float CooldownProgress { get; }

        // 이벤트
        event Action<IAbility> OnAbilityStarted;
        event Action<IAbility> OnAbilityCompleted;
        event Action<IAbility> OnAbilityCancelled;
        event Action<IAbility> OnCooldownStarted;
        event Action<IAbility> OnCooldownCompleted;
    }
}