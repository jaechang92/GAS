namespace GASPT.Core.Enums
{
    /// <summary>
    /// 아이템 카테고리
    /// </summary>
    public enum ItemCategory
    {
        /// <summary>
        /// 장비 아이템 (장착)
        /// </summary>
        Equipment,

        /// <summary>
        /// 소비 아이템 (사용)
        /// </summary>
        Consumable,

        /// <summary>
        /// 재료 아이템 (보관)
        /// </summary>
        Material,

        /// <summary>
        /// 재화 아이템 (자동 획득)
        /// </summary>
        Currency,

        /// <summary>
        /// 퀘스트 아이템
        /// </summary>
        Quest,

        /// <summary>
        /// 스킬 아이템 (스킬 부여)
        /// </summary>
        Skill
    }
}
