using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core;
using GASPT.CameraSystem;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 검 베기 - 전사 기본 공격
    /// 전방 짧은 범위에 빠른 근접 공격
    /// </summary>
    public class SwordSlashAbility : BaseAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "SwordSlash";
        public override float Cooldown => 0.3f;  // 0.3초 쿨다운 (빠른 공격)


        // ====== 스킬 설정 ======

        private const float AttackRange = 2f;       // 공격 범위
        private const float AttackAngle = 90f;      // 공격 각도 (전방 90도)
        private const float BaseDamage = 25f;       // 기본 대미지
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
            PerformSlash(caster, direction);

            // 쿨다운 시작
            StartCooldown();

            // 공격 애니메이션 대기
            await Awaitable.WaitForSecondsAsync(0.15f, token);
        }

        /// <summary>
        /// 검 베기 실행
        /// </summary>
        private void PerformSlash(GameObject caster, Vector2 direction)
        {
            // 범위 내 적 탐색
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                caster.transform.position,
                AttackRange,
                LayerMask.GetMask(EnemyLayerName)
            );

            int hitCount = 0;

            foreach (var hit in hits)
            {
                // 방향 체크 (전방 각도 내에 있는지)
                Vector2 toTarget = (hit.transform.position - caster.transform.position).normalized;
                float angle = Vector2.Angle(direction, toTarget);

                if (angle <= AttackAngle * 0.5f)
                {
                    // 대미지 적용
                    ApplyDamage(hit.gameObject);
                    hitCount++;
                }
            }

            // 디버그 시각화
            Debug.DrawRay(caster.transform.position, direction * AttackRange, Color.red, 0.3f);
            Debug.Log($"[SwordSlash] 검 베기! 방향: {direction}, 적중: {hitCount}명");
        }

        /// <summary>
        /// 대미지 적용
        /// </summary>
        private void ApplyDamage(GameObject target)
        {
            // IDamageable 인터페이스 체크
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(BaseDamage, DamageType.Physical);
                Debug.Log($"[SwordSlash] {target.name}에게 {BaseDamage} 물리 대미지!");
            }
            else
            {
                Debug.Log($"[SwordSlash] {target.name} 적중 (IDamageable 없음)");
            }
        }
    }
}
