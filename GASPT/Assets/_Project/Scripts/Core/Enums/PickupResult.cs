namespace GASPT.Core.Enums
{
    /// <summary>
    /// 아이템 획득 결과 코드
    /// </summary>
    public enum PickupResult
    {
        /// <summary>
        /// 획득 성공
        /// </summary>
        Success,

        /// <summary>
        /// 인벤토리 가득 참
        /// </summary>
        InventoryFull,

        /// <summary>
        /// 이미 획득됨
        /// </summary>
        AlreadyPickedUp,

        /// <summary>
        /// 거리가 너무 멀음
        /// </summary>
        TooFar,


        /// <summary>
        /// 유효하지 않은 아이템
        /// </summary>
        InvalidItem
    }
}
