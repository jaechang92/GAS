// ===================================
// 파일: Assets/Scripts/Ability/Data/AbilityData.cs
// ===================================
using UnityEngine;
using AbilitySystem;

namespace AbilitySystem
{
    /// <summary>
    /// 어빌리티 정적 데이터를 담는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbility", menuName = "Ability/AbilityData")]
    public class AbilityData : ScriptableObject
    {
        [Header("기본 정보")]
        public string abilityId;
        public string abilityName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("어빌리티 설정")]
        public AbilityType abilityType = AbilityType.Instant;
        public TargetType targetType = TargetType.Self;
        public float cooldownTime = 1.0f;
        public float castTime = 0f;

        [Header("코스트")]
        public int manaCost = 0;
        public int staminaCost = 0;
        public int healthCost = 0;

        [Header("효과")]
        public float range = 5.0f;
        public float effectValue = 10.0f;  // 데미지, 힐량 등의 기본값
        public float duration = 0f;  // 버프/디버프 지속시간

        /// <summary>
        /// 어빌리티 사용 가능 여부 체크 (코스트 기준)
        /// </summary>
        public bool CanAfford(int currentMana, int currentStamina, int currentHealth)
        {
            // 코스트를 감당할 수 있는지 체크
            return currentMana >= manaCost &&
                   currentStamina >= staminaCost &&
                   currentHealth > healthCost;
        }

        /// <summary>
        /// 어빌리티 데이터 유효성 검증
        /// </summary>
        public bool ValidateData()
        {
            // 데이터 유효성 검증 로직
            return !string.IsNullOrEmpty(abilityId) &&
                   !string.IsNullOrEmpty(abilityName) &&
                   cooldownTime >= 0;
        }
    }
}