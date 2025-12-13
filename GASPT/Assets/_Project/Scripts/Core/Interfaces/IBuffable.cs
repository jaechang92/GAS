using GASPT.Core.Enums;

namespace GASPT.Core
{
    /// <summary>
    /// 임시 버프를 받을 수 있는 객체 인터페이스
    /// ISP (Interface Segregation Principle) 적용
    /// - IStatusEffectTarget: 상태 효과 시스템을 통한 버프/디버프
    /// - IBuffable: 직접적인 스탯 버프 (StatusEffect 시스템 외부에서 사용)
    /// </summary>
    public interface IBuffable
    {
        /// <summary>
        /// 임시 스탯 버프 적용
        /// </summary>
        /// <param name="statType">버프할 스탯 타입</param>
        /// <param name="value">버프 값 (절대값 또는 %)</param>
        /// <param name="duration">지속 시간 (초)</param>
        /// <param name="isPercentage">true: 백분율, false: 절대값</param>
        void ApplyTemporaryBuff(StatType statType, float value, float duration, bool isPercentage = true);

        /// <summary>
        /// 특정 스탯의 버프 제거
        /// </summary>
        /// <param name="statType">제거할 버프의 스탯 타입</param>
        void RemoveBuff(StatType statType);

        /// <summary>
        /// 모든 버프 제거
        /// </summary>
        void ClearAllBuffs();

        /// <summary>
        /// 특정 스탯에 버프가 적용되어 있는지 확인
        /// </summary>
        /// <param name="statType">확인할 스탯 타입</param>
        /// <returns>버프 적용 여부</returns>
        bool HasBuff(StatType statType);

        /// <summary>
        /// 특정 스탯의 총 버프량 반환
        /// </summary>
        /// <param name="statType">확인할 스탯 타입</param>
        /// <returns>버프량 (없으면 0)</returns>
        float GetBuffAmount(StatType statType);
    }
}
