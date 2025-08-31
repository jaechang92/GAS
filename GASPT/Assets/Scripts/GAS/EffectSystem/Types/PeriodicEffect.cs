using GAS.AttributeSystem;
using GAS.Core;
using GAS.TagSystem;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.EffectSystem
{
    /// <summary>
    /// 주기적으로 효과가 발생하는 GameplayEffect
    /// 예: 독, 화상, 재생, DOT(Damage Over Time), HOT(Heal Over Time)
    /// </summary>
    [CreateAssetMenu(fileName = "PeriodicEffect", menuName = "GAS/Effects/Periodic Effect", order = 3)]
    public class PeriodicEffect : DurationEffect
    {
        #region Additional Fields

        [Header("=== Periodic Settings ===")]
        [SerializeField] private int maxTicks = -1; // -1 = 무제한
        [SerializeField] private bool executeOnFirstApplication = true;
        [SerializeField] private bool executeOnLastTick = true;
        [SerializeField] private bool scaleTickWithStack = true;

        [Header("=== Tick Modifiers ===")]
        [SerializeField] private List<ModifierConfig> tickModifiers = new List<ModifierConfig>();
        [SerializeField] private AnimationCurve tickDamageCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Header("=== Tick Effects ===")]
        [SerializeField] private List<InstantEffect> onTickEffects = new List<InstantEffect>();
        [SerializeField] private bool useRandomTickEffect = false;

        [Header("=== Tick Visuals ===")]
        [SerializeField] private GameObject tickVisualPrefab;
        [SerializeField] private AudioClip tickSound;
        [SerializeField] private float tickVisualDuration = 0.5f;

        [Header("=== Advanced Periodic ===")]
        [SerializeField] private bool variablePeriod = false;
        [SerializeField] private float periodVariance = 0.1f;
        [SerializeField] private bool accelerateOverTime = false;
        [SerializeField] private float accelerationRate = 0.9f;

        #endregion

        #region Constructor

        public PeriodicEffect()
        {
            effectType = EffectType.Periodic;
            durationPolicy = EffectDurationPolicy.HasDuration;
            period = 1f; // 기본 1초마다 틱
            duration = 5f; // 기본 5초 지속
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Periodic 효과 적용
        /// </summary>
        public override bool Apply(EffectContext context, GameObject target)
        {
            if (!CanApply(context, target))
            {
                Debug.LogWarning($"[PeriodicEffect] Cannot apply {effectName} to {target.name}");
                return false;
            }

            try
            {
                // 기존 인스턴스 확인
                EffectInstance existingInstance = FindExistingInstance(target);

                if (existingInstance != null)
                {
                    return HandleStacking(existingInstance, context, target);
                }

                // 새 인스턴스 생성
                var instance = CreatePeriodicInstance(context, target);

                // 초기 Modifier 적용 (Duration 효과)
                if (modifiers.Count > 0)
                {
                    ApplyModifiers(instance, context, target);
                }

                // 태그 부여
                GrantTags(target);

                // 시각/청각 효과
                PlayApplicationEffects(instance, target);

                // 첫 틱 즉시 실행
                if (executeOnFirstApplication)
                {
                    ExecutePeriodicTick(instance, context, target);
                }

                // Periodic 추적 시작
                StartPeriodicTracking(instance, target);

                // 이벤트 발생
                TriggerApplicationEvents(instance, target);

                // 인스턴스 저장
                StoreInstance(target, instance);

                int expectedTicks = CalculateExpectedTicks(instance);
                Debug.Log($"[PeriodicEffect] Applied {effectName} to {target.name} - Period: {period}s, Expected ticks: {expectedTicks}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PeriodicEffect] Failed to apply {effectName}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 주기적 실행 (DurationEffect override)
        /// </summary>
        public override bool ExecutePeriodic(EffectContext context, GameObject target)
        {
            var instance = FindExistingInstance(target);
            if (instance == null)
            {
                Debug.LogWarning($"[PeriodicEffect] No instance found for periodic execution");
                return false;
            }

            return ExecutePeriodicTick(instance, context, target);
        }

        #endregion

        #region Private Methods - Periodic Logic

        /// <summary>
        /// Periodic 인스턴스 생성
        /// </summary>
        private EffectInstance CreatePeriodicInstance(EffectContext context, GameObject target)
        {
            var instance = new EffectInstance(this, context);

            // Duration 계산
            float finalDuration = CalculateDuration(context);
            instance.RemainingDuration = finalDuration;

            // 다음 틱 시간 설정
            float nextPeriod = CalculateNextPeriod(0);
            instance.NextPeriodicTime = Time.time + nextPeriod;

            return instance;
        }

        /// <summary>
        /// Periodic 추적 시작
        /// </summary>
        private async void StartPeriodicTracking(EffectInstance instance, GameObject target)
        {
            var cts = new CancellationTokenSource();
            instance.Context.SetData("PeriodicCancellationToken", cts);

            try
            {
                float updateInterval = 0.05f; // 50ms 정밀도
                float currentPeriod = period;
                int tickCount = executeOnFirstApplication ? 1 : 0;

                while (!instance.IsExpired && instance.RemainingDuration > 0)
                {
                    await Awaitable.WaitForSecondsAsync(updateInterval, cts.Token);

                    if (target == null || instance.IsExpired)
                        break;

                    // Duration 감소
                    instance.RemainingDuration -= updateInterval;

                    // 주기 체크
                    if (Time.time >= instance.NextPeriodicTime)
                    {
                        // 최대 틱 체크
                        if (maxTicks > 0 && tickCount >= maxTicks)
                        {
                            Debug.Log($"[PeriodicEffect] Max ticks ({maxTicks}) reached for {effectName}");
                            break;
                        }

                        // 틱 실행
                        bool success = ExecutePeriodicTick(instance, instance.Context, target);
                        if (!success)
                        {
                            Debug.LogWarning($"[PeriodicEffect] Tick execution failed for {effectName}");
                        }

                        tickCount++;

                        // 다음 주기 계산
                        currentPeriod = CalculateNextPeriod(tickCount);
                        instance.NextPeriodicTime = Time.time + currentPeriod;

                        // 가속 적용
                        if (accelerateOverTime)
                        {
                            currentPeriod *= accelerationRate;
                            currentPeriod = Mathf.Max(0.1f, currentPeriod);
                        }
                    }

                    // 만료 직전 마지막 틱
                    if (instance.RemainingDuration <= updateInterval && executeOnLastTick)
                    {
                        if (tickCount == 0 || Time.time - instance.NextPeriodicTime + currentPeriod > updateInterval)
                        {
                            ExecutePeriodicTick(instance, instance.Context, target);
                            Debug.Log($"[PeriodicEffect] Final tick executed for {effectName}");
                        }
                    }

                    // 만료 체크
                    if (instance.RemainingDuration <= 0)
                    {
                        await HandleExpiration(instance, target);
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[PeriodicEffect] Periodic tracking cancelled for {effectName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[PeriodicEffect] Error in periodic tracking: {e.Message}");
            }
            finally
            {
                cts?.Dispose();
            }
        }

        /// <summary>
        /// 주기적 틱 실행
        /// </summary>
        private bool ExecutePeriodicTick(EffectInstance instance, EffectContext context, GameObject target)
        {
            if (target == null) return false;

            instance.IncrementPeriodicCount();

            Debug.Log($"[PeriodicEffect] Tick #{instance.PeriodicExecutionCount} for {effectName} on {target.name}");

            try
            {
                // 틱 강도 계산
                float tickMagnitude = CalculateTickMagnitude(instance, context);

                // 1. Tick Modifiers 적용
                ApplyTickModifiers(target, tickMagnitude, instance);

                // 2. Tick Effects 적용
                ApplyTickEffects(target, context, instance);

                // 3. 시각/청각 효과
                PlayTickEffects(target);

                // 4. 이벤트 발생
                TriggerTickEvent(instance, target, tickMagnitude);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PeriodicEffect] Failed to execute tick: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 틱 강도 계산
        /// </summary>
        private float CalculateTickMagnitude(EffectInstance instance, EffectContext context)
        {
            float magnitude = context.Magnitude;

            // 스택 기반 scaling
            if (scaleTickWithStack && instance.CurrentStack > 1)
            {
                magnitude *= instance.CurrentStack;
            }

            // 시간 기반 curve
            if (tickDamageCurve != null && tickDamageCurve.length > 0)
            {
                float progress = instance.Progress;
                magnitude *= tickDamageCurve.Evaluate(progress);
            }

            return magnitude;
        }

        /// <summary>
        /// Tick Modifiers 적용
        /// </summary>
        private void ApplyTickModifiers(GameObject target, float magnitude, EffectInstance instance)
        {
            if (tickModifiers == null || tickModifiers.Count == 0) return;

            var attributeComponent = target.GetComponent<IAttributeComponent>();
            if (attributeComponent == null) return;

            foreach (var config in tickModifiers)
            {
                if (config == null) continue;

                // 즉시 적용 modifier (tick은 영구적이지 않음)
                float currentValue = attributeComponent.GetAttributeValue(config.attributeType);
                float modifiedValue = currentValue;

                switch (config.operation)
                {
                    case ModifierOperation.Add:
                        modifiedValue = currentValue + (config.value * magnitude);
                        break;
                    case ModifierOperation.Multiply:
                        modifiedValue = currentValue * (config.value * magnitude);
                        break;
                    case ModifierOperation.Override:
                        modifiedValue = config.value * magnitude;
                        break;
                }

                // AttributeSetComponent의 BaseValue 직접 변경
                if (attributeComponent is AttributeSetComponent attrComponent)
                {
                    var attributeSet = attrComponent.GetAttributeSet<AttributeSet>();
                    BaseAttribute attribute = attributeSet.GetAttribute(config.attributeType);
                    if (attributeSet != null && attribute != null)
                    {
                        attribute.BaseValue = modifiedValue;

                        // 변경 이벤트
                        GASEvents.TriggerAttributeChanged(
                            target,
                            config.attributeType.ToString(),
                            currentValue,
                            modifiedValue
                        );

                        // 틱 데미지/힐 로그
                        float changeAmount = modifiedValue - currentValue;
                        string changeType = changeAmount < 0 ? "damage" : "heal";
                        Debug.Log($"[PeriodicEffect] Tick {changeType}: {config.attributeType} {changeAmount:+0.##}");
                    }
                }
            }
        }

        /// <summary>
        /// Tick Effects 적용
        /// </summary>
        private void ApplyTickEffects(GameObject target, EffectContext context, EffectInstance instance)
        {
            if (onTickEffects == null || onTickEffects.Count == 0) return;

            if (useRandomTickEffect)
            {
                // 랜덤하게 하나 선택
                int randomIndex = UnityEngine.Random.Range(0, onTickEffects.Count);
                var effect = onTickEffects[randomIndex];
                if (effect != null)
                {
                    var tickContext = context.Clone();
                    tickContext.SetData("TickNumber", instance.PeriodicExecutionCount);
                    effect.Apply(tickContext, target);
                }
            }
            else
            {
                // 모든 tick effect 적용
                foreach (var effect in onTickEffects)
                {
                    if (effect != null)
                    {
                        var tickContext = context.Clone();
                        tickContext.SetData("TickNumber", instance.PeriodicExecutionCount);
                        effect.Apply(tickContext, target);
                    }
                }
            }
        }

        /// <summary>
        /// 다음 주기 계산
        /// </summary>
        private float CalculateNextPeriod(int tickCount)
        {
            float nextPeriod = period;

            // 가변 주기
            if (variablePeriod)
            {
                float variance = UnityEngine.Random.Range(-periodVariance, periodVariance);
                nextPeriod += nextPeriod * variance;
            }

            // 가속
            if (accelerateOverTime && tickCount > 0)
            {
                nextPeriod *= Mathf.Pow(accelerationRate, tickCount);
            }

            return Mathf.Max(0.1f, nextPeriod);
        }

        /// <summary>
        /// 예상 틱 횟수 계산
        /// </summary>
        private int CalculateExpectedTicks(EffectInstance instance)
        {
            if (maxTicks > 0) return maxTicks;

            int ticks = Mathf.FloorToInt(instance.RemainingDuration / period);
            if (executeOnFirstApplication) ticks++;
            if (executeOnLastTick && instance.RemainingDuration % period > 0.01f) ticks++;

            return ticks;
        }

        #endregion

        #region Private Methods - Effects

        /// <summary>
        /// 틱 효과 재생
        /// </summary>
        private void PlayTickEffects(GameObject target)
        {
            // 시각 효과
            if (tickVisualPrefab != null)
            {
                var vfx = Instantiate(tickVisualPrefab, target.transform.position, Quaternion.identity);
                vfx.transform.SetParent(target.transform);
                Destroy(vfx, tickVisualDuration);
            }

            // 사운드
            if (tickSound != null)
            {
                PlaySound(tickSound, target.transform.position);
            }
        }

        /// <summary>
        /// 틱 이벤트 발생
        /// </summary>
        private void TriggerTickEvent(EffectInstance instance, GameObject target, float magnitude)
        {
            Debug.Log($"[PeriodicEffect] Tick event: {effectName} - Tick #{instance.PeriodicExecutionCount}, Magnitude: {magnitude:F2}");

            // 데미지/힐 판정
            bool isDamage = false;
            bool isHeal = false;

            foreach (var modifier in tickModifiers)
            {
                if (modifier.attributeType == AttributeType.Health)
                {
                    if (modifier.value < 0) isDamage = true;
                    else if (modifier.value > 0) isHeal = true;
                }
            }

            if (isDamage)
            {
                Debug.Log($"[PeriodicEffect] DOT tick: {effectName} - Damage: {magnitude:F2}");
            }
            else if (isHeal)
            {
                Debug.Log($"[PeriodicEffect] HOT tick: {effectName} - Healing: {magnitude:F2}");
            }
        }

        #endregion

        #region Override Stacking

        /// <summary>
        /// 스택 처리 (override)
        /// </summary>
        protected override bool HandleStacking(EffectInstance existingInstance, EffectContext context, GameObject target)
        {
            bool result = base.HandleStacking(existingInstance, context, target);

            if (result && resetPeriodicOnStack)
            {
                // Periodic 타이머 리셋
                existingInstance.ResetPeriodicTimer();
                existingInstance.Context.SetData("PeriodicExecutionCount", 0);

                Debug.Log($"[PeriodicEffect] Stack applied, periodic timer reset for {effectName}");
            }

            return result;
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// DOT(Damage Over Time) Effect 생성
        /// </summary>
        public static PeriodicEffect CreateDOT(string name, float damage, float duration, float period)
        {
            var effect = CreateInstance<PeriodicEffect>();
            effect.effectName = name;
            effect.effectId = Guid.NewGuid().ToString();
            effect.duration = duration;
            effect.period = period;
            effect.tickModifiers = new List<ModifierConfig>
            {
                new ModifierConfig(AttributeType.Health, ModifierOperation.Add, -damage, 0)
            };
            return effect;
        }

        /// <summary>
        /// HOT(Heal Over Time) Effect 생성
        /// </summary>
        public static PeriodicEffect CreateHOT(string name, float healing, float duration, float period)
        {
            var effect = CreateInstance<PeriodicEffect>();
            effect.effectName = name;
            effect.effectId = Guid.NewGuid().ToString();
            effect.duration = duration;
            effect.period = period;
            effect.tickModifiers = new List<ModifierConfig>
            {
                new ModifierConfig(AttributeType.Health, ModifierOperation.Add, healing, 0)
            };
            return effect;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            // PeriodicEffect 강제 설정
            effectType = EffectType.Periodic;

            // Period 검증
            if (period <= 0f) period = 1f;

            // Duration이 period보다 짧으면 조정
            if (duration < period && duration > 0)
            {
                duration = period * 3; // 최소 3틱
            }

            // Max ticks 검증
            if (maxTicks == 0) maxTicks = -1;

            // Acceleration 검증
            if (accelerateOverTime)
            {
                accelerationRate = Mathf.Clamp(accelerationRate, 0.1f, 1f);
            }

            // Period variance 검증
            periodVariance = Mathf.Clamp01(periodVariance);
        }
#endif

        #endregion
    }
}

// 파일 위치: Assets/Scripts/GAS/EffectSystem/Types/PeriodicEffect.cs