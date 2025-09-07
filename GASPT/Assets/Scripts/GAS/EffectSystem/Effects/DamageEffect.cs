// ================================
// File: Assets/Scripts/GAS/EffectSystem/Effects/DamageEffect.cs
// ������ ����Ʈ ����
// ================================
using UnityEngine;
using GAS.AttributeSystem;
using GAS.TagSystem;
using static GAS.Core.GASConstants;

namespace GAS.EffectSystem.Effects
{
    /// <summary>
    /// ��� �������� �ִ� ����Ʈ
    /// </summary>
    [CreateAssetMenu(fileName = "DamageEffect", menuName = "GAS/Effects/Damage")]
    public class DamageEffect : InstantEffect
    {
        [Header("Damage Settings")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private bool canCritical = true;
        [SerializeField] private float criticalMultiplier = 2f;

        [Header("Scaling")]
        [SerializeField] private bool scaleWithSourcePower = true;
        [SerializeField] private AttributeType scalingAttribute = AttributeType.AttackPower;
        [SerializeField] private float scalingFactor = 0.01f; // 1% per point

        [Header("Visual")]
        [SerializeField] private GameObject damageNumberPrefab;
        [SerializeField] private Color normalDamageColor = Color.white;
        [SerializeField] private Color criticalDamageColor = Color.yellow;

        public override void InitializeEffect()
        {
            base.InitializeEffect();

            effectType = EffectType.Instant;
            durationPolicy = DurationPolicy.Instant;

            if (string.IsNullOrEmpty(effectName))
                effectName = "Damage";
        }

        public override void OnApplication(EffectContext context)
        {
            base.OnApplication(context);

            if (context.target == null) return;

            // ������ ���
            float finalDamage = CalculateDamage(context);

            // ũ��Ƽ�� üũ
            bool isCritical = false;
            if (canCritical)
            {
                isCritical = CheckCritical(context);
                if (isCritical)
                {
                    finalDamage *= criticalMultiplier;
                }
            }

            // ���� ���
            finalDamage = ApplyDefense(context, finalDamage);

            // ������ ����
            ApplyDamage(context, finalDamage, isCritical);

            // �ð� ȿ��
            ShowDamageNumber(context, finalDamage, isCritical);

            if (debugMode)
            {
                Debug.Log($"[DamageEffect] Dealt {finalDamage} damage to {context.target.name} (Critical: {isCritical})");
            }
        }

        private float CalculateDamage(EffectContext context)
        {
            float damage = baseDamage;

            // ���ؽ�Ʈ�� power �� ���
            if (context.power > 0)
            {
                damage = context.power;
            }

            // �ҽ��� ���ݷ� �����ϸ�
            if (scaleWithSourcePower && context.source != null)
            {
                var sourceAttributes = context.source.GetComponent<AttributeSetComponent>();
                if (sourceAttributes != null)
                {
                    float powerValue = sourceAttributes.GetAttributeValue(scalingAttribute);
                    damage *= (1f + powerValue * scalingFactor);
                }
            }

            // ���� �����ϸ�
            if (context.level > 1)
            {
                damage *= (1f + (context.level - 1) * 0.1f); // ������ 10% ����
            }

            return damage;
        }

        private bool CheckCritical(EffectContext context)
        {
            if (context.source == null) return false;

            var sourceAttributes = context.source.GetComponent<AttributeSetComponent>();
            if (sourceAttributes != null)
            {
                float critChance = sourceAttributes.GetAttributeValue(AttributeType.CriticalChance);
                return Random.Range(0f, 100f) < critChance;
            }

            return false;
        }

        private float ApplyDefense(EffectContext context, float damage)
        {
            var targetAttributes = context.target.GetComponent<AttributeSetComponent>();
            if (targetAttributes == null) return damage;

            float defense = 0f;

            switch (damageType)
            {
                case DamageType.Physical:
                    defense = targetAttributes.GetAttributeValue(AttributeType.Defense);
                    break;
                case DamageType.Magical:
                    defense = targetAttributes.GetAttributeValue(AttributeType.MagicDefense);
                    break;
            }

            // ���� ����: damage * (100 / (100 + defense))
            float damageReduction = 100f / (100f + defense);
            return damage * damageReduction;
        }

        private void ApplyDamage(EffectContext context, float damage, bool isCritical)
        {
            var targetAttributes = context.target.GetComponent<AttributeSetComponent>();
            if (targetAttributes == null) return;

            // ü�� ����
            targetAttributes.ModifyAttribute(AttributeType.Health, -damage);

            // �̺�Ʈ �߻�
            Core.GASEvents.Trigger(Core.GASEventType.DamageDealt,
                new Core.DamageEventData
                {
                    source = context.source,
                    target = context.target,
                    damage = damage,
                    damageType = damageType,
                    isCritical = isCritical
                });
        }

        private void ShowDamageNumber(EffectContext context, float damage, bool isCritical)
        {
            if (damageNumberPrefab == null) return;

            Vector3 spawnPosition = context.target.transform.position + Vector3.up * 1.5f;
            GameObject damageNumber = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);

            // �ؽ�Ʈ ����
            var textMesh = damageNumber.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = Mathf.RoundToInt(damage).ToString();
                textMesh.color = isCritical ? criticalDamageColor : normalDamageColor;

                if (isCritical)
                {
                    textMesh.fontSize *= 1.5f;
                }
            }

            // �ִϸ��̼� (���� �ö󰡸� �����)
            var floatingText = damageNumber.AddComponent<FloatingText>();
            floatingText.Initialize(1.5f, Vector3.up * 2f);

            Destroy(damageNumber, 2f);
        }
    }
}