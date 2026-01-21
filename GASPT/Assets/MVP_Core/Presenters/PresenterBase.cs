using System;
using UnityEngine;

namespace MVP_Core
{
    /// <summary>
    /// Presenter 추상 베이스 클래스
    /// Plain C# - Unity 없이 테스트 가능
    /// </summary>
    /// <typeparam name="TView">View 인터페이스 타입</typeparam>
    public abstract class PresenterBase<TView> : IPresenter<TView> where TView : class, IView
    {
        // ====== 참조 ======

        /// <summary>
        /// 연결된 View 참조
        /// </summary>
        public TView View { get; }

        /// <summary>
        /// 초기화 여부
        /// </summary>
        public bool IsInitialized { get; private set; }

        // ====== 상태 ======

        private bool isDisposed;

        // ====== 생성자 ======

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="view">View 인터페이스</param>
        protected PresenterBase(TView view)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        // ====== IPresenter 구현 ======

        /// <summary>
        /// Presenter 초기화
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning($"[{GetType().Name}] Already initialized.");
                return;
            }

            SubscribeToViewEvents();
            OnInitialize();
            IsInitialized = true;

            Debug.Log($"[{GetType().Name}] Initialized.");
        }

        /// <summary>
        /// Presenter 해제
        /// </summary>
        public void Dispose()
        {
            if (isDisposed) return;

            UnsubscribeFromViewEvents();
            OnDispose();
            isDisposed = true;
            IsInitialized = false;

            Debug.Log($"[{GetType().Name}] Disposed.");
        }

        // ====== 추상/가상 메서드 (파생 클래스에서 구현) ======

        /// <summary>
        /// 초기화 시 호출 (Model 참조 획득, 추가 이벤트 구독 등)
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        /// 해제 시 호출 (리소스 정리, 이벤트 구독 해제 등)
        /// </summary>
        protected abstract void OnDispose();

        /// <summary>
        /// View 이벤트 구독
        /// </summary>
        protected virtual void SubscribeToViewEvents() { }

        /// <summary>
        /// View 이벤트 구독 해제
        /// </summary>
        protected virtual void UnsubscribeFromViewEvents() { }

        // ====== 유틸리티 ======

        /// <summary>
        /// 초기화 상태 검증
        /// </summary>
        protected void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"[{GetType().Name}] Not initialized. Call Initialize() first.");
            }
        }
    }

    /// <summary>
    /// 토글 가능한 View를 위한 Presenter 베이스
    /// </summary>
    /// <typeparam name="TView">IToggleableView를 구현하는 View 타입</typeparam>
    public abstract class ToggleablePresenterBase<TView> : PresenterBase<TView>
        where TView : class, IToggleableView
    {
        protected ToggleablePresenterBase(TView view) : base(view) { }

        protected override void SubscribeToViewEvents()
        {
            base.SubscribeToViewEvents();
            View.OnOpenRequested += HandleOpenRequest;
            View.OnCloseRequested += HandleCloseRequest;
        }

        protected override void UnsubscribeFromViewEvents()
        {
            View.OnOpenRequested -= HandleOpenRequest;
            View.OnCloseRequested -= HandleCloseRequest;
            base.UnsubscribeFromViewEvents();
        }

        /// <summary>
        /// 열기 요청 처리
        /// </summary>
        protected virtual void HandleOpenRequest()
        {
            if (View.IsVisible) return;

            OnBeforeShow();
            View.Show();
            OnAfterShow();
        }

        /// <summary>
        /// 닫기 요청 처리
        /// </summary>
        protected virtual void HandleCloseRequest()
        {
            if (!View.IsVisible) return;

            OnBeforeHide();
            View.Hide();
            OnAfterHide();
        }

        /// <summary>
        /// View 표시 전 호출 (데이터 로드 등)
        /// </summary>
        protected virtual void OnBeforeShow() { }

        /// <summary>
        /// View 표시 후 호출
        /// </summary>
        protected virtual void OnAfterShow() { }

        /// <summary>
        /// View 숨김 전 호출 (데이터 저장 등)
        /// </summary>
        protected virtual void OnBeforeHide() { }

        /// <summary>
        /// View 숨김 후 호출
        /// </summary>
        protected virtual void OnAfterHide() { }
    }

    /// <summary>
    /// Model을 포함하는 Presenter 베이스
    /// </summary>
    /// <typeparam name="TView">View 인터페이스 타입</typeparam>
    /// <typeparam name="TModel">Model 타입</typeparam>
    public abstract class PresenterBase<TView, TModel> : PresenterBase<TView>, IPresenter<TView, TModel>
        where TView : class, IView
    {
        /// <summary>
        /// Model 참조
        /// </summary>
        protected TModel Model { get; private set; }

        protected PresenterBase(TView view) : base(view) { }

        /// <summary>
        /// Model 참조 설정
        /// </summary>
        public void SetModel(TModel model)
        {
            if (Model != null)
            {
                UnsubscribeFromModelEvents();
            }

            Model = model;

            if (Model != null)
            {
                SubscribeToModelEvents();
                RefreshView();
            }

            Debug.Log($"[{GetType().Name}] Model set: {model?.GetType().Name ?? "null"}");
        }

        /// <summary>
        /// Model 이벤트 구독
        /// </summary>
        protected virtual void SubscribeToModelEvents() { }

        /// <summary>
        /// Model 이벤트 구독 해제
        /// </summary>
        protected virtual void UnsubscribeFromModelEvents() { }

        /// <summary>
        /// View 갱신 (Model 데이터 → ViewModel → View)
        /// </summary>
        protected abstract void RefreshView();

        protected override void OnDispose()
        {
            if (Model != null)
            {
                UnsubscribeFromModelEvents();
                Model = default;
            }
        }
    }
}
