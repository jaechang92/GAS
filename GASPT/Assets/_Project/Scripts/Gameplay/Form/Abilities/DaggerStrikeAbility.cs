using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core;
using GASPT.CameraSystem;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 단검 공격 - 암살자 기본 공격
    /// 빠른 연속 근접 공격
    /// </summary>
    public class DaggerStrikeAbility : BaseAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "DaggerStrike";
        public override float Cooldown => 0.2f;  // 0.2초 쿨다운 (매우 빠름)


        // ====== 스킬 설정 ======

        private const float AttackRange = 1.5f;     // 공격 범위 (짧음)
        private const float BaseDamage = 15f;       // 기본 대미지 (낮음)
        private const float CriticalChance = 0.25f; // 크리티컬 확률 25%
        private const float CriticalMultiplier = 2f; // 크리티컬 배율
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

            // 공격 실행
            PerformStrike(caster, direction);

            // 쿨다운 시작
            StartCooldown();

            // 짧은 공격 애니메이션 대기
            await Awaitable.WaitForSecondsAsync(0.1f, token);
        }

        /// <summary>
        /// 단검 공격 실행
        /// </summary>
        private void PerformStrike(GameObject caster, Vector2 direction)
        {
            // 가장 가까운 적 탐색
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                caster.transform.position,
                AttackRange,
                LayerMask.GetMask(EnemyLayerName)
            );

            if (hits.Length == 0)
            {
                Debug.Log("[DaggerStrike] 범위 내 적 없음");
                return;
            }

            // 마우스 방향에 가장 가까운 적 선택
            Collider2D closest = null;
            float closestAngle = float.MaxValue;

            foreach (var hit in hits)
            {
                Vector2 toTarget = (hit.transform.position - caster.transform.position).normalized;
                float angle = Vector2.Angle(direction, toTarget);

                if (angle < closestAngle && angle < 60f) // 60도 내의 적만
                {
                    closestAngle = angle;
                    closest = hit;
                }
            }

            if (closest != null)
            {
                // 크리티컬 판정
                bool isCritical = Random.value < CriticalChance;
                float damage = isCritical ? BaseDamage * CriticalMultiplier : BaseDamage;

                ApplyDamage(closest.gameObject, damage, isCritical);
            }

            // 디버그 시각화
            Debug.DrawRay(caster.transform.position, direction * AttackRange, Color.green, 0.2f);
        }

        /// <summary>
        /// 대미지 적용
        /// </summary>
        private void ApplyDamage(GameObject target, float damage, bool isCritical)
        {
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, DamageType.Physical);

                string critText = isCritical ? " [크리티컬!]" : "";
                Debug.Log($"[DaggerStrike] {target.name}에게 {damage} 대미지{critText}");
            }
        }
    }
}
