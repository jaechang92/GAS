using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Projectiles;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 원거리 패턴
    /// 투사체 발사, 연속 발사 등
    /// </summary>
    [System.Serializable]
    public class RangedPattern : BossPattern
    {
        // ====== 원거리 패턴 설정 ======

        [Header("원거리 패턴 설정")]
        [Tooltip("투사체 속도")]
        [Range(5f, 30f)]
        public float projectileSpeed = 12f;

        [Tooltip("발사 수")]
        [Range(1, 10)]
        public int projectileCount = 1;

        [Tooltip("발사 간격 (연속 발사 시)")]
        [Range(0f, 1f)]
        public float fireInterval = 0.2f;

        [Tooltip("확산 각도 (복수 발사 시)")]
        [Range(0f, 90f)]
        public float spreadAngle = 15f;

        [Tooltip("조준 방식 (true = 플레이어 추적)")]
        public bool trackTarget = true;


        // ====== 생성자 ======

        public RangedPattern()
        {
            patternName = "원거리 공격";
            patternType = PatternType.Ranged;
            damage = 15;
            cooldown = 3f;
            telegraphDuration = 0.5f;
            weight = 20;
            minPhase = 1;
            minRange = 3f;
            maxRange = 20f;
        }


        // ====== 텔레그래프 ======

        public override void ShowTelegraph(BaseBoss boss, Vector3 targetPosition)
        {
            // 마커로 대상 위치 표시
            if (trackTarget)
            {
                var player = Object.FindAnyObjectByType<GASPT.Stats.PlayerStats>();
                if (player != null)
                {
                    TelegraphController.Instance.ShowMarker(
                        player.transform,
                        telegraphDuration,
                        new Color(1f, 0f, 0f, 0.5f)
                    );
                }
            }
            else
            {
                TelegraphController.Instance.ShowCircle(
                    targetPosition,
                    1f,
                    telegraphDuration,
                    new Color(1f, 0f, 0f, 0.3f)
                );
            }
        }


        // ====== 실행 ======

        public override async Awaitable Execute(BaseBoss boss, Transform target)
        {
            if (boss == null || target == null) return;

            BeginExecution();

            try
            {
                // 1. 텔레그래프 표시
                ShowTelegraph(boss, target.position);

                // 2. 텔레그래프 시간 대기
                await Awaitable.WaitForSecondsAsync(telegraphDuration);
                if (IsCancelled()) return;

                // 3. 투사체 발사
                for (int i = 0; i < projectileCount; i++)
                {
                    if (IsCancelled()) return;

                    // 방향 계산
                    Vector2 baseDirection = (target.position - boss.transform.position).normalized;

                    // 확산 적용
                    float angle = 0f;
                    if (projectileCount > 1)
                    {
                        float totalSpread = spreadAngle * (projectileCount - 1);
                        angle = -totalSpread / 2f + spreadAngle * i;
                    }

                    Vector2 direction = RotateVector(baseDirection, angle);

                    // 투사체 발사
                    FireProjectile(boss.transform.position, direction);

                    // 연속 발사 간격
                    if (i < projectileCount - 1 && fireInterval > 0)
                    {
                        await Awaitable.WaitForSecondsAsync(fireInterval);
                    }
                }

                Debug.Log($"[RangedPattern] {patternName} 완료 (발사: {projectileCount})");
            }
            finally
            {
                EndExecution();
            }
        }


        // ====== 투사체 발사 ======

        private void FireProjectile(Vector3 spawnPos, Vector2 direction)
        {
            // 풀에서 투사체 가져오기
            if (PoolManager.Instance != null)
            {
                var projectile = PoolManager.Instance.Spawn<EnemyProjectile>(
                    spawnPos,
                    Quaternion.identity
                );

                if (projectile != null)
                {
                    projectile.Initialize(direction, projectileSpeed, damage);
                }
                else
                {
                    Debug.LogWarning("[RangedPattern] 투사체를 풀에서 가져올 수 없습니다.");
                }
            }
        }


        // ====== 유틸리티 ======

        private Vector2 RotateVector(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);

            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }
    }
}
