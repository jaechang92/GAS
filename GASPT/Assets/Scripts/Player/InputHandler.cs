using UnityEngine;
using System;

namespace Player
{
    /// <summary>
    /// 플레이어 입력 처리 전용 클래스
    /// 단일책임원칙: 입력 감지 및 이벤트 발생만 담당
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [Header("입력 설정")]
        [SerializeField] private bool enableInput = true;

        // 입력 상태
        private Vector2 inputVector;
        private bool jumpPressed;
        private bool dashPressed;
        private bool attackPressed;
        private bool slidePressed;

        // 입력 기반 이동 상태 추적
        private bool wasMovingInput = false;

        // 입력 이벤트
        public event Action<Vector2> OnMovementInput;
        public event Action OnJumpPressed;
        public event Action OnJumpReleased;
        public event Action OnDashPressed;
        public event Action OnAttackPressed;
        public event Action OnSlidePressed;
        public event Action OnStartMove;
        public event Action OnStopMove;

        // 프로퍼티
        public Vector2 InputVector => inputVector;
        public bool IsJumpPressed => jumpPressed;
        public bool IsDashPressed => dashPressed;
        public bool IsAttackPressed => attackPressed;
        public bool IsSlidePressed => slidePressed;

        private void Update()
        {
            if (!enableInput) return;

            HandleMovementInput();
            HandleJumpInput();
            HandleDashInput();
            HandleAttackInput();
            HandleSlideInput();
            HandleMovementState();
        }

        /// <summary>
        /// 이동 입력 처리
        /// </summary>
        private void HandleMovementInput()
        {
            Vector2 previousInput = inputVector;
            inputVector.x = Input.GetAxisRaw("Horizontal");

            if (previousInput != inputVector)
            {
                OnMovementInput?.Invoke(inputVector);
            }
        }

        /// <summary>
        /// 점프 입력 처리
        /// </summary>
        private void HandleJumpInput()
        {
            bool jumpInput = Input.GetButtonDown("Jump");
            if (jumpInput && !jumpPressed)
            {
                jumpPressed = true;
                OnJumpPressed?.Invoke();
            }
            else if (Input.GetButtonUp("Jump"))
            {
                jumpPressed = false;
                OnJumpReleased?.Invoke();
            }
        }

        /// <summary>
        /// 대시 입력 처리
        /// </summary>
        private void HandleDashInput()
        {
            bool dashInput = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
            if (dashInput && !dashPressed)
            {
                dashPressed = true;
                OnDashPressed?.Invoke();
            }
        }

        /// <summary>
        /// 공격 입력 처리
        /// </summary>
        private void HandleAttackInput()
        {
            bool attackInput = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.X);
            if (attackInput && !attackPressed)
            {
                attackPressed = true;
                OnAttackPressed?.Invoke();
            }
        }

        /// <summary>
        /// 슬라이딩 입력 처리
        /// </summary>
        private void HandleSlideInput()
        {
            bool slideInput = Input.GetKeyDown(KeyCode.S) && Mathf.Abs(inputVector.x) > 0;
            if (slideInput && !slidePressed)
            {
                slidePressed = true;
                OnSlidePressed?.Invoke();
            }
        }

        /// <summary>
        /// 이동 상태 변화 감지 및 이벤트 발생
        /// </summary>
        private void HandleMovementState()
        {
            bool hasMovementInput = Mathf.Abs(inputVector.x) > 0.1f;

            if (hasMovementInput && !wasMovingInput)
            {
                // 이동 입력 시작
                OnStartMove?.Invoke();
                wasMovingInput = true;
            }
            else if (!hasMovementInput && wasMovingInput)
            {
                // 이동 입력 중단
                OnStopMove?.Invoke();
                wasMovingInput = false;
            }
        }

        /// <summary>
        /// 입력 상태 리셋
        /// </summary>
        public void ResetJump()
        {
            jumpPressed = false;
        }

        public void ResetDash()
        {
            dashPressed = false;
        }

        public void ResetAttack()
        {
            attackPressed = false;
        }

        public void ResetSlide()
        {
            slidePressed = false;
        }

        /// <summary>
        /// 모든 입력 상태 리셋
        /// </summary>
        public void ResetAllInputs()
        {
            jumpPressed = false;
            dashPressed = false;
            attackPressed = false;
            slidePressed = false;
        }

        /// <summary>
        /// 입력 활성화/비활성화
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            enableInput = enabled;
            if (!enabled)
            {
                inputVector = Vector2.zero;
                ResetAllInputs();
            }
        }
    }
}