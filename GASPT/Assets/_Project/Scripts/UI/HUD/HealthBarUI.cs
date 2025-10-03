using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    /// <summary>
    /// 체력바 UI 컴포넌트
    /// 체력 게이지 표시 및 애니메이션 처리
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Text healthText;

        [Header("애니메이션 설정")]
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private bool useSmooth = true;

        [Header("색상 설정")]
        [SerializeField] private Color fullHealthColor = new Color(0.6f, 0.2f, 0.8f); // 보라색
        [SerializeField] private Color lowHealthColor = Color.red;
        [SerializeField] private float lowHealthThreshold = 0.3f;

        private float currentFillAmount;
        private float targetFillAmount;
        private float currentHealth;
        private float maxHealth;

        private void Update()
        {
            if (useSmooth && fillImage != null)
            {
                // 부드러운 애니메이션
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
                fillImage.fillAmount = currentFillAmount;

                // 체력에 따른 색상 변경
                UpdateHealthColor();
            }
        }

        /// <summary>
        /// 체력바 업데이트
        /// </summary>
        public void UpdateHealth(float current, float max)
        {
            currentHealth = current;
            maxHealth = max;

            targetFillAmount = Mathf.Clamp01(current / max);

            if (!useSmooth && fillImage != null)
            {
                fillImage.fillAmount = targetFillAmount;
                currentFillAmount = targetFillAmount;
            }

            // 텍스트 업데이트
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
            }

            UpdateHealthColor();
        }

        /// <summary>
        /// 체력 퍼센트 반환
        /// </summary>
        public float GetHealthPercent()
        {
            return targetFillAmount;
        }

        /// <summary>
        /// 체력에 따른 색상 업데이트
        /// </summary>
        private void UpdateHealthColor()
        {
            if (fillImage == null) return;

            float percent = currentFillAmount;

            if (percent <= lowHealthThreshold)
            {
                // 낮은 체력: 빨간색
                fillImage.color = lowHealthColor;
            }
            else
            {
                // 정상 체력: 보라색
                fillImage.color = fullHealthColor;
            }
        }

        /// <summary>
        /// 즉시 체력바 설정 (애니메이션 없이)
        /// </summary>
        public void SetHealthImmediate(float current, float max)
        {
            currentHealth = current;
            maxHealth = max;
            targetFillAmount = Mathf.Clamp01(current / max);
            currentFillAmount = targetFillAmount;

            if (fillImage != null)
            {
                fillImage.fillAmount = targetFillAmount;
            }

            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
            }

            UpdateHealthColor();
        }

        /// <summary>
        /// 체력바 초기화
        /// </summary>
        public void Initialize(float current, float max)
        {
            SetHealthImmediate(current, max);
        }
    }
}
