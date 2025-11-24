using UnityEngine;
using Core.Enums;
using GASPT.StatusEffects;

namespace GASPT.UI
{
    /// <summary>
    /// BuffIcon 표시 데이터 (ViewModel)
    /// Presenter가 StatusEffect를 View용으로 변환한 결과
    /// </summary>
    public class BuffIconViewModel
    {
        /// <summary>
        /// 상태 효과 타입
        /// </summary>
        public StatusEffectType EffectType { get; }

        /// <summary>
        /// 아이콘 스프라이트
        /// </summary>
        public Sprite Icon { get; }

        /// <summary>
        /// 버프 여부 (true: 버프, false: 디버프)
        /// </summary>
        public bool IsBuff { get; }

        /// <summary>
        /// 스택 수
        /// </summary>
        public int StackCount { get; }

        /// <summary>
        /// 지속 시간
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// 남은 시간
        /// </summary>
        public float RemainingTime { get; }

        /// <summary>
        /// StatusEffect 원본 참조 (타이머 업데이트용)
        /// </summary>
        public StatusEffect Effect { get; }

        /// <summary>
        /// 생성자
        /// </summary>
        public BuffIconViewModel(StatusEffect effect)
        {
            if (effect == null)
            {
                Debug.LogError("[BuffIconViewModel] effect가 null입니다!");
                return;
            }

            Effect = effect;
            EffectType = effect.EffectType;
            Icon = effect.Icon;
            IsBuff = effect.IsBuff;
            StackCount = effect.StackCount;
            Duration = effect.Duration;
            RemainingTime = effect.RemainingTime;
        }

        /// <summary>
        /// 스택 수만 업데이트된 새 ViewModel 생성
        /// </summary>
        public BuffIconViewModel WithStackCount(int newStackCount)
        {
            // 기존 Effect의 StackCount가 이미 업데이트되었으므로
            // 새 ViewModel 생성 시 최신 값을 반영
            return new BuffIconViewModel(Effect);
        }

        /// <summary>
        /// 디버그용 ToString
        /// </summary>
        public override string ToString()
        {
            string typeStr = IsBuff ? "Buff" : "Debuff";
            return $"[BuffIconViewModel] {EffectType} ({typeStr}), Stack={StackCount}, Time={RemainingTime:F1}/{Duration:F1}";
        }
    }
}
