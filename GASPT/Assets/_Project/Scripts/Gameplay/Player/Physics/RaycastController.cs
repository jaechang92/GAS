using UnityEngine;

namespace Character.Physics
{
    /// <summary>
    /// 정밀한 레이캐스트 기반 충돌 검사 시스템
    /// Skul 스타일 픽셀 퍼펙트 충돌 처리
    /// </summary>
    public class RaycastController : MonoBehaviour
    {
        [Header("충돌 설정")]
        [SerializeField] private LayerMask collisionMask = 1;
        [SerializeField] private float skinWidth = 0.08f;

        [Header("레이캐스트 설정")]
        [SerializeField] private int horizontalRayCount = 4;
        [SerializeField] private int verticalRayCount = 4;

        [Header("디버그")]
        [SerializeField] private bool showRaycastGizmos = false;
        [SerializeField] private Color raycastColor = Color.red;

        // 충돌 검사 결과
        public struct CollisionInfo
        {
            public bool above, below;
            public bool left, right;
            public bool climbingSlope, descendingSlope;
            public float slopeAngle, slopeAngleOld;
            public Vector3 velocityOld;
            public int faceDir;
            public bool fallingThroughPlatform;

            public void Reset()
            {
                above = below = false;
                left = right = false;
                climbingSlope = descendingSlope = false;
                slopeAngleOld = slopeAngle;
                slopeAngle = 0;
                faceDir = 0;
            }
        }

        public CollisionInfo collisions;
        private BoxCollider2D boxCollider;
        private RaycastOrigins raycastOrigins;

        // 레이캐스트 원점 계산용 구조체
        struct RaycastOrigins
        {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                Debug.LogError("[RaycastController] BoxCollider2D가 필요합니다!");
            }
            CalculateRaySpacing();
        }

        private float horizontalRaySpacing, verticalRaySpacing;

        /// <summary>
        /// 레이캐스트 간격 계산
        /// </summary>
        public void CalculateRaySpacing()
        {
            if (boxCollider == null) return;

            Bounds bounds = boxCollider.bounds;
            bounds.Expand(skinWidth * -2);

            horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
            verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }

        /// <summary>
        /// 이동 처리 및 충돌 검사
        /// </summary>
        public void Move(Vector3 moveAmount, bool standingOnPlatform = false)
        {
            UpdateRaycastOrigins();
            collisions.Reset();
            collisions.velocityOld = moveAmount;

            if (moveAmount.x != 0)
            {
                collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
            }

            // 매우 작은 이동량은 무시 (떨림 방지)
            if (Mathf.Abs(moveAmount.x) < 0.001f && Mathf.Abs(moveAmount.y) < 0.001f)
            {
                return;
            }

            if (moveAmount.y < 0)
            {
                DescendSlope(ref moveAmount);
            }

            HorizontalCollisions(ref moveAmount);

            if (moveAmount.y != 0)
            {
                VerticalCollisions(ref moveAmount);
            }

            // 부드러운 이동 적용
            transform.Translate(moveAmount);

            if (standingOnPlatform)
            {
                collisions.below = true;
            }
        }

        /// <summary>
        /// 수평 충돌 검사
        /// </summary>
        void HorizontalCollisions(ref Vector3 moveAmount)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            float rayLength = skinWidth * 2f; // 고정된 레이 길이

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                if (showRaycastGizmos)
                {
                    Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, raycastColor);
                }

                if (hit)
                {
                    if (hit.distance == 0)
                    {
                        continue;
                    }

                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                    if (i == 0 && slopeAngle <= 45f)
                    {
                        if (collisions.descendingSlope)
                        {
                            collisions.descendingSlope = false;
                            moveAmount = collisions.velocityOld;
                        }
                        float distanceToSlopeStart = 0;
                        if (slopeAngle != collisions.slopeAngleOld)
                        {
                            distanceToSlopeStart = hit.distance - skinWidth;
                            moveAmount.x -= distanceToSlopeStart * directionX;
                        }
                        ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
                        moveAmount.x += distanceToSlopeStart * directionX;
                    }

                    if (!collisions.climbingSlope || slopeAngle > 45f)
                    {
                        moveAmount.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;

                        if (collisions.climbingSlope)
                        {
                            moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                        }

                        collisions.left = directionX == -1;
                        collisions.right = directionX == 1;
                    }
                }
            }
        }

        /// <summary>
        /// 수직 충돌 검사
        /// </summary>
        void VerticalCollisions(ref Vector3 moveAmount)
        {
            float directionY = Mathf.Sign(moveAmount.y);
            float rayLength = skinWidth * 2f; // 고정된 레이 길이

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

                if (showRaycastGizmos)
                {
                    Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, raycastColor);
                }

                if (hit)
                {
                    if (hit.collider.tag == "OneWayPlatform")
                    {
                        if (directionY == 1 || hit.distance == 0)
                        {
                            continue;
                        }
                        if (collisions.fallingThroughPlatform)
                        {
                            continue;
                        }
                    }

                    moveAmount.y = (hit.distance - skinWidth) * directionY;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
                    }

                    collisions.below = directionY == -1;
                    collisions.above = directionY == 1;
                }
            }

            if (collisions.climbingSlope)
            {
                float directionX = Mathf.Sign(moveAmount.x);
                rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
                Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (slopeAngle != collisions.slopeAngle)
                    {
                        moveAmount.x = (hit.distance - skinWidth) * directionX;
                        collisions.slopeAngle = slopeAngle;
                        collisions.climbingSlope = true;
                    }
                }
            }
        }

        /// <summary>
        /// 경사 오르기 처리
        /// </summary>
        void ClimbSlope(ref Vector3 moveAmount, float slopeAngle, Vector2 slopeNormal)
        {
            float moveDistance = Mathf.Abs(moveAmount.x);
            float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            if (moveAmount.y <= climbmoveAmountY)
            {
                moveAmount.y = climbmoveAmountY;
                moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                collisions.below = true;
                collisions.climbingSlope = true;
                collisions.slopeAngle = slopeAngle;
            }
        }

        /// <summary>
        /// 경사 내려가기 처리
        /// </summary>
        void DescendSlope(ref Vector3 moveAmount)
        {
            RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
            RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
            if (maxSlopeHitLeft ^ maxSlopeHitRight)
            {
                SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
                SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
            }

            if (!collisions.descendingSlope)
            {
                float directionX = Mathf.Sign(moveAmount.x);
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (slopeAngle != 0 && slopeAngle <= 45f)
                    {
                        if (Mathf.Sign(hit.normal.x) == directionX)
                        {
                            if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                            {
                                float moveDistance = Mathf.Abs(moveAmount.x);
                                float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                                moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                                moveAmount.y -= descendmoveAmountY;

                                collisions.slopeAngle = slopeAngle;
                                collisions.descendingSlope = true;
                                collisions.below = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 최대 경사각 슬라이드 처리
        /// </summary>
        void SlideDownMaxSlope(RaycastHit2D hit, ref Vector3 moveAmount)
        {
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle > 45f)
                {
                    moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                    collisions.slopeAngle = slopeAngle;
                    collisions.descendingSlope = true;
                    collisions.below = true;
                }
            }
        }

        /// <summary>
        /// 레이캐스트 원점 업데이트
        /// </summary>
        void UpdateRaycastOrigins()
        {
            if (boxCollider == null) return;

            Bounds bounds = boxCollider.bounds;
            bounds.Expand(skinWidth * -2);

            raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        /// <summary>
        /// 접지 상태 확인
        /// </summary>
        public bool IsGrounded()
        {
            return collisions.below;
        }

        /// <summary>
        /// 벽 접촉 상태 확인
        /// </summary>
        public bool IsTouchingWall()
        {
            return collisions.left || collisions.right;
        }

        /// <summary>
        /// 원웨이 플랫폼 관통 설정
        /// </summary>
        public void SetFallingThroughPlatform(bool falling)
        {
            collisions.fallingThroughPlatform = falling;
        }

        private void OnDrawGizmos()
        {
            if (showRaycastGizmos && boxCollider != null)
            {
                Bounds bounds = boxCollider.bounds;
                bounds.Expand(skinWidth * -2);

                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
}