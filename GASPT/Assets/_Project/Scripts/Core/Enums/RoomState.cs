namespace GASPT.Core.Enums
{
    /// <summary>
    /// 방 상태
    /// 방의 현재 진행 상태를 정의
    /// </summary>
    public enum RoomState
    {
        /// <summary>
        /// 비활성 (진입 전)
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// 진입 중 (페이드 인)
        /// </summary>
        Entering = 1,

        /// <summary>
        /// 진행 중 (전투)
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// 클리어
        /// </summary>
        Cleared = 3,

        /// <summary>
        /// 실패
        /// </summary>
        Failed = 4
    }
}
