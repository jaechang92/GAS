using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// 분기 선택 UI 뷰
    /// 여러 방 중 하나를 선택하는 UI
    /// </summary>
    public class BranchSelectionView : MonoBehaviour, IBranchSelectionView
    {
        // ====== UI 요소 ======

        [Header("컨테이너")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private Transform optionsContainer;

        [Header("옵션 프리팹")]
        [SerializeField] private GameObject optionButtonPrefab;

        [Header("텍스트")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI hintText;

        [Header("설정")]
        [SerializeField] private float inputCooldown = 0.2f;


        // ====== 이벤트 ======

        public event Action<int> OnOptionSelected;
        public event Action OnCancelled;


        // ====== 상태 ======

        private List<BranchOptionData> currentOptions = new List<BranchOptionData>();
        private List<BranchOptionButton> optionButtons = new List<BranchOptionButton>();
        private int selectedIndex = 0;
        private float lastInputTime;
        private bool isVisible;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isVisible) return;

            HandleInput();
        }


        // ====== IBranchSelectionView 구현 ======

        public void Show()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
            }
            isVisible = true;
            selectedIndex = 0;
            UpdateSelection();

            Debug.Log("[BranchSelectionView] UI 표시");
        }

        public void Hide()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
            isVisible = false;

            Debug.Log("[BranchSelectionView] UI 숨김");
        }

        public void SetOptions(List<BranchOptionData> options)
        {
            currentOptions = options ?? new List<BranchOptionData>();

            // 기존 버튼 제거
            ClearOptionButtons();

            // 새 버튼 생성
            CreateOptionButtons();

            // 타이틀 업데이트
            if (titleText != null)
            {
                titleText.text = "다음 방을 선택하세요";
            }

            // 힌트 업데이트
            if (hintText != null)
            {
                hintText.text = "← → 또는 A D 로 선택, Enter 또는 Space 로 확정";
            }

            Debug.Log($"[BranchSelectionView] {options.Count}개 옵션 설정됨");
        }

        public void SetSelectedIndex(int index)
        {
            if (currentOptions.Count == 0) return;

            selectedIndex = Mathf.Clamp(index, 0, currentOptions.Count - 1);
            UpdateSelection();
        }


        // ====== 입력 처리 ======

        private void HandleInput()
        {
            // 입력 쿨다운
            if (Time.time - lastInputTime < inputCooldown) return;

            // 좌우 이동
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                NavigateLeft();
                lastInputTime = Time.time;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                NavigateRight();
                lastInputTime = Time.time;
            }
            // 선택 확정
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            {
                ConfirmSelection();
                lastInputTime = Time.time;
            }
            // 취소
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cancel();
                lastInputTime = Time.time;
            }
        }

        private void NavigateLeft()
        {
            if (currentOptions.Count == 0) return;

            selectedIndex--;
            if (selectedIndex < 0)
            {
                selectedIndex = currentOptions.Count - 1;
            }
            UpdateSelection();
        }

        private void NavigateRight()
        {
            if (currentOptions.Count == 0) return;

            selectedIndex++;
            if (selectedIndex >= currentOptions.Count)
            {
                selectedIndex = 0;
            }
            UpdateSelection();
        }

        private void ConfirmSelection()
        {
            if (currentOptions.Count == 0) return;

            Debug.Log($"[BranchSelectionView] 옵션 선택: {selectedIndex}");
            OnOptionSelected?.Invoke(selectedIndex);
        }

        private void Cancel()
        {
            Debug.Log("[BranchSelectionView] 취소");
            OnCancelled?.Invoke();
        }


        // ====== 버튼 관리 ======

        private void ClearOptionButtons()
        {
            foreach (var button in optionButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            optionButtons.Clear();
        }

        private void CreateOptionButtons()
        {
            if (optionButtonPrefab == null || optionsContainer == null)
            {
                Debug.LogWarning("[BranchSelectionView] optionButtonPrefab 또는 optionsContainer가 설정되지 않았습니다.");
                return;
            }

            for (int i = 0; i < currentOptions.Count; i++)
            {
                var option = currentOptions[i];
                var buttonObj = Instantiate(optionButtonPrefab, optionsContainer);
                var button = buttonObj.GetComponent<BranchOptionButton>();

                if (button != null)
                {
                    button.Setup(option, i);
                    button.OnClicked += OnButtonClicked;
                    optionButtons.Add(button);
                }
                else
                {
                    // BranchOptionButton 컴포넌트가 없으면 간단히 텍스트만 설정
                    var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = $"{option.typeName}\n{option.difficultyText}";
                    }
                }
            }
        }

        private void OnButtonClicked(int index)
        {
            selectedIndex = index;
            ConfirmSelection();
        }

        private void UpdateSelection()
        {
            for (int i = 0; i < optionButtons.Count; i++)
            {
                var button = optionButtons[i];
                if (button != null)
                {
                    button.SetSelected(i == selectedIndex);
                }
            }
        }
    }


    /// <summary>
    /// 분기 선택 옵션 버튼
    /// </summary>
    public class BranchOptionButton : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI typeNameText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.yellow;

        public event Action<int> OnClicked;

        private int index;
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnClicked?.Invoke(index));
            }
        }

        public void Setup(BranchOptionData data, int index)
        {
            this.index = index;

            if (typeNameText != null)
                typeNameText.text = data.typeName;

            if (difficultyText != null)
                difficultyText.text = data.difficultyText;

            if (rewardText != null)
                rewardText.text = data.rewardHint;

            // TODO: 아이콘 로드 (Resources.Load 또는 스프라이트 아틀라스)
        }

        public void SetSelected(bool selected)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = selected ? selectedColor : normalColor;
            }

            // 스케일 애니메이션
            transform.localScale = selected ? Vector3.one * 1.1f : Vector3.one;
        }
    }
}
