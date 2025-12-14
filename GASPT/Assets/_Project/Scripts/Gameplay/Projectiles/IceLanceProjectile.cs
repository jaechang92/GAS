using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Gameplay.Enemies;
using GASPT.StatusEffects;
using GASPT.Core;

namespace GASPT.Gameplay.Projectiles
{
    /// <summary>
    /// 아이스 랜스 투사체
    /// 단일 대상 데미지 + 슬로우 효과
    /// </summary>
    public class IceLanceProjectile : Projectile
    {
        // ====== 상태 효과 설정 ======

        [Header("상태 효과")]
        [SerializeField] private float slowDuration = 1.5f;
        [SerializeField] private float slowAmount = 40f;  // 40% 슬로우


        // ====== 초기화 ======

        /// <summary>
        /// 투사체 설정 초기화
        /// </summary>
        public void Initialize(float damage, float speed, float slowDur, float slowAmt)
        {
            this.damage = damage;
            this.speed = speed;
            this.slowDuration = slowDur;
            this.slowAmount = slowAmt;
        }


        // ====== 오버라이드 ======

        protected override void OnHit(Collider2D hitCollider)
        {
            var enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                // 데미지 적용
                enemy.TakeDamage((int)damage);
                Debug.Log($"[IceLance] {enemy.Data.enemyName}에 {damage} 데미지!");

                // 슬로우 효과 적용
                ApplySlow(enemy.gameObject);
            }

            // 풀로 반환
            ReturnToPool();
        }

        /// <summary>
        /// 슬로우 상태 효과 적용
        /// </summary>
        private void ApplySlow(GameObject target)
        {
            var statusTarget = target.GetComponent<IStatusEffectTarget>();
            if (statusTarget != null)
            {
                statusTarget.ApplyStatusEffect(StatusEffectType.Slow, slowDuration, slowAmount);
                Debug.Log($"[IceLance] {target.name}에게 슬로우 적용 (지속: {slowDuration}초, 감소: {slowAmount}%)");
            }
        }


        // ====== OnSpawn 초기화 ======

        public override void OnSpawn()
        {
            base.OnSpawn();

            // 시각 효과: 얼음색
            if (projectileRenderer is SpriteRenderer sr)
            {
                sr.color = new Color(0.6f, 0.9f, 1f);  // 밝은 얼음색
            }
        }
    }
}
