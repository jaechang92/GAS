using UnityEngine;
using UnityEngine.UI;
using UI.Core;

namespace UI.Panels
{
    /// <summary>
    /// 로딩 화면 Panel
    /// 로딩 진행률 표시 및 팁 표시
    /// </summary>
    public class LoadingPanel : BasePanel
    {
        [Header("UI 참조")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private Text progressText;
        [SerializeField] private Text loadingTipText;
        [SerializeField] private Text loadingText;

        [Header("로딩 팁")]
        [SerializeField] private string[] loadingTips = new string[]
        {
            "스페이스바를 눌러 점프할 수 있습니다.",
            "연속으로 공격하면 콤보가 발동됩니다!",
            "적의 공격 패턴을 관찰하세요.",
            "대시로 적의 공격을 회피하세요!",
            "체력이 부족하면 물약을 사용하세요."
        };

        protected override void Awake()
        {
            base.Awake();

            // Panel 설정
            panelType = PanelType.Loading;
            layer = UILayer.System;
            openTransition = TransitionType.Fade;
            closeTransition = TransitionType.Fade;
            transitionDuration = 0.3f;

            // Panel 이벤트 구독
            OnOpened += OnPanelOpened;
        }

        private void OnPanelOpened(BasePanel panel)
        {
            Debug.Log("[LoadingPanel] 로딩 화면 표시");

            // 랜덤 팁 표시
            ShowRandomTip();

            // 진행률 초기화
            UpdateProgress(0f);
        }

        /// <summary>
        /// 진행률 업데이트
        /// </summary>
        public override void UpdateProgress(float progress)
        {
            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            if (progressText != null)
            {
                progressText.text = $"{(progress * 100):F0}%";
            }
        }

        /// <summary>
        /// 랜덤 팁 표시
        /// </summary>
        private void ShowRandomTip()
        {
            if (loadingTipText != null && loadingTips.Length > 0)
            {
                int randomIndex = Random.Range(0, loadingTips.Length);
                loadingTipText.text = $"TIP: {loadingTips[randomIndex]}";
            }
        }

        private void OnDestroy()
        {
            // Panel 이벤트 구독 해제
            OnOpened -= OnPanelOpened;
        }
    }
}
