using GASPT.Core.Enums;

namespace GASPT.Core
{
    /// <summary>
    /// 상태 효과를 받을 수 있는 객체 인터페이스
    /// </summary>
    public interface IStatusEffectTarget
    {
        /// <summary>
        /// 상태 효과 적용
        /// </summary>
        /// <param name="effectType">효과 타입</param>
        /// <param name="duration">지속시간</param>
        /// <param name="value">효과 수치 (선택)</param>
        void ApplyStatusEffect(StatusEffectType effectType, float duration, float value = 0f);

        /// <summary>
        /// 상태 효과 제거
        /// </summary>
        /// <param name="effectType">제거할 효과 타입</param>
        void RemoveStatusEffect(StatusEffectType effectType);

        /// <summary>
        /// 특정 상태 효과 보유 여부
        /// </summary>
        /// <param name="effectType">확인할 효과 타입</param>
        /// <returns>보유 여부</returns>
        bool HasStatusEffect(StatusEffectType effectType);

        /// <summary>
        /// 모든 상태 효과 제거
        /// </summary>
        void ClearAllStatusEffects();

        /// <summary>
        /// 모든 디버프 제거
        /// </summary>
        void ClearDebuffs();

        /// <summary>
        /// 모든 버프 제거
        /// </summary>
        void ClearBuffs();
    }
}
