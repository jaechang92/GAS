using System;
using UnityEngine;

namespace MVP_Core
{
    /// <summary>
    /// 범용 리소스 바 ViewModel
    /// HP, Mana, Stamina, 경험치, 쿨다운 등 모든 진행률 표시에 사용
    /// </summary>
    public class ResourceBarViewModel : ViewModelBase
    {
        /// <summary>
        /// 현재 값
        /// </summary>
        public float CurrentValue { get; }

        /// <summary>
        /// 최대 값
        /// </summary>
        public float MaxValue { get; }

        /// <summary>
        /// 비율 (0.0 ~ 1.0)
        /// </summary>
        public float Ratio { get; }

        /// <summary>
        /// 표시 텍스트 (예: "75/100")
        /// </summary>
        public string DisplayText { get; }

        /// <summary>
        /// 바 색상
        /// </summary>
        public Color BarColor { get; }

        /// <summary>
        /// 리소스 종류 식별자 (프로젝트별 정의)
        /// </summary>
        public string ResourceType { get; }

        /// <summary>
        /// 값이 감소했는지 여부
        /// </summary>
        public bool IsDecreased { get; }

        /// <summary>
        /// 값이 증가했는지 여부
        /// </summary>
        public bool IsIncreased { get; }

        /// <summary>
        /// 정수 값 생성자
        /// </summary>
        public ResourceBarViewModel(
            int currentValue,
            int maxValue,
            Color barColor,
            string resourceType = "",
            bool isDecreased = false,
            bool isIncreased = false)
            : this((float)currentValue, (float)maxValue, barColor, resourceType, isDecreased, isIncreased)
        {
        }

        /// <summary>
        /// 실수 값 생성자
        /// </summary>
        public ResourceBarViewModel(
            float currentValue,
            float maxValue,
            Color barColor,
            string resourceType = "",
            bool isDecreased = false,
            bool isIncreased = false)
        {
            CurrentValue = currentValue;
            MaxValue = maxValue;
            Ratio = maxValue > 0 ? Mathf.Clamp01(currentValue / maxValue) : 0f;
            DisplayText = FormatDisplayText(currentValue, maxValue);
            BarColor = barColor;
            ResourceType = resourceType;
            IsDecreased = isDecreased;
            IsIncreased = isIncreased;
        }

        /// <summary>
        /// 표시 텍스트 포맷팅
        /// </summary>
        protected virtual string FormatDisplayText(float current, float max)
        {
            // 정수로 표시 가능하면 정수로
            if (Mathf.Approximately(current, Mathf.Floor(current)) &&
                Mathf.Approximately(max, Mathf.Floor(max)))
            {
                return $"{(int)current}/{(int)max}";
            }
            return $"{current:F1}/{max:F1}";
        }

        public override bool Equals(ViewModelBase other)
        {
            return other is ResourceBarViewModel vm &&
                   Mathf.Approximately(CurrentValue, vm.CurrentValue) &&
                   Mathf.Approximately(MaxValue, vm.MaxValue) &&
                   ResourceType == vm.ResourceType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CurrentValue, MaxValue, ResourceType);
        }

        public override string ToString()
        {
            return $"[ResourceBarViewModel] {ResourceType}: {CurrentValue}/{MaxValue} ({Ratio:P0})";
        }
    }

    /// <summary>
    /// 퍼센트 기반 리소스 바 ViewModel
    /// 쿨다운, 경험치 등에 사용
    /// </summary>
    public class PercentBarViewModel : ResourceBarViewModel
    {
        public PercentBarViewModel(
            float ratio,
            Color barColor,
            string resourceType = "",
            bool isDecreased = false,
            bool isIncreased = false)
            : base(ratio * 100f, 100f, barColor, resourceType, isDecreased, isIncreased)
        {
        }

        protected override string FormatDisplayText(float current, float max)
        {
            return $"{current:F0}%";
        }
    }
}
