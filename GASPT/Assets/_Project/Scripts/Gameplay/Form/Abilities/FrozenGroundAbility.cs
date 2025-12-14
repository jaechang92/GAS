using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Effects;
using GASPT.Gameplay.Enemies;
using GASPT.StatusEffects;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 빙결 지대 - 얼음 마법사 궁극기
    /// 마우스 위치에 지속되는 얼음 지대 생성
    /// 범위 내 적에게 슬로우 + 틱 데미지 + 빙결 확률
    /// </summary>
    public class FrozenGroundAbility : BaseProjectileAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "Frozen Ground";
        public override float Cooldown => 10f;  // 10초 쿨다운


        // ====== 스킬 설정 ======

        private const int DamagePerTick = 10;
        private const float GroundRadius = 3.5f;
        private const float GroundDuration = 4f;
        private const float TickInterval = 0.5f;
        private const float SlowDuration = 1f;
        private const float SlowAmount = 60f;  // 60% 슬로우
        private const float FreezeChance = 0.15f;  // 15% 빙결 확률
        private const float FreezeDuration = 1.5f;


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
            Vector3 groundCenter = GetMousePosition();

            Debug.Log($"[FrozenGround] 빙결 지대 생성! 위치: {groundCenter}");

            // 빙결 지대 효과 생성
            CreateFrozenGroundEffect(groundCenter);

            // 지속 효과 적용
            await ApplyFrozenGroundEffectAsync(groundCenter, token);
        }


        // ====== 시각 효과 ======

        /// <summary>
        /// 빙결 지대 시각 효과
        /// </summary>
        private void CreateFrozenGroundEffect(Vector3 position)
        {
            var effect = PoolManager.Instance?.Spawn<VisualEffect>(position, Quaternion.identity);

            if (effect != null)
            {
                // 밝은 파란-흰색 얼음 효과
                Color startColor = new Color(0.7f, 0.9f, 1f, 0.6f);
                Color endColor = new Color(0.5f, 0.8f, 1f, 0f);

                effect.Play(
                    duration: GroundDuration,
                    startScale: GroundRadius,
                    endScale: GroundRadius * 0.8f,
                    startColor: startColor,
                    endColor: endColor
                );
            }
        }


        // ====== 지속 효과 ======

        /// <summary>
        /// 빙결 지대 지속 효과 비동기 처리
        /// </summary>
        private async Task ApplyFrozenGroundEffectAsync(Vector3 center, CancellationToken token)
        {
            float elapsed = 0f;
            float lastTickTime = 0f;

            while (elapsed < GroundDuration)
            {
                if (token.IsCancellationRequested) break;

                // 틱 간격마다 효과 적용
                if (elapsed - lastTickTime >= TickInterval)
                {
                    ApplyTickEffect(center);
                    lastTickTime = elapsed;
                }

                await Task.Yield();
                elapsed += Time.deltaTime;
            }
        }

        /// <summary>
        /// 틱 효과 적용
        /// </summary>
        private void ApplyTickEffect(Vector3 center)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, GroundRadius);

            foreach (var hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy == null) continue;

                // 틱 데미지
                enemy.TakeDamage(DamagePerTick);

                // 슬로우 효과
                ApplySlow(enemy.gameObject);

                // 빙결 확률 체크
                if (Random.value < FreezeChance)
                {
                    ApplyFreeze(enemy.gameObject);
                }
            }
        }

        /// <summary>
        /// 슬로우 상태 효과 적용
        /// </summary>
        private void ApplySlow(GameObject target)
        {
            var statusTarget = target.GetComponent<IStatusEffectTarget>();
            if (statusTarget != null)
            {
                statusTarget.ApplyStatusEffect(StatusEffectType.Slow, SlowDuration, SlowAmount);
            }
        }

        /// <summary>
        /// 빙결 상태 효과 적용
        /// </summary>
        private void ApplyFreeze(GameObject target)
        {
            var statusTarget = target.GetComponent<IStatusEffectTarget>();
            if (statusTarget != null)
            {
                statusTarget.ApplyStatusEffect(StatusEffectType.Freeze, FreezeDuration, 0f);
                Debug.Log($"[FrozenGround] {target.name} 빙결!");
            }
        }
    }
}
