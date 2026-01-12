using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Projectiles;

namespace GASPT.Gameplay.Enemies
{
    /// <summary>
    /// BossEnemy 전투 패턴
    /// - Phase별 패턴 실행
    /// - Phase 1: 원거리 공격
    /// - Phase 2: 돌진 공격 + 소환
    /// - Phase 3: 범위 공격
    /// </summary>
    public partial class BossEnemy
    {
        // ====== Phase별 패턴 실행 ======

        /// <summary>
        /// 현재 Phase에 맞는 패턴 실행
        /// </summary>
        private void ExecutePhasePatterns()
        {
            if (phaseController == null) return;

            int currentPhaseIndex = phaseController.CurrentPhaseIndex;

            switch (currentPhaseIndex)
            {
                case 0: // Phase 1: 근접 공격 + 원거리 공격
                    if (CanUseRangedAttack())
                    {
                        ExecuteRangedAttack();
                    }
                    break;

                case 1: // Phase 2: 근접 + 원거리 + 돌진 + 소환
                    if (CanUseRangedAttack())
                    {
                        ExecuteRangedAttack();
                    }
                    if (CanUseChargeAttack())
                    {
                        ExecuteChargeAttack();
                    }
                    if (CanUseSummon())
                    {
                        ExecuteSummon();
                    }
                    break;

                case 2: // Phase 3: 근접 + 원거리 + 돌진 + 범위 공격
                    if (CanUseRangedAttack())
                    {
                        ExecuteRangedAttack();
                    }
                    if (CanUseChargeAttack())
                    {
                        ExecuteChargeAttack();
                    }
                    if (CanUseAreaAttack())
                    {
                        ExecuteAreaAttack();
                    }
                    break;
            }
        }


        // ====== Phase 1: 원거리 공격 ======

        private bool CanUseRangedAttack()
        {
            if (playerTransform == null) return false;
            if (Data == null) return false;
            if (Time.time - lastRangedAttackTime < Data.bossRangedCooldown) return false;
            if (!IsPlayerInDetectionRange()) return false;
            return true;
        }

        private void ExecuteRangedAttack()
        {
            if (playerTransform == null || Data == null) return;

            // 투사체 발사 방향
            Vector2 direction = (playerTransform.position - transform.position).normalized;

            // 풀에서 투사체 가져오기
            var projectile = PoolManager.Instance.Spawn<EnemyProjectile>(
                transform.position,
                Quaternion.identity
            );

            if (projectile != null)
            {
                projectile.Initialize(direction, Data.bossProjectileSpeed, Data.bossProjectileDamage);

                if (showDebugLogs)
                    Debug.Log($"[BossEnemy] 원거리 공격 발사! 방향: {direction}");
            }
            else
            {
                Debug.LogWarning("[BossEnemy] EnemyProjectile을 풀에서 가져올 수 없습니다!");
            }

            lastRangedAttackTime = Time.time;
        }


        // ====== Phase 2: 돌진 공격 ======

        private bool CanUseChargeAttack()
        {
            if (playerTransform == null) return false;
            if (Data == null) return false;
            if (isCharging) return false;
            if (Time.time - lastChargeAttackTime < Data.bossChargeCooldown) return false;
            if (!IsPlayerInDetectionRange()) return false;
            return true;
        }

        private void ExecuteChargeAttack()
        {
            if (playerTransform == null || Data == null || col == null) return;

            // 돌진 시작 위치 설정
            chargeStartPos = transform.position;

            // 플레이어 방향 계산 (수평만)
            Vector2 directionToPlayer = new Vector2(
                playerTransform.position.x - transform.position.x,
                0f
            );

            float distanceToPlayer = directionToPlayer.magnitude;

            // 돌진 거리 결정 (플레이어까지의 거리 vs 최대 돌진 거리)
            float chargeDistance = Mathf.Min(distanceToPlayer, Data.bossChargeDistance);
            chargeDirection = directionToPlayer.normalized;

            // CapsuleCollider2D 크기 가져오기
            CapsuleCollider2D capsule = col as CapsuleCollider2D;
            if (capsule == null)
            {
                Debug.LogWarning("[BossEnemy] CapsuleCollider2D가 아닙니다. 돌진 취소!");
                return;
            }

            // Collider 크기 (스케일 적용)
            Vector2 capsuleSize = capsule.size;
            Vector2 scaledSize = new Vector2(
                capsuleSize.x * Mathf.Abs(transform.localScale.x),
                capsuleSize.y * Mathf.Abs(transform.localScale.y)
            );

            // CapsuleCast2D로 보스 크기를 고려한 장애물 체크
            RaycastHit2D hit = Physics2D.CapsuleCast(
                transform.position,                    // 시작 위치
                scaledSize,                            // Capsule 크기
                CapsuleDirection2D.Vertical,           // 방향
                0f,                                    // 회전 각도
                chargeDirection,                       // Cast 방향
                chargeDistance,                        // Cast 거리
                LayerMask.GetMask("Ground", "Platform") // 지형 레이어
            );

            if (hit.collider != null)
            {
                // 장애물이 있으면 돌진 취소
                if (showDebugLogs)
                    Debug.Log($"[BossEnemy] 돌진 경로에 장애물 감지: {hit.collider.name} (거리: {hit.distance:F1}m). 돌진 취소!");

                lastChargeAttackTime = Time.time; // 쿨다운만 적용
                return;
            }

            // 목표 위치 설정 (Y축은 현재 위치 유지)
            chargeTargetPos = chargeStartPos + new Vector3(
                chargeDirection.x * chargeDistance,
                0f,
                0f
            );

            isCharging = true;
            lastChargeAttackTime = Time.time;

            if (showDebugLogs)
                Debug.Log($"[BossEnemy] 돌진 공격 시작! {chargeStartPos} → {chargeTargetPos} (거리: {chargeDistance:F1}m)");
        }

        private void UpdateCharge()
        {
            if (Data == null || rb == null) return;

            // Rigidbody2D로 이동 (물리 기반)
            Vector2 moveVelocity = chargeDirection * Data.bossChargeSpeed;
            moveVelocity.y = rb.linearVelocity.y; // Y축은 중력 유지

            rb.linearVelocity = moveVelocity;

            // 도착 확인 (X축 거리만 체크)
            float horizontalDistance = Mathf.Abs(transform.position.x - chargeTargetPos.x);
            if (horizontalDistance < 0.5f)
            {
                isCharging = false;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // 돌진 종료 시 정지

                if (showDebugLogs)
                    Debug.Log($"[BossEnemy] 돌진 공격 완료!");
            }
        }

        /// <summary>
        /// 충돌 처리 (돌진 중 장애물과 충돌 시 중단)
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 돌진 중일 때만 충돌 처리
            if (!isCharging) return;

            // Ground 또는 Platform 레이어와 충돌 시 돌진 중단
            int groundLayer = LayerMask.NameToLayer("Ground");
            int platformLayer = LayerMask.NameToLayer("Platform");

            if (collision.gameObject.layer == groundLayer || collision.gameObject.layer == platformLayer)
            {
                // 돌진 중단
                isCharging = false;

                // 속도 0으로 설정
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                }

                if (showDebugLogs)
                    Debug.Log($"[BossEnemy] 돌진 중 장애물 충돌! ({collision.gameObject.name}) 돌진 중단!");
            }
        }


        // ====== Phase 2: 소환 ======

        private bool CanUseSummon()
        {
            if (Data == null) return false;
            if (Time.time - lastSummonTime < Data.bossSummonCooldown) return false;
            if (currentSummonCount >= maxSummonCount) return false;
            return true;
        }

        private void ExecuteSummon()
        {
            if (minionPrefab == null)
            {
                Debug.LogWarning("[BossEnemy] minionPrefab이 null입니다. Inspector에서 설정하세요.");
                return;
            }

            if (minionData == null)
            {
                Debug.LogWarning("[BossEnemy] minionData가 null입니다. Inspector에서 설정하세요.");
                return;
            }

            // 소환 위치 (보스 주변 랜덤)
            Vector3 summonPos = transform.position + new Vector3(
                Random.Range(-3f, 3f),
                0f,
                0f
            );

            // 소환
            GameObject minion = Instantiate(minionPrefab, summonPos, Quaternion.identity);

            if (minion != null)
            {
                // 소환된 적 사망 시 카운트 감소
                Enemy minionEnemy = minion.GetComponent<Enemy>();
                if (minionEnemy != null)
                {
                    // EnemyData 설정 (중요!)
                    minionEnemy.InitializeWithData(minionData);

                    currentSummonCount++;

                    minionEnemy.OnDeath += (enemy) =>
                    {
                        currentSummonCount--;
                    };

                    if (showDebugLogs)
                        Debug.Log($"[BossEnemy] 소환 완료! 위치: {summonPos}, 현재 소환 수: {currentSummonCount}");
                }
                else
                {
                    Debug.LogError("[BossEnemy] 소환된 프리팹에 Enemy 컴포넌트가 없습니다!");
                    Destroy(minion);
                }
            }

            lastSummonTime = Time.time;
        }


        // ====== Phase 3: 범위 공격 ======

        private bool CanUseAreaAttack()
        {
            if (playerTransform == null) return false;
            if (Data == null) return false;
            if (Time.time - lastAreaAttackTime < Data.bossAreaCooldown) return false;
            if (!IsPlayerInDetectionRange()) return false;
            return true;
        }

        private void ExecuteAreaAttack()
        {
            if (Data == null) return;

            // 범위 내 플레이어 확인
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, Data.bossAreaRadius);

            int hitCount = 0;

            foreach (var hit in hits)
            {
                // PlayerStats 확인
                var playerStats = hit.GetComponent<GASPT.Stats.PlayerStats>();

                if (playerStats != null)
                {
                    // 데미지 적용
                    int finalDamage = Mathf.RoundToInt(Data.bossAreaDamage * phaseController.GetAttackMultiplier());
                    playerStats.TakeDamage(finalDamage);

                    hitCount++;

                    if (showDebugLogs)
                        Debug.Log($"[BossEnemy] 범위 공격 적중! 데미지: {finalDamage}");
                }
            }

            if (hitCount == 0 && showDebugLogs)
            {
                Debug.Log($"[BossEnemy] 범위 공격 빗나감");
            }

            // TODO: 범위 공격 시각 효과 (VisualEffect 풀 사용)
            // DebugExtensions.DrawCircle(transform.position, Data.bossAreaRadius, Color.red, 1f);

            lastAreaAttackTime = Time.time;
        }
    }
}
