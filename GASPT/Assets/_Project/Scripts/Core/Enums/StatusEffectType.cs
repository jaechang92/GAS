using GASPT.Core.Attributes;

namespace GASPT.Core.Enums
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
        [StatusEffectCategory(StatusEffectCategory.Buff)]
        AttackUp,

        /// <summary>
        /// 방어력 증가
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.Buff)]
        DefenseUp,

        /// <summary>
        /// 이동 속도 증가
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.Buff)]
        SpeedUp,

        /// <summary>
        /// 크리티컬 확률 증가
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.Buff)]
        CriticalRateUp,


        // ====== Debuff (디버프) ======

        /// <summary>
        /// 공격력 감소
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.Debuff)]
        AttackDown,

        /// <summary>
        /// 방어력 감소
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.Debuff)]
        DefenseDown,

        /// <summary>
        /// 이동 속도 감소
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.Debuff)]
        SpeedDown,

        /// <summary>
        /// 기절 - 행동 불가
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.Debuff)]
        Stun,

        /// <summary>
        /// 느려짐
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.Debuff)]
        Slow,


        // ====== Damage over Time (지속 데미지) ======

        /// <summary>
        /// 독 - 지속 데미지
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.DoT)]
        Poison,

        /// <summary>
        /// 화상 - 지속 데미지
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.DoT)]
        Burn,

        /// <summary>
        /// 동상 - 슬로우와 지속 데미지
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.DoT)]
        Freeze,

        /// <summary>
        /// 출혈 - 지속 데미지
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.DoT)]
        Bleed,


        // ====== Special (특수 효과 - 버프 성격) ======

        /// <summary>
        /// 무적 상태
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.Special)]
        Invincible,

        /// <summary>
        /// 재생 - 지속 회복
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.Special)]
        Regeneration,

        /// <summary>
        /// 보호막 (데미지 흡수)
        /// </summary>
        [StatusEffectCategory(StatusEffectCategory.Special)]
        Shield
    }
}
