using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Components
{
    /// <summary>
    /// 스킬 슬롯 UI 템플릿
    /// 쿨다운 타이머와 스킬 아이콘을 표시
    /// </summary>
    public class SkillSlotUI : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Image skillIcon;              // 스킬 아이콘
        [SerializeField] private Image cooldownOverlay;        // 쿨다운 오버레이 (어두운 층)
        [SerializeField] private TextMeshProUGUI cooldownText; // 쿨다운 텍스트
        [SerializeField] private TextMeshProUGUI keyText;      // 키 바인딩 텍스트 (Q, E 등)

        [Header("쿨다운 설정")]
        [SerializeField] private bool showCooldownText = true;
        [SerializeField] private Color cooldownColor = new Color(0, 0, 0, 0.7f);

        private float cooldownDuration = 0f;
        private float remainingCooldown = 0f;
        private bool isOnCooldown = false;

        private void Awake()
        {
            // 초기화
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.color = cooldownColor;
            }

            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (isOnCooldown)
            {
                UpdateCooldown();
            }
        }

        /// <summary>
        /// 스킬 아이콘 설정
        /// </summary>
        public void SetSkillIcon(Sprite icon)
        {
            if (skillIcon != null)
            {
                skillIcon.sprite = icon;
            }
        }

        /// <summary>
        /// 키 바인딩 텍스트 설정
        /// </summary>
        public void SetKeyText(string key)
        {
            if (keyText != null)
            {
                keyText.text = key;
            }
        }

        /// <summary>
        /// 쿨다운 시작
        /// </summary>
        public void StartCooldown(float duration)
        {
            cooldownDuration = duration;
            remainingCooldown = duration;
            isOnCooldown = true;

            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 1f;
            }

            if (cooldownText != null && showCooldownText)
            {
                cooldownText.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 쿨다운 업데이트
        /// </summary>
        private void UpdateCooldown()
        {
            remainingCooldown -= Time.deltaTime;

            if (remainingCooldown <= 0f)
            {
                // 쿨다운 종료
                EndCooldown();
                return;
            }

            // UI 업데이트
            UpdateCooldownUI();
        }

        /// <summary>
        /// 쿨다운 UI 업데이트
        /// </summary>
        private void UpdateCooldownUI()
        {
            // Fill Amount 업데이트 (1 → 0)
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = remainingCooldown / cooldownDuration;
            }

            // 텍스트 업데이트 (1초 단위)
            if (cooldownText != null && showCooldownText)
            {
                cooldownText.text = Mathf.Ceil(remainingCooldown).ToString("F0");
            }
        }

        /// <summary>
        /// 쿨다운 종료
        /// </summary>
        private void EndCooldown()
        {
            isOnCooldown = false;
            remainingCooldown = 0f;

            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 0f;
            }

            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 쿨다운 중인지 확인
        /// </summary>
        public bool IsOnCooldown => isOnCooldown;

        /// <summary>
        /// 남은 쿨다운 시간
        /// </summary>
        public float RemainingCooldown => remainingCooldown;

        /// <summary>
        /// 쿨다운 즉시 종료
        /// </summary>
        public void ResetCooldown()
        {
            EndCooldown();
        }
    }
}
