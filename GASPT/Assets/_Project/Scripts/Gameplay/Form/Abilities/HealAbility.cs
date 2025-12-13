using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 회복 - 공용 스킬
    /// 자신의 체력을 회복
    /// </summary>
    public class HealAbility : BaseAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "Heal";
        public override float Cooldown => 10f;  // 10초 쿨다운


        // ====== 스킬 설정 ======

        private const float HealAmount = 30f;        // 기본 회복량
        private const float HealPercent = 0.2f;      // 최대 체력의 20% 추가 회복
        private const float CastTime = 0.5f;         // 시전 시간


        // ====== 실행 ======

        public override async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (!CheckCooldown())
            {
                return;
            }

            // 회복 실행
            await PerformHeal(caster, token);

            // 쿨다운 시작
            StartCooldown();
        }

        /// <summary>
        /// 회복 실행
        /// </summary>
        private async Task PerformHeal(GameObject caster, CancellationToken token)
        {
            Debug.Log($"[Heal] 회복 시전 중... ({CastTime}초)");

            // 시전 시간 대기
            await Awaitable.WaitForSecondsAsync(CastTime, token);

            // 회복량 계산
            float totalHeal = CalculateHealAmount(caster);

            // 회복 적용
            ApplyHeal(caster, totalHeal);

            // 이펙트 (TODO: 파티클)
            SpawnHealEffect(caster);

            Debug.Log($"[Heal] 회복 완료! +{totalHeal} HP");
        }

        /// <summary>
        /// 회복량 계산
        /// </summary>
        private float CalculateHealAmount(GameObject caster)
        {
            float baseHeal = HealAmount;

            // 최대 체력 비례 추가 회복
            var damageable = caster.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float bonusHeal = damageable.MaxHealth * HealPercent;
                baseHeal += bonusHeal;
            }

            return baseHeal;
        }

        /// <summary>
        /// 회복 적용
        /// </summary>
        private void ApplyHeal(GameObject target, float amount)
        {
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.Heal(amount);
                Debug.Log($"[Heal] {target.name} 회복: +{amount} (현재: {damageable.CurrentHealth}/{damageable.MaxHealth})");
            }
            else
            {
                Debug.Log($"[Heal] {target.name}에게 회복 (IDamageable 없음)");
            }
        }

        /// <summary>
        /// 회복 이펙트 생성
        /// </summary>
        private void SpawnHealEffect(GameObject caster)
        {
            // TODO: 파티클 시스템 연동
            // - 초록색 빛 이펙트
            // - 상승하는 파티클

            // 디버그 시각화
            Debug.DrawRay(caster.transform.position, Vector3.up * 2f, Color.green, 1f);
        }
    }
}
