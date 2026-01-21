using System;
using UnityEngine;

namespace MVP_Core
{
    /// <summary>
    /// MonoBehaviour 기반 View 베이스 클래스
    /// 공통 기능 제공: 표시/숨김, CanvasGroup 애니메이션
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ViewBase : MonoBehaviour, IView
    {
        // ====== 컴포넌트 참조 ======

        [Header("View Base Settings")]
        [SerializeField] [Tooltip("애니메이션에 사용할 CanvasGroup")]
        protected CanvasGroup canvasGroup;

        [SerializeField] [Tooltip("표시/숨김 시 GameObject 활성화 제어")]
        protected bool useGameObjectActivation = true;

        // ====== 상태 ======

        private bool isVisible;

        // ====== IView 구현 ======

        /// <summary>
        /// View가 현재 표시 중인지 여부
        /// </summary>
        public bool IsVisible => isVisible;

        /// <summary>
        /// View 표시
        /// </summary>
        public virtual void Show()
        {
            if (isVisible) return;

            isVisible = true;

            if (useGameObjectActivation)
            {
                gameObject.SetActive(true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            OnShow();
        }

        /// <summary>
        /// View 숨김
        /// </summary>
        public virtual void Hide()
        {
            if (!isVisible) return;

            isVisible = false;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (useGameObjectActivation)
            {
                gameObject.SetActive(false);
            }

            OnHide();
        }

        // ====== Unity 생명주기 ======

        protected virtual void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        protected virtual void Start()
        {
            // 초기 상태 설정
            isVisible = gameObject.activeSelf;
        }

        // ====== 오버라이드 가능한 콜백 ======

        /// <summary>
        /// View 표시 후 호출
        /// </summary>
        protected virtual void OnShow() { }

        /// <summary>
        /// View 숨김 후 호출
        /// </summary>
        protected virtual void OnHide() { }
    }

    /// <summary>
    /// 토글 가능한 View 베이스 클래스
    /// 열기/닫기 이벤트 제공
    /// </summary>
    public abstract class ToggleableViewBase : ViewBase, IToggleableView
    {
        // ====== 이벤트 ======

        public event Action OnOpenRequested;
        public event Action OnCloseRequested;

        // ====== IToggleableView 구현 ======

        /// <summary>
        /// 토글 (열려있으면 닫고, 닫혀있으면 열기)
        /// </summary>
        public void Toggle()
        {
            if (IsVisible)
            {
                RequestClose();
            }
            else
            {
                RequestOpen();
            }
        }

        // ====== 이벤트 발생 ======

        /// <summary>
        /// 열기 요청 (이벤트 발생)
        /// </summary>
        protected void RequestOpen()
        {
            OnOpenRequested?.Invoke();
        }

        /// <summary>
        /// 닫기 요청 (이벤트 발생)
        /// </summary>
        protected void RequestClose()
        {
            OnCloseRequested?.Invoke();
        }
    }

    /// <summary>
    /// 메시지 표시 가능한 View 베이스 클래스
    /// </summary>
    public abstract class MessageViewBase : ToggleableViewBase, IMessageView
    {
        // ====== IMessageView 구현 ======

        /// <summary>
        /// 에러 메시지 표시 (파생 클래스에서 구현)
        /// </summary>
        public abstract void ShowError(string message);

        /// <summary>
        /// 성공 메시지 표시 (파생 클래스에서 구현)
        /// </summary>
        public abstract void ShowSuccess(string message);

        /// <summary>
        /// 정보 메시지 표시 (파생 클래스에서 구현)
        /// </summary>
        public abstract void ShowInfo(string message);
    }
}
