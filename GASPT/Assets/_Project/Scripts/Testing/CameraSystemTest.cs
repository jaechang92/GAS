using UnityEngine;
using GASPT.CameraSystem;

namespace GASPT.Testing
{
    /// <summary>
    /// 카메라 시스템 테스트 스크립트
    /// 인스펙터에서 버튼으로 각 기능 테스트 가능
    /// </summary>
    public class CameraSystemTest : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool enableKeyboardShortcuts = true;

        [Header("Shake 테스트")]
        [SerializeField] private float shakeIntensity = 0.3f;
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float shakeFrequency = 25f;

        [Header("Zoom 테스트")]
        [SerializeField] private float zoomMultiplier = 0.5f;

        [Header("참조 (자동 탐색)")]
        [SerializeField] private CameraManager cameraManager;
        [SerializeField] private CameraEffects cameraEffects;

        private void Start()
        {
            // 자동 탐색
            if (cameraManager == null)
                cameraManager = CameraManager.Instance;

            if (cameraEffects == null)
                cameraEffects = FindAnyObjectByType<CameraEffects>();

            Debug.Log("[CameraSystemTest] 테스트 스크립트 시작");
            Debug.Log($"  - CameraManager: {(cameraManager != null ? "O" : "X")}");
            Debug.Log($"  - CameraEffects: {(cameraEffects != null ? "O" : "X")}");
            Debug.Log("  - 키보드 단축키:");
            Debug.Log("    [1] 약한 Shake");
            Debug.Log("    [2] 중간 Shake");
            Debug.Log("    [3] 강한 Shake");
            Debug.Log("    [4] Zoom In (확대)");
            Debug.Log("    [5] Zoom Out (축소)");
            Debug.Log("    [6] Zoom Reset");
            Debug.Log("    [7] 피격 효과");
            Debug.Log("    [8] 치유 효과");
            Debug.Log("    [9] 사망 효과");
            Debug.Log("    [0] 효과 리셋");
        }

        private void Update()
        {
            if (!enableKeyboardShortcuts) return;

            // Shake 테스트
            if (Input.GetKeyDown(KeyCode.Alpha1))
                TestShakeLight();
            if (Input.GetKeyDown(KeyCode.Alpha2))
                TestShakeMedium();
            if (Input.GetKeyDown(KeyCode.Alpha3))
                TestShakeHeavy();

            // Zoom 테스트
            if (Input.GetKeyDown(KeyCode.Alpha4))
                TestZoomIn();
            if (Input.GetKeyDown(KeyCode.Alpha5))
                TestZoomOut();
            if (Input.GetKeyDown(KeyCode.Alpha6))
                TestZoomReset();

            // 효과 테스트
            if (Input.GetKeyDown(KeyCode.Alpha7))
                TestHitEffect();
            if (Input.GetKeyDown(KeyCode.Alpha8))
                TestHealEffect();
            if (Input.GetKeyDown(KeyCode.Alpha9))
                TestDeathEffect();
            if (Input.GetKeyDown(KeyCode.Alpha0))
                TestResetAllEffects();
        }

        // ====== Shake 테스트 ======

        [ContextMenu("Test Shake - Light")]
        public void TestShakeLight()
        {
            if (cameraManager == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraManager가 없습니다!");
                return;
            }

            cameraManager.Shake(0.1f, 0.1f, 20f);
            Debug.Log("[CameraSystemTest] 약한 Shake 실행");
        }

        [ContextMenu("Test Shake - Medium")]
        public void TestShakeMedium()
        {
            if (cameraManager == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraManager가 없습니다!");
                return;
            }

            cameraManager.Shake(0.3f, 0.2f, 25f);
            Debug.Log("[CameraSystemTest] 중간 Shake 실행");
        }

        [ContextMenu("Test Shake - Heavy")]
        public void TestShakeHeavy()
        {
            if (cameraManager == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraManager가 없습니다!");
                return;
            }

            cameraManager.Shake(0.5f, 0.3f, 30f);
            Debug.Log("[CameraSystemTest] 강한 Shake 실행");
        }

        [ContextMenu("Test Shake - Custom")]
        public void TestShakeCustom()
        {
            if (cameraManager == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraManager가 없습니다!");
                return;
            }

            cameraManager.Shake(shakeIntensity, shakeDuration, shakeFrequency);
            Debug.Log($"[CameraSystemTest] 커스텀 Shake 실행: intensity={shakeIntensity}, duration={shakeDuration}, frequency={shakeFrequency}");
        }

        [ContextMenu("Stop Shake")]
        public void StopShake()
        {
            if (cameraManager == null) return;
            cameraManager.StopShake();
            Debug.Log("[CameraSystemTest] Shake 중지");
        }

        // ====== Zoom 테스트 ======

        [ContextMenu("Test Zoom In")]
        public void TestZoomIn()
        {
            if (cameraManager == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraManager가 없습니다!");
                return;
            }

            cameraManager.SetZoomMultiplier(0.5f);
            Debug.Log("[CameraSystemTest] Zoom In (0.5x) 실행");
        }

        [ContextMenu("Test Zoom Out")]
        public void TestZoomOut()
        {
            if (cameraManager == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraManager가 없습니다!");
                return;
            }

            cameraManager.SetZoomMultiplier(1.5f);
            Debug.Log("[CameraSystemTest] Zoom Out (1.5x) 실행");
        }

        [ContextMenu("Test Zoom Custom")]
        public void TestZoomCustom()
        {
            if (cameraManager == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraManager가 없습니다!");
                return;
            }

            cameraManager.SetZoomMultiplier(zoomMultiplier);
            Debug.Log($"[CameraSystemTest] 커스텀 Zoom 실행: {zoomMultiplier}x");
        }

        [ContextMenu("Test Zoom Reset")]
        public void TestZoomReset()
        {
            if (cameraManager == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraManager가 없습니다!");
                return;
            }

            cameraManager.ResetZoom();
            Debug.Log("[CameraSystemTest] Zoom 리셋");
        }

        // ====== Post-Processing 효과 테스트 ======

        [ContextMenu("Test Hit Effect")]
        public void TestHitEffect()
        {
            if (cameraEffects == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraEffects가 없습니다!");
                return;
            }

            cameraEffects.PlayHitEffect(0.5f, 0.3f);
            cameraManager?.Shake(0.2f, 0.15f, 25f);
            Debug.Log("[CameraSystemTest] 피격 효과 실행");
        }

        [ContextMenu("Test Heal Effect")]
        public void TestHealEffect()
        {
            if (cameraEffects == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraEffects가 없습니다!");
                return;
            }

            cameraEffects.PlayHealEffect(0.3f, 0.5f);
            Debug.Log("[CameraSystemTest] 치유 효과 실행");
        }

        [ContextMenu("Test Skill Effect")]
        public void TestSkillEffect()
        {
            if (cameraEffects == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraEffects가 없습니다!");
                return;
            }

            cameraEffects.PlaySkillEffect(2f, 0.3f);
            Debug.Log("[CameraSystemTest] 스킬 효과 실행");
        }

        [ContextMenu("Test Death Effect")]
        public void TestDeathEffect()
        {
            if (cameraEffects == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraEffects가 없습니다!");
                return;
            }

            cameraEffects.PlayDeathEffect();
            Debug.Log("[CameraSystemTest] 사망 효과 실행");
        }

        [ContextMenu("Reset All Effects")]
        public void TestResetAllEffects()
        {
            if (cameraEffects == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraEffects가 없습니다!");
                return;
            }

            cameraEffects.ResetAllEffects(true);
            Debug.Log("[CameraSystemTest] 모든 효과 리셋");
        }

        // ====== 통합 테스트 ======

        [ContextMenu("Test Boss Appear Sequence")]
        public async void TestBossAppearSequence()
        {
            if (cameraManager == null || cameraEffects == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraManager 또는 CameraEffects가 없습니다!");
                return;
            }

            Debug.Log("[CameraSystemTest] 보스 등장 시퀀스 시작");

            // 1. Zoom In
            cameraManager.SetZoomMultiplier(0.7f);
            cameraEffects.SetVignette(0.5f, true);

            await Awaitable.WaitForSecondsAsync(1.5f);

            // 2. Shake
            cameraManager.Shake(0.4f, 0.5f, 20f);

            await Awaitable.WaitForSecondsAsync(0.5f);

            // 3. Reset
            cameraManager.ResetZoom();
            cameraEffects.ResetVignette();

            Debug.Log("[CameraSystemTest] 보스 등장 시퀀스 완료");
        }

        [ContextMenu("Test Damage Combo")]
        public async void TestDamageCombo()
        {
            if (cameraManager == null || cameraEffects == null)
            {
                Debug.LogWarning("[CameraSystemTest] CameraManager 또는 CameraEffects가 없습니다!");
                return;
            }

            Debug.Log("[CameraSystemTest] 연속 피격 테스트 시작");

            for (int i = 0; i < 5; i++)
            {
                cameraEffects.PlayHitEffect(0.3f + i * 0.1f, 0.2f);
                cameraManager.Shake(0.1f + i * 0.05f, 0.1f, 25f);
                await Awaitable.WaitForSecondsAsync(0.3f);
            }

            Debug.Log("[CameraSystemTest] 연속 피격 테스트 완료");
        }

        // ====== 상태 출력 ======

        [ContextMenu("Print Camera Status")]
        public void PrintCameraStatus()
        {
            Debug.Log("========== 카메라 시스템 상태 ==========");

            if (cameraManager != null)
            {
                Debug.Log($"[CameraManager]");
                Debug.Log($"  - MainCamera: {(cameraManager.MainCamera != null ? cameraManager.MainCamera.name : "null")}");
                Debug.Log($"  - Target: {(cameraManager.FollowTarget != null ? cameraManager.FollowTarget.name : "null")}");
                Debug.Log($"  - CurrentZoom: {cameraManager.CurrentZoom}");
                Debug.Log($"  - Bounds: {cameraManager.CurrentBounds}");
            }
            else
            {
                Debug.Log("[CameraManager] 없음");
            }

            if (cameraEffects != null)
            {
                Debug.Log($"[CameraEffects]");
                Debug.Log($"  - Volume: {(cameraEffects.GlobalVolume != null ? cameraEffects.GlobalVolume.name : "null")}");
                Debug.Log($"  - Initialized: {cameraEffects.IsInitialized}");
            }
            else
            {
                Debug.Log("[CameraEffects] 없음");
            }

            Debug.Log("=========================================");
        }
    }
}
