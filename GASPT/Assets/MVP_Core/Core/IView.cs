using System;

namespace MVP_Core
{
    /// <summary>
    /// MVP 패턴 기본 View 인터페이스
    /// 모든 View는 이 인터페이스를 구현하거나 상속
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// View가 현재 표시 중인지 여부
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// View 표시
        /// </summary>
        void Show();

        /// <summary>
        /// View 숨김
        /// </summary>
        void Hide();
    }

    /// <summary>
    /// 토글 가능한 View 인터페이스
    /// </summary>
    public interface IToggleableView : IView
    {
        /// <summary>
        /// View 열기 요청 이벤트
        /// </summary>
        event Action OnOpenRequested;

        /// <summary>
        /// View 닫기 요청 이벤트
        /// </summary>
        event Action OnCloseRequested;

        /// <summary>
        /// 토글 (열려있으면 닫고, 닫혀있으면 열기)
        /// </summary>
        void Toggle();
    }

    /// <summary>
    /// 메시지 표시 가능한 View 인터페이스
    /// </summary>
    public interface IMessageView : IView
    {
        /// <summary>
        /// 에러 메시지 표시
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// 성공 메시지 표시
        /// </summary>
        void ShowSuccess(string message);

        /// <summary>
        /// 정보 메시지 표시
        /// </summary>
        void ShowInfo(string message);
    }
}
