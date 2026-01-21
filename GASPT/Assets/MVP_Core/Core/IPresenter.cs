using System;

namespace MVP_Core
{
    /// <summary>
    /// MVP 패턴 기본 Presenter 인터페이스
    /// Plain C# 클래스로 구현하여 Unity 없이 테스트 가능
    /// </summary>
    public interface IPresenter : IDisposable
    {
        /// <summary>
        /// Presenter 초기화
        /// Model 참조 획득 및 이벤트 구독
        /// </summary>
        void Initialize();

        /// <summary>
        /// 초기화 여부
        /// </summary>
        bool IsInitialized { get; }
    }

    /// <summary>
    /// 제네릭 Presenter 인터페이스
    /// View 타입을 명시적으로 지정
    /// </summary>
    /// <typeparam name="TView">View 인터페이스 타입</typeparam>
    public interface IPresenter<TView> : IPresenter where TView : IView
    {
        /// <summary>
        /// 연결된 View 참조
        /// </summary>
        TView View { get; }
    }

    /// <summary>
    /// Model을 포함하는 Presenter 인터페이스
    /// </summary>
    /// <typeparam name="TView">View 인터페이스 타입</typeparam>
    /// <typeparam name="TModel">Model 타입</typeparam>
    public interface IPresenter<TView, TModel> : IPresenter<TView> where TView : IView
    {
        /// <summary>
        /// Model 참조 설정
        /// </summary>
        void SetModel(TModel model);
    }
}
