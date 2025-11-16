using UnityEngine;
using UnityEngine.UI;
using System.Threading;

namespace GASPT.UI
{
    /// <summary>
    /// UI 애니메이션을 위한 공통 유틸리티 클래스
    ///
    /// 주요 기능:
    /// - Image 색상 플래시 애니메이션
    /// - Text 스케일 애니메이션
    /// - 페이드 인/아웃 애니메이션
    ///
    /// 작성일: 2025-11-16
    /// 목적: Player Bar 스크립트들의 중복 애니메이션 코드 제거 (75줄 절감)
    /// </summary>
    public static class UIAnimationHelper
    {
        #region 색상 애니메이션

        /// <summary>
        /// Image 색상을 플래시하고 점진적으로 원래 색상으로 복귀합니다.
        /// </summary>
        /// <param name="image">애니메이션 대상 Image</param>
        /// <param name="flashColor">플래시 색상</param>
        /// <param name="normalColor">복귀할 원래 색상</param>
        /// <param name="duration">애니메이션 지속 시간 (초)</param>
        /// <param name="ct">취소 토큰</param>
        public static async Awaitable FlashColorAsync(
            Image image,
            Color flashColor,
            Color normalColor,
            float duration,
            CancellationToken ct)
        {
            if (image == null)
            {
                Debug.LogWarning("[UIAnimationHelper] FlashColorAsync: Image is null");
                return;
            }

            float elapsed = 0f;

            // 플래시 색상으로 변경
            image.color = flashColor;

            // 점진적으로 원래 색상으로 복귀
            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                image.color = Color.Lerp(flashColor, normalColor, t);

                await Awaitable.NextFrameAsync(ct);
            }

            // 최종 색상 확정
            image.color = normalColor;
        }

        #endregion

        #region 스케일 애니메이션

        /// <summary>
        /// RectTransform의 스케일을 애니메이션합니다.
        /// </summary>
        /// <param name="rectTransform">애니메이션 대상 RectTransform</param>
        /// <param name="targetScale">목표 스케일</param>
        /// <param name="duration">애니메이션 지속 시간 (초)</param>
        /// <param name="ct">취소 토큰</param>
        public static async Awaitable ScaleAsync(
            RectTransform rectTransform,
            Vector3 targetScale,
            float duration,
            CancellationToken ct)
        {
            if (rectTransform == null)
            {
                Debug.LogWarning("[UIAnimationHelper] ScaleAsync: RectTransform is null");
                return;
            }

            Vector3 startScale = rectTransform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

                await Awaitable.NextFrameAsync(ct);
            }

            // 최종 스케일 확정
            rectTransform.localScale = targetScale;
        }

        /// <summary>
        /// RectTransform의 스케일을 펄스 애니메이션합니다 (커졌다 작아짐).
        /// </summary>
        /// <param name="rectTransform">애니메이션 대상 RectTransform</param>
        /// <param name="maxScale">최대 스케일</param>
        /// <param name="duration">애니메이션 지속 시간 (초)</param>
        /// <param name="ct">취소 토큰</param>
        public static async Awaitable PulseScaleAsync(
            RectTransform rectTransform,
            Vector3 maxScale,
            float duration,
            CancellationToken ct)
        {
            if (rectTransform == null)
            {
                Debug.LogWarning("[UIAnimationHelper] PulseScaleAsync: RectTransform is null");
                return;
            }

            Vector3 originalScale = rectTransform.localScale;

            // 확대
            await ScaleAsync(rectTransform, maxScale, duration * 0.5f, ct);

            if (ct.IsCancellationRequested) return;

            // 축소
            await ScaleAsync(rectTransform, originalScale, duration * 0.5f, ct);
        }

        #endregion

        #region 페이드 애니메이션

        /// <summary>
        /// CanvasGroup의 알파값을 애니메이션합니다.
        /// </summary>
        /// <param name="canvasGroup">애니메이션 대상 CanvasGroup</param>
        /// <param name="targetAlpha">목표 알파값 (0-1)</param>
        /// <param name="duration">애니메이션 지속 시간 (초)</param>
        /// <param name="ct">취소 토큰</param>
        public static async Awaitable FadeAsync(
            CanvasGroup canvasGroup,
            float targetAlpha,
            float duration,
            CancellationToken ct)
        {
            if (canvasGroup == null)
            {
                Debug.LogWarning("[UIAnimationHelper] FadeAsync: CanvasGroup is null");
                return;
            }

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

            // 최종 알파값 확정
            canvasGroup.alpha = targetAlpha;
        }

        /// <summary>
        /// CanvasGroup을 페이드 인합니다 (알파 0 → 1).
        /// </summary>
        public static async Awaitable FadeInAsync(
            CanvasGroup canvasGroup,
            float duration,
            CancellationToken ct)
        {
            await FadeAsync(canvasGroup, 1f, duration, ct);
        }

        /// <summary>
        /// CanvasGroup을 페이드 아웃합니다 (알파 1 → 0).
        /// </summary>
        public static async Awaitable FadeOutAsync(
            CanvasGroup canvasGroup,
            float duration,
            CancellationToken ct)
        {
            await FadeAsync(canvasGroup, 0f, duration, ct);
        }

        #endregion

        #region 복합 애니메이션

        /// <summary>
        /// Image 색상 플래시 + RectTransform 스케일 펄스 동시 실행
        /// </summary>
        public static async Awaitable FlashAndPulseAsync(
            Image image,
            RectTransform rectTransform,
            Color flashColor,
            Color normalColor,
            Vector3 maxScale,
            float duration,
            CancellationToken ct)
        {
            // 두 애니메이션을 동시에 시작
            var flashTask = FlashColorAsync(image, flashColor, normalColor, duration, ct);
            var pulseTask = PulseScaleAsync(rectTransform, maxScale, duration, ct);

            // Unity Awaitable은 WhenAll이 없으므로 순차적으로 await
            // 이미 두 작업이 시작되었으므로 병렬로 실행됨
            await flashTask;
            await pulseTask;
        }

        #endregion
    }
}
