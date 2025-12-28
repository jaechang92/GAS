namespace GASPT.Core.Enums
{
    /// <summary>
    /// 상태 효과 카테고리
    /// </summary>
    public enum StatusEffectCategory
    {
        /// <summary>
        /// 버프 (긍정적 효과)
        /// </summary>
        Buff,

        /// <summary>
        /// 디버프 (부정적 효과)
        /// </summary>
        Debuff,

        /// <summary>
        /// 지속 데미지 (DoT, 디버프의 일종)
        /// </summary>
        DoT,

        /// <summary>
        /// 특수 효과 (버프 성격)
        /// </summary>
        Special
    }
}
