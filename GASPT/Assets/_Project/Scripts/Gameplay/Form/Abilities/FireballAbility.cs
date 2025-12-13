using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Projectiles;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 화염구 - 마법사 스킬 2
    /// 강력한 화염 투사체 발사 (폭발 범위 데미지)
    /// 오브젝트 풀링 적용
    /// </summary>
    public class FireballAbility : BaseProjectileAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "Fireball";
        public override float Cooldown => 5f;  // 5초 쿨다운


        // ====== 실행 ======

        public override async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (!CheckCooldown())
            {
                return;
            }

            // 쿨다운 시작 (중복 실행 방지)
            StartCooldown();

            // 마우스 방향 계산
            Vector2 direction = GetMouseDirection(caster);

            Debug.Log($"[Fireball] 화염구 발사! 방향: {direction}");

            // 풀에서 화염구 가져오기
            LaunchFireball(caster.transform.position, direction);

            // 비동기 작업 없음 (Projectile이 자체적으로 움직임)
            await Task.CompletedTask;
        }


        // ====== 발사 ======

        /// <summary>
        /// 화염구 발사 (풀 사용)
        /// </summary>
        private void LaunchFireball(Vector3 startPos, Vector2 direction)
        {
            // 풀에서 FireballProjectile 가져오기
            var fireball = PoolManager.Instance.Spawn<FireballProjectile>(
                startPos,
                Quaternion.identity
            );

            if (fireball != null)
            {
                // Launch 호출 (Projectile이 자체적으로 움직임)
                fireball.Launch(direction);
            }
            else
            {
                Debug.LogError("[FireballAbility] 풀에서 FireballProjectile을 가져올 수 없습니다!");
            }
        }
    }
}
