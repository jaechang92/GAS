using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GASPT.UI.Forms
{
    /// <summary>
    /// 폼 교체 쿨다운 UI
    /// 쿨다운 게이지와 남은 시간을 표시
    /// </summary>
    public class FormCooldownUI : MonoBehaviour
    {
        // ====== UI 요소 ======

        [Header("게이지")]
        [SerializeField] private Image cooldownFill;
        [SerializeField] private Image cooldownBackground;

        [Header("텍스트")]
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private TextMeshProUGUI swapKeyText;

        [Header("상태 표시")]
        [SerializeField] private GameObject readyIndicator;
        [SerializeField] private GameObject cooldownIndicator;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("설정")]
        [SerializeField] private Color readyColor = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color cooldownColor = new Color(1f, 0.5f, 0.2f);
        [SerializeField] private Color notAvailableColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private bool showDecimalTime = true;
        [SerializeField] private float readyPulseSpeed = 2f;

        [Header("애니메이션")]
        [SerializeField] private bool enablePulseAnimation = true;
        [SerializeField] private float pulseMinScale = 0.95f;
        [SerializeField] private float pulseMaxScale = 1.05f;


        // ====== 상태 ======

        private bool isOnCooldown;
        private bool canSwap;
        private float currentCooldown;
        private float maxCooldown;
        private RectTransform rectTransform;


        // ====== 프로퍼티 ======

        /// <summary>쿨다운 중인지 여부</summary>
        public bool IsOnCooldown => isOnCooldown;

        /// <summary>교체 가능 여부</summary>
        public bool CanSwap => canSwap;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            SetReady();
        }

        private void Update()
        {
            if (enablePulseAnimation && canSwap && !isOnCooldown)
            {
                AnimateReadyPulse();
            }
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 쿨다운 시작
        /// </summary>
        public void StartCooldown(float duration)
        {
            isOnCooldown = true;
            maxCooldown = duration;
            currentCooldown = duration;
            canSwap = false;

            UpdateCooldownDisplay();

            if (readyIndicator != null)
                readyIndicator.SetActive(false);

            if (cooldownIndicator != null)
                cooldownIndicator.SetActive(true);
        }

        /// <summary>
        /// 쿨다운 진행 업데이트
        /// </summary>
        public void UpdateCooldown(float remaining, float progress)
        {
            currentCooldown = remaining;

            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = progress;
                cooldownFill.color = Color.Lerp(cooldownColor, readyColor, progress);
            }

            UpdateCooldownText(remaining);

            // 쿨다운 완료 체크
            if (remaining <= 0f && isOnCooldown)
            {
                SetReady();
            }
        }

        /// <summary>
        /// 준비 완료 상태로 설정
        /// </summary>
        public void SetReady()
        {
            isOnCooldown = false;
            currentCooldown = 0f;
            canSwap = true;

            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = 1f;
                cooldownFill.color = readyColor;
            }

            if (cooldownText != null)
            {
                cooldownText.text = "Q";
                cooldownText.color = readyColor;
            }

            if (readyIndicator != null)
                readyIndicator.SetActive(true);

            if (cooldownIndicator != null)
                cooldownIndicator.SetActive(false);

            // 스케일 초기화
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 교체 불가 상태로 설정 (대기 폼 없음)
        /// </summary>
        public void SetNotAvailable()
        {
            isOnCooldown = false;
            canSwap = false;

            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = 1f;
                cooldownFill.color = notAvailableColor;
            }

            if (cooldownText != null)
            {
                cooldownText.text = "-";
                cooldownText.color = notAvailableColor;
            }

            if (readyIndicator != null)
                readyIndicator.SetActive(false);

            if (cooldownIndicator != null)
                cooldownIndicator.SetActive(false);
        }

        /// <summary>
        /// 교체 키 텍스트 설정
        /// </summary>
        public void SetSwapKeyText(string keyText)
        {
            if (swapKeyText != null)
            {
                swapKeyText.text = keyText;
            }
        }


        // ====== 내부 메서드 ======

        private void UpdateCooldownDisplay()
        {
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = 0f;
                cooldownFill.color = cooldownColor;
            }

            UpdateCooldownText(currentCooldown);
        }

        private void UpdateCooldownText(float remaining)
        {
            if (cooldownText == null) return;

            if (remaining > 0f)
            {
                if (showDecimalTime)
                {
                    cooldownText.text = remaining.ToString("F1");
                }
                else
                {
                    cooldownText.text = Mathf.CeilToInt(remaining).ToString();
                }
                cooldownText.color = cooldownColor;
            }
        }

        private void AnimateReadyPulse()
        {
            if (rectTransform == null) return;

            float pulse = Mathf.Sin(Time.time * readyPulseSpeed) * 0.5f + 0.5f;
            float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, pulse);
            rectTransform.localScale = Vector3.one * scale;
        }
    }
}
