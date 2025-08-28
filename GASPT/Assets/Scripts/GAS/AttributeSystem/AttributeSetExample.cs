// 파일 위치: Assets/Scripts/GAS/Learning/Phase1/AttributeSetExample.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAS.Learning.Phase1
{
    /// <summary>
    /// Step 3: AttributeSet 관리 시스템
    /// </summary>
    public class AttributeSetExample : MonoBehaviour
    {
        // Attribute 타입 enum
        public enum AttributeType
        {
            // 기본 속성
            Health,
            MaxHealth,
            Mana,
            MaxMana,
            Stamina,
            MaxStamina,

            // 전투 속성
            AttackPower,
            Defense,
            CriticalChance,
            CriticalDamage,

            // 이동 속성
            MoveSpeed,
            JumpPower,

            // 저항 속성
            FireResistance,
            IceResistance,
            PoisonResistance
        }

        // AttributeSet 클래스 - 여러 Attribute를 관리
        [Serializable]
        public class AttributeSet
        {
            [SerializeField]
            private SerializableDictionary<AttributeType, ModifierTestExample.ModifiableAttribute> attributes;

            public event Action<AttributeType, float, float> OnAnyAttributeChanged;

            public AttributeSet()
            {
                attributes = new SerializableDictionary<AttributeType, ModifierTestExample.ModifiableAttribute>();
            }

            // Attribute 초기화
            public void InitializeAttribute(AttributeType type, float baseValue, float minValue = 0, float maxValue = 999)
            {
                var attribute = new ModifierTestExample.ModifiableAttribute(
                    type.ToString(),
                    baseValue,
                    minValue,
                    maxValue
                );

                attribute.OnValueChanged += (oldVal, newVal) =>
                {
                    OnAnyAttributeChanged?.Invoke(type, oldVal, newVal);
                };

                attributes[type] = attribute;
                Debug.Log($"[AttributeSet] {type} 초기화: {baseValue}");
            }

            // Attribute 가져오기
            public ModifierTestExample.ModifiableAttribute GetAttribute(AttributeType type)
            {
                if (attributes.TryGetValue(type, out var attribute))
                {
                    return attribute;
                }
                Debug.LogWarning($"[AttributeSet] {type} 속성을 찾을 수 없습니다!");
                return null;
            }

            // 값 가져오기
            public float GetAttributeValue(AttributeType type)
            {
                var attr = GetAttribute(type);
                return attr?.CurrentValue ?? 0f;
            }

            // 기본값 가져오기
            public float GetAttributeBaseValue(AttributeType type)
            {
                var attr = GetAttribute(type);
                return attr?.BaseValue ?? 0f;
            }

            // Modifier 적용
            public void ApplyModifier(AttributeType type, ModifierTestExample.AttributeModifier modifier)
            {
                var attr = GetAttribute(type);
                attr?.AddModifier(modifier);
            }

            // 여러 속성에 Modifier 적용
            public void ApplyModifiersToMultiple(AttributeType[] types, ModifierTestExample.AttributeModifier modifier)
            {
                foreach (var type in types)
                {
                    ApplyModifier(type, modifier);
                }
            }

            // 모든 속성 정보
            public string GetAllAttributesInfo()
            {
                var info = "=== AttributeSet Status ===\n";
                foreach (var kvp in attributes.OrderBy(x => x.Key.ToString()))
                {
                    info += $"{kvp.Key}: {kvp.Value.CurrentValue:F1} (Base: {kvp.Value.BaseValue:F1})\n";
                }
                return info;
            }

            // 전투 스탯 계산 (파생 속성)
            public float CalculateEffectiveDamage(float baseDamage)
            {
                float attackPower = GetAttributeValue(AttributeType.AttackPower);
                float critChance = GetAttributeValue(AttributeType.CriticalChance) / 100f;
                float critDamage = GetAttributeValue(AttributeType.CriticalDamage) / 100f;

                // 크리티컬 판정
                bool isCritical = UnityEngine.Random.value < critChance;
                float damage = baseDamage + attackPower;

                if (isCritical)
                {
                    damage *= (1f + critDamage);
                    Debug.Log($"크리티컬! 데미지: {damage:F1}");
                }

                return damage;
            }

            public float CalculateDamageReduction(float incomingDamage)
            {
                float defense = GetAttributeValue(AttributeType.Defense);
                // 간단한 방어력 공식: 방어력 / (방어력 + 100)
                float reduction = defense / (defense + 100f);
                return incomingDamage * (1f - reduction);
            }
        }

        // ScriptableObject로 AttributeSet 데이터 정의
        [CreateAssetMenu(fileName = "AttributeSetData", menuName = "GAS/Learning/AttributeSetData")]
        public class AttributeSetData : ScriptableObject
        {
            [Serializable]
            public class AttributeDefinition
            {
                public AttributeType type;
                public float baseValue;
                public float minValue;
                public float maxValue;
            }

            public List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>();
        }

        [Header("AttributeSet 테스트")]
        [SerializeField] private AttributeSet playerAttributes;
        [SerializeField] private AttributeSet enemyAttributes;

        [Header("테스트 설정")]
        [SerializeField] private float testDamageAmount = 20f;
        [SerializeField] private bool showDebugInfo = true;

        private List<ModifierTestExample.AttributeModifier> temporaryBuffs = new List<ModifierTestExample.AttributeModifier>();

        private void Start()
        {
            InitializeAttributeSets();
            SubscribeToEvents();

            Debug.Log("=== Phase 1 - Step 3: AttributeSet 테스트 시작 ===");
            RunAttributeSetTests();
        }

        private void InitializeAttributeSets()
        {
            // 플레이어 AttributeSet 초기화
            playerAttributes = new AttributeSet();

            // 기본 속성
            playerAttributes.InitializeAttribute(AttributeType.Health, 100f, 0f, 100f);
            playerAttributes.InitializeAttribute(AttributeType.MaxHealth, 100f, 50f, 200f);
            playerAttributes.InitializeAttribute(AttributeType.Mana, 50f, 0f, 50f);
            playerAttributes.InitializeAttribute(AttributeType.MaxMana, 50f, 20f, 100f);
            playerAttributes.InitializeAttribute(AttributeType.Stamina, 100f, 0f, 100f);
            playerAttributes.InitializeAttribute(AttributeType.MaxStamina, 100f, 50f, 150f);

            // 전투 속성
            playerAttributes.InitializeAttribute(AttributeType.AttackPower, 10f, 0f, 999f);
            playerAttributes.InitializeAttribute(AttributeType.Defense, 5f, 0f, 999f);
            playerAttributes.InitializeAttribute(AttributeType.CriticalChance, 10f, 0f, 100f);
            playerAttributes.InitializeAttribute(AttributeType.CriticalDamage, 50f, 0f, 500f);

            // 이동 속성
            playerAttributes.InitializeAttribute(AttributeType.MoveSpeed, 5f, 0f, 20f);
            playerAttributes.InitializeAttribute(AttributeType.JumpPower, 10f, 0f, 30f);

            // 저항 속성
            playerAttributes.InitializeAttribute(AttributeType.FireResistance, 0f, -100f, 100f);
            playerAttributes.InitializeAttribute(AttributeType.IceResistance, 0f, -100f, 100f);
            playerAttributes.InitializeAttribute(AttributeType.PoisonResistance, 0f, -100f, 100f);

            // 적 AttributeSet 초기화 (간단하게)
            enemyAttributes = new AttributeSet();
            enemyAttributes.InitializeAttribute(AttributeType.Health, 50f, 0f, 50f);
            enemyAttributes.InitializeAttribute(AttributeType.AttackPower, 5f, 0f, 999f);
            enemyAttributes.InitializeAttribute(AttributeType.Defense, 2f, 0f, 999f);
        }

        private void SubscribeToEvents()
        {
            playerAttributes.OnAnyAttributeChanged += (type, oldVal, newVal) =>
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[Player] {type}: {oldVal:F1} → {newVal:F1}");
                }

                // 특정 속성 변경에 대한 추가 처리
                switch (type)
                {
                    case AttributeType.Health:
                        if (newVal <= 0)
                        {
                            Debug.LogWarning("플레이어 사망!");
                        }
                        else if (newVal < 30f && oldVal >= 30f)
                        {
                            Debug.LogWarning("체력 위험!");
                        }
                        break;

                    case AttributeType.MaxHealth:
                        // MaxHealth 변경 시 현재 Health 비율 유지
                        var health = playerAttributes.GetAttribute(AttributeType.Health);
                        if (health != null && oldVal > 0)
                        {
                            float ratio = health.CurrentValue / oldVal;
                            health.SetCurrentValue(newVal * ratio);
                        }
                        break;
                }
            };
        }

        private void RunAttributeSetTests()
        {
            TestScenario1_BasicOperations();
            TestScenario2_BuffDebuff();
            TestScenario3_CombatCalculation();
            TestScenario4_AttributeRelationships();
        }

        // 시나리오 1: 기본 작업
        private void TestScenario1_BasicOperations()
        {
            Debug.Log("\n========== 시나리오 1: 기본 AttributeSet 작업 ==========");

            Debug.Log("초기 플레이어 속성:");
            Debug.Log(playerAttributes.GetAllAttributesInfo());

            // 데미지 받기
            var health = playerAttributes.GetAttribute(AttributeType.Health);
            health.AddToCurrentValue(-30f);
            Debug.Log($"30 데미지 후 체력: {health.CurrentValue}");

            // 마나 사용
            var mana = playerAttributes.GetAttribute(AttributeType.Mana);
            mana.AddToCurrentValue(-20f);
            Debug.Log($"20 마나 사용 후: {mana.CurrentValue}");

            // 회복
            health.AddToCurrentValue(20f);
            mana.AddToCurrentValue(15f);
            Debug.Log($"회복 후 - 체력: {health.CurrentValue}, 마나: {mana.CurrentValue}");
        }

        // 시나리오 2: 버프/디버프
        private void TestScenario2_BuffDebuff()
        {
            Debug.Log("\n========== 시나리오 2: 버프/디버프 시스템 ==========");

            // 공격력, 방어력 동시 버프
            var powerBuff = new ModifierTestExample.AttributeModifier(
                "파워 버프",
                ModifierTestExample.ModifierOperation.Add,
                5f
            );

            playerAttributes.ApplyModifiersToMultiple(
                new[] { AttributeType.AttackPower, AttributeType.Defense },
                powerBuff
            );

            Debug.Log($"파워 버프 후:");
            Debug.Log($"공격력: {playerAttributes.GetAttributeValue(AttributeType.AttackPower)}");
            Debug.Log($"방어력: {playerAttributes.GetAttributeValue(AttributeType.Defense)}");

            // 이동속도 디버프
            var slowDebuff = new ModifierTestExample.AttributeModifier(
                "슬로우",
                ModifierTestExample.ModifierOperation.Multiply,
                0.5f
            );

            playerAttributes.ApplyModifier(AttributeType.MoveSpeed, slowDebuff);
            Debug.Log($"슬로우 후 이동속도: {playerAttributes.GetAttributeValue(AttributeType.MoveSpeed)}");

            temporaryBuffs.Add(powerBuff);
            temporaryBuffs.Add(slowDebuff);
        }

        // 시나리오 3: 전투 계산
        private void TestScenario3_CombatCalculation()
        {
            Debug.Log("\n========== 시나리오 3: 전투 계산 ==========");

            // 플레이어 공격
            float playerDamage = playerAttributes.CalculateEffectiveDamage(10f);
            Debug.Log($"플레이어 기본 공격 데미지: {playerDamage:F1}");

            // 적 방어 계산
            float actualDamage = enemyAttributes.CalculateDamageReduction(playerDamage);
            Debug.Log($"적 방어력 적용 후 실제 데미지: {actualDamage:F1}");

            var enemyHealth = enemyAttributes.GetAttribute(AttributeType.Health);
            enemyHealth.AddToCurrentValue(-actualDamage);
            Debug.Log($"적 남은 체력: {enemyHealth.CurrentValue:F1}");

            // 크리티컬 테스트
            Debug.Log("\n크리티컬 확률 테스트 (5회):");
            for (int i = 0; i < 5; i++)
            {
                float damage = playerAttributes.CalculateEffectiveDamage(10f);
                Debug.Log($"  공격 {i + 1}: {damage:F1} 데미지");
            }
        }

        // 시나리오 4: 속성 간 관계
        private void TestScenario4_AttributeRelationships()
        {
            Debug.Log("\n========== 시나리오 4: 속성 간 관계 ==========");

            // MaxHealth 증가 (현재 체력 비율 유지)
            var maxHealthBuff = new ModifierTestExample.AttributeModifier(
                "체력 증가",
                ModifierTestExample.ModifierOperation.Add,
                50f
            );

            float healthBefore = playerAttributes.GetAttributeValue(AttributeType.Health);
            float maxHealthBefore = playerAttributes.GetAttributeValue(AttributeType.MaxHealth);

            playerAttributes.ApplyModifier(AttributeType.MaxHealth, maxHealthBuff);

            float healthAfter = playerAttributes.GetAttributeValue(AttributeType.Health);
            float maxHealthAfter = playerAttributes.GetAttributeValue(AttributeType.MaxHealth);

            Debug.Log($"MaxHealth 버프 전: HP {healthBefore}/{maxHealthBefore}");
            Debug.Log($"MaxHealth 버프 후: HP {healthAfter}/{maxHealthAfter}");

            // 저항 속성 테스트
            var fireResistBuff = new ModifierTestExample.AttributeModifier(
                "화염 저항",
                ModifierTestExample.ModifierOperation.Add,
                25f
            );

            playerAttributes.ApplyModifier(AttributeType.FireResistance, fireResistBuff);
            Debug.Log($"화염 저항: {playerAttributes.GetAttributeValue(AttributeType.FireResistance)}%");
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // 테스트 입력
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SimulateCombat();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ApplyRandomBuff();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ClearAllBuffs();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log(playerAttributes.GetAllAttributesInfo());
            }
        }

        private void SimulateCombat()
        {
            Debug.Log("\n=== 전투 시뮬레이션 ===");

            // 플레이어가 적을 공격
            float damage = playerAttributes.CalculateEffectiveDamage(testDamageAmount);
            float reducedDamage = enemyAttributes.CalculateDamageReduction(damage);

            var enemyHealth = enemyAttributes.GetAttribute(AttributeType.Health);
            enemyHealth.AddToCurrentValue(-reducedDamage);

            Debug.Log($"플레이어 공격: {damage:F1} → {reducedDamage:F1} (방어 적용)");
            Debug.Log($"적 체력: {enemyHealth.CurrentValue:F1}/{enemyHealth.BaseValue:F1}");

            // 적이 반격
            float enemyDamage = enemyAttributes.GetAttributeValue(AttributeType.AttackPower);
            float playerReducedDamage = playerAttributes.CalculateDamageReduction(enemyDamage);

            var playerHealth = playerAttributes.GetAttribute(AttributeType.Health);
            playerHealth.AddToCurrentValue(-playerReducedDamage);

            Debug.Log($"적 반격: {enemyDamage:F1} → {playerReducedDamage:F1} (방어 적용)");
            Debug.Log($"플레이어 체력: {playerHealth.CurrentValue:F1}/{playerHealth.BaseValue:F1}");
        }

        private void ApplyRandomBuff()
        {
            var buffTypes = new[]
            {
                (AttributeType.AttackPower, "공격력", 5f),
                (AttributeType.Defense, "방어력", 3f),
                (AttributeType.MoveSpeed, "이동속도", 2f),
                (AttributeType.CriticalChance, "크리티컬", 10f)
            };

            var randomBuff = buffTypes[UnityEngine.Random.Range(0, buffTypes.Length)];

            var modifier = new ModifierTestExample.AttributeModifier(
                $"{randomBuff.Item2} 버프",
                ModifierTestExample.ModifierOperation.Add,
                randomBuff.Item3
            );

            playerAttributes.ApplyModifier(randomBuff.Item1, modifier);
            temporaryBuffs.Add(modifier);

            Debug.Log($"랜덤 버프 적용: {randomBuff.Item2} +{randomBuff.Item3}");
        }

        private void ClearAllBuffs()
        {
            foreach (var buff in temporaryBuffs)
            {
                // 모든 속성에서 버프 제거 시도
                foreach (AttributeType type in Enum.GetValues(typeof(AttributeType)))
                {
                    var attr = playerAttributes.GetAttribute(type);
                    attr?.RemoveModifier(buff);
                }
            }
            temporaryBuffs.Clear();
            Debug.Log("모든 버프 제거됨");
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 400, 250), "Phase 1 - AttributeSet 테스트");

            int y = 40;

            // 플레이어 주요 속성
            GUI.Label(new Rect(20, y, 180, 20), "=== 플레이어 ===");
            y += 25;

            var health = playerAttributes?.GetAttribute(AttributeType.Health);
            var mana = playerAttributes?.GetAttribute(AttributeType.Mana);

            GUI.Label(new Rect(20, y, 180, 20), $"HP: {health?.CurrentValue:F0}/{health?.ModifiedValue:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 180, 20), $"MP: {mana?.CurrentValue:F0}/{mana?.ModifiedValue:F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 180, 20), $"공격력: {playerAttributes?.GetAttributeValue(AttributeType.AttackPower):F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 180, 20), $"방어력: {playerAttributes?.GetAttributeValue(AttributeType.Defense):F0}");
            y += 20;
            GUI.Label(new Rect(20, y, 180, 20), $"크리티컬: {playerAttributes?.GetAttributeValue(AttributeType.CriticalChance):F0}%");

            // 적 속성
            y = 65;
            GUI.Label(new Rect(210, y, 180, 20), "=== 적 ===");
            y += 25;

            var enemyHealthAttr = enemyAttributes?.GetAttribute(AttributeType.Health);
            GUI.Label(new Rect(210, y, 180, 20), $"HP: {enemyHealthAttr?.CurrentValue:F0}/{enemyHealthAttr?.BaseValue:F0}");
            y += 20;
            GUI.Label(new Rect(210, y, 180, 20), $"공격력: {enemyAttributes?.GetAttributeValue(AttributeType.AttackPower):F0}");
            y += 20;
            GUI.Label(new Rect(210, y, 180, 20), $"방어력: {enemyAttributes?.GetAttributeValue(AttributeType.Defense):F0}");

            // 조작 설명
            y = 180;
            GUI.Label(new Rect(20, y, 380, 60), "1: 전투 시뮬레이션  2: 랜덤 버프  3: 모든 버프 제거\nSpace: 전체 속성 출력");

            // 활성 버프 수
            y = 220;
            GUI.Label(new Rect(20, y, 200, 20), $"활성 버프: {temporaryBuffs?.Count ?? 0}개");
        }
    }
}