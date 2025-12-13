using System;
using UnityEngine;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 폼 교체 실행 시스템
    /// 폼 교체 시 스탯, 애니메이터, 이펙트 등을 처리
    /// FormManager에서 호출됨
    /// </summary>
    public class FormSwapSystem : MonoBehaviour
    {
        // ====== 이벤트 ======

        /// <summary>교체 시작 이벤트</summary>
        public event Action OnSwapStarted;

        /// <summary>교체 완료 이벤트</summary>
        public event Action OnSwapCompleted;

        /// <summary>무적 시작 이벤트</summary>
        public event Action<float> OnInvincibilityStarted;

        /// <summary>무적 종료 이벤트</summary>
        public event Action OnInvincibilityEnded;


        // ====== 설정 ======

        [Header("설정")]
        [SerializeField] private float invincibilityDuration = 0.2f;

        [Header("이펙트")]
        [SerializeField] private Transform effectSpawnPoint;

        [Header("참조")]
        [SerializeField] private SpriteRenderer playerSpriteRenderer;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private AudioSource audioSource;

        [Header("디버그")]
        [SerializeField] private bool logDebugInfo = true;


        // ====== 상태 ======

        private bool isSwapping;
        private bool isInvincible;
        private float invincibilityTimer;
        private FormInstance previousForm;
        private FormInstance newForm;

        // 현재 적용된 폼 보너스 (되돌리기용)
        private FormStats appliedBonusStats;
        private bool hasBonusApplied;


        // ====== 프로퍼티 ======

        /// <summary>교체 중인지 여부</summary>
        public bool IsSwapping => isSwapping;

        /// <summary>무적 상태인지 여부</summary>
        public bool IsInvincible => isInvincible;

        /// <summary>현재 적용된 폼 보너스</summary>
        public FormStats AppliedBonusStats => appliedBonusStats;


        // ====== Unity 생명주기 ======

        private void Update()
        {
            UpdateInvincibility();
        }


        // ====== 교체 실행 ======

        /// <summary>
        /// 폼 교체 실행
        /// </summary>
        /// <param name="from">이전 폼</param>
        /// <param name="to">새 폼</param>
        public void ExecuteSwap(FormInstance from, FormInstance to)
        {
            if (isSwapping)
            {
                Log("이미 교체 중입니다.");
                return;
            }

            previousForm = from;
            newForm = to;

            isSwapping = true;
            OnSwapStarted?.Invoke();

            Log($"폼 교체 시작: {from?.FormName ?? "없음"} → {to?.FormName ?? "없음"}");

            // 1. 이전 폼 보너스 제거
            RemoveFormBonus(from);

            // 2. 새 폼 보너스 적용
            ApplyFormBonus(to);

            // 3. 외형 변경
            UpdateVisuals(to);

            // 4. 애니메이터 교체
            UpdateAnimator(to);

            // 5. 이펙트 재생
            PlaySwapEffect(to);

            // 6. 사운드 재생
            PlaySwapSound(to);

            // 7. 무적 프레임 시작
            StartInvincibility();

            isSwapping = false;
            OnSwapCompleted?.Invoke();

            Log($"폼 교체 완료: {to?.FormName}");
        }


        // ====== 스탯 보너스 ======

        /// <summary>
        /// 폼 보너스 적용
        /// </summary>
        private void ApplyFormBonus(FormInstance form)
        {
            if (form == null) return;

            appliedBonusStats = form.CurrentStats;
            hasBonusApplied = true;

            // TODO: PlayerStats와 연동
            // PlayerStats에 AddExternalBonus 메서드가 필요
            // 현재는 이벤트로 전달하여 외부에서 처리하도록 함

            Log($"폼 보너스 적용: {appliedBonusStats}");
        }

        /// <summary>
        /// 폼 보너스 제거
        /// </summary>
        private void RemoveFormBonus(FormInstance form)
        {
            if (!hasBonusApplied) return;

            // TODO: PlayerStats에서 이전 보너스 제거
            // PlayerStats.RemoveExternalBonus() 호출 필요

            hasBonusApplied = false;
            appliedBonusStats = default;

            Log("이전 폼 보너스 제거됨");
        }

        /// <summary>
        /// 현재 적용된 폼 스탯 반환 (외부에서 PlayerStats 연동용)
        /// </summary>
        public FormStats GetCurrentFormStats()
        {
            return hasBonusApplied ? appliedBonusStats : default;
        }


        // ====== 외형 변경 ======

        /// <summary>
        /// 플레이어 외형 업데이트
        /// </summary>
        private void UpdateVisuals(FormInstance form)
        {
            if (form == null) return;

            // 스프라이트 변경
            if (playerSpriteRenderer != null && form.FormSprite != null)
            {
                playerSpriteRenderer.sprite = form.FormSprite;
            }

            // 색상 변경 (틴트)
            if (playerSpriteRenderer != null)
            {
                playerSpriteRenderer.color = form.FormColor;
            }

            Log($"외형 업데이트: {form.FormName}");
        }


        // ====== 애니메이터 ======

        /// <summary>
        /// 애니메이터 컨트롤러 교체
        /// </summary>
        private void UpdateAnimator(FormInstance form)
        {
            if (playerAnimator == null || form == null) return;

            if (form.AnimatorController != null)
            {
                playerAnimator.runtimeAnimatorController = form.AnimatorController;
                Log($"애니메이터 교체: {form.AnimatorController.name}");
            }
        }


        // ====== 이펙트 ======

        /// <summary>
        /// 교체 이펙트 재생
        /// </summary>
        private void PlaySwapEffect(FormInstance form)
        {
            if (form?.Data?.swapEffectPrefab == null) return;

            Vector3 spawnPos = effectSpawnPoint != null
                ? effectSpawnPoint.position
                : transform.position;

            var effect = Instantiate(form.Data.swapEffectPrefab, spawnPos, Quaternion.identity);

            // 자동 삭제 (3초 후)
            Destroy(effect, 3f);

            Log($"교체 이펙트 재생: {form.Data.swapEffectPrefab.name}");
        }


        // ====== 사운드 ======

        /// <summary>
        /// 교체 사운드 재생
        /// </summary>
        private void PlaySwapSound(FormInstance form)
        {
            if (audioSource == null || form?.Data?.swapSound == null) return;

            audioSource.PlayOneShot(form.Data.swapSound);
            Log($"교체 사운드 재생");
        }


        // ====== 무적 프레임 ======

        /// <summary>
        /// 무적 시작
        /// </summary>
        private void StartInvincibility()
        {
            if (invincibilityDuration <= 0f) return;

            isInvincible = true;
            invincibilityTimer = invincibilityDuration;

            OnInvincibilityStarted?.Invoke(invincibilityDuration);

            Log($"무적 시작: {invincibilityDuration}초");
        }

        /// <summary>
        /// 무적 상태 업데이트
        /// </summary>
        private void UpdateInvincibility()
        {
            if (!isInvincible) return;

            invincibilityTimer -= Time.deltaTime;

            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                invincibilityTimer = 0f;

                OnInvincibilityEnded?.Invoke();

                Log("무적 종료");
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// 초기 폼 설정 (게임 시작 시)
        /// </summary>
        public void InitializeWithForm(FormInstance form)
        {
            if (form == null) return;

            ApplyFormBonus(form);
            UpdateVisuals(form);
            UpdateAnimator(form);

            Log($"초기 폼 설정: {form.FormName}");
        }

        /// <summary>
        /// 참조 자동 찾기
        /// </summary>
        public void AutoFindReferences()
        {
            if (playerSpriteRenderer == null)
                playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (playerAnimator == null)
                playerAnimator = GetComponentInChildren<Animator>();

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            if (effectSpawnPoint == null)
                effectSpawnPoint = transform;
        }

        private void Awake()
        {
            AutoFindReferences();
        }


        // ====== 유틸리티 ======

        private void Log(string message)
        {
            if (logDebugInfo)
            {
                Debug.Log($"[FormSwapSystem] {message}");
            }
        }
    }
}
