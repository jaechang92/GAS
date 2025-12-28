using System;
using System.Reflection;
using GASPT.Core.Attributes;
using GASPT.Core.Enums;

namespace GASPT.Core.Extensions
{
    /// <summary>
    /// StatusEffectType 확장 메서드
    /// </summary>
    public static class StatusEffectTypeExtensions
    {
        /// <summary>
        /// 효과의 카테고리 반환
        /// </summary>
        /// <param name="effectType">효과 타입</param>
        /// <returns>카테고리</returns>
        public static StatusEffectCategory GetCategory(this StatusEffectType effectType)
        {
            FieldInfo field = typeof(StatusEffectType).GetField(effectType.ToString());

            if (field == null)
                return StatusEffectCategory.Debuff;

            StatusEffectCategoryAttribute attribute =
                field.GetCustomAttribute<StatusEffectCategoryAttribute>();

            return attribute?.Category ?? StatusEffectCategory.Debuff;
        }

        /// <summary>
        /// 버프인지 확인 (Buff 또는 Special)
        /// </summary>
        /// <param name="effectType">효과 타입</param>
        /// <returns>true = 버프</returns>
        public static bool IsBuff(this StatusEffectType effectType)
        {
            StatusEffectCategory category = effectType.GetCategory();
            return category == StatusEffectCategory.Buff || category == StatusEffectCategory.Special;
        }

        /// <summary>
        /// 디버프인지 확인 (Debuff 또는 DoT)
        /// </summary>
        /// <param name="effectType">효과 타입</param>
        /// <returns>true = 디버프</returns>
        public static bool IsDebuff(this StatusEffectType effectType)
        {
            StatusEffectCategory category = effectType.GetCategory();
            return category == StatusEffectCategory.Debuff || category == StatusEffectCategory.DoT;
        }

        /// <summary>
        /// 지속 데미지(DoT)인지 확인
        /// </summary>
        /// <param name="effectType">효과 타입</param>
        /// <returns>true = DoT</returns>
        public static bool IsDoT(this StatusEffectType effectType)
        {
            return effectType.GetCategory() == StatusEffectCategory.DoT;
        }

        /// <summary>
        /// 특수 효과인지 확인
        /// </summary>
        /// <param name="effectType">효과 타입</param>
        /// <returns>true = Special</returns>
        public static bool IsSpecial(this StatusEffectType effectType)
        {
            return effectType.GetCategory() == StatusEffectCategory.Special;
        }
    }
}
