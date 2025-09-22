using UnityEngine;
using FSM.Core;
using GAS.Core;
using System.Collections.Generic;

namespace Player
{
    /// <summary>
    /// FSM과 GAS를 통합한 플레이어 컨트롤러
    /// 플레이어의 모든 행동과 상태를 관리
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(AbilitySystem))]
    public class PlayerController : MonoBehaviour
    {
        [Header("플레이어 설정")]
        [SerializeField] private PlayerStats playerStats;

        [Header("물리 설정")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 15f;
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private float dashDuration = 0.2f;

        [Header("접지 검사")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayerMask = 1;

        [Header("벽 검사")]
        [SerializeField] private Transform wallCheck;
        [SerializeField] private float wallCheckDistance = 0.5f;

        [Header("디버그")]
        [SerializeField] private bool showDebugInfo = true;

        // 컴포넌트 참조
        private Rigidbody2D rb;
        private Collider2D col;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private AbilitySystem abilitySystem;

        // FSM 관련
        private StateMachine stateMachine;
        private Dictionary<PlayerEventType, System.Action> eventHandlers;

        // 입력 관련
        private Vector2 inputVector;
        private bool jumpPressed;
        private bool dashPressed;
        private bool attackPressed;
        private bool slidePressed;

        // 상태 정보
        private bool isGrounded;
        private bool isTouchingWall;
        private bool canDash = true;
        private float lastDashTime;
        private int facingDirection = 1; // 1: 오른쪽, -1: 왼쪽

        // 프로퍼티
        public PlayerStateType CurrentState =>
            System.Enum.TryParse<PlayerStateType>(stateMachine?.CurrentStateId, out var state) ? state : PlayerStateType.Idle;

        public bool IsGrounded => isGrounded;
        public bool IsTouchingWall => isTouchingWall;
        public bool CanDash => canDash;
        public int FacingDirection => facingDirection;
        public Vector2 Velocity => rb.linearVelocity;

        // 이벤트
        public event System.Action<PlayerStateType, PlayerStateType> OnStateChanged;
        public event System.Action<PlayerEventType> OnPlayerEvent;

        private void Awake()
        {
            InitializeComponents();
            InitializeStateMachine();
            InitializeEventHandlers();
        }

        private void Start()
        {
            InitializeStats();

            // 초기 상태로 시작
            stateMachine.StartStateMachine(PlayerStateType.Idle.ToString());
        }

        private void Update()
        {
            HandleInput();
            CheckEnvironment();
            UpdateAnimations();

            if (showDebugInfo)
            {
                DrawDebugInfo();
            }
        }

        private void FixedUpdate()
        {
            // 물리 업데이트는 각 상태에서 처리
        }

        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        private void InitializeComponents()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            abilitySystem = GetComponent<AbilitySystem>();

            // Ground Check 설정
            if (groundCheck == null)
            {
                GameObject groundCheckGO = new GameObject("GroundCheck");
                groundCheckGO.transform.SetParent(transform);
                groundCheckGO.transform.localPosition = new Vector3(0, -col.bounds.extents.y, 0);
                groundCheck = groundCheckGO.transform;
            }

            // Wall Check 설정
            if (wallCheck == null)
            {
                GameObject wallCheckGO = new GameObject("WallCheck");
                wallCheckGO.transform.SetParent(transform);
                wallCheckGO.transform.localPosition = new Vector3(col.bounds.extents.x, 0, 0);
                wallCheck = wallCheckGO.transform;
            }
        }

        /// <summary>
        /// FSM 초기화
        /// </summary>
        private void InitializeStateMachine()
        {
            stateMachine = gameObject.AddComponent<StateMachine>();

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

        /// <summary>
        /// 입력 처리
        /// </summary>
        private void HandleInput()
        {
            // 이동 입력
            inputVector.x = Input.GetAxisRaw("Horizontal");

            // 점프 입력
            bool jumpInput = Input.GetButtonDown("Jump");
            if (jumpInput && !jumpPressed)
            {
                jumpPressed = true;
                TriggerEvent(PlayerEventType.JumpPressed);
            }
            else if (Input.GetButtonUp("Jump"))
            {
                jumpPressed = false;
                TriggerEvent(PlayerEventType.JumpReleased);
            }

            // 대시 입력 (Shift 키)
            bool dashInput = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
            if (dashInput && !dashPressed && CanDash)
            {
                dashPressed = true;
                TriggerEvent(PlayerEventType.DashPressed);
            }

            // 공격 입력 (마우스 왼쪽 버튼 또는 X 키)
            bool attackInput = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.X);
            if (attackInput && !attackPressed)
            {
                attackPressed = true;
                TriggerEvent(PlayerEventType.AttackPressed);
            }

            // 슬라이딩 입력 (S 키 + 이동)
            bool slideInput = Input.GetKeyDown(KeyCode.S) && Mathf.Abs(inputVector.x) > 0;
            if (slideInput && !slidePressed && isGrounded)
            {
                slidePressed = true;
                TriggerEvent(PlayerEventType.SlidePressed);
            }

            // 이동 상태 체크
            if (Mathf.Abs(inputVector.x) > 0.1f)
            {
                if (!IsMoving())
                {
                    TriggerEvent(PlayerEventType.StartMove);
                }
            }
            else
            {
                if (IsMoving())
                {
                    TriggerEvent(PlayerEventType.StopMove);
                }
            }

            // 방향 업데이트
            if (inputVector.x > 0)
                facingDirection = 1;
            else if (inputVector.x < 0)
                facingDirection = -1;
        }

        /// <summary>
        /// 환경 검사 (접지, 벽 접촉)
        /// </summary>
        private void CheckEnvironment()
        {
            // 접지 검사
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

            if (!wasGrounded && isGrounded)
            {
                TriggerEvent(PlayerEventType.TouchGround);
            }
            else if (wasGrounded && !isGrounded)
            {
                TriggerEvent(PlayerEventType.LeaveGround);
            }

            // 벽 접촉 검사
            bool wasTouchingWall = isTouchingWall;
            Vector2 wallCheckPos = wallCheck.position;
            wallCheckPos.x += wallCheckDistance * facingDirection;

            isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDirection, wallCheckDistance, groundLayerMask);

            if (!wasTouchingWall && isTouchingWall)
            {
                TriggerEvent(PlayerEventType.TouchWall);
            }
            else if (wasTouchingWall && !isTouchingWall)
            {
                TriggerEvent(PlayerEventType.LeaveWall);
            }

            // 대시 쿨다운 관리
            if (!canDash && Time.time - lastDashTime > 1f)
            {
                canDash = true;
            }
        }

        /// <summary>
        /// 애니메이션 업데이트
        /// </summary>
        private void UpdateAnimations()
        {
            if (animator == null) return;

            // 기본 애니메이션 파라미터 설정
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetFloat("VelocityY", rb.linearVelocity.y);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsTouchingWall", isTouchingWall);

            // 스프라이트 방향 설정
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = facingDirection < 0;
            }
        }

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

            Debug.Log($"[PlayerController] 이벤트 발생: {eventType}");
        }

        /// <summary>
        /// 상태 전환 트리거
        /// </summary>
        private void TriggerStateTransition(PlayerEventType eventType)
        {
            if (stateMachine != null)
            {
                stateMachine.TriggerEvent(eventType.ToString());
            }
        }

        /// <summary>
        /// 플레이어 상태 변경 이벤트 핸들러
        /// </summary>
        private void OnPlayerStateChanged(string fromState, string toState)
        {
            if (System.Enum.TryParse<PlayerStateType>(fromState, out var from) &&
                System.Enum.TryParse<PlayerStateType>(toState, out var to))
            {
                OnStateChanged?.Invoke(from, to);
                Debug.Log($"[PlayerController] 상태 변경: {from} → {to}");
            }
        }

        // 공개 메서드들
        public Vector2 GetInputVector() => inputVector;
        public bool IsJumpPressed() => jumpPressed;
        public bool IsDashPressed() => dashPressed;
        public bool IsAttackPressed() => attackPressed;

        public bool IsMoving() => Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        public void ResetDash()
        {
            canDash = true;
            dashPressed = false;
        }

        public void StartDash()
        {
            canDash = false;
            lastDashTime = Time.time;
        }

        public void ResetJump()
        {
            jumpPressed = false;
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
        /// 디버그 정보 그리기
        /// </summary>
        private void DrawDebugInfo()
        {
            if (groundCheck != null)
            {
                Debug.DrawWireSphere(groundCheck.position, groundCheckRadius, isGrounded ? Color.green : Color.red);
            }

            if (wallCheck != null)
            {
                Vector3 wallPos = wallCheck.position + Vector3.right * wallCheckDistance * facingDirection;
                Debug.DrawLine(wallCheck.position, wallPos, isTouchingWall ? Color.green : Color.red);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }

            if (wallCheck != null)
            {
                Gizmos.color = isTouchingWall ? Color.green : Color.red;
                Vector3 start = wallCheck.position;
                Vector3 end = start + Vector3.right * wallCheckDistance * facingDirection;
                Gizmos.DrawLine(start, end);
            }
        }

        private void OnDestroy()
        {
            if (stateMachine != null)
            {
                stateMachine.OnStateChanged -= OnPlayerStateChanged;
            }
        }
    }
}