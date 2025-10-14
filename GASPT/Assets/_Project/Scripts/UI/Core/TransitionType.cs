namespace UI.Core
{
    /// <summary>
    /// UI Panel Transition 애니메이션 타입
    /// Panel이 열리고 닫힐 때 재생되는 애니메이션 정의
    /// </summary>
    public enum TransitionType
    {
        /// <summary>
        /// 애니메이션 없음 (즉시 표시/숨김)
        /// </summary>
        None,

        /// <summary>
        /// 페이드 효과 (투명도 0 → 1 또는 1 → 0)
        /// 가장 일반적인 전환 효과
        /// </summary>
        Fade,

        /// <summary>
        /// 스케일 효과 (크기 0 → 1 또는 1 → 0)
        /// 팝업에 주로 사용
        /// </summary>
        Scale,

        /// <summary>
        /// 아래에서 위로 슬라이드
        /// 하단 UI나 토스트 메시지에 사용
        /// </summary>
        SlideFromBottom,

        /// <summary>
        /// 위에서 아래로 슬라이드 (닫기용)
        /// </summary>
        SlideToBottom,

        /// <summary>
        /// 왼쪽에서 오른쪽으로 슬라이드
        /// 사이드 메뉴나 인벤토리에 사용
        /// </summary>
        SlideFromLeft,

        /// <summary>
        /// 오른쪽에서 왼쪽으로 슬라이드 (닫기용)
        /// </summary>
        SlideToLeft,

        /// <summary>
        /// 오른쪽에서 왼쪽으로 슬라이드
        /// </summary>
        SlideFromRight,

        /// <summary>
        /// 왼쪽에서 오른쪽으로 슬라이드 (닫기용)
        /// </summary>
        SlideToRight
    }
}
