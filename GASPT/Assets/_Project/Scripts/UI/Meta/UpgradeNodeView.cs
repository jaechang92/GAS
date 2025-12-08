using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Meta;

namespace GASPT.UI.Meta
{
    /// <summary>
    /// 업그레이드 노드 UI
    /// 업그레이드 트리의 각 노드를 표시
    /// </summary>
    public class UpgradeNodeView : MonoBehaviour
    {
        // ====== 이벤트 ======

        /// <summary>노드 선택 이벤트</summary>
        public event Action<UpgradeNodeView> OnSelected;

        /// <summary>구매 버튼 클릭 이벤트</summary>
        public event Action<UpgradeNodeView> OnPurchaseClicked;


        // ====== UI 요소 ======

        [Header("기본 UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("상태 표시")]
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private GameObject maxLevelMark;
        [SerializeField] private Image progressBar;

        [Header("버튼")]
        [SerializeField] private Button selectButton;

        [Header("색상 설정")]
        [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color availableColor = new Color(0.2f, 0.6f, 0.2f);
        [SerializeField] private Color maxedColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color selectedBorderColor = Color.white;
        [SerializeField] private Color normalBorderColor = new Color(0.5f, 0.5f, 0.5f);


        // ====== 상태 ======

        private PermanentUpgrade upgrade;
        private int currentLevel;
        private bool isLocked;
        private bool isSelected;


        // ====== 프로퍼티 ======

        public PermanentUpgrade Upgrade => upgrade;
        public int CurrentLevel => currentLevel;
        public bool IsLocked => isLocked;
        public bool IsMaxed => upgrade != null && currentLevel >= upgrade.maxLevel;
        public bool IsSelected => isSelected;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (selectButton == null)
            {
                selectButton = GetComponent<Button>();
                if (selectButton == null)
                {
                    selectButton = gameObject.AddComponent<Button>();
                }
            }

            selectButton.onClick.AddListener(() => OnSelected?.Invoke(this));
        }

        private void OnDestroy()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
            }
        }


        // ====== 설정 ======

        /// <summary>
        /// 노드 설정
        /// </summary>
        public void Setup(PermanentUpgrade upgrade, int currentLevel, bool isLocked)
        {
            this.upgrade = upgrade;
            this.currentLevel = currentLevel;
            this.isLocked = isLocked;

            UpdateVisuals();
        }

        /// <summary>
        /// 레벨 업데이트
        /// </summary>
        public void UpdateLevel(int newLevel)
        {
            currentLevel = newLevel;
            UpdateVisuals();
        }

        /// <summary>
        /// 잠금 상태 업데이트
        /// </summary>
        public void SetLocked(bool locked)
        {
            isLocked = locked;
            UpdateVisuals();
        }

        /// <summary>
        /// 선택 상태 설정
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;

            if (borderImage != null)
            {
                borderImage.color = selected ? selectedBorderColor : normalBorderColor;
            }
        }


        // ====== 시각 업데이트 ======

        private void UpdateVisuals()
        {
            if (upgrade == null) return;

            // 아이콘
            if (iconImage != null)
            {
                iconImage.sprite = upgrade.icon;
                iconImage.color = isLocked ? Color.gray : Color.white;
            }

            // 이름
            if (nameText != null)
            {
                nameText.text = upgrade.upgradeName;
                nameText.color = isLocked ? Color.gray : Color.white;
            }

            // 레벨
            if (levelText != null)
            {
                if (IsMaxed)
                {
                    levelText.text = "MAX";
                    levelText.color = maxedColor;
                }
                else
                {
                    levelText.text = $"Lv.{currentLevel}/{upgrade.maxLevel}";
                    levelText.color = Color.white;
                }
            }

            // 배경색
            if (backgroundImage != null)
            {
                if (isLocked)
                    backgroundImage.color = lockedColor;
                else if (IsMaxed)
                    backgroundImage.color = maxedColor;
                else
                    backgroundImage.color = availableColor;
            }

            // 잠금 오버레이
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(isLocked);
            }

            // 최대 레벨 마크
            if (maxLevelMark != null)
            {
                maxLevelMark.SetActive(IsMaxed);
            }

            // 진행도 바
            if (progressBar != null && upgrade.maxLevel > 0)
            {
                progressBar.fillAmount = (float)currentLevel / upgrade.maxLevel;
            }

            // 버튼 상호작용
            if (selectButton != null)
            {
                selectButton.interactable = !isLocked;
            }
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 다음 레벨 비용 가져오기
        /// </summary>
        public int GetNextLevelCost()
        {
            if (upgrade == null || IsMaxed) return -1;
            return upgrade.GetUpgradeCost(currentLevel);
        }

        /// <summary>
        /// 현재 효과 설명 가져오기
        /// </summary>
        public string GetCurrentEffectDescription()
        {
            if (upgrade == null) return "";
            return upgrade.GetEffectDescription(currentLevel);
        }

        /// <summary>
        /// 다음 레벨 효과 설명 가져오기
        /// </summary>
        public string GetNextEffectDescription()
        {
            if (upgrade == null || IsMaxed) return "";
            return upgrade.GetEffectDescription(currentLevel + 1);
        }
    }
}
