using System.Collections.Generic;
using UnityEngine;

namespace GASPT.Audio
{
    /// <summary>
    /// SFX 재생용 AudioSource 풀링 시스템
    /// 동시 다중 SFX 재생 지원
    /// </summary>
    public class SFXPool : MonoBehaviour
    {
        // ====== 설정 ======

        private int poolSize = 10;
        private bool canGrow = true;


        // ====== 풀 데이터 ======

        private Queue<AudioSource> availableSources = new Queue<AudioSource>();
        private List<AudioSource> activeSources = new List<AudioSource>();
        private Transform poolRoot;


        // ====== 초기화 ======

        /// <summary>
        /// 풀 초기화
        /// </summary>
        /// <param name="size">초기 풀 크기</param>
        /// <param name="allowGrow">풀 확장 허용 여부</param>
        public void Initialize(int size, bool allowGrow = true)
        {
            poolSize = size;
            canGrow = allowGrow;

            // 풀 루트 생성
            poolRoot = new GameObject("SFX Sources").transform;
            poolRoot.SetParent(transform);

            // 초기 AudioSource 생성
            for (int i = 0; i < poolSize; i++)
            {
                CreateAudioSource();
            }

            Debug.Log($"[SFXPool] 초기화 완료 - 크기: {poolSize}, 확장 가능: {canGrow}");
        }

        private AudioSource CreateAudioSource()
        {
            GameObject sourceObject = new GameObject($"SFX Source {availableSources.Count + activeSources.Count}");
            sourceObject.transform.SetParent(poolRoot);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 기본 2D
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1f;
            source.maxDistance = 50f;

            sourceObject.SetActive(false);
            availableSources.Enqueue(source);

            return source;
        }


        // ====== 풀에서 가져오기 ======

        private AudioSource GetSource()
        {
            AudioSource source;

            if (availableSources.Count > 0)
            {
                source = availableSources.Dequeue();
            }
            else if (canGrow)
            {
                // 풀 확장
                source = CreateAudioSource();
                availableSources.Dequeue(); // 방금 추가된 것 제거
                Debug.Log($"[SFXPool] 풀 확장 - 현재 총: {availableSources.Count + activeSources.Count + 1}");
            }
            else
            {
                // 가장 오래된 활성 소스 재사용
                if (activeSources.Count > 0)
                {
                    source = activeSources[0];
                    activeSources.RemoveAt(0);
                    source.Stop();
                }
                else
                {
                    Debug.LogWarning("[SFXPool] 사용 가능한 AudioSource가 없습니다.");
                    return null;
                }
            }

            source.gameObject.SetActive(true);
            activeSources.Add(source);
            return source;
        }


        // ====== 풀로 반환 ======

        private void ReturnToPool(AudioSource source)
        {
            if (source == null) return;

            source.Stop();
            source.clip = null;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.transform.position = Vector3.zero;
            source.gameObject.SetActive(false);

            activeSources.Remove(source);
            availableSources.Enqueue(source);
        }

        private async Awaitable ReturnAfterPlayAsync(AudioSource source, float duration)
        {
            await Awaitable.WaitForSecondsAsync(duration);

            if (source != null && activeSources.Contains(source))
            {
                ReturnToPool(source);
            }
        }


        // ====== 재생 메서드 ======

        /// <summary>
        /// 클립 재생 (2D)
        /// </summary>
        /// <param name="clip">재생할 클립</param>
        /// <param name="volume">볼륨</param>
        /// <returns>사용된 AudioSource</returns>
        public AudioSource PlayClip(AudioClip clip, float volume)
        {
            if (clip == null) return null;

            AudioSource source = GetSource();
            if (source == null) return null;

            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = 0f;
            source.pitch = 1f;
            source.Play();

            // 재생 완료 후 자동 반환
            _ = ReturnAfterPlayAsync(source, clip.length + 0.1f);

            return source;
        }

        /// <summary>
        /// 클립 재생 (3D 위치)
        /// </summary>
        /// <param name="clip">재생할 클립</param>
        /// <param name="position">재생 위치</param>
        /// <param name="volume">볼륨</param>
        /// <param name="spatialBlend">공간 블렌드 (0=2D, 1=3D)</param>
        /// <returns>사용된 AudioSource</returns>
        public AudioSource PlayClipAtPosition(AudioClip clip, Vector3 position, float volume, float spatialBlend = 0.5f)
        {
            if (clip == null) return null;

            AudioSource source = GetSource();
            if (source == null) return null;

            source.transform.position = position;
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = spatialBlend;
            source.pitch = 1f;
            source.Play();

            // 재생 완료 후 자동 반환
            _ = ReturnAfterPlayAsync(source, clip.length + 0.1f);

            return source;
        }

        /// <summary>
        /// 클립 재생 (피치 조절)
        /// </summary>
        /// <param name="clip">재생할 클립</param>
        /// <param name="volume">볼륨</param>
        /// <param name="pitch">피치</param>
        /// <returns>사용된 AudioSource</returns>
        public AudioSource PlayClipWithPitch(AudioClip clip, float volume, float pitch)
        {
            if (clip == null) return null;

            AudioSource source = GetSource();
            if (source == null) return null;

            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = 0f;
            source.pitch = pitch;
            source.Play();

            // 피치에 따른 재생 시간 조정
            float adjustedDuration = clip.length / Mathf.Abs(pitch);
            _ = ReturnAfterPlayAsync(source, adjustedDuration + 0.1f);

            return source;
        }


        // ====== 제어 ======

        /// <summary>
        /// 모든 SFX 정지
        /// </summary>
        public void StopAll()
        {
            // 활성 소스를 복사하여 순회 (순회 중 수정 방지)
            List<AudioSource> toReturn = new List<AudioSource>(activeSources);

            foreach (var source in toReturn)
            {
                ReturnToPool(source);
            }

            Debug.Log("[SFXPool] 모든 SFX 정지");
        }


        // ====== 상태 조회 ======

        /// <summary>
        /// 현재 재생 중인 SFX 수
        /// </summary>
        public int ActiveCount => activeSources.Count;

        /// <summary>
        /// 사용 가능한 AudioSource 수
        /// </summary>
        public int AvailableCount => availableSources.Count;

        /// <summary>
        /// 전체 풀 크기
        /// </summary>
        public int TotalCount => activeSources.Count + availableSources.Count;


        // ====== 에디터/디버그 ======

        [ContextMenu("Print Pool Status")]
        private void DebugPrintStatus()
        {
            Debug.Log($"[SFXPool] 상태:");
            Debug.Log($"  - 활성: {ActiveCount}");
            Debug.Log($"  - 대기: {AvailableCount}");
            Debug.Log($"  - 전체: {TotalCount}");
        }

        [ContextMenu("Stop All SFX")]
        private void DebugStopAll()
        {
            StopAll();
        }
    }
}
