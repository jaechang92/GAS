using UnityEngine;

namespace MVP_Core
{
    /// <summary>
    /// 범용 버프/디버프 아이콘 ViewModel
    /// </summary>
    public class BuffIconViewModel : ViewModelBase
    {
        /// <summary>
        /// 효과 ID (고유 식별자)
        /// </summary>
        public string EffectId { get; }

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
        /// 총 지속 시간 (초)
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// 남은 시간 (초)
        /// </summary>
        public float RemainingTime { get; }

        /// <summary>
        /// 남은 시간 비율 (0.0 ~ 1.0)
        /// </summary>
        public float TimeRatio => Duration > 0 ? Mathf.Clamp01(RemainingTime / Duration) : 0f;

        /// <summary>
        /// 영구 효과 여부 (Duration <= 0)
        /// </summary>
        public bool IsPermanent => Duration <= 0;

        /// <summary>
        /// 표시명 (툴팁용)
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 설명 (툴팁용)
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 테두리 색상 (버프/디버프 구분용)
        /// </summary>
        public Color BorderColor { get; }

        /// <summary>
        /// 생성자
        /// </summary>
        public BuffIconViewModel(
            string effectId,
            Sprite icon,
            bool isBuff,
            int stackCount,
            float duration,
            float remainingTime,
            string displayName = "",
            string description = "",
            Color? borderColor = null)
        {
            EffectId = effectId;
            Icon = icon;
            IsBuff = isBuff;
            StackCount = stackCount;
            Duration = duration;
            RemainingTime = remainingTime;
            DisplayName = displayName;
            Description = description;
            BorderColor = borderColor ?? (isBuff ? Color.green : Color.red);
        }

        /// <summary>
        /// 남은 시간만 업데이트된 새 ViewModel 반환
        /// </summary>
        public BuffIconViewModel WithRemainingTime(float newRemainingTime)
        {
            return new BuffIconViewModel(
                EffectId, Icon, IsBuff, StackCount, Duration, newRemainingTime,
                DisplayName, Description, BorderColor);
        }

        /// <summary>
        /// 스택 수만 업데이트된 새 ViewModel 반환
        /// </summary>
        public BuffIconViewModel WithStackCount(int newStackCount)
        {
            return new BuffIconViewModel(
                EffectId, Icon, IsBuff, newStackCount, Duration, RemainingTime,
                DisplayName, Description, BorderColor);
        }

        public override bool Equals(ViewModelBase other)
        {
            return other is BuffIconViewModel vm && EffectId == vm.EffectId;
        }

        public override int GetHashCode()
        {
            return EffectId?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            string type = IsBuff ? "Buff" : "Debuff";
            return $"[BuffIconViewModel] {EffectId} ({type}), Stack={StackCount}, Time={RemainingTime:F1}/{Duration:F1}";
        }
    }
}
