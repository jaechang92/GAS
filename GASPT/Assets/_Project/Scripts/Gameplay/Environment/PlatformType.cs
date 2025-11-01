namespace Gameplay.Environment
{
    /// <summary>
    /// 플랫폼 타입을 정의하는 Enum
    /// </summary>
    public enum PlatformType
    {
        /// <summary>일반 플랫폼 (모든 방향 충돌)</summary>
        Solid,

        /// <summary>낙하 플랫폼 (위에서만 착지)</summary>
        OneWay,

        /// <summary>이동 플랫폼 (향후 확장용)</summary>
        Moving,

        /// <summary>붕괴 플랫폼 (향후 확장용)</summary>
        Crumbling
    }
}
