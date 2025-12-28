namespace GASPT.Core.Enums
{
    /// <summary>
    /// 장비 장착 결과 코드
    /// </summary>
    public enum EquipResult
    {
        /// <summary>
        /// 장착 성공
        /// </summary>
        Success,

        /// <summary>
        /// 인벤토리 가득 참 (기존 장비를 인벤토리에 넣을 공간 없음)
        /// </summary>
        InventoryFull,

        /// <summary>
        /// 레벨 부족
        /// </summary>
        LevelTooLow,

        /// <summary>
        /// 폼 불일치
        /// </summary>
        WrongForm,

        /// <summary>
        /// 잘못된 슬롯
        /// </summary>
        InvalidSlot,

        /// <summary>
        /// 아이템 미보유
        /// </summary>
        ItemNotOwned
    }
}
