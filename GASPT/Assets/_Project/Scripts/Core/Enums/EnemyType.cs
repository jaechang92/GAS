namespace Core.Enums
{
    /// <summary>
    /// 적 타입 분류
    /// </summary>
    public enum EnemyType
    {
        /// <summary>
        /// 일반 몹 (30 HP, 5 Attack, 15-25 gold)
        /// </summary>
        Normal,

        /// <summary>
        /// 네임드 몹 (60 HP, 10 Attack, 40-60 gold)
        /// </summary>
        Named,

        /// <summary>
        /// 보스 몹 (150 HP, 15 Attack, 100-150 gold)
        /// </summary>
        Boss
    }
}
