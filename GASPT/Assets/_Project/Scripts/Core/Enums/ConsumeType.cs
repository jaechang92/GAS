namespace GASPT.Core.Enums
{
    /// <summary>
    /// 소비 아이템 효과 타입
    /// </summary>
    public enum ConsumeType
    {
        /// <summary>
        /// 즉시 HP 회복
        /// </summary>
        Heal,

        /// <summary>
        /// 지속 HP 회복
        /// </summary>
        HealOverTime,

        /// <summary>
        /// MP 회복
        /// </summary>
        RestoreMana,

        /// <summary>
        /// 버프 적용
        /// </summary>
        Buff,

        /// <summary>
        /// 상태이상 해제
        /// </summary>
        Cleanse,

        /// <summary>
        /// 이동 (귀환)
        /// </summary>
        Teleport,

        /// <summary>
        /// 부활
        /// </summary>
        Revive
    }
}
