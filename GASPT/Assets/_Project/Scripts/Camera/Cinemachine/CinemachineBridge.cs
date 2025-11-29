using UnityEngine;
using Unity.Cinemachine;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// 기존 CameraManager API와 Cinemachine 연결 브릿지
    /// CameraManager에서 이 컴포넌트를 통해 Cinemachine 제어
    /// 점진적 마이그레이션을 위한 Wrapper
    /// </summary>
    public class CinemachineBridge : MonoBehaviour
    {
        // ====== 싱글톤 (선택적) ======

        private static CinemachineBridge instance;
        public static CinemachineBridge Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<CinemachineBridge>();
                }
                return instance;
            }
        }

        /// <summary>
        /// 인스턴스 존재 여부 확인 (FindAnyObjectByType 호출 방지)
        /// </summary>
        public static bool HasInstance => instance != null;


        // ====== Cinemachine 참조 ======

        [Header("Cinemachine 참조")]
        [Tooltip("플레이어 추적용 Virtual Camera")]
        [SerializeField] private CinemachineCamera playerVirtualCamera;

        [Tooltip("Cinemachine Brain (null이면 자동 탐색)")]
        [SerializeField] private CinemachineBrain brain;

        [Tooltip("Confiner2D (경계 제한)")]
        [SerializeField] private CinemachineConfiner2D confiner;


        // ====== Impulse ======

        [Header("Impulse")]
        [SerializeField] private CinemachineImpulseHelper impulseHelper;


        // ====== 기존 CameraEffects 연동 ======

        [Header("Post-Processing")]
        [SerializeField] private CameraEffects cameraEffects;


        // ====== 설정 ======

        [Header("설정")]
        [SerializeField] private float defaultOrthoSize = 5f;
        [SerializeField] private bool useCinemachineForFollow = true;
        [SerializeField] private bool useCinemachineForBounds = true;


        // ====== 상태 ======

        private bool isInitialized;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Initialize();
        }

        private void Start()
        {
            // 지연 초기화 (다른 컴포넌트 로드 후)
            if (!isInitialized)
            {
                Initialize();
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        private void Initialize()
        {
            // Brain 자동 탐색
            if (brain == null)
            {
                brain = FindAnyObjectByType<CinemachineBrain>();
            }

            // Virtual Camera 자동 탐색
            if (playerVirtualCamera == null)
            {
                playerVirtualCamera = FindAnyObjectByType<CinemachineCamera>();
            }

            // Confiner 자동 탐색
            if (confiner == null && playerVirtualCamera != null)
            {
                confiner = playerVirtualCamera.GetComponent<CinemachineConfiner2D>();
            }

            // Impulse Helper 자동 탐색
            if (impulseHelper == null)
            {
                impulseHelper = FindAnyObjectByType<CinemachineImpulseHelper>();
            }

            // CameraEffects 자동 탐색
            if (cameraEffects == null)
            {
                cameraEffects = FindAnyObjectByType<CameraEffects>();
            }

            // 기본 Ortho Size 저장
            if (playerVirtualCamera != null)
            {
                defaultOrthoSize = playerVirtualCamera.Lens.OrthographicSize;
            }

            isInitialized = true;
            Debug.Log($"[CinemachineBridge] 초기화 완료 - " +
                     $"Brain: {(brain != null ? "O" : "X")}, " +
                     $"VCam: {(playerVirtualCamera != null ? "O" : "X")}, " +
                     $"Confiner: {(confiner != null ? "O" : "X")}");
        }


        // ====== Follow API (기존 CameraManager 호환) ======

        /// <summary>
        /// 추적 타겟 설정
        /// </summary>
        public void SetFollowTarget(Transform target)
        {
            if (playerVirtualCamera != null && useCinemachineForFollow)
            {
                playerVirtualCamera.Follow = target;
                Debug.Log($"[CinemachineBridge] Follow 타겟 설정: {target?.name ?? "null"}");
            }
        }

        /// <summary>
        /// 현재 추적 타겟
        /// </summary>
        public Transform GetFollowTarget()
        {
            return playerVirtualCamera?.Follow;
        }

        /// <summary>
        /// 타겟으로 즉시 이동 (Cut)
        /// </summary>
        public void SnapToTarget()
        {
            if (brain != null && playerVirtualCamera != null)
            {
                // 포지션 캐시 무효화로 즉시 이동
                playerVirtualCamera.PreviousStateIsValid = false;
            }
        }


        // ====== Shake API (Cinemachine Impulse 사용) ======

        /// <summary>
        /// 카메라 Shake (기존 API 호환)
        /// </summary>
        public void Shake(float intensity = 0.3f, float duration = 0.3f, float frequency = 25f)
        {
            // Cinemachine Impulse 사용
            if (impulseHelper != null)
            {
                impulseHelper.Shake(intensity);
            }
            else
            {
                // Fallback: CameraEffects 사용
                cameraEffects?.PlayHitEffect(intensity * 0.5f, duration);
            }
        }

        /// <summary>
        /// 방향성 Shake
        /// </summary>
        public void ShakeDirectional(Vector2 direction, float intensity = 1f)
        {
            impulseHelper?.ShakeDirectional(direction, intensity);
        }

        /// <summary>
        /// Shake 중지
        /// </summary>
        public void StopShake()
        {
            // Cinemachine Impulse는 자동 페이드아웃되므로 특별한 처리 불필요
        }


        // ====== Zoom API ======

        /// <summary>
        /// Zoom 설정 (Ortho Size)
        /// </summary>
        public void SetZoom(float orthographicSize, bool instant = false)
        {
            if (playerVirtualCamera == null) return;

            if (instant)
            {
                var lens = playerVirtualCamera.Lens;
                lens.OrthographicSize = orthographicSize;
                playerVirtualCamera.Lens = lens;
            }
            else
            {
                // 부드러운 전환을 위해 코루틴 사용
                _ = SmoothZoom(orthographicSize, 0.3f);
            }
        }

        /// <summary>
        /// Zoom 배율 설정
        /// </summary>
        public void SetZoomMultiplier(float multiplier, bool instant = false)
        {
            SetZoom(defaultOrthoSize * multiplier, instant);
        }

        /// <summary>
        /// 기본 Zoom으로 복귀
        /// </summary>
        public void ResetZoom(bool instant = false)
        {
            SetZoom(defaultOrthoSize, instant);
        }

        /// <summary>
        /// 부드러운 Zoom 전환
        /// </summary>
        private async Awaitable SmoothZoom(float targetSize, float duration)
        {
            if (playerVirtualCamera == null) return;

            float startSize = playerVirtualCamera.Lens.OrthographicSize;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                var lens = playerVirtualCamera.Lens;
                lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
                playerVirtualCamera.Lens = lens;

                await Awaitable.NextFrameAsync();
            }
        }

        /// <summary>
        /// 현재 Zoom 값
        /// </summary>
        public float CurrentZoom => playerVirtualCamera?.Lens.OrthographicSize ?? defaultOrthoSize;


        // ====== Bounds API (Cinemachine Confiner2D 사용) ======

        /// <summary>
        /// 경계 Collider 설정
        /// </summary>
        public void SetBoundsCollider(Collider2D boundsCollider)
        {
            if (confiner != null && useCinemachineForBounds)
            {
                confiner.BoundingShape2D = boundsCollider;
                confiner.InvalidateBoundingShapeCache();
                Debug.Log($"[CinemachineBridge] Confiner 경계 설정: {boundsCollider?.name ?? "null"}");
            }
        }

        /// <summary>
        /// 경계 비활성화
        /// </summary>
        public void DisableBounds()
        {
            if (confiner != null)
            {
                confiner.BoundingShape2D = null;
            }
        }


        // ====== Post-Processing API (CameraEffects 사용) ======

        /// <summary>
        /// 피격 효과
        /// </summary>
        public void PlayHitEffect(float intensity = 0.5f, float duration = 0.2f)
        {
            cameraEffects?.PlayHitEffect(intensity, duration);
            impulseHelper?.Shake(intensity * 0.5f);
        }

        /// <summary>
        /// 치유 효과
        /// </summary>
        public void PlayHealEffect(float intensity = 0.3f, float duration = 0.5f)
        {
            cameraEffects?.PlayHealEffect(intensity, duration);
        }

        /// <summary>
        /// 사망 효과
        /// </summary>
        public void PlayDeathEffect()
        {
            cameraEffects?.PlayDeathEffect();
            impulseHelper?.ShakeHeavy();
        }

        /// <summary>
        /// 모든 효과 리셋
        /// </summary>
        public void ResetAllEffects(bool instant = false)
        {
            cameraEffects?.ResetAllEffects(instant);
        }


        // ====== Virtual Camera 관리 ======

        /// <summary>
        /// Virtual Camera Priority 설정
        /// </summary>
        public void SetVirtualCameraPriority(CinemachineCamera vcam, int priority)
        {
            if (vcam != null)
            {
                vcam.Priority = priority;
            }
        }

        /// <summary>
        /// Player Virtual Camera 활성화
        /// </summary>
        public void ActivatePlayerCamera()
        {
            if (playerVirtualCamera != null)
            {
                playerVirtualCamera.Priority = 10;
            }
        }


        // ====== 참조 설정 ======

        /// <summary>
        /// Player Virtual Camera 설정
        /// </summary>
        public void SetPlayerVirtualCamera(CinemachineCamera vcam)
        {
            playerVirtualCamera = vcam;

            if (vcam != null)
            {
                confiner = vcam.GetComponent<CinemachineConfiner2D>();
                defaultOrthoSize = vcam.Lens.OrthographicSize;
            }
        }

        /// <summary>
        /// Impulse Helper 설정
        /// </summary>
        public void SetImpulseHelper(CinemachineImpulseHelper helper)
        {
            impulseHelper = helper;
        }

        /// <summary>
        /// Camera Effects 설정
        /// </summary>
        public void SetCameraEffects(CameraEffects effects)
        {
            cameraEffects = effects;
        }


        // ====== 프로퍼티 ======

        /// <summary>
        /// Brain 참조
        /// </summary>
        public CinemachineBrain Brain => brain;

        /// <summary>
        /// Player Virtual Camera 참조
        /// </summary>
        public CinemachineCamera PlayerVirtualCamera => playerVirtualCamera;

        /// <summary>
        /// Confiner 참조
        /// </summary>
        public CinemachineConfiner2D Confiner => confiner;

        /// <summary>
        /// Camera Effects 참조
        /// </summary>
        public CameraEffects Effects => cameraEffects;


        // ====== 에디터 유틸리티 ======

#if UNITY_EDITOR
        [ContextMenu("Refresh References")]
        private void EditorRefreshReferences()
        {
            isInitialized = false;
            Initialize();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Print Status")]
        private void EditorPrintStatus()
        {
            Debug.Log($"[CinemachineBridge] 상태:\n" +
                     $"  Brain: {(brain != null ? brain.name : "null")}\n" +
                     $"  PlayerVCam: {(playerVirtualCamera != null ? playerVirtualCamera.name : "null")}\n" +
                     $"  Confiner: {(confiner != null ? "있음" : "없음")}\n" +
                     $"  ImpulseHelper: {(impulseHelper != null ? "있음" : "없음")}\n" +
                     $"  CameraEffects: {(cameraEffects != null ? "있음" : "없음")}\n" +
                     $"  DefaultOrthoSize: {defaultOrthoSize}\n" +
                     $"  FollowTarget: {(playerVirtualCamera?.Follow != null ? playerVirtualCamera.Follow.name : "null")}");
        }
#endif
    }
}
