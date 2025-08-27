// 파일 위치: Assets/Scripts/GAS/Learning/Phase3/BasicEffectExample.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using GAS.Learning.Phase1;
using GAS.Learning.Phase2;

namespace GAS.Learning.Phase3
{
    /// <summary>
    /// Step 1: GameplayEffect 기초 - 효과의 기본 구조와 적용
    /// </summary>
    public class GameplayEffect : MonoBehaviour
    {
        // 간단한 Effect 구현
        [Serializable]
        public class SimpleGameplayEffect
        {
            // 효과 기본 정보
            [Header("기본 정보")]
            public string effectName;
            public string description;
            public Sprite icon;

            // Duration 타입
            public enum DurationType
            {
                Instant,    // 즉시 적용
                Duration,   // 일정 시간
                Infinite    // 영구
            }

            [Header("Duration 설정")]
            public DurationType durationType = DurationType.Instant;
            public float duration = 0f;
            public float remainingTime = 0f;

            // Attribute 수정자
            [Header("Attribute Modifiers")]
            public List<AttributeModifier> modifiers = new List<AttributeModifier>();

            [Serializable]
            public class AttributeModifier
            {
                public string attributeName;
                public ModifierOperation operation;
                public float value;

                public enum ModifierOperation
                {
                    Add,
                    Multiply,
                    Override
                }

                public AttributeModifier(string name, ModifierOperation op, float val)
                {
                    attributeName = name;
                    operation = op;
                    value = val;
                }

                public void Apply(Dictionary<string, float> attributes)
                {
                    if (!attributes.ContainsKey(attributeName))
                    {
                        Debug.LogWarning($"Attribute {attributeName} not found!");
                        return;
                    }

                    switch (operation)
                    {
                        case ModifierOperation.Add:
                            attributes[attributeName] += value;
                            break;
                        case ModifierOperation.Multiply:
                            attributes[attributeName] *= value;
                            break;
                        case ModifierOperation.Override:
                            attributes[attributeName] = value;
                            break;
                    }

                    Debug.Log($"[Modifier] {attributeName} {operation} {value} → Result: {attributes[attributeName]}");
                }

                public void Remove(Dictionary<string, float> attributes)
                {
                    // Duration 효과 제거 시 원래값 복원 로직
                    // 실제로는 더 복잡한 시스템 필요
                    Debug.Log($"[Modifier] {attributeName} 효과 제거");
                }
            }

            // Tag 변경
            [Header("Tag 변경")]
            public List<string> grantedTags = new List<string>();
            public List<string> removedTags = new List<string>();

            // 스택 설정
            [Header("스택 설정")]
            public bool isStackable = false;
            public int maxStacks = 1;
            public int currentStacks = 0;

            // 효과 ID (고유 식별자)
            private string effectId;

            public SimpleGameplayEffect(string name, DurationType type, float duration = 0)
            {
                this.effectName = name;
                this.durationType = type;
                this.duration = duration;
                this.remainingTime = duration;
                this.effectId = Guid.NewGuid().ToString();
            }

            // 효과 적용 가능 체크
            public bool CanApply(EffectTarget target)
            {
                // 태그 조건 체크
                foreach (var requiredTag in removedTags)
                {
                    if (target.HasTag(requiredTag))
                    {
                        Debug.Log($"[Effect] {effectName} 적용 불가: 차단 태그 {requiredTag}");
                        return false;
                    }
                }

                // 스택 체크
                if (!isStackable && target.HasEffect(effectName))
                {
                    Debug.Log($"[Effect] {effectName} 적용 불가: 이미 존재 (스택 불가)");
                    return false;
                }

                return true;
            }

            // 효과 적용
            public void Apply(EffectTarget target)
            {
                if (!CanApply(target))
                {
                    Debug.LogWarning($"[Effect] {effectName} 적용 실패!");
                    return;
                }

                Debug.Log($"[Effect] === {effectName} 적용 시작 ===");

                // 1. Attribute 수정
                foreach (var modifier in modifiers)
                {
                    modifier.Apply(target.attributes);
                }

                // 2. Tag 부여
                foreach (var tag in grantedTags)
                {
                    target.AddTag(tag);
                    Debug.Log($"[Effect] Tag 추가: {tag}");
                }

                // 3. Tag 제거
                foreach (var tag in removedTags)
                {
                    target.RemoveTag(tag);
                    Debug.Log($"[Effect] Tag 제거: {tag}");
                }

                // 4. 스택 처리
                if (isStackable)
                {
                    currentStacks = Math.Min(currentStacks + 1, maxStacks);
                    Debug.Log($"[Effect] 스택 증가: {currentStacks}/{maxStacks}");
                }

                Debug.Log($"[Effect] === {effectName} 적용 완료 ===");
            }

            // 효과 제거
            public void Remove(EffectTarget target)
            {
                Debug.Log($"[Effect] === {effectName} 제거 시작 ===");

                // Duration/Infinite 효과만 제거 처리
                if (durationType != DurationType.Instant)
                {
                    // Modifier 제거 (실제로는 복잡한 복원 로직 필요)
                    foreach (var modifier in modifiers)
                    {
                        modifier.Remove(target.attributes);
                    }

                    // Tag 제거
                    foreach (var tag in grantedTags)
                    {
                        target.RemoveTag(tag);
                    }
                }

                Debug.Log($"[Effect] === {effectName} 제거 완료 ===");
            }

            // 시간 업데이트
            public bool UpdateTime(float deltaTime)
            {
                if (durationType != DurationType.Duration)
                    return false;

                remainingTime -= deltaTime;

                if (remainingTime <= 0)
                {
                    Debug.Log($"[Effect] {effectName} 시간 만료!");
                    return true; // 제거 필요
                }

                return false;
            }

            public string GetDebugInfo()
            {
                string info = $"[{effectName}]\n";
                info += $"  Type: {durationType}\n";
                if (durationType == DurationType.Duration)
                {
                    info += $"  Time: {remainingTime:F1}/{duration:F1}\n";
                }
                if (isStackable)
                {
                    info += $"  Stacks: {currentStacks}/{maxStacks}\n";
                }
                return info;
            }
        }

        // Effect 대상 (캐릭터)
        [Serializable]
        public class EffectTarget
        {
            public string targetName;
            public Dictionary<string, float> attributes = new Dictionary<string, float>();
            public List<string> tags = new List<string>();
            public List<SimpleGameplayEffect> activeEffects = new List<SimpleGameplayEffect>();

            public EffectTarget(string name)
            {
                targetName = name;
                InitializeAttributes();
            }

            private void InitializeAttributes()
            {
                attributes["Health"] = 100f;
                attributes["MaxHealth"] = 100f;
                attributes["Mana"] = 50f;
                attributes["MaxMana"] = 50f;
                attributes["AttackPower"] = 10f;
                attributes["Defense"] = 5f;
                attributes["MoveSpeed"] = 5f;
            }

            public bool HasTag(string tag)
            {
                return tags.Contains(tag);
            }

            public void AddTag(string tag)
            {
                if (!tags.Contains(tag))
                {
                    tags.Add(tag);
                }
            }

            public void RemoveTag(string tag)
            {
                tags.Remove(tag);
            }

            public bool HasEffect(string effectName)
            {
                return activeEffects.Exists(e => e.effectName == effectName);
            }

            public void ApplyEffect(SimpleGameplayEffect effect)
            {
                effect.Apply(this);

                // Duration/Infinite 효과는 목록에 추가
                if (effect.durationType != SimpleGameplayEffect.DurationType.Instant)
                {
                    activeEffects.Add(effect);
                }
            }

            public void RemoveEffect(SimpleGameplayEffect effect)
            {
                effect.Remove(this);
                activeEffects.Remove(effect);
            }

            public void UpdateEffects(float deltaTime)
            {
                var effectsToRemove = new List<SimpleGameplayEffect>();

                foreach (var effect in activeEffects)
                {
                    if (effect.UpdateTime(deltaTime))
                    {
                        effectsToRemove.Add(effect);
                    }
                }

                foreach (var effect in effectsToRemove)
                {
                    RemoveEffect(effect);
                }
            }

            public string GetStatus()
            {
                string status = $"=== {targetName} Status ===\n";

                // Attributes
                status += "Attributes:\n";
                foreach (var attr in attributes)
                {
                    status += $"  {attr.Key}: {attr.Value:F1}\n";
                }

                // Tags
                status += $"Tags: {string.Join(", ", tags)}\n";

                // Active Effects
                status += $"Active Effects ({activeEffects.Count}):\n";
                foreach (var effect in activeEffects)
                {
                    status += effect.GetDebugInfo();
                }

                return status;
            }
        }

        [Header("테스트 대상")]
        [SerializeField] private EffectTarget playerTarget;
        [SerializeField] private EffectTarget enemyTarget;

        [Header("미리 정의된 효과")]
        [SerializeField] private List<SimpleGameplayEffect> predefinedEffects = new List<SimpleGameplayEffect>();

        private void Start()
        {
            InitializeTargets();
            CreatePredefinedEffects();

            Debug.Log("=== Phase 3 - Step 1: Basic Effect 테스트 ===");
            RunBasicTests();
        }

        private void InitializeTargets()
        {
            playerTarget = new EffectTarget("Player");
            enemyTarget = new EffectTarget("Enemy");
        }

        private void CreatePredefinedEffects()
        {
            // 1. 즉시 데미지
            var instantDamage = new SimpleGameplayEffect("즉시 데미지", SimpleGameplayEffect.DurationType.Instant);
            instantDamage.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("Health",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Add, -20f));
            instantDamage.grantedTags.Add("Combat.Damaged");
            predefinedEffects.Add(instantDamage);

            // 2. 즉시 회복
            var instantHeal = new SimpleGameplayEffect("즉시 회복", SimpleGameplayEffect.DurationType.Instant);
            instantHeal.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("Health",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Add, 30f));
            instantHeal.grantedTags.Add("Combat.Healed");
            predefinedEffects.Add(instantHeal);

            // 3. 공격력 버프 (10초)
            var attackBuff = new SimpleGameplayEffect("공격력 버프", SimpleGameplayEffect.DurationType.Duration, 10f);
            attackBuff.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("AttackPower",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Add, 5f));
            attackBuff.grantedTags.Add("Status.Buff.Attack");
            predefinedEffects.Add(attackBuff);

            // 4. 방어력 버프 (15초)
            var defenseBuff = new SimpleGameplayEffect("방어력 버프", SimpleGameplayEffect.DurationType.Duration, 15f);
            defenseBuff.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("Defense",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Multiply, 1.5f));
            defenseBuff.grantedTags.Add("Status.Buff.Defense");
            predefinedEffects.Add(defenseBuff);

            // 5. 슬로우 디버프 (5초)
            var slowDebuff = new SimpleGameplayEffect("슬로우", SimpleGameplayEffect.DurationType.Duration, 5f);
            slowDebuff.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("MoveSpeed",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Multiply, 0.5f));
            slowDebuff.grantedTags.Add("Status.Debuff.Slow");
            predefinedEffects.Add(slowDebuff);

            // 6. 영구 패시브
            var passiveBuff = new SimpleGameplayEffect("영구 패시브", SimpleGameplayEffect.DurationType.Infinite);
            passiveBuff.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("MaxHealth",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Add, 20f));
            passiveBuff.grantedTags.Add("Status.Passive.HealthBoost");
            predefinedEffects.Add(passiveBuff);
        }

        private void RunBasicTests()
        {
            TestInstantEffects();
            TestDurationEffects();
            TestInfiniteEffects();
            TestEffectStacking();
        }

        // 테스트 1: 즉시 효과
        private void TestInstantEffects()
        {
            Debug.Log("\n========== 테스트 1: Instant Effects ==========");

            Debug.Log("초기 상태:");
            Debug.Log(playerTarget.GetStatus());

            // 즉시 데미지
            var damage = predefinedEffects[0];
            playerTarget.ApplyEffect(damage);

            Debug.Log("\n즉시 데미지 후:");
            Debug.Log($"Health: {playerTarget.attributes["Health"]}");
            Debug.Log($"Tags: {string.Join(", ", playerTarget.tags)}");

            // 즉시 회복
            var heal = predefinedEffects[1];
            playerTarget.ApplyEffect(heal);

            Debug.Log("\n즉시 회복 후:");
            Debug.Log($"Health: {playerTarget.attributes["Health"]}");
        }

        // 테스트 2: Duration 효과
        private void TestDurationEffects()
        {
            Debug.Log("\n========== 테스트 2: Duration Effects ==========");

            // 공격력 버프 적용
            var attackBuff = new SimpleGameplayEffect("공격력 버프", SimpleGameplayEffect.DurationType.Duration, 10f);
            attackBuff.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("AttackPower",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Add, 5f));
            attackBuff.grantedTags.Add("Status.Buff.Attack");

            Debug.Log($"버프 전 공격력: {playerTarget.attributes["AttackPower"]}");
            playerTarget.ApplyEffect(attackBuff);
            Debug.Log($"버프 후 공격력: {playerTarget.attributes["AttackPower"]}");
            Debug.Log($"활성 효과 수: {playerTarget.activeEffects.Count}");
        }

        // 테스트 3: Infinite 효과
        private void TestInfiniteEffects()
        {
            Debug.Log("\n========== 테스트 3: Infinite Effects ==========");

            var passive = predefinedEffects[5];

            Debug.Log($"패시브 전 MaxHealth: {enemyTarget.attributes["MaxHealth"]}");
            enemyTarget.ApplyEffect(passive);
            Debug.Log($"패시브 후 MaxHealth: {enemyTarget.attributes["MaxHealth"]}");
            Debug.Log($"영구 효과이므로 제거 전까지 유지됨");
        }

        // 테스트 4: 효과 스택
        private void TestEffectStacking()
        {
            Debug.Log("\n========== 테스트 4: Effect Stacking ==========");

            // 스택 가능한 효과 생성
            var stackableBuff = new SimpleGameplayEffect("스택형 버프", SimpleGameplayEffect.DurationType.Duration, 20f);
            stackableBuff.isStackable = true;
            stackableBuff.maxStacks = 3;
            stackableBuff.modifiers.Add(new SimpleGameplayEffect.AttributeModifier("AttackPower",
                SimpleGameplayEffect.AttributeModifier.ModifierOperation.Add, 2f));

            Debug.Log("스택형 버프 3회 적용:");
            for (int i = 0; i < 4; i++)
            {
                playerTarget.ApplyEffect(stackableBuff);
                Debug.Log($"  적용 {i + 1}회: 스택 {stackableBuff.currentStacks}/{stackableBuff.maxStacks}");
            }
        }

        private void Update()
        {
            // Duration 효과 업데이트
            if (playerTarget != null)
            {
                playerTarget.UpdateEffects(Time.deltaTime);
            }
            if (enemyTarget != null)
            {
                enemyTarget.UpdateEffects(Time.deltaTime);
            }

            HandleInput();
        }

        private void HandleInput()
        {
            // 효과 적용 테스트
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ApplyEffectToPlayer(0); // 즉시 데미지
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ApplyEffectToPlayer(1); // 즉시 회복
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ApplyEffectToPlayer(2); // 공격력 버프
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                ApplyEffectToPlayer(3); // 방어력 버프
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                ApplyEffectToPlayer(4); // 슬로우
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetTarget(playerTarget);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void ApplyEffectToPlayer(int index)
        {
            if (index < 0 || index >= predefinedEffects.Count) return;

            // 새 인스턴스 생성 (중요!)
            var effectTemplate = predefinedEffects[index];
            var newEffect = new SimpleGameplayEffect(effectTemplate.effectName, effectTemplate.durationType, effectTemplate.duration);
            newEffect.modifiers.AddRange(effectTemplate.modifiers);
            newEffect.grantedTags.AddRange(effectTemplate.grantedTags);

            playerTarget.ApplyEffect(newEffect);
        }

        private void ResetTarget(EffectTarget target)
        {
            target.activeEffects.Clear();
            target.tags.Clear();
            target.attributes["Health"] = 100f;
            target.attributes["AttackPower"] = 10f;
            target.attributes["Defense"] = 5f;
            target.attributes["MoveSpeed"] = 5f;
            Debug.Log($"{target.targetName} 초기화 완료!");
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== 현재 상태 ===");
            Debug.Log(playerTarget.GetStatus());
            Debug.Log(enemyTarget.GetStatus());
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 350, 280), "Phase 3 - Basic Effects");

            int y = 40;
            GUI.Label(new Rect(20, y, 330, 20), "=== Player Status ===");
            y += 25;

            // Attributes
            GUI.Label(new Rect(20, y, 330, 20), $"HP: {playerTarget?.attributes["Health"]:F0}/{playerTarget?.attributes["MaxHealth"]:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 330, 20), $"MP: {playerTarget?.attributes["Mana"]:F0}/{playerTarget?.attributes["MaxMana"]:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 330, 20), $"ATK: {playerTarget?.attributes["AttackPower"]:F0} | DEF: {playerTarget?.attributes["Defense"]:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 330, 20), $"Speed: {playerTarget?.attributes["MoveSpeed"]:F1}");
            y += 25;

            // Effects
            GUI.Label(new Rect(20, y, 330, 20), $"Active Effects: {playerTarget?.activeEffects.Count ?? 0}");
            y += 20;
            if (playerTarget != null && playerTarget.activeEffects.Count > 0)
            {
                foreach (var effect in playerTarget.activeEffects)
                {
                    string timeInfo = effect.durationType == SimpleGameplayEffect.DurationType.Duration
                        ? $" ({effect.remainingTime:F1}s)"
                        : effect.durationType == SimpleGameplayEffect.DurationType.Infinite
                        ? " (∞)"
                        : "";
                    GUI.Label(new Rect(30, y, 320, 20), $" {effect.effectName}{timeInfo}");
                    y += 20;
                }
            }

            // Tags
            y += 5;
            GUI.Label(new Rect(20, y, 330, 20), $"Tags: {string.Join(", ", playerTarget?.tags ?? new List<string>())}");

            // 조작법
            y = 230;
            GUI.Box(new Rect(10, y, 350, 80), "조작법");
            GUI.Label(new Rect(20, y + 25, 330, 50),
                "1:데미지 2:회복 3:공격버프 4:방어버프 5:슬로우\n" +
                "R: 초기화  Space: 상태 출력");
        }
    }
}