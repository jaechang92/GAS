using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Meta;

namespace GASPT.UI.Meta
{
    /// <summary>
    /// 업적 목록 UI
    /// 모든 업적의 진행도와 완료 상태를 표시
    /// </summary>
    public class AchievementListView : MonoBehaviour
    {
        // ====== 이벤트 ======

        /// <summary>패널 닫힘 이벤트</summary>
        public event Action OnClosed;


        // ====== UI 요소 ======

        [Header("패널")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        [Header("상태 표시")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image progressBar;

        [Header("필터 탭")]
        [SerializeField] private Button allTab;
        [SerializeField] private Button incompleteTab;
        [SerializeField] private Button completedTab;

        [Header("목록")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject achievementItemPrefab;

        [Header("상세 정보")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailDescText;
        [SerializeField] private TextMeshProUGUI detailProgressText;
        [SerializeField] private Image detailProgressBar;
        [SerializeField] private TextMeshProUGUI detailRewardText;
        [SerializeField] private Image detailTierBadge;


        // ====== 런타임 ======

        private AchievementManager achievementManager;
        private List<AchievementItemUI> itemUIList = new List<AchievementItemUI>();
        private Achievement selectedAchievement;
        private FilterType currentFilter = FilterType.All;


        // ====== 필터 타입 ======

        private enum FilterType
        {
            All,
            Incomplete,
            Completed
        }


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            SetupButtons();
        }

        private void Start()
        {
            achievementManager = AchievementManager.Instance;

            if (achievementManager != null)
            {
                achievementManager.OnAchievementCompleted += OnAchievementCompleted;
                achievementManager.OnProgressUpdated += OnProgressUpdated;
            }

            Hide();
        }

        private void OnDestroy()
        {
            ClearButtons();

            if (achievementManager != null)
            {
                achievementManager.OnAchievementCompleted -= OnAchievementCompleted;
                achievementManager.OnProgressUpdated -= OnProgressUpdated;
            }
        }


        // ====== 버튼 설정 ======

        private void SetupButtons()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (allTab != null)
                allTab.onClick.AddListener(() => SetFilter(FilterType.All));

            if (incompleteTab != null)
                incompleteTab.onClick.AddListener(() => SetFilter(FilterType.Incomplete));

            if (completedTab != null)
                completedTab.onClick.AddListener(() => SetFilter(FilterType.Completed));
        }

        private void ClearButtons()
        {
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
            if (allTab != null) allTab.onClick.RemoveAllListeners();
            if (incompleteTab != null) incompleteTab.onClick.RemoveAllListeners();
            if (completedTab != null) completedTab.onClick.RemoveAllListeners();
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
            RefreshStatus();
            RefreshList();
            RefreshDetailPanel();
        }

        /// <summary>
        /// 상태 표시 갱신
        /// </summary>
        private void RefreshStatus()
        {
            if (achievementManager == null) return;

            int completed = achievementManager.CompletedCount;
            int total = achievementManager.TotalCount;

            if (statusText != null)
            {
                statusText.text = $"업적: {completed} / {total}";
            }

            if (progressBar != null && total > 0)
            {
                progressBar.fillAmount = (float)completed / total;
            }
        }

        /// <summary>
        /// 목록 갱신
        /// </summary>
        private void RefreshList()
        {
            ClearList();

            if (achievementManager == null || contentParent == null) return;

            IEnumerable<Achievement> achievements = currentFilter switch
            {
                FilterType.Incomplete => achievementManager.GetIncompleteAchievements(),
                FilterType.Completed => achievementManager.GetCompletedAchievements(),
                _ => achievementManager.AllAchievements
            };

            foreach (var achievement in achievements)
            {
                // 숨김 업적은 완료 전까지 표시 안 함
                if (achievement.isHidden && !achievementManager.IsCompleted(achievement.achievementId))
                    continue;

                CreateItemUI(achievement);
            }
        }

        /// <summary>
        /// 아이템 UI 생성
        /// </summary>
        private void CreateItemUI(Achievement achievement)
        {
            GameObject itemObj;

            if (achievementItemPrefab != null)
            {
                itemObj = Instantiate(achievementItemPrefab, contentParent);
            }
            else
            {
                itemObj = CreateDefaultItemObject();
                itemObj.transform.SetParent(contentParent, false);
            }

            var itemUI = itemObj.GetComponent<AchievementItemUI>();
            if (itemUI == null)
            {
                itemUI = itemObj.AddComponent<AchievementItemUI>();
            }

            int progress = achievementManager.GetProgress(achievement.achievementId);
            bool isCompleted = achievementManager.IsCompleted(achievement.achievementId);

            itemUI.Setup(achievement, progress, isCompleted);
            itemUI.OnSelected += () => SelectAchievement(achievement);

            itemUIList.Add(itemUI);
        }

        /// <summary>
        /// 기본 아이템 오브젝트 생성
        /// </summary>
        private GameObject CreateDefaultItemObject()
        {
            GameObject itemObj = new GameObject("AchievementItem");

            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 80);

            Image bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            itemObj.AddComponent<Button>();

            // 레이아웃
            HorizontalLayoutGroup layout = itemObj.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            return itemObj;
        }

        /// <summary>
        /// 목록 제거
        /// </summary>
        private void ClearList()
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

            if (selectedAchievement == null)
            {
                detailPanel.SetActive(false);
                return;
            }

            detailPanel.SetActive(true);

            int progress = achievementManager?.GetProgress(selectedAchievement.achievementId) ?? 0;
            bool isCompleted = achievementManager?.IsCompleted(selectedAchievement.achievementId) ?? false;

            // 아이콘
            if (detailIcon != null)
            {
                detailIcon.sprite = selectedAchievement.icon;
                detailIcon.color = isCompleted ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            // 이름
            if (detailNameText != null)
            {
                detailNameText.text = selectedAchievement.achievementName;
                detailNameText.color = selectedAchievement.GetTierColor();
            }

            // 설명
            if (detailDescText != null)
            {
                detailDescText.text = selectedAchievement.description;
            }

            // 진행도 텍스트
            if (detailProgressText != null)
            {
                if (isCompleted)
                {
                    detailProgressText.text = "완료!";
                    detailProgressText.color = Color.green;
                }
                else
                {
                    detailProgressText.text = selectedAchievement.GetProgressText(progress);
                    detailProgressText.color = Color.white;
                }
            }

            // 진행도 바
            if (detailProgressBar != null)
            {
                detailProgressBar.fillAmount = selectedAchievement.GetProgress(progress);
                detailProgressBar.color = isCompleted ? Color.green : new Color(0.3f, 0.6f, 0.9f);
            }

            // 보상
            if (detailRewardText != null)
            {
                detailRewardText.text = $"보상: {selectedAchievement.GetRewardText()}";
            }

            // 등급 배지
            if (detailTierBadge != null)
            {
                detailTierBadge.color = selectedAchievement.GetTierColor();
            }
        }


        // ====== 선택 ======

        /// <summary>
        /// 업적 선택
        /// </summary>
        private void SelectAchievement(Achievement achievement)
        {
            selectedAchievement = achievement;

            // 선택 상태 업데이트
            foreach (var item in itemUIList)
            {
                if (item != null)
                {
                    item.SetSelected(item.Achievement == achievement);
                }
            }

            RefreshDetailPanel();
        }

        /// <summary>
        /// 선택 해제
        /// </summary>
        private void ClearSelection()
        {
            selectedAchievement = null;

            foreach (var item in itemUIList)
            {
                if (item != null)
                {
                    item.SetSelected(false);
                }
            }

            RefreshDetailPanel();
        }


        // ====== 필터 ======

        /// <summary>
        /// 필터 설정
        /// </summary>
        private void SetFilter(FilterType filter)
        {
            currentFilter = filter;
            RefreshList();
            ClearSelection();
        }


        // ====== 이벤트 콜백 ======

        private void OnAchievementCompleted(Achievement achievement)
        {
            RefreshUI();

            // 완료 알림 표시 (TODO: 토스트 메시지)
            Debug.Log($"[AchievementListView] 업적 완료: {achievement.achievementName}");
        }

        private void OnProgressUpdated(string achievementId, int current, int target)
        {
            // 해당 아이템만 업데이트
            foreach (var item in itemUIList)
            {
                if (item.Achievement?.achievementId == achievementId)
                {
                    bool isCompleted = achievementManager?.IsCompleted(achievementId) ?? false;
                    item.UpdateProgress(current, isCompleted);
                    break;
                }
            }

            // 선택된 업적이면 상세 패널도 업데이트
            if (selectedAchievement?.achievementId == achievementId)
            {
                RefreshDetailPanel();
            }
        }
    }


    /// <summary>
    /// 업적 아이템 UI
    /// </summary>
    public class AchievementItemUI : MonoBehaviour
    {
        // ====== 이벤트 ======

        public event Action OnSelected;


        // ====== UI 요소 ======

        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image progressBar;
        [SerializeField] private GameObject completedMark;
        [SerializeField] private Image tierBadge;
        [SerializeField] private Button selectButton;


        // ====== 상태 ======

        private Achievement achievement;
        private bool isCompleted;


        // ====== 프로퍼티 ======

        public Achievement Achievement => achievement;


        // ====== 초기화 ======

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
        public void Setup(Achievement achievement, int currentProgress, bool isCompleted)
        {
            this.achievement = achievement;
            this.isCompleted = isCompleted;

            UpdateVisuals(currentProgress);
        }

        /// <summary>
        /// 진행도 업데이트
        /// </summary>
        public void UpdateProgress(int currentProgress, bool isCompleted)
        {
            this.isCompleted = isCompleted;
            UpdateVisuals(currentProgress);
        }

        /// <summary>
        /// 선택 상태 설정
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = selected
                    ? new Color(0.3f, 0.3f, 0.3f, 0.9f)
                    : new Color(0.15f, 0.15f, 0.15f, 0.9f);
            }
        }


        // ====== 시각 업데이트 ======

        private void UpdateVisuals(int currentProgress)
        {
            if (achievement == null) return;

            // 아이콘
            if (iconImage != null)
            {
                iconImage.sprite = achievement.icon;
                iconImage.color = isCompleted ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            // 이름
            if (nameText != null)
            {
                nameText.text = achievement.achievementName;
                nameText.color = isCompleted ? achievement.GetTierColor() : Color.gray;
            }

            // 진행도 텍스트
            if (progressText != null)
            {
                if (isCompleted)
                {
                    progressText.text = "완료";
                    progressText.color = Color.green;
                }
                else if (achievement.showProgress)
                {
                    progressText.text = achievement.GetProgressText(currentProgress);
                    progressText.color = Color.white;
                }
                else
                {
                    progressText.text = "";
                }
            }

            // 진행도 바
            if (progressBar != null)
            {
                progressBar.fillAmount = achievement.GetProgress(currentProgress);
                progressBar.color = isCompleted ? Color.green : new Color(0.3f, 0.6f, 0.9f);
            }

            // 완료 마크
            if (completedMark != null)
            {
                completedMark.SetActive(isCompleted);
            }

            // 등급 배지
            if (tierBadge != null)
            {
                tierBadge.color = achievement.GetTierColor();
            }
        }
    }
}
