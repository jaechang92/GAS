namespace Gameplay.Player.Physics
{
    /// <summary>
    /// 벽의 방향을 정의하는 Enum
    /// </summary>
    public enum WallDirection
    {
        /// <summary>벽에 붙어있지 않음</summary>
        None = 0,

        /// <summary>왼쪽 벽</summary>
        Left = -1,

        /// <summary>오른쪽 벽</summary>
        Right = 1
    }
}
