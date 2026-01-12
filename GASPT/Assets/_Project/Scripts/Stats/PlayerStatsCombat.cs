using GASPT.Combat;
using GASPT.Core;
using GASPT.Core.Enums;
using GASPT.DTOs;
using GASPT.Gameplay.Enemies;
using GASPT.UI;
using UnityEngine;

namespace GASPT.Stats
{
    /// <summary>
    /// PlayerStats 전투 관련 partial class
    /// </summary>
    public partial class PlayerStats
    {
        // ====== Combat 메서드 ======

        /// <summary>
        /// 데미지를 받습니다 (방어력 적용)
        /// </summary>
        /// <param name="incomingDamage">들어오는 데미지</param>
        public void TakeDamage(int incomingDamage)
        {
            if (isDead)
            {
                Debug.LogWarning("[PlayerStats] TakeDamage(): 이미 사망한 상태입니다.");
                return;
            }

            if (incomingDamage <= 0)
            {
                Debug.LogWarning($"[PlayerStats] TakeDamage(): 유효하지 않은 데미지입니다: {incomingDamage}");
                return;
            }

            // DamageCalculator를 사용하여 방어력 적용
            int actualDamage = DamageCalculator.CalculateDamageReceived(incomingDamage, Defense);

            // HP 감소
            int previousHP = currentHP;
            currentHP -= actualDamage;
            currentHP = Mathf.Max(currentHP, 0);

            // DamageNumber 표시
            if (DamageNumberPool.Instance != null)
            {
                Vector3 damagePosition = transform.position + Vector3.up * 1.5f;
                DamageNumberPool.Instance.ShowDamage(actualDamage, damagePosition, false);
            }

            // 이벤트 발생
            OnDamaged?.Invoke(actualDamage, currentHP, MaxHP);

            // 사망 체크
            if (currentHP <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 체력을 회복합니다
        /// </summary>
        /// <param name="healAmount">회복량</param>
        public void Heal(int healAmount)
        {
            if (isDead)
            {
                Debug.LogWarning("[PlayerStats] Heal(): 사망한 상태에서는 회복할 수 없습니다.");
                return;
            }

            if (healAmount <= 0)
            {
                Debug.LogWarning($"[PlayerStats] Heal(): 유효하지 않은 회복량입니다: {healAmount}");
                return;
            }

            int previousHP = currentHP;
            currentHP += healAmount;
            currentHP = Mathf.Min(currentHP, MaxHP);

            int actualHealed = currentHP - previousHP;

            // DamageNumber 표시 (회복)
            if (DamageNumberPool.Instance != null && actualHealed > 0)
            {
                Vector3 healPosition = transform.position + Vector3.up * 1.5f;
                DamageNumberPool.Instance.ShowHeal(actualHealed, healPosition);
            }

            // 이벤트 발생
            OnHealed?.Invoke(actualHealed, currentHP, MaxHP);
        }

        /// <summary>
        /// 적에게 데미지를 입힙니다
        /// </summary>
        /// <param name="target">공격할 적</param>
        public void DealDamageTo(Enemy target)
        {
            if (target == null)
            {
                Debug.LogWarning("[PlayerStats] DealDamageTo(): target이 null입니다.");
                return;
            }

            if (isDead)
            {
                Debug.LogWarning("[PlayerStats] DealDamageTo(): 사망한 상태에서는 공격할 수 없습니다.");
                return;
            }

            if (target.IsDead)
            {
                Debug.LogWarning($"[PlayerStats] DealDamageTo(): {target.name}은(는) 이미 사망했습니다.");
                return;
            }

            // DamageCalculator를 사용하여 데미지 계산
            int damage = DamageCalculator.CalculateDamageDealt(Attack);

            // 적에게 데미지 적용
            target.TakeDamage(damage);
        }

        /// <summary>
        /// 플레이어 사망 처리
        /// </summary>
        private void Die()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            currentHP = 0;

            Debug.Log("[PlayerStats] 플레이어 사망!");

            // 이벤트 발생
            OnDeath?.Invoke();

            // GameFlow 상태 전환 (GameOver로)
            if (GameFlowStateMachine.HasInstance)
            {
                GameFlowStateMachine.Instance.TriggerPlayerDied();
            }
        }

        /// <summary>
        /// 플레이어를 부활시킵니다 (테스트용)
        /// </summary>
        public void Revive()
        {
            isDead = false;
            int oldHP = currentHP;
            currentHP = MaxHP;

            // 회복 이벤트 발생 (UI 업데이트용)
            int healAmount = currentHP - oldHP;
            OnHealed?.Invoke(healAmount, currentHP, MaxHP);
        }

        /// <summary>
        /// RunData로부터 플레이어 초기화 (씬 로드 후)
        /// RunManager.SyncToPlayer()에서 호출됨
        /// </summary>
        public void InitializeFromRunData(PlayerRunData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[PlayerStats] InitializeFromRunData: data가 null입니다.");
                return;
            }

            // 이전 값 저장
            int oldHP = currentHP;
            int oldMana = currentMana;

            // 기본 스탯 설정
            baseHP = data.maxHP;
            baseAttack = data.baseAttack;
            baseDefense = data.baseDefense;
            baseMana = data.maxMana;

            // 현재 상태 복원
            currentHP = data.currentHP;
            currentMana = data.currentMana;
            isDead = currentHP <= 0;

            // 스탯 재계산
            isDirty = true;
            RecalculateIfDirty();

            // 이벤트 발생
            OnStatsChanged?.Invoke(StatType.HP, oldHP, currentHP);
            OnStatsChanged?.Invoke(StatType.Mana, oldMana, currentMana);
        }

        /// <summary>
        /// 현재 상태를 RunData로 변환 (씬 전환 전)
        /// RunManager.SyncFromPlayer()에서 호출됨
        /// </summary>
        public PlayerRunData ToRunData()
        {
            return new PlayerRunData
            {
                maxHP = baseHP,
                currentHP = currentHP,
                maxMana = baseMana,
                currentMana = currentMana,
                baseAttack = baseAttack,
                baseDefense = baseDefense,
                level = 1, // TODO: PlayerLevel 연동
                currentExp = 0
            };
        }

        /// <summary>
        /// 런 시작 시 플레이어를 초기 상태로 리셋
        /// GameManager.StartNewRun()에서 호출됨
        /// </summary>
        public void ResetToBaseStats()
        {
            // 모든 장비 해제
            UnequipAll();

            // 이전 값 저장
            int oldHP = currentHP;
            int oldMana = currentMana;

            // HP/마나 최대로 회복
            isDead = false;
            currentHP = MaxHP;
            currentMana = MaxMana;

            // 상태 효과 클리어 (있다면)
            // ClearAllStatusEffects();

            // OnStatsChanged로 통일된 이벤트 발생
            OnStatsChanged?.Invoke(StatType.HP, oldHP, currentHP);
            OnStatsChanged?.Invoke(StatType.Mana, oldMana, currentMana);
        }
    }
}
