using UnityEngine;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// CameraManager - 관리 시스템
    /// Bounds, 타겟, 카메라 참조 관리, Gizmos, 디버그
    /// </summary>
    public partial class CameraManager
    {
        // ====== Bounds 시스템 ======

        /// <summary>
        /// BoundsProvider 등록
        /// </summary>
        public void RegisterBoundsProvider(CameraBoundsProvider provider)
        {
            if (provider == null || boundsProviders.Contains(provider)) return;

            boundsProviders.Add(provider);

            // 우선순위로 정렬
            boundsProviders.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            // 가장 높은 우선순위 경계 적용
            UpdateActiveBounds();

            if (showDebugLogs)
                Debug.Log($"[CameraManager] BoundsProvider 등록: {provider.name} (총 {boundsProviders.Count}개)");
        }

        /// <summary>
        /// BoundsProvider 해제
        /// </summary>
        public void UnregisterBoundsProvider(CameraBoundsProvider provider)
        {
            if (provider == null) return;

            boundsProviders.Remove(provider);
            UpdateActiveBounds();

            if (showDebugLogs)
                Debug.Log($"[CameraManager] BoundsProvider 해제: {provider.name} (남은 {boundsProviders.Count}개)");
        }

        /// <summary>
        /// 활성 경계 업데이트
        /// </summary>
        private void UpdateActiveBounds()
        {
            if (boundsProviders.Count > 0)
            {
                currentBounds = boundsProviders[0].Bounds;
                useBounds = true;
            }
            else
            {
                currentBounds = CameraBounds.Default;
                useBounds = false;
            }
        }

        /// <summary>
        /// 경계 수동 설정
        /// </summary>
        public void SetBounds(CameraBounds bounds)
        {
            currentBounds = bounds;
            useBounds = true;

            if (showDebugLogs)
                Debug.Log($"[CameraManager] 경계 수동 설정: {bounds}");
        }

        /// <summary>
        /// 경계 비활성화
        /// </summary>
        public void DisableBounds()
        {
            useBounds = false;
        }

        /// <summary>
        /// 경계 활성화
        /// </summary>
        public void EnableBounds()
        {
            useBounds = true;
        }


        // ====== 타겟 관리 ======

        /// <summary>
        /// 추적 타겟 설정
        /// </summary>
        public void SetTarget(Transform target)
        {
            followTarget = target;
            velocity = Vector3.zero;

            if (showDebugLogs)
                Debug.Log($"[CameraManager] 타겟 설정: {(target != null ? target.name : "null")}");
        }

        /// <summary>
        /// 타겟 위치로 즉시 이동
        /// </summary>
        public void SnapToTarget()
        {
            if (followTarget == null || mainCamera == null) return;

            Vector3 targetPosition = followTarget.position + offset;

            // 경계 제한 적용
            if (useBounds && currentBounds.IsValid)
            {
                Vector2 clamped = currentBounds.ClampCamera(
                    new Vector2(targetPosition.x, targetPosition.y),
                    mainCamera.orthographicSize,
                    mainCamera.aspect
                );
                targetPosition.x = clamped.x;
                targetPosition.y = clamped.y;
            }

            targetPosition.z = offset.z;
            mainCamera.transform.position = targetPosition;
            velocity = Vector3.zero;

            if (showDebugLogs)
                Debug.Log($"[CameraManager] 타겟으로 즉시 이동: {targetPosition}");
        }

        /// <summary>
        /// 특정 위치로 즉시 이동
        /// </summary>
        public void SnapToPosition(Vector3 position)
        {
            if (mainCamera == null) return;

            position.z = offset.z;
            mainCamera.transform.position = position;
            velocity = Vector3.zero;
        }


        // ====== 카메라 참조 관리 ======

        /// <summary>
        /// 메인 카메라 설정
        /// </summary>
        public void SetMainCamera(UnityEngine.Camera camera)
        {
            mainCamera = camera;

            if (camera != null)
            {
                defaultOrthographicSize = camera.orthographicSize;
                targetOrthographicSize = defaultOrthographicSize;
            }

            if (showDebugLogs)
                Debug.Log($"[CameraManager] 메인 카메라 설정: {(camera != null ? camera.name : "null")}");
        }

        /// <summary>
        /// UI 카메라 설정
        /// </summary>
        public void SetUICamera(UnityEngine.Camera camera)
        {
            uiCamera = camera;

            if (showDebugLogs)
                Debug.Log($"[CameraManager] UI 카메라 설정: {(camera != null ? camera.name : "null")}");
        }

        /// <summary>
        /// 카메라 재탐색
        /// </summary>
        public void RefreshCameraReference()
        {
            InitializeCamera();
        }


        // ====== Gizmos ======

        private void OnDrawGizmos()
        {
            if (!showBoundsGizmos || !useBounds) return;

            DrawBoundsGizmo();
        }

        private void OnDrawGizmosSelected()
        {
            DrawBoundsGizmo();
        }

        private void DrawBoundsGizmo()
        {
            if (!currentBounds.IsValid) return;

            Gizmos.color = Color.green;
            Vector3 center = new Vector3(currentBounds.Center.x, currentBounds.Center.y, 0f);
            Vector3 size = new Vector3(currentBounds.Size.x, currentBounds.Size.y, 0f);
            Gizmos.DrawWireCube(center, size);

            // 카메라 뷰 영역 표시
            if (mainCamera != null)
            {
                Gizmos.color = Color.yellow;
                float camHeight = mainCamera.orthographicSize * 2f;
                float camWidth = camHeight * mainCamera.aspect;
                Gizmos.DrawWireCube(mainCamera.transform.position, new Vector3(camWidth, camHeight, 0f));
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Print Camera Info")]
        private void PrintCameraInfo()
        {
            Debug.Log($"[CameraManager] ========== 카메라 정보 ==========\n" +
                     $"Main Camera: {(mainCamera != null ? mainCamera.name : "null")}\n" +
                     $"UI Camera: {(uiCamera != null ? uiCamera.name : "null")}\n" +
                     $"Target: {(followTarget != null ? followTarget.name : "null")}\n" +
                     $"Position: {(mainCamera != null ? mainCamera.transform.position.ToString() : "N/A")}\n" +
                     $"Orthographic Size: {(mainCamera != null ? mainCamera.orthographicSize.ToString() : "N/A")}\n" +
                     $"Use Bounds: {useBounds}\n" +
                     $"Current Bounds: {currentBounds}\n" +
                     $"Bounds Providers: {boundsProviders.Count}개\n" +
                     $"Shake Active: {shakeDuration > 0f}\n" +
                     $"=========================================");
        }

        [ContextMenu("Snap To Target Now")]
        private void SnapToTargetNow()
        {
            if (Application.isPlaying)
            {
                SnapToTarget();
            }
        }

        [ContextMenu("Test Shake")]
        private void TestShake()
        {
            if (Application.isPlaying)
            {
                Shake(0.5f, 0.5f, 30f);
            }
        }

        [ContextMenu("Test Zoom In")]
        private void TestZoomIn()
        {
            if (Application.isPlaying)
            {
                SetZoomMultiplier(0.5f);
            }
        }

        [ContextMenu("Test Zoom Out")]
        private void TestZoomOut()
        {
            if (Application.isPlaying)
            {
                SetZoomMultiplier(1.5f);
            }
        }

        [ContextMenu("Reset Zoom")]
        private void ResetZoomNow()
        {
            if (Application.isPlaying)
            {
                ResetZoom();
            }
        }
    }
}
