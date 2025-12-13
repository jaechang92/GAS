using GASPT.Gameplay.Enemies;
using UnityEngine;

namespace GASPT.Core.Pooling
{
    /// <summary>
    /// 풀링 가능한 GameObject에 붙이는 컴포넌트
    /// 자동 반환 기능 제공
    /// </summary>
    public class PooledObject : MonoBehaviour, IPoolable
    {
        // ====== 설정 ======

        [Header("자동 반환 설정")]
        [Tooltip("자동 반환 사용 여부")]
        [SerializeField] private bool autoReturn = false;

        [Tooltip("자동 반환 시간 (초)")]
        [SerializeField] private float autoReturnTime = 3f;


        // ====== 상태 ======

        private float spawnTime;


        // ====== IPoolable 구현 ======

        public void OnSpawn()
        {
            spawnTime = Time.time;

            if (autoReturn)
            {
                CancelInvoke(nameof(ReturnToPool));
                Invoke(nameof(ReturnToPool), autoReturnTime);
            }
        }

        public void OnDespawn()
        {
            CancelInvoke(nameof(ReturnToPool));
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 풀로 반환 (수동 호출)
        /// </summary>
        public void ReturnToPool()
        {
            if (PoolManager.Instance == null)
            {
                Debug.LogWarning($"[PooledObject] PoolManager가 없어 GameObject를 파괴합니다: {name}");
                Destroy(gameObject);
                return;
            }

            // 타입별로 PoolManager.Despawn 호출
            // Projectile
            var projectile = GetComponent<GASPT.Gameplay.Projectiles.Projectile>();
            if (projectile != null)
            {
                PoolManager.Instance.Despawn(projectile);
                return;
            }

            // Enemy
            var basicMelee = GetComponent<BasicMeleeEnemy>();
            if (basicMelee != null)
            {
                PoolManager.Instance.Despawn(basicMelee);
                return;
            }

            // VisualEffect
            var visualEffect = GetComponent<GASPT.Gameplay.Effects.VisualEffect>();
            if (visualEffect != null)
            {
                PoolManager.Instance.Despawn(visualEffect);
                return;
            }

            // 알 수 없는 타입
            Debug.LogWarning($"[PooledObject] {name}의 풀링 가능한 컴포넌트를 찾을 수 없습니다. GameObject를 파괴합니다.");
            Destroy(gameObject);
        }

        /// <summary>
        /// 지연 후 풀로 반환
        /// </summary>
        public void ReturnToPoolDelayed(float delay)
        {
            CancelInvoke(nameof(ReturnToPool));
            Invoke(nameof(ReturnToPool), delay);
        }


        // ====== Unity 이벤트 ======

        private void OnDisable()
        {
            CancelInvoke(nameof(ReturnToPool));
        }
    }
}
