using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using GASPT.Data;

namespace GASPT.UI
{
    /// <summary>
    /// 개별 아이템 획득 슬롯 UI
    /// </summary>
    public class ItemPickupSlot : MonoBehaviour
    {
        // ====== UI 참조 ======

        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private CanvasGroup canvasGroup;


        // ====== 내부 상태 ======

        private CancellationTokenSource animationCts;
        private System.Action onComplete;


        // ====== 초기화 ======

        private void OnDestroy()
        {
            StopAnimation();
        }


        // ====== Public 메서드 ======

        /// <summary>
        /// 슬롯 표시
        /// </summary>
        public void Show(Item item, float displayDuration, float fadeDuration, System.Action onCompleteCallback)
        {
            if (item == null)
                return;

            onComplete = onCompleteCallback;

            // UI 설정
            if (iconImage != null && item.icon != null)
            {
                iconImage.sprite = item.icon;
            }

            if (nameText != null)
            {
                nameText.text = $"{item.itemName} 획득!";
            }

            // 애니메이션 시작
            StopAnimation();
            gameObject.SetActive(true);

            animationCts = new CancellationTokenSource();
            PlayAnimationAsync(displayDuration, fadeDuration, animationCts.Token);
        }


        // ====== 애니메이션 ======

        private async void PlayAnimationAsync(float displayDuration, float fadeDuration, CancellationToken ct)
        {
            try
            {
                // 페이드 인
                await FadeAsync(0f, 1f, fadeDuration, ct);

                // 표시 시간 대기
                await Awaitable.WaitForSecondsAsync(displayDuration, ct);

                // 페이드 아웃
                await FadeAsync(1f, 0f, fadeDuration, ct);

                // 완료 콜백 호출
                onComplete?.Invoke();
            }
            catch (System.OperationCanceledException)
            {
                // 취소는 정상 동작
            }
        }

        private async Awaitable FadeAsync(float startAlpha, float endAlpha, float duration, CancellationToken ct)
        {
            if (canvasGroup == null)
                return;

            float elapsedTime = 0f;

            while (elapsedTime < duration && !ct.IsCancellationRequested)
            {
                await Awaitable.NextFrameAsync(ct);

                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            }

            // 최종 값 설정
            if (!ct.IsCancellationRequested)
            {
                canvasGroup.alpha = endAlpha;
            }
        }

        private void StopAnimation()
        {
            if (animationCts != null)
            {
                animationCts.Cancel();
                animationCts.Dispose();
                animationCts = null;
            }
        }
    }
}
