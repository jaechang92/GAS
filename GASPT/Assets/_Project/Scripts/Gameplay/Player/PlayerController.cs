using UnityEngine;
using FSM.Core;
using GAS.Core;
using System.Collections.Generic;

namespace Player
{
    /// <summary>
    /// 리팩토링된 플레이어 컨트롤러
    /// 단일책임원칙: FSM 상태 관리 및 컴포넌트 조정만 담당
    /// 모든 물리/입력/애니메이션은 전용 컴포넌트에 위임
    /// </summary>
    [RequireComponent(typeof(AbilitySystem))]
    public class PlayerController : MonoBehaviour
    {
        [Header("디버그")]
        [SerializeField] private bool showDebugInfo = false;

        // === 핵심 컴포넌트 참조 ===
        private AbilitySystem abilitySystem;
        private InputHandler inputHandler;
        private Character.Physics.PhysicsEngine physicsEngine;
        private AnimationController animationController;

        // === FSM 관련 ===
        private StateMachine stateMachine;
        private PlayerStateType previousState = PlayerStateType.Idle;

        // === 방향 관리 ===
        private int facingDirection = 1; // 1: 오른쪽, -1: 왼쪽

        // === 프로퍼티 (PhysicsEngine 위임) ===
        public PlayerStateType CurrentState => GetCurrentState();
        public PlayerStateType PreviousState => previousState;
        public bool IsGrounded => physicsEngine?.IsGrounded ?? false;
        public bool IsTouchingWall => physicsEngine?.IsTouchingWall ?? false;
        public bool CanDash => physicsEngine?.CanDash ?? false;
        public Vector3 Velocity => physicsEngine?.Velocity ?? Vector3.zero;
        public int FacingDirection => facingDirection;
        public bool IsDashing => physicsEngine?.IsDashing ?? false;

        // === 이벤트 ===
        public event System.Action<PlayerStateType, PlayerStateType> OnStateChanged;
        public event System.Action<PlayerEventType> OnPlayerEvent;

        private void Awake()
        {
            InitializeComponents();
            InitializeFSM();
            SetupEventHandlers();
        }

        private void Start()
        {
            StartFSM();
        }

        private void OnDestroy()
        {
            CleanupEventHandlers();
        }

        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        private void InitializeComponents()
        {
            // AbilitySystem (필수)
            abilitySystem = GetComponent<AbilitySystem>();

            // InputHandler 초기화
            inputHandler = GetComponent<InputHandler>();
            if (inputHandler == null)
            {
                inputHandler = gameObject.AddComponent<InputHandler>();
                LogDebug("InputHandler 컴포넌트가 자동으로 추가되었습니다.");
            }

            // PhysicsEngine 초기화
            physicsEngine = GetComponent<Character.Physics.PhysicsEngine>();
            if (physicsEngine == null)
            {
                physicsEngine = gameObject.AddComponent<Character.Physics.PhysicsEngine>();
                LogDebug("PhysicsEngine 컴포넌트가 자동으로 추가되었습니다.");
            }

            // AnimationController 초기화
            animationController = GetComponent<AnimationController>();
            if (animationController == null)
            {
                animationController = gameObject.AddComponent<AnimationController>();
                LogDebug("AnimationController 컴포넌트가 자동으로 추가되었습니다.");
            }
        }

        /// <summary>
        /// FSM 초기화
        /// </summary>
        private void InitializeFSM()
        {
            stateMachine = gameObject.AddComponent<StateMachine>();

            // 상태들 등록
            stateMachine.AddState(new PlayerIdleState(this));
            stateMachine.AddState(new PlayerMoveState(this));
            stateMachine.AddState(new PlayerJumpState(this));
            stateMachine.AddState(new PlayerFallState(this));
            stateMachine.AddState(new PlayerDashState(this));
            stateMachine.AddState(new PlayerAttackState(this));
            stateMachine.AddState(new PlayerHitState(this));
            stateMachine.AddState(new PlayerDeadState(this));
            stateMachine.AddState(new PlayerSlideState(this));
            stateMachine.AddState(new PlayerWallGrabState(this));
            stateMachine.AddState(new PlayerWallJumpState(this));

            // 전환 규칙 등록
            SetupTransitions();

            // 상태 변경 이벤트 구독
            stateMachine.OnStateChanged += OnFSMStateChanged;
        }

        /// <summary>
        /// 상태 전환 규칙 설정
        /// </summary>
        private void SetupTransitions()
        {
            // Idle 상태에서의 전환
            stateMachine.AddTransition("Idle", "Move", "StartMove");
            stateMachine.AddTransition("Idle", "Jump", "JumpPressed");
            stateMachine.AddTransition("Idle", "Attack", "AttackPressed");
            stateMachine.AddTransition("Idle", "Dash", "DashPressed");
            stateMachine.AddTransition("Idle", "Slide", "SlidePressed");

            // Move 상태에서의 전환
            stateMachine.AddTransition("Move", "Idle", "StopMove");
            stateMachine.AddTransition("Move", "Jump", "JumpPressed");
            stateMachine.AddTransition("Move", "Attack", "AttackPressed");
            stateMachine.AddTransition("Move", "Dash", "DashPressed");
            stateMachine.AddTransition("Move", "Slide", "SlidePressed");

            // Jump 상태에서의 전환
            stateMachine.AddTransition("Jump", "Idle", "TouchGround");
            stateMachine.AddTransition("Jump", "Fall", "StartFall");
            stateMachine.AddTransition("Jump", "Dash", "DashPressed");
            stateMachine.AddTransition("Jump", "WallGrab", "TouchWall");

            // Fall 상태에서의 전환
            stateMachine.AddTransition("Fall", "Idle", "TouchGround");
            stateMachine.AddTransition("Fall", "Jump", "JumpPressed");
            stateMachine.AddTransition("Fall", "Dash", "DashPressed");
            stateMachine.AddTransition("Fall", "WallGrab", "TouchWall");

            // Attack 상태에서의 전환
            stateMachine.AddTransition("Attack", "Idle", "AttackFinished");
            stateMachine.AddTransition("Attack", "Move", "StartMove");

            // Dash 상태에서의 전환
            stateMachine.AddTransition("Dash", "Idle", "DashFinished");
            stateMachine.AddTransition("Dash", "Move", "StartMove");

            // WallGrab 상태에서의 전환
            stateMachine.AddTransition("WallGrab", "WallJump", "JumpPressed");
            stateMachine.AddTransition("WallGrab", "Fall", "LeaveWall");
            stateMachine.AddTransition("WallGrab", "Idle", "TouchGround");

            // WallJump 상태에서의 전환
            stateMachine.AddTransition("WallJump", "Fall", "StartFall");
            stateMachine.AddTransition("WallJump", "Idle", "TouchGround");

            // Slide 상태에서의 전환
            stateMachine.AddTransition("Slide", "Idle", "SlideFinished");
            stateMachine.AddTransition("Slide", "Move", "StartMove");

            // Hit 상태에서의 전환
            stateMachine.AddTransition("Hit", "Idle", "HitRecovered");
            stateMachine.AddTransition("Hit", "Dead", "HealthDepleted");

            // 모든 상태에서 Hit 상태로의 전환
            var allStates = new[] { "Idle", "Move", "Jump", "Fall", "Attack", "Dash", "WallGrab", "WallJump", "Slide" };
            foreach (var state in allStates)
            {
                stateMachine.AddTransition(state, "Hit", "TakeDamage");
            }
        }

        /// <summary>
        /// 이벤트 핸들러 설정
        /// </summary>
        private void SetupEventHandlers()
        {
            if (inputHandler == null) return;

            // 입력 이벤트 구독
            inputHandler.OnMovementInput += OnMovementInput;
            inputHandler.OnJumpPressed += OnJumpPressed;
            inputHandler.OnJumpReleased += OnJumpReleased;
            inputHandler.OnDashPressed += OnDashPressed;
            inputHandler.OnAttackPressed += () => TriggerEvent(PlayerEventType.AttackPressed);
            inputHandler.OnSlidePressed += () => { if (IsGrounded) TriggerEvent(PlayerEventType.SlidePressed); };

            // PhysicsEngine 이벤트 구독
            if (physicsEngine != null)
            {
                physicsEngine.OnGroundedChanged += OnGroundedChanged;
                physicsEngine.OnJump += () => TriggerEvent(PlayerEventType.JumpPressed);
            }
        }

        /// <summary>
        /// 이벤트 핸들러 정리
        /// </summary>
        private void CleanupEventHandlers()
        {
            // 입력 이벤트 구독 해제
            if (inputHandler != null)
            {
                inputHandler.OnMovementInput -= OnMovementInput;
                inputHandler.OnJumpPressed -= OnJumpPressed;
                inputHandler.OnJumpReleased -= OnJumpReleased;
                inputHandler.OnDashPressed -= OnDashPressed;
                inputHandler.OnAttackPressed -= () => TriggerEvent(PlayerEventType.AttackPressed);
                inputHandler.OnSlidePressed -= () => { if (IsGrounded) TriggerEvent(PlayerEventType.SlidePressed); };
            }

            // PhysicsEngine 이벤트 구독 해제
            if (physicsEngine != null)
            {
                physicsEngine.OnGroundedChanged -= OnGroundedChanged;
                physicsEngine.OnJump -= () => TriggerEvent(PlayerEventType.JumpPressed);
            }

            // FSM 이벤트 구독 해제
            if (stateMachine != null)
            {
                stateMachine.OnStateChanged -= OnFSMStateChanged;
            }
        }

        /// <summary>
        /// FSM 시작
        /// </summary>
        private void StartFSM()
        {
            stateMachine?.StartStateMachine(PlayerStateType.Idle.ToString());
        }

        /// <summary>
        /// 움직임 입력 처리
        /// </summary>
        private void OnMovementInput(Vector2 input)
        {
            // 방향 업데이트
            if (input.x > 0) facingDirection = 1;
            else if (input.x < 0) facingDirection = -1;

            // PhysicsEngine에 입력 전달
            physicsEngine?.SetHorizontalMovement(input.x);

            // AnimationController에 방향 전달
            animationController?.UpdateFacingDirection(facingDirection);

            // 이동 이벤트 발생
            if (input.magnitude > 0.1f)
                TriggerEvent(PlayerEventType.StartMove);
            else
                TriggerEvent(PlayerEventType.StopMove);
        }

        /// <summary>
        /// 점프 입력 처리
        /// </summary>
        private void OnJumpPressed()
        {
            // PhysicsEngine에 점프 입력 전달
            physicsEngine?.SetJumpInput(true, true);

            // StateMachine에 이벤트 발생
            TriggerEvent(PlayerEventType.JumpPressed);
        }

        /// <summary>
        /// 점프 해제 처리
        /// </summary>
        private void OnJumpReleased()
        {
            // PhysicsEngine에 점프 해제 전달
            physicsEngine?.SetJumpInput(false, false);

            // StateMachine에 이벤트 발생
            TriggerEvent(PlayerEventType.JumpReleased);
        }

        /// <summary>
        /// 대시 입력 처리
        /// </summary>
        private void OnDashPressed()
        {
            // 대시 방향 계산 (현재 향하고 있는 방향)
            Vector2 dashDirection = new Vector2(facingDirection, 0);

            // PhysicsEngine에 대시 실행 요청
            physicsEngine?.PerformDash(dashDirection);

            // StateMachine에 이벤트 발생
            TriggerEvent(PlayerEventType.DashPressed);
        }

        /// <summary>
        /// 접지 상태 변경 처리
        /// </summary>
        private void OnGroundedChanged()
        {
            if (IsGrounded)
            {
                TriggerEvent(PlayerEventType.TouchGround);
            }
            else
            {
                TriggerEvent(PlayerEventType.LeaveGround);
            }
        }

        /// <summary>
        /// FSM 상태 변경 이벤트 처리
        /// </summary>
        private void OnFSMStateChanged(string oldStateId, string newStateId)
        {
            // 이전 상태 저장
            if (System.Enum.TryParse<PlayerStateType>(oldStateId, out var oldState))
            {
                previousState = oldState;
            }

            // 새 상태 확인
            if (System.Enum.TryParse<PlayerStateType>(newStateId, out var newState))
            {
                LogDebug($"상태 변경: {oldState} → {newState}");
                OnStateChanged?.Invoke(oldState, newState);
            }
        }

        /// <summary>
        /// 플레이어 이벤트 발생
        /// </summary>
        public void TriggerEvent(PlayerEventType eventType)
        {
            stateMachine?.TriggerEvent(eventType.ToString());
            OnPlayerEvent?.Invoke(eventType);
        }

        /// <summary>
        /// 상태 변경
        /// </summary>
        public void ChangeState(PlayerStateType newState)
        {
            stateMachine?.ForceTransitionTo(newState.ToString());
        }

        /// <summary>
        /// 현재 상태 가져오기 (간소화)
        /// </summary>
        private PlayerStateType GetCurrentState()
        {
            if (stateMachine?.CurrentStateId != null &&
                System.Enum.TryParse<PlayerStateType>(stateMachine.CurrentStateId, out var state))
            {
                return state;
            }
            return PlayerStateType.Idle;
        }

        /// <summary>
        /// 속도 직접 설정 (PhysicsEngine으로 위임)
        /// </summary>
        public void SetVelocity(Vector3 newVelocity)
        {
            physicsEngine?.SetVelocity(newVelocity);
        }

        /// <summary>
        /// 디버그 로그 (조건부)
        /// </summary>
        private void LogDebug(string message)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[PlayerController] {message}");
            }
        }

        /// <summary>
        /// GAS 어빌리티 활성화
        /// </summary>
        public void ActivateAbility(string abilityId)
        {
            abilitySystem?.ActivateAbility(abilityId);
        }

        /// <summary>
        /// GAS 어빌리티 비활성화
        /// </summary>
        public void DeactivateAbility(string abilityId)
        {
            abilitySystem?.DeactivateAbility(abilityId);
        }
    }
}