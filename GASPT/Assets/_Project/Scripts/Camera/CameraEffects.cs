using GASPT.Core.Extensions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// 카메라 Post-Processing 효과 관리
    /// CameraManager와 함께 작동하여 시각 효과를 제어
    /// </summary>
    public class CameraEffects : MonoBehaviour
    {
        // ====== Volume 참조 ======

        [Header("Post-Processing Volume")]
        [Tooltip("Global Volume (null이면 자동 탐색)")]
        [SerializeField] private Volume globalVolume;

        [Tooltip("Volume이 없으면 자동 생성")]
        [SerializeField] private bool createVolumeIfMissing = true;


        // ====== 기본 효과 설정 ======

        [Header("기본 Bloom 설정")]
        [SerializeField] private float defaultBloomIntensity = 1f;
        [SerializeField] private float defaultBloomThreshold = 0.9f;

        [Header("기본 Vignette 설정")]
        [SerializeField] private float defaultVignetteIntensity = 0.3f;
        [SerializeField] private Color defaultVignetteColor = Color.black;

        [Header("기본 ChromaticAberration 설정")]
        [SerializeField] private float defaultChromaticIntensity = 0f;


        // ====== 효과 전환 설정 ======

        [Header("효과 전환")]
        [Tooltip("효과 전환 속도")]
        [SerializeField] private float transitionSpeed = 5f;


        // ====== 디버그 ======

        [Header("디버그")]
        [SerializeField] private bool showDebugLogs = false;


        // ====== 상태 ======

        private VolumeProfile profile;

        // 효과 오버라이드 참조
        private Bloom bloom;
        private Vignette vignette;
        private ChromaticAberration chromaticAberration;
        private ColorAdjustments colorAdjustments;
        private DepthOfField depthOfField;

        // 타겟 값들
        private float targetBloomIntensity;
        private float targetVignetteIntensity;
        private float targetChromaticIntensity;
        private float targetSaturation;

        // 현재 값들
        private float currentBloomIntensity;
        private float currentVignetteIntensity;
        private float currentChromaticIntensity;
        private float currentSaturation;


        // ====== 프로퍼티 ======

        /// <summary>
        /// Global Volume 참조
        /// </summary>
        public Volume GlobalVolume => globalVolume;

        /// <summary>
        /// 효과 초기화 완료 여부
        /// </summary>
        public bool IsInitialized => profile != null;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            InitializeVolume();
            InitializeEffects();
            SetDefaultValues();
        }

        private void Update()
        {
            // 부드러운 효과 전환
            UpdateEffectTransitions();
        }


        // ====== 초기화 ======

        /// <summary>
        /// Volume 초기화
        /// </summary>
        private void InitializeVolume()
        {
            // Volume 자동 탐색
            if (globalVolume == null)
            {
                globalVolume = FindAnyObjectByType<Volume>();
            }

            // Volume 생성
            if (globalVolume == null && createVolumeIfMissing)
            {
                GameObject volumeObj = new GameObject("Global Volume");
                volumeObj.transform.SetParent(transform);
                globalVolume = volumeObj.AddComponent<Volume>();
                globalVolume.isGlobal = true;

                // 새 프로파일 생성
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                globalVolume.profile = profile;

                if (showDebugLogs)
                    Debug.Log("[CameraEffects] Global Volume 자동 생성");
            }
            else if (globalVolume != null)
            {
                profile = globalVolume.profile;

                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<VolumeProfile>();
                    globalVolume.profile = profile;
                }

                if (showDebugLogs)
                    Debug.Log($"[CameraEffects] Volume 연결: {globalVolume.name}");
            }
        }

        /// <summary>
        /// 효과 컴포넌트 초기화
        /// </summary>
        private void InitializeEffects()
        {
            if (profile == null) return;

            // Bloom
            if (!profile.TryGet(out bloom))
            {
                bloom = profile.Add<Bloom>();
            }
            bloom.active = true;

            // Vignette
            if (!profile.TryGet(out vignette))
            {
                vignette = profile.Add<Vignette>();
            }
            vignette.active = true;

            // Chromatic Aberration
            if (!profile.TryGet(out chromaticAberration))
            {
                chromaticAberration = profile.Add<ChromaticAberration>();
            }
            chromaticAberration.active = true;

            // Color Adjustments
            if (!profile.TryGet(out colorAdjustments))
            {
                colorAdjustments = profile.Add<ColorAdjustments>();
            }
            colorAdjustments.active = true;

            // Depth of Field
            if (!profile.TryGet(out depthOfField))
            {
                depthOfField = profile.Add<DepthOfField>();
            }
            depthOfField.active = false; // 기본적으로 비활성

            if (showDebugLogs)
                Debug.Log("[CameraEffects] 효과 컴포넌트 초기화 완료");
        }

        /// <summary>
        /// 기본값 설정
        /// </summary>
        private void SetDefaultValues()
        {
            targetBloomIntensity = defaultBloomIntensity;
            targetVignetteIntensity = defaultVignetteIntensity;
            targetChromaticIntensity = defaultChromaticIntensity;
            targetSaturation = 0f; // 기본 채도

            currentBloomIntensity = targetBloomIntensity;
            currentVignetteIntensity = targetVignetteIntensity;
            currentChromaticIntensity = targetChromaticIntensity;
            currentSaturation = targetSaturation;

            ApplyEffectValues();
        }


        // ====== 효과 전환 ======

        /// <summary>
        /// 효과 전환 업데이트
        /// </summary>
        private void UpdateEffectTransitions()
        {
            if (profile == null) return;

            bool changed = false;

            // Bloom 전환
            if (!Mathf.Approximately(currentBloomIntensity, targetBloomIntensity))
            {
                currentBloomIntensity = Mathf.Lerp(currentBloomIntensity, targetBloomIntensity, Time.deltaTime * transitionSpeed);
                changed = true;
            }

            // Vignette 전환
            if (!Mathf.Approximately(currentVignetteIntensity, targetVignetteIntensity))
            {
                currentVignetteIntensity = Mathf.Lerp(currentVignetteIntensity, targetVignetteIntensity, Time.deltaTime * transitionSpeed);
                changed = true;
            }

            // Chromatic Aberration 전환
            if (!Mathf.Approximately(currentChromaticIntensity, targetChromaticIntensity))
            {
                currentChromaticIntensity = Mathf.Lerp(currentChromaticIntensity, targetChromaticIntensity, Time.deltaTime * transitionSpeed);
                changed = true;
            }

            // Saturation 전환
            if (!Mathf.Approximately(currentSaturation, targetSaturation))
            {
                currentSaturation = Mathf.Lerp(currentSaturation, targetSaturation, Time.deltaTime * transitionSpeed);
                changed = true;
            }

            if (changed)
            {
                ApplyEffectValues();
            }
        }

        /// <summary>
        /// 현재 효과 값 적용
        /// </summary>
        private void ApplyEffectValues()
        {
            if (bloom != null)
            {
                bloom.intensity.Override(currentBloomIntensity);
                bloom.threshold.Override(defaultBloomThreshold);
            }

            if (vignette != null)
            {
                vignette.intensity.Override(currentVignetteIntensity);
                vignette.color.Override(defaultVignetteColor);
            }

            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.Override(currentChromaticIntensity);
            }

            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.Override(currentSaturation);
            }
        }


        // ====== 공개 API - Bloom ======

        /// <summary>
        /// Bloom 강도 설정
        /// </summary>
        public void SetBloom(float intensity, bool instant = false)
        {
            targetBloomIntensity = Mathf.Max(0f, intensity);

            if (instant)
            {
                currentBloomIntensity = targetBloomIntensity;
                ApplyEffectValues();
            }
        }

        /// <summary>
        /// Bloom 비활성화
        /// </summary>
        public void DisableBloom()
        {
            SetBloom(0f);
        }

        /// <summary>
        /// Bloom 기본값으로 복귀
        /// </summary>
        public void ResetBloom()
        {
            SetBloom(defaultBloomIntensity);
        }


        // ====== 공개 API - Vignette ======

        /// <summary>
        /// Vignette 강도 설정
        /// </summary>
        public void SetVignette(float intensity, bool instant = false)
        {
            targetVignetteIntensity = Mathf.Clamp01(intensity);

            if (instant)
            {
                currentVignetteIntensity = targetVignetteIntensity;
                ApplyEffectValues();
            }
        }

        /// <summary>
        /// Vignette 색상 설정
        /// </summary>
        public void SetVignetteColor(Color color)
        {
            defaultVignetteColor = color;
            if (vignette != null)
            {
                vignette.color.Override(color);
            }
        }

        /// <summary>
        /// Vignette 비활성화
        /// </summary>
        public void DisableVignette()
        {
            SetVignette(0f);
        }

        /// <summary>
        /// Vignette 기본값으로 복귀
        /// </summary>
        public void ResetVignette()
        {
            SetVignette(defaultVignetteIntensity);
            SetVignetteColor(Color.black);
        }


        // ====== 공개 API - Chromatic Aberration ======

        /// <summary>
        /// Chromatic Aberration 강도 설정
        /// </summary>
        public void SetChromaticAberration(float intensity, bool instant = false)
        {
            targetChromaticIntensity = Mathf.Clamp01(intensity);

            if (instant)
            {
                currentChromaticIntensity = targetChromaticIntensity;
                ApplyEffectValues();
            }
        }

        /// <summary>
        /// Chromatic Aberration 비활성화
        /// </summary>
        public void DisableChromaticAberration()
        {
            SetChromaticAberration(0f);
        }


        // ====== 공개 API - Saturation ======

        /// <summary>
        /// 채도 설정 (-100 ~ 100)
        /// </summary>
        public void SetSaturation(float saturation, bool instant = false)
        {
            targetSaturation = Mathf.Clamp(saturation, -100f, 100f);

            if (instant)
            {
                currentSaturation = targetSaturation;
                ApplyEffectValues();
            }
        }

        /// <summary>
        /// 흑백 효과 (채도 -100)
        /// </summary>
        public void SetGrayscale(bool instant = false)
        {
            SetSaturation(-100f, instant);
        }

        /// <summary>
        /// 채도 기본값으로 복귀
        /// </summary>
        public void ResetSaturation()
        {
            SetSaturation(0f);
        }


        // ====== 공개 API - 프리셋 효과 ======

        /// <summary>
        /// 피격 효과 (빨간 Vignette + Chromatic Aberration)
        /// </summary>
        public void PlayHitEffect(float intensity = 0.5f, float duration = 0.3f)
        {
            // 빨간 Vignette
            SetVignetteColor(new Color(0.5f, 0f, 0f));
            SetVignette(intensity, true);

            // Chromatic Aberration
            SetChromaticAberration(intensity * 0.5f, true);

            // 복귀 예약
            ResetEffectsDelayedAsync(duration).Forget();
        }

        /// <summary>
        /// 치유 효과 (초록 Vignette)
        /// </summary>
        public void PlayHealEffect(float intensity = 0.3f, float duration = 0.5f)
        {
            SetVignetteColor(new Color(0f, 0.3f, 0f));
            SetVignette(intensity, true);

            ResetEffectsDelayedAsync(duration).Forget();
        }

        /// <summary>
        /// 스킬 사용 효과 (Bloom 증가)
        /// </summary>
        public void PlaySkillEffect(float intensity = 2f, float duration = 0.3f)
        {
            SetBloom(intensity, true);

            ResetBloomDelayedAsync(duration).Forget();
        }

        /// <summary>
        /// 사망 효과 (흑백 + 강한 Vignette)
        /// </summary>
        public void PlayDeathEffect()
        {
            SetGrayscale(false);
            SetVignette(0.6f, false);
        }

        /// <summary>
        /// 모든 효과 기본값으로 복귀
        /// </summary>
        public void ResetAllEffects(bool instant = false)
        {
            SetBloom(defaultBloomIntensity, instant);
            SetVignette(defaultVignetteIntensity, instant);
            SetVignetteColor(Color.black);
            SetChromaticAberration(defaultChromaticIntensity, instant);
            SetSaturation(0f, instant);

            if (showDebugLogs)
                Debug.Log("[CameraEffects] 모든 효과 기본값으로 복귀");
        }


        // ====== 비동기 헬퍼 ======

        private async Awaitable ResetEffectsDelayedAsync(float delay)
        {
            await Awaitable.WaitForSecondsAsync(delay);
            ResetVignette();
            DisableChromaticAberration();
        }

        private async Awaitable ResetBloomDelayedAsync(float delay)
        {
            await Awaitable.WaitForSecondsAsync(delay);
            ResetBloom();
        }


        // ====== 디버그 ======

        [ContextMenu("Print Effects Info")]
        private void PrintEffectsInfo()
        {
            Debug.Log($"[CameraEffects] ========== 효과 정보 ==========\n" +
                     $"Volume: {(globalVolume != null ? globalVolume.name : "null")}\n" +
                     $"Profile: {(profile != null ? profile.name : "null")}\n" +
                     $"Bloom: {currentBloomIntensity:F2} (target: {targetBloomIntensity:F2})\n" +
                     $"Vignette: {currentVignetteIntensity:F2} (target: {targetVignetteIntensity:F2})\n" +
                     $"Chromatic: {currentChromaticIntensity:F2} (target: {targetChromaticIntensity:F2})\n" +
                     $"Saturation: {currentSaturation:F0} (target: {targetSaturation:F0})\n" +
                     $"=========================================");
        }

        [ContextMenu("Test Hit Effect")]
        private void TestHitEffect()
        {
            if (Application.isPlaying)
                PlayHitEffect();
        }

        [ContextMenu("Test Heal Effect")]
        private void TestHealEffect()
        {
            if (Application.isPlaying)
                PlayHealEffect();
        }

        [ContextMenu("Test Death Effect")]
        private void TestDeathEffect()
        {
            if (Application.isPlaying)
                PlayDeathEffect();
        }

        [ContextMenu("Reset All Effects")]
        private void ResetAllNow()
        {
            if (Application.isPlaying)
                ResetAllEffects(true);
        }
    }
}
