using System;

namespace Gameplay.Common
{
    /// <summary>
    /// 스컬 매니저 인터페이스
    /// 스컬 변경 이벤트를 제공
    /// </summary>
    public interface ISkullManager
    {
        /// <summary>
        /// 스컬 변경 이벤트
        /// </summary>
        event Action<ISkullController, ISkullController> OnSkullChanged;

        /// <summary>
        /// 현재 활성화된 스컬
        /// </summary>
        ISkullController CurrentSkull { get; }

        /// <summary>
        /// 스컬 교체 중인지
        /// </summary>
        bool IsSwitching { get; }
    }
}
