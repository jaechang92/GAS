using UnityEngine;
using GAS.Core;

namespace GAS.Examples
{
    /// <summary>
    /// GAS Core ��� ����
    /// </summary>
    public class SimpleAbilityExample : MonoBehaviour
    {
        [Header("�����Ƽ ����")]
        [SerializeField] private AbilityData[] initialAbilities;

        [Header("���ҽ� ����")]
        [SerializeField] private bool useResourceSystem = true;
        [SerializeField] private float maxMana = 100f;
        [SerializeField] private float maxStamina = 100f;

        private IAbilitySystem abilitySystem;

        void Start()
        {
            SetupAbilitySystem();
            AddInitialAbilities();
            SetupResourceSystem();
        }

        void Update()
        {
            // Ű���� �Է����� �����Ƽ �׽�Ʈ
            if (Input.GetKeyDown(KeyCode.Q))
            {
                TryUseAbility("basic_attack");
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                TryUseAbility("heal");
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                TryUseAbility("fireball");
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                TryUseAbility("ultimate");
            }
        }

        /// <summary>
        /// �����Ƽ �ý��� ����
        /// </summary>
        private void SetupAbilitySystem()
        {
            var systemComponent = GetComponent<GAS.Core.AbilitySystem>();
            if (systemComponent == null)
            {
                systemComponent = gameObject.AddComponent<GAS.Core.AbilitySystem>();
            }

            abilitySystem = systemComponent;

            // �̺�Ʈ ���
            abilitySystem.OnAbilityUsed += OnAbilityUsed;
            abilitySystem.OnResourceChanged += OnResourceChanged;
        }

        /// <summary>
        /// �ʱ� �����Ƽ �߰�
        /// </summary>
        private void AddInitialAbilities()
        {
            foreach (var abilityData in initialAbilities)
            {
                if (abilityData != null)
                {
                    abilitySystem.AddAbility(abilityData);
                    Debug.Log($"Added ability: {abilityData.AbilityName}");
                }
            }
        }

        /// <summary>
        /// ���ҽ� �ý��� ����
        /// </summary>
        private void SetupResourceSystem()
        {
            if (!useResourceSystem) return;

            // ���� ����
            abilitySystem.SetMaxResource("Mana", maxMana);
            abilitySystem.SetResource("Mana", maxMana);

            // ���¹̳� ����
            abilitySystem.SetMaxResource("Stamina", maxStamina);
            abilitySystem.SetResource("Stamina", maxStamina);

            Debug.Log($"Resource system initialized - Mana: {maxMana}, Stamina: {maxStamina}");
        }

        /// <summary>
        /// �����Ƽ ��� �õ�
        /// </summary>
        private void TryUseAbility(string abilityId)
        {
            if (abilitySystem.CanUseAbility(abilityId))
            {
                bool success = abilitySystem.TryUseAbility(abilityId);
                if (success)
                {
                    Debug.Log($"Successfully used ability: {abilityId}");
                }
                else
                {
                    Debug.Log($"Failed to use ability: {abilityId}");
                }
            }
            else
            {
                Debug.Log($"Cannot use ability: {abilityId} (on cooldown or insufficient resources)");
            }
        }

        /// <summary>
        /// �����Ƽ ��� �̺�Ʈ �ڵ鷯
        /// </summary>
        private void OnAbilityUsed(string abilityId)
        {
            Debug.Log($"Ability used: {abilityId}");

            // UI ������Ʈ �� �߰� ó�� ����
            UpdateUI();
        }

        /// <summary>
        /// ���ҽ� ���� �̺�Ʈ �ڵ鷯
        /// </summary>
        private void OnResourceChanged(string resourceType, float newValue)
        {
            Debug.Log($"Resource changed - {resourceType}: {newValue:F1}");

            // UI ������Ʈ
            UpdateUI();
        }

        /// <summary>
        /// UI ������Ʈ (����)
        /// </summary>
        private void UpdateUI()
        {
            // ���� ������Ʈ������ UI ������Ʈ ������Ʈ
            if (useResourceSystem)
            {
                float currentMana = abilitySystem.GetResource("Mana");
                float currentStamina = abilitySystem.GetResource("Stamina");

                // UI �ؽ�Ʈ�� �����̴� ������Ʈ
                // manaText.text = $"Mana: {currentMana:F0}/{maxMana:F0}";
                // staminaSlider.value = currentStamina / maxStamina;
            }
        }

        /// <summary>
        /// ����׿�: ���� ���� ���
        /// </summary>
        [ContextMenu("Print Current Status")]
        public void PrintCurrentStatus()
        {
            Debug.Log("=== Ability System Status ===");

            foreach (var ability in abilitySystem.Abilities)
            {
                Debug.Log($"Ability: {ability.Value.Name}, State: {ability.Value.State}, " +
                         $"Cooldown: {ability.Value.CooldownRemaining:F1}s");
            }

            if (useResourceSystem)
            {
                Debug.Log($"Mana: {abilitySystem.GetResource("Mana"):F1}/{abilitySystem.GetMaxResource("Mana"):F1}");
                Debug.Log($"Stamina: {abilitySystem.GetResource("Stamina"):F1}/{abilitySystem.GetMaxResource("Stamina"):F1}");
            }
        }

        /// <summary>
        /// ����׿�: ��� ��ٿ� ����
        /// </summary>
        [ContextMenu("Reset All Cooldowns")]
        public void ResetAllCooldowns()
        {
            abilitySystem.ResetAllCooldowns();
            Debug.Log("All cooldowns have been reset");
        }

        /// <summary>
        /// ����׿�: ��� ���ҽ� ����
        /// </summary>
        [ContextMenu("Restore All Resources")]
        public void RestoreAllResources()
        {
            if (useResourceSystem)
            {
                abilitySystem.SetResource("Mana", maxMana);
                abilitySystem.SetResource("Stamina", maxStamina);
                Debug.Log("All resources have been restored");
            }
        }
    }
}