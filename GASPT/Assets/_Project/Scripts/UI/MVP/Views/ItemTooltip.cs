using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.UI.MVP.ViewModels;

namespace GASPT.UI.MVP.Views
{
    /// <summary>
    /// 아이템 툴팁 UI
    /// </summary>
    public class ItemTooltip : MonoBehaviour
    {
        // ====== UI 참조 ======

        [Header("기본 정보")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Image rarityBorder;

        [Header("장비 정보")]
        [SerializeField] private GameObject equipmentSection;
        [SerializeField] private TextMeshProUGUI slotTypeText;
        [SerializeField] private TextMeshProUGUI requiredLevelText;
        [SerializeField] private TextMeshProUGUI durabilityText;

        [Header("스탯 정보")]
        [SerializeField] private GameObject statsSection;
        [SerializeField] private Transform baseStatsContainer;
        [SerializeField] private Transform randomStatsContainer;
        [SerializeField] private GameObject statEntryPrefab;

        [Header("소비 정보")]
        [SerializeField] private GameObject consumableSection;
        [SerializeField] private TextMeshProUGUI effectText;
        [SerializeField] private TextMeshProUGUI cooldownText;

        [Header("설명")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("경제 정보")]
        [SerializeField] private TextMeshProUGUI sellPriceText;

        [Header("패널")]
        [SerializeField] private RectTransform tooltipPanel;
        [SerializeField] private CanvasGroup canvasGroup;


        // ====== 설정 ======

        [Header("설정")]
        [SerializeField] private Vector2 offset = new Vector2(20f, -20f);
        [SerializeField] private float showDelay = 0.2f;


        // ====== 상태 ======

        private bool isShowing = false;
        private float showTimer = 0f;
        private ItemViewModelV2 currentItem;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (isShowing && tooltipPanel != null)
            {
                UpdatePosition();
            }
        }


        // ====== 표시/숨기기 ======

        /// <summary>
        /// 툴팁 표시
        /// </summary>
        public void Show(ItemViewModelV2 viewModel)
        {
            if (viewModel == null)
            {
                Hide();
                return;
            }

            currentItem = viewModel;
            UpdateContent(viewModel);

            isShowing = true;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            if (tooltipPanel != null)
            {
                tooltipPanel.gameObject.SetActive(true);
            }

            UpdatePosition();
        }

        /// <summary>
        /// InventorySlotViewModel로 표시
        /// </summary>
        public void Show(InventorySlotViewModel slotViewModel)
        {
            if (slotViewModel == null || slotViewModel.IsEmpty)
            {
                Hide();
                return;
            }

            // InventorySlot에서 ItemInstance 정보 추출하여 간략 표시
            ItemViewModelV2 itemVm = new ItemViewModelV2
            {
                InstanceId = slotViewModel.InstanceId,
                ItemName = slotViewModel.ItemName,
                Description = slotViewModel.Description,
                Icon = slotViewModel.Icon,
                Category = slotViewModel.Category,
                Rarity = slotViewModel.Rarity,
                RarityColor = slotViewModel.RarityColor,
                IsEquipment = slotViewModel.IsEquipment,
                IsConsumable = slotViewModel.IsConsumable,
                EquipSlot = slotViewModel.EquipSlot
            };

            Show(itemVm);
        }

        /// <summary>
        /// 툴팁 숨기기
        /// </summary>
        public void Hide()
        {
            isShowing = false;
            currentItem = null;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (tooltipPanel != null)
            {
                tooltipPanel.gameObject.SetActive(false);
            }
        }


        // ====== 내용 업데이트 ======

        private void UpdateContent(ItemViewModelV2 vm)
        {
            // 기본 정보
            if (itemNameText != null)
            {
                itemNameText.text = vm.ItemName;
                itemNameText.color = vm.RarityColor;
            }

            if (rarityText != null)
            {
                rarityText.text = vm.RarityName;
                rarityText.color = vm.RarityColor;
            }

            if (itemIcon != null)
            {
                itemIcon.sprite = vm.Icon;
                itemIcon.enabled = vm.Icon != null;
            }

            if (rarityBorder != null)
            {
                rarityBorder.color = vm.RarityColor;
            }

            // 장비 섹션
            if (equipmentSection != null)
            {
                equipmentSection.SetActive(vm.IsEquipment);

                if (vm.IsEquipment)
                {
                    if (slotTypeText != null)
                        slotTypeText.text = vm.SlotName;

                    if (requiredLevelText != null)
                        requiredLevelText.text = vm.RequiredLevel > 1 ? $"필요 레벨: {vm.RequiredLevel}" : "";

                    if (durabilityText != null)
                        durabilityText.text = vm.HasDurability ? $"내구도: {vm.DurabilityText}" : "";
                }
            }

            // 스탯 섹션
            if (statsSection != null)
            {
                bool hasStats = (vm.BaseStats != null && vm.BaseStats.Count > 0) ||
                               (vm.RandomStats != null && vm.RandomStats.Count > 0);
                statsSection.SetActive(hasStats);

                if (hasStats)
                {
                    UpdateStatsList(baseStatsContainer, vm.BaseStats);
                    UpdateStatsList(randomStatsContainer, vm.RandomStats);
                }
            }

            // 소비 섹션
            if (consumableSection != null)
            {
                consumableSection.SetActive(vm.IsConsumable);

                if (vm.IsConsumable)
                {
                    if (effectText != null)
                        effectText.text = vm.EffectDescription;

                    if (cooldownText != null)
                        cooldownText.text = vm.Cooldown > 0 ? $"재사용 대기: {vm.Cooldown}초" : "";
                }
            }

            // 설명
            if (descriptionText != null)
            {
                descriptionText.text = vm.Description;
                descriptionText.enabled = !string.IsNullOrEmpty(vm.Description);
            }

            // 판매 가격
            if (sellPriceText != null)
            {
                sellPriceText.text = vm.SellPrice > 0 ? $"판매가: {vm.SellPrice}G" : "";
            }
        }

        private void UpdateStatsList(Transform container, System.Collections.Generic.List<StatDisplayInfo> stats)
        {
            if (container == null)
                return;

            // 기존 항목 제거
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            if (stats == null)
                return;

            // 새 항목 생성
            foreach (var stat in stats)
            {
                if (statEntryPrefab != null)
                {
                    GameObject entry = Instantiate(statEntryPrefab, container);
                    TextMeshProUGUI text = entry.GetComponentInChildren<TextMeshProUGUI>();

                    if (text != null)
                    {
                        text.text = stat.DisplayText;
                        text.color = stat.ValueColor;
                    }
                }
            }
        }


        // ====== 위치 업데이트 ======

        private void UpdatePosition()
        {
            if (tooltipPanel == null)
                return;

            Vector3 mousePos = Input.mousePosition;
            Vector2 targetPos = mousePos + (Vector3)offset;

            // 화면 경계 보정
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                Vector2 tooltipSize = tooltipPanel.sizeDelta;

                // 우측 경계
                if (targetPos.x + tooltipSize.x > canvasRect.sizeDelta.x)
                {
                    targetPos.x = mousePos.x - tooltipSize.x - offset.x;
                }

                // 하단 경계
                if (targetPos.y - tooltipSize.y < 0)
                {
                    targetPos.y = mousePos.y + tooltipSize.y - offset.y;
                }
            }

            tooltipPanel.position = targetPos;
        }
    }
}
