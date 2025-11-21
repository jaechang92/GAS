using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Stats;
using Core.Enums;
using Core;

namespace GASPT.UI
{
    /// <summary>
    /// 플레이어 마나바 UI (Screen Space)
    /// PlayerStats의 Mana 변화를 실시간으로 표시
    /// </summary>
    public class PlayerManaBar : MonoBehaviour
    {
        // ====== UI 참조 ======

        [Header("UI References")]
        [SerializeField] [Tooltip("마나 슬라이더")]
        private Slider manaSlider;

        [SerializeField] [Tooltip("마나 텍스트 (예: 50/100)")]
        private TextMeshProUGUI manaText;

        [SerializeField] [Tooltip("슬라이더 Fill 이미지 (색상 변경용)")]
        private Image fillImage;


        // ====== PlayerStats 참조 ======

        [Header("Player Reference")]
        [SerializeField] [Tooltip("플레이어 스탯 (null이면 자동 검색)")]
        private PlayerStats playerStats;


        // ====== 시각 효과 설정 ======

        [Header("Visual Effects")]
        [SerializeField] [Tooltip("마나 소모 시 색상")]
        private Color spendColor = new Color(1f, 0.3f, 0.3f); // 빨간색

        [SerializeField] [Tooltip("마나 회복 시 색상")]
        private Color regenColor = new Color(0.3f, 0.8f, 1f); // 밝은 파란색

        [SerializeField] [Tooltip("정상 마나 색상")]
        private Color normalColor = new Color(0.2f, 0.4f, 1f); // 파란색

        [SerializeField] [Tooltip("저마나 색상 (20% 이하)")]
        private Color lowManaColor = new Color(1f, 0.5f, 0f); // 주황색

        [SerializeField] [Tooltip("색상 플래시 지속 시간")]
        private float flashDuration = 0.3f;


        // ====== 내부 상태 ======

        private Color currentNormalColor;
        private CancellationTokenSource flashCts;
        private int lastMana; // 이전 마나 값 (플래시 효과 판단용)


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            // 비동기 초기화
            InitializeAsync().Forget();
        }

        /// <summary>
        /// 비동기 초기화 (PlayerStats를 찾을 때까지 대기)
        /// </summary>
        private async Awaitable InitializeAsync()
        {
            await FindPlayerStatsAsync();
            SubscribeToEvents(); // PlayerStats를 찾은 후 이벤트 구독
            InitializeUI();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            flashCts?.Cancel(); // 플래시 중단
        }


        // ====== 초기화 ======

        /// <summary>
        /// UI 참조 유효성 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (manaSlider == null)
            {
                Debug.LogError("[PlayerManaBar] manaSlider가 설정되지 않았습니다.");
            }

            if (manaText == null)
            {
                Debug.LogError("[PlayerManaBar] manaText가 설정되지 않았습니다.");
            }

            if (fillImage == null)
            {
                Debug.LogWarning("[PlayerManaBar] fillImage가 설정되지 않았습니다. 색상 효과를 사용할 수 없습니다.");
            }
        }

        /// <summary>
        /// PlayerStats 자동 검색 (동기) - RunManager 우선 사용
        /// </summary>
        private void FindPlayerStats()
        {
            if (playerStats == null)
            {
                // RunManager에서 먼저 찾기
                if (GASPT.Core.RunManager.HasInstance && GASPT.Core.RunManager.Instance.CurrentPlayer != null)
                {
                    playerStats = GASPT.Core.RunManager.Instance.CurrentPlayer;
                    Debug.Log("[PlayerManaBar] RunManager에서 PlayerStats 찾기 완료.");
                    return;
                }

                // GameManager에서 찾기
                if (GASPT.Core.GameManager.HasInstance && GASPT.Core.GameManager.Instance.PlayerStats != null)
                {
                    playerStats = GASPT.Core.GameManager.Instance.PlayerStats;
                    Debug.Log("[PlayerManaBar] GameManager에서 PlayerStats 찾기 완료.");
                    return;
                }

                Debug.LogError("[PlayerManaBar] PlayerStats를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// PlayerStats 자동 검색 (비동기 - 재시도 로직)
        /// </summary>
        private async Awaitable FindPlayerStatsAsync()
        {
            int maxAttempts = 50;
            int attempts = 0;

            while (playerStats == null && attempts < maxAttempts)
            {
                // RunManager 우선
                if (GASPT.Core.RunManager.HasInstance && GASPT.Core.RunManager.Instance.CurrentPlayer != null)
                {
                    playerStats = GASPT.Core.RunManager.Instance.CurrentPlayer;
                }
                // GameManager 차선
                else if (GASPT.Core.GameManager.HasInstance && GASPT.Core.GameManager.Instance.PlayerStats != null)
                {
                    playerStats = GASPT.Core.GameManager.Instance.PlayerStats;
                }

                if (playerStats != null)
                {
                    Debug.Log("[PlayerManaBar] PlayerStats 찾기 성공!");
                    break;
                }

                await Awaitable.WaitForSecondsAsync(0.1f);
                attempts++;
            }

            if (playerStats == null)
            {
                Debug.LogError("[PlayerManaBar] PlayerStats를 찾을 수 없습니다. (타임아웃)");
            }
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            if (playerStats != null)
            {
                lastMana = playerStats.CurrentMana;
                UpdateManaBar(playerStats.CurrentMana, playerStats.MaxMana);
            }

            currentNormalColor = normalColor;

            if (fillImage != null)
            {
                fillImage.color = currentNormalColor;
            }
        }


        // ====== 이벤트 구독 ======

        private void SubscribeToEvents()
        {
            if (playerStats != null)
            {
                playerStats.OnStatsChanged += OnPlayerStatsChanged;

                Debug.Log("[PlayerManaBar] PlayerStats 이벤트 구독 완료");
            }
            else
            {
                Debug.LogWarning("[PlayerManaBar] playerStats가 null이어서 이벤트를 구독할 수 없습니다.");
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (playerStats != null)
            {
                playerStats.OnStatsChanged -= OnPlayerStatsChanged;
            }
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 플레이어 스탯이 변경되었을 때 (Mana만 처리)
        /// </summary>
        private void OnPlayerStatsChanged(StatType statType, int oldValue, int newValue)
        {
            // Mana 변경만 처리
            if (statType != StatType.Mana)
                return;

            Debug.Log($"[PlayerManaBar] OnStatsChanged(Mana) 호출: {oldValue} → {newValue}");
            UpdateManaBar(newValue, playerStats.MaxMana);

            // 마나 소모 또는 회복 플래시
            if (newValue < oldValue)
            {
                // 마나 소모
                FlashColor(spendColor);
            }
            else if (newValue > oldValue)
            {
                // 마나 회복
                FlashColor(regenColor);
            }

            // 현재 마나를 lastMana에 저장
            lastMana = newValue;
        }


        // ====== 마나 업데이트 ======

        /// <summary>
        /// 마나 바 및 텍스트 업데이트
        /// </summary>
        private void UpdateManaBar(int currentMana, int maxMana)
        {
            Debug.Log($"[PlayerManaBar] UpdateManaBar 호출: {currentMana}/{maxMana}");

            if (manaSlider != null)
            {
                float manaRatio = maxMana > 0 ? (float)currentMana / maxMana : 0f;
                manaSlider.value = manaRatio;
                Debug.Log($"[PlayerManaBar] Slider value 설정: {manaRatio}");
            }
            else
            {
                Debug.LogWarning("[PlayerManaBar] manaSlider가 null입니다!");
            }

            if (manaText != null)
            {
                manaText.text = $"{currentMana}/{maxMana}";
                Debug.Log($"[PlayerManaBar] Text 설정: {currentMana}/{maxMana}");
            }
            else
            {
                Debug.LogWarning("[PlayerManaBar] manaText가 null입니다!");
            }

            // 마나 비율에 따라 색상 변경
            UpdateManaColor(currentMana, maxMana);
        }

        /// <summary>
        /// 마나 비율에 따라 정상 색상 업데이트
        /// </summary>
        private void UpdateManaColor(int currentMana, int maxMana)
        {
            if (maxMana <= 0) return;

            float manaRatio = (float)currentMana / maxMana;

            // 마나 비율에 따라 색상 결정
            if (manaRatio <= 0.2f)
            {
                currentNormalColor = lowManaColor; // 20% 이하: 주황색 (경고)
            }
            else
            {
                currentNormalColor = normalColor; // 20% 초과: 파란색
            }
        }


        // ====== 시각 효과 (Awaitable 사용) ======

        /// <summary>
        /// 색상 플래시 효과
        /// </summary>
        private async void FlashColor(Color flashColor)
        {
            if (fillImage == null) return;

            // 이전 플래시 중단
            flashCts?.Cancel();
            flashCts = new CancellationTokenSource();

            try
            {
                await UIAnimationHelper.FlashColorAsync(
                    fillImage,
                    flashColor,
                    currentNormalColor,
                    flashDuration,
                    flashCts.Token
                );
            }
            catch (System.OperationCanceledException)
            {
                // 취소됨 - 정상적인 동작
            }
        }


        // ====== Public 메서드 ======

        /// <summary>
        /// PlayerStats 참조 설정 (외부에서 설정 가능)
        /// </summary>
        public void SetPlayerStats(PlayerStats stats)
        {
            // 기존 이벤트 구독 해제
            UnsubscribeFromEvents();

            playerStats = stats;

            // 새 이벤트 구독
            SubscribeToEvents();

            // UI 업데이트
            if (playerStats != null)
            {
                lastMana = playerStats.CurrentMana;
                UpdateManaBar(playerStats.CurrentMana, playerStats.MaxMana);
            }
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Update Mana Bar (Test)")]
        private void TestUpdateManaBar()
        {
            if (playerStats != null)
            {
                UpdateManaBar(playerStats.CurrentMana, playerStats.MaxMana);
                Debug.Log($"[PlayerManaBar] Mana Bar 업데이트: {playerStats.CurrentMana}/{playerStats.MaxMana}");
            }
            else
            {
                Debug.LogWarning("[PlayerManaBar] PlayerStats가 없습니다.");
            }
        }

        [ContextMenu("Flash Spend Color (Test)")]
        private void TestFlashSpend()
        {
            FlashColor(spendColor);
        }

        [ContextMenu("Flash Regen Color (Test)")]
        private void TestFlashRegen()
        {
            FlashColor(regenColor);
        }
    }
}
