namespace GASPT.UI
{
    /// <summary>
    /// 리소스 타입 (HP, Mana 등)
    /// ResourceBar에서 사용하는 리소스 종류를 정의
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// 체력 (Health Points)
        /// </summary>
        HP,

        /// <summary>
        /// 마나 (Mana Points)
        /// </summary>
        Mana,

        /// <summary>
        /// 스태미나 (향후 확장용)
        /// </summary>
        Stamina,

        /// <summary>
        /// 실드 (향후 확장용)
        /// </summary>
        Shield
    }
}
