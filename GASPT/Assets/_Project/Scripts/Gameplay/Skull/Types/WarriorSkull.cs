using UnityEngine;
using Skull.Core;
using System.Threading;

namespace Skull.Types
{
    /// <summary>
    /// 전사 스컬
    /// 강력한 근접 공격과 높은 체력을 가진 스컬
    /// </summary>
    public class WarriorSkull : BaseSkull
    {
        [Header("전사 스컬 설정")]
        [SerializeField] private float meleeRange = 2f;
        [SerializeField] private LayerMask enemyLayerMask = -1;
        [SerializeField] private float chargeSpeed = 25f;
        [SerializeField] private float chargeDistance = 8f;

        [Header("공격 설정")]
        [SerializeField] private Transform weaponPoint;
        [SerializeField] private float heavyAttackDamage = 20f;
        [SerializeField] private float chargeAttackDamage = 15f;
        [SerializeField] private float slamDamage = 35f;

        [Header("방어 설정")]
        [SerializeField] private float blockDuration = 2f;
        [SerializeField] private float blockCooldown = 5f;
        [SerializeField] private float damageReduction = 0.7f;

        [Header("이펙트")]
        [SerializeField] private GameObject slashEffect;
        [SerializeField] private GameObject chargeEffect;
        [SerializeField] private GameObject slamEffect;
        [SerializeField] private GameObject blockEffect;

        // 전사 상태
        private bool isCharging = false;
        private bool isBlocking = false;
        private float blockStartTime = 0f;
        private float lastBlockTime = 0f;
        private Vector2 chargeDirection;
        private float chargeStartTime;

        // 콤보 시스템
        private int heavyComboCount = 0;
        private float lastHeavyAttackTime = 0f;
        private readonly float heavyComboWindow = 2f;

        #region 초기화

        protected override void InitializeSkull()
        {
            base.InitializeSkull();

            // 무기 포인트 설정
            if (weaponPoint == null)
            {
                var weaponPointObj = new GameObject("WeaponPoint");
                weaponPointObj.transform.SetParent(transform);
                weaponPointObj.transform.localPosition = new Vector3(1.2f, 0f, 0f);
                weaponPoint = weaponPointObj.transform;
            }

            LogDebug("전사 스컬 초기화 완료");
        }

        #endregion

        #region 기본 공격 (강력한 근접 공격)

        protected override async Awaitable ExecutePrimaryAttack(CancellationToken cancellationToken)
        {
            LogDebug("전사 기본 공격 실행");

            if (isCharging || isBlocking) return;

            // 공격 애니메이션
            PlayAttackAnimation("HeavyAttack");

            // 공격 준비 시간
            await Awaitable.WaitForSecondsAsync(0.2f, cancellationToken);

            // 강력한 근접 공격 수행
            PerformHeavyMeleeAttack();

            // 콤보 관리
            UpdateHeavyCombo();

            LogDebug("전사 기본 공격 완료");
        }

        /// <summary>
        /// 강력한 근접 공격 수행
        /// </summary>
        private void PerformHeavyMeleeAttack()
        {
            if (weaponPoint == null) return;

            // 넓은 공격 범위
            var colliders = Physics2D.OverlapCircleAll(weaponPoint.position, meleeRange, enemyLayerMask);

            // 공격 이펙트 생성
            CreateSlashEffect();

            foreach (var collider in colliders)
            {
                var enemy = collider.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    float damage = CalculateHeavyDamage();
                    enemy.TakeDamage(damage);

                    // 넉백 효과 (향후 구현)
                    ApplyKnockback(collider.transform);

                    LogDebug($"전사 공격 데미지: {damage}");
                }
            }
        }

        /// <summary>
        /// 강공격 데미지 계산
        /// </summary>
        private float CalculateHeavyDamage()
        {
            float baseDamage = skullData?.BaseStats?.AttackDamage ?? heavyAttackDamage;

            // 콤보에 따른 데미지 증가
            float comboMultiplier = 1f + (heavyComboCount * 0.3f);

            return baseDamage * comboMultiplier;
        }

        /// <summary>
        /// 강공격 콤보 업데이트
        /// </summary>
        private void UpdateHeavyCombo()
        {
            if (Time.time - lastHeavyAttackTime <= heavyComboWindow)
            {
                heavyComboCount = Mathf.Min(heavyComboCount + 1, 4); // 최대 4콤보
            }
            else
            {
                heavyComboCount = 1;
            }

            lastHeavyAttackTime = Time.time;
        }

        /// <summary>
        /// 슬래시 이펙트 생성
        /// </summary>
        private void CreateSlashEffect()
        {
            if (slashEffect != null && weaponPoint != null)
            {
                var effect = Instantiate(slashEffect, weaponPoint.position, weaponPoint.rotation);
                Destroy(effect, 1f);
            }
        }

        #endregion

        #region 보조 공격 (돌진 공격)

        protected override async Awaitable ExecuteSecondaryAttack(CancellationToken cancellationToken)
        {
            LogDebug("전사 보조 공격 실행 - 돌진 공격");

            if (isCharging || isBlocking) return;

            // 마나 소모
            if (abilitySystem != null)
            {
                abilitySystem.ConsumeResource("Mana", GetSecondaryManaCost());
            }

            // 돌진 방향 계산
            chargeDirection = GetChargeDirection();

            // 돌진 실행
            await PerformChargeAttack(cancellationToken);

            LogDebug("전사 보조 공격 완료");
        }

        /// <summary>
        /// 돌진 공격 수행
        /// </summary>
        private async Awaitable PerformChargeAttack(CancellationToken cancellationToken)
        {
            isCharging = true;
            chargeStartTime = Time.time;

            try
            {
                // 돌진 애니메이션
                PlayAttackAnimation("Charge");

                // 돌진 이펙트 생성
                CreateChargeEffect();

                // 돌진 이동
                await PerformChargeDash(cancellationToken);

                // 돌진 종료 공격
                PerformChargeFinisher();
            }
            finally
            {
                isCharging = false;
                DestroyChargeEffect();
            }
        }

        /// <summary>
        /// 돌진 대시 수행
        /// </summary>
        private async Awaitable PerformChargeDash(CancellationToken cancellationToken)
        {
            Vector3 startPos = playerTransform.position;
            Vector3 targetPos = startPos + (Vector3)chargeDirection * chargeDistance;
            float chargeDuration = chargeDistance / chargeSpeed;

            float elapsed = 0f;

            while (elapsed < chargeDuration && !cancellationToken.IsCancellationRequested)
            {
                float progress = elapsed / chargeDuration;
                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);

                // 장애물 체크
                if (CheckChargeCollision(currentPos))
                {
                    break;
                }

                playerTransform.position = currentPos;

                // 돌진 중 적 데미지 체크
                CheckChargeDamage();

                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(cancellationToken);
            }
        }

        /// <summary>
        /// 돌진 충돌 체크
        /// </summary>
        private bool CheckChargeCollision(Vector3 position)
        {
            // 간단한 벽 충돌 체크
            RaycastHit2D hit = Physics2D.Raycast(position, chargeDirection, 0.5f, LayerMask.GetMask("Ground"));
            return hit.collider != null;
        }

        /// <summary>
        /// 돌진 중 데미지 체크
        /// </summary>
        private void CheckChargeDamage()
        {
            var colliders = Physics2D.OverlapCircleAll(playerTransform.position, 1f, enemyLayerMask);

            foreach (var collider in colliders)
            {
                var enemy = collider.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    enemy.TakeDamage(chargeAttackDamage);
                    ApplyKnockback(collider.transform);
                    LogDebug($"돌진 공격 데미지: {chargeAttackDamage}");
                }
            }
        }

        /// <summary>
        /// 돌진 마무리 공격
        /// </summary>
        private void PerformChargeFinisher()
        {
            LogDebug("돌진 마무리 공격");

            // 마무리 공격 범위
            var colliders = Physics2D.OverlapCircleAll(playerTransform.position, meleeRange * 1.5f, enemyLayerMask);

            foreach (var collider in colliders)
            {
                var enemy = collider.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    float finisherDamage = chargeAttackDamage * 1.5f;
                    enemy.TakeDamage(finisherDamage);
                    ApplyKnockback(collider.transform);
                    LogDebug($"돌진 마무리 데미지: {finisherDamage}");
                }
            }

            CreateSlashEffect();
        }

        /// <summary>
        /// 돌진 방향 계산
        /// </summary>
        private Vector2 GetChargeDirection()
        {
            // 마우스 방향 또는 이동 입력 방향
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            Vector2 direction = (mousePos - playerTransform.position).normalized;
            return direction.magnitude > 0.1f ? direction : Vector2.right;
        }

        #endregion

        #region 궁극기 (지면 강타)

        protected override async Awaitable ExecuteUltimate(CancellationToken cancellationToken)
        {
            LogDebug("전사 궁극기 실행 - 지면 강타");

            if (isCharging || isBlocking) return;

            // 마나 소모
            if (abilitySystem != null)
            {
                abilitySystem.ConsumeResource("Mana", GetUltimateManaCost());
            }

            // 궁극기 실행
            await PerformGroundSlam(cancellationToken);

            LogDebug("전사 궁극기 완료");
        }

        /// <summary>
        /// 지면 강타 수행
        /// </summary>
        private async Awaitable PerformGroundSlam(CancellationToken cancellationToken)
        {
            // 준비 애니메이션
            PlayAttackAnimation("SlamPrep");

            // 준비 시간
            await Awaitable.WaitForSecondsAsync(1f, cancellationToken);

            // 강타 애니메이션
            PlayAttackAnimation("SlamExecute");

            // 강타 이펙트
            CreateSlamEffect();

            // 광범위 데미지
            PerformSlamDamage();
        }

        /// <summary>
        /// 지면 강타 데미지
        /// </summary>
        private void PerformSlamDamage()
        {
            float slamRadius = meleeRange * 3f;
            var colliders = Physics2D.OverlapCircleAll(playerTransform.position, slamRadius, enemyLayerMask);

            foreach (var collider in colliders)
            {
                var enemy = collider.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    // 거리에 따른 데미지 감소
                    float distance = Vector2.Distance(playerTransform.position, collider.transform.position);
                    float damageMultiplier = Mathf.Lerp(1f, 0.3f, distance / slamRadius);
                    float finalDamage = slamDamage * damageMultiplier;

                    enemy.TakeDamage(finalDamage);
                    ApplyKnockback(collider.transform);
                    LogDebug($"지면 강타 데미지: {finalDamage}");
                }
            }
        }

        /// <summary>
        /// 지면 강타 이펙트 생성
        /// </summary>
        private void CreateSlamEffect()
        {
            if (slamEffect != null)
            {
                var effect = Instantiate(slamEffect, playerTransform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }

        #endregion

        #region 스컬 던지기 (방패 던지기)

        protected override async Awaitable ExecuteSkullThrow(Vector2 direction, CancellationToken cancellationToken)
        {
            LogDebug($"전사 방패 던지기 실행 - 방향: {direction}");

            // 방패 던지기 애니메이션
            PlayAttackAnimation("ShieldThrow");

            // 방패 투사체 생성 및 이동
            await CreateAndThrowShield(direction, cancellationToken);

            LogDebug("전사 방패 던지기 완료");
        }

        /// <summary>
        /// 방패 투사체 생성 및 투척
        /// </summary>
        private async Awaitable CreateAndThrowShield(Vector2 direction, CancellationToken cancellationToken)
        {
            // 방패 투사체 생성 (임시)
            var shield = new GameObject("WarriorShield");
            shield.transform.position = playerTransform.position;

            // 방패 렌더러 추가
            var renderer = shield.AddComponent<SpriteRenderer>();
            if (playerRenderer != null)
            {
                renderer.sprite = playerRenderer.sprite;
                renderer.color = new Color(0.8f, 0.8f, 1f, 0.8f);
            }

            // 방패 이동
            await MoveShieldProjectile(shield, direction, cancellationToken);

            // 방패 제거
            if (shield != null)
            {
                Destroy(shield);
            }
        }

        /// <summary>
        /// 방패 투사체 이동
        /// </summary>
        private async Awaitable MoveShieldProjectile(GameObject shield, Vector2 direction, CancellationToken cancellationToken)
        {
            if (shield == null) return;

            float shieldSpeed = 18f;
            float maxDistance = 12f;
            float traveledDistance = 0f;

            while (shield != null && traveledDistance < maxDistance && !cancellationToken.IsCancellationRequested)
            {
                float moveDistance = shieldSpeed * Time.deltaTime;
                shield.transform.Translate(direction * moveDistance, Space.World);

                // 회전 효과
                shield.transform.Rotate(0, 0, 720f * Time.deltaTime);

                traveledDistance += moveDistance;

                // 충돌 검사
                if (CheckShieldCollision(shield))
                {
                    break;
                }

                await Awaitable.NextFrameAsync(cancellationToken);
            }
        }

        /// <summary>
        /// 방패 충돌 검사
        /// </summary>
        private bool CheckShieldCollision(GameObject shield)
        {
            if (shield == null) return false;

            var collider = Physics2D.OverlapCircle(shield.transform.position, 0.5f, enemyLayerMask);
            if (collider != null)
            {
                var enemy = collider.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    float shieldDamage = heavyAttackDamage * 1.2f;
                    enemy.TakeDamage(shieldDamage);
                    ApplyKnockback(collider.transform);
                    LogDebug($"방패 투사체 데미지: {shieldDamage}");
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 방어 시스템 (향후 확장)

        /// <summary>
        /// 방어 시작
        /// </summary>
        public async Awaitable StartBlocking(CancellationToken cancellationToken = default)
        {
            if (isCharging || isBlocking || Time.time < lastBlockTime + blockCooldown) return;

            LogDebug("방어 시작");

            isBlocking = true;
            blockStartTime = Time.time;

            // 방어 애니메이션
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsBlocking", true);
            }

            // 방어 이펙트
            CreateBlockEffect();

            // 방어 지속
            await Awaitable.WaitForSecondsAsync(blockDuration, cancellationToken);

            StopBlocking();
        }

        /// <summary>
        /// 방어 종료
        /// </summary>
        public void StopBlocking()
        {
            if (!isBlocking) return;

            LogDebug("방어 종료");

            isBlocking = false;
            lastBlockTime = Time.time;

            // 방어 애니메이션 종료
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsBlocking", false);
            }

            DestroyBlockEffect();
        }

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 공격 애니메이션 재생
        /// </summary>
        private void PlayAttackAnimation(string triggerName)
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger(triggerName);
            }
        }

        /// <summary>
        /// 넉백 효과 적용
        /// </summary>
        private void ApplyKnockback(Transform target)
        {
            if (target == null) return;

            Vector2 knockDirection = (target.position - playerTransform.position).normalized;
            float knockForce = 5f;

            // 간단한 넉백 (Rigidbody2D가 있는 경우)
            var rb = target.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(knockDirection * knockForce, ForceMode2D.Impulse);
            }
        }

        /// <summary>
        /// 돌진 이펙트 생성
        /// </summary>
        private void CreateChargeEffect()
        {
            if (chargeEffect != null)
            {
                var effect = Instantiate(chargeEffect, playerTransform);
                effect.transform.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// 돌진 이펙트 제거
        /// </summary>
        private void DestroyChargeEffect()
        {
            var effect = GetComponentInChildren<ParticleSystem>();
            if (effect != null)
            {
                Destroy(effect.gameObject);
            }
        }

        /// <summary>
        /// 방어 이펙트 생성
        /// </summary>
        private void CreateBlockEffect()
        {
            if (blockEffect != null)
            {
                var effect = Instantiate(blockEffect, playerTransform);
                effect.transform.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// 방어 이펙트 제거
        /// </summary>
        private void DestroyBlockEffect()
        {
            // 방어 이펙트 제거 로직
        }

        #endregion

        #region 업데이트

        public override void OnUpdate()
        {
            base.OnUpdate();

            // 콤보 타이머 체크
            if (Time.time - lastHeavyAttackTime > heavyComboWindow)
            {
                heavyComboCount = 0;
            }

            // 방어 입력 체크 (향후 입력 시스템과 연동)
            if (Input.GetMouseButton(1) && !isBlocking) // 우클릭으로 방어
            {
                _ = StartBlocking();
            }
            else if (Input.GetMouseButtonUp(1) && isBlocking)
            {
                StopBlocking();
            }
        }

        #endregion

        #region 쿨다운 및 비용 오버라이드

        protected override float GetPrimaryCooldown() => 1.2f; // 전사는 느린 기본 공격
        protected override float GetSecondaryCooldown() => 6f;
        protected override float GetUltimateCooldown() => 18f;
        protected override float GetSecondaryManaCost() => 30f;
        protected override float GetUltimateManaCost() => 70f;

        #endregion

        #region 기즈모

        private void OnDrawGizmosSelected()
        {
            if (weaponPoint != null)
            {
                // 근접 공격 범위
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(weaponPoint.position, meleeRange);
            }

            if (playerTransform != null)
            {
                // 돌진 거리 표시
                Gizmos.color = Color.yellow;
                if (isCharging)
                {
                    Gizmos.DrawLine(playerTransform.position,
                        playerTransform.position + (Vector3)chargeDirection * chargeDistance);
                }

                // 지면 강타 범위
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(playerTransform.position, meleeRange * 3f);
            }
        }

        #endregion
    }
}