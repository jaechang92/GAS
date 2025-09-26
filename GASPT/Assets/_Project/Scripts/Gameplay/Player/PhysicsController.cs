using UnityEngine;
using System;

namespace Player
{
    /// <summary>
    /// 플레이어 물리 시스템 전용 클래스
    /// 단일책임원칙: 커스텀 물리 계산 및 이동 처리만 담당
    /// </summary>
    public class PhysicsController : MonoBehaviour
    {
        [Header("물리 설정")]
        [SerializeField] private float gravity = 30f;
        [SerializeField] private float maxFallSpeed = 20f;
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private bool enableGroundCorrection = true; // 지면 보정 활성화/비활성화

        // 물리 상태
        private Vector3 velocity;
        private Vector3 deltaMovement;
        private Vector3 previousVelocity;
        private float previousHorizontalSpeed;

        // 이벤트
        public event Action<Vector3> OnVelocityChanged;
        public event Action<Vector3> OnPositionChanged;

        // 프로퍼티
        public Vector3 Velocity => velocity;
        public Vector3 DeltaMovement => deltaMovement;
        public float PreviousHorizontalSpeed => previousHorizontalSpeed;
        public float Gravity => gravity;
        public float MaxFallSpeed => maxFallSpeed;

        private void Awake()
        {
            velocity = Vector3.zero;
            deltaMovement = Vector3.zero;
            previousVelocity = Vector3.zero;
            previousHorizontalSpeed = 0f;
        }

        private void FixedUpdate()
        {
            // 이전 프레임 상태 저장
            previousVelocity = velocity;
            previousHorizontalSpeed = Mathf.Abs(velocity.x);

            // 물리 업데이트
            ApplyCustomPhysics();
        }

        /// <summary>
        /// 커스텀 물리 시스템 적용
        /// </summary>
        private void ApplyCustomPhysics()
        {
            // 델타 이동량 계산
            deltaMovement = velocity * Time.fixedDeltaTime;

            // Transform 기반 이동 적용
            if (deltaMovement != Vector3.zero)
            {
                Vector3 oldPosition = transform.position;
                ApplyMovement();

                // 위치 변경 이벤트 발생
                if (transform.position != oldPosition)
                {
                    OnPositionChanged?.Invoke(transform.position);
                }
            }
        }

        /// <summary>
        /// 실제 이동 적용 (충돌 예측 포함)
        /// </summary>
        private void ApplyMovement()
        {
            if (enableGroundCorrection)
            {
                // 수직 이동에 대한 충돌 예측 및 보정
                Vector3 correctedMovement = PredictAndCorrectVerticalMovement(deltaMovement);
                transform.Translate(correctedMovement, Space.World);
            }
            else
            {
                // 기본 이동 (보정 없음)
                transform.Translate(deltaMovement, Space.World);
            }
        }

        /// <summary>
        /// 수직 이동 예측 및 지면 충돌 보정
        /// </summary>
        private Vector3 PredictAndCorrectVerticalMovement(Vector3 movement)
        {
            // 하강 중이 아니면 그대로 이동
            if (movement.y >= 0) return movement;

            Collider2D playerCollider = GetComponent<Collider2D>();
            if (playerCollider == null) return movement;

            // 현재 위치에서 이동 후 위치까지 레이캐스트
            LayerMask groundLayer = LayerMask.GetMask("Ground", "Platform");
            Vector2 rayOrigin = new Vector2(transform.position.x, playerCollider.bounds.min.y);
            float rayDistance = Mathf.Abs(movement.y) + 0.1f; // 이동 거리 + 여유분

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundLayer);

            if (hit.collider != null)
            {
                // 지면에 닿기 직전까지만 이동
                float safeDistance = hit.distance - 0.01f; // 1cm 여유 공간
                if (safeDistance < Mathf.Abs(movement.y))
                {
                    Vector3 correctedMovement = movement;
                    correctedMovement.y = -safeDistance;

                    if (enableDebugLogs)
                    {
                        Debug.Log($"[PhysicsController] 지면 충돌 예측 - 이동 보정: {movement.y:F3} → {correctedMovement.y:F3}");
                    }

                    return correctedMovement;
                }
            }

            return movement;
        }

        /// <summary>
        /// 속도 직접 설정
        /// </summary>
        public void SetVelocity(Vector3 newVelocity)
        {
            Vector3 oldVelocity = velocity;
            velocity = newVelocity;

            if (oldVelocity != velocity)
            {
                OnVelocityChanged?.Invoke(velocity);

                if (enableDebugLogs)
                {
                    Debug.Log($"[PhysicsController] 속도 변경: {oldVelocity} → {velocity}");
                }
            }
        }

        /// <summary>
        /// 속도에 값 추가
        /// </summary>
        public void AddVelocity(Vector3 additionalVelocity)
        {
            SetVelocity(velocity + additionalVelocity);
        }

        /// <summary>
        /// 수평 이동 설정
        /// </summary>
        public void SetHorizontalMovement(float horizontalInput, float speed)
        {
            Vector3 newVelocity = velocity;
            newVelocity.x = horizontalInput * speed;
            SetVelocity(newVelocity);
        }

        /// <summary>
        /// 점프 적용
        /// </summary>
        public void ApplyJump(float jumpPower)
        {
            Vector3 newVelocity = velocity;
            newVelocity.y = jumpPower;
            SetVelocity(newVelocity);

            if (enableDebugLogs)
            {
                Debug.Log($"[PhysicsController] 점프 적용: {jumpPower}");
            }
        }

        /// <summary>
        /// 중력 적용 (조건부)
        /// </summary>
        public void ApplyGravity(bool isGrounded, float multiplier = 1f)
        {
            // 접지 상태에서는 중력 적용하지 않음
            if (isGrounded)
            {
                // 접지 시 아래 방향 속도만 0으로 설정
                if (velocity.y < 0)
                {
                    Vector3 newVelocity = velocity;
                    newVelocity.y = 0;
                    SetVelocity(newVelocity);
                }
                return;
            }

            // 공중에서만 중력 적용
            Vector3 gravityVelocity = velocity;
            gravityVelocity.y -= gravity * multiplier * Time.deltaTime;

            // 최대 낙하 속도 제한
            if (gravityVelocity.y < -maxFallSpeed)
            {
                gravityVelocity.y = -maxFallSpeed;
            }

            SetVelocity(gravityVelocity);
        }

        /// <summary>
        /// 강제 중력 적용 (접지 상태 무시)
        /// </summary>
        public void ForceApplyGravity(float multiplier = 1f)
        {
            Vector3 gravityVelocity = velocity;
            gravityVelocity.y -= gravity * multiplier * Time.deltaTime;

            // 최대 낙하 속도 제한
            if (gravityVelocity.y < -maxFallSpeed)
            {
                gravityVelocity.y = -maxFallSpeed;
            }

            SetVelocity(gravityVelocity);
        }

        /// <summary>
        /// 착지 시 수직 속도 0으로 설정 및 위치 보정
        /// </summary>
        public void HandleGroundTouch()
        {
            if (velocity.y <= 0)
            {
                Vector3 newVelocity = velocity;
                newVelocity.y = 0;
                SetVelocity(newVelocity);

                // 지면으로 묻힌 위치 보정
                if (enableGroundCorrection)
                {
                    CorrectGroundPosition();
                }

                if (enableDebugLogs)
                {
                    Debug.Log("[PhysicsController] 착지 - 수직 속도 0 및 위치 보정");
                }
            }
        }

        /// <summary>
        /// 지면으로 묻힌 캐릭터 위치 보정
        /// </summary>
        private void CorrectGroundPosition()
        {
            Collider2D playerCollider = GetComponent<Collider2D>();
            if (playerCollider == null) return;

            // 플레이어 하단에서 지면까지 레이캐스트
            LayerMask groundLayer = LayerMask.GetMask("Ground", "Platform");
            Vector2 rayOrigin = new Vector2(transform.position.x, playerCollider.bounds.min.y);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 1f, groundLayer);

            if (hit.collider != null)
            {
                // 지면 위로 정확히 위치 조정
                float correctionDistance = hit.distance;
                if (correctionDistance < 0.05f) // 0.05f 이하로 묻혔을 때만 보정
                {
                    Vector3 correctedPosition = transform.position;
                    correctedPosition.y += correctionDistance + 0.01f; // 약간의 여유 공간
                    transform.position = correctedPosition;

                    if (enableDebugLogs)
                    {
                        Debug.Log($"[PhysicsController] 위치 보정: {correctionDistance:F3}만큼 위로 이동");
                    }
                }
            }
        }

        /// <summary>
        /// TODO: 향후 정교한 착지 물리 시스템에서 사용 예정
        /// 착지 시 운동량 보존을 위한 메서드
        /// </summary>
        public void PreserveLandingMomentum()
        {
            float currentSpeed = Mathf.Abs(velocity.x);

            // 착지 시 수평 속도가 감소했다면 이전 속도로 복구
            if (currentSpeed < previousHorizontalSpeed * 0.8f && previousHorizontalSpeed > 2f)
            {
                float direction = velocity.x >= 0 ? 1f : -1f;
                Vector3 newVelocity = velocity;
                newVelocity.x = direction * previousHorizontalSpeed;
                SetVelocity(newVelocity);

                if (enableDebugLogs)
                {
                    Debug.Log($"[PhysicsController] 착지 시 운동량 복구: {currentSpeed:F1} → {previousHorizontalSpeed:F1}");
                }
            }
        }

        /// <summary>
        /// 속도 정지
        /// </summary>
        public void Stop()
        {
            SetVelocity(Vector3.zero);
        }

        /// <summary>
        /// 수평 속도만 정지
        /// </summary>
        public void StopHorizontal()
        {
            Vector3 newVelocity = velocity;
            newVelocity.x = 0;
            SetVelocity(newVelocity);
        }

        /// <summary>
        /// 수직 속도만 정지
        /// </summary>
        public void StopVertical()
        {
            Vector3 newVelocity = velocity;
            newVelocity.y = 0;
            SetVelocity(newVelocity);
        }

        /// <summary>
        /// 물리 설정값 업데이트
        /// </summary>
        public void UpdatePhysicsSettings(float newGravity, float newMaxFallSpeed)
        {
            gravity = newGravity;
            maxFallSpeed = newMaxFallSpeed;

            if (enableDebugLogs)
            {
                Debug.Log($"[PhysicsController] 물리 설정 업데이트 - 중력: {gravity}, 최대낙하속도: {maxFallSpeed}");
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
        /// 현재 물리 상태 정보
        /// </summary>
        public string GetPhysicsInfo()
        {
            return $"Velocity: {velocity}, Speed: {velocity.magnitude:F2}, Ground: {velocity.y <= 0.1f}";
        }
    }
}