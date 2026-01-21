using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace MVP_Core
{
    /// <summary>
    /// 범용 UI 애니메이션 유틸리티
    /// 색상 플래시, 스케일 애니메이션, 페이드 인/아웃 등
    /// </summary>
    public static class UIAnimationHelper
    {
        #region 색상 애니메이션

        /// <summary>
        /// Image 색상 플래시 후 원래 색상으로 복귀
        /// </summary>
        public static async Awaitable FlashColorAsync(
            Image image,
            Color flashColor,
            Color normalColor,
            float duration,
            CancellationToken ct = default)
        {
            if (image == null) return;

            float elapsed = 0f;
            image.color = flashColor;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                image.color = Color.Lerp(flashColor, normalColor, t);

                await Awaitable.NextFrameAsync(ct);
            }

            image.color = normalColor;
        }

        /// <summary>
        /// Graphic (Image, Text 등) 색상 애니메이션
        /// </summary>
        public static async Awaitable AnimateColorAsync(
            Graphic graphic,
            Color targetColor,
            float duration,
            CancellationToken ct = default)
        {
            if (graphic == null) return;

            Color startColor = graphic.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                graphic.color = Color.Lerp(startColor, targetColor, t);

                await Awaitable.NextFrameAsync(ct);
            }

            graphic.color = targetColor;
        }

        #endregion

        #region 스케일 애니메이션

        /// <summary>
        /// Transform 스케일 애니메이션
        /// </summary>
        public static async Awaitable ScaleAsync(
            Transform transform,
            Vector3 targetScale,
            float duration,
            CancellationToken ct = default)
        {
            if (transform == null) return;

            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);

                await Awaitable.NextFrameAsync(ct);
            }

            transform.localScale = targetScale;
        }

        /// <summary>
        /// 펄스 스케일 애니메이션 (커졌다 작아짐)
        /// </summary>
        public static async Awaitable PulseScaleAsync(
            Transform transform,
            Vector3 maxScale,
            float duration,
            CancellationToken ct = default)
        {
            if (transform == null) return;

            Vector3 originalScale = transform.localScale;

            await ScaleAsync(transform, maxScale, duration * 0.5f, ct);
            if (ct.IsCancellationRequested) return;

            await ScaleAsync(transform, originalScale, duration * 0.5f, ct);
        }

        /// <summary>
        /// 바운스 스케일 애니메이션 (탄성 효과)
        /// </summary>
        public static async Awaitable BounceScaleAsync(
            Transform transform,
            Vector3 targetScale,
            float duration,
            float overshoot = 1.2f,
            CancellationToken ct = default)
        {
            if (transform == null) return;

            Vector3 startScale = transform.localScale;
            Vector3 overshootScale = Vector3.Lerp(startScale, targetScale, overshoot);

            // 오버슈트까지
            await ScaleAsync(transform, overshootScale, duration * 0.6f, ct);
            if (ct.IsCancellationRequested) return;

            // 타겟으로 복귀
            await ScaleAsync(transform, targetScale, duration * 0.4f, ct);
        }

        #endregion

        #region 페이드 애니메이션

        /// <summary>
        /// CanvasGroup 알파 애니메이션
        /// </summary>
        public static async Awaitable FadeAsync(
            CanvasGroup canvasGroup,
            float targetAlpha,
            float duration,
            CancellationToken ct = default)
        {
            if (canvasGroup == null) return;

            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

                await Awaitable.NextFrameAsync(ct);
            }

            canvasGroup.alpha = targetAlpha;
        }

        /// <summary>
        /// 페이드 인 (0 → 1)
        /// </summary>
        public static async Awaitable FadeInAsync(
            CanvasGroup canvasGroup,
            float duration,
            CancellationToken ct = default)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            await FadeAsync(canvasGroup, 1f, duration, ct);
        }

        /// <summary>
        /// 페이드 아웃 (1 → 0)
        /// </summary>
        public static async Awaitable FadeOutAsync(
            CanvasGroup canvasGroup,
            float duration,
            CancellationToken ct = default)
        {
            await FadeAsync(canvasGroup, 0f, duration, ct);
        }

        #endregion

        #region 슬라이더 애니메이션

        /// <summary>
        /// Slider 값 부드럽게 변경
        /// </summary>
        public static async Awaitable AnimateSliderAsync(
            Slider slider,
            float targetValue,
            float duration,
            CancellationToken ct = default)
        {
            if (slider == null) return;

            float startValue = slider.value;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                slider.value = Mathf.Lerp(startValue, targetValue, t);

                await Awaitable.NextFrameAsync(ct);
            }

            slider.value = targetValue;
        }

        #endregion

        #region 이동 애니메이션

        /// <summary>
        /// RectTransform 앵커 위치 애니메이션
        /// </summary>
        public static async Awaitable MoveAnchoredPositionAsync(
            RectTransform rectTransform,
            Vector2 targetPosition,
            float duration,
            CancellationToken ct = default)
        {
            if (rectTransform == null) return;

            Vector2 startPosition = rectTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

                await Awaitable.NextFrameAsync(ct);
            }

            rectTransform.anchoredPosition = targetPosition;
        }

        #endregion

        #region 복합 애니메이션

        /// <summary>
        /// 색상 플래시 + 스케일 펄스 동시 실행
        /// </summary>
        public static async Awaitable FlashAndPulseAsync(
            Image image,
            Transform transform,
            Color flashColor,
            Color normalColor,
            Vector3 maxScale,
            float duration,
            CancellationToken ct = default)
        {
            var flashTask = FlashColorAsync(image, flashColor, normalColor, duration, ct);
            var pulseTask = PulseScaleAsync(transform, maxScale, duration, ct);

            await flashTask;
            await pulseTask;
        }

        /// <summary>
        /// 페이드 인 + 스케일 업 동시 실행
        /// </summary>
        public static async Awaitable FadeInWithScaleAsync(
            CanvasGroup canvasGroup,
            Transform transform,
            Vector3 startScale,
            Vector3 targetScale,
            float duration,
            CancellationToken ct = default)
        {
            if (transform != null) transform.localScale = startScale;
            if (canvasGroup != null) canvasGroup.alpha = 0f;

            var fadeTask = FadeInAsync(canvasGroup, duration, ct);
            var scaleTask = ScaleAsync(transform, targetScale, duration, ct);

            await fadeTask;
            await scaleTask;
        }

        /// <summary>
        /// 페이드 아웃 + 스케일 다운 동시 실행
        /// </summary>
        public static async Awaitable FadeOutWithScaleAsync(
            CanvasGroup canvasGroup,
            Transform transform,
            Vector3 targetScale,
            float duration,
            CancellationToken ct = default)
        {
            var fadeTask = FadeOutAsync(canvasGroup, duration, ct);
            var scaleTask = ScaleAsync(transform, targetScale, duration, ct);

            await fadeTask;
            await scaleTask;
        }

        #endregion
    }
}
