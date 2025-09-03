// ���� ��ġ: Assets/Scripts/GAS/Core/GASConstants.cs
using System;

namespace GAS.Core
{
    /// <summary>
    /// GAS �ý��� ��ü���� ���Ǵ� ��� ����
    /// </summary>
    public static class GASConstants
    {
        #region System Constants
        /// <summary>
        /// �ִ� ȿ�� ���� ��
        /// </summary>
        public const int MAX_EFFECT_STACKS = 99;

        /// <summary>
        /// �⺻ ƽ ����Ʈ (��)
        /// </summary>
        public const float DEFAULT_TICK_RATE = 0.1f;

        /// <summary>
        /// �ּ� �Ӽ���
        /// </summary>
        public const float MIN_ATTRIBUTE_VALUE = 0f;

        /// <summary>
        /// �ִ� �Ӽ��� (�⺻)
        /// </summary>
        public const float DEFAULT_MAX_ATTRIBUTE_VALUE = 9999f;
        #endregion

        #region Tag Categories
        /// <summary>
        /// �±� ī�װ� ���λ�
        /// </summary>
        public static class TagCategories
        {
            public const string STATUS = "Status";
            public const string ABILITY = "Ability";
            public const string EFFECT = "Effect";
            public const string DAMAGE = "Damage";
            public const string BUFF = "Buff";
            public const string DEBUFF = "Debuff";
            public const string CONTROL = "Control";
            public const string MOVEMENT = "Movement";
            public const string COMBAT = "Combat";
            public const string IMMUNITY = "Immunity";
        }

        /// <summary>
        /// ���� ���Ǵ� �±� ����
        /// </summary>
        public static class CommonTags
        {
            // Status Tags
            public const string STUNNED = "Status.Control.Stunned";
            public const string ROOTED = "Status.Control.Rooted";
            public const string SILENCED = "Status.Control.Silenced";
            public const string INVULNERABLE = "Status.Immunity.Invulnerable";
            public const string INVISIBLE = "Status.Invisible";

            // Combat Tags
            public const string IN_COMBAT = "Combat.InCombat";
            public const string CASTING = "Combat.Casting";
            public const string CHANNELING = "Combat.Channeling";
            public const string ATTACKING = "Combat.Attacking";

            // Movement Tags
            public const string MOVING = "Movement.Moving";
            public const string SPRINTING = "Movement.Sprinting";
            public const string JUMPING = "Movement.Jumping";
            public const string FALLING = "Movement.Falling";

            // Effect Tags
            public const string BURNING = "Effect.DOT.Burning";
            public const string POISONED = "Effect.DOT.Poisoned";
            public const string BLEEDING = "Effect.DOT.Bleeding";
            public const string REGENERATING = "Effect.HOT.Regenerating";
            public const string SHIELDED = "Effect.Shield";
        }
        #endregion

        #region Attribute Types
        /// <summary>
        /// �⺻ �Ӽ� Ÿ��
        /// </summary>
        public enum AttributeType
        {
            // Core Attributes
            Health,
            HealthMax,
            HealthRegen,

            Mana,
            ManaMax,
            ManaRegen,

            Stamina,
            StaminaMax,
            StaminaRegen,

            // Combat Attributes
            AttackPower,
            DefensePower,
            MagicPower,
            MagicResistance,

            CriticalChance,
            CriticalDamage,
            AttackSpeed,
            MovementSpeed,

            // Defensive Attributes
            Armor,
            Block,
            Dodge,
            Parry,

            // Resource Percentages (Derived)
            HealthPercent,
            ManaPercent,
            StaminaPercent,

            // Cost Percentages (Derived)
            HealthMax10Pct,
            HealthMax25Pct,
            HealthMax50Pct,

            ManaMax10Pct,
            ManaMax25Pct,
            ManaMax50Pct,

            StaminaMax10Pct,
            StaminaMax25Pct,
            StaminaMax50Pct,

            // Status
            Level,
            Experience,
            Gold,

            // Custom extensible range (for game-specific attributes)
            Custom_Start = 1000
        }
        #endregion

        #region Effect Types
        /// <summary>
        /// ȿ�� Ÿ��
        /// </summary>
        public enum EffectType
        {
            Instant,
            Duration,
            Infinite,
            Periodic
        }

        /// <summary>
        /// ȿ�� ���� ��å
        /// </summary>
        public enum EffectStackPolicy
        {
            None,           // ���� �Ұ�
            Stack,          // ȿ�� ��ø
            Refresh,        // ���ӽð� ����
            StackAndRefresh // ��ø + ����
        }

        /// <summary>
        /// ȿ�� ���� ��å
        /// </summary>
        public enum EffectRemovalPolicy
        {
            OnExpire,       // �ð� ���� ��
            OnDeath,        // ��� ��
            OnCleanse,      // ��ȭ ��
            Manual,         // ���� ���Ÿ�
            Never           // ���� �Ұ�
        }
        #endregion

        #region Ability Types
        /// <summary>
        /// �ɷ� Ÿ��
        /// </summary>
        public enum AbilityType
        {
            Instant,
            Channel,
            Charge,
            Toggle,
            Passive,
            Combo
        }

        /// <summary>
        /// �ɷ� Ȱ��ȭ ���� ����
        /// </summary>
        public enum AbilityFailureReason
        {
            None,
            OnCooldown,
            NotEnoughResource,
            InvalidTarget,
            OutOfRange,
            RequiredTagMissing,
            BlockedByTag,
            Silenced,
            Stunned,
            Dead,
            AlreadyActive
        }
        #endregion

        #region Modifier Operations
        /// <summary>
        /// �Ӽ� ���� ���� Ÿ��
        /// </summary>
        public enum ModifierOperation
        {
            Add,            // ���ϱ�
            Subtract,       // ����
            Multiply,       // ���ϱ�
            Divide,         // ������
            Override,       // �����
            AddPercent,     // �ۼ�Ʈ ����
            SubtractPercent // �ۼ�Ʈ ����
        }

        /// <summary>
        /// ������ ���� ����
        /// </summary>
        public enum ModifierPriority
        {
            VeryLow = -100,
            Low = -50,
            Normal = 0,
            High = 50,
            VeryHigh = 100,
            Override = 1000  // �׻� �������� ����
        }
        #endregion

        #region Damage Types
        /// <summary>
        /// ������ Ÿ��
        /// </summary>
        [Flags]
        public enum DamageType
        {
            None = 0,
            Physical = 1 << 0,
            Magical = 1 << 1,
            True = 1 << 2,
            Fire = 1 << 3,
            Ice = 1 << 4,
            Lightning = 1 << 5,
            Poison = 1 << 6,
            Holy = 1 << 7,
            Dark = 1 << 8
        }
        #endregion

        #region Targeting
        /// <summary>
        /// Ÿ���� ���
        /// </summary>
        public enum TargetingMode
        {
            None,
            Self,
            SingleTarget,
            MultiTarget,
            Area,
            Cone,
            Line
        }

        /// <summary>
        /// Ÿ�� ����
        /// </summary>
        [Flags]
        public enum TargetFilter
        {
            None = 0,
            Self = 1 << 0,
            Ally = 1 << 1,
            Enemy = 1 << 2,
            Neutral = 1 << 3,
            Dead = 1 << 4,
            Alive = 1 << 5
        }
        #endregion

        #region Layer Masks
        /// <summary>
        /// ���� ���̾� �̸�
        /// </summary>
        public static class Layers
        {
            public const string PLAYER = "Player";
            public const string ENEMY = "Enemy";
            public const string PROJECTILE = "Projectile";
            public const string EFFECT_ZONE = "EffectZone";
            public const string INTERACTABLE = "Interactable";
        }
        #endregion

        #region Animation Parameters
        /// <summary>
        /// �ִϸ��̼� �Ķ���� �̸�
        /// </summary>
        public static class AnimationParams
        {
            public const string IS_CASTING = "IsCasting";
            public const string IS_CHANNELING = "IsChanneling";
            public const string IS_STUNNED = "IsStunned";
            public const string IS_MOVING = "IsMoving";
            public const string ABILITY_INDEX = "AbilityIndex";
            public const string CAST_SPEED = "CastSpeed";
            public const string MOVEMENT_SPEED = "MovementSpeed";
        }
        #endregion
    }
}