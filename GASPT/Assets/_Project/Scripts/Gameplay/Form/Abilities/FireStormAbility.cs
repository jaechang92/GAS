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
    /// 파이어스톰 - 화염 마법사 스킬
    /// 마우스 위치에 지속되는 화염 폭풍 생성
    /// 범위 내 적에게 틱 데미지 + 화상 상태 효과
    /// </summary>
    public class FireStormAbility : BaseProjectileAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "Fire Storm";
        public override float Cooldown => 8f;  // 8초 쿨다운


        // ====== 스킬 설정 ======

        private const int DamagePerTick = 18;  // 밸런싱: 15 → 18 (화염 특화 강화)
        private const float StormRadius = 3f;
        private const float StormDuration = 3f;
        private const float TickInterval = 0.5f;
        private const float BurnDuration = 2f;
        private const float BurnDamage = 5f;


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

            // 마우스 위치
            Vector3 stormCenter = GetMousePosition();

            Debug.Log($"[FireStorm] 화염 폭풍 생성! 위치: {stormCenter}");

            // 화염 폭풍 효과 생성
            CreateFireStormEffect(stormCenter);

            // 지속 데미지 적용
            await ApplyFireStormDamageAsync(stormCenter, token);
        }


        // ====== 시각 효과 ======

        /// <summary>
        /// 화염 폭풍 시각 효과 생성
        /// </summary>
        private void CreateFireStormEffect(Vector3 position)
        {
            var effect = PoolManager.Instance?.Spawn<VisualEffect>(position, Quaternion.identity);

            if (effect != null)
            {
                // 주황-빨강 화염색
                Color startColor = new Color(1f, 0.5f, 0f, 0.7f);
                Color endColor = new Color(1f, 0.2f, 0f, 0f);

                effect.Play(
                    duration: StormDuration,
                    startScale: StormRadius * 0.8f,
                    endScale: StormRadius * 1.2f,
                    startColor: startColor,
                    endColor: endColor
                );
            }
        }


        // ====== 지속 데미지 ======

        /// <summary>
        /// 화염 폭풍 지속 데미지 비동기 처리
        /// </summary>
        private async Task ApplyFireStormDamageAsync(Vector3 center, CancellationToken token)
        {
            float elapsed = 0f;
            float lastTickTime = 0f;

            while (elapsed < StormDuration)
            {
                if (token.IsCancellationRequested) break;

                // 틱 간격마다 데미지 적용
                if (elapsed - lastTickTime >= TickInterval)
                {
                    ApplyTickDamage(center);
                    lastTickTime = elapsed;
                }

                await Task.Yield();
                elapsed += Time.deltaTime;
            }
        }

        /// <summary>
        /// 틱 데미지 적용
        /// </summary>
        private void ApplyTickDamage(Vector3 center)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, StormRadius);

            foreach (var hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy == null) continue;

                // 틱 데미지
                enemy.TakeDamage(DamagePerTick);

                // 화상 효과 적용
                ApplyBurn(enemy.gameObject);
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
