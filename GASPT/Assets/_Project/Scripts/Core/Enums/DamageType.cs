namespace GASPT.Core.Enums
{
    /// <summary>
    /// 대미지 타입
    /// 대미지의 속성 및 종류를 정의
    /// </summary>
    public enum DamageType
    {
        /// <summary>
        /// 일반 대미지
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 물리 대미지
        /// </summary>
        Physical = 1,

        /// <summary>
        /// 마법 대미지
        /// </summary>
        Magical = 2,

        /// <summary>
        /// 화염 대미지
        /// </summary>
        Fire = 3,

        /// <summary>
        /// 얼음 대미지
        /// </summary>
        Ice = 4,

        /// <summary>
        /// 번개 대미지
        /// </summary>
        Lightning = 5,

        /// <summary>
        /// 독 대미지
        /// </summary>
        Poison = 6,

        /// <summary>
        /// 출혈 대미지
        /// </summary>
        Bleed = 7,

        /// <summary>
        /// 고정 대미지 (방어 무시)
        /// </summary>
        True = 8
    }
}
