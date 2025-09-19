// ���� ��ġ: Assets/Scripts/Ability/Interfaces/IAbilityTarget.cs
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// �����Ƽ�� ����� �� �� �ִ� ��ü�� �����ؾ� �ϴ� �������̽�
    /// </summary>
    public interface IAbilityTarget
    {
        // �⺻ ����
        GameObject GameObject { get; }
        Transform Transform { get; }
        bool IsAlive { get; }
        bool IsTargetable { get; }

        // ü�� �ý���
        float CurrentHealth { get; }
        float MaxHealth { get; }

        // ������/�� ó��
        void TakeDamage(float damage);
        void TakeDamage(float damage, GameObject attacker);
        void TakeDamage(float damage, GameObject attacker, DamageType damageType);
        void Heal(float amount);
        void Heal(float amount, GameObject healer);

        // ����/�����
        void ApplyBuff(BuffType buffType, float duration, float value);
        void RemoveBuff(BuffType buffType);

        // ���� �̻�
        void ApplyStatusEffect(StatusEffectType effectType, float duration);
        void RemoveStatusEffect(StatusEffectType effectType);

        // �˹�
        void ApplyKnockback(Vector2 force);
        void ApplyKnockback(Vector2 direction, float power);
    }

    /// <summary>
    /// ������ Ÿ�� ������
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
    /// ���� Ÿ�� ������
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
    /// ���� �̻� Ÿ�� ������
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
    /// IAbilityTarget�� �⺻ ����ü
    /// </summary>
    public abstract class AbilityTargetBase : MonoBehaviour, IAbilityTarget
    {
        [Header("Target Settings")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth = 100f;
        [SerializeField] protected bool isTargetable = true;

        // IAbilityTarget ����
        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public bool IsAlive => currentHealth > 0;
        public bool IsTargetable => isTargetable && IsAlive;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        // Abstract methods - �� ����ü���� ����
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