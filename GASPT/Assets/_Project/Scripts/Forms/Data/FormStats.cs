using System;
using UnityEngine;

namespace GASPT.Forms
{
    /// <summary>
    /// 폼의 스탯 데이터
    /// 각 폼이 플레이어에게 부여하는 스탯 보너스
    /// </summary>
    [Serializable]
    public struct FormStats
    {
        [Header("공격 스탯")]
        [Tooltip("공격력 보너스 (기본값에 더해짐)")]
        [Range(0f, 100f)]
        public float attackPower;

        [Tooltip("최대 마나 보너스")]
        [Range(0f, 100f)]
        public float maxManaBonus;

        [Tooltip("공격 속도 배율 (1.0 = 100%)")]
        [Range(0.5f, 2f)]
        public float attackSpeed;

        [Tooltip("치명타 확률 (0~1)")]
        [Range(0f, 1f)]
        public float criticalChance;

        [Tooltip("치명타 데미지 배율 (1.5 = 150%)")]
        [Range(1f, 3f)]
        public float criticalDamage;

        [Header("방어 스탯")]
        [Tooltip("최대 체력 보너스")]
        [Range(0f, 200f)]
        public float maxHealthBonus;

        [Tooltip("방어력 보너스")]
        [Range(0f, 50f)]
        public float defense;

        [Header("이동 스탯")]
        [Tooltip("이동 속도 배율 (1.0 = 100%)")]
        [Range(0.5f, 2f)]
        public float moveSpeed;

        [Tooltip("점프력 배율 (1.0 = 100%)")]
        [Range(0.5f, 2f)]
        public float jumpPower;

        [Header("특수 스탯")]
        [Tooltip("스킬 쿨다운 감소율 (0~0.5, 0.3 = 30% 감소)")]
        [Range(0f, 0.5f)]
        public float cooldownReduction;

        [Tooltip("마나 회복 속도 배율")]
        [Range(0.5f, 2f)]
        public float manaRegen;

        [Tooltip("흡혈율 (0~1, 데미지의 %만큼 회복)")]
        [Range(0f, 1f)]
        public float vampirismRate;


        /// <summary>
        /// 기본 스탯 생성 (밸런스형)
        /// </summary>
        public static FormStats Default => new FormStats
        {
            attackPower = 10f,
            attackSpeed = 1f,
            criticalChance = 0.05f,
            criticalDamage = 1.5f,
            maxHealthBonus = 0f,
            defense = 0f,
            moveSpeed = 1f,
            jumpPower = 1f,
            cooldownReduction = 0f,
            manaRegen = 1f,
            maxManaBonus = 0f,
            vampirismRate = 0f
        };

        /// <summary>
        /// 두 스탯을 더함 (보너스 적용 시 사용)
        /// </summary>
        public static FormStats operator +(FormStats a, FormStats b)
        {
            return new FormStats
            {
                attackPower = a.attackPower + b.attackPower,
                attackSpeed = a.attackSpeed * b.attackSpeed,
                criticalChance = Mathf.Clamp01(a.criticalChance + b.criticalChance),
                criticalDamage = a.criticalDamage + (b.criticalDamage - 1f),
                maxHealthBonus = a.maxHealthBonus + b.maxHealthBonus,
                defense = a.defense + b.defense,
                moveSpeed = a.moveSpeed * b.moveSpeed,
                jumpPower = a.jumpPower * b.jumpPower,
                cooldownReduction = Mathf.Clamp(a.cooldownReduction + b.cooldownReduction, 0f, 0.5f),
                manaRegen = a.manaRegen * b.manaRegen,
                maxManaBonus = a.maxManaBonus + b.maxManaBonus,
                vampirismRate = a.vampirismRate + b.vampirismRate
            };
        }

        /// <summary>
        /// 스탯에 배율 적용 (각성 보너스 등)
        /// </summary>
        public FormStats ApplyMultiplier(float multiplier)
        {
            return new FormStats
            {
                attackPower = attackPower * multiplier,
                attackSpeed = 1f + (attackSpeed - 1f) * multiplier,
                criticalChance = Mathf.Clamp01(criticalChance * multiplier),
                criticalDamage = 1f + (criticalDamage - 1f) * multiplier,
                maxHealthBonus = maxHealthBonus * multiplier,
                defense = defense * multiplier,
                moveSpeed = 1f + (moveSpeed - 1f) * multiplier,
                jumpPower = 1f + (jumpPower - 1f) * multiplier,
                cooldownReduction = Mathf.Clamp(cooldownReduction * multiplier, 0f, 0.5f),
                manaRegen = 1f + (manaRegen - 1f) * multiplier,
                maxManaBonus = maxManaBonus * multiplier,
                vampirismRate = vampirismRate * multiplier
            };
        }

        /// <summary>
        /// 디버그용 문자열 출력
        /// </summary>
        public override string ToString()
        {
            return $"ATK:{attackPower:F1} SPD:{attackSpeed:P0} CRIT:{criticalChance:P0} " +
                   $"HP+:{maxHealthBonus:F0} DEF:{defense:F1} MOVE:{moveSpeed:P0}";
        }
    }
}
