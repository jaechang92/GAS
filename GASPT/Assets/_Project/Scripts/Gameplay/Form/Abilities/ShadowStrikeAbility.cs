using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Core;
using GASPT.CameraSystem;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 그림자 일격 - 암살자 스킬 1
    /// 마우스 위치로 순간이동 후 범위 공격
    /// </summary>
    public class ShadowStrikeAbility : BaseAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "ShadowStrike";
        public override float Cooldown => 5f;  // 5초 쿨다운


        // ====== 스킬 설정 ======

        private const float MaxTeleportDistance = 8f;  // 최대 순간이동 거리
        private const float StrikeDamage = 50f;        // 일격 대미지
        private const float StrikeRadius = 2f;         // 공격 범위
        private const float InvincibleDuration = 0.3f; // 무적 시간
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

            // 마우스 위치 계산
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            // 최대 거리 제한
            Vector3 direction = mousePos - caster.transform.position;
            if (direction.magnitude > MaxTeleportDistance)
            {
                mousePos = caster.transform.position + direction.normalized * MaxTeleportDistance;
            }

            // 그림자 일격 실행
            await PerformShadowStrike(caster, mousePos, token);

            // 쿨다운 시작
            StartCooldown();
        }

        /// <summary>
        /// 그림자 일격 실행
        /// </summary>
        private async Task PerformShadowStrike(GameObject caster, Vector3 targetPos, CancellationToken token)
        {
            Vector3 startPos = caster.transform.position;

            Debug.Log($"[ShadowStrike] 그림자 일격! {startPos} → {targetPos}");

            // 1. 페이드아웃 효과 (무적 시작)
            SetInvincible(caster, true);

            // 2. 짧은 대기 (연출용)
            await Awaitable.WaitForSecondsAsync(0.1f, token);

            // 3. 순간이동
            caster.transform.position = targetPos;

            // 4. 도착 지점에서 범위 공격
            PerformStrike(caster);

            // 5. 무적 해제 대기
            await Awaitable.WaitForSecondsAsync(InvincibleDuration, token);
            SetInvincible(caster, false);

            // 디버그 시각화
            Debug.DrawLine(startPos, targetPos, Color.magenta, 1f);
        }

        /// <summary>
        /// 범위 공격 실행
        /// </summary>
        private void PerformStrike(GameObject caster)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                caster.transform.position,
                StrikeRadius,
                LayerMask.GetMask(EnemyLayerName)
            );

            foreach (var hit in hits)
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(StrikeDamage, DamageType.Physical);
                    Debug.Log($"[ShadowStrike] {hit.name}에게 {StrikeDamage} 대미지!");
                }
            }

            Debug.Log($"[ShadowStrike] 적중: {hits.Length}명");
        }

        /// <summary>
        /// 무적 상태 설정
        /// </summary>
        private void SetInvincible(GameObject target, bool invincible)
        {
            var statusTarget = target.GetComponent<IStatusEffectTarget>();
            if (statusTarget != null)
            {
                if (invincible)
                {
                    statusTarget.ApplyStatusEffect(StatusEffectType.Invincible, InvincibleDuration);
                }
            }

            // 대체: IDamageable의 무적 플래그
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.SetInvincible(invincible);
            }
        }
    }
}
