using UnityEngine;

namespace GASPT.Gameplay.Player
{
    /// <summary>
    /// 2D 플랫포머 플레이어 컨트롤러
    /// Rigidbody2D 기반 물리 이동, 점프, 스프라이트 방향 전환
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        // ====== 이동 설정 ======

        [Header("이동 설정")]
        [Tooltip("이동 속도 (m/s)")]
        [SerializeField] private float moveSpeed = 5f;

        [Tooltip("점프 힘")]
        [SerializeField] private float jumpForce = 10f;

        [Tooltip("공중 이동 배율 (0~1, 1이면 공중에서도 동일하게 이동)")]
        [SerializeField] [Range(0f, 1f)] private float airControlMultiplier = 0.8f;


        // ====== 지면 체크 ======

        [Header("지면 체크")]
        [Tooltip("지면 체크 포인트 (발 위치) - null이면 자동 생성")]
        [SerializeField] private Transform groundCheck;

        [Tooltip("지면 체크 반지름")]
        [SerializeField] private float groundCheckRadius = 0.2f;

        [Tooltip("지면 레이어")]
        [SerializeField] private LayerMask groundLayer = 1 << 0; // Default layer


        // ====== 스프라이트 ======

        [Header("스프라이트")]
        [Tooltip("방향 전환할 스프라이트 렌더러 - null이면 자동 탐색")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("처음 시작 방향이 오른쪽인지 (false면 왼쪽)")]
        [SerializeField] private bool startsLookingRight = true;


        // ====== 디버그 ======

        [Header("디버그")]
        [SerializeField] private bool showDebugLogs = false;


        // ====== 상태 ======

        private Rigidbody2D rb;
        private bool isGrounded;
        private float horizontalInput;
        private bool isFacingRight;


        // ====== 프로퍼티 ======

        public bool IsGrounded => isGrounded;
        public Vector2 Velocity => rb.linearVelocity;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // Rigidbody2D 초기화
            rb = GetComponent<Rigidbody2D>();
            rb.freezeRotation = true; // 플랫포머는 회전 안 함
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // SpriteRenderer 자동 탐색
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            // GroundCheck 자동 생성
            if (groundCheck == null)
            {
                GameObject groundCheckObj = new GameObject("GroundCheck");
                groundCheckObj.transform.SetParent(transform);
                groundCheckObj.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                groundCheck = groundCheckObj.transform;

                if (showDebugLogs)
                    Debug.Log("[PlayerController] GroundCheck 자동 생성됨");
            }

            // 초기 방향 설정
            isFacingRight = startsLookingRight;
            UpdateSpriteDirection(isFacingRight);
        }

        private void Update()
        {
            // 입력 받기
            HandleInput();

            // 스프라이트 방향 업데이트
            if (horizontalInput != 0)
            {
                bool shouldFaceRight = horizontalInput > 0;
                if (shouldFaceRight != isFacingRight)
                {
                    isFacingRight = shouldFaceRight;
                    UpdateSpriteDirection(isFacingRight);
                }
            }
        }

        private void FixedUpdate()
        {
            // 지면 체크
            CheckGround();

            // 이동
            Move(horizontalInput);
        }


        // ====== 입력 처리 ======

        /// <summary>
        /// 플레이어 입력 처리
        /// </summary>
        private void HandleInput()
        {
            // 좌우 이동 입력 (A/D, Left/Right)
            horizontalInput = Input.GetAxis("Horizontal");

            // 점프 입력 제거됨 - FormInputHandler에서 처리
            // 점프는 이제 JumpAbility를 통해 FormInputHandler에서 관리됨
        }


        // ====== 이동 ======

        /// <summary>
        /// 플레이어 이동 (물리 기반)
        /// </summary>
        private void Move(float direction)
        {
            if (direction == 0f)
            {
                // 입력이 없으면 수평 속도 감소 (미끄러짐 방지)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y);
                return;
            }

            // 공중이면 이동력 감소
            float currentMoveSpeed = isGrounded ? moveSpeed : moveSpeed * airControlMultiplier;

            // 속도 적용
            Vector2 targetVelocity = new Vector2(direction * currentMoveSpeed, rb.linearVelocity.y);
            rb.linearVelocity = targetVelocity;
        }

        /// <summary>
        /// 점프 실행
        /// </summary>
        private void Jump()
        {
            // Y축 속도를 0으로 리셋 후 점프 (이전 하강 속도 무시)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

            // 점프 힘 적용
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (showDebugLogs)
                Debug.Log($"[PlayerController] 점프! 힘: {jumpForce}");
        }


        // ====== 지면 체크 ======

        /// <summary>
        /// 지면 체크 (OverlapCircle)
        /// </summary>
        private void CheckGround()
        {
            if (groundCheck == null) return;

            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            // 착지 순간 로그
            if (!wasGrounded && isGrounded && showDebugLogs)
            {
                Debug.Log("[PlayerController] 착지!");
            }
        }


        // ====== 스프라이트 방향 ======

        /// <summary>
        /// 스프라이트 방향 업데이트 (좌우 반전)
        /// </summary>
        private void UpdateSpriteDirection(bool facingRight)
        {
            if (spriteRenderer == null) return;

            // flipX로 좌우 반전
            spriteRenderer.flipX = !facingRight;

            if (showDebugLogs)
                Debug.Log($"[PlayerController] 방향 전환: {(facingRight ? "오른쪽" : "왼쪽")}");
        }


        // ====== Gizmos ======

        private void OnDrawGizmos()
        {
            if (groundCheck == null) return;

            // 지면 체크 영역 표시
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;

            // 선택 시 더 명확하게 표시
            Gizmos.color = isGrounded ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);

#if UNITY_EDITOR
            // 지면 체크 정보 표시
            string info = $"Grounded: {isGrounded}\n" +
                         $"Velocity: ({rb?.linearVelocity.x:F2}, {rb?.linearVelocity.y:F2})";
            UnityEditor.Handles.Label(groundCheck.position + Vector3.up * 0.5f, info);
#endif
        }


        // ====== 디버그 ======

        [ContextMenu("Print Controller Info")]
        private void PrintControllerInfo()
        {
            Debug.Log($"=== PlayerController ===\n" +
                     $"Move Speed: {moveSpeed} m/s\n" +
                     $"Jump Force: {jumpForce}\n" +
                     $"Grounded: {isGrounded}\n" +
                     $"Facing Right: {isFacingRight}\n" +
                     $"Velocity: {(rb != null ? rb.linearVelocity.ToString() : "null")}\n" +
                     $"=======================");
        }

        [ContextMenu("Force Jump (Test)")]
        private void ForceJumpTest()
        {
            if (Application.isPlaying && rb != null)
            {
                Jump();
            }
            else
            {
                Debug.LogWarning("[PlayerController] Play 모드에서만 실행 가능합니다.");
            }
        }
    }
}
