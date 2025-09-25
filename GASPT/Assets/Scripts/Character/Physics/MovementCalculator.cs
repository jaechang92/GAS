// 파일 위치: Assets/Scripts/Character/Physics/MovementCalculator.cs
// UpdatePhysics 메서드 중심 - 중력 제거

using UnityEngine;

namespace Character.Physics
{
    public class MovementCalculator : MonoBehaviour
    {
        [SerializeField] private CharacterPhysicsConfig config;
        [SerializeField] private RaycastController raycastController;

        [Header("Ground Correction")]
        [SerializeField] private bool useGroundCorrection = true;
        [SerializeField] private float maxGroundCorrectionDistance = 0.1f;

        private PhysicsData physicsData;
        private CollisionData collisionData;
        private Vector2 currentPosition;
        private Vector2 targetVelocity;

        // 이전 프레임 데이터
        private bool wasGroundedLastFrame;
        private float lastGroundedY;

        private void Awake()
        {
            if (raycastController == null)
                raycastController = GetComponent<RaycastController>();

            physicsData = PhysicsData.Default;
            collisionData = new CollisionData(); // 초기화 추가
            currentPosition = transform.position;
        }

        public void UpdatePhysics(float deltaTime)
        {
            // 이전 프레임 상태 기록
            wasGroundedLastFrame = collisionData.isGrounded;
            if (collisionData.isGrounded)
            {
                lastGroundedY = transform.position.y;
            }

            // 1. Apply Forces
            ApplyForces(deltaTime);

            // 2. Check Collisions
            collisionData = raycastController.CheckCollisions(physicsData.velocity);

            // 3. Resolve Collisions
            ResolveCollisions();

            if (useGroundCorrection)
            {
                ApplyGroundCorrection();
            }

            // 4. Apply Friction/Air Resistance - Skul 스타일 적용 상당히 다른 느낌 있음
            ApplySkulStyleFriction(deltaTime);

            // 5. Clamp Velocity - Skul 스타일
            ClampVelocity();

            // 6. Move Transform
            ApplyMovement(deltaTime);

            // 7. Reset External Forces
            physicsData.externalForce = Vector2.zero;
        }


        private void ApplyForces(float deltaTime)
        {
            if (physicsData.externalForce == Vector2.zero) return;

            Vector2 acceleration = physicsData.externalForce / physicsData.mass;
            physicsData.velocity += acceleration * deltaTime;
        }

        private void ResolveCollisions()
        {
            // Ground Collision
            if (collisionData.isGrounded && physicsData.velocity.y < 0)
            {
                physicsData.velocity.y = 0;
            }

            // Ceiling Collision
            if (collisionData.isCeiling && physicsData.velocity.y > 0)
            {
                physicsData.velocity.y = 0;
            }

            // Wall Collisions
            if (collisionData.isWallLeft && physicsData.velocity.x < 0)
            {
                physicsData.velocity.x = 0;
            }

            if (collisionData.isWallRight && physicsData.velocity.x > 0)
            {
                physicsData.velocity.x = 0;
            }

            // Slope Adjustment
            if (collisionData.isOnSlope && collisionData.isGrounded)
            {
                AdjustVelocityForSlope();
            }
        }

        private void CorrectGroundPosition()
        {
            if (collisionData.groundHit)
            {
                BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
                if (boxCollider != null)
                {
                    // Ground 표면 위에 정확한 y 위치 설정
                    float targetY = collisionData.groundHit.point.y +
                                   (boxCollider.size.y * 0.5f) +
                                   raycastController.GetSkinWidth();

                    Vector3 pos = transform.position;
                    float yDiff = targetY - pos.y;

                    // 너무 극단적인 보정은 방지하고 부드럽게 보정
                    if (Mathf.Abs(yDiff) < maxGroundCorrectionDistance)
                    {
                        pos.y = targetY;
                        transform.position = pos;

                        if (config.enableDebugLogs)
                        {
                            Debug.Log($"[Movement] Ground position corrected by {yDiff:F4}");
                        }
                    }
                }
            }
        }

        private void ApplyGroundCorrection()
        {
            if (!collisionData.isGrounded) return;

            // Ground에 파묻혔거나 떠있는 것 보정
            if (collisionData.groundDistance < raycastController.GetSkinWidth() * 0.9f)
            {
                // 너무 가까움 - 위로 밀어냄
                float pushUp = raycastController.GetSkinWidth() - collisionData.groundDistance;
                transform.position += Vector3.up * pushUp;

                if (config.enableDebugLogs)
                {
                    Debug.Log($"[Movement] Pushed up from ground by {pushUp:F4}");
                }
            }
            else if (collisionData.groundDistance > raycastController.GetSkinWidth() * 1.5f &&
                     collisionData.groundDistance < maxGroundCorrectionDistance)
            {
                // 약간 떠있음 - 아래로 당김
                if (physicsData.velocity.y <= 0)
                {
                    float snapDown = collisionData.groundDistance - raycastController.GetSkinWidth();
                    transform.position += Vector3.down * snapDown;
                    physicsData.velocity.y = 0;

                    if (config.enableDebugLogs)
                    {
                        Debug.Log($"[Movement] Snapped down to ground by {snapDown:F4}");
                    }
                }
            }
        }

        private void AdjustVelocityForSlope()
        {
            float targetVelocityX = physicsData.velocity.x;
            Vector2 slopeDirection = Vector2.Perpendicular(collisionData.slopeNormal).normalized;

            if (Vector2.Dot(slopeDirection, Vector2.right) * targetVelocityX < 0)
            {
                slopeDirection = -slopeDirection;
            }

            physicsData.velocity = slopeDirection * Mathf.Abs(targetVelocityX);
        }

        // Skul 스타일 마찰력 (훨씬 반응성 좋음)
        private void ApplySkulStyleFriction(float deltaTime)
        {
            if (collisionData.isGrounded)
            {
                // 지면: 즉시 반응
                if (Mathf.Abs(targetVelocity.x) < 0.1f)
                {
                    // 입력이 없으면 빠르게 정지
                    physicsData.velocity.x = Mathf.MoveTowards(physicsData.velocity.x, 0,
                        config.groundDeceleration * deltaTime);
                }
                else
                {
                    // 입력이 있으면 빠른 반응
                    physicsData.velocity.x = Mathf.MoveTowards(physicsData.velocity.x, targetVelocity.x,
                        config.groundAcceleration * deltaTime);
                }
            }
            else
            {
                // 공중: 약간 느린반응
                float airControl = config.airControlMultiplier;
                physicsData.velocity.x = Mathf.MoveTowards(physicsData.velocity.x, targetVelocity.x,
                    config.airAcceleration * airControl * deltaTime);
            }
        }

        private void ClampVelocity()
        {
            // Clamp horizontal velocity
            physicsData.velocity.x = Mathf.Clamp(physicsData.velocity.x, -config.maxMoveSpeed, config.maxMoveSpeed);

            // Skul 스타일: Y 속도는 State에서 직접 제어하므로 여기서는 최소한의 제한
            // 중력 대신 상태 기반 최대 속도만 제한
            if (physicsData.velocity.y < config.maxFallSpeed)
            {
                physicsData.velocity.y = config.maxFallSpeed;
            }

            // Ground에 있을 때 미세한 y 속도 제거
            if (collisionData.isGrounded && Mathf.Abs(physicsData.velocity.y) < 0.01f)
            {
                physicsData.velocity.y = 0;
            }
        }

        private void ApplyMovement(float deltaTime)
        {
            Vector2 movement = physicsData.velocity * deltaTime;

            if (!collisionData.isGrounded && physicsData.velocity.y < 0)
            {
                // 미래 프레임에 착지할 것으로 예측되면
                RaycastHit2D predictedGround = Physics2D.Raycast(
                    transform.position + (Vector3)movement,
                    Vector2.down,
                    0.1f,
                    raycastController.GetCollisionMask());

                if (predictedGround)
                {
                    BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
                    if (boxCollider != null)
                    {
                        // 정확한 착지 위치로 정확히 이동
                        float targetY = predictedGround.point.y +
                                       (boxCollider.size.y * 0.5f) +
                                       raycastController.GetSkinWidth();

                        movement.y = targetY - transform.position.y;

                        // 착지 했으므로 속도를 이미 0으로
                        physicsData.velocity.y = 0;
                    }
                }
            }

            // Moving Platform Support
            if (collisionData.isOnMovingPlatform && collisionData.platformTransform != null)
            {
                movement += collisionData.platformVelocity * deltaTime;
            }

            transform.Translate(movement);

            // 이동 후 미세 조정
            if (collisionData.isGrounded)
            {
                // Ground에 있을 때 y 위치 정규화
                Vector3 pos = transform.position;
                pos.y = Mathf.Round(pos.y * 1000f) / 1000f; // 소수점 3자리까지
                transform.position = pos;
            }
        }

        // Public Movement Methods
        public void Move(float horizontal)
        {
            targetVelocity.x = horizontal * config.moveSpeed;

            // 방향 전환 시 더 빠른 반응
            bool isTurningAround = Mathf.Sign(physicsData.velocity.x) != Mathf.Sign(horizontal) &&
                                  Mathf.Abs(physicsData.velocity.x) > 0.1f;

            float acceleration = config.GetAcceleration(collisionData.isGrounded, isTurningAround);
            physicsData.velocity.x = Mathf.MoveTowards(physicsData.velocity.x, targetVelocity.x,
                acceleration * Time.deltaTime);

        }

        public void SetVelocity(Vector2 velocity)
        {
            physicsData.velocity = velocity;
        }

        public void AddForce(Vector2 force)
        {
            physicsData.AddForce(force);
        }

        public void Stop()
        {
            physicsData.velocity = Vector2.zero;
            targetVelocity = Vector2.zero;
        }

        // Getters
        public PhysicsData GetPhysicsData() => physicsData;
        public CollisionData GetCollisionData() => collisionData;
        public Vector2 GetVelocity() => physicsData.velocity;
        public bool IsGrounded() => collisionData.isGrounded;
        public bool IsAgainstWall() => collisionData.IsAgainstWall;
    }
}