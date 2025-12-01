using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Forms;

namespace GASPT.UI.Forms
{
    /// <summary>
    /// 단일 폼 슬롯 UI
    /// 아이콘, 이름, 각성 레벨을 표시
    /// </summary>
    public class FormSlotUI : MonoBehaviour
    {
        // ====== UI 요소 ======

        [Header("기본 요소")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image borderImage;

        [Header("텍스트")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI awakeningText;

        [Header("각성 표시")]
        [SerializeField] private GameObject[] awakeningStars;
        [SerializeField] private Image awakeningGauge;

        [Header("상태 표시")]
        [SerializeField] private GameObject activeIndicator;
        [SerializeField] private GameObject emptySlotIndicator;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("설정")]
        [SerializeField] private bool isActiveSlot = true;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);


        // ====== 상태 ======

        private FormInstance currentForm;
        private bool isEmpty = true;


        // ====== 프로퍼티 ======

        /// <summary>현재 표시 중인 폼</summary>
        public FormInstance CurrentForm => currentForm;

        /// <summary>빈 슬롯 여부</summary>
        public bool IsEmpty => isEmpty;

        /// <summary>활성 슬롯 여부</summary>
        public bool IsActiveSlot
        {
            get => isActiveSlot;
            set
            {
                isActiveSlot = value;
                UpdateActiveState();
            }
        }


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            SetEmpty();
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 폼 데이터로 슬롯 업데이트
        /// </summary>
        public void SetForm(FormInstance form)
        {
            currentForm = form;

            if (form == null)
            {
                SetEmpty();
                return;
            }

            isEmpty = false;

            // 아이콘 설정
            if (iconImage != null)
            {
                if (form.FormSprite != null)
                {
                    iconImage.sprite = form.FormSprite;
                    iconImage.enabled = true;
                }
                else if (form.Data?.icon != null)
                {
                    iconImage.sprite = form.Data.icon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }

                iconImage.color = form.FormColor;
            }

            // 이름 설정
            if (nameText != null)
            {
                nameText.text = form.FormName;
                nameText.color = GetRarityColor(form.CurrentRarity);
            }

            // 각성 레벨 설정
            UpdateAwakeningDisplay(form.AwakeningLevel);

            // 배경색
            if (backgroundImage != null)
            {
                backgroundImage.color = GetRarityBackgroundColor(form.CurrentRarity);
            }

            // 빈 슬롯 표시 숨김
            if (emptySlotIndicator != null)
            {
                emptySlotIndicator.SetActive(false);
            }

            UpdateActiveState();
        }

        /// <summary>
        /// 빈 슬롯으로 설정
        /// </summary>
        public void SetEmpty()
        {
            currentForm = null;
            isEmpty = true;

            if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            if (nameText != null)
            {
                nameText.text = "빈 슬롯";
                nameText.color = emptyColor;
            }

            if (awakeningText != null)
            {
                awakeningText.text = "";
            }

            // 각성 별 숨김
            if (awakeningStars != null)
            {
                foreach (var star in awakeningStars)
                {
                    if (star != null) star.SetActive(false);
                }
            }

            if (awakeningGauge != null)
            {
                awakeningGauge.fillAmount = 0f;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = emptyColor;
            }

            if (emptySlotIndicator != null)
            {
                emptySlotIndicator.SetActive(true);
            }

            UpdateActiveState();
        }

        /// <summary>
        /// 각성 레벨만 업데이트
        /// </summary>
        public void UpdateAwakening(int level)
        {
            UpdateAwakeningDisplay(level);
        }


        // ====== 내부 메서드 ======

        private void UpdateAwakeningDisplay(int level)
        {
            // 텍스트 표시
            if (awakeningText != null)
            {
                if (level > 0)
                {
                    awakeningText.text = $"+{level}";
                    awakeningText.color = GetAwakeningColor(level);
                }
                else
                {
                    awakeningText.text = "";
                }
            }

            // 별 표시
            if (awakeningStars != null)
            {
                for (int i = 0; i < awakeningStars.Length; i++)
                {
                    if (awakeningStars[i] != null)
                    {
                        awakeningStars[i].SetActive(i < level);
                    }
                }
            }

            // 게이지 표시
            if (awakeningGauge != null)
            {
                int maxLevel = currentForm?.Data?.maxAwakeningLevel ?? 3;
                awakeningGauge.fillAmount = maxLevel > 0 ? (float)level / maxLevel : 0f;
            }
        }

        private void UpdateActiveState()
        {
            Color targetColor = isEmpty ? emptyColor : (isActiveSlot ? activeColor : inactiveColor);

            if (borderImage != null)
            {
                borderImage.color = isActiveSlot ? activeColor : inactiveColor;
            }

            if (activeIndicator != null)
            {
                activeIndicator.SetActive(isActiveSlot && !isEmpty);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = isActiveSlot ? 1f : 0.7f;
            }
        }

        private Color GetRarityColor(FormRarity rarity)
        {
            return rarity switch
            {
                FormRarity.Common => Color.white,
                FormRarity.Rare => new Color(0.3f, 0.5f, 1f),
                FormRarity.Unique => new Color(0.7f, 0.3f, 0.9f),
                FormRarity.Legendary => new Color(1f, 0.8f, 0.2f),
                _ => Color.white
            };
        }

        private Color GetRarityBackgroundColor(FormRarity rarity)
        {
            return rarity switch
            {
                FormRarity.Common => new Color(0.2f, 0.2f, 0.2f, 0.8f),
                FormRarity.Rare => new Color(0.1f, 0.15f, 0.3f, 0.8f),
                FormRarity.Unique => new Color(0.2f, 0.1f, 0.3f, 0.8f),
                FormRarity.Legendary => new Color(0.3f, 0.25f, 0.1f, 0.8f),
                _ => new Color(0.2f, 0.2f, 0.2f, 0.8f)
            };
        }

        private Color GetAwakeningColor(int level)
        {
            return level switch
            {
                1 => new Color(0.5f, 1f, 0.5f),
                2 => new Color(0.3f, 0.8f, 1f),
                >= 3 => new Color(1f, 0.8f, 0.2f),
                _ => Color.white
            };
        }
    }
}
