namespace GASPT.Core.Enums
{
    /// <summary>
    /// 보스 등급
    /// 미니보스/중간보스/최종보스
    /// </summary>
    public enum BossGrade
    {
        /// <summary>
        /// 미니보스 (1~2 페이즈, 30초~1분 전투)
        /// </summary>
        MiniBoss = 0,

        /// <summary>
        /// 중간보스 (2~3 페이즈, 1~2분 전투)
        /// </summary>
        MidBoss = 1,

        /// <summary>
        /// 최종보스 (3~4 페이즈, 2~4분 전투)
        /// </summary>
        FinalBoss = 2
    }
}
