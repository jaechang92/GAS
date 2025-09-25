using UnityEngine;

namespace Player
{
    /// <summary>
    /// 지면 검사를 담당하는 컴포넌트
    /// OnOff 방식으로 FixedUpdate에서 지면 검사를 수행
    /// </summary>
    public class GroundChecker : MonoBehaviour
    {
        [Header("Ground Check Settings")]
        [SerializeField] private bool isGroundCheckEnabled = true; // OnOff 스위치
        [SerializeField] private LayerMask groundLayer = -1; // Ground Layer
        [SerializeField] private float groundCheckDistance = 0.15f; // 지면 검사 거리 (0.2f에서 0.15f로 감소)
        [SerializeField] private Vector2 groundCheckOffset = Vector2.zero; // 검사 위치 오프셋
        [SerializeField] private float groundCheckWidth = 0.6f; // 검사 폭 (0.8f에서 0.6f로 감소)
        [SerializeField] private int rayCount = 3; // 발사할 Ray 개수
        [SerializeField] private bool showDebugRays = true; // 디버그 Ray 표시
        [SerializeField] private float velocityThreshold = -0.5f; // 하강 속도 임계값 (점프 중 착지 오감지 방지)

        [Header("Components")]
        private Collider2D playerCollider;
        private PlayerController playerController; // 점프 상태 확인용

        [Header("Ground State")]
        [SerializeField] private bool isGrounded = false;
        private bool wasGrounded = false;

        // 이벤트
        public System.Action OnTouchGround;
        public System.Action OnLeaveGround;

        // 프로퍼티
        public bool IsGrounded => isGrounded;
        public bool IsGroundCheckEnabled
        {
            get => isGroundCheckEnabled;
            set => isGroundCheckEnabled = value;
        }

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            // 시작 시 초기 지면 검사
            if (isGroundCheckEnabled)
            {
                PerformGroundCheck();
            }
        }

        private void FixedUpdate()
        {
            // OnOff 기능 - 활성화되어 있을 때만 지면 검사 수행
            if (isGroundCheckEnabled)
            {
                PerformGroundCheck();
            }
        }

        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        private void InitializeComponents()
        {
            playerCollider = GetComponent<Collider2D>();
            playerController = GetComponent<PlayerController>();

            if (playerCollider == null)
            {
                Debug.LogError($"[GroundChecker] Collider2D를 찾을 수 없습니다! GameObject: {gameObject.name}");
            }

            if (playerController == null)
            {
                Debug.LogError($"[GroundChecker] PlayerController를 찾을 수 없습니다! GameObject: {gameObject.name}");
            }

            // Ground Layer가 설정되지 않았다면 기본값 설정
            if (groundLayer.value == -1)
            {
                groundLayer = LayerMask.GetMask("Ground");
                if (showDebugRays)
                {
                    Debug.Log($"[GroundChecker] Ground Layer를 기본값으로 설정했습니다: {groundLayer.value}");
                }
            }
        }

        /// <summary>
        /// 지면 검사 수행
        /// </summary>
        private void PerformGroundCheck()
        {
            if (playerCollider == null) return;

            wasGrounded = isGrounded;
            isGrounded = false; // 기본값은 공중 상태

            // 지면 검사를 위한 Ray 발사
            isGrounded = CheckGroundWithRaycast();

            // 상태 변화 감지 및 이벤트 발생
            HandleGroundStateChange();
        }

        /// <summary>
        /// Raycast를 사용한 지면 검사
        /// </summary>
        private bool CheckGroundWithRaycast()
        {
            Bounds bounds = playerCollider.bounds;
            Vector2 centerPoint = new Vector2(bounds.center.x + groundCheckOffset.x, bounds.min.y + groundCheckOffset.y);

            // 단일 Ray인 경우
            if (rayCount <= 1)
            {
                return CheckSingleRay(centerPoint);
            }

            // 다중 Ray인 경우
            return CheckMultipleRays(centerPoint);
        }

        /// <summary>
        /// 단일 Ray 검사
        /// </summary>
        private bool CheckSingleRay(Vector2 startPoint)
        {
            RaycastHit2D hit = Physics2D.Raycast(startPoint, Vector2.down, groundCheckDistance, groundLayer);

            if (showDebugRays)
            {
                Debug.DrawRay(startPoint, Vector2.down * groundCheckDistance, hit ? Color.green : Color.red, 0.1f);
            }

            return hit.collider != null;
        }

        /// <summary>
        /// 다중 Ray 검사
        /// </summary>
        private bool CheckMultipleRays(Vector2 centerPoint)
        {
            for (int i = 0; i < rayCount; i++)
            {
                float offsetX = 0f;
                if (rayCount > 1)
                {
                    // Ray들을 groundCheckWidth 범위에 균등하게 배치
                    offsetX = -groundCheckWidth * 0.5f + (groundCheckWidth / (rayCount - 1)) * i;
                }

                Vector2 rayOrigin = centerPoint + Vector2.right * offsetX;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);

                if (showDebugRays)
                {
                    Debug.DrawRay(rayOrigin, Vector2.down * groundCheckDistance, hit ? Color.green : Color.red, 0.1f);
                }

                // 하나의 Ray라도 지면을 감지하면 접지 상태
                if (hit.collider != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 지면 상태 변화 처리 (점프 중 착지 오감지 방지)
        /// </summary>
        private void HandleGroundStateChange()
        {
            // 착지 (공중 → 지면)
            if (!wasGrounded && isGrounded)
            {
                // 점프 중 착지 오감지 방지: 하강 중일 때만 착지로 인정
                if (playerController != null)
                {
                    float currentVelocityY = playerController.Velocity.y;
                    Player.PlayerStateType currentState = playerController.CurrentState;

                    // 점프 상태이고 상승 중이면 착지 무시
                    if (currentState == Player.PlayerStateType.Jump && currentVelocityY > velocityThreshold)
                    {
                        if (showDebugRays)
                        {
                            Debug.Log($"[GroundChecker] 점프 상승 중 착지 감지 무시! 속도: {currentVelocityY:F2}");
                        }
                        isGrounded = false; // 착지 상태 취소
                        return;
                    }
                }

                if (showDebugRays)
                {
                    Debug.Log($"[GroundChecker] 착지 감지! GameObject: {gameObject.name}");
                }
                OnTouchGround?.Invoke();
            }
            // 이륙 (지면 → 공중)
            else if (wasGrounded && !isGrounded)
            {
                if (showDebugRays)
                {
                    Debug.Log($"[GroundChecker] 이륙 감지! GameObject: {gameObject.name}");
                }
                OnLeaveGround?.Invoke();
            }
        }

        /// <summary>
        /// 지면 검사 활성화/비활성화
        /// </summary>
        public void EnableGroundCheck(bool enable)
        {
            isGroundCheckEnabled = enable;
            if (showDebugRays)
            {
                Debug.Log($"[GroundChecker] 지면 검사 {(enable ? "활성화" : "비활성화")}");
            }
        }

        /// <summary>
        /// 지면 상태 강제 설정 (디버깅용)
        /// </summary>
        public void ForceSetGrounded(bool grounded)
        {
            wasGrounded = isGrounded;
            isGrounded = grounded;

            if (showDebugRays)
            {
                Debug.Log($"[GroundChecker] 지면 상태 강제 설정: {grounded}");
            }

            HandleGroundStateChange();
        }

        /// <summary>
        /// Scene View에서 Gizmo 그리기
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (playerCollider == null) return;

            Bounds bounds = playerCollider.bounds;
            Vector3 centerPoint = new Vector3(bounds.center.x + groundCheckOffset.x, bounds.min.y + groundCheckOffset.y, bounds.center.z);

            // 지면 검사 영역 표시
            Gizmos.color = isGrounded ? Color.green : Color.red;

            if (rayCount <= 1)
            {
                // 단일 Ray
                Gizmos.DrawLine(centerPoint, centerPoint + Vector3.down * groundCheckDistance);
                Gizmos.DrawWireSphere(centerPoint + Vector3.down * groundCheckDistance, 0.05f);
            }
            else
            {
                // 다중 Ray
                for (int i = 0; i < rayCount; i++)
                {
                    float offsetX = 0f;
                    if (rayCount > 1)
                    {
                        offsetX = -groundCheckWidth * 0.5f + (groundCheckWidth / (rayCount - 1)) * i;
                    }

                    Vector3 rayOrigin = centerPoint + Vector3.right * offsetX;
                    Vector3 rayEnd = rayOrigin + Vector3.down * groundCheckDistance;

                    Gizmos.DrawLine(rayOrigin, rayEnd);
                    Gizmos.DrawWireSphere(rayEnd, 0.03f);
                }

                // 검사 영역 폭 표시
                Gizmos.color = Color.yellow;
                Vector3 leftPoint = centerPoint + Vector3.left * (groundCheckWidth * 0.5f);
                Vector3 rightPoint = centerPoint + Vector3.right * (groundCheckWidth * 0.5f);
                Gizmos.DrawLine(leftPoint, rightPoint);
            }
        }
    }
}