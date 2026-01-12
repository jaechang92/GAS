using UnityEngine;
using GASPT.Data;
using GASPT.Economy;
using GASPT.Combat;
using GASPT.Level;
using GASPT.UI;
using GASPT.Meta;
using GASPT.Core.Enums;
using GASPT.Loot;

namespace GASPT.Gameplay.Enemies
{
    /// <summary>
    /// Enemy partial class - 전투 및 보상 시스템
    ///
    /// 포함 내용:
    /// - 데미지 처리 (TakeDamage)
    /// - 사망 처리 (Die)
    /// - 공격 (DealDamageTo)
    /// - 보상 (골드, 경험치, 아이템, 메타재화 드롭)
    /// </summary>
    public abstract partial class Enemy
    {
        // ====== 데미지 처리 ======

        /// <summary>
        /// 속성 데미지 받기 (속성 상성 적용)
        /// </summary>
        /// <param name="baseDamage">기본 데미지</param>
        /// <param name="attackElement">공격 속성</param>
        public void TakeDamage(int baseDamage, ElementType attackElement)
        {
            if (enemyData == null) return;

            // ElementDamageCalculator로 속성 상성 계산
            int finalDamage = ElementDamageCalculator.CalculateDamage(
                baseDamage,
                attackElement,
                enemyData.elementType
            );

            // 기존 TakeDamage 호출
            TakeDamage(finalDamage);
        }

        /// <summary>
        /// 데미지 받기 (기본)
        /// </summary>
        /// <param name="damage">받을 데미지</param>
        public void TakeDamage(int damage)
        {
            if (isDead)
            {
                Debug.LogWarning($"[Enemy] {enemyData.enemyName}: 이미 사망한 적입니다.");
                return;
            }

            if (damage <= 0)
            {
                Debug.LogWarning($"[Enemy] {enemyData.enemyName}: 데미지가 0 이하입니다: {damage}");
                return;
            }

            // HP 감소
            currentHp -= damage;
            currentHp = Mathf.Max(0, currentHp);

            // DamageNumber 표시
            if (DamageNumberPool.Instance != null)
            {
                Vector3 damagePosition = transform.position + Vector3.up * 1.5f;
                DamageNumberPool.Instance.ShowDamage(damage, damagePosition, false);
            }

            // 이벤트 발생
            OnHpChanged?.Invoke(currentHp, enemyData.maxHp);

            // 사망 체크
            if (currentHp <= 0)
            {
                Die();
            }
        }


        // ====== 사망 처리 ======

        /// <summary>
        /// 사망 처리 (골드 드롭 포함)
        /// </summary>
        private void Die()
        {
            if (isDead)
            {
                Debug.LogWarning($"[Enemy] {enemyData.enemyName}: 이미 사망한 적입니다.");
                return;
            }

            isDead = true;

            // 골드 드롭
            DropGold();

            // 경험치 지급
            GiveExp();

            // 아이템 드롭
            DropLoot();

            // 메타 재화 드롭 (Bone/Soul)
            DropMetaCurrency();

            // 사망 이벤트 발생
            OnDeath?.Invoke(this);

            // 풀로 반환 (1초 후 - 사망 애니메이션용)
            ReturnToPoolDelayed(1f);
        }


        // ====== 공격 ======

        /// <summary>
        /// 플레이어를 공격합니다
        /// </summary>
        /// <param name="target">공격할 플레이어</param>
        public void DealDamageTo(GASPT.Stats.PlayerStats target)
        {
            if (target == null)
            {
                Debug.LogWarning($"[Enemy] {enemyData.enemyName}: target이 null입니다.");
                return;
            }

            if (isDead)
            {
                Debug.LogWarning($"[Enemy] {enemyData.enemyName}: 사망한 상태에서는 공격할 수 없습니다.");
                return;
            }

            if (target.IsDead)
            {
                Debug.LogWarning($"[Enemy] {enemyData.enemyName}: 플레이어가 이미 사망했습니다.");
                return;
            }

            // DamageCalculator를 사용하여 데미지 계산
            int damage = DamageCalculator.CalculateDamageDealt(Attack);

            // 플레이어에게 데미지 적용
            target.TakeDamage(damage);
        }


        // ====== 골드 드롭 ======

        /// <summary>
        /// 골드 드롭 처리 (스케일링 적용)
        /// </summary>
        private void DropGold()
        {
            if (enemyData == null) return;

            // 스케일링된 골드 또는 기본 골드 계산
            int goldDrop;
            if (scaledMinGold > 0 && scaledMaxGold > 0)
            {
                goldDrop = UnityEngine.Random.Range(scaledMinGold, scaledMaxGold + 1);
            }
            else
            {
                goldDrop = enemyData.GetRandomGoldDrop();
            }

            // CurrencySystem에 골드 추가
            CurrencySystem currencySystem = CurrencySystem.Instance;

            if (currencySystem != null)
            {
                currencySystem.AddGold(goldDrop);
            }
            else
            {
                Debug.LogError($"[Enemy] CurrencySystem을 찾을 수 없습니다. 골드 드롭 실패: {goldDrop}");
            }
        }


        // ====== 경험치 지급 ======

        /// <summary>
        /// 경험치 지급 처리 (스케일링 적용)
        /// </summary>
        private void GiveExp()
        {
            if (enemyData == null) return;

            // 스케일링된 경험치 또는 기본 경험치
            int expReward = scaledExp > 0 ? scaledExp : enemyData.expReward;

            // PlayerLevel에 경험치 추가
            PlayerLevel playerLevel = PlayerLevel.Instance;

            if (playerLevel != null)
            {
                playerLevel.AddExp(expReward);

                // EXP Number 표시
                if (DamageNumberPool.Instance != null)
                {
                    Vector3 expPosition = transform.position + Vector3.up * 2f;
                    DamageNumberPool.Instance.ShowExp(expReward, expPosition);
                }
            }
            else
            {
                Debug.LogError($"[Enemy] PlayerLevel을 찾을 수 없습니다. EXP 지급 실패: {enemyData.expReward}");
            }
        }


        // ====== 아이템 드롭 ======

        /// <summary>
        /// 아이템 드롭 처리
        /// </summary>
        private void DropLoot()
        {
            if (enemyData == null || enemyData.lootTableV2 == null) return;

            if (ItemDropManager.HasInstance)
            {
                ItemDropManager.Instance.DropFromTable(enemyData.lootTableV2, transform.position);
            }
            else
            {
                Debug.LogWarning("[Enemy] ItemDropManager를 찾을 수 없습니다.");
            }
        }


        // ====== 메타 재화 드롭 ======

        /// <summary>
        /// 메타 재화 (Bone/Soul) 드롭 처리
        /// </summary>
        private void DropMetaCurrency()
        {
            if (enemyData == null) return;

            // MetaProgressionManager가 없거나 런 진행 중이 아니면 스킵
            if (!MetaProgressionManager.HasInstance || !MetaProgressionManager.Instance.IsInRun)
            {
                return;
            }

            var meta = MetaProgressionManager.Instance;

            // Bone 드롭 (일반 적 포함 모든 적)
            int boneDrop = enemyData.GetRandomBoneDrop();
            if (boneDrop > 0)
            {
                meta.Currency.AddTempBone(boneDrop);
            }

            // Soul 드롭 (보스 전용)
            if (enemyData.enemyType == EnemyType.Boss)
            {
                int soulDrop = enemyData.GetSoulDrop();
                if (soulDrop > 0)
                {
                    meta.Currency.AddTempSoul(soulDrop);
                }
            }
        }
    }
}
