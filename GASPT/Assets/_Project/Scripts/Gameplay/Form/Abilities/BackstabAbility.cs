using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core;
using GASPT.Core.Enums;
using GASPT.CameraSystem;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 백스탭 - 암살자 스킬 2
    /// 적의 뒤로 순간이동 후 크리티컬 공격
    /// </summary>
    public class BackstabAbility : BaseAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "Backstab";
        public override float Cooldown => 8f;  // 8초 쿨다운


        // ====== 스킬 설정 ======

        private const float TargetRange = 6f;           // 타겟 선택 범위
        private const float BackstabDamage = 80f;       // 백스탭 대미지
        private const float BackstabMultiplier = 1.5f;  // 후방 공격 배율
        private const float BackDistance = 1.5f;        // 적 뒤 거리
        private const string EnemyLayerName = "Enemy";


        // ====== 실행 ======

        public override async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (!CheckCooldown())
            {
                return;
            }

            // 마우스 방향의 적 탐색
            GameObject target = FindTarget(caster);

            if (target == null)
            {
                Debug.Log("[Backstab] 범위 내 적 없음");
                return;
            }

            // 백스탭 실행
            await PerformBackstab(caster, target, token);

            // 쿨다운 시작
            StartCooldown();
        }

        /// <summary>
        /// 타겟 찾기
        /// </summary>
        private GameObject FindTarget(GameObject caster)
        {
            var mainCamera = CameraManager.Instance?.MainCamera;
            if (mainCamera == null) return null;

            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 direction = (mousePos - caster.transform.position).normalized;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                caster.transform.position,
                TargetRange,
                LayerMask.GetMask(EnemyLayerName)
            );

            Collider2D closest = null;
            float closestAngle = float.MaxValue;

            foreach (var hit in hits)
            {
                Vector2 toTarget = (hit.transform.position - caster.transform.position).normalized;
                float angle = Vector2.Angle(direction, toTarget);

                if (angle < closestAngle && angle < 45f) // 45도 내의 적
                {
                    closestAngle = angle;
                    closest = hit;
                }
            }

            return closest?.gameObject;
        }

        /// <summary>
        /// 백스탭 실행
        /// </summary>
        private async Task PerformBackstab(GameObject caster, GameObject target, CancellationToken token)
        {
            Vector3 startPos = caster.transform.position;

            // 적의 뒤쪽 위치 계산
            Vector2 targetFacing = GetTargetFacing(target);
            Vector3 backPos = target.transform.position - (Vector3)(targetFacing * BackDistance);

            Debug.Log($"[Backstab] 백스탭! {startPos} → {backPos} (적: {target.name})");

            // 1. 페이드아웃
            await Awaitable.WaitForSecondsAsync(0.1f, token);

            // 2. 적 뒤로 순간이동
            caster.transform.position = backPos;

            // 3. 후방 공격 (높은 대미지)
            float finalDamage = BackstabDamage * BackstabMultiplier;

            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(finalDamage, DamageType.Physical);
                Debug.Log($"[Backstab] {target.name}에게 {finalDamage} 백스탭 대미지!");
            }

            // 4. 공격 후 딜레이
            await Awaitable.WaitForSecondsAsync(0.2f, token);

            // 디버그 시각화
            Debug.DrawLine(startPos, backPos, Color.red, 1f);
            Debug.DrawLine(backPos, target.transform.position, Color.yellow, 1f);
        }

        /// <summary>
        /// 적의 facing 방향 계산
        /// </summary>
        private Vector2 GetTargetFacing(GameObject target)
        {
            // SpriteRenderer의 flipX 또는 localScale.x로 판단
            var spriteRenderer = target.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                return spriteRenderer.flipX ? Vector2.left : Vector2.right;
            }

            // 대체: Transform scale 기반
            return target.transform.localScale.x < 0 ? Vector2.left : Vector2.right;
        }
    }
}
