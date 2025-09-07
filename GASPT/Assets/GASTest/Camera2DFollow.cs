// ================================
// File: Assets/Scripts/TestScene/Camera2DFollow.cs
// 2D 플랫포머 카메라 팔로우 시스템
// ================================
using UnityEngine;

namespace TestScene
{
    /// <summary>
    /// 2D 플랫포머용 카메라 팔로우 시스템
    /// </summary>
    public class Camera2DFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private bool autoFindPlayer = true;

        [Header("Follow Settings")]
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector2 offset = new Vector2(0, 2f);
        [SerializeField] private bool followX = true;
        [SerializeField] private bool followY = true;

        [Header("Dead Zone")]
        [SerializeField] private bool useDeadZone = true;
        [SerializeField] private Vector2 deadZoneSize = new Vector2(2f, 2f);

        [Header("Bounds")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-30f, -10f);
        [SerializeField] private Vector2 maxBounds = new Vector2(30f, 20f);

        [Header("Look Ahead")]
        [SerializeField] private bool useLookAhead = true;
        [SerializeField] private float lookAheadDistance = 3f;
        [SerializeField] private float lookAheadSpeed = 2f;

        [Header("Camera Shake")]
        [SerializeField] private float shakeDecay = 2f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool showGizmos = true;

        // Internal state
        private Camera cam;
        private Vector3 desiredPosition;
        private Vector3 smoothedPosition;
        private Vector3 currentVelocity;
        private float currentLookAhead = 0f;
        private Vector2 lastTargetPosition;

        // Camera shake
        private float shakeIntensity = 0f;
        private float shakeTime = 0f;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }
        }

        private void Start()
        {
            if (autoFindPlayer && target == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    SetTarget(player.transform);
                }
            }

            if (target != null)
            {
                // 즉시 타겟 위치로 이동
                transform.position = GetTargetPosition();
                lastTargetPosition = target.position;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            UpdateCameraPosition();
            UpdateCameraShake();

            if (showDebugInfo && Input.GetKeyDown(KeyCode.F2))
            {
                PrintDebugInfo();
            }
        }

        /// <summary>
        /// 카메라 위치 업데이트
        /// </summary>
        private void UpdateCameraPosition()
        {
            Vector3 targetPos = GetTargetPosition();

            // Dead Zone 적용
            if (useDeadZone)
            {
                targetPos = ApplyDeadZone(targetPos);
            }

            // Look Ahead 적용
            if (useLookAhead)
            {
                targetPos = ApplyLookAhead(targetPos);
            }

            // Bounds 적용
            if (useBounds)
            {
                targetPos = ApplyBounds(targetPos);
            }

            // Smooth 이동
            desiredPosition = targetPos;
            smoothedPosition = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref currentVelocity,
                1f / smoothSpeed
            );

            transform.position = smoothedPosition;

            // 타겟 위치 저장
            lastTargetPosition = target.position;
        }

        /// <summary>
        /// 타겟 위치 계산
        /// </summary>
        private Vector3 GetTargetPosition()
        {
            Vector3 targetPos = target.position;

            // 오프셋 적용
            targetPos.x += offset.x;
            targetPos.y += offset.y;

            // 축별 팔로우 설정
            if (!followX)
            {
                targetPos.x = transform.position.x;
            }

            if (!followY)
            {
                targetPos.y = transform.position.y;
            }

            // Z 위치는 카메라 위치 유지
            targetPos.z = transform.position.z;

            return targetPos;
        }

        /// <summary>
        /// Dead Zone 적용
        /// </summary>
        private Vector3 ApplyDeadZone(Vector3 targetPos)
        {
            Vector3 currentPos = transform.position;

            // X축 Dead Zone
            float deltaX = targetPos.x - currentPos.x;
            if (Mathf.Abs(deltaX) < deadZoneSize.x / 2f)
            {
                targetPos.x = currentPos.x;
            }
            else
            {
                float sign = Mathf.Sign(deltaX);
                targetPos.x = currentPos.x + sign * (Mathf.Abs(deltaX) - deadZoneSize.x / 2f);
            }

            // Y축 Dead Zone
            float deltaY = targetPos.y - currentPos.y;
            if (Mathf.Abs(deltaY) < deadZoneSize.y / 2f)
            {
                targetPos.y = currentPos.y;
            }
            else
            {
                float sign = Mathf.Sign(deltaY);
                targetPos.y = currentPos.y + sign * (Mathf.Abs(deltaY) - deadZoneSize.y / 2f);
            }

            return targetPos;
        }

        /// <summary>
        /// Look Ahead 적용
        /// </summary>
        private Vector3 ApplyLookAhead(Vector3 targetPos)
        {
            // 타겟의 이동 방향 계산
            Vector2 targetVelocity = ((Vector2)target.position - lastTargetPosition) / Time.deltaTime;

            if (targetVelocity.magnitude > 0.1f)
            {
                // 이동 중일 때 Look Ahead 적용
                float targetLookAhead = Mathf.Sign(targetVelocity.x) * lookAheadDistance;
                currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);
            }
            else
            {
                // 정지 시 Look Ahead 감소
                currentLookAhead = Mathf.Lerp(currentLookAhead, 0, lookAheadSpeed * Time.deltaTime);
            }

            targetPos.x += currentLookAhead;

            return targetPos;
        }

        /// <summary>
        /// Bounds 적용
        /// </summary>
        private Vector3 ApplyBounds(Vector3 targetPos)
        {
            // 카메라 뷰포트 크기 계산
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            // 바운드 내로 제한
            targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
            targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

            return targetPos;
        }

        /// <summary>
        /// 카메라 흔들기 업데이트
        /// </summary>
        private void UpdateCameraShake()
        {
            if (shakeIntensity > 0)
            {
                Vector3 shakeOffset = Random.insideUnitCircle * shakeIntensity;
                transform.position += shakeOffset;

                shakeIntensity = Mathf.Lerp(shakeIntensity, 0, shakeDecay * Time.deltaTime);
                shakeTime -= Time.deltaTime;

                if (shakeTime <= 0)
                {
                    shakeIntensity = 0;
                }
            }
        }

        /// <summary>
        /// 타겟 설정
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                lastTargetPosition = target.position;
            }
        }

        /// <summary>
        /// 카메라 흔들기
        /// </summary>
        public void Shake(float intensity, float duration)
        {
            shakeIntensity = intensity;
            shakeTime = duration;
        }

        /// <summary>
        /// 특정 위치로 즉시 이동
        /// </summary>
        public void SnapToTarget()
        {
            if (target != null)
            {
                transform.position = GetTargetPosition();
                currentVelocity = Vector3.zero;
                currentLookAhead = 0;
            }
        }

        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        private void PrintDebugInfo()
        {
            Debug.Log("=== Camera2D Follow Debug ===");
            Debug.Log($"Camera Position: {transform.position}");
            Debug.Log($"Target Position: {(target != null ? target.position.ToString() : "None")}");
            Debug.Log($"Desired Position: {desiredPosition}");
            Debug.Log($"Current Velocity: {currentVelocity}");
            Debug.Log($"Look Ahead: {currentLookAhead}");
            Debug.Log($"Shake Intensity: {shakeIntensity}");
            Debug.Log("==============================");
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Camera gizmoCam = cam != null ? cam : Camera.main;
            if (gizmoCam == null) return;

            // Dead Zone 표시
            if (useDeadZone)
            {
                Gizmos.color = new Color(1, 1, 0, 0.3f);
                Vector3 center = Application.isPlaying ? transform.position : gizmoCam.transform.position;
                Gizmos.DrawCube(center, new Vector3(deadZoneSize.x, deadZoneSize.y, 0.1f));
            }

            // Bounds 표시
            if (useBounds)
            {
                Gizmos.color = Color.red;
                Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, 0);
                Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, 0);
                Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, 0);
                Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, 0);

                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(bottomRight, topRight);
                Gizmos.DrawLine(topRight, topLeft);
                Gizmos.DrawLine(topLeft, bottomLeft);
            }

            // Camera View 표시
            if (gizmoCam.orthographic)
            {
                float halfHeight = gizmoCam.orthographicSize;
                float halfWidth = halfHeight * gizmoCam.aspect;

                Gizmos.color = Color.white;
                Vector3 camPos = gizmoCam.transform.position;
                Vector3 bl = camPos + new Vector3(-halfWidth, -halfHeight, 0);
                Vector3 br = camPos + new Vector3(halfWidth, -halfHeight, 0);
                Vector3 tr = camPos + new Vector3(halfWidth, halfHeight, 0);
                Vector3 tl = camPos + new Vector3(-halfWidth, halfHeight, 0);

                Gizmos.DrawLine(bl, br);
                Gizmos.DrawLine(br, tr);
                Gizmos.DrawLine(tr, tl);
                Gizmos.DrawLine(tl, bl);
            }

            // Target 표시
            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(target.position, 0.5f);

                if (Application.isPlaying)
                {
                    Gizmos.DrawLine(transform.position, target.position);
                }
            }
        }
    }
}