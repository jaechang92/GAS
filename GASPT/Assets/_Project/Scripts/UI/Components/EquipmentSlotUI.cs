using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using GASPT.Core.Enums;
using GASPT.UI.MVP.ViewModels;

namespace GASPT.UI.Components
{
    /// <summary>
    /// 장비 슬롯 UI 컴포넌트
    /// 캐릭터 장비창에 사용
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        // ====== 슬롯 타입 ======

        [Header("슬롯 설정")]
        [SerializeField] private EquipmentSlot slotType = EquipmentSlot.Weapon;


        // ====== UI 참조 ======

        [Header("UI 요소")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image slotTypeIcon;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private Image durabilityBar;
        [SerializeField] private TextMeshProUGUI slotLabel;


        // ====== 설정 ======

        [Header("설정")]
        [SerializeField] private Sprite emptySlotIcon;
        [SerializeField] private Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color filledColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color highlightColor = new Color(0.5f, 0.5f, 0.2f, 1f);


        // ====== 상태 ======

        private bool isEmpty = true;
        private bool isHighlighted = false;
        private EquipmentSlotViewModel currentViewModel;


        // ====== 이벤트 ======

        /// <summary>
        /// 슬롯 클릭 이벤트
        /// 매개변수: (슬롯 타입)
        /// </summary>
        public event System.Action<EquipmentSlot> OnSlotClicked;

        /// <summary>
        /// 슬롯 호버 진입 이벤트
        /// </summary>
        public event System.Action<EquipmentSlot, EquipmentSlotViewModel> OnSlotHoverEnter;

        /// <summary>
        /// 슬롯 호버 이탈 이벤트
        /// </summary>
        public event System.Action OnSlotHoverExit;


        // ====== 프로퍼티 ======

        public EquipmentSlot SlotType => slotType;
        public bool IsEmpty => isEmpty;
        public EquipmentSlotViewModel ViewModel => currentViewModel;


        // ====== 초기화 ======

        private void Awake()
        {
            UpdateSlotLabel();
            Clear();
        }

        private void UpdateSlotLabel()
        {
            if (slotLabel != null)
            {
                slotLabel.text = GetSlotDisplayName(slotType);
            }
        }

        private string GetSlotDisplayName(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Weapon => "무기",
                EquipmentSlot.Armor => "갑옷",
                EquipmentSlot.Helmet => "투구",
                EquipmentSlot.Gloves => "장갑",
                EquipmentSlot.Boots => "부츠",
                EquipmentSlot.Accessory1 => "장신구1",
                EquipmentSlot.Accessory2 => "장신구2",
                _ => ""
            };
        }


        // ====== 업데이트 ======

        /// <summary>
        /// ViewModel로 UI 업데이트
        /// </summary>
        public void UpdateUI(EquipmentSlotViewModel viewModel)
        {
            currentViewModel = viewModel;

            if (viewModel == null || viewModel.IsEmpty)
            {
                Clear();
                return;
            }

            isEmpty = false;

            // 아이콘
            if (iconImage != null)
            {
                iconImage.sprite = viewModel.Icon;
                iconImage.enabled = viewModel.Icon != null;
            }

            // 배경
            if (backgroundImage != null)
            {
                backgroundImage.color = isHighlighted ? highlightColor : filledColor;
            }

            // 등급 테두리
            if (rarityBorder != null)
            {
                rarityBorder.color = viewModel.RarityColor;
                rarityBorder.enabled = true;
            }

            // 내구도 바
            if (durabilityBar != null)
            {
                durabilityBar.fillAmount = viewModel.DurabilityRatio;
                durabilityBar.enabled = viewModel.DurabilityRatio < 1f;

                // 내구도에 따른 색상
                if (viewModel.DurabilityRatio < 0.3f)
                {
                    durabilityBar.color = Color.red;
                }
                else if (viewModel.DurabilityRatio < 0.6f)
                {
                    durabilityBar.color = Color.yellow;
                }
                else
                {
                    durabilityBar.color = Color.green;
                }
            }
        }

        /// <summary>
        /// 슬롯 비우기
        /// </summary>
        public void Clear()
        {
            isEmpty = true;
            currentViewModel = null;

            if (iconImage != null)
            {
                iconImage.sprite = emptySlotIcon;
                iconImage.enabled = emptySlotIcon != null;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = emptyColor;
            }

            if (rarityBorder != null)
            {
                rarityBorder.enabled = false;
            }

            if (durabilityBar != null)
            {
                durabilityBar.enabled = false;
            }
        }


        // ====== 하이라이트 ======

        /// <summary>
        /// 하이라이트 설정 (드래그 시 호환 슬롯 표시용)
        /// </summary>
        public void SetHighlight(bool highlight)
        {
            isHighlighted = highlight;

            if (backgroundImage != null)
            {
                if (!isEmpty)
                {
                    backgroundImage.color = highlight ? highlightColor : filledColor;
                }
                else
                {
                    backgroundImage.color = highlight ? highlightColor : emptyColor;
                }
            }
        }


        // ====== 이벤트 핸들러 ======

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isEmpty && currentViewModel != null)
            {
                OnSlotHoverEnter?.Invoke(slotType, currentViewModel);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnSlotHoverExit?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(slotType);
        }
    }
}
