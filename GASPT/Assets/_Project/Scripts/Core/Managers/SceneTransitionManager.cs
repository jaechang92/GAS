using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Managers
{
    /// <summary>
    /// 씬 전환 효과 (페이드 인/아웃) 관리
    /// SingletonManager 패턴으로 DontDestroyOnLoad 자동 설정
    /// </summary>
    public class SceneTransitionManager : SingletonManager<SceneTransitionManager>
    {

        [Header("페이드 설정")]
        [SerializeField] private float defaultFadeDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;

        [Header("디버그 설정")]
        [SerializeField] private bool showDebugLog = true;

        // UI 요소
        private Canvas fadeCanvas;
        private Image fadeImage;

        // 상태
        private bool isFading = false;

        // 이벤트
        public event Action OnFadeOutStarted;
        public event Action OnFadeOutCompleted;
        public event Action OnFadeInStarted;
        public event Action OnFadeInCompleted;

        protected override void OnSingletonAwake()
        {
            InitializeFadeUI();
            Log("[SceneTransition] 초기화 완료");
        }

        /// <summary>
        /// 페이드 UI 초기화
        /// </summary>
        private void InitializeFadeUI()
        {
            // Canvas 생성
            GameObject canvasGO = new GameObject("FadeCanvas");
            canvasGO.transform.SetParent(transform);

            fadeCanvas = canvasGO.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 9999; // 최상위에 렌더링

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Fade Image 생성
            GameObject imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(canvasGO.transform, false);

            fadeImage = imageGO.AddComponent<Image>();
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f); // 초기 투명

            // RectTransform 설정 (전체 화면)
            RectTransform rect = imageGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            // 초기에는 비활성화
            fadeCanvas.gameObject.SetActive(false);
        }

        /// <summary>
        /// 페이드 아웃 (화면 어두워짐)
        /// </summary>
        public async Awaitable FadeOutAsync(float duration = -1f)
        {
            if (duration < 0f)
            {
                duration = defaultFadeDuration;
            }

            if (isFading)
            {
                LogWarning("[SceneTransition] 이미 페이드 진행 중");
                return;
            }

            isFading = true;
            Log($"[SceneTransition] 페이드 아웃 시작 ({duration:F2}초)");

            OnFadeOutStarted?.Invoke();

            // Canvas 활성화
            fadeCanvas.gameObject.SetActive(true);

            // 페이드 아웃 애니메이션 (0 → 1)
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime; // Time.timeScale 영향 받지 않음
                float alpha = Mathf.Clamp01(elapsed / duration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                await Awaitable.NextFrameAsync();
            }

            // 완전히 불투명
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

            OnFadeOutCompleted?.Invoke();
            Log("[SceneTransition] 페이드 아웃 완료");

            isFading = false;
        }

        /// <summary>
        /// 페이드 인 (화면 밝아짐)
        /// </summary>
        public async Awaitable FadeInAsync(float duration = -1f)
        {
            if (duration < 0f)
            {
                duration = defaultFadeDuration;
            }

            if (isFading)
            {
                LogWarning("[SceneTransition] 이미 페이드 진행 중");
                return;
            }

            isFading = true;
            Log($"[SceneTransition] 페이드 인 시작 ({duration:F2}초)");

            OnFadeInStarted?.Invoke();

            // 페이드 인 애니메이션 (1 → 0)
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime; // Time.timeScale 영향 받지 않음
                float alpha = 1f - Mathf.Clamp01(elapsed / duration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                await Awaitable.NextFrameAsync();
            }

            // 완전히 투명
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

            // Canvas 비활성화
            fadeCanvas.gameObject.SetActive(false);

            OnFadeInCompleted?.Invoke();
            Log("[SceneTransition] 페이드 인 완료");

            isFading = false;
        }

        /// <summary>
        /// 즉시 페이드 아웃 (애니메이션 없음)
        /// </summary>
        public void SetFadeOut()
        {
            fadeCanvas.gameObject.SetActive(true);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            Log("[SceneTransition] 즉시 페이드 아웃");
        }

        /// <summary>
        /// 즉시 페이드 인 (애니메이션 없음)
        /// </summary>
        public void SetFadeIn()
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeCanvas.gameObject.SetActive(false);
            Log("[SceneTransition] 즉시 페이드 인");
        }

        /// <summary>
        /// 현재 페이드 진행 중인지 확인
        /// </summary>
        public bool IsFading => isFading;

        #region 로깅

        private void Log(string message)
        {
            if (showDebugLog)
            {
                Debug.Log(message);
            }
        }

        private void LogWarning(string message)
        {
            if (showDebugLog)
            {
                Debug.LogWarning(message);
            }
        }

        #endregion

        #region Context Menu (에디터 테스트용)

#if UNITY_EDITOR
        [ContextMenu("페이드 아웃 테스트")]
        private void TestFadeOut()
        {
            _ = FadeOutAsync();
        }

        [ContextMenu("페이드 인 테스트")]
        private void TestFadeIn()
        {
            _ = FadeInAsync();
        }

        [ContextMenu("즉시 페이드 아웃")]
        private void TestSetFadeOut()
        {
            SetFadeOut();
        }

        [ContextMenu("즉시 페이드 인")]
        private void TestSetFadeIn()
        {
            SetFadeIn();
        }
#endif

        #endregion
    }
}
