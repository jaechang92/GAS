using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GASPT.UI.Meta
{
    /// <summary>
    /// 부활 프롬프트 UI
    /// 플레이어 사망 시 부활 옵션 표시
    /// </summary>
    public class RevivePromptView : MonoBehaviour
    {
        // ====== 이벤트 ======

        /// <summary>부활 선택 이벤트</summary>
        public event Action OnReviveSelected;

        /// <summary>포기 선택 이벤트</summary>
        public event Action OnGiveUpSelected;


        // ====== UI 요소 ======

        [Header("패널")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("텍스트")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI remainingText;

        [Header("버튼")]
        [SerializeField] private Button reviveButton;
        [SerializeField] private TextMeshProUGUI reviveButtonText;
        [SerializeField] private Button giveUpButton;
        [SerializeField] private TextMeshProUGUI giveUpButtonText;

        [Header("시각 효과")]
        [SerializeField] private Image backgroundOverlay;
        [SerializeField] private float fadeInDuration = 0.5f;


        // ====== 런타임 ======

        private bool isShowing;
        private float fadeTimer;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            SetupButtons();
            Hide();
        }

        private void OnDestroy()
        {
            ClearButtons();
        }

        private void Update()
        {
            if (isShowing && fadeTimer < fadeInDuration)
            {
                fadeTimer += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(fadeTimer / fadeInDuration);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = alpha;
                }
            }
        }


        // ====== 버튼 설정 ======

        private void SetupButtons()
        {
            if (reviveButton != null)
                reviveButton.onClick.AddListener(OnReviveClicked);

            if (giveUpButton != null)
                giveUpButton.onClick.AddListener(OnGiveUpClicked);
        }

        private void ClearButtons()
        {
            if (reviveButton != null)
                reviveButton.onClick.RemoveAllListeners();

            if (giveUpButton != null)
                giveUpButton.onClick.RemoveAllListeners();
        }


        // ====== 표시/숨기기 ======

        /// <summary>
        /// 부활 프롬프트 표시
        /// </summary>
        /// <param name="remainingRevives">남은 부활 횟수</param>
        public void Show(int remainingRevives)
        {
            if (panelRoot == null) return;

            isShowing = true;
            fadeTimer = 0f;

            panelRoot.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            UpdateUI(remainingRevives);
        }

        /// <summary>
        /// 프롬프트 숨기기
        /// </summary>
        public void Hide()
        {
            isShowing = false;

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }


        // ====== UI 갱신 ======

        private void UpdateUI(int remainingRevives)
        {
            bool canRevive = remainingRevives > 0;

            // 타이틀
            if (titleText != null)
            {
                titleText.text = "사망";
            }

            // 설명
            if (descriptionText != null)
            {
                if (canRevive)
                {
                    descriptionText.text = "부활하여 계속 도전하시겠습니까?";
                }
                else
                {
                    descriptionText.text = "부활 횟수가 모두 소진되었습니다.";
                }
            }

            // 남은 횟수
            if (remainingText != null)
            {
                if (canRevive)
                {
                    remainingText.text = $"남은 부활: {remainingRevives}회";
                    remainingText.color = Color.green;
                }
                else
                {
                    remainingText.text = "남은 부활: 0회";
                    remainingText.color = Color.red;
                }
            }

            // 부활 버튼
            if (reviveButton != null)
            {
                reviveButton.interactable = canRevive;
            }

            if (reviveButtonText != null)
            {
                reviveButtonText.text = canRevive ? "부활" : "부활 불가";
                reviveButtonText.color = canRevive ? Color.white : Color.gray;
            }

            // 포기 버튼
            if (giveUpButtonText != null)
            {
                giveUpButtonText.text = "포기";
            }
        }


        // ====== 버튼 콜백 ======

        private void OnReviveClicked()
        {
            Hide();
            OnReviveSelected?.Invoke();
        }

        private void OnGiveUpClicked()
        {
            Hide();
            OnGiveUpSelected?.Invoke();
        }


        // ====== 외부 호출 ======

        /// <summary>
        /// 부활 시스템과 연동하여 프롬프트 표시
        /// </summary>
        public void ShowWithSystem()
        {
            var applier = GASPT.Meta.SpecialUpgradeApplier.Instance;
            if (applier != null)
            {
                Show(applier.RemainingRevives);
            }
            else
            {
                Show(0);
            }
        }
    }
}
