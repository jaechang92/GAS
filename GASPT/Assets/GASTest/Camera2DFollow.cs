// ================================
// File: Assets/Scripts/TestScene/Camera2DFollow.cs
// 2D �÷����� ī�޶� �ȷο� �ý���
// ================================
using UnityEngine;

namespace TestScene
{
    /// <summary>
    /// 2D �÷����ӿ� ī�޶� �ȷο� �ý���
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
                // ��� Ÿ�� ��ġ�� �̵�
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
        /// ī�޶� ��ġ ������Ʈ
        /// </summary>
        private void UpdateCameraPosition()
        {
            Vector3 targetPos = GetTargetPosition();

            // Dead Zone ����
            if (useDeadZone)
            {
                targetPos = ApplyDeadZone(targetPos);
            }

            // Look Ahead ����
            if (useLookAhead)
            {
                targetPos = ApplyLookAhead(targetPos);
            }

            // Bounds ����
            if (useBounds)
            {
                targetPos = ApplyBounds(targetPos);
            }

            // Smooth �̵�
            desiredPosition = targetPos;
            smoothedPosition = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref currentVelocity,
                1f / smoothSpeed
            );

            transform.position = smoothedPosition;

            // Ÿ�� ��ġ ����
            lastTargetPosition = target.position;
        }

        /// <summary>
        /// Ÿ�� ��ġ ���
        /// </summary>
        private Vector3 GetTargetPosition()
        {
            Vector3 targetPos = target.position;

            // ������ ����
            targetPos.x += offset.x;
            targetPos.y += offset.y;

            // �ະ �ȷο� ����
            if (!followX)
            {
                targetPos.x = transform.position.x;
            }

            if (!followY)
            {
                targetPos.y = transform.position.y;
            }

            // Z ��ġ�� ī�޶� ��ġ ����
            targetPos.z = transform.position.z;

            return targetPos;
        }

        /// <summary>
        /// Dead Zone ����
        /// </summary>
        private Vector3 ApplyDeadZone(Vector3 targetPos)
        {
            Vector3 currentPos = transform.position;

            // X�� Dead Zone
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

            // Y�� Dead Zone
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
        /// Look Ahead ����
        /// </summary>
        private Vector3 ApplyLookAhead(Vector3 targetPos)
        {
            // Ÿ���� �̵� ���� ���
            Vector2 targetVelocity = ((Vector2)target.position - lastTargetPosition) / Time.deltaTime;

            if (targetVelocity.magnitude > 0.1f)
            {
                // �̵� ���� �� Look Ahead ����
                float targetLookAhead = Mathf.Sign(targetVelocity.x) * lookAheadDistance;
                currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);
            }
            else
            {
                // ���� �� Look Ahead ����
                currentLookAhead = Mathf.Lerp(currentLookAhead, 0, lookAheadSpeed * Time.deltaTime);
            }

            targetPos.x += currentLookAhead;

            return targetPos;
        }

        /// <summary>
        /// Bounds ����
        /// </summary>
        private Vector3 ApplyBounds(Vector3 targetPos)
        {
            // ī�޶� ����Ʈ ũ�� ���
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            // �ٿ�� ���� ����
            targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
            targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

            return targetPos;
        }

        /// <summary>
        /// ī�޶� ���� ������Ʈ
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
        /// Ÿ�� ����
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
        /// ī�޶� ����
        /// </summary>
        public void Shake(float intensity, float duration)
        {
            shakeIntensity = intensity;
            shakeTime = duration;
        }

        /// <summary>
        /// Ư�� ��ġ�� ��� �̵�
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
        /// ����� ���� ���
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

            // Dead Zone ǥ��
            if (useDeadZone)
            {
                Gizmos.color = new Color(1, 1, 0, 0.3f);
                Vector3 center = Application.isPlaying ? transform.position : gizmoCam.transform.position;
                Gizmos.DrawCube(center, new Vector3(deadZoneSize.x, deadZoneSize.y, 0.1f));
            }

            // Bounds ǥ��
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

            // Camera View ǥ��
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

            // Target ǥ��
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