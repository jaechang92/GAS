using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Meta;

namespace GASPT.UI.Meta
{
    /// <summary>
    /// 업그레이드 트리 뷰
    /// 모든 영구 업그레이드를 트리/그리드 형태로 표시
    /// </summary>
    public class UpgradeTreeView : MonoBehaviour
    {
        // ====== 이벤트 ======

        /// <summary>업그레이드 구매 완료 이벤트</summary>
        public event Action<PermanentUpgrade, int> OnUpgradePurchased;

        /// <summary>패널 닫힘 이벤트</summary>
        public event Action OnClosed;


        // ====== UI 요소 ======

        [Header("패널")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        [Header("재화 표시")]
        [SerializeField] private TextMeshProUGUI boneText;
        [SerializeField] private TextMeshProUGUI soulText;

        [Header("노드 목록")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject nodeViewPrefab;

        [Header("상세 정보")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailDescriptionText;
        [SerializeField] private TextMeshProUGUI detailLevelText;
        [SerializeField] private TextMeshProUGUI currentEffectText;
        [SerializeField] private TextMeshProUGUI nextEffectText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI purchaseButtonText;

        [Header("카테고리 탭")]
        [SerializeField] private Button allTab;
        [SerializeField] private Button combatTab;
        [SerializeField] private Button utilityTab;
        [SerializeField] private Button specialTab;

        [Header("상태")]
        [SerializeField] private TextMeshProUGUI statusText;


        // ====== 런타임 ======

        private MetaProgressionManager metaManager;
        private UpgradeNodeView selectedNode;
        private List<UpgradeNodeView> nodeViews = new List<UpgradeNodeView>();
        private UpgradeCategory currentCategory = UpgradeCategory.All;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            SetupButtons();
        }

        private void Start()
        {
            metaManager = MetaProgressionManager.Instance;

            if (metaManager != null)
            {
                metaManager.OnUpgradePurchased += OnUpgradePurchasedCallback;
                metaManager.OnProgressLoaded += RefreshUI;
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (metaManager != null)
            {
                metaManager.OnUpgradePurchased -= OnUpgradePurchasedCallback;
                metaManager.OnProgressLoaded -= RefreshUI;
            }

            ClearButtons();
        }


        // ====== 버튼 설정 ======

        private void SetupButtons()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);

            // 카테고리 탭
            if (allTab != null)
                allTab.onClick.AddListener(() => SetCategory(UpgradeCategory.All));

            if (combatTab != null)
                combatTab.onClick.AddListener(() => SetCategory(UpgradeCategory.Combat));

            if (utilityTab != null)
                utilityTab.onClick.AddListener(() => SetCategory(UpgradeCategory.Utility));

            if (specialTab != null)
                specialTab.onClick.AddListener(() => SetCategory(UpgradeCategory.Special));
        }

        private void ClearButtons()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();

            if (purchaseButton != null)
                purchaseButton.onClick.RemoveAllListeners();

            if (allTab != null) allTab.onClick.RemoveAllListeners();
            if (combatTab != null) combatTab.onClick.RemoveAllListeners();
            if (utilityTab != null) utilityTab.onClick.RemoveAllListeners();
            if (specialTab != null) specialTab.onClick.RemoveAllListeners();
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
        /// 토글
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
            RefreshCurrencyDisplay();
            RefreshNodeList();
            RefreshDetailPanel();
            UpdateStatus();
        }

        /// <summary>
        /// 재화 표시 갱신
        /// </summary>
        private void RefreshCurrencyDisplay()
        {
            if (metaManager == null) return;

            if (boneText != null)
                boneText.text = metaManager.Currency.Bone.ToString("N0");

            if (soulText != null)
                soulText.text = metaManager.Currency.Soul.ToString("N0");
        }

        /// <summary>
        /// 노드 목록 갱신
        /// </summary>
        private void RefreshNodeList()
        {
            ClearNodeList();

            if (metaManager == null || contentParent == null) return;

            foreach (var upgrade in metaManager.GetAllUpgrades())
            {
                if (!ShouldShowUpgrade(upgrade)) continue;

                CreateNodeView(upgrade);
            }
        }

        /// <summary>
        /// 노드 뷰 생성
        /// </summary>
        private void CreateNodeView(PermanentUpgrade upgrade)
        {
            GameObject nodeObj;

            if (nodeViewPrefab != null)
            {
                nodeObj = Instantiate(nodeViewPrefab, contentParent);
            }
            else
            {
                // 기본 노드 생성
                nodeObj = CreateDefaultNodeObject();
                nodeObj.transform.SetParent(contentParent, false);
            }

            var nodeView = nodeObj.GetComponent<UpgradeNodeView>();
            if (nodeView == null)
            {
                nodeView = nodeObj.AddComponent<UpgradeNodeView>();
            }

            int currentLevel = metaManager.GetUpgradeLevel(upgrade.upgradeId);
            bool isLocked = !CheckPrerequisites(upgrade);

            nodeView.Setup(upgrade, currentLevel, isLocked);
            nodeView.OnSelected += OnNodeSelected;

            nodeViews.Add(nodeView);
        }

        /// <summary>
        /// 기본 노드 오브젝트 생성
        /// </summary>
        private GameObject CreateDefaultNodeObject()
        {
            var nodeObj = new GameObject("UpgradeNode");

            // RectTransform
            var rectTransform = nodeObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(120, 140);

            // 배경
            var bg = new GameObject("Background");
            bg.transform.SetParent(nodeObj.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // 버튼
            nodeObj.AddComponent<Button>();

            return nodeObj;
        }

        /// <summary>
        /// 노드 목록 제거
        /// </summary>
        private void ClearNodeList()
        {
            foreach (var node in nodeViews)
            {
                if (node != null)
                {
                    node.OnSelected -= OnNodeSelected;
                    Destroy(node.gameObject);
                }
            }
            nodeViews.Clear();
        }

        /// <summary>
        /// 상세 패널 갱신
        /// </summary>
        private void RefreshDetailPanel()
        {
            if (detailPanel == null) return;

            if (selectedNode == null || selectedNode.Upgrade == null)
            {
                detailPanel.SetActive(false);
                return;
            }

            detailPanel.SetActive(true);

            var upgrade = selectedNode.Upgrade;
            int currentLevel = selectedNode.CurrentLevel;
            bool isMaxed = selectedNode.IsMaxed;
            bool isLocked = selectedNode.IsLocked;

            // 아이콘
            if (detailIcon != null)
            {
                detailIcon.sprite = upgrade.icon;
            }

            // 이름
            if (detailNameText != null)
            {
                detailNameText.text = upgrade.upgradeName;
            }

            // 설명
            if (detailDescriptionText != null)
            {
                detailDescriptionText.text = upgrade.description;
            }

            // 레벨
            if (detailLevelText != null)
            {
                detailLevelText.text = isMaxed ? "MAX LEVEL" : $"Level {currentLevel} / {upgrade.maxLevel}";
            }

            // 현재 효과
            if (currentEffectText != null)
            {
                if (currentLevel > 0)
                {
                    currentEffectText.text = $"현재: {upgrade.GetEffectDescription(currentLevel)}";
                    currentEffectText.gameObject.SetActive(true);
                }
                else
                {
                    currentEffectText.gameObject.SetActive(false);
                }
            }

            // 다음 효과
            if (nextEffectText != null)
            {
                if (!isMaxed)
                {
                    nextEffectText.text = $"다음: {upgrade.GetEffectDescription(currentLevel + 1)}";
                    nextEffectText.color = Color.green;
                    nextEffectText.gameObject.SetActive(true);
                }
                else
                {
                    nextEffectText.gameObject.SetActive(false);
                }
            }

            // 비용
            if (costText != null)
            {
                if (isMaxed)
                {
                    costText.text = "최대 레벨 도달";
                    costText.color = Color.yellow;
                }
                else if (isLocked)
                {
                    costText.text = "선행 조건 미충족";
                    costText.color = Color.red;
                }
                else
                {
                    int cost = upgrade.GetUpgradeCost(currentLevel);
                    string currencyName = upgrade.currencyType == CurrencyType.Bone ? "Bone" : "Soul";
                    bool canAfford = CanAffordUpgrade(upgrade, currentLevel);

                    costText.text = $"{cost:N0} {currencyName}";
                    costText.color = canAfford ? Color.white : Color.red;
                }
            }

            // 구매 버튼
            UpdatePurchaseButton(upgrade, currentLevel, isMaxed, isLocked);
        }

        /// <summary>
        /// 구매 버튼 업데이트
        /// </summary>
        private void UpdatePurchaseButton(PermanentUpgrade upgrade, int currentLevel, bool isMaxed, bool isLocked)
        {
            if (purchaseButton == null) return;

            bool canPurchase = !isMaxed && !isLocked && CanAffordUpgrade(upgrade, currentLevel);
            purchaseButton.interactable = canPurchase;

            if (purchaseButtonText != null)
            {
                if (isMaxed)
                    purchaseButtonText.text = "최대 레벨";
                else if (isLocked)
                    purchaseButtonText.text = "잠김";
                else if (!canPurchase)
                    purchaseButtonText.text = "재화 부족";
                else
                    purchaseButtonText.text = "업그레이드";
            }
        }

        /// <summary>
        /// 상태 텍스트 업데이트
        /// </summary>
        private void UpdateStatus()
        {
            if (statusText == null || metaManager == null) return;

            int totalUpgrades = 0;
            int purchasedLevels = 0;
            int maxLevels = 0;

            foreach (var upgrade in metaManager.GetAllUpgrades())
            {
                totalUpgrades++;
                int level = metaManager.GetUpgradeLevel(upgrade.upgradeId);
                purchasedLevels += level;
                maxLevels += upgrade.maxLevel;
            }

            statusText.text = $"업그레이드: {purchasedLevels}/{maxLevels}";
        }


        // ====== 선택 ======

        /// <summary>
        /// 노드 선택 콜백
        /// </summary>
        private void OnNodeSelected(UpgradeNodeView node)
        {
            // 이전 선택 해제
            if (selectedNode != null)
            {
                selectedNode.SetSelected(false);
            }

            selectedNode = node;

            if (selectedNode != null)
            {
                selectedNode.SetSelected(true);
            }

            RefreshDetailPanel();
        }

        /// <summary>
        /// 선택 해제
        /// </summary>
        private void ClearSelection()
        {
            if (selectedNode != null)
            {
                selectedNode.SetSelected(false);
                selectedNode = null;
            }

            RefreshDetailPanel();
        }


        // ====== 구매 ======

        /// <summary>
        /// 구매 버튼 클릭
        /// </summary>
        private void OnPurchaseButtonClicked()
        {
            if (selectedNode == null || metaManager == null) return;

            var upgrade = selectedNode.Upgrade;
            if (upgrade == null) return;

            if (metaManager.TryPurchaseUpgrade(upgrade.upgradeId))
            {
                Debug.Log($"[UpgradeTreeView] 업그레이드 구매 성공: {upgrade.upgradeName}");
            }
        }

        /// <summary>
        /// 업그레이드 구매 콜백
        /// </summary>
        private void OnUpgradePurchasedCallback(string upgradeId, int newLevel)
        {
            // 노드 업데이트
            foreach (var node in nodeViews)
            {
                if (node.Upgrade?.upgradeId == upgradeId)
                {
                    node.UpdateLevel(newLevel);
                    break;
                }
            }

            // 선행 조건 업데이트 (다른 노드들의 잠금 상태)
            RefreshNodeLockStates();

            RefreshCurrencyDisplay();
            RefreshDetailPanel();
            UpdateStatus();

            // 이벤트 발생
            var upgrade = metaManager?.GetUpgrade(upgradeId);
            if (upgrade != null)
            {
                OnUpgradePurchased?.Invoke(upgrade, newLevel);
            }
        }

        /// <summary>
        /// 노드 잠금 상태 갱신
        /// </summary>
        private void RefreshNodeLockStates()
        {
            foreach (var node in nodeViews)
            {
                if (node.Upgrade != null)
                {
                    bool isLocked = !CheckPrerequisites(node.Upgrade);
                    node.SetLocked(isLocked);
                }
            }
        }


        // ====== 카테고리 ======

        /// <summary>
        /// 카테고리 설정
        /// </summary>
        private void SetCategory(UpgradeCategory category)
        {
            currentCategory = category;
            RefreshNodeList();
            ClearSelection();
        }

        /// <summary>
        /// 업그레이드 표시 여부 확인
        /// </summary>
        private bool ShouldShowUpgrade(PermanentUpgrade upgrade)
        {
            if (currentCategory == UpgradeCategory.All)
                return true;

            return GetUpgradeCategory(upgrade.upgradeType) == currentCategory;
        }

        /// <summary>
        /// 업그레이드 타입으로 카테고리 결정
        /// </summary>
        private UpgradeCategory GetUpgradeCategory(UpgradeType type)
        {
            return type switch
            {
                UpgradeType.MaxHP => UpgradeCategory.Combat,
                UpgradeType.Attack => UpgradeCategory.Combat,
                UpgradeType.Defense => UpgradeCategory.Combat,
                UpgradeType.MoveSpeed => UpgradeCategory.Utility,
                UpgradeType.GoldBonus => UpgradeCategory.Utility,
                UpgradeType.ExpBonus => UpgradeCategory.Utility,
                UpgradeType.StartGold => UpgradeCategory.Utility,
                UpgradeType.ExtraDash => UpgradeCategory.Special,
                UpgradeType.Revive => UpgradeCategory.Special,
                _ => UpgradeCategory.All
            };
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 선행 조건 확인
        /// </summary>
        private bool CheckPrerequisites(PermanentUpgrade upgrade)
        {
            if (upgrade.prerequisiteIds == null || upgrade.prerequisiteIds.Length == 0)
                return true;

            if (metaManager == null)
                return false;

            foreach (string prereqId in upgrade.prerequisiteIds)
            {
                if (string.IsNullOrEmpty(prereqId)) continue;

                var prereq = metaManager.GetUpgrade(prereqId);
                if (prereq == null) continue;

                int prereqLevel = metaManager.GetUpgradeLevel(prereqId);
                if (prereqLevel < prereq.maxLevel)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 구매 가능 여부 확인
        /// </summary>
        private bool CanAffordUpgrade(PermanentUpgrade upgrade, int currentLevel)
        {
            if (metaManager == null) return false;

            int cost = upgrade.GetUpgradeCost(currentLevel);
            if (cost <= 0) return false;

            return upgrade.currencyType switch
            {
                CurrencyType.Bone => metaManager.Currency.Bone >= cost,
                CurrencyType.Soul => metaManager.Currency.Soul >= cost,
                _ => false
            };
        }
    }


    /// <summary>
    /// 업그레이드 카테고리
    /// </summary>
    public enum UpgradeCategory
    {
        All,
        Combat,
        Utility,
        Special
    }
}
