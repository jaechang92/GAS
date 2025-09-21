using UnityEngine;
using GAS.Core;

namespace GAS.Examples
{
    /// <summary>
    /// GAS Core 사용 예제
    /// </summary>
    public class SimpleAbilityExample : MonoBehaviour
    {
        [Header("어빌리티 설정")]
        [SerializeField] private AbilityData[] initialAbilities;

        [Header("리소스 설정")]
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
            // 키보드 입력으로 어빌리티 테스트
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
        /// 어빌리티 시스템 설정
        /// </summary>
        private void SetupAbilitySystem()
        {
            var systemComponent = GetComponent<GAS.Core.AbilitySystem>();
            if (systemComponent == null)
            {
                systemComponent = gameObject.AddComponent<GAS.Core.AbilitySystem>();
            }

            abilitySystem = systemComponent;

            // 이벤트 등록
            abilitySystem.OnAbilityUsed += OnAbilityUsed;
            abilitySystem.OnResourceChanged += OnResourceChanged;
        }

        /// <summary>
        /// 초기 어빌리티 추가
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
        /// 리소스 시스템 설정
        /// </summary>
        private void SetupResourceSystem()
        {
            if (!useResourceSystem) return;

            // 마나 설정
            abilitySystem.SetMaxResource("Mana", maxMana);
            abilitySystem.SetResource("Mana", maxMana);

            // 스태미나 설정
            abilitySystem.SetMaxResource("Stamina", maxStamina);
            abilitySystem.SetResource("Stamina", maxStamina);

            Debug.Log($"Resource system initialized - Mana: {maxMana}, Stamina: {maxStamina}");
        }

        /// <summary>
        /// 어빌리티 사용 시도
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
        /// 어빌리티 사용 이벤트 핸들러
        /// </summary>
        private void OnAbilityUsed(string abilityId)
        {
            Debug.Log($"Ability used: {abilityId}");

            // UI 업데이트 등 추가 처리 가능
            UpdateUI();
        }

        /// <summary>
        /// 리소스 변경 이벤트 핸들러
        /// </summary>
        private void OnResourceChanged(string resourceType, float newValue)
        {
            Debug.Log($"Resource changed - {resourceType}: {newValue:F1}");

            // UI 업데이트
            UpdateUI();
        }

        /// <summary>
        /// UI 업데이트 (예시)
        /// </summary>
        private void UpdateUI()
        {
            // 실제 프로젝트에서는 UI 컴포넌트 업데이트
            if (useResourceSystem)
            {
                float currentMana = abilitySystem.GetResource("Mana");
                float currentStamina = abilitySystem.GetResource("Stamina");

                // UI 텍스트나 슬라이더 업데이트
                // manaText.text = $"Mana: {currentMana:F0}/{maxMana:F0}";
                // staminaSlider.value = currentStamina / maxStamina;
            }
        }

        /// <summary>
        /// 디버그용: 현재 상태 출력
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
        /// 디버그용: 모든 쿨다운 리셋
        /// </summary>
        [ContextMenu("Reset All Cooldowns")]
        public void ResetAllCooldowns()
        {
            abilitySystem.ResetAllCooldowns();
            Debug.Log("All cooldowns have been reset");
        }

        /// <summary>
        /// 디버그용: 모든 리소스 복원
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