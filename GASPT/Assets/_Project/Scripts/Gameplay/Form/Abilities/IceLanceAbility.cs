using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Projectiles;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 아이스 랜스 - 얼음 마법사 스킬
    /// 빠른 얼음 창 투사체 발사
    /// 단일 대상 데미지 + 슬로우
    /// </summary>
    public class IceLanceAbility : BaseProjectileAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "Ice Lance";
        public override float Cooldown => 2f;  // 2초 쿨다운 (빠른 연사)


        // ====== 스킬 설정 ======

        private const int Damage = 25;
        private const float ProjectileSpeed = 18f;
        private const float SlowDuration = 1.5f;
        private const float SlowAmount = 40f;  // 40% 슬로우


        // ====== 실행 ======

        public override async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (!CheckCooldown())
            {
                return;
            }

            // 쿨다운 시작
            StartCooldown();

            // 마우스 방향
            Vector2 direction = GetMouseDirection(caster);
            Vector3 startPos = GetProjectileStartPositionTowardsMouse(caster, 0.5f);

            Debug.Log($"[IceLance] 아이스 랜스 발사! 방향: {direction}");

            // 투사체 생성
            LaunchIceLance(startPos, direction);

            await Task.CompletedTask;
        }


        // ====== 발사 ======

        /// <summary>
        /// 아이스 랜스 투사체 발사
        /// </summary>
        private void LaunchIceLance(Vector3 startPos, Vector2 direction)
        {
            var projectile = PoolManager.Instance?.Spawn<IceLanceProjectile>(startPos, Quaternion.identity);

            if (projectile != null)
            {
                // 투사체 설정
                projectile.Initialize(Damage, ProjectileSpeed, SlowDuration, SlowAmount);

                // 발사
                projectile.Launch(direction);
            }
            else
            {
                Debug.LogWarning("[IceLanceAbility] 풀에서 IceLanceProjectile을 가져올 수 없습니다!");
            }
        }
    }
}
