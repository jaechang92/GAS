using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using GASPT.Core;

namespace GASPT.UI
{
    /// <summary>
    /// 화면 Fade In/Out 효과 컨트롤러
    /// - 씬 전환 시 부드러운 전환 효과 제공
    /// - DontDestroyOnLoad로 씬 전환 시에도 유지
    /// - SingletonManager 상속으로 중복 생성 방지
    /// </summary>
    public class FadeController : SingletonManager<FadeController>
    {
        private Canvas fadeCanvas;
        private Image fadeImage;
        private bool isFading = false;

        protected override void Awake()
        {
            base.Awake();
            CreateFadeUI();
        }

        /// <summary>
        /// Fade UI 생성
        /// </summary>
        private void CreateFadeUI()
        {
            // Canvas 생성
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);

            fadeCanvas = canvasObj.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 9999; // 최상위 레이어

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // Fade Image 생성
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvasObj.transform, false);

            fadeImage = imageObj.AddComponent<Image>();
            fadeImage.color = new Color(0, 0, 0, 0); // 투명한 검은색
            fadeImage.raycastTarget = false; // 클릭 이벤트 차단 안 함

            RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // 초기 상태: 비활성화
            fadeCanvas.gameObject.SetActive(false);

            Debug.Log("[FadeController] Fade UI 생성 완료");
        }

        /// <summary>
        /// Fade In (검은 화면 → 투명)
        /// </summary>
        /// <param name="duration">페이드 시간 (초)</param>
        public async Awaitable FadeIn(float duration = 1f)
        {
            if (isFading)
            {
                Debug.LogWarning("[FadeController] 이미 Fade 진행 중!");
                return;
            }

            isFading = true;
            fadeCanvas.gameObject.SetActive(true);

            float elapsed = 0f;
            Color color = fadeImage.color;

            // 검은색 (Alpha 1) → 투명 (Alpha 0)
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                color.a = alpha;
                fadeImage.color = color;

                await Awaitable.NextFrameAsync(destroyCancellationToken);
            }

            // 완전히 투명하게
            color.a = 0f;
            fadeImage.color = color;

            fadeCanvas.gameObject.SetActive(false);
            isFading = false;

            Debug.Log("[FadeController] Fade In 완료");
        }

        /// <summary>
        /// Fade Out (투명 → 검은 화면)
        /// </summary>
        /// <param name="duration">페이드 시간 (초)</param>
        public async Awaitable FadeOut(float duration = 1f)
        {
            if (isFading)
            {
                Debug.LogWarning("[FadeController] 이미 Fade 진행 중!");
                return;
            }

            isFading = true;
            fadeCanvas.gameObject.SetActive(true);

            float elapsed = 0f;
            Color color = fadeImage.color;

            // 투명 (Alpha 0) → 검은색 (Alpha 1)
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                color.a = alpha;
                fadeImage.color = color;

                await Awaitable.NextFrameAsync(destroyCancellationToken);
            }

            // 완전히 불투명하게
            color.a = 1f;
            fadeImage.color = color;

            isFading = false;

            Debug.Log("[FadeController] Fade Out 완료");
        }

        /// <summary>
        /// 즉시 검은 화면으로 설정
        /// </summary>
        public void SetBlack()
        {
            fadeCanvas.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
            isFading = false;
        }

        /// <summary>
        /// 즉시 투명하게 설정
        /// </summary>
        public void SetTransparent()
        {
            fadeCanvas.gameObject.SetActive(false);
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            isFading = false;
        }

        /// <summary>
        /// Fade 진행 중 여부
        /// </summary>
        public bool IsFading => isFading;
    }
}
