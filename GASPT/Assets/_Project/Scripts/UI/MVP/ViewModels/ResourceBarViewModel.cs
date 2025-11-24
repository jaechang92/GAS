using UnityEngine;

namespace GASPT.UI
{
    /// <summary>
    /// 리소스 바 표시 데이터 (ViewModel)
    /// Presenter가 Model 데이터를 View용으로 변환한 결과
    /// </summary>
    public class ResourceBarViewModel
    {
        /// <summary>
        /// 현재 리소스 값
        /// </summary>
        public int CurrentValue { get; }

        /// <summary>
        /// 최대 리소스 값
        /// </summary>
        public int MaxValue { get; }

        /// <summary>
        /// 리소스 비율 (0.0 ~ 1.0)
        /// </summary>
        public float Ratio { get; }

        /// <summary>
        /// 표시 텍스트 (예: "75/100")
        /// </summary>
        public string DisplayText { get; }

        /// <summary>
        /// 바 색상 (현재 비율에 따른 색상)
        /// </summary>
        public Color BarColor { get; }

        /// <summary>
        /// 리소스 타입 (HP, Mana 등)
        /// </summary>
        public ResourceType Type { get; }

        /// <summary>
        /// 리소스가 감소했는지 여부 (플래시 효과 판단용)
        /// </summary>
        public bool IsDecreased { get; }

        /// <summary>
        /// 리소스가 증가했는지 여부 (플래시 효과 판단용)
        /// </summary>
        public bool IsIncreased { get; }

        /// <summary>
        /// 생성자
        /// </summary>
        public ResourceBarViewModel(
            int currentValue,
            int maxValue,
            Color barColor,
            ResourceType type,
            bool isDecreased = false,
            bool isIncreased = false)
        {
            CurrentValue = currentValue;
            MaxValue = maxValue;
            Ratio = maxValue > 0 ? (float)currentValue / maxValue : 0f;
            DisplayText = $"{currentValue}/{maxValue}";
            BarColor = barColor;
            Type = type;
            IsDecreased = isDecreased;
            IsIncreased = isIncreased;
        }

        /// <summary>
        /// 디버그용 ToString
        /// </summary>
        public override string ToString()
        {
            return $"[ResourceBarViewModel] Type={Type}, Value={CurrentValue}/{MaxValue} ({Ratio:P0}), Color={BarColor}";
        }
    }
}
