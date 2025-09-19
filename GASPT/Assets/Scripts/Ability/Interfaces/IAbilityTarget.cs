// 파일 위치: Assets/Scripts/Ability/Interfaces/IAbilityTarget.cs
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// 어빌리티의 대상이 될 수 있는 객체가 구현해야 하는 인터페이스
    /// </summary>
    public interface IAbilityTarget
    {
        // 기본 정보
        GameObject GameObject { get; }
        Transform Transform { get; }
        bool IsAlive { get; }
        bool IsTargetable { get; }

        // 체력 시스템
        float CurrentHealth { get; }
        float MaxHealth { get; }

        // 데미지/힐 처리
        void TakeDamage(float damage);
        void TakeDamage(float damage, GameObject attacker);
        void TakeDamage(float damage, GameObject attacker, DamageType damageType);
        void Heal(float amount);
        void Heal(float amount, GameObject healer);

        // 버프/디버프
        void ApplyBuff(BuffType buffType, float duration, float value);
        void RemoveBuff(BuffType buffType);

        // 상태 이상
        void ApplyStatusEffect(StatusEffectType effectType, float duration);
        void RemoveStatusEffect(StatusEffectType effectType);

        // 넉백
        void ApplyKnockback(Vector2 force);
        void ApplyKnockback(Vector2 direction, float power);
    }

    /// <summary>
    /// 데미지 타입 열거형
    /// </summary>
    public enum DamageType
    {
        Physical,
        Magical,
        True,
        Fire,
        Ice,
        Lightning,
        Poison,
        Dark,
        Holy
    }

    /// <summary>
    /// 버프 타입 열거형
    /// </summary>
    public enum BuffType
    {
        AttackPower,
        DefensePower,
        MoveSpeed,
        AttackSpeed,
        CriticalRate,
        CriticalDamage,
        LifeSteal,
        DamageReduction,
        CooldownReduction
    }

    /// <summary>
    /// 상태 이상 타입 열거형
    /// </summary>
    public enum StatusEffectType
    {
        None,
        Stun,
        Slow,
        Freeze,
        Burn,
        Poison,
        Bleed,
        Silence,
        Blind,
        Root
    }

    /// <summary>
    /// IAbilityTarget의 기본 구현체
    /// </summary>
    public abstract class AbilityTargetBase : MonoBehaviour, IAbilityTarget
    {
        [Header("Target Settings")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth = 100f;
        [SerializeField] protected bool isTargetable = true;

        // IAbilityTarget 구현
        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public bool IsAlive => currentHealth > 0;
        public bool IsTargetable => isTargetable && IsAlive;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        // Abstract methods - 각 구현체에서 정의
        public abstract void TakeDamage(float damage);
        public abstract void TakeDamage(float damage, GameObject attacker);
        public abstract void TakeDamage(float damage, GameObject attacker, DamageType damageType);
        public abstract void Heal(float amount);
        public abstract void Heal(float amount, GameObject healer);
        public abstract void ApplyBuff(BuffType buffType, float duration, float value);
        public abstract void RemoveBuff(BuffType buffType);
        public abstract void ApplyStatusEffect(StatusEffectType effectType, float duration);
        public abstract void RemoveStatusEffect(StatusEffectType effectType);
        public abstract void ApplyKnockback(Vector2 force);
        public abstract void ApplyKnockback(Vector2 direction, float power);

        protected virtual void Start()
        {
            currentHealth = maxHealth;
        }
    }
}