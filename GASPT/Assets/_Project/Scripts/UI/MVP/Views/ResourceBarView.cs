using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Stats;
using GASPT.Core.Extensions;

namespace GASPT.UI
{
    /// <summary>
    /// ResourceBar View (MVP 패턴)
    /// MonoBehaviour - 순수 렌더링 책임만 담당
    /// HP, Mana 등 모든 리소스 바에 재사용 가능
    /// </summary>
    public class ResourceBarView : MonoBehaviour, IResourceBarView
    {
        // ====== UI 참조 ======

        [Header("UI References")]
        [SerializeField] [Tooltip("리소스 슬라이더")]
        private Slider resourceSlider;

        [SerializeField] [Tooltip("리소스 텍스트 (예: 75/100)")]
        private TextMeshProUGUI resourceText;

        [SerializeField] [Tooltip("슬라이더 Fill 이미지 (색상 변경용)")]
        private Image fillImage;

        [Header("Configuration")]
        [SerializeField] [Tooltip("리소스 바 설정 (HP, Mana 등)")]
        private ResourceBarConfig config;

        // ====== Presenter ======

        private ResourceBarPresenter presenter;

        // ====== 내부 상태 ======

        private CancellationTokenSource flashCts;

        // ====== Unity 생명주기 ======

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            InitializePresenter();
            InitializeAsync().Forget();
        }

        private void OnEnable()
        {
            SubscribeToGameManagerEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromGameManagerEvents();
        }

        private void OnDestroy()
        {
            presenter?.Dispose();
            flashCts?.Cancel();
        }

        // ====== 초기화 ======

        /// <summary>
        /// UI 참조 유효성 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (resourceSlider == null)
            {
                Debug.LogError($"[ResourceBarView] resourceSlider가 설정되지 않았습니다! (Type: {config?.resourceType})");
            }

            if (resourceText == null)
            {
                Debug.LogError($"[ResourceBarView] resourceText가 설정되지 않았습니다! (Type: {config?.resourceType})");
            }

            if (fillImage == null)
            {
                Debug.LogWarning($"[ResourceBarView] fillImage가 설정되지 않았습니다. 색상 효과를 사용할 수 없습니다. (Type: {config?.resourceType})");
            }

            if (config == null)
            {
                Debug.LogError("[ResourceBarView] config가 설정되지 않았습니다! ResourceBarConfig를 할당해주세요.");
            }
        }

        /// <summary>
        /// Presenter 초기화
        /// </summary>
        private void InitializePresenter()
        {
            if (config == null)
            {
                Debug.LogError("[ResourceBarView] config가 null이어서 Presenter를 생성할 수 없습니다!");
                return;
            }

            presenter = new ResourceBarPresenter(this, config);
        }

        /// <summary>
        /// 비동기 초기화 (PlayerStats를 찾을 때까지 대기)
        /// </summary>
        private async Awaitable InitializeAsync()
        {
            PlayerStats stats = await FindPlayerStatsAsync();
            if (stats != null && presenter != null)
            {
                presenter.Initialize(stats);
            }
        }

        /// <summary>
        /// PlayerStats 자동 검색 (비동기 - 재시도 로직)
        /// </summary>
        private async Awaitable<PlayerStats> FindPlayerStatsAsync()
        {
            int maxAttempts = 50;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                // RunManager 우선
                if (GASPT.Core.RunManager.HasInstance && GASPT.Core.RunManager.Instance.CurrentPlayer != null)
                {
                    Debug.Log($"[ResourceBarView] RunManager에서 PlayerStats 찾기 성공! (Type: {config.resourceType})");
                    return GASPT.Core.RunManager.Instance.CurrentPlayer;
                }

                // GameManager 차선
                if (GASPT.Core.GameManager.HasInstance && GASPT.Core.GameManager.Instance.PlayerStats != null)
                {
                    Debug.Log($"[ResourceBarView] GameManager에서 PlayerStats 찾기 성공! (Type: {config.resourceType})");
                    return GASPT.Core.GameManager.Instance.PlayerStats;
                }

                await Awaitable.WaitForSecondsAsync(0.1f);
                attempts++;
            }

            Debug.LogError($"[ResourceBarView] PlayerStats를 찾을 수 없습니다. (타임아웃, Type: {config.resourceType})");
            return null;
        }

        // ====== GameManager 이벤트 구독 ======

        private void SubscribeToGameManagerEvents()
        {
            if (GASPT.Core.GameManager.HasInstance)
            {
                GASPT.Core.GameManager.Instance.OnPlayerRegistered += OnPlayerRegistered;
                GASPT.Core.GameManager.Instance.OnPlayerUnregistered += OnPlayerUnregistered;
            }
        }

        private void UnsubscribeFromGameManagerEvents()
        {
            if (GASPT.Core.GameManager.HasInstance)
            {
                GASPT.Core.GameManager.Instance.OnPlayerRegistered -= OnPlayerRegistered;
                GASPT.Core.GameManager.Instance.OnPlayerUnregistered -= OnPlayerUnregistered;
            }
        }

        /// <summary>
        /// Player 등록 시 호출 (씬 전환 후 Player 재생성)
        /// </summary>
        private void OnPlayerRegistered(PlayerStats player)
        {
            presenter?.SetPlayerStats(player);
            Debug.Log($"[ResourceBarView] Player 참조 갱신 완료 (이벤트): {player.name} (Type: {config.resourceType})");
        }

        /// <summary>
        /// Player 해제 시 호출 (씬 전환 전 Player 파괴)
        /// </summary>
        private void OnPlayerUnregistered()
        {
            presenter?.SetPlayerStats(null);
            Debug.Log($"[ResourceBarView] Player 참조 해제 (이벤트, Type: {config.resourceType})");
        }

        // ====== IResourceBarView 구현 ======

        /// <summary>
        /// 리소스 바 업데이트 (슬라이더 + 텍스트)
        /// </summary>
        public void UpdateResourceBar(ResourceBarViewModel viewModel)
        {
            if (viewModel == null)
            {
                Debug.LogWarning($"[ResourceBarView] viewModel이 null입니다! (Type: {config?.resourceType})");
                return;
            }

            // 슬라이더 업데이트
            if (resourceSlider != null)
            {
                resourceSlider.value = viewModel.Ratio;
            }

            // 텍스트 업데이트
            if (resourceText != null)
            {
                resourceText.text = viewModel.DisplayText;
            }

            // 색상 업데이트 (플래시 없이)
            if (fillImage != null)
            {
                fillImage.color = viewModel.BarColor;
            }

            Debug.Log($"[ResourceBarView] UpdateResourceBar: {viewModel}");
        }

        /// <summary>
        /// 색상 플래시 효과 (데미지, 회복 등)
        /// </summary>
        public void FlashColor(Color flashColor, Color normalColor, float duration)
        {
            if (fillImage == null) return;

            // 이전 플래시 중단
            flashCts?.Cancel();
            flashCts = new CancellationTokenSource();

            FlashColorAsync(flashColor, normalColor, duration, flashCts.Token).Forget();
        }

        /// <summary>
        /// 색상 플래시 비동기 처리
        /// </summary>
        private async Awaitable FlashColorAsync(Color flashColor, Color normalColor, float duration, CancellationToken ct)
        {
            try
            {
                await UIAnimationHelper.FlashColorAsync(fillImage, flashColor, normalColor, duration, ct);
            }
            catch (System.OperationCanceledException)
            {
                // 취소됨 - 정상적인 동작
            }
        }

        /// <summary>
        /// 바 색상 즉시 변경 (플래시 없이)
        /// </summary>
        public void SetBarColor(Color color)
        {
            if (fillImage != null)
            {
                fillImage.color = color;
            }
        }

        /// <summary>
        /// View 표시
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// View 숨김
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        // ====== Public 메서드 ======

        /// <summary>
        /// PlayerStats 참조 설정 (외부에서 설정 가능)
        /// </summary>
        public void SetPlayerStats(PlayerStats stats)
        {
            presenter?.SetPlayerStats(stats);
        }

        /// <summary>
        /// 강제 UI 갱신
        /// </summary>
        public void ForceRefresh()
        {
            presenter?.ForceRefresh();
        }

        // ====== Context Menu (테스트용) ======

        [ContextMenu("Force Refresh (Test)")]
        private void TestForceRefresh()
        {
            ForceRefresh();
        }

        [ContextMenu("Flash Decrease Color (Test)")]
        private void TestFlashDecrease()
        {
            if (config != null && fillImage != null)
            {
                FlashColor(config.decreaseFlashColor, config.normalColor, config.flashDuration);
            }
        }

        [ContextMenu("Flash Increase Color (Test)")]
        private void TestFlashIncrease()
        {
            if (config != null && fillImage != null)
            {
                FlashColor(config.increaseFlashColor, config.normalColor, config.flashDuration);
            }
        }

        [ContextMenu("Automatically reference variables")]
        private void AutoReferenceVariables()
        {
            if (resourceSlider == null)
            {
                resourceSlider = GetComponentInChildren<Slider>();
            }
            if (resourceText == null)
            {
                resourceText = GetComponentInChildren<TextMeshProUGUI>();
            }
            if (fillImage == null && resourceSlider != null)
            {
                fillImage = resourceSlider.fillRect?.GetComponent<Image>();
            }
        }
    }
}
