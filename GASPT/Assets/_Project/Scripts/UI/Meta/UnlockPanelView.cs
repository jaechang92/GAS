using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Meta;
using GASPT.Forms;

namespace GASPT.UI.Meta
{
    /// <summary>
    /// 폼 해금 패널 UI
    /// 로비에서 Soul을 사용하여 새로운 폼을 해금
    /// </summary>
    public class UnlockPanelView : MonoBehaviour
    {
        // ====== 이벤트 ======

        /// <summary>해금 완료 이벤트</summary>
        public event Action<UnlockableForm> OnUnlocked;

        /// <summary>패널 닫힘 이벤트</summary>
        public event Action OnClosed;


        // ====== UI 요소 ======

        [Header("패널")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        [Header("Soul 표시")]
        [SerializeField] private TextMeshProUGUI soulText;
        [SerializeField] private Image soulIcon;

        [Header("폼 목록")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject unlockItemPrefab;

        [Header("상세 정보")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image formIcon;
        [SerializeField] private TextMeshProUGUI formNameText;
        [SerializeField] private TextMeshProUGUI formDescriptionText;
        [SerializeField] private TextMeshProUGUI formStatsText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI purchaseButtonText;

        [Header("상태 텍스트")]
        [SerializeField] private TextMeshProUGUI statusText;


        // ====== 런타임 ======

        private UnlockManager unlockManager;
        private MetaProgressionManager metaManager;
        private UnlockableForm selectedUnlockable;
        private List<UnlockItemUI> itemUIList = new List<UnlockItemUI>();


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }

        private void Start()
        {
            unlockManager = FindAnyObjectByType<UnlockManager>();
            metaManager = MetaProgressionManager.Instance;

            if (unlockManager != null)
            {
                unlockManager.OnFormUnlocked += OnFormUnlocked;
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();

            if (purchaseButton != null)
                purchaseButton.onClick.RemoveAllListeners();

            if (unlockManager != null)
            {
                unlockManager.OnFormUnlocked -= OnFormUnlocked;
            }
        }


        // ====== 표시/숨기기 ======

        /// <summary>
        /// 패널 표시
        /// </summary>
        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            RefreshUI();
            ClearSelection();
        }

        /// <summary>
        /// 패널 숨기기
        /// </summary>
        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            OnClosed?.Invoke();
        }

        /// <summary>
        /// 표시 상태 토글
        /// </summary>
        public void Toggle()
        {
            if (panelRoot != null && panelRoot.activeSelf)
                Hide();
            else
                Show();
        }


        // ====== UI 갱신 ======

        /// <summary>
        /// 전체 UI 갱신
        /// </summary>
        public void RefreshUI()
        {
            RefreshSoulDisplay();
            RefreshUnlockList();
            RefreshDetailPanel();
        }

        /// <summary>
        /// Soul 표시 갱신
        /// </summary>
        private void RefreshSoulDisplay()
        {
            if (soulText == null || metaManager == null) return;

            soulText.text = metaManager.Currency.Soul.ToString("N0");
        }

        /// <summary>
        /// 해금 목록 갱신
        /// </summary>
        private void RefreshUnlockList()
        {
            // 기존 아이템 제거
            ClearUnlockList();

            if (unlockManager == null || contentParent == null || unlockItemPrefab == null)
            {
                SetStatus("해금 시스템을 찾을 수 없습니다.");
                return;
            }

            // 해금 가능한 폼 목록 생성
            int index = 0;
            foreach (var unlockable in unlockManager.AllUnlockables)
            {
                CreateUnlockItem(unlockable, index++);
            }

            if (index == 0)
            {
                SetStatus("해금 가능한 폼이 없습니다.");
            }
            else
            {
                SetStatus($"총 {unlockManager.UnlockedCount}/{unlockManager.TotalCount}개 해금됨");
            }
        }

        /// <summary>
        /// 해금 아이템 UI 생성
        /// </summary>
        private void CreateUnlockItem(UnlockableForm unlockable, int index)
        {
            GameObject itemObj = Instantiate(unlockItemPrefab, contentParent);
            var itemUI = itemObj.GetComponent<UnlockItemUI>();

            if (itemUI == null)
            {
                itemUI = itemObj.AddComponent<UnlockItemUI>();
            }

            bool isUnlocked = unlockManager.IsUnlocked(unlockable.unlockId);
            bool canPurchase = unlockManager.CanUnlock(unlockable);

            itemUI.Setup(unlockable, isUnlocked, canPurchase);
            itemUI.OnSelected += () => SelectUnlockable(unlockable);

            itemUIList.Add(itemUI);
        }

        /// <summary>
        /// 목록 UI 제거
        /// </summary>
        private void ClearUnlockList()
        {
            foreach (var item in itemUIList)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            itemUIList.Clear();
        }

        /// <summary>
        /// 상세 패널 갱신
        /// </summary>
        private void RefreshDetailPanel()
        {
            if (detailPanel == null) return;

            if (selectedUnlockable == null)
            {
                detailPanel.SetActive(false);
                return;
            }

            detailPanel.SetActive(true);

            bool isUnlocked = unlockManager != null && unlockManager.IsUnlocked(selectedUnlockable.unlockId);
            bool canPurchase = unlockManager != null && unlockManager.CanUnlock(selectedUnlockable);

            // 폼 정보 표시
            var formData = selectedUnlockable.formData;
            if (formData != null)
            {
                if (formIcon != null)
                {
                    formIcon.sprite = isUnlocked || canPurchase ? formData.icon : null;
                    formIcon.color = GetRarityColor(formData.baseRarity);
                }

                if (formNameText != null)
                {
                    formNameText.text = isUnlocked ? formData.formName : selectedUnlockable.lockedDisplayName;
                    formNameText.color = GetRarityColor(formData.baseRarity);
                }

                if (formDescriptionText != null)
                {
                    formDescriptionText.text = isUnlocked ? formData.description : selectedUnlockable.unlockHint;
                }

                if (formStatsText != null && isUnlocked)
                {
                    var stats = formData.GetStatsAtAwakening(0);
                    formStatsText.text = FormatStats(stats);
                    formStatsText.gameObject.SetActive(true);
                }
                else if (formStatsText != null)
                {
                    formStatsText.gameObject.SetActive(false);
                }
            }

            // 비용 표시
            if (costText != null)
            {
                if (isUnlocked)
                {
                    costText.text = "해금됨";
                    costText.color = Color.green;
                }
                else
                {
                    costText.text = $"{selectedUnlockable.soulCost} Soul";
                    costText.color = canPurchase ? Color.white : Color.red;
                }
            }

            // 구매 버튼
            if (purchaseButton != null)
            {
                purchaseButton.interactable = canPurchase;

                if (purchaseButtonText != null)
                {
                    if (isUnlocked)
                        purchaseButtonText.text = "해금 완료";
                    else if (canPurchase)
                        purchaseButtonText.text = "해금하기";
                    else
                        purchaseButtonText.text = "조건 미충족";
                }
            }
        }


        // ====== 선택 ======

        /// <summary>
        /// 해금 가능 폼 선택
        /// </summary>
        private void SelectUnlockable(UnlockableForm unlockable)
        {
            selectedUnlockable = unlockable;
            RefreshDetailPanel();

            // 선택 상태 UI 업데이트
            foreach (var item in itemUIList)
            {
                if (item != null)
                {
                    item.SetSelected(item.Unlockable == unlockable);
                }
            }
        }

        /// <summary>
        /// 선택 해제
        /// </summary>
        private void ClearSelection()
        {
            selectedUnlockable = null;
            RefreshDetailPanel();

            foreach (var item in itemUIList)
            {
                if (item != null)
                {
                    item.SetSelected(false);
                }
            }
        }


        // ====== 구매 ======

        /// <summary>
        /// 구매 버튼 클릭
        /// </summary>
        private void OnPurchaseClicked()
        {
            if (selectedUnlockable == null || unlockManager == null) return;

            if (unlockManager.TryUnlock(selectedUnlockable))
            {
                Debug.Log($"[UnlockPanelView] 해금 성공: {selectedUnlockable.formData?.formName}");
            }
            else
            {
                Debug.LogWarning($"[UnlockPanelView] 해금 실패: {selectedUnlockable.unlockId}");
            }
        }

        /// <summary>
        /// 폼 해금 완료 콜백
        /// </summary>
        private void OnFormUnlocked(UnlockableForm unlockable)
        {
            RefreshUI();
            OnUnlocked?.Invoke(unlockable);
        }


        // ====== 유틸리티 ======

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private string FormatStats(FormStats stats)
        {
            return $"공격력: +{stats.attackPower:F0}\n" +
                   $"공속: x{stats.attackSpeed:F1}\n" +
                   $"치확: {stats.criticalChance:P0}\n" +
                   $"이속: x{stats.moveSpeed:F1}";
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
    }


    /// <summary>
    /// 해금 아이템 UI (목록의 각 항목)
    /// </summary>
    public class UnlockItemUI : MonoBehaviour
    {
        // ====== 이벤트 ======

        public event Action OnSelected;


        // ====== UI 요소 ======

        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private GameObject unlockedMark;
        [SerializeField] private Button selectButton;


        // ====== 상태 ======

        private UnlockableForm unlockable;
        private bool isUnlocked;
        private bool isSelected;


        // ====== 프로퍼티 ======

        public UnlockableForm Unlockable => unlockable;


        // ====== 초기화 ======

        private void Awake()
        {
            // 버튼이 없으면 자동 추가
            if (selectButton == null)
            {
                selectButton = GetComponent<Button>();
                if (selectButton == null)
                {
                    selectButton = gameObject.AddComponent<Button>();
                }
            }

            selectButton.onClick.AddListener(() => OnSelected?.Invoke());
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
        /// 아이템 설정
        /// </summary>
        public void Setup(UnlockableForm unlockable, bool isUnlocked, bool canPurchase)
        {
            this.unlockable = unlockable;
            this.isUnlocked = isUnlocked;

            UpdateVisuals(canPurchase);
        }

        /// <summary>
        /// 선택 상태 설정
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;

            if (backgroundImage != null)
            {
                backgroundImage.color = selected ? new Color(1f, 1f, 1f, 0.3f) : new Color(1f, 1f, 1f, 0.1f);
            }
        }


        // ====== 시각 업데이트 ======

        private void UpdateVisuals(bool canPurchase)
        {
            if (unlockable == null) return;

            var formData = unlockable.formData;

            // 아이콘
            if (iconImage != null && formData != null)
            {
                iconImage.sprite = isUnlocked ? formData.icon : null;
                iconImage.color = isUnlocked ? GetRarityColor(formData.baseRarity) : Color.gray;
            }

            // 이름
            if (nameText != null)
            {
                nameText.text = isUnlocked ? formData?.formName : unlockable.lockedDisplayName;
                nameText.color = isUnlocked ? Color.white : Color.gray;
            }

            // 비용
            if (costText != null)
            {
                if (isUnlocked)
                {
                    costText.text = "";
                }
                else
                {
                    costText.text = $"{unlockable.soulCost}";
                    costText.color = canPurchase ? Color.yellow : Color.red;
                }
            }

            // 잠금 오버레이
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!isUnlocked);
            }

            // 해금 마크
            if (unlockedMark != null)
            {
                unlockedMark.SetActive(isUnlocked);
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
    }
}
