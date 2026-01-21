using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MVP_Core
{
    /// <summary>
    /// UI 컴포넌트 확장 메서드
    /// </summary>
    public static class UIExtensions
    {
        #region Image 확장

        /// <summary>
        /// 스프라이트 설정 (null 체크 포함)
        /// </summary>
        public static void SetSpriteSafe(this Image image, Sprite sprite)
        {
            if (image == null) return;
            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        /// <summary>
        /// 알파값만 변경
        /// </summary>
        public static void SetAlpha(this Graphic graphic, float alpha)
        {
            if (graphic == null) return;
            var color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        /// <summary>
        /// 비동기 색상 플래시
        /// </summary>
        public static async Awaitable FlashColorAsync(
            this Image image,
            Color flashColor,
            Color normalColor,
            float duration,
            CancellationToken ct = default)
        {
            await UIAnimationHelper.FlashColorAsync(image, flashColor, normalColor, duration, ct);
        }

        #endregion

        #region TextMeshPro 확장

        /// <summary>
        /// 텍스트 설정 (null 체크 포함)
        /// </summary>
        public static void SetTextSafe(this TextMeshProUGUI text, string value)
        {
            if (text == null) return;
            text.text = value ?? "";
        }

        /// <summary>
        /// 텍스트와 색상 동시 설정
        /// </summary>
        public static void SetTextWithColor(this TextMeshProUGUI text, string value, Color color)
        {
            if (text == null) return;
            text.text = value ?? "";
            text.color = color;
        }

        /// <summary>
        /// 숫자 포맷팅 (천 단위 구분자)
        /// </summary>
        public static void SetNumber(this TextMeshProUGUI text, int value, string format = "N0")
        {
            if (text == null) return;
            text.text = value.ToString(format);
        }

        /// <summary>
        /// 퍼센트 표시
        /// </summary>
        public static void SetPercent(this TextMeshProUGUI text, float ratio, string format = "P0")
        {
            if (text == null) return;
            text.text = ratio.ToString(format);
        }

        #endregion

        #region Slider 확장

        /// <summary>
        /// 슬라이더 값 설정 (0~1 범위 클램프)
        /// </summary>
        public static void SetRatio(this Slider slider, float ratio)
        {
            if (slider == null) return;
            slider.value = Mathf.Clamp01(ratio);
        }

        /// <summary>
        /// 현재값/최대값으로 슬라이더 설정
        /// </summary>
        public static void SetValue(this Slider slider, float current, float max)
        {
            if (slider == null || max <= 0) return;
            slider.value = Mathf.Clamp01(current / max);
        }

        /// <summary>
        /// 비동기 슬라이더 애니메이션
        /// </summary>
        public static async Awaitable AnimateToAsync(
            this Slider slider,
            float targetValue,
            float duration,
            CancellationToken ct = default)
        {
            await UIAnimationHelper.AnimateSliderAsync(slider, targetValue, duration, ct);
        }

        #endregion

        #region CanvasGroup 확장

        /// <summary>
        /// 상호작용 활성화/비활성화
        /// </summary>
        public static void SetInteractable(this CanvasGroup canvasGroup, bool interactable)
        {
            if (canvasGroup == null) return;
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }

        /// <summary>
        /// 완전히 표시/숨김
        /// </summary>
        public static void SetVisible(this CanvasGroup canvasGroup, bool visible)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        /// <summary>
        /// 비동기 페이드 인
        /// </summary>
        public static async Awaitable FadeInAsync(
            this CanvasGroup canvasGroup,
            float duration,
            CancellationToken ct = default)
        {
            await UIAnimationHelper.FadeInAsync(canvasGroup, duration, ct);
            canvasGroup?.SetInteractable(true);
        }

        /// <summary>
        /// 비동기 페이드 아웃
        /// </summary>
        public static async Awaitable FadeOutAsync(
            this CanvasGroup canvasGroup,
            float duration,
            CancellationToken ct = default)
        {
            canvasGroup?.SetInteractable(false);
            await UIAnimationHelper.FadeOutAsync(canvasGroup, duration, ct);
        }

        #endregion

        #region Transform 확장

        /// <summary>
        /// 비동기 스케일 애니메이션
        /// </summary>
        public static async Awaitable ScaleToAsync(
            this Transform transform,
            Vector3 targetScale,
            float duration,
            CancellationToken ct = default)
        {
            await UIAnimationHelper.ScaleAsync(transform, targetScale, duration, ct);
        }

        /// <summary>
        /// 비동기 펄스 애니메이션
        /// </summary>
        public static async Awaitable PulseAsync(
            this Transform transform,
            float maxScaleMultiplier,
            float duration,
            CancellationToken ct = default)
        {
            Vector3 maxScale = transform.localScale * maxScaleMultiplier;
            await UIAnimationHelper.PulseScaleAsync(transform, maxScale, duration, ct);
        }

        /// <summary>
        /// 비동기 바운스 애니메이션
        /// </summary>
        public static async Awaitable BounceToAsync(
            this Transform transform,
            Vector3 targetScale,
            float duration,
            float overshoot = 1.2f,
            CancellationToken ct = default)
        {
            await UIAnimationHelper.BounceScaleAsync(transform, targetScale, duration, overshoot, ct);
        }

        #endregion

        #region RectTransform 확장

        /// <summary>
        /// 비동기 위치 이동
        /// </summary>
        public static async Awaitable MoveToAsync(
            this RectTransform rectTransform,
            Vector2 targetPosition,
            float duration,
            CancellationToken ct = default)
        {
            await UIAnimationHelper.MoveAnchoredPositionAsync(rectTransform, targetPosition, duration, ct);
        }

        /// <summary>
        /// 화면 경계 내로 위치 보정
        /// </summary>
        public static void ClampToScreen(this RectTransform rectTransform, Canvas canvas, Vector2 padding = default)
        {
            if (rectTransform == null || canvas == null) return;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect == null) return;

            Vector2 pos = rectTransform.anchoredPosition;
            Vector2 size = rectTransform.sizeDelta;
            Vector2 canvasSize = canvasRect.sizeDelta;

            // 우측 경계
            if (pos.x + size.x / 2 > canvasSize.x / 2 - padding.x)
                pos.x = canvasSize.x / 2 - size.x / 2 - padding.x;

            // 좌측 경계
            if (pos.x - size.x / 2 < -canvasSize.x / 2 + padding.x)
                pos.x = -canvasSize.x / 2 + size.x / 2 + padding.x;

            // 상단 경계
            if (pos.y + size.y / 2 > canvasSize.y / 2 - padding.y)
                pos.y = canvasSize.y / 2 - size.y / 2 - padding.y;

            // 하단 경계
            if (pos.y - size.y / 2 < -canvasSize.y / 2 + padding.y)
                pos.y = -canvasSize.y / 2 + size.y / 2 + padding.y;

            rectTransform.anchoredPosition = pos;
        }

        #endregion

        #region GameObject 확장

        /// <summary>
        /// 안전한 SetActive (null 체크)
        /// </summary>
        public static void SetActiveSafe(this GameObject gameObject, bool active)
        {
            if (gameObject != null && gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// 컴포넌트 안전하게 가져오기 또는 추가
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null) return null;

            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        #endregion
    }
}
