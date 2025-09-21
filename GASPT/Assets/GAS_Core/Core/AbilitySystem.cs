using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace GAS.Core
{
    /// <summary>
    /// ���� �����Ƽ �ý���
    /// �پ��� ���� �帣���� ����� �� �ֵ��� ����
    /// </summary>
    public class AbilitySystem : MonoBehaviour, IAbilitySystem
    {
        [Header("�ʱ� �����Ƽ")]
        [SerializeField] private List<AbilityData> initialAbilities = new List<AbilityData>();

        [Header("���ҽ� �ý��� (���û���)")]
        [SerializeField] private bool useResourceSystem = true;
        [SerializeField] private List<ResourceConfig> resourceConfigs = new List<ResourceConfig>();

        // ��ϵ� �����Ƽ��
        private Dictionary<string, IAbility> abilities = new Dictionary<string, IAbility>();

        // ���ҽ� �ý���
        private Dictionary<string, float> currentResources = new Dictionary<string, float>();
        private Dictionary<string, float> maxResources = new Dictionary<string, float>();

        // �����÷��� ���ؽ�Ʈ
        private IGameplayContext gameplayContext;

        // ������Ƽ
        public IReadOnlyDictionary<string, IAbility> Abilities => abilities;
        public bool UseResourceSystem => useResourceSystem;

        // �̺�Ʈ
        public event Action<string> OnAbilityAdded;
        public event Action<string> OnAbilityRemoved;
        public event Action<string> OnAbilityUsed;
        public event Action<string> OnAbilityCancelled;
        public event Action<string, float> OnResourceChanged;

        /// <summary>
        /// �ý��� �ʱ�ȭ
        /// </summary>
        private void Awake()
        {
            InitializeResourceSystem();
            InitializeAbilities();
            InitializeGameplayContext();
        }

        /// <summary>
        /// ���ҽ� �ý��� �ʱ�ȭ
        /// </summary>
        private void InitializeResourceSystem()
        {
            if (!useResourceSystem) return;

            foreach (var config in resourceConfigs)
            {
                maxResources[config.resourceType] = config.maxValue;
                currentResources[config.resourceType] = config.initialValue;
            }
        }

        /// <summary>
        /// �ʱ� �����Ƽ ����
        /// </summary>
        private void InitializeAbilities()
        {
            foreach (var abilityData in initialAbilities)
            {
                if (abilityData != null)
                {
                    var ability = CreateAbility(abilityData);
                    AddAbility(ability);
                }
            }
        }

        /// <summary>
        /// �����÷��� ���ؽ�Ʈ �ʱ�ȭ
        /// </summary>
        private void InitializeGameplayContext()
        {
            gameplayContext = GetComponent<IGameplayContext>();
            if (gameplayContext == null)
            {
                // �⺻ ���ؽ�Ʈ ����
                var contextComponent = gameObject.AddComponent<DefaultGameplayContext>();
                gameplayContext = contextComponent;
            }
        }

        /// <summary>
        /// �� ������ ������Ʈ
        /// </summary>
        private void Update()
        {
            // ��� �����Ƽ�� ��ٿ� ������Ʈ
            foreach (var ability in abilities.Values)
            {
                if (ability is Ability concreteAbility)
                {
                    concreteAbility.UpdateCooldown(Time.deltaTime);
                }
            }

            // ���ҽ� �ڵ� ȸ��
            if (useResourceSystem)
            {
                UpdateResourceRegeneration();
            }
        }

        /// <summary>
        /// ���ҽ� �ڵ� ȸ�� ó��
        /// </summary>
        private void UpdateResourceRegeneration()
        {
            foreach (var config in resourceConfigs)
            {
                if (config.regenerationRate > 0 && currentResources.ContainsKey(config.resourceType))
                {
                    var oldValue = currentResources[config.resourceType];
                    var newValue = Mathf.Min(
                        maxResources[config.resourceType],
                        oldValue + config.regenerationRate * Time.deltaTime
                    );

                    if (newValue != oldValue)
                    {
                        currentResources[config.resourceType] = newValue;
                        OnResourceChanged?.Invoke(config.resourceType, newValue);
                    }
                }
            }
        }

        /// <summary>
        /// �����Ƽ ����
        /// </summary>
        private IAbility CreateAbility(IAbilityData data)
        {
            var ability = new Ability();
            ability.Initialize(data, gameObject, this, gameplayContext);
            return ability;
        }

        #region IAbilitySystem ����

        public bool HasAbility(string abilityId)
        {
            return abilities.ContainsKey(abilityId);
        }

        public bool TryGetAbility(string abilityId, out IAbility ability)
        {
            return abilities.TryGetValue(abilityId, out ability);
        }

        public void AddAbility(IAbility ability)
        {
            if (ability == null || string.IsNullOrEmpty(ability.Id)) return;

            // ���� �����Ƽ�� ������ ����
            if (abilities.ContainsKey(ability.Id))
            {
                RemoveAbility(ability.Id);
            }

            abilities[ability.Id] = ability;
            OnAbilityAdded?.Invoke(ability.Id);

            Debug.Log($"Added ability: {ability.Name} ({ability.Id})");
        }

        public void RemoveAbility(string abilityId)
        {
            if (abilities.TryGetValue(abilityId, out var ability))
            {
                if (ability is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                abilities.Remove(abilityId);
                OnAbilityRemoved?.Invoke(abilityId);

                Debug.Log($"Removed ability: {abilityId}");
            }
        }

        public bool CanUseAbility(string abilityId)
        {
            if (!abilities.TryGetValue(abilityId, out var ability))
            {
                return false;
            }

            // �����Ƽ ��ü ���� Ȯ��
            if (!ability.CanExecute())
            {
                return false;
            }

            // ���ҽ� Ȯ��
            if (useResourceSystem && ability.Data != null)
            {
                foreach (var cost in ability.Data.ResourceCosts)
                {
                    if (!HasEnoughResource(cost.Key, cost.Value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool TryUseAbility(string abilityId)
        {
            if (!CanUseAbility(abilityId))
            {
                return false;
            }

            var ability = abilities[abilityId];

            // ���ҽ� �Ҹ�
            if (useResourceSystem && ability.Data != null)
            {
                foreach (var cost in ability.Data.ResourceCosts)
                {
                    ConsumeResource(cost.Key, cost.Value);
                }
            }

            // �����Ƽ ���� (�񵿱�)
            _ = ExecuteAbilityAsync(ability);

            OnAbilityUsed?.Invoke(abilityId);
            return true;
        }

        public void CancelAbility(string abilityId)
        {
            if (abilities.TryGetValue(abilityId, out var ability))
            {
                ability.Cancel();
                OnAbilityCancelled?.Invoke(abilityId);
            }
        }

        public void CancelAllAbilities()
        {
            foreach (var ability in abilities.Values)
            {
                ability.Cancel();
            }
        }

        public bool HasResource(string resourceType)
        {
            return currentResources.ContainsKey(resourceType);
        }

        public float GetResource(string resourceType)
        {
            return currentResources.TryGetValue(resourceType, out var value) ? value : 0f;
        }

        public void SetResource(string resourceType, float value)
        {
            if (!currentResources.ContainsKey(resourceType)) return;

            var maxValue = maxResources.TryGetValue(resourceType, out var max) ? max : float.MaxValue;
            var newValue = Mathf.Clamp(value, 0f, maxValue);

            currentResources[resourceType] = newValue;
            OnResourceChanged?.Invoke(resourceType, newValue);
        }

        public bool ConsumeResource(string resourceType, float amount)
        {
            if (!HasEnoughResource(resourceType, amount))
            {
                return false;
            }

            currentResources[resourceType] -= amount;
            OnResourceChanged?.Invoke(resourceType, currentResources[resourceType]);
            return true;
        }

        public void RestoreResource(string resourceType, float amount)
        {
            if (!currentResources.ContainsKey(resourceType)) return;

            var maxValue = maxResources.TryGetValue(resourceType, out var max) ? max : float.MaxValue;
            var newValue = Mathf.Min(maxValue, currentResources[resourceType] + amount);

            currentResources[resourceType] = newValue;
            OnResourceChanged?.Invoke(resourceType, newValue);
        }

        #endregion

        /// <summary>
        /// �����Ƽ �񵿱� ����
        /// </summary>
        private async Awaitable ExecuteAbilityAsync(IAbility ability)
        {
            try
            {
                await ability.ExecuteAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error executing ability {ability.Id}: {e.Message}");
            }
        }

        /// <summary>
        /// ���ҽ� ��� ���� Ȯ��
        /// </summary>
        private bool HasEnoughResource(string resourceType, float amount)
        {
            return currentResources.TryGetValue(resourceType, out var current) && current >= amount;
        }

        /// <summary>
        /// �����Ƽ �����ͷ� �����Ƽ �߰�
        /// </summary>
        public void AddAbility(IAbilityData abilityData)
        {
            if (abilityData == null) return;

            var ability = CreateAbility(abilityData);
            AddAbility(ability);
        }

        /// <summary>
        /// �ִ� ���ҽ� �� ����
        /// </summary>
        public void SetMaxResource(string resourceType, float maxValue)
        {
            maxResources[resourceType] = maxValue;

            // ���� ���� �ִ밪�� �ʰ��ϸ� ����
            if (currentResources.TryGetValue(resourceType, out var current) && current > maxValue)
            {
                SetResource(resourceType, maxValue);
            }
        }

        /// <summary>
        /// �ִ� ���ҽ� �� ��������
        /// </summary>
        public float GetMaxResource(string resourceType)
        {
            return maxResources.TryGetValue(resourceType, out var value) ? value : 0f;
        }

        /// <summary>
        /// ��� �����Ƽ ��ٿ� ����
        /// </summary>
        public void ResetAllCooldowns()
        {
            foreach (var ability in abilities.Values)
            {
                if (ability is Ability concreteAbility)
                {
                    concreteAbility.ResetCooldown();
                }
            }
        }

        /// <summary>
        /// �ý��� ����
        /// </summary>
        private void OnDestroy()
        {
            foreach (var ability in abilities.Values)
            {
                if (ability is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            abilities.Clear();
        }
    }

    /// <summary>
    /// ���ҽ� ����
    /// </summary>
    [System.Serializable]
    public class ResourceConfig
    {
        public string resourceType;
        public float maxValue = 100f;
        public float initialValue = 100f;
        public float regenerationRate = 0f; // �ʴ� ȸ����
    }
}