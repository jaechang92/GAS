// 파일 위치: Assets/Scripts/GAS/Learning/Phase1/ModifierTestExample.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAS.Learning.Phase1
{
    /// <summary>
    /// Step 2: AttributeModifier 이해 및 테스트
    /// </summary>
    public class ModifierTestExample : MonoBehaviour
    {
        // Modifier 작업 타입
        public enum ModifierOperation
        {
            Add,        // 고정값 더하기
            Multiply,   // 배수 적용
            Override    // 값 대체
        }

        // Modifier 타입
        public enum ModifierType
        {
            Permanent,  // 영구적
            Temporary,  // 일시적
            Duration    // 지속시간
        }

        // Modifier 클래스
        [System.Serializable]
        public class AttributeModifier
        {
            public string id;
            public string name;
            public ModifierOperation operation;
            public float value;
            public ModifierType modifierType;
            public float duration; // Duration 타입일 때만 사용
            public float remainingTime;
            public int priority; // 적용 우선순위 (낮을수록 먼저 적용)

            public AttributeModifier(string name, ModifierOperation op, float val, ModifierType type = ModifierType.Temporary, int priority = 0)
            {
                this.id = Guid.NewGuid().ToString();
                this.name = name;
                this.operation = op;
                this.value = val;
                this.modifierType = type;
                this.priority = priority;
                this.duration = 0;
                this.remainingTime = 0;
            }

            public override string ToString()
            {
                string opStr = operation switch
                {
                    ModifierOperation.Add => value >= 0 ? $"+{value}" : $"{value}",
                    ModifierOperation.Multiply => $"x{value}",
                    ModifierOperation.Override => $"={value}",
                    _ => "?"
                };
                return $"{name} ({opStr})";
            }
        }

        // 개선된 Attribute 클래스 (Modifier 지원)
        [System.Serializable]
        public class ModifiableAttribute
        {
            [SerializeField] private string name;
            [SerializeField] private float baseValue;
            [SerializeField] private float currentValue;
            [SerializeField] private float minValue;
            [SerializeField] private float maxValue;

            private List<AttributeModifier> modifiers = new List<AttributeModifier>();

            public event Action<float, float> OnValueChanged;
            public event Action<AttributeModifier> OnModifierAdded;
            public event Action<AttributeModifier> OnModifierRemoved;

            public string Name => name;
            public float BaseValue => baseValue;
            public float CurrentValue => currentValue;
            public float MinValue => minValue;
            public float MaxValue => maxValue;
            public float ModifiedValue { get; private set; }

            public ModifiableAttribute(string name, float baseValue, float minValue, float maxValue)
            {
                this.name = name;
                this.baseValue = baseValue;
                this.minValue = minValue;
                this.maxValue = maxValue;
                this.currentValue = baseValue;
                this.ModifiedValue = baseValue;
            }

            // Modifier 추가
            public void AddModifier(AttributeModifier modifier)
            {
                modifiers.Add(modifier);
                modifiers.Sort((a, b) => a.priority.CompareTo(b.priority));
                RecalculateValue();
                OnModifierAdded?.Invoke(modifier);

                Debug.Log($"[{name}] Modifier 추가: {modifier}");
            }

            // Modifier 제거
            public bool RemoveModifier(AttributeModifier modifier)
            {
                if (modifiers.Remove(modifier))
                {
                    RecalculateValue();
                    OnModifierRemoved?.Invoke(modifier);
                    Debug.Log($"[{name}] Modifier 제거: {modifier}");
                    return true;
                }
                return false;
            }

            // ID로 Modifier 제거
            public bool RemoveModifierById(string id)
            {
                var modifier = modifiers.FirstOrDefault(m => m.id == id);
                if (modifier != null)
                {
                    return RemoveModifier(modifier);
                }
                return false;
            }

            // 모든 Modifier 제거
            public void ClearModifiers()
            {
                modifiers.Clear();
                RecalculateValue();
                Debug.Log($"[{name}] 모든 Modifier 제거됨");
            }

            // 현재 값 직접 설정 (범위 제한 적용)
            public void SetCurrentValue(float value)
            {
                float oldValue = currentValue;
                currentValue = Mathf.Clamp(value, minValue, maxValue);

                if (Math.Abs(oldValue - currentValue) > 0.01f)
                {
                    OnValueChanged?.Invoke(oldValue, currentValue);
                    Debug.Log($"[{name}] CurrentValue 변경: {oldValue:F1} → {currentValue:F1}");
                }
            }

            // 현재 값에 더하기 (범위 제한 적용)
            public void AddToCurrentValue(float amount)
            {
                SetCurrentValue(currentValue + amount);
            }

            // ModifiedValue 기준으로 CurrentValue 리셋
            public void ResetToModifiedValue()
            {
                SetCurrentValue(ModifiedValue);
                Debug.Log($"[{name}] CurrentValue를 ModifiedValue로 리셋: {currentValue:F1}");
            }

            // 퍼센트 가져오기 (0~1)
            public float GetPercentage()
            {
                if (maxValue - minValue == 0) return 0;
                return (currentValue - minValue) / (maxValue - minValue);
            }

            // 최대값 대비 퍼센트로 설정
            public void SetCurrentValueByPercentage(float percentage)
            {
                float value = minValue + (maxValue - minValue) * Mathf.Clamp01(percentage);
                SetCurrentValue(value);
            }

            // 값 재계산 (Modifier 적용)
            private void RecalculateValue()
            {
                float oldModified = ModifiedValue;
                float oldCurrent = currentValue;
                float calcValue = baseValue;

                // Priority 순서대로 Modifier 적용
                var sortedModifiers = modifiers.OrderBy(m => m.priority).ToList();

                // 1단계: Override 적용 (있으면 다른 모든 것 무시)
                var overrideModifier = sortedModifiers.FirstOrDefault(m => m.operation == ModifierOperation.Override);
                if (overrideModifier != null)
                {
                    calcValue = overrideModifier.value;
                }
                else
                {
                    // 2단계: Add 적용
                    float totalAdd = sortedModifiers
                        .Where(m => m.operation == ModifierOperation.Add)
                        .Sum(m => m.value);
                    calcValue += totalAdd;

                    // 3단계: Multiply 적용
                    foreach (var modifier in sortedModifiers.Where(m => m.operation == ModifierOperation.Multiply))
                    {
                        calcValue *= modifier.value;
                    }
                }

                ModifiedValue = calcValue;

                // CurrentValue가 ModifiedValue를 추적하는 경우 (옵션)
                // 주석 해제하면 Modifier 변경 시 CurrentValue도 자동 조정됨
                // if (Math.Abs(currentValue - oldModified) < 0.01f)
                // {
                //     currentValue = Mathf.Clamp(ModifiedValue, minValue, maxValue);
                // }

                if (Math.Abs(oldModified - ModifiedValue) > 0.01f)
                {
                    Debug.Log($"[{name}] 재계산: Base({baseValue:F1}) → Modified({ModifiedValue:F1}) | Current({currentValue:F1})");
                }
            }

            // 활성 Modifier 목록
            public List<AttributeModifier> GetActiveModifiers()
            {
                return new List<AttributeModifier>(modifiers);
            }

            // 데미지/힐 처리 (현재값 기준)
            public float ApplyDamage(float damage)
            {
                float actualDamage = Mathf.Min(damage, currentValue - minValue);
                AddToCurrentValue(-actualDamage);
                return actualDamage; // 실제로 적용된 데미지 반환
            }

            public float ApplyHeal(float healAmount)
            {
                float actualHeal = Mathf.Min(healAmount, maxValue - currentValue);
                AddToCurrentValue(actualHeal);
                return actualHeal; // 실제로 적용된 힐 반환
            }

            public override string ToString()
            {
                return $"{name}: {currentValue:F1}/{ModifiedValue:F1} (Base: {baseValue:F1}) [{modifiers.Count} modifiers]";
            }
        }

        // 테스트용 속성들
        [Header("테스트 속성")]
        [SerializeField] private ModifiableAttribute attackPower;
        [SerializeField] private ModifiableAttribute defense;
        [SerializeField] private ModifiableAttribute moveSpeed;

        [Header("활성 Modifiers")]
        [SerializeField] private List<AttributeModifier> activeModifiers = new List<AttributeModifier>();

        private void Start()
        {
            InitializeAttributes();
            Debug.Log("=== Phase 1 - Step 2: Modifier 테스트 시작 ===");
            RunModifierTests();
        }

        private void InitializeAttributes()
        {
            attackPower = new ModifiableAttribute("공격력", 10f, 0f, 999f);
            defense = new ModifiableAttribute("방어력", 5f, 0f, 999f);
            moveSpeed = new ModifiableAttribute("이동속도", 5f, 0f, 20f);

            // 이벤트 구독
            attackPower.OnModifierAdded += (mod) => activeModifiers.Add(mod);
            attackPower.OnModifierRemoved += (mod) => activeModifiers.Remove(mod);
            defense.OnModifierAdded += (mod) => activeModifiers.Add(mod);
            defense.OnModifierRemoved += (mod) => activeModifiers.Remove(mod);
        }

        private void RunModifierTests()
        {
            TestScenario1_BasicModifiers();
            TestScenario2_ModifierStacking();
            TestScenario3_PrioritySystem();
            TestScenario4_OverrideModifier();
        }

        // 시나리오 1: 기본 Modifier 테스트
        private void TestScenario1_BasicModifiers()
        {
            Debug.Log("\n========== 시나리오 1: 기본 Modifier ==========");
            Debug.Log($"초기 공격력: {attackPower}");

            // 고정값 버프
            var atkBuff = new AttributeModifier("공격력 버프", ModifierOperation.Add, 5f);
            attackPower.AddModifier(atkBuff);
            Debug.Log($"공격력 +5 버프 후: {attackPower}");

            // 배수 버프
            var atkMultiplier = new AttributeModifier("공격력 증폭", ModifierOperation.Multiply, 1.5f);
            attackPower.AddModifier(atkMultiplier);
            Debug.Log($"공격력 x1.5 버프 후: {attackPower}");

            // 버프 제거
            attackPower.RemoveModifier(atkBuff);
            Debug.Log($"고정값 버프 제거 후: {attackPower}");

            attackPower.ClearModifiers();
            Debug.Log($"모든 버프 제거 후: {attackPower}");
        }

        // 시나리오 2: Modifier 스택
        private void TestScenario2_ModifierStacking()
        {
            Debug.Log("\n========== 시나리오 2: Modifier 스택 ==========");
            Debug.Log($"초기 방어력: {defense}");

            // 여러 개의 버프 스택
            var def1 = new AttributeModifier("방어력 버프 1", ModifierOperation.Add, 3f);
            var def2 = new AttributeModifier("방어력 버프 2", ModifierOperation.Add, 2f);
            var def3 = new AttributeModifier("방어력 증폭", ModifierOperation.Multiply, 2f);

            defense.AddModifier(def1);
            Debug.Log($"버프1(+3) 후: {defense}");

            defense.AddModifier(def2);
            Debug.Log($"버프2(+2) 후: {defense}");

            defense.AddModifier(def3);
            Debug.Log($"증폭(x2) 후: {defense}");
            // 계산: (5 + 3 + 2) * 2 = 20

            defense.ClearModifiers();
        }

        // 시나리오 3: Priority 시스템
        private void TestScenario3_PrioritySystem()
        {
            Debug.Log("\n========== 시나리오 3: Priority 시스템 ==========");
            Debug.Log($"초기 이동속도: {moveSpeed}");

            // Priority가 다른 Modifier들
            var speed1 = new AttributeModifier("기본 신발", ModifierOperation.Add, 2f, ModifierType.Temporary, priority: 1);
            var speed2 = new AttributeModifier("스프린트", ModifierOperation.Multiply, 1.5f, ModifierType.Temporary, priority: 2);
            var speed3 = new AttributeModifier("초기 버프", ModifierOperation.Add, 1f, ModifierType.Temporary, priority: 0);

            moveSpeed.AddModifier(speed1);
            moveSpeed.AddModifier(speed2);
            moveSpeed.AddModifier(speed3);

            Debug.Log($"모든 버프 적용 후: {moveSpeed}");
            // Priority 순서: speed3(0) → speed1(1) → speed2(2)
            // 계산: (5 + 1 + 2) * 1.5 = 12

            moveSpeed.ClearModifiers();
        }

        // 시나리오 4: Override Modifier
        private void TestScenario4_OverrideModifier()
        {
            Debug.Log("\n========== 시나리오 4: Override Modifier ==========");
            Debug.Log($"초기 공격력: {attackPower}");

            var buff = new AttributeModifier("일반 버프", ModifierOperation.Add, 10f);
            attackPower.AddModifier(buff);
            Debug.Log($"일반 버프(+10) 후: {attackPower}");

            var overrideMod = new AttributeModifier("강제 설정", ModifierOperation.Override, 50f);
            attackPower.AddModifier(overrideMod);
            Debug.Log($"Override(=50) 후: {attackPower}");
            // Override가 있으면 다른 모든 Modifier 무시

            attackPower.RemoveModifier(overrideMod);
            Debug.Log($"Override 제거 후: {attackPower}");
            // 다시 일반 버프만 적용

            attackPower.ClearModifiers();

            // 시나리오 5: CurrentValue 직접 조작 테스트
            TestScenario5_CurrentValueManipulation();
        }

        // 시나리오 5: CurrentValue 직접 조작
        private void TestScenario5_CurrentValueManipulation()
        {
            Debug.Log("\n========== 시나리오 5: CurrentValue 직접 조작 ==========");

            // 초기 상태
            Debug.Log($"초기 방어력: {defense}");
            Debug.Log($"  Base: {defense.BaseValue}, Modified: {defense.ModifiedValue}, Current: {defense.CurrentValue}");

            // Modifier 적용
            var defBuff = new AttributeModifier("방어 버프", ModifierOperation.Add, 10f);
            defense.AddModifier(defBuff);
            Debug.Log($"버프 적용 후: Base: {defense.BaseValue}, Modified: {defense.ModifiedValue}, Current: {defense.CurrentValue}");

            // CurrentValue 직접 감소 (데미지)
            float damage = defense.ApplyDamage(8f);
            Debug.Log($"8 데미지 받음 (실제: {damage:F1}): Current: {defense.CurrentValue}");

            // CurrentValue 직접 증가 (힐)
            float heal = defense.ApplyHeal(5f);
            Debug.Log($"5 힐 받음 (실제: {heal:F1}): Current: {defense.CurrentValue}");

            // CurrentValue를 ModifiedValue로 리셋
            defense.ResetToModifiedValue();
            Debug.Log($"리셋 후: Current: {defense.CurrentValue} (Modified: {defense.ModifiedValue})");

            // 퍼센트로 설정
            defense.SetCurrentValueByPercentage(0.5f);
            Debug.Log($"50% 설정: Current: {defense.CurrentValue} ({defense.GetPercentage():P0})");

            defense.ClearModifiers();
            defense.ResetToModifiedValue();
        }

        // Update에서 Duration 타입 Modifier 처리
        private void Update()
        {
            // Duration 타입 Modifier들의 시간 처리
            var modifiersToRemove = new List<AttributeModifier>();

            foreach (var modifier in activeModifiers.Where(m => m.modifierType == ModifierType.Duration))
            {
                modifier.remainingTime -= Time.deltaTime;
                if (modifier.remainingTime <= 0)
                {
                    modifiersToRemove.Add(modifier);
                }
            }

            // 만료된 Modifier 제거
            foreach (var modifier in modifiersToRemove)
            {
                RemoveModifierFromAttribute(modifier);
            }

            // 테스트 입력
            HandleTestInput();
        }

        private void HandleTestInput()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                var mod = new AttributeModifier("테스트 버프", ModifierOperation.Add, 5f);
                attackPower.AddModifier(mod);
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                var mod = new AttributeModifier("테스트 증폭", ModifierOperation.Multiply, 1.5f);
                attackPower.AddModifier(mod);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                attackPower.ClearModifiers();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                // CurrentValue에 데미지
                float damage = attackPower.ApplyDamage(3f);
                Debug.Log($"공격력에 {damage:F1} 데미지 적용");
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                // CurrentValue에 힐
                float heal = attackPower.ApplyHeal(5f);
                Debug.Log($"공격력에 {heal:F1} 힐 적용");
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                // CurrentValue를 ModifiedValue로 리셋
                attackPower.ResetToModifiedValue();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                // 50%로 설정
                attackPower.SetCurrentValueByPercentage(0.5f);
                Debug.Log($"공격력을 50%로 설정: {attackPower.CurrentValue:F1}");
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintAllAttributes();
            }
        }

        private void RemoveModifierFromAttribute(AttributeModifier modifier)
        {
            attackPower.RemoveModifier(modifier);
            defense.RemoveModifier(modifier);
            moveSpeed.RemoveModifier(modifier);
        }

        private void PrintAllAttributes()
        {
            Debug.Log("\n=== 현재 속성 상태 ===");
            Debug.Log($"공격력 - Base: {attackPower.BaseValue:F1} | Modified: {attackPower.ModifiedValue:F1} | Current: {attackPower.CurrentValue:F1} ({attackPower.GetPercentage():P0})");
            Debug.Log($"방어력 - Base: {defense.BaseValue:F1} | Modified: {defense.ModifiedValue:F1} | Current: {defense.CurrentValue:F1} ({defense.GetPercentage():P0})");
            Debug.Log($"이동속도 - Base: {moveSpeed.BaseValue:F1} | Modified: {moveSpeed.ModifiedValue:F1} | Current: {moveSpeed.CurrentValue:F1} ({moveSpeed.GetPercentage():P0})");
            Debug.Log($"활성 Modifier 수: {activeModifiers.Count}");

            if (activeModifiers.Count > 0)
            {
                Debug.Log("활성 Modifiers:");
                foreach (var mod in activeModifiers)
                {
                    Debug.Log($"  - {mod}");
                }
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 350, 250), "Phase 1 - Modifier 테스트");

            int y = 40;
            GUI.Label(new Rect(20, y, 330, 20), $"공격력: {attackPower?.CurrentValue:F1}/{attackPower?.ModifiedValue:F1} (Base: {attackPower?.BaseValue:F1})");
            y += 25;
            GUI.Label(new Rect(20, y, 330, 20), $"방어력: {defense?.CurrentValue:F1}/{defense?.ModifiedValue:F1} (Base: {defense?.BaseValue:F1})");
            y += 25;
            GUI.Label(new Rect(20, y, 330, 20), $"이동속도: {moveSpeed?.CurrentValue:F1}/{moveSpeed?.ModifiedValue:F1} (Base: {moveSpeed?.BaseValue:F1})");
            y += 30;
            GUI.Label(new Rect(20, y, 330, 20), $"활성 Modifiers: {activeModifiers?.Count ?? 0}개");
            y += 30;

            // 조작 설명
            GUI.Label(new Rect(20, y, 330, 30), "=== Modifier 조작 ===");
            y += 25;
            GUI.Label(new Rect(20, y, 330, 20), "Q: 공격력+5  W: 공격력x1.5  E: 모든버프제거");
            y += 20;
            GUI.Label(new Rect(20, y, 330, 20), "=== CurrentValue 조작 ===");
            y += 20;
            GUI.Label(new Rect(20, y, 330, 20), "A: 데미지  S: 힐  D: 리셋  F: 50%설정");
            y += 20;
            GUI.Label(new Rect(20, y, 330, 20), "Space: 상태 출력");

            // 값 표시 설명
            y += 30;
            GUI.Label(new Rect(20, y, 330, 20), "표시: Current/Modified (Base)");
        }
    }
}