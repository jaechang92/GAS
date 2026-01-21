using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace MVP_Core.Examples
{
    /// <summary>
    /// 슬롯 View 구현 예제
    /// 인벤토리, 퀵슬롯, 장비 슬롯 등에 사용
    /// </summary>
    public class ExampleSlotView : ViewBase, ISlotView,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        // ====== UI 참조 ======

        [Header("Slot UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private TextMeshProUGUI hotkeyText;

        [Header("Selection")]
        [SerializeField] private GameObject selectionIndicator;
        [SerializeField] private Color normalBorderColor = Color.gray;
        [SerializeField] private Color selectedBorderColor = Color.yellow;

        // ====== 이벤트 ======

        public event Action<int> OnSlotClicked;
        public event Action<int> OnSlotHoverEnter;
        public event Action<int> OnSlotHoverExit;

        // ====== 상태 ======

        [SerializeField] private int slotIndex;
        private bool isSelected;
        private SlotViewModel currentViewModel;

        // ====== ISlotView 구현 ======

        public int SlotIndex => slotIndex;
        public bool IsSelected => isSelected;

        public void Select()
        {
            if (isSelected) return;
            isSelected = true;

            if (selectionIndicator != null)
                selectionIndicator.SetActive(true);

            if (borderImage != null)
                borderImage.color = selectedBorderColor;
        }

        public void Deselect()
        {
            if (!isSelected) return;
            isSelected = false;

            if (selectionIndicator != null)
                selectionIndicator.SetActive(false);

            if (borderImage != null)
                borderImage.color = currentViewModel?.BorderColor ?? normalBorderColor;
        }

        public void Clear()
        {
            currentViewModel = null;

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (quantityText != null)
                quantityText.text = "";

            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.enabled = false;
            }

            if (borderImage != null)
                borderImage.color = normalBorderColor;
        }

        // ====== 데이터 바인딩 ======

        /// <summary>
        /// 슬롯 인덱스 설정 (초기화 시)
        /// </summary>
        public void SetSlotIndex(int index)
        {
            slotIndex = index;
        }

        /// <summary>
        /// ViewModel로 슬롯 업데이트
        /// </summary>
        public void UpdateSlot(SlotViewModel viewModel)
        {
            currentViewModel = viewModel;

            if (viewModel == null || viewModel.IsEmpty)
            {
                Clear();
                return;
            }

            // 아이콘
            if (iconImage != null)
            {
                iconImage.sprite = viewModel.Icon;
                iconImage.enabled = viewModel.Icon != null;
            }

            // 수량
            if (quantityText != null)
            {
                quantityText.text = viewModel.Quantity > 1 ? viewModel.Quantity.ToString() : "";
            }

            // 테두리 색상 (등급)
            if (borderImage != null && !isSelected)
            {
                borderImage.color = viewModel.BorderColor;
            }

            // 쿨다운 오버레이
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = viewModel.CooldownRatio;
                cooldownOverlay.enabled = viewModel.CooldownRatio > 0f;
            }
        }

        /// <summary>
        /// 퀵슬롯 ViewModel로 업데이트
        /// </summary>
        public void UpdateSlot(QuickSlotViewModel viewModel)
        {
            UpdateSlot((SlotViewModel)viewModel);

            // 단축키 표시
            if (hotkeyText != null && viewModel != null)
            {
                hotkeyText.text = viewModel.Hotkey;
            }
        }

        /// <summary>
        /// 쿨다운만 업데이트
        /// </summary>
        public void UpdateCooldown(float ratio)
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = Mathf.Clamp01(ratio);
                cooldownOverlay.enabled = ratio > 0f;
            }
        }

        // ====== 이벤트 핸들러 ======

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(slotIndex);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnSlotHoverEnter?.Invoke(slotIndex);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnSlotHoverExit?.Invoke(slotIndex);
        }

        // ====== Context Menu (테스트용) ======

        [ContextMenu("Test Fill Slot")]
        private void TestFillSlot()
        {
            var vm = new SlotViewModel(
                slotIndex,
                "test_item",
                null,
                "Test Item",
                5,
                99,
                Color.blue,
                true,
                false,
                false,
                0f);

            UpdateSlot(vm);
        }

        [ContextMenu("Test Clear Slot")]
        private void TestClearSlot()
        {
            Clear();
        }
    }
}
