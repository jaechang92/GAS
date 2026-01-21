using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MVP_Core.Examples
{
    /// <summary>
    /// ResourceBar View 구현 예제
    /// MVP_Core를 사용한 HP/Mana 바 구현 방법 시연
    /// </summary>
    public class ExampleResourceBarView : ViewBase, IResourceBar
    {
        // ====== UI 참조 ======

        [Header("Resource Bar UI")]
        [SerializeField] private Slider slider;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI valueText;

        [Header("Configuration")]
        [SerializeField] private ResourceBarConfig config;

        // ====== 상태 ======

        private CancellationTokenSource flashCts;

        // ====== Unity 생명주기 ======

        protected override void Awake()
        {
            base.Awake();
            ValidateReferences();
        }

        private void OnDestroy()
        {
            flashCts?.Cancel();
        }

        // ====== 검증 ======

        private void ValidateReferences()
        {
            if (slider == null)
                Debug.LogError("[ExampleResourceBarView] Slider is not assigned!");

            if (fillImage == null)
                Debug.LogWarning("[ExampleResourceBarView] Fill Image is not assigned. Color effects disabled.");

            if (config == null)
                Debug.LogError("[ExampleResourceBarView] ResourceBarConfig is not assigned!");
        }

        // ====== IResourceBar 구현 ======

        public void UpdateBar(ResourceBarViewModel viewModel)
        {
            if (viewModel == null) return;

            // 슬라이더 업데이트
            if (slider != null)
            {
                slider.value = viewModel.Ratio;
            }

            // 텍스트 업데이트
            if (valueText != null)
            {
                valueText.text = viewModel.DisplayText;
            }

            // 색상 업데이트
            if (fillImage != null)
            {
                fillImage.color = viewModel.BarColor;
            }

            // 플래시 효과
            if (viewModel.IsDecreased && config != null)
            {
                FlashColor(config.decreaseFlashColor, viewModel.BarColor, config.flashDuration);
            }
            else if (viewModel.IsIncreased && config != null)
            {
                FlashColor(config.increaseFlashColor, viewModel.BarColor, config.flashDuration);
            }
        }

        public void FlashColor(Color flashColor, Color normalColor, float duration)
        {
            if (fillImage == null) return;

            flashCts?.Cancel();
            flashCts = new CancellationTokenSource();

            FlashColorAsync(flashColor, normalColor, duration, flashCts.Token);
        }

        public void SetBarColor(Color color)
        {
            if (fillImage != null)
            {
                fillImage.color = color;
            }
        }

        // ====== 비동기 처리 ======

        private async void FlashColorAsync(Color flashColor, Color normalColor, float duration, CancellationToken ct)
        {
            try
            {
                await UIAnimationHelper.FlashColorAsync(fillImage, flashColor, normalColor, duration, ct);
            }
            catch (System.OperationCanceledException)
            {
                // 정상 취소
            }
        }

        // ====== Context Menu (테스트용) ======

        [ContextMenu("Test Update (50%)")]
        private void TestUpdate()
        {
            if (config == null) return;

            var vm = new ResourceBarViewModel(
                50, 100,
                config.GetColorForRatio(0.5f),
                config.resourceType);

            UpdateBar(vm);
        }

        [ContextMenu("Test Flash Decrease")]
        private void TestFlashDecrease()
        {
            if (config != null)
            {
                FlashColor(config.decreaseFlashColor, config.normalColor, config.flashDuration);
            }
        }
    }
}
