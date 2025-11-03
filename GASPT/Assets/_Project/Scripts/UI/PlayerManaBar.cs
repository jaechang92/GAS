using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Stats;

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
            FindPlayerStats();
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
        /// PlayerStats 자동 검색
        /// </summary>
        private void FindPlayerStats()
        {
            if (playerStats == null)
            {
                playerStats = FindAnyObjectByType<PlayerStats>();

                if (playerStats == null)
                {
                    Debug.LogError("[PlayerManaBar] PlayerStats를 찾을 수 없습니다. Scene에 PlayerStats가 있는지 확인하세요.");
                }
                else
                {
                    Debug.Log("[PlayerManaBar] PlayerStats 자동 검색 완료.");
                }
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
                playerStats.OnManaChanged += OnPlayerManaChanged;

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
                playerStats.OnManaChanged -= OnPlayerManaChanged;
            }
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 플레이어 마나가 변경되었을 때
        /// </summary>
        private void OnPlayerManaChanged(int currentMana, int maxMana)
        {
            Debug.Log($"[PlayerManaBar] OnManaChanged 호출: {lastMana} → {currentMana}/{maxMana}");
            UpdateManaBar(currentMana, maxMana);

            // 마나 소모 또는 회복 플래시
            if (currentMana < lastMana)
            {
                // 마나 소모
                FlashColor(spendColor);
            }
            else if (currentMana > lastMana)
            {
                // 마나 회복
                FlashColor(regenColor);
            }

            // 현재 마나를 lastMana에 저장
            lastMana = currentMana;
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
                await FlashColorAsync(flashColor, flashCts.Token);
            }
            catch (System.OperationCanceledException)
            {
                // 취소됨 - 정상적인 동작
            }
        }

        /// <summary>
        /// 색상 플래시 Awaitable
        /// </summary>
        private async Awaitable FlashColorAsync(Color flashColor, CancellationToken ct)
        {
            float elapsed = 0f;

            // 플래시 색상으로 변경
            fillImage.color = flashColor;

            // 점진적으로 원래 색상으로 복귀
            while (elapsed < flashDuration)
            {
                if (ct.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;

                fillImage.color = Color.Lerp(flashColor, currentNormalColor, t);

                await Awaitable.NextFrameAsync(ct);
            }

            // 최종 색상 설정
            fillImage.color = currentNormalColor;
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
