using System;
using UnityEngine;

namespace GASPT.Audio
{
    /// <summary>
    /// 중앙 오디오 관리 싱글톤
    /// SFX/BGM 재생 및 볼륨 제어
    /// </summary>
    public class AudioManager : SingletonManager<AudioManager>
    {
        // ====== 볼륨 설정 ======

        [Header("볼륨 설정")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float bgmVolume = 0.7f;

        [SerializeField] private bool isMuted = false;


        // ====== 풀 설정 ======

        [Header("SFX 풀 설정")]
        [SerializeField] private int sfxPoolSize = 10;
        [SerializeField] private bool sfxPoolCanGrow = true;


        // ====== 이벤트 ======

        /// <summary>마스터 볼륨 변경 이벤트</summary>
        public event Action<float> OnMasterVolumeChanged;

        /// <summary>SFX 볼륨 변경 이벤트</summary>
        public event Action<float> OnSFXVolumeChanged;

        /// <summary>BGM 볼륨 변경 이벤트</summary>
        public event Action<float> OnBGMVolumeChanged;

        /// <summary>음소거 상태 변경 이벤트</summary>
        public event Action<bool> OnMuteChanged;


        // ====== 컴포넌트 ======

        private AudioSource bgmSource;
        private SFXPool sfxPool;


        // ====== 프로퍼티 ======

        public float MasterVolume => masterVolume;
        public float SFXVolume => sfxVolume;
        public float BGMVolume => bgmVolume;
        public bool IsMuted => isMuted;

        /// <summary>실제 SFX 볼륨 (마스터 × SFX × 음소거)</summary>
        public float EffectiveSFXVolume => isMuted ? 0f : masterVolume * sfxVolume;

        /// <summary>실제 BGM 볼륨 (마스터 × BGM × 음소거)</summary>
        public float EffectiveBGMVolume => isMuted ? 0f : masterVolume * bgmVolume;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            InitializeBGMSource();
            InitializeSFXPool();

            Debug.Log("[AudioManager] 초기화 완료");
        }


        // ====== 초기화 ======

        private void InitializeBGMSource()
        {
            // BGM용 AudioSource 생성
            GameObject bgmObject = new GameObject("BGM Source");
            bgmObject.transform.SetParent(transform);

            bgmSource = bgmObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f; // 2D 사운드
            bgmSource.volume = EffectiveBGMVolume;
        }

        private void InitializeSFXPool()
        {
            // SFXPool 컴포넌트 생성
            GameObject poolObject = new GameObject("SFX Pool");
            poolObject.transform.SetParent(transform);

            sfxPool = poolObject.AddComponent<SFXPool>();
            sfxPool.Initialize(sfxPoolSize, sfxPoolCanGrow);
        }


        // ====== SFX 재생 ======

        /// <summary>
        /// SFX 재생
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="volumeScale">볼륨 스케일 (0~1)</param>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;

            float finalVolume = EffectiveSFXVolume * volumeScale;
            sfxPool?.PlayClip(clip, finalVolume);
        }

        /// <summary>
        /// 특정 위치에서 SFX 재생 (3D 사운드)
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="position">재생 위치</param>
        /// <param name="volumeScale">볼륨 스케일 (0~1)</param>
        /// <param name="spatialBlend">공간 블렌드 (0=2D, 1=3D)</param>
        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f, float spatialBlend = 0.5f)
        {
            if (clip == null) return;

            float finalVolume = EffectiveSFXVolume * volumeScale;
            sfxPool?.PlayClipAtPosition(clip, position, finalVolume, spatialBlend);
        }

        /// <summary>
        /// 랜덤 피치로 SFX 재생 (변형 효과)
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="pitchMin">최소 피치</param>
        /// <param name="pitchMax">최대 피치</param>
        /// <param name="volumeScale">볼륨 스케일</param>
        public void PlaySFXWithRandomPitch(AudioClip clip, float pitchMin = 0.9f, float pitchMax = 1.1f, float volumeScale = 1f)
        {
            if (clip == null) return;

            float finalVolume = EffectiveSFXVolume * volumeScale;
            float randomPitch = UnityEngine.Random.Range(pitchMin, pitchMax);
            sfxPool?.PlayClipWithPitch(clip, finalVolume, randomPitch);
        }


        // ====== BGM 재생 ======

        /// <summary>
        /// BGM 재생
        /// </summary>
        /// <param name="clip">재생할 BGM 클립</param>
        /// <param name="loop">반복 여부</param>
        public void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;

            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = EffectiveBGMVolume;
            bgmSource.Play();

            Debug.Log($"[AudioManager] BGM 재생: {clip.name}");
        }

        /// <summary>
        /// BGM 정지
        /// </summary>
        public void StopBGM()
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }

        /// <summary>
        /// BGM 일시정지
        /// </summary>
        public void PauseBGM()
        {
            bgmSource.Pause();
        }

        /// <summary>
        /// BGM 재개
        /// </summary>
        public void ResumeBGM()
        {
            bgmSource.UnPause();
        }

        /// <summary>
        /// BGM 페이드 (볼륨 전환)
        /// </summary>
        /// <param name="targetVolume">목표 볼륨 (0~1)</param>
        /// <param name="duration">전환 시간 (초)</param>
        public async Awaitable FadeBGM(float targetVolume, float duration)
        {
            float startVolume = bgmSource.volume;
            float targetActualVolume = targetVolume * masterVolume * bgmVolume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                bgmSource.volume = Mathf.Lerp(startVolume, targetActualVolume, t);
                await Awaitable.NextFrameAsync();
            }

            bgmSource.volume = targetActualVolume;

            // 볼륨이 0이면 정지
            if (targetActualVolume <= 0f)
            {
                StopBGM();
            }
        }

        /// <summary>
        /// BGM 교체 (크로스페이드)
        /// </summary>
        /// <param name="newClip">새 BGM 클립</param>
        /// <param name="fadeDuration">페이드 시간</param>
        public async Awaitable CrossFadeBGM(AudioClip newClip, float fadeDuration = 1f)
        {
            // 현재 BGM 페이드 아웃
            await FadeBGM(0f, fadeDuration * 0.5f);

            // 새 BGM 재생
            PlayBGM(newClip);

            // 페이드 인
            bgmSource.volume = 0f;
            await FadeBGM(1f, fadeDuration * 0.5f);
        }


        // ====== 볼륨 제어 ======

        /// <summary>
        /// 마스터 볼륨 설정
        /// </summary>
        /// <param name="volume">볼륨 (0~1)</param>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateBGMVolume();
            OnMasterVolumeChanged?.Invoke(masterVolume);

            Debug.Log($"[AudioManager] 마스터 볼륨: {masterVolume:F2}");
        }

        /// <summary>
        /// SFX 볼륨 설정
        /// </summary>
        /// <param name="volume">볼륨 (0~1)</param>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            OnSFXVolumeChanged?.Invoke(sfxVolume);

            Debug.Log($"[AudioManager] SFX 볼륨: {sfxVolume:F2}");
        }

        /// <summary>
        /// BGM 볼륨 설정
        /// </summary>
        /// <param name="volume">볼륨 (0~1)</param>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            UpdateBGMVolume();
            OnBGMVolumeChanged?.Invoke(bgmVolume);

            Debug.Log($"[AudioManager] BGM 볼륨: {bgmVolume:F2}");
        }

        /// <summary>
        /// 음소거 토글
        /// </summary>
        public void ToggleMute()
        {
            SetMute(!isMuted);
        }

        /// <summary>
        /// 음소거 설정
        /// </summary>
        /// <param name="mute">음소거 여부</param>
        public void SetMute(bool mute)
        {
            isMuted = mute;
            UpdateBGMVolume();
            OnMuteChanged?.Invoke(isMuted);

            Debug.Log($"[AudioManager] 음소거: {isMuted}");
        }

        private void UpdateBGMVolume()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = EffectiveBGMVolume;
            }
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 모든 SFX 정지
        /// </summary>
        public void StopAllSFX()
        {
            sfxPool?.StopAll();
        }

        /// <summary>
        /// 모든 오디오 정지
        /// </summary>
        public void StopAll()
        {
            StopBGM();
            StopAllSFX();
        }


        // ====== 저장/로드 ======

        /// <summary>
        /// 볼륨 설정 저장
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("Audio_MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("Audio_SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("Audio_BGMVolume", bgmVolume);
            PlayerPrefs.SetInt("Audio_Muted", isMuted ? 1 : 0);
            PlayerPrefs.Save();

            Debug.Log("[AudioManager] 설정 저장 완료");
        }

        /// <summary>
        /// 볼륨 설정 로드
        /// </summary>
        public void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("Audio_SFXVolume", 1f);
            bgmVolume = PlayerPrefs.GetFloat("Audio_BGMVolume", 0.7f);
            isMuted = PlayerPrefs.GetInt("Audio_Muted", 0) == 1;

            UpdateBGMVolume();

            Debug.Log("[AudioManager] 설정 로드 완료");
        }


        // ====== 에디터/디버그 ======

        [ContextMenu("Save Settings")]
        private void DebugSaveSettings()
        {
            SaveSettings();
        }

        [ContextMenu("Load Settings")]
        private void DebugLoadSettings()
        {
            LoadSettings();
        }

        [ContextMenu("Toggle Mute")]
        private void DebugToggleMute()
        {
            ToggleMute();
        }

        [ContextMenu("Print Status")]
        private void DebugPrintStatus()
        {
            Debug.Log($"[AudioManager] 상태:");
            Debug.Log($"  - 마스터 볼륨: {masterVolume:F2}");
            Debug.Log($"  - SFX 볼륨: {sfxVolume:F2} (실제: {EffectiveSFXVolume:F2})");
            Debug.Log($"  - BGM 볼륨: {bgmVolume:F2} (실제: {EffectiveBGMVolume:F2})");
            Debug.Log($"  - 음소거: {isMuted}");
            Debug.Log($"  - BGM 재생중: {(bgmSource != null && bgmSource.isPlaying ? bgmSource.clip?.name : "없음")}");
        }
    }
}
