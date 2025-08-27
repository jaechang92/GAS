// 파일 위치: Assets/Scripts/GAS/Learning/Phase1/Phase1TestController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GAS.Learning.Phase1
{
    /// <summary>
    /// Phase 1 통합 테스트 컨트롤러
    /// 학습한 모든 내용을 실제로 적용하는 예제
    /// </summary>
    public class Phase1TestController : MonoBehaviour
    {
        [Header("테스트 모드")]
        [SerializeField] private TestMode currentTestMode = TestMode.Integration;

        private enum TestMode
        {
            BasicAttribute,    // Step 1 테스트
            Modifier,         // Step 2 테스트
            AttributeSet,     // Step 3 테스트
            Integration       // 통합 테스트
        }

        [Header("통합 시스템")]
        [SerializeField] private CharacterAttributeSystem playerSystem;
        [SerializeField] private CharacterAttributeSystem enemySystem;

        [Header("테스트 UI")]
        [SerializeField] private bool showDetailedInfo = false;
        [SerializeField] private float uiUpdateInterval = 0.1f;
        private float nextUiUpdate;

        // 통합 Character Attribute System
        [System.Serializable]
        public class CharacterAttributeSystem
        {
            public string characterName;
            public AttributeSetExample.AttributeSet attributes;
            public List<ActiveEffect> activeEffects = new List<ActiveEffect>();

            [System.Serializable]
            public class ActiveEffect
            {
                public string name;
                public List<ModifierTestExample.AttributeModifier> modifiers;
                public float duration;
                public float remainingTime;
                public bool isPermanent;

                public ActiveEffect(string name, float duration = 0)
                {
                    this.name = name;
                    this.duration = duration;
                    this.remainingTime = duration;
                    this.isPermanent = duration <= 0;
                    this.modifiers = new List<ModifierTestExample.AttributeModifier>();
                }
            }

            public void Initialize(string name)
            {
                characterName = name;
                attributes = new AttributeSetExample.AttributeSet();

                // 기본 속성 설정
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.Health, 100f, 0f, 100f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.MaxHealth, 100f, 50f, 200f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.Mana, 50f, 0f, 50f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.MaxMana, 50f, 20f, 100f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.AttackPower, 10f, 0f, 999f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.Defense, 5f, 0f, 999f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.MoveSpeed, 5f, 0f, 20f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.CriticalChance, 10f, 0f, 100f);
                attributes.InitializeAttribute(AttributeSetExample.AttributeType.CriticalDamage, 50f, 0f, 500f);
            }

            public void ApplyEffect(string effectName, float duration, params (AttributeSetExample.AttributeType, ModifierTestExample.ModifierOperation, float)[] modifiers)
            {
                var effect = new ActiveEffect(effectName, duration);

                foreach (var (type, operation, value) in modifiers)
                {
                    var modifier = new ModifierTestExample.AttributeModifier(
                        effectName,
                        operation,
                        value
                    );

                    effect.modifiers.Add(modifier);
                    attributes.ApplyModifier(type, modifier);
                }

                activeEffects.Add(effect);
                Debug.Log($"[{characterName}] 효과 적용: {effectName} ({duration}초)");
            }

            public void RemoveEffect(ActiveEffect effect)
            {
                foreach (var modifier in effect.modifiers)
                {
                    // 모든 속성에서 modifier 제거 시도
                    foreach (AttributeSetExample.AttributeType type in System.Enum.GetValues(typeof(AttributeSetExample.AttributeType)))
                    {
                        var attr = attributes.GetAttribute(type);
                        attr?.RemoveModifier(modifier);
                    }
                }

                activeEffects.Remove(effect);
                Debug.Log($"[{characterName}] 효과 제거: {effect.name}");
            }

            public void UpdateEffects(float deltaTime)
            {
                var effectsToRemove = new List<ActiveEffect>();

                foreach (var effect in activeEffects)
                {
                    if (!effect.isPermanent)
                    {
                        effect.remainingTime -= deltaTime;
                        if (effect.remainingTime <= 0)
                        {
                            effectsToRemove.Add(effect);
                        }
                    }
                }

                foreach (var effect in effectsToRemove)
                {
                    RemoveEffect(effect);
                }
            }
        }

        private async void Start()
        {
            InitializeSystems();

            await Task.Delay(1000); // 1초 대기

            Debug.Log("=== Phase 1 통합 테스트 시작 ===");
            await RunIntegrationTest();
        }

        private void InitializeSystems()
        {
            playerSystem = new CharacterAttributeSystem();
            playerSystem.Initialize("Player");

            enemySystem = new CharacterAttributeSystem();
            enemySystem.Initialize("Enemy");

            // 이벤트 구독
            playerSystem.attributes.OnAnyAttributeChanged += (type, oldVal, newVal) =>
            {
                if (type == AttributeSetExample.AttributeType.Health && newVal <= 0)
                {
                    Debug.LogError($"[{playerSystem.characterName}] 사망!");
                }
            };

            enemySystem.attributes.OnAnyAttributeChanged += (type, oldVal, newVal) =>
            {
                if (type == AttributeSetExample.AttributeType.Health && newVal <= 0)
                {
                    Debug.LogError($"[{enemySystem.characterName}] 처치!");
                }
            };
        }

        private async Task RunIntegrationTest()
        {
            switch (currentTestMode)
            {
                case TestMode.BasicAttribute:
                    await TestBasicAttributeOperations();
                    break;
                case TestMode.Modifier:
                    await TestModifierSystem();
                    break;
                case TestMode.AttributeSet:
                    await TestAttributeSetOperations();
                    break;
                case TestMode.Integration:
                    await TestFullIntegration();
                    break;
            }
        }

        // 테스트 1: 기본 Attribute 작업
        private async Task TestBasicAttributeOperations()
        {
            Debug.Log("\n=== 기본 Attribute 테스트 ===");

            var health = playerSystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);

            Debug.Log($"초기 체력: {health.CurrentValue}");

            // 데미지 테스트
            health.AddToCurrentValue(-30);
            Debug.Log($"30 데미지 후: {health.CurrentValue}");
            await Task.Delay(1000);

            // 힐 테스트
            health.AddToCurrentValue(20);
            Debug.Log($"20 힐 후: {health.CurrentValue}");
            await Task.Delay(1000);

            // 오버힐 테스트
            health.AddToCurrentValue(200);
            Debug.Log($"오버힐 테스트: {health.CurrentValue} (최대값으로 제한됨)");
        }

        // 테스트 2: Modifier 시스템
        private async Task TestModifierSystem()
        {
            Debug.Log("\n=== Modifier 시스템 테스트 ===");

            // 버프 적용
            playerSystem.ApplyEffect(
                "공격력 증가",
                5f,
                (AttributeSetExample.AttributeType.AttackPower, ModifierTestExample.ModifierOperation.Add, 5f)
            );

            Debug.Log($"버프 후 공격력: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.AttackPower)}");
            await Task.Delay(2000);

            // 디버프 적용
            playerSystem.ApplyEffect(
                "슬로우",
                3f,
                (AttributeSetExample.AttributeType.MoveSpeed, ModifierTestExample.ModifierOperation.Multiply, 0.5f)
            );

            Debug.Log($"슬로우 후 이동속도: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.MoveSpeed)}");
            await Task.Delay(3000);

            Debug.Log("버프/디버프 만료 후:");
            Debug.Log($"공격력: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.AttackPower)}");
            Debug.Log($"이동속도: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.MoveSpeed)}");
        }

        // 테스트 3: AttributeSet 작업
        private async Task TestAttributeSetOperations()
        {
            Debug.Log("\n=== AttributeSet 테스트 ===");

            // 여러 속성 동시 버프
            playerSystem.ApplyEffect(
                "전투 강화",
                10f,
                (AttributeSetExample.AttributeType.AttackPower, ModifierTestExample.ModifierOperation.Add, 10f),
                (AttributeSetExample.AttributeType.Defense, ModifierTestExample.ModifierOperation.Add, 5f),
                (AttributeSetExample.AttributeType.CriticalChance, ModifierTestExample.ModifierOperation.Add, 20f)
            );

            Debug.Log("전투 강화 버프 적용:");
            Debug.Log($"공격력: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.AttackPower)}");
            Debug.Log($"방어력: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.Defense)}");
            Debug.Log($"크리티컬: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.CriticalChance)}%");

            await Task.Delay(5000);
        }

        // 테스트 4: 전체 통합 테스트
        private async Task TestFullIntegration()
        {
            Debug.Log("\n=== 전체 통합 테스트 ===");

            // 1단계: 전투 준비
            Debug.Log("\n[1단계] 전투 준비");

            // 플레이어 버프
            playerSystem.ApplyEffect(
                "전사의 축복",
                15f,
                (AttributeSetExample.AttributeType.AttackPower, ModifierTestExample.ModifierOperation.Multiply, 1.5f),
                (AttributeSetExample.AttributeType.Defense, ModifierTestExample.ModifierOperation.Add, 10f)
            );

            await Task.Delay(2000);

            // 2단계: 전투 시뮬레이션
            Debug.Log("\n[2단계] 전투 시작");

            for (int round = 1; round <= 3; round++)
            {
                Debug.Log($"\n=== 라운드 {round} ===");

                // 플레이어 공격
                float playerDamage = playerSystem.attributes.CalculateEffectiveDamage(10f);
                float actualDamage = enemySystem.attributes.CalculateDamageReduction(playerDamage);

                var enemyHealth = enemySystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);
                enemyHealth.AddToCurrentValue(-actualDamage);

                Debug.Log($"플레이어 공격: {actualDamage:F1} 데미지");
                Debug.Log($"적 체력: {enemyHealth.CurrentValue:F1}/{enemyHealth.BaseValue:F1}");

                if (enemyHealth.CurrentValue <= 0) break;

                await Task.Delay(1500);

                // 적 반격
                float enemyDamage = enemySystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.AttackPower);
                float playerReducedDamage = playerSystem.attributes.CalculateDamageReduction(enemyDamage);

                var playerHealth = playerSystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);
                playerHealth.AddToCurrentValue(-playerReducedDamage);

                Debug.Log($"적 반격: {playerReducedDamage:F1} 데미지");
                Debug.Log($"플레이어 체력: {playerHealth.CurrentValue:F1}/{playerHealth.BaseValue:F1}");

                if (playerHealth.CurrentValue <= 0) break;

                await Task.Delay(1500);
            }

            // 3단계: 전투 종료
            Debug.Log("\n[3단계] 전투 종료");
            Debug.Log(playerSystem.attributes.GetAllAttributesInfo());
        }

        private void Update()
        {
            // Effect Duration 업데이트
            if (playerSystem != null)
            {
                playerSystem.UpdateEffects(Time.deltaTime);
            }
            if (enemySystem != null)
            {
                enemySystem.UpdateEffects(Time.deltaTime);
            }

            // UI 업데이트
            if (Time.time >= nextUiUpdate)
            {
                nextUiUpdate = Time.time + uiUpdateInterval;
                // UI 업데이트 로직
            }

            // 입력 처리
            HandleInput();
        }

        private void HandleInput()
        {
            // 플레이어 조작
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ApplyQuickBuff();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                ApplyQuickDebuff();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                SimulateQuickCombat();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestoreHealth();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void ApplyQuickBuff()
        {
            playerSystem.ApplyEffect(
                "빠른 버프",
                5f,
                (AttributeSetExample.AttributeType.AttackPower, ModifierTestExample.ModifierOperation.Add, 5f)
            );
        }

        private void ApplyQuickDebuff()
        {
            enemySystem.ApplyEffect(
                "약화",
                5f,
                (AttributeSetExample.AttributeType.Defense, ModifierTestExample.ModifierOperation.Multiply, 0.5f)
            );
        }

        private void SimulateQuickCombat()
        {
            float damage = playerSystem.attributes.CalculateEffectiveDamage(15f);
            var enemyHealth = enemySystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);
            enemyHealth.AddToCurrentValue(-damage);
            Debug.Log($"빠른 공격! {damage:F1} 데미지");
        }

        private void RestoreHealth()
        {
            var playerHealth = playerSystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);
            var enemyHealth = enemySystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);

            playerHealth.SetCurrentValue(playerHealth.BaseValue);
            enemyHealth.SetCurrentValue(enemyHealth.BaseValue);

            Debug.Log("체력 완전 회복!");
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== 현재 상태 ===");
            Debug.Log($"[플레이어] 활성 효과: {playerSystem.activeEffects.Count}개");
            Debug.Log(playerSystem.attributes.GetAllAttributesInfo());
            Debug.Log($"\n[적] 활성 효과: {enemySystem.activeEffects.Count}개");
            Debug.Log(enemySystem.attributes.GetAllAttributesInfo());
        }

        private void OnGUI()
        {
            // 메인 박스
            GUI.Box(new Rect(10, 10, 450, 300), "Phase 1 - 통합 테스트");

            // 테스트 모드
            GUI.Label(new Rect(20, 40, 200, 20), $"테스트 모드: {currentTestMode}");

            int y = 70;

            // 플레이어 정보
            GUI.Label(new Rect(20, y, 200, 20), "=== 플레이어 ===");
            y += 25;

            if (playerSystem != null)
            {
                var health = playerSystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);
                var mana = playerSystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Mana);

                GUI.Label(new Rect(20, y, 200, 20), $"HP: {health?.CurrentValue:F0}/{health?.BaseValue:F0}");
                y += 20;
                GUI.Label(new Rect(20, y, 200, 20), $"MP: {mana?.CurrentValue:F0}/{mana?.BaseValue:F0}");
                y += 20;
                GUI.Label(new Rect(20, y, 200, 20), $"공격력: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.AttackPower):F0}");
                y += 20;
                GUI.Label(new Rect(20, y, 200, 20), $"방어력: {playerSystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.Defense):F0}");
                y += 20;
                GUI.Label(new Rect(20, y, 200, 20), $"활성 효과: {playerSystem.activeEffects.Count}개");
            }

            // 적 정보
            y = 95;
            GUI.Label(new Rect(240, y, 200, 20), "=== 적 ===");
            y += 25;

            if (enemySystem != null)
            {
                var health = enemySystem.attributes.GetAttribute(AttributeSetExample.AttributeType.Health);

                GUI.Label(new Rect(240, y, 200, 20), $"HP: {health?.CurrentValue:F0}/{health?.BaseValue:F0}");
                y += 20;
                GUI.Label(new Rect(240, y, 200, 20), $"공격력: {enemySystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.AttackPower):F0}");
                y += 20;
                GUI.Label(new Rect(240, y, 200, 20), $"방어력: {enemySystem.attributes.GetAttributeValue(AttributeSetExample.AttributeType.Defense):F0}");
                y += 20;
                GUI.Label(new Rect(240, y, 200, 20), $"활성 효과: {enemySystem.activeEffects.Count}개");
            }

            // 조작 설명
            y = 220;
            GUI.Label(new Rect(20, y, 430, 70),
                "Q: 플레이어 버프  W: 적 디버프  E: 빠른 공격\n" +
                "R: 체력 회복  Space: 상태 출력\n\n" +
                "Phase 1 학습 완료! 다음 단계로 진행하세요.");
        }
    }
}