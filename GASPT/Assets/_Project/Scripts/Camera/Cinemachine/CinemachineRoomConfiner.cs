using UnityEngine;
using Unity.Cinemachine;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// Room 진입 시 Cinemachine Confiner 경계 변경
    /// 각 Room/Zone에 배치하여 카메라 경계를 동적으로 설정
    /// </summary>
    public class CinemachineRoomConfiner : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("Room 경계")]
        [Tooltip("이 Room의 카메라 경계로 사용할 Collider")]
        [SerializeField] private Collider2D roomBoundsCollider;

        [Header("Confiner 참조")]
        [Tooltip("null이면 자동 탐색")]
        [SerializeField] private CinemachineConfiner2D confiner;

        [Header("전환 설정")]
        [Tooltip("경계 전환 시 추가 Damping")]
        [SerializeField] private float transitionDamping = 0.5f;

        [Tooltip("전환 Damping 지속 시간")]
        [SerializeField] private float transitionDuration = 0.5f;

        [Header("자동 탐색")]
        [Tooltip("활성화 시 자동으로 Confiner 탐색")]
        [SerializeField] private bool autoFindConfiner = true;

        [Header("디버그")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0.5f, 0.3f);


        // ====== 상태 ======

        private float originalDamping;
        private bool isTransitioning;


        // ====== Unity 생명주기 ======

        private void Start()
        {
            // 자동으로 Collider 찾기
            if (roomBoundsCollider == null)
            {
                roomBoundsCollider = GetComponent<Collider2D>();
            }

            if (roomBoundsCollider == null)
            {
                Debug.LogError($"[CinemachineRoomConfiner] {name}: Room Bounds Collider가 없습니다!");
                return;
            }

            // Confiner 자동 탐색
            if (autoFindConfiner && confiner == null)
            {
                FindConfiner();
            }
        }


        // ====== Confiner 탐색 ======

        /// <summary>
        /// 활성화된 Virtual Camera에서 Confiner 찾기
        /// </summary>
        private void FindConfiner()
        {
            // Cinemachine 3.x: CinemachineBrain.GetActiveBrain() 사용
            if (CinemachineBrain.ActiveBrainCount == 0)
            {
                Debug.LogWarning("[CinemachineRoomConfiner] 활성화된 Cinemachine Brain이 없습니다!");
                return;
            }

            var brain = CinemachineBrain.GetActiveBrain(0);
            if (brain == null)
            {
                Debug.LogWarning("[CinemachineRoomConfiner] Cinemachine Brain을 찾을 수 없습니다!");
                return;
            }

            var activeCamera = brain.ActiveVirtualCamera as CinemachineCamera;
            if (activeCamera != null)
            {
                confiner = activeCamera.GetComponent<CinemachineConfiner2D>();
                if (confiner != null && showDebugLogs)
                {
                    Debug.Log($"[CinemachineRoomConfiner] Confiner 자동 탐색 완료: {activeCamera.name}");
                }
            }

            if (confiner == null)
            {
                Debug.LogWarning("[CinemachineRoomConfiner] CinemachineConfiner2D를 찾을 수 없습니다!");
            }
        }


        // ====== Trigger ======

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            // Confiner 재탐색 (런타임 중 변경될 수 있음)
            if (confiner == null)
            {
                FindConfiner();
            }

            if (confiner == null) return;

            // 경계 변경
            ApplyRoomBounds();
        }


        // ====== 경계 적용 ======

        /// <summary>
        /// Room 경계를 Confiner에 적용
        /// </summary>
        public void ApplyRoomBounds()
        {
            if (confiner == null || roomBoundsCollider == null) return;

            // 이미 같은 경계면 스킵
            if (confiner.BoundingShape2D == roomBoundsCollider) return;

            if (showDebugLogs)
            {
                Debug.Log($"[CinemachineRoomConfiner] Room 경계 변경: {name}");
            }

            // 부드러운 전환을 위한 Damping 조정
            _ = TransitionWithDamping();
        }

        /// <summary>
        /// Damping을 조절하며 부드럽게 전환
        /// </summary>
        private async Awaitable TransitionWithDamping()
        {
            if (isTransitioning) return;
            isTransitioning = true;

            // 기존 Damping 저장
            originalDamping = confiner.Damping;

            // 전환용 Damping 적용
            confiner.Damping = transitionDamping;

            // 경계 변경
            confiner.BoundingShape2D = roomBoundsCollider;
            confiner.InvalidateBoundingShapeCache();

            // 전환 대기
            await Awaitable.WaitForSecondsAsync(transitionDuration);

            // Damping 복구
            if (confiner != null)
            {
                confiner.Damping = originalDamping;
            }

            isTransitioning = false;
        }

        /// <summary>
        /// 즉시 경계 적용 (전환 효과 없음)
        /// </summary>
        public void ApplyRoomBoundsImmediate()
        {
            if (confiner == null || roomBoundsCollider == null) return;

            confiner.BoundingShape2D = roomBoundsCollider;
            confiner.InvalidateBoundingShapeCache();

            if (showDebugLogs)
            {
                Debug.Log($"[CinemachineRoomConfiner] Room 경계 즉시 적용: {name}");
            }
        }


        // ====== Public API ======

        /// <summary>
        /// Room 경계 Collider 설정
        /// </summary>
        public void SetRoomBounds(Collider2D bounds)
        {
            roomBoundsCollider = bounds;
        }

        /// <summary>
        /// Confiner 참조 설정
        /// </summary>
        public void SetConfiner(CinemachineConfiner2D newConfiner)
        {
            confiner = newConfiner;
        }


        // ====== Gizmos ======

        private void OnDrawGizmos()
        {
            if (roomBoundsCollider == null) return;

            Gizmos.color = gizmoColor;

            // Collider 타입에 따라 다르게 그리기
            if (roomBoundsCollider is BoxCollider2D box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawCube(box.offset, box.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (roomBoundsCollider is PolygonCollider2D polygon)
            {
                DrawPolygonGizmo(polygon);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (roomBoundsCollider == null) return;

            // 선택 시 더 진한 색으로 표시
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.8f);

            if (roomBoundsCollider is BoxCollider2D box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawWireCube(box.offset, box.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (roomBoundsCollider is PolygonCollider2D polygon)
            {
                DrawPolygonGizmo(polygon, true);
            }

#if UNITY_EDITOR
            // 라벨 표시
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f,
                $"Room Confiner: {name}"
            );
#endif
        }

        private void DrawPolygonGizmo(PolygonCollider2D polygon, bool wireOnly = false)
        {
            Vector2[] points = polygon.points;
            if (points.Length < 3) return;

            Vector3 position = transform.position;

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 start = position + (Vector3)points[i];
                Vector3 end = position + (Vector3)points[(i + 1) % points.Length];
                Gizmos.DrawLine(start, end);
            }

            if (!wireOnly)
            {
                // 중심점 표시
                Vector2 center = Vector2.zero;
                foreach (var point in points)
                {
                    center += point;
                }
                center /= points.Length;

                Gizmos.DrawSphere(position + (Vector3)center, 0.3f);
            }
        }


        // ====== 에디터 유틸리티 ======

#if UNITY_EDITOR
        [ContextMenu("Apply Bounds Now")]
        private void EditorApplyBounds()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[CinemachineRoomConfiner] 플레이 모드에서만 작동합니다.");
                return;
            }
            ApplyRoomBoundsImmediate();
        }

        [ContextMenu("Create Box Collider Bounds")]
        private void EditorCreateBoxBounds()
        {
            if (roomBoundsCollider == null)
            {
                var box = gameObject.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                box.size = new Vector2(20f, 12f);
                roomBoundsCollider = box;
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log("[CinemachineRoomConfiner] BoxCollider2D 생성됨");
            }
        }

        [ContextMenu("Create Polygon Collider Bounds")]
        private void EditorCreatePolygonBounds()
        {
            if (roomBoundsCollider == null)
            {
                var polygon = gameObject.AddComponent<PolygonCollider2D>();
                polygon.isTrigger = true;
                // 기본 사각형
                polygon.points = new Vector2[]
                {
                    new Vector2(-10f, -6f),
                    new Vector2(10f, -6f),
                    new Vector2(10f, 6f),
                    new Vector2(-10f, 6f)
                };
                roomBoundsCollider = polygon;
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log("[CinemachineRoomConfiner] PolygonCollider2D 생성됨");
            }
        }

        [ContextMenu("Find Confiner In Scene")]
        private void EditorFindConfiner()
        {
            var confiners = FindObjectsByType<CinemachineConfiner2D>(FindObjectsSortMode.None);
            if (confiners.Length > 0)
            {
                confiner = confiners[0];
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"[CinemachineRoomConfiner] Confiner 발견: {confiner.name}");
            }
            else
            {
                Debug.LogWarning("[CinemachineRoomConfiner] Scene에서 Confiner를 찾을 수 없습니다.");
            }
        }
#endif
    }
}
