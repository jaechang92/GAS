using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using GASPT.UI.MVP.ViewModels;

namespace GASPT.UI.Components
{
    /// <summary>
    /// 아이템 슬롯 UI 컴포넌트
    /// 인벤토리/퀵슬롯에 사용
    /// </summary>
    public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        // ====== UI 참조 ======

        [Header("UI 요소")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private Image equippedMark;


        // ====== 설정 ======

        [Header("설정")]
        [SerializeField] private Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color filledSlotColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.5f, 0.5f, 0.1f, 1f);


        // ====== 상태 ======

        private int slotIndex = -1;
        private bool isEmpty = true;
        private bool isSelected = false;
        private InventorySlotViewModel currentViewModel;


        // ====== 이벤트 ======

        /// <summary>
        /// 슬롯 클릭 이벤트
        /// 매개변수: (슬롯 인덱스)
        /// </summary>
        public event System.Action<int> OnSlotClicked;

        /// <summary>
        /// 슬롯 호버 진입 이벤트
        /// 매개변수: (슬롯 인덱스, ViewModel)
        /// </summary>
        public event System.Action<int, InventorySlotViewModel> OnSlotHoverEnter;

        /// <summary>
        /// 슬롯 호버 이탈 이벤트
        /// </summary>
        public event System.Action OnSlotHoverExit;


        // ====== 프로퍼티 ======

        public int SlotIndex => slotIndex;
        public bool IsEmpty => isEmpty;
        public bool IsSelected => isSelected;
        public InventorySlotViewModel ViewModel => currentViewModel;


        // ====== 초기화 ======

        /// <summary>
        /// 슬롯 초기화
        /// </summary>
        /// <param name="index">슬롯 인덱스</param>
        public void Initialize(int index)
        {
            slotIndex = index;
            Clear();
        }


        // ====== 업데이트 ======

        /// <summary>
        /// ViewModel로 UI 업데이트
        /// </summary>
        /// <param name="viewModel">슬롯 뷰모델</param>
        public void UpdateUI(InventorySlotViewModel viewModel)
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
                backgroundImage.color = isSelected ? selectedColor : filledSlotColor;
            }

            // 등급 테두리
            if (rarityBorder != null)
            {
                rarityBorder.color = viewModel.RarityColor;
                rarityBorder.enabled = true;
            }

            // 수량
            if (quantityText != null)
            {
                quantityText.text = viewModel.QuantityText;
                quantityText.enabled = viewModel.Quantity > 1;
            }

            // 장착 표시
            if (equippedMark != null)
            {
                equippedMark.enabled = viewModel.IsEquipped;
            }

            // 쿨다운 오버레이
            UpdateCooldown(viewModel.IsOnCooldown, viewModel.RemainingCooldown);
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
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = emptySlotColor;
            }

            if (rarityBorder != null)
            {
                rarityBorder.enabled = false;
            }

            if (quantityText != null)
            {
                quantityText.text = "";
                quantityText.enabled = false;
            }

            if (equippedMark != null)
            {
                equippedMark.enabled = false;
            }

            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.enabled = false;
            }
        }


        // ====== 선택 상태 ======

        /// <summary>
        /// 선택 상태 설정
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;

            if (backgroundImage != null)
            {
                if (!isEmpty)
                {
                    backgroundImage.color = selected ? selectedColor : filledSlotColor;
                }
            }
        }


        // ====== 쿨다운 ======

        /// <summary>
        /// 쿨다운 UI 업데이트
        /// </summary>
        /// <param name="isOnCooldown">쿨다운 중 여부</param>
        /// <param name="remainingTime">남은 시간</param>
        /// <param name="totalTime">총 쿨다운 시간 (선택)</param>
        public void UpdateCooldown(bool isOnCooldown, float remainingTime, float totalTime = 0f)
        {
            if (cooldownOverlay == null)
                return;

            if (!isOnCooldown || remainingTime <= 0f)
            {
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.enabled = false;
                return;
            }

            cooldownOverlay.enabled = true;

            if (totalTime > 0f)
            {
                cooldownOverlay.fillAmount = remainingTime / totalTime;
            }
            else
            {
                cooldownOverlay.fillAmount = 1f;
            }
        }


        // ====== 이벤트 핸들러 ======

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isEmpty && currentViewModel != null)
            {
                OnSlotHoverEnter?.Invoke(slotIndex, currentViewModel);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnSlotHoverExit?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(slotIndex);
        }
    }
}
