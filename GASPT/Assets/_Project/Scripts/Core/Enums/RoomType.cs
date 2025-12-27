namespace GASPT.Core.Enums
{
    /// <summary>
    /// 방 타입
    /// 절차적 던전 생성 시 사용
    /// </summary>
    public enum RoomType
    {
        /// <summary>
        /// 일반 전투 방
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 엘리트 적 방
        /// </summary>
        Elite = 1,

        /// <summary>
        /// 보스 방
        /// </summary>
        Boss = 2,

        /// <summary>
        /// 상점 방
        /// </summary>
        Shop = 3,

        /// <summary>
        /// 보물 방
        /// </summary>
        Treasure = 4,

        /// <summary>
        /// 휴식 방
        /// </summary>
        Rest = 5,

        /// <summary>
        /// 이벤트 방
        /// </summary>
        Event = 6,

        /// <summary>
        /// 시작 방
        /// </summary>
        Start = 7,

        /// <summary>
        /// 비어있는 방
        /// </summary>
        Empty = 8
    }
}
