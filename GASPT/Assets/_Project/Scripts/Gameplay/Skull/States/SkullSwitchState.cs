using UnityEngine;
using FSM.Core;
using Skull.Core;
using System.Threading;
using static UnityEngine.Object;

namespace Skull.States
{
    /// <summary>
    /// 스컬 교체 상태
    /// FSM_Core를 활용하여 스컬 교체 중의 플레이어 상태를 관리
    /// </summary>
    public class SkullSwitchState : State
    {
        [Header("교체 설정")]
        [SerializeField] private float switchDuration = 0.5f;
        [SerializeField] private bool disableMovementDuringSwitch = true;
        [SerializeField] private bool disableInputDuringSwitch = true;

        [Header("애니메이션")]
        [SerializeField] private string switchAnimationTrigger = "SkullSwitch";
        [SerializeField] private bool useCustomAnimation = true;

        [Header("이펙트")]
        [SerializeField] private GameObject switchEffect;
        [SerializeField] private AudioClip switchSound;

        // 컴포넌트 참조
        private SkullSystem skullSystem;
        private SkullManager skullManager;
        private Animator animator;
        private Rigidbody2D rb2d;

        // 상태 관리
        private float switchStartTime;
        private bool isSwitchComplete = false;
        private Vector2 savedVelocity;
        private ISkullController targetSkull;

        #region State 생명주기

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken = default)
        {
            LogDebug("스컬 교체 상태 진입");

            // 컴포넌트 참조 초기화
            InitializeComponents();

            // 교체 시작 처리
            await StartSwitchProcess(cancellationToken);

            LogDebug("스컬 교체 상태 진입 완료");
        }

        protected override void OnUpdateState(float deltaTime)
        {
            if (isSwitchComplete) return;

            // 교체 진행도 체크
            float elapsed = Time.time - switchStartTime;
            float progress = elapsed / switchDuration;

            // 교체 완료 체크
            if (progress >= 1f)
            {
                // 비동기 작업은 별도 메서드에서 처리
                _ = CompleteSwitchProcessAsync();
            }
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken = default)
        {
            LogDebug("스컬 교체 상태 종료");

            // 교체 종료 처리
            await FinalizeSwitchProcess(cancellationToken);

            LogDebug("스컬 교체 상태 종료 완료");
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 컴포넌트 참조 초기화
        /// </summary>
        private void InitializeComponents()
        {
            if (skullSystem == null)
                skullSystem = Owner.GetComponent<SkullSystem>();

            if (skullManager == null)
                skullManager = Owner.GetComponent<SkullManager>();

            if (animator == null)
                animator = Owner.GetComponent<Animator>();

            if (rb2d == null)
                rb2d = Owner.GetComponent<Rigidbody2D>();
        }

        #endregion

        #region 교체 프로세스

        /// <summary>
        /// 교체 프로세스 시작
        /// </summary>
        private async Awaitable StartSwitchProcess(CancellationToken cancellationToken)
        {
            switchStartTime = Time.time;
            isSwitchComplete = false;

            // 현재 상태 저장
            SaveCurrentState();

            // 입력 및 이동 제한
            ApplyMovementRestrictions();

            // 교체 애니메이션 시작
            PlaySwitchAnimation();

            // 교체 이펙트 생성
            CreateSwitchEffect();

            // 교체 사운드 재생
            PlaySwitchSound();

            LogDebug($"스컬 교체 프로세스 시작: 지속시간={switchDuration}초");

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        /// <summary>
        /// 교체 프로세스 완료 (비동기)
        /// </summary>
        private async Awaitable CompleteSwitchProcessAsync()
        {
            if (isSwitchComplete) return;

            isSwitchComplete = true;

            LogDebug("스컬 교체 프로세스 완료");

            // 새 스컬 활성화 확인
            ValidateSkullSwitch();

            // 상태 복원 준비
            PrepareStateRestore();

            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// 교체 프로세스 완료 (CancellationToken 지원)
        /// </summary>
        private async Awaitable CompleteSwitchProcess(CancellationToken cancellationToken)
        {
            if (isSwitchComplete) return;

            isSwitchComplete = true;

            LogDebug("스컬 교체 프로세스 완료");

            // 새 스컬 활성화 확인
            ValidateSkullSwitch();

            // 상태 복원 준비
            PrepareStateRestore();

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        /// <summary>
        /// 교체 프로세스 마무리
        /// </summary>
        private async Awaitable FinalizeSwitchProcess(CancellationToken cancellationToken)
        {
            // 상태 복원
            RestorePlayerState();

            // 제한 해제
            RemoveMovementRestrictions();

            // 교체 완료 이벤트 발생
            NotifySwitchComplete();

            LogDebug("스컬 교체 프로세스 마무리 완료");

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        #endregion

        #region 상태 관리

        /// <summary>
        /// 현재 상태 저장
        /// </summary>
        private void SaveCurrentState()
        {
            // 속도 저장
            if (rb2d != null)
            {
                savedVelocity = rb2d.linearVelocity;
            }

            LogDebug($"플레이어 상태 저장: 속도={savedVelocity}");
        }

        /// <summary>
        /// 플레이어 상태 복원
        /// </summary>
        private void RestorePlayerState()
        {
            // 속도 복원 (선택적)
            if (rb2d != null && !disableMovementDuringSwitch)
            {
                rb2d.linearVelocity = savedVelocity;
            }

            LogDebug($"플레이어 상태 복원: 속도={savedVelocity}");
        }

        /// <summary>
        /// 이동 제한 적용
        /// </summary>
        private void ApplyMovementRestrictions()
        {
            if (!disableMovementDuringSwitch) return;

            // 속도 제거
            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
            }

            // 입력 비활성화 (향후 InputHandler와 연동)
            if (disableInputDuringSwitch)
            {
                // inputHandler.SetEnabled(false);
            }

            LogDebug("이동 제한 적용");
        }

        /// <summary>
        /// 이동 제한 해제
        /// </summary>
        private void RemoveMovementRestrictions()
        {
            // 입력 활성화
            if (disableInputDuringSwitch)
            {
                // inputHandler.SetEnabled(true);
            }

            LogDebug("이동 제한 해제");
        }

        #endregion

        #region 애니메이션 및 이펙트

        /// <summary>
        /// 교체 애니메이션 재생
        /// </summary>
        private void PlaySwitchAnimation()
        {
            if (!useCustomAnimation || animator == null) return;

            if (!string.IsNullOrEmpty(switchAnimationTrigger))
            {
                animator.SetTrigger(switchAnimationTrigger);
                LogDebug($"교체 애니메이션 재생: {switchAnimationTrigger}");
            }
        }

        /// <summary>
        /// 교체 이펙트 생성
        /// </summary>
        private void CreateSwitchEffect()
        {
            if (switchEffect == null) return;

            var effect = Instantiate(switchEffect, Owner.transform.position, Owner.transform.rotation);
            Destroy(effect, switchDuration + 1f);

            LogDebug("교체 이펙트 생성");
        }

        /// <summary>
        /// 교체 사운드 재생
        /// </summary>
        private void PlaySwitchSound()
        {
            if (switchSound == null) return;

            // AudioSource.PlayClipAtPoint(switchSound, Owner.transform.position);
            LogDebug("교체 사운드 재생");
        }

        #endregion

        #region 검증 및 알림

        /// <summary>
        /// 스컬 교체 검증
        /// </summary>
        private void ValidateSkullSwitch()
        {
            if (skullManager?.CurrentSkull == null)
            {
                LogWarning("교체 완료 후 활성화된 스컬이 없습니다.");
                return;
            }

            var currentSkull = skullManager.CurrentSkull;
            LogDebug($"교체 검증 완료: 현재 스컬={currentSkull.SkullData?.SkullName}");
        }

        /// <summary>
        /// 상태 복원 준비
        /// </summary>
        private void PrepareStateRestore()
        {
            // 새 스컬의 능력치 적용 등 준비 작업
            LogDebug("상태 복원 준비");
        }

        /// <summary>
        /// 교체 완료 알림
        /// </summary>
        private void NotifySwitchComplete()
        {
            // 다른 시스템들에게 교체 완료 알림
            // 이벤트 발생이나 콜백 호출 등

            LogDebug("교체 완료 알림 발송");
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 교체 대상 스컬 설정
        /// </summary>
        public void SetTargetSkull(ISkullController skull)
        {
            targetSkull = skull;
            LogDebug($"교체 대상 스컬 설정: {skull?.SkullData?.SkullName}");
        }

        /// <summary>
        /// 교체 지속시간 설정
        /// </summary>
        public void SetSwitchDuration(float duration)
        {
            switchDuration = Mathf.Max(0.1f, duration);
            LogDebug($"교체 지속시간 설정: {switchDuration}초");
        }

        /// <summary>
        /// 교체 진행도 가져오기
        /// </summary>
        public float GetSwitchProgress()
        {
            if (!IsActive) return 0f;

            float elapsed = Time.time - switchStartTime;
            return Mathf.Clamp01(elapsed / switchDuration);
        }

        /// <summary>
        /// 교체 완료 여부
        /// </summary>
        public bool IsSwitchComplete()
        {
            return isSwitchComplete;
        }

        #endregion

        #region 상태 전환 조건

        /// <summary>
        /// 교체 완료 시 다음 상태로 전환할 수 있는지 체크
        /// </summary>
        public bool CanExitSwitchState()
        {
            return isSwitchComplete;
        }

        /// <summary>
        /// 강제 교체 중단이 가능한지 체크
        /// </summary>
        public bool CanCancelSwitch()
        {
            // 교체 시작 후 일정 시간이 지나면 중단 불가
            float elapsed = Time.time - switchStartTime;
            return elapsed < switchDuration * 0.3f;
        }

        #endregion

        #region 디버그

        private void LogDebug(string message)
        {
            Debug.Log($"[SkullSwitchState] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SkullSwitchState] {message}");
        }

        #endregion

        #region 에디터 전용

        [ContextMenu("교체 상태 정보")]
        private void PrintSwitchStateInfo()
        {
            if (!Application.isPlaying) return;

            Debug.Log($"=== 스컬 교체 상태 정보 ===\n" +
                     $"상태 활성화: {IsActive}\n" +
                     $"교체 완료: {isSwitchComplete}\n" +
                     $"교체 진행도: {GetSwitchProgress():P}\n" +
                     $"교체 지속시간: {switchDuration}초\n" +
                     $"이동 제한: {disableMovementDuringSwitch}\n" +
                     $"입력 제한: {disableInputDuringSwitch}\n" +
                     $"대상 스컬: {targetSkull?.SkullData?.SkullName ?? "없음"}");
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (!IsActive) return;

            // 교체 진행도 시각화
            float progress = GetSwitchProgress();

            Gizmos.color = Color.Lerp(Color.red, Color.green, progress);
            Gizmos.DrawWireSphere(Owner.transform.position, 1f + progress);

            // 교체 완료 표시
            if (isSwitchComplete)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(Owner.transform.position + Vector3.up * 2f, 0.2f);
            }
        }

        #endregion
    }
}
