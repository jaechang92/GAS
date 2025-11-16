using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Stats;
using Core.Enums;

namespace GASPT.UI
{
    /// <summary>
    /// 플레이어 체력바 UI (Screen Space)
    /// PlayerStats의 HP 변화를 실시간으로 표시
    /// </summary>
    public class PlayerHealthBar : MonoBehaviour
    {
        // ====== UI 참조 ======

        [Header("UI References")]
        [SerializeField] [Tooltip("HP 슬라이더")]
        private Slider hpSlider;

        [SerializeField] [Tooltip("HP 텍스트 (예: 75/100)")]
        private TextMeshProUGUI hpText;

        [SerializeField] [Tooltip("슬라이더 Fill 이미지 (색상 변경용)")]
        private Image fillImage;


        // ====== PlayerStats 참조 ======

        [Header("Player Reference")]
        [SerializeField] [Tooltip("플레이어 스탯 (null이면 자동 검색)")]
        private PlayerStats playerStats;


        // ====== 시각 효과 설정 ======

        [Header("Visual Effects")]
        [SerializeField] [Tooltip("데미지 받을 때 색상")]
        private Color damageColor = new Color(1f, 0.3f, 0.3f); // 빨간색

        [SerializeField] [Tooltip("회복할 때 색상")]
        private Color healColor = new Color(0.3f, 1f, 0.3f); // 초록색

        [SerializeField] [Tooltip("정상 HP 색상")]
        private Color normalColor = new Color(0.2f, 0.8f, 0.2f); // 녹색

        [SerializeField] [Tooltip("저체력 색상 (30% 이하)")]
        private Color lowHPColor = new Color(1f, 0.5f, 0f); // 주황색

        [SerializeField] [Tooltip("위험 체력 색상 (10% 이하)")]
        private Color criticalHPColor = new Color(1f, 0.2f, 0.2f); // 빨간색

        [SerializeField] [Tooltip("색상 플래시 지속 시간")]
        private float flashDuration = 0.3f;


        // ====== 내부 상태 ======

        private Color currentNormalColor;
        private CancellationTokenSource flashCts;


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
            if (hpSlider == null)
            {
                Debug.LogError("[PlayerHealthBar] hpSlider가 설정되지 않았습니다.");
            }

            if (hpText == null)
            {
                Debug.LogError("[PlayerHealthBar] hpText가 설정되지 않았습니다.");
            }

            if (fillImage == null)
            {
                Debug.LogWarning("[PlayerHealthBar] fillImage가 설정되지 않았습니다. 색상 효과를 사용할 수 없습니다.");
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
                    Debug.LogError("[PlayerHealthBar] PlayerStats를 찾을 수 없습니다. Scene에 PlayerStats가 있는지 확인하세요.");
                }
                else
                {
                    Debug.Log("[PlayerHealthBar] PlayerStats 자동 검색 완료.");
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
                UpdateHPBar(playerStats.CurrentHP, playerStats.MaxHP);
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
                playerStats.OnDamaged += OnPlayerDamaged;
                playerStats.OnHealed += OnPlayerHealed;
                playerStats.OnDeath += OnPlayerDeath;
                playerStats.OnStatChanged += OnPlayerStatChanged;

                Debug.Log("[PlayerHealthBar] PlayerStats 이벤트 구독 완료");
            }
            else
            {
                Debug.LogWarning("[PlayerHealthBar] playerStats가 null이어서 이벤트를 구독할 수 없습니다.");
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (playerStats != null)
            {
                playerStats.OnDamaged -= OnPlayerDamaged;
                playerStats.OnHealed -= OnPlayerHealed;
                playerStats.OnDeath -= OnPlayerDeath;
                playerStats.OnStatChanged -= OnPlayerStatChanged;
            }
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 플레이어 스탯이 변경되었을 때 (MaxHP 변경 감지)
        /// </summary>
        private void OnPlayerStatChanged(StatType statType, int oldValue, int newValue)
        {
            // HP 스탯이 변경되었을 때만 처리 (MaxHP 변경)
            if (statType == StatType.HP)
            {
                if (playerStats != null)
                {
                    UpdateHPBar(playerStats.CurrentHP, playerStats.MaxHP);
                    Debug.Log($"[PlayerHealthBar] MaxHP 변경: {oldValue} → {newValue}");
                }
            }
        }

        /// <summary>
        /// 플레이어가 데미지를 받았을 때
        /// </summary>
        private void OnPlayerDamaged(int damage, int currentHP, int maxHP)
        {
            Debug.Log($"[PlayerHealthBar] OnPlayerDamaged 호출: damage={damage}, HP={currentHP}/{maxHP}");
            UpdateHPBar(currentHP, maxHP);

            // 빨간색 플래시 효과
            FlashColor(damageColor);
        }

        /// <summary>
        /// 플레이어가 체력을 회복했을 때
        /// </summary>
        private void OnPlayerHealed(int healAmount, int currentHP, int maxHP)
        {
            Debug.Log($"[PlayerHealthBar] OnPlayerHealed 호출: heal={healAmount}, HP={currentHP}/{maxHP}");
            UpdateHPBar(currentHP, maxHP);

            // 초록색 플래시 효과
            FlashColor(healColor);
        }

        /// <summary>
        /// 플레이어가 사망했을 때
        /// </summary>
        private void OnPlayerDeath()
        {
            UpdateHPBar(0, playerStats != null ? playerStats.MaxHP : 100);

            // 사망 시 빨간색으로 변경
            if (fillImage != null)
            {
                fillImage.color = criticalHPColor;
            }
        }


        // ====== HP 업데이트 ======

        /// <summary>
        /// HP 바 및 텍스트 업데이트
        /// </summary>
        private void UpdateHPBar(int currentHP, int maxHP)
        {
            Debug.Log($"[PlayerHealthBar] UpdateHPBar 호출: {currentHP}/{maxHP}");

            if (hpSlider != null)
            {
                float hpRatio = maxHP > 0 ? (float)currentHP / maxHP : 0f;
                hpSlider.value = hpRatio;
                Debug.Log($"[PlayerHealthBar] Slider value 설정: {hpRatio}");
            }
            else
            {
                Debug.LogWarning("[PlayerHealthBar] hpSlider가 null입니다!");
            }

            if (hpText != null)
            {
                hpText.text = $"{currentHP}/{maxHP}";
                Debug.Log($"[PlayerHealthBar] Text 설정: {currentHP}/{maxHP}");
            }
            else
            {
                Debug.LogWarning("[PlayerHealthBar] hpText가 null입니다!");
            }

            // HP 비율에 따라 색상 변경
            UpdateHPColor(currentHP, maxHP);
        }

        /// <summary>
        /// HP 비율에 따라 정상 색상 업데이트
        /// </summary>
        private void UpdateHPColor(int currentHP, int maxHP)
        {
            if (maxHP <= 0) return;

            float hpRatio = (float)currentHP / maxHP;

            // HP 비율에 따라 색상 결정
            if (hpRatio <= 0.1f)
            {
                currentNormalColor = criticalHPColor; // 10% 이하: 빨간색
            }
            else if (hpRatio <= 0.3f)
            {
                currentNormalColor = lowHPColor; // 30% 이하: 주황색
            }
            else
            {
                currentNormalColor = normalColor; // 30% 초과: 녹색
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
                UpdateHPBar(playerStats.CurrentHP, playerStats.MaxHP);
            }
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Update HP Bar (Test)")]
        private void TestUpdateHPBar()
        {
            if (playerStats != null)
            {
                UpdateHPBar(playerStats.CurrentHP, playerStats.MaxHP);
                Debug.Log($"[PlayerHealthBar] HP Bar 업데이트: {playerStats.CurrentHP}/{playerStats.MaxHP}");
            }
            else
            {
                Debug.LogWarning("[PlayerHealthBar] PlayerStats가 없습니다.");
            }
        }

        [ContextMenu("Flash Damage Color (Test)")]
        private void TestFlashDamage()
        {
            FlashColor(damageColor);
        }

        [ContextMenu("Flash Heal Color (Test)")]
        private void TestFlashHeal()
        {
            FlashColor(healColor);
        }
    }
}
