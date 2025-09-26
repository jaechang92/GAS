using UnityEngine;
using System;

namespace Player
{
    /// <summary>
    /// 플레이어 환경 검사 전용 클래스
    /// 단일책임원칙: 지면, 벽, 천장 등 환경 요소 검사만 담당
    /// </summary>
    public class EnvironmentChecker : MonoBehaviour
    {
        [Header("환경 검사 설정")]
        [SerializeField] private Transform wallCheck;
        [SerializeField] private float wallCheckDistance = 0.5f;
        [SerializeField] private LayerMask groundLayerMask = 1;
        [SerializeField] private bool enableDebugLogs = false;

        [Header("대시 쿨다운 설정")]
        [SerializeField] private float dashCooldownTime = 1f;

        // 환경 상태
        private bool isGrounded;
        private bool isTouchingWall;
        private bool canDash = true;
        private float lastDashTime;
        private int facingDirection = 1;

        // 컴포넌트 참조
        private GroundChecker groundChecker;

        // 이벤트
        public event Action OnTouchWall;
        public event Action OnLeaveWall;
        public event Action OnDashAvailable;

        // 프로퍼티
        public bool IsGrounded => isGrounded;
        public bool IsTouchingWall => isTouchingWall;
        public bool CanDash => canDash;
        public int FacingDirection => facingDirection;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Update()
        {
            CheckGroundState();
            CheckWallCollision();
            UpdateDashCooldown();
        }

        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        private void InitializeComponents()
        {
            // GroundChecker 참조
            groundChecker = GetComponent<GroundChecker>();
            if (groundChecker == null)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("[EnvironmentChecker] GroundChecker 컴포넌트를 찾을 수 없습니다.");
            }

            // Wall Check 설정
            if (wallCheck == null)
            {
                GameObject wallCheckGO = new GameObject("WallCheck");
                wallCheckGO.transform.SetParent(transform);

                Collider2D playerCollider = GetComponent<Collider2D>();
                float xOffset = playerCollider != null ? playerCollider.bounds.extents.x : 0.4f;
                wallCheckGO.transform.localPosition = new Vector3(xOffset, 0, 0);
                wallCheck = wallCheckGO.transform;

                if (enableDebugLogs)
                {
                    Debug.Log("[EnvironmentChecker] WallCheck가 자동으로 생성되었습니다.");
                }
            }

            // 레이어 마스크 설정
            groundLayerMask = LayerMask.GetMask("Ground", "Platform");
        }

        /// <summary>
        /// 지면 상태 검사
        /// </summary>
        private void CheckGroundState()
        {
            if (groundChecker != null)
            {
                bool wasGrounded = isGrounded;
                isGrounded = groundChecker.IsGrounded;

                // 지면 상태 변화 로그 (선택적)
                if (enableDebugLogs && wasGrounded != isGrounded)
                {
                    Debug.Log($"[EnvironmentChecker] 지면 상태 변경: {wasGrounded} → {isGrounded}");
                }
            }
        }

        /// <summary>
        /// 벽 충돌 검사
        /// </summary>
        private void CheckWallCollision()
        {
            if (wallCheck == null) return;

            bool wasTouchingWall = isTouchingWall;

            // 현재 방향으로 레이캐스트
            Vector3 rayOrigin = wallCheck.position;
            Vector3 rayDirection = Vector3.right * facingDirection;

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, wallCheckDistance, groundLayerMask);
            isTouchingWall = hit.collider != null;

            // 벽 접촉 상태 변화 감지 및 이벤트 발생
            if (!wasTouchingWall && isTouchingWall)
            {
                OnTouchWall?.Invoke();
                if (enableDebugLogs)
                {
                    Debug.Log($"[EnvironmentChecker] 벽 접촉: {hit.collider.name}");
                }
            }
            else if (wasTouchingWall && !isTouchingWall)
            {
                OnLeaveWall?.Invoke();
                if (enableDebugLogs)
                {
                    Debug.Log("[EnvironmentChecker] 벽에서 떨어짐");
                }
            }
        }

        /// <summary>
        /// 대시 쿨다운 관리
        /// </summary>
        private void UpdateDashCooldown()
        {
            if (!canDash && Time.time - lastDashTime >= dashCooldownTime)
            {
                canDash = true;
                OnDashAvailable?.Invoke();

                if (enableDebugLogs)
                {
                    Debug.Log("[EnvironmentChecker] 대시 쿨다운 완료");
                }
            }
        }

        /// <summary>
        /// 방향 업데이트
        /// </summary>
        public void UpdateFacingDirection(int newDirection)
        {
            if (newDirection != 0)
            {
                facingDirection = newDirection;
            }
        }

        /// <summary>
        /// 대시 사용 (쿨다운 시작)
        /// </summary>
        public void StartDashCooldown()
        {
            canDash = false;
            lastDashTime = Time.time;

            if (enableDebugLogs)
            {
                Debug.Log("[EnvironmentChecker] 대시 쿨다운 시작");
            }
        }

        /// <summary>
        /// 대시 쿨다운 리셋 (즉시 사용 가능)
        /// </summary>
        public void ResetDashCooldown()
        {
            canDash = true;
            lastDashTime = 0f;

            if (enableDebugLogs)
            {
                Debug.Log("[EnvironmentChecker] 대시 쿨다운 리셋");
            }
        }

        /// <summary>
        /// 강제로 지면 상태 설정
        /// </summary>
        public void ForceSetGrounded(bool grounded)
        {
            if (enableDebugLogs && isGrounded != grounded)
            {
                Debug.Log($"[EnvironmentChecker] 강제 지면 상태 변경: {isGrounded} → {grounded}");
            }
            isGrounded = grounded;
        }

        /// <summary>
        /// 환경 검사 설정 업데이트
        /// </summary>
        public void UpdateSettings(float newWallCheckDistance, float newDashCooldownTime)
        {
            wallCheckDistance = newWallCheckDistance;
            dashCooldownTime = newDashCooldownTime;

            if (enableDebugLogs)
            {
                Debug.Log($"[EnvironmentChecker] 설정 업데이트 - 벽검사거리: {wallCheckDistance}, 대시쿨다운: {dashCooldownTime}");
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
        /// 현재 환경 상태 정보
        /// </summary>
        public string GetEnvironmentInfo()
        {
            return $"Ground: {isGrounded}, Wall: {isTouchingWall}, CanDash: {canDash}, Direction: {facingDirection}";
        }

        /// <summary>
        /// 디버그용 기즈모 그리기
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (wallCheck == null) return;

            // 벽 검사 레이 그리기
            Gizmos.color = isTouchingWall ? Color.green : Color.red;
            Vector3 start = wallCheck.position;
            Vector3 end = start + Vector3.right * wallCheckDistance * facingDirection;
            Gizmos.DrawLine(start, end);
        }

        private void OnDestroy()
        {
            // 필요시 이벤트 구독 해제
        }
    }
}