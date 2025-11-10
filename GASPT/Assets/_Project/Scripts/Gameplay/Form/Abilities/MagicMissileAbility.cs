using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Projectiles;

namespace GASPT.Form
{
    /// <summary>
    /// 마법 미사일 - 마법사의 기본 공격
    /// 마우스 방향으로 빠른 마법 투사체 발사
    /// 오브젝트 풀링 적용
    /// </summary>
    public class MagicMissileAbility : IAbility
    {
        public string AbilityName => "Magic Missile";
        public float Cooldown => 0.5f;  // 0.5초 쿨다운

        private float lastUsedTime;

        public async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (Time.time - lastUsedTime < Cooldown)
            {
                Debug.Log("[MagicMissile] 쿨다운 중...");
                return;
            }

            // 쿨다운 즉시 시작 (중복 실행 방지)
            lastUsedTime = Time.time;

            // 마우스 방향 계산
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 direction = (mousePos - caster.transform.position).normalized;

            Debug.Log($"[MagicMissile] 발사! 방향: {direction}");

            // 풀에서 마법 미사일 가져오기
            LaunchMissile(caster.transform.position, direction);

            // 비동기 작업 없음 (Projectile이 자체적으로 움직임)
            await Task.CompletedTask;
        }

        /// <summary>
        /// 마법 미사일 발사 (풀 사용)
        /// </summary>
        private void LaunchMissile(Vector3 startPos, Vector2 direction)
        {
            // 풀에서 MagicMissileProjectile 가져오기
            var missile = PoolManager.Instance.Spawn<MagicMissileProjectile>(
                startPos,
                Quaternion.identity
            );

            if (missile != null)
            {
                // Launch 호출 (Projectile이 자체적으로 움직임)
                missile.Launch(direction);
            }
            else
            {
                Debug.LogError("[MagicMissileAbility] 풀에서 MagicMissileProjectile을 가져올 수 없습니다!");
            }
        }
    }
}
