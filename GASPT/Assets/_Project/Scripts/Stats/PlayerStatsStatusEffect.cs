using GASPT.Core.Enums;
using GASPT.StatusEffects;
using GASPT.UI;
using UnityEngine;

namespace GASPT.Stats
{
    /// <summary>
    /// PlayerStats 상태 효과 관련 partial class
    /// </summary>
    public partial class PlayerStats
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
                Debug.LogError("[PlayerStats] StatusEffectManager를 찾을 수 없습니다.");
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

            // 공격력/방어력 관련 효과면 OnStatChanged 이벤트 발생
            TriggerStatChangedForEffect(effect);
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

            // 공격력/방어력 관련 효과면 OnStatChanged 이벤트 발생
            TriggerStatChangedForEffect(effect);
        }

        /// <summary>
        /// StatusEffect에 따라 OnStatChanged 이벤트 발생
        /// </summary>
        private void TriggerStatChangedForEffect(StatusEffect effect)
        {
            if (effect.EffectType == StatusEffectType.AttackUp ||
                effect.EffectType == StatusEffectType.AttackDown)
            {
                // 공격력 변경 이벤트 발생
                int currentAttack = Attack;
                OnStatsChanged?.Invoke(StatType.Attack, currentAttack, currentAttack);
            }
            else if (effect.EffectType == StatusEffectType.DefenseUp ||
                     effect.EffectType == StatusEffectType.DefenseDown)
            {
                // 방어력 변경 이벤트 발생
                int currentDefense = Defense;
                OnStatsChanged?.Invoke(StatType.Defense, currentDefense, currentDefense);
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
                Heal(Mathf.RoundToInt(tickValue));
            }
            // Poison, Burn, Bleed (지속 데미지)
            else if (effect.EffectType == StatusEffectType.Poison ||
                     effect.EffectType == StatusEffectType.Burn ||
                     effect.EffectType == StatusEffectType.Bleed)
            {
                // DoT는 방어력 무시
                int damage = Mathf.RoundToInt(Mathf.Abs(tickValue));
                int previousHP = currentHP;
                currentHP -= damage;
                currentHP = Mathf.Max(currentHP, 0);

                // DamageNumber 표시
                if (DamageNumberPool.Instance != null)
                {
                    Vector3 damagePosition = transform.position + Vector3.up * 1.5f;
                    DamageNumberPool.Instance.ShowDamage(damage, damagePosition, false);
                }

                // 이벤트 발생
                OnDamaged?.Invoke(damage, currentHP, MaxHP);

                // 사망 체크
                if (currentHP <= 0)
                {
                    Die();
                }
            }
        }

        /// <summary>
        /// StatusEffect 버프/디버프 적용
        /// </summary>
        /// <param name="baseStat">기본 스탯 값</param>
        /// <param name="buffType">버프 타입</param>
        /// <param name="debuffType">디버프 타입</param>
        /// <returns>버프/디버프 적용된 최종 값</returns>
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
    }
}
