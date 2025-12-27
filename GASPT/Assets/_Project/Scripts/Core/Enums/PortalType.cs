namespace GASPT.Core.Enums
{
    /// <summary>
    /// 포탈 타입
    /// 포탈의 이동 동작 방식을 정의
    /// </summary>
    public enum PortalType
    {
        /// <summary>
        /// 던전 내 다음 방으로 (GameFlow 사용)
        /// </summary>
        NextRoom = 0,

        /// <summary>
        /// 특정 방으로 (직접 이동)
        /// </summary>
        SpecificRoom = 1,

        /// <summary>
        /// 랜덤 방으로 (직접 이동)
        /// </summary>
        RandomRoom = 2,

        /// <summary>
        /// StartRoom에서 던전 입장 (GameFlow 사용)
        /// </summary>
        DungeonEntrance = 3,

        /// <summary>
        /// 분기 선택 (그래프 기반 - 다중 목적지)
        /// </summary>
        BranchSelection = 4
    }
}
