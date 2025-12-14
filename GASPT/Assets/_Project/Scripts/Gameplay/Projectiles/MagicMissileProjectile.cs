using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Enemies;
using GASPT.Gameplay.Effects;

namespace GASPT.Gameplay.Projectiles
{
    /// <summary>
    /// 마법 미사일 투사체
    /// 빠르고 작은 단일 타겟 투사체
    /// </summary>
    public class MagicMissileProjectile : Projectile
    {
        // ====== 초기화 ======

        protected override void Awake()
        {
            base.Awake();

            // 기본값 설정
            speed = 15f;
            maxDistance = 10f;
            damage = 25f;  // 밸런싱: 10 → 25 (원거리 DPS 50)
            collisionRadius = 0.2f;

            SetupVisuals();
        }

        /// <summary>
        /// 시각 효과 설정
        /// </summary>
        private void SetupVisuals()
        {
            // 기본 Renderer 설정 (보라빛 파랑)
            if (projectileRenderer != null)
            {
                projectileRenderer.material.color = new Color(0.5f, 0.3f, 1f);
            }

            // Trail 설정
            if (trailRenderer != null)
            {
                trailRenderer.time = 0.3f;
                trailRenderer.startWidth = 0.15f;
                trailRenderer.endWidth = 0.02f;
                trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
                trailRenderer.startColor = new Color(0.5f, 0.3f, 1f, 1f); // 보라색
                trailRenderer.endColor = new Color(0.3f, 0.8f, 1f, 0f); // 투명한 하늘색
            }
        }


        // ====== 충돌 처리 ======

        /// <summary>
        /// 충돌 시 타격 효과
        /// </summary>
        protected override void OnHit(Collider2D hitCollider)
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                // 데미지 적용
                enemy.TakeDamage((int)damage);
                Debug.Log($"[MagicMissileProjectile] {enemy.Data.enemyName}에 {damage} 데미지!");

                // 타격 시각 효과
                PlayHitEffect(transform.position);
            }

            // 풀로 반환
            ReturnToPool();
        }


        // ====== 시각 효과 ======

        /// <summary>
        /// 타격 시각 효과 (풀 사용)
        /// </summary>
        private void PlayHitEffect(Vector3 hitPos)
        {
            // 풀에서 VisualEffect 가져오기
            var hitFlash = PoolManager.Instance.Spawn<VisualEffect>(
                hitPos,
                Quaternion.identity
            );

            if (hitFlash != null)
            {
                // 타격 효과 설정
                Color startColor = new Color(0.3f, 0.8f, 1f, 0.9f); // 밝은 파란색
                Color endColor = new Color(0.3f, 0.8f, 1f, 0f);     // 투명

                hitFlash.Play(
                    duration: 0.2f,
                    startScale: 0.3f,
                    endScale: 0.8f,
                    startColor: startColor,
                    endColor: endColor
                );
            }
            else
            {
                Debug.LogWarning("[MagicMissileProjectile] 타격 효과를 풀에서 가져올 수 없습니다.");
            }
        }
    }
}
