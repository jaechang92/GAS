using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Enemies;

namespace GASPT.Gameplay.Projectiles
{
    /// <summary>
    /// 투사체 베이스 클래스
    /// FireBall, MagicMissile 등이 상속
    /// 풀링 지원
    /// </summary>
    [RequireComponent(typeof(PooledObject))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        // ====== 설정 ======

        [Header("투사체 설정")]
        [SerializeField] protected float speed = 10f;
        [SerializeField] protected float maxDistance = 20f;
        [SerializeField] protected float damage = 10f;

        [Header("시각 효과")]
        [SerializeField] protected Renderer projectileRenderer;
        [SerializeField] protected TrailRenderer trailRenderer;

        [Header("충돌")]
        [SerializeField] protected float collisionRadius = 0.3f;
        [SerializeField] protected LayerMask targetLayers;


        // ====== 상태 ======

        protected Vector3 startPosition;
        protected Vector2 direction;
        protected float travelDistance;
        protected bool isActive;

        protected PooledObject pooledObject;


        // ====== Unity 생명주기 ======

        protected virtual void Awake()
        {
            pooledObject = GetComponent<PooledObject>();

            if (projectileRenderer == null)
                projectileRenderer = GetComponent<Renderer>();

            if (trailRenderer == null)
                trailRenderer = GetComponent<TrailRenderer>();
        }

        protected virtual void Update()
        {
            if (!isActive) return;

            UpdateProjectile();
        }


        // ====== IPoolable 구현 ======

        public virtual void OnSpawn()
        {
            startPosition = transform.position;
            travelDistance = 0f;
            isActive = true;

            // Trail 초기화
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
            }
        }

        public virtual void OnDespawn()
        {
            isActive = false;
        }


        // ====== 발사 ======

        /// <summary>
        /// 투사체 발사
        /// </summary>
        public virtual void Launch(Vector2 direction)
        {
            this.direction = direction.normalized;
            startPosition = transform.position;
            travelDistance = 0f;
            isActive = true;
        }


        // ====== 업데이트 ======

        /// <summary>
        /// 투사체 이동 및 충돌 체크
        /// </summary>
        protected virtual void UpdateProjectile()
        {
            // 이동
            float moveDistance = speed * Time.deltaTime;
            transform.position += (Vector3)direction * moveDistance;
            travelDistance += moveDistance;

            // 최대 거리 체크
            if (travelDistance >= maxDistance)
            {
                OnReachMaxDistance();
                return;
            }

            // 충돌 체크
            if (CheckCollision(out Collider2D hitCollider))
            {
                OnHit(hitCollider);
            }
        }


        // ====== 충돌 ======

        /// <summary>
        /// 충돌 체크
        /// </summary>
        protected virtual bool CheckCollision(out Collider2D hitCollider)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collisionRadius, targetLayers);

            foreach (var hit in hits)
            {
                // 자기 자신은 무시
                if (hit.gameObject == gameObject)
                    continue;

                hitCollider = hit;
                return true;
            }

            hitCollider = null;
            return false;
        }

        /// <summary>
        /// 충돌 시 호출
        /// </summary>
        protected virtual void OnHit(Collider2D hitCollider)
        {
            // Enemy에 데미지 적용
            var enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage((int)damage);
                Debug.Log($"[Projectile] {enemy.Data.enemyName}에 {damage} 데미지!");
            }

            // 풀로 반환
            ReturnToPool();
        }

        /// <summary>
        /// 최대 거리 도달 시 호출
        /// </summary>
        protected virtual void OnReachMaxDistance()
        {
            ReturnToPool();
        }


        // ====== 풀 반환 ======

        /// <summary>
        /// 풀로 반환
        /// </summary>
        protected virtual void ReturnToPool()
        {
            isActive = false;

            // PoolManager를 통해 풀로 반환
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Despawn(this);
            }
            else
            {
                Debug.LogWarning($"[Projectile] PoolManager가 없어 GameObject를 파괴합니다: {name}");
                Destroy(gameObject);
            }
        }


        // ====== Gizmos ======

        protected virtual void OnDrawGizmos()
        {
            if (!isActive) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, collisionRadius);
        }
    }
}
