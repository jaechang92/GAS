using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// 플레이어 런타임 데이터
    /// 게임 진행 중 플레이어 상태를 저장하는 클래스
    /// </summary>
    [Serializable]
    public class PlayerRuntimeData
    {
        [Header("=== 기본 스텟 ===")]
        [Tooltip("현재 체력")]
        public float currentHP = 100f;

        [Tooltip("최대 체력")]
        public float maxHP = 100f;

        [Tooltip("현재 마나")]
        public float currentMP = 50f;

        [Tooltip("최대 마나")]
        public float maxMP = 50f;

        [Header("=== 전투 스텟 ===")]
        [Tooltip("기본 공격력")]
        public float attackPower = 10f;

        [Tooltip("방어력")]
        public float defense = 5f;

        [Tooltip("치명타 확률 (0~1)")]
        [Range(0f, 1f)]
        public float criticalChance = 0.1f;

        [Tooltip("치명타 데미지 배율")]
        public float criticalDamageMultiplier = 1.5f;

        [Header("=== 이동 스텟 ===")]
        [Tooltip("이동 속도")]
        public float moveSpeed = 5f;

        [Tooltip("점프력")]
        public float jumpPower = 10f;

        [Tooltip("대시 속도")]
        public float dashSpeed = 15f;

        [Header("=== 게임 진행 데이터 ===")]
        [Tooltip("현재 골드")]
        public int gold = 0;

        [Tooltip("현재 경험치")]
        public int experience = 0;

        [Tooltip("현재 레벨")]
        public int level = 1;

        [Header("=== 스킬/아이템 데이터 ===")]
        [Tooltip("획득한 어빌리티 ID 목록")]
        public List<string> unlockedAbilityIds = new List<string>();

        [Tooltip("장착 중인 어빌리티 ID 목록")]
        public List<string> equippedAbilityIds = new List<string>();

        [Tooltip("획득한 아이템 ID 목록")]
        public List<string> acquiredItemIds = new List<string>();

        [Header("=== Skull 시스템 (추후 확장) ===")]
        [Tooltip("현재 선택된 Skull ID")]
        public string currentSkullId = "default";

        [Tooltip("보유 중인 Skull ID 목록")]
        public List<string> ownedSkullIds = new List<string> { "default" };

        /// <summary>
        /// 기본값으로 초기화 (새 게임 시작)
        /// </summary>
        public void ResetToDefault()
        {
            currentHP = 100f;
            maxHP = 100f;
            currentMP = 50f;
            maxMP = 50f;

            attackPower = 10f;
            defense = 5f;
            criticalChance = 0.1f;
            criticalDamageMultiplier = 1.5f;

            moveSpeed = 5f;
            jumpPower = 10f;
            dashSpeed = 15f;

            gold = 0;
            experience = 0;
            level = 1;

            unlockedAbilityIds.Clear();
            equippedAbilityIds.Clear();
            acquiredItemIds.Clear();

            currentSkullId = "default";
            ownedSkullIds = new List<string> { "default" };
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(float amount)
        {
            currentHP = Mathf.Min(currentHP + amount, maxHP);
        }

        /// <summary>
        /// 마나 회복
        /// </summary>
        public void RestoreMP(float amount)
        {
            currentMP = Mathf.Min(currentMP + amount, maxMP);
        }

        /// <summary>
        /// 데미지 받기
        /// </summary>
        public void TakeDamage(float damage)
        {
            float actualDamage = Mathf.Max(damage - defense, 0f);
            currentHP = Mathf.Max(currentHP - actualDamage, 0f);
        }

        /// <summary>
        /// 골드 획득
        /// </summary>
        public void AddGold(int amount)
        {
            gold += amount;
        }

        /// <summary>
        /// 경험치 획득
        /// </summary>
        public void AddExperience(int amount)
        {
            experience += amount;
            // TODO: 레벨업 로직 추가
        }

        /// <summary>
        /// 어빌리티 잠금 해제
        /// </summary>
        public void UnlockAbility(string abilityId)
        {
            if (!unlockedAbilityIds.Contains(abilityId))
            {
                unlockedAbilityIds.Add(abilityId);
            }
        }

        /// <summary>
        /// 어빌리티 장착
        /// </summary>
        public void EquipAbility(string abilityId)
        {
            if (unlockedAbilityIds.Contains(abilityId) && !equippedAbilityIds.Contains(abilityId))
            {
                equippedAbilityIds.Add(abilityId);
            }
        }

        /// <summary>
        /// 아이템 획득
        /// </summary>
        public void AcquireItem(string itemId)
        {
            if (!acquiredItemIds.Contains(itemId))
            {
                acquiredItemIds.Add(itemId);
            }
        }

        /// <summary>
        /// 사망 여부 확인
        /// </summary>
        public bool IsDead()
        {
            return currentHP <= 0f;
        }

        /// <summary>
        /// 깊은 복사
        /// </summary>
        public PlayerRuntimeData Clone()
        {
            var clone = new PlayerRuntimeData
            {
                currentHP = this.currentHP,
                maxHP = this.maxHP,
                currentMP = this.currentMP,
                maxMP = this.maxMP,
                attackPower = this.attackPower,
                defense = this.defense,
                criticalChance = this.criticalChance,
                criticalDamageMultiplier = this.criticalDamageMultiplier,
                moveSpeed = this.moveSpeed,
                jumpPower = this.jumpPower,
                dashSpeed = this.dashSpeed,
                gold = this.gold,
                experience = this.experience,
                level = this.level,
                currentSkullId = this.currentSkullId
            };

            clone.unlockedAbilityIds = new List<string>(this.unlockedAbilityIds);
            clone.equippedAbilityIds = new List<string>(this.equippedAbilityIds);
            clone.acquiredItemIds = new List<string>(this.acquiredItemIds);
            clone.ownedSkullIds = new List<string>(this.ownedSkullIds);

            return clone;
        }
    }
}
