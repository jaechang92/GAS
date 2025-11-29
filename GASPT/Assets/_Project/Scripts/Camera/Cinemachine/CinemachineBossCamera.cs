using UnityEngine;
using Unity.Cinemachine;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// 보스전 전용 카메라 시스템
    /// 보스 등장 연출, 페이즈 전환, 사망 연출 등을 처리
    /// </summary>
    public class CinemachineBossCamera : MonoBehaviour
    {
        // ====== 카메라 참조 ======

        [Header("Virtual Cameras")]
        [Tooltip("플레이어 기본 카메라")]
        [SerializeField] private CinemachineCamera playerCamera;

        [Tooltip("보스 인트로 카메라 (보스 줌인)")]
        [SerializeField] private CinemachineCamera bossIntroCamera;

        [Tooltip("보스전 카메라 (아레나 전체)")]
        [SerializeField] private CinemachineCamera bossFightCamera;

        [Tooltip("보스 사망 카메라")]
        [SerializeField] private CinemachineCamera bossDeathCamera;


        // ====== 타겟 ======

        [Header("타겟")]
        [SerializeField] private Transform bossTransform;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform arenaCenter;


        // ====== Impulse ======

        [Header("Impulse")]
        [SerializeField] private CinemachineImpulseHelper impulseHelper;


        // ====== 설정 ======

        [Header("연출 설정")]
        [Tooltip("보스 인트로 줌인 시간")]
        [SerializeField] private float introZoomDuration = 2f;

        [Tooltip("보스 착지 흔들림 강도")]
        [SerializeField] private float bossLandingShakeIntensity = 2f;

        [Tooltip("페이즈 전환 줌 배율")]
        [SerializeField] private float phaseTransitionZoom = 0.8f;

        [Tooltip("보스 사망 슬로우모션 시간")]
        [SerializeField] private float deathSlowmoDuration = 1.5f;


        // ====== Priority 설정 ======

        [Header("Priority 설정")]
        [SerializeField] private int playerPriority = 10;
        [SerializeField] private int bossIntroPriority = 100;
        [SerializeField] private int bossFightPriority = 50;
        [SerializeField] private int bossDeathPriority = 200;


        // ====== 상태 ======

        private bool isBossFightActive;
        private int currentPhase = 1;


        // ====== Unity 생명주기 ======

        private void Start()
        {
            // 자동 참조 설정
            if (impulseHelper == null)
            {
                impulseHelper = CinemachineImpulseHelper.Instance;
            }

            // 초기 Priority 설정
            ResetCameraPriorities();
        }


        // ====== 보스 등장 시퀀스 ======

        /// <summary>
        /// 보스 등장 연출 시작
        /// </summary>
        public async Awaitable PlayBossIntroSequence()
        {
            if (bossTransform == null)
            {
                Debug.LogWarning("[CinemachineBossCamera] Boss Transform이 설정되지 않았습니다!");
                return;
            }

            Debug.Log("[CinemachineBossCamera] 보스 등장 시퀀스 시작");

            // 1. 보스 인트로 카메라 활성화
            if (bossIntroCamera != null)
            {
                bossIntroCamera.Follow = bossTransform;
                bossIntroCamera.Priority = bossIntroPriority;
            }

            // 2. 보스 등장 대기
            await Awaitable.WaitForSecondsAsync(introZoomDuration);

            // 3. 보스 착지 흔들림
            impulseHelper?.ShakeBoss();

            await Awaitable.WaitForSecondsAsync(0.5f);

            // 4. 보스전 카메라로 전환
            StartBossFight();

            Debug.Log("[CinemachineBossCamera] 보스 등장 시퀀스 완료");
        }

        /// <summary>
        /// 보스전 시작
        /// </summary>
        public void StartBossFight()
        {
            isBossFightActive = true;
            currentPhase = 1;

            // 인트로 카메라 비활성화
            if (bossIntroCamera != null)
            {
                bossIntroCamera.Priority = 0;
            }

            // 보스전 카메라 활성화
            if (bossFightCamera != null)
            {
                // 아레나 중심 또는 플레이어+보스 중간점 추적
                bossFightCamera.Follow = arenaCenter ?? playerTransform;
                bossFightCamera.Priority = bossFightPriority;
            }

            Debug.Log("[CinemachineBossCamera] 보스전 시작");
        }


        // ====== 페이즈 전환 ======

        /// <summary>
        /// 페이즈 전환 연출
        /// </summary>
        public async Awaitable PlayPhaseTransition(int newPhase)
        {
            if (!isBossFightActive) return;

            currentPhase = newPhase;
            Debug.Log($"[CinemachineBossCamera] 페이즈 {newPhase} 전환 시작");

            // 줌 효과
            await ZoomEffect(phaseTransitionZoom, 0.3f);

            // 강한 흔들림
            float shakeIntensity = 1f + (newPhase * 0.5f);
            impulseHelper?.Shake(shakeIntensity);

            // 줌 복귀
            await ZoomEffect(1f, 0.2f);

            Debug.Log($"[CinemachineBossCamera] 페이즈 {newPhase} 전환 완료");
        }


        // ====== 보스 사망 ======

        /// <summary>
        /// 보스 사망 연출
        /// </summary>
        public async Awaitable PlayBossDeathSequence()
        {
            if (!isBossFightActive) return;

            Debug.Log("[CinemachineBossCamera] 보스 사망 시퀀스 시작");

            // 1. 슬로우모션 시작
            Time.timeScale = 0.3f;

            // 2. 사망 카메라 활성화 (보스 줌인)
            if (bossDeathCamera != null)
            {
                bossDeathCamera.Follow = bossTransform;
                bossDeathCamera.Priority = bossDeathPriority;
            }

            // 3. 강한 흔들림
            impulseHelper?.ShakeBoss();

            // 4. 슬로우모션 유지
            await Awaitable.WaitForSecondsAsync(deathSlowmoDuration * 0.3f); // Time.timeScale 보정

            // 5. 슬로우모션 해제
            Time.timeScale = 1f;

            // 6. 추가 대기
            await Awaitable.WaitForSecondsAsync(1f);

            // 7. 정리
            EndBossFight();

            Debug.Log("[CinemachineBossCamera] 보스 사망 시퀀스 완료");
        }

        /// <summary>
        /// 보스전 종료
        /// </summary>
        public void EndBossFight()
        {
            isBossFightActive = false;
            currentPhase = 1;

            // 모든 보스 카메라 비활성화
            ResetCameraPriorities();

            Debug.Log("[CinemachineBossCamera] 보스전 종료");
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 카메라 Priority 리셋
        /// </summary>
        private void ResetCameraPriorities()
        {
            if (playerCamera != null) playerCamera.Priority = playerPriority;
            if (bossIntroCamera != null) bossIntroCamera.Priority = 0;
            if (bossFightCamera != null) bossFightCamera.Priority = 0;
            if (bossDeathCamera != null) bossDeathCamera.Priority = 0;
        }

        /// <summary>
        /// 줌 효과 (현재 활성 카메라)
        /// </summary>
        private async Awaitable ZoomEffect(float targetZoom, float duration)
        {
            var activeCamera = GetActiveCamera();
            if (activeCamera == null) return;

            float startSize = activeCamera.Lens.OrthographicSize;
            float targetSize = 5f * targetZoom; // 기본 크기 5 기준
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                var lens = activeCamera.Lens;
                lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
                activeCamera.Lens = lens;

                await Awaitable.NextFrameAsync();
            }
        }

        /// <summary>
        /// 현재 활성화된 카메라 반환
        /// </summary>
        private CinemachineCamera GetActiveCamera()
        {
            if (bossDeathCamera != null && bossDeathCamera.Priority > 0) return bossDeathCamera;
            if (bossIntroCamera != null && bossIntroCamera.Priority > 0) return bossIntroCamera;
            if (bossFightCamera != null && bossFightCamera.Priority > 0) return bossFightCamera;
            return playerCamera;
        }


        // ====== Public API ======

        /// <summary>
        /// 보스 Transform 설정
        /// </summary>
        public void SetBossTransform(Transform boss)
        {
            bossTransform = boss;
        }

        /// <summary>
        /// 플레이어 Transform 설정
        /// </summary>
        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }

        /// <summary>
        /// 아레나 중심 설정
        /// </summary>
        public void SetArenaCenter(Transform center)
        {
            arenaCenter = center;
        }

        /// <summary>
        /// 보스전 활성 여부
        /// </summary>
        public bool IsBossFightActive => isBossFightActive;

        /// <summary>
        /// 현재 페이즈
        /// </summary>
        public int CurrentPhase => currentPhase;


        // ====== 에디터 유틸리티 ======

#if UNITY_EDITOR
        [ContextMenu("Test Boss Intro")]
        private async void TestBossIntro()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("플레이 모드에서만 테스트 가능합니다.");
                return;
            }
            await PlayBossIntroSequence();
        }

        [ContextMenu("Test Phase 2 Transition")]
        private async void TestPhase2()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("플레이 모드에서만 테스트 가능합니다.");
                return;
            }
            await PlayPhaseTransition(2);
        }

        [ContextMenu("Test Boss Death")]
        private async void TestBossDeath()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("플레이 모드에서만 테스트 가능합니다.");
                return;
            }
            await PlayBossDeathSequence();
        }

        [ContextMenu("Reset to Player Camera")]
        private void TestResetCamera()
        {
            EndBossFight();
        }
#endif
    }
}
