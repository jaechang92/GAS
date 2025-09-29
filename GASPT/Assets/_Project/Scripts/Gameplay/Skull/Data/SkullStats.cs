using UnityEngine;

namespace Skull.Data
{
    /// <summary>
    /// 스컬별 능력치 데이터
    /// </summary>
    [System.Serializable]
    public class SkullStats
    {
        [Header("기본 능력치")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxMana = 50f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float criticalChance = 0.05f;

        [Header("이동 능력치")]
        [SerializeField] private float moveSpeed = 12f;
        [SerializeField] private float jumpPower = 16f;
        [SerializeField] private float dashDistance = 8f;
        [SerializeField] private float dashCooldown = 1f;

        [Header("특수 능력치")]
        [SerializeField] private float abilityPower = 1f;
        [SerializeField] private float abilityCooldownReduction = 0f;
        [SerializeField] private float manaRegeneration = 5f;
        [SerializeField] private float healthRegeneration = 1f;

        // Properties for external access
        public float MaxHealth => maxHealth;
        public float MaxMana => maxMana;
        public float AttackDamage => attackDamage;
        public float AttackSpeed => attackSpeed;
        public float CriticalChance => criticalChance;
        public float MoveSpeed => moveSpeed;
        public float JumpPower => jumpPower;
        public float DashDistance => dashDistance;
        public float DashCooldown => dashCooldown;
        public float AbilityPower => abilityPower;
        public float AbilityCooldownReduction => abilityCooldownReduction;
        public float ManaRegeneration => manaRegeneration;
        public float HealthRegeneration => healthRegeneration;

        /// <summary>
        /// 기본 스탯으로 초기화
        /// </summary>
        public static SkullStats CreateDefault()
        {
            return new SkullStats
            {
                maxHealth = 100f,
                maxMana = 50f,
                attackDamage = 10f,
                attackSpeed = 1f,
                criticalChance = 0.05f,
                moveSpeed = 12f,
                jumpPower = 16f,
                dashDistance = 8f,
                dashCooldown = 1f,
                abilityPower = 1f,
                abilityCooldownReduction = 0f,
                manaRegeneration = 5f,
                healthRegeneration = 1f
            };
        }

        /// <summary>
        /// 마법사 스탯으로 초기화
        /// </summary>
        public static SkullStats CreateMage()
        {
            return new SkullStats
            {
                maxHealth = 75f,
                maxMana = 100f,
                attackDamage = 8f,
                attackSpeed = 0.8f,
                criticalChance = 0.1f,
                moveSpeed = 10f,
                jumpPower = 14f,
                dashDistance = 10f,
                dashCooldown = 0.8f,
                abilityPower = 1.5f,
                abilityCooldownReduction = 0.2f,
                manaRegeneration = 10f,
                healthRegeneration = 0.5f
            };
        }

        /// <summary>
        /// 전사 스탯으로 초기화
        /// </summary>
        public static SkullStats CreateWarrior()
        {
            return new SkullStats
            {
                maxHealth = 150f,
                maxMana = 30f,
                attackDamage = 15f,
                attackSpeed = 1.2f,
                criticalChance = 0.15f,
                moveSpeed = 14f,
                jumpPower = 18f,
                dashDistance = 6f,
                dashCooldown = 1.2f,
                abilityPower = 0.8f,
                abilityCooldownReduction = 0f,
                manaRegeneration = 3f,
                healthRegeneration = 2f
            };
        }

        /// <summary>
        /// 스탯을 다른 스탯으로 복사
        /// </summary>
        public void CopyFrom(SkullStats other)
        {
            maxHealth = other.maxHealth;
            maxMana = other.maxMana;
            attackDamage = other.attackDamage;
            attackSpeed = other.attackSpeed;
            criticalChance = other.criticalChance;
            moveSpeed = other.moveSpeed;
            jumpPower = other.jumpPower;
            dashDistance = other.dashDistance;
            dashCooldown = other.dashCooldown;
            abilityPower = other.abilityPower;
            abilityCooldownReduction = other.abilityCooldownReduction;
            manaRegeneration = other.manaRegeneration;
            healthRegeneration = other.healthRegeneration;
        }
    }
}