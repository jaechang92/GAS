// 파일 위치: Assets/Scripts/GAS/Learning/Phase3/Phase3TestController.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GAS.Learning.Phase1;
using GAS.Learning.Phase2;

namespace GAS.Learning.Phase3
{
    /// <summary>
    /// Phase 3 통합 테스트 컨트롤러
    /// Effect System + Attribute System + Tag System 통합
    /// </summary>
    public class Phase3TestController : MonoBehaviour
    {
        [Header("테스트 모드")]
        [SerializeField] private TestMode currentTestMode = TestMode.Integration;

        private enum TestMode
        {
            BasicEffect,     // Step 1 테스트
            Duration,        // Step 2 테스트
            Periodic,        // Step 3 테스트
            Integration      // 통합 테스트
        }

        // 통합 캐릭터 시스템
        [Serializable]
        public class IntegratedCharacter
        {
            public string characterName;

            // Phase 1: Attributes
            public Dictionary<string, float> attributes = new Dictionary<string, float>();
            public Dictionary<string, float> baseAttributes = new Dictionary<string, float>();

            // Phase 2: Tags
            public List<string> tags = new List<string>();

            // Phase 3: Effects
            public List<IntegratedEffect> activeEffects = new List<IntegratedEffect>();
            public List<EffectInstance> effectHistory = new List<EffectInstance>();

            // 상태
            public float health;
            public float maxHealth;
            public float mana;
            public float maxMana;
            public bool isDead;

            public IntegratedCharacter(string name)
            {
                characterName = name;
                InitializeAttributes();
            }

            private void InitializeAttributes()
            {
                // 기본 속성 설정
                baseAttributes["Health"] = 100f;
                baseAttributes["MaxHealth"] = 100f;
                baseAttributes["Mana"] = 50f;
                baseAttributes["MaxMana"] = 50f;
                baseAttributes["AttackPower"] = 10f;
                baseAttributes["Defense"] = 5f;
                baseAttributes["MoveSpeed"] = 5f;
                baseAttributes["CritChance"] = 10f;
                baseAttributes["CritDamage"] = 50f;

                // 현재 속성 초기화
                foreach (var kvp in baseAttributes)
                {
                    attributes[kvp.Key] = kvp.Value;
                }

                health = attributes["Health"];
                maxHealth = attributes["MaxHealth"];
                mana = attributes["Mana"];
                maxMana = attributes["MaxMana"];
            }

            public void ApplyEffect(IntegratedEffect effect)
            {
                if (!effect.CanApply(this))
                {
                    Debug.LogWarning($"[{characterName}] {effect.effectName} 적용 불가!");
                    return;
                }

                effect.Apply(this);

                if (effect.durationType != IntegratedEffect.DurationType.Instant)
                {
                    activeEffects.Add(effect);
                }

                // 히스토리 추가
                effectHistory.Add(new EffectInstance
                {
                    effectName = effect.effectName,
                    appliedTime = Time.time,
                    duration = effect.duration
                });

                Debug.Log($"[{characterName}] {effect.effectName} 적용!");
            }

            public void RemoveEffect(IntegratedEffect effect)
            {
                effect.Remove(this);
                activeEffects.Remove(effect);
                Debug.Log($"[{characterName}] {effect.effectName} 제거!");
            }

            public void UpdateEffects(float deltaTime)
            {
                var effectsToRemove = new List<IntegratedEffect>();

                foreach (var effect in activeEffects)
                {
                    if (effect.Update(deltaTime, this))
                    {
                        effectsToRemove.Add(effect);
                    }
                }

                foreach (var effect in effectsToRemove)
                {
                    RemoveEffect(effect);
                }

                // 체력/마나 업데이트
                health = Mathf.Clamp(attributes["Health"], 0, attributes["MaxHealth"]);
                maxHealth = attributes["MaxHealth"];
                mana = Mathf.Clamp(attributes["Mana"], 0, attributes["MaxMana"]);
                maxMana = attributes["MaxMana"];

                // 사망 체크
                if (health <= 0 && !isDead)
                {
                    isDead = true;
                    tags.Add("State.Dead");
                    Debug.Log($"[{characterName}] 사망!");
                }
            }

            public void TakeDamage(float damage)
            {
                float defense = attributes["Defense"];
                float actualDamage = Mathf.Max(1, damage - defense * 0.5f);
                attributes["Health"] -= actualDamage;
                Debug.Log($"[{characterName}] {actualDamage:F1} 데미지 받음 (방어: {defense})");
            }

            public void Heal(float amount)
            {
                attributes["Health"] = Mathf.Min(attributes["Health"] + amount, attributes["MaxHealth"]);
                Debug.Log($"[{characterName}] {amount:F1} 회복");
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

            public string GetStatus()
            {
                string status = $"=== {characterName} ===\n";
                status += $"HP: {health:F0}/{maxHealth:F0} | MP: {mana:F0}/{maxMana:F0}\n";
                status += $"ATK: {attributes["AttackPower"]:F0} | DEF: {attributes["Defense"]:F0} | SPD: {attributes["MoveSpeed"]:F1}\n";
                status += $"Tags: {string.Join(", ", tags)}\n";
                status += $"Active Effects: {activeEffects.Count}";
                return status;
            }
        }

        // 통합 Effect
        [Serializable]
        public class IntegratedEffect
        {
            public string effectName;
            public string description;

            public enum DurationType
            {
                Instant,
                Duration,
                Infinite
            }

            public DurationType durationType;
            public float duration;
            public float remainingTime;

            // Periodic
            public bool isPeriodic;
            public float tickInterval;
            public float nextTickTime;
            public int tickCount;

            // Modifiers
            public List<AttributeModifier> modifiers = new List<AttributeModifier>();

            // Tags
            public List<string> grantedTags = new List<string>();
            public List<string> requiredTags = new List<string>();
            public List<string> blockingTags = new List<string>();

            // Stack
            public bool isStackable;
            public int currentStacks;
            public int maxStacks;

            [Serializable]
            public class AttributeModifier
            {
                public string attributeName;
                public float value;
                public ModifierType type;

                public enum ModifierType
                {
                    Flat,
                    Percent,
                    Override
                }
            }

            public IntegratedEffect(string name, DurationType type, float dur = 0)
            {
                effectName = name;
                durationType = type;
                duration = dur;
                remainingTime = dur;
                currentStacks = 1;
                maxStacks = 1;
            }

            public bool CanApply(IntegratedCharacter target)
            {
                // 필수 태그 체크
                foreach (var tag in requiredTags)
                {
                    if (!target.HasTag(tag))
                    {
                        return false;
                    }
                }

                // 차단 태그 체크
                foreach (var tag in blockingTags)
                {
                    if (target.HasTag(tag))
                    {
                        return false;
                    }
                }

                return true;
            }

            public void Apply(IntegratedCharacter target)
            {
                // Attribute 수정
                foreach (var modifier in modifiers)
                {
                    ApplyModifier(target, modifier);
                }

                // Tag 부여
                foreach (var tag in grantedTags)
                {
                    target.AddTag(tag);
                }

                // Periodic 초기화
                if (isPeriodic)
                {
                    nextTickTime = tickInterval;
                    tickCount = 0;
                }
            }

            public void Remove(IntegratedCharacter target)
            {
                // Duration/Infinite 효과 제거
                if (durationType != DurationType.Instant)
                {
                    // Modifier 제거 (간단한 구현)
                    foreach (var modifier in modifiers)
                    {
                        RemoveModifier(target, modifier);
                    }

                    // Tag 제거
                    foreach (var tag in grantedTags)
                    {
                        target.RemoveTag(tag);
                    }
                }
            }

            public bool Update(float deltaTime, IntegratedCharacter target)
            {
                if (durationType == DurationType.Instant)
                    return true;

                if (durationType == DurationType.Infinite)
                    return false;

                remainingTime -= deltaTime;

                // Periodic 처리
                if (isPeriodic)
                {
                    nextTickTime -= deltaTime;
                    if (nextTickTime <= 0)
                    {
                        Tick(target);
                        nextTickTime = tickInterval;
                    }
                }

                return remainingTime <= 0;
            }

            private void Tick(IntegratedCharacter target)
            {
                tickCount++;
                Debug.Log($"[{effectName}] Tick #{tickCount}");

                // Periodic 효과 적용
                foreach (var modifier in modifiers)
                {
                    if (modifier.type == AttributeModifier.ModifierType.Flat)
                    {
                        ApplyModifier(target, modifier);
                    }
                }
            }

            private void ApplyModifier(IntegratedCharacter target, AttributeModifier modifier)
            {
                if (!target.attributes.ContainsKey(modifier.attributeName))
                    return;

                switch (modifier.type)
                {
                    case AttributeModifier.ModifierType.Flat:
                        target.attributes[modifier.attributeName] += modifier.value * currentStacks;
                        break;
                    case AttributeModifier.ModifierType.Percent:
                        target.attributes[modifier.attributeName] *= (1 + modifier.value * currentStacks);
                        break;
                    case AttributeModifier.ModifierType.Override:
                        target.attributes[modifier.attributeName] = modifier.value;
                        break;
                }
            }

            private void RemoveModifier(IntegratedCharacter target, AttributeModifier modifier)
            {
                // 간단한 구현 (실제로는 더 복잡)
                if (!target.attributes.ContainsKey(modifier.attributeName))
                    return;

                switch (modifier.type)
                {
                    case AttributeModifier.ModifierType.Flat:
                        target.attributes[modifier.attributeName] -= modifier.value * currentStacks;
                        break;
                    case AttributeModifier.ModifierType.Percent:
                        target.attributes[modifier.attributeName] /= (1 + modifier.value * currentStacks);
                        break;
                }
            }
        }

        // Effect Instance (히스토리용)
        [Serializable]
        public class EffectInstance
        {
            public string effectName;
            public float appliedTime;
            public float duration;
        }

        // 전투 시뮬레이션
        public class CombatSimulation
        {
            private IntegratedCharacter player;
            private IntegratedCharacter enemy;

            public CombatSimulation(IntegratedCharacter p, IntegratedCharacter e)
            {
                player = p;
                enemy = e;
            }

            public async Task RunCombat()
            {
                Debug.Log("\n=== 전투 시뮬레이션 시작 ===");

                // Phase 1: 전투 준비
                Debug.Log("\n[Phase 1] 전투 준비");
                player.AddTag("State.Combat");
                enemy.AddTag("State.Combat");

                // 버프 적용
                var attackBuff = CreateAttackBuff();
                player.ApplyEffect(attackBuff);

                await Task.Delay(1000);

                // Phase 2: 교전
                Debug.Log("\n[Phase 2] 교전 시작");

                for (int round = 1; round <= 3; round++)
                {
                    Debug.Log($"\n--- Round {round} ---");

                    // 플레이어 공격
                    float playerDamage = player.attributes["AttackPower"];
                    enemy.TakeDamage(playerDamage);

                    // DoT 적용
                    if (round == 1)
                    {
                        var poisonEffect = CreatePoisonEffect();
                        enemy.ApplyEffect(poisonEffect);
                    }

                    await Task.Delay(1000);

                    // 적 반격
                    if (!enemy.isDead)
                    {
                        float enemyDamage = enemy.attributes["AttackPower"];
                        player.TakeDamage(enemyDamage);

                        // 디버프 적용
                        if (round == 2)
                        {
                            var slowEffect = CreateSlowEffect();
                            player.ApplyEffect(slowEffect);
                        }
                    }

                    await Task.Delay(1000);

                    if (player.isDead || enemy.isDead)
                        break;
                }

                // Phase 3: 전투 종료
                Debug.Log("\n[Phase 3] 전투 종료");
                Debug.Log(player.GetStatus());
                Debug.Log(enemy.GetStatus());
            }

            private IntegratedEffect CreateAttackBuff()
            {
                var buff = new IntegratedEffect("공격력 증가", IntegratedEffect.DurationType.Duration, 10f);
                buff.modifiers.Add(new IntegratedEffect.AttributeModifier
                {
                    attributeName = "AttackPower",
                    value = 5,
                    type = IntegratedEffect.AttributeModifier.ModifierType.Flat
                });
                buff.grantedTags.Add("Status.Buff.Attack");
                return buff;
            }

            private IntegratedEffect CreatePoisonEffect()
            {
                var poison = new IntegratedEffect("독", IntegratedEffect.DurationType.Duration, 5f);
                poison.isPeriodic = true;
                poison.tickInterval = 1f;
                poison.modifiers.Add(new IntegratedEffect.AttributeModifier
                {
                    attributeName = "Health",
                    value = -3,
                    type = IntegratedEffect.AttributeModifier.ModifierType.Flat
                });
                poison.grantedTags.Add("Status.Debuff.Poison");
                return poison;
            }

            private IntegratedEffect CreateSlowEffect()
            {
                var slow = new IntegratedEffect("슬로우", IntegratedEffect.DurationType.Duration, 3f);
                slow.modifiers.Add(new IntegratedEffect.AttributeModifier
                {
                    attributeName = "MoveSpeed",
                    value = -0.5f,
                    type = IntegratedEffect.AttributeModifier.ModifierType.Percent
                });
                slow.grantedTags.Add("Status.Debuff.Slow");
                return slow;
            }
        }

        [Header("통합 시스템")]
        [SerializeField] private IntegratedCharacter player;
        [SerializeField] private IntegratedCharacter enemy;
        [SerializeField] private CombatSimulation combatSim;

        [Header("미리 정의된 효과")]
        [SerializeField] private List<IntegratedEffect> predefinedEffects = new List<IntegratedEffect>();

        private async void Start()
        {
            InitializeSystems();
            CreatePredefinedEffects();

            await Task.Delay(1000);

            Debug.Log("=== Phase 3 통합 테스트 시작 ===");
            await RunIntegrationTest();
        }

        private void InitializeSystems()
        {
            player = new IntegratedCharacter("Player");
            enemy = new IntegratedCharacter("Enemy");
            combatSim = new CombatSimulation(player, enemy);
        }

        private void CreatePredefinedEffects()
        {
            // 즉시 데미지
            var damage = new IntegratedEffect("파이어볼", IntegratedEffect.DurationType.Instant);
            damage.modifiers.Add(new IntegratedEffect.AttributeModifier
            {
                attributeName = "Health",
                value = -20,
                type = IntegratedEffect.AttributeModifier.ModifierType.Flat
            });
            predefinedEffects.Add(damage);

            // 즉시 회복
            var heal = new IntegratedEffect("힐", IntegratedEffect.DurationType.Instant);
            heal.modifiers.Add(new IntegratedEffect.AttributeModifier
            {
                attributeName = "Health",
                value = 15,
                type = IntegratedEffect.AttributeModifier.ModifierType.Flat
            });
            predefinedEffects.Add(heal);

            // 버프
            var buff = new IntegratedEffect("파워 버프", IntegratedEffect.DurationType.Duration, 10f);
            buff.modifiers.Add(new IntegratedEffect.AttributeModifier
            {
                attributeName = "AttackPower",
                value = 0.5f,
                type = IntegratedEffect.AttributeModifier.ModifierType.Percent
            });
            buff.grantedTags.Add("Status.Buff.Power");
            predefinedEffects.Add(buff);

            // DoT
            var dot = new IntegratedEffect("번", IntegratedEffect.DurationType.Duration, 6f);
            dot.isPeriodic = true;
            dot.tickInterval = 0.5f;
            dot.modifiers.Add(new IntegratedEffect.AttributeModifier
            {
                attributeName = "Health",
                value = -2,
                type = IntegratedEffect.AttributeModifier.ModifierType.Flat
            });
            dot.grantedTags.Add("Status.Debuff.Burn");
            predefinedEffects.Add(dot);

            // 스택형 효과
            var stackable = new IntegratedEffect("중첩 독", IntegratedEffect.DurationType.Duration, 8f);
            stackable.isStackable = true;
            stackable.maxStacks = 5;
            stackable.isPeriodic = true;
            stackable.tickInterval = 1f;
            stackable.modifiers.Add(new IntegratedEffect.AttributeModifier
            {
                attributeName = "Health",
                value = -1,
                type = IntegratedEffect.AttributeModifier.ModifierType.Flat
            });
            predefinedEffects.Add(stackable);
        }

        private async Task RunIntegrationTest()
        {
            switch (currentTestMode)
            {
                case TestMode.BasicEffect:
                    await TestBasicEffects();
                    break;
                case TestMode.Duration:
                    await TestDurationEffects();
                    break;
                case TestMode.Periodic:
                    await TestPeriodicEffects();
                    break;
                case TestMode.Integration:
                    await TestFullIntegration();
                    break;
            }
        }

        private async Task TestBasicEffects()
        {
            Debug.Log("\n=== 기본 효과 테스트 ===");

            player.ApplyEffect(predefinedEffects[0]); // 데미지
            await Task.Delay(1000);

            player.ApplyEffect(predefinedEffects[1]); // 힐
            await Task.Delay(1000);
        }

        private async Task TestDurationEffects()
        {
            Debug.Log("\n=== Duration 효과 테스트 ===");

            player.ApplyEffect(predefinedEffects[2]); // 버프
            await Task.Delay(2000);

            enemy.ApplyEffect(predefinedEffects[3]); // DoT
            await Task.Delay(2000);
        }

        private async Task TestPeriodicEffects()
        {
            Debug.Log("\n=== 주기적 효과 테스트 ===");

            enemy.ApplyEffect(predefinedEffects[3]); // 번
            enemy.ApplyEffect(predefinedEffects[4]); // 중첩 독

            await Task.Delay(3000);
        }

        private async Task TestFullIntegration()
        {
            Debug.Log("\n=== 전체 통합 테스트 ===");
            await combatSim.RunCombat();
        }

        private void Update()
        {
            // 효과 업데이트
            player?.UpdateEffects(Time.deltaTime);
            enemy?.UpdateEffects(Time.deltaTime);

            HandleInput();
        }

        private void HandleInput()
        {
            // 효과 적용
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ApplyEffectToPlayer(0); // 데미지
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ApplyEffectToPlayer(1); // 힐
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ApplyEffectToPlayer(2); // 버프
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                ApplyEffectToEnemy(3); // DoT
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                ApplyStackableEffect(); // 스택형
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                StartCombat();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetAll();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void ApplyEffectToPlayer(int index)
        {
            if (index < 0 || index >= predefinedEffects.Count) return;

            // 새 인스턴스 생성
            var template = predefinedEffects[index];
            var newEffect = new IntegratedEffect(template.effectName, template.durationType, template.duration);
            newEffect.modifiers.AddRange(template.modifiers);
            newEffect.grantedTags.AddRange(template.grantedTags);
            newEffect.isPeriodic = template.isPeriodic;
            newEffect.tickInterval = template.tickInterval;

            player.ApplyEffect(newEffect);
        }

        private void ApplyEffectToEnemy(int index)
        {
            if (index < 0 || index >= predefinedEffects.Count) return;

            var template = predefinedEffects[index];
            var newEffect = new IntegratedEffect(template.effectName, template.durationType, template.duration);
            newEffect.modifiers.AddRange(template.modifiers);
            newEffect.grantedTags.AddRange(template.grantedTags);
            newEffect.isPeriodic = template.isPeriodic;
            newEffect.tickInterval = template.tickInterval;

            enemy.ApplyEffect(newEffect);
        }

        private void ApplyStackableEffect()
        {
            var existing = player.activeEffects.Find(e => e.effectName == "중첩 독");
            if (existing != null && existing.currentStacks < existing.maxStacks)
            {
                existing.currentStacks++;
                Debug.Log($"중첩 독 스택: {existing.currentStacks}/{existing.maxStacks}");
            }
            else
            {
                ApplyEffectToPlayer(4);
            }
        }

        private async void StartCombat()
        {
            await combatSim.RunCombat();
        }

        private void ResetAll()
        {
            player = new IntegratedCharacter("Player");
            enemy = new IntegratedCharacter("Enemy");
            combatSim = new CombatSimulation(player, enemy);
            Debug.Log("시스템 초기화 완료!");
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== 현재 상태 ===");
            Debug.Log(player.GetStatus());
            Debug.Log(enemy.GetStatus());
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 500, 400), "Phase 3 - 통합 테스트");

            // 테스트 모드
            GUI.Label(new Rect(20, 40, 200, 20), $"테스트 모드: {currentTestMode}");

            int y = 70;

            // Player 정보
            GUI.Label(new Rect(20, y, 230, 20), "=== Player ===");
            y += 25;
            GUI.Label(new Rect(20, y, 230, 20), $"HP: {player?.health:F0}/{player?.maxHealth:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 230, 20), $"MP: {player?.mana:F0}/{player?.maxMana:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 230, 20), $"ATK: {player?.attributes["AttackPower"]:F0} | DEF: {player?.attributes["Defense"]:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 230, 20), $"SPD: {player?.attributes["MoveSpeed"]:F1}");
            y += 20;
            GUI.Label(new Rect(20, y, 230, 20), $"Effects: {player?.activeEffects.Count ?? 0}");
            y += 20;

            // Player Effects
            if (player != null && player.activeEffects.Count > 0)
            {
                foreach (var effect in player.activeEffects)
                {
                    string info = effect.durationType == IntegratedEffect.DurationType.Duration
                        ? $" ({effect.remainingTime:F1}s)"
                        : effect.durationType == IntegratedEffect.DurationType.Infinite
                        ? " (∞)"
                        : "";
                    GUI.Label(new Rect(30, y, 220, 20), $" {effect.effectName}{info}");
                    y += 20;
                }
            }

            // Enemy 정보
            y = 95;
            GUI.Label(new Rect(260, y, 230, 20), "=== Enemy ===");
            y += 25;
            GUI.Label(new Rect(260, y, 230, 20), $"HP: {enemy?.health:F0}/{enemy?.maxHealth:F0}");
            y += 20;
            GUI.Label(new Rect(260, y, 230, 20), $"MP: {enemy?.mana:F0}/{enemy?.maxMana:F0}");
            y += 20;
            GUI.Label(new Rect(260, y, 230, 20), $"ATK: {enemy?.attributes["AttackPower"]:F0} | DEF: {enemy?.attributes["Defense"]:F0}");
            y += 20;
            GUI.Label(new Rect(260, y, 230, 20), $"Effects: {enemy?.activeEffects.Count ?? 0}");
            y += 20;

            // Enemy Effects
            if (enemy != null && enemy.activeEffects.Count > 0)
            {
                foreach (var effect in enemy.activeEffects)
                {
                    string info = effect.durationType == IntegratedEffect.DurationType.Duration
                        ? $" ({effect.remainingTime:F1}s)"
                        : "";
                    GUI.Label(new Rect(270, y, 220, 20), $" {effect.effectName}{info}");
                    y += 20;
                }
            }

            // Tags
            y = 250;
            GUI.Label(new Rect(20, y, 480, 20), $"Player Tags: {string.Join(", ", player?.tags ?? new List<string>())}");
            y += 20;
            GUI.Label(new Rect(20, y, 480, 20), $"Enemy Tags: {string.Join(", ", enemy?.tags ?? new List<string>())}");

            // 조작법
            y = 300;
            GUI.Box(new Rect(10, y, 500, 100), "조작법");
            y += 25;
            GUI.Label(new Rect(20, y, 480, 70),
                "효과: 1-데미지 2-힐 3-버프 4-DoT 5-스택형\n" +
                "Q: 전투 시뮬레이션  R: 초기화  Space: 상태 출력\n\n" +
                "Phase 1 (Attributes) + Phase 2 (Tags) + Phase 3 (Effects) 통합!");
        }
    }
}