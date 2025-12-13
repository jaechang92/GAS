using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Core;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 연막 - 암살자 스킬 3
    /// 짧은 무적 + 이동속도 증가
    /// </summary>
    public class SmokeScreenAbility : BaseAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "SmokeScreen";
        public override float Cooldown => 12f;  // 12초 쿨다운


        // ====== 스킬 설정 ======

        private const float InvincibleDuration = 1.5f;  // 무적 지속시간
        private const float SpeedBoostDuration = 3f;    // 속도 버프 지속시간
        private const float SpeedBoostAmount = 50f;     // 속도 증가량 (%)
        private const float SmokeRadius = 3f;           // 연막 범위


        // ====== 실행 ======

        public override async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (!CheckCooldown())
            {
                return;
            }

            // 연막 실행
            await PerformSmokeScreen(caster, token);

            // 쿨다운 시작
            StartCooldown();
        }

        /// <summary>
        /// 연막 실행
        /// </summary>
        private async Task PerformSmokeScreen(GameObject caster, CancellationToken token)
        {
            Debug.Log($"[SmokeScreen] 연막 발동! 무적: {InvincibleDuration}초, 속도 증가: {SpeedBoostAmount}%");

            // 1. 무적 적용
            ApplyInvincibility(caster);

            // 2. 속도 버프 적용
            ApplySpeedBoost(caster);

            // 3. 연막 이펙트 (TODO: 파티클)
            SpawnSmokeEffect(caster);

            // 4. 무적 지속시간 대기
            await Awaitable.WaitForSecondsAsync(InvincibleDuration, token);

            // 5. 무적 해제
            RemoveInvincibility(caster);

            Debug.Log("[SmokeScreen] 무적 종료, 속도 버프 유지 중...");
        }

        /// <summary>
        /// 무적 적용
        /// </summary>
        private void ApplyInvincibility(GameObject target)
        {
            var statusTarget = target.GetComponent<IStatusEffectTarget>();
            if (statusTarget != null)
            {
                statusTarget.ApplyStatusEffect(StatusEffectType.Invincible, InvincibleDuration);
            }

            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.SetInvincible(true);
            }
        }

        /// <summary>
        /// 무적 해제
        /// </summary>
        private void RemoveInvincibility(GameObject target)
        {
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.SetInvincible(false);
            }
        }

        /// <summary>
        /// 속도 버프 적용
        /// </summary>
        private void ApplySpeedBoost(GameObject target)
        {
            var statusTarget = target.GetComponent<IStatusEffectTarget>();
            if (statusTarget != null)
            {
                statusTarget.ApplyStatusEffect(StatusEffectType.SpeedUp, SpeedBoostDuration, SpeedBoostAmount);
            }
        }

        /// <summary>
        /// 연막 이펙트 생성
        /// </summary>
        private void SpawnSmokeEffect(GameObject caster)
        {
            // TODO: 파티클 시스템 연동
            // var smokeParticle = ObjectPool.Get<ParticleSystem>("SmokeEffect");
            // smokeParticle.transform.position = caster.transform.position;
            // smokeParticle.Play();

            // 디버그 시각화
            Debug.DrawRay(caster.transform.position, Vector3.up * SmokeRadius, Color.gray, InvincibleDuration);
            Debug.DrawRay(caster.transform.position, Vector3.down * SmokeRadius, Color.gray, InvincibleDuration);
            Debug.DrawRay(caster.transform.position, Vector3.left * SmokeRadius, Color.gray, InvincibleDuration);
            Debug.DrawRay(caster.transform.position, Vector3.right * SmokeRadius, Color.gray, InvincibleDuration);
        }
    }
}
