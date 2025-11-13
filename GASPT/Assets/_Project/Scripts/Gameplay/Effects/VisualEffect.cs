using UnityEngine;
using GASPT.Core.Pooling;

namespace GASPT.Gameplay.Effects
{
    /// <summary>
    /// 범용 시각 효과 컴포넌트
    /// 크기 변화 및 페이드아웃 애니메이션 지원
    /// 오브젝트 풀링 가능
    /// </summary>
    [RequireComponent(typeof(PooledObject))]
    public class VisualEffect : MonoBehaviour, IPoolable
    {
        [Header("애니메이션 설정")]
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float startScale = 0.5f;
        [SerializeField] private float endScale = 2f;
        [SerializeField] private float startAlpha = 0.7f;
        [SerializeField] private float endAlpha = 0f;

        [Header("컴포넌트")]
        [SerializeField] private Renderer effectRenderer;

        private float elapsedTime;
        private bool isPlaying;
        private PooledObject pooledObject;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            pooledObject = GetComponent<PooledObject>();

            if (effectRenderer == null)
                effectRenderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            if (!isPlaying) return;

            UpdateEffect();
        }


        // ====== IPoolable 구현 ======

        public void OnSpawn()
        {
            elapsedTime = 0f;
            isPlaying = true;
        }

        public void OnDespawn()
        {
            isPlaying = false;
        }


        // ====== 효과 재생 ======

        /// <summary>
        /// 효과 시작 (설정 커스터마이즈)
        /// </summary>
        public void Play(float duration, float startScale, float endScale, Color startColor, Color endColor)
        {
            this.duration = duration;
            this.startScale = startScale;
            this.endScale = endScale;
            this.startAlpha = startColor.a;
            this.endAlpha = endColor.a;

            transform.localScale = Vector3.one * startScale;

            if (effectRenderer != null && effectRenderer.material != null)
            {
                effectRenderer.material.color = startColor;
            }

            elapsedTime = 0f;
            isPlaying = true;
        }

        /// <summary>
        /// 효과 업데이트 (크기 및 투명도 변화)
        /// </summary>
        private void UpdateEffect()
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);

            // 크기 변화
            float scale = Mathf.Lerp(startScale, endScale, progress);
            transform.localScale = Vector3.one * scale;

            // 투명도 변화
            if (effectRenderer != null && effectRenderer.material != null)
            {
                Color color = effectRenderer.material.color;
                color.a = Mathf.Lerp(startAlpha, endAlpha, progress);
                effectRenderer.material.color = color;
            }

            // 애니메이션 완료 시 풀로 반환
            if (progress >= 1f)
            {
                ReturnToPool();
            }
        }

        /// <summary>
        /// 풀로 반환
        /// </summary>
        private void ReturnToPool()
        {
            isPlaying = false;

            // PoolManager를 통해 풀로 반환
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Despawn(this);
            }
            else
            {
                Debug.LogWarning("[VisualEffect] PoolManager가 없어 GameObject를 파괴합니다.");
                Destroy(gameObject);
            }
        }
    }
}
