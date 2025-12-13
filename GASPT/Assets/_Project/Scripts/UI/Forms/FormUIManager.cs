using UnityEngine;
using GASPT.Gameplay.Form;

namespace GASPT.UI.Forms
{
    /// <summary>
    /// 폼 UI 통합 매니저
    /// FormManager 이벤트를 받아 UI 업데이트
    /// </summary>
    public class FormUIManager : MonoBehaviour
    {
        // ====== 참조 ======

        [Header("시스템 참조")]
        [SerializeField] private FormManager formManager;

        [Header("UI 참조")]
        [SerializeField] private FormSlotUI primarySlotUI;
        [SerializeField] private FormSlotUI secondarySlotUI;
        [SerializeField] private FormCooldownUI cooldownUI;
        [SerializeField] private FormSelectionUI selectionUI;

        [Header("각성 UI")]
        [SerializeField] private GameObject awakeningNotification;
        [SerializeField] private GameObject maxAwakeningNotification;
        [SerializeField] private float notificationDuration = 2f;

        [Header("설정")]
        [SerializeField] private bool autoFindReferences = true;
        [SerializeField] private bool logDebugInfo = true;


        // ====== 상태 ======

        private FormInstance subscribedPrimaryForm;
        private FormInstance subscribedSecondaryForm;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (autoFindReferences)
            {
                AutoFindReferences();
            }
        }

        private void Start()
        {
            SubscribeToEvents();
            InitializeUI();
        }

        private void Update()
        {
            UpdateCooldownUI();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }


        // ====== 초기화 ======

        private void AutoFindReferences()
        {
            if (formManager == null)
            {
                formManager = FindAnyObjectByType<FormManager>();
            }

            if (selectionUI == null)
            {
                selectionUI = FindAnyObjectByType<FormSelectionUI>();
            }
        }

        private void InitializeUI()
        {
            if (formManager == null)
            {
                Log("FormManager를 찾을 수 없습니다.");
                return;
            }

            // 초기 슬롯 표시
            UpdateSlotUI(primarySlotUI, formManager.CurrentForm, true);
            UpdateSlotUI(secondarySlotUI, formManager.ReserveForm, false);

            // 초기 쿨다운 상태
            if (cooldownUI != null)
            {
                if (formManager.CanSwap)
                {
                    cooldownUI.SetReady();
                }
                else if (formManager.ReserveForm == null)
                {
                    cooldownUI.SetNotAvailable();
                }
            }

            Log("UI 초기화 완료");
        }


        // ====== 이벤트 구독 ======

        private void SubscribeToEvents()
        {
            if (formManager != null)
            {
                formManager.OnFormSwapped += HandleFormSwapped;
                formManager.OnFormAcquired += HandleFormAcquired;
                formManager.OnFormDropped += HandleFormDropped;
                formManager.OnSwapCooldownStarted += HandleCooldownStarted;
                formManager.OnSwapCooldownEnded += HandleCooldownEnded;

                // 초기 폼에 대한 각성 이벤트 구독
                SubscribeToFormAwakening(formManager.CurrentForm, true);
                SubscribeToFormAwakening(formManager.ReserveForm, false);
            }

            if (selectionUI != null)
            {
                selectionUI.OnSlotSelected += HandleSlotSelected;
                selectionUI.OnCancelled += HandleSelectionCancelled;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (formManager != null)
            {
                formManager.OnFormSwapped -= HandleFormSwapped;
                formManager.OnFormAcquired -= HandleFormAcquired;
                formManager.OnFormDropped -= HandleFormDropped;
                formManager.OnSwapCooldownStarted -= HandleCooldownStarted;
                formManager.OnSwapCooldownEnded -= HandleCooldownEnded;
            }

            if (selectionUI != null)
            {
                selectionUI.OnSlotSelected -= HandleSlotSelected;
                selectionUI.OnCancelled -= HandleSelectionCancelled;
            }

            // 폼 각성 이벤트 구독 해제
            UnsubscribeFromFormAwakening(subscribedPrimaryForm);
            UnsubscribeFromFormAwakening(subscribedSecondaryForm);
        }

        private void SubscribeToFormAwakening(FormInstance form, bool isPrimary)
        {
            if (form == null) return;

            form.OnAwakened += HandleFormAwakened;
            form.OnMaxAwakeningReached += HandleMaxAwakeningReached;

            if (isPrimary)
                subscribedPrimaryForm = form;
            else
                subscribedSecondaryForm = form;

            Log($"폼 각성 이벤트 구독: {form.FormName}");
        }

        private void UnsubscribeFromFormAwakening(FormInstance form)
        {
            if (form == null) return;

            form.OnAwakened -= HandleFormAwakened;
            form.OnMaxAwakeningReached -= HandleMaxAwakeningReached;
        }


        // ====== 이벤트 핸들러 ======

        private void HandleFormSwapped(FormInstance previousForm, FormInstance newForm)
        {
            Log($"폼 교체됨: {previousForm?.FormName} → {newForm?.FormName}");

            // 슬롯 UI 업데이트
            UpdateSlotUI(primarySlotUI, formManager.CurrentForm, true);
            UpdateSlotUI(secondarySlotUI, formManager.ReserveForm, false);
        }

        private void HandleFormAcquired(FormInstance form)
        {
            Log($"폼 획득됨: {form?.FormName}");

            // 새 폼에 대한 각성 이벤트 구독
            if (form != null)
            {
                SubscribeToFormAwakening(form, formManager.CurrentForm == form);
            }

            // 슬롯 UI 업데이트
            UpdateSlotUI(primarySlotUI, formManager.CurrentForm, true);
            UpdateSlotUI(secondarySlotUI, formManager.ReserveForm, false);

            // 쿨다운 UI 상태 업데이트
            if (cooldownUI != null && formManager.ReserveForm != null)
            {
                if (!formManager.IsOnCooldown)
                {
                    cooldownUI.SetReady();
                }
            }
        }

        private void HandleFormDropped(FormInstance form)
        {
            Log($"폼 드롭됨: {form?.FormName}");
        }

        private void HandleCooldownStarted(float duration)
        {
            Log($"쿨다운 시작: {duration}초");

            if (cooldownUI != null)
            {
                cooldownUI.StartCooldown(duration);
            }
        }

        private void HandleCooldownEnded()
        {
            Log("쿨다운 완료");

            if (cooldownUI != null)
            {
                if (formManager.ReserveForm != null)
                {
                    cooldownUI.SetReady();
                }
                else
                {
                    cooldownUI.SetNotAvailable();
                }
            }
        }

        private void HandleSlotSelected(int slotIndex)
        {
            Log($"슬롯 {slotIndex + 1} 선택됨");
        }

        private void HandleSelectionCancelled()
        {
            Log("선택 취소됨");
        }

        private void HandleFormAwakened(int level, FormRarity rarity)
        {
            Log($"폼 각성됨! 레벨: {level}, 등급: {rarity}");

            // 슬롯 UI 업데이트
            UpdateSlotUI(primarySlotUI, formManager.CurrentForm, true);
            UpdateSlotUI(secondarySlotUI, formManager.ReserveForm, false);

            // 각성 알림 표시
            ShowAwakeningNotification();
        }

        private void HandleMaxAwakeningReached()
        {
            Log("최대 각성 도달!");

            // 슬롯 UI 업데이트
            UpdateSlotUI(primarySlotUI, formManager.CurrentForm, true);
            UpdateSlotUI(secondarySlotUI, formManager.ReserveForm, false);

            // 최대 각성 알림 표시
            ShowMaxAwakeningNotification();
        }


        // ====== UI 업데이트 ======

        private void UpdateSlotUI(FormSlotUI slotUI, FormInstance form, bool isActive)
        {
            if (slotUI == null) return;

            if (form != null)
            {
                slotUI.SetForm(form);
            }
            else
            {
                slotUI.SetEmpty();
            }

            slotUI.IsActiveSlot = isActive;
        }

        private void UpdateCooldownUI()
        {
            if (cooldownUI == null || formManager == null) return;

            if (formManager.IsOnCooldown)
            {
                cooldownUI.UpdateCooldown(
                    formManager.RemainingCooldown,
                    formManager.CooldownProgress
                );
            }
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 폼 선택 UI 표시 (FormPickup에서 호출)
        /// </summary>
        public void ShowFormSelection(FormData newForm, FormPickup pickup)
        {
            if (selectionUI == null || formManager == null)
            {
                Log("선택 UI 또는 FormManager가 없습니다.");
                return;
            }

            selectionUI.ShowSelection(
                newForm,
                formManager.CurrentForm,
                formManager.ReserveForm,
                pickup
            );
        }

        /// <summary>
        /// UI 수동 갱신
        /// </summary>
        public void RefreshUI()
        {
            InitializeUI();
        }


        // ====== 알림 UI ======

        private void ShowAwakeningNotification()
        {
            if (awakeningNotification != null)
            {
                awakeningNotification.SetActive(true);
                CancelInvoke(nameof(HideAwakeningNotification));
                Invoke(nameof(HideAwakeningNotification), notificationDuration);
            }
        }

        private void HideAwakeningNotification()
        {
            if (awakeningNotification != null)
            {
                awakeningNotification.SetActive(false);
            }
        }

        private void ShowMaxAwakeningNotification()
        {
            if (maxAwakeningNotification != null)
            {
                maxAwakeningNotification.SetActive(true);
                CancelInvoke(nameof(HideMaxAwakeningNotification));
                Invoke(nameof(HideMaxAwakeningNotification), notificationDuration * 1.5f);
            }
            else
            {
                // 최대 각성 알림이 없으면 일반 각성 알림 사용
                ShowAwakeningNotification();
            }
        }

        private void HideMaxAwakeningNotification()
        {
            if (maxAwakeningNotification != null)
            {
                maxAwakeningNotification.SetActive(false);
            }
        }


        // ====== 유틸리티 ======

        private void Log(string message)
        {
            if (logDebugInfo)
            {
                Debug.Log($"[FormUIManager] {message}");
            }
        }


        // ====== 에디터 ======

        [ContextMenu("Refresh UI")]
        private void DebugRefreshUI()
        {
            RefreshUI();
        }

        [ContextMenu("Find References")]
        private void DebugFindReferences()
        {
            AutoFindReferences();
            Debug.Log($"[FormUIManager] FormManager: {formManager != null}");
            Debug.Log($"[FormUIManager] PrimarySlotUI: {primarySlotUI != null}");
            Debug.Log($"[FormUIManager] SecondarySlotUI: {secondarySlotUI != null}");
            Debug.Log($"[FormUIManager] CooldownUI: {cooldownUI != null}");
            Debug.Log($"[FormUIManager] SelectionUI: {selectionUI != null}");
        }
    }
}
