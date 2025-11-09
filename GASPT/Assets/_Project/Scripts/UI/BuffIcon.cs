using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Enums;
using GASPT.StatusEffects;
using System.Threading;

namespace GASPT.UI
{
    /// <summary>
    /// 단일 버프/디버프 아이콘 UI
    /// 아이콘, 타이머, 스택 수 표시
    /// </summary>
    public class BuffIcon : MonoBehaviour
    {
        // ====== UI 참조 ======

        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image timerFillImage;
        [SerializeField] private TextMeshProUGUI stackText;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Colors")]
        [SerializeField] private Color buffBorderColor = new Color(0f, 1f, 0.3f, 1f); // 초록
        [SerializeField] private Color debuffBorderColor = new Color(1f, 0.2f, 0.2f, 1f); // 빨강

        [Header("Optional Border")]
        [SerializeField] private Image borderImage;


        // ====== 내부 상태 ======

        private StatusEffect currentEffect;
        private CancellationTokenSource updateCts;
        private bool isActive = false;


        // ====== 초기화 ======

        private void OnDestroy()
        {
            StopUpdating();
        }


        // ====== Public 메서드 ======

        /// <summary>
        /// 버프 아이콘 활성화
        /// </summary>
        public void Show(StatusEffect effect, Sprite icon, bool isBuff)
        {
            currentEffect = effect;
            isActive = true;

            // 아이콘 설정
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
            }

            // 테두리 색상 설정
            if (borderImage != null)
            {
                borderImage.color = isBuff ? buffBorderColor : debuffBorderColor;
                borderImage.enabled = true;
            }

            // UI 요소 활성화
            if (timerFillImage != null)
                timerFillImage.enabled = true;

            gameObject.SetActive(true);

            // 업데이트 시작
            StartUpdating();
        }

        /// <summary>
        /// 버프 아이콘 비활성화
        /// </summary>
        public void Hide()
        {
            isActive = false;
            currentEffect = null;

            StopUpdating();

            gameObject.SetActive(false);
        }

        /// <summary>
        /// 스택 수 업데이트
        /// </summary>
        public void UpdateStack(int stack)
        {
            if (stackText != null)
            {
                if (stack > 1)
                {
                    stackText.text = stack.ToString();
                    stackText.enabled = true;
                }
                else
                {
                    stackText.enabled = false;
                }
            }
        }


        // ====== Private 메서드 ======

        private void StartUpdating()
        {
            StopUpdating();

            updateCts = new CancellationTokenSource();
            UpdateTimerAsync(updateCts.Token).ContinueWith(() => { });
        }

        private void StopUpdating()
        {
            if (updateCts != null)
            {
                updateCts.Cancel();
                updateCts.Dispose();
                updateCts = null;
            }
        }

        /// <summary>
        /// 타이머 업데이트 (Awaitable 기반)
        /// </summary>
        private async Awaitable UpdateTimerAsync(CancellationToken ct)
        {
            try
            {
                while (isActive && currentEffect != null && !ct.IsCancellationRequested)
                {
                    await Awaitable.NextFrameAsync(ct);

                    if (currentEffect == null || !isActive)
                        break;

                    // 진행도 계산 (0.0 ~ 1.0)
                    float ratio = currentEffect.RemainingTime / currentEffect.Duration;

                    // 타이머 Fill 업데이트
                    if (timerFillImage != null)
                    {
                        timerFillImage.fillAmount = ratio;
                    }

                    // 남은 시간 텍스트 업데이트
                    if (timeText != null)
                    {
                        float remaining = currentEffect.RemainingTime;
                        if (remaining >= 10f)
                        {
                            timeText.text = Mathf.CeilToInt(remaining).ToString();
                        }
                        else
                        {
                            timeText.text = remaining.ToString("F1") + "s";
                        }
                    }
                }
            }
            catch (System.OperationCanceledException)
            {
                // 취소는 정상 동작
            }
        }


        // ====== Getters ======

        public StatusEffectType? EffectType => currentEffect?.EffectType;
        public bool IsActive => isActive;
    }
}
