using UnityEngine;
using GASPT.Core.Pooling;

namespace GASPT.Gameplay.Projectiles
{
    /// <summary>
    /// 적 투사체
    /// 플레이어를 타겟팅하는 투사체
    /// 풀링 지원
    /// </summary>
    public class EnemyProjectile : Projectile
    {
        // ====== 설정 ======

        [Header("적 투사체 설정")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private float hitEffectDuration = 0.5f;


        // ====== Unity 생명주기 ======

        protected override void Awake()
        {
            base.Awake();

            // targetLayers가 설정되지 않았다면 Player Layer로 설정
            if (targetLayers == 0)
            {
                // Layer 7 = Player
                targetLayers = LayerMask.GetMask("Player");
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// 투사체 초기화 및 발사
        /// </summary>
        /// <param name="direction">발사 방향</param>
        /// <param name="projectileSpeed">투사체 속도</param>
        /// <param name="projectileDamage">투사체 데미지</param>
        public void Initialize(Vector2 direction, float projectileSpeed, int projectileDamage)
        {
            // 속도 및 데미지 설정
            speed = projectileSpeed;
            damage = projectileDamage;

            // 발사
            Launch(direction);
        }


        // ====== 충돌 ======

        /// <summary>
        /// 충돌 시 호출 (오버라이드)
        /// </summary>
        protected override void OnHit(Collider2D hitCollider)
        {
            // PlayerStats 찾기
            var playerStats = hitCollider.GetComponent<GASPT.Stats.PlayerStats>();
            if (playerStats != null && !playerStats.IsDead)
            {
                // 플레이어에게 데미지 적용
                playerStats.TakeDamage((int)damage);

                Debug.Log($"[EnemyProjectile] 플레이어에게 {damage} 데미지!");
            }

            // 시각 효과
            SpawnHitEffect();

            // 풀로 반환
            ReturnToPool();
        }


        // ====== 시각 효과 ======

        /// <summary>
        /// 충돌 시각 효과 생성
        /// </summary>
        private void SpawnHitEffect()
        {
            if (hitEffectPrefab == null) return;

            // 시각 효과 생성
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

            // 일정 시간 후 파괴
            Destroy(effect, hitEffectDuration);
        }


        // ====== 디버그 ======

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (!isActive) return;

            // 투사체 방향 표시 (빨간색)
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, direction * 1f);
        }

        [ContextMenu("Print EnemyProjectile Info")]
        private void DebugPrintInfo()
        {
            Debug.Log($"=== EnemyProjectile Info ===\n" +
                     $"Damage: {damage}\n" +
                     $"Speed: {speed}\n" +
                     $"Max Distance: {maxDistance}\n" +
                     $"Travel Distance: {travelDistance}\n" +
                     $"Is Active: {isActive}\n" +
                     $"Direction: {direction}\n" +
                     $"Target Layers: {targetLayers.value}\n" +
                     $"============================");
        }
    }
}
