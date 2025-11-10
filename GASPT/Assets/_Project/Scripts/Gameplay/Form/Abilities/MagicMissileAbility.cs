using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GASPT.Form
{
    /// <summary>
    /// 마법 미사일 - 마법사의 기본 공격
    /// 마우스 방향으로 빠른 마법 투사체 발사
    /// </summary>
    public class MagicMissileAbility : IAbility
    {
        public string AbilityName => "Magic Missile";
        public float Cooldown => 0.5f;  // 0.5초 쿨다운

        private float lastUsedTime;
        private const float MissileDamage = 10f;
        private const float MissileSpeed = 15f;

        public async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (Time.time - lastUsedTime < Cooldown)
            {
                Debug.Log("[MagicMissile] 쿨다운 중...");
                return;
            }

            // 마우스 방향 계산
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 direction = (mousePos - caster.transform.position).normalized;

            // 마법 미사일 발사
            FireMissile(caster.transform.position, direction);

            // 쿨다운 시작
            lastUsedTime = Time.time;

            Debug.Log($"[MagicMissile] 발사! 방향: {direction}");

            // 비동기 대기 (애니메이션 등)
            await Awaitable.NextFrameAsync(token);
        }

        /// <summary>
        /// 마법 미사일 발사
        /// TODO: 실제 투사체 프리팹 생성 및 물리 처리
        /// </summary>
        private void FireMissile(Vector3 startPos, Vector2 direction)
        {
            // 임시 구현 - 나중에 실제 투사체로 교체
            Debug.DrawRay(startPos, direction * 10f, Color.cyan, 1f);

            // TODO: 투사체 생성
            // GameObject missile = Object.Instantiate(missilePrefab, startPos, Quaternion.identity);
            // Rigidbody2D rb = missile.GetComponent<Rigidbody2D>();
            // rb.linearVelocity = direction * MissileSpeed;

            Debug.Log($"[MagicMissile] 투사체 발사 (임시) - 데미지: {MissileDamage}, 속도: {MissileSpeed}");
        }
    }
}
