using System;
using UnityEngine;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 폼 시스템의 핵심 매니저
    /// 플레이어의 폼 슬롯 관리, 교체, 획득 등을 담당
    /// </summary>
    public class FormManager : MonoBehaviour
    {
        // ====== 이벤트 ======

        /// <summary>폼 교체 완료 이벤트 (이전 폼, 새 폼)</summary>
        public event Action<FormInstance, FormInstance> OnFormSwapped;

        /// <summary>폼 획득 이벤트</summary>
        public event Action<FormInstance> OnFormAcquired;

        /// <summary>폼 버림 이벤트</summary>
        public event Action<FormInstance> OnFormDropped;

        /// <summary>쿨다운 시작 이벤트 (쿨다운 시간)</summary>
        public event Action<float> OnSwapCooldownStarted;

        /// <summary>쿨다운 완료 이벤트</summary>
        public event Action OnSwapCooldownEnded;


        // ====== 설정 ======

        [Header("설정")]
        [SerializeField] private float swapCooldown = 5f;
        [SerializeField] private float swapInvincibilityDuration = 0.2f;
        [SerializeField] private FormData defaultForm;

        [Header("입력")]
        [SerializeField] private KeyCode swapKey = KeyCode.Q;

        [Header("참조")]
        [SerializeField] private FormSwapSystem swapSystem;

        [Header("디버그")]
        [SerializeField] private bool logDebugInfo = true;


        // ====== 상태 ======

        private FormInstance primaryForm;    // 현재 활성 폼 (슬롯 1)
        private FormInstance secondaryForm;  // 대기 폼 (슬롯 2)
        private float cooldownTimer;
        private bool isSwapping;


        // ====== 프로퍼티 ======

        /// <summary>현재 활성 폼</summary>
        public FormInstance CurrentForm => primaryForm;

        /// <summary>대기 폼</summary>
        public FormInstance ReserveForm => secondaryForm;

        /// <summary>교체 쿨다운 중인지 여부</summary>
        public bool IsOnCooldown => cooldownTimer > 0f;

        /// <summary>교체 가능 여부</summary>
        public bool CanSwap => !IsOnCooldown && !isSwapping && secondaryForm != null;

        /// <summary>남은 쿨다운 시간</summary>
        public float RemainingCooldown => Mathf.Max(0f, cooldownTimer);

        /// <summary>쿨다운 진행률 (0~1)</summary>
        public float CooldownProgress => swapCooldown > 0f ? 1f - (cooldownTimer / swapCooldown) : 1f;

        /// <summary>빈 슬롯 여부</summary>
        public bool HasEmptySlot => primaryForm == null || secondaryForm == null;

        /// <summary>폼 슬롯 수</summary>
        public int FormCount => (primaryForm != null ? 1 : 0) + (secondaryForm != null ? 1 : 0);


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            InitializeDefaultForm();
        }

        private void Update()
        {
            UpdateCooldown();
            HandleInput();
        }


        // ====== 초기화 ======

        /// <summary>
        /// 기본 폼으로 초기화
        /// </summary>
        private void InitializeDefaultForm()
        {
            if (defaultForm != null && primaryForm == null)
            {
                primaryForm = new FormInstance(defaultForm);
                Log($"기본 폼 설정: {primaryForm.FormName}");

                // SwapSystem에 초기 폼 적용
                if (swapSystem != null)
                {
                    swapSystem.InitializeWithForm(primaryForm);
                }
            }
        }

        /// <summary>
        /// 외부에서 폼 초기화
        /// </summary>
        public void Initialize(FormData primary, FormData secondary = null)
        {
            primaryForm = primary != null ? new FormInstance(primary) : null;
            secondaryForm = secondary != null ? new FormInstance(secondary) : null;

            cooldownTimer = 0f;
            isSwapping = false;

            Log($"폼 초기화 완료 - Primary: {primaryForm?.FormName ?? "없음"}, Secondary: {secondaryForm?.FormName ?? "없음"}");
        }


        // ====== 입력 처리 ======

        private void HandleInput()
        {
            if (Input.GetKeyDown(swapKey))
            {
                TrySwapForm();
            }
        }


        // ====== 폼 교체 ======

        /// <summary>
        /// 폼 교체 시도
        /// </summary>
        /// <returns>교체 성공 여부</returns>
        public bool TrySwapForm()
        {
            if (!CanSwap)
            {
                if (IsOnCooldown)
                    Log($"쿨다운 중: {cooldownTimer:F1}초 남음");
                else if (secondaryForm == null)
                    Log("대기 폼이 없습니다.");

                return false;
            }

            PerformSwap();
            return true;
        }

        /// <summary>
        /// 실제 폼 교체 수행
        /// </summary>
        private void PerformSwap()
        {
            isSwapping = true;

            var previousForm = primaryForm;
            var newForm = secondaryForm;

            // 슬롯 교환
            (primaryForm, secondaryForm) = (secondaryForm, primaryForm);

            // 쿨다운 시작
            cooldownTimer = swapCooldown;

            Log($"폼 교체: {previousForm?.FormName} → {newForm?.FormName}");

            // FormSwapSystem으로 실제 교체 실행 (스탯, 외형, 이펙트 등)
            if (swapSystem != null)
            {
                swapSystem.ExecuteSwap(previousForm, newForm);
            }

            // 이벤트 발생
            OnFormSwapped?.Invoke(previousForm, newForm);
            OnSwapCooldownStarted?.Invoke(swapCooldown);

            isSwapping = false;
        }


        // ====== 쿨다운 ======

        private void UpdateCooldown()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;

                if (cooldownTimer <= 0f)
                {
                    cooldownTimer = 0f;
                    OnSwapCooldownEnded?.Invoke();
                    Log("교체 쿨다운 완료");
                }
            }
        }


        // ====== 폼 획득 ======

        /// <summary>
        /// 새 폼 획득 시도
        /// </summary>
        /// <param name="newFormData">획득할 폼 데이터</param>
        /// <returns>획득 결과</returns>
        public AcquireResult TryAcquireForm(FormData newFormData)
        {
            if (newFormData == null)
                return AcquireResult.InvalidForm;

            // 동일 폼 체크 (각성)
            if (primaryForm != null && primaryForm.IsSameForm(newFormData))
            {
                if (primaryForm.Awaken())
                    return AcquireResult.Awakened;
                else
                    return AcquireResult.MaxAwakening;
            }

            if (secondaryForm != null && secondaryForm.IsSameForm(newFormData))
            {
                if (secondaryForm.Awaken())
                    return AcquireResult.Awakened;
                else
                    return AcquireResult.MaxAwakening;
            }

            // 빈 슬롯에 획득
            var newForm = new FormInstance(newFormData);

            if (primaryForm == null)
            {
                primaryForm = newForm;
                OnFormAcquired?.Invoke(newForm);
                Log($"폼 획득 (슬롯1): {newForm.FormName}");
                return AcquireResult.AcquiredPrimary;
            }

            if (secondaryForm == null)
            {
                secondaryForm = newForm;
                OnFormAcquired?.Invoke(newForm);
                Log($"폼 획득 (슬롯2): {newForm.FormName}");
                return AcquireResult.AcquiredSecondary;
            }

            // 슬롯이 가득 찬 경우
            return AcquireResult.SlotFull;
        }

        /// <summary>
        /// 특정 슬롯의 폼을 교체
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스 (0 또는 1)</param>
        /// <param name="newFormData">새 폼 데이터</param>
        /// <returns>버려진 폼 인스턴스</returns>
        public FormInstance ReplaceForm(int slotIndex, FormData newFormData)
        {
            FormInstance droppedForm = null;
            var newForm = new FormInstance(newFormData);

            if (slotIndex == 0)
            {
                droppedForm = primaryForm;
                primaryForm = newForm;
            }
            else
            {
                droppedForm = secondaryForm;
                secondaryForm = newForm;
            }

            OnFormAcquired?.Invoke(newForm);

            if (droppedForm != null)
            {
                OnFormDropped?.Invoke(droppedForm);
                Log($"폼 교체: {droppedForm.FormName} → {newForm.FormName}");
            }

            return droppedForm;
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 특정 슬롯의 폼 반환
        /// </summary>
        public FormInstance GetForm(int slotIndex)
        {
            return slotIndex == 0 ? primaryForm : secondaryForm;
        }

        /// <summary>
        /// 폼 타입으로 보유 폼 검색
        /// </summary>
        public FormInstance GetFormByType(FormType type)
        {
            if (primaryForm?.FormType == type) return primaryForm;
            if (secondaryForm?.FormType == type) return secondaryForm;
            return null;
        }

        /// <summary>
        /// 현재 상태 로그 출력
        /// </summary>
        [ContextMenu("Log Status")]
        public void LogStatus()
        {
            Debug.Log("===== FormManager Status =====");
            Debug.Log($"Primary: {primaryForm?.ToString() ?? "Empty"}");
            Debug.Log($"Secondary: {secondaryForm?.ToString() ?? "Empty"}");
            Debug.Log($"Cooldown: {cooldownTimer:F1}s / CanSwap: {CanSwap}");
        }

        private void Log(string message)
        {
            if (logDebugInfo)
            {
                Debug.Log($"[FormManager] {message}");
            }
        }
    }

}
