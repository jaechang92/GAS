using UnityEngine;
using Core;

namespace Core.Managers
{
    /// <summary>
    /// 오디오 관리 매니저
    /// 최소 기능: 음악, 효과음 재생 및 볼륨 조절
    /// </summary>
    public class AudioManager : SingletonManager<AudioManager>
    {
        [Header("오디오 소스")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("볼륨 설정")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;

        [Header("디버그")]
        [SerializeField] private bool showDebugLog = false;

        // 프로퍼티
        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp01(value);
                UpdateVolume();
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                UpdateVolume();
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                UpdateVolume();
            }
        }

        public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying;

        protected override void OnSingletonAwake()
        {
            Log("오디오 매니저 초기화");
            SetupAudioSources();
        }

        /// <summary>
        /// 오디오 소스 설정
        /// </summary>
        private void SetupAudioSources()
        {
            // 음악 소스 생성
            if (musicSource == null)
            {
                GameObject musicGO = new GameObject("MusicSource");
                musicGO.transform.SetParent(transform);
                musicSource = musicGO.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            // 효과음 소스 생성
            if (sfxSource == null)
            {
                GameObject sfxGO = new GameObject("SFXSource");
                sfxGO.transform.SetParent(transform);
                sfxSource = sfxGO.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            UpdateVolume();
        }

        /// <summary>
        /// 볼륨 업데이트
        /// </summary>
        private void UpdateVolume()
        {
            if (musicSource != null)
                musicSource.volume = masterVolume * musicVolume;

            if (sfxSource != null)
                sfxSource.volume = masterVolume * sfxVolume;
        }

        /// <summary>
        /// 음악 재생
        /// </summary>
        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null) return;

            musicSource.clip = clip;
            musicSource.Play();
            Log($"음악 재생: {clip.name}");
        }

        /// <summary>
        /// 음악 정지
        /// </summary>
        public void StopMusic()
        {
            if (musicSource == null) return;

            musicSource.Stop();
            Log("음악 정지");
        }

        /// <summary>
        /// 음악 일시정지/재개
        /// </summary>
        public void PauseMusic(bool pause)
        {
            if (musicSource == null) return;

            if (pause)
            {
                musicSource.Pause();
                Log("음악 일시정지");
            }
            else
            {
                musicSource.UnPause();
                Log("음악 재개");
            }
        }

        /// <summary>
        /// 효과음 재생
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;

            sfxSource.PlayOneShot(clip);
            Log($"효과음 재생: {clip.name}");
        }

        private void Log(string message)
        {
            if (showDebugLog) Debug.Log($"[AudioManager] {message}");
        }
    }
}
