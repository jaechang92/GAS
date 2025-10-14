using System;

namespace Core.Utilities.Interfaces
{
    /// <summary>
    /// 체력 변경 이벤트를 제공하는 인터페이스
    /// UI 시스템이 Combat 시스템에 직접 의존하지 않도록 분리
    /// </summary>
    public interface IHealthEventProvider
    {
        /// <summary>
        /// 체력 변경 이벤트 (current, max)
        /// </summary>
        event Action<float, float> OnHealthChanged;

        /// <summary>
        /// 현재 체력
        /// </summary>
        float CurrentHealth { get; }

        /// <summary>
        /// 최대 체력
        /// </summary>
        float MaxHealth { get; }
    }
}
