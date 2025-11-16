using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Level;

namespace GASPT.UI
{
    /// <summary>
    /// 플레이어 경험치바 UI (Screen Space)
    /// PlayerLevel의 EXP 변화를 실시간으로 표시
    /// </summary>
    public class PlayerExpBar : MonoBehaviour
    {
        // ====== UI 참조 ======

        [Header("UI References")]
        [SerializeField] [Tooltip("EXP 슬라이더")]
        private Slider expSlider;

        [SerializeField] [Tooltip("EXP 텍스트 (예: 50/100)")]
        private TextMeshProUGUI expText;

        [SerializeField] [Tooltip("레벨 텍스트 (예: Lv.5)")]
        private TextMeshProUGUI levelText;

        [SerializeField] [Tooltip("슬라이더 Fill 이미지 (색상 변경용)")]
        private Image fillImage;


        // ====== PlayerLevel 참조 ======

        [Header("Player Reference")]
        [SerializeField] [Tooltip("플레이어 레벨 (null이면 자동 검색)")]
        private PlayerLevel playerLevel;


        // ====== 시각 효과 설정 ======

        [Header("Visual Effects")]
        [SerializeField] [Tooltip("EXP 획득 시 색상")]
        private Color expGainColor = new Color(1f, 0.8f, 0.2f); // 골드색

        [SerializeField] [Tooltip("정상 EXP 색상")]
        private Color normalColor = new Color(0.2f, 0.6f, 1f); // 파란색

        [SerializeField] [Tooltip("레벨업 시 색상")]
        private Color levelUpColor = new Color(1f, 1f, 0.2f); // 노란색

        [SerializeField] [Tooltip("색상 플래시 지속 시간")]
        private float flashDuration = 0.3f;

        [SerializeField] [Tooltip("레벨업 애니메이션 지속 시간")]
        private float levelUpAnimationDuration = 1f;


        // ====== 내부 상태 ======

        private CancellationTokenSource flashCts;
        private CancellationTokenSource levelUpCts;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            FindPlayerLevel();
            SubscribeToEvents(); // PlayerLevel을 찾은 후 이벤트 구독
            InitializeUI();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            flashCts?.Cancel(); // 플래시 중단
            levelUpCts?.Cancel(); // 레벨업 애니메이션 중단
        }


        // ====== 초기화 ======

        /// <summary>
        /// UI 참조 유효성 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (expSlider == null)
            {
                Debug.LogError("[PlayerExpBar] expSlider가 설정되지 않았습니다.");
            }

            if (expText == null)
            {
                Debug.LogError("[PlayerExpBar] expText가 설정되지 않았습니다.");
            }

            if (levelText == null)
            {
                Debug.LogError("[PlayerExpBar] levelText가 설정되지 않았습니다.");
            }

            if (fillImage == null)
            {
                Debug.LogWarning("[PlayerExpBar] fillImage가 설정되지 않았습니다. 색상 효과를 사용할 수 없습니다.");
            }
        }

        /// <summary>
        /// PlayerLevel 자동 검색
        /// </summary>
        private void FindPlayerLevel()
        {
            if (playerLevel == null)
            {
                playerLevel = PlayerLevel.Instance;

                if (playerLevel == null)
                {
                    Debug.LogError("[PlayerExpBar] PlayerLevel을 찾을 수 없습니다. Scene에 PlayerLevel이 있는지 확인하세요.");
                }
                else
                {
                    Debug.Log("[PlayerExpBar] PlayerLevel 자동 검색 완료.");
                }
            }
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            if (playerLevel != null)
            {
                UpdateExpBar(playerLevel.CurrentExp, playerLevel.RequiredExp);
                UpdateLevelText(playerLevel.Level);
            }

            if (fillImage != null)
            {
                fillImage.color = normalColor;
            }
        }


        // ====== 이벤트 구독 ======

        private void SubscribeToEvents()
        {
            if (playerLevel != null)
            {
                playerLevel.OnExpGained += OnExpGained;
                playerLevel.OnLevelUp += OnLevelUp;

                Debug.Log("[PlayerExpBar] PlayerLevel 이벤트 구독 완료");
            }
            else
            {
                Debug.LogWarning("[PlayerExpBar] playerLevel이 null이어서 이벤트를 구독할 수 없습니다.");
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (playerLevel != null)
            {
                playerLevel.OnExpGained -= OnExpGained;
                playerLevel.OnLevelUp -= OnLevelUp;
            }
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 경험치 획득 시
        /// </summary>
        private void OnExpGained(int gainedExp, int currentExp, int requiredExp)
        {
            Debug.Log($"[PlayerExpBar] OnExpGained 호출: +{gainedExp} EXP ({currentExp}/{requiredExp})");
            UpdateExpBar(currentExp, requiredExp);

            // 골드색 플래시 효과
            FlashColor(expGainColor);
        }

        /// <summary>
        /// 레벨업 시
        /// </summary>
        private void OnLevelUp(int newLevel)
        {
            Debug.Log($"[PlayerExpBar] OnLevelUp 호출: Level {newLevel}");
            UpdateLevelText(newLevel);

            // EXP 바 업데이트 (레벨업 후 남은 경험치 반영)
            if (playerLevel != null)
            {
                UpdateExpBar(playerLevel.CurrentExp, playerLevel.RequiredExp);
            }

            // 레벨업 애니메이션
            PlayLevelUpAnimation();
        }


        // ====== EXP 업데이트 ======

        /// <summary>
        /// EXP 바 및 텍스트 업데이트
        /// </summary>
        private void UpdateExpBar(int currentExp, int requiredExp)
        {
            Debug.Log($"[PlayerExpBar] UpdateExpBar 호출: {currentExp}/{requiredExp}");

            if (expSlider != null)
            {
                float expRatio = requiredExp > 0 ? (float)currentExp / requiredExp : 0f;
                expSlider.value = expRatio;
                Debug.Log($"[PlayerExpBar] Slider value 설정: {expRatio}");
            }
            else
            {
                Debug.LogWarning("[PlayerExpBar] expSlider가 null입니다!");
            }

            if (expText != null)
            {
                expText.text = $"{currentExp}/{requiredExp}";
                Debug.Log($"[PlayerExpBar] EXP Text 설정: {currentExp}/{requiredExp}");
            }
            else
            {
                Debug.LogWarning("[PlayerExpBar] expText가 null입니다!");
            }
        }

        /// <summary>
        /// 레벨 텍스트 업데이트
        /// </summary>
        private void UpdateLevelText(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv.{level}";
                Debug.Log($"[PlayerExpBar] Level Text 설정: Lv.{level}");
            }
            else
            {
                Debug.LogWarning("[PlayerExpBar] levelText가 null입니다!");
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
                    normalColor,
                    flashDuration,
                    flashCts.Token
                );
            }
            catch (System.OperationCanceledException)
            {
                // 취소됨 - 정상적인 동작
            }
        }


        /// <summary>
        /// 레벨업 애니메이션
        /// </summary>
        private async void PlayLevelUpAnimation()
        {
            levelUpCts?.Cancel();
            levelUpCts = new CancellationTokenSource();

            try
            {
                await LevelUpAnimationAsync(levelUpCts.Token);
            }
            catch (System.OperationCanceledException)
            {
                // 취소됨 - 정상적인 동작
            }
        }

        /// <summary>
        /// 레벨업 애니메이션 Awaitable
        /// </summary>
        private async Awaitable LevelUpAnimationAsync(CancellationToken ct)
        {
            if (fillImage == null || levelText == null) return;

            // 원래 스케일 저장
            Vector3 originalScale = levelText.transform.localScale;

            float elapsed = 0f;

            while (elapsed < levelUpAnimationDuration)
            {
                if (ct.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float t = elapsed / levelUpAnimationDuration;

                // 레벨 텍스트 크기 애니메이션 (1.0 → 1.5 → 1.0)
                float scaleMultiplier = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f;
                levelText.transform.localScale = originalScale * scaleMultiplier;

                // Fill 색상 애니메이션 (노란색 → 파란색)
                fillImage.color = Color.Lerp(levelUpColor, normalColor, t);

                await Awaitable.NextFrameAsync(ct);
            }

            // 최종 상태 설정
            levelText.transform.localScale = originalScale;
            fillImage.color = normalColor;
        }


        // ====== Public 메서드 ======

        /// <summary>
        /// PlayerLevel 참조 설정 (외부에서 설정 가능)
        /// </summary>
        public void SetPlayerLevel(PlayerLevel level)
        {
            // 기존 이벤트 구독 해제
            UnsubscribeFromEvents();

            playerLevel = level;

            // 새 이벤트 구독
            SubscribeToEvents();

            // UI 업데이트
            if (playerLevel != null)
            {
                UpdateExpBar(playerLevel.CurrentExp, playerLevel.RequiredExp);
                UpdateLevelText(playerLevel.Level);
            }
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Update EXP Bar (Test)")]
        private void TestUpdateExpBar()
        {
            if (playerLevel != null)
            {
                UpdateExpBar(playerLevel.CurrentExp, playerLevel.RequiredExp);
                UpdateLevelText(playerLevel.Level);
                Debug.Log($"[PlayerExpBar] EXP Bar 업데이트: {playerLevel.CurrentExp}/{playerLevel.RequiredExp}, Level {playerLevel.Level}");
            }
            else
            {
                Debug.LogWarning("[PlayerExpBar] PlayerLevel이 없습니다.");
            }
        }

        [ContextMenu("Flash Exp Gain Color (Test)")]
        private void TestFlashExpGain()
        {
            FlashColor(expGainColor);
        }

        [ContextMenu("Play Level Up Animation (Test)")]
        private void TestLevelUpAnimation()
        {
            PlayLevelUpAnimation();
        }
    }
}
