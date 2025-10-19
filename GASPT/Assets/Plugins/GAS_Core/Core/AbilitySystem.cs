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

        // 체이닝 관리 (콤보 시스템)
        private string currentChainStarterId = null;   // 현재 체인의 시작점
        private string nextChainAbilityId = null;       // 다음 준비된 어빌리티
        private float chainTimer = 0f;                  // 체인 윈도우 타이머
        private bool isChainActive = false;
        private string currentChainingAbilityId = null; // 현재 체이닝 처리 중인 어빌리티 (중복 방지)

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

            // 체이닝 타이머 업데이트
            UpdateChainTimer();
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
        /// 체이닝 타이머 업데이트
        /// </summary>
        private void UpdateChainTimer()
        {
            if (isChainActive && chainTimer > 0f)
            {
                chainTimer -= Time.deltaTime;

                if (chainTimer <= 0f)
                {
                    // 체인 윈도우 만료 - 완전 초기화
                    Debug.Log($"<color=#FF6B6B>[AbilitySystem] ⏱ 체인 타임아웃</color>");
                    ClearChain();
                }
            }
        }

        /// <summary>
        /// 다음 체인 준비
        /// </summary>
        private void PrepareNextChain(string nextAbilityId, float windowDuration)
        {
            nextChainAbilityId = nextAbilityId;
            chainTimer = windowDuration;
            isChainActive = true;

            Debug.Log($"<color=#00BFFF>[AbilitySystem] ⏳ 다음 체인 준비: {nextAbilityId} (윈도우: {windowDuration}초)</color>");
        }

        /// <summary>
        /// 체인 완전 초기화
        /// </summary>
        private void ClearChain()
        {
            if (!string.IsNullOrEmpty(currentChainStarterId))
            {
                Debug.Log($"<color=#9370DB>[AbilitySystem] ✓ 체인 완료/초기화</color>");
            }

            nextChainAbilityId = null;
            currentChainStarterId = null;
            chainTimer = 0f;
            isChainActive = false;
            currentChainingAbilityId = null;
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
                Debug.LogWarning($"<color=#FF0000>[AbilitySystem] ✗ 어빌리티 없음: {abilityId}</color>");
                return false;
            }

            // 어빌리티 실행 가능 확인
            if (!ability.CanExecute())
            {
                Debug.LogWarning($"<color=#FF0000>[AbilitySystem] ✗ CanExecute 실패: {abilityId} (상태: {ability.State}, 쿨다운: {ability.CooldownRemaining:F2}초)</color>");
                return false;
            }

            // 리소스 확인
            if (useResourceSystem && ability.Data != null)
            {
                foreach (var cost in ability.Data.ResourceCosts)
                {
                    if (!HasEnoughResource(cost.Key, cost.Value))
                    {
                        Debug.LogWarning($"<color=#FF0000>[AbilitySystem] ✗ 리소스 부족: {abilityId} (필요: {cost.Key} {cost.Value})</color>");
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

            // 리소스 소모
            if (useResourceSystem && ability.Data != null)
            {
                foreach (var cost in ability.Data.ResourceCosts)
                {
                    ConsumeResource(cost.Key, cost.Value);
                }
            }

            // 어빌리티 실행 (비동기)
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
        /// 어빌리티 활성화 (편의 메서드, 체이닝 지원)
        /// </summary>
        public bool ActivateAbility(string abilityId)
        {
            Debug.Log($"<color=#FFFFFF>[AbilitySystem] → ActivateAbility 호출: {abilityId} (체인활성: {isChainActive}, 다음체인: {nextChainAbilityId})</color>");

            // 체이닝 활성 중이면 nextChainAbilityId 사용
            string targetAbilityId = abilityId;

            if (isChainActive && !string.IsNullOrEmpty(nextChainAbilityId))
            {
                // 체인 스타터 입력만 체인 진행 가능
                if (abilityId == currentChainStarterId)
                {
                    targetAbilityId = nextChainAbilityId;
                    Debug.Log($"<color=#FFA500>[AbilitySystem] ⚡ 체인 진행: {abilityId} → {nextChainAbilityId}</color>");

                    // 체인 즉시 소비 (중복 진행 방지)
                    nextChainAbilityId = null;
                    isChainActive = false;
                }
                else
                {
                    // 잘못된 입력 무시
                    Debug.LogWarning($"<color=#FF0000>[AbilitySystem] ✗ 체인 활성 중 - 잘못된 입력 무시: {abilityId}</color>");
                    return false;
                }
            }

            // 어빌리티 실행
            bool result = TryUseAbility(targetAbilityId);

            if (!result)
            {
                Debug.LogWarning($"<color=#FF0000>[AbilitySystem] ✗ 어빌리티 실행 실패: {targetAbilityId}</color>");
            }

            if (result && abilities.TryGetValue(targetAbilityId, out var ability))
            {
                // 체인 스타터 등록
                if (ability.Data is AbilityData data && data.IsComboAbility && data.IsChainStarter)
                {
                    currentChainStarterId = targetAbilityId;
                    Debug.Log($"<color=#00FF00>[AbilitySystem] ▶ 체인 시작: {targetAbilityId}</color>");
                }

                // 완료 시 체이닝 처리 (비동기로 대기)
                _ = HandleAbilityChaining(ability);
            }

            return result;
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
        /// 어빌리티 체이닝 처리 (비동기)
        /// </summary>
        private async Awaitable HandleAbilityChaining(IAbility ability)
        {
            // 중복 실행 방지
            string abilityId = ability.Data.AbilityId;
            if (currentChainingAbilityId == abilityId)
            {
                return; // 이미 처리 중
            }

            currentChainingAbilityId = abilityId;

            // 어빌리티 실행 완료 대기
            // Ability 클래스의 State를 모니터링
            while (ability.State == AbilityState.Casting || ability.State == AbilityState.Active)
            {
                await Awaitable.WaitForSecondsAsync(0.05f);
            }

            // 체이닝 처리
            if (ability.Data is AbilityData data && data.IsComboAbility)
            {
                if (!string.IsNullOrEmpty(data.NextAbilityId))
                {
                    // 다음 체인 준비
                    PrepareNextChain(data.NextAbilityId, data.ChainWindowDuration);
                }
                else
                {
                    // 체인 종료 (마지막 콤보) - 완전 초기화
                    ClearChain();
                }
            }

            // 처리 완료
            if (currentChainingAbilityId == abilityId)
            {
                currentChainingAbilityId = null;
            }
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
