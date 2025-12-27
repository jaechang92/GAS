namespace GASPT.Core.Enums
{
    /// <summary>
    /// 보스 패턴 타입
    /// </summary>
    public enum PatternType
    {
        /// <summary>
        /// 근접 공격 (내려찍기, 돌진)
        /// </summary>
        Melee = 0,

        /// <summary>
        /// 원거리 공격 (투사체 발사, 빔)
        /// </summary>
        Ranged = 1,

        /// <summary>
        /// 범위 공격 (장판, 충격파)
        /// </summary>
        Area = 2,

        /// <summary>
        /// 소환 (졸개 소환)
        /// </summary>
        Summon = 3,

        /// <summary>
        /// 자기 강화 (분노, 방어 강화)
        /// </summary>
        Buff = 4,

        /// <summary>
        /// 특수 기술 (페이즈 전용 필살기)
        /// </summary>
        Special = 5
    }
}
