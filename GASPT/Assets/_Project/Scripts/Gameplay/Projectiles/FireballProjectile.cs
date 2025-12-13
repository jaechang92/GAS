using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Enemies;
using GASPT.Gameplay.Effects;

namespace GASPT.Gameplay.Projectiles
{
    /// <summary>
    /// 화염구 투사체
    /// 폭발 범위 데미지를 가진 강력한 투사체
    /// </summary>
    public class FireballProjectile : Projectile
    {
        [Header("화염구 전용 설정")]
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private GameObject explosionEffectPrefab;

        private bool hasExploded = false;


        // ====== 초기화 ======

        protected override void Awake()
        {
            base.Awake();

            // 기본값 설정
            speed = 10f;
            maxDistance = 20f;
            damage = 50f;
            collisionRadius = 0.3f;

            SetupVisuals();
        }

        /// <summary>
        /// 시각 효과 설정
        /// </summary>
        private void SetupVisuals()
        {
            // 기본 Renderer 설정 (주황빛 빨강)
            if (projectileRenderer != null)
            {
                projectileRenderer.material.color = new Color(1f, 0.3f, 0f);
            }

            // Trail 설정
            if (trailRenderer != null)
            {
                trailRenderer.time = 0.5f;
                trailRenderer.startWidth = 0.3f;
                trailRenderer.endWidth = 0.05f;
                trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
                trailRenderer.startColor = new Color(1f, 0.5f, 0f, 1f); // 주황색
                trailRenderer.endColor = new Color(1f, 0f, 0f, 0f); // 투명한 빨강
            }
        }


        // ====== IPoolable 오버라이드 ======

        public override void OnSpawn()
        {
            base.OnSpawn();
            hasExploded = false;
        }


        // ====== 충돌 처리 ======

        /// <summary>
        /// 충돌 시 폭발
        /// </summary>
        protected override void OnHit(Collider2D hitCollider)
        {
            if (hasExploded) return;

            Vector3 explosionPos = transform.position;
            Explode(explosionPos);
        }

        /// <summary>
        /// 최대 거리 도달 시에도 폭발
        /// </summary>
        protected override void OnReachMaxDistance()
        {
            if (hasExploded) return;

            Vector3 explosionPos = transform.position;
            Explode(explosionPos);
        }


        // ====== 폭발 ======

        /// <summary>
        /// 폭발 (범위 데미지)
        /// </summary>
        private void Explode(Vector3 explosionPos)
        {
            hasExploded = true;

            Debug.Log($"[FireballProjectile] 폭발! 위치: {explosionPos}, 반경: {explosionRadius}m");

            // 범위 내 모든 적 검색
            Collider2D[] hits = Physics2D.OverlapCircleAll(explosionPos, explosionRadius);

            int enemiesHit = 0;

            foreach (var hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage((int)damage);
                    enemiesHit++;
                    Debug.Log($"[FireballProjectile] {enemy.Data.enemyName}에 {damage} 폭발 데미지!");
                }
            }

            Debug.Log($"[FireballProjectile] 폭발 완료 - {enemiesHit}명의 적에게 데미지 적용");

            // 폭발 시각 효과 재생
            PlayExplosionEffect(explosionPos);

            // 풀로 반환
            ReturnToPool();
        }

        /// <summary>
        /// 폭발 시각 효과 (풀 사용)
        /// </summary>
        private void PlayExplosionEffect(Vector3 explosionPos)
        {
            // 풀에서 VisualEffect 가져오기
            var explosion = PoolManager.Instance.Spawn<VisualEffect>(
                explosionPos,
                Quaternion.identity
            );

            if (explosion != null)
            {
                // 폭발 효과 설정
                Color startColor = new Color(1f, 0.8f, 0f, 0.7f); // 반투명 노란색
                Color endColor = new Color(1f, 0.8f, 0f, 0f);     // 투명

                explosion.Play(
                    duration: 0.5f,
                    startScale: 0.5f,
                    endScale: explosionRadius * 2f,
                    startColor: startColor,
                    endColor: endColor
                );
            }
            else
            {
                Debug.LogWarning("[FireballProjectile] 폭발 효과를 풀에서 가져올 수 없습니다.");
            }
        }


        // ====== Gizmos ======

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (hasExploded)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, explosionRadius);
            }
        }
    }
}
