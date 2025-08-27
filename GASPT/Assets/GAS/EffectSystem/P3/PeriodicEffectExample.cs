// 파일 위치: Assets/Scripts/GAS/Learning/Phase3/PeriodicEffectExample.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAS.Learning.Phase3
{
    /// <summary>
    /// Step 3: Periodic Effects - 주기적 효과와 스택 시스템
    /// </summary>
    public class PeriodicEffectExample : MonoBehaviour
    {
        // 주기적 효과에 특화된 Effect
        [Serializable]
        public class PeriodicEffect
        {
            [Header("기본 정보")]
            public string effectName;
            public EffectType effectType;

            public enum EffectType
            {
                DamageOverTime,     // DoT
                HealOverTime,       // HoT
                ResourceDrain,      // 자원 소모
                ResourceRestore,    // 자원 회복
                StatModifier,       // 스탯 변경
                Custom             // 커스텀
            }

            [Header("주기 설정")]
            public float tickInterval = 1f;        // 틱 간격
            public int maxTicks = -1;              // 최대 틱 (-1은 무한)
            public int currentTick = 0;            // 현재 틱
            public float nextTickTime = 0;         // 다음 틱 시간
            public bool tickOnStart = true;        // 시작 시 즉시 틱

            [Header("효과 값")]
            public float baseTickValue;            // 기본 틱당 값
            public float currentTickValue;         // 현재 틱당 값
            public float totalValue;               // 누적 값

            [Header("스택 시스템")]
            public StackBehavior stackBehavior;
            public int currentStacks = 1;
            public int maxStacks = 5;

            public enum StackBehavior
            {
                Independent,        // 각각 독립적으로 틱
                Synchronized,       // 동기화된 틱
                Amplified,         // 값 증폭
                Extended,          // 지속시간 연장
                Accelerated        // 틱 속도 증가
            }

            [Header("스케일링")]
            public ScalingType scalingType;
            public AnimationCurve scalingCurve;
            public float scalingFactor = 1f;

            public enum ScalingType
            {
                None,              // 스케일링 없음
                Linear,            // 선형 증가/감소
                Exponential,       // 지수적 증가
                Logarithmic,       // 로그 증가
                Custom            // 커스텀 곡선
            }

            [Header("조건부 효과")]
            public bool hasConditionalEffect;
            public ConditionalTrigger conditionalTrigger;
            public float conditionalThreshold;
            public Action conditionalAction;

            public enum ConditionalTrigger
            {
                OnHealthBelow,     // 체력이 특정값 이하
                OnHealthAbove,     // 체력이 특정값 이상
                OnStackReached,    // 특정 스택 도달
                OnTickCount,       // 특정 틱 수 도달
                OnTotalValue      // 누적값 도달
            }

            [Header("시너지")]
            public List<string> synergyTags = new List<string>();  // 시너지 태그
            public float synergyMultiplier = 1.5f;                 // 시너지 배수

            // 이벤트
            public event Action<PeriodicEffect, float> OnTick;
            public event Action<PeriodicEffect> OnStackChange;
            public event Action<PeriodicEffect> OnComplete;
            public event Action<PeriodicEffect> OnConditionMet;

            // 상태
            public bool isActive;
            public float elapsedTime;
            private string effectId;

            public PeriodicEffect(string name, EffectType type, float interval, float tickValue)
            {
                effectName = name;
                effectType = type;
                tickInterval = interval;
                baseTickValue = tickValue;
                currentTickValue = tickValue;
                effectId = Guid.NewGuid().ToString();

                // 기본 스케일링 곡선
                scalingCurve = AnimationCurve.Linear(0, 1, 1, 1);
            }

            // 효과 시작
            public void Start()
            {
                isActive = true;
                elapsedTime = 0;
                currentTick = 0;
                totalValue = 0;
                nextTickTime = tickOnStart ? 0 : tickInterval;

                Debug.Log($"[Periodic] {effectName} 시작 (간격: {tickInterval}s, 값: {baseTickValue})");

                if (tickOnStart)
                {
                    ProcessTick();
                }
            }

            // 업데이트
            public bool Update(float deltaTime)
            {
                if (!isActive) return false;

                elapsedTime += deltaTime;

                // 틱 처리
                if (elapsedTime >= nextTickTime)
                {
                    ProcessTick();
                    nextTickTime += GetAdjustedTickInterval();

                    // 최대 틱 체크
                    if (maxTicks > 0 && currentTick >= maxTicks)
                    {
                        Complete();
                        return true;
                    }
                }

                return false;
            }

            // 틱 처리
            private void ProcessTick()
            {
                currentTick++;

                // 스케일링 적용
                float scaledValue = CalculateScaledValue();

                // 스택 효과 적용
                float stackedValue = ApplyStackEffect(scaledValue);

                // 시너지 체크
                float finalValue = ApplySynergy(stackedValue);

                currentTickValue = finalValue;
                totalValue += finalValue;

                Debug.Log($"[Periodic] {effectName} Tick #{currentTick}: {finalValue:F1} (총: {totalValue:F1})");
                OnTick?.Invoke(this, finalValue);

                // 조건부 효과 체크
                CheckConditionalEffect();
            }

            // 스케일링 값 계산
            private float CalculateScaledValue()
            {
                float value = baseTickValue;

                switch (scalingType)
                {
                    case ScalingType.Linear:
                        value = baseTickValue * (1 + (currentTick - 1) * scalingFactor);
                        break;

                    case ScalingType.Exponential:
                        value = baseTickValue * Mathf.Pow(scalingFactor, currentTick - 1);
                        break;

                    case ScalingType.Logarithmic:
                        value = baseTickValue * Mathf.Log(currentTick + 1);
                        break;

                    case ScalingType.Custom:
                        if (scalingCurve != null && maxTicks > 0)
                        {
                            float t = (float)currentTick / maxTicks;
                            value = baseTickValue * scalingCurve.Evaluate(t);
                        }
                        break;
                }

                return value;
            }

            // 스택 효과 적용
            private float ApplyStackEffect(float value)
            {
                switch (stackBehavior)
                {
                    case StackBehavior.Amplified:
                        return value * currentStacks;

                    case StackBehavior.Accelerated:
                        // 틱 속도는 GetAdjustedTickInterval에서 처리
                        return value;

                    case StackBehavior.Extended:
                        // 지속시간은 maxTicks 조정으로 처리
                        return value;

                    default:
                        return value;
                }
            }

            // 조정된 틱 간격
            private float GetAdjustedTickInterval()
            {
                if (stackBehavior == StackBehavior.Accelerated && currentStacks > 1)
                {
                    return tickInterval / (1 + (currentStacks - 1) * 0.2f); // 스택당 20% 빨라짐
                }
                return tickInterval;
            }

            // 시너지 적용
            private float ApplySynergy(float value)
            {
                // 실제로는 태그 시스템과 연동
                if (synergyTags.Count > 0)
                {
                    // 시너지 태그가 있다고 가정
                    bool hasSynergy = UnityEngine.Random.value > 0.5f; // 테스트용
                    if (hasSynergy)
                    {
                        Debug.Log($"[Periodic] 시너지 발동! x{synergyMultiplier}");
                        return value * synergyMultiplier;
                    }
                }
                return value;
            }

            // 조건부 효과 체크
            private void CheckConditionalEffect()
            {
                if (!hasConditionalEffect) return;

                bool conditionMet = false;

                switch (conditionalTrigger)
                {
                    case ConditionalTrigger.OnStackReached:
                        conditionMet = currentStacks >= conditionalThreshold;
                        break;

                    case ConditionalTrigger.OnTickCount:
                        conditionMet = currentTick >= conditionalThreshold;
                        break;

                    case ConditionalTrigger.OnTotalValue:
                        conditionMet = totalValue >= conditionalThreshold;
                        break;
                }

                if (conditionMet)
                {
                    Debug.Log($"[Periodic] {effectName} 조건 충족!");
                    OnConditionMet?.Invoke(this);
                    conditionalAction?.Invoke();
                    hasConditionalEffect = false; // 한 번만 발동
                }
            }

            // 스택 추가
            public void AddStack(int amount = 1)
            {
                int prevStacks = currentStacks;
                currentStacks = Mathf.Min(currentStacks + amount, maxStacks);

                if (prevStacks != currentStacks)
                {
                    Debug.Log($"[Periodic] {effectName} 스택: {prevStacks} → {currentStacks}");

                    // Extended 타입은 지속시간 증가
                    if (stackBehavior == StackBehavior.Extended && maxTicks > 0)
                    {
                        maxTicks += 3; // 스택당 3틱 추가
                    }

                    OnStackChange?.Invoke(this);
                }
            }

            // 스택 제거
            public void RemoveStack(int amount = 1)
            {
                int prevStacks = currentStacks;
                currentStacks = Mathf.Max(currentStacks - amount, 0);

                if (prevStacks != currentStacks)
                {
                    Debug.Log($"[Periodic] {effectName} 스택: {prevStacks} → {currentStacks}");
                    OnStackChange?.Invoke(this);

                    if (currentStacks == 0)
                    {
                        Stop();
                    }
                }
            }

            // 효과 완료
            private void Complete()
            {
                Debug.Log($"[Periodic] {effectName} 완료! 총 {currentTick}틱, 누적값: {totalValue:F1}");
                isActive = false;
                OnComplete?.Invoke(this);
            }

            // 효과 중지
            public void Stop()
            {
                Debug.Log($"[Periodic] {effectName} 중지");
                isActive = false;
            }

            // 효과 리셋
            public void Reset()
            {
                currentTick = 0;
                totalValue = 0;
                currentStacks = 1;
                elapsedTime = 0;
                nextTickTime = tickOnStart ? 0 : tickInterval;
            }

            public string GetDebugInfo()
            {
                string info = $"{effectName} [{effectType}]\n";
                info += $"  Tick: {currentTick}/{(maxTicks > 0 ? maxTicks.ToString() : "∞")}";
                info += $" | Stack: {currentStacks}/{maxStacks}";
                info += $" | Total: {totalValue:F1}";

                if (isActive)
                {
                    float timeToNextTick = nextTickTime - elapsedTime;
                    info += $" | Next: {timeToNextTick:F1}s";
                }

                return info;
            }
        }

        // 효과 조합 시스템
        [Serializable]
        public class EffectCombination
        {
            public string combinationName;
            public List<string> requiredEffects;
            public string resultEffect;
            public bool consumeOriginals;

            public EffectCombination(string name)
            {
                combinationName = name;
                requiredEffects = new List<string>();
                consumeOriginals = true;
            }
        }

        [Header("주기적 효과 관리")]
        [SerializeField] private List<PeriodicEffect> activeEffects = new List<PeriodicEffect>();

        [Header("효과 조합")]
        [SerializeField] private List<EffectCombination> effectCombinations = new List<EffectCombination>();

        [Header("테스트 설정")]
        [SerializeField] private bool autoCheckCombinations = true;

        private void Start()
        {
            SetupCombinations();

            Debug.Log("=== Phase 3 - Step 3: Periodic Effects 테스트 ===");
            RunPeriodicTests();
        }

        private void SetupCombinations()
        {
            // 불 + 기름 = 폭발
            var fireOil = new EffectCombination("폭발");
            fireOil.requiredEffects.Add("화염");
            fireOil.requiredEffects.Add("기름");
            fireOil.resultEffect = "폭발";
            effectCombinations.Add(fireOil);

            // 독 + 독 = 맹독
            var doublePoisonCombo = new EffectCombination("맹독");
            doublePoisonCombo.requiredEffects.Add("독");
            doublePoisonCombo.requiredEffects.Add("독");
            doublePoisonCombo.resultEffect = "맹독";
            effectCombinations.Add(doublePoisonCombo);

            // 재생 + 재생 = 대재생
            var doubleRegenCombo = new EffectCombination("대재생");
            doubleRegenCombo.requiredEffects.Add("재생");
            doubleRegenCombo.requiredEffects.Add("재생");
            doubleRegenCombo.resultEffect = "대재생";
            effectCombinations.Add(doubleRegenCombo);
        }

        private void RunPeriodicTests()
        {
            TestBasicPeriodic();
            TestStackBehaviors();
            TestScalingEffects();
            TestConditionalEffects();
        }

        // 테스트 1: 기본 주기적 효과
        private void TestBasicPeriodic()
        {
            Debug.Log("\n========== 테스트 1: Basic Periodic ==========");

            // 기본 DoT
            var dot = new PeriodicEffect("화염", PeriodicEffect.EffectType.DamageOverTime, 0.5f, 5f);
            dot.maxTicks = 10;

            dot.OnTick += (effect, value) =>
            {
                Debug.Log($"  화염 데미지: {value}");
            };

            dot.Start();
            activeEffects.Add(dot);

            // 기본 HoT
            var hot = new PeriodicEffect("재생", PeriodicEffect.EffectType.HealOverTime, 1f, 3f);
            hot.maxTicks = 5;

            hot.Start();
            activeEffects.Add(hot);
        }

        // 테스트 2: 스택 동작
        private void TestStackBehaviors()
        {
            Debug.Log("\n========== 테스트 2: Stack Behaviors ==========");

            // Amplified 스택 (값 증폭)
            var amplified = new PeriodicEffect("증폭형 독", PeriodicEffect.EffectType.DamageOverTime, 1f, 4f);
            amplified.stackBehavior = PeriodicEffect.StackBehavior.Amplified;
            amplified.maxStacks = 5;
            amplified.currentStacks = 1;

            Debug.Log("증폭형 독: 스택이 늘어날수록 데미지 증가");

            // Accelerated 스택 (속도 증가)
            var accelerated = new PeriodicEffect("가속형 버프", PeriodicEffect.EffectType.StatModifier, 2f, 2f);
            accelerated.stackBehavior = PeriodicEffect.StackBehavior.Accelerated;
            accelerated.maxStacks = 3;

            Debug.Log("가속형 버프: 스택이 늘어날수록 틱 속도 증가");

            // Extended 스택 (지속시간 연장)
            var extended = new PeriodicEffect("연장형 재생", PeriodicEffect.EffectType.HealOverTime, 1f, 5f);
            extended.stackBehavior = PeriodicEffect.StackBehavior.Extended;
            extended.maxTicks = 5;
            extended.maxStacks = 3;

            Debug.Log("연장형 재생: 스택이 늘어날수록 지속시간 증가");
        }

        // 테스트 3: 스케일링 효과
        private void TestScalingEffects()
        {
            Debug.Log("\n========== 테스트 3: Scaling Effects ==========");

            // Linear 스케일링
            var linear = new PeriodicEffect("선형 증가 독", PeriodicEffect.EffectType.DamageOverTime, 1f, 5f);
            linear.scalingType = PeriodicEffect.ScalingType.Linear;
            linear.scalingFactor = 0.5f;
            linear.maxTicks = 5;

            Debug.Log("선형 독: 틱마다 데미지가 선형으로 증가");

            // Exponential 스케일링
            var exponential = new PeriodicEffect("지수 증가 버프", PeriodicEffect.EffectType.StatModifier, 2f, 2f);
            exponential.scalingType = PeriodicEffect.ScalingType.Exponential;
            exponential.scalingFactor = 1.5f;
            exponential.maxTicks = 4;

            Debug.Log("지수 버프: 틱마다 효과가 지수적으로 증가");
        }

        // 테스트 4: 조건부 효과
        private void TestConditionalEffects()
        {
            Debug.Log("\n========== 테스트 4: Conditional Effects ==========");

            // 틱 수 조건
            var tickCondition = new PeriodicEffect("누적 독", PeriodicEffect.EffectType.DamageOverTime, 0.5f, 3f);
            tickCondition.hasConditionalEffect = true;
            tickCondition.conditionalTrigger = PeriodicEffect.ConditionalTrigger.OnTickCount;
            tickCondition.conditionalThreshold = 5;
            tickCondition.conditionalAction = () =>
            {
                Debug.Log("★ 5틱 도달! 폭발 효과 발동!");
            };

            Debug.Log("누적 독: 5틱에 도달하면 폭발");

            // 누적값 조건
            var valueCondition = new PeriodicEffect("충전", PeriodicEffect.EffectType.ResourceRestore, 1f, 10f);
            valueCondition.hasConditionalEffect = true;
            valueCondition.conditionalTrigger = PeriodicEffect.ConditionalTrigger.OnTotalValue;
            valueCondition.conditionalThreshold = 50;
            valueCondition.conditionalAction = () =>
            {
                Debug.Log("★ 50 충전 완료! 특수 능력 활성화!");
            };

            Debug.Log("충전: 누적 50에 도달하면 특수 능력");
        }

        private void Update()
        {
            // 활성 효과 업데이트
            var effectsToRemove = new List<PeriodicEffect>();

            foreach (var effect in activeEffects)
            {
                if (effect.Update(Time.deltaTime))
                {
                    effectsToRemove.Add(effect);
                }
            }

            foreach (var effect in effectsToRemove)
            {
                activeEffects.Remove(effect);
            }

            // 조합 체크
            if (autoCheckCombinations)
            {
                CheckEffectCombinations();
            }

            HandleInput();
        }

        private void CheckEffectCombinations()
        {
            foreach (var combination in effectCombinations)
            {
                bool hasAll = true;
                var matchingEffects = new List<PeriodicEffect>();

                foreach (var requiredEffect in combination.requiredEffects)
                {
                    var found = activeEffects.Find(e => e.effectName == requiredEffect && e.isActive);
                    if (found == null)
                    {
                        hasAll = false;
                        break;
                    }
                    matchingEffects.Add(found);
                }

                if (hasAll && matchingEffects.Count > 0)
                {
                    TriggerCombination(combination, matchingEffects);
                    break; // 한 번에 하나의 조합만
                }
            }
        }

        private void TriggerCombination(EffectCombination combination, List<PeriodicEffect> sourceEffects)
        {
            Debug.Log($"★★★ 효과 조합 발동: {combination.combinationName} ★★★");

            // 원본 효과 제거
            if (combination.consumeOriginals)
            {
                foreach (var effect in sourceEffects)
                {
                    effect.Stop();
                    activeEffects.Remove(effect);
                }
            }

            // 결과 효과 생성
            CreateResultEffect(combination.resultEffect);
        }

        private void CreateResultEffect(string effectName)
        {
            PeriodicEffect newEffect = null;

            switch (effectName)
            {
                case "폭발":
                    newEffect = new PeriodicEffect("폭발", PeriodicEffect.EffectType.DamageOverTime, 0.2f, 15f);
                    newEffect.maxTicks = 5;
                    break;

                case "맹독":
                    newEffect = new PeriodicEffect("맹독", PeriodicEffect.EffectType.DamageOverTime, 0.5f, 8f);
                    newEffect.maxTicks = 10;
                    newEffect.scalingType = PeriodicEffect.ScalingType.Linear;
                    newEffect.scalingFactor = 0.3f;
                    break;

                case "대재생":
                    newEffect = new PeriodicEffect("대재생", PeriodicEffect.EffectType.HealOverTime, 0.5f, 10f);
                    newEffect.maxTicks = 10;
                    break;
            }

            if (newEffect != null)
            {
                newEffect.Start();
                activeEffects.Add(newEffect);
            }
        }

        private void HandleInput()
        {
            // 주기적 효과 추가
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AddPeriodicDamage();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AddPeriodicHeal();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                AddStackableEffect();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                AddScalingEffect();
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                AddConditionalEffect();
            }

            // 조합용 효과
            if (Input.GetKeyDown(KeyCode.Q))
            {
                AddEffect("화염");
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                AddEffect("기름");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                AddEffect("독");
            }

            // 스택 조작
            if (Input.GetKeyDown(KeyCode.Z))
            {
                AddStackToFirst();
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                RemoveStackFromFirst();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ClearAllEffects();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void AddPeriodicDamage()
        {
            var dot = new PeriodicEffect("독", PeriodicEffect.EffectType.DamageOverTime, 0.5f, 3f);
            dot.maxTicks = 10;
            dot.Start();
            activeEffects.Add(dot);
            Debug.Log("독 효과 추가");
        }

        private void AddPeriodicHeal()
        {
            var hot = new PeriodicEffect("재생", PeriodicEffect.EffectType.HealOverTime, 1f, 5f);
            hot.maxTicks = 5;
            hot.Start();
            activeEffects.Add(hot);
            Debug.Log("재생 효과 추가");
        }

        private void AddStackableEffect()
        {
            var existing = activeEffects.Find(e => e.effectName == "스택형 독");
            if (existing != null)
            {
                existing.AddStack();
            }
            else
            {
                var stacking = new PeriodicEffect("스택형 독", PeriodicEffect.EffectType.DamageOverTime, 1f, 2f);
                stacking.stackBehavior = PeriodicEffect.StackBehavior.Amplified;
                stacking.maxStacks = 5;
                stacking.maxTicks = 10;
                stacking.Start();
                activeEffects.Add(stacking);
            }
            Debug.Log("스택형 독 추가/스택 증가");
        }

        private void AddScalingEffect()
        {
            var scaling = new PeriodicEffect("성장형 버프", PeriodicEffect.EffectType.StatModifier, 2f, 1f);
            scaling.scalingType = PeriodicEffect.ScalingType.Linear;
            scaling.scalingFactor = 1f;
            scaling.maxTicks = 5;
            scaling.Start();
            activeEffects.Add(scaling);
            Debug.Log("성장형 버프 추가");
        }

        private void AddConditionalEffect()
        {
            var conditional = new PeriodicEffect("충전", PeriodicEffect.EffectType.ResourceRestore, 0.5f, 5f);
            conditional.hasConditionalEffect = true;
            conditional.conditionalTrigger = PeriodicEffect.ConditionalTrigger.OnTotalValue;
            conditional.conditionalThreshold = 30;
            conditional.conditionalAction = () =>
            {
                Debug.Log("★★★ 충전 완료! 특수 효과 발동! ★★★");
            };
            conditional.maxTicks = 10;
            conditional.Start();
            activeEffects.Add(conditional);
            Debug.Log("조건부 충전 효과 추가");
        }

        private void AddEffect(string effectName)
        {
            PeriodicEffect effect = null;

            switch (effectName)
            {
                case "화염":
                    effect = new PeriodicEffect("화염", PeriodicEffect.EffectType.DamageOverTime, 0.5f, 4f);
                    effect.maxTicks = 6;
                    break;
                case "기름":
                    effect = new PeriodicEffect("기름", PeriodicEffect.EffectType.DamageOverTime, 2f, 1f);
                    effect.maxTicks = 3;
                    break;
                case "독":
                    effect = new PeriodicEffect("독", PeriodicEffect.EffectType.DamageOverTime, 1f, 3f);
                    effect.maxTicks = 5;
                    break;
            }

            if (effect != null)
            {
                effect.Start();
                activeEffects.Add(effect);
                Debug.Log($"{effectName} 효과 추가");
            }
        }

        private void AddStackToFirst()
        {
            var stackable = activeEffects.Find(e => e.maxStacks > 1);
            if (stackable != null)
            {
                stackable.AddStack();
            }
        }

        private void RemoveStackFromFirst()
        {
            var stackable = activeEffects.Find(e => e.currentStacks > 1);
            if (stackable != null)
            {
                stackable.RemoveStack();
            }
        }

        private void ClearAllEffects()
        {
            foreach (var effect in activeEffects)
            {
                effect.Stop();
            }
            activeEffects.Clear();
            Debug.Log("모든 효과 제거");
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== 활성 주기적 효과 ===");
            foreach (var effect in activeEffects)
            {
                Debug.Log(effect.GetDebugInfo());
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 450, 380), "Phase 3 - Periodic Effects");

            int y = 40;
            GUI.Label(new Rect(20, y, 430, 20), $"=== 활성 효과 ({activeEffects.Count}) ===");
            y += 25;

            // 활성 효과 목록
            foreach (var effect in activeEffects)
            {
                if (!effect.isActive) continue;

                // 효과 이름과 타입
                GUI.Label(new Rect(30, y, 200, 20), $"{effect.effectName} [{effect.effectType}]");

                // 진행 상황
                if (effect.maxTicks > 0)
                {
                    float progress = (float)effect.currentTick / effect.maxTicks;
                    Rect barRect = new Rect(240, y, 100, 18);
                    GUI.Box(barRect, "");
                    GUI.Box(new Rect(barRect.x, barRect.y, barRect.width * progress, barRect.height), "");
                    GUI.Label(barRect, $" {effect.currentTick}/{effect.maxTicks}", GUI.skin.label);
                }
                else
                {
                    GUI.Label(new Rect(240, y, 100, 20), $"Tick: {effect.currentTick}");
                }

                // 스택
                if (effect.maxStacks > 1)
                {
                    GUI.Label(new Rect(350, y, 80, 20), $"Stack: {effect.currentStacks}/{effect.maxStacks}");
                }

                y += 22;

                // 추가 정보
                string info = $"  값: {effect.currentTickValue:F1} | 총: {effect.totalValue:F1}";
                if (effect.elapsedTime > 0)
                {
                    float nextTick = effect.nextTickTime - effect.elapsedTime;
                    info += $" | 다음: {nextTick:F1}s";
                }
                GUI.Label(new Rect(50, y, 380, 20), info);
                y += 22;
            }

            if (activeEffects.Count == 0)
            {
                GUI.Label(new Rect(30, y, 400, 20), "활성 효과 없음");
                y += 20;
            }

            // 조작법
            y = 280;
            GUI.Box(new Rect(10, y, 450, 120), "조작법");
            y += 25;
            GUI.Label(new Rect(20, y, 430, 90),
                "효과: 1-독 2-재생 3-스택형 4-성장형 5-조건부\n" +
                "조합: Q-화염 W-기름 E-독 (화염+기름=폭발, 독+독=맹독)\n" +
                "스택: Z-스택추가 X-스택제거\n" +
                "R: 모든 효과 제거  Space: 상태 출력");
        }
    }
}