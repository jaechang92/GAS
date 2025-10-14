using UnityEngine;
using UnityEngine.UI;

namespace UI.Core
{
    /// <summary>
    /// 모든 UI Panel의 기본 클래스
    /// Prefab으로 제작되며 UIManager에 의해 관리됨
    /// </summary>
    public abstract class BasePanel : MonoBehaviour
    {
        [Header("Panel 설정")]
        [SerializeField] protected PanelType panelType = PanelType.None;
        [SerializeField] protected UILayer layer = UILayer.Normal;
        [SerializeField] protected bool closeOnEscape = false;
        [SerializeField] protected bool blockRaycast = true;

        [Header("Transition 설정")]
        [SerializeField] protected TransitionType openTransition = TransitionType.Fade;
        [SerializeField] protected TransitionType closeTransition = TransitionType.Fade;
        [SerializeField] protected float transitionDuration = 0.3f;

        // 공개 속성
        public PanelType PanelType => panelType;
        public UILayer Layer => layer;
        public bool IsOpen { get; private set; }
        public bool CloseOnEscape => closeOnEscape;

        // Canvas 컴포넌트
        protected Canvas canvas;
        protected CanvasGroup canvasGroup;
        protected GraphicRaycaster raycaster;

        // 생명주기 이벤트
        public event System.Action<BasePanel> OnOpened;
        public event System.Action<BasePanel> OnClosed;

        protected virtual void Awake()
        {
            InitializeComponents();
            SetupCanvas();
            gameObject.SetActive(false); // 초기에는 숨김
        }

        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        private void InitializeComponents()
        {
            canvas = GetComponent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
            raycaster = GetComponent<GraphicRaycaster>();

            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (raycaster == null && blockRaycast)
            {
                raycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        /// <summary>
        /// Canvas 설정
        /// </summary>
        private void SetupCanvas()
        {
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = (int)layer;
            }
        }

        /// <summary>
        /// Panel 열기 (단순 활성화)
        /// </summary>
        public void Open()
        {
            if (IsOpen)
            {
                Debug.LogWarning($"[{panelType}] 이미 열려있습니다.");
                return;
            }

            gameObject.SetActive(true);
            IsOpen = true;

            OnOpened?.Invoke(this);
        }

        /// <summary>
        /// Panel 닫기 (단순 비활성화)
        /// </summary>
        public void Close()
        {
            if (!IsOpen)
            {
                Debug.LogWarning($"[{panelType}] 이미 닫혀있습니다.");
                return;
            }

            IsOpen = false;
            gameObject.SetActive(false);

            OnClosed?.Invoke(this);
        }

        /// <summary>
        /// 즉시 열기 (Transition 없이)
        /// </summary>
        public void OpenImmediate()
        {
            gameObject.SetActive(true);
            IsOpen = true;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            OnOpened?.Invoke(this);
        }

        /// <summary>
        /// 즉시 닫기 (Transition 없이)
        /// </summary>
        public void CloseImmediate()
        {
            IsOpen = false;
            gameObject.SetActive(false);
            OnClosed?.Invoke(this);
        }

        // === 하위 클래스에서 오버라이드 가능한 생명주기 메서드 ===
        protected virtual Awaitable OnBeforeOpen() => Awaitable.NextFrameAsync();
        protected virtual Awaitable OnAfterOpen() => Awaitable.NextFrameAsync();
        protected virtual Awaitable OnBeforeClose() => Awaitable.NextFrameAsync();
        protected virtual Awaitable OnAfterClose() => Awaitable.NextFrameAsync();

        /// <summary>
        /// 진행률 업데이트 (LoadingPanel 등에서 오버라이드)
        /// </summary>
        public virtual void UpdateProgress(float progress)
        {
            // 기본 구현: 아무것도 안 함
        }

        // === Transition 재생 ===
        protected virtual async Awaitable PlayOpenTransition()
        {
            switch (openTransition)
            {
                case TransitionType.None:
                    if (canvasGroup != null)
                        canvasGroup.alpha = 1f;
                    break;

                case TransitionType.Fade:
                    await FadeIn(transitionDuration);
                    break;

                case TransitionType.Scale:
                    await ScaleIn(transitionDuration);
                    break;

                case TransitionType.SlideFromBottom:
                    await SlideFromBottom(transitionDuration);
                    break;

                case TransitionType.SlideFromLeft:
                    await SlideFromLeft(transitionDuration);
                    break;

                case TransitionType.SlideFromRight:
                    await SlideFromRight(transitionDuration);
                    break;
            }
        }

        protected virtual async Awaitable PlayCloseTransition()
        {
            switch (closeTransition)
            {
                case TransitionType.None:
                    if (canvasGroup != null)
                        canvasGroup.alpha = 0f;
                    break;

                case TransitionType.Fade:
                    await FadeOut(transitionDuration);
                    break;

                case TransitionType.Scale:
                    await ScaleOut(transitionDuration);
                    break;

                case TransitionType.SlideToBottom:
                    await SlideToBottom(transitionDuration);
                    break;

                case TransitionType.SlideToLeft:
                    await SlideToLeft(transitionDuration);
                    break;

                case TransitionType.SlideToRight:
                    await SlideToRight(transitionDuration);
                    break;
            }
        }

        // === Transition 애니메이션 구현 ===

        /// <summary>
        /// 페이드 인 (투명 → 불투명)
        /// </summary>
        protected async Awaitable FadeIn(float duration)
        {
            if (canvasGroup == null) return;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                await Awaitable.NextFrameAsync();
            }
            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 페이드 아웃 (불투명 → 투명)
        /// </summary>
        protected async Awaitable FadeOut(float duration)
        {
            if (canvasGroup == null) return;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                await Awaitable.NextFrameAsync();
            }
            canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// 스케일 인 (작음 → 정상)
        /// </summary>
        protected async Awaitable ScaleIn(float duration)
        {
            transform.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // EaseOutBack 효과
                float overshoot = 1.70158f;
                t = t - 1;
                float scale = t * t * ((overshoot + 1) * t + overshoot) + 1;
                transform.localScale = Vector3.one * scale;
                await Awaitable.NextFrameAsync();
            }
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// 스케일 아웃 (정상 → 작음)
        /// </summary>
        protected async Awaitable ScaleOut(float duration)
        {
            transform.localScale = Vector3.one;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
                await Awaitable.NextFrameAsync();
            }
            transform.localScale = Vector3.zero;
        }

        /// <summary>
        /// 아래에서 슬라이드 인
        /// </summary>
        protected async Awaitable SlideFromBottom(float duration)
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            Vector2 startPos = new Vector2(0, -Screen.height);
            Vector2 endPos = Vector2.zero;
            rectTransform.anchoredPosition = startPos;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                await Awaitable.NextFrameAsync();
            }
            rectTransform.anchoredPosition = endPos;
        }

        /// <summary>
        /// 아래로 슬라이드 아웃
        /// </summary>
        protected async Awaitable SlideToBottom(float duration)
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = new Vector2(0, -Screen.height);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                await Awaitable.NextFrameAsync();
            }
            rectTransform.anchoredPosition = endPos;
        }

        /// <summary>
        /// 왼쪽에서 슬라이드 인
        /// </summary>
        protected async Awaitable SlideFromLeft(float duration)
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            Vector2 startPos = new Vector2(-Screen.width, 0);
            Vector2 endPos = Vector2.zero;
            rectTransform.anchoredPosition = startPos;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                await Awaitable.NextFrameAsync();
            }
            rectTransform.anchoredPosition = endPos;
        }

        /// <summary>
        /// 왼쪽으로 슬라이드 아웃
        /// </summary>
        protected async Awaitable SlideToLeft(float duration)
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = new Vector2(-Screen.width, 0);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                await Awaitable.NextFrameAsync();
            }
            rectTransform.anchoredPosition = endPos;
        }

        /// <summary>
        /// 오른쪽에서 슬라이드 인
        /// </summary>
        protected async Awaitable SlideFromRight(float duration)
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            Vector2 startPos = new Vector2(Screen.width, 0);
            Vector2 endPos = Vector2.zero;
            rectTransform.anchoredPosition = startPos;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                await Awaitable.NextFrameAsync();
            }
            rectTransform.anchoredPosition = endPos;
        }

        /// <summary>
        /// 오른쪽으로 슬라이드 아웃
        /// </summary>
        protected async Awaitable SlideToRight(float duration)
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = new Vector2(Screen.width, 0);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                await Awaitable.NextFrameAsync();
            }
            rectTransform.anchoredPosition = endPos;
        }
    }
}
