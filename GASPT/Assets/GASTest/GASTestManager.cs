// ================================
// File: Assets/Scripts/GAS/Test/GASTestManager.cs
// GAS 시스템 통합 테스트 매니저
// ================================
using GAS.AbilitySystem;
using GAS.AbilitySystem.Abilities;
using GAS.AttributeSystem;
using GAS.EffectSystem;
using GAS.EffectSystem.Effects;
using GAS.TagSystem;
using System.Collections.Generic;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.Test
{
    /// <summary>
    /// GAS 시스템 테스트를 위한 통합 매니저
    /// </summary>
    public class GASTestManager : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool autoSetupPlayer = true;
        [SerializeField] private bool autoSetupEnemies = true;
        [SerializeField] private bool showDebugUI = true;

        [Header("Player Setup")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private AttributeSetData playerAttributes;
        [SerializeField] private List<GameplayAbility> playerAbilities;

        [Header("Enemy Setup")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private AttributeSetData enemyAttributes;
        [SerializeField] private int enemyCount = 3;

        [Header("Test Effects")]
        [SerializeField] private DamageEffect testDamageEffect;
        [SerializeField] private HealEffect testHealEffect;
        [SerializeField] private SpeedBuffEffect testSpeedBuff;

        [Header("Runtime")]
        [SerializeField] private GameObject playerInstance;
        [SerializeField] private List<GameObject> enemyInstances = new List<GameObject>();

        // Components
        private AbilitySystemComponent playerAbilitySystem;
        private AttributeSetComponent playerAttributeSet;
        private EffectComponent playerEffectComponent;
        private TagComponent playerTagComponent;

        #region Unity Lifecycle

        private void Start()
        {
            if (autoSetupPlayer)
            {
                SetupPlayer();
            }

            if (autoSetupEnemies)
            {
                SetupEnemies();
            }

            CreateTestEffects();
        }

        private void OnGUI()
        {
            if (!showDebugUI) return;

            DrawDebugUI();
        }

        #endregion

        #region Setup

        private void SetupPlayer()
        {
            // 플레이어가 이미 있는지 확인
            playerInstance = GameObject.FindWithTag("Player");

            if (playerInstance == null)
            {
                // 플레이어 생성
                if (playerPrefab != null)
                {
                    playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                }
                else
                {
                    playerInstance = GameObject.Find("Player");
                }
            }

            if (playerInstance == null)
            {
                Debug.LogError("[GASTestManager] Player not found!");
                return;
            }

            // GAS 컴포넌트 추가/가져오기
            SetupGASComponents(playerInstance, true);

            // 어빌리티 부여
            GrantPlayerAbilities();

            Debug.Log("[GASTestManager] Player setup complete");
        }

        private void SetupEnemies()
        {
            // 기존 적 찾기
            GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            enemyInstances.AddRange(existingEnemies);

            // 추가 적 생성
            int toSpawn = enemyCount - enemyInstances.Count;
            for (int i = 0; i < toSpawn; i++)
            {
                Vector3 spawnPos = new Vector3(Random.Range(-5f, 5f), 0, 0);
                GameObject enemy = null;

                if (enemyPrefab != null)
                {
                    enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    enemy.transform.position = spawnPos;
                    enemy.tag = "Enemy";
                }

                SetupGASComponents(enemy, false);
                enemyInstances.Add(enemy);
            }

            Debug.Log($"[GASTestManager] Setup {enemyInstances.Count} enemies");
        }

        private void SetupGASComponents(GameObject target, bool isPlayer)
        {
            // AttributeSetComponent
            var attributes = target.GetComponent<AttributeSetComponent>();
            if (attributes == null)
            {
                attributes = target.AddComponent<AttributeSetComponent>();
            }

            // 어트리뷰트 데이터 설정
            if (isPlayer && playerAttributes != null)
            {
                // PlayerAttributes 설정
                // attributes.SetAttributeSetData(playerAttributes);
            }
            else if (!isPlayer && enemyAttributes != null)
            {
                // EnemyAttributes 설정
                // attributes.SetAttributeSetData(enemyAttributes);
            }

            // TagComponent
            var tags = target.GetComponent<TagComponent>();
            if (tags == null)
            {
                tags = target.AddComponent<TagComponent>();
            }

            // EffectComponent
            var effects = target.GetComponent<EffectComponent>();
            if (effects == null)
            {
                effects = target.AddComponent<EffectComponent>();
            }

            // AbilitySystemComponent (플레이어만)
            if (isPlayer)
            {
                var abilities = target.GetComponent<AbilitySystemComponent>();
                if (abilities == null)
                {
                    abilities = target.AddComponent<AbilitySystemComponent>();
                }
                playerAbilitySystem = abilities;

                // AbilityInputHandler
                var inputHandler = target.GetComponent<AbilityInputHandler>();
                if (inputHandler == null)
                {
                    inputHandler = target.AddComponent<AbilityInputHandler>();
                }
            }

            // 참조 저장
            if (isPlayer)
            {
                playerAttributeSet = attributes;
                playerEffectComponent = effects;
                playerTagComponent = tags;
            }
        }

        private void GrantPlayerAbilities()
        {
            if (playerAbilitySystem == null) return;

            // 리스트의 어빌리티 부여
            for (int i = 0; i < playerAbilities.Count; i++)
            {
                if (playerAbilities[i] != null)
                {
                    playerAbilitySystem.GiveAbility(playerAbilities[i], i + 1);
                }
            }

            // 기본 어빌리티가 없으면 생성
            if (playerAbilities.Count == 0)
            {
                CreateDefaultAbilities();
            }
        }

        private void CreateDefaultAbilities()
        {
            // 근접 공격
            var meleeAttack = ScriptableObject.CreateInstance<MeleeAttackAbility>();
            meleeAttack.name = "DefaultMeleeAttack";
            playerAbilitySystem.GiveAbility(meleeAttack, 1);

            // 대시
            var dash = ScriptableObject.CreateInstance<DashAbility>();
            dash.name = "DefaultDash";
            playerAbilitySystem.GiveAbility(dash, 2);

            // 원거리 공격
            var projectile = ScriptableObject.CreateInstance<ProjectileAttackAbility>();
            projectile.name = "DefaultProjectile";
            playerAbilitySystem.GiveAbility(projectile, 3);

            Debug.Log("[GASTestManager] Created default abilities");
        }

        private void CreateTestEffects()
        {
            // 테스트 이펙트가 없으면 생성
            if (testDamageEffect == null)
            {
                testDamageEffect = ScriptableObject.CreateInstance<DamageEffect>();
                testDamageEffect.name = "TestDamageEffect";
            }

            if (testHealEffect == null)
            {
                testHealEffect = ScriptableObject.CreateInstance<HealEffect>();
                testHealEffect.name = "TestHealEffect";
            }

            if (testSpeedBuff == null)
            {
                testSpeedBuff = ScriptableObject.CreateInstance<SpeedBuffEffect>();
                testSpeedBuff.name = "TestSpeedBuff";
            }
        }

        #endregion

        #region Debug UI

        private void DrawDebugUI()
        {
            if (playerInstance == null) return;

            // UI 영역
            GUILayout.BeginArea(new Rect(10, 10, 300, 600));

            GUILayout.Label("=== GAS Test Manager ===", GUI.skin.box);

            // 플레이어 상태
            DrawPlayerStatus();

            GUILayout.Space(10);

            // 테스트 버튼
            DrawTestButtons();

            GUILayout.Space(10);

            // 어빌리티 정보
            DrawAbilityInfo();

            GUILayout.Space(10);

            // 이펙트 정보
            DrawEffectInfo();

            GUILayout.EndArea();
        }

        private void DrawPlayerStatus()
        {
            GUILayout.Label("Player Status:", GUI.skin.box);

            if (playerAttributeSet != null)
            {
                float health = playerAttributeSet.GetAttributeValue(AttributeType.Health);
                float maxHealth = playerAttributeSet.GetAttributeValue(AttributeType.HealthMax);
                float mana = playerAttributeSet.GetAttributeValue(AttributeType.Mana);
                float maxMana = playerAttributeSet.GetAttributeValue(AttributeType.ManaMax);

                GUILayout.Label($"Health: {health:F0}/{maxHealth:F0}");
                GUILayout.Label($"Mana: {mana:F0}/{maxMana:F0}");

                float attackPower = playerAttributeSet.GetAttributeValue(AttributeType.AttackPower);
                float defense = playerAttributeSet.GetAttributeValue(AttributeType.Defense);
                float moveSpeed = playerAttributeSet.GetAttributeValue(AttributeType.MovementSpeed);

                GUILayout.Label($"Attack: {attackPower:F0} | Defense: {defense:F0}");
                GUILayout.Label($"Move Speed: {moveSpeed:F1}");
            }

            if (playerTagComponent != null)
            {
                var tags = playerTagComponent.GetAllTags();
                string tagList = tags.Count > 0 ? string.Join(", ", tags) : "None";
                GUILayout.Label($"Tags: {tagList}");
            }
        }

        private void DrawTestButtons()
        {
            GUILayout.Label("Test Actions:", GUI.skin.box);

            // 데미지 테스트
            if (GUILayout.Button("Take 10 Damage"))
            {
                ApplyTestDamage(10f);
            }

            // 힐 테스트
            if (GUILayout.Button("Heal 20 HP"))
            {
                ApplyTestHeal(20f);
            }

            // 버프 테스트
            if (GUILayout.Button("Apply Speed Buff"))
            {
                ApplyTestBuff();
            }

            // 적 스폰
            if (GUILayout.Button("Spawn Enemy"))
            {
                SpawnTestEnemy();
            }

            // 초기화
            if (GUILayout.Button("Reset Player"))
            {
                ResetPlayer();
            }
        }

        private void DrawAbilityInfo()
        {
            if (playerAbilitySystem == null) return;

            GUILayout.Label("Abilities:", GUI.skin.box);

            var abilities = playerAbilitySystem.GetGrantedAbilities();
            foreach (var spec in abilities)
            {
                if (spec.ability != null)
                {
                    string status = spec.IsOnCooldown ? $"CD: {spec.GetCooldownRemaining():F1}s" : "Ready";
                    GUILayout.Label($"{spec.ability.AbilityName} [{spec.inputId}]: {status}");
                }
            }
        }

        private void DrawEffectInfo()
        {
            if (playerEffectComponent == null) return;

            GUILayout.Label("Active Effects:", GUI.skin.box);

            var effects = playerEffectComponent.GetActiveEffects();
            if (effects.Count == 0)
            {
                GUILayout.Label("No active effects");
            }
            else
            {
                foreach (var effect in effects)
                {
                    if (effect != null && effect.effect != null)
                    {
                        string remaining = effect.remainingDuration > 0 ?
                            $" ({effect.remainingDuration:F1}s)" : "";
                        GUILayout.Label($"- {effect.effect.EffectName}{remaining}");
                    }
                }
            }
        }

        #endregion

        #region Test Actions

        private void ApplyTestDamage(float amount)
        {
            if (playerEffectComponent == null || testDamageEffect == null) return;

            var context = new EffectContext
            {
                source = playerInstance,
                target = playerInstance,
                power = amount
            };

            playerEffectComponent.ApplyEffect(testDamageEffect, context);
        }

        private void ApplyTestHeal(float amount)
        {
            if (playerEffectComponent == null || testHealEffect == null) return;

            var context = new EffectContext
            {
                source = playerInstance,
                target = playerInstance,
                power = amount
            };

            playerEffectComponent.ApplyEffect(testHealEffect, context);
        }

        private void ApplyTestBuff()
        {
            if (playerEffectComponent == null || testSpeedBuff == null) return;

            var context = new EffectContext
            {
                source = playerInstance,
                target = playerInstance
            };

            playerEffectComponent.ApplyEffect(testSpeedBuff, context);
        }

        private void SpawnTestEnemy()
        {
            Vector3 spawnPos = playerInstance.transform.position + Vector3.right * 3;
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.transform.position = spawnPos;
            enemy.tag = "Enemy";
            enemy.name = $"TestEnemy_{enemyInstances.Count}";

            SetupGASComponents(enemy, false);
            enemyInstances.Add(enemy);
        }

        private void ResetPlayer()
        {
            if (playerAttributeSet != null)
            {
                // 체력/마나 회복
                playerAttributeSet.SetAttribute(AttributeType.Health,
                    playerAttributeSet.GetAttributeValue(AttributeType.HealthMax));
                playerAttributeSet.SetAttribute(AttributeType.Mana,
                    playerAttributeSet.GetAttributeValue(AttributeType.ManaMax));
            }

            if (playerEffectComponent != null)
            {
                // 모든 이펙트 제거
                playerEffectComponent.RemoveAllEffects();
            }

            if (playerAbilitySystem != null)
            {
                // 쿨다운 리셋
                playerAbilitySystem.ClearAllCooldowns();
            }
        }

        #endregion
    }
}