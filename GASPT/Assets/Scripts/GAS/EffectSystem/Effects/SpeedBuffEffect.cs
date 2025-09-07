// ================================
// File: Assets/Scripts/GAS/EffectSystem/Effects/SpeedBuffEffect.cs
// 이동속도 버프 이펙트
// ================================
using GAS.AttributeSystem;
using GAS.TagSystem;
using UnityEngine;
using static GAS.Core.GASConstants;
using static GAS.Learning.Phase1.ModifierTestExample;

namespace GAS.EffectSystem.Effects
{
    /// <summary>
    /// 일정 시간 동안 이동속도를 증가시키는 버프
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedBuffEffect", menuName = "GAS/Effects/Speed Buff")]
    public class SpeedBuffEffect : DurationEffect
    {
        [Header("Speed Buff Settings")]
        [SerializeField] private float speedIncreaseAmount = 5f;
        [SerializeField] private bool usePercentage = true;
        [SerializeField] private float speedIncreasePercentage = 50f; // 50% 증가

        [Header("Visual")]
        [SerializeField] private GameObject buffVisualPrefab;
        [SerializeField] private Color buffAuraColor = new Color(0, 1, 1, 0.5f);
        [SerializeField] private bool showSpeedLines = true;

        private GameObject visualEffect;

        public override void InitializeEffect()
        {
            base.InitializeEffect();

            effectType = EffectType.Duration;
            durationPolicy = DurationPolicy.Duration;

            if (duration <= 0)
                duration = 5f;

            if (string.IsNullOrEmpty(effectName))
                effectName = "Speed Buff";

            // 버프 태그 설정
            if (assetTags == null)
                assetTags = ScriptableObject.CreateInstance<TagContainer>();
        }

        public override void OnApplication(EffectContext context)
        {
            base.OnApplication(context);

            if (context.target == null) return;

            // 이동속도 증가 모디파이어 적용
            ApplySpeedModifier(context);

            // 시각 효과 생성
            CreateVisualEffect(context);

            // 버프 태그 추가
            AddBuffTag(context);

            if (debugMode)
            {
                float increase = usePercentage ? speedIncreasePercentage : speedIncreaseAmount;
                string unit = usePercentage ? "%" : "";
                Debug.Log($"[SpeedBuff] Applied +{increase}{unit} speed to {context.target.name} for {duration}s");
            }
        }

        public override void OnRemoval(EffectContext context)
        {
            if (context.target == null) return;

            // 이동속도 모디파이어 제거
            RemoveSpeedModifier(context);

            // 시각 효과 제거
            RemoveVisualEffect();

            // 버프 태그 제거
            RemoveBuffTag(context);

            base.OnRemoval(context);

            if (debugMode)
            {
                Debug.Log($"[SpeedBuff] Removed from {context.target.name}");
            }
        }

        public override void OnTick(EffectContext context, float deltaTime)
        {
            base.OnTick(context, deltaTime);

            // 시각 효과 업데이트
            UpdateVisualEffect(context);
        }

        private void ApplySpeedModifier(EffectContext context)
        {
            var attributes = context.target.GetComponent<AttributeSetComponent>();
            if (attributes == null) return;

            AttributeModifier modifier;

            if (usePercentage)
            {
                modifier = new AttributeModifier
                {
                    modifierType = ModifierType.Multiplicative,
                    value = speedIncreasePercentage / 100f,
                    source = this
                };
            }
            else
            {
                modifier = new AttributeModifier
                {
                    modifierType = ModifierType.Additive,
                    value = speedIncreaseAmount,
                    source = this
                };
            }

            attributes.AddModifier(AttributeType.MovementSpeed, modifier);

            // Platform2DController에 직접 적용 (옵션)
            var platformController = context.target.GetComponent<TestScene.Platform2DController>();
            if (platformController != null)
            {
                var field = platformController.GetType().GetField("moveSpeed",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    float currentSpeed = (float)field.GetValue(platformController);
                    float newSpeed = usePercentage ?
                        currentSpeed * (1f + speedIncreasePercentage / 100f) :
                        currentSpeed + speedIncreaseAmount;
                    field.SetValue(platformController, newSpeed);
                }
            }
        }

        private void RemoveSpeedModifier(EffectContext context)
        {
            var attributes = context.target.GetComponent<AttributeSetComponent>();
            if (attributes == null) return;

            attributes.RemoveAllModifiersFromSource(this);

            // Platform2DController 원래 속도로 복구
            var platformController = context.target.GetComponent<TestScene.Platform2DController>();
            if (platformController != null)
            {
                var field = platformController.GetType().GetField("moveSpeed",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    // 기본값으로 복구 (8f)
                    field.SetValue(platformController, 8f);
                }
            }
        }

        private void CreateVisualEffect(EffectContext context)
        {
            if (buffVisualPrefab != null)
            {
                visualEffect = Instantiate(buffVisualPrefab, context.target.transform);
                visualEffect.transform.localPosition = Vector3.zero;
            }
            else
            {
                // 기본 시각 효과 생성
                visualEffect = new GameObject("SpeedBuffEffect");
                visualEffect.transform.SetParent(context.target.transform);
                visualEffect.transform.localPosition = Vector3.zero;

                // 파티클 시스템 추가
                var particles = visualEffect.AddComponent<ParticleSystem>();
                var main = particles.main;
                main.loop = true;
                main.startLifetime = 0.5f;
                main.startSpeed = 2f;
                main.startSize = 0.3f;
                main.startColor = buffAuraColor;

                var shape = particles.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.5f;

                var emission = particles.emission;
                emission.rateOverTime = 20;
            }

            // 스프라이트 아우라
            if (showSpeedLines)
            {
                CreateSpeedLines(context);
            }
        }

        private void CreateSpeedLines(EffectContext context)
        {
            var spriteRenderer = context.target.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null) return;

            // 잔상 효과 생성
            GameObject afterImage = new GameObject("AfterImage");
            afterImage.transform.SetParent(visualEffect.transform);
            afterImage.transform.localPosition = Vector3.zero;

            var afterImageSprite = afterImage.AddComponent<SpriteRenderer>();
            afterImageSprite.sprite = spriteRenderer.sprite;
            afterImageSprite.color = new Color(buffAuraColor.r, buffAuraColor.g, buffAuraColor.b, 0.3f);
            afterImageSprite.sortingOrder = spriteRenderer.sortingOrder - 1;
        }

        private void UpdateVisualEffect(EffectContext context)
        {
            if (visualEffect == null) return;

            // 파티클 색상 펄스 효과
            var particles = visualEffect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var main = particles.main;
                float pulse = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
                Color color = buffAuraColor;
                color.a = 0.3f + pulse * 0.2f;
                main.startColor = color;
            }
        }

        private void RemoveVisualEffect()
        {
            if (visualEffect != null)
            {
                Destroy(visualEffect);
                visualEffect = null;
            }
        }

        private void AddBuffTag(EffectContext context)
        {
            var tagComponent = context.target.GetComponent<TagComponent>();
            if (tagComponent == null) return;

            // "Buff.Speed" 태그 추가
            var buffTag = Resources.Load<GameplayTag>("Tags/Buff_Speed");
            if (buffTag != null)
            {
                tagComponent.AddTag(buffTag, this);
            }
        }

        private void RemoveBuffTag(EffectContext context)
        {
            var tagComponent = context.target.GetComponent<TagComponent>();
            if (tagComponent == null) return;

            tagComponent.RemoveAllTagsFromSource(this);
        }
    }
}