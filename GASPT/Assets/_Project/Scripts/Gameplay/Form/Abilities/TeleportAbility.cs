using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 순간이동 - 마법사 스킬 1
    /// 마우스 방향으로 짧은 거리 텔레포트
    /// </summary>
    public class TeleportAbility : BaseProjectileAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "Teleport";
        public override float Cooldown => 3f;  // 3초 쿨다운


        // ====== 스킬 설정 ======

        private const float TeleportDistance = 5f;  // 텔레포트 거리


        // ====== 실행 ======

        public override async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (!CheckCooldown())
            {
                return;
            }

            // 마우스 방향 계산
            Vector2 direction = GetMouseDirection(caster);

            // 텔레포트 위치 계산
            Vector3 teleportPos = caster.transform.position + (Vector3)(direction * TeleportDistance);

            // 텔레포트 실행
            PerformTeleport(caster, teleportPos);

            // 쿨다운 시작
            StartCooldown();

            Debug.Log($"[Teleport] 순간이동! {caster.transform.position} → {teleportPos}");

            // 비동기 대기 (이펙트 재생 등)
            await Awaitable.WaitForSecondsAsync(0.1f, token);
        }

        /// <summary>
        /// 텔레포트 실행
        /// TODO: 장애물 체크, 안전한 위치 확인
        /// </summary>
        private void PerformTeleport(GameObject caster, Vector3 targetPos)
        {
            // 임시 구현 - 즉시 이동
            caster.transform.position = targetPos;

            // TODO: 텔레포트 이펙트
            // - 시작 위치 이펙트
            // - 끝 위치 이펙트
            // - 무적 프레임 (0.1초)

            // 임시 디버그 시각화
            Debug.DrawLine(caster.transform.position, targetPos, Color.magenta, 1f);

            Debug.Log($"[Teleport] 텔레포트 완료 - 거리: {TeleportDistance}m");
        }
    }
}
