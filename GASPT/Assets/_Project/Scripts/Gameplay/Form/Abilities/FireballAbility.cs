using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GASPT.Form
{
    /// <summary>
    /// 화염구 - 마법사 스킬 2
    /// 강력한 화염 투사체 발사 (폭발 범위 데미지)
    /// </summary>
    public class FireballAbility : IAbility
    {
        public string AbilityName => "Fireball";
        public float Cooldown => 5f;  // 5초 쿨다운

        private float lastUsedTime;
        private const float FireballDamage = 50f;  // 직격 데미지
        private const float FireballSpeed = 10f;   // 투사체 속도
        private const float ExplosionRadius = 3f;  // 폭발 반경

        public async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (Time.time - lastUsedTime < Cooldown)
            {
                Debug.Log("[Fireball] 쿨다운 중...");
                return;
            }

            // 마우스 방향 계산
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 direction = (mousePos - caster.transform.position).normalized;

            // 화염구 발사
            await LaunchFireball(caster.transform.position, direction, token);

            // 쿨다운 시작
            lastUsedTime = Time.time;

            Debug.Log($"[Fireball] 화염구 발사! 방향: {direction}");
        }

        /// <summary>
        /// 화염구 발사 및 폭발
        /// TODO: 실제 투사체 프리팹 및 폭발 이펙트
        /// </summary>
        private async Task LaunchFireball(Vector3 startPos, Vector2 direction, CancellationToken token)
        {
            // 임시 구현 - 직선 레이캐스트로 시뮬레이션
            Vector3 endPos = startPos + (Vector3)(direction * 20f);

            // TODO: 실제 투사체 생성
            // GameObject fireball = Object.Instantiate(fireballPrefab, startPos, Quaternion.identity);
            // Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();
            // rb.linearVelocity = direction * FireballSpeed;

            Debug.DrawRay(startPos, direction * 20f, Color.red, 2f);

            // 비행 시간 시뮬레이션
            float travelTime = Vector2.Distance(startPos, endPos) / FireballSpeed;
            await Awaitable.WaitForSecondsAsync(travelTime, token);

            // 폭발
            Explode(endPos);
        }

        /// <summary>
        /// 폭발 효과 (범위 데미지)
        /// </summary>
        private void Explode(Vector3 explosionPos)
        {
            Debug.Log($"[Fireball] 폭발! 위치: {explosionPos}, 반경: {ExplosionRadius}m");

            // TODO: 범위 내 적들에게 데미지
            // Collider2D[] hits = Physics2D.OverlapCircleAll(explosionPos, ExplosionRadius);
            // foreach (var hit in hits)
            // {
            //     if (hit.CompareTag("Enemy"))
            //     {
            //         hit.GetComponent<HealthSystem>().TakeDamage(FireballDamage, null);
            //     }
            // }

            // TODO: 폭발 이펙트 재생
            // ParticleSystem explosion = Object.Instantiate(explosionEffectPrefab, explosionPos, Quaternion.identity);
            // explosion.Play();

            // 임시 디버그 시각화
            Debug.DrawRay(explosionPos, Vector3.up * ExplosionRadius, Color.yellow, 2f);
            Debug.DrawRay(explosionPos, Vector3.down * ExplosionRadius, Color.yellow, 2f);
            Debug.DrawRay(explosionPos, Vector3.left * ExplosionRadius, Color.yellow, 2f);
            Debug.DrawRay(explosionPos, Vector3.right * ExplosionRadius, Color.yellow, 2f);

            Debug.Log($"[Fireball] 데미지: {FireballDamage}, 범위 데미지 적용 완료 (임시)");
        }
    }
}
