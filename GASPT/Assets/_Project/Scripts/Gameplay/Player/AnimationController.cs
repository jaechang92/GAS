using UnityEngine;
using System;

namespace Player
{
    /// <summary>
    /// 플레이어 애니메이션 제어 전용 클래스
    /// 단일책임원칙: 애니메이션 파라미터 설정 및 스프라이트 방향 제어만 담당
    /// </summary>
    public class AnimationController : MonoBehaviour
    {
        [Header("애니메이션 설정")]
        [SerializeField] private bool enableAnimations = true;
        [SerializeField] private bool enableDebugLogs = false;

        // 컴포넌트 참조
        private Animator animator;
        private SpriteRenderer spriteRenderer;

        // 애니메이션 상태
        private int facingDirection = 1;
        private bool isInitialized = false;

        // 이벤트
        public event Action<string> OnAnimationStateChanged;
        public event Action<int> OnDirectionChanged;

        // 프로퍼티
        public bool EnableAnimations
        {
            get => enableAnimations;
            set => enableAnimations = value;
        }

        public int FacingDirection => facingDirection;
        public bool IsInitialized => isInitialized;

        private void Awake()
        {
            InitializeComponents();
        }

        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        private void InitializeComponents()
        {
            // Animator 컴포넌트 가져오기
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("[AnimationController] Animator 컴포넌트를 찾을 수 없습니다.");
            }

            // SpriteRenderer 컴포넌트 가져오기
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("[AnimationController] SpriteRenderer 컴포넌트를 찾을 수 없습니다.");
            }

            isInitialized = (animator != null || spriteRenderer != null);

            if (enableDebugLogs && isInitialized)
            {
                Debug.Log($"[AnimationController] 초기화 완료 - Animator: {animator != null}, SpriteRenderer: {spriteRenderer != null}");
            }
        }

        /// <summary>
        /// 애니메이션 파라미터 업데이트
        /// </summary>
        public void UpdateAnimationParameters(float speed, float velocityY, bool isGrounded, bool isTouchingWall)
        {
            if (!enableAnimations || animator == null) return;

            try
            {
                // 기본 애니메이션 파라미터 설정
                animator.SetFloat("Speed", Mathf.Abs(speed));
                animator.SetFloat("VelocityY", velocityY);
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetBool("IsTouchingWall", isTouchingWall);

                if (enableDebugLogs)
                {
                    Debug.Log($"[AnimationController] 파라미터 업데이트 - Speed: {speed:F2}, VelY: {velocityY:F2}, Ground: {isGrounded}, Wall: {isTouchingWall}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AnimationController] 애니메이션 파라미터 설정 오류: {e.Message}");
            }
        }

        /// <summary>
        /// 방향 업데이트 (스프라이트 플립)
        /// </summary>
        public void UpdateFacingDirection(int newDirection)
        {
            if (newDirection == 0) return;

            int oldDirection = facingDirection;
            facingDirection = newDirection;

            // 스프라이트 방향 설정
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = facingDirection < 0;
            }

            // 방향 변경 이벤트 발생
            if (oldDirection != facingDirection)
            {
                OnDirectionChanged?.Invoke(facingDirection);

                if (enableDebugLogs)
                {
                    Debug.Log($"[AnimationController] 방향 변경: {oldDirection} → {facingDirection}");
                }
            }
        }

        /// <summary>
        /// 특정 애니메이션 트리거 실행
        /// </summary>
        public void TriggerAnimation(string triggerName)
        {
            if (!enableAnimations || animator == null || string.IsNullOrEmpty(triggerName))
                return;

            try
            {
                animator.SetTrigger(triggerName);
                OnAnimationStateChanged?.Invoke(triggerName);

                if (enableDebugLogs)
                {
                    Debug.Log($"[AnimationController] 트리거 실행: {triggerName}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AnimationController] 트리거 실행 오류 '{triggerName}': {e.Message}");
            }
        }

        /// <summary>
        /// 특정 애니메이션 bool 파라미터 설정
        /// </summary>
        public void SetAnimationBool(string parameterName, bool value)
        {
            if (!enableAnimations || animator == null || string.IsNullOrEmpty(parameterName))
                return;

            try
            {
                animator.SetBool(parameterName, value);

                if (enableDebugLogs)
                {
                    Debug.Log($"[AnimationController] Bool 파라미터 설정: {parameterName} = {value}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AnimationController] Bool 파라미터 설정 오류 '{parameterName}': {e.Message}");
            }
        }

        /// <summary>
        /// 특정 애니메이션 float 파라미터 설정
        /// </summary>
        public void SetAnimationFloat(string parameterName, float value)
        {
            if (!enableAnimations || animator == null || string.IsNullOrEmpty(parameterName))
                return;

            try
            {
                animator.SetFloat(parameterName, value);

                if (enableDebugLogs)
                {
                    Debug.Log($"[AnimationController] Float 파라미터 설정: {parameterName} = {value:F2}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AnimationController] Float 파라미터 설정 오류 '{parameterName}': {e.Message}");
            }
        }

        /// <summary>
        /// 특정 애니메이션 int 파라미터 설정
        /// </summary>
        public void SetAnimationInt(string parameterName, int value)
        {
            if (!enableAnimations || animator == null || string.IsNullOrEmpty(parameterName))
                return;

            try
            {
                animator.SetInteger(parameterName, value);

                if (enableDebugLogs)
                {
                    Debug.Log($"[AnimationController] Int 파라미터 설정: {parameterName} = {value}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AnimationController] Int 파라미터 설정 오류 '{parameterName}': {e.Message}");
            }
        }

        /// <summary>
        /// 현재 애니메이션 상태 정보 가져오기
        /// </summary>
        public AnimatorStateInfo GetCurrentAnimationState()
        {
            if (animator == null)
                return new AnimatorStateInfo();

            return animator.GetCurrentAnimatorStateInfo(0);
        }

        /// <summary>
        /// 애니메이션이 재생 중인지 확인
        /// </summary>
        public bool IsAnimationPlaying(string stateName)
        {
            if (animator == null || string.IsNullOrEmpty(stateName))
                return false;

            return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
        }

        /// <summary>
        /// 스프라이트 색상 변경
        /// </summary>
        public void SetSpriteColor(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;

                if (enableDebugLogs)
                {
                    Debug.Log($"[AnimationController] 스프라이트 색상 변경: {color}");
                }
            }
        }

        /// <summary>
        /// 스프라이트 투명도 설정
        /// </summary>
        public void SetSpriteAlpha(float alpha)
        {
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Clamp01(alpha);
                spriteRenderer.color = color;

                if (enableDebugLogs)
                {
                    Debug.Log($"[AnimationController] 스프라이트 투명도 설정: {alpha:F2}");
                }
            }
        }

        /// <summary>
        /// 애니메이션 속도 설정
        /// </summary>
        public void SetAnimationSpeed(float speed)
        {
            if (animator != null)
            {
                animator.speed = Mathf.Max(0f, speed);

                if (enableDebugLogs)
                {
                    Debug.Log($"[AnimationController] 애니메이션 속도 설정: {speed:F2}");
                }
            }
        }

        /// <summary>
        /// 디버그 로그 활성화/비활성화
        /// </summary>
        public void SetDebugLogsEnabled(bool enabled)
        {
            enableDebugLogs = enabled;
        }

        /// <summary>
        /// 현재 애니메이션 상태 정보
        /// </summary>
        public string GetAnimationInfo()
        {
            if (animator == null)
                return "No Animator";

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return $"State: {stateInfo.shortNameHash}, Time: {stateInfo.normalizedTime:F2}, Speed: {animator.speed:F2}";
        }
    }
}