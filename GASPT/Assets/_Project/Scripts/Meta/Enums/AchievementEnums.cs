namespace GASPT.Meta.Enums
{
    /// <summary>
    /// 업적 타입
    /// </summary>
    public enum AchievementType
    {
        /// <summary>적 처치 횟수</summary>
        EnemyKill = 0,

        /// <summary>보스 처치 횟수</summary>
        BossKill = 1,

        /// <summary>런 클리어 횟수</summary>
        RunClear = 2,

        /// <summary>총 플레이 시간</summary>
        PlayTime = 3,

        /// <summary>스테이지 도달</summary>
        StageReach = 4,

        /// <summary>폼 수집</summary>
        FormCollect = 5,

        /// <summary>폼 각성</summary>
        FormAwaken = 6,

        /// <summary>재화 획득</summary>
        CurrencyEarn = 7,

        /// <summary>업그레이드 구매</summary>
        UpgradePurchase = 8,

        /// <summary>특수 조건</summary>
        Special = 9
    }

    /// <summary>
    /// 업적 등급
    /// </summary>
    public enum AchievementTier
    {
        /// <summary>브론즈</summary>
        Bronze = 0,

        /// <summary>실버</summary>
        Silver = 1,

        /// <summary>골드</summary>
        Gold = 2,

        /// <summary>플래티넘</summary>
        Platinum = 3
    }
}
