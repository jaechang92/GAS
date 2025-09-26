using UnityEngine;
using System;

namespace Character.Physics
{
    /// <summary>
    /// 통합 물리 상태 관리자
    /// 단일 책임: 모든 물리 상태의 추적 및 변경 이벤트 관리
    /// </summary>
    public class PhysicsState : MonoBehaviour, IPhysicsState
    {
        [Header("현재 상태")]
        [SerializeField] private Vector3 velocity;
        [SerializeField] private Vector3 previousVelocity;
        [SerializeField] private float horizontalInput;

        [Header("접지 상태")]
        [SerializeField] private bool isGrounded;
        [SerializeField] private bool wasGrounded;
        [SerializeField] private float timeOnGround;
        [SerializeField] private float timeInAir;
        [SerializeField] private float coyoteTimeRemaining;

        // 접지 상태 안정성을 위한 필터링
        [SerializeField] private int groundedFrameCount = 0;
        [SerializeField] private int airborneFrameCount = 0;
        private const int GROUND_DETECTION_THRESHOLD = 3; // 3프레임 연속으로 감지되어야 상태 변경
        private const int AIRBORNE_DETECTION_THRESHOLD = 5; // 5프레임 연속으로 공중 상태여야 변경 (더 보수적)

        [Header("벽 상태")]
        [SerializeField] private bool isTouchingWall;
        [SerializeField] private bool wasTouchingWall;
        [SerializeField] private int wallDirection;
        [SerializeField] private float timeOnWall;

        // 설정
        private CharacterPhysicsConfig config;

        // 이벤트
        public event Action<Vector3> OnVelocityChanged;
        public event Action OnGroundedChanged;
        public event Action OnWallTouchChanged;

        #region IPhysicsState 구현

        // IMovementState
        public Vector3 Velocity => velocity;
        public Vector3 PreviousVelocity => previousVelocity;
        public float HorizontalInput => horizontalInput;
        public bool IsMoving => Mathf.Abs(velocity.x) > 0.1f;
        public float MovementSpeed => config?.moveSpeed ?? 8f;

        // IGroundState
        public bool IsGrounded => isGrounded;
        public bool WasGrounded => wasGrounded;
        public float TimeOnGround => timeOnGround;
        public float TimeInAir => timeInAir;
        public float CoyoteTime => coyoteTimeRemaining;
        public bool CanCoyoteJump => coyoteTimeRemaining > 0 && !isGrounded;

        // IWallState
        public bool IsTouchingWall => isTouchingWall;
        public bool WasTouchingWall => wasTouchingWall;
        public int WallDirection => wallDirection;
        public float TimeOnWall => timeOnWall;
        public bool CanWallJump => isTouchingWall && config?.allowWallJump == true;

        #endregion

        public void Initialize(CharacterPhysicsConfig physicsConfig)
        {
            config = physicsConfig;
            ResetState();
        }

        public void UpdateState(ICollisionInfo collisionInfo, float deltaTime)
        {
            previousVelocity = velocity;

            // 접지 상태 업데이트
            UpdateGroundState(collisionInfo, deltaTime);

            // 벽 상태 업데이트
            UpdateWallState(collisionInfo, deltaTime);

            // 중력 적용
            if (config != null)
            {
                ApplyGravity(deltaTime);
            }
        }

        /// <summary>
        /// 접지/벽 상태만 업데이트 (중력 적용 전)
        /// </summary>
        public void UpdateGroundAndWallState(ICollisionInfo collisionInfo, float deltaTime)
        {
            previousVelocity = velocity;

            // 접지 상태 업데이트
            UpdateGroundState(collisionInfo, deltaTime);

            // 벽 상태 업데이트
            UpdateWallState(collisionInfo, deltaTime);
        }

        /// <summary>
        /// 중력 및 물리 효과 적용 (점프/이동 처리 후)
        /// </summary>
        public void ApplyGravityAndPhysics(float deltaTime)
        {
            if (config != null)
            {
                ApplyGravity(deltaTime);
            }
        }

        public void SetVelocity(Vector3 newVelocity)
        {
            if (velocity != newVelocity)
            {
                previousVelocity = velocity;
                velocity = newVelocity;
                OnVelocityChanged?.Invoke(velocity);
            }
        }

        public void AddVelocity(Vector3 additionalVelocity)
        {
            SetVelocity(velocity + additionalVelocity);
        }

        public void ForceGroundState(bool grounded)
        {
            if (isGrounded != grounded)
            {
                wasGrounded = isGrounded;
                isGrounded = grounded;

                if (grounded)
                {
                    timeOnGround = 0;
                    coyoteTimeRemaining = 0;
                }
                else
                {
                    timeInAir = 0;
                    coyoteTimeRemaining = config?.coyoteTime ?? 0.1f;
                }

                OnGroundedChanged?.Invoke();
            }
        }

        public void ForceWallState(bool touching, int direction = 0)
        {
            if (isTouchingWall != touching)
            {
                wasTouchingWall = isTouchingWall;
                isTouchingWall = touching;
                wallDirection = direction;

                if (touching)
                {
                    timeOnWall = 0;
                }

                OnWallTouchChanged?.Invoke();
            }
        }

        public void SetHorizontalInput(float input)
        {
            horizontalInput = input;
        }

        #region 내부 업데이트 메서드

        private void UpdateGroundState(ICollisionInfo collisionInfo, float deltaTime)
        {
            wasGrounded = isGrounded;
            bool rawGroundDetection = collisionInfo.Below;

            // 안정성을 위한 필터링 적용
            bool filteredGroundState = ApplyGroundStateFilter(rawGroundDetection);

            if (isGrounded != filteredGroundState)
            {
                isGrounded = filteredGroundState;

                if (isGrounded)
                {
                    timeOnGround = 0;
                    timeInAir = 0;
                    coyoteTimeRemaining = 0;
                    if (config != null && config.enableDebugLogs)
                    {
                        Debug.Log($"[PhysicsState] 접지 상태로 변경 (필터링 적용)");
                    }
                }
                else
                {
                    timeOnGround = 0;
                    timeInAir = 0;
                    coyoteTimeRemaining = config?.coyoteTime ?? 0.1f;
                    if (config != null && config.enableDebugLogs)
                    {
                        Debug.Log($"[PhysicsState] 공중 상태로 변경 (필터링 적용)");
                    }
                }

                OnGroundedChanged?.Invoke();
            }

            // 시간 업데이트
            if (isGrounded)
            {
                timeOnGround += deltaTime;
            }
            else
            {
                timeInAir += deltaTime;
                if (coyoteTimeRemaining > 0)
                {
                    coyoteTimeRemaining -= deltaTime;
                }
            }
        }

        /// <summary>
        /// 접지 상태 감지에 필터링을 적용하여 안정성 확보
        /// </summary>
        private bool ApplyGroundStateFilter(bool rawGroundDetection)
        {
            if (rawGroundDetection)
            {
                groundedFrameCount++;
                airborneFrameCount = 0;

                // 현재 공중 상태인데 연속으로 바닥 감지되면 접지 상태로 전환
                if (!isGrounded && groundedFrameCount >= GROUND_DETECTION_THRESHOLD)
                {
                    // 추가 안정성 체크: 속도도 확인
                    if (velocity.y <= 0.1f) // 위로 빠르게 이동 중이면 접지 상태로 전환하지 않음
                    {
                        return true;
                    }
                }
                // 이미 접지 상태라면 유지
                else if (isGrounded)
                {
                    return true;
                }
            }
            else
            {
                airborneFrameCount++;
                groundedFrameCount = 0;

                // 현재 접지 상태인데 연속으로 바닥 감지 안되면 공중 상태로 전환
                // 더 보수적인 임계값 사용
                if (isGrounded && airborneFrameCount >= AIRBORNE_DETECTION_THRESHOLD)
                {
                    // 추가 안정성 체크: 실제로 위로 이동 중이거나 확실히 공중에 있을 때만
                    if (velocity.y > 0.5f || timeInAir > 0.1f)
                    {
                        return false;
                    }
                }
                // 이미 공중 상태라면 유지
                else if (!isGrounded)
                {
                    return false;
                }
            }

            // 변화가 임계값에 도달하지 않았으면 현재 상태 유지
            return isGrounded;
        }

        private void UpdateWallState(ICollisionInfo collisionInfo, float deltaTime)
        {
            wasTouchingWall = isTouchingWall;
            bool newWallState = collisionInfo.Left || collisionInfo.Right;
            int newWallDirection = 0;

            if (collisionInfo.Left) newWallDirection = -1;
            else if (collisionInfo.Right) newWallDirection = 1;

            if (isTouchingWall != newWallState || wallDirection != newWallDirection)
            {
                isTouchingWall = newWallState;
                wallDirection = newWallDirection;

                if (isTouchingWall)
                {
                    timeOnWall = 0;
                }

                OnWallTouchChanged?.Invoke();
            }

            // 시간 업데이트
            if (isTouchingWall)
            {
                timeOnWall += deltaTime;
            }
        }

        private void ApplyGravity(float deltaTime)
        {
            if (config != null && config.enableDebugLogs)
            {
                Debug.Log($"[PhysicsState] ApplyGravity - isGrounded: {isGrounded}, velocity.y: {velocity.y:F2}, gravity: {config.gravity}");
            }

            if (!isGrounded && config != null)
            {
                // 상승 중인지 하강 중인지에 따라 다른 중력 적용
                float gravityMultiplier = velocity.y > 0 ? config.upwardGravityMultiplier : config.downwardGravityMultiplier;
                float gravityForce = config.gravity * gravityMultiplier * deltaTime;
                velocity.y -= gravityForce;

                if (config.enableDebugLogs)
                {
                    Debug.Log($"[PhysicsState] 중력 적용됨 - gravityForce: {gravityForce:F2}, 결과 velocity.y: {velocity.y:F2}");
                }

                // 최대 낙하 속도 제한
                if (velocity.y < -config.maxFallSpeed)
                {
                    velocity.y = -config.maxFallSpeed;
                }
            }
            else if (isGrounded)
            {
                // 접지 상태일 때는 하향 속도만 0으로 제한 (점프는 허용)
                if (velocity.y < 0)
                {
                    if (config != null && config.enableDebugLogs)
                    {
                        Debug.Log($"[PhysicsState] 접지 상태 - velocity.y를 0으로 설정");
                    }
                    velocity.y = 0;
                }
            }
        }

        #endregion

        private void ResetState()
        {
            velocity = Vector3.zero;
            previousVelocity = Vector3.zero;
            horizontalInput = 0f;

            isGrounded = false;
            wasGrounded = false;
            timeOnGround = 0f;
            timeInAir = 0f;
            coyoteTimeRemaining = 0f;

            // 필터링 상태도 초기화
            groundedFrameCount = 0;
            airborneFrameCount = 0;

            isTouchingWall = false;
            wasTouchingWall = false;
            wallDirection = 0;
            timeOnWall = 0f;
        }

        #region 디버그

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            // 속도 벡터 (크기 조정)
            if (velocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.blue;
                Vector3 scaledVelocity = velocity.normalized * Mathf.Min(velocity.magnitude * 0.3f, 1f);
                Gizmos.DrawLine(transform.position, transform.position + scaledVelocity);

                // 속도 방향 표시
                Gizmos.color = Color.cyan;
                Vector3 arrowHead = transform.position + scaledVelocity;
                Gizmos.DrawWireSphere(arrowHead, 0.03f);
            }

            // 상태 표시
            var pos = transform.position;

            if (isGrounded)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(pos + Vector3.down * 0.7f, Vector3.one * 0.2f);
            }

            if (isTouchingWall)
            {
                Gizmos.color = Color.red;
                var wallPos = pos + Vector3.right * wallDirection * 0.7f;
                Gizmos.DrawWireCube(wallPos, Vector3.one * 0.2f);
            }

            if (CanCoyoteJump)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(pos + Vector3.up * 0.7f, 0.1f);
            }
        }

        #endregion
    }
}
