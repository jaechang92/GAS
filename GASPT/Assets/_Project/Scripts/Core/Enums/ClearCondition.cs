namespace GASPT.Core.Enums
{
    /// <summary>
    /// 방 클리어 조건
    /// 방이 클리어되기 위해 충족해야 하는 조건을 정의
    /// </summary>
    public enum ClearCondition
    {
        /// <summary>
        /// 모든 적 처치
        /// </summary>
        KillAllEnemies = 0,

        /// <summary>
        /// 생존 (시간 제한)
        /// </summary>
        Survival = 1,

        /// <summary>
        /// 보스 처치
        /// </summary>
        BossKill = 2,

        /// <summary>
        /// 자동 (시작 방, 휴식 방 등)
        /// </summary>
        Automatic = 3
    }
}
