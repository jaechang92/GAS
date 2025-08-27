// ���� ��ġ: Assets/Scripts/GAS/Learning/Phase3/PeriodicEffectExample.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAS.Learning.Phase3
{
    /// <summary>
    /// Step 3: Periodic Effects - �ֱ��� ȿ���� ���� �ý���
    /// </summary>
    public class PeriodicEffectExample : MonoBehaviour
    {
        // �ֱ��� ȿ���� Ưȭ�� Effect
        [Serializable]
        public class PeriodicEffect
        {
            [Header("�⺻ ����")]
            public string effectName;
            public EffectType effectType;

            public enum EffectType
            {
                DamageOverTime,     // DoT
                HealOverTime,       // HoT
                ResourceDrain,      // �ڿ� �Ҹ�
                ResourceRestore,    // �ڿ� ȸ��
                StatModifier,       // ���� ����
                Custom             // Ŀ����
            }

            [Header("�ֱ� ����")]
            public float tickInterval = 1f;        // ƽ ����
            public int maxTicks = -1;              // �ִ� ƽ (-1�� ����)
            public int currentTick = 0;            // ���� ƽ
            public float nextTickTime = 0;         // ���� ƽ �ð�
            public bool tickOnStart = true;        // ���� �� ��� ƽ

            [Header("ȿ�� ��")]
            public float baseTickValue;            // �⺻ ƽ�� ��
            public float currentTickValue;         // ���� ƽ�� ��
            public float totalValue;               // ���� ��

            [Header("���� �ý���")]
            public StackBehavior stackBehavior;
            public int currentStacks = 1;
            public int maxStacks = 5;

            public enum StackBehavior
            {
                Independent,        // ���� ���������� ƽ
                Synchronized,       // ����ȭ�� ƽ
                Amplified,         // �� ����
                Extended,          // ���ӽð� ����
                Accelerated        // ƽ �ӵ� ����
            }

            [Header("�����ϸ�")]
            public ScalingType scalingType;
            public AnimationCurve scalingCurve;
            public float scalingFactor = 1f;

            public enum ScalingType
            {
                None,              // �����ϸ� ����
                Linear,            // ���� ����/����
                Exponential,       // ������ ����
                Logarithmic,       // �α� ����
                Custom            // Ŀ���� �
            }

            [Header("���Ǻ� ȿ��")]
            public bool hasConditionalEffect;
            public ConditionalTrigger conditionalTrigger;
            public float conditionalThreshold;
            public Action conditionalAction;

            public enum ConditionalTrigger
            {
                OnHealthBelow,     // ü���� Ư���� ����
                OnHealthAbove,     // ü���� Ư���� �̻�
                OnStackReached,    // Ư�� ���� ����
                OnTickCount,       // Ư�� ƽ �� ����
                OnTotalValue      // ������ ����
            }

            [Header("�ó���")]
            public List<string> synergyTags = new List<string>();  // �ó��� �±�
            public float synergyMultiplier = 1.5f;                 // �ó��� ���

            // �̺�Ʈ
            public event Action<PeriodicEffect, float> OnTick;
            public event Action<PeriodicEffect> OnStackChange;
            public event Action<PeriodicEffect> OnComplete;
            public event Action<PeriodicEffect> OnConditionMet;

            // ����
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

                // �⺻ �����ϸ� �
                scalingCurve = AnimationCurve.Linear(0, 1, 1, 1);
            }

            // ȿ�� ����
            public void Start()
            {
                isActive = true;
                elapsedTime = 0;
                currentTick = 0;
                totalValue = 0;
                nextTickTime = tickOnStart ? 0 : tickInterval;

                Debug.Log($"[Periodic] {effectName} ���� (����: {tickInterval}s, ��: {baseTickValue})");

                if (tickOnStart)
                {
                    ProcessTick();
                }
            }

            // ������Ʈ
            public bool Update(float deltaTime)
            {
                if (!isActive) return false;

                elapsedTime += deltaTime;

                // ƽ ó��
                if (elapsedTime >= nextTickTime)
                {
                    ProcessTick();
                    nextTickTime += GetAdjustedTickInterval();

                    // �ִ� ƽ üũ
                    if (maxTicks > 0 && currentTick >= maxTicks)
                    {
                        Complete();
                        return true;
                    }
                }

                return false;
            }

            // ƽ ó��
            private void ProcessTick()
            {
                currentTick++;

                // �����ϸ� ����
                float scaledValue = CalculateScaledValue();

                // ���� ȿ�� ����
                float stackedValue = ApplyStackEffect(scaledValue);

                // �ó��� üũ
                float finalValue = ApplySynergy(stackedValue);

                currentTickValue = finalValue;
                totalValue += finalValue;

                Debug.Log($"[Periodic] {effectName} Tick #{currentTick}: {finalValue:F1} (��: {totalValue:F1})");
                OnTick?.Invoke(this, finalValue);

                // ���Ǻ� ȿ�� üũ
                CheckConditionalEffect();
            }

            // �����ϸ� �� ���
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

            // ���� ȿ�� ����
            private float ApplyStackEffect(float value)
            {
                switch (stackBehavior)
                {
                    case StackBehavior.Amplified:
                        return value * currentStacks;

                    case StackBehavior.Accelerated:
                        // ƽ �ӵ��� GetAdjustedTickInterval���� ó��
                        return value;

                    case StackBehavior.Extended:
                        // ���ӽð��� maxTicks �������� ó��
                        return value;

                    default:
                        return value;
                }
            }

            // ������ ƽ ����
            private float GetAdjustedTickInterval()
            {
                if (stackBehavior == StackBehavior.Accelerated && currentStacks > 1)
                {
                    return tickInterval / (1 + (currentStacks - 1) * 0.2f); // ���ô� 20% ������
                }
                return tickInterval;
            }

            // �ó��� ����
            private float ApplySynergy(float value)
            {
                // �����δ� �±� �ý��۰� ����
                if (synergyTags.Count > 0)
                {
                    // �ó��� �±װ� �ִٰ� ����
                    bool hasSynergy = UnityEngine.Random.value > 0.5f; // �׽�Ʈ��
                    if (hasSynergy)
                    {
                        Debug.Log($"[Periodic] �ó��� �ߵ�! x{synergyMultiplier}");
                        return value * synergyMultiplier;
                    }
                }
                return value;
            }

            // ���Ǻ� ȿ�� üũ
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
                    Debug.Log($"[Periodic] {effectName} ���� ����!");
                    OnConditionMet?.Invoke(this);
                    conditionalAction?.Invoke();
                    hasConditionalEffect = false; // �� ���� �ߵ�
                }
            }

            // ���� �߰�
            public void AddStack(int amount = 1)
            {
                int prevStacks = currentStacks;
                currentStacks = Mathf.Min(currentStacks + amount, maxStacks);

                if (prevStacks != currentStacks)
                {
                    Debug.Log($"[Periodic] {effectName} ����: {prevStacks} �� {currentStacks}");

                    // Extended Ÿ���� ���ӽð� ����
                    if (stackBehavior == StackBehavior.Extended && maxTicks > 0)
                    {
                        maxTicks += 3; // ���ô� 3ƽ �߰�
                    }

                    OnStackChange?.Invoke(this);
                }
            }

            // ���� ����
            public void RemoveStack(int amount = 1)
            {
                int prevStacks = currentStacks;
                currentStacks = Mathf.Max(currentStacks - amount, 0);

                if (prevStacks != currentStacks)
                {
                    Debug.Log($"[Periodic] {effectName} ����: {prevStacks} �� {currentStacks}");
                    OnStackChange?.Invoke(this);

                    if (currentStacks == 0)
                    {
                        Stop();
                    }
                }
            }

            // ȿ�� �Ϸ�
            private void Complete()
            {
                Debug.Log($"[Periodic] {effectName} �Ϸ�! �� {currentTick}ƽ, ������: {totalValue:F1}");
                isActive = false;
                OnComplete?.Invoke(this);
            }

            // ȿ�� ����
            public void Stop()
            {
                Debug.Log($"[Periodic] {effectName} ����");
                isActive = false;
            }

            // ȿ�� ����
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
                info += $"  Tick: {currentTick}/{(maxTicks > 0 ? maxTicks.ToString() : "��")}";
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

        // ȿ�� ���� �ý���
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

        [Header("�ֱ��� ȿ�� ����")]
        [SerializeField] private List<PeriodicEffect> activeEffects = new List<PeriodicEffect>();

        [Header("ȿ�� ����")]
        [SerializeField] private List<EffectCombination> effectCombinations = new List<EffectCombination>();

        [Header("�׽�Ʈ ����")]
        [SerializeField] private bool autoCheckCombinations = true;

        private void Start()
        {
            SetupCombinations();

            Debug.Log("=== Phase 3 - Step 3: Periodic Effects �׽�Ʈ ===");
            RunPeriodicTests();
        }

        private void SetupCombinations()
        {
            // �� + �⸧ = ����
            var fireOil = new EffectCombination("����");
            fireOil.requiredEffects.Add("ȭ��");
            fireOil.requiredEffects.Add("�⸧");
            fireOil.resultEffect = "����";
            effectCombinations.Add(fireOil);

            // �� + �� = �͵�
            var doublePoisonCombo = new EffectCombination("�͵�");
            doublePoisonCombo.requiredEffects.Add("��");
            doublePoisonCombo.requiredEffects.Add("��");
            doublePoisonCombo.resultEffect = "�͵�";
            effectCombinations.Add(doublePoisonCombo);

            // ��� + ��� = �����
            var doubleRegenCombo = new EffectCombination("�����");
            doubleRegenCombo.requiredEffects.Add("���");
            doubleRegenCombo.requiredEffects.Add("���");
            doubleRegenCombo.resultEffect = "�����";
            effectCombinations.Add(doubleRegenCombo);
        }

        private void RunPeriodicTests()
        {
            TestBasicPeriodic();
            TestStackBehaviors();
            TestScalingEffects();
            TestConditionalEffects();
        }

        // �׽�Ʈ 1: �⺻ �ֱ��� ȿ��
        private void TestBasicPeriodic()
        {
            Debug.Log("\n========== �׽�Ʈ 1: Basic Periodic ==========");

            // �⺻ DoT
            var dot = new PeriodicEffect("ȭ��", PeriodicEffect.EffectType.DamageOverTime, 0.5f, 5f);
            dot.maxTicks = 10;

            dot.OnTick += (effect, value) =>
            {
                Debug.Log($"  ȭ�� ������: {value}");
            };

            dot.Start();
            activeEffects.Add(dot);

            // �⺻ HoT
            var hot = new PeriodicEffect("���", PeriodicEffect.EffectType.HealOverTime, 1f, 3f);
            hot.maxTicks = 5;

            hot.Start();
            activeEffects.Add(hot);
        }

        // �׽�Ʈ 2: ���� ����
        private void TestStackBehaviors()
        {
            Debug.Log("\n========== �׽�Ʈ 2: Stack Behaviors ==========");

            // Amplified ���� (�� ����)
            var amplified = new PeriodicEffect("������ ��", PeriodicEffect.EffectType.DamageOverTime, 1f, 4f);
            amplified.stackBehavior = PeriodicEffect.StackBehavior.Amplified;
            amplified.maxStacks = 5;
            amplified.currentStacks = 1;

            Debug.Log("������ ��: ������ �þ���� ������ ����");

            // Accelerated ���� (�ӵ� ����)
            var accelerated = new PeriodicEffect("������ ����", PeriodicEffect.EffectType.StatModifier, 2f, 2f);
            accelerated.stackBehavior = PeriodicEffect.StackBehavior.Accelerated;
            accelerated.maxStacks = 3;

            Debug.Log("������ ����: ������ �þ���� ƽ �ӵ� ����");

            // Extended ���� (���ӽð� ����)
            var extended = new PeriodicEffect("������ ���", PeriodicEffect.EffectType.HealOverTime, 1f, 5f);
            extended.stackBehavior = PeriodicEffect.StackBehavior.Extended;
            extended.maxTicks = 5;
            extended.maxStacks = 3;

            Debug.Log("������ ���: ������ �þ���� ���ӽð� ����");
        }

        // �׽�Ʈ 3: �����ϸ� ȿ��
        private void TestScalingEffects()
        {
            Debug.Log("\n========== �׽�Ʈ 3: Scaling Effects ==========");

            // Linear �����ϸ�
            var linear = new PeriodicEffect("���� ���� ��", PeriodicEffect.EffectType.DamageOverTime, 1f, 5f);
            linear.scalingType = PeriodicEffect.ScalingType.Linear;
            linear.scalingFactor = 0.5f;
            linear.maxTicks = 5;

            Debug.Log("���� ��: ƽ���� �������� �������� ����");

            // Exponential �����ϸ�
            var exponential = new PeriodicEffect("���� ���� ����", PeriodicEffect.EffectType.StatModifier, 2f, 2f);
            exponential.scalingType = PeriodicEffect.ScalingType.Exponential;
            exponential.scalingFactor = 1.5f;
            exponential.maxTicks = 4;

            Debug.Log("���� ����: ƽ���� ȿ���� ���������� ����");
        }

        // �׽�Ʈ 4: ���Ǻ� ȿ��
        private void TestConditionalEffects()
        {
            Debug.Log("\n========== �׽�Ʈ 4: Conditional Effects ==========");

            // ƽ �� ����
            var tickCondition = new PeriodicEffect("���� ��", PeriodicEffect.EffectType.DamageOverTime, 0.5f, 3f);
            tickCondition.hasConditionalEffect = true;
            tickCondition.conditionalTrigger = PeriodicEffect.ConditionalTrigger.OnTickCount;
            tickCondition.conditionalThreshold = 5;
            tickCondition.conditionalAction = () =>
            {
                Debug.Log("�� 5ƽ ����! ���� ȿ�� �ߵ�!");
            };

            Debug.Log("���� ��: 5ƽ�� �����ϸ� ����");

            // ������ ����
            var valueCondition = new PeriodicEffect("����", PeriodicEffect.EffectType.ResourceRestore, 1f, 10f);
            valueCondition.hasConditionalEffect = true;
            valueCondition.conditionalTrigger = PeriodicEffect.ConditionalTrigger.OnTotalValue;
            valueCondition.conditionalThreshold = 50;
            valueCondition.conditionalAction = () =>
            {
                Debug.Log("�� 50 ���� �Ϸ�! Ư�� �ɷ� Ȱ��ȭ!");
            };

            Debug.Log("����: ���� 50�� �����ϸ� Ư�� �ɷ�");
        }

        private void Update()
        {
            // Ȱ�� ȿ�� ������Ʈ
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

            // ���� üũ
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
                    break; // �� ���� �ϳ��� ���ո�
                }
            }
        }

        private void TriggerCombination(EffectCombination combination, List<PeriodicEffect> sourceEffects)
        {
            Debug.Log($"�ڡڡ� ȿ�� ���� �ߵ�: {combination.combinationName} �ڡڡ�");

            // ���� ȿ�� ����
            if (combination.consumeOriginals)
            {
                foreach (var effect in sourceEffects)
                {
                    effect.Stop();
                    activeEffects.Remove(effect);
                }
            }

            // ��� ȿ�� ����
            CreateResultEffect(combination.resultEffect);
        }

        private void CreateResultEffect(string effectName)
        {
            PeriodicEffect newEffect = null;

            switch (effectName)
            {
                case "����":
                    newEffect = new PeriodicEffect("����", PeriodicEffect.EffectType.DamageOverTime, 0.2f, 15f);
                    newEffect.maxTicks = 5;
                    break;

                case "�͵�":
                    newEffect = new PeriodicEffect("�͵�", PeriodicEffect.EffectType.DamageOverTime, 0.5f, 8f);
                    newEffect.maxTicks = 10;
                    newEffect.scalingType = PeriodicEffect.ScalingType.Linear;
                    newEffect.scalingFactor = 0.3f;
                    break;

                case "�����":
                    newEffect = new PeriodicEffect("�����", PeriodicEffect.EffectType.HealOverTime, 0.5f, 10f);
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
            // �ֱ��� ȿ�� �߰�
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

            // ���տ� ȿ��
            if (Input.GetKeyDown(KeyCode.Q))
            {
                AddEffect("ȭ��");
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                AddEffect("�⸧");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                AddEffect("��");
            }

            // ���� ����
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
            var dot = new PeriodicEffect("��", PeriodicEffect.EffectType.DamageOverTime, 0.5f, 3f);
            dot.maxTicks = 10;
            dot.Start();
            activeEffects.Add(dot);
            Debug.Log("�� ȿ�� �߰�");
        }

        private void AddPeriodicHeal()
        {
            var hot = new PeriodicEffect("���", PeriodicEffect.EffectType.HealOverTime, 1f, 5f);
            hot.maxTicks = 5;
            hot.Start();
            activeEffects.Add(hot);
            Debug.Log("��� ȿ�� �߰�");
        }

        private void AddStackableEffect()
        {
            var existing = activeEffects.Find(e => e.effectName == "������ ��");
            if (existing != null)
            {
                existing.AddStack();
            }
            else
            {
                var stacking = new PeriodicEffect("������ ��", PeriodicEffect.EffectType.DamageOverTime, 1f, 2f);
                stacking.stackBehavior = PeriodicEffect.StackBehavior.Amplified;
                stacking.maxStacks = 5;
                stacking.maxTicks = 10;
                stacking.Start();
                activeEffects.Add(stacking);
            }
            Debug.Log("������ �� �߰�/���� ����");
        }

        private void AddScalingEffect()
        {
            var scaling = new PeriodicEffect("������ ����", PeriodicEffect.EffectType.StatModifier, 2f, 1f);
            scaling.scalingType = PeriodicEffect.ScalingType.Linear;
            scaling.scalingFactor = 1f;
            scaling.maxTicks = 5;
            scaling.Start();
            activeEffects.Add(scaling);
            Debug.Log("������ ���� �߰�");
        }

        private void AddConditionalEffect()
        {
            var conditional = new PeriodicEffect("����", PeriodicEffect.EffectType.ResourceRestore, 0.5f, 5f);
            conditional.hasConditionalEffect = true;
            conditional.conditionalTrigger = PeriodicEffect.ConditionalTrigger.OnTotalValue;
            conditional.conditionalThreshold = 30;
            conditional.conditionalAction = () =>
            {
                Debug.Log("�ڡڡ� ���� �Ϸ�! Ư�� ȿ�� �ߵ�! �ڡڡ�");
            };
            conditional.maxTicks = 10;
            conditional.Start();
            activeEffects.Add(conditional);
            Debug.Log("���Ǻ� ���� ȿ�� �߰�");
        }

        private void AddEffect(string effectName)
        {
            PeriodicEffect effect = null;

            switch (effectName)
            {
                case "ȭ��":
                    effect = new PeriodicEffect("ȭ��", PeriodicEffect.EffectType.DamageOverTime, 0.5f, 4f);
                    effect.maxTicks = 6;
                    break;
                case "�⸧":
                    effect = new PeriodicEffect("�⸧", PeriodicEffect.EffectType.DamageOverTime, 2f, 1f);
                    effect.maxTicks = 3;
                    break;
                case "��":
                    effect = new PeriodicEffect("��", PeriodicEffect.EffectType.DamageOverTime, 1f, 3f);
                    effect.maxTicks = 5;
                    break;
            }

            if (effect != null)
            {
                effect.Start();
                activeEffects.Add(effect);
                Debug.Log($"{effectName} ȿ�� �߰�");
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
            Debug.Log("��� ȿ�� ����");
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== Ȱ�� �ֱ��� ȿ�� ===");
            foreach (var effect in activeEffects)
            {
                Debug.Log(effect.GetDebugInfo());
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 450, 380), "Phase 3 - Periodic Effects");

            int y = 40;
            GUI.Label(new Rect(20, y, 430, 20), $"=== Ȱ�� ȿ�� ({activeEffects.Count}) ===");
            y += 25;

            // Ȱ�� ȿ�� ���
            foreach (var effect in activeEffects)
            {
                if (!effect.isActive) continue;

                // ȿ�� �̸��� Ÿ��
                GUI.Label(new Rect(30, y, 200, 20), $"{effect.effectName} [{effect.effectType}]");

                // ���� ��Ȳ
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

                // ����
                if (effect.maxStacks > 1)
                {
                    GUI.Label(new Rect(350, y, 80, 20), $"Stack: {effect.currentStacks}/{effect.maxStacks}");
                }

                y += 22;

                // �߰� ����
                string info = $"  ��: {effect.currentTickValue:F1} | ��: {effect.totalValue:F1}";
                if (effect.elapsedTime > 0)
                {
                    float nextTick = effect.nextTickTime - effect.elapsedTime;
                    info += $" | ����: {nextTick:F1}s";
                }
                GUI.Label(new Rect(50, y, 380, 20), info);
                y += 22;
            }

            if (activeEffects.Count == 0)
            {
                GUI.Label(new Rect(30, y, 400, 20), "Ȱ�� ȿ�� ����");
                y += 20;
            }

            // ���۹�
            y = 280;
            GUI.Box(new Rect(10, y, 450, 120), "���۹�");
            y += 25;
            GUI.Label(new Rect(20, y, 430, 90),
                "ȿ��: 1-�� 2-��� 3-������ 4-������ 5-���Ǻ�\n" +
                "����: Q-ȭ�� W-�⸧ E-�� (ȭ��+�⸧=����, ��+��=�͵�)\n" +
                "����: Z-�����߰� X-��������\n" +
                "R: ��� ȿ�� ����  Space: ���� ���");
        }
    }
}