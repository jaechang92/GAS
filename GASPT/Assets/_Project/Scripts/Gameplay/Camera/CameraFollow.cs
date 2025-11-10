using UnityEngine;

namespace GASPT.Gameplay.Camera
{
    /// <summary>
    /// 2D 카메라 플레이어 추적
    /// SmoothDamp로 부드럽게 플레이어를 따라감
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        // ====== 타겟 설정 ======

        [Header("추적 타겟")]
        [Tooltip("따라갈 타겟 (플레이어) - null이면 \"Player\" 태그로 자동 탐색")]
        [SerializeField] private Transform target;


        // ====== 추적 설정 ======

        [Header("추적 설정")]
        [Tooltip("카메라 오프셋 (타겟 기준 상대 위치)")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);

        [Tooltip("부드러운 이동 시간 (작을수록 빠름, 0이면 즉시)")]
        [SerializeField] private float smoothTime = 0.3f;

        [Tooltip("X축 추적 활성화")]
        [SerializeField] private bool followX = true;

        [Tooltip("Y축 추적 활성화")]
        [SerializeField] private bool followY = true;


        // ====== 카메라 경계 (선택사항) ======

        [Header("카메라 경계 (선택사항)")]
        [Tooltip("카메라 이동 범위 제한 활성화")]
        [SerializeField] private bool useBounds = false;

        [Tooltip("최소 경계 (왼쪽 아래)")]
        [SerializeField] private Vector2 minBounds = new Vector2(-100f, -100f);

        [Tooltip("최대 경계 (오른쪽 위)")]
        [SerializeField] private Vector2 maxBounds = new Vector2(100f, 100f);


        // ====== 디버그 ======

        [Header("디버그")]
        [SerializeField] private bool showDebugLogs = false;
        [SerializeField] private bool showBoundsGizmos = true;


        // ====== 상태 ======

        private Vector3 velocity = Vector3.zero;


        // ====== 프로퍼티 ======

        public Transform Target => target;
        public Vector3 Offset => offset;


        // ====== Unity 생명주기 ======

        private void Start()
        {
            // 타겟 자동 탐색
            if (target == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    target = playerObj.transform;
                    if (showDebugLogs)
                        Debug.Log($"[CameraFollow] 플레이어 자동 탐색 완료: {target.name}");
                }
                else
                {
                    Debug.LogWarning("[CameraFollow] \"Player\" 태그를 가진 GameObject를 찾을 수 없습니다!");
                }
            }

            // 초기 위치 설정 (부드러운 시작)
            if (target != null)
            {
                transform.position = target.position + offset;
                if (showDebugLogs)
                    Debug.Log($"[CameraFollow] 초기 위치 설정: {transform.position}");
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // 타겟 위치 계산
            Vector3 targetPosition = target.position + offset;

            // 선택적 축 추적
            Vector3 currentPosition = transform.position;
            if (!followX) targetPosition.x = currentPosition.x;
            if (!followY) targetPosition.y = currentPosition.y;

            // 부드러운 이동 (SmoothDamp)
            Vector3 smoothedPosition;
            if (smoothTime > 0f)
            {
                smoothedPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothTime);
            }
            else
            {
                // smoothTime이 0이면 즉시 이동
                smoothedPosition = targetPosition;
            }

            // 경계 제한 적용
            if (useBounds)
            {
                smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minBounds.x, maxBounds.x);
                smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minBounds.y, maxBounds.y);
            }

            // Z축은 항상 오프셋 값 유지 (2D 게임)
            smoothedPosition.z = offset.z;

            // 최종 위치 적용
            transform.position = smoothedPosition;
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 추적 타겟 설정
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;

            if (showDebugLogs)
                Debug.Log($"[CameraFollow] 타겟 변경: {(newTarget != null ? newTarget.name : "null")}");

            // 타겟 변경 시 velocity 초기화 (급격한 이동 방지)
            velocity = Vector3.zero;
        }

        /// <summary>
        /// 카메라 경계 설정
        /// </summary>
        public void SetBounds(Vector2 min, Vector2 max)
        {
            useBounds = true;
            minBounds = min;
            maxBounds = max;

            if (showDebugLogs)
                Debug.Log($"[CameraFollow] 경계 설정: Min({min.x}, {min.y}), Max({max.x}, {max.y})");
        }

        /// <summary>
        /// 카메라 경계 비활성화
        /// </summary>
        public void DisableBounds()
        {
            useBounds = false;

            if (showDebugLogs)
                Debug.Log("[CameraFollow] 경계 비활성화");
        }

        /// <summary>
        /// 카메라를 즉시 타겟 위치로 이동 (부드러운 이동 없이)
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null)
            {
                Debug.LogWarning("[CameraFollow] 타겟이 없어 즉시 이동할 수 없습니다!");
                return;
            }

            Vector3 targetPosition = target.position + offset;

            // 선택적 축 추적
            Vector3 currentPosition = transform.position;
            if (!followX) targetPosition.x = currentPosition.x;
            if (!followY) targetPosition.y = currentPosition.y;

            // 경계 제한 적용
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }

            // Z축 고정
            targetPosition.z = offset.z;

            // 즉시 이동
            transform.position = targetPosition;
            velocity = Vector3.zero;

            if (showDebugLogs)
                Debug.Log($"[CameraFollow] 타겟으로 즉시 이동: {targetPosition}");
        }


        // ====== Gizmos ======

        private void OnDrawGizmos()
        {
            if (!showBoundsGizmos || !useBounds) return;

            // 카메라 경계 표시
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (minBounds.x + maxBounds.x) * 0.5f,
                (minBounds.y + maxBounds.y) * 0.5f,
                offset.z
            );
            Vector3 size = new Vector3(
                maxBounds.x - minBounds.x,
                maxBounds.y - minBounds.y,
                0f
            );
            Gizmos.DrawWireCube(center, size);
        }

        private void OnDrawGizmosSelected()
        {
            if (!useBounds) return;

            // 선택 시 경계를 더 명확하게 표시
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Vector3 center = new Vector3(
                (minBounds.x + maxBounds.x) * 0.5f,
                (minBounds.y + maxBounds.y) * 0.5f,
                offset.z
            );
            Vector3 size = new Vector3(
                maxBounds.x - minBounds.x,
                maxBounds.y - minBounds.y,
                0f
            );
            Gizmos.DrawCube(center, size);

#if UNITY_EDITOR
            // 경계 정보 표시
            string info = $"Camera Bounds\n" +
                         $"Min: ({minBounds.x}, {minBounds.y})\n" +
                         $"Max: ({maxBounds.x}, {maxBounds.y})";
            UnityEditor.Handles.Label(new Vector3(center.x, maxBounds.y + 2f, offset.z), info);
#endif
        }


        // ====== 디버그 ======

        [ContextMenu("Print Camera Info")]
        private void PrintCameraInfo()
        {
            Debug.Log($"=== CameraFollow ===\n" +
                     $"Target: {(target != null ? target.name : "null")}\n" +
                     $"Position: {transform.position}\n" +
                     $"Offset: {offset}\n" +
                     $"Smooth Time: {smoothTime}\n" +
                     $"Follow X: {followX}, Follow Y: {followY}\n" +
                     $"Use Bounds: {useBounds}\n" +
                     (useBounds ? $"Bounds: Min({minBounds.x}, {minBounds.y}), Max({maxBounds.x}, {maxBounds.y})\n" : "") +
                     $"===================");
        }

        [ContextMenu("Snap To Target Now")]
        private void SnapToTargetNow()
        {
            if (Application.isPlaying)
            {
                SnapToTarget();
            }
            else
            {
                Debug.LogWarning("[CameraFollow] Play 모드에서만 실행 가능합니다.");
            }
        }
    }
}
