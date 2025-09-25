using UnityEngine;

namespace Character.Physics
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RaycastController : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private LayerMask collisionMask = -1;
        [SerializeField] private LayerMask oneWayPlatformMask;
        [SerializeField] private float skinWidth = 0.015f;
        [SerializeField] private int horizontalRayCount = 4;
        [SerializeField] private int verticalRayCount = 4;

        [Header("Detection Distances")]
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private float wallCheckDistance = 0.1f;
        [SerializeField] private float ceilingCheckDistance = 0.1f;
        [SerializeField] private float maxSlopeAngle = 45f;

        [Header("Ground Snap Settings")]
        [SerializeField] private bool useGroundSnap = true;
        [SerializeField] private float groundSnapDistance = 0.05f;
        [SerializeField] private float groundSnapThreshold = 0.1f;

        [Header("Debug")]
        [SerializeField] private bool showDebugRays = false;
        [SerializeField] private Color groundRayColor = Color.green;
        [SerializeField] private Color wallRayColor = Color.blue;
        [SerializeField] private Color ceilingRayColor = Color.red;

        private BoxCollider2D boxCollider;
        private RaycastOrigins raycastOrigins;
        private float horizontalRaySpacing;
        private float verticalRaySpacing;
        private bool platformDropThrough;

        // Ground 위치 추적용
        private float lastGroundY;
        private bool wasGroundedLastFrame;

        private struct RaycastOrigins
        {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            CalculateRaySpacing();
        }

        public CollisionData CheckCollisions(Vector2 velocity)
        {
            UpdateRaycastOrigins();

            var collisionData = new CollisionData();

            // Ground 체크
            CheckGroundCollision(ref collisionData, velocity);

            // 정확한 위치 보정
            if (useGroundSnap)
            {
                ApplyGroundSnap(ref collisionData, velocity);
            }

            // 벽 체크
            CheckWallCollisions(ref collisionData, velocity);

            // 천장 체크
            CheckCeilingCollision(ref collisionData, velocity);

            // 경사면 체크
            if (collisionData.isGrounded)
            {
                CheckSlopeCollision(ref collisionData, velocity);
            }

            // 이전 프레임 상태 업데이트
            wasGroundedLastFrame = collisionData.isGrounded;
            if (collisionData.isGrounded)
            {
                lastGroundY = transform.position.y;
            }

            collisionData.UpdateGroundedState();

            return collisionData;
        }

        private void CheckGroundCollision(ref CollisionData data, Vector2 velocity)
        {
            float rayLength = groundCheckDistance + skinWidth;
            Vector2 rayDirection = Vector2.down;

            float closestDistance = float.MaxValue;
            RaycastHit2D closestHit = new RaycastHit2D();
            bool foundGround = false;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = Vector2.Lerp(raycastOrigins.bottomLeft, raycastOrigins.bottomRight,
                    (float)i / (verticalRayCount - 1));

                // Regular collision check
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength, collisionMask);

                // One-way platform check
                if (!hit && velocity.y <= 0 && !platformDropThrough)
                {
                    hit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength, oneWayPlatformMask);
                    if (hit)
                    {
                        data.isOnOneWayPlatform = true;
                    }
                }

                if (hit)
                {
                    if (hit.distance < closestDistance)
                    {
                        closestDistance = hit.distance;
                        closestHit = hit;
                        foundGround = true;
                    }
                }

                if (showDebugRays)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * rayLength,
                        hit ? groundRayColor : Color.white);
                }
            }

            if (foundGround)
            {
                data.isGrounded = true;
                data.groundDistance = closestDistance;
                data.groundNormal = closestHit.normal;
                data.groundHit = closestHit;

                // Moving platform 체크
                if (closestHit.transform.CompareTag("MovingPlatform"))
                {
                    data.isOnMovingPlatform = true;
                    data.platformTransform = closestHit.transform;

                    var platformRb = closestHit.transform.GetComponent<Rigidbody2D>();
                    if (platformRb != null)
                    {
                        data.platformVelocity = platformRb.linearVelocity;
                    }
                }

                data.collisionFlags |= CollisionFlags.Below;
            }
        }

        /// <summary>
        /// Ground 위치를 정확히 보정하는 기능
        /// </summary>
        private void ApplyGroundSnap(ref CollisionData data, Vector2 velocity)
        {
            // 현재 접지 되거나 Ground 상태 변경 시
            if (data.isGrounded)
            {
                // Ground에 파묻혔는지 체크
                if (data.groundDistance < skinWidth)
                {
                    float penetrationDepth = skinWidth - data.groundDistance;

                    // 파묻힌 만큼 위로 이동
                    Vector3 currentPos = transform.position;
                    currentPos.y += penetrationDepth;
                    transform.position = currentPos;

                    if (showDebugRays)
                    {
                        Debug.Log($"[GroundSnap] Pushed up by {penetrationDepth:F4}");
                    }
                }
                // 너무 떠 있는지 체크
                else if (data.groundDistance > skinWidth + groundSnapThreshold)
                {
                    // 하강 중이면서 Ground에 가까운 경우
                    if (velocity.y <= 0 && data.groundDistance < groundSnapDistance)
                    {
                        float snapDistance = data.groundDistance - skinWidth;

                        Vector3 currentPos = transform.position;
                        currentPos.y -= snapDistance;
                        transform.position = currentPos;

                        if (showDebugRays)
                        {
                            Debug.Log($"[GroundSnap] Snapped down by {snapDistance:F4}");
                        }
                    }
                }
            }

            // 새로 착지 특별 처리
            if (!wasGroundedLastFrame && data.isGrounded && velocity.y <= 0)
            {
                // Ground 표면에 정확히 위치시킴
                if (data.groundHit)
                {
                    float correctY = data.groundHit.point.y + (boxCollider.size.y * 0.5f) + skinWidth;

                    Vector3 currentPos = transform.position;
                    float yDiff = correctY - currentPos.y;

                    // 너무 크지 않으면서만 보정
                    if (Mathf.Abs(yDiff) < 0.1f)
                    {
                        currentPos.y = correctY;
                        transform.position = currentPos;

                        if (showDebugRays)
                        {
                            Debug.Log($"[GroundSnap] Landing correction: {yDiff:F4}");
                        }
                    }
                }
            }
        }

        private void CheckWallCollisions(ref CollisionData data, Vector2 velocity)
        {
            float rayLength = wallCheckDistance + skinWidth;

            // Check left wall
            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = Vector2.Lerp(raycastOrigins.bottomLeft, raycastOrigins.topLeft,
                    (float)i / (horizontalRayCount - 1));

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.left, rayLength, collisionMask);

                if (hit)
                {
                    data.isWallLeft = true;
                    data.wallDistanceLeft = hit.distance;
                    data.wallNormalLeft = hit.normal;
                    data.collisionFlags |= CollisionFlags.Left;

                    // 벽에 파묻힌 경우
                    if (hit.distance < skinWidth)
                    {
                        float pushDistance = skinWidth - hit.distance;
                        transform.position += Vector3.right * pushDistance;
                    }
                }

                if (showDebugRays)
                {
                    Debug.DrawRay(rayOrigin, Vector2.left * rayLength,
                        hit ? wallRayColor : Color.white);
                }
            }

            // Check right wall
            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = Vector2.Lerp(raycastOrigins.bottomRight, raycastOrigins.topRight,
                    (float)i / (horizontalRayCount - 1));

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right, rayLength, collisionMask);

                if (hit)
                {
                    data.isWallRight = true;
                    data.wallDistanceRight = hit.distance;
                    data.wallNormalRight = hit.normal;
                    data.collisionFlags |= CollisionFlags.Right;

                    // 벽에 파묻힌 경우
                    if (hit.distance < skinWidth)
                    {
                        float pushDistance = skinWidth - hit.distance;
                        transform.position += Vector3.left * pushDistance;
                    }
                }

                if (showDebugRays)
                {
                    Debug.DrawRay(rayOrigin, Vector2.right * rayLength,
                        hit ? wallRayColor : Color.white);
                }
            }
        }

        private void CheckCeilingCollision(ref CollisionData data, Vector2 velocity)
        {
            float rayLength = ceilingCheckDistance + skinWidth;
            Vector2 rayDirection = Vector2.up;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = Vector2.Lerp(raycastOrigins.topLeft, raycastOrigins.topRight,
                    (float)i / (verticalRayCount - 1));

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength, collisionMask);

                if (hit)
                {
                    data.isCeiling = true;
                    data.ceilingDistance = hit.distance;
                    data.ceilingNormal = hit.normal;
                    data.collisionFlags |= CollisionFlags.Above;

                    // 천장에 파묻힌 경우
                    if (hit.distance < skinWidth)
                    {
                        float pushDistance = skinWidth - hit.distance;
                        transform.position += Vector3.down * pushDistance;
                    }
                }

                if (showDebugRays)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * rayLength,
                        hit ? ceilingRayColor : Color.white);
                }
            }
        }

        private void CheckSlopeCollision(ref CollisionData data, Vector2 velocity)
        {
            if (data.groundHit)
            {
                float angle = Vector2.Angle(data.groundNormal, Vector2.up);

                if (angle > 0 && angle <= maxSlopeAngle)
                {
                    data.isOnSlope = true;
                    data.slopeAngle = angle;
                    data.slopeNormal = data.groundNormal;
                    data.canWalkOnSlope = true;
                    data.collisionFlags |= CollisionFlags.Slope;
                }
                else if (angle > maxSlopeAngle)
                {
                    data.isOnSlope = true;
                    data.slopeAngle = angle;
                    data.slopeNormal = data.groundNormal;
                    data.canWalkOnSlope = false;
                }
            }
        }

        private void UpdateRaycastOrigins()
        {
            Bounds bounds = boxCollider.bounds;
            bounds.Expand(skinWidth * -2);

            raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        private void CalculateRaySpacing()
        {
            Bounds bounds = boxCollider.bounds;
            bounds.Expand(skinWidth * -2);

            horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
            verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }

        public void EnablePlatformDropThrough(float duration)
        {
            platformDropThrough = true;
            Invoke(nameof(DisablePlatformDropThrough), duration);
        }

        private void DisablePlatformDropThrough()
        {
            platformDropThrough = false;
        }

        // Public Getters
        public LayerMask GetCollisionMask() => collisionMask;
        public float GetSkinWidth() => skinWidth;
    }
}