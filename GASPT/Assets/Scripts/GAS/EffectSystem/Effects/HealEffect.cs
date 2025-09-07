// ================================
// File: Assets/Scripts/GAS/EffectSystem/Effects/HealEffect.cs
// 힐 이펙트 구현
// ================================
using UnityEngine;
using GAS.AttributeSystem;
using static GAS.Core.GASConstants;

namespace GAS.EffectSystem.Effects
{
    /// <summary>
    /// 즉시 체력을 회복하는 이펙트
    /// </summary>
    [CreateAssetMenu(fileName = "HealEffect", menuName = "GAS/Effects/Heal")]
    public class HealEffect : InstantEffect
    {
        [Header("Heal Settings")]
        [SerializeField] private float baseHealAmount = 20f;
        [SerializeField] private bool healPercentage = false;
        [SerializeField] private float healPercentageAmount = 0.25f; // 25%

        [Header("Scaling")]
        [SerializeField] private bool scaleWithSourcePower = true;
        [SerializeField] private AttributeType scalingAttribute = AttributeType.HealingPower;
        [SerializeField] private float scalingFactor = 0.01f;

        [Header("Overheal")]
        [SerializeField] private bool allowOverheal = false;
        [SerializeField] private float overhealDecayRate = 1f; // per second

        [Header("Visual")]
        [SerializeField] private GameObject healEffectPrefab;
        [SerializeField] private GameObject healNumberPrefab;
        [SerializeField] private Color healColor = Color.green;

        public override void InitializeEffect()
        {
            base.InitializeEffect();

            effectType = EffectType.Instant;
            durationPolicy = DurationPolicy.Instant;

            if (string.IsNullOrEmpty(effectName))
                effectName = "Heal";
        }

        public override void OnApplication(EffectContext context)
        {
            base.OnApplication(context);

            if (context.target == null) return;

            var targetAttributes = context.target.GetComponent<AttributeSetComponent>();
            if (targetAttributes == null) return;

            // 힐량 계산
            float healAmount = CalculateHealAmount(context, targetAttributes);

            // 현재 체력과 최대 체력
            float currentHealth = targetAttributes.GetAttributeValue(AttributeType.Health);
            float maxHealth = targetAttributes.GetAttributeValue(AttributeType.HealthMax);

            // 힐 적용
            float actualHeal = healAmount;
            if (!allowOverheal)
            {
                actualHeal = Mathf.Min(healAmount, maxHealth - currentHealth);
            }

            targetAttributes.ModifyAttribute(AttributeType.Health, actualHeal);

            // 오버힐 처리
            if (allowOverheal && currentHealth + actualHeal > maxHealth)
            {
                ApplyOverheal(context, currentHealth + actualHeal - maxHealth);
            }

            // 시각 효과
            ShowHealEffect(context, actualHeal);

            // 이벤트 발생
            Core.GASEvents.Trigger(Core.GASEventType.HealingApplied,
                new Core.HealingEventData
                {
                    source = context.source,
                    target = context.target,
                    healing = actualHeal,
                    isOverheal = actualHeal > maxHealth - currentHealth
                });

            if (debugMode)
            {
                Debug.Log($"[HealEffect] Healed {actualHeal} HP to {context.target.name}");
            }
        }

        private float CalculateHealAmount(EffectContext context, AttributeSetComponent targetAttributes)
        {
            float healAmount = baseHealAmount;

            // 퍼센트 힐
            if (healPercentage)
            {
                float maxHealth = targetAttributes.GetAttributeValue(AttributeType.HealthMax);
                healAmount = maxHealth * healPercentageAmount;
            }

            // 컨텍스트 power 사용
            if (context.power > 0)
            {
                healAmount = context.power;
            }

            // 소스의 힐 파워 스케일링
            if (scaleWithSourcePower && context.source != null)
            {
                var sourceAttributes = context.source.GetComponent<AttributeSetComponent>();
                if (sourceAttributes != null)
                {
                    float healPower = sourceAttributes.GetAttributeValue(scalingAttribute);
                    healAmount *= (1f + healPower * scalingFactor);
                }
            }

            return healAmount;
        }

        private void ApplyOverheal(EffectContext context, float overhealAmount)
        {
            // 오버힐을 임시 체력으로 처리
            // TODO: TempHealth 어트리뷰트 추가 필요
        }

        private void ShowHealEffect(EffectContext context, float healAmount)
        {
            // 힐 이펙트
            if (healEffectPrefab != null)
            {
                GameObject effect = Instantiate(healEffectPrefab, context.target.transform.position, Quaternion.identity);
                effect.transform.SetParent(context.target.transform);
                Destroy(effect, 2f);
            }

            // 힐 숫자
            if (healNumberPrefab != null)
            {
                Vector3 spawnPosition = context.target.transform.position + Vector3.up * 1.5f;
                GameObject healNumber = Instantiate(healNumberPrefab, spawnPosition, Quaternion.identity);

                var textMesh = healNumber.GetComponentInChildren<TMPro.TextMeshPro>();
                if (textMesh != null)
                {
                    textMesh.text = "+" + Mathf.RoundToInt(healAmount);
                    textMesh.color = healColor;
                }

                var floatingText = healNumber.AddComponent<FloatingText>();
                floatingText.Initialize(1.5f, Vector3.up * 2f);

                Destroy(healNumber, 2f);
            }
        }
    }

    /// <summary>
    /// 떠오르는 텍스트 컴포넌트
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        private float duration;
        private Vector3 movement;
        private float elapsedTime;

        public void Initialize(float duration, Vector3 movement)
        {
            this.duration = duration;
            this.movement = movement;
            this.elapsedTime = 0f;
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // 이동
            transform.position += movement * Time.deltaTime * (1f - t);

            // 페이드 아웃
            var textMesh = GetComponentInChildren<TMPro.TextMeshPro>();
            if (textMesh != null)
            {
                Color color = textMesh.color;
                color.a = 1f - t;
                textMesh.color = color;
            }

            // 스케일 감소
            transform.localScale = Vector3.one * (1f - t * 0.5f);
        }
    }
}