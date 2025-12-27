using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// 보스 체력바 View 구현
    /// 화면 상단에 보스 체력, 이름, 페이즈 표시
    /// </summary>
    public class BossHealthBarView : MonoBehaviour, IBossHealthBarView
    {
        // ====== UI 참조 ======

        [Header("메인 패널")]
        [SerializeField]
        private GameObject mainPanel;

        [SerializeField]
        private CanvasGroup canvasGroup;


        [Header("보스 이름")]
        [SerializeField]
        private TextMeshProUGUI bossNameText;


        [Header("체력바")]
        [SerializeField]
        private Image healthBarFill;

        [SerializeField]
        private Image healthBarBackground;

        [SerializeField]
        private Image healthBarDamageIndicator;

        [SerializeField]
        private TextMeshProUGUI healthPercentText;


        [Header("페이즈")]
        [SerializeField]
        private TextMeshProUGUI phaseText;

        [SerializeField]
        private Transform phaseIconContainer;

        [SerializeField]
        private GameObject phaseIconPrefab;


        [Header("무적 오버레이")]
        [SerializeField]
        private GameObject invulnerabilityOverlay;


        [Header("시간 제한")]
        [SerializeField]
        private GameObject timeLimitPanel;

        [SerializeField]
        private TextMeshProUGUI timeLimitText;


        [Header("설정")]
        [SerializeField]
        private float damageIndicatorDelay = 0.5f;

        [SerializeField]
        private float damageIndicatorSpeed = 2f;


        // ====== 페이즈별 색상 ======

        [Header("페이즈별 색상")]
        [SerializeField]
        private Color phase1Color = new Color(0.3f, 0.8f, 0.3f); // 녹색

        [SerializeField]
        private Color phase2Color = new Color(1f, 0.6f, 0f);     // 주황색

        [SerializeField]
        private Color phase3Color = new Color(0.9f, 0.2f, 0.2f); // 빨간색

        [SerializeField]
        private Color phase4Color = new Color(0.6f, 0.1f, 0.6f); // 보라색

        [SerializeField]
        private Color invulnerableColor = new Color(0.6f, 0.2f, 0.8f); // 보라색


        // ====== 상태 ======

        private float currentHealthRatio = 1f;
        private float targetHealthRatio = 1f;
        private float damageIndicatorRatio = 1f;
        private bool isAnimating = false;
        private List<GameObject> phaseIcons = new List<GameObject>();
        private int currentPhase = 1;
        private int totalPhases = 1;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            Hide();
        }

        private void Update()
        {
            UpdateDamageIndicator();
        }


        // ====== IBossHealthBarView 구현 ======

        public void Show(string bossName, int totalPhases)
        {
            if (mainPanel != null)
                mainPanel.SetActive(true);

            this.totalPhases = totalPhases;
            currentPhase = 1;
            currentHealthRatio = 1f;
            targetHealthRatio = 1f;
            damageIndicatorRatio = 1f;

            SetBossName(bossName);
            UpdateHealth(1f);
            UpdatePhase(1, totalPhases);
            SetInvulnerable(false);

            // 페이즈 아이콘 생성
            CreatePhaseIcons(totalPhases);

            // 페이드 인
            FadeIn();

            Debug.Log($"[BossHealthBarView] 표시: {bossName} (페이즈: {totalPhases})");
        }

        public void Hide()
        {
            if (mainPanel != null)
                mainPanel.SetActive(false);

            ClearPhaseIcons();
        }

        public void UpdateHealth(float currentRatio)
        {
            targetHealthRatio = Mathf.Clamp01(currentRatio);

            if (healthBarFill != null)
                healthBarFill.fillAmount = targetHealthRatio;

            currentHealthRatio = targetHealthRatio;

            UpdateHealthPercentText();
        }

        public void UpdateHealthAnimated(float currentRatio, float animationDuration = 0.3f)
        {
            targetHealthRatio = Mathf.Clamp01(currentRatio);

            if (!isAnimating)
            {
                AnimateHealthBar(animationDuration);
            }

            UpdateHealthPercentText();
        }

        public void UpdatePhase(int currentPhase, int totalPhases)
        {
            this.currentPhase = currentPhase;
            this.totalPhases = totalPhases;

            if (phaseText != null)
                phaseText.text = $"Phase {currentPhase}/{totalPhases}";

            UpdatePhaseIcons(currentPhase);
            UpdateHealthBarColor(currentPhase);
        }

        public void SetInvulnerable(bool invulnerable)
        {
            if (invulnerabilityOverlay != null)
                invulnerabilityOverlay.SetActive(invulnerable);

            if (invulnerable && healthBarFill != null)
            {
                healthBarFill.color = invulnerableColor;
            }
            else
            {
                UpdateHealthBarColor(currentPhase);
            }
        }

        public void SetHealthBarColor(Color color)
        {
            if (healthBarFill != null)
                healthBarFill.color = color;
        }

        public void SetBossName(string name)
        {
            if (bossNameText != null)
                bossNameText.text = name;
        }

        public void PlayPhaseTransitionEffect(int newPhase)
        {
            // 페이즈 전환 연출
            // 플래시 효과
            FlashEffect();

            Debug.Log($"[BossHealthBarView] 페이즈 전환 연출: Phase {newPhase}");
        }

        public void UpdateTimeLimit(float remainingTime, bool show = true)
        {
            if (timeLimitPanel != null)
                timeLimitPanel.SetActive(show && remainingTime > 0);

            if (timeLimitText != null && show)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                timeLimitText.text = $"{minutes:00}:{seconds:00}";

                // 30초 이하일 때 빨간색
                timeLimitText.color = remainingTime <= 30f ? Color.red : Color.white;
            }
        }


        // ====== 체력바 애니메이션 ======

        private async void AnimateHealthBar(float duration)
        {
            isAnimating = true;

            float startRatio = currentHealthRatio;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                currentHealthRatio = Mathf.Lerp(startRatio, targetHealthRatio, t);

                if (healthBarFill != null)
                    healthBarFill.fillAmount = currentHealthRatio;

                await Awaitable.NextFrameAsync();
            }

            currentHealthRatio = targetHealthRatio;

            if (healthBarFill != null)
                healthBarFill.fillAmount = currentHealthRatio;

            isAnimating = false;
        }


        // ====== 데미지 인디케이터 ======

        private void UpdateDamageIndicator()
        {
            if (healthBarDamageIndicator == null) return;

            // 딜레이 후 현재 체력으로 천천히 감소
            if (damageIndicatorRatio > currentHealthRatio)
            {
                damageIndicatorRatio = Mathf.MoveTowards(
                    damageIndicatorRatio,
                    currentHealthRatio,
                    damageIndicatorSpeed * Time.deltaTime
                );

                healthBarDamageIndicator.fillAmount = damageIndicatorRatio;
            }
        }


        // ====== 페이즈 아이콘 ======

        private void CreatePhaseIcons(int count)
        {
            ClearPhaseIcons();

            if (phaseIconContainer == null || phaseIconPrefab == null)
                return;

            for (int i = 0; i < count; i++)
            {
                GameObject icon = Instantiate(phaseIconPrefab, phaseIconContainer);
                phaseIcons.Add(icon);
            }

            UpdatePhaseIcons(1);
        }

        private void UpdatePhaseIcons(int currentPhase)
        {
            for (int i = 0; i < phaseIcons.Count; i++)
            {
                var icon = phaseIcons[i];
                if (icon == null) continue;

                var image = icon.GetComponent<Image>();
                if (image != null)
                {
                    // 현재 페이즈 이하는 채워진 아이콘, 이상은 빈 아이콘
                    image.color = i < currentPhase
                        ? Color.white
                        : new Color(1f, 1f, 1f, 0.3f);
                }
            }
        }

        private void ClearPhaseIcons()
        {
            foreach (var icon in phaseIcons)
            {
                if (icon != null)
                    Destroy(icon);
            }

            phaseIcons.Clear();
        }


        // ====== 색상 업데이트 ======

        private void UpdateHealthBarColor(int phase)
        {
            Color color = phase switch
            {
                1 => phase1Color,
                2 => phase2Color,
                3 => phase3Color,
                4 => phase4Color,
                _ => phase1Color
            };

            SetHealthBarColor(color);
        }


        // ====== 퍼센트 텍스트 ======

        private void UpdateHealthPercentText()
        {
            if (healthPercentText != null)
            {
                int percent = Mathf.RoundToInt(targetHealthRatio * 100f);
                healthPercentText.text = $"{percent}%";
            }
        }


        // ====== 효과 ======

        private async void FadeIn()
        {
            if (canvasGroup == null) return;

            canvasGroup.alpha = 0f;
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = elapsed / duration;
                await Awaitable.NextFrameAsync();
            }

            canvasGroup.alpha = 1f;
        }

        private async void FlashEffect()
        {
            if (canvasGroup == null) return;

            // 빠른 깜빡임
            for (int i = 0; i < 3; i++)
            {
                canvasGroup.alpha = 0.5f;
                await Awaitable.WaitForSecondsAsync(0.05f);
                canvasGroup.alpha = 1f;
                await Awaitable.WaitForSecondsAsync(0.05f);
            }
        }
    }
}
