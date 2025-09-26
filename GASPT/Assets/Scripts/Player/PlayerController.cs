using UnityEngine;
using FSM.Core;
using GAS.Core;
using System.Collections.Generic;

namespace Player
{
    /// <summary>
    /// FSM과 GAS를 통합한 플레이어 컨트롤러 (컴포넌트 조합 패턴)
    /// 단일책임원칙 준수: FSM 상태 관리 및 컴포넌트 조합만 담당
    /// - InputHandler: 입력 처리
    /// - PhysicsController: 물리 시스템
    /// - EnvironmentChecker: 환경 검사
    /// - AnimationController: 애니메이션 제어
    /// </summary>
    [RequireComponent(typeof(AbilitySystem))]
    public class PlayerController : MonoBehaviour
    {
        [Header("플레이어 설정")]
        [SerializeField] private PlayerStats playerStats;

        [Header("완전 커스텀 물리 설정")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 15f;
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float gravity = 30f;
        [SerializeField] private float maxFallSpeed = 20f;
        [SerializeField] private float airMoveSpeed = 6f;


        [Header("접지 검사")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.1f; // 0.2f에서 0.1f로 축소
        [SerializeField] private LayerMask groundLayerMask = 1;


        [Header("디버그")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool showDetailedLogs = false; // 상세 로그 (매 프레임 정보 등)

        // 컴포넌트 참조 (SRP 준수)
        private AbilitySystem abilitySystem;
        private Collider2D playerCollider;
        private GroundChecker groundChecker; // GroundChecker 컴포넌트
        private InputHandler inputHandler; // 입력 처리 전용 컴포넌트
        private PhysicsController physicsController; // 물리 시스템 전용 컴포넌트
        private EnvironmentChecker environmentChecker; // 환경 검사 전용 컴포넌트
        private AnimationController animationController; // 애니메이션 제어 전용 컴포넌트


        // FSM 관련
        private StateMachine stateMachine;
        private Dictionary<PlayerEventType, System.Action> eventHandlers;
        private PlayerStateType previousState = PlayerStateType.Idle; // 이전 상태 추적


        // 방향 정보 (EnvironmentChecker와 동기화됨)
        private int facingDirection = 1; // 1: 오른쪽, -1: 왼쪽



        // 프로퍼티
        public PlayerStateType CurrentState
        {
            get
            {
                if (stateMachine == null)
                {
                    if (showDetailedLogs)
                        Debug.LogWarning("[CurrentState] StateMachine이 null입니다!");
                    return PlayerStateType.Idle;
                }

                string currentStateId = stateMachine.CurrentStateId;
                if (string.IsNullOrEmpty(currentStateId))
                {
                    if (showDetailedLogs)
                        Debug.LogWarning("[CurrentState] CurrentStateId가 null 또는 empty입니다!");
                    return PlayerStateType.Idle;
                }

                if (System.Enum.TryParse<PlayerStateType>(currentStateId, out var state))
                {
                    return state;
                }
                else
                {
                    if (showDetailedLogs)
                        Debug.LogWarning($"[CurrentState] '{currentStateId}'를 PlayerStateType으로 파싱할 수 없습니다!");
                    return PlayerStateType.Idle;
                }
            }
        }

        public bool IsGrounded => environmentChecker != null ? environmentChecker.IsGrounded : false;
        public bool IsTouchingWall => environmentChecker != null ? environmentChecker.IsTouchingWall : false;
        public PlayerStateType PreviousState => previousState; // 이전 상태 프로퍼티
        public bool CanDash => environmentChecker != null ? environmentChecker.CanDash : true;
        public int FacingDirection => environmentChecker != null ? environmentChecker.FacingDirection : facingDirection;
        public Vector3 Velocity => physicsController != null ? physicsController.Velocity : Vector3.zero;
        public float PreviousHorizontalSpeed => physicsController != null ? physicsController.PreviousHorizontalSpeed : 0f;

        // 커스텀 물리 접근자
        public float MoveSpeed => moveSpeed;
        public float JumpForce => jumpForce;
        public float Gravity => gravity;
        public float MaxFallSpeed => maxFallSpeed;
        public float AirMoveSpeed => airMoveSpeed;

        // 이벤트
        public event System.Action<PlayerStateType, PlayerStateType> OnStateChanged;
        public event System.Action<PlayerEventType> OnPlayerEvent;

        private void Awake()
        {
            InitializeLayer();
            InitializeComponents();
            InitializeStateMachine();
            InitializeEventHandlers();
        }

        private void Start()
        {
            InitializeStats();

            // 초기 상태로 시작
            Debug.Log($"[PlayerController] StateMachine 시작 - 초기 상태: {PlayerStateType.Idle}");
            stateMachine.StartStateMachine(PlayerStateType.Idle.ToString());

            // 초기화 후 상태 확인
            Debug.Log($"[PlayerController] 초기화 후 현재 상태: {CurrentState}, StateMachine ID: '{stateMachine?.CurrentStateId}'");
        }

        private void Update()
        {
            // 컴포넌트별 업데이트는 각 컴포넌트에서 자동 처리됨

            // 애니메이션 업데이트 (AnimationController로 위임)
            if (animationController != null)
            {
                animationController.UpdateAnimationParameters(
                    Velocity.magnitude,
                    Velocity.y,
                    IsGrounded,
                    IsTouchingWall
                );
                animationController.UpdateFacingDirection(FacingDirection);
            }

            if (showDebugInfo)
            {
                DrawDebugInfo();
            }

            // 현재 상태 지속적 모니터링 (문제 진단용)
            if (showDetailedLogs && Time.frameCount % 60 == 0) // 1초마다
            {
                Debug.Log($"[Monitor] 현재 상태: {CurrentState}, StateMachine ID: '{stateMachine?.CurrentStateId}', 입력: {GetInputVector().x:F2}, 접지: {IsGrounded}");
            }
        }

        // FixedUpdate 및 물리 계산은 PhysicsController에서 처리됨

        private void InitializeLayer()
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
            groundLayerMask = LayerMask.GetMask("Ground", "Platform");
        }

        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        private void InitializeComponents()
        {
            abilitySystem = GetComponent<AbilitySystem>();
            playerCollider = GetComponent<Collider2D>();

            // GroundChecker 초기화
            groundChecker = GetComponent<GroundChecker>();
            if (groundChecker == null)
            {
                groundChecker = gameObject.AddComponent<GroundChecker>();
                if (showDebugInfo)
                {
                    Debug.Log("[PlayerController] GroundChecker 컴포넌트가 자동으로 추가되었습니다.");
                }
            }

            // GroundChecker 이벤트 구독
            groundChecker.OnTouchGround += HandleGroundTouchEvent;
            groundChecker.OnLeaveGround += HandleGroundLeaveEvent;

            // InputHandler 초기화
            inputHandler = GetComponent<InputHandler>();
            if (inputHandler == null)
            {
                inputHandler = gameObject.AddComponent<InputHandler>();
                if (showDebugInfo)
                {
                    Debug.Log("[PlayerController] InputHandler 컴포넌트가 자동으로 추가되었습니다.");
                }
            }

            // InputHandler 이벤트 구독
            inputHandler.OnJumpPressed += () => { if (CanDash) TriggerEvent(PlayerEventType.JumpPressed); };
            inputHandler.OnJumpReleased += () => TriggerEvent(PlayerEventType.JumpReleased);
            inputHandler.OnDashPressed += () => { if (CanDash) TriggerEvent(PlayerEventType.DashPressed); };
            inputHandler.OnAttackPressed += () => TriggerEvent(PlayerEventType.AttackPressed);
            inputHandler.OnSlidePressed += () => { if (IsGrounded) TriggerEvent(PlayerEventType.SlidePressed); };
            inputHandler.OnStartMove += () => {
                TriggerEvent(PlayerEventType.StartMove);
                if (showDebugInfo) Debug.Log($"[Input] StartMove 이벤트 발생 - 입력: {inputHandler.InputVector.x:F2}");
            };
            inputHandler.OnStopMove += () => {
                TriggerEvent(PlayerEventType.StopMove);
                if (showDebugInfo) Debug.Log($"[Input] StopMove 이벤트 발생 - 입력: {inputHandler.InputVector.x:F2}");
            };
            inputHandler.OnMovementInput += (input) => {
                // 방향 업데이트
                if (input.x > 0) facingDirection = 1;
                else if (input.x < 0) facingDirection = -1;

                // EnvironmentChecker와 AnimationController에 방향 전달
                environmentChecker?.UpdateFacingDirection(facingDirection);
                animationController?.UpdateFacingDirection(facingDirection);
            };

            // Ground Check 설정 (GroundChecker에서 사용)
            if (groundCheck == null)
            {
                GameObject groundCheckGO = new GameObject("GroundCheck");
                groundCheckGO.transform.SetParent(transform);
                // 플레이어 콜라이더 하단에서 아주 조금만 아래로
                float yOffset = playerCollider != null ? -playerCollider.bounds.extents.y - 0.05f : -0.9f;
                groundCheckGO.transform.localPosition = new Vector3(0, yOffset, 0);
                groundCheck = groundCheckGO.transform;

                if (showDebugInfo)
                {
                    Debug.Log($"[PlayerController] GroundCheck 생성됨: {groundCheckGO.transform.localPosition}");
                }
            }

            // PhysicsController 초기화
            physicsController = GetComponent<PhysicsController>();
            if (physicsController == null)
            {
                physicsController = gameObject.AddComponent<PhysicsController>();
                if (showDebugInfo)
                {
                    Debug.Log("[PlayerController] PhysicsController 컴포넌트가 자동으로 추가되었습니다.");
                }
            }

            // PhysicsController 이벤트 구독
            physicsController.OnVelocityChanged += (newVelocity) =>
            {
                if (showDetailedLogs)
                {
                    Debug.Log($"[Physics] 속도 변경: {newVelocity}");
                }
            };

            // EnvironmentChecker 초기화
            environmentChecker = GetComponent<EnvironmentChecker>();
            if (environmentChecker == null)
            {
                environmentChecker = gameObject.AddComponent<EnvironmentChecker>();
                if (showDebugInfo)
                {
                    Debug.Log("[PlayerController] EnvironmentChecker 컴포넌트가 자동으로 추가되었습니다.");
                }
            }

            // EnvironmentChecker 이벤트 구독
            environmentChecker.OnTouchWall += () => TriggerEvent(PlayerEventType.TouchWall);
            environmentChecker.OnLeaveWall += () => TriggerEvent(PlayerEventType.LeaveWall);
            environmentChecker.OnDashAvailable += () =>
            {
                if (showDebugInfo)
                    Debug.Log("[Environment] 대시 사용 가능");
            };

            // AnimationController 초기화
            animationController = GetComponent<AnimationController>();
            if (animationController == null)
            {
                animationController = gameObject.AddComponent<AnimationController>();
                if (showDebugInfo)
                {
                    Debug.Log("[PlayerController] AnimationController 컴포넌트가 자동으로 추가되었습니다.");
                }
            }

            // AnimationController 이벤트 구독
            animationController.OnDirectionChanged += (direction) =>
            {
                if (showDetailedLogs)
                {
                    Debug.Log($"[Animation] 방향 변경: {direction}");
                }
            };
            animationController.OnAnimationStateChanged += (stateName) =>
            {
                if (showDetailedLogs)
                {
                    Debug.Log($"[Animation] 애니메이션 상태 변경: {stateName}");
                }
            };
        }


        /// <summary>
        /// FSM 초기화
        /// </summary>
        private void InitializeStateMachine()
        {
            stateMachine = gameObject.GetComponent<StateMachine>();
            if (stateMachine == null) stateMachine = gameObject.AddComponent<StateMachine>();

            // 플레이어 상태들 추가
            stateMachine.AddState(new PlayerIdleState());
            stateMachine.AddState(new PlayerMoveState());
            stateMachine.AddState(new PlayerJumpState());
            stateMachine.AddState(new PlayerFallState());
            stateMachine.AddState(new PlayerDashState());
            stateMachine.AddState(new PlayerAttackState());
            stateMachine.AddState(new PlayerHitState());
            stateMachine.AddState(new PlayerDeadState());
            stateMachine.AddState(new PlayerWallGrabState());
            stateMachine.AddState(new PlayerWallJumpState());
            stateMachine.AddState(new PlayerSlideState());

            // 상태 전환 설정
            PlayerStateTransitions.SetupTransitions(stateMachine, this);

            // 상태 변경 이벤트 구독
            stateMachine.OnStateChanged += OnPlayerStateChanged;
        }

        /// <summary>
        /// 이벤트 핸들러 초기화
        /// </summary>
        private void InitializeEventHandlers()
        {
            eventHandlers = new Dictionary<PlayerEventType, System.Action>
            {
                { PlayerEventType.StartMove, () => TriggerStateTransition(PlayerEventType.StartMove) },
                { PlayerEventType.StopMove, () => TriggerStateTransition(PlayerEventType.StopMove) },
                { PlayerEventType.JumpPressed, () => TriggerStateTransition(PlayerEventType.JumpPressed) },
                { PlayerEventType.DashPressed, () => TriggerStateTransition(PlayerEventType.DashPressed) },
                { PlayerEventType.AttackPressed, () => TriggerStateTransition(PlayerEventType.AttackPressed) },
                { PlayerEventType.TouchGround, () => TriggerStateTransition(PlayerEventType.TouchGround) },
                { PlayerEventType.LeaveGround, () => TriggerStateTransition(PlayerEventType.LeaveGround) },
                { PlayerEventType.TouchWall, () => TriggerStateTransition(PlayerEventType.TouchWall) },
                { PlayerEventType.LeaveWall, () => TriggerStateTransition(PlayerEventType.LeaveWall) }
            };
        }

        /// <summary>
        /// 플레이어 스탯 초기화
        /// </summary>
        private void InitializeStats()
        {
            if (playerStats != null)
            {
                moveSpeed = playerStats.moveSpeed;
                jumpForce = playerStats.jumpForce;
                dashSpeed = playerStats.dashSpeed;
                dashDuration = playerStats.dashDuration;
            }
        }

        // 입력, 환경검사, 애니메이션 처리는 각각 전용 컴포넌트에서 처리됨

        /// <summary>
        /// 이벤트 트리거
        /// </summary>
        public void TriggerEvent(PlayerEventType eventType)
        {
            OnPlayerEvent?.Invoke(eventType);

            if (eventHandlers.TryGetValue(eventType, out var handler))
            {
                handler?.Invoke();
            }

            // 착지 관련 이벤트만 특별 디버그
            if (showDebugInfo && (eventType == PlayerEventType.TouchGround || eventType == PlayerEventType.LeaveGround))
            {
                Debug.Log($"[PlayerController] 중요 이벤트 발생: {eventType} (현재 상태: {CurrentState}, 접지: {IsGrounded})");
            }
            else if (showDebugInfo && (eventType == PlayerEventType.JumpPressed || eventType == PlayerEventType.DashPressed || eventType == PlayerEventType.AttackPressed))
            {
                // 주요 액션 이벤트만 로그
                Debug.Log($"[PlayerController] 액션 이벤트: {eventType}");
            }
            // 나머지 이벤트는 로그 생략 (StartMove, StopMove 등의 빈번한 이벤트)
        }

        /// <summary>
        /// 상태 전환 트리거
        /// </summary>
        private void TriggerStateTransition(PlayerEventType eventType)
        {
            if (stateMachine != null)
            {
                if (showDetailedLogs)
                {
                    Debug.Log($"[StateTransition] 이벤트 트리거: {eventType} (현재: {CurrentState})");
                }
                stateMachine.TriggerEvent(eventType.ToString());
            }
            else
            {
                Debug.LogError("[StateTransition] StateMachine이 null입니다!");
            }
        }

        /// <summary>
        /// 플레이어 상태 변경 이벤트 핸들러
        /// </summary>
        private void OnPlayerStateChanged(string fromState, string toState)
        {
            Debug.Log($"[FSM] Raw 상태 변경: '{fromState}' → '{toState}'");

            if (System.Enum.TryParse<PlayerStateType>(fromState, out var from) &&
                System.Enum.TryParse<PlayerStateType>(toState, out var to))
            {
                // 이전 상태 저장
                previousState = from;

                OnStateChanged?.Invoke(from, to);
                Debug.Log($"[PlayerController] 상태 변경: {from} → {to} (이전 상태: {previousState})");
            }
            else
            {
                Debug.LogWarning($"[PlayerController] 상태 파싱 실패: '{fromState}' → '{toState}'");
            }
        }

        // 공개 메서드들 (InputHandler로 위임)
        public Vector2 GetInputVector() => inputHandler != null ? inputHandler.InputVector : Vector2.zero;
        public bool IsJumpPressed() => inputHandler != null ? inputHandler.IsJumpPressed : false;
        public bool IsDashPressed() => inputHandler != null ? inputHandler.IsDashPressed : false;
        public bool IsAttackPressed() => inputHandler != null ? inputHandler.IsAttackPressed : false;

        public bool IsMoving() => Mathf.Abs(Velocity.x) > 0.1f;

        public void ResetDash()
        {
            environmentChecker?.ResetDashCooldown();
            inputHandler?.ResetDash();
        }

        public void StartDash()
        {
            environmentChecker?.StartDashCooldown();
        }

        public void ResetJump()
        {
            inputHandler?.ResetJump();
        }

        public void ResetAttack()
        {
            inputHandler?.ResetAttack();
        }

        public void ResetSlide()
        {
            inputHandler?.ResetSlide();
        }

        /// <summary>
        /// 강제로 접지 상태 설정 (EnvironmentChecker로 위임)
        /// </summary>
        public void ForceSetGrounded(bool grounded)
        {
            environmentChecker?.ForceSetGrounded(grounded);
            if (showDebugInfo)
            {
                Debug.Log($"[PlayerController] 강제 접지 상태 변경: {IsGrounded} → {grounded}");
            }
        }

        /// <summary>
        /// 커스텀 물리 - 속도 직접 설정 (PhysicsController로 위임)
        /// </summary>
        public void SetVelocity(Vector3 newVelocity)
        {
            physicsController?.SetVelocity(newVelocity);
        }

        /// <summary>
        /// 커스텀 물리 - 중력 적용 (조건부) (PhysicsController로 위임)
        /// </summary>
        public void ApplyGravity(float multiplier = 1f)
        {
            physicsController?.ApplyGravity(IsGrounded, multiplier);
        }

        /// <summary>
        /// 강제 중력 적용 (접지 상태 무시) (PhysicsController로 위임)
        /// </summary>
        public void ForceApplyGravity(float multiplier = 1f)
        {
            physicsController?.ForceApplyGravity(multiplier);
        }

        /// <summary>
        /// 커스텀 물리 - 수평 이동 설정 (PhysicsController로 위임)
        /// </summary>
        public void SetHorizontalMovement(float horizontalInput, float speed)
        {
            physicsController?.SetHorizontalMovement(horizontalInput, speed);
        }

        /// <summary>
        /// 커스텀 물리 - 점프 적용 (PhysicsController로 위임)
        /// </summary>
        public void ApplyJump(float jumpPower)
        {
            physicsController?.ApplyJump(jumpPower);
        }


        /// <summary>
        /// GroundChecker에서 착지 이벤트 처리
        /// </summary>
        private void HandleGroundTouchEvent()
        {
            // 착지 시 수직 속도 0 (PhysicsController에 위임)
            physicsController?.HandleGroundTouch();

            if (showDebugInfo)
            {
                Debug.Log($"[GroundChecker] 착지 이벤트 수신! 상태: {CurrentState}");
            }

            // PlayerEventType.TouchGround 이벤트 발생
            TriggerEvent(PlayerEventType.TouchGround);

            // 착지 시 상태 전환 처리
            HandleLandingStateTransition();
        }

        /// <summary>
        /// GroundChecker에서 이륙 이벤트 처리
        /// </summary>
        private void HandleGroundLeaveEvent()
        {
            if (showDebugInfo)
            {
                Debug.Log($"[GroundChecker] 이륙 이벤트 수신! 상태: {CurrentState}");
            }

            // PlayerEventType.LeaveGround 이벤트 발생
            TriggerEvent(PlayerEventType.LeaveGround);
        }

        /// <summary>
        /// 착지 시 상태 전환 처리
        /// </summary>
        private void HandleLandingStateTransition()
        {
            // 현재 입력 상태 확인
            bool hasMovementInput = Mathf.Abs(GetInputVector().x) > 0.1f;

            if (hasMovementInput)
            {
                // 이동 입력이 있으면 Move 상태로
                if (showDebugInfo)
                {
                    Debug.Log($"[Landing] Move 상태로 전환 - 입력: {GetInputVector().x:F2}");
                }
                stateMachine?.ForceTransitionTo(PlayerStateType.Move.ToString());
            }
            else
            {
                // 이동 입력이 없으면 Idle 상태로
                if (showDebugInfo)
                {
                    Debug.Log($"[Landing] Idle 상태로 전환 - 입력 없음");
                }
                stateMachine?.ForceTransitionTo(PlayerStateType.Idle.ToString());
            }
        }

        /// <summary>
        /// TODO: 향후 정교한 착지 물리 시스템에서 사용 예정
        /// 착지 시 운동량 보존을 위한 메서드 (PhysicsController로 위임)
        /// </summary>
        public void PreserveLandingMomentum()
        {
            physicsController?.PreserveLandingMomentum();
        }


        /// <summary>
        /// 디버그 정보 그리기
        /// </summary>
        private void DrawDebugInfo()
        {
            // 환경 검사 디버그는 각 컴포넌트에서 처리됨
        }

        private void OnDrawGizmosSelected()
        {
            // Ground Check 디버그 그리기
            if (groundCheck != null)
            {
                Gizmos.color = IsGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }

        private void OnDestroy()
        {
            if (stateMachine != null)
            {
                stateMachine.OnStateChanged -= OnPlayerStateChanged;
            }

            // GroundChecker 이벤트 구독 해제
            if (groundChecker != null)
            {
                groundChecker.OnTouchGround -= HandleGroundTouchEvent;
                groundChecker.OnLeaveGround -= HandleGroundLeaveEvent;
            }

            // InputHandler 이벤트 구독 해제
            if (inputHandler != null)
            {
                inputHandler.OnJumpPressed -= () => { if (CanDash) TriggerEvent(PlayerEventType.JumpPressed); };
                inputHandler.OnJumpReleased -= () => TriggerEvent(PlayerEventType.JumpReleased);
                inputHandler.OnDashPressed -= () => { if (CanDash) TriggerEvent(PlayerEventType.DashPressed); };
                inputHandler.OnAttackPressed -= () => TriggerEvent(PlayerEventType.AttackPressed);
                inputHandler.OnSlidePressed -= () => { if (IsGrounded) TriggerEvent(PlayerEventType.SlidePressed); };
                // StartMove, StopMove, MovementInput 이벤트도 필요 시 해제
            }

            // EnvironmentChecker 이벤트 구독 해제
            if (environmentChecker != null)
            {
                environmentChecker.OnTouchWall -= () => TriggerEvent(PlayerEventType.TouchWall);
                environmentChecker.OnLeaveWall -= () => TriggerEvent(PlayerEventType.LeaveWall);
                // OnDashAvailable 이벤트도 필요 시 해제
            }

            // AnimationController 이벤트 구독 해제
            if (animationController != null)
            {
                // 필요시 AnimationController 이벤트 구독 해제 코드 추가
            }
        }
    }
}
