namespace GASPT.Inventory
{
    /// <summary>
    /// 인벤토리 정렬 타입
    /// </summary>
    public enum InventorySortType
    {
        /// <summary>
        /// 카테고리별 정렬
        /// </summary>
        ByCategory,

        /// <summary>
        /// 희귀도별 정렬 (높은 순)
        /// </summary>
        ByRarity,

        /// <summary>
        /// 이름별 정렬 (가나다 순)
        /// </summary>
        ByName,

        /// <summary>
        /// 획득 시간별 정렬 (최신 순)
        /// </summary>
        ByAcquireTime,

        /// <summary>
        /// 장착 슬롯별 정렬
        /// </summary>
        BySlot
    }
}
