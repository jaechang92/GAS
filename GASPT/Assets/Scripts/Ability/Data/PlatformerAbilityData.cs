using UnityEngine;

namespace AbilitySystem.Platformer
{
    /// <summary>
    /// 플랫포머용 어빌리티 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "NewPlatformerAbility", menuName = "Ability/Platformer/AbilityData")]
    public class PlatformerAbilityData : ScriptableObject
    {
        [Header("기본 정보")]
        public string abilityId;
        public string abilityName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("어빌리티 타입")]
        public PlatformerAbilityType abilityType;
        public AttackDirection attackDirection;

        [Header("쿨다운 & 코스트")]
        public float cooldownTime = 1f;
        public int manaCost = 0;

        [Header("공격 설정")]
        public float damageMultiplier = 1f;
        public float range = 2f;
        public float knockbackPower = 0f;
        public bool canMoveWhileUsing = false;
        public bool cancelable = true;

        [Header("히트박스")]
        public Vector2 hitboxSize = Vector2.one;
        public Vector2 hitboxOffset = Vector2.zero;
        public float hitboxDuration = 0.2f;

        [Header("애니메이션")]
        public string animationTrigger;
        public float animationSpeed = 1f;

        [Header("이펙트")]
        public GameObject effectPrefab;
        public AudioClip soundEffect;

        [Header("특수 설정")]
        public bool isChargeSkill = false;     // 차징 스킬 여부
        public float maxChargeTime = 2f;       // 최대 차징 시간
        public bool isMultiHit = false;        // 다단히트 여부
        public int hitCount = 1;               // 히트 횟수

        /// <summary>
        /// 어빌리티 사용 가능 여부 체크 (코스트 기준)
        /// </summary>
        public bool CanAfford(int currentMana)
        {
            // 코스트를 감당할 수 있는지 체크
            return currentMana >= manaCost;
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