namespace GASPT.Core.Enums
{
    /// <summary>
    /// 소비 아이템 사용 결과 코드
    /// </summary>
    public enum UseResult
    {
        /// <summary>
        /// 사용 성공
        /// </summary>
        Success,

        /// <summary>
        /// 쿨다운 중
        /// </summary>
        OnCooldown,

        /// <summary>
        /// 소비 아이템이 아님
        /// </summary>
        NotConsumable,

        /// <summary>
        /// 효과 적용 실패
        /// </summary>
        EffectFailed,

        /// <summary>
        /// 아이템 미보유
        /// </summary>
        NotOwned
    }
}
