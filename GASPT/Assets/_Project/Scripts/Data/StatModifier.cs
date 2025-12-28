using System;
using GASPT.Core.Enums;

namespace GASPT.Data
{
    /// <summary>
    /// 스탯 수정자 구조체
    /// 장비 및 버프에서 스탯 보너스를 정의하는데 사용
    /// </summary>
    [Serializable]
    public struct StatModifier
    {
        /// <summary>
        /// 수정할 스탯 종류
        /// </summary>
        public StatType statType;

        /// <summary>
        /// 수정 방식 (Flat: 고정값, Percent: 퍼센트)
        /// </summary>
        public ModifierType modifierType;

        /// <summary>
        /// 수정 수치
        /// </summary>
        public float value;

        /// <summary>
        /// StatModifier 생성자
        /// </summary>
        /// <param name="type">스탯 종류</param>
        /// <param name="modType">수정 방식</param>
        /// <param name="val">수정 수치</param>
        public StatModifier(StatType type, ModifierType modType, float val)
        {
            statType = type;
            modifierType = modType;
            value = val;
        }

        /// <summary>
        /// 디버그용 문자열 출력
        /// </summary>
        public override string ToString()
        {
            string sign = value >= 0 ? "+" : "";
            string suffix = modifierType == ModifierType.Percent ? "%" : "";
            return $"{statType}: {sign}{value}{suffix}";
        }
    }
}
