using UnityEngine;

namespace GASPT.Forms
{
    /// <summary>
    /// 폼 시스템 테스트용 간단한 플레이어 컨트롤러
    /// 실제 게임의 PlayerController 대신 테스트 목적으로 사용
    /// </summary>
    public class FormTestPlayerController : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 10f;

        [Header("지면 체크")]
        [SerializeField] private LayerMask groundLayer = ~0;
        [SerializeField] private float groundCheckDistance = 0.1f;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private bool isGrounded;
        private float horizontalInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 3f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        private void Update()
        {
            // 입력 처리
            horizontalInput = Input.GetAxisRaw("Horizontal");

            // 지면 체크
            CheckGround();

            // 점프
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                Jump();
            }

            // 스프라이트 방향
            UpdateSpriteDirection();
        }

        private void FixedUpdate()
        {
            // 이동
            Move();
        }

        private void Move()
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        private void Jump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        private void CheckGround()
        {
            var hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance + 0.5f, groundLayer);
            isGrounded = hit.collider != null;
        }

        private void UpdateSpriteDirection()
        {
            if (spriteRenderer == null) return;

            if (horizontalInput > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (horizontalInput < 0)
            {
                spriteRenderer.flipX = true;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 지면 체크 시각화
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (groundCheckDistance + 0.5f));
        }
    }
}
