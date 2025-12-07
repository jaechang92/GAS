using UnityEngine;
using UnityEngine.UI;
using GASPT.Meta;

namespace GASPT.UI.Meta
{
    /// <summary>
    /// 메타 재화 HUD 표시
    /// 런 진행 중 임시 재화 (Bone/Soul) 표시
    /// </summary>
    public class MetaHUDView : MonoBehaviour
    {
        // ====== UI 요소 ======

        [Header("재화 표시")]
        [Tooltip("Bone 수량 텍스트")]
        [SerializeField] private Text boneText;

        [Tooltip("Soul 수량 텍스트")]
        [SerializeField] private Text soulText;

        [Tooltip("Bone 아이콘")]
        [SerializeField] private Image boneIcon;

        [Tooltip("Soul 아이콘")]
        [SerializeField] private Image soulIcon;


        // ====== 설정 ======

        [Header("설정")]
        [Tooltip("Soul 텍스트는 획득 시에만 표시")]
        [SerializeField] private bool showSoulOnlyWhenEarned = true;

        [Tooltip("재화 변경 시 펄스 효과")]
        [SerializeField] private bool enablePulseEffect = true;

        [Tooltip("펄스 효과 스케일")]
        [SerializeField] private float pulseScale = 1.2f;

        [Tooltip("펄스 효과 지속 시간")]
        [SerializeField] private float pulseDuration = 0.2f;


        // ====== 런타임 ======

        private int lastBone;
        private int lastSoul;
        private RectTransform boneRect;
        private RectTransform soulRect;
        private Vector3 boneOriginalScale;
        private Vector3 soulOriginalScale;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // RectTransform 캐싱
            if (boneText != null)
            {
                boneRect = boneText.GetComponent<RectTransform>();
                if (boneRect != null)
                {
                    boneOriginalScale = boneRect.localScale;
                }
            }

            if (soulText != null)
            {
                soulRect = soulText.GetComponent<RectTransform>();
                if (soulRect != null)
                {
                    soulOriginalScale = soulRect.localScale;
                }
            }
        }

        private void Start()
        {
            // MetaProgressionManager 이벤트 구독
            SubscribeToEvents();

            // 초기 상태 업데이트
            UpdateDisplay();
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            UnsubscribeFromEvents();
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
            }

            Debug.Log("[MetaHUDView] 이벤트 구독 완료");
        }

        private void UnsubscribeFromEvents()
        {
            if (!MetaProgressionManager.HasInstance) return;

            var currency = MetaProgressionManager.Instance.Currency;
            if (currency != null)
            {
                currency.OnTempBoneChanged -= OnTempBoneChanged;
                currency.OnTempSoulChanged -= OnTempSoulChanged;
            }
        }


        // ====== 이벤트 핸들러 ======

        private void OnTempBoneChanged(int oldAmount, int newAmount)
        {
            if (boneText != null)
            {
                boneText.text = newAmount.ToString();

                // 펄스 효과 (증가 시에만)
                if (enablePulseEffect && newAmount > oldAmount)
                {
                    PlayPulseEffect(boneRect, boneOriginalScale);
                }
            }

            lastBone = newAmount;
        }

        private void OnTempSoulChanged(int oldAmount, int newAmount)
        {
            if (soulText != null)
            {
                soulText.text = newAmount.ToString();

                // Soul 표시 여부
                if (showSoulOnlyWhenEarned)
                {
                    bool showSoul = newAmount > 0;
                    soulText.gameObject.SetActive(showSoul);
                    if (soulIcon != null)
                    {
                        soulIcon.gameObject.SetActive(showSoul);
                    }
                }

                // 펄스 효과 (증가 시에만)
                if (enablePulseEffect && newAmount > oldAmount)
                {
                    PlayPulseEffect(soulRect, soulOriginalScale);
                }
            }

            lastSoul = newAmount;
        }


        // ====== 표시 업데이트 ======

        /// <summary>
        /// 현재 상태로 표시 업데이트
        /// </summary>
        public void UpdateDisplay()
        {
            if (!MetaProgressionManager.HasInstance) return;

            var currency = MetaProgressionManager.Instance.Currency;
            if (currency == null) return;

            // Bone 표시
            if (boneText != null)
            {
                lastBone = currency.TempBone;
                boneText.text = lastBone.ToString();
            }

            // Soul 표시
            if (soulText != null)
            {
                lastSoul = currency.TempSoul;
                soulText.text = lastSoul.ToString();

                // Soul 표시 여부
                if (showSoulOnlyWhenEarned)
                {
                    bool showSoul = lastSoul > 0;
                    soulText.gameObject.SetActive(showSoul);
                    if (soulIcon != null)
                    {
                        soulIcon.gameObject.SetActive(showSoul);
                    }
                }
            }
        }

        /// <summary>
        /// 영구 재화 표시로 전환 (런 종료 후)
        /// </summary>
        public void ShowPermanentCurrency()
        {
            if (!MetaProgressionManager.HasInstance) return;

            var currency = MetaProgressionManager.Instance.Currency;
            if (currency == null) return;

            if (boneText != null)
            {
                boneText.text = currency.Bone.ToString();
            }

            if (soulText != null)
            {
                soulText.text = currency.Soul.ToString();

                // 영구 재화 표시 시에는 항상 표시
                soulText.gameObject.SetActive(true);
                if (soulIcon != null)
                {
                    soulIcon.gameObject.SetActive(true);
                }
            }
        }


        // ====== 펄스 효과 ======

        /// <summary>
        /// 펄스 효과 재생
        /// </summary>
        private async void PlayPulseEffect(RectTransform target, Vector3 originalScale)
        {
            if (target == null) return;

            // 확대
            target.localScale = originalScale * pulseScale;

            // 대기
            await Awaitable.WaitForSecondsAsync(pulseDuration);

            // 원래 크기로 복원
            if (target != null)
            {
                target.localScale = originalScale;
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Update Display")]
        private void DebugUpdateDisplay()
        {
            UpdateDisplay();
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
    }
}
