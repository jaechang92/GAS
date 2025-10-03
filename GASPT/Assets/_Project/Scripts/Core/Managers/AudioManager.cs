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
            Debug.Log("[AudioManager] 오디오 매니저 초기화");
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
            Debug.Log($"[AudioManager] 음악 재생: {clip.name}");
        }

        /// <summary>
        /// 음악 정지
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
                Debug.Log("[AudioManager] 음악 정지");
            }
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
                Debug.Log("[AudioManager] 음악 일시정지");
            }
            else
            {
                musicSource.UnPause();
                Debug.Log("[AudioManager] 음악 재개");
            }
        }

        /// <summary>
        /// 효과음 재생
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;

            sfxSource.PlayOneShot(clip);
            Debug.Log($"[AudioManager] 효과음 재생: {clip.name}");
        }

        // TODO: 차후 구현 예정
        // - 3D 사운드 지원
        // - 사운드 풀링 시스템
        // - 오디오 믹서 그룹 지원
        // - 페이드 인/아웃 효과
        // - 오디오 설정 저장/로드
        // - 다중 음악 레이어 지원
    }
}
