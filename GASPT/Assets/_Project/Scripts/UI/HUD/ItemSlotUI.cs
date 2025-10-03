using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    /// <summary>
    /// 아이템/스킬 슬롯 UI 컴포넌트
    /// 아이콘 표시, 쿨다운, 개수 표시 등
    /// </summary>
    public class ItemSlotUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private Text countText;

        [Header("슬롯 설정")]
        [SerializeField] private bool showCount = true;
        [SerializeField] private Color emptySlotColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color filledSlotColor = Color.white;

        private Sprite currentIcon;
        private int currentCount;
        private float cooldownTimer;
        private float cooldownDuration;
        private bool isOnCooldown;

        private void Awake()
        {
            // 초기화
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.gameObject.SetActive(false);
            }

            if (countText != null)
            {
                countText.gameObject.SetActive(false);
            }

            ClearSlot();
        }

        private void Update()
        {
            // 쿨다운 업데이트
            if (isOnCooldown && cooldownDuration > 0)
            {
                cooldownTimer -= Time.deltaTime;

                if (cooldownTimer <= 0)
                {
                    cooldownTimer = 0;
                    isOnCooldown = false;
                    if (cooldownOverlay != null)
                    {
                        cooldownOverlay.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // 쿨다운 진행도 표시 (fillAmount는 남은 시간 비율)
                    if (cooldownOverlay != null)
                    {
                        cooldownOverlay.fillAmount = cooldownTimer / cooldownDuration;
                    }
                }
            }
        }

        /// <summary>
        /// 슬롯에 아이템/스킬 설정
        /// </summary>
        public void SetItem(Sprite icon, int count = 1)
        {
            currentIcon = icon;
            currentCount = count;

            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.color = icon != null ? filledSlotColor : emptySlotColor;
            }

            UpdateCountDisplay();
        }

        /// <summary>
        /// 슬롯 비우기
        /// </summary>
        public void ClearSlot()
        {
            currentIcon = null;
            currentCount = 0;

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.color = emptySlotColor;
            }

            if (countText != null)
            {
                countText.gameObject.SetActive(false);
            }

            StopCooldown();
        }

        /// <summary>
        /// 아이템 개수 업데이트
        /// </summary>
        public void SetCount(int count)
        {
            currentCount = count;
            UpdateCountDisplay();
        }

        /// <summary>
        /// 쿨다운 시작
        /// </summary>
        public void StartCooldown(float duration)
        {
            cooldownDuration = duration;
            cooldownTimer = duration;
            isOnCooldown = true;

            if (cooldownOverlay != null)
            {
                cooldownOverlay.gameObject.SetActive(true);
                cooldownOverlay.fillAmount = 1f;
            }
        }

        /// <summary>
        /// 쿨다운 중지
        /// </summary>
        public void StopCooldown()
        {
            isOnCooldown = false;
            cooldownTimer = 0;

            if (cooldownOverlay != null)
            {
                cooldownOverlay.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 쿨다운 중인지 확인
        /// </summary>
        public bool IsOnCooldown()
        {
            return isOnCooldown;
        }

        /// <summary>
        /// 남은 쿨다운 시간 반환
        /// </summary>
        public float GetRemainingCooldown()
        {
            return cooldownTimer;
        }

        /// <summary>
        /// 슬롯이 비어있는지 확인
        /// </summary>
        public bool IsEmpty()
        {
            return currentIcon == null;
        }

        /// <summary>
        /// 개수 표시 업데이트
        /// </summary>
        private void UpdateCountDisplay()
        {
            if (!showCount || countText == null)
                return;

            if (currentIcon != null && currentCount > 1)
            {
                countText.text = currentCount.ToString();
                countText.gameObject.SetActive(true);
            }
            else
            {
                countText.gameObject.SetActive(false);
            }
        }
    }
}
