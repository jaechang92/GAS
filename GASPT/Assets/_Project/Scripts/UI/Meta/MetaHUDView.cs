using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Meta;

namespace GASPT.UI.Meta
{
    /// <summary>
    /// 메타 재화 HUD 표시
    /// 런 진행 중 임시 재화 (Bone/Soul) 표시
    /// 로비에서는 영구 재화 표시
    /// </summary>
    public class MetaHUDView : MonoBehaviour
    {
        // ====== UI 요소 ======

        [Header("Bone 표시")]
        [Tooltip("Bone 수량 텍스트 (TextMeshPro)")]
        [SerializeField] private TextMeshProUGUI boneText;

        [Tooltip("Bone 아이콘")]
        [SerializeField] private Image boneIcon;

        [Tooltip("Bone 컨테이너 (전체 표시/숨김용)")]
        [SerializeField] private GameObject boneContainer;


        [Header("Soul 표시")]
        [Tooltip("Soul 수량 텍스트 (TextMeshPro)")]
        [SerializeField] private TextMeshProUGUI soulText;

        [Tooltip("Soul 아이콘")]
        [SerializeField] private Image soulIcon;

        [Tooltip("Soul 컨테이너 (전체 표시/숨김용)")]
        [SerializeField] private GameObject soulContainer;


        [Header("추가 정보 (선택적)")]
        [Tooltip("골드 표시 텍스트")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Tooltip("'+' 접두사 표시 (임시 재화일 때)")]
        [SerializeField] private bool showPlusPrefix = true;


        // ====== 설정 ======

        [Header("동작 설정")]
        [Tooltip("Soul은 획득 시에만 표시")]
        [SerializeField] private bool showSoulOnlyWhenEarned = true;

        [Tooltip("재화 변경 시 펄스 효과")]
        [SerializeField] private bool enablePulseEffect = true;

        [Tooltip("펄스 효과 스케일")]
        [SerializeField] private float pulseScale = 1.2f;

        [Tooltip("펄스 효과 지속 시간")]
        [SerializeField] private float pulseDuration = 0.15f;

        [Tooltip("숫자 포맷 (천 단위 구분)")]
        [SerializeField] private bool useNumberFormat = true;


        // ====== 런타임 ======

        private int lastBone;
        private int lastSoul;
        private int lastGold;
        private RectTransform boneRect;
        private RectTransform soulRect;
        private RectTransform goldRect;
        private Vector3 boneOriginalScale;
        private Vector3 soulOriginalScale;
        private Vector3 goldOriginalScale;

        // 표시 모드
        private bool isShowingTempCurrency = true;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 표시 중인 Bone 수량
        /// </summary>
        public int DisplayedBone => lastBone;

        /// <summary>
        /// 현재 표시 중인 Soul 수량
        /// </summary>
        public int DisplayedSoul => lastSoul;

        /// <summary>
        /// 임시 재화 표시 모드인지 여부
        /// </summary>
        public bool IsShowingTempCurrency => isShowingTempCurrency;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            CacheRectTransforms();
        }

        private void Start()
        {
            SubscribeToEvents();
            UpdateDisplay();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void OnEnable()
        {
            // 활성화될 때 표시 갱신
            UpdateDisplay();
        }


        // ====== 초기화 ======

        private void CacheRectTransforms()
        {
            // Bone RectTransform
            if (boneText != null)
            {
                boneRect = boneText.GetComponent<RectTransform>();
                if (boneRect != null)
                {
                    boneOriginalScale = boneRect.localScale;
                }
            }

            // Soul RectTransform
            if (soulText != null)
            {
                soulRect = soulText.GetComponent<RectTransform>();
                if (soulRect != null)
                {
                    soulOriginalScale = soulRect.localScale;
                }
            }

            // Gold RectTransform
            if (goldText != null)
            {
                goldRect = goldText.GetComponent<RectTransform>();
                if (goldRect != null)
                {
                    goldOriginalScale = goldRect.localScale;
                }
            }
        }


        // ====== 이벤트 구독 ======

        private void SubscribeToEvents()
        {
            if (!MetaProgressionManager.HasInstance) return;

            var currency = MetaProgressionManager.Instance.Currency;
            if (currency != null)
            {
                currency.OnTempBoneChanged += OnTempBoneChanged;
                currency.OnTempSoulChanged += OnTempSoulChanged;
                currency.OnBoneChanged += OnBoneChanged;
                currency.OnSoulChanged += OnSoulChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (!MetaProgressionManager.HasInstance) return;

            var currency = MetaProgressionManager.Instance.Currency;
            if (currency != null)
            {
                currency.OnTempBoneChanged -= OnTempBoneChanged;
                currency.OnTempSoulChanged -= OnTempSoulChanged;
                currency.OnBoneChanged -= OnBoneChanged;
                currency.OnSoulChanged -= OnSoulChanged;
            }
        }


        // ====== 이벤트 핸들러 ======

        private void OnTempBoneChanged(int oldAmount, int newAmount)
        {
            if (!isShowingTempCurrency) return;

            UpdateBoneDisplay(newAmount, true);

            // 펄스 효과 (증가 시에만)
            if (enablePulseEffect && newAmount > oldAmount)
            {
                PlayPulseEffect(boneRect, boneOriginalScale);
            }
        }

        private void OnTempSoulChanged(int oldAmount, int newAmount)
        {
            if (!isShowingTempCurrency) return;

            UpdateSoulDisplay(newAmount, true);

            // 펄스 효과 (증가 시에만)
            if (enablePulseEffect && newAmount > oldAmount)
            {
                PlayPulseEffect(soulRect, soulOriginalScale);
            }
        }

        private void OnBoneChanged(int oldAmount, int newAmount)
        {
            if (isShowingTempCurrency) return;

            UpdateBoneDisplay(newAmount, false);

            if (enablePulseEffect && newAmount > oldAmount)
            {
                PlayPulseEffect(boneRect, boneOriginalScale);
            }
        }

        private void OnSoulChanged(int oldAmount, int newAmount)
        {
            if (isShowingTempCurrency) return;

            UpdateSoulDisplay(newAmount, false);

            if (enablePulseEffect && newAmount > oldAmount)
            {
                PlayPulseEffect(soulRect, soulOriginalScale);
            }
        }


        // ====== 표시 업데이트 ======

        /// <summary>
        /// 현재 모드에 따라 표시 업데이트
        /// </summary>
        public void UpdateDisplay()
        {
            if (!MetaProgressionManager.HasInstance) return;

            var currency = MetaProgressionManager.Instance.Currency;
            if (currency == null) return;

            if (isShowingTempCurrency)
            {
                UpdateBoneDisplay(currency.TempBone, true);
                UpdateSoulDisplay(currency.TempSoul, true);
            }
            else
            {
                UpdateBoneDisplay(currency.Bone, false);
                UpdateSoulDisplay(currency.Soul, false);
            }
        }

        /// <summary>
        /// Bone 표시 업데이트
        /// </summary>
        private void UpdateBoneDisplay(int amount, bool isTemp)
        {
            lastBone = amount;

            if (boneText != null)
            {
                string prefix = (isTemp && showPlusPrefix && amount > 0) ? "+" : "";
                boneText.text = prefix + FormatNumber(amount);
            }
        }

        /// <summary>
        /// Soul 표시 업데이트
        /// </summary>
        private void UpdateSoulDisplay(int amount, bool isTemp)
        {
            lastSoul = amount;

            if (soulText != null)
            {
                string prefix = (isTemp && showPlusPrefix && amount > 0) ? "+" : "";
                soulText.text = prefix + FormatNumber(amount);
            }

            // Soul 표시 여부 (임시 재화일 때만 조건부 숨김)
            if (showSoulOnlyWhenEarned && isTemp)
            {
                bool showSoul = amount > 0;
                SetSoulVisibility(showSoul);
            }
            else
            {
                SetSoulVisibility(true);
            }
        }

        /// <summary>
        /// 골드 표시 업데이트
        /// </summary>
        public void UpdateGoldDisplay(int amount)
        {
            lastGold = amount;

            if (goldText != null)
            {
                goldText.text = FormatNumber(amount);
            }
        }

        /// <summary>
        /// 골드 증가 (펄스 효과 포함)
        /// </summary>
        public void AddGold(int oldAmount, int newAmount)
        {
            UpdateGoldDisplay(newAmount);

            if (enablePulseEffect && newAmount > oldAmount)
            {
                PlayPulseEffect(goldRect, goldOriginalScale);
            }
        }


        // ====== 모드 전환 ======

        /// <summary>
        /// 임시 재화 표시 모드로 전환 (런 진행 중)
        /// </summary>
        public void ShowTempCurrency()
        {
            isShowingTempCurrency = true;
            UpdateDisplay();
        }

        /// <summary>
        /// 영구 재화 표시 모드로 전환 (로비)
        /// </summary>
        public void ShowPermanentCurrency()
        {
            isShowingTempCurrency = false;
            UpdateDisplay();
        }


        // ====== 표시/숨김 ======

        /// <summary>
        /// Bone 표시 영역 표시/숨김
        /// </summary>
        public void SetBoneVisibility(bool visible)
        {
            if (boneContainer != null)
            {
                boneContainer.SetActive(visible);
            }
            else if (boneText != null)
            {
                boneText.gameObject.SetActive(visible);
                if (boneIcon != null)
                {
                    boneIcon.gameObject.SetActive(visible);
                }
            }
        }

        /// <summary>
        /// Soul 표시 영역 표시/숨김
        /// </summary>
        public void SetSoulVisibility(bool visible)
        {
            if (soulContainer != null)
            {
                soulContainer.SetActive(visible);
            }
            else if (soulText != null)
            {
                soulText.gameObject.SetActive(visible);
                if (soulIcon != null)
                {
                    soulIcon.gameObject.SetActive(visible);
                }
            }
        }

        /// <summary>
        /// 전체 HUD 표시
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            UpdateDisplay();
        }

        /// <summary>
        /// 전체 HUD 숨김
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 표시 중인지 여부
        /// </summary>
        public bool IsVisible => gameObject.activeSelf;


        // ====== 펄스 효과 ======

        private async void PlayPulseEffect(RectTransform target, Vector3 originalScale)
        {
            if (target == null) return;

            // 확대
            target.localScale = originalScale * pulseScale;

            // 대기
            await Awaitable.WaitForSecondsAsync(pulseDuration);

            // 원래 크기로 복원 (오브젝트가 아직 유효한지 확인)
            if (target != null && this != null)
            {
                target.localScale = originalScale;
            }
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 숫자 포맷 (천 단위 구분)
        /// </summary>
        private string FormatNumber(int number)
        {
            if (useNumberFormat)
            {
                return number.ToString("N0");
            }
            return number.ToString();
        }


        // ====== 디버그 ======

#if UNITY_EDITOR
        [ContextMenu("Update Display")]
        private void DebugUpdateDisplay()
        {
            UpdateDisplay();
        }

        [ContextMenu("Show Temp Currency")]
        private void DebugShowTempCurrency()
        {
            ShowTempCurrency();
        }

        [ContextMenu("Show Permanent Currency")]
        private void DebugShowPermanentCurrency()
        {
            ShowPermanentCurrency();
        }

        [ContextMenu("Test Bone Pulse")]
        private void DebugTestBonePulse()
        {
            PlayPulseEffect(boneRect, boneOriginalScale);
        }

        [ContextMenu("Test Soul Pulse")]
        private void DebugTestSoulPulse()
        {
            PlayPulseEffect(soulRect, soulOriginalScale);
        }

        [ContextMenu("Test Add Bone (+10)")]
        private void DebugTestAddBone()
        {
            OnTempBoneChanged(lastBone, lastBone + 10);
        }

        [ContextMenu("Test Add Soul (+1)")]
        private void DebugTestAddSoul()
        {
            OnTempSoulChanged(lastSoul, lastSoul + 1);
        }
#endif
    }
}
