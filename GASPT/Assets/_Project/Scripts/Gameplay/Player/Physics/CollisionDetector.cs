using UnityEngine;

namespace Character.Physics
{
    /// <summary>
    /// 고성능 충돌 검사 시스템
    /// 단일 책임: 레이캐스트 기반 정밀한 충돌 검사만 담당
    /// </summary>
    public class CollisionDetector : MonoBehaviour, ICollisionDetector
    {
        [Header("충돌 설정")]
        [SerializeField] private LayerMask collisionMask = 1;
        [SerializeField] private float skinWidth = 0.08f;

        [Header("레이캐스트 설정")]
        [SerializeField] private int horizontalRayCount = 4;
        [SerializeField] private int verticalRayCount = 4;

        [Header("디버그")]
        [SerializeField] private bool showDebugRays = false;
        [SerializeField] private Color rayColor = Color.red;

        // 컴포넌트
        private BoxCollider2D boxCollider;
        private CollisionInfo collisionInfo;

        // 구현
        public ICollisionInfo CollisionInfo => collisionInfo;

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            collisionInfo = new CollisionInfo();

            if (boxCollider == null)
            {
                Debug.LogError($"[CollisionDetector] BoxCollider2D가 필요합니다! {gameObject.name}");
            }
        }

        public void Initialize(CharacterPhysicsConfig config)
        {
            if (config != null)
            {
                // 설정에서 값 적용
                skinWidth = config.skinWidth;
                horizontalRayCount = config.horizontalRayCount;
                verticalRayCount = config.verticalRayCount;
                collisionMask = config.collisionMask;
                showDebugRays = config.showRaycastGizmos;
            }

            UpdateRaycastOrigins();
        }

        public void UpdateCollisions(Vector3 velocity)
        {
            UpdateRaycastOrigins();
            collisionInfo.Reset(velocity);

            // 수평 충돌 검사
            if (velocity.x != 0)
            {
                HorizontalCollisions(ref velocity);
            }

            // 수직 충돌 검사 (항상 실행하여 접지 상태 확인)
            VerticalCollisions(ref velocity);

            collisionInfo.SetFinalVelocity(velocity);
        }

        public bool CheckGround(Vector3 position, float distance = 0.1f)
        {
            var bounds = boxCollider.bounds;
            var rayOrigin = new Vector3(bounds.center.x, bounds.min.y, position.z);

            RaycastHit2D hit = Physics2D.Raycast(
                rayOrigin,
                Vector2.down,
                distance,
                collisionMask
            );

            if (showDebugRays)
            {
                Debug.DrawRay(rayOrigin, Vector2.down * distance, hit ? Color.green : Color.red);
            }

            return hit.collider != null;
        }

        public bool CheckWall(Vector3 position, Vector3 direction, float distance = 0.1f)
        {
            var bounds = boxCollider.bounds;
            var rayOrigin = new Vector3(bounds.center.x, bounds.center.y, position.z);

            RaycastHit2D hit = Physics2D.Raycast(
                rayOrigin,
                direction.normalized,
                distance,
                collisionMask
            );

            if (showDebugRays)
            {
                Debug.DrawRay(rayOrigin, direction.normalized * distance, hit ? Color.green : Color.red);
            }

            return hit.collider != null;
        }

        public bool CheckCeiling(Vector3 position, float distance = 0.1f)
        {
            var bounds = boxCollider.bounds;
            var rayOrigin = new Vector3(bounds.center.x, bounds.max.y, position.z);

            RaycastHit2D hit = Physics2D.Raycast(
                rayOrigin,
                Vector2.up,
                distance,
                collisionMask
            );

            if (showDebugRays)
            {
                Debug.DrawRay(rayOrigin, Vector2.up * distance, hit ? Color.green : Color.red);
            }

            return hit.collider != null;
        }

        #region 레이캐스트 내부 구현

        private RaycastOrigins raycastOrigins;

        private struct RaycastOrigins
        {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }

        private void UpdateRaycastOrigins()
        {
            if (boxCollider == null) return;

            Bounds bounds = boxCollider.bounds;
            bounds.Expand(skinWidth * -2);

            raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        private void HorizontalCollisions(ref Vector3 velocity)
        {
            float directionX = Mathf.Sign(velocity.x);
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            if (Mathf.Abs(velocity.x) < skinWidth)
            {
                rayLength = 2 * skinWidth;
            }

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRayCount > 1 ? i / (float)(horizontalRayCount - 1) : 0) *
                            (raycastOrigins.topLeft.y - raycastOrigins.bottomLeft.y);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                if (showDebugRays)
                {
                    Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, rayColor);
                }

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                    if (i == 0 && slopeAngle <= collisionInfo.MaxSlopeAngle)
                    {
                        HandleSlopeClimbing(ref velocity, hit, slopeAngle, directionX);
                    }

                    if (!collisionInfo.ClimbingSlope || slopeAngle > collisionInfo.MaxSlopeAngle)
                    {
                        velocity.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;

                        if (collisionInfo.ClimbingSlope)
                        {
                            velocity.y = Mathf.Tan(collisionInfo.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                        }

                        if (directionX == -1)
                            ((CollisionInfo)collisionInfo).Left_Internal = true;
                        else if (directionX == 1)
                            ((CollisionInfo)collisionInfo).Right_Internal = true;
                    }
                }
            }
        }

        private void VerticalCollisions(ref Vector3 velocity)
        {
            float originalVelocityY = velocity.y;

            // 아래쪽 검사 (접지 상태 확인용 - 항상 실행)
            CheckVerticalDirection(ref velocity, -1, originalVelocityY);

            // 위쪽 검사 (위로 이동할 때만)
            if (velocity.y > 0)
            {
                CheckVerticalDirection(ref velocity, 1, originalVelocityY);
            }
        }

        private void CheckVerticalDirection(ref Vector3 velocity, int directionY, float originalVelocityY)
        {
            float rayLength;

            if (directionY == -1)
            {
                // 아래쪽 검사: 고정된 감지 거리
                rayLength = skinWidth * 2f; // 항상 동일한 거리로 감지
            }
            else
            {
                // 위쪽 검사: 고정된 감지 거리
                rayLength = skinWidth * 2f; // 위쪽도 동일한 거리로 감지
            }

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRayCount > 1 ? i / (float)(verticalRayCount - 1) : 0) *
                            (raycastOrigins.bottomRight.x - raycastOrigins.bottomLeft.x);

                // 중요: velocity.x 오프셋 제거 - 현재 위치에서 검사
                // rayOrigin += Vector2.right * velocity.x; // 이 줄 제거됨

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

                if (showDebugRays)
                {
                    Color debugColor = directionY == -1 ? Color.green : Color.blue;
                    Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, debugColor, 0.1f);
                }

                if (hit)
                {
                    if (directionY == -1)
                    {
                        // 아래쪽 충돌: 정밀한 충돌 처리로 파묻힘 방지
                        float frameMovement = Mathf.Abs(velocity.y) * Time.fixedDeltaTime;
                        float safeDistance = skinWidth * 1.1f; // 안전 거리

                        if (hit.distance <= safeDistance)
                        {
                            // 접지 상태 - 안전 거리 내에 있음
                            velocity.y = 0;
                            ((CollisionInfo)collisionInfo).Below_Internal = true;
                        }
                        else if (velocity.y < 0 && hit.distance <= frameMovement + safeDistance)
                        {
                            // 이번 프레임에 충돌할 예정 - 정확한 정지 지점 계산
                            float exactStopDistance = hit.distance - skinWidth;

                            if (exactStopDistance > 0.001f) // 미세한 거리는 무시
                            {
                                // 정확히 skinWidth 거리에서 정지하도록 속도 조정
                                velocity.y = -exactStopDistance / Time.fixedDeltaTime;
                            }
                            else
                            {
                                // 이미 충분히 가까우면 완전 정지
                                velocity.y = 0;
                            }

                            ((CollisionInfo)collisionInfo).Below_Internal = true;
                        }
                        // else: 아직 멀리 있음 - 아무것도 하지 않음 (자연스러운 낙하 계속)
                    }
                    else
                    {
                        // 위쪽 충돌: 천장에 부딪힐 때
                        if (velocity.y > 0)
                        {
                            float frameMovement = velocity.y * Time.fixedDeltaTime;

                            if (hit.distance <= frameMovement + skinWidth)
                            {
                                // 정확한 정지 지점 계산
                                float exactStopDistance = hit.distance - skinWidth;

                                if (exactStopDistance > 0.001f)
                                {
                                    velocity.y = exactStopDistance / Time.fixedDeltaTime;
                                }
                                else
                                {
                                    velocity.y = 0; // 천장에 부딪힘
                                }

                                ((CollisionInfo)collisionInfo).Above_Internal = true;
                            }
                        }
                    }

                    rayLength = hit.distance;

                    // 경사면 처리
                    if (collisionInfo.ClimbingSlope && directionY == -1)
                    {
                        velocity.x = velocity.y / Mathf.Tan(collisionInfo.SlopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                    }

                    break; // 첫 번째 충돌에서 중단
                }
            }
        }

        private void HandleSlopeClimbing(ref Vector3 velocity, RaycastHit2D hit, float slopeAngle, float directionX)
        {
            float moveDistance = Mathf.Abs(velocity.x);
            float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            if (velocity.y <= climbVelocityY)
            {
                velocity.y = climbVelocityY;
                velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                ((CollisionInfo)collisionInfo).Below_Internal = true;
                ((CollisionInfo)collisionInfo).ClimbingSlope_Internal = true;
                ((CollisionInfo)collisionInfo).SlopeAngle_Internal = slopeAngle;
            }
        }

        #endregion

        #region 기즈모 디버그

        private void OnDrawGizmos()
        {
            if (!showDebugRays || !Application.isPlaying || boxCollider == null) return;

            var pos = transform.position;
            var bounds = boxCollider.bounds;
            bounds.Expand(skinWidth * -2);

            // 충돌 박스 표시
            Gizmos.color = new Color(1, 1, 0, 0.3f); // 반투명 노란색
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // 레이캐스트 원점들 표시
            Gizmos.color = Color.white;
            Vector2 bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            Vector2 bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            Vector2 topLeft = new Vector2(bounds.min.x, bounds.max.y);
            Vector2 topRight = new Vector2(bounds.max.x, bounds.max.y);

            Gizmos.DrawWireSphere(bottomLeft, 0.02f);
            Gizmos.DrawWireSphere(bottomRight, 0.02f);
            Gizmos.DrawWireSphere(topLeft, 0.02f);
            Gizmos.DrawWireSphere(topRight, 0.02f);

            // 수직 레이캐스트 시뮬레이션 (아래쪽)
            Gizmos.color = Color.green;
            float rayLength = skinWidth * 2f; // 고정된 레이 길이

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = bottomLeft;
                rayOrigin += Vector2.right * (verticalRayCount > 1 ? i / (float)(verticalRayCount - 1) : 0) *
                            (bottomRight.x - bottomLeft.x);

                // 아래쪽 레이 표시
                Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * rayLength);
                Gizmos.DrawWireSphere(rayOrigin + Vector2.down * rayLength, 0.01f);
            }

            // 수평 레이캐스트 시뮬레이션
            Gizmos.color = Color.red;
            for (int i = 0; i < horizontalRayCount; i++)
            {
                // 왼쪽 레이
                Vector2 leftRayOrigin = bottomLeft;
                leftRayOrigin += Vector2.up * (horizontalRayCount > 1 ? i / (float)(horizontalRayCount - 1) : 0) *
                                (topLeft.y - bottomLeft.y);
                Gizmos.DrawLine(leftRayOrigin, leftRayOrigin + Vector2.left * rayLength);

                // 오른쪽 레이
                Vector2 rightRayOrigin = bottomRight;
                rightRayOrigin += Vector2.up * (horizontalRayCount > 1 ? i / (float)(horizontalRayCount - 1) : 0) *
                                 (topRight.y - bottomRight.y);
                Gizmos.DrawLine(rightRayOrigin, rightRayOrigin + Vector2.right * rayLength);
            }

            // CollisionInfo 상태 표시
            if (collisionInfo != null)
            {
                Vector3 infoPos = pos + Vector3.up * 0.8f;

                if (collisionInfo.Below)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(infoPos + Vector3.down * 0.2f, Vector3.one * 0.1f);
                }

                if (collisionInfo.Above)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(infoPos + Vector3.up * 0.2f, Vector3.one * 0.1f);
                }

                if (collisionInfo.Left)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(infoPos + Vector3.left * 0.2f, Vector3.one * 0.1f);
                }

                if (collisionInfo.Right)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(infoPos + Vector3.right * 0.2f, Vector3.one * 0.1f);
                }
            }

            // 충돌 감지 정보 텍스트
            #if UNITY_EDITOR
            if (showDebugRays && collisionInfo != null)
            {
                UnityEditor.Handles.color = Color.white;
                string info = $"Collision Info:\nBelow: {collisionInfo.Below}\nAbove: {collisionInfo.Above}\nLeft: {collisionInfo.Left}\nRight: {collisionInfo.Right}";
                UnityEditor.Handles.Label(pos + Vector3.up * 1.2f, info);
            }
            #endif
        }

        #endregion
    }

    /// <summary>
    /// 충돌 정보 구조체
    /// </summary>
    public class CollisionInfo : ICollisionInfo
    {
        public bool Above { get; private set; }
        public bool Below { get; private set; }
        public bool Left { get; private set; }
        public bool Right { get; private set; }
        public bool ClimbingSlope { get; private set; }
        public bool DescendingSlope { get; private set; }
        public float SlopeAngle { get; private set; }
        public Vector3 VelocityOld { get; private set; }
        public int FaceDirection { get; private set; }
        public bool FallingThroughPlatform { get; private set; }

        // 내부 설정
        public float MaxSlopeAngle { get; set; } = 50f;

        public void Reset(Vector3 velocity)
        {
            Above = Below = false;
            Left = Right = false;
            ClimbingSlope = DescendingSlope = false;
            FallingThroughPlatform = false;

            VelocityOld = velocity;
            FaceDirection = (int)Mathf.Sign(velocity.x);
        }

        public void SetFinalVelocity(Vector3 finalVelocity)
        {
            // 최종 속도 설정 시 추가 로직이 필요한 경우
        }

        // 내부 접근용 프로퍼티들
        internal bool Left_Internal { set => Left = value; }
        internal bool Right_Internal { set => Right = value; }
        internal bool Above_Internal { set => Above = value; }
        internal bool Below_Internal { set => Below = value; }
        internal bool ClimbingSlope_Internal { set => ClimbingSlope = value; }
        internal bool DescendingSlope_Internal { set => DescendingSlope = value; }
        internal float SlopeAngle_Internal { set => SlopeAngle = value; }
    }
}
