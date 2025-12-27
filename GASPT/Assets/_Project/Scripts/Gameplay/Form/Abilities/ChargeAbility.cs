using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core;
using GASPT.Core.Enums;
using GASPT.CameraSystem;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 돌진 - 전사 스킬 1
    /// 전방으로 빠르게 돌진하며 경로의 적에게 대미지
    /// </summary>
    public class ChargeAbility : BaseAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "Charge";
        public override float Cooldown => 6f;  // 6초 쿨다운


        // ====== 스킬 설정 ======

        private const float ChargeDistance = 6f;    // 돌진 거리
        private const float ChargeSpeed = 20f;      // 돌진 속도
        private const float ChargeDamage = 40f;     // 돌진 대미지
        private const float HitRadius = 1f;         // 충돌 판정 반경
        private const string EnemyLayerName = "Enemy";


        // ====== 실행 ======

        public override async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (!CheckCooldown())
            {
                return;
            }

            // CameraManager에서 카메라 가져오기
            var mainCamera = CameraManager.Instance?.MainCamera;
            if (mainCamera == null) return;

            // 마우스 방향 계산
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 direction = (mousePos - caster.transform.position).normalized;

            // 돌진 실행
            await PerformCharge(caster, direction, token);

            // 쿨다운 시작
            StartCooldown();
        }

        /// <summary>
        /// 돌진 실행
        /// </summary>
        private async Task PerformCharge(GameObject caster, Vector2 direction, CancellationToken token)
        {
            Vector3 startPos = caster.transform.position;
            Vector3 targetPos = startPos + (Vector3)(direction * ChargeDistance);
            float travelTime = ChargeDistance / ChargeSpeed;
            float elapsed = 0f;

            Debug.Log($"[Charge] 돌진 시작! {startPos} → {targetPos}");

            // 돌진 중 적중한 적 추적 (중복 대미지 방지)
            var hitEnemies = new System.Collections.Generic.HashSet<GameObject>();

            // 돌진 이동
            while (elapsed < travelTime && !token.IsCancellationRequested)
            {
                float t = elapsed / travelTime;
                caster.transform.position = Vector3.Lerp(startPos, targetPos, t);

                // 이동 중 적 충돌 체크
                CheckChargeHits(caster, hitEnemies);

                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(token);
            }

            // 최종 위치 설정
            caster.transform.position = targetPos;

            // 디버그 시각화
            Debug.DrawLine(startPos, targetPos, Color.yellow, 1f);
            Debug.Log($"[Charge] 돌진 완료! 적중: {hitEnemies.Count}명");
        }

        /// <summary>
        /// 돌진 중 적 충돌 체크
        /// </summary>
        private void CheckChargeHits(GameObject caster, System.Collections.Generic.HashSet<GameObject> hitEnemies)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                caster.transform.position,
                HitRadius,
                LayerMask.GetMask(EnemyLayerName)
            );

            foreach (var hit in hits)
            {
                if (!hitEnemies.Contains(hit.gameObject))
                {
                    hitEnemies.Add(hit.gameObject);
                    ApplyChargeDamage(hit.gameObject);
                }
            }
        }

        /// <summary>
        /// 돌진 대미지 적용
        /// </summary>
        private void ApplyChargeDamage(GameObject target)
        {
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(ChargeDamage, DamageType.Physical);
                Debug.Log($"[Charge] {target.name}에게 {ChargeDamage} 돌진 대미지!");
            }

            // 넉백 효과 (선택적)
            var rb = target.GetComponent<Rigidbody2D>();
            var mainCamera = CameraManager.Instance?.MainCamera;
            if (rb != null && mainCamera != null)
            {
                Vector2 knockbackDir = (target.transform.position - mainCamera.ScreenToWorldPoint(Input.mousePosition)).normalized;
                rb.AddForce(knockbackDir * 5f, ForceMode2D.Impulse);
            }
        }
    }
}
