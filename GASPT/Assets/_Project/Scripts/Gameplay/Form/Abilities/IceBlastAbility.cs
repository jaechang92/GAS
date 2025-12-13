using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Core;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Effects;
using GASPT.Gameplay.Enemies;
using GASPT.StatusEffects;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 빙결 - 신규 스킬
    /// 마우스 위치에 빙결 범위 공격 (데미지 + 슬로우)
    /// 오브젝트 풀링 적용
    /// </summary>
    public class IceBlastAbility : BaseProjectileAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "Ice Blast";
        public override float Cooldown => 3f;  // 3초 쿨다운


        // ====== 스킬 설정 ======

        private const int Damage = 30;
        private const float BlastRadius = 2.5f;
        private const float SlowDuration = 2f;
        private const float SlowAmount = 0.5f;  // 50% 슬로우


        // ====== 실행 ======

        public override async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (!CheckCooldown())
            {
                return;
            }

            // 쿨다운 시작 (중복 실행 방지)
            StartCooldown();

            // 마우스 위치 가져오기
            Vector3 mousePos = GetMousePosition();

            Debug.Log($"[IceBlast] 빙결 시전! 위치: {mousePos}");

            // 빙결 효과 생성
            CreateIceBlastEffect(mousePos);

            // 범위 내 적에게 데미지 + 슬로우
            ApplyIceBlastDamage(mousePos);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 빙결 시각 효과 생성 (풀 사용)
        /// </summary>
        private void CreateIceBlastEffect(Vector3 position)
        {
            // 풀에서 VisualEffect 가져오기
            var effect = PoolManager.Instance.Spawn<VisualEffect>(position, Quaternion.identity);

            if (effect != null)
            {
                // 파란색 빙결 효과
                SpriteRenderer sr = effect.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(0.5f, 0.8f, 1f);  // 밝은 파란색
                }

                Color startColor = new Color(0.5f, 0.8f, 1f, 0.8f); // 반투명 파란색
                Color endColor = new Color(0.5f, 0.8f, 1f, 0f);     // 투명

                // 크기 애니메이션 (0.5 → 2.5 → 0)
                effect.Play(
                    duration: 0.5f,
                    startScale: 0.5f,
                    endScale: BlastRadius,
                    startColor: startColor,
                    endColor: startColor
                );
            }
            else
            {
                Debug.LogWarning("[IceBlastAbility] VisualEffect를 풀에서 가져올 수 없습니다!");
            }
        }

        /// <summary>
        /// 범위 내 적에게 데미지 및 슬로우 적용
        /// </summary>
        private void ApplyIceBlastDamage(Vector3 center)
        {
            // Physics2D.OverlapCircleAll로 범위 내 모든 오브젝트 찾기
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, BlastRadius);

            int hitCount = 0;

            foreach (var hit in hits)
            {
                // Enemy 컴포넌트 확인
                Enemy enemy = hit.GetComponent<Enemy>();

                if (enemy != null)
                {
                    // 데미지 적용
                    enemy.TakeDamage(Damage);

                    // 슬로우 효과 적용 (TODO: StatusEffect 통합)
                    ApplySlow(enemy.gameObject);

                    hitCount++;
                }
            }

            if (hitCount > 0)
            {
                Debug.Log($"[IceBlast] {hitCount}명의 적에게 빙결 데미지 적용!");
            }
            else
            {
                Debug.Log("[IceBlast] 범위 내 적 없음");
            }
        }

        /// <summary>
        /// 슬로우 효과 적용
        /// </summary>
        private void ApplySlow(GameObject target)
        {
            // IStatusEffectTarget을 통한 슬로우 적용
            var statusTarget = target.GetComponent<IStatusEffectTarget>();
            if (statusTarget != null)
            {
                // 슬로우 효과: 지속시간 2초, 50% 감속
                statusTarget.ApplyStatusEffect(StatusEffectType.Slow, SlowDuration, SlowAmount * 100f);
                Debug.Log($"[IceBlast] {target.name}에게 슬로우 적용 (지속시간: {SlowDuration}초, 감소량: {SlowAmount * 100}%)");
            }
            else
            {
                Debug.Log($"[IceBlast] {target.name}에 IStatusEffectTarget 없음 - 슬로우 미적용");
            }
        }
    }
}
