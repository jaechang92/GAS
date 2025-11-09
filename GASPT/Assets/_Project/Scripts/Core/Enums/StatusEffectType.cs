namespace Core.Enums
{
    /// <summary>
    /// 상태 이상 효과 타입
    /// 버프/디버프 및 지속 효과 정의
    /// </summary>
    public enum StatusEffectType
    {
        // ====== Buff (버프) ======

        /// <summary>
        /// 공격력 증가
        /// </summary>
        AttackUp,

        /// <summary>
        /// 방어력 증가
        /// </summary>
        DefenseUp,

        /// <summary>
        /// 이동 속도 증가
        /// </summary>
        SpeedUp,

        /// <summary>
        /// 크리티컬 확률 증가
        /// </summary>
        CriticalRateUp,


        // ====== Debuff (디버프) ======

        /// <summary>
        /// 공격력 감소
        /// </summary>
        AttackDown,

        /// <summary>
        /// 방어력 감소
        /// </summary>
        DefenseDown,

        /// <summary>
        /// 이동 속도 감소
        /// </summary>
        SpeedDown,

        /// <summary>
        /// 기절 - 행동 불가
        /// </summary>
        Stun,

        /// <summary>
        /// 느려짐
        /// </summary>
        Slow,


        // ====== Damage over Time (지속 데미지) ======

        /// <summary>
        /// 독 - 지속 데미지
        /// </summary>
        Poison,

        /// <summary>
        /// 화상 - 지속 데미지
        /// </summary>
        Burn,

        /// <summary>
        /// 출혈 - 지속 데미지
        /// </summary>
        Bleed,


        // ====== Special (특수 효과) ======

        /// <summary>
        /// 무적 상태
        /// </summary>
        Invincible,

        /// <summary>
        /// 재생 - 지속 회복
        /// </summary>
        Regeneration,

        /// <summary>
        /// 보호막 (데미지 흡수)
        /// </summary>
        Shield
    }
}
