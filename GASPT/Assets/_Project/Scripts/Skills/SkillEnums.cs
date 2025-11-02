namespace GASPT.Skills
{
    /// <summary>
    /// 스킬 타입
    /// </summary>
    public enum SkillType
    {
        /// <summary>
        /// 데미지형 스킬
        /// </summary>
        Damage,

        /// <summary>
        /// 회복형 스킬
        /// </summary>
        Heal,

        /// <summary>
        /// 버프/디버프형 스킬
        /// </summary>
        Buff,

        /// <summary>
        /// 유틸리티형 스킬 (이동, 보호막 등)
        /// </summary>
        Utility
    }

    /// <summary>
    /// 스킬 타겟 타입
    /// </summary>
    public enum TargetType
    {
        /// <summary>
        /// 자기 자신
        /// </summary>
        Self,

        /// <summary>
        /// 단일 적
        /// </summary>
        Enemy,

        /// <summary>
        /// 범위 (여러 적)
        /// </summary>
        Area,

        /// <summary>
        /// 아군 단일
        /// </summary>
        Ally
    }
}
