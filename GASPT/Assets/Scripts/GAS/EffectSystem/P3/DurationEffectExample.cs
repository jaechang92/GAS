// 파일 위치: Assets/Scripts/GAS/Learning/Phase3/DurationEffectExample.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAS.Learning.Phase3
{
    /// <summary>
    /// Step 2: Duration Effects - 시간 기반 효과 관리
    /// </summary>
    public class DurationEffectExample : MonoBehaviour
    {
        // Duration 관리가 강화된 Effect
        [Serializable]
        public class DurationEffect
        {
            [Header("기본 정보")]
            public string effectName;
            public string description;
            public EffectDuration durationType;

            public enum EffectDuration
            {
                Instant,        // 즉시
                Duration,       // 지속시간
                Infinite,       // 영구
                UntilRemoved    // 제거될 때까지
            }

            [Header("시간 설정")]
            public float baseDuration;
            public float currentDuration;
            public float elapsedTime;

            [Header("주기적 효과")]
            public bool isPeriodic;
            public float periodInterval = 1f;
            public float nextPeriodTime;
            public int periodicTicks;
            public int maxTicks = -1; // -1은 무한

            [Header("효과 값")]
            public float baseValue;
            public float currentValue;
            public AnimationCurve valueCurve; // 시간에 따른 효과 변화

            [Header("스택 & 갱신")]
            public StackPolicy stackPolicy;
            public RefreshPolicy refreshPolicy;

            public enum StackPolicy
            {
                None,           // 스택 불가
                Replace,        // 교체
                Stack,          // 중첩
                Unique,         // 개별 인스턴스
                Refresh         // 시간 갱신
            }

            public enum RefreshPolicy
            {
                None,           // 갱신 안함
                RestartDuration,// 지속시간 리셋
                AddDuration,    // 지속시간 추가
                MaxDuration     // 최대값으로
            }

            [Header("페이드 효과")]
            public bool hasFadeIn;
            public float fadeInDuration = 0.5f;
            public bool hasFadeOut;
            public float fadeOutDuration = 1f;

            // 이벤트
            public event Action<DurationEffect> OnStart;
            public event Action<DurationEffect> OnTick;
            public event Action<DurationEffect> OnExpire;
            public event Action<DurationEffect> OnRemove;

            // 상태
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

                // 기본 값 곡선 (일정한 값)
                valueCurve = AnimationCurve.Constant(0, 1, 1);
            }

            // 효과 시작
            public void Start()
            {
                if (isActive) return;

                isActive = true;
                elapsedTime = 0;
                periodicTicks = 0;
                nextPeriodTime = isPeriodic ? periodInterval : float.MaxValue;

                Debug.Log($"[Duration] {effectName} 시작 (Duration: {GetDurationString()})");
                OnStart?.Invoke(this);

                // 즉시 효과는 바로 틱
                if (durationType == EffectDuration.Instant)
                {
                    Tick();
                    Expire();
                }
                else if (isPeriodic && periodInterval <= 0)
                {
                    Tick(); // 첫 틱은 즉시
                }
            }

            // 시간 업데이트
            public bool Update(float deltaTime)
            {
                if (!isActive || isPaused) return false;
                if (durationType == EffectDuration.Instant) return true;
                if (durationType == EffectDuration.Infinite || durationType == EffectDuration.UntilRemoved) return false;

                elapsedTime += deltaTime;

                // 값 곡선 적용
                if (valueCurve != null && valueCurve.length > 0)
                {
                    float normalizedTime = elapsedTime / baseDuration;
                    float curveValue = valueCurve.Evaluate(normalizedTime);
                    currentValue = baseValue * curveValue;

                    // Fade 효과
                    if (hasFadeIn && elapsedTime < fadeInDuration)
                    {
                        currentValue *= (elapsedTime / fadeInDuration);
                    }
                    else if (hasFadeOut && (currentDuration - elapsedTime) < fadeOutDuration)
                    {
                        currentValue *= ((currentDuration - elapsedTime) / fadeOutDuration);
                    }
                }

                // 주기적 틱
                if (isPeriodic && elapsedTime >= nextPeriodTime)
                {
                    Tick();
                    nextPeriodTime += periodInterval;

                    // 최대 틱 체크
                    if (maxTicks > 0 && periodicTicks >= maxTicks)
                    {
                        Expire();
                        return true;
                    }
                }

                // Duration 체크
                currentDuration -= deltaTime;
                if (currentDuration <= 0)
                {
                    Expire();
                    return true;
                }

                return false;
            }

            // 효과 틱
            private void Tick()
            {
                periodicTicks++;
                Debug.Log($"[Duration] {effectName} Tick #{periodicTicks} (Value: {currentValue:F1})");
                OnTick?.Invoke(this);
            }

            // 효과 만료
            private void Expire()
            {
                Debug.Log($"[Duration] {effectName} 만료!");
                isActive = false;
                OnExpire?.Invoke(this);
            }

            // 효과 일시정지/재개
            public void Pause()
            {
                isPaused = true;
                Debug.Log($"[Duration] {effectName} 일시정지");
            }

            public void Resume()
            {
                isPaused = false;
                Debug.Log($"[Duration] {effectName} 재개");
            }

            // 효과 갱신
            public void Refresh(DurationEffect newEffect)
            {
                switch (refreshPolicy)
                {
                    case RefreshPolicy.RestartDuration:
                        currentDuration = baseDuration;
                        elapsedTime = 0;
                        Debug.Log($"[Duration] {effectName} 지속시간 리셋");
                        break;

                    case RefreshPolicy.AddDuration:
                        currentDuration += newEffect.baseDuration;
                        Debug.Log($"[Duration] {effectName} 지속시간 추가 (+{newEffect.baseDuration})");
                        break;

                    case RefreshPolicy.MaxDuration:
                        currentDuration = Mathf.Max(currentDuration, newEffect.baseDuration);
                        Debug.Log($"[Duration] {effectName} 최대 지속시간으로 설정");
                        break;
                }
            }

            // 수동 제거
            public void Remove()
            {
                Debug.Log($"[Duration] {effectName} 수동 제거");
                isActive = false;
                OnRemove?.Invoke(this);
            }

            // 남은 시간 비율
            public float GetRemainingPercent()
            {
                if (durationType == EffectDuration.Infinite) return 1f;
                if (baseDuration <= 0) return 0;
                return currentDuration / baseDuration;
            }

            // 지속시간 문자열
            public string GetDurationString()
            {
                switch (durationType)
                {
                    case EffectDuration.Instant:
                        return "즉시";
                    case EffectDuration.Duration:
                        return $"{currentDuration:F1}/{baseDuration:F1}s";
                    case EffectDuration.Infinite:
                        return "영구";
                    case EffectDuration.UntilRemoved:
                        return "제거될 때까지";
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

            // 효과 적용
            public bool ApplyEffect(DurationEffect effect)
            {
                // 스택 정책 체크
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
                            Debug.Log($"[Manager] {effect.effectName} 이미 존재 (스택 불가)");
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
                        // 그냥 추가
                        break;

                    case DurationEffect.StackPolicy.Unique:
                        // 개별 인스턴스로 추가
                        break;
                }

                // 효과 추가
                activeEffects.Add(effect);
                effectsByName[effect.effectName].Add(effect);
                effect.Start();

                OnEffectApplied?.Invoke(effect);
                Debug.Log($"[Manager] {effect.effectName} 적용됨");
                return true;
            }

            // 효과 제거
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

            // 이름으로 효과 제거
            public void RemoveEffectsByName(string effectName)
            {
                if (!effectsByName.ContainsKey(effectName)) return;

                var effectsToRemove = new List<DurationEffect>(effectsByName[effectName]);
                foreach (var effect in effectsToRemove)
                {
                    RemoveEffect(effect);
                }
            }

            // 모든 효과 업데이트
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

            // 효과 존재 체크
            public bool HasEffect(string effectName)
            {
                return effectsByName.ContainsKey(effectName) && effectsByName[effectName].Count > 0;
            }

            // 효과 개수
            public int GetEffectCount(string effectName)
            {
                return effectsByName.ContainsKey(effectName) ? effectsByName[effectName].Count : 0;
            }

            // 모든 효과 제거
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

        [Header("테스트 설정")]
        [SerializeField] private float timeScale = 1f;
        [SerializeField] private bool showDebugInfo = true;

        private void Start()
        {
            InitializeManagers();

            Debug.Log("=== Phase 3 - Step 2: Duration Effects 테스트 ===");
            RunDurationTests();
        }

        private void InitializeManagers()
        {
            playerEffects = new EffectManager();
            enemyEffects = new EffectManager();

            // 이벤트 구독
            playerEffects.OnEffectApplied += (effect) =>
            {
                if (showDebugInfo)
                    Debug.Log($"[Player] 효과 적용: {effect.effectName}");
            };

            playerEffects.OnEffectRemoved += (effect) =>
            {
                if (showDebugInfo)
                    Debug.Log($"[Player] 효과 제거: {effect.effectName}");
            };
        }

        private void RunDurationTests()
        {
            TestBasicDuration();
            TestPeriodicEffect();
            TestStackPolicies();
            TestValueCurves();
        }

        // 테스트 1: 기본 Duration
        private void TestBasicDuration()
        {
            Debug.Log("\n========== 테스트 1: Basic Duration ==========");

            // 5초 지속 버프
            var buff = new DurationEffect("5초 버프", DurationEffect.EffectDuration.Duration, 5f);
            buff.baseValue = 10f;
            buff.currentValue = 10f;

            playerEffects.ApplyEffect(buff);

            Debug.Log($"버프 적용: {buff.GetDurationString()}");
        }

        // 테스트 2: 주기적 효과
        private void TestPeriodicEffect()
        {
            Debug.Log("\n========== 테스트 2: Periodic Effect ==========");

            // 독 효과 (6초간 1초마다 데미지)
            var poison = new DurationEffect("독", DurationEffect.EffectDuration.Duration, 6f);
            poison.isPeriodic = true;
            poison.periodInterval = 1f;
            poison.baseValue = 5f;
            poison.currentValue = 5f;

            poison.OnTick += (effect) =>
            {
                Debug.Log($"  독 데미지: {effect.currentValue}");
            };

            playerEffects.ApplyEffect(poison);

            Debug.Log($"독 적용: 6초간 1초마다 5 데미지");
        }

        // 테스트 3: 스택 정책
        private void TestStackPolicies()
        {
            Debug.Log("\n========== 테스트 3: Stack Policies ==========");

            // Replace 정책
            var replaceBuff = new DurationEffect("교체형 버프", DurationEffect.EffectDuration.Duration, 3f);
            replaceBuff.stackPolicy = DurationEffect.StackPolicy.Replace;

            playerEffects.ApplyEffect(replaceBuff);
            Debug.Log("교체형 버프 첫 번째 적용");

            var replaceBuff2 = new DurationEffect("교체형 버프", DurationEffect.EffectDuration.Duration, 5f);
            replaceBuff2.stackPolicy = DurationEffect.StackPolicy.Replace;

            playerEffects.ApplyEffect(replaceBuff2);
            Debug.Log("교체형 버프 두 번째 적용 (첫 번째는 제거됨)");

            // Stack 정책
            var stackBuff = new DurationEffect("중첩형 버프", DurationEffect.EffectDuration.Duration, 4f);
            stackBuff.stackPolicy = DurationEffect.StackPolicy.Stack;

            for (int i = 0; i < 3; i++)
            {
                var newStack = new DurationEffect("중첩형 버프", DurationEffect.EffectDuration.Duration, 4f);
                newStack.stackPolicy = DurationEffect.StackPolicy.Stack;
                enemyEffects.ApplyEffect(newStack);
                Debug.Log($"중첩형 버프 스택 {i + 1}");
            }
        }

        // 테스트 4: 값 곡선
        private void TestValueCurves()
        {
            Debug.Log("\n========== 테스트 4: Value Curves ==========");

            // 페이드 인/아웃 효과
            var fadeBuff = new DurationEffect("페이드 버프", DurationEffect.EffectDuration.Duration, 10f);
            fadeBuff.baseValue = 20f;
            fadeBuff.hasFadeIn = true;
            fadeBuff.fadeInDuration = 2f;
            fadeBuff.hasFadeOut = true;
            fadeBuff.fadeOutDuration = 2f;

            // 커스텀 값 곡선 (중간에 최대)
            fadeBuff.valueCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            enemyEffects.ApplyEffect(fadeBuff);

            Debug.Log("페이드 버프 적용: 2초 페이드인, 2초 페이드아웃");
        }

        private void Update()
        {
            float scaledDeltaTime = Time.deltaTime * timeScale;

            // 효과 업데이트
            playerEffects?.UpdateEffects(scaledDeltaTime);
            enemyEffects?.UpdateEffects(scaledDeltaTime);

            HandleInput();
        }

        private void HandleInput()
        {
            // 다양한 Duration 효과 테스트
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

            // 시간 조절
            if (Input.GetKeyDown(KeyCode.Q))
            {
                timeScale = 0.5f;
                Debug.Log("시간 속도: 0.5x");
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                timeScale = 1f;
                Debug.Log("시간 속도: 1x");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                timeScale = 2f;
                Debug.Log("시간 속도: 2x");
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                playerEffects.ClearAll();
                Debug.Log("플레이어 효과 모두 제거");
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void ApplyInstantEffect()
        {
            var instant = new DurationEffect("즉시 효과", DurationEffect.EffectDuration.Instant);
            instant.baseValue = 50f;
            instant.currentValue = 50f;

            instant.OnTick += (effect) =>
            {
                Debug.Log($"즉시 효과 발동! 값: {effect.currentValue}");
            };

            playerEffects.ApplyEffect(instant);
        }

        private void ApplyShortDuration()
        {
            var shortBuff = new DurationEffect("짧은 버프", DurationEffect.EffectDuration.Duration, 3f);
            shortBuff.baseValue = 15f;
            shortBuff.currentValue = 15f;
            shortBuff.stackPolicy = DurationEffect.StackPolicy.Refresh;
            shortBuff.refreshPolicy = DurationEffect.RefreshPolicy.RestartDuration;

            playerEffects.ApplyEffect(shortBuff);
        }

        private void ApplyLongDuration()
        {
            var longBuff = new DurationEffect("긴 버프", DurationEffect.EffectDuration.Duration, 15f);
            longBuff.baseValue = 5f;
            longBuff.currentValue = 5f;
            longBuff.hasFadeIn = true;
            longBuff.fadeInDuration = 3f;

            playerEffects.ApplyEffect(longBuff);
        }

        private void ApplyPeriodicEffect()
        {
            var dot = new DurationEffect("주기적 데미지", DurationEffect.EffectDuration.Duration, 10f);
            dot.isPeriodic = true;
            dot.periodInterval = 0.5f;
            dot.baseValue = 3f;
            dot.currentValue = 3f;
            dot.maxTicks = 20;

            int tickCount = 0;
            dot.OnTick += (effect) =>
            {
                tickCount++;
                Debug.Log($"Tick #{tickCount}: {effect.currentValue} 데미지");
            };

            playerEffects.ApplyEffect(dot);
        }

        private void ApplyInfiniteEffect()
        {
            var passive = new DurationEffect("영구 패시브", DurationEffect.EffectDuration.Infinite);
            passive.baseValue = 10f;
            passive.currentValue = 10f;

            playerEffects.ApplyEffect(passive);
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== 현재 상태 ===");
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
                    // 효과 이름과 시간
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
                        GUI.Label(new Rect(230, y, 150, 20), "영구");
                    }

                    y += 22;

                    // 주기적 효과 정보
                    if (effect.isPeriodic)
                    {
                        GUI.Label(new Rect(50, y, 330, 20), $"Ticks: {effect.periodicTicks} | Next: {(effect.nextPeriodTime - effect.elapsedTime):F1}s");
                        y += 20;
                    }
                }
            }
            else
            {
                GUI.Label(new Rect(30, y, 360, 20), "효과 없음");
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
                GUI.Label(new Rect(30, y, 360, 20), "효과 없음");
                y += 20;
            }

            // 조작법
            GUI.Box(new Rect(10, 340, 400, 100), "조작법");
            GUI.Label(new Rect(20, 365, 380, 70),
                "효과: 1-즉시 2-짧은 3-긴 4-주기적 5-영구\n" +
                "시간: Q-0.5x W-1x E-2x\n" +
                "R: 플레이어 효과 제거  Space: 상태 출력");
        }
    }
}