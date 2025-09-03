// 파일 위치: Assets/Scripts/GAS/Core/GASConstants.cs
using System;

namespace GAS.Core
{
    /// <summary>
    /// GAS 시스템 전체에서 사용되는 상수 정의
    /// </summary>
    public static class GASConstants
    {
        #region System Constants
        /// <summary>
        /// 최대 효과 스택 수
        /// </summary>
        public const int MAX_EFFECT_STACKS = 99;

        /// <summary>
        /// 기본 틱 레이트 (초)
        /// </summary>
        public const float DEFAULT_TICK_RATE = 0.1f;

        /// <summary>
        /// 최소 속성값
        /// </summary>
        public const float MIN_ATTRIBUTE_VALUE = 0f;

        /// <summary>
        /// 최대 속성값 (기본)
        /// </summary>
        public const float DEFAULT_MAX_ATTRIBUTE_VALUE = 9999f;
        #endregion

        #region Tag Categories
        /// <summary>
        /// 태그 카테고리 접두사
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
        /// 자주 사용되는 태그 정의
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
        /// 기본 속성 타입
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
        /// 효과 타입
        /// </summary>
        public enum EffectType
        {
            Instant,
            Duration,
            Infinite,
            Periodic
        }

        /// <summary>
        /// 효과 스택 정책
        /// </summary>
        public enum EffectStackPolicy
        {
            None,           // 스택 불가
            Stack,          // 효과 중첩
            Refresh,        // 지속시간 갱신
            StackAndRefresh // 중첩 + 갱신
        }

        /// <summary>
        /// 효과 제거 정책
        /// </summary>
        public enum EffectRemovalPolicy
        {
            OnExpire,       // 시간 만료 시
            OnDeath,        // 사망 시
            OnCleanse,      // 정화 시
            Manual,         // 수동 제거만
            Never           // 제거 불가
        }
        #endregion

        #region Ability Types
        /// <summary>
        /// 능력 타입
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
        /// 능력 활성화 실패 이유
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
        /// 속성 수정 연산 타입
        /// </summary>
        public enum ModifierOperation
        {
            Add,            // 더하기
            Subtract,       // 빼기
            Multiply,       // 곱하기
            Divide,         // 나누기
            Override,       // 덮어쓰기
            AddPercent,     // 퍼센트 증가
            SubtractPercent // 퍼센트 감소
        }

        /// <summary>
        /// 수정자 적용 순서
        /// </summary>
        public enum ModifierPriority
        {
            VeryLow = -100,
            Low = -50,
            Normal = 0,
            High = 50,
            VeryHigh = 100,
            Override = 1000  // 항상 마지막에 적용
        }
        #endregion

        #region Damage Types
        /// <summary>
        /// 데미지 타입
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
        /// 타겟팅 모드
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
        /// 타겟 필터
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
        /// 물리 레이어 이름
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
        /// 애니메이션 파라미터 이름
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