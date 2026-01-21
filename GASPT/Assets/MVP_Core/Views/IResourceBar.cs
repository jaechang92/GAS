using UnityEngine;

namespace MVP_Core
{
    /// <summary>
    /// 범용 리소스 바 View 인터페이스
    /// HP, Mana, Stamina, 경험치, 쿨다운 등 모든 진행률 표시에 사용
    /// </summary>
    public interface IResourceBar : IView
    {
        /// <summary>
        /// 리소스 바 업데이트
        /// </summary>
        /// <param name="viewModel">표시할 데이터</param>
        void UpdateBar(ResourceBarViewModel viewModel);

        /// <summary>
        /// 색상 플래시 효과 (증가/감소 시 시각적 피드백)
        /// </summary>
        /// <param name="flashColor">플래시 색상</param>
        /// <param name="normalColor">복귀할 기본 색상</param>
        /// <param name="duration">플래시 지속 시간</param>
        void FlashColor(Color flashColor, Color normalColor, float duration);

        /// <summary>
        /// 바 색상 즉시 변경 (플래시 없이)
        /// </summary>
        /// <param name="color">적용할 색상</param>
        void SetBarColor(Color color);
    }

    /// <summary>
    /// 버프/디버프 아이콘 View 인터페이스
    /// </summary>
    public interface IBuffIcon : IView
    {
        /// <summary>
        /// 아이콘 데이터 설정
        /// </summary>
        void SetData(BuffIconViewModel viewModel);

        /// <summary>
        /// 타이머 업데이트 (남은 시간)
        /// </summary>
        void UpdateTimer(float remainingTime, float totalDuration);

        /// <summary>
        /// 스택 수 업데이트
        /// </summary>
        void UpdateStackCount(int count);
    }

    /// <summary>
    /// 슬롯 기반 View 인터페이스
    /// 인벤토리, 퀵슬롯, 장비 슬롯 등에 사용
    /// </summary>
    public interface ISlotView : IView
    {
        /// <summary>
        /// 슬롯 인덱스
        /// </summary>
        int SlotIndex { get; }

        /// <summary>
        /// 슬롯이 선택되었는지 여부
        /// </summary>
        bool IsSelected { get; }

        /// <summary>
        /// 슬롯 선택
        /// </summary>
        void Select();

        /// <summary>
        /// 슬롯 선택 해제
        /// </summary>
        void Deselect();

        /// <summary>
        /// 슬롯 비우기
        /// </summary>
        void Clear();
    }
}
