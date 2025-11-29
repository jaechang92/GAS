using UnityEngine;
using System.Collections.Generic;
using Core;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// 카메라 매니저 (싱글톤)
    /// 카메라 추적, 경계, 효과(Shake, Zoom)를 통합 관리
    /// PersistentManagers Scene에 배치
    /// </summary>
    public class CameraManager : SingletonManager<CameraManager>
    {
        // ====== 카메라 참조 ======

        [Header("카메라 참조")]
        [Tooltip("메인 카메라 (null이면 자동 탐색)")]
        [SerializeField] private UnityEngine.Camera mainCamera;

        [Tooltip("UI 카메라 (선택사항)")]
        [SerializeField] private UnityEngine.Camera uiCamera;


        // ====== 추적 설정 ======

        [Header("추적 설정")]
        [Tooltip("추적 타겟 (플레이어)")]
        [SerializeField] private Transform followTarget;

        [Tooltip("카메라 오프셋")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);

        [Tooltip("부드러운 이동 시간 (작을수록 빠름)")]
        [SerializeField] private float smoothTime = 0.2f;

        [Tooltip("X축 추적")]
        [SerializeField] private bool followX = true;

        [Tooltip("Y축 추적")]
        [SerializeField] private bool followY = true;


        // ====== 경계 설정 ======

        [Header("경계 설정")]
        [Tooltip("경계 제한 활성화")]
        [SerializeField] private bool useBounds = true;

        [Tooltip("현재 적용 중인 경계")]
        [SerializeField] private CameraBounds currentBounds = CameraBounds.Default;


        // ====== Shake 설정 ======

        [Header("Shake 효과")]
        [Tooltip("Shake 강도 배율")]
        [SerializeField] private float shakeMultiplier = 1f;


        // ====== Zoom 설정 ======

        [Header("Zoom 효과")]
        [Tooltip("기본 Orthographic Size")]
        [SerializeField] private float defaultOrthographicSize = 5f;

        [Tooltip("Zoom 부드러움")]
        [SerializeField] private float zoomSmoothTime = 0.3f;

        [Tooltip("최소 Zoom (확대)")]
        [SerializeField] private float minOrthographicSize = 2f;

        [Tooltip("최대 Zoom (축소)")]
        [SerializeField] private float maxOrthographicSize = 15f;


        // ====== 디버그 ======

        [Header("디버그")]
        [SerializeField] private bool showDebugLogs = false;
        [SerializeField] private bool showBoundsGizmos = true;


        // ====== 상태 ======

        private Vector3 velocity = Vector3.zero;
        private Vector3 shakeOffset = Vector3.zero;
        private float targetOrthographicSize;
        private float currentZoomVelocity;

        // Shake 상태
        private float shakeDuration;
        private float shakeIntensity;
        private float shakeFrequency;
        private float shakeTimer;

        // Bounds Provider 관리
        private readonly List<CameraBoundsProvider> boundsProviders = new List<CameraBoundsProvider>();


        // ====== 프로퍼티 ======

        /// <summary>
        /// 메인 카메라
        /// </summary>
        public UnityEngine.Camera MainCamera => mainCamera;

        /// <summary>
        /// UI 카메라
        /// </summary>
        public UnityEngine.Camera UICamera => uiCamera;

        /// <summary>
        /// 현재 추적 타겟
        /// </summary>
        public Transform FollowTarget => followTarget;

        /// <summary>
        /// 현재 경계
        /// </summary>
        public CameraBounds CurrentBounds => currentBounds;

        /// <summary>
        /// 현재 Zoom 레벨 (Orthographic Size)
        /// </summary>
        public float CurrentZoom => mainCamera != null ? mainCamera.orthographicSize : defaultOrthographicSize;


        // ====== 싱글톤 초기화 ======

        protected override void OnSingletonAwake()
        {
            InitializeCamera();
            targetOrthographicSize = defaultOrthographicSize;
        }

        /// <summary>
        /// 카메라 초기화
        /// </summary>
        private void InitializeCamera()
        {
            // 메인 카메라 자동 탐색
            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Camera.main;

                if (mainCamera == null)
                {
                    // 태그로 찾기
                    GameObject mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
                    if (mainCamObj != null)
                    {
                        mainCamera = mainCamObj.GetComponent<UnityEngine.Camera>();
                    }
                }

                if (mainCamera != null && showDebugLogs)
                {
                    Debug.Log($"[CameraManager] 메인 카메라 자동 탐색: {mainCamera.name}");
                }
            }

            // 카메라가 없으면 경고
            if (mainCamera == null)
            {
                Debug.LogWarning("[CameraManager] 메인 카메라를 찾을 수 없습니다!");
            }
            else
            {
                // 기본 Orthographic Size 저장
                defaultOrthographicSize = mainCamera.orthographicSize;
                targetOrthographicSize = defaultOrthographicSize;
            }
        }


        // ====== Unity 생명주기 ======

        private void Start()
        {
            // 플레이어 타겟 자동 탐색
            if (followTarget == null)
            {
                FindPlayerTargetAsync().Forget();
            }
        }

        private void LateUpdate()
        {
            if (mainCamera == null) return;

            // 1. Follow 처리
            UpdateFollow();

            // 2. Shake 처리
            UpdateShake();

            // 3. Zoom 처리
            UpdateZoom();

            // 4. 최종 위치 적용
            ApplyCameraPosition();
        }


        // ====== Follow 시스템 ======

        /// <summary>
        /// 플레이어 타겟 자동 탐색 (비동기)
        /// </summary>
        private async Awaitable FindPlayerTargetAsync()
        {
            int maxAttempts = 50;
            int attempts = 0;

            while (followTarget == null && attempts < maxAttempts)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    followTarget = playerObj.transform;
                    if (showDebugLogs)
                        Debug.Log($"[CameraManager] 플레이어 타겟 탐색 완료: {followTarget.name}");

                    // 즉시 타겟 위치로 이동
                    SnapToTarget();
                    break;
                }

                await Awaitable.WaitForSecondsAsync(0.1f);
                attempts++;
            }

            if (followTarget == null)
            {
                Debug.LogWarning("[CameraManager] 플레이어를 찾을 수 없습니다! (타임아웃)");
            }
        }

        /// <summary>
        /// Follow 업데이트
        /// </summary>
        private void UpdateFollow()
        {
            if (followTarget == null) return;

            Vector3 targetPosition = followTarget.position + offset;

            // 선택적 축 추적
            Vector3 currentPos = mainCamera.transform.position;
            if (!followX) targetPosition.x = currentPos.x;
            if (!followY) targetPosition.y = currentPos.y;

            // Z축 고정
            targetPosition.z = offset.z;
        }

        /// <summary>
        /// 최종 카메라 위치 적용
        /// </summary>
        private void ApplyCameraPosition()
        {
            if (followTarget == null) return;

            Vector3 targetPosition = followTarget.position + offset;

            // 선택적 축 추적
            Vector3 currentPos = mainCamera.transform.position;
            if (!followX) targetPosition.x = currentPos.x;
            if (!followY) targetPosition.y = currentPos.y;

            // SmoothDamp 이동
            Vector3 smoothedPosition;
            if (smoothTime > 0f)
            {
                smoothedPosition = Vector3.SmoothDamp(currentPos, targetPosition, ref velocity, smoothTime);
            }
            else
            {
                smoothedPosition = targetPosition;
            }

            // 경계 제한 적용
            if (useBounds && currentBounds.IsValid)
            {
                Vector2 clamped = currentBounds.ClampCamera(
                    new Vector2(smoothedPosition.x, smoothedPosition.y),
                    mainCamera.orthographicSize,
                    mainCamera.aspect
                );
                smoothedPosition.x = clamped.x;
                smoothedPosition.y = clamped.y;
            }

            // Z축 고정
            smoothedPosition.z = offset.z;

            // Shake 오프셋 적용
            smoothedPosition += shakeOffset;

            // 최종 위치 적용
            mainCamera.transform.position = smoothedPosition;
        }


        // ====== Shake 시스템 ======

        /// <summary>
        /// Shake 업데이트
        /// </summary>
        private void UpdateShake()
        {
            if (shakeDuration <= 0f)
            {
                shakeOffset = Vector3.zero;
                return;
            }

            shakeTimer += Time.deltaTime;
            shakeDuration -= Time.deltaTime;

            // Perlin Noise 기반 Shake
            float offsetX = (Mathf.PerlinNoise(shakeTimer * shakeFrequency, 0f) - 0.5f) * 2f;
            float offsetY = (Mathf.PerlinNoise(0f, shakeTimer * shakeFrequency) - 0.5f) * 2f;

            // 페이드 아웃
            float fadeOut = shakeDuration / (shakeDuration + 0.1f);

            shakeOffset = new Vector3(offsetX, offsetY, 0f) * shakeIntensity * shakeMultiplier * fadeOut;
        }

        /// <summary>
        /// 카메라 Shake 실행
        /// </summary>
        /// <param name="intensity">강도 (0.1 ~ 1.0 권장)</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="frequency">진동 빈도 (10 ~ 50 권장)</param>
        public void Shake(float intensity = 0.3f, float duration = 0.3f, float frequency = 25f)
        {
            // 더 강한 Shake가 이미 진행 중이면 무시
            if (shakeDuration > 0f && shakeIntensity > intensity)
                return;

            shakeIntensity = intensity;
            shakeDuration = duration;
            shakeFrequency = frequency;
            shakeTimer = Random.Range(0f, 100f); // 랜덤 시작점

            if (showDebugLogs)
                Debug.Log($"[CameraManager] Shake 시작: intensity={intensity}, duration={duration}");
        }

        /// <summary>
        /// Shake 즉시 중지
        /// </summary>
        public void StopShake()
        {
            shakeDuration = 0f;
            shakeOffset = Vector3.zero;
        }


        // ====== Zoom 시스템 ======

        /// <summary>
        /// Zoom 업데이트
        /// </summary>
        private void UpdateZoom()
        {
            if (mainCamera == null) return;

            float currentSize = mainCamera.orthographicSize;

            if (Mathf.Abs(currentSize - targetOrthographicSize) > 0.01f)
            {
                mainCamera.orthographicSize = Mathf.SmoothDamp(
                    currentSize,
                    targetOrthographicSize,
                    ref currentZoomVelocity,
                    zoomSmoothTime
                );
            }
        }

        /// <summary>
        /// Zoom 설정 (절대값)
        /// </summary>
        /// <param name="orthographicSize">Orthographic Size</param>
        /// <param name="instant">즉시 적용 여부</param>
        public void SetZoom(float orthographicSize, bool instant = false)
        {
            targetOrthographicSize = Mathf.Clamp(orthographicSize, minOrthographicSize, maxOrthographicSize);

            if (instant && mainCamera != null)
            {
                mainCamera.orthographicSize = targetOrthographicSize;
                currentZoomVelocity = 0f;
            }

            if (showDebugLogs)
                Debug.Log($"[CameraManager] Zoom 설정: {targetOrthographicSize}");
        }

        /// <summary>
        /// Zoom 배율 설정 (기본 크기 기준)
        /// </summary>
        /// <param name="multiplier">배율 (1.0 = 기본, 0.5 = 확대, 2.0 = 축소)</param>
        /// <param name="instant">즉시 적용</param>
        public void SetZoomMultiplier(float multiplier, bool instant = false)
        {
            SetZoom(defaultOrthographicSize * multiplier, instant);
        }

        /// <summary>
        /// 기본 Zoom으로 복귀
        /// </summary>
        public void ResetZoom(bool instant = false)
        {
            SetZoom(defaultOrthographicSize, instant);
        }


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
