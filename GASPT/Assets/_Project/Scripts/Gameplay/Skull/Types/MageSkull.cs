using UnityEngine;
using Skull.Core;
using System.Threading;

namespace Skull.Types
{
    /// <summary>
    /// 마법사 스컬
    /// 원거리 마법 공격과 높은 마나를 가진 스컬
    /// </summary>
    public class MageSkull : BaseSkull
    {
        [Header("마법사 스컬 설정")]
        [SerializeField] private float castRange = 8f;
        [SerializeField] private LayerMask enemyLayerMask = -1;
        [SerializeField] private float projectileSpeed = 15f;
        [SerializeField] private float teleportDistance = 12f;

        [Header("마법 투사체")]
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private GameObject iceBoltPrefab;
        [SerializeField] private GameObject meteorPrefab;
        [SerializeField] private Transform castPoint;

        [Header("마법 이펙트")]
        [SerializeField] private GameObject castEffect;
        [SerializeField] private GameObject teleportEffect;
        [SerializeField] private GameObject auraEffect;

        [Header("마법 설정")]
        [SerializeField] private float fireballDamage = 12f;
        [SerializeField] private float iceBoltDamage = 8f;
        [SerializeField] private float meteorDamage = 30f;
        [SerializeField] private float meteorRadius = 3f;

        // 마법 상태
        private bool isCasting = false;
        private GameObject activeAura;
        private int spellRotation = 0; // 기본 공격 순환

        #region 초기화

        protected override void InitializeSkull()
        {
            base.InitializeSkull();

            // 캐스트 포인트 설정
            if (castPoint == null)
            {
                var castPointObj = new GameObject("CastPoint");
                castPointObj.transform.SetParent(transform);
                castPointObj.transform.localPosition = new Vector3(1f, 0.5f, 0f);
                castPoint = castPointObj.transform;
            }

            LogDebug("마법사 스컬 초기화 완료");
        }

        #endregion

        #region 스컬 장착/해제

        public override async Awaitable OnEquip(CancellationToken cancellationToken = default)
        {
            await base.OnEquip(cancellationToken);

            // 마법사 오라 생성
            CreateMageAura();
        }

        public override async Awaitable OnUnequip(CancellationToken cancellationToken = default)
        {
            // 오라 제거
            DestroyMageAura();

            await base.OnUnequip(cancellationToken);
        }

        /// <summary>
        /// 마법사 오라 생성
        /// </summary>
        private void CreateMageAura()
        {
            if (auraEffect != null && playerTransform != null)
            {
                activeAura = Instantiate(auraEffect, playerTransform);
                LogDebug("마법사 오라 생성");
            }
        }

        /// <summary>
        /// 마법사 오라 제거
        /// </summary>
        private void DestroyMageAura()
        {
            if (activeAura != null)
            {
                Destroy(activeAura);
                activeAura = null;
                LogDebug("마법사 오라 제거");
            }
        }

        #endregion

        #region 기본 공격 (마법 투사체)

        protected override async Awaitable ExecutePrimaryAttack(CancellationToken cancellationToken)
        {
            LogDebug("기본 마법 공격 실행");

            if (isCasting) return;

            isCasting = true;

            try
            {
                // 캐스팅 애니메이션
                PlayCastAnimation("BasicCast");

                // 캐스팅 이펙트
                await CreateCastEffect(cancellationToken);

                // 스펠 순환에 따른 다른 마법 발사
                switch (spellRotation % 2)
                {
                    case 0:
                        await CastFireball(cancellationToken);
                        break;
                    case 1:
                        await CastIceBolt(cancellationToken);
                        break;
                }

                spellRotation++;
            }
            finally
            {
                isCasting = false;
            }

            LogDebug("기본 마법 공격 완료");
        }

        /// <summary>
        /// 파이어볼 시전
        /// </summary>
        private async Awaitable CastFireball(CancellationToken cancellationToken)
        {
            LogDebug("파이어볼 시전");

            // 목표 방향 계산
            Vector2 targetDirection = GetTargetDirection();

            // 파이어볼 생성
            await CreateProjectile(fireballPrefab, targetDirection, fireballDamage, cancellationToken);
        }

        /// <summary>
        /// 아이스볼트 시전
        /// </summary>
        private async Awaitable CastIceBolt(CancellationToken cancellationToken)
        {
            LogDebug("아이스볼트 시전");

            // 목표 방향 계산
            Vector2 targetDirection = GetTargetDirection();

            // 아이스볼트 생성 (더 빠름)
            await CreateProjectile(iceBoltPrefab, targetDirection, iceBoltDamage, cancellationToken, projectileSpeed * 1.5f);
        }

        /// <summary>
        /// 마법 투사체 생성
        /// </summary>
        private async Awaitable CreateProjectile(GameObject prefab, Vector2 direction, float damage,
            CancellationToken cancellationToken, float speed = 0f)
        {
            if (prefab == null || castPoint == null) return;

            if (speed <= 0f) speed = projectileSpeed;

            // 투사체 생성
            var projectile = Instantiate(prefab, castPoint.position, Quaternion.identity);

            // 투사체 방향 설정
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // 투사체 이동 시작
            _ = MoveProjectile(projectile, direction, speed, damage, cancellationToken);

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        /// <summary>
        /// 투사체 이동 처리
        /// </summary>
        private async Awaitable MoveProjectile(GameObject projectile, Vector2 direction, float speed,
            float damage, CancellationToken cancellationToken)
        {
            if (projectile == null) return;

            float maxDistance = castRange;
            float traveledDistance = 0f;

            while (projectile != null && traveledDistance < maxDistance && !cancellationToken.IsCancellationRequested)
            {
                float moveDistance = speed * Time.deltaTime;
                projectile.transform.Translate(Vector3.right * moveDistance);

                traveledDistance += moveDistance;

                // 충돌 검사
                if (CheckProjectileCollision(projectile, damage))
                {
                    break;
                }

                await Awaitable.NextFrameAsync(cancellationToken);
            }

            // 투사체 제거
            if (projectile != null)
            {
                CreateProjectileImpact(projectile.transform.position);
                Destroy(projectile);
            }
        }

        /// <summary>
        /// 투사체 충돌 검사
        /// </summary>
        private bool CheckProjectileCollision(GameObject projectile, float damage)
        {
            if (projectile == null) return false;

            var collider = Physics2D.OverlapCircle(projectile.transform.position, 0.3f, enemyLayerMask);
            if (collider != null)
            {
                var enemy = collider.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    LogDebug($"투사체가 적에게 적중: 데미지 {damage}");
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 보조 공격 (다중 투사체)

        protected override async Awaitable ExecuteSecondaryAttack(CancellationToken cancellationToken)
        {
            LogDebug("보조 마법 공격 실행 - 다중 투사체");

            if (isCasting) return;

            // 마나 소모
            if (abilitySystem != null)
            {
                abilitySystem.ConsumeResource("Mana", GetSecondaryManaCost());
            }

            isCasting = true;

            try
            {
                // 캐스팅 애니메이션
                PlayCastAnimation("MultiCast");

                // 캐스팅 시간
                await Awaitable.WaitForSecondsAsync(0.5f, cancellationToken);

                // 3방향 투사체 발사
                await CastMultipleProjectiles(cancellationToken);
            }
            finally
            {
                isCasting = false;
            }

            LogDebug("보조 마법 공격 완료");
        }

        /// <summary>
        /// 다중 투사체 시전
        /// </summary>
        private async Awaitable CastMultipleProjectiles(CancellationToken cancellationToken)
        {
            Vector2 baseDirection = GetTargetDirection();

            // 3방향으로 투사체 발사 (-20도, 0도, +20도)
            float[] angles = { -20f, 0f, 20f };

            foreach (float angleOffset in angles)
            {
                float radians = (Mathf.Atan2(baseDirection.y, baseDirection.x) + angleOffset * Mathf.Deg2Rad);
                Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

                await CreateProjectile(fireballPrefab, direction, fireballDamage * 0.8f, cancellationToken);
                await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken); // 약간의 지연
            }
        }

        #endregion

        #region 궁극기 (메테오)

        protected override async Awaitable ExecuteUltimate(CancellationToken cancellationToken)
        {
            LogDebug("궁극기 실행 - 메테오");

            if (isCasting) return;

            // 마나 소모
            if (abilitySystem != null)
            {
                abilitySystem.ConsumeResource("Mana", GetUltimateManaCost());
            }

            isCasting = true;

            try
            {
                // 궁극기 애니메이션
                PlayCastAnimation("Ultimate");

                // 캐스팅 시간
                await Awaitable.WaitForSecondsAsync(1.5f, cancellationToken);

                // 메테오 시전
                await CastMeteor(cancellationToken);
            }
            finally
            {
                isCasting = false;
            }

            LogDebug("궁극기 완료");
        }

        /// <summary>
        /// 메테오 시전
        /// </summary>
        private async Awaitable CastMeteor(CancellationToken cancellationToken)
        {
            // 목표 지점 계산 (마우스 방향)
            Vector2 targetDirection = GetTargetDirection();
            Vector3 meteorTarget = playerTransform.position + (Vector3)(targetDirection.normalized * 6f);

            LogDebug($"메테오 목표 지점: {meteorTarget}");

            // 메테오 생성 (하늘 위에서 시작)
            Vector3 meteorStart = meteorTarget + Vector3.up * 10f;
            var meteor = Instantiate(meteorPrefab, meteorStart, Quaternion.identity);

            // 메테오 낙하
            await DropMeteor(meteor, meteorTarget, cancellationToken);
        }

        /// <summary>
        /// 메테오 낙하 처리
        /// </summary>
        private async Awaitable DropMeteor(GameObject meteor, Vector3 target, CancellationToken cancellationToken)
        {
            if (meteor == null) return;

            float fallSpeed = 15f;
            Vector3 startPos = meteor.transform.position;

            while (meteor != null && meteor.transform.position.y > target.y && !cancellationToken.IsCancellationRequested)
            {
                meteor.transform.position = Vector3.MoveTowards(
                    meteor.transform.position,
                    target,
                    fallSpeed * Time.deltaTime
                );

                await Awaitable.NextFrameAsync(cancellationToken);
            }

            // 메테오 충돌
            if (meteor != null)
            {
                PerformMeteorImpact(target);
                Destroy(meteor);
            }
        }

        /// <summary>
        /// 메테오 충돌 처리
        /// </summary>
        private void PerformMeteorImpact(Vector3 impactPoint)
        {
            LogDebug($"메테오 충돌: {impactPoint}");

            // 범위 내 모든 적에게 데미지
            var colliders = Physics2D.OverlapCircleAll(impactPoint, meteorRadius, enemyLayerMask);

            foreach (var collider in colliders)
            {
                var enemy = collider.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    // 거리에 따른 데미지 감소
                    float distance = Vector2.Distance(impactPoint, collider.transform.position);
                    float damageMultiplier = Mathf.Lerp(1f, 0.3f, distance / meteorRadius);
                    float finalDamage = meteorDamage * damageMultiplier;

                    enemy.TakeDamage(finalDamage);
                    LogDebug($"메테오 데미지: {finalDamage}");
                }
            }

            // 충돌 이펙트 생성
            CreateProjectileImpact(impactPoint);
        }

        #endregion

        #region 스컬 던지기 (순간이동)

        protected override async Awaitable ExecuteSkullThrow(Vector2 direction, CancellationToken cancellationToken)
        {
            LogDebug($"마법사 순간이동 실행 - 방향: {direction}");

            // 순간이동 애니메이션
            PlayCastAnimation("Teleport");

            // 목표 지점 계산
            Vector3 teleportTarget = playerTransform.position + (Vector3)(direction.normalized * teleportDistance);

            // 장애물 체크 (간단한 버전)
            teleportTarget = GetValidTeleportPosition(teleportTarget);

            // 텔레포트 이펙트 (출발지)
            if (teleportEffect != null)
            {
                Instantiate(teleportEffect, playerTransform.position, Quaternion.identity);
            }

            // 짧은 지연
            await Awaitable.WaitForSecondsAsync(0.2f, cancellationToken);

            // 순간이동 실행
            if (playerTransform != null)
            {
                playerTransform.position = teleportTarget;
            }

            // 텔레포트 이펙트 (도착지)
            if (teleportEffect != null)
            {
                Instantiate(teleportEffect, playerTransform.position, Quaternion.identity);
            }

            LogDebug($"순간이동 완료 - 위치: {teleportTarget}");
        }

        /// <summary>
        /// 유효한 텔레포트 위치 계산
        /// </summary>
        private Vector3 GetValidTeleportPosition(Vector3 targetPosition)
        {
            // 간단한 충돌 체크 (레이캐스트로 벽 감지)
            Vector3 direction = (targetPosition - playerTransform.position).normalized;
            float distance = Vector3.Distance(playerTransform.position, targetPosition);

            RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, direction, distance, LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                // 충돌 지점 바로 앞으로 이동
                return hit.point - (Vector2)direction * 0.5f;
            }

            return targetPosition;
        }

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 캐스팅 애니메이션 재생
        /// </summary>
        private void PlayCastAnimation(string triggerName)
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger(triggerName);
                playerAnimator.SetBool("IsCasting", true);
            }
        }

        /// <summary>
        /// 캐스팅 이펙트 생성
        /// </summary>
        private async Awaitable CreateCastEffect(CancellationToken cancellationToken)
        {
            if (castEffect != null && castPoint != null)
            {
                var effect = Instantiate(castEffect, castPoint.position, castPoint.rotation);

                await Awaitable.WaitForSecondsAsync(0.3f, cancellationToken);

                if (effect != null)
                {
                    Destroy(effect);
                }
            }
        }

        /// <summary>
        /// 투사체 충돌 이펙트 생성
        /// </summary>
        private void CreateProjectileImpact(Vector3 position)
        {
            // 임시 충돌 이펙트 (향후 이펙트 시스템과 연동)
            LogDebug($"투사체 충돌 이펙트: {position}");
        }

        /// <summary>
        /// 목표 방향 계산 (마우스 방향)
        /// </summary>
        private Vector2 GetTargetDirection()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            Vector2 direction = (mousePos - playerTransform.position).normalized;
            return direction.magnitude > 0.1f ? direction : Vector2.right; // 기본값은 오른쪽
        }

        #endregion

        #region 업데이트

        public override void OnUpdate()
        {
            base.OnUpdate();

            // 캐스팅 상태 업데이트
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsCasting", isCasting);
            }
        }

        #endregion

        #region 쿨다운 및 비용 오버라이드

        protected override float GetPrimaryCooldown() => 0.8f; // 마법사는 빠른 기본 공격
        protected override float GetSecondaryCooldown() => 5f;
        protected override float GetUltimateCooldown() => 15f;
        protected override float GetSecondaryManaCost() => 35f;
        protected override float GetUltimateManaCost() => 80f;

        #endregion

        #region 기즈모

        private void OnDrawGizmosSelected()
        {
            if (castPoint != null)
            {
                // 캐스팅 범위 표시
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(castPoint.position, castRange);
            }

            if (playerTransform != null)
            {
                // 텔레포트 범위 표시
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(playerTransform.position, teleportDistance);

                // 메테오 범위 표시
                Gizmos.color = Color.red;
                Vector2 mouseDir = GetTargetDirection();
                Vector3 meteorTarget = playerTransform.position + (Vector3)(mouseDir.normalized * 6f);
                Gizmos.DrawWireSphere(meteorTarget, meteorRadius);
            }
        }

        #endregion
    }
}