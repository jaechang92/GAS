using UnityEngine;
using System.Collections.Generic;
using GASPT.Core.Extensions;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// 카메라 매니저 (싱글톤)
    /// 카메라 추적, 경계, 효과(Shake, Zoom)를 통합 관리
    /// PersistentManagers Scene에 배치
    ///
    /// partial class 구조:
    /// - CameraManager.cs: 필드, 프로퍼티, 초기화, 생명주기
    /// - CameraManager.Effects.cs: Follow, Shake, Zoom 시스템
    /// - CameraManager.Management.cs: Bounds, 타겟, 카메라 참조, Gizmos, 디버그
    /// </summary>
    public partial class CameraManager : SingletonManager<CameraManager>
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
    }
}
