using UnityEngine;
using System.Collections.Generic;
using Core.Enums;

namespace GAS.Core
{
    /// <summary>
    /// 범용 어빌리티 데이터
    /// 다양한 게임 장르에서 사용할 수 있도록 설계
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbility", menuName = "GAS/AbilityData")]
    public class AbilityData : ScriptableObject, IAbilityData
    {
        [Header("기본 정보")]
        [SerializeField] private string abilityId;
        [SerializeField] private string abilityName;
        [TextArea(3, 5)]
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [Header("어빌리티 타입")]
        [SerializeField] private AbilityType abilityType = AbilityType.Active;
        [SerializeField] private TargetType targetType = TargetType.None;

        [Header("실행 설정")]
        [SerializeField] private float cooldownDuration = 1f;
        [SerializeField] private float castTime = 0f;
        [SerializeField] private float duration = 0f;
        [SerializeField] private bool canBeCancelled = true;

        [Header("리소스 비용")]
        [SerializeField] private List<ResourceCost> resourceCosts = new List<ResourceCost>();

        [Header("범위/타겟팅")]
        [SerializeField] private float range = 5f;
        [SerializeField] private float radius = 1f;
        [SerializeField] private float angle = 90f;

        [Header("태그 시스템")]
        [SerializeField] private List<string> abilityTags = new List<string>();
        [SerializeField] private List<string> cancelTags = new List<string>();
        [SerializeField] private List<string> blockTags = new List<string>();

        [Header("효과")]
        [SerializeField] private float damageValue = 0f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private float healValue = 0f;

        [Header("비주얼/오디오")]
        [SerializeField] private string animationTrigger;
        [SerializeField] private GameObject effectPrefab;
        [SerializeField] private AudioClip soundEffect;

        [Header("커스텀 프로퍼티")]
        [SerializeField] private List<CustomProperty> customProperties = new List<CustomProperty>();

        [Header("어빌리티 체이닝 (콤보 시스템)")]
        [Tooltip("이 어빌리티가 체인(콤보) 시스템을 사용하는지 여부")]
        [SerializeField] private bool isComboAbility = false;

        [Tooltip("체인의 시작점인지 여부 (첫 콤보)")]
        [SerializeField] private bool isChainStarter = false;

        [Tooltip("다음 어빌리티 ID (체인 연결)")]
        [SerializeField] private string nextAbilityId = "";

        [Tooltip("다음 어빌리티 입력 가능 시간 (초)")]
        [Range(0.1f, 2f)]
        [SerializeField] private float chainWindowDuration = 0.5f;

        [Tooltip("체인 실패 시 자동으로 첫 체인으로 리셋")]
        [SerializeField] private bool autoResetChain = true;

        // IAbilityData 구현
        public string AbilityId
        {
            get => abilityId;
            set => abilityId = value;
        }
        public string AbilityName
        {
            get => abilityName;
            set => abilityName = value;
        }
        public string Description => description;
        public AbilityType AbilityType => abilityType;
        public float CooldownDuration => cooldownDuration;
        public float CastTime => castTime;
        public float Duration => duration;
        public bool CanBeCancelled => canBeCancelled;
        public float Range => range;
        public TargetType TargetType => targetType;

        public Dictionary<string, float> ResourceCosts
        {
            get
            {
                var costs = new Dictionary<string, float>();
                foreach (var cost in resourceCosts)
                {
                    costs[cost.resourceType] = cost.amount;
                }
                return costs;
            }
        }

        public List<string> AbilityTags => new List<string>(abilityTags);
        public List<string> CancelTags => new List<string>(cancelTags);
        public List<string> BlockTags => new List<string>(blockTags);

        // 추가 프로퍼티
        public Sprite Icon => icon;
        public float Radius => radius;
        public float Angle => angle;
        public float DamageValue => damageValue;
        public DamageType DamageType => damageType;
        public float HealValue => healValue;
        public string AnimationTrigger => animationTrigger;
        public GameObject EffectPrefab => effectPrefab;
        public AudioClip SoundEffect => soundEffect;

        // 체이닝 프로퍼티
        public bool IsComboAbility => isComboAbility;
        public bool IsChainStarter => isChainStarter;
        public string NextAbilityId => nextAbilityId;
        public float ChainWindowDuration => chainWindowDuration;
        public bool AutoResetChain => autoResetChain;

        /// <summary>
        /// 커스텀 프로퍼티 값 가져오기
        /// </summary>
        public T GetCustomProperty<T>(string key, T defaultValue = default)
        {
            var property = customProperties.Find(p => p.key == key);
            if (property != null)
            {
                try
                {
                    return (T)System.Convert.ChangeType(property.value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 데이터 유효성 검증
        /// </summary>
        public bool ValidateData()
        {
            return !string.IsNullOrEmpty(abilityId) &&
                   !string.IsNullOrEmpty(abilityName) &&
                   cooldownDuration >= 0 &&
                   castTime >= 0;
        }

        /// <summary>
        /// 특정 리소스 비용 확인
        /// </summary>
        public float GetResourceCost(string resourceType)
        {
            var cost = resourceCosts.Find(c => c.resourceType == resourceType);
            return cost?.amount ?? 0f;
        }

        /// <summary>
        /// 어빌리티가 특정 태그를 가지고 있는지 확인
        /// </summary>
        public bool HasTag(string tag)
        {
            return abilityTags.Contains(tag);
        }

        /// <summary>
        /// 에디터 검증 (Inspector 값 변경 시 호출)
        /// </summary>
        private void OnValidate()
        {
            // 콤보 어빌리티가 아니면 체이닝 필드 초기화
            if (!isComboAbility)
            {
                isChainStarter = false;
                nextAbilityId = "";
                chainWindowDuration = 0.5f;
                autoResetChain = true;
            }

            // 체인 스타터는 nextAbilityId 필수
            if (isComboAbility && isChainStarter && string.IsNullOrEmpty(nextAbilityId))
            {
                Debug.LogWarning($"[{abilityName}] 체인 스타터는 NextAbilityId가 필요합니다.");
            }

            // 기본 검증
            if (cooldownDuration < 0) cooldownDuration = 0;
            if (castTime < 0) castTime = 0;
            if (duration < 0) duration = 0;
        }
    }

    /// <summary>
    /// 리소스 비용 정보
    /// </summary>
    [System.Serializable]
    public class ResourceCost
    {
        public string resourceType;
        public float amount;
    }

    /// <summary>
    /// 커스텀 프로퍼티
    /// </summary>
    [System.Serializable]
    public class CustomProperty
    {
        public string key;
        public string value;
    }
}
