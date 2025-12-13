using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Core;
using GASPT.CameraSystem;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 방패 강타 - 전사 스킬 2
    /// 전방 적에게 대미지 + 스턴 효과
    /// </summary>
    public class ShieldBashAbility : BaseAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "ShieldBash";
        public override float Cooldown => 8f;  // 8초 쿨다운


        // ====== 스킬 설정 ======

        private const float BashRange = 2.5f;       // 강타 범위
        private const float BashDamage = 20f;       // 강타 대미지
        private const float StunDuration = 1.5f;    // 스턴 지속시간
        private const float BashAngle = 120f;       // 강타 각도
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
            if (mainCamera == null)
            {
                Debug.LogWarning("[ShieldBash] 메인 카메라를 찾을 수 없습니다.");
                return;
            }

            // 마우스 방향 계산
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 direction = (mousePos - caster.transform.position).normalized;

            // 강타 실행
            PerformBash(caster, direction);

            // 쿨다운 시작
            StartCooldown();

            // 강타 애니메이션 대기
            await Awaitable.WaitForSecondsAsync(0.3f, token);
        }

        /// <summary>
        /// 방패 강타 실행
        /// </summary>
        private void PerformBash(GameObject caster, Vector2 direction)
        {
            // 범위 내 적 탐색
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                caster.transform.position,
                BashRange,
                LayerMask.GetMask(EnemyLayerName)
            );

            int hitCount = 0;

            foreach (var hit in hits)
            {
                // 방향 체크
                Vector2 toTarget = (hit.transform.position - caster.transform.position).normalized;
                float angle = Vector2.Angle(direction, toTarget);

                if (angle <= BashAngle * 0.5f)
                {
                    // 대미지 및 스턴 적용
                    ApplyBashEffect(hit.gameObject);
                    hitCount++;
                }
            }

            // 디버그 시각화
            Debug.DrawRay(caster.transform.position, direction * BashRange, Color.cyan, 0.5f);
            Debug.Log($"[ShieldBash] 방패 강타! 적중: {hitCount}명, 스턴: {StunDuration}초");
        }

        /// <summary>
        /// 방패 강타 효과 적용
        /// </summary>
        private void ApplyBashEffect(GameObject target)
        {
            // 대미지 적용
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(BashDamage, DamageType.Physical);
            }

            // 스턴 효과 적용 (StatusEffectManager와 연동)
            var statusTarget = target.GetComponent<IStatusEffectTarget>();
            if (statusTarget != null)
            {
                statusTarget.ApplyStatusEffect(StatusEffectType.Stun, StunDuration);
                Debug.Log($"[ShieldBash] {target.name}에게 {StunDuration}초 스턴!");
            }
            else
            {
                Debug.Log($"[ShieldBash] {target.name} 적중 - 스턴 (IStatusEffectTarget 없음)");
            }

            // 넉백 효과
            var rb = target.GetComponent<Rigidbody2D>();
            var mainCamera = CameraManager.Instance?.MainCamera;
            if (rb != null && mainCamera != null)
            {
                Vector2 knockbackDir = (target.transform.position - mainCamera.ScreenToWorldPoint(Input.mousePosition)).normalized;
                rb.AddForce(knockbackDir * 8f, ForceMode2D.Impulse);
            }
        }
    }
}
