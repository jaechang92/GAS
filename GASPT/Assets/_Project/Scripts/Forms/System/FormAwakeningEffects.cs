using System;
using UnityEngine;

namespace GASPT.Forms
{
    /// <summary>
    /// 폼 각성 이펙트 및 사운드 재생 시스템
    /// FormManager와 연동하여 각성 시 피드백 제공
    /// </summary>
    public class FormAwakeningEffects : MonoBehaviour
    {
        // ====== 이벤트 ======

        /// <summary>각성 이펙트 재생 시작</summary>
        public event Action<int> OnAwakeningEffectPlayed;

        /// <summary>최대 각성 이펙트 재생</summary>
        public event Action OnMaxAwakeningEffectPlayed;


        // ====== 설정 ======

        [Header("참조")]
        [SerializeField] private FormManager formManager;
        [SerializeField] private Transform effectSpawnPoint;
        [SerializeField] private AudioSource audioSource;

        [Header("기본 이펙트 (폼별 없을 시 사용)")]
        [SerializeField] private GameObject defaultAwakeningEffect;
        [SerializeField] private GameObject defaultMaxAwakeningEffect;
        [SerializeField] private AudioClip defaultAwakeningSound;
        [SerializeField] private AudioClip defaultMaxAwakeningSound;

        [Header("설정")]
        [SerializeField] private float effectDuration = 2f;
        [SerializeField] private bool autoFindReferences = true;

        [Header("화면 효과")]
        [SerializeField] private bool enableScreenFlash = true;
        [SerializeField] private float screenFlashDuration = 0.3f;
        [SerializeField] private Color normalAwakeningFlashColor = new Color(1f, 1f, 0.5f, 0.3f);
        [SerializeField] private Color maxAwakeningFlashColor = new Color(1f, 0.8f, 0.2f, 0.5f);

        [Header("디버그")]
        [SerializeField] private bool logDebugInfo = true;


        // ====== 상태 ======

        private FormInstance subscribedPrimaryForm;
        private FormInstance subscribedSecondaryForm;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (autoFindReferences)
            {
                AutoFindReferences();
            }
        }

        private void Start()
        {
            SubscribeToFormManager();
        }

        private void OnDestroy()
        {
            UnsubscribeFromForms();
            UnsubscribeFromFormManager();
        }


        // ====== 초기화 ======

        private void AutoFindReferences()
        {
            if (formManager == null)
            {
                formManager = GetComponentInParent<FormManager>();
                if (formManager == null)
                {
                    formManager = FindAnyObjectByType<FormManager>();
                }
            }

            if (effectSpawnPoint == null)
            {
                effectSpawnPoint = transform;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                }
            }
        }


        // ====== 이벤트 구독 ======

        private void SubscribeToFormManager()
        {
            if (formManager == null) return;

            formManager.OnFormAcquired += HandleFormAcquired;
            formManager.OnFormSwapped += HandleFormSwapped;

            // 초기 폼에 대한 구독
            SubscribeToForm(formManager.CurrentForm);
            SubscribeToForm(formManager.ReserveForm);

            Log("FormManager 이벤트 구독 완료");
        }

        private void UnsubscribeFromFormManager()
        {
            if (formManager == null) return;

            formManager.OnFormAcquired -= HandleFormAcquired;
            formManager.OnFormSwapped -= HandleFormSwapped;
        }

        private void SubscribeToForm(FormInstance form)
        {
            if (form == null) return;

            form.OnAwakened += HandleFormAwakened;
            form.OnMaxAwakeningReached += HandleMaxAwakeningReached;

            Log($"폼 이벤트 구독: {form.FormName}");
        }

        private void UnsubscribeFromForm(FormInstance form)
        {
            if (form == null) return;

            form.OnAwakened -= HandleFormAwakened;
            form.OnMaxAwakeningReached -= HandleMaxAwakeningReached;
        }

        private void UnsubscribeFromForms()
        {
            UnsubscribeFromForm(subscribedPrimaryForm);
            UnsubscribeFromForm(subscribedSecondaryForm);
        }


        // ====== 이벤트 핸들러 ======

        private void HandleFormAcquired(FormInstance form)
        {
            // 새 폼에 구독
            SubscribeToForm(form);
        }

        private void HandleFormSwapped(FormInstance previous, FormInstance current)
        {
            // 필요시 구독 업데이트
            if (formManager != null)
            {
                UnsubscribeFromForms();
                SubscribeToForm(formManager.CurrentForm);
                SubscribeToForm(formManager.ReserveForm);
            }
        }

        private void HandleFormAwakened(int level, FormRarity rarity)
        {
            Log($"각성 감지! 레벨: {level}, 등급: {rarity}");

            // 각성한 폼 찾기
            FormInstance awakenedForm = null;
            if (formManager?.CurrentForm?.AwakeningLevel == level)
            {
                awakenedForm = formManager.CurrentForm;
            }
            else if (formManager?.ReserveForm?.AwakeningLevel == level)
            {
                awakenedForm = formManager.ReserveForm;
            }

            PlayAwakeningEffect(awakenedForm, level);
            PlayAwakeningSound(awakenedForm, level);

            if (enableScreenFlash)
            {
                TriggerScreenFlash(normalAwakeningFlashColor);
            }

            OnAwakeningEffectPlayed?.Invoke(level);
        }

        private void HandleMaxAwakeningReached()
        {
            Log("최대 각성 도달!");

            // 최대 각성 폼 찾기
            FormInstance maxForm = null;
            if (formManager?.CurrentForm?.IsMaxAwakening == true)
            {
                maxForm = formManager.CurrentForm;
            }
            else if (formManager?.ReserveForm?.IsMaxAwakening == true)
            {
                maxForm = formManager.ReserveForm;
            }

            PlayMaxAwakeningEffect(maxForm);
            PlayMaxAwakeningSound(maxForm);

            if (enableScreenFlash)
            {
                TriggerScreenFlash(maxAwakeningFlashColor);
            }

            OnMaxAwakeningEffectPlayed?.Invoke();
        }


        // ====== 이펙트 재생 ======

        private void PlayAwakeningEffect(FormInstance form, int level)
        {
            GameObject effectPrefab = form?.Data?.awakeningEffectPrefab ?? defaultAwakeningEffect;

            if (effectPrefab == null)
            {
                Log("각성 이펙트 프리팹 없음");
                return;
            }

            Vector3 spawnPos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
            var effect = Instantiate(effectPrefab, spawnPos, Quaternion.identity);

            // 폼 색상 적용
            if (form != null)
            {
                ApplyColorToEffect(effect, form.FormColor);
            }

            Destroy(effect, effectDuration);

            Log($"각성 이펙트 재생: {effectPrefab.name}");
        }

        private void PlayMaxAwakeningEffect(FormInstance form)
        {
            GameObject effectPrefab = form?.Data?.maxAwakeningEffectPrefab
                ?? defaultMaxAwakeningEffect
                ?? form?.Data?.awakeningEffectPrefab
                ?? defaultAwakeningEffect;

            if (effectPrefab == null)
            {
                Log("최대 각성 이펙트 프리팹 없음");
                return;
            }

            Vector3 spawnPos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
            var effect = Instantiate(effectPrefab, spawnPos, Quaternion.identity);

            // 폼 색상 적용 (더 밝게)
            if (form != null)
            {
                Color brightColor = form.FormColor * 1.5f;
                brightColor.a = 1f;
                ApplyColorToEffect(effect, brightColor);
            }

            // 스케일 확대
            effect.transform.localScale *= 1.5f;

            Destroy(effect, effectDuration * 1.5f);

            Log($"최대 각성 이펙트 재생: {effectPrefab.name}");
        }

        private void ApplyColorToEffect(GameObject effect, Color color)
        {
            // ParticleSystem에 색상 적용
            var particles = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                var main = ps.main;
                main.startColor = color;
            }

            // SpriteRenderer에 색상 적용
            var sprites = effect.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in sprites)
            {
                sr.color = color;
            }
        }


        // ====== 사운드 재생 ======

        private void PlayAwakeningSound(FormInstance form, int level)
        {
            AudioClip clip = form?.Data?.awakeningSound ?? defaultAwakeningSound;

            if (clip == null || audioSource == null)
            {
                Log("각성 사운드 없음");
                return;
            }

            // 레벨에 따라 피치 조절 (높은 레벨일수록 높은 피치)
            audioSource.pitch = 1f + (level * 0.1f);
            audioSource.PlayOneShot(clip);

            Log($"각성 사운드 재생: {clip.name}");
        }

        private void PlayMaxAwakeningSound(FormInstance form)
        {
            AudioClip clip = form?.Data?.maxAwakeningSound
                ?? defaultMaxAwakeningSound
                ?? form?.Data?.awakeningSound
                ?? defaultAwakeningSound;

            if (clip == null || audioSource == null)
            {
                Log("최대 각성 사운드 없음");
                return;
            }

            audioSource.pitch = 1f;
            audioSource.PlayOneShot(clip);

            Log($"최대 각성 사운드 재생: {clip.name}");
        }


        // ====== 화면 효과 ======

        private void TriggerScreenFlash(Color flashColor)
        {
            // 화면 플래시 효과 (간단한 구현 - 별도 UI 필요)
            // TODO: ScreenFlashUI 컴포넌트와 연동
            Log($"화면 플래시: {flashColor}");
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 수동으로 각성 이펙트 재생 (테스트용)
        /// </summary>
        public void TestAwakeningEffect(int level = 1)
        {
            PlayAwakeningEffect(formManager?.CurrentForm, level);
            PlayAwakeningSound(formManager?.CurrentForm, level);
        }

        /// <summary>
        /// 수동으로 최대 각성 이펙트 재생 (테스트용)
        /// </summary>
        public void TestMaxAwakeningEffect()
        {
            PlayMaxAwakeningEffect(formManager?.CurrentForm);
            PlayMaxAwakeningSound(formManager?.CurrentForm);
        }


        // ====== 유틸리티 ======

        private void Log(string message)
        {
            if (logDebugInfo)
            {
                Debug.Log($"[FormAwakeningEffects] {message}");
            }
        }


        // ====== 에디터 ======

        [ContextMenu("Test Awakening Effect")]
        private void DebugTestAwakening()
        {
            TestAwakeningEffect(1);
        }

        [ContextMenu("Test Max Awakening Effect")]
        private void DebugTestMaxAwakening()
        {
            TestMaxAwakeningEffect();
        }
    }
}
