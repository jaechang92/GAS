using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace GAS.Core
{
    /// <summary>
    /// 범용 어빌리티 시스템
    /// 다양한 게임 장르에서 사용할 수 있도록 설계
    /// </summary>
    public class AbilitySystem : MonoBehaviour, IAbilitySystem
    {
        [Header("초기 어빌리티")]
        [SerializeField] private List<AbilityData> initialAbilities = new List<AbilityData>();

        [Header("리소스 시스템 (선택사항)")]
        [SerializeField] private bool useResourceSystem = true;
        [SerializeField] private List<ResourceConfig> resourceConfigs = new List<ResourceConfig>();

        // 등록된 어빌리티들
        private Dictionary<string, IAbility> abilities = new Dictionary<string, IAbility>();

        // 리소스 시스템
        private Dictionary<string, float> currentResources = new Dictionary<string, float>();
        private Dictionary<string, float> maxResources = new Dictionary<string, float>();

        // 게임플레이 컨텍스트
        private IGameplayContext gameplayContext;

        // 프로퍼티
        public IReadOnlyDictionary<string, IAbility> Abilities => abilities;
        public bool UseResourceSystem => useResourceSystem;

        // 이벤트
        public event Action<string> OnAbilityAdded;
        public event Action<string> OnAbilityRemoved;
        public event Action<string> OnAbilityUsed;
        public event Action<string> OnAbilityCancelled;
        public event Action<string, float> OnResourceChanged;

        /// <summary>
        /// 시스템 초기화
        /// </summary>
        private void Awake()
        {
            InitializeResourceSystem();
            InitializeAbilities();
            InitializeGameplayContext();
        }

        /// <summary>
        /// 리소스 시스템 초기화
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
        /// 초기 어빌리티 등록
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
        /// 게임플레이 컨텍스트 초기화
        /// </summary>
        private void InitializeGameplayContext()
        {
            gameplayContext = GetComponent<IGameplayContext>();
            if (gameplayContext == null)
            {
                // 기본 컨텍스트 생성
                var contextComponent = gameObject.AddComponent<DefaultGameplayContext>();
                gameplayContext = contextComponent;
            }
        }

        /// <summary>
        /// 매 프레임 업데이트
        /// </summary>
        private void Update()
        {
            // 모든 어빌리티의 쿨다운 업데이트
            foreach (var ability in abilities.Values)
            {
                if (ability is Ability concreteAbility)
                {
                    concreteAbility.UpdateCooldown(Time.deltaTime);
                }
            }

            // 리소스 자동 회복
            if (useResourceSystem)
            {
                UpdateResourceRegeneration();
            }
        }

        /// <summary>
        /// 리소스 자동 회복 처리
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
        /// 어빌리티 생성
        /// </summary>
        private IAbility CreateAbility(IAbilityData data)
        {
            var ability = new Ability();
            ability.Initialize(data, gameObject, this, gameplayContext);
            return ability;
        }

        #region IAbilitySystem 구현

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

            // 기존 어빌리티가 있으면 교체
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
        /// 어빌리티 비동기 실행
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
        /// 리소스 충분 여부 확인
        /// </summary>
        private bool HasEnoughResource(string resourceType, float amount)
        {
            return currentResources.TryGetValue(resourceType, out var current) && current >= amount;
        }

        /// <summary>
        /// 어빌리티 데이터로 어빌리티 추가
        /// </summary>
        public void AddAbility(IAbilityData abilityData)
        {
            if (abilityData == null) return;

            var ability = CreateAbility(abilityData);
            AddAbility(ability);
        }

        /// <summary>
        /// 최대 리소스 값 설정
        /// </summary>
        public void SetMaxResource(string resourceType, float maxValue)
        {
            maxResources[resourceType] = maxValue;

            // 현재 값이 최대값을 초과하면 조정
            if (currentResources.TryGetValue(resourceType, out var current) && current > maxValue)
            {
                SetResource(resourceType, maxValue);
            }
        }

        /// <summary>
        /// 최대 리소스 값 가져오기
        /// </summary>
        public float GetMaxResource(string resourceType)
        {
            return maxResources.TryGetValue(resourceType, out var value) ? value : 0f;
        }

        /// <summary>
        /// 모든 어빌리티 쿨다운 리셋
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
        /// 어빌리티 활성화 (편의 메서드)
        /// TryUseAbility의 별칭
        /// </summary>
        public bool ActivateAbility(string abilityId)
        {
            return TryUseAbility(abilityId);
        }

        /// <summary>
        /// 어빌리티 비활성화 (편의 메서드)
        /// CancelAbility의 별칭
        /// </summary>
        public void DeactivateAbility(string abilityId)
        {
            CancelAbility(abilityId);
        }

        /// <summary>
        /// 시스템 정리
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
    /// 리소스 설정
    /// </summary>
    [System.Serializable]
    public class ResourceConfig
    {
        public string resourceType;
        public float maxValue = 100f;
        public float initialValue = 100f;
        public float regenerationRate = 0f; // 초당 회복량
    }
}