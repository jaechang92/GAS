using UnityEngine;
using GASPT.StatusEffects;
using GASPT.Core.Pooling;
using GASPT.UI;
using GASPT.Core.Enums;

namespace GASPT.Gameplay.Enemies
{
    /// <summary>
    /// Enemy partial class - StatusEffect 통합, 풀링, 디버그
    ///
    /// 포함 내용:
    /// - StatusEffect 이벤트 구독/해제
    /// - StatusEffect 틱 처리 (DoT, Regeneration)
    /// - 버프/디버프 스탯 적용
    /// - IPoolable 구현 (OnSpawn, OnDespawn)
    /// - 풀 반환 처리
    /// - 디버그 메서드
    /// </summary>
    public abstract partial class Enemy
    {
        // ====== StatusEffect 통합 ======

        /// <summary>
        /// StatusEffect 이벤트 구독
        /// </summary>
        private void SubscribeToStatusEffectEvents()
        {
            // Instance 호출로 StatusEffectManager가 없으면 자동 생성
            StatusEffectManager manager = StatusEffectManager.Instance;

            if (manager != null)
            {
                // 중복 구독 방지를 위해 먼저 구독 해제
                manager.OnEffectApplied -= OnEffectApplied;
                manager.OnEffectRemoved -= OnEffectRemoved;

                // 구독
                manager.OnEffectApplied += OnEffectApplied;
                manager.OnEffectRemoved += OnEffectRemoved;
            }
            else
            {
                Debug.LogError("[Enemy] StatusEffectManager를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// StatusEffect 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromStatusEffectEvents()
        {
            if (StatusEffectManager.HasInstance)
            {
                StatusEffectManager.Instance.OnEffectApplied -= OnEffectApplied;
                StatusEffectManager.Instance.OnEffectRemoved -= OnEffectRemoved;
            }
        }

        /// <summary>
        /// 효과 적용 시 호출
        /// </summary>
        private void OnEffectApplied(GameObject target, StatusEffect effect)
        {
            if (target != gameObject) return;

            // DoT 효과 처리
            if (effect.TickInterval > 0f)
            {
                effect.OnTick += OnStatusEffectTick;
            }
        }

        /// <summary>
        /// 효과 제거 시 호출
        /// </summary>
        private void OnEffectRemoved(GameObject target, StatusEffect effect)
        {
            if (target != gameObject) return;

            // DoT 이벤트 구독 해제
            if (effect.TickInterval > 0f)
            {
                effect.OnTick -= OnStatusEffectTick;
            }
        }

        /// <summary>
        /// StatusEffect 틱 발생 시 호출 (DoT/Regeneration)
        /// </summary>
        private void OnStatusEffectTick(StatusEffect effect, float tickValue)
        {
            if (effect.Target != gameObject) return;

            // Regeneration (회복)
            if (effect.EffectType == StatusEffectType.Regeneration)
            {
                int healAmount = Mathf.RoundToInt(tickValue);
                currentHp += healAmount;
                currentHp = Mathf.Min(currentHp, MaxHp);

                OnHpChanged?.Invoke(currentHp, MaxHp);
            }
            // Poison, Burn, Bleed (지속 데미지)
            else if (effect.EffectType == StatusEffectType.Poison ||
                     effect.EffectType == StatusEffectType.Burn ||
                     effect.EffectType == StatusEffectType.Bleed)
            {
                int damage = Mathf.RoundToInt(Mathf.Abs(tickValue));
                currentHp -= damage;
                currentHp = Mathf.Max(0, currentHp);

                // DamageNumber 표시
                if (DamageNumberPool.Instance != null)
                {
                    Vector3 damagePosition = transform.position + Vector3.up * 1.5f;
                    DamageNumberPool.Instance.ShowDamage(damage, damagePosition, false);
                }

                // 이벤트 발생
                OnHpChanged?.Invoke(currentHp, MaxHp);

                // 사망 체크
                if (currentHp <= 0)
                {
                    // Die()는 Combat partial에 있으므로 직접 호출 가능
                    TakeDamage(0); // 사망 트리거용 (이미 HP가 0이므로)
                }
            }
        }

        /// <summary>
        /// 공격력에 StatusEffect 버프/디버프 적용 (Attack 프로퍼티에서 호출)
        /// </summary>
        private int GetAttackWithStatusEffects()
        {
            int baseAttack = scaledAttack > 0 ? scaledAttack : (enemyData != null ? enemyData.attack : 0);
            return ApplyStatusEffects(baseAttack, StatusEffectType.AttackUp, StatusEffectType.AttackDown);
        }

        /// <summary>
        /// StatusEffect 버프/디버프 적용
        /// </summary>
        private int ApplyStatusEffects(int baseStat, StatusEffectType buffType, StatusEffectType debuffType)
        {
            if (!StatusEffectManager.HasInstance)
            {
                return baseStat;
            }

            float modifier = 0f;

            // 버프 합산
            StatusEffect buffEffect = StatusEffectManager.Instance.GetEffect(gameObject, buffType);
            if (buffEffect != null && buffEffect.IsActive)
            {
                modifier += buffEffect.Value * buffEffect.StackCount;
            }

            // 디버프 합산
            StatusEffect debuffEffect = StatusEffectManager.Instance.GetEffect(gameObject, debuffType);
            if (debuffEffect != null && debuffEffect.IsActive)
            {
                modifier -= debuffEffect.Value * debuffEffect.StackCount;
            }

            int finalValue = baseStat + Mathf.RoundToInt(modifier);
            return Mathf.Max(finalValue, 1); // 최소 1 보장
        }


        // ====== IPoolable 구현 ======

        /// <summary>
        /// 풀에서 스폰될 때 호출
        /// </summary>
        public void OnSpawn()
        {
            // 상태 초기화
            isDead = false;

            // HP 복원 (enemyData가 설정되어 있으면)
            if (enemyData != null)
            {
                currentHp = enemyData.maxHp;
                OnHpChanged?.Invoke(currentHp, enemyData.maxHp);
            }
        }

        /// <summary>
        /// 풀로 반환될 때 호출
        /// </summary>
        public void OnDespawn()
        {
            // StatusEffect 정리
            UnsubscribeFromStatusEffectEvents();

            // 이벤트 구독자 정리
            OnHpChanged = null;
            OnDeath = null;
        }

        /// <summary>
        /// 지연 후 풀로 반환
        /// </summary>
        private async void ReturnToPoolDelayed(float delay)
        {
            // 지연 대기
            await Awaitable.WaitForSecondsAsync(delay);

            // PoolManager를 통해 풀로 반환
            if (PoolManager.Instance != null)
            {
                // Enemy 타입에 맞게 Despawn
                if (this is BasicMeleeEnemy basicMelee)
                {
                    PoolManager.Instance.Despawn(basicMelee);
                }
                else if (this is RangedEnemy rangedEnemy)
                {
                    PoolManager.Instance.Despawn(rangedEnemy);
                }
                else if (this is FlyingEnemy flyingEnemy)
                {
                    PoolManager.Instance.Despawn(flyingEnemy);
                }
                else if (this is EliteEnemy eliteEnemy)
                {
                    PoolManager.Instance.Despawn(eliteEnemy);
                }
                else
                {
                    Debug.LogWarning($"[Enemy] {enemyData?.enemyName} 알 수 없는 Enemy 타입. GameObject를 파괴합니다.");
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogWarning($"[Enemy] {enemyData?.enemyName} PoolManager가 없어 GameObject를 파괴합니다.");
                Destroy(gameObject);
            }
        }


        // ====== 디버그 ======

        /// <summary>
        /// 현재 적 정보 출력
        /// </summary>
        [ContextMenu("Print Enemy Info")]
        private void DebugPrintEnemyInfo()
        {
            if (enemyData == null)
            {
                Debug.LogWarning("[Enemy] enemyData가 null입니다.");
                return;
            }

            Debug.Log($"[Enemy] ========== {enemyData.enemyName} 정보 ==========");
            Debug.Log($"[Enemy] 타입: {enemyData.enemyType}");
            Debug.Log($"[Enemy] HP: {currentHp}/{enemyData.maxHp}");
            Debug.Log($"[Enemy] 공격력: {enemyData.attack}");
            Debug.Log($"[Enemy] 골드 드롭: {enemyData.minGoldDrop}-{enemyData.maxGoldDrop}");
            Debug.Log($"[Enemy] 사망 여부: {isDead}");
            Debug.Log($"[Enemy] =====================================");
        }

        /// <summary>
        /// 테스트용 데미지 받기 (10 데미지)
        /// </summary>
        [ContextMenu("Take 10 Damage (Test)")]
        private void DebugTakeDamage()
        {
            TakeDamage(10);
        }

        /// <summary>
        /// 테스트용 즉사 (Test)
        /// </summary>
        [ContextMenu("Instant Death (Test)")]
        private void DebugInstantDeath()
        {
            TakeDamage(currentHp);
        }
    }
}
