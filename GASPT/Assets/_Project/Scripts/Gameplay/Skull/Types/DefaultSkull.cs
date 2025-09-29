using UnityEngine;
using Skull.Core;
using System.Threading;

namespace Skull.Types
{
    /// <summary>
    /// 기본 뼈 전사 스컬
    /// 균형잡힌 능력치와 기본적인 공격 패턴을 가진 스컬
    /// </summary>
    public class DefaultSkull : BaseSkull
    {
        [Header("기본 스컬 설정")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private LayerMask enemyLayerMask = -1;
        [SerializeField] private float throwProjectileSpeed = 20f;
        [SerializeField] private float teleportDelay = 0.3f;

        [Header("공격 설정")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float comboWindow = 1f;

        [Header("이펙트")]
        [SerializeField] private GameObject attackEffect;
        [SerializeField] private GameObject throwEffect;
        [SerializeField] private GameObject teleportEffect;

        // 공격 상태
        private int comboCount = 0;
        private float lastComboTime = 0f;
        private bool isPerformingCombo = false;

        // 투사체 관련
        private GameObject activeProjectile;
        private Vector3 projectileTarget;

        #region 초기화

        protected override void InitializeSkull()
        {
            base.InitializeSkull();

            // 공격 포인트 설정
            if (attackPoint == null)
            {
                var attackPointObj = new GameObject("AttackPoint");
                attackPointObj.transform.SetParent(transform);
                attackPointObj.transform.localPosition = new Vector3(0.8f, 0f, 0f);
                attackPoint = attackPointObj.transform;
            }

            LogDebug("기본 스컬 초기화 완료");
        }

        #endregion

        #region 기본 공격 (근접 공격)

        protected override async Awaitable ExecutePrimaryAttack(CancellationToken cancellationToken)
        {
            LogDebug("기본 공격 실행");

            // 콤보 시스템 체크
            UpdateComboState();

            // 공격 애니메이션 재생
            PlayAttackAnimation();

            // 공격 이펙트 생성
            await CreateAttackEffect(cancellationToken);

            // 데미지 적용
            PerformMeleeAttack();

            // 콤보 카운트 증가
            IncrementCombo();

            LogDebug($"기본 공격 완료 - 콤보: {comboCount}");
        }

        /// <summary>
        /// 콤보 상태 업데이트
        /// </summary>
        private void UpdateComboState()
        {
            if (Time.time - lastComboTime > comboWindow)
            {
                comboCount = 0;
                isPerformingCombo = false;
            }
        }

        /// <summary>
        /// 공격 애니메이션 재생
        /// </summary>
        private void PlayAttackAnimation()
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Attack");
                playerAnimator.SetInteger("ComboCount", comboCount);
            }
        }

        /// <summary>
        /// 공격 이펙트 생성
        /// </summary>
        private async Awaitable CreateAttackEffect(CancellationToken cancellationToken)
        {
            if (attackEffect != null && attackPoint != null)
            {
                var effect = Instantiate(attackEffect, attackPoint.position, attackPoint.rotation);

                // 0.5초 후 이펙트 제거
                await Awaitable.WaitForSecondsAsync(0.5f, cancellationToken);

                if (effect != null)
                {
                    Destroy(effect);
                }
            }
        }

        /// <summary>
        /// 근접 공격 수행
        /// </summary>
        private void PerformMeleeAttack()
        {
            if (attackPoint == null) return;

            // 공격 범위 내 적 탐지
            var colliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayerMask);

            foreach (var collider in colliders)
            {
                var enemy = collider.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    float damage = CalculateAttackDamage();
                    enemy.TakeDamage(damage);
                    LogDebug($"적에게 데미지 적용: {damage}");
                }
            }
        }

        /// <summary>
        /// 공격 데미지 계산
        /// </summary>
        private float CalculateAttackDamage()
        {
            float baseDamage = skullData?.BaseStats?.AttackDamage ?? attackDamage;

            // 콤보에 따른 데미지 보너스
            float comboMultiplier = 1f + (comboCount * 0.2f);

            return baseDamage * comboMultiplier;
        }

        /// <summary>
        /// 콤보 카운트 증가
        /// </summary>
        private void IncrementCombo()
        {
            comboCount = Mathf.Min(comboCount + 1, 3); // 최대 3콤보
            lastComboTime = Time.time;
            isPerformingCombo = true;
        }

        #endregion

        #region 보조 공격 (강화된 근접 공격)

        protected override async Awaitable ExecuteSecondaryAttack(CancellationToken cancellationToken)
        {
            LogDebug("보조 공격 실행 - 강화된 근접 공격");

            // 마나 소모
            if (abilitySystem != null)
            {
                abilitySystem.ConsumeResource("Mana", GetSecondaryManaCost());
            }

            // 강화된 공격 애니메이션
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("HeavyAttack");
            }

            // 공격 준비 시간
            await Awaitable.WaitForSecondsAsync(0.3f, cancellationToken);

            // 강화된 데미지로 공격
            PerformHeavyAttack();

            LogDebug("보조 공격 완료");
        }

        /// <summary>
        /// 강화된 공격 수행
        /// </summary>
        private void PerformHeavyAttack()
        {
            if (attackPoint == null) return;

            // 더 넓은 공격 범위
            float heavyRange = attackRange * 1.5f;
            var colliders = Physics2D.OverlapCircleAll(attackPoint.position, heavyRange, enemyLayerMask);

            foreach (var collider in colliders)
            {
                var enemy = collider.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    // 기본 공격의 2배 데미지
                    float damage = CalculateAttackDamage() * 2f;
                    enemy.TakeDamage(damage);
                    LogDebug($"강화 공격으로 적에게 데미지 적용: {damage}");
                }
            }

            // 콤보 리셋
            comboCount = 0;
            isPerformingCombo = false;
        }

        #endregion

        #region 궁극기 (회전 공격)

        protected override async Awaitable ExecuteUltimate(CancellationToken cancellationToken)
        {
            LogDebug("궁극기 실행 - 회전 공격");

            // 마나 소모
            if (abilitySystem != null)
            {
                abilitySystem.ConsumeResource("Mana", GetUltimateManaCost());
            }

            // 궁극기 애니메이션
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Ultimate");
            }

            // 회전 공격 수행
            await PerformSpinAttack(cancellationToken);

            LogDebug("궁극기 완료");
        }

        /// <summary>
        /// 회전 공격 수행
        /// </summary>
        private async Awaitable PerformSpinAttack(CancellationToken cancellationToken)
        {
            float spinDuration = 1.5f;
            float spinSpeed = 720f; // 초당 2회전
            float attackInterval = 0.2f;

            float elapsed = 0f;
            float nextAttackTime = 0f;

            while (elapsed < spinDuration && !cancellationToken.IsCancellationRequested)
            {
                // 회전
                if (playerTransform != null)
                {
                    float rotation = spinSpeed * Time.deltaTime;
                    playerTransform.Rotate(0, 0, rotation);
                }

                // 주기적으로 공격
                if (elapsed >= nextAttackTime)
                {
                    PerformSpinDamage();
                    nextAttackTime = elapsed + attackInterval;
                }

                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            // 회전 정리
            if (playerTransform != null)
            {
                playerTransform.rotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// 회전 공격 데미지
        /// </summary>
        private void PerformSpinDamage()
        {
            // 넓은 범위의 공격
            float spinRange = attackRange * 2f;
            var colliders = Physics2D.OverlapCircleAll(playerTransform.position, spinRange, enemyLayerMask);

            foreach (var collider in colliders)
            {
                var enemy = collider.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    float damage = CalculateAttackDamage() * 1.5f;
                    enemy.TakeDamage(damage);
                }
            }
        }

        #endregion

        #region 스컬 던지기

        protected override async Awaitable ExecuteSkullThrow(Vector2 direction, CancellationToken cancellationToken)
        {
            LogDebug($"스컬 던지기 실행 - 방향: {direction}");

            // 스컬 던지기 애니메이션
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("SkullThrow");
            }

            // 투사체 생성 및 발사
            await CreateAndThrowProjectile(direction, cancellationToken);

            LogDebug("스컬 던지기 완료");
        }

        /// <summary>
        /// 투사체 생성 및 발사
        /// </summary>
        private async Awaitable CreateAndThrowProjectile(Vector2 direction, CancellationToken cancellationToken)
        {
            // 투사체 생성 (임시로 빈 게임오브젝트)
            activeProjectile = new GameObject("SkullProjectile");
            activeProjectile.transform.position = playerTransform.position;

            // 투사체에 SpriteRenderer 추가 (임시)
            var renderer = activeProjectile.AddComponent<SpriteRenderer>();
            if (playerRenderer != null)
            {
                renderer.sprite = playerRenderer.sprite;
                renderer.color = new Color(1f, 1f, 1f, 0.7f); // 반투명
            }

            // 목표 지점 계산
            float throwDistance = 10f;
            projectileTarget = (Vector3)direction.normalized * throwDistance + playerTransform.position;

            // 투사체 이동
            await MoveProjectileToTarget(cancellationToken);

            // 텔레포트 실행
            await PerformTeleport(cancellationToken);
        }

        /// <summary>
        /// 투사체를 목표 지점으로 이동
        /// </summary>
        private async Awaitable MoveProjectileToTarget(CancellationToken cancellationToken)
        {
            if (activeProjectile == null) return;

            Vector3 startPos = activeProjectile.transform.position;
            float journeyTime = Vector3.Distance(startPos, projectileTarget) / throwProjectileSpeed;
            float elapsed = 0f;

            while (elapsed < journeyTime && activeProjectile != null && !cancellationToken.IsCancellationRequested)
            {
                float progress = elapsed / journeyTime;
                activeProjectile.transform.position = Vector3.Lerp(startPos, projectileTarget, progress);

                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            // 투사체를 목표 지점에 정확히 배치
            if (activeProjectile != null)
            {
                activeProjectile.transform.position = projectileTarget;
            }
        }

        /// <summary>
        /// 텔레포트 실행
        /// </summary>
        private async Awaitable PerformTeleport(CancellationToken cancellationToken)
        {
            // 텔레포트 지연
            await Awaitable.WaitForSecondsAsync(teleportDelay, cancellationToken);

            // 텔레포트 이펙트 (출발지)
            if (teleportEffect != null)
            {
                Instantiate(teleportEffect, playerTransform.position, Quaternion.identity);
            }

            // 플레이어 위치 이동
            if (playerTransform != null && activeProjectile != null)
            {
                playerTransform.position = activeProjectile.transform.position;
            }

            // 텔레포트 이펙트 (도착지)
            if (teleportEffect != null)
            {
                Instantiate(teleportEffect, playerTransform.position, Quaternion.identity);
            }

            // 투사체 제거
            if (activeProjectile != null)
            {
                Destroy(activeProjectile);
                activeProjectile = null;
            }

            LogDebug($"텔레포트 완료 - 위치: {playerTransform.position}");
        }

        #endregion

        #region 업데이트

        public override void OnUpdate()
        {
            base.OnUpdate();

            // 콤보 타이머 체크
            UpdateComboState();
        }

        #endregion

        #region 쿨다운 및 비용 오버라이드

        protected override float GetSecondaryCooldown() => 4f;
        protected override float GetUltimateCooldown() => 12f;
        protected override float GetSecondaryManaCost() => 25f;
        protected override float GetUltimateManaCost() => 60f;

        #endregion

        #region 기즈모

        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                // 공격 범위 표시
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attackPoint.position, attackRange);

                // 강화 공격 범위 표시
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(attackPoint.position, attackRange * 1.5f);
            }

            if (playerTransform != null)
            {
                // 궁극기 범위 표시
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(playerTransform.position, attackRange * 2f);
            }
        }

        #endregion
    }

    /// <summary>
    /// 데미지를 받을 수 있는 객체 인터페이스 (임시)
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}