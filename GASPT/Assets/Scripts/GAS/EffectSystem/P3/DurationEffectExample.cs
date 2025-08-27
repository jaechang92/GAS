// ���� ��ġ: Assets/Scripts/GAS/Learning/Phase3/DurationEffectExample.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAS.Learning.Phase3
{
    /// <summary>
    /// Step 2: Duration Effects - �ð� ��� ȿ�� ����
    /// </summary>
    public class DurationEffectExample : MonoBehaviour
    {
        // Duration ������ ��ȭ�� Effect
        [Serializable]
        public class DurationEffect
        {
            [Header("�⺻ ����")]
            public string effectName;
            public string description;
            public EffectDuration durationType;

            public enum EffectDuration
            {
                Instant,        // ���
                Duration,       // ���ӽð�
                Infinite,       // ����
                UntilRemoved    // ���ŵ� ������
            }

            [Header("�ð� ����")]
            public float baseDuration;
            public float currentDuration;
            public float elapsedTime;

            [Header("�ֱ��� ȿ��")]
            public bool isPeriodic;
            public float periodInterval = 1f;
            public float nextPeriodTime;
            public int periodicTicks;
            public int maxTicks = -1; // -1�� ����

            [Header("ȿ�� ��")]
            public float baseValue;
            public float currentValue;
            public AnimationCurve valueCurve; // �ð��� ���� ȿ�� ��ȭ

            [Header("���� & ����")]
            public StackPolicy stackPolicy;
            public RefreshPolicy refreshPolicy;

            public enum StackPolicy
            {
                None,           // ���� �Ұ�
                Replace,        // ��ü
                Stack,          // ��ø
                Unique,         // ���� �ν��Ͻ�
                Refresh         // �ð� ����
            }

            public enum RefreshPolicy
            {
                None,           // ���� ����
                RestartDuration,// ���ӽð� ����
                AddDuration,    // ���ӽð� �߰�
                MaxDuration     // �ִ밪����
            }

            [Header("���̵� ȿ��")]
            public bool hasFadeIn;
            public float fadeInDuration = 0.5f;
            public bool hasFadeOut;
            public float fadeOutDuration = 1f;

            // �̺�Ʈ
            public event Action<DurationEffect> OnStart;
            public event Action<DurationEffect> OnTick;
            public event Action<DurationEffect> OnExpire;
            public event Action<DurationEffect> OnRemove;

            // ����
            public bool isActive { get; private set; }
            public bool isPaused { get; private set; }
            private string effectId;

            public DurationEffect(string name, EffectDuration type, float duration = 0)
            {
                effectName = name;
                durationType = type;
                baseDuration = duration;
                currentDuration = duration;
                effectId = Guid.NewGuid().ToString();

                // �⺻ �� � (������ ��)
                valueCurve = AnimationCurve.Constant(0, 1, 1);
            }

            // ȿ�� ����
            public void Start()
            {
                if (isActive) return;

                isActive = true;
                elapsedTime = 0;
                periodicTicks = 0;
                nextPeriodTime = isPeriodic ? periodInterval : float.MaxValue;

                Debug.Log($"[Duration] {effectName} ���� (Duration: {GetDurationString()})");
                OnStart?.Invoke(this);

                // ��� ȿ���� �ٷ� ƽ
                if (durationType == EffectDuration.Instant)
                {
                    Tick();
                    Expire();
                }
                else if (isPeriodic && periodInterval <= 0)
                {
                    Tick(); // ù ƽ�� ���
                }
            }

            // �ð� ������Ʈ
            public bool Update(float deltaTime)
            {
                if (!isActive || isPaused) return false;
                if (durationType == EffectDuration.Instant) return true;
                if (durationType == EffectDuration.Infinite || durationType == EffectDuration.UntilRemoved) return false;

                elapsedTime += deltaTime;

                // �� � ����
                if (valueCurve != null && valueCurve.length > 0)
                {
                    float normalizedTime = elapsedTime / baseDuration;
                    float curveValue = valueCurve.Evaluate(normalizedTime);
                    currentValue = baseValue * curveValue;

                    // Fade ȿ��
                    if (hasFadeIn && elapsedTime < fadeInDuration)
                    {
                        currentValue *= (elapsedTime / fadeInDuration);
                    }
                    else if (hasFadeOut && (currentDuration - elapsedTime) < fadeOutDuration)
                    {
                        currentValue *= ((currentDuration - elapsedTime) / fadeOutDuration);
                    }
                }

                // �ֱ��� ƽ
                if (isPeriodic && elapsedTime >= nextPeriodTime)
                {
                    Tick();
                    nextPeriodTime += periodInterval;

                    // �ִ� ƽ üũ
                    if (maxTicks > 0 && periodicTicks >= maxTicks)
                    {
                        Expire();
                        return true;
                    }
                }

                // Duration üũ
                currentDuration -= deltaTime;
                if (currentDuration <= 0)
                {
                    Expire();
                    return true;
                }

                return false;
            }

            // ȿ�� ƽ
            private void Tick()
            {
                periodicTicks++;
                Debug.Log($"[Duration] {effectName} Tick #{periodicTicks} (Value: {currentValue:F1})");
                OnTick?.Invoke(this);
            }

            // ȿ�� ����
            private void Expire()
            {
                Debug.Log($"[Duration] {effectName} ����!");
                isActive = false;
                OnExpire?.Invoke(this);
            }

            // ȿ�� �Ͻ�����/�簳
            public void Pause()
            {
                isPaused = true;
                Debug.Log($"[Duration] {effectName} �Ͻ�����");
            }

            public void Resume()
            {
                isPaused = false;
                Debug.Log($"[Duration] {effectName} �簳");
            }

            // ȿ�� ����
            public void Refresh(DurationEffect newEffect)
            {
                switch (refreshPolicy)
                {
                    case RefreshPolicy.RestartDuration:
                        currentDuration = baseDuration;
                        elapsedTime = 0;
                        Debug.Log($"[Duration] {effectName} ���ӽð� ����");
                        break;

                    case RefreshPolicy.AddDuration:
                        currentDuration += newEffect.baseDuration;
                        Debug.Log($"[Duration] {effectName} ���ӽð� �߰� (+{newEffect.baseDuration})");
                        break;

                    case RefreshPolicy.MaxDuration:
                        currentDuration = Mathf.Max(currentDuration, newEffect.baseDuration);
                        Debug.Log($"[Duration] {effectName} �ִ� ���ӽð����� ����");
                        break;
                }
            }

            // ���� ����
            public void Remove()
            {
                Debug.Log($"[Duration] {effectName} ���� ����");
                isActive = false;
                OnRemove?.Invoke(this);
            }

            // ���� �ð� ����
            public float GetRemainingPercent()
            {
                if (durationType == EffectDuration.Infinite) return 1f;
                if (baseDuration <= 0) return 0;
                return currentDuration / baseDuration;
            }

            // ���ӽð� ���ڿ�
            public string GetDurationString()
            {
                switch (durationType)
                {
                    case EffectDuration.Instant:
                        return "���";
                    case EffectDuration.Duration:
                        return $"{currentDuration:F1}/{baseDuration:F1}s";
                    case EffectDuration.Infinite:
                        return "����";
                    case EffectDuration.UntilRemoved:
                        return "���ŵ� ������";
                    default:
                        return "?";
                }
            }

            public string GetDebugInfo()
            {
                string info = $"{effectName} [{GetDurationString()}]";
                if (isPeriodic)
                {
                    info += $" | Ticks: {periodicTicks}";
                }
                if (stackPolicy != StackPolicy.None)
                {
                    info += $" | Stack: {stackPolicy}";
                }
                return info;
            }
        }

        // Effect Manager
        [Serializable]
        public class EffectManager
        {
            public List<DurationEffect> activeEffects = new List<DurationEffect>();
            public Dictionary<string, List<DurationEffect>> effectsByName = new Dictionary<string, List<DurationEffect>>();

            public event Action<DurationEffect> OnEffectApplied;
            public event Action<DurationEffect> OnEffectRemoved;

            // ȿ�� ����
            public bool ApplyEffect(DurationEffect effect)
            {
                // ���� ��å üũ
                if (!effectsByName.ContainsKey(effect.effectName))
                {
                    effectsByName[effect.effectName] = new List<DurationEffect>();
                }

                var existingEffects = effectsByName[effect.effectName];

                switch (effect.stackPolicy)
                {
                    case DurationEffect.StackPolicy.None:
                        if (existingEffects.Count > 0)
                        {
                            Debug.Log($"[Manager] {effect.effectName} �̹� ���� (���� �Ұ�)");
                            return false;
                        }
                        break;

                    case DurationEffect.StackPolicy.Replace:
                        foreach (var existing in existingEffects)
                        {
                            RemoveEffect(existing);
                        }
                        break;

                    case DurationEffect.StackPolicy.Refresh:
                        if (existingEffects.Count > 0)
                        {
                            existingEffects[0].Refresh(effect);
                            return true;
                        }
                        break;

                    case DurationEffect.StackPolicy.Stack:
                        // �׳� �߰�
                        break;

                    case DurationEffect.StackPolicy.Unique:
                        // ���� �ν��Ͻ��� �߰�
                        break;
                }

                // ȿ�� �߰�
                activeEffects.Add(effect);
                effectsByName[effect.effectName].Add(effect);
                effect.Start();

                OnEffectApplied?.Invoke(effect);
                Debug.Log($"[Manager] {effect.effectName} �����");
                return true;
            }

            // ȿ�� ����
            public void RemoveEffect(DurationEffect effect)
            {
                if (!activeEffects.Contains(effect)) return;

                activeEffects.Remove(effect);

                if (effectsByName.ContainsKey(effect.effectName))
                {
                    effectsByName[effect.effectName].Remove(effect);
                    if (effectsByName[effect.effectName].Count == 0)
                    {
                        effectsByName.Remove(effect.effectName);
                    }
                }

                effect.Remove();
                OnEffectRemoved?.Invoke(effect);
            }

            // �̸����� ȿ�� ����
            public void RemoveEffectsByName(string effectName)
            {
                if (!effectsByName.ContainsKey(effectName)) return;

                var effectsToRemove = new List<DurationEffect>(effectsByName[effectName]);
                foreach (var effect in effectsToRemove)
                {
                    RemoveEffect(effect);
                }
            }

            // ��� ȿ�� ������Ʈ
            public void UpdateEffects(float deltaTime)
            {
                var effectsToRemove = new List<DurationEffect>();

                foreach (var effect in activeEffects)
                {
                    if (effect.Update(deltaTime))
                    {
                        effectsToRemove.Add(effect);
                    }
                }

                foreach (var effect in effectsToRemove)
                {
                    RemoveEffect(effect);
                }
            }

            // ȿ�� ���� üũ
            public bool HasEffect(string effectName)
            {
                return effectsByName.ContainsKey(effectName) && effectsByName[effectName].Count > 0;
            }

            // ȿ�� ����
            public int GetEffectCount(string effectName)
            {
                return effectsByName.ContainsKey(effectName) ? effectsByName[effectName].Count : 0;
            }

            // ��� ȿ�� ����
            public void ClearAll()
            {
                var effectsToRemove = new List<DurationEffect>(activeEffects);
                foreach (var effect in effectsToRemove)
                {
                    RemoveEffect(effect);
                }
            }

            public string GetStatus()
            {
                string status = $"Active Effects ({activeEffects.Count}):\n";
                foreach (var effect in activeEffects)
                {
                    status += $"  {effect.GetDebugInfo()}\n";
                }
                return status;
            }
        }

        [Header("Effect Managers")]
        [SerializeField] private EffectManager playerEffects;
        [SerializeField] private EffectManager enemyEffects;

        [Header("�׽�Ʈ ����")]
        [SerializeField] private float timeScale = 1f;
        [SerializeField] private bool showDebugInfo = true;

        private void Start()
        {
            InitializeManagers();

            Debug.Log("=== Phase 3 - Step 2: Duration Effects �׽�Ʈ ===");
            RunDurationTests();
        }

        private void InitializeManagers()
        {
            playerEffects = new EffectManager();
            enemyEffects = new EffectManager();

            // �̺�Ʈ ����
            playerEffects.OnEffectApplied += (effect) =>
            {
                if (showDebugInfo)
                    Debug.Log($"[Player] ȿ�� ����: {effect.effectName}");
            };

            playerEffects.OnEffectRemoved += (effect) =>
            {
                if (showDebugInfo)
                    Debug.Log($"[Player] ȿ�� ����: {effect.effectName}");
            };
        }

        private void RunDurationTests()
        {
            TestBasicDuration();
            TestPeriodicEffect();
            TestStackPolicies();
            TestValueCurves();
        }

        // �׽�Ʈ 1: �⺻ Duration
        private void TestBasicDuration()
        {
            Debug.Log("\n========== �׽�Ʈ 1: Basic Duration ==========");

            // 5�� ���� ����
            var buff = new DurationEffect("5�� ����", DurationEffect.EffectDuration.Duration, 5f);
            buff.baseValue = 10f;
            buff.currentValue = 10f;

            playerEffects.ApplyEffect(buff);

            Debug.Log($"���� ����: {buff.GetDurationString()}");
        }

        // �׽�Ʈ 2: �ֱ��� ȿ��
        private void TestPeriodicEffect()
        {
            Debug.Log("\n========== �׽�Ʈ 2: Periodic Effect ==========");

            // �� ȿ�� (6�ʰ� 1�ʸ��� ������)
            var poison = new DurationEffect("��", DurationEffect.EffectDuration.Duration, 6f);
            poison.isPeriodic = true;
            poison.periodInterval = 1f;
            poison.baseValue = 5f;
            poison.currentValue = 5f;

            poison.OnTick += (effect) =>
            {
                Debug.Log($"  �� ������: {effect.currentValue}");
            };

            playerEffects.ApplyEffect(poison);

            Debug.Log($"�� ����: 6�ʰ� 1�ʸ��� 5 ������");
        }

        // �׽�Ʈ 3: ���� ��å
        private void TestStackPolicies()
        {
            Debug.Log("\n========== �׽�Ʈ 3: Stack Policies ==========");

            // Replace ��å
            var replaceBuff = new DurationEffect("��ü�� ����", DurationEffect.EffectDuration.Duration, 3f);
            replaceBuff.stackPolicy = DurationEffect.StackPolicy.Replace;

            playerEffects.ApplyEffect(replaceBuff);
            Debug.Log("��ü�� ���� ù ��° ����");

            var replaceBuff2 = new DurationEffect("��ü�� ����", DurationEffect.EffectDuration.Duration, 5f);
            replaceBuff2.stackPolicy = DurationEffect.StackPolicy.Replace;

            playerEffects.ApplyEffect(replaceBuff2);
            Debug.Log("��ü�� ���� �� ��° ���� (ù ��°�� ���ŵ�)");

            // Stack ��å
            var stackBuff = new DurationEffect("��ø�� ����", DurationEffect.EffectDuration.Duration, 4f);
            stackBuff.stackPolicy = DurationEffect.StackPolicy.Stack;

            for (int i = 0; i < 3; i++)
            {
                var newStack = new DurationEffect("��ø�� ����", DurationEffect.EffectDuration.Duration, 4f);
                newStack.stackPolicy = DurationEffect.StackPolicy.Stack;
                enemyEffects.ApplyEffect(newStack);
                Debug.Log($"��ø�� ���� ���� {i + 1}");
            }
        }

        // �׽�Ʈ 4: �� �
        private void TestValueCurves()
        {
            Debug.Log("\n========== �׽�Ʈ 4: Value Curves ==========");

            // ���̵� ��/�ƿ� ȿ��
            var fadeBuff = new DurationEffect("���̵� ����", DurationEffect.EffectDuration.Duration, 10f);
            fadeBuff.baseValue = 20f;
            fadeBuff.hasFadeIn = true;
            fadeBuff.fadeInDuration = 2f;
            fadeBuff.hasFadeOut = true;
            fadeBuff.fadeOutDuration = 2f;

            // Ŀ���� �� � (�߰��� �ִ�)
            fadeBuff.valueCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            enemyEffects.ApplyEffect(fadeBuff);

            Debug.Log("���̵� ���� ����: 2�� ���̵���, 2�� ���̵�ƿ�");
        }

        private void Update()
        {
            float scaledDeltaTime = Time.deltaTime * timeScale;

            // ȿ�� ������Ʈ
            playerEffects?.UpdateEffects(scaledDeltaTime);
            enemyEffects?.UpdateEffects(scaledDeltaTime);

            HandleInput();
        }

        private void HandleInput()
        {
            // �پ��� Duration ȿ�� �׽�Ʈ
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ApplyInstantEffect();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ApplyShortDuration();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ApplyLongDuration();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                ApplyPeriodicEffect();
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                ApplyInfiniteEffect();
            }

            // �ð� ����
            if (Input.GetKeyDown(KeyCode.Q))
            {
                timeScale = 0.5f;
                Debug.Log("�ð� �ӵ�: 0.5x");
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                timeScale = 1f;
                Debug.Log("�ð� �ӵ�: 1x");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                timeScale = 2f;
                Debug.Log("�ð� �ӵ�: 2x");
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                playerEffects.ClearAll();
                Debug.Log("�÷��̾� ȿ�� ��� ����");
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void ApplyInstantEffect()
        {
            var instant = new DurationEffect("��� ȿ��", DurationEffect.EffectDuration.Instant);
            instant.baseValue = 50f;
            instant.currentValue = 50f;

            instant.OnTick += (effect) =>
            {
                Debug.Log($"��� ȿ�� �ߵ�! ��: {effect.currentValue}");
            };

            playerEffects.ApplyEffect(instant);
        }

        private void ApplyShortDuration()
        {
            var shortBuff = new DurationEffect("ª�� ����", DurationEffect.EffectDuration.Duration, 3f);
            shortBuff.baseValue = 15f;
            shortBuff.currentValue = 15f;
            shortBuff.stackPolicy = DurationEffect.StackPolicy.Refresh;
            shortBuff.refreshPolicy = DurationEffect.RefreshPolicy.RestartDuration;

            playerEffects.ApplyEffect(shortBuff);
        }

        private void ApplyLongDuration()
        {
            var longBuff = new DurationEffect("�� ����", DurationEffect.EffectDuration.Duration, 15f);
            longBuff.baseValue = 5f;
            longBuff.currentValue = 5f;
            longBuff.hasFadeIn = true;
            longBuff.fadeInDuration = 3f;

            playerEffects.ApplyEffect(longBuff);
        }

        private void ApplyPeriodicEffect()
        {
            var dot = new DurationEffect("�ֱ��� ������", DurationEffect.EffectDuration.Duration, 10f);
            dot.isPeriodic = true;
            dot.periodInterval = 0.5f;
            dot.baseValue = 3f;
            dot.currentValue = 3f;
            dot.maxTicks = 20;

            int tickCount = 0;
            dot.OnTick += (effect) =>
            {
                tickCount++;
                Debug.Log($"Tick #{tickCount}: {effect.currentValue} ������");
            };

            playerEffects.ApplyEffect(dot);
        }

        private void ApplyInfiniteEffect()
        {
            var passive = new DurationEffect("���� �нú�", DurationEffect.EffectDuration.Infinite);
            passive.baseValue = 10f;
            passive.currentValue = 10f;

            playerEffects.ApplyEffect(passive);
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== ���� ���� ===");
            Debug.Log("[Player Effects]");
            Debug.Log(playerEffects.GetStatus());
            Debug.Log("\n[Enemy Effects]");
            Debug.Log(enemyEffects.GetStatus());
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 400, 320), "Phase 3 - Duration Effects");

            int y = 40;
            GUI.Label(new Rect(20, y, 380, 20), $"Time Scale: {timeScale}x");
            y += 25;

            // Player Effects
            GUI.Label(new Rect(20, y, 380, 20), $"=== Player Effects ({playerEffects?.activeEffects.Count ?? 0}) ===");
            y += 25;

            if (playerEffects != null && playerEffects.activeEffects.Count > 0)
            {
                foreach (var effect in playerEffects.activeEffects)
                {
                    // ȿ�� �̸��� �ð�
                    GUI.Label(new Rect(30, y, 200, 20), effect.effectName);

                    // Progress Bar
                    if (effect.durationType == DurationEffect.EffectDuration.Duration)
                    {
                        float percent = effect.GetRemainingPercent();
                        Rect barRect = new Rect(230, y, 150, 18);
                        GUI.Box(barRect, "");
                        GUI.Box(new Rect(barRect.x, barRect.y, barRect.width * percent, barRect.height), "");
                        GUI.Label(barRect, $" {effect.currentDuration:F1}s", GUI.skin.label);
                    }
                    else if (effect.durationType == DurationEffect.EffectDuration.Infinite)
                    {
                        GUI.Label(new Rect(230, y, 150, 20), "����");
                    }

                    y += 22;

                    // �ֱ��� ȿ�� ����
                    if (effect.isPeriodic)
                    {
                        GUI.Label(new Rect(50, y, 330, 20), $"Ticks: {effect.periodicTicks} | Next: {(effect.nextPeriodTime - effect.elapsedTime):F1}s");
                        y += 20;
                    }
                }
            }
            else
            {
                GUI.Label(new Rect(30, y, 360, 20), "ȿ�� ����");
                y += 20;
            }

            // Enemy Effects
            y += 10;
            GUI.Label(new Rect(20, y, 380, 20), $"=== Enemy Effects ({enemyEffects?.activeEffects.Count ?? 0}) ===");
            y += 25;

            if (enemyEffects != null && enemyEffects.activeEffects.Count > 0)
            {
                foreach (var effect in enemyEffects.activeEffects)
                {
                    GUI.Label(new Rect(30, y, 350, 20), effect.GetDebugInfo());
                    y += 20;
                }
            }
            else
            {
                GUI.Label(new Rect(30, y, 360, 20), "ȿ�� ����");
                y += 20;
            }

            // ���۹�
            GUI.Box(new Rect(10, 340, 400, 100), "���۹�");
            GUI.Label(new Rect(20, 365, 380, 70),
                "ȿ��: 1-��� 2-ª�� 3-�� 4-�ֱ��� 5-����\n" +
                "�ð�: Q-0.5x W-1x E-2x\n" +
                "R: �÷��̾� ȿ�� ����  Space: ���� ���");
        }
    }
}