using System;
using GASPT.Core.Enums;

namespace GASPT.Core.Attributes
{
    /// <summary>
    /// StatusEffectType enum 값에 카테고리를 지정하는 속성
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class StatusEffectCategoryAttribute : Attribute
    {
        /// <summary>
        /// 효과 카테고리
        /// </summary>
        public StatusEffectCategory Category { get; }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="category">카테고리</param>
        public StatusEffectCategoryAttribute(StatusEffectCategory category)
        {
            Category = category;
        }
    }
}
