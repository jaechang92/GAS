using UnityEngine;
using System;

namespace Character.Physics
{
    /// <summary>
    /// 통합 물리 엔진 - 모든 물리 관련 기능을 관리하는 중앙 허브
    /// 단일 책임: 물리 시스템 조정 및 생명주기 관리
    /// </summary>
    public class PhysicsEngine : MonoBehaviour
    {
        [Header("물리 설정")]
        [SerializeField] private CharacterPhysicsConfig config;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private bool showPhysicsGizmos = false;

        // 핵심 컴포넌트들
        private CollisionDetector collisionDetector;
        private MovementProcessor movementProcessor;
        private PhysicsState physicsState;

        // 외부 인터페이스
        public ICollisionInfo CollisionInfo => collisionDetector?.CollisionInfo;
        public IMovementState MovementState => physicsState;
        public IGroundState GroundState => physicsState;
        public IWallState WallState => physicsState;

        // 주요 프로퍼티
        public Vector3 Velocity => physicsState?.Velocity ?? Vector3.zero;
        public bool IsGrounded => physicsState?.IsGrounded ?? false;
        public bool IsTouchingWall => physicsState?.IsTouchingWall ?? false;
        public bool CanDash => movementProcessor?.CanDash ?? false;
        public bool IsDashing => movementProcessor?.IsDashing ?? false;

        // 이벤트 시스템
        public event Action<Vector3> OnVelocityChanged;
        public event Action OnGroundedChanged;
        public event Action OnWallTouchChanged;
        public event Action OnJump;
        public event Action OnDash;

        #region Unity 생명주기

        private void Awake()
        {
            InitializeComponents();
            ValidateConfiguration();
        }

        private void Start()
        {
            SetupEventConnections();
            InitializePhysicsState();
        }

        private void FixedUpdate()
        {
            UpdatePhysics(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            CleanupEventConnections();
        }

        #endregion

        #region 초기화

        private void InitializeComponents()
        {
            // CollisionDetector 초기화
            collisionDetector = GetComponent<CollisionDetector>();
            if (collisionDetector == null)
            {
                collisionDetector = gameObject.AddComponent<CollisionDetector>();
                LogDebug("CollisionDetector 자동 생성됨");
            }

            // MovementProcessor 초기화
            movementProcessor = GetComponent<MovementProcessor>();
            if (movementProcessor == null)
            {
                movementProcessor = gameObject.AddComponent<MovementProcessor>();
                LogDebug("MovementProcessor 자동 생성됨");
            }

            // PhysicsState 초기화
            physicsState = GetComponent<PhysicsState>();
            if (physicsState == null)
            {
                physicsState = gameObject.AddComponent<PhysicsState>();
                LogDebug("PhysicsState 자동 생성됨");
            }
        }

        private void ValidateConfiguration()
        {
            if (config == null)
            {
                Debug.LogError($"[PhysicsEngine] CharacterPhysicsConfig가 설정되지 않았습니다! {gameObject.name}");
            }
        }

        private void SetupEventConnections()
        {
            // PhysicsState 이벤트 연결
            if (physicsState != null)
            {
                physicsState.OnVelocityChanged += (velocity) => OnVelocityChanged?.Invoke(velocity);
                physicsState.OnGroundedChanged += () => OnGroundedChanged?.Invoke();
                physicsState.OnWallTouchChanged += () => OnWallTouchChanged?.Invoke();
            }

            // MovementProcessor 이벤트 연결
            if (movementProcessor != null)
            {
                movementProcessor.OnJump += () => OnJump?.Invoke();
                movementProcessor.OnDash += () => OnDash?.Invoke();
            }
        }

        private void CleanupEventConnections()
        {
            // 이벤트 구독 해제
            if (physicsState != null)
            {
                physicsState.OnVelocityChanged -= (velocity) => OnVelocityChanged?.Invoke(velocity);
                physicsState.OnGroundedChanged -= () => OnGroundedChanged?.Invoke();
                physicsState.OnWallTouchChanged -= () => OnWallTouchChanged?.Invoke();
            }

            if (movementProcessor != null)
            {
                movementProcessor.OnJump -= () => OnJump?.Invoke();
                movementProcessor.OnDash -= () => OnDash?.Invoke();
            }
        }

        private void InitializePhysicsState()
        {
            // 모든 컴포넌트에 설정 전달
            collisionDetector?.Initialize(config);
            movementProcessor?.Initialize(config);
            physicsState?.Initialize(config);

            LogDebug("물리 엔진 초기화 완료");
        }

        #endregion

        #region 물리 업데이트

        private void UpdatePhysics(float deltaTime)
        {
            if (config == null) return;

            if (enableDebugLogs)
            {
                Debug.Log($"[PhysicsEngine] UpdatePhysics 시작 - deltaTime: {deltaTime:F4}");
            }

            // 1. 충돌 검사 업데이트
            collisionDetector?.UpdateCollisions(physicsState.Velocity);

            // 2. 물리 상태 업데이트 (접지 상태 확인 + 중력 적용)
            physicsState?.UpdateState(collisionDetector.CollisionInfo, deltaTime);

            // 3. 이동 처리 업데이트 (점프, 대시 등 - 중력 위에 덮어쓰기)
            movementProcessor?.ProcessMovement(physicsState, deltaTime);

            // 4. 최종 속도 적용
            ApplyFinalVelocity();
        }

        private void ApplyFinalVelocity()
        {
            var finalVelocity = movementProcessor.GetFinalVelocity();
            physicsState.SetVelocity(finalVelocity);

            // Transform 위치 업데이트 (CollisionDetector가 이미 정밀한 충돌 처리 완료)
            transform.Translate(finalVelocity * Time.fixedDeltaTime);
        }


        #endregion

        #region 외부 인터페이스

        /// <summary>
        /// 수평 이동 입력 설정
        /// </summary>
        public void SetHorizontalMovement(float input)
        {
            movementProcessor?.SetHorizontalInput(input);
        }

        /// <summary>
        /// 점프 입력 설정
        /// </summary>
        public void SetJumpInput(bool pressed, bool held)
        {
            movementProcessor?.SetJumpInput(pressed, held);
        }

        /// <summary>
        /// 대시 실행
        /// </summary>
        public void PerformDash(Vector2 direction)
        {
            movementProcessor?.PerformDash(direction);
        }

        /// <summary>
        /// 속도 직접 설정
        /// </summary>
        public void SetVelocity(Vector3 newVelocity)
        {
            physicsState?.SetVelocity(newVelocity);
        }


        #endregion

        #region 디버그

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[PhysicsEngine] {message}");
            }
        }

        private void OnDrawGizmos()
        {
            if (!showPhysicsGizmos || !Application.isPlaying) return;

            var pos = transform.position;

            // 속도 벡터 그리기 (크기 조정)
            if (Velocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(pos, pos + Velocity * 0.5f); // 크기 조정

                // 속도 방향 화살표
                Gizmos.color = Color.cyan;
                Vector3 arrowHead = pos + Velocity.normalized * 0.3f;
                Gizmos.DrawWireSphere(arrowHead, 0.05f);
            }

            // 접지 상태 표시 (개선)
            if (IsGrounded)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(pos + Vector3.down * 0.6f, Vector3.one * 0.15f);

                // 접지 감지 범위 표시
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                if (collisionDetector != null)
                {
                    var bounds = GetComponent<BoxCollider2D>()?.bounds;
                    if (bounds.HasValue)
                    {
                        float detectionRange = 0.08f * 2f; // skinWidth * 2
                        Vector3 detectionArea = new Vector3(bounds.Value.size.x, detectionRange, 1f);
                        Gizmos.DrawCube(pos + Vector3.down * (bounds.Value.extents.y + detectionRange/2), detectionArea);
                    }
                }
            }

            // 벽 감지 상태 표시 (개선)
            if (IsTouchingWall)
            {
                Gizmos.color = Color.red;
                Vector3 wallIndicator = pos + Vector3.right * 0.6f;
                Gizmos.DrawWireCube(wallIndicator, Vector3.one * 0.15f);

                // 벽 방향 표시
                if (physicsState != null)
                {
                    Gizmos.color = Color.yellow;
                    float wallDir = physicsState.WallDirection;
                    Vector3 wallDirection = Vector3.right * wallDir * 0.4f;
                    Gizmos.DrawLine(pos, pos + wallDirection);
                }
            }

            // 대시 상태 표시
            if (IsDashing)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(pos, 0.3f);
            }

            // 충돌 안전 거리 표시
            if (IsGrounded && showPhysicsGizmos)
            {
                var boxCollider = GetComponent<BoxCollider2D>();
                if (boxCollider != null)
                {
                    var bounds = boxCollider.bounds;
                    float safeDistance = config?.skinWidth * 1.1f ?? 0.088f;

                    // 안전 거리 영역 표시
                    Gizmos.color = new Color(0, 1, 0, 0.2f); // 반투명 녹색
                    Vector3 safeZoneSize = new Vector3(bounds.size.x, safeDistance, 1f);
                    Vector3 safeZoneCenter = new Vector3(bounds.center.x, bounds.min.y - safeDistance/2, pos.z);
                    Gizmos.DrawCube(safeZoneCenter, safeZoneSize);
                }
            }

            // PhysicsEngine 정보 텍스트
            #if UNITY_EDITOR
            if (showPhysicsGizmos)
            {
                UnityEditor.Handles.color = Color.white;
                string info = $"Grounded: {IsGrounded}\nVel: {Velocity:F1}\nWall: {IsTouchingWall}";
                UnityEditor.Handles.Label(pos + Vector3.up * 1f, info);
            }
            #endif
        }

        #endregion
    }
}
