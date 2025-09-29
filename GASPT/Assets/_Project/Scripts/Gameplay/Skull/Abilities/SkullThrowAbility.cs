using UnityEngine;
using GAS.Core;
using System.Threading;
using static UnityEngine.Object;


namespace Skull.Abilities
{
    /// <summary>
    /// 스컬 던지기 + 텔레포트 어빌리티
    /// 모든 스컬이 공통으로 사용하는 기본 어빌리티
    /// </summary>
    [CreateAssetMenu(fileName = "Skull Throw Ability", menuName = "GAS/Abilities/Skull/Skull Throw")]
    public class SkullThrowAbility : Ability
    {
        [Header("투사체 설정")]
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private float maxThrowDistance = 12f;
        [SerializeField] private float projectileLifetime = 3f;

        [Header("텔레포트 설정")]
        [SerializeField] private float teleportDelay = 0.3f;
        [SerializeField] private bool teleportOnImpact = true;
        [SerializeField] private bool teleportOnMaxDistance = true;

        [Header("이펙트")]
        [SerializeField] private GameObject throwEffect;
        [SerializeField] private GameObject teleportEffect;
        [SerializeField] private GameObject projectileTrail;

        [Header("사운드")]
        [SerializeField] private AudioClip throwSound;
        [SerializeField] private AudioClip teleportSound;

        // 상태 관리
        private GameObject activeProjectile;
        private Vector3 throwStartPosition;
        private Vector3 teleportTarget;
        private bool isTeleportScheduled = false;

        public override bool CanExecute()
        {
            return base.CanExecute() &&
                   activeProjectile == null; // 이미 투사체가 활성화되어 있으면 실행 불가
        }

        protected override async Awaitable ExecuteAbilityEffect(CancellationToken cancellationToken)
        {
            // owner는 이미 베이스 클래스의 멤버
            if (owner == null) return;

            // 던지기 방향 계산
            Vector2 throwDirection = GetThrowDirection(owner.transform.position);

            LogDebug($"스컬 던지기 실행: 방향={throwDirection}");

            // 투사체 생성 및 발사
            await CreateAndLaunchProjectile(owner.transform, throwDirection, cancellationToken);
        }

        /// <summary>
        /// 던지기 방향 계산
        /// </summary>
        private Vector2 GetThrowDirection(Vector3 ownerPosition)
        {
            // 마우스 방향으로 계산
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            Vector2 direction = (mousePos - ownerPosition).normalized;
            return direction.magnitude > 0.1f ? direction : Vector2.right;
        }

        /// <summary>
        /// 투사체 생성 및 발사
        /// </summary>
        private async Awaitable CreateAndLaunchProjectile(Transform owner, Vector2 direction, CancellationToken cancellationToken)
        {
            throwStartPosition = owner.position;

            // 투사체 생성 (임시로 빈 게임오브젝트)
            activeProjectile = new GameObject("SkullProjectile");
            activeProjectile.transform.position = throwStartPosition;

            // 투사체 시각화 (임시)
            var renderer = activeProjectile.AddComponent<SpriteRenderer>();
            var ownerRenderer = owner.GetComponent<SpriteRenderer>();
            if (ownerRenderer != null)
            {
                renderer.sprite = ownerRenderer.sprite;
                renderer.color = new Color(1f, 1f, 1f, 0.7f); // 반투명
                renderer.sortingOrder = ownerRenderer.sortingOrder + 1;
            }

            // 투사체 회전 설정
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            activeProjectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // 던지기 이펙트 생성
            CreateThrowEffect(throwStartPosition);

            // 던지기 사운드 재생
            PlayThrowSound(throwStartPosition);

            // 투사체 이동 시작
            await MoveProjectile(owner, direction, cancellationToken);
        }

        /// <summary>
        /// 투사체 이동 처리
        /// </summary>
        private async Awaitable MoveProjectile(Transform owner, Vector2 direction, CancellationToken cancellationToken)
        {
            if (activeProjectile == null) return;

            float traveledDistance = 0f;
            float elapsedTime = 0f;

            while (activeProjectile != null &&
                   traveledDistance < maxThrowDistance &&
                   elapsedTime < projectileLifetime &&
                   !cancellationToken.IsCancellationRequested)
            {
                float moveDistance = projectileSpeed * Time.deltaTime;
                activeProjectile.transform.Translate(Vector3.right * moveDistance);

                traveledDistance += moveDistance;
                elapsedTime += Time.deltaTime;

                // 충돌 검사
                if (CheckProjectileCollision())
                {
                    teleportTarget = activeProjectile.transform.position;
                    if (teleportOnImpact)
                    {
                        await ScheduleTeleport(owner, cancellationToken);
                    }
                    break;
                }

                await Awaitable.NextFrameAsync(cancellationToken);
            }

            // 최대 거리 도달 시
            if (activeProjectile != null)
            {
                teleportTarget = activeProjectile.transform.position;
                if (teleportOnMaxDistance)
                {
                    await ScheduleTeleport(owner, cancellationToken);
                }
            }

            // 투사체 정리
            CleanupProjectile();
        }

        /// <summary>
        /// 투사체 충돌 검사
        /// </summary>
        private bool CheckProjectileCollision()
        {
            if (activeProjectile == null) return false;

            // 간단한 벽 충돌 체크
            RaycastHit2D hit = Physics2D.Raycast(
                activeProjectile.transform.position,
                activeProjectile.transform.right,
                0.5f,
                LayerMask.GetMask("Ground")
            );

            return hit.collider != null;
        }

        /// <summary>
        /// 텔레포트 예약
        /// </summary>
        private async Awaitable ScheduleTeleport(Transform owner, CancellationToken cancellationToken)
        {
            if (isTeleportScheduled) return;

            isTeleportScheduled = true;

            LogDebug($"텔레포트 예약: 목표={teleportTarget}, 지연={teleportDelay}초");

            // 텔레포트 지연
            await Awaitable.WaitForSecondsAsync(teleportDelay, cancellationToken);

            // 텔레포트 실행
            await ExecuteTeleport(owner, cancellationToken);
        }

        /// <summary>
        /// 텔레포트 실행
        /// </summary>
        private async Awaitable ExecuteTeleport(Transform owner, CancellationToken cancellationToken)
        {
            if (owner == null) return;

            LogDebug($"텔레포트 실행: {owner.position} → {teleportTarget}");

            // 출발지 텔레포트 이펙트
            CreateTeleportEffect(owner.position);

            // 텔레포트 사운드 재생
            PlayTeleportSound(owner.position);

            // 짧은 지연 (시각적 효과)
            await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken);

            // 위치 이동
            owner.position = teleportTarget;

            // 도착지 텔레포트 이펙트
            CreateTeleportEffect(teleportTarget);

            // 텔레포트 사운드 재생
            PlayTeleportSound(teleportTarget);

            LogDebug($"텔레포트 완료: 새 위치={owner.position}");

            isTeleportScheduled = false;
        }

        /// <summary>
        /// 던지기 이펙트 생성
        /// </summary>
        private void CreateThrowEffect(Vector3 position)
        {
            if (throwEffect != null)
            {
                var effect = Instantiate(throwEffect, position, Quaternion.identity);
                Destroy(effect, 2f);
                LogDebug("던지기 이펙트 생성");
            }
        }

        /// <summary>
        /// 텔레포트 이펙트 생성
        /// </summary>
        private void CreateTeleportEffect(Vector3 position)
        {
            if (teleportEffect != null)
            {
                var effect = Instantiate(teleportEffect, position, Quaternion.identity);
                Destroy(effect, 2f);
                LogDebug($"텔레포트 이펙트 생성: {position}");
            }
        }

        /// <summary>
        /// 던지기 사운드 재생
        /// </summary>
        private void PlayThrowSound(Vector3 position)
        {
            if (throwSound != null)
            {
                // AudioSource.PlayClipAtPoint(throwSound, position);
                LogDebug("던지기 사운드 재생");
            }
        }

        /// <summary>
        /// 텔레포트 사운드 재생
        /// </summary>
        private void PlayTeleportSound(Vector3 position)
        {
            if (teleportSound != null)
            {
                // AudioSource.PlayClipAtPoint(teleportSound, position);
                LogDebug("텔레포트 사운드 재생");
            }
        }

        /// <summary>
        /// 투사체 정리
        /// </summary>
        private void CleanupProjectile()
        {
            if (activeProjectile != null)
            {
                Destroy(activeProjectile);
                activeProjectile = null;
                LogDebug("투사체 정리 완료");
            }
        }

        /// <summary>
        /// 어빌리티 중단 시 호출 (Cancel 메서드에서 호출됨)
        /// </summary>
        public override void Cancel()
        {
            LogDebug("스컬 던지기 어빌리티 취소");

            // 진행 중인 투사체 정리
            CleanupProjectile();

            // 예약된 텔레포트 취소
            isTeleportScheduled = false;

            // 베이스 클래스 Cancel 호출
            base.Cancel();
        }

        /// <summary>
        /// 디버그 로그
        /// </summary>
        private void LogDebug(string message)
        {
            #if UNITY_EDITOR
            Debug.Log($"[SkullThrowAbility] {message}");
            #endif
        }

        /// <summary>
        /// 에디터에서 기즈모 그리기
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (activeProjectile != null)
            {
                // 투사체 경로 표시
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(throwStartPosition, activeProjectile.transform.position);

                // 최대 거리 표시
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(throwStartPosition, maxThrowDistance);

                // 텔레포트 목표 표시
                if (isTeleportScheduled)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(teleportTarget, 0.5f);
                }
            }
        }
    }
}
