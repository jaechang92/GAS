// ================================
// File: Assets/GASTest/Camera2DFollow.cs
// 2D 카메라 추적 시스템
// ================================
using UnityEngine;

namespace TestScene
{
    /// <summary>
    /// 2D 플랫포머용 카메라 추적 시스템
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class Camera2DFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private bool autoFindPlayer = true;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
        [SerializeField] private float smoothSpeed = 0.125f;
        [SerializeField] private bool lockYAxis = false;
        [SerializeField] private float yLockPosition = 0f;

        [Header("Camera Bounds")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-20, -5);
        [SerializeField] private Vector2 maxBounds = new Vector2(20, 10);

        [Header("Look Ahead")]
        [SerializeField] private bool useLookAhead = true;
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private float lookAheadSpeed = 2f;

        [Header("Dead Zone")]
        [SerializeField] private bool useDeadZone = false;
        [SerializeField] private Vector2 deadZoneSize = new Vector2(2, 1);

        [Header("Camera Shake")]
        [SerializeField] private float shakeDecay = 0.5f;
        private float shakeIntensity = 0f;
        private Vector3 shakeOffset;

        // Runtime
        private Camera cam;
        private Vector3 velocity = Vector3.zero;
        private Vector3 desiredPosition;
        private float currentLookAheadX;
        private Vector3 lastTargetPosition;

        #region Unity Lifecycle

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            if (autoFindPlayer && target == null)
            {
                FindPlayer();
            }

            if (target != null)
            {
                lastTargetPosition = target.position;
                transform.position = GetDesiredPosition();
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                if (autoFindPlayer)
                {
                    FindPlayer();
                }
                return;
            }

            UpdateCameraPosition();
            UpdateCameraShake();
        }

        #endregion

        #region Camera Movement

        private void UpdateCameraPosition()
        {
            // 목표 위치 계산
            desiredPosition = GetDesiredPosition();

            // Look Ahead 적용
            if (useLookAhead)
            {
                ApplyLookAhead();
            }

            // Dead Zone 적용
            if (useDeadZone)
            {
                ApplyDeadZone();
            }

            // Y축 잠금
            if (lockYAxis)
            {
                desiredPosition.y = yLockPosition + offset.y;
            }

            // 경계 제한
            if (useBounds)
            {
                ApplyBounds();
            }

            // 부드러운 이동
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // 카메라 흔들림 적용
            transform.position = smoothedPosition + shakeOffset;

            // 마지막 위치 저장
            lastTargetPosition = target.position;
        }

        private Vector3 GetDesiredPosition()
        {
            return target.position + offset;
        }

        private void ApplyLookAhead()
        {
            // 타겟의 이동 방향 감지
            float targetVelocityX = (target.position.x - lastTargetPosition.x) / Time.deltaTime;

            // Look ahead 값 부드럽게 전환
            float targetLookAheadX = Mathf.Sign(targetVelocityX) * lookAheadDistance;
            currentLookAheadX = Mathf.Lerp(currentLookAheadX, targetLookAheadX, Time.deltaTime * lookAheadSpeed);

            // 적용
            desiredPosition.x += currentLookAheadX;
        }

        private void ApplyDeadZone()
        {
            Vector3 currentPos = transform.position;
            Vector3 targetPos = target.position;

            // X축 데드존
            float deltaX = targetPos.x - currentPos.x;
            if (Mathf.Abs(deltaX) > deadZoneSize.x / 2)
            {
                float direction = Mathf.Sign(deltaX);
                desiredPosition.x = targetPos.x - direction * (deadZoneSize.x / 2) + offset.x;
            }
            else
            {
                desiredPosition.x = currentPos.x;
            }

            // Y축 데드존
            if (!lockYAxis)
            {
                float deltaY = targetPos.y - currentPos.y;
                if (Mathf.Abs(deltaY) > deadZoneSize.y / 2)
                {
                    float direction = Mathf.Sign(deltaY);
                    desiredPosition.y = targetPos.y - direction * (deadZoneSize.y / 2) + offset.y;
                }
                else
                {
                    desiredPosition.y = currentPos.y;
                }
            }
        }

        private void ApplyBounds()
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        #endregion

        #region Camera Shake

        public void ShakeCamera(float intensity, float duration = 0.5f)
        {
            shakeIntensity = intensity;
            shakeDecay = duration;
        }

        private void UpdateCameraShake()
        {
            if (shakeIntensity > 0)
            {
                shakeOffset = Random.insideUnitSphere * shakeIntensity;
                shakeOffset.z = 0; // 2D이므로 Z축 제거

                shakeIntensity -= shakeDecay * Time.deltaTime;

                if (shakeIntensity <= 0)
                {
                    shakeIntensity = 0;
                    shakeOffset = Vector3.zero;
                }
            }
        }

        #endregion

        #region Public Methods

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                lastTargetPosition = target.position;
            }
        }

        public void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }

            if (player != null)
            {
                SetTarget(player.transform);
                Debug.Log("[Camera2DFollow] Player found and set as target");
            }
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
            useBounds = true;
        }

        public void DisableBounds()
        {
            useBounds = false;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // 카메라 경계
            if (useBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, transform.position.z);
                Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, transform.position.z);
                Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, transform.position.z);
                Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, transform.position.z);

                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(bottomRight, topRight);
                Gizmos.DrawLine(topRight, topLeft);
                Gizmos.DrawLine(topLeft, bottomLeft);
            }

            // 데드존
            if (useDeadZone)
            {
                Gizmos.color = new Color(1, 0, 0, 0.3f);
                Vector3 center = transform.position;
                center.z = 0;
                Gizmos.DrawWireCube(center, new Vector3(deadZoneSize.x, deadZoneSize.y, 0));
            }

            // 타겟 연결선
            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
            }
        }

        #endregion
    }
}