using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Effects;
using GASPT.Gameplay.Enemies;
using GASPT.StatusEffects;
using GASPT.Core;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 메테오 스트라이크 - 화염 마법사 궁극기
    /// 딜레이 후 마우스 위치에 메테오 충돌
    /// 높은 범위 데미지 + 강력한 화상
    /// </summary>
    public class MeteorStrikeAbility : BaseProjectileAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "Meteor Strike";
        public override float Cooldown => 12f;  // 12초 쿨다운


        // ====== 스킬 설정 ======

        private const int ImpactDamage = 80;
        private const float ImpactRadius = 4f;
        private const float CastDelay = 1.5f;  // 시전 딜레이
        private const float BurnDuration = 4f;
        private const float BurnDamage = 10f;


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

            // 마우스 위치 (충돌 지점)
            Vector3 impactPoint = GetMousePosition();

            Debug.Log($"[MeteorStrike] 메테오 시전 시작! 충돌 지점: {impactPoint}");

            // 경고 표시 (시전 딜레이 동안)
            CreateWarningIndicator(impactPoint);

            // 딜레이 대기
            float elapsed = 0f;
            while (elapsed < CastDelay)
            {
                if (token.IsCancellationRequested) return;
                await Task.Yield();
                elapsed += Time.deltaTime;
            }

            // 메테오 충돌
            CreateMeteorImpact(impactPoint);
            ApplyMeteorDamage(impactPoint);

            Debug.Log($"[MeteorStrike] 메테오 충돌!");
        }


        // ====== 시각 효과 ======

        /// <summary>
        /// 경고 표시 (낙하 지점)
        /// </summary>
        private void CreateWarningIndicator(Vector3 position)
        {
            var warning = PoolManager.Instance?.Spawn<VisualEffect>(position, Quaternion.identity);

            if (warning != null)
            {
                // 빨간 경고 원
                Color warningColor = new Color(1f, 0f, 0f, 0.3f);
                Color fadeColor = new Color(1f, 0f, 0f, 0.6f);

                warning.Play(
                    duration: CastDelay,
                    startScale: ImpactRadius * 0.5f,
                    endScale: ImpactRadius,
                    startColor: warningColor,
                    endColor: fadeColor
                );
            }
        }

        /// <summary>
        /// 메테오 충돌 효과
        /// </summary>
        private void CreateMeteorImpact(Vector3 position)
        {
            var impact = PoolManager.Instance?.Spawn<VisualEffect>(position, Quaternion.identity);

            if (impact != null)
            {
                // 강렬한 주황-흰색 폭발
                Color startColor = new Color(1f, 0.8f, 0.3f, 1f);
                Color endColor = new Color(1f, 0.3f, 0f, 0f);

                impact.Play(
                    duration: 0.6f,
                    startScale: ImpactRadius * 0.5f,
                    endScale: ImpactRadius * 1.5f,
                    startColor: startColor,
                    endColor: endColor
                );
            }
        }


        // ====== 데미지 ======

        /// <summary>
        /// 메테오 충돌 데미지 적용
        /// </summary>
        private void ApplyMeteorDamage(Vector3 center)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, ImpactRadius);
            int hitCount = 0;

            foreach (var hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy == null) continue;

                // 충돌 데미지
                enemy.TakeDamage(ImpactDamage);

                // 강력한 화상 적용
                ApplyBurn(enemy.gameObject);

                hitCount++;
            }

            if (hitCount > 0)
            {
                Debug.Log($"[MeteorStrike] {hitCount}명의 적에게 메테오 데미지!");
            }
        }

        /// <summary>
        /// 화상 상태 효과 적용
        /// </summary>
        private void ApplyBurn(GameObject target)
        {
            var statusTarget = target.GetComponent<IStatusEffectTarget>();
            if (statusTarget != null)
            {
                statusTarget.ApplyStatusEffect(StatusEffectType.Burn, BurnDuration, BurnDamage);
            }
        }
    }
}
